using System;
using System.ComponentModel;

namespace WinRT;

[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
internal sealed class WindowsRuntimeTypeAttribute : Attribute
{
}
