using System.Collections.Generic;
using System.IO.Packaging;
using System.Xml;

namespace MS.Internal.IO.Packaging;

internal static class InternalRelationshipCollection
{
	private static readonly string RelationshipTagName = "Relationship";

	private static readonly string TargetAttributeName = "Target";

	private static readonly string TypeAttributeName = "Type";

	private static readonly string IdAttributeName = "Id";

	private static readonly string TargetModeAttributeName = "TargetMode";

	internal static void WriteRelationshipsAsXml(XmlWriter writer, IEnumerable<PackageRelationship> relationships, bool alwaysWriteTargetModeAttribute, bool inStreamingProduction)
	{
		foreach (PackageRelationship relationship in relationships)
		{
			writer.WriteStartElement(RelationshipTagName);
			writer.WriteAttributeString(TypeAttributeName, relationship.RelationshipType);
			writer.WriteAttributeString(TargetAttributeName, relationship.TargetUri.OriginalString);
			if (alwaysWriteTargetModeAttribute || relationship.TargetMode == TargetMode.External)
			{
				writer.WriteAttributeString(TargetModeAttributeName, relationship.TargetMode.ToString());
			}
			writer.WriteAttributeString(IdAttributeName, relationship.Id);
			writer.WriteEndElement();
		}
	}
}
