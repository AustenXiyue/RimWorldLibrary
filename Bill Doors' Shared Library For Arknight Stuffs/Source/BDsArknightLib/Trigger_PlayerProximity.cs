using Verse;
using Verse.AI.Group;
using RimWorld;

namespace BDsArknightLib
{
    public class Trigger_PlayerProximity : Trigger
    {
        int tick = 0;

        protected TriggerData_PlayerProximity Data => (TriggerData_PlayerProximity)data;

        public Trigger_PlayerProximity(IntVec3 cell, float radius)
        {
            data = new TriggerData_PlayerProximity(cell, radius);
        }

        public override bool ActivateOn(Lord lord, TriggerSignal signal)
        {
            if (signal.type == TriggerSignalType.Tick)
            {
                tick++;
                if (tick > 90) tick = 0;
                if (tick == 90)
                {
                    foreach (var p in lord.Map.mapPawns.PawnsInFaction(Faction.OfPlayer))
                    {
                        if (p.Position.DistanceTo(Data.cell) < Data.radius) return true;
                    }
                }
            }
            return false;
        }
    }

    public class TriggerData_PlayerProximity : TriggerData
    {
        public TriggerData_PlayerProximity()
        {

        }

        public TriggerData_PlayerProximity(IntVec3 cell, float radius)
        {
            this.cell = cell;
            this.radius = radius;
        }


        public IntVec3 cell;
        public float radius;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref cell, "cell");
            Scribe_Values.Look(ref radius, "radius");
        }
    }
}
