using MS.Internal;

namespace System.Windows;

/// <summary>Describes an action to perform for a trigger.</summary>
public abstract class TriggerAction : DependencyObject
{
	private TriggerBase _containingTrigger;

	private DependencyObject _inheritanceContext;

	private bool _hasMultipleInheritanceContexts;

	internal TriggerBase ContainingTrigger => _containingTrigger;

	internal override DependencyObject InheritanceContext => _inheritanceContext;

	internal override bool HasMultipleInheritanceContexts => _hasMultipleInheritanceContexts;

	internal TriggerAction()
	{
	}

	internal abstract void Invoke(FrameworkElement fe, FrameworkContentElement fce, Style targetStyle, FrameworkTemplate targetTemplate, long layer);

	internal abstract void Invoke(FrameworkElement fe);

	internal void Seal(TriggerBase containingTrigger)
	{
		if (base.IsSealed && containingTrigger != _containingTrigger)
		{
			throw new InvalidOperationException(SR.TriggerActionMustBelongToASingleTrigger);
		}
		_containingTrigger = containingTrigger;
		Seal();
	}

	internal override void Seal()
	{
		if (base.IsSealed)
		{
			throw new InvalidOperationException(SR.TriggerActionAlreadySealed);
		}
		base.Seal();
	}

	internal void CheckSealed()
	{
		if (base.IsSealed)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "TriggerAction"));
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
}
