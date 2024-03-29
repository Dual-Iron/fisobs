﻿using Fisobs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using ObjectType = AbstractPhysicalObject.AbstractObjectType;

namespace Fisobs.Items;

/// <summary>
/// A registry that stores <see cref="Fisob"/> instances and the hooks relevant to them.
/// </summary>
public sealed class FisobRegistry : Registry
{
    bool init;

    /// <summary>
    /// The singleton instance of this class.
    /// </summary>
    public static FisobRegistry Instance { get; } = new FisobRegistry();

    readonly Dictionary<ObjectType, Fisob> fisobs = new();

    private FisobRegistry() { }

    /// <inheritdoc/>
    protected override void Process(IContent entry)
    {
        if (entry is Fisob fisob) {
            fisobs[fisob.Type] = fisob;
        }
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.ItemSymbol.SymbolDataFromItem += ItemSymbol_SymbolDataFromItem;
        On.ItemSymbol.ColorForItem += ItemSymbol_ColorForItem;
        On.ItemSymbol.SpriteNameForItem += ItemSymbol_SpriteNameForItem;
        On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (!init) {
            init = true;
            foreach (var common in fisobs.Values) {
                common.LoadResources(self);
            }
        }
    }

    private IconSymbol.IconSymbolData? ItemSymbol_SymbolDataFromItem(On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
    {
        if (fisobs.TryGetValue(item.type, out var fisob)) {
            return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, item.type, fisob.Icon.Data(item));
        }
        return orig(item);
    }

    private Color ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, ObjectType itemType, int intData)
    {
        if (fisobs.TryGetValue(itemType, out var fisob)) {
            return fisob.Icon.SpriteColor(intData);
        }
        return orig(itemType, intData);
    }

    private string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, ObjectType itemType, int intData)
    {
        if (fisobs.TryGetValue(itemType, out var fisob)) {
            return fisob.Icon.SpriteName(intData);
        }
        return orig(itemType, intData);
    }

    private AbstractPhysicalObject? SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
    {
        var data = objString.Split(new[] { "<oA>" }, StringSplitOptions.None);
        var type = new ObjectType(data[1]);

        if (fisobs.TryGetValue(type, out Fisob o) && data.Length > 2) {
            EntityID id = EntityID.FromString(data[0]);
            WorldCoordinate coord = WorldCoordinate.FromString(data[2]);
            string customData = data.Length > 3 ? data[3] : "";

            try {
                return o.Parse(world, new EntitySaveData(o.Type, id, coord, customData, SaveUtils.PopulateUnrecognizedStringAttrs(data, 4)), null);
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError($"An exception was thrown in {o.GetType().FullName}::Parse: {e.Message}");
                return null;
            }
        }

        return orig(world, objString);
    }
}
