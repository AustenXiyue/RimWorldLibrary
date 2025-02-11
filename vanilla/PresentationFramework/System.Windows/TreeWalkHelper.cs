using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MS.Internal;
using MS.Utility;

namespace System.Windows;

internal static class TreeWalkHelper
{
	private static VisitedCallback<TreeChangeInfo> TreeChangeDelegate = OnAncestorChanged;

	private static VisitedCallback<TreeChangeInfo> TreeChangePostDelegate = OnPostAncestorChanged;

	private static VisitedCallback<ResourcesChangeInfo> ResourcesChangeDelegate = OnResourcesChangedCallback;

	private static VisitedCallback<InheritablePropertyChangeInfo> InheritablePropertyChangeDelegate = OnInheritablePropertyChanged;

	internal static void InvalidateOnTreeChange(FrameworkElement fe, FrameworkContentElement fce, DependencyObject parent, bool isAddOperation)
	{
		FrameworkObject frameworkObject = new FrameworkObject(parent);
		if (!frameworkObject.IsValid)
		{
			parent = frameworkObject.FrameworkParent.DO;
		}
		FrameworkObject frameworkObject2 = new FrameworkObject(fe, fce);
		if (isAddOperation)
		{
			frameworkObject2.SetShouldLookupImplicitStyles();
		}
		frameworkObject2.Reset(frameworkObject2.TemplatedParent);
		frameworkObject2.HasTemplateChanged = false;
		DependencyObject dependencyObject = ((fe != null) ? ((DependencyObject)fe) : ((DependencyObject)fce));
		if (fe != null)
		{
			if (fe.IsInitialized && !fe.HasLocalStyle)
			{
				fe.HasStyleChanged = false;
				fe.HasStyleInvalidated = false;
				fe.HasTemplateChanged = false;
				fe.AncestorChangeInProgress = true;
				fe.UpdateStyleProperty();
				fe.AncestorChangeInProgress = false;
			}
		}
		else if (!fce.HasLocalStyle)
		{
			fce.HasStyleChanged = false;
			fce.HasStyleInvalidated = false;
			fce.AncestorChangeInProgress = true;
			fce.UpdateStyleProperty();
			fce.AncestorChangeInProgress = false;
		}
		if (HasChildren(fe, fce))
		{
			FrameworkContextData frameworkContextData = FrameworkContextData.From(dependencyObject.Dispatcher);
			if (!frameworkContextData.WasNodeVisited(dependencyObject, TreeChangeDelegate))
			{
				PrePostDescendentsWalker<TreeChangeInfo> prePostDescendentsWalker = new PrePostDescendentsWalker<TreeChangeInfo>(data: new TreeChangeInfo(dependencyObject, parent, isAddOperation), priority: TreeWalkPriority.LogicalTree, preCallback: TreeChangeDelegate, postCallback: TreeChangePostDelegate);
				frameworkContextData.AddWalker(TreeChangeDelegate, prePostDescendentsWalker);
				try
				{
					prePostDescendentsWalker.StartWalk(dependencyObject);
				}
				finally
				{
					frameworkContextData.RemoveWalker(TreeChangeDelegate, prePostDescendentsWalker);
				}
			}
		}
		else
		{
			TreeChangeInfo info = new TreeChangeInfo(dependencyObject, parent, isAddOperation);
			OnAncestorChanged(fe, fce, info);
			bool visitedViaVisualTree = false;
			OnPostAncestorChanged(dependencyObject, info, visitedViaVisualTree);
		}
	}

	private static bool OnAncestorChanged(DependencyObject d, TreeChangeInfo info, bool visitedViaVisualTree)
	{
		FrameworkObject frameworkObject = new FrameworkObject(d, throwIfNeither: true);
		OnAncestorChanged(frameworkObject.FE, frameworkObject.FCE, info);
		return true;
	}

	private static void OnAncestorChanged(FrameworkElement fe, FrameworkContentElement fce, TreeChangeInfo info)
	{
		if (fe != null)
		{
			fe.OnAncestorChangedInternal(info);
		}
		else
		{
			fce.OnAncestorChangedInternal(info);
		}
	}

	private static bool OnPostAncestorChanged(DependencyObject d, TreeChangeInfo info, bool visitedViaVisualTree)
	{
		if (info.TopmostCollapsedParentNode == d)
		{
			info.TopmostCollapsedParentNode = null;
		}
		info.InheritablePropertiesStack.Pop();
		return true;
	}

