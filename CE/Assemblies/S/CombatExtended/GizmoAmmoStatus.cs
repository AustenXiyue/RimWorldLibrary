using UnityEngine;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class GizmoAmmoStatus : Command
{
	public CompAmmoUser compAmmo;

	public string prefix = "";

	private new static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/DesButBG");

	public override float GetWidth(float maxWidth)
	{
		return 120f;
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Rect rect2 = rect.ContractedBy(6f);
		GUI.DrawTexture(rect, BGTex);
		Text.Font = GameFont.Tiny;
		Rect rect3 = rect2.TopHalf();
		Widgets.Label(rect3, prefix + ((compAmmo.CurrentAmmo == null) ? compAmmo.parent.def.LabelCap : compAmmo.CurrentAmmo.ammoClass.LabelCap));
		if (compAmmo.HasMagazine)
		{
			Rect rect4 = rect2.BottomHalf();
			Widgets.FillableBar(rect4, (float)compAmmo.CurMagCount / (float)compAmmo.MagSize);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect4, compAmmo.CurMagCount + " / " + compAmmo.MagSize);
			Text.Anchor = TextAnchor.UpperLeft;
		}
		return new GizmoResult(GizmoState.Clear);
	}
}
