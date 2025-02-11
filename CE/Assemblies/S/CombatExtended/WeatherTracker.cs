using System;
using UnityEngine;
using Verse;

namespace CombatExtended;

public class WeatherTracker : MapComponent
{
	private static readonly string[] windDirections = new string[8] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

	private const float BaseWindStrength = 3f;

	private const float MaxWindStrength = 9f;

	private const float MaxDirectionDelta = 5f;

	private float _humidity = FireSpread.values.maxHumidity * 0.5f;

	private float _windDirection;

	private float _windDirectionTarget;

	public float Humidity
	{
		get
		{
			return _humidity;
		}
		private set
		{
			_humidity = Mathf.Clamp(value, 0f, FireSpread.values.maxHumidity);
		}
	}

	public float HumidityPercent => Humidity / FireSpread.values.maxHumidity;

	public Vector3 WindDirection => Vector3Utility.FromAngleFlat(_windDirection - 90f);

	private float WindStrength => Mathf.Min(3f * map.windManager.WindSpeed, 9f);

	private int BeaufortScale => Mathf.FloorToInt(WindStrength);

	private string WindStrengthText => ("CE_Wind_Beaufort" + BeaufortScale).Translate();

	private string WindDirectionText
	{
		get
		{
			if (BeaufortScale == 0)
			{
				return "";
			}
			float num = 360f / (float)windDirections.Length;
			int num2 = Mathf.Clamp(Mathf.RoundToInt((_windDirection - num * 0.5f) / num), 0, windDirections.Length - 1);
			return ", " + ("CE_Wind_Direction_" + windDirections[num2]).Translate();
		}
	}

	public WeatherTracker(Map map)
		: base(map)
	{
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref _humidity, "humidity", FireSpread.values.maxHumidity * 0.5f);
		Scribe_Values.Look(ref _windDirection, "windDirection", 0f);
		Scribe_Values.Look(ref _windDirectionTarget, "windDirectionTarget", 0f);
	}

	public float GetWindStrengthAt(IntVec3 cell)
	{
		if (!cell.UsesOutdoorTemperature(map))
		{
			return 0f;
		}
		return WindStrength;
	}

	public override void MapComponentTick()
	{
		base.MapComponentTick();
		if (map.weatherManager.RainRate > 0f)
		{
			Humidity += map.weatherManager.RainRate * FireSpread.values.humidityIncreaseMultiplier;
		}
		else
		{
			Humidity -= FireSpread.values.humidityDecayPerTick;
		}
		if (GenTicks.TicksGame % 250 == 0)
		{
			if (Math.Abs(_windDirection - _windDirectionTarget) < 1f)
			{
				_windDirectionTarget = Rand.Range(0, 360);
			}
			_windDirection = Mathf.MoveTowardsAngle(_windDirection, _windDirectionTarget, Rand.Range(0f, 5f));
		}
	}

	public void DoWindGUI(float xPos, ref float yPos)
	{
		float num = 100f;
		float width = 200f + num;
		float num2 = 26f;
		Rect rect = new Rect(xPos - num, yPos - num2, width, num2);
		Text.Anchor = TextAnchor.MiddleRight;
		rect.width -= 15f;
		Text.Font = GameFont.Small;
		Widgets.Label(rect, WindStrengthText + WindDirectionText);
		TooltipHandler.TipRegion(rect, "CE_Wind_Tooltip".Translate());
		Text.Anchor = TextAnchor.UpperLeft;
		yPos -= num2;
	}
}
