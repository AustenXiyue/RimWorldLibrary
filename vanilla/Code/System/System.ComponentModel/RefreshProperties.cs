namespace System.ComponentModel;

/// <summary>Defines identifiers that indicate the type of a refresh of the Properties window.</summary>
public enum RefreshProperties
{
	/// <summary>No refresh is necessary.</summary>
	None,
	/// <summary>The properties should be requeried and the view should be refreshed.</summary>
	All,
	/// <summary>The view should be refreshed.</summary>
	Repaint
}
