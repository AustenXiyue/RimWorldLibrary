using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CombatExtended.Compatibility;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatExtended;

public static class CE_Utility
{
	private const int blitMaxDimensions = 64;

	public static List<ThingDef> allWeaponDefs;

	private static readonly SimpleCurve RecoilCurveAxisY;

	private static readonly SimpleCurve RecoilCurveRotation;

	private const float RecoilMagicNumber = 2.6f;

	private const float MuzzleRiseMagicNumber = 0.1f;

	public const float GravityConst = 1.96f;

	private static Map[] _mapsLighting;

	private static LightingTracker[] _lightingTrackers;

	private static Map[] _mapsDanger;

	private static DangerTracker[] _DangerTrackers;

	private static readonly SimpleCurve lightingCurve;

	private static readonly List<PawnKindDef> _validPawnKinds;

	public static float CameraAltitude => Find.CameraDriver?.CurrentRealPosition.y ?? (-1f);

	public static float RandomGaussian(float minValue = 0f, float maxValue = 1f)
	{
		float num;
		float num3;
		do
		{
			num = 2f * UnityEngine.Random.value - 1f;
			float num2 = 2f * UnityEngine.Random.value - 1f;
			num3 = num * num + num2 * num2;
		}
		while (num3 >= 1f);
		float num4 = num * Mathf.Sqrt(-2f * Mathf.Log(num3) / num3);
		float num5 = (minValue + maxValue) / 2f;
		float num6 = (maxValue - num5) / 5f;
		return Mathf.Clamp(num4 * num6 + num5, minValue, maxValue);
	}

	public static float GetWeaponStatWith(this WeaponPlatform platform, StatDef stat, List<AttachmentLink> links, bool applyPostProcess = true)
	{
		StatRequest req = StatRequest.For(platform);
		float val = stat.Worker.GetValueUnfinalized(StatRequest.For(platform));
		if (stat.parts != null)
		{
			for (int i = 0; i < stat.parts.Count; i++)
			{
				if (!(stat.parts[i] is StatPart_Attachments))
				{
					stat.parts[i].TransformValue(req, ref val);
				}
			}
			if (links != null)
			{
				stat.TransformValue(links, ref val);
			}
		}
		if (applyPostProcess && stat.postProcessCurve != null)
		{
			val = stat.postProcessCurve.Evaluate(val);
		}
		if (applyPostProcess && stat.postProcessStatFactors != null)
		{
			for (int j = 0; j < stat.postProcessStatFactors.Count; j++)
			{
				val *= req.Thing.GetStatValue(stat.postProcessStatFactors[j]);
			}
		}
		if (Find.Scenario != null)
		{
			val *= Find.Scenario.GetStatFactor(stat);
		}
		if (Mathf.Abs(val) > stat.roundToFiveOver)
		{
			val = Mathf.Round(val / 5f) * 5f;
		}
		if (stat.roundValue)
		{
			val = Mathf.RoundToInt(val);
		}
		if (applyPostProcess)
		{
			val = Mathf.Clamp(val, stat.minValue, stat.maxValue);
		}
		return val;
	}

	public static bool TrySyncPlatformLoadout(this WeaponPlatform platform, Pawn pawn)
	{
		Loadout loadout = pawn.GetLoadout();
		if (loadout == null)
		{
			return false;
		}
		LoadoutSlot loadoutSlot = loadout.Slots.FirstOrFallback((LoadoutSlot s) => s.weaponPlatformDef == platform.Platform);
		if (loadoutSlot == null || loadoutSlot.allowAllAttachments)
		{
			return false;
		}
		bool flag = false;
		foreach (AttachmentDef def in loadoutSlot.attachments)
		{
			if (!platform.TargetConfig.Any((AttachmentDef a) => a == def))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			platform.TargetConfig = loadoutSlot.attachments;
			platform.UpdateConfiguration();
		}
		return flag;
	}

	public static string ExplainAttachmentsStat(this StatDef stat, IEnumerable<AttachmentLink> links)
	{
		if (links == null || links.Count() == 0)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		bool flag2 = false;
		foreach (AttachmentLink link in links)
		{
			StatModifier statModifier = link.statReplacers?.FirstOrFallback((StatModifier m) => m.stat == stat) ?? null;
			if (statModifier == null)
			{
				continue;
			}
			stringBuilder.AppendLine(string.Concat("Replaced with " + link.attachment.LabelCap + ": ", statModifier.value.ToString()));
			break;
		}
		foreach (AttachmentLink link2 in links)
		{
			StatModifier statModifier2 = link2.statOffsets?.FirstOrFallback((StatModifier m) => m.stat == stat) ?? null;
			if (statModifier2 != null && statModifier2.value != 0f)
			{
				if (!flag)
				{
					stringBuilder.AppendLine("Attachment offsets:");
					flag = true;
				}
				stringBuilder.AppendLine("    " + link2.attachment.LabelCap + ": " + stat.Worker.ValueToString(statModifier2.value, finalized: false, ToStringNumberSense.Offset));
			}
		}
		foreach (AttachmentLink link3 in links)
		{
			StatModifier statModifier3 = link3.statMultipliers?.FirstOrFallback((StatModifier m) => m.stat == stat) ?? null;
			if (statModifier3 != null && statModifier3.value != 0f && statModifier3.value != 1f)
			{
				if (!flag2)
				{
					stringBuilder.AppendLine("Attachment factors:");
					flag2 = true;
				}
				stringBuilder.AppendLine("    " + link3.attachment.LabelCap + ": " + stat.Worker.ValueToString(statModifier3.value, finalized: false, ToStringNumberSense.Factor));
			}
		}
		return stringBuilder.ToString();
	}

