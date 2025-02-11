using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic.Utils;
using System.Runtime.CompilerServices;
using Unity;

namespace System.Linq.Expressions;

/// <summary>Represents a dynamic operation.</summary>
public class DynamicExpression : Expression, IDynamicExpression, IArgumentProvider
{
	public override bool CanReduce => true;

	/// <summary>Gets the static type of the expression that this <see cref="T:System.Linq.Expressions.Expression" /> represents.</summary>
	/// <returns>The <see cref="P:System.Linq.Expressions.DynamicExpression.Type" /> that represents the static type of the expression.</returns>
	public override Type Type => typeof(object);

	/// <summary>Returns the node type of this expression. Extension nodes should return <see cref="F:System.Linq.Expressions.ExpressionType.Extension" /> when overriding this method.</summary>
	/// <returns>The <see cref="T:System.Linq.Expressions.ExpressionType" /> of the expression.</returns>
	public sealed override ExpressionType NodeType => ExpressionType.Dynamic;

	/// <summary>Gets the <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" />, which determines the run-time behavior of the dynamic site.</summary>
	/// <returns>The <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" />, which determines the run-time behavior of the dynamic site.</returns>
	public CallSiteBinder Binder { get; }

	/// <summary>Gets the type of the delegate used by the <see cref="T:System.Runtime.CompilerServices.CallSite" />.</summary>
	/// <returns>The <see cref="T:System.Type" /> object representing the type of the delegate used by the <see cref="T:System.Runtime.CompilerServices.CallSite" />.</returns>
	public Type DelegateType { get; }

	/// <summary>Gets the arguments to the dynamic operation.</summary>
	/// <returns>The read-only collections containing the arguments to the dynamic operation.</returns>
	public ReadOnlyCollection<Expression> Arguments => GetOrMakeArguments();

	[ExcludeFromCodeCoverage]
	int IArgumentProvider.ArgumentCount
	{
		get
		{
			throw ContractUtils.Unreachable;
		}
	}

	internal DynamicExpression(Type delegateType, CallSiteBinder binder)
	{
		DelegateType = delegateType;
		Binder = binder;
	}

	public override Expression Reduce()
	{
		ConstantExpression constantExpression = Expression.Constant(CallSite.Create(DelegateType, Binder));
		return Expression.Invoke(Expression.Field(constantExpression, "Target"), Arguments.AddFirst(constantExpression));
	}

	internal static DynamicExpression Make(Type returnType, Type delegateType, CallSiteBinder binder, ReadOnlyCollection<Expression> arguments)
	{
		if (returnType == typeof(object))
		{
			return new DynamicExpressionN(delegateType, binder, arguments);
		}
		return new TypedDynamicExpressionN(returnType, delegateType, binder, arguments);
	}

	internal static DynamicExpression Make(Type returnType, Type delegateType, CallSiteBinder binder, Expression arg0)
	{
		if (returnType == typeof(object))
		{
			return new DynamicExpression1(delegateType, binder, arg0);
		}
		return new TypedDynamicExpression1(returnType, delegateType, binder, arg0);
	}

	internal static DynamicExpression Make(Type returnType, Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1)
	{
		if (returnType == typeof(object))
		{
			return new DynamicExpression2(delegateType, binder, arg0, arg1);
		}
		return new TypedDynamicExpression2(returnType, delegateType, binder, arg0, arg1);
	}

	internal static DynamicExpression Make(Type returnType, Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2)
	{
		if (returnType == typeof(object))
		{
			return new DynamicExpression3(delegateType, binder, arg0, arg1, arg2);
		}
		return new TypedDynamicExpression3(returnType, delegateType, binder, arg0, arg1, arg2);
	}

