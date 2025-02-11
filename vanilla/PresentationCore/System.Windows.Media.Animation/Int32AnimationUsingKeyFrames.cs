using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary> Animates the value of a <see cref="T:System.Int32" /> property along a set of <see cref="P:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames.KeyFrames" />.</summary>
[ContentProperty("KeyFrames")]
public class Int32AnimationUsingKeyFrames : Int32AnimationBase, IKeyFrameAnimation, IAddChild
{
	private struct KeyTimeBlock
	{
		public int BeginIndex;

		public int EndIndex;
	}

	private Int32KeyFrameCollection _keyFrames;

	private ResolvedKeyFrameEntry[] _sortedResolvedKeyFrames;

	private bool _areKeyTimesValid;

	/// <summary>Gets or sets an ordered collection P:System.Windows.Media.Animation.IKeyFrameAnimation.KeyFrames associated with this animation sequence.</summary>
	/// <returns>An <see cref="T:System.Collections.IList" /> of <see cref="P:System.Windows.Media.Animation.IKeyFrameAnimation.KeyFrames" />.</returns>
	IList IKeyFrameAnimation.KeyFrames
	{
		get
		{
			return KeyFrames;
		}
		set
		{
			KeyFrames = (Int32KeyFrameCollection)value;
		}
	}

	/// <summary> Gets or sets the collection of <see cref="T:System.Windows.Media.Animation.Int32KeyFrame" /> objects that define the animation. </summary>
	/// <returns>The collection of <see cref="T:System.Windows.Media.Animation.Int32KeyFrame" /> objects that define the animation. The default value is <see cref="P:System.Windows.Media.Animation.Int32KeyFrameCollection.Empty" />.</returns>
	public Int32KeyFrameCollection KeyFrames
	{
		get
		{
			ReadPreamble();
			if (_keyFrames == null)
			{
				if (base.IsFrozen)
				{
					_keyFrames = Int32KeyFrameCollection.Empty;
				}
				else
				{
					WritePreamble();
					_keyFrames = new Int32KeyFrameCollection();
					OnFreezablePropertyChanged(null, _keyFrames);
					WritePostscript();
				}
			}
			return _keyFrames;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			WritePreamble();
			if (value != _keyFrames)
			{
				OnFreezablePropertyChanged(_keyFrames, value);
				_keyFrames = value;
				WritePostscript();
			}
		}
	}

