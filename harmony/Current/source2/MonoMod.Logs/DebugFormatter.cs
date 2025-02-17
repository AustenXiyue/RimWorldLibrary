using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using MonoMod.Utils;

namespace MonoMod.Logs;

internal static class DebugFormatter
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool CanDebugFormat<T>(in T value, out object? extraData)
	{
		extraData = null;
		if (typeof(T) == typeof(Type))
		{
			return true;
		}
		if (typeof(T) == typeof(MethodBase))
		{
			return true;
		}
		if (typeof(T) == typeof(MethodInfo))
		{
			return true;
		}
		if (typeof(T) == typeof(ConstructorInfo))
		{
			return true;
		}
		if (typeof(T) == typeof(FieldInfo))
		{
			return true;
		}
		if (typeof(T) == typeof(PropertyInfo))
		{
			return true;
		}
		if (typeof(T) == typeof(Exception))
		{
			return true;
		}
		if (typeof(T) == typeof(IDebugFormattable))
		{
			return true;
		}
		T val = value;
		if ((val is Type || val is MethodBase || val is FieldInfo || val is PropertyInfo) ? true : false)
		{
			return true;
		}
		if (value is Exception ex)
		{
			extraData = ex.ToString();
			return true;
		}
		if (value is IDebugFormattable)
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryFormatInto<T>(in T value, object? extraData, Span<char> into, out int wrote)
	{
		if (default(T) == null && value == null)
		{
			wrote = 0;
			return true;
		}
		if (typeof(T) == typeof(Type))
		{
			return TryFormatType(Transmute<Type>(in value), into, out wrote);
		}
		if (typeof(T) == typeof(MethodInfo))
		{
			return TryFormatMethodInfo(Transmute<MethodInfo>(in value), into, out wrote);
		}
		if (typeof(T) == typeof(ConstructorInfo))
		{
			return TryFormatMethodBase(Transmute<ConstructorInfo>(in value), into, out wrote);
		}
		if (typeof(T) == typeof(FieldInfo))
		{
			return TryFormatFieldInfo(Transmute<FieldInfo>(in value), into, out wrote);
		}
		if (typeof(T) == typeof(PropertyInfo))
		{
			return TryFormatPropertyInfo(Transmute<PropertyInfo>(in value), into, out wrote);
		}
		if (typeof(T) == typeof(Exception))
		{
			return TryFormatException(Transmute<Exception>(in value), Unsafe.As<string>(extraData), into, out wrote);
		}
		if (typeof(T) == typeof(IDebugFormattable))
		{
			return Transmute<IDebugFormattable>(in value).TryFormatInto(into, out wrote);
		}
		if (value is Type type)
		{
			return TryFormatType(type, into, out wrote);
		}
		if (value is MethodInfo method)
		{
			return TryFormatMethodInfo(method, into, out wrote);
		}
		if (value is ConstructorInfo method2)
		{
			return TryFormatMethodBase(method2, into, out wrote);
		}
		if (value is MethodBase method3)
		{
			return TryFormatMethodBase(method3, into, out wrote);
		}
		if (value is FieldInfo field)
		{
			return TryFormatFieldInfo(field, into, out wrote);
		}
		if (value is PropertyInfo prop)
		{
			return TryFormatPropertyInfo(prop, into, out wrote);
		}
		if (value is Exception e)
		{
			return TryFormatException(e, Unsafe.As<string>(extraData), into, out wrote);
		}
		if (value is IDebugFormattable)
		{
			return ((IDebugFormattable)(object)value).TryFormatInto(into, out wrote);
		}
		bool flag = false;
		bool isEnabled;
		AssertionInterpolatedStringHandler message = new AssertionInterpolatedStringHandler(48, 1, flag, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("Called TryFormatInto with value of unknown type ");
			message.AppendFormatted(value.GetType());
		}
		Helpers.Assert(flag, ref message, "false");
		wrote = 0;
		return false;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static ref TOut Transmute<TOut>(in T val)
		{
			return ref Unsafe.As<T, TOut>(ref Unsafe.AsRef(in val));
		}
	}

	private static bool TryFormatException(Exception e, string? eStr, Span<char> into, out int wrote)
	{
		wrote = 0;
		if (eStr == null)
		{
			eStr = e.ToString();
		}
		string newLine = Environment.NewLine;
		if (into.Slice(wrote).Length < eStr.Length)
		{
			return false;
		}
		eStr.AsSpan().CopyTo(into.Slice(wrote));
		wrote += eStr.Length;
		FormatIntoInterpolatedStringHandler handler;
		bool enabled;
		int wrote2;
		if (e is ReflectionTypeLoadException ex)
		{
			for (int i = 0; i < 4 && i < ex.Types.Length; i++)
			{
				Span<char> span = into.Slice(wrote);
				Span<char> into2 = span;
				handler = new FormatIntoInterpolatedStringHandler(56, 3, span, out enabled);
				if (enabled && handler.AppendFormatted(newLine) && handler.AppendLiteral("System.Reflection.ReflectionTypeLoadException.Types[") && handler.AppendFormatted(i) && handler.AppendLiteral("] = "))
				{
					handler.AppendFormatted(ex.Types[i]);
				}
				else
					_ = 0;
				if (!Into(into2, out wrote2, ref handler))
				{
					return false;
				}
				wrote += wrote2;
			}
			if (ex.Types.Length >= 4)
			{
				Span<char> span = into.Slice(wrote);
				Span<char> into3 = span;
				handler = new FormatIntoInterpolatedStringHandler(62, 1, span, out enabled);
				if (enabled && handler.AppendFormatted(newLine))
				{
					handler.AppendLiteral("System.Reflection.ReflectionTypeLoadException.Types[...] = ...");
				}
				else
					_ = 0;
				if (!Into(into3, out wrote2, ref handler))
				{
					return false;
				}
				wrote += wrote2;
			}
			if (ex.LoaderExceptions.Length != 0)
			{
				if (into.Slice(wrote).Length < newLine.Length + "System.Reflection.ReflectionTypeLoadException.LoaderExceptions = [".Length)
				{
					return false;
				}
				newLine.AsSpan().CopyTo(into.Slice(wrote));
				wrote += newLine.Length;
				"System.Reflection.ReflectionTypeLoadException.LoaderExceptions = [".AsSpan().CopyTo(into.Slice(wrote));
				wrote += "System.Reflection.ReflectionTypeLoadException.LoaderExceptions = [".Length;
				for (int j = 0; j < ex.LoaderExceptions.Length; j++)
				{
					Exception ex2 = ex.LoaderExceptions[j];
					if (ex2 != null)
					{
						if (into.Slice(wrote).Length < newLine.Length)
						{
							return false;
						}
						newLine.AsSpan().CopyTo(into.Slice(wrote));
						wrote += newLine.Length;
						if (!TryFormatException(ex2, null, into.Slice(wrote), out wrote2))
						{
							return false;
						}
						wrote += wrote2;
					}
				}
				if (into.Slice(wrote).Length < newLine.Length + 1)
				{
					return false;
				}
				newLine.AsSpan().CopyTo(into.Slice(wrote));
				wrote += newLine.Length;
				into[wrote++] = ']';
			}
		}
		if (e is TypeLoadException ex3)
		{
			Span<char> span = into.Slice(wrote);
			Span<char> into4 = span;
			handler = new FormatIntoInterpolatedStringHandler(36, 2, span, out enabled);
			if (enabled && handler.AppendFormatted(newLine) && handler.AppendLiteral("System.TypeLoadException.TypeName = "))
			{
				handler.AppendFormatted(ex3.TypeName);
			}
			else
				_ = 0;
			if (!Into(into4, out wrote2, ref handler))
			{
				return false;
			}
			wrote += wrote2;
		}
		if (e is BadImageFormatException ex4)
		{
			Span<char> span = into.Slice(wrote);
			Span<char> into5 = span;
			handler = new FormatIntoInterpolatedStringHandler(42, 2, span, out enabled);
			if (enabled && handler.AppendFormatted(newLine) && handler.AppendLiteral("System.BadImageFormatException.FileName = "))
			{
				handler.AppendFormatted(ex4.FileName);
			}
			else
				_ = 0;
			if (!Into(into5, out wrote2, ref handler))
			{
				return false;
			}
			wrote += wrote2;
		}
		return true;
	}

	private static bool TryFormatType(Type type, Span<char> into, out int wrote)
	{
		wrote = 0;
		string fullName = type.FullName;
		if (fullName == null)
		{
			return true;
		}
		if (into.Length < fullName.Length)
		{
			return false;
		}
		fullName.AsSpan().CopyTo(into);
		wrote = fullName.Length;
		return true;
	}

	private static bool TryFormatMethodInfo(MethodInfo method, Span<char> into, out int wrote)
	{
		Type returnType = method.ReturnType;
		wrote = 0;
		if (!TryFormatType(returnType, into.Slice(wrote), out var wrote2))
		{
			return false;
		}
		wrote += wrote2;
		if (into.Slice(wrote).Length < 1)
		{
			return false;
		}
		into[wrote++] = ' ';
		if (!TryFormatMethodBase(method, into.Slice(wrote), out wrote2))
		{
			return false;
		}
		wrote += wrote2;
		return true;
	}

	private static bool TryFormatMemberInfoName(MemberInfo member, Span<char> into, out int wrote)
	{
		wrote = 0;
		Type declaringType = member.DeclaringType;
		if ((object)declaringType != null)
		{
			if (!TryFormatType(declaringType, into.Slice(wrote), out var wrote2))
			{
				return false;
			}
			wrote += wrote2;
			if (into.Slice(wrote).Length < 1)
			{
				return false;
			}
			into[wrote++] = ':';
		}
		string name = member.Name;
		if (into.Slice(wrote).Length < name.Length)
		{
			return false;
		}
		name.AsSpan().CopyTo(into.Slice(wrote));
		wrote += name.Length;
		return true;
	}

	private static bool TryFormatMethodBase(MethodBase method, Span<char> into, out int wrote)
	{
		wrote = 0;
		if (!TryFormatMemberInfoName(method, into.Slice(wrote), out var wrote2))
		{
			return false;
		}
		wrote += wrote2;
		if (method.IsGenericMethod)
		{
			if (into.Slice(wrote).Length < 1)
			{
				return false;
			}
			into[wrote++] = '<';
			Type[] genericArguments = method.GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (i != 0)
				{
					if (into.Slice(wrote).Length < 2)
					{
						return false;
					}
					into[wrote++] = ',';
					into[wrote++] = ' ';
				}
				if (!TryFormatType(genericArguments[i], into.Slice(wrote), out wrote2))
				{
					return false;
				}
				wrote += wrote2;
			}
			if (into.Slice(wrote).Length < 1)
			{
				return false;
			}
			into[wrote++] = '>';
		}
		ParameterInfo[] parameters = method.GetParameters();
		if (into.Slice(wrote).Length < 1)
		{
			return false;
		}
		into[wrote++] = '(';
		for (int j = 0; j < parameters.Length; j++)
		{
			if (j != 0)
			{
				if (into.Slice(wrote).Length < 2)
				{
					return false;
				}
				into[wrote++] = ',';
				into[wrote++] = ' ';
			}
			if (!TryFormatType(parameters[j].ParameterType, into.Slice(wrote), out wrote2))
			{
				return false;
			}
			wrote += wrote2;
		}
		if (into.Slice(wrote).Length < 1)
		{
			return false;
		}
		into[wrote++] = ')';
		return true;
	}

	private static bool TryFormatFieldInfo(FieldInfo field, Span<char> into, out int wrote)
	{
		wrote = 0;
		if (!TryFormatType(field.FieldType, into.Slice(wrote), out var wrote2))
		{
			return false;
		}
		wrote += wrote2;
		if (into.Slice(wrote).Length < 1)
		{
			return false;
		}
		into[wrote++] = ' ';
		if (!TryFormatMemberInfoName(field, into.Slice(wrote), out wrote2))
		{
			return false;
		}
		wrote += wrote2;
		return true;
	}

	private static bool TryFormatPropertyInfo(PropertyInfo prop, Span<char> into, out int wrote)
	{
		wrote = 0;
		if (!TryFormatType(prop.PropertyType, into.Slice(wrote), out var wrote2))
		{
			return false;
		}
		wrote += wrote2;
		if (into.Slice(wrote).Length < 1)
		{
			return false;
		}
		into[wrote++] = ' ';
		if (!TryFormatMemberInfoName(prop, into.Slice(wrote), out wrote2))
		{
			return false;
		}
		wrote += wrote2;
		bool canRead = prop.CanRead;
		bool canWrite = prop.CanWrite;
		int num = 5 + (canRead ? 4 : 0) + (canWrite ? 4 : 0) + ((canRead && canWrite) ? 1 : 0);
		if (into.Slice(wrote).Length < num)
		{
			return false;
		}
		" { ".AsSpan().CopyTo(into.Slice(wrote));
		wrote += 3;
		if (canRead)
		{
			"get;".AsSpan().CopyTo(into.Slice(wrote));
			wrote += 4;
		}
		if (canRead && canWrite)
		{
			into[wrote++] = ' ';
		}
		if (canWrite)
		{
			"set;".AsSpan().CopyTo(into.Slice(wrote));
			wrote += 4;
		}
		" }".AsSpan().CopyTo(into.Slice(wrote));
		wrote += 2;
		return true;
	}

	public static string Format(ref FormatInterpolatedStringHandler handler)
	{
		return handler.ToStringAndClear();
	}

	public static bool Into(Span<char> into, out int wrote, [InterpolatedStringHandlerArgument("into")] ref FormatIntoInterpolatedStringHandler handler)
	{
		wrote = handler.pos;
		return !handler.incomplete;
	}
}
