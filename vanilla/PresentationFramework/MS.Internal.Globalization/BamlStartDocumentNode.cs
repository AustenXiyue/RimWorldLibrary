using System.Windows;
using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlStartDocumentNode : BamlTreeNode, ILocalizabilityInheritable
{
	public ILocalizabilityInheritable LocalizabilityAncestor => null;

	public LocalizabilityAttribute InheritableAttribute
	{
		get
		{
			return new LocalizabilityAttribute(LocalizationCategory.None)
			{
				Readability = Readability.Readable,
				Modifiability = Modifiability.Modifiable
			};
		}
		set
		{
		}
	}

	public bool IsIgnored
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	internal BamlStartDocumentNode()
		: base(BamlNodeType.StartDocument)
	{
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteStartDocument();
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlStartDocumentNode();
	}
}
