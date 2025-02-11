using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Documents;
using MS.Win32;

namespace System.Windows.Documents;

internal class TextStore : MS.Win32.UnsafeNativeMethods.ITextStoreACP, MS.Win32.UnsafeNativeMethods.ITfThreadFocusSink, MS.Win32.UnsafeNativeMethods.ITfContextOwnerCompositionSink, MS.Win32.UnsafeNativeMethods.ITfTextEditSink, MS.Win32.UnsafeNativeMethods.ITfTransitoryExtensionSink, MS.Win32.UnsafeNativeMethods.ITfMouseTrackerACP
{
	private enum AttributeStyle
	{
		InputScope,
		Font_Style_Height,
		Font_FaceName,
		Font_SizePts,
		Text_ReadOnly,
		Text_Orientation,
		Text_VerticalWriting
	}

	private struct TextServicesAttribute
	{
		private Guid _guid;

		private AttributeStyle _style;

		internal Guid Guid => _guid;

		internal AttributeStyle Style => _style;

		internal TextServicesAttribute(Guid guid, AttributeStyle style)
		{
			_guid = guid;
			_style = style;
		}
	}

	private class ScopeWeakReference : WeakReference
	{
		internal bool IsValid
		{
			get
			{
				try
				{
					return IsAlive;
				}
				catch (InvalidOperationException)
				{
					return false;
				}
			}
		}

		internal TextEditor TextEditor
		{
			get
			{
				try
				{
					return (TextEditor)Target;
				}
				catch (InvalidOperationException)
				{
					return null;
				}
			}
		}

		internal ScopeWeakReference(object obj)
			: base(obj)
		{
		}
	}

	private class MouseSink : IDisposable, IComparer
	{
		private SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfRangeACP> _range;

		private SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfMouseSink> _sink;

		private int _cookie;

		private bool _locked;

		private bool _pendingDispose;

		internal bool Locked
		{
			get
			{
				return _locked;
			}
			set
			{
				_locked = value;
				if (!_locked && _pendingDispose)
				{
					Dispose();
				}
			}
		}

		internal bool PendingDispose
		{
			set
			{
				_pendingDispose = value;
			}
		}

		internal MS.Win32.UnsafeNativeMethods.ITfRangeACP Range => _range.Value;

		internal MS.Win32.UnsafeNativeMethods.ITfMouseSink Sink => _sink.Value;

		internal int Cookie => _cookie;

		internal MouseSink(MS.Win32.UnsafeNativeMethods.ITfRangeACP range, MS.Win32.UnsafeNativeMethods.ITfMouseSink sink, int cookie)
		{
			_range = new SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfRangeACP>(range);
			_sink = new SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfMouseSink>(sink);
			_cookie = cookie;
		}

		public void Dispose()
		{
			Invariant.Assert(!_locked);
			if (_range != null)
			{
				Marshal.ReleaseComObject(_range.Value);
				_range = null;
			}
			if (_sink != null)
			{
				Marshal.ReleaseComObject(_sink.Value);
				_sink = null;
			}
			_cookie = -1;
			GC.SuppressFinalize(this);
		}

		public int Compare(object x, object y)
		{
			return ((MouseSink)x)._cookie - ((MouseSink)y)._cookie;
		}
	}

	private class CompositionParentUndoUnit : TextParentUndoUnit
	{
		private readonly bool _isFirstCompositionUnit;

		private bool _isLastCompositionUnit;

		internal bool IsFirstCompositionUnit => _isFirstCompositionUnit;

		internal bool IsLastCompositionUnit
		{
			get
			{
				return _isLastCompositionUnit;
			}
			set
			{
				_isLastCompositionUnit = value;
			}
		}

		internal CompositionParentUndoUnit(ITextSelection selection, ITextPointer anchorPosition, ITextPointer movingPosition, bool isFirstCompositionUnit)
			: base(selection, anchorPosition, movingPosition)
		{
			_isFirstCompositionUnit = isFirstCompositionUnit;
		}

		private CompositionParentUndoUnit(CompositionParentUndoUnit undoUnit)
			: base(undoUnit)
		{
			_isFirstCompositionUnit = undoUnit._isFirstCompositionUnit;
			_isLastCompositionUnit = undoUnit._isLastCompositionUnit;
		}

		protected override TextParentUndoUnit CreateRedoUnit()
		{
			return new CompositionParentUndoUnit(this);
		}

		internal void MergeCompositionUnit(CompositionParentUndoUnit unit)
		{
			object[] array = unit.CopyUnits();
			Invariant.Assert(Locked);
			Locked = false;
			for (int num = array.Length - 1; num >= 0; num--)
			{
				Add((IUndoUnit)array[num]);
			}
			Locked = true;
			MergeRedoSelectionState(unit);
			_isLastCompositionUnit |= unit.IsLastCompositionUnit;
		}

		private object[] CopyUnits()
		{
			return base.Units.ToArray();
		}
	}

	private enum CompositionEventState
	{
		NotRaisingEvents,
		RaisingEvents,
		ApplyingApplicationChange
	}

	private enum CompositionStage
	{
		StartComposition = 1,
		UpdateComposition,
		EndComposition
	}

	private class CompositionEventRecord
	{
		private readonly CompositionStage _stage;

		private readonly int _startOffsetBefore;

		private readonly int _endOffsetBefore;

		private readonly string _text;

		private readonly bool _isShiftUpdate;

		internal CompositionStage Stage => _stage;

		internal int StartOffsetBefore => _startOffsetBefore;

		internal int EndOffsetBefore => _endOffsetBefore;

		internal string Text => _text;

		internal bool IsShiftUpdate => _isShiftUpdate;

		internal CompositionEventRecord(CompositionStage stage, int startOffsetBefore, int endOffsetBefore, string text)
			: this(stage, startOffsetBefore, endOffsetBefore, text, isShiftUpdate: false)
		{
		}

		internal CompositionEventRecord(CompositionStage stage, int startOffsetBefore, int endOffsetBefore, string text, bool isShiftUpdate)
		{
			_stage = stage;
			_startOffsetBefore = startOffsetBefore;
			_endOffsetBefore = endOffsetBefore;
			_text = text;
			_isShiftUpdate = isShiftUpdate;
		}
	}

	private class IMECompositionTracer
	{
		private class TraceList
		{
			private List<IMECompositionTraceRecord> _traceList = new List<IMECompositionTraceRecord>();

			private BinaryWriter _writer;

			private int _flushIndex;

			internal TraceList(string filename)
			{
				if (filename != "none")
				{
					_writer = new BinaryWriter(File.Open(filename, FileMode.Create));
					_writer.Write(1);
				}
			}

			internal void Add(IMECompositionTraceRecord record)
			{
				_traceList.Add(record);
			}

			internal void Flush(int depth)
			{
				if (_writer != null && depth <= _flushDepth)
				{
					while (_flushIndex < _traceList.Count)
					{
						_traceList[_flushIndex].Write(_writer);
						_flushIndex++;
					}
					_writer.Flush();
					if (_flushIndex > 3000)
					{
						int count = _flushIndex - 500;
						_traceList.RemoveRange(0, count);
						_flushIndex = _traceList.Count;
					}
				}
			}

			internal void FlushAndClose()
			{
				if (_writer != null)
				{
					Flush(_flushDepth);
					_writer.Close();
					_writer = null;
				}
			}

			internal void FlushAndClear()
			{
				if (_writer != null)
				{
					Flush(_flushDepth);
					_traceList.Clear();
					_flushIndex = 0;
				}
			}
		}

		private const int s_CtfFormatVersion = 1;

		private const int s_MaxTraceRecords = 3000;

		private const int s_MinTraceRecords = 500;

		private static string _targetName;

		private static bool _isEnabled;

		private static string _fileName;

		private static int _flushDepth;

		private static IMECompositionTracingInfo _nullInfo;

		private static string[] s_format;

		private Stack<IMECompositionTraceOp> _opStack = new Stack<IMECompositionTraceOp>();

		private TraceList _traceList;

		private static List<Tuple<WeakReference<FrameworkElement>, TraceList>> s_TargetToTraceListMap;

		private static int s_seqno;

		internal static bool IsEnabled => _isEnabled;

		static IMECompositionTracer()
		{
			_nullInfo = new IMECompositionTracingInfo(null, 0);
			s_format = new string[15]
			{
				"", "{0}", "{0} {1}", "{0} {1} {2}", "{0} {1} {2} {3}", "{0} {1} {2} {3} {4} ", "{0} {1} {2} {3} {4} {5}", "{0} {1} {2} {3} {4} {5} {6}", "{0} {1} {2} {3} {4} {5} {6} {7}", "{0} {1} {2} {3} {4} {5} {6} {7} {8}",
				"{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}"
			};
			s_TargetToTraceListMap = new List<Tuple<WeakReference<FrameworkElement>, TraceList>>();
			_targetName = FrameworkCompatibilityPreferences.GetIMECompositionTraceTarget();
			_flushDepth = 0;
			string iMECompositionTraceFile = FrameworkCompatibilityPreferences.GetIMECompositionTraceFile();
			if (!string.IsNullOrEmpty(iMECompositionTraceFile))
			{
				string[] array = iMECompositionTraceFile.Split(';');
				_fileName = array[0];
				if (array.Length > 1 && int.TryParse(array[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
				{
					_flushDepth = result;
				}
			}
			if (_targetName != null)
			{
				Enable();
			}
		}

		private static void Enable()
		{
			if (!IsEnabled)
			{
				_isEnabled = true;
				Application current = Application.Current;
				if (current != null)
				{
					current.Exit += OnApplicationExit;
					current.DispatcherUnhandledException += OnUnhandledException;
				}
			}
		}

		internal static bool SetTarget(object o)
		{
			FrameworkElement frameworkElement = o as FrameworkElement;
			if (frameworkElement != null || o == null)
			{
				lock (s_TargetToTraceListMap)
				{
					CloseAllTraceLists();
					if (frameworkElement != null)
					{
						Enable();
						AddToMap(frameworkElement);
						IMECompositionTracingInfo nullInfo = _nullInfo;
						int generation = nullInfo.Generation + 1;
						nullInfo.Generation = generation;
					}
				}
			}
			return frameworkElement == o;
		}

		internal static void Flush()
		{
			lock (s_TargetToTraceListMap)
			{
				int i = 0;
				for (int count = s_TargetToTraceListMap.Count; i < count; i++)
				{
					s_TargetToTraceListMap[i].Item2.Flush(-1);
				}
			}
		}

		internal static void Mark(params object[] args)
		{
			IMECompositionTraceRecord record = new IMECompositionTraceRecord(IMECompositionTraceOp.Mark, BuildDetail(args));
			lock (s_TargetToTraceListMap)
			{
				int i = 0;
				for (int count = s_TargetToTraceListMap.Count; i < count; i++)
				{
					s_TargetToTraceListMap[i].Item2.Add(record);
				}
			}
		}

		internal static void ConfigureTracing(TextStore textStore)
		{
			FrameworkElement uiScope = textStore.UiScope;
			IMECompositionTracer iMECompositionTracer = null;
			IMECompositionTracingInfo value = _nullInfo;
			IMECompositionTracingInfo iMECompositionTracingInfo = IMECompositionTracingInfoField.GetValue(uiScope);
			if (iMECompositionTracingInfo != null && iMECompositionTracingInfo.Generation < _nullInfo.Generation)
			{
				iMECompositionTracingInfo = null;
			}
			if (iMECompositionTracingInfo == null)
			{
				TraceList traceList = TraceListForUiScope(uiScope);
				if (traceList != null)
				{
					iMECompositionTracer = new IMECompositionTracer(textStore, traceList);
				}
				if (iMECompositionTracer != null)
				{
					value = new IMECompositionTracingInfo(iMECompositionTracer, _nullInfo.Generation);
				}
				IMECompositionTracingInfoField.SetValue(uiScope, value);
				textStore.IsTracing = IsTracing(textStore);
			}
		}

		internal static bool IsTracing(TextStore textStore)
		{
			IMECompositionTracingInfo value = IMECompositionTracingInfoField.GetValue(textStore.UiScope);
			if (value != null)
			{
				return value.IMECompositionTracer != null;
			}
			return false;
		}

		internal static void Trace(TextStore textStore, IMECompositionTraceOp op, params object[] args)
		{
			IMECompositionTracingInfo value = IMECompositionTracingInfoField.GetValue(textStore.UiScope);
			IMECompositionTracer iMECompositionTracer = value.IMECompositionTracer;
			if (!ShouldIgnore(op, value))
			{
				iMECompositionTracer.AddTrace(textStore, op, value, args);
			}
		}

		private static bool ShouldIgnore(IMECompositionTraceOp op, IMECompositionTracingInfo cti)
		{
			return op == IMECompositionTraceOp.NoOp;
		}

		private static string DisplayType(object o)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			bool flag2 = false;
			Type type = o.GetType();
			while (!flag2 && type != null)
			{
				if (flag)
				{
					stringBuilder.Append('/');
				}
				string text = type.ToString();
				flag2 = text.StartsWith("System.Windows.Controls.");
				if (flag2)
				{
					text = text.Substring(24);
				}
				stringBuilder.Append(text);
				flag = true;
				type = type.BaseType;
			}
			return stringBuilder.ToString();
		}

		private static string BuildDetail(object[] args)
		{
			int num = ((args != null) ? args.Length : 0);
			if (num == 0)
			{
				return string.Empty;
			}
			return string.Format(CultureInfo.InvariantCulture, s_format[num], args);
		}

		private IMECompositionTracer(TextStore textStore, TraceList traceList)
		{
			_traceList = traceList;
			IdentifyTrace(textStore);
		}

		private static void OnApplicationExit(object sender, ExitEventArgs e)
		{
			if (sender is Application application)
			{
				application.Exit -= OnApplicationExit;
			}
			CloseAllTraceLists();
		}

		private static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			if (sender is Application application)
			{
				application.DispatcherUnhandledException -= OnUnhandledException;
			}
			CloseAllTraceLists();
		}

		private void IdentifyTrace(TextStore textStore)
		{
			FrameworkElement uiScope = textStore.UiScope;
			AddTrace(textStore, IMECompositionTraceOp.ID, _nullInfo, DisplayType(uiScope));
		}

		private void AddTrace(TextStore textStore, IMECompositionTraceOp op, IMECompositionTracingInfo cti, params object[] args)
		{
			if (33 <= (int)op && _opStack.Count > 0)
			{
				_opStack.Pop();
			}
			int netCharCount = 0;
			int imeCharCount = 0;
			CompositionEventState eventState = (CompositionEventState)(-1);
			if (textStore != null)
			{
				netCharCount = textStore._netCharCount;
				imeCharCount = textStore.TextContainer.IMECharCount;
				eventState = textStore._compositionEventState;
			}
			IMECompositionTraceRecord record = new IMECompositionTraceRecord(op, _opStack.Count, netCharCount, imeCharCount, eventState, BuildDetail(args));
			_traceList.Add(record);
			if (10 <= (int)op && (int)op < 33)
			{
				_opStack.Push(op);
			}
			if (_flushDepth < 0)
			{
				_traceList.Flush(_flushDepth);
			}
			else
			{
				_traceList.Flush(_opStack.Count);
			}
		}

		private static TraceList TraceListForUiScope(FrameworkElement target)
		{
			TraceList traceList = null;
			lock (s_TargetToTraceListMap)
			{
				int i = 0;
				for (int count = s_TargetToTraceListMap.Count; i < count; i++)
				{
					if (s_TargetToTraceListMap[i].Item1.TryGetTarget(out var target2) && target2 == target)
					{
						traceList = s_TargetToTraceListMap[i].Item2;
						break;
					}
				}
				if (traceList == null && target.Name == _targetName)
				{
					traceList = AddToMap(target);
				}
			}
			return traceList;
		}

		private static TraceList AddToMap(FrameworkElement target)
		{
			TraceList traceList = null;
			lock (s_TargetToTraceListMap)
			{
				PurgeMap();
				s_seqno++;
				string text = _fileName;
				if (string.IsNullOrEmpty(text) || text == "default")
				{
					text = "IMECompositionTrace.ctf";
				}
				if (text != "none" && s_seqno > 1)
				{
					int num = text.LastIndexOf(".", StringComparison.Ordinal);
					if (num < 0)
					{
						num = text.Length;
					}
					text = $"{text.AsSpan(0, num)}{s_seqno}{text.AsSpan(num)}";
				}
				traceList = new TraceList(text);
				s_TargetToTraceListMap.Add(new Tuple<WeakReference<FrameworkElement>, TraceList>(new WeakReference<FrameworkElement>(target), traceList));
				return traceList;
			}
		}

		private static void CloseAllTraceLists()
		{
			int i = 0;
			for (int count = s_TargetToTraceListMap.Count; i < count; i++)
			{
				s_TargetToTraceListMap[i].Item2.FlushAndClose();
			}
			s_TargetToTraceListMap.Clear();
		}

		private static void PurgeMap()
		{
			for (int i = 0; i < s_TargetToTraceListMap.Count; i++)
			{
				if (!s_TargetToTraceListMap[i].Item1.TryGetTarget(out var _))
				{
					s_TargetToTraceListMap[i].Item2.FlushAndClose();
					s_TargetToTraceListMap.RemoveAt(i);
					i--;
				}
			}
		}
	}

