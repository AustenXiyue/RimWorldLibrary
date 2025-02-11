using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CombatExtended.RocketGUI;

public abstract class IListing_Custom
{
	protected struct RectSlice
	{
		public Rect inside;

		public Rect outside;

		public RectSlice(Rect inside, Rect outside)
		{
			this.outside = outside;
			this.inside = inside;
		}
	}

	public const float ScrollViewWidthDelta = 25f;

	protected Vector2 margins = new Vector2(8f, 4f);

	protected float inXMin = 0f;

	protected float inXMax = 0f;

	protected float curYMin = 0f;

	protected float inYMin = 0f;

	protected float inYMax = 0f;

	protected float previousHeight = 0f;

	protected bool isOverflowing = false;

	protected Rect contentRect;

	private Rect inRect;

	private bool started = false;

	public readonly bool ScrollViewOnOverflow;

	public Vector2 ScrollPosition = Vector2.zero;

	public Color CollapsibleBGColor = Widgets.MenuSectionBGFillColor;

	public Color CollapsibleBGBorderColor = Widgets.MenuSectionBGBorderColor;

	public bool drawBorder = false;

	public bool drawBackground = false;

	protected virtual bool Overflowing => isOverflowing;

	protected virtual float insideWidth => inXMax - inXMin - margins.x * 2f;

	public virtual Vector2 Margins
	{
		get
		{
			return margins;
		}
		set
		{
			margins = value;
		}
	}

	public Rect Rect
	{
		get
		{
			return new Rect(inXMin, curYMin, inXMax - inXMin, inYMax - curYMin);
		}
		set
		{
			inXMin = value.xMin;
			inXMax = value.xMax;
			curYMin = value.yMin;
			inYMin = value.yMin;
			inYMax = value.yMax;
		}
	}

	public IListing_Custom(bool scrollViewOnOverflow = true, bool drawBorder = false, bool drawBackground = false)
	{
		ScrollViewOnOverflow = scrollViewOnOverflow;
		this.drawBorder = drawBorder;
		this.drawBackground = drawBackground;
	}

	protected virtual void Begin(Rect inRect, bool scrollViewOnOverflow = true)
	{
		this.inRect = inRect;
		if (ScrollViewOnOverflow && started && inRect.height < previousHeight)
		{
			GUIUtility.ExecuteSafeGUIAction(delegate
			{
				isOverflowing = true;
				GUIUtility.StashGUIState();
				GUI.color = Color.white;
				contentRect = new Rect(0f, 0f, inRect.width - 25f, previousHeight);
				inYMin = contentRect.yMin;
				Rect = contentRect;
				Widgets.BeginScrollView(inRect, ref ScrollPosition, contentRect);
				GUIUtility.RestoreGUIState();
			});
		}
		else
		{
			inYMin = inRect.yMin;
			Rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
		}
	}

	protected virtual void Start()
	{
		GUIUtility.StashGUIState();
		Text.Font = GameFont.Tiny;
		Text.CurFontStyle.fontStyle = FontStyle.Normal;
	}

	protected virtual void Label(TaggedString text, Color color, string tooltip = null, bool hightlightIfMouseOver = true, GameFont fontSize = GameFont.Tiny, FontStyle fontStyle = FontStyle.Normal, TextAnchor anchor = TextAnchor.UpperLeft)
	{
		GUIUtility.ExecuteSafeGUIAction(delegate
		{
			RectSlice rectSlice = Slice(text.GetTextHeight(insideWidth));
			if (hightlightIfMouseOver)
			{
				Widgets.DrawHighlightIfMouseover(rectSlice.outside);
			}
			GUI.color = color;
			Text.Anchor = anchor;
			Text.Font = fontSize;
			Text.CurFontStyle.fontStyle = fontStyle;
			Widgets.Label(rectSlice.inside, text);
			if (tooltip != null)
			{
				TooltipHandler.TipRegion(rectSlice.outside, tooltip);
			}
		});
	}

	protected virtual bool CheckboxLabeled(TaggedString text, ref bool checkOn, string tooltip = null, bool disabled = false, bool hightlightIfMouseOver = true, GameFont fontSize = GameFont.Tiny, FontStyle fontStyle = FontStyle.Normal)
	{
		bool result = false;
		bool checkOnInt = checkOn;
		GUIUtility.ExecuteSafeGUIAction(delegate
		{
			Text.Font = fontSize;
			Text.CurFontStyle.fontStyle = fontStyle;
			RectSlice rectSlice = Slice(text.GetTextHeight(insideWidth - 23f));
			if (hightlightIfMouseOver)
			{
				Widgets.DrawHighlightIfMouseover(rectSlice.outside);
			}
			GUIUtility.CheckBoxLabeled(rectSlice.inside, text, ref checkOnInt, disabled, monotone: false, 23f, GameFont.Tiny, FontStyle.Normal, placeCheckboxNearText: false, drawHighlightIfMouseover: false);
			if (tooltip != null)
			{
				TooltipHandler.TipRegion(rectSlice.outside, tooltip);
			}
		});
		if (checkOnInt != checkOn)
		{
			checkOn = checkOnInt;
			result = true;
		}
		return result;
	}

