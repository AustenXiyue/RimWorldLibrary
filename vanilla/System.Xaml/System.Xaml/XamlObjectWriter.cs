using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Markup;
using System.Xaml.MS.Impl;
using System.Xaml.Schema;
using MS.Internal.Xaml.Context;
using MS.Internal.Xaml.Parser;
using MS.Internal.Xaml.Runtime;

namespace System.Xaml;

/// <summary>Creates an object graph from a source XAML node stream.</summary>
public class XamlObjectWriter : XamlWriter, IXamlLineInfoConsumer, IAddLineInfo, ICheckIfInitialized
{
	private class PendingCollectionAdd : IAddLineInfo
	{
		public object Key { get; set; }

		public bool KeyIsSet { get; set; }

		public bool KeyIsUnconverted { get; set; }

		public object Item { get; set; }

		public XamlType ItemType { get; set; }

		public int LineNumber { get; set; }

		public int LinePosition { get; set; }

		XamlException IAddLineInfo.WithLineInfo(XamlException ex)
		{
			if (LineNumber > 0)
			{
				ex.SetLineInfo(LineNumber, LinePosition);
			}
			return ex;
		}
	}

	private object _lastInstance;

	private bool _inDispose;

	private ObjectWriterContext _context;

	private DeferringWriter _deferringWriter;

	private EventHandler<XamlObjectEventArgs> _afterBeginInitHandler;

	private EventHandler<XamlObjectEventArgs> _beforePropertiesHandler;

	private EventHandler<XamlObjectEventArgs> _afterPropertiesHandler;

	private EventHandler<XamlObjectEventArgs> _afterEndInitHandler;

	private EventHandler<XamlSetValueEventArgs> _xamlSetValueHandler;

	private object _rootObjectInstance;

	private bool _skipDuplicatePropertyCheck;

	private NameFixupGraph _nameFixupGraph;

	private Dictionary<object, List<PendingCollectionAdd>> _pendingCollectionAdds;

	private INameScope _rootNamescope;

	private bool _skipProvideValueOnRoot;

	private bool _nextNodeMustBeEndMember;

	private bool _preferUnconvertedDictionaryKeys;

	private Dictionary<object, ObjectWriterContext> _pendingKeyConversionContexts;

	private NameFixupGraph NameFixupGraph
	{
		get
		{
			if (_nameFixupGraph == null)
			{
				_nameFixupGraph = new NameFixupGraph();
			}
			return _nameFixupGraph;
		}
	}

	private Dictionary<object, List<PendingCollectionAdd>> PendingCollectionAdds
	{
		get
		{
			if (_pendingCollectionAdds == null)
			{
				_pendingCollectionAdds = new Dictionary<object, List<PendingCollectionAdd>>();
			}
			return _pendingCollectionAdds;
		}
	}

	private Dictionary<object, ObjectWriterContext> PendingKeyConversionContexts
	{
		get
		{
			if (_pendingKeyConversionContexts == null)
			{
				_pendingKeyConversionContexts = new Dictionary<object, ObjectWriterContext>();
			}
			return _pendingKeyConversionContexts;
		}
	}

	private XamlRuntime Runtime => _context.Runtime;

	/// <summary>Gets the <see cref="T:System.Windows.Markup.INameScope" /> service that maintains the XAML namescope for the absolute node root of the XAML object writer.</summary>
	/// <returns>The <see cref="T:System.Windows.Markup.INameScope" /> service that maintains the XAML namescope for the current root of the object writer.</returns>
	public INameScope RootNameScope
	{
		get
		{
			if (_rootNamescope != null)
			{
				return _rootNamescope;
			}
			return _context.RootNameScope;
		}
	}

	/// <summary>Gets the last object that was written. Typically only called when the node stream is at its end and the object graph is complete.</summary>
	/// <returns>The last object written, or null. See Remarks.</returns>
	public virtual object Result => _lastInstance;

	/// <summary>Gets the active XAML schema context.</summary>
	/// <returns>The XAML schema context.</returns>
	public override XamlSchemaContext SchemaContext
	{
		get
		{
			ThrowIfDisposed();
			return _context.SchemaContext;
		}
	}

