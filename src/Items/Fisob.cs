using Fisobs.Properties;
using Fisobs.Core;
using Fisobs.Sandbox;
using System;
using System.Collections.Generic;
using UnityEngine;
using ObjectType = AbstractPhysicalObject.AbstractObjectType;

namespace Fisobs.Items
{
    /// <summary>
    /// Represents the "metadata" for a custom item.
    /// </summary>
    public abstract class Fisob : IContent, IPropertyHandler, ISandboxHandler
    {
        private readonly List<SandboxUnlock> sandboxUnlocks = new();

        /// <summary>
        /// Creates a new <see cref="Fisob"/> instance for the given <paramref name="type"/>.
        /// </summary>
        protected Fisob(ObjectType type)
        {
            if (type == 0) {
                ArgumentException e = new($"The {GetType().Name} fisob's enum value was zero. Did you forget to add a BepInDependency attribute to your plugin class?", nameof(type));
                Debug.LogException(e);
                Console.WriteLine(e);
                throw e;
            }

            Type = type;
        }

        /// <summary>The fisob's type.</summary>
        public ObjectType Type { get; }
        /// <summary>The fisob's icon; a <see cref="DefaultIcon"/> by default.</summary>
        /// <remarks>When <see cref="LoadResources(RainWorld)"/> is called, an embedded resource with the name <c>$"icon_{Type}"</c> will be auto-loaded as a <see cref="SimpleIcon"/>, if it exists.</remarks>
        public Icon Icon { get; set; } = new DefaultIcon();

        /// <summary>
        /// Gets a new <see cref="AbstractPhysicalObject"/> instance from custom data.
        /// </summary>
        /// <param name="world">The world the entity lives in.</param>
        /// <param name="entitySaveData">The entity's save data.</param>
        /// <param name="unlock">The sandbox unlock that spawned this entity, or <see langword="null"/> if the entity wasn't spawned by one.</param>
        public abstract AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock? unlock);

        /// <inheritdoc/>
        public virtual ItemProperties? Properties(PhysicalObject forObject) => null;
        
        /// <summary>
        /// Loads <see cref="FAtlas"/> and <see cref="FAtlasElement"/> sprites. The <see cref="Ext.LoadAtlasFromEmbRes(System.Reflection.Assembly, string)"/> is recommended for this.
        /// </summary>
        /// <param name="rainWorld">The current <see cref="RainWorld"/> instance.</param>
        public virtual void LoadResources(RainWorld rainWorld)
        {
            string iconName = Ext.LoadAtlasFromEmbRes(GetType().Assembly, $"icon_{Type}")?.name ?? "Futile_White";

            if (Icon is DefaultIcon) {
                Icon = new SimpleIcon(iconName, Ext.MenuGrey);
            }
        }

        /// <summary>
        /// Registers a sandbox unlock under this fisob.
        /// </summary>
        /// <param name="type">The sandbox unlock type.</param>
        /// <param name="parent">The sandbox's parent unlock. If the parent type's token has been collected in story mode, then this item will be unlocked. To unconditionally unlock this item, set <paramref name="parent"/> to <see cref="MultiplayerUnlocks.SandboxUnlockID.Slugcat"/>.</param>
        /// <param name="data">The sandbox unlock's data value. This takes the place of <see cref="Icon.Data(AbstractPhysicalObject)"/> when spawning objects from sandbox mode.</param>
        public void RegisterUnlock(MultiplayerUnlocks.SandboxUnlockID type, MultiplayerUnlocks.SandboxUnlockID? parent = null, int data = 0)
        {
            sandboxUnlocks.Add(new(type, parent, data, default));
        }

        PhysobType IPropertyHandler.Type => Type;
        PhysobType ISandboxHandler.Type => Type;

        IList<SandboxUnlock> ISandboxHandler.SandboxUnlocks => sandboxUnlocks;

        IEnumerable<Registry> IContent.Registries()
        {
            yield return FisobRegistry.Instance;
            yield return PropertyRegistry.Instance;
            yield return SandboxRegistry.Instance;
        }

        AbstractWorldEntity ISandboxHandler.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
        {
            return Parse(world, data, unlock);
        }
    }
}
