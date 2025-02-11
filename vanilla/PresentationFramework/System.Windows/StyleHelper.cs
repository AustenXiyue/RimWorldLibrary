using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using MS.Internal;
using MS.Internal.Data;
using MS.Utility;

namespace System.Windows;

internal static class StyleHelper
{
	internal static readonly UncommonField<HybridDictionary[]> StyleDataField;

	internal static readonly UncommonField<HybridDictionary[]> TemplateDataField;

	internal static readonly UncommonField<HybridDictionary> ParentTemplateValuesField;

	internal static readonly UncommonField<HybridDictionary[]> ThemeStyleDataField;

	internal static readonly UncommonField<List<DependencyObject>> TemplatedFeChildrenField;

	internal static readonly UncommonField<Hashtable> TemplatedNonFeChildrenField;

	internal const string SelfName = "~Self";

	internal static FrugalStructList<ContainerDependent> EmptyContainerDependents;

	internal static readonly object NotYetApplied;

	private static AlternativeExpressionStorageCallback _getExpression;

	internal static RoutedEventHandler EventTriggerHandlerOnContainer;

	internal static RoutedEventHandler EventTriggerHandlerOnChild;

	internal const int UnsharedTemplateContentPropertyIndex = -1;

	static StyleHelper()
	{
		StyleDataField = new UncommonField<HybridDictionary[]>();
		TemplateDataField = new UncommonField<HybridDictionary[]>();
		ParentTemplateValuesField = new UncommonField<HybridDictionary>();
		ThemeStyleDataField = new UncommonField<HybridDictionary[]>();
		TemplatedFeChildrenField = new UncommonField<List<DependencyObject>>();
		TemplatedNonFeChildrenField = new UncommonField<Hashtable>();
		NotYetApplied = new NamedObject("NotYetApplied");
		EventTriggerHandlerOnContainer = ExecuteEventTriggerActionsOnContainer;
		EventTriggerHandlerOnChild = ExecuteEventTriggerActionsOnChild;
		RegisterAlternateExpressionStorage();
	}

	internal static void UpdateStyleCache(FrameworkElement fe, FrameworkContentElement fce, Style oldStyle, Style newStyle, ref Style styleCache)
	{
		if (newStyle != null)
		{
			DependencyObject dependencyObject = fe;
			if (dependencyObject == null)
			{
				dependencyObject = fce;
			}
			newStyle.CheckTargetType(dependencyObject);
			newStyle.Seal();
		}
		styleCache = newStyle;
		DoStyleInvalidations(fe, fce, oldStyle, newStyle);
		ExecuteOnApplyEnterExitActions(fe, fce, newStyle, StyleDataField);
	}

	internal static void UpdateThemeStyleCache(FrameworkElement fe, FrameworkContentElement fce, Style oldThemeStyle, Style newThemeStyle, ref Style themeStyleCache)
	{
		if (newThemeStyle != null)
		{
			DependencyObject dependencyObject = fe;
			if (dependencyObject == null)
			{
				dependencyObject = fce;
			}
			newThemeStyle.CheckTargetType(dependencyObject);
			newThemeStyle.Seal();
			if (IsSetOnContainer(FrameworkElement.OverridesDefaultStyleProperty, ref newThemeStyle.ContainerDependents, alsoFromTriggers: true))
			{
				throw new InvalidOperationException(SR.CannotHaveOverridesDefaultStyleInThemeStyle);
			}
			if (newThemeStyle.HasEventSetters)
			{
				throw new InvalidOperationException(SR.CannotHaveEventHandlersInThemeStyle);
			}
		}
		themeStyleCache = newThemeStyle;
		Style style = null;
		if (fe != null)
		{
			if (ShouldGetValueFromStyle(FrameworkElement.DefaultStyleKeyProperty))
			{
				style = fe.Style;
			}
		}
		else if (ShouldGetValueFromStyle(FrameworkContentElement.DefaultStyleKeyProperty))
		{
			style = fce.Style;
		}
		DoThemeStyleInvalidations(fe, fce, oldThemeStyle, newThemeStyle, style);
		ExecuteOnApplyEnterExitActions(fe, fce, newThemeStyle, ThemeStyleDataField);
	}

	internal static Style GetThemeStyle(FrameworkElement fe, FrameworkContentElement fce)
	{
		object obj = null;
		Style style = null;
		Style style2 = null;
		bool overridesDefaultStyle;
		if (fe != null)
		{
			fe.HasThemeStyleEverBeenFetched = true;
			obj = fe.DefaultStyleKey;
			overridesDefaultStyle = fe.OverridesDefaultStyle;
			style = fe.ThemeStyle;
		}
		else
		{
			fce.HasThemeStyleEverBeenFetched = true;
			obj = fce.DefaultStyleKey;
			overridesDefaultStyle = fce.OverridesDefaultStyle;
			style = fce.ThemeStyle;
		}
		if (obj != null && !overridesDefaultStyle)
		{
			DependencyObjectType dependencyObjectType = ((fe == null) ? fce.DTypeThemeStyleKey : fe.DTypeThemeStyleKey);
			object obj2 = ((dependencyObjectType == null || !(dependencyObjectType.SystemType != null) || !dependencyObjectType.SystemType.Equals(obj)) ? SystemResources.FindResourceInternal(obj) : SystemResources.FindThemeStyle(dependencyObjectType));
			if (obj2 != null)
			{
				if (!(obj2 is Style))
				{
					throw new InvalidOperationException(SR.Format(SR.SystemResourceForTypeIsNotStyle, obj));
				}
				style2 = (Style)obj2;
			}
			if (style2 == null)
			{
				Type type = obj as Type;
				if (type != null)
				{
					PropertyMetadata metadata = FrameworkElement.StyleProperty.GetMetadata(type);
					if (metadata != null)
					{
						style2 = metadata.DefaultValue as Style;
					}
				}
			}
		}
		if (style != style2)
		{
			if (fe != null)
			{
				FrameworkElement.OnThemeStyleChanged(fe, style, style2);
			}
			else
			{
				FrameworkContentElement.OnThemeStyleChanged(fce, style, style2);
			}
		}
		return style2;
	}

	internal static void UpdateTemplateCache(FrameworkElement fe, FrameworkTemplate oldTemplate, FrameworkTemplate newTemplate, DependencyProperty templateProperty)
	{
		newTemplate?.Seal();
		fe.TemplateCache = newTemplate;
		DoTemplateInvalidations(fe, oldTemplate);
		ExecuteOnApplyEnterExitActions(fe, null, newTemplate);
	}

	internal static void SealTemplate(FrameworkTemplate frameworkTemplate, ref bool isSealed, FrameworkElementFactory templateRoot, TriggerCollection triggers, ResourceDictionary resources, HybridDictionary childIndexFromChildID, ref FrugalStructList<ChildRecord> childRecordFromChildIndex, ref FrugalStructList<ItemStructMap<TriggerSourceRecord>> triggerSourceRecordFromChildIndex, ref FrugalStructList<ContainerDependent> containerDependents, ref FrugalStructList<ChildPropertyDependent> resourceDependents, ref ItemStructList<ChildEventDependent> eventDependents, ref HybridDictionary triggerActions, ref HybridDictionary dataTriggerRecordFromBinding, ref bool hasInstanceValues, ref EventHandlersStore eventHandlersStore)
	{
		if (!isSealed)
		{
			frameworkTemplate?.ProcessTemplateBeforeSeal();
			templateRoot?.Seal(frameworkTemplate);
			triggers?.Seal();
			if (resources != null)
			{
				resources.IsReadOnly = true;
			}
			if (templateRoot != null)
			{
				ProcessTemplateContentFromFEF(templateRoot, ref childRecordFromChildIndex, ref triggerSourceRecordFromChildIndex, ref resourceDependents, ref eventDependents, ref dataTriggerRecordFromBinding, childIndexFromChildID, ref hasInstanceValues);
			}
			bool hasLoadedChangeHandler = false;
			ProcessTemplateTriggers(triggers, frameworkTemplate, ref childRecordFromChildIndex, ref triggerSourceRecordFromChildIndex, ref containerDependents, ref resourceDependents, ref eventDependents, ref dataTriggerRecordFromBinding, childIndexFromChildID, ref hasInstanceValues, ref triggerActions, templateRoot, ref eventHandlersStore, ref frameworkTemplate.PropertyTriggersWithActions, ref frameworkTemplate.DataTriggersWithActions, ref hasLoadedChangeHandler);
			frameworkTemplate.HasLoadedChangeHandler = hasLoadedChangeHandler;
			frameworkTemplate.SetResourceReferenceState();
			isSealed = true;
			frameworkTemplate.DetachFromDispatcher();
			if (IsSetOnContainer(Control.TemplateProperty, ref containerDependents, alsoFromTriggers: true) || IsSetOnContainer(ContentPresenter.TemplateProperty, ref containerDependents, alsoFromTriggers: true))
			{
				throw new InvalidOperationException(SR.Format(SR.CannotHavePropertyInTemplate, Control.TemplateProperty.Name));
			}
			if (IsSetOnContainer(FrameworkElement.StyleProperty, ref containerDependents, alsoFromTriggers: true))
			{
				throw new InvalidOperationException(SR.Format(SR.CannotHavePropertyInTemplate, FrameworkElement.StyleProperty.Name));
			}
			if (IsSetOnContainer(FrameworkElement.DefaultStyleKeyProperty, ref containerDependents, alsoFromTriggers: true))
			{
				throw new InvalidOperationException(SR.Format(SR.CannotHavePropertyInTemplate, FrameworkElement.DefaultStyleKeyProperty.Name));
			}
			if (IsSetOnContainer(FrameworkElement.OverridesDefaultStyleProperty, ref containerDependents, alsoFromTriggers: true))
			{
				throw new InvalidOperationException(SR.Format(SR.CannotHavePropertyInTemplate, FrameworkElement.OverridesDefaultStyleProperty.Name));
			}
			if (IsSetOnContainer(FrameworkElement.NameProperty, ref containerDependents, alsoFromTriggers: true))
			{
				throw new InvalidOperationException(SR.Format(SR.CannotHavePropertyInTemplate, FrameworkElement.NameProperty.Name));
			}
		}
	}

	internal static void UpdateTables(ref PropertyValue propertyValue, ref FrugalStructList<ChildRecord> childRecordFromChildIndex, ref FrugalStructList<ItemStructMap<TriggerSourceRecord>> triggerSourceRecordFromChildIndex, ref FrugalStructList<ChildPropertyDependent> resourceDependents, ref HybridDictionary dataTriggerRecordFromBinding, HybridDictionary childIndexFromChildName, ref bool hasInstanceValues)
	{
		int num = QueryChildIndexFromChildName(propertyValue.ChildName, childIndexFromChildName);
		if (num == -1)
		{
			throw new InvalidOperationException(SR.Format(SR.NameNotFound, propertyValue.ChildName));
		}
		object value = propertyValue.ValueInternal;
		bool flag = RequiresInstanceStorage(ref value);
		propertyValue.ValueInternal = value;
		childRecordFromChildIndex.EnsureIndex(num);
		ChildRecord value2 = childRecordFromChildIndex[num];
		int num2 = value2.ValueLookupListFromProperty.EnsureEntry(propertyValue.Property.GlobalIndex);
		ChildValueLookup item = default(ChildValueLookup);
		item.LookupType = (ValueLookupType)propertyValue.ValueType;
		item.Conditions = propertyValue.Conditions;
		item.Property = propertyValue.Property;
		item.Value = propertyValue.ValueInternal;
		value2.ValueLookupListFromProperty.Entries[num2].Value.Add(ref item);
		childRecordFromChildIndex[num] = value2;
		switch ((ValueLookupType)propertyValue.ValueType)
		{
		case ValueLookupType.Simple:
			hasInstanceValues |= flag;
			break;
		case ValueLookupType.Trigger:
		case ValueLookupType.PropertyTriggerResource:
			if (propertyValue.Conditions != null)
			{
				for (int j = 0; j < propertyValue.Conditions.Length; j++)
				{
					int sourceChildIndex = propertyValue.Conditions[j].SourceChildIndex;
					triggerSourceRecordFromChildIndex.EnsureIndex(sourceChildIndex);
					ItemStructMap<TriggerSourceRecord> value4 = triggerSourceRecordFromChildIndex[sourceChildIndex];
					if (propertyValue.Conditions[j].Property == null)
					{
						throw new InvalidOperationException(SR.MissingTriggerProperty);
					}
					int num4 = value4.EnsureEntry(propertyValue.Conditions[j].Property.GlobalIndex);
					AddPropertyDependent(num, propertyValue.Property, ref value4.Entries[num4].Value.ChildPropertyDependents);
					triggerSourceRecordFromChildIndex[sourceChildIndex] = value4;
				}
				if (propertyValue.ValueType == PropertyValueType.PropertyTriggerResource)
				{
					AddResourceDependent(num, propertyValue.Property, propertyValue.ValueInternal, ref resourceDependents);
				}
			}
			if (propertyValue.ValueType != PropertyValueType.PropertyTriggerResource)
			{
				hasInstanceValues |= flag;
			}
			break;
		case ValueLookupType.DataTrigger:
		case ValueLookupType.DataTriggerResource:
			if (propertyValue.Conditions != null)
			{
				if (dataTriggerRecordFromBinding == null)
				{
					dataTriggerRecordFromBinding = new HybridDictionary();
				}
				for (int i = 0; i < propertyValue.Conditions.Length; i++)
				{
					DataTriggerRecord dataTriggerRecord = (DataTriggerRecord)dataTriggerRecordFromBinding[propertyValue.Conditions[i].Binding];
					if (dataTriggerRecord == null)
					{
						dataTriggerRecord = new DataTriggerRecord();
						dataTriggerRecordFromBinding[propertyValue.Conditions[i].Binding] = dataTriggerRecord;
					}
					AddPropertyDependent(num, propertyValue.Property, ref dataTriggerRecord.Dependents);
				}
				if (propertyValue.ValueType == PropertyValueType.DataTriggerResource)
				{
					AddResourceDependent(num, propertyValue.Property, propertyValue.ValueInternal, ref resourceDependents);
				}
			}
			if (propertyValue.ValueType != PropertyValueType.DataTriggerResource)
			{
				hasInstanceValues |= flag;
			}
			break;
		case ValueLookupType.TemplateBinding:
		{
			TemplateBindingExtension obj = (TemplateBindingExtension)propertyValue.ValueInternal;
			DependencyProperty property = propertyValue.Property;
			DependencyProperty property2 = obj.Property;
			int index = 0;
			triggerSourceRecordFromChildIndex.EnsureIndex(index);
			ItemStructMap<TriggerSourceRecord> value3 = triggerSourceRecordFromChildIndex[index];
			int num3 = value3.EnsureEntry(property2.GlobalIndex);
			AddPropertyDependent(num, property, ref value3.Entries[num3].Value.ChildPropertyDependents);
			triggerSourceRecordFromChildIndex[index] = value3;
			break;
		}
		case ValueLookupType.Resource:
			AddResourceDependent(num, propertyValue.Property, propertyValue.ValueInternal, ref resourceDependents);
			break;
		}
	}

