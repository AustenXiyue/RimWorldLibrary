using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Diagnostics;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;
using MS.Utility;

namespace System.Windows;

/// <summary>Provides a WPF framework-level set of properties, events, and methods for Windows Presentation Foundation (WPF) elements. This class represents the provided WPF framework-level implementation that is built on the WPF core-level APIs that are defined by <see cref="T:System.Windows.UIElement" />.</summary>
[StyleTypedProperty(Property = "FocusVisualStyle", StyleTargetType = typeof(Control))]
[XmlLangProperty("Language")]
[UsableDuringInitialization(true)]
[RuntimeNameProperty("Name")]
public class FrameworkElement : UIElement, IFrameworkInputElement, IInputElement, ISupportInitialize, IHaveResources, IQueryAmbient
{
	private struct MinMax
	{
		internal double minWidth;

		internal double maxWidth;

		internal double minHeight;

		internal double maxHeight;

		internal MinMax(FrameworkElement e)
		{
			maxHeight = e.MaxHeight;
			minHeight = e.MinHeight;
			double height = e.Height;
			double val = (double.IsNaN(height) ? double.PositiveInfinity : height);
			maxHeight = Math.Max(Math.Min(val, maxHeight), minHeight);
			val = (double.IsNaN(height) ? 0.0 : height);
			minHeight = Math.Max(Math.Min(maxHeight, val), minHeight);
			maxWidth = e.MaxWidth;
			minWidth = e.MinWidth;
			height = e.Width;
			double val2 = (double.IsNaN(height) ? double.PositiveInfinity : height);
			maxWidth = Math.Max(Math.Min(val2, maxWidth), minWidth);
			val2 = (double.IsNaN(height) ? 0.0 : height);
			minWidth = Math.Max(Math.Min(maxWidth, val2), minWidth);
		}
	}

	private class LayoutTransformData
	{
		internal Size UntransformedDS;

		internal Size TransformedUnroundedDS;

		private Transform _transform;

		internal Transform Transform => _transform;

		internal void CreateTransformSnapshot(Transform sourceTransform)
		{
			_transform = new MatrixTransform(sourceTransform.Value);
			_transform.Freeze();
		}
	}

	private class FrameworkServices
	{
		internal KeyboardNavigation _keyboardNavigation;

		internal PopupControlService _popupControlService;

		internal FrameworkServices()
		{
			_keyboardNavigation = new KeyboardNavigation();
			_popupControlService = new PopupControlService();
		}
	}

	private static readonly Type _typeofThis;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.Style" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.Style" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty StyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.OverridesDefaultStyle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.OverridesDefaultStyle" /> dependency property.</returns>
	public static readonly DependencyProperty OverridesDefaultStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.UseLayoutRounding" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.UseLayoutRounding" /> dependency property.</returns>
	public static readonly DependencyProperty UseLayoutRoundingProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.DefaultStyleKey" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkElement.DefaultStyleKey" /> dependency property identifier.</returns>
	protected internal static readonly DependencyProperty DefaultStyleKeyProperty;

	internal static readonly NumberSubstitution DefaultNumberSubstitution;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.DataContext" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkElement.DataContext" /> dependency property identifier.</returns>
	public static readonly DependencyProperty DataContextProperty;

	internal static readonly EventPrivateKey DataContextChangedKey;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.BindingGroup" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.BindingGroup" /> dependency property.</returns>
	public static readonly DependencyProperty BindingGroupProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.Language" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkElement.Language" /> dependency property identifier.</returns>
	public static readonly DependencyProperty LanguageProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.Name" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.Name" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty NameProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.Tag" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.Tag" /> dependency property.</returns>
	public static readonly DependencyProperty TagProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.InputScope" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.InputScope" /> dependency property.</returns>
	public static readonly DependencyProperty InputScopeProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.FrameworkElement.RequestBringIntoView" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.RequestBringIntoView" /> routed event.Routed event identifiers are created when routed events are registered. These identifiers contain an identifying name, owner type, handler type, routing strategy, and utility method for adding owners for the event. You can use these identifiers to add class handlers. For more information about registering routed events, see <see cref="M:System.Windows.EventManager.RegisterRoutedEvent(System.String,System.Windows.RoutingStrategy,System.Type,System.Type)" />. For more information about using routed event identifiers to add class handlers, see <see cref="M:System.Windows.EventManager.RegisterClassHandler(System.Type,System.Windows.RoutedEvent,System.Delegate)" />.</returns>
	public static readonly RoutedEvent RequestBringIntoViewEvent;

	/// <summary> Identifies the <see cref="E:System.Windows.FrameworkElement.SizeChanged" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.RequestBringIntoView" /> routed event.</returns>
	public static readonly RoutedEvent SizeChangedEvent;

	private static PropertyMetadata _actualWidthMetadata;

	private static readonly DependencyPropertyKey ActualWidthPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.ActualWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.ActualWidth" /> dependency property.</returns>
	public static readonly DependencyProperty ActualWidthProperty;

	private static PropertyMetadata _actualHeightMetadata;

	private static readonly DependencyPropertyKey ActualHeightPropertyKey;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.ActualHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.ActualHeight" /> dependency property.</returns>
	public static readonly DependencyProperty ActualHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.LayoutTransform" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkElement.LayoutTransform" /> dependency property identifier.</returns>
	public static readonly DependencyProperty LayoutTransformProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.Width" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.Width" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty WidthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.MinWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.MinWidth" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty MinWidthProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.MaxWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.MaxWidth" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty MaxWidthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.Height" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.Height" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.MinHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.MinHeight" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty MinHeightProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.MaxHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.MaxHeight" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty MaxHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.FlowDirection" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkElement.FlowDirection" /> dependency property identifier.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty FlowDirectionProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.Margin" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkElement.Margin" /> dependency property identifier.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty MarginProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.HorizontalAlignment" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkElement.HorizontalAlignment" /> dependency property identifier.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HorizontalAlignmentProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.VerticalAlignment" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkElement.VerticalAlignment" /> dependency property identifier.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty VerticalAlignmentProperty;

	private static Style _defaultFocusVisualStyle;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.FocusVisualStyle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.FocusVisualStyle" /> dependency property.</returns>
	public static readonly DependencyProperty FocusVisualStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.Cursor" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.Cursor" /> dependency property.</returns>
	public static readonly DependencyProperty CursorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.ForceCursor" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.ForceCursor" /> dependency property.</returns>
	public static readonly DependencyProperty ForceCursorProperty;

	internal static readonly EventPrivateKey InitializedKey;

	internal static readonly DependencyPropertyKey LoadedPendingPropertyKey;

	internal static readonly DependencyProperty LoadedPendingProperty;

	internal static readonly DependencyPropertyKey UnloadedPendingPropertyKey;

	internal static readonly DependencyProperty UnloadedPendingProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.FrameworkElement.Loaded" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.Loaded" /> routed event.</returns>
	public static readonly RoutedEvent LoadedEvent;

	/// <summary> Identifies the <see cref="E:System.Windows.FrameworkElement.Unloaded" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.Unloaded" /> routed event.</returns>
	public static readonly RoutedEvent UnloadedEvent;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.ToolTip" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkElement.ToolTip" /> dependency property identifier.</returns>
	public static readonly DependencyProperty ToolTipProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.ContextMenu" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkElement.ContextMenu" /> dependency property identifier.</returns>
	public static readonly DependencyProperty ContextMenuProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.FrameworkElement.ToolTipOpening" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.ToolTipOpening" /> routed event.</returns>
	public static readonly RoutedEvent ToolTipOpeningEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.FrameworkElement.ToolTipClosing" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.ToolTipClosing" /> routed event.</returns>
	public static readonly RoutedEvent ToolTipClosingEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.FrameworkElement.ContextMenuOpening" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.ContextMenuClosing" /> routed event.</returns>
	public static readonly RoutedEvent ContextMenuOpeningEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.FrameworkElement.ContextMenuClosing" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.ContextMenuClosing" /> routed event.</returns>
	public static readonly RoutedEvent ContextMenuClosingEvent;

	private Style _themeStyleCache;

	private static readonly UncommonField<SizeBox> UnclippedDesiredSizeField;

	private static readonly UncommonField<LayoutTransformData> LayoutTransformDataField;

	private Style _styleCache;

	internal static readonly UncommonField<ResourceDictionary> ResourcesField;

	internal DependencyObject _templatedParent;

	private UIElement _templateChild;

	private InternalFlags _flags;

	private InternalFlags2 _flags2 = InternalFlags2.Default;

	internal static DependencyObjectType UIElementDType;

	private static DependencyObjectType _controlDType;

	private static DependencyObjectType _contentPresenterDType;

	private static DependencyObjectType _pageFunctionBaseDType;

	private static DependencyObjectType _pageDType;

	[ThreadStatic]
	private static FrameworkServices _frameworkServices;

	internal static readonly EventPrivateKey ResourcesChangedKey;

	internal static readonly EventPrivateKey InheritedPropertyChangedKey;

	private new DependencyObject _parent;

	private FrugalObjectList<DependencyProperty> _inheritableProperties;

	private static readonly UncommonField<DependencyObject> InheritanceContextField;

	private static readonly UncommonField<DependencyObject> MentorField;

