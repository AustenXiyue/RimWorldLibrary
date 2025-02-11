using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Represents a data format by using a format name and numeric ID.</summary>
public sealed class DataFormat
{
	private readonly string _name;

	private readonly int _id;

	/// <summary>Gets the name of the data format.</summary>
	/// <returns>The name of the data format.</returns>
	public string Name => _name;

	/// <summary>Gets the numeric ID of the data format.</summary>
	/// <returns>The numeric ID of the data format.</returns>
	public int Id => _id;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DataFormat" /> class.</summary>
	/// <param name="name">The name for the data format.</param>
	/// <param name="id">The integer ID for the data format.</param>
	public DataFormat(string name, int id)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		_name = name;
		_id = id;
	}
}
