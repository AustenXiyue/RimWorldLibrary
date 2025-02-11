#define TRACE
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.IO.Packaging;
using MS.Internal.Telemetry.PresentationFramework;
using MS.Internal.Utility;

namespace System.Windows.Documents;

internal class Speller
{
	private class TextMap
	{
		private readonly ITextPointer _basePosition;

		private readonly char[] _text;

		private readonly int[] _positionMap;

		private readonly int _textLength;

		private readonly int _contentStartOffset;

		private readonly int _contentEndOffset;

		internal int ContentStartOffset => _contentStartOffset;

		internal int ContentEndOffset => _contentEndOffset;

		internal char[] Text => _text;

		internal int TextLength => _textLength;

		internal TextMap(ITextPointer contextStart, ITextPointer contextEnd, ITextPointer contentStart, ITextPointer contentEnd)
		{
			Invariant.Assert(contextStart.CompareTo(contentStart) <= 0);
			Invariant.Assert(contextEnd.CompareTo(contentEnd) >= 0);
			_basePosition = contextStart.GetFrozenPointer(LogicalDirection.Backward);
			ITextPointer textPointer = contextStart.CreatePointer();
			int offsetToPosition = contextStart.GetOffsetToPosition(contextEnd);
			_text = new char[offsetToPosition];
			_positionMap = new int[offsetToPosition + 1];
			_textLength = 0;
			int num = 0;
			_contentStartOffset = 0;
			_contentEndOffset = 0;
			while (textPointer.CompareTo(contextEnd) < 0)
			{
				if (textPointer.CompareTo(contentStart) == 0)
				{
					_contentStartOffset = _textLength;
				}
				if (textPointer.CompareTo(contentEnd) == 0)
				{
					_contentEndOffset = _textLength;
				}
				switch (textPointer.GetPointerContext(LogicalDirection.Forward))
				{
				case TextPointerContext.Text:
				{
					int textRunLength = textPointer.GetTextRunLength(LogicalDirection.Forward);
					textRunLength = Math.Min(textRunLength, _text.Length - _textLength);
					textRunLength = Math.Min(textRunLength, textPointer.GetOffsetToPosition(contextEnd));
					textPointer.GetTextInRun(LogicalDirection.Forward, _text, _textLength, textRunLength);
					for (int i = _textLength; i < _textLength + textRunLength; i++)
					{
						_positionMap[i] = i + num;
					}
					int offsetToPosition2 = textPointer.GetOffsetToPosition(contentStart);
					if (offsetToPosition2 >= 0 && offsetToPosition2 <= textRunLength)
					{
						_contentStartOffset = _textLength + textPointer.GetOffsetToPosition(contentStart);
					}
					offsetToPosition2 = textPointer.GetOffsetToPosition(contentEnd);
					if (offsetToPosition2 >= 0 && offsetToPosition2 <= textRunLength)
					{
						_contentEndOffset = _textLength + textPointer.GetOffsetToPosition(contentEnd);
					}
					textPointer.MoveByOffset(textRunLength);
					_textLength += textRunLength;
					break;
				}
				case TextPointerContext.ElementStart:
				case TextPointerContext.ElementEnd:
					if (IsAdjacentToFormatElement(textPointer))
					{
						num++;
					}
					else
					{
						_text[_textLength] = ' ';
						_positionMap[_textLength] = _textLength + num;
						_textLength++;
					}
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
					break;
				case TextPointerContext.EmbeddedElement:
					_text[_textLength] = '\uf8ff';
					_positionMap[_textLength] = _textLength + num;
					_textLength++;
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
					break;
				}
			}
			if (textPointer.CompareTo(contentEnd) == 0)
			{
				_contentEndOffset = _textLength;
			}
			if (_textLength > 0)
			{
				_positionMap[_textLength] = _positionMap[_textLength - 1] + 1;
			}
			else
			{
				_positionMap[0] = 0;
			}
			Invariant.Assert(_contentStartOffset <= _contentEndOffset);
		}

		internal ITextPointer MapOffsetToPosition(int offset)
		{
			Invariant.Assert(offset >= 0 && offset <= _textLength);
			return _basePosition.CreatePointer(_positionMap[offset]);
		}

		private bool IsAdjacentToFormatElement(ITextPointer pointer)
		{
			bool result = false;
			TextPointerContext pointerContext = pointer.GetPointerContext(LogicalDirection.Forward);
			if (pointerContext == TextPointerContext.ElementStart && TextSchema.IsFormattingType(pointer.GetElementType(LogicalDirection.Forward)))
			{
				result = true;
			}
			else if (pointerContext == TextPointerContext.ElementEnd && TextSchema.IsFormattingType(pointer.ParentType))
			{
				result = true;
			}
			return result;
		}
	}

	private class ScanStatus
	{
		private readonly long _timeLimit;

		private readonly ITextPointer _startPosition;

		private ITextPointer _timeoutPosition;

		internal bool HasExceededTimeLimit => DateTime.Now.Ticks >= _timeLimit;

		internal ITextPointer TimeoutPosition
		{
			get
			{
				return _timeoutPosition;
			}
			set
			{
				_timeoutPosition = value;
			}
		}

		internal ITextPointer StartPosition => _startPosition;

		internal ScanStatus(long timeLimit, ITextPointer startPosition)
		{
			_timeLimit = timeLimit;
			_startPosition = startPosition;
		}
	}

	private class TextMapCallbackData
	{
		private readonly TextMap _textmap;

