using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.PresentationFramework;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Defines a flexible grid area that consists of columns and rows.</summary>
public class Grid : Panel, IAddChild
{
	private class ExtendedData
	{
		internal ColumnDefinitionCollection ColumnDefinitions;

		internal RowDefinitionCollection RowDefinitions;

		internal DefinitionBase[] DefinitionsU;

		internal DefinitionBase[] DefinitionsV;

		internal CellCache[] CellCachesCollection;

		internal int CellGroup1;

		internal int CellGroup2;

		internal int CellGroup3;

		internal int CellGroup4;

		internal DefinitionBase[] TempDefinitions;
	}

	[Flags]
	private enum Flags
	{
		ValidDefinitionsUStructure = 1,
		ValidDefinitionsVStructure = 2,
		ValidCellsStructure = 4,
		ShowGridLinesPropertyValue = 0x100,
		ListenToNotifications = 0x1000,
		SizeToContentU = 0x2000,
		SizeToContentV = 0x4000,
		HasStarCellsU = 0x8000,
		HasStarCellsV = 0x10000,
		HasGroup3CellsInAutoRows = 0x20000,
		MeasureOverrideInProgress = 0x40000,
		ArrangeOverrideInProgress = 0x80000
	}

	[Flags]
	internal enum LayoutTimeSizeType : byte
	{
		None = 0,
		Pixel = 1,
		Auto = 2,
		Star = 4
	}

	private struct CellCache
	{
		internal int ColumnIndex;

		internal int RowIndex;

		internal int ColumnSpan;

		internal int RowSpan;

		internal LayoutTimeSizeType SizeTypeU;

		internal LayoutTimeSizeType SizeTypeV;

		internal int Next;

		internal bool IsStarU => (SizeTypeU & LayoutTimeSizeType.Star) != 0;

		internal bool IsAutoU => (SizeTypeU & LayoutTimeSizeType.Auto) != 0;

		internal bool IsStarV => (SizeTypeV & LayoutTimeSizeType.Star) != 0;

		internal bool IsAutoV => (SizeTypeV & LayoutTimeSizeType.Auto) != 0;
	}

	private class SpanKey
	{
		private int _start;

		private int _count;

		private bool _u;

		internal int Start => _start;

		internal int Count => _count;

		internal bool U => _u;

		internal SpanKey(int start, int count, bool u)
		{
			_start = start;
			_count = count;
			_u = u;
		}

		public override int GetHashCode()
		{
			int num = _start ^ (_count << 2);
			if (_u)
			{
				return num & 0x7FFFFFF;
			}
			return num | 0x8000000;
		}

		public override bool Equals(object obj)
		{
			if (obj is SpanKey spanKey && spanKey._start == _start && spanKey._count == _count)
			{
				return spanKey._u == _u;
			}
			return false;
		}
	}

	private sealed class StarDistributionOrderIndexComparer : IComparer<int>
	{
		private readonly DefinitionBase[] definitions;

		internal StarDistributionOrderIndexComparer(DefinitionBase[] definitions)
		{
			Invariant.Assert(definitions != null);
			this.definitions = definitions;
		}

		public int Compare(int x, int y)
		{
			DefinitionBase definitionBase = definitions[x];
			DefinitionBase definitionBase2 = definitions[y];
			if (!CompareNullRefs(definitionBase, definitionBase2, out var result))
			{
				return definitionBase.SizeCache.CompareTo(definitionBase2.SizeCache);
			}
			return result;
		}
	}

	private sealed class DistributionOrderIndexComparer : IComparer<int>
	{
		private readonly DefinitionBase[] definitions;

		internal DistributionOrderIndexComparer(DefinitionBase[] definitions)
		{
			Invariant.Assert(definitions != null);
			this.definitions = definitions;
		}

		public int Compare(int x, int y)
		{
			DefinitionBase definitionBase = definitions[x];
			DefinitionBase definitionBase2 = definitions[y];
			if (!CompareNullRefs(definitionBase, definitionBase2, out var result))
			{
				double num = definitionBase.SizeCache - definitionBase.MinSizeForArrange;
				double value = definitionBase2.SizeCache - definitionBase2.MinSizeForArrange;
				return num.CompareTo(value);
			}
			return result;
		}
	}

	private sealed class RoundingErrorIndexComparer : IComparer<int>
	{
		private readonly double[] errors;

		internal RoundingErrorIndexComparer(double[] errors)
		{
			Invariant.Assert(errors != null);
			this.errors = errors;
		}

		public int Compare(int x, int y)
		{
			return errors[x].CompareTo(errors[y]);
		}
	}

	private sealed class MinRatioIndexComparer : IComparer<int>
	{
		private readonly DefinitionBase[] definitions;

		internal MinRatioIndexComparer(DefinitionBase[] definitions)
		{
			Invariant.Assert(definitions != null);
			this.definitions = definitions;
		}

		public int Compare(int x, int y)
		{
			DefinitionBase definitionBase = definitions[x];
			DefinitionBase definitionBase2 = definitions[y];
			if (!CompareNullRefs(definitionBase2, definitionBase, out var result))
			{
				return definitionBase2.MeasureSize.CompareTo(definitionBase.MeasureSize);
			}
			return result;
		}
	}

	private sealed class MaxRatioIndexComparer : IComparer<int>
	{
		private readonly DefinitionBase[] definitions;

		internal MaxRatioIndexComparer(DefinitionBase[] definitions)
		{
			Invariant.Assert(definitions != null);
			this.definitions = definitions;
		}

		public int Compare(int x, int y)
		{
			DefinitionBase definitionBase = definitions[x];
			DefinitionBase definitionBase2 = definitions[y];
			if (!CompareNullRefs(definitionBase, definitionBase2, out var result))
			{
				return definitionBase.SizeCache.CompareTo(definitionBase2.SizeCache);
			}
			return result;
		}
	}

	private sealed class StarWeightIndexComparer : IComparer<int>
	{
		private readonly DefinitionBase[] definitions;

		internal StarWeightIndexComparer(DefinitionBase[] definitions)
		{
			Invariant.Assert(definitions != null);
			this.definitions = definitions;
		}

		public int Compare(int x, int y)
		{
			DefinitionBase definitionBase = definitions[x];
			DefinitionBase definitionBase2 = definitions[y];
			if (!CompareNullRefs(definitionBase, definitionBase2, out var result))
			{
				return definitionBase.MeasureSize.CompareTo(definitionBase2.MeasureSize);
			}
			return result;
		}
	}

	private class GridChildrenCollectionEnumeratorSimple : IEnumerator
	{
		private int _currentEnumerator;

		private object _currentChild;

		private ColumnDefinitionCollection.Enumerator _enumerator0;

		private RowDefinitionCollection.Enumerator _enumerator1;

		private UIElementCollection _enumerator2Collection;

		private int _enumerator2Index;

		private int _enumerator2Count;

		public object Current
		{
			get
			{
				if (_currentEnumerator == -1)
				{
					throw new InvalidOperationException(SR.EnumeratorNotStarted);
				}
				if (_currentEnumerator >= 3)
				{
					throw new InvalidOperationException(SR.EnumeratorReachedEnd);
				}
				return _currentChild;
			}
		}

		internal GridChildrenCollectionEnumeratorSimple(Grid grid, bool includeChildren)
		{
			_currentEnumerator = -1;
			_enumerator0 = new ColumnDefinitionCollection.Enumerator((grid.ExtData != null) ? grid.ExtData.ColumnDefinitions : null);
			_enumerator1 = new RowDefinitionCollection.Enumerator((grid.ExtData != null) ? grid.ExtData.RowDefinitions : null);
			_enumerator2Index = 0;
			if (includeChildren)
			{
				_enumerator2Collection = grid.Children;
				_enumerator2Count = _enumerator2Collection.Count;
			}
			else
			{
				_enumerator2Collection = null;
				_enumerator2Count = 0;
			}
		}

		public bool MoveNext()
		{
			while (_currentEnumerator < 3)
			{
				if (_currentEnumerator >= 0)
				{
					switch (_currentEnumerator)
					{
					case 0:
						if (_enumerator0.MoveNext())
						{
							_currentChild = _enumerator0.Current;
							return true;
						}
						break;
					case 1:
						if (_enumerator1.MoveNext())
						{
							_currentChild = _enumerator1.Current;
							return true;
						}
						break;
					case 2:
						if (_enumerator2Index < _enumerator2Count)
						{
							_currentChild = _enumerator2Collection[_enumerator2Index];
							_enumerator2Index++;
							return true;
						}
						break;
					}
				}
				_currentEnumerator++;
			}
			return false;
		}

		public void Reset()
		{
			_currentEnumerator = -1;
			_currentChild = null;
			_enumerator0.Reset();
			_enumerator1.Reset();
			_enumerator2Index = 0;
		}
	}

	internal class GridLinesRenderer : DrawingVisual
	{
		private const double c_dashLength = 4.0;

		private const double c_penWidth = 1.0;

		private static readonly Pen s_oddDashPen;

		private static readonly Pen s_evenDashPen;

		private static readonly Point c_zeroPoint;

		static GridLinesRenderer()
		{
			c_zeroPoint = new Point(0.0, 0.0);
			s_oddDashPen = new Pen(Brushes.Blue, 1.0);
			DoubleCollection dashes = new DoubleCollection { 4.0, 4.0 };
			s_oddDashPen.DashStyle = new DashStyle(dashes, 0.0);
			s_oddDashPen.DashCap = PenLineCap.Flat;
			s_oddDashPen.Freeze();
			s_evenDashPen = new Pen(Brushes.Yellow, 1.0);
			DoubleCollection dashes2 = new DoubleCollection { 4.0, 4.0 };
			s_evenDashPen.DashStyle = new DashStyle(dashes2, 4.0);
			s_evenDashPen.DashCap = PenLineCap.Flat;
			s_evenDashPen.Freeze();
		}

		internal void UpdateRenderBounds(Size boundsSize)
		{
			using DrawingContext drawingContext = RenderOpen();
			if (VisualTreeHelper.GetParent(this) is Grid { ShowGridLines: not false } grid)
			{
				for (int i = 1; i < grid.DefinitionsU.Length; i++)
				{
					DrawGridLine(drawingContext, grid.DefinitionsU[i].FinalOffset, 0.0, grid.DefinitionsU[i].FinalOffset, boundsSize.Height);
				}
				for (int j = 1; j < grid.DefinitionsV.Length; j++)
				{
					DrawGridLine(drawingContext, 0.0, grid.DefinitionsV[j].FinalOffset, boundsSize.Width, grid.DefinitionsV[j].FinalOffset);
				}
			}
		}

