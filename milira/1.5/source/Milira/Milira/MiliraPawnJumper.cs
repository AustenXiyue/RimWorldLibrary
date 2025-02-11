using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Milira;

public class MiliraPawnJumper : PawnFlyer
{
	private static readonly Func<float, float> FlightSpeed;

	private static readonly Func<float, float> FlightCurveHeight;

	public Material cachedShadowMaterial;

	public Effecter flightEffecter;

	public int positionLastComputedTick = -1;

	public Vector3 groundPos;

	public Vector3 effectivePos;

	public float effectiveHeight;

	public Vector3 direction => (base.DestinationPos - startVec).normalized;

	public override Vector3 DrawPos
	{
		get
		{
			RecomputePosition();
			return effectivePos;
		}
	}

	static MiliraPawnJumper()
	{
		FlightCurveHeight = GenMath.InverseParabola;
		AnimationCurve animationCurve = new AnimationCurve();
		animationCurve.AddKey(0f, 0f);
		animationCurve.AddKey(0.1f, 0.3f);
		animationCurve.AddKey(0.2f, 0.47f);
		animationCurve.AddKey(0.3f, 0.6f);
		animationCurve.AddKey(0.4f, 0.69f);
		animationCurve.AddKey(0.5f, 0.77f);
		animationCurve.AddKey(0.6f, 0.84f);
		animationCurve.AddKey(0.7f, 0.9f);
		animationCurve.AddKey(0.8f, 0.95f);
		animationCurve.AddKey(0.9f, 0.98f);
		animationCurve.AddKey(1f, 1f);
		FlightSpeed = animationCurve.Evaluate;
	}

	protected virtual void RecomputePosition()
	{
		if (positionLastComputedTick != ticksFlying)
		{
			positionLastComputedTick = ticksFlying;
			float arg = (float)ticksFlying / (float)ticksFlightTime;
			float t = FlightSpeed(arg);
			effectiveHeight = FlightCurveHeight(0f);
			groundPos = Vector3.Lerp(startVec, base.DestinationPos, t);
			Vector3 vector = new Vector3(0f, 0f, 2f);
			Vector3 vector2 = Altitudes.AltIncVect * effectiveHeight;
			Vector3 vector3 = vector * effectiveHeight;
			effectivePos = groundPos + vector2 + vector3;
		}
	}

	public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
	{
		RecomputePosition();
		if (base.FlyingPawn != null)
		{
			base.FlyingPawn.DynamicDrawPhaseAt(phase, effectivePos);
		}
		else
		{
			base.FlyingThing?.DynamicDrawPhaseAt(phase, effectivePos);
		}
		float aimAngle = direction.AngleFlat();
		DrawEquipmentAiming(base.FlyingPawn.equipment.Primary, drawLoc, aimAngle);
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		DrawShadow(groundPos, effectiveHeight);
		if (base.CarriedThing != null && base.FlyingPawn != null)
		{
			PawnRenderUtility.DrawCarriedThing(base.FlyingPawn, effectivePos, base.CarriedThing);
		}
	}

	protected virtual void DrawShadow(Vector3 drawLoc, float height)
	{
		Material shadowMaterial = def.pawnFlyer.ShadowMaterial;
		if (!(shadowMaterial == null))
		{
			float num = Mathf.Lerp(1f, 0.6f, height);
			Vector3 s = new Vector3(num, 1f, num);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(drawLoc, Quaternion.identity, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
		}
	}

	protected override void RespawnPawn()
	{
		LandingEffects();
		base.RespawnPawn();
	}

	public override void Tick()
	{
		if (flightEffecter == null && flightEffecterDef != null)
		{
			flightEffecter = flightEffecterDef.Spawn();
			flightEffecter.Trigger(this, TargetInfo.Invalid);
		}
		else
		{
			flightEffecter?.EffectTick(this, TargetInfo.Invalid);
		}
		base.Tick();
	}

	protected virtual void LandingEffects()
	{
		if (soundLanding != null)
		{
			soundLanding.PlayOneShot(new TargetInfo(base.Position, base.Map));
		}
		FleckMaker.ThrowDustPuff(base.DestinationPos + Gen.RandomHorizontalVector(0.5f), base.Map, 2f);
	}

	public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
	{
		flightEffecter?.Cleanup();
		base.Destroy(mode);
	}

	public virtual void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
	{
		if (eq == null)
		{
			return;
		}
		Mesh mesh = null;
		float num = aimAngle - 90f;
		if (aimAngle > 20f && aimAngle < 160f)
		{
			mesh = MeshPool.plane10;
			num += eq.def.equippedAngleOffset;
		}
		else if (aimAngle > 200f && aimAngle < 340f)
		{
			mesh = MeshPool.plane10Flip;
			num -= 180f;
			num -= eq.def.equippedAngleOffset;
		}
		else
		{
			mesh = MeshPool.plane10;
			num += eq.def.equippedAngleOffset;
		}
		num %= 360f;
		CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
		if (compEquippable != null)
		{
			EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
			drawLoc += drawOffset;
			num += angleOffset;
			drawLoc.x += 0.6f * direction.x;
			drawLoc.z += 0.6f * direction.z;
			if (aimAngle > 45f && aimAngle < 315f)
			{
				drawLoc.y = AltitudeLayer.Skyfaller.AltitudeFor() + 0.02f;
			}
		}
		Material material = null;
		material = ((!(eq.Graphic is Graphic_StackCount graphic_StackCount)) ? eq.Graphic.MatSingleFor(eq) : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq));
		Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(eq.Graphic.drawSize.x, 0f, eq.Graphic.drawSize.y), pos: drawLoc, q: Quaternion.AngleAxis(num, Vector3.up));
		Graphics.DrawMesh(mesh, matrix, material, 0);
	}
}
