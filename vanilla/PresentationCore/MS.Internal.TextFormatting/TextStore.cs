using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.Generic;
using MS.Internal.PresentationCore;

namespace MS.Internal.TextFormatting;

internal class TextStore
{
	private struct TextEffectBoundary : IComparable<TextEffectBoundary>
	{
		private readonly int _position;

		private readonly bool _isStart;

		internal int Position => _position;

		internal bool IsStart => _isStart;

		internal TextEffectBoundary(int position, bool isStart)
		{
			_position = position;
			_isStart = isStart;
		}

		public int CompareTo(TextEffectBoundary other)
		{
			if (Position != other.Position)
			{
				return Position - other.Position;
			}
			if (IsStart == other.IsStart)
			{
				return 0;
			}
			if (!IsStart)
			{
				return 1;
			}
			return -1;
		}
	}

	[Flags]
	private enum NumberContext
	{
		Unknown = 0,
		Arabic = 1,
		European = 2,
		Mask = 3,
		FromLetter = 4,
		FromFlowDirection = 8
	}

	internal enum ObjectId : ushort
	{
		Reverse = 0,
		MaxNative = 1,
		InlineObject = 1,
		Max = 2,
		Text_chp = ushort.MaxValue
	}

	private FormatSettings _settings;

	private int _lscpFirstValue;

	private int _cpFirst;

	private int _lscchUpTo;

	private int _cchUpTo;

	private int _cchEol;

	private int _accNominalWidthSoFar;

	private int _accTextLengthSoFar;

	private NumberContext _numberContext;

	private int _cpNumberContext;

	private SpanVector _plsrunVector;

	private SpanPosition _plsrunVectorLatestPosition;

	private ArrayList _lsrunList;

	private BidiState _bidiState;

	private TextModifierScope _modifierScope;

	private int _formatWidth;

	private SpanVector _textObjectMetricsVector;

	internal static LSRun[] ControlRuns;

	internal const int LscpFirstMarker = -2147483647;

	internal const int TypicalCharactersPerLine = 100;

	internal const char CharLineSeparator = '\u2028';

	internal const char CharParaSeparator = '\u2029';

	internal const char CharLineFeed = '\n';

	internal const char CharCarriageReturn = '\r';

	internal const char CharTab = '\t';

	internal static nint PwchParaSeparator;

	internal static nint PwchLineSeparator;

	internal static nint PwchNbsp;

	internal static nint PwchHidden;

	internal static nint PwchObjectTerminator;

	internal static nint PwchObjectReplacement;

	internal const int MaxCharactersPerLine = 9600;

	private const int MaxCchWordToHyphenate = 80;

	private int BaseBidiLevel => _settings.Pap.RightToLeft ? 1 : 0;

	internal FormatSettings Settings => _settings;

	internal ParaProp Pap => _settings.Pap;

	internal int CpFirst => _cpFirst;

	internal SpanVector PlsrunVector => _plsrunVector;

	internal ArrayList LsrunList => _lsrunList;

	internal int FormatWidth => _formatWidth;

	internal int CchEol
	{
		get
		{
			return _cchEol;
		}
		set
		{
			_cchEol = value;
		}
	}

	static TextStore()
	{
		EscStringInfo escStringInfo = default(EscStringInfo);
		UnsafeNativeMethods.LoGetEscString(ref escStringInfo);
		ControlRuns = new LSRun[3];
		ControlRuns[0] = new LSRun(Plsrun.CloseAnchor, escStringInfo.szObjectTerminator);
		ControlRuns[1] = new LSRun(Plsrun.Reverse, escStringInfo.szObjectReplacement);
		ControlRuns[2] = new LSRun(Plsrun.FakeLineBreak, escStringInfo.szLineSeparator);
		PwchNbsp = escStringInfo.szNbsp;
		PwchHidden = escStringInfo.szHidden;
		PwchParaSeparator = escStringInfo.szParaSeparator;
		PwchLineSeparator = escStringInfo.szLineSeparator;
		PwchObjectReplacement = escStringInfo.szObjectReplacement;
		PwchObjectTerminator = escStringInfo.szObjectTerminator;
	}

	public TextStore(FormatSettings settings, int cpFirst, int lscpFirstValue, int formatWidth)
	{
		_settings = settings;
		_formatWidth = formatWidth;
		_cpFirst = cpFirst;
		_lscpFirstValue = lscpFirstValue;
		_lsrunList = new ArrayList(2);
		_plsrunVector = new SpanVector(null);
		_plsrunVectorLatestPosition = default(SpanPosition);
		TextLineBreak previousLineBreak = settings.PreviousLineBreak;
		if (previousLineBreak != null && previousLineBreak.TextModifierScope != null)
		{
			_modifierScope = previousLineBreak.TextModifierScope.CloneStack();
			_bidiState = new BidiState(_settings, _cpFirst, _modifierScope);
		}
	}

