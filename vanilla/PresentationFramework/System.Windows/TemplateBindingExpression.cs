using System.ComponentModel;

namespace System.Windows;

/// <summary>Describes a run-time instance of a <see cref="T:System.Windows.TemplateBindingExtension" />.</summary>
[TypeConverter(typeof(TemplateBindingExpressionConverter))]
public class TemplateBindingExpression : Expression
{
	private TemplateBindingExtension _templateBindingExtension;

	/// <summary>Gets the <see cref="T:System.Windows.TemplateBindingExtension" /> object of this expression instance.</summary>
	/// <returns>The template binding extension of this expression instance.</returns>
	public TemplateBindingExtension TemplateBindingExtension => _templateBindingExtension;

	internal TemplateBindingExpression(TemplateBindingExtension templateBindingExtension)
	{
		_templateBindingExtension = templateBindingExtension;
	}

	internal override object GetValue(DependencyObject d, DependencyProperty dp)
	{
		return dp.GetDefaultValue(d.DependencyObjectType);
	}
}
