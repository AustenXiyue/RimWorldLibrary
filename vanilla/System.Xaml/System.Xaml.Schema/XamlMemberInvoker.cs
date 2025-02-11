using System.Reflection;

namespace System.Xaml.Schema;

/// <summary>Provides an extension point that can access member characteristics of a XAML member through techniques other than reflection.</summary>
public class XamlMemberInvoker
{
	private class DirectiveMemberInvoker : XamlMemberInvoker
	{
		public override object GetValue(object instance)
		{
			throw new NotSupportedException(System.SR.NotSupportedOnDirective);
		}

		public override void SetValue(object instance, object value)
		{
			throw new NotSupportedException(System.SR.NotSupportedOnDirective);
		}
	}

	private static XamlMemberInvoker s_Directive;

	private static XamlMemberInvoker s_Unknown;

	private static object[] s_emptyObjectArray = Array.Empty<object>();

	private XamlMember _member;

	private NullableReference<MethodInfo> _shouldSerializeMethod;

	/// <summary>Provides a static value that represents an unknown, not fully implemented <see cref="T:System.Xaml.Schema.XamlMemberInvoker" />.</summary>
	/// <returns>A static value that represents an unknown, not fully implemented <see cref="T:System.Xaml.Schema.XamlMemberInvoker" />.</returns>
	public static XamlMemberInvoker UnknownInvoker
	{
		get
		{
			if (s_Unknown == null)
			{
				s_Unknown = new XamlMemberInvoker();
			}
			return s_Unknown;
		}
	}

	/// <summary>Gets the <see cref="T:System.Reflection.MethodInfo" /> for the CLR method that gets values for the property that is relevant for this <see cref="T:System.Xaml.Schema.XamlMemberInvoker" />.</summary>
	/// <returns>The <see cref="T:System.Reflection.MethodInfo" /> for the CLR method that gets values for the property that is relevant for this <see cref="T:System.Xaml.Schema.XamlMemberInvoker" />, or null.</returns>
	public MethodInfo UnderlyingGetter
	{
		get
		{
			if (!IsUnknown)
			{
				return _member.Getter;
			}
			return null;
		}
	}

	/// <summary>Gets the <see cref="T:System.Reflection.MethodInfo" /> for the CLR method that sets values for the property that is relevant for this <see cref="T:System.Xaml.Schema.XamlMemberInvoker" />.</summary>
	/// <returns>The <see cref="T:System.Reflection.MethodInfo" /> for the CLR method that sets values for the property that is relevant for this <see cref="T:System.Xaml.Schema.XamlMemberInvoker" />, or null.</returns>
	public MethodInfo UnderlyingSetter
	{
		get
		{
			if (!IsUnknown)
			{
				return _member.Setter;
			}
			return null;
		}
	}

	internal static XamlMemberInvoker DirectiveInvoker
	{
		get
		{
			if (s_Directive == null)
			{
				s_Directive = new DirectiveMemberInvoker();
			}
			return s_Directive;
		}
	}

