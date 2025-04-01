using System.Collections;
using System.Configuration.Internal;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Permissions;
using System.Xml;
using Unity;

namespace System.Configuration;

/// <summary>Represents a configuration file that is applicable to a particular computer, application, or resource. This class cannot be inherited.</summary>
public sealed class Configuration
{
	private Configuration parent;

	private Hashtable elementData;

	private string streamName;

	private ConfigurationSectionGroup rootSectionGroup;

	private ConfigurationLocationCollection locations;

	private SectionGroupInfo rootGroup;

	private IConfigSystem system;

	private bool hasFile;

	private string rootNamespace;

	private string configPath;

	private string locationConfigPath;

	private string locationSubPath;

	private ContextInformation evaluationContext;

	internal Configuration Parent
	{
		get
		{
			return parent;
		}
		set
		{
			parent = value;
		}
	}

	internal string FileName => streamName;

	internal IInternalConfigHost ConfigHost => system.Host;

	internal string LocationConfigPath => locationConfigPath;

	internal string ConfigPath => configPath;

	/// <summary>Gets the <see cref="T:System.Configuration.AppSettingsSection" /> object configuration section that applies to this <see cref="T:System.Configuration.Configuration" /> object.</summary>
	/// <returns>An <see cref="T:System.Configuration.AppSettingsSection" /> object representing the appSettings configuration section that applies to this <see cref="T:System.Configuration.Configuration" /> object.</returns>
	public AppSettingsSection AppSettings => (AppSettingsSection)GetSection("appSettings");

	/// <summary>Gets a <see cref="T:System.Configuration.ConnectionStringsSection" /> configuration-section object that applies to this <see cref="T:System.Configuration.Configuration" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.ConnectionStringsSection" /> configuration-section object representing the connectionStrings configuration section that applies to this <see cref="T:System.Configuration.Configuration" /> object.</returns>
	public ConnectionStringsSection ConnectionStrings => (ConnectionStringsSection)GetSection("connectionStrings");

	/// <summary>Gets the physical path to the configuration file represented by this <see cref="T:System.Configuration.Configuration" /> object.</summary>
	/// <returns>The physical path to the configuration file represented by this <see cref="T:System.Configuration.Configuration" /> object.</returns>
	public string FilePath
	{
		get
		{
			if (streamName == null && parent != null)
			{
				return parent.FilePath;
			}
			return streamName;
		}
	}

	/// <summary>Gets a value that indicates whether a file exists for the resource represented by this <see cref="T:System.Configuration.Configuration" /> object.</summary>
	/// <returns>true if there is a configuration file; otherwise, false.</returns>
	public bool HasFile => hasFile;

	/// <summary>Gets the <see cref="T:System.Configuration.ContextInformation" /> object for the <see cref="T:System.Configuration.Configuration" /> object.</summary>
	/// <returns>The <see cref="T:System.Configuration.ContextInformation" /> object for the <see cref="T:System.Configuration.Configuration" /> object.</returns>
	public ContextInformation EvaluationContext
	{
		get
		{
			if (evaluationContext == null)
			{
				object ctx = system.Host.CreateConfigurationContext(configPath, GetLocationSubPath());
				evaluationContext = new ContextInformation(this, ctx);
			}
			return evaluationContext;
		}
	}

	/// <summary>Gets the locations defined within this <see cref="T:System.Configuration.Configuration" /> object.</summary>
	/// <returns>A <see cref="T:System.Configuration.ConfigurationLocationCollection" /> containing the locations defined within this <see cref="T:System.Configuration.Configuration" /> object.</returns>
	public ConfigurationLocationCollection Locations
	{
		get
		{
			if (locations == null)
			{
				locations = new ConfigurationLocationCollection();
			}
			return locations;
		}
	}

	/// <summary>Gets or sets a value indicating whether the configuration file has an XML namespace.</summary>
	/// <returns>true if the configuration file has an XML namespace; otherwise, false.</returns>
	public bool NamespaceDeclared
	{
		get
		{
			return rootNamespace != null;
		}
		set
		{
			rootNamespace = (value ? "http://schemas.microsoft.com/.NetConfiguration/v2.0" : null);
		}
	}

