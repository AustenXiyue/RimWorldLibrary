using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Xps.Serialization;
using Microsoft.Win32;

namespace System.Windows.Documents.Serialization;

/// <summary>Manages serialization plug-ins created, using <see cref="T:System.Windows.Documents.Serialization.ISerializerFactory" /> and <see cref="T:System.Windows.Documents.Serialization.SerializerDescriptor" />, by manufacturers who have their own proprietary serialization formats.</summary>
public sealed class SerializerProvider
{
	private const string _registryPath = "SOFTWARE\\Microsoft\\WinFX Serializers";

	private static readonly RegistryKey _rootKey = Registry.LocalMachine;

	private ReadOnlyCollection<SerializerDescriptor> _installedSerializers;

	/// <summary>Gets a collection of the installed plug-in serializers.</summary>
	/// <returns>A <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> of the <see cref="T:System.Windows.Documents.Serialization.SerializerDescriptor" /> objects already registered. </returns>
	public ReadOnlyCollection<SerializerDescriptor> InstalledSerializers => _installedSerializers;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.Serialization.SerializerProvider" /> class. </summary>
	public SerializerProvider()
	{
		SerializerDescriptor serializerDescriptor = null;
		List<SerializerDescriptor> list = new List<SerializerDescriptor>();
		serializerDescriptor = CreateSystemSerializerDescriptor();
		if (serializerDescriptor != null)
		{
			list.Add(serializerDescriptor);
		}
		RegistryKey registryKey = _rootKey.CreateSubKey("SOFTWARE\\Microsoft\\WinFX Serializers");
		if (registryKey != null)
		{
			string[] subKeyNames = registryKey.GetSubKeyNames();
			foreach (string keyName in subKeyNames)
			{
				serializerDescriptor = SerializerDescriptor.CreateFromRegistry(registryKey, keyName);
				if (serializerDescriptor != null)
				{
					list.Add(serializerDescriptor);
				}
			}
			registryKey.Close();
		}
		_installedSerializers = list.AsReadOnly();
	}

	/// <summary>Registers a serializer plug-in. </summary>
	/// <param name="serializerDescriptor">The <see cref="T:System.Windows.Documents.Serialization.SerializerDescriptor" /> for the plug-in.</param>
	/// <param name="overwrite">true to overwrite an existing registration for the same plug-in; otherwise, false. See Remarks.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serializerDescriptor" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="overwrite" /> is false and the plug-in is already registered.</exception>
	public static void RegisterSerializer(SerializerDescriptor serializerDescriptor, bool overwrite)
	{
		if (serializerDescriptor == null)
		{
			throw new ArgumentNullException("serializerDescriptor");
		}
		RegistryKey registryKey = _rootKey.CreateSubKey("SOFTWARE\\Microsoft\\WinFX Serializers");
		string text = serializerDescriptor.DisplayName + "/" + serializerDescriptor.AssemblyName + "/" + serializerDescriptor.AssemblyVersion?.ToString() + "/" + serializerDescriptor.WinFXVersion;
		if (!overwrite && registryKey.OpenSubKey(text) != null)
		{
			throw new ArgumentException(SR.SerializerProviderAlreadyRegistered, text);
		}
		RegistryKey registryKey2 = registryKey.CreateSubKey(text);
		serializerDescriptor.WriteToRegistryKey(registryKey2);
		registryKey2.Close();
	}

	/// <summary>Deletes a serializer plug-in from the registry.</summary>
	/// <param name="serializerDescriptor">The <see cref="T:System.Windows.Documents.Serialization.SerializerDescriptor" /> for the plug-in.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="serializerDescriptor" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The plug-in is not registered. See Remarks.</exception>
	public static void UnregisterSerializer(SerializerDescriptor serializerDescriptor)
	{
		if (serializerDescriptor == null)
		{
			throw new ArgumentNullException("serializerDescriptor");
		}
		RegistryKey registryKey = _rootKey.CreateSubKey("SOFTWARE\\Microsoft\\WinFX Serializers");
		string text = serializerDescriptor.DisplayName + "/" + serializerDescriptor.AssemblyName + "/" + serializerDescriptor.AssemblyVersion?.ToString() + "/" + serializerDescriptor.WinFXVersion;
		if (registryKey.OpenSubKey(text) == null)
		{
			throw new ArgumentException(SR.SerializerProviderNotRegistered, text);
		}
		registryKey.DeleteSubKeyTree(text);
	}

	/// <summary>Initializes an object derived from the abstract <see cref="T:System.Windows.Documents.Serialization.SerializerWriter" /> class for the specified <see cref="T:System.IO.Stream" /> that will use the specified descriptor.</summary>
	/// <returns>An object of a class derived from <see cref="T:System.Windows.Documents.Serialization.SerializerWriter" />.</returns>
	/// <param name="serializerDescriptor">A <see cref="T:System.Windows.Documents.Serialization.SerializerDescriptor" /> that contains serialization information for the <see cref="T:System.Windows.Documents.Serialization.SerializerWriter" />.</param>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> to which the returned object writes.</param>
	/// <exception cref="T:System.ArgumentNullException">One of the parameters is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="serializerDescriptor" /> is specifying the wrong version.-or-It is not registered.-or-The assembly file cannot be found.-or-The assembly cannot be loaded.</exception>
	public SerializerWriter CreateSerializerWriter(SerializerDescriptor serializerDescriptor, Stream stream)
	{
		SerializerWriter serializerWriter = null;
		if (serializerDescriptor == null)
		{
			throw new ArgumentNullException("serializerDescriptor");
		}
		string paramName = serializerDescriptor.DisplayName + "/" + serializerDescriptor.AssemblyName + "/" + serializerDescriptor.AssemblyVersion?.ToString() + "/" + serializerDescriptor.WinFXVersion;
		if (!serializerDescriptor.IsLoadable)
		{
			throw new ArgumentException(SR.SerializerProviderWrongVersion, paramName);
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		bool flag = false;
		foreach (SerializerDescriptor installedSerializer in InstalledSerializers)
		{
			if (installedSerializer.Equals(serializerDescriptor))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			throw new ArgumentException(SR.SerializerProviderUnknownSerializer, paramName);
		}
		try
		{
			return serializerDescriptor.CreateSerializerFactory().CreateSerializerWriter(stream);
		}
		catch (FileNotFoundException)
		{
			throw new ArgumentException(SR.SerializerProviderCannotLoad, serializerDescriptor.DisplayName);
		}
		catch (FileLoadException)
		{
			throw new ArgumentException(SR.SerializerProviderCannotLoad, serializerDescriptor.DisplayName);
		}
		catch (BadImageFormatException)
		{
			throw new ArgumentException(SR.SerializerProviderCannotLoad, serializerDescriptor.DisplayName);
		}
		catch (MissingMethodException)
		{
			throw new ArgumentException(SR.SerializerProviderCannotLoad, serializerDescriptor.DisplayName);
		}
	}

	private SerializerDescriptor CreateSystemSerializerDescriptor()
	{
		return SerializerDescriptor.CreateFromFactoryInstance(new XpsSerializerFactory());
	}
}
