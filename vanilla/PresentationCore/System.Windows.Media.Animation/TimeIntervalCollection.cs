namespace System.Windows.Media.Animation;

internal struct TimeIntervalCollection
{
	private TimeSpan[] _nodeTime;

	private bool[] _nodeIsPoint;

	private bool[] _nodeIsInterval;

	private bool _containsNullPoint;

	private int _count;

	private int _current;

	private bool _invertCollection;

	private const int _minimumCapacity = 4;

	internal bool IsSingleInterval
	{
		get
		{
			if (_count >= 2)
			{
				if (_count == 2)
				{
					return _nodeIsInterval[0];
				}
				return false;
			}
			return true;
		}
	}

	internal TimeSpan FirstNodeTime => _nodeTime[0];

	internal static TimeIntervalCollection Empty => default(TimeIntervalCollection);

	internal bool IsEmptyOfRealPoints => _count == 0;

	internal bool IsEmpty
	{
		get
		{
			if (_count == 0)
			{
				return !_containsNullPoint;
			}
			return false;
		}
	}

	private bool CurrentIsAtLastNode => _current + 1 == _count;

	private TimeSpan CurrentNodeTime
	{
		get
		{
			return _nodeTime[_current];
		}
		set
		{
			_nodeTime[_current] = value;
		}
	}

	private bool CurrentNodeIsPoint
	{
		get
		{
			return _nodeIsPoint[_current] ^ _invertCollection;
		}
		set
		{
			_nodeIsPoint[_current] = value;
		}
	}

	private bool CurrentNodeIsInterval
	{
		get
		{
			return _nodeIsInterval[_current] ^ _invertCollection;
		}
		set
		{
			_nodeIsInterval[_current] = value;
		}
	}

	private TimeSpan NextNodeTime => _nodeTime[_current + 1];

	private bool NextNodeIsPoint => _nodeIsPoint[_current + 1] ^ _invertCollection;

	private bool NextNodeIsInterval => _nodeIsInterval[_current + 1] ^ _invertCollection;

	internal bool ContainsNullPoint => _containsNullPoint ^ _invertCollection;

	private TimeIntervalCollection(bool containsNullPoint)
	{
		_containsNullPoint = containsNullPoint;
		_count = 0;
		_current = 0;
		_invertCollection = false;
		_nodeTime = null;
		_nodeIsPoint = null;
		_nodeIsInterval = null;
	}

	private TimeIntervalCollection(TimeSpan point)
		: this(containsNullPoint: false)
	{
		InitializePoint(point);
	}

	private void InitializePoint(TimeSpan point)
	{
		EnsureAllocatedCapacity(4);
		_nodeTime[0] = point;
		_nodeIsPoint[0] = true;
		_nodeIsInterval[0] = false;
		_count = 1;
	}

	private TimeIntervalCollection(TimeSpan point, bool includePoint)
		: this(containsNullPoint: false)
	{
		InitializePoint(point);
		_nodeIsPoint[0] = includePoint;
		_nodeIsInterval[0] = true;
	}

	private TimeIntervalCollection(TimeSpan from, bool includeFrom, TimeSpan to, bool includeTo)
		: this(containsNullPoint: false)
	{
		EnsureAllocatedCapacity(4);
		_nodeTime[0] = from;
		if (from == to)
		{
			if (includeFrom || includeTo)
			{
				_nodeIsPoint[0] = true;
				_count = 1;
			}
			return;
		}
		if (from < to)
		{
			_nodeIsPoint[0] = includeFrom;
			_nodeIsInterval[0] = true;
			_nodeTime[1] = to;
			_nodeIsPoint[1] = includeTo;
		}
		else
		{
			_nodeTime[0] = to;
			_nodeIsPoint[0] = includeTo;
			_nodeIsInterval[0] = true;
			_nodeTime[1] = from;
			_nodeIsPoint[1] = includeFrom;
		}
		_count = 2;
	}

