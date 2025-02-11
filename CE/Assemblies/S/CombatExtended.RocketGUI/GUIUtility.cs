using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CombatExtended.RocketGUI;

public static class GUIUtility
{
	private struct FontState
	{
		public GameFont font;

		public GUIStyle curStyle;

		public FontState(GameFont font)
		{
			GameFont gameFont = Text.Font;
			Text.Font = font;
			this.font = font;
			curStyle = new GUIStyle(Text.CurFontStyle);
			Text.Font = gameFont;
		}

		public void Restore()
		{
			Text.Font = font;
			Text.CurFontStyle.fontSize = curStyle.fontSize;
			Text.CurFontStyle.fontStyle = curStyle.fontStyle;
			Text.CurFontStyle.alignment = curStyle.alignment;
		}
	}

	private struct GUIState
	{
		public GameFont gameFont;

		public FontState[] fonts;

		public Color color;

		public Color contentColor;

		public Color backgroundColor;

		public bool wordWrap;

		public static GUIState Copy()
		{
			GUIState result = default(GUIState);
			result.gameFont = Text.Font;
			result.fonts = new FontState[3]
			{
				new FontState(GameFont.Tiny),
				new FontState(GameFont.Small),
				new FontState(GameFont.Medium)
			};
			result.color = GUI.color;
			result.contentColor = GUI.contentColor;
			result.backgroundColor = GUI.backgroundColor;
			result.wordWrap = Text.WordWrap;
			return result;
		}

		public void Restore()
		{
			for (int i = 0; i < 3; i++)
			{
				fonts[i].Restore();
			}
			Text.Font = gameFont;
			GUI.color = color;
			GUI.contentColor = contentColor;
			GUI.backgroundColor = backgroundColor;
			Text.WordWrap = wordWrap;
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}

	private static readonly Color _altGray = new Color(0.2f, 0.2f, 0.2f);

	private static float[] _heights = new float[5000];

	private static Rect _r1 = new Rect(0f, 0f, 1f, 1f);

	private static readonly Color _hColor = new Color(0.2f, 0.2f, 0.2f, 1f);

	private static int depth = 0;

	private static GUIState initialState;

	private static readonly Dictionary<Tuple<string, GameFont, float, float, float>, float> textHeightCache = new Dictionary<Tuple<string, GameFont, float, float, float>, float>(512);

	public static Exception ExecuteSafeGUIAction(Action function, Action fallbackAction = null, bool catchExceptions = false)
	{
		StashGUIState();
		Exception ex = null;
		try
		{
			function();
		}
		catch (Exception ex2)
		{
			Log.Error($"ROCKETMAN:UI error in ExecuteSafeGUIAction {ex2}");
			ex = ex2;
		}
		finally
		{
			RestoreGUIState();
		}
		if (ex != null && !catchExceptions)
		{
			if (fallbackAction != null)
			{
				ex = ExecuteSafeGUIAction(fallbackAction);
			}
			if (ex != null)
			{
				throw ex;
			}
		}
		return ex;
	}