	internal static DynamicExpression Make(Type returnType, Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
	{
		if (returnType == typeof(object))
		{
			return new DynamicExpression4(delegateType, binder, arg0, arg1, arg2, arg3);
		}
		return new TypedDynamicExpression4(returnType, delegateType, binder, arg0, arg1, arg2, arg3);
	}

	[ExcludeFromCodeCoverage]
	internal virtual ReadOnlyCollection<Expression> GetOrMakeArguments()
	{
		throw ContractUtils.Unreachable;
	}

	/// <summary>Dispatches to the specific visit method for this node type. For example, <see cref="T:System.Linq.Expressions.MethodCallExpression" /> calls the <see cref="M:System.Linq.Expressions.ExpressionVisitor.VisitMethodCall(System.Linq.Expressions.MethodCallExpression)" />.</summary>
	/// <returns>The result of visiting this node.</returns>
	/// <param name="visitor">The visitor to visit this node with.</param>
	protected internal override Expression Accept(ExpressionVisitor visitor)
	{
		if (visitor is DynamicExpressionVisitor dynamicExpressionVisitor)
		{
			return dynamicExpressionVisitor.VisitDynamic(this);
		}
		return base.Accept(visitor);
	}

	[ExcludeFromCodeCoverage]
	internal virtual DynamicExpression Rewrite(Expression[] args)
	{
		throw ContractUtils.Unreachable;
	}

	/// <summary>Compares the value sent to the parameter, arguments, to the Arguments property of the current instance of DynamicExpression. If the values of the parameter and the property are equal, the current instance is returned. If they are not equal, a new DynamicExpression instance is returned that is identical to the current instance except that the Arguments property is set to the value of parameter arguments. </summary>
	/// <returns>This expression if no children are changed or an expression with the updated children.</returns>
	/// <param name="arguments">The <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> property of the result.</param>
	public DynamicExpression Update(IEnumerable<Expression> arguments)
	{
		ICollection<Expression> collection;
		if (arguments == null)
		{
			collection = null;
		}
		else
		{
			collection = arguments as ICollection<Expression>;
			if (collection == null)
			{
				arguments = (collection = arguments.ToReadOnly());
			}
		}
		if (SameArguments(collection))
		{
			return this;
		}
		return ExpressionExtension.MakeDynamic(DelegateType, Binder, arguments);
	}

	[ExcludeFromCodeCoverage]
	internal virtual bool SameArguments(ICollection<Expression> arguments)
	{
		throw ContractUtils.Unreachable;
	}

	[ExcludeFromCodeCoverage]
	Expression IArgumentProvider.GetArgument(int index)
	{
		throw ContractUtils.Unreachable;
	}

	/// <summary>Creates a <see cref="T:System.Linq.Expressions.DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" />.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.DynamicExpression" /> that has <see cref="P:System.Linq.Expressions.DynamicExpression.NodeType" /> equal to <see cref="F:System.Linq.Expressions.ExpressionType.Dynamic" />, and has the <see cref="P:System.Linq.Expressions.DynamicExpression.Binder" /> and <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> set to the specified values.</returns>
	/// <param name="binder">The runtime binder for the dynamic operation.</param>
	/// <param name="returnType">The result type of the dynamic expression.</param>
	/// <param name="arguments">The arguments to the dynamic operation.</param>
	public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, params Expression[] arguments)
	{
		return ExpressionExtension.Dynamic(binder, returnType, arguments);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Expressions.DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" />.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.DynamicExpression" /> that has <see cref="P:System.Linq.Expressions.DynamicExpression.NodeType" /> equal to <see cref="F:System.Linq.Expressions.ExpressionType.Dynamic" />,  and has the <see cref="P:System.Linq.Expressions.DynamicExpression.Binder" /> and <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> set to the specified values.</returns>
	/// <param name="binder">The runtime binder for the dynamic operation.</param>
	/// <param name="returnType">The result type of the dynamic expression.</param>
	/// <param name="arguments">The arguments to the dynamic operation.</param>
	public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, IEnumerable<Expression> arguments)
	{
		return ExpressionExtension.Dynamic(binder, returnType, arguments);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Expressions.DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" />.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.DynamicExpression" /> that has <see cref="P:System.Linq.Expressions.DynamicExpression.NodeType" /> equal to <see cref="F:System.Linq.Expressions.ExpressionType.Dynamic" />,  and has the <see cref="P:System.Linq.Expressions.DynamicExpression.Binder" /> and <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> set to the specified values.</returns>
	/// <param name="binder">The runtime binder for the dynamic operation.</param>
	/// <param name="returnType">The result type of the dynamic expression.</param>
	/// <param name="arg0">The first argument to the dynamic operation.</param>
	public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0)
	{
		return ExpressionExtension.Dynamic(binder, returnType, arg0);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Expressions.DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" />.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.DynamicExpression" /> that has <see cref="P:System.Linq.Expressions.DynamicExpression.NodeType" /> equal to <see cref="F:System.Linq.Expressions.ExpressionType.Dynamic" />, and has the <see cref="P:System.Linq.Expressions.DynamicExpression.Binder" /> and <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> set to the specified values.</returns>
	/// <param name="binder">The runtime binder for the dynamic operation.</param>
	/// <param name="returnType">The result type of the dynamic expression.</param>
	/// <param name="arg0">The first argument to the dynamic operation.</param>
	/// <param name="arg1">The second argument to the dynamic operation.</param>
	public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1)
	{
		return ExpressionExtension.Dynamic(binder, returnType, arg0, arg1);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Expressions.DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" />.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.DynamicExpression" /> that has <see cref="P:System.Linq.Expressions.DynamicExpression.NodeType" /> equal to <see cref="F:System.Linq.Expressions.ExpressionType.Dynamic" />, and has the <see cref="P:System.Linq.Expressions.DynamicExpression.Binder" /> and <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> set to the specified values.</returns>
	/// <param name="binder">The runtime binder for the dynamic operation.</param>
	/// <param name="returnType">The result type of the dynamic expression.</param>
	/// <param name="arg0">The first argument to the dynamic operation.</param>
	/// <param name="arg1">The second argument to the dynamic operation.</param>
	/// <param name="arg2">The third argument to the dynamic operation.</param>
	public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2)
	{
		return ExpressionExtension.Dynamic(binder, returnType, arg0, arg1, arg2);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Expressions.DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" />.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.DynamicExpression" /> that has <see cref="P:System.Linq.Expressions.DynamicExpression.NodeType" /> equal to <see cref="F:System.Linq.Expressions.ExpressionType.Dynamic" />, and has the <see cref="P:System.Linq.Expressions.DynamicExpression.Binder" /> and <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> set to the specified values.</returns>
	/// <param name="binder">The runtime binder for the dynamic operation.</param>
	/// <param name="returnType">The result type of the dynamic expression.</param>
	/// <param name="arg0">The first argument to the dynamic operation.</param>
	/// <param name="arg1">The second argument to the dynamic operation.</param>
	/// <param name="arg2">The third argument to the dynamic operation.</param>
	/// <param name="arg3">The fourth argument to the dynamic operation.</param>
	public new static DynamicExpression Dynamic(CallSiteBinder binder, Type returnType, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
	{
		return ExpressionExtension.Dynamic(binder, returnType, arg0, arg1, arg2, arg3);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Expressions.DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" />.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.DynamicExpression" /> that has <see cref="P:System.Linq.Expressions.DynamicExpression.NodeType" /> equal to <see cref="F:System.Linq.Expressions.ExpressionType.Dynamic" />, and has the <see cref="P:System.Linq.Expressions.DynamicExpression.DelegateType" />, <see cref="P:System.Linq.Expressions.DynamicExpression.Binder" />, and <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> set to the specified values.</returns>
	/// <param name="delegateType">The type of the delegate used by the <see cref="T:System.Runtime.CompilerServices.CallSite" />.</param>
	/// <param name="binder">The runtime binder for the dynamic operation.</param>
	/// <param name="arguments">The arguments to the dynamic operation.</param>
	public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, IEnumerable<Expression> arguments)
	{
		return ExpressionExtension.MakeDynamic(delegateType, binder, arguments);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Expressions.DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" />.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.DynamicExpression" /> that has <see cref="P:System.Linq.Expressions.DynamicExpression.NodeType" /> equal to <see cref="F:System.Linq.Expressions.ExpressionType.Dynamic" />, and has the <see cref="P:System.Linq.Expressions.DynamicExpression.DelegateType" />, <see cref="P:System.Linq.Expressions.DynamicExpression.Binder" />, and <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> set to the specified values.</returns>
	/// <param name="delegateType">The type of the delegate used by the <see cref="T:System.Runtime.CompilerServices.CallSite" />.</param>
	/// <param name="binder">The runtime binder for the dynamic operation.</param>
	/// <param name="arguments">The arguments to the dynamic operation.</param>
	public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, params Expression[] arguments)
	{
		return ExpressionExtension.MakeDynamic(delegateType, binder, arguments);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Expressions.DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" /> and one argument.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.DynamicExpression" /> that has <see cref="P:System.Linq.Expressions.DynamicExpression.NodeType" /> equal to <see cref="F:System.Linq.Expressions.ExpressionType.Dynamic" />, and has the <see cref="P:System.Linq.Expressions.DynamicExpression.DelegateType" />, <see cref="P:System.Linq.Expressions.DynamicExpression.Binder" />, and <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> set to the specified values.</returns>
	/// <param name="delegateType">The type of the delegate used by the <see cref="T:System.Runtime.CompilerServices.CallSite" />.</param>
	/// <param name="binder">The runtime binder for the dynamic operation.</param>
	/// <param name="arg0">The argument to the dynamic operation.</param>
	public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0)
	{
		return ExpressionExtension.MakeDynamic(delegateType, binder, arg0);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Expressions.DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" /> and two arguments.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.DynamicExpression" /> that has <see cref="P:System.Linq.Expressions.DynamicExpression.NodeType" /> equal to <see cref="F:System.Linq.Expressions.ExpressionType.Dynamic" />, and has the <see cref="P:System.Linq.Expressions.DynamicExpression.DelegateType" />, <see cref="P:System.Linq.Expressions.DynamicExpression.Binder" />, and <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> set to the specified values.</returns>
	/// <param name="delegateType">The type of the delegate used by the <see cref="T:System.Runtime.CompilerServices.CallSite" />.</param>
	/// <param name="binder">The runtime binder for the dynamic operation.</param>
	/// <param name="arg0">The first argument to the dynamic operation.</param>
	/// <param name="arg1">The second argument to the dynamic operation.</param>
	public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1)
	{
		return ExpressionExtension.MakeDynamic(delegateType, binder, arg0, arg1);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Expressions.DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" /> and three arguments.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.DynamicExpression" /> that has <see cref="P:System.Linq.Expressions.DynamicExpression.NodeType" /> equal to <see cref="F:System.Linq.Expressions.ExpressionType.Dynamic" />, and has the <see cref="P:System.Linq.Expressions.DynamicExpression.DelegateType" />, <see cref="P:System.Linq.Expressions.DynamicExpression.Binder" />, and <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> set to the specified values.</returns>
	/// <param name="delegateType">The type of the delegate used by the <see cref="T:System.Runtime.CompilerServices.CallSite" />.</param>
	/// <param name="binder">The runtime binder for the dynamic operation.</param>
	/// <param name="arg0">The first argument to the dynamic operation.</param>
	/// <param name="arg1">The second argument to the dynamic operation.</param>
	/// <param name="arg2">The third argument to the dynamic operation.</param>
	public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2)
	{
		return ExpressionExtension.MakeDynamic(delegateType, binder, arg0, arg1, arg2);
	}

	/// <summary>Creates a <see cref="T:System.Linq.Expressions.DynamicExpression" /> that represents a dynamic operation bound by the provided <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" /> and four arguments.</summary>
	/// <returns>A <see cref="T:System.Linq.Expressions.DynamicExpression" /> that has <see cref="P:System.Linq.Expressions.DynamicExpression.NodeType" /> equal to <see cref="F:System.Linq.Expressions.ExpressionType.Dynamic" />, and has the <see cref="P:System.Linq.Expressions.DynamicExpression.DelegateType" />, <see cref="P:System.Linq.Expressions.DynamicExpression.Binder" />, and <see cref="P:System.Linq.Expressions.DynamicExpression.Arguments" /> set to the specified values.</returns>
	/// <param name="delegateType">The type of the delegate used by the <see cref="T:System.Runtime.CompilerServices.CallSite" />.</param>
	/// <param name="binder">The runtime binder for the dynamic operation.</param>
	/// <param name="arg0">The first argument to the dynamic operation.</param>
	/// <param name="arg1">The second argument to the dynamic operation.</param>
	/// <param name="arg2">The third argument to the dynamic operation.</param>
	/// <param name="arg3">The fourth argument to the dynamic operation.</param>
	public new static DynamicExpression MakeDynamic(Type delegateType, CallSiteBinder binder, Expression arg0, Expression arg1, Expression arg2, Expression arg3)
	{
		return ExpressionExtension.MakeDynamic(delegateType, binder, arg0, arg1, arg2, arg3);
	}

	Expression IDynamicExpression.Rewrite(Expression[] args)
	{
		return Rewrite(args);
	}

	object IDynamicExpression.CreateCallSite()
	{
		return CallSite.Create(DelegateType, Binder);
	}

	internal DynamicExpression()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
