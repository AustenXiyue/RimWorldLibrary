using System.Collections.Generic;
using System.Xml;
using Verse;

namespace CombatExtended;

public class ERAComponent : IExposable
{
	public BodyPartDef part;

	public float armor;

	public float damageTreshold;

	public float APTreshold;

	public bool triggered = false;

	public CompProperties_Fragments frags;

	public List<DamageDef> ignoredDmgDefs;

	public CompFragments fragComp => new CompFragments
	{
		props = frags
	};

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		foreach (XmlNode childNode in xmlRoot.ChildNodes)
		{
			switch (childNode.Name.ToLower())
			{
			case "armor":
				armor = ParseHelper.ParseFloat(childNode.InnerText);
				break;
			case "damagetreshold":
				damageTreshold = ParseHelper.ParseFloat(childNode.InnerText);
				break;
			case "aptreshold":
				damageTreshold = ParseHelper.ParseFloat(childNode.InnerText);
				break;
			case "part":
				DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "part", childNode.InnerText);
				break;
			case "triggered":
				triggered = ParseHelper.ParseBool(childNode.InnerText);
				break;
			case "frags":
				frags = new CompProperties_Fragments
				{
					fragments = new List<ThingDefCountClass>()
				};
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name == "fragments")
					{
						foreach (XmlNode childNode3 in childNode2.ChildNodes)
						{
							ThingDefCountClass thingDefCountClass = new ThingDefCountClass();
							thingDefCountClass.LoadDataFromXmlCustom(childNode3);
							frags.fragments.Add(thingDefCountClass);
						}
					}
					if (childNode2.Name == "fragSpeedFactor")
					{
						frags.fragSpeedFactor = ParseHelper.ParseFloat(childNode2.InnerText);
					}
				}
				break;
			case "ignoreddmgdefs":
				ignoredDmgDefs = new List<DamageDef>();
				foreach (XmlNode childNode4 in childNode.ChildNodes)
				{
					DirectXmlCrossRefLoader.RegisterListWantsCrossRef(ignoredDmgDefs, childNode4.InnerText);
				}
				break;
			}
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref triggered, "triggered", defaultValue: false);
	}
}
