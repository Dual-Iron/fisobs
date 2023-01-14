using Fisobs.Core;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Menu.SandboxEditorSelector;

namespace Fisobs.Sandbox
{
    public sealed partial class SandboxRegistry : Registry
    {
        private delegate void InsertSandboxDelegate(SandboxEditorSelector self, ref int counter);

        private void AddCustomFisobs(ILContext il, bool sbuc)
        {
            ILCursor cursor = new(il);

            try {
                OpCode load_self = sbuc ? OpCodes.Ldarg_2 : OpCodes.Ldarg_0;
                int arg_self = sbuc ? 2 : 0;
                int loc_creatureIter = sbuc ? 14 : 3;
                int loc_counter = sbuc ? 2 : 0;

                // Move before creatures are added. The const `0` is the initial iteration value, just before the loop starts
                cursor.GotoNext(MoveType.Before, i => i.MatchLdcI4(0) && i.Next.MatchStloc(loc_creatureIter));

                // Call `InsertPhysicalObjects` with `this` and `ref counter`
                cursor.Emit(load_self);
                cursor.Emit(OpCodes.Ldloca_S, il.Body.Variables[loc_counter]);
                cursor.EmitDelegate<InsertSandboxDelegate>(InsertPhysicalObjects);

                // Move after creatures are added, before play button is added. The const `1` is the ActionButton.Action.Play enum member.
                cursor.GotoNext(MoveType.Before, i => i.MatchLdarg(arg_self) && i.Next.MatchLdcI4(1));

                // Call `InsertCreatures` with `this` and `ref counter`
                cursor.Emit(load_self);
                cursor.Emit(OpCodes.Ldloca_S, il.Body.Variables[loc_counter]);
                cursor.EmitDelegate<InsertSandboxDelegate>(InsertCreatures);
            } catch (Exception e) {
                Debug.LogException(e);
                Console.WriteLine($"Couldn't register fisobs in \"{nameof(Fisobs)}\" because of exception in {nameof(AddCustomFisobs)}: {e.Message}");
            }
        }

        // Must be static to work around a weird Realm bug (see https://github.com/MonoMod/MonoMod/issues/85)
        private static void InsertPhysicalObjects(SandboxEditorSelector self, ref int counter) => Instance?.InsertEntries(self, ref counter, false);
        private static void InsertCreatures(SandboxEditorSelector self, ref int counter) => Instance?.InsertEntries(self, ref counter, true);

        private void InsertEntries(SandboxEditorSelector self, ref int counter, bool creatures)
        {
            IEnumerable<ISandboxHandler> selection = sboxes.Values.Where(c => c.Type.IsCrit == creatures);

            foreach (var common in selection) {
                foreach (var unlock in common.SandboxUnlocks) {
                    // Reserve slots for:
                    int padding = creatures
                        ? 8     // empty space (3) + randomize button (1) + config buttons (3) + play button (1)
                        : 51    // all of the above (8) + creature unlocks (43)
                        ;

                    if (counter >= Width * Height - padding) {
                        GrowEditorSelector(self);
                    }

                    Button button;
                    if (self.unlocks.SandboxItemUnlocked(unlock.Type)) {
                        button = new CreatureOrItemButton(self.menu, self, new(common.Type.CritType, common.Type.ObjectType, unlock.Data));
                    } else {
                        button = new LockedButton(self.menu, self);
                    }
                    self.AddButton(button, ref counter);
                }
            }
        }

        private static void GrowEditorSelector(SandboxEditorSelector self)
        {
            self.bkgRect.size.y += ButtonSize;
            self.size.y += ButtonSize;
            self.pos.y += ButtonSize;
            Height += 1;

            Button[,] newArr = new Button[Width, Height];

            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height - 1; j++) {
                    newArr[i, j + 1] = self.buttons[i, j];
                }
            }

            self.buttons = newArr;
        }

        private void ResetWidthAndHeight(On.Menu.SandboxEditorSelector.orig_ctor orig, SandboxEditorSelector self, Menu.Menu menu, Menu.MenuObject owner, SandboxOverlayOwner overlayOwner)
        {
            Width = 19;
            Height = 4;
            orig(self, menu, owner, overlayOwner);
        }
    }
}