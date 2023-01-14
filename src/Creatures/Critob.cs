using Fisobs.Properties;
using Fisobs.Core;
using Fisobs.Sandbox;
using System;
using System.Collections.Generic;
using UnityEngine;
using CreatureType = CreatureTemplate.Type;

namespace Fisobs.Creatures
{
    /// <summary>Represents the "metadata" for a custom creature.</summary>
    public abstract class Critob2 : Critob
    {
        /// <summary>Creates a new <see cref="Critob"/> instance for the given <paramref name="type"/>.</summary>
        protected Critob2(CreatureType type) : base(type, internalOverload: true)
        { }

        /// <summary>Obsolete. Call <see cref="CreateTemplate"/> instead.</summary>
        public sealed override IEnumerable<CreatureTemplate> GetTemplates()
        {
            yield return CreateTemplate();
        }

        /// <summary>Establishes the creature template for this critob. The <see cref="CreatureFormula"/> type is recommended for this.</summary>
        public abstract CreatureTemplate CreateTemplate();
    }

    /// <summary>
    /// Represents the "metadata" for a custom creature.
    /// </summary>
    public abstract class Critob : IContent, IPropertyHandler, ISandboxHandler
    {
        private readonly List<SandboxUnlock> sandboxUnlocks = new();

#pragma warning disable IDE0060 // Remove unused parameter
        internal Critob(CreatureType type, bool internalOverload)
        {
            if (type == 0) {
                ArgumentException e = new($"The {GetType().Name} critob's enum value was zero. Did the developer forget to add a BepInDependency attribute to their plugin class?", nameof(type));
                Debug.LogException(e);
                Console.WriteLine(e);
                throw e;
            }

            Type = type;
        }
#pragma warning restore IDE0060 // Remove unused parameter

        /// <summary>
        /// Creates a new <see cref="Critob"/> instance for the given <paramref name="type"/>.
        /// </summary>
        [Obsolete("Do not extend Critob. Extend Critob2 instead.", error: false)]
        protected Critob(CreatureType type) : this(type, internalOverload: true)
        { }

        /// <summary>The critob's type.</summary>
        public CreatureType Type { get; }
        /// <summary>The critob's icon; a <see cref="DefaultIcon"/> by default.</summary>
        /// <remarks>When <see cref="LoadResources(RainWorld)"/> is called, an embedded resource with the name <c>$"icon_{Type}"</c> will be auto-loaded as a <see cref="SimpleIcon"/>, if it exists.</remarks>
        public Icon Icon { get; set; } = new DefaultIcon();

        /// <summary>Gets a new instance of <see cref="CreatureState"/> for <paramref name="acrit"/>. If spawned by a sandbox unlock, the <c>SandboxData</c> section of the creature's state will equal that unlock's <see cref="SandboxUnlock.Data"/> value.</summary>
        /// <remarks>The default implementation of this method returns a <see cref="HealthState"/>.</remarks>
        public virtual CreatureState GetState(AbstractCreature acrit) => new HealthState(acrit);
        /// <summary>Gets a new instance of <see cref="AbstractCreatureAI"/> (or <see langword="null"/>) from an abstract creature.</summary>
        /// <remarks>If <see cref="CreatureTemplate.AI"/> is true for <paramref name="acrit"/>, then null will default to a simple <see cref="AbstractCreatureAI"/>. If false, then this method is not called in the first place.</remarks>
        public virtual AbstractCreatureAI? GetAbstractAI(AbstractCreature acrit) => null;
        /// <summary>Perform arbitrary work after the <see cref="AbstractCreature(World, CreatureTemplate, Creature, WorldCoordinate, EntityID)"/> constructor runs.</summary>
        public virtual void Init(AbstractCreature acrit, World world, WorldCoordinate pos, EntityID id) { }
        /// <summary>Determines if being grasped by this creature should paralyze the player.</summary>
        public virtual bool GraspParalyzesPlayer(Creature.Grasp grasp) => false;
        /// <summary>Determines if <paramref name="type"/> should be displayed when listing kills in arena and hunter modes.</summary>
        public virtual void KillsMatter(CreatureType type, ref bool killsMatter) { }
        /// <summary>If this creature would be spawned in arena mode but isn't unlocked yet, the creature returned by this method is spawned instead.</summary>
        /// <returns>The creature to spawn, or <see langword="null"/> to spawn nothing.</returns>
        public virtual CreatureType? ArenaFallback(CreatureType type) => null;
        /// <inheritdoc/>
        public virtual ItemProperties? Properties(PhysicalObject forObject) => null;

