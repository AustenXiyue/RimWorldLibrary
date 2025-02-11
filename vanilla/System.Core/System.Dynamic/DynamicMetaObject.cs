using System.Collections.Generic;
using System.Dynamic.Utils;
using System.Linq.Expressions;

namespace System.Dynamic;

/// <summary>Represents the dynamic binding and a binding logic of an object participating in the dynamic binding.</summary>
public class DynamicMetaObject
{
	/// <summary>Represents an empty array of type <see cref="T:System.Dynamic.DynamicMetaObject" />. This field is read only.</summary>
	public static readonly DynamicMetaObject[] EmptyMetaObjects = Array.Empty<DynamicMetaObject>();

	/// <summary>The expression representing the <see cref="T:System.Dynamic.DynamicMetaObject" /> during the dynamic binding process.</summary>
	/// <returns>The expression representing the <see cref="T:System.Dynamic.DynamicMetaObject" /> during the dynamic binding process.</returns>
	public Expression Expression { get; }

	/// <summary>The set of binding restrictions under which the binding is valid.</summary>
	/// <returns>The set of binding restrictions.</returns>
	public BindingRestrictions Restrictions { get; }

	/// <summary>The runtime value represented by this <see cref="T:System.Dynamic.DynamicMetaObject" />.</summary>
	/// <returns>The runtime value represented by this <see cref="T:System.Dynamic.DynamicMetaObject" />.</returns>
	public object Value { get; }

	/// <summary>Gets a value indicating whether the <see cref="T:System.Dynamic.DynamicMetaObject" /> has the runtime value.</summary>
	/// <returns>True if the <see cref="T:System.Dynamic.DynamicMetaObject" /> has the runtime value, otherwise false.</returns>
	public bool HasValue { get; }

	/// <summary>Gets the <see cref="T:System.Type" /> of the runtime value or null if the <see cref="T:System.Dynamic.DynamicMetaObject" /> has no value associated with it.</summary>
	/// <returns>The <see cref="T:System.Type" /> of the runtime value or null.</returns>
	public Type RuntimeType
	{
		get
		{
			if (HasValue)
			{
				Type type = Expression.Type;
				if (type.IsValueType)
				{
					return type;
				}
				return Value?.GetType();
			}
			return null;
		}
	}

	/// <summary>Gets the limit type of the <see cref="T:System.Dynamic.DynamicMetaObject" />.</summary>
	/// <returns>
	///   <see cref="P:System.Dynamic.DynamicMetaObject.RuntimeType" /> if runtime value is available, a type of the <see cref="P:System.Dynamic.DynamicMetaObject.Expression" /> otherwise.</returns>
	public Type LimitType => RuntimeType ?? Expression.Type;

