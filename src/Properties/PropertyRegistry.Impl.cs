using Fisobs.Core;

namespace Fisobs.Properties;

public sealed partial class PropertyRegistry : Registry
{
    private bool Player_IsObjectThrowable(On.Player.orig_IsObjectThrowable orig, Player self, PhysicalObject obj)
    {
        bool ret = orig(self, obj);

        P(obj)?.Throwable(self, ref ret);

        return ret;
    }

    private Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        Player.ObjectGrabability ret = orig(self, obj);

        P(obj)?.Grabability(self, ref ret);

        return ret;
    }

    private bool ScavengerAI_RealWeapon(On.ScavengerAI.orig_RealWeapon orig, ScavengerAI self, PhysicalObject obj)
    {
        bool ret = orig(self, obj);

        P(obj)?.LethalWeapon(self.scavenger, ref ret);

        return ret;
    }

    private int ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection, bool reallyWantsSpear)
    {
        int ret = orig(self, obj, pickupDropInsteadOfWeaponSelection, reallyWantsSpear);

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

    static Player? player;
    private void Player_ObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
    {
        try {
            player = self;
            orig(self, edible);
        } finally {
            player = null;
        }
    }

    private int SlugcatStats_NourishmentOfObjectEaten(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject)
    {
        int quarterPips = orig(slugcatIndex, eatenobject);
        if (player != null && eatenobject is PhysicalObject o) {
            P(o)?.Nourishment(player, ref quarterPips);
        }
        return quarterPips;
    }
}
