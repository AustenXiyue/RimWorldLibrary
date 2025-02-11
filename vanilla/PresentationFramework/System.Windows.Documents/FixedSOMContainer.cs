using System.Collections.Generic;

namespace System.Windows.Documents;

internal abstract class FixedSOMContainer : FixedSOMSemanticBox, IComparable
{
	protected List<FixedSOMSemanticBox> _semanticBoxes;

	protected List<FixedNode> _fixedNodes;

	internal virtual FixedElement.ElementType[] ElementTypes => Array.Empty<FixedElement.ElementType>();

	public List<FixedSOMSemanticBox> SemanticBoxes
	{
		get
		{
			return _semanticBoxes;
		}
		set
		{
			_semanticBoxes = value;
		}
	}

	public List<FixedNode> FixedNodes
	{
		get
		{
			if (_fixedNodes == null)
			{
				_ConstructFixedNodes();
			}
			return _fixedNodes;
		}
	}

	protected FixedSOMContainer()
	{
		_semanticBoxes = new List<FixedSOMSemanticBox>();
	}

	int IComparable.CompareTo(object comparedObj)
	{
		int num = int.MinValue;
		FixedSOMPageElement fixedSOMPageElement = comparedObj as FixedSOMPageElement;
		FixedSOMPageElement fixedSOMPageElement2 = this as FixedSOMPageElement;
		if (fixedSOMPageElement == null)
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, comparedObj.GetType(), typeof(FixedSOMContainer)), "comparedObj");
		}
		SpatialComparison spatialComparison = _CompareHorizontal(fixedSOMPageElement, RTL: false);
		SpatialComparison spatialComparison2 = _CompareVertical(fixedSOMPageElement);
		switch (spatialComparison)
		{
		case SpatialComparison.Before:
			if (spatialComparison2 != SpatialComparison.After)
			{
				num = -1;
			}
			break;
		case SpatialComparison.After:
			if (spatialComparison2 != SpatialComparison.Before)
			{
				num = 1;
			}
			break;
		case SpatialComparison.OverlapBefore:
			switch (spatialComparison2)
			{
			case SpatialComparison.Before:
				num = -1;
				break;
			case SpatialComparison.After:
				num = 1;
				break;
			}
			break;
		case SpatialComparison.OverlapAfter:
			switch (spatialComparison2)
			{
			case SpatialComparison.After:
				num = 1;
				break;
			case SpatialComparison.Before:
				num = -1;
				break;
			}
			break;
		case SpatialComparison.Equal:
			switch (spatialComparison2)
			{
			case SpatialComparison.OverlapAfter:
			case SpatialComparison.After:
				num = 1;
				break;
			case SpatialComparison.Before:
			case SpatialComparison.OverlapBefore:
				num = -1;
				break;
			case SpatialComparison.Equal:
				num = 0;
				break;
			}
			break;
		}
		if (num == int.MinValue)
		{
			if (fixedSOMPageElement2.FixedNodes.Count == 0 || fixedSOMPageElement.FixedNodes.Count == 0)
			{
				num = 0;
			}
			else
			{
				FixedNode item = fixedSOMPageElement2.FixedNodes[0];
				FixedNode item2 = fixedSOMPageElement2.FixedNodes[fixedSOMPageElement2.FixedNodes.Count - 1];
				FixedNode item3 = fixedSOMPageElement.FixedNodes[0];
				FixedNode item4 = fixedSOMPageElement.FixedNodes[fixedSOMPageElement.FixedNodes.Count - 1];
				if (fixedSOMPageElement2.FixedSOMPage.MarkupOrder.IndexOf(item3) - fixedSOMPageElement2.FixedSOMPage.MarkupOrder.IndexOf(item2) == 1)
				{
					num = -1;
				}
				else if (fixedSOMPageElement2.FixedSOMPage.MarkupOrder.IndexOf(item4) - fixedSOMPageElement2.FixedSOMPage.MarkupOrder.IndexOf(item) == 1)
				{
					num = 1;
				}
				else
				{
					int num2 = _SpatialToAbsoluteComparison(spatialComparison2);
					num = ((num2 != 0) ? num2 : _SpatialToAbsoluteComparison(spatialComparison));
				}
			}
		}
		return num;
	}

	protected void AddSorted(FixedSOMSemanticBox box)
	{
		int num = _semanticBoxes.Count - 1;
		while (num >= 0 && box.CompareTo(_semanticBoxes[num]) != 1)
		{
			num--;
		}
		_semanticBoxes.Insert(num + 1, box);
		_UpdateBoundingRect(box.BoundingRect);
	}

	protected void Add(FixedSOMSemanticBox box)
	{
		_semanticBoxes.Add(box);
		_UpdateBoundingRect(box.BoundingRect);
	}

	private void _ConstructFixedNodes()
	{
		_fixedNodes = new List<FixedNode>();
		foreach (FixedSOMSemanticBox semanticBox in _semanticBoxes)
		{
			if (semanticBox is FixedSOMElement fixedSOMElement)
			{
				_fixedNodes.Add(fixedSOMElement.FixedNode);
				continue;
			}
			foreach (FixedNode fixedNode in (semanticBox as FixedSOMContainer).FixedNodes)
			{
				_fixedNodes.Add(fixedNode);
			}
		}
	}

	private void _UpdateBoundingRect(Rect rect)
	{
		if (_boundingRect.IsEmpty)
		{
			_boundingRect = rect;
		}
		else
		{
			_boundingRect.Union(rect);
		}
	}
}
