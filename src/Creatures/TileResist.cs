// Generated from RegEx
namespace Fisobs.Creatures
{
    /// <summary>
    /// Resistance to pathfinding over certain tiles.
    /// </summary>
    public struct TileResist
    {
        /// <summary>
        /// Abstract pathfinding.
        /// </summary>
        public PathCost OffScreen;
        /// <summary>
        /// The air just above solid tiles.
        /// </summary>
        public PathCost Floor;
        /// <summary>
        /// Tight corridors. These are the very thin and long "tunnels" creatures crawl through. Pipe cleaners love it here.
        /// </summary>
        public PathCost Corridor;
        /// <summary>
        /// Climbable tiles like poles.
        /// </summary>
        public PathCost Climb;
        /// <summary>
        /// The air just to the left and right of solid tiles.
        /// </summary>
        public PathCost Wall;
        /// <summary>
        /// The air just below solid tiles.
        /// </summary>
        public PathCost Ceiling;
        /// <summary>
        /// The air not adjacent to any solid tiles.
        /// </summary>
        public PathCost Air;
        /// <summary>
        /// Impassable solid tiles. Unless your creature can phase through walls, don't set this.
        /// </summary>
        public PathCost Solid;
    }
}
