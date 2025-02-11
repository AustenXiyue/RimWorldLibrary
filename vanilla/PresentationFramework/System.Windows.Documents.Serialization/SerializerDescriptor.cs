using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using Microsoft.Win32;

namespace System.Windows.Documents.Serialization;

/// <summary>Provides information about installed plug-in serializers.    </summary>
public sealed class SerializerDescriptor
{
	private string _displayName;

	private string _manufacturerName;

	private Uri _manufacturerWebsite;

	private string _defaultFileExtension;

	private string _assemblyName;

	private string _assemblyPath;

	private string _factoryInterfaceName;

	private Version _assemblyVersion;

	private Version _winFXVersion;

	private bool _isLoadable;

	/// <summary>Gets the public display name of the serializer. </summary>
	/// <returns>The public display name of the serializer. </returns>
	public string DisplayName => _displayName;

	/// <summary>Gets the name of the company that developed the serializer. </summary>
	/// <returns>The name of the company that developed the plug-in serializer. </returns>
	public string ManufacturerName => _manufacturerName;

	/// <summary>Gets the web address of the company that developed the serializer. </summary>
	/// <returns>The web address of the company that developed the serializer. </returns>
	public Uri ManufacturerWebsite => _manufacturerWebsite;

	/// <summary>Gets the default extension associated with files that the serializer outputs. </summary>
	/// <returns>The default extension associated with files that the serializer outputs. </returns>
	public string DefaultFileExtension => _defaultFileExtension;

	/// <summary>Gets the name of the assembly that contains the serializer. </summary>
	/// <returns>The name of the assembly (usually a DLL) that contains the plug-in serializer. </returns>
	public string AssemblyName => _assemblyName;

	/// <summary>Gets the path to the assembly file that contains the serializer. </summary>
	/// <returns>The path to the assembly file that contains the plug-in serializer. </returns>
	public string AssemblyPath => _assemblyPath;

	/// <summary>Gets the name of the <see cref="T:System.Windows.Documents.Serialization.ISerializerFactory" /> derived class that implements the serializer. </summary>
	/// <returns>The name of the <see cref="T:System.Windows.Documents.Serialization.ISerializerFactory" /> derived class that implements the serializer. </returns>
	public string FactoryInterfaceName => _factoryInterfaceName;

	/// <summary>Gets the version of the assembly that contains the serializer. </summary>
	/// <returns>The version of the assembly that contains the plug-in serializer. </returns>
	public Version AssemblyVersion => _assemblyVersion;

	/// <summary>Gets the version of Microsoft .NET Framework required by the serializer.</summary>
	/// <returns>The version of Microsoft .NET Framework required by the plug-in serializer. </returns>
	public Version WinFXVersion => _winFXVersion;

	/// <summary>Gets a value indicating whether the serializer can be loaded with the currently installed version of Microsoft .NET Framework.</summary>
	/// <returns>true if the serializer assembly can be loaded; otherwise, false.  The default is false.</returns>
	public bool IsLoadable => _isLoadable;

	private SerializerDescriptor()
	{
	}

