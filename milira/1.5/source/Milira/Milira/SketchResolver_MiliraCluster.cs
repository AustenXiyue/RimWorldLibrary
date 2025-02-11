using RimWorld.SketchGen;

namespace Milira;

public class SketchResolver_MiliraCluster : SketchResolver
{
	protected override void ResolveInt(ResolveParams parms)
	{
		MiliraClusterGenerator.ResolveSketch(parms);
	}

	protected override bool CanResolveInt(ResolveParams parms)
	{
		return true;
	}
}
