using System.Globalization;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Represents a property stored in a <see cref="T:System.Windows.Input.StylusPoint" />.</summary>
public class StylusPointProperty
{
	private Guid _id;

	private bool _isButton;

	/// <summary>Gets the GUID for the current <see cref="T:System.Windows.Input.StylusPointProperty" />.</summary>
	/// <returns>The GUID for the current <see cref="T:System.Windows.Input.StylusPointProperty" />.</returns>
	public Guid Id => _id;

	/// <summary>Gets whether the <see cref="T:System.Windows.Input.StylusPointProperty" /> represents a button on the stylus.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Input.StylusPointProperty" /> represents a button on the stylus; otherwise, false.</returns>
	public bool IsButton => _isButton;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointProperty" /> class using the specified GUID. </summary>
	/// <param name="identifier">The <see cref="T:System.Guid" /> that uniquely identifies the <see cref="T:System.Windows.Input.StylusPointProperty" />.</param>
	/// <param name="isButton">true to indicate that the property represents a button on the stylus; otherwise, false. </param>
	public StylusPointProperty(Guid identifier, bool isButton)
	{
		Initialize(identifier, isButton);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointProperty" /> class, copying the specified <see cref="T:System.Windows.Input.StylusPointProperty" />. </summary>
	/// <param name="stylusPointProperty">The <see cref="T:System.Windows.Input.StylusPointProperty" /> to copy.</param>
	protected StylusPointProperty(StylusPointProperty stylusPointProperty)
	{
		if (stylusPointProperty == null)
		{
			throw new ArgumentNullException("stylusPointProperty");
		}
		Initialize(stylusPointProperty.Id, stylusPointProperty.IsButton);
	}

	private void Initialize(Guid identifier, bool isButton)
	{
		if (StylusPointPropertyIds.IsKnownButton(identifier))
		{
			if (!isButton)
			{
				throw new ArgumentException(SR.InvalidIsButtonForId, "isButton");
			}
		}
		else if (StylusPointPropertyIds.IsKnownId(identifier) && isButton)
		{
			throw new ArgumentException(SR.InvalidIsButtonForId2, "isButton");
		}
		_id = identifier;
		_isButton = isButton;
	}

	public override string ToString()
	{
		return "{Id=" + StylusPointPropertyIds.GetStringRepresentation(_id) + ", IsButton=" + _isButton.ToString(CultureInfo.InvariantCulture) + "}";
	}
}
