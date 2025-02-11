using System.Xml;
using Verse;

namespace CombatExtended;

public class AdditionalWeapon
{
	public ThingDef projectile;

	public float chanceToUse;

	public int burstCount;

	public int uses;

	public int shotTime;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		foreach (XmlNode childNode in xmlRoot.ChildNodes)
		{
			if (childNode.Name.ToLower() == "uses")
			{
				uses = ParseHelper.ParseIntPermissive(childNode.InnerText);
			}
			if (childNode.Name.ToLower() == "burstcount")
			{
				burstCount = ParseHelper.ParseIntPermissive(childNode.InnerText);
			}
			if (childNode.Name.ToLower() == "chancetouse")
			{
				chanceToUse = ParseHelper.ParseFloat(childNode.InnerText);
			}
			if (childNode.Name.ToLower() == "shottime")
			{
				shotTime = ParseHelper.ParseIntPermissive(childNode.InnerText);
			}
			if (childNode.Name.ToLower() == "projectile")
			{
				DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "projectile", childNode.InnerText);
			}
		}
	}
}