	internal void Clear()
	{
		if (_nodeTime != null && _nodeTime.Length > 4)
		{
			_nodeTime = null;
			_nodeIsPoint = null;
			_nodeIsInterval = null;
		}
		_containsNullPoint = false;
		_count = 0;
		_current = 0;
		_invertCollection = false;
	}

	internal TimeIntervalCollection SlipBeginningOfConnectedInterval(TimeSpan slipTime)
	{
		if (slipTime == TimeSpan.Zero)
		{
			return this;
		}
		TimeIntervalCollection result = ((_count >= 2 && !(slipTime > _nodeTime[1] - _nodeTime[0])) ? new TimeIntervalCollection(_nodeTime[0] + slipTime, _nodeIsPoint[0], _nodeTime[1], _nodeIsPoint[1]) : Empty);
		if (ContainsNullPoint)
		{
			result.AddNullPoint();
		}
		return result;
	}

	internal TimeIntervalCollection SetBeginningOfConnectedInterval(TimeSpan beginTime)
	{
		if (_count == 1)
		{
			return new TimeIntervalCollection(_nodeTime[0], _nodeIsPoint[0], beginTime, includeTo: true);
		}
		return new TimeIntervalCollection(beginTime, includeFrom: false, _nodeTime[1], _nodeIsPoint[1]);
	}

	internal static TimeIntervalCollection CreatePoint(TimeSpan time)
	{
		return new TimeIntervalCollection(time);
	}

	internal static TimeIntervalCollection CreateClosedOpenInterval(TimeSpan from, TimeSpan to)
	{
		return new TimeIntervalCollection(from, includeFrom: true, to, includeTo: false);
	}

	internal static TimeIntervalCollection CreateOpenClosedInterval(TimeSpan from, TimeSpan to)
	{
		return new TimeIntervalCollection(from, includeFrom: false, to, includeTo: true);
	}

	internal static TimeIntervalCollection CreateInfiniteClosedInterval(TimeSpan from)
	{
		return new TimeIntervalCollection(from, includePoint: true);
	}

	internal static TimeIntervalCollection CreateNullPoint()
	{
		return new TimeIntervalCollection(containsNullPoint: true);
	}

	internal void AddNullPoint()
	{
		_containsNullPoint = true;
	}

	internal bool Contains(TimeSpan time)
	{
		int num = Locate(time);
		if (num < 0)
		{
			return false;
		}
		if (_nodeTime[num] == time)
		{
			return _nodeIsPoint[num];
		}
		return _nodeIsInterval[num];
	}

	internal bool Intersects(TimeSpan from, TimeSpan to)
	{
		if (from == to)
		{
			return false;
		}
		if (from > to)
		{
			TimeSpan timeSpan = from;
			from = to;
			to = timeSpan;
		}
		int num = Locate(from);
		int num2 = Locate(to);
		if (num == num2)
		{
			if (num2 >= 0)
			{
				return _nodeIsInterval[num2];
			}
			return false;
		}
		if (num + 1 == num2)
		{
			if (!(to > _nodeTime[num2]))
			{
				if (num >= 0)
				{
					return _nodeIsInterval[num];
				}
				return false;
			}
			return true;
		}
		return true;
	}

	internal bool Intersects(TimeIntervalCollection other)
	{
		if (ContainsNullPoint && other.ContainsNullPoint)
		{
			return true;
		}
		if (IsEmptyOfRealPoints || other.IsEmptyOfRealPoints)
		{
			return false;
		}
		return IntersectsHelper(other);
	}

	private bool IntersectsHelper(TimeIntervalCollection other)
	{
		IntersectsHelperPrepareIndexers(ref this, ref other);
		bool intersectionFound = false;
		while (true)
		{
			if (CurrentNodeTime < other.CurrentNodeTime && IntersectsHelperUnequalCase(ref this, ref other, ref intersectionFound))
			{
				return intersectionFound;
			}
			if (CurrentNodeTime > other.CurrentNodeTime && IntersectsHelperUnequalCase(ref other, ref this, ref intersectionFound))
			{
				break;
			}
			while (CurrentNodeTime == other.CurrentNodeTime)
			{
				if (IntersectsHelperEqualCase(ref this, ref other, ref intersectionFound))
				{
					return intersectionFound;
				}
			}
		}
		return intersectionFound;
	}

