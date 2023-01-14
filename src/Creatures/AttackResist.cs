// Generated from RegEx
namespace Fisobs.Creatures
{
    /// <summary>
    /// Damage and stun resistance to particular damage types. Damage and stun amount will be <i>divided by</i> these values, so higher values mean more resistance.
    /// </summary>
    public struct AttackResist
    {
        /// <summary>
        /// Base resistance. This applies to all damage types.
        /// </summary>
        public float Base;
        /// <summary>
        /// Blunt damage, like from rocks.
        /// </summary>
        public float Blunt;
        /// <summary>
        /// Stab damage, like from spears.
        /// </summary>
        public float Stab;
        /// <summary>
        /// Bite damage, like from lizards.
        /// </summary>
        public float Bite;
        /// <summary>
        /// Seems unused?
        /// </summary>
        public float Water;
        /// <summary>
        /// Explosion damage, like from <see cref="ScavengerBomb"/> items.
        /// </summary>
        public float Explosion;
        /// <summary>
        /// Electric damage, like from underwater shocks and Underhang storms.
        /// </summary>
        public float Electric;
    }
}