	/// <summary>Gets or sets the style used by this element when it is rendered.  </summary>
	/// <returns>The applied, nondefault style for the element, if present. Otherwise, null. The default for a default-constructed <see cref="T:System.Windows.FrameworkElement" /> is null.</returns>
	public Style Style
	{
		get
		{
			return _styleCache;
		}
		set
		{
			SetValue(StyleProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether this element incorporates style properties from theme styles. </summary>
	/// <returns>true if this element does not use theme style properties; all style-originating properties come from local application styles, and theme style properties do not apply. false if application styles apply first, and then theme styles apply for properties that were not specifically set in application styles. The default is false.</returns>
	public bool OverridesDefaultStyle
	{
		get
		{
			return (bool)GetValue(OverridesDefaultStyleProperty);
		}
		set
		{
			SetValue(OverridesDefaultStyleProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that indicates whether layout rounding should be applied to this element's size and position during layout. </summary>
	/// <returns>true if layout rounding is applied; otherwise, false. The default is false.</returns>
	public bool UseLayoutRounding
	{
		get
		{
			return (bool)GetValue(UseLayoutRoundingProperty);
		}
		set
		{
			SetValue(UseLayoutRoundingProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets the key to use to reference the style for this control, when theme styles are used or defined.</summary>
	/// <returns>The style key. To work correctly as part of theme style lookup, this value is expected to be the <see cref="T:System.Type" /> of the control being styled.</returns>
	protected internal object DefaultStyleKey
	{
		get
		{
			return GetValue(DefaultStyleKeyProperty);
		}
		set
		{
			SetValue(DefaultStyleKeyProperty, value);
		}
	}

	internal Style ThemeStyle => _themeStyleCache;

	internal virtual DependencyObjectType DTypeThemeStyleKey => null;

	internal virtual FrameworkTemplate TemplateInternal => null;

	internal virtual FrameworkTemplate TemplateCache
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	/// <summary>Gets the collection of triggers established directly on this element, or in child elements. </summary>
	/// <returns>A strongly typed collection of <see cref="T:System.Windows.Trigger" /> objects.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public TriggerCollection Triggers
	{
		get
		{
			TriggerCollection triggerCollection = EventTrigger.TriggerCollectionField.GetValue(this);
			if (triggerCollection == null)
			{
				triggerCollection = new TriggerCollection(this);
				EventTrigger.TriggerCollectionField.SetValue(this, triggerCollection);
			}
			return triggerCollection;
		}
	}

	/// <summary>Gets a reference to the template parent of this element. This property is not relevant if the element was not created through a template.</summary>
	/// <returns>The element whose <see cref="T:System.Windows.FrameworkTemplate" /> <see cref="P:System.Windows.FrameworkTemplate.VisualTree" /> caused this element to be created. This value is frequently null; see Remarks.</returns>
	public DependencyObject TemplatedParent => _templatedParent;

	internal bool IsTemplateRoot => TemplateChildIndex == 1;

	internal virtual UIElement TemplateChild
	{
		get
		{
			return _templateChild;
		}
		set
		{
			if (value != _templateChild)
			{
				RemoveVisualChild(_templateChild);
				_templateChild = value;
				AddVisualChild(value);
			}
		}
	}

	internal virtual FrameworkElement StateGroupsRoot => _templateChild as FrameworkElement;

	/// <summary>Gets the number of visual child elements within this element.</summary>
	/// <returns>The number of visual child elements for this element.</returns>
	protected override int VisualChildrenCount => (_templateChild != null) ? 1 : 0;

	internal bool HasResources
	{
		get
		{
			ResourceDictionary value = ResourcesField.GetValue(this);
			if (value != null)
			{
				if (value.Count <= 0)
				{
					return value.MergedDictionaries.Count > 0;
				}
				return true;
			}
			return false;
		}
	}

	/// <summary> Gets or sets the locally-defined resource dictionary. </summary>
	/// <returns>The current locally-defined dictionary of resources, where each resource can be accessed by key.</returns>
	[Ambient]
	public ResourceDictionary Resources
	{
		get
		{
			ResourceDictionary resourceDictionary = ResourcesField.GetValue(this);
			if (resourceDictionary == null)
			{
				resourceDictionary = new ResourceDictionary();
				resourceDictionary.AddOwner(this);
				ResourcesField.SetValue(this, resourceDictionary);
				if (TraceResourceDictionary.IsEnabled)
				{
					TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.NewResourceDictionary, this, 0, resourceDictionary);
				}
			}
			return resourceDictionary;
		}
		set
		{
			ResourceDictionary value2 = ResourcesField.GetValue(this);
			ResourcesField.SetValue(this, value);
			if (TraceResourceDictionary.IsEnabled)
			{
				TraceResourceDictionary.Trace(TraceEventType.Start, TraceResourceDictionary.NewResourceDictionary, this, value2, value);
			}
			value2?.RemoveOwner(this);
			if (value != null && !value.ContainsOwner(this))
			{
				value.AddOwner(this);
			}
			if (value2 != value)
			{
				TreeWalkHelper.InvalidateOnResourcesChange(this, null, new ResourcesChangeInfo(value2, value));
			}
			if (TraceResourceDictionary.IsEnabled)
			{
				TraceResourceDictionary.Trace(TraceEventType.Stop, TraceResourceDictionary.NewResourceDictionary, this, value2, value);
			}
		}
	}

	ResourceDictionary IHaveResources.Resources
	{
		get
		{
			return Resources;
		}
		set
		{
			Resources = value;
		}
	}

	/// <summary>Gets or sets the scope limits for property value inheritance, resource key lookup, and RelativeSource FindAncestor lookup.</summary>
	/// <returns>A value of the enumeration. The default is <see cref="F:System.Windows.InheritanceBehavior.Default" />.</returns>
	protected internal InheritanceBehavior InheritanceBehavior
	{
		get
		{
			return (InheritanceBehavior)((uint)(_flags & (InternalFlags)56u) >> 3);
		}
		set
		{
			if (!IsInitialized)
			{
				if ((uint)value < 0u || (uint)value > 6u)
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(InheritanceBehavior));
				}
				uint num = (uint)((int)value << 3);
				_flags = (InternalFlags)((num & 0x38) | ((uint)_flags & 0xFFFFFFC7u));
				if (_parent != null)
				{
					TreeWalkHelper.InvalidateOnTreeChange(this, null, _parent, isAddOperation: true);
				}
				return;
			}
			throw new InvalidOperationException(SR.Illegal_InheritanceBehaviorSettor);
		}
	}

	/// <summary> Gets or sets the data context for an element when it participates in data binding.</summary>
	/// <returns>The object to use as data context.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public object DataContext
	{
		get
		{
			return GetValue(DataContextProperty);
		}
		set
		{
			SetValue(DataContextProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Data.BindingGroup" /> that is used for the element.</summary>
	/// <returns>The <see cref="T:System.Windows.Data.BindingGroup" /> that is used for the element.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public BindingGroup BindingGroup
	{
		get
		{
			return (BindingGroup)GetValue(BindingGroupProperty);
		}
		set
		{
			SetValue(BindingGroupProperty, value);
		}
	}

	/// <summary>Gets or sets localization/globalization language information that applies to an element.</summary>
	/// <returns>The language information for this element. The default value is an <see cref="T:System.Windows.Markup.XmlLanguage" /> with its <see cref="P:System.Windows.Markup.XmlLanguage.IetfLanguageTag" /> value set to the string "en-US".</returns>
	public XmlLanguage Language
	{
		get
		{
			return (XmlLanguage)GetValue(LanguageProperty);
		}
		set
		{
			SetValue(LanguageProperty, value);
		}
	}

	/// <summary>Gets or sets the identifying name of the element. The name provides a reference so that code-behind, such as event handler code, can refer to a markup element after it is constructed during processing by a XAML processor.</summary>
	/// <returns>The name of the element. The default is an empty string.</returns>
	[Localizability(LocalizationCategory.NeverLocalize)]
	[MergableProperty(false)]
	[DesignerSerializationOptions(DesignerSerializationOptions.SerializeAsAttribute)]
	public string Name
	{
		get
		{
			return (string)GetValue(NameProperty);
		}
		set
		{
			SetValue(NameProperty, value);
		}
	}

	/// <summary>Gets or sets an arbitrary object value that can be used to store custom information about this element.</summary>
	/// <returns>The intended value. This property has no default value.</returns>
	[Localizability(LocalizationCategory.NeverLocalize)]
	public object Tag
	{
		get
		{
			return GetValue(TagProperty);
		}
		set
		{
			SetValue(TagProperty, value);
		}
	}

	/// <summary>Gets or sets the context for input used by this <see cref="T:System.Windows.FrameworkElement" />. </summary>
	/// <returns>The input scope, which modifies how input from alternative input methods is interpreted. The default value is null (which results in a default handling of commands).</returns>
	public InputScope InputScope
	{
		get
		{
			return (InputScope)GetValue(InputScopeProperty);
		}
		set
		{
			SetValue(InputScopeProperty, value);
		}
	}

	/// <summary>Gets the rendered width of this element.</summary>
	/// <returns>The element's width, as a value in device-independent units (1/96th inch per unit). The default value is 0 (zero).</returns>
	public double ActualWidth => base.RenderSize.Width;

	/// <summary>Gets the rendered height of this element.</summary>
	/// <returns>The element's height, as a value in device-independent units (1/96th inch per unit). The default value is 0 (zero).</returns>
	public double ActualHeight => base.RenderSize.Height;

	/// <summary> Gets or sets a graphics transformation that should apply to this element when  layout is performed.</summary>
	/// <returns>The transform this element should use. The default is <see cref="P:System.Windows.Media.Transform.Identity" />.</returns>
	public Transform LayoutTransform
	{
		get
		{
			return (Transform)GetValue(LayoutTransformProperty);
		}
		set
		{
			SetValue(LayoutTransformProperty, value);
		}
	}

	/// <summary> Gets or sets the width of the element.</summary>
	/// <returns>The width of the element, in device-independent units (1/96th inch per unit). The default value is <see cref="F:System.Double.NaN" />. This value must be equal to or greater than 0.0. See Remarks for upper bound information.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public double Width
	{
		get
		{
			return (double)GetValue(WidthProperty);
		}
		set
		{
			SetValue(WidthProperty, value);
		}
	}

	/// <summary> Gets or sets the minimum width constraint of the element.</summary>
	/// <returns>The minimum width of the element, in device-independent units (1/96th inch per unit). The default value is 0.0. This value can be any value equal to or greater than 0.0. However, <see cref="F:System.Double.PositiveInfinity" /> is not valid, nor is <see cref="F:System.Double.NaN" />.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public double MinWidth
	{
		get
		{
			return (double)GetValue(MinWidthProperty);
		}
		set
		{
			SetValue(MinWidthProperty, value);
		}
	}

	/// <summary>Gets or sets the maximum width constraint of the element.</summary>
	/// <returns>The maximum width of the element, in device-independent units (1/96th inch per unit). The default value is <see cref="F:System.Double.PositiveInfinity" />. This value can be any value equal to or greater than 0.0. <see cref="F:System.Double.PositiveInfinity" /> is also valid.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public double MaxWidth
	{
		get
		{
			return (double)GetValue(MaxWidthProperty);
		}
		set
		{
			SetValue(MaxWidthProperty, value);
		}
	}

	/// <summary> Gets or sets the suggested height of the element.</summary>
	/// <returns>The height of the element, in device-independent units (1/96th inch per unit). The default value is <see cref="F:System.Double.NaN" />. This value must be equal to or greater than 0.0. See Remarks for upper bound information.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public double Height
	{
		get
		{
			return (double)GetValue(HeightProperty);
		}
		set
		{
			SetValue(HeightProperty, value);
		}
	}

	/// <summary>Gets or sets the minimum height constraint of the element.</summary>
	/// <returns>The minimum height of the element, in device-independent units (1/96th inch per unit). The default value is 0.0. This value can be any value equal to or greater than 0.0. However, <see cref="F:System.Double.PositiveInfinity" /> is NOT valid, nor is <see cref="F:System.Double.NaN" />.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public double MinHeight
	{
		get
		{
			return (double)GetValue(MinHeightProperty);
		}
		set
		{
			SetValue(MinHeightProperty, value);
		}
	}

	/// <summary>Gets or sets the maximum height constraint of the element.</summary>
	/// <returns>The maximum height of the element, in device-independent units (1/96th inch per unit). The default value is <see cref="F:System.Double.PositiveInfinity" />. This value can be any value equal to or greater than 0.0. <see cref="F:System.Double.PositiveInfinity" /> is also valid.</returns>
	[TypeConverter(typeof(LengthConverter))]
	[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
	public double MaxHeight
	{
		get
		{
			return (double)GetValue(MaxHeightProperty);
		}
		set
		{
			SetValue(MaxHeightProperty, value);
		}
	}

	/// <summary>Gets or sets the direction that text and other user interface (UI) elements flow within any parent element that controls their layout.</summary>
	/// <returns>The direction that text and other UI elements flow within their parent element, as a value of the enumeration. The default value is <see cref="F:System.Windows.FlowDirection.LeftToRight" />.</returns>
	[Localizability(LocalizationCategory.None)]
	public FlowDirection FlowDirection
	{
		get
		{
			if (!IsRightToLeft)
			{
				return FlowDirection.LeftToRight;
			}
			return FlowDirection.RightToLeft;
		}
		set
		{
			SetValue(FlowDirectionProperty, value);
		}
	}

	/// <summary>Gets or sets the outer margin of an element.</summary>
	/// <returns>Provides margin values for the element. The default value is a <see cref="T:System.Windows.Thickness" /> with all properties equal to 0 (zero).</returns>
	public Thickness Margin
	{
		get
		{
			return (Thickness)GetValue(MarginProperty);
		}
		set
		{
			SetValue(MarginProperty, value);
		}
	}

	/// <summary>Gets or sets the horizontal alignment characteristics applied to this element when it is composed within a parent element, such as a panel or items control.</summary>
	/// <returns>A horizontal alignment setting, as a value of the enumeration. The default is <see cref="F:System.Windows.HorizontalAlignment.Stretch" />.</returns>
	public HorizontalAlignment HorizontalAlignment
	{
		get
		{
			return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty);
		}
		set
		{
			SetValue(HorizontalAlignmentProperty, value);
		}
	}

	/// <summary>Gets or sets the vertical alignment characteristics applied to this element when it is composed within a parent element such as a panel or items control.</summary>
	/// <returns>A vertical alignment setting. The default is <see cref="F:System.Windows.VerticalAlignment.Stretch" />.</returns>
	public VerticalAlignment VerticalAlignment
	{
		get
		{
			return (VerticalAlignment)GetValue(VerticalAlignmentProperty);
		}
		set
		{
			SetValue(VerticalAlignmentProperty, value);
		}
	}

	internal static Style DefaultFocusVisualStyle
	{
		get
		{
			if (_defaultFocusVisualStyle == null)
			{
				Style style = new Style();
				style.Seal();
				_defaultFocusVisualStyle = style;
			}
			return _defaultFocusVisualStyle;
		}
	}

	/// <summary>Gets or sets a property that enables customization of appearance, effects, or other style characteristics that will apply to this element when it captures keyboard focus.</summary>
	/// <returns>The desired style to apply on focus. The default value as declared in the dependency property is an empty static <see cref="T:System.Windows.Style" />. However, the effective value at run time is often (but not always) a style as supplied by theme support for controls. </returns>
	public Style FocusVisualStyle
	{
		get
		{
			return (Style)GetValue(FocusVisualStyleProperty);
		}
		set
		{
			SetValue(FocusVisualStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the cursor that displays when the mouse pointer is over this element.</summary>
	/// <returns>The cursor to display. The default value is defined as null per this dependency property. However, the practical default at run time will come from a variety of factors.</returns>
	public Cursor Cursor
	{
		get
		{
			return (Cursor)GetValue(CursorProperty);
		}
		set
		{
			SetValue(CursorProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether this <see cref="T:System.Windows.FrameworkElement" /> should force the user interface (UI) to render the cursor as declared by the <see cref="P:System.Windows.FrameworkElement.Cursor" /> property.</summary>
	/// <returns>true if cursor presentation while over this element is forced to use current <see cref="P:System.Windows.FrameworkElement.Cursor" /> settings for the cursor (including on all child elements); otherwise false. The default value is false.</returns>
	public bool ForceCursor
	{
		get
		{
			return (bool)GetValue(ForceCursorProperty);
		}
		set
		{
			SetValue(ForceCursorProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets a value that indicates whether this element has been initialized, either during processing by a XAML processor, or by explicitly having its <see cref="M:System.Windows.FrameworkElement.EndInit" /> method called. </summary>
	/// <returns>true if the element is initialized per the aforementioned XAML processing or method calls; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public bool IsInitialized => ReadInternalFlag(InternalFlags.IsInitialized);

	/// <summary>Gets a value that indicates whether this element has been loaded for presentation. </summary>
	/// <returns>true if the current element is attached to an element tree; false if the element has never been attached to a loaded element tree. </returns>
	public bool IsLoaded
	{
		get
		{
			object[] loadedPending = LoadedPending;
			object[] unloadedPending = UnloadedPending;
			if (loadedPending == null && unloadedPending == null)
			{
				if (SubtreeHasLoadedChangeHandler)
				{
					return IsLoadedCache;
				}
				return BroadcastEventHelper.IsParentLoaded(this);
			}
			return unloadedPending != null;
		}
	}

	internal static PopupControlService PopupControlService => EnsureFrameworkServices()._popupControlService;

	internal static KeyboardNavigation KeyboardNavigation => EnsureFrameworkServices()._keyboardNavigation;

	/// <summary> Gets or sets the tool-tip object that is displayed for this element in the user interface (UI).</summary>
	/// <returns>The tooltip object. See Remarks below for details on why this parameter is not strongly typed.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	[Localizability(LocalizationCategory.ToolTip)]
	public object ToolTip
	{
		get
		{
			return ToolTipService.GetToolTip(this);
		}
		set
		{
			ToolTipService.SetToolTip(this, value);
		}
	}

	/// <summary> Gets or sets the context menu element that should appear whenever the context menu is requested through user interface (UI) from within this element.</summary>
	/// <returns>The context menu assigned to this element. </returns>
	public ContextMenu ContextMenu
	{
		get
		{
			return GetValue(ContextMenuProperty) as ContextMenu;
		}
		set
		{
			SetValue(ContextMenuProperty, value);
		}
	}

	internal bool HasResourceReference
	{
		get
		{
			return ReadInternalFlag(InternalFlags.HasResourceReferences);
		}
		set
		{
			WriteInternalFlag(InternalFlags.HasResourceReferences, value);
		}
	}

	internal bool IsLogicalChildrenIterationInProgress
	{
		get
		{
			return ReadInternalFlag(InternalFlags.IsLogicalChildrenIterationInProgress);
		}
		set
		{
			WriteInternalFlag(InternalFlags.IsLogicalChildrenIterationInProgress, value);
		}
	}

	internal bool InVisibilityCollapsedTree
	{
		get
		{
			return ReadInternalFlag(InternalFlags.InVisibilityCollapsedTree);
		}
		set
		{
			WriteInternalFlag(InternalFlags.InVisibilityCollapsedTree, value);
		}
	}

	internal bool SubtreeHasLoadedChangeHandler
	{
		get
		{
			return ReadInternalFlag2(InternalFlags2.TreeHasLoadedChangeHandler);
		}
		set
		{
			WriteInternalFlag2(InternalFlags2.TreeHasLoadedChangeHandler, value);
		}
	}

	internal bool IsLoadedCache
	{
		get
		{
			return ReadInternalFlag2(InternalFlags2.IsLoadedCache);
		}
		set
		{
			WriteInternalFlag2(InternalFlags2.IsLoadedCache, value);
		}
	}

	internal bool IsParentAnFE
	{
		get
		{
			return ReadInternalFlag2(InternalFlags2.IsParentAnFE);
		}
		set
		{
			WriteInternalFlag2(InternalFlags2.IsParentAnFE, value);
		}
	}

	internal bool IsTemplatedParentAnFE
	{
		get
		{
			return ReadInternalFlag2(InternalFlags2.IsTemplatedParentAnFE);
		}
		set
		{
			WriteInternalFlag2(InternalFlags2.IsTemplatedParentAnFE, value);
		}
	}

	internal bool HasLogicalChildren
	{
		get
		{
			return ReadInternalFlag(InternalFlags.HasLogicalChildren);
		}
		set
		{
			WriteInternalFlag(InternalFlags.HasLogicalChildren, value);
		}
	}

	private bool NeedsClipBounds
	{
		get
		{
			return ReadInternalFlag(InternalFlags.NeedsClipBounds);
		}
		set
		{
			WriteInternalFlag(InternalFlags.NeedsClipBounds, value);
		}
	}

	private bool HasWidthEverChanged
	{
		get
		{
			return ReadInternalFlag(InternalFlags.HasWidthEverChanged);
		}
		set
		{
			WriteInternalFlag(InternalFlags.HasWidthEverChanged, value);
		}
	}

	private bool HasHeightEverChanged
	{
		get
		{
			return ReadInternalFlag(InternalFlags.HasHeightEverChanged);
		}
		set
		{
			WriteInternalFlag(InternalFlags.HasHeightEverChanged, value);
		}
	}

	internal bool IsRightToLeft
	{
		get
		{
			return ReadInternalFlag(InternalFlags.IsRightToLeft);
		}
		set
		{
			WriteInternalFlag(InternalFlags.IsRightToLeft, value);
		}
	}

	internal int TemplateChildIndex
	{
		get
		{
			uint num = (uint)(_flags2 & InternalFlags2.Default);
			if (num == 65535)
			{
				return -1;
			}
			return (int)num;
		}
		set
		{
			if (value < -1 || value >= 65535)
			{
				throw new ArgumentOutOfRangeException("value", SR.TemplateChildIndexOutOfRange);
			}
			uint num = ((value == -1) ? 65535u : ((uint)value));
			_flags2 = (InternalFlags2)(num | ((uint)_flags2 & 0xFFFF0000u));
		}
	}

	internal bool IsRequestingExpression
	{
		get
		{
			return ReadInternalFlag2(InternalFlags2.IsRequestingExpression);
		}
		set
		{
			WriteInternalFlag2(InternalFlags2.IsRequestingExpression, value);
		}
	}

	internal bool BypassLayoutPolicies
	{
		get
		{
			return ReadInternalFlag2(InternalFlags2.BypassLayoutPolicies);
		}
		set
		{
			WriteInternalFlag2(InternalFlags2.BypassLayoutPolicies, value);
		}
	}

	private static DependencyObjectType ControlDType
	{
		get
		{
			if (_controlDType == null)
			{
				_controlDType = DependencyObjectType.FromSystemTypeInternal(typeof(Control));
			}
			return _controlDType;
		}
	}

	private static DependencyObjectType ContentPresenterDType
	{
		get
		{
			if (_contentPresenterDType == null)
			{
				_contentPresenterDType = DependencyObjectType.FromSystemTypeInternal(typeof(ContentPresenter));
			}
			return _contentPresenterDType;
		}
	}

	private static DependencyObjectType PageDType
	{
		get
		{
			if (_pageDType == null)
			{
				_pageDType = DependencyObjectType.FromSystemTypeInternal(typeof(Page));
			}
			return _pageDType;
		}
	}

	private static DependencyObjectType PageFunctionBaseDType
	{
		get
		{
			if (_pageFunctionBaseDType == null)
			{
				_pageFunctionBaseDType = DependencyObjectType.FromSystemTypeInternal(typeof(PageFunctionBase));
			}
			return _pageFunctionBaseDType;
		}
	}

	internal override int EffectiveValuesInitialSize => 7;

	/// <summary>Gets the logical parent  element of this element. </summary>
	/// <returns>This element's logical parent.</returns>
	public DependencyObject Parent => ContextVerifiedGetParent();

	/// <summary> Gets an enumerator for logical child elements of this element. </summary>
	/// <returns>An enumerator for logical child elements of this element.</returns>
	protected internal virtual IEnumerator LogicalChildren => null;

	internal bool ThisHasLoadedChangeEventHandler
	{
		get
		{
			if (base.EventHandlersStore != null && (base.EventHandlersStore.Contains(LoadedEvent) || base.EventHandlersStore.Contains(UnloadedEvent)))
			{
				return true;
			}
			if (Style != null && Style.HasLoadedChangeHandler)
			{
				return true;
			}
			if (ThemeStyle != null && ThemeStyle.HasLoadedChangeHandler)
			{
				return true;
			}
			if (TemplateInternal != null && TemplateInternal.HasLoadedChangeHandler)
			{
				return true;
			}
			if (HasFefLoadedChangeHandler)
			{
				return true;
			}
			return false;
		}
	}

	internal bool HasFefLoadedChangeHandler
	{
		get
		{
			if (TemplatedParent == null)
			{
				return false;
			}
			FrameworkElementFactory fEFTreeRoot = BroadcastEventHelper.GetFEFTreeRoot(TemplatedParent);
			if (fEFTreeRoot == null)
			{
				return false;
			}
			return StyleHelper.FindFEF(fEFTreeRoot, TemplateChildIndex)?.HasLoadedChangeHandler ?? false;
		}
	}

	internal override DependencyObject InheritanceContext => InheritanceContextField.GetValue(this);

	internal bool IsStyleUpdateInProgress
	{
		get
		{
			return ReadInternalFlag(InternalFlags.IsStyleUpdateInProgress);
		}
		set
		{
			WriteInternalFlag(InternalFlags.IsStyleUpdateInProgress, value);
		}
	}

	internal bool IsThemeStyleUpdateInProgress
	{
		get
		{
			return ReadInternalFlag(InternalFlags.IsThemeStyleUpdateInProgress);
		}
		set
		{
			WriteInternalFlag(InternalFlags.IsThemeStyleUpdateInProgress, value);
		}
	}

	internal bool StoresParentTemplateValues
	{
		get
		{
			return ReadInternalFlag(InternalFlags.StoresParentTemplateValues);
		}
		set
		{
			WriteInternalFlag(InternalFlags.StoresParentTemplateValues, value);
		}
	}

	internal bool HasNumberSubstitutionChanged
	{
		get
		{
			return ReadInternalFlag(InternalFlags.HasNumberSubstitutionChanged);
		}
		set
		{
			WriteInternalFlag(InternalFlags.HasNumberSubstitutionChanged, value);
		}
	}

	internal bool HasTemplateGeneratedSubTree
	{
		get
		{
			return ReadInternalFlag(InternalFlags.HasTemplateGeneratedSubTree);
		}
		set
		{
			WriteInternalFlag(InternalFlags.HasTemplateGeneratedSubTree, value);
		}
	}

	internal bool HasImplicitStyleFromResources
	{
		get
		{
			return ReadInternalFlag(InternalFlags.HasImplicitStyleFromResources);
		}
		set
		{
			WriteInternalFlag(InternalFlags.HasImplicitStyleFromResources, value);
		}
	}

	internal bool ShouldLookupImplicitStyles
	{
		get
		{
			return ReadInternalFlag(InternalFlags.ShouldLookupImplicitStyles);
		}
		set
		{
			WriteInternalFlag(InternalFlags.ShouldLookupImplicitStyles, value);
		}
	}

	internal bool IsStyleSetFromGenerator
	{
		get
		{
			return ReadInternalFlag2(InternalFlags2.IsStyleSetFromGenerator);
		}
		set
		{
			WriteInternalFlag2(InternalFlags2.IsStyleSetFromGenerator, value);
		}
	}

	internal bool HasStyleChanged
	{
		get
		{
			return ReadInternalFlag2(InternalFlags2.HasStyleChanged);
		}
		set
		{
			WriteInternalFlag2(InternalFlags2.HasStyleChanged, value);
		}
	}

	internal bool HasTemplateChanged
	{
		get
		{
			return ReadInternalFlag2(InternalFlags2.HasTemplateChanged);
		}
		set
		{
			WriteInternalFlag2(InternalFlags2.HasTemplateChanged, value);
		}
	}

	internal bool HasStyleInvalidated
	{
		get
		{
			return ReadInternalFlag2(InternalFlags2.HasStyleInvalidated);
		}
		set
		{
			WriteInternalFlag2(InternalFlags2.HasStyleInvalidated, value);
		}
	}

	internal bool HasStyleEverBeenFetched
	{
		get
		{
			return ReadInternalFlag(InternalFlags.HasStyleEverBeenFetched);
		}
		set
		{
			WriteInternalFlag(InternalFlags.HasStyleEverBeenFetched, value);
		}
	}

	internal bool HasLocalStyle
	{
		get
		{
			return ReadInternalFlag(InternalFlags.HasLocalStyle);
		}
		set
		{
			WriteInternalFlag(InternalFlags.HasLocalStyle, value);
		}
	}

	internal bool HasThemeStyleEverBeenFetched
	{
		get
		{
			return ReadInternalFlag(InternalFlags.HasThemeStyleEverBeenFetched);
		}
		set
		{
			WriteInternalFlag(InternalFlags.HasThemeStyleEverBeenFetched, value);
		}
	}

	internal bool AncestorChangeInProgress
	{
		get
		{
			return ReadInternalFlag(InternalFlags.AncestorChangeInProgress);
		}
		set
		{
			WriteInternalFlag(InternalFlags.AncestorChangeInProgress, value);
		}
	}

	internal FrugalObjectList<DependencyProperty> InheritableProperties
	{
		get
		{
			return _inheritableProperties;
		}
		set
		{
			_inheritableProperties = value;
		}
	}

	internal object[] LoadedPending => (object[])GetValue(LoadedPendingProperty);

	internal object[] UnloadedPending => (object[])GetValue(UnloadedPendingProperty);

	internal override bool HasMultipleInheritanceContexts => ReadInternalFlag2(InternalFlags2.HasMultipleInheritanceContexts);

	internal bool PotentiallyHasMentees
	{
		get
		{
			return ReadInternalFlag(InternalFlags.PotentiallyHasMentees);
		}
		set
		{
			WriteInternalFlag(InternalFlags.PotentiallyHasMentees, value);
		}
	}

	/// <summary>Occurs when the target value changes for any property binding on this element. </summary>
	public event EventHandler<DataTransferEventArgs> TargetUpdated
	{
		add
		{
			AddHandler(Binding.TargetUpdatedEvent, value);
		}
		remove
		{
			RemoveHandler(Binding.TargetUpdatedEvent, value);
		}
	}

	/// <summary>Occurs when the source value changes for any existing property binding on this element.</summary>
	public event EventHandler<DataTransferEventArgs> SourceUpdated
	{
		add
		{
			AddHandler(Binding.SourceUpdatedEvent, value);
		}
		remove
		{
			RemoveHandler(Binding.SourceUpdatedEvent, value);
		}
	}

	/// <summary>Occurs when the data context for this element changes. </summary>
	public event DependencyPropertyChangedEventHandler DataContextChanged
	{
		add
		{
			EventHandlersStoreAdd(DataContextChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(DataContextChangedKey, value);
		}
	}

	/// <summary>Occurs when <see cref="M:System.Windows.FrameworkElement.BringIntoView(System.Windows.Rect)" /> is called on this element. </summary>
	public event RequestBringIntoViewEventHandler RequestBringIntoView
	{
		add
		{
			AddHandler(RequestBringIntoViewEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(RequestBringIntoViewEvent, value);
		}
	}

	/// <summary>Occurs when either the <see cref="P:System.Windows.FrameworkElement.ActualHeight" /> or the <see cref="P:System.Windows.FrameworkElement.ActualWidth" /> properties change value on this element. </summary>
	public event SizeChangedEventHandler SizeChanged
	{
		add
		{
			AddHandler(SizeChangedEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(SizeChangedEvent, value);
		}
	}

	/// <summary>Occurs when this <see cref="T:System.Windows.FrameworkElement" /> is initialized. This event coincides with cases where the value of the <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> property changes from false (or undefined) to true. </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public event EventHandler Initialized
	{
		add
		{
			EventHandlersStoreAdd(InitializedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(InitializedKey, value);
		}
	}

	/// <summary>Occurs when the element is laid out, rendered, and ready for interaction. </summary>
	public event RoutedEventHandler Loaded
	{
		add
		{
			AddHandler(LoadedEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(LoadedEvent, value);
		}
	}

	/// <summary>Occurs when the element is removed from within an element tree of loaded elements. </summary>
	public event RoutedEventHandler Unloaded
	{
		add
		{
			AddHandler(UnloadedEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(UnloadedEvent, value);
		}
	}

	/// <summary>Occurs when any tooltip on the element is opened. </summary>
	public event ToolTipEventHandler ToolTipOpening
	{
		add
		{
			AddHandler(ToolTipOpeningEvent, value);
		}
		remove
		{
			RemoveHandler(ToolTipOpeningEvent, value);
		}
	}

	/// <summary>Occurs just before any tooltip on the element is closed. </summary>
	public event ToolTipEventHandler ToolTipClosing
	{
		add
		{
			AddHandler(ToolTipClosingEvent, value);
		}
		remove
		{
			RemoveHandler(ToolTipClosingEvent, value);
		}
	}

	/// <summary>Occurs when any context menu on the element is opened. </summary>
	public event ContextMenuEventHandler ContextMenuOpening
	{
		add
		{
			AddHandler(ContextMenuOpeningEvent, value);
		}
		remove
		{
			RemoveHandler(ContextMenuOpeningEvent, value);
		}
	}

	/// <summary>Occurs just before any context menu on the element is closed. </summary>
	public event ContextMenuEventHandler ContextMenuClosing
	{
		add
		{
			AddHandler(ContextMenuClosingEvent, value);
		}
		remove
		{
			RemoveHandler(ContextMenuClosingEvent, value);
		}
	}

	internal event EventHandler ResourcesChanged
	{
		add
		{
			PotentiallyHasMentees = true;
			EventHandlersStoreAdd(ResourcesChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(ResourcesChangedKey, value);
		}
	}

	internal event InheritedPropertyChangedEventHandler InheritedPropertyChanged
	{
		add
		{
			PotentiallyHasMentees = true;
			EventHandlersStoreAdd(InheritedPropertyChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(InheritedPropertyChangedKey, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkElement" /> class. </summary>
	public FrameworkElement()
	{
		PropertyMetadata metadata = StyleProperty.GetMetadata(base.DependencyObjectType);
		Style style = (Style)metadata.DefaultValue;
		if (style != null)
		{
			OnStyleChanged(this, new DependencyPropertyChangedEventArgs(StyleProperty, metadata, null, style));
		}
		if ((FlowDirection)FlowDirectionProperty.GetDefaultValue(base.DependencyObjectType) == FlowDirection.RightToLeft)
		{
			IsRightToLeft = true;
		}
		Application current = Application.Current;
		if (current != null && current.HasImplicitStylesInResources)
		{
			ShouldLookupImplicitStyles = true;
		}
		EnsureFrameworkServices();
	}

	/// <summary>Returns whether serialization processes should serialize the contents of the <see cref="P:System.Windows.FrameworkElement.Style" /> property.</summary>
	/// <returns>true if the <see cref="P:System.Windows.FrameworkElement.Style" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeStyle()
	{
		if (!IsStyleSetFromGenerator)
		{
			return ReadLocalValue(StyleProperty) != DependencyProperty.UnsetValue;
		}
		return false;
	}

	private static void OnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		FrameworkElement frameworkElement = (FrameworkElement)d;
		frameworkElement.HasLocalStyle = e.NewEntry.BaseValueSourceInternal == BaseValueSourceInternal.Local;
		StyleHelper.UpdateStyleCache(frameworkElement, null, (Style)e.OldValue, (Style)e.NewValue, ref frameworkElement._styleCache);
	}

	private static void OnUseLayoutRoundingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		FrameworkElement obj = (FrameworkElement)d;
		bool value = (bool)e.NewValue;
		obj.SetFlags(value, VisualFlags.UseLayoutRounding);
	}

	private static void OnThemeStyleKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((FrameworkElement)d).UpdateThemeStyleProperty();
	}

	internal static void OnThemeStyleChanged(DependencyObject d, object oldValue, object newValue)
	{
		FrameworkElement frameworkElement = (FrameworkElement)d;
		StyleHelper.UpdateThemeStyleCache(frameworkElement, null, (Style)oldValue, (Style)newValue, ref frameworkElement._themeStyleCache);
	}

	internal virtual void OnTemplateChangedInternal(FrameworkTemplate oldTemplate, FrameworkTemplate newTemplate)
	{
		HasTemplateChanged = true;
	}

	/// <summary>Invoked when the style in use on this element changes, which will invalidate the layout. </summary>
	/// <param name="oldStyle">The old style.</param>
	/// <param name="newStyle">The new style.</param>
	protected internal virtual void OnStyleChanged(Style oldStyle, Style newStyle)
	{
		HasStyleChanged = true;
	}

	/// <summary> Supports incremental layout implementations in specialized subclasses of <see cref="T:System.Windows.FrameworkElement" />. <see cref="M:System.Windows.FrameworkElement.ParentLayoutInvalidated(System.Windows.UIElement)" />  is invoked when a child element has invalidated a property that is marked in metadata as affecting the parent's measure or arrange passes during layout. </summary>
	/// <param name="child">The child element reporting the change.</param>
	protected internal virtual void ParentLayoutInvalidated(UIElement child)
	{
	}

	/// <summary>Builds the current template's visual tree if necessary, and returns a value that indicates whether the visual tree was rebuilt by this call. </summary>
	/// <returns>true if visuals were added to the tree; returns false otherwise.</returns>
	public bool ApplyTemplate()
	{
		OnPreApplyTemplate();
		bool flag = false;
		UncommonField<HybridDictionary[]> templateDataField = StyleHelper.TemplateDataField;
		FrameworkTemplate templateInternal = TemplateInternal;
		int num = 2;
		int num2 = 0;
		while (templateInternal != null && num2 < num && !HasTemplateGeneratedSubTree)
		{
			flag = templateInternal.ApplyTemplateContent(templateDataField, this);
			if (flag)
			{
				HasTemplateGeneratedSubTree = true;
				StyleHelper.InvokeDeferredActions(this, templateInternal);
				OnApplyTemplate();
			}
			if (templateInternal == TemplateInternal)
			{
				break;
			}
			templateInternal = TemplateInternal;
			num2++;
		}
		OnPostApplyTemplate();
		return flag;
	}

	internal virtual void OnPreApplyTemplate()
	{
	}

	/// <summary>When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.</summary>
	public virtual void OnApplyTemplate()
	{
	}

	internal virtual void OnPostApplyTemplate()
	{
	}

	/// <summary>Begins the sequence of actions that are contained in the provided storyboard. </summary>
	/// <param name="storyboard">The storyboard to begin.</param>
	public void BeginStoryboard(Storyboard storyboard)
	{
		BeginStoryboard(storyboard, HandoffBehavior.SnapshotAndReplace, isControllable: false);
	}

	/// <summary>Begins the sequence of actions contained in the provided storyboard, with options specified for what should happen if the property is already animated. </summary>
	/// <param name="storyboard">The storyboard to begin.</param>
	/// <param name="handoffBehavior">A value of the enumeration that describes behavior to use if a property described in the storyboard is already animated.</param>
	public void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior)
	{
		BeginStoryboard(storyboard, handoffBehavior, isControllable: false);
	}

	/// <summary> Begins the sequence of actions contained in the provided storyboard, with specified state for control of the animation after it is started. </summary>
	/// <param name="storyboard">The storyboard to begin. </param>
	/// <param name="handoffBehavior">A value of the enumeration that describes behavior to use if a property described in the storyboard is already animated.</param>
	/// <param name="isControllable">Declares whether the animation is controllable (can be paused) after it is started.</param>
	public void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior, bool isControllable)
	{
		if (storyboard == null)
		{
			throw new ArgumentNullException("storyboard");
		}
		storyboard.Begin(this, handoffBehavior, isControllable);
	}

	internal static FrameworkElement FindNamedFrameworkElement(FrameworkElement startElement, string targetName)
	{
		FrameworkElement frameworkElement = null;
		if (targetName == null || targetName.Length == 0)
		{
			return startElement;
		}
		DependencyObject dependencyObject = null;
		dependencyObject = LogicalTreeHelper.FindLogicalNode(startElement, targetName);
		if (dependencyObject == null)
		{
			throw new ArgumentException(SR.Format(SR.TargetNameNotFound, targetName));
		}
		FrameworkObject frameworkObject = new FrameworkObject(dependencyObject);
		if (frameworkObject.IsFE)
		{
			return frameworkObject.FE;
		}
		throw new InvalidOperationException(SR.Format(SR.NamedObjectMustBeFrameworkElement, targetName));
	}

	/// <summary>Returns whether serialization processes should serialize the contents of the <see cref="P:System.Windows.FrameworkElement.Triggers" /> property.</summary>
	/// <returns>true if the <see cref="P:System.Windows.FrameworkElement.Triggers" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeTriggers()
	{
		TriggerCollection value = EventTrigger.TriggerCollectionField.GetValue(this);
		if (value == null || value.Count == 0)
		{
			return false;
		}
		return true;
	}

	private void PrivateInitialized()
	{
		EventTrigger.ProcessTriggerCollection(this);
	}

	/// <summary>Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)" />, and returns a child at the specified index from a collection of child elements. </summary>
	/// <returns>The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.</returns>
	/// <param name="index">The zero-based index of the requested child element in the collection.</param>
	protected override Visual GetVisualChild(int index)
	{
		if (_templateChild == null)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		if (index != 0)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _templateChild;
	}

	/// <summary>For a description of this member, see the <see cref="M:System.Windows.Markup.IQueryAmbient.IsAmbientPropertyAvailable(System.String)" /> method.</summary>
	/// <returns>true if <paramref name="propertyName" /> is available; otherwise, false. </returns>
	/// <param name="propertyName">The name of the requested ambient property.</param>
	bool IQueryAmbient.IsAmbientPropertyAvailable(string propertyName)
	{
		if (!(propertyName != "Resources"))
		{
			return HasResources;
		}
		return true;
	}

	/// <summary>Returns whether serialization processes should serialize the contents of the <see cref="P:System.Windows.FrameworkElement.Resources" /> property. </summary>
	/// <returns>true if the <see cref="P:System.Windows.FrameworkElement.Resources" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeResources()
	{
		if (Resources == null || Resources.Count == 0)
		{
			return false;
		}
		return true;
	}

	/// <summary>Returns the named element in the visual tree of an instantiated <see cref="T:System.Windows.Controls.ControlTemplate" />.</summary>
	/// <returns>The requested element. May be null if no element of the requested name exists.</returns>
	/// <param name="childName">Name of the child to find.</param>
	protected internal DependencyObject GetTemplateChild(string childName)
	{
		FrameworkTemplate templateInternal = TemplateInternal;
		if (templateInternal == null)
		{
			return null;
		}
		return StyleHelper.FindNameInTemplateContent(this, childName, templateInternal) as DependencyObject;
	}

	/// <summary>Searches for a resource with the specified key, and throws an exception if the requested resource is not found. </summary>
	/// <returns>The requested resource. If no resource with the provided key was found, an exception is thrown. An <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> value might also be returned in the exception case.</returns>
	/// <param name="resourceKey">The key identifier for the requested resource.</param>
	/// <exception cref="T:System.Windows.ResourceReferenceKeyNotFoundException">
	///   <paramref name="resourceKey" /> was not found and an event handler does not exist for the <see cref="E:System.Windows.Threading.Dispatcher.UnhandledException" /> event.-or-<paramref name="resourceKey" /> was not found and the <see cref="P:System.Windows.Threading.DispatcherUnhandledExceptionEventArgs.Handled" /> property is false in the <see cref="E:System.Windows.Threading.Dispatcher.UnhandledException" /> event.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="resourceKey" /> is null.</exception>
	public object FindResource(object resourceKey)
	{
		if (resourceKey == null)
		{
			throw new ArgumentNullException("resourceKey");
		}
		object obj = FindResourceInternal(this, null, resourceKey);
		if (obj == DependencyProperty.UnsetValue)
		{
			Helper.ResourceFailureThrow(resourceKey);
		}
		return obj;
	}

	/// <summary>Searches for a resource with the specified key, and returns that resource if found. </summary>
	/// <returns>The found resource, or null if no resource with the provided <paramref name="key" /> is found.</returns>
	/// <param name="resourceKey">The key identifier of the resource to be found.</param>
	public object TryFindResource(object resourceKey)
	{
		if (resourceKey == null)
		{
			throw new ArgumentNullException("resourceKey");
		}
		object obj = FindResourceInternal(this, null, resourceKey);
		if (obj == DependencyProperty.UnsetValue)
		{
			obj = null;
		}
		return obj;
	}

	internal static object FindImplicitStyleResource(FrameworkElement fe, object resourceKey, out object source)
	{
		if (fe.ShouldLookupImplicitStyles)
		{
			object unlinkedParent = null;
			bool allowDeferredResourceReference = false;
			bool mustReturnDeferredResourceReference = false;
			bool isImplicitStyleLookup = true;
			DependencyObject boundaryElement = null;
			if (!(fe is Control))
			{
				boundaryElement = fe.TemplatedParent;
			}
			return FindResourceInternal(fe, null, StyleProperty, resourceKey, unlinkedParent, allowDeferredResourceReference, mustReturnDeferredResourceReference, boundaryElement, isImplicitStyleLookup, out source);
		}
		source = null;
		return DependencyProperty.UnsetValue;
	}

	internal static object FindImplicitStyleResource(FrameworkContentElement fce, object resourceKey, out object source)
	{
		if (fce.ShouldLookupImplicitStyles)
		{
			object unlinkedParent = null;
			bool allowDeferredResourceReference = false;
			bool mustReturnDeferredResourceReference = false;
			bool isImplicitStyleLookup = true;
			DependencyObject templatedParent = fce.TemplatedParent;
			return FindResourceInternal(null, fce, FrameworkContentElement.StyleProperty, resourceKey, unlinkedParent, allowDeferredResourceReference, mustReturnDeferredResourceReference, templatedParent, isImplicitStyleLookup, out source);
		}
		source = null;
		return DependencyProperty.UnsetValue;
	}

	internal static object FindResourceInternal(FrameworkElement fe, FrameworkContentElement fce, object resourceKey)
	{
		object source;
		return FindResourceInternal(fe, fce, null, resourceKey, null, allowDeferredResourceReference: false, mustReturnDeferredResourceReference: false, null, isImplicitStyleLookup: false, out source);
	}

	internal static object FindResourceFromAppOrSystem(object resourceKey, out object source, bool disableThrowOnResourceNotFound, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference)
	{
		return FindResourceInternal(null, null, null, resourceKey, null, allowDeferredResourceReference, mustReturnDeferredResourceReference, null, disableThrowOnResourceNotFound, out source);
	}

	internal static object FindResourceInternal(FrameworkElement fe, FrameworkContentElement fce, DependencyProperty dp, object resourceKey, object unlinkedParent, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference, DependencyObject boundaryElement, bool isImplicitStyleLookup, out object source)
	{
		InheritanceBehavior inheritanceBehavior = InheritanceBehavior.Default;
		if (TraceResourceDictionary.IsEnabled)
		{
			FrameworkObject frameworkObject = new FrameworkObject(fe, fce);
			TraceResourceDictionary.Trace(TraceEventType.Start, TraceResourceDictionary.FindResource, frameworkObject.DO, resourceKey);
		}
		try
		{
			if (fe != null || fce != null || unlinkedParent != null)
			{
				object obj = FindResourceInTree(fe, fce, dp, resourceKey, unlinkedParent, allowDeferredResourceReference, mustReturnDeferredResourceReference, boundaryElement, out inheritanceBehavior, out source);
				if (obj != DependencyProperty.UnsetValue)
				{
					return obj;
				}
			}
			Application current = Application.Current;
			if (current != null && (inheritanceBehavior == InheritanceBehavior.Default || inheritanceBehavior == InheritanceBehavior.SkipToAppNow || inheritanceBehavior == InheritanceBehavior.SkipToAppNext))
			{
				object obj = current.FindResourceInternal(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
				if (obj != null)
				{
					source = current;
					if (TraceResourceDictionary.IsEnabled)
					{
						TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceInApplication, resourceKey, obj);
					}
					return obj;
				}
			}
			if (!isImplicitStyleLookup && inheritanceBehavior != InheritanceBehavior.SkipAllNow && inheritanceBehavior != InheritanceBehavior.SkipAllNext)
			{
				object obj = SystemResources.FindResourceInternal(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
				if (obj != null)
				{
					source = SystemResourceHost.Instance;
					if (TraceResourceDictionary.IsEnabled)
					{
						TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceInTheme, source, resourceKey, obj);
					}
					return obj;
				}
			}
		}
		finally
		{
			if (TraceResourceDictionary.IsEnabled)
			{
				FrameworkObject frameworkObject2 = new FrameworkObject(fe, fce);
				TraceResourceDictionary.Trace(TraceEventType.Stop, TraceResourceDictionary.FindResource, frameworkObject2.DO, resourceKey);
			}
		}
		if (TraceResourceDictionary.IsEnabledOverride && !isImplicitStyleLookup)
		{
			if ((fe != null && fe.IsLoaded) || (fce != null && fce.IsLoaded))
			{
				TraceResourceDictionary.Trace(TraceEventType.Warning, TraceResourceDictionary.ResourceNotFound, resourceKey);
			}
			else if (TraceResourceDictionary.IsEnabled)
			{
				TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.ResourceNotFound, resourceKey);
			}
		}
		source = null;
		return DependencyProperty.UnsetValue;
	}

	internal static object FindResourceInTree(FrameworkElement feStart, FrameworkContentElement fceStart, DependencyProperty dp, object resourceKey, object unlinkedParent, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference, DependencyObject boundaryElement, out InheritanceBehavior inheritanceBehavior, out object source)
	{
		FrameworkObject frameworkObject = new FrameworkObject(feStart, fceStart);
		FrameworkObject frameworkObject2 = frameworkObject;
		int num = 0;
		bool flag = true;
		inheritanceBehavior = InheritanceBehavior.Default;
		while (flag)
		{
			if (num > ContextLayoutManager.s_LayoutRecursionLimit)
			{
				throw new InvalidOperationException(SR.LogicalTreeLoop);
			}
			num++;
			Style style = null;
			FrameworkTemplate frameworkTemplate = null;
			Style style2 = null;
			if (frameworkObject2.IsFE)
			{
				FrameworkElement fE = frameworkObject2.FE;
				object obj = fE.FindResourceOnSelf(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
				if (obj != DependencyProperty.UnsetValue)
				{
					source = fE;
					if (TraceResourceDictionary.IsEnabled)
					{
						TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceOnElement, source, resourceKey, obj);
					}
					return obj;
				}
				if (fE != frameworkObject.FE || StyleHelper.ShouldGetValueFromStyle(dp))
				{
					style = fE.Style;
				}
				if (fE != frameworkObject.FE || StyleHelper.ShouldGetValueFromTemplate(dp))
				{
					frameworkTemplate = fE.TemplateInternal;
				}
				if (fE != frameworkObject.FE || StyleHelper.ShouldGetValueFromThemeStyle(dp))
				{
					style2 = fE.ThemeStyle;
				}
			}
			else if (frameworkObject2.IsFCE)
			{
				FrameworkContentElement fCE = frameworkObject2.FCE;
				object obj = fCE.FindResourceOnSelf(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
				if (obj != DependencyProperty.UnsetValue)
				{
					source = fCE;
					if (TraceResourceDictionary.IsEnabled)
					{
						TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceOnElement, source, resourceKey, obj);
					}
					return obj;
				}
				if (fCE != frameworkObject.FCE || StyleHelper.ShouldGetValueFromStyle(dp))
				{
					style = fCE.Style;
				}
				if (fCE != frameworkObject.FCE || StyleHelper.ShouldGetValueFromThemeStyle(dp))
				{
					style2 = fCE.ThemeStyle;
				}
			}
			if (style != null)
			{
				object obj = style.FindResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
				if (obj != DependencyProperty.UnsetValue)
				{
					source = style;
					if (TraceResourceDictionary.IsEnabled)
					{
						TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceInStyle, style.Resources, resourceKey, style, frameworkObject2.DO, obj);
					}
					return obj;
				}
			}
			if (frameworkTemplate != null)
			{
				object obj = frameworkTemplate.FindResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
				if (obj != DependencyProperty.UnsetValue)
				{
					source = frameworkTemplate;
					if (TraceResourceDictionary.IsEnabled)
					{
						TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceInTemplate, frameworkTemplate.Resources, resourceKey, frameworkTemplate, frameworkObject2.DO, obj);
					}
					return obj;
				}
			}
			if (style2 != null)
			{
				object obj = style2.FindResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
				if (obj != DependencyProperty.UnsetValue)
				{
					source = style2;
					if (TraceResourceDictionary.IsEnabled)
					{
						TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceInThemeStyle, style2.Resources, resourceKey, style2, frameworkObject2.DO, obj);
					}
					return obj;
				}
			}
			if (boundaryElement != null && frameworkObject2.DO == boundaryElement)
			{
				break;
			}
			if (frameworkObject2.IsValid && TreeWalkHelper.SkipNext(frameworkObject2.InheritanceBehavior))
			{
				inheritanceBehavior = frameworkObject2.InheritanceBehavior;
				break;
			}
			if (unlinkedParent != null)
			{
				if (unlinkedParent is DependencyObject d)
				{
					frameworkObject2.Reset(d);
					if (frameworkObject2.IsValid)
					{
						flag = true;
					}
					else
					{
						DependencyObject frameworkParent = GetFrameworkParent(unlinkedParent);
						if (frameworkParent != null)
						{
							frameworkObject2.Reset(frameworkParent);
							flag = true;
						}
						else
						{
							flag = false;
						}
					}
				}
				else
				{
					flag = false;
				}
				unlinkedParent = null;
			}
			else
			{
				frameworkObject2 = frameworkObject2.FrameworkParent;
				flag = frameworkObject2.IsValid;
			}
			if (frameworkObject2.IsValid && TreeWalkHelper.SkipNow(frameworkObject2.InheritanceBehavior))
			{
				inheritanceBehavior = frameworkObject2.InheritanceBehavior;
				break;
			}
		}
		source = null;
		return DependencyProperty.UnsetValue;
	}

	internal static object FindTemplateResourceInternal(DependencyObject target, object item, Type templateType)
	{
		if (item == null || item is UIElement)
		{
			return null;
		}
		Type type;
		object obj = ContentPresenter.DataTypeForItem(item, target, out type);
		ArrayList arrayList = new ArrayList();
		int num = -1;
		while (obj != null)
		{
			object obj2 = null;
			if (templateType == typeof(ItemContainerTemplate))
			{
				obj2 = new ItemContainerTemplateKey(obj);
			}
			else if (templateType == typeof(DataTemplate))
			{
				obj2 = new DataTemplateKey(obj);
			}
			if (obj2 != null)
			{
				arrayList.Add(obj2);
			}
			if (num == -1)
			{
				num = arrayList.Count;
			}
			if (type != null)
			{
				type = type.BaseType;
				if (type == typeof(object))
				{
					type = null;
				}
			}
			obj = type;
		}
		int bestMatch = arrayList.Count;
		object result = FindTemplateResourceInTree(target, arrayList, num, ref bestMatch);
		if (bestMatch >= num)
		{
			object obj3 = Helper.FindTemplateResourceFromAppOrSystem(target, arrayList, num, ref bestMatch);
			if (obj3 != null)
			{
				result = obj3;
			}
		}
		return result;
	}

	private static object FindTemplateResourceInTree(DependencyObject target, ArrayList keys, int exactMatch, ref int bestMatch)
	{
		object result = null;
		FrameworkObject frameworkObject = new FrameworkObject(target);
		while (frameworkObject.IsValid)
		{
			ResourceDictionary instanceResourceDictionary = GetInstanceResourceDictionary(frameworkObject.FE, frameworkObject.FCE);
			if (instanceResourceDictionary != null)
			{
				object obj = FindBestMatchInResourceDictionary(instanceResourceDictionary, keys, exactMatch, ref bestMatch);
				if (obj != null)
				{
					result = obj;
					if (bestMatch < exactMatch)
					{
						return result;
					}
				}
			}
			instanceResourceDictionary = GetStyleResourceDictionary(frameworkObject.FE, frameworkObject.FCE);
			if (instanceResourceDictionary != null)
			{
				object obj = FindBestMatchInResourceDictionary(instanceResourceDictionary, keys, exactMatch, ref bestMatch);
				if (obj != null)
				{
					result = obj;
					if (bestMatch < exactMatch)
					{
						return result;
					}
				}
			}
			instanceResourceDictionary = GetThemeStyleResourceDictionary(frameworkObject.FE, frameworkObject.FCE);
			if (instanceResourceDictionary != null)
			{
				object obj = FindBestMatchInResourceDictionary(instanceResourceDictionary, keys, exactMatch, ref bestMatch);
				if (obj != null)
				{
					result = obj;
					if (bestMatch < exactMatch)
					{
						return result;
					}
				}
			}
			instanceResourceDictionary = GetTemplateResourceDictionary(frameworkObject.FE, frameworkObject.FCE);
			if (instanceResourceDictionary != null)
			{
				object obj = FindBestMatchInResourceDictionary(instanceResourceDictionary, keys, exactMatch, ref bestMatch);
				if (obj != null)
				{
					result = obj;
					if (bestMatch < exactMatch)
					{
						return result;
					}
				}
			}
			if (frameworkObject.IsValid && TreeWalkHelper.SkipNext(frameworkObject.InheritanceBehavior))
			{
				break;
			}
			frameworkObject = frameworkObject.FrameworkParent;
			if (frameworkObject.IsValid && TreeWalkHelper.SkipNext(frameworkObject.InheritanceBehavior))
			{
				break;
			}
		}
		return result;
	}

	private static object FindBestMatchInResourceDictionary(ResourceDictionary table, ArrayList keys, int exactMatch, ref int bestMatch)
	{
		object result = null;
		if (table != null)
		{
			for (int i = 0; i < bestMatch; i++)
			{
				object obj = table[keys[i]];
				if (obj != null)
				{
					result = obj;
					bestMatch = i;
					if (bestMatch < exactMatch)
					{
						return result;
					}
				}
			}
		}
		return result;
	}

	private static ResourceDictionary GetInstanceResourceDictionary(FrameworkElement fe, FrameworkContentElement fce)
	{
		ResourceDictionary result = null;
		if (fe != null)
		{
			if (fe.HasResources)
			{
				result = fe.Resources;
			}
		}
		else if (fce.HasResources)
		{
			result = fce.Resources;
		}
		return result;
	}

	private static ResourceDictionary GetStyleResourceDictionary(FrameworkElement fe, FrameworkContentElement fce)
	{
		ResourceDictionary result = null;
		if (fe != null)
		{
			if (fe.Style != null && fe.Style._resources != null)
			{
				result = fe.Style._resources;
			}
		}
		else if (fce.Style != null && fce.Style._resources != null)
		{
			result = fce.Style._resources;
		}
		return result;
	}

	private static ResourceDictionary GetThemeStyleResourceDictionary(FrameworkElement fe, FrameworkContentElement fce)
	{
		ResourceDictionary result = null;
		if (fe != null)
		{
			if (fe.ThemeStyle != null && fe.ThemeStyle._resources != null)
			{
				result = fe.ThemeStyle._resources;
			}
		}
		else if (fce.ThemeStyle != null && fce.ThemeStyle._resources != null)
		{
			result = fce.ThemeStyle._resources;
		}
		return result;
	}

	private static ResourceDictionary GetTemplateResourceDictionary(FrameworkElement fe, FrameworkContentElement fce)
	{
		ResourceDictionary result = null;
		if (fe != null && fe.TemplateInternal != null && fe.TemplateInternal._resources != null)
		{
			result = fe.TemplateInternal._resources;
		}
		return result;
	}

	internal bool HasNonDefaultValue(DependencyProperty dp)
	{
		return !Helper.HasDefaultValue(this, dp);
	}

	internal static INameScope FindScope(DependencyObject d)
	{
		DependencyObject scopeOwner;
		return FindScope(d, out scopeOwner);
	}

	internal static INameScope FindScope(DependencyObject d, out DependencyObject scopeOwner)
	{
		while (d != null)
		{
			INameScope nameScope = NameScope.NameScopeFromObject(d);
			if (nameScope != null)
			{
				scopeOwner = d;
				return nameScope;
			}
			DependencyObject parent = LogicalTreeHelper.GetParent(d);
			d = ((parent != null) ? parent : Helper.FindMentor(d.InheritanceContext));
		}
		scopeOwner = null;
		return null;
	}

	/// <summary>Searches for a resource with the specified name and sets up a resource reference to it for the specified property. </summary>
	/// <param name="dp">The property to which the resource is bound.</param>
	/// <param name="name">The name of the resource.</param>
	public void SetResourceReference(DependencyProperty dp, object name)
	{
		SetValue(dp, new ResourceReferenceExpression(name));
		HasResourceReference = true;
	}

	internal sealed override void EvaluateBaseValueCore(DependencyProperty dp, PropertyMetadata metadata, ref EffectiveValueEntry newEntry)
	{
		if (dp == StyleProperty)
		{
			HasStyleEverBeenFetched = true;
			HasImplicitStyleFromResources = false;
			IsStyleSetFromGenerator = false;
		}
		GetRawValue(dp, metadata, ref newEntry);
		Storyboard.GetComplexPathValue(this, dp, ref newEntry, metadata);
	}

	internal void GetRawValue(DependencyProperty dp, PropertyMetadata metadata, ref EffectiveValueEntry entry)
	{
		if ((entry.BaseValueSourceInternal == BaseValueSourceInternal.Local && entry.GetFlattenedEntry(RequestFlags.FullyResolved).Value != DependencyProperty.UnsetValue) || (TemplateChildIndex != -1 && GetValueFromTemplatedParent(dp, ref entry)))
		{
			return;
		}
		if (dp != StyleProperty)
		{
			if (StyleHelper.GetValueFromStyleOrTemplate(new FrameworkObject(this, null), dp, ref entry))
			{
				return;
			}
		}
		else
		{
			object source;
			object obj = FindImplicitStyleResource(this, GetType(), out source);
			if (obj != DependencyProperty.UnsetValue)
			{
				HasImplicitStyleFromResources = true;
				entry.BaseValueSourceInternal = BaseValueSourceInternal.ImplicitReference;
				entry.Value = obj;
				return;
			}
		}
		if (metadata is FrameworkPropertyMetadata { Inherits: not false } frameworkPropertyMetadata)
		{
			object inheritableValue = GetInheritableValue(dp, frameworkPropertyMetadata);
			if (inheritableValue != DependencyProperty.UnsetValue)
			{
				entry.BaseValueSourceInternal = BaseValueSourceInternal.Inherited;
				entry.Value = inheritableValue;
			}
		}
	}

	private bool GetValueFromTemplatedParent(DependencyProperty dp, ref EffectiveValueEntry entry)
	{
		FrameworkTemplate frameworkTemplate = null;
		frameworkTemplate = ((FrameworkElement)_templatedParent).TemplateInternal;
		if (frameworkTemplate != null)
		{
			return StyleHelper.GetValueFromTemplatedParent(_templatedParent, TemplateChildIndex, new FrameworkObject(this, null), dp, ref frameworkTemplate.ChildRecordFromChildIndex, frameworkTemplate.VisualTree, ref entry);
		}
		return false;
	}

	private object GetInheritableValue(DependencyProperty dp, FrameworkPropertyMetadata fmetadata)
	{
		if (!TreeWalkHelper.SkipNext(InheritanceBehavior) || fmetadata.OverridesInheritanceBehavior)
		{
			InheritanceBehavior inheritanceBehavior = InheritanceBehavior.Default;
			FrameworkElement feParent;
			FrameworkContentElement fceParent;
			bool flag = GetFrameworkParent(this, out feParent, out fceParent);
			while (flag)
			{
				bool flag2 = ((feParent == null) ? TreeWalkHelper.IsInheritanceNode(fceParent, dp, out inheritanceBehavior) : TreeWalkHelper.IsInheritanceNode(feParent, dp, out inheritanceBehavior));
				if (TreeWalkHelper.SkipNow(inheritanceBehavior))
				{
					break;
				}
				if (flag2)
				{
					if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose))
					{
						string text = string.Format(CultureInfo.InvariantCulture, "[{0}]{1}({2})", GetType().Name, dp.Name, GetHashCode());
						EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientPropParentCheck, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose, GetHashCode(), text);
					}
					DependencyObject dependencyObject = feParent;
					if (dependencyObject == null)
					{
						dependencyObject = fceParent;
					}
					EntryIndex entryIndex = dependencyObject.LookupEntry(dp.GlobalIndex);
					return dependencyObject.GetValueEntry(entryIndex, dp, fmetadata, (RequestFlags)12).Value;
				}
				if (TreeWalkHelper.SkipNext(inheritanceBehavior))
				{
					break;
				}
				flag = ((feParent == null) ? GetFrameworkParent(fceParent, out feParent, out fceParent) : GetFrameworkParent(feParent, out feParent, out fceParent));
			}
		}
		return DependencyProperty.UnsetValue;
	}

	internal Expression GetExpressionCore(DependencyProperty dp, PropertyMetadata metadata)
	{
		IsRequestingExpression = true;
		EffectiveValueEntry newEntry = new EffectiveValueEntry(dp);
		newEntry.Value = DependencyProperty.UnsetValue;
		EvaluateBaseValueCore(dp, metadata, ref newEntry);
		IsRequestingExpression = false;
		return newEntry.Value as Expression;
	}

	/// <summary>Invoked whenever the effective value of any dependency property on this <see cref="T:System.Windows.FrameworkElement" /> has been updated. The specific dependency property that changed is reported in the arguments parameter. Overrides <see cref="M:System.Windows.DependencyObject.OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs)" />.</summary>
	/// <param name="e">The event data that describes the property that changed, as well as old and new values.</param>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		DependencyProperty property = e.Property;
		VisualDiagnostics.VerifyVisualTreeChange(this);
		base.OnPropertyChanged(e);
		if (e.IsAValueChange || e.IsASubPropertyChange)
		{
			if (property != null && property.OwnerType == typeof(PresentationSource) && property.Name == "RootSource")
			{
				TryFireInitialized();
			}
			if (property == NameProperty && EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose))
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.PerfElementIDName, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose, PerfService.GetPerfElementID(this), GetType().Name, GetValue(property));
			}
			if (property != StyleProperty && property != Control.TemplateProperty && property != DefaultStyleKeyProperty)
			{
				if (TemplatedParent != null)
				{
					FrameworkTemplate templateInternal = (TemplatedParent as FrameworkElement).TemplateInternal;
					if (templateInternal != null)
					{
						StyleHelper.OnTriggerSourcePropertyInvalidated(null, templateInternal, TemplatedParent, property, e, invalidateOnlyContainer: false, ref templateInternal.TriggerSourceRecordFromChildIndex, ref templateInternal.PropertyTriggersWithActions, TemplateChildIndex);
					}
				}
				if (Style != null)
				{
					StyleHelper.OnTriggerSourcePropertyInvalidated(Style, null, this, property, e, invalidateOnlyContainer: true, ref Style.TriggerSourceRecordFromChildIndex, ref Style.PropertyTriggersWithActions, 0);
				}
				if (TemplateInternal != null)
				{
					StyleHelper.OnTriggerSourcePropertyInvalidated(null, TemplateInternal, this, property, e, !HasTemplateGeneratedSubTree, ref TemplateInternal.TriggerSourceRecordFromChildIndex, ref TemplateInternal.PropertyTriggersWithActions, 0);
				}
				if (ThemeStyle != null && Style != ThemeStyle)
				{
					StyleHelper.OnTriggerSourcePropertyInvalidated(ThemeStyle, null, this, property, e, invalidateOnlyContainer: true, ref ThemeStyle.TriggerSourceRecordFromChildIndex, ref ThemeStyle.PropertyTriggersWithActions, 0);
				}
			}
		}
		if (!(e.Metadata is FrameworkPropertyMetadata frameworkPropertyMetadata))
		{
			return;
		}
		if (frameworkPropertyMetadata.Inherits && (InheritanceBehavior == InheritanceBehavior.Default || frameworkPropertyMetadata.OverridesInheritanceBehavior) && (!DependencyObject.IsTreeWalkOperation(e.OperationType) || PotentiallyHasMentees))
		{
			EffectiveValueEntry newEntry = e.NewEntry;
			EffectiveValueEntry oldEntry = e.OldEntry;
			if (oldEntry.BaseValueSourceInternal > newEntry.BaseValueSourceInternal)
			{
				newEntry = new EffectiveValueEntry(property, BaseValueSourceInternal.Inherited);
			}
			else
			{
				newEntry = newEntry.GetFlattenedEntry(RequestFlags.FullyResolved);
				newEntry.BaseValueSourceInternal = BaseValueSourceInternal.Inherited;
			}
			if (oldEntry.BaseValueSourceInternal != BaseValueSourceInternal.Default || oldEntry.HasModifiers)
			{
				oldEntry = oldEntry.GetFlattenedEntry(RequestFlags.FullyResolved);
				oldEntry.BaseValueSourceInternal = BaseValueSourceInternal.Inherited;
			}
			else
			{
				oldEntry = default(EffectiveValueEntry);
			}
			InheritablePropertyChangeInfo info = new InheritablePropertyChangeInfo(this, property, oldEntry, newEntry);
			if (!DependencyObject.IsTreeWalkOperation(e.OperationType))
			{
				TreeWalkHelper.InvalidateOnInheritablePropertyChange(this, null, info, skipStartNode: true);
			}
			if (PotentiallyHasMentees)
			{
				TreeWalkHelper.OnInheritedPropertyChanged(this, ref info, InheritanceBehavior);
			}
		}
		if ((!e.IsAValueChange && !e.IsASubPropertyChange) || (AncestorChangeInProgress && InVisibilityCollapsedTree))
		{
			return;
		}
		UIElement uIElement = null;
		bool affectsParentMeasure = frameworkPropertyMetadata.AffectsParentMeasure;
		bool affectsParentArrange = frameworkPropertyMetadata.AffectsParentArrange;
		bool affectsMeasure = frameworkPropertyMetadata.AffectsMeasure;
		bool affectsArrange = frameworkPropertyMetadata.AffectsArrange;
		if (affectsMeasure || affectsArrange || affectsParentArrange || affectsParentMeasure)
		{
			for (Visual visual = VisualTreeHelper.GetParent(this) as Visual; visual != null; visual = VisualTreeHelper.GetParent(visual) as Visual)
			{
				if (visual is UIElement uIElement2)
				{
					if (uIElement2 is FrameworkElement frameworkElement)
					{
						frameworkElement.ParentLayoutInvalidated(this);
					}
					if (affectsParentMeasure)
					{
						uIElement2.InvalidateMeasure();
					}
					if (affectsParentArrange)
					{
						uIElement2.InvalidateArrange();
					}
					break;
				}
			}
		}
		if (frameworkPropertyMetadata.AffectsMeasure && (!BypassLayoutPolicies || (property != WidthProperty && property != HeightProperty)))
		{
			InvalidateMeasure();
		}
		if (frameworkPropertyMetadata.AffectsArrange)
		{
			InvalidateArrange();
		}
		if (frameworkPropertyMetadata.AffectsRender && (e.IsAValueChange || !frameworkPropertyMetadata.SubPropertiesDoNotAffectRender))
		{
			InvalidateVisual();
		}
	}

	internal static DependencyObject GetFrameworkParent(object current)
	{
		return new FrameworkObject(current as DependencyObject).FrameworkParent.DO;
	}

	internal static bool GetFrameworkParent(FrameworkElement current, out FrameworkElement feParent, out FrameworkContentElement fceParent)
	{
		FrameworkObject frameworkParent = new FrameworkObject(current, null).FrameworkParent;
		feParent = frameworkParent.FE;
		fceParent = frameworkParent.FCE;
		return frameworkParent.IsValid;
	}

	internal static bool GetFrameworkParent(FrameworkContentElement current, out FrameworkElement feParent, out FrameworkContentElement fceParent)
	{
		FrameworkObject frameworkParent = new FrameworkObject(null, current).FrameworkParent;
		feParent = frameworkParent.FE;
		fceParent = frameworkParent.FCE;
		return frameworkParent.IsValid;
	}

	internal static bool GetContainingFrameworkElement(DependencyObject current, out FrameworkElement fe, out FrameworkContentElement fce)
	{
		FrameworkObject containingFrameworkElement = FrameworkObject.GetContainingFrameworkElement(current);
		if (containingFrameworkElement.IsValid)
		{
			fe = containingFrameworkElement.FE;
			fce = containingFrameworkElement.FCE;
			return true;
		}
		fe = null;
		fce = null;
		return false;
	}

	internal static void GetTemplatedParentChildRecord(DependencyObject templatedParent, int childIndex, out ChildRecord childRecord, out bool isChildRecordValid)
	{
		FrameworkTemplate frameworkTemplate = null;
		isChildRecordValid = false;
		childRecord = default(ChildRecord);
		if (templatedParent != null)
		{
			frameworkTemplate = new FrameworkObject(templatedParent, throwIfNeither: true).FE.TemplateInternal;
			if (frameworkTemplate != null && 0 <= childIndex && childIndex < frameworkTemplate.ChildRecordFromChildIndex.Count)
			{
				childRecord = frameworkTemplate.ChildRecordFromChildIndex[childIndex];
				isChildRecordValid = true;
			}
		}
	}

	internal virtual string GetPlainText()
	{
		return null;
	}

	static FrameworkElement()
	{
		_typeofThis = typeof(FrameworkElement);
		StyleProperty = DependencyProperty.Register("Style", typeof(Style), _typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnStyleChanged));
		OverridesDefaultStyleProperty = DependencyProperty.Register("OverridesDefaultStyle", typeof(bool), _typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsMeasure, OnThemeStyleKeyChanged));
		UseLayoutRoundingProperty = DependencyProperty.Register("UseLayoutRounding", typeof(bool), typeof(FrameworkElement), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits, OnUseLayoutRoundingChanged));
		DefaultStyleKeyProperty = DependencyProperty.Register("DefaultStyleKey", typeof(object), _typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnThemeStyleKeyChanged));
		DefaultNumberSubstitution = new NumberSubstitution(NumberCultureSource.User, null, NumberSubstitutionMethod.AsCulture);
		DataContextProperty = DependencyProperty.Register("DataContext", typeof(object), _typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, OnDataContextChanged));
		DataContextChangedKey = new EventPrivateKey();
		BindingGroupProperty = DependencyProperty.Register("BindingGroup", typeof(BindingGroup), _typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
		LanguageProperty = DependencyProperty.RegisterAttached("Language", typeof(XmlLanguage), _typeofThis, new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("en-US"), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));
		NameProperty = DependencyProperty.Register("Name", typeof(string), _typeofThis, new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.None, null, null, isAnimationProhibited: true), NameValidationHelper.NameValidationCallback);
		TagProperty = DependencyProperty.Register("Tag", typeof(object), _typeofThis, new FrameworkPropertyMetadata((object)null));
		InputScopeProperty = InputMethod.InputScopeProperty.AddOwner(_typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
		RequestBringIntoViewEvent = EventManager.RegisterRoutedEvent("RequestBringIntoView", RoutingStrategy.Bubble, typeof(RequestBringIntoViewEventHandler), _typeofThis);
		SizeChangedEvent = EventManager.RegisterRoutedEvent("SizeChanged", RoutingStrategy.Direct, typeof(SizeChangedEventHandler), _typeofThis);
		_actualWidthMetadata = new ReadOnlyFrameworkPropertyMetadata(0.0, GetActualWidth);
		ActualWidthPropertyKey = DependencyProperty.RegisterReadOnly("ActualWidth", typeof(double), _typeofThis, _actualWidthMetadata);
		ActualWidthProperty = ActualWidthPropertyKey.DependencyProperty;
		_actualHeightMetadata = new ReadOnlyFrameworkPropertyMetadata(0.0, GetActualHeight);
		ActualHeightPropertyKey = DependencyProperty.RegisterReadOnly("ActualHeight", typeof(double), _typeofThis, _actualHeightMetadata);
		ActualHeightProperty = ActualHeightPropertyKey.DependencyProperty;
		LayoutTransformProperty = DependencyProperty.Register("LayoutTransform", typeof(Transform), _typeofThis, new FrameworkPropertyMetadata(Transform.Identity, FrameworkPropertyMetadataOptions.AffectsMeasure, OnLayoutTransformChanged));
		WidthProperty = DependencyProperty.Register("Width", typeof(double), _typeofThis, new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTransformDirty), IsWidthHeightValid);
		MinWidthProperty = DependencyProperty.Register("MinWidth", typeof(double), _typeofThis, new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTransformDirty), IsMinWidthHeightValid);
		MaxWidthProperty = DependencyProperty.Register("MaxWidth", typeof(double), _typeofThis, new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTransformDirty), IsMaxWidthHeightValid);
		HeightProperty = DependencyProperty.Register("Height", typeof(double), _typeofThis, new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTransformDirty), IsWidthHeightValid);
		MinHeightProperty = DependencyProperty.Register("MinHeight", typeof(double), _typeofThis, new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTransformDirty), IsMinWidthHeightValid);
		MaxHeightProperty = DependencyProperty.Register("MaxHeight", typeof(double), _typeofThis, new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTransformDirty), IsMaxWidthHeightValid);
		FlowDirectionProperty = DependencyProperty.RegisterAttached("FlowDirection", typeof(FlowDirection), _typeofThis, new FrameworkPropertyMetadata(FlowDirection.LeftToRight, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.Inherits, OnFlowDirectionChanged, CoerceFlowDirectionProperty), IsValidFlowDirection);
		MarginProperty = DependencyProperty.Register("Margin", typeof(Thickness), _typeofThis, new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure), IsMarginValid);
		HorizontalAlignmentProperty = DependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), _typeofThis, new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsArrange), ValidateHorizontalAlignmentValue);
		VerticalAlignmentProperty = DependencyProperty.Register("VerticalAlignment", typeof(VerticalAlignment), _typeofThis, new FrameworkPropertyMetadata(VerticalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsArrange), ValidateVerticalAlignmentValue);
		_defaultFocusVisualStyle = null;
		FocusVisualStyleProperty = DependencyProperty.Register("FocusVisualStyle", typeof(Style), _typeofThis, new FrameworkPropertyMetadata(DefaultFocusVisualStyle));
		CursorProperty = DependencyProperty.Register("Cursor", typeof(Cursor), _typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, OnCursorChanged));
		ForceCursorProperty = DependencyProperty.Register("ForceCursor", typeof(bool), _typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.None, OnForceCursorChanged));
		InitializedKey = new EventPrivateKey();
		LoadedPendingPropertyKey = DependencyProperty.RegisterReadOnly("LoadedPending", typeof(object[]), _typeofThis, new PropertyMetadata(null));
		LoadedPendingProperty = LoadedPendingPropertyKey.DependencyProperty;
		UnloadedPendingPropertyKey = DependencyProperty.RegisterReadOnly("UnloadedPending", typeof(object[]), _typeofThis, new PropertyMetadata(null));
		UnloadedPendingProperty = UnloadedPendingPropertyKey.DependencyProperty;
		LoadedEvent = EventManager.RegisterRoutedEvent("Loaded", RoutingStrategy.Direct, typeof(RoutedEventHandler), _typeofThis);
		UnloadedEvent = EventManager.RegisterRoutedEvent("Unloaded", RoutingStrategy.Direct, typeof(RoutedEventHandler), _typeofThis);
		ToolTipProperty = ToolTipService.ToolTipProperty.AddOwner(_typeofThis);
		ContextMenuProperty = ContextMenuService.ContextMenuProperty.AddOwner(_typeofThis, new FrameworkPropertyMetadata((object)null));
		ToolTipOpeningEvent = ToolTipService.ToolTipOpeningEvent.AddOwner(_typeofThis);
		ToolTipClosingEvent = ToolTipService.ToolTipClosingEvent.AddOwner(_typeofThis);
		ContextMenuOpeningEvent = ContextMenuService.ContextMenuOpeningEvent.AddOwner(_typeofThis);
		ContextMenuClosingEvent = ContextMenuService.ContextMenuClosingEvent.AddOwner(_typeofThis);
		UnclippedDesiredSizeField = new UncommonField<SizeBox>();
		LayoutTransformDataField = new UncommonField<LayoutTransformData>();
		ResourcesField = new UncommonField<ResourceDictionary>();
		UIElementDType = DependencyObjectType.FromSystemTypeInternal(typeof(UIElement));
		_controlDType = null;
		_contentPresenterDType = null;
		_pageFunctionBaseDType = null;
		_pageDType = null;
		ResourcesChangedKey = new EventPrivateKey();
		InheritedPropertyChangedKey = new EventPrivateKey();
		InheritanceContextField = new UncommonField<DependencyObject>();
		MentorField = new UncommonField<DependencyObject>();
		UIElement.SnapsToDevicePixelsProperty.OverrideMetadata(_typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.Inherits));
		EventManager.RegisterClassHandler(_typeofThis, Mouse.QueryCursorEvent, new QueryCursorEventHandler(OnQueryCursorOverride), handledEventsToo: true);
		EventManager.RegisterClassHandler(_typeofThis, Keyboard.PreviewGotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnPreviewGotKeyboardFocus));
		EventManager.RegisterClassHandler(_typeofThis, Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus));
		EventManager.RegisterClassHandler(_typeofThis, Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostKeyboardFocus));
		UIElement.AllowDropProperty.OverrideMetadata(_typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));
		Stylus.IsPressAndHoldEnabledProperty.AddOwner(_typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
		Stylus.IsFlicksEnabledProperty.AddOwner(_typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
		Stylus.IsTapFeedbackEnabledProperty.AddOwner(_typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
		Stylus.IsTouchFeedbackEnabledProperty.AddOwner(_typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
		PropertyChangedCallback propertyChangedCallback = NumberSubstitutionChanged;
		NumberSubstitution.CultureSourceProperty.OverrideMetadata(_typeofThis, new FrameworkPropertyMetadata(NumberCultureSource.User, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback));
		NumberSubstitution.CultureOverrideProperty.OverrideMetadata(_typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback));
		NumberSubstitution.SubstitutionProperty.OverrideMetadata(_typeofThis, new FrameworkPropertyMetadata(NumberSubstitutionMethod.AsCulture, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback));
		EventManager.RegisterClassHandler(_typeofThis, ToolTipOpeningEvent, new ToolTipEventHandler(OnToolTipOpeningThunk));
		EventManager.RegisterClassHandler(_typeofThis, ToolTipClosingEvent, new ToolTipEventHandler(OnToolTipClosingThunk));
		EventManager.RegisterClassHandler(_typeofThis, ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpeningThunk));
		EventManager.RegisterClassHandler(_typeofThis, ContextMenuClosingEvent, new ContextMenuEventHandler(OnContextMenuClosingThunk));
		TextElement.FontFamilyProperty.OverrideMetadata(_typeofThis, new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.Inherits, null, CoerceFontFamily));
		TextElement.FontSizeProperty.OverrideMetadata(_typeofThis, new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.Inherits, null, CoerceFontSize));
		TextElement.FontStyleProperty.OverrideMetadata(_typeofThis, new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle, FrameworkPropertyMetadataOptions.Inherits, null, CoerceFontStyle));
		TextElement.FontWeightProperty.OverrideMetadata(_typeofThis, new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, FrameworkPropertyMetadataOptions.Inherits, null, CoerceFontWeight));
		TextOptions.TextRenderingModeProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(TextRenderingMode_Changed));
	}

	private static void TextRenderingMode_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((FrameworkElement)d).pushTextRenderingMode();
	}

	internal virtual void pushTextRenderingMode()
	{
		if (DependencyPropertyHelper.GetValueSource(this, TextOptions.TextRenderingModeProperty).BaseValueSource > BaseValueSource.Inherited)
		{
			base.VisualTextRenderingMode = TextOptions.GetTextRenderingMode(this);
		}
	}

	internal virtual void OnAncestorChanged()
	{
	}

	/// <summary>Invoked when the parent of this element in the visual tree is changed. Overrides <see cref="M:System.Windows.UIElement.OnVisualParentChanged(System.Windows.DependencyObject)" />.</summary>
	/// <param name="oldParent">The old parent element. May be null to indicate that the element did not have a visual parent previously.</param>
	protected internal override void OnVisualParentChanged(DependencyObject oldParent)
	{
		DependencyObject parentInternal = VisualTreeHelper.GetParentInternal(this);
		if (parentInternal != null)
		{
			ClearInheritanceContext();
		}
		BroadcastEventHelper.AddOrRemoveHasLoadedChangeHandlerFlag(this, oldParent, parentInternal);
		BroadcastEventHelper.BroadcastLoadedOrUnloadedEvent(this, oldParent, parentInternal);
		if (parentInternal != null && !(parentInternal is FrameworkElement))
		{
			if (parentInternal is Visual visual)
			{
				visual.VisualAncestorChanged += OnVisualAncestorChanged;
			}
			else if (parentInternal is Visual3D)
			{
				((Visual3D)parentInternal).VisualAncestorChanged += OnVisualAncestorChanged;
			}
		}
		else if (oldParent != null && !(oldParent is FrameworkElement))
		{
			if (oldParent is Visual visual2)
			{
				visual2.VisualAncestorChanged -= OnVisualAncestorChanged;
			}
			else if (oldParent is Visual3D)
			{
				((Visual3D)oldParent).VisualAncestorChanged -= OnVisualAncestorChanged;
			}
		}
		if (Parent == null)
		{
			DependencyObject parent = ((parentInternal != null) ? parentInternal : oldParent);
			TreeWalkHelper.InvalidateOnTreeChange(this, null, parent, parentInternal != null);
		}
		TryFireInitialized();
		base.OnVisualParentChanged(oldParent);
	}

	internal new void OnVisualAncestorChanged(object sender, AncestorChangedEventArgs e)
	{
		FrameworkElement fe = null;
		FrameworkContentElement fce = null;
		GetContainingFrameworkElement(VisualTreeHelper.GetParent(this), out fe, out fce);
		if (e.OldParent == null)
		{
			if (fe == null || !VisualTreeHelper.IsAncestorOf(e.Ancestor, fe))
			{
				BroadcastEventHelper.AddOrRemoveHasLoadedChangeHandlerFlag(this, null, VisualTreeHelper.GetParent(e.Ancestor));
				BroadcastEventHelper.BroadcastLoadedOrUnloadedEvent(this, null, VisualTreeHelper.GetParent(e.Ancestor));
			}
		}
		else if (fe == null)
		{
			GetContainingFrameworkElement(e.OldParent, out fe, out fce);
			if (fe != null)
			{
				BroadcastEventHelper.AddOrRemoveHasLoadedChangeHandlerFlag(this, fe, null);
				BroadcastEventHelper.BroadcastLoadedOrUnloadedEvent(this, fe, null);
			}
		}
	}

	private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue != BindingExpressionBase.DisconnectedItem)
		{
			((FrameworkElement)d).RaiseDependencyPropertyChanged(DataContextChangedKey, e);
		}
	}

	/// <summary>Returns the <see cref="T:System.Windows.Data.BindingExpression" /> that represents the binding on the specified property. </summary>
	/// <returns>A <see cref="T:System.Windows.Data.BindingExpression" /> if the target property has an active binding; otherwise, returns null.</returns>
	/// <param name="dp">The target <see cref="T:System.Windows.DependencyProperty" /> to get the binding from.</param>
	public BindingExpression GetBindingExpression(DependencyProperty dp)
	{
		return BindingOperations.GetBindingExpression(this, dp);
	}

	/// <summary>Attaches a binding to this element, based on the provided binding object. </summary>
	/// <returns>Records the conditions of the binding. This return value can be useful for error checking.</returns>
	/// <param name="dp">Identifies the property where the binding should be established.</param>
	/// <param name="binding">Represents the specifics of the data binding.</param>
	public BindingExpressionBase SetBinding(DependencyProperty dp, BindingBase binding)
	{
		return BindingOperations.SetBinding(this, dp, binding);
	}

	/// <summary>Attaches a binding to this element, based on the provided source property name as a path qualification to the data source. </summary>
	/// <returns>Records the conditions of the binding. This return value can be useful for error checking.</returns>
	/// <param name="dp">Identifies the destination property where the binding should be established.</param>
	/// <param name="path">The source property name or the path to the property used for the binding.</param>
	public BindingExpression SetBinding(DependencyProperty dp, string path)
	{
		return (BindingExpression)SetBinding(dp, new Binding(path));
	}

	/// <summary>Returns an alternative logical parent for this element if there is no visual parent.</summary>
	/// <returns>Returns something other than null whenever a WPF framework-level implementation of this method has a non-visual parent connection.</returns>
	protected internal override DependencyObject GetUIParentCore()
	{
		return _parent;
	}

	internal override object AdjustEventSource(RoutedEventArgs args)
	{
		object result = null;
		if ((_parent != null || HasLogicalChildren) && (!(args.Source is DependencyObject child) || !IsLogicalDescendent(child)))
		{
			args.Source = this;
			result = this;
		}
		return result;
	}

	internal virtual void AdjustBranchSource(RoutedEventArgs args)
	{
	}

	internal override bool BuildRouteCore(EventRoute route, RoutedEventArgs args)
	{
		return BuildRouteCoreHelper(route, args, shouldAddIntermediateElementsToRoute: true);
	}

	internal bool BuildRouteCoreHelper(EventRoute route, RoutedEventArgs args, bool shouldAddIntermediateElementsToRoute)
	{
		bool result = false;
		DependencyObject parent = VisualTreeHelper.GetParent(this);
		DependencyObject uIParentCore = GetUIParentCore();
		if (route.PeekBranchNode() is DependencyObject dependencyObject && IsLogicalDescendent(dependencyObject))
		{
			args.Source = route.PeekBranchSource();
			AdjustBranchSource(args);
			route.AddSource(args.Source);
			route.PopBranchNode();
			if (shouldAddIntermediateElementsToRoute)
			{
				AddIntermediateElementsToRoute(this, route, args, LogicalTreeHelper.GetParent(dependencyObject));
			}
		}
		if (!IgnoreModelParentBuildRoute(args))
		{
			if (parent == null)
			{
				result = uIParentCore != null;
			}
			else if (uIParentCore != null)
			{
				if (parent is Visual visual)
				{
					if (visual.CheckFlagsAnd(VisualFlags.IsLayoutIslandRoot))
					{
						result = true;
					}
				}
				else if (((Visual3D)parent).CheckFlagsAnd(VisualFlags.IsLayoutIslandRoot))
				{
					result = true;
				}
				route.PushBranchNode(this, args.Source);
				args.Source = parent;
			}
		}
		return result;
	}

	internal override void AddToEventRouteCore(EventRoute route, RoutedEventArgs args)
	{
		AddStyleHandlersToEventRoute(this, null, route, args);
	}

	internal static void AddStyleHandlersToEventRoute(FrameworkElement fe, FrameworkContentElement fce, EventRoute route, RoutedEventArgs args)
	{
		DependencyObject source = ((fe != null) ? ((DependencyObject)fe) : ((DependencyObject)fce));
		Style style = null;
		FrameworkTemplate frameworkTemplate = null;
		DependencyObject dependencyObject = null;
		int num = -1;
		if (fe != null)
		{
			style = fe.Style;
			frameworkTemplate = fe.TemplateInternal;
			dependencyObject = fe.TemplatedParent;
			num = fe.TemplateChildIndex;
		}
		else
		{
			style = fce.Style;
			dependencyObject = fce.TemplatedParent;
			num = fce.TemplateChildIndex;
		}
		RoutedEventHandlerInfo[] array = null;
		if (style != null && style.EventHandlersStore != null)
		{
			array = style.EventHandlersStore.GetRoutedEventHandlers(args.RoutedEvent);
			AddStyleHandlersToEventRoute(route, source, array);
		}
		if (frameworkTemplate != null && frameworkTemplate.EventHandlersStore != null)
		{
			array = frameworkTemplate.EventHandlersStore.GetRoutedEventHandlers(args.RoutedEvent);
			AddStyleHandlersToEventRoute(route, source, array);
		}
		if (dependencyObject != null)
		{
			FrameworkTemplate frameworkTemplate2 = null;
			frameworkTemplate2 = (dependencyObject as FrameworkElement).TemplateInternal;
			array = null;
			if (frameworkTemplate2 != null && frameworkTemplate2.HasEventDependents)
			{
				array = StyleHelper.GetChildRoutedEventHandlers(num, args.RoutedEvent, ref frameworkTemplate2.EventDependents);
			}
			AddStyleHandlersToEventRoute(route, source, array);
		}
	}

	private static void AddStyleHandlersToEventRoute(EventRoute route, DependencyObject source, RoutedEventHandlerInfo[] handlers)
	{
		if (handlers != null)
		{
			for (int i = 0; i < handlers.Length; i++)
			{
				route.Add(source, handlers[i].Handler, handlers[i].InvokeHandledEventsToo);
			}
		}
	}

	internal virtual bool IgnoreModelParentBuildRoute(RoutedEventArgs args)
	{
		return false;
	}

	internal override bool InvalidateAutomationAncestorsCore(Stack<DependencyObject> branchNodeStack, out bool continuePastCoreTree)
	{
		bool shouldInvalidateIntermediateElements = true;
		return InvalidateAutomationAncestorsCoreHelper(branchNodeStack, out continuePastCoreTree, shouldInvalidateIntermediateElements);
	}

	internal override void InvalidateForceInheritPropertyOnChildren(DependencyProperty property)
	{
		if (property == UIElement.IsEnabledProperty)
		{
			IEnumerator logicalChildren = LogicalChildren;
			if (logicalChildren != null)
			{
				while (logicalChildren.MoveNext())
				{
					if (logicalChildren.Current is DependencyObject dependencyObject)
					{
						dependencyObject.CoerceValue(property);
					}
				}
			}
		}
		base.InvalidateForceInheritPropertyOnChildren(property);
	}

	internal bool InvalidateAutomationAncestorsCoreHelper(Stack<DependencyObject> branchNodeStack, out bool continuePastCoreTree, bool shouldInvalidateIntermediateElements)
	{
		bool result = true;
		continuePastCoreTree = false;
		DependencyObject parent = VisualTreeHelper.GetParent(this);
		DependencyObject uIParentCore = GetUIParentCore();
		DependencyObject dependencyObject = ((branchNodeStack.Count > 0) ? branchNodeStack.Peek() : null);
		if (dependencyObject != null && IsLogicalDescendent(dependencyObject))
		{
			branchNodeStack.Pop();
			if (shouldInvalidateIntermediateElements)
			{
				result = InvalidateAutomationIntermediateElements(this, LogicalTreeHelper.GetParent(dependencyObject));
			}
		}
		if (parent == null)
		{
			continuePastCoreTree = uIParentCore != null;
		}
		else if (uIParentCore != null)
		{
			if (parent is Visual visual)
			{
				if (visual.CheckFlagsAnd(VisualFlags.IsLayoutIslandRoot))
				{
					continuePastCoreTree = true;
				}
			}
			else if (((Visual3D)parent).CheckFlagsAnd(VisualFlags.IsLayoutIslandRoot))
			{
				continuePastCoreTree = true;
			}
			branchNodeStack.Push(this);
		}
		return result;
	}

	internal static bool InvalidateAutomationIntermediateElements(DependencyObject mergePoint, DependencyObject modelTreeNode)
	{
		UIElement e = null;
		ContentElement ce = null;
		UIElement3D e3d = null;
		while (modelTreeNode != null && modelTreeNode != mergePoint)
		{
			if (!UIElementHelper.InvalidateAutomationPeer(modelTreeNode, out e, out ce, out e3d))
			{
				return false;
			}
			modelTreeNode = LogicalTreeHelper.GetParent(modelTreeNode);
		}
		return true;
	}

	/// <summary>Attempts to bring this element into view, within any scrollable regions it is contained within. </summary>
	public void BringIntoView()
	{
		BringIntoView(Rect.Empty);
	}

	/// <summary>Attempts to bring the provided region size of this element into view, within any scrollable regions it is contained within. </summary>
	/// <param name="targetRectangle">Specified size of the element that should also be brought into view. </param>
	public void BringIntoView(Rect targetRectangle)
	{
		RequestBringIntoViewEventArgs requestBringIntoViewEventArgs = new RequestBringIntoViewEventArgs(this, targetRectangle);
		requestBringIntoViewEventArgs.RoutedEvent = RequestBringIntoViewEvent;
		RaiseEvent(requestBringIntoViewEventArgs);
	}

	private static object GetActualWidth(DependencyObject d, out BaseValueSourceInternal source)
	{
		FrameworkElement frameworkElement = (FrameworkElement)d;
		if (frameworkElement.HasWidthEverChanged)
		{
			source = BaseValueSourceInternal.Local;
			return frameworkElement.RenderSize.Width;
		}
		source = BaseValueSourceInternal.Default;
		return 0.0;
	}

	private static object GetActualHeight(DependencyObject d, out BaseValueSourceInternal source)
	{
		FrameworkElement frameworkElement = (FrameworkElement)d;
		if (frameworkElement.HasHeightEverChanged)
		{
			source = BaseValueSourceInternal.Local;
			return frameworkElement.RenderSize.Height;
		}
		source = BaseValueSourceInternal.Default;
		return 0.0;
	}

	private static void OnLayoutTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((FrameworkElement)d).AreTransformsClean = false;
	}

	private static bool IsWidthHeightValid(object value)
	{
		double num = (double)value;
		if (!double.IsNaN(num))
		{
			if (num >= 0.0)
			{
				return !double.IsPositiveInfinity(num);
			}
			return false;
		}
		return true;
	}

	private static bool IsMinWidthHeightValid(object value)
	{
		double num = (double)value;
		if (!double.IsNaN(num) && num >= 0.0)
		{
			return !double.IsPositiveInfinity(num);
		}
		return false;
	}

	private static bool IsMaxWidthHeightValid(object value)
	{
		double num = (double)value;
		if (!double.IsNaN(num))
		{
			return num >= 0.0;
		}
		return false;
	}

	private static void OnTransformDirty(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((FrameworkElement)d).AreTransformsClean = false;
	}

	private static object CoerceFlowDirectionProperty(DependencyObject d, object value)
	{
		if (d is FrameworkElement frameworkElement)
		{
			frameworkElement.InvalidateArrange();
			frameworkElement.InvalidateVisual();
			frameworkElement.AreTransformsClean = false;
		}
		return value;
	}

	private static void OnFlowDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FrameworkElement frameworkElement)
		{
			frameworkElement.IsRightToLeft = (FlowDirection)e.NewValue == FlowDirection.RightToLeft;
			frameworkElement.AreTransformsClean = false;
		}
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.FrameworkElement.FlowDirection" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />. </summary>
	/// <returns>The requested flow direction, as a value of the enumeration.</returns>
	/// <param name="element">The element to return a <see cref="P:System.Windows.FrameworkElement.FlowDirection" /> for.</param>
	public static FlowDirection GetFlowDirection(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (FlowDirection)element.GetValue(FlowDirectionProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.FrameworkElement.FlowDirection" /> attached property for the provided element. </summary>
	/// <param name="element">The element that specifies a flow direction.</param>
	/// <param name="value">A value of the enumeration, specifying the direction.</param>
	public static void SetFlowDirection(DependencyObject element, FlowDirection value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(FlowDirectionProperty, value);
	}

	private static bool IsValidFlowDirection(object o)
	{
		FlowDirection flowDirection = (FlowDirection)o;
		if (flowDirection != 0)
		{
			return flowDirection == FlowDirection.RightToLeft;
		}
		return true;
	}

	private static bool IsMarginValid(object value)
	{
		return ((Thickness)value).IsValid(allowNegative: true, allowNaN: false, allowPositiveInfinity: true, allowNegativeInfinity: false);
	}

	internal static bool ValidateHorizontalAlignmentValue(object value)
	{
		HorizontalAlignment horizontalAlignment = (HorizontalAlignment)value;
		if (horizontalAlignment != 0 && horizontalAlignment != HorizontalAlignment.Center && horizontalAlignment != HorizontalAlignment.Right)
		{
			return horizontalAlignment == HorizontalAlignment.Stretch;
		}
		return true;
	}

	internal static bool ValidateVerticalAlignmentValue(object value)
	{
		VerticalAlignment verticalAlignment = (VerticalAlignment)value;
		if (verticalAlignment != 0 && verticalAlignment != VerticalAlignment.Center && verticalAlignment != VerticalAlignment.Bottom)
		{
			return verticalAlignment == VerticalAlignment.Stretch;
		}
		return true;
	}

	private static void OnCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (((FrameworkElement)d).IsMouseOver)
		{
			Mouse.UpdateCursor();
		}
	}

	private static void OnForceCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (((FrameworkElement)d).IsMouseOver)
		{
			Mouse.UpdateCursor();
		}
	}

	private static void OnQueryCursorOverride(object sender, QueryCursorEventArgs e)
	{
		FrameworkElement frameworkElement = (FrameworkElement)sender;
		Cursor cursor = frameworkElement.Cursor;
		if (cursor != null && (!e.Handled || frameworkElement.ForceCursor))
		{
			e.Cursor = cursor;
			e.Handled = true;
		}
	}

	private Transform GetFlowDirectionTransform()
	{
		if (!BypassLayoutPolicies && ShouldApplyMirrorTransform(this))
		{
			return new MatrixTransform(-1.0, 0.0, 0.0, 1.0, base.RenderSize.Width, 0.0);
		}
		return null;
	}

	internal static bool ShouldApplyMirrorTransform(FrameworkElement fe)
	{
		FlowDirection flowDirection = fe.FlowDirection;
		FlowDirection parentFD = FlowDirection.LeftToRight;
		DependencyObject parent = VisualTreeHelper.GetParent(fe);
		FrameworkElement feParent;
		FrameworkContentElement fceParent;
		if (parent != null)
		{
			parentFD = GetFlowDirectionFromVisual(parent);
		}
		else if (GetFrameworkParent(fe, out feParent, out fceParent))
		{
			if (feParent != null && feParent is IContentHost)
			{
				parentFD = feParent.FlowDirection;
			}
			else if (fceParent != null)
			{
				parentFD = (FlowDirection)fceParent.GetValue(FlowDirectionProperty);
			}
		}
		return ApplyMirrorTransform(parentFD, flowDirection);
	}

	private static FlowDirection GetFlowDirectionFromVisual(DependencyObject visual)
	{
		FlowDirection result = FlowDirection.LeftToRight;
		for (DependencyObject dependencyObject = visual; dependencyObject != null; dependencyObject = VisualTreeHelper.GetParent(dependencyObject))
		{
			if (dependencyObject is FrameworkElement frameworkElement)
			{
				result = frameworkElement.FlowDirection;
				break;
			}
			object obj = dependencyObject.ReadLocalValue(FlowDirectionProperty);
			if (obj != DependencyProperty.UnsetValue)
			{
				result = (FlowDirection)obj;
				break;
			}
		}
		return result;
	}

	internal static bool ApplyMirrorTransform(FlowDirection parentFD, FlowDirection thisFD)
	{
		if (parentFD != 0 || thisFD != FlowDirection.RightToLeft)
		{
			if (parentFD == FlowDirection.RightToLeft)
			{
				return thisFD == FlowDirection.LeftToRight;
			}
			return false;
		}
		return true;
	}

	private Size FindMaximalAreaLocalSpaceRect(Transform layoutTransform, Size transformSpaceBounds)
	{
		double num = transformSpaceBounds.Width;
		double num2 = transformSpaceBounds.Height;
		if (DoubleUtil.IsZero(num) || DoubleUtil.IsZero(num2))
		{
			return new Size(0.0, 0.0);
		}
		bool flag = double.IsInfinity(num);
		bool flag2 = double.IsInfinity(num2);
		if (flag && flag2)
		{
			return new Size(double.PositiveInfinity, double.PositiveInfinity);
		}
		if (flag)
		{
			num = num2;
		}
		else if (flag2)
		{
			num2 = num;
		}
		Matrix value = layoutTransform.Value;
		if (!value.HasInverse)
		{
			return new Size(0.0, 0.0);
		}
		double m = value.M11;
		double m2 = value.M12;
		double m3 = value.M21;
		double m4 = value.M22;
		double num3 = 0.0;
		double num4 = 0.0;
		if (DoubleUtil.IsZero(m2) || DoubleUtil.IsZero(m3))
		{
			double num5 = (flag2 ? double.PositiveInfinity : Math.Abs(num2 / m4));
			double num6 = (flag ? double.PositiveInfinity : Math.Abs(num / m));
			if (DoubleUtil.IsZero(m2))
			{
				if (DoubleUtil.IsZero(m3))
				{
					num4 = num5;
					num3 = num6;
				}
				else
				{
					num4 = Math.Min(0.5 * Math.Abs(num / m3), num5);
					num3 = num6 - m3 * num4 / m;
				}
			}
			else
			{
				num3 = Math.Min(0.5 * Math.Abs(num2 / m2), num6);
				num4 = num5 - m2 * num3 / m4;
			}
		}
		else if (DoubleUtil.IsZero(m) || DoubleUtil.IsZero(m4))
		{
			double num7 = Math.Abs(num2 / m2);
			double num8 = Math.Abs(num / m3);
			if (DoubleUtil.IsZero(m))
			{
				if (DoubleUtil.IsZero(m4))
				{
					num4 = num8;
					num3 = num7;
				}
				else
				{
					num4 = Math.Min(0.5 * Math.Abs(num2 / m4), num8);
					num3 = num7 - m4 * num4 / m2;
				}
			}
			else
			{
				num3 = Math.Min(0.5 * Math.Abs(num / m), num7);
				num4 = num8 - m * num3 / m3;
			}
		}
		else
		{
			double num9 = Math.Abs(num / m);
			double num10 = Math.Abs(num / m3);
			double num11 = Math.Abs(num2 / m2);
			double num12 = Math.Abs(num2 / m4);
			num3 = Math.Min(num11, num9) * 0.5;
			num4 = Math.Min(num10, num12) * 0.5;
			if ((DoubleUtil.GreaterThanOrClose(num9, num11) && DoubleUtil.LessThanOrClose(num10, num12)) || (DoubleUtil.LessThanOrClose(num9, num11) && DoubleUtil.GreaterThanOrClose(num10, num12)))
			{
				Rect rect = Rect.Transform(new Rect(0.0, 0.0, num3, num4), layoutTransform.Value);
				double num13 = Math.Min(num / rect.Width, num2 / rect.Height);
				if (!double.IsNaN(num13) && !double.IsInfinity(num13))
				{
					num3 *= num13;
					num4 *= num13;
				}
			}
		}
		return new Size(num3, num4);
	}

	/// <summary>Implements basic measure-pass layout system behavior for <see cref="T:System.Windows.FrameworkElement" />. </summary>
	/// <returns>The desired size of this element in layout.</returns>
	/// <param name="availableSize">The available size that the parent element can give to the child elements.</param>
	protected sealed override Size MeasureCore(Size availableSize)
	{
		bool useLayoutRounding = UseLayoutRounding;
		DpiScale dpi = GetDpi();
		if (useLayoutRounding && !CheckFlagsAnd(VisualFlags.UseLayoutRounding))
		{
			SetFlags(value: true, VisualFlags.UseLayoutRounding);
		}
		ApplyTemplate();
		if (BypassLayoutPolicies)
		{
			return MeasureOverride(availableSize);
		}
		Thickness margin = Margin;
		double num = margin.Left + margin.Right;
		double num2 = margin.Top + margin.Bottom;
		if (useLayoutRounding && (this is ScrollContentPresenter || !FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness))
		{
			num = UIElement.RoundLayoutValue(num, dpi.DpiScaleX);
			num2 = UIElement.RoundLayoutValue(num2, dpi.DpiScaleY);
		}
		Size size = new Size(Math.Max(availableSize.Width - num, 0.0), Math.Max(availableSize.Height - num2, 0.0));
		MinMax minMax = new MinMax(this);
		if (useLayoutRounding && !FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness)
		{
			minMax.maxHeight = UIElement.RoundLayoutValue(minMax.maxHeight, dpi.DpiScaleY);
			minMax.maxWidth = UIElement.RoundLayoutValue(minMax.maxWidth, dpi.DpiScaleX);
			minMax.minHeight = UIElement.RoundLayoutValue(minMax.minHeight, dpi.DpiScaleY);
			minMax.minWidth = UIElement.RoundLayoutValue(minMax.minWidth, dpi.DpiScaleX);
		}
		LayoutTransformData layoutTransformData = LayoutTransformDataField.GetValue(this);
		Transform layoutTransform = LayoutTransform;
		if (layoutTransform != null && !layoutTransform.IsIdentity)
		{
			if (layoutTransformData == null)
			{
				layoutTransformData = new LayoutTransformData();
				LayoutTransformDataField.SetValue(this, layoutTransformData);
			}
			layoutTransformData.CreateTransformSnapshot(layoutTransform);
			layoutTransformData.UntransformedDS = default(Size);
			if (useLayoutRounding)
			{
				layoutTransformData.TransformedUnroundedDS = default(Size);
			}
		}
		else if (layoutTransformData != null)
		{
			layoutTransformData = null;
			LayoutTransformDataField.ClearValue(this);
		}
		if (layoutTransformData != null)
		{
			size = FindMaximalAreaLocalSpaceRect(layoutTransformData.Transform, size);
		}
		size.Width = Math.Max(minMax.minWidth, Math.Min(size.Width, minMax.maxWidth));
		size.Height = Math.Max(minMax.minHeight, Math.Min(size.Height, minMax.maxHeight));
		if (useLayoutRounding)
		{
			size = UIElement.RoundLayoutSize(size, dpi.DpiScaleX, dpi.DpiScaleY);
		}
		Size size2 = MeasureOverride(size);
		size2 = new Size(Math.Max(size2.Width, minMax.minWidth), Math.Max(size2.Height, minMax.minHeight));
		Size size3 = size2;
		if (layoutTransformData != null)
		{
			layoutTransformData.UntransformedDS = size3;
			Rect rect = Rect.Transform(new Rect(0.0, 0.0, size3.Width, size3.Height), layoutTransformData.Transform.Value);
			size3.Width = rect.Width;
			size3.Height = rect.Height;
		}
		bool flag = false;
		if (size2.Width > minMax.maxWidth)
		{
			size2.Width = minMax.maxWidth;
			flag = true;
		}
		if (size2.Height > minMax.maxHeight)
		{
			size2.Height = minMax.maxHeight;
			flag = true;
		}
		if (layoutTransformData != null)
		{
			Rect rect2 = Rect.Transform(new Rect(0.0, 0.0, size2.Width, size2.Height), layoutTransformData.Transform.Value);
			size2.Width = rect2.Width;
			size2.Height = rect2.Height;
		}
		double num3 = size2.Width + num;
		double num4 = size2.Height + num2;
		if (num3 > availableSize.Width)
		{
			num3 = availableSize.Width;
			flag = true;
		}
		if (num4 > availableSize.Height)
		{
			num4 = availableSize.Height;
			flag = true;
		}
		if (layoutTransformData != null)
		{
			layoutTransformData.TransformedUnroundedDS = new Size(Math.Max(0.0, num3), Math.Max(0.0, num4));
		}
		if (useLayoutRounding)
		{
			num3 = UIElement.RoundLayoutValue(num3, dpi.DpiScaleX);
			num4 = UIElement.RoundLayoutValue(num4, dpi.DpiScaleY);
		}
		SizeBox value = UnclippedDesiredSizeField.GetValue(this);
		if (flag || num3 < 0.0 || num4 < 0.0)
		{
			if (value == null)
			{
				value = new SizeBox(size3);
				UnclippedDesiredSizeField.SetValue(this, value);
			}
			else
			{
				value.Width = size3.Width;
				value.Height = size3.Height;
			}
		}
		else if (value != null)
		{
			UnclippedDesiredSizeField.ClearValue(this);
		}
		return new Size(Math.Max(0.0, num3), Math.Max(0.0, num4));
	}

	/// <summary>Implements <see cref="M:System.Windows.UIElement.ArrangeCore(System.Windows.Rect)" /> (defined as virtual in <see cref="T:System.Windows.UIElement" />) and seals the implementation.</summary>
	/// <param name="finalRect">The final area within the parent that this element should use to arrange itself and its children.</param>
	protected sealed override void ArrangeCore(Rect finalRect)
	{
		bool useLayoutRounding = UseLayoutRounding;
		DpiScale dpi = GetDpi();
		LayoutTransformData value = LayoutTransformDataField.GetValue(this);
		Size size = Size.Empty;
		if (useLayoutRounding && !CheckFlagsAnd(VisualFlags.UseLayoutRounding))
		{
			SetFlags(value: true, VisualFlags.UseLayoutRounding);
		}
		if (BypassLayoutPolicies)
		{
			Size renderSize = base.RenderSize;
			Size renderSize2 = ArrangeOverride(finalRect.Size);
			base.RenderSize = renderSize2;
			SetLayoutOffset(new Vector(finalRect.X, finalRect.Y), renderSize);
			return;
		}
		NeedsClipBounds = false;
		Size size2 = finalRect.Size;
		Thickness margin = Margin;
		double num = margin.Left + margin.Right;
		double num2 = margin.Top + margin.Bottom;
		if (useLayoutRounding && !FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness)
		{
			num = UIElement.RoundLayoutValue(num, dpi.DpiScaleX);
			num2 = UIElement.RoundLayoutValue(num2, dpi.DpiScaleY);
		}
		size2.Width = Math.Max(0.0, size2.Width - num);
		size2.Height = Math.Max(0.0, size2.Height - num2);
		if (useLayoutRounding && value != null)
		{
			size = value.TransformedUnroundedDS;
			size.Width = Math.Max(0.0, size.Width - num);
			size.Height = Math.Max(0.0, size.Height - num2);
		}
		SizeBox value2 = UnclippedDesiredSizeField.GetValue(this);
		Size size3;
		if (value2 == null)
		{
			size3 = new Size(Math.Max(0.0, base.DesiredSize.Width - num), Math.Max(0.0, base.DesiredSize.Height - num2));
			if (size != Size.Empty)
			{
				size3.Width = Math.Max(size.Width, size3.Width);
				size3.Height = Math.Max(size.Height, size3.Height);
			}
		}
		else
		{
			size3 = new Size(value2.Width, value2.Height);
		}
		if (DoubleUtil.LessThan(size2.Width, size3.Width))
		{
			NeedsClipBounds = true;
			size2.Width = size3.Width;
		}
		if (DoubleUtil.LessThan(size2.Height, size3.Height))
		{
			NeedsClipBounds = true;
			size2.Height = size3.Height;
		}
		if (HorizontalAlignment != HorizontalAlignment.Stretch)
		{
			size2.Width = size3.Width;
		}
		if (VerticalAlignment != VerticalAlignment.Stretch)
		{
			size2.Height = size3.Height;
		}
		if (value != null)
		{
			Size size4 = FindMaximalAreaLocalSpaceRect(value.Transform, size2);
			size2 = size4;
			size3 = value.UntransformedDS;
			if (!DoubleUtil.IsZero(size4.Width) && !DoubleUtil.IsZero(size4.Height) && (LayoutDoubleUtil.LessThan(size4.Width, size3.Width) || LayoutDoubleUtil.LessThan(size4.Height, size3.Height)))
			{
				size2 = size3;
			}
			if (DoubleUtil.LessThan(size2.Width, size3.Width))
			{
				NeedsClipBounds = true;
				size2.Width = size3.Width;
			}
			if (DoubleUtil.LessThan(size2.Height, size3.Height))
			{
				NeedsClipBounds = true;
				size2.Height = size3.Height;
			}
		}
		MinMax minMax = new MinMax(this);
		if (useLayoutRounding && !FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness)
		{
			minMax.maxHeight = UIElement.RoundLayoutValue(minMax.maxHeight, dpi.DpiScaleY);
			minMax.maxWidth = UIElement.RoundLayoutValue(minMax.maxWidth, dpi.DpiScaleX);
			minMax.minHeight = UIElement.RoundLayoutValue(minMax.minHeight, dpi.DpiScaleY);
			minMax.minWidth = UIElement.RoundLayoutValue(minMax.minWidth, dpi.DpiScaleX);
		}
		double num3 = Math.Max(size3.Width, minMax.maxWidth);
		if (DoubleUtil.LessThan(num3, size2.Width))
		{
			NeedsClipBounds = true;
			size2.Width = num3;
		}
		double num4 = Math.Max(size3.Height, minMax.maxHeight);
		if (DoubleUtil.LessThan(num4, size2.Height))
		{
			NeedsClipBounds = true;
			size2.Height = num4;
		}
		if (useLayoutRounding)
		{
			size2 = UIElement.RoundLayoutSize(size2, dpi.DpiScaleX, dpi.DpiScaleY);
		}
		Size renderSize3 = base.RenderSize;
		Size size6 = (base.RenderSize = ArrangeOverride(size2));
		if (useLayoutRounding)
		{
			base.RenderSize = UIElement.RoundLayoutSize(base.RenderSize, dpi.DpiScaleX, dpi.DpiScaleY);
		}
		Size size7 = new Size(Math.Min(size6.Width, minMax.maxWidth), Math.Min(size6.Height, minMax.maxHeight));
		if (useLayoutRounding)
		{
			size7 = UIElement.RoundLayoutSize(size7, dpi.DpiScaleX, dpi.DpiScaleY);
		}
		NeedsClipBounds |= DoubleUtil.LessThan(size7.Width, size6.Width) || DoubleUtil.LessThan(size7.Height, size6.Height);
		if (value != null)
		{
			Rect rect = Rect.Transform(new Rect(0.0, 0.0, size7.Width, size7.Height), value.Transform.Value);
			size7.Width = rect.Width;
			size7.Height = rect.Height;
			if (useLayoutRounding)
			{
				size7 = UIElement.RoundLayoutSize(size7, dpi.DpiScaleX, dpi.DpiScaleY);
			}
		}
		Size size8 = new Size(Math.Max(0.0, finalRect.Width - num), Math.Max(0.0, finalRect.Height - num2));
		if (useLayoutRounding)
		{
			size8 = UIElement.RoundLayoutSize(size8, dpi.DpiScaleX, dpi.DpiScaleY);
		}
		NeedsClipBounds |= DoubleUtil.LessThan(size8.Width, size7.Width) || DoubleUtil.LessThan(size8.Height, size7.Height);
		Vector offset = ComputeAlignmentOffset(size8, size7);
		offset.X += finalRect.X + margin.Left;
		offset.Y += finalRect.Y + margin.Top;
		if (useLayoutRounding)
		{
			offset.X = UIElement.RoundLayoutValue(offset.X, dpi.DpiScaleX);
			offset.Y = UIElement.RoundLayoutValue(offset.Y, dpi.DpiScaleY);
		}
		SetLayoutOffset(offset, renderSize3);
	}

	/// <summary>Raises the <see cref="E:System.Windows.FrameworkElement.SizeChanged" /> event, using the specified information as part of the eventual event data. </summary>
	/// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
	protected internal override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		SizeChangedEventArgs sizeChangedEventArgs = new SizeChangedEventArgs(this, sizeInfo);
		sizeChangedEventArgs.RoutedEvent = SizeChangedEvent;
		if (sizeInfo.WidthChanged)
		{
			HasWidthEverChanged = true;
			NotifyPropertyChange(new DependencyPropertyChangedEventArgs(ActualWidthProperty, _actualWidthMetadata, sizeInfo.PreviousSize.Width, sizeInfo.NewSize.Width));
		}
		if (sizeInfo.HeightChanged)
		{
			HasHeightEverChanged = true;
			NotifyPropertyChange(new DependencyPropertyChangedEventArgs(ActualHeightProperty, _actualHeightMetadata, sizeInfo.PreviousSize.Height, sizeInfo.NewSize.Height));
		}
		RaiseEvent(sizeChangedEventArgs);
	}

	private Vector ComputeAlignmentOffset(Size clientSize, Size inkSize)
	{
		Vector result = default(Vector);
		HorizontalAlignment horizontalAlignment = HorizontalAlignment;
		VerticalAlignment verticalAlignment = VerticalAlignment;
		if (horizontalAlignment == HorizontalAlignment.Stretch && inkSize.Width > clientSize.Width)
		{
			horizontalAlignment = HorizontalAlignment.Left;
		}
		if (verticalAlignment == VerticalAlignment.Stretch && inkSize.Height > clientSize.Height)
		{
			verticalAlignment = VerticalAlignment.Top;
		}
		switch (horizontalAlignment)
		{
		case HorizontalAlignment.Center:
		case HorizontalAlignment.Stretch:
			result.X = (clientSize.Width - inkSize.Width) * 0.5;
			break;
		case HorizontalAlignment.Right:
			result.X = clientSize.Width - inkSize.Width;
			break;
		default:
			result.X = 0.0;
			break;
		}
		switch (verticalAlignment)
		{
		case VerticalAlignment.Center:
		case VerticalAlignment.Stretch:
			result.Y = (clientSize.Height - inkSize.Height) * 0.5;
			break;
		case VerticalAlignment.Bottom:
			result.Y = clientSize.Height - inkSize.Height;
			break;
		default:
			result.Y = 0.0;
			break;
		}
		return result;
	}

	/// <summary>Returns a geometry for a clipping mask. The mask applies if the layout system attempts to arrange an element that is larger than the available display space.</summary>
	/// <returns>The clipping geometry.</returns>
	/// <param name="layoutSlotSize">The size of the part of the element that does visual presentation. </param>
	protected override Geometry GetLayoutClip(Size layoutSlotSize)
	{
		bool useLayoutRounding = UseLayoutRounding;
		DpiScale dpi = GetDpi();
		if (useLayoutRounding && !CheckFlagsAnd(VisualFlags.UseLayoutRounding))
		{
			SetFlags(value: true, VisualFlags.UseLayoutRounding);
		}
		if (NeedsClipBounds || base.ClipToBounds)
		{
			MinMax minMax = new MinMax(this);
			if (useLayoutRounding && !FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness)
			{
				minMax.maxHeight = UIElement.RoundLayoutValue(minMax.maxHeight, dpi.DpiScaleY);
				minMax.maxWidth = UIElement.RoundLayoutValue(minMax.maxWidth, dpi.DpiScaleX);
				minMax.minHeight = UIElement.RoundLayoutValue(minMax.minHeight, dpi.DpiScaleY);
				minMax.minWidth = UIElement.RoundLayoutValue(minMax.minWidth, dpi.DpiScaleX);
			}
			Size renderSize = base.RenderSize;
			double num = (double.IsPositiveInfinity(minMax.maxWidth) ? renderSize.Width : minMax.maxWidth);
			double num2 = (double.IsPositiveInfinity(minMax.maxHeight) ? renderSize.Height : minMax.maxHeight);
			bool flag = base.ClipToBounds || DoubleUtil.LessThan(num, renderSize.Width) || DoubleUtil.LessThan(num2, renderSize.Height);
			renderSize.Width = Math.Min(renderSize.Width, minMax.maxWidth);
			renderSize.Height = Math.Min(renderSize.Height, minMax.maxHeight);
			LayoutTransformData value = LayoutTransformDataField.GetValue(this);
			Rect rect = default(Rect);
			if (value != null)
			{
				rect = Rect.Transform(new Rect(0.0, 0.0, renderSize.Width, renderSize.Height), value.Transform.Value);
				renderSize.Width = rect.Width;
				renderSize.Height = rect.Height;
			}
			Thickness margin = Margin;
			double num3 = margin.Left + margin.Right;
			double num4 = margin.Top + margin.Bottom;
			Size clientSize = new Size(Math.Max(0.0, layoutSlotSize.Width - num3), Math.Max(0.0, layoutSlotSize.Height - num4));
			bool flag2 = base.ClipToBounds || DoubleUtil.LessThan(clientSize.Width, renderSize.Width) || DoubleUtil.LessThan(clientSize.Height, renderSize.Height);
			Transform flowDirectionTransform = GetFlowDirectionTransform();
			if (flag && !flag2)
			{
				Rect rect2 = new Rect(0.0, 0.0, num, num2);
				if (useLayoutRounding)
				{
					rect2 = UIElement.RoundLayoutRect(rect2, dpi.DpiScaleX, dpi.DpiScaleY);
				}
				RectangleGeometry rectangleGeometry = new RectangleGeometry(rect2);
				if (flowDirectionTransform != null)
				{
					rectangleGeometry.Transform = flowDirectionTransform;
				}
				return rectangleGeometry;
			}
			if (flag2)
			{
				Vector vector = ComputeAlignmentOffset(clientSize, renderSize);
				if (value != null)
				{
					Rect rect3 = new Rect(0.0 - vector.X + rect.X, 0.0 - vector.Y + rect.Y, clientSize.Width, clientSize.Height);
					if (useLayoutRounding)
					{
						rect3 = UIElement.RoundLayoutRect(rect3, dpi.DpiScaleX, dpi.DpiScaleY);
					}
					RectangleGeometry rectangleGeometry2 = new RectangleGeometry(rect3);
					Matrix value2 = value.Transform.Value;
					if (value2.HasInverse)
					{
						value2.Invert();
						rectangleGeometry2.Transform = new MatrixTransform(value2);
					}
					if (flag)
					{
						Rect rect4 = new Rect(0.0, 0.0, num, num2);
						if (useLayoutRounding)
						{
							rect4 = UIElement.RoundLayoutRect(rect4, dpi.DpiScaleX, dpi.DpiScaleY);
						}
						PathGeometry pathGeometry = Geometry.Combine(new RectangleGeometry(rect4), rectangleGeometry2, GeometryCombineMode.Intersect, null);
						if (flowDirectionTransform != null)
						{
							pathGeometry.Transform = flowDirectionTransform;
						}
						return pathGeometry;
					}
					if (flowDirectionTransform != null)
					{
						if (rectangleGeometry2.Transform != null)
						{
							rectangleGeometry2.Transform = new MatrixTransform(rectangleGeometry2.Transform.Value * flowDirectionTransform.Value);
						}
						else
						{
							rectangleGeometry2.Transform = flowDirectionTransform;
						}
					}
					return rectangleGeometry2;
				}
				Rect rect5 = new Rect(0.0 - vector.X + rect.X, 0.0 - vector.Y + rect.Y, clientSize.Width, clientSize.Height);
				if (useLayoutRounding)
				{
					rect5 = UIElement.RoundLayoutRect(rect5, dpi.DpiScaleX, dpi.DpiScaleY);
				}
				if (flag)
				{
					Rect rect6 = new Rect(0.0, 0.0, num, num2);
					if (useLayoutRounding)
					{
						rect6 = UIElement.RoundLayoutRect(rect6, dpi.DpiScaleX, dpi.DpiScaleY);
					}
					rect5.Intersect(rect6);
				}
				RectangleGeometry rectangleGeometry3 = new RectangleGeometry(rect5);
				if (flowDirectionTransform != null)
				{
					rectangleGeometry3.Transform = flowDirectionTransform;
				}
				return rectangleGeometry3;
			}
			return null;
		}
		return base.GetLayoutClip(layoutSlotSize);
	}

	internal Geometry GetLayoutClipInternal()
	{
		if (base.IsMeasureValid && base.IsArrangeValid)
		{
			return GetLayoutClip(base.PreviousArrangeRect.Size);
		}
		return null;
	}

	/// <summary>When overridden in a derived class, measures the size in layout required for child elements and determines a size for the <see cref="T:System.Windows.FrameworkElement" />-derived class. </summary>
	/// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes.</returns>
	/// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
	protected virtual Size MeasureOverride(Size availableSize)
	{
		return new Size(0.0, 0.0);
	}

	/// <summary>When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement" /> derived class. </summary>
	/// <returns>The actual size used.</returns>
	/// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
	protected virtual Size ArrangeOverride(Size finalSize)
	{
		return finalSize;
	}

	internal static void InternalSetLayoutTransform(UIElement element, Transform layoutTransform)
	{
		FrameworkElement frameworkElement = element as FrameworkElement;
		element.InternalSetOffsetWorkaround(default(Vector));
		Transform transform = frameworkElement?.GetFlowDirectionTransform();
		Transform transform2 = element.RenderTransform;
		if (transform2 == Transform.Identity)
		{
			transform2 = null;
		}
		TransformCollection transformCollection = new TransformCollection();
		transformCollection.CanBeInheritanceContext = false;
		if (transform != null)
		{
			transformCollection.Add(transform);
		}
		if (transform2 != null)
		{
			transformCollection.Add(transform2);
		}
		transformCollection.Add(layoutTransform);
		TransformGroup transformGroup = new TransformGroup();
		transformGroup.Children = transformCollection;
		element.InternalSetTransformWorkaround(transformGroup);
	}

	private void SetLayoutOffset(Vector offset, Size oldRenderSize)
	{
		Transform transform;
		TransformGroup transformGroup;
		Point renderTransformOrigin;
		int num;
		if (!base.AreTransformsClean || !DoubleUtil.AreClose(base.RenderSize, oldRenderSize))
		{
			Transform flowDirectionTransform = GetFlowDirectionTransform();
			transform = base.RenderTransform;
			if (transform == Transform.Identity)
			{
				transform = null;
			}
			LayoutTransformData value = LayoutTransformDataField.GetValue(this);
			transformGroup = null;
			if (flowDirectionTransform != null || transform != null || value != null)
			{
				transformGroup = new TransformGroup();
				transformGroup.CanBeInheritanceContext = false;
				transformGroup.Children.CanBeInheritanceContext = false;
				if (flowDirectionTransform != null)
				{
					transformGroup.Children.Add(flowDirectionTransform);
				}
				if (value != null)
				{
					transformGroup.Children.Add(value.Transform);
					MinMax minMax = new MinMax(this);
					Size renderSize = base.RenderSize;
					if (double.IsPositiveInfinity(minMax.maxWidth))
					{
						_ = renderSize.Width;
					}
					if (double.IsPositiveInfinity(minMax.maxHeight))
					{
						_ = renderSize.Height;
					}
					renderSize.Width = Math.Min(renderSize.Width, minMax.maxWidth);
					renderSize.Height = Math.Min(renderSize.Height, minMax.maxHeight);
					Rect rect = Rect.Transform(new Rect(renderSize), value.Transform.Value);
					transformGroup.Children.Add(new TranslateTransform(0.0 - rect.X, 0.0 - rect.Y));
				}
				if (transform != null)
				{
					renderTransformOrigin = GetRenderTransformOrigin();
					if (renderTransformOrigin.X == 0.0)
					{
						num = ((renderTransformOrigin.Y != 0.0) ? 1 : 0);
						if (num == 0)
						{
							goto IL_01a1;
						}
					}
					else
					{
						num = 1;
					}
					TranslateTransform translateTransform = new TranslateTransform(0.0 - renderTransformOrigin.X, 0.0 - renderTransformOrigin.Y);
					translateTransform.Freeze();
					transformGroup.Children.Add(translateTransform);
					goto IL_01a1;
				}
			}
			goto IL_01da;
		}
		goto IL_01e9;
		IL_01a1:
		transformGroup.Children.Add(transform);
		if (num != 0)
		{
			TranslateTransform translateTransform2 = new TranslateTransform(renderTransformOrigin.X, renderTransformOrigin.Y);
			translateTransform2.Freeze();
			transformGroup.Children.Add(translateTransform2);
		}
		goto IL_01da;
		IL_01da:
		base.VisualTransform = transformGroup;
		base.AreTransformsClean = true;
		goto IL_01e9;
		IL_01e9:
		Vector visualOffset = base.VisualOffset;
		if (!DoubleUtil.AreClose(visualOffset.X, offset.X) || !DoubleUtil.AreClose(visualOffset.Y, offset.Y))
		{
			base.VisualOffset = offset;
		}
	}

	private Point GetRenderTransformOrigin()
	{
		Point renderTransformOrigin = base.RenderTransformOrigin;
		Size renderSize = base.RenderSize;
		return new Point(renderSize.Width * renderTransformOrigin.X, renderSize.Height * renderTransformOrigin.Y);
	}

	/// <summary>Moves the keyboard focus away from this element and to another element in a provided traversal direction. </summary>
	/// <returns>Returns true if focus is moved successfully; false if the target element in direction as specified does not exist or could not be keyboard focused.</returns>
	/// <param name="request">The direction that focus is to be moved, as a value of the enumeration.</param>
	public sealed override bool MoveFocus(TraversalRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		return KeyboardNavigation.Current.Navigate(this, request);
	}

	/// <summary>Determines the next element that would receive focus relative to this element for a provided focus movement direction, but does not actually move the focus.</summary>
	/// <returns>The next element that focus would move to if focus were actually traversed. May return null if focus cannot be moved relative to this element for the provided direction.</returns>
	/// <param name="direction">The direction for which a prospective focus change should be determined.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">Specified one of the following directions in the <see cref="T:System.Windows.Input.TraversalRequest" />: <see cref="F:System.Windows.Input.FocusNavigationDirection.Next" />, <see cref="F:System.Windows.Input.FocusNavigationDirection.Previous" />, <see cref="F:System.Windows.Input.FocusNavigationDirection.First" />, <see cref="F:System.Windows.Input.FocusNavigationDirection.Last" />. These directions are not legal for <see cref="M:System.Windows.FrameworkElement.PredictFocus(System.Windows.Input.FocusNavigationDirection)" /> (but they are legal for <see cref="M:System.Windows.FrameworkElement.MoveFocus(System.Windows.Input.TraversalRequest)" />). </exception>
	public sealed override DependencyObject PredictFocus(FocusNavigationDirection direction)
	{
		return KeyboardNavigation.Current.PredictFocusedElement(this, direction);
	}

	private static void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		if (e.OriginalSource != sender)
		{
			return;
		}
		IInputElement focusedElement = FocusManager.GetFocusedElement((FrameworkElement)sender, validate: true);
		if (focusedElement != null && focusedElement != sender && Keyboard.IsFocusable(focusedElement as DependencyObject))
		{
			IInputElement focusedElement2 = Keyboard.FocusedElement;
			focusedElement.Focus();
			if (Keyboard.FocusedElement == focusedElement || Keyboard.FocusedElement != focusedElement2)
			{
				e.Handled = true;
			}
		}
	}

	private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		if (sender == e.OriginalSource)
		{
			FrameworkElement frameworkElement = (FrameworkElement)sender;
			KeyboardNavigation.UpdateFocusedElement(frameworkElement);
			KeyboardNavigation current = KeyboardNavigation.Current;
			KeyboardNavigation.ShowFocusVisual();
			current.NotifyFocusChanged(frameworkElement, e);
			current.UpdateActiveElement(frameworkElement);
		}
	}

	private static void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		if (sender == e.OriginalSource)
		{
			KeyboardNavigation.Current.HideFocusVisual();
			if (e.NewFocus == null)
			{
				KeyboardNavigation.Current.NotifyFocusChanged(sender, e);
			}
		}
	}

	/// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.UIElement.GotFocus" /> event reaches this element in its route.</summary>
	/// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
	protected override void OnGotFocus(RoutedEventArgs e)
	{
		if (base.IsKeyboardFocused)
		{
			BringIntoView();
		}
		base.OnGotFocus(e);
	}

	/// <summary>Starts the initialization process for this element. </summary>
	public virtual void BeginInit()
	{
		if (ReadInternalFlag(InternalFlags.InitPending))
		{
			throw new InvalidOperationException(SR.NestedBeginInitNotSupported);
		}
		WriteInternalFlag(InternalFlags.InitPending, set: true);
	}

	/// <summary>Indicates that the initialization process for the element is complete. </summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="M:System.Windows.FrameworkElement.EndInit" /> was called without <see cref="M:System.Windows.FrameworkElement.BeginInit" /> having previously been called on the element.</exception>
	public virtual void EndInit()
	{
		if (!ReadInternalFlag(InternalFlags.InitPending))
		{
			throw new InvalidOperationException(SR.EndInitWithoutBeginInitNotSupported);
		}
		WriteInternalFlag(InternalFlags.InitPending, set: false);
		TryFireInitialized();
	}

	/// <summary>Raises the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to true internally. </summary>
	/// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
	protected virtual void OnInitialized(EventArgs e)
	{
		if (!HasStyleEverBeenFetched)
		{
			UpdateStyleProperty();
		}
		if (!HasThemeStyleEverBeenFetched)
		{
			UpdateThemeStyleProperty();
		}
		RaiseInitialized(InitializedKey, e);
	}

	private void TryFireInitialized()
	{
		if (!ReadInternalFlag(InternalFlags.InitPending) && !ReadInternalFlag(InternalFlags.IsInitialized))
		{
			WriteInternalFlag(InternalFlags.IsInitialized, set: true);
			PrivateInitialized();
			OnInitialized(EventArgs.Empty);
		}
	}

	private void RaiseInitialized(EventPrivateKey key, EventArgs e)
	{
		EventHandlersStore eventHandlersStore = base.EventHandlersStore;
		if (eventHandlersStore != null)
		{
			Delegate @delegate = eventHandlersStore.Get(key);
			if ((object)@delegate != null)
			{
				((EventHandler)@delegate)(this, e);
			}
		}
	}

	private static void NumberSubstitutionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		((FrameworkElement)o).HasNumberSubstitutionChanged = true;
	}

	private static bool ShouldUseSystemFont(FrameworkElement fe, DependencyProperty dp)
	{
		bool hasModifiers;
		if ((SystemResources.SystemResourcesAreChanging || (fe.ReadInternalFlag(InternalFlags.CreatingRoot) && SystemResources.SystemResourcesHaveChanged)) && fe._parent == null && VisualTreeHelper.GetParent(fe) == null)
		{
			return fe.GetValueSource(dp, null, out hasModifiers) == BaseValueSourceInternal.Default;
		}
		return false;
	}

	private static object CoerceFontFamily(DependencyObject o, object value)
	{
		if (ShouldUseSystemFont((FrameworkElement)o, TextElement.FontFamilyProperty))
		{
			return SystemFonts.MessageFontFamily;
		}
		return value;
	}

	private static object CoerceFontSize(DependencyObject o, object value)
	{
		if (ShouldUseSystemFont((FrameworkElement)o, TextElement.FontSizeProperty))
		{
			return SystemFonts.MessageFontSize;
		}
		return value;
	}

	private static object CoerceFontStyle(DependencyObject o, object value)
	{
		if (ShouldUseSystemFont((FrameworkElement)o, TextElement.FontStyleProperty))
		{
			return SystemFonts.MessageFontStyle;
		}
		return value;
	}

	private static object CoerceFontWeight(DependencyObject o, object value)
	{
		if (ShouldUseSystemFont((FrameworkElement)o, TextElement.FontWeightProperty))
		{
			return SystemFonts.MessageFontWeight;
		}
		return value;
	}

	internal sealed override void OnPresentationSourceChanged(bool attached)
	{
		base.OnPresentationSourceChanged(attached);
		if (attached)
		{
			FireLoadedOnDescendentsInternal();
			if (SystemResources.SystemResourcesHaveChanged)
			{
				WriteInternalFlag(InternalFlags.CreatingRoot, set: true);
				CoerceValue(TextElement.FontFamilyProperty);
				CoerceValue(TextElement.FontSizeProperty);
				CoerceValue(TextElement.FontStyleProperty);
				CoerceValue(TextElement.FontWeightProperty);
				WriteInternalFlag(InternalFlags.CreatingRoot, set: false);
			}
		}
		else
		{
			FireUnloadedOnDescendentsInternal();
		}
	}

	internal override void OnAddHandler(RoutedEvent routedEvent, Delegate handler)
	{
		base.OnAddHandler(routedEvent, handler);
		if (routedEvent == LoadedEvent || routedEvent == UnloadedEvent)
		{
			BroadcastEventHelper.AddHasLoadedChangeHandlerFlagInAncestry(this);
		}
	}

	internal override void OnRemoveHandler(RoutedEvent routedEvent, Delegate handler)
	{
		base.OnRemoveHandler(routedEvent, handler);
		if ((routedEvent == LoadedEvent || routedEvent == UnloadedEvent) && !ThisHasLoadedChangeEventHandler)
		{
			BroadcastEventHelper.RemoveHasLoadedChangeHandlerFlagInAncestry(this);
		}
	}

	internal void OnLoaded(RoutedEventArgs args)
	{
		RaiseEvent(args);
	}

	internal void OnUnloaded(RoutedEventArgs args)
	{
		RaiseEvent(args);
	}

	internal override void AddSynchronizedInputPreOpportunityHandlerCore(EventRoute route, RoutedEventArgs args)
	{
		if (_templatedParent is UIElement uIElement)
		{
			uIElement.AddSynchronizedInputPreOpportunityHandler(route, args);
		}
	}

	internal void RaiseClrEvent(EventPrivateKey key, EventArgs args)
	{
		EventHandlersStore eventHandlersStore = base.EventHandlersStore;
		if (eventHandlersStore != null)
		{
			Delegate @delegate = eventHandlersStore.Get(key);
			if ((object)@delegate != null)
			{
				((EventHandler)@delegate)(this, args);
			}
		}
	}

	private static FrameworkServices EnsureFrameworkServices()
	{
		if (_frameworkServices == null)
		{
			_frameworkServices = new FrameworkServices();
		}
		return _frameworkServices;
	}

	private static void OnToolTipOpeningThunk(object sender, ToolTipEventArgs e)
	{
		((FrameworkElement)sender).OnToolTipOpening(e);
	}

	/// <summary> Invoked whenever the <see cref="E:System.Windows.FrameworkElement.ToolTipOpening" /> routed event reaches this class in its route. Implement this method to add class handling for this event. </summary>
	/// <param name="e">Provides data about the event.</param>
	protected virtual void OnToolTipOpening(ToolTipEventArgs e)
	{
	}

	private static void OnToolTipClosingThunk(object sender, ToolTipEventArgs e)
	{
		((FrameworkElement)sender).OnToolTipClosing(e);
	}

	/// <summary> Invoked whenever an unhandled <see cref="E:System.Windows.FrameworkElement.ToolTipClosing" /> routed event reaches this class in its route. Implement this method to add class handling for this event. </summary>
	/// <param name="e">Provides data about the event.</param>
	protected virtual void OnToolTipClosing(ToolTipEventArgs e)
	{
	}

	private static void OnContextMenuOpeningThunk(object sender, ContextMenuEventArgs e)
	{
		((FrameworkElement)sender).OnContextMenuOpening(e);
	}

	/// <summary> Invoked whenever an unhandled <see cref="E:System.Windows.FrameworkElement.ContextMenuOpening" /> routed event reaches this class in its route. Implement this method to add class handling for this event. </summary>
	/// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
	protected virtual void OnContextMenuOpening(ContextMenuEventArgs e)
	{
	}

	private static void OnContextMenuClosingThunk(object sender, ContextMenuEventArgs e)
	{
		((FrameworkElement)sender).OnContextMenuClosing(e);
	}

	/// <summary> Invoked whenever an unhandled <see cref="E:System.Windows.FrameworkElement.ContextMenuClosing" /> routed event reaches this class in its route. Implement this method to add class handling for this event. </summary>
	/// <param name="e">Provides data about the event.</param>
	protected virtual void OnContextMenuClosing(ContextMenuEventArgs e)
	{
	}

	private void RaiseDependencyPropertyChanged(EventPrivateKey key, DependencyPropertyChangedEventArgs args)
	{
		EventHandlersStore eventHandlersStore = base.EventHandlersStore;
		if (eventHandlersStore != null)
		{
			Delegate @delegate = eventHandlersStore.Get(key);
			if ((object)@delegate != null)
			{
				((DependencyPropertyChangedEventHandler)@delegate)(this, args);
			}
		}
	}

	internal static void AddIntermediateElementsToRoute(DependencyObject mergePoint, EventRoute route, RoutedEventArgs args, DependencyObject modelTreeNode)
	{
		while (modelTreeNode != null && modelTreeNode != mergePoint)
		{
			UIElement uIElement = modelTreeNode as UIElement;
			ContentElement contentElement = modelTreeNode as ContentElement;
			UIElement3D uIElement3D = modelTreeNode as UIElement3D;
			if (uIElement != null)
			{
				uIElement.AddToEventRoute(route, args);
				if (uIElement is FrameworkElement fe)
				{
					AddStyleHandlersToEventRoute(fe, null, route, args);
				}
			}
			else if (contentElement != null)
			{
				contentElement.AddToEventRoute(route, args);
				if (contentElement is FrameworkContentElement fce)
				{
					AddStyleHandlersToEventRoute(null, fce, route, args);
				}
			}
			else
			{
				uIElement3D?.AddToEventRoute(route, args);
			}
			modelTreeNode = LogicalTreeHelper.GetParent(modelTreeNode);
		}
	}

	private bool IsLogicalDescendent(DependencyObject child)
	{
		while (child != null)
		{
			if (child == this)
			{
				return true;
			}
			child = LogicalTreeHelper.GetParent(child);
		}
		return false;
	}

	internal void EventHandlersStoreAdd(EventPrivateKey key, Delegate handler)
	{
		EnsureEventHandlersStore();
		base.EventHandlersStore.Add(key, handler);
	}

	internal void EventHandlersStoreRemove(EventPrivateKey key, Delegate handler)
	{
		base.EventHandlersStore?.Remove(key, handler);
	}

	internal bool ReadInternalFlag(InternalFlags reqFlag)
	{
		return (_flags & reqFlag) != 0;
	}

	internal bool ReadInternalFlag2(InternalFlags2 reqFlag)
	{
		return (_flags2 & reqFlag) != 0;
	}

	internal void WriteInternalFlag(InternalFlags reqFlag, bool set)
	{
		if (set)
		{
			_flags |= reqFlag;
		}
		else
		{
			_flags &= ~reqFlag;
		}
	}

	internal void WriteInternalFlag2(InternalFlags2 reqFlag, bool set)
	{
		if (set)
		{
			_flags2 |= reqFlag;
		}
		else
		{
			_flags2 &= ~reqFlag;
		}
	}

	/// <summary>Provides an accessor that simplifies access to the <see cref="T:System.Windows.NameScope" /> registration method.</summary>
	/// <param name="name">Name to use for the specified name-object mapping.</param>
	/// <param name="scopedElement">Object for the mapping.</param>
	public void RegisterName(string name, object scopedElement)
	{
		INameScope nameScope = FindScope(this);
		if (nameScope != null)
		{
			nameScope.RegisterName(name, scopedElement);
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.NameScopeNotFound, name, "register"));
	}

	/// <summary>Simplifies access to the <see cref="T:System.Windows.NameScope" /> de-registration method.</summary>
	/// <param name="name">Name of the name-object pair to remove from the current scope.</param>
	public void UnregisterName(string name)
	{
		INameScope nameScope = FindScope(this);
		if (nameScope != null)
		{
			nameScope.UnregisterName(name);
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.NameScopeNotFound, name, "unregister"));
	}

	/// <summary>Finds an element that has the provided identifier name. </summary>
	/// <returns>The requested element. This can be null if no matching element was found.</returns>
	/// <param name="name">The name of the requested element.</param>
	public object FindName(string name)
	{
		DependencyObject scopeOwner;
		return FindName(name, out scopeOwner);
	}

	internal object FindName(string name, out DependencyObject scopeOwner)
	{
		return FindScope(this, out scopeOwner)?.FindName(name);
	}

	/// <summary>Reapplies the default style to the current <see cref="T:System.Windows.FrameworkElement" />.</summary>
	public void UpdateDefaultStyle()
	{
		TreeWalkHelper.InvalidateOnResourcesChange(this, null, ResourcesChangeInfo.ThemeChangeInfo);
	}

	internal object FindResourceOnSelf(object resourceKey, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference)
	{
		ResourceDictionary value = ResourcesField.GetValue(this);
		bool canCache;
		if (value != null && value.Contains(resourceKey))
		{
			return value.FetchResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference, out canCache);
		}
		return DependencyProperty.UnsetValue;
	}

	internal DependencyObject ContextVerifiedGetParent()
	{
		return _parent;
	}

	/// <summary>Adds the provided object to the logical tree of this element. </summary>
	/// <param name="child">Child element to be added.</param>
	protected internal void AddLogicalChild(object child)
	{
		if (child != null)
		{
			if (IsLogicalChildrenIterationInProgress)
			{
				throw new InvalidOperationException(SR.CannotModifyLogicalChildrenDuringTreeWalk);
			}
			TryFireInitialized();
			bool flag = true;
			try
			{
				HasLogicalChildren = true;
				new FrameworkObject(child as DependencyObject).ChangeLogicalParent(this);
				flag = false;
			}
			finally
			{
			}
		}
	}

	/// <summary>Removes the provided object from this element's logical tree. <see cref="T:System.Windows.FrameworkElement" /> updates the affected logical tree parent pointers to keep in sync with this deletion.</summary>
	/// <param name="child">The element to remove.</param>
	protected internal void RemoveLogicalChild(object child)
	{
		if (child != null)
		{
			if (IsLogicalChildrenIterationInProgress)
			{
				throw new InvalidOperationException(SR.CannotModifyLogicalChildrenDuringTreeWalk);
			}
			FrameworkObject frameworkObject = new FrameworkObject(child as DependencyObject);
			if (frameworkObject.Parent == this)
			{
				frameworkObject.ChangeLogicalParent(null);
			}
			IEnumerator logicalChildren = LogicalChildren;
			if (logicalChildren == null)
			{
				HasLogicalChildren = false;
			}
			else
			{
				HasLogicalChildren = logicalChildren.MoveNext();
			}
		}
	}

	internal void ChangeLogicalParent(DependencyObject newParent)
	{
		VerifyAccess();
		newParent?.VerifyAccess();
		if (_parent != null && newParent != null && _parent != newParent)
		{
			throw new InvalidOperationException(SR.HasLogicalParent);
		}
		if (newParent == this)
		{
			throw new InvalidOperationException(SR.CannotBeSelfParent);
		}
		VisualDiagnostics.VerifyVisualTreeChange(this);
		if (newParent != null)
		{
			ClearInheritanceContext();
		}
		IsParentAnFE = newParent is FrameworkElement;
		DependencyObject parent = _parent;
		OnNewParent(newParent);
		BroadcastEventHelper.AddOrRemoveHasLoadedChangeHandlerFlag(this, parent, newParent);
		DependencyObject parent2 = ((newParent != null) ? newParent : parent);
		TreeWalkHelper.InvalidateOnTreeChange(this, null, parent2, newParent != null);
		TryFireInitialized();
	}

	internal virtual void OnNewParent(DependencyObject newParent)
	{
		DependencyObject parent = _parent;
		_parent = newParent;
		if (_parent != null && _parent is ContentElement)
		{
			UIElement.SynchronizeForceInheritProperties(this, null, null, _parent);
		}
		else if (parent is ContentElement)
		{
			UIElement.SynchronizeForceInheritProperties(this, null, null, parent);
		}
		SynchronizeReverseInheritPropertyFlags(parent, isCoreParent: false);
	}

	internal void OnAncestorChangedInternal(TreeChangeInfo parentTreeState)
	{
		bool isSelfInheritanceParent = base.IsSelfInheritanceParent;
		if (parentTreeState.Root != this)
		{
			HasStyleChanged = false;
			HasStyleInvalidated = false;
			HasTemplateChanged = false;
		}
		if (parentTreeState.IsAddOperation)
		{
			new FrameworkObject(this, null).SetShouldLookupImplicitStyles();
		}
		if (HasResourceReference)
		{
			TreeWalkHelper.OnResourcesChanged(this, ResourcesChangeInfo.TreeChangeInfo, raiseResourceChangedEvent: false);
		}
		FrugalObjectList<DependencyProperty> item = InvalidateTreeDependentProperties(parentTreeState, base.IsSelfInheritanceParent, isSelfInheritanceParent);
		parentTreeState.InheritablePropertiesStack.Push(item);
		OnAncestorChanged();
		if (PotentiallyHasMentees)
		{
			RaiseClrEvent(ResourcesChangedKey, EventArgs.Empty);
		}
	}

	internal FrugalObjectList<DependencyProperty> InvalidateTreeDependentProperties(TreeChangeInfo parentTreeState, bool isSelfInheritanceParent, bool wasSelfInheritanceParent)
	{
		AncestorChangeInProgress = true;
		InVisibilityCollapsedTree = false;
		if (parentTreeState.TopmostCollapsedParentNode == null)
		{
			if (base.Visibility == Visibility.Collapsed)
			{
				parentTreeState.TopmostCollapsedParentNode = this;
				InVisibilityCollapsedTree = true;
			}
		}
		else
		{
			InVisibilityCollapsedTree = true;
		}
		try
		{
			if (IsInitialized && !HasLocalStyle && this != parentTreeState.Root)
			{
				UpdateStyleProperty();
			}
			Style style = null;
			Style style2 = null;
			int num = -1;
			ChildRecord childRecord = default(ChildRecord);
			bool isChildRecordValid = false;
			style = Style;
			style2 = ThemeStyle;
			DependencyObject templatedParent = TemplatedParent;
			num = TemplateChildIndex;
			bool hasStyleChanged = HasStyleChanged;
			GetTemplatedParentChildRecord(templatedParent, num, out childRecord, out isChildRecordValid);
			FrameworkElement feParent;
			FrameworkContentElement fceParent;
			bool frameworkParent = GetFrameworkParent(this, out feParent, out fceParent);
			DependencyObject parent = null;
			InheritanceBehavior inheritanceBehavior = InheritanceBehavior.Default;
			if (frameworkParent)
			{
				if (feParent != null)
				{
					parent = feParent;
					inheritanceBehavior = feParent.InheritanceBehavior;
				}
				else
				{
					parent = fceParent;
					inheritanceBehavior = fceParent.InheritanceBehavior;
				}
			}
			if (!TreeWalkHelper.SkipNext(InheritanceBehavior) && !TreeWalkHelper.SkipNow(inheritanceBehavior))
			{
				SynchronizeInheritanceParent(parent);
			}
			else if (!base.IsSelfInheritanceParent)
			{
				SetIsSelfInheritanceParent();
			}
			return TreeWalkHelper.InvalidateTreeDependentProperties(parentTreeState, this, null, style, style2, ref childRecord, isChildRecordValid, hasStyleChanged, isSelfInheritanceParent, wasSelfInheritanceParent);
		}
		finally
		{
			AncestorChangeInProgress = false;
			InVisibilityCollapsedTree = false;
		}
	}

	internal void UpdateStyleProperty()
	{
		if (HasStyleInvalidated)
		{
			return;
		}
		if (!IsStyleUpdateInProgress)
		{
			IsStyleUpdateInProgress = true;
			try
			{
				InvalidateProperty(StyleProperty);
				HasStyleInvalidated = true;
				return;
			}
			finally
			{
				IsStyleUpdateInProgress = false;
			}
		}
		throw new InvalidOperationException(SR.Format(SR.CyclicStyleReferenceDetected, this));
	}

	internal void UpdateThemeStyleProperty()
	{
		if (!IsThemeStyleUpdateInProgress)
		{
			IsThemeStyleUpdateInProgress = true;
			try
			{
				StyleHelper.GetThemeStyle(this, null);
				if (GetValueEntry(LookupEntry(ContextMenuProperty.GlobalIndex), ContextMenuProperty, null, RequestFlags.DeferredReferences).Value is ContextMenu fe)
				{
					TreeWalkHelper.InvalidateOnResourcesChange(fe, null, ResourcesChangeInfo.ThemeChangeInfo);
				}
				if (GetValueEntry(LookupEntry(ToolTipProperty.GlobalIndex), ToolTipProperty, null, RequestFlags.DeferredReferences).Value is DependencyObject d)
				{
					FrameworkObject frameworkObject = new FrameworkObject(d);
					if (frameworkObject.IsValid)
					{
						TreeWalkHelper.InvalidateOnResourcesChange(frameworkObject.FE, frameworkObject.FCE, ResourcesChangeInfo.ThemeChangeInfo);
					}
				}
				OnThemeChanged();
				return;
			}
			finally
			{
				IsThemeStyleUpdateInProgress = false;
			}
		}
		throw new InvalidOperationException(SR.Format(SR.CyclicThemeStyleReferenceDetected, this));
	}

	internal virtual void OnThemeChanged()
	{
	}

	internal void FireLoadedOnDescendentsInternal()
	{
		if (LoadedPending == null)
		{
			DependencyObject parent = Parent;
			if (parent == null)
			{
				parent = VisualTreeHelper.GetParent(this);
			}
			object[] unloadedPending = UnloadedPending;
			if (unloadedPending == null || unloadedPending[2] != parent)
			{
				BroadcastEventHelper.AddLoadedCallback(this, parent);
			}
			else
			{
				BroadcastEventHelper.RemoveUnloadedCallback(this, unloadedPending);
			}
		}
	}

	internal void FireUnloadedOnDescendentsInternal()
	{
		if (UnloadedPending == null)
		{
			DependencyObject parent = Parent;
			if (parent == null)
			{
				parent = VisualTreeHelper.GetParent(this);
			}
			object[] loadedPending = LoadedPending;
			if (loadedPending == null)
			{
				BroadcastEventHelper.AddUnloadedCallback(this, parent);
			}
			else
			{
				BroadcastEventHelper.RemoveLoadedCallback(this, loadedPending);
			}
		}
	}

	internal override bool ShouldProvideInheritanceContext(DependencyObject target, DependencyProperty property)
	{
		return !new FrameworkObject(target).IsValid;
	}

	internal override void AddInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		base.AddInheritanceContext(context, property);
		TryFireInitialized();
		if ((property == VisualBrush.VisualProperty || property == BitmapCacheBrush.TargetProperty) && GetFrameworkParent(this) == null && !FrameworkObject.IsEffectiveAncestor(this, context))
		{
			if (!HasMultipleInheritanceContexts && InheritanceContext == null)
			{
				InheritanceContextField.SetValue(this, context);
				OnInheritanceContextChanged(EventArgs.Empty);
			}
			else if (InheritanceContext != null)
			{
				InheritanceContextField.ClearValue(this);
				WriteInternalFlag2(InternalFlags2.HasMultipleInheritanceContexts, set: true);
				OnInheritanceContextChanged(EventArgs.Empty);
			}
		}
	}

	internal override void RemoveInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		if (InheritanceContext == context)
		{
			InheritanceContextField.ClearValue(this);
			OnInheritanceContextChanged(EventArgs.Empty);
		}
		base.RemoveInheritanceContext(context, property);
	}

	private void ClearInheritanceContext()
	{
		if (InheritanceContext != null)
		{
			InheritanceContextField.ClearValue(this);
			OnInheritanceContextChanged(EventArgs.Empty);
		}
	}

	internal override void OnInheritanceContextChangedCore(EventArgs args)
	{
		DependencyObject value = MentorField.GetValue(this);
		DependencyObject dependencyObject = Helper.FindMentor(InheritanceContext);
		if (value != dependencyObject)
		{
			MentorField.SetValue(this, dependencyObject);
			if (value != null)
			{
				DisconnectMentor(value);
			}
			if (dependencyObject != null)
			{
				ConnectMentor(dependencyObject);
			}
		}
	}

	private void ConnectMentor(DependencyObject mentor)
	{
		FrameworkObject foMentor = new FrameworkObject(mentor);
		foMentor.InheritedPropertyChanged += OnMentorInheritedPropertyChanged;
		foMentor.ResourcesChanged += OnMentorResourcesChanged;
		TreeWalkHelper.InvalidateOnTreeChange(this, null, foMentor.DO, isAddOperation: true);
		if (SubtreeHasLoadedChangeHandler)
		{
			bool isLoaded = foMentor.IsLoaded;
			ConnectLoadedEvents(ref foMentor, isLoaded);
			if (isLoaded)
			{
				FireLoadedOnDescendentsInternal();
			}
		}
	}

	private void DisconnectMentor(DependencyObject mentor)
	{
		FrameworkObject foMentor = new FrameworkObject(mentor);
		foMentor.InheritedPropertyChanged -= OnMentorInheritedPropertyChanged;
		foMentor.ResourcesChanged -= OnMentorResourcesChanged;
		TreeWalkHelper.InvalidateOnTreeChange(this, null, foMentor.DO, isAddOperation: false);
		if (SubtreeHasLoadedChangeHandler)
		{
			bool isLoaded = foMentor.IsLoaded;
			DisconnectLoadedEvents(ref foMentor, isLoaded);
			if (foMentor.IsLoaded)
			{
				FireUnloadedOnDescendentsInternal();
			}
		}
	}

	internal void ChangeSubtreeHasLoadedChangedHandler(DependencyObject mentor)
	{
		FrameworkObject foMentor = new FrameworkObject(mentor);
		bool isLoaded = foMentor.IsLoaded;
		if (SubtreeHasLoadedChangeHandler)
		{
			ConnectLoadedEvents(ref foMentor, isLoaded);
		}
		else
		{
			DisconnectLoadedEvents(ref foMentor, isLoaded);
		}
	}

	private void OnMentorLoaded(object sender, RoutedEventArgs e)
	{
		FrameworkObject frameworkObject = new FrameworkObject((DependencyObject)sender);
		frameworkObject.Loaded -= OnMentorLoaded;
		frameworkObject.Unloaded += OnMentorUnloaded;
		BroadcastEventHelper.BroadcastLoadedSynchronously(this, IsLoaded);
	}

	private void OnMentorUnloaded(object sender, RoutedEventArgs e)
	{
		FrameworkObject frameworkObject = new FrameworkObject((DependencyObject)sender);
		frameworkObject.Unloaded -= OnMentorUnloaded;
		frameworkObject.Loaded += OnMentorLoaded;
		BroadcastEventHelper.BroadcastUnloadedSynchronously(this, IsLoaded);
	}

	private void ConnectLoadedEvents(ref FrameworkObject foMentor, bool isLoaded)
	{
		if (foMentor.IsValid)
		{
			if (isLoaded)
			{
				foMentor.Unloaded += OnMentorUnloaded;
			}
			else
			{
				foMentor.Loaded += OnMentorLoaded;
			}
		}
	}

	private void DisconnectLoadedEvents(ref FrameworkObject foMentor, bool isLoaded)
	{
		if (foMentor.IsValid)
		{
			if (isLoaded)
			{
				foMentor.Unloaded -= OnMentorUnloaded;
			}
			else
			{
				foMentor.Loaded -= OnMentorLoaded;
			}
		}
	}

	private void OnMentorInheritedPropertyChanged(object sender, InheritedPropertyChangedEventArgs e)
	{
		TreeWalkHelper.InvalidateOnInheritablePropertyChange(this, null, e.Info, skipStartNode: false);
	}

	private void OnMentorResourcesChanged(object sender, EventArgs e)
	{
		TreeWalkHelper.InvalidateOnResourcesChange(this, null, ResourcesChangeInfo.CatastrophicDictionaryChangeInfo);
	}

	internal void RaiseInheritedPropertyChangedEvent(ref InheritablePropertyChangeInfo info)
	{
		EventHandlersStore eventHandlersStore = base.EventHandlersStore;
		if (eventHandlersStore != null)
		{
			Delegate @delegate = eventHandlersStore.Get(InheritedPropertyChangedKey);
			if ((object)@delegate != null)
			{
				InheritedPropertyChangedEventArgs e = new InheritedPropertyChangedEventArgs(ref info);
				((InheritedPropertyChangedEventHandler)@delegate)(this, e);
			}
		}
	}
}
