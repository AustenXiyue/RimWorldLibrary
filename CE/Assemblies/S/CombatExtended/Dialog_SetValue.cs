using System;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Dialog_SetValue : Window
{
	private readonly Func<int, string> textGetter;

	private readonly Action<int> confirmAction;

	private int curValue;

	private const float BotAreaWidth = 60f;

	private const float BotAreaHeight = 30f;

	private new const float Margin = 10f;

	public override Vector2 InitialSize => new Vector2(300f, 130f);

	public Dialog_SetValue(Func<int, string> textGetter, Action<int> confirmAction, int value)
	{
		this.textGetter = textGetter;
		this.confirmAction = confirmAction;
		curValue = value;
	}

	public override void DoWindowContents(Rect inRect)
	{
		Text.Font = GameFont.Small;
		string text = textGetter(curValue);
		float height = Text.CalcHeight(text, inRect.width);
		Rect rect = new Rect(inRect.x, inRect.y, inRect.width, height);
		Text.Anchor = TextAnchor.UpperCenter;
		Widgets.Label(rect, text);
		Text.Anchor = TextAnchor.UpperLeft;
		float y = inRect.y + rect.height + 10f;
		Text.Anchor = TextAnchor.UpperCenter;
		Text.Font = GameFont.Medium;
		Widgets.Label(new Rect(inRect.x + 60f, y, inRect.width - 120f, 30f), curValue.ToString());
		Text.Font = GameFont.Small;
		if (Widgets.ButtonText(new Rect(inRect.x, y, 60f, 30f), "-", drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			curValue--;
		}
		if (Widgets.ButtonText(new Rect(inRect.x + inRect.width - 60f, y, 60f, 30f), "+", drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			curValue++;
		}
		curValue = ((curValue >= 0) ? curValue : 0);
		Text.Anchor = TextAnchor.UpperLeft;
		GUI.color = Color.white;
		if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 30f, inRect.width, 30f), "OK".Translate(), drawBackground: true, doMouseoverSound: true, active: true, null))
		{
			Close();
			confirmAction(curValue);
		}
	}
}
