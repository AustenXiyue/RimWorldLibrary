using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace System.Windows;

/// <summary>Contains mutually exclusive <see cref="T:System.Windows.VisualState" /> objects and <see cref="T:System.Windows.VisualTransition" /> objects that are used to move from one state to another.</summary>
[ContentProperty("States")]
[RuntimeNameProperty("Name")]
public class VisualStateGroup : DependencyObject
{
	private Collection<Storyboard> _currentStoryboards;

	private FreezableCollection<VisualState> _states;

	private FreezableCollection<VisualTransition> _transitions;

	/// <summary>Gets or sets the name of the <see cref="T:System.Windows.VisualStateGroup" />.</summary>
	/// <returns>The name of the <see cref="T:System.Windows.VisualStateGroup" />.</returns>
	public string Name { get; set; }

	/// <summary>Gets the collection of mutually exclusive <see cref="T:System.Windows.VisualState" /> objects.</summary>
	/// <returns>The collection of mutually exclusive <see cref="T:System.Windows.VisualState" /> objects.</returns>
	public IList States
	{
		get
		{
			if (_states == null)
			{
				_states = new FreezableCollection<VisualState>();
			}
			return _states;
		}
	}

	/// <summary>Gets the collection of <see cref="T:System.Windows.VisualTransition" /> objects.</summary>
	/// <returns>The collection of <see cref="T:System.Windows.VisualTransition" /> objects.</returns>
	public IList Transitions
	{
		get
		{
			if (_transitions == null)
			{
				_transitions = new FreezableCollection<VisualTransition>();
			}
			return _transitions;
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.VisualState" /> that is currently applied to the control.</summary>
	/// <returns>The <see cref="T:System.Windows.VisualState" /> that is currently applied to the control.</returns>
	public VisualState CurrentState { get; internal set; }

	internal Collection<Storyboard> CurrentStoryboards
	{
		get
		{
			if (_currentStoryboards == null)
			{
				_currentStoryboards = new Collection<Storyboard>();
			}
			return _currentStoryboards;
		}
	}

	/// <summary>Occurs after a control transitions to a different state.</summary>
	public event EventHandler<VisualStateChangedEventArgs> CurrentStateChanged;

	/// <summary>Occurs when a control starts transitioning to a different state.</summary>
	public event EventHandler<VisualStateChangedEventArgs> CurrentStateChanging;

	internal VisualState GetState(string stateName)
	{
		for (int i = 0; i < States.Count; i++)
		{
			VisualState visualState = (VisualState)States[i];
			if (visualState.Name == stateName)
			{
				return visualState;
			}
		}
		return null;
	}

	internal void StartNewThenStopOld(FrameworkElement element, params Storyboard[] newStoryboards)
	{
		for (int i = 0; i < CurrentStoryboards.Count; i++)
		{
			if (CurrentStoryboards[i] != null)
			{
				CurrentStoryboards[i].Remove(element);
			}
		}
		CurrentStoryboards.Clear();
		for (int j = 0; j < newStoryboards.Length; j++)
		{
			if (newStoryboards[j] != null)
			{
				newStoryboards[j].Begin(element, HandoffBehavior.SnapshotAndReplace, isControllable: true);
				CurrentStoryboards.Add(newStoryboards[j]);
			}
		}
	}

	internal void RaiseCurrentStateChanging(FrameworkElement stateGroupsRoot, VisualState oldState, VisualState newState, FrameworkElement control)
	{
		if (this.CurrentStateChanging != null)
		{
			this.CurrentStateChanging(stateGroupsRoot, new VisualStateChangedEventArgs(oldState, newState, control, stateGroupsRoot));
		}
	}

	internal void RaiseCurrentStateChanged(FrameworkElement stateGroupsRoot, VisualState oldState, VisualState newState, FrameworkElement control)
	{
		if (this.CurrentStateChanged != null)
		{
			this.CurrentStateChanged(stateGroupsRoot, new VisualStateChangedEventArgs(oldState, newState, control, stateGroupsRoot));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.VisualStateGroup" /> class. </summary>
	public VisualStateGroup()
	{
	}
}
