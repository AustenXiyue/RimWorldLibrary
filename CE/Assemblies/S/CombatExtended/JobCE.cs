using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CombatExtended;

public class JobCE : Job
{
	public bool flagA;

	public bool flagB;

	public bool flagC;

	public int intA;

	public int intB;

	public int intC;

	private List<ThingDef> _targetThingDefs;

	public List<ThingDef> targetThingDefs
	{
		get
		{
			if (_targetThingDefs == null)
			{
				_targetThingDefs = new List<ThingDef>();
			}
			return _targetThingDefs;
		}
	}

	public void PostExposeData()
	{
		Scribe_Values.Look(ref flagA, "flagA", defaultValue: false);
		Scribe_Values.Look(ref flagB, "flagB", defaultValue: false);
		Scribe_Values.Look(ref flagC, "flagC", defaultValue: false);
		Scribe_Values.Look(ref intA, "intA", 0);
		Scribe_Values.Look(ref intB, "intB", 0);
		Scribe_Values.Look(ref intC, "intC", 0);
		Scribe_Collections.Look(ref _targetThingDefs, "TargetThingDef", LookMode.Def);
		if (_targetThingDefs == null)
		{
			_targetThingDefs = new List<ThingDef>();
		}
	}
}
