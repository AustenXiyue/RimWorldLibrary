using UnityEngine;
using Verse;

namespace AncotLibrary;

public class PawnRenderNodeWorker_TurretGun_Custom : PawnRenderNodeWorker
{
	public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
	{
		if (!base.CanDrawNow(node, parms))
		{
			return false;
		}
		PawnRenderNode_TurretGun_Custom pawnRenderNode_TurretGun_Custom = node as PawnRenderNode_TurretGun_Custom;
		PawnRenderNodeProperties_TurretGun_Custom pawnRenderNodeProperties_TurretGun_Custom = node.Props as PawnRenderNodeProperties_TurretGun_Custom;
		if (pawnRenderNodeProperties_TurretGun_Custom.isApparel)
		{
			pawnRenderNode_TurretGun_Custom.turretComp = pawnRenderNode_TurretGun_Custom.apparel.TryGetComp<CompTurretGun_Custom>();
		}
		if (pawnRenderNode_TurretGun_Custom.turretComp != null && pawnRenderNode_TurretGun_Custom.turretComp.CanShoot)
		{
			return true;
		}
		return false;
	}

	public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
	{
		Quaternion result = base.RotationFor(node, parms);
		PawnRenderNode_TurretGun_Custom pawnRenderNode_TurretGun_Custom = node as PawnRenderNode_TurretGun_Custom;
		PawnRenderNodeProperties_TurretGun_Custom pawnRenderNodeProperties_TurretGun_Custom = node.Props as PawnRenderNodeProperties_TurretGun_Custom;
		if (pawnRenderNodeProperties_TurretGun_Custom.isApparel)
		{
			pawnRenderNode_TurretGun_Custom.turretComp = pawnRenderNode_TurretGun_Custom.apparel.TryGetComp<CompTurretGun_Custom>();
		}
		if (pawnRenderNodeProperties_TurretGun_Custom.combatDrone)
		{
			return result;
		}
		Pawn pawnOwner = pawnRenderNode_TurretGun_Custom.turretComp.PawnOwner;
		if (pawnRenderNode_TurretGun_Custom != null)
		{
			if (pawnRenderNode_TurretGun_Custom.turretComp.currentTarget == LocalTargetInfo.Invalid && pawnOwner.CurJob != null && pawnOwner.CurJob.targetA != null)
			{
				IntVec3 position = pawnOwner.Position;
				IntVec3 cell = pawnOwner.CurJob.targetA.Cell;
				float ang = (cell - position).ToVector3Shifted().ToAngleFlat();
				result *= ang.ToQuat();
			}
			result *= pawnRenderNode_TurretGun_Custom.turretComp.curRotation.ToQuat();
		}
		return result;
	}

	public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
	{
		PawnRenderNode_TurretGun_Custom pawnRenderNode_TurretGun_Custom = node as PawnRenderNode_TurretGun_Custom;
		PawnRenderNodeProperties_TurretGun_Custom pawnRenderNodeProperties_TurretGun_Custom = node.Props as PawnRenderNodeProperties_TurretGun_Custom;
		if (pawnRenderNodeProperties_TurretGun_Custom.isApparel)
		{
			pawnRenderNode_TurretGun_Custom.turretComp = pawnRenderNode_TurretGun_Custom.apparel.TryGetComp<CompTurretGun_Custom>();
		}
		if (pawnRenderNode_TurretGun_Custom != null && pawnRenderNode_TurretGun_Custom.turretComp != null)
		{
			Vector3 vector = new Vector3(pawnRenderNode_TurretGun_Custom.turretComp.floatOffset_xAxis, 0f, pawnRenderNode_TurretGun_Custom.turretComp.floatOffset_yAxis);
			return base.OffsetFor(node, parms, out pivot) + vector;
		}
		return base.OffsetFor(node, parms, out pivot);
	}
}
