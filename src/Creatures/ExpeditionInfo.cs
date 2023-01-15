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

    internal readonly Dictionary<SlugcatStats.Name, int> spawns = new() {
        [SlugcatStats.Name.White] = 0,
        [SlugcatStats.Name.Yellow] = 0,
        [SlugcatStats.Name.Red] = 0,
        [MoreSlugcatsEnums.SlugcatStatsName.Gourmand] = 0,
        [MoreSlugcatsEnums.SlugcatStatsName.Artificer] = 0,
        [MoreSlugcatsEnums.SlugcatStatsName.Rivulet] = 0,
        [MoreSlugcatsEnums.SlugcatStatsName.Spear] = 0,
        [MoreSlugcatsEnums.SlugcatStatsName.Saint] = 0,
    };

    /// <summary>
    /// Creates a new instance of the <see cref="ExpeditionInfo"/> class.
    /// </summary>
    public ExpeditionInfo() { }

    /// <summary>Gets the maximum number of spawns on Survivor.</summary>
    public int SpawnsForWhite() => spawns[SlugcatStats.Name.White];
    /// <summary>Gets the maximum number of spawns on Monk.</summary>
    public int SpawnsForYellow() => spawns[SlugcatStats.Name.Yellow];
    /// <summary>Gets the maximum number of spawns on Hunter.</summary>
    public int SpawnsForRed() => spawns[SlugcatStats.Name.Red];
    /// <summary>Gets the maximum number of spawns on Gourmand.</summary>
    public int SpawnsForGourmand() => spawns[MoreSlugcatsEnums.SlugcatStatsName.Gourmand];
    /// <summary>Gets the maximum number of spawns on Artificer.</summary>
    public int SpawnsForArtificer() => spawns[MoreSlugcatsEnums.SlugcatStatsName.Artificer];
    /// <summary>Gets the maximum number of spawns on Rivulet.</summary>
    public int SpawnsForRivulet() => spawns[MoreSlugcatsEnums.SlugcatStatsName.Rivulet];
    /// <summary>Gets the maximum number of spawns on Spearmaster.</summary>
    public int SpawnsForSpear() => spawns[MoreSlugcatsEnums.SlugcatStatsName.Spear];
    /// <summary>Gets the maximum number of spawns on Saint.</summary>
    public int SpawnsForSaint() => spawns[MoreSlugcatsEnums.SlugcatStatsName.Saint];
    /// <summary>Gets the maximum number of spawns on the given slugcat class.</summary>
    public int SpawnsFor(SlugcatStats.Name name) => spawns.TryGetValue(name, out int value) ? value : 0;

    /// <summary>Sets the maximum number of spawns on Survivor.</summary>
    public void SpawnsForWhite(int value) => spawns[SlugcatStats.Name.White] = value;
    /// <summary>Sets the maximum number of spawns on Monk.</summary>
    public void SpawnsForYellow(int value) => spawns[SlugcatStats.Name.Yellow] = value;
    /// <summary>Sets the maximum number of spawns on Red.</summary>
    public void SpawnsForRed(int value) => spawns[SlugcatStats.Name.Red] = value;
    /// <summary>Sets the maximum number of spawns on Gourmand.</summary>
    public void SpawnsForGourmand(int value) => spawns[MoreSlugcatsEnums.SlugcatStatsName.Gourmand] = value;
    /// <summary>Sets the maximum number of spawns on Artificer.</summary>
    public void SpawnsForArtificer(int value) => spawns[MoreSlugcatsEnums.SlugcatStatsName.Artificer] = value;
    /// <summary>Sets the maximum number of spawns on Rivulet.</summary>
    public void SpawnsForRivulet(int value) => spawns[MoreSlugcatsEnums.SlugcatStatsName.Rivulet] = value;
    /// <summary>Sets the maximum number of spawns on Spearmaster.</summary>
    public void SpawnsForSpear(int value) => spawns[MoreSlugcatsEnums.SlugcatStatsName.Spear] = value;
    /// <summary>Sets the maximum number of spawns on Saint.</summary>
    public void SpawnsForSaint(int value) => spawns[MoreSlugcatsEnums.SlugcatStatsName.Saint] = value;
    /// <summary>Sets the maximum number of spawns on the given slugcat class.</summary>
    public void SpawnsFor(SlugcatStats.Name name, int value) => spawns[name] = value;
}