	/// <summary>Gets a value that specifies whether the animation's output value is added to the base value of the property being animated.  </summary>
	/// <returns>true if the animation adds its output value to the base value of the property being animated instead of replacing it; otherwise, false. The default value is false.</returns>
	public bool IsAdditive
	{
		get
		{
			return (bool)GetValue(AnimationTimeline.IsAdditiveProperty);
		}
		set
		{
			SetValueInternal(AnimationTimeline.IsAdditiveProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets a value that specifies whether the animation's value accumulates when it repeats.</summary>
	/// <returns>true if the animation accumulates its values when its <see cref="P:System.Windows.Media.Animation.Timeline.RepeatBehavior" /> property causes it to repeat its simple duration; otherwise, false. The default value is false.</returns>
	public bool IsCumulative
	{
		get
		{
			return (bool)GetValue(AnimationTimeline.IsCumulativeProperty);
		}
		set
		{
			SetValueInternal(AnimationTimeline.IsCumulativeProperty, BooleanBoxes.Box(value));
		}
	}

	private TimeSpan LargestTimeSpanKeyTime
	{
		get
		{
			bool flag = false;
			TimeSpan timeSpan = TimeSpan.Zero;
			if (_keyFrames != null)
			{
				int count = _keyFrames.Count;
				for (int i = 0; i < count; i++)
				{
					KeyTime keyTime = _keyFrames[i].KeyTime;
					if (keyTime.Type == KeyTimeType.TimeSpan)
					{
						flag = true;
						if (keyTime.TimeSpan > timeSpan)
						{
							timeSpan = keyTime.TimeSpan;
						}
					}
				}
			}
			if (flag)
			{
				return timeSpan;
			}
			return TimeSpan.FromSeconds(1.0);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" /> class.</summary>
	public Int32AnimationUsingKeyFrames()
	{
		_areKeyTimesValid = true;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Int32AnimationUsingKeyFrames Clone()
	{
		return (Int32AnimationUsingKeyFrames)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Int32AnimationUsingKeyFrames CloneCurrentValue()
	{
		return (Int32AnimationUsingKeyFrames)base.CloneCurrentValue();
	}

	/// <summary>Makes this instance of <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" /> object unmodifiable or determines whether it can be made unmodifiable..</summary>
	/// <returns>If <paramref name="isChecking" /> is true, this method returns true if this instance can be made read-only, or false if it cannot be made read-only. If <paramref name="isChecking" /> is false, this method returns true if this instance is now read-only, or false if it cannot be made read-only, with the side effect of having begun to change the frozen status of this object.</returns>
	/// <param name="isChecking">true if this method should simply determine whether this instance can be frozen. false if this instance should actually freeze itself when this method is called.</param>
	protected override bool FreezeCore(bool isChecking)
	{
		bool num = base.FreezeCore(isChecking) & Freezable.Freeze(_keyFrames, isChecking);
		if (num & !_areKeyTimesValid)
		{
			ResolveKeyTimes();
		}
		return num;
	}

	/// <summary>Called when the current <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" /> object is modified.</summary>
	protected override void OnChanged()
	{
		_areKeyTimesValid = false;
		base.OnChanged();
	}

	/// <summary>Creates a new instance of <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" />. </summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" />.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new Int32AnimationUsingKeyFrames();
	}

	/// <summary>Makes this instance a deep copy of the specified <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" />. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" /> to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		Int32AnimationUsingKeyFrames sourceAnimation = (Int32AnimationUsingKeyFrames)sourceFreezable;
		base.CloneCore(sourceFreezable);
		CopyCommon(sourceAnimation, isCurrentValueClone: false);
	}

	/// <summary>Makes this instance a modifiable deep copy of the specified <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" /> using current property values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" /> to clone.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		Int32AnimationUsingKeyFrames sourceAnimation = (Int32AnimationUsingKeyFrames)sourceFreezable;
		base.CloneCurrentValueCore(sourceFreezable);
		CopyCommon(sourceAnimation, isCurrentValueClone: true);
	}

	/// <summary>Makes this instance a clone of the specified <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" /> object.</summary>
	/// <param name="source">The <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" /> object to clone.</param>
	protected override void GetAsFrozenCore(Freezable source)
	{
		Int32AnimationUsingKeyFrames sourceAnimation = (Int32AnimationUsingKeyFrames)source;
		base.GetAsFrozenCore(source);
		CopyCommon(sourceAnimation, isCurrentValueClone: false);
	}

	/// <summary>Makes this instance a frozen clone of the specified <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" />. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="source">The <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" /> to copy and freeze.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable source)
	{
		Int32AnimationUsingKeyFrames sourceAnimation = (Int32AnimationUsingKeyFrames)source;
		base.GetCurrentValueAsFrozenCore(source);
		CopyCommon(sourceAnimation, isCurrentValueClone: true);
	}

	private void CopyCommon(Int32AnimationUsingKeyFrames sourceAnimation, bool isCurrentValueClone)
	{
		_areKeyTimesValid = sourceAnimation._areKeyTimesValid;
		if (_areKeyTimesValid && sourceAnimation._sortedResolvedKeyFrames != null)
		{
			_sortedResolvedKeyFrames = (ResolvedKeyFrameEntry[])sourceAnimation._sortedResolvedKeyFrames.Clone();
		}
		if (sourceAnimation._keyFrames != null)
		{
			if (isCurrentValueClone)
			{
				_keyFrames = (Int32KeyFrameCollection)sourceAnimation._keyFrames.CloneCurrentValue();
			}
			else
			{
				_keyFrames = sourceAnimation._keyFrames.Clone();
			}
			OnFreezablePropertyChanged(null, _keyFrames);
		}
	}

	/// <summary>Adds a child object.</summary>
	/// <param name="child">The child object to add.</param>
	void IAddChild.AddChild(object child)
	{
		WritePreamble();
		if (child == null)
		{
			throw new ArgumentNullException("child");
		}
		AddChild(child);
		WritePostscript();
	}

	/// <summary>Adds a child <see cref="T:System.Windows.Media.Animation.Int32KeyFrame" /> to this <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" />. </summary>
	/// <param name="child">The object to be added as the child of this <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" />. </param>
	/// <exception cref="T:System.ArgumentException">The parameter <paramref name="child" /> is not a <see cref="T:System.Windows.Media.Animation.Int32KeyFrame" />.</exception>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	protected virtual void AddChild(object child)
	{
		if (child is Int32KeyFrame keyFrame)
		{
			KeyFrames.Add(keyFrame);
			return;
		}
		throw new ArgumentException(SR.Animation_ChildMustBeKeyFrame, "child");
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="childText">The text to add to the object.</param>
	void IAddChild.AddText(string childText)
	{
		if (childText == null)
		{
			throw new ArgumentNullException("childText");
		}
		AddText(childText);
	}

	/// <summary>Adds a text string as a child of this <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" />.</summary>
	/// <param name="childText">The text added to the <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" />.</param>
	/// <exception cref="T:System.InvalidOperationException">A <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" /> does not accept text as a child, so this method will raise this exception unless a derived class has overridden this behavior which allows text to be added.</exception>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	protected virtual void AddText(string childText)
	{
		throw new InvalidOperationException(SR.Animation_NoTextChildren);
	}

	/// <summary> Calculates a value that represents the current value of the property being animated, as determined by this instance of <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" />.</summary>
	/// <returns>The calculated value of the property, as determined by the current instance.</returns>
	/// <param name="defaultOriginValue">The suggested origin value, used if the animation does not have its own explicitly set start value.</param>
	/// <param name="defaultDestinationValue">The suggested destination value, used if the animation does not have its own explicitly set end value.</param>
	/// <param name="animationClock">An <see cref="T:System.Windows.Media.Animation.AnimationClock" /> that generates the <see cref="P:System.Windows.Media.Animation.Clock.CurrentTime" /> or <see cref="P:System.Windows.Media.Animation.Clock.CurrentProgress" /> used by the host animation.</param>
	protected sealed override int GetCurrentValueCore(int defaultOriginValue, int defaultDestinationValue, AnimationClock animationClock)
	{
		if (_keyFrames == null)
		{
			return defaultDestinationValue;
		}
		if (!_areKeyTimesValid)
		{
			ResolveKeyTimes();
		}
		if (_sortedResolvedKeyFrames == null)
		{
			return defaultDestinationValue;
		}
		TimeSpan value = animationClock.CurrentTime.Value;
		int num = _sortedResolvedKeyFrames.Length;
		int num2 = num - 1;
		int i;
		for (i = 0; i < num && value > _sortedResolvedKeyFrames[i]._resolvedKeyTime; i++)
		{
		}
		for (; i < num2 && value == _sortedResolvedKeyFrames[i + 1]._resolvedKeyTime; i++)
		{
		}
		int num3;
		if (i == num)
		{
			num3 = GetResolvedKeyFrameValue(num2);
		}
		else if (value == _sortedResolvedKeyFrames[i]._resolvedKeyTime)
		{
			num3 = GetResolvedKeyFrameValue(i);
		}
		else
		{
			double num4 = 0.0;
			int baseValue;
			if (i == 0)
			{
				baseValue = ((!IsAdditive) ? defaultOriginValue : AnimatedTypeHelpers.GetZeroValueInt32(defaultOriginValue));
				num4 = value.TotalMilliseconds / _sortedResolvedKeyFrames[0]._resolvedKeyTime.TotalMilliseconds;
			}
			else
			{
				int num5 = i - 1;
				TimeSpan resolvedKeyTime = _sortedResolvedKeyFrames[num5]._resolvedKeyTime;
				baseValue = GetResolvedKeyFrameValue(num5);
				TimeSpan timeSpan = value - resolvedKeyTime;
				TimeSpan timeSpan2 = _sortedResolvedKeyFrames[i]._resolvedKeyTime - resolvedKeyTime;
				num4 = timeSpan.TotalMilliseconds / timeSpan2.TotalMilliseconds;
			}
			num3 = GetResolvedKeyFrame(i).InterpolateValue(baseValue, num4);
		}
		if (IsCumulative)
		{
			double num6 = (animationClock.CurrentIteration - 1).Value;
			if (num6 > 0.0)
			{
				num3 = AnimatedTypeHelpers.AddInt32(num3, AnimatedTypeHelpers.ScaleInt32(GetResolvedKeyFrameValue(num2), num6));
			}
		}
		if (IsAdditive)
		{
			return AnimatedTypeHelpers.AddInt32(defaultOriginValue, num3);
		}
		return num3;
	}

	/// <summary>Provide a custom natural <see cref="T:System.Windows.Duration" /> when the <see cref="T:System.Windows.Duration" /> property is set to <see cref="P:System.Windows.Duration.Automatic" />. </summary>
	/// <returns>If the last key frame of this animation is a <see cref="T:System.Windows.Media.Animation.KeyTime" />, then this value is used as the <see cref="P:System.Windows.Media.Animation.Clock.NaturalDuration" />; otherwise it will be one second.</returns>
	/// <param name="clock">The <see cref="T:System.Windows.Media.Animation.Clock" /> whose natural duration is desired.</param>
	protected sealed override Duration GetNaturalDurationCore(Clock clock)
	{
		return new Duration(LargestTimeSpanKeyTime);
	}

	/// <summary>Returns true if the value of the <see cref="P:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames.KeyFrames" /> property of this instance of <see cref="T:System.Windows.Media.Animation.Int32AnimationUsingKeyFrames" /> should be value-serialized.</summary>
	/// <returns>true if the property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeKeyFrames()
	{
		ReadPreamble();
		if (_keyFrames != null)
		{
			return _keyFrames.Count > 0;
		}
		return false;
	}

	private int GetResolvedKeyFrameValue(int resolvedKeyFrameIndex)
	{
		return GetResolvedKeyFrame(resolvedKeyFrameIndex).Value;
	}

	private Int32KeyFrame GetResolvedKeyFrame(int resolvedKeyFrameIndex)
	{
		return _keyFrames[_sortedResolvedKeyFrames[resolvedKeyFrameIndex]._originalKeyFrameIndex];
	}

	private void ResolveKeyTimes()
	{
		int num = 0;
		if (_keyFrames != null)
		{
			num = _keyFrames.Count;
		}
		if (num == 0)
		{
			_sortedResolvedKeyFrames = null;
			_areKeyTimesValid = true;
			return;
		}
		_sortedResolvedKeyFrames = new ResolvedKeyFrameEntry[num];
		int i;
		for (i = 0; i < num; i++)
		{
			_sortedResolvedKeyFrames[i]._originalKeyFrameIndex = i;
		}
		TimeSpan zero = TimeSpan.Zero;
		Duration duration = base.Duration;
		zero = ((!duration.HasTimeSpan) ? LargestTimeSpanKeyTime : duration.TimeSpan);
		int num2 = num - 1;
		List<KeyTimeBlock> list = new List<KeyTimeBlock>();
		bool flag = false;
		i = 0;
		while (i < num)
		{
			KeyTime keyTime = _keyFrames[i].KeyTime;
			switch (keyTime.Type)
			{
			case KeyTimeType.Percent:
				_sortedResolvedKeyFrames[i]._resolvedKeyTime = TimeSpan.FromMilliseconds(keyTime.Percent * zero.TotalMilliseconds);
				i++;
				break;
			case KeyTimeType.TimeSpan:
				_sortedResolvedKeyFrames[i]._resolvedKeyTime = keyTime.TimeSpan;
				i++;
				break;
			case KeyTimeType.Uniform:
			case KeyTimeType.Paced:
			{
				if (i == num2)
				{
					_sortedResolvedKeyFrames[i]._resolvedKeyTime = zero;
					i++;
					break;
				}
				if (i == 0 && keyTime.Type == KeyTimeType.Paced)
				{
					_sortedResolvedKeyFrames[i]._resolvedKeyTime = TimeSpan.Zero;
					i++;
					break;
				}
				if (keyTime.Type == KeyTimeType.Paced)
				{
					flag = true;
				}
				KeyTimeBlock item = default(KeyTimeBlock);
				item.BeginIndex = i;
				while (++i < num2)
				{
					switch (_keyFrames[i].KeyTime.Type)
					{
					case KeyTimeType.Paced:
						flag = true;
						continue;
					default:
						continue;
					case KeyTimeType.Percent:
					case KeyTimeType.TimeSpan:
						break;
					}
					break;
				}
				item.EndIndex = i;
				list.Add(item);
				break;
			}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			KeyTimeBlock keyTimeBlock = list[j];
			TimeSpan timeSpan = TimeSpan.Zero;
			if (keyTimeBlock.BeginIndex > 0)
			{
				timeSpan = _sortedResolvedKeyFrames[keyTimeBlock.BeginIndex - 1]._resolvedKeyTime;
			}
			long num3 = keyTimeBlock.EndIndex - keyTimeBlock.BeginIndex + 1;
			TimeSpan timeSpan2 = TimeSpan.FromTicks((_sortedResolvedKeyFrames[keyTimeBlock.EndIndex]._resolvedKeyTime - timeSpan).Ticks / num3);
			i = keyTimeBlock.BeginIndex;
			TimeSpan resolvedKeyTime = timeSpan + timeSpan2;
			for (; i < keyTimeBlock.EndIndex; i++)
			{
				_sortedResolvedKeyFrames[i]._resolvedKeyTime = resolvedKeyTime;
				resolvedKeyTime += timeSpan2;
			}
		}
		if (flag)
		{
			ResolvePacedKeyTimes();
		}
		Array.Sort(_sortedResolvedKeyFrames);
		_areKeyTimesValid = true;
	}

	private void ResolvePacedKeyTimes()
	{
		int num = 1;
		int num2 = _sortedResolvedKeyFrames.Length - 1;
		do
		{
			if (_keyFrames[num].KeyTime.Type == KeyTimeType.Paced)
			{
				int num3 = num;
				List<double> list = new List<double>();
				TimeSpan resolvedKeyTime = _sortedResolvedKeyFrames[num - 1]._resolvedKeyTime;
				double num4 = 0.0;
				int from = _keyFrames[num - 1].Value;
				do
				{
					int value = _keyFrames[num].Value;
					num4 += AnimatedTypeHelpers.GetSegmentLengthInt32(from, value);
					list.Add(num4);
					from = value;
					num++;
				}
				while (num < num2 && _keyFrames[num].KeyTime.Type == KeyTimeType.Paced);
				num4 += AnimatedTypeHelpers.GetSegmentLengthInt32(from, _keyFrames[num].Value);
				TimeSpan timeSpan = _sortedResolvedKeyFrames[num]._resolvedKeyTime - resolvedKeyTime;
				int num5 = 0;
				int num6 = num3;
				while (num5 < list.Count)
				{
					_sortedResolvedKeyFrames[num6]._resolvedKeyTime = resolvedKeyTime + TimeSpan.FromMilliseconds(list[num5] / num4 * timeSpan.TotalMilliseconds);
					num5++;
					num6++;
				}
			}
			else
			{
				num++;
			}
		}
		while (num < num2);
	}
}