	/// <summary>Gets the root <see cref="T:System.Configuration.ConfigurationSectionGroup" /> for this <see cref="T:System.Configuration.Configuration" /> object.</summary>
	/// <returns>The root section group for this <see cref="T:System.Configuration.Configuration" /> object.</returns>
	public ConfigurationSectionGroup RootSectionGroup
	{
		get
		{
			if (rootSectionGroup == null)
			{
				rootSectionGroup = new ConfigurationSectionGroup();
				rootSectionGroup.Initialize(this, rootGroup);
			}
			return rootSectionGroup;
		}
	}

	/// <summary>Gets a collection of the section groups defined by this configuration.</summary>
	/// <returns>A <see cref="T:System.Configuration.ConfigurationSectionGroupCollection" /> collection representing the collection of section groups for this <see cref="T:System.Configuration.Configuration" /> object.</returns>
	public ConfigurationSectionGroupCollection SectionGroups => RootSectionGroup.SectionGroups;

	/// <summary>Gets a collection of the sections defined by this <see cref="T:System.Configuration.Configuration" /> object.</summary>
	/// <returns>A collection of the sections defined by this <see cref="T:System.Configuration.Configuration" /> object.</returns>
	public ConfigurationSectionCollection Sections => RootSectionGroup.Sections;

	/// <summary>Specifies a function delegate that is used to transform assembly strings in configuration files.</summary>
	/// <returns>A delegate that transforms type strings. The default value is null.</returns>
	public Func<string, string> AssemblyStringTransformer
	{
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (Func<string, string>)0;
		}
		[ConfigurationPermission(SecurityAction.Demand, Unrestricted = true)]
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Specifies the targeted version of the .NET Framework when a version earlier than the current one is targeted.</summary>
	/// <returns>The name of the targeted version of the .NET Framework. The default value is null, which indicates that the current version is targeted.</returns>
	public FrameworkName TargetFramework
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
		[ConfigurationPermission(SecurityAction.Demand, Unrestricted = true)]
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>Specifies a function delegate that is used to transform type strings in configuration files. </summary>
	/// <returns>A delegate that transforms type strings. The default value is null.</returns>
	public Func<string, string> TypeStringTransformer
	{
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (Func<string, string>)0;
		}
		[ConfigurationPermission(SecurityAction.Demand, Unrestricted = true)]
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	internal static event ConfigurationSaveEventHandler SaveStart;

	internal static event ConfigurationSaveEventHandler SaveEnd;

	internal Configuration(Configuration parent, string locationSubPath)
	{
		elementData = new Hashtable();
		base._002Ector();
		this.parent = parent;
		system = parent.system;
		rootGroup = parent.rootGroup;
		this.locationSubPath = locationSubPath;
		configPath = parent.ConfigPath;
	}

	internal Configuration(InternalConfigurationSystem system, string locationSubPath)
	{
		elementData = new Hashtable();
		base._002Ector();
		hasFile = true;
		this.system = system;
		system.InitForConfiguration(ref locationSubPath, out configPath, out locationConfigPath);
		Configuration configuration = null;
		if (locationSubPath != null)
		{
			configuration = new Configuration(system, locationSubPath);
			if (locationConfigPath != null)
			{
				configuration = configuration.FindLocationConfiguration(locationConfigPath, configuration);
			}
		}
		Init(system, configPath, configuration);
	}

	internal Configuration FindLocationConfiguration(string relativePath, Configuration defaultConfiguration)
	{
		Configuration configuration = defaultConfiguration;
		if (!string.IsNullOrEmpty(LocationConfigPath))
		{
			Configuration parentWithFile = GetParentWithFile();
			if (parentWithFile != null)
			{
				string configPathFromLocationSubPath = system.Host.GetConfigPathFromLocationSubPath(configPath, relativePath);
				configuration = parentWithFile.FindLocationConfiguration(configPathFromLocationSubPath, defaultConfiguration);
			}
		}
		string text = configPath.Substring(1) + "/";
		if (relativePath.StartsWith(text, StringComparison.Ordinal))
		{
			relativePath = relativePath.Substring(text.Length);
		}
		ConfigurationLocation configurationLocation = Locations.FindBest(relativePath);
		if (configurationLocation == null)
		{
			return configuration;
		}
		configurationLocation.SetParentConfiguration(configuration);
		return configurationLocation.OpenConfiguration();
	}

