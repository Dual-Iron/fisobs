using Fisobs.Core;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using SandboxUnlockCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
            // SBUC needs special support for UI
            try {
                HookSbuc();
            } catch (FileNotFoundException) {
                IL.Menu.SandboxEditorSelector.ctor += il => AddCustomFisobs(il, false);
                On.Menu.SandboxSettingsInterface.ctor += AddPages;
            }

            orig(self);
        }

        private void HookSbuc()
        {
            var addIcon = typeof(Main).GetMethod("AddIcon", BindingFlags.Instance | BindingFlags.NonPublic);
            var addKills = typeof(Main).GetMethod("AddKills", BindingFlags.Instance | BindingFlags.NonPublic);
            var squishPlayerSelect = typeof(Main).GetMethod("SquishPlayerSelect", BindingFlags.Instance | BindingFlags.NonPublic);

            if (addIcon != null && addKills != null && squishPlayerSelect != null) {
                new ILHook(addIcon, il => AddCustomFisobs(il, true));
                new ILHook(addKills, AddKillsHook);
                new ILHook(squishPlayerSelect, SquishPlayerSelectHook);
            }

            static void SquishPlayerSelectHook(ILContext il)
            {
                // BANED
                il.Instrs.Insert(0, Instruction.Create(OpCodes.Ret));
            }

            static void AddKillsHook(ILContext il)
            {
                ILCursor cursor = new(il);

                // Run AddPagesImpl just before returning
                cursor.Index = cursor.Instrs.Count - 1;
                cursor.Emit(OpCodes.Ldarg_2);
                cursor.Emit(OpCodes.Ldarg_3);
                cursor.EmitDelegate<Action<SandboxSettingsInterface, Menu.Menu>>((s, m) => Instance.AddPagesImpl(s, m));
            }
        }
    }
}
