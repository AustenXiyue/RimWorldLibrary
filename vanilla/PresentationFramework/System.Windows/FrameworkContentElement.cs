using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Diagnostics;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;
using MS.Utility;

namespace System.Windows;

/// <summary>
///   <see cref="T:System.Windows.FrameworkContentElement" /> is the WPF framework-level implementation and expansion of the <see cref="T:System.Windows.ContentElement" /> base class. <see cref="T:System.Windows.FrameworkContentElement" /> adds support for additional input APIs (including tooltips and context menus), storyboards, data context for data binding, styles support, and logical tree helper APIs. </summary>
[StyleTypedProperty(Property = "FocusVisualStyle", StyleTargetType = typeof(Control))]
[XmlLangProperty("Language")]
[UsableDuringInitialization(true)]
[RuntimeNameProperty("Name")]
public class FrameworkContentElement : ContentElement, IFrameworkInputElement, IInputElement, ISupportInitialize, IQueryAmbient
{
	internal static readonly NumberSubstitution DefaultNumberSubstitution;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.Style" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkContentElement.Style" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty StyleProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.OverridesDefaultStyle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkContentElement.OverridesDefaultStyle" /> dependency property.</returns>
	public static readonly DependencyProperty OverridesDefaultStyleProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.DefaultStyleKey" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkContentElement.DefaultStyleKey" /> dependency property identifier.</returns>
	protected internal static readonly DependencyProperty DefaultStyleKeyProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.Name" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkContentElement.Name" /> dependency property.</returns>
	public static readonly DependencyProperty NameProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.Tag" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkContentElement.Tag" /> dependency property.</returns>
	public static readonly DependencyProperty TagProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.Language" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkContentElement.Language" /> dependency property identifier.</returns>
	public static readonly DependencyProperty LanguageProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.FocusVisualStyle" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkContentElement.FocusVisualStyle" /> dependency property.</returns>
	public static readonly DependencyProperty FocusVisualStyleProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.Cursor" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkContentElement.Cursor" /> dependency property.</returns>
	public static readonly DependencyProperty CursorProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.ForceCursor" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkContentElement.ForceCursor" /> dependency property.</returns>
	public static readonly DependencyProperty ForceCursorProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.InputScope" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkContentElement.InputScope" /> dependency property.</returns>
	public static readonly DependencyProperty InputScopeProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.DataContext" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkContentElement.DataContext" /> dependency property identifier.</returns>
	public static readonly DependencyProperty DataContextProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.FrameworkContentElement.BindingGroup" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.FrameworkContentElement.BindingGroup" /> dependency property.</returns>
	public static readonly DependencyProperty BindingGroupProperty;

	private static readonly DependencyProperty LoadedPendingProperty;

	private static readonly DependencyProperty UnloadedPendingProperty;

	/// <summary> Identifies the <see cref="E:System.Windows.FrameworkContentElement.Loaded" /> Routed Events Overview. </summary>
	/// <returns>The <see cref="E:System.Windows.FrameworkContentElement.Loaded" /> event's identifier.</returns>
	public static readonly RoutedEvent LoadedEvent;

	/// <summary> Identifies the <see cref="E:System.Windows.FrameworkContentElement.Unloaded" /> Routed Events Overview. </summary>
	/// <returns>The <see cref="E:System.Windows.FrameworkContentElement.Unloaded" /> event's identifier.</returns>
	public static readonly RoutedEvent UnloadedEvent;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.ToolTip" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkContentElement.ToolTip" /> dependency property identifier.</returns>
	public static readonly DependencyProperty ToolTipProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.FrameworkContentElement.ContextMenu" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.FrameworkContentElement.ContextMenu" /> dependency property identifier.</returns>
	public static readonly DependencyProperty ContextMenuProperty;

	/// <summary> Identifies the <see cref="E:System.Windows.FrameworkContentElement.ToolTipOpening" /> Routed Events Overview. </summary>
	/// <returns>The <see cref="E:System.Windows.FrameworkContentElement.ToolTipOpening" /> event's identifier.</returns>
	public static readonly RoutedEvent ToolTipOpeningEvent;

	/// <summary> Identifies the <see cref="E:System.Windows.FrameworkContentElement.ToolTipClosing" /> Routed Events Overview. </summary>
	/// <returns>The <see cref="E:System.Windows.FrameworkContentElement.ToolTipClosing" /> event's identifier.</returns>
	public static readonly RoutedEvent ToolTipClosingEvent;

	/// <summary> Identifies the <see cref="E:System.Windows.FrameworkContentElement.ContextMenuOpening" /> Routed Events Overview. </summary>
	/// <returns>The <see cref="E:System.Windows.FrameworkContentElement.ContextMenuOpening" /> event's identifier.</returns>
	public static readonly RoutedEvent ContextMenuOpeningEvent;

	/// <summary> Identifies the <see cref="E:System.Windows.FrameworkContentElement.ContextMenuClosing" /> Routed Events Overview. </summary>
	/// <returns>The <see cref="E:System.Windows.FrameworkContentElement.ContextMenuClosing" /> event's identifier.</returns>
	public static readonly RoutedEvent ContextMenuClosingEvent;

	private Style _styleCache;

	private Style _themeStyleCache;

	internal DependencyObject _templatedParent;

	private static readonly UncommonField<ResourceDictionary> ResourcesField;

	private InternalFlags _flags;

	private InternalFlags2 _flags2 = InternalFlags2.Default;

	private new DependencyObject _parent;

	private FrugalObjectList<DependencyProperty> _inheritableProperties;

	private static readonly UncommonField<DependencyObject> InheritanceContextField;

	private static readonly UncommonField<DependencyObject> MentorField;

	/// <summary>Gets or sets the style to be used by this element.  </summary>
	/// <returns>The applied, nondefault style for the element, if present. Otherwise, null. The default for a default-constructed <see cref="T:System.Windows.FrameworkContentElement" /> is null.</returns>
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

