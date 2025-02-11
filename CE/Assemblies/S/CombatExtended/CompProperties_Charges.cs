using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public class CompProperties_Charges : CompProperties
{
	public List<int> chargeSpeeds = new List<int>();

	public CompProperties_Charges()
	{
		compClass = typeof(CompCharges);
	}
}
