using System.Xml;
using Verse;

namespace CombatExtended;

public class PatchOperationMakeGunCECompatible : PatchOperation
{
	public string defName;

	public string texPath;

	public bool isWeaponPlatform = false;

	public bool AllowWithRunAndGun = true;

	public XmlContainer statBases;

	public XmlContainer Properties;

	public XmlContainer AmmoUser;

	public XmlContainer FireModes;

	public XmlContainer weaponTags;

	public XmlContainer weaponClasses;

	public XmlContainer costList;

	public XmlContainer researchPrerequisite;

	public XmlContainer attachmentLinks;

	public XmlContainer defaultGraphicParts;

	public override bool ApplyWorker(XmlDocument xml)
	{
		bool result = false;
		if (defName.NullOrEmpty())
		{
			return false;
		}
		foreach (object item in xml.SelectNodes("Defs/ThingDef[defName=\"" + defName + "\"]"))
		{
			result = true;
			XmlNode xmlNode = item as XmlNode;
			if (!isWeaponPlatform)
			{
				XmlContainer xmlContainer = attachmentLinks;
				if (xmlContainer == null || !xmlContainer.node.HasChildNodes)
				{
					XmlContainer xmlContainer2 = defaultGraphicParts;
					if (xmlContainer2 == null || !xmlContainer2.node.HasChildNodes)
					{
						goto IL_00a2;
					}
				}
			}
			MakeWeaponPlatform(xml, xmlNode);
			goto IL_00a2;
			IL_00a2:
			if (texPath != null)
			{
				AddOrReplaceTexPath(xml, xmlNode);
			}
			XmlContainer xmlContainer3 = attachmentLinks;
			if (xmlContainer3 != null && xmlContainer3.node.HasChildNodes)
			{
				AddOrReplaceAttachmentLinks(xml, xmlNode);
			}
			XmlContainer xmlContainer4 = defaultGraphicParts;
			if (xmlContainer4 != null && xmlContainer4.node.HasChildNodes)
			{
				AddOrReplaceDefaultGraphicParts(xml, xmlNode);
			}
			XmlContainer xmlContainer5 = statBases;
			if (xmlContainer5 != null && xmlContainer5.node.HasChildNodes)
			{
				AddOrReplaceStatBases(xml, xmlNode);
			}
			XmlContainer xmlContainer6 = costList;
			if (xmlContainer6 != null && xmlContainer6.node.HasChildNodes)
			{
				AddOrReplaceCostList(xml, xmlNode);
			}
			if (Properties != null && Properties.node.HasChildNodes)
			{
				AddOrReplaceVerbPropertiesCE(xml, xmlNode);
			}
			if (AmmoUser != null || FireModes != null)
			{
				AddOrReplaceCompsCE(xml, xmlNode);
			}
			if (weaponClasses != null && weaponClasses.node.HasChildNodes)
			{
				AddOrReplaceWeaponClasses(xml, xmlNode);
			}
			if (weaponTags != null && weaponTags.node.HasChildNodes)
			{
				AddOrReplaceWeaponTags(xml, xmlNode);
			}
			if (researchPrerequisite != null)
			{
				AddOrReplaceResearchPrereq(xml, xmlNode);
			}
			if (ModLister.HasActiveModWithName("RunAndGun") && !AllowWithRunAndGun)
			{
				AddRunAndGunExtension(xml, xmlNode);
			}
		}
		return result;
	}

	private void MakeWeaponPlatform(XmlDocument xml, XmlNode xmlNode)
	{
		XmlElement xmlElement = xmlNode as XmlElement;
		if (!xmlElement.Name.Contains("WeaponPlatformDef"))
		{
			xmlElement.SetAttribute("Class", "CombatExtended.WeaponPlatformDef");
			GetOrCreateNode(xml, xmlNode, "thingClass", out var output);
			if (!output.InnerText.StartsWith("CombatExtended"))
			{
				output.InnerText = "CombatExtended.WeaponPlatform";
			}
			GetOrCreateNode(xml, xmlNode, "drawerType", out var output2);
			output2.InnerText = "RealtimeOnly";
		}
	}

	private void AddOrReplaceAttachmentLinks(XmlDocument xml, XmlNode xmlNode)
	{
		GetOrCreateNode(xml, xmlNode, "attachmentLinks", out var output);
		Populate(xml, attachmentLinks.node, ref output);
	}

	private void AddOrReplaceDefaultGraphicParts(XmlDocument xml, XmlNode xmlNode)
	{
		GetOrCreateNode(xml, xmlNode, "defaultGraphicParts", out var output);
		Populate(xml, defaultGraphicParts.node, ref output);
	}

	private void AddOrReplaceTexPath(XmlDocument xml, XmlNode xmlNode)
	{
		GetOrCreateNode(xml, xmlNode, "graphicData", out var output);
		GetOrCreateNode(xml, output, "texPath", out var output2);
		output2.InnerText = texPath;
		GetOrCreateNode(xml, output, "graphicClass", out var output3);
		output3.InnerText = "Graphic_Single";
	}