	internal void Init(IConfigSystem system, string configPath, Configuration parent)
	{
		this.system = system;
		this.configPath = configPath;
		streamName = system.Host.GetStreamName(configPath);
		this.parent = parent;
		if (parent != null)
		{
			rootGroup = parent.rootGroup;
		}
		else
		{
			rootGroup = new SectionGroupInfo();
			rootGroup.StreamName = streamName;
		}
		try
		{
			if (streamName != null)
			{
				Load();
			}
		}
		catch (XmlException ex)
		{
			throw new ConfigurationErrorsException(ex.Message, ex, streamName, 0);
		}
	}

	internal Configuration GetParentWithFile()
	{
		Configuration configuration = Parent;
		while (configuration != null && !configuration.HasFile)
		{
			configuration = configuration.Parent;
		}
		return configuration;
	}

	internal string GetLocationSubPath()
	{
		Configuration configuration = parent;
		string text = null;
		while (configuration != null)
		{
			text = configuration.locationSubPath;
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			configuration = configuration.parent;
		}
		return text;
	}

	/// <summary>Returns the specified <see cref="T:System.Configuration.ConfigurationSection" /> object.</summary>
	/// <returns>The specified <see cref="T:System.Configuration.ConfigurationSection" /> object.</returns>
	/// <param name="sectionName">The path to the section to be returned.</param>
	public ConfigurationSection GetSection(string sectionName)
	{
		string[] array = sectionName.Split('/');
		if (array.Length == 1)
		{
			return Sections[array[0]];
		}
		ConfigurationSectionGroup configurationSectionGroup = SectionGroups[array[0]];
		int num = 1;
		while (configurationSectionGroup != null && num < array.Length - 1)
		{
			configurationSectionGroup = configurationSectionGroup.SectionGroups[array[num]];
			num++;
		}
		return configurationSectionGroup?.Sections[array[array.Length - 1]];
	}

	/// <summary>Gets the specified <see cref="T:System.Configuration.ConfigurationSectionGroup" /> object.</summary>
	/// <returns>The <see cref="T:System.Configuration.ConfigurationSectionGroup" /> specified.</returns>
	/// <param name="sectionGroupName">The path name of the <see cref="T:System.Configuration.ConfigurationSectionGroup" /> to return.</param>
	public ConfigurationSectionGroup GetSectionGroup(string sectionGroupName)
	{
		string[] array = sectionGroupName.Split('/');
		ConfigurationSectionGroup configurationSectionGroup = SectionGroups[array[0]];
		int num = 1;
		while (configurationSectionGroup != null && num < array.Length)
		{
			configurationSectionGroup = configurationSectionGroup.SectionGroups[array[num]];
			num++;
		}
		return configurationSectionGroup;
	}

	internal ConfigurationSection GetSectionInstance(SectionInfo config, bool createDefaultInstance)
	{
		object obj = elementData[config];
		ConfigurationSection configurationSection = obj as ConfigurationSection;
		if (configurationSection != null || !createDefaultInstance)
		{
			return configurationSection;
		}
		object obj2 = config.CreateInstance();
		configurationSection = obj2 as ConfigurationSection;
		if (configurationSection == null)
		{
			configurationSection = new DefaultSection
			{
				SectionHandler = (obj2 as IConfigurationSectionHandler)
			};
		}
		configurationSection.Configuration = this;
		ConfigurationSection configurationSection2 = null;
		if (parent != null)
		{
			configurationSection2 = parent.GetSectionInstance(config, createDefaultInstance: true);
			configurationSection.SectionInformation.SetParentSection(configurationSection2);
		}
		configurationSection.SectionInformation.ConfigFilePath = FilePath;
		configurationSection.ConfigContext = system.Host.CreateDeprecatedConfigContext(configPath);
		string text2 = (configurationSection.RawXml = obj as string);
		configurationSection.Reset(configurationSection2);
		if (text2 != null)
		{
			XmlTextReader xmlTextReader = new ConfigXmlTextReader(new StringReader(text2), FilePath);
			configurationSection.DeserializeSection(xmlTextReader);
			xmlTextReader.Close();
			if (!string.IsNullOrEmpty(configurationSection.SectionInformation.ConfigSource) && !string.IsNullOrEmpty(FilePath))
			{
				configurationSection.DeserializeConfigSource(Path.GetDirectoryName(FilePath));
			}
		}
		elementData[config] = configurationSection;
		return configurationSection;
	}

