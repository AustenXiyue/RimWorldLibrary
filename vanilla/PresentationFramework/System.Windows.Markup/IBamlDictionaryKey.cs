using System.IO;

namespace System.Windows.Markup;

internal interface IBamlDictionaryKey
{
	int ValuePosition { get; set; }

	object KeyObject { get; set; }

	long ValuePositionPosition { get; set; }

	bool Shared { get; set; }

	bool SharedSet { get; set; }

	object[] StaticResourceValues { get; set; }

	void UpdateValuePosition(int newPosition, BinaryWriter bamlBinaryWriter);
}
