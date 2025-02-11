namespace System.Xaml;

/// <summary>Represents a service that can return a CLR type system identifier for the destination type. The destination type is relevant when the destination type for a property-setting operation is indirectly reported by reflection or other mechanisms.</summary>
public interface IDestinationTypeProvider
{
	/// <summary>Returns the CLR <see cref="T:System.Type" /> that identifies the destination type for the relevant type converter or markup extension.</summary>
	/// <returns>A CLR <see cref="T:System.Type" /> value for the destination type.</returns>
	Type GetDestinationType();
}
