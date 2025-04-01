namespace System.Runtime.InteropServices;

/// <summary>Provides support for user customization of interop stubs in managed-to-COM interop scenarios.</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
[ComVisible(false)]
public sealed class ManagedToNativeComInteropStubAttribute : Attribute
{
	internal Type _classType;

	internal string _methodName;

	/// <summary>Gets the class that contains the required stub method.</summary>
	/// <returns>The class that contains the customized interop stub.</returns>
	public Type ClassType => _classType;

	/// <summary>Gets the name of the stub method.</summary>
	/// <returns>The name of a customized interop stub.</returns>
	public string MethodName => _methodName;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.ManagedToNativeComInteropStubAttribute" /> class with the specified class type and method name.</summary>
	/// <param name="classType">The class that contains the required stub method. </param>
	/// <param name="methodName">The name of the stub method.</param>
	/// <exception cref="T:System.ArgumentException">The stub method is not in the same assembly as the interface that contains the managed interop method.-or-<paramref name="classType" /> is a generic type.-or-<paramref name="classType" /> is an interface. </exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="methodName" /> cannot be found.-or-The method is not static or non-generic.-or-The method's parameter list does not match the expected parameter list for the stub.</exception>
	/// <exception cref="T:System.MethodAccessException">The interface that contains the managed interop method has no access to the stub method, because the stub method has private or protected accessibility, or because of a security issue.</exception>
	public ManagedToNativeComInteropStubAttribute(Type classType, string methodName)
	{
		_classType = classType;
		_methodName = methodName;
	}
}
