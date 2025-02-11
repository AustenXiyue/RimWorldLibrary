using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Controls;
using MS.Utility;

namespace System.Windows.Controls;

/// <summary>Arranges and virtualizes content on a single line that is oriented either horizontally or vertically.</summary>
public class VirtualizingStackPanel : VirtualizingPanel, IScrollInfo, IStackMeasure
{
	[Flags]
	private enum BoolField : byte
	{
		HasVirtualizingChildren = 1,
		AlignTopOfBringIntoViewContainer = 2,
		WasLastMeasurePassAnchored = 4,
		ItemsChangedDuringMeasure = 8,
		IsScrollActive = 0x10,
		IgnoreMaxDesiredSize = 0x20,
		AlignBottomOfBringIntoViewContainer = 0x40,
		IsMeasureCachesPending = 0x80
	}

	private enum ScrollType
	{
		None,
		Relative,
		Absolute,
		ToEnd
	}

	private class ScrollData : IStackMeasureScrollData
	{
		internal bool _allowHorizontal;

		internal bool _allowVertical;

		internal Vector _offset;

		internal Vector _computedOffset = new Vector(0.0, 0.0);

		internal Size _viewport;

		internal Size _extent;

		internal ScrollViewer _scrollOwner;

		internal Size _maxDesiredSize;

		internal DependencyObject _bringIntoViewLeafContainer;

		internal FrameworkElement _firstContainerInViewport;

		internal double _firstContainerOffsetFromViewport;

		internal double _expectedDistanceBetweenViewports;

		internal long _scrollGeneration;

		internal bool IsEmpty
		{
			get
			{
				if (_offset.X == 0.0 && _offset.Y == 0.0 && _viewport.Width == 0.0 && _viewport.Height == 0.0 && _extent.Width == 0.0 && _extent.Height == 0.0 && _maxDesiredSize.Width == 0.0)
				{
					return _maxDesiredSize.Height == 0.0;
				}
				return false;
			}
		}

		public Vector Offset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
			}
		}

		public Size Viewport
		{
			get
			{
				return _viewport;
			}
			set
			{
				_viewport = value;
			}
		}

		public Size Extent
		{
			get
			{
				return _extent;
			}
			set
			{
				_extent = value;
			}
		}

		public Vector ComputedOffset
		{
			get
			{
				return _computedOffset;
			}
			set
			{
				_computedOffset = value;
			}
		}

		public ScrollType HorizontalScrollType { get; set; }

		public ScrollType VerticalScrollType { get; set; }

		internal void ClearLayout()
		{
			_offset = default(Vector);
			_viewport = (_extent = (_maxDesiredSize = default(Size)));
		}

		public void SetPhysicalViewport(double value)
		{
		}

		public void SetHorizontalScrollType(double oldOffset, double newOffset)
		{
			if (DoubleUtil.GreaterThanOrClose(newOffset, _extent.Width - _viewport.Width))
			{
				HorizontalScrollType = ScrollType.ToEnd;
			}
			else if (DoubleUtil.GreaterThan(Math.Abs(newOffset - oldOffset), _viewport.Width))
			{
				HorizontalScrollType = ScrollType.Absolute;
			}
			else if (HorizontalScrollType == ScrollType.None)
			{
				HorizontalScrollType = ScrollType.Relative;
			}
		}

		public void SetVerticalScrollType(double oldOffset, double newOffset)
		{
			if (DoubleUtil.GreaterThanOrClose(newOffset, _extent.Height - _viewport.Height))
			{
				VerticalScrollType = ScrollType.ToEnd;
			}
			else if (DoubleUtil.GreaterThan(Math.Abs(newOffset - oldOffset), _viewport.Height))
			{
				VerticalScrollType = ScrollType.Absolute;
			}
			else if (VerticalScrollType == ScrollType.None)
			{
				VerticalScrollType = ScrollType.Relative;
			}
		}
	}

	private class OffsetInformation
	{
		public List<double> previouslyMeasuredOffsets { get; set; }

		public double? lastPageSafeOffset { get; set; }

		public double? lastPagePixelSize { get; set; }
	}

	private class FirstContainerInformation
	{
		public Rect Viewport;

		public DependencyObject FirstContainer;

		public int FirstItemIndex;

		public double FirstItemOffset;

		public long ScrollGeneration;

		public FirstContainerInformation(ref Rect viewport, DependencyObject firstContainer, int firstItemIndex, double firstItemOffset, long scrollGeneration)
		{
			Viewport = viewport;
			FirstContainer = firstContainer;
			FirstItemIndex = firstItemIndex;
			FirstItemOffset = firstItemOffset;
			ScrollGeneration = scrollGeneration;
		}
	}

	private class ContainerSizeDual : Tuple<Size, Size>
	{
		public Size PixelSize => base.Item1;

		public Size ItemSize => base.Item2;

		public ContainerSizeDual(Size pixelSize, Size itemSize)
			: base(pixelSize, itemSize)
		{
		}
	}

	private class UniformOrAverageContainerSizeDual : Tuple<double, double>
	{
		public double PixelSize => base.Item1;

		public double ItemSize => base.Item2;

		public UniformOrAverageContainerSizeDual(double pixelSize, double itemSize)
			: base(pixelSize, itemSize)
		{
		}
	}

	private class EffectiveOffsetInformation
	{
		public long ScrollGeneration { get; private set; }

		public List<double> OffsetList { get; private set; }

		public EffectiveOffsetInformation(long scrollGeneration)
		{
			ScrollGeneration = scrollGeneration;
			OffsetList = new List<double>(2);
		}
	}

	private class ScrollTracer
	{
		private class TraceList
		{
			private List<ScrollTraceRecord> _traceList = new List<ScrollTraceRecord>();

			private BinaryWriter _writer;

			private int _flushIndex;

			internal TraceList(string filename)
			{
				if (filename != "none")
				{
					_writer = new BinaryWriter(File.Open(filename, FileMode.Create));
					_writer.Write(3);
				}
			}

			internal void Add(ScrollTraceRecord record)
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
					if (_flushIndex > 30000)
					{
						int count = _flushIndex - 5000;
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

		private const int s_StfFormatVersion = 3;

		private const int s_MaxTraceRecords = 30000;

		private const int s_MinTraceRecords = 5000;

		private const int s_DefaultLayoutUpdatedThreshold = 20;

		private static string _targetName;

		private static bool _isEnabled;

		private static string _fileName;

		private static int _flushDepth;

		private static int _luThreshold;

		private static ScrollTracingInfo _nullInfo;

		private static string[] s_format;

		private int _depth;

		private TraceList _traceList;

		private WeakReference<ItemsControl> _wrIC;

		private int _luCount = -1;

		private static List<Tuple<WeakReference<ItemsControl>, TraceList>> s_TargetToTraceListMap;

		private static int s_seqno;

		internal static bool IsEnabled => _isEnabled;

		static ScrollTracer()
		{
			_nullInfo = new ScrollTracingInfo(null, 0, -1, null, null, null, -1);
			s_format = new string[21]
			{
				"", "{0}", "{0} {1}", "{0} {1} {2}", "{0} {1} {2} {3}", "{0} {1} {2} {3} {4} ", "{0} {1} {2} {3} {4} {5}", "{0} {1} {2} {3} {4} {5} {6}", "{0} {1} {2} {3} {4} {5} {6} {7}", "{0} {1} {2} {3} {4} {5} {6} {7} {8}",
				"{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17}", "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18}",
				"{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18} {19}"
			};
			s_TargetToTraceListMap = new List<Tuple<WeakReference<ItemsControl>, TraceList>>();
			_targetName = FrameworkCompatibilityPreferences.GetScrollingTraceTarget();
			_flushDepth = 0;
			_luThreshold = 20;
			string scrollingTraceFile = FrameworkCompatibilityPreferences.GetScrollingTraceFile();
			if (!string.IsNullOrEmpty(scrollingTraceFile))
			{
				string[] array = scrollingTraceFile.Split(';');
				_fileName = array[0];
				if (array.Length > 1 && int.TryParse(array[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
				{
					_flushDepth = result;
				}
				if (array.Length > 2 && int.TryParse(array[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2))
				{
					_luThreshold = ((result2 <= 0) ? int.MaxValue : result2);
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
			ItemsControl itemsControl = o as ItemsControl;
			if (itemsControl != null || o == null)
			{
				lock (s_TargetToTraceListMap)
				{
					CloseAllTraceLists();
					if (itemsControl != null)
					{
						Enable();
						AddToMap(itemsControl);
						ScrollTracingInfo nullInfo = _nullInfo;
						int generation = nullInfo.Generation + 1;
						nullInfo.Generation = generation;
					}
				}
			}
			return itemsControl == o;
		}

		internal static void SetFileAndDepth(string filename, int flushDepth)
		{
			throw new NotSupportedException();
		}

		private static void Flush()
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

		private static void Mark(params object[] args)
		{
			ScrollTraceRecord record = new ScrollTraceRecord(ScrollTraceOp.Mark, null, -1, 0, 0, BuildDetail(args));
			lock (s_TargetToTraceListMap)
			{
				int i = 0;
				for (int count = s_TargetToTraceListMap.Count; i < count; i++)
				{
					s_TargetToTraceListMap[i].Item2.Add(record);
				}
			}
		}

		internal static bool IsConfigured(VirtualizingStackPanel vsp)
		{
			return ScrollTracingInfoField.GetValue(vsp) != null;
		}

		internal static void ConfigureTracing(VirtualizingStackPanel vsp, DependencyObject itemsOwner, object parentItem, ItemsControl itemsControl)
		{
			ScrollTracer scrollTracer = null;
			ScrollTracingInfo value = _nullInfo;
			ScrollTracingInfo scrollTracingInfo = ScrollTracingInfoField.GetValue(vsp);
			if (scrollTracingInfo != null && scrollTracingInfo.Generation < _nullInfo.Generation)
			{
				scrollTracingInfo = null;
			}
			if (parentItem == vsp)
			{
				if (scrollTracingInfo == null)
				{
					if (itemsOwner == itemsControl)
					{
						TraceList traceList = TraceListForItemsControl(itemsControl);
						if (traceList != null)
						{
							scrollTracer = new ScrollTracer(itemsControl, vsp, traceList);
						}
					}
					if (scrollTracer != null)
					{
						value = new ScrollTracingInfo(scrollTracer, _nullInfo.Generation, 0, itemsOwner as FrameworkElement, null, null, 0);
					}
				}
			}
			else if (VisualTreeHelper.GetParent(itemsOwner) is VirtualizingStackPanel virtualizingStackPanel)
			{
				ScrollTracingInfo value2 = ScrollTracingInfoField.GetValue(virtualizingStackPanel);
				if (value2 != null)
				{
					scrollTracer = value2.ScrollTracer;
					if (scrollTracer != null)
					{
						int num = ((virtualizingStackPanel.ItemContainerGenerator is ItemContainerGenerator itemContainerGenerator) ? itemContainerGenerator.IndexFromContainer(itemsOwner, returnLocalIndex: true) : (-1));
						if (scrollTracingInfo == null)
						{
							value = new ScrollTracingInfo(scrollTracer, _nullInfo.Generation, value2.Depth + 1, itemsOwner as FrameworkElement, virtualizingStackPanel, parentItem, num);
						}
						else if (object.Equals(parentItem, scrollTracingInfo.ParentItem))
						{
							if (num != scrollTracingInfo.ItemIndex)
							{
								Trace(vsp, ScrollTraceOp.ID, "Index changed from ", scrollTracingInfo.ItemIndex, " to ", num);
								scrollTracingInfo.ChangeIndex(num);
							}
						}
						else
						{
							Trace(vsp, ScrollTraceOp.ID, "Container recyled from ", scrollTracingInfo.ItemIndex, " to ", num);
							scrollTracingInfo.ChangeItem(parentItem);
							scrollTracingInfo.ChangeIndex(num);
						}
					}
				}
			}
			if (scrollTracingInfo == null)
			{
				ScrollTracingInfoField.SetValue(vsp, value);
			}
		}

		internal static bool IsTracing(VirtualizingStackPanel vsp)
		{
			ScrollTracingInfo value = ScrollTracingInfoField.GetValue(vsp);
			if (value != null)
			{
				return value.ScrollTracer != null;
			}
			return false;
		}

		internal static void Trace(VirtualizingStackPanel vsp, ScrollTraceOp op, params object[] args)
		{
			ScrollTracingInfo value = ScrollTracingInfoField.GetValue(vsp);
			ScrollTracer scrollTracer = value.ScrollTracer;
			if (!ShouldIgnore(op, value))
			{
				scrollTracer.AddTrace(vsp, op, value, args);
			}
		}

		private static bool ShouldIgnore(ScrollTraceOp op, ScrollTracingInfo sti)
		{
			return op == ScrollTraceOp.NoOp;
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
				ReadOnlySpan<char> readOnlySpan = type.ToString();
				flag2 = readOnlySpan.StartsWith("System.Windows.Controls.");
				if (flag2)
				{
					readOnlySpan = readOnlySpan.Slice(24);
				}
				stringBuilder.Append(readOnlySpan);
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

		private void Push()
		{
			_depth++;
		}

		private void Pop()
		{
			_depth--;
		}

		private void Pop(ScrollTraceRecord record)
		{
			_depth--;
			record.ChangeOpDepth(-1);
		}

		private ScrollTracer(ItemsControl itemsControl, VirtualizingStackPanel vsp, TraceList traceList)
		{
			_wrIC = new WeakReference<ItemsControl>(itemsControl);
			_traceList = traceList;
			IdentifyTrace(itemsControl, vsp);
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

		private void IdentifyTrace(ItemsControl ic, VirtualizingStackPanel vsp)
		{
			AddTrace(null, ScrollTraceOp.ID, _nullInfo, DisplayType(ic), "Items:", ic.Items.Count, "Panel:", DisplayType(vsp), "Time:", DateTime.Now);
			AddTrace(null, ScrollTraceOp.ID, _nullInfo, "IsVirt:", VirtualizingPanel.GetIsVirtualizing(ic), "IsVirtWhenGroup:", VirtualizingPanel.GetIsVirtualizingWhenGrouping(ic), "VirtMode:", VirtualizingPanel.GetVirtualizationMode(ic), "ScrollUnit:", VirtualizingPanel.GetScrollUnit(ic), "CacheLen:", VirtualizingPanel.GetCacheLength(ic), VirtualizingPanel.GetCacheLengthUnit(ic));
			DpiScale dpi = vsp.GetDpi();
			AddTrace(null, ScrollTraceOp.ID, _nullInfo, "DPIScale:", dpi.DpiScaleX, dpi.DpiScaleY, "UseLayoutRounding:", vsp.UseLayoutRounding, "Rounding Quantum:", 1.0 / dpi.DpiScaleY);
			AddTrace(null, ScrollTraceOp.ID, _nullInfo, "CanContentScroll:", ScrollViewer.GetCanContentScroll(ic), "IsDeferredScrolling:", ScrollViewer.GetIsDeferredScrollingEnabled(ic), "PanningMode:", ScrollViewer.GetPanningMode(ic), "HSBVisibility:", ScrollViewer.GetHorizontalScrollBarVisibility(ic), "VSBVisibility:", ScrollViewer.GetVerticalScrollBarVisibility(ic));
			if (ic is DataGrid dataGrid)
			{
				AddTrace(null, ScrollTraceOp.ID, _nullInfo, "EnableRowVirt:", dataGrid.EnableRowVirtualization, "EnableColVirt:", dataGrid.EnableColumnVirtualization, "Columns:", dataGrid.Columns.Count, "FrozenCols:", dataGrid.FrozenColumnCount);
			}
		}

		private void AddTrace(VirtualizingStackPanel vsp, ScrollTraceOp op, ScrollTracingInfo sti, params object[] args)
		{
			if (op == ScrollTraceOp.LayoutUpdated)
			{
				if (++_luCount > _luThreshold)
				{
					AddTrace(null, ScrollTraceOp.ID, _nullInfo, "Inactive at", DateTime.Now);
					if (_wrIC.TryGetTarget(out var target))
					{
						target.LayoutUpdated -= OnLayoutUpdated;
					}
					_traceList.FlushAndClear();
					_luCount = -1;
				}
			}
			else
			{
				int luCount = _luCount;
				_luCount = 0;
				if (luCount < 0)
				{
					AddTrace(null, ScrollTraceOp.ID, _nullInfo, "Reactivate at", DateTime.Now);
					if (_wrIC.TryGetTarget(out var target2))
					{
						target2.LayoutUpdated += OnLayoutUpdated;
					}
				}
			}
			ScrollTraceRecord scrollTraceRecord = new ScrollTraceRecord(op, vsp, sti.Depth, sti.ItemIndex, _depth, BuildDetail(args));
			_traceList.Add(scrollTraceRecord);
			switch (op)
			{
			case ScrollTraceOp.BeginMeasure:
				Push();
				break;
			case ScrollTraceOp.EndMeasure:
				Pop(scrollTraceRecord);
				scrollTraceRecord.Snapshot = vsp.TakeSnapshot();
				_traceList.Flush(sti.Depth);
				break;
			case ScrollTraceOp.BeginArrange:
				Push();
				break;
			case ScrollTraceOp.EndArrange:
				Pop(scrollTraceRecord);
				scrollTraceRecord.Snapshot = vsp.TakeSnapshot();
				_traceList.Flush(sti.Depth);
				break;
			case ScrollTraceOp.BSetAnchor:
				Push();
				break;
			case ScrollTraceOp.ESetAnchor:
				Pop(scrollTraceRecord);
				break;
			case ScrollTraceOp.BOnAnchor:
				Push();
				break;
			case ScrollTraceOp.ROnAnchor:
				Pop(scrollTraceRecord);
				break;
			case ScrollTraceOp.SOnAnchor:
				Pop();
				break;
			case ScrollTraceOp.EOnAnchor:
				Pop(scrollTraceRecord);
				break;
			case ScrollTraceOp.RecycleChildren:
			case ScrollTraceOp.RemoveChildren:
				scrollTraceRecord.RevirtualizedChildren = args[2] as List<string>;
				break;
			}
			if (_flushDepth < 0)
			{
				_traceList.Flush(_flushDepth);
			}
		}

		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			AddTrace(null, ScrollTraceOp.LayoutUpdated, _nullInfo, null);
		}

		private static TraceList TraceListForItemsControl(ItemsControl target)
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

		private static TraceList AddToMap(ItemsControl target)
		{
			TraceList traceList = null;
			lock (s_TargetToTraceListMap)
			{
				PurgeMap();
				s_seqno++;
				string text = _fileName;
				if (string.IsNullOrEmpty(text) || text == "default")
				{
					text = "ScrollTrace.stf";
				}
				if (text != "none" && s_seqno > 1)
				{
					int num = text.LastIndexOf('.');
					if (num < 0)
					{
						num = text.Length;
					}
					text = $"{text.AsSpan(0, num)}{s_seqno}{text.AsSpan(num)}";
				}
				traceList = new TraceList(text);
				s_TargetToTraceListMap.Add(new Tuple<WeakReference<ItemsControl>, TraceList>(new WeakReference<ItemsControl>(target), traceList));
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

	private class ScrollTracingInfo
	{
		internal ScrollTracer ScrollTracer { get; private set; }

		internal int Generation { get; set; }

		internal int Depth { get; private set; }

		internal FrameworkElement Owner { get; private set; }

		internal VirtualizingStackPanel Parent { get; private set; }

		internal object ParentItem { get; private set; }

		internal int ItemIndex { get; private set; }

		internal ScrollTracingInfo(ScrollTracer tracer, int generation, int depth, FrameworkElement owner, VirtualizingStackPanel parent, object parentItem, int itemIndex)
		{
			ScrollTracer = tracer;
			Generation = generation;
			Depth = depth;
			Owner = owner;
			Parent = parent;
			ParentItem = parentItem;
			ItemIndex = itemIndex;
		}

		internal void ChangeItem(object newItem)
		{
			ParentItem = newItem;
		}

		internal void ChangeIndex(int newIndex)
		{
			ItemIndex = newIndex;
		}
	}

	private enum ScrollTraceOp : ushort
	{
		NoOp,
		ID,
		Mark,
		LineUp,
		LineDown,
		LineLeft,
		LineRight,
		PageUp,
		PageDown,
		PageLeft,
		PageRight,
		MouseWheelUp,
		MouseWheelDown,
		MouseWheelLeft,
		MouseWheelRight,
		SetHorizontalOffset,
		SetVerticalOffset,
		SetHOff,
		SetVOff,
		MakeVisible,
		BeginMeasure,
		EndMeasure,
		BeginArrange,
		EndArrange,
		LayoutUpdated,
		BSetAnchor,
		ESetAnchor,
		BOnAnchor,
		ROnAnchor,
		SOnAnchor,
		EOnAnchor,
		RecycleChildren,
		RemoveChildren,
		ItemsChanged,
		IsScrollActive,
		CFCIV,
		CFIVIO,
		SyncAveSize,
		StoreSubstOffset,
		UseSubstOffset,
		ReviseArrangeOffset,
		SVSDBegin,
		AdjustOffset,
		ScrollBarChangeVisibility,
		RemeasureCycle,
		RemeasureEndExpandViewport,
		RemeasureEndChangeOffset,
		RemeasureEndExtentChanged,
		RemeasureRatio,
		RecomputeFirstOffset,
		LastPageSizeChange,
		SVSDEnd,
		SetContainerSize,
		SizeChangeDuringAnchorScroll,
		UpdateExtent
	}

	private class ScrollTraceRecord
	{
		private object _extraData;

		internal ScrollTraceOp Op { get; private set; }

		internal int OpDepth { get; private set; }

		internal VirtualizingStackPanel VSP { get; private set; }

		internal int VDepth { get; private set; }

		internal int ItemIndex { get; private set; }

		internal string Detail { get; set; }

		internal Snapshot Snapshot
		{
			get
			{
				return _extraData as Snapshot;
			}
			set
			{
				_extraData = value;
			}
		}

		internal List<string> RevirtualizedChildren
		{
			get
			{
				return _extraData as List<string>;
			}
			set
			{
				_extraData = value;
			}
		}

		internal ScrollTraceRecord(ScrollTraceOp op, VirtualizingStackPanel vsp, int vspDepth, int itemIndex, int opDepth, string detail)
		{
			Op = op;
			VSP = vsp;
			VDepth = vspDepth;
			ItemIndex = itemIndex;
			OpDepth = opDepth;
			Detail = detail;
		}

		internal void ChangeOpDepth(int delta)
		{
			OpDepth += delta;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3} {4}", OpDepth, VDepth, ItemIndex, Op, Detail);
		}

		internal void Write(BinaryWriter writer)
		{
			writer.Write((ushort)Op);
			writer.Write(OpDepth);
			writer.Write(VDepth);
			writer.Write(ItemIndex);
			writer.Write(Detail);
			List<string> revirtualizedChildren;
			if (Snapshot != null)
			{
				writer.Write((byte)1);
				Snapshot.Write(writer, VSP);
			}
			else if ((revirtualizedChildren = RevirtualizedChildren) != null)
			{
				int count = revirtualizedChildren.Count;
				writer.Write((byte)2);
				writer.Write(count);
				for (int i = 0; i < count; i++)
				{
					writer.Write(revirtualizedChildren[i]);
				}
			}
			else
			{
				writer.Write((byte)0);
			}
		}
	}

	private class Snapshot
	{
		internal ScrollData _scrollData;

		internal BoolField _boolFieldStore;

		internal bool? _areContainersUniformlySized;

		internal double? _uniformOrAverageContainerSize;

		internal double? _uniformOrAverageContainerPixelSize;

		internal List<ChildInfo> _realizedChildren;

		internal int _firstItemInExtendedViewportChildIndex;

		internal int _firstItemInExtendedViewportIndex;

		internal double _firstItemInExtendedViewportOffset;

		internal int _actualItemsInExtendedViewportCount;

		internal Rect _viewport;

		internal int _itemsInExtendedViewportCount;

		internal Rect _extendedViewport;

		internal Size _previousStackPixelSizeInViewport;

		internal Size _previousStackLogicalSizeInViewport;

		internal Size _previousStackPixelSizeInCacheBeforeViewport;

		internal FrameworkElement _firstContainerInViewport;

		internal double _firstContainerOffsetFromViewport;

		internal double _expectedDistanceBetweenViewports;

		internal DependencyObject _bringIntoViewContainer;

		internal DependencyObject _bringIntoViewLeafContainer;

		internal List<double> _effectiveOffsets;

		internal void Write(BinaryWriter writer, VirtualizingStackPanel vsp)
		{
			if (_scrollData == null)
			{
				writer.Write(value: false);
			}
			else
			{
				writer.Write(value: true);
				WriteVector(writer, ref _scrollData._offset);
				WriteSize(writer, ref _scrollData._extent);
				WriteVector(writer, ref _scrollData._computedOffset);
			}
			writer.Write((byte)_boolFieldStore);
			writer.Write(_areContainersUniformlySized != false);
			writer.Write(_uniformOrAverageContainerSize.HasValue ? _uniformOrAverageContainerSize.Value : (-1.0));
			writer.Write(_uniformOrAverageContainerPixelSize.HasValue ? _uniformOrAverageContainerPixelSize.Value : (-1.0));
			writer.Write(_firstItemInExtendedViewportChildIndex);
			writer.Write(_firstItemInExtendedViewportIndex);
			writer.Write(_firstItemInExtendedViewportOffset);
			writer.Write(_actualItemsInExtendedViewportCount);
			WriteRect(writer, ref _viewport);
			writer.Write(_itemsInExtendedViewportCount);
			WriteRect(writer, ref _extendedViewport);
			WriteSize(writer, ref _previousStackPixelSizeInViewport);
			WriteSize(writer, ref _previousStackLogicalSizeInViewport);
			WriteSize(writer, ref _previousStackPixelSizeInCacheBeforeViewport);
			writer.Write(vsp.ContainerPath(_firstContainerInViewport));
			writer.Write(_firstContainerOffsetFromViewport);
			writer.Write(_expectedDistanceBetweenViewports);
			writer.Write(vsp.ContainerPath(_bringIntoViewContainer));
			writer.Write(vsp.ContainerPath(_bringIntoViewLeafContainer));
			writer.Write(_realizedChildren.Count);
			for (int i = 0; i < _realizedChildren.Count; i++)
			{
				ChildInfo childInfo = _realizedChildren[i];
				writer.Write(childInfo._itemIndex);
				WriteSize(writer, ref childInfo._desiredSize);
				WriteRect(writer, ref childInfo._arrangeRect);
				WriteThickness(writer, ref childInfo._inset);
			}
			if (_effectiveOffsets != null)
			{
				writer.Write(_effectiveOffsets.Count);
				{
					foreach (double effectiveOffset in _effectiveOffsets)
					{
						writer.Write(effectiveOffset);
					}
					return;
				}
			}
			writer.Write(0);
		}

		private static void WriteRect(BinaryWriter writer, ref Rect rect)
		{
			writer.Write(rect.Left);
			writer.Write(rect.Top);
			writer.Write(rect.Width);
			writer.Write(rect.Height);
		}

		private static void WriteSize(BinaryWriter writer, ref Size size)
		{
			writer.Write(size.Width);
			writer.Write(size.Height);
		}

		private static void WriteVector(BinaryWriter writer, ref Vector vector)
		{
			writer.Write(vector.X);
			writer.Write(vector.Y);
		}

		private static void WriteThickness(BinaryWriter writer, ref Thickness thickness)
		{
			writer.Write(thickness.Left);
			writer.Write(thickness.Top);
			writer.Write(thickness.Right);
			writer.Write(thickness.Bottom);
		}
	}

	private class ChildInfo
	{
		internal int _itemIndex;

		internal Size _desiredSize;

		internal Rect _arrangeRect;

		internal Thickness _inset;

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} ds: {1} ar: {2} in: {3}", _itemIndex, _desiredSize, _arrangeRect, _inset);
		}
	}

	private class SnapshotData
	{
		internal double UniformOrAverageContainerSize { get; set; }

		internal double UniformOrAverageContainerPixelSize { get; set; }

		internal List<double> EffectiveOffsets { get; set; }
	}

	private static readonly DependencyProperty ContainerSizeProperty;

	private static readonly DependencyProperty ContainerSizeDualProperty;

	private static readonly DependencyProperty AreContainersUniformlySizedProperty;

	private static readonly DependencyProperty UniformOrAverageContainerSizeProperty;

	private static readonly DependencyProperty UniformOrAverageContainerSizeDualProperty;

	internal static readonly DependencyProperty ItemsHostInsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.VirtualizingStackPanel.IsVirtualizing" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.VirtualizingStackPanel.IsVirtualizing" /> attached property.</returns>
	public new static readonly DependencyProperty IsVirtualizingProperty;

	/// <summary>Identifies the VirtualizingStackPanel.VirtualizationMode attached property.</summary>
	/// <returns>The identifier for the VirtualizingStackPanel.VirtualizationMode attached property.</returns>
	public new static readonly DependencyProperty VirtualizationModeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.VirtualizingStackPanel.Orientation" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.VirtualizingStackPanel.Orientation" /> dependency property.</returns>
	public static readonly DependencyProperty OrientationProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.VirtualizingStackPanel.CleanUpVirtualizedItem" /> attached event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.VirtualizingStackPanel.CleanUpVirtualizedItem" /> attached event.</returns>
	public static readonly RoutedEvent CleanUpVirtualizedItemEvent;

	private BoolField _boolFieldStore;

	private ScrollData _scrollData;

	private int _firstItemInExtendedViewportChildIndex;

	private int _firstItemInExtendedViewportIndex;

	private double _firstItemInExtendedViewportOffset;

	private int _actualItemsInExtendedViewportCount;

	private Rect _viewport;

	private int _itemsInExtendedViewportCount;

	private Rect _extendedViewport;

	private Size _previousStackPixelSizeInViewport;

	private Size _previousStackLogicalSizeInViewport;

	private Size _previousStackPixelSizeInCacheBeforeViewport;

	private double _pixelDistanceToFirstContainerInExtendedViewport;

	private double _pixelDistanceToViewport;

	private List<UIElement> _realizedChildren;

	private DispatcherOperation _cleanupOperation;

	private DispatcherTimer _cleanupDelay;

	private const int FocusTrail = 5;

	private DependencyObject _bringIntoViewContainer;

	private static int[] _indicesStoredInItemValueStorage;

	private static readonly UncommonField<DispatcherOperation> MeasureCachesOperationField;

	private static readonly UncommonField<DispatcherOperation> AnchorOperationField;

	private static readonly UncommonField<DispatcherOperation> AnchoredInvalidateMeasureOperationField;

	private static readonly UncommonField<DispatcherOperation> ClearIsScrollActiveOperationField;

	private static readonly UncommonField<OffsetInformation> OffsetInformationField;

	private static readonly UncommonField<EffectiveOffsetInformation> EffectiveOffsetInformationField;

	private static readonly UncommonField<SnapshotData> SnapshotDataField;

	private static UncommonField<FirstContainerInformation> FirstContainerInformationField;

	private static readonly UncommonField<ScrollTracingInfo> ScrollTracingInfoField;

	/// <summary>Gets or sets a value that describes the horizontal or vertical orientation of stacked content.  </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.Orientation" /> of child content. The default is <see cref="F:System.Windows.Controls.Orientation.Vertical" />.</returns>
	public Orientation Orientation
	{
		get
		{
			return (Orientation)GetValue(OrientationProperty);
		}
		set
		{
			SetValue(OrientationProperty, value);
		}
	}

	/// <summary>Gets a value that indicates if this <see cref="T:System.Windows.Controls.VirtualizingStackPanel" /> has a vertical or horizontal orientation.</summary>
	/// <returns>This property always returns true.</returns>
	protected internal override bool HasLogicalOrientation => true;

	/// <summary>Gets a value that represents the <see cref="T:System.Windows.Controls.Orientation" /> of the <see cref="T:System.Windows.Controls.VirtualizingStackPanel" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Controls.Orientation" /> value.</returns>
	protected internal override Orientation LogicalOrientation => Orientation;

	/// <summary>Gets or sets a value that indicates whether a <see cref="T:System.Windows.Controls.VirtualizingStackPanel" /> can scroll in the horizontal dimension.</summary>
	/// <returns>true if content can scroll in the horizontal dimension; otherwise, false. The default is false.</returns>
	[DefaultValue(false)]
	public bool CanHorizontallyScroll
	{
		get
		{
			if (_scrollData == null)
			{
				return false;
			}
			return _scrollData._allowHorizontal;
		}
		set
		{
			EnsureScrollData();
			if (_scrollData._allowHorizontal != value)
			{
				_scrollData._allowHorizontal = value;
				InvalidateMeasure();
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether content can scroll in the vertical dimension.</summary>
	/// <returns>true if content can scroll in the vertical dimension; otherwise, false. The default is false.</returns>
	[DefaultValue(false)]
	public bool CanVerticallyScroll
	{
		get
		{
			if (_scrollData == null)
			{
				return false;
			}
			return _scrollData._allowVertical;
		}
		set
		{
			EnsureScrollData();
			if (_scrollData._allowVertical != value)
			{
				_scrollData._allowVertical = value;
				InvalidateMeasure();
			}
		}
	}

	/// <summary>Gets a value that contains the horizontal size of the extent.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the horizontal size of the extent. The default is 0.0.</returns>
	public double ExtentWidth
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData._extent.Width;
		}
	}

	/// <summary>Gets a value that contains the vertical size of the extent. </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the vertical size of the extent. The default is 0.0.</returns>
	public double ExtentHeight
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData._extent.Height;
		}
	}

	/// <summary>Gets a value that contains the horizontal size of the viewport of the content.</summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the vertical size of the viewport of the content. The default is 0.0.</returns>
	public double ViewportWidth
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData._viewport.Width;
		}
	}

	/// <summary>Gets a value that contains the vertical size of the viewport of the content. </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the vertical size of the viewport of the content. The default is 0.0.</returns>
	public double ViewportHeight
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData._viewport.Height;
		}
	}

	/// <summary>Gets a value that contains the horizontal offset of the scrolled content. </summary>
	/// <returns>
	///   <see cref="T:System.Double" /> that represents the horizontal offset of the scrolled content. The default is 0.0.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public double HorizontalOffset
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData._computedOffset.X;
		}
	}

	/// <summary>Gets a value that contains the vertical offset of the scrolled content. </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the vertical offset of the scrolled content. The default is 0.0.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public double VerticalOffset
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData._computedOffset.Y;
		}
	}

	/// <summary>Gets or sets a value that identifies the container that controls scrolling behavior in this <see cref="T:System.Windows.Controls.VirtualizingStackPanel" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ScrollViewer" /> that owns scrolling for this <see cref="T:System.Windows.Controls.VirtualizingStackPanel" />. </returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ScrollViewer ScrollOwner
	{
		get
		{
			if (_scrollData == null)
			{
				return null;
			}
			return _scrollData._scrollOwner;
		}
		set
		{
			EnsureScrollData();
			if (value != _scrollData._scrollOwner)
			{
				ResetScrolling(this);
				_scrollData._scrollOwner = value;
			}
		}
	}

	/// <summary>Gets value that indicates whether the <see cref="T:System.Windows.Controls.VirtualizingStackPanel" /> can virtualize items that are grouped or organized in a hierarchy.</summary>
	/// <returns>true in all cases.</returns>
	protected override bool CanHierarchicallyScrollAndVirtualizeCore => true;

	private int MinDesiredGenerated => Math.Max(0, _firstItemInExtendedViewportIndex);

	private int MaxDesiredGenerated => Math.Min(ItemCount, _firstItemInExtendedViewportIndex + _actualItemsInExtendedViewportCount);

	private int ItemCount
	{
		get
		{
			EnsureGenerator();
			return ((ItemContainerGenerator)base.Generator).ItemsInternal.Count;
		}
	}

	private bool HasMeasured
	{
		get
		{
			return base.VSP_HasMeasured;
		}
		set
		{
			base.VSP_HasMeasured = value;
		}
	}

	private bool InRecyclingMode
	{
		get
		{
			return base.VSP_InRecyclingMode;
		}
		set
		{
			base.VSP_InRecyclingMode = value;
		}
	}

	internal bool IsScrolling
	{
		get
		{
			if (_scrollData != null)
			{
				return _scrollData._scrollOwner != null;
			}
			return false;
		}
	}

	internal bool IsPixelBased
	{
		get
		{
			return base.VSP_IsPixelBased;
		}
		set
		{
			base.VSP_IsPixelBased = value;
		}
	}

	internal bool MustDisableVirtualization
	{
		get
		{
			return base.VSP_MustDisableVirtualization;
		}
		set
		{
			base.VSP_MustDisableVirtualization = value;
		}
	}

	internal bool MeasureCaches
	{
		get
		{
			if (!base.VSP_MeasureCaches)
			{
				return !IsVirtualizing;
			}
			return true;
		}
		set
		{
			base.VSP_MeasureCaches = value;
		}
	}

	private bool IsVirtualizing
	{
		get
		{
			return base.VSP_IsVirtualizing;
		}
		set
		{
			if (!(base.IsItemsHost && value))
			{
				_realizedChildren = null;
			}
			base.VSP_IsVirtualizing = value;
		}
	}

	private bool HasVirtualizingChildren
	{
		get
		{
			return GetBoolField(BoolField.HasVirtualizingChildren);
		}
		set
		{
			SetBoolField(BoolField.HasVirtualizingChildren, value);
		}
	}

	private bool AlignTopOfBringIntoViewContainer
	{
		get
		{
			return GetBoolField(BoolField.AlignTopOfBringIntoViewContainer);
		}
		set
		{
			SetBoolField(BoolField.AlignTopOfBringIntoViewContainer, value);
		}
	}

	private bool AlignBottomOfBringIntoViewContainer
	{
		get
		{
			return GetBoolField(BoolField.AlignBottomOfBringIntoViewContainer);
		}
		set
		{
			SetBoolField(BoolField.AlignBottomOfBringIntoViewContainer, value);
		}
	}

	private bool WasLastMeasurePassAnchored
	{
		get
		{
			return GetBoolField(BoolField.WasLastMeasurePassAnchored);
		}
		set
		{
			SetBoolField(BoolField.WasLastMeasurePassAnchored, value);
		}
	}

	private bool ItemsChangedDuringMeasure
	{
		get
		{
			return GetBoolField(BoolField.ItemsChangedDuringMeasure);
		}
		set
		{
			SetBoolField(BoolField.ItemsChangedDuringMeasure, value);
		}
	}

	private bool IsScrollActive
	{
		get
		{
			return GetBoolField(BoolField.IsScrollActive);
		}
		set
		{
			if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
			{
				bool boolField = GetBoolField(BoolField.IsScrollActive);
				if (value != boolField)
				{
					ScrollTracer.Trace(this, ScrollTraceOp.IsScrollActive, value);
				}
			}
			SetBoolField(BoolField.IsScrollActive, value);
			if (!value)
			{
				_scrollData.HorizontalScrollType = ScrollType.None;
				_scrollData.VerticalScrollType = ScrollType.None;
			}
		}
	}

	internal bool IgnoreMaxDesiredSize
	{
		get
		{
			return GetBoolField(BoolField.IgnoreMaxDesiredSize);
		}
		set
		{
			SetBoolField(BoolField.IgnoreMaxDesiredSize, value);
		}
	}

	private bool IsMeasureCachesPending
	{
		get
		{
			return GetBoolField(BoolField.IsMeasureCachesPending);
		}
		set
		{
			SetBoolField(BoolField.IsMeasureCachesPending, value);
		}
	}

	private bool? AreContainersUniformlySized { get; set; }

	private double? UniformOrAverageContainerSize { get; set; }

	private double? UniformOrAverageContainerPixelSize { get; set; }

	private IList RealizedChildren
	{
		get
		{
			if (IsVirtualizing && InRecyclingMode)
			{
				EnsureRealizedChildren();
				return _realizedChildren;
			}
			return base.InternalChildren;
		}
	}

	internal static bool IsVSP45Compat => FrameworkCompatibilityPreferences.GetVSP45Compat();

	bool IStackMeasure.IsScrolling => IsScrolling;

	UIElementCollection IStackMeasure.InternalChildren => base.InternalChildren;

	private DependencyObject BringIntoViewLeafContainer => _scrollData?._bringIntoViewLeafContainer ?? null;

	private FrameworkElement FirstContainerInViewport => _scrollData?._firstContainerInViewport ?? null;

	private double FirstContainerOffsetFromViewport => _scrollData?._firstContainerOffsetFromViewport ?? 0.0;

	private double ExpectedDistanceBetweenViewports => _scrollData?._expectedDistanceBetweenViewports ?? 0.0;

	private bool CanMouseWheelVerticallyScroll => SystemParameters.WheelScrollLines > 0;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.VirtualizingStackPanel" /> class.</summary>
	public VirtualizingStackPanel()
	{
		base.IsVisibleChanged += OnIsVisibleChanged;
	}

	static VirtualizingStackPanel()
	{
		ContainerSizeProperty = DependencyProperty.Register("ContainerSize", typeof(Size), typeof(VirtualizingStackPanel));
		ContainerSizeDualProperty = DependencyProperty.Register("ContainerSizeDual", typeof(ContainerSizeDual), typeof(VirtualizingStackPanel));
		AreContainersUniformlySizedProperty = DependencyProperty.Register("AreContainersUniformlySized", typeof(bool), typeof(VirtualizingStackPanel));
		UniformOrAverageContainerSizeProperty = DependencyProperty.Register("UniformOrAverageContainerSize", typeof(double), typeof(VirtualizingStackPanel));
		UniformOrAverageContainerSizeDualProperty = DependencyProperty.Register("UniformOrAverageContainerSizeDual", typeof(UniformOrAverageContainerSizeDual), typeof(VirtualizingStackPanel));
		ItemsHostInsetProperty = DependencyProperty.Register("ItemsHostInset", typeof(Thickness), typeof(VirtualizingStackPanel));
		IsVirtualizingProperty = VirtualizingPanel.IsVirtualizingProperty;
		VirtualizationModeProperty = VirtualizingPanel.VirtualizationModeProperty;
		OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(VirtualizingStackPanel), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationChanged), ScrollBar.IsValidOrientation);
		CleanUpVirtualizedItemEvent = EventManager.RegisterRoutedEvent("CleanUpVirtualizedItemEvent", RoutingStrategy.Direct, typeof(CleanUpVirtualizedItemEventHandler), typeof(VirtualizingStackPanel));
		MeasureCachesOperationField = new UncommonField<DispatcherOperation>();
		AnchorOperationField = new UncommonField<DispatcherOperation>();
		AnchoredInvalidateMeasureOperationField = new UncommonField<DispatcherOperation>();
		ClearIsScrollActiveOperationField = new UncommonField<DispatcherOperation>();
		OffsetInformationField = new UncommonField<OffsetInformation>();
		EffectiveOffsetInformationField = new UncommonField<EffectiveOffsetInformation>();
		SnapshotDataField = new UncommonField<SnapshotData>();
		FirstContainerInformationField = new UncommonField<FirstContainerInformation>();
		ScrollTracingInfoField = new UncommonField<ScrollTracingInfo>();
		lock (DependencyProperty.Synchronized)
		{
			_indicesStoredInItemValueStorage = new int[6] { ContainerSizeProperty.GlobalIndex, ContainerSizeDualProperty.GlobalIndex, AreContainersUniformlySizedProperty.GlobalIndex, UniformOrAverageContainerSizeProperty.GlobalIndex, UniformOrAverageContainerSizeDualProperty.GlobalIndex, ItemsHostInsetProperty.GlobalIndex };
		}
	}

	/// <summary>Scrolls content upward by one logical unit.</summary>
	public virtual void LineUp()
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.LineUp);
		}
		bool flag = Orientation == Orientation.Horizontal;
		double offset = ((IsPixelBased || flag) ? (VerticalOffset - 16.0) : NewItemOffset(flag, -1.0, fromFirst: true));
		SetVerticalOffsetImpl(offset, setAnchorInformation: true);
	}

	/// <summary>Scrolls content downward by one logical unit.</summary>
	public virtual void LineDown()
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.LineDown);
		}
		bool flag = Orientation == Orientation.Horizontal;
		double offset = ((IsPixelBased || flag) ? (VerticalOffset + 16.0) : NewItemOffset(flag, 1.0, fromFirst: false));
		SetVerticalOffsetImpl(offset, setAnchorInformation: true);
	}

	/// <summary>Scrolls content to the left by one logical unit.</summary>
	public virtual void LineLeft()
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.LineLeft);
		}
		bool flag = Orientation == Orientation.Horizontal;
		double offset = ((IsPixelBased || !flag) ? (HorizontalOffset - 16.0) : NewItemOffset(flag, -1.0, fromFirst: true));
		SetHorizontalOffsetImpl(offset, setAnchorInformation: true);
	}

	/// <summary>Scrolls content to the right by one logical unit.</summary>
	public virtual void LineRight()
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.LineRight);
		}
		bool flag = Orientation == Orientation.Horizontal;
		double offset = ((IsPixelBased || !flag) ? (HorizontalOffset + 16.0) : NewItemOffset(flag, 1.0, fromFirst: false));
		SetHorizontalOffsetImpl(offset, setAnchorInformation: true);
	}

	/// <summary>Scrolls content upward by one page.</summary>
	public virtual void PageUp()
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.PageUp);
		}
		bool flag = Orientation == Orientation.Horizontal;
		double offset = ((IsPixelBased || flag) ? (VerticalOffset - ViewportHeight) : NewItemOffset(flag, 0.0 - ViewportHeight, fromFirst: true));
		SetVerticalOffsetImpl(offset, setAnchorInformation: true);
	}

	/// <summary>Scrolls content downward by one page.</summary>
	public virtual void PageDown()
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.PageDown);
		}
		bool flag = Orientation == Orientation.Horizontal;
		double offset = ((IsPixelBased || flag) ? (VerticalOffset + ViewportHeight) : NewItemOffset(flag, ViewportHeight, fromFirst: true));
		SetVerticalOffsetImpl(offset, setAnchorInformation: true);
	}

	/// <summary>Scrolls content to the left by one page.</summary>
	public virtual void PageLeft()
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.PageLeft);
		}
		bool flag = Orientation == Orientation.Horizontal;
		double offset = ((IsPixelBased || !flag) ? (HorizontalOffset - ViewportWidth) : NewItemOffset(flag, 0.0 - ViewportWidth, fromFirst: true));
		SetHorizontalOffsetImpl(offset, setAnchorInformation: true);
	}

	/// <summary>Scrolls content to the right by one page.</summary>
	public virtual void PageRight()
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.PageRight);
		}
		bool flag = Orientation == Orientation.Horizontal;
		double offset = ((IsPixelBased || !flag) ? (HorizontalOffset + ViewportWidth) : NewItemOffset(flag, ViewportWidth, fromFirst: true));
		SetHorizontalOffsetImpl(offset, setAnchorInformation: true);
	}

	/// <summary>Scrolls content logically upward in response to an upward click of the mouse wheel button.</summary>
	public virtual void MouseWheelUp()
	{
		if (CanMouseWheelVerticallyScroll)
		{
			if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
			{
				ScrollTracer.Trace(this, ScrollTraceOp.MouseWheelUp);
			}
			int wheelScrollLines = SystemParameters.WheelScrollLines;
			bool flag = Orientation == Orientation.Horizontal;
			double offset = ((IsPixelBased || flag) ? (VerticalOffset - (double)wheelScrollLines * 16.0) : NewItemOffset(flag, -wheelScrollLines, fromFirst: true));
			SetVerticalOffsetImpl(offset, setAnchorInformation: true);
		}
		else
		{
			PageUp();
		}
	}

	/// <summary>Scrolls content logically downward in response to a downward click of the mouse wheel button.</summary>
	public virtual void MouseWheelDown()
	{
		if (CanMouseWheelVerticallyScroll)
		{
			if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
			{
				ScrollTracer.Trace(this, ScrollTraceOp.MouseWheelDown);
			}
			int wheelScrollLines = SystemParameters.WheelScrollLines;
			bool flag = Orientation == Orientation.Horizontal;
			double offset = ((IsPixelBased || flag) ? (VerticalOffset + (double)wheelScrollLines * 16.0) : NewItemOffset(flag, wheelScrollLines, fromFirst: false));
			SetVerticalOffsetImpl(offset, setAnchorInformation: true);
		}
		else
		{
			PageDown();
		}
	}

	/// <summary>Scrolls content logically to the left in response to a left click of the mouse wheel button.</summary>
	public virtual void MouseWheelLeft()
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.MouseWheelLeft);
		}
		bool flag = Orientation == Orientation.Horizontal;
		double offset = ((IsPixelBased || !flag) ? (HorizontalOffset - 48.0) : NewItemOffset(flag, -3.0, fromFirst: true));
		SetHorizontalOffsetImpl(offset, setAnchorInformation: true);
	}

	/// <summary>Scrolls content logically to the right in response to a right click of the mouse wheel button.</summary>
	public virtual void MouseWheelRight()
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.MouseWheelRight);
		}
		bool flag = Orientation == Orientation.Horizontal;
		double offset = ((IsPixelBased || !flag) ? (HorizontalOffset + 48.0) : NewItemOffset(flag, 3.0, fromFirst: false));
		SetHorizontalOffsetImpl(offset, setAnchorInformation: true);
	}

	private double NewItemOffset(bool isHorizontal, double delta, bool fromFirst)
	{
		if (DoubleUtil.IsZero(delta))
		{
			delta = 1.0;
		}
		if (IsVSP45Compat)
		{
			return (isHorizontal ? HorizontalOffset : VerticalOffset) + delta;
		}
		double firstContainerOffsetFromViewport;
		FrameworkElement frameworkElement = ComputeFirstContainerInViewport(this, isHorizontal ? FocusNavigationDirection.Right : FocusNavigationDirection.Down, this, null, findTopContainer: true, out firstContainerOffsetFromViewport);
		if (frameworkElement == null || DoubleUtil.IsZero(firstContainerOffsetFromViewport))
		{
			return (isHorizontal ? HorizontalOffset : VerticalOffset) + delta;
		}
		double num = FindScrollOffset(frameworkElement);
		if (fromFirst)
		{
			num -= firstContainerOffsetFromViewport;
		}
		if (isHorizontal)
		{
			_scrollData._computedOffset.X = num;
		}
		else
		{
			_scrollData._computedOffset.Y = num;
		}
		return num + delta;
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.VirtualizingStackPanel.HorizontalOffset" /> property.</summary>
	/// <param name="offset">The value of the <see cref="P:System.Windows.Controls.VirtualizingStackPanel.HorizontalOffset" /> property.</param>
	public void SetHorizontalOffset(double offset)
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.SetHorizontalOffset, offset, "delta:", offset - HorizontalOffset);
		}
		ClearAnchorInformation(shouldAbort: true);
		SetHorizontalOffsetImpl(offset, setAnchorInformation: false);
	}

	private void SetHorizontalOffsetImpl(double offset, bool setAnchorInformation)
	{
		if (!IsScrolling)
		{
			return;
		}
		double num = ScrollContentPresenter.ValidateInputOffset(offset, "HorizontalOffset");
		if (!DoubleUtil.AreClose(num, _scrollData._offset.X))
		{
			Vector offset2 = _scrollData._offset;
			_scrollData._offset.X = num;
			OnViewportOffsetChanged(offset2, _scrollData._offset);
			if (IsVirtualizing)
			{
				IsScrollActive = true;
				_scrollData.SetHorizontalScrollType(offset2.X, num);
				InvalidateMeasure();
				if (!IsVSP45Compat && Orientation == Orientation.Horizontal)
				{
					IncrementScrollGeneration();
					if (DoubleUtil.LessThanOrClose(Math.Abs(num - offset2.X), ViewportWidth))
					{
						if (!IsPixelBased)
						{
							_scrollData._offset.X = Math.Floor(_scrollData._offset.X);
							_scrollData._computedOffset.X = Math.Floor(_scrollData._computedOffset.X);
						}
						else if (base.UseLayoutRounding)
						{
							DpiScale dpi = GetDpi();
							_scrollData._offset.X = UIElement.RoundLayoutValue(_scrollData._offset.X, dpi.DpiScaleX);
							_scrollData._computedOffset.X = UIElement.RoundLayoutValue(_scrollData._computedOffset.X, dpi.DpiScaleX);
						}
						if (!setAnchorInformation && !IsPixelBased)
						{
							double firstContainerOffsetFromViewport;
							FrameworkElement v = ComputeFirstContainerInViewport(this, FocusNavigationDirection.Right, this, null, findTopContainer: true, out firstContainerOffsetFromViewport);
							if (firstContainerOffsetFromViewport > 0.0)
							{
								double x = FindScrollOffset(v);
								_scrollData._computedOffset.X = x;
							}
						}
						setAnchorInformation = true;
					}
				}
			}
			else if (!IsPixelBased)
			{
				InvalidateMeasure();
			}
			else
			{
				_scrollData._offset.X = ScrollContentPresenter.CoerceOffset(num, _scrollData._extent.Width, _scrollData._viewport.Width);
				_scrollData._computedOffset.X = _scrollData._offset.X;
				InvalidateArrange();
				OnScrollChange();
			}
			if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
			{
				ScrollTracer.Trace(this, ScrollTraceOp.SetHOff, _scrollData._offset, _scrollData._extent, _scrollData._computedOffset);
			}
		}
		if (setAnchorInformation)
		{
			SetAnchorInformation(isHorizontalOffset: true);
		}
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.VirtualizingStackPanel.VerticalOffset" /> property.</summary>
	/// <param name="offset">The value of the <see cref="P:System.Windows.Controls.VirtualizingStackPanel.VerticalOffset" /> property.</param>
	public void SetVerticalOffset(double offset)
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.SetVerticalOffset, offset, "delta:", offset - VerticalOffset);
		}
		ClearAnchorInformation(shouldAbort: true);
		SetVerticalOffsetImpl(offset, setAnchorInformation: false);
	}

	private void SetVerticalOffsetImpl(double offset, bool setAnchorInformation)
	{
		if (!IsScrolling)
		{
			return;
		}
		double num = ScrollContentPresenter.ValidateInputOffset(offset, "VerticalOffset");
		if (!DoubleUtil.AreClose(num, _scrollData._offset.Y))
		{
			Vector offset2 = _scrollData._offset;
			_scrollData._offset.Y = num;
			OnViewportOffsetChanged(offset2, _scrollData._offset);
			if (IsVirtualizing)
			{
				InvalidateMeasure();
				IsScrollActive = true;
				_scrollData.SetVerticalScrollType(offset2.Y, num);
				if (!IsVSP45Compat && Orientation == Orientation.Vertical)
				{
					IncrementScrollGeneration();
					if (DoubleUtil.LessThanOrClose(Math.Abs(num - offset2.Y), ViewportHeight))
					{
						if (!IsPixelBased)
						{
							_scrollData._offset.Y = Math.Floor(_scrollData._offset.Y);
							_scrollData._computedOffset.Y = Math.Floor(_scrollData._computedOffset.Y);
						}
						else if (base.UseLayoutRounding)
						{
							DpiScale dpi = GetDpi();
							_scrollData._offset.Y = UIElement.RoundLayoutValue(_scrollData._offset.Y, dpi.DpiScaleY);
							_scrollData._computedOffset.Y = UIElement.RoundLayoutValue(_scrollData._computedOffset.Y, dpi.DpiScaleY);
						}
						if (!setAnchorInformation && !IsPixelBased)
						{
							double firstContainerOffsetFromViewport;
							FrameworkElement v = ComputeFirstContainerInViewport(this, FocusNavigationDirection.Down, this, null, findTopContainer: true, out firstContainerOffsetFromViewport);
							if (firstContainerOffsetFromViewport > 0.0)
							{
								double y = FindScrollOffset(v);
								_scrollData._computedOffset.Y = y;
							}
						}
						setAnchorInformation = true;
					}
				}
			}
			else if (!IsPixelBased)
			{
				InvalidateMeasure();
			}
			else
			{
				_scrollData._offset.Y = ScrollContentPresenter.CoerceOffset(num, _scrollData._extent.Height, _scrollData._viewport.Height);
				_scrollData._computedOffset.Y = _scrollData._offset.Y;
				InvalidateArrange();
				OnScrollChange();
			}
		}
		if (setAnchorInformation)
		{
			SetAnchorInformation(isHorizontalOffset: false);
		}
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.SetVOff, _scrollData._offset, _scrollData._extent, _scrollData._computedOffset);
		}
	}

	private void SetAnchorInformation(bool isHorizontalOffset)
	{
		if (!IsScrolling || !IsVirtualizing)
		{
			return;
		}
		bool flag = Orientation == Orientation.Horizontal;
		if (flag != isHorizontalOffset || (GetAreContainersUniformlySized(null, this) && !HasVirtualizingChildren))
		{
			return;
		}
		ItemsControl.GetItemsOwnerInternal(this, out var itemsControl);
		if (itemsControl == null)
		{
			return;
		}
		bool flag2 = ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this);
		double num = (flag ? (_scrollData._offset.X - _scrollData._computedOffset.X) : (_scrollData._offset.Y - _scrollData._computedOffset.Y));
		if (flag2)
		{
			ScrollTracer.Trace(this, ScrollTraceOp.BSetAnchor, num);
		}
		if (_scrollData._firstContainerInViewport != null)
		{
			OnAnchorOperation(isAnchorOperationPending: true);
			if (flag)
			{
				_scrollData._offset.X += num;
			}
			else
			{
				_scrollData._offset.Y += num;
			}
		}
		if (_scrollData._firstContainerInViewport == null)
		{
			_scrollData._firstContainerInViewport = ComputeFirstContainerInViewport(itemsControl.GetViewportElement(), flag ? FocusNavigationDirection.Right : FocusNavigationDirection.Down, this, delegate(DependencyObject d)
			{
				d.SetCurrentValue(VirtualizingPanel.IsContainerVirtualizableProperty, value: false);
			}, findTopContainer: false, out _scrollData._firstContainerOffsetFromViewport);
			if (_scrollData._firstContainerInViewport != null)
			{
				_scrollData._expectedDistanceBetweenViewports = num;
				DispatcherOperation value = base.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(OnAnchorOperation));
				AnchorOperationField.SetValue(this, value);
			}
		}
		else
		{
			_scrollData._expectedDistanceBetweenViewports += num;
		}
		if (flag2)
		{
			ScrollTracer.Trace(this, ScrollTraceOp.ESetAnchor, _scrollData._expectedDistanceBetweenViewports, _scrollData._firstContainerInViewport, _scrollData._firstContainerOffsetFromViewport);
		}
	}

	private void OnAnchorOperation()
	{
		bool isAnchorOperationPending = false;
		OnAnchorOperation(isAnchorOperationPending);
	}

	private void OnAnchorOperation(bool isAnchorOperationPending)
	{
		ItemsControl.GetItemsOwnerInternal(this, out var itemsControl);
		if (itemsControl == null || !VisualTreeHelper.IsAncestorOf(this, _scrollData._firstContainerInViewport))
		{
			ClearAnchorInformation(isAnchorOperationPending);
			return;
		}
		bool flag = ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this);
		if (flag)
		{
			ScrollTracer.Trace(this, ScrollTraceOp.BOnAnchor, isAnchorOperationPending, _scrollData._expectedDistanceBetweenViewports, _scrollData._firstContainerInViewport);
		}
		bool isVSP45Compat = IsVSP45Compat;
		if (!isVSP45Compat && !isAnchorOperationPending && (base.MeasureDirty || base.ArrangeDirty))
		{
			if (flag)
			{
				ScrollTracer.Trace(this, ScrollTraceOp.ROnAnchor);
			}
			CancelPendingAnchoredInvalidateMeasure();
			DispatcherOperation value = base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(OnAnchorOperation));
			AnchorOperationField.SetValue(this, value);
			return;
		}
		bool flag2 = Orientation == Orientation.Horizontal;
		_ = _scrollData._firstContainerInViewport;
		double firstContainerOffsetFromViewport = _scrollData._firstContainerOffsetFromViewport;
		double num = FindScrollOffset(_scrollData._firstContainerInViewport);
		double firstContainerOffsetFromViewport2;
		FrameworkElement v = ComputeFirstContainerInViewport(itemsControl.GetViewportElement(), flag2 ? FocusNavigationDirection.Right : FocusNavigationDirection.Down, this, null, findTopContainer: false, out firstContainerOffsetFromViewport2);
		double num2 = FindScrollOffset(v);
		double num3 = num2 - firstContainerOffsetFromViewport2 - (num - firstContainerOffsetFromViewport);
		bool flag3 = LayoutDoubleUtil.AreClose(_scrollData._expectedDistanceBetweenViewports, num3);
		if (!flag3 && !isVSP45Compat && !IsPixelBased)
		{
			ComputeFirstContainerInViewport(this, flag2 ? FocusNavigationDirection.Right : FocusNavigationDirection.Down, this, null, findTopContainer: true, out var firstContainerOffsetFromViewport3);
			double num4 = num3 - _scrollData._expectedDistanceBetweenViewports;
			flag3 = !LayoutDoubleUtil.LessThan(num4, 0.0) && !LayoutDoubleUtil.LessThan(firstContainerOffsetFromViewport3, num4);
			if (flag3)
			{
				firstContainerOffsetFromViewport2 += firstContainerOffsetFromViewport3;
			}
			if (!flag3)
			{
				double value2;
				double value3;
				if (flag2)
				{
					value2 = _scrollData._computedOffset.X;
					value3 = _scrollData._extent.Width - _scrollData._viewport.Width;
				}
				else
				{
					value2 = _scrollData._computedOffset.Y;
					value3 = _scrollData._extent.Height - _scrollData._viewport.Height;
				}
				flag3 = LayoutDoubleUtil.LessThan(value3, value2) || LayoutDoubleUtil.AreClose(value3, value2);
			}
		}
		if (flag3)
		{
			if (flag2)
			{
				_scrollData._computedOffset.X = num2 - firstContainerOffsetFromViewport2;
				_scrollData._offset.X = _scrollData._computedOffset.X;
			}
			else
			{
				_scrollData._computedOffset.Y = num2 - firstContainerOffsetFromViewport2;
				_scrollData._offset.Y = _scrollData._computedOffset.Y;
			}
			ClearAnchorInformation(isAnchorOperationPending);
			if (flag)
			{
				ScrollTracer.Trace(this, ScrollTraceOp.SOnAnchor, _scrollData._offset);
			}
			return;
		}
		bool flag4 = false;
		double num5;
		double num6;
		if (flag2)
		{
			_scrollData._computedOffset.X = num - firstContainerOffsetFromViewport;
			num5 = _scrollData._computedOffset.X + num3;
			num6 = _scrollData._computedOffset.X + _scrollData._expectedDistanceBetweenViewports;
			double num7 = _scrollData._extent.Width - _scrollData._viewport.Width;
			if (LayoutDoubleUtil.LessThan(num6, 0.0) || LayoutDoubleUtil.LessThan(num7, num6))
			{
				if (LayoutDoubleUtil.AreClose(num5, 0.0) || LayoutDoubleUtil.AreClose(num5, num7))
				{
					_scrollData._computedOffset.X = num5;
					_scrollData._offset.X = num5;
				}
				else
				{
					flag4 = true;
					_scrollData._offset.X = num6;
				}
			}
			else
			{
				flag4 = true;
				_scrollData._offset.X = num6;
			}
		}
		else
		{
			_scrollData._computedOffset.Y = num - firstContainerOffsetFromViewport;
			num5 = _scrollData._computedOffset.Y + num3;
			num6 = _scrollData._computedOffset.Y + _scrollData._expectedDistanceBetweenViewports;
			double num7 = _scrollData._extent.Height - _scrollData._viewport.Height;
			if (LayoutDoubleUtil.LessThan(num6, 0.0) || LayoutDoubleUtil.LessThan(num7, num6))
			{
				if (LayoutDoubleUtil.AreClose(num5, 0.0) || LayoutDoubleUtil.AreClose(num5, num7))
				{
					_scrollData._computedOffset.Y = num5;
					_scrollData._offset.Y = num5;
				}
				else
				{
					flag4 = true;
					_scrollData._offset.Y = num6;
				}
			}
			else
			{
				flag4 = true;
				_scrollData._offset.Y = num6;
			}
		}
		if (flag)
		{
			ScrollTracer.Trace(this, ScrollTraceOp.EOnAnchor, flag4, num6, num5, _scrollData._offset, _scrollData._computedOffset);
		}
		if (flag4)
		{
			OnScrollChange();
			InvalidateMeasure();
			if (!isVSP45Compat)
			{
				CancelPendingAnchoredInvalidateMeasure();
				IncrementScrollGeneration();
			}
			if (!isAnchorOperationPending)
			{
				DispatcherOperation value4 = base.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(OnAnchorOperation));
				AnchorOperationField.SetValue(this, value4);
			}
			if (!isVSP45Compat && IsScrollActive)
			{
				DispatcherOperation value5 = ClearIsScrollActiveOperationField.GetValue(this);
				if (value5 != null)
				{
					value5.Abort();
					ClearIsScrollActiveOperationField.SetValue(this, null);
				}
			}
		}
		else
		{
			ClearAnchorInformation(isAnchorOperationPending);
		}
	}

	private void ClearAnchorInformation(bool shouldAbort)
	{
		if (_scrollData == null || _scrollData._firstContainerInViewport == null)
		{
			return;
		}
		DependencyObject dependencyObject = _scrollData._firstContainerInViewport;
		do
		{
			DependencyObject parent = VisualTreeHelper.GetParent(dependencyObject);
			if (parent is Panel { IsItemsHost: not false })
			{
				dependencyObject.InvalidateProperty(VirtualizingPanel.IsContainerVirtualizableProperty);
			}
			dependencyObject = parent;
		}
		while (dependencyObject != null && dependencyObject != this);
		_scrollData._firstContainerInViewport = null;
		_scrollData._firstContainerOffsetFromViewport = 0.0;
		_scrollData._expectedDistanceBetweenViewports = 0.0;
		if (shouldAbort)
		{
			AnchorOperationField.GetValue(this).Abort();
		}
		AnchorOperationField.ClearValue(this);
	}

	private FrameworkElement ComputeFirstContainerInViewport(FrameworkElement viewportElement, FocusNavigationDirection direction, Panel itemsHost, Action<DependencyObject> action, bool findTopContainer, out double firstContainerOffsetFromViewport)
	{
		bool foundTopContainer;
		return ComputeFirstContainerInViewport(viewportElement, direction, itemsHost, action, findTopContainer, out firstContainerOffsetFromViewport, out foundTopContainer);
	}

	private FrameworkElement ComputeFirstContainerInViewport(FrameworkElement viewportElement, FocusNavigationDirection direction, Panel itemsHost, Action<DependencyObject> action, bool findTopContainer, out double firstContainerOffsetFromViewport, out bool foundTopContainer)
	{
		firstContainerOffsetFromViewport = 0.0;
		foundTopContainer = false;
		if (itemsHost == null)
		{
			return null;
		}
		bool isVSP45Compat = IsVSP45Compat;
		if (!isVSP45Compat)
		{
			viewportElement = this;
		}
		FrameworkElement frameworkElement = null;
		UIElementCollection children = itemsHost.Children;
		if (children != null)
		{
			int count = children.Count;
			int num = 0;
			for (int i = ((itemsHost is VirtualizingStackPanel) ? ((VirtualizingStackPanel)itemsHost)._firstItemInExtendedViewportChildIndex : 0); i < count; i++)
			{
				if (!(children[i] is FrameworkElement frameworkElement2))
				{
					continue;
				}
				if (frameworkElement2.IsVisible)
				{
					Rect elementRect;
					Rect layoutRect;
					switch (ItemsControl.GetElementViewportPosition(viewportElement, frameworkElement2, direction, fullyVisible: false, !isVSP45Compat, out elementRect, out layoutRect))
					{
					case ElementViewportPosition.PartiallyInViewport:
					case ElementViewportPosition.CompletelyInViewport:
					{
						bool flag = false;
						if (!IsPixelBased)
						{
							double value = ((direction == FocusNavigationDirection.Down) ? elementRect.Y : elementRect.X);
							if (findTopContainer && DoubleUtil.GreaterThanZero(value))
							{
								break;
							}
							flag = DoubleUtil.IsZero(value);
						}
						action?.Invoke(frameworkElement2);
						if (isVSP45Compat)
						{
							if (frameworkElement2 is ItemsControl itemsControl)
							{
								if (itemsControl.ItemsHost != null && itemsControl.ItemsHost.IsVisible)
								{
									frameworkElement = ComputeFirstContainerInViewport(viewportElement, direction, itemsControl.ItemsHost, action, findTopContainer, out firstContainerOffsetFromViewport);
								}
							}
							else if (frameworkElement2 is GroupItem { ItemsHost: not null } groupItem && groupItem.ItemsHost.IsVisible)
							{
								frameworkElement = ComputeFirstContainerInViewport(viewportElement, direction, groupItem.ItemsHost, action, findTopContainer, out firstContainerOffsetFromViewport);
							}
						}
						else
						{
							Panel panel = null;
							if (frameworkElement2 is ItemsControl itemsControl2)
							{
								panel = itemsControl2.ItemsHost;
							}
							else if (frameworkElement2 is GroupItem groupItem2)
							{
								panel = groupItem2.ItemsHost;
							}
							panel = panel as VirtualizingStackPanel;
							if (panel != null && panel.IsVisible)
							{
								frameworkElement = ComputeFirstContainerInViewport(viewportElement, direction, panel, action, findTopContainer, out firstContainerOffsetFromViewport, out foundTopContainer);
							}
						}
						if (frameworkElement == null)
						{
							frameworkElement = frameworkElement2;
							foundTopContainer = flag;
							if (IsPixelBased)
							{
								if (direction == FocusNavigationDirection.Down)
								{
									if (isVSP45Compat)
									{
										firstContainerOffsetFromViewport = elementRect.Y;
									}
									else
									{
										firstContainerOffsetFromViewport = layoutRect.Top;
									}
								}
								else if (isVSP45Compat)
								{
									firstContainerOffsetFromViewport = elementRect.X;
								}
								else
								{
									firstContainerOffsetFromViewport = layoutRect.Left;
								}
							}
							else if (findTopContainer && flag)
							{
								firstContainerOffsetFromViewport += num;
							}
						}
						else
						{
							if (IsPixelBased || !(frameworkElement2 is IHierarchicalVirtualizationAndScrollInfo hierarchicalVirtualizationAndScrollInfo))
							{
								break;
							}
							if (isVSP45Compat)
							{
								if (direction == FocusNavigationDirection.Down)
								{
									if (DoubleUtil.GreaterThanOrClose(elementRect.Y, 0.0))
									{
										firstContainerOffsetFromViewport += hierarchicalVirtualizationAndScrollInfo.HeaderDesiredSizes.LogicalSize.Height;
									}
								}
								else if (DoubleUtil.GreaterThanOrClose(elementRect.X, 0.0))
								{
									firstContainerOffsetFromViewport += hierarchicalVirtualizationAndScrollInfo.HeaderDesiredSizes.LogicalSize.Width;
								}
								break;
							}
							Thickness inset = GetItemsHostInsetForChild(hierarchicalVirtualizationAndScrollInfo);
							if (direction == FocusNavigationDirection.Down)
							{
								if (IsHeaderBeforeItems(isHorizontal: false, frameworkElement2, ref inset) && DoubleUtil.GreaterThanOrClose(elementRect.Y, 0.0) && (findTopContainer || !foundTopContainer || DoubleUtil.GreaterThanZero(elementRect.Y)))
								{
									firstContainerOffsetFromViewport += 1.0;
								}
							}
							else if (IsHeaderBeforeItems(isHorizontal: true, frameworkElement2, ref inset) && DoubleUtil.GreaterThanOrClose(elementRect.X, 0.0) && (findTopContainer || !foundTopContainer || DoubleUtil.GreaterThanZero(elementRect.X)))
							{
								firstContainerOffsetFromViewport += 1.0;
							}
						}
						break;
					}
					default:
						num = 0;
						continue;
					case ElementViewportPosition.AfterViewport:
						break;
					}
					break;
				}
				num++;
			}
		}
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.CFCIV, ContainerPath(frameworkElement), firstContainerOffsetFromViewport);
		}
		return frameworkElement;
	}

	internal void AnchoredInvalidateMeasure()
	{
		WasLastMeasurePassAnchored = FirstContainerInViewport != null || BringIntoViewLeafContainer != null;
		DispatcherOperation value = AnchoredInvalidateMeasureOperationField.GetValue(this);
		if (value != null)
		{
			return;
		}
		value = base.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)delegate
		{
			if (IsVSP45Compat)
			{
				AnchoredInvalidateMeasureOperationField.ClearValue(this);
				if (WasLastMeasurePassAnchored)
				{
					SetAnchorInformation(Orientation == Orientation.Horizontal);
				}
				InvalidateMeasure();
			}
			else
			{
				InvalidateMeasure();
				AnchoredInvalidateMeasureOperationField.ClearValue(this);
				if (WasLastMeasurePassAnchored)
				{
					SetAnchorInformation(Orientation == Orientation.Horizontal);
				}
			}
		});
		AnchoredInvalidateMeasureOperationField.SetValue(this, value);
	}

	private void CancelPendingAnchoredInvalidateMeasure()
	{
		DispatcherOperation value = AnchoredInvalidateMeasureOperationField.GetValue(this);
		if (value != null)
		{
			value.Abort();
			AnchoredInvalidateMeasureOperationField.ClearValue(this);
		}
	}

	/// <summary>Scrolls to the specified coordinates and makes that portion of a <see cref="T:System.Windows.Media.Visual" /> visible.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> that is visible.</returns>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> that becomes visible.</param>
	/// <param name="rectangle">A <see cref="T:System.Windows.Rect" /> that represents the coordinate space within a <see cref="T:System.Windows.Media.Visual" />.</param>
	public Rect MakeVisible(Visual visual, Rect rectangle)
	{
		ClearAnchorInformation(shouldAbort: true);
		Vector newOffset = default(Vector);
		Rect newRect = default(Rect);
		Rect rect = rectangle;
		bool flag = Orientation == Orientation.Horizontal;
		if (rectangle.IsEmpty || visual == null || visual == this || !IsAncestorOf(visual))
		{
			return Rect.Empty;
		}
		rectangle = visual.TransformToAncestor(this).TransformBounds(rectangle);
		if (!IsScrolling)
		{
			return rectangle;
		}
		bool isVSP45Compat = IsVSP45Compat;
		bool alignTop = false;
		bool alignBottom = false;
		MakeVisiblePhysicalHelper(rectangle, ref newOffset, ref newRect, !flag, ref alignTop, ref alignBottom);
		alignTop = _scrollData._bringIntoViewLeafContainer == visual && AlignTopOfBringIntoViewContainer;
		alignBottom = _scrollData._bringIntoViewLeafContainer == visual && (isVSP45Compat ? (!AlignTopOfBringIntoViewContainer) : AlignBottomOfBringIntoViewContainer);
		if (IsPixelBased)
		{
			MakeVisiblePhysicalHelper(rectangle, ref newOffset, ref newRect, flag, ref alignTop, ref alignBottom);
		}
		else
		{
			int childIndex = (int)FindScrollOffset(visual);
			MakeVisibleLogicalHelper(childIndex, rectangle, ref newOffset, ref newRect, ref alignTop, ref alignBottom);
		}
		newOffset.X = ScrollContentPresenter.CoerceOffset(newOffset.X, _scrollData._extent.Width, _scrollData._viewport.Width);
		newOffset.Y = ScrollContentPresenter.CoerceOffset(newOffset.Y, _scrollData._extent.Height, _scrollData._viewport.Height);
		if (!LayoutDoubleUtil.AreClose(newOffset.X, _scrollData._offset.X) || !LayoutDoubleUtil.AreClose(newOffset.Y, _scrollData._offset.Y))
		{
			if (visual != _scrollData._bringIntoViewLeafContainer)
			{
				_scrollData._bringIntoViewLeafContainer = visual;
				AlignTopOfBringIntoViewContainer = alignTop;
				AlignBottomOfBringIntoViewContainer = alignBottom;
			}
			Vector offset = _scrollData._offset;
			_scrollData._offset = newOffset;
			if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
			{
				ScrollTracer.Trace(this, ScrollTraceOp.MakeVisible, _scrollData._offset, rectangle, _scrollData._bringIntoViewLeafContainer);
			}
			OnViewportOffsetChanged(offset, newOffset);
			if (IsVirtualizing)
			{
				IsScrollActive = true;
				_scrollData.SetHorizontalScrollType(offset.X, newOffset.X);
				_scrollData.SetVerticalScrollType(offset.Y, newOffset.Y);
				InvalidateMeasure();
			}
			else if (!IsPixelBased)
			{
				InvalidateMeasure();
			}
			else
			{
				_scrollData._computedOffset = newOffset;
				InvalidateArrange();
			}
			OnScrollChange();
			if (ScrollOwner != null)
			{
				ScrollOwner.MakeVisible(visual, rect);
			}
		}
		else
		{
			if (isVSP45Compat)
			{
				_scrollData._bringIntoViewLeafContainer = null;
			}
			AlignTopOfBringIntoViewContainer = false;
			AlignBottomOfBringIntoViewContainer = false;
		}
		return newRect;
	}

	/// <summary>Generates the item at the specified index position and brings it into view.</summary>
	/// <param name="index">The position of the item to generate and make visible.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="index" /> position does not exist in the child collection.</exception>
	protected internal override void BringIndexIntoView(int index)
	{
		ItemsControl itemsOwner = ItemsControl.GetItemsOwner(this);
		if (!itemsOwner.IsGrouping)
		{
			BringContainerIntoView(itemsOwner, index);
			return;
		}
		EnsureGenerator();
		ItemContainerGenerator itemContainerGenerator = (ItemContainerGenerator)base.Generator;
		IList itemsInternal = itemContainerGenerator.ItemsInternal;
		for (int i = 0; i < itemsInternal.Count; i++)
		{
			if (itemsInternal[i] is CollectionViewGroup collectionViewGroup)
			{
				if (index < collectionViewGroup.ItemCount)
				{
					GroupItem groupItem = itemContainerGenerator.ContainerFromItem(collectionViewGroup) as GroupItem;
					if (groupItem == null)
					{
						BringContainerIntoView(itemsOwner, i);
						groupItem = itemContainerGenerator.ContainerFromItem(collectionViewGroup) as GroupItem;
					}
					if (groupItem != null)
					{
						groupItem.UpdateLayout();
						if (groupItem.ItemsHost is VirtualizingPanel virtualizingPanel)
						{
							virtualizingPanel.BringIndexIntoViewPublic(index);
						}
					}
					break;
				}
				index -= collectionViewGroup.ItemCount;
			}
			else if (i == index)
			{
				BringContainerIntoView(itemsOwner, i);
			}
		}
	}

	private void BringContainerIntoView(ItemsControl itemsControl, int itemIndex)
	{
		if (itemIndex < 0 || itemIndex >= ItemCount)
		{
			throw new ArgumentOutOfRangeException("itemIndex");
		}
		IItemContainerGenerator generator = base.Generator;
		int childIndex;
		GeneratorPosition position = IndexToGeneratorPositionForStart(itemIndex, out childIndex);
		UIElement uIElement;
		using (generator.StartAt(position, GeneratorDirection.Forward, allowStartAtRealizedItem: true))
		{
			uIElement = generator.GenerateNext(out var isNewlyRealized) as UIElement;
			if (uIElement != null && AddContainerFromGenerator(childIndex, uIElement, isNewlyRealized, isBeforeViewport: false))
			{
				InvalidateZState();
			}
		}
		if (uIElement != null && uIElement is FrameworkElement frameworkElement)
		{
			_bringIntoViewContainer = frameworkElement;
			base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)delegate
			{
				_bringIntoViewContainer = null;
			});
			if (!itemsControl.IsGrouping && VirtualizingPanel.GetScrollUnit(itemsControl) == ScrollUnit.Item)
			{
				frameworkElement.BringIntoView();
			}
			else if (!(frameworkElement is GroupItem))
			{
				UpdateLayout();
				frameworkElement.BringIntoView();
			}
		}
	}

	/// <summary>Adds an event handler for the <see cref="E:System.Windows.Controls.VirtualizingStackPanel.CleanUpVirtualizedItem" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> that is listening for this event.</param>
	/// <param name="handler">The event handler that is to be added.</param>
	public static void AddCleanUpVirtualizedItemHandler(DependencyObject element, CleanUpVirtualizedItemEventHandler handler)
	{
		UIElement.AddHandler(element, CleanUpVirtualizedItemEvent, handler);
	}

	/// <summary>Removes an event handler for the <see cref="E:System.Windows.Controls.VirtualizingStackPanel.CleanUpVirtualizedItem" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.DependencyObject" /> from which the handler is being removed.</param>
	/// <param name="handler">Specifies the event handler that is to be removed.</param>
	public static void RemoveCleanUpVirtualizedItemHandler(DependencyObject element, CleanUpVirtualizedItemEventHandler handler)
	{
		UIElement.RemoveHandler(element, CleanUpVirtualizedItemEvent, handler);
	}

	/// <summary>Called when an item that is hosted by the <see cref="T:System.Windows.Controls.VirtualizingStackPanel" /> is re-virtualized. </summary>
	/// <param name="e">Data about the event.</param>
	protected virtual void OnCleanUpVirtualizedItem(CleanUpVirtualizedItemEventArgs e)
	{
		ItemsControl.GetItemsOwner(this)?.RaiseEvent(e);
	}

	/// <summary>Measures the child elements of a <see cref="T:System.Windows.Controls.VirtualizingStackPanel" /> in anticipation of arranging them during the <see cref="M:System.Windows.Controls.VirtualizingStackPanel.ArrangeOverride(System.Windows.Size)" /> pass.</summary>
	/// <returns>The <see cref="T:System.Windows.Size" /> that represents the desired size of the element.</returns>
	/// <param name="constraint">An upper limit <see cref="T:System.Windows.Size" /> that should not be exceeded.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		List<double> previouslyMeasuredOffsets = null;
		double? lastPageSafeOffset = null;
		double? lastPagePixelSize = null;
		if (IsVSP45Compat)
		{
			return MeasureOverrideImpl(constraint, ref lastPageSafeOffset, ref previouslyMeasuredOffsets, ref lastPagePixelSize, remeasure: false);
		}
		OffsetInformation value = OffsetInformationField.GetValue(this);
		if (value != null)
		{
			previouslyMeasuredOffsets = value.previouslyMeasuredOffsets;
			lastPageSafeOffset = value.lastPageSafeOffset;
			lastPagePixelSize = value.lastPagePixelSize;
		}
		Size result = MeasureOverrideImpl(constraint, ref lastPageSafeOffset, ref previouslyMeasuredOffsets, ref lastPagePixelSize, remeasure: false);
		if (IsScrollActive)
		{
			value = new OffsetInformation
			{
				previouslyMeasuredOffsets = previouslyMeasuredOffsets,
				lastPageSafeOffset = lastPageSafeOffset,
				lastPagePixelSize = lastPagePixelSize
			};
			OffsetInformationField.SetValue(this, value);
		}
		return result;
	}

	private Size MeasureOverrideImpl(Size constraint, ref double? lastPageSafeOffset, ref List<double> previouslyMeasuredOffsets, ref double? lastPagePixelSize, bool remeasure)
	{
		bool flag = IsScrolling && EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info);
		if (flag)
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringBegin, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "VirtualizingStackPanel :MeasureOverride");
		}
		Size stackPixelSize = default(Size);
		Size stackLogicalSize = default(Size);
		Size stackPixelSizeInViewport = default(Size);
		Size stackLogicalSizeInViewport = default(Size);
		Size stackPixelSizeInCacheBeforeViewport = default(Size);
		Size stackLogicalSizeInCacheBeforeViewport = default(Size);
		Size stackPixelSizeInCacheAfterViewport = default(Size);
		Size stackLogicalSizeInCacheAfterViewport = default(Size);
		bool hasVirtualizingChildren = false;
		ItemsChangedDuringMeasure = false;
		try
		{
			bool isVSP45Compat;
			IHierarchicalVirtualizationAndScrollInfo virtualizationInfoProvider;
			IContainItemStorage itemStorageProvider;
			object parentItem;
			bool isHorizontal;
			bool mustDisableVirtualization;
			IContainItemStorage parentItemStorageProvider;
			Rect viewport;
			VirtualizationCacheLength cacheSize;
			VirtualizationCacheLengthUnit cacheUnit;
			long scrollGeneration;
			int firstItemInViewportIndex;
			UIElement uIElement;
			double firstItemInViewportOffset;
			bool foundFirstItemInViewport;
			IList children;
			IItemContainerGenerator generator;
			IList items;
			int count;
			bool areContainersUniformlySized;
			bool computedAreContainersUniformlySized;
			double uniformOrAverageContainerSize;
			double uniformOrAverageContainerPixelSize;
			bool hasUniformOrAverageContainerSizeBeenSet;
			double computedUniformOrAverageContainerSize;
			double computedUniformOrAverageContainerPixelSize;
			Size childConstraint;
			bool visualOrderChanged;
			bool hasBringIntoViewContainerBeenMeasured;
			bool hasAverageContainerSizeChanged;
			bool hasAnyContainerSpanChanged;
			if (base.IsItemsHost)
			{
				isVSP45Compat = IsVSP45Compat;
				ItemsControl itemsControl = null;
				GroupItem groupItem = null;
				virtualizationInfoProvider = null;
				itemStorageProvider = null;
				parentItem = null;
				isHorizontal = Orientation == Orientation.Horizontal;
				mustDisableVirtualization = false;
				GetOwners(shouldSetVirtualizationState: true, isHorizontal, out itemsControl, out groupItem, out itemStorageProvider, out virtualizationInfoProvider, out parentItem, out parentItemStorageProvider, out mustDisableVirtualization);
				viewport = Rect.Empty;
				Rect extendedViewport = Rect.Empty;
				cacheSize = new VirtualizationCacheLength(0.0);
				cacheUnit = VirtualizationCacheLengthUnit.Pixel;
				InitializeViewport(parentItem, parentItemStorageProvider, virtualizationInfoProvider, isHorizontal, constraint, ref viewport, ref cacheSize, ref cacheUnit, out extendedViewport, out scrollGeneration);
				firstItemInViewportIndex = int.MinValue;
				int num = int.MaxValue;
				int num2 = int.MinValue;
				int firstItemInViewportIndex2 = int.MinValue;
				uIElement = null;
				firstItemInViewportOffset = 0.0;
				double firstItemInViewportOffset2 = 0.0;
				foundFirstItemInViewport = false;
				bool flag2 = false;
				bool foundFirstItemInViewport2 = false;
				EnsureGenerator();
				children = RealizedChildren;
				generator = base.Generator;
				items = ((ItemContainerGenerator)generator).ItemsInternal;
				count = items.Count;
				IContainItemStorage itemStorageProvider2 = (isVSP45Compat ? itemStorageProvider : parentItemStorageProvider);
				areContainersUniformlySized = GetAreContainersUniformlySized(itemStorageProvider2, parentItem);
				computedAreContainersUniformlySized = areContainersUniformlySized;
				GetUniformOrAverageContainerSize(itemStorageProvider2, parentItem, IsPixelBased || isVSP45Compat, out uniformOrAverageContainerSize, out uniformOrAverageContainerPixelSize, out hasUniformOrAverageContainerSizeBeenSet);
				computedUniformOrAverageContainerSize = uniformOrAverageContainerSize;
				computedUniformOrAverageContainerPixelSize = uniformOrAverageContainerPixelSize;
				if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
				{
					ScrollTracer.Trace(this, ScrollTraceOp.BeginMeasure, constraint, "MC:", MeasureCaches, "reM:", remeasure, "acs:", uniformOrAverageContainerSize, areContainersUniformlySized, hasUniformOrAverageContainerSizeBeenSet);
				}
				ComputeFirstItemInViewportIndexAndOffset(items, count, itemStorageProvider, viewport, cacheSize, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, out firstItemInViewportOffset, out var firstItemInViewportContainerSpan, out firstItemInViewportIndex, out foundFirstItemInViewport);
				ComputeFirstItemInViewportIndexAndOffset(items, count, itemStorageProvider, extendedViewport, new VirtualizationCacheLength(0.0), isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, out firstItemInViewportOffset2, out var _, out firstItemInViewportIndex2, out foundFirstItemInViewport2);
				if (IsVirtualizing && !remeasure && InRecyclingMode)
				{
					int num3 = _itemsInExtendedViewportCount;
					if (!isVSP45Compat)
					{
						int num4 = (int)Math.Ceiling(Math.Min(1.0, isHorizontal ? (viewport.Width / extendedViewport.Width) : (viewport.Height / extendedViewport.Height)) * (double)_itemsInExtendedViewportCount);
						num3 = Math.Max(num3, firstItemInViewportIndex + num4 - firstItemInViewportIndex2);
					}
					CleanupContainers(firstItemInViewportIndex2, num3, itemsControl);
				}
				childConstraint = constraint;
				if (isHorizontal)
				{
					childConstraint.Width = double.PositiveInfinity;
					if (IsScrolling && CanVerticallyScroll)
					{
						childConstraint.Height = double.PositiveInfinity;
					}
				}
				else
				{
					childConstraint.Height = double.PositiveInfinity;
					if (IsScrolling && CanHorizontallyScroll)
					{
						childConstraint.Width = double.PositiveInfinity;
					}
				}
				remeasure = false;
				_actualItemsInExtendedViewportCount = 0;
				_firstItemInExtendedViewportIndex = 0;
				_firstItemInExtendedViewportOffset = 0.0;
				_firstItemInExtendedViewportChildIndex = 0;
				visualOrderChanged = false;
				int childIndex = 0;
				hasBringIntoViewContainerBeenMeasured = false;
				hasAverageContainerSizeChanged = false;
				hasAnyContainerSpanChanged = false;
				if (count <= 0)
				{
					goto IL_0980;
				}
				using (((ItemContainerGenerator)generator).GenerateBatches())
				{
					if (foundFirstItemInViewport && IsEndOfCache(isHorizontal, cacheSize.CacheBeforeViewport, cacheUnit, stackPixelSizeInCacheBeforeViewport, stackLogicalSizeInCacheBeforeViewport) && IsEndOfViewport(isHorizontal, viewport, stackPixelSizeInViewport))
					{
						goto IL_081e;
					}
					bool flag3 = false;
					while (true)
					{
						flag3 = false;
						bool flag4 = false;
						bool isAfterFirstItem = false;
						bool isAfterLastItem = false;
						if (IsViewportEmpty(isHorizontal, viewport) && DoubleUtil.GreaterThan(cacheSize.CacheBeforeViewport, 0.0))
						{
							flag4 = true;
						}
						int num5 = firstItemInViewportIndex;
						GeneratorPosition position = IndexToGeneratorPositionForStart(firstItemInViewportIndex, out childIndex);
						num2 = childIndex;
						_firstItemInExtendedViewportIndex = firstItemInViewportIndex;
						_firstItemInExtendedViewportOffset = firstItemInViewportOffset;
						_firstItemInExtendedViewportChildIndex = childIndex;
						using (generator.StartAt(position, GeneratorDirection.Backward, allowStartAtRealizedItem: true))
						{
							for (int num6 = num5; num6 >= 0; num6--)
							{
								object item = items[num6];
								MeasureChild(ref generator, ref itemStorageProvider, ref parentItemStorageProvider, ref parentItem, ref hasUniformOrAverageContainerSizeBeenSet, ref computedUniformOrAverageContainerSize, ref computedUniformOrAverageContainerPixelSize, ref computedAreContainersUniformlySized, ref hasAnyContainerSpanChanged, ref items, ref item, ref children, ref _firstItemInExtendedViewportChildIndex, ref visualOrderChanged, ref isHorizontal, ref childConstraint, ref viewport, ref cacheSize, ref cacheUnit, ref scrollGeneration, ref foundFirstItemInViewport, ref firstItemInViewportOffset, ref stackPixelSize, ref stackPixelSizeInViewport, ref stackPixelSizeInCacheBeforeViewport, ref stackPixelSizeInCacheAfterViewport, ref stackLogicalSize, ref stackLogicalSizeInViewport, ref stackLogicalSizeInCacheBeforeViewport, ref stackLogicalSizeInCacheAfterViewport, ref mustDisableVirtualization, num6 < firstItemInViewportIndex || flag4, isAfterFirstItem, isAfterLastItem, skipActualMeasure: false, skipGeneration: false, ref hasBringIntoViewContainerBeenMeasured, ref hasVirtualizingChildren);
								if (ItemsChangedDuringMeasure)
								{
									remeasure = true;
									goto end_IL_03c4;
								}
								_actualItemsInExtendedViewportCount++;
								if (!foundFirstItemInViewport)
								{
									if (isVSP45Compat)
									{
										SyncUniformSizeFlags(parentItem, parentItemStorageProvider, children, items, itemStorageProvider, count, computedAreContainersUniformlySized, computedUniformOrAverageContainerSize, ref areContainersUniformlySized, ref uniformOrAverageContainerSize, ref hasAverageContainerSizeChanged, isHorizontal, evaluateAreContainersUniformlySized: false);
									}
									else
									{
										SyncUniformSizeFlags(parentItem, parentItemStorageProvider, children, items, itemStorageProvider, count, computedAreContainersUniformlySized, computedUniformOrAverageContainerSize, computedUniformOrAverageContainerPixelSize, ref areContainersUniformlySized, ref uniformOrAverageContainerSize, ref uniformOrAverageContainerPixelSize, ref hasAverageContainerSizeChanged, isHorizontal, evaluateAreContainersUniformlySized: false);
									}
									ComputeFirstItemInViewportIndexAndOffset(items, count, itemStorageProvider, viewport, cacheSize, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, out firstItemInViewportOffset, out firstItemInViewportContainerSpan, out firstItemInViewportIndex, out foundFirstItemInViewport);
									if (!foundFirstItemInViewport)
									{
										break;
									}
									if (num6 != firstItemInViewportIndex)
									{
										stackPixelSize = default(Size);
										stackLogicalSize = default(Size);
										_actualItemsInExtendedViewportCount--;
										flag3 = true;
										break;
									}
									MeasureChild(ref generator, ref itemStorageProvider, ref parentItemStorageProvider, ref parentItem, ref hasUniformOrAverageContainerSizeBeenSet, ref computedUniformOrAverageContainerSize, ref computedUniformOrAverageContainerPixelSize, ref computedAreContainersUniformlySized, ref hasAnyContainerSpanChanged, ref items, ref item, ref children, ref _firstItemInExtendedViewportChildIndex, ref visualOrderChanged, ref isHorizontal, ref childConstraint, ref viewport, ref cacheSize, ref cacheUnit, ref scrollGeneration, ref foundFirstItemInViewport, ref firstItemInViewportOffset, ref stackPixelSize, ref stackPixelSizeInViewport, ref stackPixelSizeInCacheBeforeViewport, ref stackPixelSizeInCacheAfterViewport, ref stackLogicalSize, ref stackLogicalSizeInViewport, ref stackLogicalSizeInCacheBeforeViewport, ref stackLogicalSizeInCacheAfterViewport, ref mustDisableVirtualization, isBeforeFirstItem: false, isAfterFirstItem: false, isAfterLastItem: false, skipActualMeasure: true, skipGeneration: true, ref hasBringIntoViewContainerBeenMeasured, ref hasVirtualizingChildren);
									if (ItemsChangedDuringMeasure)
									{
										remeasure = true;
										goto end_IL_03c4;
									}
								}
								if (!isVSP45Compat && uIElement == null && foundFirstItemInViewport && num6 == num5 && 0 <= num2 && num2 < children.Count)
								{
									uIElement = children[num2] as UIElement;
									if (IsScrolling && _scrollData._firstContainerInViewport != null && !areContainersUniformlySized)
									{
										GetContainerSizeForItem(itemStorageProvider, item, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, out var containerSize);
										double num7 = Math.Max(isHorizontal ? viewport.X : viewport.Y, 0.0);
										double num8 = (isHorizontal ? containerSize.Width : containerSize.Height);
										if (!DoubleUtil.AreClose(num7, 0.0) && !LayoutDoubleUtil.LessThan(num7, firstItemInViewportOffset + num8))
										{
											double num9 = num8 - firstItemInViewportContainerSpan;
											if (!LayoutDoubleUtil.AreClose(num9, 0.0))
											{
												if (isHorizontal)
												{
													_scrollData._offset.X += num9;
												}
												else
												{
													_scrollData._offset.Y += num9;
												}
												if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
												{
													ScrollTracer.Trace(this, ScrollTraceOp.SizeChangeDuringAnchorScroll, "fivOffset:", firstItemInViewportOffset, "vpSpan:", num7, "oldCSpan:", firstItemInViewportContainerSpan, "newCSpan:", num8, "delta:", num9, "newVpOff:", _scrollData._offset);
												}
												remeasure = true;
												goto end_IL_03c4;
											}
										}
									}
								}
								if (IsEndOfCache(isHorizontal, cacheSize.CacheBeforeViewport, cacheUnit, stackPixelSizeInCacheBeforeViewport, stackLogicalSizeInCacheBeforeViewport))
								{
									break;
								}
								_firstItemInExtendedViewportIndex = Math.Max(_firstItemInExtendedViewportIndex - 1, 0);
								IndexToGeneratorPositionForStart(_firstItemInExtendedViewportIndex, out _firstItemInExtendedViewportChildIndex);
								_firstItemInExtendedViewportChildIndex = Math.Max(_firstItemInExtendedViewportChildIndex, 0);
							}
						}
						if (flag3)
						{
							continue;
						}
						ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, 0, _firstItemInExtendedViewportIndex, out _firstItemInExtendedViewportOffset);
						goto IL_081e;
						continue;
						end_IL_03c4:
						break;
					}
					goto end_IL_0396;
					IL_081e:
					if (foundFirstItemInViewport && (!IsEndOfCache(isHorizontal, cacheSize.CacheAfterViewport, cacheUnit, stackPixelSizeInCacheAfterViewport, stackLogicalSizeInCacheAfterViewport) || !IsEndOfViewport(isHorizontal, viewport, stackPixelSizeInViewport)))
					{
						bool isBeforeFirstItem = false;
						bool flag5 = false;
						bool flag6 = false;
						int num10;
						if (IsViewportEmpty(isHorizontal, viewport))
						{
							num10 = 0;
							flag5 = true;
							flag6 = true;
						}
						else
						{
							num10 = firstItemInViewportIndex + 1;
							flag5 = true;
						}
						GeneratorPosition position = IndexToGeneratorPositionForStart(num10, out childIndex);
						using (generator.StartAt(position, GeneratorDirection.Forward, allowStartAtRealizedItem: true))
						{
							int num11 = num10;
							while (num11 < count)
							{
								object item2 = items[num11];
								MeasureChild(ref generator, ref itemStorageProvider, ref parentItemStorageProvider, ref parentItem, ref hasUniformOrAverageContainerSizeBeenSet, ref computedUniformOrAverageContainerSize, ref computedUniformOrAverageContainerPixelSize, ref computedAreContainersUniformlySized, ref hasAnyContainerSpanChanged, ref items, ref item2, ref children, ref childIndex, ref visualOrderChanged, ref isHorizontal, ref childConstraint, ref viewport, ref cacheSize, ref cacheUnit, ref scrollGeneration, ref foundFirstItemInViewport, ref firstItemInViewportOffset, ref stackPixelSize, ref stackPixelSizeInViewport, ref stackPixelSizeInCacheBeforeViewport, ref stackPixelSizeInCacheAfterViewport, ref stackLogicalSize, ref stackLogicalSizeInViewport, ref stackLogicalSizeInCacheBeforeViewport, ref stackLogicalSizeInCacheAfterViewport, ref mustDisableVirtualization, isBeforeFirstItem, num11 > firstItemInViewportIndex || flag5, num11 > num || flag6, skipActualMeasure: false, skipGeneration: false, ref hasBringIntoViewContainerBeenMeasured, ref hasVirtualizingChildren);
								if (!ItemsChangedDuringMeasure)
								{
									_actualItemsInExtendedViewportCount++;
									if (IsEndOfViewport(isHorizontal, viewport, stackPixelSizeInViewport))
									{
										if (!flag2)
										{
											flag2 = true;
											num = num11;
										}
										if (IsEndOfCache(isHorizontal, cacheSize.CacheAfterViewport, cacheUnit, stackPixelSizeInCacheAfterViewport, stackLogicalSizeInCacheAfterViewport))
										{
											break;
										}
									}
									num11++;
									childIndex++;
									continue;
								}
								remeasure = true;
								goto end_IL_0396;
							}
						}
					}
					goto IL_0980;
					end_IL_0396:;
				}
				goto IL_0ddb;
			}
			stackPixelSize = MeasureNonItemsHost(constraint);
			goto end_IL_0073;
			IL_0980:
			if (IsVirtualizing && !IsPixelBased && (hasVirtualizingChildren || virtualizationInfoProvider != null) && (MeasureCaches || (DoubleUtil.AreClose(cacheSize.CacheBeforeViewport, 0.0) && DoubleUtil.AreClose(cacheSize.CacheAfterViewport, 0.0))))
			{
				int num12 = _firstItemInExtendedViewportChildIndex + _actualItemsInExtendedViewportCount;
				int count2 = children.Count;
				int childIndex2 = num12;
				while (childIndex2 < count2)
				{
					MeasureExistingChildBeyondExtendedViewport(ref generator, ref itemStorageProvider, ref parentItemStorageProvider, ref parentItem, ref hasUniformOrAverageContainerSizeBeenSet, ref computedUniformOrAverageContainerSize, ref computedUniformOrAverageContainerPixelSize, ref computedAreContainersUniformlySized, ref hasAnyContainerSpanChanged, ref items, ref children, ref childIndex2, ref visualOrderChanged, ref isHorizontal, ref childConstraint, ref foundFirstItemInViewport, ref firstItemInViewportOffset, ref mustDisableVirtualization, ref hasVirtualizingChildren, ref hasBringIntoViewContainerBeenMeasured, ref scrollGeneration);
					if (!ItemsChangedDuringMeasure)
					{
						childIndex2++;
						continue;
					}
					goto IL_0a29;
				}
			}
			if (_bringIntoViewContainer != null && !hasBringIntoViewContainerBeenMeasured)
			{
				int childIndex = children.IndexOf(_bringIntoViewContainer);
				if (childIndex < 0)
				{
					_bringIntoViewContainer = null;
				}
				else
				{
					MeasureExistingChildBeyondExtendedViewport(ref generator, ref itemStorageProvider, ref parentItemStorageProvider, ref parentItem, ref hasUniformOrAverageContainerSizeBeenSet, ref computedUniformOrAverageContainerSize, ref computedUniformOrAverageContainerPixelSize, ref computedAreContainersUniformlySized, ref hasAnyContainerSpanChanged, ref items, ref children, ref childIndex, ref visualOrderChanged, ref isHorizontal, ref childConstraint, ref foundFirstItemInViewport, ref firstItemInViewportOffset, ref mustDisableVirtualization, ref hasVirtualizingChildren, ref hasBringIntoViewContainerBeenMeasured, ref scrollGeneration);
					if (ItemsChangedDuringMeasure)
					{
						remeasure = true;
						goto IL_0ddb;
					}
				}
			}
			if (isVSP45Compat)
			{
				SyncUniformSizeFlags(parentItem, parentItemStorageProvider, children, items, itemStorageProvider, count, computedAreContainersUniformlySized, computedUniformOrAverageContainerSize, ref areContainersUniformlySized, ref uniformOrAverageContainerSize, ref hasAverageContainerSizeChanged, isHorizontal, evaluateAreContainersUniformlySized: false);
			}
			else
			{
				SyncUniformSizeFlags(parentItem, parentItemStorageProvider, children, items, itemStorageProvider, count, computedAreContainersUniformlySized, computedUniformOrAverageContainerSize, computedUniformOrAverageContainerPixelSize, ref areContainersUniformlySized, ref uniformOrAverageContainerSize, ref uniformOrAverageContainerPixelSize, ref hasAverageContainerSizeChanged, isHorizontal, evaluateAreContainersUniformlySized: false);
			}
			if (IsVirtualizing)
			{
				ExtendPixelAndLogicalSizes(children, items, count, itemStorageProvider, areContainersUniformlySized, uniformOrAverageContainerSize, uniformOrAverageContainerPixelSize, ref stackPixelSize, ref stackLogicalSize, isHorizontal, _firstItemInExtendedViewportIndex, _firstItemInExtendedViewportChildIndex, firstItemInViewportIndex, before: true);
				ExtendPixelAndLogicalSizes(children, items, count, itemStorageProvider, areContainersUniformlySized, uniformOrAverageContainerSize, uniformOrAverageContainerPixelSize, ref stackPixelSize, ref stackLogicalSize, isHorizontal, _firstItemInExtendedViewportIndex + _actualItemsInExtendedViewportCount, _firstItemInExtendedViewportChildIndex + _actualItemsInExtendedViewportCount, -1, before: false);
			}
			_previousStackPixelSizeInViewport = stackPixelSizeInViewport;
			_previousStackLogicalSizeInViewport = stackLogicalSizeInViewport;
			_previousStackPixelSizeInCacheBeforeViewport = stackPixelSizeInCacheBeforeViewport;
			if (!IsPixelBased && DoubleUtil.GreaterThan(isHorizontal ? viewport.Left : viewport.Top, firstItemInViewportOffset))
			{
				IHierarchicalVirtualizationAndScrollInfo virtualizingChild = GetVirtualizingChild(uIElement);
				if (virtualizingChild != null)
				{
					Thickness itemsHostInsetForChild = GetItemsHostInsetForChild(virtualizingChild);
					_pixelDistanceToViewport += (isHorizontal ? itemsHostInsetForChild.Left : itemsHostInsetForChild.Top);
					if (virtualizingChild.ItemsHost is VirtualizingStackPanel virtualizingStackPanel)
					{
						_pixelDistanceToViewport += virtualizingStackPanel._pixelDistanceToViewport;
					}
				}
			}
			if (double.IsInfinity(viewport.Width))
			{
				viewport.Width = stackPixelSize.Width;
			}
			if (double.IsInfinity(viewport.Height))
			{
				viewport.Height = stackPixelSize.Height;
			}
			_extendedViewport = ExtendViewport(virtualizationInfoProvider, isHorizontal, viewport, cacheSize, cacheUnit, stackPixelSizeInCacheBeforeViewport, stackLogicalSizeInCacheBeforeViewport, stackPixelSizeInCacheAfterViewport, stackLogicalSizeInCacheAfterViewport, stackPixelSize, stackLogicalSize, ref _itemsInExtendedViewportCount);
			_viewport = viewport;
			if (virtualizationInfoProvider != null && base.IsVisible)
			{
				virtualizationInfoProvider.ItemDesiredSizes = new HierarchicalVirtualizationItemDesiredSizes(stackLogicalSize, stackLogicalSizeInViewport, stackLogicalSizeInCacheBeforeViewport, stackLogicalSizeInCacheAfterViewport, stackPixelSize, stackPixelSizeInViewport, stackPixelSizeInCacheBeforeViewport, stackPixelSizeInCacheAfterViewport);
				virtualizationInfoProvider.MustDisableVirtualization = mustDisableVirtualization;
			}
			if (MustDisableVirtualization != mustDisableVirtualization)
			{
				MustDisableVirtualization = mustDisableVirtualization;
				remeasure |= IsScrolling;
			}
			double newOffset = 0.0;
			if (!isVSP45Compat)
			{
				if (hasAverageContainerSizeChanged || hasAnyContainerSpanChanged)
				{
					newOffset = ComputeEffectiveOffset(ref viewport, uIElement, firstItemInViewportIndex, firstItemInViewportOffset, items, itemStorageProvider, virtualizationInfoProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, scrollGeneration);
					if (uIElement != null)
					{
						ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, 0, _firstItemInExtendedViewportIndex, out var distance);
						if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
						{
							ScrollTracer.Trace(this, ScrollTraceOp.ReviseArrangeOffset, _firstItemInExtendedViewportOffset, distance);
						}
						_firstItemInExtendedViewportOffset = distance;
					}
					if (!IsScrolling)
					{
						((itemStorageProvider is DependencyObject reference) ? (VisualTreeHelper.GetParent(reference) as Panel) : null)?.InvalidateMeasure();
					}
				}
				if (HasVirtualizingChildren)
				{
					FirstContainerInformation value = new FirstContainerInformation(ref viewport, uIElement, firstItemInViewportIndex, firstItemInViewportOffset, scrollGeneration);
					FirstContainerInformationField.SetValue(this, value);
				}
			}
			if (IsScrolling)
			{
				if (isVSP45Compat)
				{
					SetAndVerifyScrollingData(isHorizontal, viewport, constraint, ref stackPixelSize, ref stackLogicalSize, ref stackPixelSizeInViewport, ref stackLogicalSizeInViewport, ref stackPixelSizeInCacheBeforeViewport, ref stackLogicalSizeInCacheBeforeViewport, ref remeasure, ref lastPageSafeOffset, ref previouslyMeasuredOffsets);
				}
				else
				{
					SetAndVerifyScrollingData(isHorizontal, viewport, constraint, uIElement, firstItemInViewportOffset, hasAverageContainerSizeChanged, newOffset, ref stackPixelSize, ref stackLogicalSize, ref stackPixelSizeInViewport, ref stackLogicalSizeInViewport, ref stackPixelSizeInCacheBeforeViewport, ref stackLogicalSizeInCacheBeforeViewport, ref remeasure, ref lastPageSafeOffset, ref lastPagePixelSize, ref previouslyMeasuredOffsets);
				}
			}
			goto IL_0ddb;
			IL_0a29:
			remeasure = true;
			goto IL_0ddb;
			IL_0ddb:
			if (!remeasure)
			{
				if (IsVirtualizing)
				{
					if (InRecyclingMode)
					{
						DisconnectRecycledContainers();
						if (visualOrderChanged)
						{
							InvalidateZState();
						}
					}
					else
					{
						EnsureCleanupOperation(delay: false);
					}
				}
				HasVirtualizingChildren = hasVirtualizingChildren;
			}
			if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
			{
				EffectiveOffsetInformation effectiveOffsetInformation = ((virtualizationInfoProvider is DependencyObject instance) ? EffectiveOffsetInformationField.GetValue(instance) : null);
				SnapshotData value2 = new SnapshotData
				{
					UniformOrAverageContainerSize = uniformOrAverageContainerPixelSize,
					UniformOrAverageContainerPixelSize = uniformOrAverageContainerPixelSize,
					EffectiveOffsets = effectiveOffsetInformation?.OffsetList
				};
				SnapshotDataField.SetValue(this, value2);
			}
			end_IL_0073:;
		}
		finally
		{
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringEnd, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "VirtualizingStackPanel :MeasureOverride");
			}
		}
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.EndMeasure, stackPixelSize, remeasure);
		}
		if (remeasure)
		{
			if (!IsVSP45Compat && IsScrolling)
			{
				IncrementScrollGeneration();
			}
			return MeasureOverrideImpl(constraint, ref lastPageSafeOffset, ref previouslyMeasuredOffsets, ref lastPagePixelSize, remeasure);
		}
		return stackPixelSize;
	}

	private Size MeasureNonItemsHost(Size constraint)
	{
		return StackPanel.StackMeasureHelper(this, _scrollData, constraint);
	}

	private Size ArrangeNonItemsHost(Size arrangeSize)
	{
		return StackPanel.StackArrangeHelper(this, _scrollData, arrangeSize);
	}

	/// <summary>Arranges the content of a <see cref="T:System.Windows.Controls.VirtualizingStackPanel" /> element.</summary>
	/// <returns>The <see cref="T:System.Windows.Size" /> that represents the arranged size of this <see cref="T:System.Windows.Controls.VirtualizingStackPanel" /> element and its child elements.</returns>
	/// <param name="arrangeSize">The <see cref="T:System.Windows.Size" /> that this element should use to arrange its child elements.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		bool flag = IsScrolling && EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info);
		if (flag)
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringBegin, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "VirtualizingStackPanel :ArrangeOverride");
		}
		try
		{
			if (!base.IsItemsHost)
			{
				ArrangeNonItemsHost(arrangeSize);
			}
			else
			{
				ItemsControl itemsControl = null;
				GroupItem groupItem = null;
				IHierarchicalVirtualizationAndScrollInfo virtualizationInfoProvider = null;
				IContainItemStorage itemStorageProvider = null;
				object parentItem = null;
				bool flag2 = Orientation == Orientation.Horizontal;
				bool mustDisableVirtualization = false;
				GetOwners(shouldSetVirtualizationState: false, flag2, out itemsControl, out groupItem, out itemStorageProvider, out virtualizationInfoProvider, out parentItem, out var parentItemStorageProvider, out mustDisableVirtualization);
				if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
				{
					ScrollTracer.Trace(this, ScrollTraceOp.BeginArrange, arrangeSize, "ptv:", _pixelDistanceToViewport, "ptfc:", _pixelDistanceToFirstContainerInExtendedViewport);
				}
				EnsureGenerator();
				IList realizedChildren = RealizedChildren;
				IItemContainerGenerator generator = base.Generator;
				IList itemsInternal = ((ItemContainerGenerator)generator).ItemsInternal;
				_ = itemsInternal.Count;
				IContainItemStorage itemStorageProvider2 = (IsVSP45Compat ? itemStorageProvider : parentItemStorageProvider);
				bool areContainersUniformlySized = GetAreContainersUniformlySized(itemStorageProvider2, parentItem);
				GetUniformOrAverageContainerSize(itemStorageProvider2, parentItem, IsPixelBased || IsVSP45Compat, out var uniformOrAverageContainerSize, out var uniformOrAverageContainerPixelSize);
				ScrollViewer scrollOwner = ScrollOwner;
				double val = 0.0;
				if (scrollOwner != null && scrollOwner.CanContentScroll)
				{
					val = GetMaxChildArrangeLength(realizedChildren, flag2);
				}
				val = Math.Max(flag2 ? arrangeSize.Height : arrangeSize.Width, val);
				UIElement uIElement = null;
				Size empty = Size.Empty;
				Rect rcChild = new Rect(arrangeSize);
				Size previousChildSize = default(Size);
				int previousChildItemIndex = -1;
				Point previousChildOffset = default(Point);
				bool isVSP45Compat = IsVSP45Compat;
				for (int i = _firstItemInExtendedViewportChildIndex; i < realizedChildren.Count; i++)
				{
					uIElement = (UIElement)realizedChildren[i];
					empty = uIElement.DesiredSize;
					if (i >= _firstItemInExtendedViewportChildIndex && i < _firstItemInExtendedViewportChildIndex + _actualItemsInExtendedViewportCount)
					{
						if (i == _firstItemInExtendedViewportChildIndex)
						{
							ArrangeFirstItemInExtendedViewport(flag2, uIElement, empty, val, ref rcChild, ref previousChildSize, ref previousChildOffset, ref previousChildItemIndex);
							UIElement uIElement2 = null;
							Size empty2 = Size.Empty;
							Rect rcChild2 = rcChild;
							Size previousChildSize2 = uIElement.DesiredSize;
							int previousChildItemIndex2 = previousChildItemIndex;
							Point previousChildOffset2 = previousChildOffset;
							for (int num = _firstItemInExtendedViewportChildIndex - 1; num >= 0; num--)
							{
								uIElement2 = (UIElement)realizedChildren[num];
								empty2 = uIElement2.DesiredSize;
								ArrangeItemsBeyondTheExtendedViewport(flag2, uIElement2, empty2, val, itemsInternal, generator, itemStorageProvider, areContainersUniformlySized, uniformOrAverageContainerSize, beforeExtendedViewport: true, ref rcChild2, ref previousChildSize2, ref previousChildOffset2, ref previousChildItemIndex2);
								if (!isVSP45Compat)
								{
									SetItemsHostInsetForChild(num, uIElement2, itemStorageProvider, flag2);
								}
							}
						}
						else
						{
							ArrangeOtherItemsInExtendedViewport(flag2, uIElement, empty, val, i, ref rcChild, ref previousChildSize, ref previousChildOffset, ref previousChildItemIndex);
						}
					}
					else
					{
						ArrangeItemsBeyondTheExtendedViewport(flag2, uIElement, empty, val, itemsInternal, generator, itemStorageProvider, areContainersUniformlySized, uniformOrAverageContainerSize, beforeExtendedViewport: false, ref rcChild, ref previousChildSize, ref previousChildOffset, ref previousChildItemIndex);
					}
					if (!isVSP45Compat)
					{
						SetItemsHostInsetForChild(i, uIElement, itemStorageProvider, flag2);
					}
				}
				if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
				{
					EffectiveOffsetInformation effectiveOffsetInformation = ((virtualizationInfoProvider is DependencyObject instance) ? EffectiveOffsetInformationField.GetValue(instance) : null);
					SnapshotData value = new SnapshotData
					{
						UniformOrAverageContainerSize = uniformOrAverageContainerPixelSize,
						UniformOrAverageContainerPixelSize = uniformOrAverageContainerPixelSize,
						EffectiveOffsets = effectiveOffsetInformation?.OffsetList
					};
					SnapshotDataField.SetValue(this, value);
					ScrollTracer.Trace(this, ScrollTraceOp.EndArrange, arrangeSize, _firstItemInExtendedViewportIndex, _firstItemInExtendedViewportOffset);
				}
			}
		}
		finally
		{
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringEnd, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "VirtualizingStackPanel :ArrangeOverride");
			}
		}
		return arrangeSize;
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with the <see cref="T:System.Windows.Controls.ItemsControl" /> for this <see cref="T:System.Windows.Controls.Panel" /> changes.</summary>
	/// <param name="sender">The <see cref="T:System.Object" /> that raised the event.</param>
	/// <param name="args">Provides data for the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged" /> event.</param>
	protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
	{
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.ItemsChanged, args.Action, "stpos:", args.Position, base.Generator.IndexFromGeneratorPosition(args.Position), "oldpos:", args.OldPosition, base.Generator.IndexFromGeneratorPosition(args.OldPosition), "count:", args.ItemCount, args.ItemUICount, "ev:", _firstItemInExtendedViewportIndex, "+", _itemsInExtendedViewportCount, "ext:", IsScrolling ? _scrollData._extent : Size.Empty, base.MeasureInProgress ? "MeasureInProgress" : string.Empty);
		}
		if (base.MeasureInProgress)
		{
			ItemsChangedDuringMeasure = true;
		}
		base.OnItemsChanged(sender, args);
		bool flag = false;
		switch (args.Action)
		{
		case NotifyCollectionChangedAction.Remove:
			OnItemsRemove(args);
			flag = true;
			break;
		case NotifyCollectionChangedAction.Replace:
			OnItemsReplace(args);
			flag = true;
			break;
		case NotifyCollectionChangedAction.Move:
			OnItemsMove(args);
			break;
		case NotifyCollectionChangedAction.Reset:
			flag = true;
			GetItemStorageProvider(this).Clear();
			ClearAsyncOperations();
			break;
		}
		if (flag && IsScrolling)
		{
			ResetMaximumDesiredSize();
		}
	}

	internal void ResetMaximumDesiredSize()
	{
		if (IsScrolling)
		{
			_scrollData._maxDesiredSize = default(Size);
		}
	}

	/// <summary>Returns a value that indicates whether a changed item in an <see cref="T:System.Windows.Controls.ItemsControl" /> affects the layout for this panel.</summary>
	/// <returns>true if the changed item in an <see cref="T:System.Windows.Controls.ItemsControl" /> affects the layout for this panel; otherwise, false.</returns>
	/// <param name="areItemChangesLocal">true if the changed item is a direct child of this <see cref="T:System.Windows.Controls.VirtualizingPanel" />; false if the changed item is an indirect descendant of the <see cref="T:System.Windows.Controls.VirtualizingPanel" />.</param>
	/// <param name="args">Contains data regarding the changed item.</param>
	protected override bool ShouldItemsChangeAffectLayoutCore(bool areItemChangesLocal, ItemsChangedEventArgs args)
	{
		bool flag = true;
		if (IsVirtualizing)
		{
			if (areItemChangesLocal)
			{
				switch (args.Action)
				{
				case NotifyCollectionChangedAction.Remove:
				{
					int num3 = base.Generator.IndexFromGeneratorPosition(args.Position);
					flag = args.ItemUICount > 0 || num3 < _firstItemInExtendedViewportIndex + _itemsInExtendedViewportCount;
					break;
				}
				case NotifyCollectionChangedAction.Replace:
					flag = args.ItemUICount > 0;
					break;
				case NotifyCollectionChangedAction.Add:
					flag = base.Generator.IndexFromGeneratorPosition(args.Position) < _firstItemInExtendedViewportIndex + _itemsInExtendedViewportCount;
					break;
				case NotifyCollectionChangedAction.Move:
				{
					int num = base.Generator.IndexFromGeneratorPosition(args.Position);
					int num2 = base.Generator.IndexFromGeneratorPosition(args.OldPosition);
					flag = num < _firstItemInExtendedViewportIndex + _itemsInExtendedViewportCount || num2 < _firstItemInExtendedViewportIndex + _itemsInExtendedViewportCount;
					break;
				}
				}
			}
			else
			{
				flag = base.Generator.IndexFromGeneratorPosition(args.Position) != _firstItemInExtendedViewportIndex + _itemsInExtendedViewportCount - 1;
			}
			if (!flag)
			{
				if (IsScrolling)
				{
					flag = !IsExtendedViewportFull();
					if (!flag)
					{
						UpdateExtent(areItemChangesLocal);
					}
				}
				else
				{
					DependencyObject itemsOwnerInternal = ItemsControl.GetItemsOwnerInternal(this);
					if (VisualTreeHelper.GetParent(itemsOwnerInternal) is VirtualizingPanel virtualizingPanel)
					{
						UpdateExtent(areItemChangesLocal);
						IItemContainerGenerator itemContainerGenerator = virtualizingPanel.ItemContainerGenerator;
						int itemIndex = ((ItemContainerGenerator)itemContainerGenerator).IndexFromContainer(itemsOwnerInternal, returnLocalIndex: true);
						ItemsChangedEventArgs args2 = new ItemsChangedEventArgs(NotifyCollectionChangedAction.Reset, itemContainerGenerator.GeneratorPositionFromIndex(itemIndex), 1, 1);
						flag = virtualizingPanel.ShouldItemsChangeAffectLayout(areItemChangesLocal: false, args2);
					}
					else
					{
						flag = true;
					}
				}
			}
		}
		return flag;
	}

	private void UpdateExtent(bool areItemChangesLocal)
	{
		bool flag = Orientation == Orientation.Horizontal;
		bool isVSP45Compat = IsVSP45Compat;
		bool flag2 = ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this);
		GetOwners(shouldSetVirtualizationState: false, flag, out var _, out var _, out var itemStorageProvider, out var virtualizationInfoProvider, out var parentItem, out var parentItemStorageProvider, out var _);
		IContainItemStorage itemStorageProvider2 = (isVSP45Compat ? itemStorageProvider : parentItemStorageProvider);
		bool areContainersUniformlySized = GetAreContainersUniformlySized(itemStorageProvider2, parentItem);
		GetUniformOrAverageContainerSize(itemStorageProvider2, parentItem, isVSP45Compat || IsPixelBased, out var uniformOrAverageContainerSize, out var uniformOrAverageContainerPixelSize);
		IList realizedChildren = RealizedChildren;
		IList itemsInternal = ((ItemContainerGenerator)base.Generator).ItemsInternal;
		int count = itemsInternal.Count;
		if (!areItemChangesLocal)
		{
			double computedUniformOrAverageContainerSize = uniformOrAverageContainerSize;
			double computedUniformOrAverageContainerPixelSize = uniformOrAverageContainerPixelSize;
			bool computedAreContainersUniformlySized = areContainersUniformlySized;
			bool hasAverageContainerSizeChanged = false;
			if (isVSP45Compat)
			{
				SyncUniformSizeFlags(parentItem, parentItemStorageProvider, realizedChildren, itemsInternal, itemStorageProvider, count, computedAreContainersUniformlySized, computedUniformOrAverageContainerSize, ref areContainersUniformlySized, ref uniformOrAverageContainerSize, ref hasAverageContainerSizeChanged, flag, evaluateAreContainersUniformlySized: true);
			}
			else
			{
				SyncUniformSizeFlags(parentItem, parentItemStorageProvider, realizedChildren, itemsInternal, itemStorageProvider, count, computedAreContainersUniformlySized, computedUniformOrAverageContainerSize, computedUniformOrAverageContainerPixelSize, ref areContainersUniformlySized, ref uniformOrAverageContainerSize, ref uniformOrAverageContainerPixelSize, ref hasAverageContainerSizeChanged, flag, evaluateAreContainersUniformlySized: true);
			}
			if (hasAverageContainerSizeChanged && !IsVSP45Compat)
			{
				FirstContainerInformation value = FirstContainerInformationField.GetValue(this);
				if (value != null)
				{
					ComputeEffectiveOffset(ref value.Viewport, value.FirstContainer, value.FirstItemIndex, value.FirstItemOffset, itemsInternal, itemStorageProvider, virtualizationInfoProvider, flag, areContainersUniformlySized, uniformOrAverageContainerSize, value.ScrollGeneration);
				}
			}
		}
		double distance = 0.0;
		ComputeDistance(itemsInternal, itemStorageProvider, flag, areContainersUniformlySized, uniformOrAverageContainerSize, 0, itemsInternal.Count, out distance);
		if (IsScrolling)
		{
			if (flag)
			{
				_scrollData._extent.Width = distance;
			}
			else
			{
				_scrollData._extent.Height = distance;
			}
			if (flag2)
			{
				ScrollTracer.Trace(this, ScrollTraceOp.UpdateExtent, "ext:", _scrollData._extent);
			}
			ScrollOwner.InvalidateScrollInfo();
		}
		else
		{
			if (virtualizationInfoProvider == null)
			{
				return;
			}
			HierarchicalVirtualizationItemDesiredSizes itemDesiredSizes = virtualizationInfoProvider.ItemDesiredSizes;
			if (IsPixelBased)
			{
				Size pixelSize = itemDesiredSizes.PixelSize;
				if (flag)
				{
					pixelSize.Width = distance;
				}
				else
				{
					pixelSize.Height = distance;
				}
				if (flag2)
				{
					ScrollTracer.Trace(this, ScrollTraceOp.UpdateExtent, "ids.Px:", pixelSize);
				}
				itemDesiredSizes = new HierarchicalVirtualizationItemDesiredSizes(itemDesiredSizes.LogicalSize, itemDesiredSizes.LogicalSizeInViewport, itemDesiredSizes.LogicalSizeBeforeViewport, itemDesiredSizes.LogicalSizeAfterViewport, pixelSize, itemDesiredSizes.PixelSizeInViewport, itemDesiredSizes.PixelSizeBeforeViewport, itemDesiredSizes.PixelSizeAfterViewport);
			}
			else
			{
				Size logicalSize = itemDesiredSizes.LogicalSize;
				if (flag)
				{
					logicalSize.Width = distance;
				}
				else
				{
					logicalSize.Height = distance;
				}
				if (flag2)
				{
					ScrollTracer.Trace(this, ScrollTraceOp.UpdateExtent, "ids.Lg:", logicalSize);
				}
				itemDesiredSizes = new HierarchicalVirtualizationItemDesiredSizes(logicalSize, itemDesiredSizes.LogicalSizeInViewport, itemDesiredSizes.LogicalSizeBeforeViewport, itemDesiredSizes.LogicalSizeAfterViewport, itemDesiredSizes.PixelSize, itemDesiredSizes.PixelSizeInViewport, itemDesiredSizes.PixelSizeBeforeViewport, itemDesiredSizes.PixelSizeAfterViewport);
			}
			virtualizationInfoProvider.ItemDesiredSizes = itemDesiredSizes;
		}
	}

	private bool IsExtendedViewportFull()
	{
		bool flag = Orientation == Orientation.Horizontal;
		if ((flag && DoubleUtil.GreaterThanOrClose(base.DesiredSize.Width, base.PreviousConstraint.Width)) || (!flag && DoubleUtil.GreaterThanOrClose(base.DesiredSize.Height, base.PreviousConstraint.Height)))
		{
			IHierarchicalVirtualizationAndScrollInfo virtualizationInfoProvider = null;
			Rect viewport = _viewport;
			Rect extendedViewport = _extendedViewport;
			Rect empty = Rect.Empty;
			VirtualizationCacheLength cacheLength = VirtualizingPanel.GetCacheLength(this);
			VirtualizationCacheLengthUnit cacheUnit = VirtualizingPanel.GetCacheLengthUnit(this);
			int itemsInExtendedViewportCount = _itemsInExtendedViewportCount;
			NormalizeCacheLength(flag, viewport, ref cacheLength, ref cacheUnit);
			empty = ExtendViewport(virtualizationInfoProvider, flag, viewport, cacheLength, cacheUnit, Size.Empty, Size.Empty, Size.Empty, Size.Empty, Size.Empty, Size.Empty, ref itemsInExtendedViewportCount);
			if (!flag || !DoubleUtil.GreaterThanOrClose(extendedViewport.Width, empty.Width))
			{
				if (!flag)
				{
					return DoubleUtil.GreaterThanOrClose(extendedViewport.Height, empty.Height);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	/// <summary>Called when the collection of child elements is cleared by the base <see cref="T:System.Windows.Controls.Panel" /> class.</summary>
	protected override void OnClearChildren()
	{
		base.OnClearChildren();
		if (IsVirtualizing && base.IsItemsHost)
		{
			ItemsControl.GetItemsOwnerInternal(this, out var itemsControl);
			CleanupContainers(int.MaxValue, int.MaxValue, itemsControl);
		}
		if (_realizedChildren != null)
		{
			_realizedChildren.Clear();
		}
		base.InternalChildren.ClearInternal();
	}

	private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(bool)e.NewValue)
		{
			IHierarchicalVirtualizationAndScrollInfo virtualizingProvider = GetVirtualizingProvider();
			if (virtualizingProvider != null)
			{
				Helper.ClearVirtualizingElement(virtualizingProvider);
			}
			ClearAsyncOperations();
		}
		else
		{
			InvalidateMeasure();
		}
	}

	internal void ClearAllContainers()
	{
		base.Generator?.RemoveAll();
	}

	private IHierarchicalVirtualizationAndScrollInfo GetVirtualizingProvider()
	{
		ItemsControl itemsControl = null;
		DependencyObject itemsOwnerInternal = ItemsControl.GetItemsOwnerInternal(this, out itemsControl);
		if (itemsOwnerInternal is GroupItem)
		{
			return GetVirtualizingProvider(itemsOwnerInternal);
		}
		return GetVirtualizingProvider(itemsControl);
	}

	private static IHierarchicalVirtualizationAndScrollInfo GetVirtualizingProvider(DependencyObject element)
	{
		IHierarchicalVirtualizationAndScrollInfo hierarchicalVirtualizationAndScrollInfo = element as IHierarchicalVirtualizationAndScrollInfo;
		if (hierarchicalVirtualizationAndScrollInfo != null && !(VisualTreeHelper.GetParent(element) is VirtualizingPanel { CanHierarchicallyScrollAndVirtualize: not false }))
		{
			hierarchicalVirtualizationAndScrollInfo = null;
		}
		return hierarchicalVirtualizationAndScrollInfo;
	}

	private static IHierarchicalVirtualizationAndScrollInfo GetVirtualizingChild(DependencyObject element)
	{
		bool isChildHorizontal = false;
		return GetVirtualizingChild(element, ref isChildHorizontal);
	}

	private static IHierarchicalVirtualizationAndScrollInfo GetVirtualizingChild(DependencyObject element, ref bool isChildHorizontal)
	{
		IHierarchicalVirtualizationAndScrollInfo hierarchicalVirtualizationAndScrollInfo = element as IHierarchicalVirtualizationAndScrollInfo;
		if (hierarchicalVirtualizationAndScrollInfo != null && hierarchicalVirtualizationAndScrollInfo.ItemsHost != null)
		{
			isChildHorizontal = hierarchicalVirtualizationAndScrollInfo.ItemsHost.LogicalOrientationPublic == Orientation.Horizontal;
			if (!(hierarchicalVirtualizationAndScrollInfo.ItemsHost is VirtualizingPanel { CanHierarchicallyScrollAndVirtualize: not false }))
			{
				hierarchicalVirtualizationAndScrollInfo = null;
			}
		}
		return hierarchicalVirtualizationAndScrollInfo;
	}

	private static IContainItemStorage GetItemStorageProvider(Panel itemsHost)
	{
		ItemsControl itemsControl = null;
		DependencyObject itemsOwnerInternal = ItemsControl.GetItemsOwnerInternal(itemsHost, out itemsControl);
		return itemsOwnerInternal as IContainItemStorage;
	}

	private void GetOwners(bool shouldSetVirtualizationState, bool isHorizontal, out ItemsControl itemsControl, out GroupItem groupItem, out IContainItemStorage itemStorageProvider, out IHierarchicalVirtualizationAndScrollInfo virtualizationInfoProvider, out object parentItem, out IContainItemStorage parentItemStorageProvider, out bool mustDisableVirtualization)
	{
		groupItem = null;
		parentItem = null;
		parentItemStorageProvider = null;
		bool isScrolling = IsScrolling;
		mustDisableVirtualization = isScrolling && MustDisableVirtualization;
		ItemsControl itemsControl2 = null;
		DependencyObject itemsOwnerInternal = ItemsControl.GetItemsOwnerInternal(this, out itemsControl);
		if (itemsOwnerInternal != itemsControl)
		{
			groupItem = itemsOwnerInternal as GroupItem;
			parentItem = itemsControl.ItemContainerGenerator.ItemFromContainer(groupItem);
		}
		else if (!isScrolling)
		{
			if (ItemsControl.GetItemsOwnerInternal(VisualTreeHelper.GetParent(itemsControl)) is ItemsControl itemsControl3)
			{
				parentItem = itemsControl3.ItemContainerGenerator.ItemFromContainer(itemsControl);
			}
			else
			{
				parentItem = this;
			}
		}
		else
		{
			parentItem = this;
		}
		itemStorageProvider = itemsOwnerInternal as IContainItemStorage;
		virtualizationInfoProvider = null;
		parentItemStorageProvider = ((IsVSP45Compat || isScrolling || itemsOwnerInternal == null) ? null : (ItemsControl.GetItemsOwnerInternal(VisualTreeHelper.GetParent(itemsOwnerInternal)) as IContainItemStorage));
		if (groupItem != null)
		{
			virtualizationInfoProvider = GetVirtualizingProvider(groupItem);
			mustDisableVirtualization = virtualizationInfoProvider != null && virtualizationInfoProvider.MustDisableVirtualization;
		}
		else if (!isScrolling)
		{
			virtualizationInfoProvider = GetVirtualizingProvider(itemsControl);
			mustDisableVirtualization = virtualizationInfoProvider != null && virtualizationInfoProvider.MustDisableVirtualization;
		}
		if (shouldSetVirtualizationState)
		{
			if (ScrollTracer.IsEnabled)
			{
				ScrollTracer.ConfigureTracing(this, itemsOwnerInternal, parentItem, itemsControl);
			}
			SetVirtualizationState(itemStorageProvider, itemsControl, mustDisableVirtualization);
		}
	}

	private void SetVirtualizationState(IContainItemStorage itemStorageProvider, ItemsControl itemsControl, bool mustDisableVirtualization)
	{
		if (itemsControl == null)
		{
			return;
		}
		bool isVirtualizing = VirtualizingPanel.GetIsVirtualizing(itemsControl);
		bool isVirtualizingWhenGrouping = VirtualizingPanel.GetIsVirtualizingWhenGrouping(itemsControl);
		VirtualizationMode virtualizationMode = VirtualizingPanel.GetVirtualizationMode(itemsControl);
		bool isGrouping = itemsControl.IsGrouping;
		IsVirtualizing = !mustDisableVirtualization && ((!isGrouping && isVirtualizing) || (isGrouping && isVirtualizing && isVirtualizingWhenGrouping));
		ScrollUnit scrollUnit = VirtualizingPanel.GetScrollUnit(itemsControl);
		bool isPixelBased = IsPixelBased;
		IsPixelBased = mustDisableVirtualization || scrollUnit == ScrollUnit.Pixel;
		if (IsScrolling)
		{
			if (!HasMeasured || isPixelBased != IsPixelBased)
			{
				ClearItemValueStorageRecursive(itemStorageProvider, this);
			}
			VirtualizingPanel.SetCacheLength(this, VirtualizingPanel.GetCacheLength(itemsControl));
			VirtualizingPanel.SetCacheLengthUnit(this, VirtualizingPanel.GetCacheLengthUnit(itemsControl));
		}
		if (HasMeasured)
		{
			if ((InRecyclingMode ? 1 : 0) != (int)virtualizationMode)
			{
				throw new InvalidOperationException(SR.CantSwitchVirtualizationModePostMeasure);
			}
		}
		else
		{
			HasMeasured = true;
		}
		InRecyclingMode = virtualizationMode == VirtualizationMode.Recycling;
	}

	private static void ClearItemValueStorageRecursive(IContainItemStorage itemStorageProvider, Panel itemsHost)
	{
		Helper.ClearItemValueStorage((DependencyObject)itemStorageProvider, _indicesStoredInItemValueStorage);
		UIElementCollection internalChildren = itemsHost.InternalChildren;
		int count = internalChildren.Count;
		for (int i = 0; i < count; i++)
		{
			if (internalChildren[i] is IHierarchicalVirtualizationAndScrollInfo { ItemsHost: { } itemsHost2 })
			{
				IContainItemStorage itemStorageProvider2 = GetItemStorageProvider(itemsHost2);
				if (itemStorageProvider2 != null)
				{
					ClearItemValueStorageRecursive(itemStorageProvider2, itemsHost2);
				}
			}
		}
	}

	private void InitializeViewport(object parentItem, IContainItemStorage parentItemStorageProvider, IHierarchicalVirtualizationAndScrollInfo virtualizationInfoProvider, bool isHorizontal, Size constraint, ref Rect viewport, ref VirtualizationCacheLength cacheSize, ref VirtualizationCacheLengthUnit cacheUnit, out Rect extendedViewport, out long scrollGeneration)
	{
		Size size = default(Size);
		bool isVSP45Compat = IsVSP45Compat;
		if (IsScrolling)
		{
			Size size2 = constraint;
			double x = _scrollData._offset.X;
			double y = _scrollData._offset.Y;
			size = _scrollData._extent;
			Size viewport2 = _scrollData._viewport;
			scrollGeneration = _scrollData._scrollGeneration;
			if (!IsScrollActive || IgnoreMaxDesiredSize)
			{
				_scrollData._maxDesiredSize = default(Size);
			}
			if (IsPixelBased)
			{
				viewport = new Rect(x, y, size2.Width, size2.Height);
				CoerceScrollingViewportOffset(ref viewport, size, isHorizontal);
			}
			else
			{
				viewport = new Rect(x, y, viewport2.Width, viewport2.Height);
				CoerceScrollingViewportOffset(ref viewport, size, isHorizontal);
				viewport.Size = size2;
			}
			if (IsVirtualizing)
			{
				cacheSize = VirtualizingPanel.GetCacheLength(this);
				cacheUnit = VirtualizingPanel.GetCacheLengthUnit(this);
				if (DoubleUtil.GreaterThanZero(cacheSize.CacheBeforeViewport) || DoubleUtil.GreaterThanZero(cacheSize.CacheAfterViewport))
				{
					if (!MeasureCaches)
					{
						WasLastMeasurePassAnchored = _scrollData._firstContainerInViewport != null || _scrollData._bringIntoViewLeafContainer != null;
						DispatcherOperation value = MeasureCachesOperationField.GetValue(this);
						if (value == null)
						{
							Action measureCachesAction = null;
							int retryCount = 3;
							measureCachesAction = delegate
							{
								bool flag = 0 < retryCount-- && (base.MeasureDirty || base.ArrangeDirty);
								try
								{
									if (isVSP45Compat || !flag)
									{
										MeasureCachesOperationField.ClearValue(this);
										MeasureCaches = true;
										if (WasLastMeasurePassAnchored)
										{
											SetAnchorInformation(isHorizontal);
										}
										InvalidateMeasure();
										UpdateLayout();
									}
								}
								finally
								{
									flag = flag || (0 < retryCount && (base.MeasureDirty || base.ArrangeDirty));
									if (!isVSP45Compat && flag)
									{
										MeasureCachesOperationField.SetValue(this, base.Dispatcher.BeginInvoke(DispatcherPriority.Background, measureCachesAction));
									}
									MeasureCaches = false;
									if (AnchoredInvalidateMeasureOperationField.GetValue(this) == null && (isVSP45Compat || !flag))
									{
										if (isVSP45Compat)
										{
											IsScrollActive = false;
										}
										else if (IsScrollActive)
										{
											ClearIsScrollActiveOperationField.GetValue(this)?.Abort();
											DispatcherOperation value2 = base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(ClearIsScrollActive));
											ClearIsScrollActiveOperationField.SetValue(this, value2);
										}
									}
								}
							};
							value = base.Dispatcher.BeginInvoke(DispatcherPriority.Background, measureCachesAction);
							MeasureCachesOperationField.SetValue(this, value);
						}
					}
				}
				else if (IsScrollActive)
				{
					DispatcherOperation value3 = ClearIsScrollActiveOperationField.GetValue(this);
					if (value3 == null)
					{
						value3 = base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(ClearIsScrollActive));
						ClearIsScrollActiveOperationField.SetValue(this, value3);
					}
				}
				NormalizeCacheLength(isHorizontal, viewport, ref cacheSize, ref cacheUnit);
			}
			else
			{
				cacheSize = new VirtualizationCacheLength(double.PositiveInfinity, IsViewportEmpty(isHorizontal, viewport) ? 0.0 : double.PositiveInfinity);
				cacheUnit = VirtualizationCacheLengthUnit.Pixel;
				ClearAsyncOperations();
			}
		}
		else if (virtualizationInfoProvider != null)
		{
			HierarchicalVirtualizationConstraints constraints = virtualizationInfoProvider.Constraints;
			viewport = constraints.Viewport;
			cacheSize = constraints.CacheLength;
			cacheUnit = constraints.CacheLengthUnit;
			scrollGeneration = constraints.ScrollGeneration;
			MeasureCaches = virtualizationInfoProvider.InBackgroundLayout;
			if (isVSP45Compat)
			{
				AdjustNonScrollingViewportForHeader(virtualizationInfoProvider, ref viewport, ref cacheSize, ref cacheUnit);
			}
			else
			{
				AdjustNonScrollingViewportForInset(isHorizontal, parentItem, parentItemStorageProvider, virtualizationInfoProvider, ref viewport, ref cacheSize, ref cacheUnit);
				DependencyObject instance = virtualizationInfoProvider as DependencyObject;
				EffectiveOffsetInformation value4 = EffectiveOffsetInformationField.GetValue(instance);
				if (value4 != null)
				{
					List<double> offsetList = value4.OffsetList;
					int num = -1;
					if (value4.ScrollGeneration >= scrollGeneration)
					{
						double value5 = (isHorizontal ? viewport.X : viewport.Y);
						int i = 0;
						for (int count = offsetList.Count; i < count; i++)
						{
							if (LayoutDoubleUtil.AreClose(value5, offsetList[i]))
							{
								num = i;
								break;
							}
						}
					}
					if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
					{
						object[] array = new object[offsetList.Count + 7];
						array[0] = "gen";
						array[1] = value4.ScrollGeneration;
						array[2] = constraints.ScrollGeneration;
						array[3] = viewport.Location;
						array[4] = "at";
						array[5] = num;
						array[6] = "in";
						for (int j = 0; j < offsetList.Count; j++)
						{
							array[j + 7] = offsetList[j];
						}
						ScrollTracer.Trace(this, ScrollTraceOp.UseSubstOffset, array);
					}
					if (num >= 0)
					{
						if (isHorizontal)
						{
							viewport.X = offsetList[offsetList.Count - 1];
						}
						else
						{
							viewport.Y = offsetList[offsetList.Count - 1];
						}
						offsetList.RemoveRange(0, num);
					}
					if (num < 0 || offsetList.Count <= 1)
					{
						EffectiveOffsetInformationField.ClearValue(instance);
					}
				}
			}
		}
		else
		{
			scrollGeneration = 0L;
			viewport = new Rect(0.0, 0.0, constraint.Width, constraint.Height);
			if (isHorizontal)
			{
				viewport.Width = double.PositiveInfinity;
			}
			else
			{
				viewport.Height = double.PositiveInfinity;
			}
		}
		extendedViewport = _extendedViewport;
		if (isHorizontal)
		{
			extendedViewport.X += viewport.X - _viewport.X;
		}
		else
		{
			extendedViewport.Y += viewport.Y - _viewport.Y;
		}
		if (IsVirtualizing)
		{
			if (MeasureCaches)
			{
				IsMeasureCachesPending = false;
			}
			else if (DoubleUtil.GreaterThanZero(cacheSize.CacheBeforeViewport) || DoubleUtil.GreaterThanZero(cacheSize.CacheAfterViewport))
			{
				IsMeasureCachesPending = true;
			}
		}
	}

	private void ClearMeasureCachesState()
	{
		DispatcherOperation value = MeasureCachesOperationField.GetValue(this);
		if (value != null)
		{
			value.Abort();
			MeasureCachesOperationField.ClearValue(this);
		}
		IsMeasureCachesPending = false;
		if (_cleanupOperation != null && _cleanupOperation.Abort())
		{
			_cleanupOperation = null;
		}
		if (_cleanupDelay != null)
		{
			_cleanupDelay.Stop();
			_cleanupDelay = null;
		}
	}

	private void ClearIsScrollActive()
	{
		ClearIsScrollActiveOperationField.ClearValue(this);
		OffsetInformationField.ClearValue(this);
		_scrollData._bringIntoViewLeafContainer = null;
		IsScrollActive = false;
		if (!IsVSP45Compat)
		{
			_scrollData._offset = _scrollData._computedOffset;
		}
	}

	private void NormalizeCacheLength(bool isHorizontal, Rect viewport, ref VirtualizationCacheLength cacheLength, ref VirtualizationCacheLengthUnit cacheUnit)
	{
		if (cacheUnit == VirtualizationCacheLengthUnit.Page)
		{
			double num = (isHorizontal ? viewport.Width : viewport.Height);
			if (double.IsPositiveInfinity(num))
			{
				cacheLength = new VirtualizationCacheLength(0.0, 0.0);
			}
			else
			{
				cacheLength = new VirtualizationCacheLength(cacheLength.CacheBeforeViewport * num, cacheLength.CacheAfterViewport * num);
			}
			cacheUnit = VirtualizationCacheLengthUnit.Pixel;
		}
		if (IsViewportEmpty(isHorizontal, viewport))
		{
			cacheLength = new VirtualizationCacheLength(0.0, 0.0);
		}
	}

	private Rect ExtendViewport(IHierarchicalVirtualizationAndScrollInfo virtualizationInfoProvider, bool isHorizontal, Rect viewport, VirtualizationCacheLength cacheLength, VirtualizationCacheLengthUnit cacheUnit, Size stackPixelSizeInCacheBeforeViewport, Size stackLogicalSizeInCacheBeforeViewport, Size stackPixelSizeInCacheAfterViewport, Size stackLogicalSizeInCacheAfterViewport, Size stackPixelSize, Size stackLogicalSize, ref int itemsInExtendedViewportCount)
	{
		Rect rect = viewport;
		if (isHorizontal)
		{
			double num = ((DoubleUtil.GreaterThanZero(_previousStackPixelSizeInViewport.Width) && DoubleUtil.GreaterThanZero(_previousStackLogicalSizeInViewport.Width)) ? (_previousStackPixelSizeInViewport.Width / _previousStackLogicalSizeInViewport.Width) : 16.0);
			double width = stackPixelSize.Width;
			double width2 = stackLogicalSize.Width;
			double num2;
			double num3;
			double num4;
			if (MeasureCaches)
			{
				num2 = stackPixelSizeInCacheBeforeViewport.Width;
				num3 = stackPixelSizeInCacheAfterViewport.Width;
				num4 = stackLogicalSizeInCacheBeforeViewport.Width;
				_ = stackLogicalSizeInCacheAfterViewport.Width;
			}
			else
			{
				num2 = ((cacheUnit == VirtualizationCacheLengthUnit.Item) ? (cacheLength.CacheBeforeViewport * num) : cacheLength.CacheBeforeViewport);
				num3 = ((cacheUnit == VirtualizationCacheLengthUnit.Item) ? (cacheLength.CacheAfterViewport * num) : cacheLength.CacheAfterViewport);
				num4 = ((cacheUnit == VirtualizationCacheLengthUnit.Item) ? cacheLength.CacheBeforeViewport : (cacheLength.CacheBeforeViewport / num));
				if (cacheUnit != VirtualizationCacheLengthUnit.Item)
				{
					_ = cacheLength.CacheAfterViewport / num;
				}
				else
				{
					_ = cacheLength.CacheAfterViewport;
				}
				if (IsPixelBased)
				{
					num2 = Math.Max(num2, Math.Abs(_viewport.X - _extendedViewport.X));
				}
				else
				{
					num4 = Math.Max(num4, Math.Abs(_viewport.X - _extendedViewport.X));
				}
			}
			if (IsPixelBased)
			{
				if (!IsScrolling && virtualizationInfoProvider != null && IsViewportEmpty(isHorizontal, rect) && DoubleUtil.GreaterThanZero(num2))
				{
					rect.X = width - num2;
				}
				else
				{
					rect.X -= num2;
				}
				rect.Width += num2 + num3;
				if (IsScrolling)
				{
					if (DoubleUtil.LessThan(rect.X, 0.0))
					{
						rect.Width = Math.Max(rect.Width + rect.X, 0.0);
						rect.X = 0.0;
					}
					if (DoubleUtil.GreaterThan(rect.X + rect.Width, _scrollData._extent.Width))
					{
						rect.Width = Math.Max(_scrollData._extent.Width - rect.X, 0.0);
					}
				}
			}
			else
			{
				if (!IsScrolling && virtualizationInfoProvider != null && IsViewportEmpty(isHorizontal, rect) && DoubleUtil.GreaterThanZero(num2))
				{
					rect.X = width2 - num4;
				}
				else
				{
					rect.X -= num4;
				}
				rect.Width += num2 + num3;
				if (IsScrolling)
				{
					if (DoubleUtil.LessThan(rect.X, 0.0))
					{
						rect.Width = Math.Max(rect.Width / num + rect.X, 0.0) * num;
						rect.X = 0.0;
					}
					if (DoubleUtil.GreaterThan(rect.X + rect.Width / num, _scrollData._extent.Width))
					{
						rect.Width = Math.Max(_scrollData._extent.Width - rect.X, 0.0) * num;
					}
				}
			}
		}
		else
		{
			double num5 = ((DoubleUtil.GreaterThanZero(_previousStackPixelSizeInViewport.Height) && DoubleUtil.GreaterThanZero(_previousStackLogicalSizeInViewport.Height)) ? (_previousStackPixelSizeInViewport.Height / _previousStackLogicalSizeInViewport.Height) : 16.0);
			double width = stackPixelSize.Height;
			double width2 = stackLogicalSize.Height;
			double num2;
			double num3;
			double num4;
			if (MeasureCaches)
			{
				num2 = stackPixelSizeInCacheBeforeViewport.Height;
				num3 = stackPixelSizeInCacheAfterViewport.Height;
				num4 = stackLogicalSizeInCacheBeforeViewport.Height;
				_ = stackLogicalSizeInCacheAfterViewport.Height;
			}
			else
			{
				num2 = ((cacheUnit == VirtualizationCacheLengthUnit.Item) ? (cacheLength.CacheBeforeViewport * num5) : cacheLength.CacheBeforeViewport);
				num3 = ((cacheUnit == VirtualizationCacheLengthUnit.Item) ? (cacheLength.CacheAfterViewport * num5) : cacheLength.CacheAfterViewport);
				num4 = ((cacheUnit == VirtualizationCacheLengthUnit.Item) ? cacheLength.CacheBeforeViewport : (cacheLength.CacheBeforeViewport / num5));
				if (cacheUnit != VirtualizationCacheLengthUnit.Item)
				{
					_ = cacheLength.CacheAfterViewport / num5;
				}
				else
				{
					_ = cacheLength.CacheAfterViewport;
				}
				if (IsPixelBased)
				{
					num2 = Math.Max(num2, Math.Abs(_viewport.Y - _extendedViewport.Y));
				}
				else
				{
					num4 = Math.Max(num4, Math.Abs(_viewport.Y - _extendedViewport.Y));
				}
			}
			if (IsPixelBased)
			{
				if (!IsScrolling && virtualizationInfoProvider != null && IsViewportEmpty(isHorizontal, rect) && DoubleUtil.GreaterThanZero(num2))
				{
					rect.Y = width - num2;
				}
				else
				{
					rect.Y -= num2;
				}
				rect.Height += num2 + num3;
				if (IsScrolling)
				{
					if (DoubleUtil.LessThan(rect.Y, 0.0))
					{
						rect.Height = Math.Max(rect.Height + rect.Y, 0.0);
						rect.Y = 0.0;
					}
					if (DoubleUtil.GreaterThan(rect.Y + rect.Height, _scrollData._extent.Height))
					{
						rect.Height = Math.Max(_scrollData._extent.Height - rect.Y, 0.0);
					}
				}
			}
			else
			{
				if (!IsScrolling && virtualizationInfoProvider != null && IsViewportEmpty(isHorizontal, rect) && DoubleUtil.GreaterThanZero(num2))
				{
					rect.Y = width2 - num4;
				}
				else
				{
					rect.Y -= num4;
				}
				rect.Height += num2 + num3;
				if (IsScrolling)
				{
					if (DoubleUtil.LessThan(rect.Y, 0.0))
					{
						rect.Height = Math.Max(rect.Height / num5 + rect.Y, 0.0) * num5;
						rect.Y = 0.0;
					}
					if (DoubleUtil.GreaterThan(rect.Y + rect.Height / num5, _scrollData._extent.Height))
					{
						rect.Height = Math.Max(_scrollData._extent.Height - rect.Y, 0.0) * num5;
					}
				}
			}
		}
		if (MeasureCaches)
		{
			itemsInExtendedViewportCount = _actualItemsInExtendedViewportCount;
		}
		else
		{
			int val = (int)Math.Ceiling(Math.Max(1.0, isHorizontal ? (rect.Width / viewport.Width) : (rect.Height / viewport.Height)) * (double)_actualItemsInExtendedViewportCount);
			itemsInExtendedViewportCount = Math.Max(val, itemsInExtendedViewportCount);
		}
		return rect;
	}

	private void CoerceScrollingViewportOffset(ref Rect viewport, Size extent, bool isHorizontal)
	{
		if (!_scrollData.IsEmpty)
		{
			viewport.X = ScrollContentPresenter.CoerceOffset(viewport.X, extent.Width, viewport.Width);
			if (!IsPixelBased && isHorizontal && DoubleUtil.IsZero(viewport.Width) && DoubleUtil.AreClose(viewport.X, extent.Width))
			{
				viewport.X = ScrollContentPresenter.CoerceOffset(viewport.X - 1.0, extent.Width, viewport.Width);
			}
		}
		if (!_scrollData.IsEmpty)
		{
			viewport.Y = ScrollContentPresenter.CoerceOffset(viewport.Y, extent.Height, viewport.Height);
			if (!IsPixelBased && !isHorizontal && DoubleUtil.IsZero(viewport.Height) && DoubleUtil.AreClose(viewport.Y, extent.Height))
			{
				viewport.Y = ScrollContentPresenter.CoerceOffset(viewport.Y - 1.0, extent.Height, viewport.Height);
			}
		}
	}

	private void AdjustNonScrollingViewportForHeader(IHierarchicalVirtualizationAndScrollInfo virtualizationInfoProvider, ref Rect viewport, ref VirtualizationCacheLength cacheLength, ref VirtualizationCacheLengthUnit cacheLengthUnit)
	{
		bool forHeader = true;
		AdjustNonScrollingViewport(virtualizationInfoProvider, ref viewport, ref cacheLength, ref cacheLengthUnit, forHeader);
	}

	private void AdjustNonScrollingViewportForItems(IHierarchicalVirtualizationAndScrollInfo virtualizationInfoProvider, ref Rect viewport, ref VirtualizationCacheLength cacheLength, ref VirtualizationCacheLengthUnit cacheLengthUnit)
	{
		bool forHeader = false;
		AdjustNonScrollingViewport(virtualizationInfoProvider, ref viewport, ref cacheLength, ref cacheLengthUnit, forHeader);
	}

	private void AdjustNonScrollingViewport(IHierarchicalVirtualizationAndScrollInfo virtualizationInfoProvider, ref Rect viewport, ref VirtualizationCacheLength cacheLength, ref VirtualizationCacheLengthUnit cacheUnit, bool forHeader)
	{
		Rect rect = viewport;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = cacheLength.CacheBeforeViewport;
		double num6 = cacheLength.CacheAfterViewport;
		HierarchicalVirtualizationHeaderDesiredSizes headerDesiredSizes = virtualizationInfoProvider.HeaderDesiredSizes;
		HierarchicalVirtualizationItemDesiredSizes itemDesiredSizes = virtualizationInfoProvider.ItemDesiredSizes;
		Size size = (forHeader ? headerDesiredSizes.PixelSize : itemDesiredSizes.PixelSize);
		Size size2 = (forHeader ? headerDesiredSizes.LogicalSize : itemDesiredSizes.LogicalSize);
		RelativeHeaderPosition relativeHeaderPosition = RelativeHeaderPosition.Top;
		if ((forHeader && relativeHeaderPosition == RelativeHeaderPosition.Left) || (!forHeader && relativeHeaderPosition == RelativeHeaderPosition.Right))
		{
			viewport.X -= (IsPixelBased ? size.Width : size2.Width);
			if (DoubleUtil.GreaterThanZero(rect.X))
			{
				if (IsPixelBased && DoubleUtil.GreaterThan(size.Width, rect.X))
				{
					num = size.Width - rect.X;
					num2 = size.Width - num;
					viewport.Width = Math.Max(viewport.Width - num, 0.0);
					num5 = ((cacheUnit != 0) ? Math.Max(num5 - Math.Floor(size2.Width * num2 / size.Width), 0.0) : Math.Max(num5 - num2, 0.0));
				}
			}
			else if (!DoubleUtil.GreaterThanZero(rect.Width))
			{
				num6 = ((cacheUnit != 0) ? Math.Max(num6 - size2.Width, 0.0) : Math.Max(num6 - size.Width, 0.0));
			}
			else if (DoubleUtil.GreaterThanOrClose(rect.Width, size.Width))
			{
				viewport.Width = Math.Max(0.0, rect.Width - size.Width);
			}
			else
			{
				num4 = rect.Width;
				num3 = size.Width - num4;
				viewport.Width = 0.0;
				num6 = ((cacheUnit != 0) ? Math.Max(num6 - Math.Floor(size2.Width * num3 / size.Width), 0.0) : Math.Max(num6 - num3, 0.0));
			}
		}
		else if ((forHeader && relativeHeaderPosition == RelativeHeaderPosition.Top) || (!forHeader && relativeHeaderPosition == RelativeHeaderPosition.Bottom))
		{
			viewport.Y -= (IsPixelBased ? size.Height : size2.Height);
			if (DoubleUtil.GreaterThanZero(rect.Y))
			{
				if (IsPixelBased && DoubleUtil.GreaterThan(size.Height, rect.Y))
				{
					num = size.Height - rect.Y;
					num2 = size.Height - num;
					viewport.Height = Math.Max(viewport.Height - num, 0.0);
					num5 = ((cacheUnit != 0) ? Math.Max(num5 - Math.Floor(size2.Height * num2 / size.Height), 0.0) : Math.Max(num5 - num2, 0.0));
				}
			}
			else if (!DoubleUtil.GreaterThanZero(rect.Height))
			{
				num6 = ((cacheUnit != 0) ? Math.Max(num6 - size2.Height, 0.0) : Math.Max(num6 - size.Height, 0.0));
			}
			else if (DoubleUtil.GreaterThanOrClose(rect.Height, size.Height))
			{
				viewport.Height = Math.Max(0.0, rect.Height - size.Height);
			}
			else
			{
				num4 = rect.Height;
				num3 = size.Height - num4;
				viewport.Height = 0.0;
				num6 = ((cacheUnit != 0) ? Math.Max(num6 - Math.Floor(size2.Height * num3 / size.Height), 0.0) : Math.Max(num6 - num3, 0.0));
			}
		}
		cacheLength = new VirtualizationCacheLength(num5, num6);
	}

	private void AdjustNonScrollingViewportForInset(bool isHorizontal, object parentItem, IContainItemStorage parentItemStorageProvider, IHierarchicalVirtualizationAndScrollInfo virtualizationInfoProvider, ref Rect viewport, ref VirtualizationCacheLength cacheLength, ref VirtualizationCacheLengthUnit cacheUnit)
	{
		Rect rect = viewport;
		FrameworkElement container = virtualizationInfoProvider as FrameworkElement;
		Thickness inset = GetItemsHostInsetForChild(virtualizationInfoProvider, parentItemStorageProvider, parentItem);
		bool flag = IsHeaderBeforeItems(isHorizontal, container, ref inset);
		double num = cacheLength.CacheBeforeViewport;
		double num2 = cacheLength.CacheAfterViewport;
		if (isHorizontal)
		{
			viewport.X -= (IsPixelBased ? inset.Left : ((double)(flag ? 1 : 0)));
		}
		else
		{
			viewport.Y -= (IsPixelBased ? inset.Top : ((double)(flag ? 1 : 0)));
		}
		if (isHorizontal)
		{
			if (DoubleUtil.GreaterThanZero(rect.X))
			{
				if (DoubleUtil.GreaterThanZero(viewport.Width))
				{
					if (IsPixelBased && DoubleUtil.GreaterThan(0.0, viewport.X))
					{
						if (cacheUnit == VirtualizationCacheLengthUnit.Pixel)
						{
							num = Math.Max(0.0, num - rect.X);
						}
						viewport.Width = Math.Max(0.0, viewport.Width + viewport.X);
					}
				}
				else if (cacheUnit == VirtualizationCacheLengthUnit.Pixel)
				{
					num = Math.Max(0.0, num - inset.Right);
				}
				else if (!flag)
				{
					num = Math.Max(0.0, num - 1.0);
				}
			}
			else if (DoubleUtil.GreaterThanZero(viewport.Width))
			{
				if (DoubleUtil.GreaterThanOrClose(viewport.Width, inset.Left))
				{
					viewport.Width = Math.Max(0.0, viewport.Width - inset.Left);
				}
				else
				{
					if (cacheUnit == VirtualizationCacheLengthUnit.Pixel)
					{
						num2 = Math.Max(0.0, num2 - (inset.Left - viewport.Width));
					}
					viewport.Width = 0.0;
				}
			}
			else if (cacheUnit == VirtualizationCacheLengthUnit.Pixel)
			{
				num2 = Math.Max(0.0, num2 - inset.Left);
			}
			else if (flag)
			{
				num2 = Math.Max(0.0, num2 - 1.0);
			}
		}
		else if (DoubleUtil.GreaterThan(rect.Y, 0.0))
		{
			if (DoubleUtil.GreaterThanZero(viewport.Height))
			{
				if (IsPixelBased && DoubleUtil.GreaterThan(0.0, viewport.Y))
				{
					if (cacheUnit == VirtualizationCacheLengthUnit.Pixel)
					{
						num = Math.Max(0.0, num - rect.Y);
					}
					viewport.Height = Math.Max(0.0, viewport.Height + viewport.Y);
				}
			}
			else if (cacheUnit == VirtualizationCacheLengthUnit.Pixel)
			{
				num = Math.Max(0.0, num - inset.Bottom);
			}
			else if (!flag)
			{
				num = Math.Max(0.0, num - 1.0);
			}
		}
		else if (DoubleUtil.GreaterThanZero(viewport.Height))
		{
			if (DoubleUtil.GreaterThanOrClose(viewport.Height, inset.Top))
			{
				viewport.Height = Math.Max(0.0, viewport.Height - inset.Top);
			}
			else
			{
				if (cacheUnit == VirtualizationCacheLengthUnit.Pixel)
				{
					num2 = Math.Max(0.0, num2 - (inset.Top - viewport.Height));
				}
				viewport.Height = 0.0;
			}
		}
		else if (cacheUnit == VirtualizationCacheLengthUnit.Pixel)
		{
			num2 = Math.Max(0.0, num2 - inset.Top);
		}
		else if (flag)
		{
			num2 = Math.Max(0.0, num2 - 1.0);
		}
		cacheLength = new VirtualizationCacheLength(num, num2);
	}

	private void ComputeFirstItemInViewportIndexAndOffset(IList items, int itemCount, IContainItemStorage itemStorageProvider, Rect viewport, VirtualizationCacheLength cacheSize, bool isHorizontal, bool areContainersUniformlySized, double uniformOrAverageContainerSize, out double firstItemInViewportOffset, out double firstItemInViewportContainerSpan, out int firstItemInViewportIndex, out bool foundFirstItemInViewport)
	{
		firstItemInViewportOffset = 0.0;
		firstItemInViewportContainerSpan = 0.0;
		firstItemInViewportIndex = 0;
		foundFirstItemInViewport = false;
		if (IsViewportEmpty(isHorizontal, viewport))
		{
			if (DoubleUtil.GreaterThanZero(cacheSize.CacheBeforeViewport))
			{
				firstItemInViewportIndex = itemCount - 1;
				ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, 0, itemCount - 1, out firstItemInViewportOffset);
				foundFirstItemInViewport = true;
			}
			else
			{
				firstItemInViewportIndex = 0;
				firstItemInViewportOffset = 0.0;
				foundFirstItemInViewport = DoubleUtil.GreaterThanZero(cacheSize.CacheAfterViewport);
			}
		}
		else
		{
			double num = Math.Max(isHorizontal ? viewport.X : viewport.Y, 0.0);
			if (areContainersUniformlySized)
			{
				if (DoubleUtil.GreaterThanZero(uniformOrAverageContainerSize))
				{
					firstItemInViewportIndex = (int)Math.Floor(num / uniformOrAverageContainerSize);
					firstItemInViewportOffset = (double)firstItemInViewportIndex * uniformOrAverageContainerSize;
				}
				firstItemInViewportContainerSpan = uniformOrAverageContainerSize;
				foundFirstItemInViewport = firstItemInViewportIndex < itemCount;
				if (!foundFirstItemInViewport)
				{
					firstItemInViewportOffset = 0.0;
					firstItemInViewportIndex = 0;
				}
			}
			else if (DoubleUtil.AreClose(num, 0.0))
			{
				foundFirstItemInViewport = true;
				firstItemInViewportOffset = 0.0;
				firstItemInViewportIndex = 0;
			}
			else
			{
				double num2 = 0.0;
				double num3 = 0.0;
				bool isVSP45Compat = IsVSP45Compat;
				for (int i = 0; i < itemCount; i++)
				{
					object item = items[i];
					GetContainerSizeForItem(itemStorageProvider, item, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, out var containerSize);
					num3 = (isHorizontal ? containerSize.Width : containerSize.Height);
					num2 += num3;
					if (isVSP45Compat ? DoubleUtil.GreaterThan(num2, num) : LayoutDoubleUtil.LessThan(num, num2))
					{
						firstItemInViewportIndex = i;
						firstItemInViewportOffset = num2 - num3;
						firstItemInViewportContainerSpan = num3;
						break;
					}
				}
				foundFirstItemInViewport = (isVSP45Compat ? DoubleUtil.GreaterThan(num2, num) : LayoutDoubleUtil.LessThan(num, num2));
				if (!foundFirstItemInViewport)
				{
					firstItemInViewportOffset = 0.0;
					firstItemInViewportIndex = 0;
				}
			}
		}
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.CFIVIO, viewport, foundFirstItemInViewport, firstItemInViewportIndex, firstItemInViewportOffset);
		}
	}

	private double ComputeEffectiveOffset(ref Rect viewport, DependencyObject firstContainer, int itemIndex, double firstItemOffset, IList items, IContainItemStorage itemStorageProvider, IHierarchicalVirtualizationAndScrollInfo virtualizationInfoProvider, bool isHorizontal, bool areContainersUniformlySized, double uniformOrAverageContainerSize, long scrollGeneration)
	{
		if (firstContainer == null || IsViewportEmpty(isHorizontal, viewport))
		{
			return -1.0;
		}
		double num = (isHorizontal ? viewport.X : viewport.Y);
		ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, 0, itemIndex, out var distance);
		distance += num - firstItemOffset;
		List<double> list = EffectiveOffsetInformationField.GetValue(firstContainer)?.OffsetList;
		if (list != null)
		{
			int count = list.Count;
			distance += list[count - 1] - list[0];
		}
		if (virtualizationInfoProvider is DependencyObject instance && !LayoutDoubleUtil.AreClose(num, distance))
		{
			EffectiveOffsetInformation effectiveOffsetInformation = EffectiveOffsetInformationField.GetValue(instance);
			if (effectiveOffsetInformation == null || effectiveOffsetInformation.ScrollGeneration != scrollGeneration)
			{
				effectiveOffsetInformation = new EffectiveOffsetInformation(scrollGeneration);
				effectiveOffsetInformation.OffsetList.Add(num);
			}
			effectiveOffsetInformation.OffsetList.Add(distance);
			if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
			{
				List<double> offsetList = effectiveOffsetInformation.OffsetList;
				object[] array = new object[offsetList.Count + 2];
				array[0] = scrollGeneration;
				array[1] = ":";
				for (int i = 0; i < offsetList.Count; i++)
				{
					array[i + 2] = offsetList[i];
				}
				ScrollTracer.Trace(this, ScrollTraceOp.StoreSubstOffset, array);
			}
			EffectiveOffsetInformationField.SetValue(instance, effectiveOffsetInformation);
		}
		return distance;
	}

	private void IncrementScrollGeneration()
	{
		_scrollData._scrollGeneration++;
	}

	private void ExtendPixelAndLogicalSizes(IList children, IList items, int itemCount, IContainItemStorage itemStorageProvider, bool areContainersUniformlySized, double uniformOrAverageContainerSize, double uniformOrAverageContainerPixelSize, ref Size stackPixelSize, ref Size stackLogicalSize, bool isHorizontal, int pivotIndex, int pivotChildIndex, int firstContainerInViewportIndex, bool before)
	{
		bool isVSP45Compat = IsVSP45Compat;
		double pixelDistance = 0.0;
		double distance;
		if (before)
		{
			if (isVSP45Compat)
			{
				ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, 0, pivotIndex, out distance);
			}
			else
			{
				ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, uniformOrAverageContainerPixelSize, 0, pivotIndex, out distance, out pixelDistance);
				if (!IsPixelBased)
				{
					ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, uniformOrAverageContainerPixelSize, pivotIndex, firstContainerInViewportIndex - pivotIndex, out var _, out var pixelDistance2);
					_pixelDistanceToViewport = pixelDistance + pixelDistance2;
					_pixelDistanceToFirstContainerInExtendedViewport = pixelDistance;
				}
			}
		}
		else if (isVSP45Compat)
		{
			ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, pivotIndex, itemCount - pivotIndex, out distance);
		}
		else
		{
			ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, uniformOrAverageContainerPixelSize, pivotIndex, itemCount - pivotIndex, out distance, out pixelDistance);
		}
		if (IsPixelBased)
		{
			if (isHorizontal)
			{
				stackPixelSize.Width += distance;
			}
			else
			{
				stackPixelSize.Height += distance;
			}
			return;
		}
		if (isHorizontal)
		{
			stackLogicalSize.Width += distance;
		}
		else
		{
			stackLogicalSize.Height += distance;
		}
		if (isVSP45Compat)
		{
			if (IsScrolling)
			{
				return;
			}
			int num;
			int num2;
			if (before)
			{
				num = 0;
				num2 = pivotChildIndex;
			}
			else
			{
				num = pivotChildIndex;
				num2 = children.Count;
			}
			for (int i = num; i < num2; i++)
			{
				Size desiredSize = ((UIElement)children[i]).DesiredSize;
				if (isHorizontal)
				{
					stackPixelSize.Width += desiredSize.Width;
				}
				else
				{
					stackPixelSize.Height += desiredSize.Height;
				}
			}
		}
		else if (!IsScrolling)
		{
			if (isHorizontal)
			{
				stackPixelSize.Width += pixelDistance;
			}
			else
			{
				stackPixelSize.Height += pixelDistance;
			}
		}
	}

	private void ComputeDistance(IList items, IContainItemStorage itemStorageProvider, bool isHorizontal, bool areContainersUniformlySized, double uniformOrAverageContainerSize, int startIndex, int itemCount, out double distance)
	{
		if (!IsPixelBased && !IsVSP45Compat)
		{
			ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, 1.0, startIndex, itemCount, out distance, out var _);
			return;
		}
		distance = 0.0;
		if (areContainersUniformlySized)
		{
			if (isHorizontal)
			{
				distance += uniformOrAverageContainerSize * (double)itemCount;
			}
			else
			{
				distance += uniformOrAverageContainerSize * (double)itemCount;
			}
			return;
		}
		for (int i = startIndex; i < startIndex + itemCount; i++)
		{
			object item = items[i];
			GetContainerSizeForItem(itemStorageProvider, item, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, out var containerSize);
			if (isHorizontal)
			{
				distance += containerSize.Width;
			}
			else
			{
				distance += containerSize.Height;
			}
		}
	}

	private void ComputeDistance(IList items, IContainItemStorage itemStorageProvider, bool isHorizontal, bool areContainersUniformlySized, double uniformOrAverageContainerSize, double uniformOrAverageContainerPixelSize, int startIndex, int itemCount, out double distance, out double pixelDistance)
	{
		distance = 0.0;
		pixelDistance = 0.0;
		if (areContainersUniformlySized)
		{
			distance += uniformOrAverageContainerSize * (double)itemCount;
			pixelDistance += uniformOrAverageContainerPixelSize * (double)itemCount;
			return;
		}
		for (int i = startIndex; i < startIndex + itemCount; i++)
		{
			object item = items[i];
			GetContainerSizeForItem(itemStorageProvider, item, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, uniformOrAverageContainerPixelSize, out var containerSize, out var containerPixelSize);
			if (isHorizontal)
			{
				distance += containerSize.Width;
				pixelDistance += containerPixelSize.Width;
			}
			else
			{
				distance += containerSize.Height;
				pixelDistance += containerPixelSize.Height;
			}
		}
	}

	private void GetContainerSizeForItem(IContainItemStorage itemStorageProvider, object item, bool isHorizontal, bool areContainersUniformlySized, double uniformOrAverageContainerSize, out Size containerSize)
	{
		if (!IsVSP45Compat)
		{
			GetContainerSizeForItem(itemStorageProvider, item, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, 1.0, out containerSize, out var _);
			return;
		}
		containerSize = Size.Empty;
		if (areContainersUniformlySized)
		{
			containerSize = default(Size);
			if (isHorizontal)
			{
				containerSize.Width = uniformOrAverageContainerSize;
				containerSize.Height = (IsPixelBased ? base.DesiredSize.Height : 1.0);
			}
			else
			{
				containerSize.Height = uniformOrAverageContainerSize;
				containerSize.Width = (IsPixelBased ? base.DesiredSize.Width : 1.0);
			}
			return;
		}
		object obj = itemStorageProvider.ReadItemValue(item, ContainerSizeProperty);
		if (obj != null)
		{
			containerSize = (Size)obj;
			return;
		}
		containerSize = default(Size);
		if (isHorizontal)
		{
			containerSize.Width = uniformOrAverageContainerSize;
			containerSize.Height = (IsPixelBased ? base.DesiredSize.Height : 1.0);
		}
		else
		{
			containerSize.Height = uniformOrAverageContainerSize;
			containerSize.Width = (IsPixelBased ? base.DesiredSize.Width : 1.0);
		}
	}

	private void GetContainerSizeForItem(IContainItemStorage itemStorageProvider, object item, bool isHorizontal, bool areContainersUniformlySized, double uniformOrAverageContainerSize, double uniformOrAverageContainerPixelSize, out Size containerSize, out Size containerPixelSize)
	{
		containerSize = default(Size);
		containerPixelSize = default(Size);
		bool flag = areContainersUniformlySized;
		if (!areContainersUniformlySized)
		{
			if (IsPixelBased)
			{
				object obj = itemStorageProvider.ReadItemValue(item, ContainerSizeProperty);
				if (obj != null)
				{
					containerSize = (Size)obj;
					containerPixelSize = containerSize;
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				object obj2 = itemStorageProvider.ReadItemValue(item, ContainerSizeDualProperty);
				if (obj2 != null)
				{
					ContainerSizeDual containerSizeDual = (ContainerSizeDual)obj2;
					containerSize = containerSizeDual.ItemSize;
					containerPixelSize = containerSizeDual.PixelSize;
				}
				else
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			if (isHorizontal)
			{
				double height = base.DesiredSize.Height;
				containerSize.Width = uniformOrAverageContainerSize;
				containerSize.Height = (IsPixelBased ? height : 1.0);
				containerPixelSize.Width = uniformOrAverageContainerPixelSize;
				containerPixelSize.Height = height;
			}
			else
			{
				double width = base.DesiredSize.Width;
				containerSize.Height = uniformOrAverageContainerSize;
				containerSize.Width = (IsPixelBased ? width : 1.0);
				containerPixelSize.Height = uniformOrAverageContainerPixelSize;
				containerPixelSize.Width = width;
			}
		}
	}

	private void SetContainerSizeForItem(IContainItemStorage itemStorageProvider, IContainItemStorage parentItemStorageProvider, object parentItem, object item, Size containerSize, bool isHorizontal, ref bool hasUniformOrAverageContainerSizeBeenSet, ref double uniformOrAverageContainerSize, ref bool areContainersUniformlySized)
	{
		if (!hasUniformOrAverageContainerSizeBeenSet)
		{
			if (IsVSP45Compat)
			{
				parentItemStorageProvider = itemStorageProvider;
			}
			hasUniformOrAverageContainerSizeBeenSet = true;
			uniformOrAverageContainerSize = (isHorizontal ? containerSize.Width : containerSize.Height);
			SetUniformOrAverageContainerSize(parentItemStorageProvider, parentItem, uniformOrAverageContainerSize, 1.0);
		}
		else if (areContainersUniformlySized)
		{
			if (isHorizontal)
			{
				areContainersUniformlySized = DoubleUtil.AreClose(containerSize.Width, uniformOrAverageContainerSize);
			}
			else
			{
				areContainersUniformlySized = DoubleUtil.AreClose(containerSize.Height, uniformOrAverageContainerSize);
			}
		}
		if (!areContainersUniformlySized)
		{
			itemStorageProvider.StoreItemValue(item, ContainerSizeProperty, containerSize);
		}
	}

	private void SetContainerSizeForItem(IContainItemStorage itemStorageProvider, IContainItemStorage parentItemStorageProvider, object parentItem, object item, Size containerSize, Size containerPixelSize, bool isHorizontal, bool hasVirtualizingChildren, ref bool hasUniformOrAverageContainerSizeBeenSet, ref double uniformOrAverageContainerSize, ref double uniformOrAverageContainerPixelSize, ref bool areContainersUniformlySized, ref bool hasAnyContainerSpanChanged)
	{
		if (!hasUniformOrAverageContainerSizeBeenSet)
		{
			hasUniformOrAverageContainerSizeBeenSet = true;
			uniformOrAverageContainerSize = (isHorizontal ? containerSize.Width : containerSize.Height);
			uniformOrAverageContainerPixelSize = (isHorizontal ? containerPixelSize.Width : containerPixelSize.Height);
			SetUniformOrAverageContainerSize(parentItemStorageProvider, parentItem, uniformOrAverageContainerSize, uniformOrAverageContainerPixelSize);
		}
		else if (areContainersUniformlySized)
		{
			bool flag = IsPixelBased || (IsScrolling && !hasVirtualizingChildren);
			if (isHorizontal)
			{
				areContainersUniformlySized = DoubleUtil.AreClose(containerSize.Width, uniformOrAverageContainerSize) && (flag || DoubleUtil.AreClose(containerPixelSize.Width, uniformOrAverageContainerPixelSize));
			}
			else
			{
				areContainersUniformlySized = DoubleUtil.AreClose(containerSize.Height, uniformOrAverageContainerSize) && (flag || DoubleUtil.AreClose(containerPixelSize.Height, uniformOrAverageContainerPixelSize));
			}
		}
		if (areContainersUniformlySized)
		{
			return;
		}
		double value = 0.0;
		double value2 = 0.0;
		bool flag2 = ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this);
		if (IsPixelBased)
		{
			object obj = itemStorageProvider.ReadItemValue(item, ContainerSizeProperty);
			Size size = ((obj != null) ? ((Size)obj) : Size.Empty);
			if (obj == null || containerSize != size)
			{
				if (flag2)
				{
					ItemContainerGenerator itemContainerGenerator = (ItemContainerGenerator)base.Generator;
					ScrollTracer.Trace(this, ScrollTraceOp.SetContainerSize, itemContainerGenerator.IndexFromContainer(itemContainerGenerator.ContainerFromItem(item)), size, containerSize);
				}
				if (isHorizontal)
				{
					value = ((obj != null) ? size.Width : uniformOrAverageContainerSize);
					value2 = containerSize.Width;
				}
				else
				{
					value = ((obj != null) ? size.Height : uniformOrAverageContainerSize);
					value2 = containerSize.Height;
				}
			}
			itemStorageProvider.StoreItemValue(item, ContainerSizeProperty, containerSize);
		}
		else
		{
			object obj2 = itemStorageProvider.ReadItemValue(item, ContainerSizeDualProperty);
			ContainerSizeDual containerSizeDual = ((obj2 != null) ? ((ContainerSizeDual)obj2) : new ContainerSizeDual(Size.Empty, Size.Empty));
			if (obj2 == null || containerSize != containerSizeDual.ItemSize || containerPixelSize != containerSizeDual.PixelSize)
			{
				if (flag2)
				{
					ItemContainerGenerator itemContainerGenerator2 = (ItemContainerGenerator)base.Generator;
					ScrollTracer.Trace(this, ScrollTraceOp.SetContainerSize, itemContainerGenerator2.IndexFromContainer(itemContainerGenerator2.ContainerFromItem(item)), containerSizeDual.ItemSize, containerSize, containerSizeDual.PixelSize, containerPixelSize);
				}
				if (isHorizontal)
				{
					value = ((obj2 != null) ? containerSizeDual.ItemSize.Width : uniformOrAverageContainerSize);
					value2 = containerSize.Width;
				}
				else
				{
					value = ((obj2 != null) ? containerSizeDual.ItemSize.Height : uniformOrAverageContainerSize);
					value2 = containerSize.Height;
				}
			}
			ContainerSizeDual value3 = new ContainerSizeDual(containerPixelSize, containerSize);
			itemStorageProvider.StoreItemValue(item, ContainerSizeDualProperty, value3);
		}
		if (!LayoutDoubleUtil.AreClose(value, value2))
		{
			hasAnyContainerSpanChanged = true;
		}
	}

	private Thickness GetItemsHostInsetForChild(IHierarchicalVirtualizationAndScrollInfo virtualizationInfoProvider, IContainItemStorage parentItemStorageProvider = null, object parentItem = null)
	{
		FrameworkElement frameworkElement = virtualizationInfoProvider as FrameworkElement;
		if (parentItemStorageProvider == null)
		{
			return (Thickness)frameworkElement.GetValue(ItemsHostInsetProperty);
		}
		Thickness thickness = default(Thickness);
		object obj = parentItemStorageProvider.ReadItemValue(parentItem, ItemsHostInsetProperty);
		if (obj != null)
		{
			thickness = (Thickness)obj;
		}
		else if ((obj = frameworkElement.ReadLocalValue(ItemsHostInsetProperty)) != DependencyProperty.UnsetValue)
		{
			thickness = (Thickness)obj;
		}
		else
		{
			HierarchicalVirtualizationHeaderDesiredSizes headerDesiredSizes = virtualizationInfoProvider.HeaderDesiredSizes;
			Thickness margin = frameworkElement.Margin;
			thickness.Top = headerDesiredSizes.PixelSize.Height + margin.Top;
			thickness.Left = headerDesiredSizes.PixelSize.Width + margin.Left;
			parentItemStorageProvider.StoreItemValue(parentItem, ItemsHostInsetProperty, thickness);
		}
		frameworkElement.SetValue(ItemsHostInsetProperty, thickness);
		return thickness;
	}

	private void SetItemsHostInsetForChild(int index, UIElement child, IContainItemStorage itemStorageProvider, bool isHorizontal)
	{
		bool isChildHorizontal = isHorizontal;
		IHierarchicalVirtualizationAndScrollInfo virtualizingChild = GetVirtualizingChild(child, ref isChildHorizontal);
		Panel panel = virtualizingChild?.ItemsHost;
		if (panel == null || !panel.IsVisible)
		{
			return;
		}
		GeneralTransform generalTransform = child.TransformToDescendant(panel);
		if (generalTransform == null)
		{
			return;
		}
		Thickness thickness = ((!(virtualizingChild is FrameworkElement frameworkElement)) ? default(Thickness) : frameworkElement.Margin);
		Rect rect = new Rect(default(Point), child.DesiredSize);
		rect.Offset(0.0 - thickness.Left, 0.0 - thickness.Top);
		Rect rect2 = generalTransform.TransformBounds(rect);
		Size desiredSize = panel.DesiredSize;
		double left = (DoubleUtil.AreClose(0.0, rect2.Left) ? 0.0 : (0.0 - rect2.Left));
		double top = (DoubleUtil.AreClose(0.0, rect2.Top) ? 0.0 : (0.0 - rect2.Top));
		double right = (DoubleUtil.AreClose(desiredSize.Width, rect2.Right) ? 0.0 : (rect2.Right - desiredSize.Width));
		double bottom = (DoubleUtil.AreClose(desiredSize.Height, rect2.Bottom) ? 0.0 : (rect2.Bottom - desiredSize.Height));
		Thickness thickness2 = new Thickness(left, top, right, bottom);
		object itemFromContainer = GetItemFromContainer(child);
		if (itemFromContainer == DependencyProperty.UnsetValue)
		{
			return;
		}
		object obj = itemStorageProvider.ReadItemValue(itemFromContainer, ItemsHostInsetProperty);
		bool flag = obj == null;
		bool flag2 = flag;
		if (!flag)
		{
			Thickness thickness3 = (Thickness)obj;
			flag = !DoubleUtil.AreClose(thickness3.Left, thickness2.Left) || !DoubleUtil.AreClose(thickness3.Top, thickness2.Top) || !DoubleUtil.AreClose(thickness3.Right, thickness2.Right) || !DoubleUtil.AreClose(thickness3.Bottom, thickness2.Bottom);
			flag2 = flag && ((isHorizontal && (!AreInsetsClose(thickness3.Left, thickness2.Left) || !AreInsetsClose(thickness3.Right, thickness2.Right))) || (!isHorizontal && (!AreInsetsClose(thickness3.Top, thickness2.Top) || !AreInsetsClose(thickness3.Bottom, thickness2.Bottom))));
		}
		if (flag)
		{
			itemStorageProvider.StoreItemValue(itemFromContainer, ItemsHostInsetProperty, thickness2);
			child.SetValue(ItemsHostInsetProperty, thickness2);
		}
		if (!flag2)
		{
			return;
		}
		Panel panel2 = GetScrollingItemsControl(child)?.ItemsHost;
		if (panel2 != null)
		{
			if (panel2 is VirtualizingStackPanel virtualizingStackPanel)
			{
				virtualizingStackPanel.AnchoredInvalidateMeasure();
			}
			else
			{
				panel2.InvalidateMeasure();
			}
		}
	}

	private static bool AreInsetsClose(double value1, double value2)
	{
		if (value1 == value2)
		{
			return true;
		}
		double num = (Math.Abs(value1) + Math.Abs(value2)) * 0.001;
		double num2 = value1 - value2;
		if (0.0 - num <= num2)
		{
			return num >= num2;
		}
		return false;
	}

	private ItemsControl GetScrollingItemsControl(UIElement container)
	{
		if (container is TreeViewItem)
		{
			for (ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(container); itemsControl != null; itemsControl = ItemsControl.ItemsControlFromItemContainer(itemsControl))
			{
				if (itemsControl is TreeView result)
				{
					return result;
				}
			}
		}
		else if (container is GroupItem)
		{
			DependencyObject dependencyObject = container;
			do
			{
				dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
				if (dependencyObject is ItemsControl result2)
				{
					return result2;
				}
			}
			while (dependencyObject != null);
		}
		else
		{
			_ = container?.GetType().Name;
		}
		return null;
	}

	private object GetItemFromContainer(DependencyObject container)
	{
		return container.ReadLocalValue(System.Windows.Controls.ItemContainerGenerator.ItemForItemContainerProperty);
	}

	private bool IsHeaderBeforeItems(bool isHorizontal, FrameworkElement container, ref Thickness inset)
	{
		Thickness thickness = container?.Margin ?? default(Thickness);
		if (isHorizontal)
		{
			return DoubleUtil.GreaterThanOrClose(inset.Left - thickness.Left, inset.Right - thickness.Right);
		}
		return DoubleUtil.GreaterThanOrClose(inset.Top - thickness.Top, inset.Bottom - thickness.Bottom);
	}

	private bool IsEndOfCache(bool isHorizontal, double cacheSize, VirtualizationCacheLengthUnit cacheUnit, Size stackPixelSizeInCache, Size stackLogicalSizeInCache)
	{
		if (!MeasureCaches)
		{
			return true;
		}
		switch (cacheUnit)
		{
		case VirtualizationCacheLengthUnit.Item:
			if (isHorizontal)
			{
				return DoubleUtil.GreaterThanOrClose(stackLogicalSizeInCache.Width, cacheSize);
			}
			return DoubleUtil.GreaterThanOrClose(stackLogicalSizeInCache.Height, cacheSize);
		case VirtualizationCacheLengthUnit.Pixel:
			if (isHorizontal)
			{
				return DoubleUtil.GreaterThanOrClose(stackPixelSizeInCache.Width, cacheSize);
			}
			return DoubleUtil.GreaterThanOrClose(stackPixelSizeInCache.Height, cacheSize);
		default:
			return false;
		}
	}

	private bool IsEndOfViewport(bool isHorizontal, Rect viewport, Size stackPixelSizeInViewport)
	{
		if (isHorizontal)
		{
			return DoubleUtil.GreaterThanOrClose(stackPixelSizeInViewport.Width, viewport.Width);
		}
		return DoubleUtil.GreaterThanOrClose(stackPixelSizeInViewport.Height, viewport.Height);
	}

	private bool IsViewportEmpty(bool isHorizontal, Rect viewport)
	{
		if (isHorizontal)
		{
			return DoubleUtil.AreClose(viewport.Width, 0.0);
		}
		return DoubleUtil.AreClose(viewport.Height, 0.0);
	}

	private void SetViewportForChild(bool isHorizontal, IContainItemStorage itemStorageProvider, bool areContainersUniformlySized, double uniformOrAverageContainerSize, bool mustDisableVirtualization, UIElement child, IHierarchicalVirtualizationAndScrollInfo virtualizingChild, object item, bool isBeforeFirstItem, bool isAfterFirstItem, double firstItemInViewportOffset, Rect parentViewport, VirtualizationCacheLength parentCacheSize, VirtualizationCacheLengthUnit parentCacheUnit, long scrollGeneration, Size stackPixelSize, Size stackPixelSizeInViewport, Size stackPixelSizeInCacheBeforeViewport, Size stackPixelSizeInCacheAfterViewport, Size stackLogicalSize, Size stackLogicalSizeInViewport, Size stackLogicalSizeInCacheBeforeViewport, Size stackLogicalSizeInCacheAfterViewport, out Rect childViewport, ref VirtualizationCacheLength childCacheSize, ref VirtualizationCacheLengthUnit childCacheUnit)
	{
		childViewport = parentViewport;
		if (isHorizontal)
		{
			if (isBeforeFirstItem)
			{
				GetContainerSizeForItem(itemStorageProvider, item, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, out var containerSize);
				childViewport.X = (IsPixelBased ? stackPixelSizeInCacheBeforeViewport.Width : stackLogicalSizeInCacheBeforeViewport.Width) + containerSize.Width;
				childViewport.Width = 0.0;
			}
			else if (isAfterFirstItem)
			{
				childViewport.X = Math.Min(childViewport.X, 0.0) - (IsPixelBased ? (stackPixelSizeInViewport.Width + stackPixelSizeInCacheAfterViewport.Width) : (stackLogicalSizeInViewport.Width + stackLogicalSizeInCacheAfterViewport.Width));
				childViewport.Width = Math.Max(childViewport.Width - stackPixelSizeInViewport.Width, 0.0);
			}
			else
			{
				childViewport.X -= firstItemInViewportOffset;
				childViewport.Width = Math.Max(childViewport.Width - stackPixelSizeInViewport.Width, 0.0);
			}
			switch (parentCacheUnit)
			{
			case VirtualizationCacheLengthUnit.Item:
				childCacheSize = new VirtualizationCacheLength((isAfterFirstItem || DoubleUtil.LessThanOrClose(childViewport.X, 0.0)) ? 0.0 : Math.Max(parentCacheSize.CacheBeforeViewport - stackLogicalSizeInCacheBeforeViewport.Width, 0.0), isBeforeFirstItem ? 0.0 : Math.Max(parentCacheSize.CacheAfterViewport - stackLogicalSizeInCacheAfterViewport.Width, 0.0));
				childCacheUnit = VirtualizationCacheLengthUnit.Item;
				break;
			case VirtualizationCacheLengthUnit.Pixel:
				childCacheSize = new VirtualizationCacheLength((isAfterFirstItem || DoubleUtil.LessThanOrClose(childViewport.X, 0.0)) ? 0.0 : Math.Max(parentCacheSize.CacheBeforeViewport - stackPixelSizeInCacheBeforeViewport.Width, 0.0), isBeforeFirstItem ? 0.0 : Math.Max(parentCacheSize.CacheAfterViewport - stackPixelSizeInCacheAfterViewport.Width, 0.0));
				childCacheUnit = VirtualizationCacheLengthUnit.Pixel;
				break;
			}
		}
		else
		{
			if (isBeforeFirstItem)
			{
				GetContainerSizeForItem(itemStorageProvider, item, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, out var containerSize2);
				childViewport.Y = (IsPixelBased ? stackPixelSizeInCacheBeforeViewport.Height : stackLogicalSizeInCacheBeforeViewport.Height) + containerSize2.Height;
				childViewport.Height = 0.0;
			}
			else if (isAfterFirstItem)
			{
				childViewport.Y = Math.Min(childViewport.Y, 0.0) - (IsPixelBased ? (stackPixelSizeInViewport.Height + stackPixelSizeInCacheAfterViewport.Height) : (stackLogicalSizeInViewport.Height + stackLogicalSizeInCacheAfterViewport.Height));
				childViewport.Height = Math.Max(childViewport.Height - stackPixelSizeInViewport.Height, 0.0);
			}
			else
			{
				childViewport.Y -= firstItemInViewportOffset;
				childViewport.Height = Math.Max(childViewport.Height - stackPixelSizeInViewport.Height, 0.0);
			}
			switch (parentCacheUnit)
			{
			case VirtualizationCacheLengthUnit.Item:
				childCacheSize = new VirtualizationCacheLength((isAfterFirstItem || DoubleUtil.LessThanOrClose(childViewport.Y, 0.0)) ? 0.0 : Math.Max(parentCacheSize.CacheBeforeViewport - stackLogicalSizeInCacheBeforeViewport.Height, 0.0), isBeforeFirstItem ? 0.0 : Math.Max(parentCacheSize.CacheAfterViewport - stackLogicalSizeInCacheAfterViewport.Height, 0.0));
				childCacheUnit = VirtualizationCacheLengthUnit.Item;
				break;
			case VirtualizationCacheLengthUnit.Pixel:
				childCacheSize = new VirtualizationCacheLength((isAfterFirstItem || DoubleUtil.LessThanOrClose(childViewport.Y, 0.0)) ? 0.0 : Math.Max(parentCacheSize.CacheBeforeViewport - stackPixelSizeInCacheBeforeViewport.Height, 0.0), isBeforeFirstItem ? 0.0 : Math.Max(parentCacheSize.CacheAfterViewport - stackPixelSizeInCacheAfterViewport.Height, 0.0));
				childCacheUnit = VirtualizationCacheLengthUnit.Pixel;
				break;
			}
		}
		if (virtualizingChild != null)
		{
			HierarchicalVirtualizationConstraints constraints = new HierarchicalVirtualizationConstraints(childCacheSize, childCacheUnit, childViewport);
			constraints.ScrollGeneration = scrollGeneration;
			virtualizingChild.Constraints = constraints;
			virtualizingChild.InBackgroundLayout = MeasureCaches;
			virtualizingChild.MustDisableVirtualization = mustDisableVirtualization;
		}
		if (child is IHierarchicalVirtualizationAndScrollInfo)
		{
			InvalidateMeasureOnItemsHost((IHierarchicalVirtualizationAndScrollInfo)child);
		}
	}

	private void InvalidateMeasureOnItemsHost(IHierarchicalVirtualizationAndScrollInfo virtualizingChild)
	{
		Panel itemsHost = virtualizingChild.ItemsHost;
		if (itemsHost == null)
		{
			return;
		}
		Helper.InvalidateMeasureOnPath(itemsHost, this, duringMeasure: true);
		if (itemsHost is VirtualizingStackPanel)
		{
			return;
		}
		IList internalChildren = itemsHost.InternalChildren;
		for (int i = 0; i < internalChildren.Count; i++)
		{
			if (internalChildren[i] is IHierarchicalVirtualizationAndScrollInfo virtualizingChild2)
			{
				InvalidateMeasureOnItemsHost(virtualizingChild2);
			}
		}
	}

	private void GetSizesForChild(bool isHorizontal, bool isChildHorizontal, bool isBeforeFirstItem, bool isAfterLastItem, IHierarchicalVirtualizationAndScrollInfo virtualizingChild, Size childDesiredSize, Rect childViewport, VirtualizationCacheLength childCacheSize, VirtualizationCacheLengthUnit childCacheUnit, out Size childPixelSize, out Size childPixelSizeInViewport, out Size childPixelSizeInCacheBeforeViewport, out Size childPixelSizeInCacheAfterViewport, out Size childLogicalSize, out Size childLogicalSizeInViewport, out Size childLogicalSizeInCacheBeforeViewport, out Size childLogicalSizeInCacheAfterViewport)
	{
		childPixelSize = default(Size);
		childPixelSizeInViewport = default(Size);
		childPixelSizeInCacheBeforeViewport = default(Size);
		childPixelSizeInCacheAfterViewport = default(Size);
		childLogicalSize = default(Size);
		childLogicalSizeInViewport = default(Size);
		childLogicalSizeInCacheBeforeViewport = default(Size);
		childLogicalSizeInCacheAfterViewport = default(Size);
		if (virtualizingChild != null)
		{
			RelativeHeaderPosition relativeHeaderPosition = RelativeHeaderPosition.Top;
			HierarchicalVirtualizationHeaderDesiredSizes headerDesiredSizes = virtualizingChild.HeaderDesiredSizes;
			HierarchicalVirtualizationItemDesiredSizes itemDesiredSizes = virtualizingChild.ItemDesiredSizes;
			Size pixelSize = headerDesiredSizes.PixelSize;
			Size logicalSize = headerDesiredSizes.LogicalSize;
			childPixelSize = childDesiredSize;
			if (relativeHeaderPosition == RelativeHeaderPosition.Top || relativeHeaderPosition == RelativeHeaderPosition.Bottom)
			{
				childLogicalSize.Height = itemDesiredSizes.LogicalSize.Height + logicalSize.Height;
				childLogicalSize.Width = Math.Max(itemDesiredSizes.LogicalSize.Width, logicalSize.Width);
			}
			else
			{
				childLogicalSize.Width = itemDesiredSizes.LogicalSize.Width + logicalSize.Width;
				childLogicalSize.Height = Math.Max(itemDesiredSizes.LogicalSize.Height, logicalSize.Height);
			}
			if (IsPixelBased && ((isHorizontal && DoubleUtil.AreClose(itemDesiredSizes.PixelSize.Width, itemDesiredSizes.PixelSizeInViewport.Width)) || (!isHorizontal && DoubleUtil.AreClose(itemDesiredSizes.PixelSize.Height, itemDesiredSizes.PixelSizeInViewport.Height))))
			{
				Rect viewport = childViewport;
				if (relativeHeaderPosition == RelativeHeaderPosition.Top || relativeHeaderPosition == RelativeHeaderPosition.Left)
				{
					VirtualizationCacheLength cacheLength = childCacheSize;
					VirtualizationCacheLengthUnit cacheLengthUnit = childCacheUnit;
					AdjustNonScrollingViewportForHeader(virtualizingChild, ref viewport, ref cacheLength, ref cacheLengthUnit);
				}
				GetSizesForChildIntersectingTheViewport(isHorizontal, isChildHorizontal, itemDesiredSizes.PixelSizeInViewport, itemDesiredSizes.LogicalSizeInViewport, viewport, ref childPixelSizeInViewport, ref childLogicalSizeInViewport, ref childPixelSizeInCacheBeforeViewport, ref childLogicalSizeInCacheBeforeViewport, ref childPixelSizeInCacheAfterViewport, ref childLogicalSizeInCacheAfterViewport);
			}
			else
			{
				StackSizes(isHorizontal, ref childPixelSizeInViewport, itemDesiredSizes.PixelSizeInViewport);
				StackSizes(isHorizontal, ref childLogicalSizeInViewport, itemDesiredSizes.LogicalSizeInViewport);
			}
			if (isChildHorizontal == isHorizontal)
			{
				StackSizes(isHorizontal, ref childPixelSizeInCacheBeforeViewport, itemDesiredSizes.PixelSizeBeforeViewport);
				StackSizes(isHorizontal, ref childLogicalSizeInCacheBeforeViewport, itemDesiredSizes.LogicalSizeBeforeViewport);
				StackSizes(isHorizontal, ref childPixelSizeInCacheAfterViewport, itemDesiredSizes.PixelSizeAfterViewport);
				StackSizes(isHorizontal, ref childLogicalSizeInCacheAfterViewport, itemDesiredSizes.LogicalSizeAfterViewport);
			}
			Rect viewport2 = childViewport;
			Size childPixelSizeInViewport2 = default(Size);
			Size childLogicalSizeInViewport2 = default(Size);
			Size childPixelSizeInCacheBeforeViewport2 = default(Size);
			Size childLogicalSizeInCacheBeforeViewport2 = default(Size);
			Size childPixelSizeInCacheAfterViewport2 = default(Size);
			Size childLogicalSizeInCacheAfterViewport2 = default(Size);
			bool isHorizontal2 = relativeHeaderPosition == RelativeHeaderPosition.Left || relativeHeaderPosition == RelativeHeaderPosition.Right;
			if (relativeHeaderPosition == RelativeHeaderPosition.Bottom || relativeHeaderPosition == RelativeHeaderPosition.Right)
			{
				VirtualizationCacheLength cacheLength2 = childCacheSize;
				VirtualizationCacheLengthUnit cacheLengthUnit2 = childCacheUnit;
				AdjustNonScrollingViewportForItems(virtualizingChild, ref viewport2, ref cacheLength2, ref cacheLengthUnit2);
			}
			if (isBeforeFirstItem)
			{
				childPixelSizeInCacheBeforeViewport2 = pixelSize;
				childLogicalSizeInCacheBeforeViewport2 = logicalSize;
			}
			else if (isAfterLastItem)
			{
				childPixelSizeInCacheAfterViewport2 = pixelSize;
				childLogicalSizeInCacheAfterViewport2 = logicalSize;
			}
			else
			{
				GetSizesForChildIntersectingTheViewport(isHorizontal, isChildHorizontal, pixelSize, logicalSize, viewport2, ref childPixelSizeInViewport2, ref childLogicalSizeInViewport2, ref childPixelSizeInCacheBeforeViewport2, ref childLogicalSizeInCacheBeforeViewport2, ref childPixelSizeInCacheAfterViewport2, ref childLogicalSizeInCacheAfterViewport2);
			}
			StackSizes(isHorizontal2, ref childPixelSizeInViewport, childPixelSizeInViewport2);
			StackSizes(isHorizontal2, ref childLogicalSizeInViewport, childLogicalSizeInViewport2);
			StackSizes(isHorizontal2, ref childPixelSizeInCacheBeforeViewport, childPixelSizeInCacheBeforeViewport2);
			StackSizes(isHorizontal2, ref childLogicalSizeInCacheBeforeViewport, childLogicalSizeInCacheBeforeViewport2);
			StackSizes(isHorizontal2, ref childPixelSizeInCacheAfterViewport, childPixelSizeInCacheAfterViewport2);
			StackSizes(isHorizontal2, ref childLogicalSizeInCacheAfterViewport, childLogicalSizeInCacheAfterViewport2);
		}
		else
		{
			childPixelSize = childDesiredSize;
			childLogicalSize = new Size(DoubleUtil.GreaterThanZero(childPixelSize.Width) ? 1 : 0, DoubleUtil.GreaterThanZero(childPixelSize.Height) ? 1 : 0);
			if (isBeforeFirstItem)
			{
				childPixelSizeInCacheBeforeViewport = childDesiredSize;
				childLogicalSizeInCacheBeforeViewport = new Size(DoubleUtil.GreaterThanZero(childPixelSizeInCacheBeforeViewport.Width) ? 1 : 0, DoubleUtil.GreaterThanZero(childPixelSizeInCacheBeforeViewport.Height) ? 1 : 0);
			}
			else if (isAfterLastItem)
			{
				childPixelSizeInCacheAfterViewport = childDesiredSize;
				childLogicalSizeInCacheAfterViewport = new Size(DoubleUtil.GreaterThanZero(childPixelSizeInCacheAfterViewport.Width) ? 1 : 0, DoubleUtil.GreaterThanZero(childPixelSizeInCacheAfterViewport.Height) ? 1 : 0);
			}
			else
			{
				GetSizesForChildIntersectingTheViewport(isHorizontal, isHorizontal, childPixelSize, childLogicalSize, childViewport, ref childPixelSizeInViewport, ref childLogicalSizeInViewport, ref childPixelSizeInCacheBeforeViewport, ref childLogicalSizeInCacheBeforeViewport, ref childPixelSizeInCacheAfterViewport, ref childLogicalSizeInCacheAfterViewport);
			}
		}
	}

	private void GetSizesForChildWithInset(bool isHorizontal, bool isChildHorizontal, bool isBeforeFirstItem, bool isAfterLastItem, IHierarchicalVirtualizationAndScrollInfo virtualizingChild, Size childDesiredSize, Rect childViewport, VirtualizationCacheLength childCacheSize, VirtualizationCacheLengthUnit childCacheUnit, out Size childPixelSize, out Size childPixelSizeInViewport, out Size childPixelSizeInCacheBeforeViewport, out Size childPixelSizeInCacheAfterViewport, out Size childLogicalSize, out Size childLogicalSizeInViewport, out Size childLogicalSizeInCacheBeforeViewport, out Size childLogicalSizeInCacheAfterViewport)
	{
		childPixelSize = childDesiredSize;
		childPixelSizeInViewport = default(Size);
		childPixelSizeInCacheBeforeViewport = default(Size);
		childPixelSizeInCacheAfterViewport = default(Size);
		childLogicalSize = default(Size);
		childLogicalSizeInViewport = default(Size);
		childLogicalSizeInCacheBeforeViewport = default(Size);
		childLogicalSizeInCacheAfterViewport = default(Size);
		HierarchicalVirtualizationItemDesiredSizes hierarchicalVirtualizationItemDesiredSizes = virtualizingChild?.ItemDesiredSizes ?? default(HierarchicalVirtualizationItemDesiredSizes);
		if ((!isHorizontal && (hierarchicalVirtualizationItemDesiredSizes.PixelSize.Height > 0.0 || hierarchicalVirtualizationItemDesiredSizes.LogicalSize.Height > 0.0)) || (isHorizontal && (hierarchicalVirtualizationItemDesiredSizes.PixelSize.Width > 0.0 || hierarchicalVirtualizationItemDesiredSizes.LogicalSize.Width > 0.0)))
		{
			StackSizes(isHorizontal, ref childPixelSizeInCacheBeforeViewport, hierarchicalVirtualizationItemDesiredSizes.PixelSizeBeforeViewport);
			StackSizes(isHorizontal, ref childPixelSizeInViewport, hierarchicalVirtualizationItemDesiredSizes.PixelSizeInViewport);
			StackSizes(isHorizontal, ref childPixelSizeInCacheAfterViewport, hierarchicalVirtualizationItemDesiredSizes.PixelSizeAfterViewport);
			StackSizes(isHorizontal, ref childLogicalSize, hierarchicalVirtualizationItemDesiredSizes.LogicalSize);
			StackSizes(isHorizontal, ref childLogicalSizeInCacheBeforeViewport, hierarchicalVirtualizationItemDesiredSizes.LogicalSizeBeforeViewport);
			StackSizes(isHorizontal, ref childLogicalSizeInViewport, hierarchicalVirtualizationItemDesiredSizes.LogicalSizeInViewport);
			StackSizes(isHorizontal, ref childLogicalSizeInCacheAfterViewport, hierarchicalVirtualizationItemDesiredSizes.LogicalSizeAfterViewport);
			Thickness inset = GetItemsHostInsetForChild(virtualizingChild);
			bool num = IsHeaderBeforeItems(isHorizontal, virtualizingChild as FrameworkElement, ref inset);
			Size childPixelSize2 = (isHorizontal ? new Size(Math.Max(inset.Left, 0.0), childDesiredSize.Height) : new Size(childDesiredSize.Width, Math.Max(inset.Top, 0.0)));
			Size size = (num ? new Size(1.0, 1.0) : new Size(0.0, 0.0));
			StackSizes(isHorizontal, ref childLogicalSize, size);
			GetSizesForChildIntersectingTheViewport(isHorizontal, isChildHorizontal, childPixelSize2, size, childViewport, ref childPixelSizeInViewport, ref childLogicalSizeInViewport, ref childPixelSizeInCacheBeforeViewport, ref childLogicalSizeInCacheBeforeViewport, ref childPixelSizeInCacheAfterViewport, ref childLogicalSizeInCacheAfterViewport);
			Size childPixelSize3 = (isHorizontal ? new Size(Math.Max(inset.Right, 0.0), childDesiredSize.Height) : new Size(childDesiredSize.Width, Math.Max(inset.Bottom, 0.0)));
			Size size2 = (num ? new Size(0.0, 0.0) : new Size(1.0, 1.0));
			StackSizes(isHorizontal, ref childLogicalSize, size2);
			Rect childViewport2 = childViewport;
			if (isHorizontal)
			{
				childViewport2.X -= (IsPixelBased ? (childPixelSize2.Width + hierarchicalVirtualizationItemDesiredSizes.PixelSize.Width) : (size.Width + hierarchicalVirtualizationItemDesiredSizes.LogicalSize.Width));
				childViewport2.Width = Math.Max(0.0, childViewport2.Width - childPixelSizeInViewport.Width);
			}
			else
			{
				childViewport2.Y -= (IsPixelBased ? (childPixelSize2.Height + hierarchicalVirtualizationItemDesiredSizes.PixelSize.Height) : (size.Height + hierarchicalVirtualizationItemDesiredSizes.LogicalSize.Height));
				childViewport2.Height = Math.Max(0.0, childViewport2.Height - childPixelSizeInViewport.Height);
			}
			GetSizesForChildIntersectingTheViewport(isHorizontal, isChildHorizontal, childPixelSize3, size2, childViewport2, ref childPixelSizeInViewport, ref childLogicalSizeInViewport, ref childPixelSizeInCacheBeforeViewport, ref childLogicalSizeInCacheBeforeViewport, ref childPixelSizeInCacheAfterViewport, ref childLogicalSizeInCacheAfterViewport);
		}
		else
		{
			childLogicalSize = new Size(1.0, 1.0);
			if (isBeforeFirstItem)
			{
				childPixelSizeInCacheBeforeViewport = childDesiredSize;
				childLogicalSizeInCacheBeforeViewport = new Size(DoubleUtil.GreaterThanZero(childPixelSizeInCacheBeforeViewport.Width) ? 1 : 0, DoubleUtil.GreaterThanZero(childPixelSizeInCacheBeforeViewport.Height) ? 1 : 0);
			}
			else if (isAfterLastItem)
			{
				childPixelSizeInCacheAfterViewport = childDesiredSize;
				childLogicalSizeInCacheAfterViewport = new Size(DoubleUtil.GreaterThanZero(childPixelSizeInCacheAfterViewport.Width) ? 1 : 0, DoubleUtil.GreaterThanZero(childPixelSizeInCacheAfterViewport.Height) ? 1 : 0);
			}
			else
			{
				GetSizesForChildIntersectingTheViewport(isHorizontal, isChildHorizontal, childPixelSize, childLogicalSize, childViewport, ref childPixelSizeInViewport, ref childLogicalSizeInViewport, ref childPixelSizeInCacheBeforeViewport, ref childLogicalSizeInCacheBeforeViewport, ref childPixelSizeInCacheAfterViewport, ref childLogicalSizeInCacheAfterViewport);
			}
		}
	}

	private void GetSizesForChildIntersectingTheViewport(bool isHorizontal, bool childIsHorizontal, Size childPixelSize, Size childLogicalSize, Rect childViewport, ref Size childPixelSizeInViewport, ref Size childLogicalSizeInViewport, ref Size childPixelSizeInCacheBeforeViewport, ref Size childLogicalSizeInCacheBeforeViewport, ref Size childPixelSizeInCacheAfterViewport, ref Size childLogicalSizeInCacheAfterViewport)
	{
		bool isVSP45Compat = IsVSP45Compat;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		if (isHorizontal)
		{
			if (IsPixelBased)
			{
				if (childIsHorizontal != isHorizontal && (DoubleUtil.GreaterThanOrClose(childViewport.Y, childPixelSize.Height) || DoubleUtil.AreClose(childViewport.Height, 0.0)))
				{
					return;
				}
				num3 = (DoubleUtil.LessThan(childViewport.X, childPixelSize.Width) ? Math.Max(childViewport.X, 0.0) : childPixelSize.Width);
				num = Math.Min(childViewport.Width, childPixelSize.Width - num3);
				num5 = Math.Max(childPixelSize.Width - num - num3, 0.0);
			}
			else
			{
				if (childIsHorizontal != isHorizontal && (DoubleUtil.GreaterThanOrClose(childViewport.Y, childLogicalSize.Height) || DoubleUtil.AreClose(childViewport.Height, 0.0)))
				{
					return;
				}
				if (DoubleUtil.GreaterThanOrClose(childViewport.X, childLogicalSize.Width))
				{
					num3 = childPixelSize.Width;
					if (!isVSP45Compat)
					{
						num4 = childLogicalSize.Width;
					}
				}
				else if (DoubleUtil.GreaterThanZero(childViewport.Width))
				{
					num = childPixelSize.Width;
				}
				else
				{
					num5 = childPixelSize.Width;
					if (!isVSP45Compat)
					{
						num6 = childLogicalSize.Width;
					}
				}
			}
			if (DoubleUtil.GreaterThanZero(childPixelSize.Width))
			{
				num4 = Math.Floor(childLogicalSize.Width * num3 / childPixelSize.Width);
				num6 = Math.Floor(childLogicalSize.Width * num5 / childPixelSize.Width);
				num2 = childLogicalSize.Width - num4 - num6;
			}
			else if (!isVSP45Compat)
			{
				num2 = childLogicalSize.Width - num4 - num6;
			}
			double val = Math.Min(childViewport.Height, childPixelSize.Height - Math.Max(childViewport.Y, 0.0));
			childPixelSizeInViewport.Width += num;
			childPixelSizeInViewport.Height = Math.Max(childPixelSizeInViewport.Height, val);
			childPixelSizeInCacheBeforeViewport.Width += num3;
			childPixelSizeInCacheBeforeViewport.Height = Math.Max(childPixelSizeInCacheBeforeViewport.Height, val);
			childPixelSizeInCacheAfterViewport.Width += num5;
			childPixelSizeInCacheAfterViewport.Height = Math.Max(childPixelSizeInCacheAfterViewport.Height, val);
			childLogicalSizeInViewport.Width += num2;
			childLogicalSizeInViewport.Height = Math.Max(childLogicalSizeInViewport.Height, childLogicalSize.Height);
			childLogicalSizeInCacheBeforeViewport.Width += num4;
			childLogicalSizeInCacheBeforeViewport.Height = Math.Max(childLogicalSizeInCacheBeforeViewport.Height, childLogicalSize.Height);
			childLogicalSizeInCacheAfterViewport.Width += num6;
			childLogicalSizeInCacheAfterViewport.Height = Math.Max(childLogicalSizeInCacheAfterViewport.Height, childLogicalSize.Height);
			return;
		}
		if (IsPixelBased)
		{
			if (childIsHorizontal != isHorizontal && (DoubleUtil.GreaterThanOrClose(childViewport.X, childPixelSize.Width) || DoubleUtil.AreClose(childViewport.Width, 0.0)))
			{
				return;
			}
			num3 = (DoubleUtil.LessThan(childViewport.Y, childPixelSize.Height) ? Math.Max(childViewport.Y, 0.0) : childPixelSize.Height);
			num = Math.Min(childViewport.Height, childPixelSize.Height - num3);
			num5 = Math.Max(childPixelSize.Height - num - num3, 0.0);
		}
		else
		{
			if (childIsHorizontal != isHorizontal && (DoubleUtil.GreaterThanOrClose(childViewport.X, childLogicalSize.Width) || DoubleUtil.AreClose(childViewport.Width, 0.0)))
			{
				return;
			}
			if (DoubleUtil.GreaterThanOrClose(childViewport.Y, childLogicalSize.Height))
			{
				num3 = childPixelSize.Height;
				if (!isVSP45Compat)
				{
					num4 = childLogicalSize.Height;
				}
			}
			else if (DoubleUtil.GreaterThanZero(childViewport.Height))
			{
				num = childPixelSize.Height;
			}
			else
			{
				num5 = childPixelSize.Height;
				if (!isVSP45Compat)
				{
					num6 = childLogicalSize.Height;
				}
			}
		}
		if (DoubleUtil.GreaterThanZero(childPixelSize.Height))
		{
			num4 = Math.Floor(childLogicalSize.Height * num3 / childPixelSize.Height);
			num6 = Math.Floor(childLogicalSize.Height * num5 / childPixelSize.Height);
			num2 = childLogicalSize.Height - num4 - num6;
		}
		else if (!IsVSP45Compat)
		{
			num2 = childLogicalSize.Height - num4 - num6;
		}
		double val2 = Math.Min(childViewport.Width, childPixelSize.Width - Math.Max(childViewport.X, 0.0));
		childPixelSizeInViewport.Height += num;
		childPixelSizeInViewport.Width = Math.Max(childPixelSizeInViewport.Width, val2);
		childPixelSizeInCacheBeforeViewport.Height += num3;
		childPixelSizeInCacheBeforeViewport.Width = Math.Max(childPixelSizeInCacheBeforeViewport.Width, val2);
		childPixelSizeInCacheAfterViewport.Height += num5;
		childPixelSizeInCacheAfterViewport.Width = Math.Max(childPixelSizeInCacheAfterViewport.Width, val2);
		childLogicalSizeInViewport.Height += num2;
		childLogicalSizeInViewport.Width = Math.Max(childLogicalSizeInViewport.Width, childLogicalSize.Width);
		childLogicalSizeInCacheBeforeViewport.Height += num4;
		childLogicalSizeInCacheBeforeViewport.Width = Math.Max(childLogicalSizeInCacheBeforeViewport.Width, childLogicalSize.Width);
		childLogicalSizeInCacheAfterViewport.Height += num6;
		childLogicalSizeInCacheAfterViewport.Width = Math.Max(childLogicalSizeInCacheAfterViewport.Width, childLogicalSize.Width);
	}

	private void UpdateStackSizes(bool isHorizontal, bool foundFirstItemInViewport, Size childPixelSize, Size childPixelSizeInViewport, Size childPixelSizeInCacheBeforeViewport, Size childPixelSizeInCacheAfterViewport, Size childLogicalSize, Size childLogicalSizeInViewport, Size childLogicalSizeInCacheBeforeViewport, Size childLogicalSizeInCacheAfterViewport, ref Size stackPixelSize, ref Size stackPixelSizeInViewport, ref Size stackPixelSizeInCacheBeforeViewport, ref Size stackPixelSizeInCacheAfterViewport, ref Size stackLogicalSize, ref Size stackLogicalSizeInViewport, ref Size stackLogicalSizeInCacheBeforeViewport, ref Size stackLogicalSizeInCacheAfterViewport)
	{
		StackSizes(isHorizontal, ref stackPixelSize, childPixelSize);
		StackSizes(isHorizontal, ref stackLogicalSize, childLogicalSize);
		if (foundFirstItemInViewport)
		{
			StackSizes(isHorizontal, ref stackPixelSizeInViewport, childPixelSizeInViewport);
			StackSizes(isHorizontal, ref stackLogicalSizeInViewport, childLogicalSizeInViewport);
			StackSizes(isHorizontal, ref stackPixelSizeInCacheBeforeViewport, childPixelSizeInCacheBeforeViewport);
			StackSizes(isHorizontal, ref stackLogicalSizeInCacheBeforeViewport, childLogicalSizeInCacheBeforeViewport);
			StackSizes(isHorizontal, ref stackPixelSizeInCacheAfterViewport, childPixelSizeInCacheAfterViewport);
			StackSizes(isHorizontal, ref stackLogicalSizeInCacheAfterViewport, childLogicalSizeInCacheAfterViewport);
		}
	}

	private static void StackSizes(bool isHorizontal, ref Size sz1, Size sz2)
	{
		if (isHorizontal)
		{
			sz1.Width += sz2.Width;
			sz1.Height = Math.Max(sz1.Height, sz2.Height);
		}
		else
		{
			sz1.Height += sz2.Height;
			sz1.Width = Math.Max(sz1.Width, sz2.Width);
		}
	}

	private void SyncUniformSizeFlags(object parentItem, IContainItemStorage parentItemStorageProvider, IList children, IList items, IContainItemStorage itemStorageProvider, int itemCount, bool computedAreContainersUniformlySized, double computedUniformOrAverageContainerSize, ref bool areContainersUniformlySized, ref double uniformOrAverageContainerSize, ref bool hasAverageContainerSizeChanged, bool isHorizontal, bool evaluateAreContainersUniformlySized)
	{
		parentItemStorageProvider = itemStorageProvider;
		if (evaluateAreContainersUniformlySized || areContainersUniformlySized != computedAreContainersUniformlySized)
		{
			if (!evaluateAreContainersUniformlySized)
			{
				areContainersUniformlySized = computedAreContainersUniformlySized;
				SetAreContainersUniformlySized(parentItemStorageProvider, parentItem, areContainersUniformlySized);
			}
			for (int i = 0; i < children.Count; i++)
			{
				if (!(children[i] is UIElement uIElement) || !VirtualizingPanel.GetShouldCacheContainerSize(uIElement))
				{
					continue;
				}
				IHierarchicalVirtualizationAndScrollInfo virtualizingChild = GetVirtualizingChild(uIElement);
				Size size;
				if (virtualizingChild == null)
				{
					size = ((!IsPixelBased) ? new Size(DoubleUtil.GreaterThanZero(uIElement.DesiredSize.Width) ? 1 : 0, DoubleUtil.GreaterThanZero(uIElement.DesiredSize.Height) ? 1 : 0) : uIElement.DesiredSize);
				}
				else
				{
					HierarchicalVirtualizationHeaderDesiredSizes headerDesiredSizes = virtualizingChild.HeaderDesiredSizes;
					HierarchicalVirtualizationItemDesiredSizes itemDesiredSizes = virtualizingChild.ItemDesiredSizes;
					size = ((!IsPixelBased) ? new Size(Math.Max(headerDesiredSizes.LogicalSize.Width, itemDesiredSizes.LogicalSize.Width), headerDesiredSizes.LogicalSize.Height + itemDesiredSizes.LogicalSize.Height) : new Size(Math.Max(headerDesiredSizes.PixelSize.Width, itemDesiredSizes.PixelSize.Width), headerDesiredSizes.PixelSize.Height + itemDesiredSizes.PixelSize.Height));
				}
				if (evaluateAreContainersUniformlySized && computedAreContainersUniformlySized)
				{
					computedAreContainersUniformlySized = ((!isHorizontal) ? DoubleUtil.AreClose(size.Height, uniformOrAverageContainerSize) : DoubleUtil.AreClose(size.Width, uniformOrAverageContainerSize));
					if (!computedAreContainersUniformlySized)
					{
						i = -1;
					}
				}
				else
				{
					itemStorageProvider.StoreItemValue(((ItemContainerGenerator)base.Generator).ItemFromContainer(uIElement), ContainerSizeProperty, size);
				}
			}
			if (evaluateAreContainersUniformlySized)
			{
				areContainersUniformlySized = computedAreContainersUniformlySized;
				SetAreContainersUniformlySized(parentItemStorageProvider, parentItem, areContainersUniformlySized);
			}
		}
		if (!computedAreContainersUniformlySized)
		{
			double num = 0.0;
			int num2 = 0;
			for (int j = 0; j < itemCount; j++)
			{
				object obj = itemStorageProvider.ReadItemValue(items[j], ContainerSizeProperty);
				if (obj != null)
				{
					Size size2 = (Size)obj;
					if (isHorizontal)
					{
						num += size2.Width;
						num2++;
					}
					else
					{
						num += size2.Height;
						num2++;
					}
				}
			}
			if (num2 > 0)
			{
				if (IsPixelBased)
				{
					uniformOrAverageContainerSize = num / (double)num2;
				}
				else
				{
					uniformOrAverageContainerSize = Math.Round(num / (double)num2);
				}
			}
		}
		else
		{
			uniformOrAverageContainerSize = computedUniformOrAverageContainerSize;
		}
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.SyncAveSize, uniformOrAverageContainerSize, areContainersUniformlySized, hasAverageContainerSizeChanged);
		}
	}

	private void SyncUniformSizeFlags(object parentItem, IContainItemStorage parentItemStorageProvider, IList children, IList items, IContainItemStorage itemStorageProvider, int itemCount, bool computedAreContainersUniformlySized, double computedUniformOrAverageContainerSize, double computedUniformOrAverageContainerPixelSize, ref bool areContainersUniformlySized, ref double uniformOrAverageContainerSize, ref double uniformOrAverageContainerPixelSize, ref bool hasAverageContainerSizeChanged, bool isHorizontal, bool evaluateAreContainersUniformlySized)
	{
		bool flag = ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this);
		ItemContainerGenerator itemContainerGenerator = (ItemContainerGenerator)base.Generator;
		if (evaluateAreContainersUniformlySized || areContainersUniformlySized != computedAreContainersUniformlySized)
		{
			if (!evaluateAreContainersUniformlySized)
			{
				areContainersUniformlySized = computedAreContainersUniformlySized;
				SetAreContainersUniformlySized(parentItemStorageProvider, parentItem, areContainersUniformlySized);
			}
			for (int i = 0; i < children.Count; i++)
			{
				if (!(children[i] is UIElement uIElement) || !VirtualizingPanel.GetShouldCacheContainerSize(uIElement))
				{
					continue;
				}
				IHierarchicalVirtualizationAndScrollInfo virtualizingChild = GetVirtualizingChild(uIElement);
				Size size;
				Size size2;
				if (virtualizingChild != null)
				{
					HierarchicalVirtualizationItemDesiredSizes itemDesiredSizes = virtualizingChild.ItemDesiredSizes;
					object obj = uIElement.ReadLocalValue(ItemsHostInsetProperty);
					if (obj != DependencyProperty.UnsetValue)
					{
						Thickness thickness = (Thickness)obj;
						size = new Size(thickness.Left + itemDesiredSizes.PixelSize.Width + thickness.Right, thickness.Top + itemDesiredSizes.PixelSize.Height + thickness.Bottom);
					}
					else
					{
						size = uIElement.DesiredSize;
					}
					size2 = ((!IsPixelBased) ? (isHorizontal ? new Size(1.0 + itemDesiredSizes.LogicalSize.Width, Math.Max(1.0, itemDesiredSizes.LogicalSize.Height)) : new Size(Math.Max(1.0, itemDesiredSizes.LogicalSize.Width), 1.0 + itemDesiredSizes.LogicalSize.Height)) : size);
				}
				else
				{
					size = uIElement.DesiredSize;
					size2 = ((!IsPixelBased) ? new Size(DoubleUtil.GreaterThanZero(uIElement.DesiredSize.Width) ? 1 : 0, DoubleUtil.GreaterThanZero(uIElement.DesiredSize.Height) ? 1 : 0) : size);
				}
				if (evaluateAreContainersUniformlySized && computedAreContainersUniformlySized)
				{
					computedAreContainersUniformlySized = ((!isHorizontal) ? (DoubleUtil.AreClose(size2.Height, uniformOrAverageContainerSize) && (IsPixelBased || DoubleUtil.AreClose(size.Height, uniformOrAverageContainerPixelSize))) : (DoubleUtil.AreClose(size2.Width, uniformOrAverageContainerSize) && (IsPixelBased || DoubleUtil.AreClose(size.Width, uniformOrAverageContainerPixelSize))));
					if (!computedAreContainersUniformlySized)
					{
						i = -1;
					}
					continue;
				}
				if (IsPixelBased)
				{
					if (flag)
					{
						ScrollTracer.Trace(this, ScrollTraceOp.SetContainerSize, itemContainerGenerator.IndexFromContainer(uIElement), size2);
					}
					itemStorageProvider.StoreItemValue(itemContainerGenerator.ItemFromContainer(uIElement), ContainerSizeProperty, size2);
					continue;
				}
				if (flag)
				{
					ScrollTracer.Trace(this, ScrollTraceOp.SetContainerSize, itemContainerGenerator.IndexFromContainer(uIElement), size2, size);
				}
				ContainerSizeDual value = new ContainerSizeDual(size, size2);
				itemStorageProvider.StoreItemValue(itemContainerGenerator.ItemFromContainer(uIElement), ContainerSizeDualProperty, value);
			}
			if (evaluateAreContainersUniformlySized)
			{
				areContainersUniformlySized = computedAreContainersUniformlySized;
				SetAreContainersUniformlySized(parentItemStorageProvider, parentItem, areContainersUniformlySized);
			}
		}
		if (!computedAreContainersUniformlySized)
		{
			Size size3 = default(Size);
			Size size4 = default(Size);
			double num = 0.0;
			double num2 = 0.0;
			int num3 = 0;
			for (int j = 0; j < itemCount; j++)
			{
				object obj2 = null;
				if (IsPixelBased)
				{
					obj2 = itemStorageProvider.ReadItemValue(items[j], ContainerSizeProperty);
					if (obj2 != null)
					{
						size3 = (Size)obj2;
						size4 = size3;
					}
				}
				else
				{
					obj2 = itemStorageProvider.ReadItemValue(items[j], ContainerSizeDualProperty);
					if (obj2 != null)
					{
						ContainerSizeDual obj3 = (ContainerSizeDual)obj2;
						size3 = obj3.ItemSize;
						size4 = obj3.PixelSize;
					}
				}
				if (obj2 != null)
				{
					if (isHorizontal)
					{
						num += size3.Width;
						num2 += size4.Width;
						num3++;
					}
					else
					{
						num += size3.Height;
						num2 += size4.Height;
						num3++;
					}
				}
			}
			if (num3 > 0)
			{
				uniformOrAverageContainerPixelSize = num2 / (double)num3;
				if (base.UseLayoutRounding)
				{
					DpiScale dpi = GetDpi();
					double num4 = (isHorizontal ? dpi.DpiScaleX : dpi.DpiScaleY);
					uniformOrAverageContainerPixelSize = UIElement.RoundLayoutValue(Math.Max(uniformOrAverageContainerPixelSize, num4), num4);
				}
				if (IsPixelBased)
				{
					uniformOrAverageContainerSize = uniformOrAverageContainerPixelSize;
				}
				else
				{
					uniformOrAverageContainerSize = Math.Round(num / (double)num3);
				}
				if (SetUniformOrAverageContainerSize(parentItemStorageProvider, parentItem, uniformOrAverageContainerSize, uniformOrAverageContainerPixelSize))
				{
					hasAverageContainerSizeChanged = true;
				}
			}
		}
		else
		{
			uniformOrAverageContainerSize = computedUniformOrAverageContainerSize;
			uniformOrAverageContainerPixelSize = computedUniformOrAverageContainerPixelSize;
		}
		if (flag)
		{
			ScrollTracer.Trace(this, ScrollTraceOp.SyncAveSize, uniformOrAverageContainerSize, uniformOrAverageContainerPixelSize, areContainersUniformlySized, hasAverageContainerSizeChanged);
		}
	}

	private void ClearAsyncOperations()
	{
		bool isVSP45Compat = IsVSP45Compat;
		if (isVSP45Compat)
		{
			DispatcherOperation value = MeasureCachesOperationField.GetValue(this);
			if (value != null)
			{
				value.Abort();
				MeasureCachesOperationField.ClearValue(this);
			}
		}
		else
		{
			ClearMeasureCachesState();
		}
		DispatcherOperation value2 = AnchorOperationField.GetValue(this);
		if (value2 != null)
		{
			if (isVSP45Compat)
			{
				value2.Abort();
				AnchorOperationField.ClearValue(this);
			}
			else
			{
				ClearAnchorInformation(shouldAbort: true);
			}
		}
		DispatcherOperation value3 = AnchoredInvalidateMeasureOperationField.GetValue(this);
		if (value3 != null)
		{
			value3.Abort();
			AnchoredInvalidateMeasureOperationField.ClearValue(this);
		}
		DispatcherOperation value4 = ClearIsScrollActiveOperationField.GetValue(this);
		if (value4 != null)
		{
			if (isVSP45Compat)
			{
				value4.Abort();
				ClearIsScrollActiveOperationField.ClearValue(this);
			}
			else
			{
				value4.Abort();
				ClearIsScrollActive();
			}
		}
	}

	private bool GetAreContainersUniformlySized(IContainItemStorage itemStorageProvider, object item)
	{
		if (item == this)
		{
			if (AreContainersUniformlySized.HasValue)
			{
				return AreContainersUniformlySized.Value;
			}
		}
		else
		{
			object obj = itemStorageProvider.ReadItemValue(item, AreContainersUniformlySizedProperty);
			if (obj != null)
			{
				return (bool)obj;
			}
		}
		return true;
	}

	private void SetAreContainersUniformlySized(IContainItemStorage itemStorageProvider, object item, bool value)
	{
		if (item == this)
		{
			AreContainersUniformlySized = value;
		}
		else
		{
			itemStorageProvider.StoreItemValue(item, AreContainersUniformlySizedProperty, value);
		}
	}

	private double GetUniformOrAverageContainerSize(IContainItemStorage itemStorageProvider, object item)
	{
		GetUniformOrAverageContainerSize(itemStorageProvider, item, IsPixelBased || IsVSP45Compat, out var uniformOrAverageContainerSize, out var _);
		return uniformOrAverageContainerSize;
	}

	private void GetUniformOrAverageContainerSize(IContainItemStorage itemStorageProvider, object item, bool isSingleValue, out double uniformOrAverageContainerSize, out double uniformOrAverageContainerPixelSize)
	{
		GetUniformOrAverageContainerSize(itemStorageProvider, item, isSingleValue, out uniformOrAverageContainerSize, out uniformOrAverageContainerPixelSize, out var _);
	}

	private void GetUniformOrAverageContainerSize(IContainItemStorage itemStorageProvider, object item, bool isSingleValue, out double uniformOrAverageContainerSize, out double uniformOrAverageContainerPixelSize, out bool hasUniformOrAverageContainerSizeBeenSet)
	{
		if (item == this)
		{
			if (UniformOrAverageContainerSize.HasValue)
			{
				hasUniformOrAverageContainerSizeBeenSet = true;
				uniformOrAverageContainerSize = UniformOrAverageContainerSize.Value;
				if (isSingleValue)
				{
					uniformOrAverageContainerPixelSize = uniformOrAverageContainerSize;
				}
				else
				{
					uniformOrAverageContainerPixelSize = UniformOrAverageContainerPixelSize.Value;
				}
				return;
			}
		}
		else if (isSingleValue)
		{
			object obj = itemStorageProvider.ReadItemValue(item, UniformOrAverageContainerSizeProperty);
			if (obj != null)
			{
				hasUniformOrAverageContainerSizeBeenSet = true;
				uniformOrAverageContainerSize = (double)obj;
				uniformOrAverageContainerPixelSize = uniformOrAverageContainerSize;
				return;
			}
		}
		else
		{
			object obj2 = itemStorageProvider.ReadItemValue(item, UniformOrAverageContainerSizeDualProperty);
			if (obj2 != null)
			{
				UniformOrAverageContainerSizeDual uniformOrAverageContainerSizeDual = (UniformOrAverageContainerSizeDual)obj2;
				hasUniformOrAverageContainerSizeBeenSet = true;
				uniformOrAverageContainerSize = uniformOrAverageContainerSizeDual.ItemSize;
				uniformOrAverageContainerPixelSize = uniformOrAverageContainerSizeDual.PixelSize;
				return;
			}
		}
		hasUniformOrAverageContainerSizeBeenSet = false;
		uniformOrAverageContainerPixelSize = 16.0;
		uniformOrAverageContainerSize = (IsPixelBased ? uniformOrAverageContainerPixelSize : 1.0);
	}

	private bool SetUniformOrAverageContainerSize(IContainItemStorage itemStorageProvider, object item, double value, double pixelValue)
	{
		bool result = false;
		if (DoubleUtil.GreaterThanZero(value))
		{
			if (item == this)
			{
				if (UniformOrAverageContainerSize != value)
				{
					UniformOrAverageContainerSize = value;
					UniformOrAverageContainerPixelSize = pixelValue;
					result = true;
				}
			}
			else if (IsPixelBased || IsVSP45Compat)
			{
				object objA = itemStorageProvider.ReadItemValue(item, UniformOrAverageContainerSizeProperty);
				itemStorageProvider.StoreItemValue(item, UniformOrAverageContainerSizeProperty, value);
				result = !object.Equals(objA, value);
			}
			else
			{
				UniformOrAverageContainerSizeDual uniformOrAverageContainerSizeDual = itemStorageProvider.ReadItemValue(item, UniformOrAverageContainerSizeDualProperty) as UniformOrAverageContainerSizeDual;
				UniformOrAverageContainerSizeDual value2 = new UniformOrAverageContainerSizeDual(pixelValue, value);
				itemStorageProvider.StoreItemValue(item, UniformOrAverageContainerSizeDualProperty, value2);
				result = uniformOrAverageContainerSizeDual == null || uniformOrAverageContainerSizeDual.ItemSize != value;
			}
		}
		return result;
	}

	private void MeasureExistingChildBeyondExtendedViewport(ref IItemContainerGenerator generator, ref IContainItemStorage itemStorageProvider, ref IContainItemStorage parentItemStorageProvider, ref object parentItem, ref bool hasUniformOrAverageContainerSizeBeenSet, ref double computedUniformOrAverageContainerSize, ref double computedUniformOrAverageContainerPixelSize, ref bool computedAreContainersUniformlySized, ref bool hasAnyContainerSpanChanged, ref IList items, ref IList children, ref int childIndex, ref bool visualOrderChanged, ref bool isHorizontal, ref Size childConstraint, ref bool foundFirstItemInViewport, ref double firstItemInViewportOffset, ref bool mustDisableVirtualization, ref bool hasVirtualizingChildren, ref bool hasBringIntoViewContainerBeenMeasured, ref long scrollGeneration)
	{
		object item = ((ItemContainerGenerator)generator).ItemFromContainer((UIElement)children[childIndex]);
		Rect viewport = default(Rect);
		VirtualizationCacheLength cacheSize = default(VirtualizationCacheLength);
		VirtualizationCacheLengthUnit cacheUnit = VirtualizationCacheLengthUnit.Pixel;
		Size stackPixelSize = default(Size);
		Size stackPixelSizeInViewport = default(Size);
		Size stackPixelSizeInCacheBeforeViewport = default(Size);
		Size stackPixelSizeInCacheAfterViewport = default(Size);
		Size stackLogicalSize = default(Size);
		Size stackLogicalSizeInViewport = default(Size);
		Size stackLogicalSizeInCacheBeforeViewport = default(Size);
		Size stackLogicalSizeInCacheAfterViewport = default(Size);
		bool isBeforeFirstItem = childIndex < _firstItemInExtendedViewportChildIndex;
		bool isAfterFirstItem = childIndex > _firstItemInExtendedViewportChildIndex;
		bool isAfterLastItem = childIndex > _firstItemInExtendedViewportChildIndex + _actualItemsInExtendedViewportCount;
		bool skipActualMeasure = false;
		bool skipGeneration = true;
		MeasureChild(ref generator, ref itemStorageProvider, ref parentItemStorageProvider, ref parentItem, ref hasUniformOrAverageContainerSizeBeenSet, ref computedUniformOrAverageContainerSize, ref computedUniformOrAverageContainerPixelSize, ref computedAreContainersUniformlySized, ref hasAnyContainerSpanChanged, ref items, ref item, ref children, ref childIndex, ref visualOrderChanged, ref isHorizontal, ref childConstraint, ref viewport, ref cacheSize, ref cacheUnit, ref scrollGeneration, ref foundFirstItemInViewport, ref firstItemInViewportOffset, ref stackPixelSize, ref stackPixelSizeInViewport, ref stackPixelSizeInCacheBeforeViewport, ref stackPixelSizeInCacheAfterViewport, ref stackLogicalSize, ref stackLogicalSizeInViewport, ref stackLogicalSizeInCacheBeforeViewport, ref stackLogicalSizeInCacheAfterViewport, ref mustDisableVirtualization, isBeforeFirstItem, isAfterFirstItem, isAfterLastItem, skipActualMeasure, skipGeneration, ref hasBringIntoViewContainerBeenMeasured, ref hasVirtualizingChildren);
	}

	private void MeasureChild(ref IItemContainerGenerator generator, ref IContainItemStorage itemStorageProvider, ref IContainItemStorage parentItemStorageProvider, ref object parentItem, ref bool hasUniformOrAverageContainerSizeBeenSet, ref double computedUniformOrAverageContainerSize, ref double computedUniformOrAverageContainerPixelSize, ref bool computedAreContainersUniformlySized, ref bool hasAnyContainerSpanChanged, ref IList items, ref object item, ref IList children, ref int childIndex, ref bool visualOrderChanged, ref bool isHorizontal, ref Size childConstraint, ref Rect viewport, ref VirtualizationCacheLength cacheSize, ref VirtualizationCacheLengthUnit cacheUnit, ref long scrollGeneration, ref bool foundFirstItemInViewport, ref double firstItemInViewportOffset, ref Size stackPixelSize, ref Size stackPixelSizeInViewport, ref Size stackPixelSizeInCacheBeforeViewport, ref Size stackPixelSizeInCacheAfterViewport, ref Size stackLogicalSize, ref Size stackLogicalSizeInViewport, ref Size stackLogicalSizeInCacheBeforeViewport, ref Size stackLogicalSizeInCacheAfterViewport, ref bool mustDisableVirtualization, bool isBeforeFirstItem, bool isAfterFirstItem, bool isAfterLastItem, bool skipActualMeasure, bool skipGeneration, ref bool hasBringIntoViewContainerBeenMeasured, ref bool hasVirtualizingChildren)
	{
		UIElement uIElement = null;
		IHierarchicalVirtualizationAndScrollInfo hierarchicalVirtualizationAndScrollInfo = null;
		Rect childViewport = Rect.Empty;
		VirtualizationCacheLength childCacheSize = new VirtualizationCacheLength(0.0);
		VirtualizationCacheLengthUnit childCacheUnit = VirtualizationCacheLengthUnit.Pixel;
		Size size = default(Size);
		if (!skipActualMeasure && !skipGeneration)
		{
			uIElement = generator.GenerateNext(out var isNewlyRealized) as UIElement;
			if (uIElement == null && generator is ItemContainerGenerator itemContainerGenerator)
			{
				itemContainerGenerator.Verify();
			}
			visualOrderChanged |= AddContainerFromGenerator(childIndex, uIElement, isNewlyRealized, isBeforeFirstItem);
		}
		else
		{
			uIElement = (UIElement)children[childIndex];
		}
		hasBringIntoViewContainerBeenMeasured |= uIElement == _bringIntoViewContainer;
		bool isChildHorizontal = isHorizontal;
		hierarchicalVirtualizationAndScrollInfo = GetVirtualizingChild(uIElement, ref isChildHorizontal);
		SetViewportForChild(isHorizontal, itemStorageProvider, computedAreContainersUniformlySized, computedUniformOrAverageContainerSize, mustDisableVirtualization, uIElement, hierarchicalVirtualizationAndScrollInfo, item, isBeforeFirstItem, isAfterFirstItem, firstItemInViewportOffset, viewport, cacheSize, cacheUnit, scrollGeneration, stackPixelSize, stackPixelSizeInViewport, stackPixelSizeInCacheBeforeViewport, stackPixelSizeInCacheAfterViewport, stackLogicalSize, stackLogicalSizeInViewport, stackLogicalSizeInCacheBeforeViewport, stackLogicalSizeInCacheAfterViewport, out childViewport, ref childCacheSize, ref childCacheUnit);
		if (!skipActualMeasure)
		{
			uIElement.Measure(childConstraint);
		}
		size = uIElement.DesiredSize;
		if (hierarchicalVirtualizationAndScrollInfo != null)
		{
			hierarchicalVirtualizationAndScrollInfo = GetVirtualizingChild(uIElement, ref isChildHorizontal);
			mustDisableVirtualization |= (hierarchicalVirtualizationAndScrollInfo != null && hierarchicalVirtualizationAndScrollInfo.MustDisableVirtualization) || isChildHorizontal != isHorizontal;
		}
		Size childPixelSize;
		Size childPixelSizeInViewport;
		Size childPixelSizeInCacheBeforeViewport;
		Size childPixelSizeInCacheAfterViewport;
		Size childLogicalSize;
		Size childLogicalSizeInViewport;
		Size childLogicalSizeInCacheBeforeViewport;
		Size childLogicalSizeInCacheAfterViewport;
		if (IsVSP45Compat)
		{
			GetSizesForChild(isHorizontal, isChildHorizontal, isBeforeFirstItem, isAfterLastItem, hierarchicalVirtualizationAndScrollInfo, size, childViewport, childCacheSize, childCacheUnit, out childPixelSize, out childPixelSizeInViewport, out childPixelSizeInCacheBeforeViewport, out childPixelSizeInCacheAfterViewport, out childLogicalSize, out childLogicalSizeInViewport, out childLogicalSizeInCacheBeforeViewport, out childLogicalSizeInCacheAfterViewport);
		}
		else
		{
			GetSizesForChildWithInset(isHorizontal, isChildHorizontal, isBeforeFirstItem, isAfterLastItem, hierarchicalVirtualizationAndScrollInfo, size, childViewport, childCacheSize, childCacheUnit, out childPixelSize, out childPixelSizeInViewport, out childPixelSizeInCacheBeforeViewport, out childPixelSizeInCacheAfterViewport, out childLogicalSize, out childLogicalSizeInViewport, out childLogicalSizeInCacheBeforeViewport, out childLogicalSizeInCacheAfterViewport);
		}
		UpdateStackSizes(isHorizontal, foundFirstItemInViewport, childPixelSize, childPixelSizeInViewport, childPixelSizeInCacheBeforeViewport, childPixelSizeInCacheAfterViewport, childLogicalSize, childLogicalSizeInViewport, childLogicalSizeInCacheBeforeViewport, childLogicalSizeInCacheAfterViewport, ref stackPixelSize, ref stackPixelSizeInViewport, ref stackPixelSizeInCacheBeforeViewport, ref stackPixelSizeInCacheAfterViewport, ref stackLogicalSize, ref stackLogicalSizeInViewport, ref stackLogicalSizeInCacheBeforeViewport, ref stackLogicalSizeInCacheAfterViewport);
		if (hierarchicalVirtualizationAndScrollInfo != null)
		{
			hasVirtualizingChildren = true;
		}
		if (VirtualizingPanel.GetShouldCacheContainerSize(uIElement))
		{
			if (IsVSP45Compat)
			{
				SetContainerSizeForItem(itemStorageProvider, parentItemStorageProvider, parentItem, item, IsPixelBased ? childPixelSize : childLogicalSize, isHorizontal, ref hasUniformOrAverageContainerSizeBeenSet, ref computedUniformOrAverageContainerSize, ref computedAreContainersUniformlySized);
			}
			else
			{
				SetContainerSizeForItem(itemStorageProvider, parentItemStorageProvider, parentItem, item, IsPixelBased ? childPixelSize : childLogicalSize, childPixelSize, isHorizontal, hasVirtualizingChildren, ref hasUniformOrAverageContainerSizeBeenSet, ref computedUniformOrAverageContainerSize, ref computedUniformOrAverageContainerPixelSize, ref computedAreContainersUniformlySized, ref hasAnyContainerSpanChanged);
			}
		}
	}

	private void ArrangeFirstItemInExtendedViewport(bool isHorizontal, UIElement child, Size childDesiredSize, double arrangeLength, ref Rect rcChild, ref Size previousChildSize, ref Point previousChildOffset, ref int previousChildItemIndex)
	{
		rcChild.X = 0.0;
		rcChild.Y = 0.0;
		if (IsScrolling)
		{
			if (!IsPixelBased)
			{
				if (isHorizontal)
				{
					rcChild.X = -1.0 * ((IsVSP45Compat || !IsVirtualizing || !HasVirtualizingChildren) ? _previousStackPixelSizeInCacheBeforeViewport.Width : _pixelDistanceToViewport);
					rcChild.Y = -1.0 * _scrollData._computedOffset.Y;
				}
				else
				{
					rcChild.Y = -1.0 * ((IsVSP45Compat || !IsVirtualizing || !HasVirtualizingChildren) ? _previousStackPixelSizeInCacheBeforeViewport.Height : _pixelDistanceToViewport);
					rcChild.X = -1.0 * _scrollData._computedOffset.X;
				}
			}
			else
			{
				rcChild.X = -1.0 * _scrollData._computedOffset.X;
				rcChild.Y = -1.0 * _scrollData._computedOffset.Y;
			}
		}
		if (IsVirtualizing)
		{
			if (IsPixelBased)
			{
				if (isHorizontal)
				{
					rcChild.X += _firstItemInExtendedViewportOffset;
				}
				else
				{
					rcChild.Y += _firstItemInExtendedViewportOffset;
				}
			}
			else if (!IsVSP45Compat && (!IsScrolling || HasVirtualizingChildren))
			{
				if (isHorizontal)
				{
					rcChild.X += _pixelDistanceToFirstContainerInExtendedViewport;
				}
				else
				{
					rcChild.Y += _pixelDistanceToFirstContainerInExtendedViewport;
				}
			}
		}
		bool isChildHorizontal = isHorizontal;
		IHierarchicalVirtualizationAndScrollInfo virtualizingChild = GetVirtualizingChild(child, ref isChildHorizontal);
		if (isHorizontal)
		{
			rcChild.Width = childDesiredSize.Width;
			rcChild.Height = Math.Max(arrangeLength, childDesiredSize.Height);
			previousChildSize = childDesiredSize;
			if (!IsPixelBased && virtualizingChild != null && IsVSP45Compat)
			{
				HierarchicalVirtualizationItemDesiredSizes itemDesiredSizes = virtualizingChild.ItemDesiredSizes;
				previousChildSize.Width = itemDesiredSizes.PixelSizeInViewport.Width;
				if (isChildHorizontal == isHorizontal)
				{
					previousChildSize.Width += itemDesiredSizes.PixelSizeBeforeViewport.Width + itemDesiredSizes.PixelSizeAfterViewport.Width;
				}
				RelativeHeaderPosition relativeHeaderPosition = RelativeHeaderPosition.Top;
				Size pixelSize = virtualizingChild.HeaderDesiredSizes.PixelSize;
				if (relativeHeaderPosition == RelativeHeaderPosition.Left || relativeHeaderPosition == RelativeHeaderPosition.Right)
				{
					previousChildSize.Width += pixelSize.Width;
				}
				else
				{
					previousChildSize.Width = Math.Max(previousChildSize.Width, pixelSize.Width);
				}
			}
		}
		else
		{
			rcChild.Height = childDesiredSize.Height;
			rcChild.Width = Math.Max(arrangeLength, childDesiredSize.Width);
			previousChildSize = childDesiredSize;
			if (!IsPixelBased && virtualizingChild != null && IsVSP45Compat)
			{
				HierarchicalVirtualizationItemDesiredSizes itemDesiredSizes2 = virtualizingChild.ItemDesiredSizes;
				previousChildSize.Height = itemDesiredSizes2.PixelSizeInViewport.Height;
				if (isChildHorizontal == isHorizontal)
				{
					previousChildSize.Height += itemDesiredSizes2.PixelSizeBeforeViewport.Height + itemDesiredSizes2.PixelSizeAfterViewport.Height;
				}
				RelativeHeaderPosition relativeHeaderPosition2 = RelativeHeaderPosition.Top;
				Size pixelSize2 = virtualizingChild.HeaderDesiredSizes.PixelSize;
				if (relativeHeaderPosition2 == RelativeHeaderPosition.Top || relativeHeaderPosition2 == RelativeHeaderPosition.Bottom)
				{
					previousChildSize.Height += pixelSize2.Height;
				}
				else
				{
					previousChildSize.Height = Math.Max(previousChildSize.Height, pixelSize2.Height);
				}
			}
		}
		previousChildItemIndex = _firstItemInExtendedViewportIndex;
		previousChildOffset = rcChild.Location;
		child.Arrange(rcChild);
	}

	private void ArrangeOtherItemsInExtendedViewport(bool isHorizontal, UIElement child, Size childDesiredSize, double arrangeLength, int index, ref Rect rcChild, ref Size previousChildSize, ref Point previousChildOffset, ref int previousChildItemIndex)
	{
		if (isHorizontal)
		{
			rcChild.X += previousChildSize.Width;
			rcChild.Width = childDesiredSize.Width;
			rcChild.Height = Math.Max(arrangeLength, childDesiredSize.Height);
		}
		else
		{
			rcChild.Y += previousChildSize.Height;
			rcChild.Height = childDesiredSize.Height;
			rcChild.Width = Math.Max(arrangeLength, childDesiredSize.Width);
		}
		previousChildSize = childDesiredSize;
		previousChildItemIndex = _firstItemInExtendedViewportIndex + (index - _firstItemInExtendedViewportChildIndex);
		previousChildOffset = rcChild.Location;
		child.Arrange(rcChild);
	}

	private void ArrangeItemsBeyondTheExtendedViewport(bool isHorizontal, UIElement child, Size childDesiredSize, double arrangeLength, IList items, IItemContainerGenerator generator, IContainItemStorage itemStorageProvider, bool areContainersUniformlySized, double uniformOrAverageContainerSize, bool beforeExtendedViewport, ref Rect rcChild, ref Size previousChildSize, ref Point previousChildOffset, ref int previousChildItemIndex)
	{
		if (isHorizontal)
		{
			rcChild.Width = childDesiredSize.Width;
			rcChild.Height = Math.Max(arrangeLength, childDesiredSize.Height);
			if (IsPixelBased)
			{
				int num = ((ItemContainerGenerator)generator).IndexFromContainer(child, returnLocalIndex: true);
				double distance;
				if (beforeExtendedViewport)
				{
					if (previousChildItemIndex == -1)
					{
						ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, 0, num, out distance);
					}
					else
					{
						ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, num, previousChildItemIndex - num, out distance);
					}
					rcChild.X = previousChildOffset.X - distance;
					rcChild.Y = previousChildOffset.Y;
				}
				else
				{
					if (previousChildItemIndex == -1)
					{
						ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, 0, num, out distance);
					}
					else
					{
						ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, previousChildItemIndex, num - previousChildItemIndex, out distance);
					}
					rcChild.X = previousChildOffset.X + distance;
					rcChild.Y = previousChildOffset.Y;
				}
				previousChildItemIndex = num;
			}
			else if (beforeExtendedViewport)
			{
				rcChild.X -= childDesiredSize.Width;
			}
			else
			{
				rcChild.X += previousChildSize.Width;
			}
		}
		else
		{
			rcChild.Height = childDesiredSize.Height;
			rcChild.Width = Math.Max(arrangeLength, childDesiredSize.Width);
			if (IsPixelBased)
			{
				int num2 = ((ItemContainerGenerator)generator).IndexFromContainer(child, returnLocalIndex: true);
				double distance2;
				if (beforeExtendedViewport)
				{
					if (previousChildItemIndex == -1)
					{
						ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, 0, num2, out distance2);
					}
					else
					{
						ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, num2, previousChildItemIndex - num2, out distance2);
					}
					rcChild.Y = previousChildOffset.Y - distance2;
					rcChild.X = previousChildOffset.X;
				}
				else
				{
					if (previousChildItemIndex == -1)
					{
						ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, 0, num2, out distance2);
					}
					else
					{
						ComputeDistance(items, itemStorageProvider, isHorizontal, areContainersUniformlySized, uniformOrAverageContainerSize, previousChildItemIndex, num2 - previousChildItemIndex, out distance2);
					}
					rcChild.Y = previousChildOffset.Y + distance2;
					rcChild.X = previousChildOffset.X;
				}
				previousChildItemIndex = num2;
			}
			else if (beforeExtendedViewport)
			{
				rcChild.Y -= childDesiredSize.Height;
			}
			else
			{
				rcChild.Y += previousChildSize.Height;
			}
		}
		previousChildSize = childDesiredSize;
		previousChildOffset = rcChild.Location;
		child.Arrange(rcChild);
	}

	private void InsertNewContainer(int childIndex, UIElement container)
	{
		InsertContainer(childIndex, container, isRecycled: false);
	}

	private bool InsertRecycledContainer(int childIndex, UIElement container)
	{
		return InsertContainer(childIndex, container, isRecycled: true);
	}

	private bool InsertContainer(int childIndex, UIElement container, bool isRecycled)
	{
		bool result = false;
		UIElementCollection internalChildren = base.InternalChildren;
		int num = 0;
		if (childIndex > 0)
		{
			num = ChildIndexFromRealizedIndex(childIndex - 1);
			num++;
		}
		else
		{
			num = ChildIndexFromRealizedIndex(childIndex);
		}
		if (!isRecycled || num >= internalChildren.Count || internalChildren[num] != container)
		{
			if (num < internalChildren.Count)
			{
				int index = num;
				if (isRecycled && container.InternalVisualParent != null)
				{
					internalChildren.MoveVisualChild(container, internalChildren[num]);
					result = true;
				}
				else
				{
					VirtualizingPanel.InsertInternalChild(internalChildren, index, container);
				}
			}
			else if (isRecycled && container.InternalVisualParent != null)
			{
				internalChildren.MoveVisualChild(container, null);
				result = true;
			}
			else
			{
				VirtualizingPanel.AddInternalChild(internalChildren, container);
			}
		}
		if (IsVirtualizing && InRecyclingMode)
		{
			if (ItemsChangedDuringMeasure)
			{
				_realizedChildren = null;
			}
			if (_realizedChildren != null)
			{
				_realizedChildren.Insert(childIndex, container);
			}
			else
			{
				EnsureRealizedChildren();
			}
		}
		base.Generator.PrepareItemContainer(container);
		return result;
	}

	private void EnsureCleanupOperation(bool delay)
	{
		if (delay)
		{
			bool flag = true;
			if (_cleanupOperation != null)
			{
				flag = _cleanupOperation.Abort();
				if (flag)
				{
					_cleanupOperation = null;
				}
			}
			if (flag && _cleanupDelay == null)
			{
				_cleanupDelay = new DispatcherTimer();
				_cleanupDelay.Tick += OnDelayCleanup;
				_cleanupDelay.Interval = TimeSpan.FromMilliseconds(500.0);
				_cleanupDelay.Start();
			}
		}
		else if (_cleanupOperation == null && _cleanupDelay == null)
		{
			_cleanupOperation = base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(OnCleanUp), null);
		}
	}

	private bool PreviousChildIsGenerated(int childIndex)
	{
		GeneratorPosition position = new GeneratorPosition(childIndex, 0);
		position = base.Generator.GeneratorPositionFromIndex(base.Generator.IndexFromGeneratorPosition(position) - 1);
		if (position.Offset == 0)
		{
			return position.Index >= 0;
		}
		return false;
	}

	private bool AddContainerFromGenerator(int childIndex, UIElement child, bool newlyRealized, bool isBeforeViewport)
	{
		bool result = false;
		if (!newlyRealized)
		{
			if (InRecyclingMode)
			{
				IList realizedChildren = RealizedChildren;
				if (childIndex < 0 || childIndex >= realizedChildren.Count || realizedChildren[childIndex] != child)
				{
					result = InsertRecycledContainer(childIndex, child);
				}
			}
		}
		else
		{
			InsertNewContainer(childIndex, child);
		}
		return result;
	}

	private void OnItemsRemove(ItemsChangedEventArgs args)
	{
		RemoveChildRange(args.Position, args.ItemCount, args.ItemUICount);
	}

	private void OnItemsReplace(ItemsChangedEventArgs args)
	{
		if (args.ItemUICount <= 0)
		{
			return;
		}
		UIElementCollection internalChildren = base.InternalChildren;
		using (base.Generator.StartAt(args.Position, GeneratorDirection.Forward, allowStartAtRealizedItem: true))
		{
			for (int i = 0; i < args.ItemUICount; i++)
			{
				int index = args.Position.Index + i;
				bool isNewlyRealized;
				UIElement uIElement = base.Generator.GenerateNext(out isNewlyRealized) as UIElement;
				internalChildren.SetInternal(index, uIElement);
				base.Generator.PrepareItemContainer(uIElement);
			}
		}
	}

	private void OnItemsMove(ItemsChangedEventArgs args)
	{
		RemoveChildRange(args.OldPosition, args.ItemCount, args.ItemUICount);
	}

	private void RemoveChildRange(GeneratorPosition position, int itemCount, int itemUICount)
	{
		if (!base.IsItemsHost)
		{
			return;
		}
		UIElementCollection internalChildren = base.InternalChildren;
		int num = position.Index;
		if (position.Offset > 0)
		{
			num++;
		}
		if (num >= internalChildren.Count)
		{
			return;
		}
		if (itemUICount > 0)
		{
			VirtualizingPanel.RemoveInternalChildRange(internalChildren, num, itemUICount);
			if (IsVirtualizing && InRecyclingMode)
			{
				_realizedChildren.RemoveRange(num, itemUICount);
			}
		}
	}

	private void CleanupContainers(int firstItemInExtendedViewportIndex, int itemsInExtendedViewportCount, ItemsControl itemsControl)
	{
		CleanupContainers(firstItemInExtendedViewportIndex, itemsInExtendedViewportCount, itemsControl, timeBound: false, 0);
	}

	private bool CleanupContainers(int firstItemInExtendedViewportIndex, int itemsInExtendedViewportCount, ItemsControl itemsControl, bool timeBound, int startTickCount)
	{
		IList realizedChildren = RealizedChildren;
		if (realizedChildren.Count == 0)
		{
			return false;
		}
		int num = -1;
		int num2 = 0;
		int num3 = -1;
		int num4 = -1;
		bool flag = false;
		_ = IsVirtualizing;
		bool result = false;
		for (int i = 0; i < realizedChildren.Count; i++)
		{
			if (timeBound && Environment.TickCount - startTickCount > 50 && num2 > 0)
			{
				result = true;
				break;
			}
			UIElement uIElement = (UIElement)realizedChildren[i];
			num4 = num3;
			num3 = GetGeneratedIndex(i);
			object item = itemsControl.ItemContainerGenerator.ItemFromContainer(uIElement);
			if (num3 - num4 != 1)
			{
				flag = true;
			}
			if (flag)
			{
				if (num >= 0 && num2 > 0)
				{
					CleanupRange(realizedChildren, base.Generator, num, num2);
					i -= num2;
					num2 = 0;
					num = -1;
				}
				flag = false;
			}
			if ((num3 < firstItemInExtendedViewportIndex || num3 >= firstItemInExtendedViewportIndex + itemsInExtendedViewportCount) && num3 >= 0 && !((IGeneratorHost)itemsControl).IsItemItsOwnContainer(item) && !uIElement.IsKeyboardFocusWithin && uIElement != _bringIntoViewContainer && NotifyCleanupItem(uIElement, itemsControl) && VirtualizingPanel.GetIsContainerVirtualizable(uIElement))
			{
				if (num == -1)
				{
					num = i;
				}
				num2++;
			}
			else
			{
				flag = true;
			}
		}
		if (num >= 0 && num2 > 0)
		{
			CleanupRange(realizedChildren, base.Generator, num, num2);
		}
		return result;
	}

	private void EnsureRealizedChildren()
	{
		if (_realizedChildren == null)
		{
			UIElementCollection internalChildren = base.InternalChildren;
			_realizedChildren = new List<UIElement>(internalChildren.Count);
			for (int i = 0; i < internalChildren.Count; i++)
			{
				_realizedChildren.Add(internalChildren[i]);
			}
		}
	}

	[Conditional("DEBUG")]
	private void debug_VerifyRealizedChildren()
	{
		ItemContainerGenerator itemContainerGenerator = base.Generator as ItemContainerGenerator;
		ItemsControl itemsOwner = ItemsControl.GetItemsOwner(this);
		if (itemContainerGenerator == null || itemsOwner == null || itemsOwner.IsGrouping)
		{
			return;
		}
		foreach (UIElement internalChild in base.InternalChildren)
		{
			int num = itemContainerGenerator.IndexFromContainer(internalChild);
			if (num != -1)
			{
				base.Generator.GeneratorPositionFromIndex(num);
			}
		}
	}

	[Conditional("DEBUG")]
	private void debug_AssertRealizedChildrenEqualVisualChildren()
	{
		if (IsVirtualizing && InRecyclingMode)
		{
			UIElementCollection internalChildren = base.InternalChildren;
			for (int i = 0; i < internalChildren.Count; i++)
			{
			}
		}
	}

	private int ChildIndexFromRealizedIndex(int realizedChildIndex)
	{
		if (IsVirtualizing && InRecyclingMode && realizedChildIndex < _realizedChildren.Count)
		{
			UIElement uIElement = _realizedChildren[realizedChildIndex];
			UIElementCollection internalChildren = base.InternalChildren;
			for (int i = realizedChildIndex; i < internalChildren.Count; i++)
			{
				if (internalChildren[i] == uIElement)
				{
					return i;
				}
			}
		}
		return realizedChildIndex;
	}

	private void DisconnectRecycledContainers()
	{
		int num = 0;
		UIElement uIElement = ((_realizedChildren.Count > 0) ? _realizedChildren[0] : null);
		UIElementCollection internalChildren = base.InternalChildren;
		for (int i = 0; i < internalChildren.Count; i++)
		{
			UIElement uIElement2 = internalChildren[i];
			if (uIElement2 == uIElement)
			{
				num++;
				uIElement = ((num >= _realizedChildren.Count) ? null : _realizedChildren[num]);
			}
			else
			{
				internalChildren.RemoveNoVerify(uIElement2);
				i--;
			}
		}
	}

	private GeneratorPosition IndexToGeneratorPositionForStart(int index, out int childIndex)
	{
		GeneratorPosition result = base.Generator?.GeneratorPositionFromIndex(index) ?? new GeneratorPosition(-1, index + 1);
		childIndex = ((result.Offset == 0) ? result.Index : (result.Index + 1));
		return result;
	}

	private void OnDelayCleanup(object sender, EventArgs e)
	{
		bool flag = false;
		try
		{
			flag = CleanUp();
		}
		finally
		{
			if (!flag)
			{
				_cleanupDelay.Stop();
				_cleanupDelay = null;
			}
		}
	}

	private object OnCleanUp(object args)
	{
		bool flag = false;
		try
		{
			flag = CleanUp();
		}
		finally
		{
			_cleanupOperation = null;
		}
		if (flag)
		{
			EnsureCleanupOperation(delay: true);
		}
		return null;
	}

	private bool CleanUp()
	{
		ItemsControl itemsControl = null;
		ItemsControl.GetItemsOwnerInternal(this, out itemsControl);
		if (itemsControl == null || !IsVirtualizing || !base.IsItemsHost)
		{
			return false;
		}
		if (!IsVSP45Compat && IsMeasureCachesPending)
		{
			return true;
		}
		int tickCount = Environment.TickCount;
		bool result = false;
		UIElementCollection internalChildren = base.InternalChildren;
		int minDesiredGenerated = MinDesiredGenerated;
		int num = MaxDesiredGenerated - minDesiredGenerated;
		int num2 = internalChildren.Count - num;
		if (HasVirtualizingChildren || num2 > num * 2)
		{
			result = (Mouse.LeftButton == MouseButtonState.Pressed && num2 < 1000) || CleanupContainers(_firstItemInExtendedViewportIndex, _actualItemsInExtendedViewportCount, itemsControl, timeBound: true, tickCount);
		}
		return result;
	}

	private bool NotifyCleanupItem(int childIndex, UIElementCollection children, ItemsControl itemsControl)
	{
		return NotifyCleanupItem(children[childIndex], itemsControl);
	}

	private bool NotifyCleanupItem(UIElement child, ItemsControl itemsControl)
	{
		CleanUpVirtualizedItemEventArgs cleanUpVirtualizedItemEventArgs = new CleanUpVirtualizedItemEventArgs(itemsControl.ItemContainerGenerator.ItemFromContainer(child), child);
		cleanUpVirtualizedItemEventArgs.Source = this;
		OnCleanUpVirtualizedItem(cleanUpVirtualizedItemEventArgs);
		return !cleanUpVirtualizedItemEventArgs.Cancel;
	}

	private void CleanupRange(IList children, IItemContainerGenerator generator, int startIndex, int count)
	{
		if (InRecyclingMode)
		{
			if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
			{
				List<string> list = new List<string>(count);
				for (int i = 0; i < count; i++)
				{
					list.Add(ContainerPath((DependencyObject)children[startIndex + i]));
				}
				ScrollTracer.Trace(this, ScrollTraceOp.RecycleChildren, startIndex, count, list);
			}
			((IRecyclingItemContainerGenerator)generator).Recycle(new GeneratorPosition(startIndex, 0), count);
			_realizedChildren.RemoveRange(startIndex, count);
			return;
		}
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			List<string> list2 = new List<string>(count);
			for (int j = 0; j < count; j++)
			{
				list2.Add(ContainerPath((DependencyObject)children[startIndex + j]));
			}
			ScrollTracer.Trace(this, ScrollTraceOp.RemoveChildren, startIndex, count, list2);
		}
		VirtualizingPanel.RemoveInternalChildRange((UIElementCollection)children, startIndex, count);
		generator.Remove(new GeneratorPosition(startIndex, 0), count);
		AdjustFirstVisibleChildIndex(startIndex, count);
	}

	private void AdjustFirstVisibleChildIndex(int startIndex, int count)
	{
		if (startIndex < _firstItemInExtendedViewportChildIndex)
		{
			if (startIndex + count - 1 < _firstItemInExtendedViewportChildIndex)
			{
				_firstItemInExtendedViewportChildIndex -= count;
			}
			else
			{
				_firstItemInExtendedViewportChildIndex = startIndex;
			}
		}
	}

	private void EnsureScrollData()
	{
		if (_scrollData == null)
		{
			_scrollData = new ScrollData();
		}
	}

	private static void ResetScrolling(VirtualizingStackPanel element)
	{
		element.InvalidateMeasure();
		if (element.IsScrolling)
		{
			element._scrollData.ClearLayout();
		}
	}

	private void OnScrollChange()
	{
		if (ScrollOwner != null)
		{
			ScrollOwner.InvalidateScrollInfo();
		}
	}

	private void SetAndVerifyScrollingData(bool isHorizontal, Rect viewport, Size constraint, UIElement firstContainerInViewport, double firstContainerOffsetFromViewport, bool hasAverageContainerSizeChanged, double newOffset, ref Size stackPixelSize, ref Size stackLogicalSize, ref Size stackPixelSizeInViewport, ref Size stackLogicalSizeInViewport, ref Size stackPixelSizeInCacheBeforeViewport, ref Size stackLogicalSizeInCacheBeforeViewport, ref bool remeasure, ref double? lastPageSafeOffset, ref double? lastPagePixelSize, ref List<double> previouslyMeasuredOffsets)
	{
		Vector vector = new Vector(viewport.Location.X, viewport.Location.Y);
		Vector offset = _scrollData._offset;
		Size size;
		Size size2;
		if (IsPixelBased)
		{
			size = stackPixelSize;
			size2 = viewport.Size;
		}
		else
		{
			size = stackLogicalSize;
			size2 = stackLogicalSizeInViewport;
			if (isHorizontal)
			{
				if (DoubleUtil.GreaterThan(stackPixelSizeInViewport.Width, constraint.Width) && size2.Width > 1.0)
				{
					size2.Width--;
				}
				size2.Height = viewport.Height;
			}
			else
			{
				if (DoubleUtil.GreaterThan(stackPixelSizeInViewport.Height, constraint.Height) && size2.Height > 1.0)
				{
					size2.Height--;
				}
				size2.Width = viewport.Width;
			}
		}
		if (isHorizontal)
		{
			if (MeasureCaches && IsVirtualizing)
			{
				stackPixelSize.Height = _scrollData._extent.Height;
			}
			_scrollData._maxDesiredSize.Height = Math.Max(_scrollData._maxDesiredSize.Height, stackPixelSize.Height);
			stackPixelSize.Height = _scrollData._maxDesiredSize.Height;
			size.Height = stackPixelSize.Height;
			if (double.IsPositiveInfinity(constraint.Height))
			{
				size2.Height = stackPixelSize.Height;
			}
		}
		else
		{
			if (MeasureCaches && IsVirtualizing)
			{
				stackPixelSize.Width = _scrollData._extent.Width;
			}
			_scrollData._maxDesiredSize.Width = Math.Max(_scrollData._maxDesiredSize.Width, stackPixelSize.Width);
			stackPixelSize.Width = _scrollData._maxDesiredSize.Width;
			size.Width = stackPixelSize.Width;
			if (double.IsPositiveInfinity(constraint.Width))
			{
				size2.Width = stackPixelSize.Width;
			}
		}
		if (!double.IsPositiveInfinity(constraint.Width))
		{
			stackPixelSize.Width = ((IsPixelBased || DoubleUtil.AreClose(vector.X, 0.0)) ? Math.Min(stackPixelSize.Width, constraint.Width) : constraint.Width);
		}
		if (!double.IsPositiveInfinity(constraint.Height))
		{
			stackPixelSize.Height = ((IsPixelBased || DoubleUtil.AreClose(vector.Y, 0.0)) ? Math.Min(stackPixelSize.Height, constraint.Height) : constraint.Height);
		}
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.SVSDBegin, "isa:", IsScrollActive, "mc:", MeasureCaches, "o:", _scrollData._offset, "co:", vector, "ex:", size, "vs:", size2, "pxInV:", stackPixelSizeInViewport);
			if (hasAverageContainerSizeChanged)
			{
				ScrollTracer.Trace(this, ScrollTraceOp.SVSDBegin, "acs:", UniformOrAverageContainerSize, UniformOrAverageContainerPixelSize);
			}
		}
		bool flag = ((!isHorizontal) ? (!DoubleUtil.AreClose(vector.Y, _scrollData._offset.Y) || (IsScrollActive && vector.Y > 0.0 && DoubleUtil.GreaterThanOrClose(vector.Y, _scrollData.Extent.Height - _scrollData.Viewport.Height))) : (!DoubleUtil.AreClose(vector.X, _scrollData._offset.X) || (IsScrollActive && vector.X > 0.0 && DoubleUtil.GreaterThanOrClose(vector.X, _scrollData.Extent.Width - _scrollData.Viewport.Width))));
		if (isHorizontal)
		{
			if (!DoubleUtil.AreClose(vector.Y, _scrollData._offset.Y))
			{
				_ = 1;
			}
			else if (IsScrollActive && vector.Y > 0.0)
			{
				DoubleUtil.GreaterThanOrClose(vector.Y, _scrollData.Extent.Height - _scrollData.Viewport.Height);
			}
			else
				_ = 0;
		}
		else if (!DoubleUtil.AreClose(vector.X, _scrollData._offset.X))
		{
			_ = 1;
		}
		else if (IsScrollActive && vector.X > 0.0)
		{
			DoubleUtil.GreaterThanOrClose(vector.X, _scrollData.Extent.Width - _scrollData.Viewport.Width);
		}
		else
			_ = 0;
		bool flag2 = false;
		if (hasAverageContainerSizeChanged && newOffset >= 0.0)
		{
			if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
			{
				ScrollTracer.Trace(this, ScrollTraceOp.AdjustOffset, newOffset, vector);
			}
			if (isHorizontal)
			{
				if (!LayoutDoubleUtil.AreClose(vector.X, newOffset))
				{
					double num = newOffset - vector.X;
					vector.X = newOffset;
					offset.X = newOffset;
					_viewport.X = newOffset;
					_extendedViewport.X += num;
					flag2 = true;
					if (DoubleUtil.GreaterThan(newOffset + size2.Width, size.Width))
					{
						flag = true;
						IsScrollActive = true;
						_scrollData.HorizontalScrollType = ScrollType.ToEnd;
					}
				}
			}
			else if (!LayoutDoubleUtil.AreClose(vector.Y, newOffset))
			{
				double num2 = newOffset - vector.Y;
				vector.Y = newOffset;
				offset.Y = newOffset;
				_viewport.Y = newOffset;
				_extendedViewport.Y += num2;
				if (DoubleUtil.GreaterThan(newOffset + size2.Height, size.Height))
				{
					flag = true;
					IsScrollActive = true;
					_scrollData.VerticalScrollType = ScrollType.ToEnd;
				}
			}
		}
		if (lastPagePixelSize.HasValue && !lastPageSafeOffset.HasValue && !DoubleUtil.AreClose(isHorizontal ? stackPixelSizeInViewport.Width : stackPixelSizeInViewport.Height, lastPagePixelSize.Value))
		{
			flag2 = true;
			flag = true;
			if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
			{
				ScrollTracer.Trace(this, ScrollTraceOp.LastPageSizeChange, vector, stackPixelSizeInViewport, lastPagePixelSize);
			}
		}
		if (flag2)
		{
			if (previouslyMeasuredOffsets != null)
			{
				previouslyMeasuredOffsets.Clear();
			}
			lastPageSafeOffset = null;
			lastPagePixelSize = null;
		}
		bool flag3 = !DoubleUtil.AreClose(size2, _scrollData._viewport);
		bool flag4 = !DoubleUtil.AreClose(size, _scrollData._extent);
		bool flag5 = !DoubleUtil.AreClose(vector, _scrollData._computedOffset);
		bool flag6;
		bool flag7;
		if (flag4)
		{
			flag6 = !DoubleUtil.AreClose(size.Width, _scrollData._extent.Width);
			flag7 = !DoubleUtil.AreClose(size.Height, _scrollData._extent.Height);
		}
		else
		{
			flag6 = (flag7 = false);
		}
		Vector vector2 = vector;
		bool flag8 = false;
		ScrollViewer scrollOwner = ScrollOwner;
		if (scrollOwner.InChildMeasurePass1 || scrollOwner.InChildMeasurePass2)
		{
			if (scrollOwner.VerticalScrollBarVisibility == ScrollBarVisibility.Auto)
			{
				Visibility computedVerticalScrollBarVisibility = scrollOwner.ComputedVerticalScrollBarVisibility;
				Visibility visibility = (DoubleUtil.LessThanOrClose(size.Height, size2.Height) ? Visibility.Collapsed : Visibility.Visible);
				if (computedVerticalScrollBarVisibility != visibility)
				{
					vector2 = offset;
					flag8 = true;
				}
			}
			if (!flag8 && scrollOwner.HorizontalScrollBarVisibility == ScrollBarVisibility.Auto)
			{
				Visibility computedHorizontalScrollBarVisibility = scrollOwner.ComputedHorizontalScrollBarVisibility;
				Visibility visibility2 = (DoubleUtil.LessThanOrClose(size.Width, size2.Width) ? Visibility.Collapsed : Visibility.Visible);
				if (computedHorizontalScrollBarVisibility != visibility2)
				{
					vector2 = offset;
					flag8 = true;
				}
			}
			if (flag8 && ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
			{
				ScrollTracer.Trace(this, ScrollTraceOp.ScrollBarChangeVisibility, vector2);
			}
		}
		if (isHorizontal)
		{
			if (!flag8)
			{
				if (WasOffsetPreviouslyMeasured(previouslyMeasuredOffsets, vector.X))
				{
					if (!IsPixelBased && lastPageSafeOffset.HasValue && !DoubleUtil.AreClose(lastPageSafeOffset.Value, vector.X))
					{
						if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
						{
							ScrollTracer.Trace(this, ScrollTraceOp.RemeasureCycle, vector2.X, lastPageSafeOffset);
						}
						vector2.X = lastPageSafeOffset.Value;
						lastPageSafeOffset = null;
						remeasure = true;
					}
				}
				else if (!remeasure)
				{
					if (!IsPixelBased)
					{
						if (!remeasure && !IsEndOfViewport(isHorizontal, viewport, stackPixelSizeInViewport) && DoubleUtil.GreaterThan(stackLogicalSize.Width, stackLogicalSizeInViewport.Width))
						{
							if (!lastPageSafeOffset.HasValue || vector.X < lastPageSafeOffset.Value)
							{
								lastPageSafeOffset = vector.X;
								lastPagePixelSize = stackPixelSizeInViewport.Width;
							}
							double num3 = stackPixelSizeInViewport.Width / stackLogicalSizeInViewport.Width;
							double num4 = Math.Floor(viewport.Width / num3);
							if (DoubleUtil.GreaterThan(num4, size2.Width))
							{
								if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
								{
									ScrollTracer.Trace(this, ScrollTraceOp.RemeasureEndExpandViewport, "off:", vector.X, lastPageSafeOffset, "pxSz:", stackPixelSizeInViewport.Width, viewport.Width, "itSz:", stackLogicalSizeInViewport.Width, size2.Width, "newVpSz:", num4);
								}
								vector2.X = double.PositiveInfinity;
								size2.Width = num4;
								remeasure = true;
								StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.X);
							}
						}
						if (!remeasure && flag && flag3 && !DoubleUtil.AreClose(_scrollData._viewport.Width, size2.Width))
						{
							if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
							{
								ScrollTracer.Trace(this, ScrollTraceOp.RemeasureEndChangeOffset, "off:", vector.X, "vpSz:", _scrollData._viewport.Width, size2.Width, "newOff:", _scrollData._offset);
							}
							remeasure = true;
							vector2.X = double.PositiveInfinity;
							StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.X);
							if (DoubleUtil.AreClose(size2.Width, 0.0))
							{
								size2.Width = _scrollData._viewport.Width;
							}
						}
					}
					if (!remeasure && flag6)
					{
						if (_scrollData.HorizontalScrollType == ScrollType.ToEnd || (DoubleUtil.GreaterThanZero(vector.X) && DoubleUtil.GreaterThan(vector.X, size.Width - size2.Width)))
						{
							if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
							{
								ScrollTracer.Trace(this, ScrollTraceOp.RemeasureEndExtentChanged, "off:", vector.X, "ext:", _scrollData._extent.Width, size.Width, "vpSz:", size2.Width);
							}
							remeasure = true;
							vector2.X = double.PositiveInfinity;
							_scrollData.HorizontalScrollType = ScrollType.ToEnd;
							StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.X);
						}
						else if (_scrollData.HorizontalScrollType == ScrollType.Absolute && !DoubleUtil.AreClose(_scrollData._extent.Width, 0.0) && !DoubleUtil.AreClose(size.Width, 0.0))
						{
							if (IsPixelBased)
							{
								if (!LayoutDoubleUtil.AreClose(vector.X / size.Width, _scrollData._offset.X / _scrollData._extent.Width))
								{
									remeasure = true;
									vector2.X = size.Width * _scrollData._offset.X / _scrollData._extent.Width;
									StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.X);
								}
							}
							else if (!LayoutDoubleUtil.AreClose(Math.Floor(vector.X) / size.Width, Math.Floor(_scrollData._offset.X) / _scrollData._extent.Width))
							{
								remeasure = true;
								vector2.X = Math.Floor(size.Width * Math.Floor(_scrollData._offset.X) / _scrollData._extent.Width);
								StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.X);
							}
							if ((ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this)) & remeasure)
							{
								ScrollTracer.Trace(this, ScrollTraceOp.RemeasureRatio, "expRat:", _scrollData._offset.X, _scrollData._extent.Width, _scrollData._offset.X / _scrollData._extent.Width, "actRat:", vector.X, size.Width, vector.X / size.Width, "newOff:", vector2.X);
							}
						}
					}
					if (!remeasure && flag7)
					{
						if (_scrollData.VerticalScrollType == ScrollType.ToEnd || (DoubleUtil.GreaterThanZero(vector.Y) && DoubleUtil.GreaterThan(vector.Y, size.Height - size2.Height)))
						{
							if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
							{
								ScrollTracer.Trace(this, ScrollTraceOp.RemeasureEndExtentChanged, "perp", "off:", vector.Y, "ext:", _scrollData._extent.Height, size.Height, "vpSz:", size2.Height);
							}
							remeasure = true;
							vector2.Y = double.PositiveInfinity;
							_scrollData.VerticalScrollType = ScrollType.ToEnd;
						}
						else if (_scrollData.VerticalScrollType == ScrollType.Absolute && !DoubleUtil.AreClose(_scrollData._extent.Height, 0.0) && !DoubleUtil.AreClose(size.Height, 0.0))
						{
							if (!LayoutDoubleUtil.AreClose(vector.Y / size.Height, _scrollData._offset.Y / _scrollData._extent.Height))
							{
								remeasure = true;
								vector2.Y = size.Height * _scrollData._offset.Y / _scrollData._extent.Height;
							}
							if ((ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this)) & remeasure)
							{
								ScrollTracer.Trace(this, ScrollTraceOp.RemeasureRatio, "perp", "expRat:", _scrollData._offset.Y, _scrollData._extent.Height, _scrollData._offset.Y / _scrollData._extent.Height, "actRat:", vector.Y, size.Height, vector.Y / size.Height, "newOff:", vector2.Y);
							}
						}
					}
				}
			}
		}
		else if (!flag8)
		{
			if (WasOffsetPreviouslyMeasured(previouslyMeasuredOffsets, vector.Y))
			{
				if (!IsPixelBased && lastPageSafeOffset.HasValue && !DoubleUtil.AreClose(lastPageSafeOffset.Value, vector.Y))
				{
					if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
					{
						ScrollTracer.Trace(this, ScrollTraceOp.RemeasureCycle, vector2.Y, lastPageSafeOffset);
					}
					vector2.Y = lastPageSafeOffset.Value;
					lastPageSafeOffset = null;
					remeasure = true;
				}
			}
			else if (!remeasure)
			{
				if (!IsPixelBased)
				{
					if (!remeasure && !IsEndOfViewport(isHorizontal, viewport, stackPixelSizeInViewport) && DoubleUtil.GreaterThan(stackLogicalSize.Height, stackLogicalSizeInViewport.Height))
					{
						if (!lastPageSafeOffset.HasValue || vector.Y < lastPageSafeOffset.Value)
						{
							lastPageSafeOffset = vector.Y;
							lastPagePixelSize = stackPixelSizeInViewport.Height;
						}
						double num5 = stackPixelSizeInViewport.Height / stackLogicalSizeInViewport.Height;
						double num6 = Math.Floor(viewport.Height / num5);
						if (DoubleUtil.GreaterThan(num6, size2.Height))
						{
							if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
							{
								ScrollTracer.Trace(this, ScrollTraceOp.RemeasureEndExpandViewport, "off:", vector.Y, lastPageSafeOffset, "pxSz:", stackPixelSizeInViewport.Height, viewport.Height, "itSz:", stackLogicalSizeInViewport.Height, size2.Height, "newVpSz:", num6);
							}
							vector2.Y = double.PositiveInfinity;
							size2.Height = num6;
							remeasure = true;
							StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.Y);
						}
					}
					if (!remeasure && flag && flag3 && !DoubleUtil.AreClose(_scrollData._viewport.Height, size2.Height))
					{
						if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
						{
							ScrollTracer.Trace(this, ScrollTraceOp.RemeasureEndChangeOffset, "off:", vector.Y, "vpSz:", _scrollData._viewport.Height, size2.Height, "newOff:", _scrollData._offset);
						}
						remeasure = true;
						vector2.Y = double.PositiveInfinity;
						StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.Y);
						if (DoubleUtil.AreClose(size2.Height, 0.0))
						{
							size2.Height = _scrollData._viewport.Height;
						}
					}
				}
				if (!remeasure && flag7)
				{
					if (_scrollData.VerticalScrollType == ScrollType.ToEnd || (DoubleUtil.GreaterThanZero(vector.Y) && DoubleUtil.GreaterThan(vector.Y, size.Height - size2.Height)))
					{
						if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
						{
							ScrollTracer.Trace(this, ScrollTraceOp.RemeasureEndExtentChanged, "off:", vector.Y, "ext:", _scrollData._extent.Height, size.Height, "vpSz:", size2.Height);
						}
						remeasure = true;
						vector2.Y = double.PositiveInfinity;
						_scrollData.VerticalScrollType = ScrollType.ToEnd;
						StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.Y);
					}
					else if (_scrollData.VerticalScrollType == ScrollType.Absolute && !DoubleUtil.AreClose(_scrollData._extent.Height, 0.0) && !DoubleUtil.AreClose(size.Height, 0.0))
					{
						if (IsPixelBased)
						{
							if (!LayoutDoubleUtil.AreClose(vector.Y / size.Height, _scrollData._offset.Y / _scrollData._extent.Height))
							{
								remeasure = true;
								vector2.Y = size.Height * _scrollData._offset.Y / _scrollData._extent.Height;
								StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.Y);
							}
						}
						else if (!LayoutDoubleUtil.AreClose(Math.Floor(vector.Y) / size.Height, Math.Floor(_scrollData._offset.Y) / _scrollData._extent.Height))
						{
							remeasure = true;
							vector2.Y = Math.Floor(size.Height * Math.Floor(_scrollData._offset.Y) / _scrollData._extent.Height);
						}
						if ((ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this)) & remeasure)
						{
							ScrollTracer.Trace(this, ScrollTraceOp.RemeasureRatio, "expRat:", _scrollData._offset.Y, _scrollData._extent.Height, _scrollData._offset.Y / _scrollData._extent.Height, "actRat:", vector.Y, size.Height, vector.Y / size.Height, "newOff:", vector2.Y);
						}
					}
				}
				if (!remeasure && flag6)
				{
					if (_scrollData.HorizontalScrollType == ScrollType.ToEnd || (DoubleUtil.GreaterThanZero(vector.X) && DoubleUtil.GreaterThan(vector.X, size.Width - size2.Width)))
					{
						if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
						{
							ScrollTracer.Trace(this, ScrollTraceOp.RemeasureEndExtentChanged, "perp", "off:", vector.X, "ext:", _scrollData._extent.Width, size.Width, "vpSz:", size2.Width);
						}
						remeasure = true;
						vector2.X = double.PositiveInfinity;
						_scrollData.HorizontalScrollType = ScrollType.ToEnd;
					}
					else if (_scrollData.HorizontalScrollType == ScrollType.Absolute && !DoubleUtil.AreClose(_scrollData._extent.Width, 0.0) && !DoubleUtil.AreClose(size.Width, 0.0))
					{
						if (!LayoutDoubleUtil.AreClose(vector.X / size.Width, _scrollData._offset.X / _scrollData._extent.Width))
						{
							remeasure = true;
							vector2.X = size.Width * _scrollData._offset.X / _scrollData._extent.Width;
						}
						if ((ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this)) & remeasure)
						{
							ScrollTracer.Trace(this, ScrollTraceOp.RemeasureRatio, "perp", "expRat:", _scrollData._offset.X, _scrollData._extent.Width, _scrollData._offset.X / _scrollData._extent.Width, "actRat:", vector.X, size.Width, vector.X / size.Width, "newOff:", vector2.X);
						}
					}
				}
			}
		}
		if (remeasure && IsVirtualizing && !IsScrollActive)
		{
			if (isHorizontal && _scrollData.HorizontalScrollType == ScrollType.ToEnd)
			{
				IsScrollActive = true;
			}
			if (!isHorizontal && _scrollData.VerticalScrollType == ScrollType.ToEnd)
			{
				IsScrollActive = true;
			}
		}
		if (!IsVirtualizing && !remeasure)
		{
			ClearIsScrollActive();
		}
		flag3 = !DoubleUtil.AreClose(size2, _scrollData._viewport);
		if (ScrollTracer.IsEnabled && ScrollTracer.IsTracing(this))
		{
			ScrollTracer.Trace(this, ScrollTraceOp.SVSDEnd, "off:", _scrollData._offset, vector2, "ext:", _scrollData._extent, size, "co:", _scrollData._computedOffset, vector, "vp:", _scrollData._viewport, size2);
		}
		if (flag3 || flag4 || flag5)
		{
			Vector computedOffset = _scrollData._computedOffset;
			Size viewport2 = _scrollData._viewport;
			_scrollData._viewport = size2;
			_scrollData._extent = size;
			_scrollData._computedOffset = vector;
			if (flag3)
			{
				OnViewportSizeChanged(viewport2, size2);
			}
			if (flag5)
			{
				OnViewportOffsetChanged(computedOffset, vector);
			}
			OnScrollChange();
		}
		_scrollData._offset = vector2;
	}

	private void SetAndVerifyScrollingData(bool isHorizontal, Rect viewport, Size constraint, ref Size stackPixelSize, ref Size stackLogicalSize, ref Size stackPixelSizeInViewport, ref Size stackLogicalSizeInViewport, ref Size stackPixelSizeInCacheBeforeViewport, ref Size stackLogicalSizeInCacheBeforeViewport, ref bool remeasure, ref double? lastPageSafeOffset, ref List<double> previouslyMeasuredOffsets)
	{
		Vector vector = new Vector(viewport.Location.X, viewport.Location.Y);
		Size size;
		Size size2;
		if (IsPixelBased)
		{
			size = stackPixelSize;
			size2 = viewport.Size;
		}
		else
		{
			size = stackLogicalSize;
			size2 = stackLogicalSizeInViewport;
			if (isHorizontal)
			{
				if (DoubleUtil.GreaterThan(stackPixelSizeInViewport.Width, constraint.Width) && size2.Width > 1.0)
				{
					size2.Width--;
				}
				size2.Height = viewport.Height;
			}
			else
			{
				if (DoubleUtil.GreaterThan(stackPixelSizeInViewport.Height, constraint.Height) && size2.Height > 1.0)
				{
					size2.Height--;
				}
				size2.Width = viewport.Width;
			}
		}
		if (isHorizontal)
		{
			if (MeasureCaches && IsVirtualizing)
			{
				stackPixelSize.Height = _scrollData._extent.Height;
			}
			_scrollData._maxDesiredSize.Height = Math.Max(_scrollData._maxDesiredSize.Height, stackPixelSize.Height);
			stackPixelSize.Height = _scrollData._maxDesiredSize.Height;
			size.Height = stackPixelSize.Height;
			if (double.IsPositiveInfinity(constraint.Height))
			{
				size2.Height = stackPixelSize.Height;
			}
		}
		else
		{
			if (MeasureCaches && IsVirtualizing)
			{
				stackPixelSize.Width = _scrollData._extent.Width;
			}
			_scrollData._maxDesiredSize.Width = Math.Max(_scrollData._maxDesiredSize.Width, stackPixelSize.Width);
			stackPixelSize.Width = _scrollData._maxDesiredSize.Width;
			size.Width = stackPixelSize.Width;
			if (double.IsPositiveInfinity(constraint.Width))
			{
				size2.Width = stackPixelSize.Width;
			}
		}
		if (!double.IsPositiveInfinity(constraint.Width))
		{
			stackPixelSize.Width = ((IsPixelBased || DoubleUtil.AreClose(vector.X, 0.0)) ? Math.Min(stackPixelSize.Width, constraint.Width) : constraint.Width);
		}
		if (!double.IsPositiveInfinity(constraint.Height))
		{
			stackPixelSize.Height = ((IsPixelBased || DoubleUtil.AreClose(vector.Y, 0.0)) ? Math.Min(stackPixelSize.Height, constraint.Height) : constraint.Height);
		}
		bool flag = !DoubleUtil.AreClose(size2, _scrollData._viewport);
		bool flag2 = !DoubleUtil.AreClose(size, _scrollData._extent);
		bool flag3 = !DoubleUtil.AreClose(vector, _scrollData._computedOffset);
		Vector offset = vector;
		bool flag4 = true;
		ScrollViewer scrollOwner = ScrollOwner;
		if (scrollOwner.InChildMeasurePass1 || scrollOwner.InChildMeasurePass2)
		{
			if (scrollOwner.VerticalScrollBarVisibility == ScrollBarVisibility.Auto)
			{
				Visibility computedVerticalScrollBarVisibility = scrollOwner.ComputedVerticalScrollBarVisibility;
				Visibility visibility = (DoubleUtil.LessThanOrClose(size.Height, size2.Height) ? Visibility.Collapsed : Visibility.Visible);
				if (computedVerticalScrollBarVisibility != visibility)
				{
					offset = _scrollData._offset;
					flag4 = false;
				}
			}
			if (flag4 && scrollOwner.HorizontalScrollBarVisibility == ScrollBarVisibility.Auto)
			{
				Visibility computedHorizontalScrollBarVisibility = scrollOwner.ComputedHorizontalScrollBarVisibility;
				Visibility visibility2 = (DoubleUtil.LessThanOrClose(size.Width, size2.Width) ? Visibility.Collapsed : Visibility.Visible);
				if (computedHorizontalScrollBarVisibility != visibility2)
				{
					offset = _scrollData._offset;
					flag4 = false;
				}
			}
		}
		if (isHorizontal)
		{
			if (!WasOffsetPreviouslyMeasured(previouslyMeasuredOffsets, vector.X))
			{
				bool flag5 = !DoubleUtil.AreClose(vector.X, _scrollData._offset.X);
				if (!IsPixelBased)
				{
					if (!IsEndOfViewport(isHorizontal, viewport, stackPixelSizeInViewport) && DoubleUtil.GreaterThan(stackLogicalSize.Width, stackLogicalSizeInViewport.Width))
					{
						lastPageSafeOffset = (lastPageSafeOffset.HasValue ? Math.Min(vector.X, lastPageSafeOffset.Value) : vector.X);
						double num = stackPixelSizeInViewport.Width / stackLogicalSizeInViewport.Width;
						double num2 = Math.Floor(viewport.Width / num);
						if (DoubleUtil.GreaterThan(num2, size2.Width))
						{
							offset.X = double.PositiveInfinity;
							size2.Width = num2;
							remeasure = true;
							StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.X);
						}
					}
					if (!remeasure && flag5 && flag && !DoubleUtil.AreClose(_scrollData._viewport.Width, size2.Width))
					{
						remeasure = true;
						offset.X = _scrollData._offset.X;
						StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.X);
						if (DoubleUtil.AreClose(size2.Width, 0.0))
						{
							size2.Width = _scrollData._viewport.Width;
						}
					}
				}
				if (!remeasure && flag2 && !DoubleUtil.AreClose(_scrollData._extent.Width, size.Width))
				{
					if (DoubleUtil.GreaterThanZero(vector.X) && DoubleUtil.GreaterThan(vector.X, size.Width - size2.Width))
					{
						remeasure = true;
						offset.X = double.PositiveInfinity;
						StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.X);
					}
					if (!remeasure && flag5)
					{
						remeasure = true;
						offset.X = _scrollData._offset.X;
						StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.X);
					}
					if (!remeasure && ((MeasureCaches && !WasLastMeasurePassAnchored) || (_scrollData._firstContainerInViewport == null && flag3 && !LayoutDoubleUtil.AreClose(vector.X, _scrollData._computedOffset.X))) && !DoubleUtil.AreClose(_scrollData._extent.Width, 0.0) && !DoubleUtil.AreClose(size.Width, 0.0))
					{
						if (IsPixelBased)
						{
							if (!LayoutDoubleUtil.AreClose(vector.X / size.Width, _scrollData._offset.X / _scrollData._extent.Width))
							{
								remeasure = true;
								offset.X = size.Width * _scrollData._offset.X / _scrollData._extent.Width;
								StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.X);
							}
						}
						else if (!LayoutDoubleUtil.AreClose(Math.Floor(vector.X) / size.Width, Math.Floor(_scrollData._offset.X) / _scrollData._extent.Width))
						{
							remeasure = true;
							offset.X = Math.Floor(size.Width * Math.Floor(_scrollData._offset.X) / _scrollData._extent.Width);
							StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.X);
						}
					}
				}
			}
			else if (!IsPixelBased && lastPageSafeOffset.HasValue && !DoubleUtil.AreClose(lastPageSafeOffset.Value, vector.X))
			{
				offset.X = lastPageSafeOffset.Value;
				lastPageSafeOffset = null;
				remeasure = true;
			}
		}
		else if (!WasOffsetPreviouslyMeasured(previouslyMeasuredOffsets, vector.Y))
		{
			bool flag6 = !DoubleUtil.AreClose(vector.Y, _scrollData._offset.Y);
			if (!IsPixelBased)
			{
				if (!IsEndOfViewport(isHorizontal, viewport, stackPixelSizeInViewport) && DoubleUtil.GreaterThan(stackLogicalSize.Height, stackLogicalSizeInViewport.Height))
				{
					lastPageSafeOffset = (lastPageSafeOffset.HasValue ? Math.Min(vector.Y, lastPageSafeOffset.Value) : vector.Y);
					double num3 = stackPixelSizeInViewport.Height / stackLogicalSizeInViewport.Height;
					double num4 = Math.Floor(viewport.Height / num3);
					if (DoubleUtil.GreaterThan(num4, size2.Height))
					{
						offset.Y = double.PositiveInfinity;
						size2.Height = num4;
						remeasure = true;
						StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.Y);
					}
				}
				if (!remeasure && flag6 && flag && !DoubleUtil.AreClose(_scrollData._viewport.Height, size2.Height))
				{
					remeasure = true;
					offset.Y = _scrollData._offset.Y;
					StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.Y);
					if (DoubleUtil.AreClose(size2.Height, 0.0))
					{
						size2.Height = _scrollData._viewport.Height;
					}
				}
			}
			if (!remeasure && flag2 && !DoubleUtil.AreClose(_scrollData._extent.Height, size.Height))
			{
				if (DoubleUtil.GreaterThanZero(vector.Y) && DoubleUtil.GreaterThan(vector.Y, size.Height - size2.Height))
				{
					remeasure = true;
					offset.Y = double.PositiveInfinity;
					StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.Y);
				}
				if (!remeasure && flag6)
				{
					remeasure = true;
					offset.Y = _scrollData._offset.Y;
					StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.Y);
				}
				if (!remeasure && ((MeasureCaches && !WasLastMeasurePassAnchored) || (_scrollData._firstContainerInViewport == null && flag3 && !LayoutDoubleUtil.AreClose(vector.Y, _scrollData._computedOffset.Y))) && !DoubleUtil.AreClose(_scrollData._extent.Height, 0.0) && !DoubleUtil.AreClose(size.Height, 0.0))
				{
					if (IsPixelBased)
					{
						if (!LayoutDoubleUtil.AreClose(vector.Y / size.Height, _scrollData._offset.Y / _scrollData._extent.Height))
						{
							remeasure = true;
							offset.Y = size.Height * _scrollData._offset.Y / _scrollData._extent.Height;
							StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.Y);
						}
					}
					else if (!LayoutDoubleUtil.AreClose(Math.Floor(vector.Y) / size.Height, Math.Floor(_scrollData._offset.Y) / _scrollData._extent.Height))
					{
						remeasure = true;
						offset.Y = Math.Floor(size.Height * Math.Floor(_scrollData._offset.Y) / _scrollData._extent.Height);
						StorePreviouslyMeasuredOffset(ref previouslyMeasuredOffsets, vector.Y);
					}
				}
			}
		}
		else if (!IsPixelBased && lastPageSafeOffset.HasValue && !DoubleUtil.AreClose(lastPageSafeOffset.Value, vector.Y))
		{
			offset.Y = lastPageSafeOffset.Value;
			lastPageSafeOffset = null;
			remeasure = true;
		}
		flag = !DoubleUtil.AreClose(size2, _scrollData._viewport);
		if (flag || flag2 || flag3)
		{
			Vector computedOffset = _scrollData._computedOffset;
			Size viewport2 = _scrollData._viewport;
			_scrollData._viewport = size2;
			_scrollData._extent = size;
			_scrollData._computedOffset = vector;
			if (flag)
			{
				OnViewportSizeChanged(viewport2, size2);
			}
			if (flag3)
			{
				OnViewportOffsetChanged(computedOffset, vector);
			}
			OnScrollChange();
		}
		_scrollData._offset = offset;
	}

	private void StorePreviouslyMeasuredOffset(ref List<double> previouslyMeasuredOffsets, double offset)
	{
		if (previouslyMeasuredOffsets == null)
		{
			previouslyMeasuredOffsets = new List<double>();
		}
		previouslyMeasuredOffsets.Add(offset);
	}

	private bool WasOffsetPreviouslyMeasured(List<double> previouslyMeasuredOffsets, double offset)
	{
		if (previouslyMeasuredOffsets != null)
		{
			for (int i = 0; i < previouslyMeasuredOffsets.Count; i++)
			{
				if (DoubleUtil.AreClose(previouslyMeasuredOffsets[i], offset))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Called when the size of the viewport changes.</summary>
	/// <param name="oldViewportSize">The old size of the viewport.</param>
	/// <param name="newViewportSize">The new size of the viewport.</param>
	protected virtual void OnViewportSizeChanged(Size oldViewportSize, Size newViewportSize)
	{
	}

	/// <summary>Called when the offset of the viewport changes as a user scrolls through content.</summary>
	/// <param name="oldViewportOffset">The old offset of the viewport.</param>
	/// <param name="newViewportOffset">The new offset of the viewport</param>
	protected virtual void OnViewportOffsetChanged(Vector oldViewportOffset, Vector newViewportOffset)
	{
	}

	/// <summary>Returns the position of the specified item, relative to the <see cref="T:System.Windows.Controls.VirtualizingStackPanel" />.</summary>
	/// <returns>The position of the specified item, relative to the <see cref="T:System.Windows.Controls.VirtualizingStackPanel" />.</returns>
	/// <param name="child">The element whose position to find.</param>
	protected override double GetItemOffsetCore(UIElement child)
	{
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		bool isHorizontal = Orientation == Orientation.Horizontal;
		GetOwners(shouldSetVirtualizationState: false, isHorizontal, out var _, out var _, out var itemStorageProvider, out var _, out var parentItem, out var parentItemStorageProvider, out var _);
		ItemContainerGenerator obj = (ItemContainerGenerator)base.Generator;
		IList itemsInternal = obj.ItemsInternal;
		int num = obj.IndexFromContainer(child, returnLocalIndex: true);
		double distance = 0.0;
		if (num >= 0)
		{
			IContainItemStorage itemStorageProvider2 = (IsVSP45Compat ? itemStorageProvider : parentItemStorageProvider);
			ComputeDistance(itemsInternal, itemStorageProvider, isHorizontal, GetAreContainersUniformlySized(itemStorageProvider2, parentItem), GetUniformOrAverageContainerSize(itemStorageProvider2, parentItem), 0, num, out distance);
		}
		return distance;
	}

	private double FindScrollOffset(Visual v)
	{
		ItemsControl.GetItemsOwner(this);
		DependencyObject dependencyObject = v;
		DependencyObject parent = VisualTreeHelper.GetParent(dependencyObject);
		IHierarchicalVirtualizationAndScrollInfo hierarchicalVirtualizationAndScrollInfo = null;
		Panel panel = null;
		IList list = null;
		int num = -1;
		double num2 = 0.0;
		bool flag = Orientation == Orientation.Horizontal;
		bool returnLocalIndex = true;
		while (true)
		{
			hierarchicalVirtualizationAndScrollInfo = GetVirtualizingChild(parent);
			if (hierarchicalVirtualizationAndScrollInfo != null)
			{
				panel = hierarchicalVirtualizationAndScrollInfo.ItemsHost;
				dependencyObject = FindDirectDescendentOfItemsHost(panel, dependencyObject);
				if (dependencyObject != null && panel is VirtualizingPanel { CanHierarchicallyScrollAndVirtualize: not false } virtualizingPanel)
				{
					double itemOffset = virtualizingPanel.GetItemOffset((UIElement)dependencyObject);
					num2 += itemOffset;
					if (IsPixelBased)
					{
						if (IsVSP45Compat)
						{
							Size pixelSize = hierarchicalVirtualizationAndScrollInfo.HeaderDesiredSizes.PixelSize;
							num2 += (flag ? pixelSize.Width : pixelSize.Height);
						}
						else
						{
							Thickness itemsHostInsetForChild = GetItemsHostInsetForChild(hierarchicalVirtualizationAndScrollInfo);
							num2 += (flag ? itemsHostInsetForChild.Left : itemsHostInsetForChild.Top);
						}
					}
					else if (IsVSP45Compat)
					{
						Size logicalSize = hierarchicalVirtualizationAndScrollInfo.HeaderDesiredSizes.LogicalSize;
						num2 += (flag ? logicalSize.Width : logicalSize.Height);
					}
					else
					{
						Thickness inset = GetItemsHostInsetForChild(hierarchicalVirtualizationAndScrollInfo);
						bool flag2 = IsHeaderBeforeItems(flag, hierarchicalVirtualizationAndScrollInfo as FrameworkElement, ref inset);
						num2 += (double)(flag2 ? 1 : 0);
					}
				}
				dependencyObject = (DependencyObject)hierarchicalVirtualizationAndScrollInfo;
			}
			else if (parent == this)
			{
				break;
			}
			parent = VisualTreeHelper.GetParent(parent);
		}
		panel = this;
		dependencyObject = FindDirectDescendentOfItemsHost(panel, dependencyObject);
		if (dependencyObject != null)
		{
			IContainItemStorage itemStorageProvider = GetItemStorageProvider(this);
			IContainItemStorage itemStorageProvider2 = (IsVSP45Compat ? itemStorageProvider : (ItemsControl.GetItemsOwnerInternal(VisualTreeHelper.GetParent((Visual)itemStorageProvider)) as IContainItemStorage));
			list = ((ItemContainerGenerator)panel.Generator).ItemsInternal;
			num = ((ItemContainerGenerator)panel.Generator).IndexFromContainer(dependencyObject, returnLocalIndex);
			ComputeDistance(list, itemStorageProvider, flag, GetAreContainersUniformlySized(itemStorageProvider2, this), GetUniformOrAverageContainerSize(itemStorageProvider2, this), 0, num, out var distance);
			num2 += distance;
		}
		return num2;
	}

	private DependencyObject FindDirectDescendentOfItemsHost(Panel itemsHost, DependencyObject child)
	{
		if (itemsHost == null || !itemsHost.IsVisible)
		{
			return null;
		}
		for (DependencyObject parent = VisualTreeHelper.GetParent(child); parent != itemsHost; parent = VisualTreeHelper.GetParent(child))
		{
			child = parent;
			if (child == null)
			{
				break;
			}
		}
		return child;
	}

	private void MakeVisiblePhysicalHelper(Rect r, ref Vector newOffset, ref Rect newRect, bool isHorizontal, ref bool alignTop, ref bool alignBottom)
	{
		double num;
		double num2;
		double num4;
		double num3;
		if (isHorizontal)
		{
			num = _scrollData._computedOffset.X;
			num2 = ViewportWidth;
			num3 = r.X;
			num4 = r.Width;
		}
		else
		{
			num = _scrollData._computedOffset.Y;
			num2 = ViewportHeight;
			num3 = r.Y;
			num4 = r.Height;
		}
		num3 += num;
		double num5 = ScrollContentPresenter.ComputeScrollOffsetWithMinimalScroll(num, num + num2, num3, num3 + num4, ref alignTop, ref alignBottom);
		double num6 = num3 - num5;
		double val = num6 + num4;
		double num7 = Math.Max(num6, 0.0);
		double num8 = Math.Max(Math.Min(val, num2), num7);
		if (isHorizontal)
		{
			newOffset.X = num5;
			newRect.X = num7;
			newRect.Width = num8 - num7;
		}
		else
		{
			newOffset.Y = num5;
			newRect.Y = num7;
			newRect.Height = num8 - num7;
		}
	}

	private void MakeVisibleLogicalHelper(int childIndex, Rect r, ref Vector newOffset, ref Rect newRect, ref bool alignTop, ref bool alignBottom)
	{
		bool flag = Orientation == Orientation.Horizontal;
		double num = r.Y;
		int num2;
		int num3;
		if (flag)
		{
			num2 = (int)_scrollData._computedOffset.X;
			num3 = (int)_scrollData._viewport.Width;
		}
		else
		{
			num2 = (int)_scrollData._computedOffset.Y;
			num3 = (int)_scrollData._viewport.Height;
		}
		int num4 = num2;
		if (childIndex < num2)
		{
			alignTop = true;
			num = 0.0;
			num4 = childIndex;
		}
		else if (childIndex > num2 + Math.Max(num3 - 1, 0))
		{
			alignBottom = true;
			num4 = childIndex - num3 + 1;
			num = (flag ? base.ActualWidth : base.ActualHeight) * (1.0 - 1.0 / (double)num3);
		}
		if (flag)
		{
			newOffset.X = num4;
			newRect.X = num;
			newRect.Width = r.Width;
		}
		else
		{
			newOffset.Y = num4;
			newRect.Y = num;
			newRect.Height = r.Height;
		}
	}

	private int GetGeneratedIndex(int childIndex)
	{
		return base.Generator.IndexFromGeneratorPosition(new GeneratorPosition(childIndex, 0));
	}

	private double GetMaxChildArrangeLength(IList children, bool isHorizontal)
	{
		double num = 0.0;
		int i = 0;
		for (int count = children.Count; i < count; i++)
		{
			Size desiredSize = ((UIElement)children[i]).DesiredSize;
			num = ((!isHorizontal) ? Math.Max(num, desiredSize.Width) : Math.Max(num, desiredSize.Height));
		}
		return num;
	}

	private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ResetScrolling(d as VirtualizingStackPanel);
	}

	void IStackMeasure.OnScrollChange()
	{
		OnScrollChange();
	}

	private bool GetBoolField(BoolField field)
	{
		return (_boolFieldStore & field) != 0;
	}

	private void SetBoolField(BoolField field, bool value)
	{
		if (value)
		{
			_boolFieldStore |= field;
		}
		else
		{
			_boolFieldStore &= (BoolField)(byte)(~(int)field);
		}
	}

	private Snapshot TakeSnapshot()
	{
		Snapshot snapshot = new Snapshot();
		if (IsScrolling)
		{
			snapshot._scrollData = new ScrollData();
			snapshot._scrollData._offset = _scrollData._offset;
			snapshot._scrollData._extent = _scrollData._extent;
			snapshot._scrollData._computedOffset = _scrollData._computedOffset;
			snapshot._scrollData._viewport = _scrollData._viewport;
		}
		snapshot._boolFieldStore = _boolFieldStore;
		snapshot._areContainersUniformlySized = AreContainersUniformlySized;
		snapshot._firstItemInExtendedViewportChildIndex = _firstItemInExtendedViewportChildIndex;
		snapshot._firstItemInExtendedViewportIndex = _firstItemInExtendedViewportIndex;
		snapshot._firstItemInExtendedViewportOffset = _firstItemInExtendedViewportOffset;
		snapshot._actualItemsInExtendedViewportCount = _actualItemsInExtendedViewportCount;
		snapshot._viewport = _viewport;
		snapshot._itemsInExtendedViewportCount = _itemsInExtendedViewportCount;
		snapshot._extendedViewport = _extendedViewport;
		snapshot._previousStackPixelSizeInViewport = _previousStackPixelSizeInViewport;
		snapshot._previousStackLogicalSizeInViewport = _previousStackLogicalSizeInViewport;
		snapshot._previousStackPixelSizeInCacheBeforeViewport = _previousStackPixelSizeInCacheBeforeViewport;
		snapshot._firstContainerInViewport = FirstContainerInViewport;
		snapshot._firstContainerOffsetFromViewport = FirstContainerOffsetFromViewport;
		snapshot._expectedDistanceBetweenViewports = ExpectedDistanceBetweenViewports;
		snapshot._bringIntoViewContainer = _bringIntoViewContainer;
		snapshot._bringIntoViewLeafContainer = BringIntoViewLeafContainer;
		SnapshotData value = SnapshotDataField.GetValue(this);
		if (value != null)
		{
			snapshot._uniformOrAverageContainerSize = value.UniformOrAverageContainerSize;
			snapshot._uniformOrAverageContainerPixelSize = value.UniformOrAverageContainerPixelSize;
			snapshot._effectiveOffsets = value.EffectiveOffsets;
			SnapshotDataField.ClearValue(this);
		}
		ItemContainerGenerator itemContainerGenerator = base.Generator as ItemContainerGenerator;
		List<ChildInfo> list = new List<ChildInfo>();
		foreach (UIElement realizedChild in RealizedChildren)
		{
			ChildInfo childInfo = new ChildInfo();
			childInfo._itemIndex = itemContainerGenerator.IndexFromContainer(realizedChild, returnLocalIndex: true);
			childInfo._desiredSize = realizedChild.DesiredSize;
			childInfo._arrangeRect = realizedChild.PreviousArrangeRect;
			childInfo._inset = (Thickness)realizedChild.GetValue(ItemsHostInsetProperty);
			list.Add(childInfo);
		}
		snapshot._realizedChildren = list;
		return snapshot;
	}

	private string ContainerPath(DependencyObject container)
	{
		if (container == null)
		{
			return string.Empty;
		}
		if (!(VisualTreeHelper.GetParent(container) is VirtualizingStackPanel virtualizingStackPanel))
		{
			return "{Disconnected}";
		}
		if (virtualizingStackPanel == this)
		{
			ItemContainerGenerator itemContainerGenerator = base.Generator as ItemContainerGenerator;
			return string.Format(CultureInfo.InvariantCulture, "{0}", itemContainerGenerator.IndexFromContainer(container, returnLocalIndex: true));
		}
		int num = (virtualizingStackPanel.Generator as ItemContainerGenerator).IndexFromContainer(container, returnLocalIndex: true);
		DependencyObject dependencyObject = ItemsControl.ContainerFromElement(null, virtualizingStackPanel);
		if (dependencyObject == null)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}", num);
		}
		return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", ContainerPath(dependencyObject), num);
	}
}
