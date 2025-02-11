using System;
using CombatExtended.WorldObjects;
using Verse;

namespace CombatExtended;

public class GenStep_Attrition : GenStep
{
	private Map map;

	private HealthComp healthComp;

	public override int SeedPart => 1158116095;

	public override void Generate(Map map, GenStepParams parms)
	{
		this.map = map;
		WorldObjectDamageWorker.BeginAttrition(map);
		healthComp = this.map.Parent?.GetComponent<HealthComp>() ?? null;
		if (healthComp != null && healthComp.Health < 0.98f)
		{
			try
			{
				foreach (HealthComp.WorldDamageInfo recentShell in healthComp.recentShells)
				{
					recentShell.ShellDef.GetWorldObjectDamageWorker().ProcessShell(recentShell.ShellDef);
				}
			}
			catch (Exception arg)
			{
				Log.Error($"CE: GenStep_Attrition Failed with error {arg}");
			}
		}
		this.map = null;
		WorldObjectDamageWorker.EndAttrition();
		healthComp = null;
	}
}
