using System;
using System.Collections.Generic;
using System.Windows.Annotations;

namespace MS.Internal.Annotations.Storage;

internal class StoreAnnotationsMap
{
	private class CachedAnnotation
	{
		private Annotation _annotation;

		private bool _dirty;

		public Annotation Annotation
		{
			get
			{
				return _annotation;
			}
			set
			{
				_annotation = value;
			}
		}

		public bool Dirty
		{
			get
			{
				return _dirty;
			}
			set
			{
				_dirty = value;
			}
		}

		public CachedAnnotation(Annotation annotation, bool dirty)
		{
			Annotation = annotation;
			Dirty = dirty;
		}
	}

	private Dictionary<Guid, CachedAnnotation> _currentAnnotations = new Dictionary<Guid, CachedAnnotation>();

	private AnnotationAuthorChangedEventHandler _authorChanged;

	private AnnotationResourceChangedEventHandler _anchorChanged;

	private AnnotationResourceChangedEventHandler _cargoChanged;

	internal StoreAnnotationsMap(AnnotationAuthorChangedEventHandler authorChanged, AnnotationResourceChangedEventHandler anchorChanged, AnnotationResourceChangedEventHandler cargoChanged)
	{
		_authorChanged = authorChanged;
		_anchorChanged = anchorChanged;
		_cargoChanged = cargoChanged;
	}

	public void AddAnnotation(Annotation annotation, bool dirty)
	{
		annotation.AuthorChanged += OnAuthorChanged;
		annotation.AnchorChanged += OnAnchorChanged;
		annotation.CargoChanged += OnCargoChanged;
		_currentAnnotations.Add(annotation.Id, new CachedAnnotation(annotation, dirty));
	}

	public void RemoveAnnotation(Guid id)
	{
		CachedAnnotation value = null;
		if (_currentAnnotations.TryGetValue(id, out value))
		{
			value.Annotation.AuthorChanged -= OnAuthorChanged;
			value.Annotation.AnchorChanged -= OnAnchorChanged;
			value.Annotation.CargoChanged -= OnCargoChanged;
			_currentAnnotations.Remove(id);
		}
	}

	public Dictionary<Guid, Annotation> FindAnnotations(ContentLocator anchorLocator)
	{
		if (anchorLocator == null)
		{
			throw new ArgumentNullException("locator");
		}
		Dictionary<Guid, Annotation> dictionary = new Dictionary<Guid, Annotation>();
		Dictionary<Guid, CachedAnnotation>.ValueCollection.Enumerator enumerator = _currentAnnotations.Values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Annotation annotation = enumerator.Current.Annotation;
			bool flag = false;
			foreach (AnnotationResource anchor in annotation.Anchors)
			{
				foreach (ContentLocatorBase contentLocator2 in anchor.ContentLocators)
				{
					if (contentLocator2 is ContentLocator contentLocator)
					{
						if (contentLocator.StartsWith(anchorLocator))
						{
							flag = true;
						}
					}
					else if (contentLocator2 is ContentLocatorGroup contentLocatorGroup)
					{
						foreach (ContentLocator locator in contentLocatorGroup.Locators)
						{
							if (locator.StartsWith(anchorLocator))
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						dictionary.Add(annotation.Id, annotation);
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		return dictionary;
	}

	public Dictionary<Guid, Annotation> FindAnnotations()
	{
		Dictionary<Guid, Annotation> dictionary = new Dictionary<Guid, Annotation>();
		foreach (KeyValuePair<Guid, CachedAnnotation> currentAnnotation in _currentAnnotations)
		{
			dictionary.Add(currentAnnotation.Key, currentAnnotation.Value.Annotation);
		}
		return dictionary;
	}

	public Annotation FindAnnotation(Guid id)
	{
		CachedAnnotation value = null;
		if (_currentAnnotations.TryGetValue(id, out value))
		{
			return value.Annotation;
		}
		return null;
	}

	public List<Annotation> FindDirtyAnnotations()
	{
		List<Annotation> list = new List<Annotation>();
		foreach (KeyValuePair<Guid, CachedAnnotation> currentAnnotation in _currentAnnotations)
		{
			if (currentAnnotation.Value.Dirty)
			{
				list.Add(currentAnnotation.Value.Annotation);
			}
		}
		return list;
	}

	public void ValidateDirtyAnnotations()
	{
		foreach (KeyValuePair<Guid, CachedAnnotation> currentAnnotation in _currentAnnotations)
		{
			if (currentAnnotation.Value.Dirty)
			{
				currentAnnotation.Value.Dirty = false;
			}
		}
	}

	private void OnAnchorChanged(object sender, AnnotationResourceChangedEventArgs args)
	{
		_currentAnnotations[args.Annotation.Id].Dirty = true;
		_anchorChanged(sender, args);
	}

	private void OnCargoChanged(object sender, AnnotationResourceChangedEventArgs args)
	{
		_currentAnnotations[args.Annotation.Id].Dirty = true;
		_cargoChanged(sender, args);
	}

	private void OnAuthorChanged(object sender, AnnotationAuthorChangedEventArgs args)
	{
		_currentAnnotations[args.Annotation.Id].Dirty = true;
		_authorChanged(sender, args);
	}
}
