using System.Diagnostics;
using System.Reflection;

namespace System.Xaml;

/// <summary>Specifies processing rules or option settings for a <see cref="T:System.Xaml.XamlReader" /> implementation.</summary>
public class XamlReaderSettings
{
	/// <summary>Gets or sets a value that indicates whether the root object may include members that have a protected code access model when it reports the XAML type representation.</summary>
	/// <returns>true if the root object may include members that have a protected code access model; otherwise, false.</returns>
	public bool AllowProtectedMembersOnRoot { get; set; }

	/// <summary>Gets or sets a value that specifies whether the reader can provide line number and position.</summary>
	/// <returns>true if the reader can provide line number and position information; otherwise, false.</returns>
	public bool ProvideLineInfo { get; set; }

	/// <summary>Gets or sets the base URI that is used to resolve relative paths.</summary>
	/// <returns>The base URI to use. The default is null.</returns>
	public Uri BaseUri { get; set; }

	/// <summary>Gets or sets the object that represents the current local assembly for processing. This assembly information is used for calls to helper APIs such as <see cref="M:System.Xaml.XamlType.GetAllMembers" />.</summary>
	/// <returns>A CLR reflection <see cref="T:System.Reflection.Assembly" /> object.</returns>
	public Assembly LocalAssembly { get; set; }

	/// <summary>Gets or sets a value that specifies whether the XAML reader should ignore values for x:Uid attributes that exist on property elements.</summary>
	/// <returns>true if the reader should ignore values for x:Uid attributes on property elements. false if the reader should process x:Uid attributes on property elements. The default is false.</returns>
	public bool IgnoreUidsOnPropertyElements { get; set; }

	/// <summary>Gets or sets a value that specifies whether the reader enforces that all Value nodes are processed as a String type.</summary>
	/// <returns>true if the reader enforces that only String is contained in Value nodes; otherwise, false. The default is false.</returns>
	public bool ValuesMustBeString { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlReaderSettings" /> class.</summary>
	public XamlReaderSettings()
	{
		InitializeProvideLineInfo();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlReaderSettings" /> class based on values in another <see cref="T:System.Xaml.XamlReaderSettings" /> object.</summary>
	/// <param name="settings">An existing <see cref="T:System.Xaml.XamlReaderSettings" /> object.</param>
	public XamlReaderSettings(XamlReaderSettings settings)
		: this()
	{
		if (settings != null)
		{
			AllowProtectedMembersOnRoot = settings.AllowProtectedMembersOnRoot;
			ProvideLineInfo = settings.ProvideLineInfo;
			BaseUri = settings.BaseUri;
			LocalAssembly = settings.LocalAssembly;
			IgnoreUidsOnPropertyElements = settings.IgnoreUidsOnPropertyElements;
			ValuesMustBeString = settings.ValuesMustBeString;
		}
	}

	private void InitializeProvideLineInfo()
	{
		if (Debugger.IsAttached)
		{
			ProvideLineInfo = true;
		}
		else
		{
			ProvideLineInfo = false;
		}
	}
}