	private static bool RequiresInstanceStorage(ref object value)
	{
		DeferredReference deferredReference = null;
		MarkupExtension markupExtension = null;
		Freezable freezable = null;
		if (value is DeferredReference deferredReference2)
		{
			Type valueType = deferredReference2.GetValueType();
			if (valueType != null)
			{
				if (typeof(MarkupExtension).IsAssignableFrom(valueType))
				{
					value = deferredReference2.GetValue(BaseValueSourceInternal.Style);
					if ((markupExtension = value as MarkupExtension) == null)
					{
						freezable = value as Freezable;
					}
				}
				else if (typeof(Freezable).IsAssignableFrom(valueType))
				{
					freezable = (Freezable)deferredReference2.GetValue(BaseValueSourceInternal.Style);
				}
			}
		}
		else if ((markupExtension = value as MarkupExtension) == null)
		{
			freezable = value as Freezable;
		}
		bool result = false;
		if (markupExtension != null)
		{
			value = markupExtension;
			result = true;
		}
		else if (freezable != null)
		{
			value = freezable;
			if (!freezable.CanFreeze)
			{
				result = true;
			}
		}
		return result;
	}

	internal static void AddContainerDependent(DependencyProperty dp, bool fromVisualTrigger, ref FrugalStructList<ContainerDependent> containerDependents)
	{
		ContainerDependent containerDependent;
		for (int i = 0; i < containerDependents.Count; i++)
		{
			containerDependent = containerDependents[i];
			if (dp == containerDependent.Property)
			{
				containerDependent.FromVisualTrigger |= fromVisualTrigger;
				return;
			}
		}
		containerDependent = default(ContainerDependent);
		containerDependent.Property = dp;
		containerDependent.FromVisualTrigger = fromVisualTrigger;
		containerDependents.Add(containerDependent);
	}

	internal static void AddEventDependent(int childIndex, EventHandlersStore eventHandlersStore, ref ItemStructList<ChildEventDependent> eventDependents)
	{
		if (eventHandlersStore != null)
		{
			ChildEventDependent item = default(ChildEventDependent);
			item.ChildIndex = childIndex;
			item.EventHandlersStore = eventHandlersStore;
			eventDependents.Add(ref item);
		}
	}

	private static void AddPropertyDependent(int childIndex, DependencyProperty dp, ref FrugalStructList<ChildPropertyDependent> propertyDependents)
	{
		ChildPropertyDependent value = default(ChildPropertyDependent);
		value.ChildIndex = childIndex;
		value.Property = dp;
		propertyDependents.Add(value);
	}

	private static void AddResourceDependent(int childIndex, DependencyProperty dp, object name, ref FrugalStructList<ChildPropertyDependent> resourceDependents)
	{
		bool flag = true;
		for (int i = 0; i < resourceDependents.Count; i++)
		{
			ChildPropertyDependent childPropertyDependent = resourceDependents[i];
			if (childPropertyDependent.ChildIndex == childIndex && childPropertyDependent.Property == dp && childPropertyDependent.Name == name)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			ChildPropertyDependent value = default(ChildPropertyDependent);
			value.ChildIndex = childIndex;
			value.Property = dp;
			value.Name = name;
			resourceDependents.Add(value);
		}
	}

	internal static void ProcessTemplateContentFromFEF(FrameworkElementFactory factory, ref FrugalStructList<ChildRecord> childRecordFromChildIndex, ref FrugalStructList<ItemStructMap<TriggerSourceRecord>> triggerSourceRecordFromChildIndex, ref FrugalStructList<ChildPropertyDependent> resourceDependents, ref ItemStructList<ChildEventDependent> eventDependents, ref HybridDictionary dataTriggerRecordFromBinding, HybridDictionary childIndexFromChildID, ref bool hasInstanceValues)
	{
		for (int i = 0; i < factory.PropertyValues.Count; i++)
		{
			PropertyValue propertyValue = factory.PropertyValues[i];
			UpdateTables(ref propertyValue, ref childRecordFromChildIndex, ref triggerSourceRecordFromChildIndex, ref resourceDependents, ref dataTriggerRecordFromBinding, childIndexFromChildID, ref hasInstanceValues);
		}
		AddEventDependent(factory._childIndex, factory.EventHandlersStore, ref eventDependents);
		for (factory = factory.FirstChild; factory != null; factory = factory.NextSibling)
		{
			ProcessTemplateContentFromFEF(factory, ref childRecordFromChildIndex, ref triggerSourceRecordFromChildIndex, ref resourceDependents, ref eventDependents, ref dataTriggerRecordFromBinding, childIndexFromChildID, ref hasInstanceValues);
		}
	}

	private static void ProcessTemplateTriggers(TriggerCollection triggers, FrameworkTemplate frameworkTemplate, ref FrugalStructList<ChildRecord> childRecordFromChildIndex, ref FrugalStructList<ItemStructMap<TriggerSourceRecord>> triggerSourceRecordFromChildIndex, ref FrugalStructList<ContainerDependent> containerDependents, ref FrugalStructList<ChildPropertyDependent> resourceDependents, ref ItemStructList<ChildEventDependent> eventDependents, ref HybridDictionary dataTriggerRecordFromBinding, HybridDictionary childIndexFromChildID, ref bool hasInstanceValues, ref HybridDictionary triggerActions, FrameworkElementFactory templateRoot, ref EventHandlersStore eventHandlersStore, ref FrugalMap propertyTriggersWithActions, ref HybridDictionary dataTriggersWithActions, ref bool hasLoadedChangeHandler)
	{
		if (triggers == null)
		{
			return;
		}
		int count = triggers.Count;
		for (int i = 0; i < count; i++)
		{
			TriggerBase triggerBase = triggers[i];
			DetermineTriggerType(triggerBase, out var trigger, out var multiTrigger, out var dataTrigger, out var multiDataTrigger, out var eventTrigger);
			if (trigger != null || multiTrigger != null || dataTrigger != null || multiDataTrigger != null)
			{
				TriggerCondition[] triggerConditions = triggerBase.TriggerConditions;
				for (int j = 0; j < triggerConditions.Length; j++)
				{
					triggerConditions[j].SourceChildIndex = QueryChildIndexFromChildName(triggerConditions[j].SourceName, childIndexFromChildID);
				}
				for (int k = 0; k < triggerBase.PropertyValues.Count; k++)
				{
					PropertyValue propertyValue = triggerBase.PropertyValues[k];
					if (propertyValue.ChildName == "~Self")
					{
						AddContainerDependent(propertyValue.Property, fromVisualTrigger: true, ref containerDependents);
					}
					UpdateTables(ref propertyValue, ref childRecordFromChildIndex, ref triggerSourceRecordFromChildIndex, ref resourceDependents, ref dataTriggerRecordFromBinding, childIndexFromChildID, ref hasInstanceValues);
				}
				if (!triggerBase.HasEnterActions && !triggerBase.HasExitActions)
				{
					continue;
				}
				if (trigger != null)
				{
					AddPropertyTriggerWithAction(triggerBase, trigger.Property, ref propertyTriggersWithActions);
					continue;
				}
				if (multiTrigger != null)
				{
					for (int l = 0; l < multiTrigger.Conditions.Count; l++)
					{
						Condition condition = multiTrigger.Conditions[l];
						AddPropertyTriggerWithAction(triggerBase, condition.Property, ref propertyTriggersWithActions);
					}
					continue;
				}
				if (dataTrigger != null)
				{
					AddDataTriggerWithAction(triggerBase, dataTrigger.Binding, ref dataTriggersWithActions);
					continue;
				}
				if (multiDataTrigger == null)
				{
					throw new InvalidOperationException(SR.Format(SR.UnsupportedTriggerInTemplate, triggerBase.GetType().Name));
				}
				for (int m = 0; m < multiDataTrigger.Conditions.Count; m++)
				{
					Condition condition2 = multiDataTrigger.Conditions[m];
					AddDataTriggerWithAction(triggerBase, condition2.Binding, ref dataTriggersWithActions);
				}
			}
			else
			{
				if (eventTrigger == null)
				{
					throw new InvalidOperationException(SR.Format(SR.UnsupportedTriggerInTemplate, triggerBase.GetType().Name));
				}
				ProcessEventTrigger(eventTrigger, childIndexFromChildID, ref triggerActions, ref eventDependents, templateRoot, frameworkTemplate, ref eventHandlersStore, ref hasLoadedChangeHandler);
			}
		}
	}

	private static void DetermineTriggerType(TriggerBase triggerBase, out Trigger trigger, out MultiTrigger multiTrigger, out DataTrigger dataTrigger, out MultiDataTrigger multiDataTrigger, out EventTrigger eventTrigger)
	{
		if ((trigger = triggerBase as Trigger) != null)
		{
			multiTrigger = null;
			dataTrigger = null;
			multiDataTrigger = null;
			eventTrigger = null;
		}
		else if ((multiTrigger = triggerBase as MultiTrigger) != null)
		{
			dataTrigger = null;
			multiDataTrigger = null;
			eventTrigger = null;
		}
		else if ((dataTrigger = triggerBase as DataTrigger) != null)
		{
			multiDataTrigger = null;
			eventTrigger = null;
		}
		else if ((multiDataTrigger = triggerBase as MultiDataTrigger) != null)
		{
			eventTrigger = null;
		}
		else
		{
			EventTrigger eventTrigger2 = (eventTrigger = triggerBase as EventTrigger);
		}
	}

	internal static void ProcessEventTrigger(EventTrigger eventTrigger, HybridDictionary childIndexFromChildName, ref HybridDictionary triggerActions, ref ItemStructList<ChildEventDependent> eventDependents, FrameworkElementFactory templateRoot, FrameworkTemplate frameworkTemplate, ref EventHandlersStore eventHandlersStore, ref bool hasLoadedChangeHandler)
	{
		if (eventTrigger == null)
		{
			return;
		}
		List<TriggerAction> list = null;
		bool flag = true;
		bool flag2 = false;
		TriggerAction triggerAction = null;
		FrameworkElementFactory frameworkElementFactory = null;
		if (eventTrigger.SourceName == null)
		{
			eventTrigger.TriggerChildIndex = 0;
		}
		else
		{
			int num = QueryChildIndexFromChildName(eventTrigger.SourceName, childIndexFromChildName);
			if (num == -1)
			{
				throw new InvalidOperationException(SR.Format(SR.EventTriggerTargetNameUnresolvable, eventTrigger.SourceName));
			}
			eventTrigger.TriggerChildIndex = num;
		}
		if (triggerActions == null)
		{
			triggerActions = new HybridDictionary();
		}
		else
		{
			list = triggerActions[eventTrigger.RoutedEvent] as List<TriggerAction>;
		}
		if (list == null)
		{
			flag = false;
			list = new List<TriggerAction>();
		}
		for (int i = 0; i < eventTrigger.Actions.Count; i++)
		{
			triggerAction = eventTrigger.Actions[i];
			list.Add(triggerAction);
			flag2 = true;
		}
		if (flag2 && !flag)
		{
			triggerActions[eventTrigger.RoutedEvent] = list;
		}
		if (templateRoot != null || eventTrigger.TriggerChildIndex == 0)
		{
			if (eventTrigger.TriggerChildIndex != 0)
			{
				frameworkElementFactory = FindFEF(templateRoot, eventTrigger.TriggerChildIndex);
			}
			if (eventTrigger.RoutedEvent == FrameworkElement.LoadedEvent || eventTrigger.RoutedEvent == FrameworkElement.UnloadedEvent)
			{
				if (eventTrigger.TriggerChildIndex == 0)
				{
					hasLoadedChangeHandler = true;
				}
				else
				{
					frameworkElementFactory.HasLoadedChangeHandler = true;
				}
			}
			AddDelegateToFireTrigger(eventTrigger.RoutedEvent, eventTrigger.TriggerChildIndex, templateRoot, frameworkElementFactory, ref eventDependents, ref eventHandlersStore);
			return;
		}
		if (eventTrigger.RoutedEvent == FrameworkElement.LoadedEvent || eventTrigger.RoutedEvent == FrameworkElement.UnloadedEvent)
		{
			FrameworkTemplate.TemplateChildLoadedFlags templateChildLoadedFlags = frameworkTemplate._TemplateChildLoadedDictionary[eventTrigger.TriggerChildIndex] as FrameworkTemplate.TemplateChildLoadedFlags;
			if (templateChildLoadedFlags == null)
			{
				templateChildLoadedFlags = new FrameworkTemplate.TemplateChildLoadedFlags();
				frameworkTemplate._TemplateChildLoadedDictionary[eventTrigger.TriggerChildIndex] = templateChildLoadedFlags;
			}
			if (eventTrigger.RoutedEvent == FrameworkElement.LoadedEvent)
			{
				templateChildLoadedFlags.HasLoadedChangedHandler = true;
			}
			else
			{
				templateChildLoadedFlags.HasUnloadedChangedHandler = true;
			}
		}
		AddDelegateToFireTrigger(eventTrigger.RoutedEvent, eventTrigger.TriggerChildIndex, ref eventDependents, ref eventHandlersStore);
	}

