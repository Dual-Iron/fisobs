using Fisobs.Core;
using UnityEngine;

namespace CentiShields
{
    sealed class CentiShieldAbstract : AbstractPhysicalObject
    {
        public CentiShieldAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, EnumExt_CentiShields.CentiShield, null, pos, ID)
        {
            scaleX = 1;
            scaleY = 1;
            saturation = 0.5f;
            hue = 1f;
        }

        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
                realizedObject = new CentiShield(this, Room.realizedRoom.MiddleOfTile(pos.Tile), Vector2.zero);
        }

        public float hue;
        public float saturation;
        public float scaleX;
        public float scaleY;
        public float damage;

        public override string ToString()
        {
            return this.SaveToString($"{hue};{saturation};{scaleX};{scaleY};{damage}");
        }
    }
}