	internal ConfigurationSectionGroup GetSectionGroupInstance(SectionGroupInfo group)
	{
		ConfigurationSectionGroup configurationSectionGroup = group.CreateInstance() as ConfigurationSectionGroup;
		configurationSectionGroup?.Initialize(this, group);
		return configurationSectionGroup;
	}

	internal void SetConfigurationSection(SectionInfo config, ConfigurationSection sec)
	{
		elementData[config] = sec;
	}

	internal void SetSectionXml(SectionInfo config, string data)
	{
		elementData[config] = data;
	}

	internal string GetSectionXml(SectionInfo config)
	{
		return elementData[config] as string;
	}

	internal void CreateSection(SectionGroupInfo group, string name, ConfigurationSection sec)
	{
		if (group.HasChild(name))
		{
			throw new ConfigurationErrorsException("Cannot add a ConfigurationSection. A section or section group already exists with the name '" + name + "'");
		}
		if (!HasFile && !sec.SectionInformation.AllowLocation)
		{
			throw new ConfigurationErrorsException("The configuration section <" + name + "> cannot be defined inside a <location> element.");
		}
		if (!system.Host.IsDefinitionAllowed(configPath, sec.SectionInformation.AllowDefinition, sec.SectionInformation.AllowExeDefinition))
		{
			object obj = ((sec.SectionInformation.AllowExeDefinition != ConfigurationAllowExeDefinition.MachineToApplication) ? ((object)sec.SectionInformation.AllowExeDefinition) : ((object)sec.SectionInformation.AllowDefinition));
			throw new ConfigurationErrorsException(string.Concat("The section <", name, "> can't be defined in this configuration file (the allowed definition context is '", obj, "')."));
		}
		if (sec.SectionInformation.Type == null)
		{
			sec.SectionInformation.Type = system.Host.GetConfigTypeName(sec.GetType());
		}
		SectionInfo sectionInfo = new SectionInfo(name, sec.SectionInformation);
		sectionInfo.StreamName = streamName;
		sectionInfo.ConfigHost = system.Host;
		group.AddChild(sectionInfo);
		elementData[sectionInfo] = sec;
		sec.Configuration = this;
	}

	internal void CreateSectionGroup(SectionGroupInfo parentGroup, string name, ConfigurationSectionGroup sec)
	{
		if (parentGroup.HasChild(name))
		{
			throw new ConfigurationErrorsException("Cannot add a ConfigurationSectionGroup. A section or section group already exists with the name '" + name + "'");
		}
		if (sec.Type == null)
		{
			sec.Type = system.Host.GetConfigTypeName(sec.GetType());
		}
		sec.SetName(name);
		SectionGroupInfo sectionGroupInfo = new SectionGroupInfo(name, sec.Type);
		sectionGroupInfo.StreamName = streamName;
		sectionGroupInfo.ConfigHost = system.Host;
		parentGroup.AddChild(sectionGroupInfo);
		elementData[sectionGroupInfo] = sec;
		sec.Initialize(this, sectionGroupInfo);
	}

	internal void RemoveConfigInfo(ConfigInfo config)
	{
		elementData.Remove(config);
	}

	/// <summary>Writes the configuration settings contained within this <see cref="T:System.Configuration.Configuration" /> object to the current XML configuration file.</summary>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The configuration file could not be written to.- or -The configuration file has changed. </exception>
	public void Save()
	{
		Save(ConfigurationSaveMode.Modified, forceSaveAll: false);
	}