	private static void IntersectsHelperPrepareIndexers(ref TimeIntervalCollection tic1, ref TimeIntervalCollection tic2)
	{
		tic1.MoveFirst();
		tic2.MoveFirst();
		if (tic1.CurrentNodeTime < tic2.CurrentNodeTime)
		{
			while (!tic1.CurrentIsAtLastNode && tic1.NextNodeTime <= tic2.CurrentNodeTime)
			{
				tic1.MoveNext();
			}
		}
		else if (tic2.CurrentNodeTime < tic1.CurrentNodeTime)
		{
			while (!tic2.CurrentIsAtLastNode && tic2.NextNodeTime <= tic1.CurrentNodeTime)
			{
				tic2.MoveNext();
			}
		}
	}

	private static bool IntersectsHelperUnequalCase(ref TimeIntervalCollection tic1, ref TimeIntervalCollection tic2, ref bool intersectionFound)
	{
		if (tic1.CurrentNodeIsInterval)
		{
			intersectionFound = true;
			return true;
		}
		if (tic1.CurrentIsAtLastNode)
		{
			intersectionFound = false;
			return true;
		}
		while (!tic2.CurrentIsAtLastNode && tic2.NextNodeTime <= tic1.NextNodeTime)
		{
			tic2.MoveNext();
		}
		tic1.MoveNext();
		return false;
	}

	private static bool IntersectsHelperEqualCase(ref TimeIntervalCollection tic1, ref TimeIntervalCollection tic2, ref bool intersectionFound)
	{
		if ((tic1.CurrentNodeIsPoint && tic2.CurrentNodeIsPoint) || (tic1.CurrentNodeIsInterval && tic2.CurrentNodeIsInterval))
		{
			intersectionFound = true;
			return true;
		}
		if (!tic1.CurrentIsAtLastNode && (tic2.CurrentIsAtLastNode || tic1.NextNodeTime < tic2.NextNodeTime))
		{
			tic1.MoveNext();
		}
		else if (!tic2.CurrentIsAtLastNode && (tic1.CurrentIsAtLastNode || tic2.NextNodeTime < tic1.NextNodeTime))
		{
			tic2.MoveNext();
		}
		else
		{
			if (tic1.CurrentIsAtLastNode || tic2.CurrentIsAtLastNode)
			{
				intersectionFound = false;
				return true;
			}
			tic1.MoveNext();
			tic2.MoveNext();
		}
		return false;
	}

	internal bool IntersectsInverseOf(TimeIntervalCollection other)
	{
		if (ContainsNullPoint && !other.ContainsNullPoint)
		{
			return true;
		}
		if (IsEmptyOfRealPoints)
		{
			return false;
		}
		if (other.IsEmptyOfRealPoints || _nodeTime[0] < other._nodeTime[0])
		{
			return true;
		}
		other.SetInvertedMode(mode: true);
		bool result = IntersectsHelper(other);
		other.SetInvertedMode(mode: false);
		return result;
	}

