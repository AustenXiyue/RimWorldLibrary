using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.Generic;

namespace MS.Internal.TextFormatting;

internal sealed class FullTextState
{
	[Flags]
	private enum StatusFlags
	{
		None = 0,
		VerticalAdjust = 1,
		ForceWrap = 2,
		KeepState = 0x40
	}

	private TextStore _store;

	private TextStore _markerStore;

	private StatusFlags _statusFlags;

	private int _cpMeasured;

	private int _lscpHyphenationLookAhead;

	private bool _isSideways;

	private const int MinCchWordToHyphenate = 7;

	internal int CpMeasured
	{
		get
		{
			return _cpMeasured;
		}
		set
		{
			_cpMeasured = value;
		}
	}

	internal int LscpHyphenationLookAhead => _lscpHyphenationLookAhead;

	internal TextFormattingMode TextFormattingMode => Formatter.TextFormattingMode;

	internal bool IsSideways => _isSideways;

	internal bool VerticalAdjust
	{
		get
		{
			return (_statusFlags & StatusFlags.VerticalAdjust) != 0;
		}
		set
		{
			if (value)
			{
				_statusFlags |= StatusFlags.VerticalAdjust;
			}
			else
			{
				_statusFlags &= ~StatusFlags.VerticalAdjust;
			}
		}
	}

	internal bool ForceWrap
	{
		get
		{
			return (_statusFlags & StatusFlags.ForceWrap) != 0;
		}
		set
		{
			if (value)
			{
				_statusFlags |= StatusFlags.ForceWrap;
			}
			else
			{
				_statusFlags &= ~StatusFlags.ForceWrap;
			}
		}
	}

	internal bool KeepState => (_statusFlags & StatusFlags.KeepState) != 0;

	internal TextStore TextStore => _store;

	internal TextStore TextMarkerStore => _markerStore;

	internal TextFormatterImp Formatter => _store.Settings.Formatter;

	internal int FormatWidth => _store.FormatWidth;

	internal static FullTextState Create(FormatSettings settings, int cpFirst, int finiteFormatWidth)
	{
		TextStore store = new TextStore(settings, cpFirst, 0, settings.GetFormatWidth(finiteFormatWidth));
		ParaProp pap = settings.Pap;
		TextStore markerStore = null;
		if (pap.FirstLineInParagraph && pap.TextMarkerProperties != null && pap.TextMarkerProperties.TextSource != null)
		{
			markerStore = new TextStore(new FormatSettings(settings.Formatter, pap.TextMarkerProperties.TextSource, new TextRunCacheImp(), pap, null, isSingleLineFormatting: true, settings.TextFormattingMode, settings.IsSideways), 0, -2147483647, 1073741822);
		}
		return new FullTextState(store, markerStore, settings.IsSideways);
	}

	private FullTextState(TextStore store, TextStore markerStore, bool isSideways)
	{
		_isSideways = isSideways;
		_store = store;
		_markerStore = markerStore;
	}

	internal unsafe void SetTabs(TextFormatterContext context)
	{
		ParaProp pap = _store.Pap;
		FormatSettings settings = _store.Settings;
		int incrementalTab = TextFormatterImp.RealToIdeal(pap.DefaultIncrementalTab);
		int num = ((pap.Tabs != null) ? pap.Tabs.Count : 0);
		if (_markerStore != null)
		{
			if (pap.Tabs != null && pap.Tabs.Count > 0)
			{
				num = pap.Tabs.Count + 1;
				LsTbd[] array = new LsTbd[num];
				array[0].ur = settings.TextIndent;
				fixed (LsTbd* ptr = &array[1])
				{
					CreateLsTbds(pap, ptr, num - 1);
					context.SetTabs(incrementalTab, ptr - 1, num);
				}
			}
			else
			{
				LsTbd lsTbd = default(LsTbd);
				lsTbd.ur = settings.TextIndent;
				context.SetTabs(incrementalTab, &lsTbd, 1);
			}
		}
		else if (pap.Tabs != null && pap.Tabs.Count > 0)
		{
			fixed (LsTbd* ptr2 = &(new LsTbd[num])[0])
			{
				CreateLsTbds(pap, ptr2, num);
				context.SetTabs(incrementalTab, ptr2, num);
			}
		}
		else
		{
			context.SetTabs(incrementalTab, null, 0);
		}
	}

	private unsafe void CreateLsTbds(ParaProp pap, LsTbd* plsTbds, int lsTbdCount)
	{
		for (int i = 0; i < lsTbdCount; i++)
		{
			TextTabProperties textTabProperties = pap.Tabs[i];
			plsTbds[i].lskt = Convert.LsKTabFromTabAlignment(textTabProperties.Alignment);
			plsTbds[i].ur = TextFormatterImp.RealToIdeal(textTabProperties.Location);
			if (textTabProperties.TabLeader != 0)
			{
				plsTbds[i].wchTabLeader = (char)textTabProperties.TabLeader;
				_statusFlags |= StatusFlags.KeepState;
			}
			plsTbds[i].wchCharTab = (char)textTabProperties.AligningCharacter;
		}
	}