        /// <summary>Gets a new instance of <see cref="ArtificialIntelligence"/> (or <see langword="null"/>) from an abstract creature.</summary>
        /// <remarks>If <see cref="CreatureTemplate.AI"/> is true for <paramref name="acrit"/>, then this must return a non-null object.</remarks>
        public abstract ArtificialIntelligence? GetRealizedAI(AbstractCreature acrit);
        /// <summary>Gets a new instance of <see cref="Creature"/> from an abstract creature.</summary>
        public abstract Creature GetRealizedCreature(AbstractCreature acrit);
        /// <summary>Establishes creature templates for this critob. The <see cref="CreatureFormula"/> type is recommended for this.</summary>
        public abstract IEnumerable<CreatureTemplate> GetTemplates();
        /// <summary>Establishes relationships between creatures. The <see cref="Relationships"/> type is recommended for this.</summary>
        public abstract void EstablishRelationships();

        /// <summary>
        /// Used to load <see cref="FAtlas"/> and <see cref="FAtlasElement"/> sprites. The <see cref="Ext.LoadAtlasFromEmbRes(System.Reflection.Assembly, string)"/> is recommended for this.
        /// </summary>
        /// <param name="rainWorld">The current <see cref="RainWorld"/> instance.</param>
        public virtual void LoadResources(RainWorld rainWorld)
        {
            string iconName = Ext.LoadAtlasFromEmbRes(GetType().Assembly, $"icon_{Type}")?.name ?? "Futile_White";

            if (Icon is DefaultIcon) {
                Icon = new SimpleIcon(iconName, Ext.MenuGrey);
            }
        }

        /// <summary>
        /// Registers a sandbox unlock under this critob.
        /// </summary>
        /// <param name="type">The sandbox unlock type.</param>
        /// <param name="parent">The sandbox's parent unlock. If the parent type's token has been collected in story mode, then this item will be unlocked. To unconditionally unlock this item, set <paramref name="parent"/> to <see cref="MultiplayerUnlocks.SandboxUnlockID.Slugcat"/>.</param>
        /// <param name="data">The sandbox unlock's data value. This takes the place of <see cref="Icon.Data(AbstractPhysicalObject)"/> when spawning objects from sandbox mode.</param>
        /// <param name="killScore">The creature unlock's kill score. This is ignored for items.</param>
        public void RegisterUnlock(KillScore killScore, MultiplayerUnlocks.SandboxUnlockID type, MultiplayerUnlocks.SandboxUnlockID? parent = null, int data = 0)
        {
            sandboxUnlocks.Add(new(type, parent, data, killScore));
        }

        PhysobType IPropertyHandler.Type => Type;
        PhysobType ISandboxHandler.Type => Type;

        IList<SandboxUnlock> ISandboxHandler.SandboxUnlocks => sandboxUnlocks;

        IEnumerable<Registry> IContent.Registries()
        {
            yield return CritobRegistry.Instance;
            yield return PropertyRegistry.Instance;
            yield return SandboxRegistry.Instance;
        }

        AbstractWorldEntity ISandboxHandler.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
        {
            string stateString = $"{data.CustomData}SandboxData<cC>{unlock.Data}<cB>";
            EntitySaveData withSandboxData = new(data.Type, data.ID, data.Pos, stateString);
            AbstractCreature crit = SaveState.AbstractCreatureFromString(world, withSandboxData.ToString(), false);
            crit.pos = data.Pos;
            return crit;
        }
    }
}
