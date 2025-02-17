using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;

namespace System.Diagnostics;

/// <summary>Provides version information for a physical file on disk.</summary>
/// <filterpriority>2</filterpriority>
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
public sealed class FileVersionInfo
{
	private string comments;

	private string companyname;

	private string filedescription;

	private string filename;

	private string fileversion;

	private string internalname;

	private string language;

	private string legalcopyright;

	private string legaltrademarks;

	private string originalfilename;

	private string privatebuild;

	private string productname;

	private string productversion;

	private string specialbuild;

	private bool isdebug;

	private bool ispatched;

	private bool isprerelease;

	private bool isprivatebuild;

	private bool isspecialbuild;

	private int filemajorpart;

	private int fileminorpart;

	private int filebuildpart;

	private int fileprivatepart;

	private int productmajorpart;

	private int productminorpart;

	private int productbuildpart;

	private int productprivatepart;

	/// <summary>Gets the comments associated with the file.</summary>
	/// <returns>The comments associated with the file or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public string Comments => comments;

	/// <summary>Gets the name of the company that produced the file.</summary>
	/// <returns>The name of the company that produced the file or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public string CompanyName => companyname;

	/// <summary>Gets the build number of the file.</summary>
	/// <returns>A value representing the build number of the file or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public int FileBuildPart => filebuildpart;

	/// <summary>Gets the description of the file.</summary>
	/// <returns>The description of the file or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public string FileDescription => filedescription;

	/// <summary>Gets the major part of the version number.</summary>
	/// <returns>A value representing the major part of the version number or 0 (zero) if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public int FileMajorPart => filemajorpart;

	/// <summary>Gets the minor part of the version number of the file.</summary>
	/// <returns>A value representing the minor part of the version number of the file or 0 (zero) if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public int FileMinorPart => fileminorpart;

	/// <summary>Gets the name of the file that this instance of <see cref="T:System.Diagnostics.FileVersionInfo" /> describes.</summary>
	/// <returns>The name of the file described by this instance of <see cref="T:System.Diagnostics.FileVersionInfo" />.</returns>
	/// <filterpriority>2</filterpriority>
	public string FileName => filename;

	/// <summary>Gets the file private part number.</summary>
	/// <returns>A value representing the file private part number or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public int FilePrivatePart => fileprivatepart;

	/// <summary>Gets the file version number.</summary>
	/// <returns>The version number of the file or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public string FileVersion => fileversion;

	/// <summary>Gets the internal name of the file, if one exists.</summary>
	/// <returns>The internal name of the file. If none exists, this property will contain the original name of the file without the extension.</returns>
	/// <filterpriority>2</filterpriority>
	public string InternalName => internalname;

	/// <summary>Gets a value that specifies whether the file contains debugging information or is compiled with debugging features enabled.</summary>
	/// <returns>true if the file contains debugging information or is compiled with debugging features enabled; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsDebug => isdebug;

	/// <summary>Gets a value that specifies whether the file has been modified and is not identical to the original shipping file of the same version number.</summary>
	/// <returns>true if the file is patched; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsPatched => ispatched;

	/// <summary>Gets a value that specifies whether the file is a development version, rather than a commercially released product.</summary>
	/// <returns>true if the file is prerelease; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsPreRelease => isprerelease;

	/// <summary>Gets a value that specifies whether the file was built using standard release procedures.</summary>
	/// <returns>true if the file is a private build; false if the file was built using standard release procedures or if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsPrivateBuild => isprivatebuild;

	/// <summary>Gets a value that specifies whether the file is a special build.</summary>
	/// <returns>true if the file is a special build; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsSpecialBuild => isspecialbuild;

	/// <summary>Gets the default language string for the version info block.</summary>
	/// <returns>The description string for the Microsoft Language Identifier in the version resource or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public string Language => language;

	/// <summary>Gets all copyright notices that apply to the specified file.</summary>
	/// <returns>The copyright notices that apply to the specified file.</returns>
	/// <filterpriority>2</filterpriority>
	public string LegalCopyright => legalcopyright;

	/// <summary>Gets the trademarks and registered trademarks that apply to the file.</summary>
	/// <returns>The trademarks and registered trademarks that apply to the file or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public string LegalTrademarks => legaltrademarks;

	/// <summary>Gets the name the file was created with.</summary>
	/// <returns>The name the file was created with or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public string OriginalFilename => originalfilename;

	/// <summary>Gets information about a private version of the file.</summary>
	/// <returns>Information about a private version of the file or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public string PrivateBuild => privatebuild;

	/// <summary>Gets the build number of the product this file is associated with.</summary>
	/// <returns>A value representing the build number of the product this file is associated with or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public int ProductBuildPart => productbuildpart;

	/// <summary>Gets the major part of the version number for the product this file is associated with.</summary>
	/// <returns>A value representing the major part of the product version number or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public int ProductMajorPart => productmajorpart;

	/// <summary>Gets the minor part of the version number for the product the file is associated with.</summary>
	/// <returns>A value representing the minor part of the product version number or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public int ProductMinorPart => productminorpart;

	/// <summary>Gets the name of the product this file is distributed with.</summary>
	/// <returns>The name of the product this file is distributed with or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public string ProductName => productname;

