using Fisobs.Core;
using System.Collections.Generic;

namespace Fisobs.Sandbox
{
    /// <summary>
    /// Used by <see cref="SandboxRegistry"/> to manage a set of sandbox unlocks.
    /// </summary>
    public interface ISandboxHandler
    {
        /// <summary>
        /// The physob these unlocks spawn.
        /// </summary>
        PhysobType Type { get; }

        /// <summary>
        /// The performance cost associated with the entity in sandbox mode.
        /// </summary>
        /// <value>A value of 0.2 for linear and 0 for exponential is the default. Some examples:
        /// <list type="bullet">
        /// <item>Scavengers have 0.5 for linear and 0.925 for exponential.</item>
        /// <item>Leviathans have 4.0 for linear and 1.2 for exponential.</item>
        /// <item>Leeches have 0.3 for linear and 0.1 for exponential.</item>
        /// <item>Batflies have 0.5 for linear and 0.1 for exponential.</item>
        /// <item>Scavenger bombs have 1.2 for linear and 0 for exponential.</item>
        /// </list>
        /// </value>
        SandboxPerformanceCost SandboxPerformanceCost { get; }

        /// <summary>
        /// The sandbox unlocks associated with the entity.
        /// </summary>
        IList<SandboxUnlock> SandboxUnlocks { get; }

        /// <summary>
        /// Gets a new <see cref="AbstractWorldEntity"/> instance for when an entity is spawned in sandbox mode.
        /// </summary>
        /// <param name="world">The arena mode world.</param>
        /// <param name="data">The entity's data.</param>
        /// <param name="unlock">The unlock which spawned this entity.</param>
        AbstractWorldEntity ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock);
    }
}