		private static void DrawGridLine(DrawingContext drawingContext, double startX, double startY, double endX, double endY)
		{
			Point point = new Point(startX, startY);
			Point point2 = new Point(endX, endY);
			drawingContext.DrawLine(s_oddDashPen, point, point2);
			drawingContext.DrawLine(s_evenDashPen, point, point2);
		}
	}

	internal enum Counters
	{
		Default = -1,
		MeasureOverride,
		_ValidateColsStructure,
		_ValidateRowsStructure,
		_ValidateCells,
		_MeasureCell,
		__MeasureChild,
		_CalculateDesiredSize,
		ArrangeOverride,
		_SetFinalSize,
		_ArrangeChildHelper2,
		_PositionCell,
		Count
	}

	private ExtendedData _data;

	private Flags _flags;

	private GridLinesRenderer _gridLinesRenderer;

	private int[] _definitionIndices;

	private double[] _roundingErrors;

	private const double c_epsilon = 1E-05;

	private const double c_starClip = 1E+298;

	private const int c_layoutLoopMaxCount = 5;

	private static readonly LocalDataStoreSlot s_tempDefinitionsDataSlot;

	private static readonly Comparison<DefinitionBase> s_spanPreferredDistributionOrderComparer;

	private static readonly Comparison<DefinitionBase> s_spanMaxDistributionOrderComparer;

	private static readonly Comparison<DefinitionBase> s_starDistributionOrderComparer;

	private static readonly Comparison<DefinitionBase> s_minRatioComparer;

	private static readonly Comparison<DefinitionBase> s_maxRatioComparer;

	private static readonly Comparison<DefinitionBase> s_starWeightComparer;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Grid.ShowGridLines" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Grid.ShowGridLines" /> dependency property.</returns>
	public static readonly DependencyProperty ShowGridLinesProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Grid.Column" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Grid.Column" /> attached property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ColumnProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Grid.Row" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Grid.Row" /> attached property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty RowProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Grid.ColumnSpan" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Grid.ColumnSpan" /> attached property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty ColumnSpanProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Grid.RowSpan" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Grid.RowSpan" /> attached property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty RowSpanProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Grid.IsSharedSizeScope" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Grid.IsSharedSizeScope" /> attached property.</returns>
	public static readonly DependencyProperty IsSharedSizeScopeProperty;

	/// <summary>Gets an enumerator that can iterate the logical children of this <see cref="T:System.Windows.Controls.Grid" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" />. This property has no default value.</returns>
	protected internal override IEnumerator LogicalChildren
	{
		get
		{
			bool flag = base.VisualChildrenCount == 0 || base.IsItemsHost;
			if (flag)
			{
				ExtendedData extData = ExtData;
				if (extData == null || ((extData.ColumnDefinitions == null || extData.ColumnDefinitions.Count == 0) && (extData.RowDefinitions == null || extData.RowDefinitions.Count == 0)))
				{
					return EmptyEnumerator.Instance;
				}
			}
			return new GridChildrenCollectionEnumeratorSimple(this, !flag);
		}
	}

