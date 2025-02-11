using System;
using Verse;

namespace CombatExtended.Compatibility;

public class Multiplayer : IPatch
{
	[AttributeUsage(AttributeTargets.Method)]
	public class SyncMethodAttribute : Attribute
	{
		public int syncContext = -1;

		public int[] exposeParameters = null;
	}

	private static bool isMultiplayerActive;

	private static Func<bool> _inMultiplayer;

	private static Func<bool> _isExecutingCommands;

	private static Func<bool> _isExecutingCommandsIssuedBySelf;

	public static bool InMultiplayer
	{
		get
		{
			if (isMultiplayerActive)
			{
				return _inMultiplayer();
			}
			return false;
		}
	}

	public static bool IsExecutingCommands
	{
		get
		{
			if (isMultiplayerActive)
			{
				return _isExecutingCommands();
			}
			return false;
		}
	}

	public static bool IsExecutingCommandsIssuedBySelf
	{
		get
		{
			if (isMultiplayerActive)
			{
				return _isExecutingCommandsIssuedBySelf();
			}
			return false;
		}
	}

	public bool CanInstall()
	{
		Log.Message("Checking Multiplayer Compat");
		return ModLister.HasActiveModWithName("Multiplayer");
	}

	public void Install()
	{
		Log.Message("CombatExtended :: Installing Multiplayer Compat");
		isMultiplayerActive = true;
	}

	public static void registerCallbacks(Func<bool> inMP, Func<bool> iec, Func<bool> iecibs)
	{
		_inMultiplayer = inMP;
		_isExecutingCommands = iec;
		_isExecutingCommandsIssuedBySelf = iecibs;
	}
}