		private readonly object _data;

		internal TextMap TextMap => _textmap;

		internal object Data => _data;

		internal TextMapCallbackData(TextMap textmap, object data)
		{
			_textmap = textmap;
			_data = data;
		}
	}

	private class DictionaryInfo
	{
		private readonly object _lexicon;

		private readonly Uri _pathUri;

		internal Uri PathUri => _pathUri;

		internal object Lexicon => _lexicon;

		internal DictionaryInfo(Uri pathUri, object lexicon)
		{
			_pathUri = pathUri;
			_lexicon = lexicon;
		}
	}

	private class TextMapOffsetErrorLogger
	{
		public enum CalculationModes
		{
			ContentPosition,
			ContextPosition
		}

		[EventData]
		private struct DebugInfo
		{
			public string Direction { get; set; }

			public int SegmentCount { get; set; }

			public SegmentInfo[] SegmentStartsAndLengths { get; set; }

			public int PositionInSegmentList { get; set; }

			public int LeftWordBreak { get; set; }

			public int RightWordBreak { get; set; }

			public int ContentOffSet { get; set; }

			public int ContextOffset { get; set; }

			public CalculationModes CalculationMode { get; set; }

			public string TextMapText { get; set; }

			public int TextMapTextLength { get; set; }

			public int TextMapContentStartOffset { get; set; }

			public int TextMapContentEndOffset { get; set; }
		}

		[EventData]
		private struct SegmentInfo
		{
			public int Start { get; set; }

			public int Length { get; set; }
		}

		private static readonly string TextMapOffsetError = "TextMapOffsetError";

		private DebugInfo _debugInfo;

		private static readonly int UnsetValue = -2;

		public int ContextOffset
		{
			set
			{
				_debugInfo.ContextOffset = value;
				_debugInfo.CalculationMode = CalculationModes.ContextPosition;
			}
		}

		public TextMapOffsetErrorLogger(LogicalDirection direction, TextMap textMap, ArrayList segments, int positionInSegmentList, int leftWordBreak, int rightWordBreak, int contentOffset)
		{
			_debugInfo = new DebugInfo
			{
				Direction = direction.ToString(),
				SegmentCount = segments.Count,
				SegmentStartsAndLengths = new SegmentInfo[segments.Count],
				PositionInSegmentList = positionInSegmentList,
				LeftWordBreak = leftWordBreak,
				RightWordBreak = rightWordBreak,
				ContentOffSet = contentOffset,
				ContextOffset = UnsetValue,
				CalculationMode = CalculationModes.ContentPosition,
				TextMapText = string.Join(string.Empty, textMap.Text),
				TextMapTextLength = textMap.TextLength,
				TextMapContentStartOffset = textMap.ContentStartOffset,
				TextMapContentEndOffset = textMap.ContentEndOffset
			};
			for (int i = 0; i < segments.Count; i++)
			{
				if (segments[i] is SpellerInteropBase.ITextRange textRange)
				{
					_debugInfo.SegmentStartsAndLengths[i] = new SegmentInfo
					{
						Start = textRange.Start,
						Length = textRange.Length
					};
				}
			}
		}

		public void LogDebugInfo()
		{
			int num = ((_debugInfo.CalculationMode == CalculationModes.ContentPosition) ? _debugInfo.ContentOffSet : _debugInfo.ContextOffset);
			if (num < 0 || num > _debugInfo.TextMapTextLength)
			{
				EventSource provider = TraceLoggingProvider.GetProvider();
				EventSourceOptions eventSourceOptions = new EventSourceOptions
				{
					Keywords = (EventKeywords)70368744177664L,
					Tags = (EventTags)33554432
				};
				provider?.Write<DebugInfo>(options: eventSourceOptions, eventName: TextMapOffsetError, data: _debugInfo);
			}
		}
	}

	private const int MaxIdleTimeSliceMs = 20;

	private const long MaxIdleTimeSliceNs = 200000L;

	private const int MaxScanBlockSize = 64;

	private const int ContextBlockSize = 32;

	private const int MinWordBreaksForContext = 4;

	private TextEditor _textEditor;

	private SpellerStatusTable _statusTable;

	private SpellerHighlightLayer _highlightLayer;

	private SpellerInteropBase _spellerInterop;

	private SpellingReform _spellingReform;

	private bool _pendingIdleCallback;

	private bool _pendingCaretMovedCallback;

	private ArrayList _ignoredWordsList;

	private readonly CultureInfo _defaultCulture;

	private bool _failedToInit;

	private Dictionary<Uri, DictionaryInfo> _uriMap;

	internal SpellerStatusTable StatusTable => _statusTable;

	private Dictionary<Uri, DictionaryInfo> UriMap
	{
		get
		{
			if (_uriMap == null)
			{
				_uriMap = new Dictionary<Uri, DictionaryInfo>();
			}
			return _uriMap;
		}
	}

	internal Speller(TextEditor textEditor)
	{
		_textEditor = textEditor;
		_textEditor.TextContainer.Change += OnTextContainerChange;
		if (_textEditor.TextContainer.SymbolCount > 0)
		{
			ScheduleIdleCallback();
		}
		_defaultCulture = ((InputLanguageManager.Current != null) ? InputLanguageManager.Current.CurrentInputLanguage : Thread.CurrentThread.CurrentCulture);
	}

