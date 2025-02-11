namespace System.Windows.Media.Animation;

/// <summary>Specifies how new animations interact with any existing ones that are already applied to a property. </summary>
public enum HandoffBehavior
{
	/// <summary>New animations replace any existing animations on the properties to which they are applied.</summary>
	SnapshotAndReplace,
	/// <summary>New animations are combined with existing animations by appending the new animations to the end of the composition chain.</summary>
	Compose
}
