namespace System.Windows.Media;

/// <summary>An enumeration that specifies a hint to the rendering engine that text can be rendered with ClearType.</summary>
public enum ClearTypeHint
{
	/// <summary>The rendering engine uses ClearType when it is possible. If opacity is introduced, ClearType is disabled for that subtree.</summary>
	Auto,
	/// <summary>The rendering engine re-enables ClearType for the current subtree. Where opacity is introduced in this subtree, ClearType is disabled.</summary>
	Enabled
}
