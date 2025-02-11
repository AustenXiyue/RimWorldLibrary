namespace System.Windows.Documents;

internal abstract class FixedSOMSemanticBox : IComparable
{
	protected enum SpatialComparison
	{
		None,
		Before,
		OverlapBefore,
		Equal,
		OverlapAfter,
		After
	}

	protected Rect _boundingRect;

	public Rect BoundingRect
	{
		get
		{
			return _boundingRect;
		}
		set
		{
			_boundingRect = value;
		}
	}

	public FixedSOMSemanticBox()
	{
		_boundingRect = Rect.Empty;
	}

	public FixedSOMSemanticBox(Rect boundingRect)
	{
		_boundingRect = boundingRect;
	}

	public virtual void SetRTFProperties(FixedElement element)
	{
	}

	public int CompareTo(object o)
	{
		if (!(o is FixedSOMSemanticBox))
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedParameterType, o.GetType(), typeof(FixedSOMSemanticBox)), "o");
		}
		SpatialComparison spatialComparison = _CompareHorizontal(o as FixedSOMSemanticBox, RTL: false);
		SpatialComparison spatialComparison2 = _CompareVertical(o as FixedSOMSemanticBox);
		if (spatialComparison == SpatialComparison.Equal && spatialComparison2 == SpatialComparison.Equal)
		{
			return 0;
		}
		if (spatialComparison == SpatialComparison.Equal)
		{
			if (spatialComparison2 == SpatialComparison.Before || spatialComparison2 == SpatialComparison.OverlapBefore)
			{
				return -1;
			}
			return 1;
		}
		if (spatialComparison2 == SpatialComparison.Equal)
		{
			if (spatialComparison == SpatialComparison.Before || spatialComparison == SpatialComparison.OverlapBefore)
			{
				return -1;
			}
			return 1;
		}
		switch (spatialComparison)
		{
		case SpatialComparison.Before:
			return -1;
		case SpatialComparison.After:
			return 1;
		default:
			switch (spatialComparison2)
			{
			case SpatialComparison.Before:
				return -1;
			case SpatialComparison.After:
				return 1;
			default:
				if (spatialComparison == SpatialComparison.OverlapBefore)
				{
					return -1;
				}
				return 1;
			}
		}
	}

	int IComparable.CompareTo(object o)
	{
		return CompareTo(o);
	}

	protected SpatialComparison _CompareHorizontal(FixedSOMSemanticBox otherBox, bool RTL)
	{
		SpatialComparison spatialComparison = SpatialComparison.None;
		Rect boundingRect = BoundingRect;
		Rect boundingRect2 = otherBox.BoundingRect;
		double num = (RTL ? boundingRect.Right : boundingRect.Left);
		double num2 = (RTL ? boundingRect2.Right : boundingRect2.Left);
		if (num == num2)
		{
			spatialComparison = SpatialComparison.Equal;
		}
		else if (boundingRect.Right < boundingRect2.Left)
		{
			spatialComparison = SpatialComparison.Before;
		}
		else if (boundingRect2.Right < boundingRect.Left)
		{
			spatialComparison = SpatialComparison.After;
		}
		else
		{
			double num3 = Math.Abs(num - num2);
			double num4 = ((boundingRect.Width > boundingRect2.Width) ? boundingRect.Width : boundingRect2.Width);
			spatialComparison = ((num3 / num4 < 0.1) ? SpatialComparison.Equal : ((!(boundingRect.Left < boundingRect2.Left)) ? SpatialComparison.OverlapAfter : SpatialComparison.OverlapBefore));
		}
		if (RTL && spatialComparison != SpatialComparison.Equal)
		{
			spatialComparison = _InvertSpatialComparison(spatialComparison);
		}
		return spatialComparison;
	}

	protected SpatialComparison _CompareVertical(FixedSOMSemanticBox otherBox)
	{
		SpatialComparison spatialComparison = SpatialComparison.None;
		Rect boundingRect = BoundingRect;
		Rect boundingRect2 = otherBox.BoundingRect;
		if (boundingRect.Top == boundingRect2.Top)
		{
			return SpatialComparison.Equal;
		}
		if (boundingRect.Bottom <= boundingRect2.Top)
		{
			return SpatialComparison.Before;
		}
		if (boundingRect2.Bottom <= boundingRect.Top)
		{
			return SpatialComparison.After;
		}
		if (boundingRect.Top < boundingRect2.Top)
		{
			return SpatialComparison.OverlapBefore;
		}
		return SpatialComparison.OverlapAfter;
	}

	protected int _SpatialToAbsoluteComparison(SpatialComparison comparison)
	{
		int result = 0;
		switch (comparison)
		{
		case SpatialComparison.Before:
		case SpatialComparison.OverlapBefore:
			result = -1;
			break;
		case SpatialComparison.OverlapAfter:
		case SpatialComparison.After:
			result = 1;
			break;
		case SpatialComparison.Equal:
			result = 0;
			break;
		}
		return result;
	}

	protected SpatialComparison _InvertSpatialComparison(SpatialComparison comparison)
	{
		SpatialComparison result = comparison;
		switch (comparison)
		{
		case SpatialComparison.Before:
			result = SpatialComparison.After;
			break;
		case SpatialComparison.After:
			result = SpatialComparison.Before;
			break;
		case SpatialComparison.OverlapBefore:
			result = SpatialComparison.OverlapAfter;
			break;
		case SpatialComparison.OverlapAfter:
			result = SpatialComparison.OverlapBefore;
			break;
		}
		return result;
	}
}