	internal void Detach()
	{
		Invariant.Assert(_textEditor != null);
		_textEditor.TextContainer.Change -= OnTextContainerChange;
		if (_pendingCaretMovedCallback)
		{
			_textEditor.Selection.Changed -= OnCaretMoved;
			_textEditor.UiScope.LostFocus -= OnLostFocus;
			_pendingCaretMovedCallback = false;
		}
		if (_highlightLayer != null)
		{
			_textEditor.TextContainer.Highlights.RemoveLayer(_highlightLayer);
			_highlightLayer = null;
		}
		_statusTable = null;
		if (_spellerInterop != null)
		{
			_spellerInterop.Dispose();
			_spellerInterop = null;
		}
		_textEditor = null;
	}

	internal SpellingError GetError(ITextPointer position, LogicalDirection direction, bool forceEvaluation)
	{
		if (forceEvaluation && EnsureInitialized() && _statusTable.IsRunType(position.CreateStaticPointer(), direction, SpellerStatusTable.RunType.Dirty))
		{
			ScanPosition(position, direction);
		}
		if (_statusTable != null && _statusTable.GetError(position.CreateStaticPointer(), direction, out var start, out var end))
		{
			return new SpellingError(this, start, end);
		}
		return null;
	}

	internal ITextPointer GetNextSpellingErrorPosition(ITextPointer position, LogicalDirection direction)
	{
		if (!EnsureInitialized())
		{
			return null;
		}
		SpellerStatusTable.RunType runType;
		StaticTextPointer end;
		for (; _statusTable.GetRun(position.CreateStaticPointer(), direction, out runType, out end); position = end.CreateDynamicTextPointer(direction))
		{
			switch (runType)
			{
			case SpellerStatusTable.RunType.Dirty:
				ScanPosition(position, direction);
				_statusTable.GetRun(position.CreateStaticPointer(), direction, out runType, out end);
				Invariant.Assert(runType != SpellerStatusTable.RunType.Dirty);
				if (runType != SpellerStatusTable.RunType.Error)
				{
					continue;
				}
				break;
			default:
				continue;
			case SpellerStatusTable.RunType.Error:
				break;
			}
			break;
		}
		return GetError(position, direction, forceEvaluation: false)?.Start;
	}

	internal IList GetSuggestionsForError(SpellingError error)
	{
		ArrayList arrayList = new ArrayList(1);
		XmlLanguage language;
		CultureInfo currentCultureAndLanguage = GetCurrentCultureAndLanguage(error.Start, out language);
		if (currentCultureAndLanguage != null && _spellerInterop.CanSpellCheck(currentCultureAndLanguage))
		{
			ExpandToWordBreakAndContext(error.Start, LogicalDirection.Backward, language, out var contentPosition, out var contextPosition);
			ExpandToWordBreakAndContext(error.End, LogicalDirection.Forward, language, out var contentPosition2, out var contextPosition2);
			TextMap textMap = new TextMap(contextPosition, contextPosition2, contentPosition, contentPosition2);
			SetCulture(currentCultureAndLanguage);
			_spellerInterop.Mode = SpellerInteropBase.SpellerMode.SpellingErrorsWithSuggestions;
			_spellerInterop.EnumTextSegments(textMap.Text, textMap.TextLength, null, ScanErrorTextSegment, new TextMapCallbackData(textMap, arrayList));
		}
		return arrayList;
	}

	internal void IgnoreAll(string word)
	{
		if (_ignoredWordsList == null)
		{
			_ignoredWordsList = new ArrayList(1);
		}
		int num = _ignoredWordsList.BinarySearch(word, new CaseInsensitiveComparer(_defaultCulture));
		if (num >= 0)
		{
			return;
		}
		_ignoredWordsList.Insert(~num, word);
		if (_statusTable == null)
		{
			return;
		}
		StaticTextPointer textPosition = _textEditor.TextContainer.CreateStaticPointerAtOffset(0);
		char[] charArray = null;
		while (!textPosition.IsNull)
		{
			if (_statusTable.GetError(textPosition, LogicalDirection.Forward, out var start, out var end))
			{
				string textInternal = TextRangeBase.GetTextInternal(start, end, ref charArray);
				if (string.Compare(word, textInternal, ignoreCase: true, _defaultCulture) == 0)
				{
					_statusTable.MarkCleanRange(start, end);
				}
			}
			textPosition = _statusTable.GetNextErrorTransition(textPosition, LogicalDirection.Forward);
		}
	}

	internal void SetSpellingReform(SpellingReform spellingReform)
	{
		if (_spellingReform != spellingReform)
		{
			_spellingReform = spellingReform;
			ResetErrors();
		}
	}

	internal void SetCustomDictionaries(CustomDictionarySources dictionaryLocations, bool add)
	{
		if (!EnsureInitialized())
		{
			return;
		}
		if (add)
		{
			foreach (Uri dictionaryLocation in dictionaryLocations)
			{
				OnDictionaryUriAdded(dictionaryLocation);
			}
			return;
		}
		OnDictionaryUriCollectionCleared();
	}

	internal void ResetErrors()
	{
		if (_statusTable != null)
		{
			_statusTable.MarkDirtyRange(_textEditor.TextContainer.Start, _textEditor.TextContainer.End);
			if (_textEditor.TextContainer.SymbolCount > 0)
			{
				ScheduleIdleCallback();
			}
		}
	}

	internal static bool IsSpellerAffectingProperty(DependencyProperty property)
	{
		if (property != FrameworkElement.LanguageProperty)
		{
			return property == SpellCheck.SpellingReformProperty;
		}
		return true;
	}

