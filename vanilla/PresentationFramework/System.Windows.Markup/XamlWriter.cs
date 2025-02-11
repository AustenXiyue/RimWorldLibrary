using System.IO;
using System.Text;
using System.Windows.Markup.Primitives;
using System.Xml;

namespace System.Windows.Markup;

/// <summary>Provides a single static <see cref="Overload:System.Windows.Markup.XamlWriter.Save" /> method (multiple overloads) that can be used for limited XAML serialization of provided run-time objects into XAML markup.</summary>
public static class XamlWriter
{
	/// <summary>Returns a XAML string that serializes the specified object and its properties.</summary>
	/// <returns>A XAML string that can be written to a stream or file. The logical tree of all elements that fall under the provided <paramref name="obj" /> element will be serialized.</returns>
	/// <param name="obj">The element to be serialized. Typically, this is the root element of a page or application.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="obj" /> is null.</exception>
	/// <exception cref="T:System.Security.SecurityException">The application is not running in full trust.</exception>
	public static string Save(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		StringBuilder stringBuilder = new StringBuilder();
		TextWriter textWriter = new StringWriter(stringBuilder, TypeConverterHelper.InvariantEnglishUS);
		try
		{
			Save(obj, textWriter);
		}
		finally
		{
			textWriter.Close();
		}
		return stringBuilder.ToString();
	}

	/// <summary>Saves XAML information as the source for a provided <see cref="T:System.IO.TextWriter" /> object. The output of the <see cref="T:System.IO.TextWriter" /> can then be used to serialize the provided object and its properties.</summary>
	/// <param name="obj">The element to be serialized. Typically, this is the root element of a page or application.</param>
	/// <param name="writer">A <see cref="T:System.IO.TextWriter" /> instance as the destination where the serialized XAML information is written.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="obj" /> or <paramref name="writer" /> is null.</exception>
	/// <exception cref="T:System.Security.SecurityException">The application is not running in full trust.</exception>
	public static void Save(object obj, TextWriter writer)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		MarkupWriter.SaveAsXml(new XmlTextWriter(writer), obj);
	}

	/// <summary>Saves XAML information into a specified stream to serialize the specified object and its properties.</summary>
	/// <param name="obj">The element to be serialized. Typically, this is the root element of a page or application.</param>
	/// <param name="stream">Destination stream for the serialized XAML information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="obj" /> or <paramref name="stream" /> is null.</exception>
	/// <exception cref="T:System.Security.SecurityException">The application is not running in full trust.</exception>
	public static void Save(object obj, Stream stream)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		MarkupWriter.SaveAsXml(new XmlTextWriter(stream, null), obj);
	}

	/// <summary>Saves XAML information as the source for a provided <see cref="T:System.Xml.XmlWriter" /> object. The output of the <see cref="T:System.Xml.XmlWriter" /> can then be used to serialize the provided object and its properties.</summary>
	/// <param name="obj">The element to be serialized. Typically, this is the root element of a page or application.</param>
	/// <param name="xmlWriter">Writer to use to write the serialized XAML information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="obj" /> or <paramref name="xmlWriter" /> is null.</exception>
	/// <exception cref="T:System.Security.SecurityException">The application is not running in full trust.</exception>
	public static void Save(object obj, XmlWriter xmlWriter)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (xmlWriter == null)
		{
			throw new ArgumentNullException("xmlWriter");
		}
		try
		{
			MarkupWriter.SaveAsXml(xmlWriter, obj);
		}
		finally
		{
			xmlWriter.Flush();
		}
	}

	/// <summary>Saves XAML information into a custom serializer. The output of the serializer can then be used to serialize the provided object and its properties.</summary>
	/// <param name="obj">The element to be serialized. Typically, this is the root element of a page or application.</param>
	/// <param name="manager">A custom serialization implementation.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="obj" /> or <paramref name="manager" /> is null.</exception>
	/// <exception cref="T:System.Security.SecurityException">The application is not running in full trust.</exception>
	public static void Save(object obj, XamlDesignerSerializationManager manager)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (manager == null)
		{
			throw new ArgumentNullException("manager");
		}
		MarkupWriter.SaveAsXml(manager.XmlWriter, obj, manager);
	}
}