	internal LSRun FetchLSRun(int lscpFetch, TextFormattingMode textFormattingMode, bool isSideways, out Plsrun plsrun, out int lsrunOffset, out int lsrunLength)
	{
		lscpFetch -= _lscpFirstValue;
		Invariant.Assert(lscpFetch >= _cpFirst);
		if (_cpFirst + _lscchUpTo <= lscpFetch)
		{
			ushort num = 0;
			ushort num2 = 0;
			int num3 = 0;
			int num4 = _cchUpTo;
			int num5 = 0;
			int num6 = 0;
			SpanVector spanVector = new SpanVector(null);
			SpanVector textEffectsVector = new SpanVector(null);
			byte[] bidiLevels = null;
			int lastBidiLevel = GetLastLevel();
			do
			{
				IL_005c:
				TextRunInfo textRunInfo = FetchTextRun(_cpFirst + num4);
				if (textRunInfo != null)
				{
					if (_bidiState == null && (IsDirectionalModifier(textRunInfo.TextRun as TextModifier) || IsEndOfDirectionalModifier(textRunInfo)))
					{
						_bidiState = new BidiState(_settings, _cpFirst);
					}
					int num7 = textRunInfo.StringLength;
					if (textRunInfo.TextRun is ITextSymbols)
					{
						ushort mask;
						ushort num8;
						if (!textRunInfo.IsSymbol)
						{
							mask = 524;
							num8 = 2;
						}
						else
						{
							mask = 1032;
							num8 = 0;
						}
						num7 = Classification.AdvanceUntilUTF16(textRunInfo.CharacterBuffer, textRunInfo.OffsetToFirstChar, textRunInfo.Length, mask, out var charFlags);
						charFlags &= 0xFDFB;
						if (num7 <= 0)
						{
							textRunInfo = CreateSpecialRunFromTextContent(textRunInfo, num4);
							num7 = textRunInfo.StringLength;
							charFlags = textRunInfo.CharacterAttributeFlags;
						}
						else if (num7 != textRunInfo.Length)
						{
							textRunInfo.Length = num7;
						}
						textRunInfo.CharacterAttributeFlags |= charFlags;
						num |= charFlags;
						num2 |= (ushort)(charFlags & num8);
						num6 += num7;
					}
					_accNominalWidthSoFar += textRunInfo.GetRoughWidth(TextFormatterImp.ToIdeal);
					spanVector.SetReference(num5, num7, textRunInfo);
					TextEffectCollection textEffectCollection = ((textRunInfo.Properties != null) ? textRunInfo.Properties.TextEffects : null);
					if (textEffectCollection != null && textEffectCollection.Count != 0)
					{
						SetTextEffectsVector(textEffectsVector, num5, textRunInfo, textEffectCollection);
					}
					num5 += num7;
					num4 += textRunInfo.Length;
					if (_accNominalWidthSoFar < _formatWidth && !textRunInfo.IsEndOfLine && !IsNewline(num) && _accTextLengthSoFar + num6 <= 9600)
					{
						goto IL_005c;
					}
				}
				if (lastBidiLevel > 0 || num2 != 0 || _bidiState != null)
				{
					num3 = BidiAnalyze(spanVector, num5, out bidiLevels);
					if (num3 == 0 && _accTextLengthSoFar + num6 >= 9600)
					{
						num3 = num5;
						bidiLevels = null;
					}
				}
				else
				{
					num3 = num5;
				}
			}
			while (num3 <= 0);
			bool flag = IsForceBreakRequired(spanVector, ref num3);
			if (bidiLevels == null)
			{
				CreateLSRunsUniformBidiLevel(spanVector, textEffectsVector, _cchUpTo, 0, num3, 0, textFormattingMode, isSideways, ref lastBidiLevel);
			}
			else
			{
				int cchUpTo = _cchUpTo;
				int j;
				for (int i = 0; i < num3; i += j)
				{
					j = 1;
					int num9;
					for (num9 = bidiLevels[i]; i + j < num3 && bidiLevels[i + j] == num9; j++)
					{
					}
					CreateLSRunsUniformBidiLevel(spanVector, textEffectsVector, cchUpTo, i, j, num9, textFormattingMode, isSideways, ref lastBidiLevel);
				}
			}
			if (flag)
			{
				if (lastBidiLevel != 0)
				{
					lastBidiLevel = CreateReverseLSRuns(BaseBidiLevel, lastBidiLevel);
				}
				_plsrunVectorLatestPosition = _plsrunVector.SetValue(_lscchUpTo, 1, Plsrun.FakeLineBreak, _plsrunVectorLatestPosition);
				_lscchUpTo++;
			}
		}
		return GrabLSRun(lscpFetch, out plsrun, out lsrunOffset, out lsrunLength);
	}

	internal TextRunInfo FetchTextRun(int cpFetch)
	{
		TextRun textRun;
		int runLength;
		CharacterBufferRange charBufferRange = _settings.FetchTextRun(cpFetch, _cpFirst, out textRun, out runLength);
		CultureInfo digitCulture = null;
		bool contextualSubstitution = false;
		bool flag = false;
		Plsrun runType = TextRunInfo.GetRunType(textRun);
		if (runType == Plsrun.Text)
		{
			TextRunProperties properties = textRun.Properties;
			flag = properties.Typeface.Symbol;
			if (!flag)
			{
				_settings.DigitState.SetTextRunProperties(properties);
				digitCulture = _settings.DigitState.DigitCulture;
				contextualSubstitution = _settings.DigitState.Contextual;
			}
		}
		TextModifierScope modifierScope = _modifierScope;
		if (textRun is TextModifier modifier)
		{
			_modifierScope = new TextModifierScope(_modifierScope, modifier, cpFetch);
			modifierScope = _modifierScope;
		}
		else if (_modifierScope != null && textRun is TextEndOfSegment)
		{
			_modifierScope = _modifierScope.ParentScope;
		}
		return new TextRunInfo(charBufferRange, runLength, cpFetch - _cpFirst, textRun, runType, 0, digitCulture, contextualSubstitution, flag, modifierScope);
	}

