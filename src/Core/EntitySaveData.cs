﻿using System;
using System.Text.RegularExpressions;

namespace Fisobs.Core;

/// <summary>
/// Represents saved information about <see cref="AbstractPhysicalObject"/> instances.
/// </summary>
public sealed class EntitySaveData
{
    /// <summary>
    /// The APO's type.
    /// </summary>
    public readonly PhysobType Type;

    /// <summary>
    /// The APO's ID.
    /// </summary>
    public readonly EntityID ID;

    /// <summary>
    /// The APO's position.
    /// </summary>
    public readonly WorldCoordinate Pos;

    /// <summary>
    /// Any extra data associated with the APO. This can be <see cref="string.Empty"/>, but not <see langword="null"/>.
    /// </summary>
    /// <remarks>For creatures, this will be a stringified <see cref="CreatureState"/>.</remarks>
    public readonly string CustomData;

    /// <summary>
    /// Unrecognized data associated with the APO. This is used by mods to dynamically add information to objects. For the most part, this should simply be ignored.
    /// </summary>
    public string UnrecognizedAttribute(int index) => unrecognizedAttributes[index];
    /// <summary>
    /// The number of <see cref="UnrecognizedAttribute(int)"/> elements saved by the APO.
    /// </summary>
    public int UnrecognizedAttributeCount => unrecognizedAttributes.Length;

    private readonly string[] unrecognizedAttributes = Array.Empty<string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySaveData"/> struct.
    /// </summary>
    /// <remarks>Do not use this constructor. Call <see cref="CreateFrom(AbstractPhysicalObject, string)"/> instead.</remarks>
    internal EntitySaveData(PhysobType type, EntityID id, WorldCoordinate pos, string customData, string[]? unrecognized)
    {
        Type = type;
        ID = id;
        Pos = pos;
        CustomData = customData;

        if (unrecognized != null) {
            unrecognizedAttributes = new string[unrecognized.Length];
            unrecognized.CopyTo(unrecognizedAttributes, index: 0);
        }
    }

    // Catches ASCII letters within <>s
    static readonly Regex dataSeparator = new("<([a-zA-Z]+)>");

    /// <summary>
    /// Creates an instance of the <see cref="EntitySaveData"/> struct.
    /// </summary>
    /// <param name="apo">The abstract physical object to get basic data from.</param>
    /// <param name="customData">Extra data associated with the abstract physical object. This data should never contain &lt; characters.</param>
    /// <returns>A new instance of <see cref="EntitySaveData"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="customData"/> contains &lt; characters.</exception>
    public static EntitySaveData CreateFrom(AbstractPhysicalObject apo, string customData = "")
    {
        if (customData is null) {
            throw new ArgumentNullException(nameof(customData));
        }

        if (apo is AbstractCreature) {
            if (dataSeparator.Match(customData) is Match m && m.Success) {
                if (m.Groups[1].Value is not "cB" and not "cC") {
                    throw new ArgumentException($"Creature data cannot contain the pattern \"{m}\". Use the patterns \"<cB>\" and \"<cC>\" for separating creature data.");
                }
            }
        } else if (customData.IndexOf('<') != -1) {
            throw new ArgumentException("Item data cannot contain the < character.");
        }

        if (apo is AbstractCreature crit) {
            return new EntitySaveData(crit.creatureTemplate.type, apo.ID, apo.pos, customData, apo.unrecognizedAttributes);
        }

        return new EntitySaveData(apo.type, apo.ID, apo.pos, customData, apo.unrecognizedAttributes);
    }

    /// <summary>
    /// Gets this entity's save data as a string.
    /// </summary>
    /// <returns>A string representation of this data.</returns>
    public string ToString(AbstractPhysicalObject? apo)
    {
        if (Type.IsCrit) {
            string roomName = apo?.world.GetAbstractRoom(Pos.room)?.name ?? Pos.ResolveRoomName() ?? Pos.room.ToString();
            string baseStringC = $"{Type.CritType}<cA>{ID}<cA>{roomName}.{Pos.abstractNode}<cA>{CustomData}";
            return SaveUtils.AppendUnrecognizedStringAttrs(baseStringC, "<cA>", unrecognizedAttributes);
        }

        string baseString = $"{ID}<oA>{Type.ObjectType}<oA>{Pos.SaveToString()}<oA>{CustomData}";
        return SaveUtils.AppendUnrecognizedStringAttrs(SaveState.SetCustomData(apo, baseString), "<oA>", unrecognizedAttributes);
    }

    /// <summary>
    /// Gets this entity's save data as a string.
    /// </summary>
    /// <returns>A string representation of this data.</returns>
    public override string ToString() => ToString(null);
}