	/// <summary>Gets or sets a value indicating whether this element incorporates style properties from theme styles. </summary>
	/// <returns>true if this element does not use theme style properties; all style-originating properties come from local application styles, and theme style properties do not apply. false if application styles apply first, and then theme styles apply for properties that were not specifically set in application styles.</returns>
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

	/// <summary>Gets or sets the key to use to find the style template for this control in themes. </summary>
	/// <returns>The style key. To work correctly as part of theme style lookup, this value is expected to be the <see cref="T:System.Type" /> of the element being styled. null is an accepted value for a certain case; see Remarks.</returns>
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

	/// <summary> Gets a reference to the template parent of this element. This property is not relevant if the element was not created through a template. </summary>
	/// <returns>The element whose <see cref="T:System.Windows.FrameworkTemplate" /> <see cref="P:System.Windows.FrameworkTemplate.VisualTree" /> caused this element to be created. This value is frequently null; see Remarks.</returns>
	public DependencyObject TemplatedParent => _templatedParent;

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

	/// <summary>Gets or sets the current locally-defined resource dictionary. </summary>
	/// <returns>The current locally-defined resources. This is a dictionary of resources, where resources within the dictionary are accessed by key.</returns>
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
			}
			return resourceDictionary;
		}
		set
		{
			ResourceDictionary value2 = ResourcesField.GetValue(this);
			ResourcesField.SetValue(this, value);
			value2?.RemoveOwner(this);
			if (value != null && !value.ContainsOwner(this))
			{
				value.AddOwner(this);
			}
			if (value2 != value)
			{
				TreeWalkHelper.InvalidateOnResourcesChange(null, this, new ResourcesChangeInfo(value2, value));
			}
		}
	}

	/// <summary>Gets or sets the identifying name of the element. The name provides an instance reference so that programmatic code-behind, such as event handler code, can refer to an element once it is constructed during parsing of XAML. </summary>
	/// <returns>The name of the element.</returns>
	[MergableProperty(false)]
	[Localizability(LocalizationCategory.NeverLocalize)]
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

	/// <summary>Gets or sets an arbitrary object value that can be used to store custom information about this element.  </summary>
	/// <returns>The intended value. This property has no default value.</returns>
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

	/// <summary>Gets or sets localization/globalization language information that applies to an individual element.</summary>
	/// <returns>The culture information for this element. The default value is an <see cref="T:System.Windows.Markup.XmlLanguage" /> instance with its <see cref="P:System.Windows.Markup.XmlLanguage.IetfLanguageTag" /> value set to the string "en-US".</returns>
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

	/// <summary>Gets or sets an object that enables customization of appearance, effects, or other style characteristics that will apply to this element when it captures keyboard focus.</summary>
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

	/// <summary>Gets or sets a value indicating whether this <see cref="T:System.Windows.FrameworkContentElement" /> should force the user interface (UI) to render the cursor as declared by this instance's <see cref="P:System.Windows.FrameworkContentElement.Cursor" /> property.</summary>
	/// <returns>true to force cursor presentation while over this element to use this instance's setting for the cursor (including on all child elements); otherwise false. The default value is false.</returns>
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

	/// <summary>Gets or sets the context for input used by this <see cref="T:System.Windows.FrameworkContentElement" />.</summary>
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

	/// <summary>Gets or sets the data context for an element when it participates in data binding. </summary>
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

	/// <summary>Gets or sets the <see cref="T:System.Windows.Data.BindingGroup" /> that is used for the element. </summary>
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

	internal InheritanceBehavior InheritanceBehavior
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
					TreeWalkHelper.InvalidateOnTreeChange(null, this, _parent, isAddOperation: true);
				}
				return;
			}
			throw new InvalidOperationException(SR.Illegal_InheritanceBehaviorSettor);
		}
	}

	/// <summary>Gets a value indicating whether this element has been initialized, either by being loaded as Extensible Application Markup Language (XAML), or by explicitly having its <see cref="M:System.Windows.FrameworkContentElement.EndInit" /> method called. </summary>
	/// <returns>true if the element is initialized per the aforementioned loading or method calls; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public bool IsInitialized => ReadInternalFlag(InternalFlags.IsInitialized);

	/// <summary>Gets a value indicating whether this element has been loaded for presentation. </summary>
	/// <returns>true if the current element is attached to an element tree and has been rendered; false if the element has never been attached to a loaded element tree. </returns>
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

	/// <summary>Gets or sets the tool-tip object that is displayed for this element in the user interface (UI). </summary>
	/// <returns>The tooltip object. See Remarks below for details on why this parameter is not strongly typed.</returns>
	[Bindable(true)]
	[Category("Appearance")]
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

	/// <summary>Gets or sets the context menu element that should appear whenever the context menu is requested via user interface (UI) from within this element. </summary>
	/// <returns>The context menu that this element uses. </returns>
	public ContextMenu ContextMenu
	{
		get
		{
			return (ContextMenu)GetValue(ContextMenuProperty);
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

	/// <summary>Gets the parent in the logical tree for this element. </summary>
	/// <returns>The logical parent for this element.</returns>
	public new DependencyObject Parent => ContextVerifiedGetParent();

	/// <summary>Gets an enumerator for the logical child elements of this element. </summary>
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

	/// <summary> Occurs when any associated target property participating in a binding on this element changes. </summary>
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

	/// <summary>Occurs when any associated data source participating in a binding on this element changes. </summary>
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

	/// <summary> Occurs when this element's data context changes. </summary>
	public event DependencyPropertyChangedEventHandler DataContextChanged
	{
		add
		{
			EventHandlersStoreAdd(FrameworkElement.DataContextChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(FrameworkElement.DataContextChangedKey, value);
		}
	}

	/// <summary> Occurs when this <see cref="T:System.Windows.FrameworkContentElement" /> is initialized. This coincides with cases where the value of the <see cref="P:System.Windows.FrameworkContentElement.IsInitialized" /> property changes from false (or undefined) to true. </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public event EventHandler Initialized
	{
		add
		{
			EventHandlersStoreAdd(FrameworkElement.InitializedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(FrameworkElement.InitializedKey, value);
		}
	}

	/// <summary> Occurs when the element is laid out, rendered, and ready for interaction. </summary>
	public event RoutedEventHandler Loaded
	{
		add
		{
			AddHandler(FrameworkElement.LoadedEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(FrameworkElement.LoadedEvent, value);
		}
	}

	/// <summary>Occurs when the element is removed from an element tree of loaded elements. </summary>
	public event RoutedEventHandler Unloaded
	{
		add
		{
			AddHandler(FrameworkElement.UnloadedEvent, value, handledEventsToo: false);
		}
		remove
		{
			RemoveHandler(FrameworkElement.UnloadedEvent, value);
		}
	}

	/// <summary> Occurs when any tooltip on the element is opened. </summary>
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

	/// <summary> Occurs just before any tooltip on the element is closed. </summary>
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

	/// <summary> Occurs when any context menu on the element is opened. </summary>
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
			EventHandlersStoreAdd(FrameworkElement.ResourcesChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(FrameworkElement.ResourcesChangedKey, value);
		}
	}

	internal event InheritedPropertyChangedEventHandler InheritedPropertyChanged
	{
		add
		{
			PotentiallyHasMentees = true;
			EventHandlersStoreAdd(FrameworkElement.InheritedPropertyChangedKey, value);
		}
		remove
		{
			EventHandlersStoreRemove(FrameworkElement.InheritedPropertyChangedKey, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkContentElement" /> class. </summary>
	public FrameworkContentElement()
	{
		PropertyMetadata metadata = StyleProperty.GetMetadata(base.DependencyObjectType);
		Style style = (Style)metadata.DefaultValue;
		if (style != null)
		{
			OnStyleChanged(this, new DependencyPropertyChangedEventArgs(StyleProperty, metadata, null, style));
		}
		Application current = Application.Current;
		if (current != null && current.HasImplicitStylesInResources)
		{
			ShouldLookupImplicitStyles = true;
		}
	}

	static FrameworkContentElement()
	{
		DefaultNumberSubstitution = new NumberSubstitution(NumberCultureSource.Text, null, NumberSubstitutionMethod.AsCulture);
		StyleProperty = FrameworkElement.StyleProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnStyleChanged));
		OverridesDefaultStyleProperty = FrameworkElement.OverridesDefaultStyleProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsMeasure, OnThemeStyleKeyChanged));
		DefaultStyleKeyProperty = FrameworkElement.DefaultStyleKeyProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnThemeStyleKeyChanged));
		NameProperty = FrameworkElement.NameProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(string.Empty));
		TagProperty = FrameworkElement.TagProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata((object)null));
		LanguageProperty = FrameworkElement.LanguageProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("en-US"), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));
		FocusVisualStyleProperty = FrameworkElement.FocusVisualStyleProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(FrameworkElement.DefaultFocusVisualStyle));
		CursorProperty = FrameworkElement.CursorProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, OnCursorChanged));
		ForceCursorProperty = FrameworkElement.ForceCursorProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.None, OnForceCursorChanged));
		InputScopeProperty = InputMethod.InputScopeProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
		DataContextProperty = FrameworkElement.DataContextProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, OnDataContextChanged));
		BindingGroupProperty = FrameworkElement.BindingGroupProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
		LoadedPendingProperty = FrameworkElement.LoadedPendingProperty.AddOwner(typeof(FrameworkContentElement));
		UnloadedPendingProperty = FrameworkElement.UnloadedPendingProperty.AddOwner(typeof(FrameworkContentElement));
		LoadedEvent = FrameworkElement.LoadedEvent.AddOwner(typeof(FrameworkContentElement));
		UnloadedEvent = FrameworkElement.UnloadedEvent.AddOwner(typeof(FrameworkContentElement));
		ToolTipProperty = ToolTipService.ToolTipProperty.AddOwner(typeof(FrameworkContentElement));
		ContextMenuProperty = ContextMenuService.ContextMenuProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata((object)null));
		ToolTipOpeningEvent = ToolTipService.ToolTipOpeningEvent.AddOwner(typeof(FrameworkContentElement));
		ToolTipClosingEvent = ToolTipService.ToolTipClosingEvent.AddOwner(typeof(FrameworkContentElement));
		ContextMenuOpeningEvent = ContextMenuService.ContextMenuOpeningEvent.AddOwner(typeof(FrameworkContentElement));
		ContextMenuClosingEvent = ContextMenuService.ContextMenuClosingEvent.AddOwner(typeof(FrameworkContentElement));
		ResourcesField = FrameworkElement.ResourcesField;
		InheritanceContextField = new UncommonField<DependencyObject>();
		MentorField = new UncommonField<DependencyObject>();
		PropertyChangedCallback propertyChangedCallback = NumberSubstitutionChanged;
		NumberSubstitution.CultureSourceProperty.OverrideMetadata(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(NumberCultureSource.Text, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback));
		NumberSubstitution.CultureOverrideProperty.OverrideMetadata(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback));
		NumberSubstitution.SubstitutionProperty.OverrideMetadata(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(NumberSubstitutionMethod.AsCulture, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback));
		EventManager.RegisterClassHandler(typeof(FrameworkContentElement), Mouse.QueryCursorEvent, new QueryCursorEventHandler(OnQueryCursor), handledEventsToo: true);
		ContentElement.AllowDropProperty.OverrideMetadata(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));
		Stylus.IsPressAndHoldEnabledProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
		Stylus.IsFlicksEnabledProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
		Stylus.IsTapFeedbackEnabledProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
		Stylus.IsTouchFeedbackEnabledProperty.AddOwner(typeof(FrameworkContentElement), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
		EventManager.RegisterClassHandler(typeof(FrameworkContentElement), ToolTipOpeningEvent, new ToolTipEventHandler(OnToolTipOpeningThunk));
		EventManager.RegisterClassHandler(typeof(FrameworkContentElement), ToolTipClosingEvent, new ToolTipEventHandler(OnToolTipClosingThunk));
		EventManager.RegisterClassHandler(typeof(FrameworkContentElement), ContextMenuOpeningEvent, new ContextMenuEventHandler(OnContextMenuOpeningThunk));
		EventManager.RegisterClassHandler(typeof(FrameworkContentElement), ContextMenuClosingEvent, new ContextMenuEventHandler(OnContextMenuClosingThunk));
		EventManager.RegisterClassHandler(typeof(FrameworkContentElement), Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus));
		EventManager.RegisterClassHandler(typeof(FrameworkContentElement), Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostKeyboardFocus));
	}

	private static void NumberSubstitutionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		((FrameworkContentElement)o).HasNumberSubstitutionChanged = true;
	}

	/// <summary>Returns whether serialization processes should serialize the contents of the <see cref="P:System.Windows.FrameworkContentElement.Style" /> property on instances of this class.</summary>
	/// <returns>true if the <see cref="P:System.Windows.FrameworkContentElement.Style" /> property value should be serialized; otherwise, false.</returns>
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
		FrameworkContentElement frameworkContentElement = (FrameworkContentElement)d;
		frameworkContentElement.HasLocalStyle = e.NewEntry.BaseValueSourceInternal == BaseValueSourceInternal.Local;
		StyleHelper.UpdateStyleCache(null, frameworkContentElement, (Style)e.OldValue, (Style)e.NewValue, ref frameworkContentElement._styleCache);
	}

	/// <summary>Invoked when the style that is in use on this element changes. </summary>
	/// <param name="oldStyle">The old style.</param>
	/// <param name="newStyle">The new style.</param>
	protected internal virtual void OnStyleChanged(Style oldStyle, Style newStyle)
	{
		HasStyleChanged = true;
	}

	private static void OnThemeStyleKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((FrameworkContentElement)d).UpdateThemeStyleProperty();
	}

	internal static void OnThemeStyleChanged(DependencyObject d, object oldValue, object newValue)
	{
		FrameworkContentElement frameworkContentElement = (FrameworkContentElement)d;
		StyleHelper.UpdateThemeStyleCache(null, frameworkContentElement, (Style)oldValue, (Style)newValue, ref frameworkContentElement._themeStyleCache);
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

	/// <summary>Returns whether serialization processes should serialize the contents of the <see cref="P:System.Windows.FrameworkContentElement.Resources" /> property on instances of this class.</summary>
	/// <returns>true if the <see cref="P:System.Windows.FrameworkContentElement.Resources" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeResources()
	{
		if (Resources == null || Resources.Count == 0)
		{
			return false;
		}
		return true;
	}

	/// <summary> Searches for a resource with the specified key, and will throw an exception if the requested resource is not found. </summary>
	/// <returns>The found resource, or null if no matching resource was found (but will also throw an exception if null).</returns>
	/// <param name="resourceKey">Key identifier of the resource to be found.</param>
	/// <exception cref="T:System.Windows.ResourceReferenceKeyNotFoundException">The requested resource key was not found.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="resourceKey" /> is null.</exception>
	public object FindResource(object resourceKey)
	{
		if (resourceKey == null)
		{
			throw new ArgumentNullException("resourceKey");
		}
		object obj = FrameworkElement.FindResourceInternal(null, this, resourceKey);
		if (obj == DependencyProperty.UnsetValue)
		{
			Helper.ResourceFailureThrow(resourceKey);
		}
		return obj;
	}

	/// <summary>Searches for a resource with the specified key, and returns that resource if found. </summary>
	/// <returns>The found resource. If no resource was found, null is returned.</returns>
	/// <param name="resourceKey">Key identifier of the resource to be found.</param>
	public object TryFindResource(object resourceKey)
	{
		if (resourceKey == null)
		{
			throw new ArgumentNullException("resourceKey");
		}
		object obj = FrameworkElement.FindResourceInternal(null, this, resourceKey);
		if (obj == DependencyProperty.UnsetValue)
		{
			obj = null;
		}
		return obj;
	}

	/// <summary>Searches for a resource with the specified name and sets up a resource reference to it for the specified property. </summary>
	/// <param name="dp">The property to which the resource is bound.</param>
	/// <param name="name">The name of the resource.</param>
	public void SetResourceReference(DependencyProperty dp, object name)
	{
		SetValue(dp, new ResourceReferenceExpression(name));
		HasResourceReference = true;
	}

	/// <summary>Begins the sequence of actions that are contained in the provided storyboard. </summary>
	/// <param name="storyboard">The storyboard to begin.</param>
	public void BeginStoryboard(Storyboard storyboard)
	{
		BeginStoryboard(storyboard, HandoffBehavior.SnapshotAndReplace, isControllable: false);
	}

	/// <summary> Begins the sequence of actions that are contained in the provided storyboard, with options specified for what should occur if the property is already animated. </summary>
	/// <param name="storyboard">The storyboard to begin.</param>
	/// <param name="handoffBehavior">A value of the enumeration that describes behavior to use if a property described in the storyboard is already animated.</param>
	public void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior)
	{
		BeginStoryboard(storyboard, handoffBehavior, isControllable: false);
	}

	/// <summary> Begins the sequence of actions that are contained in the provided storyboard, with specified state for control of the animation after it is started. </summary>
	/// <param name="storyboard">The storyboard to begin. </param>
	/// <param name="handoffBehavior">A value of the enumeration that describes behavior to use if a  property described in the storyboard is already animated.</param>
	/// <param name="isControllable">Declares whether the animation is controllable (can be paused) after it is started.</param>
	public void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior, bool isControllable)
	{
		if (storyboard == null)
		{
			throw new ArgumentNullException("storyboard");
		}
		storyboard.Begin(this, handoffBehavior, isControllable);
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
			if (StyleHelper.GetValueFromStyleOrTemplate(new FrameworkObject(null, this), dp, ref entry))
			{
				return;
			}
		}
		else
		{
			object source;
			object obj = FrameworkElement.FindImplicitStyleResource(this, GetType(), out source);
			if (obj != DependencyProperty.UnsetValue)
			{
				HasImplicitStyleFromResources = true;
				entry.BaseValueSourceInternal = BaseValueSourceInternal.ImplicitReference;
				entry.Value = obj;
				return;
			}
		}
		if (!(metadata is FrameworkPropertyMetadata { Inherits: not false } frameworkPropertyMetadata) || (TreeWalkHelper.SkipNext(InheritanceBehavior) && !frameworkPropertyMetadata.OverridesInheritanceBehavior))
		{
			return;
		}
		FrameworkElement feParent;
		FrameworkContentElement fceParent;
		bool flag = FrameworkElement.GetFrameworkParent(this, out feParent, out fceParent);
		while (flag)
		{
			InheritanceBehavior inheritanceBehavior;
			bool flag2 = ((feParent == null) ? TreeWalkHelper.IsInheritanceNode(fceParent, dp, out inheritanceBehavior) : TreeWalkHelper.IsInheritanceNode(feParent, dp, out inheritanceBehavior));
			if (TreeWalkHelper.SkipNow(inheritanceBehavior))
			{
				break;
			}
			if (flag2)
			{
				if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose))
				{
					string text = "[" + GetType().Name + "]" + dp.Name;
					EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientPropParentCheck, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose, GetHashCode(), text);
				}
				DependencyObject dependencyObject = feParent;
				if (dependencyObject == null)
				{
					dependencyObject = fceParent;
				}
				EntryIndex entryIndex = dependencyObject.LookupEntry(dp.GlobalIndex);
				entry = dependencyObject.GetValueEntry(entryIndex, dp, frameworkPropertyMetadata, (RequestFlags)12);
				if (entry.Value != DependencyProperty.UnsetValue)
				{
					entry.BaseValueSourceInternal = BaseValueSourceInternal.Inherited;
				}
				break;
			}
			if (TreeWalkHelper.SkipNext(inheritanceBehavior))
			{
				break;
			}
			flag = ((feParent == null) ? FrameworkElement.GetFrameworkParent(fceParent, out feParent, out fceParent) : FrameworkElement.GetFrameworkParent(feParent, out feParent, out fceParent));
		}
	}

	private bool GetValueFromTemplatedParent(DependencyProperty dp, ref EffectiveValueEntry entry)
	{
		FrameworkTemplate frameworkTemplate = null;
		frameworkTemplate = ((FrameworkElement)_templatedParent).TemplateInternal;
		if (frameworkTemplate != null)
		{
			return StyleHelper.GetValueFromTemplatedParent(_templatedParent, TemplateChildIndex, new FrameworkObject(null, this), dp, ref frameworkTemplate.ChildRecordFromChildIndex, frameworkTemplate.VisualTree, ref entry);
		}
		return false;
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

	/// <summary>Invoked whenever the effective value of any dependency property on this <see cref="T:System.Windows.FrameworkContentElement" /> has been updated. The specific dependency property that changed is reported in the arguments parameter. Overrides <see cref="M:System.Windows.DependencyObject.OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs)" />.</summary>
	/// <param name="e">The event data that describes the property that changed, including the old and new values.</param>
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
			if (property == FrameworkElement.NameProperty && EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose))
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.PerfElementIDName, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose, PerfService.GetPerfElementID(this), GetType().Name, GetValue(property));
			}
			if (property != StyleProperty && property != DefaultStyleKeyProperty)
			{
				if (TemplatedParent != null)
				{
					FrameworkTemplate templateInternal = (TemplatedParent as FrameworkElement).TemplateInternal;
					StyleHelper.OnTriggerSourcePropertyInvalidated(null, templateInternal, TemplatedParent, property, e, invalidateOnlyContainer: false, ref templateInternal.TriggerSourceRecordFromChildIndex, ref templateInternal.PropertyTriggersWithActions, TemplateChildIndex);
				}
				if (Style != null)
				{
					StyleHelper.OnTriggerSourcePropertyInvalidated(Style, null, this, property, e, invalidateOnlyContainer: true, ref Style.TriggerSourceRecordFromChildIndex, ref Style.PropertyTriggersWithActions, 0);
				}
				if (ThemeStyle != null && Style != ThemeStyle)
				{
					StyleHelper.OnTriggerSourcePropertyInvalidated(ThemeStyle, null, this, property, e, invalidateOnlyContainer: true, ref ThemeStyle.TriggerSourceRecordFromChildIndex, ref ThemeStyle.PropertyTriggersWithActions, 0);
				}
			}
		}
		if (e.Metadata is FrameworkPropertyMetadata { Inherits: not false } frameworkPropertyMetadata && (InheritanceBehavior == InheritanceBehavior.Default || frameworkPropertyMetadata.OverridesInheritanceBehavior) && (!DependencyObject.IsTreeWalkOperation(e.OperationType) || PotentiallyHasMentees))
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
				TreeWalkHelper.InvalidateOnInheritablePropertyChange(null, this, info, skipStartNode: true);
			}
			if (PotentiallyHasMentees)
			{
				TreeWalkHelper.OnInheritedPropertyChanged(this, ref info, InheritanceBehavior);
			}
		}
	}

	private static void OnCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (((FrameworkContentElement)d).IsMouseOver)
		{
			Mouse.UpdateCursor();
		}
	}

	private static void OnForceCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (((FrameworkContentElement)d).IsMouseOver)
		{
			Mouse.UpdateCursor();
		}
	}

	private static void OnQueryCursor(object sender, QueryCursorEventArgs e)
	{
		FrameworkContentElement frameworkContentElement = (FrameworkContentElement)sender;
		Cursor cursor = frameworkContentElement.Cursor;
		if (cursor != null && (!e.Handled || frameworkContentElement.ForceCursor))
		{
			e.Cursor = cursor;
			e.Handled = true;
		}
	}

	/// <summary> Moves the keyboard focus from this element to another element. </summary>
	/// <returns>Returns true if focus is moved successfully; false if the target element in direction as specified does not exist.</returns>
	/// <param name="request">The direction that focus is to be moved, as a value of the enumeration.</param>
	public sealed override bool MoveFocus(TraversalRequest request)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		return KeyboardNavigation.Current.Navigate(this, request);
	}

	/// <summary>Determines the next element that would receive focus relative to this element for a provided focus movement direction, but does not actually move the focus. This method is sealed and cannot be overridden.</summary>
	/// <returns>The next element that focus would move to if focus were actually traversed. May return null if focus cannot be moved relative to this element for the provided direction.</returns>
	/// <param name="direction">The direction for which a prospective focus change should be determined.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">Specified one of the following directions in the <see cref="T:System.Windows.Input.TraversalRequest" />: <see cref="F:System.Windows.Input.FocusNavigationDirection.Next" />, <see cref="F:System.Windows.Input.FocusNavigationDirection.Previous" />, <see cref="F:System.Windows.Input.FocusNavigationDirection.First" />, <see cref="F:System.Windows.Input.FocusNavigationDirection.Last" />. These directions are not legal for <see cref="M:System.Windows.FrameworkContentElement.PredictFocus(System.Windows.Input.FocusNavigationDirection)" /> (but they are legal for <see cref="M:System.Windows.FrameworkContentElement.MoveFocus(System.Windows.Input.TraversalRequest)" />). </exception>
	public sealed override DependencyObject PredictFocus(FocusNavigationDirection direction)
	{
		return KeyboardNavigation.Current.PredictFocusedElement(this, direction);
	}

	/// <summary>Class handler for the <see cref="E:System.Windows.ContentElement.GotFocus" /> event.</summary>
	/// <param name="e">Event data for the event.</param>
	protected override void OnGotFocus(RoutedEventArgs e)
	{
		if (base.IsKeyboardFocused)
		{
			BringIntoView();
		}
		base.OnGotFocus(e);
	}

	private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
	{
		if (sender == e.OriginalSource)
		{
			FrameworkContentElement frameworkContentElement = (FrameworkContentElement)sender;
			KeyboardNavigation.UpdateFocusedElement(frameworkContentElement);
			KeyboardNavigation current = KeyboardNavigation.Current;
			KeyboardNavigation.ShowFocusVisual();
			current.UpdateActiveElement(frameworkContentElement);
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

	/// <summary>Attempts to bring this element into view, within any scrollable regions it is contained within. </summary>
	public void BringIntoView()
	{
		RequestBringIntoViewEventArgs requestBringIntoViewEventArgs = new RequestBringIntoViewEventArgs(this, Rect.Empty);
		requestBringIntoViewEventArgs.RoutedEvent = FrameworkElement.RequestBringIntoViewEvent;
		RaiseEvent(requestBringIntoViewEventArgs);
	}

	private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue != BindingExpressionBase.DisconnectedItem)
		{
			((FrameworkContentElement)d).RaiseDependencyPropertyChanged(FrameworkElement.DataContextChangedKey, e);
		}
	}

	/// <summary> Gets the <see cref="T:System.Windows.Data.BindingExpression" /> for the specified property's binding. </summary>
	/// <returns>Returns a <see cref="T:System.Windows.Data.BindingExpression" /> if the target is data bound; otherwise, null.</returns>
	/// <param name="dp">The target <see cref="T:System.Windows.DependencyProperty" /> from which to get the binding.</param>
	public BindingExpression GetBindingExpression(DependencyProperty dp)
	{
		return BindingOperations.GetBindingExpression(this, dp);
	}

	/// <summary>Attaches a binding to this element, based on the provided binding object. </summary>
	/// <returns>Records the conditions of the binding. This return value can be useful for error checking.</returns>
	/// <param name="dp">Identifies the bound property.</param>
	/// <param name="binding">Represents a data binding.</param>
	public BindingExpressionBase SetBinding(DependencyProperty dp, BindingBase binding)
	{
		return BindingOperations.SetBinding(this, dp, binding);
	}

	/// <summary>Attaches a binding to this element, based on the provided source property name as a path qualification to the data source. </summary>
	/// <returns>Records the conditions of the binding. This return value can be useful for error checking.</returns>
	/// <param name="dp">Identifies the bound property.</param>
	/// <param name="path">The source property name or the path to the property used for the binding.</param>
	public BindingExpression SetBinding(DependencyProperty dp, string path)
	{
		return (BindingExpression)SetBinding(dp, new Binding(path));
	}

	/// <summary>Returns an alternative logical parent for this element if there is no visual parent. In this case, a <see cref="T:System.Windows.FrameworkContentElement" />  parent is always the same value as the <see cref="P:System.Windows.FrameworkContentElement.Parent" /> property.</summary>
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

	internal virtual bool IgnoreModelParentBuildRoute(RoutedEventArgs args)
	{
		return false;
	}

	internal sealed override bool BuildRouteCore(EventRoute route, RoutedEventArgs args)
	{
		bool result = false;
		DependencyObject parent = ContentOperations.GetParent(this);
		DependencyObject parent2 = _parent;
		if (route.PeekBranchNode() is DependencyObject dependencyObject && IsLogicalDescendent(dependencyObject))
		{
			args.Source = route.PeekBranchSource();
			AdjustBranchSource(args);
			route.AddSource(args.Source);
			route.PopBranchNode();
			FrameworkElement.AddIntermediateElementsToRoute(this, route, args, LogicalTreeHelper.GetParent(dependencyObject));
		}
		if (!IgnoreModelParentBuildRoute(args))
		{
			if (parent == null)
			{
				result = parent2 != null;
			}
			else if (parent2 != null)
			{
				route.PushBranchNode(this, args.Source);
				args.Source = parent;
			}
		}
		return result;
	}

	internal override void AddToEventRouteCore(EventRoute route, RoutedEventArgs args)
	{
		FrameworkElement.AddStyleHandlersToEventRoute(null, this, route, args);
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

	internal override bool InvalidateAutomationAncestorsCore(Stack<DependencyObject> branchNodeStack, out bool continuePastCoreTree)
	{
		bool result = true;
		continuePastCoreTree = false;
		DependencyObject parent = ContentOperations.GetParent(this);
		DependencyObject parent2 = _parent;
		DependencyObject dependencyObject = ((branchNodeStack.Count > 0) ? branchNodeStack.Peek() : null);
		if (dependencyObject != null && IsLogicalDescendent(dependencyObject))
		{
			branchNodeStack.Pop();
			result = FrameworkElement.InvalidateAutomationIntermediateElements(this, LogicalTreeHelper.GetParent(dependencyObject));
		}
		if (parent == null)
		{
			continuePastCoreTree = parent2 != null;
		}
		else if (parent2 != null)
		{
			branchNodeStack.Push(this);
		}
		return result;
	}

	internal virtual void OnAncestorChanged()
	{
	}

	internal override void OnContentParentChanged(DependencyObject oldParent)
	{
		ContentOperations.GetParent(this);
		TryFireInitialized();
		base.OnContentParentChanged(oldParent);
	}

	/// <summary>Called before an element is initialized. </summary>
	public virtual void BeginInit()
	{
		if (ReadInternalFlag(InternalFlags.InitPending))
		{
			throw new InvalidOperationException(SR.NestedBeginInitNotSupported);
		}
		WriteInternalFlag(InternalFlags.InitPending, set: true);
	}

	/// <summary> Called immediately after an element is initialized. </summary>
	public virtual void EndInit()
	{
		if (!ReadInternalFlag(InternalFlags.InitPending))
		{
			throw new InvalidOperationException(SR.EndInitWithoutBeginInitNotSupported);
		}
		WriteInternalFlag(InternalFlags.InitPending, set: false);
		TryFireInitialized();
	}

	/// <summary> Raises the <see cref="E:System.Windows.FrameworkContentElement.Initialized" /> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkContentElement.IsInitialized" /> is set to true. </summary>
	/// <param name="e">Event data for the event.</param>
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
		RaiseInitialized(FrameworkElement.InitializedKey, e);
	}

	private void TryFireInitialized()
	{
		if (!ReadInternalFlag(InternalFlags.InitPending) && !ReadInternalFlag(InternalFlags.IsInitialized))
		{
			WriteInternalFlag(InternalFlags.IsInitialized, set: true);
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

	internal void OnLoaded(RoutedEventArgs args)
	{
		RaiseEvent(args);
	}

	internal void OnUnloaded(RoutedEventArgs args)
	{
		RaiseEvent(args);
	}

	internal override void OnAddHandler(RoutedEvent routedEvent, Delegate handler)
	{
		if (routedEvent == LoadedEvent || routedEvent == UnloadedEvent)
		{
			BroadcastEventHelper.AddHasLoadedChangeHandlerFlagInAncestry(this);
		}
	}

	internal override void OnRemoveHandler(RoutedEvent routedEvent, Delegate handler)
	{
		if ((routedEvent == LoadedEvent || routedEvent == UnloadedEvent) && !ThisHasLoadedChangeEventHandler)
		{
			BroadcastEventHelper.RemoveHasLoadedChangeHandlerFlagInAncestry(this);
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

	private static void OnToolTipOpeningThunk(object sender, ToolTipEventArgs e)
	{
		((FrameworkContentElement)sender).OnToolTipOpening(e);
	}

	/// <summary> Invoked whenever the <see cref="E:System.Windows.FrameworkContentElement.ToolTipOpening" /> routed event reaches this class in its route. Implement this method to add class handling for this event. </summary>
	/// <param name="e">Provides data about the event.</param>
	protected virtual void OnToolTipOpening(ToolTipEventArgs e)
	{
	}

	private static void OnToolTipClosingThunk(object sender, ToolTipEventArgs e)
	{
		((FrameworkContentElement)sender).OnToolTipClosing(e);
	}

	/// <summary> Invoked whenever the <see cref="E:System.Windows.FrameworkContentElement.ToolTipClosing" /> routed event reaches this class in its route. Implement this method to add class handling for this event. </summary>
	/// <param name="e">Provides data about the event.</param>
	protected virtual void OnToolTipClosing(ToolTipEventArgs e)
	{
	}

	private static void OnContextMenuOpeningThunk(object sender, ContextMenuEventArgs e)
	{
		((FrameworkContentElement)sender).OnContextMenuOpening(e);
	}

	/// <summary> Invoked whenever the <see cref="E:System.Windows.FrameworkContentElement.ContextMenuOpening" /> routed event reaches this class in its route. Implement this method to add class handling for this event. </summary>
	/// <param name="e">Event data for the event.</param>
	protected virtual void OnContextMenuOpening(ContextMenuEventArgs e)
	{
	}

	private static void OnContextMenuClosingThunk(object sender, ContextMenuEventArgs e)
	{
		((FrameworkContentElement)sender).OnContextMenuClosing(e);
	}

	/// <summary> Invoked whenever the <see cref="E:System.Windows.FrameworkContentElement.ContextMenuClosing" /> routed event reaches this class in its route. Implement this method to add class handling for this event. </summary>
	/// <param name="e">Provides data about the event.</param>
	protected virtual void OnContextMenuClosing(ContextMenuEventArgs e)
	{
	}

	internal override void InvalidateForceInheritPropertyOnChildren(DependencyProperty property)
	{
		IEnumerator logicalChildren = LogicalChildren;
		if (logicalChildren == null)
		{
			return;
		}
		while (logicalChildren.MoveNext())
		{
			if (logicalChildren.Current is DependencyObject dependencyObject)
			{
				dependencyObject.CoerceValue(property);
			}
		}
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

	private void EventHandlersStoreAdd(EventPrivateKey key, Delegate handler)
	{
		EnsureEventHandlersStore();
		base.EventHandlersStore.Add(key, handler);
	}

	private void EventHandlersStoreRemove(EventPrivateKey key, Delegate handler)
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
		INameScope nameScope = FrameworkElement.FindScope(this);
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
		INameScope nameScope = FrameworkElement.FindScope(this);
		if (nameScope != null)
		{
			nameScope.UnregisterName(name);
			return;
		}
		throw new InvalidOperationException(SR.Format(SR.NameScopeNotFound, name, "unregister"));
	}

	/// <summary> Finds an element that has the provided identifier name. </summary>
	/// <returns>The requested element. May be null if no matching element was found.</returns>
	/// <param name="name">Name of the element to search for.</param>
	public object FindName(string name)
	{
		DependencyObject scopeOwner;
		return FindName(name, out scopeOwner);
	}

	internal object FindName(string name, out DependencyObject scopeOwner)
	{
		return FrameworkElement.FindScope(this, out scopeOwner)?.FindName(name);
	}

	/// <summary>Reapplies the default style to the current <see cref="T:System.Windows.FrameworkContentElement" />.</summary>
	public void UpdateDefaultStyle()
	{
		TreeWalkHelper.InvalidateOnResourcesChange(null, this, ResourcesChangeInfo.ThemeChangeInfo);
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

	/// <summary>Adds the provided element as a child of this element. </summary>
	/// <param name="child">The child element to be added.</param>
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

	/// <summary>Removes the specified element from the logical tree for this element. </summary>
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
		BroadcastEventHelper.BroadcastLoadedOrUnloadedEvent(this, parent, newParent);
		DependencyObject parent2 = ((newParent != null) ? newParent : parent);
		TreeWalkHelper.InvalidateOnTreeChange(null, this, parent2, newParent != null);
		TryFireInitialized();
	}

	internal virtual void OnNewParent(DependencyObject newParent)
	{
		DependencyObject parent = _parent;
		_parent = newParent;
		if (_parent != null)
		{
			UIElement.SynchronizeForceInheritProperties(null, this, null, _parent);
		}
		else
		{
			UIElement.SynchronizeForceInheritProperties(null, this, null, parent);
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
		}
		if (parentTreeState.IsAddOperation)
		{
			new FrameworkObject(null, this).SetShouldLookupImplicitStyles();
		}
		if (HasResourceReference)
		{
			TreeWalkHelper.OnResourcesChanged(this, ResourcesChangeInfo.TreeChangeInfo, raiseResourceChangedEvent: false);
		}
		FrugalObjectList<DependencyProperty> item = InvalidateTreeDependentProperties(parentTreeState, base.IsSelfInheritanceParent, isSelfInheritanceParent);
		parentTreeState.InheritablePropertiesStack.Push(item);
		PresentationSource.OnAncestorChanged(this);
		OnAncestorChanged();
		if (PotentiallyHasMentees)
		{
			RaiseClrEvent(FrameworkElement.ResourcesChangedKey, EventArgs.Empty);
		}
	}

	internal FrugalObjectList<DependencyProperty> InvalidateTreeDependentProperties(TreeChangeInfo parentTreeState, bool isSelfInheritanceParent, bool wasSelfInheritanceParent)
	{
		AncestorChangeInProgress = true;
		try
		{
			if (!HasLocalStyle && this != parentTreeState.Root)
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
			FrameworkElement.GetTemplatedParentChildRecord(templatedParent, num, out childRecord, out isChildRecordValid);
			FrameworkElement feParent;
			FrameworkContentElement fceParent;
			bool frameworkParent = FrameworkElement.GetFrameworkParent(this, out feParent, out fceParent);
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
			return TreeWalkHelper.InvalidateTreeDependentProperties(parentTreeState, null, this, style, style2, ref childRecord, isChildRecordValid, hasStyleChanged, isSelfInheritanceParent, wasSelfInheritanceParent);
		}
		finally
		{
			AncestorChangeInProgress = false;
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
				StyleHelper.GetThemeStyle(null, this);
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
		if ((property == VisualBrush.VisualProperty || property == BitmapCacheBrush.TargetProperty) && FrameworkElement.GetFrameworkParent(this) == null && !FrameworkObject.IsEffectiveAncestor(this, context))
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
		TreeWalkHelper.InvalidateOnTreeChange(null, this, foMentor.DO, isAddOperation: true);
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
		TreeWalkHelper.InvalidateOnTreeChange(null, this, foMentor.DO, isAddOperation: false);
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
		TreeWalkHelper.InvalidateOnInheritablePropertyChange(null, this, e.Info, skipStartNode: false);
	}

	private void OnMentorResourcesChanged(object sender, EventArgs e)
	{
		TreeWalkHelper.InvalidateOnResourcesChange(null, this, ResourcesChangeInfo.CatastrophicDictionaryChangeInfo);
	}

	internal void RaiseInheritedPropertyChangedEvent(ref InheritablePropertyChangeInfo info)
	{
		EventHandlersStore eventHandlersStore = base.EventHandlersStore;
		if (eventHandlersStore != null)
		{
			Delegate @delegate = eventHandlersStore.Get(FrameworkElement.InheritedPropertyChangedKey);
			if ((object)@delegate != null)
			{
				InheritedPropertyChangedEventArgs e = new InheritedPropertyChangedEventArgs(ref info);
				((InheritedPropertyChangedEventHandler)@delegate)(this, e);
			}
		}
	}
}
