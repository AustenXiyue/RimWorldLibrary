using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class Alert_Critical : Alert
{
	private int lastActiveFrame = -1;

	private const float PulseFreq = 0.5f;

	private const float PulseAmpCritical = 0.6f;

	protected override Color BGColor
	{
		get
		{
			float num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
			return new Color(num, num, num) * Color.red;
		}
	}

	protected virtual bool DoMessage => true;

	public override void AlertActiveUpdate()
	{
		if (DoMessage && lastActiveFrame < Time.frameCount - 1)
		{
			Messages.Message("MessageCriticalAlert".Translate(base.Label.CapitalizeFirst()), new LookTargets(GetReport().AllCulprits), MessageTypeDefOf.ThreatBig);
		}
		lastActiveFrame = Time.frameCount;
	}

	public Alert_Critical()
	{
		defaultPriority = AlertPriority.Critical;
	}
}
