using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;

namespace System.Diagnostics;

/// <summary>Provides an abstract base class to create new debugging and tracing switches.</summary>
/// <filterpriority>2</filterpriority>
public abstract class Switch
{
	private SwitchElementsCollection switchSettings;

	private readonly string description;

	private readonly string displayName;

	private int switchSetting;

	private volatile bool initialized;

	private bool initializing;

	private volatile string switchValueString = string.Empty;

	private StringDictionary attributes;

	private string defaultValue;

	private object m_intializedLock;

	private static List<WeakReference> switches = new List<WeakReference>();

	private static int s_LastCollectionCount;

	private object IntializedLock
	{
		get
		{
			if (m_intializedLock == null)
			{
				object value = new object();
				Interlocked.CompareExchange<object>(ref m_intializedLock, value, (object)null);
			}
			return m_intializedLock;
		}
	}

	/// <summary>Gets the custom switch attributes defined in the application configuration file.</summary>
	/// <returns>A <see cref="T:System.Collections.Specialized.StringDictionary" /> containing the case-insensitive custom attributes for the trace switch.</returns>
	/// <filterpriority>1</filterpriority>
	[XmlIgnore]
	public StringDictionary Attributes
	{
		get
		{
			Initialize();
			if (attributes == null)
			{
				attributes = new StringDictionary();
			}
			return attributes;
		}
	}

	/// <summary>Gets a name used to identify the switch.</summary>
	/// <returns>The name used to identify the switch. The default value is an empty string ("").</returns>
	/// <filterpriority>2</filterpriority>
	public string DisplayName => displayName;

	/// <summary>Gets a description of the switch.</summary>
	/// <returns>The description of the switch. The default value is an empty string ("").</returns>
	/// <filterpriority>2</filterpriority>
	public string Description
	{
		get
		{
			if (description != null)
			{
				return description;
			}
			return string.Empty;
		}
	}

	/// <summary>Gets or sets the current setting for this switch.</summary>
	/// <returns>The current setting for this switch. The default is zero.</returns>
	protected int SwitchSetting
	{
		get
		{
			if (!initialized && InitializeWithStatus())
			{
				OnSwitchSettingChanged();
			}
			return switchSetting;
		}
		set
		{
			bool flag = false;
			lock (IntializedLock)
			{
				initialized = true;
				if (switchSetting != value)
				{
					switchSetting = value;
					flag = true;
				}
			}
			if (flag)
			{
				OnSwitchSettingChanged();
			}
		}
	}

