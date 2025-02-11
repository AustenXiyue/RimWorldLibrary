using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Documents;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;
using MS.Win32;

namespace System.Windows.Input;

/// <summary>Provides logical and directional navigation between focusable objects. </summary>
public sealed class KeyboardNavigation
{
	private sealed class FocusVisualAdorner : Adorner
	{
		private GeneralTransform _hostToAdornedElement = Transform.Identity;

		private IContentHost _contentHostParent;

		private ContentElement _adornedContentElement;

		private Style _focusVisualStyle;

		private UIElement _adorderChild;

		private UIElementCollection _canvasChildren;

		private ReadOnlyCollection<Rect> _contentRects;

		protected override int VisualChildrenCount => 1;

		private IContentHost ContentHost
		{
			get
			{
				if (_adornedContentElement != null && (_contentHostParent == null || VisualTreeHelper.GetParent(_contentHostParent as Visual) == null))
				{
					_contentHostParent = ContentHostHelper.FindContentHost(_adornedContentElement);
				}
				return _contentHostParent;
			}
		}

		public FocusVisualAdorner(UIElement adornedElement, Style focusVisualStyle)
			: base(adornedElement)
		{
			_adorderChild = new Control
			{
				Style = focusVisualStyle
			};
			base.IsClipEnabled = true;
			base.IsHitTestVisible = false;
			base.IsEnabled = false;
			AddVisualChild(_adorderChild);
		}

		public FocusVisualAdorner(ContentElement adornedElement, UIElement adornedElementParent, IContentHost contentHostParent, Style focusVisualStyle)
			: base(adornedElementParent)
		{
			_contentHostParent = contentHostParent;
			_adornedContentElement = adornedElement;
			_focusVisualStyle = focusVisualStyle;
			Canvas canvas = new Canvas();
			_canvasChildren = canvas.Children;
			_adorderChild = canvas;
			AddVisualChild(_adorderChild);
			base.IsClipEnabled = true;
			base.IsHitTestVisible = false;
			base.IsEnabled = false;
		}

		protected override Size MeasureOverride(Size constraint)
		{
			Size size = default(Size);
			if (_adornedContentElement == null)
			{
				size = base.AdornedElement.RenderSize;
				constraint = size;
			}
			((UIElement)GetVisualChild(0)).Measure(constraint);
			return size;
		}

		protected override Size ArrangeOverride(Size size)
		{
			Size size2 = base.ArrangeOverride(size);
			if (_adornedContentElement != null)
			{
				if (_contentRects == null)
				{
					_canvasChildren.Clear();
				}
				else
				{
					IContentHost contentHost = ContentHost;
					if (!(contentHost is Visual) || !base.AdornedElement.IsAncestorOf((Visual)contentHost))
					{
						_canvasChildren.Clear();
						return default(Size);
					}
					_ = Rect.Empty;
					IEnumerator<Rect> enumerator = _contentRects.GetEnumerator();
					if (_canvasChildren.Count == _contentRects.Count)
					{
						for (int i = 0; i < _canvasChildren.Count; i++)
						{
							enumerator.MoveNext();
							Rect current = enumerator.Current;
							current = _hostToAdornedElement.TransformBounds(current);
							Control obj = (Control)_canvasChildren[i];
							obj.Width = current.Width;
							obj.Height = current.Height;
							Canvas.SetLeft(obj, current.X);
							Canvas.SetTop(obj, current.Y);
						}
						_adorderChild.InvalidateArrange();
					}
					else
					{
						_canvasChildren.Clear();
						while (enumerator.MoveNext())
						{
							Rect current2 = enumerator.Current;
							current2 = _hostToAdornedElement.TransformBounds(current2);
							Control control = new Control();
							control.Style = _focusVisualStyle;
							control.Width = current2.Width;
							control.Height = current2.Height;
							Canvas.SetLeft(control, current2.X);
							Canvas.SetTop(control, current2.Y);
							_canvasChildren.Add(control);
						}
					}
				}
			}
			((UIElement)GetVisualChild(0)).Arrange(new Rect(default(Point), size2));
			return size2;
		}

		protected override Visual GetVisualChild(int index)
		{
			if (index == 0)
			{
				return _adorderChild;
			}
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}

		internal override bool NeedsUpdate(Size oldSize)
		{
			if (_adornedContentElement != null)
			{
				ReadOnlyCollection<Rect> contentRects = _contentRects;
				_contentRects = null;
				IContentHost contentHost = ContentHost;
				if (contentHost != null)
				{
					_contentRects = contentHost.GetRectangles(_adornedContentElement);
				}
				GeneralTransform hostToAdornedElement = _hostToAdornedElement;
				if (contentHost is Visual && base.AdornedElement.IsAncestorOf((Visual)contentHost))
				{
					_hostToAdornedElement = ((Visual)contentHost).TransformToAncestor(base.AdornedElement);
				}
				else
				{
					_hostToAdornedElement = Transform.Identity;
				}
				if (hostToAdornedElement != _hostToAdornedElement && (!(hostToAdornedElement is MatrixTransform) || !(_hostToAdornedElement is MatrixTransform) || !Matrix.Equals(((MatrixTransform)hostToAdornedElement).Matrix, ((MatrixTransform)_hostToAdornedElement).Matrix)))
				{
					return true;
				}
				if (_contentRects != null && contentRects != null && _contentRects.Count == contentRects.Count)
				{
					for (int i = 0; i < contentRects.Count; i++)
					{
						if (!DoubleUtil.AreClose(contentRects[i].Size, _contentRects[i].Size))
						{
							return true;
						}
					}
					return false;
				}
				return _contentRects != contentRects;
			}
			return !DoubleUtil.AreClose(base.AdornedElement.RenderSize, oldSize);
		}
	}

	internal delegate bool EnterMenuModeEventHandler(object sender, EventArgs e);

	private class WeakReferenceList : DispatcherObject
	{
		private List<WeakReference> _list = new List<WeakReference>(1);

		private bool _isCleanupRequested;

		public int Count => _list.Count;

		public void Add(object item)
		{
			if (_list.Count == _list.Capacity)
			{
				Purge();
			}
			_list.Add(new WeakReference(item));
		}

		public void Remove(object target)
		{
			bool flag = false;
			for (int i = 0; i < _list.Count; i++)
			{
				object target2 = _list[i].Target;
				if (target2 != null)
				{
					if (target2 == target)
					{
						_list.RemoveAt(i);
						i--;
					}
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				Purge();
			}
		}

		public void Process(Func<object, bool> action)
		{
			bool flag = false;
			for (int i = 0; i < _list.Count; i++)
			{
				object target = _list[i].Target;
				if (target != null)
				{
					if (action(target))
					{
						break;
					}
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				ScheduleCleanup();
			}
		}

		private void Purge()
		{
			int num = 0;
			int count = _list.Count;
			for (int i = 0; i < count; i++)
			{
				if (_list[i].IsAlive)
				{
					_list[num++] = _list[i];
				}
			}
			if (num < count)
			{
				_list.RemoveRange(num, count - num);
				int num2 = num << 1;
				if (num2 < _list.Capacity)
				{
					_list.Capacity = num2;
				}
			}
		}

		private void ScheduleCleanup()
		{
			if (_isCleanupRequested)
			{
				return;
			}
			_isCleanupRequested = true;
			base.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (DispatcherOperationCallback)delegate
			{
				lock (this)
				{
					Purge();
					_isCleanupRequested = false;
				}
				return (object)null;
			}, null);
		}
	}

	private static readonly DependencyProperty TabOnceActiveElementProperty = DependencyProperty.RegisterAttached("TabOnceActiveElement", typeof(WeakReference), typeof(KeyboardNavigation));

	internal static readonly DependencyProperty ControlTabOnceActiveElementProperty = DependencyProperty.RegisterAttached("ControlTabOnceActiveElement", typeof(WeakReference), typeof(KeyboardNavigation));

	internal static readonly DependencyProperty DirectionalNavigationMarginProperty = DependencyProperty.RegisterAttached("DirectionalNavigationMargin", typeof(Thickness), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(default(Thickness)));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.KeyboardNavigation.TabIndex" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.KeyboardNavigation.TabIndex" /> attached property.</returns>
	public static readonly DependencyProperty TabIndexProperty = DependencyProperty.RegisterAttached("TabIndex", typeof(int), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(int.MaxValue));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.KeyboardNavigation.IsTabStop" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.KeyboardNavigation.IsTabStop" /> attached property.</returns>
	public static readonly DependencyProperty IsTabStopProperty = DependencyProperty.RegisterAttached("IsTabStop", typeof(bool), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.KeyboardNavigation.TabNavigation" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.KeyboardNavigation.TabNavigation" /> attached property.</returns>
	[CustomCategory("Accessibility")]
	[Localizability(LocalizationCategory.NeverLocalize)]
	[CommonDependencyProperty]
	public static readonly DependencyProperty TabNavigationProperty = DependencyProperty.RegisterAttached("TabNavigation", typeof(KeyboardNavigationMode), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue), IsValidKeyNavigationMode);

	/// <summary>Identifies the <see cref="P:System.Windows.Input.KeyboardNavigation.ControlTabNavigation" /> attached property. </summary>
	/// <returns>The identifier for the  attached property.</returns>
	[CustomCategory("Accessibility")]
	[Localizability(LocalizationCategory.NeverLocalize)]
	[CommonDependencyProperty]
	public static readonly DependencyProperty ControlTabNavigationProperty = DependencyProperty.RegisterAttached("ControlTabNavigation", typeof(KeyboardNavigationMode), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue), IsValidKeyNavigationMode);

