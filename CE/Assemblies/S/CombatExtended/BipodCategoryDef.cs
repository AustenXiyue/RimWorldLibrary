using UnityEngine;
using Verse;

namespace CombatExtended;

public class BipodCategoryDef : Def
{
	public string bipod_id = "Bipod_Undefined";

	public int ad_Range = 0;

	public int setuptime = 240;

	public float recoil_mult_setup = 1f;

	public float recoil_mult_NOT_setup = 1f;

	public float warmup_mult_setup = 1f;

	public float warmup_mult_NOT_setup = 1f;

	public float swayMult = 1f;

	public float swayPenalty = 1f;

	public AimMode autosetMode = AimMode.SuppressFire;

	public Color logColor = Color.blue;

	public bool useAutoSetMode = true;
}
