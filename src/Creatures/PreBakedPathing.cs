namespace Fisobs.Creatures
{
    /// <summary>
    /// The kind of prebaked pathing used by a creature template. Can be <see cref="None"/> (<see langword="default"/>), <see cref="Original"/>, or <see cref="Ancestral(CreatureTemplate.Type)"/>.
    /// </summary>
    public struct PreBakedPathing
    {
        private byte discriminant; // 0 for none, 1 for original, 2 for ancestor
        private CreatureTemplate.Type ancestor;

        /// <summary>
        /// Creatures with this do not use prebaked pathing. Slugcats and garbage worms use this option.
        /// </summary>
        public static PreBakedPathing None => new() { discriminant = 0 };

        /// <summary>
        /// Creatures with this use their own, original prebaked pathing.
        /// </summary>
        public static PreBakedPathing Original => new() { discriminant = 1 };

        /// <summary>
        /// Creatures with this use prebaked pathing from another creature.
        /// </summary>
        /// <param name="ancestor">The creature to inherit pathing from.</param>
        public static PreBakedPathing Ancestral(CreatureTemplate.Type ancestor) => new() { discriminant = 2, ancestor = ancestor };

        /// <summary>
        /// True if this is <see cref="None"/>.
        /// </summary>
        public bool IsNone => discriminant == 0;

        /// <summary>
        /// True if this is <see cref="Original"/>.
        /// </summary>
        public bool IsOriginal => discriminant == 1;

        /// <summary>
        /// True if this is <see cref="Ancestral(CreatureTemplate.Type)"/>.
        /// </summary>
        /// <param name="ancestor">The creature this pathing inherits from.</param>
        public bool IsAncestral(out CreatureTemplate.Type ancestor)
        {
            ancestor = this.ancestor;
            return discriminant == 2;
        }
    }
}
