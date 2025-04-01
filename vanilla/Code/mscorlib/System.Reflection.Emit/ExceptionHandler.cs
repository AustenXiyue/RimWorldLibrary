using System.Runtime.InteropServices;

namespace System.Reflection.Emit;

/// <summary>Represents an exception handler in a byte array of IL to be passed to a method such as <see cref="M:System.Reflection.Emit.MethodBuilder.SetMethodBody(System.Byte[],System.Int32,System.Byte[],System.Collections.Generic.IEnumerable{System.Reflection.Emit.ExceptionHandler},System.Collections.Generic.IEnumerable{System.Int32})" />.</summary>
[ComVisible(false)]
public struct ExceptionHandler : IEquatable<ExceptionHandler>
{
	internal readonly int m_exceptionClass;

	internal readonly int m_tryStartOffset;

	internal readonly int m_tryEndOffset;

	internal readonly int m_filterOffset;

	internal readonly int m_handlerStartOffset;

	internal readonly int m_handlerEndOffset;

	internal readonly ExceptionHandlingClauseOptions m_kind;

	/// <summary>Gets the token of the exception type handled by this handler.</summary>
	/// <returns>The token of the exception type handled by this handler, or 0 if none exists.</returns>
	public int ExceptionTypeToken => m_exceptionClass;

	/// <summary>Gets the byte offset at which the code that is protected by this exception handler begins.</summary>
	/// <returns>The byte offset at which the code that is protected by this exception handler begins.</returns>
	public int TryOffset => m_tryStartOffset;

	/// <summary>Gets the length, in bytes, of the code protected by this exception handler.</summary>
	/// <returns>The length, in bytes, of the code protected by this exception handler.</returns>
	public int TryLength => m_tryEndOffset - m_tryStartOffset;

	/// <summary>Gets the byte offset at which the filter code for the exception handler begins.</summary>
	/// <returns>The byte offset at which the filter code begins, or 0 if no filter  is present.</returns>
	public int FilterOffset => m_filterOffset;

	/// <summary>Gets the byte offset of the first instruction of the exception handler.</summary>
	/// <returns>The byte offset of the first instruction of the exception handler.</returns>
	public int HandlerOffset => m_handlerStartOffset;

	/// <summary>Gets the length, in bytes, of the exception handler.</summary>
	/// <returns>The length, in bytes, of the exception handler.</returns>
	public int HandlerLength => m_handlerEndOffset - m_handlerStartOffset;

	/// <summary>Gets a value that represents the kind of exception handler this object represents.</summary>
	/// <returns>One of the enumeration values that specifies the kind of exception handler.</returns>
	public ExceptionHandlingClauseOptions Kind => m_kind;

