using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended;

[StaticConstructorOnStartup]
public class Bipod_Injector
{
	static Bipod_Injector()
	{
		Add_and_change_all();
		AddJobModExts();
	}

	public static void Add_and_change_all()
	{
		foreach (BipodCategoryDef bipod_def in DefDatabase<BipodCategoryDef>.AllDefs)
		{
			List<ThingDef> list = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(delegate(ThingDef k)
			{
				List<string> weaponTags = k.weaponTags;
				int result;
				if (weaponTags != null && weaponTags.Any((string O) => O == bipod_def.bipod_id))
				{
					List<VerbProperties> verbs = k.Verbs;
					result = ((verbs != null && !verbs.Any((VerbProperties P) => P.verbClass == typeof(CompProperties_BipodComp))) ? 1 : 0);
				}
				else
				{
					result = 0;
				}
				return (byte)result != 0;
			});
			foreach (ThingDef item in list)
			{
				List<VerbProperties> verbs2 = item.Verbs;
				if (verbs2 != null && verbs2.Any((VerbProperties PP) => PP.verbClass == typeof(Verb_ShootCE) || PP.verbClass.IsSubclassOf(typeof(Verb_ShootCE))))
				{
					VerbProperties verbProperties = item.Verbs.Find((VerbProperties PP) => PP.verbClass == typeof(Verb_ShootCE) || PP.verbClass.IsSubclassOf(typeof(Verb_ShootCE)));
					VerbProperties verbProperties2 = verbProperties.MemberwiseClone();
					if (verbProperties2 != null)
					{
						verbProperties2.verbClass = verbProperties.verbClass;
						item.Verbs.Clear();
						item.comps.Add(new CompProperties_BipodComp
						{
							catDef = bipod_def,
							swayMult = bipod_def.swayMult,
							swayPenalty = bipod_def.swayPenalty,
							additionalrange = bipod_def.ad_Range,
							recoilMulton = bipod_def.recoil_mult_setup,
							recoilMultoff = bipod_def.recoil_mult_NOT_setup,
							ticksToSetUp = bipod_def.setuptime,
							warmupMult = bipod_def.warmup_mult_setup,
							warmupPenalty = bipod_def.warmup_mult_NOT_setup
						});
						item.Verbs.Add(verbProperties2);
						item.statBases.Add(new StatModifier
						{
							value = 0f,
							stat = CE_StatDefOf.BipodStats
						});
					}
					continue;
				}
				Log.Message("adding bipod failed in " + item.defName.Colorize(Color.red) + ". It appears to have no VerbShootCE in verbs. It's verbs are following:");
				foreach (VerbProperties verb in item.Verbs)
				{
					Log.Message(verb.verbClass.Name.Colorize(Color.magenta));
				}
			}
		}
	}

	public static void AddJobModExts()
	{
		List<JobDef> list = DefDatabase<JobDef>.AllDefsListForReading.FindAll((JobDef x) => x.defName.Contains("Wait") | x.defName.Contains("wait"));
		foreach (JobDef item in list)
		{
			if (item.modExtensions == null)
			{
				item.modExtensions = new List<DefModExtension>();
			}
			item.modExtensions.Add(new JobDefBipodCancelExtension());
		}
	}
}
