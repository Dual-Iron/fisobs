using MoreSlugcats;
using System.Collections.Generic;

namespace Fisobs.Creatures;

/// <summary>
/// Stores creature information relevant to Expedition.
/// </summary>
public class ExpeditionInfo
{
    /// <summary>
    /// How many points the player is awarded for killing the creature.
    /// </summary>
    /// <value>The default value is 0. Examples: eggbugs score 2 points, scavengers score 6, green lizards score 10, and red lizards score 25.</value>
    public int Points { get; set; }

    internal readonly Dictionary<string, int> spawns = new() {
        ["White"] = 0,
        ["Yellow"] = 0,
        ["Red"] = 0,
        ["Gourmand"] = 0,
        ["Artificer"] = 0,
        ["Rivulet"] = 0,
        ["Spear"] = 0,
        ["Saint"] = 0,
    };

    /// <summary>
    /// Creates a new instance of the <see cref="ExpeditionInfo"/> class.
    /// </summary>
    public ExpeditionInfo() { }

    /// <summary>Gets the maximum number of spawns on Survivor.</summary>
    public int SpawnsForWhite() => spawns["White"];
    /// <summary>Gets the maximum number of spawns on Monk.</summary>
    public int SpawnsForYellow() => spawns["Yellow"];
    /// <summary>Gets the maximum number of spawns on Hunter.</summary>
    public int SpawnsForRed() => spawns["Red"];
    /// <summary>Gets the maximum number of spawns on Gourmand.</summary>
    public int SpawnsForGourmand() => spawns["Gourmand"];
    /// <summary>Gets the maximum number of spawns on Artificer.</summary>
    public int SpawnsForArtificer() => spawns["Artificer"];
    /// <summary>Gets the maximum number of spawns on Rivulet.</summary>
    public int SpawnsForRivulet() => spawns["Rivulet"];
    /// <summary>Gets the maximum number of spawns on Spearmaster.</summary>
    public int SpawnsForSpear() => spawns["Spear"];
    /// <summary>Gets the maximum number of spawns on Saint.</summary>
    public int SpawnsForSaint() => spawns["Saint"];
    /// <summary>Gets the maximum number of spawns on the given slugcat class.</summary>
    public int SpawnsFor(string name) => spawns.TryGetValue(name, out int value) ? value : 0;

    /// <summary>Sets the maximum number of spawns on Survivor.</summary>
    public void SpawnsForWhite(int value) => spawns["White"] = value;
    /// <summary>Sets the maximum number of spawns on Monk.</summary>
    public void SpawnsForYellow(int value) => spawns["Yellow"] = value;
    /// <summary>Sets the maximum number of spawns on Red.</summary>
    public void SpawnsForRed(int value) => spawns["Red"] = value;
    /// <summary>Sets the maximum number of spawns on Gourmand.</summary>
    public void SpawnsForGourmand(int value) => spawns["Gourmand"] = value;
    /// <summary>Sets the maximum number of spawns on Artificer.</summary>
    public void SpawnsForArtificer(int value) => spawns["Artificer"] = value;
    /// <summary>Sets the maximum number of spawns on Rivulet.</summary>
    public void SpawnsForRivulet(int value) => spawns["Rivulet"] = value;
    /// <summary>Sets the maximum number of spawns on Spearmaster.</summary>
    public void SpawnsForSpear(int value) => spawns["Spear"] = value;
    /// <summary>Sets the maximum number of spawns on Saint.</summary>
    public void SpawnsForSaint(int value) => spawns["Saint"] = value;
    /// <summary>Sets the maximum number of spawns on the given slugcat class.</summary>
    public void SpawnsFor(string name, int value) => spawns[name] = value;
}
