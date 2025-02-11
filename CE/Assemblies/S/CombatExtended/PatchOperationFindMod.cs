using System.Linq;
using System.Xml;
using Verse;

namespace CombatExtended;

public class PatchOperationFindMod : PatchOperation
{
	public string modName;

	public override bool ApplyWorker(XmlDocument xml)
	{
		if (modName.NullOrEmpty())
		{
			return false;
		}
		return ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.Name == modName);
	}
}
