using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace System.Diagnostics.Contracts;

/// <summary>Contains static methods for representing program contracts such as preconditions, postconditions, and object invariants.</summary>
public static class Contract
{
	[ThreadStatic]
	private static bool _assertingMustUseRewriter;

	/// <summary>Occurs when a contract fails.</summary>
	public static event EventHandler<ContractFailedEventArgs> ContractFailed
	{
		[SecurityCritical]
		add
		{
			ContractHelper.InternalContractFailed += value;
		}
		[SecurityCritical]
		remove
		{
			ContractHelper.InternalContractFailed -= value;
		}
	}

	/// <summary>Instructs code analysis tools to assume that the specified condition is true, even if it cannot be statically proven to always be true.</summary>
	/// <param name="condition">The conditional expression to assume true.</param>
	[Conditional("CONTRACTS_FULL")]
	[Conditional("DEBUG")]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static void Assume(bool condition)
	{
		if (!condition)
		{
			ReportFailure(ContractFailureKind.Assume, null, null, null);
		}
	}

	/// <summary>Instructs code analysis tools to assume that a condition is true, even if it cannot be statically proven to always be true, and displays a message if the assumption fails.</summary>
	/// <param name="condition">The conditional expression to assume true.</param>
	/// <param name="userMessage">The message to post if the assumption fails.</param>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[Conditional("DEBUG")]
	[Conditional("CONTRACTS_FULL")]
	public static void Assume(bool condition, string userMessage)
	{
		if (!condition)
		{
			ReportFailure(ContractFailureKind.Assume, userMessage, null, null);
		}
	}

	/// <summary>Checks for a condition; if the condition is false, follows the escalation policy set for the analyzer.</summary>
	/// <param name="condition">The conditional expression to test.</param>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[Conditional("CONTRACTS_FULL")]
	[Conditional("DEBUG")]
	public static void Assert(bool condition)
	{
		if (!condition)
		{
			ReportFailure(ContractFailureKind.Assert, null, null, null);
		}
	}

	/// <summary>Checks for a condition; if the condition is false, follows the escalation policy set by the analyzer and displays the specified message.</summary>
	/// <param name="condition">The conditional expression to test.</param>
	/// <param name="userMessage">A message to display if the condition is not met.</param>
	[Conditional("DEBUG")]
	[Conditional("CONTRACTS_FULL")]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static void Assert(bool condition, string userMessage)
	{
		if (!condition)
		{
			ReportFailure(ContractFailureKind.Assert, userMessage, null, null);
		}
	}

	/// <summary>Specifies a precondition contract for the enclosing method or property.</summary>
	/// <param name="condition">The conditional expression to test.</param>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[Conditional("CONTRACTS_FULL")]
	public static void Requires(bool condition)
	{
		AssertMustUseRewriter(ContractFailureKind.Precondition, "Requires");
	}

	/// <summary>Specifies a precondition contract for the enclosing method or property, and displays a message if the condition for the contract fails.</summary>
	/// <param name="condition">The conditional expression to test.</param>
	/// <param name="userMessage">The message to display if the condition is false.</param>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[Conditional("CONTRACTS_FULL")]
	public static void Requires(bool condition, string userMessage)
	{
		AssertMustUseRewriter(ContractFailureKind.Precondition, "Requires");
	}

	/// <summary>Specifies a precondition contract for the enclosing method or property, and throws an exception if the condition for the contract fails.</summary>
	/// <param name="condition">The conditional expression to test.</param>
	/// <typeparam name="TException">The exception to throw if the condition is false.</typeparam>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static void Requires<TException>(bool condition) where TException : Exception
	{
		AssertMustUseRewriter(ContractFailureKind.Precondition, "Requires<TException>");
	}

	/// <summary>Specifies a precondition contract for the enclosing method or property, and throws an exception with the provided message if the condition for the contract fails.</summary>
	/// <param name="condition">The conditional expression to test.</param>
	/// <param name="userMessage">The message to display if the condition is false.</param>
	/// <typeparam name="TException">The exception to throw if the condition is false.</typeparam>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static void Requires<TException>(bool condition, string userMessage) where TException : Exception
	{
		AssertMustUseRewriter(ContractFailureKind.Precondition, "Requires<TException>");
	}

	/// <summary>Specifies a postcondition contract for the enclosing method or property.</summary>
	/// <param name="condition">The conditional expression to test. The expression may include <see cref="M:System.Diagnostics.Contracts.Contract.OldValue``1(``0)" />, <see cref="M:System.Diagnostics.Contracts.Contract.ValueAtReturn``1(``0@)" />, and <see cref="M:System.Diagnostics.Contracts.Contract.Result``1" /> values. </param>
	[Conditional("CONTRACTS_FULL")]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static void Ensures(bool condition)
	{
		AssertMustUseRewriter(ContractFailureKind.Postcondition, "Ensures");
	}

