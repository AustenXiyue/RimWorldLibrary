using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace BDsArknightLib
{
    public class GameComponent_InvoluntaryMovingTracker : GameComponent
    {
        public static GameComponent_InvoluntaryMovingTracker Tracker => Current.Game?.GetComponent<GameComponent_InvoluntaryMovingTracker>();

        public GameComponent_InvoluntaryMovingTracker(Game game)
        {
        }
        List<InvoluntaryMovingData> datas = new List<InvoluntaryMovingData>();

        List<InvoluntaryMovingData> datasPendingRemove = new List<InvoluntaryMovingData>();

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref datas, "data", LookMode.Deep);
        }

        public InvoluntaryMovingData DataForPawn(Pawn pawn)
        {
            return datas.Find(d => d.pawn == pawn);
        }

        public void RemoveData(InvoluntaryMovingData data)
        {
            datasPendingRemove.Add(data);
        }

        public void AddData(InvoluntaryMovingData data)
        {
            DataForPawn(data.pawn)?.Stop();
            data.origin = data.pawn.Position;
            datas.Add(data);
        }

        public override void GameComponentTick()
        {
            foreach (var item in datas)
            {
                item.Interval();
            }
            foreach (var item in datasPendingRemove)
            {
                datas.Remove(item);
            }
            datasPendingRemove.Clear();
        }
    }


    public class InvoluntaryMovingData : IExposable
    {
        public Pawn pawn;

        public IntVec3 origin;

        public IntVec3 dest;

        public int ticksPassed = 0;

        public int flightTime = 0;
        public Vector3 originV3 => origin.ToVector3Shifted();
        public Vector3 destV3 => dest.ToVector3Shifted();
        Map map => pawn.Map;
        float pct => Mathf.Sqrt((float)ticksPassed / flightTime);
        public virtual Vector3 drawPosOffset => (destV3 - originV3) * pct;
        protected virtual Vector3 exactPos => drawPosOffset + originV3;
        float Speed => (origin - dest).Magnitude / flightTime;
        float KineticEnergy => Speed * pawn.GetStatValue(StatDefOf.Mass) / pct;
        StunHandler stunner => pawn.stances?.stunner;

        IntVec3 lastPos = IntVec3.Invalid;

        public InvoluntaryMovingData(Pawn pawn, int flightTime, IntVec3 dest)
        {
            this.pawn = pawn;
            this.dest = dest;

            this.flightTime = flightTime;
            origin = pawn.Position;
            stunner.StunFor(flightTime - ticksPassed, null);
        }

        public InvoluntaryMovingData(Pawn pawn, float speed, IntVec3 dest)
        {
            this.pawn = pawn;
            this.dest = dest;

            var dist = (origin - dest).Magnitude;
            flightTime = (dist / speed).SecondsToTicks();
            origin = pawn.Position;
            stunner.StunFor(flightTime - ticksPassed, null);
            map.debugDrawer.FlashLine(pawn.Position, dest);
        }

        public virtual void Interval()
        {
            if (stunner != null && !stunner.Stunned)
            {
                stunner.StunFor(flightTime - ticksPassed, null);
            }

            ticksPassed++;

            var exactCell = exactPos.ToIntVec3();

            if (!exactCell.InBounds(map))
            {
                Stop(true);
                return;
            }
            if (CheckIntercept(exactCell)) return;
            lastPos = exactCell;
            if (pct >= 1)
            {
                Stop();
            }
        }

        bool CheckIntercept(IntVec3 cell)
        {
            if (cell == lastPos) return false;
            List<Thing> thingList = cell.GetThingList(map);
            foreach (var t in thingList)
            {
                if (t == this.pawn) continue;
                if (t.def.Fillage == FillCategory.Full)
                {
                    if (!(t is Building_Door building_Door) || !building_Door.Open)
                    {
                        ticksPassed--;
                        Impact(t, true);
                        return true;
                    }
                }
                if (t is Pawn pawn)
                {
                    float chance = 1;
                    if (pawn.GetPosture() != 0)
                    {
                        chance *= 0.1f;
                    }
                    if (Rand.Chance(chance))
                    {
                        Impact(t);
                        return true;
                    }
                }
                else if (t.def.fillPercent > 0.2f && Rand.Chance(t.def.fillPercent))
                {
                    Impact(t);
                    return true;
                }
            }
            return false;
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Values.Look(ref origin, "origin");
            Scribe_Values.Look(ref dest, "dest");
            Scribe_Values.Look(ref ticksPassed, "ticksPassed");
            Scribe_Values.Look(ref flightTime, "flightTime");
        }

        public virtual void Impact(Thing thing, bool rewind = false)
        {
            Stop(rewind);
        }

        public virtual void Stop(bool rewind = false)
        {
            if (rewind) ticksPassed--;
            pawn.Position = exactPos.ToIntVec3();
            pawn.Notify_Teleported();
            GameComponent_InvoluntaryMovingTracker.Tracker.RemoveData(this);
        }
    }
}
