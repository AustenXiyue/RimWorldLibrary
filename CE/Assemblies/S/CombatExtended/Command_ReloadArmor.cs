using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class Command_ReloadArmor : Command_Action
{
	public CompApparelReloadable compReloadable;

	public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
	{
		get
		{
			yield break;
		}
	}

	public override bool GroupsWith(Gizmo other)
	{
		Command_ReloadArmor command_ReloadArmor = other as Command_ReloadArmor;
		return command_ReloadArmor != null;
	}

	public override void ProcessInput(Event ev)
	{
		if (compReloadable == null)
		{
			Log.Error("Command_ReloadArmor without reloadable comp");
		}
		else if (compReloadable.RemainingCharges < compReloadable.MaxCharges)
		{
			base.ProcessInput(ev);
		}
	}
}
