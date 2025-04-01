using System.Dynamic;
using System.Reflection;

namespace System.Runtime.CompilerServices;

/// <summary>Class that contains helper methods for DLR CallSites.</summary>
public static class CallSiteHelpers
{
	private static readonly Type s_knownNonDynamicMethodType = typeof(object).GetMethod("ToString").GetType();

	/// <summary>Checks if a <see cref="T:System.Reflection.MethodBase" /> is internally used by DLR and should not be displayed on the language code's stack.</summary>
	/// <returns>True if the input <see cref="T:System.Reflection.MethodBase" /> is internally used by DLR and should not be displayed on the language code's stack. Otherwise, false.</returns>
	/// <param name="mb">The input <see cref="T:System.Reflection.MethodBase" /></param>
	public static bool IsInternalFrame(MethodBase mb)
	{
		if (mb.Name == "CallSite.Target" && mb.GetType() != s_knownNonDynamicMethodType)
		{
			return true;
		}
		if (mb.DeclaringType == typeof(UpdateDelegates))
		{
			return true;
		}
		return false;
	}
}
