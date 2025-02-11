using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Binds a <see cref="T:System.Windows.Input.MouseGesture" /> to a <see cref="T:System.Windows.Input.RoutedCommand" /> (or another <see cref="T:System.Windows.Input.ICommand" /> implementation).</summary>
public class MouseBinding : InputBinding
{
	/// <summary>Identifies the <see cref="P:System.Windows.Input.MouseBinding.MouseAction" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Input.MouseBinding.MouseAction" /> dependency property.</returns>
	public static readonly DependencyProperty MouseActionProperty = DependencyProperty.Register("MouseAction", typeof(MouseAction), typeof(MouseBinding), new UIPropertyMetadata(MouseAction.None, OnMouseActionPropertyChanged));

	private bool _settingGesture;

	/// <summary>Gets or sets the gesture associated with this <see cref="T:System.Windows.Input.MouseBinding" />. </summary>
	/// <returns>The gesture.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <see cref="P:System.Windows.Input.MouseBinding.Gesture" /> is set to null.</exception>
	[TypeConverter(typeof(MouseGestureConverter))]
	[ValueSerializer(typeof(MouseGestureValueSerializer))]
	public override InputGesture Gesture
	{
		get
		{
			return base.Gesture as MouseGesture;
		}
		set
		{
			MouseGesture mouseGesture = Gesture as MouseGesture;
			if (value is MouseGesture mouseGesture2)
			{
				base.Gesture = mouseGesture2;
				SynchronizePropertiesFromGesture(mouseGesture2);
				if (mouseGesture != mouseGesture2)
				{
					if (mouseGesture != null)
					{
						mouseGesture.PropertyChanged -= OnMouseGesturePropertyChanged;
					}
					mouseGesture2.PropertyChanged += OnMouseGesturePropertyChanged;
				}
				return;
			}
			throw new ArgumentException(SR.Format(SR.InputBinding_ExpectedInputGesture, typeof(MouseGesture)));
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Input.MouseAction" /> associated with this <see cref="T:System.Windows.Input.MouseBinding" />.</summary>
	/// <returns>The mouse action.  The default is <see cref="F:System.Windows.Input.MouseAction.None" />.</returns>
	public MouseAction MouseAction
	{
		get
		{
			return (MouseAction)GetValue(MouseActionProperty);
		}
		set
		{
			SetValue(MouseActionProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.MouseBinding" /> class.</summary>
	public MouseBinding()
	{
	}

	internal MouseBinding(ICommand command, MouseAction mouseAction)
		: this(command, new MouseGesture(mouseAction))
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.MouseBinding" /> class, using the specified command and mouse gesture.</summary>
	/// <param name="command">The command associated with the gesture.</param>
	/// <param name="gesture">The gesture associated with the command.</param>
	public MouseBinding(ICommand command, MouseGesture gesture)
		: base(command, gesture)
	{
		SynchronizePropertiesFromGesture(gesture);
		gesture.PropertyChanged += OnMouseGesturePropertyChanged;
	}

	private static void OnMouseActionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((MouseBinding)d).SynchronizeGestureFromProperties((MouseAction)e.NewValue);
	}

	/// <summary>Creates an instance of an <see cref="T:System.Windows.Input.MouseBinding" />.</summary>
	/// <returns>The new object.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new MouseBinding();
	}

	/// <summary>Copies the base (non-animated) values of the properties of the specified object.</summary>
	/// <param name="sourceFreezable">The object to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		base.CloneCore(sourceFreezable);
		CloneGesture();
	}

	/// <summary>Copies the current values of the properties of the specified object.</summary>
	/// <param name="sourceFreezable">The object to clone.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		base.CloneCurrentValueCore(sourceFreezable);
		CloneGesture();
	}

	/// <summary>Creates the instance a frozen clone of the specified <see cref="T:System.Windows.Freezable" /> by using base (non-animated) property values.</summary>
	/// <param name="sourceFreezable">The object to clone.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		base.GetAsFrozenCore(sourceFreezable);
		CloneGesture();
	}

	/// <summary>Creates the current instance a frozen clone of the specified <see cref="T:System.Windows.Freezable" />. If the object has animated dependency properties, their current animated values are copied.</summary>
	/// <param name="sourceFreezable">The object to clone.</param>
	protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
	{
		base.GetCurrentValueAsFrozenCore(sourceFreezable);
		CloneGesture();
	}

	private void SynchronizePropertiesFromGesture(MouseGesture mouseGesture)
	{
		if (!_settingGesture)
		{
			_settingGesture = true;
			try
			{
				MouseAction = mouseGesture.MouseAction;
			}
			finally
			{
				_settingGesture = false;
			}
		}
	}

	private void SynchronizeGestureFromProperties(MouseAction mouseAction)
	{
		if (_settingGesture)
		{
			return;
		}
		_settingGesture = true;
		try
		{
			if (Gesture == null)
			{
				Gesture = new MouseGesture(mouseAction);
			}
			else
			{
				((MouseGesture)Gesture).MouseAction = mouseAction;
			}
		}
		finally
		{
			_settingGesture = false;
		}
	}

	private void OnMouseGesturePropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (string.Equals(e.PropertyName, "MouseAction", StringComparison.Ordinal) && Gesture is MouseGesture mouseGesture)
		{
			SynchronizePropertiesFromGesture(mouseGesture);
		}
	}

	private void CloneGesture()
	{
		if (Gesture is MouseGesture mouseGesture)
		{
			mouseGesture.PropertyChanged += OnMouseGesturePropertyChanged;
		}
	}
}
