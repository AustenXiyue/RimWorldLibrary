using System;
using Verse;

namespace CombatExtended.Lasers;

public class MoteLaserDectorationCE : MoteThrown
{
	public LaserBeamGraphicCE beam;

	public float baseSpeed;

	public float speedJitter;

	public float speedJitterOffset;

	public override float Alpha
	{
		get
		{
			base.Speed = (float)((double)baseSpeed + (double)speedJitter * Math.Sin(Math.PI * (double)((float)Find.TickManager.TicksGame * 18f + speedJitterOffset) / 180.0));
			if (beam != null)
			{
				return beam.Opacity;
			}
			return base.Alpha;
		}
	}
}
