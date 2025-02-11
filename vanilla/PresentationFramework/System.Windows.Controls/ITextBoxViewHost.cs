using System.Windows.Documents;

namespace System.Windows.Controls;

internal interface ITextBoxViewHost
{
	ITextContainer TextContainer { get; }

	bool IsTypographyDefaultValue { get; }
}
