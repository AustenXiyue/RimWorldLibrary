using System;
using System.Collections.Generic;

namespace MS.Internal.Annotations;

internal class AnnotationMap
{
	private Dictionary<Guid, List<IAttachedAnnotation>> _annotationIdToAttachedAnnotations = new Dictionary<Guid, List<IAttachedAnnotation>>();

	private static readonly List<IAttachedAnnotation> _emptyList = new List<IAttachedAnnotation>(0);

	internal bool IsEmpty => _annotationIdToAttachedAnnotations.Count == 0;

	internal void AddAttachedAnnotation(IAttachedAnnotation attachedAnnotation)
	{
		List<IAttachedAnnotation> value = null;
		if (!_annotationIdToAttachedAnnotations.TryGetValue(attachedAnnotation.Annotation.Id, out value))
		{
			value = new List<IAttachedAnnotation>(1);
			_annotationIdToAttachedAnnotations.Add(attachedAnnotation.Annotation.Id, value);
		}
		value.Add(attachedAnnotation);
	}

	internal void RemoveAttachedAnnotation(IAttachedAnnotation attachedAnnotation)
	{
		List<IAttachedAnnotation> value = null;
		if (_annotationIdToAttachedAnnotations.TryGetValue(attachedAnnotation.Annotation.Id, out value))
		{
			value.Remove(attachedAnnotation);
			if (value.Count == 0)
			{
				_annotationIdToAttachedAnnotations.Remove(attachedAnnotation.Annotation.Id);
			}
		}
	}

	internal List<IAttachedAnnotation> GetAttachedAnnotations(Guid annotationId)
	{
		List<IAttachedAnnotation> value = null;
		if (!_annotationIdToAttachedAnnotations.TryGetValue(annotationId, out value))
		{
			return _emptyList;
		}
		return value;
	}

	internal List<IAttachedAnnotation> GetAllAttachedAnnotations()
	{
		List<IAttachedAnnotation> list = new List<IAttachedAnnotation>(_annotationIdToAttachedAnnotations.Keys.Count);
		foreach (Guid key in _annotationIdToAttachedAnnotations.Keys)
		{
			List<IAttachedAnnotation> collection = _annotationIdToAttachedAnnotations[key];
			list.AddRange(collection);
		}
		if (list.Count == 0)
		{
			return _emptyList;
		}
		return list;
	}
}
