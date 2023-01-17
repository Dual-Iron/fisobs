using DevInterface;
using Fisobs.Creatures;
using Fisobs.Properties;
using Fisobs.Sandbox;
using RWCustom;
using System.Collections.Generic;
using UnityEngine;
using static PathCost.Legality;
using CreatureType = CreatureTemplate.Type;

namespace Mosquitoes;

sealed class MosquitoCritob : Critob
{
    public static readonly CreatureType Mosquito = new("Mosquito", true);
    public static readonly MultiplayerUnlocks.SandboxUnlockID MosquitoUnlock = new("MosquitoUnlock", true);

    public MosquitoCritob() : base(Mosquito)
    {
        LoadedPerformanceCost = 20f;
        SandboxPerformanceCost = new(linear: 0.6f, exponential: 0.1f);
        ShelterDanger = ShelterDanger.Safe;
        CreatureName = "Blood Sucker";

        // Not calling a `SpawnsForX` method here will prevent the creature from spawning for that character.
        ExpeditionInfo = new() { Points = 3 };
        ExpeditionInfo.SpawnsForWhite(30);
        ExpeditionInfo.SpawnsForYellow(20);
        ExpeditionInfo.SpawnsForRed(50);
        ExpeditionInfo.SpawnsForGourmand(60);
        ExpeditionInfo.SpawnsForArtificer(90);
        ExpeditionInfo.SpawnsForRivulet(80);
        ExpeditionInfo.SpawnsForSpear(80);
        ExpeditionInfo.SpawnsForSaint(20);

        RegisterUnlock(killScore: 3, MosquitoUnlock, parent: MultiplayerUnlocks.SandboxUnlockID.BigNeedleWorm, data: 0);
    }

    public override CreatureTemplate CreateTemplate()
    {
        // CreatureFormula does most of the ugly work for you when creating a new CreatureTemplate,
        // but you can construct a CreatureTemplate manually if you need to.

        CreatureTemplate t = new CreatureFormula(this) {
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 0.25f),
            HasAI = true,
            InstantDeathDamage = 1,
            Pathing = PreBakedPathing.Ancestral(CreatureType.Fly),
            TileResistances = new() {
                Air = new(1, Allowed),
            },
            ConnectionResistances = new() {
                Standard = new(1, Allowed),
                OpenDiagonal = new(1, Allowed),
                ShortCut = new(1, Allowed),
                NPCTransportation = new(10, Allowed),
                OffScreenMovement = new(1, Allowed),
                BetweenRooms = new(1, Allowed),
            },
            DamageResistances = new() {
                Base = 0.95f,
            },
            StunResistances = new() {
                Base = 0.6f,
            }
        }.IntoTemplate();

        // The below properties are derived from vanilla creatures, so you should have your copy of the decompiled source code handy.

        // Some notes on the fields of CreatureTemplate:

        // offScreenSpeed       how fast the creature moves between abstract rooms
        // abstractLaziness     how long it takes the creature to start migrating
        // smallCreature        determines if rocks instakill, if large predators ignore it, etc
        // dangerToPlayer       DLLs are 0.85, spiders are 0.1, pole plants are 0.5
        // waterVision          0..1 how well the creature can see through water
        // throughSurfaceVision 0..1 how well the creature can see through water surfaces
        // movementBasedVision  0..1 bonus to vision for moving creatures
        // lungCapacity         ticks until the creature falls unconscious from drowning
        // quickDeath           determines if the creature should die as determined by Creature.Violence(). if false, you must define custom death logic
        // saveCreature         determines if the creature is saved after a cycle ends. false for overseers and garbage worms
        // hibernateOffScreen   true for deer, miros birds, leviathans, vultures, and scavengers
        // bodySize             batflies are 0.1, eggbugs are 0.4, DLLs are 5.5, slugcats are 1