	/// <summary>Gets or sets a value that indicates whether grid lines are visible within this <see cref="T:System.Windows.Controls.Grid" />. </summary>
	/// <returns>true if grid lines are visible; otherwise, false. The default value is false.</returns>
	public bool ShowGridLines
	{
		get
		{
			return CheckFlagsAnd(Flags.ShowGridLinesPropertyValue);
		}
		set
		{
			SetValue(ShowGridLinesProperty, value);
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Controls.ColumnDefinitionCollection" /> defined on this instance of <see cref="T:System.Windows.Controls.Grid" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.ColumnDefinitionCollection" /> defined on this instance of <see cref="T:System.Windows.Controls.Grid" /></returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public ColumnDefinitionCollection ColumnDefinitions
	{
		get
		{
			if (_data == null)
			{
				_data = new ExtendedData();
			}
			if (_data.ColumnDefinitions == null)
			{
				_data.ColumnDefinitions = new ColumnDefinitionCollection(this);
			}
			return _data.ColumnDefinitions;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Controls.RowDefinitionCollection" /> defined on this instance of <see cref="T:System.Windows.Controls.Grid" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.RowDefinitionCollection" /> defined on this instance of <see cref="T:System.Windows.Controls.Grid" />.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public RowDefinitionCollection RowDefinitions
	{
		get
		{
			if (_data == null)
			{
				_data = new ExtendedData();
			}
			if (_data.RowDefinitions == null)
			{
				_data.RowDefinitions = new RowDefinitionCollection(this);
			}
			return _data.RowDefinitions;
		}
	}

	/// <summary>Gets a value that represents the total number of <see cref="T:System.Windows.Media.Visual" /> children within this instance of <see cref="T:System.Windows.Controls.Grid" />.</summary>
	/// <returns>
	///   <see cref="T:System.Int32" /> that represents the total number of child <see cref="T:System.Windows.Media.Visual" /> objects. The default value is zero.</returns>
	protected override int VisualChildrenCount => base.VisualChildrenCount + ((_gridLinesRenderer != null) ? 1 : 0);

	internal bool MeasureOverrideInProgress
	{
		get
		{
			return CheckFlagsAnd(Flags.MeasureOverrideInProgress);
		}
		set
		{
			SetFlags(value, Flags.MeasureOverrideInProgress);
		}
	}

	internal bool ArrangeOverrideInProgress
	{
		get
		{
			return CheckFlagsAnd(Flags.ArrangeOverrideInProgress);
		}
		set
		{
			SetFlags(value, Flags.ArrangeOverrideInProgress);
		}
	}

	internal bool ColumnDefinitionCollectionDirty
	{
		get
		{
			return !CheckFlagsAnd(Flags.ValidDefinitionsUStructure);
		}
		set
		{
			SetFlags(!value, Flags.ValidDefinitionsUStructure);
		}
	}

	internal bool RowDefinitionCollectionDirty
	{
		get
		{
			return !CheckFlagsAnd(Flags.ValidDefinitionsVStructure);
		}
		set
		{
			SetFlags(!value, Flags.ValidDefinitionsVStructure);
		}
	}

	private DefinitionBase[] DefinitionsU => ExtData.DefinitionsU;

	private DefinitionBase[] DefinitionsV => ExtData.DefinitionsV;

	private DefinitionBase[] TempDefinitions
	{
		get
		{
			ExtendedData extData = ExtData;
			int num = Math.Max(DefinitionsU.Length, DefinitionsV.Length) * 2;
			if (extData.TempDefinitions == null || extData.TempDefinitions.Length < num)
			{
				WeakReference weakReference = (WeakReference)Thread.GetData(s_tempDefinitionsDataSlot);
				if (weakReference == null)
				{
					extData.TempDefinitions = new DefinitionBase[num];
					Thread.SetData(s_tempDefinitionsDataSlot, new WeakReference(extData.TempDefinitions));
				}
				else
				{
					extData.TempDefinitions = (DefinitionBase[])weakReference.Target;
					if (extData.TempDefinitions == null || extData.TempDefinitions.Length < num)
					{
						extData.TempDefinitions = new DefinitionBase[num];
						weakReference.Target = extData.TempDefinitions;
					}
				}
			}
			return extData.TempDefinitions;
		}
	}

	private int[] DefinitionIndices
	{
		get
		{
			int num = Math.Max(Math.Max(DefinitionsU.Length, DefinitionsV.Length), 1) * 2;
			if (_definitionIndices == null || _definitionIndices.Length < num)
			{
				_definitionIndices = new int[num];
			}
			return _definitionIndices;
		}
	}

	private double[] RoundingErrors
	{
		get
		{
			int num = Math.Max(DefinitionsU.Length, DefinitionsV.Length);
			if (_roundingErrors == null && num == 0)
			{
				_roundingErrors = new double[1];
			}
			else if (_roundingErrors == null || _roundingErrors.Length < num)
			{
				_roundingErrors = new double[num];
			}
			return _roundingErrors;
		}
	}

	private CellCache[] PrivateCells => ExtData.CellCachesCollection;

	private bool CellsStructureDirty
	{
		get
		{
			return !CheckFlagsAnd(Flags.ValidCellsStructure);
		}
		set
		{
			SetFlags(!value, Flags.ValidCellsStructure);
		}
	}

	private bool ListenToNotifications
	{
		get
		{
			return CheckFlagsAnd(Flags.ListenToNotifications);
		}
		set
		{
			SetFlags(value, Flags.ListenToNotifications);
		}
	}

	private bool SizeToContentU
	{
		get
		{
			return CheckFlagsAnd(Flags.SizeToContentU);
		}
		set
		{
			SetFlags(value, Flags.SizeToContentU);
		}
	}

	private bool SizeToContentV
	{
		get
		{
			return CheckFlagsAnd(Flags.SizeToContentV);
		}
		set
		{
			SetFlags(value, Flags.SizeToContentV);
		}
	}

	private bool HasStarCellsU
	{
		get
		{
			return CheckFlagsAnd(Flags.HasStarCellsU);
		}
		set
		{
			SetFlags(value, Flags.HasStarCellsU);
		}
	}

	private bool HasStarCellsV
	{
		get
		{
			return CheckFlagsAnd(Flags.HasStarCellsV);
		}
		set
		{
			SetFlags(value, Flags.HasStarCellsV);
		}
	}

	private bool HasGroup3CellsInAutoRows
	{
		get
		{
			return CheckFlagsAnd(Flags.HasGroup3CellsInAutoRows);
		}
		set
		{
			SetFlags(value, Flags.HasGroup3CellsInAutoRows);
		}
	}

	private ExtendedData ExtData => _data;

	internal override int EffectiveValuesInitialSize => 9;

	static Grid()
	{
		s_tempDefinitionsDataSlot = Thread.AllocateDataSlot();
		s_spanPreferredDistributionOrderComparer = SpanPreferredDistributionOrderComparer;
		s_spanMaxDistributionOrderComparer = SpanMaxDistributionOrderComparer;
		s_starDistributionOrderComparer = StarDistributionOrderComparer;
		s_minRatioComparer = MinRatioComparer;
		s_maxRatioComparer = MaxRatioComparer;
		s_starWeightComparer = StarWeightComparer;
		ShowGridLinesProperty = DependencyProperty.Register("ShowGridLines", typeof(bool), typeof(Grid), new FrameworkPropertyMetadata(false, OnShowGridLinesPropertyChanged));
		ColumnProperty = DependencyProperty.RegisterAttached("Column", typeof(int), typeof(Grid), new FrameworkPropertyMetadata(0, OnCellAttachedPropertyChanged), IsIntValueNotNegative);
		RowProperty = DependencyProperty.RegisterAttached("Row", typeof(int), typeof(Grid), new FrameworkPropertyMetadata(0, OnCellAttachedPropertyChanged), IsIntValueNotNegative);
		ColumnSpanProperty = DependencyProperty.RegisterAttached("ColumnSpan", typeof(int), typeof(Grid), new FrameworkPropertyMetadata(1, OnCellAttachedPropertyChanged), IsIntValueGreaterThanZero);
		RowSpanProperty = DependencyProperty.RegisterAttached("RowSpan", typeof(int), typeof(Grid), new FrameworkPropertyMetadata(1, OnCellAttachedPropertyChanged), IsIntValueGreaterThanZero);
		IsSharedSizeScopeProperty = DependencyProperty.RegisterAttached("IsSharedSizeScope", typeof(bool), typeof(Grid), new FrameworkPropertyMetadata(false, DefinitionBase.OnIsSharedSizeScopePropertyChanged));
		ControlsTraceLogger.AddControl(TelemetryControls.Grid);
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Controls.Grid" />.</summary>
	public Grid()
	{
		SetFlags((bool)ShowGridLinesProperty.GetDefaultValue(base.DependencyObjectType), Flags.ShowGridLinesPropertyValue);
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value">An object to add as a child.</param>
	void IAddChild.AddChild(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value is UIElement element)
		{
			base.Children.Add(element);
			return;
		}
		throw new ArgumentException(SR.Format(SR.Grid_UnexpectedParameterType, value.GetType(), typeof(UIElement)), "value");
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="text">A string to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.Grid.Column" /> attached property to a given <see cref="T:System.Windows.UIElement" />. </summary>
	/// <param name="element">The element on which to set the <see cref="P:System.Windows.Controls.Grid.Column" /> attached property.</param>
	/// <param name="value">The property value to set.</param>
	public static void SetColumn(UIElement element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ColumnProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Grid.Column" /> attached property from a given <see cref="T:System.Windows.UIElement" />. </summary>
	/// <returns>The value of the <see cref="P:System.Windows.Controls.Grid.Column" /> attached property.</returns>
	/// <param name="element">The element from which to read the property value.</param>
	[AttachedPropertyBrowsableForChildren]
	public static int GetColumn(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(ColumnProperty);
	}

	/// <summary> Sets the value of the <see cref="P:System.Windows.Controls.Grid.Row" /> attached property to a given <see cref="T:System.Windows.UIElement" />. </summary>
	/// <param name="element">The element on which to set the attached property.</param>
	/// <param name="value">The property value to set.</param>
	public static void SetRow(UIElement element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(RowProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Grid.Row" /> attached property from a given <see cref="T:System.Windows.UIElement" />. </summary>
	/// <returns>The value of the <see cref="P:System.Windows.Controls.Grid.Row" /> attached property.</returns>
	/// <param name="element">The element from which to read the property value.</param>
	[AttachedPropertyBrowsableForChildren]
	public static int GetRow(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(RowProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.Grid.ColumnSpan" /> attached property to a given <see cref="T:System.Windows.UIElement" />. </summary>
	/// <param name="element">The element on which to set the <see cref="P:System.Windows.Controls.Grid.ColumnSpan" /> attached property.</param>
	/// <param name="value">The property value to set.</param>
	public static void SetColumnSpan(UIElement element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ColumnSpanProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Grid.ColumnSpan" /> attached property from a given <see cref="T:System.Windows.UIElement" />.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Controls.Grid.ColumnSpan" /> attached property.</returns>
	/// <param name="element">The element from which to read the property value.</param>
	[AttachedPropertyBrowsableForChildren]
	public static int GetColumnSpan(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(ColumnSpanProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.Grid.RowSpan" /> attached property to a given <see cref="T:System.Windows.UIElement" />. </summary>
	/// <param name="element">The element on which to set the <see cref="P:System.Windows.Controls.Grid.RowSpan" /> attached property.</param>
	/// <param name="value">The property value to set.</param>
	public static void SetRowSpan(UIElement element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(RowSpanProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Grid.RowSpan" /> attached property from a given <see cref="T:System.Windows.UIElement" />. </summary>
	/// <returns>The value of the <see cref="P:System.Windows.Controls.Grid.RowSpan" /> attached property.</returns>
	/// <param name="element">The element from which to read the  property value.</param>
	[AttachedPropertyBrowsableForChildren]
	public static int GetRowSpan(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(RowSpanProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.Grid.IsSharedSizeScope" /> attached property to a given <see cref="T:System.Windows.UIElement" />. </summary>
	/// <param name="element">The element on which to set the <see cref="P:System.Windows.Controls.Grid.IsSharedSizeScope" /> attached property.</param>
	/// <param name="value">The property value to set.</param>
	public static void SetIsSharedSizeScope(UIElement element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsSharedSizeScopeProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.Grid.IsSharedSizeScope" /> attached property from a given <see cref="T:System.Windows.UIElement" />. </summary>
	/// <returns>The value of the <see cref="P:System.Windows.Controls.Grid.IsSharedSizeScope" /> attached property.</returns>
	/// <param name="element">The element from which to read the property value.</param>
	public static bool GetIsSharedSizeScope(UIElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsSharedSizeScopeProperty);
	}

	/// <summary>Gets the child <see cref="T:System.Windows.Media.Visual" /> at the specified <paramref name="index" /> position.</summary>
	/// <returns>The child <see cref="T:System.Windows.Media.Visual" /> at the specified <paramref name="index" /> position.</returns>
	/// <param name="index">The zero-based index position of the desired <see cref="T:System.Windows.Media.Visual" />.</param>
	protected override Visual GetVisualChild(int index)
	{
		if (index == base.VisualChildrenCount)
		{
			if (_gridLinesRenderer == null)
			{
				throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
			}
			return _gridLinesRenderer;
		}
		return base.GetVisualChild(index);
	}

	/// <summary>Measures the children of a <see cref="T:System.Windows.Controls.Grid" /> in anticipation of arranging them during the <see cref="M:System.Windows.Controls.Grid.ArrangeOverride(System.Windows.Size)" /> pass.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Size" /> that represents the required size to arrange child content.</returns>
	/// <param name="constraint">Indicates an upper limit size that should not be exceeded.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		ExtendedData extData = ExtData;
		Size result;
		try
		{
			ListenToNotifications = true;
			MeasureOverrideInProgress = true;
			if (extData != null)
			{
				bool flag = double.IsPositiveInfinity(constraint.Width);
				bool flag2 = double.IsPositiveInfinity(constraint.Height);
				if (RowDefinitionCollectionDirty || ColumnDefinitionCollectionDirty)
				{
					if (_definitionIndices != null)
					{
						Array.Clear(_definitionIndices, 0, _definitionIndices.Length);
						_definitionIndices = null;
					}
					if (base.UseLayoutRounding && _roundingErrors != null)
					{
						Array.Clear(_roundingErrors, 0, _roundingErrors.Length);
						_roundingErrors = null;
					}
				}
				ValidateDefinitionsUStructure();
				ValidateDefinitionsLayout(DefinitionsU, flag);
				ValidateDefinitionsVStructure();
				ValidateDefinitionsLayout(DefinitionsV, flag2);
				CellsStructureDirty |= SizeToContentU != flag || SizeToContentV != flag2;
				SizeToContentU = flag;
				SizeToContentV = flag2;
				ValidateCells();
				MeasureCellsGroup(extData.CellGroup1, constraint, ignoreDesiredSizeU: false, forceInfinityV: false);
				if (!HasGroup3CellsInAutoRows)
				{
					if (HasStarCellsV)
					{
						ResolveStar(DefinitionsV, constraint.Height);
					}
					MeasureCellsGroup(extData.CellGroup2, constraint, ignoreDesiredSizeU: false, forceInfinityV: false);
					if (HasStarCellsU)
					{
						ResolveStar(DefinitionsU, constraint.Width);
					}
					MeasureCellsGroup(extData.CellGroup3, constraint, ignoreDesiredSizeU: false, forceInfinityV: false);
				}
				else if (extData.CellGroup2 > PrivateCells.Length)
				{
					if (HasStarCellsU)
					{
						ResolveStar(DefinitionsU, constraint.Width);
					}
					MeasureCellsGroup(extData.CellGroup3, constraint, ignoreDesiredSizeU: false, forceInfinityV: false);
					if (HasStarCellsV)
					{
						ResolveStar(DefinitionsV, constraint.Height);
					}
				}
				else
				{
					bool hasDesiredSizeUChanged = false;
					int num = 0;
					double[] minSizes = CacheMinSizes(extData.CellGroup2, isRows: false);
					double[] minSizes2 = CacheMinSizes(extData.CellGroup3, isRows: true);
					MeasureCellsGroup(extData.CellGroup2, constraint, ignoreDesiredSizeU: false, forceInfinityV: true);
					do
					{
						if (hasDesiredSizeUChanged)
						{
							ApplyCachedMinSizes(minSizes2, isRows: true);
						}
						if (HasStarCellsU)
						{
							ResolveStar(DefinitionsU, constraint.Width);
						}
						MeasureCellsGroup(extData.CellGroup3, constraint, ignoreDesiredSizeU: false, forceInfinityV: false);
						ApplyCachedMinSizes(minSizes, isRows: false);
						if (HasStarCellsV)
						{
							ResolveStar(DefinitionsV, constraint.Height);
						}
						MeasureCellsGroup(extData.CellGroup2, constraint, num == 5, forceInfinityV: false, out hasDesiredSizeUChanged);
					}
					while (hasDesiredSizeUChanged && ++num <= 5);
				}
				MeasureCellsGroup(extData.CellGroup4, constraint, ignoreDesiredSizeU: false, forceInfinityV: false);
				result = new Size(CalculateDesiredSize(DefinitionsU), CalculateDesiredSize(DefinitionsV));
				return result;
			}
			result = default(Size);
			UIElementCollection internalChildren = base.InternalChildren;
			int i = 0;
			for (int count = internalChildren.Count; i < count; i++)
			{
				UIElement uIElement = internalChildren[i];
				if (uIElement != null)
				{
					uIElement.Measure(constraint);
					result.Width = Math.Max(result.Width, uIElement.DesiredSize.Width);
					result.Height = Math.Max(result.Height, uIElement.DesiredSize.Height);
				}
			}
		}
		finally
		{
			MeasureOverrideInProgress = false;
		}
		return result;
	}

	/// <summary>Arranges the content of a <see cref="T:System.Windows.Controls.Grid" /> element.</summary>
	/// <returns>
	///   <see cref="T:System.Windows.Size" /> that represents the arranged size of this Grid element and its children.</returns>
	/// <param name="arrangeSize">Specifies the size this <see cref="T:System.Windows.Controls.Grid" /> element should use to arrange its child elements.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		try
		{
			ArrangeOverrideInProgress = true;
			if (_data == null)
			{
				UIElementCollection internalChildren = base.InternalChildren;
				int i = 0;
				for (int count = internalChildren.Count; i < count; i++)
				{
					internalChildren[i]?.Arrange(new Rect(arrangeSize));
				}
			}
			else
			{
				SetFinalSize(DefinitionsU, arrangeSize.Width, columns: true);
				SetFinalSize(DefinitionsV, arrangeSize.Height, columns: false);
				UIElementCollection internalChildren2 = base.InternalChildren;
				for (int j = 0; j < PrivateCells.Length; j++)
				{
					UIElement uIElement = internalChildren2[j];
					if (uIElement != null)
					{
						int columnIndex = PrivateCells[j].ColumnIndex;
						int rowIndex = PrivateCells[j].RowIndex;
						int columnSpan = PrivateCells[j].ColumnSpan;
						int rowSpan = PrivateCells[j].RowSpan;
						Rect finalRect = new Rect((columnIndex == 0) ? 0.0 : DefinitionsU[columnIndex].FinalOffset, (rowIndex == 0) ? 0.0 : DefinitionsV[rowIndex].FinalOffset, GetFinalSizeForRange(DefinitionsU, columnIndex, columnSpan), GetFinalSizeForRange(DefinitionsV, rowIndex, rowSpan));
						uIElement.Arrange(finalRect);
					}
				}
				EnsureGridLinesRenderer()?.UpdateRenderBounds(arrangeSize);
			}
		}
		finally
		{
			SetValid();
			ArrangeOverrideInProgress = false;
		}
		return arrangeSize;
	}

	/// <summary>Called when the visual children of a <see cref="T:System.Windows.Controls.Grid" /> element change.</summary>
	/// <param name="visualAdded">Identifies the visual child that's added.</param>
	/// <param name="visualRemoved">Identifies the visual child that's removed.</param>
	protected internal override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
	{
		CellsStructureDirty = true;
		base.OnVisualChildrenChanged(visualAdded, visualRemoved);
	}

	internal void Invalidate()
	{
		CellsStructureDirty = true;
		InvalidateMeasure();
	}

	internal double GetFinalColumnDefinitionWidth(int columnIndex)
	{
		double num = 0.0;
		Invariant.Assert(_data != null);
		if (!ColumnDefinitionCollectionDirty)
		{
			DefinitionBase[] definitionsU = DefinitionsU;
			num = definitionsU[(columnIndex + 1) % definitionsU.Length].FinalOffset;
			if (columnIndex != 0)
			{
				num -= definitionsU[columnIndex].FinalOffset;
			}
		}
		return num;
	}

	internal double GetFinalRowDefinitionHeight(int rowIndex)
	{
		double num = 0.0;
		Invariant.Assert(_data != null);
		if (!RowDefinitionCollectionDirty)
		{
			DefinitionBase[] definitionsV = DefinitionsV;
			num = definitionsV[(rowIndex + 1) % definitionsV.Length].FinalOffset;
			if (rowIndex != 0)
			{
				num -= definitionsV[rowIndex].FinalOffset;
			}
		}
		return num;
	}

	private void ValidateCells()
	{
		if (CellsStructureDirty)
		{
			ValidateCellsCore();
			CellsStructureDirty = false;
		}
	}

	private void ValidateCellsCore()
	{
		UIElementCollection internalChildren = base.InternalChildren;
		ExtendedData extData = ExtData;
		extData.CellCachesCollection = new CellCache[internalChildren.Count];
		extData.CellGroup1 = int.MaxValue;
		extData.CellGroup2 = int.MaxValue;
		extData.CellGroup3 = int.MaxValue;
		extData.CellGroup4 = int.MaxValue;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int num = PrivateCells.Length - 1; num >= 0; num--)
		{
			UIElement uIElement = internalChildren[num];
			if (uIElement != null)
			{
				CellCache cellCache = default(CellCache);
				cellCache.ColumnIndex = Math.Min(GetColumn(uIElement), DefinitionsU.Length - 1);
				cellCache.RowIndex = Math.Min(GetRow(uIElement), DefinitionsV.Length - 1);
				cellCache.ColumnSpan = Math.Min(GetColumnSpan(uIElement), DefinitionsU.Length - cellCache.ColumnIndex);
				cellCache.RowSpan = Math.Min(GetRowSpan(uIElement), DefinitionsV.Length - cellCache.RowIndex);
				cellCache.SizeTypeU = GetLengthTypeForRange(DefinitionsU, cellCache.ColumnIndex, cellCache.ColumnSpan);
				cellCache.SizeTypeV = GetLengthTypeForRange(DefinitionsV, cellCache.RowIndex, cellCache.RowSpan);
				flag |= cellCache.IsStarU;
				flag2 |= cellCache.IsStarV;
				if (!cellCache.IsStarV)
				{
					if (!cellCache.IsStarU)
					{
						cellCache.Next = extData.CellGroup1;
						extData.CellGroup1 = num;
					}
					else
					{
						cellCache.Next = extData.CellGroup3;
						extData.CellGroup3 = num;
						flag3 |= cellCache.IsAutoV;
					}
				}
				else if (cellCache.IsAutoU && !cellCache.IsStarU)
				{
					cellCache.Next = extData.CellGroup2;
					extData.CellGroup2 = num;
				}
				else
				{
					cellCache.Next = extData.CellGroup4;
					extData.CellGroup4 = num;
				}
				PrivateCells[num] = cellCache;
			}
		}
		HasStarCellsU = flag;
		HasStarCellsV = flag2;
		HasGroup3CellsInAutoRows = flag3;
	}

	private void ValidateDefinitionsUStructure()
	{
		if (!ColumnDefinitionCollectionDirty)
		{
			return;
		}
		ExtendedData extData = ExtData;
		if (extData.ColumnDefinitions == null)
		{
			if (extData.DefinitionsU == null)
			{
				extData.DefinitionsU = new DefinitionBase[1]
				{
					new ColumnDefinition()
				};
			}
		}
		else
		{
			extData.ColumnDefinitions.InternalTrimToSize();
			if (extData.ColumnDefinitions.InternalCount == 0)
			{
				extData.DefinitionsU = new DefinitionBase[1]
				{
					new ColumnDefinition()
				};
			}
			else
			{
				extData.DefinitionsU = extData.ColumnDefinitions.InternalItems;
			}
		}
		ColumnDefinitionCollectionDirty = false;
	}

	private void ValidateDefinitionsVStructure()
	{
		if (!RowDefinitionCollectionDirty)
		{
			return;
		}
		ExtendedData extData = ExtData;
		if (extData.RowDefinitions == null)
		{
			if (extData.DefinitionsV == null)
			{
				extData.DefinitionsV = new DefinitionBase[1]
				{
					new RowDefinition()
				};
			}
		}
		else
		{
			extData.RowDefinitions.InternalTrimToSize();
			if (extData.RowDefinitions.InternalCount == 0)
			{
				extData.DefinitionsV = new DefinitionBase[1]
				{
					new RowDefinition()
				};
			}
			else
			{
				extData.DefinitionsV = extData.RowDefinitions.InternalItems;
			}
		}
		RowDefinitionCollectionDirty = false;
	}

	private void ValidateDefinitionsLayout(DefinitionBase[] definitions, bool treatStarAsAuto)
	{
		for (int i = 0; i < definitions.Length; i++)
		{
			definitions[i].OnBeforeLayout(this);
			double num = definitions[i].UserMinSize;
			double userMaxSize = definitions[i].UserMaxSize;
			double val = 0.0;
			switch (definitions[i].UserSize.GridUnitType)
			{
			case GridUnitType.Pixel:
				definitions[i].SizeType = LayoutTimeSizeType.Pixel;
				val = definitions[i].UserSize.Value;
				num = Math.Max(num, Math.Min(val, userMaxSize));
				break;
			case GridUnitType.Auto:
				definitions[i].SizeType = LayoutTimeSizeType.Auto;
				val = double.PositiveInfinity;
				break;
			case GridUnitType.Star:
				if (treatStarAsAuto)
				{
					definitions[i].SizeType = LayoutTimeSizeType.Auto;
					val = double.PositiveInfinity;
				}
				else
				{
					definitions[i].SizeType = LayoutTimeSizeType.Star;
					val = double.PositiveInfinity;
				}
				break;
			}
			definitions[i].UpdateMinSize(num);
			definitions[i].MeasureSize = Math.Max(num, Math.Min(val, userMaxSize));
		}
	}

	private double[] CacheMinSizes(int cellsHead, bool isRows)
	{
		double[] array = (isRows ? new double[DefinitionsV.Length] : new double[DefinitionsU.Length]);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = -1.0;
		}
		int num = cellsHead;
		do
		{
			if (isRows)
			{
				array[PrivateCells[num].RowIndex] = DefinitionsV[PrivateCells[num].RowIndex].RawMinSize;
			}
			else
			{
				array[PrivateCells[num].ColumnIndex] = DefinitionsU[PrivateCells[num].ColumnIndex].RawMinSize;
			}
			num = PrivateCells[num].Next;
		}
		while (num < PrivateCells.Length);
		return array;
	}

	private void ApplyCachedMinSizes(double[] minSizes, bool isRows)
	{
		for (int i = 0; i < minSizes.Length; i++)
		{
			if (DoubleUtil.GreaterThanOrClose(minSizes[i], 0.0))
			{
				if (isRows)
				{
					DefinitionsV[i].SetMinSize(minSizes[i]);
				}
				else
				{
					DefinitionsU[i].SetMinSize(minSizes[i]);
				}
			}
		}
	}

	private void MeasureCellsGroup(int cellsHead, Size referenceSize, bool ignoreDesiredSizeU, bool forceInfinityV)
	{
		MeasureCellsGroup(cellsHead, referenceSize, ignoreDesiredSizeU, forceInfinityV, out var _);
	}

	private void MeasureCellsGroup(int cellsHead, Size referenceSize, bool ignoreDesiredSizeU, bool forceInfinityV, out bool hasDesiredSizeUChanged)
	{
		hasDesiredSizeUChanged = false;
		if (cellsHead >= PrivateCells.Length)
		{
			return;
		}
		UIElementCollection internalChildren = base.InternalChildren;
		Hashtable store = null;
		bool flag = forceInfinityV;
		int num = cellsHead;
		do
		{
			double width = internalChildren[num].DesiredSize.Width;
			MeasureCell(num, forceInfinityV);
			hasDesiredSizeUChanged |= !DoubleUtil.AreClose(width, internalChildren[num].DesiredSize.Width);
			if (!ignoreDesiredSizeU)
			{
				if (PrivateCells[num].ColumnSpan == 1)
				{
					DefinitionsU[PrivateCells[num].ColumnIndex].UpdateMinSize(Math.Min(internalChildren[num].DesiredSize.Width, DefinitionsU[PrivateCells[num].ColumnIndex].UserMaxSize));
				}
				else
				{
					RegisterSpan(ref store, PrivateCells[num].ColumnIndex, PrivateCells[num].ColumnSpan, u: true, internalChildren[num].DesiredSize.Width);
				}
			}
			if (!flag)
			{
				if (PrivateCells[num].RowSpan == 1)
				{
					DefinitionsV[PrivateCells[num].RowIndex].UpdateMinSize(Math.Min(internalChildren[num].DesiredSize.Height, DefinitionsV[PrivateCells[num].RowIndex].UserMaxSize));
				}
				else
				{
					RegisterSpan(ref store, PrivateCells[num].RowIndex, PrivateCells[num].RowSpan, u: false, internalChildren[num].DesiredSize.Height);
				}
			}
			num = PrivateCells[num].Next;
		}
		while (num < PrivateCells.Length);
		if (store == null)
		{
			return;
		}
		foreach (DictionaryEntry item in store)
		{
			SpanKey spanKey = (SpanKey)item.Key;
			double requestedSize = (double)item.Value;
			EnsureMinSizeInDefinitionRange(spanKey.U ? DefinitionsU : DefinitionsV, spanKey.Start, spanKey.Count, requestedSize, spanKey.U ? referenceSize.Width : referenceSize.Height);
		}
	}

	private static void RegisterSpan(ref Hashtable store, int start, int count, bool u, double value)
	{
		if (store == null)
		{
			store = new Hashtable();
		}
		SpanKey key = new SpanKey(start, count, u);
		object obj = store[key];
		if (obj == null || value > (double)obj)
		{
			store[key] = value;
		}
	}

	private void MeasureCell(int cell, bool forceInfinityV)
	{
		double width = ((!PrivateCells[cell].IsAutoU || PrivateCells[cell].IsStarU) ? GetMeasureSizeForRange(DefinitionsU, PrivateCells[cell].ColumnIndex, PrivateCells[cell].ColumnSpan) : double.PositiveInfinity);
		double height = (forceInfinityV ? double.PositiveInfinity : ((!PrivateCells[cell].IsAutoV || PrivateCells[cell].IsStarV) ? GetMeasureSizeForRange(DefinitionsV, PrivateCells[cell].RowIndex, PrivateCells[cell].RowSpan) : double.PositiveInfinity));
		UIElement uIElement = base.InternalChildren[cell];
		if (uIElement != null)
		{
			Size availableSize = new Size(width, height);
			uIElement.Measure(availableSize);
		}
	}

	private double GetMeasureSizeForRange(DefinitionBase[] definitions, int start, int count)
	{
		double num = 0.0;
		int num2 = start + count - 1;
		do
		{
			num += ((definitions[num2].SizeType == LayoutTimeSizeType.Auto) ? definitions[num2].MinSize : definitions[num2].MeasureSize);
		}
		while (--num2 >= start);
		return num;
	}

	private LayoutTimeSizeType GetLengthTypeForRange(DefinitionBase[] definitions, int start, int count)
	{
		LayoutTimeSizeType layoutTimeSizeType = LayoutTimeSizeType.None;
		int num = start + count - 1;
		do
		{
			layoutTimeSizeType |= definitions[num].SizeType;
		}
		while (--num >= start);
		return layoutTimeSizeType;
	}

	private void EnsureMinSizeInDefinitionRange(DefinitionBase[] definitions, int start, int count, double requestedSize, double percentReferenceSize)
	{
		if (_IsZero(requestedSize))
		{
			return;
		}
		DefinitionBase[] tempDefinitions = TempDefinitions;
		int num = start + count;
		int num2 = 0;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		for (int i = start; i < num; i++)
		{
			double minSize = definitions[i].MinSize;
			double preferredSize = definitions[i].PreferredSize;
			double num7 = Math.Max(definitions[i].UserMaxSize, minSize);
			num3 += minSize;
			num4 += preferredSize;
			num5 += num7;
			definitions[i].SizeCache = num7;
			if (num6 < num7)
			{
				num6 = num7;
			}
			if (definitions[i].UserSize.IsAuto)
			{
				num2++;
			}
			tempDefinitions[i - start] = definitions[i];
		}
		if (!(requestedSize > num3))
		{
			return;
		}
		if (requestedSize <= num4)
		{
			tempDefinitions.AsSpan(0, count).Sort(s_spanPreferredDistributionOrderComparer);
			int j = 0;
			double num8 = requestedSize;
			for (; j < num2; j++)
			{
				num8 -= tempDefinitions[j].MinSize;
			}
			for (; j < count; j++)
			{
				double num9 = Math.Min(num8 / (double)(count - j), tempDefinitions[j].PreferredSize);
				if (num9 > tempDefinitions[j].MinSize)
				{
					tempDefinitions[j].UpdateMinSize(num9);
				}
				num8 -= num9;
			}
			return;
		}
		if (requestedSize <= num5)
		{
			tempDefinitions.AsSpan(0, count).Sort(s_spanMaxDistributionOrderComparer);
			int k = 0;
			double num10 = requestedSize - num4;
			for (; k < count - num2; k++)
			{
				double preferredSize2 = tempDefinitions[k].PreferredSize;
				double val = preferredSize2 + num10 / (double)(count - num2 - k);
				tempDefinitions[k].UpdateMinSize(Math.Min(val, tempDefinitions[k].SizeCache));
				num10 -= tempDefinitions[k].MinSize - preferredSize2;
			}
			for (; k < count; k++)
			{
				double minSize2 = tempDefinitions[k].MinSize;
				double val2 = minSize2 + num10 / (double)(count - k);
				tempDefinitions[k].UpdateMinSize(Math.Min(val2, tempDefinitions[k].SizeCache));
				num10 -= tempDefinitions[k].MinSize - minSize2;
			}
			return;
		}
		double num11 = requestedSize / (double)count;
		if (num11 < num6 && !_AreClose(num11, num6))
		{
			double num12 = num6 * (double)count - num5;
			double num13 = requestedSize - num5;
			for (int l = 0; l < count; l++)
			{
				double num14 = (num6 - tempDefinitions[l].SizeCache) * num13 / num12;
				tempDefinitions[l].UpdateMinSize(tempDefinitions[l].SizeCache + num14);
			}
		}
		else
		{
			for (int m = 0; m < count; m++)
			{
				tempDefinitions[m].UpdateMinSize(num11);
			}
		}
	}

	private void ResolveStar(DefinitionBase[] definitions, double availableSize)
	{
		if (FrameworkAppContextSwitches.GridStarDefinitionsCanExceedAvailableSpace)
		{
			ResolveStarLegacy(definitions, availableSize);
		}
		else
		{
			ResolveStarMaxDiscrepancy(definitions, availableSize);
		}
	}

	private void ResolveStarLegacy(DefinitionBase[] definitions, double availableSize)
	{
		DefinitionBase[] tempDefinitions = TempDefinitions;
		int num = 0;
		double num2 = 0.0;
		for (int i = 0; i < definitions.Length; i++)
		{
			switch (definitions[i].SizeType)
			{
			case LayoutTimeSizeType.Auto:
				num2 += definitions[i].MinSize;
				break;
			case LayoutTimeSizeType.Pixel:
				num2 += definitions[i].MeasureSize;
				break;
			case LayoutTimeSizeType.Star:
			{
				tempDefinitions[num++] = definitions[i];
				double value = definitions[i].UserSize.Value;
				if (_IsZero(value))
				{
					definitions[i].MeasureSize = 0.0;
					definitions[i].SizeCache = 0.0;
					break;
				}
				value = Math.Min(value, 1E+298);
				definitions[i].MeasureSize = value;
				double val = Math.Max(definitions[i].MinSize, definitions[i].UserMaxSize);
				val = Math.Min(val, 1E+298);
				definitions[i].SizeCache = val / value;
				break;
			}
			}
		}
		if (num <= 0)
		{
			return;
		}
		tempDefinitions.AsSpan(0, num).Sort(s_starDistributionOrderComparer);
		double num3 = 0.0;
		int num4 = num - 1;
		do
		{
			num3 += tempDefinitions[num4].MeasureSize;
			tempDefinitions[num4].SizeCache = num3;
		}
		while (--num4 >= 0);
		num4 = 0;
		do
		{
			double measureSize = tempDefinitions[num4].MeasureSize;
			double num5;
			if (_IsZero(measureSize))
			{
				num5 = tempDefinitions[num4].MinSize;
			}
			else
			{
				num5 = Math.Min(Math.Max(availableSize - num2, 0.0) * (measureSize / tempDefinitions[num4].SizeCache), tempDefinitions[num4].UserMaxSize);
				num5 = Math.Max(tempDefinitions[num4].MinSize, num5);
			}
			tempDefinitions[num4].MeasureSize = num5;
			num2 += num5;
		}
		while (++num4 < num);
	}

	private void ResolveStarMaxDiscrepancy(DefinitionBase[] definitions, double availableSize)
	{
		int num = definitions.Length;
		DefinitionBase[] tempDefinitions = TempDefinitions;
		int num2 = 0;
		int num3 = 0;
		double num4 = 0.0;
		double num5 = 0.0;
		int num6 = 0;
		double scale = 1.0;
		double num7 = 0.0;
		for (int i = 0; i < num; i++)
		{
			DefinitionBase definitionBase = definitions[i];
			if (definitionBase.SizeType == LayoutTimeSizeType.Star)
			{
				num6++;
				definitionBase.MeasureSize = 1.0;
				if (definitionBase.UserSize.Value > num7)
				{
					num7 = definitionBase.UserSize.Value;
				}
			}
		}
		if (double.IsPositiveInfinity(num7))
		{
			scale = -1.0;
		}
		else if (num6 > 0)
		{
			double num8 = Math.Floor(Math.Log(double.MaxValue / num7 / (double)num6, 2.0));
			if (num8 < 0.0)
			{
				scale = Math.Pow(2.0, num8 - 4.0);
			}
		}
		bool flag = true;
		while (flag)
		{
			num5 = 0.0;
			num4 = 0.0;
			num2 = (num3 = 0);
			for (int j = 0; j < num; j++)
			{
				DefinitionBase definitionBase2 = definitions[j];
				switch (definitionBase2.SizeType)
				{
				case LayoutTimeSizeType.Auto:
					num4 += definitions[j].MinSize;
					break;
				case LayoutTimeSizeType.Pixel:
					num4 += definitionBase2.MeasureSize;
					break;
				case LayoutTimeSizeType.Star:
				{
					if (definitionBase2.MeasureSize < 0.0)
					{
						num4 += 0.0 - definitionBase2.MeasureSize;
						break;
					}
					double num9 = StarWeight(definitionBase2, scale);
					num5 += num9;
					if (definitionBase2.MinSize > 0.0)
					{
						tempDefinitions[num2++] = definitionBase2;
						definitionBase2.MeasureSize = num9 / definitionBase2.MinSize;
					}
					double num10 = Math.Max(definitionBase2.MinSize, definitionBase2.UserMaxSize);
					if (!double.IsPositiveInfinity(num10))
					{
						tempDefinitions[num + num3++] = definitionBase2;
						definitionBase2.SizeCache = num9 / num10;
					}
					break;
				}
				}
			}
			int num11 = num2;
			int num12 = num3;
			double num13 = 0.0;
			double num14 = availableSize - num4;
			double num15 = num5 - num13;
			tempDefinitions.AsSpan(0, num2).Sort(s_minRatioComparer);
			tempDefinitions.AsSpan(num, num3).Sort(s_maxRatioComparer);
			while (num2 + num3 > 0 && num14 > 0.0)
			{
				if (num15 < num5 * (1.0 / 256.0))
				{
					num13 = 0.0;
					num5 = 0.0;
					for (int k = 0; k < num; k++)
					{
						DefinitionBase definitionBase3 = definitions[k];
						if (definitionBase3.SizeType == LayoutTimeSizeType.Star && definitionBase3.MeasureSize > 0.0)
						{
							num5 += StarWeight(definitionBase3, scale);
						}
					}
					num15 = num5 - num13;
				}
				double minRatio = ((num2 > 0) ? tempDefinitions[num2 - 1].MeasureSize : double.PositiveInfinity);
				double maxRatio = ((num3 > 0) ? tempDefinitions[num + num3 - 1].SizeCache : (-1.0));
				double proportion = num15 / num14;
				bool? flag2 = Choose(minRatio, maxRatio, proportion);
				if (!flag2.HasValue)
				{
					break;
				}
				DefinitionBase definitionBase4;
				double num16;
				if (flag2 == true)
				{
					definitionBase4 = tempDefinitions[num2 - 1];
					num16 = definitionBase4.MinSize;
					num2--;
				}
				else
				{
					definitionBase4 = tempDefinitions[num + num3 - 1];
					num16 = Math.Max(definitionBase4.MinSize, definitionBase4.UserMaxSize);
					num3--;
				}
				num4 += num16;
				definitionBase4.MeasureSize = 0.0 - num16;
				num13 += StarWeight(definitionBase4, scale);
				num6--;
				num14 = availableSize - num4;
				num15 = num5 - num13;
				while (num2 > 0 && tempDefinitions[num2 - 1].MeasureSize < 0.0)
				{
					num2--;
					tempDefinitions[num2] = null;
				}
				while (num3 > 0 && tempDefinitions[num + num3 - 1].MeasureSize < 0.0)
				{
					num3--;
					tempDefinitions[num + num3] = null;
				}
			}
			flag = false;
			if (num6 == 0 && num4 < availableSize)
			{
				for (int l = num2; l < num11; l++)
				{
					DefinitionBase definitionBase5 = tempDefinitions[l];
					if (definitionBase5 != null)
					{
						definitionBase5.MeasureSize = 1.0;
						num6++;
						flag = true;
					}
				}
			}
			if (!(num4 > availableSize))
			{
				continue;
			}
			for (int m = num3; m < num12; m++)
			{
				DefinitionBase definitionBase6 = tempDefinitions[num + m];
				if (definitionBase6 != null)
				{
					definitionBase6.MeasureSize = 1.0;
					num6++;
					flag = true;
				}
			}
		}
		num6 = 0;
		for (int n = 0; n < num; n++)
		{
			DefinitionBase definitionBase7 = definitions[n];
			if (definitionBase7.SizeType == LayoutTimeSizeType.Star)
			{
				if (definitionBase7.MeasureSize < 0.0)
				{
					definitionBase7.MeasureSize = 0.0 - definitionBase7.MeasureSize;
					continue;
				}
				tempDefinitions[num6++] = definitionBase7;
				definitionBase7.MeasureSize = StarWeight(definitionBase7, scale);
			}
		}
		if (num6 > 0)
		{
			tempDefinitions.AsSpan(0, num6).Sort(s_starWeightComparer);
			num5 = 0.0;
			for (int num17 = 0; num17 < num6; num17++)
			{
				DefinitionBase definitionBase8 = tempDefinitions[num17];
				num5 = (definitionBase8.SizeCache = num5 + definitionBase8.MeasureSize);
			}
			for (int num19 = num6 - 1; num19 >= 0; num19--)
			{
				DefinitionBase definitionBase9 = tempDefinitions[num19];
				double val = ((definitionBase9.MeasureSize > 0.0) ? (Math.Max(availableSize - num4, 0.0) * (definitionBase9.MeasureSize / definitionBase9.SizeCache)) : 0.0);
				val = Math.Min(val, definitionBase9.UserMaxSize);
				val = (definitionBase9.MeasureSize = Math.Max(definitionBase9.MinSize, val));
				num4 += val;
			}
		}
	}

	private double CalculateDesiredSize(DefinitionBase[] definitions)
	{
		double num = 0.0;
		for (int i = 0; i < definitions.Length; i++)
		{
			num += definitions[i].MinSize;
		}
		return num;
	}

	private void SetFinalSize(DefinitionBase[] definitions, double finalSize, bool columns)
	{
		if (FrameworkAppContextSwitches.GridStarDefinitionsCanExceedAvailableSpace)
		{
			SetFinalSizeLegacy(definitions, finalSize, columns);
		}
		else
		{
			SetFinalSizeMaxDiscrepancy(definitions, finalSize, columns);
		}
	}

	private void SetFinalSizeLegacy(DefinitionBase[] definitions, double finalSize, bool columns)
	{
		int num = 0;
		int num2 = definitions.Length;
		double num3 = 0.0;
		bool useLayoutRounding = base.UseLayoutRounding;
		int[] definitionIndices = DefinitionIndices;
		double[] array = null;
		double dpiScale = 1.0;
		if (useLayoutRounding)
		{
			DpiScale dpi = GetDpi();
			dpiScale = (columns ? dpi.DpiScaleX : dpi.DpiScaleY);
			array = RoundingErrors;
		}
		for (int i = 0; i < definitions.Length; i++)
		{
			if (definitions[i].UserSize.IsStar)
			{
				double value = definitions[i].UserSize.Value;
				if (_IsZero(value))
				{
					definitions[i].MeasureSize = 0.0;
					definitions[i].SizeCache = 0.0;
				}
				else
				{
					value = Math.Min(value, 1E+298);
					definitions[i].MeasureSize = value;
					double val = Math.Max(definitions[i].MinSizeForArrange, definitions[i].UserMaxSize);
					val = Math.Min(val, 1E+298);
					definitions[i].SizeCache = val / value;
					if (useLayoutRounding)
					{
						array[i] = definitions[i].SizeCache;
						definitions[i].SizeCache = UIElement.RoundLayoutValue(definitions[i].SizeCache, dpiScale);
					}
				}
				definitionIndices[num++] = i;
				continue;
			}
			double num4 = 0.0;
			switch (definitions[i].UserSize.GridUnitType)
			{
			case GridUnitType.Pixel:
				num4 = definitions[i].UserSize.Value;
				break;
			case GridUnitType.Auto:
				num4 = definitions[i].MinSizeForArrange;
				break;
			}
			double val2 = ((!definitions[i].IsShared) ? definitions[i].UserMaxSize : num4);
			definitions[i].SizeCache = Math.Max(definitions[i].MinSizeForArrange, Math.Min(num4, val2));
			if (useLayoutRounding)
			{
				array[i] = definitions[i].SizeCache;
				definitions[i].SizeCache = UIElement.RoundLayoutValue(definitions[i].SizeCache, dpiScale);
			}
			num3 += definitions[i].SizeCache;
			definitionIndices[--num2] = i;
		}
		if (num > 0)
		{
			Array.Sort(definitionIndices, 0, num, new StarDistributionOrderIndexComparer(definitions));
			double num5 = 0.0;
			int num6 = num - 1;
			do
			{
				num5 += definitions[definitionIndices[num6]].MeasureSize;
				definitions[definitionIndices[num6]].SizeCache = num5;
			}
			while (--num6 >= 0);
			num6 = 0;
			do
			{
				double measureSize = definitions[definitionIndices[num6]].MeasureSize;
				double sizeCache;
				if (_IsZero(measureSize))
				{
					sizeCache = definitions[definitionIndices[num6]].MinSizeForArrange;
				}
				else
				{
					sizeCache = Math.Min(Math.Max(finalSize - num3, 0.0) * (measureSize / definitions[definitionIndices[num6]].SizeCache), definitions[definitionIndices[num6]].UserMaxSize);
					sizeCache = Math.Max(definitions[definitionIndices[num6]].MinSizeForArrange, sizeCache);
				}
				definitions[definitionIndices[num6]].SizeCache = sizeCache;
				if (useLayoutRounding)
				{
					array[definitionIndices[num6]] = definitions[definitionIndices[num6]].SizeCache;
					definitions[definitionIndices[num6]].SizeCache = UIElement.RoundLayoutValue(definitions[definitionIndices[num6]].SizeCache, dpiScale);
				}
				num3 += definitions[definitionIndices[num6]].SizeCache;
			}
			while (++num6 < num);
		}
		if (num3 > finalSize && !_AreClose(num3, finalSize))
		{
			Array.Sort(definitionIndices, 0, definitions.Length, new DistributionOrderIndexComparer(definitions));
			double num7 = finalSize - num3;
			for (int j = 0; j < definitions.Length; j++)
			{
				int num8 = definitionIndices[j];
				double num9 = definitions[num8].SizeCache + num7 / (double)(definitions.Length - j);
				double value2 = num9;
				num9 = Math.Max(num9, definitions[num8].MinSizeForArrange);
				num9 = Math.Min(num9, definitions[num8].SizeCache);
				if (useLayoutRounding)
				{
					array[num8] = num9;
					num9 = UIElement.RoundLayoutValue(value2, dpiScale);
					num9 = Math.Max(num9, definitions[num8].MinSizeForArrange);
					num9 = Math.Min(num9, definitions[num8].SizeCache);
				}
				num7 -= num9 - definitions[num8].SizeCache;
				definitions[num8].SizeCache = num9;
			}
			num3 = finalSize - num7;
		}
		if (useLayoutRounding && !_AreClose(num3, finalSize))
		{
			for (int k = 0; k < definitions.Length; k++)
			{
				array[k] -= definitions[k].SizeCache;
				definitionIndices[k] = k;
			}
			Array.Sort(definitionIndices, 0, definitions.Length, new RoundingErrorIndexComparer(array));
			double num10 = num3;
			double num11 = UIElement.RoundLayoutValue(1.0, dpiScale);
			if (num3 > finalSize)
			{
				int num12 = definitions.Length - 1;
				while (num10 > finalSize && !_AreClose(num10, finalSize) && num12 >= 0)
				{
					DefinitionBase definitionBase = definitions[definitionIndices[num12]];
					double val3 = definitionBase.SizeCache - num11;
					val3 = Math.Max(val3, definitionBase.MinSizeForArrange);
					if (val3 < definitionBase.SizeCache)
					{
						num10 -= num11;
					}
					definitionBase.SizeCache = val3;
					num12--;
				}
			}
			else if (num3 < finalSize)
			{
				int num13 = 0;
				while (num10 < finalSize && !_AreClose(num10, finalSize) && num13 < definitions.Length)
				{
					DefinitionBase definitionBase2 = definitions[definitionIndices[num13]];
					double val4 = definitionBase2.SizeCache + num11;
					val4 = Math.Max(val4, definitionBase2.MinSizeForArrange);
					if (val4 > definitionBase2.SizeCache)
					{
						num10 += num11;
					}
					definitionBase2.SizeCache = val4;
					num13++;
				}
			}
		}
		definitions[0].FinalOffset = 0.0;
		for (int l = 0; l < definitions.Length; l++)
		{
			definitions[(l + 1) % definitions.Length].FinalOffset = definitions[l].FinalOffset + definitions[l].SizeCache;
		}
	}

	private void SetFinalSizeMaxDiscrepancy(DefinitionBase[] definitions, double finalSize, bool columns)
	{
		int num = definitions.Length;
		int[] definitionIndices = DefinitionIndices;
		int num2 = 0;
		int num3 = 0;
		double num4 = 0.0;
		double num5 = 0.0;
		int num6 = 0;
		double scale = 1.0;
		double num7 = 0.0;
		for (int i = 0; i < num; i++)
		{
			DefinitionBase definitionBase = definitions[i];
			if (definitionBase.UserSize.IsStar)
			{
				num6++;
				definitionBase.MeasureSize = 1.0;
				if (definitionBase.UserSize.Value > num7)
				{
					num7 = definitionBase.UserSize.Value;
				}
			}
		}
		if (double.IsPositiveInfinity(num7))
		{
			scale = -1.0;
		}
		else if (num6 > 0)
		{
			double num8 = Math.Floor(Math.Log(double.MaxValue / num7 / (double)num6, 2.0));
			if (num8 < 0.0)
			{
				scale = Math.Pow(2.0, num8 - 4.0);
			}
		}
		bool flag = true;
		while (flag)
		{
			num5 = 0.0;
			num4 = 0.0;
			num2 = (num3 = 0);
			for (int j = 0; j < num; j++)
			{
				DefinitionBase definitionBase2 = definitions[j];
				if (definitionBase2.UserSize.IsStar)
				{
					if (definitionBase2.MeasureSize < 0.0)
					{
						num4 += 0.0 - definitionBase2.MeasureSize;
						continue;
					}
					double num9 = StarWeight(definitionBase2, scale);
					num5 += num9;
					if (definitionBase2.MinSizeForArrange > 0.0)
					{
						definitionIndices[num2++] = j;
						definitionBase2.MeasureSize = num9 / definitionBase2.MinSizeForArrange;
					}
					double num10 = Math.Max(definitionBase2.MinSizeForArrange, definitionBase2.UserMaxSize);
					if (!double.IsPositiveInfinity(num10))
					{
						definitionIndices[num + num3++] = j;
						definitionBase2.SizeCache = num9 / num10;
					}
				}
				else
				{
					double num11 = 0.0;
					switch (definitionBase2.UserSize.GridUnitType)
					{
					case GridUnitType.Pixel:
						num11 = definitionBase2.UserSize.Value;
						break;
					case GridUnitType.Auto:
						num11 = definitionBase2.MinSizeForArrange;
						break;
					}
					definitionBase2.SizeCache = Math.Max(val2: Math.Min(num11, (!definitionBase2.IsShared) ? definitionBase2.UserMaxSize : num11), val1: definitionBase2.MinSizeForArrange);
					num4 += definitionBase2.SizeCache;
				}
			}
			int num12 = num2;
			int num13 = num3;
			double num14 = 0.0;
			double num15 = finalSize - num4;
			double num16 = num5 - num14;
			Array.Sort(definitionIndices, 0, num2, new MinRatioIndexComparer(definitions));
			Array.Sort(definitionIndices, num, num3, new MaxRatioIndexComparer(definitions));
			while (num2 + num3 > 0 && num15 > 0.0)
			{
				if (num16 < num5 * (1.0 / 256.0))
				{
					num14 = 0.0;
					num5 = 0.0;
					for (int k = 0; k < num; k++)
					{
						DefinitionBase definitionBase3 = definitions[k];
						if (definitionBase3.UserSize.IsStar && definitionBase3.MeasureSize > 0.0)
						{
							num5 += StarWeight(definitionBase3, scale);
						}
					}
					num16 = num5 - num14;
				}
				double minRatio = ((num2 > 0) ? definitions[definitionIndices[num2 - 1]].MeasureSize : double.PositiveInfinity);
				double maxRatio = ((num3 > 0) ? definitions[definitionIndices[num + num3 - 1]].SizeCache : (-1.0));
				double proportion = num16 / num15;
				bool? flag2 = Choose(minRatio, maxRatio, proportion);
				if (!flag2.HasValue)
				{
					break;
				}
				DefinitionBase definitionBase4;
				double num18;
				if (flag2 == true)
				{
					int num17 = definitionIndices[num2 - 1];
					definitionBase4 = definitions[num17];
					num18 = definitionBase4.MinSizeForArrange;
					num2--;
				}
				else
				{
					int num17 = definitionIndices[num + num3 - 1];
					definitionBase4 = definitions[num17];
					num18 = Math.Max(definitionBase4.MinSizeForArrange, definitionBase4.UserMaxSize);
					num3--;
				}
				num4 += num18;
				definitionBase4.MeasureSize = 0.0 - num18;
				num14 += StarWeight(definitionBase4, scale);
				num6--;
				num15 = finalSize - num4;
				num16 = num5 - num14;
				while (num2 > 0 && definitions[definitionIndices[num2 - 1]].MeasureSize < 0.0)
				{
					num2--;
					definitionIndices[num2] = -1;
				}
				while (num3 > 0 && definitions[definitionIndices[num + num3 - 1]].MeasureSize < 0.0)
				{
					num3--;
					definitionIndices[num + num3] = -1;
				}
			}
			flag = false;
			if (num4 < finalSize && DoubleUtil.AreClose(num4, finalSize) && num12 > 0)
			{
				definitions[definitionIndices[num12 - 1]].MeasureSize -= finalSize - num4;
				num4 = finalSize;
				num15 = 0.0;
			}
			if (num6 == 0 && num4 < finalSize)
			{
				for (int l = num2; l < num12; l++)
				{
					if (definitionIndices[l] >= 0)
					{
						definitions[definitionIndices[l]].MeasureSize = 1.0;
						num6++;
						flag = true;
					}
				}
			}
			if (!(num4 > finalSize))
			{
				continue;
			}
			for (int m = num3; m < num13; m++)
			{
				if (definitionIndices[num + m] >= 0)
				{
					definitions[definitionIndices[num + m]].MeasureSize = 1.0;
					num6++;
					flag = true;
				}
			}
		}
		num6 = 0;
		for (int n = 0; n < num; n++)
		{
			DefinitionBase definitionBase5 = definitions[n];
			if (definitionBase5.UserSize.IsStar)
			{
				if (definitionBase5.MeasureSize < 0.0)
				{
					definitionBase5.SizeCache = 0.0 - definitionBase5.MeasureSize;
					continue;
				}
				definitionIndices[num6++] = n;
				definitionBase5.MeasureSize = StarWeight(definitionBase5, scale);
			}
		}
		if (num6 > 0)
		{
			Array.Sort(definitionIndices, 0, num6, new StarWeightIndexComparer(definitions));
			num5 = 0.0;
			for (int num19 = 0; num19 < num6; num19++)
			{
				DefinitionBase definitionBase6 = definitions[definitionIndices[num19]];
				num5 = (definitionBase6.SizeCache = num5 + definitionBase6.MeasureSize);
			}
			for (int num21 = num6 - 1; num21 >= 0; num21--)
			{
				DefinitionBase definitionBase7 = definitions[definitionIndices[num21]];
				double val2 = ((definitionBase7.MeasureSize > 0.0) ? (Math.Max(finalSize - num4, 0.0) * (definitionBase7.MeasureSize / definitionBase7.SizeCache)) : 0.0);
				val2 = Math.Min(val2, definitionBase7.UserMaxSize);
				val2 = Math.Max(definitionBase7.MinSizeForArrange, val2);
				num4 += val2;
				definitionBase7.SizeCache = val2;
			}
		}
		if (base.UseLayoutRounding)
		{
			DpiScale dpi = GetDpi();
			double num22 = (columns ? dpi.DpiScaleX : dpi.DpiScaleY);
			double[] roundingErrors = RoundingErrors;
			double num23 = 0.0;
			for (int num24 = 0; num24 < definitions.Length; num24++)
			{
				DefinitionBase definitionBase8 = definitions[num24];
				double num25 = UIElement.RoundLayoutValue(definitionBase8.SizeCache, num22);
				roundingErrors[num24] = num25 - definitionBase8.SizeCache;
				definitionBase8.SizeCache = num25;
				num23 += num25;
			}
			if (!_AreClose(num23, finalSize))
			{
				for (int num26 = 0; num26 < definitions.Length; num26++)
				{
					definitionIndices[num26] = num26;
				}
				Array.Sort(definitionIndices, 0, definitions.Length, new RoundingErrorIndexComparer(roundingErrors));
				double num27 = num23;
				double num28 = 1.0 / num22;
				if (num23 > finalSize)
				{
					int num29 = definitions.Length - 1;
					while (num27 > finalSize && !_AreClose(num27, finalSize) && num29 >= 0)
					{
						DefinitionBase definitionBase9 = definitions[definitionIndices[num29]];
						double val3 = definitionBase9.SizeCache - num28;
						val3 = Math.Max(val3, definitionBase9.MinSizeForArrange);
						if (val3 < definitionBase9.SizeCache)
						{
							num27 -= num28;
						}
						definitionBase9.SizeCache = val3;
						num29--;
					}
				}
				else if (num23 < finalSize)
				{
					int num30 = 0;
					while (num27 < finalSize && !_AreClose(num27, finalSize) && num30 < definitions.Length)
					{
						DefinitionBase definitionBase10 = definitions[definitionIndices[num30]];
						double val4 = definitionBase10.SizeCache + num28;
						val4 = Math.Max(val4, definitionBase10.MinSizeForArrange);
						if (val4 > definitionBase10.SizeCache)
						{
							num27 += num28;
						}
						definitionBase10.SizeCache = val4;
						num30++;
					}
				}
			}
		}
		definitions[0].FinalOffset = 0.0;
		for (int num31 = 0; num31 < definitions.Length; num31++)
		{
			definitions[(num31 + 1) % definitions.Length].FinalOffset = definitions[num31].FinalOffset + definitions[num31].SizeCache;
		}
	}

	private static bool? Choose(double minRatio, double maxRatio, double proportion)
	{
		if (minRatio < proportion)
		{
			if (maxRatio > proportion)
			{
				double num = Math.Floor(Math.Log(minRatio, 2.0));
				double num2 = Math.Floor(Math.Log(maxRatio, 2.0));
				double num3 = Math.Pow(2.0, Math.Floor((num + num2) / 2.0));
				if (proportion / num3 * (proportion / num3) > minRatio / num3 * (maxRatio / num3))
				{
					return true;
				}
				return false;
			}
			return true;
		}
		if (maxRatio > proportion)
		{
			return false;
		}
		return null;
	}

	private static int CompareRoundingErrors(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
	{
		if (x.Value < y.Value)
		{
			return -1;
		}
		if (x.Value > y.Value)
		{
			return 1;
		}
		return 0;
	}

	private double GetFinalSizeForRange(DefinitionBase[] definitions, int start, int count)
	{
		double num = 0.0;
		int num2 = start + count - 1;
		do
		{
			num += definitions[num2].SizeCache;
		}
		while (--num2 >= start);
		return num;
	}

	private void SetValid()
	{
		ExtendedData extData = ExtData;
		if (extData != null && extData.TempDefinitions != null)
		{
			Array.Clear(extData.TempDefinitions, 0, Math.Max(DefinitionsU.Length, DefinitionsV.Length));
			extData.TempDefinitions = null;
		}
	}

	/// <summary>Returns true if <see cref="T:System.Windows.Controls.ColumnDefinitionCollection" /> associated with this instance of <see cref="T:System.Windows.Controls.Grid" /> is not empty.</summary>
	/// <returns>true if <see cref="T:System.Windows.Controls.ColumnDefinitionCollection" /> associated with this instance of <see cref="T:System.Windows.Controls.Grid" /> is not empty; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeColumnDefinitions()
	{
		ExtendedData extData = ExtData;
		if (extData != null && extData.ColumnDefinitions != null)
		{
			return extData.ColumnDefinitions.Count > 0;
		}
		return false;
	}

	/// <summary>Returns true if <see cref="T:System.Windows.Controls.RowDefinitionCollection" /> associated with this instance of <see cref="T:System.Windows.Controls.Grid" /> is not empty.</summary>
	/// <returns>true if <see cref="T:System.Windows.Controls.RowDefinitionCollection" /> associated with this instance of <see cref="T:System.Windows.Controls.Grid" /> is not empty; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeRowDefinitions()
	{
		ExtendedData extData = ExtData;
		if (extData != null && extData.RowDefinitions != null)
		{
			return extData.RowDefinitions.Count > 0;
		}
		return false;
	}

	private GridLinesRenderer EnsureGridLinesRenderer()
	{
		if (ShowGridLines && _gridLinesRenderer == null)
		{
			_gridLinesRenderer = new GridLinesRenderer();
			AddVisualChild(_gridLinesRenderer);
		}
		if (!ShowGridLines && _gridLinesRenderer != null)
		{
			RemoveVisualChild(_gridLinesRenderer);
			_gridLinesRenderer = null;
		}
		return _gridLinesRenderer;
	}

	private void SetFlags(bool value, Flags flags)
	{
		_flags = (value ? (_flags | flags) : (_flags & ~flags));
	}

	private bool CheckFlagsAnd(Flags flags)
	{
		return (_flags & flags) == flags;
	}

	private bool CheckFlagsOr(Flags flags)
	{
		if (flags != 0)
		{
			return (_flags & flags) != 0;
		}
		return true;
	}

	private static void OnShowGridLinesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		Grid grid = (Grid)d;
		if (grid.ExtData != null && grid.ListenToNotifications)
		{
			grid.InvalidateVisual();
		}
		grid.SetFlags((bool)e.NewValue, Flags.ShowGridLinesPropertyValue);
	}

	private static void OnCellAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is Visual reference && VisualTreeHelper.GetParent(reference) is Grid { ExtData: not null, ListenToNotifications: not false } grid)
		{
			grid.CellsStructureDirty = true;
			grid.InvalidateMeasure();
		}
	}

	private static bool IsIntValueNotNegative(object value)
	{
		return (int)value >= 0;
	}

	private static bool IsIntValueGreaterThanZero(object value)
	{
		return (int)value > 0;
	}

	private static bool CompareNullRefs(object x, object y, out int result)
	{
		result = 2;
		if (x == null)
		{
			if (y == null)
			{
				result = 0;
			}
			else
			{
				result = -1;
			}
		}
		else if (y == null)
		{
			result = 1;
		}
		return result != 2;
	}

	private static bool _IsZero(double d)
	{
		return Math.Abs(d) < 1E-05;
	}

	private static bool _AreClose(double d1, double d2)
	{
		return Math.Abs(d1 - d2) < 1E-05;
	}

	private static double StarWeight(DefinitionBase def, double scale)
	{
		if (scale < 0.0)
		{
			if (!double.IsPositiveInfinity(def.UserSize.Value))
			{
				return 0.0;
			}
			return 1.0;
		}
		return def.UserSize.Value * scale;
	}

	private static int SpanPreferredDistributionOrderComparer(DefinitionBase x, DefinitionBase y)
	{
		if (!CompareNullRefs(x, y, out var result))
		{
			if (x.UserSize.IsAuto)
			{
				if (y.UserSize.IsAuto)
				{
					return x.MinSize.CompareTo(y.MinSize);
				}
				return -1;
			}
			if (y.UserSize.IsAuto)
			{
				return 1;
			}
			return x.PreferredSize.CompareTo(y.PreferredSize);
		}
		return result;
	}

	private static int SpanMaxDistributionOrderComparer(DefinitionBase x, DefinitionBase y)
	{
		if (!CompareNullRefs(x, y, out var result))
		{
			if (x.UserSize.IsAuto)
			{
				if (y.UserSize.IsAuto)
				{
					return x.SizeCache.CompareTo(y.SizeCache);
				}
				return 1;
			}
			if (y.UserSize.IsAuto)
			{
				return -1;
			}
			return x.SizeCache.CompareTo(y.SizeCache);
		}
		return result;
	}

	private static int StarDistributionOrderComparer(DefinitionBase x, DefinitionBase y)
	{
		if (!CompareNullRefs(x, y, out var result))
		{
			return x.SizeCache.CompareTo(y.SizeCache);
		}
		return result;
	}

	private static int MinRatioComparer(DefinitionBase x, DefinitionBase y)
	{
		if (!CompareNullRefs(y, x, out var result))
		{
			return y.MeasureSize.CompareTo(x.MeasureSize);
		}
		return result;
	}

	private static int MaxRatioComparer(DefinitionBase x, DefinitionBase y)
	{
		if (!CompareNullRefs(x, y, out var result))
		{
			return x.SizeCache.CompareTo(y.SizeCache);
		}
		return result;
	}

	private static int StarWeightComparer(DefinitionBase x, DefinitionBase y)
	{
		if (!CompareNullRefs(x, y, out var result))
		{
			return x.MeasureSize.CompareTo(y.MeasureSize);
		}
		return result;
	}

	[Conditional("GRIDPARANOIA")]
	internal void EnterCounterScope(Counters scopeCounter)
	{
	}

	[Conditional("GRIDPARANOIA")]
	internal void ExitCounterScope(Counters scopeCounter)
	{
	}

	[Conditional("GRIDPARANOIA")]
	internal void EnterCounter(Counters counter)
	{
	}

	[Conditional("GRIDPARANOIA")]
	internal void ExitCounter(Counters counter)
	{
	}
}
