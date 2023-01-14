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
            // Special SBUC support
            On.RainWorld.Start += LateHooks;

            // Items + Creatures
            On.Menu.SandboxEditorSelector.ctor += ResetWidthAndHeight;
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

        private void LateHooks(On.RainWorld.orig_Start orig, RainWorld self)
        {
            IL.Menu.SandboxEditorSelector.ctor += il => AddCustomFisobs(il, false);
            On.Menu.SandboxSettingsInterface.ctor += AddPages;

            orig(self);
        }
    }
}
