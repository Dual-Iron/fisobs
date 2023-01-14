using Fisobs.Core;
using Menu;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fisobs.Sandbox
{
    public sealed partial class SandboxRegistry : Registry
    {
        private IEnumerable<SandboxUnlock> UnlocksToAdd()
        {
            return sboxes.Values.Where(c => c.Type.IsCrit).SelectMany(c => c.SandboxUnlocks).Where(s => s.KillScore.IsConfigurable);
        }

        private void TryAddPaginator(SandboxSettingsInterface self)
        {
            // If there's no existing paginator, add one.
            var paginator = self.subObjects.Find(m => m.ToString().StartsWith(paginatorKey));
            if (paginator == null) {
                self.subObjects.Add(new Paginator(self, Vector2.zero));
                return;
            }

            // If there's a paginator, but it's outdated, replace it with ours.
            string versionString = paginator.ToString().Substring(startIndex: paginatorKeyLength);

            if (int.TryParse(versionString, out int version) && version < paginatorVersion) {
                self.RemoveSubObject(paginator);
                paginator.RemoveSprites();

                self.subObjects.Add(new Paginator(self, Vector2.zero));
            }
        }

        private void AddPages(On.Menu.SandboxSettingsInterface.orig_ctor orig, SandboxSettingsInterface self, Menu.Menu menu, MenuObject owner)
        {
            orig(self, menu, owner);
            AddPagesImpl(self, menu);
        }

        private void AddPagesImpl(SandboxSettingsInterface self, Menu.Menu menu)
        {
            var unlocksToAdd = UnlocksToAdd();
            if (!unlocksToAdd.Any()) {
                return;
            }

            TryAddPaginator(self);

            foreach (var unlock in unlocksToAdd) {
                SandboxSettingsInterface.ScoreController button =
                    self.GetMultiplayerMenu.multiplayerUnlocks.SandboxItemUnlocked(unlock.Type)
                    ? new SandboxSettingsInterface.KillScore(menu, self, unlock.Type)
                    : new SandboxSettingsInterface.LockedScore(menu, self);
                    
                self.scoreControllers.Add(button);
                self.subObjects.Add(button);
                button.scoreDragger.UpdateScoreText();
                button.pos = new(100000, 100000); // this will be calculated correctly in the paginator
            }
        }

        private void DefaultKillScores(On.Menu.SandboxSettingsInterface.orig_DefaultKillScores orig, ref int[] killScores)
        {
            orig(ref killScores);

            foreach (var unlock in UnlocksToAdd()) {
                int unlockTy = (int)unlock.Type;

                if (unlockTy >= 0 && unlockTy < killScores.Length) {
                    killScores[unlockTy] = unlock.KillScore.Value;
                } else {
                    Debug.LogError($"The sandbox unlock type \"{unlock.Type}\" ({(int)unlock.Type}) is not in the range [0, {killScores.Length}).");
                }
            }
        }
    }
}