	internal bool IntersectsPeriodicCollection(TimeSpan beginTime, Duration period, double appliedSpeedRatio, double accelRatio, double decelRatio, bool isAutoReversed)
	{
		if (IsEmptyOfRealPoints || period == TimeSpan.Zero || (accelRatio == 0.0 && decelRatio == 0.0 && !isAutoReversed) || !period.HasTimeSpan || appliedSpeedRatio > (double)period.TimeSpan.Ticks)
		{
			return false;
		}
		MoveFirst();
		long ticks = beginTime.Ticks;
		long num = (long)((double)period.TimeSpan.Ticks / appliedSpeedRatio);
		if (num < 0)
		{
			num = 4611686018427387903L;
		}
		long num2 = 2 * num;
		long num3 = (long)(accelRatio * (double)num);
		long num4 = (long)((1.0 - decelRatio) * (double)num);
		while (_current < _count)
		{
			bool flag = false;
			long num5;
			if (isAutoReversed)
			{
				num5 = (CurrentNodeTime.Ticks - ticks) % num2;
				if (num5 >= num)
				{
					num5 = num2 - num5;
					flag = true;
				}
			}
			else
			{
				num5 = (CurrentNodeTime.Ticks - ticks) % num;
			}
			if ((0 < num5 && num5 < num3) || num4 < num5)
			{
				return true;
			}
			if ((num5 == 0L || num5 == num4) && CurrentNodeIsPoint)
			{
				return true;
			}
			if (CurrentNodeIsInterval)
			{
				if ((num5 == 0L && num3 > 0) || (num5 == num4 && num4 < num))
				{
					return true;
				}
				long num6 = ((!flag) ? (num4 - num5) : (num5 - num3));
				if (CurrentIsAtLastNode || NextNodeTime.Ticks - CurrentNodeTime.Ticks >= num6)
				{
					return true;
				}
			}
			MoveNext();
		}
		return false;
	}

	internal bool IntersectsMultiplePeriods(TimeSpan beginTime, Duration period, double appliedSpeedRatio)
	{
		if (_count < 2 || period == TimeSpan.Zero || !period.HasTimeSpan || appliedSpeedRatio > (double)period.TimeSpan.Ticks)
		{
			return false;
		}
		long num = (long)((double)period.TimeSpan.Ticks / appliedSpeedRatio);
		if (num <= 0)
		{
			return false;
		}
		long num2 = (FirstNodeTime - beginTime).Ticks / num;
		long num3 = (_nodeTime[_count - 1] - beginTime).Ticks / num;
		return num2 != num3;
	}

	internal void ProjectPostFillZone(ref TimeIntervalCollection projection, TimeSpan beginTime, TimeSpan endTime, Duration period, double appliedSpeedRatio, double accelRatio, double decelRatio, bool isAutoReversed)
	{
		long num;
		if (beginTime == endTime)
		{
			num = 0L;
		}
		else
		{
			num = (long)(appliedSpeedRatio * (double)(endTime - beginTime).Ticks);
			if (period.HasTimeSpan)
			{
				long ticks = period.TimeSpan.Ticks;
				if (isAutoReversed)
				{
					long num2 = ticks << 1;
					num %= num2;
					if (num > ticks)
					{
						num = num2 - num;
					}
				}
				else
				{
					num %= ticks;
					if (num == 0L)
					{
						num = ticks;
					}
				}
				if (accelRatio + decelRatio > 0.0)
				{
					double num3 = ticks;
					double num4 = 1.0 / num3;
					double num5 = 1.0 / (2.0 - accelRatio - decelRatio);
					long num6 = (long)(num3 * accelRatio);
					long num7 = ticks - (long)(num3 * decelRatio);
					if (num < num6)
					{
						double num8 = num;
						num = (long)(num5 * num4 * num8 * num8 / accelRatio);
					}
					else if (num <= num7)
					{
						double num8 = num;
						num = (long)(num5 * (2.0 * num8 - accelRatio));
					}
					else
					{
						double num8 = ticks - num;
						num = ticks - (long)(num5 * num4 * num8 * num8 / decelRatio);
					}
				}
			}
		}
		projection.InitializePoint(TimeSpan.FromTicks(num));
	}

