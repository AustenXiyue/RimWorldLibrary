using Verse;
using RimWorld;
using System.Collections.Generic;
using Verse.AI.Group;
using System;
using UnityEngine;

namespace BDsArknightLib
{
    public class LordJob_HostileWhenProvoked : LordJob
    {
        private Faction faction;

        private IntVec3 baseCenter;
        private float radius;

        public LordJob_HostileWhenProvoked()
        {
        }

        public LordJob_HostileWhenProvoked(Faction faction, IntVec3 baseCenter, float radius)
        {
            this.faction = faction;
            this.baseCenter = baseCenter;
            this.radius = radius;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_DefendBase lordToil_DefendBase = (LordToil_DefendBase)(stateGraph.StartingToil = new LordToil_DefendBase(baseCenter));
            LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony(attackDownedIfStarving: false);
            stateGraph.AddToil(lordToil_AssaultColony);

            Transition transition3 = new Transition(lordToil_DefendBase, lordToil_AssaultColony);
            transition3.AddTrigger(new Trigger_FractionPawnsLost(0.2f));
            transition3.AddTrigger(new Trigger_PawnKilled());
            transition3.AddTrigger(new Trigger_TicksPassed(251999));
            transition3.AddTrigger(new Trigger_UrgentlyHungry());
            transition3.AddTrigger(new Trigger_PlayerProximity(baseCenter, radius));
            transition3.AddTrigger(new Trigger_ChanceOnPlayerHarmNPCBuilding(0.2f));
            transition3.AddTrigger(new Trigger_BecamePlayerEnemy());
            transition3.AddPostAction(new TransitionAction_Custom((Action)delegate { faction.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Hostile); }));
            TaggedString taggedString = "MessageDefendersAttacking".Translate(faction.def.pawnsPlural, faction.Name, Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
            transition3.AddPreAction(new TransitionAction_Message(taggedString, MessageTypeDefOf.ThreatBig));
            stateGraph.AddTransition(transition3);
            return stateGraph;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref faction, "faction");
            Scribe_Values.Look(ref baseCenter, "baseCenter");
            Scribe_Values.Look(ref radius, "radius");
        }
    }
}