	/// <summary>Identifies the <see cref="P:System.Windows.Input.KeyboardNavigation.DirectionalNavigation" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.KeyboardNavigation.DirectionalNavigation" /> attached property.</returns>
	[CustomCategory("Accessibility")]
	[Localizability(LocalizationCategory.NeverLocalize)]
	[CommonDependencyProperty]
	public static readonly DependencyProperty DirectionalNavigationProperty = DependencyProperty.RegisterAttached("DirectionalNavigation", typeof(KeyboardNavigationMode), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue), IsValidKeyNavigationMode);

	internal static readonly DependencyProperty ShowKeyboardCuesProperty = DependencyProperty.RegisterAttached("ShowKeyboardCues", typeof(bool), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior, null, CoerceShowKeyboardCues));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.KeyboardNavigation.AcceptsReturn" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.KeyboardNavigation.AcceptsReturn" /> attached property.</returns>
	public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.RegisterAttached("AcceptsReturn", typeof(bool), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

	private WeakReferenceList _weakFocusChangedHandlers = new WeakReferenceList();

	private static bool _alwaysShowFocusVisual = SystemParameters.KeyboardCues;

	private FocusVisualAdorner _focusVisualAdornerCache;

	private Key _lastKeyPressed;

	private WeakReferenceList _weakEnterMenuModeHandlers;

	private bool _win32MenuModeWorkAround;

	private WeakReferenceList _weakFocusEnterMainFocusScopeHandlers = new WeakReferenceList();

	private const double BASELINE_DEFAULT = double.MinValue;

	private double _verticalBaseline = double.MinValue;

	private double _horizontalBaseline = double.MinValue;

	private DependencyProperty _navigationProperty;

	private Hashtable _containerHashtable = new Hashtable(10);

	private static object _fakeNull = new object();

	internal static bool AlwaysShowFocusVisual
	{
		get
		{
			return _alwaysShowFocusVisual;
		}
		set
		{
			_alwaysShowFocusVisual = value;
		}
	}

	internal static KeyboardNavigation Current => FrameworkElement.KeyboardNavigation;

	internal event KeyboardFocusChangedEventHandler FocusChanged
	{
		add
		{
			lock (_weakFocusChangedHandlers)
			{
				_weakFocusChangedHandlers.Add(value);
			}
		}
		remove
		{
			lock (_weakFocusChangedHandlers)
			{
				_weakFocusChangedHandlers.Remove(value);
			}
		}
	}

	internal event EnterMenuModeEventHandler EnterMenuMode
	{
		add
		{
			if (_weakEnterMenuModeHandlers == null)
			{
				_weakEnterMenuModeHandlers = new WeakReferenceList();
			}
			lock (_weakEnterMenuModeHandlers)
			{
				_weakEnterMenuModeHandlers.Add(value);
			}
		}
		remove
		{
			if (_weakEnterMenuModeHandlers != null)
			{
				lock (_weakEnterMenuModeHandlers)
				{
					_weakEnterMenuModeHandlers.Remove(value);
				}
			}
		}
	}

	internal event EventHandler FocusEnterMainFocusScope
	{
		add
		{
			lock (_weakFocusEnterMainFocusScopeHandlers)
			{
				_weakFocusEnterMainFocusScopeHandlers.Add(value);
			}
		}
		remove
		{
			lock (_weakFocusEnterMainFocusScopeHandlers)
			{
				_weakFocusEnterMainFocusScopeHandlers.Remove(value);
			}
		}
	}

	internal KeyboardNavigation()
	{
		InputManager current = InputManager.Current;
		current.PostProcessInput += PostProcessInput;
		current.TranslateAccelerator += TranslateAccelerator;
	}

	internal static DependencyObject GetTabOnceActiveElement(DependencyObject d)
	{
		WeakReference weakReference = (WeakReference)d.GetValue(TabOnceActiveElementProperty);
		if (weakReference != null && weakReference.IsAlive)
		{
			DependencyObject dependencyObject = weakReference.Target as DependencyObject;
			if (GetVisualRoot(dependencyObject) == GetVisualRoot(d))
			{
				return dependencyObject;
			}
			d.SetValue(TabOnceActiveElementProperty, null);
		}
		return null;
	}

	internal static void SetTabOnceActiveElement(DependencyObject d, DependencyObject value)
	{
		d.SetValue(TabOnceActiveElementProperty, new WeakReference(value));
	}

	private static DependencyObject GetControlTabOnceActiveElement(DependencyObject d)
	{
		WeakReference weakReference = (WeakReference)d.GetValue(ControlTabOnceActiveElementProperty);
		if (weakReference != null && weakReference.IsAlive)
		{
			DependencyObject dependencyObject = weakReference.Target as DependencyObject;
			if (GetVisualRoot(dependencyObject) == GetVisualRoot(d))
			{
				return dependencyObject;
			}
			d.SetValue(ControlTabOnceActiveElementProperty, null);
		}
		return null;
	}

	private static void SetControlTabOnceActiveElement(DependencyObject d, DependencyObject value)
	{
		d.SetValue(ControlTabOnceActiveElementProperty, new WeakReference(value));
	}

	private DependencyObject GetActiveElement(DependencyObject d)
	{
		if (_navigationProperty != ControlTabNavigationProperty)
		{
			return GetTabOnceActiveElement(d);
		}
		return GetControlTabOnceActiveElement(d);
	}

	private void SetActiveElement(DependencyObject d, DependencyObject value)
	{
		if (_navigationProperty == TabNavigationProperty)
		{
			SetTabOnceActiveElement(d, value);
		}
		else
		{
			SetControlTabOnceActiveElement(d, value);
		}
	}

	internal static Visual GetVisualRoot(DependencyObject d)
	{
		if (d is Visual || d is Visual3D)
		{
			PresentationSource presentationSource = PresentationSource.CriticalFromVisual(d);
			if (presentationSource != null)
			{
				return presentationSource.RootVisual;
			}
		}
		else if (d is FrameworkContentElement frameworkContentElement)
		{
			return GetVisualRoot(frameworkContentElement.Parent);
		}
		return null;
	}

	private static object CoerceShowKeyboardCues(DependencyObject d, object value)
	{
		if (!SystemParameters.KeyboardCues)
		{
			return value;
		}
		return BooleanBoxes.TrueBox;
	}

	internal void NotifyFocusChanged(object sender, KeyboardFocusChangedEventArgs e)
	{
		_weakFocusChangedHandlers.Process(delegate(object item)
		{
			if (item is KeyboardFocusChangedEventHandler keyboardFocusChangedEventHandler)
			{
				keyboardFocusChangedEventHandler(sender, e);
			}
			return false;
		});
	}

	/// <summary>Set the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.TabIndex" /> attached property for the specified element. </summary>
	/// <param name="element">The element on which to set the attached property to.</param>
	/// <param name="index">The property value to set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetTabIndex(DependencyObject element, int index)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TabIndexProperty, index);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.TabIndex" />  attached property for the specified element. </summary>
	/// <returns>The value of the <see cref="P:System.Windows.Input.KeyboardNavigation.TabIndex" /> property.</returns>
	/// <param name="element">The element from which to read the attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static int GetTabIndex(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return GetTabIndexHelper(element);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.IsTabStop" /> attached property for the specified element. </summary>
	/// <param name="element">The element to which to write the attached property.</param>
	/// <param name="isTabStop">The property value to set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetIsTabStop(DependencyObject element, bool isTabStop)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsTabStopProperty, BooleanBoxes.Box(isTabStop));
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.IsTabStop" /> attached property for the specified element. </summary>
	/// <returns>The value of the <see cref="P:System.Windows.Input.KeyboardNavigation.IsTabStop" /> property.</returns>
	/// <param name="element">The element from which to read the attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetIsTabStop(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsTabStopProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.TabNavigation" /> attached property for the specified element. </summary>
	/// <param name="element">Element on which to set the attached property.</param>
	/// <param name="mode">Property value to set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetTabNavigation(DependencyObject element, KeyboardNavigationMode mode)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(TabNavigationProperty, mode);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.TabNavigation" /> attached property for the specified element. </summary>
	/// <returns>The value of the <see cref="P:System.Windows.Input.KeyboardNavigation.TabNavigation" /> property.</returns>
	/// <param name="element">Element from which to get the attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	[CustomCategory("Accessibility")]
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static KeyboardNavigationMode GetTabNavigation(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (KeyboardNavigationMode)element.GetValue(TabNavigationProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.ControlTabNavigation" /> attached property for the specified element. </summary>
	/// <param name="element">Element on which to set the attached property.</param>
	/// <param name="mode">The property value to set</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetControlTabNavigation(DependencyObject element, KeyboardNavigationMode mode)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ControlTabNavigationProperty, mode);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.ControlTabNavigation" /> attached property for the specified element. </summary>
	/// <returns>The value of the <see cref="P:System.Windows.Input.KeyboardNavigation.ControlTabNavigation" /> property.</returns>
	/// <param name="element">Element from which to get the attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	[CustomCategory("Accessibility")]
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static KeyboardNavigationMode GetControlTabNavigation(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (KeyboardNavigationMode)element.GetValue(ControlTabNavigationProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.DirectionalNavigation" /> attached property for the specified element. </summary>
	/// <param name="element">Element on which to set the attached property.</param>
	/// <param name="mode">Property value to set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetDirectionalNavigation(DependencyObject element, KeyboardNavigationMode mode)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(DirectionalNavigationProperty, mode);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.DirectionalNavigation" /> attached property for the specified element. </summary>
	/// <returns>The value of the <see cref="P:System.Windows.Input.KeyboardNavigation.DirectionalNavigation" /> property.</returns>
	/// <param name="element">Element from which to get the attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	[CustomCategory("Accessibility")]
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static KeyboardNavigationMode GetDirectionalNavigation(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (KeyboardNavigationMode)element.GetValue(DirectionalNavigationProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.AcceptsReturn" />  attached property for the specified element. </summary>
	/// <param name="element">The element to write the attached property to.</param>
	/// <param name="enabled">The property value to set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static void SetAcceptsReturn(DependencyObject element, bool enabled)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(AcceptsReturnProperty, BooleanBoxes.Box(enabled));
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.AcceptsReturn" /> attached property for the specified element. </summary>
	/// <returns>The value of the <see cref="P:System.Windows.Input.KeyboardNavigation.AcceptsReturn" /> property.</returns>
	/// <param name="element">The element from which to read the attached property.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetAcceptsReturn(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(AcceptsReturnProperty);
	}

	private static bool IsValidKeyNavigationMode(object o)
	{
		KeyboardNavigationMode keyboardNavigationMode = (KeyboardNavigationMode)o;
		if (keyboardNavigationMode != KeyboardNavigationMode.Contained && keyboardNavigationMode != 0 && keyboardNavigationMode != KeyboardNavigationMode.Cycle && keyboardNavigationMode != KeyboardNavigationMode.None && keyboardNavigationMode != KeyboardNavigationMode.Once)
		{
			return keyboardNavigationMode == KeyboardNavigationMode.Local;
		}
		return true;
	}

	internal static UIElement GetParentUIElementFromContentElement(ContentElement ce)
	{
		IContentHost ichParent = null;
		return GetParentUIElementFromContentElement(ce, ref ichParent);
	}

	internal static UIElement GetParentUIElementFromContentElement(ContentElement ce, ref IContentHost ichParent)
	{
		if (ce == null)
		{
			return null;
		}
		IContentHost contentHost = ContentHostHelper.FindContentHost(ce);
		if (ichParent == null)
		{
			ichParent = contentHost;
		}
		if (contentHost is DependencyObject dependencyObject)
		{
			if (dependencyObject is UIElement result)
			{
				return result;
			}
			Visual visual = dependencyObject as Visual;
			while (visual != null)
			{
				visual = VisualTreeHelper.GetParent(visual) as Visual;
				if (visual is UIElement result2)
				{
					return result2;
				}
			}
			if (dependencyObject is ContentElement ce2)
			{
				return GetParentUIElementFromContentElement(ce2, ref ichParent);
			}
		}
		return null;
	}

	internal void HideFocusVisual()
	{
		if (_focusVisualAdornerCache != null)
		{
			if (VisualTreeHelper.GetParent(_focusVisualAdornerCache) is AdornerLayer adornerLayer)
			{
				adornerLayer.Remove(_focusVisualAdornerCache);
			}
			_focusVisualAdornerCache = null;
		}
	}

	internal static bool IsKeyboardMostRecentInputDevice()
	{
		return InputManager.Current.MostRecentInputDevice is KeyboardDevice;
	}

	internal static void ShowFocusVisual()
	{
		Current.ShowFocusVisual(Keyboard.FocusedElement as DependencyObject);
	}

	private void ShowFocusVisual(DependencyObject element)
	{
		HideFocusVisual();
		if (!IsKeyboardMostRecentInputDevice())
		{
			EnableKeyboardCues(element, enable: false);
		}
		if (!AlwaysShowFocusVisual && !IsKeyboardMostRecentInputDevice())
		{
			return;
		}
		if (element is FrameworkElement frameworkElement)
		{
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(frameworkElement);
			if (adornerLayer != null)
			{
				Style style = frameworkElement.FocusVisualStyle;
				if (style == FrameworkElement.DefaultFocusVisualStyle)
				{
					style = FrameworkElement.FindResourceInternal(frameworkElement, null, SystemParameters.FocusVisualStyleKey) as Style;
				}
				if (style != null)
				{
					_focusVisualAdornerCache = new FocusVisualAdorner(frameworkElement, style);
					adornerLayer.Add(_focusVisualAdornerCache);
				}
			}
		}
		else
		{
			if (!(element is FrameworkContentElement frameworkContentElement))
			{
				return;
			}
			IContentHost ichParent = null;
			UIElement parentUIElementFromContentElement = GetParentUIElementFromContentElement(frameworkContentElement, ref ichParent);
			if (ichParent == null || parentUIElementFromContentElement == null)
			{
				return;
			}
			AdornerLayer adornerLayer2 = AdornerLayer.GetAdornerLayer(parentUIElementFromContentElement);
			if (adornerLayer2 != null)
			{
				Style style2 = frameworkContentElement.FocusVisualStyle;
				if (style2 == FrameworkElement.DefaultFocusVisualStyle)
				{
					style2 = FrameworkElement.FindResourceInternal(null, frameworkContentElement, SystemParameters.FocusVisualStyleKey) as Style;
				}
				if (style2 != null)
				{
					_focusVisualAdornerCache = new FocusVisualAdorner(frameworkContentElement, parentUIElementFromContentElement, ichParent, style2);
					adornerLayer2.Add(_focusVisualAdornerCache);
				}
			}
		}
	}

	internal static void UpdateFocusedElement(DependencyObject focusTarget)
	{
		DependencyObject focusScope = FocusManager.GetFocusScope(focusTarget);
		if (focusScope != null && focusScope != focusTarget)
		{
			FocusManager.SetFocusedElement(focusScope, focusTarget as IInputElement);
			Visual visualRoot = GetVisualRoot(focusTarget);
			if (visualRoot != null && focusScope == visualRoot)
			{
				Current.NotifyFocusEnterMainFocusScope(visualRoot, EventArgs.Empty);
			}
		}
	}

	internal void UpdateActiveElement(DependencyObject activeElement)
	{
		UpdateActiveElement(activeElement, TabNavigationProperty);
		UpdateActiveElement(activeElement, ControlTabNavigationProperty);
	}

	private void UpdateActiveElement(DependencyObject activeElement, DependencyProperty dp)
	{
		_navigationProperty = dp;
		DependencyObject groupParent = GetGroupParent(activeElement);
		UpdateActiveElement(groupParent, activeElement, dp);
	}

	internal void UpdateActiveElement(DependencyObject container, DependencyObject activeElement)
	{
		UpdateActiveElement(container, activeElement, TabNavigationProperty);
		UpdateActiveElement(container, activeElement, ControlTabNavigationProperty);
	}

	private void UpdateActiveElement(DependencyObject container, DependencyObject activeElement, DependencyProperty dp)
	{
		_navigationProperty = dp;
		if (activeElement != container && GetKeyNavigationMode(container) == KeyboardNavigationMode.Once)
		{
			SetActiveElement(container, activeElement);
		}
	}

	internal bool Navigate(DependencyObject currentElement, TraversalRequest request)
	{
		return Navigate(currentElement, request, Keyboard.Modifiers);
	}

	private bool Navigate(DependencyObject currentElement, TraversalRequest request, ModifierKeys modifierKeys, bool fromProcessInputTabKey = false)
	{
		return Navigate(currentElement, request, modifierKeys, null, fromProcessInputTabKey);
	}

	private bool Navigate(DependencyObject currentElement, TraversalRequest request, ModifierKeys modifierKeys, DependencyObject firstElement, bool fromProcessInputTabKey = false)
	{
		DependencyObject dependencyObject = null;
		IKeyboardInputSink keyboardInputSink = null;
		switch (request.FocusNavigationDirection)
		{
		case FocusNavigationDirection.Next:
			_navigationProperty = (((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control) ? ControlTabNavigationProperty : TabNavigationProperty);
			dependencyObject = GetNextTab(currentElement, GetGroupParent(currentElement, includeCurrent: true), goDownOnly: false);
			break;
		case FocusNavigationDirection.Previous:
			_navigationProperty = (((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control) ? ControlTabNavigationProperty : TabNavigationProperty);
			dependencyObject = GetPrevTab(currentElement, null, goDownOnly: false);
			break;
		case FocusNavigationDirection.First:
			_navigationProperty = (((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control) ? ControlTabNavigationProperty : TabNavigationProperty);
			dependencyObject = GetNextTab(null, currentElement, goDownOnly: true);
			break;
		case FocusNavigationDirection.Last:
			_navigationProperty = (((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control) ? ControlTabNavigationProperty : TabNavigationProperty);
			dependencyObject = GetPrevTab(null, currentElement, goDownOnly: true);
			break;
		case FocusNavigationDirection.Left:
		case FocusNavigationDirection.Right:
		case FocusNavigationDirection.Up:
		case FocusNavigationDirection.Down:
			_navigationProperty = DirectionalNavigationProperty;
			dependencyObject = GetNextInDirection(currentElement, request.FocusNavigationDirection);
			break;
		}
		if (dependencyObject == null)
		{
			if (request.Wrapped || request.FocusNavigationDirection == FocusNavigationDirection.First || request.FocusNavigationDirection == FocusNavigationDirection.Last)
			{
				return false;
			}
			bool shouldCycle = true;
			if (NavigateOutsidePresentationSource(currentElement, request, fromProcessInputTabKey, ref shouldCycle))
			{
				return true;
			}
			if (shouldCycle && (request.FocusNavigationDirection == FocusNavigationDirection.Next || request.FocusNavigationDirection == FocusNavigationDirection.Previous))
			{
				Visual visualRoot = GetVisualRoot(currentElement);
				if (visualRoot != null)
				{
					return Navigate(visualRoot, new TraversalRequest((request.FocusNavigationDirection == FocusNavigationDirection.Next) ? FocusNavigationDirection.First : FocusNavigationDirection.Last));
				}
			}
			return false;
		}
		if (!(dependencyObject is IKeyboardInputSink keyboardInputSink2))
		{
			IInputElement obj = dependencyObject as IInputElement;
			obj.Focus();
			return obj.IsKeyboardFocusWithin;
		}
		bool flag = false;
		if (request.FocusNavigationDirection == FocusNavigationDirection.First || request.FocusNavigationDirection == FocusNavigationDirection.Next)
		{
			flag = keyboardInputSink2.TabInto(new TraversalRequest(FocusNavigationDirection.First));
		}
		else if (request.FocusNavigationDirection == FocusNavigationDirection.Last || request.FocusNavigationDirection == FocusNavigationDirection.Previous)
		{
			flag = keyboardInputSink2.TabInto(new TraversalRequest(FocusNavigationDirection.Last));
		}
		else
		{
			TraversalRequest traversalRequest = new TraversalRequest(request.FocusNavigationDirection);
			traversalRequest.Wrapped = true;
			flag = keyboardInputSink2.TabInto(traversalRequest);
		}
		if (!flag && firstElement != dependencyObject)
		{
			flag = Navigate(dependencyObject, request, modifierKeys, (firstElement == null) ? dependencyObject : firstElement);
		}
		return flag;
	}

	private bool NavigateOutsidePresentationSource(DependencyObject currentElement, TraversalRequest request, bool fromProcessInput, ref bool shouldCycle)
	{
		Visual visual = currentElement as Visual;
		if (visual == null)
		{
			visual = GetParentUIElementFromContentElement(currentElement as ContentElement);
			if (visual == null)
			{
				return false;
			}
		}
		if (PresentationSource.CriticalFromVisual(visual) is IKeyboardInputSink keyboardInputSink)
		{
			IKeyboardInputSite keyboardInputSite = null;
			keyboardInputSite = keyboardInputSink.KeyboardInputSite;
			if (keyboardInputSite != null && ShouldNavigateOutsidePresentationSource(currentElement, request))
			{
				if (!AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures)
				{
					IAvalonAdapter avalonAdapter = keyboardInputSite as IAvalonAdapter;
					if (avalonAdapter != null && fromProcessInput)
					{
						return avalonAdapter.OnNoMoreTabStops(request, ref shouldCycle);
					}
				}
				return keyboardInputSite.OnNoMoreTabStops(request);
			}
		}
		return false;
	}

	private bool ShouldNavigateOutsidePresentationSource(DependencyObject currentElement, TraversalRequest request)
	{
		if (request.FocusNavigationDirection == FocusNavigationDirection.Left || request.FocusNavigationDirection == FocusNavigationDirection.Right || request.FocusNavigationDirection == FocusNavigationDirection.Up || request.FocusNavigationDirection == FocusNavigationDirection.Down)
		{
			DependencyObject dependencyObject = null;
			while ((dependencyObject = GetGroupParent(currentElement)) != null && dependencyObject != currentElement)
			{
				KeyboardNavigationMode keyNavigationMode = GetKeyNavigationMode(dependencyObject);
				if (keyNavigationMode == KeyboardNavigationMode.Contained || keyNavigationMode == KeyboardNavigationMode.Cycle)
				{
					return false;
				}
				currentElement = dependencyObject;
			}
		}
		return true;
	}

	private void PostProcessInput(object sender, ProcessInputEventArgs e)
	{
		ProcessInput(e.StagingItem.Input);
	}

	private void TranslateAccelerator(object sender, KeyEventArgs e)
	{
		ProcessInput(e);
	}

	private void ProcessInput(InputEventArgs inputEventArgs)
	{
		ProcessForMenuMode(inputEventArgs);
		ProcessForUIState(inputEventArgs);
		if (inputEventArgs.RoutedEvent != Keyboard.KeyDownEvent)
		{
			return;
		}
		KeyEventArgs keyEventArgs = (KeyEventArgs)inputEventArgs;
		if (keyEventArgs.Handled)
		{
			return;
		}
		DependencyObject dependencyObject = keyEventArgs.OriginalSource as DependencyObject;
		if (keyEventArgs.KeyboardDevice.Target is DependencyObject dependencyObject2 && dependencyObject != dependencyObject2 && dependencyObject is HwndHost)
		{
			dependencyObject = dependencyObject2;
		}
		if (dependencyObject == null)
		{
			if (!(keyEventArgs.UnsafeInputSource is HwndSource hwndSource))
			{
				return;
			}
			dependencyObject = hwndSource.RootVisual;
			if (dependencyObject == null)
			{
				return;
			}
		}
		switch (GetRealKey(keyEventArgs))
		{
		case Key.LeftAlt:
		case Key.RightAlt:
			ShowFocusVisual();
			EnableKeyboardCues(dependencyObject, enable: true);
			break;
		case Key.Tab:
		case Key.Left:
		case Key.Up:
		case Key.Right:
		case Key.Down:
			ShowFocusVisual();
			break;
		}
		keyEventArgs.Handled = Navigate(dependencyObject, keyEventArgs.Key, keyEventArgs.KeyboardDevice.Modifiers, fromProcessInput: true);
	}

	internal static void EnableKeyboardCues(DependencyObject element, bool enable)
	{
		Visual visual = element as Visual;
		if (visual == null)
		{
			visual = GetParentUIElementFromContentElement(element as ContentElement);
			if (visual == null)
			{
				return;
			}
		}
		GetVisualRoot(visual)?.SetValue(ShowKeyboardCuesProperty, enable ? BooleanBoxes.TrueBox : BooleanBoxes.FalseBox);
	}

	internal static FocusNavigationDirection KeyToTraversalDirection(Key key)
	{
		return key switch
		{
			Key.Left => FocusNavigationDirection.Left, 
			Key.Right => FocusNavigationDirection.Right, 
			Key.Up => FocusNavigationDirection.Up, 
			Key.Down => FocusNavigationDirection.Down, 
			_ => throw new NotSupportedException(), 
		};
	}

	internal DependencyObject PredictFocusedElement(DependencyObject sourceElement, FocusNavigationDirection direction)
	{
		return PredictFocusedElement(sourceElement, direction, treeViewNavigation: false);
	}

	internal DependencyObject PredictFocusedElement(DependencyObject sourceElement, FocusNavigationDirection direction, bool treeViewNavigation)
	{
		return PredictFocusedElement(sourceElement, direction, treeViewNavigation, considerDescendants: true);
	}

	internal DependencyObject PredictFocusedElement(DependencyObject sourceElement, FocusNavigationDirection direction, bool treeViewNavigation, bool considerDescendants)
	{
		if (sourceElement == null)
		{
			return null;
		}
		_navigationProperty = DirectionalNavigationProperty;
		_verticalBaseline = double.MinValue;
		_horizontalBaseline = double.MinValue;
		return GetNextInDirection(sourceElement, direction, treeViewNavigation, considerDescendants);
	}

	internal DependencyObject PredictFocusedElementAtViewportEdge(DependencyObject sourceElement, FocusNavigationDirection direction, bool treeViewNavigation, FrameworkElement viewportBoundsElement, DependencyObject container)
	{
		try
		{
			_containerHashtable.Clear();
			return PredictFocusedElementAtViewportEdgeRecursive(sourceElement, direction, treeViewNavigation, viewportBoundsElement, container);
		}
		finally
		{
			_containerHashtable.Clear();
		}
	}

	private DependencyObject PredictFocusedElementAtViewportEdgeRecursive(DependencyObject sourceElement, FocusNavigationDirection direction, bool treeViewNavigation, FrameworkElement viewportBoundsElement, DependencyObject container)
	{
		_navigationProperty = DirectionalNavigationProperty;
		_verticalBaseline = double.MinValue;
		_horizontalBaseline = double.MinValue;
		if (container == null)
		{
			container = GetGroupParent(sourceElement);
		}
		if (container == sourceElement)
		{
			return null;
		}
		if (IsEndlessLoop(sourceElement, container))
		{
			return null;
		}
		DependencyObject dependencyObject = FindElementAtViewportEdge(sourceElement, viewportBoundsElement, container, direction, treeViewNavigation);
		if (dependencyObject != null)
		{
			if (IsElementEligible(dependencyObject, treeViewNavigation))
			{
				return dependencyObject;
			}
			DependencyObject sourceElement2 = dependencyObject;
			dependencyObject = PredictFocusedElementAtViewportEdgeRecursive(sourceElement, direction, treeViewNavigation, viewportBoundsElement, dependencyObject);
			if (dependencyObject != null)
			{
				return dependencyObject;
			}
			dependencyObject = PredictFocusedElementAtViewportEdgeRecursive(sourceElement2, direction, treeViewNavigation, viewportBoundsElement, null);
		}
		return dependencyObject;
	}

	internal bool Navigate(DependencyObject sourceElement, Key key, ModifierKeys modifiers, bool fromProcessInput = false)
	{
		bool result = false;
		switch (key)
		{
		case Key.Tab:
			result = Navigate(sourceElement, new TraversalRequest(((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next), modifiers, fromProcessInput);
			break;
		case Key.Right:
			result = Navigate(sourceElement, new TraversalRequest(FocusNavigationDirection.Right), modifiers);
			break;
		case Key.Left:
			result = Navigate(sourceElement, new TraversalRequest(FocusNavigationDirection.Left), modifiers);
			break;
		case Key.Up:
			result = Navigate(sourceElement, new TraversalRequest(FocusNavigationDirection.Up), modifiers);
			break;
		case Key.Down:
			result = Navigate(sourceElement, new TraversalRequest(FocusNavigationDirection.Down), modifiers);
			break;
		}
		return result;
	}

	private static bool IsInNavigationTree(DependencyObject visual)
	{
		if (visual is UIElement { IsVisible: not false })
		{
			return true;
		}
		if (visual is IContentHost && !(visual is UIElementIsland))
		{
			return true;
		}
		if (visual is UIElement3D { IsVisible: not false })
		{
			return true;
		}
		return false;
	}

	private DependencyObject GetPreviousSibling(DependencyObject e)
	{
		DependencyObject parent = GetParent(e);
		if (parent is IContentHost contentHost)
		{
			IInputElement inputElement = null;
			IEnumerator<IInputElement> hostedElements = contentHost.HostedElements;
			while (hostedElements.MoveNext())
			{
				IInputElement current = hostedElements.Current;
				if (current == e)
				{
					return inputElement as DependencyObject;
				}
				if (current is UIElement || current is UIElement3D)
				{
					inputElement = current;
				}
				else if (current is ContentElement e2 && IsTabStop(e2))
				{
					inputElement = current;
				}
			}
			return null;
		}
		DependencyObject dependencyObject = parent as UIElement;
		if (dependencyObject == null)
		{
			dependencyObject = parent as UIElement3D;
		}
		DependencyObject dependencyObject2 = e as Visual;
		if (dependencyObject2 == null)
		{
			dependencyObject2 = e as Visual3D;
		}
		if (dependencyObject != null && dependencyObject2 != null)
		{
			int childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
			DependencyObject result = null;
			for (int i = 0; i < childrenCount; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
				if (child == dependencyObject2)
				{
					break;
				}
				if (IsInNavigationTree(child))
				{
					result = child;
				}
			}
			return result;
		}
		return null;
	}

	private DependencyObject GetNextSibling(DependencyObject e)
	{
		DependencyObject parent = GetParent(e);
		if (parent is IContentHost { HostedElements: var hostedElements })
		{
			bool flag = false;
			while (hostedElements.MoveNext())
			{
				IInputElement current = hostedElements.Current;
				if (flag)
				{
					if (current is UIElement || current is UIElement3D)
					{
						return current as DependencyObject;
					}
					if (current is ContentElement contentElement && IsTabStop(contentElement))
					{
						return contentElement;
					}
				}
				else if (current == e)
				{
					flag = true;
				}
			}
		}
		else
		{
			DependencyObject dependencyObject = parent as UIElement;
			if (dependencyObject == null)
			{
				dependencyObject = parent as UIElement3D;
			}
			DependencyObject dependencyObject2 = e as Visual;
			if (dependencyObject2 == null)
			{
				dependencyObject2 = e as Visual3D;
			}
			if (dependencyObject != null && dependencyObject2 != null)
			{
				int childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
				int i;
				for (i = 0; i < childrenCount && VisualTreeHelper.GetChild(dependencyObject, i) != dependencyObject2; i++)
				{
				}
				for (i++; i < childrenCount; i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
					if (IsInNavigationTree(child))
					{
						return child;
					}
				}
			}
		}
		return null;
	}

	private DependencyObject FocusedElement(DependencyObject e)
	{
		if (e is IInputElement { IsKeyboardFocusWithin: false } && FocusManager.GetFocusedElement(e) is DependencyObject dependencyObject && (_navigationProperty == ControlTabNavigationProperty || !IsFocusScope(e)))
		{
			Visual visual = dependencyObject as Visual;
			if (visual == null)
			{
				if (!(dependencyObject is Visual3D visual3D))
				{
					visual = GetParentUIElementFromContentElement(dependencyObject as ContentElement);
				}
				else if (visual3D != e && visual3D.IsDescendantOf(e))
				{
					return dependencyObject;
				}
			}
			if (visual != null && visual != e && visual.IsDescendantOf(e))
			{
				return dependencyObject;
			}
		}
		return null;
	}

	private DependencyObject GetFirstChild(DependencyObject e)
	{
		DependencyObject dependencyObject = FocusedElement(e);
		if (dependencyObject != null)
		{
			return dependencyObject;
		}
		if (e is IContentHost { HostedElements: var hostedElements })
		{
			while (hostedElements.MoveNext())
			{
				IInputElement current = hostedElements.Current;
				if (current is UIElement || current is UIElement3D)
				{
					return current as DependencyObject;
				}
				if (current is ContentElement contentElement && IsTabStop(contentElement))
				{
					return contentElement;
				}
			}
			return null;
		}
		DependencyObject dependencyObject2 = e as UIElement;
		if (dependencyObject2 == null)
		{
			dependencyObject2 = e as UIElement3D;
		}
		if (dependencyObject2 == null || UIElementHelper.IsVisible(dependencyObject2))
		{
			DependencyObject dependencyObject3 = e as Visual;
			if (dependencyObject3 == null)
			{
				dependencyObject3 = e as Visual3D;
			}
			if (dependencyObject3 != null)
			{
				int childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject3);
				for (int i = 0; i < childrenCount; i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(dependencyObject3, i);
					if (IsInNavigationTree(child))
					{
						return child;
					}
					DependencyObject firstChild = GetFirstChild(child);
					if (firstChild != null)
					{
						return firstChild;
					}
				}
			}
		}
		return null;
	}

	private DependencyObject GetLastChild(DependencyObject e)
	{
		DependencyObject dependencyObject = FocusedElement(e);
		if (dependencyObject != null)
		{
			return dependencyObject;
		}
		if (e is IContentHost { HostedElements: var hostedElements })
		{
			IInputElement inputElement = null;
			while (hostedElements.MoveNext())
			{
				IInputElement current = hostedElements.Current;
				if (current is UIElement || current is UIElement3D)
				{
					inputElement = current;
				}
				else if (current is ContentElement e2 && IsTabStop(e2))
				{
					inputElement = current;
				}
			}
			return inputElement as DependencyObject;
		}
		DependencyObject dependencyObject2 = e as UIElement;
		if (dependencyObject2 == null)
		{
			dependencyObject2 = e as UIElement3D;
		}
		if (dependencyObject2 == null || UIElementHelper.IsVisible(dependencyObject2))
		{
			DependencyObject dependencyObject3 = e as Visual;
			if (dependencyObject3 == null)
			{
				dependencyObject3 = e as Visual3D;
			}
			if (dependencyObject3 != null)
			{
				for (int num = VisualTreeHelper.GetChildrenCount(dependencyObject3) - 1; num >= 0; num--)
				{
					DependencyObject child = VisualTreeHelper.GetChild(dependencyObject3, num);
					if (IsInNavigationTree(child))
					{
						return child;
					}
					DependencyObject lastChild = GetLastChild(child);
					if (lastChild != null)
					{
						return lastChild;
					}
				}
			}
		}
		return null;
	}

	internal static DependencyObject GetParent(DependencyObject e)
	{
		if (e is Visual || e is Visual3D)
		{
			DependencyObject dependencyObject = e;
			while ((dependencyObject = VisualTreeHelper.GetParent(dependencyObject)) != null)
			{
				if (IsInNavigationTree(dependencyObject))
				{
					return dependencyObject;
				}
			}
		}
		else if (e is ContentElement contentElement)
		{
			return ContentHostHelper.FindContentHost(contentElement) as DependencyObject;
		}
		return null;
	}

	private DependencyObject GetNextInTree(DependencyObject e, DependencyObject container)
	{
		DependencyObject dependencyObject = null;
		if (e == container || !IsGroup(e))
		{
			dependencyObject = GetFirstChild(e);
		}
		if (dependencyObject != null || e == container)
		{
			return dependencyObject;
		}
		DependencyObject dependencyObject2 = e;
		do
		{
			DependencyObject nextSibling = GetNextSibling(dependencyObject2);
			if (nextSibling != null)
			{
				return nextSibling;
			}
			dependencyObject2 = GetParent(dependencyObject2);
		}
		while (dependencyObject2 != null && dependencyObject2 != container);
		return null;
	}

	private DependencyObject GetPreviousInTree(DependencyObject e, DependencyObject container)
	{
		if (e == container)
		{
			return null;
		}
		DependencyObject previousSibling = GetPreviousSibling(e);
		if (previousSibling != null)
		{
			if (IsGroup(previousSibling))
			{
				return previousSibling;
			}
			return GetLastInTree(previousSibling);
		}
		return GetParent(e);
	}

	private DependencyObject GetLastInTree(DependencyObject container)
	{
		DependencyObject result;
		do
		{
			result = container;
			container = GetLastChild(container);
		}
		while (container != null && !IsGroup(container));
		if (container != null)
		{
			return container;
		}
		return result;
	}

	private DependencyObject GetGroupParent(DependencyObject e)
	{
		return GetGroupParent(e, includeCurrent: false);
	}

	private DependencyObject GetGroupParent(DependencyObject e, bool includeCurrent)
	{
		DependencyObject result = e;
		if (!includeCurrent)
		{
			result = e;
			e = GetParent(e);
			if (e == null)
			{
				return result;
			}
		}
		while (e != null)
		{
			if (IsGroup(e))
			{
				return e;
			}
			result = e;
			e = GetParent(e);
		}
		return result;
	}

	private bool IsTabStop(DependencyObject e)
	{
		if (e is FrameworkElement frameworkElement)
		{
			if (frameworkElement.Focusable && (bool)frameworkElement.GetValue(IsTabStopProperty) && frameworkElement.IsEnabled)
			{
				return frameworkElement.IsVisible;
			}
			return false;
		}
		if (e is FrameworkContentElement { Focusable: not false } frameworkContentElement && (bool)frameworkContentElement.GetValue(IsTabStopProperty))
		{
			return frameworkContentElement.IsEnabled;
		}
		return false;
	}

	private bool IsGroup(DependencyObject e)
	{
		return GetKeyNavigationMode(e) != KeyboardNavigationMode.Continue;
	}

	internal bool IsFocusableInternal(DependencyObject element)
	{
		if (element is UIElement uIElement)
		{
			if (uIElement.Focusable && uIElement.IsEnabled)
			{
				return uIElement.IsVisible;
			}
			return false;
		}
		if (element is ContentElement contentElement)
		{
			if (contentElement != null && contentElement.Focusable)
			{
				return contentElement.IsEnabled;
			}
			return false;
		}
		return false;
	}

	private bool IsElementEligible(DependencyObject element, bool treeViewNavigation)
	{
		if (treeViewNavigation)
		{
			if (element is TreeViewItem)
			{
				return IsFocusableInternal(element);
			}
			return false;
		}
		return IsTabStop(element);
	}

	private bool IsGroupElementEligible(DependencyObject element, bool treeViewNavigation)
	{
		if (treeViewNavigation)
		{
			if (element is TreeViewItem)
			{
				return IsFocusableInternal(element);
			}
			return false;
		}
		return IsTabStopOrGroup(element);
	}

	private KeyboardNavigationMode GetKeyNavigationMode(DependencyObject e)
	{
		return (KeyboardNavigationMode)e.GetValue(_navigationProperty);
	}

	private bool IsTabStopOrGroup(DependencyObject e)
	{
		if (!IsTabStop(e))
		{
			return IsGroup(e);
		}
		return true;
	}

	private static int GetTabIndexHelper(DependencyObject d)
	{
		return (int)d.GetValue(TabIndexProperty);
	}

	internal DependencyObject GetFirstTabInGroup(DependencyObject container)
	{
		DependencyObject dependencyObject = null;
		int num = int.MinValue;
		DependencyObject dependencyObject2 = container;
		while ((dependencyObject2 = GetNextInTree(dependencyObject2, container)) != null)
		{
			if (IsTabStopOrGroup(dependencyObject2))
			{
				int tabIndexHelper = GetTabIndexHelper(dependencyObject2);
				if (tabIndexHelper < num || dependencyObject == null)
				{
					num = tabIndexHelper;
					dependencyObject = dependencyObject2;
				}
			}
		}
		return dependencyObject;
	}

	private DependencyObject GetNextTabWithSameIndex(DependencyObject e, DependencyObject container)
	{
		int tabIndexHelper = GetTabIndexHelper(e);
		DependencyObject dependencyObject = e;
		while ((dependencyObject = GetNextInTree(dependencyObject, container)) != null)
		{
			if (IsTabStopOrGroup(dependencyObject) && GetTabIndexHelper(dependencyObject) == tabIndexHelper)
			{
				return dependencyObject;
			}
		}
		return null;
	}

	private DependencyObject GetNextTabWithNextIndex(DependencyObject e, DependencyObject container, KeyboardNavigationMode tabbingType)
	{
		DependencyObject dependencyObject = null;
		DependencyObject dependencyObject2 = null;
		int num = int.MinValue;
		int num2 = int.MinValue;
		int tabIndexHelper = GetTabIndexHelper(e);
		DependencyObject dependencyObject3 = container;
		while ((dependencyObject3 = GetNextInTree(dependencyObject3, container)) != null)
		{
			if (IsTabStopOrGroup(dependencyObject3))
			{
				int tabIndexHelper2 = GetTabIndexHelper(dependencyObject3);
				if (tabIndexHelper2 > tabIndexHelper && (tabIndexHelper2 < num2 || dependencyObject == null))
				{
					num2 = tabIndexHelper2;
					dependencyObject = dependencyObject3;
				}
				if (tabIndexHelper2 < num || dependencyObject2 == null)
				{
					num = tabIndexHelper2;
					dependencyObject2 = dependencyObject3;
				}
			}
		}
		if (tabbingType == KeyboardNavigationMode.Cycle && dependencyObject == null)
		{
			dependencyObject = dependencyObject2;
		}
		return dependencyObject;
	}

	private DependencyObject GetNextTabInGroup(DependencyObject e, DependencyObject container, KeyboardNavigationMode tabbingType)
	{
		if (tabbingType == KeyboardNavigationMode.None)
		{
			return null;
		}
		if (e == null || e == container)
		{
			return GetFirstTabInGroup(container);
		}
		if (tabbingType == KeyboardNavigationMode.Once)
		{
			return null;
		}
		DependencyObject nextTabWithSameIndex = GetNextTabWithSameIndex(e, container);
		if (nextTabWithSameIndex != null)
		{
			return nextTabWithSameIndex;
		}
		return GetNextTabWithNextIndex(e, container, tabbingType);
	}

	private DependencyObject GetNextTab(DependencyObject e, DependencyObject container, bool goDownOnly)
	{
		KeyboardNavigationMode keyNavigationMode = GetKeyNavigationMode(container);
		if (e == null)
		{
			if (IsTabStop(container))
			{
				return container;
			}
			DependencyObject activeElement = GetActiveElement(container);
			if (activeElement != null)
			{
				return GetNextTab(null, activeElement, goDownOnly: true);
			}
		}
		else if ((keyNavigationMode == KeyboardNavigationMode.Once || keyNavigationMode == KeyboardNavigationMode.None) && container != e)
		{
			if (goDownOnly)
			{
				return null;
			}
			DependencyObject groupParent = GetGroupParent(container);
			return GetNextTab(container, groupParent, goDownOnly);
		}
		DependencyObject dependencyObject = null;
		DependencyObject dependencyObject2 = e;
		KeyboardNavigationMode keyboardNavigationMode = keyNavigationMode;
		while ((dependencyObject2 = GetNextTabInGroup(dependencyObject2, container, keyboardNavigationMode)) != null && dependencyObject != dependencyObject2)
		{
			if (dependencyObject == null)
			{
				dependencyObject = dependencyObject2;
			}
			DependencyObject nextTab = GetNextTab(null, dependencyObject2, goDownOnly: true);
			if (nextTab != null)
			{
				return nextTab;
			}
			if (keyboardNavigationMode == KeyboardNavigationMode.Once)
			{
				keyboardNavigationMode = KeyboardNavigationMode.Contained;
			}
		}
		if (!goDownOnly && keyboardNavigationMode != KeyboardNavigationMode.Contained && GetParent(container) != null)
		{
			return GetNextTab(container, GetGroupParent(container), goDownOnly: false);
		}
		return null;
	}

	internal DependencyObject GetLastTabInGroup(DependencyObject container)
	{
		DependencyObject dependencyObject = null;
		int num = int.MaxValue;
		DependencyObject dependencyObject2 = GetLastInTree(container);
		while (dependencyObject2 != null && dependencyObject2 != container)
		{
			if (IsTabStopOrGroup(dependencyObject2))
			{
				int tabIndexHelper = GetTabIndexHelper(dependencyObject2);
				if (tabIndexHelper > num || dependencyObject == null)
				{
					num = tabIndexHelper;
					dependencyObject = dependencyObject2;
				}
			}
			dependencyObject2 = GetPreviousInTree(dependencyObject2, container);
		}
		return dependencyObject;
	}

	private DependencyObject GetPrevTabWithSameIndex(DependencyObject e, DependencyObject container)
	{
		int tabIndexHelper = GetTabIndexHelper(e);
		for (DependencyObject previousInTree = GetPreviousInTree(e, container); previousInTree != null; previousInTree = GetPreviousInTree(previousInTree, container))
		{
			if (IsTabStopOrGroup(previousInTree) && GetTabIndexHelper(previousInTree) == tabIndexHelper && previousInTree != container)
			{
				return previousInTree;
			}
		}
		return null;
	}

	private DependencyObject GetPrevTabWithPrevIndex(DependencyObject e, DependencyObject container, KeyboardNavigationMode tabbingType)
	{
		DependencyObject dependencyObject = null;
		DependencyObject dependencyObject2 = null;
		int tabIndexHelper = GetTabIndexHelper(e);
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		for (DependencyObject dependencyObject3 = GetLastInTree(container); dependencyObject3 != null; dependencyObject3 = GetPreviousInTree(dependencyObject3, container))
		{
			if (IsTabStopOrGroup(dependencyObject3) && dependencyObject3 != container)
			{
				int tabIndexHelper2 = GetTabIndexHelper(dependencyObject3);
				if (tabIndexHelper2 < tabIndexHelper && (tabIndexHelper2 > num2 || dependencyObject2 == null))
				{
					num2 = tabIndexHelper2;
					dependencyObject2 = dependencyObject3;
				}
				if (tabIndexHelper2 > num || dependencyObject == null)
				{
					num = tabIndexHelper2;
					dependencyObject = dependencyObject3;
				}
			}
		}
		if (tabbingType == KeyboardNavigationMode.Cycle && dependencyObject2 == null)
		{
			dependencyObject2 = dependencyObject;
		}
		return dependencyObject2;
	}

	private DependencyObject GetPrevTabInGroup(DependencyObject e, DependencyObject container, KeyboardNavigationMode tabbingType)
	{
		if (tabbingType == KeyboardNavigationMode.None)
		{
			return null;
		}
		if (e == null)
		{
			return GetLastTabInGroup(container);
		}
		if (tabbingType == KeyboardNavigationMode.Once)
		{
			return null;
		}
		if (e == container)
		{
			return null;
		}
		DependencyObject prevTabWithSameIndex = GetPrevTabWithSameIndex(e, container);
		if (prevTabWithSameIndex != null)
		{
			return prevTabWithSameIndex;
		}
		return GetPrevTabWithPrevIndex(e, container, tabbingType);
	}

	private DependencyObject GetPrevTab(DependencyObject e, DependencyObject container, bool goDownOnly)
	{
		if (container == null)
		{
			container = GetGroupParent(e);
		}
		KeyboardNavigationMode keyNavigationMode = GetKeyNavigationMode(container);
		if (e == null)
		{
			DependencyObject activeElement = GetActiveElement(container);
			if (activeElement != null)
			{
				return GetPrevTab(null, activeElement, goDownOnly: true);
			}
			if (keyNavigationMode == KeyboardNavigationMode.Once)
			{
				DependencyObject nextTabInGroup = GetNextTabInGroup(null, container, keyNavigationMode);
				if (nextTabInGroup == null)
				{
					if (IsTabStop(container))
					{
						return container;
					}
					if (goDownOnly)
					{
						return null;
					}
					return GetPrevTab(container, null, goDownOnly: false);
				}
				return GetPrevTab(null, nextTabInGroup, goDownOnly: true);
			}
		}
		else if (keyNavigationMode == KeyboardNavigationMode.Once || keyNavigationMode == KeyboardNavigationMode.None)
		{
			if (goDownOnly || container == e)
			{
				return null;
			}
			if (IsTabStop(container))
			{
				return container;
			}
			return GetPrevTab(container, null, goDownOnly: false);
		}
		DependencyObject dependencyObject = null;
		DependencyObject dependencyObject2 = e;
		while ((dependencyObject2 = GetPrevTabInGroup(dependencyObject2, container, keyNavigationMode)) != null && (dependencyObject2 != container || keyNavigationMode != KeyboardNavigationMode.Local))
		{
			if (IsTabStop(dependencyObject2) && !IsGroup(dependencyObject2))
			{
				return dependencyObject2;
			}
			if (dependencyObject == dependencyObject2)
			{
				break;
			}
			if (dependencyObject == null)
			{
				dependencyObject = dependencyObject2;
			}
			DependencyObject prevTab = GetPrevTab(null, dependencyObject2, goDownOnly: true);
			if (prevTab != null)
			{
				return prevTab;
			}
		}
		if (keyNavigationMode == KeyboardNavigationMode.Contained)
		{
			return null;
		}
		if (e != container && IsTabStop(container))
		{
			return container;
		}
		if (!goDownOnly && GetParent(container) != null)
		{
			return GetPrevTab(container, null, goDownOnly: false);
		}
		return null;
	}

	internal static Rect GetRectangle(DependencyObject element)
	{
		if (element is UIElement uIElement)
		{
			if (!uIElement.IsArrangeValid)
			{
				uIElement.UpdateLayout();
			}
			Visual visualRoot = GetVisualRoot(uIElement);
			if (visualRoot != null)
			{
				GeneralTransform generalTransform = uIElement.TransformToAncestor(visualRoot);
				Thickness thickness = (Thickness)uIElement.GetValue(DirectionalNavigationMarginProperty);
				double x = 0.0 - thickness.Left;
				double y = 0.0 - thickness.Top;
				double num = uIElement.RenderSize.Width + thickness.Left + thickness.Right;
				double num2 = uIElement.RenderSize.Height + thickness.Top + thickness.Bottom;
				if (num < 0.0)
				{
					x = uIElement.RenderSize.Width * 0.5;
					num = 0.0;
				}
				if (num2 < 0.0)
				{
					y = uIElement.RenderSize.Height * 0.5;
					num2 = 0.0;
				}
				return generalTransform.TransformBounds(new Rect(x, y, num, num2));
			}
		}
		else if (element is ContentElement contentElement)
		{
			IContentHost ichParent = null;
			UIElement parentUIElementFromContentElement = GetParentUIElementFromContentElement(contentElement, ref ichParent);
			Visual visual = ichParent as Visual;
			if (ichParent != null && visual != null && parentUIElementFromContentElement != null)
			{
				Visual visualRoot2 = GetVisualRoot(visual);
				if (visualRoot2 != null)
				{
					if (!parentUIElementFromContentElement.IsMeasureValid)
					{
						parentUIElementFromContentElement.UpdateLayout();
					}
					IEnumerator<Rect> enumerator = ichParent.GetRectangles(contentElement).GetEnumerator();
					if (enumerator.MoveNext())
					{
						GeneralTransform generalTransform2 = visual.TransformToAncestor(visualRoot2);
						Rect current = enumerator.Current;
						return generalTransform2.TransformBounds(current);
					}
				}
			}
		}
		else if (element is UIElement3D uIElement3D)
		{
			Visual visualRoot3 = GetVisualRoot(uIElement3D);
			Visual containingVisual2D = VisualTreeHelper.GetContainingVisual2D(uIElement3D);
			if (visualRoot3 != null && containingVisual2D != null)
			{
				Rect visual2DContentBounds = uIElement3D.Visual2DContentBounds;
				return containingVisual2D.TransformToAncestor(visualRoot3).TransformBounds(visual2DContentBounds);
			}
		}
		return Rect.Empty;
	}

	private Rect GetRepresentativeRectangle(DependencyObject element)
	{
		Rect rectangle = GetRectangle(element);
		if (element is TreeViewItem { ItemsHost: { IsVisible: not false } itemsHost } treeViewItem)
		{
			Rect rectangle2 = GetRectangle(itemsHost);
			if (rectangle2 != Rect.Empty)
			{
				bool? flag = null;
				FrameworkElement frameworkElement = treeViewItem.TryGetHeaderElement();
				if (frameworkElement != null && frameworkElement != treeViewItem && frameworkElement.IsVisible)
				{
					Rect rectangle3 = GetRectangle(frameworkElement);
					if (!rectangle3.IsEmpty)
					{
						if (DoubleUtil.LessThan(rectangle3.Top, rectangle2.Top))
						{
							flag = true;
						}
						else if (DoubleUtil.GreaterThan(rectangle3.Bottom, rectangle2.Bottom))
						{
							flag = false;
						}
					}
				}
				double num = rectangle2.Top - rectangle.Top;
				double num2 = rectangle.Bottom - rectangle2.Bottom;
				if (!flag.HasValue)
				{
					flag = DoubleUtil.GreaterThanOrClose(num, num2);
				}
				if (flag == true)
				{
					rectangle.Height = Math.Min(Math.Max(num, 0.0), rectangle.Height);
				}
				else
				{
					double num3 = Math.Min(Math.Max(num2, 0.0), rectangle.Height);
					rectangle.Y = rectangle.Bottom - num3;
					rectangle.Height = num3;
				}
			}
		}
		return rectangle;
	}

	private double GetDistance(Point p1, Point p2)
	{
		double num = p1.X - p2.X;
		double num2 = p1.Y - p2.Y;
		return Math.Sqrt(num * num + num2 * num2);
	}

	private double GetPerpDistance(Rect sourceRect, Rect targetRect, FocusNavigationDirection direction)
	{
		return direction switch
		{
			FocusNavigationDirection.Right => targetRect.Left - sourceRect.Left, 
			FocusNavigationDirection.Left => sourceRect.Right - targetRect.Right, 
			FocusNavigationDirection.Up => sourceRect.Bottom - targetRect.Bottom, 
			FocusNavigationDirection.Down => targetRect.Top - sourceRect.Top, 
			_ => throw new InvalidEnumArgumentException("direction", (int)direction, typeof(FocusNavigationDirection)), 
		};
	}

	private double GetDistance(Rect sourceRect, Rect targetRect, FocusNavigationDirection direction)
	{
		Point p;
		Point p2;
		switch (direction)
		{
		case FocusNavigationDirection.Right:
			p = sourceRect.TopLeft;
			if (_horizontalBaseline != double.MinValue)
			{
				p.Y = _horizontalBaseline;
			}
			p2 = targetRect.TopLeft;
			break;
		case FocusNavigationDirection.Left:
			p = sourceRect.TopRight;
			if (_horizontalBaseline != double.MinValue)
			{
				p.Y = _horizontalBaseline;
			}
			p2 = targetRect.TopRight;
			break;
		case FocusNavigationDirection.Up:
			p = sourceRect.BottomLeft;
			if (_verticalBaseline != double.MinValue)
			{
				p.X = _verticalBaseline;
			}
			p2 = targetRect.BottomLeft;
			break;
		case FocusNavigationDirection.Down:
			p = sourceRect.TopLeft;
			if (_verticalBaseline != double.MinValue)
			{
				p.X = _verticalBaseline;
			}
			p2 = targetRect.TopLeft;
			break;
		default:
			throw new InvalidEnumArgumentException("direction", (int)direction, typeof(FocusNavigationDirection));
		}
		return GetDistance(p, p2);
	}

	private bool IsInDirection(Rect fromRect, Rect toRect, FocusNavigationDirection direction)
	{
		return direction switch
		{
			FocusNavigationDirection.Right => DoubleUtil.LessThanOrClose(fromRect.Right, toRect.Left), 
			FocusNavigationDirection.Left => DoubleUtil.GreaterThanOrClose(fromRect.Left, toRect.Right), 
			FocusNavigationDirection.Up => DoubleUtil.GreaterThanOrClose(fromRect.Top, toRect.Bottom), 
			FocusNavigationDirection.Down => DoubleUtil.LessThanOrClose(fromRect.Bottom, toRect.Top), 
			_ => throw new InvalidEnumArgumentException("direction", (int)direction, typeof(FocusNavigationDirection)), 
		};
	}

	private bool IsFocusScope(DependencyObject e)
	{
		if (!FocusManager.GetIsFocusScope(e))
		{
			return GetParent(e) == null;
		}
		return true;
	}

	private bool IsAncestorOf(DependencyObject sourceElement, DependencyObject targetElement)
	{
		Visual visual = sourceElement as Visual;
		Visual visual2 = targetElement as Visual;
		if (visual == null || visual2 == null)
		{
			return false;
		}
		return visual.IsAncestorOf(visual2);
	}

	internal bool IsAncestorOfEx(DependencyObject sourceElement, DependencyObject targetElement)
	{
		while (targetElement != null && targetElement != sourceElement)
		{
			targetElement = GetParent(targetElement);
		}
		return targetElement == sourceElement;
	}

	private bool IsInRange(DependencyObject sourceElement, DependencyObject targetElement, Rect sourceRect, Rect targetRect, FocusNavigationDirection direction, double startRange, double endRange)
	{
		switch (direction)
		{
		case FocusNavigationDirection.Left:
		case FocusNavigationDirection.Right:
			if (_horizontalBaseline != double.MinValue)
			{
				startRange = Math.Min(startRange, _horizontalBaseline);
				endRange = Math.Max(endRange, _horizontalBaseline);
			}
			if (!DoubleUtil.GreaterThan(targetRect.Bottom, startRange) || !DoubleUtil.LessThan(targetRect.Top, endRange))
			{
				break;
			}
			if (sourceElement == null)
			{
				return true;
			}
			if (direction == FocusNavigationDirection.Right)
			{
				if (!DoubleUtil.GreaterThan(targetRect.Left, sourceRect.Left))
				{
					if (DoubleUtil.AreClose(targetRect.Left, sourceRect.Left))
					{
						return IsAncestorOfEx(sourceElement, targetElement);
					}
					return false;
				}
				return true;
			}
			if (!DoubleUtil.LessThan(targetRect.Right, sourceRect.Right))
			{
				if (DoubleUtil.AreClose(targetRect.Right, sourceRect.Right))
				{
					return IsAncestorOfEx(sourceElement, targetElement);
				}
				return false;
			}
			return true;
		case FocusNavigationDirection.Up:
		case FocusNavigationDirection.Down:
			if (_verticalBaseline != double.MinValue)
			{
				startRange = Math.Min(startRange, _verticalBaseline);
				endRange = Math.Max(endRange, _verticalBaseline);
			}
			if (!DoubleUtil.GreaterThan(targetRect.Right, startRange) || !DoubleUtil.LessThan(targetRect.Left, endRange))
			{
				break;
			}
			if (sourceElement == null)
			{
				return true;
			}
			if (direction == FocusNavigationDirection.Down)
			{
				if (!DoubleUtil.GreaterThan(targetRect.Top, sourceRect.Top))
				{
					if (DoubleUtil.AreClose(targetRect.Top, sourceRect.Top))
					{
						return IsAncestorOfEx(sourceElement, targetElement);
					}
					return false;
				}
				return true;
			}
			if (!DoubleUtil.LessThan(targetRect.Bottom, sourceRect.Bottom))
			{
				if (DoubleUtil.AreClose(targetRect.Bottom, sourceRect.Bottom))
				{
					return IsAncestorOfEx(sourceElement, targetElement);
				}
				return false;
			}
			return true;
		default:
			throw new InvalidEnumArgumentException("direction", (int)direction, typeof(FocusNavigationDirection));
		}
		return false;
	}

	private DependencyObject GetNextInDirection(DependencyObject sourceElement, FocusNavigationDirection direction)
	{
		return GetNextInDirection(sourceElement, direction, treeViewNavigation: false);
	}

	private DependencyObject GetNextInDirection(DependencyObject sourceElement, FocusNavigationDirection direction, bool treeViewNavigation)
	{
		return GetNextInDirection(sourceElement, direction, treeViewNavigation, considerDescendants: true);
	}

	private DependencyObject GetNextInDirection(DependencyObject sourceElement, FocusNavigationDirection direction, bool treeViewNavigation, bool considerDescendants)
	{
		_containerHashtable.Clear();
		DependencyObject dependencyObject = MoveNext(sourceElement, null, direction, double.MinValue, double.MinValue, treeViewNavigation, considerDescendants);
		if (dependencyObject != null)
		{
			if (sourceElement is UIElement uIElement)
			{
				uIElement.RemoveHandler(Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(_LostFocus));
			}
			else if (sourceElement is ContentElement contentElement)
			{
				contentElement.RemoveHandler(Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(_LostFocus));
			}
			UIElement uIElement2 = dependencyObject as UIElement;
			if (uIElement2 == null)
			{
				uIElement2 = GetParentUIElementFromContentElement(dependencyObject as ContentElement);
			}
			else if (dependencyObject is ContentElement contentElement2)
			{
				contentElement2.AddHandler(Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(_LostFocus), handledEventsToo: true);
			}
			if (uIElement2 != null)
			{
				uIElement2.LayoutUpdated += OnLayoutUpdated;
				if (dependencyObject == uIElement2)
				{
					uIElement2.AddHandler(Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(_LostFocus), handledEventsToo: true);
				}
			}
		}
		_containerHashtable.Clear();
		return dependencyObject;
	}

	private void OnLayoutUpdated(object sender, EventArgs e)
	{
		if (sender is UIElement uIElement)
		{
			uIElement.LayoutUpdated -= OnLayoutUpdated;
		}
		_verticalBaseline = double.MinValue;
		_horizontalBaseline = double.MinValue;
	}

	private void _LostFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		_verticalBaseline = double.MinValue;
		_horizontalBaseline = double.MinValue;
		if (sender is UIElement)
		{
			((UIElement)sender).RemoveHandler(Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(_LostFocus));
		}
		else if (sender is ContentElement)
		{
			((ContentElement)sender).RemoveHandler(Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(_LostFocus));
		}
	}

	private bool IsEndlessLoop(DependencyObject element, DependencyObject container)
	{
		object key = ((element != null) ? element : _fakeNull);
		Hashtable hashtable = _containerHashtable[container] as Hashtable;
		if (hashtable != null)
		{
			if (hashtable[key] != null)
			{
				return true;
			}
		}
		else
		{
			hashtable = new Hashtable(10);
			_containerHashtable[container] = hashtable;
		}
		hashtable[key] = BooleanBoxes.TrueBox;
		return false;
	}

	private void ResetBaseLines(double value, bool horizontalDirection)
	{
		if (horizontalDirection)
		{
			_verticalBaseline = double.MinValue;
			if (_horizontalBaseline == double.MinValue)
			{
				_horizontalBaseline = value;
			}
		}
		else
		{
			_horizontalBaseline = double.MinValue;
			if (_verticalBaseline == double.MinValue)
			{
				_verticalBaseline = value;
			}
		}
	}

	private DependencyObject FindNextInDirection(DependencyObject sourceElement, Rect sourceRect, DependencyObject container, FocusNavigationDirection direction, double startRange, double endRange, bool treeViewNavigation, bool considerDescendants)
	{
		DependencyObject dependencyObject = null;
		Rect targetRect = Rect.Empty;
		double value = 0.0;
		bool flag = sourceElement == null;
		DependencyObject dependencyObject2 = container;
		while ((dependencyObject2 = GetNextInTree(dependencyObject2, container)) != null)
		{
			if (dependencyObject2 == sourceElement || !IsGroupElementEligible(dependencyObject2, treeViewNavigation))
			{
				continue;
			}
			Rect representativeRectangle = GetRepresentativeRectangle(dependencyObject2);
			if (!(representativeRectangle != Rect.Empty))
			{
				continue;
			}
			bool flag2 = IsInDirection(sourceRect, representativeRectangle, direction);
			bool flag3 = IsInRange(sourceElement, dependencyObject2, sourceRect, representativeRectangle, direction, startRange, endRange);
			if (!(flag || flag2 || flag3))
			{
				continue;
			}
			double num = (flag3 ? GetPerpDistance(sourceRect, representativeRectangle, direction) : GetDistance(sourceRect, representativeRectangle, direction));
			if (!double.IsNaN(num))
			{
				if (dependencyObject == null && (considerDescendants || !IsAncestorOfEx(sourceElement, dependencyObject2)))
				{
					dependencyObject = dependencyObject2;
					targetRect = representativeRectangle;
					value = num;
				}
				else if ((DoubleUtil.LessThan(num, value) || (DoubleUtil.AreClose(num, value) && GetDistance(sourceRect, targetRect, direction) > GetDistance(sourceRect, representativeRectangle, direction))) && (considerDescendants || !IsAncestorOfEx(sourceElement, dependencyObject2)))
				{
					dependencyObject = dependencyObject2;
					targetRect = representativeRectangle;
					value = num;
				}
			}
		}
		return dependencyObject;
	}

	private DependencyObject MoveNext(DependencyObject sourceElement, DependencyObject container, FocusNavigationDirection direction, double startRange, double endRange, bool treeViewNavigation, bool considerDescendants)
	{
		if (container == null)
		{
			container = GetGroupParent(sourceElement);
		}
		if (container == sourceElement)
		{
			return null;
		}
		if (IsEndlessLoop(sourceElement, container))
		{
			return null;
		}
		KeyboardNavigationMode keyNavigationMode = GetKeyNavigationMode(container);
		bool flag = sourceElement == null;
		if (keyNavigationMode == KeyboardNavigationMode.None && flag)
		{
			return null;
		}
		Rect sourceRect = (flag ? GetRectangle(container) : GetRepresentativeRectangle(sourceElement));
		bool flag2 = direction == FocusNavigationDirection.Right || direction == FocusNavigationDirection.Left;
		ResetBaseLines(flag2 ? sourceRect.Top : sourceRect.Left, flag2);
		if (startRange == double.MinValue || endRange == double.MinValue)
		{
			startRange = (flag2 ? sourceRect.Top : sourceRect.Left);
			endRange = (flag2 ? sourceRect.Bottom : sourceRect.Right);
		}
		if (keyNavigationMode == KeyboardNavigationMode.Once && !flag)
		{
			return MoveNext(container, null, direction, startRange, endRange, treeViewNavigation, considerDescendants: true);
		}
		DependencyObject dependencyObject = FindNextInDirection(sourceElement, sourceRect, container, direction, startRange, endRange, treeViewNavigation, considerDescendants);
		if (dependencyObject == null)
		{
			return keyNavigationMode switch
			{
				KeyboardNavigationMode.Cycle => MoveNext(null, container, direction, startRange, endRange, treeViewNavigation, considerDescendants: true), 
				KeyboardNavigationMode.Contained => null, 
				_ => MoveNext(container, null, direction, startRange, endRange, treeViewNavigation, considerDescendants: true), 
			};
		}
		if (IsElementEligible(dependencyObject, treeViewNavigation))
		{
			return dependencyObject;
		}
		DependencyObject activeElementChain = GetActiveElementChain(dependencyObject, treeViewNavigation);
		if (activeElementChain != null)
		{
			return activeElementChain;
		}
		DependencyObject dependencyObject2 = MoveNext(null, dependencyObject, direction, startRange, endRange, treeViewNavigation, considerDescendants: true);
		if (dependencyObject2 != null)
		{
			return dependencyObject2;
		}
		return MoveNext(dependencyObject, null, direction, startRange, endRange, treeViewNavigation, considerDescendants: true);
	}

	private DependencyObject GetActiveElementChain(DependencyObject element, bool treeViewNavigation)
	{
		DependencyObject result = null;
		DependencyObject dependencyObject = element;
		while ((dependencyObject = GetActiveElement(dependencyObject)) != null)
		{
			if (IsElementEligible(dependencyObject, treeViewNavigation))
			{
				result = dependencyObject;
			}
		}
		return result;
	}

	private DependencyObject FindElementAtViewportEdge(DependencyObject sourceElement, FrameworkElement viewportBoundsElement, DependencyObject container, FocusNavigationDirection direction, bool treeViewNavigation)
	{
		Rect elementRect = new Rect(0.0, 0.0, 0.0, 0.0);
		if (sourceElement != null && ItemsControl.GetElementViewportPosition(viewportBoundsElement, ItemsControl.TryGetTreeViewItemHeader(sourceElement) as UIElement, direction, fullyVisible: false, out elementRect) == ElementViewportPosition.None)
		{
			elementRect = new Rect(0.0, 0.0, 0.0, 0.0);
		}
		DependencyObject dependencyObject = null;
		double value = double.NegativeInfinity;
		double value2 = double.NegativeInfinity;
		DependencyObject dependencyObject2 = null;
		double value3 = double.NegativeInfinity;
		double value4 = double.NegativeInfinity;
		DependencyObject dependencyObject3 = container;
		while ((dependencyObject3 = GetNextInTree(dependencyObject3, container)) != null)
		{
			if (!IsGroupElementEligible(dependencyObject3, treeViewNavigation))
			{
				continue;
			}
			DependencyObject dependencyObject4 = dependencyObject3;
			if (treeViewNavigation)
			{
				dependencyObject4 = ItemsControl.TryGetTreeViewItemHeader(dependencyObject3);
			}
			Rect elementRect2;
			ElementViewportPosition elementViewportPosition = ItemsControl.GetElementViewportPosition(viewportBoundsElement, dependencyObject4 as UIElement, direction, fullyVisible: false, out elementRect2);
			if (elementViewportPosition != ElementViewportPosition.CompletelyInViewport && elementViewportPosition != ElementViewportPosition.PartiallyInViewport)
			{
				continue;
			}
			double num = double.NegativeInfinity;
			switch (direction)
			{
			case FocusNavigationDirection.Up:
				num = 0.0 - elementRect2.Top;
				break;
			case FocusNavigationDirection.Down:
				num = elementRect2.Bottom;
				break;
			case FocusNavigationDirection.Left:
				num = 0.0 - elementRect2.Left;
				break;
			case FocusNavigationDirection.Right:
				num = elementRect2.Right;
				break;
			}
			double num2 = double.NegativeInfinity;
			switch (direction)
			{
			case FocusNavigationDirection.Up:
			case FocusNavigationDirection.Down:
				num2 = ComputeRangeScore(elementRect.Left, elementRect.Right, elementRect2.Left, elementRect2.Right);
				break;
			case FocusNavigationDirection.Left:
			case FocusNavigationDirection.Right:
				num2 = ComputeRangeScore(elementRect.Top, elementRect.Bottom, elementRect2.Top, elementRect2.Bottom);
				break;
			}
			if (elementViewportPosition == ElementViewportPosition.CompletelyInViewport)
			{
				if (dependencyObject == null || DoubleUtil.GreaterThan(num, value) || (DoubleUtil.AreClose(num, value) && DoubleUtil.GreaterThan(num2, value2)))
				{
					dependencyObject = dependencyObject3;
					value = num;
					value2 = num2;
				}
			}
			else if (dependencyObject2 == null || DoubleUtil.GreaterThan(num, value3) || (DoubleUtil.AreClose(num, value3) && DoubleUtil.GreaterThan(num2, value4)))
			{
				dependencyObject2 = dependencyObject3;
				value3 = num;
				value4 = num2;
			}
		}
		if (dependencyObject == null)
		{
			return dependencyObject2;
		}
		return dependencyObject;
	}

	private double ComputeRangeScore(double rangeStart1, double rangeEnd1, double rangeStart2, double rangeEnd2)
	{
		if (DoubleUtil.GreaterThan(rangeStart1, rangeStart2))
		{
			double num = rangeStart1;
			rangeStart1 = rangeStart2;
			rangeStart2 = num;
			double num2 = rangeEnd1;
			rangeEnd1 = rangeEnd2;
			rangeEnd2 = num2;
		}
		if (DoubleUtil.LessThan(rangeEnd1, rangeEnd2))
		{
			return rangeEnd1 - rangeStart2;
		}
		return rangeEnd2 - rangeStart2;
	}

	private void ProcessForMenuMode(InputEventArgs inputEventArgs)
	{
		if (inputEventArgs.RoutedEvent == Keyboard.LostKeyboardFocusEvent)
		{
			if (inputEventArgs is KeyboardFocusChangedEventArgs { NewFocus: null } || inputEventArgs.Handled)
			{
				_lastKeyPressed = Key.None;
			}
		}
		else if (inputEventArgs.RoutedEvent == Keyboard.KeyDownEvent)
		{
			if (inputEventArgs.Handled)
			{
				_lastKeyPressed = Key.None;
				return;
			}
			KeyEventArgs keyEventArgs = inputEventArgs as KeyEventArgs;
			if (keyEventArgs.IsRepeat)
			{
				return;
			}
			if (_lastKeyPressed == Key.None)
			{
				if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Windows)) == 0)
				{
					_lastKeyPressed = GetRealKey(keyEventArgs);
				}
			}
			else
			{
				_lastKeyPressed = Key.None;
			}
			_win32MenuModeWorkAround = false;
		}
		else if (inputEventArgs.RoutedEvent == Keyboard.KeyUpEvent)
		{
			if (!inputEventArgs.Handled)
			{
				KeyEventArgs keyEventArgs2 = inputEventArgs as KeyEventArgs;
				Key realKey = GetRealKey(keyEventArgs2);
				if (realKey == _lastKeyPressed && IsMenuKey(realKey))
				{
					EnableKeyboardCues(keyEventArgs2.Source as DependencyObject, enable: true);
					keyEventArgs2.Handled = OnEnterMenuMode(keyEventArgs2.Source);
				}
				if (_win32MenuModeWorkAround)
				{
					if (IsMenuKey(realKey))
					{
						_win32MenuModeWorkAround = false;
						keyEventArgs2.Handled = true;
					}
				}
				else if (keyEventArgs2.Handled)
				{
					_win32MenuModeWorkAround = true;
				}
			}
			_lastKeyPressed = Key.None;
		}
		else if (inputEventArgs.RoutedEvent == Mouse.MouseDownEvent || inputEventArgs.RoutedEvent == Mouse.MouseUpEvent)
		{
			_lastKeyPressed = Key.None;
			_win32MenuModeWorkAround = false;
		}
	}

	private bool IsMenuKey(Key key)
	{
		if (key != Key.LeftAlt && key != Key.RightAlt)
		{
			return key == Key.F10;
		}
		return true;
	}

	private Key GetRealKey(KeyEventArgs e)
	{
		if (e.Key != Key.System)
		{
			return e.Key;
		}
		return e.SystemKey;
	}

	private bool OnEnterMenuMode(object eventSource)
	{
		if (_weakEnterMenuModeHandlers == null)
		{
			return false;
		}
		lock (_weakEnterMenuModeHandlers)
		{
			if (_weakEnterMenuModeHandlers.Count == 0)
			{
				return false;
			}
			PresentationSource source = null;
			if (eventSource != null)
			{
				Visual visual = eventSource as Visual;
				source = ((visual != null) ? PresentationSource.CriticalFromVisual(visual) : null);
			}
			else
			{
				nint activeWindow = MS.Win32.UnsafeNativeMethods.GetActiveWindow();
				if (activeWindow != IntPtr.Zero)
				{
					source = HwndSource.CriticalFromHwnd(activeWindow);
				}
			}
			if (source == null)
			{
				return false;
			}
			EventArgs e = EventArgs.Empty;
			bool handled = false;
			_weakEnterMenuModeHandlers.Process(delegate(object obj)
			{
				if (obj is EnterMenuModeEventHandler enterMenuModeEventHandler && enterMenuModeEventHandler(source, e))
				{
					handled = true;
				}
				return handled;
			});
			return handled;
		}
	}

	private void ProcessForUIState(InputEventArgs inputEventArgs)
	{
		RawUIStateInputReport rawUIStateInputReport = ExtractRawUIStateInputReport(inputEventArgs, InputManager.InputReportEvent);
		PresentationSource inputSource;
		if (rawUIStateInputReport != null && (inputSource = rawUIStateInputReport.InputSource) != null && (rawUIStateInputReport.Targets & RawUIStateTargets.HideAccelerators) != 0)
		{
			Visual rootVisual = inputSource.RootVisual;
			bool enable = rawUIStateInputReport.Action == RawUIStateActions.Clear;
			EnableKeyboardCues(rootVisual, enable);
		}
	}

	private RawUIStateInputReport ExtractRawUIStateInputReport(InputEventArgs e, RoutedEvent Event)
	{
		RawUIStateInputReport result = null;
		if (e is InputReportEventArgs inputReportEventArgs && inputReportEventArgs.Report.Type == InputType.Keyboard && inputReportEventArgs.RoutedEvent == Event)
		{
			result = inputReportEventArgs.Report as RawUIStateInputReport;
		}
		return result;
	}

	private void NotifyFocusEnterMainFocusScope(object sender, EventArgs e)
	{
		_weakFocusEnterMainFocusScopeHandlers.Process(delegate(object item)
		{
			if (item is EventHandler eventHandler)
			{
				eventHandler(sender, e);
			}
			return false;
		});
	}
}
