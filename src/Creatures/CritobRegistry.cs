using Fisobs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CreatureType = CreatureTemplate.Type;
using static StaticWorld;

namespace Fisobs.Creatures
{
    /// <summary>
    /// A registry that stores <see cref="Critob"/> instances and the hooks relevant to them.
    /// </summary>
    public sealed class CritobRegistry : Registry
    {
        private static CreatureTemplate? dummy;

        /// <summary>
        /// The singleton instance of this class.
        /// </summary>
        public static CritobRegistry Instance { get; } = new CritobRegistry();

        readonly Dictionary<CreatureType, Critob> critobs = new();

        /// <inheritdoc/>
        protected override void Process(IContent entry)
        {
            if (entry is Critob critob) {
                critobs[critob.Type] = critob;
            }
        }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            // TODO see RoomRealizer.RoomPerformanceEstimation and iterate to get `critobs[n].PerformanceCost`

            On.StaticWorld.InitStaticWorld += StaticWorld_InitStaticWorld;
            On.RainWorld.LoadResources += LoadResources;

            On.Player.CanEatMeat += Player_CanEatMeat;
            On.Player.Grabbed += PlayerGrabbed;
            On.AbstractCreature.Realize += Realize;
            On.AbstractCreature.InitiateAI += InitiateAI;
            On.AbstractCreature.ctor += Ctor;
            On.CreatureSymbol.DoesCreatureEarnATrophy += KillsMatter;
            On.MultiplayerUnlocks.FallBackCrit += ArenaFallback;
            On.WorldLoader.CreatureTypeFromString += WorldLoader_CreatureTypeFromString;

            On.DevInterface.MapPage.CreatureVis.CritString += CreatureVis_CritString;
            On.DevInterface.MapPage.CreatureVis.CritCol += CreatureVis_CritCol;

            // TODO see AImap.TileAccessibleToCreature(IntVector2 pos, CreatureTemplate crit) and hook it using Critob.TileIsAllowed
            // TODO see AImap.IsConnectionAllowedForCreature and hook it using Critob.ConnectionIsAllowed
            On.ArenaBehaviors.SandboxEditor.StayOutOfTerrainIcon.AllowedTile += StayOutOfTerrainIcon_AllowedTile;

            On.CreatureSymbol.SymbolDataFromCreature += CreatureSymbol_SymbolDataFromCreature;
            On.CreatureSymbol.ColorOfCreature += CreatureSymbol_ColorOfCreature;
            On.CreatureSymbol.SpriteNameOfCreature += CreatureSymbol_SpriteNameOfCreature;
        }

        private void StaticWorld_InitStaticWorld(On.StaticWorld.orig_InitStaticWorld orig)
        {
            // Adjust critobs' enum indices
            foreach (var critob in critobs.Values) {
                critob.Type.Unregister();
            }
            foreach (var critob in critobs.Values) {
                CreatureType.values.AddEntry(critob.Type.value);
            }

            orig();

            try {
                ApplyCritobs();
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError($"An exception was thrown in {nameof(Fisobs)}.{nameof(Creatures)}::{nameof(ApplyCritobs)} with details logged.");
                throw;
            }
        }

