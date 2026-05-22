using BepInEx;
using Fisobs.Core;
using System.Linq;
using System.Security.Permissions;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace CentiShields;

[BepInPlugin("org.dual.centishields", nameof(CentiShields), "1.1.1")]
sealed class Plugin : BaseUnityPlugin
{
    public void OnEnable()
    {
        Content.Register(new CentiShieldFisob());

        // Create centi shields when centipedes lose their shells
        On.Room.AddObject += RoomAddObject;

        // Protect the player from grabs while holding a shield
        On.Creature.Grab += CreatureGrab;

        // Allow BeastMaster to spawn CentiShields
        On.AbstractPhysicalObject.Realize += AbstractPhysicalObjectRealize;

        // Allow swallowing object, uncomment to allow players to swallow CentiShields
        //On.Player.CanBeSwallowed += PlayerCanBeSwallowed;
    }

    void RoomAddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
    {
        if (obj is CentipedeShell shell && shell.scaleX > 0.9f && shell.scaleY > 0.9f && Random.value < 0.25f) {
            var tilePos = self.GetTilePosition(shell.pos);
            var pos = new WorldCoordinate(self.abstractRoom.index, tilePos.x, tilePos.y, 0);
            var abstr = new CentiShieldAbstract(self.world, pos, self.game.GetNewID()) {
                hue = shell.hue,
                saturation = shell.saturation,
                scaleX = shell.scaleX,
                scaleY = shell.scaleY
            };
            obj = new CentiShield(abstr, shell.pos, shell.vel);

            self.abstractRoom.AddEntity(abstr);
        }

        orig(self, obj);
    }

    bool CreatureGrab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int _, int _2, Creature.Grasp.Shareability _3, float dominance, bool _4, bool _5)
    {
        const float maxDistance = 8;

        if (obj is Creature grabbed && self is not DropBug) {
            var grasp = grabbed.grasps?.FirstOrDefault(g => g?.grabbed is CentiShield);
            if (grasp?.grabbed is CentiShield shield && self.bodyChunks.Any(b => Vector2.Distance(b.pos, shield.firstChunk.pos) - (b.rad + shield.firstChunk.rad) < maxDistance)) {
                shield.AllGraspsLetGoOfThisObject(true);
                shield.Forbid();
                shield.HitEffect((shield.firstChunk.pos - self.firstChunk.pos).normalized);
                shield.AddDamage(Mathf.Clamp(dominance, 0.2f, 1f));
                self.Stun(20); return false;
            }
        }

        return orig(self, obj, _, _2, _3, dominance, _4, _5);
    }

    void AbstractPhysicalObjectRealize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        if (self.realizedObject == null && self.type == CentiShieldFisob.CentiShield && self is not CentiShieldAbstract) {
            CentiShieldAbstract result = new CentiShieldAbstract(self.world, self.pos, self.ID);
            result.hue = 0f;
            result.saturation = 1f;
            result.scaleX = 1.2f;
            result.scaleY = 1.2f;
            result.damage = 0f;
            self.realizedObject = new CentiShield(result, self.pos.Tile.ToVector2(), new Vector2());
        }

        orig(self);
    }

    bool PlayerCanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
    {
        bool ret = orig(self, testObj);
        ret |= testObj is CentiShield;
        return ret;
    }
}
