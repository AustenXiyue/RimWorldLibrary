namespace System.Windows.Controls;

/// <summary>Specifies the visibility of a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> for scrollable content.</summary>
public enum ScrollBarVisibility
{
	/// <summary>A <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> does not appear even when the viewport cannot display all of the content. The dimension of the content is set to the corresponding dimension of the <see cref="T:System.Windows.Controls.ScrollViewer" /> parent. For a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />, the width of the content is set to the <see cref="P:System.Windows.Controls.ScrollViewer.ViewportWidth" /> of the <see cref="T:System.Windows.Controls.ScrollViewer" />. For a vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />, the height of the content is set to the <see cref="P:System.Windows.Controls.ScrollViewer.ViewportHeight" /> of the <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	Disabled,
	/// <summary>A <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> appears and the dimension of the <see cref="T:System.Windows.Controls.ScrollViewer" /> is applied to the content when the viewport cannot display all of the content. For a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />, the width of the content is set to the <see cref="P:System.Windows.Controls.ScrollViewer.ViewportWidth" /> of the <see cref="T:System.Windows.Controls.ScrollViewer" />. For a vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />, the height of the content is set to the <see cref="P:System.Windows.Controls.ScrollViewer.ViewportHeight" /> of the <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	Auto,
	/// <summary>A <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> does not appear even when the viewport cannot display all of the content. The dimension of the <see cref="T:System.Windows.Controls.ScrollViewer" /> is not applied to the content.</summary>
	Hidden,
	/// <summary>A <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> always appears. The dimension of the <see cref="T:System.Windows.Controls.ScrollViewer" /> is applied to the content. For a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />, the width of the content is set to the <see cref="P:System.Windows.Controls.ScrollViewer.ViewportWidth" /> of the <see cref="T:System.Windows.Controls.ScrollViewer" />. For a vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />, the height of the content is set to the <see cref="P:System.Windows.Controls.ScrollViewer.ViewportHeight" /> of the <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	Visible
}
