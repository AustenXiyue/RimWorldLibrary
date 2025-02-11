using System.Windows.Threading;

namespace System.Windows.Input;

/// <summary>Abstract class that describes an input devices. </summary>
public abstract class InputDevice : DispatcherObject
{
	/// <summary>When overridden in a derived class, gets the element that receives input from this device.</summary>
	/// <returns>The element that receives input.</returns>
	public abstract IInputElement Target { get; }

	/// <summary>When overridden in a derived class, gets the <see cref="T:System.Windows.PresentationSource" /> that is reporting input for this device.</summary>
	/// <returns>The source that is reporting input for this device.</returns>
	public abstract PresentationSource ActiveSource { get; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputDevice" /> class. </summary>
	protected InputDevice()
	{
	}
}
