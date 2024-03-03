using RWCustom;
using System;
using UnityEngine;

namespace Fisobs.Core;

/// <summary>
/// Provides extension methods for POs and APOs.
/// </summary>
public static class Ext
{
    /// <summary>
    /// The default color for menu items.
    /// </summary>
    public static Color MenuGrey { get; } = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);

    /// <summary>
    /// Realizes an APO at a position with a specified velocity.
    /// </summary>
    /// <param name="apo">The abstract physical object.</param>
    /// <param name="pos">The position of the object in the room.</param>
    /// <param name="vel">The velocity of the object's body chunks.</param>
    public static void Spawn(this AbstractPhysicalObject apo, Vector2 pos, Vector2 vel)
    {
        if (apo.realizedObject != null) {
            Debug.Log($"{nameof(Fisobs)} : TRYING TO REALIZE TWICE! " + apo);
            return;
        }

        apo.Room.AddEntity(apo);
        apo.RealizeInRoom();

        if (apo.realizedObject is PhysicalObject o) {
            foreach (var chunk in o.bodyChunks) {
                chunk.HardSetPosition(pos);
                chunk.vel = vel;
            }
        }
    }

    /// <summary>
    /// Realizes an APO at a position with a speed of zero.
    /// </summary>
    /// <param name="apo">The abstract physical object.</param>
    /// <param name="pos">The position of the object in the room.</param>
    public static void Spawn(this AbstractPhysicalObject apo, Vector2 pos)
    {
        Spawn(apo, pos, Vector2.zero);
    }

    /// <summary>
    /// Realizes an APO wherever it may be with a speed of zero.
    /// </summary>
    /// <param name="apo">The abstract physical object.</param>
    public static void Spawn(this AbstractPhysicalObject apo)
    {
        if (apo.Room.realizedRoom != null)
            Spawn(apo, apo.Room.realizedRoom.MiddleOfTile(apo.pos.Tile), Vector2.zero);
        else
            Debug.Log($"{nameof(Fisobs)} : TRYING TO REALIZE IN NON REALIZED ROOM! {apo}");
    }

    /// <summary>
    /// Gets a string representation of an APO.
    /// </summary>
    /// <param name="apo">The abstract physical object.</param>
    /// <param name="customData">Extra data associated with the abstract physical object. For creatures, this should be <see cref="CreatureState"/> data.</param>
    /// <returns>A string representing this APO, for use in <see cref="AbstractPhysicalObject.ToString"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="customData"/> contains &lt; characters.</exception>
    public static string SaveToString(this AbstractPhysicalObject apo, string customData = "")
    {
        return EntitySaveData.CreateFrom(apo, customData).ToString(apo);
    }

    /// <summary>
    /// Gets if the location <paramref name="tile"/> on <paramref name="map"/> is <paramref name="tilesOfFreeSpace"/>-or-more tiles away from any solid terrain.
    /// </summary>
    /// <returns>True if the space is clear.</returns>
    public static bool IsFreeSpace(this AImap map, IntVector2 tile, int tilesOfFreeSpace)
    {
        return map.getTerrainProximity(tile) >= tilesOfFreeSpace;
    }

    /// <summary>
    /// Loads an icon for a critob named <paramref name="name"/> into <see cref="Futile.atlasManager"/> if the resource exists.
    /// </summary>
    /// <param name="name">The critob's name.</param>
    /// <returns>If the resource was successfully loaded, the atlas; otherwise, <see langword="null"/>.</returns>
    public static string IconAtlasName(string name)
    {
        try {
            Futile.atlasManager.LoadImage($"icon_{name}");

            return $"icon_{name}";
        } catch {
            return "Futile_White";
        }
    }
}
