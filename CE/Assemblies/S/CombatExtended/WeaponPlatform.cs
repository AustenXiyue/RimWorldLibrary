using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class WeaponPlatform : ThingWithComps
{
	public readonly List<AttachmentLink> attachments = new List<AttachmentLink>();

	private Quaternion drawQat;

	private List<WeaponPlatformDef.WeaponGraphicPart> _defaultPart = new List<WeaponPlatformDef.WeaponGraphicPart>();

	private List<AttachmentDef> _additionList = new List<AttachmentDef>();

	private List<AttachmentDef> _removalList = new List<AttachmentDef>();

	private List<AttachmentDef> _targetConfig = new List<AttachmentDef>();

	private AttachmentLink[] _curLinks;

	private CompEquippable _compEquippable;

	private WeaponPlatformDef _platformDef;

	private Dictionary<AttachmentDef, AttachmentLink> _LinkByDef = new Dictionary<AttachmentDef, AttachmentLink>();

	private Matrix4x4 _drawMat;

	private Vector3 _drawLoc;

	private List<Pair<Material, Mesh>> _graphicCache;

	private List<Pair<Material, Mesh>> _graphicFlipCache;

	public List<AttachmentDef> TargetConfig
	{
		get
		{
			return _targetConfig.ToList();
		}
		set
		{
			_targetConfig = value;
			UpdateConfiguration();
		}
	}

	public AttachmentLink[] CurLinks
	{
		get
		{
			if (_curLinks == null || _curLinks.Length != attachments.Count)
			{
				_curLinks = attachments.ToArray();
			}
			return _curLinks;
		}
	}

	public bool ConfigApplied => _additionList.Count == 0 && _removalList.Count == 0;

	public CompEquippable CompEquippable
	{
		get
		{
			if (_compEquippable == null)
			{
				_compEquippable = GetComp<CompEquippable>();
			}
			return _compEquippable;
		}
	}

	public Pawn Wielder
	{
		get
		{
			if (CompEquippable != null && CompEquippable.PrimaryVerb != null && CompEquippable.PrimaryVerb.caster != null)
			{
				Pawn pawn = (CompEquippable?.parent?.ParentHolder as Pawn_EquipmentTracker)?.pawn;
				if (pawn == null || pawn == CompEquippable?.PrimaryVerb?.CasterPawn)
				{
					return CompEquippable.PrimaryVerb.CasterPawn;
				}
			}
			return null;
		}
	}

	public List<AttachmentDef> RemovalList => _removalList;

	public List<AttachmentDef> AdditionList => _additionList;

	public List<WeaponPlatformDef.WeaponGraphicPart> VisibleDefaultParts
	{
		get
		{
			if (_defaultPart == null)
			{
				_defaultPart = new List<WeaponPlatformDef.WeaponGraphicPart>();
			}
			return _defaultPart;
		}
	}

	public WeaponPlatformDef Platform
	{
		get
		{
			if (_platformDef == null)
			{
				_platformDef = (WeaponPlatformDef)def;
			}
			return _platformDef;
		}
	}

	public Dictionary<AttachmentDef, AttachmentLink> LinkByDef
	{
		get
		{
			if (_LinkByDef.Count != Platform.attachmentLinks.Count)
			{
				_LinkByDef.Clear();
				foreach (AttachmentLink attachmentLink in Platform.attachmentLinks)
				{
					_LinkByDef.Add(attachmentLink.attachment, attachmentLink);
				}
			}
			return _LinkByDef;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		List<AttachmentDef> list = attachments.Select((AttachmentLink l) => l.attachment).ToList();
		Scribe_Collections.Look(ref list, "attachments", LookMode.Def);
		if (Scribe.mode != LoadSaveMode.Saving && list != null)
		{
			attachments.Clear();
			attachments.AddRange(list.Select((AttachmentDef a) => Platform.attachmentLinks.First((AttachmentLink l) => l.attachment == a)));
		}
		Scribe_Collections.Look(ref _additionList, "additionList", LookMode.Def);
		if (_additionList == null)
		{
			_additionList = new List<AttachmentDef>();
		}
		Scribe_Collections.Look(ref _removalList, "removalList", LookMode.Def);
		if (_removalList == null)
		{
			_removalList = new List<AttachmentDef>();
		}
		Scribe_Collections.Look(ref _targetConfig, "targetConfig", LookMode.Def);
		if (_targetConfig == null)
		{
			_targetConfig = new List<AttachmentDef>();
		}
		if (Scribe.mode != LoadSaveMode.Saving)
		{
			UpdateConfiguration();
		}
	}

	public IEnumerable<AttachmentDef> GetModificationList()
	{
		return AdditionList.Concat(RemovalList);
	}

	public override void PostPostMake()
	{
		base.PostPostMake();
		UpdateConfiguration();
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		float ang = Rand.Range(0f - Platform.graphicData.onGroundRandomRotateAngle, Platform.graphicData.onGroundRandomRotateAngle) % 180f;
		drawQat = ang.ToQuat();
		UpdateConfiguration();
	}

	public override IEnumerable<InspectTabBase> GetInspectTabs()
	{
		List<InspectTabBase> list = base.GetInspectTabs()?.ToList() ?? new List<InspectTabBase>();
		if (!list.Any((InspectTabBase t) => t is ITab_AttachmentView))
		{
			list.Add(InspectTabManager.GetSharedInstance(typeof(ITab_AttachmentView)));
		}
		return list;
	}

	public override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		if (_drawLoc.x != drawLoc.x || _drawLoc.z != drawLoc.z)
		{
			_drawMat = default(Matrix4x4);
			_drawMat.SetTRS(drawLoc, drawQat, Vector3.one);
			_drawLoc = drawLoc;
		}
		DrawPlatform(_drawMat);
	}

	public AttachmentLink GetLink(AttachmentDef def)
	{
		AttachmentLink value;
		return LinkByDef.TryGetValue(def, out value) ? value : null;
	}

	public void DrawPlatform(Matrix4x4 matrix, bool flip = false, int layer = 0)
	{
		if (_graphicCache == null)
		{
			UpdateDrawCache();
		}
		List<Pair<Material, Mesh>> list = ((!flip) ? _graphicCache : _graphicFlipCache);
		for (int i = 0; i < list.Count; i++)
		{
			Pair<Material, Mesh> pair = list[i];
			Graphics.DrawMesh(pair.Second, matrix, pair.First, layer);
		}
	}

	public void UpdateConfiguration()
	{
		_removalList.Clear();
		_additionList.Clear();
		_curLinks = attachments.Select((AttachmentLink t) => LinkByDef[t.attachment]).ToArray();
		foreach (AttachmentLink attachmentLink in Platform.attachmentLinks)
		{
			AttachmentDef def = attachmentLink.attachment;
			bool flag = _targetConfig.Any((AttachmentDef d) => d.index == def.index);
			bool flag2 = attachments.Any((AttachmentLink l) => l.attachment.index == def.index);
			if (flag && !flag2)
			{
				_additionList.Add(def);
			}
			else if (!flag && flag2)
			{
				_removalList.Add(def);
			}
		}
		_defaultPart.Clear();
		foreach (WeaponPlatformDef.WeaponGraphicPart defaultGraphicPart in Platform.defaultGraphicParts)
		{
			if (defaultGraphicPart.slotTags == null || defaultGraphicPart.slotTags.All((string s) => _curLinks.All((AttachmentLink l) => !l.attachment.slotTags.Contains(s))))
			{
				_defaultPart.Add(defaultGraphicPart);
			}
		}
		_graphicCache = null;
		_graphicFlipCache = null;
	}

	public void UpdateDrawCache()
	{
		if (_graphicCache == null)
		{
			_graphicCache = new List<Pair<Material, Mesh>>();
		}
		_graphicCache.Clear();
		if (_graphicFlipCache == null)
		{
			_graphicFlipCache = new List<Pair<Material, Mesh>>();
		}
		_graphicFlipCache.Clear();
		for (int i = 0; i < _defaultPart.Count; i++)
		{
			WeaponPlatformDef.WeaponGraphicPart weaponGraphicPart = _defaultPart[i];
			if (weaponGraphicPart.HasOutline)
			{
				_graphicCache.Add(new Pair<Material, Mesh>(weaponGraphicPart.OutlineMat, CE_MeshMaker.plane10Bot));
				_graphicFlipCache.Add(new Pair<Material, Mesh>(weaponGraphicPart.OutlineMat, CE_MeshMaker.plane10FlipBot));
			}
		}
		for (int j = 0; j < _curLinks.Length; j++)
		{
			AttachmentLink attachmentLink = _curLinks[j];
			if (attachmentLink.HasOutline)
			{
				_graphicCache.Add(new Pair<Material, Mesh>(attachmentLink.OutlineMat, attachmentLink.meshBot));
				_graphicFlipCache.Add(new Pair<Material, Mesh>(attachmentLink.OutlineMat, attachmentLink.meshFlipBot));
			}
		}
		_graphicCache.Add(new Pair<Material, Mesh>(Platform.graphic.MatSingle, CE_MeshMaker.plane10Mid));
		_graphicFlipCache.Add(new Pair<Material, Mesh>(Platform.graphic.MatSingle, CE_MeshMaker.plane10FlipMid));
		for (int k = 0; k < _defaultPart.Count; k++)
		{
			WeaponPlatformDef.WeaponGraphicPart weaponGraphicPart2 = _defaultPart[k];
			if (weaponGraphicPart2.HasPartMat)
			{
				_graphicCache.Add(new Pair<Material, Mesh>(weaponGraphicPart2.PartMat, CE_MeshMaker.plane10Top));
				_graphicFlipCache.Add(new Pair<Material, Mesh>(weaponGraphicPart2.PartMat, CE_MeshMaker.plane10FlipTop));
			}
		}
		for (int l = 0; l < _curLinks.Length; l++)
		{
			AttachmentLink attachmentLink2 = _curLinks[l];
			if (attachmentLink2.HasAttachmentMat)
			{
				_graphicCache.Add(new Pair<Material, Mesh>(attachmentLink2.AttachmentMat, attachmentLink2.meshTop));
				_graphicFlipCache.Add(new Pair<Material, Mesh>(attachmentLink2.AttachmentMat, attachmentLink2.meshFlipTop));
			}
		}
	}
}
