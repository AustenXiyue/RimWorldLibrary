namespace System.Runtime.InteropServices.ComTypes;

/// <summary>Provides a managed definition of the ITypeLib2 interface.</summary>
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("00020411-0000-0000-C000-000000000046")]
public interface ITypeLib2 : ITypeLib
{
	/// <summary>Returns the number of type descriptions in the type library.</summary>
	/// <returns>The number of type descriptions in the type library.</returns>
	[PreserveSig]
	new int GetTypeInfoCount();

	/// <summary>Retrieves the specified type description in the library.</summary>
	/// <param name="index">An index of the ITypeInfo interface to return. </param>
	/// <param name="ppTI">When this method returns, contains an ITypeInfo describing the type referenced by <paramref name="index" />. This parameter is passed uninitialized. </param>
	new void GetTypeInfo(int index, out ITypeInfo ppTI);

	/// <summary>Retrieves the type of a type description.</summary>
	/// <param name="index">The index of the type description within the type library. </param>
	/// <param name="pTKind">When this method returns, contains a reference to the TYPEKIND enumeration for the type description. This parameter is passed uninitialized.</param>
	new void GetTypeInfoType(int index, out TYPEKIND pTKind);

	/// <summary>Retrieves the type description that corresponds to the specified GUID.</summary>
	/// <param name="guid">The <see cref="T:System.Guid" />, passed by reference, that represents the IID of the CLSID interface of the class whose type info is requested. </param>
	/// <param name="ppTInfo">When this method returns, contains the requested ITypeInfo interface. This parameter is passed uninitialized.</param>
	new void GetTypeInfoOfGuid(ref Guid guid, out ITypeInfo ppTInfo);

	/// <summary>Retrieves the structure that contains the library's attributes.</summary>
	/// <param name="ppTLibAttr">When this method returns, contains a structure that contains the library's attributes. This parameter is passed uninitialized. </param>
	new void GetLibAttr(out IntPtr ppTLibAttr);

	/// <summary>Enables a client compiler to bind to a library's types, variables, constants, and global functions.</summary>
	/// <param name="ppTComp">When this method returns, contains an ITypeComp instance for this ITypeLib. This parameter is passed uninitialized. </param>
	new void GetTypeComp(out ITypeComp ppTComp);

	/// <summary>Retrieves the library's documentation string, the complete Help file name and path, and the context identifier for the library Help topic in the Help file.</summary>
	/// <param name="index">An index of the type description whose documentation is to be returned. </param>
	/// <param name="strName">When this method returns, contains a string that specifies the name of the specified item. This parameter is passed uninitialized. </param>
	/// <param name="strDocString">When this method returns, contains the documentation string for the specified item. This parameter is passed uninitialized.</param>
	/// <param name="dwHelpContext">When this method returns, contains the Help context identifier associated with the specified item. This parameter is passed uninitialized. </param>
	/// <param name="strHelpFile">When this method returns, contains a string that specifies the fully qualified name of the Help file. This parameter is passed uninitialized.</param>
	new void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);

	/// <summary>Indicates whether a passed-in string contains the name of a type or member described in the library.</summary>
	/// <returns>true if <paramref name="szNameBuf" /> was found in the type library; otherwise, false.</returns>
	/// <param name="szNameBuf">The string to test. </param>
	/// <param name="lHashVal">The hash value of <paramref name="szNameBuf" />. </param>
	[return: MarshalAs(UnmanagedType.Bool)]
	new bool IsName([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal);

	/// <summary>Finds occurrences of a type description in a type library.</summary>
	/// <param name="szNameBuf">The name to search for. </param>
	/// <param name="lHashVal">A hash value to speed up the search, computed by the LHashValOfNameSys function. If <paramref name="lHashVal" /> is 0, a value is computed. </param>
	/// <param name="ppTInfo">When this method returns, contains an array of pointers to the type descriptions that contain the name specified in <paramref name="szNameBuf" />. This parameter is passed uninitialized. </param>
	/// <param name="rgMemId">When this method returns, contains an array of the MEMBERIDs of the found items; <paramref name="rgMemId" /> [i] is the MEMBERID that indexes into the type description specified by <paramref name="ppTInfo" /> [i]. This parameter cannot be null. This parameter is passed uninitialized. </param>
	/// <param name="pcFound">On entry, a value, passed by reference, that indicates how many instances to look for. For example, <paramref name="pcFound" /> = 1 can be called to find the first occurrence. The search stops when one instance is found.On exit, indicates the number of instances that were found. If the in and out values of <paramref name="pcFound" /> are identical, there might be more type descriptions that contain the name. </param>
	new void FindName([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal, [Out][MarshalAs(UnmanagedType.LPArray)] ITypeInfo[] ppTInfo, [Out][MarshalAs(UnmanagedType.LPArray)] int[] rgMemId, ref short pcFound);

	/// <summary>Releases the <see cref="T:System.Runtime.InteropServices.TYPELIBATTR" /> structure originally obtained from the <see cref="M:System.Runtime.InteropServices.ComTypes.ITypeLib.GetLibAttr(System.IntPtr@)" /> method.</summary>
	/// <param name="pTLibAttr">The TLIBATTR structure to release. </param>
	[PreserveSig]
	new void ReleaseTLibAttr(IntPtr pTLibAttr);

	/// <summary>Gets the custom data.</summary>
	/// <param name="guid">A <see cref="T:System.Guid" /> , passed by reference, that is used to identify the data. </param>
	/// <param name="pVarVal">When this method returns, contains an object that specifies where to put the retrieved data. This parameter is passed uninitialized.</param>
	void GetCustData(ref Guid guid, out object pVarVal);

	/// <summary>Retrieves the library's documentation string, the complete Help file name and path, the localization context to use, and the context ID for the library Help topic in the Help file.</summary>
	/// <param name="index">An index of the type description whose documentation is to be returned; if <paramref name="index" /> is -1, the documentation for the library is returned. </param>
	/// <param name="pbstrHelpString">When this method returns, contains a BSTR that specifies the name of the specified item. If the caller does not need the item name, <paramref name="pbstrHelpString" /> can be null. This parameter is passed uninitialized. </param>
	/// <param name="pdwHelpStringContext">When this method returns, contains the Help localization context. If the caller does not need the Help context, <paramref name="pdwHelpStringContext" /> can be null. This parameter is passed uninitialized. </param>
	/// <param name="pbstrHelpStringDll">When this method returns, contains a BSTR that specifies the fully qualified name of the file containing the DLL used for Help file. If the caller does not need the file name, <paramref name="pbstrHelpStringDll" /> can be null. This parameter is passed uninitialized.</param>
	[LCIDConversion(1)]
	void GetDocumentation2(int index, out string pbstrHelpString, out int pdwHelpStringContext, out string pbstrHelpStringDll);

	/// <summary>Returns statistics about a type library that are required for efficient sizing of hash tables.</summary>
	/// <param name="pcUniqueNames">A pointer to a count of unique names. If the caller does not need this information, set to null. </param>
	/// <param name="pcchUniqueNames">When this method returns, contains a pointer to a change in the count of unique names. This parameter is passed uninitialized. </param>
	void GetLibStatistics(IntPtr pcUniqueNames, out int pcchUniqueNames);

	/// <summary>Gets all custom data items for the library.</summary>
	/// <param name="pCustData">A pointer to CUSTDATA, which holds all custom data items. </param>
	void GetAllCustData(IntPtr pCustData);
}
