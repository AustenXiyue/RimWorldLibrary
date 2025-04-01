using Unity;

namespace System.Xml.Schema;

/// <summary>Returns detailed information related to the ValidationEventHandler.</summary>
public class ValidationEventArgs : EventArgs
{
	private XmlSchemaException ex;

	private XmlSeverityType severity;

	/// <summary>Gets the severity of the validation event.</summary>
	/// <returns>An <see cref="T:System.Xml.Schema.XmlSeverityType" /> value representing the severity of the validation event.</returns>
	public XmlSeverityType Severity => severity;

	/// <summary>Gets the <see cref="T:System.Xml.Schema.XmlSchemaException" /> associated with the validation event.</summary>
	/// <returns>The XmlSchemaException associated with the validation event.</returns>
	public XmlSchemaException Exception => ex;

	/// <summary>Gets the text description corresponding to the validation event.</summary>
	/// <returns>The text description.</returns>
	public string Message => ex.Message;

	internal ValidationEventArgs(XmlSchemaException ex)
	{
		this.ex = ex;
		severity = XmlSeverityType.Error;
	}

	internal ValidationEventArgs(XmlSchemaException ex, XmlSeverityType severity)
	{
		this.ex = ex;
		this.severity = severity;
	}

	internal ValidationEventArgs()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
