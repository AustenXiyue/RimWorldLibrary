using System.Collections;
using System.Collections.Generic;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Defines the functionality required to support a shared-size group that is used by the <see cref="T:System.Windows.Controls.ColumnDefinitionCollection" /> and <see cref="T:System.Windows.Controls.RowDefinitionCollection" /> classes. This is an abstract class. </summary>
[Localizability(LocalizationCategory.Ignore)]
public abstract class DefinitionBase : FrameworkContentElement
{
	[Flags]
	private enum Flags : byte
	{
		UseSharedMinimum = 0x20,
		LayoutWasUpdated = 0x40
	}

	private class SharedSizeScope
	{
		private Hashtable _registry = new Hashtable();

		internal SharedSizeState EnsureSharedState(string sharedSizeGroup)
		{
			SharedSizeState sharedSizeState = _registry[sharedSizeGroup] as SharedSizeState;
			if (sharedSizeState == null)
			{
				sharedSizeState = new SharedSizeState(this, sharedSizeGroup);
				_registry[sharedSizeGroup] = sharedSizeState;
			}
			return sharedSizeState;
		}

		internal void Remove(object key)
		{
			_registry.Remove(key);
		}
	}

	private class SharedSizeState
	{
		private readonly SharedSizeScope _sharedSizeScope;

		private readonly string _sharedSizeGroupId;

		private readonly List<DefinitionBase> _registry;

		private readonly EventHandler _layoutUpdated;

		private UIElement _layoutUpdatedHost;

		private bool _broadcastInvalidation;

		private bool _userSizeValid;

		private GridLength _userSize;

		private double _minSize;

		internal double MinSize
		{
			get
			{
				if (!_userSizeValid)
				{
					EnsureUserSizeValid();
				}
				return _minSize;
			}
		}

		internal GridLength UserSize
		{
			get
			{
				if (!_userSizeValid)
				{
					EnsureUserSizeValid();
				}
				return _userSize;
			}
		}

		internal SharedSizeState(SharedSizeScope sharedSizeScope, string sharedSizeGroupId)
		{
			_sharedSizeScope = sharedSizeScope;
			_sharedSizeGroupId = sharedSizeGroupId;
			_registry = new List<DefinitionBase>();
			_layoutUpdated = OnLayoutUpdated;
			_broadcastInvalidation = true;
		}

		internal void AddMember(DefinitionBase member)
		{
			_registry.Add(member);
			Invalidate();
		}

		internal void RemoveMember(DefinitionBase member)
		{
			Invalidate();
			_registry.Remove(member);
			if (_registry.Count == 0)
			{
				_sharedSizeScope.Remove(_sharedSizeGroupId);
			}
		}

		internal void Invalidate()
		{
			_userSizeValid = false;
			if (_broadcastInvalidation)
			{
				int i = 0;
				for (int count = _registry.Count; i < count; i++)
				{
					((Grid)_registry[i].Parent).Invalidate();
				}
				_broadcastInvalidation = false;
			}
		}

		internal void EnsureDeferredValidation(UIElement layoutUpdatedHost)
		{
			if (_layoutUpdatedHost == null)
			{
				_layoutUpdatedHost = layoutUpdatedHost;
				_layoutUpdatedHost.LayoutUpdated += _layoutUpdated;
			}
		}