	internal static FrugalObjectList<DependencyProperty> InvalidateTreeDependentProperties(TreeChangeInfo info, FrameworkElement fe, FrameworkContentElement fce, Style selfStyle, Style selfThemeStyle, ref ChildRecord childRecord, bool isChildRecordValid, bool hasStyleChanged, bool isSelfInheritanceParent, bool wasSelfInheritanceParent)
	{
		DependencyObject dependencyObject = ((fe != null) ? ((DependencyObject)fe) : ((DependencyObject)fce));
		FrameworkObject fo = new FrameworkObject(fe, fce);
		FrugalObjectList<DependencyProperty> frugalObjectList = info.InheritablePropertiesStack.Peek();
		int num = frugalObjectList?.Count ?? 0;
		FrugalObjectList<DependencyProperty> frugalObjectList2 = null;
		if (HasChildren(fe, fce))
		{
			frugalObjectList2 = new FrugalObjectList<DependencyProperty>(num);
		}
		info.ResetInheritableValueIndexer();
		for (int i = 0; i < num; i++)
		{
			DependencyProperty dependencyProperty = frugalObjectList[i];
			PropertyMetadata metadata = dependencyProperty.GetMetadata(dependencyObject);
			if (metadata.IsInherited)
			{
				FrameworkPropertyMetadata frameworkPropertyMetadata = (FrameworkPropertyMetadata)metadata;
				if (InvalidateTreeDependentProperty(info, dependencyObject, ref fo, dependencyProperty, frameworkPropertyMetadata, selfStyle, selfThemeStyle, ref childRecord, isChildRecordValid, hasStyleChanged, isSelfInheritanceParent, wasSelfInheritanceParent) && frugalObjectList2 != null && (!SkipNow(fo.InheritanceBehavior) || frameworkPropertyMetadata.OverridesInheritanceBehavior))
				{
					frugalObjectList2.Add(dependencyProperty);
				}
			}
		}
		return frugalObjectList2;
	}

	private static bool InvalidateTreeDependentProperty(TreeChangeInfo info, DependencyObject d, ref FrameworkObject fo, DependencyProperty dp, FrameworkPropertyMetadata fMetadata, Style selfStyle, Style selfThemeStyle, ref ChildRecord childRecord, bool isChildRecordValid, bool hasStyleChanged, bool isSelfInheritanceParent, bool wasSelfInheritanceParent)
	{
		if (!SkipNext(fo.InheritanceBehavior) || fMetadata.OverridesInheritanceBehavior)
		{
			InheritablePropertyChangeInfo rootInheritableValue = info.GetRootInheritableValue(dp);
			EffectiveValueEntry oldEntry = rootInheritableValue.OldEntry;
			EffectiveValueEntry newEntry = (info.IsAddOperation ? rootInheritableValue.NewEntry : new EffectiveValueEntry(dp, BaseValueSourceInternal.Inherited));
			bool flag = IsForceInheritedProperty(dp);
			if (d != info.Root)
			{
				if (wasSelfInheritanceParent)
				{
					oldEntry = d.GetValueEntry(d.LookupEntry(dp.GlobalIndex), dp, fMetadata, RequestFlags.DeferredReferences);
				}
				else if (isSelfInheritanceParent)
				{
					EffectiveValueEntry valueEntry = d.GetValueEntry(d.LookupEntry(dp.GlobalIndex), dp, fMetadata, RequestFlags.DeferredReferences);
					if (valueEntry.BaseValueSourceInternal <= BaseValueSourceInternal.Inherited)
					{
						oldEntry = oldEntry.GetFlattenedEntry(RequestFlags.FullyResolved);
						oldEntry.BaseValueSourceInternal = BaseValueSourceInternal.Inherited;
					}
					else
					{
						oldEntry = valueEntry;
					}
				}
				else
				{
					oldEntry = oldEntry.GetFlattenedEntry(RequestFlags.FullyResolved);
					oldEntry.BaseValueSourceInternal = BaseValueSourceInternal.Inherited;
				}
			}
			else if (info.IsAddOperation && (flag || oldEntry.BaseValueSourceInternal <= BaseValueSourceInternal.Inherited))
			{
				EffectiveValueEntry valueEntry2 = d.GetValueEntry(d.LookupEntry(dp.GlobalIndex), dp, fMetadata, RequestFlags.DeferredReferences);
				if (valueEntry2.BaseValueSourceInternal > BaseValueSourceInternal.Inherited)
				{
					oldEntry = valueEntry2;
				}
			}
			OperationType operationType = (info.IsAddOperation ? OperationType.AddChild : OperationType.RemoveChild);
			if (BaseValueSourceInternal.Inherited >= oldEntry.BaseValueSourceInternal)
			{
				return (d.UpdateEffectiveValue(d.LookupEntry(dp.GlobalIndex), dp, fMetadata, oldEntry, ref newEntry, coerceWithDeferredReference: false, coerceWithCurrentValue: false, operationType) & (UpdateResult)5) == UpdateResult.ValueChanged;
			}
			if (flag)
			{
				newEntry = new EffectiveValueEntry(dp, FullValueSource.IsCoerced);
				return (d.UpdateEffectiveValue(d.LookupEntry(dp.GlobalIndex), dp, fMetadata, oldEntry, ref newEntry, coerceWithDeferredReference: false, coerceWithCurrentValue: false, operationType) & (UpdateResult)5) == UpdateResult.ValueChanged;
			}
		}
		return false;
	}

