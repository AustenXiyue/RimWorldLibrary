using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using MS.Internal;
using MS.Internal.Controls;
using MS.Internal.Data;

namespace System.Windows.Data;

/// <summary>Provides high-level access to the definition of a binding, which connects the properties of binding target objects (typically, WPF elements), and any data source (for example, a database, an XML file, or any object that contains data).</summary>
public class Binding : BindingBase
{
	private enum SourceProperties : byte
	{
		None,
		RelativeSource,
		ElementName,
		Source,
		StaticSource,
		InternalSource
	}

	/// <summary>Identifies the <see cref="E:System.Windows.Data.Binding.SourceUpdated" /> attached event.</summary>
	public static readonly RoutedEvent SourceUpdatedEvent = EventManager.RegisterRoutedEvent("SourceUpdated", RoutingStrategy.Bubble, typeof(EventHandler<DataTransferEventArgs>), typeof(Binding));

	/// <summary>Identifies the <see cref="E:System.Windows.Data.Binding.TargetUpdated" /> attached event.</summary>
	public static readonly RoutedEvent TargetUpdatedEvent = EventManager.RegisterRoutedEvent("TargetUpdated", RoutingStrategy.Bubble, typeof(EventHandler<DataTransferEventArgs>), typeof(Binding));

	/// <summary>Identifies the <see cref="P:System.Windows.Data.Binding.XmlNamespaceManager" /> attached property.</summary>
	public static readonly DependencyProperty XmlNamespaceManagerProperty = DependencyProperty.RegisterAttached("XmlNamespaceManager", typeof(object), typeof(Binding), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits), IsValidXmlNamespaceManager);

	/// <summary>Used as a returned value to instruct the binding engine not to perform any action.</summary>
	public static readonly object DoNothing = new NamedObject("Binding.DoNothing");

	/// <summary>Used as the <see cref="P:System.ComponentModel.PropertyChangedEventArgs.PropertyName" /> of <see cref="T:System.ComponentModel.PropertyChangedEventArgs" /> to indicate that an indexer property has changed. </summary>
	public const string IndexerName = "Item[]";

	private SourceProperties _sourceInUse;

	private PropertyPath _ppath;

	private ObjectRef _source = UnsetSource;

	private bool _isAsync;

	private bool _bindsDirectlyToSource;

	private bool _doesNotTransferDefaultValue;

	private int _attachedPropertiesInPath;

	private static readonly ObjectRef UnsetSource = new ExplicitObjectRef(null);

	private static readonly ObjectRef StaticSourceRef = new ExplicitObjectRef(BindingExpression.StaticSource);

	/// <summary>Gets a collection of rules that check the validity of the user input.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Controls.ValidationRule" /> objects.</returns>
	public Collection<ValidationRule> ValidationRules
	{
		get
		{
			if (!HasValue(Feature.ValidationRules))
			{
				SetValue(Feature.ValidationRules, new ValidationRuleCollection());
			}
			return (ValidationRuleCollection)GetValue(Feature.ValidationRules, null);
		}
	}

	/// <summary>Gets or sets a value that indicates whether to include the <see cref="T:System.Windows.Controls.ExceptionValidationRule" />.</summary>
	/// <returns>true to include the <see cref="T:System.Windows.Controls.ExceptionValidationRule" />; otherwise, false.</returns>
	[DefaultValue(false)]
	public bool ValidatesOnExceptions
	{
		get
		{
			return TestFlag(BindingFlags.ValidatesOnExceptions);
		}
		set
		{
			if (TestFlag(BindingFlags.ValidatesOnExceptions) != value)
			{
				CheckSealed();
				ChangeFlag(BindingFlags.ValidatesOnExceptions, value);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether to include the <see cref="T:System.Windows.Controls.DataErrorValidationRule" />.</summary>
	/// <returns>true to include the <see cref="T:System.Windows.Controls.DataErrorValidationRule" />; otherwise, false.</returns>
	[DefaultValue(false)]
	public bool ValidatesOnDataErrors
	{
		get
		{
			return TestFlag(BindingFlags.ValidatesOnDataErrors);
		}
		set
		{
			if (TestFlag(BindingFlags.ValidatesOnDataErrors) != value)
			{
				CheckSealed();
				ChangeFlag(BindingFlags.ValidatesOnDataErrors, value);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether to include the <see cref="T:System.Windows.Controls.NotifyDataErrorValidationRule" />.</summary>
	/// <returns>true to include the <see cref="T:System.Windows.Controls.NotifyDataErrorValidationRule" />; otherwise, false. The default is true.</returns>
	[DefaultValue(true)]
	public bool ValidatesOnNotifyDataErrors
	{
		get
		{
			return TestFlag(BindingFlags.ValidatesOnNotifyDataErrors);
		}
		set
		{
			if (TestFlag(BindingFlags.ValidatesOnNotifyDataErrors) != value)
			{
				CheckSealed();
				ChangeFlag(BindingFlags.ValidatesOnNotifyDataErrors, value);
			}
		}
	}

	/// <summary>Gets or sets the path to the binding source property.</summary>
	/// <returns>The path to the binding source. The default is null.</returns>
	public PropertyPath Path
	{
		get
		{
			return _ppath;
		}
		set
		{
			CheckSealed();
			_ppath = value;
			_attachedPropertiesInPath = -1;
			ClearFlag(BindingFlags.PathGeneratedInternally);
			if (_ppath != null && _ppath.StartsWithStaticProperty)
			{
				if (_sourceInUse != 0 && _sourceInUse != SourceProperties.StaticSource && !FrameworkCompatibilityPreferences.TargetsDesktop_V4_0)
				{
					throw new InvalidOperationException(SR.Format(SR.BindingConflict, SourceProperties.StaticSource, _sourceInUse));
				}
				SourceReference = StaticSourceRef;
			}
		}
	}

	/// <summary>Gets or sets an XPath query that returns the value on the XML binding source to use.</summary>
	/// <returns>The XPath query. The default is null.</returns>
	[DefaultValue(null)]
	public string XPath
	{
		get
		{
			return (string)GetValue(Feature.XPath, null);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.XPath, value, null);
		}
	}

	/// <summary>Gets or sets a value that indicates the direction of the data flow in the binding.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Data.BindingMode" /> values. The default is <see cref="F:System.Windows.Data.BindingMode.Default" />, which returns the default binding mode value of the target dependency property. However, the default value varies for each dependency property. In general, user-editable control properties, such as those of text boxes and check boxes, default to two-way bindings, whereas most other properties default to one-way bindings.A programmatic way to determine whether a dependency property binds one-way or two-way by default is to get the property metadata of the property using <see cref="M:System.Windows.DependencyProperty.GetMetadata(System.Type)" /> and then check the Boolean value of the <see cref="P:System.Windows.FrameworkPropertyMetadata.BindsTwoWayByDefault" /> property.</returns>
	[DefaultValue(BindingMode.Default)]
	public BindingMode Mode
	{
		get
		{
			switch (GetFlagsWithinMask(BindingFlags.PropagationMask))
			{
			case BindingFlags.OneWay:
				return BindingMode.OneWay;
			case BindingFlags.TwoWay:
				return BindingMode.TwoWay;
			case BindingFlags.OneWayToSource:
				return BindingMode.OneWayToSource;
			case BindingFlags.OneTime:
				return BindingMode.OneTime;
			case BindingFlags.PropDefault:
				return BindingMode.Default;
			default:
				Invariant.Assert(condition: false, "Unexpected BindingMode value");
				return BindingMode.TwoWay;
			}
		}
		set
		{
			CheckSealed();
			BindingFlags bindingFlags = BindingBase.FlagsFrom(value);
			if (bindingFlags == BindingFlags.IllegalInput)
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(BindingMode));
			}
			ChangeFlagsWithinMask(BindingFlags.PropagationMask, bindingFlags);
		}
	}

	/// <summary>Gets or sets a value that determines the timing of binding source updates.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Data.UpdateSourceTrigger" /> values. The default is <see cref="F:System.Windows.Data.UpdateSourceTrigger.Default" />, which returns the default <see cref="T:System.Windows.Data.UpdateSourceTrigger" /> value of the target dependency property. However, the default value for most dependency properties is <see cref="F:System.Windows.Data.UpdateSourceTrigger.PropertyChanged" />, while the <see cref="P:System.Windows.Controls.TextBox.Text" /> property has a default value of <see cref="F:System.Windows.Data.UpdateSourceTrigger.LostFocus" />.A programmatic way to determine the default <see cref="P:System.Windows.Data.Binding.UpdateSourceTrigger" /> value of a dependency property is to get the property metadata of the property using <see cref="M:System.Windows.DependencyProperty.GetMetadata(System.Type)" /> and then check the value of the <see cref="P:System.Windows.FrameworkPropertyMetadata.DefaultUpdateSourceTrigger" /> property.</returns>
	[DefaultValue(UpdateSourceTrigger.Default)]
	public UpdateSourceTrigger UpdateSourceTrigger
	{
		get
		{
			switch (GetFlagsWithinMask(BindingFlags.UpdateDefault))
			{
			case BindingFlags.OneTime:
				return UpdateSourceTrigger.PropertyChanged;
			case BindingFlags.UpdateOnLostFocus:
				return UpdateSourceTrigger.LostFocus;
			case BindingFlags.UpdateExplicitly:
				return UpdateSourceTrigger.Explicit;
			case BindingFlags.UpdateDefault:
				return UpdateSourceTrigger.Default;
			default:
				Invariant.Assert(condition: false, "Unexpected UpdateSourceTrigger value");
				return UpdateSourceTrigger.Default;
			}
		}
		set
		{
			CheckSealed();
			BindingFlags bindingFlags = BindingBase.FlagsFrom(value);
			if (bindingFlags == BindingFlags.IllegalInput)
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(UpdateSourceTrigger));
			}
			ChangeFlagsWithinMask(BindingFlags.UpdateDefault, bindingFlags);
		}
	}

	/// <summary>Gets or sets a value that indicates whether to raise the <see cref="E:System.Windows.Data.Binding.SourceUpdated" /> event when a value is transferred from the binding target to the binding source.</summary>
	/// <returns>true if the <see cref="E:System.Windows.Data.Binding.SourceUpdated" /> event should be raised when the binding source value is updated; otherwise, false. The default is false.</returns>
	[DefaultValue(false)]
	public bool NotifyOnSourceUpdated
	{
		get
		{
			return TestFlag(BindingFlags.NotifyOnSourceUpdated);
		}
		set
		{
			if (TestFlag(BindingFlags.NotifyOnSourceUpdated) != value)
			{
				CheckSealed();
				ChangeFlag(BindingFlags.NotifyOnSourceUpdated, value);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether to raise the <see cref="E:System.Windows.Data.Binding.TargetUpdated" /> event when a value is transferred from the binding source to the binding target.</summary>
	/// <returns>true if the <see cref="E:System.Windows.Data.Binding.TargetUpdated" /> event should be raised when the binding target value is updated; otherwise, false. The default is false.</returns>
	[DefaultValue(false)]
	public bool NotifyOnTargetUpdated
	{
		get
		{
			return TestFlag(BindingFlags.NotifyOnTargetUpdated);
		}
		set
		{
			if (TestFlag(BindingFlags.NotifyOnTargetUpdated) != value)
			{
				CheckSealed();
				ChangeFlag(BindingFlags.NotifyOnTargetUpdated, value);
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether to raise the <see cref="E:System.Windows.Controls.Validation.Error" /> attached event on the bound object.</summary>
	/// <returns>true if the <see cref="E:System.Windows.Controls.Validation.Error" /> attached event should be raised on the bound object when there is a validation error during source updates; otherwise, false. The default is false.</returns>
	[DefaultValue(false)]
	public bool NotifyOnValidationError
	{
		get
		{
			return TestFlag(BindingFlags.NotifyOnValidationError);
		}
		set
		{
			if (TestFlag(BindingFlags.NotifyOnValidationError) != value)
			{
				CheckSealed();
				ChangeFlag(BindingFlags.NotifyOnValidationError, value);
			}
		}
	}

	/// <summary>Gets or sets the converter to use.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Data.IValueConverter" />. The default is null.</returns>
	[DefaultValue(null)]
	public IValueConverter Converter
	{
		get
		{
			return (IValueConverter)GetValue(Feature.Converter, null);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.Converter, value, null);
		}
	}

	/// <summary>Gets or sets the parameter to pass to the <see cref="P:System.Windows.Data.Binding.Converter" />.</summary>
	/// <returns>The parameter to pass to the <see cref="P:System.Windows.Data.Binding.Converter" />. The default is null.</returns>
	[DefaultValue(null)]
	public object ConverterParameter
	{
		get
		{
			return GetValue(Feature.ConverterParameter, null);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.ConverterParameter, value, null);
		}
	}

	/// <summary>Gets or sets the culture in which to evaluate the converter.</summary>
	/// <returns>The default is null.</returns>
	[DefaultValue(null)]
	[TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
	public CultureInfo ConverterCulture
	{
		get
		{
			return (CultureInfo)GetValue(Feature.Culture, null);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.Culture, value, null);
		}
	}

	/// <summary>Gets or sets the object to use as the binding source.</summary>
	/// <returns>The object to use as the binding source.</returns>
	public object Source
	{
		get
		{
			WeakReference<object> weakReference = (WeakReference<object>)GetValue(Feature.ObjectSource, null);
			if (weakReference == null)
			{
				return null;
			}
			if (!weakReference.TryGetTarget(out var target))
			{
				return null;
			}
			return target;
		}
		set
		{
			CheckSealed();
			if (_sourceInUse == SourceProperties.None || _sourceInUse == SourceProperties.Source)
			{
				if (value != DependencyProperty.UnsetValue)
				{
					SetValue(Feature.ObjectSource, new WeakReference<object>(value));
					SourceReference = new ExplicitObjectRef(value);
				}
				else
				{
					ClearValue(Feature.ObjectSource);
					SourceReference = null;
				}
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.BindingConflict, SourceProperties.Source, _sourceInUse));
		}
	}

	/// <summary>Gets or sets the binding source by specifying its location relative to the position of the binding target.</summary>
	/// <returns>A <see cref="T:System.Windows.Data.RelativeSource" /> object specifying the relative location of the binding source to use. The default is null.</returns>
	[DefaultValue(null)]
	public RelativeSource RelativeSource
	{
		get
		{
			return (RelativeSource)GetValue(Feature.RelativeSource, null);
		}
		set
		{
			CheckSealed();
			if (_sourceInUse == SourceProperties.None || _sourceInUse == SourceProperties.RelativeSource)
			{
				SetValue(Feature.RelativeSource, value, null);
				SourceReference = ((value != null) ? new RelativeObjectRef(value) : null);
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.BindingConflict, SourceProperties.RelativeSource, _sourceInUse));
		}
	}

	/// <summary>Gets or sets the name of the element to use as the binding source object.</summary>
	/// <returns>The value of the Name property or x:Name Directive of the element of interest. You can refer to elements in code only if they are registered to the appropriate <see cref="T:System.Windows.NameScope" /> through RegisterName. For more information, see WPF XAML Namescopes.The default is null.</returns>
	[DefaultValue(null)]
	public string ElementName
	{
		get
		{
			return (string)GetValue(Feature.ElementSource, null);
		}
		set
		{
			CheckSealed();
			if (_sourceInUse == SourceProperties.None || _sourceInUse == SourceProperties.ElementName)
			{
				SetValue(Feature.ElementSource, value, null);
				SourceReference = ((value != null) ? new ElementObjectRef(value) : null);
				return;
			}
			throw new InvalidOperationException(SR.Format(SR.BindingConflict, SourceProperties.ElementName, _sourceInUse));
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Data.Binding" /> should get and set values asynchronously.</summary>
	/// <returns>The default is null.</returns>
	[DefaultValue(false)]
	public bool IsAsync
	{
		get
		{
			return _isAsync;
		}
		set
		{
			CheckSealed();
			_isAsync = value;
		}
	}

	/// <summary>Gets or sets opaque data passed to the asynchronous data dispatcher.</summary>
	/// <returns>Data passed to the asynchronous data dispatcher.</returns>
	[DefaultValue(null)]
	public object AsyncState
	{
		get
		{
			return GetValue(Feature.AsyncState, null);
		}
		set
		{
			CheckSealed();
			SetValue(Feature.AsyncState, value, null);
		}
	}

	/// <summary>Gets or sets a value that indicates whether to evaluate the <see cref="P:System.Windows.Data.Binding.Path" /> relative to the data item or the <see cref="T:System.Windows.Data.DataSourceProvider" /> object.</summary>
	/// <returns>false to evaluate the path relative to the data item itself; otherwise, true. The default is false.</returns>
	[DefaultValue(false)]
	public bool BindsDirectlyToSource
	{
		get
		{
			return _bindsDirectlyToSource;
		}
		set
		{
			CheckSealed();
			_bindsDirectlyToSource = value;
		}
	}

	/// <summary>Gets or sets a handler you can use to provide custom logic for handling exceptions that the binding engine encounters during the update of the binding source value. This is only applicable if you have associated an <see cref="T:System.Windows.Controls.ExceptionValidationRule" /> with your binding.</summary>
	/// <returns>A method that provides custom logic for handling exceptions that the binding engine encounters during the update of the binding source value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public UpdateSourceExceptionFilterCallback UpdateSourceExceptionFilter
	{
		get
		{
			return (UpdateSourceExceptionFilterCallback)GetValue(Feature.ExceptionFilterCallback, null);
		}
		set
		{
			SetValue(Feature.ExceptionFilterCallback, value, null);
		}
	}

	internal override CultureInfo ConverterCultureInternal => ConverterCulture;

	internal ObjectRef SourceReference
	{
		get
		{
			if (_source != UnsetSource)
			{
				return _source;
			}
			return null;
		}
		set
		{
			CheckSealed();
			_source = value;
			DetermineSource();
		}
	}

	internal bool TreeContextIsRequired
	{
		get
		{
			if (_attachedPropertiesInPath < 0)
			{
				if (_ppath != null)
				{
					_attachedPropertiesInPath = _ppath.ComputeUnresolvedAttachedPropertiesInPath();
				}
				else
				{
					_attachedPropertiesInPath = 0;
				}
			}
			bool flag = _attachedPropertiesInPath > 0;
			if (!flag && HasValue(Feature.XPath) && XPath.Contains(':'))
			{
				flag = true;
			}
			return flag;
		}
	}

	internal override Collection<ValidationRule> ValidationRulesInternal => (ValidationRuleCollection)GetValue(Feature.ValidationRules, null);

	internal bool TransfersDefaultValue
	{
		get
		{
			return !_doesNotTransferDefaultValue;
		}
		set
		{
			CheckSealed();
			_doesNotTransferDefaultValue = !value;
		}
	}

	internal override bool ValidatesOnNotifyDataErrorsInternal => ValidatesOnNotifyDataErrors;

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Data.Binding.SourceUpdated" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to the event.</param>
	/// <param name="handler">The handler to add.</param>
	public static void AddSourceUpdatedHandler(DependencyObject element, EventHandler<DataTransferEventArgs> handler)
	{
		UIElement.AddHandler(element, SourceUpdatedEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Data.Binding.SourceUpdated" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to the event.</param>
	/// <param name="handler">The handler to remove.</param>
	public static void RemoveSourceUpdatedHandler(DependencyObject element, EventHandler<DataTransferEventArgs> handler)
	{
		UIElement.RemoveHandler(element, SourceUpdatedEvent, handler);
	}

	/// <summary>Adds a handler for the <see cref="E:System.Windows.Data.Binding.TargetUpdated" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to the event.</param>
	/// <param name="handler">The handler to add.</param>
	public static void AddTargetUpdatedHandler(DependencyObject element, EventHandler<DataTransferEventArgs> handler)
	{
		UIElement.AddHandler(element, TargetUpdatedEvent, handler);
	}

	/// <summary>Removes a handler for the <see cref="E:System.Windows.Data.Binding.TargetUpdated" /> attached event. </summary>
	/// <param name="element">The <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" /> that listens to the event.</param>
	/// <param name="handler">The handler to remove.</param>
	public static void RemoveTargetUpdatedHandler(DependencyObject element, EventHandler<DataTransferEventArgs> handler)
	{
		UIElement.RemoveHandler(element, TargetUpdatedEvent, handler);
	}

	/// <summary>Returns an XML namespace manager object used by the binding attached to the specified object. </summary>
	/// <returns>A returned object used for viewing XML namespaces that relate to the binding on the passed object element. This object should be cast as <see cref="T:System.Xml.XmlNamespaceManager" />.</returns>
	/// <param name="target">The object from which to get namespace information.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="target" /> parameter cannot be null.</exception>
	public static XmlNamespaceManager GetXmlNamespaceManager(DependencyObject target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		return (XmlNamespaceManager)target.GetValue(XmlNamespaceManagerProperty);
	}

	/// <summary>Sets a namespace manager object used by the binding attached to the provided element. </summary>
	/// <param name="target">The object from which to get namespace information.</param>
	/// <param name="value">The <see cref="T:System.Xml.XmlNamespaceManager" /> to use for namespace evaluation in the passed element.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="target" /> is null. </exception>
	public static void SetXmlNamespaceManager(DependencyObject target, XmlNamespaceManager value)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		target.SetValue(XmlNamespaceManagerProperty, value);
	}

	private static bool IsValidXmlNamespaceManager(object value)
	{
		if (value != null)
		{
			return SystemXmlHelper.IsXmlNamespaceManager(value);
		}
		return true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.Binding" /> class.</summary>
	public Binding()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.Binding" /> class with an initial path.</summary>
	/// <param name="path">The initial <see cref="P:System.Windows.Data.Binding.Path" /> for the binding.</param>
	public Binding(string path)
	{
		if (path != null)
		{
			if (Dispatcher.CurrentDispatcher == null)
			{
				throw new InvalidOperationException();
			}
			Path = new PropertyPath(path, (object[])null);
		}
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.Binding.ValidationRules" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeValidationRules()
	{
		if (HasValue(Feature.ValidationRules))
		{
			return ValidationRules.Count > 0;
		}
		return false;
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.Binding.Path" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializePath()
	{
		if (_ppath != null)
		{
			return !TestFlag(BindingFlags.PathGeneratedInternally);
		}
		return false;
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.Binding.Source" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeSource()
	{
		return false;
	}

	internal override BindingExpressionBase CreateBindingExpressionOverride(DependencyObject target, DependencyProperty dp, BindingExpressionBase owner)
	{
		return BindingExpression.CreateBindingExpression(target, dp, this, owner);
	}

	internal override ValidationRule LookupValidationRule(Type type)
	{
		return BindingBase.LookupValidationRule(type, ValidationRulesInternal);
	}

	internal object DoFilterException(object bindExpr, Exception exception)
	{
		UpdateSourceExceptionFilterCallback updateSourceExceptionFilterCallback = (UpdateSourceExceptionFilterCallback)GetValue(Feature.ExceptionFilterCallback, null);
		if (updateSourceExceptionFilterCallback != null)
		{
			return updateSourceExceptionFilterCallback(bindExpr, exception);
		}
		return exception;
	}

	internal void UsePath(PropertyPath path)
	{
		_ppath = path;
		SetFlag(BindingFlags.PathGeneratedInternally);
	}

	internal override BindingBase CreateClone()
	{
		return new Binding();
	}

	internal override void InitializeClone(BindingBase baseClone, BindingMode mode)
	{
		Binding binding = (Binding)baseClone;
		binding._ppath = _ppath;
		CopyValue(Feature.XPath, binding);
		binding._source = _source;
		CopyValue(Feature.Culture, binding);
		binding._isAsync = _isAsync;
		CopyValue(Feature.AsyncState, binding);
		binding._bindsDirectlyToSource = _bindsDirectlyToSource;
		binding._doesNotTransferDefaultValue = _doesNotTransferDefaultValue;
		CopyValue(Feature.ObjectSource, binding);
		CopyValue(Feature.RelativeSource, binding);
		CopyValue(Feature.Converter, binding);
		CopyValue(Feature.ConverterParameter, binding);
		binding._attachedPropertiesInPath = _attachedPropertiesInPath;
		CopyValue(Feature.ValidationRules, binding);
		base.InitializeClone(baseClone, mode);
	}

	private void DetermineSource()
	{
		_sourceInUse = ((_source != UnsetSource) ? (HasValue(Feature.RelativeSource) ? SourceProperties.RelativeSource : (HasValue(Feature.ElementSource) ? SourceProperties.ElementName : (HasValue(Feature.ObjectSource) ? SourceProperties.Source : ((_source == StaticSourceRef) ? SourceProperties.StaticSource : SourceProperties.InternalSource)))) : SourceProperties.None);
	}
}
