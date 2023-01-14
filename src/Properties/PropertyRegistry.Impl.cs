using Fisobs.Core;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace Fisobs.Properties
{
    public sealed partial class PropertyRegistry : Registry
    {
        private bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig, Player self, PhysicalObject obj)
        {
            bool ret = orig(self, obj);

            P(obj)?.Throwable(self, ref ret);

            return ret;
        }

        private int Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            Player.ObjectGrabability ret = (Player.ObjectGrabability)orig(self, obj);

            P(obj)?.Grabability(self, ref ret);

            return (int)ret;
        }

        private bool ScavengerAI_RealWeapon(On.ScavengerAI.orig_RealWeapon orig, ScavengerAI self, PhysicalObject obj)
        {
            bool ret = orig(self, obj);

            P(obj)?.LethalWeapon(self.scavenger, ref ret);

            return ret;
        }

        private int ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection)
        {
            int ret = orig(self, obj, pickupDropInsteadOfWeaponSelection);

            if (pickupDropInsteadOfWeaponSelection)
                P(obj)?.ScavWeaponPickupScore(self.scavenger, ref ret);
            else
                P(obj)?.ScavWeaponUseScore(self.scavenger, ref ret);

            return ret;
        }

        private int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
        {
            if (weaponFiltered) return orig(self, obj, true);

            int ret = orig(self, obj, weaponFiltered);

            P(obj)?.ScavCollectScore(self.scavenger, ref ret);

            return ret;
        }

        private void Player_ObjectEaten(ILContext il)
        {
            try {
                ILCursor cursor = new(il);

                cursor.GotoNext(MoveType.Before, i => i.MatchStloc(1));
                cursor.GotoPrev(MoveType.After, i => i.MatchLdloc(0));
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<bool, Player, IPlayerEdible, bool>>(Instance.ModifyIsMeat);

            } catch (Exception e) {
                Debug.LogException(e);
                Console.WriteLine($"Couldn't register fisobs in \"{nameof(Fisobs)}\" because of exception in {nameof(Player_ObjectEaten)}: {e.Message}");
            }
        }

        private bool ModifyIsMeat(bool meat, Player player, IPlayerEdible edible)
        {
            if (edible is PhysicalObject o) {
                P(o)?.Meat(player, ref meat);
            }
            return meat;
        }
    }
}