	internal int GetMainTextToMarkerIdealDistance()
	{
		if (_markerStore != null)
		{
			return Math.Min(0, TextFormatterImp.RealToIdeal(_markerStore.Pap.TextMarkerProperties.Offset) - _store.Settings.TextIndent);
		}
		return 0;
	}

	internal LSRun CountText(int lscpLim, int cpFirst, out int count)
	{
		LSRun lSRun = null;
		count = 0;
		int num = lscpLim - _store.CpFirst;
		foreach (Span item in _store.PlsrunVector)
		{
			if (num <= 0)
			{
				break;
			}
			Plsrun plsrun = (Plsrun)item.element;
			if (plsrun >= Plsrun.FormatAnchor)
			{
				lSRun = _store.GetRun(plsrun);
				int length = lSRun.Length;
				if (length > 0)
				{
					if (num < item.length && length == item.length)
					{
						count += num;
						break;
					}
					count += length;
				}
			}
			num -= item.length;
		}
		count = count - cpFirst + _store.CpFirst;
		return lSRun;
	}

	internal int GetBreakpointInternalCp(int cp)
	{
		int num = cp - _store.CpFirst;
		int num2 = _store.CpFirst;
		int num3 = 0;
		SpanVector plsrunVector = _store.PlsrunVector;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		LSRun run;
		do
		{
			Span span = plsrunVector[num4];
			Plsrun plsrun = (Plsrun)span.element;
			run = _store.GetRun(plsrun);
			if (num == num3 && run.Type == Plsrun.Reverse)
			{
				break;
			}
			num5 = span.length;
			num6 = ((plsrun >= Plsrun.FormatAnchor) ? run.Length : 0);
			num2 += num5;
			num3 += num6;
		}
		while (++num4 < plsrunVector.Count && run.Type != Plsrun.ParaBreak && num >= num3);
		if (num3 == num || num5 == num6)
		{
			return num2 - num3 + num;
		}
		Invariant.Assert(num3 - num == num6);
		return num2 - num5;
	}

	internal bool FindNextHyphenBreak(int lscpCurrent, int lscchLim, bool isCurrentAtWordStart, ref int lscpHyphen, ref LsHyph lshyph)
	{
		lshyph = default(LsHyph);
		if (_store.Pap.Hyphenator != null)
		{
			int lscpChunk;
			int lscchChunk;
			LexicalChunk chunk = GetChunk(_store.Pap.Hyphenator, lscpCurrent, lscchLim, isCurrentAtWordStart, out lscpChunk, out lscchChunk);
			_lscpHyphenationLookAhead = lscpChunk + lscchChunk;
			if (!chunk.IsNoBreak)
			{
				int num = chunk.LSCPToCharacterIndex(lscpCurrent - lscpChunk);
				int num2 = chunk.LSCPToCharacterIndex(lscpCurrent + lscchLim - lscpChunk);
				if (lscchLim >= 0)
				{
					int nextBreak = chunk.Breaks.GetNextBreak(num);
					if (nextBreak >= 0 && nextBreak > num && nextBreak <= num2)
					{
						lscpHyphen = chunk.CharacterIndexToLSCP(nextBreak - 1) + lscpChunk;
						return true;
					}
				}
				else
				{
					int previousBreak = chunk.Breaks.GetPreviousBreak(num);
					if (previousBreak >= 0 && previousBreak <= num && previousBreak > num2)
					{
						lscpHyphen = chunk.CharacterIndexToLSCP(previousBreak - 1) + lscpChunk;
						return true;
					}
				}
			}
		}
		return false;
	}

	private LexicalChunk GetChunk(TextLexicalService lexicalService, int lscpCurrent, int lscchLim, bool isCurrentAtWordStart, out int lscpChunk, out int lscchChunk)
	{
		int num = lscpCurrent;
		int num2 = num + lscchLim;
		_ = _store.CpFirst;
		if (num > num2)
		{
			num = num2;
			num2 = lscpCurrent;
		}
		LexicalChunk result = default(LexicalChunk);
		CultureInfo textCulture;
		int cchWordMax;
		SpanVector<int> textVector;
		char[] array = _store.CollectRawWord(num, isCurrentAtWordStart, _isSideways, out lscpChunk, out lscchChunk, out textCulture, out cchWordMax, out textVector);
		if (array != null && cchWordMax >= 7 && num2 < lscpChunk + lscchChunk && textCulture != null && lexicalService != null && lexicalService.IsCultureSupported(textCulture))
		{
			TextLexicalBreaks textLexicalBreaks = lexicalService.AnalyzeText(array, array.Length, textCulture);
			if (textLexicalBreaks != null)
			{
				result = new LexicalChunk(textLexicalBreaks, textVector);
			}
		}
		return result;
	}

	internal TextStore StoreFrom(Plsrun plsrun)
	{
		if (!TextStore.IsMarker(plsrun))
		{
			return _store;
		}
		return _markerStore;
	}

	internal TextStore StoreFrom(int lscp)
	{
		if (lscp >= 0)
		{
			return _store;
		}
		return _markerStore;
	}
}
