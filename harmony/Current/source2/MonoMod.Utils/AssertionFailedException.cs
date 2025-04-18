using System;

namespace MonoMod.Utils;

internal sealed class AssertionFailedException : Exception
{
	private const string AssertFailed = "Assertion failed! ";

	public string Expression { get; } = "";

	public new string Message { get; }

	public AssertionFailedException()
	{
		Message = "";
	}

	public AssertionFailedException(string? message)
		: base("Assertion failed! " + message)
	{
		Message = message ?? "";
	}

	public AssertionFailedException(string? message, Exception innerException)
		: base("Assertion failed! " + message, innerException)
	{
		Message = message ?? "";
	}

	public AssertionFailedException(string? message, string expression)
		: base("Assertion failed! " + expression + " " + message)
	{
		Message = message ?? "";
		Expression = expression;
	}
}
