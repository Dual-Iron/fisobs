using Fisobs.Core;
using Menu;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fisobs.Sandbox;

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

    private void Update(List<MultiplayerUnlocks.SandboxUnlockID> list, IList<SandboxUnlock> unlocks, bool remove)
    {
        foreach (var unlock in unlocks) {
            if (remove) {
                list.Remove(unlock.Type);
            } else if (!list.Contains(unlock.Type)) {
                list.Add(unlock.Type);
            }
        }
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
        // Sandbox UI
        On.Menu.SandboxSettingsInterface.ReinitInterface += SandboxSettingsInterface_ReinitInterface1;
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;

        // Creatures
        On.Menu.SandboxSettingsInterface.DefaultKillScores += DefaultKillScores;
        On.Menu.SandboxSettingsInterface.GetSandboxUnlocksToShow += SandboxSettingsInterface_GetSandboxUnlocksToShow;

        // Common.cs: Items + Creatures
        On.SandboxGameSession.SpawnEntity += SpawnEntity;
        On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += FromUnlock;
        On.MultiplayerUnlocks.SandboxUnlockForSymbolData += FromSymbolData;
        On.MultiplayerUnlocks.ParentSandboxID += GetParent;
        On.MultiplayerUnlocks.TiedSandboxIDs += TiedSandboxIDs;
        On.PlayerProgression.MiscProgressionData.GetTokenCollected_SandboxUnlockID += GetCollected; // force-assume slugcat token is collected
        On.ArenaBehaviors.SandboxEditor.GetPerformanceEstimate += SandboxEditor_GetPerformanceEstimate;
    }

    private void SandboxSettingsInterface_ReinitInterface1(On.Menu.SandboxSettingsInterface.orig_ReinitInterface orig, SandboxSettingsInterface self)
    {
        orig(self);

        if (self.nextPage != null && self.prevPage != null) {
            self.RemoveSubObject(self.nextPage);
            self.RemoveSubObject(self.prevPage);
            self.nextPage.RemoveSprites();
            self.prevPage.RemoveSprites();
            self.nextPage = null;
            self.prevPage = null;
        }

        self.subObjects.Add(new Paginator(self, Vector2.zero));

        foreach (var ctrl in self.scoreControllers) {
            ctrl.RemoveSprites();
            self.RemoveSubObject(ctrl);
        }
        self.scoreControllers.Clear();

        var __ = new IntVector2();
        var show = SandboxSettingsInterface.GetSandboxUnlocksToShow();
        foreach (var unlock in show) {
            self.AddScoreButton(unlock, ref __);
        }

        self.AddPositionedScoreButton(new SandboxSettingsInterface.MiscScore(self.menu, self, self.menu.Translate("Food"), "FOODSCORE"), ref __, default);
        self.AddPositionedScoreButton(new SandboxSettingsInterface.MiscScore(self.menu, self, self.menu.Translate("Survive"), "SURVIVESCORE"), ref __, default);
        self.AddPositionedScoreButton(new SandboxSettingsInterface.MiscScore(self.menu, self, self.menu.Translate("Spear hit"), "SPEARHITSCORE"), ref __, default);

        self.scoreControllers.ForEach(s => s.scoreDragger.UpdateScoreText());
    }


    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        foreach (var sbox in sboxes.Values) {
            if (sbox.Type.IsCrit) {
                Update(MultiplayerUnlocks.CreatureUnlockList, sbox.SandboxUnlocks, remove: false);
            } else {
                Update(MultiplayerUnlocks.ItemUnlockList, sbox.SandboxUnlocks, remove: false);
            }
        }
    }

    private void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);

        foreach (var sbox in sboxes.Values) {
            if (sbox.Type.IsCrit) {
                Update(MultiplayerUnlocks.CreatureUnlockList, sbox.SandboxUnlocks, remove: true);
            } else {
                Update(MultiplayerUnlocks.ItemUnlockList, sbox.SandboxUnlocks, remove: true);
            }
        }
    }

    private void DefaultKillScores(On.Menu.SandboxSettingsInterface.orig_DefaultKillScores orig, ref int[] killScores)
    {
        orig(ref killScores);

        foreach (var unlock in sboxes.Values.Where(c => c.Type.IsCrit).SelectMany(c => c.SandboxUnlocks)) {
            int unlockTy = (int)unlock.Type;
            if (unlockTy >= 0 && unlockTy < killScores.Length) {
                killScores[unlockTy] = unlock.KillScore.Value;
            } else {
                Debug.LogError($"The sandbox unlock type \"{unlock.Type}\" ({(int)unlock.Type}) is not in the range [0, {killScores.Length}).");
            }
        }
    }

    private List<MultiplayerUnlocks.SandboxUnlockID> SandboxSettingsInterface_GetSandboxUnlocksToShow(On.Menu.SandboxSettingsInterface.orig_GetSandboxUnlocksToShow orig)
    {
        var list = orig();
        foreach (var sbox in sboxes.Values) {
            foreach (var unlock in sbox.SandboxUnlocks) {
                if (!unlock.KillScore.IsConfigurable) {
                    list.Remove(unlock.Type);
                }
            }
        }
        return list;
    }
}
