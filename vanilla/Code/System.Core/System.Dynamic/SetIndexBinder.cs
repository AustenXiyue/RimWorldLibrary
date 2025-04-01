using System.Dynamic.Utils;

namespace System.Dynamic;

/// <summary>Represents the dynamic set index operation at the call site, providing the binding semantic and the details about the operation.</summary>
public abstract class SetIndexBinder : DynamicMetaObjectBinder
{
	/// <summary>The result type of the operation.</summary>
	/// <returns>The <see cref="T:System.Type" /> object representing the result type of the operation.</returns>
	public sealed override Type ReturnType => typeof(object);

	/// <summary>Gets the signature of the arguments at the call site.</summary>
	/// <returns>The signature of the arguments at the call site.</returns>
	public CallInfo CallInfo { get; }

	internal sealed override bool IsStandardBinder => true;

	/// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.SetIndexBinder" />.</summary>
	/// <param name="callInfo">The signature of the arguments at the call site.</param>
	protected SetIndexBinder(CallInfo callInfo)
	{
		ContractUtils.RequiresNotNull(callInfo, "callInfo");
		CallInfo = callInfo;
	}

	/// <summary>Performs the binding of the dynamic set index operation.</summary>
	/// <returns>The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="target">The target of the dynamic set index operation.</param>
	/// <param name="args">An array of arguments of the dynamic set index operation.</param>
	public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
	{
		ContractUtils.RequiresNotNull(target, "target");
		ContractUtils.RequiresNotNull(args, "args");
		ContractUtils.Requires(args.Length >= 2, "args");
		DynamicMetaObject value = args[args.Length - 1];
		DynamicMetaObject[] array = args.RemoveLast();
		ContractUtils.RequiresNotNull(value, "args");
		ContractUtils.RequiresNotNullItems(array, "args");
		return target.BindSetIndex(this, array, value);
	}

	/// <summary>Performs the binding of the dynamic set index operation if the target dynamic object cannot bind.</summary>
	/// <returns>The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="target">The target of the dynamic set index operation.</param>
	/// <param name="indexes">The arguments of the dynamic set index operation.</param>
	/// <param name="value">The value to set to the collection.</param>
	public DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value)
	{
		return FallbackSetIndex(target, indexes, value, null);
	}

	/// <summary>When overridden in the derived class, performs the binding of the dynamic set index operation if the target dynamic object cannot bind.</summary>
	/// <returns>The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="target">The target of the dynamic set index operation.</param>
	/// <param name="indexes">The arguments of the dynamic set index operation.</param>
	/// <param name="value">The value to set to the collection.</param>
	/// <param name="errorSuggestion">The binding result to use if binding fails, or null.</param>
	public abstract DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion);
}