	internal static void InvalidateOnResourcesChange(FrameworkElement fe, FrameworkContentElement fce, ResourcesChangeInfo info)
	{
		FrameworkObject frameworkObject = new FrameworkObject(fe, fce);
		frameworkObject.Reset(frameworkObject.TemplatedParent);
		frameworkObject.HasTemplateChanged = false;
		DependencyObject dependencyObject = ((fe != null) ? ((DependencyObject)fe) : ((DependencyObject)fce));
		if (HasChildren(fe, fce))
		{
			new DescendentsWalker<ResourcesChangeInfo>(TreeWalkPriority.LogicalTree, ResourcesChangeDelegate, info).StartWalk(dependencyObject);
		}
		else
		{
			OnResourcesChanged(dependencyObject, info, raiseResourceChangedEvent: true);
		}
	}

	private static bool OnResourcesChangedCallback(DependencyObject d, ResourcesChangeInfo info, bool visitedViaVisualTree)
	{
		OnResourcesChanged(d, info, raiseResourceChangedEvent: true);
		return true;
	}

	internal static void OnResourcesChanged(DependencyObject d, ResourcesChangeInfo info, bool raiseResourceChangedEvent)
	{
		bool flag = info.Contains(d.DependencyObjectType.SystemType, isImplicitStyleKey: true);
		bool isThemeChange = info.IsThemeChange;
		bool isStyleResourcesChange = info.IsStyleResourcesChange;
		bool isTemplateResourcesChange = info.IsTemplateResourcesChange;
		bool flag2 = info.Container == d;
		FrameworkObject frameworkObject = new FrameworkObject(d);
		if (info.IsResourceAddOperation || info.IsCatastrophicDictionaryChange)
		{
			frameworkObject.SetShouldLookupImplicitStyles();
		}
		if (frameworkObject.IsFE)
		{
			FrameworkElement fE = frameworkObject.FE;
			fE.HasStyleChanged = false;
			fE.HasStyleInvalidated = false;
			fE.HasTemplateChanged = false;
			if (info.IsImplicitDataTemplateChange && fE is ContentPresenter contentPresenter)
			{
				contentPresenter.ReevaluateTemplate();
			}
			if (fE.HasResourceReference)
			{
				InvalidateResourceReferences(fE, info);
				if ((!isStyleResourcesChange && !isTemplateResourcesChange) || !flag2)
				{
					InvalidateStyleAndReferences(d, info, flag);
				}
			}
			else if (flag && (fE.HasImplicitStyleFromResources || fE.Style == FrameworkElement.StyleProperty.GetMetadata(fE.DependencyObjectType).DefaultValue) && (!isStyleResourcesChange || !flag2))
			{
				fE.UpdateStyleProperty();
			}
			if (isThemeChange)
			{
				fE.UpdateThemeStyleProperty();
			}
			if (raiseResourceChangedEvent && fE.PotentiallyHasMentees)
			{
				fE.RaiseClrEvent(FrameworkElement.ResourcesChangedKey, new ResourcesChangedEventArgs(info));
			}
			return;
		}
		FrameworkContentElement fCE = frameworkObject.FCE;
		fCE.HasStyleChanged = false;
		fCE.HasStyleInvalidated = false;
		if (fCE.HasResourceReference)
		{
			InvalidateResourceReferences(fCE, info);
			if ((!isStyleResourcesChange && !isTemplateResourcesChange) || !flag2)
			{
				InvalidateStyleAndReferences(d, info, flag);
			}
		}
		else if (flag && (fCE.HasImplicitStyleFromResources || fCE.Style == FrameworkContentElement.StyleProperty.GetMetadata(fCE.DependencyObjectType).DefaultValue) && (!isStyleResourcesChange || !flag2))
		{
			fCE.UpdateStyleProperty();
		}
		if (isThemeChange)
		{
			fCE.UpdateThemeStyleProperty();
		}
		if (raiseResourceChangedEvent && fCE.PotentiallyHasMentees)
		{
			fCE.RaiseClrEvent(FrameworkElement.ResourcesChangedKey, new ResourcesChangedEventArgs(info));
		}
	}