	public static float GetWeaponStatAbstractWith(this WeaponPlatformDef platform, StatDef stat, List<AttachmentLink> links, bool applyPostProcess = true)
	{
		platform.statBases.FirstOrFallback((StatModifier s) => s.stat == stat);
		float val = platform.statBases.FirstOrFallback((StatModifier s) => s.stat == stat)?.value ?? stat.defaultBaseValue;
		if (stat.parts != null)
		{
			for (int i = 0; i < stat.parts.Count; i++)
			{
				if (stat.parts[i] is StatPart_Quality || stat.parts[i] is StatPart_Quality_Offset)
				{
					stat.parts[i].TransformValue(new StatRequest
					{
						qualityCategoryInt = QualityCategory.Normal
					}, ref val);
				}
			}
			if (links != null)
			{
				stat.TransformValue(links, ref val);
			}
		}
		if (applyPostProcess && stat.postProcessCurve != null)
		{
			val = stat.postProcessCurve.Evaluate(val);
		}
		if (Find.Scenario != null)
		{
			val *= Find.Scenario.GetStatFactor(stat);
		}
		if (Mathf.Abs(val) > stat.roundToFiveOver)
		{
			val = Mathf.Round(val / 5f) * 5f;
		}
		if (stat.roundValue)
		{
			val = Mathf.RoundToInt(val);
		}
		if (applyPostProcess)
		{
			val = Mathf.Clamp(val, stat.minValue, stat.maxValue);
		}
		return val;
	}

	public static bool CanAttachTo(this AttachmentDef attachment, WeaponPlatform platform)
	{
		foreach (AttachmentLink attachment2 in platform.attachments)
		{
			if (!platform.Platform.AttachmentsCompatible(attachment2.attachment, attachment))
			{
				return false;
			}
		}
		return true;
	}

	public static void TransformValue(this StatDef stat, List<AttachmentLink> links, ref float val)
	{
		if (links == null || links.Count == 0)
		{
			return;
		}
		for (int i = 0; i < links.Count; i++)
		{
			AttachmentLink attachmentLink = links[i];
			StatModifier statModifier = attachmentLink.statReplacers?.FirstOrFallback((StatModifier m) => m.stat == stat) ?? null;
			if (statModifier != null)
			{
				val = statModifier.value;
				return;
			}
		}
		for (int j = 0; j < links.Count; j++)
		{
			AttachmentLink attachmentLink2 = links[j];
			StatModifier statModifier2 = attachmentLink2.statOffsets?.FirstOrFallback((StatModifier m) => m.stat == stat) ?? null;
			if (statModifier2 != null)
			{
				val += statModifier2.value;
			}
		}
		for (int k = 0; k < links.Count; k++)
		{
			AttachmentLink attachmentLink3 = links[k];
			StatModifier statModifier3 = attachmentLink3.statMultipliers?.FirstOrFallback((StatModifier m) => m.stat == stat) ?? null;
			if (statModifier3 != null && !(statModifier3.value <= 0f))
			{
				val *= statModifier3.value;
			}
		}
	}

	public static Texture2D Blit(this Texture2D texture, Rect blitRect, int[] rtSize)
	{
		FilterMode filterMode = texture.filterMode;
		texture.filterMode = FilterMode.Point;
		RenderTexture temporary = RenderTexture.GetTemporary(rtSize[0], rtSize[1], 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);
		temporary.filterMode = FilterMode.Point;
		RenderTexture.active = temporary;
		Graphics.Blit(texture, temporary);
		Texture2D texture2D = new Texture2D((int)blitRect.width, (int)blitRect.height);
		texture2D.ReadPixels(blitRect, 0, 0);
		texture2D.Apply();
		RenderTexture.active = null;
		texture.filterMode = filterMode;
		return texture2D;
	}

	public static Color[] GetColorSafe(this Texture2D texture, out int width, out int height)
	{
		width = texture.width;
		height = texture.height;
		if (texture.width > texture.height)
		{
			width = Math.Min(width, 64);
			height = (int)((float)width * ((float)texture.height / (float)texture.width));
		}
		else if (texture.height > texture.width)
		{
			height = Math.Min(height, 64);
			width = (int)((float)height * ((float)texture.width / (float)texture.height));
		}
		else
		{
			width = Math.Min(width, 64);
			height = Math.Min(height, 64);
		}
		Color[] array = null;
		Rect blitRect = new Rect(0f, 0f, width, height);
		int[] rtSize = new int[2] { width, height };
		if (width == texture.width && height == texture.height)
		{
			try
			{
				return texture.GetPixels();
			}
			catch
			{
				return texture.Blit(blitRect, rtSize).GetPixels();
			}
		}
		return texture.Blit(blitRect, rtSize).GetPixels();
	}

	public static Texture2D BlitCrop(this Texture2D texture, Rect blitRect)
	{
		return texture.Blit(blitRect, new int[2] { texture.width, texture.height });
	}

	public static T GetLastModExtension<T>(this Def def) where T : DefModExtension
	{
		if (def.modExtensions == null)
		{
			return null;
		}
		for (int num = def.modExtensions.Count - 1; num >= 0; num--)
		{
			if (def.modExtensions[num] is T)
			{
				return def.modExtensions[num] as T;
			}
		}
		return null;
	}

	public static float PartialStat(this Apparel apparel, StatDef stat, BodyPartRecord part)
	{
		if (!apparel.def.apparel.CoversBodyPart(part))
		{
			return 0f;
		}
		float num = apparel.GetStatValue(stat);
		if (Controller.settings.PartialStat && apparel.def.HasModExtension<PartialArmorExt>())
		{
			foreach (ApparelPartialStat stat2 in apparel.def.GetModExtension<PartialArmorExt>().stats)
			{
				if ((stat2?.parts?.Contains(part.def) == true) | (stat2?.parts?.Contains(part?.parent?.def) == true && part.depth == BodyPartDepth.Inside))
				{
					if (stat2.staticValue > 0f)
					{
						return stat2.staticValue;
					}
					num *= stat2.mult;
					break;
				}
			}
		}
		return num;
	}

