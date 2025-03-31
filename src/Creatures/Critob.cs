using Fisobs.Core;
using Fisobs.Properties;
using Fisobs.Sandbox;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using CreatureType = CreatureTemplate.Type;

namespace Fisobs.Creatures;

/// <summary>
/// Represents the "metadata" for a custom creature.
/// </summary>
public abstract class Critob : IContent, IPropertyHandler, ISandboxHandler
{
    private readonly List<SandboxUnlock> sandboxUnlocks = new();

    /// <summary>
    /// Creates a new <see cref="Critob"/> instance for the given <paramref name="type"/>.
    /// </summary>
    protected Critob(CreatureType type)
    {
        if ((int)type <= 0) {
            ArgumentException e = new($"The {GetType().Name} fisob's enum value ({(int)type}) was invalid.", nameof(type));
            Debug.LogException(e);
            Console.WriteLine(e);
            throw e;
        }

        Type = type;
        CreatureName = Regex.Replace(type.ToString(), "([a-z])([A-Z])", "$1 $2");
    }

    /// <summary>The critob's type.</summary>
    public CreatureType Type { get; }
    /// <summary>The critob's icon; a <see cref="DefaultIcon"/> by default.</summary>
    /// <remarks>When <see cref="LoadResources(RainWorld)"/> is called, an embedded resource with the name <c>$"icon_{Type}"</c> will be auto-loaded as a <see cref="SimpleIcon"/>, if it exists.</remarks>
    public Icon Icon { get; set; } = new DefaultIcon();
    /// <summary>The performance cost associated with the creature while it's loaded.</summary>
    /// <value>A value of 10 is the default, and the minimum. Scavengers and leviathans use 300, lizards use 50, and batflies use 10.</value>
    public float LoadedPerformanceCost { get; set; } = 10f;
    /// <inheritdoc/>
    public SandboxPerformanceCost SandboxPerformanceCost { get; set; } = new(0.2f, 0.0f);
    /// <summary>How much danger the creature poses to a player that shares a shelter with it.</summary>
    public ShelterDanger ShelterDanger { get; set; }
    /// <summary>The creature's user-facing name. Used in debug logs, <see cref="CreatureFormula"/>, and various Expedition features.</summary>
    /// <remarks>Defaults to the creature's type with spaces inserted after each lowercase letter that appears before a capital letter. For example, "BouncingBall" becomes "Bouncing Ball".</remarks>
    public string CreatureName { get; set; }

    /// <summary>Gets a new instance of <see cref="CreatureState"/> for <paramref name="acrit"/>. If spawned by a sandbox unlock, the <c>SandboxData</c> section of the creature's state will equal that unlock's <see cref="SandboxUnlock.Data"/> value.</summary>
    /// <remarks>By default, this returns a <see cref="HealthState"/>.</remarks>
    public virtual CreatureState CreateState(AbstractCreature acrit) => new HealthState(acrit);
    /// <summary>Gets a new instance of <see cref="AbstractCreatureAI"/> (or <see langword="null"/>) from an abstract creature.</summary>
    /// <remarks>If <see cref="CreatureTemplate.AI"/> is true for <paramref name="acrit"/>, then null will default to a simple <see cref="AbstractCreatureAI"/>. If false, then this method is not called in the first place.</remarks>
    public virtual AbstractCreatureAI? CreateAbstractAI(AbstractCreature acrit) => null;
    /// <summary>Perform arbitrary work after the <see cref="AbstractCreature(World, CreatureTemplate, Creature, WorldCoordinate, EntityID)"/> constructor runs.</summary>
    public virtual void Init(AbstractCreature acrit, World world, WorldCoordinate pos, EntityID id) { }
    /// <summary>Determines if being grasped by this creature should paralyze the player.</summary>
    public virtual void GraspParalyzesPlayer(Creature.Grasp grasp, ref bool paralyzing) { }
    /// <summary>Determines if a creature's corpse is edible by the given player.</summary>
    public virtual void CorpseIsEdible(Player player, Creature crit, ref bool canEatMeat) { }
    /// <summary>Determines if the creature should be displayed when listing kills in arena and hunter modes.</summary>
    public virtual void KillsMatter(ref bool killsMatter) { }
    /// <summary>If this creature would be spawned in arena mode but isn't unlocked yet, the creature returned by this method is spawned instead.</summary>
    /// <remarks>By default, this returns the creature's ancestor, or <see langword="null"/> if it has none.</remarks>
    /// <returns>The creature type to spawn, or <see langword="null"/> to spawn nothing.</returns>
    public virtual CreatureType? ArenaFallback() => StaticWorld.GetCreatureTemplate(Type).ancestor?.type;
    /// <summary>The creature's score when killed in Expedition mode.</summary>
    /// <remarks>By default, this returns a relevant sandbox unlock score, or 0 if that's not applicable.</remarks>
    public virtual int ExpeditionScore()
    {
        if (sandboxUnlocks.Count == 0) {
            return 0;
        }
        if (sandboxUnlocks.FirstOrDefault(s => s.Type.value == Type.value) is SandboxUnlock unlock) {
            return unlock.KillScore.Value;
        }
        return sandboxUnlocks[0].KillScore.Value;
    }
    /// <summary>Gets the custom properties of a creature.</summary>
    /// <returns>An instance of <see cref="ItemProperties"/> or null.</returns>
    public virtual ItemProperties? Properties(Creature crit) => null;

