namespace System.Runtime.InteropServices.ComTypes;

/// <summary>Provides the managed definition of the ITypeLib interface.</summary>
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("00020402-0000-0000-C000-000000000046")]
public interface ITypeLib
{
	/// <summary>Returns the number of type descriptions in the type library.</summary>
	/// <returns>The number of type descriptions in the type library.</returns>
	[PreserveSig]
	int GetTypeInfoCount();

	/// <summary>Retrieves the specified type description in the library.</summary>
	/// <param name="index">The index of the ITypeInfo interface to return. </param>
	/// <param name="ppTI">When this method returns, contains an ITypeInfo describing the type referenced by <paramref name="index" />. This parameter is passed uninitialized.</param>
	void GetTypeInfo(int index, out ITypeInfo ppTI);

	/// <summary>Retrieves the type of a type description.</summary>
	/// <param name="index">The index of the type description within the type library. </param>
	/// <param name="pTKind">When this method returns, contains a reference to the TYPEKIND enumeration for the type description. This parameter is passed uninitialized.</param>
	void GetTypeInfoType(int index, out TYPEKIND pTKind);

	/// <summary>Retrieves the type description that corresponds to the specified GUID.</summary>
	/// <param name="guid">The IID of the interface or CLSID of the class whose type info is requested. </param>
	/// <param name="ppTInfo">When this method returns, contains the requested ITypeInfo interface. This parameter is passed uninitialized.</param>
	void GetTypeInfoOfGuid(ref Guid guid, out ITypeInfo ppTInfo);

	/// <summary>Retrieves the structure that contains the library's attributes.</summary>
	/// <param name="ppTLibAttr">When this method returns, contains a structure that contains the library's attributes. This parameter is passed uninitialized.</param>
	void GetLibAttr(out IntPtr ppTLibAttr);

	/// <summary>Enables a client compiler to bind to a library's types, variables, constants, and global functions.</summary>
	/// <param name="ppTComp">When this method returns, contains an instance of a ITypeComp instance for this ITypeLib. This parameter is passed uninitialized. </param>
	void GetTypeComp(out ITypeComp ppTComp);

	/// <summary>Retrieves the library's documentation string, the complete Help file name and path, and the context identifier for the library Help topic in the Help file.</summary>
	/// <param name="index">The index of the type description whose documentation is to be returned. </param>
	/// <param name="strName">When this method returns, contains a string that represents the name of the specified item. This parameter is passed uninitialized.</param>
	/// <param name="strDocString">When this method returns, contains a string that represents the documentation string for the specified item. This parameter is passed uninitialized.</param>
	/// <param name="dwHelpContext">When this method returns, contains the Help context identifier associated with the specified item. This parameter is passed uninitialized.</param>
	/// <param name="strHelpFile">When this method returns, contains a string that represents the fully qualified name of the Help file. This parameter is passed uninitialized.</param>
	void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);

	/// <summary>Indicates whether a passed-in string contains the name of a type or member described in the library.</summary>
	/// <returns>true if <paramref name="szNameBuf" /> was found in the type library; otherwise, false.</returns>
	/// <param name="szNameBuf">The string to test. This is an in/out parameter.</param>
	/// <param name="lHashVal">The hash value of <paramref name="szNameBuf" />. </param>
	[return: MarshalAs(UnmanagedType.Bool)]
	bool IsName([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal);

	/// <summary>Finds occurrences of a type description in a type library.</summary>
	/// <param name="szNameBuf">The name to search for. This is an in/out parameter.</param>
	/// <param name="lHashVal">A hash value to speed up the search, computed by the LHashValOfNameSys function. If <paramref name="lHashVal" /> is 0, a value is computed. </param>
	/// <param name="ppTInfo">When this method returns, contains an array of pointers to the type descriptions that contain the name specified in <paramref name="szNameBuf" />. This parameter is passed uninitialized.</param>
	/// <param name="rgMemId">An array of the MEMBERID 's of the found items; <paramref name="rgMemId" /> [i] is the MEMBERID that indexes into the type description specified by <paramref name="ppTInfo" /> [i]. Cannot be null. </param>
	/// <param name="pcFound">On entry, indicates how many instances to look for. For example, <paramref name="pcFound" /> = 1 can be called to find the first occurrence. The search stops when one instance is found.On exit, indicates the number of instances that were found. If the in and out values of <paramref name="pcFound" /> are identical, there might be more type descriptions that contain the name. </param>
	void FindName([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal, [Out][MarshalAs(UnmanagedType.LPArray)] ITypeInfo[] ppTInfo, [Out][MarshalAs(UnmanagedType.LPArray)] int[] rgMemId, ref short pcFound);

	/// <summary>Releases the <see cref="T:System.Runtime.InteropServices.TYPELIBATTR" /> structure originally obtained from the <see cref="M:System.Runtime.InteropServices.ComTypes.ITypeLib.GetLibAttr(System.IntPtr@)" /> method.</summary>
	/// <param name="pTLibAttr">The TLIBATTR structure to release. </param>
	[PreserveSig]
	void ReleaseTLibAttr(IntPtr pTLibAttr);
}
