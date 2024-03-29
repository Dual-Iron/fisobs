﻿namespace Fisobs.Core;

/// <summary>
/// The type of an item (<see cref="AbstractPhysicalObject.AbstractObjectType"/>) or creature (<see cref="CreatureTemplate.Type"/>).
/// </summary>
public readonly struct PhysobType
{
    /// <summary>
    /// If the object is an item, this is its type.
    /// </summary>
    public readonly AbstractPhysicalObject.AbstractObjectType ObjectType { get; }
    /// <summary>
    /// If the object is a creature, this is its type.
    /// </summary>
    public readonly CreatureTemplate.Type CritType { get; }
    /// <summary>
    /// True if the object is a creature.
    /// </summary>
    public readonly bool IsCrit => ObjectType == AbstractPhysicalObject.AbstractObjectType.Creature;

    /// <summary>
    /// Creates a new <see cref="PhysobType"/> that represents an object type.
    /// </summary>
    public PhysobType(AbstractPhysicalObject.AbstractObjectType objectType) : this()
    {
        CritType = CreatureTemplate.Type.StandardGroundCreature;
        ObjectType = objectType;
    }

    /// <summary>
    /// Creates a new <see cref="PhysobType"/> that represents a creature type.
    /// </summary>
    public PhysobType(CreatureTemplate.Type critType) : this()
    {
        CritType = critType;
        ObjectType = AbstractPhysicalObject.AbstractObjectType.Creature;
    }

    /// <summary>
    /// Checks if two <see cref="PhysobType"/> instances refer to the same type.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is PhysobType type &&
               ObjectType == type.ObjectType &&
               CritType == type.CritType;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 1756035919;
        hashCode = hashCode * -1521134295 + ObjectType?.GetHashCode() ?? 0;
        hashCode = hashCode * -1521134295 + CritType?.GetHashCode() ?? 0;
        return hashCode;
    }

    /// <inheritdoc/>
    public override readonly string? ToString()
    {
        return IsCrit ? CritType.ToString() : ObjectType.ToString();
    }

    /// <summary>
    /// Checks if two <see cref="PhysobType"/> instances refer to the same type.
    /// </summary>
    public static bool operator ==(PhysobType left, PhysobType right) => left.Equals(right);
    /// <summary>
    /// Checks if two <see cref="PhysobType"/> instances refer to the same type.
    /// </summary>
    public static bool operator !=(PhysobType left, PhysobType right) => !(left == right);

    /// <summary>
    /// Creates a new <see cref="PhysobType"/> that represents an object type.
    /// </summary>
    public static implicit operator PhysobType(AbstractPhysicalObject.AbstractObjectType objectType) => new(objectType);
    /// <summary>
    /// Creates a new <see cref="PhysobType"/> that represents a creature type.
    /// </summary>
    public static implicit operator PhysobType(CreatureTemplate.Type critType) => new(critType);
    /// <summary>
    /// Creates a new <see cref="PhysobType"/> that represents an icon's object or creature type.
    /// </summary>
    public static implicit operator PhysobType(IconSymbol.IconSymbolData icon)
    {
        if (icon.itemType == AbstractPhysicalObject.AbstractObjectType.Creature) {
            return new(icon.critType);
        }
        return new(icon.itemType);
    }
}