	protected virtual void Columns(float height, IEnumerable<Action<Rect>> lambdas, float gap = 5f, bool useMargins = false, Action fallback = null)
	{
		GUIUtility.ExecuteSafeGUIAction(delegate
		{
			if (lambdas.Count() == 1)
			{
				Lambda(height, lambdas.First(), useMargins, hightlightIfMouseOver: false, fallback);
				return;
			}
			Rect rect = (useMargins ? Slice(height).inside : Slice(height).outside);
			Rect[] columns = rect.Columns(lambdas.Count(), gap);
			int i = 0;
			foreach (Action<Rect> lambda in lambdas)
			{
				GUIUtility.ExecuteSafeGUIAction(delegate
				{
					Action<Rect> action = lambda;
					Rect[] array = columns;
					int num = i;
					i = num + 1;
					action(array[num]);
				}, fallback);
			}
		});
	}

	protected virtual void DropDownMenu<T>(TaggedString text, T selection, Func<T, string> labelLambda, Action<T> selectedLambda, IEnumerable<T> options, bool disabled = false, GameFont fontSize = GameFont.Tiny, FontStyle fontStyle = FontStyle.Normal)
	{
		GUIUtility.ExecuteSafeGUIAction(delegate
		{
			string text2 = labelLambda(selection);
			Text.Font = fontSize;
			Text.CurFontStyle.fontStyle = fontStyle;
			Rect inside = Slice(text2.GetTextHeight(insideWidth - 23f)).inside;
			Rect[] array = inside.Columns(2);
			Widgets.Label(array[0], text);
			if (Widgets.ButtonText(array[1], text2, drawBackground: true, doMouseoverSound: true, !disabled, null))
			{
				GUIUtility.DropDownMenu(labelLambda, selectedLambda, options);
			}
		});
	}

	protected virtual void Lambda(float height, Action<Rect> contentLambda, bool useMargins = false, bool hightlightIfMouseOver = true, Action fallback = null)
	{
		RectSlice slice = Slice(height);
		if (hightlightIfMouseOver)
		{
			Widgets.DrawHighlightIfMouseover(slice.outside);
		}
		GUIUtility.ExecuteSafeGUIAction(delegate
		{
			contentLambda(useMargins ? slice.inside : slice.outside);
		}, fallback);
	}

	protected virtual void Gap(float height = 9f)
	{
		Slice(height, includeMargins: false);
	}

	protected virtual void Line(float thickness)
	{
		Widgets.DrawBoxSolid((!drawBorder) ? Slice(thickness).inside : Slice(thickness, includeMargins: false).outside, CollapsibleBGBorderColor);
	}

	protected virtual bool ButtonText(TaggedString text, bool disabled = false, bool drawBackground = false)
	{
		bool clicked = false;
		GUIUtility.ExecuteSafeGUIAction(delegate
		{
			Text.Font = GameFont.Small;
			Text.CurFontStyle.fontStyle = FontStyle.Normal;
			RectSlice rectSlice = Slice(text.GetTextHeight(insideWidth) + 4f);
			if (!drawBackground)
			{
				Text.Anchor = TextAnchor.MiddleCenter;
				GUI.color = (Mouse.IsOver(rectSlice.inside) ? Color.white : Color.cyan);
			}
			clicked = Widgets.ButtonText(rectSlice.inside, text, drawBackground, doMouseoverSound: true, active: true, null);
		});
		return clicked;
	}

	public virtual void End(ref Rect inRect)
	{
		Gap(5f);
		GUI.color = CollapsibleBGBorderColor;
		if (drawBorder)
		{
			Widgets.DrawBox(new Rect(inXMin, inYMin, inXMax - inXMin, curYMin - inYMin));
		}
		started = true;
		previousHeight = Mathf.Abs(inYMin - curYMin);
		if (isOverflowing)
		{
			Widgets.EndScrollView();
			if (started && inRect.height < previousHeight)
			{
				GUI.color = CollapsibleBGBorderColor;
				Widgets.DrawBox(new Rect(inRect.xMin, inRect.yMin, inRect.width - 25f, 1f));
				Widgets.DrawBox(new Rect(inRect.xMin, inRect.yMax - 1f, inRect.width - 25f, 1f));
			}
			inRect.yMin = Mathf.Min(curYMin + this.inRect.yMin, this.inRect.yMax);
		}
		else
		{
			inRect.yMin = curYMin;
		}
		isOverflowing = false;
		GUIUtility.RestoreGUIState();
	}

	protected virtual RectSlice Slice(float height, bool includeMargins = true)
	{
		Rect rect = new Rect(inXMin, curYMin, inXMax - inXMin, includeMargins ? (height + margins.y) : height);
		Rect inside = new Rect(rect);
		if (includeMargins)
		{
			inside.xMin += margins.x * 2f;
			inside.xMax -= margins.x;
			inside.yMin += margins.y / 2f;
			inside.yMax -= margins.y / 2f;
		}
		curYMin += (includeMargins ? (height + margins.y) : height);
		if (drawBackground)
		{
			Widgets.DrawBoxSolid(rect, CollapsibleBGColor);
		}
		return new RectSlice(inside, rect);
	}
}
