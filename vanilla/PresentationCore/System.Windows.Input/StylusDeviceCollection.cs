using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Input;

/// <summary>Contains the <see cref="T:System.Windows.Input.StylusDevice" /> objects that represent a Tablet PC's stylus devices.</summary>
public class StylusDeviceCollection : ReadOnlyCollection<StylusDevice>
{
	internal StylusDeviceCollection(IEnumerable<StylusDeviceBase> styluses)
		: base((IList<StylusDevice>)new List<StylusDevice>())
	{
		foreach (StylusDeviceBase styluse in styluses)
		{
			base.Items.Add(styluse.StylusDevice);
		}
	}

	internal void Dispose()
	{
		foreach (StylusDevice item in base.Items)
		{
			item.StylusDeviceImpl.Dispose();
		}
	}

	internal void AddStylusDevice(int index, StylusDeviceBase stylusDevice)
	{
		base.Items.Insert(index, stylusDevice.StylusDevice);
	}
}