	private class IMECompositionTracingInfo
	{
		internal IMECompositionTracer IMECompositionTracer { get; private set; }

		internal int Generation { get; set; }

		internal IMECompositionTracingInfo(IMECompositionTracer tracer, int generation)
		{
			IMECompositionTracer = tracer;
			Generation = generation;
		}
	}

	private enum IMECompositionTraceOp : ushort
	{
		NoOp = 0,
		ID = 1,
		Mark = 2,
		GetStatus = 3,
		GetSelection = 4,
		SetSelection = 5,
		GetText = 6,
		GetACPFromPoint = 7,
		GetTextExt = 8,
		DeferRequest = 9,
		BRequestLock = 10,
		BSetText = 11,
		BInsertTextAtSelection = 12,
		BGrantLockHandler = 13,
		BGrantLockWorker = 14,
		BOnLockGranted = 15,
		BOnLayoutChange = 16,
		BOnSelectionChange = 17,
		BOnTextChange = 18,
		BOnTextContainerChange = 19,
		BOnStartComposition = 20,
		BOnUpdateComposition = 21,
		BOnEndComposition = 22,
		BUpdateCompositionText = 23,
		BHandleCompositionEvents = 24,
		BSetFinalDocumentState = 25,
		BUndoQuietly = 26,
		BRedoQuietly = 27,
		BRaiseCompositionEvents = 28,
		BSelectionChangeEvent = 29,
		BStartCompositionEvent = 30,
		BUpdateCompositionEvent = 31,
		BCompleteCompositionEvent = 32,
		ERequestLock = 33,
		ESetText = 34,
		EInsertTextAtSelection = 35,
		EGrantLockHandler = 36,
		EGrantLockWorker = 37,
		EOnLockGranted = 38,
		EOnLayoutChange = 39,
		EOnSelectionChange = 40,
		EOnTextChange = 41,
		EOnTextContainerChange = 42,
		EOnStartComposition = 43,
		EOnUpdateComposition = 44,
		EOnEndComposition = 45,
		EUpdateCompositionText = 46,
		EHandleCompositionEvents = 47,
		ESetFinalDocumentState = 48,
		EUndoQuietly = 49,
		ERedoQuietly = 50,
		ERaiseCompositionEvents = 51,
		ESelectionChangeEvent = 52,
		EStartCompositionEvent = 53,
		EUpdateCompositionEvent = 54,
		ECompleteCompositionEvent = 55,
		FirstBeginOp = 10,
		FirstEndOp = 33
	}

	private class IMECompositionTraceRecord
	{
		internal IMECompositionTraceOp Op { get; private set; }

		internal int OpDepth { get; private set; }

		internal int NetCharCount { get; private set; }

		internal int IMECharCount { get; private set; }

		internal CompositionEventState EventState { get; private set; }

		internal string Detail { get; set; }

		internal IMECompositionTraceRecord(IMECompositionTraceOp op, int opDepth, int netCharCount, int imeCharCount, CompositionEventState eventState, string detail)
		{
			Op = op;
			OpDepth = opDepth;
			NetCharCount = netCharCount;
			IMECharCount = imeCharCount;
			EventState = eventState;
			Detail = detail;
		}

		internal IMECompositionTraceRecord(IMECompositionTraceOp op, string detail)
			: this(op, -1, 0, 0, (CompositionEventState)(-1), detail)
		{
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3} {4} {5}", OpDepth, NetCharCount, IMECharCount, Op, EventState, Detail);
		}

		internal void Write(BinaryWriter writer)
		{
			writer.Write((ushort)Op);
			writer.Write(OpDepth);
			writer.Write(NetCharCount);
			writer.Write(IMECharCount);
			writer.Write((byte)EventState);
			writer.Write(Detail);
		}
	}

	private readonly ScopeWeakReference _weakTextEditor;

	private TextServicesHost _textservicesHost;

	private MS.Win32.UnsafeNativeMethods.ITextStoreACPSink _sink;

	private bool _pendingWriteReq;

	private MS.Win32.UnsafeNativeMethods.LockFlags _lockFlags;

	private MS.Win32.UnsafeNativeMethods.LockFlags _pendingAsyncLockFlags;

	private int _textChangeReentrencyCount;

	private int _replayingIMEChangeReentrancyCount;

	private bool _isComposing;

	private bool _isEffectivelyNotComposing;

	private int _previousCompositionStartOffset = -1;

	private int _previousCompositionEndOffset = -1;

	private ITextPointer _previousCompositionStart;

	private ITextPointer _previousCompositionEnd;

	private TextServicesProperty _textservicesproperty;

	private const int _viewCookie = 0;

	private SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfDocumentMgr> _documentmanager;

	private int _threadFocusCookie;

	private int _editSinkCookie;

	private int _editCookie;

	private int _transitoryExtensionSinkCookie;

	private ArrayList _preparedattributes;

	private ArrayList _mouseSinks;

	private static readonly TextServicesAttribute[] _supportingattributes = new TextServicesAttribute[7]
	{
		new TextServicesAttribute(MS.Win32.UnsafeNativeMethods.GUID_PROP_INPUTSCOPE, AttributeStyle.InputScope),
		new TextServicesAttribute(MS.Win32.UnsafeNativeMethods.TSATTRID_Font_Style_Height, AttributeStyle.Font_Style_Height),
		new TextServicesAttribute(MS.Win32.UnsafeNativeMethods.TSATTRID_Font_FaceName, AttributeStyle.Font_FaceName),
		new TextServicesAttribute(MS.Win32.UnsafeNativeMethods.TSATTRID_Font_SizePts, AttributeStyle.Font_SizePts),
		new TextServicesAttribute(MS.Win32.UnsafeNativeMethods.TSATTRID_Text_ReadOnly, AttributeStyle.Text_ReadOnly),
		new TextServicesAttribute(MS.Win32.UnsafeNativeMethods.TSATTRID_Text_Orientation, AttributeStyle.Text_Orientation),
		new TextServicesAttribute(MS.Win32.UnsafeNativeMethods.TSATTRID_Text_VerticalWriting, AttributeStyle.Text_VerticalWriting)
	};

	private bool _interimSelection;

	private bool _ignoreNextSelectionChange;

	private int _netCharCount;

	private bool _makeLayoutChangeOnGotFocus;

	private CompositionEventState _compositionEventState;

	private bool _compositionModifiedByEventListener;

	private List<CompositionEventRecord> _compositionEventList;

	private bool _nextUndoUnitIsFirstCompositionUnit = true;

	private string _lastCompositionText;

	private bool _handledByTextStoreListener;

	private bool _isInUpdateLayout;

	private bool _hasTextChangedInUpdateLayout;

	private bool _isTracing;

	private static readonly UncommonField<IMECompositionTracingInfo> IMECompositionTracingInfoField = new UncommonField<IMECompositionTracingInfo>();

	internal UIElement RenderScope
	{
		get
		{
			if (TextEditor == null)
			{
				return null;
			}
			if (TextEditor.TextView == null)
			{
				return null;
			}
			return TextEditor.TextView.RenderScope;
		}
	}

	internal FrameworkElement UiScope => TextEditor.UiScope;

	internal ITextContainer TextContainer => TextEditor.TextContainer;

	internal ITextView TextView => TextEditor.TextView;

	internal MS.Win32.UnsafeNativeMethods.ITfDocumentMgr DocumentManager
	{
		get
		{
			if (_documentmanager == null)
			{
				return null;
			}
			return _documentmanager.Value;
		}
		set
		{
			_documentmanager = new SecurityCriticalDataClass<MS.Win32.UnsafeNativeMethods.ITfDocumentMgr>(value);
		}
	}

	internal int ThreadFocusCookie
	{
		get
		{
			return _threadFocusCookie;
		}
		set
		{
			_threadFocusCookie = value;
		}
	}

	internal int EditSinkCookie
	{
		get
		{
			return _editSinkCookie;
		}
		set
		{
			_editSinkCookie = value;
		}
	}

	internal int EditCookie
	{
		get
		{
			return _editCookie;
		}
		set
		{
			_editCookie = value;
		}
	}

	internal bool IsInterimSelection => _interimSelection;

	internal bool IsComposing => _isComposing;

	internal bool IsEffectivelyComposing
	{
		get
		{
			if (_isComposing)
			{
				return !_isEffectivelyNotComposing;
			}
			return false;
		}
	}

	internal int TransitoryExtensionSinkCookie
	{
		get
		{
			return _transitoryExtensionSinkCookie;
		}
		set
		{
			_transitoryExtensionSinkCookie = value;
		}
	}

	internal nint CriticalSourceWnd
	{
		get
		{
			bool callerIsTrusted = true;
			return GetSourceWnd(callerIsTrusted);
		}
	}

	private bool HasSink => _sink != null;

	private bool IsTextEditorValid => _weakTextEditor.IsValid;

	private TextEditor TextEditor => _weakTextEditor.TextEditor;

	private ITextSelection TextSelection => TextEditor.Selection;

	private bool IsReadOnly
	{
		get
		{
			if (!(bool)UiScope.GetValue(TextEditor.IsReadOnlyProperty))
			{
				return TextEditor.IsReadOnly;
			}
			return true;
		}
	}

	private List<CompositionEventRecord> CompositionEventList
	{
		get
		{
			if (_compositionEventList == null)
			{
				_compositionEventList = new List<CompositionEventRecord>();
			}
			return _compositionEventList;
		}
	}

	private bool IsTracing
	{
		get
		{
			return _isTracing;
		}
		set
		{
			_isTracing = value;
		}
	}

	internal TextStore(TextEditor textEditor)
	{
		_weakTextEditor = new ScopeWeakReference(textEditor);
		_threadFocusCookie = -1;
		_editSinkCookie = -1;
		_editCookie = -1;
		_transitoryExtensionSinkCookie = -1;
	}

	public void AdviseSink(ref Guid riid, object obj, MS.Win32.UnsafeNativeMethods.AdviseFlags flags)
	{
		if (riid != MS.Win32.UnsafeNativeMethods.IID_ITextStoreACPSink)
		{
			throw new COMException(SR.TextStore_CONNECT_E_CANNOTCONNECT, -2147220990);
		}
		if (!(obj is MS.Win32.UnsafeNativeMethods.ITextStoreACPSink sink))
		{
			throw new COMException(SR.TextStore_E_NOINTERFACE, -2147467262);
		}
		if (HasSink)
		{
			Marshal.ReleaseComObject(_sink);
		}
		else
		{
			_textservicesHost.RegisterWinEventSink(this);
		}
		_sink = sink;
	}

	public void UnadviseSink(object obj)
	{
		if (obj != _sink)
		{
			throw new COMException(SR.TextStore_CONNECT_E_NOCONNECTION, -2147220992);
		}
		Marshal.ReleaseComObject(_sink);
		_sink = null;
		_textservicesHost.UnregisterWinEventSink(this);
	}

