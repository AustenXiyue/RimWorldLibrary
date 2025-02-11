using System.ComponentModel;
using System.Windows.Controls.Primitives;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Provides the system implementation for displaying a <see cref="T:System.Windows.Controls.ContextMenu" />. </summary>
public static class ContextMenuService
{
	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ContextMenuService.ContextMenu" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenuService.ContextMenu" /> attached property.</returns>
	public static readonly DependencyProperty ContextMenuProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ContextMenuService.HorizontalOffset" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenuService.HorizontalOffset" /> attached property.</returns>
	public static readonly DependencyProperty HorizontalOffsetProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ContextMenuService.VerticalOffset" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenuService.VerticalOffset" /> attached property.</returns>
	public static readonly DependencyProperty VerticalOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ContextMenuService.HasDropShadow" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenuService.HasDropShadow" /> attached property.</returns>
	public static readonly DependencyProperty HasDropShadowProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ContextMenuService.PlacementTarget" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenuService.PlacementTarget" /> attached property.</returns>
	public static readonly DependencyProperty PlacementTargetProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ContextMenuService.PlacementRectangle" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenuService.PlacementRectangle" /> attached property.</returns>
	public static readonly DependencyProperty PlacementRectangleProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ContextMenuService.Placement" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenuService.Placement" /> attached property.</returns>
	public static readonly DependencyProperty PlacementProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ContextMenuService.ShowOnDisabled" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenuService.ShowOnDisabled" /> attached property.</returns>
	public static readonly DependencyProperty ShowOnDisabledProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Controls.ContextMenuService.IsEnabled" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ContextMenuService.IsEnabled" /> attached property.</returns>
	public static readonly DependencyProperty IsEnabledProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.ContextMenuService.ContextMenuOpening" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.ContextMenuService.ContextMenuOpening" />  attached event.</returns>
	public static readonly RoutedEvent ContextMenuOpeningEvent;

	/// <summary> Identifies the <see cref="E:System.Windows.Controls.ContextMenuService.ContextMenuClosing" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.ContextMenuService.ContextMenuClosing" /> routed event.</returns>
	public static readonly RoutedEvent ContextMenuClosingEvent;