	public static void ScrollView<T>(Rect rect, ref Vector2 scrollPosition, IEnumerable<T> elements, Func<T, float> heightLambda, Action<Rect, T> elementLambda, Func<T, IComparable> orderByLambda = null, bool drawBackground = true, bool showScrollbars = true, bool catchExceptions = false, bool drawMouseOverHighlights = true)
	{
		StashGUIState();
		Exception ex = null;
		try
		{
			if (drawBackground)
			{
				Widgets.DrawMenuSection(rect);
				rect = rect.ContractedBy(2f);
			}
			Rect viewRect = new Rect(0f, 0f, showScrollbars ? (rect.width - 23f) : rect.width, 0f);
			IEnumerable<T> enumerable2;
			if (orderByLambda != null)
			{
				IEnumerable<T> enumerable = elements.OrderBy(orderByLambda);
				enumerable2 = enumerable;
			}
			else
			{
				enumerable2 = elements;
			}
			IEnumerable<T> enumerable3 = enumerable2;
			if (_heights.Length < enumerable3.Count())
			{
				_heights = new float[enumerable3.Count() * 2];
			}
			float width = (showScrollbars ? (rect.width - 16f) : rect.width);
			int num = 0;
			int num2 = 0;
			bool flag = true;
			foreach (T item in enumerable3)
			{
				float num3 = heightLambda(item);
				_heights[num++] = num3;
				viewRect.height += Math.Max(num3, 0f);
			}
			num = 0;
			Widgets.BeginScrollView(rect, ref scrollPosition, viewRect, showScrollbars);
			Rect rect2 = new Rect(1f, 0f, width, 0f);
			foreach (T item2 in enumerable3)
			{
				if (_heights[num] <= 0f)
				{
					num++;
					continue;
				}
				rect2.height = _heights[num];
				if (scrollPosition.y - 50f > rect2.yMax || scrollPosition.y + 50f + rect.height < rect2.yMin)
				{
					flag = false;
				}
				if (flag)
				{
					if (drawBackground && num2 % 2 == 0)
					{
						Widgets.DrawBoxSolid(rect2, _altGray);
					}
					if (drawMouseOverHighlights)
					{
						Widgets.DrawHighlightIfMouseover(rect2);
					}
					elementLambda(rect2, item2);
				}
				rect2.y += _heights[num];
				num2++;
				num++;
				flag = true;
			}
		}
		catch (Exception ex2)
		{
			Log.Error($"ROCKETMAN:UI error in ScrollView {ex2}");
			ex = ex2;
		}
		finally
		{
			RestoreGUIState();
			Widgets.EndScrollView();
		}
		if (ex != null && !catchExceptions)
		{
			throw ex;
		}
	}

	public static void GridView<T>(Rect rect, int columns, List<T> elements, Action<Rect, T> cellLambda, bool drawBackground = true, bool drawVerticalDivider = false)
	{
		ExecuteSafeGUIAction(delegate
		{
			if (drawBackground)
			{
				Widgets.DrawMenuSection(rect);
			}
			rect = rect.ContractedBy(1f);
			int num = (int)Math.Ceiling((decimal)elements.Count / (decimal)columns);
			float num2 = rect.width / (float)columns;
			float num3 = rect.height / (float)num;
			Rect arg = new Rect(0f, 0f, num2, num3);
			int num4 = 0;
			for (int i = 0; i < columns; i++)
			{
				if (num4 >= elements.Count)
				{
					break;
				}
				arg.x = (float)i * num2 + rect.x;
				for (int j = 0; j < num; j++)
				{
					if (num4 >= elements.Count)
					{
						break;
					}
					arg.y = (float)j * num3 + rect.y;
					cellLambda(arg, elements[num4++]);
				}
			}
		});
	}

	public static void DrawTexture(Rect rect, Texture2D texture, Material material = null)
	{
		if (material == null)
		{
			GUI.DrawTexture(rect, texture);
		}
		else if (Event.current.type == EventType.Repaint)
		{
			Graphics.DrawTexture(rect, texture, _r1, 0, 0, 0, 0, new Color(GUI.color.r * 0.5f, GUI.color.g * 0.5f, GUI.color.b * 0.5f, GUI.color.a * 0.5f), material);
		}
	}

