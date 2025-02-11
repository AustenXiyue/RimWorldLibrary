using UnityEngine;
using Verse;

namespace CombatExtended;

public class GunDrawExtension : DefModExtension
{
	public Vector2 DrawSize = Vector2.one;

	public Vector2 DrawOffset = Vector2.zero;

	public float recoilModifier = 1f;

	public float recoilScale = -1f;

	public int recoilTick = -1;

	public float muzzleJumpModifier = -1f;

	public Vector2 CasingOffset = Vector2.zero;

	public float CasingAngleOffset = 0f;

	public bool DropCasingWhenReload;

	public bool AdvancedCasingVariables = false;

	public FloatRange CasingAngleOffsetRange = new FloatRange(0f, 0f);

	public float CasingLifeTimeMultiplier = -1f;

	public FloatRange CasingLifeTimeOverrideRange = new FloatRange(-1f, 1f);

	public float CasingSpeedMultiplier = -1f;

	public FloatRange CasingSpeedOverrideRange = new FloatRange(-1f, 1f);

	public float CasingSizeOffset = 1f;

	public Vector2 CasingOffsetRandomRange = Vector2.zero;

	public float CasingRotationRandomRange = 0f;
}
