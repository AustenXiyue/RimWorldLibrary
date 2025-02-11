using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MS.Internal;

namespace System.Windows;

/// <summary>Manages states and the logic for transitioning between states for controls.</summary>
public class VisualStateManager : DependencyObject
{
	private struct TimelineDataToken : IEquatable<TimelineDataToken>
	{
		private DependencyObject _target;

		private string _targetName;

		private PropertyPath _targetProperty;

		public TimelineDataToken(Timeline timeline)
		{
			_target = Storyboard.GetTarget(timeline);
			_targetName = Storyboard.GetTargetName(timeline);
			_targetProperty = Storyboard.GetTargetProperty(timeline);
		}

		public bool Equals(TimelineDataToken other)
		{
			bool flag = false;
			flag = ((_targetName != null) ? (other._targetName == _targetName) : ((_target == null) ? (other._target == null && other._targetName == null) : (other._target == _target)));
			if (flag && other._targetProperty.Path == _targetProperty.Path && other._targetProperty.PathParameters.Count == _targetProperty.PathParameters.Count)
			{
				bool result = true;
				int i = 0;
				for (int count = _targetProperty.PathParameters.Count; i < count; i++)
				{
					if (other._targetProperty.PathParameters[i] != _targetProperty.PathParameters[i])
					{
						result = false;
						break;
					}
				}
				return result;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = ((_target != null) ? _target.GetHashCode() : 0);
			int num2 = ((_targetName != null) ? _targetName.GetHashCode() : 0);
			int num3 = ((_targetProperty != null && _targetProperty.Path != null) ? _targetProperty.Path.GetHashCode() : 0);
			return ((_targetName != null) ? num2 : num) ^ num3;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.VisualStateManager.CustomVisualStateManager" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.VisualStateManager.CustomVisualStateManager" /> dependency property.</returns>
	public static readonly DependencyProperty CustomVisualStateManagerProperty = DependencyProperty.RegisterAttached("CustomVisualStateManager", typeof(VisualStateManager), typeof(VisualStateManager), null);

	private static readonly DependencyPropertyKey VisualStateGroupsPropertyKey = DependencyProperty.RegisterAttachedReadOnly("VisualStateGroups", typeof(IList), typeof(VisualStateManager), new FrameworkPropertyMetadata(new ObservableCollectionDefaultValueFactory<VisualStateGroup>()));

	/// <summary>Identifies the <see cref="P:System.Windows.VisualStateManager.VisualStateGroups" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.VisualStateManager.VisualStateGroups" /> dependency property.</returns>
	public static readonly DependencyProperty VisualStateGroupsProperty = VisualStateGroupsPropertyKey.DependencyProperty;

	private static readonly Duration DurationZero = new Duration(TimeSpan.Zero);

	private static bool GoToStateCommon(FrameworkElement control, FrameworkElement stateGroupsRoot, string stateName, bool useTransitions)
	{
		if (stateName == null)
		{
			throw new ArgumentNullException("stateName");
		}
		if (stateGroupsRoot == null)
		{
			return false;
		}
		IList<VisualStateGroup> visualStateGroupsInternal = GetVisualStateGroupsInternal(stateGroupsRoot);
		if (visualStateGroupsInternal == null)
		{
			return false;
		}
		TryGetState(visualStateGroupsInternal, stateName, out var group, out var state);
		VisualStateManager customVisualStateManager = GetCustomVisualStateManager(stateGroupsRoot);
		if (customVisualStateManager != null)
		{
			return customVisualStateManager.GoToStateCore(control, stateGroupsRoot, stateName, group, state, useTransitions);
		}
		if (state != null)
		{
			return GoToStateInternal(control, stateGroupsRoot, group, state, useTransitions);
		}
		return false;
	}

	/// <summary>Transitions the control between two states. Use this method to transition states on control that has a <see cref="T:System.Windows.Controls.ControlTemplate" />.</summary>
	/// <returns>true if the control successfully transitioned to the new state; otherwise, false.</returns>
	/// <param name="control">The control to transition between states. </param>
	/// <param name="stateName">The state to transition to.</param>
	/// <param name="useTransitions">true to use a <see cref="T:System.Windows.VisualTransition" /> object to transition between states; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="control" /> is null.-or-<paramref name="stateName" /> is null.</exception>
	public static bool GoToState(FrameworkElement control, string stateName, bool useTransitions)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		FrameworkElement stateGroupsRoot = control.StateGroupsRoot;
		return GoToStateCommon(control, stateGroupsRoot, stateName, useTransitions);
	}

	/// <summary>Transitions the element between two states. Use this method to transition states that are defined by an application, rather than defined by a control.</summary>
	/// <returns>true if the control successfully transitioned to the new state; otherwise, false.</returns>
	/// <param name="stateGroupsRoot">The root element that contains the <see cref="T:System.Windows.VisualStateManager" />.</param>
	/// <param name="stateName">The state to transition to.</param>
	/// <param name="useTransitions">true to use a <see cref="T:System.Windows.VisualTransition" /> object to transition between states; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stateGroupsRoot" /> is null.-or-<paramref name="stateName" /> is null.</exception>
	public static bool GoToElementState(FrameworkElement stateGroupsRoot, string stateName, bool useTransitions)
	{
		if (stateGroupsRoot == null)
		{
			throw new ArgumentNullException("stateGroupsRoot");
		}
		return GoToStateCommon(null, stateGroupsRoot, stateName, useTransitions);
	}

	/// <summary>Transitions a control between states.</summary>
	/// <returns>true if the control successfully transitioned to the new state; otherwise, false.</returns>
	/// <param name="control">The control to transition between states. </param>
	/// <param name="stateGroupsRoot">The root element that contains the <see cref="T:System.Windows.VisualStateManager" />.</param>
	/// <param name="stateName">The name of the state to transition to.</param>
	/// <param name="group">The <see cref="T:System.Windows.VisualStateGroup" /> that the state belongs to.</param>
	/// <param name="state">The representation of the state to transition to.</param>
	/// <param name="useTransitions">true to use a <see cref="T:System.Windows.VisualTransition" /> object to transition between states; otherwise, false.</param>
	protected virtual bool GoToStateCore(FrameworkElement control, FrameworkElement stateGroupsRoot, string stateName, VisualStateGroup group, VisualState state, bool useTransitions)
	{
		return GoToStateInternal(control, stateGroupsRoot, group, state, useTransitions);
	}

	/// <summary>Gets the <see cref="P:System.Windows.VisualStateManager.CustomVisualStateManager" /> attached property.</summary>
	/// <returns>The visual state manager that transitions between the states of a control. </returns>
	/// <param name="obj">The element to get the <see cref="P:System.Windows.VisualStateManager.CustomVisualStateManager" /> attached property from.</param>
	public static VisualStateManager GetCustomVisualStateManager(FrameworkElement obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		return obj.GetValue(CustomVisualStateManagerProperty) as VisualStateManager;
	}

	/// <summary>Sets the <see cref="P:System.Windows.VisualStateManager.CustomVisualStateManager" /> attached property.</summary>
	/// <param name="obj">The object to set the property on.</param>
	/// <param name="value">The visual state manager that transitions between the states of a control.</param>
	public static void SetCustomVisualStateManager(FrameworkElement obj, VisualStateManager value)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		obj.SetValue(CustomVisualStateManagerProperty, value);
	}

