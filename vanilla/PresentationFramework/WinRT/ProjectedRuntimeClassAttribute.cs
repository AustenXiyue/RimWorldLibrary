using System;
using System.ComponentModel;

namespace WinRT;

[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class ProjectedRuntimeClassAttribute : Attribute
{
	public string DefaultInterfaceProperty { get; }

	public ProjectedRuntimeClassAttribute(string defaultInterfaceProp)
	{
		DefaultInterfaceProperty = defaultInterfaceProp;
	}
}
