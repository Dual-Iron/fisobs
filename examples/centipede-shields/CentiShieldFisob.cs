using Fisobs.Properties;
using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Sandbox;
using System.Linq;
using UnityEngine;

namespace CentiShields
{
    sealed class CentiShieldFisob : Fisob
    {
        public CentiShieldFisob() : base(EnumExt_CentiShields.CentiShield)
        {
            // Fisobs auto-loads the `icon_CentiShield` embedded resource as a texture.
            // See `CentiShields.csproj` for how you can add embedded resources to your project.

            // If you want a simple grayscale icon, you can omit the following line.
            Icon = new CentiShieldIcon();

            RegisterUnlock(EnumExt_CentiShields.OrangeCentiShield, parent: MultiplayerUnlocks.SandboxUnlockID.BigCentipede, data: 70);
            RegisterUnlock(EnumExt_CentiShields.RedCentiShield, parent: MultiplayerUnlocks.SandboxUnlockID.RedCentipede, data: 0);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
        {
            // Centi shield data is just floats separated by ; characters.
            string[] p = saveData.CustomData.Split(';');

            if (p.Length < 5) {
                p = new string[5];
            }

            var result = new CentiShieldAbstract(world, saveData.Pos, saveData.ID) {
                hue = float.TryParse(p[0], out var h) ? h : 0,
                saturation = float.TryParse(p[1], out var s) ? s : 1,
                scaleX = float.TryParse(p[2], out var x) ? x : 1,
                scaleY = float.TryParse(p[3], out var y) ? y : 1,
                damage = float.TryParse(p[4], out var r) ? r : 0
            };

            // If this is coming from a sandbox unlock, the hue and size should depend on the data value (see CentiShieldIcon below).
            if (unlock is SandboxUnlock u) {
                result.hue = u.Data / 1000f;

                if (u.Data == 0) {
                    result.scaleX += 0.2f;
                    result.scaleY += 0.2f;
                }
            }

            return result;
        }

        private static readonly CentiShieldProperties properties = new();

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            // If you need to use the forObject parameter, pass it to your ItemProperties class's constructor.
            // The Mosquitoes example demonstrates this.
            return properties;
        }
    }

    sealed class CentiShieldIcon : Icon
    {
        // Vanilla only gives you one int field to store all your custom data.
        // Here, that int field is used to store the shield's hue, scaled by 1000.
        // So, 0 is red and 70 is orange.
        public override int Data(AbstractPhysicalObject apo)
        {
            return apo is CentiShieldAbstract shield ? (int)(shield.hue * 1000f) : 0;
        }

        public override Color SpriteColor(int data)
        {
            return RWCustom.Custom.HSL2RGB(data / 1000f, 0.65f, 0.4f);
        }

        public override string SpriteName(int data)
        {
            // Fisobs autoloads the embedded resource named `icon_{Type}` automatically
            // For CentiShields, this is `icon_CentiShield`
            return "icon_CentiShield";
        }
    }

    sealed class CentiShieldProperties : ItemProperties
    {
        public override void Throwable(Player player, ref bool throwable)
            => throwable = false;

        public override void ScavCollectScore(Scavenger scavenger, ref int score)
            => score = 3;

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            // The player can only grab one centishield at a time,
            // but that shouldn't prevent them from grabbing a spear,
            // so don't use Player.ObjectGrabability.BigOneHand

            if (player.grasps.Any(g => g?.grabbed is CentiShield)) {
                grabability = Player.ObjectGrabability.CantGrab;
            } else {
                grabability = Player.ObjectGrabability.OneHand;
            }
        }
    }
}
