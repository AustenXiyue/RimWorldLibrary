using System.Collections.Generic;
using System.Xml;
using RimWorld;
using Verse;

namespace CombatExtended;

public class ApparelPartialStat
{
	public StatDef stat;

	public float mult;

	public List<BodyPartDef> parts;

	public float staticValue = 0f;

	public bool useStatic = false;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		int i = 1;
		if (xmlRoot.FirstChild.Name.Contains("use"))
		{
			useStatic = ParseHelper.FromString<bool>(xmlRoot.FirstChild.InnerText);
		}
		else
		{
			i = 0;
		}
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stat", xmlRoot.ChildNodes[i].Name);
		if (useStatic)
		{
			staticValue = ParseHelper.FromString<float>(xmlRoot.ChildNodes[i].InnerText);
		}
		else
		{
			mult = ParseHelper.FromString<float>(xmlRoot.ChildNodes[i].InnerText);
		}
		if (parts == null)
		{
			parts = new List<BodyPartDef>();
		}
		foreach (XmlNode childNode in xmlRoot.LastChild.ChildNodes)
		{
			DirectXmlCrossRefLoader.RegisterListWantsCrossRef(parts, childNode.InnerText);
		}
	}
}