	private void SetTextEffectsVector(SpanVector textEffectsVector, int ich, TextRunInfo runInfo, TextEffectCollection textEffects)
	{
		int num = _cpFirst + _cchUpTo + ich;
		int num2 = num - _settings.TextSource.GetTextEffectCharacterIndexFromTextSourceCharacterIndex(num);
		int count = textEffects.Count;
		TextEffectBoundary[] array = new TextEffectBoundary[count * 2];
		for (int i = 0; i < count; i++)
		{
			TextEffect textEffect = textEffects[i];
			array[2 * i] = new TextEffectBoundary(textEffect.PositionStart, isStart: true);
			array[2 * i + 1] = new TextEffectBoundary(textEffect.PositionStart + textEffect.PositionCount, isStart: false);
		}
		Array.Sort(array);
		int num3 = Math.Max(num - num2, array[0].Position);
		int num4 = Math.Min(num - num2 + runInfo.Length, array[^1].Position);
		int num5 = 0;
		int num6 = num3;
		for (int j = 0; j < array.Length; j++)
		{
			if (num6 >= num4)
			{
				break;
			}
			if (num6 < array[j].Position && num5 > 0)
			{
				int num7 = Math.Min(array[j].Position, num4);
				IList<TextEffect> list = new TextEffect[num5];
				int num8 = 0;
				for (int k = 0; k < count; k++)
				{
					TextEffect textEffect2 = textEffects[k];
					if (num6 >= textEffect2.PositionStart && num6 < textEffect2.PositionStart + textEffect2.PositionCount)
					{
						list[num8++] = textEffect2;
					}
				}
				Invariant.Assert(num8 == num5);
				textEffectsVector.SetReference(num6 + num2 - _cchUpTo - _cpFirst, num7 - num6, list);
				num6 = num7;
			}
			num5 += (array[j].IsStart ? 1 : (-1));
			if (num5 == 0 && j < array.Length - 1)
			{
				Invariant.Assert(array[j + 1].IsStart);
				num6 = Math.Max(num6, array[j + 1].Position);
			}
		}
	}

	private unsafe TextRunInfo CreateSpecialRunFromTextContent(TextRunInfo runInfo, int cchFetched)
	{
		CharacterBuffer characterBuffer = runInfo.CharacterBuffer;
		int offsetToFirstChar = runInfo.OffsetToFirstChar;
		char c = characterBuffer[offsetToFirstChar];
		ushort flags = Classification.CharAttributeOf(Classification.GetUnicodeClassUTF16(c)).Flags;
		if ((flags & 4) != 0)
		{
			int num = 1;
			if (c == '\r')
			{
				if (runInfo.Length > 1)
				{
					num += ((characterBuffer[offsetToFirstChar + 1] == '\n') ? 1 : 0);
				}
				else
				{
					TextRunInfo textRunInfo = FetchTextRun(_cpFirst + cchFetched + 1);
					if (textRunInfo != null && textRunInfo.TextRun is ITextSymbols)
					{
						num += ((textRunInfo.CharacterBuffer[textRunInfo.OffsetToFirstChar] == '\n') ? 1 : 0);
					}
				}
			}
			runInfo = new TextRunInfo(new CharacterBufferRange((char*)PwchLineSeparator, 1), num, runInfo.OffsetToFirstCp, runInfo.TextRun, Plsrun.LineBreak, flags, null, contextualSubstitution: false, symbolTypeface: false, runInfo.TextModifierScope);
		}
		else if ((flags & 0x200) != 0)
		{
			runInfo = new TextRunInfo(new CharacterBufferRange((char*)PwchParaSeparator, 1), 1, runInfo.OffsetToFirstCp, runInfo.TextRun, Plsrun.ParaBreak, flags, null, contextualSubstitution: false, symbolTypeface: false, runInfo.TextModifierScope);
		}
		else
		{
			Invariant.Assert((flags & 8) != 0);
			runInfo = new TextRunInfo(new CharacterBufferRange((char*)PwchNbsp, 1), 1, runInfo.OffsetToFirstCp, runInfo.TextRun, runInfo.Plsrun, flags, null, contextualSubstitution: false, symbolTypeface: false, runInfo.TextModifierScope);
		}
		return runInfo;
	}

	private LSRun GrabLSRun(int lscpFetch, out Plsrun plsrun, out int lsrunOffset, out int lsrunLength)
	{
		int num = lscpFetch - _cpFirst;
		SpanRider spanRider = new SpanRider(_plsrunVector, _plsrunVectorLatestPosition, num);
		_plsrunVectorLatestPosition = spanRider.SpanPosition;
		plsrun = (Plsrun)spanRider.CurrentElement;
		LSRun result;
		if (plsrun < Plsrun.FormatAnchor)
		{
			result = ControlRuns[(uint)plsrun];
			lsrunOffset = 0;
		}
		else
		{
			result = (LSRun)_lsrunList[(int)(ToIndex(plsrun) - 3)];
			lsrunOffset = num - spanRider.CurrentSpanStart;
		}
		if (_lscpFirstValue != 0)
		{
			plsrun = MakePlsrunMarker(plsrun);
		}
		lsrunLength = spanRider.Length;
		return result;
	}

	private int GetLastLevel()
	{
		if (_lscchUpTo > 0)
		{
			SpanRider spanRider = new SpanRider(_plsrunVector, _plsrunVectorLatestPosition, _lscchUpTo - 1);
			_plsrunVectorLatestPosition = spanRider.SpanPosition;
			return GetRun((Plsrun)spanRider.CurrentElement).BidiLevel;
		}
		return BaseBidiLevel;
	}

