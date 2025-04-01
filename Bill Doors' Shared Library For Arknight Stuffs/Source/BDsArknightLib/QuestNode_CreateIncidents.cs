using Verse;
using RimWorld;
using RimWorld.QuestGen;
using RimWorld.Planet;

namespace BDsArknightLib
{
    public class QuestNode_CreateIncidents : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> inSignal;

        [NoTranslate]
        public SlateRef<string> inSignalDisable;

        public SlateRef<IncidentDef> incidentDef;

        public SlateRef<int> startOffsetTicks;

        public SlateRef<int> duration;

        public SlateRef<float> points;

        public SlateRef<Faction> faction;

        public SlateRef<MapParent> mapParent;

        protected override bool TestRunInt(Slate slate)
        {
            if (incidentDef.GetValue(slate) == null || points.GetValue(slate) < incidentDef.GetValue(slate).minThreatPoints || points.GetValue(slate) > incidentDef.GetValue(slate).maxThreatPoints)
            {
                return false;
            }
            return true;
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            Quest quest = QuestGen.quest;
            var delay = startOffsetTicks.GetValue(slate);
            string inSignalS = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");

            if (delay > 0)
            {
                QuestPart_Delay questPart_Delay = new QuestPart_Delay
                {
                    delayTicks = startOffsetTicks.GetValue(slate),
                    inSignalEnable = inSignalS,
                    inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate))
                };
                questPart_Delay.debugLabel = questPart_Delay.delayTicks.ToStringTicksToDays() + "_" + incidentDef.ToString();
                quest.AddPart(questPart_Delay);
                inSignalS = questPart_Delay.OutSignalCompleted;
            }

            QuestPart_Incident questPart_Incident = new QuestPart_Incident
            {
                incident = incidentDef.GetValue(slate),
                inSignal = inSignalS
            };
            questPart_Incident.SetIncidentParmsAndRemoveTarget(new IncidentParms
            {
                forced = true,
                points = points.GetValue(slate),
                faction = faction.GetValue(slate)
            });
            questPart_Incident.MapParent = mapParent.GetValue(slate);
            quest.AddPart(questPart_Incident);
        }
    }
}
