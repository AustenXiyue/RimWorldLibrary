using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Primitives;

/// <summary>Represents a function exported by a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" />.</summary>
public class ExportedDelegate
{
	private object _instance;

	private MethodInfo _method;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportedDelegate" /> class. </summary>
	protected ExportedDelegate()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportedDelegate" /> class for the specified part and method. </summary>
	/// <param name="instance">The part exporting the method.</param>
	/// <param name="method">The method to be exported.</param>
	public ExportedDelegate(object instance, MethodInfo method)
	{
		Requires.NotNull(method, "method");
		_instance = instance;
		_method = method;
	}

	/// <summary>Gets a delegate of the specified type.</summary>
	/// <returns>A delegate of the specified type, or null if no such delegate can be created.</returns>
	/// <param name="delegateType">The type of the delegate to return.</param>
	public virtual Delegate CreateDelegate(Type delegateType)
	{
		Requires.NotNull(delegateType, "delegateType");
		if (delegateType == typeof(Delegate) || delegateType == typeof(MulticastDelegate))
		{
			delegateType = CreateStandardDelegateType();
		}
		return Delegate.CreateDelegate(delegateType, _instance, _method, throwOnBindFailure: false);
	}

	private Type CreateStandardDelegateType()
	{
		ParameterInfo[] parameters = _method.GetParameters();
		Type[] array = new Type[parameters.Length + 1];
		array[parameters.Length] = _method.ReturnType;
		for (int i = 0; i < parameters.Length; i++)
		{
			array[i] = parameters[i].ParameterType;
		}
		return Expression.GetDelegateType(array);
	}
}