	/// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.DynamicMetaObject" /> class.</summary>
	/// <param name="expression">The expression representing this <see cref="T:System.Dynamic.DynamicMetaObject" /> during the dynamic binding process.</param>
	/// <param name="restrictions">The set of binding restrictions under which the binding is valid.</param>
	public DynamicMetaObject(Expression expression, BindingRestrictions restrictions)
	{
		ContractUtils.RequiresNotNull(expression, "expression");
		ContractUtils.RequiresNotNull(restrictions, "restrictions");
		Expression = expression;
		Restrictions = restrictions;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.DynamicMetaObject" /> class.</summary>
	/// <param name="expression">The expression representing this <see cref="T:System.Dynamic.DynamicMetaObject" /> during the dynamic binding process.</param>
	/// <param name="restrictions">The set of binding restrictions under which the binding is valid.</param>
	/// <param name="value">The runtime value represented by the <see cref="T:System.Dynamic.DynamicMetaObject" />.</param>
	public DynamicMetaObject(Expression expression, BindingRestrictions restrictions, object value)
		: this(expression, restrictions)
	{
		Value = value;
		HasValue = true;
	}

	/// <summary>Performs the binding of the dynamic conversion operation.</summary>
	/// <returns>The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="binder">An instance of the <see cref="T:System.Dynamic.ConvertBinder" /> that represents the details of the dynamic operation.</param>
	public virtual DynamicMetaObject BindConvert(ConvertBinder binder)
	{
		ContractUtils.RequiresNotNull(binder, "binder");
		return binder.FallbackConvert(this);
	}

	/// <summary>Performs the binding of the dynamic get member operation.</summary>
	/// <returns>The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="binder">An instance of the <see cref="T:System.Dynamic.GetMemberBinder" /> that represents the details of the dynamic operation.</param>
	public virtual DynamicMetaObject BindGetMember(GetMemberBinder binder)
	{
		ContractUtils.RequiresNotNull(binder, "binder");
		return binder.FallbackGetMember(this);
	}

	/// <summary>Performs the binding of the dynamic set member operation.</summary>
	/// <returns>The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="binder">An instance of the <see cref="T:System.Dynamic.SetMemberBinder" /> that represents the details of the dynamic operation.</param>
	/// <param name="value">The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the value for the set member operation.</param>
	public virtual DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
	{
		ContractUtils.RequiresNotNull(binder, "binder");
		return binder.FallbackSetMember(this, value);
	}

	/// <summary>Performs the binding of the dynamic delete member operation.</summary>
	/// <returns>The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="binder">An instance of the <see cref="T:System.Dynamic.DeleteMemberBinder" /> that represents the details of the dynamic operation.</param>
	public virtual DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
	{
		ContractUtils.RequiresNotNull(binder, "binder");
		return binder.FallbackDeleteMember(this);
	}

	/// <summary>Performs the binding of the dynamic get index operation.</summary>
	/// <returns>The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="binder">An instance of the <see cref="T:System.Dynamic.GetIndexBinder" /> that represents the details of the dynamic operation.</param>
	/// <param name="indexes">An array of <see cref="T:System.Dynamic.DynamicMetaObject" /> instances - indexes for the get index operation.</param>
	public virtual DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
	{
		ContractUtils.RequiresNotNull(binder, "binder");
		return binder.FallbackGetIndex(this, indexes);
	}

	/// <summary>Performs the binding of the dynamic set index operation.</summary>
	/// <returns>The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="binder">An instance of the <see cref="T:System.Dynamic.SetIndexBinder" /> that represents the details of the dynamic operation.</param>
	/// <param name="indexes">An array of <see cref="T:System.Dynamic.DynamicMetaObject" /> instances - indexes for the set index operation.</param>
	/// <param name="value">The <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the value for the set index operation.</param>
	public virtual DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
	{
		ContractUtils.RequiresNotNull(binder, "binder");
		return binder.FallbackSetIndex(this, indexes, value);
	}

	/// <summary>Performs the binding of the dynamic delete index operation.</summary>
	/// <returns>The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="binder">An instance of the <see cref="T:System.Dynamic.DeleteIndexBinder" /> that represents the details of the dynamic operation.</param>
	/// <param name="indexes">An array of <see cref="T:System.Dynamic.DynamicMetaObject" /> instances - indexes for the delete index operation.</param>
	public virtual DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
	{
		ContractUtils.RequiresNotNull(binder, "binder");
		return binder.FallbackDeleteIndex(this, indexes);
	}

	/// <summary>Performs the binding of the dynamic invoke member operation.</summary>
	/// <returns>The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="binder">An instance of the <see cref="T:System.Dynamic.InvokeMemberBinder" /> that represents the details of the dynamic operation.</param>
	/// <param name="args">An array of <see cref="T:System.Dynamic.DynamicMetaObject" /> instances - arguments to the invoke member operation.</param>
	public virtual DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
	{
		ContractUtils.RequiresNotNull(binder, "binder");
		return binder.FallbackInvokeMember(this, args);
	}

	/// <summary>Performs the binding of the dynamic invoke operation.</summary>
	/// <returns>The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="binder">An instance of the <see cref="T:System.Dynamic.InvokeBinder" /> that represents the details of the dynamic operation.</param>
	/// <param name="args">An array of <see cref="T:System.Dynamic.DynamicMetaObject" /> instances - arguments to the invoke operation.</param>
	public virtual DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
	{
		ContractUtils.RequiresNotNull(binder, "binder");
		return binder.FallbackInvoke(this, args);
	}

	/// <summary>Performs the binding of the dynamic create instance operation.</summary>
	/// <returns>The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="binder">An instance of the <see cref="T:System.Dynamic.CreateInstanceBinder" /> that represents the details of the dynamic operation.</param>
	/// <param name="args">An array of <see cref="T:System.Dynamic.DynamicMetaObject" /> instances - arguments to the create instance operation.</param>
	public virtual DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
	{
		ContractUtils.RequiresNotNull(binder, "binder");
		return binder.FallbackCreateInstance(this, args);
	}

	/// <summary>Performs the binding of the dynamic unary operation.</summary>
	/// <returns>The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="binder">An instance of the <see cref="T:System.Dynamic.UnaryOperationBinder" /> that represents the details of the dynamic operation.</param>
	public virtual DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
	{
		ContractUtils.RequiresNotNull(binder, "binder");
		return binder.FallbackUnaryOperation(this);
	}

	/// <summary>Performs the binding of the dynamic binary operation.</summary>
	/// <returns>The new <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the result of the binding.</returns>
	/// <param name="binder">An instance of the <see cref="T:System.Dynamic.BinaryOperationBinder" /> that represents the details of the dynamic operation.</param>
	/// <param name="arg">An instance of the <see cref="T:System.Dynamic.DynamicMetaObject" /> representing the right hand side of the binary operation.</param>
	public virtual DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
	{
		ContractUtils.RequiresNotNull(binder, "binder");
		return binder.FallbackBinaryOperation(this, arg);
	}

	/// <summary>Returns the enumeration of all dynamic member names.</summary>
	/// <returns>The list of dynamic member names.</returns>
	public virtual IEnumerable<string> GetDynamicMemberNames()
	{
		return Array.Empty<string>();
	}

	internal static Expression[] GetExpressions(DynamicMetaObject[] objects)
	{
		ContractUtils.RequiresNotNull(objects, "objects");
		Expression[] array = new Expression[objects.Length];
		for (int i = 0; i < objects.Length; i++)
		{
			DynamicMetaObject obj = objects[i];
			ContractUtils.RequiresNotNull(obj, "objects");
			Expression expression = obj.Expression;
			array[i] = expression;
		}
		return array;
	}

	/// <summary>Creates a meta-object for the specified object.</summary>
	/// <returns>If the given object implements <see cref="T:System.Dynamic.IDynamicMetaObjectProvider" /> and is not a remote object from outside the current AppDomain, returns the object's specific meta-object returned by <see cref="M:System.Dynamic.IDynamicMetaObjectProvider.GetMetaObject(System.Linq.Expressions.Expression)" />. Otherwise a plain new meta-object with no restrictions is created and returned.</returns>
	/// <param name="value">The object to get a meta-object for.</param>
	/// <param name="expression">The expression representing this <see cref="T:System.Dynamic.DynamicMetaObject" /> during the dynamic binding process.</param>
	public static DynamicMetaObject Create(object value, Expression expression)
	{
		ContractUtils.RequiresNotNull(expression, "expression");
		if (value is IDynamicMetaObjectProvider dynamicMetaObjectProvider)
		{
			DynamicMetaObject metaObject = dynamicMetaObjectProvider.GetMetaObject(expression);
			if (metaObject == null || !metaObject.HasValue || metaObject.Value == null || metaObject.Expression != expression)
			{
				throw Error.InvalidMetaObjectCreated(dynamicMetaObjectProvider.GetType());
			}
			return metaObject;
		}
		return new DynamicMetaObject(expression, BindingRestrictions.Empty, value);
	}
}