	private static void InvalidateResourceReferences(DependencyObject d, ResourcesChangeInfo info)
	{
		LocalValueEnumerator localValueEnumerator = d.GetLocalValueEnumerator();
		int count = localValueEnumerator.Count;
		if (count <= 0)
		{
			return;
		}
		ResourceReferenceExpression[] array = new ResourceReferenceExpression[count];
		int num = 0;
		while (localValueEnumerator.MoveNext())
		{
			if (localValueEnumerator.Current.Value is ResourceReferenceExpression resourceReferenceExpression && info.Contains(resourceReferenceExpression.ResourceKey, isImplicitStyleKey: false))
			{
				array[num] = resourceReferenceExpression;
				num++;
			}
		}
		ResourcesChangedEventArgs e = new ResourcesChangedEventArgs(info);
		for (int i = 0; i < num; i++)
		{
			array[i].InvalidateExpressionValue(d, e);
		}
	}

	private static void InvalidateStyleAndReferences(DependencyObject d, ResourcesChangeInfo info, bool containsTypeOfKey)
	{
		FrameworkObject frameworkObject = new FrameworkObject(d);
		if (frameworkObject.IsFE)
		{
			FrameworkElement fE = frameworkObject.FE;
			if (containsTypeOfKey && !info.IsThemeChange && (fE.HasImplicitStyleFromResources || fE.Style == FrameworkElement.StyleProperty.GetMetadata(fE.DependencyObjectType).DefaultValue))
			{
				fE.UpdateStyleProperty();
			}
			if (fE.Style != null && fE.Style.HasResourceReferences && !fE.HasStyleChanged)
			{
				StyleHelper.InvalidateResourceDependents(d, info, ref fE.Style.ResourceDependents, invalidateVisualTreeToo: false);
			}
			if (fE.TemplateInternal != null && fE.TemplateInternal.HasContainerResourceReferences)
			{
				StyleHelper.InvalidateResourceDependents(d, info, ref fE.TemplateInternal.ResourceDependents, invalidateVisualTreeToo: false);
			}
			if (fE.TemplateChildIndex > 0)
			{
				FrameworkElement frameworkElement = (FrameworkElement)fE.TemplatedParent;
				FrameworkTemplate templateInternal = frameworkElement.TemplateInternal;
				if (!frameworkElement.HasTemplateChanged && templateInternal.HasChildResourceReferences)
				{
					StyleHelper.InvalidateResourceDependentsForChild(frameworkElement, fE, fE.TemplateChildIndex, info, templateInternal);
				}
			}
			if (!info.IsThemeChange)
			{
				Style themeStyle = fE.ThemeStyle;
				if (themeStyle != null && themeStyle.HasResourceReferences && themeStyle != fE.Style)
				{
					StyleHelper.InvalidateResourceDependents(d, info, ref themeStyle.ResourceDependents, invalidateVisualTreeToo: false);
				}
			}
		}
		else
		{
			if (!frameworkObject.IsFCE)
			{
				return;
			}
			FrameworkContentElement fCE = frameworkObject.FCE;
			if (containsTypeOfKey && !info.IsThemeChange && (fCE.HasImplicitStyleFromResources || fCE.Style == FrameworkContentElement.StyleProperty.GetMetadata(fCE.DependencyObjectType).DefaultValue))
			{
				fCE.UpdateStyleProperty();
			}
			if (fCE.Style != null && fCE.Style.HasResourceReferences && !fCE.HasStyleChanged)
			{
				StyleHelper.InvalidateResourceDependents(d, info, ref fCE.Style.ResourceDependents, invalidateVisualTreeToo: true);
			}
			if (fCE.TemplateChildIndex > 0)
			{
				FrameworkElement frameworkElement2 = (FrameworkElement)fCE.TemplatedParent;
				FrameworkTemplate templateInternal2 = frameworkElement2.TemplateInternal;
				if (!frameworkElement2.HasTemplateChanged && templateInternal2.HasChildResourceReferences)
				{
					StyleHelper.InvalidateResourceDependentsForChild(frameworkElement2, fCE, fCE.TemplateChildIndex, info, templateInternal2);
				}
			}
			if (!info.IsThemeChange)
			{
				Style themeStyle2 = fCE.ThemeStyle;
				if (themeStyle2 != null && themeStyle2.HasResourceReferences && themeStyle2 != fCE.Style)
				{
					StyleHelper.InvalidateResourceDependents(d, info, ref themeStyle2.ResourceDependents, invalidateVisualTreeToo: false);
				}
			}
		}
	}

