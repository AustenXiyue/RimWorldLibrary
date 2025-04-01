namespace System.Runtime.InteropServices.ComTypes;

/// <summary>Provides a managed definition of the IAdviseSink interface.</summary>
[ComImport]
[Guid("0000010F-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IAdviseSink
{
	/// <summary>Notifies all registered advisory sinks that the object has changed from the running state to the loaded state.  This method is called by a server.</summary>
	[PreserveSig]
	void OnClose();

	/// <summary>Notifies all data objects currently registered advisory sinks that data in the object has changed.</summary>
	/// <param name="format">A <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" />, passed by reference, which describes the format, target device, rendering, and storage information of the calling data object.</param>
	/// <param name="stgmedium">A <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" />, passed by reference, which defines the storage medium (global memory, disk file, storage object, stream object, Graphics Device Interface (GDI) object, or undefined) and ownership of that medium for the calling data object.</param>
	[PreserveSig]
	void OnDataChange([In] ref FORMATETC format, [In] ref STGMEDIUM stgmedium);

	/// <summary>Notifies all registered advisory sinks that the object has been renamed. This method is called by a server.</summary>
	/// <param name="moniker">A pointer to the IMoniker interface on the new full moniker of the object.</param>
	[PreserveSig]
	void OnRename(IMoniker moniker);

	/// <summary>Notifies all registered advisory sinks that the object has been saved. This method is called by a server.</summary>
	[PreserveSig]
	void OnSave();

	/// <summary>Notifies an object's registered advisory sinks that its view has changed. This method is called by a server.</summary>
	/// <param name="aspect">The aspect, or view, of the object. Contains a value taken from the <see cref="T:System.Runtime.InteropServices.ComTypes.DVASPECT" /> enumeration.</param>
	/// <param name="index">The portion of the view that has changed. Currently, only -1 is valid.</param>
	[PreserveSig]
	void OnViewChange(int aspect, int index);
}
