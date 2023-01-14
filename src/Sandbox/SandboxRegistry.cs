using Fisobs.Core;
using System.Collections.Generic;

namespace Fisobs.Sandbox
{
    /// <summary>
    /// A registry that stores <see cref="ISandboxHandler"/> instances and the hooks relevant to them.
    /// </summary>
    public sealed partial class SandboxRegistry : Registry
    {
        /// <summary>
        /// The singleton instance of this class.
        /// </summary>
        public static SandboxRegistry Instance { get; } = new SandboxRegistry();

        readonly Dictionary<PhysobType, ISandboxHandler> sboxes = new();

        /// <inheritdoc/>
        protected override void Process(IContent content)
        {
            if (content is ISandboxHandler handler) {
                sboxes[handler.Type] = handler;
            }
        }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            // Editor UI
            IL.Menu.SandboxEditorSelector.ctor += AddCustomFisobs;
            On.Menu.SandboxSettingsInterface.ctor += AddPages;
            On.Menu.SandboxEditorSelector.ctor += ResetWidthAndHeight;

            // Items + Creatures
            On.SandboxGameSession.SpawnEntity += SpawnEntity;
            On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += FromUnlock;
            On.MultiplayerUnlocks.SandboxUnlockForSymbolData += FromSymbolData;
            On.MultiplayerUnlocks.ParentSandboxID += GetParent;
            On.MultiplayerUnlocks.TiedSandboxIDs += TiedSandboxIDs;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SandboxUnlockID += GetCollected;
            On.PlayerProgression.MiscProgressionData.SetTokenCollected_SandboxUnlockID += SetCollected;

            // Creatures
            On.Menu.SandboxSettingsInterface.DefaultKillScores += DefaultKillScores;
        }
    }
}
