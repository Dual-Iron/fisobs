using System.Collections.Generic;

namespace Fisobs.Creatures
{
    using TTR = TileTypeResistance;
    using CR = TileConnectionResistance;
    using static AItile.Accessibility;
    using static MovementConnection.MovementType;
    using static Creature.DamageType;

    /// <summary>
    /// Simplifies the creation of <see cref="CreatureTemplate"/> instances.
    /// </summary>
    public sealed class CreatureFormula
    {
        /// <summary>
        /// The creature type.
        /// </summary>
        public readonly CreatureTemplate.Type Type;
        /// <summary>
        /// The name of the creature. This will be used in region file parsing and in logs.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The creature's ancestor. Field values are inherited by the ancestor unless present in this struct.
        /// </summary>
        public readonly CreatureTemplate? Ancestor;

        /// <summary>How the creature uses prebaked pathing. Most critobs will set this to some <see cref="PreBakedPathing.Ancestral(CreatureTemplate.Type)"/> value.</summary>
        public PreBakedPathing Pathing;
        /// <summary>How certain tiles impede the creature's movement.</summary>
        public TileResist TileResistances;
        /// <summary>How certain tile connections impede the creature's movement.</summary>
        public ConnectionResist ConnectionResistances;
        /// <summary>The creature's damage resistance.</summary>
        public AttackResist DamageResistances;
        /// <summary>The creature's stun resistance.</summary>
        public AttackResist StunResistances;
        /// <summary>How much damage the creature can take before instantly dying.</summary>
        public float InstantDeathDamage = float.MaxValue;
        /// <summary>True if the creature has AI. If this is true, then <see cref="Critob.GetRealizedAI(AbstractCreature)"/> must return a non-null value.</summary>
        public bool HasAI;
        /// <summary>The default relationship this creature has with other unknown creature types.</summary>
        public CreatureTemplate.Relationship DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0f);

        /// <summary>
        /// Creates a new <see cref="CreatureFormula"/> from a critob instance.
        /// </summary>
        public CreatureFormula(Critob critob, string name) : this(null, critob.Type, name)
        { }

        /// <summary>
        /// Creates a new <see cref="CreatureFormula"/>.
        /// </summary>
        public CreatureFormula(CreatureTemplate? ancestor, CreatureTemplate.Type type, string name)
        {
            Ancestor = ancestor;
            Type = type;
            Name = name;
        }

        // Reuse these lists because it saves on memory and performance a bit.
        // Not thread-safe but who cares
        static readonly List<TTR> tRs = new(capacity: 8);
        static readonly List<CR> cRs = new(capacity: 24);

        /// <summary>
        /// Creates a new <see cref="CreatureTemplate"/> from this formula.
        /// </summary>
        /// <returns></returns>
        public CreatureTemplate IntoTemplate()
        {
            tRs.Clear();
            cRs.Clear();

            AddTileRes(in TileResistances);
            AddConnRes(in ConnectionResistances);

            CreatureTemplate template = new(Type, Ancestor, tRs, cRs, DefaultRelationship) {
                name = Name,
                AI = HasAI,
                instantDeathDamageLimit = InstantDeathDamage,
                baseDamageResistance = DamageResistances.Base,
                baseStunResistance = StunResistances.Base,
            };

            AddResistances(template.damageRestistances, in DamageResistances, in StunResistances);

            // doPreBakedPathing    true iff        this creature defines a new pre-baked pathing type
            // requireAImap         true iff        doPreBakedPathing or preBakedPathingAncestor.doPreBakedPathing is true

            if (Pathing.IsAncestral(out var pathAncestor)) {
                template.doPreBakedPathing = false;
                template.requireAImap = true;
                template.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(pathAncestor);
            } else if (Pathing.IsOriginal) {
                template.doPreBakedPathing = true;
                template.requireAImap = true;
            } else {
                template.doPreBakedPathing = false;
                template.requireAImap = false;
            }

            return template;
        }

        private static void AddResistances(float[,] res, in AttackResist dmgRes, in AttackResist stunRes)
        {
            res[(int)Blunt, 0] = dmgRes.Blunt;
            res[(int)Stab, 0] = dmgRes.Stab;
            res[(int)Bite, 0] = dmgRes.Bite;
            res[(int)Water, 0] = dmgRes.Water;
            res[(int)Explosion, 0] = dmgRes.Explosion;
            res[(int)Electric, 0] = dmgRes.Electric;

            res[(int)Blunt, 1] = stunRes.Blunt;
            res[(int)Stab, 1] = stunRes.Stab;
            res[(int)Bite, 1] = stunRes.Bite;
            res[(int)Water, 1] = stunRes.Water;
            res[(int)Explosion, 1] = stunRes.Explosion;
            res[(int)Electric, 1] = stunRes.Electric;
        }

