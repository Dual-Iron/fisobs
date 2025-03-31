using Fisobs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CreatureType = CreatureTemplate.Type;

namespace Fisobs.Creatures;

/// <summary>
/// A registry that stores <see cref="Critob"/> instances and the hooks relevant to them.
/// </summary>
public sealed class CritobRegistry : Registry
{
    bool init;

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
        _ = GhostWorldPresence.GhostID.CC; // prevents a crash in ExpeditionTools.cctor() by executing ExtEnum<GhostID>.cctor()

        On.StaticWorld.InitCustomTemplates += AddTemplates;
        On.StaticWorld.InitStaticWorld += AddRelationships;

        On.Expedition.ChallengeTools.CreatureName += ChallengeTools_CreatureName;
        On.Expedition.ChallengeTools.GenerateCreatureScores += ChallengeTools_GenerateCreatureScores;

        On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        On.Player.CanEatMeat += Player_CanEatMeat;
        On.Player.Grabbed += PlayerGrabbed;
        On.AbstractCreature.Realize += Realize;
        On.AbstractCreature.InitiateAI += InitiateAI;
        On.AbstractCreature.ctor += Ctor;
        On.CreatureSymbol.DoesCreatureEarnATrophy += KillsMatter;
        On.MultiplayerUnlocks.FallBackCrit += ArenaFallback;
        On.RoomRealizer.GetCreaturePerformanceEstimation += RoomRealizer_GetCreaturePerformanceEstimation;
        On.ShelterDoor.IsThisHostileCreatureForShelter += ShelterDoor_IsThisHostileCreatureForShelter;
        On.ShelterDoor.IsThisBigCreatureForShelter += ShelterDoor_IsThisBigCreatureForShelter;

        On.WorldLoader.CreatureTypeFromString += WorldLoader_CreatureTypeFromString;
        On.DevInterface.RoomAttractivenessPanel.ctor += RoomAttractivenessPanel_ctor;
        On.DevInterface.MapPage.CreatureVis.CritString += CreatureVis_CritString;
        On.DevInterface.MapPage.CreatureVis.CritCol += CreatureVis_CritCol;

        On.AImap.IsConnectionForceAllowedForCreature += AImap_IsConnectionForceAllowedForCreature;
        On.AImap.IsTooCloseToTerrain += AImap_IsTooCloseToTerrain;
        On.ArenaBehaviors.SandboxEditor.StayOutOfTerrainIcon.AllowedTile += StayOutOfTerrainIcon_AllowedTile;

