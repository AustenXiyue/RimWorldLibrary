using System.IO;

namespace System.Windows.Documents.Serialization;

/// <summary>Provides a means for creating a software component that can serialize any part of a Windows Presentation Foundation (WPF) application's content to a manufacturer's proprietary format. </summary>
public interface ISerializerFactory
{
	/// <summary>Gets the public name of the manufacturer's serializing component. </summary>
	/// <returns>A <see cref="T:System.String" /> representing the public name of the serializing component. </returns>
	string DisplayName { get; }

	/// <summary>Gets the name of the serializing component's manufacturer. </summary>
	/// <returns>A <see cref="T:System.String" /> representing the manufacturer's name. </returns>
	string ManufacturerName { get; }

	/// <summary>Gets the web address of the serializing component's manufacturer. </summary>
	/// <returns>A <see cref="T:System.Uri" /> representing the manufacturer's website.</returns>
	Uri ManufacturerWebsite { get; }

	/// <summary>Gets the default extension for files of the manufacturer's proprietary format. </summary>
	/// <returns>A <see cref="T:System.String" /> representing the proprietary format's default file extension.</returns>
	string DefaultFileExtension { get; }

	/// <summary>Initializes an object derived from the abstract <see cref="T:System.Windows.Documents.Serialization.SerializerWriter" /> class for the specified <see cref="T:System.IO.Stream" />. </summary>
	/// <returns>An object of a class derived from <see cref="T:System.Windows.Documents.Serialization.SerializerWriter" />.</returns>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> to which the returned object writes.</param>
	SerializerWriter CreateSerializerWriter(Stream stream);
}