	private static void AddDelegateToFireTrigger(RoutedEvent routedEvent, int childIndex, FrameworkElementFactory templateRoot, FrameworkElementFactory childFef, ref ItemStructList<ChildEventDependent> eventDependents, ref EventHandlersStore eventHandlersStore)
	{
		if (childIndex == 0)
		{
			if (eventHandlersStore == null)
			{
				eventHandlersStore = new EventHandlersStore();
				AddEventDependent(0, eventHandlersStore, ref eventDependents);
			}
			eventHandlersStore.AddRoutedEventHandler(routedEvent, EventTriggerHandlerOnContainer, handledEventsToo: false);
		}
		else
		{
			if (childFef.EventHandlersStore == null)
			{
				childFef.EventHandlersStore = new EventHandlersStore();
				AddEventDependent(childIndex, childFef.EventHandlersStore, ref eventDependents);
			}
			childFef.EventHandlersStore.AddRoutedEventHandler(routedEvent, EventTriggerHandlerOnChild, handledEventsToo: false);
		}
	}

	private static void AddDelegateToFireTrigger(RoutedEvent routedEvent, int childIndex, ref ItemStructList<ChildEventDependent> eventDependents, ref EventHandlersStore eventHandlersStore)
	{
		if (eventHandlersStore == null)
		{
			eventHandlersStore = new EventHandlersStore();
		}
		AddEventDependent(childIndex, eventHandlersStore, ref eventDependents);
		eventHandlersStore.AddRoutedEventHandler(routedEvent, EventTriggerHandlerOnChild, handledEventsToo: false);
	}

	internal static void SealIfSealable(object value)
	{
		if (value is ISealable { IsSealed: false, CanSeal: not false } sealable)
		{
			sealable.Seal();
		}
	}

	internal static void UpdateInstanceData(UncommonField<HybridDictionary[]> dataField, FrameworkElement fe, FrameworkContentElement fce, Style oldStyle, Style newStyle, FrameworkTemplate oldFrameworkTemplate, FrameworkTemplate newFrameworkTemplate, InternalFlags hasGeneratedSubTreeFlag)
	{
		DependencyObject dependencyObject = ((fe != null) ? ((DependencyObject)fe) : ((DependencyObject)fce));
		if (oldStyle != null || oldFrameworkTemplate != null)
		{
			ReleaseInstanceData(dataField, dependencyObject, fe, fce, oldStyle, oldFrameworkTemplate, hasGeneratedSubTreeFlag);
		}
		if (newStyle != null || newFrameworkTemplate != null)
		{
			CreateInstanceData(dataField, dependencyObject, fe, fce, newStyle, newFrameworkTemplate);
		}
		else
		{
			dataField.ClearValue(dependencyObject);
		}
	}

	internal static void CreateInstanceData(UncommonField<HybridDictionary[]> dataField, DependencyObject container, FrameworkElement fe, FrameworkContentElement fce, Style newStyle, FrameworkTemplate newFrameworkTemplate)
	{
		if (newStyle != null)
		{
			if (newStyle.HasInstanceValues)
			{
				HybridDictionary instanceValues = EnsureInstanceData(dataField, container, InstanceStyleData.InstanceValues);
				ProcessInstanceValuesForChild(container, container, 0, instanceValues, apply: true, ref newStyle.ChildRecordFromChildIndex);
			}
		}
		else if (newFrameworkTemplate != null && newFrameworkTemplate.HasInstanceValues)
		{
			HybridDictionary instanceValues2 = EnsureInstanceData(dataField, container, InstanceStyleData.InstanceValues);
			ProcessInstanceValuesForChild(container, container, 0, instanceValues2, apply: true, ref newFrameworkTemplate.ChildRecordFromChildIndex);
		}
	}

	internal static void CreateInstanceDataForChild(UncommonField<HybridDictionary[]> dataField, DependencyObject container, DependencyObject child, int childIndex, bool hasInstanceValues, ref FrugalStructList<ChildRecord> childRecordFromChildIndex)
	{
		if (hasInstanceValues)
		{
			HybridDictionary instanceValues = EnsureInstanceData(dataField, container, InstanceStyleData.InstanceValues);
			ProcessInstanceValuesForChild(container, child, childIndex, instanceValues, apply: true, ref childRecordFromChildIndex);
		}
	}

	internal static void ReleaseInstanceData(UncommonField<HybridDictionary[]> dataField, DependencyObject container, FrameworkElement fe, FrameworkContentElement fce, Style oldStyle, FrameworkTemplate oldFrameworkTemplate, InternalFlags hasGeneratedSubTreeFlag)
	{
		HybridDictionary[] value = dataField.GetValue(container);
		if (oldStyle != null)
		{
			HybridDictionary instanceValues = ((value != null) ? value[0] : null);
			ReleaseInstanceDataForDataTriggers(dataField, instanceValues, oldStyle, oldFrameworkTemplate);
			if (oldStyle.HasInstanceValues)
			{
				ProcessInstanceValuesForChild(container, container, 0, instanceValues, apply: false, ref oldStyle.ChildRecordFromChildIndex);
			}
		}
		else if (oldFrameworkTemplate != null)
		{
			HybridDictionary instanceValues2 = ((value != null) ? value[0] : null);
			ReleaseInstanceDataForDataTriggers(dataField, instanceValues2, oldStyle, oldFrameworkTemplate);
			if (oldFrameworkTemplate.HasInstanceValues)
			{
				ProcessInstanceValuesForChild(container, container, 0, instanceValues2, apply: false, ref oldFrameworkTemplate.ChildRecordFromChildIndex);
			}
		}
		else
		{
			HybridDictionary instanceValues3 = ((value != null) ? value[0] : null);
			ReleaseInstanceDataForDataTriggers(dataField, instanceValues3, oldStyle, oldFrameworkTemplate);
		}
	}

	internal static HybridDictionary EnsureInstanceData(UncommonField<HybridDictionary[]> dataField, DependencyObject container, InstanceStyleData dataType)
	{
		return EnsureInstanceData(dataField, container, dataType, -1);
	}

	internal static HybridDictionary EnsureInstanceData(UncommonField<HybridDictionary[]> dataField, DependencyObject container, InstanceStyleData dataType, int initialSize)
	{
		HybridDictionary[] array = dataField.GetValue(container);
		if (array == null)
		{
			array = new HybridDictionary[1];
			dataField.SetValue(container, array);
		}
		if (array[(int)dataType] == null)
		{
			if (initialSize < 0)
			{
				array[(int)dataType] = new HybridDictionary();
			}
			else
			{
				array[(int)dataType] = new HybridDictionary(initialSize);
			}
		}
		return array[(int)dataType];
	}

	private static void ProcessInstanceValuesForChild(DependencyObject container, DependencyObject child, int childIndex, HybridDictionary instanceValues, bool apply, ref FrugalStructList<ChildRecord> childRecordFromChildIndex)
	{
		if (childIndex == -1)
		{
			Helper.DowncastToFEorFCE(child, out var fe, out var fce, throwIfNeither: false);
			childIndex = fe?.TemplateChildIndex ?? fce?.TemplateChildIndex ?? (-1);
		}
		if (0 <= childIndex && childIndex < childRecordFromChildIndex.Count)
		{
			int count = childRecordFromChildIndex[childIndex].ValueLookupListFromProperty.Count;
			for (int i = 0; i < count; i++)
			{
				ProcessInstanceValuesHelper(ref childRecordFromChildIndex[childIndex].ValueLookupListFromProperty.Entries[i].Value, child, childIndex, instanceValues, apply);
			}
		}
	}

	private static void ProcessInstanceValuesHelper(ref ItemStructList<ChildValueLookup> valueLookupList, DependencyObject target, int childIndex, HybridDictionary instanceValues, bool apply)
	{
		for (int num = valueLookupList.Count - 1; num >= 0; num--)
		{
			ValueLookupType lookupType = valueLookupList.List[num].LookupType;
			if ((uint)lookupType <= 1u || lookupType == ValueLookupType.DataTrigger)
			{
				DependencyProperty property = valueLookupList.List[num].Property;
				object value = valueLookupList.List[num].Value;
				if (value is MarkupExtension)
				{
					ProcessInstanceValue(target, childIndex, instanceValues, property, num, apply);
				}
				else if (value is Freezable freezable)
				{
					if (!freezable.CheckAccess())
					{
						throw new InvalidOperationException(SR.Format(SR.CrossThreadAccessOfUnshareableFreezable, freezable.GetType().FullName));
					}
					if (!freezable.IsFrozen)
					{
						ProcessInstanceValue(target, childIndex, instanceValues, property, num, apply);
					}
				}
			}
		}
	}

	internal static void ProcessInstanceValue(DependencyObject target, int childIndex, HybridDictionary instanceValues, DependencyProperty dp, int i, bool apply)
	{
		InstanceValueKey key = new InstanceValueKey(childIndex, dp.GlobalIndex, i);
		if (apply)
		{
			instanceValues[key] = NotYetApplied;
			return;
		}
		object obj = instanceValues[key];
		instanceValues.Remove(key);
		if (obj is Expression expression)
		{
			expression.OnDetach(target, dp);
		}
		else if (obj is Freezable doValue)
		{
			target.RemoveSelfAsInheritanceContext(doValue, dp);
		}
	}

	private static void ReleaseInstanceDataForDataTriggers(UncommonField<HybridDictionary[]> dataField, HybridDictionary instanceValues, Style oldStyle, FrameworkTemplate oldFrameworkTemplate)
	{
		if (instanceValues == null)
		{
			return;
		}
		EventHandler<BindingValueChangedEventArgs> handler = ((dataField == StyleDataField) ? new EventHandler<BindingValueChangedEventArgs>(OnBindingValueInStyleChanged) : ((dataField != TemplateDataField) ? new EventHandler<BindingValueChangedEventArgs>(OnBindingValueInThemeStyleChanged) : new EventHandler<BindingValueChangedEventArgs>(OnBindingValueInTemplateChanged)));
		HybridDictionary hybridDictionary = null;
		if (oldStyle != null)
		{
			hybridDictionary = oldStyle._dataTriggerRecordFromBinding;
		}
		else if (oldFrameworkTemplate != null)
		{
			hybridDictionary = oldFrameworkTemplate._dataTriggerRecordFromBinding;
		}
		if (hybridDictionary != null)
		{
			foreach (BindingBase key in hybridDictionary.Keys)
			{
				ReleaseInstanceDataForTriggerBinding(key, instanceValues, handler);
			}
		}
		HybridDictionary hybridDictionary2 = null;
		if (oldStyle != null)
		{
			hybridDictionary2 = oldStyle.DataTriggersWithActions;
		}
		else if (oldFrameworkTemplate != null)
		{
			hybridDictionary2 = oldFrameworkTemplate.DataTriggersWithActions;
		}
		if (hybridDictionary2 == null)
		{
			return;
		}
		foreach (BindingBase key2 in hybridDictionary2.Keys)
		{
			ReleaseInstanceDataForTriggerBinding(key2, instanceValues, handler);
		}
	}

	private static void ReleaseInstanceDataForTriggerBinding(BindingBase binding, HybridDictionary instanceValues, EventHandler<BindingValueChangedEventArgs> handler)
	{
		BindingExpressionBase bindingExpressionBase = (BindingExpressionBase)instanceValues[binding];
		if (bindingExpressionBase != null)
		{
			bindingExpressionBase.ValueChanged -= handler;
			bindingExpressionBase.Detach();
			instanceValues.Remove(binding);
		}
	}

