using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.TextFormatting;

internal sealed class BidiState : Bidi.State
{
	private FormatSettings _settings;

	private int _cpFirst;

	private int _cpFirstScope;

	internal byte CurrentLevel => Bidi.BidiStack.GetMaximumLevel(base.LevelStack);

	public override DirectionClass LastNumberClass
	{
		get
		{
			if (NumberClass == DirectionClass.ClassInvalid)
			{
				GetLastDirectionClasses();
			}
			return NumberClass;
		}
		set
		{
			NumberClass = value;
		}
	}

	public override DirectionClass LastStrongClass
	{
		get
		{
			if (StrongCharClass == DirectionClass.ClassInvalid)
			{
				GetLastDirectionClasses();
			}
			return StrongCharClass;
		}
		set
		{
			StrongCharClass = value;
			NumberClass = value;
		}
	}

	public BidiState(FormatSettings settings, int cpFirst)
		: this(settings, cpFirst, null)
	{
	}

	public BidiState(FormatSettings settings, int cpFirst, TextModifierScope modifierScope)
		: base(settings.Pap.RightToLeft)
	{
		_settings = settings;
		_cpFirst = cpFirst;
		NumberClass = DirectionClass.ClassInvalid;
		StrongCharClass = DirectionClass.ClassInvalid;
		while (modifierScope != null && !modifierScope.TextModifier.HasDirectionalEmbedding)
		{
			modifierScope = modifierScope.ParentScope;
		}
		if (modifierScope != null)
		{
			_cpFirstScope = modifierScope.TextSourceCharacterIndex;
			Bidi.BidiStack bidiStack = new Bidi.BidiStack();
			bidiStack.Init(base.LevelStack);
			ushort overflowLevels = 0;
			InitLevelStackFromModifierScope(bidiStack, modifierScope, ref overflowLevels);
			base.LevelStack = bidiStack.GetData();
			base.Overflow = overflowLevels;
		}
	}

	internal void SetLastDirectionClassesAtLevelChange()
	{
		if ((CurrentLevel & 1) == 0)
		{
			LastStrongClass = DirectionClass.Left;
			LastNumberClass = DirectionClass.Left;
		}
		else
		{
			LastStrongClass = DirectionClass.ArabicLetter;
			LastNumberClass = DirectionClass.ArabicNumber;
		}
	}

	private void GetLastDirectionClasses()
	{
		DirectionClass strongClass = DirectionClass.ClassInvalid;
		DirectionClass numberClass = DirectionClass.ClassInvalid;
		bool flag = true;
		while (flag && _cpFirst > _cpFirstScope)
		{
			TextSpan<CultureSpecificCharacterBufferRange> precedingText = _settings.GetPrecedingText(_cpFirst);
			CultureSpecificCharacterBufferRange value = precedingText.Value;
			if (precedingText.Length <= 0)
			{
				break;
			}
			if (!value.CharacterBufferRange.IsEmpty)
			{
				flag = Bidi.GetLastStongAndNumberClass(value.CharacterBufferRange, ref strongClass, ref numberClass);
				if (strongClass != DirectionClass.ClassInvalid)
				{
					StrongCharClass = strongClass;
					if (NumberClass == DirectionClass.ClassInvalid)
					{
						if (numberClass == DirectionClass.EuropeanNumber)
						{
							numberClass = GetEuropeanNumberClassOverride(CultureMapper.GetSpecificCulture(value.CultureInfo));
						}
						NumberClass = numberClass;
					}
					break;
				}
			}
			_cpFirst -= precedingText.Length;
		}
		if (strongClass == DirectionClass.ClassInvalid)
		{
			StrongCharClass = (((CurrentLevel & 1) != 0) ? DirectionClass.ArabicLetter : DirectionClass.Left);
		}
		if (numberClass == DirectionClass.ClassInvalid)
		{
			NumberClass = (((CurrentLevel & 1) != 0) ? DirectionClass.ArabicNumber : DirectionClass.Left);
		}
	}

	private static void InitLevelStackFromModifierScope(Bidi.BidiStack stack, TextModifierScope scope, ref ushort overflowLevels)
	{
		Stack<TextModifier> stack2 = new Stack<TextModifier>(32);
		for (TextModifierScope textModifierScope = scope; textModifierScope != null; textModifierScope = textModifierScope.ParentScope)
		{
			if (textModifierScope.TextModifier.HasDirectionalEmbedding)
			{
				stack2.Push(textModifierScope.TextModifier);
			}
		}
		while (stack2.Count > 0)
		{
			TextModifier textModifier = stack2.Pop();
			if (overflowLevels > 0)
			{
				overflowLevels++;
			}
			else if (!stack.Push(textModifier.FlowDirection == FlowDirection.LeftToRight))
			{
				overflowLevels = 1;
			}
		}
	}

	internal DirectionClass GetEuropeanNumberClassOverride(CultureInfo cultureInfo)
	{
		if (cultureInfo != null && ((cultureInfo.LCID & 0xFF) == 1 || (cultureInfo.LCID & 0xFF) == 41) && (CurrentLevel & 1) != 0)
		{
			return DirectionClass.ArabicNumber;
		}
		return DirectionClass.EuropeanNumber;
	}
}
