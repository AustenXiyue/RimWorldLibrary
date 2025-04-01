using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Unity;

namespace System.Linq.Expressions;

/// <summary>Represents an initializer for a single element of an <see cref="T:System.Collections.IEnumerable" /> collection.</summary>
public sealed class ElementInit : IArgumentProvider
{
	/// <summary>Gets the instance method that is used to add an element to an <see cref="T:System.Collections.IEnumerable" /> collection.</summary>
	/// <returns>A <see cref="T:System.Reflection.MethodInfo" /> that represents an instance method that adds an element to a collection.</returns>
	public MethodInfo AddMethod { get; }

	/// <summary>Gets the collection of arguments that are passed to a method that adds an element to an <see cref="T:System.Collections.IEnumerable" /> collection.</summary>
	/// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> of <see cref="T:System.Linq.Expressions.Expression" /> objects that represent the arguments for a method that adds an element to a collection.</returns>
	public ReadOnlyCollection<Expression> Arguments { get; }

	public int ArgumentCount => Arguments.Count;

	internal ElementInit(MethodInfo addMethod, ReadOnlyCollection<Expression> arguments)
	{
		AddMethod = addMethod;
		Arguments = arguments;
	}

	public Expression GetArgument(int index)
	{
		return Arguments[index];
	}

	/// <summary>Returns a textual representation of an <see cref="T:System.Linq.Expressions.ElementInit" /> object.</summary>
	/// <returns>A textual representation of the <see cref="T:System.Linq.Expressions.ElementInit" /> object.</returns>
	public override string ToString()
	{
		return ExpressionStringBuilder.ElementInitBindingToString(this);
	}

	/// <summary>Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will return this expression.</summary>
	/// <returns>This expression if no children are changed or an expression with the updated children.</returns>
	/// <param name="arguments">The <see cref="P:System.Linq.Expressions.ElementInit.Arguments" /> property of the result.</param>
	public ElementInit Update(IEnumerable<Expression> arguments)
	{
		if (arguments == Arguments)
		{
			return this;
		}
		return Expression.ElementInit(AddMethod, arguments);
	}

	internal ElementInit()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
