using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic.Utils;
using Unity;

namespace System.Linq.Expressions;

/// <summary>Emits or clears a sequence point for debug information. This allows the debugger to highlight the correct source code when debugging.</summary>
[DebuggerTypeProxy(typeof(DebugInfoExpressionProxy))]
public class DebugInfoExpression : Expression
{
	/// <summary>Gets the static type of the expression that this <see cref="T:System.Linq.Expressions.Expression" /> represents.</summary>
	/// <returns>The <see cref="P:System.Linq.Expressions.DebugInfoExpression.Type" /> that represents the static type of the expression.</returns>
	public sealed override Type Type => typeof(void);

	/// <summary>Returns the node type of this <see cref="T:System.Linq.Expressions.Expression" />.</summary>
	/// <returns>The <see cref="T:System.Linq.Expressions.ExpressionType" /> that represents this expression.</returns>
	public sealed override ExpressionType NodeType => ExpressionType.DebugInfo;

	/// <summary>Gets the start line of this <see cref="T:System.Linq.Expressions.DebugInfoExpression" />.</summary>
	/// <returns>The number of the start line of the code that was used to generate the wrapped expression.</returns>
	[ExcludeFromCodeCoverage]
	public virtual int StartLine
	{
		get
		{
			throw ContractUtils.Unreachable;
		}
	}

	/// <summary>Gets the start column of this <see cref="T:System.Linq.Expressions.DebugInfoExpression" />.</summary>
	/// <returns>The number of the start column of the code that was used to generate the wrapped expression.</returns>
	[ExcludeFromCodeCoverage]
	public virtual int StartColumn
	{
		get
		{
			throw ContractUtils.Unreachable;
		}
	}

	/// <summary>Gets the end line of this <see cref="T:System.Linq.Expressions.DebugInfoExpression" />.</summary>
	/// <returns>The number of the end line of the code that was used to generate the wrapped expression.</returns>
	[ExcludeFromCodeCoverage]
	public virtual int EndLine
	{
		get
		{
			throw ContractUtils.Unreachable;
		}
	}

	/// <summary>Gets the end column of this <see cref="T:System.Linq.Expressions.DebugInfoExpression" />.</summary>
	/// <returns>The number of the end column of the code that was used to generate the wrapped expression.</returns>
	[ExcludeFromCodeCoverage]
	public virtual int EndColumn
	{
		get
		{
			throw ContractUtils.Unreachable;
		}
	}

	/// <summary>Gets the <see cref="T:System.Linq.Expressions.SymbolDocumentInfo" /> that represents the source file.</summary>
	/// <returns>The <see cref="T:System.Linq.Expressions.SymbolDocumentInfo" /> that represents the source file.</returns>
	public SymbolDocumentInfo Document { get; }

	/// <summary>Gets the value to indicate if the <see cref="T:System.Linq.Expressions.DebugInfoExpression" /> is for clearing a sequence point.</summary>
	/// <returns>True if the <see cref="T:System.Linq.Expressions.DebugInfoExpression" /> is for clearing a sequence point, otherwise false.</returns>
	[ExcludeFromCodeCoverage]
	public virtual bool IsClear
	{
		get
		{
			throw ContractUtils.Unreachable;
		}
	}

	internal DebugInfoExpression(SymbolDocumentInfo document)
	{
		Document = document;
	}

	/// <summary>Dispatches to the specific visit method for this node type. For example, <see cref="T:System.Linq.Expressions.MethodCallExpression" /> calls the <see cref="M:System.Linq.Expressions.ExpressionVisitor.VisitMethodCall(System.Linq.Expressions.MethodCallExpression)" />.</summary>
	/// <returns>The result of visiting this node.</returns>
	/// <param name="visitor">The visitor to visit this node with.</param>
	protected internal override Expression Accept(ExpressionVisitor visitor)
	{
		return visitor.VisitDebugInfo(this);
	}

	internal DebugInfoExpression()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