	/// <summary> Gets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.ContextMenu" /> property of the specified object. </summary>
	/// <returns>Value of the <see cref="P:System.Windows.Controls.ContextMenuService.ContextMenu" /> property.</returns>
	/// <param name="element">Object to query concerning the <see cref="P:System.Windows.FrameworkElement.ContextMenu" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static ContextMenu GetContextMenu(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		ContextMenu contextMenu = (ContextMenu)element.GetValue(ContextMenuProperty);
		if (contextMenu != null && element.Dispatcher != contextMenu.Dispatcher)
		{
			throw new ArgumentException(SR.ContextMenuInDifferentDispatcher);
		}
		return contextMenu;
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.ContextMenu" /> property of the specified object.</summary>
	/// <param name="element">Object to set the property on.</param>
	/// <param name="value">Value to set.</param>
	public static void SetContextMenu(DependencyObject element, ContextMenu value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ContextMenuProperty, value);
	}

	/// <summary> Gets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.HorizontalOffset" /> property of the specified object. </summary>
	/// <returns>Value of the <see cref="P:System.Windows.Controls.ContextMenuService.HorizontalOffset" /> property.</returns>
	/// <param name="element">Object to query concerning the <see cref="P:System.Windows.Con" />trols.ContextMenuService.HorizontalOffset property.</param>
	[TypeConverter(typeof(LengthConverter))]
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static double GetHorizontalOffset(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(HorizontalOffsetProperty);
	}

	/// <summary> Sets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.HorizontalOffset" /> property of the specified object. </summary>
	/// <param name="element">Object to set the value on.</param>
	/// <param name="value">Value to set.</param>
	public static void SetHorizontalOffset(DependencyObject element, double value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(HorizontalOffsetProperty, value);
	}

	/// <summary> Gets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.VerticalOffset" /> property of the specified object. </summary>
	/// <returns>Value of the <see cref="P:System.Windows.Controls.ContextMenuService.VerticalOffset" /> property.</returns>
	/// <param name="element">Object to query concerning the <see cref="P:System.Windows.Controls.ContextMenuService.VerticalOffset" /> property.</param>
	[TypeConverter(typeof(LengthConverter))]
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static double GetVerticalOffset(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(VerticalOffsetProperty);
	}

	/// <summary> Sets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.Vertica" />lOffset property of the specified object. </summary>
	/// <param name="element">Object to set value on.</param>
	/// <param name="value">Value to set.</param>
	public static void SetVerticalOffset(DependencyObject element, double value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(VerticalOffsetProperty, value);
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.ContextMenu" /> has a drop shadow. </summary>
	/// <returns>A Boolean value, true if the <see cref="T:System.Windows.Controls.ContextMenu" /> has a drop shadow; false otherwise.</returns>
	/// <param name="element">Object to query concerning whether it has a drop shadow.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetHasDropShadow(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(HasDropShadowProperty);
	}

	/// <summary>Sets a value that indicates whether the <see cref="T:System.Windows.Controls.ContextMenu" /> has a drop shadow.</summary>
	/// <param name="element">Object to set the property on.</param>
	/// <param name="value">Boolean value to set, true if the <see cref="T:System.Windows.Controls.ContextMenu" /> has a drop shadow; false otherwise.</param>
	public static void SetHasDropShadow(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(HasDropShadowProperty, BooleanBoxes.Box(value));
	}

	/// <summary> Gets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.PlacementTa" />rget property of the specified object. </summary>
	/// <returns>Value of the <see cref="P:System.Windows.Controls.ContextMenuService.PlacementTarget" /> property.</returns>
	/// <param name="element">Object to query concerning the <see cref="P:System.Windows.Controls.ContextMenuService.PlacementTarget" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static UIElement GetPlacementTarget(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (UIElement)element.GetValue(PlacementTargetProperty);
	}

	/// <summary> Sets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.PlacementTarget" /> property of the specified object. </summary>
	/// <param name="element">Object to set value on.</param>
	/// <param name="value">Value to set.</param>
	public static void SetPlacementTarget(DependencyObject element, UIElement value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(PlacementTargetProperty, value);
	}

	/// <summary> Gets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.PlacementRectangle" /> property of the specified object. </summary>
	/// <returns>Value of the <see cref="P:System.Windows.Controls.ContextMenuService.PlacementRectangle" /> property.</returns>
	/// <param name="element">Object to query concerning the <see cref="P:System.Windows.Controls.ContextMenuService.PlacementRectangle" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static Rect GetPlacementRectangle(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (Rect)element.GetValue(PlacementRectangleProperty);
	}

	/// <summary> Sets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.P" />lacementRectangle property of the specified object. </summary>
	/// <param name="element">Object to set the value on.</param>
	/// <param name="value">Value to set.</param>
	public static void SetPlacementRectangle(DependencyObject element, Rect value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(PlacementRectangleProperty, value);
	}

	/// <summary> Gets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.Placement" /> property of the specified object. </summary>
	/// <returns>Value of the <see cref="P:System.Windows.Controls.ContextMenuService.Placement" /> property.</returns>
	/// <param name="element">Object to query concerning the <see cref="P:System.Windows.Controls.ContextMenuService.Placement" />  property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static PlacementMode GetPlacement(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (PlacementMode)element.GetValue(PlacementProperty);
	}

	/// <summary> Sets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.Placement" />  property of the specified object. </summary>
	/// <param name="element">Object to set the value on.</param>
	/// <param name="value">Value to set.</param>
	public static void SetPlacement(DependencyObject element, PlacementMode value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(PlacementProperty, value);
	}

	/// <summary> Gets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.ShowOnDisabled" /> property of the specified object. </summary>
	/// <returns>Value of the <see cref="P:System.Windows.Controls.ContextMenuService.ShowOnDisabled" /> property.</returns>
	/// <param name="element">Object to query concerning the <see cref="P:System.Windows.Controls.ContextMenuService.ShowOnDisabled" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetShowOnDisabled(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(ShowOnDisabledProperty);
	}

	/// <summary> Sets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.ShowOnDisabled" /> property of the specified object. </summary>
	/// <param name="element">Object to set value on.</param>
	/// <param name="value">Value to set.</param>
	public static void SetShowOnDisabled(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ShowOnDisabledProperty, BooleanBoxes.Box(value));
	}

	/// <summary> Gets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.IsEnabled" /> property of the specified object. </summary>
	/// <returns>Value of the <see cref="P:System.Windows.Controls.ContextMenuService.IsEnabled" /> property.</returns>
	/// <param name="element">Object to query concerning the <see cref="P:System.Windows.Controls.ContextMenuService.IsEnabled" /> property.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetIsEnabled(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsEnabledProperty);
	}

	/// <summary> Sets the value of the <see cref="P:System.Windows.Controls.ContextMenuService.IsEnabled" /> property of the specified object. </summary>
	/// <param name="element">Object to set value on.</param>
	/// <param name="value">Value to set.</param>
	public static void SetIsEnabled(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsEnabledProperty, BooleanBoxes.Box(value));
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Controls.ContextMenuService.ContextMenuOpening" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddContextMenuOpeningHandler(DependencyObject element, ContextMenuEventHandler handler)
	{
		UIElement.AddHandler(element, ContextMenuOpeningEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Controls.ContextMenuService.ContextMenuOpening" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemoveContextMenuOpeningHandler(DependencyObject element, ContextMenuEventHandler handler)
	{
		UIElement.RemoveHandler(element, ContextMenuOpeningEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Controls.ContextMenuService.ContextMenuClosing" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddContextMenuClosingHandler(DependencyObject element, ContextMenuEventHandler handler)
	{
		UIElement.AddHandler(element, ContextMenuClosingEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Controls.ContextMenuService.ContextMenuClosing" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemoveContextMenuClosingHandler(DependencyObject element, ContextMenuEventHandler handler)
	{
		UIElement.RemoveHandler(element, ContextMenuClosingEvent, handler);
	}

	static ContextMenuService()
	{
		ContextMenuProperty = DependencyProperty.RegisterAttached("ContextMenu", typeof(ContextMenu), typeof(ContextMenuService), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
		HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(ContextMenuService), new FrameworkPropertyMetadata(0.0));
		VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ContextMenuService), new FrameworkPropertyMetadata(0.0));
		HasDropShadowProperty = DependencyProperty.RegisterAttached("HasDropShadow", typeof(bool), typeof(ContextMenuService), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		PlacementTargetProperty = DependencyProperty.RegisterAttached("PlacementTarget", typeof(UIElement), typeof(ContextMenuService), new FrameworkPropertyMetadata((object)null));
		PlacementRectangleProperty = DependencyProperty.RegisterAttached("PlacementRectangle", typeof(Rect), typeof(ContextMenuService), new FrameworkPropertyMetadata(Rect.Empty));
		PlacementProperty = DependencyProperty.RegisterAttached("Placement", typeof(PlacementMode), typeof(ContextMenuService), new FrameworkPropertyMetadata(PlacementMode.MousePoint));
		ShowOnDisabledProperty = DependencyProperty.RegisterAttached("ShowOnDisabled", typeof(bool), typeof(ContextMenuService), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ContextMenuService), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		ContextMenuOpeningEvent = EventManager.RegisterRoutedEvent("ContextMenuOpening", RoutingStrategy.Bubble, typeof(ContextMenuEventHandler), typeof(ContextMenuService));
		ContextMenuClosingEvent = EventManager.RegisterRoutedEvent("ContextMenuClosing", RoutingStrategy.Bubble, typeof(ContextMenuEventHandler), typeof(ContextMenuService));
		EventManager.RegisterClassHandler(typeof(UIElement), ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpening));
		EventManager.RegisterClassHandler(typeof(ContentElement), ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpening));
		EventManager.RegisterClassHandler(typeof(UIElement3D), ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpening));
	}

	private static void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
	{
		if (e.TargetElement == null && sender is DependencyObject dependencyObject && ContextMenuIsEnabled(dependencyObject))
		{
			e.TargetElement = dependencyObject;
		}
	}

	internal static bool ContextMenuIsEnabled(DependencyObject o)
	{
		bool result = false;
		if (GetContextMenu(o) != null && GetIsEnabled(o) && (PopupControlService.IsElementEnabled(o) || GetShowOnDisabled(o)))
		{
			result = true;
		}
		return result;
	}
}
