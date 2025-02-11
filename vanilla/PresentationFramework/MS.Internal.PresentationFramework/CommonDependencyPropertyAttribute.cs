using System;
using System.Diagnostics;

namespace MS.Internal.PresentationFramework;

[AttributeUsage(AttributeTargets.Field)]
[Conditional("COMMONDPS")]
internal sealed class CommonDependencyPropertyAttribute : Attribute
{
}
