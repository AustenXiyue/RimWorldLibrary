using System.Collections.Generic;

namespace System.Windows.Documents.DocumentStructures;

/// <summary>An XML element in the markup for XML Paper Specification (XPS) documents. </summary>
public class SemanticBasicElement : BlockElement
{
	internal List<BlockElement> _elementList;

	internal List<BlockElement> BlockElementList => _elementList;

	internal SemanticBasicElement()
	{
		_elementList = new List<BlockElement>();
	}
}
