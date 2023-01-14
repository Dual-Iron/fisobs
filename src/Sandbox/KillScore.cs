namespace Fisobs.Sandbox
{
    /// <summary>
    /// Determines how many points a player earns when killing a creature as The Hunter and in arena mode.
    /// </summary>
    public readonly struct KillScore
    {
        /// <summary>
        /// The score gained when the creature is killed.
        /// </summary>
        public readonly int Value;

        /// <summary>
        /// True if this value was created through <see cref="Configurable(int)"/>.
        /// </summary>
        public readonly bool IsConfigurable;

        /// <summary>
        /// Creates a kill score that players can configure in the sandbox menu.
        /// </summary>
        /// <param name="defaultScore">The score awarded to a player that kills this creature unless the value is overridden in sandbox mode.</param>
        public static KillScore Configurable(int defaultScore) => new(defaultScore, true);

        /// <summary>
        /// Creates a kill score that's hidden from the player and cannot be configured.
        /// </summary>
        /// <param name="score">The score awarded to a player that kills this creature.</param>
        public static KillScore Constant(int score) => new(score, false);

        private KillScore(int score, bool configurable)
        {
            Value = score;
            IsConfigurable = configurable;
        }
    }
}
