namespace System.Security.Cryptography.X509Certificates;

/// <summary>Specifies the type of selection requested using the <see cref="Overload:System.Security.Cryptography.X509Certificates.X509Certificate2UI.SelectFromCollection" /> method.</summary>
public enum X509SelectionFlag
{
	/// <summary>A single selection. The UI allows the user to select one X.509 certificate.</summary>
	SingleSelection,
	/// <summary>A multiple selection. The user can use the SHIFT or CRTL keys to select more than one X.509 certificate.</summary>
	MultiSelection
}
