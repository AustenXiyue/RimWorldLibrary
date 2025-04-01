namespace System.Runtime.InteropServices.ComTypes;

/// <summary>Specifies the requested behavior when setting up an advise sink or a caching connection with an object.</summary>
[Flags]
public enum ADVF
{
	/// <summary>For data advisory connections (<see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.DAdvise(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.ADVF,System.Runtime.InteropServices.ComTypes.IAdviseSink,System.Int32@)" /> or <see cref="M:System.Runtime.InteropServices.ComTypes.IConnectionPoint.Advise(System.Object,System.Int32@)" />), this flag requests the data object not to send data when it calls <see cref="M:System.Runtime.InteropServices.ComTypes.IAdviseSink.OnDataChange(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)" />. </summary>
	ADVF_NODATA = 1,
	/// <summary>Requests that the object not wait for the data or view to change before making an initial call to <see cref="M:System.Runtime.InteropServices.ComTypes.IAdviseSink.OnDataChange(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)" /> (for data or view advisory connections) or updating the cache (for cache connections).</summary>
	ADVF_PRIMEFIRST = 2,
	/// <summary>Requests that the object make only one change notification or cache update before deleting the connection.</summary>
	ADVF_ONLYONCE = 4,
	/// <summary>Synonym for <see cref="F:System.Runtime.InteropServices.ComTypes.ADVF.ADVFCACHE_FORCEBUILTIN" />, which is used more often.</summary>
	ADVFCACHE_NOHANDLER = 8,
	/// <summary>This value is used by DLL object applications and object handlers that perform the drawing of their objects.</summary>
	ADVFCACHE_FORCEBUILTIN = 0x10,
	/// <summary>For cache connections, this flag updates the cached representation only when the object containing the cache is saved.</summary>
	ADVFCACHE_ONSAVE = 0x20,
	/// <summary>For data advisory connections, assures accessibility to data. </summary>
	ADVF_DATAONSTOP = 0x40
}