	public void RequestLock(MS.Win32.UnsafeNativeMethods.LockFlags flags, out int hrSession)
	{
		if (!HasSink)
		{
			throw new COMException(SR.TextStore_NoSink);
		}
		if (flags == (MS.Win32.UnsafeNativeMethods.LockFlags)0)
		{
			throw new COMException(SR.TextStore_BadLockFlags);
		}
		bool flag = _replayingIMEChangeReentrancyCount > 0;
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BRequestLock, "f:", flags, "lf:", _lockFlags, "icr:", _replayingIMEChangeReentrancyCount, "ric:", flag, "tcr:", _textChangeReentrencyCount, "paf:", _pendingAsyncLockFlags);
			if (_replayingIMEChangeReentrancyCount != 0)
			{
				IMECompositionTracer.Mark(new StackTrace(fNeedFileInfo: true));
			}
		}
		if (_lockFlags != 0)
		{
			if ((_lockFlags & MS.Win32.UnsafeNativeMethods.LockFlags.TS_LF_WRITE) == MS.Win32.UnsafeNativeMethods.LockFlags.TS_LF_WRITE || (flags & MS.Win32.UnsafeNativeMethods.LockFlags.TS_LF_WRITE) == 0 || (flags & MS.Win32.UnsafeNativeMethods.LockFlags.TS_LF_SYNC) == MS.Win32.UnsafeNativeMethods.LockFlags.TS_LF_SYNC)
			{
				throw new COMException(SR.TextStore_ReentrantRequestLock);
			}
			if (!flag)
			{
				_pendingWriteReq = true;
				hrSession = 262912;
			}
			else
			{
				hrSession = DeferLockRequest(flags);
			}
		}
		else if (flag)
		{
			hrSession = DeferLockRequest(flags);
		}
		else if (_textChangeReentrencyCount != 0)
		{
			hrSession = DeferLockRequest(flags);
		}
		else
		{
			hrSession = GrantLockWorker(flags);
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.ERequestLock, "hr:", hrSession.ToString("X"), "lf:", _lockFlags, "icr:", _replayingIMEChangeReentrancyCount, "tcr:", _textChangeReentrencyCount, "paf:", _pendingAsyncLockFlags);
		}
	}

	private int DeferLockRequest(MS.Win32.UnsafeNativeMethods.LockFlags flags)
	{
		int result;
		if ((flags & MS.Win32.UnsafeNativeMethods.LockFlags.TS_LF_SYNC) == 0)
		{
			if (_pendingAsyncLockFlags == (MS.Win32.UnsafeNativeMethods.LockFlags)0)
			{
				_pendingAsyncLockFlags = flags;
				Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(GrantLockHandler), null);
			}
			else if ((flags & MS.Win32.UnsafeNativeMethods.LockFlags.TS_LF_READWRITE & _pendingAsyncLockFlags) != (flags & MS.Win32.UnsafeNativeMethods.LockFlags.TS_LF_READWRITE))
			{
				_pendingAsyncLockFlags = flags;
			}
			result = 262912;
		}
		else
		{
			result = -2147220984;
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.DeferRequest, "f:", flags, "paf:", _pendingAsyncLockFlags, "hr:", result.ToString("X"));
		}
		return result;
	}

	public void GetStatus(out MS.Win32.UnsafeNativeMethods.TS_STATUS status)
	{
		if (IsTextEditorValid && IsReadOnly)
		{
			status.dynamicFlags = MS.Win32.UnsafeNativeMethods.DynamicStatusFlags.TS_SD_READONLY;
		}
		else
		{
			status.dynamicFlags = (MS.Win32.UnsafeNativeMethods.DynamicStatusFlags)0;
		}
		status.staticFlags = MS.Win32.UnsafeNativeMethods.StaticStatusFlags.TS_SS_REGIONS;
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.GetStatus, status.dynamicFlags);
		}
	}

	public void QueryInsert(int startIndex, int endIndex, int cch, out int startResultIndex, out int endResultIndex)
	{
		startResultIndex = startIndex;
		endResultIndex = endIndex;
	}

	public void GetSelection(int index, int count, MS.Win32.UnsafeNativeMethods.TS_SELECTION_ACP[] selection, out int fetched)
	{
		fetched = 0;
		if (count > 0 && (index == 0 || index == -1))
		{
			selection[0].start = TextSelection.Start.CharOffset;
			selection[0].end = TextSelection.End.CharOffset;
			selection[0].style.ase = ((TextSelection.MovingPosition.CompareTo(TextSelection.Start) == 0) ? MS.Win32.UnsafeNativeMethods.TsActiveSelEnd.TS_AE_START : MS.Win32.UnsafeNativeMethods.TsActiveSelEnd.TS_AE_END);
			selection[0].style.interimChar = _interimSelection;
			fetched = 1;
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.GetSelection, selection[0].start, selection[0].end, selection[0].style.ase, selection[0].style.interimChar);
			}
		}
	}

	public void SetSelection(int count, MS.Win32.UnsafeNativeMethods.TS_SELECTION_ACP[] selection)
	{
		if (count == 1)
		{
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.SetSelection, selection[0].start, selection[0].end, selection[0].style.ase, selection[0].style.interimChar);
			}
			GetNormalizedRange(selection[0].start, selection[0].end, out var start, out var end);
			if (selection[0].start == selection[0].end)
			{
				TextSelection.SetCaretToPosition(start, LogicalDirection.Backward, allowStopAtLineEnd: true, allowStopNearSpace: true);
			}
			else if (selection[0].style.ase == MS.Win32.UnsafeNativeMethods.TsActiveSelEnd.TS_AE_START)
			{
				TextSelection.Select(end, start);
			}
			else
			{
				TextSelection.Select(start, end);
			}
			bool interimSelection = _interimSelection;
			_interimSelection = selection[0].style.interimChar;
			if (interimSelection != _interimSelection)
			{
				TextSelection.OnInterimSelectionChanged(_interimSelection);
			}
		}
	}

	public void GetText(int startIndex, int endIndex, char[] text, int cchReq, out int charsCopied, MS.Win32.UnsafeNativeMethods.TS_RUNINFO[] runInfo, int cRunInfoReq, out int cRunInfoRcv, out int nextIndex)
	{
		charsCopied = 0;
		cRunInfoRcv = 0;
		nextIndex = startIndex;
		if ((cchReq == 0 && cRunInfoReq == 0) || startIndex == endIndex)
		{
			return;
		}
		ITextPointer textPointer = CreatePointerAtCharOffset(startIndex, LogicalDirection.Forward);
		ITextPointer textPointer2 = ((endIndex >= 0) ? CreatePointerAtCharOffset(endIndex, LogicalDirection.Forward) : null);
		bool flag = false;
		while (!flag && (cchReq == 0 || cchReq > charsCopied) && (cRunInfoReq == 0 || cRunInfoReq > cRunInfoRcv))
		{
			switch (textPointer.GetPointerContext(LogicalDirection.Forward))
			{
			case TextPointerContext.Text:
				flag = WalkTextRun(textPointer, textPointer2, text, cchReq, ref charsCopied, runInfo, cRunInfoReq, ref cRunInfoRcv);
				break;
			case TextPointerContext.EmbeddedElement:
				flag = WalkObjectRun(textPointer, textPointer2, text, cchReq, ref charsCopied, runInfo, cRunInfoReq, ref cRunInfoRcv);
				break;
			case TextPointerContext.ElementStart:
			{
				Invariant.Assert(textPointer is TextPointer);
				TextElement textElement = (TextElement)((TextPointer)textPointer).GetAdjacentElement(LogicalDirection.Forward);
				if (textElement.IMELeftEdgeCharCount > 0)
				{
					Invariant.Assert(textElement.IMELeftEdgeCharCount == 1);
					flag = WalkRegionBoundary(textPointer, textPointer2, text, cchReq, ref charsCopied, runInfo, cRunInfoReq, ref cRunInfoRcv);
				}
				else
				{
					textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
					flag = textPointer2 != null && textPointer.CompareTo(textPointer2) >= 0;
				}
				break;
			}
			case TextPointerContext.ElementEnd:
				textPointer.MoveToNextContextPosition(LogicalDirection.Forward);
				flag = textPointer2 != null && textPointer.CompareTo(textPointer2) >= 0;
				break;
			case TextPointerContext.None:
				flag = true;
				break;
			default:
				Invariant.Assert(condition: false, "Bogus TextPointerContext!");
				break;
			}
		}
		nextIndex = textPointer.CharOffset;
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.GetText, "text:", startIndex, endIndex, cchReq, charsCopied, new string(text, 0, charsCopied), "runs:", cRunInfoReq, cRunInfoRcv, "next:", nextIndex);
		}
	}

	public void SetText(MS.Win32.UnsafeNativeMethods.SetTextFlags flags, int startIndex, int endIndex, char[] text, int cch, out MS.Win32.UnsafeNativeMethods.TS_TEXTCHANGE change)
	{
		if (IsReadOnly)
		{
			throw new COMException(SR.TextStore_TS_E_READONLY, -2147220983);
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BSetText, "text:", startIndex, endIndex, cch, new string(text, 0, cch));
		}
		GetNormalizedRange(startIndex, endIndex, out var start, out var end);
		while (start != null && TextPointerBase.IsBeforeFirstTable(start))
		{
			start = start.GetNextInsertionPosition(LogicalDirection.Forward);
		}
		if (start == null)
		{
			throw new COMException(SR.TextStore_CompositionRejected, -2147467259);
		}
		if (start.CompareTo(end) > 0)
		{
			end = start;
		}
		string text2 = FilterCompositionString(new string(text), start.GetOffsetToPosition(end));
		if (text2 == null)
		{
			throw new COMException(SR.TextStore_CompositionRejected, -2147467259);
		}
		CompositionParentUndoUnit textParentUndoUnit = OpenCompositionUndoUnit();
		UndoCloseAction undoCloseAction = UndoCloseAction.Rollback;
		try
		{
			ITextRange range = new TextRange(start, end, ignoreTextUnitBoundaries: true);
			TextEditor.SetText(range, text2, InputLanguageManager.Current.CurrentInputLanguage);
			change.start = startIndex;
			change.oldEnd = endIndex;
			change.newEnd = endIndex + text.Length - (endIndex - startIndex);
			ValidateChange(change);
			VerifyTextStoreConsistency();
			undoCloseAction = UndoCloseAction.Commit;
		}
		finally
		{
			CloseTextParentUndoUnit(textParentUndoUnit, undoCloseAction);
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.ESetText, "change:", change.start, change.oldEnd, change.newEnd);
		}
	}

	public void GetFormattedText(int startIndex, int endIndex, out object obj)
	{
		obj = null;
		throw new COMException(SR.TextStore_E_NOTIMPL, -2147467263);
	}

	public void GetEmbedded(int index, ref Guid guidService, ref Guid riid, out object obj)
	{
		obj = null;
	}

	public void QueryInsertEmbedded(ref Guid guidService, nint formatEtc, out bool insertable)
	{
		insertable = false;
	}

	public void InsertEmbedded(MS.Win32.UnsafeNativeMethods.InsertEmbeddedFlags flags, int startIndex, int endIndex, object obj, out MS.Win32.UnsafeNativeMethods.TS_TEXTCHANGE change)
	{
		if (IsReadOnly)
		{
			throw new COMException(SR.TextStore_TS_E_READONLY, -2147220983);
		}
		throw new COMException(SR.TextStore_TS_E_FORMAT, -2147220982);
	}

	public void InsertTextAtSelection(MS.Win32.UnsafeNativeMethods.InsertAtSelectionFlags flags, char[] text, int cch, out int startIndex, out int endIndex, out MS.Win32.UnsafeNativeMethods.TS_TEXTCHANGE change)
	{
		startIndex = -1;
		endIndex = -1;
		change.start = 0;
		change.oldEnd = 0;
		change.newEnd = 0;
		if (IsReadOnly)
		{
			throw new COMException(SR.TextStore_TS_E_READONLY, -2147220983);
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BInsertTextAtSelection, flags, new string(text, 0, cch));
		}
		ITextRange textRange = new TextRange(TextSelection.AnchorPosition, TextSelection.MovingPosition);
		textRange.ApplyTypingHeuristics(overType: false);
		GetAdjustedSelection(textRange.Start, textRange.End, out var startOut, out var endOut);
		ITextPointer textPointer = startOut.CreatePointer();
		textPointer.SetLogicalDirection(LogicalDirection.Backward);
		ITextPointer textPointer2 = endOut.CreatePointer();
		textPointer2.SetLogicalDirection(LogicalDirection.Forward);
		int charOffset = textPointer.CharOffset;
		int charOffset2 = textPointer2.CharOffset;
		if ((flags & MS.Win32.UnsafeNativeMethods.InsertAtSelectionFlags.TS_IAS_QUERYONLY) == 0)
		{
			CompositionParentUndoUnit textParentUndoUnit = OpenCompositionUndoUnit();
			UndoCloseAction undoCloseAction = UndoCloseAction.Rollback;
			try
			{
				VerifyTextStoreConsistency();
				change.oldEnd = charOffset2;
				string text2 = FilterCompositionString(new string(text), textRange.Start.GetOffsetToPosition(textRange.End));
				if (text2 == null)
				{
					throw new COMException(SR.TextStore_CompositionRejected, -2147467259);
				}
				TextSelection.ApplyTypingHeuristics(overType: false);
				if (startOut.CompareTo(TextSelection.Start) != 0 || endOut.CompareTo(TextSelection.End) != 0)
				{
					TextSelection.Select(startOut, endOut);
				}
				if (!_isComposing && _previousCompositionStartOffset == -1)
				{
					_previousCompositionStartOffset = TextSelection.Start.Offset;
					_previousCompositionEndOffset = TextSelection.End.Offset;
				}
				TextEditor.SetSelectedText(text2, InputLanguageManager.Current.CurrentInputLanguage);
				change.start = textPointer.CharOffset;
				change.newEnd = textPointer2.CharOffset;
				ValidateChange(change);
				VerifyTextStoreConsistency();
				undoCloseAction = UndoCloseAction.Commit;
			}
			finally
			{
				CloseTextParentUndoUnit(textParentUndoUnit, undoCloseAction);
			}
		}
		if ((flags & MS.Win32.UnsafeNativeMethods.InsertAtSelectionFlags.TS_IAS_NOQUERY) == 0)
		{
			startIndex = charOffset;
			endIndex = textPointer2.CharOffset;
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.EInsertTextAtSelection, "change:", change.start, change.oldEnd, change.newEnd);
		}
	}

	public void InsertEmbeddedAtSelection(MS.Win32.UnsafeNativeMethods.InsertAtSelectionFlags flags, object obj, out int startIndex, out int endIndex, out MS.Win32.UnsafeNativeMethods.TS_TEXTCHANGE change)
	{
		startIndex = -1;
		endIndex = -1;
		change.start = 0;
		change.oldEnd = 0;
		change.newEnd = 0;
		if (IsReadOnly)
		{
			throw new COMException(SR.TextStore_TS_E_READONLY, -2147220983);
		}
		throw new COMException(SR.TextStore_TS_E_FORMAT, -2147220982);
	}

	public int RequestSupportedAttrs(MS.Win32.UnsafeNativeMethods.AttributeFlags flags, int count, Guid[] filterAttributes)
	{
		PrepareAttributes((InputScope)UiScope.GetValue(InputMethod.InputScopeProperty), (double)UiScope.GetValue(TextElement.FontSizeProperty), (FontFamily)UiScope.GetValue(TextElement.FontFamilyProperty), (XmlLanguage)UiScope.GetValue(FrameworkContentElement.LanguageProperty), UiScope, count, filterAttributes);
		if (_preparedattributes.Count == 0)
		{
			return 1;
		}
		return 0;
	}

	public int RequestAttrsAtPosition(int index, int count, Guid[] filterAttributes, MS.Win32.UnsafeNativeMethods.AttributeFlags flags)
	{
		ITextPointer textPointer = CreatePointerAtCharOffset(index, LogicalDirection.Forward);
		PrepareAttributes((InputScope)textPointer.GetValue(InputMethod.InputScopeProperty), (double)textPointer.GetValue(TextElement.FontSizeProperty), (FontFamily)textPointer.GetValue(TextElement.FontFamilyProperty), (XmlLanguage)textPointer.GetValue(FrameworkContentElement.LanguageProperty), null, count, filterAttributes);
		if (_preparedattributes.Count == 0)
		{
			return 1;
		}
		return 0;
	}

	public void RequestAttrsTransitioningAtPosition(int position, int count, Guid[] filterAttributes, MS.Win32.UnsafeNativeMethods.AttributeFlags flags)
	{
		throw new COMException(SR.TextStore_E_NOTIMPL, -2147467263);
	}

	public void FindNextAttrTransition(int startIndex, int haltIndex, int count, Guid[] filterAttributes, MS.Win32.UnsafeNativeMethods.AttributeFlags flags, out int acpNext, out bool found, out int foundOffset)
	{
		acpNext = 0;
		found = false;
		foundOffset = 0;
	}

	public void RetrieveRequestedAttrs(int count, MS.Win32.UnsafeNativeMethods.TS_ATTRVAL[] attributeVals, out int fetched)
	{
		fetched = 0;
		for (int i = 0; i < count && i < _preparedattributes.Count; i++)
		{
			attributeVals[i] = (MS.Win32.UnsafeNativeMethods.TS_ATTRVAL)_preparedattributes[i];
			fetched++;
		}
		_preparedattributes.Clear();
		_preparedattributes = null;
	}

	public void GetEnd(out int end)
	{
		end = TextContainer.IMECharCount;
	}

	public void GetActiveView(out int viewCookie)
	{
		viewCookie = 0;
	}

	public void GetACPFromPoint(int viewCookie, ref MS.Win32.UnsafeNativeMethods.POINT tsfPoint, MS.Win32.UnsafeNativeMethods.GetPositionFromPointFlags flags, out int positionCP)
	{
		MS.Win32.NativeMethods.POINT pt = new MS.Win32.NativeMethods.POINT(tsfPoint.x, tsfPoint.y);
		GetVisualInfo(out var source, out var win32Window, out var view);
		CompositionTarget compositionTarget = source.CompositionTarget;
		SafeNativeMethods.ScreenToClient(new HandleRef(null, win32Window.Handle), ref pt);
		Point point = new Point(pt.x, pt.y);
		point = compositionTarget.TransformFromDevice.Transform(point);
		compositionTarget.RootVisual.TransformToDescendant(RenderScope)?.TryTransform(point, out point);
		if (!view.Validate(point))
		{
			throw new COMException(SR.TextStore_TS_E_NOLAYOUT, -2147220986);
		}
		ITextPointer textPositionFromPoint = view.GetTextPositionFromPoint(point, (flags & MS.Win32.UnsafeNativeMethods.GetPositionFromPointFlags.GXFPF_NEAREST) != 0);
		if (textPositionFromPoint == null)
		{
			throw new COMException(SR.TextStore_TS_E_INVALIDPOINT, -2147220985);
		}
		positionCP = textPositionFromPoint.CharOffset;
		if ((flags & MS.Win32.UnsafeNativeMethods.GetPositionFromPointFlags.GXFPF_ROUND_NEAREST) == 0)
		{
			ITextPointer position = textPositionFromPoint.CreatePointer(LogicalDirection.Backward);
			ITextPointer textPointer = textPositionFromPoint.CreatePointer(LogicalDirection.Forward);
			textPointer.MoveToNextInsertionPosition(LogicalDirection.Backward);
			Rect rectangleFromTextPosition = view.GetRectangleFromTextPosition(position);
			Rect rectangleFromTextPosition2 = view.GetRectangleFromTextPosition(textPointer);
			Point point2 = new Point(Math.Min(rectangleFromTextPosition2.Left, rectangleFromTextPosition.Left), Math.Min(rectangleFromTextPosition2.Top, rectangleFromTextPosition.Top));
			Point point3 = new Point(Math.Max(rectangleFromTextPosition2.Left, rectangleFromTextPosition.Left), Math.Max(rectangleFromTextPosition2.Bottom, rectangleFromTextPosition.Bottom));
			if (new Rect(point2, point3).Contains(point))
			{
				positionCP--;
			}
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.GetACPFromPoint, "pt:", tsfPoint.x, tsfPoint.y, "fl:", flags, "cp:", positionCP);
		}
	}

	void MS.Win32.UnsafeNativeMethods.ITextStoreACP.GetTextExt(int viewCookie, int startIndex, int endIndex, out MS.Win32.UnsafeNativeMethods.RECT rect, out bool clipped)
	{
		_isInUpdateLayout = true;
		UiScope.UpdateLayout();
		_isInUpdateLayout = false;
		if (_hasTextChangedInUpdateLayout)
		{
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.GetTextExt, "text changed - bail");
			}
			_netCharCount = TextContainer.IMECharCount;
			throw new COMException(SR.TextStore_TS_E_NOLAYOUT, -2147220986);
		}
		rect = default(MS.Win32.UnsafeNativeMethods.RECT);
		clipped = false;
		GetVisualInfo(out var source, out var win32Window, out var _);
		CompositionTarget compositionTarget = source.CompositionTarget;
		ITextPointer textPointer = CreatePointerAtCharOffset(startIndex, LogicalDirection.Forward);
		textPointer.MoveToInsertionPosition(LogicalDirection.Forward);
		if (!TextView.IsValid)
		{
			throw new COMException(SR.TextStore_TS_E_NOLAYOUT, -2147220986);
		}
		Point result;
		Point result2;
		if (startIndex == endIndex)
		{
			Rect characterRect = textPointer.GetCharacterRect(LogicalDirection.Forward);
			result = characterRect.TopLeft;
			result2 = characterRect.BottomRight;
		}
		else
		{
			Rect rect2 = new Rect(Size.Empty);
			ITextPointer textPointer2 = textPointer.CreatePointer();
			ITextPointer textPointer3 = CreatePointerAtCharOffset(endIndex, LogicalDirection.Backward);
			textPointer3.MoveToInsertionPosition(LogicalDirection.Backward);
			ITextPointer textPointer4;
			bool flag;
			do
			{
				TextSegment lineRange = TextView.GetLineRange(textPointer2);
				Rect rect3;
				if (!lineRange.IsNull)
				{
					ITextPointer start = ((lineRange.Start.CompareTo(textPointer) <= 0) ? textPointer : lineRange.Start);
					textPointer4 = ((lineRange.End.CompareTo(textPointer3) >= 0) ? textPointer3 : lineRange.End);
					rect3 = GetLineBounds(start, textPointer4);
					flag = textPointer2.MoveToLineBoundary(1) != 0;
				}
				else
				{
					rect3 = textPointer2.GetCharacterRect(LogicalDirection.Forward);
					flag = textPointer2.MoveToNextInsertionPosition(LogicalDirection.Forward);
					textPointer4 = textPointer2;
				}
				if (!rect3.IsEmpty)
				{
					rect2.Union(rect3);
				}
			}
			while (textPointer4.CompareTo(textPointer3) != 0 && flag);
			result = rect2.TopLeft;
			result2 = rect2.BottomRight;
		}
		GeneralTransform generalTransform = UiScope.TransformToAncestor(compositionTarget.RootVisual);
		generalTransform.TryTransform(result, out result);
		generalTransform.TryTransform(result2, out result2);
		rect = TransformRootRectToScreenCoordinates(result, result2, win32Window, source);
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.GetTextExt, startIndex, endIndex, rect.left, rect.top, rect.right, rect.bottom, clipped);
		}
	}

	public void GetScreenExt(int viewCookie, out MS.Win32.UnsafeNativeMethods.RECT rect)
	{
		Rect visualContentBounds = UiScope.VisualContentBounds;
		Rect visualDescendantBounds = UiScope.VisualDescendantBounds;
		visualContentBounds.Union(visualDescendantBounds);
		GetVisualInfo(out var source, out var win32Window, out var _);
		CompositionTarget compositionTarget = source.CompositionTarget;
		Point result = new Point(visualContentBounds.Left, visualContentBounds.Top);
		Point result2 = new Point(visualContentBounds.Right, visualContentBounds.Bottom);
		GeneralTransform generalTransform = UiScope.TransformToAncestor(compositionTarget.RootVisual);
		generalTransform.TryTransform(result, out result);
		generalTransform.TryTransform(result2, out result2);
		rect = TransformRootRectToScreenCoordinates(result, result2, win32Window, source);
	}

	void MS.Win32.UnsafeNativeMethods.ITextStoreACP.GetWnd(int viewCookie, out nint hwnd)
	{
		hwnd = IntPtr.Zero;
		hwnd = CriticalSourceWnd;
	}

	void MS.Win32.UnsafeNativeMethods.ITfThreadFocusSink.OnSetThreadFocus()
	{
		if (IsTextEditorValid && Keyboard.FocusedElement == UiScope)
		{
			OnGotFocus();
		}
	}

	public void OnKillThreadFocus()
	{
	}

	public void OnStartComposition(MS.Win32.UnsafeNativeMethods.ITfCompositionView view, out bool ok)
	{
		if (_isComposing)
		{
			ok = false;
			return;
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BOnStartComposition);
		}
		GetCompositionPositions(view, out var start, out var end);
		int startOffsetBefore = start.Offset;
		int endOffsetBefore = end.Offset;
		_lastCompositionText = TextRangeBase.GetTextInternal(start, end);
		if (_previousCompositionStartOffset != -1)
		{
			startOffsetBefore = _previousCompositionStartOffset;
			endOffsetBefore = _previousCompositionEndOffset;
		}
		else if (TextEditor.AcceptsRichContent && start.CompareTo(end) != 0)
		{
			TextElement element = (TextElement)((TextPointer)start).Parent;
			TextElement element2 = (TextElement)((TextPointer)end).Parent;
			TextElement commonAncestor = TextElement.GetCommonAncestor(element, element2);
			int iMECharCount = TextContainer.IMECharCount;
			TextRange textRange = new TextRange(start, end);
			string text = textRange.Text;
			if (commonAncestor is Run)
			{
				TextEditor.MarkCultureProperty(textRange, InputLanguageManager.Current.CurrentInputLanguage);
			}
			else if (commonAncestor is Paragraph || commonAncestor is Span)
			{
				TextEditor.SetText(textRange, text, InputLanguageManager.Current.CurrentInputLanguage);
			}
			Invariant.Assert(textRange.Text == text);
			Invariant.Assert(iMECharCount == TextContainer.IMECharCount);
		}
		CompositionEventList.Add(new CompositionEventRecord(CompositionStage.StartComposition, startOffsetBefore, endOffsetBefore, _lastCompositionText));
		_previousCompositionStartOffset = start.Offset;
		_previousCompositionEndOffset = end.Offset;
		_isComposing = true;
		BreakTypingSequence(end);
		ok = true;
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.EOnStartComposition);
		}
	}

	public void OnUpdateComposition(MS.Win32.UnsafeNativeMethods.ITfCompositionView view, MS.Win32.UnsafeNativeMethods.ITfRange rangeNew)
	{
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BOnUpdateComposition);
		}
		TextEditor.CloseToolTip();
		Invariant.Assert(_isComposing);
		Invariant.Assert(_previousCompositionStartOffset != -1);
		GetCompositionPositions(view, out var start, out var end);
		ITextPointer start2 = null;
		ITextPointer end2 = null;
		bool flag = false;
		if (rangeNew != null)
		{
			TextPositionsFromITfRange(rangeNew, out start2, out end2);
			flag = start2.Offset != start.Offset || end2.Offset != end.Offset;
		}
		string textInternal = TextRangeBase.GetTextInternal(start, end);
		if (flag)
		{
			CompositionEventRecord item = new CompositionEventRecord(CompositionStage.UpdateComposition, _previousCompositionStartOffset, _previousCompositionEndOffset, textInternal, isShiftUpdate: true);
			CompositionEventList.Add(item);
			_previousCompositionStartOffset = start2.Offset;
			_previousCompositionEndOffset = end2.Offset;
			_lastCompositionText = null;
		}
		else
		{
			CompositionEventRecord item2 = new CompositionEventRecord(CompositionStage.UpdateComposition, _previousCompositionStartOffset, _previousCompositionEndOffset, textInternal);
			if (CompositionEventList.Count != 0)
			{
				_ = CompositionEventList[CompositionEventList.Count - 1];
			}
			if (_lastCompositionText == null || string.CompareOrdinal(textInternal, _lastCompositionText) != 0)
			{
				CompositionEventList.Add(item2);
			}
			_previousCompositionStartOffset = start.Offset;
			_previousCompositionEndOffset = end.Offset;
			_lastCompositionText = textInternal;
		}
		BreakTypingSequence(end);
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.EOnUpdateComposition);
		}
	}

	public void OnEndComposition(MS.Win32.UnsafeNativeMethods.ITfCompositionView view)
	{
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BOnEndComposition);
		}
		Invariant.Assert(_isComposing);
		Invariant.Assert(_previousCompositionStartOffset != -1);
		GetCompositionPositions(view, out var start, out var end);
		if (_compositionEventState == CompositionEventState.NotRaisingEvents)
		{
			CompositionEventList.Add(new CompositionEventRecord(CompositionStage.EndComposition, start.Offset, end.Offset, TextRangeBase.GetTextInternal(start, end)));
			CompositionParentUndoUnit compositionParentUndoUnit = PeekCompositionParentUndoUnit();
			if (compositionParentUndoUnit != null)
			{
				compositionParentUndoUnit.IsLastCompositionUnit = true;
			}
		}
		_nextUndoUnitIsFirstCompositionUnit = true;
		_isComposing = false;
		_previousCompositionStartOffset = -1;
		_previousCompositionEndOffset = -1;
		if (_interimSelection)
		{
			_interimSelection = false;
			TextSelection.OnInterimSelectionChanged(_interimSelection);
		}
		BreakTypingSequence(end);
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.EOnEndComposition);
		}
	}

	void MS.Win32.UnsafeNativeMethods.ITfTextEditSink.OnEndEdit(MS.Win32.UnsafeNativeMethods.ITfContext context, int ecReadOnly, MS.Win32.UnsafeNativeMethods.ITfEditRecord editRecord)
	{
		_textservicesproperty.OnEndEdit(context, ecReadOnly, editRecord);
		Marshal.ReleaseComObject(editRecord);
	}

	public void OnTransitoryExtensionUpdated(MS.Win32.UnsafeNativeMethods.ITfContext context, int ecReadOnly, MS.Win32.UnsafeNativeMethods.ITfRange rangeResult, MS.Win32.UnsafeNativeMethods.ITfRange rangeComposition, out bool fDeleteResultRange)
	{
		fDeleteResultRange = true;
		if (rangeResult == null)
		{
			return;
		}
		string text = StringFromITfRange(rangeResult, ecReadOnly);
		if (text.Length <= 0)
		{
			return;
		}
		if (TextEditor.AllowOvertype && TextEditor._OvertypeMode && TextSelection.IsEmpty)
		{
			ITextPointer textPointer = TextSelection.End.CreatePointer();
			textPointer.MoveToInsertionPosition(LogicalDirection.Forward);
			if (textPointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
			{
				char[] array = new char[2];
				textPointer.GetTextInRun(LogicalDirection.Forward, array, 0, array.Length);
				if (array[0] != Environment.NewLine[0] || array[1] != Environment.NewLine[1])
				{
					int length = text.Length;
					while (length-- > 0)
					{
						TextSelection.ExtendToNextInsertionPosition(LogicalDirection.Forward);
					}
				}
			}
		}
		string text2 = FilterCompositionString(text, TextSelection.Start.GetOffsetToPosition(TextSelection.End));
		if (text2 == null)
		{
			throw new COMException(SR.TextStore_CompositionRejected, -2147467259);
		}
		TextEditor.SetText(TextSelection, text2, InputLanguageManager.Current.CurrentInputLanguage);
		TextSelection.Select(TextSelection.End, TextSelection.End);
	}

	public int AdviceMouseSink(MS.Win32.UnsafeNativeMethods.ITfRangeACP range, MS.Win32.UnsafeNativeMethods.ITfMouseSink sink, out int dwCookie)
	{
		if (_mouseSinks == null)
		{
			_mouseSinks = new ArrayList(1);
		}
		_mouseSinks.Sort();
		dwCookie = 0;
		while (dwCookie < _mouseSinks.Count && ((MouseSink)_mouseSinks[dwCookie]).Cookie == dwCookie)
		{
			dwCookie++;
		}
		Invariant.Assert(dwCookie != -1);
		_mouseSinks.Add(new MouseSink(range, sink, dwCookie));
		if (_mouseSinks.Count == 1)
		{
			UiScope.PreviewMouseLeftButtonDown += OnMouseButtonEvent;
			UiScope.PreviewMouseLeftButtonUp += OnMouseButtonEvent;
			UiScope.PreviewMouseRightButtonDown += OnMouseButtonEvent;
			UiScope.PreviewMouseRightButtonUp += OnMouseButtonEvent;
			UiScope.PreviewMouseMove += OnMouseEvent;
		}
		return 0;
	}

	public int UnadviceMouseSink(int dwCookie)
	{
		int result = -2147024809;
		for (int i = 0; i < _mouseSinks.Count; i++)
		{
			MouseSink mouseSink = (MouseSink)_mouseSinks[i];
			if (mouseSink.Cookie == dwCookie)
			{
				_mouseSinks.RemoveAt(i);
				if (_mouseSinks.Count == 0)
				{
					UiScope.PreviewMouseLeftButtonDown -= OnMouseButtonEvent;
					UiScope.PreviewMouseLeftButtonUp -= OnMouseButtonEvent;
					UiScope.PreviewMouseRightButtonDown -= OnMouseButtonEvent;
					UiScope.PreviewMouseRightButtonUp -= OnMouseButtonEvent;
					UiScope.PreviewMouseMove -= OnMouseEvent;
				}
				if (mouseSink.Locked)
				{
					mouseSink.PendingDispose = true;
				}
				else
				{
					mouseSink.Dispose();
				}
				result = 0;
				break;
			}
		}
		return result;
	}

	internal void OnAttach()
	{
		_netCharCount = TextContainer.IMECharCount;
		_textservicesHost = TextServicesHost.Current;
		_textservicesHost.RegisterTextStore(this);
		TextContainer.Change += OnTextContainerChange;
		_textservicesproperty = new TextServicesProperty(this);
		if (IMECompositionTracer.IsEnabled)
		{
			IMECompositionTracer.ConfigureTracing(this);
		}
	}

	internal void OnDetach(bool finalizer)
	{
		if (IsTextEditorValid)
		{
			TextContainer.Change -= OnTextContainerChange;
		}
		_textservicesHost.UnregisterTextStore(this, finalizer);
		_textservicesproperty = null;
	}

	internal void OnGotFocus()
	{
		if ((bool)UiScope.GetValue(InputMethod.IsInputMethodEnabledProperty))
		{
			_textservicesHost.ThreadManager.SetFocus(DocumentManager);
		}
		if (_makeLayoutChangeOnGotFocus)
		{
			OnLayoutUpdated();
			_makeLayoutChangeOnGotFocus = false;
		}
	}

	internal void OnLostFocus()
	{
		CompleteComposition();
	}

	internal void OnLayoutUpdated()
	{
		if (HasSink)
		{
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.BOnLayoutChange);
			}
			_sink.OnLayoutChange(MS.Win32.UnsafeNativeMethods.TsLayoutCode.TS_LC_CHANGE, 0);
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.EOnLayoutChange);
			}
		}
		if (_textservicesproperty != null)
		{
			_textservicesproperty.OnLayoutUpdated();
		}
	}

	internal void OnSelectionChange()
	{
		_compositionModifiedByEventListener = true;
	}

	internal void OnSelectionChanged()
	{
		if (_compositionEventState == CompositionEventState.RaisingEvents)
		{
			return;
		}
		if (_ignoreNextSelectionChange)
		{
			_ignoreNextSelectionChange = false;
		}
		else if (HasSink)
		{
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.BOnSelectionChange);
			}
			_sink.OnSelectionChange();
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.EOnSelectionChange);
			}
		}
	}

	internal bool QueryRangeOrReconvertSelection(bool fDoReconvert)
	{
		if (_isComposing && !fDoReconvert)
		{
			GetCompositionPositions(out var start, out var end);
			if (start != null && end != null && start.CompareTo(TextSelection.Start) <= 0 && start.CompareTo(TextSelection.End) <= 0 && end.CompareTo(TextSelection.Start) >= 0 && end.CompareTo(TextSelection.End) >= 0)
			{
				return true;
			}
		}
		MS.Win32.UnsafeNativeMethods.ITfFnReconversion funcReconv;
		MS.Win32.UnsafeNativeMethods.ITfRange rangeNew;
		bool fnReconv = GetFnReconv(TextSelection.Start, TextSelection.End, out funcReconv, out rangeNew);
		if (funcReconv != null)
		{
			if (fDoReconvert)
			{
				funcReconv.Reconvert(rangeNew);
			}
			Marshal.ReleaseComObject(funcReconv);
		}
		if (rangeNew != null)
		{
			Marshal.ReleaseComObject(rangeNew);
		}
		return fnReconv;
	}

	internal MS.Win32.UnsafeNativeMethods.ITfCandidateList GetReconversionCandidateList()
	{
		MS.Win32.UnsafeNativeMethods.ITfCandidateList candList = null;
		GetFnReconv(TextSelection.Start, TextSelection.End, out var funcReconv, out var rangeNew);
		if (funcReconv != null)
		{
			funcReconv.GetReconversion(rangeNew, out candList);
			Marshal.ReleaseComObject(funcReconv);
		}
		if (rangeNew != null)
		{
			Marshal.ReleaseComObject(rangeNew);
		}
		return candList;
	}

	private bool GetFnReconv(ITextPointer textStart, ITextPointer textEnd, out MS.Win32.UnsafeNativeMethods.ITfFnReconversion funcReconv, out MS.Win32.UnsafeNativeMethods.ITfRange rangeNew)
	{
		bool isConvertable = false;
		funcReconv = null;
		rangeNew = null;
		DocumentManager.GetBase(out var context);
		context.GetStart(EditCookie, out var range);
		MS.Win32.UnsafeNativeMethods.ITfRangeACP obj = range as MS.Win32.UnsafeNativeMethods.ITfRangeACP;
		int charOffset = textStart.CharOffset;
		int charOffset2 = textEnd.CharOffset;
		obj.SetExtent(charOffset, charOffset2 - charOffset);
		Guid classId = MS.Win32.UnsafeNativeMethods.GUID_SYSTEM_FUNCTIONPROVIDER;
		Guid guid = MS.Win32.UnsafeNativeMethods.Guid_Null;
		Guid riid = MS.Win32.UnsafeNativeMethods.IID_ITfFnReconversion;
		_textservicesHost.ThreadManager.GetFunctionProvider(ref classId, out var funcProvider);
		funcProvider.GetFunction(ref guid, ref riid, out var obj2);
		funcReconv = obj2 as MS.Win32.UnsafeNativeMethods.ITfFnReconversion;
		funcReconv.QueryRange(range, out rangeNew, out isConvertable);
		Marshal.ReleaseComObject(funcProvider);
		if (!isConvertable)
		{
			Marshal.ReleaseComObject(funcReconv);
			funcReconv = null;
		}
		Marshal.ReleaseComObject(range);
		Marshal.ReleaseComObject(context);
		return isConvertable;
	}

	internal void CompleteCompositionAsync()
	{
		Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(CompleteCompositionHandler), null);
	}

	internal void CompleteComposition()
	{
		if (_isComposing)
		{
			FrameworkTextComposition.CompleteCurrentComposition(DocumentManager);
		}
		_previousCompositionStartOffset = -1;
		_previousCompositionEndOffset = -1;
		_previousCompositionStart = null;
		_previousCompositionEnd = null;
	}

	internal ITextPointer CreatePointerAtCharOffset(int charOffset, LogicalDirection direction)
	{
		ValidateCharOffset(charOffset);
		ITextPointer textPointer = TextContainer.CreatePointerAtCharOffset(charOffset, direction);
		if (textPointer == null)
		{
			textPointer = TextSelection.Start.CreatePointer(direction);
		}
		return textPointer;
	}

	internal void MakeLayoutChangeOnGotFocus()
	{
		if (_isComposing)
		{
			_makeLayoutChangeOnGotFocus = true;
		}
	}

	internal void UpdateCompositionText(FrameworkTextComposition composition)
	{
		if (_compositionModifiedByEventListener)
		{
			return;
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BUpdateCompositionText);
		}
		_handledByTextStoreListener = true;
		bool compositionModifiedByEventListener = false;
		ITextRange textRange;
		string text;
		if (composition._ResultStart != null)
		{
			textRange = new TextRange(composition._ResultStart, composition._ResultEnd, ignoreTextUnitBoundaries: true);
			text = TextEditor._FilterText(composition.Text, textRange);
			if (text.Length != composition.Text.Length)
			{
				compositionModifiedByEventListener = true;
			}
		}
		else
		{
			textRange = new TextRange(composition._CompositionStart, composition._CompositionEnd, ignoreTextUnitBoundaries: true);
			text = TextEditor._FilterText(composition.CompositionText, textRange, filterMaxLength: false);
			Invariant.Assert(text.Length == composition.CompositionText.Length);
		}
		_nextUndoUnitIsFirstCompositionUnit = false;
		CompositionParentUndoUnit compositionParentUndoUnit = PeekCompositionParentUndoUnit();
		if (compositionParentUndoUnit != null)
		{
			compositionParentUndoUnit.IsLastCompositionUnit = false;
		}
		CompositionParentUndoUnit compositionParentUndoUnit2 = OpenCompositionUndoUnit(textRange.Start, textRange.End);
		UndoCloseAction undoCloseAction = UndoCloseAction.Rollback;
		if (composition._ResultStart != null)
		{
			_nextUndoUnitIsFirstCompositionUnit = true;
			compositionParentUndoUnit2.IsLastCompositionUnit = true;
		}
		TextSelection.BeginChange();
		try
		{
			TextEditor.SetText(textRange, text, InputLanguageManager.Current.CurrentInputLanguage);
			if (_interimSelection)
			{
				TextSelection.Select(textRange.Start, textRange.End);
			}
			else
			{
				TextSelection.SetCaretToPosition(textRange.End, LogicalDirection.Backward, allowStopAtLineEnd: true, allowStopNearSpace: true);
			}
			compositionParentUndoUnit2.RecordRedoSelectionState(textRange.End, textRange.End);
			undoCloseAction = UndoCloseAction.Commit;
		}
		finally
		{
			_compositionModifiedByEventListener = compositionModifiedByEventListener;
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.BSelectionChangeEvent);
			}
			TextSelection.EndChange();
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.ESelectionChangeEvent);
			}
			CloseTextParentUndoUnit(compositionParentUndoUnit2, undoCloseAction);
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.EUpdateCompositionText);
		}
	}

	internal static FrameworkTextComposition CreateComposition(TextEditor editor, object owner)
	{
		if (editor.AcceptsRichContent)
		{
			return new FrameworkRichTextComposition(InputManager.UnsecureCurrent, editor.UiScope, owner);
		}
		return new FrameworkTextComposition(InputManager.Current, editor.UiScope, owner);
	}

	private void OnTextContainerChange(object sender, TextContainerChangeEventArgs args)
	{
		if (args.IMECharCount > 0 && (args.TextChange == TextChangeType.ContentAdded || args.TextChange == TextChangeType.ContentRemoved))
		{
			_compositionModifiedByEventListener = true;
		}
		if (_compositionEventState == CompositionEventState.RaisingEvents)
		{
			return;
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BOnTextContainerChange);
		}
		Invariant.Assert(sender == TextContainer);
		if (_lockFlags == (MS.Win32.UnsafeNativeMethods.LockFlags)0 && HasSink)
		{
			int num = 0;
			int num2 = 0;
			if (args.TextChange == TextChangeType.ContentAdded)
			{
				num = args.IMECharCount;
			}
			else if (args.TextChange == TextChangeType.ContentRemoved)
			{
				num2 = args.IMECharCount;
			}
			if (num > 0 || num2 > 0)
			{
				MS.Win32.UnsafeNativeMethods.TS_TEXTCHANGE change = default(MS.Win32.UnsafeNativeMethods.TS_TEXTCHANGE);
				change.start = args.ITextPosition.CharOffset;
				change.oldEnd = change.start + num2;
				change.newEnd = change.start + num;
				ValidateChange(change);
				if (IsTracing)
				{
					IMECompositionTracer.Trace(this, IMECompositionTraceOp.BOnTextChange);
				}
				try
				{
					_textChangeReentrencyCount++;
					_sink.OnTextChange((MS.Win32.UnsafeNativeMethods.OnTextChangeFlags)0, ref change);
				}
				finally
				{
					_textChangeReentrencyCount--;
				}
				if (IsTracing)
				{
					IMECompositionTracer.Trace(this, IMECompositionTraceOp.EOnTextChange);
				}
			}
		}
		else if (_isInUpdateLayout)
		{
			_hasTextChangedInUpdateLayout = true;
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.EOnTextContainerChange);
		}
	}

	private object GrantLockHandler(object o)
	{
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BGrantLockHandler);
		}
		if (_textservicesHost != null && HasSink)
		{
			GrantLockWorker(_pendingAsyncLockFlags);
		}
		_pendingAsyncLockFlags = (MS.Win32.UnsafeNativeMethods.LockFlags)0;
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.EGrantLockHandler);
		}
		return null;
	}

	private int GrantLockWorker(MS.Win32.UnsafeNativeMethods.LockFlags flags)
	{
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BGrantLockWorker, flags);
		}
		TextEditor textEditor = TextEditor;
		int result;
		if (textEditor == null)
		{
			result = -2147467259;
		}
		else
		{
			_lockFlags = flags;
			_hasTextChangedInUpdateLayout = false;
			UndoManager undoManager = UndoManager.GetUndoManager(textEditor.TextContainer.Parent);
			int previousUndoCount = 0;
			bool isImeSupportModeEnabled = false;
			if (undoManager != null)
			{
				previousUndoCount = undoManager.UndoCount;
				isImeSupportModeEnabled = undoManager.IsImeSupportModeEnabled;
				undoManager.IsImeSupportModeEnabled = true;
			}
			_previousCompositionStartOffset = ((_previousCompositionStart == null) ? (-1) : _previousCompositionStart.Offset);
			_previousCompositionEndOffset = ((_previousCompositionEnd == null) ? (-1) : _previousCompositionEnd.Offset);
			try
			{
				textEditor.Selection.BeginChangeNoUndo();
				try
				{
					result = GrantLock();
					if (_pendingWriteReq)
					{
						_lockFlags = MS.Win32.UnsafeNativeMethods.LockFlags.TS_LF_READWRITE;
						GrantLock();
					}
				}
				finally
				{
					_pendingWriteReq = false;
					_lockFlags = (MS.Win32.UnsafeNativeMethods.LockFlags)0;
					_ignoreNextSelectionChange = textEditor.Selection._IsChanged;
					try
					{
						textEditor.Selection.EndChange(disableScroll: false, skipEvents: true);
					}
					finally
					{
						_ignoreNextSelectionChange = false;
						_previousCompositionStart = ((_previousCompositionStartOffset == -1) ? null : textEditor.TextContainer.CreatePointerAtOffset(_previousCompositionStartOffset, LogicalDirection.Backward));
						_previousCompositionEnd = ((_previousCompositionEndOffset == -1) ? null : textEditor.TextContainer.CreatePointerAtOffset(_previousCompositionEndOffset, LogicalDirection.Forward));
					}
				}
				if (undoManager != null)
				{
					HandleCompositionEvents(previousUndoCount);
				}
			}
			finally
			{
				if (undoManager != null)
				{
					undoManager.IsImeSupportModeEnabled = isImeSupportModeEnabled;
				}
				_previousCompositionStart = ((_previousCompositionStartOffset == -1) ? null : textEditor.TextContainer.CreatePointerAtOffset(_previousCompositionStartOffset, LogicalDirection.Backward));
				_previousCompositionEnd = ((_previousCompositionEndOffset == -1) ? null : textEditor.TextContainer.CreatePointerAtOffset(_previousCompositionEndOffset, LogicalDirection.Forward));
			}
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.EGrantLockWorker, result.ToString("X"));
		}
		return result;
	}

	private int GrantLock()
	{
		Invariant.Assert(Thread.CurrentThread == _textservicesHost.Dispatcher.Thread, "GrantLock called on bad thread!");
		VerifyTextStoreConsistency();
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BOnLockGranted);
		}
		int result = _sink.OnLockGranted(_lockFlags);
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.EOnLockGranted);
		}
		VerifyTextStoreConsistency();
		return result;
	}

	private static bool WalkTextRun(ITextPointer navigator, ITextPointer limit, char[] text, int cchReq, ref int charsCopied, MS.Win32.UnsafeNativeMethods.TS_RUNINFO[] runInfo, int cRunInfoReq, ref int cRunInfoRcv)
	{
		Invariant.Assert(navigator.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text);
		Invariant.Assert(limit == null || navigator.CompareTo(limit) <= 0);
		bool result = false;
		int num;
		if (cchReq > 0)
		{
			num = TextPointerBase.GetTextWithLimit(navigator, LogicalDirection.Forward, text, charsCopied, Math.Min(cchReq, text.Length - charsCopied), limit);
			navigator.MoveByOffset(num);
			charsCopied += num;
			result = text.Length == charsCopied || (limit != null && navigator.CompareTo(limit) == 0);
		}
		else
		{
			num = navigator.GetTextRunLength(LogicalDirection.Forward);
			navigator.MoveToNextContextPosition(LogicalDirection.Forward);
			if (limit != null && navigator.CompareTo(limit) >= 0)
			{
				int offsetToPosition = limit.GetOffsetToPosition(navigator);
				Invariant.Assert(offsetToPosition >= 0 && offsetToPosition <= num, "Bogus offset -- extends past run!");
				num -= offsetToPosition;
				navigator.MoveToPosition(limit);
				result = true;
			}
		}
		if (cRunInfoReq > 0 && num > 0)
		{
			if (cRunInfoRcv > 0 && runInfo[cRunInfoRcv - 1].type == MS.Win32.UnsafeNativeMethods.TsRunType.TS_RT_PLAIN)
			{
				runInfo[cRunInfoRcv - 1].count += num;
			}
			else
			{
				runInfo[cRunInfoRcv].count = num;
				runInfo[cRunInfoRcv].type = MS.Win32.UnsafeNativeMethods.TsRunType.TS_RT_PLAIN;
				cRunInfoRcv++;
			}
		}
		return result;
	}

	private static bool WalkObjectRun(ITextPointer navigator, ITextPointer limit, char[] text, int cchReq, ref int charsCopied, MS.Win32.UnsafeNativeMethods.TS_RUNINFO[] runInfo, int cRunInfoReq, ref int cRunInfoRcv)
	{
		Invariant.Assert(navigator.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.EmbeddedElement);
		Invariant.Assert(limit == null || navigator.CompareTo(limit) <= 0);
		if (limit != null && navigator.CompareTo(limit) == 0)
		{
			return true;
		}
		navigator.MoveToNextContextPosition(LogicalDirection.Forward);
		if (cchReq >= 1)
		{
			text[charsCopied] = '￼';
			charsCopied++;
		}
		if (cRunInfoReq > 0)
		{
			if (cRunInfoRcv > 0 && runInfo[cRunInfoRcv - 1].type == MS.Win32.UnsafeNativeMethods.TsRunType.TS_RT_PLAIN)
			{
				runInfo[cRunInfoRcv - 1].count++;
				return false;
			}
			runInfo[cRunInfoRcv].count = 1;
			runInfo[cRunInfoRcv].type = MS.Win32.UnsafeNativeMethods.TsRunType.TS_RT_PLAIN;
			cRunInfoRcv++;
		}
		return false;
	}

	private static bool WalkRegionBoundary(ITextPointer navigator, ITextPointer limit, char[] text, int cchReq, ref int charsCopied, MS.Win32.UnsafeNativeMethods.TS_RUNINFO[] runInfo, int cRunInfoReq, ref int cRunInfoRcv)
	{
		Invariant.Assert(navigator.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart || navigator.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd);
		Invariant.Assert(limit == null || navigator.CompareTo(limit) <= 0);
		if (limit != null && navigator.CompareTo(limit) >= 0)
		{
			return true;
		}
		bool result = false;
		if (cchReq > 0)
		{
			char c = ((!(navigator.GetAdjacentElement(LogicalDirection.Forward) is TableCell)) ? '\n' : '\0');
			text[charsCopied] = c;
			navigator.MoveByOffset(1);
			charsCopied++;
			result = text.Length == charsCopied || (limit != null && navigator.CompareTo(limit) == 0);
		}
		else
		{
			navigator.MoveByOffset(1);
		}
		if (cRunInfoReq > 0)
		{
			if (cRunInfoRcv > 0 && runInfo[cRunInfoRcv - 1].type == MS.Win32.UnsafeNativeMethods.TsRunType.TS_RT_PLAIN)
			{
				runInfo[cRunInfoRcv - 1].count++;
			}
			else
			{
				runInfo[cRunInfoRcv].count = 1;
				runInfo[cRunInfoRcv].type = MS.Win32.UnsafeNativeMethods.TsRunType.TS_RT_PLAIN;
				cRunInfoRcv++;
			}
		}
		return result;
	}

	private void GetVisualInfo(out PresentationSource source, out IWin32Window win32Window, out ITextView view)
	{
		source = PresentationSource.CriticalFromVisual(RenderScope);
		win32Window = source as IWin32Window;
		if (win32Window == null)
		{
			throw new COMException(SR.TextStore_TS_E_NOLAYOUT, -2147220986);
		}
		view = TextView;
	}

	private static MS.Win32.UnsafeNativeMethods.RECT TransformRootRectToScreenCoordinates(Point milPointTopLeft, Point milPointBottomRight, IWin32Window win32Window, PresentationSource source)
	{
		MS.Win32.UnsafeNativeMethods.RECT result = default(MS.Win32.UnsafeNativeMethods.RECT);
		CompositionTarget compositionTarget = source.CompositionTarget;
		milPointTopLeft = compositionTarget.TransformToDevice.Transform(milPointTopLeft);
		milPointBottomRight = compositionTarget.TransformToDevice.Transform(milPointBottomRight);
		nint zero = IntPtr.Zero;
		zero = win32Window.Handle;
		MS.Win32.NativeMethods.POINT pt = default(MS.Win32.NativeMethods.POINT);
		MS.Win32.UnsafeNativeMethods.ClientToScreen(new HandleRef(null, zero), ref pt);
		result.left = (int)((double)pt.x + milPointTopLeft.X);
		result.right = (int)((double)pt.x + milPointBottomRight.X);
		result.top = (int)((double)pt.y + milPointTopLeft.Y);
		result.bottom = (int)((double)pt.y + milPointBottomRight.Y);
		return result;
	}

	private static string GetFontFamilyName(FontFamily fontFamily, XmlLanguage language)
	{
		if (fontFamily == null)
		{
			return null;
		}
		if (fontFamily.Source != null)
		{
			return fontFamily.Source;
		}
		LanguageSpecificStringDictionary familyNames = fontFamily.FamilyNames;
		if (familyNames == null)
		{
			return null;
		}
		foreach (XmlLanguage matchingLanguage in language.MatchingLanguages)
		{
			string text = familyNames[matchingLanguage];
			if (text != null)
			{
				return text;
			}
		}
		return null;
	}

	private void PrepareAttributes(InputScope inputScope, double fontSize, FontFamily fontFamily, XmlLanguage language, Visual visual, int count, Guid[] filterAttributes)
	{
		if (_preparedattributes == null)
		{
			_preparedattributes = new ArrayList(count);
		}
		else
		{
			_preparedattributes.Clear();
		}
		for (int i = 0; i < _supportingattributes.Length; i++)
		{
			if (count != 0)
			{
				bool flag = false;
				for (int j = 0; j < count; j++)
				{
					if (_supportingattributes[i].Guid.Equals(filterAttributes[j]))
					{
						flag = true;
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			MS.Win32.UnsafeNativeMethods.TS_ATTRVAL tS_ATTRVAL = default(MS.Win32.UnsafeNativeMethods.TS_ATTRVAL);
			tS_ATTRVAL.attributeId = _supportingattributes[i].Guid;
			tS_ATTRVAL.overlappedId = (int)_supportingattributes[i].Style;
			tS_ATTRVAL.val = new MS.Win32.NativeMethods.VARIANT();
			tS_ATTRVAL.val.SuppressFinalize();
			switch (_supportingattributes[i].Style)
			{
			case AttributeStyle.InputScope:
			{
				object o = new InputScopeAttribute(inputScope);
				tS_ATTRVAL.val.vt = 13;
				tS_ATTRVAL.val.data1.Value = Marshal.GetIUnknownForObject(o);
				break;
			}
			case AttributeStyle.Font_Style_Height:
				tS_ATTRVAL.val.vt = 3;
				tS_ATTRVAL.val.data1.Value = (int)fontSize;
				break;
			case AttributeStyle.Font_FaceName:
			{
				string fontFamilyName = GetFontFamilyName(fontFamily, language);
				if (fontFamilyName != null)
				{
					tS_ATTRVAL.val.vt = 8;
					tS_ATTRVAL.val.data1.Value = Marshal.StringToBSTR(fontFamilyName);
				}
				break;
			}
			case AttributeStyle.Font_SizePts:
				tS_ATTRVAL.val.vt = 3;
				tS_ATTRVAL.val.data1.Value = (int)(fontSize / 96.0 * 72.0);
				break;
			case AttributeStyle.Text_ReadOnly:
				tS_ATTRVAL.val.vt = 11;
				tS_ATTRVAL.val.data1.Value = (IsReadOnly ? 1 : 0);
				break;
			case AttributeStyle.Text_Orientation:
			{
				tS_ATTRVAL.val.vt = 3;
				tS_ATTRVAL.val.data1.Value = 0;
				PresentationSource presentationSource = null;
				presentationSource = PresentationSource.CriticalFromVisual(RenderScope);
				if (presentationSource == null)
				{
					break;
				}
				Visual rootVisual = presentationSource.RootVisual;
				if (rootVisual == null || visual == null)
				{
					break;
				}
				Transform affineTransform = visual.TransformToAncestor(rootVisual).AffineTransform;
				if (affineTransform != null)
				{
					Matrix value = affineTransform.Value;
					if (value.M11 != 0.0 || value.M12 != 0.0)
					{
						double num = Math.Asin(value.M12 / Math.Sqrt(value.M11 * value.M11 + value.M12 * value.M12));
						double num2 = Math.Round(Math.Acos(value.M11 / Math.Sqrt(value.M11 * value.M11 + value.M12 * value.M12)) * 180.0 / Math.PI, 0);
						double num3 = ((!(num <= 0.0)) ? (360.0 - num2) : num2);
						tS_ATTRVAL.val.data1.Value = (int)num3 * 10;
					}
				}
				break;
			}
			case AttributeStyle.Text_VerticalWriting:
				tS_ATTRVAL.val.vt = 11;
				tS_ATTRVAL.val.data1.Value = 0;
				break;
			}
			_preparedattributes.Add(tS_ATTRVAL);
		}
	}

	private void TextPositionsFromITfRange(MS.Win32.UnsafeNativeMethods.ITfRange range, out ITextPointer start, out ITextPointer end)
	{
		(range as MS.Win32.UnsafeNativeMethods.ITfRangeACP).GetExtent(out var start2, out var count);
		start = CreatePointerAtCharOffset(start2, LogicalDirection.Backward);
		end = CreatePointerAtCharOffset(start2 + count, LogicalDirection.Forward);
		while (start.CompareTo(end) < 0 && start.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text)
		{
			start.MoveToNextContextPosition(LogicalDirection.Forward);
		}
	}

	private void GetCompositionPositions(out ITextPointer start, out ITextPointer end)
	{
		start = null;
		end = null;
		if (_isComposing)
		{
			MS.Win32.UnsafeNativeMethods.ITfCompositionView currentCompositionView = FrameworkTextComposition.GetCurrentCompositionView(DocumentManager);
			if (currentCompositionView != null)
			{
				GetCompositionPositions(currentCompositionView, out start, out end);
			}
		}
	}

	private void GetCompositionPositions(MS.Win32.UnsafeNativeMethods.ITfCompositionView view, out ITextPointer start, out ITextPointer end)
	{
		view.GetRange(out var range);
		TextPositionsFromITfRange(range, out start, out end);
		Marshal.ReleaseComObject(range);
		Marshal.ReleaseComObject(view);
	}

	private static string StringFromITfRange(MS.Win32.UnsafeNativeMethods.ITfRange range, int ecReadOnly)
	{
		MS.Win32.UnsafeNativeMethods.ITfRangeACP obj = (MS.Win32.UnsafeNativeMethods.ITfRangeACP)range;
		obj.GetExtent(out var _, out var count);
		char[] array = new char[count];
		obj.GetText(ecReadOnly, 0, array, count, out var _);
		return new string(array);
	}

	private void OnMouseButtonEvent(object sender, MouseButtonEventArgs e)
	{
		e.Handled = InternalMouseEventHandler();
	}

	private void OnMouseEvent(object sender, MouseEventArgs e)
	{
		e.Handled = InternalMouseEventHandler();
	}

	private bool InternalMouseEventHandler()
	{
		int num = 0;
		if (Mouse.LeftButton == MouseButtonState.Pressed)
		{
			num |= 1;
		}
		if (Mouse.RightButton == MouseButtonState.Pressed)
		{
			num |= 2;
		}
		if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
		{
			num |= 4;
		}
		if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
		{
			num |= 8;
		}
		Point position = Mouse.GetPosition(RenderScope);
		ITextView textView = TextView;
		if (textView == null)
		{
			return false;
		}
		if (!textView.Validate(position))
		{
			return false;
		}
		ITextPointer textPositionFromPoint = textView.GetTextPositionFromPoint(position, snapToText: false);
		if (textPositionFromPoint == null)
		{
			return false;
		}
		Rect rectangleFromTextPosition = textView.GetRectangleFromTextPosition(textPositionFromPoint);
		ITextPointer textPointer = textPositionFromPoint.CreatePointer();
		if (textPointer == null)
		{
			return false;
		}
		if (position.X - rectangleFromTextPosition.Left >= 0.0)
		{
			textPointer.MoveToNextInsertionPosition(LogicalDirection.Forward);
		}
		else
		{
			textPointer.MoveToNextInsertionPosition(LogicalDirection.Backward);
		}
		Rect rectangleFromTextPosition2 = textView.GetRectangleFromTextPosition(textPointer);
		int charOffset = textPositionFromPoint.CharOffset;
		int num2 = ((position.X - rectangleFromTextPosition.Left >= 0.0) ? ((!((position.X - rectangleFromTextPosition.Left) * 4.0 / (rectangleFromTextPosition2.Left - rectangleFromTextPosition.Left) <= 1.0)) ? 3 : 2) : ((!((position.X - rectangleFromTextPosition2.Left) * 4.0 / (rectangleFromTextPosition.Left - rectangleFromTextPosition2.Left) <= 3.0)) ? 1 : 0));
		bool eaten = false;
		for (int i = 0; i < _mouseSinks.Count; i++)
		{
			if (eaten)
			{
				break;
			}
			MouseSink mouseSink = (MouseSink)_mouseSinks[i];
			mouseSink.Range.GetExtent(out var start, out var count);
			if (charOffset >= start && charOffset <= start + count && (charOffset != start || num2 > 1) && (charOffset != start + count || num2 < 2))
			{
				mouseSink.Locked = true;
				try
				{
					mouseSink.Sink.OnMouseEvent(charOffset - start, num2, num, out eaten);
				}
				finally
				{
					mouseSink.Locked = false;
				}
			}
		}
		return eaten;
	}

	private CompositionParentUndoUnit OpenCompositionUndoUnit()
	{
		return OpenCompositionUndoUnit(null, null);
	}

	private CompositionParentUndoUnit OpenCompositionUndoUnit(ITextPointer compositionStart, ITextPointer compositionEnd)
	{
		UndoManager undoManager = UndoManager.GetUndoManager(TextContainer.Parent);
		if (undoManager == null || !undoManager.IsEnabled)
		{
			return null;
		}
		if (compositionStart == null)
		{
			Invariant.Assert(compositionEnd == null);
			GetCompositionPositions(out compositionStart, out compositionEnd);
		}
		ITextPointer textPointer = ((compositionStart == null || compositionStart.CompareTo(TextSelection.Start) <= 0) ? TextSelection.Start : compositionStart);
		CompositionParentUndoUnit compositionParentUndoUnit = new CompositionParentUndoUnit(TextSelection, textPointer, textPointer, _nextUndoUnitIsFirstCompositionUnit);
		_nextUndoUnitIsFirstCompositionUnit = false;
		undoManager.Open(compositionParentUndoUnit);
		return compositionParentUndoUnit;
	}

	private static Rect GetLineBounds(ITextPointer start, ITextPointer end)
	{
		if (!start.HasValidLayout || !end.HasValidLayout)
		{
			return Rect.Empty;
		}
		Rect characterRect = start.GetCharacterRect(LogicalDirection.Forward);
		characterRect.Union(end.GetCharacterRect(LogicalDirection.Backward));
		ITextPointer textPointer = start.CreatePointer(LogicalDirection.Forward);
		while (textPointer.MoveToNextContextPosition(LogicalDirection.Forward) && textPointer.CompareTo(end) < 0)
		{
			TextPointerContext pointerContext = textPointer.GetPointerContext(LogicalDirection.Backward);
			switch (pointerContext)
			{
			case TextPointerContext.ElementStart:
				characterRect.Union(textPointer.GetCharacterRect(LogicalDirection.Backward));
				textPointer.MoveToElementEdge(ElementEdge.AfterEnd);
				break;
			case TextPointerContext.EmbeddedElement:
			case TextPointerContext.ElementEnd:
				characterRect.Union(textPointer.GetCharacterRect(LogicalDirection.Backward));
				break;
			default:
				Invariant.Assert(pointerContext != TextPointerContext.None);
				break;
			case TextPointerContext.Text:
				break;
			}
		}
		return characterRect;
	}

	private string FilterCompositionString(string text, int charsToReplaceCount)
	{
		string text2 = TextEditor._FilterText(text, charsToReplaceCount, filterMaxLength: false);
		if (text2.Length != text.Length)
		{
			CompleteCompositionAsync();
			return null;
		}
		return text2;
	}

	private object CompleteCompositionHandler(object o)
	{
		CompleteComposition();
		return null;
	}

	private nint GetSourceWnd(bool callerIsTrusted)
	{
		nint result = IntPtr.Zero;
		if (RenderScope != null)
		{
			IWin32Window win32Window = ((!callerIsTrusted) ? (PresentationSource.FromVisual(RenderScope) as IWin32Window) : (PresentationSource.CriticalFromVisual(RenderScope) as IWin32Window));
			if (win32Window != null)
			{
				result = win32Window.Handle;
			}
		}
		return result;
	}

	private void ValidateChange(MS.Win32.UnsafeNativeMethods.TS_TEXTCHANGE change)
	{
		Invariant.Assert(change.start >= 0, "Bad StartIndex");
		Invariant.Assert(change.start <= change.oldEnd, "Bad oldEnd index");
		Invariant.Assert(change.start <= change.newEnd, "Bad newEnd index");
		_netCharCount += change.newEnd - change.oldEnd;
		Invariant.Assert(_netCharCount >= 0, "Negative _netCharCount!");
	}

	private void VerifyTextStoreConsistency()
	{
		if (_netCharCount != TextContainer.IMECharCount)
		{
			Invariant.Assert(condition: false, "TextContainer/TextStore have inconsistent char counts!");
		}
	}

	private void ValidateCharOffset(int offset)
	{
		if (offset < 0 || offset > TextContainer.IMECharCount)
		{
			throw new ArgumentException(SR.Format(SR.TextStore_BadIMECharOffset, offset, TextContainer.IMECharCount));
		}
	}

	private void BreakTypingSequence(ITextPointer caretPosition)
	{
		PeekCompositionParentUndoUnit()?.RecordRedoSelectionState(caretPosition, caretPosition);
	}

	private static void GetAdjustedSelection(ITextPointer startIn, ITextPointer endIn, out ITextPointer startOut, out ITextPointer endOut)
	{
		startOut = startIn;
		endOut = endIn;
		TextPointer textPointer = startOut as TextPointer;
		if (textPointer == null)
		{
			return;
		}
		TextPointer position = (TextPointer)endOut;
		if (startIn.CompareTo(endIn) != 0)
		{
			bool num = TextPointerBase.IsInBlockUIContainer(textPointer) || TextPointerBase.IsInBlockUIContainer(position);
			TableCell tableCellFromPosition = TextRangeEditTables.GetTableCellFromPosition(textPointer);
			TableCell tableCellFromPosition2 = TextRangeEditTables.GetTableCellFromPosition(position);
			bool flag = tableCellFromPosition != null && tableCellFromPosition == tableCellFromPosition2;
			bool flag2 = TextRangeEditTables.GetTableFromPosition(textPointer) != null || TextRangeEditTables.GetTableFromPosition(position) != null;
			if (!num && (flag || !flag2))
			{
				return;
			}
		}
		if (textPointer.IsAtRowEnd)
		{
			TextPointer nextInsertionPosition = textPointer.GetNextInsertionPosition(LogicalDirection.Backward);
			textPointer = TextRangeEditTables.GetAdjustedRowEndPosition(TextRangeEditTables.GetTableFromPosition(textPointer), textPointer);
			if (!textPointer.IsAtInsertionPosition)
			{
				textPointer = nextInsertionPosition;
			}
		}
		else if (TextPointerBase.IsInBlockUIContainer(textPointer))
		{
			textPointer = ((textPointer.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.ElementStart) ? textPointer.GetNextInsertionPosition(LogicalDirection.Forward) : textPointer.GetNextInsertionPosition(LogicalDirection.Backward));
		}
		while (textPointer != null && TextPointerBase.IsBeforeFirstTable(textPointer))
		{
			textPointer = textPointer.GetNextInsertionPosition(LogicalDirection.Forward);
		}
		if (textPointer == null || textPointer.IsAtRowEnd || TextPointerBase.IsInBlockUIContainer(textPointer))
		{
			throw new COMException(SR.TextStore_CompositionRejected, -2147467259);
		}
		startOut = textPointer;
		endOut = textPointer;
	}

	private void GetNormalizedRange(int startCharOffset, int endCharOffset, out ITextPointer start, out ITextPointer end)
	{
		start = CreatePointerAtCharOffset(startCharOffset, LogicalDirection.Forward);
		for (end = ((startCharOffset == endCharOffset) ? start : CreatePointerAtCharOffset(endCharOffset, LogicalDirection.Backward)); start.CompareTo(end) < 0; start.MoveToNextContextPosition(LogicalDirection.Forward))
		{
			switch (start.GetPointerContext(LogicalDirection.Forward))
			{
			case TextPointerContext.ElementStart:
				if (start.GetAdjacentElement(LogicalDirection.Forward) is TextElement { IMELeftEdgeCharCount: 0 })
				{
					continue;
				}
				break;
			case TextPointerContext.ElementEnd:
				continue;
			}
			break;
		}
		if (start.CompareTo(end) == 0)
		{
			start = start.GetFormatNormalizedPosition(LogicalDirection.Backward);
			end = start;
		}
		else
		{
			start = start.GetFormatNormalizedPosition(LogicalDirection.Backward);
			end = end.GetFormatNormalizedPosition(LogicalDirection.Backward);
		}
	}

	private void HandleCompositionEvents(int previousUndoCount)
	{
		if (CompositionEventList.Count != 0 && _compositionEventState == CompositionEventState.NotRaisingEvents)
		{
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.BHandleCompositionEvents);
			}
			_compositionEventState = CompositionEventState.RaisingEvents;
			_replayingIMEChangeReentrancyCount++;
			int num = -1;
			try
			{
				int offset = TextSelection.AnchorPosition.Offset;
				int offset2 = TextSelection.MovingPosition.Offset;
				UndoManager undoManager = UndoManager.GetUndoManager(TextContainer.Parent);
				undoManager.SetRedoStack(null);
				UndoQuietly(undoManager.UndoCount - previousUndoCount);
				Stack imeChangeStack = undoManager.SetRedoStack(null);
				int undoCount = undoManager.UndoCount;
				RaiseCompositionEvents(out var appSelectionAnchorOffset, out var appSelectionMovingOffset);
				num = 0;
				SetFinalDocumentState(undoManager, imeChangeStack, undoManager.UndoCount - undoCount, offset, offset2, appSelectionAnchorOffset, appSelectionMovingOffset);
			}
			finally
			{
				CompositionEventList.Clear();
				_compositionEventState = CompositionEventState.NotRaisingEvents;
				_replayingIMEChangeReentrancyCount += num;
			}
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.EHandleCompositionEvents);
			}
		}
	}

	private TextParentUndoUnit OpenTextParentUndoUnit()
	{
		UndoManager undoManager = UndoManager.GetUndoManager(TextContainer.Parent);
		TextParentUndoUnit textParentUndoUnit = new TextParentUndoUnit(TextSelection, TextSelection.Start, TextSelection.Start);
		undoManager.Open(textParentUndoUnit);
		return textParentUndoUnit;
	}

	private void CloseTextParentUndoUnit(TextParentUndoUnit textParentUndoUnit, UndoCloseAction undoCloseAction)
	{
		if (textParentUndoUnit != null)
		{
			UndoManager.GetUndoManager(TextContainer.Parent).Close(textParentUndoUnit, undoCloseAction);
		}
	}

	private void RaiseCompositionEvents(out int appSelectionAnchorOffset, out int appSelectionMovingOffset)
	{
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BRaiseCompositionEvents, CompositionEventList.Count);
		}
		appSelectionAnchorOffset = -1;
		appSelectionMovingOffset = -1;
		UndoManager undoManager = UndoManager.GetUndoManager(TextContainer.Parent);
		for (int i = 0; i < CompositionEventList.Count; i++)
		{
			CompositionEventRecord compositionEventRecord = CompositionEventList[i];
			FrameworkTextComposition frameworkTextComposition = CreateComposition(TextEditor, this);
			ITextPointer start = TextContainer.CreatePointerAtOffset(compositionEventRecord.StartOffsetBefore, LogicalDirection.Backward);
			ITextPointer textPointer = TextContainer.CreatePointerAtOffset(compositionEventRecord.EndOffsetBefore, LogicalDirection.Forward);
			bool flag = false;
			_handledByTextStoreListener = false;
			_compositionModifiedByEventListener = false;
			switch (compositionEventRecord.Stage)
			{
			case CompositionStage.StartComposition:
				frameworkTextComposition.Stage = TextCompositionStage.None;
				frameworkTextComposition.SetCompositionPositions(start, textPointer, compositionEventRecord.Text);
				undoManager.MinUndoStackCount = undoManager.UndoCount;
				try
				{
					if (IsTracing)
					{
						IMECompositionTracer.Trace(this, IMECompositionTraceOp.BStartCompositionEvent);
					}
					flag = TextCompositionManager.StartComposition(frameworkTextComposition);
					if (IsTracing)
					{
						IMECompositionTracer.Trace(this, IMECompositionTraceOp.EStartCompositionEvent);
					}
				}
				finally
				{
					undoManager.MinUndoStackCount = 0;
				}
				break;
			case CompositionStage.UpdateComposition:
				frameworkTextComposition.Stage = TextCompositionStage.Started;
				frameworkTextComposition.SetCompositionPositions(start, textPointer, compositionEventRecord.Text);
				undoManager.MinUndoStackCount = undoManager.UndoCount;
				try
				{
					if (IsCompositionRecordShifted(compositionEventRecord) && IsMaxLengthExceeded(frameworkTextComposition.CompositionText, compositionEventRecord.EndOffsetBefore - compositionEventRecord.StartOffsetBefore))
					{
						frameworkTextComposition.SetResultPositions(start, textPointer, compositionEventRecord.Text);
						if (IsTracing)
						{
							IMECompositionTracer.Trace(this, IMECompositionTraceOp.BCompleteCompositionEvent);
						}
						flag = TextCompositionManager.CompleteComposition(frameworkTextComposition);
						if (IsTracing)
						{
							IMECompositionTracer.Trace(this, IMECompositionTraceOp.ECompleteCompositionEvent);
						}
						_compositionModifiedByEventListener = true;
					}
					else if (!compositionEventRecord.IsShiftUpdate)
					{
						if (IsTracing)
						{
							IMECompositionTracer.Trace(this, IMECompositionTraceOp.BUpdateCompositionEvent);
						}
						flag = TextCompositionManager.UpdateComposition(frameworkTextComposition);
						if (IsTracing)
						{
							IMECompositionTracer.Trace(this, IMECompositionTraceOp.EUpdateCompositionEvent);
						}
					}
				}
				finally
				{
					undoManager.MinUndoStackCount = 0;
				}
				break;
			case CompositionStage.EndComposition:
				frameworkTextComposition.Stage = TextCompositionStage.Started;
				frameworkTextComposition.SetResultPositions(start, textPointer, compositionEventRecord.Text);
				undoManager.MinUndoStackCount = undoManager.UndoCount;
				try
				{
					_isEffectivelyNotComposing = true;
					if (IsTracing)
					{
						IMECompositionTracer.Trace(this, IMECompositionTraceOp.BCompleteCompositionEvent);
					}
					flag = TextCompositionManager.CompleteComposition(frameworkTextComposition);
					if (IsTracing)
					{
						IMECompositionTracer.Trace(this, IMECompositionTraceOp.ECompleteCompositionEvent);
					}
				}
				finally
				{
					undoManager.MinUndoStackCount = 0;
					_isEffectivelyNotComposing = false;
				}
				break;
			default:
				Invariant.Assert(condition: false, "Invalid composition stage!");
				break;
			}
			if ((compositionEventRecord.Stage == CompositionStage.EndComposition && !_handledByTextStoreListener) || (compositionEventRecord.Stage != CompositionStage.EndComposition && flag) || frameworkTextComposition.PendingComplete)
			{
				_compositionModifiedByEventListener = true;
			}
			if (_compositionModifiedByEventListener)
			{
				appSelectionAnchorOffset = TextSelection.AnchorPosition.Offset;
				appSelectionMovingOffset = TextSelection.MovingPosition.Offset;
				break;
			}
			if (compositionEventRecord.Stage != CompositionStage.EndComposition && !compositionEventRecord.IsShiftUpdate)
			{
				UpdateCompositionText(frameworkTextComposition);
			}
			if (compositionEventRecord.Stage == CompositionStage.EndComposition)
			{
				start = textPointer.GetFrozenPointer(LogicalDirection.Backward);
			}
			if (_compositionModifiedByEventListener)
			{
				appSelectionAnchorOffset = TextSelection.AnchorPosition.Offset;
				appSelectionMovingOffset = TextSelection.MovingPosition.Offset;
				break;
			}
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.ERaiseCompositionEvents);
		}
	}

	private bool IsMaxLengthExceeded(string textData, int charsToReplaceCount)
	{
		if (!TextEditor.AcceptsRichContent && TextEditor.MaxLength > 0)
		{
			int num = TextContainer.SymbolCount - charsToReplaceCount;
			int num2 = Math.Max(0, TextEditor.MaxLength - num);
			if (textData.Length > num2)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsCompositionRecordShifted(CompositionEventRecord record)
	{
		if ((0 <= record.StartOffsetBefore && 0 <= _previousCompositionStartOffset && record.StartOffsetBefore < _previousCompositionStartOffset) || record.IsShiftUpdate)
		{
			return true;
		}
		return false;
	}

	private void SetFinalDocumentState(UndoManager undoManager, Stack imeChangeStack, int appChangeCount, int imeSelectionAnchorOffset, int imeSelectionMovingOffset, int appSelectionAnchorOffset, int appSelectionMovingOffset)
	{
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.BSetFinalDocumentState);
		}
		TextSelection.BeginChangeNoUndo();
		int num = -1;
		try
		{
			bool compositionModifiedByEventListener = _compositionModifiedByEventListener;
			UndoQuietly(appChangeCount);
			Stack redoStack = undoManager.SetRedoStack(imeChangeStack);
			int count = imeChangeStack.Count;
			RedoQuietly(count);
			Invariant.Assert(_netCharCount == TextContainer.IMECharCount);
			_replayingIMEChangeReentrancyCount--;
			num = 0;
			if (compositionModifiedByEventListener)
			{
				int undoCount = undoManager.UndoCount;
				if (_isComposing)
				{
					TextParentUndoUnit textParentUndoUnit = OpenTextParentUndoUnit();
					try
					{
						CompleteComposition();
					}
					finally
					{
						CloseTextParentUndoUnit(textParentUndoUnit, (textParentUndoUnit.LastUnit == null) ? UndoCloseAction.Discard : UndoCloseAction.Commit);
					}
				}
				undoCount = undoManager.UndoCount - undoCount;
				_compositionEventState = CompositionEventState.ApplyingApplicationChange;
				try
				{
					UndoQuietly(undoCount);
					UndoQuietly(count);
					undoManager.SetRedoStack(redoStack);
					RedoQuietly(appChangeCount);
					Invariant.Assert(_netCharCount == TextContainer.IMECharCount);
					ITextPointer position = TextContainer.CreatePointerAtOffset(appSelectionAnchorOffset, LogicalDirection.Forward);
					ITextPointer position2 = TextContainer.CreatePointerAtOffset(appSelectionMovingOffset, LogicalDirection.Forward);
					TextSelection.Select(position, position2);
					MergeCompositionUndoUnits();
				}
				finally
				{
					_compositionEventState = CompositionEventState.RaisingEvents;
				}
			}
			else
			{
				ITextPointer position3 = TextContainer.CreatePointerAtOffset(imeSelectionAnchorOffset, LogicalDirection.Backward);
				ITextPointer position4 = TextContainer.CreatePointerAtOffset(imeSelectionMovingOffset, LogicalDirection.Backward);
				TextSelection.Select(position3, position4);
				MergeCompositionUndoUnits();
			}
		}
		finally
		{
			_replayingIMEChangeReentrancyCount += num;
			TextSelection.EndChange(disableScroll: false, skipEvents: true);
		}
		if (IsTracing)
		{
			IMECompositionTracer.Trace(this, IMECompositionTraceOp.ESetFinalDocumentState);
		}
	}

	private void UndoQuietly(int count)
	{
		if (count > 0)
		{
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.BUndoQuietly, count);
			}
			UndoManager undoManager = UndoManager.GetUndoManager(TextContainer.Parent);
			TextSelection.BeginChangeNoUndo();
			try
			{
				undoManager.Undo(count);
			}
			finally
			{
				TextSelection.EndChange(disableScroll: false, skipEvents: true);
			}
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.EUndoQuietly);
			}
		}
	}

	private void RedoQuietly(int count)
	{
		if (count > 0)
		{
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.BRedoQuietly, count);
			}
			UndoManager undoManager = UndoManager.GetUndoManager(TextContainer.Parent);
			TextSelection.BeginChangeNoUndo();
			try
			{
				undoManager.Redo(count);
			}
			finally
			{
				TextSelection.EndChange(disableScroll: false, skipEvents: true);
			}
			if (IsTracing)
			{
				IMECompositionTracer.Trace(this, IMECompositionTraceOp.ERedoQuietly);
			}
		}
	}

	private void MergeCompositionUndoUnits()
	{
		UndoManager undoManager = UndoManager.GetUndoManager(TextContainer.Parent);
		if (undoManager == null || !undoManager.IsEnabled)
		{
			return;
		}
		int num = undoManager.UndoCount - 1;
		int num2 = undoManager.UndoCount - 1;
		while (num >= 0 && undoManager.GetUndoUnit(num) is CompositionParentUndoUnit compositionParentUndoUnit && (!compositionParentUndoUnit.IsFirstCompositionUnit || !compositionParentUndoUnit.IsLastCompositionUnit))
		{
			if (!compositionParentUndoUnit.IsFirstCompositionUnit)
			{
				num--;
				continue;
			}
			int num3 = num2 - num;
			for (int i = num + 1; i <= num + num3; i++)
			{
				CompositionParentUndoUnit unit = (CompositionParentUndoUnit)undoManager.GetUndoUnit(i);
				compositionParentUndoUnit.MergeCompositionUnit(unit);
			}
			undoManager.RemoveUndoRange(num + 1, num3);
			num--;
			num2 = num;
		}
	}

	private CompositionParentUndoUnit PeekCompositionParentUndoUnit()
	{
		UndoManager undoManager = UndoManager.GetUndoManager(TextContainer.Parent);
		if (undoManager == null || !undoManager.IsEnabled)
		{
			return null;
		}
		return undoManager.PeekUndoStack() as CompositionParentUndoUnit;
	}
}