	private int BidiAnalyze(SpanVector runInfoVector, int stringLength, out byte[] bidiLevels)
	{
		CharacterBuffer characterBuffer = null;
		SpanRider spanRider = new SpanRider(runInfoVector);
		int num;
		if (spanRider.Length >= stringLength)
		{
			TextRunInfo textRunInfo = (TextRunInfo)spanRider.CurrentElement;
			if (!textRunInfo.IsSymbol)
			{
				characterBuffer = textRunInfo.CharacterBuffer;
				num = textRunInfo.OffsetToFirstChar;
			}
			else
			{
				characterBuffer = new StringCharacterBuffer(new string('A', stringLength));
				num = 0;
			}
		}
		else
		{
			int i = 0;
			StringBuilder stringBuilder = new StringBuilder(stringLength);
			int length;
			for (; i < stringLength; i += length)
			{
				spanRider.At(i);
				length = spanRider.Length;
				TextRunInfo textRunInfo2 = (TextRunInfo)spanRider.CurrentElement;
				if (!textRunInfo2.IsSymbol)
				{
					textRunInfo2.CharacterBuffer.AppendToStringBuilder(stringBuilder, textRunInfo2.OffsetToFirstChar, length);
				}
				else
				{
					stringBuilder.Append('A', length);
				}
			}
			characterBuffer = new StringCharacterBuffer(stringBuilder.ToString());
			num = 0;
		}
		if (_bidiState == null)
		{
			_bidiState = new BidiState(_settings, _cpFirst);
		}
		bidiLevels = new byte[stringLength];
		DirectionClass[] array = new DirectionClass[stringLength];
		int num2 = 0;
		for (int j = 0; j < runInfoVector.Count; j++)
		{
			int cchResolved = 0;
			TextRunInfo textRunInfo3 = (TextRunInfo)runInfoVector[j].element;
			TextModifier textModifier = textRunInfo3.TextRun as TextModifier;
			if (IsDirectionalModifier(textModifier))
			{
				bidiLevels[num2] = AnalyzeDirectionalModifier(_bidiState, textModifier.FlowDirection);
				cchResolved = 1;
			}
			else if (IsEndOfDirectionalModifier(textRunInfo3))
			{
				bidiLevels[num2] = AnalyzeEndOfDirectionalModifier(_bidiState);
				cchResolved = 1;
			}
			else
			{
				int num3 = num2;
				while (true)
				{
					CultureInfo specificCulture = CultureMapper.GetSpecificCulture((textRunInfo3.Properties == null) ? null : textRunInfo3.Properties.CultureInfo);
					DirectionClass europeanNumberClassOverride = _bidiState.GetEuropeanNumberClassOverride(specificCulture);
					for (int k = 0; k < runInfoVector[j].length; k++)
					{
						array[num3 + k] = europeanNumberClassOverride;
					}
					num3 += runInfoVector[j].length;
					if (++j >= runInfoVector.Count)
					{
						break;
					}
					textRunInfo3 = (TextRunInfo)runInfoVector[j].element;
					if (textRunInfo3.Plsrun == Plsrun.Hidden && (IsDirectionalModifier(textRunInfo3.TextRun as TextModifier) || IsEndOfDirectionalModifier(textRunInfo3)))
					{
						j--;
						break;
					}
				}
				Bidi.Flags flags = ((j < runInfoVector.Count) ? ((Bidi.Flags)200u) : ((Bidi.Flags)216u));
				Bidi.BidiAnalyzeInternal(characterBuffer, num + num2, num3 - num2, 0, flags, _bidiState, new PartialArray<byte>(bidiLevels, num2, num3 - num2), new PartialArray<DirectionClass>(array, num2, num3 - num2), out cchResolved);
				Invariant.Assert(cchResolved == num3 - num2 || (flags & Bidi.Flags.IncompleteText) != 0);
			}
			num2 += cchResolved;
		}
		Invariant.Assert(num2 <= bidiLevels.Length);
		return num2;
	}

	private byte AnalyzeDirectionalModifier(BidiState state, FlowDirection flowDirection)
	{
		bool pushToGreaterEven = flowDirection == FlowDirection.LeftToRight;
		ulong stack = state.LevelStack;
		byte maximumLevel = Bidi.BidiStack.GetMaximumLevel(stack);
		if (!Bidi.BidiStack.Push(ref stack, pushToGreaterEven, out var _))
		{
			state.Overflow++;
		}
		state.LevelStack = stack;
		state.SetLastDirectionClassesAtLevelChange();
		return maximumLevel;
	}

	private byte AnalyzeEndOfDirectionalModifier(BidiState state)
	{
		if (state.Overflow > 0)
		{
			state.Overflow--;
			return state.CurrentLevel;
		}
		ulong stack = state.LevelStack;
		Invariant.Assert(Bidi.BidiStack.Pop(ref stack, out var topLevel));
		state.LevelStack = stack;
		state.SetLastDirectionClassesAtLevelChange();
		return topLevel;
	}

	private bool IsEndOfDirectionalModifier(TextRunInfo runInfo)
	{
		if (runInfo.TextModifierScope != null && runInfo.TextModifierScope.TextModifier.HasDirectionalEmbedding)
		{
			return runInfo.TextRun is TextEndOfSegment;
		}
		return false;
	}

	private bool IsDirectionalModifier(TextModifier modifier)
	{
		return modifier?.HasDirectionalEmbedding ?? false;
	}

	internal bool InsertFakeLineBreak(int cpLimit)
	{
		int i = 0;
		int num = 0;
		int num2 = 0;
		for (; i < _plsrunVector.Count; i++)
		{
			Span span = _plsrunVector[i];
			Plsrun plsrun = (Plsrun)span.element;
			if (plsrun >= Plsrun.FormatAnchor)
			{
				LSRun run = GetRun(plsrun);
				if (num + run.Length >= cpLimit)
				{
					_plsrunVector.Delete(i + 1, _plsrunVector.Count - (i + 1), ref _plsrunVectorLatestPosition);
					if (run.Type == Plsrun.Text && num + run.Length > cpLimit)
					{
						run.Truncate(cpLimit - num);
						span.length = run.Length;
					}
					_lscchUpTo = num2 + run.Length;
					CreateReverseLSRuns(BaseBidiLevel, run.BidiLevel);
					_plsrunVectorLatestPosition = _plsrunVector.SetValue(_lscchUpTo, 1, Plsrun.FakeLineBreak, _plsrunVectorLatestPosition);
					_lscchUpTo++;
					return true;
				}
				num += run.Length;
			}
			num2 += span.length;
		}
		return false;
	}

	private bool IsForceBreakRequired(SpanVector runInfoVector, ref int cchToAdd)
	{
		bool result = false;
		int num = 0;
		for (int i = 0; i < runInfoVector.Count; i++)
		{
			if (num >= cchToAdd)
			{
				break;
			}
			Span span = runInfoVector[i];
			TextRunInfo textRunInfo = (TextRunInfo)span.element;
			int num2 = Math.Min(span.length, cchToAdd - num);
			if (textRunInfo.Plsrun == Plsrun.Text && !IsNewline(textRunInfo.CharacterAttributeFlags))
			{
				if (_accTextLengthSoFar + num2 <= 9600)
				{
					_accTextLengthSoFar += num2;
				}
				else
				{
					num2 = 9600 - _accTextLengthSoFar;
					_accTextLengthSoFar = 9600;
					cchToAdd = num + num2;
					result = true;
				}
			}
			num += num2;
		}
		return result;
	}

