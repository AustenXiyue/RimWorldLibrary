using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows.Media.Animation;

/// <summary>A trigger action that begins a <see cref="T:System.Windows.Media.Animation.Storyboard" /> and distributes its animations to their targeted objects and properties.</summary>
[RuntimeNameProperty("Name")]
[ContentProperty("Storyboard")]
public sealed class BeginStoryboard : TriggerAction
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.BeginStoryboard.Storyboard" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Animation.BeginStoryboard.Storyboard" /> dependency property.</returns>
	public static readonly DependencyProperty StoryboardProperty = DependencyProperty.Register("Storyboard", typeof(Storyboard), typeof(BeginStoryboard));

	private HandoffBehavior _handoffBehavior;

	private string _name;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Animation.Storyboard" /> that this <see cref="T:System.Windows.Media.Animation.BeginStoryboard" /> starts. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Animation.Storyboard" /> that the <see cref="T:System.Windows.Media.Animation.BeginStoryboard" /> starts. The default is null.</returns>
	[DefaultValue(null)]
	public Storyboard Storyboard
	{
		get
		{
			return GetValue(StoryboardProperty) as Storyboard;
		}
		set
		{
			ThrowIfSealed();
			SetValue(StoryboardProperty, value);
		}
	}

	/// <summary>Gets or sets the proper hand-off behavior to start an animation clock in this storyboard </summary>
	/// <returns>One of the <see cref="T:System.Windows.Media.Animation.HandoffBehavior" /> enumeration values. The default value is <see cref="F:System.Windows.Media.Animation.HandoffBehavior.SnapshotAndReplace" />.</returns>
	[DefaultValue(HandoffBehavior.SnapshotAndReplace)]
	public HandoffBehavior HandoffBehavior
	{
		get
		{
			return _handoffBehavior;
		}
		set
		{
			ThrowIfSealed();
			if (HandoffBehaviorEnum.IsDefined(value))
			{
				_handoffBehavior = value;
				return;
			}
			throw new ArgumentException(SR.Storyboard_UnrecognizedHandoffBehavior);
		}
	}

	/// <summary>Gets or sets the name of the <see cref="T:System.Windows.Media.Animation.BeginStoryboard" /> object. By naming the <see cref="T:System.Windows.Media.Animation.BeginStoryboard" /> object, the <see cref="T:System.Windows.Media.Animation.Storyboard" /> can be controlled after it is started.</summary>
	/// <returns>The name of the <see cref="T:System.Windows.Media.Animation.BeginStoryboard" />. The default is null.</returns>
	[DefaultValue(null)]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			ThrowIfSealed();
			if (value != null && !NameValidationHelper.IsValidIdentifierName(value))
			{
				throw new ArgumentException(SR.Format(SR.InvalidPropertyValue, value, "Name"));
			}
			_name = value;
		}
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Windows.Media.Animation.BeginStoryboard" /> class. </summary>
	public BeginStoryboard()
	{
	}

	private void ThrowIfSealed()
	{
		if (base.IsSealed)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "BeginStoryboard"));
		}
	}

	internal override void Seal()
	{
		if (!base.IsSealed)
		{
			if (!(GetValue(StoryboardProperty) is Storyboard storyboard))
			{
				throw new InvalidOperationException(SR.Storyboard_StoryboardReferenceRequired);
			}
			if (!storyboard.CanFreeze)
			{
				throw new InvalidOperationException(SR.Storyboard_UnableToFreeze);
			}
			if (!storyboard.IsFrozen)
			{
				storyboard.Freeze();
			}
			Storyboard = storyboard;
		}
		base.Seal();
		DetachFromDispatcher();
	}

	internal sealed override void Invoke(FrameworkElement fe, FrameworkContentElement fce, Style targetStyle, FrameworkTemplate frameworkTemplate, long layer)
	{
		INameScope nameScope = null;
		Begin(nameScope: (targetStyle == null) ? ((INameScope)frameworkTemplate) : ((INameScope)targetStyle), targetObject: (fe != null) ? ((DependencyObject)fe) : ((DependencyObject)fce), layer: layer);
	}

	internal sealed override void Invoke(FrameworkElement fe)
	{
		Begin(fe, null, Storyboard.Layers.ElementEventTrigger);
	}

	private void Begin(DependencyObject targetObject, INameScope nameScope, long layer)
	{
		if (Storyboard == null)
		{
			throw new InvalidOperationException(SR.Storyboard_StoryboardReferenceRequired);
		}
		if (Name != null)
		{
			Storyboard.BeginCommon(targetObject, nameScope, _handoffBehavior, isControllable: true, layer);
		}
		else
		{
			Storyboard.BeginCommon(targetObject, nameScope, _handoffBehavior, isControllable: false, layer);
		}
	}
}