	internal static Collection<VisualStateGroup> GetVisualStateGroupsInternal(FrameworkElement obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (obj.GetValueSource(VisualStateGroupsProperty, null, out var _) != BaseValueSourceInternal.Default)
		{
			return obj.GetValue(VisualStateGroupsProperty) as Collection<VisualStateGroup>;
		}
		return null;
	}

	/// <summary>Gets the <see cref="P:System.Windows.VisualStateManager.VisualStateGroups" /> attached property.</summary>
	/// <returns>The collection of <see cref="T:System.Windows.VisualStateGroup" /> objects that is associated with the specified object.</returns>
	/// <param name="obj">The element to get the <see cref="P:System.Windows.VisualStateManager.VisualStateGroups" /> attached property from.</param>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public static IList GetVisualStateGroups(FrameworkElement obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		return obj.GetValue(VisualStateGroupsProperty) as IList;
	}

	internal static bool TryGetState(IList<VisualStateGroup> groups, string stateName, out VisualStateGroup group, out VisualState state)
	{
		for (int i = 0; i < groups.Count; i++)
		{
			VisualStateGroup visualStateGroup = groups[i];
			VisualState state2 = visualStateGroup.GetState(stateName);
			if (state2 != null)
			{
				group = visualStateGroup;
				state = state2;
				return true;
			}
		}
		group = null;
		state = null;
		return false;
	}

