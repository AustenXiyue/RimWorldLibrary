using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CombatExtended.WorldObjects;

public class HealthComp : WorldObjectComp, IWorldCompCE
{
	public class WorldDamageInfo : IExposable
	{
		private float value;

		private ThingDef shellDef;

		public float Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}

		public ThingDef ShellDef
		{
			get
			{
				return shellDef;
			}
			set
			{
				shellDef = value;
			}
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref shellDef, "ShellDef");
			Scribe_Values.Look(ref value, "Value", 0f);
		}
	}

	public const float HEALTH_HEALRATE_DAY = 0.15f;

	private int lastTick = -1;

	private int _configTick = -1;

	private float health = 1f;

	private float negateChance = 0f;

	private float armorDamageMultiplier = 1f;

	private bool destroyedInstantly = false;

	public List<WorldDamageInfo> recentShells = new List<WorldDamageInfo>();

	public float Health
	{
		get
		{
			return health;
		}
		set
		{
			health = Mathf.Clamp01(value);
		}
	}

	public virtual float TotalDamageRequired => DestoyedInstantly ? 1f : (100f * ArmorDamageMultiplier);

	public virtual float DamageRequired => TotalDamageRequired * health;

	public virtual float HealingRatePerTick => (parent.Faction != null) ? ((float)(int)parent.Faction.def.techLevel / 4f * 0.15f / 60000f) : 0f;

	public virtual float ArmorDamageMultiplier
	{
		get
		{
			UpdateCacheValues();
			return armorDamageMultiplier;
		}
		protected set
		{
			armorDamageMultiplier = value;
		}
	}

	public virtual float NegateChance
	{
		get
		{
			UpdateCacheValues();
			return negateChance;
		}
		protected set
		{
			negateChance = Mathf.Clamp01(value);
		}
	}

	public virtual bool DestoyedInstantly
	{
		get
		{
			UpdateCacheValues();
			return destroyedInstantly;
		}
		protected set
		{
			destroyedInstantly = value;
		}
	}

	public WorldObjectCompProperties_Health Props => props as WorldObjectCompProperties_Health;

	private IEnumerable<Quest> RelatedQuests => Find.QuestManager.QuestsListForReading.Where((Quest x) => !x.Historical && x.QuestLookTargets.Contains(parent));

	public virtual void UpdateCacheValues()
	{
		if (_configTick == GenTicks.TicksGame)
		{
			return;
		}
		_configTick = GenTicks.TicksGame;
		if (recentShells == null)
		{
			recentShells = new List<WorldDamageInfo>();
		}
		bool flag = Props.techLevelNoImpact;
		ArmorDamageMultiplier = 1f;
		NegateChance = 0f;
		DestoyedInstantly = Props?.destoyedInstantly ?? false;
		if (Props.healthModifier > 0f)
		{
			ArmorDamageMultiplier *= Props.healthModifier;
		}
		WorldObjectHealthExtension worldObjectHealthExtension = parent.Faction?.def.GetModExtension<WorldObjectHealthExtension>();
		if (worldObjectHealthExtension != null)
		{
			if (worldObjectHealthExtension.healthModifier > 0f)
			{
				ArmorDamageMultiplier *= worldObjectHealthExtension.healthModifier;
			}
			if (worldObjectHealthExtension.chanceToNegateDamage >= 0f)
			{
				NegateChance = worldObjectHealthExtension.chanceToNegateDamage;
			}
			flag |= worldObjectHealthExtension.techLevelNoImpact;
			DestoyedInstantly |= worldObjectHealthExtension.destoyedInstantly;
		}
		if (parent is Site site)
		{
			foreach (SitePart part in site.parts)
			{
				WorldObjectHealthExtension worldObjectHealthExtension2 = part?.def?.GetModExtension<WorldObjectHealthExtension>();
				if (worldObjectHealthExtension2 != null)
				{
					if (worldObjectHealthExtension2.healthModifier > 0f)
					{
						ArmorDamageMultiplier *= worldObjectHealthExtension2.healthModifier;
					}
					if (worldObjectHealthExtension2.chanceToNegateDamage >= 0f)
					{
						NegateChance = worldObjectHealthExtension2.chanceToNegateDamage;
					}
					flag |= worldObjectHealthExtension2.techLevelNoImpact;
					DestoyedInstantly |= worldObjectHealthExtension2.destoyedInstantly;
				}
			}
		}
		if (!flag)
		{
			ArmorDamageMultiplier *= ((parent.Faction != null) ? Mathf.Max((int)parent.Faction.def.techLevel, 1f) : 1f);
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref health, "health", 1f);
		Scribe_Values.Look(ref lastTick, "lastTick", -1);
		Scribe_Collections.Look(ref recentShells, "recentShells", LookMode.Deep);
	}

	public virtual void ThrottledCompTick()
	{
		float num = Health;
		Health += HealingRatePerTick * 15f;
		if (Health == num)
		{
			return;
		}
		float num2 = Health - num;
		while (recentShells.Any() && num2 > 0f)
		{
			WorldDamageInfo worldDamageInfo = recentShells.First();
			float num3 = Mathf.Min(num2, worldDamageInfo.Value);
			worldDamageInfo.Value -= num3;
			num2 -= num3;
			if (worldDamageInfo.Value <= 0f)
			{
				recentShells.Remove(worldDamageInfo);
			}
		}
	}

	protected virtual void TryFinishDestroyQuests(Faction attackingFaction, GlobalTargetInfo sourceInfo)
	{
		Map map = sourceInfo.Map;
		QuestUtility.SendQuestTargetSignals(parent.questTags, "AllEnemiesDefeated", parent.Named("SUBJECT"), new NamedArgument(map, "MAP"));
		List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
		for (int i = 0; i < questsListForReading.Count; i++)
		{
			Quest quest = questsListForReading[i];
			if (!quest.Historical && quest.QuestLookTargets.Contains(parent))
			{
				Find.SignalManager.SendSignal(new Signal($"Quest{quest.id}.conditionCauser.Destroyed", new NamedArgument(map, "MAP")));
			}
		}
		foreach (Quest relatedQuest in RelatedQuests)
		{
			relatedQuest.End(QuestEndOutcome.Fail);
		}
		if (attackingFaction == Faction.OfPlayer && Find.Maps.Contains(map))
		{
			IdeoUtility.Notify_PlayerRaidedSomeone(map.mapPawns.FreeColonistsSpawned);
		}
	}

	public void ApplyDamage(ThingDef shellDef, Faction attackingFaction, GlobalTargetInfo sourceInfo)
	{
		if (!Rand.Chance(NegateChance))
		{
			if (DestoyedInstantly)
			{
				TryFinishDestroyQuests(attackingFaction, sourceInfo);
				TryDestroy();
				return;
			}
			float value = shellDef.GetWorldObjectDamageWorker().ApplyDamage(this, shellDef);
			recentShells.Add(new WorldDamageInfo
			{
				Value = value,
				ShellDef = shellDef
			});
			Notify_DamageTaken(attackingFaction, sourceInfo);
		}
	}

	private void TryDestroy()
	{
		if (!parent.Destroyed)
		{
			parent.Destroy();
		}
	}

	public virtual void Notify_DamageTaken(Faction attackingFaction, GlobalTargetInfo sourceInfo)
	{
		if ((double)health <= 0.0001)
		{
			TryFinishDestroyQuests(attackingFaction, sourceInfo);
			Notify_PreDestroyed(attackingFaction, sourceInfo);
			Destroy();
		}
	}

	public virtual void Notify_PreDestroyed(Faction attackingFaction, GlobalTargetInfo sourceInfo)
	{
		foreach (Building_TurretGunCE item in from Building_TurretGunCE x in from x in Find.Maps.SelectMany((Map x) => x.GetComponent<TurretTracker>().Turrets)
				where x.Faction == attackingFaction && x is Building_TurretGunCE
				select x
			where (x.globalTargetInfo.WorldObject?.tileInt ?? (-2)) == parent.tileInt
			select x)
		{
			item.ResetForcedTarget();
			item.ResetCurrentTarget();
		}
	}

	public virtual void Destroy(Faction attackingFaction = null)
	{
		int tile = parent.Tile;
		Faction faction = parent.Faction;
		FactionStrengthTracker strengthTracker = faction.GetStrengthTracker();
		if (parent is Settlement)
		{
			string text = ((faction == null) ? ((string)"CE_Message_SettlementDestroyed_Description".Translate().Formatted(parent.Label)) : ((attackingFaction == null) ? ((string)"CE_Message_SettlementDestroyed_Faction_Description".Translate().Formatted(parent.Label, faction.Name)) : ((string)"CE_Message_SettlementDestroyed_Description_Responsibility".Translate(attackingFaction.Name, parent.Label, faction.Name))));
			Find.LetterStack.ReceiveLetter("CE_Message_SettlementDestroyed_Label".Translate(), text, LetterDefOf.NeutralEvent);
			TryDestroy();
			strengthTracker?.Notify_SettlementDestroyed();
		}
		else
		{
			string text2 = null;
			text2 = ((faction != null) ? ((string)"CE_Message_SiteDestroyed_Faction".Translate().Formatted(parent.Label, faction.Name)) : ((string)"CE_Message_SiteDestroyed".Translate().Formatted(parent.Label)));
			Messages.Message(text2, MessageTypeDefOf.SituationResolved);
			if (strengthTracker != null && parent is Site)
			{
				strengthTracker.Notify_SiteDestroyed();
			}
			TryDestroy();
		}
		if (faction != null && faction.def.humanlikeFaction && attackingFaction != null && attackingFaction != faction)
		{
			FactionRelation factionRelation = faction.RelationWith(attackingFaction, allowNull: true);
			if (factionRelation == null)
			{
				faction.TryMakeInitialRelationsWith(attackingFaction);
				factionRelation = faction.RelationWith(attackingFaction, allowNull: true);
			}
			faction.TryAffectGoodwillWith(attackingFaction, -100, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.DestroyedEnemyBase, null);
		}
	}

	public override string CompInspectStringExtra()
	{
		if (DamageRequired != TotalDamageRequired || Prefs.DevMode)
		{
			StringBuilder stringBuilder = new StringBuilder($"{DamageRequired:0}/{TotalDamageRequired:0} HP");
			if (Prefs.DevMode && recentShells.Any())
			{
				stringBuilder.Append('\n');
				stringBuilder.Append(string.Join("\n", recentShells.Select((WorldDamageInfo x) => $"{x.ShellDef.defName} = {x.Value}")));
			}
			return stringBuilder.ToString();
		}
		return base.CompInspectStringExtra();
	}
}
