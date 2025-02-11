using System.Diagnostics;

namespace MS.Internal.Documents;

internal sealed class DocumentsTrace
{
	internal static class FixedFormat
	{
		public static DocumentsTrace FixedDocument;

		public static DocumentsTrace PageContent;

		public static DocumentsTrace IDF;
	}

	internal static class FixedTextOM
	{
		public static DocumentsTrace TextView;

		public static DocumentsTrace TextContainer;

		public static DocumentsTrace Map;

		public static DocumentsTrace Highlight;

		public static DocumentsTrace Builder;

		public static DocumentsTrace FlowPosition;
	}

	internal static class FixedDocumentSequence
	{
		public static DocumentsTrace Content;

		public static DocumentsTrace IDF;

		public static DocumentsTrace TextOM;

		public static DocumentsTrace Highlights;
	}

	public bool IsEnabled => false;

	static DocumentsTrace()
	{
	}

	public DocumentsTrace(string switchName)
	{
	}

	public DocumentsTrace(string switchName, bool initialState)
		: this(switchName)
	{
	}

	[Conditional("DEBUG")]
	public void Trace(string message)
	{
	}

	[Conditional("DEBUG")]
	public void TraceCallers(int Depth)
	{
	}

	[Conditional("DEBUG")]
	public void Indent()
	{
	}

	[Conditional("DEBUG")]
	public void Unindent()
	{
	}

	[Conditional("DEBUG")]
	public void Enable()
	{
	}

	[Conditional("DEBUG")]
	public void Disable()
	{
	}
}
