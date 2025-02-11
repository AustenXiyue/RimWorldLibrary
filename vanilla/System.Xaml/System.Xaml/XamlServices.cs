using System.Globalization;
using System.IO;
using System.Xml;

namespace System.Xaml;

/// <summary>Provides higher-level services (static methods) for the common XAML tasks of reading XAML and writing an object graph; or reading an object graph and writing XAML file output for serialization purposes.</summary>
public static class XamlServices
{
	/// <summary>Reads XAML as string output and returns an object graph.</summary>
	/// <returns>The object graph that is returned.</returns>
	/// <param name="xaml">The XAML string input to parse.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xaml" /> input is null.</exception>
	public static object Parse(string xaml)
	{
		ArgumentNullException.ThrowIfNull(xaml, "xaml");
		using XmlReader xmlReader = XmlReader.Create(new StringReader(xaml));
		return Load(new XamlXmlReader(xmlReader));
	}

	/// <summary>Loads a <see cref="T:System.IO.Stream" /> source for a XAML reader and returns an object graph.</summary>
	/// <returns>The object graph that is returned.</returns>
	/// <param name="fileName">The file name to load and use as source.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileName" /> input is null.</exception>
	public static object Load(string fileName)
	{
		ArgumentNullException.ThrowIfNull(fileName, "fileName");
		using XmlReader xmlReader = XmlReader.Create(fileName);
		return Load(new XamlXmlReader(xmlReader));
	}

	/// <summary>Loads a <see cref="T:System.IO.Stream" /> source for a XAML reader and writes its output as an object graph.</summary>
	/// <returns>The object graph that is written as output.</returns>
	/// <param name="stream">The stream to load as input.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> is null.</exception>
	public static object Load(Stream stream)
	{
		ArgumentNullException.ThrowIfNull(stream, "stream");
		using XmlReader xmlReader = XmlReader.Create(stream);
		return Load(new XamlXmlReader(xmlReader));
	}

	/// <summary>Creates a XAML reader from a <see cref="T:System.IO.TextReader" />, and returns an object graph.</summary>
	/// <returns>The object graph that is returned.</returns>
	/// <param name="textReader">The <see cref="T:System.IO.TextReader" /> to use as the basis for the created <see cref="T:System.Xml.XmlReader" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="textReader" /> is null.</exception>
	public static object Load(TextReader textReader)
	{
		ArgumentNullException.ThrowIfNull(textReader, "textReader");
		using XmlReader xmlReader = XmlReader.Create(textReader);
		return Load(new XamlXmlReader(xmlReader));
	}

	/// <summary>Loads a specific XML reader implementation and returns an object graph.</summary>
	/// <returns>The output object graph.</returns>
	/// <param name="xmlReader">The <see cref="T:System.Xml.XmlReader" /> implementation to use as the reader for this Load operation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xamlReader" /> input is null.</exception>
	public static object Load(XmlReader xmlReader)
	{
		ArgumentNullException.ThrowIfNull(xmlReader, "xmlReader");
		using XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
		return Load(xamlReader);
	}

	/// <summary>Loads a specific XAML reader implementation and returns an object graph.</summary>
	/// <returns>The object graph that is returned.</returns>
	/// <param name="xamlReader">The XAML reader implementation to use as the reader for this Load operation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xamlReader" /> input is null.</exception>
	public static object Load(XamlReader xamlReader)
	{
		ArgumentNullException.ThrowIfNull(xamlReader, "xamlReader");
		XamlObjectWriter xamlObjectWriter = new XamlObjectWriter(xamlReader.SchemaContext);
		Transform(xamlReader, xamlObjectWriter);
		return xamlObjectWriter.Result;
	}

	/// <summary>Connects a <see cref="T:System.Xaml.XamlReader" /> and a <see cref="T:System.Xaml.XamlWriter" /> to use a common XAML node set intermediary. Potentially transforms the content, depending on the types of readers and writers that are provided.</summary>
	/// <param name="xamlReader">The <see cref="T:System.Xaml.XamlReader" /> implementation to use.</param>
	/// <param name="xamlWriter">The <see cref="T:System.Xaml.XamlWriter" /> to use.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xamlReader" /> or <paramref name="xamlWriter" /> input is null.</exception>
	/// <exception cref="T:System.Xaml.XamlException">The XAML schema context does not match between the provided <paramref name="xamlReader" /> and <paramref name="xamlWriter" />.</exception>
	public static void Transform(XamlReader xamlReader, XamlWriter xamlWriter)
	{
		Transform(xamlReader, xamlWriter, closeWriter: true);
	}

