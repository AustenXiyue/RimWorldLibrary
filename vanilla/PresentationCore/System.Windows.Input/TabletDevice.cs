using System.Collections.ObjectModel;

namespace System.Windows.Input;

/// <summary>Represents the digitizer device of a Tablet PC. </summary>
public sealed class TabletDevice : InputDevice
{
	internal TabletDeviceBase TabletDeviceImpl { get; set; }

	/// <summary>Gets the <see cref="T:System.Windows.IInputElement" /> that provides basic input processing for the tablet device.</summary>
	/// <returns>The <see cref="T:System.Windows.IInputElement" /> that provides basic input processing for the tablet device.</returns>
	public override IInputElement Target
	{
		get
		{
			VerifyAccess();
			return TabletDeviceImpl.Target;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.PresentationSource" /> that reports current input for the tablet device.</summary>
	/// <returns>The <see cref="T:System.Windows.PresentationSource" /> that reports current input for the tablet device.</returns>
	public override PresentationSource ActiveSource
	{
		get
		{
			VerifyAccess();
			return TabletDeviceImpl.ActiveSource;
		}
	}

	/// <summary>Gets the unique identifier for the tablet device on the system.</summary>
	/// <returns>The unique identifier for the tablet device on the system.</returns>
	public int Id
	{
		get
		{
			VerifyAccess();
			return TabletDeviceImpl.Id;
		}
	}

	/// <summary>Gets the name of the tablet device.</summary>
	/// <returns>The name of the tablet device.</returns>
	public string Name
	{
		get
		{
			VerifyAccess();
			return TabletDeviceImpl.Name;
		}
	}

	/// <summary>Gets the product identifier for the tablet device.</summary>
	/// <returns>The product identifier for the tablet device.</returns>
	public string ProductId
	{
		get
		{
			VerifyAccess();
			return TabletDeviceImpl.ProductId;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Input.TabletHardwareCapabilities" /> for the tablet device.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.TabletHardwareCapabilities" /> for the tablet device.</returns>
	public TabletHardwareCapabilities TabletHardwareCapabilities
	{
		get
		{
			VerifyAccess();
			return TabletDeviceImpl.TabletHardwareCapabilities;
		}
	}

	/// <summary>Gets a collection of <see cref="T:System.Windows.Input.StylusPointProperty" /> objects that the <see cref="T:System.Windows.Input.TabletDevice" /> supports.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Input.StylusPointProperty" /> objects that the <see cref="T:System.Windows.Input.TabletDevice" /> supports.</returns>
	public ReadOnlyCollection<StylusPointProperty> SupportedStylusPointProperties
	{
		get
		{
			VerifyAccess();
			return TabletDeviceImpl.SupportedStylusPointProperties;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Input.TabletDeviceType" /> of the tablet device.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.TabletDeviceType" /> of the tablet device.</returns>
	public TabletDeviceType Type
	{
		get
		{
			VerifyAccess();
			return TabletDeviceImpl.Type;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Input.StylusDeviceCollection" /> associated with the tablet device.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.StylusDeviceCollection" /> associated with the tablet device.</returns>
	public StylusDeviceCollection StylusDevices
	{
		get
		{
			VerifyAccess();
			return TabletDeviceImpl.StylusDevices;
		}
	}

	internal TabletDevice(TabletDeviceBase impl)
	{
		if (impl == null)
		{
			throw new ArgumentNullException("impl");
		}
		TabletDeviceImpl = impl;
	}

	/// <summary>Returns the name of the tablet device.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the name of the <see cref="T:System.Windows.Input.TabletDevice" />.</returns>
	public override string ToString()
	{
		return TabletDeviceImpl.ToString();
	}

	internal T As<T>() where T : TabletDeviceBase
	{
		return TabletDeviceImpl as T;
	}
}
