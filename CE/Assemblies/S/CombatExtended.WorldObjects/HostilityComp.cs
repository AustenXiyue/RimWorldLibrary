using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CombatExtended.WorldObjects;

public class HostilityComp : WorldObjectComp, IWorldCompCE
{
	private int lastRaidTick = -1;

	private int lastTick = -1;

	public HostilitySheller sheller;

	public HostilityRaider raider;

	private int _configTick = -1;

	private float _shellingPropability;

	private float _raidMTBDays;

	private float _raidPropability;

	private List<ShellingResponseDef.ShellingResponsePart_Projectile> _availableProjectiles;

	public virtual float RaidPropability
	{
		get
		{
			UpdateCachedConfig();
			return _raidPropability;
		}
	}

	public virtual float RaidMTBDays
	{
		get
		{
			UpdateCachedConfig();
			return _raidMTBDays;
		}
	}

	public virtual float ShellingPropability
	{
		get
		{
			UpdateCachedConfig();
			return _shellingPropability;
		}
	}

	public virtual List<ShellingResponseDef.ShellingResponsePart_Projectile> AvailableProjectiles
	{
		get
		{
			UpdateCachedConfig();
			return _availableProjectiles;
		}
	}

	public HostilityComp()
	{
		sheller = new HostilitySheller();
		sheller.comp = this;
		raider = new HostilityRaider();
		raider.comp = this;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref lastTick, "lastTick", -1);
		Scribe_Values.Look(ref lastRaidTick, "lastRaidTick", -1);
		Scribe_Deep.Look(ref sheller, "sheller");
		Scribe_Deep.Look(ref raider, "raider");
		if (Scribe.mode != LoadSaveMode.Saving)
		{
			if (raider == null)
			{
				raider = new HostilityRaider();
			}
			raider.comp = this;
			if (sheller == null)
			{
				sheller = new HostilitySheller();
			}
			sheller.comp = this;
		}
	}

	public virtual void ThrottledCompTick()
	{
		sheller.ThrottledTick();
		raider.ThrottledTick();
	}

	public virtual void TryHostilityResponse(Faction attackingFaction, GlobalTargetInfo sourceInfo)
	{
		UpdateCachedConfig();
		Notify_Shelled(attackingFaction, sourceInfo);
		if (parent is MapParent { HasMap: not false, Map: not null } || parent.Faction == null || parent.Faction.IsPlayer || parent.Faction.defeated)
		{
			return;
		}
		Map map = sourceInfo.Map;
		MapParent mapParent2 = Find.World.worldObjects.MapParentAt(sourceInfo.Tile);
		if (map == null && mapParent2 != null && mapParent2.HasMap && mapParent2.Map != null && Find.Maps.Contains(mapParent2.Map))
		{
			map = mapParent2.Map;
		}
		float b;
		if (map != null)
		{
			b = StorytellerUtility.DefaultThreatPointsNow(map) * (float)Mathf.Max((int)(parent.Faction.def.techLevel - 4), 1) / 2f;
			b = Mathf.Max(500f, b);
		}
		else
		{
			b = Rand.Range(500f, 500 * Mathf.Max((int)(parent.Faction.def.techLevel - 2), 2));
		}
		Faction other = mapParent2?.Faction;
		if (!sheller.Shooting && Rand.Chance(ShellingPropability) && parent.Faction.HostileTo(other))
		{
			sheller.TryStartShelling(sourceInfo, b, attackingFaction);
		}
		if (map != null)
		{
			int num = (int)(RaidMTBDays * 60000f);
			int num2 = ((lastRaidTick > 0) ? (GenTicks.TicksGame - lastRaidTick) : (num + 1));
			if (num2 != num && (float)num2 > (float)num / 2f && Rand.Chance(RaidPropability / (float)Mathf.Max(num - num2, 1)) && raider.TryRaid(map, b))
			{
				lastRaidTick = GenTicks.TicksGame;
				Messages.Message("CE_Message_CounterRaid".Translate(parent.Label, attackingFaction.Name, map.Parent.Label), MessageTypeDefOf.ThreatBig);
			}
		}
	}

	public virtual void Notify_Destoyed(Faction attackingFaction, GlobalTargetInfo sourceInfo)
	{
	}

	public virtual void Notify_Shelled(Faction attackingFaction, GlobalTargetInfo sourceInfo)
	{
		Faction faction = parent.Faction;
		if (faction != null && attackingFaction != null && attackingFaction != faction)
		{
			FactionRelation factionRelation = faction.RelationWith(attackingFaction, allowNull: true);
			if (factionRelation == null)
			{
				faction.TryMakeInitialRelationsWith(attackingFaction);
				factionRelation = faction.RelationWith(attackingFaction, allowNull: true);
			}
			faction.TryAffectGoodwillWith(attackingFaction, -75, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.AttackedSettlement, sourceInfo);
		}
	}

	protected virtual void UpdateCachedConfig()
	{
		if (_configTick != GenTicks.TicksGame)
		{
			ShellingResponseDef shellingResponseDef = parent.Faction.GetShellingResponseDef();
			ShellingResponseDef.ShellingResponsePart_WorldObject shellingResponsePart_WorldObject;
			if ((shellingResponsePart_WorldObject = shellingResponseDef.worldObjects?.FirstOrDefault((ShellingResponseDef.ShellingResponsePart_WorldObject w) => w.worldObject == parent.def) ?? null) == null)
			{
				_raidPropability = shellingResponseDef.defaultRaidPropability;
				_raidMTBDays = shellingResponseDef.defaultRaidMTBDays;
				_shellingPropability = shellingResponseDef.defaultShellingPropability;
			}
			else
			{
				_raidPropability = shellingResponsePart_WorldObject.raidPropability;
				_raidMTBDays = shellingResponsePart_WorldObject.raidMTBDays;
				_shellingPropability = shellingResponsePart_WorldObject.shellingPropability;
			}
			_configTick = GenTicks.TicksGame;
			_availableProjectiles = shellingResponseDef.projectiles;
		}
	}
}