	private static bool GoToStateInternal(FrameworkElement control, FrameworkElement stateGroupsRoot, VisualStateGroup group, VisualState state, bool useTransitions)
	{
		if (stateGroupsRoot == null)
		{
			throw new ArgumentNullException("stateGroupsRoot");
		}
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		if (group == null)
		{
			throw new InvalidOperationException();
		}
		VisualState lastState = group.CurrentState;
		if (lastState == state)
		{
			return true;
		}
		VisualTransition transition = (useTransitions ? GetTransition(stateGroupsRoot, group, lastState, state) : null);
		Storyboard storyboard = GenerateDynamicTransitionAnimations(stateGroupsRoot, group, state, transition);
		if (transition == null || (transition.GeneratedDuration == DurationZero && (transition.Storyboard == null || transition.Storyboard.Duration == DurationZero)))
		{
			if (transition != null && transition.Storyboard != null)
			{
				group.StartNewThenStopOld(stateGroupsRoot, transition.Storyboard, state.Storyboard);
			}
			else
			{
				group.StartNewThenStopOld(stateGroupsRoot, state.Storyboard);
			}
			group.RaiseCurrentStateChanging(stateGroupsRoot, lastState, state, control);
			group.RaiseCurrentStateChanged(stateGroupsRoot, lastState, state, control);
		}
		else
		{
			transition.DynamicStoryboardCompleted = false;
			storyboard.Completed += delegate
			{
				if (transition.Storyboard == null || transition.ExplicitStoryboardCompleted)
				{
					if (ShouldRunStateStoryboard(control, stateGroupsRoot, state, group))
					{
						group.StartNewThenStopOld(stateGroupsRoot, state.Storyboard);
					}
					group.RaiseCurrentStateChanged(stateGroupsRoot, lastState, state, control);
				}
				transition.DynamicStoryboardCompleted = true;
			};
			if (transition.Storyboard != null && transition.ExplicitStoryboardCompleted)
			{
				EventHandler transitionCompleted = null;
				transitionCompleted = delegate
				{
					if (transition.DynamicStoryboardCompleted)
					{
						if (ShouldRunStateStoryboard(control, stateGroupsRoot, state, group))
						{
							group.StartNewThenStopOld(stateGroupsRoot, state.Storyboard);
						}
						group.RaiseCurrentStateChanged(stateGroupsRoot, lastState, state, control);
					}
					transition.Storyboard.Completed -= transitionCompleted;
					transition.ExplicitStoryboardCompleted = true;
				};
				transition.ExplicitStoryboardCompleted = false;
				transition.Storyboard.Completed += transitionCompleted;
			}
			group.StartNewThenStopOld(stateGroupsRoot, transition.Storyboard, storyboard);
			group.RaiseCurrentStateChanging(stateGroupsRoot, lastState, state, control);
		}
		group.CurrentState = state;
		return true;
	}

	private static bool ShouldRunStateStoryboard(FrameworkElement control, FrameworkElement stateGroupsRoot, VisualState state, VisualStateGroup group)
	{
		bool flag = true;
		bool flag2 = true;
		if (control != null && !control.IsVisible)
		{
			flag = PresentationSource.CriticalFromVisual(control) != null;
		}
		if (stateGroupsRoot != null && !stateGroupsRoot.IsVisible)
		{
			flag2 = PresentationSource.CriticalFromVisual(stateGroupsRoot) != null;
		}
		if (flag && flag2)
		{
			return state == group.CurrentState;
		}
		return false;
	}