	public static float PartialStat(this Pawn pawn, StatDef stat, BodyPartRecord part, float damage = 0f, float AP = 0f)
	{
		float num = pawn.GetStatValue(stat);
		if (Controller.settings.PartialStat && pawn.def.HasModExtension<PartialArmorExt>())
		{
			foreach (ApparelPartialStat stat2 in pawn.def.GetModExtension<PartialArmorExt>().stats)
			{
				if (stat2.stat == stat && ((stat2?.parts?.Contains(part.def) == true) | (stat2?.parts?.Contains(part?.parent?.def) == true && part.depth == BodyPartDepth.Inside)))
				{
					if (stat2.staticValue > 0f)
					{
						return stat2.staticValue;
					}
					num *= stat2.mult;
					break;
				}
			}
		}
		return num;
	}

	public static float PartialStat(this Apparel apparel, StatDef stat, BodyPartDef part)
	{
		float num = apparel.GetStatValue(stat);
		if (apparel.def.HasModExtension<PartialArmorExt>())
		{
			foreach (ApparelPartialStat stat2 in apparel.def.GetModExtension<PartialArmorExt>().stats)
			{
				if (stat2?.parts?.Contains(part) == true)
				{
					if (stat2.staticValue > 0f)
					{
						return stat2.staticValue;
					}
					num *= stat2.mult;
					break;
				}
			}
		}
		return num;
	}

	public static float PartialStat(this Pawn pawn, StatDef stat, BodyPartDef part)
	{
		float num = pawn.GetStatValue(stat);
		if (pawn.def.HasModExtension<PartialArmorExt>())
		{
			foreach (ApparelPartialStat stat2 in pawn.def.GetModExtension<PartialArmorExt>().stats)
			{
				if (stat2?.parts?.Contains(part) == true)
				{
					if (stat2.staticValue > 0f)
					{
						return stat2.staticValue;
					}
					num *= stat2.mult;
					break;
				}
			}
		}
		return num;
	}

	public static void UpdateLabel(this Def def, string label)
	{
		def.label = label;
		def.cachedLabelCap = "";
	}

	public static Vector2 GenRandInCircle(float radius)
	{
		double num = (double)Rand.Value * Math.PI * 2.0;
		double num2 = Rand.Value * radius;
		return new Vector2((float)(num2 * Math.Cos(num)), (float)(num2 * Math.Sin(num)));
	}

	public static float GetMoveSpeed(Pawn pawn)
	{
		if (!pawn.pather.Moving)
		{
			return 0f;
		}
		float num = 60f / pawn.GetStatValue(StatDefOf.MoveSpeed, applyPostProcess: false);
		num += (float)pawn.Map.pathing.For(pawn).pathGrid.CalculatedCostAt(pawn.pather.nextCell, perceivedStatic: false, pawn.Position);
		Building edifice = pawn.Position.GetEdifice(pawn.Map);
		if (edifice != null)
		{
			num += (float)(int)edifice.PathWalkCostFor(pawn);
		}
		if (pawn.CurJob != null)
		{
			switch (pawn.CurJob.locomotionUrgency)
			{
			case LocomotionUrgency.Amble:
				num *= 3f;
				if (num < 60f)
				{
					num = 60f;
				}
				break;
			case LocomotionUrgency.Walk:
				num *= 2f;
				if (num < 50f)
				{
					num = 50f;
				}
				break;
			case LocomotionUrgency.Sprint:
				num = Mathf.RoundToInt(num * 0.75f);
				break;
			}
		}
		return 60f / num;
	}

	public static float GetLightingShift(Thing caster, float glowAtTarget)
	{
		return Mathf.Max((1f - glowAtTarget) * (1f - caster.GetStatValue(CE_StatDefOf.NightVisionEfficiency)), 0f);
	}

	public static float ClosestDistBetween(Vector2 origin, Vector2 destination, Vector2 target)
	{
		return Mathf.Abs((destination.y - origin.y) * target.x - (destination.x - origin.x) * target.y + destination.x * origin.y - destination.y * origin.x) / (destination - origin).magnitude;
	}

	public static float DistanceBetweenTiles(int firstTile, int endTile, int maxCells = 500)
	{
		return Find.WorldGrid.TraversalDistanceBetween(firstTile, endTile, passImpassable: true, maxCells);
	}

	public static IntVec3 ExitCell(this Ray ray, Map map)
	{
		Vector3 vector = map.Size.ToVector3();
		vector.y = Mathf.Max(vector.x, vector.z);
		new Bounds(vector.Yto0() / 2f, vector).IntersectRay(ray, out var distance);
		Vector3 point = ray.GetPoint(distance);
		point.x = Mathf.Clamp(point.x, 0f, vector.x - 1f);
		point.z = Mathf.Clamp(point.z, 0f, vector.z - 1f);
		point.y = 0f;
		return point.ToIntVec3();
	}

	public static Pawn TryGetTurretOperator(Thing thing)
	{
		if (thing is Building_Turret)
		{
			CompMannable compMannable = thing.TryGetComp<CompMannable>();
			if (compMannable != null)
			{
				return compMannable.ManningPawn;
			}
		}
		return null;
	}

	public static bool HasShield(this Pawn pawn)
	{
		Pawn_ApparelTracker apparel = pawn.apparel;
		if (apparel == null || apparel.WornApparelCount == 0)
		{
			return false;
		}
		return pawn.apparel.WornApparel.Any((Apparel a) => a is Apparel_Shield);
	}

