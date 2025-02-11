using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Threading;
using MS.Utility;

namespace System.Windows;

/// <summary>Enables the sharing of properties, resources, and event handlers between instances of a type.</summary>
[Localizability(LocalizationCategory.Ignore)]
[DictionaryKeyProperty("TargetType")]
[ContentProperty("Setters")]
public class Style : DispatcherObject, INameScope, IAddChild, ISealable, IHaveResources, IQueryAmbient
{
	private NameScope _nameScope = new NameScope();

	private EventHandlersStore _eventHandlersStore;

	private bool _sealed;

	private bool _hasInstanceValues;

	internal static readonly Type DefaultTargetType;

	private bool _hasLoadedChangeHandler;

	private Type _targetType = DefaultTargetType;

	private Style _basedOn;

	private TriggerCollection _visualTriggers;

	private SetterBaseCollection _setters;

	internal ResourceDictionary _resources;

	internal int GlobalIndex;

	internal FrugalStructList<ChildRecord> ChildRecordFromChildIndex;

	internal FrugalStructList<ItemStructMap<TriggerSourceRecord>> TriggerSourceRecordFromChildIndex;

	internal FrugalMap PropertyTriggersWithActions;

	internal FrugalStructList<PropertyValue> PropertyValues;

	internal FrugalStructList<ContainerDependent> ContainerDependents;

	internal FrugalStructList<ChildPropertyDependent> ResourceDependents;

	internal ItemStructList<ChildEventDependent> EventDependents = new ItemStructList<ChildEventDependent>(1);

	internal HybridDictionary _triggerActions;

	internal HybridDictionary _dataTriggerRecordFromBinding;

	internal HybridDictionary DataTriggersWithActions;

	private static int StyleInstanceCount;

	internal static object Synchronized;

	private const int TargetTypeID = 1;

	internal const int BasedOnID = 2;

	private const int HasEventSetter = 16;

	private int _modified;

	/// <summary>Gets a value that indicates whether the style is read-only and cannot be changed.</summary>
	/// <returns>true if the style is sealed; otherwise false.</returns>
	public bool IsSealed
	{
		get
		{
			VerifyAccess();
			return _sealed;
		}
	}

