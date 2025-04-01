using Verse;
using RimWorld;
using System.Collections.Generic;
using RimWorld.QuestGen;
using Verse.Grammar;
using UnityEngine;

namespace BDsArknightLib
{
    public class QuestNode_TempFactionFrom : QuestNode
    {
        [NoTranslate]
        public string storeAs;

        public FactionDef FactionDef;

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            if (!slate.TryGet<Faction>(storeAs, out _))
            {
                List<FactionRelation> list = new List<FactionRelation>();
                foreach (Faction item in Find.FactionManager.AllFactionsListForReading)
                {
                    if (!item.def.PermanentlyHostileTo(FactionDef))
                    {
                        list.Add(new FactionRelation
                        {
                            other = item,
                            kind = FactionRelationKind.Neutral
                        });
                    }
                }
                var var = FactionGenerator.NewGeneratedFactionWithRelations(FactionDef, list, hidden: true);
                var.temporary = true;
                var.factionHostileOnHarmByPlayer = true;
                Find.FactionManager.Add(var);
                slate.Set(storeAs, var);
                if (!var.Hidden)
                {
                    QuestPart_InvolvedFactions questPart_InvolvedFactions = new QuestPart_InvolvedFactions();
                    questPart_InvolvedFactions.factions.Add(var);
                    QuestGen.quest.AddPart(questPart_InvolvedFactions);
                }
            }
        }

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }

    public class QuestNode_IsFactionAllyToPlayer : QuestNode
    {
        public SlateRef<Faction> faction;

        public SlateRef<Thing> factionOf;

        public QuestNode node;

        public QuestNode elseNode;

        protected override bool TestRunInt(Slate slate)
        {
            if (IsAlly(slate))
            {
                if (node != null)
                {
                    return node.TestRun(slate);
                }
                return true;
            }
            if (elseNode != null)
            {
                return elseNode.TestRun(slate);
            }
            return true;
        }

        protected override void RunInt()
        {
            if (IsAlly(QuestGen.slate))
            {
                node?.Run();
            }
            else
            {
                elseNode?.Run();
            }
        }

        private bool IsAlly(Slate slate)
        {
            Faction value = faction.GetValue(slate);
            if (value != null)
            {
                return value.PlayerRelationKind == FactionRelationKind.Ally;
            }
            Thing value2 = factionOf.GetValue(slate);
            if (value2 != null && value2.Faction != null)
            {
                return value2.Faction.PlayerRelationKind == FactionRelationKind.Ally;
            }
            return false;
        }
    }
}
