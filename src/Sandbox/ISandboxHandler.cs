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
        /// The sandbox unlocks associated with the physob.
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
