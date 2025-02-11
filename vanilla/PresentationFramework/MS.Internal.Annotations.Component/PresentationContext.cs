using System.Windows;

namespace MS.Internal.Annotations.Component;

internal abstract class PresentationContext
{
	public abstract UIElement Host { get; }

	public abstract PresentationContext EnclosingContext { get; }

	public abstract void AddToHost(IAnnotationComponent component);

	public abstract void RemoveFromHost(IAnnotationComponent component, bool reorder);

	public abstract void InvalidateTransform(IAnnotationComponent component);

	public abstract void BringToFront(IAnnotationComponent component);

	public abstract void SendToBack(IAnnotationComponent component);
}
