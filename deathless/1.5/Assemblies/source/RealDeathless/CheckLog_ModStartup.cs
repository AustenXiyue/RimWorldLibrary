using System;
using Verse;

namespace RealDeathless;

[StaticConstructorOnStartup]
public static class CheckLog_ModStartup
{
	static CheckLog_ModStartup()
	{
		Log.Message(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - RealDeathless: Load Successfully.");
	}
}
