using System;
using UnityEngine;

namespace Fisobs.Core
{
    /// <summary>
    /// Used to register custom content.
    /// </summary>
    public static class Content
    {
        /// <summary>
        /// Registers some content. Call this from your mod's entry point.
        /// </summary>
        /// <param name="content">A bunch of content. Currently, fisobs provides <see cref="Items.Fisob"/> and <see cref="Creatures.Critob"/> for content types.</param>
        public static void Register(params IContent[] content)
        {
            try {
                RegisterInner(content);
            } catch (Exception e) {
                Debug.LogException(e);
                Console.WriteLine(e);
                throw;
            }
        }

        private static void RegisterInner(IContent[] entries)
        {
            foreach (var entry in entries) {
                foreach (var registry in entry.Registries()) {
                    registry.InitInternal();
                    registry.ProcessInternal(entry);
                }
            }
        }
    }
}