using System.Collections.Generic;

namespace Fisobs.Core
{
    /// <summary>
    /// Some form of content that can be added to a <see cref="Registry"/>.
    /// </summary>
    public interface IContent
    {
        /// <summary>
        /// Gets the registries this content belongs to. This is called when the object is passed to the <see cref="Content.Register(IContent[])"/> method.
        /// </summary>
        IEnumerable<Registry> Registries();
    }
}