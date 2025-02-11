using System.Collections.Generic;
using MS.Internal;
using MS.Utility;

namespace System.Windows;

internal struct TreeChangeInfo
{
	private Stack<FrugalObjectList<DependencyProperty>> _inheritablePropertiesStack;

	private object _topmostCollapsedParentNode;

	private bool _isAddOperation;

	private DependencyObject _rootOfChange;

	private InheritablePropertyChangeInfo[] _rootInheritableValues;

	private int _valueIndexer;

	internal Stack<FrugalObjectList<DependencyProperty>> InheritablePropertiesStack
	{
		get
		{
			if (_inheritablePropertiesStack == null)
			{
				_inheritablePropertiesStack = new Stack<FrugalObjectList<DependencyProperty>>(1);
			}
			return _inheritablePropertiesStack;
		}
	}

	internal object TopmostCollapsedParentNode
	{
		get
		{
			return _topmostCollapsedParentNode;
		}
		set
		{
			_topmostCollapsedParentNode = value;
		}
	}

	internal bool IsAddOperation => _isAddOperation;

	internal DependencyObject Root => _rootOfChange;

	public TreeChangeInfo(DependencyObject root, DependencyObject parent, bool isAddOperation)
	{
		_rootOfChange = root;
		_isAddOperation = isAddOperation;
		_topmostCollapsedParentNode = null;
		_rootInheritableValues = null;
		_inheritablePropertiesStack = null;
		_valueIndexer = 0;
		InheritablePropertiesStack.Push(CreateParentInheritableProperties(root, parent, isAddOperation));
	}

	internal FrugalObjectList<DependencyProperty> CreateParentInheritableProperties(DependencyObject d, DependencyObject parent, bool isAddOperation)
	{
		if (parent == null)
		{
			return new FrugalObjectList<DependencyProperty>(0);
		}
		DependencyObjectType dependencyObjectType = d.DependencyObjectType;
		EffectiveValueEntry[] array = null;
		uint num = 0u;
		uint num2 = 0u;
		if (!parent.IsSelfInheritanceParent)
		{
			DependencyObject inheritanceParent = parent.InheritanceParent;
			if (inheritanceParent != null)
			{
				array = inheritanceParent.EffectiveValues;
				num = inheritanceParent.EffectiveValuesCount;
				num2 = inheritanceParent.InheritableEffectiveValuesCount;
			}
		}
		else
		{
			array = parent.EffectiveValues;
			num = parent.EffectiveValuesCount;
			num2 = parent.InheritableEffectiveValuesCount;
		}
		FrugalObjectList<DependencyProperty> frugalObjectList = new FrugalObjectList<DependencyProperty>((int)num2);
		if (num2 == 0)
		{
			return frugalObjectList;
		}
		_rootInheritableValues = new InheritablePropertyChangeInfo[num2];
		int num3 = 0;
		FrameworkObject frameworkObject = new FrameworkObject(parent);
		for (uint num4 = 0u; num4 < num; num4++)
		{
			EffectiveValueEntry effectiveValueEntry = array[num4];
			DependencyProperty dependencyProperty = DependencyProperty.RegisteredPropertyList.List[effectiveValueEntry.PropertyIndex];
			if (dependencyProperty == null || !dependencyProperty.IsPotentiallyInherited)
			{
				continue;
			}
			PropertyMetadata metadata = dependencyProperty.GetMetadata(parent.DependencyObjectType);
			if (metadata == null || !metadata.IsInherited)
			{
				continue;
			}
			FrameworkPropertyMetadata frameworkPropertyMetadata = (FrameworkPropertyMetadata)metadata;
			if (TreeWalkHelper.SkipNow(frameworkObject.InheritanceBehavior) && !frameworkPropertyMetadata.OverridesInheritanceBehavior)
			{
				continue;
			}
			frugalObjectList.Add(dependencyProperty);
			EffectiveValueEntry valueEntry = d.GetValueEntry(d.LookupEntry(dependencyProperty.GlobalIndex), dependencyProperty, dependencyProperty.GetMetadata(dependencyObjectType), RequestFlags.DeferredReferences);
			EffectiveValueEntry newEntry;
			if (isAddOperation)
			{
				newEntry = effectiveValueEntry;
				if (newEntry.BaseValueSourceInternal != BaseValueSourceInternal.Default || newEntry.HasModifiers)
				{
					newEntry = newEntry.GetFlattenedEntry(RequestFlags.FullyResolved);
					newEntry.BaseValueSourceInternal = BaseValueSourceInternal.Inherited;
				}
			}
			else
			{
				newEntry = default(EffectiveValueEntry);
			}
			_rootInheritableValues[num3++] = new InheritablePropertyChangeInfo(d, dependencyProperty, valueEntry, newEntry);
			if (num2 == num3)
			{
				break;
			}
		}
		return frugalObjectList;
	}

	internal void ResetInheritableValueIndexer()
	{
		_valueIndexer = 0;
	}

	internal InheritablePropertyChangeInfo GetRootInheritableValue(DependencyProperty dp)
	{
		InheritablePropertyChangeInfo result;
		do
		{
			result = _rootInheritableValues[_valueIndexer++];
		}
		while (result.Property != dp);
		return result;
	}
}