	internal static void InvalidateOnInheritablePropertyChange(FrameworkElement fe, FrameworkContentElement fce, InheritablePropertyChangeInfo info, bool skipStartNode)
	{
		_ = info.Property;
		FrameworkObject frameworkObject = new FrameworkObject(fe, fce);
		if (HasChildren(fe, fce))
		{
			DependencyObject dO = frameworkObject.DO;
			new DescendentsWalker<InheritablePropertyChangeInfo>(TreeWalkPriority.LogicalTree, InheritablePropertyChangeDelegate, info).StartWalk(dO, skipStartNode);
		}
		else if (!skipStartNode)
		{
			bool visitedViaVisualTree = false;
			OnInheritablePropertyChanged(frameworkObject.DO, info, visitedViaVisualTree);
		}
	}

	private static bool OnInheritablePropertyChanged(DependencyObject d, InheritablePropertyChangeInfo info, bool visitedViaVisualTree)
	{
		DependencyProperty property = info.Property;
		EffectiveValueEntry oldEntry = info.OldEntry;
		EffectiveValueEntry newEntry = info.NewEntry;
		InheritanceBehavior inheritanceBehavior;
		bool num = IsInheritanceNode(d, property, out inheritanceBehavior);
		bool flag = IsForceInheritedProperty(property);
		if (num && (!SkipNext(inheritanceBehavior) || flag))
		{
			PropertyMetadata metadata = property.GetMetadata(d);
			EntryIndex entryIndex = d.LookupEntry(property.GlobalIndex);
			if (!d.IsSelfInheritanceParent)
			{
				DependencyObject frameworkParent = FrameworkElement.GetFrameworkParent(d);
				InheritanceBehavior inheritanceBehavior2 = InheritanceBehavior.Default;
				if (frameworkParent != null)
				{
					inheritanceBehavior2 = new FrameworkObject(frameworkParent, throwIfNeither: true).InheritanceBehavior;
				}
				if (!SkipNext(inheritanceBehavior) && !SkipNow(inheritanceBehavior2))
				{
					d.SynchronizeInheritanceParent(frameworkParent);
				}
				if (oldEntry.BaseValueSourceInternal == BaseValueSourceInternal.Unknown)
				{
					oldEntry = EffectiveValueEntry.CreateDefaultValueEntry(property, metadata.GetDefaultValue(d, property));
				}
			}
			else
			{
				oldEntry = d.GetValueEntry(entryIndex, property, metadata, RequestFlags.RawEntry);
			}
			if (BaseValueSourceInternal.Inherited >= oldEntry.BaseValueSourceInternal)
			{
				if (visitedViaVisualTree && d is FrameworkElement)
				{
					DependencyObject parent = LogicalTreeHelper.GetParent(d);
					if (parent != null)
					{
						DependencyObject parent2 = VisualTreeHelper.GetParent(d);
						if (parent2 != null && parent2 != parent)
						{
							return false;
						}
					}
				}
				return (d.UpdateEffectiveValue(entryIndex, property, metadata, oldEntry, ref newEntry, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.Inherit) & (UpdateResult)5) == UpdateResult.ValueChanged;
			}
			if (flag)
			{
				newEntry = new EffectiveValueEntry(property, FullValueSource.IsCoerced);
				return (d.UpdateEffectiveValue(d.LookupEntry(property.GlobalIndex), property, metadata, oldEntry, ref newEntry, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.Inherit) & (UpdateResult)5) == UpdateResult.ValueChanged;
			}
			return false;
		}
		return inheritanceBehavior == InheritanceBehavior.Default || flag;
	}

