using Verse;

namespace Quality_Recasting_Table;

public class Quality_Recasting_Table_Setting : ModSettings
{
	public bool enableEnvelope = true;

	public int recastprobabilitysetting = 1;

	public float fuelspent = 250f;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref enableEnvelope, "EnableEnvelope", defaultValue: true);
		Scribe_Values.Look(ref recastprobabilitysetting, "Recastprobabilitysetting", 1);
		Scribe_Values.Look(ref fuelspent, "Fuelspent", 250f);
	}

	public void InitData()
	{
		enableEnvelope = true;
		recastprobabilitysetting = 1;
		fuelspent = 250f;
	}
}
