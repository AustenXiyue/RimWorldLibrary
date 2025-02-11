using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace System.Windows.Controls;

/// <summary>Specifies the visual structure and behavioral aspects of a <see cref="T:System.Windows.Controls.Control" /> that can be shared across multiple instances of the control.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
[DictionaryKeyProperty("TargetType")]
public class ControlTemplate : FrameworkTemplate
{
	private Type _targetType;

	private TriggerCollection _triggers;

	internal static readonly Type DefaultTargetType = typeof(Control);

	/// <summary>Gets or sets the type for which this <see cref="T:System.Windows.Controls.ControlTemplate" /> is intended.</summary>
	/// <returns>The default value is null.</returns>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Windows.Controls.ControlTemplate.TargetType" /> property must not be null if the definition of the template has a <see cref="T:System.Windows.Controls.ContentPresenter" />.</exception>
	/// <exception cref="T:System.ArgumentException">The specified types are not valid. The <see cref="P:System.Windows.Controls.ControlTemplate.TargetType" /> of a <see cref="T:System.Windows.Controls.ControlTemplate" /> must be or inherit from a <see cref="T:System.Windows.Controls.Control" />, a <see cref="T:System.Windows.Controls.Page" />, or a <see cref="T:System.Windows.Navigation.PageFunctionBase" />.</exception>
	[Ambient]
	[DefaultValue(null)]
	public Type TargetType
	{
		get
		{
			return _targetType;
		}
		set
		{
			ValidateTargetType(value, "value");
			CheckSealed();
			_targetType = value;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Windows.TriggerBase" /> objects that apply property changes or perform actions based on specified conditions.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.TriggerBase" /> objects. The default value is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[DependsOn("VisualTree")]
	[DependsOn("Template")]
	public TriggerCollection Triggers
	{
		get
		{
			if (_triggers == null)
			{
				_triggers = new TriggerCollection();
				if (base.IsSealed)
				{
					_triggers.Seal();
				}
			}
			return _triggers;
		}
	}

	internal override Type TargetTypeInternal
	{
		get
		{
			if (TargetType != null)
			{
				return TargetType;
			}
			return DefaultTargetType;
		}
	}

	internal override TriggerCollection TriggersInternal => Triggers;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ControlTemplate" /> class.</summary>
	public ControlTemplate()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ControlTemplate" /> class with the specified target type.</summary>
	/// <param name="targetType">The type this template is intended for.</param>
	public ControlTemplate(Type targetType)
	{
		ValidateTargetType(targetType, "targetType");
		_targetType = targetType;
	}

	/// <summary>Checks the templated parent against a set of rules.</summary>
	/// <param name="templatedParent">The element this template is applied to.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="templatedParent" /> must not be null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Windows.Controls.ControlTemplate.TargetType" /> of a <see cref="T:System.Windows.Controls.ControlTemplate" /> must match the type of the <see cref="T:System.Windows.Controls.Control" /> that it is being applied to.</exception>
	/// <exception cref="T:System.ArgumentException">You must associate the <see cref="T:System.Windows.Controls.ControlTemplate" /> with a <see cref="T:System.Windows.Controls.Control" /> by setting the <see cref="P:System.Windows.Controls.Control.Template" /> property before using the <see cref="T:System.Windows.Controls.ControlTemplate" /> on the <see cref="T:System.Windows.Controls.Control" />.</exception>
	protected override void ValidateTemplatedParent(FrameworkElement templatedParent)
	{
		if (templatedParent == null)
		{
			throw new ArgumentNullException("templatedParent");
		}
		if (_targetType != null && !_targetType.IsInstanceOfType(templatedParent))
		{
			throw new ArgumentException(SR.Format(SR.TemplateTargetTypeMismatch, _targetType.Name, templatedParent.GetType().Name));
		}
		if (templatedParent.TemplateInternal != this)
		{
			throw new ArgumentException(SR.MustNotTemplateUnassociatedControl);
		}
	}

	private void ValidateTargetType(Type targetType, string argName)
	{
		if (targetType == null)
		{
			throw new ArgumentNullException(argName);
		}
		if (!typeof(Control).IsAssignableFrom(targetType) && !typeof(Page).IsAssignableFrom(targetType) && !typeof(PageFunctionBase).IsAssignableFrom(targetType))
		{
			throw new ArgumentException(SR.Format(SR.InvalidControlTemplateTargetType, targetType.Name));
		}
	}

	internal override void SetTargetTypeInternal(Type targetType)
	{
		TargetType = targetType;
	}
}
