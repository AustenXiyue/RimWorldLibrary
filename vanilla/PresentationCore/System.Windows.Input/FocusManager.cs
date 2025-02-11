using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MS.Internal.KnownBoxes;

namespace System.Windows.Input;

/// <summary>Provides a set of static methods, attached properties, and events for determining and setting focus scopes and for setting the focused element within the scope. </summary>
public static class FocusManager
{
	/// <summary>Identifies the <see cref="E:System.Windows.Input.FocusManager.GotFocus" /> attached event.</summary>
	public static readonly RoutedEvent GotFocusEvent = EventManager.RegisterRoutedEvent("GotFocus", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FocusManager));

	/// <summary>Identifies the <see cref="E:System.Windows.Input.FocusManager.LostFocus" /> attached event.</summary>
	public static readonly RoutedEvent LostFocusEvent = EventManager.RegisterRoutedEvent("LostFocus", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FocusManager));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.FocusManager.FocusedElement" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.FocusManager.FocusedElement" /> attached property.</returns>
	public static readonly DependencyProperty FocusedElementProperty = DependencyProperty.RegisterAttached("FocusedElement", typeof(IInputElement), typeof(FocusManager), new PropertyMetadata(OnFocusedElementChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Input.FocusManager.IsFocusScope" /> attached property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.FocusManager.IsFocusScope" /> attached property.</returns>
	public static readonly DependencyProperty IsFocusScopeProperty = DependencyProperty.RegisterAttached("IsFocusScope", typeof(bool), typeof(FocusManager), new PropertyMetadata(BooleanBoxes.FalseBox));

	private static readonly UncommonField<bool> IsFocusedElementSet = new UncommonField<bool>();

	private static readonly UncommonField<WeakReference> FocusedElementWeakCacheField = new UncommonField<WeakReference>();

	private static readonly UncommonField<bool> IsFocusedElementCacheValid = new UncommonField<bool>();

	private static readonly UncommonField<WeakReference> FocusedElementCache = new UncommonField<WeakReference>();

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.FocusManager.GotFocus" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddGotFocusHandler(DependencyObject element, RoutedEventHandler handler)
	{
		UIElement.AddHandler(element, GotFocusEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.FocusManager.GotFocus" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemoveGotFocusHandler(DependencyObject element, RoutedEventHandler handler)
	{
		UIElement.RemoveHandler(element, GotFocusEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Input.FocusManager.LostFocus" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddLostFocusHandler(DependencyObject element, RoutedEventHandler handler)
	{
		UIElement.AddHandler(element, LostFocusEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Input.FocusManager.LostFocus" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemoveLostFocusHandler(DependencyObject element, RoutedEventHandler handler)
	{
		UIElement.RemoveHandler(element, LostFocusEvent, handler);
	}

	/// <summary>Gets the element with logical focus within the specified focus scope.</summary>
	/// <returns>The element in the specified focus scope with logical focus.</returns>
	/// <param name="element">The element with logical focus in the specified focus scope.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public static IInputElement GetFocusedElement(DependencyObject element)
	{
		return GetFocusedElement(element, validate: false);
	}

	internal static IInputElement GetFocusedElement(DependencyObject element, bool validate)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		DependencyObject dependencyObject = (DependencyObject)element.GetValue(FocusedElementProperty);
		if (validate && dependencyObject != null)
		{
			if (PresentationSource.CriticalFromVisual(element) != PresentationSource.CriticalFromVisual(dependencyObject))
			{
				SetFocusedElement(element, null);
				dependencyObject = null;
			}
		}
		return (IInputElement)dependencyObject;
	}

	/// <summary>Sets logical focus on the specified element.</summary>
	/// <param name="element">The focus scope in which to make the specified element the <see cref="P:System.Windows.Input.FocusManager.FocusedElement" />.</param>
	/// <param name="value">The element to give logical focus to.</param>
	public static void SetFocusedElement(DependencyObject element, IInputElement value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FocusedElementProperty, value);
	}

	/// <summary>Sets the specified <see cref="T:System.Windows.DependencyObject" /> as a focus scope. </summary>
	/// <param name="element">The element to make a focus scope.</param>
	/// <param name="value">true if <paramref name="element" /> is a focus scope; otherwise, false.</param>
	public static void SetIsFocusScope(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsFocusScopeProperty, value);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.DependencyObject" /> is a focus scope.</summary>
	/// <returns>true if <see cref="P:System.Windows.Input.FocusManager.IsFocusScope" /> is set to true on the specified element; otherwise, false.</returns>
	/// <param name="element">The element from which to read the attached property.</param>
	public static bool GetIsFocusScope(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsFocusScopeProperty);
	}

	/// <summary>Determines the closest ancestor of the specified element that has <see cref="P:System.Windows.Input.FocusManager.IsFocusScope" /> set to true.</summary>
	/// <returns>The focus scope for the specified element.</returns>
	/// <param name="element">The element to get the closest focus scope for.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static DependencyObject GetFocusScope(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return _GetFocusScope(element);
	}

	private static void OnFocusedElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		IInputElement element = (IInputElement)e.NewValue;
		DependencyObject dependencyObject = (DependencyObject)e.OldValue;
		DependencyObject dependencyObject2 = (DependencyObject)e.NewValue;
		dependencyObject?.ClearValue(UIElement.IsFocusedPropertyKey);
		if (dependencyObject2 != null)
		{
			DependencyObject obj = Keyboard.FocusedElement as DependencyObject;
			dependencyObject2.SetValue(UIElement.IsFocusedPropertyKey, BooleanBoxes.TrueBox);
			DependencyObject dependencyObject3 = Keyboard.FocusedElement as DependencyObject;
			if (obj == dependencyObject3 && dependencyObject2 != dependencyObject3 && (dependencyObject3 == null || GetRoot(dependencyObject2) == GetRoot(dependencyObject3)))
			{
				Keyboard.Focus(element);
			}
		}
	}

	private static DependencyObject GetRoot(DependencyObject element)
	{
		if (element == null)
		{
			return null;
		}
		DependencyObject result = null;
		DependencyObject dependencyObject = element;
		if (element is ContentElement contentElement)
		{
			dependencyObject = contentElement.GetUIParent();
		}
		while (dependencyObject != null)
		{
			result = dependencyObject;
			dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
		}
		return result;
	}

	private static DependencyObject _GetFocusScope(DependencyObject d)
	{
		if (d == null)
		{
			return null;
		}
		if ((bool)d.GetValue(IsFocusScopeProperty))
		{
			return d;
		}
		if (d is UIElement uIElement)
		{
			DependencyObject uIParentCore = uIElement.GetUIParentCore();
			if (uIParentCore != null)
			{
				return GetFocusScope(uIParentCore);
			}
		}
		else if (d is ContentElement contentElement)
		{
			DependencyObject uIParent = contentElement.GetUIParent(continuePastVisualTree: true);
			if (uIParent != null)
			{
				return _GetFocusScope(uIParent);
			}
		}
		else if (d is UIElement3D uIElement3D)
		{
			DependencyObject uIParentCore2 = uIElement3D.GetUIParentCore();
			if (uIParentCore2 != null)
			{
				return GetFocusScope(uIParentCore2);
			}
		}
		if (d is Visual || d is Visual3D)
		{
			DependencyObject parent = VisualTreeHelper.GetParent(d);
			if (parent != null)
			{
				return _GetFocusScope(parent);
			}
		}
		return d;
	}
}
