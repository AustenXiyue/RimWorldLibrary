namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
public sealed class AsyncMethodBuilderAttribute : Attribute
{
	public Type BuilderType { get; }

	public AsyncMethodBuilderAttribute(Type builderType)
	{
		BuilderType = builderType;
	}
}
