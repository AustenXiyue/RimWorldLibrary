using System.Diagnostics;
using System.Dynamic.Utils;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity;

namespace System.Linq.Expressions;

/// <summary>Represents an expression that has a unary operator.</summary>
[DebuggerTypeProxy(typeof(UnaryExpressionProxy))]
public sealed class UnaryExpression : Expression
{
	/// <summary>Gets the static type of the expression that this <see cref="T:System.Linq.Expressions.Expression" /> represents.</summary>
	/// <returns>The <see cref="P:System.Linq.Expressions.UnaryExpression.Type" /> that represents the static type of the expression.</returns>
	public sealed override Type Type { get; }

	/// <summary>Returns the node type of this <see cref="T:System.Linq.Expressions.Expression" />.</summary>
	/// <returns>The <see cref="T:System.Linq.Expressions.ExpressionType" /> that represents this expression.</returns>
	public sealed override ExpressionType NodeType { get; }

	/// <summary>Gets the operand of the unary operation.</summary>
	/// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the operand of the unary operation.</returns>
	public Expression Operand { get; }

	/// <summary>Gets the implementing method for the unary operation.</summary>
	/// <returns>The <see cref="T:System.Reflection.MethodInfo" /> that represents the implementing method.</returns>
	public MethodInfo Method { get; }

	/// <summary>Gets a value that indicates whether the expression tree node represents a lifted call to an operator.</summary>
	/// <returns>true if the node represents a lifted call; otherwise, false.</returns>
	public bool IsLifted
	{
		get
		{
			if (NodeType == ExpressionType.TypeAs || NodeType == ExpressionType.Quote || NodeType == ExpressionType.Throw)
			{
				return false;
			}
			bool flag = Operand.Type.IsNullableType();
			bool flag2 = Type.IsNullableType();
			if (Method != null)
			{
				if (!flag || TypeUtils.AreEquivalent(Method.GetParametersCached()[0].ParameterType, Operand.Type))
				{
					if (flag2)
					{
						return !TypeUtils.AreEquivalent(Method.ReturnType, Type);
					}
					return false;
				}
				return true;
			}
			return flag || flag2;
		}
	}

