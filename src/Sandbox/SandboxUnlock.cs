using ID = MultiplayerUnlocks.SandboxUnlockID;

namespace Fisobs.Sandbox
{
    /// <summary>
    /// Represents a sandbox unlock.
    /// </summary>
    public struct SandboxUnlock
    {
        internal readonly bool IsInitialized;

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
        /// The creature unlock's kill score. This is ignored for items.
        /// </summary>
        public readonly KillScore KillScore;

        /// <summary>
        /// Creates a new instance of the <see cref="SandboxUnlock"/> class.
        /// </summary>
        /// <param name="type">The sandbox unlock type.</param>
        /// <param name="parent">The sandbox's parent unlock. If the parent type's token has been collected in story mode, then this item will be unlocked. To unconditionally unlock this item, set <paramref name="parent"/> to <see cref="ID.Slugcat"/>.</param>
        /// <param name="data">The sandbox unlock's data value. This takes the place of <see cref="Core.Icon.Data(AbstractPhysicalObject)"/> when spawning objects from sandbox mode.</param>
        /// <param name="killScore">The creature unlock's kill score. This is ignored for items.</param>
        public SandboxUnlock(ID type, ID? parent, int data, KillScore killScore)
        {
            IsInitialized = true;
            Type = type;
            Parent = parent;
            Data = data;
            KillScore = killScore;
        }
    }
}
