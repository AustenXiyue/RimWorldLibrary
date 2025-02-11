using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic.Utils;
using System.Linq.Expressions.Compiler;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Unity;

namespace System.Linq.Expressions;

/// <summary>Describes a lambda expression. This captures a block of code that is similar to a .NET method body.</summary>
[DebuggerTypeProxy(typeof(LambdaExpressionProxy))]
public abstract class LambdaExpression : Expression, IParameterProvider
{
	private readonly Expression _body;

	/// <summary>Gets the static type of the expression that this <see cref="T:System.Linq.Expressions.Expression" /> represents.</summary>
	/// <returns>The <see cref="P:System.Linq.Expressions.LambdaExpression.Type" /> that represents the static type of the expression.</returns>
	public sealed override Type Type => TypeCore;

	internal abstract Type TypeCore { get; }

	internal abstract Type PublicType { get; }

	/// <summary>Returns the node type of this <see cref="T:System.Linq.Expressions.Expression" />.</summary>
	/// <returns>The <see cref="T:System.Linq.Expressions.ExpressionType" /> that represents this expression.</returns>
	public sealed override ExpressionType NodeType => ExpressionType.Lambda;

	/// <summary>Gets the parameters of the lambda expression.</summary>
	/// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> of <see cref="T:System.Linq.Expressions.ParameterExpression" /> objects that represent the parameters of the lambda expression.</returns>
	public ReadOnlyCollection<ParameterExpression> Parameters => GetOrMakeParameters();

	/// <summary>Gets the name of the lambda expression.</summary>
	/// <returns>The name of the lambda expression.</returns>
	public string Name => NameCore;

	internal virtual string NameCore => null;

	/// <summary>Gets the body of the lambda expression.</summary>
	/// <returns>An <see cref="T:System.Linq.Expressions.Expression" /> that represents the body of the lambda expression.</returns>
	public Expression Body => _body;

	/// <summary>Gets the return type of the lambda expression.</summary>
	/// <returns>The <see cref="T:System.Type" /> object representing the type of the lambda expression.</returns>
	public Type ReturnType => Type.GetInvokeMethod().ReturnType;

	/// <summary>Gets the value that indicates if the lambda expression will be compiled with the tail call optimization.</summary>
	/// <returns>True if the lambda expression will be compiled with the tail call optimization, otherwise false.</returns>
	public bool TailCall => TailCallCore;

	internal virtual bool TailCallCore => false;

	[ExcludeFromCodeCoverage]
	int IParameterProvider.ParameterCount => ParameterCount;

	[ExcludeFromCodeCoverage]
	internal virtual int ParameterCount
	{
		get
		{
			throw ContractUtils.Unreachable;
		}
	}

	internal LambdaExpression(Expression body)
	{
		_body = body;
	}

	[ExcludeFromCodeCoverage]
	internal virtual ReadOnlyCollection<ParameterExpression> GetOrMakeParameters()
	{
		throw ContractUtils.Unreachable;
	}

	[ExcludeFromCodeCoverage]
	ParameterExpression IParameterProvider.GetParameter(int index)
	{
		return GetParameter(index);
	}

	[ExcludeFromCodeCoverage]
	internal virtual ParameterExpression GetParameter(int index)
	{
		throw ContractUtils.Unreachable;
	}

	/// <summary>Produces a delegate that represents the lambda expression.</summary>
	/// <returns>A <see cref="T:System.Delegate" /> that contains the compiled version of the lambda expression.</returns>
	public Delegate Compile()
	{
		return Compile(preferInterpretation: false);
	}

	public Delegate Compile(bool preferInterpretation)
	{
		return LambdaCompiler.Compile(this);
	}

	/// <summary>Compiles the lambda into a method definition.</summary>
	/// <param name="method">A <see cref="T:System.Reflection.Emit.MethodBuilder" /> which will be used to hold the lambda's IL.</param>
	public void CompileToMethod(MethodBuilder method)
	{
		ContractUtils.RequiresNotNull(method, "method");
		ContractUtils.Requires(method.IsStatic, "method");
		if (method.DeclaringType as TypeBuilder == null)
		{
			throw Error.MethodBuilderDoesNotHaveTypeBuilder();
		}
		LambdaCompiler.Compile(this, method);
	}

	internal abstract LambdaExpression Accept(StackSpiller spiller);

	/// <summary>Produces a delegate that represents the lambda expression.</summary>
	/// <returns>A delegate containing the compiled version of the lambda.</returns>
	/// <param name="debugInfoGenerator">Debugging information generator used by the compiler to mark sequence points and annotate local variables.</param>
	public Delegate Compile(DebugInfoGenerator debugInfoGenerator)
	{
		return Compile();
	}

	/// <summary>Compiles the lambda into a method definition and custom debug information.</summary>
	/// <param name="method">A <see cref="T:System.Reflection.Emit.MethodBuilder" /> which will be used to hold the lambda's IL.</param>
	/// <param name="debugInfoGenerator">Debugging information generator used by the compiler to mark sequence points and annotate local variables.</param>
	public void CompileToMethod(MethodBuilder method, DebugInfoGenerator debugInfoGenerator)
	{
		CompileToMethod(method);
	}

	internal LambdaExpression()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
