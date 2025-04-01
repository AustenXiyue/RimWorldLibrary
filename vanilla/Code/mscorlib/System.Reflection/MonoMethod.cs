using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Reflection;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
internal class MonoMethod : RuntimeMethodInfo
{
	internal IntPtr mhandle;

	private string name;

	private Type reftype;

	public override ParameterInfo ReturnParameter => MonoMethodInfo.GetReturnParameterInfo(this);

	public override Type ReturnType => MonoMethodInfo.GetReturnType(mhandle);

	public override ICustomAttributeProvider ReturnTypeCustomAttributes => MonoMethodInfo.GetReturnParameterInfo(this);

	public override RuntimeMethodHandle MethodHandle => new RuntimeMethodHandle(mhandle);

	public override MethodAttributes Attributes => MonoMethodInfo.GetAttributes(mhandle);

	public override CallingConventions CallingConvention => MonoMethodInfo.GetCallingConvention(mhandle);

	public override Type ReflectedType => reftype;

	public override Type DeclaringType => MonoMethodInfo.GetDeclaringType(mhandle);

	public override string Name
	{
		get
		{
			if (name != null)
			{
				return name;
			}
			return get_name(this);
		}
	}

	public override extern bool IsGenericMethodDefinition
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public override extern bool IsGenericMethod
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public override bool ContainsGenericParameters
	{
		get
		{
			if (IsGenericMethod)
			{
				Type[] genericArguments = GetGenericArguments();
				for (int i = 0; i < genericArguments.Length; i++)
				{
					if (genericArguments[i].ContainsGenericParameters)
					{
						return true;
					}
				}
			}
			return DeclaringType.ContainsGenericParameters;
		}
	}

	public override bool IsSecurityTransparent => get_core_clr_security_level() == 0;

	public override bool IsSecurityCritical => get_core_clr_security_level() > 0;

	public override bool IsSecuritySafeCritical => get_core_clr_security_level() == 1;

	internal MonoMethod()
	{
	}

