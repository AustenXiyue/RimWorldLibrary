using System.Collections.Generic;
using System.Globalization;
using System.Windows.Markup;

namespace System.Windows.Documents;

internal sealed class FixedSOMPage : FixedSOMContainer
{
	private List<FixedNode> _markupOrder;

	private CultureInfo _cultureInfo;

	internal override FixedElement.ElementType[] ElementTypes => new FixedElement.ElementType[1] { FixedElement.ElementType.Section };

	internal List<FixedNode> MarkupOrder
	{
		get
		{
			return _markupOrder;
		}
		set
		{
			_markupOrder = value;
		}
	}

	internal CultureInfo CultureInfo
	{
		set
		{
			_cultureInfo = value;
		}
	}

	public void AddFixedBlock(FixedSOMFixedBlock fixedBlock)
	{
		Add(fixedBlock);
	}

	public void AddTable(FixedSOMTable table)
	{
		Add(table);
	}

	public override void SetRTFProperties(FixedElement element)
	{
		if (_cultureInfo != null)
		{
			element.SetValue(FrameworkContentElement.LanguageProperty, XmlLanguage.GetLanguage(_cultureInfo.IetfLanguageTag));
		}
	}
}
