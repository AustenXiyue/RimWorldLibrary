using System.Runtime.InteropServices;

namespace MS.Internal.Annotations;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct AnnotationXmlConstants
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct Namespaces
	{
		public const string CoreSchemaNamespace = "http://schemas.microsoft.com/windows/annotations/2003/11/core";

		public const string BaseSchemaNamespace = "http://schemas.microsoft.com/windows/annotations/2003/11/base";
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct Prefixes
	{
		internal const string XmlPrefix = "xml";

		internal const string XmlnsPrefix = "xmlns";

		internal const string CoreSchemaPrefix = "anc";

		internal const string BaseSchemaPrefix = "anb";
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct Elements
	{
		internal const string Annotation = "Annotation";

		internal const string Resource = "Resource";

		internal const string ContentLocator = "ContentLocator";

		internal const string ContentLocatorGroup = "ContentLocatorGroup";

		internal const string AuthorCollection = "Authors";

		internal const string AnchorCollection = "Anchors";

		internal const string CargoCollection = "Cargos";

		internal const string Item = "Item";

		internal const string StringAuthor = "StringAuthor";
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct Attributes
	{
		internal const string Id = "Id";

		internal const string CreationTime = "CreationTime";

		internal const string LastModificationTime = "LastModificationTime";

		internal const string TypeName = "Type";

		internal const string ResourceName = "Name";

		internal const string ItemName = "Name";

		internal const string ItemValue = "Value";
	}
}
