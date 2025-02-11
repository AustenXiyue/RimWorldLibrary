using System;
using System.Reflection;

namespace MonoMod.Core;

[CLSCompliant(true)]
internal readonly record struct CreateDetourRequest(MethodBase Source, MethodBase Target)
{
	public bool ApplyByDefault { get; set; } = true;
}