	internal static void OnInheritedPropertyChanged(DependencyObject d, ref InheritablePropertyChangeInfo info, InheritanceBehavior inheritanceBehavior)
	{
		if (inheritanceBehavior == InheritanceBehavior.Default || IsForceInheritedProperty(info.Property))
		{
			new FrameworkObject(d).OnInheritedPropertyChanged(ref info);
		}
	}

	internal static bool IsInheritanceNode(DependencyObject d, DependencyProperty dp, out InheritanceBehavior inheritanceBehavior)
	{
		inheritanceBehavior = InheritanceBehavior.Default;
		if (dp.GetMetadata(d.DependencyObjectType) is FrameworkPropertyMetadata frameworkPropertyMetadata)
		{
			FrameworkObject frameworkObject = new FrameworkObject(d);
			if (!frameworkObject.IsValid)
			{
				return false;
			}
			if (frameworkObject.InheritanceBehavior != 0 && !frameworkPropertyMetadata.OverridesInheritanceBehavior)
			{
				inheritanceBehavior = frameworkObject.InheritanceBehavior;
			}
			if (frameworkPropertyMetadata.Inherits)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsInheritanceNode(FrameworkElement fe, DependencyProperty dp, out InheritanceBehavior inheritanceBehavior)
	{
		inheritanceBehavior = InheritanceBehavior.Default;
		if (dp.GetMetadata(fe.DependencyObjectType) is FrameworkPropertyMetadata frameworkPropertyMetadata)
		{
			if (fe.InheritanceBehavior != 0 && !frameworkPropertyMetadata.OverridesInheritanceBehavior)
			{
				inheritanceBehavior = fe.InheritanceBehavior;
			}
			return frameworkPropertyMetadata.Inherits;
		}
		return false;
	}

	internal static bool IsInheritanceNode(FrameworkContentElement fce, DependencyProperty dp, out InheritanceBehavior inheritanceBehavior)
	{
		inheritanceBehavior = InheritanceBehavior.Default;
		if (dp.GetMetadata(fce.DependencyObjectType) is FrameworkPropertyMetadata frameworkPropertyMetadata)
		{
			if (fce.InheritanceBehavior != 0 && !frameworkPropertyMetadata.OverridesInheritanceBehavior)
			{
				inheritanceBehavior = fce.InheritanceBehavior;
			}
			return frameworkPropertyMetadata.Inherits;
		}
		return false;
	}

	internal static bool SkipNow(InheritanceBehavior inheritanceBehavior)
	{
		if (inheritanceBehavior == InheritanceBehavior.SkipToAppNow || inheritanceBehavior == InheritanceBehavior.SkipToThemeNow || inheritanceBehavior == InheritanceBehavior.SkipAllNow)
		{
			return true;
		}
		return false;
	}

	internal static bool SkipNext(InheritanceBehavior inheritanceBehavior)
	{
		if (inheritanceBehavior == InheritanceBehavior.SkipToAppNext || inheritanceBehavior == InheritanceBehavior.SkipToThemeNext || inheritanceBehavior == InheritanceBehavior.SkipAllNext)
		{
			return true;
		}
		return false;
	}

	internal static bool HasChildren(FrameworkElement fe, FrameworkContentElement fce)
	{
		if (fe == null || (!fe.HasLogicalChildren && !fe.HasVisualChildren && Popup.RegisteredPopupsField.GetValue(fe) == null))
		{
			return fce?.HasLogicalChildren ?? false;
		}
		return true;
	}

	private static bool IsForceInheritedProperty(DependencyProperty dp)
	{
		return dp == FrameworkElement.FlowDirectionProperty;
	}
}
