using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;
using System.Windows.Baml2006;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Media3D;
using MS.Internal;
using MS.Utility;

namespace System.Windows;

/// <summary>Supports the creation of templates.</summary>
[Localizability(LocalizationCategory.NeverLocalize)]
public class FrameworkElementFactory
{
	private bool _sealed;

	internal FrugalStructList<PropertyValue> PropertyValues;

	private EventHandlersStore _eventHandlersStore;

	internal bool _hasLoadedChangeHandler;

	private Type _type;

	private string _text;

	private Func<object> _knownTypeFactory;

	private string _childName;

	internal int _childIndex = -1;

	private FrameworkTemplate _frameworkTemplate;

	private static int AutoGenChildNamePostfix = 1;

	private static string AutoGenChildNamePrefix = "~ChildID";

	private FrameworkElementFactory _parent;

	private FrameworkElementFactory _firstChild;

	private FrameworkElementFactory _lastChild;

	private FrameworkElementFactory _nextSibling;

	private readonly object _synchronized = new object();

	/// <summary>Gets or sets the type of the objects this factory produces.</summary>
	/// <returns>The type of the objects this factory produces.</returns>
	public Type Type
	{
		get
		{
			return _type;
		}
		set
		{
			if (_sealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "FrameworkElementFactory"));
			}
			if (_text != null)
			{
				throw new InvalidOperationException(SR.FrameworkElementFactoryCannotAddText);
			}
			if (value != null && !typeof(FrameworkElement).IsAssignableFrom(value) && !typeof(FrameworkContentElement).IsAssignableFrom(value) && !typeof(Visual3D).IsAssignableFrom(value))
			{
				throw new ArgumentException(SR.Format(SR.MustBeFrameworkOr3DDerived, value.Name));
			}
			_type = value;
			WpfKnownType wpfKnownType = null;
			if (_type != null)
			{
				wpfKnownType = XamlReader.BamlSharedSchemaContext.GetKnownXamlType(_type) as WpfKnownType;
			}
			_knownTypeFactory = ((wpfKnownType != null) ? wpfKnownType.DefaultConstructor : null);
		}
	}

	/// <summary>Gets or sets the text string to produce.</summary>
	/// <returns>The text string to produce.</returns>
	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			if (_sealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "FrameworkElementFactory"));
			}
			if (_firstChild != null)
			{
				throw new InvalidOperationException(SR.FrameworkElementFactoryCannotAddText);
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_text = value;
		}
	}

	/// <summary>Gets or sets the name of a template item.</summary>
	/// <returns>A string that is the template identifier.</returns>
	public string Name
	{
		get
		{
			return _childName;
		}
		set
		{
			if (_sealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "FrameworkElementFactory"));
			}
			if (value == string.Empty)
			{
				throw new ArgumentException(SR.NameNotEmptyString);
			}
			_childName = value;
		}
	}

	internal EventHandlersStore EventHandlersStore
	{
		get
		{
			return _eventHandlersStore;
		}
		set
		{
			_eventHandlersStore = value;
		}
	}

	internal bool HasLoadedChangeHandler
	{
		get
		{
			return _hasLoadedChangeHandler;
		}
		set
		{
			_hasLoadedChangeHandler = value;
		}
	}

	/// <summary>Gets a value that indicates whether this object is in an immutable state.</summary>
	/// <returns>true if this object is in an immutable state; otherwise, false.</returns>
	public bool IsSealed => _sealed;

	/// <summary>Gets the parent <see cref="T:System.Windows.FrameworkElementFactory" />.</summary>
	/// <returns>A <see cref="T:System.Windows.FrameworkElementFactory" /> that is the parent factory.</returns>
	public FrameworkElementFactory Parent => _parent;

	/// <summary>Gets the first child factory.</summary>
	/// <returns>A <see cref="T:System.Windows.FrameworkElementFactory" /> the first child factory.</returns>
	public FrameworkElementFactory FirstChild => _firstChild;

	/// <summary>Gets the next sibling factory.</summary>
	/// <returns>A <see cref="T:System.Windows.FrameworkElementFactory" /> that is the next sibling factory.</returns>
	public FrameworkElementFactory NextSibling => _nextSibling;

	internal FrameworkTemplate FrameworkTemplate => _frameworkTemplate;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkElementFactory" /> class.</summary>
	public FrameworkElementFactory()
		: this(null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkElementFactory" /> class with the specified <see cref="T:System.Type" />.</summary>
	/// <param name="type">The type of instance to create.</param>
	public FrameworkElementFactory(Type type)
		: this(type, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkElementFactory" /> class with the specified text to produce.</summary>
	/// <param name="text">The text string to produce.</param>
	public FrameworkElementFactory(string text)
		: this(null, null)
	{
		Text = text;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkElementFactory" /> class with the specified <see cref="T:System.Type" /> and name.</summary>
	/// <param name="type">The type of instance to create.</param>
	/// <param name="name">The style identifier.</param>
	public FrameworkElementFactory(Type type, string name)
	{
		Type = type;
		Name = name;
	}

	/// <summary>Adds a child factory to this factory.</summary>
	/// <param name="child">The <see cref="T:System.Windows.FrameworkElementFactory" /> object to add as a child.</param>
	public void AppendChild(FrameworkElementFactory child)
	{
		if (_sealed)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "FrameworkElementFactory"));
		}
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		if (child._parent != null)
		{
			throw new ArgumentException(SR.FrameworkElementFactoryAlreadyParented);
		}
		if (_text != null)
		{
			throw new InvalidOperationException(SR.FrameworkElementFactoryCannotAddText);
		}
		if (_firstChild == null)
		{
			_firstChild = child;
			_lastChild = child;
		}
		else
		{
			_lastChild._nextSibling = child;
			_lastChild = child;
		}
		child._parent = this;
	}

	/// <summary>Sets the value of a dependency property.</summary>
	/// <param name="dp">The dependency property identifier of the property to set.</param>
	/// <param name="value">The new value.</param>
	public void SetValue(DependencyProperty dp, object value)
	{
		if (_sealed)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "FrameworkElementFactory"));
		}
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		if (!dp.IsValidValue(value) && !(value is MarkupExtension) && !(value is DeferredReference))
		{
			throw new ArgumentException(SR.Format(SR.InvalidPropertyValue, value, dp.Name));
		}
		if (StyleHelper.IsStylingLogicalTree(dp, value))
		{
			throw new NotSupportedException(SR.Format(SR.ModifyingLogicalTreeViaStylesNotImplemented, value, "FrameworkElementFactory.SetValue"));
		}
		if (dp.ReadOnly)
		{
			throw new ArgumentException(SR.Format(SR.ReadOnlyPropertyNotAllowed, dp.Name, GetType().Name));
		}
		ResourceReferenceExpression resourceReferenceExpression = value as ResourceReferenceExpression;
		DynamicResourceExtension dynamicResourceExtension = value as DynamicResourceExtension;
		object obj = null;
		if (resourceReferenceExpression != null)
		{
			obj = resourceReferenceExpression.ResourceKey;
		}
		else if (dynamicResourceExtension != null)
		{
			obj = dynamicResourceExtension.ResourceKey;
		}
		if (obj == null)
		{
			if (!(value is TemplateBindingExtension value2))
			{
				UpdatePropertyValueList(dp, PropertyValueType.Set, value);
			}
			else
			{
				UpdatePropertyValueList(dp, PropertyValueType.TemplateBinding, value2);
			}
		}
		else
		{
			UpdatePropertyValueList(dp, PropertyValueType.Resource, obj);
		}
	}

	/// <summary>Sets up data binding on a property.</summary>
	/// <param name="dp">Identifies the property where the binding should be established.</param>
	/// <param name="binding">Description of the binding.</param>
	public void SetBinding(DependencyProperty dp, BindingBase binding)
	{
		SetValue(dp, binding);
	}

	/// <summary>Set up a dynamic resource reference on a child property.</summary>
	/// <param name="dp">The property to which the resource is bound.</param>
	/// <param name="name">The name of the resource.</param>
	public void SetResourceReference(DependencyProperty dp, object name)
	{
		if (_sealed)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "FrameworkElementFactory"));
		}
		if (dp == null)
		{
			throw new ArgumentNullException("dp");
		}
		UpdatePropertyValueList(dp, PropertyValueType.Resource, name);
	}

	/// <summary>Adds an event handler for the given routed event to the instances created by this factory.</summary>
	/// <param name="routedEvent">Identifier object for the routed event being handled.</param>
	/// <param name="handler">A reference to the handler implementation.</param>
	public void AddHandler(RoutedEvent routedEvent, Delegate handler)
	{
		AddHandler(routedEvent, handler, handledEventsToo: false);
	}

	/// <summary>Adds an event handler for the given routed event to the instances created by this factory, with the option of having the provided handler be invoked even in cases of routed events that had already been marked as handled by another element along the route.</summary>
	/// <param name="routedEvent">Identifier object for the routed event being handled.</param>
	/// <param name="handler">A reference to the handler implementation.</param>
	/// <param name="handledEventsToo">Whether to invoke the handler in cases where the routed event has already been marked as handled in its arguments object. true to invoke the handler even when the routed event is marked handled; otherwise, false. The default is false. Asking to handle already-handled routed events is not common.</param>
	public void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
	{
		if (_sealed)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "FrameworkElementFactory"));
		}
		if (routedEvent == null)
		{
			throw new ArgumentNullException("routedEvent");
		}
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (handler.GetType() != routedEvent.HandlerType)
		{
			throw new ArgumentException(SR.HandlerTypeIllegal);
		}
		if (_eventHandlersStore == null)
		{
			_eventHandlersStore = new EventHandlersStore();
		}
		_eventHandlersStore.AddRoutedEventHandler(routedEvent, handler, handledEventsToo);
		if (routedEvent == FrameworkElement.LoadedEvent || routedEvent == FrameworkElement.UnloadedEvent)
		{
			HasLoadedChangeHandler = true;
		}
	}

	/// <summary>Removes an event handler from the given routed event. This applies to the instances created by this factory.</summary>
	/// <param name="routedEvent">Identifier object for the routed event.</param>
	/// <param name="handler">The handler to remove.</param>
	public void RemoveHandler(RoutedEvent routedEvent, Delegate handler)
	{
		if (_sealed)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "FrameworkElementFactory"));
		}
		if (routedEvent == null)
		{
			throw new ArgumentNullException("routedEvent");
		}
		if ((object)handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		if (handler.GetType() != routedEvent.HandlerType)
		{
			throw new ArgumentException(SR.HandlerTypeIllegal);
		}
		if (_eventHandlersStore != null)
		{
			_eventHandlersStore.RemoveRoutedEventHandler(routedEvent, handler);
			if ((routedEvent == FrameworkElement.LoadedEvent || routedEvent == FrameworkElement.UnloadedEvent) && !_eventHandlersStore.Contains(FrameworkElement.LoadedEvent) && !_eventHandlersStore.Contains(FrameworkElement.UnloadedEvent))
			{
				HasLoadedChangeHandler = false;
			}
		}
	}

	private void UpdatePropertyValueList(DependencyProperty dp, PropertyValueType valueType, object value)
	{
		int num = -1;
		for (int i = 0; i < PropertyValues.Count; i++)
		{
			if (PropertyValues[i].Property == dp)
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			lock (_synchronized)
			{
				PropertyValue value2 = PropertyValues[num];
				value2.ValueType = valueType;
				value2.ValueInternal = value;
				PropertyValues[num] = value2;
				return;
			}
		}
		PropertyValue value3 = default(PropertyValue);
		value3.ValueType = valueType;
		value3.ChildName = null;
		value3.Property = dp;
		value3.ValueInternal = value;
		lock (_synchronized)
		{
			PropertyValues.Add(value3);
		}
	}

	private DependencyObject CreateDependencyObject()
	{
		if (_knownTypeFactory != null)
		{
			return _knownTypeFactory() as DependencyObject;
		}
		return (DependencyObject)Activator.CreateInstance(_type);
	}

	internal object GetValue(DependencyProperty dp)
	{
		for (int i = 0; i < PropertyValues.Count; i++)
		{
			PropertyValue propertyValue = PropertyValues[i];
			if (propertyValue.ValueType == PropertyValueType.Set && propertyValue.Property == dp)
			{
				return propertyValue.ValueInternal;
			}
		}
		return DependencyProperty.UnsetValue;
	}

	internal void Seal(FrameworkTemplate ownerTemplate)
	{
		if (!_sealed)
		{
			_frameworkTemplate = ownerTemplate;
			Seal();
		}
	}

	private void Seal()
	{
		if (_type == null && _text == null)
		{
			throw new InvalidOperationException(SR.NullTypeIllegal);
		}
		if (_firstChild != null && !typeof(IAddChild).IsAssignableFrom(_type))
		{
			throw new InvalidOperationException(SR.Format(SR.TypeMustImplementIAddChild, _type.Name));
		}
		ApplyAutoAliasRules();
		if (_childName != null && _childName != string.Empty)
		{
			if (!IsChildNameValid(_childName))
			{
				throw new InvalidOperationException(SR.Format(SR.ChildNameNamePatternReserved, _childName));
			}
			_childName = string.Intern(_childName);
		}
		else
		{
			_childName = GenerateChildName();
		}
		lock (_synchronized)
		{
			for (int i = 0; i < PropertyValues.Count; i++)
			{
				PropertyValue value = PropertyValues[i];
				value.ChildName = _childName;
				StyleHelper.SealIfSealable(value.ValueInternal);
				PropertyValues[i] = value;
			}
		}
		_sealed = true;
		if (_childName != null && _childName != string.Empty && _frameworkTemplate != null)
		{
			_childIndex = StyleHelper.CreateChildIndexFromChildName(_childName, _frameworkTemplate);
		}
		for (FrameworkElementFactory frameworkElementFactory = _firstChild; frameworkElementFactory != null; frameworkElementFactory = frameworkElementFactory._nextSibling)
		{
			if (_frameworkTemplate != null)
			{
				frameworkElementFactory.Seal(_frameworkTemplate);
			}
		}
	}

	internal DependencyObject InstantiateTree(UncommonField<HybridDictionary[]> dataField, DependencyObject container, DependencyObject parent, List<DependencyObject> affectedChildren, ref List<DependencyObject> noChildIndexChildren, ref FrugalStructList<ChildPropertyDependent> resourceDependents)
	{
		EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, EventTrace.Event.WClientParseFefCrInstBegin);
		FrameworkElement frameworkElement = container as FrameworkElement;
		bool flag = frameworkElement != null;
		DependencyObject dependencyObject = null;
		if (_text != null)
		{
			if (!(parent is IAddChild addChild))
			{
				throw new InvalidOperationException(SR.Format(SR.TypeMustImplementIAddChild, parent.GetType().Name));
			}
			addChild.AddText(_text);
		}
		else
		{
			dependencyObject = CreateDependencyObject();
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, EventTrace.Event.WClientParseFefCrInstEnd);
			FrameworkObject frameworkObject = new FrameworkObject(dependencyObject);
			Visual3D visual3D = null;
			bool flag2 = false;
			if (!frameworkObject.IsValid)
			{
				visual3D = dependencyObject as Visual3D;
				if (visual3D != null)
				{
					flag2 = true;
				}
			}
			bool isFE = frameworkObject.IsFE;
			if (!flag2)
			{
				NewNodeBeginInit(isFE, frameworkObject.FE, frameworkObject.FCE);
				if (StyleHelper.HasResourceDependentsForChild(_childIndex, ref resourceDependents))
				{
					frameworkObject.HasResourceReference = true;
				}
				UpdateChildChains(_childName, _childIndex, isFE, frameworkObject.FE, frameworkObject.FCE, affectedChildren, ref noChildIndexChildren);
				NewNodeStyledParentProperty(container, flag, isFE, frameworkObject.FE, frameworkObject.FCE);
				if (_childIndex != -1)
				{
					StyleHelper.CreateInstanceDataForChild(dataField, container, dependencyObject, _childIndex, _frameworkTemplate.HasInstanceValues, ref _frameworkTemplate.ChildRecordFromChildIndex);
				}
				if (HasLoadedChangeHandler)
				{
					BroadcastEventHelper.AddHasLoadedChangeHandlerFlagInAncestry(dependencyObject);
				}
			}
			else if (_childName != null)
			{
				affectedChildren.Add(dependencyObject);
			}
			else
			{
				if (noChildIndexChildren == null)
				{
					noChildIndexChildren = new List<DependencyObject>(4);
				}
				noChildIndexChildren.Add(dependencyObject);
			}
			if (container == parent)
			{
				TemplateNameScope value = new TemplateNameScope(container);
				NameScope.SetNameScope(dependencyObject, value);
				if (flag)
				{
					frameworkElement.TemplateChild = frameworkObject.FE;
				}
				else
				{
					AddNodeToLogicalTree((FrameworkContentElement)parent, _type, isFE, frameworkObject.FE, frameworkObject.FCE);
				}
			}
			else
			{
				AddNodeToParent(parent, frameworkObject);
			}
			if (!flag2)
			{
				StyleHelper.InvalidatePropertiesOnTemplateNode(container, frameworkObject, _childIndex, ref _frameworkTemplate.ChildRecordFromChildIndex, isDetach: false, this);
			}
			else
			{
				for (int i = 0; i < PropertyValues.Count; i++)
				{
					PropertyValue propertyValue = PropertyValues[i];
					if (propertyValue.ValueType == PropertyValueType.Set)
					{
						object obj = propertyValue.ValueInternal;
						if (obj is Freezable { CanFreeze: false } freezable)
						{
							obj = freezable.Clone();
						}
						if (obj is MarkupExtension markupExtension)
						{
							ProvideValueServiceProvider provideValueServiceProvider = new ProvideValueServiceProvider();
							provideValueServiceProvider.SetData(visual3D, propertyValue.Property);
							obj = markupExtension.ProvideValue(provideValueServiceProvider);
						}
						visual3D.SetValue(propertyValue.Property, obj);
						continue;
					}
					throw new NotSupportedException(SR.Format(SR.Template3DValueOnly, propertyValue.Property));
				}
			}
			for (FrameworkElementFactory frameworkElementFactory = _firstChild; frameworkElementFactory != null; frameworkElementFactory = frameworkElementFactory._nextSibling)
			{
				frameworkElementFactory.InstantiateTree(dataField, container, dependencyObject, affectedChildren, ref noChildIndexChildren, ref resourceDependents);
			}
			if (!flag2)
			{
				NewNodeEndInit(isFE, frameworkObject.FE, frameworkObject.FCE);
			}
		}
		return dependencyObject;
	}

	private void AddNodeToParent(DependencyObject parent, FrameworkObject childFrameworkObject)
	{
		RowDefinition rowDefinition = null;
		ColumnDefinition columnDefinition;
		if (childFrameworkObject.IsFCE && parent is Grid grid && ((columnDefinition = childFrameworkObject.FCE as ColumnDefinition) != null || (rowDefinition = childFrameworkObject.FCE as RowDefinition) != null))
		{
			if (columnDefinition != null)
			{
				grid.ColumnDefinitions.Add(columnDefinition);
			}
			else if (rowDefinition != null)
			{
				grid.RowDefinitions.Add(rowDefinition);
			}
		}
		else
		{
			if (!(parent is IAddChild))
			{
				throw new InvalidOperationException(SR.Format(SR.TypeMustImplementIAddChild, parent.GetType().Name));
			}
			((IAddChild)parent).AddChild(childFrameworkObject.DO);
		}
	}

	internal FrameworkObject InstantiateUnoptimizedTree()
	{
		if (!_sealed)
		{
			throw new InvalidOperationException(SR.FrameworkElementFactoryMustBeSealed);
		}
		FrameworkObject result = new FrameworkObject(CreateDependencyObject());
		result.BeginInit();
		ProvideValueServiceProvider provideValueServiceProvider = null;
		FrameworkTemplate.SetTemplateParentValues(Name, result.DO, _frameworkTemplate, ref provideValueServiceProvider);
		FrameworkElementFactory frameworkElementFactory = _firstChild;
		IAddChild addChild = null;
		if (frameworkElementFactory != null)
		{
			addChild = result.DO as IAddChild;
			if (addChild == null)
			{
				throw new InvalidOperationException(SR.Format(SR.TypeMustImplementIAddChild, result.DO.GetType().Name));
			}
		}
		while (frameworkElementFactory != null)
		{
			if (frameworkElementFactory._text != null)
			{
				addChild.AddText(frameworkElementFactory._text);
			}
			else
			{
				FrameworkObject childFrameworkObject = frameworkElementFactory.InstantiateUnoptimizedTree();
				AddNodeToParent(result.DO, childFrameworkObject);
			}
			frameworkElementFactory = frameworkElementFactory._nextSibling;
		}
		result.EndInit();
		return result;
	}

	private static void UpdateChildChains(string childID, int childIndex, bool treeNodeIsFE, FrameworkElement treeNodeFE, FrameworkContentElement treeNodeFCE, List<DependencyObject> affectedChildren, ref List<DependencyObject> noChildIndexChildren)
	{
		if (childID != null)
		{
			if (treeNodeIsFE)
			{
				treeNodeFE.TemplateChildIndex = childIndex;
			}
			else
			{
				treeNodeFCE.TemplateChildIndex = childIndex;
			}
			affectedChildren.Add(treeNodeIsFE ? ((DependencyObject)treeNodeFE) : ((DependencyObject)treeNodeFCE));
		}
		else
		{
			if (noChildIndexChildren == null)
			{
				noChildIndexChildren = new List<DependencyObject>(4);
			}
			noChildIndexChildren.Add(treeNodeIsFE ? ((DependencyObject)treeNodeFE) : ((DependencyObject)treeNodeFCE));
		}
	}

	internal static void NewNodeBeginInit(bool treeNodeIsFE, FrameworkElement treeNodeFE, FrameworkContentElement treeNodeFCE)
	{
		if (treeNodeIsFE)
		{
			treeNodeFE.BeginInit();
		}
		else
		{
			treeNodeFCE.BeginInit();
		}
	}

	private static void NewNodeEndInit(bool treeNodeIsFE, FrameworkElement treeNodeFE, FrameworkContentElement treeNodeFCE)
	{
		if (treeNodeIsFE)
		{
			treeNodeFE.EndInit();
		}
		else
		{
			treeNodeFCE.EndInit();
		}
	}

	private static void NewNodeStyledParentProperty(DependencyObject container, bool isContainerAnFE, bool treeNodeIsFE, FrameworkElement treeNodeFE, FrameworkContentElement treeNodeFCE)
	{
		if (treeNodeIsFE)
		{
			treeNodeFE._templatedParent = container;
			treeNodeFE.IsTemplatedParentAnFE = isContainerAnFE;
		}
		else
		{
			treeNodeFCE._templatedParent = container;
			treeNodeFCE.IsTemplatedParentAnFE = isContainerAnFE;
		}
	}

	internal static void AddNodeToLogicalTree(DependencyObject parent, Type type, bool treeNodeIsFE, FrameworkElement treeNodeFE, FrameworkContentElement treeNodeFCE)
	{
		if (parent is FrameworkContentElement { LogicalChildren: { } logicalChildren } && logicalChildren.MoveNext())
		{
			throw new InvalidOperationException(SR.Format(SR.AlreadyHasLogicalChildren, parent.GetType().Name));
		}
		if (!(parent is IAddChild addChild))
		{
			throw new InvalidOperationException(SR.Format(SR.CannotHookupFCERoot, type.Name));
		}
		if (treeNodeFE != null)
		{
			addChild.AddChild(treeNodeFE);
		}
		else
		{
			addChild.AddChild(treeNodeFCE);
		}
	}

	internal bool IsChildNameValid(string childName)
	{
		return !childName.StartsWith(AutoGenChildNamePrefix, StringComparison.Ordinal);
	}

	private string GenerateChildName()
	{
		string result = AutoGenChildNamePrefix + AutoGenChildNamePostfix.ToString(CultureInfo.InvariantCulture);
		Interlocked.Increment(ref AutoGenChildNamePostfix);
		return result;
	}

	private void ApplyAutoAliasRules()
	{
		if (typeof(ContentPresenter).IsAssignableFrom(_type))
		{
			object value = GetValue(ContentPresenter.ContentSourceProperty);
			string text = ((value == DependencyProperty.UnsetValue) ? "Content" : ((string)value));
			if (string.IsNullOrEmpty(text) || IsValueDefined(ContentPresenter.ContentProperty))
			{
				return;
			}
			Type targetTypeInternal = _frameworkTemplate.TargetTypeInternal;
			DependencyProperty dependencyProperty = DependencyProperty.FromName(text, targetTypeInternal);
			DependencyProperty dependencyProperty2 = DependencyProperty.FromName(text + "Template", targetTypeInternal);
			DependencyProperty dependencyProperty3 = DependencyProperty.FromName(text + "TemplateSelector", targetTypeInternal);
			DependencyProperty dependencyProperty4 = DependencyProperty.FromName(text + "StringFormat", targetTypeInternal);
			if (dependencyProperty == null && value != DependencyProperty.UnsetValue)
			{
				throw new InvalidOperationException(SR.Format(SR.MissingContentSource, text, targetTypeInternal));
			}
			if (dependencyProperty != null)
			{
				SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(dependencyProperty));
			}
			if (!IsValueDefined(ContentPresenter.ContentTemplateProperty) && !IsValueDefined(ContentPresenter.ContentTemplateSelectorProperty) && !IsValueDefined(ContentPresenter.ContentStringFormatProperty))
			{
				if (dependencyProperty2 != null)
				{
					SetValue(ContentPresenter.ContentTemplateProperty, new TemplateBindingExtension(dependencyProperty2));
				}
				if (dependencyProperty3 != null)
				{
					SetValue(ContentPresenter.ContentTemplateSelectorProperty, new TemplateBindingExtension(dependencyProperty3));
				}
				if (dependencyProperty4 != null)
				{
					SetValue(ContentPresenter.ContentStringFormatProperty, new TemplateBindingExtension(dependencyProperty4));
				}
			}
		}
		else
		{
			if (!typeof(GridViewRowPresenter).IsAssignableFrom(_type))
			{
				return;
			}
			if (!IsValueDefined(GridViewRowPresenter.ContentProperty))
			{
				Type targetTypeInternal2 = _frameworkTemplate.TargetTypeInternal;
				DependencyProperty dependencyProperty5 = DependencyProperty.FromName("Content", targetTypeInternal2);
				if (dependencyProperty5 != null)
				{
					SetValue(GridViewRowPresenter.ContentProperty, new TemplateBindingExtension(dependencyProperty5));
				}
			}
			if (!IsValueDefined(GridViewRowPresenterBase.ColumnsProperty))
			{
				SetValue(GridViewRowPresenterBase.ColumnsProperty, new TemplateBindingExtension(GridView.ColumnCollectionProperty));
			}
		}
	}

	private bool IsValueDefined(DependencyProperty dp)
	{
		for (int i = 0; i < PropertyValues.Count; i++)
		{
			PropertyValue propertyValue = PropertyValues[i];
			if (propertyValue.Property == dp && (propertyValue.ValueType == PropertyValueType.Set || propertyValue.ValueType == PropertyValueType.Resource || propertyValue.ValueType == PropertyValueType.TemplateBinding))
			{
				return true;
			}
		}
		return false;
	}
}
