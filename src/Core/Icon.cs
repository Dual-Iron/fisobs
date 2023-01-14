using UnityEngine;

namespace Fisobs.Core
{
    /// <summary>
    /// Determines how a physical object is displayed on the map screen and sandbox menus.
    /// </summary>
    public abstract class Icon
    {
        /// <summary>
        /// Gets the name of the <see cref="FAtlasElement"/> to display.
        /// </summary>
        /// <param name="data">The physob's custom data. This comes from <see cref="Data(AbstractPhysicalObject)"/> if viewed from the story mode map, or from <see cref="Sandbox.SandboxUnlock.Data"/> if created from a sandbox unlock.</param>
        public abstract string SpriteName(int data);

        /// <summary>
        /// Gets the color of the sprite drawn on the screen.
        /// </summary>
        /// <param name="data">The physob's custom data. This comes from <see cref="Data(AbstractPhysicalObject)"/> if viewed from the story mode map, or from <see cref="Sandbox.SandboxUnlock.Data"/> if created from a sandbox unlock.</param>
        public abstract Color SpriteColor(int data);

        /// <summary>
        /// Gets the custom data for a physob. This value is passed to <see cref="SpriteName(int)"/> and <see cref="SpriteColor(int)"/>.
        /// </summary>
        /// <param name="apo">The physob in question.</param>
        public abstract int Data(AbstractPhysicalObject apo);
    }

    /// <summary>
    /// An icon whose sprite name is <c>"Futile_White"</c> and color is <see cref="Ext.MenuGrey"/>.
    /// </summary>
    public sealed class DefaultIcon : Icon
    {
        /// <inheritdoc/>
        public override int Data(AbstractPhysicalObject apo) => 0;
        /// <inheritdoc/>
        public override Color SpriteColor(int data) => Ext.MenuGrey;
        /// <inheritdoc/>
        public override string SpriteName(int data) => "Futile_White";
    }

    /// <summary>
    /// An icon whose sprite name and color is unchanging.
    /// </summary>
    public sealed class SimpleIcon : Icon
    {
        private readonly string spriteName;
        private readonly Color spriteColor;

        /// <summary>
        /// Creates a new <see cref="SimpleIcon"/> instance.
        /// </summary>
        /// <param name="spriteName">The name of the <see cref="FAtlasElement"/> to display.</param>
        /// <param name="spriteColor">The color of the sprite drawn on the screen.</param>
        public SimpleIcon(string spriteName, Color spriteColor)
        {
            this.spriteName = spriteName;
            this.spriteColor = spriteColor;
        }

        /// <inheritdoc/>
        public override int Data(AbstractPhysicalObject apo) => 0;
        /// <inheritdoc/>
        public override Color SpriteColor(int data) => spriteColor;
        /// <inheritdoc/>
        public override string SpriteName(int data) => spriteName;
    }
}
