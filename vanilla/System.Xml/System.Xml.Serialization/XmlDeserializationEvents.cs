namespace System.Xml.Serialization;

/// <summary>Contains fields that can be used to pass event delegates to a thread-safe <see cref="Overload:System.Xml.Serialization.XmlSerializer.Deserialize" /> method of the <see cref="T:System.Xml.Serialization.XmlSerializer" />.</summary>
public struct XmlDeserializationEvents
{
	private XmlNodeEventHandler onUnknownNode;

	private XmlAttributeEventHandler onUnknownAttribute;

	private XmlElementEventHandler onUnknownElement;

	private UnreferencedObjectEventHandler onUnreferencedObject;

	internal object sender;

	/// <summary>Gets or sets an object that represents the method that handles the <see cref="E:System.Xml.Serialization.XmlSerializer.UnknownNode" /> event.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlNodeEventHandler" /> that points to the event handler.</returns>
	public XmlNodeEventHandler OnUnknownNode
	{
		get
		{
			return onUnknownNode;
		}
		set
		{
			onUnknownNode = value;
		}
	}

	/// <summary>Gets or sets an object that represents the method that handles the <see cref="E:System.Xml.Serialization.XmlSerializer.UnknownAttribute" /> event.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlAttributeEventHandler" /> that points to the event handler.</returns>
	public XmlAttributeEventHandler OnUnknownAttribute
	{
		get
		{
			return onUnknownAttribute;
		}
		set
		{
			onUnknownAttribute = value;
		}
	}

	/// <summary>Gets or sets an object that represents the method that handles the <see cref="E:System.Xml.Serialization.XmlSerializer.UnknownElement" /> event.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.XmlElementEventHandler" /> that points to the event handler.</returns>
	public XmlElementEventHandler OnUnknownElement
	{
		get
		{
			return onUnknownElement;
		}
		set
		{
			onUnknownElement = value;
		}
	}

	/// <summary>Gets or sets an object that represents the method that handles the <see cref="E:System.Xml.Serialization.XmlSerializer.UnreferencedObject" /> event.</summary>
	/// <returns>An <see cref="T:System.Xml.Serialization.UnreferencedObjectEventHandler" /> that points to the event handler.</returns>
	public UnreferencedObjectEventHandler OnUnreferencedObject
	{
		get
		{
			return onUnreferencedObject;
		}
		set
		{
			onUnreferencedObject = value;
		}
	}
}
