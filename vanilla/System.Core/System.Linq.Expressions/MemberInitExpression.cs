using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic.Utils;
using Unity;

namespace System.Linq.Expressions;

/// <summary>Represents calling a constructor and initializing one or more members of the new object.</summary>
[DebuggerTypeProxy(typeof(MemberInitExpressionProxy))]
public sealed class MemberInitExpression : Expression
{
	/// <summary>Gets the static type of the expression that this <see cref="T:System.Linq.Expressions.Expression" /> represents.</summary>
	/// <returns>The <see cref="P:System.Linq.Expressions.MemberInitExpression.Type" /> that represents the static type of the expression.</returns>
	public sealed override Type Type => NewExpression.Type;

	/// <summary>Gets a value that indicates whether the expression tree node can be reduced.</summary>
	/// <returns>True if the node can be reduced, otherwise false.</returns>
	public override bool CanReduce => true;

	/// <summary>Returns the node type of this Expression. Extension nodes should return <see cref="F:System.Linq.Expressions.ExpressionType.Extension" /> when overriding this method.</summary>
	/// <returns>The <see cref="T:System.Linq.Expressions.ExpressionType" /> of the expression.</returns>
	public sealed override ExpressionType NodeType => ExpressionType.MemberInit;

	/// <summary>Gets the expression that represents the constructor call.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.NewExpression" /> that represents the constructor call.</returns>
	public NewExpression NewExpression { get; }

	/// <summary>Gets the bindings that describe how to initialize the members of the newly created object.</summary>
	/// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> of <see cref="T:System.Linq.Expressions.MemberBinding" /> objects which describe how to initialize the members.</returns>
	public ReadOnlyCollection<MemberBinding> Bindings { get; }

	internal MemberInitExpression(NewExpression newExpression, ReadOnlyCollection<MemberBinding> bindings)
	{
		NewExpression = newExpression;
		Bindings = bindings;
	}

	protected internal override Expression Accept(ExpressionVisitor visitor)
	{
		return visitor.VisitMemberInit(this);
	}

	/// <summary>Reduces the <see cref="T:System.Linq.Expressions.MemberInitExpression" /> to a simpler expression. </summary>
	/// <returns>The reduced expression.</returns>
	public override Expression Reduce()
	{
		return ReduceMemberInit(NewExpression, Bindings, keepOnStack: true);
	}

	private static Expression ReduceMemberInit(Expression objExpression, ReadOnlyCollection<MemberBinding> bindings, bool keepOnStack)
	{
		ParameterExpression parameterExpression = Expression.Variable(objExpression.Type);
		int count = bindings.Count;
		Expression[] array = new Expression[count + 2];
		array[0] = Expression.Assign(parameterExpression, objExpression);
		for (int i = 0; i < count; i++)
		{
			array[i + 1] = ReduceMemberBinding(parameterExpression, bindings[i]);
		}
		array[count + 1] = (keepOnStack ? ((Expression)parameterExpression) : ((Expression)Utils.Empty));
		return Expression.Block(new ParameterExpression[1] { parameterExpression }, array);
	}

	internal static Expression ReduceListInit(Expression listExpression, ReadOnlyCollection<ElementInit> initializers, bool keepOnStack)
	{
		ParameterExpression parameterExpression = Expression.Variable(listExpression.Type);
		int count = initializers.Count;
		Expression[] array = new Expression[count + 2];
		array[0] = Expression.Assign(parameterExpression, listExpression);
		for (int i = 0; i < count; i++)
		{
			ElementInit elementInit = initializers[i];
			array[i + 1] = Expression.Call(parameterExpression, elementInit.AddMethod, elementInit.Arguments);
		}
		array[count + 1] = (keepOnStack ? ((Expression)parameterExpression) : ((Expression)Utils.Empty));
		return Expression.Block(new ParameterExpression[1] { parameterExpression }, array);
	}

	internal static Expression ReduceMemberBinding(ParameterExpression objVar, MemberBinding binding)
	{
		MemberExpression memberExpression = Expression.MakeMemberAccess(objVar, binding.Member);
		return binding.BindingType switch
		{
			MemberBindingType.Assignment => Expression.Assign(memberExpression, ((MemberAssignment)binding).Expression), 
			MemberBindingType.ListBinding => ReduceListInit(memberExpression, ((MemberListBinding)binding).Initializers, keepOnStack: false), 
			MemberBindingType.MemberBinding => ReduceMemberInit(memberExpression, ((MemberMemberBinding)binding).Bindings, keepOnStack: false), 
			_ => throw ContractUtils.Unreachable, 
		};
	}

	/// <summary>Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will return this expression.</summary>
	/// <returns>This expression if no children are changed or an expression with the updated children.</returns>
	/// <param name="newExpression">The <see cref="P:System.Linq.Expressions.MemberInitExpression.NewExpression" /> property of the result.</param>
	/// <param name="bindings">The <see cref="P:System.Linq.Expressions.MemberInitExpression.Bindings" /> property of the result.</param>
	public MemberInitExpression Update(NewExpression newExpression, IEnumerable<MemberBinding> bindings)
	{
		if (newExpression == NewExpression && bindings != null && ExpressionUtils.SameElements(ref bindings, Bindings))
		{
			return this;
		}
		return Expression.MemberInit(newExpression, bindings);
	}

	internal MemberInitExpression()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
