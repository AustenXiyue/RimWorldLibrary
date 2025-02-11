namespace System.Windows.Input;

/// <summary>Provides data for several of the events that are associated with the <see cref="T:System.Windows.Input.Stylus" /> class. </summary>
public class StylusEventArgs : InputEventArgs
{
	private RawStylusInputReport _inputReport;

	/// <summary>Gets the <see cref="T:System.Windows.Input.StylusDevice" /> that represents the stylus.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.StylusDevice" /> that represents the stylus.</returns>
	public StylusDevice StylusDevice => (StylusDevice)base.Device;

	internal StylusDeviceBase StylusDeviceImpl => ((StylusDevice)base.Device).StylusDeviceImpl;

	/// <summary>Gets a value that indicates whether the stylus is in proximity to, but not touching, the digitizer.</summary>
	/// <returns>true if the stylus is in proximity to, but not touching, the digitizer; otherwise, false.</returns>
	public bool InAir => StylusDevice.InAir;

	/// <summary>Gets a value that indicates whether the stylus in inverted.</summary>
	/// <returns>true if the stylus is inverted; otherwise, false.</returns>
	public bool Inverted => StylusDevice.Inverted;

	internal RawStylusInputReport InputReport
	{
		get
		{
			return _inputReport;
		}
		set
		{
			_inputReport = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusEventArgs" /> class. </summary>
	/// <param name="stylus">The stylus to associate with the event.</param>
	/// <param name="timestamp">The time when the event occurs.</param>
	public StylusEventArgs(StylusDevice stylus, int timestamp)
		: base(stylus, timestamp)
	{
		if (stylus == null)
		{
			throw new ArgumentNullException("stylus");
		}
	}

	/// <summary>Gets the position of the stylus.</summary>
	/// <returns>A <see cref="T:System.Windows.Point" /> that represents the position of the stylus, based on the coordinates of <paramref name="relativeTo" />.</returns>
	/// <param name="relativeTo">The <see cref="T:System.Windows.IInputElement" /> that the (<paramref name="x" />,<paramref name="y" />) coordinates are mapped to.</param>
	public Point GetPosition(IInputElement relativeTo)
	{
		return StylusDevice.GetPosition(relativeTo);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Input.StylusPointCollection" /> that contains <see cref="T:System.Windows.Input.StylusPoint" /> objects relative to the specified input element.</summary>
	/// <returns>A <see cref="T:System.Windows.Input.StylusPointCollection" /> that contains <see cref="T:System.Windows.Input.StylusPoint" /> objects collected in the event.</returns>
	/// <param name="relativeTo">The <see cref="T:System.Windows.IInputElement" /> to which the (<paramref name="x,y" />) coordinates in the <see cref="T:System.Windows.Input.StylusPointCollection" /> are mapped.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="relativeTo" /> is neither <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.FrameworkContentElement" />.</exception>
	public StylusPointCollection GetStylusPoints(IInputElement relativeTo)
	{
		return StylusDevice.GetStylusPoints(relativeTo);
	}

	/// <summary>Returns a <see cref="T:System.Windows.Input.StylusPointCollection" /> that uses the specified <see cref="T:System.Windows.Input.StylusPointDescription" /> and contains <see cref="T:System.Windows.Input.StylusPoint" /> objects relating to the specified input element.</summary>
	/// <returns>A <see cref="T:System.Windows.Input.StylusPointCollection" /> that contains <see cref="T:System.Windows.Input.StylusPoint" /> objects collected during an event.</returns>
	/// <param name="relativeTo">The <see cref="T:System.Windows.IInputElement" /> to which the (<paramref name="x,y" />) coordinates in the <see cref="T:System.Windows.Input.StylusPointCollection" /> are mapped.</param>
	/// <param name="subsetToReformatTo">The <see cref="T:System.Windows.Input.StylusPointDescription" /> to be used by the <see cref="T:System.Windows.Input.StylusPointCollection" />.</param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="relativeTo" /> is neither <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.FrameworkContentElement" />.</exception>
	public StylusPointCollection GetStylusPoints(IInputElement relativeTo, StylusPointDescription subsetToReformatTo)
	{
		return StylusDevice.GetStylusPoints(relativeTo, subsetToReformatTo);
	}

	/// <summary>Invokes event handlers in a type-specific way, which can increase event system efficiency.</summary>
	/// <param name="genericHandler">The generic handler to call in a type-specific way.</param>
	/// <param name="genericTarget">The target to call the handler on.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((StylusEventHandler)genericHandler)(genericTarget, this);
	}
}
