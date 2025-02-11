#define DEBUG
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace System.Runtime.CompilerServices;

/// <summary>Provides methods that the binary rewriter uses to handle contract failures.</summary>
public static class ContractHelper
{
	private static volatile EventHandler<ContractFailedEventArgs> contractFailedEvent;

	private static readonly object lockObject = new object();

	internal const int COR_E_CODECONTRACTFAILED = -2146233022;

	internal static event EventHandler<ContractFailedEventArgs> InternalContractFailed
	{
		[SecurityCritical]
		add
		{
			RuntimeHelpers.PrepareContractedDelegate(value);
			lock (lockObject)
			{
				contractFailedEvent = (EventHandler<ContractFailedEventArgs>)Delegate.Combine(contractFailedEvent, value);
			}
		}
		[SecurityCritical]
		remove
		{
			lock (lockObject)
			{
				contractFailedEvent = (EventHandler<ContractFailedEventArgs>)Delegate.Remove(contractFailedEvent, value);
			}
		}
	}

	/// <summary>Used by the binary rewriter to activate the default failure behavior.</summary>
	/// <returns>A null reference (Nothing in Visual Basic) if the event was handled and should not trigger a failure; otherwise, returns the localized failure message.</returns>
	/// <param name="failureKind">One of the enumeration values that specifies the type of failure.</param>
	/// <param name="userMessage">Additional user information.</param>
	/// <param name="conditionText">The description of the condition that caused the failure.</param>
	/// <param name="innerException">The inner exception that caused the current exception.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="failureKind" /> is not a valid <see cref="T:System.Diagnostics.Contracts.ContractFailureKind" /> value.</exception>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	[DebuggerNonUserCode]
	public static string RaiseContractFailedEvent(ContractFailureKind failureKind, string userMessage, string conditionText, Exception innerException)
	{
		string resultFailureMessage = "Contract failed";
		RaiseContractFailedEventImplementation(failureKind, userMessage, conditionText, innerException, ref resultFailureMessage);
		return resultFailureMessage;
	}

	/// <summary>Triggers the default failure behavior.</summary>
	/// <param name="kind">One of the enumeration values that specifies the type of failure.</param>
	/// <param name="displayMessage">The message to display.</param>
	/// <param name="userMessage">Additional user information.</param>
	/// <param name="conditionText">The description of the condition that caused the failure.</param>
	/// <param name="innerException">The inner exception that caused the current exception.</param>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[DebuggerNonUserCode]
	public static void TriggerFailure(ContractFailureKind kind, string displayMessage, string userMessage, string conditionText, Exception innerException)
	{
		TriggerFailureImplementation(kind, displayMessage, userMessage, conditionText, innerException);
	}

	[SecuritySafeCritical]
	[DebuggerNonUserCode]
	private static void RaiseContractFailedEventImplementation(ContractFailureKind failureKind, string userMessage, string conditionText, Exception innerException, ref string resultFailureMessage)
	{
		if (failureKind < ContractFailureKind.Precondition || failureKind > ContractFailureKind.Assume)
		{
			throw new ArgumentException(Environment.GetResourceString("Illegal enum value: {0}.", failureKind), "failureKind");
		}
		string text = "contract failed.";
		ContractFailedEventArgs contractFailedEventArgs = null;
		RuntimeHelpers.PrepareConstrainedRegions();
		string text2;
		try
		{
			text = GetDisplayMessage(failureKind, userMessage, conditionText);
			EventHandler<ContractFailedEventArgs> eventHandler = contractFailedEvent;
			if (eventHandler != null)
			{
				contractFailedEventArgs = new ContractFailedEventArgs(failureKind, text, conditionText, innerException);
				Delegate[] invocationList = eventHandler.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					EventHandler<ContractFailedEventArgs> eventHandler2 = (EventHandler<ContractFailedEventArgs>)invocationList[i];
					try
					{
						eventHandler2(null, contractFailedEventArgs);
					}
					catch (Exception thrownDuringHandler)
					{
						contractFailedEventArgs.thrownDuringHandler = thrownDuringHandler;
						contractFailedEventArgs.SetUnwind();
					}
				}
				if (contractFailedEventArgs.Unwind)
				{
					if (Environment.IsCLRHosted)
					{
						TriggerCodeContractEscalationPolicy(failureKind, text, conditionText, innerException);
					}
					if (innerException == null)
					{
						innerException = contractFailedEventArgs.thrownDuringHandler;
					}
					throw new ContractException(failureKind, text, userMessage, conditionText, innerException);
				}
			}
		}
		finally
		{
			text2 = ((contractFailedEventArgs == null || !contractFailedEventArgs.Handled) ? text : null);
		}
		resultFailureMessage = text2;
	}

	[DebuggerNonUserCode]
	[SecuritySafeCritical]
	private static void TriggerFailureImplementation(ContractFailureKind kind, string displayMessage, string userMessage, string conditionText, Exception innerException)
	{
		if (Environment.IsCLRHosted)
		{
			TriggerCodeContractEscalationPolicy(kind, displayMessage, conditionText, innerException);
			throw new ContractException(kind, displayMessage, userMessage, conditionText, innerException);
		}
		if (!Environment.UserInteractive)
		{
			throw new ContractException(kind, displayMessage, userMessage, conditionText, innerException);
		}
		string resourceString = Environment.GetResourceString(GetResourceNameForFailure(kind));
		Assert.Fail(conditionText, displayMessage, resourceString, -2146233022, StackTrace.TraceFormat.Normal, 2);
	}

	private static string GetResourceNameForFailure(ContractFailureKind failureKind, bool withCondition = false)
	{
		string text = null;
		switch (failureKind)
		{
		case ContractFailureKind.Assert:
			return withCondition ? "Assertion failed: {0}" : "Assertion failed.";
		case ContractFailureKind.Assume:
			return withCondition ? "Assumption failed: {0}" : "Assumption failed.";
		case ContractFailureKind.Precondition:
			return withCondition ? "Precondition failed: {0}" : "Precondition failed.";
		case ContractFailureKind.Postcondition:
			return withCondition ? "Postcondition failed: {0}" : "Postcondition failed.";
		case ContractFailureKind.Invariant:
			return withCondition ? "Invariant failed: {0}" : "Invariant failed.";
		case ContractFailureKind.PostconditionOnException:
			return withCondition ? "Postcondition failed after throwing an exception: {0}" : "Postcondition failed after throwing an exception.";
		default:
			Contract.Assume(condition: false, "Unreachable code");
			return "Assumption failed.";
		}
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	private static string GetDisplayMessage(ContractFailureKind failureKind, string userMessage, string conditionText)
	{
		string resourceNameForFailure = GetResourceNameForFailure(failureKind, !string.IsNullOrEmpty(conditionText));
		string text = (string.IsNullOrEmpty(conditionText) ? Environment.GetResourceString(resourceNameForFailure) : Environment.GetResourceString(resourceNameForFailure, conditionText));
		if (!string.IsNullOrEmpty(userMessage))
		{
			return text + "  " + userMessage;
		}
		return text;
	}

	[DebuggerNonUserCode]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SecuritySafeCritical]
	private static void TriggerCodeContractEscalationPolicy(ContractFailureKind failureKind, string message, string conditionText, Exception innerException)
	{
		string exceptionAsString = null;
		if (innerException != null)
		{
			exceptionAsString = innerException.ToString();
		}
		Environment.TriggerCodeContractFailure(failureKind, message, conditionText, exceptionAsString);
	}
}