	internal void ProjectOntoPeriodicFunction(ref TimeIntervalCollection projection, TimeSpan beginTime, TimeSpan? endTime, Duration fillDuration, Duration period, double appliedSpeedRatio, double accelRatio, double decelRatio, bool isAutoReversed)
	{
		bool containsNullPoint = _containsNullPoint || _nodeTime[0] < beginTime || (endTime.HasValue && fillDuration.HasTimeSpan && (_nodeTime[_count - 1] > endTime.Value + fillDuration.TimeSpan || (_nodeTime[_count - 1] == endTime.Value + fillDuration.TimeSpan && _nodeIsPoint[_count - 1] && (endTime > beginTime || fillDuration.TimeSpan > TimeSpan.Zero))));
		if (endTime.HasValue)
		{
			TimeSpan value = beginTime;
			TimeSpan? timeSpan = endTime;
			if (value == timeSpan)
			{
				projection.InitializePoint(TimeSpan.Zero);
				goto IL_01f6;
			}
		}
		bool flag = !fillDuration.HasTimeSpan || fillDuration.TimeSpan > TimeSpan.Zero;
		if (period.HasTimeSpan)
		{
			TimeIntervalCollection projection2 = default(TimeIntervalCollection);
			ProjectionNormalize(ref projection2, beginTime, endTime, flag, appliedSpeedRatio);
			long ticks = period.TimeSpan.Ticks;
			TimeSpan? activeDuration;
			bool includeMaxPoint;
			if (endTime.HasValue)
			{
				activeDuration = endTime.Value - beginTime;
				includeMaxPoint = flag && activeDuration.Value.Ticks % ticks == 0;
			}
			else
			{
				activeDuration = null;
				includeMaxPoint = false;
			}
			projection.EnsureAllocatedCapacity(4);
			projection2.ProjectionFold(ref projection, activeDuration, ticks, isAutoReversed, includeMaxPoint);
			if (accelRatio + decelRatio > 0.0)
			{
				projection.ProjectionWarp(ticks, accelRatio, decelRatio);
			}
		}
		else
		{
			ProjectionNormalize(ref projection, beginTime, endTime, flag, appliedSpeedRatio);
		}
		goto IL_01f6;
		IL_01f6:
		projection._containsNullPoint = containsNullPoint;
	}

	private void ProjectionNormalize(ref TimeIntervalCollection projection, TimeSpan beginTime, TimeSpan? endTime, bool includeFillPeriod, double speedRatio)
	{
		projection.EnsureAllocatedCapacity(_nodeTime.Length);
		MoveFirst();
		projection.MoveFirst();
		while (!CurrentIsAtLastNode && NextNodeTime <= beginTime)
		{
			MoveNext();
		}
		if (CurrentNodeTime < beginTime)
		{
			if (CurrentNodeIsInterval)
			{
				projection._count++;
				projection.CurrentNodeTime = TimeSpan.Zero;
				projection.CurrentNodeIsPoint = true;
				projection.CurrentNodeIsInterval = true;
				projection.MoveNext();
			}
			MoveNext();
		}
		while (_current < _count)
		{
			if (endTime.HasValue)
			{
				TimeSpan currentNodeTime = CurrentNodeTime;
				TimeSpan? timeSpan = endTime;
				if (!(currentNodeTime < timeSpan))
				{
					break;
				}
			}
			double num = (CurrentNodeTime - beginTime).Ticks;
			projection._count++;
			projection.CurrentNodeTime = TimeSpan.FromTicks((long)(speedRatio * num));
			projection.CurrentNodeIsPoint = CurrentNodeIsPoint;
			projection.CurrentNodeIsInterval = CurrentNodeIsInterval;
			projection.MoveNext();
			MoveNext();
		}
		if (_current < _count && (_nodeIsInterval[_current - 1] || (CurrentNodeTime == endTime.Value && CurrentNodeIsPoint && includeFillPeriod)))
		{
			double num2 = (endTime.Value - beginTime).Ticks;
			projection._count++;
			projection.CurrentNodeTime = TimeSpan.FromTicks((long)(speedRatio * num2));
			projection.CurrentNodeIsPoint = includeFillPeriod && (CurrentNodeTime > endTime.Value || CurrentNodeIsPoint);
			projection.CurrentNodeIsInterval = false;
		}
	}

	private void ProjectionFold(ref TimeIntervalCollection projection, TimeSpan? activeDuration, long periodInTicks, bool isAutoReversed, bool includeMaxPoint)
	{
		MoveFirst();
		bool flag = false;
		do
		{
			if (CurrentNodeIsInterval)
			{
				flag = ProjectionFoldInterval(ref projection, activeDuration, periodInTicks, isAutoReversed, includeMaxPoint);
				_current += (NextNodeIsInterval ? 1 : 2);
			}
			else
			{
				ProjectionFoldPoint(ref projection, activeDuration, periodInTicks, isAutoReversed, includeMaxPoint);
				_current++;
			}
		}
		while (!flag && _current < _count);
	}