	public static void DrawWeaponWithAttachments(Rect inRect, WeaponPlatformDef platform, IEnumerable<AttachmentLink> attachments, IEnumerable<WeaponPlatformDef.WeaponGraphicPart> parts = null, AttachmentLink highlight = null, Color? color = null, Material colorMat = null)
	{
		ExecuteSafeGUIAction(delegate
		{
			Color color2 = GUI.color;
			Texture2D uIWeaponTex = platform.UIWeaponTex;
			foreach (AttachmentLink attachment in attachments)
			{
				if (attachment.HasOutline && (highlight == null || attachment.CompatibleWith(highlight)))
				{
					DrawTex(inRect, attachment, attachment.UIOutlineTex, colorMat);
				}
			}
			AttachmentLink attachmentLink = highlight;
			if (attachmentLink != null && attachmentLink.HasOutline)
			{
				DrawTex(inRect, highlight, highlight.UIOutlineTex, colorMat);
			}
			if (parts != null)
			{
				foreach (WeaponPlatformDef.WeaponGraphicPart part in parts)
				{
					if (part.HasOutline && (highlight == null || part.slotTags.All((string s) => !highlight.attachment.slotTags.Contains(s))))
					{
						GUI.DrawTexture(inRect, part.UIOutlineTex, ScaleMode.StretchToFill);
					}
				}
			}
			if (highlight != null)
			{
				GUI.color = _hColor;
			}
			if (color.HasValue && color.HasValue)
			{
				GUI.color = color.Value;
			}
			DrawTexture(inRect, uIWeaponTex, colorMat);
			if (parts != null)
			{
				foreach (WeaponPlatformDef.WeaponGraphicPart part2 in parts)
				{
					if (part2.HasPartMat && (highlight == null || part2.slotTags.All((string s) => !highlight.attachment.slotTags.Contains(s))))
					{
						GUI.DrawTexture(inRect, part2.UIPartTex, ScaleMode.StretchToFill);
					}
				}
			}
			foreach (AttachmentLink attachment2 in attachments)
			{
				if (attachment2.HasAttachmentMat && (highlight == null || attachment2.CompatibleWith(highlight)))
				{
					DrawTex(inRect, attachment2, attachment2.UIAttachmentTex, colorMat);
				}
			}
			GUI.color = color2;
			AttachmentLink attachmentLink2 = highlight;
			if (attachmentLink2 != null && attachmentLink2.HasAttachmentMat)
			{
				DrawTex(inRect, highlight, highlight.UIAttachmentTex, colorMat);
			}
		});
		static void DrawTex(Rect rect, AttachmentLink link, Texture2D texture, Material mat)
		{
			if (link.HasDrawOffset)
			{
				rect.x -= rect.width * link.drawOffset.x;
				rect.y -= rect.height * link.drawOffset.y;
			}
			rect.xMin = rect.xMax - rect.width * link.drawScale.x;
			rect.yMin = rect.yMax - rect.height * link.drawScale.y;
			DrawTexture(rect, texture, mat);
		}
	}

	public static void DrawWeaponWithAttachments(Rect inRect, WeaponPlatform weapon, AttachmentLink highlight = null, Color? color = null, Material colorMat = null)
	{
		DrawWeaponWithAttachments(inRect, weapon.Platform, weapon.CurLinks, weapon.VisibleDefaultParts, highlight, color, colorMat);
	}

	public static void DropDownMenu<T>(Func<T, string> labelLambda, Action<T> selectedLambda, T[] options)
	{
		DropDownMenu(labelLambda, selectedLambda, options.AsEnumerable());
	}

	public static void DropDownMenu<T>(Func<T, string> labelLambda, Action<T> selectedLambda, IEnumerable<T> options)
	{
		ExecuteSafeGUIAction(delegate
		{
			Text.Font = GameFont.Small;
			FloatMenuUtility.MakeMenu(options, (T option) => labelLambda(option), (T option) => delegate
			{
				selectedLambda(option);
			});
		});
	}

	public static void Row(Rect rect, List<Action<Rect>> contentLambdas, bool drawDivider = true, bool drawBackground = false)
	{
		ExecuteSafeGUIAction(delegate
		{
			if (drawBackground)
			{
				Widgets.DrawMenuSection(rect);
			}
			float step = rect.width / (float)contentLambdas.Count;
			Rect curRect = new Rect(rect.x - 5f, rect.y, step - 10f, rect.height);
			for (int i = 0; i < contentLambdas.Count; i++)
			{
				Action<Rect> lambda = contentLambdas[i];
				if (drawDivider && i + 1 < contentLambdas.Count)
				{
					Vector2 start = new Vector2(curRect.xMax + 5f, curRect.yMin + 1f);
					Vector2 end = new Vector2(curRect.xMax + 5f, curRect.yMax - 1f);
					Widgets.DrawLine(start, end, Color.white, 1f);
				}
				ExecuteSafeGUIAction(delegate
				{
					lambda(curRect);
					curRect.x += step;
				});
			}
		});
	}

	public static void CheckBoxLabeled(Rect rect, string label, ref bool checkOn, bool disabled = false, bool monotone = false, float iconWidth = 20f, GameFont font = GameFont.Tiny, FontStyle fontStyle = FontStyle.Normal, bool placeCheckboxNearText = false, bool drawHighlightIfMouseover = true, Texture2D texChecked = null, Texture2D texUnchecked = null)
	{
		CheckBoxLabeled(rect, label, Color.white, ref checkOn, disabled, monotone, iconWidth, font, fontStyle, placeCheckboxNearText, drawHighlightIfMouseover, texChecked, texUnchecked);
	}

