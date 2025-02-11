using UnityEngine;
using Verse;

namespace AncotLibrary;

[StaticConstructorOnStartup]
public class Gizmo_PhysicalShieldBar : Gizmo
{
	public CompPhysicalShield compPhysicalShield;

	private Color customBarColor = new Color(0.68f, 0.68f, 0.68f);

	private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.black);

	private static readonly Texture2D TargetLevelArrow = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarkerRotated");

	private const float ArrowScale = 0.5f;

	public Gizmo_PhysicalShieldBar()
	{
		Order = -99f;
	}

	public override float GetWidth(float maxWidth)
	{
		return 140f;
	}

	public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
	{
		customBarColor = compPhysicalShield.shieldBarColor;
		Texture2D fullBarTex = SolidColorMaterials.NewSolidColorTexture(customBarColor);
		Rect overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
		Find.WindowStack.ImmediateWindow(1523115973, overRect, WindowLayer.GameUI, delegate
		{
			Rect rect;
			Rect rect2 = (rect = overRect.AtZero().ContractedBy(6f));
			rect.height = overRect.height / 2f;
			Text.Font = GameFont.Tiny;
			Widgets.Label(rect, compPhysicalShield.barGizmoLabel);
			Rect rect3 = rect2;
			rect3.yMin = overRect.height / 2f;
			float fillPercent = compPhysicalShield.stamina / compPhysicalShield.maxStamina;
			Widgets.FillableBar(rect3, fillPercent, fullBarTex, EmptyBarTex, doBorder: false);
			Text.Font = GameFont.Small;
			Text.Anchor = (TextAnchor)4;
			Widgets.Label(rect3, compPhysicalShield.stamina.ToString("F0") + " / " + compPhysicalShield.maxStamina.ToString("F0"));
			Text.Anchor = (TextAnchor)0;
		});
		return new GizmoResult(GizmoState.Clear);
	}
}
