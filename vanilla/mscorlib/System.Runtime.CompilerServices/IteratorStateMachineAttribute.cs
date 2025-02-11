namespace System.Runtime.CompilerServices;

/// <summary>Indicates whether a method in Visual Basic is marked with the Iterator modifier.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class IteratorStateMachineAttribute : StateMachineAttribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.IteratorStateMachineAttribute" /> class.</summary>
	/// <param name="stateMachineType">The type object for the underlying state machine type that's used to implement a state machine method.</param>
	public IteratorStateMachineAttribute(Type stateMachineType)
		: base(stateMachineType)
	{
	}
}
