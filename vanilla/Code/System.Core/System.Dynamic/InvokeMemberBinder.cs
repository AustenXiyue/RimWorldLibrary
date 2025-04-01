using System.Dynamic.Utils;

namespace System.Dynamic;

/// <summary>Represents the invoke member dynamic operation at the call site, providing the binding semantic and the details about the operation.</summary>
public abstract class InvokeMemberBinder : DynamicMetaObjectBinder
{
	/// <summary>The result type of the operation.</summary>
	/// <returns>The <see cref="T:System.Type" /> object representing the result type of the operation.</returns>
	public sealed override Type ReturnType => typeof(object);

	/// <summary>Gets the name of the member to invoke.</summary>
	/// <returns>The name of the member to invoke.</returns>
	public string Name { get; }

	/// <summary>Gets the value indicating if the string comparison should ignore the case of the member name.</summary>
	/// <returns>True if the case is ignored, otherwise false.</returns>
	public bool IgnoreCase { get; }

	/// <summary>Gets the signature of the arguments at the call site.</summary>
	/// <returns>The signature of the arguments at the call site.</returns>
	public CallInfo CallInfo { get; }

	internal sealed override bool IsStandardBinder => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.InvokeMemberBinder" />.</summary>
	/// <param name="name">The name of the member to invoke.</param>
	/// <param name="ignoreCase">true if the name should be matched ignoring case; false otherwise.</param>
	/// <param name="callInfo">The signature of the arguments at the call site.</param>
	protected InvokeMemberBinder(string name, bool ignoreCase, CallInfo callInfo)
	{
		ContractUtils.RequiresNotNull(name, "name");
		ContractUtils.RequiresNotNull(callInfo, "callInfo");
		Name = name;
		IgnoreCase = ignoreCase;
		CallInfo = callInfo;
	}

	/// <summary>Performs the binding of the dynamic invoke member operation.</summary>
	/// <returns>The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="target">The target of the dynamic invoke member operation.</param>
	/// <param name="args">An array of arguments of the dynamic invoke member operation.</param>
	public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
	{
		ContractUtils.RequiresNotNull(target, "target");
		ContractUtils.RequiresNotNullItems(args, "args");
		return target.BindInvokeMember(this, args);
	}

	/// <summary>Performs the binding of the dynamic invoke member operation if the target dynamic object cannot bind.</summary>
	/// <returns>The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="target">The target of the dynamic invoke member operation.</param>
	/// <param name="args">The arguments of the dynamic invoke member operation.</param>
	public DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args)
	{
		return FallbackInvokeMember(target, args, null);
	}

	/// <summary>When overridden in the derived class, performs the binding of the dynamic invoke member operation if the target dynamic object cannot bind.</summary>
	/// <returns>The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="target">The target of the dynamic invoke member operation.</param>
	/// <param name="args">The arguments of the dynamic invoke member operation.</param>
	/// <param name="errorSuggestion">The binding result to use if binding fails, or null.</param>
	public abstract DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);

	/// <summary>When overridden in the derived class, performs the binding of the dynamic invoke operation if the target dynamic object cannot bind.</summary>
	/// <returns>The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="target">The target of the dynamic invoke operation.</param>
	/// <param name="args">The arguments of the dynamic invoke operation.</param>
	/// <param name="errorSuggestion">The binding result to use if binding fails, or null.</param>
	public abstract DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);
}
