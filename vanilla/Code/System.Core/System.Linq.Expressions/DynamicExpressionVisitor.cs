using System.Dynamic.Utils;

namespace System.Linq.Expressions;

/// <summary>Represents a visitor or rewriter for dynamic expression trees.</summary>
public class DynamicExpressionVisitor : ExpressionVisitor
{
	/// <summary>Visits the children of the <see cref="T:System.Linq.Expressions.DynamicExpression" />.</summary>
	/// <returns>Returns <see cref="T:System.Linq.Expressions.Expression" />, the modified expression, if it or any subexpression is modified; otherwise, returns the original expression.</returns>
	/// <param name="node">The expression to visit.</param>
	protected internal override Expression VisitDynamic(DynamicExpression node)
	{
		Expression[] array = ExpressionVisitorUtils.VisitArguments(this, node);
		if (array == null)
		{
			return node;
		}
		return node.Rewrite(array);
	}

	/// <summary>Initializes a new instance of <see cref="T:System.Linq.Expressions.DynamicExpressionVisitor" />.</summary>
	public DynamicExpressionVisitor()
	{
	}
}