	/// <summary>Writes the configuration settings contained within this <see cref="T:System.Configuration.Configuration" /> object to the current XML configuration file.</summary>
	/// <param name="saveMode">A <see cref="T:System.Configuration.ConfigurationSaveMode" /> value that determines which property values to save.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The configuration file could not be written to.- or -The configuration file has changed. </exception>
	public void Save(ConfigurationSaveMode saveMode)
	{
		Save(saveMode, forceSaveAll: false);
	}

	/// <summary>Writes the configuration settings contained within this <see cref="T:System.Configuration.Configuration" /> object to the current XML configuration file.</summary>
	/// <param name="saveMode">A <see cref="T:System.Configuration.ConfigurationSaveMode" /> value that determines which property values to save.</param>
	/// <param name="forceSaveAll">true to save even if the configuration was not modified; otherwise, false.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The configuration file could not be written to.- or -The configuration file has changed. </exception>
	public void Save(ConfigurationSaveMode saveMode, bool forceSaveAll)
	{
		if (!forceSaveAll && saveMode != ConfigurationSaveMode.Full && !HasValues(saveMode))
		{
			ResetModified();
			return;
		}
		ConfigurationSaveEventHandler saveStart = Configuration.SaveStart;
		ConfigurationSaveEventHandler saveEnd = Configuration.SaveEnd;
		object writeContext = null;
		Exception ex = null;
		Stream stream = system.Host.OpenStreamForWrite(streamName, null, ref writeContext);
		try
		{
			saveStart?.Invoke(this, new ConfigurationSaveEventArgs(streamName, start: true, null, writeContext));
			Save(stream, saveMode, forceSaveAll);
			system.Host.WriteCompleted(streamName, success: true, writeContext);
		}
		catch (Exception ex2)
		{
			ex = ex2;
			system.Host.WriteCompleted(streamName, success: false, writeContext);
			throw;
		}
		finally
		{
			stream.Close();
			saveEnd?.Invoke(this, new ConfigurationSaveEventArgs(streamName, start: false, ex, writeContext));
		}
	}

	/// <summary>Writes the configuration settings contained within this <see cref="T:System.Configuration.Configuration" /> object to the specified XML configuration file.</summary>
	/// <param name="filename">The path and file name to save the configuration file to.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The configuration file could not be written to.- or -The configuration file has changed. </exception>
	public void SaveAs(string filename)
	{
		SaveAs(filename, ConfigurationSaveMode.Modified, forceSaveAll: false);
	}

	/// <summary>Writes the configuration settings contained within this <see cref="T:System.Configuration.Configuration" /> object to the specified XML configuration file.</summary>
	/// <param name="filename">The path and file name to save the configuration file to.</param>
	/// <param name="saveMode">A <see cref="T:System.Configuration.ConfigurationSaveMode" /> value that determines which property values to save.</param>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The configuration file could not be written to.- or -The configuration file has changed. </exception>
	public void SaveAs(string filename, ConfigurationSaveMode saveMode)
	{
		SaveAs(filename, saveMode, forceSaveAll: false);
	}

