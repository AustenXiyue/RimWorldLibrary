namespace System.Windows.Media.Animation;

internal struct ResolvedKeyFrameEntry : IComparable
{
	internal int _originalKeyFrameIndex;

	internal TimeSpan _resolvedKeyTime;

	public int CompareTo(object other)
	{
		ResolvedKeyFrameEntry resolvedKeyFrameEntry = (ResolvedKeyFrameEntry)other;
		if (resolvedKeyFrameEntry._resolvedKeyTime > _resolvedKeyTime)
		{
			return -1;
		}
		if (resolvedKeyFrameEntry._resolvedKeyTime < _resolvedKeyTime)
		{
			return 1;
		}
		if (resolvedKeyFrameEntry._originalKeyFrameIndex > _originalKeyFrameIndex)
		{
			return -1;
		}
		if (resolvedKeyFrameEntry._originalKeyFrameIndex < _originalKeyFrameIndex)
		{
			return 1;
		}
		return 0;
	}
}
