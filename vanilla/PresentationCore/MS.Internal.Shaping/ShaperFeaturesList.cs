namespace MS.Internal.Shaping;

internal class ShaperFeaturesList
{
	private ushort _minimumAddCount;

	private ushort _featuresCount;

	private Feature[] _features;

	public int FeaturesCount => _featuresCount;

	public Feature[] Features => _features;

	public int NextIx => _featuresCount;

	public uint CurrentTag
	{
		get
		{
			if (_featuresCount != 0)
			{
				return _features[_featuresCount - 1].Tag;
			}
			return 0u;
		}
	}

	public int Length => _featuresCount;

	public void SetFeatureParameter(ushort featureIx, uint paramValue)
	{
		Invariant.Assert(_featuresCount > featureIx);
		_features[featureIx].Parameter = paramValue;
	}

	internal bool Initialize(ushort newSize)
	{
		if (_features == null || newSize > _features.Length || newSize == 0)
		{
			Feature[] array = new Feature[newSize];
			if (array != null)
			{
				_features = array;
			}
		}
		_featuresCount = 0;
		_minimumAddCount = 3;
		return _features != null;
	}

	internal bool Resize(ushort newSize, ushort keepCount)
	{
		_featuresCount = keepCount;
		if (_features != null && _features.Length != 0 && keepCount > 0 && _features.Length >= keepCount)
		{
			ushort num = (ushort)_features.Length;
			if (newSize < keepCount)
			{
				newSize = keepCount;
			}
			if (newSize > num)
			{
				if (newSize < num + _minimumAddCount)
				{
					newSize = (ushort)(num + _minimumAddCount);
				}
				Feature[] array = new Feature[newSize];
				if (array == null)
				{
					return false;
				}
				for (int i = 0; i < keepCount; i++)
				{
					array[i] = _features[i];
				}
				_features = array;
			}
			return true;
		}
		return Initialize(newSize);
	}

	internal void AddFeature(Feature feature)
	{
		if (_featuresCount != _features.Length || Resize((ushort)(_featuresCount + 1), _featuresCount))
		{
			_features[_featuresCount] = feature;
			_featuresCount++;
		}
	}

	internal void AddFeature(ushort startIndex, ushort length, uint featureTag, uint parameter)
	{
		if (_featuresCount != _features.Length || Resize((ushort)(_featuresCount + 1), _featuresCount))
		{
			if (_features[_featuresCount] != null)
			{
				_features[_featuresCount].Tag = featureTag;
				_features[_featuresCount].StartIndex = startIndex;
				_features[_featuresCount].Length = length;
				_features[_featuresCount].Parameter = parameter;
			}
			else
			{
				_features[_featuresCount] = new Feature(startIndex, length, featureTag, parameter);
			}
			_featuresCount++;
		}
	}

	internal void AddFeature(ushort charIx, uint featureTag)
	{
		if (featureTag == 1)
		{
			return;
		}
		if (_featuresCount > 0)
		{
			ushort num = (ushort)(_featuresCount - 1);
			if ((featureTag == 0 || featureTag == _features[num].Tag) && _features[num].StartIndex + _features[num].Length == charIx)
			{
				_features[num].Length++;
			}
			else
			{
				AddFeature(charIx, 1, (featureTag == 0) ? _features[num].Tag : featureTag, 1u);
			}
		}
		else if (featureTag != 0)
		{
			AddFeature(charIx, 1, featureTag, 1u);
		}
	}

	internal void UpdatePreviousShapedChar(uint featureTag)
	{
		if (featureTag > 1 && _featuresCount > 0)
		{
			ushort num = (ushort)(_featuresCount - 1);
			if (_features[num].Tag != featureTag)
			{
				_features[num].Tag = featureTag;
			}
		}
	}
}
