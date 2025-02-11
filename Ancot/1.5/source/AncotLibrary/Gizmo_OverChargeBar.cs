using UnityEngine;
using Verse;

namespace AncotLibrary;

[StaticConstructorOnStartup]
public class Gizmo_OverChargeBar : Gizmo
{
	public CompOverChargeShot compOverChargeShot;

	private Color customBarColor = new Color(0.35f, 0.35f, 0.2f);

	private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.black);

	private static readonly Texture2D TargetLevelArrow = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarkerRotated");

	private const float ArrowScale = 0.5f;

	public Gizmo_OverChargeBar()
	{
		Order = -99f;
	}

	public override float GetWidth(float maxWidth)
	{
		return 140f;
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		customBarColor = compOverChargeShot.barColor;
		Texture2D fullBarTex = SolidColorMaterials.NewSolidColorTexture(customBarColor);
		Rect overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Find.WindowStack.ImmediateWindow(1523289455, overRect, WindowLayer.GameUI, delegate
		{
			Rect rect;
			Rect rect2 = (rect = overRect.AtZero().ContractedBy(6f));
			rect.height = overRect.height / 2f;
			Text.Font = GameFont.Tiny;
			Widgets.Label(rect, compOverChargeShot.parent.Label);
			Rect rect3 = rect2;
			rect3.yMin = overRect.height / 2f;
			float fillPercent = compOverChargeShot.charge / compOverChargeShot.maxCharge;
			Widgets.FillableBar(rect3, fillPercent, fullBarTex, EmptyBarTex, doBorder: false);
			Text.Font = GameFont.Small;
			Text.Anchor = (TextAnchor)4;
			Widgets.Label(rect3, compOverChargeShot.charge.ToString("F0") + " / " + compOverChargeShot.maxCharge.ToString("F0"));
			Text.Anchor = (TextAnchor)0;
		});
		return new GizmoResult(GizmoState.Clear);
	}
}