	public static bool HasTwoWeapon(this Pawn pawn)
	{
		if (pawn.equipment?.Primary == null)
		{
			return false;
		}
		return !(pawn.equipment.Primary.def.weaponTags?.Contains("CE_OneHandedWeapon") ?? false);
	}

	public static bool IsTwoHandedWeapon(this Thing weapon)
	{
		return !(weapon.def.weaponTags?.Contains("CE_OneHandedWeapon") ?? false);
	}

	public static bool HasAmmo(this ThingWithComps gun)
	{
		CompAmmoUser compAmmoUser = gun.TryGetComp<CompAmmoUser>();
		if (compAmmoUser == null)
		{
			return true;
		}
		return !compAmmoUser.UseAmmo || compAmmoUser.CurMagCount > 0 || compAmmoUser.HasAmmo;
	}

	public static bool CanBeStabilized(this Hediff diff)
	{
		if (!(diff is HediffWithComps hediffWithComps))
		{
			return false;
		}
		if (hediffWithComps.BleedRate == 0f || hediffWithComps.IsTended() || hediffWithComps.IsPermanent())
		{
			return false;
		}
		HediffComp_Stabilize hediffComp_Stabilize = hediffWithComps.TryGetComp<HediffComp_Stabilize>();
		return hediffComp_Stabilize != null && !hediffComp_Stabilize.Stabilized;
	}

	public static void Recoil(ThingDef weaponDef, Verb shootVerb, out Vector3 drawOffset, out float angleOffset, float aimAngle, bool handheld)
	{
		drawOffset = Vector3.zero;
		angleOffset = 0f;
		if (shootVerb == null || shootVerb.IsMeleeAttack)
		{
			return;
		}
		float recoilAmount = ((VerbPropertiesCE)weaponDef.verbs[0]).recoilAmount;
		float num = ((weaponDef.verbs[0].burstShotCount > 1) ? ((float)weaponDef.verbs[0].ticksBetweenBurstShots) : (weaponDef.GetStatValueDef(StatDefOf.RangedWeapon_Cooldown) * 20f));
		recoilAmount = Math.Min(recoilAmount * recoilAmount, 20f) * 2.6f * Mathf.Clamp((float)Math.Log10(num), 0.1f, 10f);
		if (num < 2f)
		{
			return;
		}
		Rand.PushState(shootVerb.LastShotTick);
		try
		{
			float num2 = 10f * (float)Math.Log10(recoilAmount) + 3f;
			GunDrawExtension modExtension = weaponDef.GetModExtension<GunDrawExtension>();
			if (modExtension != null)
			{
				recoilAmount *= modExtension.recoilModifier;
				num = ((modExtension.recoilTick > 0) ? ((float)modExtension.recoilTick) : num);
				recoilAmount = ((modExtension.recoilScale > 0f) ? modExtension.recoilScale : recoilAmount);
				num2 *= ((modExtension.muzzleJumpModifier > 0f) ? modExtension.muzzleJumpModifier : 1f);
			}
			if (recoilAmount <= 0f)
			{
				return;
			}
			if (handheld)
			{
				if (weaponDef.weaponTags != null && weaponDef.weaponTags.Contains("CE_OneHandedWeapon"))
				{
					recoilAmount /= 3f;
					num2 *= 1.5f;
				}
				else
				{
					recoilAmount /= 1.3f;
				}
				if (recoilAmount > 15f)
				{
					recoilAmount = 15f;
				}
			}
			int num3 = Find.TickManager.TicksGame - shootVerb.LastShotTick;
			if ((float)num3 < num)
			{
				float num4 = Mathf.Clamp01((float)num3 / num);
				float num5 = Mathf.Lerp(recoilAmount, 0f, num4);
				drawOffset = new Vector3(0f, 0f, 0f - RecoilCurveAxisY.Evaluate(num4)) * num5;
				angleOffset = (float)(handheld ? (-1) : Rand.Sign) * RecoilCurveRotation.Evaluate(num4) * num5 * 0.1f * num2;
				drawOffset = drawOffset.RotatedBy(aimAngle);
				aimAngle += angleOffset;
			}
		}
		finally
		{
			Rand.PopState();
		}
	}

	public static void GenerateAmmoCasings(ProjectilePropertiesCE projProps, Vector3 drawPosition, Map map, float shotRotation = -180f, float recoilAmount = 2f, bool fromPawn = false, GunDrawExtension extension = null, int randSeedOverride = -1)
	{
		if (projProps.dropsCasings)
		{
			if (Controller.settings.ShowCasings)
			{
				ThrowEmptyCasing(drawPosition, map, DefDatabase<FleckDef>.GetNamed(projProps.casingMoteDefname), recoilAmount, shotRotation, 1f, fromPawn, extension, randSeedOverride);
			}
			if (Controller.settings.CreateCasingsFilth)
			{
				MakeCasingFilth(drawPosition.ToIntVec3(), map, DefDatabase<ThingDef>.GetNamed(projProps.casingFilthDefname));
			}
		}
	}

	private static Vector3 CasingOffsetRotated(GunDrawExtension ext, float shotRotation, bool flip)
	{
		if (ext == null || ext.CasingOffset == Vector2.zero)
		{
			return Vector3.zero;
		}
		return (new Vector3(flip ? (0f - ext.CasingOffset.x) : ext.CasingOffset.x, 0f, ext.CasingOffset.y) + RandomOriginOffset(ext.CasingOffsetRandomRange)).RotatedBy(shotRotation);
	}

	private static Vector3 RandomOriginOffset(Vector2 randRange)
	{
		if (randRange.y == 0f && randRange.x == 0f)
		{
			return Vector3.zero;
		}
		return new Vector3(Rand.Range(0f - randRange.x, randRange.x), 0f, Rand.Range(0f - randRange.y, randRange.y));
	}

