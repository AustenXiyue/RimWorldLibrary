namespace System.Windows.Markup;

internal enum BamlNodeType
{
	None,
	StartDocument,
	EndDocument,
	ConnectionId,
	StartElement,
	EndElement,
	Property,
	ContentProperty,
	XmlnsProperty,
	StartComplexProperty,
	EndComplexProperty,
	LiteralContent,
	Text,
	RoutedEvent,
	Event,
	IncludeReference,
	DefAttribute,
	PresentationOptionsAttribute,
	PIMapping,
	StartConstructor,
	EndConstructor
}