	public static void CheckBoxLabeled(Rect rect, string label, Color radioColor, ref bool checkOn, bool disabled = false, bool monotone = false, float iconWidth = 20f, GameFont font = GameFont.Tiny, FontStyle fontStyle = FontStyle.Normal, bool placeCheckboxNearText = false, bool drawHighlightIfMouseover = true, Texture2D texChecked = null, Texture2D texUnchecked = null)
	{
		bool checkOnInt = checkOn;
		ExecuteSafeGUIAction(delegate
		{
			Text.Font = font;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.CurFontStyle.fontStyle = fontStyle;
			if (placeCheckboxNearText)
			{
				rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + 24f + 10f);
			}
			Widgets.Label(rect, label);
			if (!disabled && Widgets.ButtonInvisible(rect))
			{
				checkOnInt = !checkOnInt;
				if (checkOnInt)
				{
					SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
				}
				else
				{
					SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
				}
			}
			Rect position = new Rect(0f, 0f, iconWidth, iconWidth)
			{
				center = rect.RightPartPixels(iconWidth).center
			};
			Color color = GUI.color;
			if (radioColor != Color.white)
			{
				GUI.color = radioColor;
			}
			else if (disabled || monotone)
			{
				GUI.color = Widgets.InactiveColor;
			}
			Texture image = ((!checkOnInt) ? ((texUnchecked != null) ? texUnchecked : Widgets.CheckboxOffTex) : ((texChecked != null) ? texChecked : Widgets.CheckboxOnTex));
			GUI.DrawTexture(position, image);
			if (disabled || monotone)
			{
				GUI.color = color;
			}
			if (drawHighlightIfMouseover)
			{
				Widgets.DrawHighlightIfMouseover(rect);
			}
		});
		checkOn = checkOnInt;
	}

	public static void ColorBoxDescription(Rect rect, Color color, string description)
	{
		Rect textRect = new Rect(rect.x + 30f, rect.y, rect.width - 30f, rect.height);
		Rect boxRect = new Rect(0f, 0f, 10f, 10f);
		boxRect.center = new Vector2(rect.xMin + 15f, rect.yMin + rect.height / 2f);
		ExecuteSafeGUIAction(delegate
		{
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.Font = GameFont.Tiny;
			Text.CurFontStyle.fontStyle = FontStyle.Normal;
			Widgets.DrawBoxSolid(boxRect, color);
			Widgets.Label(textRect, description);
		});
	}

	public static void StashGUIState()
	{
		if (depth == 0)
		{
			initialState = GUIState.Copy();
		}
		depth++;
	}

	public static void RestoreGUIState()
	{
		initialState.Restore();
		depth--;
	}

	public static void ClearGUIState()
	{
		depth = 0;
		initialState.Restore();
	}

	public static string Fit(this string text, Rect rect)
	{
		float textHeight = text.GetTextHeight(rect.width);
		if (textHeight <= rect.height)
		{
			return text;
		}
		return text.Substring(0, (int)((float)text.Length * textHeight / rect.height)) + "...";
	}

	public static float GetTextHeight(this string text, Rect rect)
	{
		return (text != null) ? CalcTextHeight(text, rect.width) : 0f;
	}

	public static float GetTextHeight(this string text, float width)
	{
		return (text != null) ? CalcTextHeight(text, width) : 0f;
	}

	public static float GetTextHeight(this TaggedString text, float width)
	{
		return ((string)text != null) ? CalcTextHeight(text, width) : 0f;
	}

	public static float CalcTextHeight(string text, float width)
	{
		Tuple<string, GameFont, float, float, float> gUIState = GetGUIState(text, width);
		if (textHeightCache.TryGetValue(gUIState, out var value))
		{
			return value;
		}
		return textHeightCache[gUIState] = Text.CalcHeight(text, width);
	}

	private static Tuple<string, GameFont, float, float, float> GetGUIState(string text, float width)
	{
		return new Tuple<string, GameFont, float, float, float>(text, Text.Font, width, Prefs.UIScale, Text.CurFontStyle.fontSize);
	}
}
