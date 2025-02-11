using System.Windows.Markup;
using System.Xaml.Permissions;

namespace System.Xaml;

/// <summary>Provides specific XAML writer settings for <see cref="T:System.Xaml.XamlObjectWriter" />.</summary>
public class XamlObjectWriterSettings : XamlWriterSettings
{
	/// <summary>Gets or sets a reference to a callback that is invoked by the XAML writer at the <see cref="M:System.ComponentModel.ISupportInitialize.BeginInit" /> phase of the object lifetime for each created object.</summary>
	/// <returns>A callback that is invoked by the XAML writer at the <see cref="M:System.ComponentModel.ISupportInitialize.BeginInit" /> phase of object lifetime.</returns>
	public EventHandler<XamlObjectEventArgs> AfterBeginInitHandler { get; set; }

	/// <summary>Gets or sets a reference to a callback that is invoked by the XAML writer at the pre-member-write phase of the object lifetime for each created object.</summary>
	/// <returns>A callback that is invoked by the XAML writer at the pre-member-write phase of object lifetime.</returns>
	public EventHandler<XamlObjectEventArgs> BeforePropertiesHandler { get; set; }

	/// <summary>Gets or sets a reference to a callback that is invoked by the XAML writer at the post-member-write phase of the object lifetime for each created object.</summary>
	/// <returns>A callback that is invoked by the XAML writer at the post-member-write phase of object lifetime.</returns>
	public EventHandler<XamlObjectEventArgs> AfterPropertiesHandler { get; set; }

	/// <summary>Gets or sets a reference to a callback that is invoked by the XAML writer at the <see cref="M:System.ComponentModel.ISupportInitialize.EndInit" /> phase of the object lifetime for each created object.</summary>
	/// <returns>A callback that is invoked by the XAML writer at the <see cref="M:System.ComponentModel.ISupportInitialize.EndInit" /> phase of object lifetime.</returns>
	public EventHandler<XamlObjectEventArgs> AfterEndInitHandler { get; set; }

	/// <summary>Gets or sets the handler to use when the object writer calls into a CLR-implemented SetValue for dependency properties.</summary>
	/// <returns>A handler implementation that handles this case.</returns>
	public EventHandler<XamlSetValueEventArgs> XamlSetValueHandler { get; set; }

	/// <summary>Gets or sets a preexisting root object for <see cref="T:System.Xaml.XamlObjectWriter" /> operations.</summary>
	/// <returns>A preexisting root object for <see cref="T:System.Xaml.XamlObjectWriter" /> operations.</returns>
	public object RootObjectInstance { get; set; }

	/// <summary>Gets or sets a value that specifies whether the XAML writer should ignore (not call) <see cref="M:System.ComponentModel.TypeConverter.CanConvertFrom(System.ComponentModel.ITypeDescriptorContext,System.Type)" /> implementations on a <see cref="T:System.ComponentModel.TypeConverter" /> in type-converter situations.</summary>
	/// <returns>true if the XAML writer ignores <see cref="M:System.ComponentModel.TypeConverter.CanConvertFrom(System.ComponentModel.ITypeDescriptorContext,System.Type)" /> implementations; otherwise, false. The default is false.</returns>
	public bool IgnoreCanConvert { get; set; }

	/// <summary>Gets or sets the XAML namescope to use for registering names from the XAML writer if <see cref="P:System.Xaml.XamlObjectWriterSettings.RegisterNamesOnExternalNamescope" /> is true.</summary>
	/// <returns>The XAML namescope to use for registering names. The default is null.</returns>
	public INameScope ExternalNameScope { get; set; }

	/// <summary>Gets or sets a value that determines whether the XAML writer omits to check for the code path that checks for duplicate properties. </summary>
	/// <returns>true if the duplicate property check should be omitted; otherwise, false.</returns>
	public bool SkipDuplicatePropertyCheck { get; set; }

	/// <summary>Gets or sets a value that determines whether name registration should occur against the specified <see cref="P:System.Xaml.XamlObjectWriterSettings.ExternalNameScope" />.</summary>
	/// <returns>true if name registration should occur against the <see cref="P:System.Xaml.XamlObjectWriterSettings.ExternalNameScope" />; false if name registration should occur into the parent XAML namescope. The default is false.</returns>
	public bool RegisterNamesOnExternalNamescope { get; set; }

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Xaml.XamlObjectWriter" /> should omit to call ProvideValue on a markup extension, which is relevant when the markup extension represents the root of an object graph. </summary>
	/// <returns>true if the <see cref="T:System.Xaml.XamlObjectWriter" /> should omit to call ProvideValue on a markup extension; otherwise, false.</returns>
	public bool SkipProvideValueOnRoot { get; set; }

	/// <summary>Gets or sets a value that determines whether to disable a default <see cref="T:System.Xaml.XamlObjectWriter" /> feature that runs type conversion on the <paramref name="K" /> component of a <see cref="T:System.Collections.Generic.Dictionary`2" /> before writing the object graph representation.</summary>
	/// <returns>true if <paramref name="K" /> type conversion for a <see cref="T:System.Collections.Generic.Dictionary`2" /> object should be disabled. false if performing <paramref name="K" /> type conversion for a <see cref="T:System.Collections.Generic.Dictionary`2" /> object applies. The default is false.</returns>
	public bool PreferUnconvertedDictionaryKeys { get; set; }

	public Uri SourceBamlUri { get; set; }

	/// <summary>Gets or sets <see cref="T:System.Xaml.Permissions.XamlAccessLevel" /> permissions that the XAML writer observes.</summary>
	/// <returns>The <see cref="T:System.Xaml.Permissions.XamlAccessLevel" /> permissions.</returns>
	public XamlAccessLevel AccessLevel { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlObjectWriterSettings" /> class.</summary>
	public XamlObjectWriterSettings()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlObjectWriterSettings" /> class that is based on the copy of an existing instance.</summary>
	/// <param name="settings">The settings instance to copy.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="settings" /> is null.</exception>
	public XamlObjectWriterSettings(XamlObjectWriterSettings settings)
	{
		ArgumentNullException.ThrowIfNull(settings, "settings");
		AfterBeginInitHandler = settings.AfterBeginInitHandler;
		BeforePropertiesHandler = settings.BeforePropertiesHandler;
		AfterPropertiesHandler = settings.AfterPropertiesHandler;
		AfterEndInitHandler = settings.AfterEndInitHandler;
		XamlSetValueHandler = settings.XamlSetValueHandler;
		RootObjectInstance = settings.RootObjectInstance;
		IgnoreCanConvert = settings.IgnoreCanConvert;
		ExternalNameScope = settings.ExternalNameScope;
		SkipDuplicatePropertyCheck = settings.SkipDuplicatePropertyCheck;
		RegisterNamesOnExternalNamescope = settings.RegisterNamesOnExternalNamescope;
		AccessLevel = settings.AccessLevel;
		SkipProvideValueOnRoot = settings.SkipProvideValueOnRoot;
		PreferUnconvertedDictionaryKeys = settings.PreferUnconvertedDictionaryKeys;
		SourceBamlUri = settings.SourceBamlUri;
	}

	internal XamlObjectWriterSettings StripDelegates()
	{
		return new XamlObjectWriterSettings(this)
		{
			AfterBeginInitHandler = null,
			AfterEndInitHandler = null,
			AfterPropertiesHandler = null,
			BeforePropertiesHandler = null,
			XamlSetValueHandler = null
		};
	}
}