	internal void OnDictionaryUriAdded(Uri uri)
	{
		if (EnsureInitialized())
		{
			if (UriMap.ContainsKey(uri))
			{
				OnDictionaryUriRemoved(uri);
			}
			if (!uri.IsAbsoluteUri || uri.IsFile)
			{
				Uri uri2 = ResolvePathUri(uri);
				object lexicon = _spellerInterop.LoadDictionary(uri2.LocalPath);
				UriMap.Add(uri, new DictionaryInfo(uri2, lexicon));
			}
			else
			{
				LoadDictionaryFromPackUri(uri);
			}
			ResetErrors();
		}
	}

	internal void OnDictionaryUriRemoved(Uri uri)
	{
		if (EnsureInitialized() && UriMap.ContainsKey(uri))
		{
			DictionaryInfo dictionaryInfo = UriMap[uri];
			try
			{
				_spellerInterop.UnloadDictionary(dictionaryInfo.Lexicon);
			}
			catch (Exception ex)
			{
				Trace.Write(string.Format(CultureInfo.InvariantCulture, "Unloading dictionary failed. Original Uri:{0}, file Uri:{1}, exception:{2}", uri.ToString(), dictionaryInfo.PathUri.ToString(), ex.ToString()));
				throw;
			}
			UriMap.Remove(uri);
			ResetErrors();
		}
	}

	internal void OnDictionaryUriCollectionCleared()
	{
		if (EnsureInitialized())
		{
			_spellerInterop.ReleaseAllLexicons();
			UriMap.Clear();
			ResetErrors();
		}
	}

	private bool EnsureInitialized()
	{
		if (_spellerInterop != null)
		{
			return true;
		}
		if (_failedToInit)
		{
			return false;
		}
		Invariant.Assert(_highlightLayer == null);
		Invariant.Assert(_statusTable == null);
		_spellerInterop = SpellerInteropBase.CreateInstance();
		_failedToInit = _spellerInterop == null;
		if (_failedToInit)
		{
			return false;
		}
		_highlightLayer = new SpellerHighlightLayer(this);
		_statusTable = new SpellerStatusTable(_textEditor.TextContainer.Start, _highlightLayer);
		_textEditor.TextContainer.Highlights.AddLayer(_highlightLayer);
		_spellingReform = (SpellingReform)_textEditor.UiScope.GetValue(SpellCheck.SpellingReformProperty);
		return true;
	}