        private void ApplyCritobs()
        {
            var newTemplates = new List<CreatureTemplate>();

            // --- Generate new critob templates ---

            foreach (Critob critob in critobs.Values) {
                var template = critob.CreateTemplate() ?? throw new InvalidOperationException($"Critob \"{critob.Type}\" returned null in GetTemplate().");
                if (template.type != critob.Type) {
                    throw new InvalidOperationException($"Critob \"{critob.Type}\" returned a template with an incorrect `type` field.");
                }
                newTemplates.Add(template);
            }

            // --- Add new critob templates ---

            // Allocate space for the new templates
            int maxType = newTemplates.Max(t => (int)t.type);
            int prebakedIndex = preBakedPathingCreatures.Length;
            int quantifyIndex = quantifiedCreatures.Length;

            Array.Resize(ref preBakedPathingCreatures, preBakedPathingCreatures.Length + newTemplates.Count(t => t.doPreBakedPathing));
            Array.Resize(ref quantifiedCreatures, quantifiedCreatures.Length + newTemplates.Count(t => t.quantified));

            if (creatureTemplates.Length < maxType + 1) {
                int oldLen = creatureTemplates.Length;

                Array.Resize(ref creatureTemplates, maxType + 1);

                for (int i = oldLen; i < maxType + 1; i++) {
                    dummy ??= new(new CreatureType($"Unknown", register: false), null, new(), new(), default) {
                        name = "Unregistered HyperCam 2"
                    };

                    creatureTemplates[i] = dummy;
                }
            }

            // Add the templates to their respective arrays in StaticWorld
            foreach (CreatureTemplate newTemplate in newTemplates) {
                if ((int)newTemplate.type >= creatureTemplates.Length) {
                    throw new InvalidOperationException(
                        $"The CreatureTemplate.Type value {newTemplate.type} ({(int)newTemplate.type}) must be less than StaticWorld.creatureTemplates.Length ({creatureTemplates.Length}).");
                }

                // Add to StaticWorld collections
                creatureTemplates[newTemplate.index = (int)newTemplate.type] = newTemplate;

                if (newTemplate.doPreBakedPathing) {
                    preBakedPathingCreatures[newTemplate.PreBakedPathingIndex = prebakedIndex] = newTemplate;
                    prebakedIndex += 1;
                }

                if (newTemplate.quantified) {
                    quantifiedCreatures[newTemplate.quantifiedIndex = quantifyIndex] = newTemplate;
                    quantifyIndex += 1;
                }
            }

            // --- Update creature-creature relationships ---

            // Update existing vanilla relationships
            foreach (CreatureTemplate template in creatureTemplates) {
                int oldRelationshipsLength = template.relationships.Length;
                Array.Resize(ref template.relationships, creatureTemplates.Length);
                for (int i = oldRelationshipsLength; i < creatureTemplates.Length; i++) {
                    template.relationships[i] = template.relationships[0];
                }
            }

            // Establish specific relationships
            foreach (Critob critob in critobs.Values) {
                critob.EstablishRelationships();
            }
        }

        private void LoadResources(On.RainWorld.orig_LoadResources orig, RainWorld self)
        {
            orig(self);

            foreach (var common in critobs.Values) {
                common.LoadResources(self);
            }
        }

        private bool Player_CanEatMeat(On.Player.orig_CanEatMeat orig, Player self, Creature crit)
        {
            bool ret = orig(self, crit);
            if (critobs.TryGetValue(crit.abstractCreature.creatureTemplate.type, out var critob)) {
                critob.CorpseIsEdible(self, crit, ref ret);
            }
            return ret;
        }

