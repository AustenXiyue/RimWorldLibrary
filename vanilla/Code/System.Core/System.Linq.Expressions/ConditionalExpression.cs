using System.Diagnostics;
using Unity;

namespace System.Linq.Expressions;

/// <summary>Represents an expression that has a conditional operator.</summary>
[DebuggerTypeProxy(typeof(ConditionalExpressionProxy))]
public class ConditionalExpression : Expression
{
	/// <summary>Returns the node type of this expression. Extension nodes should return <see cref="F:System.Linq.Expressions.ExpressionType.Extension" /> when overriding this method.</summary>
	/// <returns>The <see cref="T:System.Linq.Expressions.ExpressionType" /> of the expression.</returns>
	public sealed override ExpressionType NodeType => ExpressionType.Conditional;

	/// <summary>Gets the static type of the expression that this <see cref="T:System.Linq.Expressions.Expression" /> represents.</summary>
	/// <returns>The <see cref="P:System.Linq.Expressions.ConditionalExpression.Type" /> that represents the static type of the expression.</returns>
	public override Type Type => IfTrue.Type;

	/// <summary>Gets the test of the conditional operation.</summary>
	/// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the test of the conditional operation.</returns>
	public Expression Test { get; }

	/// <summary>Gets the expression to execute if the test evaluates to true.</summary>
	/// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the expression to execute if the test is true.</returns>
	public Expression IfTrue { get; }

	/// <summary>Gets the expression to execute if the test evaluates to false.</summary>
	/// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the expression to execute if the test is false.</returns>
	public Expression IfFalse => GetFalse();

	internal ConditionalExpression(Expression test, Expression ifTrue)
	{
		Test = test;
		IfTrue = ifTrue;
	}

	internal static ConditionalExpression Make(Expression test, Expression ifTrue, Expression ifFalse, Type type)
	{
		if (ifTrue.Type != type || ifFalse.Type != type)
		{
			return new FullConditionalExpressionWithType(test, ifTrue, ifFalse, type);
		}
		if (ifFalse is DefaultExpression && ifFalse.Type == typeof(void))
		{
			return new ConditionalExpression(test, ifTrue);
		}
		return new FullConditionalExpression(test, ifTrue, ifFalse);
	}

	internal virtual Expression GetFalse()
	{
		return Utils.Empty;
	}

	/// <summary>Dispatches to the specific visit method for this node type. For example, <see cref="T:System.Linq.Expressions.MethodCallExpression" /> calls the <see cref="M:System.Linq.Expressions.ExpressionVisitor.VisitMethodCall(System.Linq.Expressions.MethodCallExpression)" />.</summary>
	/// <returns>The result of visiting this node.</returns>
	/// <param name="visitor">The visitor to visit this node with.</param>
	protected internal override Expression Accept(ExpressionVisitor visitor)
	{
		return visitor.VisitConditional(this);
	}

	/// <summary>Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will return this expression</summary>
	/// <returns>This expression if no children changed, or an expression with the updated children.</returns>
	/// <param name="test">The <see cref="P:System.Linq.Expressions.ConditionalExpression.Test" /> property of the result.</param>
	/// <param name="ifTrue">The <see cref="P:System.Linq.Expressions.ConditionalExpression.IfTrue" /> property of the result.</param>
	/// <param name="ifFalse">The <see cref="P:System.Linq.Expressions.ConditionalExpression.IfFalse" /> property of the result.</param>
	public ConditionalExpression Update(Expression test, Expression ifTrue, Expression ifFalse)
	{
		if (test == Test && ifTrue == IfTrue && ifFalse == IfFalse)
		{
			return this;
		}
		return Expression.Condition(test, ifTrue, ifFalse, Type);
	}

	internal ConditionalExpression()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
