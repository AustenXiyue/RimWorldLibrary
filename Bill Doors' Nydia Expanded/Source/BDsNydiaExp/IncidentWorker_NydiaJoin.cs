using RimWorld;
using BillDoorsPredefinedCharacter;
using Verse;
using Verse.Noise;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using RimWorld.Planet;

namespace BDsNydiaExp
{
    public class IncidentWorker_NydiaJoin : IncidentWorker_WandererJoin
    {
        ModExtension_PDCDef ext => def.GetModExtension<ModExtension_PDCDef>();
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && ext.def.AtHome();
        }

        public override Pawn GeneratePawn()
        {
            return ext.def.GetPawn(null, null, Faction.OfPlayer, false);
        }
    }

    public class StorytellerComp_TriggeredPct : StorytellerComp
    {
        private StorytellerCompProperties_TriggeredPct Props => (StorytellerCompProperties_TriggeredPct)props;

        public override void Notify_PawnEvent(Pawn p, AdaptationEvent ev, DamageInfo? dinfo = null)
        {
            if (Props.pawnDef != null && (!Props.pawnDef.AtHome() || !BDPDC_Mod.LoadedAppearModes[Props.pawnDef].Contains(Props.methodIdentifier))) return;

            if (!p.RaceProps.Humanlike || !p.IsColonist || (ev != AdaptationEvent.Died && ev != AdaptationEvent.Kidnapped && ev != AdaptationEvent.LostBecauseMapClosed && ev != 0))
            {
                return;
            }

            foreach (var map in Find.Maps.Where(m => m.IsPlayerHome))
            {
                float totalPawns = 0;
                var downedPawns = 0;

                var pawns = map.mapPawns.FreeColonists;

                foreach (Pawn item in pawns)
                {
                    if (item.Faction?.IsPlayer ?? false)
                    {
                        totalPawns++;
                        //There're millions of ways to safely bypass this. But I don't care.
                        if (PawnValidator(item))
                        {
                            downedPawns++;
                        }
                    }
                }

                //Only count fresh (or rotting, just in case you're in big trouble), spawned corpses. So that aged corpse or buried corpse doesn't count.
                totalPawns += map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Corpse)).Cast<Corpse>().Where(CorpseValidator).Count();
                if (downedPawns / totalPawns > Props.pctDowned)
                {

                    IncidentParms parms = StorytellerUtility.DefaultParmsNow(Props.incident.category, map);
                    if (Props.incident.Worker.CanFireNow(parms))
                    {
                        QueuedIncident qi = new QueuedIncident(new FiringIncident(Props.incident, this, parms), Find.TickManager.TicksGame + Props.delayTicks);
                        Find.Storyteller.incidentQueue.Add(qi);
                    }

                    return;
                }
            }

            bool CorpseValidator(Corpse c)
            {
                if (c.InnerPawn.IsColonist)
                {
                    //There's a possible edge case when a single mother gives birth to two children, both of which stillborn...and Nydia kicks the door open.
                    if (c.InnerPawn.DevelopmentalStage.Baby()) return false;
                    if (c.GetRotStage() == RotStage.Dessicated) return false;
                    if (ModsConfig.AnomalyActive && DeathRefusalUtility.HasPlayerControlledDeathRefusal(c.InnerPawn)) return false;
                    return true;
                }
                return false;
            }

            bool PawnValidator(Pawn item)
            {
                if (item.Dead)
                {
                    return true;
                }
                if (ModsConfig.BiotechActive && item.Deathresting && SanguophageUtility.ShouldBeDeathrestingOrInComaInsteadOfDead(item)) return false;
                if (item.Downed)
                {
                    foreach (var h in item.health.hediffSet.hediffs)
                    {
                        if (h is Hediff_Injury || h is Hediff_MissingPart) continue;
                        if (h.TryGetComp<HediffComp_Immunizable>() is HediffComp_Immunizable cpim && cpim.FullyImmune) return false;
                        var hp = Props.hediffBlacklist.Find(sp => sp.defName == h.def.defName);
                        if (hp != null && (hp.severityRange == null || hp.severityRange.Includes(h.Severity))) return false;
                    }
                    return true;
                }
                return false;
            }
        }

        public override string ToString()
        {
            return base.ToString() + " " + Props.incident;
        }
    }
    public class StorytellerCompProperties_TriggeredPct : StorytellerCompProperties
    {
        public IncidentDef incident;

        public int delayTicks = 60;

        public float pctDowned = 0.65f;

        public PredefinedCharacterParmDef pawnDef;

        public string methodIdentifier;

        public List<HediffSeverityPair> hediffBlacklist = new List<HediffSeverityPair>();

        public StorytellerCompProperties_TriggeredPct()
        {
            compClass = typeof(StorytellerComp_TriggeredPct);
        }
    }

    public class HediffSeverityPair
    {
        public string defName;

        public FloatRange severityRange;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.Name == "li")
            {
                defName = xmlRoot.FirstChild.Value;
            }
            else
            {
                defName = xmlRoot.Name;
                severityRange = ParseHelper.FromString<FloatRange>(xmlRoot.FirstChild.Value);
            }
        }
    }
}