	/// <summary>Initializes a new instance of the <see cref="T:System.Reflection.Emit.ExceptionHandler" /> class with the specified parameters.</summary>
	/// <param name="tryOffset">The byte offset of the first instruction protected by this exception handler.</param>
	/// <param name="tryLength">The number of bytes protected by this exception handler.</param>
	/// <param name="filterOffset">The byte offset of the beginning of the filter code. The filter code ends at the first instruction of the handler block. For non-filter exception handlers, specify 0 (zero) for this parameter.</param>
	/// <param name="handlerOffset">The byte offset of the first instruction of this exception handler.</param>
	/// <param name="handlerLength">The number of bytes in this exception handler.</param>
	/// <param name="kind">One of the enumeration values that specifies the kind of exception handler.</param>
	/// <param name="exceptionTypeToken">The token of the exception type handled by this exception handler. If not applicable, specify 0 (zero).</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="tryOffset" />, <paramref name="filterOffset" />, <paramref name="handlerOffset" />, <paramref name="tryLength" />, or <paramref name="handlerLength" /> are negative.</exception>
	public ExceptionHandler(int tryOffset, int tryLength, int filterOffset, int handlerOffset, int handlerLength, ExceptionHandlingClauseOptions kind, int exceptionTypeToken)
	{
		if (tryOffset < 0)
		{
			throw new ArgumentOutOfRangeException("tryOffset", Environment.GetResourceString("Non-negative number required."));
		}
		if (tryLength < 0)
		{
			throw new ArgumentOutOfRangeException("tryLength", Environment.GetResourceString("Non-negative number required."));
		}
		if (filterOffset < 0)
		{
			throw new ArgumentOutOfRangeException("filterOffset", Environment.GetResourceString("Non-negative number required."));
		}
		if (handlerOffset < 0)
		{
			throw new ArgumentOutOfRangeException("handlerOffset", Environment.GetResourceString("Non-negative number required."));
		}
		if (handlerLength < 0)
		{
			throw new ArgumentOutOfRangeException("handlerLength", Environment.GetResourceString("Non-negative number required."));
		}
		if ((long)tryOffset + (long)tryLength > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("tryLength", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 0, int.MaxValue - tryOffset));
		}
		if ((long)handlerOffset + (long)handlerLength > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("handlerLength", Environment.GetResourceString("Valid values are between {0} and {1}, inclusive.", 0, int.MaxValue - handlerOffset));
		}
		if (kind == ExceptionHandlingClauseOptions.Clause && (exceptionTypeToken & 0xFFFFFF) == 0)
		{
			throw new ArgumentException(Environment.GetResourceString("Token {0:x} is not a valid Type token.", exceptionTypeToken), "exceptionTypeToken");
		}
		if (!IsValidKind(kind))
		{
			throw new ArgumentOutOfRangeException("kind", Environment.GetResourceString("Enum value was out of legal range."));
		}
		m_tryStartOffset = tryOffset;
		m_tryEndOffset = tryOffset + tryLength;
		m_filterOffset = filterOffset;
		m_handlerStartOffset = handlerOffset;
		m_handlerEndOffset = handlerOffset + handlerLength;
		m_kind = kind;
		m_exceptionClass = exceptionTypeToken;
	}

	internal ExceptionHandler(int tryStartOffset, int tryEndOffset, int filterOffset, int handlerStartOffset, int handlerEndOffset, int kind, int exceptionTypeToken)
	{
		m_tryStartOffset = tryStartOffset;
		m_tryEndOffset = tryEndOffset;
		m_filterOffset = filterOffset;
		m_handlerStartOffset = handlerStartOffset;
		m_handlerEndOffset = handlerEndOffset;
		m_kind = (ExceptionHandlingClauseOptions)kind;
		m_exceptionClass = exceptionTypeToken;
	}

	private static bool IsValidKind(ExceptionHandlingClauseOptions kind)
	{
		if ((uint)kind <= 2u || kind == ExceptionHandlingClauseOptions.Fault)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_exceptionClass ^ m_tryStartOffset ^ m_tryEndOffset ^ m_filterOffset ^ m_handlerStartOffset ^ m_handlerEndOffset ^ (int)m_kind;
	}

	/// <summary>Indicates whether this instance of the <see cref="T:System.Reflection.Emit.ExceptionHandler" /> object is equal to a specified object.</summary>
	/// <returns>true if <paramref name="obj" /> and this instance are equal; otherwise, false.</returns>
	/// <param name="obj">The object to compare this instance to.</param>
	public override bool Equals(object obj)
	{
		if (obj is ExceptionHandler)
		{
			return Equals((ExceptionHandler)obj);
		}
		return false;
	}

	/// <summary>Indicates whether this instance of the <see cref="T:System.Reflection.Emit.ExceptionHandler" /> object is equal to another <see cref="T:System.Reflection.Emit.ExceptionHandler" /> object.</summary>
	/// <returns>true if <paramref name="other" /> and this instance are equal; otherwise, false.</returns>
	/// <param name="other">The exception handler object to compare this instance to.</param>
	public bool Equals(ExceptionHandler other)
	{
		if (other.m_exceptionClass == m_exceptionClass && other.m_tryStartOffset == m_tryStartOffset && other.m_tryEndOffset == m_tryEndOffset && other.m_filterOffset == m_filterOffset && other.m_handlerStartOffset == m_handlerStartOffset && other.m_handlerEndOffset == m_handlerEndOffset)
		{
			return other.m_kind == m_kind;
		}
		return false;
	}

	/// <summary>Determines whether two specified instances of <see cref="T:System.Reflection.Emit.ExceptionHandler" /> are equal.</summary>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator ==(ExceptionHandler left, ExceptionHandler right)
	{
		return left.Equals(right);
	}

	/// <summary>Determines whether two specified instances of <see cref="T:System.Reflection.Emit.ExceptionHandler" /> are not equal.</summary>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator !=(ExceptionHandler left, ExceptionHandler right)
	{
		return !left.Equals(right);
	}
}
