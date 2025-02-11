using Verse;

namespace CombatExtended;

public class Command_SetLoadout : Command
{
	public ThingDef equipmentDef;

	public override bool GroupsWith(Gizmo other)
	{
		return equipmentDef == (other as Command_SetLoadout)?.equipmentDef && base.GroupsWith(other);
	}
}
