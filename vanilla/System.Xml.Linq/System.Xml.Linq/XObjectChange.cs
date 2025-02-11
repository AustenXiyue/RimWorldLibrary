namespace System.Xml.Linq;

/// <summary>Specifies the event type when an event is raised for an <see cref="T:System.Xml.Linq.XObject" />.</summary>
/// <filterpriority>2</filterpriority>
public enum XObjectChange
{
	/// <summary>An <see cref="T:System.Xml.Linq.XObject" /> has been or will be added to an <see cref="T:System.Xml.Linq.XContainer" />.</summary>
	Add,
	/// <summary>An <see cref="T:System.Xml.Linq.XObject" /> has been or will be removed from an <see cref="T:System.Xml.Linq.XContainer" />.</summary>
	Remove,
	/// <summary>An <see cref="T:System.Xml.Linq.XObject" /> has been or will be renamed.</summary>
	Name,
	/// <summary>The value of an <see cref="T:System.Xml.Linq.XObject" /> has been or will be changed. In addition, a change in the serialization of an empty element (either from an empty tag to start/end tag pair or vice versa) raises this event.</summary>
	Value
}
