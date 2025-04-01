namespace System.Xml.Serialization;

/// <summary>Provides data for the known, but unreferenced, object found in an encoded SOAP XML stream during deserialization.</summary>
public class UnreferencedObjectEventArgs : EventArgs
{
	private object o;

	private string id;

	/// <summary>Gets the deserialized, but unreferenced, object.</summary>
	/// <returns>The deserialized, but unreferenced, object.</returns>
	public object UnreferencedObject => o;

	/// <summary>Gets the ID of the object.</summary>
	/// <returns>The ID of the object.</returns>
	public string UnreferencedId => id;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.UnreferencedObjectEventArgs" /> class.</summary>
	/// <param name="o">The unreferenced object. </param>
	/// <param name="id">A unique string used to identify the unreferenced object. </param>
	public UnreferencedObjectEventArgs(object o, string id)
	{
		this.o = o;
		this.id = id;
	}
}
