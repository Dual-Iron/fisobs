namespace Fisobs.Properties
{
    /// <summary>
    /// Defines properties for custom item types, like how scavengers interact with the item and if players can grab it.
    /// </summary>
    public abstract class ItemProperties
    {
        /// <summary>
        /// How valuable the item is as a collectible.
        /// </summary>
        /// <remarks>Pearls score 10, spears score 3, shrooms score 2, unwanted items score 0.</remarks>
        /// <param name="scav">The scavenger.</param>
        /// <param name="score">Items with higher scores will be picked up first.</param>
        public virtual void ScavCollectScore(Scavenger scav, ref int score) { }

        /// <summary>
        /// How valuable the item is as a weapon.
        /// </summary>
        /// <remarks>Spears score 3, explosive spears score 4, spore puffs score 1.</remarks>
        /// <param name="scav">The scavenger.</param>
        /// <param name="score">Items with higher scores will be picked up first.</param>
        public virtual void ScavWeaponPickupScore(Scavenger scav, ref int score) { }

        /// <summary>
        /// How readily the item will be used as a weapon. This may depend on the scavenger's current violence type (<see cref="ScavengerAI.currentViolenceType"/>).
        /// </summary>
        /// <remarks>
        /// <code>
        /// +——————————————+————————+———————————+
        /// | ITEM         | LETHAL | NONLETHAL |
        /// +==============+========+===========+
        /// | normal spear | 3      | 2         |
        /// +——————————————+————————+———————————+
        /// | expld. spear | 4      | 1         |
        /// +——————————————+————————+———————————+
        /// | jellyfish    | 2      | 3         |
        /// +——————————————+————————+———————————+
        /// | spore puff   | 1      | 1         |
        /// +——————————————+————————+———————————+
        /// | rock         | 2      | 4*        |
        /// +——————————————+————————+———————————+
        /// | bomb         | 3**    | 0         |
        /// +——————————————+————————+———————————+
        /// 
        /// *  2 if the scav has rocks
        /// ** 1 if the scav is scared and next to the target, else 0 if there are friends next to the target
        /// </code>
        /// </remarks>
        /// <param name="scav">The scavenger.</param>
        /// <param name="score">Items with higher scores will be used first.</param>
        public virtual void ScavWeaponUseScore(Scavenger scav, ref int score) { }

        /// <summary>
        /// Determines if scavengers will purposely miss when using this item if they don't mean to kill their target.
        /// </summary>
        /// <remarks>
        /// Only <see cref="Spear"/> and <see cref="ScavengerBomb"/> objects are considered lethal in vanilla.
        /// </remarks>
        /// <param name="scav">The scavenger.</param>
        /// <param name="isLethal">True if the item is lethal.</param>
        public virtual void LethalWeapon(Scavenger scav, ref bool isLethal) { }

        /// <summary>
        /// Determines if the player can pick up the item.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="grabability">How difficult carrying the item is. Spears, for example, are <see cref="Player.ObjectGrabability.BigOneHand"/> items.</param>
        public virtual void Grabability(Player player, ref Player.ObjectGrabability grabability) { }

        /// <summary>
        /// Determines if the player can throw or toss the item.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="throwable">True if the item can be thrown. Vulture masks, for example, cannot be thrown.</param>
        public virtual void Throwable(Player player, ref bool throwable) { }

        /// <summary>
        /// This determines if the item gives full pips to the hunter slugcat when eaten.
        /// </summary>
        /// <remarks>This is ignored unless the item implements the <see cref="IPlayerEdible"/> interface.</remarks>
        /// <param name="player">The player.</param>
        /// <param name="meat">True if the item counts as "meat" in hunter mode.</param>
        public virtual void Meat(Player player, ref bool meat) { }
    }
}
