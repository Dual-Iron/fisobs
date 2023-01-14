using Fisobs.Properties;
using Fisobs.Creatures;
using System.Collections.Generic;
using static PathCost.Legality;
using CreatureType = CreatureTemplate.Type;
using Fisobs.Sandbox;

namespace Mosquitoes
{
    sealed class MosquitoCritob : Critob2
    {
        public MosquitoCritob() : base(EnumExt_Mosquito.Mosquito)
        {
            RegisterUnlock(KillScore.Configurable(defaultScore: 3), EnumExt_Mosquito.MosquitoUnlock, parent: MultiplayerUnlocks.SandboxUnlockID.BigNeedleWorm, data: 0);
        }

        public override CreatureTemplate CreateTemplate()
        {
            // CreatureFormula does most of the ugly work for you when creating a new CreatureTemplate,
            // but you can construct a CreatureTemplate manually if you need to.

            CreatureTemplate t = new CreatureFormula(this, "Mosquito") {
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

            Relationships self = new(EnumExt_Mosquito.Mosquito);

            foreach (var template in StaticWorld.creatureTemplates) {
                if (template.quantified) {
                    self.Ignores(template.type);
                    self.IgnoredBy(template.type);
                }
            }

            self.IsInPack(EnumExt_Mosquito.Mosquito, 1f);

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

        public override ArtificialIntelligence GetRealizedAI(AbstractCreature acrit)
        {
            return new MosquitoAI(acrit, (Mosquito)acrit.realizedCreature);
        }

        public override Creature GetRealizedCreature(AbstractCreature acrit)
        {
            return new Mosquito(acrit);
        }

        public override ItemProperties? Properties(PhysicalObject forObject)
        {
            // If you don't need the `forObject` parameter, store one ItemProperties instance as a static object and return that.
            // The CentiShields example demonstrates this.
            if (forObject is Mosquito mosquito) {
                return new MosquitoProperties(mosquito);
            }

            return null;
        }
    }

    sealed class MosquitoProperties : ItemProperties
    {
        private readonly Mosquito mosquito;

        public MosquitoProperties(Mosquito mosquito)
        {
            this.mosquito = mosquito;
        }

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            if (mosquito.State.alive) {
                grabability = Player.ObjectGrabability.CantGrab;
            } else {
                grabability = Player.ObjectGrabability.OneHand;
            }
        }

        public override void Meat(Player player, ref bool meat)
        {
            meat = true;
        }
    }
}