	private bool GetOrCreateNode(XmlDocument xml, XmlNode xmlNode, string name, out XmlElement output)
	{
		XmlNodeList xmlNodeList = xmlNode.SelectNodes(name);
		if (xmlNodeList.Count == 0)
		{
			output = xml.CreateElement(name);
			xmlNode.AppendChild(output);
			return false;
		}
		output = xmlNodeList[0] as XmlElement;
		return true;
	}

	private XmlElement CreateListElementAndPopulate(XmlDocument xml, XmlNode reference, string type = null)
	{
		XmlElement destination = xml.CreateElement("li");
		if (type != null)
		{
			destination.SetAttribute("Class", type);
		}
		Populate(xml, reference, ref destination);
		return destination;
	}

	private void Populate(XmlDocument xml, XmlNode reference, ref XmlElement destination, bool overrideExisting = false)
	{
		foreach (XmlNode item in reference)
		{
			if (overrideExisting)
			{
				XmlNodeList xmlNodeList = destination.SelectNodes(item.Name);
				if (xmlNodeList != null)
				{
					foreach (XmlNode item2 in xmlNodeList)
					{
						destination.RemoveChild(item2);
					}
				}
			}
			destination.AppendChild(xml.ImportNode(item, deep: true));
		}
	}

	private void AddOrReplaceVerbPropertiesCE(XmlDocument xml, XmlNode xmlNode)
	{
		if (GetOrCreateNode(xml, xmlNode, "verbs", out var output))
		{
			XmlNodeList xmlNodeList = output.SelectNodes("li[verbClass=\"Verb_Shoot\" or verbClass=\"Verb_ShootOneUse\" or verbClass=\"Verb_LaunchProjectile\"]");
			foreach (object item in xmlNodeList)
			{
				if (item is XmlNode oldChild)
				{
					output.RemoveChild(oldChild);
				}
			}
		}
		output.AppendChild(CreateListElementAndPopulate(xml, Properties.node, "CombatExtended.VerbPropertiesCE"));
	}

	private void AddOrReplaceCompsCE(XmlDocument xml, XmlNode xmlNode)
	{
		GetOrCreateNode(xml, xmlNode, "comps", out var output);
		if (AmmoUser != null)
		{
			output.AppendChild(CreateListElementAndPopulate(xml, AmmoUser.node, "CombatExtended.CompProperties_AmmoUser"));
		}
		if (FireModes != null)
		{
			output.AppendChild(CreateListElementAndPopulate(xml, FireModes.node, "CombatExtended.CompProperties_FireModes"));
		}
	}

	private void AddOrReplaceWeaponClasses(XmlDocument xml, XmlNode xmlNode)
	{
		GetOrCreateNode(xml, xmlNode, "weaponClasses", out var output);
		Populate(xml, weaponClasses.node, ref output);
	}

	private void AddOrReplaceWeaponTags(XmlDocument xml, XmlNode xmlNode)
	{
		GetOrCreateNode(xml, xmlNode, "weaponTags", out var output);
		Populate(xml, weaponTags.node, ref output);
	}

	private void AddOrReplaceStatBases(XmlDocument xml, XmlNode xmlNode)
	{
		GetOrCreateNode(xml, xmlNode, "statBases", out var output);
		if (output.HasChildNodes)
		{
			XmlNodeList xmlNodeList = output.SelectNodes("AccuracyTouch | AccuracyShort | AccuracyMedium | AccuracyLong");
			foreach (XmlNode item in xmlNodeList)
			{
				output.RemoveChild(item);
			}
		}
		Populate(xml, statBases.node, ref output, overrideExisting: true);
	}

	private void AddOrReplaceCostList(XmlDocument xml, XmlNode xmlNode)
	{
		GetOrCreateNode(xml, xmlNode, "costList", out var output);
		if (output.HasChildNodes)
		{
			output.RemoveAll();
		}
		Populate(xml, costList.node, ref output);
	}

	private void AddOrReplaceResearchPrereq(XmlDocument xml, XmlNode xmlNode)
	{
		GetOrCreateNode(xml, xmlNode, "recipeMaker", out var output);
		XmlNode xmlNode2 = output.SelectSingleNode(researchPrerequisite.node.Name);
		if (xmlNode2 != null)
		{
			output.ReplaceChild(xml.ImportNode(researchPrerequisite.node, deep: true), xmlNode2);
		}
		else
		{
			output.AppendChild(xml.ImportNode(researchPrerequisite.node, deep: true));
		}
	}

	private void AddRunAndGunExtension(XmlDocument xml, XmlNode xmlNode)
	{
		GetOrCreateNode(xml, xmlNode, "modExtensions", out var output);
		XmlElement xmlElement = xml.CreateElement("li");
		xmlElement.SetAttribute("Class", "RunAndGun.DefModExtension_SettingDefaults");
		output.AppendChild(xmlElement);
		XmlElement xmlElement2 = xml.CreateElement("weaponForbidden");
		xmlElement2.InnerText = "true";
		xmlElement.AppendChild(xmlElement2);
	}
}