	private void ProjectionFoldPoint(ref TimeIntervalCollection projection, TimeSpan? activeDuration, long periodInTicks, bool isAutoReversed, bool includeMaxPoint)
	{
		long num2;
		if (isAutoReversed)
		{
			long num = periodInTicks << 1;
			num2 = CurrentNodeTime.Ticks % num;
			if (num2 > periodInTicks)
			{
				num2 = num - num2;
			}
		}
		else
		{
			if (includeMaxPoint && activeDuration.HasValue)
			{
				TimeSpan currentNodeTime = CurrentNodeTime;
				TimeSpan? timeSpan = activeDuration;
				if (currentNodeTime == timeSpan)
				{
					num2 = periodInTicks;
					goto IL_0069;
				}
			}
			num2 = CurrentNodeTime.Ticks % periodInTicks;
		}
		goto IL_0069;
		IL_0069:
		projection.MergePoint(TimeSpan.FromTicks(num2));
	}

	private bool ProjectionFoldInterval(ref TimeIntervalCollection projection, TimeSpan? activeDuration, long periodInTicks, bool isAutoReversed, bool includeMaxPoint)
	{
		long ticks = (NextNodeTime - CurrentNodeTime).Ticks;
		long num2;
		long num3;
		if (isAutoReversed)
		{
			long num = periodInTicks << 1;
			num2 = CurrentNodeTime.Ticks % num;
			bool flag;
			if (num2 < periodInTicks)
			{
				flag = false;
				num3 = periodInTicks - num2;
			}
			else
			{
				flag = true;
				num2 = num - num2;
				num3 = num2;
			}
			long num4 = ticks - num3;
			if (num4 > 0)
			{
				if (num3 >= num4)
				{
					bool flag2 = CurrentNodeIsPoint;
					if (num3 == num4)
					{
						flag2 = flag2 || NextNodeIsPoint;
					}
					if (flag)
					{
						projection.MergeInterval(TimeSpan.Zero, includeFrom: true, TimeSpan.FromTicks(num2), flag2);
						return flag2 && num2 == periodInTicks;
					}
					projection.MergeInterval(TimeSpan.FromTicks(num2), flag2, TimeSpan.FromTicks(periodInTicks), includeTo: true);
					return flag2 && num2 == 0;
				}
				if (flag)
				{
					long num5 = ((num4 < periodInTicks) ? num4 : periodInTicks);
					projection.MergeInterval(TimeSpan.Zero, includeFrom: true, TimeSpan.FromTicks(num5), NextNodeIsPoint);
					return NextNodeIsPoint && num5 == periodInTicks;
				}
				long num6 = ((num4 < periodInTicks) ? (periodInTicks - num4) : 0);
				projection.MergeInterval(TimeSpan.FromTicks(num6), NextNodeIsPoint, TimeSpan.FromTicks(periodInTicks), includeTo: true);
				return NextNodeIsPoint && num6 == 0;
			}
			if (flag)
			{
				projection.MergeInterval(TimeSpan.FromTicks(num2 - ticks), NextNodeIsPoint, TimeSpan.FromTicks(num2), CurrentNodeIsPoint);
			}
			else
			{
				projection.MergeInterval(TimeSpan.FromTicks(num2), CurrentNodeIsPoint, TimeSpan.FromTicks(num2 + ticks), NextNodeIsPoint);
			}
			return false;
		}
		num2 = CurrentNodeTime.Ticks % periodInTicks;
		num3 = periodInTicks - num2;
		if (ticks > periodInTicks)
		{
			projection._nodeTime[0] = TimeSpan.Zero;
			projection._nodeIsPoint[0] = true;
			projection._nodeIsInterval[0] = true;
			projection._nodeTime[1] = TimeSpan.FromTicks(periodInTicks);
			projection._nodeIsPoint[1] = includeMaxPoint;
			projection._nodeIsInterval[1] = false;
			_count = 2;
			return true;
		}
		if (ticks >= num3)
		{
			projection.MergeInterval(TimeSpan.FromTicks(num2), CurrentNodeIsPoint, TimeSpan.FromTicks(periodInTicks), includeTo: false);
			if (ticks > num3)
			{
				projection.MergeInterval(TimeSpan.Zero, includeFrom: true, TimeSpan.FromTicks(ticks - num3), NextNodeIsPoint);
			}
			else if (NextNodeIsPoint)
			{
				if (includeMaxPoint && activeDuration.HasValue)
				{
					TimeSpan nextNodeTime = NextNodeTime;
					TimeSpan? timeSpan = activeDuration;
					if (nextNodeTime == timeSpan)
					{
						projection.MergePoint(TimeSpan.FromTicks(periodInTicks));
						goto IL_028e;
					}
				}
				projection.MergePoint(TimeSpan.Zero);
			}
			goto IL_028e;
		}
		projection.MergeInterval(TimeSpan.FromTicks(num2), CurrentNodeIsPoint, TimeSpan.FromTicks(num2 + ticks), NextNodeIsPoint);
		return false;
		IL_028e:
		return false;
	}

