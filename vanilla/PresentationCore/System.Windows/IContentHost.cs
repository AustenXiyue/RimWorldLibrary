using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows;

/// <summary>This interface is implemented by layouts which host <see cref="T:System.Windows.ContentElement" />. </summary>
public interface IContentHost
{
	/// <summary>Gets an enumeration containing all descendant <see cref="T:System.Windows.ContentElement" />-derived classes, as well as all <see cref="T:System.Windows.UIElement" />-derived classes that are a direct descendant of the <see cref="T:System.Windows.IContentHost" /> or one of its descendant <see cref="T:System.Windows.ContentElement" /> classes. </summary>
	/// <returns>Enumeration containing all descendant <see cref="T:System.Windows.ContentElement" />-derived classes, as well as all <see cref="T:System.Windows.UIElement" />-derived classes that are a direct descendant of the <see cref="T:System.Windows.IContentHost" /> or one of its descendant <see cref="T:System.Windows.ContentElement" /> classes. In other words, elements for which the <see cref="T:System.Windows.IContentHost" /> creates a visual representation (<see cref="T:System.Windows.ContentElement" />-derived classes) or whose layout is driven by the <see cref="T:System.Windows.IContentHost" /> (the first-level descendant <see cref="T:System.Windows.UIElement" />-derived classes).</returns>
	IEnumerator<IInputElement> HostedElements { get; }

	/// <summary>Performs hit-testing for child elements.</summary>
	/// <returns>A descendant of <see cref="T:System.Windows.IInputElement" />, or NULL if no such element exists.</returns>
	/// <param name="point">Mouse coordinates relative to the ContentHost.</param>
	IInputElement InputHitTest(Point point);

	/// <summary>Returns a collection of bounding rectangles for a child element. </summary>
	/// <returns>A collection of bounding rectangles for a child element. </returns>
	/// <param name="child">The child element that the bounding rectangles are returned for.</param>
	/// <exception cref="T:System.ArgumentNullException">If child is null.</exception>
	/// <exception cref="T:System.ArgumentException">If the element is not a direct descendant (i.e. element must be a child of the <see cref="T:System.Windows.IContentHost" /> or a <see cref="T:System.Windows.ContentElement" /> which is a direct descendant  of the <see cref="T:System.Windows.IContentHost" />).</exception>
	ReadOnlyCollection<Rect> GetRectangles(ContentElement child);

	/// <summary> Called when a <see cref="T:System.Windows.UIElement" />-derived class which is hosted by a <see cref="T:System.Windows.IContentHost" /> changes its <see cref="P:System.Windows.UIElement.DesiredSize" />.</summary>
	/// <param name="child">Child element whose <see cref="P:System.Windows.UIElement.DesiredSize" /> has changed</param>
	/// <exception cref="T:System.ArgumentNullException">If child is null.</exception>
	/// <exception cref="T:System.ArgumentException">If child is not a direct descendant (i.e. child must be a child of the <see cref="T:System.Windows.IContentHost" /> or a <see cref="T:System.Windows.ContentElement" /> which is a direct descendant of the <see cref="T:System.Windows.IContentHost" />).</exception>
	void OnChildDesiredSizeChanged(UIElement child);
}
