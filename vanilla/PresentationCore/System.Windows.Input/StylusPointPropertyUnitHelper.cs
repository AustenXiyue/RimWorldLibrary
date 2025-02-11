using System.Collections.Generic;

namespace System.Windows.Input;

internal static class StylusPointPropertyUnitHelper
{
	private const uint UNIT_MASK = 15u;

	private static Dictionary<uint, StylusPointPropertyUnit> _pointerUnitMap = new Dictionary<uint, StylusPointPropertyUnit>
	{
		{
			1u,
			StylusPointPropertyUnit.Centimeters
		},
		{
			2u,
			StylusPointPropertyUnit.Radians
		},
		{
			3u,
			StylusPointPropertyUnit.Inches
		},
		{
			4u,
			StylusPointPropertyUnit.Degrees
		}
	};

	internal static StylusPointPropertyUnit? FromPointerUnit(uint pointerUnit)
	{
		StylusPointPropertyUnit value = StylusPointPropertyUnit.None;
		_pointerUnitMap.TryGetValue(pointerUnit & 0xF, out value);
		if (!IsDefined(value))
		{
			return null;
		}
		return value;
	}

	internal static bool IsDefined(StylusPointPropertyUnit unit)
	{
		if (unit >= StylusPointPropertyUnit.None && unit <= StylusPointPropertyUnit.Grams)
		{
			return true;
		}
		return false;
	}
}
