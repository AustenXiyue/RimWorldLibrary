using System.ComponentModel;
using System.Windows.Controls.Primitives;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Represents a service that provides properties and events to control the display and behavior of tooltips.</summary>
public static class ToolTipService
{
	internal enum TriggerAction
	{
		Mouse,
		KeyboardFocus,
		KeyboardShortcut
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.ToolTip" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTipService.ToolTip" /> attached property.</returns>
	public static readonly DependencyProperty ToolTipProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.HorizontalOffset" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTipService.HorizontalOffset" /> attached property.</returns>
	public static readonly DependencyProperty HorizontalOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.VerticalOffset" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTipService.VerticalOffset" /> attached property.</returns>
	public static readonly DependencyProperty VerticalOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.HasDropShadow" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTipService.HasDropShadow" /> attached property.</returns>
	public static readonly DependencyProperty HasDropShadowProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.PlacementTarget" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTipService.PlacementTarget" /> attached property.</returns>
	public static readonly DependencyProperty PlacementTargetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.PlacementRectangle" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTipService.PlacementRectangle" /> attached property.</returns>
	public static readonly DependencyProperty PlacementRectangleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.Placement" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTipService.Placement" /> attached property.</returns>
	public static readonly DependencyProperty PlacementProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.ShowOnDisabled" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTipService.ShowOnDisabled" /> attached property.</returns>
	public static readonly DependencyProperty ShowOnDisabledProperty;

	private static readonly DependencyPropertyKey IsOpenPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.IsOpen" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTipService.IsOpen" /> attached property.</returns>
	public static readonly DependencyProperty IsOpenProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.IsEnabled" /> attached property. </summary>
	/// <returns>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.IsEnabled" /> attached property.</returns>
	public static readonly DependencyProperty IsEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.ShowDuration" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTipService.ShowDuration" /> attached property.</returns>
	public static readonly DependencyProperty ShowDurationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.InitialShowDelay" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTipService.InitialShowDelay" /> attached property.</returns>
	public static readonly DependencyProperty InitialShowDelayProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ToolTipService.BetweenShowDelay" /> attached property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ToolTipService.BetweenShowDelay" /> attached property.</returns>
	public static readonly DependencyProperty BetweenShowDelayProperty;

	public static readonly DependencyProperty ShowsToolTipOnKeyboardFocusProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.ToolTipService.ToolTipOpening" /> event that is exposed by objects that use the <see cref="T:System.Windows.Controls.ToolTipService" /> service to display tooltips. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.ToolTipService.ToolTipOpening" /> event.</returns>
	public static readonly RoutedEvent ToolTipOpeningEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.ToolTipService.ToolTipClosing" /> event that is exposed by objects that use the <see cref="T:System.Windows.Controls.ToolTipService" /> service to display tooltips. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.ToolTipService.ToolTipClosing" /> event.</returns>
	public static readonly RoutedEvent ToolTipClosingEvent;

