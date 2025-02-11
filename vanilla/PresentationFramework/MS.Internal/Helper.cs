using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xaml;
using MS.Internal.Controls;
using MS.Internal.Data;
using MS.Internal.Hashing.PresentationFramework;

namespace MS.Internal;

internal static class Helper
{
	private class FindResourceHelper
	{
		private object _name;

		private object _resource;

		internal object TryCatchWhen()
		{
			Dispatcher.CurrentDispatcher.WrappedInvoke(new DispatcherOperationCallback(DoTryCatchWhen), null, 1, new DispatcherOperationCallback(CatchHandler));
			return _resource;
		}

		private object DoTryCatchWhen(object arg)
		{
			throw new ResourceReferenceKeyNotFoundException(SR.Format(SR.MarkupExtensionResourceNotFound, _name), _name);
		}

		private object CatchHandler(object arg)
		{
			_resource = DependencyProperty.UnsetValue;
			return null;
		}

		public FindResourceHelper(object name)
		{
			_name = name;
			_resource = null;
		}
	}

	private class ModifiedItemValue
	{
		private object _value;

		private FullValueSource _valueSource;

		public object Value => _value;

		public bool IsCoercedWithCurrentValue => (_valueSource & FullValueSource.IsCoercedWithCurrentValue) != 0;

		public ModifiedItemValue(object value, FullValueSource valueSource)
		{
			_value = value;
			_valueSource = valueSource;
		}
	}

	private static readonly Type NullableType = Type.GetType("System.Nullable`1");

	private static readonly UncommonField<WeakDictionary<object, List<KeyValuePair<int, object>>>> ItemValueStorageField = new UncommonField<WeakDictionary<object, List<KeyValuePair<int, object>>>>();

	private static readonly int[] ItemValueStorageIndices = new int[5]
	{
		ItemValueStorageField.GlobalIndex,
		TreeViewItem.IsExpandedProperty.GlobalIndex,
		Expander.IsExpandedProperty.GlobalIndex,
		GroupItem.DesiredPixelItemsSizeCorrectionFactorField.GlobalIndex,
		VirtualizingStackPanel.ItemsHostInsetProperty.GlobalIndex
	};

	internal static object ResourceFailureThrow(object key)
	{
		return new FindResourceHelper(key).TryCatchWhen();
	}

	internal static object FindTemplateResourceFromAppOrSystem(DependencyObject target, ArrayList keys, int exactMatch, ref int bestMatch)
	{
		object result = null;
		if (Application.Current != null)
		{
			for (int i = 0; i < bestMatch; i++)
			{
				object obj = Application.Current.FindResourceInternal(keys[i]);
				if (obj != null)
				{
					bestMatch = i;
					result = obj;
					if (bestMatch < exactMatch)
					{
						return result;
					}
				}
			}
		}
		if (bestMatch >= exactMatch)
		{
			for (int i = 0; i < bestMatch; i++)
			{
				object obj2 = SystemResources.FindResourceInternal(keys[i]);
				if (obj2 != null)
				{
					bestMatch = i;
					result = obj2;
					if (bestMatch < exactMatch)
					{
						return result;
					}
				}
			}
		}
		return result;
	}

	internal static DependencyObject FindMentor(DependencyObject d)
	{
		while (d != null)
		{
			DowncastToFEorFCE(d, out var fe, out var fce, throwIfNeither: false);
			if (fe != null)
			{
				return fe;
			}
			if (fce != null)
			{
				return fce;
			}
			d = d.InheritanceContext;
		}
		return null;
	}

	internal static bool HasDefaultValue(DependencyObject d, DependencyProperty dp)
	{
		return HasDefaultOrInheritedValueImpl(d, dp, checkInherited: false, ignoreModifiers: true);
	}

	internal static bool HasDefaultOrInheritedValue(DependencyObject d, DependencyProperty dp)
	{
		return HasDefaultOrInheritedValueImpl(d, dp, checkInherited: true, ignoreModifiers: true);
	}