	/// <summary>Specifies a postcondition contract for a provided exit condition and a message to display if the condition is false.</summary>
	/// <param name="condition">The conditional expression to test. The expression may include <see cref="M:System.Diagnostics.Contracts.Contract.OldValue``1(``0)" /> and <see cref="M:System.Diagnostics.Contracts.Contract.Result``1" /> values. </param>
	/// <param name="userMessage">The message to display if the expression is not true.</param>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[Conditional("CONTRACTS_FULL")]
	public static void Ensures(bool condition, string userMessage)
	{
		AssertMustUseRewriter(ContractFailureKind.Postcondition, "Ensures");
	}

	/// <summary>Specifies a postcondition contract for the enclosing method or property, based on the provided exception and condition.</summary>
	/// <param name="condition">The conditional expression to test.</param>
	/// <typeparam name="TException">The type of exception that invokes the postcondition check.</typeparam>
	[Conditional("CONTRACTS_FULL")]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static void EnsuresOnThrow<TException>(bool condition) where TException : Exception
	{
		AssertMustUseRewriter(ContractFailureKind.PostconditionOnException, "EnsuresOnThrow");
	}

	/// <summary>Specifies a postcondition contract and a message to display if the condition is false for the enclosing method or property, based on the provided exception and condition.</summary>
	/// <param name="condition">The conditional expression to test.</param>
	/// <param name="userMessage">The message to display if the expression is false.</param>
	/// <typeparam name="TException">The type of exception that invokes the postcondition check.</typeparam>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[Conditional("CONTRACTS_FULL")]
	public static void EnsuresOnThrow<TException>(bool condition, string userMessage) where TException : Exception
	{
		AssertMustUseRewriter(ContractFailureKind.PostconditionOnException, "EnsuresOnThrow");
	}

	/// <summary>Represents the return value of a method or property.</summary>
	/// <returns>Return value of the enclosing method or property.</returns>
	/// <typeparam name="T">Type of return value of the enclosing method or property.</typeparam>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static T Result<T>()
	{
		return default(T);
	}

	/// <summary>Represents the final (output) value of an out parameter when returning from a method.</summary>
	/// <returns>The output value of the out parameter.</returns>
	/// <param name="value">The out parameter.</param>
	/// <typeparam name="T">The type of the out parameter.</typeparam>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static T ValueAtReturn<T>(out T value)
	{
		value = default(T);
		return value;
	}

	/// <summary>Represents values as they were at the start of a method or property.</summary>
	/// <returns>The value of the parameter or field at the start of a method or property.</returns>
	/// <param name="value">The value to represent (field or parameter).</param>
	/// <typeparam name="T">The type of value.</typeparam>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static T OldValue<T>(T value)
	{
		return default(T);
	}

	/// <summary>Specifies an invariant contract for the enclosing method or property. </summary>
	/// <param name="condition">The conditional expression to test.</param>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[Conditional("CONTRACTS_FULL")]
	public static void Invariant(bool condition)
	{
		AssertMustUseRewriter(ContractFailureKind.Invariant, "Invariant");
	}

	/// <summary>Specifies an invariant contract for the enclosing method or property, and displays a message if the condition for the contract fails.</summary>
	/// <param name="condition">The conditional expression to test.</param>
	/// <param name="userMessage">The message to display if the condition is false.</param>
	[Conditional("CONTRACTS_FULL")]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static void Invariant(bool condition, string userMessage)
	{
		AssertMustUseRewriter(ContractFailureKind.Invariant, "Invariant");
	}