    /// <summary>Extra names used for this creature in world files. Case-insensitive.</summary>
    /// <remarks>By default, this returns only <see cref="CreatureName"/> with all spaces removed.</remarks>
    /// <returns>An assortment of aliases. For example, DaddyLongLegs can also be called Daddy.</returns>
    public virtual IEnumerable<string> WorldFileAliases() => new string[] { CreatureName.Replace(" ", "").ToLowerInvariant() };
    /// <summary>What categories of room attraction this creature falls under.</summary>
    /// <returns>An assortment of categories. For example, vultures classify as Flying and LikesOutside.</returns>
    public virtual IEnumerable<DevInterface.RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => Array.Empty<DevInterface.RoomAttractivenessPanel.Category>();
    /// <summary>The name that shows for this creature in the devtools map.</summary>
    /// <remarks>By default, this returns up to the first three characters from the creature's type.</remarks>
    /// <returns>An abbreviated name that should be a few characters long.</returns>
    public virtual string DevtoolsMapName(AbstractCreature acrit) => Type.ToString().Substring(0, Math.Min(3, Type.ToString().Length));
    /// <summary>The color that shows for this creature in the devtools map.</summary>
    /// <remarks>By default, this returns the creature's icon color.</remarks>
    /// <returns>A color that should help in identifying the creature.</returns>
    public virtual Color DevtoolsMapColor(AbstractCreature acrit) => Icon.SpriteColor(Icon.Data(acrit));

    /// <summary>Modifies what movements creatures are allowed to make. Setting <paramref name="allow"/> to a non-null value will forcefully allow/disallow the connection.</summary>
    public virtual void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allow) { }
    /// <summary>Modifies what tiles creatures are allowed to move into. Setting <paramref name="allow"/> to a non-null value will forcefully allow/disallow the tile.</summary>
    public virtual void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow) { }

    /// <summary>Gets a new instance of <see cref="ArtificialIntelligence"/> (or <see langword="null"/>) from an abstract creature.</summary>
    /// <remarks>If <see cref="CreatureTemplate.AI"/> is true for <paramref name="acrit"/>, then this must return a non-null object.</remarks>
    public abstract ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit);
    /// <summary>Gets a new instance of <see cref="Creature"/> from an abstract creature.</summary>
    public abstract Creature CreateRealizedCreature(AbstractCreature acrit);
    /// <summary>Establishes creature templates for this critob. The <see cref="CreatureFormula"/> type is recommended for this.</summary>
    public abstract CreatureTemplate CreateTemplate();
    /// <summary>Establishes relationships between creatures. The <see cref="Relationships"/> type is recommended for this.</summary>
    public abstract void EstablishRelationships();

    /// <summary>
    /// Used to load <see cref="FAtlas"/> and <see cref="FAtlasElement"/> sprites. Called once.
    /// </summary>
    /// <param name="rainWorld">The current <see cref="RainWorld"/> instance.</param>
    public virtual void LoadResources(RainWorld rainWorld)
    {
        string iconName = Ext.IconAtlasName(Type.value);

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

    ItemProperties? IPropertyHandler.Properties(PhysicalObject forObject)
    {
        return forObject is Creature crit ? Properties(crit) : null;
    }

    AbstractWorldEntity ISandboxHandler.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
    {
        string stateString = $"{data.CustomData}SandboxData<cC>{unlock.Data}<cB>";
        AbstractCreature crit = new(world, StaticWorld.GetCreatureTemplate(data.Type.CritType), null, data.Pos, data.ID) {
            pos = data.Pos
        };
        crit.state.LoadFromString(stateString.Split(new string[] { "<cB>" }, StringSplitOptions.RemoveEmptyEntries));
        //crit.setCustomFlags(); // theres some sort of problem with this
        return crit;
    }
}
