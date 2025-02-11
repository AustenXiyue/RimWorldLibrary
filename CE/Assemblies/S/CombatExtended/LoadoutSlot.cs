using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CombatExtended;

public class LoadoutSlot : IExposable
{
	private const int _defaultCount = 1;

	private int _count;

	private Def _def;

	private Type _type;

	private List<AttachmentDef> _attachments = new List<AttachmentDef>();

	private LoadoutCountType _countType = LoadoutCountType.pickupDrop;

	public int count
	{
		get
		{
			return _count;
		}
		set
		{
			_count = value;
		}
	}

	public LoadoutCountType countType
	{
		get
		{
			return _countType;
		}
		set
		{
			_countType = value;
		}
	}

	public bool allowAllAttachments => attachments.Count == 0;

	public ThingDef thingDef => (_def is ThingDef) ? ((ThingDef)_def) : null;

	public WeaponPlatformDef weaponPlatformDef => (_def is WeaponPlatformDef) ? ((WeaponPlatformDef)_def) : null;

	public List<AttachmentDef> attachments => _attachments;

	public LoadoutGenericDef genericDef => (_def is LoadoutGenericDef) ? ((LoadoutGenericDef)_def) : null;

	public bool isWeaponPlatform => _def is WeaponPlatformDef;

	public List<AttachmentLink> attachmentLinks => weaponPlatformDef.attachmentLinks.Where((AttachmentLink l) => attachments.Contains(l.attachment)).ToList();

	public string label => _def.label;

	public string LabelCap => _def.LabelCap;

	public float bulk
	{
		get
		{
			float val = ((thingDef != null) ? thingDef.GetStatValueAbstract(CE_StatDefOf.Bulk) : genericDef.bulk);
			if (isWeaponPlatform)
			{
				CE_StatDefOf.Bulk.TransformValue(attachmentLinks, ref val);
			}
			return val;
		}
	}

	public float mass
	{
		get
		{
			float val = ((thingDef != null) ? thingDef.GetStatValueAbstract(StatDefOf.Mass) : genericDef.mass);
			if (isWeaponPlatform)
			{
				StatDefOf.Mass.TransformValue(attachmentLinks, ref val);
			}
			return val;
		}
	}

	public LoadoutSlot(ThingDef def, int count = 1)
	{
		_type = typeof(ThingDef);
		_count = count;
		_def = def;
		_count = ((_count < 1) ? 1 : _count);
	}

	public LoadoutSlot(LoadoutGenericDef def, int count = 0)
	{
		_type = typeof(LoadoutGenericDef);
		if (count < 1)
		{
			count = def.defaultCount;
		}
		_count = ((count < 1) ? (_count = 1) : (_count = count));
		_countType = def.defaultCountType;
		_def = def;
	}

	public LoadoutSlot()
	{
		_count = 1;
	}

	public static LoadoutSlot FromConfig(LoadoutSlotConfig loadoutSlotConfig)
	{
		if (loadoutSlotConfig.isGenericDef)
		{
			LoadoutGenericDef named = DefDatabase<LoadoutGenericDef>.GetNamed(loadoutSlotConfig.defName, errorOnFail: false);
			return (named == null) ? null : new LoadoutSlot(named, loadoutSlotConfig.count);
		}
		ThingDef named2 = DefDatabase<ThingDef>.GetNamed(loadoutSlotConfig.defName, errorOnFail: false);
		return (named2 == null) ? null : new LoadoutSlot(named2, loadoutSlotConfig.count);
	}

	public LoadoutSlotConfig ToConfig()
	{
		LoadoutSlotConfig loadoutSlotConfig = new LoadoutSlotConfig();
		loadoutSlotConfig.isGenericDef = _type == typeof(LoadoutGenericDef);
		loadoutSlotConfig.defName = _def.defName;
		loadoutSlotConfig.count = _count;
		return loadoutSlotConfig;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref _count, "count", 1);
		Scribe_Values.Look(ref _type, "DefType");
		Scribe_Collections.Look(ref _attachments, "Attachments", LookMode.Def);
		if (_attachments == null)
		{
			_attachments = new List<AttachmentDef>();
		}
		ThingDef value = thingDef;
		LoadoutGenericDef value2 = genericDef;
		if (_type == typeof(ThingDef))
		{
			Scribe_Defs.Look(ref value, "def");
		}
		if (_type == typeof(LoadoutGenericDef))
		{
			Scribe_Defs.Look(ref value2, "def");
		}
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			_def = ((_type == typeof(ThingDef)) ? ((Def)value) : ((Def)value2));
		}
		if (genericDef != null)
		{
			Scribe_Values.Look(ref _countType, "countType", LoadoutCountType.pickupDrop);
		}
	}

	public bool isSameDef(LoadoutSlot slot)
	{
		Def def = (Def)(((object)slot.thingDef) ?? ((object)slot.genericDef));
		if (isWeaponPlatform && slot.isWeaponPlatform && _def == def)
		{
			int num = slot.attachments.Intersect(attachments).Count();
			return num == slot.attachments.Count && num == attachments.Count;
		}
		return _def == def;
	}

	public LoadoutSlot Copy()
	{
		if (genericDef != null)
		{
			LoadoutSlot loadoutSlot = new LoadoutSlot(genericDef, _count);
			loadoutSlot.countType = _countType;
			if (isWeaponPlatform)
			{
				loadoutSlot.attachments.AddRange(attachments);
			}
			return loadoutSlot;
		}
		return new LoadoutSlot(thingDef, _count);
	}
}