	private bool IsUnknown
	{
		get
		{
			if (!(_member == null))
			{
				return _member.UnderlyingMember == null;
			}
			return true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> class. </summary>
	protected XamlMemberInvoker()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> class, based on a provided <see cref="T:System.Xaml.XamlMember" />.</summary>
	/// <param name="member">The <see cref="T:System.Xaml.XamlMember" /> value for the specific XAML member relevant to this <see cref="T:System.Xaml.Schema.XamlMemberInvoker" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="member" /> is null.</exception>
	public XamlMemberInvoker(XamlMember member)
	{
		_member = member ?? throw new ArgumentNullException("member");
	}

	/// <summary>Gets a value of the relevant property from an instance.</summary>
	/// <returns>The requested property value.</returns>
	/// <param name="instance">An instance of the owner type for the member.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">Invoked this method on a <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> that is based on an unknown <see cref="T:System.Xaml.XamlMember" />.-or-Invoked this method on a write-only member.-or-<see cref="P:System.Xaml.Schema.XamlMemberInvoker.UnderlyingGetter" /> is null.</exception>
	public virtual object GetValue(object instance)
	{
		ArgumentNullException.ThrowIfNull(instance, "instance");
		ThrowIfUnknown();
		if (UnderlyingGetter == null)
		{
			throw new NotSupportedException(System.SR.Format(System.SR.CantGetWriteonlyProperty, _member));
		}
		if (UnderlyingGetter.IsStatic)
		{
			return UnderlyingGetter.Invoke(null, new object[1] { instance });
		}
		return UnderlyingGetter.Invoke(instance, s_emptyObjectArray);
	}

	/// <summary>Sets a value of the relevant property on an instance.</summary>
	/// <param name="instance">An instance of the owner type for the member.</param>
	/// <param name="value">The property value to set.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">Invoked this method on a <see cref="T:System.Xaml.Schema.XamlMemberInvoker" /> that is based on an unknown <see cref="T:System.Xaml.XamlMember" />.-or-Invoked this method on a read-only member.-or-<see cref="P:System.Xaml.Schema.XamlMemberInvoker.UnderlyingSetter" /> is null.</exception>
	public virtual void SetValue(object instance, object value)
	{
		ArgumentNullException.ThrowIfNull(instance, "instance");
		ThrowIfUnknown();
		if (UnderlyingSetter == null)
		{
			throw new NotSupportedException(System.SR.Format(System.SR.CantSetReadonlyProperty, _member));
		}
		if (UnderlyingSetter.IsStatic)
		{
			UnderlyingSetter.Invoke(null, new object[2] { instance, value });
		}
		else
		{
			UnderlyingSetter.Invoke(instance, new object[1] { value });
		}
	}

	/// <summary>Indicates whether the value needs to be persisted by serialization processes.</summary>
	/// <returns>A value of the enumeration.</returns>
	/// <param name="instance">The instance with the property to be examined for persistence.</param>
	public virtual ShouldSerializeResult ShouldSerializeValue(object instance)
	{
		if (IsUnknown)
		{
			return ShouldSerializeResult.Default;
		}
		if (!_shouldSerializeMethod.IsSet)
		{
			Type declaringType = _member.UnderlyingMember.DeclaringType;
			string name = "ShouldSerialize" + _member.Name;
			BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			Type[] types;
			if (_member.IsAttachable)
			{
				types = new Type[1] { _member.TargetType.UnderlyingType ?? typeof(object) };
			}
			else
			{
				bindingFlags |= BindingFlags.Instance;
				types = Type.EmptyTypes;
			}
			_shouldSerializeMethod.Value = declaringType.GetMethod(name, bindingFlags, null, types, null);
		}
		MethodInfo value = _shouldSerializeMethod.Value;
		if (value != null)
		{
			if (!((!_member.IsAttachable) ? ((bool)value.Invoke(instance, null)) : ((bool)value.Invoke(null, new object[1] { instance }))))
			{
				return ShouldSerializeResult.False;
			}
			return ShouldSerializeResult.True;
		}
		return ShouldSerializeResult.Default;
	}

	private static bool IsSystemXamlNonPublic(ref ThreeValuedBool methodIsSystemXamlNonPublic, MethodInfo method)
	{
		if (methodIsSystemXamlNonPublic == ThreeValuedBool.NotSet)
		{
			bool flag = SafeReflectionInvoker.IsSystemXamlNonPublic(method);
			methodIsSystemXamlNonPublic = ((!flag) ? ThreeValuedBool.False : ThreeValuedBool.True);
		}
		return methodIsSystemXamlNonPublic == ThreeValuedBool.True;
	}

	private void ThrowIfUnknown()
	{
		if (IsUnknown)
		{
			throw new NotSupportedException(System.SR.NotSupportedOnUnknownMember);
		}
	}
}
