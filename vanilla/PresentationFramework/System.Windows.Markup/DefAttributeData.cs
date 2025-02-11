namespace System.Windows.Markup;

internal class DefAttributeData
{
	internal Type TargetType;

	internal Type DeclaringType;

	internal string TargetFullName;

	internal string TargetAssemblyName;

	internal string Args;

	internal string TargetNamespaceUri;

	internal int LineNumber;

	internal int LinePosition;

	internal int Depth;

	internal bool IsSimple;

	internal bool IsUnknownExtension => TargetType == typeof(MarkupExtensionParser.UnknownMarkupExtension);

	internal DefAttributeData(string targetAssemblyName, string targetFullName, Type targetType, string args, Type declaringType, string targetNamespaceUri, int lineNumber, int linePosition, int depth, bool isSimple)
	{
		TargetType = targetType;
		DeclaringType = declaringType;
		TargetFullName = targetFullName;
		TargetAssemblyName = targetAssemblyName;
		Args = args;
		TargetNamespaceUri = targetNamespaceUri;
		LineNumber = lineNumber;
		LinePosition = linePosition;
		Depth = depth;
		IsSimple = isSimple;
	}
}