	/// <summary>Writes the configuration settings contained within this <see cref="T:System.Configuration.Configuration" /> object to the specified XML configuration file.</summary>
	/// <param name="filename">The path and file name to save the configuration file to.</param>
	/// <param name="saveMode">A <see cref="T:System.Configuration.ConfigurationSaveMode" /> value that determines which property values to save.</param>
	/// <param name="forceSaveAll">true to save even if the configuration was not modified; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="filename" /> is null or an empty string ("").</exception>
	[System.MonoInternalNote("Detect if file has changed")]
	public void SaveAs(string filename, ConfigurationSaveMode saveMode, bool forceSaveAll)
	{
		if (!forceSaveAll && saveMode != ConfigurationSaveMode.Full && !HasValues(saveMode))
		{
			ResetModified();
			return;
		}
		string directoryName = Path.GetDirectoryName(Path.GetFullPath(filename));
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		Save(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write), saveMode, forceSaveAll);
	}

	private void Save(Stream stream, ConfigurationSaveMode mode, bool forceUpdateAll)
	{
		XmlTextWriter xmlTextWriter = new XmlTextWriter(new StreamWriter(stream));
		xmlTextWriter.Formatting = Formatting.Indented;
		try
		{
			xmlTextWriter.WriteStartDocument();
			if (rootNamespace != null)
			{
				xmlTextWriter.WriteStartElement("configuration", rootNamespace);
			}
			else
			{
				xmlTextWriter.WriteStartElement("configuration");
			}
			if (rootGroup.HasConfigContent(this))
			{
				rootGroup.WriteConfig(this, xmlTextWriter, mode);
			}
			foreach (ConfigurationLocation location in Locations)
			{
				if (location.OpenedConfiguration == null)
				{
					xmlTextWriter.WriteRaw("\n");
					xmlTextWriter.WriteRaw(location.XmlContent);
					continue;
				}
				xmlTextWriter.WriteStartElement("location");
				xmlTextWriter.WriteAttributeString("path", location.Path);
				if (!location.AllowOverride)
				{
					xmlTextWriter.WriteAttributeString("allowOverride", "false");
				}
				location.OpenedConfiguration.SaveData(xmlTextWriter, mode, forceUpdateAll);
				xmlTextWriter.WriteEndElement();
			}
			SaveData(xmlTextWriter, mode, forceUpdateAll);
			xmlTextWriter.WriteEndElement();
			ResetModified();
		}
		finally
		{
			xmlTextWriter.Flush();
			xmlTextWriter.Close();
		}
	}

	private void SaveData(XmlTextWriter tw, ConfigurationSaveMode mode, bool forceUpdateAll)
	{
		rootGroup.WriteRootData(tw, this, mode);
	}

	private bool HasValues(ConfigurationSaveMode mode)
	{
		foreach (ConfigurationLocation location in Locations)
		{
			if (location.OpenedConfiguration != null && location.OpenedConfiguration.HasValues(mode))
			{
				return true;
			}
		}
		return rootGroup.HasValues(this, mode);
	}

	private void ResetModified()
	{
		foreach (ConfigurationLocation location in Locations)
		{
			if (location.OpenedConfiguration != null)
			{
				location.OpenedConfiguration.ResetModified();
			}
		}
		rootGroup.ResetModified(this);
	}

	private bool Load()
	{
		if (string.IsNullOrEmpty(streamName))
		{
			return true;
		}
		Stream stream = null;
		try
		{
			stream = system.Host.OpenStreamForRead(streamName);
			if (stream == null)
			{
				return false;
			}
		}
		catch
		{
			return false;
		}
		using (XmlTextReader reader = new ConfigXmlTextReader(stream, streamName))
		{
			ReadConfigFile(reader, streamName);
		}
		ResetModified();
		return true;
	}

	private void ReadConfigFile(XmlReader reader, string fileName)
	{
		reader.MoveToContent();
		if (reader.NodeType != XmlNodeType.Element || reader.Name != "configuration")
		{
			ThrowException("Configuration file does not have a valid root element", reader);
		}
		if (reader.HasAttributes)
		{
			while (reader.MoveToNextAttribute())
			{
				if (reader.LocalName == "xmlns")
				{
					rootNamespace = reader.Value;
				}
				else
				{
					ThrowException($"Unrecognized attribute '{reader.LocalName}' in root element", reader);
				}
			}
		}
		reader.MoveToElement();
		if (reader.IsEmptyElement)
		{
			reader.Skip();
			return;
		}
		reader.ReadStartElement();
		reader.MoveToContent();
		if (reader.LocalName == "configSections")
		{
			if (reader.HasAttributes)
			{
				ThrowException("Unrecognized attribute in <configSections>.", reader);
			}
			rootGroup.ReadConfig(this, fileName, reader);
		}
		rootGroup.ReadRootData(reader, this, overrideAllowed: true);
	}

	internal void ReadData(XmlReader reader, bool allowOverride)
	{
		rootGroup.ReadData(this, reader, allowOverride);
	}

	private void ThrowException(string text, XmlReader reader)
	{
		throw new ConfigurationErrorsException(text, streamName, (reader as IXmlLineInfo)?.LineNumber ?? 0);
	}

	internal Configuration()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