	private static string GetNonEmptyRegistryString(RegistryKey key, string value)
	{
		return (key.GetValue(value) as string) ?? throw new KeyNotFoundException();
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Documents.Serialization.SerializerDescriptor" /> through a given <see cref="T:System.Windows.Documents.Serialization.ISerializerFactory" /> implementation. </summary>
	/// <returns>A new <see cref="T:System.Windows.Documents.Serialization.SerializerDescriptor" /> with its properties initialized with values from the given <see cref="T:System.Windows.Documents.Serialization.ISerializerFactory" /> implementation. </returns>
	/// <param name="factoryInstance">The source of data for the new <see cref="T:System.Windows.Documents.Serialization.SerializerDescriptor" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="factoryInstance" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">One or more of the following properties of the <paramref name="factoryInstance" /> is null: <see cref="P:System.Windows.Documents.Serialization.SerializerDescriptor.DisplayName" />, <see cref="P:System.Windows.Documents.Serialization.SerializerDescriptor.ManufacturerName" />, <see cref="P:System.Windows.Documents.Serialization.SerializerDescriptor.ManufacturerWebsite" />, and <see cref="P:System.Windows.Documents.Serialization.SerializerDescriptor.DefaultFileExtension" /></exception>
	public static SerializerDescriptor CreateFromFactoryInstance(ISerializerFactory factoryInstance)
	{
		if (factoryInstance == null)
		{
			throw new ArgumentNullException("factoryInstance");
		}
		if (factoryInstance.DisplayName == null)
		{
			throw new ArgumentException(SR.SerializerProviderDisplayNameNull);
		}
		if (factoryInstance.ManufacturerName == null)
		{
			throw new ArgumentException(SR.SerializerProviderManufacturerNameNull);
		}
		if (factoryInstance.ManufacturerWebsite == null)
		{
			throw new ArgumentException(SR.SerializerProviderManufacturerWebsiteNull);
		}
		if (factoryInstance.DefaultFileExtension == null)
		{
			throw new ArgumentException(SR.SerializerProviderDefaultFileExtensionNull);
		}
		SerializerDescriptor obj = new SerializerDescriptor
		{
			_displayName = factoryInstance.DisplayName,
			_manufacturerName = factoryInstance.ManufacturerName,
			_manufacturerWebsite = factoryInstance.ManufacturerWebsite,
			_defaultFileExtension = factoryInstance.DefaultFileExtension,
			_isLoadable = true
		};
		Type type = factoryInstance.GetType();
		obj._assemblyName = type.Assembly.FullName;
		obj._assemblyPath = type.Assembly.Location;
		obj._assemblyVersion = type.Assembly.GetName().Version;
		obj._factoryInterfaceName = type.FullName;
		obj._winFXVersion = typeof(Button).Assembly.GetName().Version;
		return obj;
	}

	internal ISerializerFactory CreateSerializerFactory()
	{
		return Assembly.LoadFrom(AssemblyPath).CreateInstance(FactoryInterfaceName) as ISerializerFactory;
	}

	internal void WriteToRegistryKey(RegistryKey key)
	{
		key.SetValue("uiLanguage", CultureInfo.CurrentUICulture.Name);
		key.SetValue("displayName", DisplayName);
		key.SetValue("manufacturerName", ManufacturerName);
		key.SetValue("manufacturerWebsite", ManufacturerWebsite);
		key.SetValue("defaultFileExtension", DefaultFileExtension);
		key.SetValue("assemblyName", AssemblyName);
		key.SetValue("assemblyPath", AssemblyPath);
		key.SetValue("factoryInterfaceName", FactoryInterfaceName);
		key.SetValue("assemblyVersion", AssemblyVersion.ToString());
		key.SetValue("winFXVersion", WinFXVersion.ToString());
	}

	internal static SerializerDescriptor CreateFromRegistry(RegistryKey plugIns, string keyName)
	{
		SerializerDescriptor serializerDescriptor = new SerializerDescriptor();
		try
		{
			RegistryKey registryKey = plugIns.OpenSubKey(keyName);
			serializerDescriptor._displayName = GetNonEmptyRegistryString(registryKey, "displayName");
			serializerDescriptor._manufacturerName = GetNonEmptyRegistryString(registryKey, "manufacturerName");
			serializerDescriptor._manufacturerWebsite = new Uri(GetNonEmptyRegistryString(registryKey, "manufacturerWebsite"));
			serializerDescriptor._defaultFileExtension = GetNonEmptyRegistryString(registryKey, "defaultFileExtension");
			serializerDescriptor._assemblyName = GetNonEmptyRegistryString(registryKey, "assemblyName");
			serializerDescriptor._assemblyPath = GetNonEmptyRegistryString(registryKey, "assemblyPath");
			serializerDescriptor._factoryInterfaceName = GetNonEmptyRegistryString(registryKey, "factoryInterfaceName");
			serializerDescriptor._assemblyVersion = new Version(GetNonEmptyRegistryString(registryKey, "assemblyVersion"));
			serializerDescriptor._winFXVersion = new Version(GetNonEmptyRegistryString(registryKey, "winFXVersion"));
			string nonEmptyRegistryString = GetNonEmptyRegistryString(registryKey, "uiLanguage");
			registryKey.Close();
			if (!nonEmptyRegistryString.Equals(CultureInfo.CurrentUICulture.Name))
			{
				ISerializerFactory serializerFactory = serializerDescriptor.CreateSerializerFactory();
				serializerDescriptor._displayName = serializerFactory.DisplayName;
				serializerDescriptor._manufacturerName = serializerFactory.ManufacturerName;
				serializerDescriptor._manufacturerWebsite = serializerFactory.ManufacturerWebsite;
				serializerDescriptor._defaultFileExtension = serializerFactory.DefaultFileExtension;
				registryKey = plugIns.CreateSubKey(keyName);
				serializerDescriptor.WriteToRegistryKey(registryKey);
				registryKey.Close();
			}
		}
		catch (KeyNotFoundException)
		{
			serializerDescriptor = null;
		}
		if (serializerDescriptor != null)
		{
			Assembly assembly = Assembly.ReflectionOnlyLoadFrom(serializerDescriptor._assemblyPath);
			if (typeof(Button).Assembly.GetName().Version == serializerDescriptor._winFXVersion && assembly != null && assembly.GetName().Version == serializerDescriptor._assemblyVersion)
			{
				serializerDescriptor._isLoadable = true;
			}
		}
		return serializerDescriptor;
	}

	/// <summary>Tests two <see cref="T:System.Windows.Documents.Serialization.SerializerDescriptor" /> objects for equality.</summary>
	/// <returns>true if both are equal; otherwise, false. </returns>
	/// <param name="obj">The object to be compared with this <see cref="T:System.Windows.Documents.Serialization.SerializerDescriptor" />.</param>
	public override bool Equals(object obj)
	{
		if (obj is SerializerDescriptor serializerDescriptor)
		{
			if (serializerDescriptor._displayName == _displayName && serializerDescriptor._assemblyName == _assemblyName && serializerDescriptor._assemblyPath == _assemblyPath && serializerDescriptor._factoryInterfaceName == _factoryInterfaceName && serializerDescriptor._defaultFileExtension == _defaultFileExtension && serializerDescriptor._assemblyVersion == _assemblyVersion)
			{
				return serializerDescriptor._winFXVersion == _winFXVersion;
			}
			return false;
		}
		return false;
	}

	/// <summary>Gets the unique hash code value of the serializer. </summary>
	/// <returns>The unique hash code value of the serializer. </returns>
	public override int GetHashCode()
	{
		return (_displayName + "/" + _assemblyName + "/" + _assemblyPath + "/" + _factoryInterfaceName + "/" + _assemblyVersion?.ToString() + "/" + _winFXVersion).GetHashCode();
	}
}
