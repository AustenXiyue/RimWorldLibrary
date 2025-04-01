using System.Collections;
using System.Reflection;

namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides the information necessary to create an instance of an object. This class cannot be inherited.</summary>
public sealed class InstanceDescriptor
{
	private MemberInfo member;

	private ICollection arguments;

	private bool isComplete;

	/// <summary>Gets the collection of arguments that can be used to reconstruct an instance of the object that this instance descriptor represents.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> of arguments that can be used to create the object.</returns>
	public ICollection Arguments => arguments;

	/// <summary>Gets a value indicating whether the contents of this <see cref="T:System.ComponentModel.Design.Serialization.InstanceDescriptor" /> completely identify the instance.</summary>
	/// <returns>true if the instance is completely described; otherwise, false.</returns>
	public bool IsComplete => isComplete;

	/// <summary>Gets the member information that describes the instance this descriptor is associated with.</summary>
	/// <returns>A <see cref="T:System.Reflection.MemberInfo" /> that describes the instance that this object is associated with.</returns>
	public MemberInfo MemberInfo => member;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.InstanceDescriptor" /> class using the specified member information and arguments.</summary>
	/// <param name="member">The member information for the descriptor. This can be a <see cref="T:System.Reflection.MethodInfo" />, <see cref="T:System.Reflection.ConstructorInfo" />, <see cref="T:System.Reflection.FieldInfo" />, or <see cref="T:System.Reflection.PropertyInfo" />. If this is a <see cref="T:System.Reflection.MethodInfo" />, <see cref="T:System.Reflection.FieldInfo" />, or <see cref="T:System.Reflection.PropertyInfo" />, it must represent a static member. </param>
	/// <param name="arguments">The collection of arguments to pass to the member. This parameter can be null or an empty collection if there are no arguments. The collection can also consist of other instances of <see cref="T:System.ComponentModel.Design.Serialization.InstanceDescriptor" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="member" /> is of type <see cref="T:System.Reflection.MethodInfo" />, <see cref="T:System.Reflection.FieldInfo" />, or <see cref="T:System.Reflection.PropertyInfo" />, and it does not represent a static member.<paramref name="member" /> is of type <see cref="T:System.Reflection.PropertyInfo" /> and is not readable.<paramref name="member" /> is of type <see cref="T:System.Reflection.MethodInfo" /> or <see cref="T:System.Reflection.ConstructorInfo" />, and the number of arguments in <paramref name="arguments" /> does not match the signature of <paramref name="member." /><paramref name="member" /> is of type <see cref="T:System.Reflection.ConstructorInfo" /> and represents a static member.<paramref name="member" /> is of type <see cref="T:System.Reflection.FieldInfo" />, and the number of arguments in <paramref name="arguments" /> is not zero. </exception>
	public InstanceDescriptor(MemberInfo member, ICollection arguments)
		: this(member, arguments, isComplete: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.InstanceDescriptor" /> class using the specified member information, arguments, and value indicating whether the specified information completely describes the instance.</summary>
	/// <param name="member">The member information for the descriptor. This can be a <see cref="T:System.Reflection.MethodInfo" />, <see cref="T:System.Reflection.ConstructorInfo" />, <see cref="T:System.Reflection.FieldInfo" />, or <see cref="T:System.Reflection.PropertyInfo" />. If this is a <see cref="T:System.Reflection.MethodInfo" />, <see cref="T:System.Reflection.FieldInfo" />, or <see cref="T:System.Reflection.PropertyInfo" />, it must represent a static member. </param>
	/// <param name="arguments">The collection of arguments to pass to the member. This parameter can be null or an empty collection if there are no arguments. The collection can also consist of other instances of <see cref="T:System.ComponentModel.Design.Serialization.InstanceDescriptor" />. </param>
	/// <param name="isComplete">true if the specified information completely describes the instance; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="member" /> is of type <see cref="T:System.Reflection.MethodInfo" />, <see cref="T:System.Reflection.FieldInfo" />, or <see cref="T:System.Reflection.PropertyInfo" />, and it does not represent a static member<paramref name="member" /> is of type <see cref="T:System.Reflection.PropertyInfo" /> and is not readable.<paramref name="member" /> is of type <see cref="T:System.Reflection.MethodInfo" /> or <see cref="T:System.Reflection.ConstructorInfo" /> and the number of arguments in <paramref name="arguments" /> does not match the signature of <paramref name="member" />.<paramref name="member" /> is of type <see cref="T:System.Reflection.ConstructorInfo" /> and represents a static member<paramref name="member" /> is of type <see cref="T:System.Reflection.FieldInfo" />, and the number of arguments in <paramref name="arguments" /> is not zero.</exception>
	public InstanceDescriptor(MemberInfo member, ICollection arguments, bool isComplete)
	{
		this.member = member;
		this.isComplete = isComplete;
		if (arguments == null)
		{
			this.arguments = new object[0];
		}
		else
		{
			object[] array = new object[arguments.Count];
			arguments.CopyTo(array, 0);
			this.arguments = array;
		}
		if (member is FieldInfo)
		{
			if (!((FieldInfo)member).IsStatic)
			{
				throw new ArgumentException(global::SR.GetString("Parameter must be static."));
			}
			if (this.arguments.Count != 0)
			{
				throw new ArgumentException(global::SR.GetString("Length mismatch."));
			}
		}
		else if (member is ConstructorInfo)
		{
			ConstructorInfo constructorInfo = (ConstructorInfo)member;
			if (constructorInfo.IsStatic)
			{
				throw new ArgumentException(global::SR.GetString("Parameter cannot be static."));
			}
			if (this.arguments.Count != constructorInfo.GetParameters().Length)
			{
				throw new ArgumentException(global::SR.GetString("Length mismatch."));
			}
		}
		else if (member is MethodInfo)
		{
			MethodInfo methodInfo = (MethodInfo)member;
			if (!methodInfo.IsStatic)
			{
				throw new ArgumentException(global::SR.GetString("Parameter must be static."));
			}
			if (this.arguments.Count != methodInfo.GetParameters().Length)
			{
				throw new ArgumentException(global::SR.GetString("Length mismatch."));
			}
		}
		else if (member is PropertyInfo)
		{
			PropertyInfo obj = (PropertyInfo)member;
			if (!obj.CanRead)
			{
				throw new ArgumentException(global::SR.GetString("Parameter must be readable."));
			}
			MethodInfo getMethod = obj.GetGetMethod();
			if (getMethod != null && !getMethod.IsStatic)
			{
				throw new ArgumentException(global::SR.GetString("Parameter must be static."));
			}
		}
	}

	/// <summary>Invokes this instance descriptor and returns the object the descriptor describes.</summary>
	/// <returns>The object this instance descriptor describes.</returns>
	public object Invoke()
	{
		object[] array = new object[arguments.Count];
		arguments.CopyTo(array, 0);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is InstanceDescriptor)
			{
				array[i] = ((InstanceDescriptor)array[i]).Invoke();
			}
		}
		if (member is ConstructorInfo)
		{
			return ((ConstructorInfo)member).Invoke(array);
		}
		if (member is MethodInfo)
		{
			return ((MethodInfo)member).Invoke(null, array);
		}
		if (member is PropertyInfo)
		{
			return ((PropertyInfo)member).GetValue(null, array);
		}
		if (member is FieldInfo)
		{
			return ((FieldInfo)member).GetValue(null);
		}
		return null;
	}
}