	/// <summary>Gets or sets the type for which this style is intended.</summary>
	/// <returns>The target type for this style.</returns>
	[Ambient]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public Type TargetType
	{
		get
		{
			VerifyAccess();
			return _targetType;
		}
		set
		{
			VerifyAccess();
			if (_sealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "Style"));
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!typeof(FrameworkElement).IsAssignableFrom(value) && !typeof(FrameworkContentElement).IsAssignableFrom(value) && !(DefaultTargetType == value))
			{
				throw new ArgumentException(SR.Format(SR.MustBeFrameworkDerived, value.Name));
			}
			_targetType = value;
			SetModified(1);
		}
	}

	/// <summary>Gets or sets a defined style that is the basis of the current style.</summary>
	/// <returns>A defined style that is the basis of the current style. The default value is null.</returns>
	[DefaultValue(null)]
	[Ambient]
	public Style BasedOn
	{
		get
		{
			VerifyAccess();
			return _basedOn;
		}
		set
		{
			VerifyAccess();
			if (_sealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "Style"));
			}
			if (value == this)
			{
				throw new ArgumentException(SR.StyleCannotBeBasedOnSelf);
			}
			_basedOn = value;
			SetModified(2);
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Windows.TriggerBase" /> objects that apply property values based on specified conditions.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.TriggerBase" /> objects. The default is an empty collection.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public TriggerCollection Triggers
	{
		get
		{
			VerifyAccess();
			if (_visualTriggers == null)
			{
				_visualTriggers = new TriggerCollection();
				if (_sealed)
				{
					_visualTriggers.Seal();
				}
			}
			return _visualTriggers;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Windows.Setter" /> and <see cref="T:System.Windows.EventSetter" /> objects.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Setter" /> and <see cref="T:System.Windows.EventSetter" /> objects. The default is an empty collection.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public SetterBaseCollection Setters
	{
		get
		{
			VerifyAccess();
			if (_setters == null)
			{
				_setters = new SetterBaseCollection();
				if (_sealed)
				{
					_setters.Seal();
				}
			}
			return _setters;
		}
	}

	/// <summary>Gets or sets the collection of resources that can be used within the scope of this style.</summary>
	/// <returns>The resources that can be used within the scope of this style.</returns>
	[Ambient]
	public ResourceDictionary Resources
	{
		get
		{
			VerifyAccess();
			if (_resources == null)
			{
				_resources = new ResourceDictionary();
				_resources.CanBeAccessedAcrossThreads = true;
				if (_sealed)
				{
					_resources.IsReadOnly = true;
				}
			}
			return _resources;
		}
		set
		{
			VerifyAccess();
			if (_sealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "Style"));
			}
			_resources = value;
			if (_resources != null)
			{
				_resources.CanBeAccessedAcrossThreads = true;
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

	bool ISealable.CanSeal => true;

	bool ISealable.IsSealed => IsSealed;

	internal bool HasResourceReferences => ResourceDependents.Count > 0;

	internal EventHandlersStore EventHandlersStore => _eventHandlersStore;

	internal bool HasEventDependents => EventDependents.Count > 0;

	internal bool HasEventSetters => IsModified(16);

	internal bool HasInstanceValues => _hasInstanceValues;

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

	internal bool IsBasedOnModified => IsModified(2);

	static Style()
	{
		DefaultTargetType = typeof(IFrameworkInputElement);
		StyleInstanceCount = 0;
		Synchronized = new object();
		StyleHelper.RegisterAlternateExpressionStorage();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Style" /> class. </summary>
	public Style()
	{
		GetUniqueGlobalIndex();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Style" /> class to use on the specified <see cref="T:System.Type" />. </summary>
	/// <param name="targetType">The type to which the style will apply.</param>
	public Style(Type targetType)
	{
		TargetType = targetType;
		GetUniqueGlobalIndex();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Style" /> class to use on the specified <see cref="T:System.Type" /> and based on the specified <see cref="T:System.Windows.Style" />. </summary>
	/// <param name="targetType">The type to which the style will apply.</param>
	/// <param name="basedOn">The style to base this style on.</param>
	public Style(Type targetType, Style basedOn)
	{
		TargetType = targetType;
		BasedOn = basedOn;
		GetUniqueGlobalIndex();
	}

	/// <summary>Registers a new name-object pair in the current namescope.</summary>
	/// <param name="name">The name to register.</param>
	/// <param name="scopedElement">The object to map to the specified <paramref name="name" />.</param>
	public void RegisterName(string name, object scopedElement)
	{
		VerifyAccess();
		_nameScope.RegisterName(name, scopedElement);
	}

	/// <summary>Removes a name-object mapping from the namescope.</summary>
	/// <param name="name">The name of the mapping to remove.</param>
	public void UnregisterName(string name)
	{
		VerifyAccess();
		_nameScope.UnregisterName(name);
	}

	/// <summary>Returns an object that has the provided identifying name. </summary>
	/// <returns>The object, if found. Returns null if no object of that name was found.</returns>
	/// <param name="name">The name identifier for the object being requested.</param>
	object INameScope.FindName(string name)
	{
		VerifyAccess();
		return _nameScope.FindName(name);
	}

	private void GetUniqueGlobalIndex()
	{
		lock (Synchronized)
		{
			StyleInstanceCount++;
			GlobalIndex = StyleInstanceCount;
		}
	}

	internal object FindResource(object resourceKey, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference)
	{
		bool canCache;
		if (_resources != null && _resources.Contains(resourceKey))
		{
			return _resources.FetchResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference, out canCache);
		}
		if (_basedOn != null)
		{
			return _basedOn.FindResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
		}
		return DependencyProperty.UnsetValue;
	}

	internal ResourceDictionary FindResourceDictionary(object resourceKey)
	{
		if (_resources != null && _resources.Contains(resourceKey))
		{
			return _resources;
		}
		if (_basedOn != null)
		{
			return _basedOn.FindResourceDictionary(resourceKey);
		}
		return null;
	}

	/// <summary>Queries whether a specified ambient property is available in the current scope.</summary>
	/// <returns>true if the requested ambient property is available; otherwise, false.</returns>
	/// <param name="propertyName">The name of the requested ambient property.</param>
	bool IQueryAmbient.IsAmbientPropertyAvailable(string propertyName)
	{
		if (!(propertyName == "Resources"))
		{
			if (propertyName == "BasedOn" && _basedOn == null)
			{
				return false;
			}
		}
		else if (_resources == null)
		{
			return false;
		}
		return true;
	}

	/// <summary>Adds a child object. </summary>
	/// <param name="value">The child object to add.</param>
	void IAddChild.AddChild(object value)
	{
		VerifyAccess();
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!(value is SetterBase item))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, value.GetType(), typeof(SetterBase)), "value");
		}
		Setters.Add(item);
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="text">The text to add to the object.</param>
	void IAddChild.AddText(string text)
	{
		VerifyAccess();
		XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
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
			PropertyValue value2 = PropertyValues[num];
			value2.ValueType = valueType;
			value2.ValueInternal = value;
			PropertyValues[num] = value2;
		}
		else
		{
			PropertyValue value3 = default(PropertyValue);
			value3.ValueType = valueType;
			value3.ChildName = "~Self";
			value3.Property = dp;
			value3.ValueInternal = value;
			PropertyValues.Add(value3);
		}
	}

	internal void CheckTargetType(object element)
	{
		if (!(DefaultTargetType == TargetType))
		{
			Type type = element.GetType();
			if (!TargetType.IsAssignableFrom(type))
			{
				throw new InvalidOperationException(SR.Format(SR.StyleTargetTypeMismatchWithElement, TargetType.Name, type.Name));
			}
		}
	}

	/// <summary>Locks this style and all factories and triggers so they cannot be changed.</summary>
	public void Seal()
	{
		VerifyAccess();
		if (!_sealed)
		{
			if (_targetType == null)
			{
				throw new InvalidOperationException(SR.Format(SR.NullPropertyIllegal, "TargetType"));
			}
			if (_basedOn != null && DefaultTargetType != _basedOn.TargetType && !_basedOn.TargetType.IsAssignableFrom(_targetType))
			{
				throw new InvalidOperationException(SR.Format(SR.MustBaseOnStyleOfABaseType, _targetType.Name));
			}
			if (_setters != null)
			{
				_setters.Seal();
			}
			if (_visualTriggers != null)
			{
				_visualTriggers.Seal();
			}
			CheckForCircularBasedOnReferences();
			if (_basedOn != null)
			{
				_basedOn.Seal();
			}
			if (_resources != null)
			{
				_resources.IsReadOnly = true;
			}
			ProcessSetters(this);
			StyleHelper.AddEventDependent(0, EventHandlersStore, ref EventDependents);
			ProcessSelfStyles(this);
			ProcessVisualTriggers(this);
			StyleHelper.SortResourceDependents(ref ResourceDependents);
			_sealed = true;
			DetachFromDispatcher();
		}
	}

	private void CheckForCircularBasedOnReferences()
	{
		Stack stack = new Stack(10);
		for (Style style = this; style != null; style = style.BasedOn)
		{
			if (stack.Contains(style))
			{
				throw new InvalidOperationException(SR.Format(SR.StyleBasedOnHasLoop));
			}
			stack.Push(style);
		}
	}

	private void ProcessSetters(Style style)
	{
		if (style == null)
		{
			return;
		}
		style.Setters.Seal();
		if (PropertyValues.Count == 0)
		{
			PropertyValues = new FrugalStructList<PropertyValue>(style.Setters.Count);
		}
		for (int i = 0; i < style.Setters.Count; i++)
		{
			SetterBase setterBase = style.Setters[i];
			if (setterBase is Setter setter)
			{
				if (setter.TargetName != null)
				{
					throw new InvalidOperationException(SR.Format(SR.SetterOnStyleNotAllowedToHaveTarget, setter.TargetName));
				}
				if (style == this)
				{
					if (!(setter.ValueInternal is DynamicResourceExtension dynamicResourceExtension))
					{
						UpdatePropertyValueList(setter.Property, PropertyValueType.Set, setter.ValueInternal);
					}
					else
					{
						UpdatePropertyValueList(setter.Property, PropertyValueType.Resource, dynamicResourceExtension.ResourceKey);
					}
				}
				continue;
			}
			EventSetter eventSetter = (EventSetter)setterBase;
			if (_eventHandlersStore == null)
			{
				_eventHandlersStore = new EventHandlersStore();
			}
			_eventHandlersStore.AddRoutedEventHandler(eventSetter.Event, eventSetter.Handler, eventSetter.HandledEventsToo);
			SetModified(16);
			if (eventSetter.Event == FrameworkElement.LoadedEvent || eventSetter.Event == FrameworkElement.UnloadedEvent)
			{
				_hasLoadedChangeHandler = true;
			}
		}
		ProcessSetters(style._basedOn);
	}

	private void ProcessSelfStyles(Style style)
	{
		if (style != null)
		{
			ProcessSelfStyles(style._basedOn);
			for (int i = 0; i < style.PropertyValues.Count; i++)
			{
				PropertyValue propertyValue = style.PropertyValues[i];
				StyleHelper.UpdateTables(ref propertyValue, ref ChildRecordFromChildIndex, ref TriggerSourceRecordFromChildIndex, ref ResourceDependents, ref _dataTriggerRecordFromBinding, null, ref _hasInstanceValues);
				StyleHelper.AddContainerDependent(propertyValue.Property, fromVisualTrigger: false, ref ContainerDependents);
			}
		}
	}

	private void ProcessVisualTriggers(Style style)
	{
		if (style == null)
		{
			return;
		}
		ProcessVisualTriggers(style._basedOn);
		if (style._visualTriggers == null)
		{
			return;
		}
		int count = style._visualTriggers.Count;
		for (int i = 0; i < count; i++)
		{
			TriggerBase triggerBase = style._visualTriggers[i];
			for (int j = 0; j < triggerBase.PropertyValues.Count; j++)
			{
				PropertyValue propertyValue = triggerBase.PropertyValues[j];
				if (propertyValue.ChildName != "~Self")
				{
					throw new InvalidOperationException(SR.StyleTriggersCannotTargetTheTemplate);
				}
				TriggerCondition[] conditions = propertyValue.Conditions;
				for (int k = 0; k < conditions.Length; k++)
				{
					if (conditions[k].SourceName != "~Self")
					{
						throw new InvalidOperationException(SR.Format(SR.TriggerOnStyleNotAllowedToHaveSource, conditions[k].SourceName));
					}
				}
				StyleHelper.AddContainerDependent(propertyValue.Property, fromVisualTrigger: true, ref ContainerDependents);
				StyleHelper.UpdateTables(ref propertyValue, ref ChildRecordFromChildIndex, ref TriggerSourceRecordFromChildIndex, ref ResourceDependents, ref _dataTriggerRecordFromBinding, null, ref _hasInstanceValues);
			}
			if (triggerBase.HasEnterActions || triggerBase.HasExitActions)
			{
				if (triggerBase is Trigger)
				{
					StyleHelper.AddPropertyTriggerWithAction(triggerBase, ((Trigger)triggerBase).Property, ref PropertyTriggersWithActions);
				}
				else if (triggerBase is MultiTrigger)
				{
					MultiTrigger multiTrigger = (MultiTrigger)triggerBase;
					for (int l = 0; l < multiTrigger.Conditions.Count; l++)
					{
						Condition condition = multiTrigger.Conditions[l];
						StyleHelper.AddPropertyTriggerWithAction(triggerBase, condition.Property, ref PropertyTriggersWithActions);
					}
				}
				else if (triggerBase is DataTrigger)
				{
					StyleHelper.AddDataTriggerWithAction(triggerBase, ((DataTrigger)triggerBase).Binding, ref DataTriggersWithActions);
				}
				else
				{
					if (!(triggerBase is MultiDataTrigger))
					{
						throw new InvalidOperationException(SR.Format(SR.UnsupportedTriggerInStyle, triggerBase.GetType().Name));
					}
					MultiDataTrigger multiDataTrigger = (MultiDataTrigger)triggerBase;
					for (int m = 0; m < multiDataTrigger.Conditions.Count; m++)
					{
						Condition condition2 = multiDataTrigger.Conditions[m];
						StyleHelper.AddDataTriggerWithAction(triggerBase, condition2.Binding, ref DataTriggersWithActions);
					}
				}
			}
			if (triggerBase is EventTrigger eventTrigger)
			{
				if (eventTrigger.SourceName != null && eventTrigger.SourceName.Length > 0)
				{
					throw new InvalidOperationException(SR.Format(SR.EventTriggerOnStyleNotAllowedToHaveTarget, eventTrigger.SourceName));
				}
				StyleHelper.ProcessEventTrigger(eventTrigger, null, ref _triggerActions, ref EventDependents, null, null, ref _eventHandlersStore, ref _hasLoadedChangeHandler);
			}
		}
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.Style" />.    </summary>
	/// <returns>The hash code for this <see cref="T:System.Windows.Style" />.   </returns>
	public override int GetHashCode()
	{
		VerifyAccess();
		return GlobalIndex;
	}

	void ISealable.Seal()
	{
		Seal();
	}

	private static bool IsEqual(object a, object b)
	{
		return a?.Equals(b) ?? (b == null);
	}

	private void SetModified(int id)
	{
		_modified |= id;
	}

	internal bool IsModified(int id)
	{
		return (id & _modified) != 0;
	}
}
