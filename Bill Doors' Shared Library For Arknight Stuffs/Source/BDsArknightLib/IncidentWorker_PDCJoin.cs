using BillDoorsPredefinedCharacter;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace BDsArknightLib
{
    public abstract class IncidentWorker_PDCSpawn : IncidentWorker
    {
        ModExtension_PDCDefs ext => def.GetModExtension<ModExtension_PDCDefs>();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms)) return false;
            foreach (var def in ext.defs) if (def.AtHome()) return CanSpawn((Map)parms.target);
            return false;
        }

        protected virtual IEnumerable<Pawn> GeneratePawns(Faction faction)
        {
            foreach (var def in ext.defs) if (def.AtHome()) yield return def.MakePawn(faction);
        }

        protected abstract bool CanSpawn(Map map);

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!CanSpawn((Map)parms.target)) return false;
            var pawns = GeneratePawns(parms.faction ?? Faction.OfPlayer).ToList();
            PostProcessPawns(pawns, parms);
            SendStandardLetter(def.letterLabel, def.letterText, def.letterDef, parms, pawns);
            return true;
        }

        protected abstract void PostProcessPawns(IEnumerable<Pawn> pawns, IncidentParms parms);
    }

    public class IncidentWorker_PDCJoin : IncidentWorker_PDCSpawn
    {
        protected override void PostProcessPawns(IEnumerable<Pawn> pawns, IncidentParms parms)
        {
            var map = (Map)parms.target;
            TryFindEntryCell(map, out IntVec3 cell);
            foreach (var pawn in pawns) GenSpawn.Spawn(pawn, cell, map);
        }

        protected bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Always, out cell);
        }

        protected override bool CanSpawn(Map map)
        {
            return TryFindEntryCell(map, out _);
        }
    }

    //For PDC-only raids. For blending PDC into raids, use the harmony patched way
    public class IncidentWorker_PDCAttack : IncidentWorker_PDCSpawn
    {
        protected override void PostProcessPawns(IEnumerable<Pawn> pawns, IncidentParms parms)
        {
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;

            parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms);
            parms.raidStrategy.Worker.MakeLords(parms, pawns.ToList());
            parms.raidArrivalMode.Worker.Arrive(pawns.ToList(), parms);
        }

        protected override bool CanSpawn(Map map)
        {
            return true;
        }
    }
}
