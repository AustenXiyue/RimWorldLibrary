using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Documents;

namespace MS.Internal.Annotations.Component;

internal class AnnotationComponentManager : DependencyObject
{
	private Dictionary<IAttachedAnnotation, IList<IAnnotationComponent>> _attachedAnnotations = new Dictionary<IAttachedAnnotation, IList<IAnnotationComponent>>();

	internal AnnotationComponentManager(AnnotationService service)
	{
		if (service != null)
		{
			service.AttachedAnnotationChanged += AttachedAnnotationUpdateEventHandler;
		}
	}

	internal void AddAttachedAnnotation(IAttachedAnnotation attachedAnnotation, bool reorder)
	{
		IAnnotationComponent annotationComponent = FindComponent(attachedAnnotation);
		if (annotationComponent != null)
		{
			AddComponent(attachedAnnotation, annotationComponent, reorder);
		}
	}

	internal void RemoveAttachedAnnotation(IAttachedAnnotation attachedAnnotation, bool reorder)
	{
		if (!_attachedAnnotations.ContainsKey(attachedAnnotation))
		{
			return;
		}
		IList<IAnnotationComponent> list = _attachedAnnotations[attachedAnnotation];
		_attachedAnnotations.Remove(attachedAnnotation);
		foreach (IAnnotationComponent item in list)
		{
			item.RemoveAttachedAnnotation(attachedAnnotation);
			if (item.AttachedAnnotations.Count == 0 && item.PresentationContext != null)
			{
				item.PresentationContext.RemoveFromHost(item, reorder);
			}
		}
	}

	private void AttachedAnnotationUpdateEventHandler(object sender, AttachedAnnotationChangedEventArgs e)
	{
		switch (e.Action)
		{
		case AttachedAnnotationAction.Added:
			AddAttachedAnnotation(e.AttachedAnnotation, reorder: true);
			break;
		case AttachedAnnotationAction.Deleted:
			RemoveAttachedAnnotation(e.AttachedAnnotation, reorder: true);
			break;
		case AttachedAnnotationAction.Loaded:
			AddAttachedAnnotation(e.AttachedAnnotation, reorder: false);
			break;
		case AttachedAnnotationAction.Unloaded:
			RemoveAttachedAnnotation(e.AttachedAnnotation, reorder: false);
			break;
		case AttachedAnnotationAction.AnchorModified:
			ModifyAttachedAnnotation(e.AttachedAnnotation, e.PreviousAttachedAnchor, e.PreviousAttachmentLevel);
			break;
		}
	}

	private IAnnotationComponent FindComponent(IAttachedAnnotation attachedAnnotation)
	{
		return AnnotationService.GetChooser(attachedAnnotation.Parent as UIElement).ChooseAnnotationComponent(attachedAnnotation);
	}

	private void AddComponent(IAttachedAnnotation attachedAnnotation, IAnnotationComponent component, bool reorder)
	{
		UIElement uIElement = attachedAnnotation.Parent as UIElement;
		if (component.PresentationContext != null)
		{
			return;
		}
		AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(uIElement);
		if (adornerLayer == null)
		{
			if (PresentationSource.FromVisual(uIElement) != null)
			{
				throw new InvalidOperationException(SR.Format(SR.NoPresentationContextForGivenElement, uIElement));
			}
		}
		else
		{
			AddToAttachedAnnotations(attachedAnnotation, component);
			component.AddAttachedAnnotation(attachedAnnotation);
			AdornerPresentationContext.HostComponent(adornerLayer, component, uIElement, reorder);
		}
	}

	private void ModifyAttachedAnnotation(IAttachedAnnotation attachedAnnotation, object previousAttachedAnchor, AttachmentLevel previousAttachmentLevel)
	{
		if (!_attachedAnnotations.ContainsKey(attachedAnnotation))
		{
			AddAttachedAnnotation(attachedAnnotation, reorder: true);
			return;
		}
		IAnnotationComponent annotationComponent = FindComponent(attachedAnnotation);
		if (annotationComponent == null)
		{
			RemoveAttachedAnnotation(attachedAnnotation, reorder: true);
			return;
		}
		IList<IAnnotationComponent> list = _attachedAnnotations[attachedAnnotation];
		if (list.Contains(annotationComponent))
		{
			foreach (IAnnotationComponent item in list)
			{
				item.ModifyAttachedAnnotation(attachedAnnotation, previousAttachedAnchor, previousAttachmentLevel);
				if (item.AttachedAnnotations.Count == 0)
				{
					item.PresentationContext.RemoveFromHost(item, reorder: true);
				}
			}
			return;
		}
		RemoveAttachedAnnotation(attachedAnnotation, reorder: true);
		AddComponent(attachedAnnotation, annotationComponent, reorder: true);
	}

	private void AddToAttachedAnnotations(IAttachedAnnotation attachedAnnotation, IAnnotationComponent component)
	{
		if (!_attachedAnnotations.TryGetValue(attachedAnnotation, out var value))
		{
			value = new List<IAnnotationComponent>();
			_attachedAnnotations[attachedAnnotation] = value;
		}
		value.Add(component);
	}
}
