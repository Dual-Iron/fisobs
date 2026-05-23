using System.Collections.Generic;
using ID = MultiplayerUnlocks.SandboxUnlockID;

namespace Fisobs.Sandbox;

/// <summary>
/// Represents a sandbox unlock.
/// </summary>
public sealed class SandboxUnlock
{
    /// <summary>
    /// The sandbox unlock type.
    /// </summary>
    public readonly ID Type;

    /// <summary>
    /// The sandbox's parent unlock. If the parent type's token has been collected in story mode, then this item will be unlocked.
    /// </summary>
    /// <remarks>If this is set to <see cref="ID.Slugcat"/>, the item is unconditionally unlocked.</remarks>
    public readonly ID? Parent;

    /// <summary>
    /// The sandbox unlock's data value. This takes the place of <see cref="Core.Icon.Data(AbstractPhysicalObject)"/> when spawning objects from sandbox mode.
    /// </summary>
    public readonly int Data;

    /// <summary>
    /// The creature unlock's default kill score. This is ignored for items.
    /// </summary>
    public readonly KillScore KillScore;

    /// <summary>
    /// A list of sandbox unlocks to attempt inserting after. Will go in order until it finds a match.
    /// </summary>
    public readonly List<ID> InsertAfter;

    /// <summary>
    /// Creates a new instance of the <see cref="SandboxUnlock"/> class.
    /// </summary>
    /// <param name="type">The sandbox unlock type.</param>
    /// <param name="parent">The sandbox's parent unlock. If the parent type's token has been collected in story mode, then this item will be unlocked. To unconditionally unlock this item, set <paramref name="parent"/> to <see cref="ID.Slugcat"/>.</param>
    /// <param name="data">The sandbox unlock's data value. This takes the place of <see cref="Core.Icon.Data(AbstractPhysicalObject)"/> when spawning objects from sandbox mode.</param>
    /// <param name="killScore">The creature unlock's kill score. This is ignored for items.</param>
    public SandboxUnlock(ID type, ID? parent, int data, KillScore killScore)
    {
        Type = type;
        Parent = parent;
        Data = data;
        KillScore = killScore;
        InsertAfter = new();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="SandboxUnlock"/> class.
    /// </summary>
    /// <param name="type">The sandbox unlock type.</param>
    /// <param name="parent">The sandbox's parent unlock. If the parent type's token has been collected in story mode, then this item will be unlocked. To unconditionally unlock this item, set <paramref name="parent"/> to <see cref="ID.Slugcat"/>.</param>
    /// <param name="data">The sandbox unlock's data value. This takes the place of <see cref="Core.Icon.Data(AbstractPhysicalObject)"/> when spawning objects from sandbox mode.</param>
    /// <param name="killScore">The creature unlock's kill score. This is ignored for items.</param>
    /// <param name="insertAfter">The sandbox unlock(s) to attempt to insert after, in order until it finds a match.</param>
    public SandboxUnlock(ID type, ID? parent, int data, KillScore killScore, List<ID> insertAfter)
    {
        Type = type;
        Parent = parent;
        Data = data;
        KillScore = killScore;
        InsertAfter = insertAfter;
    }
}
