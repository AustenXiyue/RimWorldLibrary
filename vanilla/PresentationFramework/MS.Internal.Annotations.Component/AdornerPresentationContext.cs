using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MS.Internal.Annotations.Component;

internal class AdornerPresentationContext : PresentationContext
{
	private class ZRange
	{
		private int _min;

		private int _max;

		public int Min => _min;

		public int Max => _max;

		public ZRange(int min, int max)
		{
			if (min > max)
			{
				int num = min;
				min = max;
				max = num;
			}
			_min = min;
			_max = max;
		}
	}

	private AnnotationAdorner _annotationAdorner;

	private AdornerLayer _adornerLayer;

	private static Hashtable _ZLevel = new Hashtable();

	private static Hashtable _ZRanges = new Hashtable();

	public override UIElement Host => _adornerLayer;

	public override PresentationContext EnclosingContext
	{
		get
		{
			if (!(VisualTreeHelper.GetParent(_adornerLayer) is Visual visual))
			{
				return null;
			}
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer((UIElement)visual);
			if (adornerLayer == null)
			{
				return null;
			}
			return new AdornerPresentationContext(adornerLayer, null);
		}
	}

	private AdornerPresentationContext(AdornerLayer adornerLayer, AnnotationAdorner adorner)
	{
		if (adornerLayer == null)
		{
			throw new ArgumentNullException("adornerLayer");
		}
		_adornerLayer = adornerLayer;
		if (adorner != null)
		{
			if (adorner.AnnotationComponent == null)
			{
				throw new ArgumentNullException("annotation component");
			}
			if (adorner.AnnotationComponent.PresentationContext != null)
			{
				throw new InvalidOperationException(SR.Format(SR.ComponentAlreadyInPresentationContext, adorner.AnnotationComponent));
			}
			_annotationAdorner = adorner;
		}
	}

	internal static void HostComponent(AdornerLayer adornerLayer, IAnnotationComponent component, UIElement annotatedElement, bool reorder)
	{
		AnnotationAdorner annotationAdorner = new AnnotationAdorner(component, annotatedElement);
		annotationAdorner.AnnotationComponent.PresentationContext = new AdornerPresentationContext(adornerLayer, annotationAdorner);
		int componentLevel = GetComponentLevel(component);
		if (reorder)
		{
			component.ZOrder = GetNextZOrder(adornerLayer, componentLevel);
		}
		adornerLayer.Add(annotationAdorner, ComponentToAdorner(component.ZOrder, componentLevel));
	}

	internal static void SetTypeZLevel(Type type, int level)
	{
		Invariant.Assert(level >= 0, "level is < 0");
		Invariant.Assert(type != null, "type is null");
		if (_ZLevel.ContainsKey(type))
		{
			_ZLevel[type] = level;
		}
		else
		{
			_ZLevel.Add(type, level);
		}
	}

	internal static void SetZLevelRange(int level, int min, int max)
	{
		if (_ZRanges[level] == null)
		{
			_ZRanges.Add(level, new ZRange(min, max));
		}
	}

