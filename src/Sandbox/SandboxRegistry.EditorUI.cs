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

        private void AddCustomFisobs(ILContext il)
        {
            ILCursor cursor = new(il);

            try {
                // Move to the instruction after the end of the first finally block, to add behavior just after the 'add physical objects to menu' foreach loop.
                cursor.Prev = il.Body.ExceptionHandlers[0].HandlerEnd;

                // Call `InsertPhysicalObjects` with `this` and `ref counter`
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloca_S, il.Body.Variables[0]);
                cursor.EmitDelegate<InsertSandboxDelegate>(InsertPhysicalObjects);

                // Move to the instruction after the end of the second finally block, to add behavior just after the 'add creatures to menu' foreach loop.
                cursor.Prev = il.Body.ExceptionHandlers[1].HandlerEnd;

                // Call `InsertCreatures` with `this` and `ref counter`
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloca_S, il.Body.Variables[0]);
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
                    // Reserve slots from end of box for:
                    int padding = creatures
                        ? 6                              // empty space (1) + randomize button (1) + config buttons (3) + play button (1)
                        : 49 + (ModManager.MSC ? 14 : 0) // all of the above (6) + vanilla creatures (43) + downpour creatures (14)
                        ;

                    if (counter >= Width * Height - padding) {
                        GrowEditorSelector(self);
                    }

                    if (self.unlocks.SandboxItemUnlocked(unlock.Type)) {
                        self.AddButton(new CreatureOrItemButton(self.menu, self, new(common.Type.CritType, common.Type.ObjectType, unlock.Data)), ref counter);
                    } else {
                        self.AddButton(new LockedButton(self.menu, self), ref counter);
                    }
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