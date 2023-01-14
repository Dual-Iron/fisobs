namespace Fisobs.Core
{
    /// <summary>
    /// Registries hook into the vanilla game to simplify content creation.
    /// </summary>
    public abstract class Registry
    {
        private bool applied;

        /// <summary>
        /// Processes some content. The registry must ignore content not relevant to it and should throw an exception if content is relevant but malformed.
        /// </summary>
        /// <param name="content">The content entry to process.</param>
        protected abstract void Process(IContent content);

        internal void ProcessInternal(IContent content)
        {
            Process(content);
        }

        /// <summary>
        /// Should contain initialization logic. This is called right before <see cref="Process(IContent)"/> is called and should be used to apply things like MonoMod hooks.
        /// </summary>
        protected abstract void Initialize();

        internal void InitInternal()
        {
            if (!applied) {
                applied = true;
                Initialize();
            }
        }
    }
}
