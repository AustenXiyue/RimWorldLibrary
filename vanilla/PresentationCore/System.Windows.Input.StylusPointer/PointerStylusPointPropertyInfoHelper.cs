using System.Collections.Generic;
using MS.Win32.Pointer;

namespace System.Windows.Input.StylusPointer;

internal class PointerStylusPointPropertyInfoHelper
{
	private const byte HidExponentMask = 15;

	private static Dictionary<byte, short> _hidExponentMap = new Dictionary<byte, short>
	{
		{ 5, 5 },
		{ 6, 6 },
		{ 7, 7 },
		{ 8, -8 },
		{ 9, -7 },
		{ 10, -6 },
		{ 11, -5 },
		{ 12, -4 },
		{ 13, -3 },
		{ 14, -2 },
		{ 15, -1 }
	};

	internal static StylusPointPropertyInfo CreatePropertyInfo(UnsafeNativeMethods.POINTER_DEVICE_PROPERTY prop)
	{
		StylusPointPropertyInfo result = null;
		Guid knownGuid = StylusPointPropertyIds.GetKnownGuid((StylusPointPropertyIds.HidUsagePage)prop.usagePageId, (StylusPointPropertyIds.HidUsage)prop.usageId);
		if (knownGuid != Guid.Empty)
		{
			StylusPointProperty stylusPointProperty = new StylusPointProperty(knownGuid, StylusPointPropertyIds.IsKnownButton(knownGuid));
			StylusPointPropertyUnit? stylusPointPropertyUnit = StylusPointPropertyUnitHelper.FromPointerUnit(prop.unit);
			if (!stylusPointPropertyUnit.HasValue)
			{
				stylusPointPropertyUnit = StylusPointPropertyInfoDefaults.GetStylusPointPropertyInfoDefault(stylusPointProperty).Unit;
			}
			float resolution = StylusPointPropertyInfoDefaults.GetStylusPointPropertyInfoDefault(stylusPointProperty).Resolution;
			short value = 0;
			if (_hidExponentMap.TryGetValue((byte)(prop.unitExponent & 0xF), out value))
			{
				float num = (float)Math.Pow(10.0, value);
				if (prop.physicalMax - prop.physicalMin > 0)
				{
					resolution = (float)(prop.logicalMax - prop.logicalMin) / ((float)(prop.physicalMax - prop.physicalMin) * num);
				}
			}
			result = new StylusPointPropertyInfo(stylusPointProperty, prop.logicalMin, prop.logicalMax, stylusPointPropertyUnit.Value, resolution);
		}
		return result;
	}
}
