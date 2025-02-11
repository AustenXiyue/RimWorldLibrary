using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public class CompProperties_ProjectileFleck : CompProperties
{
	public List<ProjectileFleckDataCE> FleckDatas;

	public CompProperties_ProjectileFleck()
	{
		compClass = typeof(CompProjectileFleck);
	}
}
