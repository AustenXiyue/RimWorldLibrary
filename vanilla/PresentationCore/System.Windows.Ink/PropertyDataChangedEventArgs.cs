using MS.Internal.PresentationCore;

namespace System.Windows.Ink;

/// <summary>Provides data for the PropertyDataChanged event.</summary>
public class PropertyDataChangedEventArgs : EventArgs
{
	private Guid _propertyGuid;

	private object _newValue;

	private object _previousValue;

	/// <summary>Gets the <see cref="T:System.Guid" /> of the custom property which changed.</summary>
	public Guid PropertyGuid => _propertyGuid;

	/// <summary>Gets the new custom property object. </summary>
	public object NewValue => _newValue;

	/// <summary>Gets the previous custom property object.</summary>
	public object PreviousValue => _previousValue;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.PropertyDataChangedEventArgs" /> class.</summary>
	/// <param name="propertyGuid">The <see cref="T:System.Guid" /> of the custom property which changed.</param>
	/// <param name="newValue">The new custom property object.</param>
	/// <param name="previousValue">The previous custom property object.</param>
	public PropertyDataChangedEventArgs(Guid propertyGuid, object newValue, object previousValue)
	{
		if (newValue == null && previousValue == null)
		{
			throw new ArgumentException(SR.Format(SR.CannotBothBeNull, "newValue", "previousValue"));
		}
		_propertyGuid = propertyGuid;
		_newValue = newValue;
		_previousValue = previousValue;
	}
}
