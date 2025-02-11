using System.Windows.Media;
using System.Windows.Threading;

namespace System.Windows.Controls.Primitives;

/// <summary>Defines methods that provide additional information about the layout state of an element.</summary>
public static class LayoutInformation
{
	private static void CheckArgument(FrameworkElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
	}

	/// <summary>Returns a <see cref="T:System.Windows.Rect" /> that represents the layout partition that is reserved for a child element.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> that represents the layout slot of the element.</returns>
	/// <param name="element">The <see cref="T:System.Windows.FrameworkElement" /> whose layout slot is desired.</param>
	public static Rect GetLayoutSlot(FrameworkElement element)
	{
		CheckArgument(element);
		return element.PreviousArrangeRect;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Media.Geometry" /> that represents the visible region of an element.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Geometry" /> that represents the visible region of an <paramref name="element" />.</returns>
	/// <param name="element">The <see cref="T:System.Windows.FrameworkElement" /> whose layout clip is desired.</param>
	public static Geometry GetLayoutClip(FrameworkElement element)
	{
		CheckArgument(element);
		return element.GetLayoutClipInternal();
	}

	/// <summary>Returns a <see cref="T:System.Windows.UIElement" /> that was being processed by the layout engine at the moment of an unhandled exception.</summary>
	/// <returns>A <see cref="T:System.Windows.UIElement" /> that was being processed by the layout engine at the moment of an unhandled exception.</returns>
	/// <param name="dispatcher">The <see cref="T:System.Windows.Threading.Dispatcher" /> object that defines the scope of the operation. There is one dispatcher per layout engine instance.</param>
	/// <exception cref="T:System.ArgumentNullException">Occurs when <paramref name="dispatcher" /> is null.</exception>
	public static UIElement GetLayoutExceptionElement(Dispatcher dispatcher)
	{
		if (dispatcher == null)
		{
			throw new ArgumentNullException("dispatcher");
		}
		UIElement result = null;
		ContextLayoutManager contextLayoutManager = ContextLayoutManager.From(dispatcher);
		if (contextLayoutManager != null)
		{
			result = contextLayoutManager.GetLastExceptionElement();
		}
		return result;
	}
}