	/// <summary>Gets a value that indicates whether the expression tree node represents a lifted call to an operator whose return type is lifted to a nullable type.</summary>
	/// <returns>true if the operator's return type is lifted to a nullable type; otherwise, false.</returns>
	public bool IsLiftedToNull
	{
		get
		{
			if (IsLifted)
			{
				return Type.IsNullableType();
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether the expression tree node can be reduced.</summary>
	/// <returns>True if a node can be reduced, otherwise false.</returns>
	public override bool CanReduce
	{
		get
		{
			ExpressionType nodeType = NodeType;
			if ((uint)(nodeType - 77) <= 3u)
			{
				return true;
			}
			return false;
		}
	}

	private bool IsPrefix
	{
		get
		{
			if (NodeType != ExpressionType.PreIncrementAssign)
			{
				return NodeType == ExpressionType.PreDecrementAssign;
			}
			return true;
		}
	}

	internal UnaryExpression(ExpressionType nodeType, Expression expression, Type type, MethodInfo method)
	{
		Operand = expression;
		Method = method;
		NodeType = nodeType;
		Type = type;
	}

	protected internal override Expression Accept(ExpressionVisitor visitor)
	{
		return visitor.VisitUnary(this);
	}

	/// <summary>Reduces the expression node to a simpler expression. </summary>
	/// <returns>The reduced expression.</returns>
	public override Expression Reduce()
	{
		if (CanReduce)
		{
			return Operand.NodeType switch
			{
				ExpressionType.Index => ReduceIndex(), 
				ExpressionType.MemberAccess => ReduceMember(), 
				_ => ReduceVariable(), 
			};
		}
		return this;
	}

	private UnaryExpression FunctionalOp(Expression operand)
	{
		ExpressionType nodeType = ((NodeType != ExpressionType.PreIncrementAssign && NodeType != ExpressionType.PostIncrementAssign) ? ExpressionType.Decrement : ExpressionType.Increment);
		return new UnaryExpression(nodeType, operand, operand.Type, Method);
	}

	private Expression ReduceVariable()
	{
		if (IsPrefix)
		{
			return Expression.Assign(Operand, FunctionalOp(Operand));
		}
		ParameterExpression parameterExpression = Expression.Parameter(Operand.Type, null);
		return Expression.Block(new TrueReadOnlyCollection<ParameterExpression>(parameterExpression), new TrueReadOnlyCollection<Expression>(Expression.Assign(parameterExpression, Operand), Expression.Assign(Operand, FunctionalOp(parameterExpression)), parameterExpression));
	}

	private Expression ReduceMember()
	{
		MemberExpression memberExpression = (MemberExpression)Operand;
		if (memberExpression.Expression == null)
		{
			return ReduceVariable();
		}
		ParameterExpression parameterExpression = Expression.Parameter(memberExpression.Expression.Type, null);
		BinaryExpression binaryExpression = Expression.Assign(parameterExpression, memberExpression.Expression);
		memberExpression = Expression.MakeMemberAccess(parameterExpression, memberExpression.Member);
		if (IsPrefix)
		{
			return Expression.Block(new TrueReadOnlyCollection<ParameterExpression>(parameterExpression), new TrueReadOnlyCollection<Expression>(binaryExpression, Expression.Assign(memberExpression, FunctionalOp(memberExpression))));
		}
		ParameterExpression parameterExpression2 = Expression.Parameter(memberExpression.Type, null);
		return Expression.Block(new TrueReadOnlyCollection<ParameterExpression>(parameterExpression, parameterExpression2), new TrueReadOnlyCollection<Expression>(binaryExpression, Expression.Assign(parameterExpression2, memberExpression), Expression.Assign(memberExpression, FunctionalOp(parameterExpression2)), parameterExpression2));
	}

	private Expression ReduceIndex()
	{
		bool isPrefix = IsPrefix;
		IndexExpression indexExpression = (IndexExpression)Operand;
		int argumentCount = indexExpression.ArgumentCount;
		Expression[] array = new Expression[argumentCount + (isPrefix ? 2 : 4)];
		ParameterExpression[] array2 = new ParameterExpression[argumentCount + (isPrefix ? 1 : 2)];
		ParameterExpression[] array3 = new ParameterExpression[argumentCount];
		int num = 0;
		array2[num] = Expression.Parameter(indexExpression.Object.Type, null);
		array[num] = Expression.Assign(array2[num], indexExpression.Object);
		for (num++; num <= argumentCount; num++)
		{
			Expression argument = indexExpression.GetArgument(num - 1);
			array3[num - 1] = (array2[num] = Expression.Parameter(argument.Type, null));
			array[num] = Expression.Assign(array2[num], argument);
		}
		indexExpression = Expression.MakeIndex(array2[0], indexExpression.Indexer, new TrueReadOnlyCollection<Expression>(array3));
		if (!isPrefix)
		{
			ParameterExpression parameterExpression = (array2[num] = Expression.Parameter(indexExpression.Type, null));
			array[num] = Expression.Assign(array2[num], indexExpression);
			num++;
			array[num++] = Expression.Assign(indexExpression, FunctionalOp(parameterExpression));
			array[num++] = parameterExpression;
		}
		else
		{
			array[num++] = Expression.Assign(indexExpression, FunctionalOp(indexExpression));
		}
		return Expression.Block(new TrueReadOnlyCollection<ParameterExpression>(array2), new TrueReadOnlyCollection<Expression>(array));
	}

	/// <summary>Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will return this expression.</summary>
	/// <returns>This expression if no children are changed or an expression with the updated children.</returns>
	/// <param name="operand">The <see cref="P:System.Linq.Expressions.UnaryExpression.Operand" /> property of the result.</param>
	public UnaryExpression Update(Expression operand)
	{
		if (operand == Operand)
		{
			return this;
		}
		return Expression.MakeUnary(NodeType, operand, Type, Method);
	}

	internal UnaryExpression()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