	private NumberContext GetNumberContext(TextModifierScope scope)
	{
		int num = _cpFirst + _cchUpTo;
		int num2 = _cpNumberContext;
		NumberContext numberContext = _numberContext;
		while (scope != null)
		{
			if (scope.TextModifier.HasDirectionalEmbedding)
			{
				int textSourceCharacterIndex = scope.TextSourceCharacterIndex;
				if (textSourceCharacterIndex >= _cpNumberContext)
				{
					num2 = textSourceCharacterIndex;
					numberContext = NumberContext.Unknown;
				}
				break;
			}
			scope = scope.ParentScope;
		}
		bool flag = ((scope != null) ? (scope.TextModifier.FlowDirection == FlowDirection.RightToLeft) : Pap.RightToLeft);
		while (num > num2)
		{
			TextSpan<CultureSpecificCharacterBufferRange> precedingText = _settings.GetPrecedingText(num);
			if (precedingText.Length <= 0)
			{
				break;
			}
			CharacterBufferRange characterBufferRange = precedingText.Value.CharacterBufferRange;
			if (!characterBufferRange.IsEmpty)
			{
				CharacterBuffer characterBuffer = characterBufferRange.CharacterBuffer;
				int num3 = characterBufferRange.OffsetToFirstChar + characterBufferRange.Length;
				int num4 = num3 - Math.Min(characterBufferRange.Length, num - num2);
				for (int num5 = num3 - 1; num5 >= num4; num5--)
				{
					CharacterAttribute characterAttribute = Classification.CharAttributeOf(Classification.GetUnicodeClassUTF16(characterBuffer[num5]));
					ushort num6 = (ushort)(characterAttribute.Flags & 0x804);
					if (num6 != 0)
					{
						if ((num6 & 0x800) != 0)
						{
							if (characterAttribute.Script != 1 && characterAttribute.Script != 49)
							{
								return NumberContext.European | NumberContext.FromLetter;
							}
							return NumberContext.Arabic | NumberContext.FromLetter;
						}
						if (!flag)
						{
							return NumberContext.European | NumberContext.FromFlowDirection;
						}
						return NumberContext.Arabic | NumberContext.FromFlowDirection;
					}
				}
			}
			num -= precedingText.Length;
		}
		if (num <= num2 && (numberContext & NumberContext.FromLetter) != 0)
		{
			return numberContext;
		}
		if (!flag)
		{
			return NumberContext.European | NumberContext.FromFlowDirection;
		}
		return NumberContext.Arabic | NumberContext.FromFlowDirection;
	}

	private void CreateLSRunsUniformBidiLevel(SpanVector runInfoVector, SpanVector textEffectsVector, int runInfoFirstCp, int ichUniform, int cchUniform, int uniformBidiLevel, TextFormattingMode textFormattingMode, bool isSideways, ref int lastBidiLevel)
	{
		int num = 0;
		SpanRider spanRider = new SpanRider(runInfoVector);
		SpanRider spanRider2 = new SpanRider(textEffectsVector);
		while (num < cchUniform)
		{
			spanRider.At(ichUniform + num);
			spanRider2.At(ichUniform + num);
			int num2 = Math.Min(spanRider.Length, spanRider2.Length);
			int num3 = Math.Min(num + num2, cchUniform);
			TextRunInfo textRunInfo = (TextRunInfo)spanRider.CurrentElement;
			IList<TextEffect> textEffects = (IList<TextEffect>)spanRider2.CurrentElement;
			CultureInfo digitCulture = null;
			NumberContext numberContext = NumberContext.Unknown;
			int textRunLength;
			if ((textRunInfo.CharacterAttributeFlags & 0x100) != 0)
			{
				if (!textRunInfo.ContextualSubstitution)
				{
					digitCulture = textRunInfo.DigitCulture;
				}
				else
				{
					NumberContext numberContext2 = NumberContext.Unknown;
					CharacterBuffer characterBuffer = textRunInfo.CharacterBuffer;
					int num4 = ichUniform - spanRider.CurrentSpanStart + textRunInfo.OffsetToFirstChar;
					for (int i = num; i < num3; i++)
					{
						CharacterAttribute characterAttribute = Classification.CharAttributeOf(Classification.GetUnicodeClassUTF16(characterBuffer[i + num4]));
						if ((characterAttribute.Flags & 0x100) != 0)
						{
							if (numberContext == NumberContext.Unknown)
							{
								numberContext = GetNumberContext(textRunInfo.TextModifierScope);
							}
							if ((numberContext2 & NumberContext.Mask) != (numberContext & NumberContext.Mask))
							{
								if (numberContext2 != 0)
								{
									CreateLSRuns(textRunInfo, textEffects, digitCulture, ichUniform + num - spanRider.CurrentSpanStart, i - num, uniformBidiLevel, textFormattingMode, isSideways, ref lastBidiLevel, out textRunLength);
									_cchUpTo += textRunLength;
									num = i;
								}
								digitCulture = (((numberContext & NumberContext.Mask) == NumberContext.Arabic) ? textRunInfo.DigitCulture : null);
								numberContext2 = numberContext;
							}
						}
						else if ((characterAttribute.Flags & 0x800) != 0)
						{
							numberContext = ((characterAttribute.Script == 1 || characterAttribute.Script == 49) ? (NumberContext.Arabic | NumberContext.FromLetter) : (NumberContext.European | NumberContext.FromLetter));
						}
					}
				}
			}
			CreateLSRuns(textRunInfo, textEffects, digitCulture, ichUniform + num - spanRider.CurrentSpanStart, num3 - num, uniformBidiLevel, textFormattingMode, isSideways, ref lastBidiLevel, out textRunLength);
			_cchUpTo += textRunLength;
			num = num3;
			if (numberContext != 0)
			{
				_numberContext = numberContext;
				_cpNumberContext = _cpFirst + _cchUpTo;
			}
		}
	}