	internal static bool ApplyTemplateContent(UncommonField<HybridDictionary[]> dataField, DependencyObject container, FrameworkElementFactory templateRoot, int lastChildIndex, HybridDictionary childIndexFromChildID, FrameworkTemplate frameworkTemplate)
	{
		bool result = false;
		FrameworkElement frameworkElement = container as FrameworkElement;
		if (templateRoot != null)
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, EventTrace.Event.WClientParseInstVisTreeBegin);
			CheckForCircularReferencesInTemplateTree(container, frameworkTemplate);
			List<DependencyObject> list = new List<DependencyObject>(lastChildIndex);
			TemplatedFeChildrenField.SetValue(container, list);
			List<DependencyObject> noChildIndexChildren = null;
			templateRoot.InstantiateTree(dataField, container, container, list, ref noChildIndexChildren, ref frameworkTemplate.ResourceDependents);
			if (noChildIndexChildren != null)
			{
				list.AddRange(noChildIndexChildren);
			}
			result = true;
			if (frameworkElement != null && EventTrace.IsEnabled(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose))
			{
				string text = frameworkElement.Name;
				if (text == null || text.Length == 0)
				{
					text = container.GetHashCode().ToString(CultureInfo.InvariantCulture);
				}
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientParseInstVisTreeEnd, EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, string.Format(CultureInfo.InvariantCulture, "Style.InstantiateSubTree for {0} {1}", container.GetType().Name, text));
			}
		}
		else if (frameworkTemplate != null && frameworkTemplate.HasXamlNodeContent)
		{
			EventTrace.EasyTraceEvent(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, EventTrace.Event.WClientParseInstVisTreeBegin);
			CheckForCircularReferencesInTemplateTree(container, frameworkTemplate);
			List<DependencyObject> list2 = new List<DependencyObject>(lastChildIndex);
			TemplatedFeChildrenField.SetValue(container, list2);
			frameworkTemplate.LoadContent(container, list2);
			result = true;
			if (frameworkElement != null && EventTrace.IsEnabled(EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose))
			{
				string text2 = frameworkElement.Name;
				if (text2 == null || text2.Length == 0)
				{
					text2 = container.GetHashCode().ToString(CultureInfo.InvariantCulture);
				}
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientParseInstVisTreeEnd, EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, string.Format(CultureInfo.InvariantCulture, "Style.InstantiateSubTree for {0} {1}", container.GetType().Name, text2));
			}
		}
		else if (frameworkElement != null)
		{
			result = frameworkTemplate.BuildVisualTree(frameworkElement);
		}
		return result;
	}

	internal static void AddCustomTemplateRoot(FrameworkElement container, UIElement child)
	{
		AddCustomTemplateRoot(container, child, checkVisualParent: true, mustCacheTreeStateOnChild: false);
	}

	internal static void AddCustomTemplateRoot(FrameworkElement container, UIElement child, bool checkVisualParent, bool mustCacheTreeStateOnChild)
	{
		if (child != null && checkVisualParent && VisualTreeHelper.GetParent(child) is FrameworkElement frameworkElement)
		{
			frameworkElement.TemplateChild = null;
			frameworkElement.InvalidateMeasure();
		}
		container.TemplateChild = child;
	}

	private static void CheckForCircularReferencesInTemplateTree(DependencyObject container, FrameworkTemplate frameworkTemplate)
	{
		DependencyObject dependencyObject = container;
		DependencyObject dependencyObject2 = null;
		while (dependencyObject != null)
		{
			Helper.DowncastToFEorFCE(dependencyObject, out var fe, out var fce, throwIfNeither: false);
			bool flag = fe != null;
			dependencyObject2 = ((!flag) ? fce.TemplatedParent : fe.TemplatedParent);
			if (dependencyObject != container && dependencyObject2 != null && frameworkTemplate != null && flag && fe.TemplateInternal == frameworkTemplate && dependencyObject.GetType() == container.GetType())
			{
				string p = (flag ? fe.Name : fce.Name);
				throw new InvalidOperationException(SR.Format(SR.TemplateCircularReferenceFound, p, dependencyObject.GetType()));
			}
			dependencyObject = ((dependencyObject is ContentPresenter) ? null : dependencyObject2);
		}
	}

	internal static void ClearGeneratedSubTree(HybridDictionary[] instanceData, FrameworkElement feContainer, FrameworkContentElement fceContainer, FrameworkTemplate oldFrameworkTemplate)
	{
		List<DependencyObject> value;
		if (feContainer != null)
		{
			value = TemplatedFeChildrenField.GetValue(feContainer);
			TemplatedFeChildrenField.ClearValue(feContainer);
		}
		else
		{
			value = TemplatedFeChildrenField.GetValue(fceContainer);
			TemplatedFeChildrenField.ClearValue(fceContainer);
		}
		DependencyObject dependencyObject = null;
		if (value != null)
		{
			dependencyObject = value[0];
			if (oldFrameworkTemplate != null)
			{
				ClearTemplateChain(instanceData, feContainer, fceContainer, value, oldFrameworkTemplate);
			}
		}
		dependencyObject?.ClearValue(NameScope.NameScopeProperty);
		DetachGeneratedSubTree(feContainer, fceContainer);
	}

	private static void DetachGeneratedSubTree(FrameworkElement feContainer, FrameworkContentElement fceContainer)
	{
		if (feContainer != null)
		{
			feContainer.TemplateChild = null;
			feContainer.HasTemplateGeneratedSubTree = false;
		}
		else
		{
			fceContainer.HasTemplateGeneratedSubTree = false;
		}
	}

	private static void ClearTemplateChain(HybridDictionary[] instanceData, FrameworkElement feContainer, FrameworkContentElement fceContainer, List<DependencyObject> templateChain, FrameworkTemplate oldFrameworkTemplate)
	{
		FrameworkObject frameworkObject = new FrameworkObject(feContainer, fceContainer);
		HybridDictionary instanceValues = ((instanceData != null) ? instanceData[0] : null);
		int[] array = new int[templateChain.Count];
		for (int i = 0; i < templateChain.Count; i++)
		{
			SpecialDowncastToFEorFCE(templateChain[i], out var fe, out var fce, throwIfNeither: true);
			if (fe != null)
			{
				array[i] = fe.TemplateChildIndex;
				fe._templatedParent = null;
				fe.TemplateChildIndex = -1;
			}
			else if (fce != null)
			{
				array[i] = fce.TemplateChildIndex;
				fce._templatedParent = null;
				fce.TemplateChildIndex = -1;
			}
		}
		for (int j = 0; j < templateChain.Count; j++)
		{
			DependencyObject dependencyObject = templateChain[j];
			FrameworkObject child = new FrameworkObject(dependencyObject);
			int childIndex = array[j];
			ProcessInstanceValuesForChild(feContainer, dependencyObject, array[j], instanceValues, apply: false, ref oldFrameworkTemplate.ChildRecordFromChildIndex);
			InvalidatePropertiesOnTemplateNode(frameworkObject.DO, child, array[j], ref oldFrameworkTemplate.ChildRecordFromChildIndex, isDetach: true, oldFrameworkTemplate.VisualTree);
			if (!child.StoresParentTemplateValues)
			{
				continue;
			}
			HybridDictionary value = ParentTemplateValuesField.GetValue(dependencyObject);
			ParentTemplateValuesField.ClearValue(dependencyObject);
			child.StoresParentTemplateValues = false;
			foreach (DictionaryEntry item in value)
			{
				DependencyProperty dp = (DependencyProperty)item.Key;
				if (item.Value is MarkupExtension)
				{
					ProcessInstanceValue(dependencyObject, childIndex, instanceValues, dp, -1, apply: false);
				}
				dependencyObject.InvalidateProperty(dp);
			}
		}
	}

	internal static void SpecialDowncastToFEorFCE(DependencyObject d, out FrameworkElement fe, out FrameworkContentElement fce, bool throwIfNeither)
	{
		if (d is FrameworkElement frameworkElement)
		{
			fe = frameworkElement;
			fce = null;
			return;
		}
		if (d is FrameworkContentElement frameworkContentElement)
		{
			fe = null;
			fce = frameworkContentElement;
			return;
		}
		if (throwIfNeither && !(d is Visual3D))
		{
			throw new InvalidOperationException(SR.Format(SR.MustBeFrameworkDerived, d.GetType()));
		}
		fe = null;
		fce = null;
	}

	internal static FrameworkElementFactory FindFEF(FrameworkElementFactory root, int childIndex)
	{
		if (root._childIndex == childIndex)
		{
			return root;
		}
		FrameworkElementFactory frameworkElementFactory = root.FirstChild;
		FrameworkElementFactory frameworkElementFactory2 = null;
		while (frameworkElementFactory != null)
		{
			frameworkElementFactory2 = FindFEF(frameworkElementFactory, childIndex);
			if (frameworkElementFactory2 != null)
			{
				return frameworkElementFactory2;
			}
			frameworkElementFactory = frameworkElementFactory.NextSibling;
		}
		return null;
	}

	private static void ExecuteEventTriggerActionsOnContainer(object sender, RoutedEventArgs e)
	{
		Helper.DowncastToFEorFCE((DependencyObject)sender, out var fe, out var fce, throwIfNeither: false);
		FrameworkTemplate frameworkTemplate = null;
		Style style;
		Style themeStyle;
		if (fe != null)
		{
			style = fe.Style;
			themeStyle = fe.ThemeStyle;
			frameworkTemplate = fe.TemplateInternal;
		}
		else
		{
			style = fce.Style;
			themeStyle = fce.ThemeStyle;
		}
		if (style != null && style.EventHandlersStore != null)
		{
			InvokeEventTriggerActions(fe, fce, style, null, 0, e.RoutedEvent);
		}
		if (themeStyle != null && themeStyle.EventHandlersStore != null)
		{
			InvokeEventTriggerActions(fe, fce, themeStyle, null, 0, e.RoutedEvent);
		}
		if (frameworkTemplate != null && frameworkTemplate.EventHandlersStore != null)
		{
			InvokeEventTriggerActions(fe, fce, null, frameworkTemplate, 0, e.RoutedEvent);
		}
	}

	private static void ExecuteEventTriggerActionsOnChild(object sender, RoutedEventArgs e)
	{
		Helper.DowncastToFEorFCE((DependencyObject)sender, out var fe, out var fce, throwIfNeither: false);
		DependencyObject templatedParent;
		int templateChildIndex;
		if (fe != null)
		{
			templatedParent = fe.TemplatedParent;
			templateChildIndex = fe.TemplateChildIndex;
		}
		else
		{
			templatedParent = fce.TemplatedParent;
			templateChildIndex = fce.TemplateChildIndex;
		}
		if (templatedParent != null)
		{
			Helper.DowncastToFEorFCE(templatedParent, out var fe2, out var fce2, throwIfNeither: false);
			FrameworkTemplate frameworkTemplate = null;
			frameworkTemplate = fe2.TemplateInternal;
			InvokeEventTriggerActions(fe2, fce2, null, frameworkTemplate, templateChildIndex, e.RoutedEvent);
		}
	}

	private static void InvokeEventTriggerActions(FrameworkElement fe, FrameworkContentElement fce, Style ownerStyle, FrameworkTemplate frameworkTemplate, int childIndex, RoutedEvent Event)
	{
		List<TriggerAction> list = ((ownerStyle == null) ? ((frameworkTemplate._triggerActions != null) ? (frameworkTemplate._triggerActions[Event] as List<TriggerAction>) : null) : ((ownerStyle._triggerActions != null) ? (ownerStyle._triggerActions[Event] as List<TriggerAction>) : null));
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			TriggerAction triggerAction = list[i];
			int triggerChildIndex = ((EventTrigger)triggerAction.ContainingTrigger).TriggerChildIndex;
			if (childIndex == triggerChildIndex)
			{
				triggerAction.Invoke(fe, fce, ownerStyle, frameworkTemplate, Storyboard.Layers.StyleOrTemplateEventTrigger);
			}
		}
	}

	internal static object GetChildValue(UncommonField<HybridDictionary[]> dataField, DependencyObject container, int childIndex, FrameworkObject child, DependencyProperty dp, ref FrugalStructList<ChildRecord> childRecordFromChildIndex, ref EffectiveValueEntry entry, out ValueLookupType sourceType, FrameworkElementFactory templateRoot)
	{
		object result = DependencyProperty.UnsetValue;
		sourceType = ValueLookupType.Simple;
		if (0 <= childIndex && childIndex < childRecordFromChildIndex.Count)
		{
			ChildRecord childRecord = childRecordFromChildIndex[childIndex];
			int num = childRecord.ValueLookupListFromProperty.Search(dp.GlobalIndex);
			if (num >= 0 && childRecord.ValueLookupListFromProperty.Entries[num].Value.Count > 0)
			{
				result = GetChildValueHelper(dataField, ref childRecord.ValueLookupListFromProperty.Entries[num].Value, dp, container, child, childIndex, styleLookup: true, ref entry, out sourceType, templateRoot);
			}
		}
		return result;
	}

	private static object GetChildValueHelper(UncommonField<HybridDictionary[]> dataField, ref ItemStructList<ChildValueLookup> valueLookupList, DependencyProperty dp, DependencyObject container, FrameworkObject child, int childIndex, bool styleLookup, ref EffectiveValueEntry entry, out ValueLookupType sourceType, FrameworkElementFactory templateRoot)
	{
		object obj = DependencyProperty.UnsetValue;
		sourceType = ValueLookupType.Simple;
		for (int num = valueLookupList.Count - 1; num >= 0; num--)
		{
			sourceType = valueLookupList.List[num].LookupType;
			switch (valueLookupList.List[num].LookupType)
			{
			case ValueLookupType.Simple:
				obj = valueLookupList.List[num].Value;
				break;
			case ValueLookupType.Trigger:
			case ValueLookupType.PropertyTriggerResource:
			case ValueLookupType.DataTrigger:
			case ValueLookupType.DataTriggerResource:
			{
				bool flag = true;
				if (valueLookupList.List[num].Conditions != null)
				{
					int num2 = 0;
					while (flag && num2 < valueLookupList.List[num].Conditions.Length)
					{
						switch (valueLookupList.List[num].LookupType)
						{
						case ValueLookupType.Trigger:
						case ValueLookupType.PropertyTriggerResource:
						{
							int sourceChildIndex = valueLookupList.List[num].Conditions[num2].SourceChildIndex;
							DependencyObject dependencyObject = ((sourceChildIndex != 0) ? GetChild(container, sourceChildIndex) : container);
							DependencyProperty property3 = valueLookupList.List[num].Conditions[num2].Property;
							object dataTriggerValue;
							if (dependencyObject != null)
							{
								dataTriggerValue = dependencyObject.GetValue(property3);
							}
							else
							{
								Type forType = ((templateRoot == null) ? (container as FrameworkElement).TemplateInternal.ChildTypeFromChildIndex[sourceChildIndex] : FindFEF(templateRoot, sourceChildIndex).Type);
								dataTriggerValue = property3.GetDefaultValue(forType);
							}
							flag = valueLookupList.List[num].Conditions[num2].Match(dataTriggerValue);
							break;
						}
						default:
						{
							object dataTriggerValue = GetDataTriggerValue(dataField, container, valueLookupList.List[num].Conditions[num2].Binding);
							flag = valueLookupList.List[num].Conditions[num2].ConvertAndMatch(dataTriggerValue);
							break;
						}
						}
						num2++;
					}
				}
				if (flag)
				{
					if (valueLookupList.List[num].LookupType == ValueLookupType.PropertyTriggerResource || valueLookupList.List[num].LookupType == ValueLookupType.DataTriggerResource)
					{
						obj = FrameworkElement.FindResourceInternal(child.FE, child.FCE, dp, valueLookupList.List[num].Value, null, allowDeferredResourceReference: true, mustReturnDeferredResourceReference: false, null, isImplicitStyleLookup: false, out var _);
						SealIfSealable(obj);
					}
					else
					{
						obj = valueLookupList.List[num].Value;
					}
				}
				break;
			}
			case ValueLookupType.TemplateBinding:
			{
				TemplateBindingExtension templateBindingExtension = (TemplateBindingExtension)valueLookupList.List[num].Value;
				DependencyProperty property = templateBindingExtension.Property;
				obj = container.GetValue(property);
				if (templateBindingExtension.Converter != null)
				{
					DependencyProperty property2 = valueLookupList.List[num].Property;
					CultureInfo compatibleCulture = child.Language.GetCompatibleCulture();
					obj = templateBindingExtension.Converter.Convert(obj, property2.PropertyType, templateBindingExtension.ConverterParameter, compatibleCulture);
				}
				if (obj != DependencyProperty.UnsetValue && !dp.IsValidValue(obj))
				{
					obj = DependencyProperty.UnsetValue;
				}
				break;
			}
			case ValueLookupType.Resource:
			{
				obj = FrameworkElement.FindResourceInternal(child.FE, child.FCE, dp, valueLookupList.List[num].Value, null, allowDeferredResourceReference: true, mustReturnDeferredResourceReference: false, null, isImplicitStyleLookup: false, out var _);
				SealIfSealable(obj);
				break;
			}
			}
			if (obj != DependencyProperty.UnsetValue)
			{
				entry.Value = obj;
				ValueLookupType lookupType = valueLookupList.List[num].LookupType;
				if ((uint)lookupType <= 1u || lookupType == ValueLookupType.DataTrigger)
				{
					if (obj is MarkupExtension)
					{
						obj = GetInstanceValue(dataField, container, child.FE, child.FCE, childIndex, valueLookupList.List[num].Property, num, ref entry);
					}
					else if (obj is Freezable { IsFrozen: false })
					{
						obj = GetInstanceValue(dataField, container, child.FE, child.FCE, childIndex, valueLookupList.List[num].Property, num, ref entry);
					}
				}
			}
			if (obj != DependencyProperty.UnsetValue)
			{
				break;
			}
		}
		return obj;
	}

	internal static object GetDataTriggerValue(UncommonField<HybridDictionary[]> dataField, DependencyObject container, BindingBase binding)
	{
		dataField.GetValue(container);
		HybridDictionary hybridDictionary = EnsureInstanceData(dataField, container, InstanceStyleData.InstanceValues);
		BindingExpressionBase bindingExpressionBase = (BindingExpressionBase)hybridDictionary[binding];
		if (bindingExpressionBase == null)
		{
			bindingExpressionBase = (BindingExpressionBase)(hybridDictionary[binding] = BindingExpressionBase.CreateUntargetedBindingExpression(container, binding));
			if (dataField == StyleDataField)
			{
				bindingExpressionBase.ValueChanged += OnBindingValueInStyleChanged;
			}
			else if (dataField == TemplateDataField)
			{
				bindingExpressionBase.ResolveNamesInTemplate = true;
				bindingExpressionBase.ValueChanged += OnBindingValueInTemplateChanged;
			}
			else
			{
				bindingExpressionBase.ValueChanged += OnBindingValueInThemeStyleChanged;
			}
			bindingExpressionBase.Attach(container);
		}
		return bindingExpressionBase.Value;
	}

	internal static object GetInstanceValue(UncommonField<HybridDictionary[]> dataField, DependencyObject container, FrameworkElement feChild, FrameworkContentElement fceChild, int childIndex, DependencyProperty dp, int i, ref EffectiveValueEntry entry)
	{
		object value = entry.Value;
		DependencyObject dependencyObject = null;
		Helper.DowncastToFEorFCE(container, out var _, out var _, throwIfNeither: true);
		HybridDictionary[] array = dataField?.GetValue(container);
		HybridDictionary hybridDictionary = ((array != null) ? array[0] : null);
		InstanceValueKey key = new InstanceValueKey(childIndex, dp.GlobalIndex, i);
		object obj = hybridDictionary?[key];
		bool flag = feChild?.IsRequestingExpression ?? fceChild.IsRequestingExpression;
		if (obj == null)
		{
			obj = NotYetApplied;
		}
		Expression expression = obj as Expression;
		if (expression != null && expression.HasBeenDetached)
		{
			obj = NotYetApplied;
		}
		if (obj == NotYetApplied)
		{
			dependencyObject = feChild;
			if (dependencyObject == null)
			{
				dependencyObject = fceChild;
			}
			if (value is MarkupExtension markupExtension)
			{
				if (flag && !(feChild?.IsInitialized ?? fceChild.IsInitialized))
				{
					return DependencyProperty.UnsetValue;
				}
				ProvideValueServiceProvider provideValueServiceProvider = new ProvideValueServiceProvider();
				provideValueServiceProvider.SetData(dependencyObject, dp);
				obj = markupExtension.ProvideValue(provideValueServiceProvider);
			}
			else if (value is Freezable freezable)
			{
				obj = freezable.Clone();
				dependencyObject.ProvideSelfAsInheritanceContext(obj, dp);
			}
			hybridDictionary[key] = obj;
			if (obj != DependencyProperty.UnsetValue)
			{
				expression = obj as Expression;
				expression?.OnAttach(dependencyObject, dp);
			}
		}
		if (expression != null)
		{
			if (!flag)
			{
				if (dependencyObject == null)
				{
					dependencyObject = feChild;
					if (dependencyObject == null)
					{
						dependencyObject = fceChild;
					}
				}
				entry.ResetValue(DependencyObject.ExpressionInAlternativeStore, hasExpressionMarker: true);
				entry.SetExpressionValue(expression.GetValue(dependencyObject, dp), DependencyObject.ExpressionInAlternativeStore);
			}
			else
			{
				entry.Value = obj;
			}
		}
		else
		{
			entry.Value = obj;
		}
		return obj;
	}

	internal static bool ShouldGetValueFromStyle(DependencyProperty dp)
	{
		return dp != FrameworkElement.StyleProperty;
	}

	internal static bool ShouldGetValueFromThemeStyle(DependencyProperty dp)
	{
		if (dp != FrameworkElement.StyleProperty && dp != FrameworkElement.DefaultStyleKeyProperty)
		{
			return dp != FrameworkElement.OverridesDefaultStyleProperty;
		}
		return false;
	}

	internal static bool ShouldGetValueFromTemplate(DependencyProperty dp)
	{
		if (dp != FrameworkElement.StyleProperty && dp != FrameworkElement.DefaultStyleKeyProperty && dp != FrameworkElement.OverridesDefaultStyleProperty && dp != Control.TemplateProperty)
		{
			return dp != ContentPresenter.TemplateProperty;
		}
		return false;
	}

	internal static void DoStyleInvalidations(FrameworkElement fe, FrameworkContentElement fce, Style oldStyle, Style newStyle)
	{
		if (oldStyle == newStyle)
		{
			return;
		}
		object obj = ((fe != null) ? ((object)fe) : ((object)fce));
		UpdateLoadedFlag((DependencyObject)obj, oldStyle, newStyle);
		UpdateInstanceData(StyleDataField, fe, fce, oldStyle, newStyle, null, null, (InternalFlags)0u);
		if (newStyle != null && newStyle.HasResourceReferences)
		{
			if (fe != null)
			{
				fe.HasResourceReference = true;
			}
			else
			{
				fce.HasResourceReference = true;
			}
		}
		FrugalStructList<ContainerDependent> oldContainerDependents = oldStyle?.ContainerDependents ?? EmptyContainerDependents;
		FrugalStructList<ContainerDependent> newContainerDependents = newStyle?.ContainerDependents ?? EmptyContainerDependents;
		FrugalStructList<ContainerDependent> exclusionContainerDependents = default(FrugalStructList<ContainerDependent>);
		InvalidateContainerDependents((DependencyObject)obj, ref exclusionContainerDependents, ref oldContainerDependents, ref newContainerDependents);
		DoStyleResourcesInvalidations((DependencyObject)obj, fe, fce, oldStyle, newStyle);
		if (fe != null)
		{
			fe.OnStyleChanged(oldStyle, newStyle);
		}
		else
		{
			fce.OnStyleChanged(oldStyle, newStyle);
		}
	}

	internal static void DoThemeStyleInvalidations(FrameworkElement fe, FrameworkContentElement fce, Style oldThemeStyle, Style newThemeStyle, Style style)
	{
		if (oldThemeStyle == newThemeStyle || newThemeStyle == style)
		{
			return;
		}
		object obj = ((fe != null) ? ((object)fe) : ((object)fce));
		UpdateLoadedFlag((DependencyObject)obj, oldThemeStyle, newThemeStyle);
		UpdateInstanceData(ThemeStyleDataField, fe, fce, oldThemeStyle, newThemeStyle, null, null, (InternalFlags)0u);
		if (newThemeStyle != null && newThemeStyle.HasResourceReferences)
		{
			if (fe != null)
			{
				fe.HasResourceReference = true;
			}
			else
			{
				fce.HasResourceReference = true;
			}
		}
		FrugalStructList<ContainerDependent> oldContainerDependents = oldThemeStyle?.ContainerDependents ?? EmptyContainerDependents;
		FrugalStructList<ContainerDependent> newContainerDependents = newThemeStyle?.ContainerDependents ?? EmptyContainerDependents;
		FrugalStructList<ContainerDependent> exclusionContainerDependents = style?.ContainerDependents ?? default(FrugalStructList<ContainerDependent>);
		InvalidateContainerDependents((DependencyObject)obj, ref exclusionContainerDependents, ref oldContainerDependents, ref newContainerDependents);
		DoStyleResourcesInvalidations((DependencyObject)obj, fe, fce, oldThemeStyle, newThemeStyle);
	}

	internal static void DoTemplateInvalidations(FrameworkElement feContainer, FrameworkTemplate oldFrameworkTemplate)
	{
		FrameworkTemplate frameworkTemplate = null;
		HybridDictionary[] value = TemplateDataField.GetValue(feContainer);
		frameworkTemplate = feContainer.TemplateInternal;
		object obj = frameworkTemplate;
		bool flag = frameworkTemplate?.HasResourceReferences ?? false;
		UpdateLoadedFlag(feContainer, oldFrameworkTemplate, frameworkTemplate);
		if (oldFrameworkTemplate != obj)
		{
			UpdateInstanceData(TemplateDataField, feContainer, null, null, null, oldFrameworkTemplate, frameworkTemplate, InternalFlags.HasTemplateGeneratedSubTree);
			if (obj != null && flag)
			{
				feContainer.HasResourceReference = true;
			}
			UpdateLoadedFlag(feContainer, oldFrameworkTemplate, frameworkTemplate);
			_ = oldFrameworkTemplate?.VisualTree;
			_ = frameworkTemplate?.VisualTree;
			_ = oldFrameworkTemplate?.CanBuildVisualTree;
			bool hasTemplateGeneratedSubTree = feContainer.HasTemplateGeneratedSubTree;
			FrugalStructList<ContainerDependent> oldContainerDependents = oldFrameworkTemplate?.ContainerDependents ?? EmptyContainerDependents;
			FrugalStructList<ContainerDependent> newContainerDependents = frameworkTemplate?.ContainerDependents ?? EmptyContainerDependents;
			if (hasTemplateGeneratedSubTree)
			{
				ClearGeneratedSubTree(value, feContainer, null, oldFrameworkTemplate);
			}
			FrugalStructList<ContainerDependent> exclusionContainerDependents = default(FrugalStructList<ContainerDependent>);
			InvalidateContainerDependents(feContainer, ref exclusionContainerDependents, ref oldContainerDependents, ref newContainerDependents);
			DoTemplateResourcesInvalidations(feContainer, feContainer, null, oldFrameworkTemplate, obj);
			feContainer.OnTemplateChangedInternal(oldFrameworkTemplate, frameworkTemplate);
		}
		else if (frameworkTemplate != null && feContainer.HasTemplateGeneratedSubTree && frameworkTemplate.VisualTree == null && !frameworkTemplate.HasXamlNodeContent)
		{
			ClearGeneratedSubTree(value, feContainer, null, oldFrameworkTemplate);
			feContainer.InvalidateMeasure();
		}
	}

	internal static void DoStyleResourcesInvalidations(DependencyObject container, FrameworkElement fe, FrameworkContentElement fce, Style oldStyle, Style newStyle)
	{
		if (!(fe?.AncestorChangeInProgress ?? fce.AncestorChangeInProgress))
		{
			List<ResourceDictionary> resourceDictionariesFromStyle = GetResourceDictionariesFromStyle(oldStyle);
			List<ResourceDictionary> resourceDictionariesFromStyle2 = GetResourceDictionariesFromStyle(newStyle);
			if ((resourceDictionariesFromStyle != null && resourceDictionariesFromStyle.Count > 0) || (resourceDictionariesFromStyle2 != null && resourceDictionariesFromStyle2.Count > 0))
			{
				SetShouldLookupImplicitStyles(new FrameworkObject(fe, fce), resourceDictionariesFromStyle2);
				TreeWalkHelper.InvalidateOnResourcesChange(fe, fce, new ResourcesChangeInfo(resourceDictionariesFromStyle, resourceDictionariesFromStyle2, isStyleResourcesChange: true, isTemplateResourcesChange: false, container));
			}
		}
	}

	internal static void DoTemplateResourcesInvalidations(DependencyObject container, FrameworkElement fe, FrameworkContentElement fce, object oldTemplate, object newTemplate)
	{
		if (!(fe?.AncestorChangeInProgress ?? fce.AncestorChangeInProgress))
		{
			List<ResourceDictionary> resourceDictionaryFromTemplate = GetResourceDictionaryFromTemplate(oldTemplate);
			List<ResourceDictionary> resourceDictionaryFromTemplate2 = GetResourceDictionaryFromTemplate(newTemplate);
			if (resourceDictionaryFromTemplate != resourceDictionaryFromTemplate2)
			{
				SetShouldLookupImplicitStyles(new FrameworkObject(fe, fce), resourceDictionaryFromTemplate2);
				TreeWalkHelper.InvalidateOnResourcesChange(fe, fce, new ResourcesChangeInfo(resourceDictionaryFromTemplate, resourceDictionaryFromTemplate2, isStyleResourcesChange: false, isTemplateResourcesChange: true, container));
			}
		}
	}

	private static void SetShouldLookupImplicitStyles(FrameworkObject fo, List<ResourceDictionary> dictionaries)
	{
		if (dictionaries == null || dictionaries.Count <= 0 || fo.ShouldLookupImplicitStyles)
		{
			return;
		}
		for (int i = 0; i < dictionaries.Count; i++)
		{
			if (dictionaries[i].HasImplicitStyles)
			{
				fo.ShouldLookupImplicitStyles = true;
				break;
			}
		}
	}

	private static List<ResourceDictionary> GetResourceDictionariesFromStyle(Style style)
	{
		List<ResourceDictionary> list = null;
		while (style != null)
		{
			if (style._resources != null)
			{
				if (list == null)
				{
					list = new List<ResourceDictionary>(1);
				}
				list.Add(style._resources);
			}
			style = style.BasedOn;
		}
		return list;
	}

	private static List<ResourceDictionary> GetResourceDictionaryFromTemplate(object template)
	{
		ResourceDictionary resourceDictionary = null;
		if (template is FrameworkTemplate)
		{
			resourceDictionary = ((FrameworkTemplate)template)._resources;
		}
		if (resourceDictionary != null)
		{
			return new List<ResourceDictionary>(1) { resourceDictionary };
		}
		return null;
	}

	internal static void UpdateLoadedFlag(DependencyObject d, Style oldStyle, Style newStyle)
	{
		Invariant.Assert(oldStyle != null || newStyle != null);
		if ((oldStyle == null || !oldStyle.HasLoadedChangeHandler) && newStyle != null && newStyle.HasLoadedChangeHandler)
		{
			BroadcastEventHelper.AddHasLoadedChangeHandlerFlagInAncestry(d);
		}
		else if (oldStyle != null && oldStyle.HasLoadedChangeHandler && (newStyle == null || !newStyle.HasLoadedChangeHandler))
		{
			BroadcastEventHelper.RemoveHasLoadedChangeHandlerFlagInAncestry(d);
		}
	}

	internal static void UpdateLoadedFlag(DependencyObject d, FrameworkTemplate oldFrameworkTemplate, FrameworkTemplate newFrameworkTemplate)
	{
		if ((oldFrameworkTemplate == null || !oldFrameworkTemplate.HasLoadedChangeHandler) && newFrameworkTemplate != null && newFrameworkTemplate.HasLoadedChangeHandler)
		{
			BroadcastEventHelper.AddHasLoadedChangeHandlerFlagInAncestry(d);
		}
		else if (oldFrameworkTemplate != null && oldFrameworkTemplate.HasLoadedChangeHandler && (newFrameworkTemplate == null || !newFrameworkTemplate.HasLoadedChangeHandler))
		{
			BroadcastEventHelper.RemoveHasLoadedChangeHandlerFlagInAncestry(d);
		}
	}

	internal static void InvalidateContainerDependents(DependencyObject container, ref FrugalStructList<ContainerDependent> exclusionContainerDependents, ref FrugalStructList<ContainerDependent> oldContainerDependents, ref FrugalStructList<ContainerDependent> newContainerDependents)
	{
		int count = oldContainerDependents.Count;
		for (int i = 0; i < count; i++)
		{
			DependencyProperty property = oldContainerDependents[i].Property;
			if (!IsSetOnContainer(property, ref exclusionContainerDependents, alsoFromTriggers: false))
			{
				container.InvalidateProperty(property);
			}
		}
		count = newContainerDependents.Count;
		if (count <= 0)
		{
			return;
		}
		FrameworkObject fo = new FrameworkObject(container);
		for (int j = 0; j < count; j++)
		{
			DependencyProperty property2 = newContainerDependents[j].Property;
			if (!IsSetOnContainer(property2, ref exclusionContainerDependents, alsoFromTriggers: false) && !IsSetOnContainer(property2, ref oldContainerDependents, alsoFromTriggers: false))
			{
				ApplyStyleOrTemplateValue(fo, property2);
			}
		}
	}

	internal static void ApplyTemplatedParentValue(DependencyObject container, FrameworkObject child, int childIndex, ref FrugalStructList<ChildRecord> childRecordFromChildIndex, DependencyProperty dp, FrameworkElementFactory templateRoot)
	{
		EffectiveValueEntry entry = new EffectiveValueEntry(dp);
		entry.Value = DependencyProperty.UnsetValue;
		if (GetValueFromTemplatedParent(container, childIndex, child, dp, ref childRecordFromChildIndex, templateRoot, ref entry))
		{
			DependencyObject dO = child.DO;
			dO.UpdateEffectiveValue(dO.LookupEntry(dp.GlobalIndex), dp, dp.GetMetadata(dO.DependencyObjectType), default(EffectiveValueEntry), ref entry, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.Unknown);
		}
	}

	internal static bool IsValueDynamic(DependencyObject container, int childIndex, DependencyProperty dp)
	{
		bool result = false;
		FrameworkTemplate templateInternal = new FrameworkObject(container).TemplateInternal;
		if (templateInternal != null)
		{
			FrugalStructList<ChildRecord> childRecordFromChildIndex = templateInternal.ChildRecordFromChildIndex;
			if (0 <= childIndex && childIndex < childRecordFromChildIndex.Count)
			{
				ChildRecord childRecord = childRecordFromChildIndex[childIndex];
				int num = childRecord.ValueLookupListFromProperty.Search(dp.GlobalIndex);
				if (num >= 0 && childRecord.ValueLookupListFromProperty.Entries[num].Value.Count > 0)
				{
					ChildValueLookup childValueLookup = childRecord.ValueLookupListFromProperty.Entries[num].Value.List[0];
					result = childValueLookup.LookupType == ValueLookupType.Resource || childValueLookup.LookupType == ValueLookupType.TemplateBinding || (childValueLookup.LookupType == ValueLookupType.Simple && childValueLookup.Value is BindingBase);
				}
			}
		}
		return result;
	}

	internal static bool GetValueFromTemplatedParent(DependencyObject container, int childIndex, FrameworkObject child, DependencyProperty dp, ref FrugalStructList<ChildRecord> childRecordFromChildIndex, FrameworkElementFactory templateRoot, ref EffectiveValueEntry entry)
	{
		ValueLookupType sourceType = ValueLookupType.Simple;
		object childValue = GetChildValue(TemplateDataField, container, childIndex, child, dp, ref childRecordFromChildIndex, ref entry, out sourceType, templateRoot);
		if (childValue != DependencyProperty.UnsetValue)
		{
			if (sourceType == ValueLookupType.Trigger || sourceType == ValueLookupType.PropertyTriggerResource || sourceType == ValueLookupType.DataTrigger || sourceType == ValueLookupType.DataTriggerResource)
			{
				entry.BaseValueSourceInternal = BaseValueSourceInternal.ParentTemplateTrigger;
			}
			else
			{
				entry.BaseValueSourceInternal = BaseValueSourceInternal.ParentTemplate;
			}
			return true;
		}
		if (child.StoresParentTemplateValues)
		{
			HybridDictionary value = ParentTemplateValuesField.GetValue(child.DO);
			if (value.Contains(dp))
			{
				entry.BaseValueSourceInternal = BaseValueSourceInternal.ParentTemplate;
				childValue = (entry.Value = value[dp]);
				if (childValue is MarkupExtension)
				{
					GetInstanceValue(TemplateDataField, container, child.FE, child.FCE, childIndex, dp, -1, ref entry);
				}
				return true;
			}
		}
		return false;
	}

	internal static void ApplyStyleOrTemplateValue(FrameworkObject fo, DependencyProperty dp)
	{
		EffectiveValueEntry entry = new EffectiveValueEntry(dp);
		entry.Value = DependencyProperty.UnsetValue;
		if (GetValueFromStyleOrTemplate(fo, dp, ref entry))
		{
			DependencyObject dO = fo.DO;
			dO.UpdateEffectiveValue(dO.LookupEntry(dp.GlobalIndex), dp, dp.GetMetadata(dO.DependencyObjectType), default(EffectiveValueEntry), ref entry, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.Unknown);
		}
	}

	internal static bool GetValueFromStyleOrTemplate(FrameworkObject fo, DependencyProperty dp, ref EffectiveValueEntry entry)
	{
		ValueLookupType sourceType = ValueLookupType.Simple;
		object obj = DependencyProperty.UnsetValue;
		EffectiveValueEntry entry2 = entry;
		Style style = fo.Style;
		if (style != null && ShouldGetValueFromStyle(dp))
		{
			object childValue = GetChildValue(StyleDataField, fo.DO, 0, fo, dp, ref style.ChildRecordFromChildIndex, ref entry2, out sourceType, null);
			if (childValue != DependencyProperty.UnsetValue)
			{
				if (sourceType == ValueLookupType.Trigger || sourceType == ValueLookupType.PropertyTriggerResource || sourceType == ValueLookupType.DataTrigger || sourceType == ValueLookupType.DataTriggerResource)
				{
					entry = entry2;
					entry.BaseValueSourceInternal = BaseValueSourceInternal.StyleTrigger;
					return true;
				}
				obj = childValue;
			}
		}
		if (ShouldGetValueFromTemplate(dp))
		{
			FrameworkTemplate templateInternal = fo.TemplateInternal;
			if (templateInternal != null)
			{
				object childValue = GetChildValue(TemplateDataField, fo.DO, 0, fo, dp, ref templateInternal.ChildRecordFromChildIndex, ref entry, out sourceType, templateInternal.VisualTree);
				if (childValue != DependencyProperty.UnsetValue)
				{
					entry.BaseValueSourceInternal = BaseValueSourceInternal.TemplateTrigger;
					return true;
				}
			}
		}
		if (obj != DependencyProperty.UnsetValue)
		{
			entry = entry2;
			entry.BaseValueSourceInternal = BaseValueSourceInternal.Style;
			return true;
		}
		if (ShouldGetValueFromThemeStyle(dp))
		{
			Style themeStyle = fo.ThemeStyle;
			if (themeStyle != null)
			{
				object childValue = GetChildValue(ThemeStyleDataField, fo.DO, 0, fo, dp, ref themeStyle.ChildRecordFromChildIndex, ref entry, out sourceType, null);
				if (childValue != DependencyProperty.UnsetValue)
				{
					if (sourceType == ValueLookupType.Trigger || sourceType == ValueLookupType.PropertyTriggerResource || sourceType == ValueLookupType.DataTrigger || sourceType == ValueLookupType.DataTriggerResource)
					{
						entry.BaseValueSourceInternal = BaseValueSourceInternal.ThemeStyleTrigger;
					}
					else
					{
						entry.BaseValueSourceInternal = BaseValueSourceInternal.ThemeStyle;
					}
					return true;
				}
			}
		}
		return false;
	}

	internal static void SortResourceDependents(ref FrugalStructList<ChildPropertyDependent> resourceDependents)
	{
		int count = resourceDependents.Count;
		for (int i = 1; i < count; i++)
		{
			ChildPropertyDependent value = resourceDependents[i];
			int childIndex = value.ChildIndex;
			int globalIndex = value.Property.GlobalIndex;
			int num = i - 1;
			while (num >= 0 && (childIndex < resourceDependents[num].ChildIndex || (childIndex == resourceDependents[num].ChildIndex && globalIndex < resourceDependents[num].Property.GlobalIndex)))
			{
				resourceDependents[num + 1] = resourceDependents[num];
				num--;
			}
			if (num < i - 1)
			{
				resourceDependents[num + 1] = value;
			}
		}
	}

	internal static void InvalidateResourceDependents(DependencyObject container, ResourcesChangeInfo info, ref FrugalStructList<ChildPropertyDependent> resourceDependents, bool invalidateVisualTreeToo)
	{
		List<DependencyObject> value = TemplatedFeChildrenField.GetValue(container);
		for (int i = 0; i < resourceDependents.Count; i++)
		{
			if (!info.Contains(resourceDependents[i].Name, isImplicitStyleKey: false))
			{
				continue;
			}
			DependencyObject dependencyObject = null;
			DependencyProperty property = resourceDependents[i].Property;
			int childIndex = resourceDependents[i].ChildIndex;
			if (childIndex == 0)
			{
				dependencyObject = container;
			}
			else if (invalidateVisualTreeToo)
			{
				dependencyObject = GetChild(value, childIndex);
				if (dependencyObject == null)
				{
					throw new InvalidOperationException(SR.ChildTemplateInstanceDoesNotExist);
				}
			}
			if (dependencyObject != null)
			{
				dependencyObject.InvalidateProperty(property);
				int globalIndex = property.GlobalIndex;
				while (++i < resourceDependents.Count && resourceDependents[i].ChildIndex == childIndex && resourceDependents[i].Property.GlobalIndex == globalIndex)
				{
				}
				i--;
			}
		}
	}

	internal static void InvalidateResourceDependentsForChild(DependencyObject container, DependencyObject child, int childIndex, ResourcesChangeInfo info, FrameworkTemplate parentTemplate)
	{
		FrugalStructList<ChildPropertyDependent> resourceDependents = parentTemplate.ResourceDependents;
		int count = resourceDependents.Count;
		for (int i = 0; i < count; i++)
		{
			if (resourceDependents[i].ChildIndex == childIndex && info.Contains(resourceDependents[i].Name, isImplicitStyleKey: false))
			{
				DependencyProperty property = resourceDependents[i].Property;
				child.InvalidateProperty(property);
				int globalIndex = property.GlobalIndex;
				while (++i < resourceDependents.Count && resourceDependents[i].ChildIndex == childIndex && resourceDependents[i].Property.GlobalIndex == globalIndex)
				{
				}
				i--;
			}
		}
	}

	internal static bool HasResourceDependentsForChild(int childIndex, ref FrugalStructList<ChildPropertyDependent> resourceDependents)
	{
		for (int i = 0; i < resourceDependents.Count; i++)
		{
			if (resourceDependents[i].ChildIndex == childIndex)
			{
				return true;
			}
		}
		return false;
	}

	internal static void InvalidatePropertiesOnTemplateNode(DependencyObject container, FrameworkObject child, int childIndex, ref FrugalStructList<ChildRecord> childRecordFromChildIndex, bool isDetach, FrameworkElementFactory templateRoot)
	{
		if (0 > childIndex || childIndex >= childRecordFromChildIndex.Count)
		{
			return;
		}
		ChildRecord childRecord = childRecordFromChildIndex[childIndex];
		int count = childRecord.ValueLookupListFromProperty.Count;
		if (count <= 0)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			DependencyProperty property = childRecord.ValueLookupListFromProperty.Entries[i].Value.List[0].Property;
			if (!isDetach)
			{
				ApplyTemplatedParentValue(container, child, childIndex, ref childRecordFromChildIndex, property, templateRoot);
			}
			else
			{
				if (property == FrameworkElement.StyleProperty)
				{
					continue;
				}
				bool flag = true;
				if (property.IsPotentiallyInherited)
				{
					PropertyMetadata metadata = property.GetMetadata(child.DO.DependencyObjectType);
					if (metadata != null && metadata.IsInherited)
					{
						flag = false;
					}
				}
				if (flag)
				{
					child.DO.InvalidateProperty(property);
				}
			}
		}
	}

	internal static bool IsSetOnContainer(DependencyProperty dp, ref FrugalStructList<ContainerDependent> containerDependents, bool alsoFromTriggers)
	{
		for (int i = 0; i < containerDependents.Count; i++)
		{
			if (dp == containerDependents[i].Property)
			{
				if (!alsoFromTriggers)
				{
					return !containerDependents[i].FromVisualTrigger;
				}
				return true;
			}
		}
		return false;
	}

	internal static void OnTriggerSourcePropertyInvalidated(Style ownerStyle, FrameworkTemplate frameworkTemplate, DependencyObject container, DependencyProperty dp, DependencyPropertyChangedEventArgs changedArgs, bool invalidateOnlyContainer, ref FrugalStructList<ItemStructMap<TriggerSourceRecord>> triggerSourceRecordFromChildIndex, ref FrugalMap propertyTriggersWithActions, int sourceChildIndex)
	{
		if (0 <= sourceChildIndex && sourceChildIndex < triggerSourceRecordFromChildIndex.Count)
		{
			ItemStructMap<TriggerSourceRecord> itemStructMap = triggerSourceRecordFromChildIndex[sourceChildIndex];
			int num = itemStructMap.Search(dp.GlobalIndex);
			if (num >= 0)
			{
				TriggerSourceRecord value = itemStructMap.Entries[num].Value;
				InvalidateDependents(ownerStyle, frameworkTemplate, container, dp, ref value.ChildPropertyDependents, invalidateOnlyContainer);
			}
		}
		object obj = propertyTriggersWithActions[dp.GlobalIndex];
		if (obj == DependencyProperty.UnsetValue)
		{
			return;
		}
		if (obj is TriggerBase triggerBase)
		{
			InvokePropertyTriggerActions(triggerBase, container, dp, changedArgs, sourceChildIndex, ownerStyle, frameworkTemplate);
			return;
		}
		List<TriggerBase> list = (List<TriggerBase>)obj;
		for (int i = 0; i < list.Count; i++)
		{
			InvokePropertyTriggerActions(list[i], container, dp, changedArgs, sourceChildIndex, ownerStyle, frameworkTemplate);
		}
	}

	private static void InvalidateDependents(Style ownerStyle, FrameworkTemplate frameworkTemplate, DependencyObject container, DependencyProperty dp, ref FrugalStructList<ChildPropertyDependent> dependents, bool invalidateOnlyContainer)
	{
		for (int i = 0; i < dependents.Count; i++)
		{
			DependencyObject dependencyObject = null;
			int childIndex = dependents[i].ChildIndex;
			if (childIndex == 0)
			{
				dependencyObject = container;
			}
			else if (!invalidateOnlyContainer)
			{
				List<DependencyObject> value = TemplatedFeChildrenField.GetValue(container);
				if (value != null && childIndex <= value.Count)
				{
					dependencyObject = GetChild(value, childIndex);
				}
			}
			DependencyProperty property = dependents[i].Property;
			if (dependencyObject != null && dependencyObject.GetValueSource(property, null, out var _) != BaseValueSourceInternal.Local)
			{
				dependencyObject.InvalidateProperty(property, preserveCurrentValue: true);
			}
		}
	}

	private static void InvokeDataTriggerActions(TriggerBase triggerBase, DependencyObject triggerContainer, BindingBase binding, BindingValueChangedEventArgs bindingChangedArgs, Style style, FrameworkTemplate frameworkTemplate, UncommonField<HybridDictionary[]> dataField)
	{
		bool oldState;
		bool newState;
		if (triggerBase is DataTrigger dataTrigger)
		{
			EvaluateOldNewStates(dataTrigger, triggerContainer, binding, bindingChangedArgs, dataField, style, frameworkTemplate, out oldState, out newState);
		}
		else
		{
			EvaluateOldNewStates((MultiDataTrigger)triggerBase, triggerContainer, binding, bindingChangedArgs, dataField, style, frameworkTemplate, out oldState, out newState);
		}
		InvokeEnterOrExitActions(triggerBase, oldState, newState, triggerContainer, style, frameworkTemplate);
	}

	private static void InvokePropertyTriggerActions(TriggerBase triggerBase, DependencyObject triggerContainer, DependencyProperty changedProperty, DependencyPropertyChangedEventArgs changedArgs, int sourceChildIndex, Style style, FrameworkTemplate frameworkTemplate)
	{
		bool oldState;
		bool newState;
		if (triggerBase is Trigger trigger)
		{
			EvaluateOldNewStates(trigger, triggerContainer, changedProperty, changedArgs, sourceChildIndex, style, frameworkTemplate, out oldState, out newState);
		}
		else
		{
			EvaluateOldNewStates((MultiTrigger)triggerBase, triggerContainer, changedProperty, changedArgs, sourceChildIndex, style, frameworkTemplate, out oldState, out newState);
		}
		InvokeEnterOrExitActions(triggerBase, oldState, newState, triggerContainer, style, frameworkTemplate);
	}

	private static void ExecuteOnApplyEnterExitActions(FrameworkElement fe, FrameworkContentElement fce, Style style, UncommonField<HybridDictionary[]> dataField)
	{
		if (style != null && (style.PropertyTriggersWithActions.Count != 0 || style.DataTriggersWithActions != null))
		{
			TriggerCollection triggers = style.Triggers;
			ExecuteOnApplyEnterExitActionsLoop((fe != null) ? ((DependencyObject)fe) : ((DependencyObject)fce), triggers, style, null, dataField);
		}
	}

	private static void ExecuteOnApplyEnterExitActions(FrameworkElement fe, FrameworkContentElement fce, FrameworkTemplate ft)
	{
		if (ft != null && (ft == null || ft.PropertyTriggersWithActions.Count != 0 || ft.DataTriggersWithActions != null))
		{
			TriggerCollection triggersInternal = ft.TriggersInternal;
			ExecuteOnApplyEnterExitActionsLoop((fe != null) ? ((DependencyObject)fe) : ((DependencyObject)fce), triggersInternal, null, ft, TemplateDataField);
		}
	}

	private static void ExecuteOnApplyEnterExitActionsLoop(DependencyObject triggerContainer, TriggerCollection triggers, Style style, FrameworkTemplate ft, UncommonField<HybridDictionary[]> dataField)
	{
		for (int i = 0; i < triggers.Count; i++)
		{
			TriggerBase triggerBase = triggers[i];
			if ((triggerBase.HasEnterActions || triggerBase.HasExitActions) && (triggerBase.ExecuteEnterActionsOnApply || triggerBase.ExecuteExitActionsOnApply) && NoSourceNameInTrigger(triggerBase))
			{
				bool currentState = triggerBase.GetCurrentState(triggerContainer, dataField);
				if (currentState && triggerBase.ExecuteEnterActionsOnApply)
				{
					InvokeActions(triggerBase.EnterActions, triggerBase, triggerContainer, style, ft);
				}
				else if (!currentState && triggerBase.ExecuteExitActionsOnApply)
				{
					InvokeActions(triggerBase.ExitActions, triggerBase, triggerContainer, style, ft);
				}
			}
		}
	}

	private static bool NoSourceNameInTrigger(TriggerBase triggerBase)
	{
		if (triggerBase is Trigger trigger)
		{
			if (trigger.SourceName == null)
			{
				return true;
			}
			return false;
		}
		if (triggerBase is MultiTrigger multiTrigger)
		{
			for (int i = 0; i < multiTrigger.Conditions.Count; i++)
			{
				if (multiTrigger.Conditions[i].SourceName != null)
				{
					return false;
				}
			}
			return true;
		}
		return true;
	}

	private static void InvokeEnterOrExitActions(TriggerBase triggerBase, bool oldState, bool newState, DependencyObject triggerContainer, Style style, FrameworkTemplate frameworkTemplate)
	{
		TriggerActionCollection actions = ((!oldState && newState) ? triggerBase.EnterActions : ((!oldState || newState) ? null : triggerBase.ExitActions));
		InvokeActions(actions, triggerBase, triggerContainer, style, frameworkTemplate);
	}

	private static void InvokeActions(TriggerActionCollection actions, TriggerBase triggerBase, DependencyObject triggerContainer, Style style, FrameworkTemplate frameworkTemplate)
	{
		if (actions != null)
		{
			if (CanInvokeActionsNow(triggerContainer, frameworkTemplate))
			{
				InvokeActions(triggerBase, triggerContainer, actions, style, frameworkTemplate);
			}
			else
			{
				DeferActions(triggerBase, triggerContainer, actions, style, frameworkTemplate);
			}
		}
	}

	private static bool CanInvokeActionsNow(DependencyObject container, FrameworkTemplate frameworkTemplate)
	{
		if (frameworkTemplate != null)
		{
			if (((FrameworkElement)container).HasTemplateGeneratedSubTree)
			{
				if (container is ContentPresenter { TemplateIsCurrent: false })
				{
					return false;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	private static void DeferActions(TriggerBase triggerBase, DependencyObject triggerContainer, TriggerActionCollection actions, Style style, FrameworkTemplate frameworkTemplate)
	{
		DeferredAction item = default(DeferredAction);
		item.TriggerBase = triggerBase;
		item.TriggerActionCollection = actions;
		ConditionalWeakTable<DependencyObject, List<DeferredAction>> conditionalWeakTable;
		if (frameworkTemplate != null)
		{
			conditionalWeakTable = frameworkTemplate.DeferredActions;
			if (conditionalWeakTable == null)
			{
				conditionalWeakTable = (frameworkTemplate.DeferredActions = new ConditionalWeakTable<DependencyObject, List<DeferredAction>>());
			}
		}
		else
		{
			conditionalWeakTable = null;
		}
		if (conditionalWeakTable != null)
		{
			if (!conditionalWeakTable.TryGetValue(triggerContainer, out var value))
			{
				value = new List<DeferredAction>();
				conditionalWeakTable.Add(triggerContainer, value);
			}
			value.Add(item);
		}
	}

	internal static void InvokeDeferredActions(DependencyObject triggerContainer, FrameworkTemplate frameworkTemplate)
	{
		if (frameworkTemplate != null && frameworkTemplate.DeferredActions != null && frameworkTemplate.DeferredActions.TryGetValue(triggerContainer, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				InvokeActions(value[i].TriggerBase, triggerContainer, value[i].TriggerActionCollection, null, frameworkTemplate);
			}
			frameworkTemplate.DeferredActions.Remove(triggerContainer);
		}
	}

	internal static void InvokeActions(TriggerBase triggerBase, DependencyObject triggerContainer, TriggerActionCollection actions, Style style, FrameworkTemplate frameworkTemplate)
	{
		for (int i = 0; i < actions.Count; i++)
		{
			actions[i].Invoke(triggerContainer as FrameworkElement, triggerContainer as FrameworkContentElement, style, frameworkTemplate, triggerBase.Layer);
		}
	}

	private static void EvaluateOldNewStates(Trigger trigger, DependencyObject triggerContainer, DependencyProperty changedProperty, DependencyPropertyChangedEventArgs changedArgs, int sourceChildIndex, Style style, FrameworkTemplate frameworkTemplate, out bool oldState, out bool newState)
	{
		int num = 0;
		if (trigger.SourceName != null)
		{
			num = QueryChildIndexFromChildName(trigger.SourceName, frameworkTemplate.ChildIndexFromChildName);
		}
		if (num == sourceChildIndex)
		{
			TriggerCondition[] triggerConditions = trigger.TriggerConditions;
			oldState = triggerConditions[0].Match(changedArgs.OldValue);
			newState = triggerConditions[0].Match(changedArgs.NewValue);
		}
		else
		{
			oldState = false;
			newState = false;
		}
	}

	private static void EvaluateOldNewStates(DataTrigger dataTrigger, DependencyObject triggerContainer, BindingBase binding, BindingValueChangedEventArgs bindingChangedArgs, UncommonField<HybridDictionary[]> dataField, Style style, FrameworkTemplate frameworkTemplate, out bool oldState, out bool newState)
	{
		TriggerCondition[] triggerConditions = dataTrigger.TriggerConditions;
		oldState = triggerConditions[0].ConvertAndMatch(bindingChangedArgs.OldValue);
		newState = triggerConditions[0].ConvertAndMatch(bindingChangedArgs.NewValue);
	}

	private static void EvaluateOldNewStates(MultiTrigger multiTrigger, DependencyObject triggerContainer, DependencyProperty changedProperty, DependencyPropertyChangedEventArgs changedArgs, int sourceChildIndex, Style style, FrameworkTemplate frameworkTemplate, out bool oldState, out bool newState)
	{
		int num = 0;
		DependencyObject dependencyObject = null;
		TriggerCondition[] triggerConditions = multiTrigger.TriggerConditions;
		oldState = false;
		newState = false;
		for (int i = 0; i < triggerConditions.Length; i++)
		{
			if (triggerConditions[i].SourceChildIndex != 0)
			{
				num = triggerConditions[i].SourceChildIndex;
				dependencyObject = GetChild(triggerContainer, num);
			}
			else
			{
				num = 0;
				dependencyObject = triggerContainer;
			}
			if (triggerConditions[i].Property == changedProperty && num == sourceChildIndex)
			{
				oldState = triggerConditions[i].Match(changedArgs.OldValue);
				newState = triggerConditions[i].Match(changedArgs.NewValue);
				if (oldState == newState)
				{
					break;
				}
				continue;
			}
			object value = dependencyObject.GetValue(triggerConditions[i].Property);
			if (!triggerConditions[i].Match(value))
			{
				oldState = false;
				newState = false;
				break;
			}
		}
	}

	private static void EvaluateOldNewStates(MultiDataTrigger multiDataTrigger, DependencyObject triggerContainer, BindingBase binding, BindingValueChangedEventArgs changedArgs, UncommonField<HybridDictionary[]> dataField, Style style, FrameworkTemplate frameworkTemplate, out bool oldState, out bool newState)
	{
		BindingBase bindingBase = null;
		object obj = null;
		TriggerCondition[] triggerConditions = multiDataTrigger.TriggerConditions;
		oldState = false;
		newState = false;
		for (int i = 0; i < multiDataTrigger.Conditions.Count; i++)
		{
			bindingBase = triggerConditions[i].Binding;
			if (bindingBase == binding)
			{
				oldState = triggerConditions[i].ConvertAndMatch(changedArgs.OldValue);
				newState = triggerConditions[i].ConvertAndMatch(changedArgs.NewValue);
				if (oldState == newState)
				{
					break;
				}
				continue;
			}
			obj = GetDataTriggerValue(dataField, triggerContainer, bindingBase);
			if (!triggerConditions[i].ConvertAndMatch(obj))
			{
				oldState = false;
				newState = false;
				break;
			}
		}
	}

	internal static void AddPropertyTriggerWithAction(TriggerBase triggerBase, DependencyProperty property, ref FrugalMap triggersWithActions)
	{
		object obj = triggersWithActions[property.GlobalIndex];
		if (obj == DependencyProperty.UnsetValue)
		{
			triggersWithActions[property.GlobalIndex] = triggerBase;
		}
		else if (obj is TriggerBase item)
		{
			List<TriggerBase> list = new List<TriggerBase>();
			list.Add(item);
			list.Add(triggerBase);
			triggersWithActions[property.GlobalIndex] = list;
		}
		else
		{
			((List<TriggerBase>)obj).Add(triggerBase);
		}
		triggerBase.EstablishLayer();
	}

	internal static void AddDataTriggerWithAction(TriggerBase triggerBase, BindingBase binding, ref HybridDictionary dataTriggersWithActions)
	{
		if (dataTriggersWithActions == null)
		{
			dataTriggersWithActions = new HybridDictionary();
		}
		object obj = dataTriggersWithActions[binding];
		if (obj == null)
		{
			dataTriggersWithActions[binding] = triggerBase;
		}
		else if (obj is TriggerBase item)
		{
			List<TriggerBase> list = new List<TriggerBase>();
			list.Add(item);
			list.Add(triggerBase);
			dataTriggersWithActions[binding] = list;
		}
		else
		{
			((List<TriggerBase>)obj).Add(triggerBase);
		}
		triggerBase.EstablishLayer();
	}

	private static void OnBindingValueInStyleChanged(object sender, BindingValueChangedEventArgs e)
	{
		BindingExpressionBase bindingExpressionBase = (BindingExpressionBase)sender;
		BindingBase parentBindingBase = bindingExpressionBase.ParentBindingBase;
		DependencyObject targetElement = bindingExpressionBase.TargetElement;
		if (targetElement == null)
		{
			return;
		}
		Helper.DowncastToFEorFCE(targetElement, out var fe, out var fce, throwIfNeither: false);
		Style style = ((fe != null) ? fe.Style : fce.Style);
		HybridDictionary dataTriggerRecordFromBinding = style._dataTriggerRecordFromBinding;
		if (dataTriggerRecordFromBinding != null && !bindingExpressionBase.IsAttaching)
		{
			DataTriggerRecord dataTriggerRecord = (DataTriggerRecord)dataTriggerRecordFromBinding[parentBindingBase];
			if (dataTriggerRecord != null)
			{
				InvalidateDependents(style, null, targetElement, null, ref dataTriggerRecord.Dependents, invalidateOnlyContainer: false);
			}
		}
		InvokeApplicableDataTriggerActions(style, null, targetElement, parentBindingBase, e, StyleDataField);
	}

	private static void OnBindingValueInTemplateChanged(object sender, BindingValueChangedEventArgs e)
	{
		BindingExpressionBase bindingExpressionBase = (BindingExpressionBase)sender;
		BindingBase parentBindingBase = bindingExpressionBase.ParentBindingBase;
		DependencyObject targetElement = bindingExpressionBase.TargetElement;
		if (targetElement == null)
		{
			return;
		}
		Helper.DowncastToFEorFCE(targetElement, out var fe, out var _, throwIfNeither: false);
		FrameworkTemplate templateInternal = fe.TemplateInternal;
		HybridDictionary hybridDictionary = null;
		if (templateInternal != null)
		{
			hybridDictionary = templateInternal._dataTriggerRecordFromBinding;
		}
		if (hybridDictionary != null && !bindingExpressionBase.IsAttaching)
		{
			DataTriggerRecord dataTriggerRecord = (DataTriggerRecord)hybridDictionary[parentBindingBase];
			if (dataTriggerRecord != null)
			{
				InvalidateDependents(null, templateInternal, targetElement, null, ref dataTriggerRecord.Dependents, invalidateOnlyContainer: false);
			}
		}
		InvokeApplicableDataTriggerActions(null, templateInternal, targetElement, parentBindingBase, e, TemplateDataField);
	}

	private static void OnBindingValueInThemeStyleChanged(object sender, BindingValueChangedEventArgs e)
	{
		BindingExpressionBase bindingExpressionBase = (BindingExpressionBase)sender;
		BindingBase parentBindingBase = bindingExpressionBase.ParentBindingBase;
		DependencyObject targetElement = bindingExpressionBase.TargetElement;
		if (targetElement == null)
		{
			return;
		}
		Helper.DowncastToFEorFCE(targetElement, out var fe, out var fce, throwIfNeither: false);
		Style style = ((fe != null) ? fe.ThemeStyle : fce.ThemeStyle);
		HybridDictionary dataTriggerRecordFromBinding = style._dataTriggerRecordFromBinding;
		if (dataTriggerRecordFromBinding != null && !bindingExpressionBase.IsAttaching)
		{
			DataTriggerRecord dataTriggerRecord = (DataTriggerRecord)dataTriggerRecordFromBinding[parentBindingBase];
			if (dataTriggerRecord != null)
			{
				InvalidateDependents(style, null, targetElement, null, ref dataTriggerRecord.Dependents, invalidateOnlyContainer: false);
			}
		}
		InvokeApplicableDataTriggerActions(style, null, targetElement, parentBindingBase, e, ThemeStyleDataField);
	}

	private static void InvokeApplicableDataTriggerActions(Style style, FrameworkTemplate frameworkTemplate, DependencyObject container, BindingBase binding, BindingValueChangedEventArgs e, UncommonField<HybridDictionary[]> dataField)
	{
		HybridDictionary hybridDictionary = ((style != null) ? style.DataTriggersWithActions : frameworkTemplate?.DataTriggersWithActions);
		if (hybridDictionary == null)
		{
			return;
		}
		object obj = hybridDictionary[binding];
		if (obj == null)
		{
			return;
		}
		if (obj is TriggerBase triggerBase)
		{
			InvokeDataTriggerActions(triggerBase, container, binding, e, style, frameworkTemplate, dataField);
			return;
		}
		List<TriggerBase> list = (List<TriggerBase>)obj;
		for (int i = 0; i < list.Count; i++)
		{
			InvokeDataTriggerActions(list[i], container, binding, e, style, frameworkTemplate, dataField);
		}
	}

	internal static int CreateChildIndexFromChildName(string childName, FrameworkTemplate frameworkTemplate)
	{
		HybridDictionary childIndexFromChildName = frameworkTemplate.ChildIndexFromChildName;
		int location = frameworkTemplate.LastChildIndex;
		if (childIndexFromChildName.Contains(childName))
		{
			throw new ArgumentException(SR.Format(SR.NameScopeDuplicateNamesNotAllowed, childName));
		}
		if (location >= 65535)
		{
			throw new InvalidOperationException(SR.StyleHasTooManyElements);
		}
		object obj2 = (childIndexFromChildName[childName] = location);
		Interlocked.Increment(ref location);
		frameworkTemplate.LastChildIndex = location;
		return (int)obj2;
	}

	internal static int QueryChildIndexFromChildName(string childName, HybridDictionary childIndexFromChildName)
	{
		if (childName == "~Self")
		{
			return 0;
		}
		object obj = childIndexFromChildName[childName];
		if (obj == null)
		{
			return -1;
		}
		return (int)obj;
	}

	internal static object FindNameInTemplateContent(DependencyObject container, string childName, FrameworkTemplate frameworkTemplate)
	{
		int num = QueryChildIndexFromChildName(childName, frameworkTemplate.ChildIndexFromChildName);
		if (num == -1)
		{
			return TemplatedNonFeChildrenField.GetValue(container)?[childName];
		}
		return GetChild(container, num);
	}

	internal static DependencyObject GetChild(DependencyObject container, int childIndex)
	{
		return GetChild(TemplatedFeChildrenField.GetValue(container), childIndex);
	}

	internal static DependencyObject GetChild(List<DependencyObject> styledChildren, int childIndex)
	{
		if (styledChildren == null || childIndex > styledChildren.Count)
		{
			return null;
		}
		if (childIndex < 0)
		{
			throw new ArgumentOutOfRangeException("childIndex");
		}
		return styledChildren[childIndex - 1];
	}

	internal static void RegisterAlternateExpressionStorage()
	{
		if (_getExpression == null)
		{
			DependencyObject.RegisterForAlternativeExpressionStorage(GetExpressionCore, out _getExpression);
		}
	}

	private static Expression GetExpressionCore(DependencyObject d, DependencyProperty dp, PropertyMetadata metadata)
	{
		Helper.DowncastToFEorFCE(d, out var fe, out var fce, throwIfNeither: false);
		if (fe != null)
		{
			return fe.GetExpressionCore(dp, metadata);
		}
		return fce?.GetExpressionCore(dp, metadata);
	}

	internal static Expression GetExpression(DependencyObject d, DependencyProperty dp)
	{
		Helper.DowncastToFEorFCE(d, out var fe, out var fce, throwIfNeither: false);
		bool flag = fe?.IsInitialized ?? fce?.IsInitialized ?? true;
		if (!flag)
		{
			if (fe != null)
			{
				fe.WriteInternalFlag(InternalFlags.IsInitialized, set: true);
			}
			else
			{
				fce?.WriteInternalFlag(InternalFlags.IsInitialized, set: true);
			}
		}
		Expression result = _getExpression(d, dp, dp.GetMetadata(d.DependencyObjectType));
		if (!flag)
		{
			if (fe != null)
			{
				fe.WriteInternalFlag(InternalFlags.IsInitialized, set: false);
				return result;
			}
			fce?.WriteInternalFlag(InternalFlags.IsInitialized, set: false);
		}
		return result;
	}

	internal static RoutedEventHandlerInfo[] GetChildRoutedEventHandlers(int childIndex, RoutedEvent routedEvent, ref ItemStructList<ChildEventDependent> eventDependents)
	{
		if (childIndex > 0)
		{
			EventHandlersStore eventHandlersStore = null;
			for (int i = 0; i < eventDependents.Count; i++)
			{
				if (eventDependents.List[i].ChildIndex == childIndex)
				{
					eventHandlersStore = eventDependents.List[i].EventHandlersStore;
					break;
				}
			}
			if (eventHandlersStore != null)
			{
				return eventHandlersStore.GetRoutedEventHandlers(routedEvent);
			}
		}
		return null;
	}

	internal static bool IsStylingLogicalTree(DependencyProperty dp, object value)
	{
		if (dp == ItemsControl.ItemsPanelProperty || dp == FrameworkElement.ContextMenuProperty || dp == FrameworkElement.ToolTipProperty)
		{
			return false;
		}
		if (!(value is Visual))
		{
			return value is ContentElement;
		}
		return true;
	}
}
