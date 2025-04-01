using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic.Utils;
using System.Reflection;
using Unity;

namespace System.Linq.Expressions;

/// <summary>Represents accessing a field or property.</summary>
[DebuggerTypeProxy(typeof(MemberExpressionProxy))]
public class MemberExpression : Expression
{
	/// <summary>Gets the field or property to be accessed.</summary>
	/// <returns>The <see cref="T:System.Reflection.MemberInfo" /> that represents the field or property to be accessed.</returns>
	public MemberInfo Member => GetMember();

	/// <summary>Gets the containing object of the field or property.</summary>
	/// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the containing object of the field or property.</returns>
	public Expression Expression { get; }

	/// <summary>Returns the node type of this <see cref="P:System.Linq.Expressions.MemberExpression.Expression" />.</summary>
	/// <returns>The <see cref="T:System.Linq.Expressions.ExpressionType" /> that represents this expression.</returns>
	public sealed override ExpressionType NodeType => ExpressionType.MemberAccess;

	internal MemberExpression(Expression expression)
	{
		Expression = expression;
	}

	internal static PropertyExpression Make(Expression expression, PropertyInfo property)
	{
		return new PropertyExpression(expression, property);
	}

	internal static FieldExpression Make(Expression expression, FieldInfo field)
	{
		return new FieldExpression(expression, field);
	}

	internal static MemberExpression Make(Expression expression, MemberInfo member)
	{
		FieldInfo fieldInfo = member as FieldInfo;
		if (!(fieldInfo == null))
		{
			return Make(expression, fieldInfo);
		}
		return Make(expression, (PropertyInfo)member);
	}

	[ExcludeFromCodeCoverage]
	internal virtual MemberInfo GetMember()
	{
		throw ContractUtils.Unreachable;
	}

	/// <summary>Dispatches to the specific visit method for this node type. For example, <see cref="T:System.Linq.Expressions.MethodCallExpression" /> calls the <see cref="M:System.Linq.Expressions.ExpressionVisitor.VisitMethodCall(System.Linq.Expressions.MethodCallExpression)" />.</summary>
	/// <returns>The result of visiting this node.</returns>
	/// <param name="visitor">The visitor to visit this node with.</param>
	protected internal override Expression Accept(ExpressionVisitor visitor)
	{
		return visitor.VisitMember(this);
	}

	/// <summary>Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will return this expression.</summary>
	/// <returns>This expression if no children are changed or an expression with the updated children.</returns>
	/// <param name="expression">The <see cref="P:System.Linq.Expressions.MemberExpression.Expression" /> property of the result.</param>
	public MemberExpression Update(Expression expression)
	{
		if (expression == Expression)
		{
			return this;
		}
		return Expression.MakeMemberAccess(expression, Member);
	}

	internal MemberExpression()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
