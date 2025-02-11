namespace System.Runtime.InteropServices.WindowsRuntime;

/// <summary>Enables classes to be activated by the Windows Runtime. </summary>
[ComImport]
[Guid("00000035-0000-0000-C000-000000000046")]
public interface IActivationFactory
{
	/// <summary>Returns a new instance of the Windows Runtime class that is created by the <see cref="T:System.Runtime.InteropServices.WindowsRuntime.IActivationFactory" /> interface. </summary>
	/// <returns>The new instance of the Windows Runtime class. </returns>
	object ActivateInstance();
}