        private void PlayerGrabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp g)
        {
            orig(self, g);

            bool paralyzing = self.dangerGrasp != null;

            if (g?.grabber?.abstractCreature != null && critobs.TryGetValue(g.grabber.abstractCreature.creatureTemplate.type, out var critob)) {
                critob.GraspParalyzesPlayer(g, ref paralyzing);
            }

            if (paralyzing) {
                self.dangerGraspTime = 0;
                self.dangerGrasp = g;
            } else {
                self.dangerGrasp = null;
            }
        }

        private void InitiateAI(On.AbstractCreature.orig_InitiateAI orig, AbstractCreature self)
        {
            orig(self);

            if (critobs.TryGetValue(self.creatureTemplate.type, out var crit)) {
                if (self.abstractAI != null && self.creatureTemplate.AI) {
                    self.abstractAI.RealAI = crit.CreateRealizedAI(self) ?? throw new InvalidOperationException($"{crit.GetType()}::GetRealizedAI returned null but template.AI was true!");
                } else if (!self.creatureTemplate.AI && crit.CreateRealizedAI(self) != null) {
                    Debug.LogError($"{crit.GetType()}::GetRealizedAI returned a non-null object but template.AI was false!");
                }
            }
        }

        private void Realize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
        {
            if (self.realizedCreature == null && critobs.TryGetValue(self.creatureTemplate.type, out var crit)) {
                self.realizedObject = crit.CreateRealizedCreature(self) ?? throw new InvalidOperationException($"{crit.GetType()}::GetRealizedCreature returned null!");

                self.InitiateAI();

                foreach (var stuck in self.stuckObjects) {
                    if (stuck.A.realizedObject == null) {
                        stuck.A.Realize();
                    }
                    if (stuck.B.realizedObject == null) {
                        stuck.B.Realize();
                    }
                }
            }

            orig(self);
        }

        private void Ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate template, Creature real, WorldCoordinate pos, EntityID id)
        {
            orig(self, world, template, real, pos, id);

            if (critobs.TryGetValue(template.type, out var critob)) {
                // Set creature state
                self.state = critob.CreateState(self);

                // Set creature AI
                AbstractCreatureAI? abstractAI = critob.CreateAbstractAI(self);

                if (template.AI && abstractAI != null) {
                    self.abstractAI = abstractAI;

                    bool setDenPos = pos.abstractNode > -1 && pos.abstractNode < self.Room.nodes.Length
                        && self.Room.nodes[pos.abstractNode].type == AbstractRoomNode.Type.Den && !pos.TileDefined;

                    if (setDenPos) {
                        self.abstractAI.denPosition = pos;
                    }
                } else if (abstractAI != null) {
                    Debug.LogError($"{critob.GetType()}::GetAbstractAI returned a non-null object but template.AI was false!");
                }

                // Arbitrary setup
                critob.Init(self, world, pos, id);
            }
        }

        private bool KillsMatter(On.CreatureSymbol.orig_DoesCreatureEarnATrophy orig, CreatureType creature)
        {
            var ret = orig(creature);
            if (critobs.TryGetValue(GetCreatureTemplate(creature).type, out var critob)) {
                critob.KillsMatter(ref ret);
            }
            return ret;
        }

        private CreatureType? ArenaFallback(On.MultiplayerUnlocks.orig_FallBackCrit orig, CreatureType type)
        {
            if (critobs.TryGetValue(GetCreatureTemplate(type).type, out var critob)) {
                return critob.ArenaFallback();
            }
            return orig(type);
        }

        private CreatureType WorldLoader_CreatureTypeFromString(On.WorldLoader.orig_CreatureTypeFromString orig, string s)
        {
            string name = s.ToLowerInvariant().Trim();
            foreach (var critob in critobs.Values) {
                var aliases = critob.Aliases();
                if (aliases == null) {
                    continue;
                }
                foreach (string alias in aliases) {
                    if (name == alias.ToLowerInvariant().Trim()) {
                        return critob.Type;
                    }
                }
            }
            return orig(s);
        }

        private string CreatureVis_CritString(On.DevInterface.MapPage.CreatureVis.orig_CritString orig, AbstractCreature crit)
        {
            if (critobs.TryGetValue(crit.creatureTemplate.type, out var critob)) {
                return critob.DevtoolsMapName(crit);
            }
            return orig(crit);
        }

        private Color CreatureVis_CritCol(On.DevInterface.MapPage.CreatureVis.orig_CritCol orig, AbstractCreature crit)
        {
            if (critobs.TryGetValue(crit.creatureTemplate.type, out var critob)) {
                if (crit.InDen && UnityEngine.Random.value < 0.5f) {
                    return new(0.5f, 0.5f, 0.5f);
                }
                return critob.DevtoolsMapColor(crit);
            }
            return orig(crit);
        }

        private bool StayOutOfTerrainIcon_AllowedTile(On.ArenaBehaviors.SandboxEditor.StayOutOfTerrainIcon.orig_AllowedTile orig, ArenaBehaviors.SandboxEditor.StayOutOfTerrainIcon self, Vector2 tst)
        {
            bool? ret = null;
            if (critobs.TryGetValue(self.iconData.critType, out var critob)) {
                critob.TileIsAllowed(self.room.aimap, self.room.GetTilePosition(tst), ref ret);
            }
            return ret ?? orig(self, tst);
        }

        private IconSymbol.IconSymbolData CreatureSymbol_SymbolDataFromCreature(On.CreatureSymbol.orig_SymbolDataFromCreature orig, AbstractCreature creature)
        {
            if (critobs.TryGetValue(creature.creatureTemplate.type, out var critob)) {
                return new IconSymbol.IconSymbolData(creature.creatureTemplate.type, AbstractPhysicalObject.AbstractObjectType.Creature, critob.Icon.Data(creature));
            }
            return orig(creature);
        }

        private Color CreatureSymbol_ColorOfCreature(On.CreatureSymbol.orig_ColorOfCreature orig, IconSymbol.IconSymbolData iconData)
        {
            if (critobs.TryGetValue(iconData.critType, out var critob)) {
                return critob.Icon.SpriteColor(iconData.intData);
            }
            return orig(iconData);
        }

        private string CreatureSymbol_SpriteNameOfCreature(On.CreatureSymbol.orig_SpriteNameOfCreature orig, IconSymbol.IconSymbolData iconData)
        {
            if (critobs.TryGetValue(iconData.critType, out var critob)) {
                return critob.Icon.SpriteName(iconData.intData);
            }
            return orig(iconData);
        }
    }
}
