using System.Diagnostics;
using System.Dynamic.Utils;
using System.Runtime.CompilerServices;
using Unity;

namespace System.Linq.Expressions;

/// <summary>Represents an operation between an expression and a type.</summary>
[DebuggerTypeProxy(typeof(TypeBinaryExpressionProxy))]
public sealed class TypeBinaryExpression : Expression
{
	/// <summary>Gets the static type of the expression that this <see cref="P:System.Linq.Expressions.TypeBinaryExpression.Expression" /> represents.</summary>
	/// <returns>The <see cref="P:System.Linq.Expressions.TypeBinaryExpression.Type" /> that represents the static type of the expression.</returns>
	public sealed override Type Type => typeof(bool);

	/// <summary>Returns the node type of this Expression. Extension nodes should return <see cref="F:System.Linq.Expressions.ExpressionType.Extension" /> when overriding this method.</summary>
	/// <returns>The <see cref="T:System.Linq.Expressions.ExpressionType" /> of the expression.</returns>
	public sealed override ExpressionType NodeType { get; }

	/// <summary>Gets the expression operand of a type test operation.</summary>
	/// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the expression operand of a type test operation.</returns>
	public Expression Expression { get; }

	/// <summary>Gets the type operand of a type test operation.</summary>
	/// <returns>A <see cref="T:System.Type" /> that represents the type operand of a type test operation.</returns>
	public Type TypeOperand { get; }

	internal TypeBinaryExpression(Expression expression, Type typeOperand, ExpressionType nodeType)
	{
		Expression = expression;
		TypeOperand = typeOperand;
		NodeType = nodeType;
	}

	internal Expression ReduceTypeEqual()
	{
		Type type = Expression.Type;
		if (type.IsValueType || TypeOperand.IsPointer)
		{
			if (type.IsNullableType())
			{
				if (type.GetNonNullableType() != TypeOperand.GetNonNullableType())
				{
					return Expression.Block(Expression, Utils.Constant(value: false));
				}
				return Expression.NotEqual(Expression, Expression.Constant(null, Expression.Type));
			}
			return Expression.Block(Expression, Utils.Constant(type == TypeOperand.GetNonNullableType()));
		}
		if (Expression.NodeType == ExpressionType.Constant)
		{
			return ReduceConstantTypeEqual();
		}
		if (Expression is ParameterExpression { IsByRef: false } parameterExpression)
		{
			return ByValParameterTypeEqual(parameterExpression);
		}
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object));
		return Expression.Block(new TrueReadOnlyCollection<ParameterExpression>(parameterExpression2), new TrueReadOnlyCollection<Expression>(Expression.Assign(parameterExpression2, Expression), ByValParameterTypeEqual(parameterExpression2)));
	}

	private Expression ByValParameterTypeEqual(ParameterExpression value)
	{
		Expression expression = Expression.Call(value, CachedReflectionInfo.Object_GetType);
		if (TypeOperand.IsInterface)
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(Type));
			expression = Expression.Block(new TrueReadOnlyCollection<ParameterExpression>(parameterExpression), new TrueReadOnlyCollection<Expression>(Expression.Assign(parameterExpression, expression), parameterExpression));
		}
		return Expression.AndAlso(Expression.ReferenceNotEqual(value, Utils.Null), Expression.ReferenceEqual(expression, Expression.Constant(TypeOperand.GetNonNullableType(), typeof(Type))));
	}

	private Expression ReduceConstantTypeEqual()
	{
		ConstantExpression constantExpression = Expression as ConstantExpression;
		if (constantExpression.Value == null)
		{
			return Utils.Constant(value: false);
		}
		return Utils.Constant(TypeOperand.GetNonNullableType() == constantExpression.Value.GetType());
	}

	protected internal override Expression Accept(ExpressionVisitor visitor)
	{
		return visitor.VisitTypeBinary(this);
	}

	/// <summary>Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will return this expression.</summary>
	/// <returns>This expression if no children are changed or an expression with the updated children.</returns>
	/// <param name="expression">The <see cref="P:System.Linq.Expressions.TypeBinaryExpression.Expression" /> property of the result.</param>
	public TypeBinaryExpression Update(Expression expression)
	{
		if (expression == Expression)
		{
			return this;
		}
		if (NodeType == ExpressionType.TypeIs)
		{
			return Expression.TypeIs(expression, TypeOperand);
		}
		return Expression.TypeEqual(expression, TypeOperand);
	}

	internal TypeBinaryExpression()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