		private void EnsureUserSizeValid()
		{
			_userSize = new GridLength(1.0, GridUnitType.Auto);
			int i = 0;
			for (int count = _registry.Count; i < count; i++)
			{
				GridLength userSizeValueCache = _registry[i].UserSizeValueCache;
				if (userSizeValueCache.GridUnitType == GridUnitType.Pixel)
				{
					if (_userSize.GridUnitType == GridUnitType.Auto)
					{
						_userSize = userSizeValueCache;
					}
					else if (_userSize.Value < userSizeValueCache.Value)
					{
						_userSize = userSizeValueCache;
					}
				}
			}
			_minSize = (_userSize.IsAbsolute ? _userSize.Value : 0.0);
			_userSizeValid = true;
		}

		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			double num = 0.0;
			int i = 0;
			for (int count = _registry.Count; i < count; i++)
			{
				num = Math.Max(num, _registry[i]._minSize);
			}
			bool flag = !DoubleUtil.AreClose(_minSize, num);
			int j = 0;
			for (int count2 = _registry.Count; j < count2; j++)
			{
				DefinitionBase definitionBase = _registry[j];
				bool flag2 = !DoubleUtil.AreClose(definitionBase._minSize, num);
				if (!((!definitionBase.UseSharedMinimum) ? (!flag2) : ((!flag2) ? (definitionBase.LayoutWasUpdated && DoubleUtil.GreaterThanOrClose(definitionBase._minSize, MinSize)) : (!flag))))
				{
					((Grid)definitionBase.Parent).InvalidateMeasure();
				}
				else if (!DoubleUtil.AreClose(num, definitionBase.SizeCache))
				{
					((Grid)definitionBase.Parent).InvalidateArrange();
				}
				definitionBase.UseSharedMinimum = flag2;
				definitionBase.LayoutWasUpdated = false;
			}
			_minSize = num;
			_layoutUpdatedHost.LayoutUpdated -= _layoutUpdated;
			_layoutUpdatedHost = null;
			_broadcastInvalidation = true;
		}
	}

	private readonly bool _isColumnDefinition;

	private Flags _flags;

	private int _parentIndex;

	private Grid.LayoutTimeSizeType _sizeType;

	private double _minSize;

	private double _measureSize;

	private double _sizeCache;

	private double _offset;

	private SharedSizeState _sharedState;

	internal const bool ThisIsColumnDefinition = true;

	internal const bool ThisIsRowDefinition = false;

	internal static readonly DependencyProperty PrivateSharedSizeScopeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DefinitionBase.SharedSizeGroup" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DefinitionBase.SharedSizeGroup" /> dependency property.</returns>
	public static readonly DependencyProperty SharedSizeGroupProperty;

	/// <summary>Gets or sets a value that identifies a <see cref="T:System.Windows.Controls.ColumnDefinition" /> or <see cref="T:System.Windows.Controls.RowDefinition" /> as a member of a defined group that shares sizing properties.   </summary>
	/// <returns>A <see cref="T:System.String" /> that identifies a shared-size group.</returns>
	public string SharedSizeGroup
	{
		get
		{
			return (string)GetValue(SharedSizeGroupProperty);
		}
		set
		{
			SetValue(SharedSizeGroupProperty, value);
		}
	}

	internal bool IsShared => _sharedState != null;

	internal GridLength UserSize
	{
		get
		{
			if (_sharedState == null)
			{
				return UserSizeValueCache;
			}
			return _sharedState.UserSize;
		}
	}

	internal double UserMinSize => UserMinSizeValueCache;

	internal double UserMaxSize => UserMaxSizeValueCache;

	internal int Index
	{
		get
		{
			return _parentIndex;
		}
		set
		{
			_parentIndex = value;
		}
	}

	internal Grid.LayoutTimeSizeType SizeType
	{
		get
		{
			return _sizeType;
		}
		set
		{
			_sizeType = value;
		}
	}

	internal double MeasureSize
	{
		get
		{
			return _measureSize;
		}
		set
		{
			_measureSize = value;
		}
	}

	internal double PreferredSize
	{
		get
		{
			double num = MinSize;
			if (_sizeType != Grid.LayoutTimeSizeType.Auto && num < _measureSize)
			{
				num = _measureSize;
			}
			return num;
		}
	}

	internal double SizeCache
	{
		get
		{
			return _sizeCache;
		}
		set
		{
			_sizeCache = value;
		}
	}

	internal double MinSize
	{
		get
		{
			double minSize = _minSize;
			if (UseSharedMinimum && _sharedState != null && minSize < _sharedState.MinSize)
			{
				minSize = _sharedState.MinSize;
			}
			return minSize;
		}
	}

	internal double MinSizeForArrange
	{
		get
		{
			double minSize = _minSize;
			if (_sharedState != null && (UseSharedMinimum || !LayoutWasUpdated) && minSize < _sharedState.MinSize)
			{
				minSize = _sharedState.MinSize;
			}
			return minSize;
		}
	}

	internal double RawMinSize => _minSize;

	internal double FinalOffset
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

	internal GridLength UserSizeValueCache => (GridLength)GetValue(_isColumnDefinition ? ColumnDefinition.WidthProperty : RowDefinition.HeightProperty);

	internal double UserMinSizeValueCache => (double)GetValue(_isColumnDefinition ? ColumnDefinition.MinWidthProperty : RowDefinition.MinHeightProperty);

	internal double UserMaxSizeValueCache => (double)GetValue(_isColumnDefinition ? ColumnDefinition.MaxWidthProperty : RowDefinition.MaxHeightProperty);

	internal bool InParentLogicalTree => _parentIndex != -1;

	private SharedSizeScope PrivateSharedSizeScope => (SharedSizeScope)GetValue(PrivateSharedSizeScopeProperty);

	private bool UseSharedMinimum
	{
		get
		{
			return CheckFlagsAnd(Flags.UseSharedMinimum);
		}
		set
		{
			SetFlags(value, Flags.UseSharedMinimum);
		}
	}

	private bool LayoutWasUpdated
	{
		get
		{
			return CheckFlagsAnd(Flags.LayoutWasUpdated);
		}
		set
		{
			SetFlags(value, Flags.LayoutWasUpdated);
		}
	}

	internal DefinitionBase(bool isColumnDefinition)
	{
		_isColumnDefinition = isColumnDefinition;
		_parentIndex = -1;
	}

	internal void OnEnterParentTree()
	{
		if (_sharedState != null)
		{
			return;
		}
		string sharedSizeGroup = SharedSizeGroup;
		if (sharedSizeGroup != null)
		{
			SharedSizeScope privateSharedSizeScope = PrivateSharedSizeScope;
			if (privateSharedSizeScope != null)
			{
				_sharedState = privateSharedSizeScope.EnsureSharedState(sharedSizeGroup);
				_sharedState.AddMember(this);
			}
		}
	}

	internal void OnExitParentTree()
	{
		_offset = 0.0;
		if (_sharedState != null)
		{
			_sharedState.RemoveMember(this);
			_sharedState = null;
		}
	}

	internal void OnBeforeLayout(Grid grid)
	{
		_minSize = 0.0;
		LayoutWasUpdated = true;
		if (_sharedState != null)
		{
			_sharedState.EnsureDeferredValidation(grid);
		}
	}

	internal void UpdateMinSize(double minSize)
	{
		_minSize = Math.Max(_minSize, minSize);
	}

	internal void SetMinSize(double minSize)
	{
		_minSize = minSize;
	}

	internal static void OnUserSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DefinitionBase definitionBase = (DefinitionBase)d;
		if (!definitionBase.InParentLogicalTree)
		{
			return;
		}
		if (definitionBase._sharedState != null)
		{
			definitionBase._sharedState.Invalidate();
			return;
		}
		Grid grid = (Grid)definitionBase.Parent;
		if (((GridLength)e.OldValue).GridUnitType != ((GridLength)e.NewValue).GridUnitType)
		{
			grid.Invalidate();
		}
		else
		{
			grid.InvalidateMeasure();
		}
	}

	internal static bool IsUserSizePropertyValueValid(object value)
	{
		return ((GridLength)value).Value >= 0.0;
	}

	internal static void OnUserMinSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DefinitionBase definitionBase = (DefinitionBase)d;
		if (definitionBase.InParentLogicalTree)
		{
			((Grid)definitionBase.Parent).InvalidateMeasure();
		}
	}

	internal static bool IsUserMinSizePropertyValueValid(object value)
	{
		double num = (double)value;
		if (!double.IsNaN(num) && num >= 0.0)
		{
			return !double.IsPositiveInfinity(num);
		}
		return false;
	}

	internal static void OnUserMaxSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DefinitionBase definitionBase = (DefinitionBase)d;
		if (definitionBase.InParentLogicalTree)
		{
			((Grid)definitionBase.Parent).InvalidateMeasure();
		}
	}

	internal static bool IsUserMaxSizePropertyValueValid(object value)
	{
		double num = (double)value;
		if (!double.IsNaN(num))
		{
			return num >= 0.0;
		}
		return false;
	}

	internal static void OnIsSharedSizeScopePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
		{
			SharedSizeScope value = new SharedSizeScope();
			d.SetValue(PrivateSharedSizeScopeProperty, value);
		}
		else
		{
			d.ClearValue(PrivateSharedSizeScopeProperty);
		}
	}

	private void SetFlags(bool value, Flags flags)
	{
		_flags = (value ? (_flags | flags) : ((Flags)((uint)_flags & (uint)(byte)(~(int)flags))));
	}

	private bool CheckFlagsAnd(Flags flags)
	{
		return (_flags & flags) == flags;
	}

	private static void OnSharedSizeGroupPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DefinitionBase definitionBase = (DefinitionBase)d;
		if (!definitionBase.InParentLogicalTree)
		{
			return;
		}
		string text = (string)e.NewValue;
		if (definitionBase._sharedState != null)
		{
			definitionBase._sharedState.RemoveMember(definitionBase);
			definitionBase._sharedState = null;
		}
		if (definitionBase._sharedState == null && text != null)
		{
			SharedSizeScope privateSharedSizeScope = definitionBase.PrivateSharedSizeScope;
			if (privateSharedSizeScope != null)
			{
				definitionBase._sharedState = privateSharedSizeScope.EnsureSharedState(text);
				definitionBase._sharedState.AddMember(definitionBase);
			}
		}
	}

	private static bool SharedSizeGroupPropertyValueValid(object value)
	{
		if (value == null)
		{
			return true;
		}
		string text = (string)value;
		if (text != string.Empty)
		{
			int num = -1;
			while (++num < text.Length)
			{
				bool flag = char.IsDigit(text[num]);
				if ((num == 0 && flag) || (!flag && !char.IsLetter(text[num]) && '_' != text[num]))
				{
					break;
				}
			}
			if (num == text.Length)
			{
				return true;
			}
		}
		return false;
	}

	private static void OnPrivateSharedSizeScopePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DefinitionBase definitionBase = (DefinitionBase)d;
		if (definitionBase.InParentLogicalTree)
		{
			SharedSizeScope sharedSizeScope = (SharedSizeScope)e.NewValue;
			if (definitionBase._sharedState != null)
			{
				definitionBase._sharedState.RemoveMember(definitionBase);
				definitionBase._sharedState = null;
			}
			if (definitionBase._sharedState == null && sharedSizeScope != null && definitionBase.SharedSizeGroup != null)
			{
				definitionBase._sharedState = sharedSizeScope.EnsureSharedState(definitionBase.SharedSizeGroup);
				definitionBase._sharedState.AddMember(definitionBase);
			}
		}
	}

	static DefinitionBase()
	{
		PrivateSharedSizeScopeProperty = DependencyProperty.RegisterAttached("PrivateSharedSizeScope", typeof(SharedSizeScope), typeof(DefinitionBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
		SharedSizeGroupProperty = DependencyProperty.Register("SharedSizeGroup", typeof(string), typeof(DefinitionBase), new FrameworkPropertyMetadata(OnSharedSizeGroupPropertyChanged), SharedSizeGroupPropertyValueValid);
		PrivateSharedSizeScopeProperty.OverrideMetadata(typeof(DefinitionBase), new FrameworkPropertyMetadata(OnPrivateSharedSizeScopePropertyChanged));
	}
}