	internal static readonly RoutedEvent FindToolTipEvent;

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.ToolTip" /> attached property for an object.</summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.ToolTip" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static object GetToolTip(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return element.GetValue(ToolTipProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ToolTipService.ToolTip" /> attached property for an object.</summary>
	/// <param name="element">The object to which the attached property is written.</param>
	/// <param name="value">The value to set.</param>
	public static void SetToolTip(DependencyObject element, object value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ToolTipProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.HorizontalOffset" /> attached property for an object.</summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.HorizontalOffset" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
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

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ToolTipService.HorizontalOffset" /> attached property for an object.</summary>
	/// <param name="element">The object to which the attached property is written.</param>
	/// <param name="value">The value to set.</param>
	public static void SetHorizontalOffset(DependencyObject element, double value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(HorizontalOffsetProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.VerticalOffset" /> attached property for an object.</summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.VerticalOffset" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
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

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ToolTipService.VerticalOffset" /> attached property for an object.</summary>
	/// <param name="element">The object to which the attached property is written.</param>
	/// <param name="value">The desired value.</param>
	public static void SetVerticalOffset(DependencyObject element, double value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(VerticalOffsetProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.HasDropShadow" /> attached property for an object. </summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.HasDropShadow" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetHasDropShadow(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(HasDropShadowProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ToolTipService.HasDropShadow" /> attached property for an object.</summary>
	/// <param name="element">The object to which the attached property is written.</param>
	/// <param name="value">The value to set.</param>
	public static void SetHasDropShadow(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(HasDropShadowProperty, BooleanBoxes.Box(value));
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.PlacementTarget" /> attached property for an object. </summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.PlacementTarget" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static UIElement GetPlacementTarget(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (UIElement)element.GetValue(PlacementTargetProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ToolTipService.PlacementTarget" /> attached property for an object. </summary>
	/// <param name="element">The object to which the attached property is written.</param>
	/// <param name="value">The value to set.</param>
	public static void SetPlacementTarget(DependencyObject element, UIElement value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(PlacementTargetProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.PlacementRectangle" /> attached property for an object.</summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.PlacementRectangle" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static Rect GetPlacementRectangle(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (Rect)element.GetValue(PlacementRectangleProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ToolTipService.PlacementRectangle" /> attached property for an object. </summary>
	/// <param name="element">The object to which the attached property is written.</param>
	/// <param name="value">The value to set.</param>
	public static void SetPlacementRectangle(DependencyObject element, Rect value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(PlacementRectangleProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.Placement" /> attached property for an object.</summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.Placement" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static PlacementMode GetPlacement(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (PlacementMode)element.GetValue(PlacementProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ToolTipService.Placement" /> attached property for an object. </summary>
	/// <param name="element">The object to which the attached property is written.</param>
	/// <param name="value">The value to set.</param>
	public static void SetPlacement(DependencyObject element, PlacementMode value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(PlacementProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.ShowOnDisabled" /> attached property for an object. </summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.ShowOnDisabled" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetShowOnDisabled(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(ShowOnDisabledProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ToolTipService.ShowOnDisabled" /> attached property for an object. </summary>
	/// <param name="element">The object to which the attached property is written.</param>
	/// <param name="value">The value to set.</param>
	public static void SetShowOnDisabled(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ShowOnDisabledProperty, BooleanBoxes.Box(value));
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.IsOpen" /> attached property for an object.</summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.IsOpen" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetIsOpen(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsOpenProperty);
	}

	private static void SetIsOpen(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsOpenPropertyKey, BooleanBoxes.Box(value));
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.IsEnabled" /> attached property for an object.</summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.IsEnabled" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static bool GetIsEnabled(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsEnabledProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ToolTipService.IsEnabled" /> attached property for an object. </summary>
	/// <param name="element">The object to which the attached property is written.</param>
	/// <param name="value">The value to set.</param>
	public static void SetIsEnabled(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsEnabledProperty, BooleanBoxes.Box(value));
	}

	private static bool PositiveValueValidation(object o)
	{
		return (int)o >= 0;
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.ShowDuration" /> attached property for an object. </summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.ShowDuration" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static int GetShowDuration(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(ShowDurationProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ToolTipService.ShowDuration" /> attached property for an object. </summary>
	/// <param name="element">The object to which the attached property is written.</param>
	/// <param name="value">The value to set.</param>
	public static void SetShowDuration(DependencyObject element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ShowDurationProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.InitialShowDelay" /> attached property for an object.</summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.InitialShowDelay" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static int GetInitialShowDelay(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(InitialShowDelayProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ToolTipService.InitialShowDelay" /> attached property for an object.</summary>
	/// <param name="element">The object to which the attached property is written.</param>
	/// <param name="value">The value to set.</param>
	public static void SetInitialShowDelay(DependencyObject element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(InitialShowDelayProperty, value);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ToolTipService.BetweenShowDelay" /> attached property for an object.</summary>
	/// <returns>The object's <see cref="P:System.Windows.Controls.ToolTipService.BetweenShowDelay" /> property value.</returns>
	/// <param name="element">The object from which the property value is read.</param>
	[AttachedPropertyBrowsableForType(typeof(DependencyObject))]
	public static int GetBetweenShowDelay(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (int)element.GetValue(BetweenShowDelayProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ToolTipService.BetweenShowDelay" /> attached property for an object. </summary>
	/// <param name="element">The object to which the attached property is written.</param>
	/// <param name="value">The value to set.</param>
	public static void SetBetweenShowDelay(DependencyObject element, int value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(BetweenShowDelayProperty, value);
	}

	public static bool? GetShowsToolTipOnKeyboardFocus(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool?)element.GetValue(ShowsToolTipOnKeyboardFocusProperty);
	}

	public static void SetShowsToolTipOnKeyboardFocus(DependencyObject element, bool? value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(ShowsToolTipOnKeyboardFocusProperty, BooleanBoxes.Box(value));
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Controls.ToolTipService.ToolTipOpening" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddToolTipOpeningHandler(DependencyObject element, ToolTipEventHandler handler)
	{
		UIElement.AddHandler(element, ToolTipOpeningEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Controls.ToolTipService.ToolTipOpening" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemoveToolTipOpeningHandler(DependencyObject element, ToolTipEventHandler handler)
	{
		UIElement.RemoveHandler(element, ToolTipOpeningEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Controls.ToolTipService.ToolTipClosing" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be added.</param>
	public static void AddToolTipClosingHandler(DependencyObject element, ToolTipEventHandler handler)
	{
		UIElement.AddHandler(element, ToolTipClosingEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Controls.ToolTipService.ToolTipClosing" /> attached event.</summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to this event.</param>
	/// <param name="handler">The event handler to be removed.</param>
	public static void RemoveToolTipClosingHandler(DependencyObject element, ToolTipEventHandler handler)
	{
		UIElement.RemoveHandler(element, ToolTipClosingEvent, handler);
	}

	static ToolTipService()
	{
		ToolTipProperty = DependencyProperty.RegisterAttached("ToolTip", typeof(object), typeof(ToolTipService), new FrameworkPropertyMetadata((object)null));
		HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(ToolTipService), new FrameworkPropertyMetadata(0.0));
		VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ToolTipService), new FrameworkPropertyMetadata(0.0));
		HasDropShadowProperty = DependencyProperty.RegisterAttached("HasDropShadow", typeof(bool), typeof(ToolTipService), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		PlacementTargetProperty = DependencyProperty.RegisterAttached("PlacementTarget", typeof(UIElement), typeof(ToolTipService), new FrameworkPropertyMetadata((object)null));
		PlacementRectangleProperty = DependencyProperty.RegisterAttached("PlacementRectangle", typeof(Rect), typeof(ToolTipService), new FrameworkPropertyMetadata(Rect.Empty));
		PlacementProperty = DependencyProperty.RegisterAttached("Placement", typeof(PlacementMode), typeof(ToolTipService), new FrameworkPropertyMetadata(PlacementMode.Mouse));
		ShowOnDisabledProperty = DependencyProperty.RegisterAttached("ShowOnDisabled", typeof(bool), typeof(ToolTipService), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsOpenPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsOpen", typeof(bool), typeof(ToolTipService), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		IsOpenProperty = IsOpenPropertyKey.DependencyProperty;
		IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ToolTipService), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		ShowDurationProperty = DependencyProperty.RegisterAttached("ShowDuration", typeof(int), typeof(ToolTipService), new FrameworkPropertyMetadata(int.MaxValue), PositiveValueValidation);
		InitialShowDelayProperty = DependencyProperty.RegisterAttached("InitialShowDelay", typeof(int), typeof(ToolTipService), new FrameworkPropertyMetadata(1000), PositiveValueValidation);
		BetweenShowDelayProperty = DependencyProperty.RegisterAttached("BetweenShowDelay", typeof(int), typeof(ToolTipService), new FrameworkPropertyMetadata(100), PositiveValueValidation);
		ShowsToolTipOnKeyboardFocusProperty = DependencyProperty.RegisterAttached("ShowsToolTipOnKeyboardFocus", typeof(bool?), typeof(ToolTipService), new FrameworkPropertyMetadata(null));
		ToolTipOpeningEvent = EventManager.RegisterRoutedEvent("ToolTipOpening", RoutingStrategy.Direct, typeof(ToolTipEventHandler), typeof(ToolTipService));
		ToolTipClosingEvent = EventManager.RegisterRoutedEvent("ToolTipClosing", RoutingStrategy.Direct, typeof(ToolTipEventHandler), typeof(ToolTipService));
		FindToolTipEvent = EventManager.RegisterRoutedEvent("FindToolTip", RoutingStrategy.Bubble, typeof(FindToolTipEventHandler), typeof(ToolTipService));
		EventManager.RegisterClassHandler(typeof(UIElement), FindToolTipEvent, new FindToolTipEventHandler(OnFindToolTip));
		EventManager.RegisterClassHandler(typeof(ContentElement), FindToolTipEvent, new FindToolTipEventHandler(OnFindToolTip));
		EventManager.RegisterClassHandler(typeof(UIElement3D), FindToolTipEvent, new FindToolTipEventHandler(OnFindToolTip));
	}

	private static void OnFindToolTip(object sender, FindToolTipEventArgs e)
	{
		if (e.TargetElement == null && sender is DependencyObject dependencyObject && ToolTipIsEnabled(dependencyObject, e.TriggerAction))
		{
			e.TargetElement = dependencyObject;
			e.Handled = true;
		}
	}

	internal static bool ToolTipIsEnabled(DependencyObject o, TriggerAction triggerAction)
	{
		object toolTip = GetToolTip(o);
		if (toolTip != null && GetIsEnabled(o))
		{
			bool flag = true;
			if (triggerAction == TriggerAction.KeyboardFocus)
			{
				bool? showsToolTipOnKeyboardFocus = GetShowsToolTipOnKeyboardFocus(o);
				if (!showsToolTipOnKeyboardFocus.HasValue && toolTip is ToolTip toolTip2)
				{
					showsToolTipOnKeyboardFocus = toolTip2.ShowsToolTipOnKeyboardFocus;
				}
				flag = showsToolTipOnKeyboardFocus != false;
			}
			if ((PopupControlService.IsElementEnabled(o) || GetShowOnDisabled(o)) && flag)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsFromKeyboard(TriggerAction triggerAction)
	{
		if (triggerAction != TriggerAction.KeyboardFocus)
		{
			return triggerAction == TriggerAction.KeyboardShortcut;
		}
		return true;
	}
}