        private static void AddTileRes(in TileResist tR)
        {
            // I really wish I had macros while writing this

            if (tR.OffScreen != default) tRs.Add(new TTR(OffScreen, tR.OffScreen.resistance, tR.OffScreen.legality));
            if (tR.Air != default) tRs.Add(new TTR(Air, tR.Air.resistance, tR.Air.legality));
            if (tR.Ceiling != default) tRs.Add(new TTR(Ceiling, tR.Ceiling.resistance, tR.Ceiling.legality));
            if (tR.Climb != default) tRs.Add(new TTR(Climb, tR.Climb.resistance, tR.Climb.legality));
            if (tR.Corridor != default) tRs.Add(new TTR(Corridor, tR.Corridor.resistance, tR.Corridor.legality));
            if (tR.Floor != default) tRs.Add(new TTR(Floor, tR.Floor.resistance, tR.Floor.legality));
            if (tR.Solid != default) tRs.Add(new TTR(Solid, tR.Solid.resistance, tR.Solid.legality));
            if (tR.Wall != default) tRs.Add(new TTR(Wall, tR.Wall.resistance, tR.Wall.legality));
        }

        private static void AddConnRes(in ConnectionResist cR)
        {
            // Thank Stephen Cole Kleene for RegEx. This is the closest we'll come to C# macros.

            // MATCH    public PathCost (\w+);
            // REPLACE  if (cR.$1 != default) cRs.Add(new CR($1, cR.$1.resistance, cR.$1.legality));

            if (cR.Standard != default) cRs.Add(new CR(Standard, cR.Standard.resistance, cR.Standard.legality));
            if (cR.ReachOverGap != default) cRs.Add(new CR(ReachOverGap, cR.ReachOverGap.resistance, cR.ReachOverGap.legality));
            if (cR.ReachUp != default) cRs.Add(new CR(ReachUp, cR.ReachUp.resistance, cR.ReachUp.legality));
            if (cR.DoubleReachUp != default) cRs.Add(new CR(DoubleReachUp, cR.DoubleReachUp.resistance, cR.DoubleReachUp.legality));
            if (cR.ReachDown != default) cRs.Add(new CR(ReachDown, cR.ReachDown.resistance, cR.ReachDown.legality));
            if (cR.SemiDiagonalReach != default) cRs.Add(new CR(SemiDiagonalReach, cR.SemiDiagonalReach.resistance, cR.SemiDiagonalReach.legality));
            if (cR.DropToFloor != default) cRs.Add(new CR(DropToFloor, cR.DropToFloor.resistance, cR.DropToFloor.legality));
            if (cR.DropToClimb != default) cRs.Add(new CR(DropToClimb, cR.DropToClimb.resistance, cR.DropToClimb.legality));
            if (cR.DropToWater != default) cRs.Add(new CR(DropToWater, cR.DropToWater.resistance, cR.DropToWater.legality));
            if (cR.LizardTurn != default) cRs.Add(new CR(LizardTurn, cR.LizardTurn.resistance, cR.LizardTurn.legality));
            if (cR.OpenDiagonal != default) cRs.Add(new CR(OpenDiagonal, cR.OpenDiagonal.resistance, cR.OpenDiagonal.legality));
            if (cR.Slope != default) cRs.Add(new CR(Slope, cR.Slope.resistance, cR.Slope.legality));
            if (cR.CeilingSlope != default) cRs.Add(new CR(CeilingSlope, cR.CeilingSlope.resistance, cR.CeilingSlope.legality));
            if (cR.ShortCut != default) cRs.Add(new CR(ShortCut, cR.ShortCut.resistance, cR.ShortCut.legality));
            if (cR.NPCTransportation != default) cRs.Add(new CR(NPCTransportation, cR.NPCTransportation.resistance, cR.NPCTransportation.legality));
            if (cR.BigCreatureShortCutSqueeze != default) cRs.Add(new CR(BigCreatureShortCutSqueeze, cR.BigCreatureShortCutSqueeze.resistance, cR.BigCreatureShortCutSqueeze.legality));
            if (cR.OutsideRoom != default) cRs.Add(new CR(OutsideRoom, cR.OutsideRoom.resistance, cR.OutsideRoom.legality));
            if (cR.SideHighway != default) cRs.Add(new CR(SideHighway, cR.SideHighway.resistance, cR.SideHighway.legality));
            if (cR.SkyHighway != default) cRs.Add(new CR(SkyHighway, cR.SkyHighway.resistance, cR.SkyHighway.legality));
            if (cR.SeaHighway != default) cRs.Add(new CR(SeaHighway, cR.SeaHighway.resistance, cR.SeaHighway.legality));
            if (cR.RegionTransportation != default) cRs.Add(new CR(RegionTransportation, cR.RegionTransportation.resistance, cR.RegionTransportation.legality));
            if (cR.BetweenRooms != default) cRs.Add(new CR(BetweenRooms, cR.BetweenRooms.resistance, cR.BetweenRooms.legality));
            if (cR.OffScreenMovement != default) cRs.Add(new CR(OffScreenMovement, cR.OffScreenMovement.resistance, cR.OffScreenMovement.legality));
            if (cR.OffScreenUnallowed != default) cRs.Add(new CR(OffScreenUnallowed, cR.OffScreenUnallowed.resistance, cR.OffScreenUnallowed.legality));
        }
    }
}
