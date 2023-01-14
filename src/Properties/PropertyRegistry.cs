using Fisobs.Core;
using System.Collections.Generic;

namespace Fisobs.Properties
{
    /// <summary>
    /// A registry that stores <see cref="IPropertyHandler"/> instances and the hooks relevant to them.
    /// </summary>
    public sealed partial class PropertyRegistry : Registry
    {
        /// <summary>
        /// The singleton instance of this class.
        /// </summary>
        public static PropertyRegistry Instance { get; } = new PropertyRegistry();

        readonly Dictionary<PhysobType, IPropertyHandler> objs = new();

        private PropertyRegistry() { }

        /// <inheritdoc/>
        protected override void Process(IContent content)
        {
            if (content is IPropertyHandler common) {
                objs[common.Type] = common;
            }
        }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            On.Player.IsObjectThrowable += Player_IsObjectThrowable;
            On.Player.Grabability += Player_Grabability;
            On.ScavengerAI.RealWeapon += ScavengerAI_RealWeapon;
            On.ScavengerAI.WeaponScore += ScavengerAI_WeaponScore;
            On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
            IL.Player.ObjectEaten += Player_ObjectEaten;
        }

        private ItemProperties? P(PhysicalObject po)
        {
            if (po?.abstractPhysicalObject is AbstractPhysicalObject apo) {
                if (objs.TryGetValue(apo.type, out IPropertyHandler one)) {
                    return one.Properties(po);
                }
                if (apo is AbstractCreature crit && objs.TryGetValue(crit.creatureTemplate.type, out IPropertyHandler two)) {
                    return two.Properties(po);
                }
            }
            return null;
        }
    }
}
