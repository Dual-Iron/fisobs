using Fisobs.Core;

namespace Fisobs.Properties
{
    /// <summary>
    /// Used by <see cref="PropertyRegistry"/> to manage an item's properties.
    /// </summary>
    public interface IPropertyHandler
    {
        /// <summary>
        /// The type of item. Only items of this type will be passed into <see cref="Properties(PhysicalObject)"/>.
        /// </summary>
        PhysobType Type { get; }

        /// <summary>
        /// Gets the custom properties of an item.
        /// </summary>
        /// <param name="forObject">The item in question.</param>
        /// <returns>An instance of <see cref="ItemProperties"/> or null.</returns>
        ItemProperties? Properties(PhysicalObject forObject);
    }
}
