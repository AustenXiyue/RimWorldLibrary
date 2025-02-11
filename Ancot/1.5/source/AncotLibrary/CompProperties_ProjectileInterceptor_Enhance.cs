using Verse;

namespace AncotLibrary;

public class CompProperties_ProjectileInterceptor_Enhance : CompProperties
{
	public int hitPointBase = 100;

	public CompProperties_ProjectileInterceptor_Enhance()
	{
		compClass = typeof(CompProjectileInterceptor_Enhance);
	}
}
