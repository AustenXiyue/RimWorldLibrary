using System;

namespace CombatExtended;

[Serializable]
public class LoadoutConfig
{
	public string label;

	public LoadoutSlotConfig[] slots;
}