	/// <summary>Raises the <see cref="E:System.Windows.VisualStateGroup.CurrentStateChanging" /> event on the specified <see cref="T:System.Windows.VisualStateGroup" /> object.</summary>
	/// <param name="stateGroup">The object that the <see cref="E:System.Windows.VisualStateGroup.CurrentStateChanging" /> event occurred on.</param>
	/// <param name="oldState">The state that the control is transitioning from.</param>
	/// <param name="newState">The state that the control is transitioning to.</param>
	/// <param name="control">The control that is transitioning states.</param>
	/// <param name="stateGroupsRoot">The root element that contains the <see cref="T:System.Windows.VisualStateManager" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stateGroupsRoot" /> is null.-or-<paramref name="newState" /> is null.</exception>
	protected void RaiseCurrentStateChanging(VisualStateGroup stateGroup, VisualState oldState, VisualState newState, FrameworkElement control, FrameworkElement stateGroupsRoot)
	{
		if (stateGroup == null)
		{
			throw new ArgumentNullException("stateGroup");
		}
		if (newState == null)
		{
			throw new ArgumentNullException("newState");
		}
		if (stateGroupsRoot != null)
		{
			stateGroup.RaiseCurrentStateChanging(stateGroupsRoot, oldState, newState, control);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.VisualStateGroup.CurrentStateChanging" /> event on the specified <see cref="T:System.Windows.VisualStateGroup" /> object.</summary>
	/// <param name="stateGroup">The object that the <see cref="E:System.Windows.VisualStateGroup.CurrentStateChanging" /> event occurred on.</param>
	/// <param name="oldState">The state that the control is transitioning from.</param>
	/// <param name="newState">The state that the control is transitioning to.</param>
	/// <param name="control">The control that is transitioning states.</param>
	/// <param name="stateGroupsRoot">The root element that contains the <see cref="T:System.Windows.VisualStateManager" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stateGroupsRoot" /> is null.-or-<paramref name="newState" /> is null.</exception>
	protected void RaiseCurrentStateChanged(VisualStateGroup stateGroup, VisualState oldState, VisualState newState, FrameworkElement control, FrameworkElement stateGroupsRoot)
	{
		if (stateGroup == null)
		{
			throw new ArgumentNullException("stateGroup");
		}
		if (newState == null)
		{
			throw new ArgumentNullException("newState");
		}
		if (stateGroupsRoot != null)
		{
			stateGroup.RaiseCurrentStateChanged(stateGroupsRoot, oldState, newState, control);
		}
	}

	private static Storyboard GenerateDynamicTransitionAnimations(FrameworkElement root, VisualStateGroup group, VisualState newState, VisualTransition transition)
	{
		IEasingFunction easingFunction = null;
		Storyboard storyboard = new Storyboard();
		if (transition != null)
		{
			if (transition.GeneratedDuration != DurationZero)
			{
				storyboard.Duration = transition.GeneratedDuration;
			}
			easingFunction = transition.GeneratedEasingFunction;
		}
		else
		{
			storyboard.Duration = new Duration(TimeSpan.Zero);
		}
		Dictionary<TimelineDataToken, Timeline> dictionary = FlattenTimelines(group.CurrentStoryboards);
		Dictionary<TimelineDataToken, Timeline> dictionary2 = FlattenTimelines(transition?.Storyboard);
		Dictionary<TimelineDataToken, Timeline> dictionary3 = FlattenTimelines(newState.Storyboard);
		foreach (KeyValuePair<TimelineDataToken, Timeline> item in dictionary2)
		{
			dictionary.Remove(item.Key);
			dictionary3.Remove(item.Key);
		}
		foreach (KeyValuePair<TimelineDataToken, Timeline> item2 in dictionary3)
		{
			Timeline timeline = GenerateToAnimation(root, item2.Value, easingFunction, isEntering: true);
			if (timeline != null)
			{
				timeline.Duration = storyboard.Duration;
				storyboard.Children.Add(timeline);
			}
			dictionary.Remove(item2.Key);
		}
		foreach (KeyValuePair<TimelineDataToken, Timeline> item3 in dictionary)
		{
			Timeline timeline2 = GenerateFromAnimation(root, item3.Value, easingFunction);
			if (timeline2 != null)
			{
				timeline2.Duration = storyboard.Duration;
				storyboard.Children.Add(timeline2);
			}
		}
		return storyboard;
	}

	private static Timeline GenerateFromAnimation(FrameworkElement root, Timeline timeline, IEasingFunction easingFunction)
	{
		Timeline timeline2 = null;
		if (timeline is ColorAnimation || timeline is ColorAnimationUsingKeyFrames)
		{
			timeline2 = new ColorAnimation
			{
				EasingFunction = easingFunction
			};
		}
		else if (timeline is DoubleAnimation || timeline is DoubleAnimationUsingKeyFrames)
		{
			timeline2 = new DoubleAnimation
			{
				EasingFunction = easingFunction
			};
		}
		else if (timeline is PointAnimation || timeline is PointAnimationUsingKeyFrames)
		{
			timeline2 = new PointAnimation
			{
				EasingFunction = easingFunction
			};
		}
		if (timeline2 != null)
		{
			CopyStoryboardTargetProperties(root, timeline, timeline2);
		}
		return timeline2;
	}

	private static Timeline GenerateToAnimation(FrameworkElement root, Timeline timeline, IEasingFunction easingFunction, bool isEntering)
	{
		Timeline timeline2 = null;
		Color? targetColor = GetTargetColor(timeline, isEntering);
		if (targetColor.HasValue)
		{
			timeline2 = new ColorAnimation
			{
				To = targetColor,
				EasingFunction = easingFunction
			};
		}
		if (timeline2 == null)
		{
			double? targetDouble = GetTargetDouble(timeline, isEntering);
			if (targetDouble.HasValue)
			{
				timeline2 = new DoubleAnimation
				{
					To = targetDouble,
					EasingFunction = easingFunction
				};
			}
		}
		if (timeline2 == null)
		{
			Point? targetPoint = GetTargetPoint(timeline, isEntering);
			if (targetPoint.HasValue)
			{
				timeline2 = new PointAnimation
				{
					To = targetPoint,
					EasingFunction = easingFunction
				};
			}
		}
		if (timeline2 != null)
		{
			CopyStoryboardTargetProperties(root, timeline, timeline2);
		}
		return timeline2;
	}

	private static void CopyStoryboardTargetProperties(FrameworkElement root, Timeline source, Timeline destination)
	{
		if (source != null || destination != null)
		{
			string targetName = Storyboard.GetTargetName(source);
			DependencyObject dependencyObject = Storyboard.GetTarget(source);
			PropertyPath targetProperty = Storyboard.GetTargetProperty(source);
			if (dependencyObject == null && !string.IsNullOrEmpty(targetName))
			{
				dependencyObject = root.FindName(targetName) as DependencyObject;
			}
			if (targetName != null)
			{
				Storyboard.SetTargetName(destination, targetName);
			}
			if (dependencyObject != null)
			{
				Storyboard.SetTarget(destination, dependencyObject);
			}
			if (targetProperty != null)
			{
				Storyboard.SetTargetProperty(destination, targetProperty);
			}
		}
	}

	internal static VisualTransition GetTransition(FrameworkElement element, VisualStateGroup group, VisualState from, VisualState to)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (group == null)
		{
			throw new ArgumentNullException("group");
		}
		if (to == null)
		{
			throw new ArgumentNullException("to");
		}
		VisualTransition visualTransition = null;
		VisualTransition visualTransition2 = null;
		int num = -1;
		IList<VisualTransition> list = (IList<VisualTransition>)group.Transitions;
		if (list != null)
		{
			foreach (VisualTransition item in list)
			{
				if (visualTransition2 == null && item.IsDefault)
				{
					visualTransition2 = item;
					continue;
				}
				int num2 = -1;
				VisualState state = group.GetState(item.From);
				VisualState state2 = group.GetState(item.To);
				if (from == state)
				{
					num2++;
				}
				else if (state != null)
				{
					continue;
				}
				if (to == state2)
				{
					num2 += 2;
				}
				else if (state2 != null)
				{
					continue;
				}
				if (num2 > num)
				{
					num = num2;
					visualTransition = item;
				}
			}
		}
		return visualTransition ?? visualTransition2;
	}

	private static Color? GetTargetColor(Timeline timeline, bool isEntering)
	{
		if (timeline is ColorAnimation { From: var from } colorAnimation)
		{
			if (!from.HasValue)
			{
				return colorAnimation.To;
			}
			return colorAnimation.From;
		}
		if (timeline is ColorAnimationUsingKeyFrames colorAnimationUsingKeyFrames)
		{
			if (colorAnimationUsingKeyFrames.KeyFrames.Count == 0)
			{
				return null;
			}
			return colorAnimationUsingKeyFrames.KeyFrames[(!isEntering) ? (colorAnimationUsingKeyFrames.KeyFrames.Count - 1) : 0].Value;
		}
		return null;
	}

	private static double? GetTargetDouble(Timeline timeline, bool isEntering)
	{
		if (timeline is DoubleAnimation { From: var from } doubleAnimation)
		{
			if (!from.HasValue)
			{
				return doubleAnimation.To;
			}
			return doubleAnimation.From;
		}
		if (timeline is DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames)
		{
			if (doubleAnimationUsingKeyFrames.KeyFrames.Count == 0)
			{
				return null;
			}
			return doubleAnimationUsingKeyFrames.KeyFrames[(!isEntering) ? (doubleAnimationUsingKeyFrames.KeyFrames.Count - 1) : 0].Value;
		}
		return null;
	}

	private static Point? GetTargetPoint(Timeline timeline, bool isEntering)
	{
		if (timeline is PointAnimation { From: var from } pointAnimation)
		{
			if (!from.HasValue)
			{
				return pointAnimation.To;
			}
			return pointAnimation.From;
		}
		if (timeline is PointAnimationUsingKeyFrames pointAnimationUsingKeyFrames)
		{
			if (pointAnimationUsingKeyFrames.KeyFrames.Count == 0)
			{
				return null;
			}
			return pointAnimationUsingKeyFrames.KeyFrames[(!isEntering) ? (pointAnimationUsingKeyFrames.KeyFrames.Count - 1) : 0].Value;
		}
		return null;
	}

	private static Dictionary<TimelineDataToken, Timeline> FlattenTimelines(Storyboard storyboard)
	{
		Dictionary<TimelineDataToken, Timeline> result = new Dictionary<TimelineDataToken, Timeline>();
		FlattenTimelines(storyboard, result);
		return result;
	}

	private static Dictionary<TimelineDataToken, Timeline> FlattenTimelines(Collection<Storyboard> storyboards)
	{
		Dictionary<TimelineDataToken, Timeline> result = new Dictionary<TimelineDataToken, Timeline>();
		for (int i = 0; i < storyboards.Count; i++)
		{
			FlattenTimelines(storyboards[i], result);
		}
		return result;
	}

	private static void FlattenTimelines(Storyboard storyboard, Dictionary<TimelineDataToken, Timeline> result)
	{
		if (storyboard == null)
		{
			return;
		}
		for (int i = 0; i < storyboard.Children.Count; i++)
		{
			Timeline timeline = storyboard.Children[i];
			if (timeline is Storyboard storyboard2)
			{
				FlattenTimelines(storyboard2, result);
			}
			else
			{
				result[new TimelineDataToken(timeline)] = timeline;
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.VisualStateManager" /> class. </summary>
	public VisualStateManager()
	{
	}
}
