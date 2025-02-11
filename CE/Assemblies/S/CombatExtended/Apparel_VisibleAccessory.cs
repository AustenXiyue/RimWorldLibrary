using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Apparel_VisibleAccessory : Apparel
{
	private const float MinClippingDistance = 0.002f;

	private const float _HeadOffset = 0.028515153f;

	private const float _BodyOffset = 0.024727274f;

	private const float _OffsetFactor = 0.001f;

	private static readonly Dictionary<string, bool> _OnHeadCache = new Dictionary<string, bool>();

	public bool onHead
	{
		get
		{
			if (!_OnHeadCache.ContainsKey(def.defName))
			{
				List<BodyPartRecord> list = base.Wearer.RaceProps.body.AllParts.Where(def.apparel.CoversBodyPart).ToList();
				bool flag = false;
				foreach (BodyPartRecord item in list)
				{
					for (BodyPartRecord bodyPartRecord = item; bodyPartRecord != null; bodyPartRecord = bodyPartRecord.parent)
					{
						if (bodyPartRecord.groups.Contains(BodyPartGroupDefOf.Torso))
						{
							_OnHeadCache.Add(def.defName, value: false);
							flag = true;
							break;
						}
						if (bodyPartRecord.groups.Contains(BodyPartGroupDefOf.FullHead))
						{
							_OnHeadCache.Add(def.defName, value: true);
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (!_OnHeadCache.ContainsKey(def.defName))
				{
					Log.ErrorOnce(string.Concat("Combat Extended :: ", GetType(), " was unable to determine if body or head on item '", Label, "', might the Wearer be non-human?  Assuming apparel is on body."), def.debugRandomId);
					_OnHeadCache.Add(def.defName, value: false);
				}
			}
			_OnHeadCache.TryGetValue(def.defName, out var value);
			return value;
		}
	}

	public override void DrawWornExtras()
	{
		if (base.Wearer == null || !base.Wearer.Spawned)
		{
			return;
		}
		Building_Bed building_Bed = base.Wearer.CurrentBed();
		if (building_Bed != null && !building_Bed.def.building.bed_showSleeperBody && !onHead)
		{
			return;
		}
		if (onHead)
		{
			Log.ErrorOnce("Combat Extended :: Apparel_VisibleAccessory: The head drawing code is incomplete and the apparel '" + Label + "' will not be drawn.", def.debugRandomId);
			return;
		}
		Rot4 rot = default(Rot4);
		float angle = 0f;
		Vector3 drawPos = base.Wearer.Drawer.DrawPos;
		Rot4 rot2;
		if (base.Wearer.GetPosture() != 0)
		{
			rot2 = LayingFacing();
			if (building_Bed != null)
			{
				rot = building_Bed.Rotation;
				rot.AsInt += 2;
				angle = rot.AsAngle;
				AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)building_Bed.def.altitudeLayer, 14);
				drawPos.y = base.Wearer.Position.ToVector3ShiftedWithAltitude(altLayer).y;
				drawPos += rot.FacingCell.ToVector3() * (0f - base.Wearer.Drawer.renderer.BaseHeadOffsetAt(Rot4.South).z);
			}
			else
			{
				drawPos.y = base.Wearer.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.LayingPawn).y;
				if (base.Wearer.Downed)
				{
					float? num = ((((((base.Wearer.Drawer == null) ? null : base.Wearer.Drawer.renderer) == null) ? null : base.Wearer.Drawer.renderer.wiggler) == null) ? ((float?)null) : new float?(base.Wearer.Drawer.renderer.wiggler.downedAngle));
					if (num.HasValue)
					{
						angle = num.Value;
					}
				}
				else
				{
					angle = rot2.FacingCell.AngleFlat;
				}
			}
			drawPos.y += 0.005f;
		}
		else
		{
			rot2 = base.Wearer.Rotation;
			drawPos.y = base.Wearer.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Pawn).y;
		}
		drawPos.y += GetAltitudeOffset(rot2);
		string path = def.graphicData.texPath + "_" + ((base.Wearer == null) ? null : base.Wearer.story.bodyType.ToString());
		Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutComplex, def.graphicData.drawSize, DrawColor);
		Material material = new ApparelGraphicRecord(graphic, this).graphic.MatAt(rot2);
		Vector3 s = new Vector3(1.5f, 1.5f, 1.5f);
		Matrix4x4 matrix = default(Matrix4x4);
		matrix.SetTRS(drawPos, Quaternion.AngleAxis(angle, Vector3.up), s);
		Graphics.DrawMesh((rot2 == Rot4.West) ? MeshPool.plane10Flip : MeshPool.plane10, matrix, material, 0);
	}

	public float GetAltitudeOffset(Rot4 rotation)
	{
		VisibleAccessoryDefExtension visibleAccessoryDefExtension = def.GetModExtension<VisibleAccessoryDefExtension>() ?? new VisibleAccessoryDefExtension();
		visibleAccessoryDefExtension.Validate();
		float num = 0.001f * (float)visibleAccessoryDefExtension.order;
		if (!onHead)
		{
			if (rotation == Rot4.North)
			{
				return num + 0.028515153f;
			}
			return num + 0.024727274f;
		}
		if (rotation == Rot4.North)
		{
			return num + 0.024727274f;
		}
		return num + 0.028515153f;
	}

	private Rot4 LayingFacing()
	{
		if (base.Wearer == null)
		{
			return Rot4.Random;
		}
		if (base.Wearer.GetPosture() == PawnPosture.LayingOnGroundFaceUp)
		{
			return Rot4.South;
		}
		if (base.Wearer.RaceProps.Humanlike)
		{
			switch (base.Wearer.thingIDNumber % 4)
			{
			case 0:
				return Rot4.South;
			case 1:
				return Rot4.South;
			case 2:
				return Rot4.East;
			case 3:
				return Rot4.West;
			}
		}
		else
		{
			switch (base.Wearer.thingIDNumber % 4)
			{
			case 0:
				return Rot4.South;
			case 1:
				return Rot4.East;
			case 2:
				return Rot4.West;
			case 3:
				return Rot4.West;
			}
		}
		return Rot4.Random;
	}
}
