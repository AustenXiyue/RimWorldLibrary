using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class FactionStrengthTracker : IExposable
{
	private enum FactionRecordType
	{
		None,
		SettlementDestroyed,
		SiteDestroyed,
		EnemyWeakened,
		LeaderKilled
	}

	private struct FactionStrengthRecord : IExposable
	{
		public int createdAt;

		public int duration;

		public float value;

		public FactionRecordType type;

		public int TicksLeft => Mathf.Max(duration - (GenTicks.TicksGame - createdAt), 0);

		public bool IsExpired => GenTicks.TicksGame - createdAt > duration + 1;

		public FactionStrengthRecord(FactionRecordType type, float value, int duration)
		{
			this.type = type;
			createdAt = GenTicks.TicksGame;
			this.value = value;
			this.duration = duration;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref type, "type", FactionRecordType.None);
			Scribe_Values.Look(ref createdAt, "createdAt", 0);
			Scribe_Values.Look(ref value, "value", 0f);
			Scribe_Values.Look(ref duration, "duration", 0);
		}
	}

	private string _explanation = null;

	private float _strengthMul = 1f;

	private int _explanationExpireAt = -1;

	private int _strengthMulExpireAt = -1;

	private Faction faction;

	private List<FactionStrengthRecord> records = new List<FactionStrengthRecord>();

	public Faction Faction => faction;

	public float StrengthPointsMultiplier => GetStrengthMultiplier();

	public bool CanRaid => StrengthPointsMultiplier >= 0.001f;

	public string Explanation => GetExplanation();

	public FactionStrengthTracker()
	{
	}

	public FactionStrengthTracker(Faction faction)
	{
		this.faction = faction;
	}

	public void ExposeData()
	{
		Scribe_References.Look(ref faction, "faction");
		Scribe_Collections.Look(ref records, "records", LookMode.Deep);
		if (records == null)
		{
			records = new List<FactionStrengthRecord>();
		}
	}

	public void TickLonger()
	{
		records.RemoveAll((FactionStrengthRecord r) => r.IsExpired);
	}

	public void Notify_LeaderKilled()
	{
		if (Rand.Chance(0.95f))
		{
			Register(FactionRecordType.LeaderKilled, Rand.Range(0.5f, 1f), Rand.Range(8, 30) * 60000);
		}
		if (Rand.Chance(0.95f))
		{
			Register(FactionRecordType.LeaderKilled, 0f, Rand.Range(8, 20) * 60000);
		}
	}

	public void Notify_SettlementDestroyed()
	{
		if (Rand.Chance(0.75f))
		{
			Register(FactionRecordType.SettlementDestroyed, Rand.Range(0.6f, 0.8f), Rand.Range(8, 25) * 60000);
		}
		if (Rand.Chance(0.75f))
		{
			Register(FactionRecordType.SettlementDestroyed, 0f, Rand.Range(8, 20) * 60000);
		}
	}

	public void Notify_SiteDestroyed()
	{
		if (Rand.Chance(0.75f))
		{
			Register(FactionRecordType.SiteDestroyed, Rand.Range(0.8f, 1f), Rand.Range(4, 30) * 60000);
		}
		if (Rand.Chance(0.75f))
		{
			Register(FactionRecordType.SiteDestroyed, 0f, Rand.Range(4, 15) * 60000);
		}
	}

	public void Notify_EnemyWeakened()
	{
		if (Rand.Chance(0.25f))
		{
			Register(FactionRecordType.EnemyWeakened, Rand.Range(1f, 1.2f), Rand.Range(4, 30) * 60000);
		}
	}

	public string GetExplanation()
	{
		if (_explanationExpireAt > GenTicks.TicksGame && !Controller.settings.DebugVerbose)
		{
			return _explanation;
		}
		int num = int.MaxValue;
		string arg = (faction.HasName ? faction.Name : faction.def.label);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("<color=orange>{0}:</color>", arg);
		if (DebugSettings.godMode)
		{
			stringBuilder.AppendFormat(" <color=grey>DEBUG: x{0}</color>", StrengthPointsMultiplier);
		}
		stringBuilder.AppendInNewLine(" <color=grey>");
		stringBuilder.Append("CE_FactionRecord_Explanation_Effects".Translate());
		stringBuilder.Append("</color>");
		foreach (FactionStrengthRecord record in records)
		{
			if (!record.IsExpired)
			{
				num = Math.Min(record.TicksLeft, num);
				stringBuilder.AppendLine();
				stringBuilder.Append(" ");
				stringBuilder.AppendFormat(GetRecordTypeMessage(record.type), arg);
				float num2 = (float)Math.Round((float)record.TicksLeft / 60000f, 1);
				if (record.value == 0f)
				{
					stringBuilder.AppendInNewLine("  - ");
					stringBuilder.AppendFormat("CE_FactionRecord_Explanation_RaidEmbargo".Translate(), num2);
				}
				else
				{
					stringBuilder.AppendInNewLine("  - ");
					stringBuilder.AppendFormat("CE_FactionRecord_Explanation_Strength".Translate(), (float)Math.Round(record.value, 1), num2);
				}
			}
		}
		_explanationExpireAt = GenTicks.TicksGame + num - 1;
		_explanation = stringBuilder.ToString();
		return _explanation;
	}

	private void Register(FactionRecordType recordType, float value, int duration)
	{
		FactionStrengthRecord item = new FactionStrengthRecord(recordType, value, duration);
		records.Add(item);
		_explanationExpireAt = -1;
		_strengthMulExpireAt = -1;
		GetStrengthMultiplier();
		GetExplanation();
		Alert_FactionInfluenced.instance?.Dirty();
	}

	private string GetRecordTypeMessage(FactionRecordType type)
	{
		return type switch
		{
			FactionRecordType.None => "CE_FactionRecord_Explanation_None".Translate(), 
			FactionRecordType.SettlementDestroyed => "CE_FactionRecord_Explanation_SettlementDestroyed".Translate(), 
			FactionRecordType.SiteDestroyed => "CE_FactionRecord_Explanation_SiteDestroyed".Translate(), 
			FactionRecordType.EnemyWeakened => "CE_FactionRecord_Explanation_EnemyWeakened".Translate(), 
			FactionRecordType.LeaderKilled => "CE_FactionRecord_Explanation_LeaderKilled".Translate(), 
			_ => throw new NotImplementedException(), 
		};
	}

	private float GetStrengthMultiplier()
	{
		if (GenTicks.TicksGame < _strengthMulExpireAt)
		{
			return _strengthMul;
		}
		float num = 1f;
		int num2 = int.MaxValue;
		for (int i = 0; i < records.Count; i++)
		{
			FactionStrengthRecord factionStrengthRecord = records[i];
			if (!factionStrengthRecord.IsExpired)
			{
				num *= factionStrengthRecord.value;
				num2 = Math.Min(num2, factionStrengthRecord.TicksLeft);
			}
		}
		_strengthMulExpireAt = GenTicks.TicksGame + num2 - 1;
		_strengthMul = num;
		return num;
	}
}
