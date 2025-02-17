using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MonoMod.Logs;

namespace MonoMod.Utils;

internal sealed class MethodSignature : IEquatable<MethodSignature>, IDebugFormattable
{
	private sealed class CompatableComparer : IEqualityComparer<Type>
	{
		public static readonly CompatableComparer Instance = new CompatableComparer();

		public bool Equals(Type? x, Type? y)
		{
			if ((object)x == y)
			{
				return true;
			}
			if ((object)x == null || (object)y == null)
			{
				return false;
			}
			return x.IsCompatible(y);
		}

		public int GetHashCode([DisallowNull] Type obj)
		{
			throw new NotSupportedException();
		}
	}

	private readonly Type[] parameters;

	private static readonly ConditionalWeakTable<MethodBase, MethodSignature> thisSigMap = new ConditionalWeakTable<MethodBase, MethodSignature>();

	private static readonly ConditionalWeakTable<MethodBase, MethodSignature> noThisSigMap = new ConditionalWeakTable<MethodBase, MethodSignature>();

	public Type ReturnType { get; }

	public int ParameterCount => parameters.Length;

	public IEnumerable<Type> Parameters => parameters;

	public Type? FirstParameter
	{
		get
		{
			if (parameters.Length < 1)
			{
				return null;
			}
			return parameters[0];
		}
	}

	public MethodSignature(Type returnType, Type[] parameters)
	{
		ReturnType = returnType;
		this.parameters = parameters;
	}

	public MethodSignature(Type returnType, IEnumerable<Type> parameters)
	{
		ReturnType = returnType;
		this.parameters = parameters.ToArray();
	}

	public MethodSignature(MethodBase method)
		: this(method, ignoreThis: false)
	{
	}

	public MethodSignature(MethodBase method, bool ignoreThis)
	{
		ReturnType = (method as MethodInfo)?.ReturnType ?? typeof(void);
		int num = ((!ignoreThis && !method.IsStatic) ? 1 : 0);
		ParameterInfo[] array = method.GetParameters();
		parameters = new Type[array.Length + num];
		for (int i = num; i < parameters.Length; i++)
		{
			parameters[i] = array[i - num].ParameterType;
		}
		if (!ignoreThis && !method.IsStatic)
		{
			parameters[0] = method.GetThisParamType();
		}
	}

	public static MethodSignature ForMethod(MethodBase method)
	{
		return ForMethod(method, ignoreThis: false);
	}

	public static MethodSignature ForMethod(MethodBase method, bool ignoreThis)
	{
		return (ignoreThis ? noThisSigMap : thisSigMap).GetValue(method, (MethodBase m) => new MethodSignature(m, ignoreThis));
	}

	public bool IsCompatibleWith(MethodSignature other)
	{
		Helpers.ThrowIfArgumentNull(other, "other");
		if (this == other)
		{
			return true;
		}
		if (ReturnType.IsCompatible(other.ReturnType))
		{
			return parameters.SequenceEqual<Type>(other.Parameters, CompatableComparer.Instance);
		}
		return false;
	}

	public DynamicMethodDefinition CreateDmd(string name)
	{
		return new DynamicMethodDefinition(name, ReturnType, parameters);
	}

	public override string ToString()
	{
		int literalLength = 2 + parameters.Length - 1;
		int formattedCount = 1 + parameters.Length;
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(literalLength, formattedCount);
		defaultInterpolatedStringHandler.AppendFormatted(ReturnType);
		defaultInterpolatedStringHandler.AppendLiteral(" (");
		for (int i = 0; i < parameters.Length; i++)
		{
			if (i != 0)
			{
				defaultInterpolatedStringHandler.AppendLiteral(", ");
			}
			defaultInterpolatedStringHandler.AppendFormatted(parameters[i]);
		}
		defaultInterpolatedStringHandler.AppendLiteral(")");
		return defaultInterpolatedStringHandler.ToStringAndClear();
	}

	bool IDebugFormattable.TryFormatInto(Span<char> span, out int wrote)
	{
		wrote = 0;
		Span<char> span2 = span;
		Span<char> into = span2;
		bool enabled;
		FormatIntoInterpolatedStringHandler handler = new FormatIntoInterpolatedStringHandler(2, 1, span2, out enabled);
		if (enabled && handler.AppendFormatted(ReturnType))
		{
			handler.AppendLiteral(" (");
		}
		else
			_ = 0;
		if (!DebugFormatter.Into(into, out var wrote2, ref handler))
		{
			return false;
		}
		wrote += wrote2;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (i != 0)
			{
				if (!", ".AsSpan().TryCopyTo(span.Slice(wrote)))
				{
					return false;
				}
				wrote += 2;
			}
			span2 = span;
			Span<char> into2 = span2;
			handler = new FormatIntoInterpolatedStringHandler(0, 1, span2, out enabled);
			if (enabled)
			{
				handler.AppendFormatted(parameters[i]);
			}
			else
				_ = 0;
			if (!DebugFormatter.Into(into2, out wrote2, ref handler))
			{
				return false;
			}
			wrote += wrote2;
		}
		if (span.Slice(wrote).Length < 1)
		{
			return false;
		}
		span[wrote++] = ')';
		return true;
	}

	public bool Equals(MethodSignature? other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (!ReturnType.Equals(other.ReturnType))
		{
			return false;
		}
		return Parameters.SequenceEqual(other.Parameters);
	}

	public override bool Equals(object? obj)
	{
		if (obj is MethodSignature other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(ReturnType);
		hashCode.Add(parameters.Length);
		Type[] array = parameters;
		foreach (Type value in array)
		{
			hashCode.Add(value);
		}
		return hashCode.ToHashCode();
	}
}
