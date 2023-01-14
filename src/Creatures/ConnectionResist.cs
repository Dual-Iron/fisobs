// Generated from RegEx
namespace Fisobs.Creatures
{
    /// <summary>
    /// Resistance to pathfinding over certain connections between tiles.
    /// </summary>
    public struct ConnectionResist
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        // These are all pretty self-explanatory.
        public PathCost Standard;
        public PathCost ReachOverGap;
        public PathCost ReachUp;
        public PathCost DoubleReachUp;
        public PathCost ReachDown;
        public PathCost SemiDiagonalReach;
        public PathCost DropToFloor;
        public PathCost DropToClimb;
        public PathCost DropToWater;
        public PathCost LizardTurn;
        public PathCost OpenDiagonal;
        public PathCost Slope;
        public PathCost CeilingSlope;
        public PathCost ShortCut;
        public PathCost NPCTransportation;
        public PathCost BigCreatureShortCutSqueeze;
        public PathCost OutsideRoom;
        public PathCost SideHighway;
        public PathCost SkyHighway;
        public PathCost SeaHighway;
        public PathCost RegionTransportation;
        public PathCost BetweenRooms;
        public PathCost OffScreenMovement;
        public PathCost OffScreenUnallowed;
    }
}
