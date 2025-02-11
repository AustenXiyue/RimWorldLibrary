using System.Windows;
using System.Windows.Data;

namespace MS.Internal.Data;

internal class BindingExpressionUncommonField : UncommonField<BindingExpression>
{
	internal new void SetValue(DependencyObject instance, BindingExpression bindingExpr)
	{
		base.SetValue(instance, bindingExpr);
		bindingExpr.Attach(instance);
	}

	internal new void ClearValue(DependencyObject instance)
	{
		GetValue(instance)?.Detach();
		base.ClearValue(instance);
	}
}
