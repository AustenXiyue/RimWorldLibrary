using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Abstract class that, when implemented represents a <see cref="T:System.Windows.Media.Animation.Timeline" /> that may contain a collection of child <see cref="T:System.Windows.Media.Animation.Timeline" /> objects.</summary>
[ContentProperty("Children")]
public abstract class TimelineGroup : Timeline, IAddChild
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Animation.TimelineGroup.Children" /> dependency property.</summary>
	public static readonly DependencyProperty ChildrenProperty;

	internal static TimelineCollection s_Children;

	/// <summary>Gets or sets the collection of direct child <see cref="T:System.Windows.Media.Animation.Timeline" /> objects of the <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.  </summary>
	/// <returns>Child <see cref="T:System.Windows.Media.Animation.Timeline" /> objects of the <see cref="T:System.Windows.Media.Animation.TimelineGroup" />. The default value is null.</returns>
	public TimelineCollection Children
	{
		get
		{
			return (TimelineCollection)GetValue(ChildrenProperty);
		}
		set
		{
			SetValueInternal(ChildrenProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.TimelineGroup" /> class, with default properties. </summary>
	protected TimelineGroup()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.TimelineGroup" /> class with the specified <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" />.</summary>
	/// <param name="beginTime">The <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> for this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</param>
	protected TimelineGroup(TimeSpan? beginTime)
		: base(beginTime)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.TimelineGroup" /> class with the specified <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> and <see cref="P:System.Windows.Media.Animation.Timeline.Duration" />.</summary>
	/// <param name="beginTime">The <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> for this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</param>
	/// <param name="duration">The <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> for this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</param>
	protected TimelineGroup(TimeSpan? beginTime, Duration duration)
		: base(beginTime, duration)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.TimelineGroup" /> class with the specified <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" />, <see cref="P:System.Windows.Media.Animation.Timeline.Duration" />, and <see cref="P:System.Windows.Media.Animation.Timeline.RepeatBehavior" />.</summary>
	/// <param name="beginTime">The <see cref="P:System.Windows.Media.Animation.Timeline.BeginTime" /> for this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</param>
	/// <param name="duration">The <see cref="P:System.Windows.Media.Animation.Timeline.Duration" /> for this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</param>
	/// <param name="repeatBehavior">The <see cref="P:System.Windows.Media.Animation.Timeline.RepeatBehavior" /> for this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</param>
	protected TimelineGroup(TimeSpan? beginTime, Duration duration, RepeatBehavior repeatBehavior)
		: base(beginTime, duration, repeatBehavior)
	{
	}

	/// <summary>Creates a type-specific clock for this timeline.</summary>
	/// <returns>A clock for this timeline.</returns>
	protected internal override Clock AllocateClock()
	{
		return new ClockGroup(this);
	}

	/// <summary>Instantiates a new <see cref="T:System.Windows.Media.Animation.ClockGroup" /> object, using this instance. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Animation.ClockGroup" /> object.</returns>
	public new ClockGroup CreateClock()
	{
		return (ClockGroup)base.CreateClock();
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

	/// <summary>Adds a child <see cref="T:System.Windows.Media.Animation.Timeline" /> to this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />.</summary>
	/// <param name="child">The object to be added as the child of this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />. If this object is a <see cref="T:System.Windows.Media.Animation.Timeline" /> it will be added to the <see cref="P:System.Windows.Media.Animation.TimelineGroup.Children" /> collection; otherwise, an exception will be thrown.</param>
	/// <exception cref="T:System.ArgumentException">The parameter <paramref name="child" /> is not a <see cref="T:System.Windows.Media.Animation.Timeline" />.</exception>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	protected virtual void AddChild(object child)
	{
		if (!(child is Timeline value))
		{
			throw new ArgumentException(SR.Timing_ChildMustBeTimeline, "child");
		}
		Children.Add(value);
	}

	/// <summary>Adds the text content of a node to the object. </summary>
	/// <param name="childText">The text to add to the object.</param>
	void IAddChild.AddText(string childText)
	{
		WritePreamble();
		if (childText == null)
		{
			throw new ArgumentNullException("childText");
		}
		AddText(childText);
		WritePostscript();
	}

	/// <summary>Adds a text string as a child of this <see cref="T:System.Windows.Media.Animation.Timeline" />.</summary>
	/// <param name="childText">The text added to the <see cref="T:System.Windows.Media.Animation.Timeline" />.</param>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	protected virtual void AddText(string childText)
	{
		throw new InvalidOperationException(SR.Timing_NoTextChildren);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.TimelineGroup" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TimelineGroup Clone()
	{
		return (TimelineGroup)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.TimelineGroup" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TimelineGroup CloneCurrentValue()
	{
		return (TimelineGroup)base.CloneCurrentValue();
	}

	static TimelineGroup()
	{
		s_Children = TimelineCollection.Empty;
		Type typeFromHandle = typeof(TimelineGroup);
		ChildrenProperty = Animatable.RegisterProperty("Children", typeof(TimelineCollection), typeFromHandle, new FreezableDefaultValueFactory(TimelineCollection.Empty), null, null, isIndependentlyAnimated: false, null);
	}
}