	public static void ThrowEmptyCasing(Vector3 loc, Map map, FleckDef casingFleckDef, float recoilAmount, float shotRotation, float size = 1f, bool fromPawn = false, GunDrawExtension extension = null, int randSeedOverride = -1)
	{
		if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
		{
			return;
		}
		if (recoilAmount <= 0f)
		{
			recoilAmount = 1f;
		}
		if (randSeedOverride > 0)
		{
			Rand.PushState(randSeedOverride);
		}
		else
		{
			Rand.PushState();
		}
		FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, casingFleckDef);
		dataStatic.velocitySpeed = Rand.Range(1.5f, 2f) * recoilAmount;
		dataStatic.airTimeLeft = Rand.Range(1f, 1.5f) / dataStatic.velocitySpeed;
		if (dataStatic.airTimeLeft > 1.5f)
		{
			dataStatic.airTimeLeft = Rand.Range(1f, 1.5f);
		}
		dataStatic.scale = Rand.Range(0.5f, 0.3f) * size;
		dataStatic.spawnPosition = loc;
		int num = Rand.Range(-20, 20);
		bool flag = false;
		if (fromPawn && shotRotation > 200f && shotRotation < 340f)
		{
			flag = true;
		}
		dataStatic.rotation = (flag ? (shotRotation + 90f) : (shotRotation + 90f)) + Rand.Range(-3f, 4f);
		dataStatic.rotationRate = (float)Rand.Range(-150, 150) / recoilAmount;
		float num2 = 0f;
		if (extension != null)
		{
			if (fromPawn)
			{
				dataStatic.spawnPosition += RandomOriginOffset(extension.CasingOffsetRandomRange).RotatedBy(shotRotation);
			}
			else
			{
				dataStatic.spawnPosition += CasingOffsetRotated(extension, shotRotation, flag);
			}
			num2 = extension.CasingAngleOffset;
			if (extension.AdvancedCasingVariables)
			{
				num2 += extension.CasingAngleOffsetRange.RandomInRange;
				if (extension.CasingLifeTimeOverrideRange.min > 0f)
				{
					dataStatic.airTimeLeft = extension.CasingLifeTimeOverrideRange.RandomInRange;
				}
				else if (extension.CasingLifeTimeMultiplier > 0f)
				{
					dataStatic.airTimeLeft *= extension.CasingLifeTimeMultiplier;
				}
				if (extension.CasingSpeedOverrideRange.min > 0f)
				{
					dataStatic.velocitySpeed = extension.CasingSpeedOverrideRange.RandomInRange;
				}
				else if (extension.CasingSpeedMultiplier > 0f)
				{
					dataStatic.velocitySpeed *= extension.CasingSpeedMultiplier;
				}
			}
			dataStatic.scale *= extension.CasingSizeOffset;
			dataStatic.rotation += Rand.Range(0f - extension.CasingRotationRandomRange, extension.CasingRotationRandomRange);
		}
		dataStatic.velocityAngle = (flag ? (shotRotation - 90f - num2 + (float)num) : (shotRotation + 90f + num2 + (float)num));
		map.flecks.CreateFleck(dataStatic);
		Rand.PopState();
	}

	public static void MakeCasingFilth(IntVec3 position, Map map, ThingDef casingFilthDef)
	{
		Rand.PushState();
		float num = Rand.Range(0f, 1f);
		if (num > 0.9f && position.Walkable(map))
		{
			FilthMaker.TryMakeFilth(position, map, casingFilthDef);
		}
		Rand.PopState();
	}

	public static void MakeIconOverlay(Pawn pawn, ThingDef moteDef)
	{
		if (pawn.Map != null)
		{
			Verse.MoteThrownAttached moteThrownAttached = (Verse.MoteThrownAttached)ThingMaker.MakeThing(moteDef);
			moteThrownAttached.Attach(pawn);
			moteThrownAttached.exactPosition = pawn.DrawPos;
			moteThrownAttached.Scale = 1f;
			moteThrownAttached.SetVelocity(Rand.Range(20f, 25f), 0.4f);
			GenSpawn.Spawn(moteThrownAttached, pawn.Position, pawn.Map);
		}
	}

	public static Bounds GetBoundsFor(IntVec3 cell, RoofDef roof)
	{
		if (roof == null)
		{
			return default(Bounds);
		}
		float num = 2f;
		if (roof.isNatural)
		{
			num *= 2f;
		}
		if (roof.isThickRoof)
		{
			num *= 2f;
		}
		num = Mathf.Max(0.1f, num - 2f);
		Vector3 center = cell.ToVector3Shifted();
		center.y = 2f + num / 2f;
		return new Bounds(center, new Vector3(1f, num, 1f));
	}

	public static Bounds GetBoundsFor(Thing thing)
	{
		if (thing == null)
		{
			return default(Bounds);
		}
		CollisionVertical collisionVertical = new CollisionVertical(thing);
		Vector3 drawPos = thing.DrawPos;
		drawPos.y = collisionVertical.Max - collisionVertical.HeightRange.Span / 2f;
		float x;
		float num;
		if (thing is Building)
		{
			IntVec2 rotatedSize = thing.RotatedSize;
			x = rotatedSize.x;
			num = rotatedSize.z;
		}
		else
		{
			num = GetCollisionWidth(thing);
			x = num;
		}
		Bounds result = new Bounds(drawPos, new Vector3(x, collisionVertical.HeightRange.Span, num));
		return result;
	}

	public static float GetCollisionWidth(Thing thing)
	{
		if (thing is Pawn pawn)
		{
			return GetCollisionBodyFactors(pawn).x;
		}
		return 1f;
	}

	public static bool IntersectionPoint(Vector3 p1, Vector3 p2, Vector3 center, float radius, out Vector3[] sect, bool catchOutbound = true, bool spherical = false, Map map = null)
	{
		sect = new Vector3[2];
		float num = radius * radius;
		Vector3 vector = p1 - center;
		Vector3 vector2 = p2 - center;
		if (!spherical)
		{
			vector.y = 0f;
			vector2.y = 0f;
		}
		float sqrMagnitude = vector.sqrMagnitude;
		float sqrMagnitude2 = vector2.sqrMagnitude;
		Vector3 vector3;
		float sqrMagnitude3;
		if (sqrMagnitude < num)
		{
			if (!catchOutbound || sqrMagnitude2 < num)
			{
				return false;
			}
			vector3 = vector2 - vector;
			sqrMagnitude3 = vector3.sqrMagnitude;
		}
		else
		{
			vector3 = vector2 - vector;
			sqrMagnitude3 = vector3.sqrMagnitude;
			if (sqrMagnitude2 > num)
			{
				float num2 = Mathf.Sqrt(sqrMagnitude3);
				Vector3 vector4 = vector3 / num2;
				float num3 = (0f - vector.x) * vector4.x - vector.y * vector4.y - vector.z * vector4.z;
				if (num3 <= 0f || num3 >= num2)
				{
					return false;
				}
				if ((vector + num3 / num2 * vector3).sqrMagnitude > num)
				{
					return false;
				}
			}
		}
		float num4 = sqrMagnitude3;
		float num5 = 2f * (vector3.x * vector.x + vector3.y * vector.y + vector3.z * vector.z);
		float num6 = sqrMagnitude - num;
		float num7 = num5 * num5 - 4f * num4 * num6;
		if (num7 < 0f)
		{
			Log.Error("Det < 0, but we crossed the radius of the circle");
			return false;
		}
		float num8 = Mathf.Sqrt(num7);
		float num9 = (0f - num5 + num8) / (2f * num4);
		float num10 = (0f - num5 - num8) / (2f * num4);
		sect[0] = new Vector3(p1.x + num9 * vector3.x, p1.y + num9 * vector3.y, p1.z + num9 * vector3.z);
		sect[1] = new Vector3(p1.x + num10 * vector3.x, p1.y + num10 * vector3.y, p1.z + num10 * vector3.z);
		return true;
	}

	public static Vector2 GetCollisionBodyFactors(Pawn pawn)
	{
		if (pawn == null)
		{
			Log.Error("CE calling GetCollisionBodyHeightFactor with nullPawn");
			return new Vector2(1f, 1f);
		}
		if (Patches.GetCollisionBodyFactors(pawn, out var ret))
		{
			return ret;
		}
		ret = BoundsInjector.ForPawn(pawn);
		if (pawn.GetPosture() != 0 || pawn.Downed)
		{
			RacePropertiesExtensionCE racePropertiesExtensionCE = pawn.def.GetModExtension<RacePropertiesExtensionCE>() ?? new RacePropertiesExtensionCE();
			BodyShapeDef bodyShapeDef = racePropertiesExtensionCE.bodyShape ?? CE_BodyShapeDefOf.Invalid;
			if (bodyShapeDef == CE_BodyShapeDefOf.Invalid)
			{
				Log.ErrorOnce("CE returning BodyType Undefined for pawn " + pawn.ToString(), 35000198 + pawn.GetHashCode());
			}
			ret.x *= bodyShapeDef.widthLaying / bodyShapeDef.width;
			ret.y *= bodyShapeDef.heightLaying / bodyShapeDef.height;
			if (pawn.Downed)
			{
				ret.y *= bodyShapeDef.heightLaying;
			}
		}
		return ret;
	}

	public static bool IsCrouching(this Pawn pawn)
	{
		return pawn.RaceProps.Humanlike && !pawn.Downed && pawn.CurJob?.def.GetModExtension<JobDefExtensionCE>()?.isCrouchJob == true;
	}

	public static bool IsPlant(this Thing thing)
	{
		return thing.def.category == ThingCategory.Plant;
	}

	public static float MaxProjectileRange(float shotHeight, float shotSpeed, float shotAngle, float gravityFactor)
	{
		if (shotHeight < 0.001f)
		{
			return Mathf.Pow(shotSpeed, 2f) / gravityFactor * Mathf.Sin(2f * shotAngle);
		}
		return shotSpeed * Mathf.Cos(shotAngle) / gravityFactor * (shotSpeed * Mathf.Sin(shotAngle) + Mathf.Sqrt(Mathf.Pow(shotSpeed * Mathf.Sin(shotAngle), 2f) + 2f * gravityFactor * shotHeight));
	}

	public static void TryUpdateInventory(Pawn pawn)
	{
		pawn?.TryGetComp<CompInventory>()?.UpdateInventory();
	}

	public static void TryUpdateInventory(ThingOwner owner)
	{
		if (owner?.Owner?.ParentHolder is Pawn pawn)
		{
			TryUpdateInventory(pawn);
		}
	}

	public static bool TryGetAllWeaponsInInventory(this Pawn_InventoryTracker inventoryTracker, out IEnumerable<ThingWithComps> weapons, Func<ThingWithComps, bool> predicate = null, bool rebuildInvetory = false)
	{
		Pawn pawn = inventoryTracker.pawn;
		weapons = null;
		CompInventory compInventory = pawn.TryGetComp<CompInventory>();
		if (compInventory == null)
		{
			return false;
		}
		if (rebuildInvetory)
		{
			compInventory.UpdateInventory();
		}
		weapons = ((predicate == null) ? compInventory.weapons : compInventory.weapons.Where((ThingWithComps w) => predicate(w)));
		return true;
	}

	public static float LightingRangeMultiplier(float range)
	{
		return lightingCurve.Evaluate(range);
	}

	public static LightingTracker GetLightingTracker(this Map map)
	{
		int num = map?.Index ?? (-1);
		if (num < 0)
		{
			return null;
		}
		if (num >= _mapsLighting.Length)
		{
			int num2 = Mathf.Max(_mapsLighting.Length * 2, num + 1);
			Map[] array = new Map[num2];
			LightingTracker[] array2 = new LightingTracker[num2];
			Array.Copy(_mapsLighting, array, _mapsLighting.Length);
			Array.Copy(_lightingTrackers, array2, _lightingTrackers.Length);
			_mapsLighting = array;
			_lightingTrackers = array2;
		}
		if (_mapsLighting[num] == map)
		{
			return _lightingTrackers[num];
		}
		return _lightingTrackers[num] = (_mapsLighting[num] = map).GetComponent<LightingTracker>();
	}

	public static DangerTracker GetDangerTracker(this Map map)
	{
		int num = map?.Index ?? (-1);
		if (num < 0)
		{
			return null;
		}
		if (num >= _mapsDanger.Length)
		{
			int num2 = Mathf.Max(_mapsDanger.Length * 2, num + 1);
			Map[] array = new Map[num2];
			DangerTracker[] array2 = new DangerTracker[num2];
			Array.Copy(_mapsDanger, array, _mapsDanger.Length);
			Array.Copy(_DangerTrackers, array2, _DangerTrackers.Length);
			_mapsDanger = array;
			_DangerTrackers = array2;
		}
		if (_mapsDanger[num] == map)
		{
			return _DangerTrackers[num];
		}
		return _DangerTrackers[num] = (_mapsDanger[num] = map).GetComponent<DangerTracker>();
	}

	static CE_Utility()
	{
		allWeaponDefs = new List<ThingDef>();
		RecoilCurveAxisY = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(1f, 0.03f),
			new CurvePoint(2f, 0.04f)
		};
		RecoilCurveRotation = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(1f, 2.5f),
			new CurvePoint(2f, 3f)
		};
		_mapsLighting = new Map[20];
		_lightingTrackers = new LightingTracker[20];
		_mapsDanger = new Map[20];
		_DangerTrackers = new DangerTracker[20];
		lightingCurve = new SimpleCurve();
		_validPawnKinds = new List<PawnKindDef>();
		lightingCurve.Add(5f, 0.05f);
		lightingCurve.Add(10f, 0.15f);
		lightingCurve.Add(22f, 0.475f);
		lightingCurve.Add(35f, 1f);
		lightingCurve.Add(60f, 1.2f);
		lightingCurve.Add(90f, 2f);
	}

	public static float DistanceToSegment(this Vector3 point, Vector3 lineStart, Vector3 lineEnd, out Vector3 closest)
	{
		float num = lineEnd.x - lineStart.x;
		float num2 = lineEnd.z - lineStart.z;
		if (num == 0f && num2 == 0f)
		{
			closest = lineStart;
			num = point.x - lineStart.x;
			num2 = point.z - lineStart.z;
			return Mathf.Sqrt(num * num + num2 * num2);
		}
		float num3 = ((point.x - lineStart.x) * num + (point.z - lineStart.z) * num2) / (num * num + num2 * num2);
		if (num3 < 0f)
		{
			closest = new Vector3(lineStart.x, 0f, lineStart.z);
			num = point.x - lineStart.x;
			num2 = point.z - lineStart.z;
		}
		else if (num3 > 1f)
		{
			closest = new Vector3(lineEnd.x, 0f, lineEnd.z);
			num = point.x - lineEnd.x;
			num2 = point.z - lineEnd.z;
		}
		else
		{
			closest = new Vector3(lineStart.x + num3 * num, 0f, lineStart.z + num3 * num2);
			num = point.x - closest.x;
			num2 = point.z - closest.z;
		}
		return Mathf.Sqrt(num * num + num2 * num2);
	}

	public static Vector3 ToVec3Gridified(this Vector3 originalVec3)
	{
		Vector2 vector = new Vector2(originalVec3.normalized.x, originalVec3.normalized.z);
		float num = Math.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
		if (num <= 0.6f)
		{
			return originalVec3;
		}
		return new Vector3(originalVec3.x / num, originalVec3.y, originalVec3.z / num);
	}

	public static object LaunchProjectileCE(ThingDef projectileDef, Vector2 origin, LocalTargetInfo target, Thing shooter, float shotAngle, float shotRotation, float shotHeight, float shotSpeed)
	{
		projectileDef = projectileDef.GetProjectile();
		ProjectileCE projectileCE = (ProjectileCE)ThingMaker.MakeThing(projectileDef);
		GenSpawn.Spawn(projectileCE, shooter.Position, shooter.Map);
		projectileCE.ExactPosition = origin;
		projectileCE.canTargetSelf = false;
		projectileCE.minCollisionDistance = 1f;
		projectileCE.intendedTarget = target;
		projectileCE.mount = null;
		projectileCE.AccuracyFactor = 1f;
		projectileCE.Launch(shooter, origin, shotAngle, shotRotation, shotHeight, shotSpeed, shooter);
		return projectileCE;
	}

	public static ThingDef GetProjectile(this ThingDef thingDef)
	{
		if (thingDef.projectile != null)
		{
			return thingDef;
		}
		if (thingDef is AmmoDef ammoDef)
		{
			ThingDef thingDef2;
			if ((thingDef2 = ammoDef.Users.FirstOrFallback()) == null)
			{
				return ammoDef.detonateProjectile;
			}
			CompProperties_AmmoUser compProperties = thingDef2.GetCompProperties<CompProperties_AmmoUser>();
			AmmoSetDef ammoSet = compProperties.ammoSet;
			AmmoLink ammoLink;
			if ((ammoLink = ammoSet.ammoTypes.FirstOrFallback()) != null)
			{
				return ammoLink.projectile;
			}
		}
		return thingDef;
	}

	public static void DamageOutsideSquishy(DamageWorker_AddInjury __instance, DamageInfo dinfo, Pawn pawn, float totalDamage, DamageWorker.DamageResult result, float lastHitPartHealth)
	{
		BodyPartRecord hitPart = dinfo.HitPart;
		if (hitPart.depth != BodyPartDepth.Outside || dinfo.Def == DamageDefOf.SurgicalCut || dinfo.Def == DamageDefOf.ExecutionCut || !hitPart.def.tags.Contains(CE_BodyPartTagDefOf.OutsideSquishy))
		{
			return;
		}
		BodyPartRecord parent = hitPart.parent;
		if (parent == null)
		{
			return;
		}
		if (lastHitPartHealth > totalDamage)
		{
			return;
		}
		dinfo.SetHitPart(parent);
		float partHealth = pawn.health.hediffSet.GetPartHealth(parent);
		if (!(partHealth <= 0f) && !(parent.coverageAbs <= 0f))
		{
			Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(HealthUtility.GetHediffDefFromDamage(dinfo.Def, pawn, parent), pawn);
			hediff_Injury.Part = parent;
			hediff_Injury.sourceDef = dinfo.Weapon;
			hediff_Injury.sourceBodyPartGroup = dinfo.WeaponBodyPartGroup;
			hediff_Injury.Severity = totalDamage - lastHitPartHealth * lastHitPartHealth / totalDamage;
			if (hediff_Injury.Severity <= 0f)
			{
				hediff_Injury.Severity = 1f;
			}
			__instance.FinalizeAndAddInjury(pawn, hediff_Injury, dinfo, result);
		}
	}

	public static Pawn GetRandomWorldPawn(this Faction faction, bool capableOfCombat = true)
	{
		Pawn pawn = Find.World.worldPawns.AllPawnsAlive.Where((Pawn p) => p.Faction == faction && (!capableOfCombat || p.kindDef.isFighter || p.kindDef.isGoodBreacher)).RandomElementWithFallback();
		if (pawn != null)
		{
			return pawn;
		}
		Log.Warning($"CE: Couldn't find world pawns for faction {faction}. CE had to create a new one..");
		_validPawnKinds.Clear();
		IEnumerable<PawnGroupMaker> enumerable;
		if (!capableOfCombat)
		{
			IEnumerable<PawnGroupMaker> pawnGroupMakers = faction.def.pawnGroupMakers;
			enumerable = pawnGroupMakers;
		}
		else
		{
			enumerable = faction.def.pawnGroupMakers.Where((PawnGroupMaker x) => x.kindDef == PawnGroupKindDefOf.Combat);
		}
		foreach (PawnGroupMaker item in enumerable)
		{
			foreach (PawnGenOption option in item.options)
			{
				_validPawnKinds.Add(option.kind);
			}
		}
		if (faction.def.fixedLeaderKinds != null)
		{
			_validPawnKinds.AddRange(faction.def.fixedLeaderKinds);
		}
		if (_validPawnKinds.TryRandomElement(out var result))
		{
			PawnGenerationRequest request = new PawnGenerationRequest(result, faction, PawnGenerationContext.NonPlayer, -1, faction.def.leaderForceGenerateNewPawn, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null);
			Gender supremeGender = faction.ideos.PrimaryIdeo.SupremeGender;
			if (supremeGender != 0)
			{
				request.FixedGender = supremeGender;
			}
			pawn = PawnGenerator.GeneratePawn(request);
			if (pawn.RaceProps.IsFlesh)
			{
				pawn.relations.everSeenByPlayer = true;
			}
			if (!Find.WorldPawns.Contains(pawn))
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
			}
		}
		return pawn;
	}

	public static object LaunchProjectileCE(ThingDef projectileDef, ThingDef _ammoDef, Def _ammosetDef, Vector2 origin, LocalTargetInfo target, Pawn launcher, float shotAngle, float shotRotation, float shotHeight, float shotSpeed)
	{
		if (_ammoDef is AmmoDef ammoDef && _ammosetDef is AmmoSetDef ammoSetDef)
		{
			foreach (AmmoLink ammoType in ammoSetDef.ammoTypes)
			{
				if (ammoType.ammo == ammoDef)
				{
					projectileDef = ammoType.projectile;
					break;
				}
			}
		}
		else
		{
			projectileDef = projectileDef.GetProjectile();
		}
		Thing thing = ThingMaker.MakeThing(projectileDef);
		ProjectileCE projectileCE = (ProjectileCE)thing;
		GenSpawn.Spawn(projectileCE, launcher.Position, launcher.Map);
		projectileCE.ExactPosition = origin;
		projectileCE.canTargetSelf = false;
		projectileCE.minCollisionDistance = 1f;
		projectileCE.intendedTarget = target;
		projectileCE.mount = null;
		projectileCE.AccuracyFactor = 1f;
		ProjectilePropertiesCE projectilePropertiesCE = projectileDef.projectile as ProjectilePropertiesCE;
		bool flag = false;
		float spreadDegrees = 0f;
		float aperatureSize = 0.03f;
		VerbPropertiesCE verbProps = new VerbPropertiesCE
		{
			range = 1000f
		};
		if (projectilePropertiesCE != null)
		{
			flag = projectilePropertiesCE.isInstant;
		}
		if (flag)
		{
			projectileCE.RayCast(launcher, verbProps, origin, shotAngle, shotRotation, shotHeight, shotSpeed, spreadDegrees, aperatureSize, launcher);
		}
		else
		{
			projectileCE.Launch(launcher, origin, shotAngle, shotRotation, shotHeight, shotSpeed, launcher);
		}
		return projectileCE;
	}

	public static FactionStrengthTracker GetStrengthTracker(this Faction faction)
	{
		return Find.World.GetComponent<WorldStrengthTracker>().GetFactionTracker(faction);
	}
}
