namespace Fisobs.Creatures
{
    /// <summary>
    /// Represents how much danger a creature poses to a player that shares a shelter with it.
    /// </summary>
    public enum ShelterDanger
    {
        /// <summary>Likely won't pose harm to the player.</summary>
        Safe,
        /// <summary>Almost certainly poses harm to the player. Excludes lizards.</summary>
        /// <remarks>After sleeping in a shelter with a player, these creatures are killed.</remarks>
        Hostile,
        /// <summary>Too big to sanely stuff into a shelter. Includes rain deer, vultures, daddy long legs, and more.</summary>
        /// <remarks>After sleeping in a shelter with a player, these creatures are killed and sent back to their den.</remarks>
        TooLarge
    }
}