	/// <summary>Determines whether a particular condition is valid for all integers in a specified range.</summary>
	/// <returns>true if <paramref name="predicate" /> returns true for all integers starting from <paramref name="fromInclusive" /> to <paramref name="toExclusive" /> - 1.</returns>
	/// <param name="fromInclusive">The first integer to pass to <paramref name="predicate" />.</param>
	/// <param name="toExclusive">One more than the last integer to pass to <paramref name="predicate" />.</param>
	/// <param name="predicate">The function to evaluate for the existence of the integers in the specified range.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="predicate" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="toExclusive " />is less than <paramref name="fromInclusive" />.</exception>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static bool ForAll(int fromInclusive, int toExclusive, Predicate<int> predicate)
	{
		if (fromInclusive > toExclusive)
		{
			throw new ArgumentException("fromInclusive must be less than or equal to toExclusive.");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		for (int i = fromInclusive; i < toExclusive; i++)
		{
			if (!predicate(i))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Determines whether all the elements in a collection exist within a function.</summary>
	/// <returns>true if and only if <paramref name="predicate" /> returns true for all elements of type <paramref name="T" /> in <paramref name="collection" />.</returns>
	/// <param name="collection">The collection from which elements of type <paramref name="T" /> will be drawn to pass to <paramref name="predicate" />.</param>
	/// <param name="predicate">The function to evaluate for the existence of all the elements in <paramref name="collection" />.</param>
	/// <typeparam name="T">The type that is contained in <paramref name="collection" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> or <paramref name="predicate" /> is null.</exception>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static bool ForAll<T>(IEnumerable<T> collection, Predicate<T> predicate)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		foreach (T item in collection)
		{
			if (!predicate(item))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Determines whether a specified test is true for any integer within a range of integers.</summary>
	/// <returns>true if <paramref name="predicate" /> returns true for any integer starting from <paramref name="fromInclusive" /> to <paramref name="toExclusive" /> - 1.</returns>
	/// <param name="fromInclusive">The first integer to pass to <paramref name="predicate" />.</param>
	/// <param name="toExclusive">One more than the last integer to pass to <paramref name="predicate" />.</param>
	/// <param name="predicate">The function to evaluate for any value of the integer in the specified range.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="predicate" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="toExclusive " />is less than <paramref name="fromInclusive" />.</exception>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static bool Exists(int fromInclusive, int toExclusive, Predicate<int> predicate)
	{
		if (fromInclusive > toExclusive)
		{
			throw new ArgumentException("fromInclusive must be less than or equal to toExclusive.");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		for (int i = fromInclusive; i < toExclusive; i++)
		{
			if (predicate(i))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Determines whether an element within a collection of elements exists within a function.</summary>
	/// <returns>true if and only if <paramref name="predicate" /> returns true for any element of type <paramref name="T" /> in <paramref name="collection" />.</returns>
	/// <param name="collection">The collection from which elements of type <paramref name="T" /> will be drawn to pass to <paramref name="predicate" />.</param>
	/// <param name="predicate">The function to evaluate for an element in <paramref name="collection" />.</param>
	/// <typeparam name="T">The type that is contained in <paramref name="collection" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="collection" /> or <paramref name="predicate" /> is null.</exception>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static bool Exists<T>(IEnumerable<T> collection, Predicate<T> predicate)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		foreach (T item in collection)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Marks the end of the contract section when a method's contracts contain only preconditions in the if-then-throw form.</summary>
	[Conditional("CONTRACTS_FULL")]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static void EndContractBlock()
	{
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[DebuggerNonUserCode]
	private static void ReportFailure(ContractFailureKind failureKind, string userMessage, string conditionText, Exception innerException)
	{
		if (failureKind < ContractFailureKind.Precondition || failureKind > ContractFailureKind.Assume)
		{
			throw new ArgumentException(Environment.GetResourceString("Illegal enum value: {0}.", failureKind), "failureKind");
		}
		string text = ContractHelper.RaiseContractFailedEvent(failureKind, userMessage, conditionText, innerException);
		if (text != null)
		{
			ContractHelper.TriggerFailure(failureKind, text, userMessage, conditionText, innerException);
		}
	}

	[SecuritySafeCritical]
	private static void AssertMustUseRewriter(ContractFailureKind kind, string contractKind)
	{
		if (_assertingMustUseRewriter)
		{
			System.Diagnostics.Assert.Fail("Asserting that we must use the rewriter went reentrant.", "Didn't rewrite this mscorlib?");
		}
		_assertingMustUseRewriter = true;
		Assembly assembly = typeof(Contract).Assembly;
		StackTrace stackTrace = new StackTrace();
		Assembly assembly2 = null;
		for (int i = 0; i < stackTrace.FrameCount; i++)
		{
			Assembly assembly3 = stackTrace.GetFrame(i).GetMethod().DeclaringType.Assembly;
			if (assembly3 != assembly)
			{
				assembly2 = assembly3;
				break;
			}
		}
		if (assembly2 == null)
		{
			assembly2 = assembly;
		}
		string name = assembly2.GetName().Name;
		ContractHelper.TriggerFailure(kind, Environment.GetResourceString("An assembly (probably \"{1}\") must be rewritten using the code contracts binary rewriter (CCRewrite) because it is calling Contract.{0} and the CONTRACTS_FULL symbol is defined.  Remove any explicit definitions of the CONTRACTS_FULL symbol from your project and rebuild.  CCRewrite can be downloaded from http://go.microsoft.com/fwlink/?LinkID=169180. \\r\\nAfter the rewriter is installed, it can be enabled in Visual Studio from the project's Properties page on the Code Contracts pane.  Ensure that \"Perform Runtime Contract Checking\" is enabled, which will define CONTRACTS_FULL.", contractKind, name), null, null, null);
		_assertingMustUseRewriter = false;
	}
}
