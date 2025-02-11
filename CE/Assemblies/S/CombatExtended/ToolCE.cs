using Verse;

namespace CombatExtended;

public class ToolCE : Tool
{
	public float armorPenetrationSharp;

	public float armorPenetrationBlunt;

	public Gender restrictedGender = Gender.None;

	public AttachmentDef requiredAttachment = null;
}