        On.CreatureSymbol.SymbolDataFromCreature += CreatureSymbol_SymbolDataFromCreature;
        On.CreatureSymbol.ColorOfCreature += CreatureSymbol_ColorOfCreature;
        On.CreatureSymbol.SpriteNameOfCreature += CreatureSymbol_SpriteNameOfCreature;
    }

    private void AddTemplates(On.StaticWorld.orig_InitCustomTemplates orig)
    {
        orig();

        // Add custom templates
        // No need to resize CreatureTemplate.relationships or StaticWorld.creatureTemplates, because they use CreatureTemplate.Type.values.Count for length
        foreach (Critob critob in critobs.Values) {
            var template = critob.CreateTemplate() ?? throw new InvalidOperationException($"Critob \"{critob.Type}\" returned null in GetTemplate().");
            if (template.type != critob.Type || template.type.Index == -1) {
                throw new InvalidOperationException($"Critob \"{critob.Type}\" returned a template with an incorrect `type` field.");
            }
            StaticWorld.creatureTemplates[template.type.Index] = template;
        }
    }

    private void AddRelationships(On.StaticWorld.orig_InitStaticWorld orig)
    {
        orig();

        foreach (var critob in critobs.Values) {
            critob.EstablishRelationships();
        }
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (!init) {
            init = true;
            foreach (var common in critobs.Values) {
                common.LoadResources(self);
            }
        }
    }

    private void ChallengeTools_CreatureName(On.Expedition.ChallengeTools.orig_CreatureName orig, ref string[] creatureNames)
    {
        orig(ref creatureNames);
        foreach (var critob in critobs) {
            creatureNames[(int)critob.Key] = critob.Value.CreatureName;
        }
    }

    private void ChallengeTools_GenerateCreatureScores(On.Expedition.ChallengeTools.orig_GenerateCreatureScores orig, ref Dictionary<string, int> dict)
    {
        orig(ref dict);
        foreach (var critob in critobs) {
            dict[critob.Value.Type.value] = critob.Value.ExpeditionScore();
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
                UnityEngine.Debug.LogError($"{crit.GetType()}::GetRealizedAI returned a non-null object but template.AI was false!");
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
                UnityEngine.Debug.LogError($"{critob.GetType()}::GetAbstractAI returned a non-null object but template.AI was false!");
            }

            // Arbitrary setup
            critob.Init(self, world, pos, id);
        }
    }

    private bool KillsMatter(On.CreatureSymbol.orig_DoesCreatureEarnATrophy orig, CreatureType creature)
    {
        var ret = orig(creature);
        if (critobs.TryGetValue(StaticWorld.GetCreatureTemplate(creature).type, out var critob)) {
            critob.KillsMatter(ref ret);
        }
        return ret;
    }

    private CreatureType? ArenaFallback(On.MultiplayerUnlocks.orig_FallBackCrit orig, CreatureType type)
    {
        if (critobs.TryGetValue(type, out var critob)) {
            return critob.ArenaFallback();
        }
        return orig(type);
    }

    private float RoomRealizer_GetCreaturePerformanceEstimation(On.RoomRealizer.orig_GetCreaturePerformanceEstimation orig, AbstractCreature crit)
    {
        if (critobs.TryGetValue(crit.creatureTemplate.type, out var critob)) {
            return critob.LoadedPerformanceCost;
        }
        return orig(crit);
    }

    private bool ShelterDoor_IsThisHostileCreatureForShelter(On.ShelterDoor.orig_IsThisHostileCreatureForShelter orig, AbstractCreature creature)
    {
        if (critobs.TryGetValue(creature.creatureTemplate.type, out var critob)) {
            return critob.ShelterDanger != ShelterDanger.Safe;
        }
        return orig(creature);
    }

    private bool ShelterDoor_IsThisBigCreatureForShelter(On.ShelterDoor.orig_IsThisBigCreatureForShelter orig, AbstractCreature creature)
    {
        if (critobs.TryGetValue(creature.creatureTemplate.type, out var critob)) {
            return critob.ShelterDanger == ShelterDanger.TooLarge;
        }
        return orig(creature);
    }

    private CreatureType WorldLoader_CreatureTypeFromString(On.WorldLoader.orig_CreatureTypeFromString orig, string s)
    {
        string name = s.ToLowerInvariant().Trim();
        foreach (var critob in critobs.Values) {
            var aliases = critob.WorldFileAliases();
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

    private void RoomAttractivenessPanel_ctor(On.DevInterface.RoomAttractivenessPanel.orig_ctor orig, DevInterface.RoomAttractivenessPanel self, DevInterface.DevUI owner, World world, string IDstring, DevInterface.DevUINode parentNode, Vector2 pos, string title, DevInterface.MapPage mapPage)
    {
        pos.x -= 120f;

        orig(self, owner, world, IDstring, parentNode, pos, title, mapPage);

        foreach (var critob in critobs.Values) {
            var cats = critob.DevtoolsRoomAttraction();
            if (cats == null) {
                continue;
            }
            foreach (var cat in cats) {
                ref var templateIndices = ref self.categories[(int)cat];

                Array.Resize(ref templateIndices, templateIndices.Length + 1);

                templateIndices[templateIndices.Length - 1] = StaticWorld.GetCreatureTemplate(critob.Type).index;
            }
        }

        // Traverse from last to first and quit early when we're past the Category nodes.
        int y = 31;
        int x = 2;
        foreach (var creatureButton in self.subNodes.OfType<DevInterface.RoomAttractivenessPanel.CreatureButton>().Reverse()) {
            if (!creatureButton.Category) {
                break;
            }
            creatureButton.pos = new(5f + 120f * x, 680f - 20f * y);
            y -= 1;
        }

        self.size.x += 120f;
        self.Refresh();
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

    private bool AImap_IsConnectionForceAllowedForCreature(On.AImap.orig_IsConnectionForceAllowedForCreature orig, AImap self, MovementConnection connection, CreatureTemplate crit, out bool forceAllow)
    {
        bool? ret = orig(self, connection, crit, out forceAllow) ? forceAllow : null;
        if (critobs.TryGetValue(crit.type, out var critob)) {
            critob.ConnectionIsAllowed(self, connection, ref ret);
        }
        if (ret.HasValue) {
            forceAllow = ret.Value;
            return true;
        }
        return false;
    }

    private bool AImap_IsTooCloseToTerrain(On.AImap.orig_IsTooCloseToTerrain orig, AImap self, RWCustom.IntVector2 pos, CreatureTemplate crit, out bool result)
    {
        bool? ret = orig(self, pos, crit, out result) ? result : null;
        if (critobs.TryGetValue(crit.type, out var critob)) {
            critob.TileIsAllowed(self, pos, ref ret);
        }
        if (ret.HasValue) {
            result = ret.Value;
            return true;
        }
        return false;
    }

    private bool StayOutOfTerrainIcon_AllowedTile(On.ArenaBehaviors.SandboxEditor.StayOutOfTerrainIcon.orig_AllowedTile orig, ArenaBehaviors.SandboxEditor.StayOutOfTerrainIcon self, Vector2 tst)
    {
        bool? ret = null;
        if (critobs.TryGetValue(self.iconData.critType, out var critob) && self.room?.aimap != null && self.room.readyForAI) {
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
