namespace System.Windows.Documents;

internal class FixedSOMGroup : FixedSOMPageElement, IComparable
{
	private int _RTLCount;

	private int _LTRCount;

	public override bool IsRTL => _RTLCount > _LTRCount;

	public FixedSOMGroup(FixedSOMPage page)
		: base(page)
	{
	}

	int IComparable.CompareTo(object comparedObj)
	{
		int result = int.MinValue;
		if (!(comparedObj is FixedSOMGroup fixedSOMGroup))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, comparedObj.GetType(), typeof(FixedSOMGroup)), "comparedObj");
		}
		bool rTL = IsRTL && fixedSOMGroup.IsRTL;
		SpatialComparison spatialComparison = _CompareHorizontal(fixedSOMGroup, rTL);
		switch (_CompareVertical(fixedSOMGroup))
		{
		case SpatialComparison.Before:
			result = -1;
			break;
		case SpatialComparison.After:
			result = 1;
			break;
		case SpatialComparison.OverlapBefore:
			result = ((spatialComparison > SpatialComparison.Equal) ? 1 : (-1));
			break;
		case SpatialComparison.OverlapAfter:
			result = ((spatialComparison >= SpatialComparison.Equal) ? 1 : (-1));
			break;
		case SpatialComparison.Equal:
			switch (spatialComparison)
			{
			case SpatialComparison.OverlapAfter:
			case SpatialComparison.After:
				result = 1;
				break;
			case SpatialComparison.Before:
			case SpatialComparison.OverlapBefore:
				result = -1;
				break;
			case SpatialComparison.Equal:
				result = 0;
				break;
			}
			break;
		}
		return result;
	}

	public void AddContainer(FixedSOMPageElement pageElement)
	{
		if (!(pageElement is FixedSOMFixedBlock fixedSOMFixedBlock) || (!fixedSOMFixedBlock.IsFloatingImage && !fixedSOMFixedBlock.IsWhiteSpace))
		{
			if (pageElement.IsRTL)
			{
				_RTLCount++;
			}
			else
			{
				_LTRCount++;
			}
		}
		_semanticBoxes.Add(pageElement);
		if (_boundingRect.IsEmpty)
		{
			_boundingRect = pageElement.BoundingRect;
		}
		else
		{
			_boundingRect.Union(pageElement.BoundingRect);
		}
	}
}
