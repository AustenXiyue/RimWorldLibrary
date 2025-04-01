using RimWorld;
using Verse;

namespace BDsArknightLib
{
    public class CompUseEffect_TurnInto : CompUseEffect
    {
        public new CompProperties_TurnInto Props => props as CompProperties_TurnInto;

        public override void DoEffect(Pawn usedBy)
        {
            var pos = parent.Position;
            var map = parent.Map;
            var faction = parent.Faction;
            var rot = parent.Rotation;
            parent.Destroy();
            Thing thing = ThingMaker.MakeThing(Props.def, null);
            thing.SetFactionDirect(faction);
            GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Direct, out _, null, null, rot);
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "Open")
            {
                DoEffect(null);
            }
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            return base.CanBeUsedBy(p);
        }
    }

    public class CompProperties_TurnInto : CompProperties_UseEffect
    {
        public CompProperties_TurnInto()
        {
            this.compClass = typeof(CompUseEffect_TurnInto);
        }

        public ThingDef def;
    }
}
