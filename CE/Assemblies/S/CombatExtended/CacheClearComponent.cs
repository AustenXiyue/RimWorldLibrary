using System;
using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public class CacheClearComponent : GameComponent
{
	private static HashSet<Action> clearCacheActions = new HashSet<Action>();

	public CacheClearComponent(Game _)
	{
	}

	public override void FinalizeInit()
	{
		foreach (Action clearCacheAction in clearCacheActions)
		{
			clearCacheAction();
		}
	}

	public static void AddClearCacheAction(Action action)
	{
		clearCacheActions.Add(action);
	}
}