	/// <summary>Gets the private part number of the product this file is associated with.</summary>
	/// <returns>A value representing the private part number of the product this file is associated with or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public int ProductPrivatePart => productprivatepart;

	/// <summary>Gets the version of the product this file is distributed with.</summary>
	/// <returns>The version of the product this file is distributed with or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public string ProductVersion => productversion;

	/// <summary>Gets the special build information for the file.</summary>
	/// <returns>The special build information for the file or null if the file did not contain version information.</returns>
	/// <filterpriority>2</filterpriority>
	public string SpecialBuild => specialbuild;

	private FileVersionInfo()
	{
		comments = null;
		companyname = null;
		filedescription = null;
		filename = null;
		fileversion = null;
		internalname = null;
		language = null;
		legalcopyright = null;
		legaltrademarks = null;
		originalfilename = null;
		privatebuild = null;
		productname = null;
		productversion = null;
		specialbuild = null;
		isdebug = false;
		ispatched = false;
		isprerelease = false;
		isprivatebuild = false;
		isspecialbuild = false;
		filemajorpart = 0;
		fileminorpart = 0;
		filebuildpart = 0;
		fileprivatepart = 0;
		productmajorpart = 0;
		productminorpart = 0;
		productbuildpart = 0;
		productprivatepart = 0;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetVersionInfo_internal(string fileName);

	/// <summary>Returns a <see cref="T:System.Diagnostics.FileVersionInfo" /> representing the version information associated with the specified file.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.FileVersionInfo" /> containing information about the file. If the file did not contain version information, the <see cref="T:System.Diagnostics.FileVersionInfo" /> contains only the name of the file requested.</returns>
	/// <param name="fileName">The fully qualified path and name of the file to retrieve the version information for. </param>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified cannot be found. </exception>
	/// <filterpriority>1</filterpriority>
	public static FileVersionInfo GetVersionInfo(string fileName)
	{
		if (!File.Exists(Path.GetFullPath(fileName)))
		{
			throw new FileNotFoundException(fileName);
		}
		FileVersionInfo fileVersionInfo = new FileVersionInfo();
		fileVersionInfo.GetVersionInfo_internal(fileName);
		return fileVersionInfo;
	}

	private static void AppendFormat(StringBuilder sb, string format, params object[] args)
	{
		sb.AppendFormat(format, args);
	}

	/// <summary>Returns a partial list of properties in the <see cref="T:System.Diagnostics.FileVersionInfo" /> and their values.</summary>
	/// <returns>A list of the following properties in this class and their values: <see cref="P:System.Diagnostics.FileVersionInfo.FileName" />, <see cref="P:System.Diagnostics.FileVersionInfo.InternalName" />, <see cref="P:System.Diagnostics.FileVersionInfo.OriginalFilename" />, <see cref="P:System.Diagnostics.FileVersionInfo.FileVersion" />, <see cref="P:System.Diagnostics.FileVersionInfo.FileDescription" />, <see cref="P:System.Diagnostics.FileVersionInfo.ProductName" />, <see cref="P:System.Diagnostics.FileVersionInfo.ProductVersion" />, <see cref="P:System.Diagnostics.FileVersionInfo.IsDebug" />, <see cref="P:System.Diagnostics.FileVersionInfo.IsPatched" />, <see cref="P:System.Diagnostics.FileVersionInfo.IsPreRelease" />, <see cref="P:System.Diagnostics.FileVersionInfo.IsPrivateBuild" />, <see cref="P:System.Diagnostics.FileVersionInfo.IsSpecialBuild" />,<see cref="P:System.Diagnostics.FileVersionInfo.Language" />.If the file did not contain version information, this list will contain only the name of the requested file. Boolean values will be false, and all other entries will be null.</returns>
	/// <filterpriority>2</filterpriority>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendFormat(stringBuilder, "File:             {0}{1}", FileName, Environment.NewLine);
		AppendFormat(stringBuilder, "InternalName:     {0}{1}", internalname, Environment.NewLine);
		AppendFormat(stringBuilder, "OriginalFilename: {0}{1}", originalfilename, Environment.NewLine);
		AppendFormat(stringBuilder, "FileVersion:      {0}{1}", fileversion, Environment.NewLine);
		AppendFormat(stringBuilder, "FileDescription:  {0}{1}", filedescription, Environment.NewLine);
		AppendFormat(stringBuilder, "Product:          {0}{1}", productname, Environment.NewLine);
		AppendFormat(stringBuilder, "ProductVersion:   {0}{1}", productversion, Environment.NewLine);
		AppendFormat(stringBuilder, "Debug:            {0}{1}", isdebug, Environment.NewLine);
		AppendFormat(stringBuilder, "Patched:          {0}{1}", ispatched, Environment.NewLine);
		AppendFormat(stringBuilder, "PreRelease:       {0}{1}", isprerelease, Environment.NewLine);
		AppendFormat(stringBuilder, "PrivateBuild:     {0}{1}", isprivatebuild, Environment.NewLine);
		AppendFormat(stringBuilder, "SpecialBuild:     {0}{1}", isspecialbuild, Environment.NewLine);
		AppendFormat(stringBuilder, "Language          {0}{1}", language, Environment.NewLine);
		return stringBuilder.ToString();
	}
}