	/// <summary>Connects a <see cref="T:System.Xaml.XamlReader" /> and a <see cref="T:System.Xaml.XamlWriter" /> to use a common XAML node set intermediary. Potentially transforms the content, depending on the types of readers and writers that are provided. Provides a parameter for specifying whether to close the writer after the call is completed.</summary>
	/// <param name="xamlReader">The <see cref="T:System.Xaml.XamlReader" /> implementation to use.</param>
	/// <param name="xamlWriter">The <see cref="T:System.Xaml.XamlWriter" /> to use.</param>
	/// <param name="closeWriter">true to close the writer after the call is complete; false to leave the writer active at the last written position.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xamlReader" /> or <paramref name="xamlWriter" /> input is null.</exception>
	/// <exception cref="T:System.Xaml.XamlException">The XAML schema context does not match between the provided <paramref name="xamlReader" /> and <paramref name="xamlWriter" />.</exception>
	public static void Transform(XamlReader xamlReader, XamlWriter xamlWriter, bool closeWriter)
	{
		ArgumentNullException.ThrowIfNull(xamlReader, "xamlReader");
		ArgumentNullException.ThrowIfNull(xamlWriter, "xamlWriter");
		IXamlLineInfo xamlLineInfo = xamlReader as IXamlLineInfo;
		IXamlLineInfoConsumer xamlLineInfoConsumer = xamlWriter as IXamlLineInfoConsumer;
		bool flag = false;
		if (xamlLineInfo != null && xamlLineInfo.HasLineInfo && xamlLineInfoConsumer != null && xamlLineInfoConsumer.ShouldProvideLineInfo)
		{
			flag = true;
		}
		while (xamlReader.Read())
		{
			if (flag && xamlLineInfo.LineNumber != 0)
			{
				xamlLineInfoConsumer.SetLineInfo(xamlLineInfo.LineNumber, xamlLineInfo.LinePosition);
			}
			xamlWriter.WriteNode(xamlReader);
		}
		if (closeWriter)
		{
			xamlWriter.Close();
		}
	}

	/// <summary>Processes a provided object tree into a XAML node representation, and returns a string representation of the output XAML.</summary>
	/// <returns>The XAML markup output as a string. </returns>
	/// <param name="instance">The root of the object graph to process.</param>
	public static string Save(object instance)
	{
		StringWriter stringWriter = new StringWriter(CultureInfo.CurrentCulture);
		using (XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings
		{
			Indent = true,
			OmitXmlDeclaration = true
		}))
		{
			Save(writer, instance);
		}
		return stringWriter.ToString();
	}

	/// <summary>Processes a provided object graph into a XAML node representation and then writes it to an output file at a provided location.</summary>
	/// <param name="fileName">The name and location of the file to write the output to.</param>
	/// <param name="instance">The root of the object graph to process.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fileName" /> is an empty string.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileName" /> is null.</exception>
	public static void Save(string fileName, object instance)
	{
		ArgumentNullException.ThrowIfNull(fileName, "fileName");
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentException(System.SR.StringIsNullOrEmpty, "fileName");
		}
		using XmlWriter xmlWriter = XmlWriter.Create(fileName, new XmlWriterSettings
		{
			Indent = true,
			OmitXmlDeclaration = true
		});
		Save(xmlWriter, instance);
		xmlWriter.Flush();
	}

	/// <summary>Processes a provided object graph into a XAML node representation and then into an output stream for serialization.</summary>
	/// <param name="stream">The destination stream.</param>
	/// <param name="instance">The root of the object graph to process.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="stream" /> input is null.</exception>
	public static void Save(Stream stream, object instance)
	{
		ArgumentNullException.ThrowIfNull(stream, "stream");
		using XmlWriter xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings
		{
			Indent = true,
			OmitXmlDeclaration = true
		});
		Save(xmlWriter, instance);
		xmlWriter.Flush();
	}

	/// <summary>Processes a provided object graph into a XAML node representation and then into an output that goes to the provided <see cref="T:System.IO.TextWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.IO.TextWriter" /> that writes the output.</param>
	/// <param name="instance">The root of the object graph to process.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> input is null.</exception>
	public static void Save(TextWriter writer, object instance)
	{
		ArgumentNullException.ThrowIfNull(writer, "writer");
		using XmlWriter xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings
		{
			Indent = true,
			OmitXmlDeclaration = true
		});
		Save(xmlWriter, instance);
		xmlWriter.Flush();
	}

	/// <summary>Processes a provided object graph into a XAML node representation and then writes it to the provided <see cref="T:System.Xml.XmlWriter" />.</summary>
	/// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> implementation to use.</param>
	/// <param name="instance">The root of the object graph to process.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> input is null.</exception>
	public static void Save(XmlWriter writer, object instance)
	{
		ArgumentNullException.ThrowIfNull(writer, "writer");
		using XamlXmlWriter writer2 = new XamlXmlWriter(writer, new XamlSchemaContext());
		Save(writer2, instance);
	}

	/// <summary>Processes a provided object graph into a XAML node representation and then writes it to the provided XAML writer.</summary>
	/// <param name="writer">The <see cref="T:System.Xaml.XamlWriter" /> implementation to use.</param>
	/// <param name="instance">The root of the object graph to process.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="writer" /> input is null.</exception>
	public static void Save(XamlWriter writer, object instance)
	{
		ArgumentNullException.ThrowIfNull(writer, "writer");
		Transform(new XamlObjectReader(instance, writer.SchemaContext), writer);
	}
}