	internal static bool HasUnmodifiedDefaultValue(DependencyObject d, DependencyProperty dp)
	{
		return HasDefaultOrInheritedValueImpl(d, dp, checkInherited: false, ignoreModifiers: false);
	}

	internal static bool HasUnmodifiedDefaultOrInheritedValue(DependencyObject d, DependencyProperty dp)
	{
		return HasDefaultOrInheritedValueImpl(d, dp, checkInherited: true, ignoreModifiers: false);
	}

	private static bool HasDefaultOrInheritedValueImpl(DependencyObject d, DependencyProperty dp, bool checkInherited, bool ignoreModifiers)
	{
		PropertyMetadata metadata = dp.GetMetadata(d);
		bool hasModifiers;
		BaseValueSourceInternal valueSource = d.GetValueSource(dp, metadata, out hasModifiers);
		if (valueSource == BaseValueSourceInternal.Default || (checkInherited && valueSource == BaseValueSourceInternal.Inherited))
		{
			if (ignoreModifiers && (d is FrameworkElement || d is FrameworkContentElement))
			{
				hasModifiers = false;
			}
			return !hasModifiers;
		}
		return false;
	}

	internal static void DowncastToFEorFCE(DependencyObject d, out FrameworkElement fe, out FrameworkContentElement fce, bool throwIfNeither)
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
		if (throwIfNeither)
		{
			throw new InvalidOperationException(SR.Format(SR.MustBeFrameworkDerived, d.GetType()));
		}
		fe = null;
		fce = null;
	}

	internal static void CheckStyleAndStyleSelector(string name, DependencyProperty styleProperty, DependencyProperty styleSelectorProperty, DependencyObject d)
	{
		if (!TraceData.IsEnabled)
		{
			return;
		}
		object obj = d.ReadLocalValue(styleSelectorProperty);
		if (obj != DependencyProperty.UnsetValue && (obj is StyleSelector || obj is ResourceReferenceExpression))
		{
			object obj2 = d.ReadLocalValue(styleProperty);
			if (obj2 != DependencyProperty.UnsetValue && (obj2 is Style || obj2 is ResourceReferenceExpression))
			{
				TraceData.TraceAndNotify(TraceEventType.Error, TraceData.StyleAndStyleSelectorDefined(name), null, new object[1] { d });
			}
		}
	}

	internal static void CheckTemplateAndTemplateSelector(string name, DependencyProperty templateProperty, DependencyProperty templateSelectorProperty, DependencyObject d)
	{
		if (TraceData.IsEnabled && IsTemplateSelectorDefined(templateSelectorProperty, d) && IsTemplateDefined(templateProperty, d))
		{
			TraceData.TraceAndNotify(TraceEventType.Error, TraceData.TemplateAndTemplateSelectorDefined(name), null, new object[1] { d });
		}
	}

	internal static bool IsTemplateSelectorDefined(DependencyProperty templateSelectorProperty, DependencyObject d)
	{
		object obj = d.ReadLocalValue(templateSelectorProperty);
		if (obj != DependencyProperty.UnsetValue && obj != null)
		{
			if (!(obj is DataTemplateSelector))
			{
				return obj is ResourceReferenceExpression;
			}
			return true;
		}
		return false;
	}

	internal static bool IsTemplateDefined(DependencyProperty templateProperty, DependencyObject d)
	{
		object obj = d.ReadLocalValue(templateProperty);
		if (obj != DependencyProperty.UnsetValue && obj != null)
		{
			if (!(obj is FrameworkTemplate))
			{
				return obj is ResourceReferenceExpression;
			}
			return true;
		}
		return false;
	}

	internal static object FindNameInTemplate(string name, DependencyObject templatedParent)
	{
		FrameworkElement frameworkElement = templatedParent as FrameworkElement;
		return frameworkElement.TemplateInternal.FindName(name, frameworkElement);
	}

	internal static IGeneratorHost GeneratorHostForElement(DependencyObject element)
	{
		DependencyObject dependencyObject = null;
		DependencyObject dependencyObject2 = null;
		while (element != null)
		{
			while (element != null)
			{
				dependencyObject = element;
				element = GetTemplatedParent(element);
				if (dependencyObject is ContentPresenter && element is ComboBox result)
				{
					return result;
				}
			}
			if (dependencyObject is Visual reference)
			{
				dependencyObject2 = VisualTreeHelper.GetParent(reference);
				element = dependencyObject2 as GridViewRowPresenterBase;
			}
			else
			{
				dependencyObject2 = null;
			}
		}
		if (dependencyObject2 != null)
		{
			ItemsControl itemsOwner = ItemsControl.GetItemsOwner(dependencyObject2);
			if (itemsOwner != null)
			{
				return itemsOwner;
			}
		}
		return null;
	}

	internal static DependencyObject GetTemplatedParent(DependencyObject d)
	{
		DowncastToFEorFCE(d, out var fe, out var fce, throwIfNeither: false);
		if (fe != null)
		{
			return fe.TemplatedParent;
		}
		return fce?.TemplatedParent;
	}

	internal static XmlDataProvider XmlDataProviderForElement(DependencyObject d)
	{
		ICollectionView collectionView = (GeneratorHostForElement(d)?.View)?.CollectionView;
		return ((collectionView != null) ? (collectionView.SourceCollection as XmlDataCollection) : null)?.ParentXmlDataProvider;
	}

	internal static Size MeasureElementWithSingleChild(UIElement element, Size constraint)
	{
		UIElement uIElement = ((VisualTreeHelper.GetChildrenCount(element) > 0) ? (VisualTreeHelper.GetChild(element, 0) as UIElement) : null);
		if (uIElement != null)
		{
			uIElement.Measure(constraint);
			return uIElement.DesiredSize;
		}
		return default(Size);
	}

	internal static Size ArrangeElementWithSingleChild(UIElement element, Size arrangeSize)
	{
		((VisualTreeHelper.GetChildrenCount(element) > 0) ? (VisualTreeHelper.GetChild(element, 0) as UIElement) : null)?.Arrange(new Rect(arrangeSize));
		return arrangeSize;
	}

	internal static bool IsDoubleValid(double value)
	{
		if (!double.IsInfinity(value))
		{
			return !double.IsNaN(value);
		}
		return false;
	}

	internal static void CheckCanReceiveMarkupExtension(MarkupExtension markupExtension, IServiceProvider serviceProvider, out DependencyObject targetDependencyObject, out DependencyProperty targetDependencyProperty)
	{
		targetDependencyObject = null;
		targetDependencyProperty = null;
		if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget { TargetObject: { } targetObject } provideValueTarget))
		{
			return;
		}
		Type type = targetObject.GetType();
		object targetProperty = provideValueTarget.TargetProperty;
		if (targetProperty != null)
		{
			targetDependencyProperty = targetProperty as DependencyProperty;
			if (targetDependencyProperty != null)
			{
				targetDependencyObject = targetObject as DependencyObject;
				return;
			}
			MemberInfo memberInfo = targetProperty as MemberInfo;
			if (memberInfo != null)
			{
				PropertyInfo propertyInfo = memberInfo as PropertyInfo;
				EventHandler<XamlSetMarkupExtensionEventArgs> eventHandler = LookupSetMarkupExtensionHandler(type);
				if (eventHandler != null && propertyInfo != null && serviceProvider.GetService(typeof(IXamlSchemaContextProvider)) is IXamlSchemaContextProvider xamlSchemaContextProvider)
				{
					XamlType xamlType = xamlSchemaContextProvider.SchemaContext.GetXamlType(type);
					if (xamlType != null)
					{
						XamlMember member = xamlType.GetMember(propertyInfo.Name);
						if (member != null)
						{
							XamlSetMarkupExtensionEventArgs xamlSetMarkupExtensionEventArgs = new XamlSetMarkupExtensionEventArgs(member, markupExtension, serviceProvider);
							eventHandler(targetObject, xamlSetMarkupExtensionEventArgs);
							if (xamlSetMarkupExtensionEventArgs.Handled)
							{
								return;
							}
						}
					}
				}
				Type type2 = ((!(propertyInfo != null)) ? ((MethodInfo)memberInfo).GetParameters()[1].ParameterType : propertyInfo.PropertyType);
				if (!typeof(MarkupExtension).IsAssignableFrom(type2) || !type2.IsAssignableFrom(markupExtension.GetType()))
				{
					throw new System.Windows.Markup.XamlParseException(SR.Format(SR.MarkupExtensionDynamicOrBindingOnClrProp, markupExtension.GetType().Name, memberInfo.Name, type.Name));
				}
			}
			else if (!typeof(BindingBase).IsAssignableFrom(markupExtension.GetType()) || !typeof(Collection<BindingBase>).IsAssignableFrom(targetProperty.GetType()))
			{
				throw new System.Windows.Markup.XamlParseException(SR.Format(SR.MarkupExtensionDynamicOrBindingInCollection, markupExtension.GetType().Name, targetProperty.GetType().Name));
			}
		}
		else if (!typeof(BindingBase).IsAssignableFrom(markupExtension.GetType()) || !typeof(Collection<BindingBase>).IsAssignableFrom(type))
		{
			throw new System.Windows.Markup.XamlParseException(SR.Format(SR.MarkupExtensionDynamicOrBindingInCollection, markupExtension.GetType().Name, type.Name));
		}
	}

	private static EventHandler<XamlSetMarkupExtensionEventArgs> LookupSetMarkupExtensionHandler(Type type)
	{
		if (typeof(Setter).IsAssignableFrom(type))
		{
			return Setter.ReceiveMarkupExtension;
		}
		if (typeof(DataTrigger).IsAssignableFrom(type))
		{
			return DataTrigger.ReceiveMarkupExtension;
		}
		if (typeof(Condition).IsAssignableFrom(type))
		{
			return Condition.ReceiveMarkupExtension;
		}
		return null;
	}

	internal static string GetEffectiveStringFormat(string stringFormat)
	{
		if (stringFormat.IndexOf('{') < 0)
		{
			stringFormat = "{0:" + stringFormat + "}";
		}
		return stringFormat;
	}

	internal static object ReadItemValue(DependencyObject owner, object item, int dpIndex)
	{
		if (item != null)
		{
			List<KeyValuePair<int, object>> itemValues = GetItemValues(owner, item);
			if (itemValues != null)
			{
				for (int i = 0; i < itemValues.Count; i++)
				{
					if (itemValues[i].Key == dpIndex)
					{
						return itemValues[i].Value;
					}
				}
			}
		}
		return null;
	}

	internal static void StoreItemValue(DependencyObject owner, object item, int dpIndex, object value)
	{
		if (item == null)
		{
			return;
		}
		List<KeyValuePair<int, object>> list = EnsureItemValues(owner, item);
		bool flag = false;
		KeyValuePair<int, object> keyValuePair = new KeyValuePair<int, object>(dpIndex, value);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Key == dpIndex)
			{
				list[i] = keyValuePair;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			list.Add(keyValuePair);
		}
	}

	internal static void ClearItemValue(DependencyObject owner, object item, int dpIndex)
	{
		if (item == null)
		{
			return;
		}
		List<KeyValuePair<int, object>> itemValues = GetItemValues(owner, item);
		if (itemValues == null)
		{
			return;
		}
		for (int i = 0; i < itemValues.Count; i++)
		{
			if (itemValues[i].Key == dpIndex)
			{
				itemValues.RemoveAt(i);
				break;
			}
		}
	}

	internal static List<KeyValuePair<int, object>> GetItemValues(DependencyObject owner, object item)
	{
		return GetItemValues(owner, item, ItemValueStorageField.GetValue(owner));
	}

	internal static List<KeyValuePair<int, object>> GetItemValues(DependencyObject owner, object item, WeakDictionary<object, List<KeyValuePair<int, object>>> itemValueStorage)
	{
		List<KeyValuePair<int, object>> value = null;
		itemValueStorage?.TryGetValue(item, out value);
		return value;
	}

	internal static List<KeyValuePair<int, object>> EnsureItemValues(DependencyObject owner, object item)
	{
		WeakDictionary<object, List<KeyValuePair<int, object>>> weakDictionary = EnsureItemValueStorage(owner);
		List<KeyValuePair<int, object>> list = GetItemValues(owner, item, weakDictionary);
		if (list == null && HashHelper.HasReliableHashCode(item))
		{
			list = (weakDictionary[item] = new List<KeyValuePair<int, object>>(3));
		}
		return list;
	}

	internal static WeakDictionary<object, List<KeyValuePair<int, object>>> EnsureItemValueStorage(DependencyObject owner)
	{
		WeakDictionary<object, List<KeyValuePair<int, object>>> weakDictionary = ItemValueStorageField.GetValue(owner);
		if (weakDictionary == null)
		{
			weakDictionary = new WeakDictionary<object, List<KeyValuePair<int, object>>>();
			ItemValueStorageField.SetValue(owner, weakDictionary);
		}
		return weakDictionary;
	}

	internal static void SetItemValuesOnContainer(DependencyObject owner, DependencyObject container, object item)
	{
		int[] itemValueStorageIndices = ItemValueStorageIndices;
		List<KeyValuePair<int, object>> list = GetItemValues(owner, item) ?? new List<KeyValuePair<int, object>>();
		foreach (int num in itemValueStorageIndices)
		{
			DependencyProperty dependencyProperty = DependencyProperty.RegisteredPropertyList.List[num];
			object obj = DependencyProperty.UnsetValue;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].Key == num)
				{
					obj = list[j].Value;
					break;
				}
			}
			if (dependencyProperty != null)
			{
				if (obj != DependencyProperty.UnsetValue)
				{
					if (!(obj is ModifiedItemValue modifiedItemValue))
					{
						container.SetValue(dependencyProperty, obj);
					}
					else if (modifiedItemValue.IsCoercedWithCurrentValue)
					{
						container.SetCurrentValue(dependencyProperty, modifiedItemValue.Value);
					}
				}
				else
				{
					if (container == container.GetValue(ItemContainerGenerator.ItemForItemContainerProperty))
					{
						continue;
					}
					EntryIndex entryIndex = container.LookupEntry(num);
					EffectiveValueEntry effectiveValueEntry = new EffectiveValueEntry(dependencyProperty);
					if (entryIndex.Found)
					{
						effectiveValueEntry = container.EffectiveValues[entryIndex.Index];
						if (effectiveValueEntry.IsCoercedWithCurrentValue)
						{
							container.InvalidateProperty(dependencyProperty, preserveCurrentValue: false);
							entryIndex = container.LookupEntry(num);
							if (entryIndex.Found)
							{
								effectiveValueEntry = container.EffectiveValues[entryIndex.Index];
							}
						}
					}
					if (entryIndex.Found && (effectiveValueEntry.BaseValueSourceInternal == BaseValueSourceInternal.Local || effectiveValueEntry.BaseValueSourceInternal == BaseValueSourceInternal.ParentTemplate) && !effectiveValueEntry.HasModifiers)
					{
						container.ClearValue(dependencyProperty);
					}
				}
			}
			else if (obj != DependencyProperty.UnsetValue)
			{
				EntryIndex entryIndex2 = container.LookupEntry(num);
				container.SetEffectiveValue(entryIndex2, null, num, null, obj, BaseValueSourceInternal.Local);
			}
		}
	}

	internal static void StoreItemValues(IContainItemStorage owner, DependencyObject container, object item)
	{
		int[] itemValueStorageIndices = ItemValueStorageIndices;
		DependencyObject owner2 = (DependencyObject)owner;
		foreach (int num in itemValueStorageIndices)
		{
			EntryIndex entryIndex = container.LookupEntry(num);
			if (entryIndex.Found)
			{
				EffectiveValueEntry effectiveValueEntry = container.EffectiveValues[entryIndex.Index];
				if ((effectiveValueEntry.BaseValueSourceInternal == BaseValueSourceInternal.Local || effectiveValueEntry.BaseValueSourceInternal == BaseValueSourceInternal.ParentTemplate) && !effectiveValueEntry.HasModifiers)
				{
					StoreItemValue(owner2, item, num, effectiveValueEntry.Value);
				}
				else if (effectiveValueEntry.IsCoercedWithCurrentValue)
				{
					StoreItemValue(owner2, item, num, new ModifiedItemValue(effectiveValueEntry.ModifiedValue.CoercedValue, FullValueSource.IsCoercedWithCurrentValue));
				}
				else
				{
					ClearItemValue(owner2, item, num);
				}
			}
		}
	}

	internal static void ClearItemValueStorage(DependencyObject owner)
	{
		ItemValueStorageField.ClearValue(owner);
	}

	internal static void ClearItemValueStorage(DependencyObject owner, int[] dpIndices)
	{
		ClearItemValueStorageRecursive(ItemValueStorageField.GetValue(owner), dpIndices);
	}

	private static void ClearItemValueStorageRecursive(WeakDictionary<object, List<KeyValuePair<int, object>>> itemValueStorage, int[] dpIndices)
	{
		if (itemValueStorage == null)
		{
			return;
		}
		foreach (List<KeyValuePair<int, object>> value in itemValueStorage.Values)
		{
			for (int i = 0; i < value.Count; i++)
			{
				KeyValuePair<int, object> keyValuePair = value[i];
				if (keyValuePair.Key == ItemValueStorageField.GlobalIndex)
				{
					ClearItemValueStorageRecursive((WeakDictionary<object, List<KeyValuePair<int, object>>>)keyValuePair.Value, dpIndices);
				}
				for (int j = 0; j < dpIndices.Length; j++)
				{
					if (keyValuePair.Key == dpIndices[j])
					{
						value.RemoveAt(i--);
						break;
					}
				}
			}
		}
	}

	internal static void ApplyCorrectionFactorToPixelHeaderSize(ItemsControl scrollingItemsControl, FrameworkElement virtualizingElement, Panel itemsHost, ref Size headerSize)
	{
		if (VirtualizingStackPanel.IsVSP45Compat)
		{
			if (itemsHost != null && itemsHost.IsVisible)
			{
				headerSize.Height = Math.Max(GroupItem.DesiredPixelItemsSizeCorrectionFactorField.GetValue(virtualizingElement).Top, headerSize.Height);
			}
			else
			{
				headerSize.Height = Math.Max(virtualizingElement.DesiredSize.Height, headerSize.Height);
			}
			headerSize.Width = Math.Max(virtualizingElement.DesiredSize.Width, headerSize.Width);
		}
	}

	internal static HierarchicalVirtualizationItemDesiredSizes ApplyCorrectionFactorToItemDesiredSizes(FrameworkElement virtualizingElement, Panel itemsHost)
	{
		HierarchicalVirtualizationItemDesiredSizes result = GroupItem.HierarchicalVirtualizationItemDesiredSizesField.GetValue(virtualizingElement);
		if (!VirtualizingStackPanel.IsVSP45Compat)
		{
			return result;
		}
		if (itemsHost != null && itemsHost.IsVisible)
		{
			Size pixelSize = result.PixelSize;
			Size pixelSizeInViewport = result.PixelSizeInViewport;
			Size pixelSizeBeforeViewport = result.PixelSizeBeforeViewport;
			Size pixelSizeAfterViewport = result.PixelSizeAfterViewport;
			bool flag = false;
			Thickness thickness = new Thickness(0.0);
			Size desiredSize = virtualizingElement.DesiredSize;
			if (DoubleUtil.GreaterThanZero(pixelSize.Height))
			{
				thickness = GroupItem.DesiredPixelItemsSizeCorrectionFactorField.GetValue(virtualizingElement);
				pixelSize.Height += thickness.Bottom;
				flag = true;
			}
			pixelSize.Width = Math.Max(desiredSize.Width, pixelSize.Width);
			if (DoubleUtil.AreClose(result.PixelSizeAfterViewport.Height, 0.0) && DoubleUtil.AreClose(result.PixelSizeInViewport.Height, 0.0) && DoubleUtil.GreaterThanZero(result.PixelSizeBeforeViewport.Height))
			{
				if (!flag)
				{
					thickness = GroupItem.DesiredPixelItemsSizeCorrectionFactorField.GetValue(virtualizingElement);
				}
				pixelSizeBeforeViewport.Height += thickness.Bottom;
				flag = true;
			}
			pixelSizeBeforeViewport.Width = Math.Max(desiredSize.Width, pixelSizeBeforeViewport.Width);
			if (DoubleUtil.IsZero(result.PixelSizeAfterViewport.Height) && DoubleUtil.GreaterThanZero(result.PixelSizeInViewport.Height))
			{
				if (!flag)
				{
					thickness = GroupItem.DesiredPixelItemsSizeCorrectionFactorField.GetValue(virtualizingElement);
				}
				pixelSizeInViewport.Height += thickness.Bottom;
				flag = true;
			}
			pixelSizeInViewport.Width = Math.Max(desiredSize.Width, pixelSizeInViewport.Width);
			if (DoubleUtil.GreaterThanZero(result.PixelSizeAfterViewport.Height))
			{
				if (!flag)
				{
					thickness = GroupItem.DesiredPixelItemsSizeCorrectionFactorField.GetValue(virtualizingElement);
				}
				pixelSizeAfterViewport.Height += thickness.Bottom;
				flag = true;
			}
			pixelSizeAfterViewport.Width = Math.Max(desiredSize.Width, pixelSizeAfterViewport.Width);
			result = new HierarchicalVirtualizationItemDesiredSizes(result.LogicalSize, result.LogicalSizeInViewport, result.LogicalSizeBeforeViewport, result.LogicalSizeAfterViewport, pixelSize, pixelSizeInViewport, pixelSizeBeforeViewport, pixelSizeAfterViewport);
		}
		return result;
	}

	internal static void ComputeCorrectionFactor(ItemsControl scrollingItemsControl, FrameworkElement virtualizingElement, Panel itemsHost, FrameworkElement headerElement)
	{
		if (!VirtualizingStackPanel.IsVSP45Compat)
		{
			return;
		}
		Rect rect = new Rect(default(Point), virtualizingElement.DesiredSize);
		bool flag = false;
		if (itemsHost != null)
		{
			Thickness value = default(Thickness);
			if (itemsHost.IsVisible)
			{
				Rect rect2 = itemsHost.TransformToAncestor(virtualizingElement).TransformBounds(new Rect(default(Point), itemsHost.DesiredSize));
				value.Top = rect2.Top;
				value.Bottom = rect.Bottom - rect2.Bottom;
				if (value.Bottom < 0.0)
				{
					value.Bottom = 0.0;
				}
			}
			Thickness value2 = GroupItem.DesiredPixelItemsSizeCorrectionFactorField.GetValue(virtualizingElement);
			if (!DoubleUtil.AreClose(value.Top, value2.Top) || !DoubleUtil.AreClose(value.Bottom, value2.Bottom))
			{
				flag = true;
				GroupItem.DesiredPixelItemsSizeCorrectionFactorField.SetValue(virtualizingElement, value);
			}
		}
		if (!flag || scrollingItemsControl == null)
		{
			return;
		}
		itemsHost = scrollingItemsControl.ItemsHost;
		if (itemsHost != null)
		{
			if (itemsHost is VirtualizingStackPanel virtualizingStackPanel)
			{
				virtualizingStackPanel.AnchoredInvalidateMeasure();
			}
			else
			{
				itemsHost.InvalidateMeasure();
			}
		}
	}

	internal static void ClearVirtualizingElement(IHierarchicalVirtualizationAndScrollInfo virtualizingElement)
	{
		virtualizingElement.ItemDesiredSizes = default(HierarchicalVirtualizationItemDesiredSizes);
		virtualizingElement.MustDisableVirtualization = false;
	}

	internal static T FindTemplatedDescendant<T>(FrameworkElement searchStart, FrameworkElement templatedParent) where T : FrameworkElement
	{
		FrameworkElement frameworkElement = null;
		T val = null;
		int childrenCount = VisualTreeHelper.GetChildrenCount(searchStart);
		for (int i = 0; i < childrenCount; i++)
		{
			if (val != null)
			{
				break;
			}
			if (VisualTreeHelper.GetChild(searchStart, i) is FrameworkElement frameworkElement2 && frameworkElement2.TemplatedParent == templatedParent)
			{
				val = ((!(frameworkElement2 is T val2)) ? FindTemplatedDescendant<T>(frameworkElement2, templatedParent) : val2);
			}
		}
		return val;
	}

	internal static T FindVisualAncestor<T>(DependencyObject element, Func<DependencyObject, bool> shouldContinueFunc) where T : DependencyObject
	{
		while (element != null)
		{
			element = VisualTreeHelper.GetParent(element);
			if (element is T result)
			{
				return result;
			}
			if (!shouldContinueFunc(element))
			{
				break;
			}
		}
		return null;
	}

	internal static void InvalidateMeasureOnPath(DependencyObject pathStartElement, DependencyObject pathEndElement, bool duringMeasure)
	{
		InvalidateMeasureOnPath(pathStartElement, pathEndElement, duringMeasure, includePathEnd: false);
	}

	internal static void InvalidateMeasureOnPath(DependencyObject pathStartElement, DependencyObject pathEndElement, bool duringMeasure, bool includePathEnd)
	{
		DependencyObject dependencyObject = pathStartElement;
		while (dependencyObject != null && (includePathEnd || dependencyObject != pathEndElement))
		{
			if (dependencyObject is UIElement uIElement)
			{
				if (duringMeasure)
				{
					uIElement.InvalidateMeasureInternal();
				}
				else
				{
					uIElement.InvalidateMeasure();
				}
			}
			if (dependencyObject != pathEndElement)
			{
				dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
				continue;
			}
			break;
		}
	}

	internal static void InvalidateMeasureForSubtree(DependencyObject d)
	{
		if (d is UIElement uIElement)
		{
			if (uIElement.MeasureDirty)
			{
				return;
			}
			uIElement.InvalidateMeasureInternal();
		}
		int childrenCount = VisualTreeHelper.GetChildrenCount(d);
		for (int i = 0; i < childrenCount; i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(d, i);
			if (child != null)
			{
				InvalidateMeasureForSubtree(child);
			}
		}
	}

	internal static bool IsAnyAncestorOf(DependencyObject ancestor, DependencyObject element)
	{
		if (ancestor == null || element == null)
		{
			return false;
		}
		return FindAnyAncestor(element, (DependencyObject d) => d == ancestor) != null;
	}

	internal static DependencyObject FindAnyAncestor(DependencyObject element, Predicate<DependencyObject> predicate)
	{
		while (element != null)
		{
			element = GetAnyParent(element);
			if (element != null && predicate(element))
			{
				return element;
			}
		}
		return null;
	}

	internal static DependencyObject GetAnyParent(DependencyObject element)
	{
		DependencyObject dependencyObject = null;
		if (!(element is ContentElement))
		{
			dependencyObject = VisualTreeHelper.GetParent(element);
		}
		if (dependencyObject == null)
		{
			dependencyObject = LogicalTreeHelper.GetParent(element);
		}
		return dependencyObject;
	}

	internal static bool IsDefaultValue(DependencyProperty dp, DependencyObject element)
	{
		bool hasModifiers;
		return element.GetValueSource(dp, null, out hasModifiers) == BaseValueSourceInternal.Default;
	}

	internal static bool IsComposing(DependencyObject d, DependencyProperty dp)
	{
		if (dp != TextBox.TextProperty)
		{
			return false;
		}
		return IsComposing(d as TextBoxBase);
	}

	internal static bool IsComposing(TextBoxBase tbb)
	{
		if (tbb == null)
		{
			return false;
		}
		TextEditor textEditor = tbb.TextEditor;
		if (textEditor == null)
		{
			return false;
		}
		return textEditor.TextStore?.IsEffectivelyComposing ?? false;
	}
}
