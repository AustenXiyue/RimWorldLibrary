using System.Xml;
using Verse;

namespace CombatExtended;

public class CaliberFloatRange
{
	public FloatRange DamageRange;

	public FloatRange SpeedRange;

	public AmmoSetDef AmmoSet;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DamageRange = ParseHelper.FromString<FloatRange>(xmlRoot.ChildNodes[0].InnerText);
		SpeedRange = ParseHelper.FromString<FloatRange>(xmlRoot.ChildNodes[1].InnerText);
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "AmmoSet", xmlRoot.LastChild.InnerText);
	}
}