	private void ScheduleIdleCallback()
	{
		if (!_pendingIdleCallback)
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new DispatcherOperationCallback(OnIdle), null);
			_pendingIdleCallback = true;
		}
	}

	private void ScheduleCaretMovedCallback()
	{
		if (!_pendingCaretMovedCallback)
		{
			_textEditor.Selection.Changed += OnCaretMoved;
			_textEditor.UiScope.LostFocus += OnLostFocus;
			_pendingCaretMovedCallback = true;
		}
	}

	private void OnTextContainerChange(object sender, TextContainerChangeEventArgs e)
	{
		Invariant.Assert(sender == _textEditor.TextContainer);
		if (e.Count != 0 && (e.TextChange != TextChangeType.PropertyModified || IsSpellerAffectingProperty(e.Property)) && !_failedToInit)
		{
			if (_statusTable != null)
			{
				_statusTable.OnTextChange(e);
			}
			ScheduleIdleCallback();
		}
	}

	private object OnIdle(object unused)
	{
		Invariant.Assert(_pendingIdleCallback);
		_pendingIdleCallback = false;
		if (_textEditor != null && EnsureInitialized())
		{
			long timeLimit = DateTime.Now.Ticks + 200000;
			ITextPointer end = null;
			ScanStatus scanStatus = null;
			ITextPointer start;
			while (GetNextScanRange(end, out start, out end))
			{
				scanStatus = ScanRange(start, end, timeLimit);
				if (scanStatus.HasExceededTimeLimit)
				{
					break;
				}
			}
			if (scanStatus != null && scanStatus.HasExceededTimeLimit)
			{
				ScheduleIdleCallback();
			}
		}
		return null;
	}

	private void OnCaretMoved(object sender, EventArgs e)
	{
		OnCaretMovedWorker();
	}

	private void OnLostFocus(object sender, RoutedEventArgs e)
	{
		OnCaretMovedWorker();
	}

	private void OnCaretMovedWorker()
	{
		if (_pendingCaretMovedCallback && _textEditor != null)
		{
			_textEditor.Selection.Changed -= OnCaretMoved;
			_textEditor.UiScope.LostFocus -= OnLostFocus;
			_pendingCaretMovedCallback = false;
			ScheduleIdleCallback();
		}
	}

	private bool GetNextScanRange(ITextPointer searchStart, out ITextPointer start, out ITextPointer end)
	{
		start = null;
		end = null;
		if (searchStart == null)
		{
			searchStart = _textEditor.TextContainer.Start;
		}
		GetNextScanRangeRaw(searchStart, out var start2, out var end2);
		if (start2 != null)
		{
			AdjustScanRangeAroundComposition(start2, end2, out start, out end);
		}
		return start != null;
	}

	private void GetNextScanRangeRaw(ITextPointer searchStart, out ITextPointer start, out ITextPointer end)
	{
		Invariant.Assert(searchStart != null);
		start = null;
		end = null;
		_statusTable.GetFirstDirtyRange(searchStart, out start, out end);
		if (start != null)
		{
			Invariant.Assert(start.CompareTo(end) < 0);
			if (start.GetOffsetToPosition(end) > 64)
			{
				end = start.CreatePointer(64);
			}
			XmlLanguage currentLanguage = GetCurrentLanguage(start);
			end = GetNextLanguageTransition(start, LogicalDirection.Forward, currentLanguage, end);
			Invariant.Assert(start.CompareTo(end) < 0);
		}
	}

	private void AdjustScanRangeAroundComposition(ITextPointer rawStart, ITextPointer rawEnd, out ITextPointer start, out ITextPointer end)
	{
		start = rawStart;
		end = rawEnd;
		if (!_textEditor.Selection.IsEmpty || !_textEditor.UiScope.IsKeyboardFocused)
		{
			return;
		}
		ITextPointer start2 = _textEditor.Selection.Start;
		_spellerInterop.Mode = SpellerInteropBase.SpellerMode.WordBreaking;
		XmlLanguage currentLanguage = GetCurrentLanguage(start2);
		ITextPointer textPointer = SearchForWordBreaks(start2, LogicalDirection.Backward, currentLanguage, 1, stopOnError: false);
		ITextPointer textPointer2 = SearchForWordBreaks(start2, LogicalDirection.Forward, currentLanguage, 1, stopOnError: false);
		TextMap textMap = new TextMap(textPointer, textPointer2, start2, start2);
		ArrayList arrayList = new ArrayList(2);
		_spellerInterop.EnumTextSegments(textMap.Text, textMap.TextLength, null, ExpandToWordBreakCallback, arrayList);
		if (arrayList.Count != 0)
		{
			FindPositionInSegmentList(textMap, LogicalDirection.Backward, arrayList, out var leftWordBreak, out var rightWordBreak);
			textPointer = textMap.MapOffsetToPosition(leftWordBreak);
			textPointer2 = textMap.MapOffsetToPosition(rightWordBreak);
		}
		if (textPointer.CompareTo(rawEnd) < 0 && textPointer2.CompareTo(rawStart) > 0)
		{
			if (textPointer.CompareTo(rawStart) > 0)
			{
				end = textPointer;
			}
			else if (textPointer2.CompareTo(rawEnd) < 0)
			{
				start = textPointer2;
			}
			else
			{
				GetNextScanRangeRaw(textPointer2, out start, out end);
			}
			ScheduleCaretMovedCallback();
		}
	}

	private ScanStatus ScanRange(ITextPointer start, ITextPointer end, long timeLimit)
	{
		ScanStatus scanStatus = new ScanStatus(timeLimit, start);
		XmlLanguage language;
		CultureInfo currentCultureAndLanguage = GetCurrentCultureAndLanguage(start, out language);
		if (currentCultureAndLanguage == null)
		{
			_statusTable.MarkCleanRange(start, end);
		}
		else
		{
			SetCulture(currentCultureAndLanguage);
			ExpandToWordBreakAndContext(start, LogicalDirection.Backward, language, out var contentPosition, out var contextPosition);
			ExpandToWordBreakAndContext(end, LogicalDirection.Forward, language, out var contentPosition2, out var contextPosition2);
			Invariant.Assert(contentPosition.CompareTo(contentPosition2) < 0);
			Invariant.Assert(contextPosition.CompareTo(contextPosition2) < 0);
			Invariant.Assert(contentPosition.CompareTo(contextPosition) >= 0);
			Invariant.Assert(contentPosition2.CompareTo(contextPosition2) <= 0);
			_statusTable.MarkCleanRange(contentPosition, contentPosition2);
			if (_spellerInterop.CanSpellCheck(currentCultureAndLanguage))
			{
				_spellerInterop.Mode = SpellerInteropBase.SpellerMode.SpellingErrors;
				TextMap textMap = new TextMap(contextPosition, contextPosition2, contentPosition, contentPosition2);
				_spellerInterop.EnumTextSegments(textMap.Text, textMap.TextLength, ScanRangeCheckTimeLimitCallback, ScanTextSegment, new TextMapCallbackData(textMap, scanStatus));
				if (scanStatus.TimeoutPosition != null)
				{
					if (scanStatus.TimeoutPosition.CompareTo(end) < 0)
					{
						_statusTable.MarkDirtyRange(scanStatus.TimeoutPosition, end);
						if (scanStatus.TimeoutPosition.CompareTo(start) <= 0)
						{
							string text = "Speller is not advancing! \nCulture = " + currentCultureAndLanguage?.ToString() + "\nStart offset = " + start.Offset + " parent = " + start.ParentType.Name + "\nContextStart offset = " + contextPosition.Offset + " parent = " + contextPosition.ParentType.Name + "\nContentStart offset = " + contentPosition.Offset + " parent = " + contentPosition.ParentType.Name + "\nContentEnd offset = " + contentPosition2.Offset + " parent = " + contentPosition2.ParentType.Name + "\nContextEnd offset = " + contextPosition2.Offset + " parent = " + contextPosition2.ParentType.Name + "\nTimeout offset = " + scanStatus.TimeoutPosition.Offset + " parent = " + scanStatus.TimeoutPosition.ParentType.Name + "\ntextMap TextLength = " + textMap.TextLength + " text = " + new string(textMap.Text) + "\nDocument = " + start.TextContainer.Parent.GetType().Name + "\n";
							if (start is TextPointer)
							{
								text = text + "Xml = " + new TextRange((TextPointer)start.TextContainer.Start, (TextPointer)start.TextContainer.End).Xml;
							}
							Invariant.Assert(condition: false, text);
						}
					}
					else
					{
						Invariant.Assert(scanStatus.TimeoutPosition.CompareTo(contentPosition2) <= 0);
					}
				}
			}
		}
		return scanStatus;
	}

	private bool ScanErrorTextSegment(SpellerInteropBase.ISpellerSegment textSegment, object o)
	{
		TextMapCallbackData textMapCallbackData = (TextMapCallbackData)o;
		SpellerInteropBase.ITextRange textRange = textSegment.TextRange;
		if (textRange.Start + textRange.Length <= textMapCallbackData.TextMap.ContentStartOffset)
		{
			return true;
		}
		if (textRange.Start >= textMapCallbackData.TextMap.ContentEndOffset)
		{
			return false;
		}
		if (textRange.Length > 1)
		{
			if (textSegment.SubSegments.Count == 0)
			{
				ArrayList arrayList = (ArrayList)textMapCallbackData.Data;
				if (textSegment.Suggestions.Count > 0)
				{
					foreach (string suggestion in textSegment.Suggestions)
					{
						arrayList.Add(suggestion);
					}
				}
			}
			else
			{
				textSegment.EnumSubSegments(ScanErrorTextSegment, textMapCallbackData);
			}
		}
		return false;
	}

	private bool ScanTextSegment(SpellerInteropBase.ISpellerSegment textSegment, object o)
	{
		TextMapCallbackData textMapCallbackData = (TextMapCallbackData)o;
		SpellerInteropBase.ITextRange textRange = textSegment.TextRange;
		if (textRange.Start + textRange.Length <= textMapCallbackData.TextMap.ContentStartOffset)
		{
			return true;
		}
		if (textRange.Start >= textMapCallbackData.TextMap.ContentEndOffset)
		{
			return false;
		}
		if (textRange.Length > 1)
		{
			char[] array = new char[textRange.Length];
			Array.Copy(textMapCallbackData.TextMap.Text, textRange.Start, array, 0, textRange.Length);
			if (!IsIgnoredWord(array) && !textSegment.IsClean)
			{
				if (textSegment.SubSegments.Count == 0)
				{
					MarkErrorRange(textMapCallbackData.TextMap, textRange);
				}
				else
				{
					textSegment.EnumSubSegments(ScanTextSegment, textMapCallbackData);
				}
			}
		}
		return true;
	}

	private bool ScanRangeCheckTimeLimitCallback(SpellerInteropBase.ISpellerSentence sentence, object o)
	{
		TextMapCallbackData textMapCallbackData = (TextMapCallbackData)o;
		ScanStatus scanStatus = (ScanStatus)textMapCallbackData.Data;
		if (scanStatus.HasExceededTimeLimit)
		{
			Invariant.Assert(scanStatus.TimeoutPosition == null);
			int endOffset = sentence.EndOffset;
			if (endOffset >= 0)
			{
				int num = Math.Min(textMapCallbackData.TextMap.ContentEndOffset, endOffset);
				if (num > textMapCallbackData.TextMap.ContentStartOffset)
				{
					ITextPointer textPointer = textMapCallbackData.TextMap.MapOffsetToPosition(num);
					if (textPointer.CompareTo(scanStatus.StartPosition) > 0)
					{
						scanStatus.TimeoutPosition = textPointer;
					}
				}
			}
		}
		return scanStatus.TimeoutPosition == null;
	}

	private void MarkErrorRange(TextMap textMap, SpellerInteropBase.ITextRange sTextRange)
	{
		if (sTextRange.Start + sTextRange.Length <= textMap.ContentEndOffset)
		{
			ITextPointer start = textMap.MapOffsetToPosition(sTextRange.Start);
			ITextPointer end = textMap.MapOffsetToPosition(sTextRange.Start + sTextRange.Length);
			if (sTextRange.Start < textMap.ContentStartOffset)
			{
				Invariant.Assert(sTextRange.Start + sTextRange.Length > textMap.ContentStartOffset);
				_statusTable.MarkCleanRange(start, end);
			}
			_statusTable.MarkErrorRange(start, end);
		}
	}

	private void ExpandToWordBreakAndContext(ITextPointer position, LogicalDirection direction, XmlLanguage language, out ITextPointer contentPosition, out ITextPointer contextPosition)
	{
		contentPosition = position;
		contextPosition = position;
		if (position.GetPointerContext(direction) == TextPointerContext.None)
		{
			return;
		}
		_spellerInterop.Mode = SpellerInteropBase.SpellerMode.WordBreaking;
		ITextPointer textPointer = SearchForWordBreaks(position, direction, language, 4, stopOnError: true);
		LogicalDirection direction2 = ((direction != LogicalDirection.Forward) ? LogicalDirection.Forward : LogicalDirection.Backward);
		ITextPointer textPointer2 = SearchForWordBreaks(position, direction2, language, 1, stopOnError: false);
		ITextPointer contextStart;
		ITextPointer contextEnd;
		if (direction == LogicalDirection.Backward)
		{
			contextStart = textPointer;
			contextEnd = textPointer2;
		}
		else
		{
			contextStart = textPointer2;
			contextEnd = textPointer;
		}
		TextMap textMap = new TextMap(contextStart, contextEnd, position, position);
		ArrayList arrayList = new ArrayList(5);
		_spellerInterop.EnumTextSegments(textMap.Text, textMap.TextLength, null, ExpandToWordBreakCallback, arrayList);
		if (arrayList.Count != 0)
		{
			int num = FindPositionInSegmentList(textMap, direction, arrayList, out var leftWordBreak, out var rightWordBreak);
			int num2 = ((direction != 0) ? ((textMap.ContentStartOffset == leftWordBreak) ? leftWordBreak : rightWordBreak) : ((textMap.ContentStartOffset == rightWordBreak) ? rightWordBreak : leftWordBreak));
			TextMapOffsetErrorLogger textMapOffsetErrorLogger = new TextMapOffsetErrorLogger(direction, textMap, arrayList, num, leftWordBreak, rightWordBreak, num2);
			textMapOffsetErrorLogger.LogDebugInfo();
			contentPosition = textMap.MapOffsetToPosition(num2);
			int num3;
			if (direction == LogicalDirection.Backward)
			{
				num -= 3;
				SpellerInteropBase.ITextRange textRange = (SpellerInteropBase.ITextRange)arrayList[Math.Max(num, 0)];
				num3 = Math.Min(textRange.Start, num2);
			}
			else
			{
				num += 4;
				SpellerInteropBase.ITextRange textRange = (SpellerInteropBase.ITextRange)arrayList[Math.Min(num, arrayList.Count - 1)];
				num3 = Math.Max(textRange.Start + textRange.Length, num2);
			}
			textMapOffsetErrorLogger.ContextOffset = num3;
			textMapOffsetErrorLogger.LogDebugInfo();
			contextPosition = textMap.MapOffsetToPosition(num3);
		}
		if (direction == LogicalDirection.Backward)
		{
			if (position.CompareTo(contentPosition) < 0)
			{
				contentPosition = position;
			}
			if (position.CompareTo(contextPosition) < 0)
			{
				contextPosition = position;
			}
		}
		else
		{
			if (position.CompareTo(contentPosition) > 0)
			{
				contentPosition = position;
			}
			if (position.CompareTo(contextPosition) > 0)
			{
				contextPosition = position;
			}
		}
	}

	private int FindPositionInSegmentList(TextMap textMap, LogicalDirection direction, ArrayList segments, out int leftWordBreak, out int rightWordBreak)
	{
		leftWordBreak = int.MaxValue;
		rightWordBreak = -1;
		SpellerInteropBase.ITextRange textRange = (SpellerInteropBase.ITextRange)segments[0];
		int i;
		if (textMap.ContentStartOffset < textRange.Start)
		{
			leftWordBreak = 0;
			rightWordBreak = textRange.Start;
			i = -1;
		}
		else
		{
			textRange = (SpellerInteropBase.ITextRange)segments[segments.Count - 1];
			if (textMap.ContentStartOffset > textRange.Start + textRange.Length)
			{
				leftWordBreak = textRange.Start + textRange.Length;
				rightWordBreak = textMap.TextLength;
				i = segments.Count;
			}
			else
			{
				for (i = 0; i < segments.Count; i++)
				{
					textRange = (SpellerInteropBase.ITextRange)segments[i];
					leftWordBreak = textRange.Start;
					rightWordBreak = textRange.Start + textRange.Length;
					if (leftWordBreak <= textMap.ContentStartOffset && rightWordBreak >= textMap.ContentStartOffset)
					{
						break;
					}
					if (i >= segments.Count - 1 || rightWordBreak >= textMap.ContentStartOffset)
					{
						continue;
					}
					textRange = (SpellerInteropBase.ITextRange)segments[i + 1];
					leftWordBreak = rightWordBreak;
					rightWordBreak = textRange.Start;
					if (rightWordBreak > textMap.ContentStartOffset)
					{
						if (direction == LogicalDirection.Backward)
						{
							i++;
						}
						break;
					}
				}
			}
		}
		Invariant.Assert(leftWordBreak <= textMap.ContentStartOffset && textMap.ContentStartOffset <= rightWordBreak);
		return i;
	}

	private ITextPointer SearchForWordBreaks(ITextPointer position, LogicalDirection direction, XmlLanguage language, int minWordCount, bool stopOnError)
	{
		ITextPointer textPointer = position.CreatePointer();
		ITextPointer textPointer2 = null;
		if (stopOnError)
		{
			StaticTextPointer nextErrorTransition = _statusTable.GetNextErrorTransition(position.CreateStaticPointer(), direction);
			if (!nextErrorTransition.IsNull)
			{
				textPointer2 = nextErrorTransition.CreateDynamicTextPointer(LogicalDirection.Forward);
			}
		}
		bool flag = false;
		int num;
		do
		{
			textPointer.MoveByOffset((direction == LogicalDirection.Backward) ? (-32) : 32);
			if (textPointer2 != null && ((direction == LogicalDirection.Backward && textPointer2.CompareTo(textPointer) > 0) || (direction == LogicalDirection.Forward && textPointer2.CompareTo(textPointer) < 0)))
			{
				textPointer.MoveToPosition(textPointer2);
				flag = true;
			}
			ITextPointer nextLanguageTransition = GetNextLanguageTransition(position, direction, language, textPointer);
			if ((direction == LogicalDirection.Backward && nextLanguageTransition.CompareTo(textPointer) > 0) || (direction == LogicalDirection.Forward && nextLanguageTransition.CompareTo(textPointer) < 0))
			{
				textPointer.MoveToPosition(nextLanguageTransition);
				flag = true;
			}
			ITextPointer textPointer3;
			ITextPointer textPointer4;
			if (direction == LogicalDirection.Backward)
			{
				textPointer3 = textPointer;
				textPointer4 = position;
			}
			else
			{
				textPointer3 = position;
				textPointer4 = textPointer;
			}
			TextMap textMap = new TextMap(textPointer3, textPointer4, textPointer3, textPointer4);
			num = _spellerInterop.EnumTextSegments(textMap.Text, textMap.TextLength, null, null, null);
		}
		while (!flag && num < minWordCount + 1 && textPointer.GetPointerContext(direction) != 0);
		return textPointer;
	}

	private ITextPointer GetNextLanguageTransition(ITextPointer position, LogicalDirection direction, XmlLanguage language, ITextPointer haltPosition)
	{
		ITextPointer textPointer = position.CreatePointer();
		while (((direction == LogicalDirection.Forward && textPointer.CompareTo(haltPosition) < 0) || (direction == LogicalDirection.Backward && textPointer.CompareTo(haltPosition) > 0)) && GetCurrentLanguage(textPointer) == language)
		{
			textPointer.MoveToNextContextPosition(direction);
		}
		if ((direction == LogicalDirection.Forward && textPointer.CompareTo(haltPosition) > 0) || (direction == LogicalDirection.Backward && textPointer.CompareTo(haltPosition) < 0))
		{
			textPointer.MoveToPosition(haltPosition);
		}
		return textPointer;
	}

	private bool ExpandToWordBreakCallback(SpellerInteropBase.ISpellerSegment textSegment, object o)
	{
		((ArrayList)o).Add(textSegment.TextRange);
		return true;
	}

	private bool IsIgnoredWord(char[] word)
	{
		bool result = false;
		if (_ignoredWordsList != null)
		{
			result = _ignoredWordsList.BinarySearch(new string(word), new CaseInsensitiveComparer(_defaultCulture)) >= 0;
		}
		return result;
	}

	private static bool CanSpellCheck(CultureInfo culture)
	{
		switch (culture.TwoLetterISOLanguageName)
		{
		case "en":
		case "de":
		case "fr":
		case "es":
			return true;
		default:
			return false;
		}
	}

	private void SetCulture(CultureInfo culture)
	{
		_spellerInterop.SetLocale(culture);
		_spellerInterop.SetReformMode(culture, _spellingReform);
	}

	private void ScanPosition(ITextPointer position, LogicalDirection direction)
	{
		ITextPointer start;
		ITextPointer end;
		if (direction == LogicalDirection.Forward)
		{
			start = position;
			end = position.CreatePointer(1);
		}
		else
		{
			start = position.CreatePointer(-1);
			end = position;
		}
		ScanRange(start, end, long.MaxValue);
	}

	private XmlLanguage GetCurrentLanguage(ITextPointer position)
	{
		GetCurrentCultureAndLanguage(position, out var language);
		return language;
	}

	private CultureInfo GetCurrentCultureAndLanguage(ITextPointer position, out XmlLanguage language)
	{
		CultureInfo cultureInfo;
		if (!_textEditor.AcceptsRichContent && _textEditor.UiScope.GetValueSource(FrameworkElement.LanguageProperty, null, out var _) == BaseValueSourceInternal.Default)
		{
			cultureInfo = _defaultCulture;
			language = XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag);
		}
		else
		{
			language = (XmlLanguage)position.GetValue(FrameworkElement.LanguageProperty);
			if (language == null)
			{
				cultureInfo = null;
			}
			else
			{
				try
				{
					cultureInfo = language.GetSpecificCulture();
				}
				catch (InvalidOperationException)
				{
					cultureInfo = null;
				}
			}
		}
		return cultureInfo;
	}

	private static Uri ResolvePathUri(Uri uri)
	{
		if (!uri.IsAbsoluteUri)
		{
			return new Uri(new Uri(Directory.GetCurrentDirectory() + "/"), uri);
		}
		return uri;
	}

	private void LoadDictionaryFromPackUri(Uri item)
	{
		Uri uri = LoadPackFile(item);
		string tempPath = Path.GetTempPath();
		try
		{
			object lexicon = _spellerInterop.LoadDictionary(uri, tempPath);
			UriMap.Add(item, new DictionaryInfo(uri, lexicon));
		}
		finally
		{
			CleanupDictionaryTempFile(uri);
		}
	}

	private void CleanupDictionaryTempFile(Uri tempLocationUri)
	{
		if (tempLocationUri != null)
		{
			try
			{
				File.Delete(tempLocationUri.LocalPath);
			}
			catch (Exception ex)
			{
				Trace.Write(string.Format(CultureInfo.InvariantCulture, "Failure to delete temporary file with custom dictionary data. file Uri:{0},exception:{1}", tempLocationUri.ToString(), ex.ToString()));
				throw;
			}
		}
	}

	private static Uri LoadPackFile(Uri uri)
	{
		Invariant.Assert(PackUriHelper.IsPackUri(uri));
		string filePath;
		using (Stream stream = WpfWebRequestHelper.CreateRequestAndGetResponseStream(MS.Internal.Utility.BindUriHelper.GetResolvedUri(BaseUriHelper.PackAppBaseUri, uri)))
		{
			using FileStream destination = FileHelper.CreateAndOpenTemporaryFile(out filePath, FileAccess.ReadWrite);
			stream.CopyTo(destination);
		}
		return new Uri(filePath);
	}
}
