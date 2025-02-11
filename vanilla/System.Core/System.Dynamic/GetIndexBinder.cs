using System.Dynamic.Utils;

namespace System.Dynamic;

/// <summary>Represents the dynamic get index operation at the call site, providing the binding semantic and the details about the operation.</summary>
public abstract class GetIndexBinder : DynamicMetaObjectBinder
{
	/// <summary>The result type of the operation.</summary>
	/// <returns>The <see cref="T:System.Type" /> object representing the result type of the operation.</returns>
	public sealed override Type ReturnType => typeof(object);

	/// <summary>Gets the signature of the arguments at the call site.</summary>
	/// <returns>The signature of the arguments at the call site.</returns>
	public CallInfo CallInfo { get; }

	internal sealed override bool IsStandardBinder => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.GetIndexBinder" />.</summary>
	/// <param name="callInfo">The signature of the arguments at the call site.</param>
	protected GetIndexBinder(CallInfo callInfo)
	{
		ContractUtils.RequiresNotNull(callInfo, "callInfo");
		CallInfo = callInfo;
	}

	/// <summary>Performs the binding of the dynamic get index operation.</summary>
	/// <returns>The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="target">The target of the dynamic get index operation.</param>
	/// <param name="args">An array of arguments of the dynamic get index operation.</param>
	public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
	{
		ContractUtils.RequiresNotNull(target, "target");
		ContractUtils.RequiresNotNullItems(args, "args");
		return target.BindGetIndex(this, args);
	}

	/// <summary>Performs the binding of the dynamic get index operation if the target dynamic object cannot bind.</summary>
	/// <returns>The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="target">The target of the dynamic get index operation.</param>
	/// <param name="indexes">The arguments of the dynamic get index operation.</param>
	public DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes)
	{
		return FallbackGetIndex(target, indexes, null);
	}

	/// <summary>When overridden in the derived class, performs the binding of the dynamic get index operation if the target dynamic object cannot bind.</summary>
	/// <returns>The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="target">The target of the dynamic get index operation.</param>
	/// <param name="indexes">The arguments of the dynamic get index operation.</param>
	/// <param name="errorSuggestion">The binding result to use if binding fails, or null.</param>
	public abstract DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion);
}
