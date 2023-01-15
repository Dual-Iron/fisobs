using Fisobs.Properties;
using System.Linq;

namespace CentiShields;

sealed class CentiShieldProperties : ItemProperties
{
    // TODO scav support
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