	private void MergePoint(TimeSpan point)
	{
		int num = Locate(point);
		if (num >= 0 && _nodeTime[num] == point)
		{
			if (_nodeIsPoint[num])
			{
				return;
			}
			if (num == 0 || !_nodeIsInterval[num - 1] || !_nodeIsInterval[num])
			{
				_nodeIsPoint[num] = true;
				return;
			}
			for (int i = num; i + 1 < _count; i++)
			{
				_nodeTime[i] = _nodeTime[i + 1];
				_nodeIsPoint[i] = _nodeIsPoint[i + 1];
				_nodeIsInterval[i] = _nodeIsInterval[i + 1];
			}
			_count--;
		}
		else if (num == -1 || !_nodeIsInterval[num])
		{
			EnsureAllocatedCapacity(_count + 1);
			for (int num2 = _count - 1; num2 > num; num2--)
			{
				_nodeTime[num2 + 1] = _nodeTime[num2];
				_nodeIsPoint[num2 + 1] = _nodeIsPoint[num2];
				_nodeIsInterval[num2 + 1] = _nodeIsInterval[num2];
			}
			_nodeTime[num + 1] = point;
			_nodeIsPoint[num + 1] = true;
			_nodeIsInterval[num + 1] = false;
			_count++;
		}
	}

	private void MergeInterval(TimeSpan from, bool includeFrom, TimeSpan to, bool includeTo)
	{
		if (IsEmptyOfRealPoints)
		{
			_nodeTime[0] = from;
			_nodeIsPoint[0] = includeFrom;
			_nodeIsInterval[0] = true;
			_nodeTime[1] = to;
			_nodeIsPoint[1] = includeTo;
			_nodeIsInterval[1] = false;
			_count = 2;
			return;
		}
		int num = Locate(from);
		int num2 = Locate(to);
		bool flag = false;
		bool flag2 = false;
		int num3 = num - num2;
		int num4 = num + 1;
		int num5 = num2;
		if (num == -1 || _nodeTime[num] < from)
		{
			if (num == -1 || !_nodeIsInterval[num])
			{
				flag = true;
				num3++;
			}
		}
		else if (num > 0 && _nodeIsInterval[num - 1] && (includeFrom || _nodeIsPoint[num]))
		{
			num3--;
			num4--;
		}
		else
		{
			_nodeIsPoint[num] = includeFrom || _nodeIsPoint[num];
		}
		if (num2 == -1 || _nodeTime[num2] < to)
		{
			if (num2 == -1 || !_nodeIsInterval[num2])
			{
				flag2 = true;
				num3++;
			}
		}
		else if (!_nodeIsInterval[num2] || (!includeTo && !_nodeIsPoint[num2]))
		{
			num3++;
			num5--;
			_nodeIsPoint[num2] = includeTo || _nodeIsPoint[num2];
		}
		if (num3 > 0)
		{
			EnsureAllocatedCapacity(_count + num3);
			for (int num6 = _count - 1; num6 > num5; num6--)
			{
				_nodeTime[num6 + num3] = _nodeTime[num6];
				_nodeIsPoint[num6 + num3] = _nodeIsPoint[num6];
				_nodeIsInterval[num6 + num3] = _nodeIsInterval[num6];
			}
		}
		else if (num3 < 0)
		{
			for (int i = num5 + 1; i < _count; i++)
			{
				_nodeTime[i + num3] = _nodeTime[i];
				_nodeIsPoint[i + num3] = _nodeIsPoint[i];
				_nodeIsInterval[i + num3] = _nodeIsInterval[i];
			}
		}
		_count += num3;
		if (flag)
		{
			_nodeTime[num4] = from;
			_nodeIsPoint[num4] = includeFrom;
			_nodeIsInterval[num4] = true;
			num4++;
		}
		if (flag2)
		{
			_nodeTime[num4] = to;
			_nodeIsPoint[num4] = includeTo;
			_nodeIsInterval[num4] = false;
		}
	}

