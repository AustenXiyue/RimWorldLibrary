using System;
using System.Linq.Expressions;
using System.Reflection;

namespace HarmonyLib;

public static class SymbolExtensions
{
	public static MethodInfo GetMethodInfo(Expression<Action> expression)
	{
		return GetMethodInfo((LambdaExpression)expression);
	}

	public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
	{
		return GetMethodInfo((LambdaExpression)expression);
	}

	public static MethodInfo GetMethodInfo<T, TResult>(Expression<Func<T, TResult>> expression)
	{
		return GetMethodInfo((LambdaExpression)expression);
	}

	public static MethodInfo GetMethodInfo(LambdaExpression expression)
	{
		if (!(expression.Body is MethodCallExpression { Method: var method }))
		{
			if (expression.Body is UnaryExpression { Operand: MethodCallExpression { Object: ConstantExpression { Value: MethodInfo value } } })
			{
				return value;
			}
			throw new ArgumentException("Invalid Expression. Expression should consist of a Method call only.");
		}
		if ((object)method == null)
		{
			throw new Exception($"Cannot find method for expression {expression}");
		}
		return method;
	}
}
