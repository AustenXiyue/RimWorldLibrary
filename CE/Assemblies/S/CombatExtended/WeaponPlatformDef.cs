using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class WeaponPlatformDef : ThingDef
{
	public class WeaponGraphicPart
	{
		public GraphicData partGraphicData;

		public GraphicData outlineGraphicData;

		public List<string> slotTags;

		private Texture2D _UIPartTex = null;

		private Texture2D _UIOutlineTex = null;

		public bool HasOutline => outlineGraphicData != null;

		public bool HasPartMat => partGraphicData != null;

		public Texture2D UIPartTex
		{
			get
			{
				if (_UIPartTex == null && HasPartMat)
				{
					_UIPartTex = (Texture2D)PartMat.mainTexture;
				}
				return _UIPartTex;
			}
		}

		public Texture2D UIOutlineTex
		{
			get
			{
				if (_UIOutlineTex == null && HasOutline)
				{
					_UIOutlineTex = (Texture2D)OutlineMat.mainTexture;
				}
				return _UIOutlineTex;
			}
		}

		public Material PartMat => partGraphicData.Graphic.MatSingle;

		public Material OutlineMat => outlineGraphicData.Graphic.MatSingle;
	}

	public List<AttachmentLink> attachmentLinks;

	public List<WeaponGraphicPart> defaultGraphicParts = new List<WeaponGraphicPart>();

	private Texture2D _UIWeaponTex = null;

	private Dictionary<Pair<AttachmentDef, AttachmentDef>, bool> _compatibilite = new Dictionary<Pair<AttachmentDef, AttachmentDef>, bool>();

	private Dictionary<Pair<AttachmentDef, WeaponGraphicPart>, bool> _removes = new Dictionary<Pair<AttachmentDef, WeaponGraphicPart>, bool>();

	public Texture2D UIWeaponTex
	{
		get
		{
			if (_UIWeaponTex == null)
			{
				_UIWeaponTex = (Texture2D)graphic.MatSingle.mainTexture;
			}
			return _UIWeaponTex;
		}
	}

	public override void PostLoad()
	{
		if (defaultGraphicParts == null)
		{
			defaultGraphicParts = new List<WeaponGraphicPart>();
		}
		if (attachmentLinks == null)
		{
			attachmentLinks = new List<AttachmentLink>();
		}
		base.PostLoad();
		if (inspectorTabs == null)
		{
			inspectorTabs = new List<Type>();
		}
		inspectorTabs.Add(typeof(ITab_AttachmentView));
		if (inspectorTabsResolved == null)
		{
			inspectorTabsResolved = new List<InspectTabBase>();
		}
		inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_AttachmentView)));
	}

	public bool AttachmentsCompatible(AttachmentDef first, AttachmentDef second)
	{
		if (first.index > second.index)
		{
			return AttachmentsCompatible(second, first);
		}
		Pair<AttachmentDef, AttachmentDef> key = new Pair<AttachmentDef, AttachmentDef>(first, second);
		if (_compatibilite.TryGetValue(key, out var value))
		{
			return value;
		}
		return _compatibilite[key] = first.slotTags.All((string s) => !second.slotTags.Contains(s));
	}

	public bool AttachmentRemoves(AttachmentDef attachment, WeaponGraphicPart part)
	{
		Pair<AttachmentDef, WeaponGraphicPart> key = new Pair<AttachmentDef, WeaponGraphicPart>(attachment, part);
		if (_removes.TryGetValue(key, out var value))
		{
			return value;
		}
		return _removes[key] = attachment.slotTags.Any((string s) => part.slotTags.Contains(s));
	}

	public void PrepareStats()
	{
		if (attachmentLinks == null)
		{
			attachmentLinks = new List<AttachmentLink>();
			return;
		}
		HashSet<StatDef> hashSet = new HashSet<StatDef>();
		for (int i = 0; i < attachmentLinks.Count; i++)
		{
			bool flag = true;
			bool flag2 = true;
			bool flag3 = true;
			AttachmentLink attachmentLink = attachmentLinks[i];
			if (!attachmentLink.attachment.statsValidated)
			{
				attachmentLink.attachment.ValidateStats();
			}
			if (attachmentLink.statReplacers == null)
			{
				attachmentLink.statReplacers = attachmentLink.attachment.statReplacers.ToList() ?? new List<StatModifier>();
				flag3 = false;
			}
			if (flag3 && attachmentLink.attachment.statReplacers.Count > 0)
			{
				foreach (StatModifier modifier in attachmentLink.attachment.statReplacers)
				{
					if (attachmentLink.statReplacers.All((StatModifier m) => m.stat != modifier.stat))
					{
						attachmentLink.statReplacers.Add(modifier);
					}
				}
			}
			if (attachmentLink.statOffsets == null)
			{
				attachmentLink.statOffsets = attachmentLink.attachment.statOffsets.ToList() ?? new List<StatModifier>();
				flag = false;
			}
			if (attachmentLink.statMultipliers == null)
			{
				attachmentLink.statMultipliers = attachmentLink.attachment.statMultipliers.ToList() ?? new List<StatModifier>();
				flag2 = false;
			}
			if (flag)
			{
				foreach (StatModifier modifier2 in attachmentLink.attachment.statOffsets)
				{
					if (attachmentLink.statOffsets.All((StatModifier m) => m.stat != modifier2.stat))
					{
						attachmentLink.statOffsets.Add(modifier2);
					}
				}
			}
			if (flag2)
			{
				foreach (StatModifier modifier3 in attachmentLink.attachment.statMultipliers)
				{
					if (attachmentLink.statMultipliers.All((StatModifier m) => m.stat != modifier3.stat))
					{
						attachmentLink.statMultipliers.Add(modifier3);
					}
				}
			}
			foreach (StatModifier modifier4 in attachmentLink.attachment.statReplacers)
			{
				if (statBases.All((StatModifier s) => s.stat != modifier4.stat))
				{
					statBases.Add(new StatModifier
					{
						value = modifier4.stat.defaultBaseValue,
						stat = modifier4.stat
					});
				}
			}
			foreach (StatModifier modifier5 in attachmentLink.attachment.statOffsets)
			{
				if (statBases.All((StatModifier s) => s.stat != modifier5.stat))
				{
					statBases.Add(new StatModifier
					{
						value = modifier5.stat.defaultBaseValue,
						stat = modifier5.stat
					});
				}
			}
			foreach (StatModifier modifier6 in attachmentLink.attachment.statMultipliers)
			{
				if (statBases.All((StatModifier s) => s.stat != modifier6.stat))
				{
					statBases.Add(new StatModifier
					{
						value = modifier6.stat.defaultBaseValue,
						stat = modifier6.stat
					});
				}
			}
			attachmentLink.PrepareTexture(this);
		}
	}
}
