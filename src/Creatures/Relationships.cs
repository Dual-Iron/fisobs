using CreatureType = CreatureTemplate.Type;

namespace Fisobs.Creatures
{
    using static CreatureTemplate.Relationship;

    /// <summary>
    /// A wrapper around a <see cref="CreatureTemplate.Relationship"/> used to establish creature relationships.
    /// </summary>
    public struct Relationships
    {
        private readonly CreatureType self;

        /// <summary>
        /// Creates a new <see cref="Relationships"/> instance wrapping <paramref name="self"/>.
        /// </summary>
        public Relationships(CreatureType self)
        {
            this.self = self;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public void Ignores(CreatureType other) => StaticWorld.EstablishRelationship(self, other, new(Type.Ignores, 0f));
        public void IgnoredBy(CreatureType other) => StaticWorld.EstablishRelationship(other, self, new(Type.Ignores, 0f));
        public void Eats(CreatureType other, float intensity) => StaticWorld.EstablishRelationship(self, other, new(Type.Eats, intensity));
        public void EatenBy(CreatureType other, float intensity) => StaticWorld.EstablishRelationship(other, self, new(Type.Eats, intensity));
        public void Fears(CreatureType other, float intensity) => StaticWorld.EstablishRelationship(self, other, new(Type.Afraid, intensity));
        public void FearedBy(CreatureType other, float intensity) => StaticWorld.EstablishRelationship(other, self, new(Type.Afraid, intensity));
        public void IntimidatedBy(CreatureType other, float intensity) => StaticWorld.EstablishRelationship(self, other, new(Type.StayOutOfWay, intensity));
        public void Intimidates(CreatureType other, float intensity) => StaticWorld.EstablishRelationship(other, self, new(Type.StayOutOfWay, intensity));
        public void Attacks(CreatureType other, float intensity) => StaticWorld.EstablishRelationship(self, other, new(Type.Attacks, intensity));
        public void AttackedBy(CreatureType other, float intensity) => StaticWorld.EstablishRelationship(other, self, new(Type.Attacks, intensity));
        public void UncomfortableAround(CreatureType other, float intensity) => StaticWorld.EstablishRelationship(self, other, new(Type.Uncomfortable, intensity));
        public void MakesUncomfortable(CreatureType other, float intensity) => StaticWorld.EstablishRelationship(other, self, new(Type.Uncomfortable, intensity));
        public void Antagonizes(CreatureType other, float intensity) => StaticWorld.EstablishRelationship(self, other, new(Type.Antagonizes, intensity));
        public void AntagonizedBy(CreatureType other, float intensity) => StaticWorld.EstablishRelationship(other, self, new(Type.Antagonizes, intensity));

        public void HasDynamicRelationship(CreatureType other, float intensity = 1f) => StaticWorld.EstablishRelationship(self, other, new(Type.SocialDependent, intensity));

        public void IsInPack(CreatureType other, float intensity)
        {
            StaticWorld.EstablishRelationship(self, other, new(Type.Pack, intensity));
            StaticWorld.EstablishRelationship(other, self, new(Type.Pack, intensity));
        }

        public void PlaysWith(CreatureType other, float intensity)
        {
            StaticWorld.EstablishRelationship(self, other, new(Type.PlaysWith, intensity));
            StaticWorld.EstablishRelationship(other, self, new(Type.PlaysWith, intensity));
        }

        public void Rivals(CreatureType other, float intensity)
        {
            StaticWorld.EstablishRelationship(self, other, new(Type.AgressiveRival, intensity));
            StaticWorld.EstablishRelationship(other, self, new(Type.AgressiveRival, intensity));
        }
    }
}
