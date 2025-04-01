using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;

namespace BDsNydiaExp
{
    public class QuestPart_PawnsKilledMapFaction : QuestPartActivable
    {
        public Faction requiredInstigatorFaction;

        public int count;

        public Faction requiredVictimFaction;

        public string outSignalPawnKilled;

        private int killed;

        public override string DescriptionPart => "PawnsKilled: " + killed + " / " + count;

        public override IEnumerable<Faction> InvolvedFactions
        {
            get
            {
                foreach (Faction involvedFaction in base.InvolvedFactions)
                {
                    yield return involvedFaction;
                }
                if (requiredInstigatorFaction != null)
                {
                    yield return requiredInstigatorFaction;
                }
            }
        }

        protected override void Enable(SignalArgs receivedArgs)
        {
            base.Enable(receivedArgs);
            killed = 0;
        }

        public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
        {
            if (State == QuestPartState.Enabled && pawn.Faction == requiredVictimFaction && (requiredInstigatorFaction == null || (dinfo.HasValue && (dinfo.Value.Instigator == null || dinfo.Value.Instigator.Faction == requiredInstigatorFaction))))
            {
                killed++;
                Find.SignalManager.SendSignal(new Signal(outSignalPawnKilled));
                if (killed >= count)
                {
                    Complete();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref requiredInstigatorFaction, "requiredInstigatorFaction");
            Scribe_References.Look(ref requiredVictimFaction, "requiredVictimFaction");
            Scribe_Values.Look(ref count, "count", 0);
            Scribe_Values.Look(ref killed, "killed", 0);
            Scribe_Values.Look(ref outSignalPawnKilled, "outSignalPawnKilled");
        }

        public override void AssignDebugData()
        {
            base.AssignDebugData();
            requiredInstigatorFaction = Faction.OfPlayer;
            count = 10;
        }
    }
    public class QuestNode_MapParentPawnsKilled : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> inSignalEnable, inSignalDisable;

        [NoTranslate]
        public SlateRef<string> outSignalComplete;

        public SlateRef<ThingDef> race;

        public SlateRef<int> count;

        public SlateRef<MapParent> mapParent;

        public QuestNode node;

        private const string PawnOfRaceKilledSignal = "PawnOfRaceKilled";

        protected override bool TestRunInt(Slate slate)
        {
            if (!Find.Storyteller.difficulty.allowViolentQuests)
            {
                return false;
            }
            if (node != null)
            {
                return node.TestRun(slate);
            }
            return true;
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            string text = QuestGen.GenerateNewSignal("PawnOfRaceKilled");
            QuestPart_PawnsKilledMapFaction questPart_PawnsKilled = new QuestPart_PawnsKilledMapFaction
            {
                inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"),
                inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignalDisable"),
                requiredInstigatorFaction = Faction.OfPlayer,
                count = count.GetValue(slate),
                requiredVictimFaction = mapParent.GetValue(slate).Faction,
                outSignalPawnKilled = text
            };
            if (node != null)
            {
                QuestGenUtility.RunInnerNode(node, questPart_PawnsKilled);
            }
            if (!outSignalComplete.GetValue(slate).NullOrEmpty())
            {
                questPart_PawnsKilled.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate)));
            }
            QuestGen.quest.AddPart(questPart_PawnsKilled);
            QuestPart_PawnsAvailable questPart_PawnsAvailable = new QuestPart_PawnsAvailable();
            questPart_PawnsAvailable.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
        }
    }
}

