using System.Globalization;

namespace System.Windows.Input;

/// <summary>Represents a button on a stylus.</summary>
public class StylusButton
{
	private StylusDeviceBase _stylusDevice;

	private string _name;

	private Guid _guid;

	private StylusButtonState _cachedButtonState;

	/// <summary>Gets the <see cref="T:System.Guid" /> that represents the stylus button.</summary>
	/// <returns>The <see cref="T:System.Guid" /> property that represents the stylus button.</returns>
	public Guid Guid => _guid;

	/// <summary>Gets the state of the stylus button.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Input.StylusButtonState" /> values.</returns>
	public StylusButtonState StylusButtonState
	{
		get
		{
			StylusPointCollection stylusPoints = _stylusDevice.GetStylusPoints(null);
			if (stylusPoints == null || stylusPoints.Count == 0)
			{
				return CachedButtonState;
			}
			return (StylusButtonState)stylusPoints[stylusPoints.Count - 1].GetPropertyValue(new StylusPointProperty(Guid, isButton: true));
		}
	}

	internal StylusButtonState CachedButtonState
	{
		get
		{
			return _cachedButtonState;
		}
		set
		{
			_cachedButtonState = value;
		}
	}

	/// <summary>Gets the name of the stylus button.</summary>
	/// <returns>The name of the stylus button.</returns>
	public string Name => _name;

	/// <summary>Gets the stylus that this button belongs to.</summary>
	/// <returns>A <see cref="T:System.Windows.Input.StylusDevice" /> that represents the stylus of the current <see cref="T:System.Windows.Input.StylusButton" />.</returns>
	public StylusDevice StylusDevice => _stylusDevice.StylusDevice;

	internal StylusButton(string name, Guid id)
	{
		_name = name;
		_guid = id;
	}

	internal void SetOwner(StylusDeviceBase stylusDevice)
	{
		_stylusDevice = stylusDevice;
	}

	/// <summary>Creates a string representation of the <see cref="T:System.Windows.Input.StylusButton" />.</summary>
	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}({1})", base.ToString(), Name);
	}
}
