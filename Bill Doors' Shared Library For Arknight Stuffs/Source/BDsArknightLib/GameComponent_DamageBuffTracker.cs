using System.Collections.Generic;
using System.Text;
using Verse;

namespace BDsArknightLib
{
    public class GameComponent_DamageBuffTracker : GameComponent
    {
        public static GameComponent_DamageBuffTracker Tracker => Current.Game?.GetComponent<GameComponent_DamageBuffTracker>();

        public GameComponent_DamageBuffTracker(Game game)
        {
        }
        Dictionary<Pawn, DamageBuffWorker> DamageBuffList = new Dictionary<Pawn, DamageBuffWorker>();
        public Dictionary<Pawn, DamageBuffWorker> DamageBuff_Duration_ListReadOnly => DamageBuffList;

        List<Pawn> PawnsList;
        List<DamageBuffWorker> DBCTL;

        StringBuilder stb = new StringBuilder();

        public DamageBuffWorker CheckForDamageBuff(Pawn pawn)
        {
            return CheckForDamageBuff(pawn, out _, out _);
        }

        public DamageBuffWorker CheckForDamageBuff(Pawn pawn, out int validUntil, out int charges)
        {
            validUntil = 0;
            charges = 0;
            if (DamageBuffList.ContainsKey(pawn))
            {
                var v = DamageBuffList[pawn];
                if (v.validUntil > 0 && Find.TickManager.TicksGame > v.validUntil)
                {
                    DamageBuffList.Remove(pawn);
                    return null;
                }

                if (v.charges == 0)
                {
                    DamageBuffList.Remove(pawn);
                }

                validUntil = v.validUntil;
                charges = v.charges;
                return v;
            }
            return null;
        }

        public void RegisterDamageBuffDuration(Pawn pawn, DamageBuffWorker tracker, HediffDef trackerHediff = null, bool replace = true)
        {
            if (DamageBuffList.ContainsKey(pawn))
            {
                if (replace || tracker.OverrideHeirachy > DamageBuffList[pawn].OverrideHeirachy) ;
                DamageBuffList[pawn] = tracker;
                return;
            }
            DamageBuffList.Add(pawn, tracker);
            if (trackerHediff != null && trackerHediff.hediffClass == typeof(Hediff_DamageBuffVisualizer)) pawn.health.AddHediff(trackerHediff);
        }

        public void DeregisterDamageBuff(Pawn pawn)
        {
            List<Hediff_DamageBuffVisualizer> list = new List<Hediff_DamageBuffVisualizer>();
            pawn.health.hediffSet.GetHediffs(ref list);
            foreach (var h in list)
            {
                h.ImproperlyRemoved = false;
                pawn.health.RemoveHediff(h);
            }
            DamageBuffList.Remove(pawn);
        }

        public void SelfCheck()
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref DamageBuffList, "DamageBuffList", LookMode.Reference, LookMode.Deep, ref PawnsList, ref DBCTL);

        }
    }
}