	/// <summary>Gets or sets the value of the switch.</summary>
	/// <returns>A string representing the value of the switch.</returns>
	/// <exception cref="T:System.Configuration.ConfigurationErrorsException">The value is null.-or-The value does not consist solely of an optional negative sign followed by a sequence of digits ranging from 0 to 9.-or-The value represents a number less than <see cref="F:System.Int32.MinValue" /> or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
	protected string Value
	{
		get
		{
			Initialize();
			return switchValueString;
		}
		set
		{
			Initialize();
			switchValueString = value;
			try
			{
				OnValueChanged();
			}
			catch (ArgumentException inner)
			{
				throw new ConfigurationErrorsException(global::SR.GetString("The config value for Switch '{0}' was invalid.", DisplayName), inner);
			}
			catch (FormatException inner2)
			{
				throw new ConfigurationErrorsException(global::SR.GetString("The config value for Switch '{0}' was invalid.", DisplayName), inner2);
			}
			catch (OverflowException inner3)
			{
				throw new ConfigurationErrorsException(global::SR.GetString("The config value for Switch '{0}' was invalid.", DisplayName), inner3);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Switch" /> class.</summary>
	/// <param name="displayName">The name of the switch. </param>
	/// <param name="description">The description for the switch. </param>
	protected Switch(string displayName, string description)
		: this(displayName, description, "0")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Switch" /> class, specifying the display name, description, and default value for the switch. </summary>
	/// <param name="displayName">The name of the switch. </param>
	/// <param name="description">The description of the switch. </param>
	/// <param name="defaultSwitchValue">The default value for the switch.</param>
	protected Switch(string displayName, string description, string defaultSwitchValue)
	{
		if (displayName == null)
		{
			displayName = string.Empty;
		}
		this.displayName = displayName;
		this.description = description;
		lock (switches)
		{
			_pruneCachedSwitches();
			switches.Add(new WeakReference(this));
		}
		defaultValue = defaultSwitchValue;
	}

	private static void _pruneCachedSwitches()
	{
		lock (switches)
		{
			if (s_LastCollectionCount == GC.CollectionCount(2))
			{
				return;
			}
			List<WeakReference> list = new List<WeakReference>(switches.Count);
			for (int i = 0; i < switches.Count; i++)
			{
				if ((Switch)switches[i].Target != null)
				{
					list.Add(switches[i]);
				}
			}
			if (list.Count < switches.Count)
			{
				switches.Clear();
				switches.AddRange(list);
				switches.TrimExcess();
			}
			s_LastCollectionCount = GC.CollectionCount(2);
		}
	}

	private void Initialize()
	{
		InitializeWithStatus();
	}

	private bool InitializeWithStatus()
	{
		if (!initialized)
		{
			lock (IntializedLock)
			{
				if (initialized || initializing)
				{
					return false;
				}
				initializing = true;
				if (switchSettings == null && !InitializeConfigSettings())
				{
					initialized = true;
					initializing = false;
					return false;
				}
				if (switchSettings != null)
				{
					SwitchElement switchElement = switchSettings[displayName];
					if (switchElement != null)
					{
						string value = switchElement.Value;
						if (value != null)
						{
							Value = value;
						}
						else
						{
							Value = defaultValue;
						}
						try
						{
							TraceUtils.VerifyAttributes(switchElement.Attributes, GetSupportedAttributes(), this);
						}
						catch (ConfigurationException)
						{
							initialized = false;
							initializing = false;
							throw;
						}
						attributes = new StringDictionary();
						attributes.ReplaceHashtable(switchElement.Attributes);
					}
					else
					{
						switchValueString = defaultValue;
						OnValueChanged();
					}
				}
				else
				{
					switchValueString = defaultValue;
					OnValueChanged();
				}
				initialized = true;
				initializing = false;
			}
		}
		return true;
	}

	private bool InitializeConfigSettings()
	{
		if (switchSettings != null)
		{
			return true;
		}
		if (!DiagnosticsConfiguration.CanInitialize())
		{
			return false;
		}
		switchSettings = DiagnosticsConfiguration.SwitchSettings;
		return true;
	}

	/// <summary>Gets the custom attributes supported by the switch.</summary>
	/// <returns>A string array that contains the names of the custom attributes supported by the switch, or null if there no custom attributes are supported.</returns>
	protected internal virtual string[] GetSupportedAttributes()
	{
		return null;
	}

	/// <summary>Invoked when the <see cref="P:System.Diagnostics.Switch.SwitchSetting" /> property is changed.</summary>
	protected virtual void OnSwitchSettingChanged()
	{
	}

	/// <summary>Invoked when the <see cref="P:System.Diagnostics.Switch.Value" /> property is changed.</summary>
	protected virtual void OnValueChanged()
	{
		SwitchSetting = int.Parse(Value, CultureInfo.InvariantCulture);
	}

	internal static void RefreshAll()
	{
		lock (switches)
		{
			_pruneCachedSwitches();
			for (int i = 0; i < switches.Count; i++)
			{
				((Switch)switches[i].Target)?.Refresh();
			}
		}
	}

	internal void Refresh()
	{
		lock (IntializedLock)
		{
			initialized = false;
			switchSettings = null;
			Initialize();
		}
	}
}
