// This code was made by ratrat (https://github.com/ratrat44) and is included in Fisobs with his permission.

using RWCustom;
using System.Linq;
using UnityEngine;
using static CreatureTemplate.Relationship.Type;

namespace Mosquitoes;

sealed class MosquitoAI : ArtificialIntelligence, IUseARelationshipTracker
{
    enum Behavior
    {
        Idle,
        Swarm,
        Flee,
        EscapeRain,
        Hunt
    }

    sealed class MosquitoTrackedState : RelationshipTracker.TrackedCreatureState
    {
        public int prickedTime;
    }

    public Mosquito bug;
    public int tiredOfHuntingCounter;
    public AbstractCreature? tiredOfHuntingCreature;
    private Behavior behavior;
    private int behaviorCounter;
    private WorldCoordinate tempIdlePos;

    public MosquitoAI(AbstractCreature acrit, Mosquito bug) : base(acrit, acrit.world)
    {
        this.bug = bug;
        bug.AI = this;
        AddModule(new StandardPather(this, acrit.world, acrit));
        pathFinder.stepsPerFrame = 20;
        AddModule(new Tracker(this, 10, 10, 600, 0.5f, 5, 5, 10));
        AddModule(new ThreatTracker(this, 3));
        AddModule(new RainTracker(this));
        AddModule(new DenFinder(this, acrit));
        AddModule(new NoiseTracker(this, tracker));
        AddModule(new PreyTracker(this, 5, 1f, 5f, 150f, 0.05f));
        AddModule(new UtilityComparer(this));
        AddModule(new RelationshipTracker(this, tracker));
        var smoother = new FloatTweener.FloatTweenUpAndDown(new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Lerp, 0.5f), new FloatTweener.FloatTweenBasic(FloatTweener.TweenType.Tick, 0.005f));
        utilityComparer.AddComparedModule(threatTracker, smoother, 1f, 1.1f);
        utilityComparer.AddComparedModule(rainTracker, null, 1f, 1.1f);
        utilityComparer.AddComparedModule(preyTracker, null, 0.4f, 1.1f);
        noiseTracker.hearingSkill = 0.5f;
        behavior = Behavior.Idle;
    }

    public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation otherCreature)
    {
        // If we're wandering aimlessly and we find another pack member, stop what we're doing and hang with them
        if (behavior == Behavior.Swarm && RandomPackMember() == null) {
            behaviorCounter = 0;
        }
    }

    AIModule? IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
    {
        if (relationship.type == Eats) return preyTracker;
        if (relationship.type == Afraid) return threatTracker;
        return null;
    }

    RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
    {
        return new MosquitoTrackedState();
    }

    CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
    {
        if (dRelation.state is not MosquitoTrackedState state) return default;

        if (dRelation.trackerRep.VisualContact) {
            dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
        }

        if (!dRelation.state.alive) {
            return new(Ignores, 0f);
        }

        if (dRelation.trackerRep.representedCreature.realizedObject is Creature c && c.State.alive && bug.grasps[0]?.grabbed == c) {
            state.prickedTime += 2;
            preyTracker.ForgetPrey(tiredOfHuntingCreature);
        } else {
            state.prickedTime -= 1;
        }

        if (state.prickedTime > 0) {
            return new(Afraid, 0.5f);
        }

        return StaticRelationship(dRelation.trackerRep.representedCreature);
    }

    public override void Update()
    {
        base.Update();

        if (bug.room == null) {
            return;
        }

        pathFinder.walkPastPointOfNoReturn = stranded
            || denFinder.GetDenPosition() is not WorldCoordinate denPos
            || !pathFinder.CoordinatePossibleToGetBackFrom(denPos)
            || threatTracker.Utility() > 0.95f;

        utilityComparer.GetUtilityTracker(threatTracker).weight = Custom.LerpMap(threatTracker.ThreatOfTile(creature.pos, true), 0.1f, 2f, 0.1f, 1f, 0.5f);

        if (utilityComparer.HighestUtility() < 0.02f && (behavior != Behavior.Hunt || preyTracker.MostAttractivePrey == null)) {
            if (behavior is not Behavior.Idle or Behavior.Swarm) {
                behaviorCounter = 0;
                behavior = Random.value < 0.1f ? Behavior.Idle : Behavior.Swarm;
            }
        } else {
            behavior = utilityComparer.HighestUtilityModule() switch {
                ThreatTracker => Behavior.Flee,
                RainTracker => Behavior.EscapeRain,
                PreyTracker => Behavior.Hunt,
                _ => behavior
            };
        }

        switch (behavior) {
            case Behavior.Idle:
                bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.6f + 0.4f * threatTracker.Utility(), 0.01f, 0.016666668f);

                WorldCoordinate coord = new(bug.room.abstractRoom.index, Random.Range(0, bug.room.TileWidth), Random.Range(0, bug.room.TileHeight), -1);
                if (IdleScore(tempIdlePos) > IdleScore(coord)) {
                    tempIdlePos = coord;
                }

                if (IdleScore(tempIdlePos) < IdleScore(pathFinder.GetDestination) + Custom.LerpMap(behaviorCounter, 0f, 300f, 100f, -300f)) {
                    SetDestination(tempIdlePos);
                    behaviorCounter = Random.Range(100, 400);
                    tempIdlePos = new WorldCoordinate(bug.room.abstractRoom.index, Random.Range(0, bug.room.TileWidth), Random.Range(0, bug.room.TileHeight), -1);
                }

                behaviorCounter--;
                break;

            case Behavior.Swarm:
                bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.3f + 0.7f * threatTracker.Utility(), 1f / 100f, 1f / 60f);

                if (behaviorCounter <= 0) {
                    // Try to hang with another pack member, or wander if there are none
                    var other = RandomPackMember();
                    if (other != null) {
                        var newDest = other.BestGuessForPosition();
                        if (newDest.x != -1 && newDest.y != -1) {
                            newDest.x += Random.Range(-10, 10);
                            newDest.y += Random.Range(-10, 10);
                        }
                        creature.abstractAI.SetDestination(newDest);

                        behaviorCounter = Random.Range(50, 100);
                    } else {
                        creature.abstractAI.SetDestination(creature.abstractAI.MigrationDestination);

                        behaviorCounter = Random.Range(200, 400);
                    }
                }

                behaviorCounter--;
                break;

            case Behavior.Flee:
                bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
                creature.abstractAI.SetDestination(threatTracker.FleeTo(creature.pos, 20, 20, true));
                break;

            case Behavior.Hunt:
                bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, .1f);

                if (preyTracker.MostAttractivePrey != null)
                    creature.abstractAI.SetDestination(preyTracker.MostAttractivePrey.BestGuessForPosition());

                tiredOfHuntingCounter++;
                if (tiredOfHuntingCounter > 100) {
                    tiredOfHuntingCreature = preyTracker.MostAttractivePrey?.representedCreature;
                    tiredOfHuntingCounter = 0;
                    preyTracker.ForgetPrey(tiredOfHuntingCreature);
                    tracker.ForgetCreature(tiredOfHuntingCreature);
                }
                break;

            case Behavior.EscapeRain:
                bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 1f, 0.01f, 0.1f);
                if (denFinder.GetDenPosition() is WorldCoordinate den) {
                    creature.abstractAI.SetDestination(den);
                }
                break;
        }
    }

    private Tracker.CreatureRepresentation? RandomPackMember()
    {
        var others = tracker.creatures.Where(r => r.dynamicRelationship.state.alive && r.dynamicRelationship.currentRelationship.type == Pack).ToList();
        if (others.Any()) {
            return others[Random.Range(0, others.Count)];
        }
        return null;
    }

    private float IdleScore(WorldCoordinate coord)
    {
        if (coord.NodeDefined || coord.room != creature.pos.room || !pathFinder.CoordinateReachableAndGetbackable(coord) || bug.room.aimap.getAItile(coord).acc == AItile.Accessibility.Solid) {
            return float.MaxValue;
        }
        float result = 1f;
        if (bug.room.aimap.getAItile(coord).narrowSpace) {
            result += 100f;
        }
        result += threatTracker.ThreatOfTile(coord, true) * 1000f;
        result += threatTracker.ThreatOfTile(bug.room.GetWorldCoordinate((bug.room.MiddleOfTile(coord) + bug.room.MiddleOfTile(creature.pos)) / 2f), true) * 1000f;
        for (int i = 0; i < noiseTracker.sources.Count; i++) {
            result += Custom.LerpMap(Vector2.Distance(bug.room.MiddleOfTile(coord), noiseTracker.sources[i].pos), 40f, 400f, 100f, 0f);
        }
        return result;
    }

    public override bool WantToStayInDenUntilEndOfCycle()
    {
        return rainTracker.Utility() > 0.01f;
    }

    public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
    {
        return otherCreature.creatureTemplate.smallCreature
            ? new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0f, false)
            : new Tracker.ElaborateCreatureRepresentation(tracker, otherCreature, 1f, 3);
    }

    public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
    {
        float val = Mathf.Max(0f, threatTracker.ThreatOfTile(coord.destinationCoord, false) - threatTracker.ThreatOfTile(creature.pos, false));
        return new PathCost(cost.resistance + Custom.LerpMap(val, 0f, 1.5f, 0f, 10000f, 5f), cost.legality);
    }
}
