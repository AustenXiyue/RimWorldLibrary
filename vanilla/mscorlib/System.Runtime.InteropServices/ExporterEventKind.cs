namespace System.Runtime.InteropServices;

/// <summary>Describes the callbacks that the type library exporter makes when exporting a type library.</summary>
[Serializable]
[ComVisible(true)]
public enum ExporterEventKind
{
	/// <summary>Specifies that the event is invoked when a type has been exported.</summary>
	NOTIF_TYPECONVERTED,
	/// <summary>Specifies that the event is invoked when a warning occurs during conversion.</summary>
	NOTIF_CONVERTWARNING,
	/// <summary>This value is not supported in this version of the .NET Framework.</summary>
	ERROR_REFTOINVALIDASSEMBLY
}