        t.offScreenSpeed = 0.1f;
        t.abstractedLaziness = 200;
        t.roamBetweenRoomsChance = 0.07f;
        t.bodySize = 0.5f;
        t.stowFoodInDen = true;
        t.shortcutSegments = 2;
        t.grasps = 1;
        t.visualRadius = 800f;
        t.movementBasedVision = 0.65f;
        t.communityInfluence = 0.1f;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        t.waterPathingResistance = 2f;
        t.canFly = true;
        t.meatPoints = 3;
        t.dangerousToPlayer = 0.4f;

        return t;
    }

    public override void EstablishRelationships()
    {
        // You can use StaticWorld.EstablishRelationship, but the Relationships class exists to make this process more ergonomic.

        Relationships self = new(Mosquito);

        foreach (var template in StaticWorld.creatureTemplates) {
            if (template.quantified) {
                self.Ignores(template.type);
                self.IgnoredBy(template.type);
            }
        }

        self.IsInPack(Mosquito, 1f);

        self.Eats(CreatureType.Slugcat, 0.4f);
        self.Eats(CreatureType.Scavenger, 0.6f);
        self.Eats(CreatureType.LizardTemplate, 0.3f);
        self.Eats(CreatureType.CicadaA, 0.4f);

        self.Intimidates(CreatureType.LizardTemplate, 0.35f);
        self.Intimidates(CreatureType.CicadaA, 0.3f);

        self.AttackedBy(CreatureType.Slugcat, 0.2f);
        self.AttackedBy(CreatureType.Scavenger, 0.2f);

        self.EatenBy(CreatureType.BigSpider, 0.35f);

        self.Fears(CreatureType.Spider, 0.2f);
        self.Fears(CreatureType.BigSpider, 0.2f);
        self.Fears(CreatureType.SpitterSpider, 0.6f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
    {
        return new MosquitoAI(acrit, (Mosquito)acrit.realizedCreature);
    }

    public override Creature CreateRealizedCreature(AbstractCreature acrit)
    {
        return new Mosquito(acrit);
    }

    public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allowed)
    {
        // DLLs don't travel through shortcuts that start and end in the same room—they only travel through room exits.
        // To emulate this behavior, use something like:

        //ShortcutData.Type n = ShortcutData.Type.Normal;
        //if (connection.type == MovementConnection.MovementType.ShortCut) {
        //    allowed &=
        //        connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == n ||
        //        connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == n
        //        ;
        //} else if (connection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze) {
        //    allowed &=
        //        map.room.GetTile(connection.startCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.StartTile).shortCutType == n ||
        //        map.room.GetTile(connection.destinationCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.DestTile).shortCutType == n
        //        ;
        //}
    }

    public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allowed)
    {
        // Large creatures like vultures, miros birds, and DLLs need 2 tiles of free space to move around in. Leviathans need 4! None of them can fit in one-tile tunnels.
        // To emulate this behavior, use something like:

        //allowed &= map.IsFreeSpace(tilePos, tilesOfFreeSpace: 2);

        // DLLs can fit into shortcuts despite being fat.
        // To emulate this behavior, use something like:

        //allowed |= map.room.GetTile(tilePos).Terrain == Room.Tile.TerrainType.ShortcutEntrance;
    }

    public override IEnumerable<string> WorldFileAliases()
    {
        yield return "mosq";
        yield return "bloodsucker";
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        yield return RoomAttractivenessPanel.Category.Flying;
        yield return RoomAttractivenessPanel.Category.LikesWater;
        yield return RoomAttractivenessPanel.Category.LikesOutside;
    }

    public override string DevtoolsMapName(AbstractCreature acrit)
    {
        return "mqto";
    }

    public override Color DevtoolsMapColor(AbstractCreature acrit)
    {
        // Default would return the mosquito's icon color (which is gray), which is fine, but red is better.
        return new Color(.7f, .4f, .4f);
    }

    public override ItemProperties? Properties(Creature crit)
    {
        // If you don't need the `forObject` parameter, store one ItemProperties instance as a static object and return that.
        // The CentiShields example demonstrates this.
        if (crit is Mosquito mosquito) {
            return new MosquitoProperties(mosquito);
        }

        return null;
    }
}