	/// <summary>Gets a value that reports whether a line information service should provide values and therefore, should also call the <see cref="M:System.Xaml.IXamlLineInfoConsumer.SetLineInfo(System.Int32,System.Int32)" /> method when it is relevant.</summary>
	/// <returns>true if line information is used by the writer; otherwise, false.</returns>
	public bool ShouldProvideLineInfo
	{
		get
		{
			ThrowIfDisposed();
			return true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlObjectWriter" /> class using  the XAML schema context that is used by a <see cref="T:System.Xaml.XamlReader" />.</summary>
	/// <param name="schemaContext">A XAML schema context that is shared with the XAML reader that provides nodes for writing.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="schemaContext" /> is null.</exception>
	public XamlObjectWriter(XamlSchemaContext schemaContext)
	{
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		Initialize(schemaContext, null, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xaml.XamlObjectWriter" /> class using the context that is used by a <see cref="T:System.Xaml.XamlReader" /> and its settings.</summary>
	/// <param name="schemaContext">A XAML schema context that is shared with the XAML reader that provides nodes for writing.</param>
	/// <param name="settings">A settings object that specifies certain options for <see cref="T:System.Xaml.XamlObjectWriter" /> behavior and output.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="schemaContext" /> is null.</exception>
	public XamlObjectWriter(XamlSchemaContext schemaContext, XamlObjectWriterSettings settings)
	{
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		Initialize(schemaContext, null, settings);
	}

	internal XamlObjectWriter(XamlSavedContext savedContext, XamlObjectWriterSettings settings)
	{
		ArgumentNullException.ThrowIfNull(savedContext, "savedContext");
		if (savedContext.SchemaContext == null)
		{
			throw new ArgumentException(System.SR.SavedContextSchemaContextNull, "savedContext");
		}
		Initialize(savedContext.SchemaContext, savedContext, settings);
	}

	private void Initialize(XamlSchemaContext schemaContext, XamlSavedContext savedContext, XamlObjectWriterSettings settings)
	{
		_inDispose = false;
		ArgumentNullException.ThrowIfNull(schemaContext, "schemaContext");
		if (savedContext != null && schemaContext != savedContext.SchemaContext)
		{
			throw new ArgumentException(System.SR.SavedContextSchemaContextMismatch, "schemaContext");
		}
		if (settings != null)
		{
			_afterBeginInitHandler = settings.AfterBeginInitHandler;
			_beforePropertiesHandler = settings.BeforePropertiesHandler;
			_afterPropertiesHandler = settings.AfterPropertiesHandler;
			_afterEndInitHandler = settings.AfterEndInitHandler;
			_xamlSetValueHandler = settings.XamlSetValueHandler;
			_rootObjectInstance = settings.RootObjectInstance;
			_skipDuplicatePropertyCheck = settings.SkipDuplicatePropertyCheck;
			_skipProvideValueOnRoot = settings.SkipProvideValueOnRoot;
			_preferUnconvertedDictionaryKeys = settings.PreferUnconvertedDictionaryKeys;
		}
		INameScope rootNameScope = settings?.ExternalNameScope;
		XamlRuntime runtime = CreateRuntime(settings, schemaContext);
		if (savedContext != null)
		{
			_context = new ObjectWriterContext(savedContext, settings, rootNameScope, runtime);
		}
		else
		{
			if (schemaContext == null)
			{
				throw _context.WithLineInfo(new XamlInternalException());
			}
			_context = new ObjectWriterContext(schemaContext, settings, rootNameScope, runtime);
			_context.AddNamespacePrefix("xml", "http://www.w3.org/XML/1998/namespace");
		}
		_context.IsInitializedCallback = this;
		_deferringWriter = new DeferringWriter(_context);
		_rootNamescope = null;
	}

	private XamlRuntime CreateRuntime(XamlObjectWriterSettings settings, XamlSchemaContext schemaContext)
	{
		XamlRuntime xamlRuntime = null;
		XamlRuntimeSettings xamlRuntimeSettings = null;
		if (settings != null)
		{
			xamlRuntimeSettings = new XamlRuntimeSettings
			{
				IgnoreCanConvert = settings.IgnoreCanConvert
			};
			if (settings.AccessLevel != null)
			{
				xamlRuntime = new PartialTrustTolerantRuntime(xamlRuntimeSettings, settings.AccessLevel, schemaContext);
			}
		}
		if (xamlRuntime == null)
		{
			xamlRuntime = new ClrObjectRuntime(xamlRuntimeSettings, isWriter: true);
		}
		xamlRuntime.LineInfo = this;
		return xamlRuntime;
	}

	/// <summary>Invokes any <see cref="P:System.Xaml.XamlObjectWriterSettings.AfterBeginInitHandler" /> method that is associated with <see cref="T:System.Xaml.XamlObjectWriterSettings" /> for this <see cref="T:System.Xaml.XamlObjectWriter" />.</summary>
	/// <param name="value">The object to report on. The object is packaged into <see cref="T:System.Xaml.XamlObjectEventArgs" /> when the handler is invoked.</param>
	protected virtual void OnAfterBeginInit(object value)
	{
		if (_afterBeginInitHandler != null)
		{
			_afterBeginInitHandler(this, new XamlObjectEventArgs(value, _context.BaseUri ?? _context.SourceBamlUri, _context.LineNumber_StartObject, _context.LinePosition_StartObject));
		}
	}

	/// <summary>Invokes any <see cref="P:System.Xaml.XamlObjectWriterSettings.BeforePropertiesHandler" /> referenced method that is associated with <see cref="T:System.Xaml.XamlObjectWriterSettings" /> for this <see cref="T:System.Xaml.XamlObjectWriter" />.</summary>
	/// <param name="value">The value to pass through <see cref="T:System.Xaml.XamlObjectEventArgs" /> when the handler is invoked.</param>
	protected virtual void OnBeforeProperties(object value)
	{
		if (_beforePropertiesHandler != null)
		{
			_beforePropertiesHandler(this, new XamlObjectEventArgs(value));
		}
	}

	/// <summary>Invokes any <see cref="P:System.Xaml.XamlObjectWriterSettings.AfterPropertiesHandler" /> referenced method that is associated with <see cref="T:System.Xaml.XamlObjectWriterSettings" /> for this <see cref="T:System.Xaml.XamlObjectWriter" />.</summary>
	/// <param name="value">The object to report on. This is packaged into <see cref="T:System.Xaml.XamlObjectEventArgs" /> when the handler is invoked.</param>
	protected virtual void OnAfterProperties(object value)
	{
		if (_afterPropertiesHandler != null)
		{
			_afterPropertiesHandler(this, new XamlObjectEventArgs(value));
		}
	}

	/// <summary>Invokes any <see cref="P:System.Xaml.XamlObjectWriterSettings.AfterEndInitHandler" /> method that is associated with <see cref="T:System.Xaml.XamlObjectWriterSettings" /> for this <see cref="T:System.Xaml.XamlObjectWriter" />.</summary>
	/// <param name="value">The object to report on. The value  is packaged into <see cref="T:System.Xaml.XamlObjectEventArgs" /> when the handler is invoked.</param>
	protected virtual void OnAfterEndInit(object value)
	{
		if (_afterEndInitHandler != null)
		{
			_afterEndInitHandler(this, new XamlObjectEventArgs(value));
		}
	}

	/// <summary>Invokes any <see cref="P:System.Xaml.XamlObjectWriterSettings.XamlSetValueHandler" /> referenced method that is associated with <see cref="T:System.Xaml.XamlObjectWriterSettings" /> for this writer, as long as <paramref name="handled" /> in <see cref="T:System.Windows.Markup.XamlSetValueEventArgs" /> event data is not true.</summary>
	/// <returns>A Boolean value that is set by any associated handler; otherwise, false. See Remarks.</returns>
	/// <param name="eventSender">The object to report as the source to the handler.</param>
	/// <param name="member">The name of the property to set, which is passed to the handler as part of <see cref="T:System.Windows.Markup.XamlSetValueEventArgs" />.</param>
	/// <param name="value">The value to provide for the property that is named by <paramref name="property" />. The value is passed to the handler as part of <see cref="T:System.Windows.Markup.XamlSetValueEventArgs" />.</param>
	protected virtual bool OnSetValue(object eventSender, XamlMember member, object value)
	{
		if (_xamlSetValueHandler != null)
		{
			XamlSetValueEventArgs xamlSetValueEventArgs = new XamlSetValueEventArgs(member, value);
			_xamlSetValueHandler(eventSender, xamlSetValueEventArgs);
			return xamlSetValueEventArgs.Handled;
		}
		return false;
	}

	private bool HasUnresolvedChildren(object parent)
	{
		if (_nameFixupGraph == null)
		{
			return false;
		}
		return _nameFixupGraph.HasUnresolvedChildren(parent);
	}

	private void TryCreateParentInstance(ObjectWriterContext ctx)
	{
		if (ctx.ParentInstance == null && ctx.ParentProperty != XamlLanguage.Arguments)
		{
			ctx.LiftScope();
			Logic_CreateAndAssignToParentStart(ctx);
			ctx.UnLiftScope();
		}
	}

	/// <summary>Writes the conceptual StartObject into the object graph when the specified object is a default or implicit value of the parent property. The implicit value comes from information that is obtained from the XAML schema context and backing type information, instead of being specified as an object value in the input XAML node set.</summary>
	public override void WriteGetObject()
	{
		ThrowIfDisposed();
		_deferringWriter.WriteGetObject();
		if (!_deferringWriter.Handled)
		{
			if (_nextNodeMustBeEndMember)
			{
				string valueMustBeFollowedByEndMember = System.SR.ValueMustBeFollowedByEndMember;
				throw _context.WithLineInfo(new XamlObjectWriterException(valueMustBeFollowedByEndMember));
			}
			XamlMember xamlMember = ((_context.CurrentType == null && _context.Depth > 1) ? _context.ParentProperty : _context.CurrentProperty);
			if (xamlMember == null)
			{
				XamlType xamlType = ((_context.CurrentType == null && _context.Depth > 1) ? _context.ParentType : _context.CurrentType);
				string message = ((xamlType != null) ? System.SR.Format(System.SR.NoPropertyInCurrentFrame_GO, xamlType.ToString()) : System.SR.NoPropertyInCurrentFrame_GO_noType);
				throw _context.WithLineInfo(new XamlObjectWriterException(message));
			}
			_lastInstance = null;
			if (_context.CurrentType != null)
			{
				_context.PushScope();
			}
			TryCreateParentInstance(_context);
			_context.CurrentIsObjectFromMember = true;
			object parentInstance = _context.ParentInstance;
			_context.CurrentType = xamlMember.Type;
			object value = Runtime.GetValue(parentInstance, xamlMember);
			_context.CurrentInstance = value ?? throw _context.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.GetObjectNull, parentInstance.GetType(), xamlMember.Name)));
			if (xamlMember.Type.IsCollection || xamlMember.Type.IsDictionary)
			{
				_context.CurrentCollection = value;
			}
		}
	}

	/// <summary>Writes an object node into the current scope, and sets the scope to a new object scope.</summary>
	/// <param name="xamlType">The type to write. Typically you obtain this value from a XAML reader.</param>
	/// <exception cref="T:System.Xaml.XamlObjectWriterException">Processing an <see cref="P:System.Xaml.XamlType.IsUnknown" /> type and cannot create an object.-or-Attempted to write a start object in an invalid scope.</exception>
	/// <exception cref="T:System.Xaml.XamlParseException">Attempted to write a root object when the root object was already provided in the settings. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="xamlType" /> is null.</exception>
	public override void WriteStartObject(XamlType xamlType)
	{
		ThrowIfDisposed();
		ArgumentNullException.ThrowIfNull(xamlType, "xamlType");
		_deferringWriter.WriteStartObject(xamlType);
		if (_deferringWriter.Handled)
		{
			return;
		}
		_context.LineNumber_StartObject = _context.LineNumber;
		_context.LinePosition_StartObject = _context.LinePosition;
		if (_nextNodeMustBeEndMember)
		{
			string valueMustBeFollowedByEndMember = System.SR.ValueMustBeFollowedByEndMember;
			throw _context.WithLineInfo(new XamlObjectWriterException(valueMustBeFollowedByEndMember));
		}
		if (xamlType.IsUnknown)
		{
			string message = System.SR.Format(System.SR.CantCreateUnknownType, xamlType.GetQualifiedName());
			throw _context.WithLineInfo(new XamlObjectWriterException(message));
		}
		if (_context.CurrentType != null && _context.CurrentProperty == null)
		{
			string message2 = System.SR.Format(System.SR.NoPropertyInCurrentFrame_SO, xamlType.ToString(), _context.CurrentType.ToString());
			throw _context.WithLineInfo(new XamlObjectWriterException(message2));
		}
		_lastInstance = null;
		if (_context.CurrentType != null)
		{
			_context.PushScope();
		}
		_context.CurrentType = xamlType;
		if (_context.LiveDepth == 1 && _rootObjectInstance != null)
		{
			XamlType xamlType2 = GetXamlType(_rootObjectInstance.GetType());
			if (!xamlType2.CanAssignTo(_context.CurrentType))
			{
				throw new XamlParseException(System.SR.Format(System.SR.CantAssignRootInstance, xamlType2.GetQualifiedName(), xamlType.GetQualifiedName()));
			}
			_context.CurrentInstance = _rootObjectInstance;
			if (_context.CurrentType.IsCollection || _context.CurrentType.IsDictionary)
			{
				_context.CurrentCollection = _rootObjectInstance;
			}
			Logic_BeginInit(_context);
		}
	}

	/// <summary>Closes the current object scope in the writer. The new object scope becomes the parent member scope.</summary>
	public override void WriteEndObject()
	{
		ThrowIfDisposed();
		_deferringWriter.WriteEndObject();
		if (_deferringWriter.Handled)
		{
			if (_deferringWriter.Mode == DeferringMode.TemplateReady)
			{
				XamlNodeList xamlNodeList = _deferringWriter.CollectTemplateList();
				_context.PushScope();
				_context.CurrentInstance = xamlNodeList.GetReader();
			}
			return;
		}
		if (_nextNodeMustBeEndMember)
		{
			string valueMustBeFollowedByEndMember = System.SR.ValueMustBeFollowedByEndMember;
			throw _context.WithLineInfo(new XamlObjectWriterException(valueMustBeFollowedByEndMember));
		}
		if (_context.CurrentType == null)
		{
			string noTypeInCurrentFrame_EO = System.SR.NoTypeInCurrentFrame_EO;
			throw _context.WithLineInfo(new XamlObjectWriterException(noTypeInCurrentFrame_EO));
		}
		if (_context.CurrentProperty != null)
		{
			string message = System.SR.Format(System.SR.OpenPropertyInCurrentFrame_EO, _context.CurrentType.ToString(), _context.CurrentProperty.ToString());
			throw _context.WithLineInfo(new XamlObjectWriterException(message));
		}
		bool flag = HasUnresolvedChildren(_context.CurrentInstance);
		bool flag2 = _context.CurrentInstance is NameFixupToken;
		if (!_context.CurrentIsObjectFromMember)
		{
			if (_context.CurrentInstance == null)
			{
				Logic_CreateAndAssignToParentStart(_context);
			}
			_ = _context.CurrentType;
			object currentInstance = _context.CurrentInstance;
			OnAfterProperties(currentInstance);
			if (_context.CurrentType.IsMarkupExtension)
			{
				if (flag)
				{
					Logic_DeferProvideValue(_context);
				}
				else
				{
					ExecutePendingAdds(_context.CurrentType, _context.CurrentInstance);
					Logic_EndInit(_context);
					currentInstance = _context.CurrentInstance;
					Logic_AssignProvidedValue(_context);
					if (_context.CurrentInstanceRegisteredName != null)
					{
						if (_nameFixupGraph != null)
						{
							TriggerNameResolution(currentInstance, _context.CurrentInstanceRegisteredName);
						}
						_context.CurrentInstanceRegisteredName = null;
					}
					currentInstance = _context.CurrentInstance;
					flag2 = currentInstance is NameFixupToken;
					flag = !flag2 && HasUnresolvedChildren(currentInstance);
				}
			}
			else
			{
				if (_context.LiveDepth > 1 && !_context.CurrentWasAssignedAtCreation)
				{
					Logic_DoAssignmentToParentProperty(_context);
				}
				if (flag)
				{
					if (_context.LiveDepth > 1)
					{
						Logic_AddDependencyForUnresolvedChildren(_context, null);
					}
				}
				else if (!flag2)
				{
					ExecutePendingAdds(_context.CurrentType, _context.CurrentInstance);
					Logic_EndInit(_context);
				}
			}
		}
		else
		{
			if (flag)
			{
				Logic_AddDependencyForUnresolvedChildren(_context, null);
			}
			else
			{
				ExecutePendingAdds(_context.CurrentType, _context.CurrentInstance);
			}
			if (_context.ParentIsPropertyValueSet)
			{
				throw _context.WithLineInfo(new XamlDuplicateMemberException(_context.ParentProperty, _context.ParentType));
			}
		}
		_lastInstance = _context.CurrentInstance;
		string currentInstanceRegisteredName = _context.CurrentInstanceRegisteredName;
		if (_context.LiveDepth == 1)
		{
			_rootNamescope = _context.RootNameScope;
		}
		_context.PopScope();
		if (flag)
		{
			_nameFixupGraph.IsOffTheStack(_lastInstance, currentInstanceRegisteredName, _context.LineNumber, _context.LinePosition);
		}
		else if (flag2)
		{
			if (currentInstanceRegisteredName != null)
			{
				NameFixupToken nameFixupToken = (NameFixupToken)_lastInstance;
				if (nameFixupToken.FixupType == FixupType.ObjectInitializationValue && !nameFixupToken.CanAssignDirectly)
				{
					nameFixupToken.SavedContext.Stack.PreviousFrame.InstanceRegisteredName = currentInstanceRegisteredName;
				}
			}
		}
		else if (_nameFixupGraph != null)
		{
			TriggerNameResolution(_lastInstance, currentInstanceRegisteredName);
		}
		if (_context.LiveDepth == 0 && !_inDispose)
		{
			CompleteNameReferences();
			_context.RaiseNameScopeInitializationCompleteEvent();
		}
	}

	/// <summary>Writes a new member node into the current object scope, and the scope becomes a new member scope.</summary>
	/// <param name="property">The XAML property to write. Typically you obtain this value from a XAML reader through the XAML node stream intermediate.</param>
	/// <exception cref="T:System.Xaml.XamlObjectWriterException">
	///   <paramref name="property" /> has <see cref="P:System.Xaml.XamlMember.IsUnknown" /> set to true.-or-<see cref="P:System.Xaml.XamlLanguage.UnknownContent" /> passed, which indicates that the XAML node stream contained content for an object that does not support content.-or-Attempted to write a start member in an invalid scope.</exception>
	/// <exception cref="T:System.Xaml.XamlDuplicateMemberException">
	///   <paramref name="property" /> specifies a property that is already explicitly set in the parent object scope.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="property" /> is null.</exception>
	public override void WriteStartMember(XamlMember property)
	{
		ThrowIfDisposed();
		ArgumentNullException.ThrowIfNull(property, "property");
		_deferringWriter.WriteStartMember(property);
		if (_deferringWriter.Handled)
		{
			return;
		}
		string text = null;
		if (_nextNodeMustBeEndMember)
		{
			text = System.SR.ValueMustBeFollowedByEndMember;
		}
		else if (property == XamlLanguage.UnknownContent)
		{
			text = System.SR.Format(System.SR.TypeHasNoContentProperty, _context.CurrentType);
		}
		else if (property.IsUnknown)
		{
			text = System.SR.Format(System.SR.CantSetUnknownProperty, property.ToString());
		}
		else if (_context.CurrentProperty != null)
		{
			text = System.SR.Format(System.SR.OpenPropertyInCurrentFrame_SM, _context.CurrentType.ToString(), _context.CurrentProperty.ToString(), property.ToString());
		}
		else if (_context.CurrentType == null)
		{
			text = System.SR.Format(System.SR.NoTypeInCurrentFrame_SM, property.ToString());
		}
		if (text != null)
		{
			throw _context.WithLineInfo(new XamlObjectWriterException(text));
		}
		_context.CurrentProperty = property;
		Logic_DuplicatePropertyCheck(_context, property, onParent: false);
		if (_context.CurrentInstance == null)
		{
			if (!IsConstructionDirective(_context.CurrentProperty) && !IsDirectiveAllowedOnNullInstance(_context.CurrentProperty, _context.CurrentType))
			{
				Logic_CreateAndAssignToParentStart(_context);
			}
			if (property == XamlLanguage.PositionalParameters)
			{
				_context.CurrentCollection = new List<PositionalParameterDescriptor>();
			}
		}
		else
		{
			if (IsTextConstructionDirective(property))
			{
				throw _context.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.LateConstructionDirective, property.Name)));
			}
			if (_context.CurrentIsTypeConvertedObject)
			{
				if (!property.IsDirective && !property.IsAttachable)
				{
					throw _context.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.SettingPropertiesIsNotAllowed, property.Name)));
				}
				if (property.IsAttachable && _context.CurrentInstance is NameFixupToken)
				{
					NameFixupToken nameFixupToken = (NameFixupToken)_context.CurrentInstance;
					throw _context.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.AttachedPropOnFwdRefTC, property, _context.CurrentType, string.Join(", ", nameFixupToken.NeededNames.ToArray()))));
				}
			}
		}
		if (property.IsDirective && property != XamlLanguage.Items && property != XamlLanguage.PositionalParameters)
		{
			XamlType type = property.Type;
			if (type.IsCollection || type.IsDictionary)
			{
				_context.CurrentCollection = Runtime.CreateInstance(property.Type, null);
			}
		}
	}

	/// <summary>Closes the current member scope, and may write the value of the member scope while it closes. The new scope becomes the parent object scope of the member.</summary>
	/// <exception cref="T:System.Xaml.XamlObjectWriterException">Failed to create member value from a value node that is encountered between this call and a previous StartMember.</exception>
	public override void WriteEndMember()
	{
		ThrowIfDisposed();
		_deferringWriter.WriteEndMember();
		if (_deferringWriter.Handled)
		{
			return;
		}
		XamlMember xamlMember = ((!(_context.CurrentType == null)) ? _context.CurrentProperty : _context.ParentProperty);
		if (xamlMember == null)
		{
			string message = ((_context.CurrentType != null) ? System.SR.Format(System.SR.NoPropertyInCurrentFrame_EM, _context.CurrentType.ToString()) : System.SR.NoPropertyInCurrentFrame_EM_noType);
			throw _context.WithLineInfo(new XamlObjectWriterException(message));
		}
		_nextNodeMustBeEndMember = false;
		_lastInstance = null;
		if (xamlMember == XamlLanguage.Arguments)
		{
			_context.CurrentCtorArgs = ((List<object>)_context.CurrentCollection).ToArray();
		}
		else if (xamlMember == XamlLanguage.Initialization)
		{
			Logic_CreateFromInitializationValue(_context);
		}
		else if (xamlMember == XamlLanguage.Items)
		{
			_context.CurrentCollection = null;
		}
		else if (xamlMember == XamlLanguage.PositionalParameters)
		{
			Logic_ConvertPositionalParamsToArgs(_context);
		}
		else if (xamlMember == XamlLanguage.Class)
		{
			object value = null;
			if (_context.CurrentType == null)
			{
				value = _context.CurrentInstance;
				_context.PopScope();
			}
			Logic_ValidateXClass(_context, value);
		}
		else if (_context.CurrentType == null)
		{
			object currentInstance = _context.CurrentInstance;
			bool flag = true;
			if (currentInstance != null)
			{
				if (currentInstance is MarkupExtension currentInstance2)
				{
					_context.CurrentInstance = currentInstance2;
					XamlType xamlType = GetXamlType(currentInstance.GetType());
					if (!xamlMember.Type.IsMarkupExtension || !xamlType.CanAssignTo(xamlMember.Type))
					{
						Logic_AssignProvidedValue(_context);
						flag = false;
					}
				}
				else
				{
					XamlType xamlType2 = GetXamlType(currentInstance.GetType());
					if (xamlType2 == XamlLanguage.String || !xamlType2.CanAssignTo(xamlMember.Type))
					{
						if (xamlMember.IsDirective && xamlMember == XamlLanguage.Key && !Logic_ShouldConvertKey(_context))
						{
							flag = true;
							_context.ParentKeyIsUnconverted = true;
						}
						else
						{
							flag = Logic_CreatePropertyValueFromValue(_context);
						}
					}
				}
			}
			_lastInstance = _context.CurrentInstance;
			if (flag)
			{
				Logic_DoAssignmentToParentProperty(_context);
			}
			_context.PopScope();
		}
		_context.CurrentProperty = null;
		_context.CurrentIsPropertyValueSet = false;
	}

	/// <summary>Writes a value to the current member scope. If the current scope is inside a collection, dictionary, or array object, the value should be added to the collection, dictionary, or array.</summary>
	/// <param name="value">The value to write.</param>
	/// <exception cref="T:System.Xaml.XamlObjectWriterException">Attempted to write a value in an invalid scope.</exception>
	public override void WriteValue(object value)
	{
		ThrowIfDisposed();
		_deferringWriter.WriteValue(value);
		if (_deferringWriter.Handled)
		{
			if (_deferringWriter.Mode == DeferringMode.TemplateReady)
			{
				XamlNodeList xamlNodeList = _deferringWriter.CollectTemplateList();
				_context.PushScope();
				_context.CurrentInstance = xamlNodeList.GetReader();
			}
			return;
		}
		XamlMember currentProperty = _context.CurrentProperty;
		if (currentProperty == null)
		{
			string message = ((_context.CurrentType != null) ? System.SR.Format(System.SR.NoPropertyInCurrentFrame_V, value, _context.CurrentType.ToString()) : System.SR.Format(System.SR.NoPropertyInCurrentFrame_V_noType, value));
			throw _context.WithLineInfo(new XamlObjectWriterException(message));
		}
		_lastInstance = null;
		_context.PushScope();
		_context.CurrentInstance = value;
		XamlMember xamlMember = currentProperty;
		_nextNodeMustBeEndMember = true;
		if (!xamlMember.IsDirective)
		{
			return;
		}
		XamlType type = xamlMember.Type;
		if (type.IsCollection || type.IsDictionary)
		{
			_nextNodeMustBeEndMember = false;
			if (xamlMember == XamlLanguage.PositionalParameters)
			{
				_context.CurrentType = XamlLanguage.PositionalParameterDescriptor;
				_context.CurrentInstance = new PositionalParameterDescriptor(value, wasText: true);
				Logic_DoAssignmentToParentCollection(_context);
				_context.PopScope();
			}
			else
			{
				_context.CurrentInstance = value;
				Logic_DoAssignmentToParentCollection(_context);
				_context.PopScope();
			}
		}
	}

	/// <summary>Defines a namespace declaration that applies to the next object scope or member scope.</summary>
	/// <param name="namespaceDeclaration">The namespace declaration to write.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="namespaceDeclaration" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">A component of <paramref name="namespaceDeclaration" /> is null.</exception>
	/// <exception cref="T:System.Xaml.XamlObjectReaderException">Tried to write a XAML namespace node in an invalid scope.</exception>
	public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
	{
		ThrowIfDisposed();
		ArgumentNullException.ThrowIfNull(namespaceDeclaration, "namespaceDeclaration");
		if (namespaceDeclaration.Prefix == null)
		{
			throw new ArgumentException(System.SR.NamespaceDeclarationPrefixCannotBeNull);
		}
		if (namespaceDeclaration.Namespace == null)
		{
			throw new ArgumentException(System.SR.NamespaceDeclarationNamespaceCannotBeNull);
		}
		_deferringWriter.WriteNamespace(namespaceDeclaration);
		if (!_deferringWriter.Handled)
		{
			if (_nextNodeMustBeEndMember)
			{
				string valueMustBeFollowedByEndMember = System.SR.ValueMustBeFollowedByEndMember;
				throw _context.WithLineInfo(new XamlObjectWriterException(valueMustBeFollowedByEndMember));
			}
			if (_context.CurrentType != null && _context.CurrentProperty == null)
			{
				string message = System.SR.Format(System.SR.NoPropertyInCurrentFrame_NS, namespaceDeclaration.Prefix, namespaceDeclaration.Namespace, _context.CurrentType.ToString());
				throw _context.WithLineInfo(new XamlObjectWriterException(message));
			}
			if (_context.CurrentType != null)
			{
				_context.PushScope();
			}
			_context.AddNamespacePrefix(namespaceDeclaration.Prefix, namespaceDeclaration.Namespace);
		}
	}

	/// <summary>Resets all possible frames and clears the object graph.</summary>
	public void Clear()
	{
		ThrowIfDisposed();
		while (_context.LiveDepth > 0)
		{
			_context.PopScope();
		}
		_rootNamescope = null;
		_nextNodeMustBeEndMember = false;
		_deferringWriter.Clear();
		_context.PushScope();
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Xaml.XamlObjectWriter" />, and optionally, releases the managed resources. </summary>
	/// <param name="disposing">true to release the managed resources; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			_inDispose = true;
			if (!disposing || base.IsDisposed)
			{
				return;
			}
			if (_context.LiveDepth > 1 || _context.CurrentType != null)
			{
				while (_context.LiveDepth > 0)
				{
					if (_context.CurrentProperty != null)
					{
						WriteEndMember();
					}
					WriteEndObject();
				}
			}
			_deferringWriter.Close();
			_deferringWriter = null;
			_context = null;
			_afterBeginInitHandler = null;
			_beforePropertiesHandler = null;
			_afterPropertiesHandler = null;
			_afterEndInitHandler = null;
		}
		finally
		{
			base.Dispose(disposing);
			_inDispose = false;
		}
	}

	private void ThrowIfDisposed()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException("XamlObjectWriter");
		}
	}

	/// <summary>Implements <see cref="M:System.Xaml.IXamlLineInfoConsumer.SetLineInfo(System.Int32,System.Int32)" /> and provides line information for exceptions.</summary>
	/// <param name="lineNumber">The line number from the reader.</param>
	/// <param name="linePosition">The line position from the reader.</param>
	public void SetLineInfo(int lineNumber, int linePosition)
	{
		ThrowIfDisposed();
		_context.LineNumber = lineNumber;
		_context.LinePosition = linePosition;
		_deferringWriter.SetLineInfo(lineNumber, linePosition);
	}

	XamlException IAddLineInfo.WithLineInfo(XamlException ex)
	{
		return _context.WithLineInfo(ex);
	}

	private object GetKeyFromInstance(object instance, XamlType instanceType, IAddLineInfo lineInfo)
	{
		XamlMember aliasedProperty = instanceType.GetAliasedProperty(XamlLanguage.Key);
		if (aliasedProperty == null || instance == null)
		{
			throw lineInfo.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.MissingKey, instanceType.Name)));
		}
		return Runtime.GetValue(instance, aliasedProperty);
	}

	private XamlType GetXamlType(Type clrType)
	{
		XamlType xamlType = SchemaContext.GetXamlType(clrType);
		if (xamlType == null)
		{
			throw new InvalidOperationException(System.SR.Format(System.SR.ObjectWriterTypeNotAllowed, SchemaContext.GetType(), clrType));
		}
		return xamlType;
	}

	private bool IsConstructionDirective(XamlMember xamlMember)
	{
		if (!(xamlMember == XamlLanguage.Arguments) && !(xamlMember == XamlLanguage.Base) && !(xamlMember == XamlLanguage.FactoryMethod) && !(xamlMember == XamlLanguage.Initialization) && !(xamlMember == XamlLanguage.PositionalParameters))
		{
			return xamlMember == XamlLanguage.TypeArguments;
		}
		return true;
	}

	private bool IsTextConstructionDirective(XamlMember xamlMember)
	{
		if (!(xamlMember == XamlLanguage.Arguments) && !(xamlMember == XamlLanguage.FactoryMethod) && !(xamlMember == XamlLanguage.PositionalParameters))
		{
			return xamlMember == XamlLanguage.TypeArguments;
		}
		return true;
	}

	private bool IsDirectiveAllowedOnNullInstance(XamlMember xamlMember, XamlType xamlType)
	{
		if (xamlMember == XamlLanguage.Key)
		{
			return true;
		}
		if (xamlMember == XamlLanguage.Uid && null == xamlType.GetAliasedProperty(XamlLanguage.Uid))
		{
			return true;
		}
		return false;
	}

	private void Logic_CreateAndAssignToParentStart(ObjectWriterContext ctx)
	{
		XamlType currentType = ctx.CurrentType;
		if (ctx.CurrentIsObjectFromMember)
		{
			throw ctx.WithLineInfo(new XamlInternalException(System.SR.ConstructImplicitType));
		}
		if (currentType.IsMarkupExtension && ctx.CurrentCtorArgs != null)
		{
			object[] currentCtorArgs = ctx.CurrentCtorArgs;
			for (int i = 0; i < currentCtorArgs.Length; i++)
			{
				if (currentCtorArgs[i] is MarkupExtension me)
				{
					currentCtorArgs[i] = Logic_PushAndPopAProvideValueStackFrame(ctx, XamlLanguage.PositionalParameters, me, useIRME: false);
				}
			}
		}
		object obj;
		if (!ctx.CurrentHasPreconstructionPropertyValuesDictionary || !ctx.CurrentPreconstructionPropertyValues.TryGetValue(XamlLanguage.FactoryMethod, out var value))
		{
			obj = Runtime.CreateInstance(currentType, ctx.CurrentCtorArgs);
		}
		else
		{
			XamlPropertyName xamlPropertyName = XamlPropertyName.Parse((string)value);
			if (xamlPropertyName == null)
			{
				string message = string.Format(TypeConverterHelper.InvariantEnglishUS, System.SR.InvalidExpression, value);
				throw ctx.WithLineInfo(new XamlInternalException(message));
			}
			XamlType xamlType;
			if (xamlPropertyName.Owner == null)
			{
				xamlType = currentType;
			}
			else
			{
				xamlType = ctx.GetXamlType(xamlPropertyName.Owner);
				if (xamlType == null)
				{
					XamlTypeName xamlTypeName = ctx.GetXamlTypeName(xamlPropertyName.Owner);
					throw ctx.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.CannotResolveTypeForFactoryMethod, xamlTypeName, xamlPropertyName.Name)));
				}
			}
			obj = Runtime.CreateWithFactoryMethod(xamlType, xamlPropertyName.Name, ctx.CurrentCtorArgs);
			XamlType xamlType2 = GetXamlType(obj.GetType());
			if (!xamlType2.CanAssignTo(currentType))
			{
				throw ctx.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.NotAssignableFrom, currentType.GetQualifiedName(), xamlType2.GetQualifiedName())));
			}
		}
		ctx.CurrentCtorArgs = null;
		ctx.CurrentInstance = obj;
		if (currentType.IsCollection || currentType.IsDictionary)
		{
			ctx.CurrentCollection = obj;
		}
		Logic_BeginInit(ctx);
		if (ctx.LiveDepth > 1 && !(obj is MarkupExtension) && ctx.LiveDepth > 1)
		{
			Logic_CheckAssignmentToParentStart(ctx);
		}
		OnBeforeProperties(obj);
		Logic_ApplyCurrentPreconstructionPropertyValues(ctx);
	}

	private void Logic_ConvertPositionalParamsToArgs(ObjectWriterContext ctx)
	{
		XamlType currentType = ctx.CurrentType;
		if (!currentType.IsMarkupExtension)
		{
			throw ctx.WithLineInfo(new XamlInternalException(System.SR.NonMEWithPositionalParameters));
		}
		List<PositionalParameterDescriptor> list = (List<PositionalParameterDescriptor>)ctx.CurrentCollection;
		object[] array = new object[list.Count];
		IList<XamlType> positionalParameters = currentType.GetPositionalParameters(list.Count);
		if (positionalParameters == null)
		{
			string message = string.Format(TypeConverterHelper.InvariantEnglishUS, System.SR.NoSuchConstructor, list.Count, currentType.Name);
			throw ctx.WithLineInfo(new XamlObjectWriterException(message));
		}
		int count = positionalParameters.Count;
		for (int i = 0; i < count; i++)
		{
			if (i >= list.Count)
			{
				throw ctx.WithLineInfo(new XamlInternalException(System.SR.PositionalParamsWrongLength));
			}
			XamlType xamlType = positionalParameters[i];
			PositionalParameterDescriptor positionalParameterDescriptor = list[i];
			array[i] = (positionalParameterDescriptor.WasText ? Logic_CreateFromValue(ctx, xamlType.TypeConverter, positionalParameterDescriptor.Value, null, xamlType.Name) : list[i].Value);
			ctx.CurrentCtorArgs = array;
		}
	}

	private void Logic_CreateFromInitializationValue(ObjectWriterContext ctx)
	{
		XamlType parentType = ctx.ParentType;
		XamlValueConverter<TypeConverter> typeConverter = parentType.TypeConverter;
		object currentInstance = ctx.CurrentInstance;
		object obj = null;
		if (parentType.IsUnknown)
		{
			string message = System.SR.Format(System.SR.CantCreateUnknownType, parentType.GetQualifiedName());
			throw ctx.WithLineInfo(new XamlObjectWriterException(message));
		}
		if (typeConverter == null)
		{
			throw ctx.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.InitializationSyntaxWithoutTypeConverter, parentType.GetQualifiedName())));
		}
		obj = Logic_CreateFromValue(ctx, typeConverter, currentInstance, null, parentType.Name);
		ctx.PopScope();
		ctx.CurrentInstance = obj;
		ctx.CurrentIsTypeConvertedObject = true;
		if (!(obj is NameFixupToken))
		{
			if (parentType.IsCollection || parentType.IsDictionary)
			{
				ctx.CurrentCollection = obj;
			}
			Logic_ApplyCurrentPreconstructionPropertyValues(ctx, skipDirectives: true);
		}
	}

	private object Logic_CreateFromValue(ObjectWriterContext ctx, XamlValueConverter<TypeConverter> typeConverter, object value, XamlMember property, string targetName)
	{
		return Logic_CreateFromValue(ctx, typeConverter, value, property, targetName, this);
	}

	private object Logic_CreateFromValue(ObjectWriterContext ctx, XamlValueConverter<TypeConverter> typeConverter, object value, XamlMember property, string targetName, IAddLineInfo lineInfo)
	{
		try
		{
			return Runtime.CreateFromValue(ctx.ServiceProviderContext, typeConverter, value, property);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			string message = System.SR.Format(System.SR.TypeConverterFailed, targetName, value);
			throw lineInfo.WithLineInfo(new XamlObjectWriterException(message, ex));
		}
	}

	private bool Logic_CreatePropertyValueFromValue(ObjectWriterContext ctx)
	{
		XamlMember parentProperty = ctx.ParentProperty;
		_ = parentProperty.Type;
		object currentInstance = ctx.CurrentInstance;
		if (currentInstance is XamlReader deferredContent)
		{
			XamlValueConverter<XamlDeferringLoader> deferringLoader = parentProperty.DeferringLoader;
			if (deferringLoader != null)
			{
				ctx.CurrentInstance = Runtime.DeferredLoad(ctx.ServiceProviderContext, deferringLoader, deferredContent);
				return true;
			}
		}
		XamlValueConverter<TypeConverter> typeConverter = parentProperty.TypeConverter;
		object obj = null;
		XamlType xamlType = null;
		xamlType = ((!parentProperty.IsAttachable) ? ctx.ParentType : parentProperty.DeclaringType);
		if (parentProperty != null && !parentProperty.IsUnknown && xamlType != null)
		{
			XamlType grandParentType = ctx.GrandParentType;
			if (parentProperty.IsDirective && parentProperty == XamlLanguage.Key && grandParentType != null && grandParentType.IsDictionary)
			{
				typeConverter = grandParentType.KeyType.TypeConverter;
			}
			if (typeConverter != null && typeConverter.ConverterType != null && typeConverter != BuiltInValueConverter.String)
			{
				TypeConverter converterInstance = Runtime.GetConverterInstance(typeConverter);
				if (converterInstance != null && xamlType.SetTypeConverterHandler != null)
				{
					XamlSetTypeConverterEventArgs xamlSetTypeConverterEventArgs = new XamlSetTypeConverterEventArgs(parentProperty, converterInstance, currentInstance, ctx.ServiceProviderContext, TypeConverterHelper.InvariantEnglishUS, ctx.ParentInstance);
					xamlSetTypeConverterEventArgs.CurrentType = xamlType;
					xamlType.SetTypeConverterHandler(ctx.ParentInstance, xamlSetTypeConverterEventArgs);
					if (xamlSetTypeConverterEventArgs.Handled)
					{
						return false;
					}
				}
			}
		}
		obj = ((!(typeConverter != null)) ? currentInstance : Logic_CreateFromValue(ctx, typeConverter, currentInstance, parentProperty, parentProperty.Name));
		ctx.CurrentInstance = obj;
		return true;
	}

	private bool Logic_ShouldConvertKey(ObjectWriterContext ctx)
	{
		if (!_preferUnconvertedDictionaryKeys || ctx.GrandParentShouldConvertChildKeys)
		{
			return true;
		}
		if (ctx.GrandParentShouldNotConvertChildKeys)
		{
			return false;
		}
		XamlType grandParentType = ctx.GrandParentType;
		if (grandParentType != null && grandParentType.IsDictionary && typeof(IDictionary).IsAssignableFrom(grandParentType.UnderlyingType) && !IsBuiltInGenericDictionary(grandParentType.UnderlyingType))
		{
			return false;
		}
		ctx.GrandParentShouldConvertChildKeys = true;
		return true;
	}

	private static bool IsBuiltInGenericDictionary(Type type)
	{
		if (type == null || !type.IsGenericType)
		{
			return false;
		}
		Type genericTypeDefinition = type.GetGenericTypeDefinition();
		if (!(genericTypeDefinition == typeof(Dictionary<, >)) && !(genericTypeDefinition == typeof(SortedDictionary<, >)) && !(genericTypeDefinition == typeof(SortedList<, >)))
		{
			return genericTypeDefinition == typeof(ConcurrentDictionary<, >);
		}
		return true;
	}

	private void Logic_BeginInit(ObjectWriterContext ctx)
	{
		object currentInstance = ctx.CurrentInstance;
		XamlType currentType = ctx.CurrentType;
		Runtime.InitializationGuard(currentType, currentInstance, begin: true);
		if (ctx.BaseUri != null)
		{
			Runtime.SetUriBase(currentType, currentInstance, ctx.BaseUri);
		}
		if (currentInstance == ctx.RootInstance)
		{
			Logic_SetConnectionId(ctx, 0, currentInstance);
		}
		OnAfterBeginInit(currentInstance);
	}

	private void Logic_EndInit(ObjectWriterContext ctx)
	{
		XamlType currentType = ctx.CurrentType;
		object currentInstance = ctx.CurrentInstance;
		Runtime.InitializationGuard(currentType, currentInstance, begin: false);
		OnAfterEndInit(currentInstance);
	}

	private void Logic_DeferProvideValue(ObjectWriterContext ctx)
	{
		XamlSavedContext savedContext = ctx.GetSavedContext(SavedContextType.ReparseMarkupExtension);
		if (ctx.LiveDepth > 2 && ctx.ParentProperty == XamlLanguage.Key && ctx.GrandParentType.IsDictionary)
		{
			NameFixupToken tokenForUnresolvedChildren = GetTokenForUnresolvedChildren(ctx.CurrentInstance, XamlLanguage.Key, savedContext);
			Logic_PendKeyFixupToken(ctx, tokenForUnresolvedChildren);
		}
		else
		{
			Logic_AddDependencyForUnresolvedChildren(ctx, savedContext);
		}
	}

	private void Logic_DuplicatePropertyCheck(ObjectWriterContext ctx, XamlMember property, bool onParent)
	{
		if (_skipDuplicatePropertyCheck)
		{
			return;
		}
		HashSet<XamlMember> hashSet = (onParent ? ctx.ParentAssignedProperties : ctx.CurrentAssignedProperties);
		if (hashSet.Contains(property))
		{
			if (property != XamlLanguage.Space)
			{
				XamlType type = (onParent ? ctx.ParentType : ctx.CurrentType);
				throw ctx.WithLineInfo(new XamlDuplicateMemberException(property, type));
			}
		}
		else
		{
			hashSet.Add(property);
		}
	}

	private void Logic_ApplyCurrentPreconstructionPropertyValues(ObjectWriterContext ctx)
	{
		Logic_ApplyCurrentPreconstructionPropertyValues(ctx, skipDirectives: false);
	}

	private void Logic_ApplyCurrentPreconstructionPropertyValues(ObjectWriterContext ctx, bool skipDirectives)
	{
		if (!ctx.CurrentHasPreconstructionPropertyValuesDictionary)
		{
			return;
		}
		Dictionary<XamlMember, object> currentPreconstructionPropertyValues = ctx.CurrentPreconstructionPropertyValues;
		object obj = null;
		foreach (XamlMember key in currentPreconstructionPropertyValues.Keys)
		{
			if (!skipDirectives || !key.IsDirective)
			{
				obj = currentPreconstructionPropertyValues[key];
				if (obj is MarkupExtension me && !key.IsDirective)
				{
					Logic_PushAndPopAProvideValueStackFrame(ctx, key, me, useIRME: true);
				}
				else
				{
					Logic_ApplyPropertyValue(ctx, key, obj, onParent: false);
				}
			}
		}
	}

	private object Logic_PushAndPopAProvideValueStackFrame(ObjectWriterContext ctx, XamlMember prop, MarkupExtension me, bool useIRME)
	{
		XamlMember currentProperty = ctx.CurrentProperty;
		ctx.CurrentProperty = prop;
		ctx.PushScope();
		ctx.CurrentInstance = me;
		object result = null;
		if (useIRME)
		{
			Logic_AssignProvidedValue(ctx);
		}
		else
		{
			result = Runtime.CallProvideValue(me, ctx.ServiceProviderContext);
		}
		ctx.PopScope();
		ctx.CurrentProperty = currentProperty;
		return result;
	}

	private void Logic_ApplyPropertyValue(ObjectWriterContext ctx, XamlMember prop, object value, bool onParent)
	{
		object obj = (onParent ? ctx.ParentInstance : ctx.CurrentInstance);
		if (value is XData)
		{
			XData xData = value as XData;
			if (prop.Type.IsXData)
			{
				Runtime.SetXmlInstance(obj, prop, xData);
				return;
			}
			value = xData.Text;
		}
		SetValue(obj, prop, value);
		if (!prop.IsDirective)
		{
			return;
		}
		XamlMember aliasedProperty = (onParent ? ctx.ParentType : ctx.CurrentType).GetAliasedProperty(prop as XamlDirective);
		if (prop != XamlLanguage.Key && aliasedProperty != null)
		{
			Logic_DuplicatePropertyCheck(ctx, aliasedProperty, onParent);
			object value2 = Logic_CreateFromValue(ctx, aliasedProperty.TypeConverter, value, aliasedProperty, aliasedProperty.Name);
			SetValue(obj, aliasedProperty, value2);
		}
		if (prop == XamlLanguage.Name)
		{
			if (obj == ctx.CurrentInstance)
			{
				Logic_RegisterName_OnCurrent(ctx, (string)value);
			}
			else
			{
				Logic_RegisterName_OnParent(ctx, (string)value);
			}
		}
		else if (prop == XamlLanguage.ConnectionId)
		{
			Logic_SetConnectionId(ctx, (int)value, obj);
		}
		else if (prop == XamlLanguage.Base)
		{
			Logic_CheckBaseUri(ctx, (string)value);
			ctx.BaseUri = new Uri((string)value);
			if (ctx.ParentInstance != null)
			{
				Runtime.SetUriBase(ctx.ParentType, ctx.ParentInstance, ctx.BaseUri);
			}
		}
	}

	private void Logic_CheckBaseUri(ObjectWriterContext ctx, string value)
	{
		if (ctx.BaseUri != null || ctx.Depth > 2)
		{
			throw new XamlObjectWriterException(System.SR.CannotSetBaseUri);
		}
	}

	private void Logic_AssignProvidedValue(ObjectWriterContext ctx)
	{
		if (!Logic_ProvideValue(ctx) && ctx.ParentProperty != null)
		{
			Logic_DoAssignmentToParentProperty(ctx);
		}
	}

	private bool Logic_ProvideValue(ObjectWriterContext ctx)
	{
		MarkupExtension markupExtension = (MarkupExtension)ctx.CurrentInstance;
		object parentInstance = ctx.ParentInstance;
		XamlMember parentProperty = ctx.ParentProperty;
		if (parentProperty != null && !parentProperty.IsUnknown)
		{
			XamlType xamlType = null;
			xamlType = ((!parentProperty.IsAttachable) ? ctx.ParentType : parentProperty.DeclaringType);
			if (xamlType != null && xamlType.SetMarkupExtensionHandler != null)
			{
				XamlSetMarkupExtensionEventArgs xamlSetMarkupExtensionEventArgs = new XamlSetMarkupExtensionEventArgs(parentProperty, markupExtension, ctx.ServiceProviderContext, parentInstance);
				xamlSetMarkupExtensionEventArgs.CurrentType = xamlType;
				xamlType.SetMarkupExtensionHandler(parentInstance, xamlSetMarkupExtensionEventArgs);
				if (xamlSetMarkupExtensionEventArgs.Handled)
				{
					return true;
				}
			}
		}
		object obj = markupExtension;
		if (ctx.LiveDepth != 1 || !_skipProvideValueOnRoot)
		{
			obj = Runtime.CallProvideValue(markupExtension, ctx.ServiceProviderContext);
		}
		if (ctx.ParentProperty != null)
		{
			if (obj != null)
			{
				if (!(obj is NameFixupToken))
				{
					ctx.CurrentType = GetXamlType(obj.GetType());
				}
			}
			else if (ctx.ParentProperty == XamlLanguage.Items)
			{
				ctx.CurrentType = ctx.ParentType.ItemType;
			}
			else
			{
				ctx.CurrentType = ctx.ParentProperty.Type;
			}
			ctx.CurrentInstance = obj;
		}
		else
		{
			ctx.CurrentInstance = obj;
		}
		return false;
	}

	private void Logic_PendCurrentFixupToken_SetValue(ObjectWriterContext ctx, NameFixupToken token)
	{
		token.LineNumber = ctx.LineNumber;
		token.LinePosition = ctx.LinePosition;
		token.Runtime = Runtime;
		NameFixupGraph.AddDependency(token);
	}

	private void Logic_CheckAssignmentToParentStart(ObjectWriterContext ctx)
	{
		bool flag = ctx.ParentProperty == XamlLanguage.Items && ctx.ParentType.IsDictionary;
		if (ctx.CurrentType.IsUsableDuringInitialization && !flag)
		{
			ctx.CurrentWasAssignedAtCreation = true;
			Logic_DoAssignmentToParentProperty(ctx);
		}
		else
		{
			ctx.CurrentWasAssignedAtCreation = false;
		}
	}

	private void Logic_DoAssignmentToParentCollection(ObjectWriterContext ctx)
	{
		object parentCollection = ctx.ParentCollection;
		XamlType parentType = ctx.ParentType;
		XamlType xamlType = ctx.CurrentType;
		object obj = ctx.CurrentInstance;
		if (!parentType.IsDictionary)
		{
			if (!Logic_PendAssignmentToParentCollection(ctx, null, keyIsSet: false))
			{
				if (obj is MarkupExtension me && !Logic_WillParentCollectionAdd(ctx, obj.GetType(), excludeObjectType: true))
				{
					obj = Runtime.CallProvideValue(me, ctx.ServiceProviderContext);
				}
				Runtime.Add(parentCollection, parentType, obj, xamlType);
			}
			return;
		}
		if (xamlType == null)
		{
			xamlType = ((obj == null) ? parentType.ItemType : GetXamlType(obj.GetType()));
		}
		object key = ctx.CurrentKey;
		bool currentIsKeySet = ctx.CurrentIsKeySet;
		if (!Logic_PendAssignmentToParentCollection(ctx, key, currentIsKeySet))
		{
			if (!currentIsKeySet)
			{
				key = GetKeyFromInstance(obj, xamlType, this);
			}
			Logic_AddToParentDictionary(ctx, key, obj);
		}
	}

	private bool Logic_WillParentCollectionAdd(ObjectWriterContext ctx, Type type, bool excludeObjectType)
	{
		XamlType itemType = ctx.ParentType.ItemType;
		if (excludeObjectType && itemType == XamlLanguage.Object)
		{
			return false;
		}
		if (itemType.UnderlyingType.IsAssignableFrom(type))
		{
			return true;
		}
		return false;
	}

	private void Logic_AddToParentDictionary(ObjectWriterContext ctx, object key, object value)
	{
		if (ctx.CurrentKeyIsUnconverted && !ctx.ParentShouldNotConvertChildKeys)
		{
			if (!ctx.ParentShouldConvertChildKeys)
			{
				try
				{
					Runtime.AddToDictionary(ctx.ParentCollection, ctx.ParentType, value, ctx.CurrentType, key);
					ctx.ParentShouldNotConvertChildKeys = true;
					return;
				}
				catch (XamlObjectWriterException ex)
				{
					if (!(ex.InnerException is ArgumentException) && !(ex.InnerException is InvalidCastException))
					{
						throw;
					}
					Debugger.IsLogging();
				}
				ctx.ParentShouldConvertChildKeys = true;
			}
			ctx.CurrentProperty = XamlLanguage.Key;
			ctx.PushScope();
			ctx.CurrentInstance = key;
			Logic_CreatePropertyValueFromValue(ctx);
			key = ctx.CurrentInstance;
			ctx.PopScope();
			ctx.CurrentProperty = null;
		}
		Runtime.AddToDictionary(ctx.ParentCollection, ctx.ParentType, value, ctx.CurrentType, key);
	}

	private bool Logic_PendAssignmentToParentCollection(ObjectWriterContext ctx, object key, bool keyIsSet)
	{
		object parentCollection = ctx.ParentCollection;
		object currentInstance = ctx.CurrentInstance;
		NameFixupToken nameFixupToken = key as NameFixupToken;
		NameFixupToken nameFixupToken2 = currentInstance as NameFixupToken;
		List<PendingCollectionAdd> value = null;
		if (_pendingCollectionAdds != null)
		{
			PendingCollectionAdds.TryGetValue(parentCollection, out value);
		}
		if (value == null && (nameFixupToken != null || nameFixupToken2 != null || HasUnresolvedChildren(key) || HasUnresolvedChildren(currentInstance)))
		{
			value = new List<PendingCollectionAdd>();
			PendingCollectionAdds.Add(parentCollection, value);
		}
		if (nameFixupToken != null)
		{
			nameFixupToken.Target.KeyHolder = null;
			nameFixupToken.Target.TemporaryCollectionIndex = value.Count;
		}
		if (nameFixupToken2 != null)
		{
			Logic_PendCurrentFixupToken_SetValue(ctx, nameFixupToken2);
			nameFixupToken2.Target.TemporaryCollectionIndex = value.Count;
		}
		if (value != null)
		{
			PendingCollectionAdd pendingCollectionAdd = new PendingCollectionAdd
			{
				Key = key,
				KeyIsSet = keyIsSet,
				KeyIsUnconverted = ctx.CurrentKeyIsUnconverted,
				Item = currentInstance,
				ItemType = ctx.CurrentType,
				LineNumber = ctx.LineNumber,
				LinePosition = ctx.LinePosition
			};
			value.Add(pendingCollectionAdd);
			if (pendingCollectionAdd.KeyIsUnconverted && !PendingKeyConversionContexts.ContainsKey(parentCollection))
			{
				XamlSavedContext savedContext = ctx.GetSavedContext(SavedContextType.ReparseMarkupExtension);
				PendingKeyConversionContexts.Add(parentCollection, new ObjectWriterContext(savedContext, null, null, Runtime));
			}
			return true;
		}
		return false;
	}

	private void Logic_DoAssignmentToParentProperty(ObjectWriterContext ctx)
	{
		XamlMember parentProperty = ctx.ParentProperty;
		object currentInstance = ctx.CurrentInstance;
		XamlType type = parentProperty.Type;
		if (parentProperty.IsDirective && (type.IsCollection || type.IsDictionary))
		{
			if (currentInstance is NameFixupToken && parentProperty != XamlLanguage.Items)
			{
				NameFixupToken nameFixupToken = currentInstance as NameFixupToken;
				string p = string.Join(',', nameFixupToken.NeededNames.ToArray());
				string message = System.SR.Format(System.SR.ForwardRefDirectives, p);
				throw ctx.WithLineInfo(new XamlObjectWriterException(message));
			}
			if (parentProperty == XamlLanguage.PositionalParameters)
			{
				ctx.CurrentType = XamlLanguage.PositionalParameterDescriptor;
				ctx.CurrentInstance = new PositionalParameterDescriptor(currentInstance, wasText: false);
			}
			Logic_DoAssignmentToParentCollection(ctx);
			return;
		}
		if (ctx.ParentInstance != null)
		{
			if (ctx.ParentIsPropertyValueSet)
			{
				throw ctx.WithLineInfo(new XamlDuplicateMemberException(ctx.ParentProperty, ctx.ParentType));
			}
			ctx.ParentIsPropertyValueSet = true;
			if (currentInstance is NameFixupToken)
			{
				NameFixupToken nameFixupToken2 = (NameFixupToken)currentInstance;
				if (parentProperty.IsDirective)
				{
					if (parentProperty != XamlLanguage.Key)
					{
						string p2 = string.Join(',', nameFixupToken2.NeededNames.ToArray());
						string message2 = System.SR.Format(System.SR.ForwardRefDirectives, p2);
						throw ctx.WithLineInfo(new XamlObjectWriterException(message2));
					}
					Logic_PendKeyFixupToken(ctx, nameFixupToken2);
				}
				else
				{
					Logic_PendCurrentFixupToken_SetValue(ctx, nameFixupToken2);
				}
				return;
			}
			XamlType parentType = ctx.ParentType;
			if (!ctx.CurrentIsObjectFromMember)
			{
				Logic_ApplyPropertyValue(ctx, parentProperty, currentInstance, onParent: true);
				if (parentProperty == parentType.GetAliasedProperty(XamlLanguage.Name))
				{
					Logic_RegisterName_OnParent(ctx, (string)currentInstance);
				}
				if (parentProperty == XamlLanguage.Key)
				{
					ctx.ParentKey = currentInstance;
				}
			}
			return;
		}
		if (parentProperty.IsDirective)
		{
			if (parentProperty == XamlLanguage.Base)
			{
				Logic_CheckBaseUri(ctx, (string)currentInstance);
				ctx.BaseUri = new Uri((string)currentInstance);
			}
			else if (currentInstance is NameFixupToken)
			{
				if (parentProperty != XamlLanguage.Key)
				{
					NameFixupToken nameFixupToken3 = (NameFixupToken)currentInstance;
					string p3 = string.Join(',', nameFixupToken3.NeededNames.ToArray());
					string message3 = System.SR.Format(System.SR.ForwardRefDirectives, p3);
					throw ctx.WithLineInfo(new XamlObjectWriterException(message3));
				}
				Logic_PendKeyFixupToken(ctx, (NameFixupToken)currentInstance);
			}
			else if (parentProperty == XamlLanguage.Key)
			{
				ctx.ParentKey = currentInstance;
			}
			else
			{
				ctx.ParentPreconstructionPropertyValues.Add(parentProperty, currentInstance);
			}
			return;
		}
		throw new XamlInternalException(System.SR.BadStateObjectWriter);
	}

	private void Logic_PendKeyFixupToken(ObjectWriterContext ctx, NameFixupToken token)
	{
		token.Target.Instance = ctx.GrandParentInstance;
		token.Target.InstanceType = ctx.GrandParentType;
		token.Target.InstanceWasGotten = ctx.GrandParentIsObjectFromMember;
		FixupTargetKeyHolder fixupTargetKeyHolder = new FixupTargetKeyHolder(token);
		token.Target.KeyHolder = fixupTargetKeyHolder;
		ctx.ParentKey = fixupTargetKeyHolder;
		if (token.Target.Instance != null)
		{
			Logic_PendCurrentFixupToken_SetValue(ctx, token);
		}
	}

	private void Logic_RegisterName_OnCurrent(ObjectWriterContext ctx, string name)
	{
		bool isRoot = ctx.LiveDepth == 1;
		RegisterName(ctx, name, ctx.CurrentInstance, ctx.CurrentType, ctx.CurrentNameScope, ctx.ParentNameScope, isRoot);
		ctx.CurrentInstanceRegisteredName = name;
	}

	private void Logic_RegisterName_OnParent(ObjectWriterContext ctx, string name)
	{
		RegisterName(ctx, name, ctx.ParentInstance, ctx.ParentType, ctx.ParentNameScope, ctx.GrandParentNameScope, isRoot: false);
		ctx.ParentInstanceRegisteredName = name;
	}

	private void RegisterName(ObjectWriterContext ctx, string name, object inst, XamlType xamlType, INameScope nameScope, INameScope parentNameScope, bool isRoot)
	{
		INameScope nameScope2 = nameScope;
		if (nameScope is NameScopeDictionary nameScopeDictionary)
		{
			nameScope2 = nameScopeDictionary.UnderlyingNameScope;
		}
		if (nameScope2 == inst && !isRoot)
		{
			nameScope = parentNameScope;
		}
		if (inst is NameFixupToken)
		{
			return;
		}
		try
		{
			nameScope.RegisterName(name, inst);
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex))
			{
				throw;
			}
			throw ctx.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.NameScopeException, ex.Message), ex));
		}
	}

	private void Logic_SetConnectionId(ObjectWriterContext ctx, int connectionId, object instance)
	{
		object rootInstance = ctx.RootInstance;
		Runtime.SetConnectionId(rootInstance, connectionId, instance);
	}

	private void SetValue(object inst, XamlMember property, object value)
	{
		if (property.IsDirective || !OnSetValue(inst, property, value))
		{
			Runtime.SetValue(inst, property, value);
		}
	}

	private void Logic_ValidateXClass(ObjectWriterContext ctx, object value)
	{
		if (ctx.Depth > 1)
		{
			throw ctx.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.DirectiveNotAtRoot, XamlLanguage.Class)));
		}
		string text = value as string;
		if (text == null)
		{
			throw ctx.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.DirectiveMustBeString, XamlLanguage.Class)));
		}
		object currentInstance = ctx.CurrentInstance;
		Type type = ((currentInstance != null) ? currentInstance.GetType() : ctx.CurrentType.UnderlyingType);
		if (type.FullName != text)
		{
			string rootNamespace = SchemaContext.GetRootNamespace(type.Assembly);
			if (!string.IsNullOrEmpty(rootNamespace))
			{
				text = rootNamespace + "." + text;
			}
			if (type.FullName != text)
			{
				throw ctx.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.XClassMustMatchRootInstance, text, type.FullName)));
			}
		}
	}

	private void Logic_AddDependencyForUnresolvedChildren(ObjectWriterContext ctx, XamlSavedContext deferredMarkupExtensionContext)
	{
		object currentInstance = ctx.CurrentInstance;
		XamlMember parentProperty = ctx.ParentProperty;
		if (parentProperty != null && parentProperty.IsDirective && ctx.ParentInstance == null && parentProperty != XamlLanguage.Key)
		{
			List<string> list = new List<string>();
			_nameFixupGraph.GetDependentNames(currentInstance, list);
			string p = string.Join(", ", list.ToArray());
			throw ctx.WithLineInfo(new XamlObjectWriterException(System.SR.Format(System.SR.TransitiveForwardRefDirectives, currentInstance.GetType(), parentProperty, p)));
		}
		NameFixupToken tokenForUnresolvedChildren = GetTokenForUnresolvedChildren(currentInstance, parentProperty, deferredMarkupExtensionContext);
		tokenForUnresolvedChildren.Target.Instance = ctx.ParentInstance;
		tokenForUnresolvedChildren.Target.InstanceType = ctx.ParentType;
		tokenForUnresolvedChildren.Target.InstanceWasGotten = ctx.ParentIsObjectFromMember;
		Logic_PendCurrentFixupToken_SetValue(ctx, tokenForUnresolvedChildren);
	}

	private NameFixupToken GetTokenForUnresolvedChildren(object childThatHasUnresolvedChildren, XamlMember property, XamlSavedContext deferredMarkupExtensionContext)
	{
		NameFixupToken nameFixupToken = new NameFixupToken();
		if (deferredMarkupExtensionContext != null)
		{
			nameFixupToken.FixupType = FixupType.MarkupExtensionFirstRun;
			nameFixupToken.SavedContext = deferredMarkupExtensionContext;
		}
		else
		{
			nameFixupToken.FixupType = FixupType.UnresolvedChildren;
		}
		nameFixupToken.ReferencedObject = childThatHasUnresolvedChildren;
		nameFixupToken.Target.Property = property;
		return nameFixupToken;
	}

	private void CompleteNameReferences()
	{
		if (_nameFixupGraph == null)
		{
			return;
		}
		List<NameFixupToken> list = null;
		foreach (NameFixupToken remainingSimpleFixup in _nameFixupGraph.GetRemainingSimpleFixups())
		{
			object obj = remainingSimpleFixup.ResolveName(remainingSimpleFixup.NeededNames[0]);
			if (obj == null)
			{
				if (list == null)
				{
					list = new List<NameFixupToken>();
				}
				list.Add(remainingSimpleFixup);
			}
			else if (list == null)
			{
				remainingSimpleFixup.ReferencedObject = obj;
				remainingSimpleFixup.NeededNames.RemoveAt(0);
				ProcessNameFixup(remainingSimpleFixup, nameResolutionIsComplete: true);
				_nameFixupGraph.AddEndOfParseDependency(remainingSimpleFixup.ReferencedObject, remainingSimpleFixup.Target);
			}
		}
		if (list != null)
		{
			ThrowUnresolvedRefs(list);
		}
		foreach (NameFixupToken remainingReparse in _nameFixupGraph.GetRemainingReparses())
		{
			ProcessNameFixup(remainingReparse, nameResolutionIsComplete: true);
			_nameFixupGraph.AddEndOfParseDependency(remainingReparse.TargetContext.CurrentInstance, remainingReparse.Target);
		}
		foreach (NameFixupToken remainingObjectDependency in _nameFixupGraph.GetRemainingObjectDependencies())
		{
			ProcessNameFixup(remainingObjectDependency, nameResolutionIsComplete: true);
			if (remainingObjectDependency.Target.Instance != null && !_nameFixupGraph.HasUnresolvedChildren(remainingObjectDependency.Target.Instance))
			{
				CompleteDeferredInitialization(remainingObjectDependency.Target);
			}
		}
	}

	private void ThrowUnresolvedRefs(IEnumerable<NameFixupToken> unresolvedRefs)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (NameFixupToken unresolvedRef in unresolvedRefs)
		{
			if (!flag)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(System.SR.Format(System.SR.UnresolvedForwardReferences, unresolvedRef.NeededNames[0]));
			if (unresolvedRef.LineNumber != 0)
			{
				if (unresolvedRef.LinePosition != 0)
				{
					stringBuilder.Append(System.SR.Format(System.SR.LineNumberAndPosition, string.Empty, unresolvedRef.LineNumber, unresolvedRef.LinePosition));
				}
				stringBuilder.Append(System.SR.Format(System.SR.LineNumberOnly, string.Empty, unresolvedRef.LineNumber));
			}
			flag = false;
		}
		throw new XamlObjectWriterException(stringBuilder.ToString());
	}

	private void TriggerNameResolution(object instance, string name)
	{
		_nameFixupGraph.ResolveDependenciesTo(instance, name);
		while (_nameFixupGraph.HasResolvedTokensPendingProcessing)
		{
			NameFixupToken nextResolvedTokenPendingProcessing = _nameFixupGraph.GetNextResolvedTokenPendingProcessing();
			ProcessNameFixup(nextResolvedTokenPendingProcessing, nameResolutionIsComplete: false);
			if (nextResolvedTokenPendingProcessing.FixupType == FixupType.ObjectInitializationValue && !nextResolvedTokenPendingProcessing.CanAssignDirectly && nextResolvedTokenPendingProcessing.TargetContext.CurrentInstanceRegisteredName != null && !_context.IsOnTheLiveStack(nextResolvedTokenPendingProcessing.TargetContext.CurrentInstance))
			{
				string currentInstanceRegisteredName = nextResolvedTokenPendingProcessing.TargetContext.CurrentInstanceRegisteredName;
				object currentInstance = nextResolvedTokenPendingProcessing.TargetContext.CurrentInstance;
				_nameFixupGraph.ResolveDependenciesTo(currentInstance, currentInstanceRegisteredName);
			}
			if (!nextResolvedTokenPendingProcessing.Target.InstanceIsOnTheStack && !_nameFixupGraph.HasUnresolvedOrPendingChildren(nextResolvedTokenPendingProcessing.Target.Instance))
			{
				CompleteDeferredInitialization(nextResolvedTokenPendingProcessing.Target);
				object instance2 = nextResolvedTokenPendingProcessing.Target.Instance;
				string instanceName = nextResolvedTokenPendingProcessing.Target.InstanceName;
				_nameFixupGraph.ResolveDependenciesTo(instance2, instanceName);
			}
		}
	}

	bool ICheckIfInitialized.IsFullyInitialized(object instance)
	{
		if (instance == null)
		{
			return true;
		}
		if (_context.LiveDepth > 0)
		{
			if (_context.IsOnTheLiveStack(instance))
			{
				return false;
			}
			if (_nameFixupGraph != null)
			{
				return !_nameFixupGraph.HasUnresolvedOrPendingChildren(instance);
			}
			return true;
		}
		if (_nameFixupGraph != null)
		{
			return !_nameFixupGraph.WasUninitializedAtEndOfParse(instance);
		}
		return true;
	}

	private void CompleteDeferredInitialization(FixupTarget target)
	{
		ExecutePendingAdds(target.InstanceType, target.Instance);
		if (!target.InstanceWasGotten)
		{
			IAddLineInfo lineInfo = Runtime.LineInfo;
			Runtime.LineInfo = target;
			try
			{
				Runtime.InitializationGuard(target.InstanceType, target.Instance, begin: false);
			}
			finally
			{
				Runtime.LineInfo = lineInfo;
			}
			OnAfterEndInit(target.Instance);
		}
	}

	private void ProcessNameFixup(NameFixupToken token, bool nameResolutionIsComplete)
	{
		IAddLineInfo lineInfo = Runtime.LineInfo;
		try
		{
			Runtime.LineInfo = token;
			if (token.CanAssignDirectly)
			{
				ProcessNameFixup_Simple(token);
			}
			else if (token.FixupType != FixupType.UnresolvedChildren)
			{
				ProcessNameFixup_Reparse(token, nameResolutionIsComplete);
			}
		}
		finally
		{
			Runtime.LineInfo = lineInfo;
		}
	}

	private void ProcessNameFixup_Simple(NameFixupToken token)
	{
		object referencedObject = token.ReferencedObject;
		if (token.Target.Property == XamlLanguage.Key)
		{
			ProcessNameFixup_UpdatePendingAddKey(token, referencedObject);
		}
		else if (token.Target.Property == XamlLanguage.Items)
		{
			ProcessNameFixup_UpdatePendingAddItem(token, referencedObject);
		}
		else
		{
			SetValue(token.Target.Instance, token.Target.Property, referencedObject);
		}
	}

	private void ProcessNameFixup_Reparse(NameFixupToken token, bool nameResolutionIsComplete)
	{
		object obj = null;
		ObjectWriterContext targetContext = token.TargetContext;
		targetContext.NameResolutionComplete = nameResolutionIsComplete;
		targetContext.IsInitializedCallback = this;
		switch (token.FixupType)
		{
		case FixupType.MarkupExtensionFirstRun:
			if (Logic_ProvideValue(targetContext))
			{
				return;
			}
			break;
		case FixupType.MarkupExtensionRerun:
			obj = Runtime.CallProvideValue((MarkupExtension)targetContext.CurrentInstance, targetContext.ServiceProviderContext);
			targetContext.CurrentInstance = obj;
			break;
		case FixupType.PropertyValue:
			obj = Logic_CreateFromValue(targetContext, targetContext.ParentProperty.TypeConverter, targetContext.CurrentInstance, targetContext.ParentProperty, targetContext.ParentProperty.Name, token);
			token.TargetContext.CurrentInstance = obj;
			break;
		case FixupType.ObjectInitializationValue:
			Logic_CreateFromInitializationValue(targetContext);
			if (token.TargetContext.CurrentInstanceRegisteredName != null)
			{
				Logic_RegisterName_OnCurrent(token.TargetContext, token.TargetContext.CurrentInstanceRegisteredName);
			}
			break;
		}
		if (token.Target.Property == XamlLanguage.Key)
		{
			ProcessNameFixup_UpdatePendingAddKey(token, targetContext.CurrentInstance);
		}
		else if (token.Target.Property == XamlLanguage.Items)
		{
			ProcessNameFixup_UpdatePendingAddItem(token, targetContext.CurrentInstance);
		}
		else if (token.Target.Property != null)
		{
			Logic_DoAssignmentToParentProperty(targetContext);
		}
		else
		{
			_lastInstance = targetContext.CurrentInstance;
		}
		if (targetContext.CurrentInstance is NameFixupToken nameFixupToken)
		{
			nameFixupToken.Target = token.Target;
			nameFixupToken.LineNumber = token.LineNumber;
			nameFixupToken.LinePosition = token.LinePosition;
			if (token.Target.Property == XamlLanguage.Key || token.Target.Property == XamlLanguage.Items)
			{
				_nameFixupGraph.AddDependency(nameFixupToken);
			}
		}
	}

	private void ProcessNameFixup_UpdatePendingAddKey(NameFixupToken token, object key)
	{
		if (token.Target.KeyHolder != null)
		{
			token.Target.KeyHolder.Key = key;
		}
		else if (token.Target.TemporaryCollectionIndex >= 0)
		{
			PendingCollectionAdd pendingCollectionAdd = PendingCollectionAdds[token.Target.Instance][token.Target.TemporaryCollectionIndex];
			pendingCollectionAdd.Key = key;
			pendingCollectionAdd.KeyIsSet = true;
		}
	}

	private void ProcessNameFixup_UpdatePendingAddItem(NameFixupToken token, object item)
	{
		PendingCollectionAdd pendingCollectionAdd = PendingCollectionAdds[token.Target.Instance][token.Target.TemporaryCollectionIndex];
		pendingCollectionAdd.Item = item;
		if (!(item is NameFixupToken))
		{
			pendingCollectionAdd.ItemType = ((item != null) ? GetXamlType(item.GetType()) : null);
		}
	}

	private void ExecutePendingAdds(XamlType instanceType, object instance)
	{
		if (_pendingCollectionAdds == null || !PendingCollectionAdds.TryGetValue(instance, out var value))
		{
			return;
		}
		foreach (PendingCollectionAdd item in value)
		{
			XamlType xamlType = item.ItemType ?? instanceType.ItemType;
			IAddLineInfo lineInfo = Runtime.LineInfo;
			Runtime.LineInfo = item;
			try
			{
				if (instanceType.IsDictionary)
				{
					if (!item.KeyIsSet)
					{
						item.Key = GetKeyFromInstance(item.Item, xamlType, item);
						item.KeyIsSet = true;
					}
					if (item.KeyIsUnconverted)
					{
						ObjectWriterContext objectWriterContext = PendingKeyConversionContexts[instance];
						objectWriterContext.PopScope();
						objectWriterContext.PushScope();
						objectWriterContext.CurrentType = xamlType;
						objectWriterContext.CurrentInstance = item.Item;
						objectWriterContext.CurrentKeyIsUnconverted = item.KeyIsUnconverted;
						Logic_AddToParentDictionary(objectWriterContext, item.Key, item.Item);
					}
					else
					{
						Runtime.AddToDictionary(instance, instanceType, item.Item, xamlType, item.Key);
					}
				}
				else
				{
					Runtime.Add(instance, instanceType, item.Item, item.ItemType);
				}
			}
			finally
			{
				Runtime.LineInfo = lineInfo;
			}
		}
		PendingCollectionAdds.Remove(instance);
		if (_pendingKeyConversionContexts != null && _pendingKeyConversionContexts.ContainsKey(instance))
		{
			_pendingKeyConversionContexts.Remove(instance);
		}
	}
}