	internal MonoMethod(RuntimeMethodHandle mhandle)
	{
		this.mhandle = mhandle.Value;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern string get_name(MethodBase method);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern MonoMethod get_base_method(MonoMethod method, bool definition);

	public override MethodInfo GetBaseDefinition()
	{
		return get_base_method(this, definition: true);
	}

	internal override MethodInfo GetBaseMethod()
	{
		return get_base_method(this, definition: false);
	}

	public override MethodImplAttributes GetMethodImplementationFlags()
	{
		return MonoMethodInfo.GetMethodImplementationFlags(mhandle);
	}

	public override ParameterInfo[] GetParameters()
	{
		ParameterInfo[] parametersInfo = MonoMethodInfo.GetParametersInfo(mhandle, this);
		if (parametersInfo.Length == 0)
		{
			return parametersInfo;
		}
		ParameterInfo[] array = new ParameterInfo[parametersInfo.Length];
		Array.FastCopy(parametersInfo, 0, array, 0, parametersInfo.Length);
		return array;
	}

	internal override ParameterInfo[] GetParametersInternal()
	{
		return MonoMethodInfo.GetParametersInfo(mhandle, this);
	}

	internal override int GetParametersCount()
	{
		return MonoMethodInfo.GetParametersInfo(mhandle, this).Length;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern object InternalInvoke(object obj, object[] parameters, out Exception exc);

	[DebuggerHidden]
	[DebuggerStepThrough]
	public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
	{
		if (binder == null)
		{
			binder = Type.DefaultBinder;
		}
		ParameterInfo[] parametersInternal = GetParametersInternal();
		ConvertValues(binder, parameters, parametersInternal, culture, invokeAttr);
		if (ContainsGenericParameters)
		{
			throw new InvalidOperationException("Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.");
		}
		object obj2 = null;
		Exception exc;
		try
		{
			obj2 = InternalInvoke(obj, parameters, out exc);
		}
		catch (ThreadAbortException)
		{
			throw;
		}
		catch (Exception inner)
		{
			throw new TargetInvocationException(inner);
		}
		if (exc != null)
		{
			throw exc;
		}
		return obj2;
	}

	internal static void ConvertValues(Binder binder, object[] args, ParameterInfo[] pinfo, CultureInfo culture, BindingFlags invokeAttr)
	{
		if (args == null)
		{
			if (pinfo.Length != 0)
			{
				throw new TargetParameterCountException();
			}
			return;
		}
		if (pinfo.Length != args.Length)
		{
			throw new TargetParameterCountException();
		}
		for (int i = 0; i < args.Length; i++)
		{
			object obj = args[i];
			ParameterInfo parameterInfo = pinfo[i];
			if (obj == Type.Missing)
			{
				if (parameterInfo.DefaultValue == DBNull.Value)
				{
					throw new ArgumentException(Environment.GetResourceString("Missing parameter does not have a default value."), "parameters");
				}
				args[i] = parameterInfo.DefaultValue;
			}
			else
			{
				RuntimeType runtimeType = (RuntimeType)parameterInfo.ParameterType;
				args[i] = runtimeType.CheckValue(obj, binder, culture, invokeAttr);
			}
		}
	}

	public override bool IsDefined(Type attributeType, bool inherit)
	{
		return MonoCustomAttrs.IsDefined(this, attributeType, inherit);
	}

	public override object[] GetCustomAttributes(bool inherit)
	{
		return MonoCustomAttrs.GetCustomAttributes(this, inherit);
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		return MonoCustomAttrs.GetCustomAttributes(this, attributeType, inherit);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern void GetPInvoke(out PInvokeAttributes flags, out string entryPoint, out string dllName);

	internal object[] GetPseudoCustomAttributes()
	{
		int num = 0;
		MonoMethodInfo methodInfo = MonoMethodInfo.GetMethodInfo(mhandle);
		if ((methodInfo.iattrs & MethodImplAttributes.PreserveSig) != 0)
		{
			num++;
		}
		if ((methodInfo.attrs & MethodAttributes.PinvokeImpl) != 0)
		{
			num++;
		}
		if (num == 0)
		{
			return null;
		}
		object[] array = new object[num];
		num = 0;
		if ((methodInfo.iattrs & MethodImplAttributes.PreserveSig) != 0)
		{
			array[num++] = new PreserveSigAttribute();
		}
		if ((methodInfo.attrs & MethodAttributes.PinvokeImpl) != 0)
		{
			array[num++] = DllImportAttribute.GetCustomAttribute(this);
		}
		return array;
	}

	public override MethodInfo MakeGenericMethod(params Type[] methodInstantiation)
	{
		if (methodInstantiation == null)
		{
			throw new ArgumentNullException("methodInstantiation");
		}
		if (!IsGenericMethodDefinition)
		{
			throw new InvalidOperationException("not a generic method definition");
		}
		if (GetGenericArguments().Length != methodInstantiation.Length)
		{
			throw new ArgumentException("Incorrect length");
		}
		bool flag = false;
		foreach (Type obj in methodInstantiation)
		{
			if (obj == null)
			{
				throw new ArgumentNullException();
			}
			if (!(obj is RuntimeType))
			{
				flag = true;
			}
		}
		if (flag)
		{
			return new MethodOnTypeBuilderInst(this, methodInstantiation);
		}
		MethodInfo methodInfo = MakeGenericMethod_impl(methodInstantiation);
		if (methodInfo == null)
		{
			throw new ArgumentException($"The method has {GetGenericArguments().Length} generic parameter(s) but {methodInstantiation.Length} generic argument(s) were provided.");
		}
		return methodInfo;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern MethodInfo MakeGenericMethod_impl(Type[] types);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public override extern Type[] GetGenericArguments();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern MethodInfo GetGenericMethodDefinition_impl();

	public override MethodInfo GetGenericMethodDefinition()
	{
		MethodInfo genericMethodDefinition_impl = GetGenericMethodDefinition_impl();
		if (genericMethodDefinition_impl == null)
		{
			throw new InvalidOperationException();
		}
		return genericMethodDefinition_impl;
	}

	public override MethodBody GetMethodBody()
	{
		return MethodBase.GetMethodBody(mhandle);
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return CustomAttributeData.GetCustomAttributes(this);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int get_core_clr_security_level();
}
