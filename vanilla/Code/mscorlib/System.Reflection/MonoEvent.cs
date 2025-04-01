using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Reflection;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
internal sealed class MonoEvent : RuntimeEventInfo
{
	private IntPtr klass;

	private IntPtr handle;

	public override EventAttributes Attributes => MonoEventInfo.GetEventInfo(this).attrs;

	public override Type DeclaringType => MonoEventInfo.GetEventInfo(this).declaring_type;

	public override Type ReflectedType => MonoEventInfo.GetEventInfo(this).reflected_type;

	public override string Name => MonoEventInfo.GetEventInfo(this).name;

	public override MethodInfo GetAddMethod(bool nonPublic)
	{
		MonoEventInfo eventInfo = MonoEventInfo.GetEventInfo(this);
		if (nonPublic || (eventInfo.add_method != null && eventInfo.add_method.IsPublic))
		{
			return eventInfo.add_method;
		}
		return null;
	}

	public override MethodInfo GetRaiseMethod(bool nonPublic)
	{
		MonoEventInfo eventInfo = MonoEventInfo.GetEventInfo(this);
		if (nonPublic || (eventInfo.raise_method != null && eventInfo.raise_method.IsPublic))
		{
			return eventInfo.raise_method;
		}
		return null;
	}

	public override MethodInfo GetRemoveMethod(bool nonPublic)
	{
		MonoEventInfo eventInfo = MonoEventInfo.GetEventInfo(this);
		if (nonPublic || (eventInfo.remove_method != null && eventInfo.remove_method.IsPublic))
		{
			return eventInfo.remove_method;
		}
		return null;
	}

	public override MethodInfo[] GetOtherMethods(bool nonPublic)
	{
		MonoEventInfo eventInfo = MonoEventInfo.GetEventInfo(this);
		if (nonPublic)
		{
			return eventInfo.other_methods;
		}
		int num = 0;
		MethodInfo[] other_methods = eventInfo.other_methods;
		for (int i = 0; i < other_methods.Length; i++)
		{
			if (other_methods[i].IsPublic)
			{
				num++;
			}
		}
		if (num == eventInfo.other_methods.Length)
		{
			return eventInfo.other_methods;
		}
		MethodInfo[] array = new MethodInfo[num];
		num = 0;
		other_methods = eventInfo.other_methods;
		foreach (MethodInfo methodInfo in other_methods)
		{
			if (methodInfo.IsPublic)
			{
				array[num++] = methodInfo;
			}
		}
		return array;
	}

	public override string ToString()
	{
		return string.Concat(EventHandlerType, " ", Name);
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

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return CustomAttributeData.GetCustomAttributes(this);
	}
}
