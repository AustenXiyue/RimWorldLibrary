using System.CodeDom.Compiler;

namespace System.Xml.Xsl;

/// <summary>Specifies the XSLT features to support during execution of the XSLT style sheet.</summary>
public sealed class XsltSettings
{
	private bool enableDocumentFunction;

	private bool enableScript;

	private bool checkOnly;

	private bool includeDebugInformation;

	private int warningLevel = -1;

	private bool treatWarningsAsErrors;

	private TempFileCollection tempFiles;

	/// <summary>Gets an <see cref="T:System.Xml.Xsl.XsltSettings" /> object with default settings. Support for the XSLT document() function and embedded script blocks is disabled.</summary>
	/// <returns>An <see cref="T:System.Xml.Xsl.XsltSettings" /> object with the <see cref="P:System.Xml.Xsl.XsltSettings.EnableDocumentFunction" /> and <see cref="P:System.Xml.Xsl.XsltSettings.EnableScript" /> properties set to false.</returns>
	public static XsltSettings Default => new XsltSettings(enableDocumentFunction: false, enableScript: false);

	/// <summary>Gets an <see cref="T:System.Xml.Xsl.XsltSettings" /> object that enables support for the XSLT document() function and embedded script blocks.</summary>
	/// <returns>An <see cref="T:System.Xml.Xsl.XsltSettings" /> object with the <see cref="P:System.Xml.Xsl.XsltSettings.EnableDocumentFunction" /> and <see cref="P:System.Xml.Xsl.XsltSettings.EnableScript" /> properties set to true.</returns>
	public static XsltSettings TrustedXslt => new XsltSettings(enableDocumentFunction: true, enableScript: true);

	/// <summary>Gets or sets a value indicating whether to enable support for the XSLT document() function.</summary>
	/// <returns>true to support the XSLT document() function; otherwise, false. The default is false.</returns>
	public bool EnableDocumentFunction
	{
		get
		{
			return enableDocumentFunction;
		}
		set
		{
			enableDocumentFunction = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether to enable support for embedded script blocks.</summary>
	/// <returns>true to support script blocks in XSLT style sheets; otherwise, false. The default is false.</returns>
	public bool EnableScript
	{
		get
		{
			return enableScript;
		}
		set
		{
			enableScript = value;
		}
	}

	internal bool CheckOnly
	{
		get
		{
			return checkOnly;
		}
		set
		{
			checkOnly = value;
		}
	}

	internal bool IncludeDebugInformation
	{
		get
		{
			return includeDebugInformation;
		}
		set
		{
			includeDebugInformation = value;
		}
	}

	internal int WarningLevel
	{
		get
		{
			return warningLevel;
		}
		set
		{
			warningLevel = value;
		}
	}

	internal bool TreatWarningsAsErrors
	{
		get
		{
			return treatWarningsAsErrors;
		}
		set
		{
			treatWarningsAsErrors = value;
		}
	}

	internal TempFileCollection TempFiles
	{
		get
		{
			return tempFiles;
		}
		set
		{
			tempFiles = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Xsl.XsltSettings" /> class with default settings.</summary>
	public XsltSettings()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Xsl.XsltSettings" /> class with the specified settings.</summary>
	/// <param name="enableDocumentFunction">true to enable support for the XSLT document() function; otherwise, false.</param>
	/// <param name="enableScript">true to enable support for embedded scripts blocks; otherwise, false.</param>
	public XsltSettings(bool enableDocumentFunction, bool enableScript)
	{
		this.enableDocumentFunction = enableDocumentFunction;
		this.enableScript = enableScript;
	}
}
