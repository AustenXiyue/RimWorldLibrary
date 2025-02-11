using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Media.Animation;
using MS.Internal;
using MS.Utility;

namespace System.Windows;

/// <summary>Represents the base class for specifying a conditional value within a <see cref="T:System.Windows.Style" /> object. </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public abstract class TriggerBase : DependencyObject
{
	internal FrugalStructList<PropertyValue> PropertyValues;

	private static readonly object Synchronized = new object();

	private TriggerCondition[] _triggerConditions;

	private DependencyObject _inheritanceContext;

	private bool _hasMultipleInheritanceContexts;

	private TriggerActionCollection _enterActions;

	private TriggerActionCollection _exitActions;

	private long _globalLayerRank;

	private static long _nextGlobalLayerRank = Storyboard.Layers.PropertyTriggerStartLayer;

	/// <summary>Gets a collection of <see cref="T:System.Windows.TriggerAction" /> objects to apply when the trigger object becomes active. This property does not apply to the <see cref="T:System.Windows.EventTrigger" /> class.</summary>
	/// <returns>The default value is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public TriggerActionCollection EnterActions
	{
		get
		{
			VerifyAccess();
			if (_enterActions == null)
			{
				_enterActions = new TriggerActionCollection();
				if (base.IsSealed)
				{
					_enterActions.Seal(this);
				}
			}
			return _enterActions;
		}
	}

	internal bool HasEnterActions
	{
		get
		{
			if (_enterActions != null)
			{
				return _enterActions.Count > 0;
			}
			return false;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Windows.TriggerAction" /> objects to apply when the trigger object becomes inactive. This property does not apply to the <see cref="T:System.Windows.EventTrigger" /> class.</summary>
	/// <returns>The default value is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public TriggerActionCollection ExitActions
	{
		get
		{
			VerifyAccess();
			if (_exitActions == null)
			{
				_exitActions = new TriggerActionCollection();
				if (base.IsSealed)
				{
					_exitActions.Seal(this);
				}
			}
			return _exitActions;
		}
	}

	internal bool HasExitActions
	{
		get
		{
			if (_exitActions != null)
			{
				return _exitActions.Count > 0;
			}
			return false;
		}
	}

	internal bool ExecuteEnterActionsOnApply => true;

	internal bool ExecuteExitActionsOnApply => false;

	internal override DependencyObject InheritanceContext => _inheritanceContext;

	internal override bool HasMultipleInheritanceContexts => _hasMultipleInheritanceContexts;

	internal long Layer => _globalLayerRank;

	internal TriggerCondition[] TriggerConditions
	{
		get
		{
			return _triggerConditions;
		}
		set
		{
			_triggerConditions = value;
		}
	}

	internal TriggerBase()
	{
	}

	internal void ProcessParametersContainer(DependencyProperty dp)
	{
		if (dp == FrameworkElement.StyleProperty)
		{
			throw new ArgumentException(SR.StylePropertyInStyleNotAllowed);
		}
	}

	internal string ProcessParametersVisualTreeChild(DependencyProperty dp, string target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (target.Length == 0)
		{
			throw new ArgumentException(SR.ChildNameMustBeNonEmpty);
		}
		return string.Intern(target);
	}

	internal void AddToPropertyValues(string childName, DependencyProperty dp, object value, PropertyValueType valueType)
	{
		PropertyValue value2 = default(PropertyValue);
		value2.ValueType = valueType;
		value2.Conditions = null;
		value2.ChildName = childName;
		value2.Property = dp;
		value2.ValueInternal = value;
		PropertyValues.Add(value2);
	}

	internal override void Seal()
	{
		VerifyAccess();
		base.Seal();
		for (int i = 0; i < PropertyValues.Count; i++)
		{
			PropertyValue propertyValue = PropertyValues[i];
			DependencyProperty property = propertyValue.Property;
			for (int j = 0; j < propertyValue.Conditions.Length; j++)
			{
				DependencyProperty property2 = propertyValue.Conditions[j].Property;
				if (property2 == property && propertyValue.ChildName == "~Self")
				{
					throw new InvalidOperationException(SR.Format(SR.PropertyTriggerCycleDetected, property2.Name));
				}
			}
		}
		if (_enterActions != null)
		{
			_enterActions.Seal(this);
		}
		if (_exitActions != null)
		{
			_exitActions.Seal(this);
		}
		DetachFromDispatcher();
	}

	internal void ProcessSettersCollection(SetterBaseCollection setters)
	{
		if (setters == null)
		{
			return;
		}
		setters.Seal();
		for (int i = 0; i < setters.Count; i++)
		{
			if (setters[i] is Setter { Property: var property, ValueInternal: var valueInternal, TargetName: var targetName })
			{
				string childName;
				if (targetName == null)
				{
					ProcessParametersContainer(property);
					childName = "~Self";
				}
				else
				{
					childName = ProcessParametersVisualTreeChild(property, targetName);
				}
				if (!(valueInternal is DynamicResourceExtension dynamicResourceExtension))
				{
					AddToPropertyValues(childName, property, valueInternal, PropertyValueType.Trigger);
				}
				else
				{
					AddToPropertyValues(childName, property, dynamicResourceExtension.ResourceKey, PropertyValueType.PropertyTriggerResource);
				}
				continue;
			}
			throw new InvalidOperationException(SR.Format(SR.VisualTriggerSettersIncludeUnsupportedSetterType, setters[i].GetType().Name));
		}
	}

	internal override void AddInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		InheritanceContextHelper.AddInheritanceContext(context, this, ref _hasMultipleInheritanceContexts, ref _inheritanceContext);
	}

	internal override void RemoveInheritanceContext(DependencyObject context, DependencyProperty property)
	{
		InheritanceContextHelper.RemoveInheritanceContext(context, this, ref _hasMultipleInheritanceContexts, ref _inheritanceContext);
	}

	internal void EstablishLayer()
	{
		if (_globalLayerRank == 0L)
		{
			lock (Synchronized)
			{
				_globalLayerRank = _nextGlobalLayerRank++;
			}
			if (_nextGlobalLayerRank == long.MaxValue)
			{
				throw new InvalidOperationException(SR.PropertyTriggerLayerLimitExceeded);
			}
		}
	}

	internal virtual bool GetCurrentState(DependencyObject container, UncommonField<HybridDictionary[]> dataField)
	{
		return false;
	}
}
