namespace Fisobs.Sandbox
{
    /// <summary>
    /// Represents an item or creature's performance impact in sandbox mode.
    /// </summary>
    public readonly struct SandboxPerformanceCost
    {
        /// <summary>
        /// A higher value means many creatures will proportionally degrade performance.
        /// </summary>
        public readonly float Linear;

        /// <summary>
        /// A higher value means many creatures will explosively degrade performance.
        /// </summary>
        public readonly float Exponential;

        /// <summary>
        /// Creates a new instance of the <see cref="SandboxPerformanceCost"/> class.
        /// </summary>
        /// <param name="linear">A higher value indicates having many of a creature will proportionally degrade performance.</param>
        /// <param name="exponential">A higher value indicates having many of a creature will explosively degrade performance.</param>
        public SandboxPerformanceCost(float linear, float exponential)
        {
            Linear = linear;
            Exponential = exponential;
        }
    }
}
