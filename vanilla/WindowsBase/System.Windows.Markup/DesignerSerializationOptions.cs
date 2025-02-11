namespace System.Windows.Markup;

/// <summary>Specifies how a property is to be serialized.</summary>
[Flags]
public enum DesignerSerializationOptions
{
	/// <summary>The property should be serialized as an attribute.</summary>
	SerializeAsAttribute = 1
}