	private int CreateReverseLSRuns(int currentBidiLevel, int lastBidiLevel)
	{
		int num = currentBidiLevel - lastBidiLevel;
		Plsrun plsrun;
		if (num > 0)
		{
			plsrun = Plsrun.Reverse;
		}
		else
		{
			plsrun = Plsrun.CloseAnchor;
			num = -num;
		}
		for (int i = 0; i < num; i++)
		{
			_plsrunVectorLatestPosition = _plsrunVector.SetValue(_lscchUpTo, 1, plsrun, _plsrunVectorLatestPosition);
			_lscchUpTo++;
		}
		return currentBidiLevel;
	}

	private void CreateLSRuns(TextRunInfo runInfo, IList<TextEffect> textEffects, CultureInfo digitCulture, int offsetToFirstChar, int stringLength, int uniformBidiLevel, TextFormattingMode textFormattingMode, bool isSideways, ref int lastBidiLevel, out int textRunLength)
	{
		LSRun lSRun = null;
		int lsrunLength = 0;
		textRunLength = 0;
		switch (runInfo.Plsrun)
		{
		case Plsrun.Text:
		{
			ushort characterAttributeFlags = runInfo.CharacterAttributeFlags;
			Invariant.Assert(!IsNewline(characterAttributeFlags));
			if ((characterAttributeFlags & 8) != 0)
			{
				lSRun = new LSRun(runInfo, Plsrun.FormatAnchor, PwchNbsp, 1, runInfo.OffsetToFirstCp, (byte)uniformBidiLevel);
				lsrunLength = (textRunLength = lSRun.StringLength);
			}
			else
			{
				lsrunLength = (textRunLength = stringLength);
				CreateTextLSRuns(runInfo, textEffects, digitCulture, offsetToFirstChar, stringLength, uniformBidiLevel, textFormattingMode, isSideways, ref lastBidiLevel);
			}
			break;
		}
		case Plsrun.InlineObject:
		{
			double toIdeal = TextFormatterImp.ToIdeal;
			lSRun = new LSRun(runInfo, textEffects, Plsrun.InlineObject, runInfo.OffsetToFirstCp, runInfo.Length, (int)Math.Round(toIdeal * runInfo.TextRun.Properties.FontRenderingEmSize), 0, new CharacterBufferRange(runInfo.CharacterBuffer, 0, stringLength), null, toIdeal, (byte)uniformBidiLevel);
			lsrunLength = stringLength;
			textRunLength = runInfo.Length;
			break;
		}
		case Plsrun.LineBreak:
			uniformBidiLevel = (Pap.RightToLeft ? 1 : 0);
			lSRun = CreateLineBreakLSRun(runInfo, stringLength, out lsrunLength, out textRunLength);
			break;
		case Plsrun.ParaBreak:
			lSRun = CreateLineBreakLSRun(runInfo, stringLength, out lsrunLength, out textRunLength);
			break;
		case Plsrun.Hidden:
			lsrunLength = (textRunLength = runInfo.Length - offsetToFirstChar);
			lSRun = new LSRun(runInfo, Plsrun.Hidden, PwchHidden, textRunLength, runInfo.OffsetToFirstCp, (byte)uniformBidiLevel);
			break;
		}
		if (lSRun != null)
		{
			if (lastBidiLevel != uniformBidiLevel)
			{
				lastBidiLevel = CreateReverseLSRuns(uniformBidiLevel, lastBidiLevel);
			}
			_plsrunVectorLatestPosition = _plsrunVector.SetValue(_lscchUpTo, lsrunLength, AddLSRun(lSRun), _plsrunVectorLatestPosition);
			_lscchUpTo += lsrunLength;
		}
	}

