using System;
using System.Diagnostics;

namespace MS.Internal.PresentationCore;

[AttributeUsage(AttributeTargets.Field)]
[Conditional("COMMONDPS")]
internal sealed class CommonDependencyPropertyAttribute : Attribute
{
}
