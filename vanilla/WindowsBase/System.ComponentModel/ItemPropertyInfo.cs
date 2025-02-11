namespace System.ComponentModel;

/// <summary>Contains information about a property.</summary>
public class ItemPropertyInfo
{
	private string _name;

	private Type _type;

	private object _descriptor;

	/// <summary>Gets the name of the property.</summary>
	/// <returns>The name of the property.</returns>
	public string Name => _name;

	/// <summary>Gets the type of the property.</summary>
	/// <returns>The type of the property.</returns>
	public Type PropertyType => _type;

	/// <summary>Get an object that contains additional information about the property.</summary>
	/// <returns>An object that contains additional information about the property.</returns>
	public object Descriptor => _descriptor;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.ItemPropertyInfo" /> class. </summary>
	/// <param name="name">The name of the property.</param>
	/// <param name="type">The type of the property.</param>
	/// <param name="descriptor">An object that contains additional information about the property.</param>
	public ItemPropertyInfo(string name, Type type, object descriptor)
	{
		_name = name;
		_type = type;
		_descriptor = descriptor;
	}
}