	private void CreateTextLSRuns(TextRunInfo runInfo, IList<TextEffect> textEffects, CultureInfo digitCulture, int offsetToFirstChar, int stringLength, int uniformBidiLevel, TextFormattingMode textFormattingMode, bool isSideways, ref int lastBidiLevel)
	{
		IList<TextShapeableSymbols> list = null;
		if (runInfo.TextRun is ITextSymbols textSymbols)
		{
			bool isRightToLeftParagraph = ((runInfo.TextModifierScope != null) ? (runInfo.TextModifierScope.TextModifier.FlowDirection == FlowDirection.RightToLeft) : _settings.Pap.RightToLeft);
			list = textSymbols.GetTextShapeableSymbols(_settings.Formatter.GlyphingCache, new CharacterBufferReference(runInfo.CharacterBuffer, runInfo.OffsetToFirstChar + offsetToFirstChar), stringLength, (uniformBidiLevel & 1) != 0, isRightToLeftParagraph, digitCulture, runInfo.TextModifierScope, textFormattingMode, isSideways);
		}
		else if (runInfo.TextRun is TextShapeableSymbols textShapeableSymbols)
		{
			list = new TextShapeableSymbols[1] { textShapeableSymbols };
		}
		if (list == null)
		{
			throw new NotSupportedException();
		}
		double toIdeal = TextFormatterImp.ToIdeal;
		int num = 0;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			TextShapeableSymbols textShapeableSymbols2 = list[i];
			int length = textShapeableSymbols2.Length;
			LSRun lsrun = new LSRun(runInfo, textEffects, Plsrun.Text, runInfo.OffsetToFirstCp + offsetToFirstChar + num, length, (int)Math.Round(toIdeal * runInfo.TextRun.Properties.FontRenderingEmSize), runInfo.CharacterAttributeFlags, new CharacterBufferRange(runInfo.CharacterBuffer, runInfo.OffsetToFirstChar + offsetToFirstChar + num, length), textShapeableSymbols2, toIdeal, (byte)uniformBidiLevel);
			if (uniformBidiLevel != lastBidiLevel)
			{
				lastBidiLevel = CreateReverseLSRuns(uniformBidiLevel, lastBidiLevel);
			}
			_plsrunVectorLatestPosition = _plsrunVector.SetValue(_lscchUpTo, length, AddLSRun(lsrun), _plsrunVectorLatestPosition);
			_lscchUpTo += length;
			num += length;
		}
	}

	private LSRun CreateLineBreakLSRun(TextRunInfo runInfo, int stringLength, out int lsrunLength, out int textRunLength)
	{
		lsrunLength = stringLength;
		textRunLength = runInfo.Length;
		return new LSRun(runInfo, null, runInfo.Plsrun, runInfo.OffsetToFirstCp, runInfo.Length, 0, runInfo.CharacterAttributeFlags, new CharacterBufferRange(runInfo.CharacterBuffer, runInfo.OffsetToFirstChar, stringLength), null, TextFormatterImp.ToIdeal, Pap.RightToLeft ? ((byte)1) : ((byte)0));
	}

	private Plsrun AddLSRun(LSRun lsrun)
	{
		if (lsrun.Type < Plsrun.FormatAnchor)
		{
			return lsrun.Type;
		}
		Plsrun plsrun = (Plsrun)(_lsrunList.Count + 3);
		if (lsrun.IsSymbol)
		{
			plsrun = MakePlsrunSymbol(plsrun);
		}
		_lsrunList.Add(lsrun);
		return plsrun;
	}

	internal int GetExternalCp(int lscp)
	{
		lscp -= _lscpFirstValue;
		SpanRider spanRider = new SpanRider(_plsrunVector, _plsrunVectorLatestPosition, lscp - _cpFirst);
		_plsrunVectorLatestPosition = spanRider.SpanPosition;
		return GetRun((Plsrun)spanRider.CurrentElement).OffsetToFirstCp + lscp - spanRider.CurrentSpanStart;
	}

	internal LSRun GetRun(Plsrun plsrun)
	{
		plsrun = ToIndex(plsrun);
		return (LSRun)(IsContent(plsrun) ? _lsrunList[(int)(plsrun - 3)] : ControlRuns[(uint)plsrun]);
	}

	internal static bool IsMarker(Plsrun plsrun)
	{
		return (plsrun & Plsrun.IsMarker) != 0;
	}

	internal static Plsrun MakePlsrunMarker(Plsrun plsrun)
	{
		return plsrun | Plsrun.IsMarker;
	}

	internal static Plsrun MakePlsrunSymbol(Plsrun plsrun)
	{
		return plsrun | Plsrun.IsSymbol;
	}

	internal static Plsrun ToIndex(Plsrun plsrun)
	{
		return plsrun & Plsrun.UnmaskAll;
	}

	internal static bool IsContent(Plsrun plsrun)
	{
		plsrun = ToIndex(plsrun);
		return plsrun >= Plsrun.FormatAnchor;
	}

	internal static bool IsSpace(char ch)
	{
		if (ch != ' ')
		{
			return ch == '\u00a0';
		}
		return true;
	}

	internal static bool IsStrong(char ch)
	{
		return Classification.CharAttributeOf(Classification.GetUnicodeClass(ch)).ItemClass == 5;
	}

	internal static bool IsNewline(Plsrun plsrun)
	{
		if (plsrun != Plsrun.LineBreak)
		{
			return plsrun == Plsrun.ParaBreak;
		}
		return true;
	}

	internal static bool IsNewline(ushort flags)
	{
		if ((flags & 4) == 0)
		{
			return (flags & 0x200) != 0;
		}
		return true;
	}

	internal void AdjustRunsVerticalOffset(int dcpLimit, int height, int baselineOffset, out int cellHeight, out int cellAscent)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		ArrayList arrayList = new ArrayList(3);
		int i = 0;
		int num8 = 0;
		Span span;
		for (; i < dcpLimit; i += span.length)
		{
			span = _plsrunVector[num8++];
			LSRun run = GetRun((Plsrun)span.element);
			if (run.Type == Plsrun.Text || run.Type == Plsrun.InlineObject)
			{
				if (run.RunProp.BaselineAlignment == BaselineAlignment.Baseline)
				{
					num3 = Math.Max(num3, run.BaselineOffset);
					num4 = Math.Max(num4, run.Descent);
				}
				arrayList.Add(run);
			}
		}
		num3 = -num3;
		num = ((height > 0) ? (-baselineOffset) : num3);
		foreach (LSRun item in arrayList)
		{
			switch (item.RunProp.BaselineAlignment)
			{
			case BaselineAlignment.Top:
				num4 = Math.Max(num4, item.Height + num);
				break;
			case BaselineAlignment.TextTop:
				num4 = Math.Max(num4, item.Height + num3);
				break;
			}
		}
		num2 = ((height > 0) ? (height - baselineOffset) : num4);
		num7 = (num + num2) / 2;
		num6 = num2 / 2;
		num5 = num * 2 / 3;
		cellAscent = 0;
		int num9 = 0;
		foreach (LSRun item2 in arrayList)
		{
			int num10 = 0;
			switch (item2.RunProp.BaselineAlignment)
			{
			case BaselineAlignment.Top:
				num10 = num + item2.BaselineOffset;
				break;
			case BaselineAlignment.Bottom:
				num10 = num2 - item2.Height + item2.BaselineOffset;
				break;
			case BaselineAlignment.TextTop:
				num10 = num3 + item2.BaselineOffset;
				break;
			case BaselineAlignment.TextBottom:
				num10 = num4 - item2.Height + item2.BaselineOffset;
				break;
			case BaselineAlignment.Center:
				num10 = num7 - item2.Height / 2 + item2.BaselineOffset;
				break;
			case BaselineAlignment.Superscript:
				num10 = num5;
				break;
			case BaselineAlignment.Subscript:
				num10 = num6;
				break;
			}
			item2.Move(num10);
			cellAscent = Math.Max(cellAscent, item2.BaselineOffset - num10);
			num9 = Math.Max(num9, item2.Descent + num10);
		}
		cellHeight = cellAscent + num9;
	}

	internal char[] CollectRawWord(int lscpCurrent, bool isCurrentAtWordStart, bool isSideways, out int lscpChunk, out int lscchChunk, out CultureInfo textCulture, out int cchWordMax, out SpanVector<int> textVector)
	{
		textVector = default(SpanVector<int>);
		textCulture = null;
		lscpChunk = lscpCurrent;
		lscchChunk = 0;
		cchWordMax = 0;
		LSRun lSRun = FetchLSRun(lscpCurrent, Settings.Formatter.TextFormattingMode, isSideways, out var plsrun, out var lsrunOffset, out var lsrunLength);
		if (lSRun == null)
		{
			return null;
		}
		textCulture = lSRun.TextCulture;
		int num = 0;
		if (!isCurrentAtWordStart && lscpChunk > _cpFirst)
		{
			SpanRider spanRider = new SpanRider(_plsrunVector, _plsrunVectorLatestPosition);
			do
			{
				spanRider.At(lscpChunk - _cpFirst - 1);
				int num2 = spanRider.CurrentSpanStart + _cpFirst;
				lSRun = GetRun((Plsrun)spanRider.CurrentElement);
				if (IsNewline(lSRun.Type) || lSRun.Type == Plsrun.InlineObject)
				{
					break;
				}
				if (lSRun.Type == Plsrun.Text)
				{
					if (!lSRun.TextCulture.Equals(textCulture))
					{
						break;
					}
					int num3 = lscpChunk - num2;
					int num4 = lSRun.OffsetToFirstChar + lscpChunk - _cpFirst - spanRider.CurrentSpanStart;
					int i;
					for (i = 0; i < num3 && !IsSpace(lSRun.CharacterBuffer[num4 - i - 1]); i++)
					{
					}
					num += i;
					if (i < num3)
					{
						lscpChunk -= i;
						break;
					}
				}
				Invariant.Assert(num2 < lscpChunk);
				lscpChunk = num2;
			}
			while (lscpChunk > _cpFirst && num <= 80);
			_plsrunVectorLatestPosition = spanRider.SpanPosition;
		}
		if (num > 80)
		{
			return null;
		}
		StringBuilder stringBuilder = null;
		int num5 = lscpChunk;
		int num6 = 0;
		int num7 = 0;
		do
		{
			lSRun = FetchLSRun(num5, Settings.Formatter.TextFormattingMode, isSideways, out plsrun, out lsrunOffset, out lsrunLength);
			if (lSRun == null)
			{
				return null;
			}
			int num2 = num5 + lsrunLength;
			if (IsNewline(lSRun.Type) || lSRun.Type == Plsrun.InlineObject)
			{
				break;
			}
			if (lSRun.Type == Plsrun.Text)
			{
				if (!lSRun.TextCulture.Equals(textCulture))
				{
					break;
				}
				int num8 = num2 - num5;
				int num9 = lSRun.OffsetToFirstChar + lSRun.Length - lsrunLength;
				int j = 0;
				if (num6 == 0)
				{
					for (; j < num8 && IsSpace(lSRun.CharacterBuffer[num9 + j]); j++)
					{
					}
				}
				int num10 = num7;
				char ch;
				while (j < num8 && num6 + j < 80 && !IsSpace(ch = lSRun.CharacterBuffer[num9 + j]))
				{
					j++;
					if (IsStrong(ch))
					{
						num10++;
						continue;
					}
					if (num10 > cchWordMax)
					{
						cchWordMax = num10;
					}
					num10 = 0;
				}
				num7 = num10;
				if (num7 > cchWordMax)
				{
					cchWordMax = num7;
				}
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder();
				}
				lSRun.CharacterBuffer.AppendToStringBuilder(stringBuilder, num9, j);
				textVector.Set(num6, j, num5 - lscpChunk);
				num6 += j;
				if (j < num8)
				{
					num5 += j;
					break;
				}
			}
			Invariant.Assert(num2 > num5);
			num5 = num2;
		}
		while (num6 < 80);
		if (stringBuilder == null)
		{
			return null;
		}
		lscchChunk = num5 - lscpChunk;
		Invariant.Assert(stringBuilder.Length == num6);
		char[] array = new char[stringBuilder.Length];
		stringBuilder.CopyTo(0, array, 0, array.Length);
		return array;
	}

	internal TextEmbeddedObjectMetrics FormatTextObject(TextEmbeddedObject textObject, int cpFirst, int currentPosition, int rightMargin)
	{
		if (_textObjectMetricsVector == null)
		{
			_textObjectMetricsVector = new SpanVector(null);
		}
		SpanRider spanRider = new SpanRider(_textObjectMetricsVector);
		spanRider.At(cpFirst);
		TextEmbeddedObjectMetrics textEmbeddedObjectMetrics = (TextEmbeddedObjectMetrics)spanRider.CurrentElement;
		if (textEmbeddedObjectMetrics == null)
		{
			int num = _formatWidth - currentPosition;
			if (num <= 0)
			{
				num = rightMargin - _formatWidth;
			}
			textEmbeddedObjectMetrics = textObject.Format(_settings.Formatter.IdealToReal(num, _settings.TextSource.PixelsPerDip));
			if (double.IsPositiveInfinity(textEmbeddedObjectMetrics.Width))
			{
				textEmbeddedObjectMetrics = new TextEmbeddedObjectMetrics(_settings.Formatter.IdealToReal(1073741822 - currentPosition, _settings.TextSource.PixelsPerDip), textEmbeddedObjectMetrics.Height, textEmbeddedObjectMetrics.Baseline);
			}
			else if (textEmbeddedObjectMetrics.Width > _settings.Formatter.IdealToReal(1073741822 - currentPosition, _settings.TextSource.PixelsPerDip))
			{
				throw new ArgumentException(SR.TextObjectMetrics_WidthOutOfRange);
			}
			_textObjectMetricsVector.SetReference(cpFirst, textObject.Length, textEmbeddedObjectMetrics);
		}
		return textEmbeddedObjectMetrics;
	}
}
