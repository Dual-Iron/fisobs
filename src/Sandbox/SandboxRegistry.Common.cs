using ArenaBehaviors;
using Fisobs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlayerProgression;

namespace Fisobs.Sandbox;

public sealed partial class SandboxRegistry : Registry
{
    private void SpawnEntity(On.SandboxGameSession.orig_SpawnEntity orig, SandboxGameSession self, SandboxEditor.PlacedIconData p)
    {
        EntityID id = self.GameTypeSetup.saveCreatures ? p.ID : self.game.GetNewID();
        WorldCoordinate coord = new(0, Mathf.RoundToInt(p.pos.x / 20f), Mathf.RoundToInt(p.pos.y / 20f), -1);

        if (sboxes.TryGetValue(p.data.critType, out var critob)) {
            EntitySaveData? data = null;

            if (self.GameTypeSetup.saveCreatures) {
                var creature = self.arenaSitting.creatures.FirstOrDefault(c => c.creatureTemplate.type == p.data.critType && c.ID == p.ID);
                if (creature != null) {
                    self.arenaSitting.creatures.Remove(creature);

                    for (int i = 0; i < 2; i++) {
                        creature.state.CycleTick();
                    }

                    data = EntitySaveData.CreateFrom(creature, creature.state.ToString());
                }
            }

            DoSpawn(self, p, data ?? new(p.data.critType, id, coord, ""), critob);

        } else if (sboxes.TryGetValue(p.data.itemType, out var fisob)) {
            EntitySaveData data = new(p.data.itemType, id, coord, "");

            DoSpawn(self, p, data, fisob);

        } else {
            orig(self, p);
        }
    }

    private static void DoSpawn(SandboxGameSession self, SandboxEditor.PlacedIconData p, EntitySaveData data, ISandboxHandler handler)
    {
        SandboxUnlock unlock = handler.SandboxUnlocks.FirstOrDefault(u => u.Data == p.data.intData);

        if (unlock == null) {
            Debug.LogError($"The fisob \"{handler.Type}\" had no sandbox unlocks where Data={p.data.intData}.");
            return;
        }

        try {
            var entity = handler.ParseFromSandbox(self.game.world, data, unlock);
            if (entity != null) {
                self.game.world.GetAbstractRoom(0).AddEntity(entity);
            } else {
                Debug.LogError($"The sandbox unlock \"{unlock.Type}\" returned null when being parsed in sandbox mode.");
            }
        } catch (Exception e) {
            Debug.LogError($"The sandbox unlock \"{unlock.Type}\" threw an exception when being parsed in sandbox mode.");
            Debug.LogException(e);
        }
    }

    private IconSymbol.IconSymbolData FromUnlock(On.MultiplayerUnlocks.orig_SymbolDataForSandboxUnlock orig, MultiplayerUnlocks.SandboxUnlockID unlockID)
    {
        foreach (var common in sboxes.Values) {
            var unlock = common.SandboxUnlocks.FirstOrDefault(u => u.Type == unlockID);
            if (unlock != null) {
                return new(common.Type.CritType, common.Type.ObjectType, unlock.Data);
            }
        }
        return orig(unlockID);
    }

    private MultiplayerUnlocks.SandboxUnlockID FromSymbolData(On.MultiplayerUnlocks.orig_SandboxUnlockForSymbolData orig, IconSymbol.IconSymbolData data)
    {
        if (sboxes.TryGetValue(data, out var item) && item.SandboxUnlocks.FirstOrDefault(u => u.Data == data.intData) is SandboxUnlock unlock) {
            return unlock.Type;
        }
        return orig(data);
    }

    private MultiplayerUnlocks.SandboxUnlockID? GetParent(On.MultiplayerUnlocks.orig_ParentSandboxID orig, MultiplayerUnlocks.SandboxUnlockID unlockID)
    {
        foreach (var common in sboxes.Values) {
            var unlock = common.SandboxUnlocks.FirstOrDefault(s => s.Type == unlockID);
            if (unlock != null) {
                return unlock.Parent;
            }
        }
        return orig(unlockID);
    }

    private List<MultiplayerUnlocks.SandboxUnlockID> TiedSandboxIDs(On.MultiplayerUnlocks.orig_TiedSandboxIDs orig, MultiplayerUnlocks.SandboxUnlockID parent, bool includeParent)
    {
        var ret = orig(parent, includeParent);

        int count = ret.Count;

        foreach (string name in ExtEnumBase.GetNames(typeof(MultiplayerUnlocks.SandboxUnlockID))) {
            var childUnlock = new MultiplayerUnlocks.SandboxUnlockID(name);

            if (MultiplayerUnlocks.ParentSandboxID(childUnlock) == parent) {
                ret.Add(childUnlock);
            }
        }

        if (count == ret.Count) {
            // If nothing changed, return the original list.
            return ret;
        } else {
            // Otherwise, remove any duplicate elements, then return as usual.
            return ret.Distinct().ToList();
        }
    }

    private bool GetCollected(On.PlayerProgression.MiscProgressionData.orig_GetTokenCollected_SandboxUnlockID orig, MiscProgressionData self, MultiplayerUnlocks.SandboxUnlockID unlockID)
    {
        if (unlockID == MultiplayerUnlocks.SandboxUnlockID.Slugcat) {
            return true;
        }
        return orig(self, unlockID);
    }

    private void SandboxEditor_GetPerformanceEstimate(On.ArenaBehaviors.SandboxEditor.orig_GetPerformanceEstimate orig, SandboxEditor.PlacedIcon placedIcon, ref float exponentialPart, ref float linearPart)
    {
        if (placedIcon is SandboxEditor.CreatureOrItemIcon icon && sboxes.TryGetValue(icon.iconData, out var sbox)) {
            linearPart += sbox.SandboxPerformanceCost.Linear;
            exponentialPart += sbox.SandboxPerformanceCost.Exponential;
        } else {
            orig(placedIcon, ref exponentialPart, ref linearPart);
        }
    }
}
