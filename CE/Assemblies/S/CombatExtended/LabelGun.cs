using System.Collections.Generic;
using System.Xml;
using RimWorld;
using Verse;

namespace CombatExtended;

public class LabelGun
{
	public List<string> names;

	public int magCap;

	public float reloadTime;

	public float mass;

	public float bulk;

	public AmmoSetDef caliber;

	public List<StatModifier> stats;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		foreach (XmlNode childNode in xmlRoot.ChildNodes)
		{
			switch (childNode.Name)
			{
			case "magCap":
				magCap = ParseHelper.FromString<int>(childNode.InnerText);
				break;
			case "reloadTime":
				reloadTime = ParseHelper.FromString<float>(childNode.InnerText);
				break;
			case "mass":
				mass = ParseHelper.FromString<float>(childNode.InnerText);
				break;
			case "bulk":
				bulk = ParseHelper.FromString<float>(childNode.InnerText);
				break;
			case "caliber":
				DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "caliber", childNode.InnerText);
				break;
			case "names":
				if (names == null)
				{
					names = new List<string>();
				}
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					names.Add(childNode2.InnerText);
				}
				break;
			case "stats":
				if (stats == null)
				{
					stats = new List<StatModifier>();
				}
				foreach (XmlNode childNode3 in childNode.ChildNodes)
				{
					StatModifier statModifier = new StatModifier();
					Log.Message(childNode3.ToString());
					statModifier.LoadDataFromXmlCustom(childNode3);
					stats.Add(statModifier);
				}
				break;
			}
		}
	}
}
