using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows.Media.Animation;

/// <summary>Manipulates a <see cref="T:System.Windows.Media.Animation.Storyboard" /> that has been applied by a <see cref="T:System.Windows.Media.Animation.BeginStoryboard" /> action.</summary>
public abstract class ControllableStoryboardAction : TriggerAction
{
	private string _beginStoryboardName;

	/// <summary>Gets or sets the <see cref="P:System.Windows.Media.Animation.BeginStoryboard.Name" /> of the <see cref="T:System.Windows.Media.Animation.BeginStoryboard" /> that began the <see cref="T:System.Windows.Media.Animation.Storyboard" /> you want to interactively control. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.Animation.BeginStoryboard.Name" /> of the <see cref="T:System.Windows.Media.Animation.BeginStoryboard" /> that began the <see cref="T:System.Windows.Media.Animation.Storyboard" /> you want to interactively control. The default value is null.</returns>
	[DefaultValue(null)]
	public string BeginStoryboardName
	{
		get
		{
			return _beginStoryboardName;
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "ControllableStoryboardAction"));
			}
			_beginStoryboardName = value;
		}
	}

	internal ControllableStoryboardAction()
	{
	}

	internal sealed override void Invoke(FrameworkElement fe, FrameworkContentElement fce, Style targetStyle, FrameworkTemplate frameworkTemplate, long layer)
	{
		INameScope nameScope = null;
		nameScope = ((targetStyle == null) ? ((INameScope)frameworkTemplate) : ((INameScope)targetStyle));
		Invoke(fe, fce, GetStoryboard(fe, fce, nameScope));
	}

	internal sealed override void Invoke(FrameworkElement fe)
	{
		Invoke(fe, null, GetStoryboard(fe, null, null));
	}

	internal virtual void Invoke(FrameworkElement containingFE, FrameworkContentElement containingFCE, Storyboard storyboard)
	{
	}

	private Storyboard GetStoryboard(FrameworkElement fe, FrameworkContentElement fce, INameScope nameScope)
	{
		if (BeginStoryboardName == null)
		{
			throw new InvalidOperationException(SR.Storyboard_BeginStoryboardNameRequired);
		}
		return Storyboard.ResolveBeginStoryboardName(BeginStoryboardName, nameScope, fe, fce).Storyboard ?? throw new InvalidOperationException(SR.Format(SR.Storyboard_BeginStoryboardNoStoryboard, BeginStoryboardName));
	}
}