	public override void AddToHost(IAnnotationComponent component)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		HostComponent(_adornerLayer, component, component.AnnotatedElement, reorder: false);
	}

	public override void RemoveFromHost(IAnnotationComponent component, bool reorder)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		if (IsInternalComponent(component))
		{
			_annotationAdorner.AnnotationComponent.PresentationContext = null;
			_adornerLayer.Remove(_annotationAdorner);
			_annotationAdorner.RemoveChildren();
			_annotationAdorner = null;
			return;
		}
		AnnotationAdorner annotationAdorner = FindAnnotationAdorner(component);
		if (annotationAdorner == null)
		{
			throw new InvalidOperationException(SR.Format(SR.ComponentNotInPresentationContext, component));
		}
		_adornerLayer.Remove(annotationAdorner);
		annotationAdorner.RemoveChildren();
		AdornerPresentationContext adornerPresentationContext = component.PresentationContext as AdornerPresentationContext;
		if (adornerPresentationContext != null)
		{
			adornerPresentationContext.ResetInternalAnnotationAdorner();
		}
		component.PresentationContext = null;
	}

	public override void InvalidateTransform(IAnnotationComponent component)
	{
		GetAnnotationAdorner(component).InvalidateTransform();
	}

	public override void BringToFront(IAnnotationComponent component)
	{
		AnnotationAdorner annotationAdorner = GetAnnotationAdorner(component);
		int componentLevel = GetComponentLevel(component);
		int nextZOrder = GetNextZOrder(_adornerLayer, componentLevel);
		if (nextZOrder != component.ZOrder + 1)
		{
			component.ZOrder = nextZOrder;
			_adornerLayer.SetAdornerZOrder(annotationAdorner, ComponentToAdorner(component.ZOrder, componentLevel));
		}
	}

	public override void SendToBack(IAnnotationComponent component)
	{
		GetAnnotationAdorner(component);
		GetComponentLevel(component);
		if (component.ZOrder != 0)
		{
			component.ZOrder = 0;
			UpdateComponentZOrder(component);
		}
	}

	public override bool Equals(object o)
	{
		AdornerPresentationContext adornerPresentationContext = o as AdornerPresentationContext;
		if (adornerPresentationContext != null)
		{
			return adornerPresentationContext._adornerLayer == _adornerLayer;
		}
		return false;
	}

	public static bool operator ==(AdornerPresentationContext left, AdornerPresentationContext right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	public static bool operator !=(AdornerPresentationContext c1, AdornerPresentationContext c2)
	{
		return !(c1 == c2);
	}

	public override int GetHashCode()
	{
		return _adornerLayer.GetHashCode();
	}

	public void UpdateComponentZOrder(IAnnotationComponent component)
	{
		Invariant.Assert(component != null, "null component");
		int componentLevel = GetComponentLevel(component);
		AnnotationAdorner annotationAdorner = FindAnnotationAdorner(component);
		if (annotationAdorner == null)
		{
			return;
		}
		_adornerLayer.SetAdornerZOrder(annotationAdorner, ComponentToAdorner(component.ZOrder, componentLevel));
		List<AnnotationAdorner> topAnnotationAdorners = GetTopAnnotationAdorners(componentLevel, component);
		if (topAnnotationAdorners == null)
		{
			return;
		}
		int num = component.ZOrder + 1;
		foreach (AnnotationAdorner item in topAnnotationAdorners)
		{
			item.AnnotationComponent.ZOrder = num;
			_adornerLayer.SetAdornerZOrder(item, ComponentToAdorner(num, componentLevel));
			num++;
		}
	}

	private void ResetInternalAnnotationAdorner()
	{
		_annotationAdorner = null;
	}

	private bool IsInternalComponent(IAnnotationComponent component)
	{
		if (_annotationAdorner != null)
		{
			return component == _annotationAdorner.AnnotationComponent;
		}
		return false;
	}

	private AnnotationAdorner FindAnnotationAdorner(IAnnotationComponent component)
	{
		if (_adornerLayer == null)
		{
			return null;
		}
		Adorner[] adorners = _adornerLayer.GetAdorners(component.AnnotatedElement);
		for (int i = 0; i < adorners.Length; i++)
		{
			if (adorners[i] is AnnotationAdorner annotationAdorner && annotationAdorner.AnnotationComponent == component)
			{
				return annotationAdorner;
			}
		}
		return null;
	}

	private List<AnnotationAdorner> GetTopAnnotationAdorners(int level, IAnnotationComponent component)
	{
		List<AnnotationAdorner> list = new List<AnnotationAdorner>();
		int childrenCount = VisualTreeHelper.GetChildrenCount(_adornerLayer);
		if (childrenCount == 0)
		{
			return list;
		}
		for (int i = 0; i < childrenCount; i++)
		{
			if (VisualTreeHelper.GetChild(_adornerLayer, i) is AnnotationAdorner { AnnotationComponent: var annotationComponent } annotationAdorner && annotationComponent != component && GetComponentLevel(annotationComponent) == level && annotationComponent.ZOrder >= component.ZOrder)
			{
				AddAdorner(list, annotationAdorner);
			}
		}
		return list;
	}

	private void AddAdorner(List<AnnotationAdorner> adorners, AnnotationAdorner adorner)
	{
		int num = 0;
		if (adorners.Count > 0)
		{
			num = adorners.Count;
			while (num > 0 && adorners[num - 1].AnnotationComponent.ZOrder > adorner.AnnotationComponent.ZOrder)
			{
				num--;
			}
		}
		adorners.Insert(num, adorner);
	}

	private static int GetNextZOrder(AdornerLayer adornerLayer, int level)
	{
		Invariant.Assert(adornerLayer != null, "null adornerLayer");
		int num = 0;
		int childrenCount = VisualTreeHelper.GetChildrenCount(adornerLayer);
		if (childrenCount == 0)
		{
			return num;
		}
		for (int i = 0; i < childrenCount; i++)
		{
			if (VisualTreeHelper.GetChild(adornerLayer, i) is AnnotationAdorner annotationAdorner && GetComponentLevel(annotationAdorner.AnnotationComponent) == level && annotationAdorner.AnnotationComponent.ZOrder >= num)
			{
				num = annotationAdorner.AnnotationComponent.ZOrder + 1;
			}
		}
		return num;
	}

	private AnnotationAdorner GetAnnotationAdorner(IAnnotationComponent component)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		AnnotationAdorner annotationAdorner = _annotationAdorner;
		if (!IsInternalComponent(component))
		{
			annotationAdorner = FindAnnotationAdorner(component);
			if (annotationAdorner == null)
			{
				throw new InvalidOperationException(SR.Format(SR.ComponentNotInPresentationContext, component));
			}
		}
		return annotationAdorner;
	}

	private static int GetComponentLevel(IAnnotationComponent component)
	{
		int result = 0;
		Type type = component.GetType();
		if (_ZLevel.ContainsKey(type))
		{
			result = (int)_ZLevel[type];
		}
		return result;
	}

	private static int ComponentToAdorner(int zOrder, int level)
	{
		int num = zOrder;
		ZRange zRange = (ZRange)_ZRanges[level];
		if (zRange != null)
		{
			num += zRange.Min;
			if (num < zRange.Min)
			{
				num = zRange.Min;
			}
			if (num > zRange.Max)
			{
				num = zRange.Max;
			}
		}
		return num;
	}
}