	private void EnsureAllocatedCapacity(int requiredCapacity)
	{
		if (_nodeTime == null)
		{
			_nodeTime = new TimeSpan[requiredCapacity];
			_nodeIsPoint = new bool[requiredCapacity];
			_nodeIsInterval = new bool[requiredCapacity];
		}
		else if (_nodeTime.Length < requiredCapacity)
		{
			int num = _nodeTime.Length << 1;
			TimeSpan[] array = new TimeSpan[num];
			bool[] array2 = new bool[num];
			bool[] array3 = new bool[num];
			for (int i = 0; i < _count; i++)
			{
				array[i] = _nodeTime[i];
				array2[i] = _nodeIsPoint[i];
				array3[i] = _nodeIsInterval[i];
			}
			_nodeTime = array;
			_nodeIsPoint = array2;
			_nodeIsInterval = array3;
		}
	}

	private void ProjectionWarp(long periodInTicks, double accelRatio, double decelRatio)
	{
		double num = periodInTicks;
		double num2 = 1.0 / num;
		double num3 = 1.0 / (2.0 - accelRatio - decelRatio);
		TimeSpan timeSpan = TimeSpan.FromTicks((long)(num * accelRatio));
		TimeSpan timeSpan2 = TimeSpan.FromTicks(periodInTicks - (long)(num * decelRatio));
		MoveFirst();
		while (_current < _count && CurrentNodeTime < timeSpan)
		{
			double num4 = _nodeTime[_current].Ticks;
			_nodeTime[_current] = TimeSpan.FromTicks((long)(num3 * num2 * num4 * num4 / accelRatio));
			MoveNext();
		}
		while (_current < _count && CurrentNodeTime <= timeSpan2)
		{
			double num4 = _nodeTime[_current].Ticks;
			_nodeTime[_current] = TimeSpan.FromTicks((long)(num3 * (2.0 * num4 - accelRatio * num)));
			MoveNext();
		}
		while (_current < _count)
		{
			double num4 = periodInTicks - _nodeTime[_current].Ticks;
			_nodeTime[_current] = TimeSpan.FromTicks(periodInTicks - (long)(num3 * num2 * num4 * num4 / decelRatio));
			MoveNext();
		}
	}

	private int Locate(TimeSpan time)
	{
		if (_count == 0 || time < _nodeTime[0])
		{
			return -1;
		}
		int num = 0;
		int num2 = _count - 1;
		while (num + 1 < num2)
		{
			int num3 = num + num2 >> 1;
			if (time < _nodeTime[num3])
			{
				num2 = num3;
			}
			else
			{
				num = num3;
			}
		}
		if (time < _nodeTime[num2])
		{
			return num;
		}
		return num2;
	}

	private void MoveFirst()
	{
		_current = 0;
	}

	private void MoveNext()
	{
		_current++;
	}

	private void SetInvertedMode(bool mode)
	{
		_invertCollection = mode;
	}
}
