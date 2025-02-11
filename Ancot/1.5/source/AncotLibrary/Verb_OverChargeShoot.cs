using Verse;

namespace AncotLibrary;

public class Verb_OverChargeShoot : Verb_Shoot
{
	protected override bool TryCastShot()
	{
		base.TryCastShot();
		if (base.EquipmentSource != null)
		{
			CompOverChargeShot comp = base.EquipmentSource.GetComp<CompOverChargeShot>();
		}
		return true;
	}
}
