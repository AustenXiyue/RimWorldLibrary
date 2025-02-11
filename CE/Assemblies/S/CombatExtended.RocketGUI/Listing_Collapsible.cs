using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CombatExtended.RocketGUI;

public class Listing_Collapsible : IListing_Custom
{
	public class Group_Collapsible
	{
		private List<Listing_Collapsible> collapsibles;

		public List<Listing_Collapsible> AllCollapsibles => (collapsibles != null) ? collapsibles : (collapsibles = new List<Listing_Collapsible>());

		public void CollapseAll()
		{
			foreach (Listing_Collapsible allCollapsible in AllCollapsibles)
			{
				allCollapsible.expanded = false;
			}
		}

		public void Register(Listing_Collapsible collapsible)
		{
			AllCollapsibles.Add(collapsible);
			collapsible.expanded = false;
		}
	}

	private Group_Collapsible group;

	private bool expanded = false;

	public Group_Collapsible Group
	{
		get
		{
			return group;
		}
		set
		{
			group.AllCollapsibles.RemoveAll((Listing_Collapsible c) => c == this);
			group = value;
			group.Register(this);
		}
	}

	public bool Expanded
	{
		get
		{
			return expanded;
		}
		set
		{
			group.CollapseAll();
			expanded = value;
		}
	}

	public Listing_Collapsible(bool expanded = false, bool scrollViewOnOverflow = true, bool drawBorder = false, bool drawBackground = false)
		: base(scrollViewOnOverflow, drawBorder, drawBackground)
	{
		this.expanded = expanded;
		group = new Group_Collapsible();
	}

	public Listing_Collapsible(Group_Collapsible group, bool expanded = false, bool scrollViewOnOverflow = true, bool drawBorder = false, bool drawBackground = false)
		: base(scrollViewOnOverflow, drawBorder, drawBackground)
	{
		this.expanded = expanded;
		this.group = group;
		this.group.Register(this);
	}

	public virtual void Begin(Rect inRect)
	{
		base.Begin(inRect);
		Gap(2f);
	}

	public virtual void Begin(Rect inRect, TaggedString title, bool drawInfo = true, bool drawIcon = true, bool hightlightIfMouseOver = true, GameFont fontSize = GameFont.Small, FontStyle fontStyle = FontStyle.Normal)
	{
		base.Begin(inRect);
		GUIUtility.ExecuteSafeGUIAction(delegate
		{
			Text.Font = fontSize;
			Text.CurFontStyle.fontStyle = fontStyle;
			Text.Anchor = TextAnchor.MiddleLeft;
			RectSlice rectSlice = Slice(title.GetTextHeight(insideWidth - 30f));
			if (hightlightIfMouseOver)
			{
				Widgets.DrawHighlightIfMouseover(rectSlice.outside);
			}
			Rect titleRect = rectSlice.inside;
			GUIUtility.ExecuteSafeGUIAction(delegate
			{
				GUI.color = CollapsibleBGBorderColor;
				GUI.color = Color.gray;
				if (drawInfo)
				{
					Text.Font = GameFont.Tiny;
					Text.Anchor = TextAnchor.MiddleRight;
					Widgets.Label(titleRect, (!expanded) ? "Collapsed" : "Expanded");
				}
			});
			GUIUtility.ExecuteSafeGUIAction(delegate
			{
				Text.Font = fontSize;
				Text.CurFontStyle.fontStyle = fontStyle;
				if (drawBorder && drawBackground)
				{
					Text.CurFontStyle.fontSize = 12;
				}
				Text.Anchor = TextAnchor.MiddleLeft;
				GUI.color = CollapsibleBGBorderColor;
				GUI.color = Color.gray;
				if (drawIcon)
				{
					Widgets.DrawTextureFitted(titleRect.LeftPartPixels(25f), expanded ? TexButton.Collapse : TexButton.Reveal, 0.65f);
					titleRect.xMin += 35f;
				}
				GUI.color = Color.white;
				Widgets.Label(titleRect, title);
			});
			if (Widgets.ButtonInvisible(rectSlice.outside))
			{
				Expanded = !Expanded;
			}
			GUI.color = CollapsibleBGBorderColor;
			if (drawBorder)
			{
				Widgets.DrawBox(rectSlice.outside);
			}
		});
		if (Expanded && drawBorder)
		{
			Gap(2f);
		}
		if (!drawBorder)
		{
			Line(1f);
		}
		base.Start();
	}

	public void Label(TaggedString text, string tooltip = null, bool invert = false, bool hightlightIfMouseOver = true, GameFont fontSize = GameFont.Tiny, FontStyle fontStyle = FontStyle.Normal, TextAnchor anchor = TextAnchor.UpperLeft)
	{
		if (invert != expanded)
		{
			base.Label(text, GUI.color, tooltip, hightlightIfMouseOver, fontSize, fontStyle, anchor);
		}
	}

	public void Label(TaggedString text, Color color, string tooltip = null, bool invert = false, bool hightlightIfMouseOver = true, GameFont fontSize = GameFont.Tiny, FontStyle fontStyle = FontStyle.Normal, TextAnchor anchor = TextAnchor.UpperLeft)
	{
		if (invert != expanded)
		{
			base.Label(text, color, tooltip, hightlightIfMouseOver, fontSize, fontStyle, anchor);
		}
	}

	public bool CheckboxLabeled(TaggedString text, ref bool checkOn, string tooltip = null, bool invert = false, bool disabled = false, bool hightlightIfMouseOver = true, GameFont fontSize = GameFont.Tiny, FontStyle fontStyle = FontStyle.Normal)
	{
		if (invert == expanded)
		{
			return false;
		}
		return base.CheckboxLabeled(text, ref checkOn, tooltip, disabled, hightlightIfMouseOver, fontSize, fontStyle);
	}

	public void DropDownMenu<T>(string text, T selection, Func<T, string> labelLambda, Action<T> selectedLambda, IEnumerable<T> options, bool invert = false, bool disabled = false, GameFont fontSize = GameFont.Tiny, FontStyle fontStyle = FontStyle.Normal)
	{
		if (invert != expanded)
		{
			base.DropDownMenu(text, selection, labelLambda, selectedLambda, options, disabled, fontSize, fontStyle);
		}
	}

	public void Columns(float height, IEnumerable<Action<Rect>> lambdas, float gap = 5f, bool invert = false, bool useMargins = false, Action fallback = null)
	{
		if (invert != expanded)
		{
			base.Columns(height, lambdas, gap, useMargins, fallback);
		}
	}

	public void Lambda(float height, Action<Rect> contentLambda, bool invert = false, bool useMargins = false, bool hightlightIfMouseOver = false, Action fallback = null)
	{
		if (invert != expanded)
		{
			base.Lambda(height, contentLambda, useMargins, hightlightIfMouseOver, fallback);
		}
	}

	public bool ButtonText(TaggedString text, bool disabled = false, bool invert = false, bool drawBackground = true)
	{
		if (invert == expanded)
		{
			return false;
		}
		return base.ButtonText(text, disabled, drawBackground);
	}

	public void Gap(float height = 9f, bool invert = false)
	{
		if (expanded != invert)
		{
			base.Gap(height);
		}
	}

	public void Line(float thickness, bool invert = false)
	{
		if (expanded != invert)
		{
			base.Line(thickness);
		}
	}

	public override void End(ref Rect inRect)
	{
		base.End(ref inRect);
	}

	protected override RectSlice Slice(float height, bool includeMargins = true)
	{
		return base.Slice(height, includeMargins);
	}
}
