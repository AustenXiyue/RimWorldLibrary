using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Baml2006;
using System.Windows.Data;
using System.Windows.Diagnostics;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xaml;
using MS.Internal;
using MS.Internal.Xaml.Context;
using MS.Utility;

namespace System.Windows;

/// <summary>Enables the instantiation of a tree of <see cref="T:System.Windows.FrameworkElement" /> and/or <see cref="T:System.Windows.FrameworkContentElement" /> objects.</summary>
[ContentProperty("VisualTree")]
[Localizability(LocalizationCategory.NeverLocalize)]
public abstract class FrameworkTemplate : DispatcherObject, INameScope, ISealable, IHaveResources, IQueryAmbient
{
	private class Frame : MS.Internal.Xaml.Context.XamlFrame
	{
		public XamlType Type { get; set; }

		public XamlMember Property { get; set; }

		public bool InsideNameScope { get; set; }

		public object Instance { get; set; }

		public override void Reset()
		{
			Type = null;
			Property = null;
			Instance = null;
			if (InsideNameScope)
			{
				InsideNameScope = false;
			}
		}
	}

	internal class TemplateChildLoadedFlags
	{
		public bool HasLoadedChangedHandler;

		public bool HasUnloadedChangedHandler;
	}

	[Flags]
	private enum InternalFlags : uint
	{
		CanBuildVisualTree = 4u,
		HasLoadedChangeHandler = 8u,
		HasContainerResourceReferences = 0x10u,
		HasChildResourceReferences = 0x20u
	}

	private NameScope _nameScope = new NameScope();

	private MS.Internal.Xaml.Context.XamlContextStack<Frame> Names;

	private InternalFlags _flags;

	private bool _sealed;

	internal bool _hasInstanceValues;

	private ParserContext _parserContext;

	private IStyleConnector _styleConnector;

	private IComponentConnector _componentConnector;

	private FrameworkElementFactory _templateRoot;

	private TemplateContent _templateHolder;

	private bool _hasXamlNodeContent;

	private HybridDictionary _childIndexFromChildName = new HybridDictionary();

	private Dictionary<int, Type> _childTypeFromChildIndex = new Dictionary<int, Type>();

	private int _lastChildIndex = 1;

	private List<string> _childNames = new List<string>();

	internal ResourceDictionary _resources;

	internal HybridDictionary _triggerActions;

	internal FrugalStructList<ChildRecord> ChildRecordFromChildIndex;

	internal FrugalStructList<ItemStructMap<TriggerSourceRecord>> TriggerSourceRecordFromChildIndex;

	internal FrugalMap PropertyTriggersWithActions;

	internal FrugalStructList<ContainerDependent> ContainerDependents;

	internal FrugalStructList<ChildPropertyDependent> ResourceDependents;

	internal HybridDictionary _dataTriggerRecordFromBinding;

	internal HybridDictionary DataTriggersWithActions;

	internal ConditionalWeakTable<DependencyObject, List<DeferredAction>> DeferredActions;

	internal HybridDictionary _TemplateChildLoadedDictionary = new HybridDictionary();

	internal ItemStructList<ChildEventDependent> EventDependents = new ItemStructList<ChildEventDependent>(1);

	private EventHandlersStore _eventHandlersStore;

	private object[] _staticResourceValues;

	/// <summary>Gets a value that indicates whether this object is in an immutable state so it cannot be changed.</summary>
	/// <returns>true if this object is in an immutable state; otherwise, false.</returns>
	public bool IsSealed
	{
		get
		{
			VerifyAccess();
			return _sealed;
		}
	}

	/// <summary>Gets or sets the root node of the template.</summary>
	/// <returns>The root node of the template.</returns>
	public FrameworkElementFactory VisualTree
	{
		get
		{
			VerifyAccess();
			return _templateRoot;
		}
		set
		{
			VerifyAccess();
			CheckSealed();
			ValidateVisualTree(value);
			_templateRoot = value;
		}
	}

	/// <summary>Gets or sets a reference to the object that records or plays the XAML nodes for the template when the template is defined or applied by a writer.</summary>
	/// <returns>A reference to the object that records or plays the XAML nodes for the template.</returns>
	[Ambient]
	[DefaultValue(null)]
	public TemplateContent Template
	{
		get
		{
			return _templateHolder;
		}
		set
		{
			CheckSealed();
			if (!_hasXamlNodeContent)
			{
				value.OwnerTemplate = this;
				value.ParseXaml();
				_templateHolder = value;
				_hasXamlNodeContent = true;
				return;
			}
			throw new System.Windows.Markup.XamlParseException(SR.TemplateContentSetTwice);
		}
	}

	/// <summary>Gets or sets the collection of resources that can be used within the scope of this template.</summary>
	/// <returns>The resources that can be used within the scope of this template.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Ambient]
	public ResourceDictionary Resources
	{
		get
		{
			VerifyAccess();
			if (_resources == null)
			{
				_resources = new ResourceDictionary();
				_resources.CanBeAccessedAcrossThreads = true;
			}
			if (IsSealed)
			{
				_resources.IsReadOnly = true;
			}
			return _resources;
		}
		set
		{
			VerifyAccess();
			if (IsSealed)
			{
				throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "Template"));
			}
			_resources = value;
			if (_resources != null)
			{
				_resources.CanBeAccessedAcrossThreads = true;
			}
		}
	}

	ResourceDictionary IHaveResources.Resources
	{
		get
		{
			return Resources;
		}
		set
		{
			Resources = value;
		}
	}

	/// <summary>Gets a value that indicates whether this template has optimized content.</summary>
	/// <returns>true if this template has optimized content; otherwise, false.</returns>
	public bool HasContent
	{
		get
		{
			VerifyAccess();
			return _hasXamlNodeContent;
		}
	}

	internal bool CanBuildVisualTree
	{
		get
		{
			return ReadInternalFlag(InternalFlags.CanBuildVisualTree);
		}
		set
		{
			WriteInternalFlag(InternalFlags.CanBuildVisualTree, value);
		}
	}

	internal virtual Type TargetTypeInternal => null;

	internal virtual object DataTypeInternal => null;

	bool ISealable.CanSeal => true;

	bool ISealable.IsSealed => IsSealed;

	internal virtual TriggerCollection TriggersInternal => null;

	internal bool HasResourceReferences => ResourceDependents.Count > 0;

	internal bool HasContainerResourceReferences => ReadInternalFlag(InternalFlags.HasContainerResourceReferences);

	internal bool HasChildResourceReferences => ReadInternalFlag(InternalFlags.HasChildResourceReferences);

	internal bool HasEventDependents => EventDependents.Count > 0;

	internal bool HasInstanceValues => _hasInstanceValues;

	internal bool HasLoadedChangeHandler
	{
		get
		{
			return ReadInternalFlag(InternalFlags.HasLoadedChangeHandler);
		}
		set
		{
			WriteInternalFlag(InternalFlags.HasLoadedChangeHandler, value);
		}
	}

	internal ParserContext ParserContext => _parserContext;

	internal EventHandlersStore EventHandlersStore => _eventHandlersStore;

	internal IStyleConnector StyleConnector
	{
		get
		{
			return _styleConnector;
		}
		set
		{
			_styleConnector = value;
		}
	}

	internal IComponentConnector ComponentConnector
	{
		get
		{
			return _componentConnector;
		}
		set
		{
			_componentConnector = value;
		}
	}

	internal object[] StaticResourceValues
	{
		get
		{
			return _staticResourceValues;
		}
		set
		{
			_staticResourceValues = value;
		}
	}

	internal bool HasXamlNodeContent => _hasXamlNodeContent;

	internal HybridDictionary ChildIndexFromChildName => _childIndexFromChildName;

	internal Dictionary<int, Type> ChildTypeFromChildIndex => _childTypeFromChildIndex;

	internal int LastChildIndex
	{
		get
		{
			return _lastChildIndex;
		}
		set
		{
			_lastChildIndex = value;
		}
	}

	internal List<string> ChildNames => _childNames;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkTemplate" /> class. </summary>
	protected FrameworkTemplate()
	{
	}

	/// <summary>When overridden in a derived class, supplies rules for the element this template is applied to.</summary>
	/// <param name="templatedParent">The element this template is applied to.</param>
	protected virtual void ValidateTemplatedParent(FrameworkElement templatedParent)
	{
	}

	/// <summary>Returns a value that indicates whether serialization processes should serialize the value of the <see cref="P:System.Windows.FrameworkTemplate.VisualTree" /> property on instances of this class.</summary>
	/// <returns>true if the <see cref="P:System.Windows.FrameworkTemplate.VisualTree" /> property value should be serialized; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeVisualTree()
	{
		VerifyAccess();
		if (!HasContent)
		{
			return VisualTree != null;
		}
		return true;
	}

	internal object FindResource(object resourceKey, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference)
	{
		bool canCache;
		if (_resources != null && _resources.Contains(resourceKey))
		{
			return _resources.FetchResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference, out canCache);
		}
		return DependencyProperty.UnsetValue;
	}

	/// <summary>Queries whether a specified ambient property is available in the current scope.</summary>
	/// <returns>true if the requested ambient property is available; otherwise, false.</returns>
	/// <param name="propertyName">The name of the requested ambient property.</param>
	bool IQueryAmbient.IsAmbientPropertyAvailable(string propertyName)
	{
		if (propertyName == "Resources" && _resources == null)
		{
			return false;
		}
		if (propertyName == "Template" && !_hasXamlNodeContent)
		{
			return false;
		}
		return true;
	}

	/// <summary>Finds the element associated with the specified name defined within this template.</summary>
	/// <returns>The element associated with the specified name.</returns>
	/// <param name="name">The string name.</param>
	/// <param name="templatedParent">The context of the <see cref="T:System.Windows.FrameworkElement" /> where this template is applied.</param>
	public object FindName(string name, FrameworkElement templatedParent)
	{
		VerifyAccess();
		if (templatedParent == null)
		{
			throw new ArgumentNullException("templatedParent");
		}
		if (this != templatedParent.TemplateInternal)
		{
			throw new InvalidOperationException(SR.TemplateFindNameInInvalidElement);
		}
		return StyleHelper.FindNameInTemplateContent(templatedParent, name, this);
	}

	/// <summary>Registers a new name/object pair into the current name scope.</summary>
	/// <param name="name">The name to register.</param>
	/// <param name="scopedElement">The object to be mapped to the provided name.</param>
	public void RegisterName(string name, object scopedElement)
	{
		VerifyAccess();
		_nameScope.RegisterName(name, scopedElement);
	}

	/// <summary>Removes a name/object mapping from the XAML namescope.</summary>
	/// <param name="name">The name of the mapping to remove.</param>
	public void UnregisterName(string name)
	{
		VerifyAccess();
		_nameScope.UnregisterName(name);
	}

	/// <summary>Returns an object that has the provided identifying name. </summary>
	/// <returns>The object, if found. Returns null if no object of that name was found.</returns>
	/// <param name="name">The name identifier for the object being requested.</param>
	object INameScope.FindName(string name)
	{
		VerifyAccess();
		return _nameScope.FindName(name);
	}

	private void ValidateVisualTree(FrameworkElementFactory templateRoot)
	{
		if (templateRoot != null && typeof(FrameworkContentElement).IsAssignableFrom(templateRoot.Type))
		{
			throw new ArgumentException(SR.Format(SR.VisualTreeRootIsFrameworkElement, typeof(FrameworkElement).Name, templateRoot.Type.Name));
		}
	}

	internal virtual void ProcessTemplateBeforeSeal()
	{
	}

	/// <summary>Locks the template so it cannot be changed.</summary>
	public void Seal()
	{
		VerifyAccess();
		StyleHelper.SealTemplate(this, ref _sealed, _templateRoot, TriggersInternal, _resources, ChildIndexFromChildName, ref ChildRecordFromChildIndex, ref TriggerSourceRecordFromChildIndex, ref ContainerDependents, ref ResourceDependents, ref EventDependents, ref _triggerActions, ref _dataTriggerRecordFromBinding, ref _hasInstanceValues, ref _eventHandlersStore);
		if (_templateHolder != null)
		{
			_templateHolder.ResetTemplateLoadData();
		}
	}

	internal void CheckSealed()
	{
		if (_sealed)
		{
			throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, "Template"));
		}
	}

	internal void SetResourceReferenceState()
	{
		StyleHelper.SortResourceDependents(ref ResourceDependents);
		for (int i = 0; i < ResourceDependents.Count; i++)
		{
			if (ResourceDependents[i].ChildIndex == 0)
			{
				WriteInternalFlag(InternalFlags.HasContainerResourceReferences, set: true);
			}
			else
			{
				WriteInternalFlag(InternalFlags.HasChildResourceReferences, set: true);
			}
		}
	}

	internal bool ApplyTemplateContent(UncommonField<HybridDictionary[]> templateDataField, FrameworkElement container)
	{
		if (TraceDependencyProperty.IsEnabled)
		{
			TraceDependencyProperty.Trace(TraceEventType.Start, TraceDependencyProperty.ApplyTemplateContent, container, this);
		}
		ValidateTemplatedParent(container);
		bool result = StyleHelper.ApplyTemplateContent(templateDataField, container, _templateRoot, _lastChildIndex, ChildIndexFromChildName, this);
		if (TraceDependencyProperty.IsEnabled)
		{
			TraceDependencyProperty.Trace(TraceEventType.Stop, TraceDependencyProperty.ApplyTemplateContent, container, this);
		}
		return result;
	}

	/// <summary>Loads the content of the template as an instance of an object and returns the root element of the content.</summary>
	/// <returns>The root element of the content. Calling this multiple times returns separate instances.</returns>
	public DependencyObject LoadContent()
	{
		VerifyAccess();
		if (VisualTree != null)
		{
			return VisualTree.InstantiateUnoptimizedTree().DO;
		}
		return LoadContent(null, null);
	}

	internal DependencyObject LoadContent(DependencyObject container, List<DependencyObject> affectedChildren)
	{
		if (!HasContent)
		{
			return null;
		}
		lock (SystemResources.ThemeDictionaryLock)
		{
			return LoadOptimizedTemplateContent(container, null, _styleConnector, affectedChildren, null);
		}
	}

	internal static bool IsNameScope(XamlType type)
	{
		if (typeof(ResourceDictionary).IsAssignableFrom(type.UnderlyingType))
		{
			return true;
		}
		return type.IsNameScope;
	}

	internal virtual bool BuildVisualTree(FrameworkElement container)
	{
		return false;
	}

	/// <summary>Returns a value that indicates whether serialization processes should serialize the value of the <see cref="P:System.Windows.FrameworkTemplate.Resources" /> property on instances of this class.</summary>
	/// <returns>true if the <see cref="P:System.Windows.FrameworkTemplate.Resources" /> property value should be serialized; otherwise, false.</returns>
	/// <param name="manager">The <see cref="T:System.Windows.Markup.XamlDesignerSerializationManager" />.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeResources(XamlDesignerSerializationManager manager)
	{
		VerifyAccess();
		bool result = true;
		if (manager != null)
		{
			result = manager.XmlWriter == null;
		}
		return result;
	}

	private bool ReadInternalFlag(InternalFlags reqFlag)
	{
		return (_flags & reqFlag) != 0;
	}

	private void WriteInternalFlag(InternalFlags reqFlag, bool set)
	{
		if (set)
		{
			_flags |= reqFlag;
		}
		else
		{
			_flags &= ~reqFlag;
		}
	}

	private bool ReceivePropertySet(object targetObject, XamlMember member, object value, DependencyObject templatedParent)
	{
		DependencyObject dependencyObject = targetObject as DependencyObject;
		WpfXamlMember wpfXamlMember = member as WpfXamlMember;
		if (wpfXamlMember != null)
		{
			DependencyProperty dependencyProperty = wpfXamlMember.DependencyProperty;
			if (dependencyProperty == null || dependencyObject == null)
			{
				return false;
			}
			FrameworkObject frameworkObject = new FrameworkObject(dependencyObject);
			if (frameworkObject.TemplatedParent == null || templatedParent == null)
			{
				return false;
			}
			if (dependencyProperty == BaseUriHelper.BaseUriProperty)
			{
				if (!frameworkObject.IsInitialized)
				{
					return true;
				}
			}
			else if (dependencyProperty == UIElement.UidProperty)
			{
				return true;
			}
			HybridDictionary hybridDictionary;
			if (!frameworkObject.StoresParentTemplateValues)
			{
				hybridDictionary = new HybridDictionary();
				StyleHelper.ParentTemplateValuesField.SetValue(dependencyObject, hybridDictionary);
				frameworkObject.StoresParentTemplateValues = true;
			}
			else
			{
				hybridDictionary = StyleHelper.ParentTemplateValuesField.GetValue(dependencyObject);
			}
			int templateChildIndex = frameworkObject.TemplateChildIndex;
			if (value is Expression expression)
			{
				if (expression is BindingExpressionBase bindingExpressionBase)
				{
					HybridDictionary instanceValues = StyleHelper.EnsureInstanceData(StyleHelper.TemplateDataField, templatedParent, InstanceStyleData.InstanceValues);
					StyleHelper.ProcessInstanceValue(dependencyObject, templateChildIndex, instanceValues, dependencyProperty, -1, apply: true);
					value = bindingExpressionBase.ParentBindingBase;
				}
				else if (expression is TemplateBindingExpression { TemplateBindingExtension: var templateBindingExtension })
				{
					HybridDictionary instanceValues2 = StyleHelper.EnsureInstanceData(StyleHelper.TemplateDataField, templatedParent, InstanceStyleData.InstanceValues);
					StyleHelper.ProcessInstanceValue(dependencyObject, templateChildIndex, instanceValues2, dependencyProperty, -1, apply: true);
					value = new Binding
					{
						Mode = BindingMode.OneWay,
						RelativeSource = RelativeSource.TemplatedParent,
						Path = new PropertyPath(templateBindingExtension.Property),
						Converter = templateBindingExtension.Converter,
						ConverterParameter = templateBindingExtension.ConverterParameter
					};
				}
			}
			bool flag = value is MarkupExtension;
			if (!dependencyProperty.IsValidValue(value) && !flag && !(value is DeferredReference))
			{
				throw new ArgumentException(SR.Format(SR.InvalidPropertyValue, value, dependencyProperty.Name));
			}
			hybridDictionary[dependencyProperty] = value;
			dependencyObject.ProvideSelfAsInheritanceContext(value, dependencyProperty);
			EffectiveValueEntry entry = new EffectiveValueEntry(dependencyProperty);
			entry.BaseValueSourceInternal = BaseValueSourceInternal.ParentTemplate;
			entry.Value = value;
			if (flag)
			{
				StyleHelper.GetInstanceValue(StyleHelper.TemplateDataField, templatedParent, frameworkObject.FE, frameworkObject.FCE, templateChildIndex, dependencyProperty, -1, ref entry);
			}
			dependencyObject.UpdateEffectiveValue(dependencyObject.LookupEntry(dependencyProperty.GlobalIndex), dependencyProperty, dependencyProperty.GetMetadata(dependencyObject.DependencyObjectType), default(EffectiveValueEntry), ref entry, coerceWithDeferredReference: false, coerceWithCurrentValue: false, OperationType.Unknown);
			return true;
		}
		return false;
	}

	private DependencyObject LoadOptimizedTemplateContent(DependencyObject container, IComponentConnector componentConnector, IStyleConnector styleConnector, List<DependencyObject> affectedChildren, UncommonField<Hashtable> templatedNonFeChildrenField)
	{
		if (Names == null)
		{
			Names = new MS.Internal.Xaml.Context.XamlContextStack<Frame>(() => new Frame());
		}
		DependencyObject rootObject = null;
		if (TraceMarkup.IsEnabled)
		{
			TraceMarkup.Trace(TraceEventType.Start, TraceMarkup.Load);
		}
		FrameworkElement feContainer = container as FrameworkElement;
		_ = feContainer;
		TemplateNameScope nameScope = new TemplateNameScope(container, affectedChildren, this);
		XamlObjectWriterSettings xamlObjectWriterSettings = System.Windows.Markup.XamlReader.CreateObjectWriterSettings(_templateHolder.ObjectWriterParentSettings);
		xamlObjectWriterSettings.ExternalNameScope = nameScope;
		xamlObjectWriterSettings.RegisterNamesOnExternalNamescope = true;
		IEnumerator<string> nameEnumerator = ChildNames.GetEnumerator();
		xamlObjectWriterSettings.AfterBeginInitHandler = delegate(object sender, XamlObjectEventArgs args)
		{
			HandleAfterBeginInit(args.Instance, ref rootObject, container, feContainer, nameScope, nameEnumerator);
			if (XamlSourceInfoHelper.IsXamlSourceInfoEnabled)
			{
				XamlSourceInfoHelper.SetXamlSourceInfo(args.Instance, args, null);
			}
		};
		xamlObjectWriterSettings.BeforePropertiesHandler = delegate(object sender, XamlObjectEventArgs args)
		{
			HandleBeforeProperties(args.Instance, ref rootObject, container, feContainer, nameScope);
		};
		xamlObjectWriterSettings.XamlSetValueHandler = delegate(object sender, XamlSetValueEventArgs setArgs)
		{
			setArgs.Handled = ReceivePropertySet(sender, setArgs.Member, setArgs.Value, container);
		};
		XamlObjectWriter xamlObjectWriter = _templateHolder.ObjectWriterFactory.GetXamlObjectWriter(xamlObjectWriterSettings);
		try
		{
			LoadTemplateXaml(xamlObjectWriter);
		}
		finally
		{
			if (TraceMarkup.IsEnabled)
			{
				TraceMarkup.Trace(TraceEventType.Stop, TraceMarkup.Load, rootObject);
			}
		}
		return rootObject;
	}

	private void LoadTemplateXaml(XamlObjectWriter objectWriter)
	{
		System.Xaml.XamlReader templateReader = _templateHolder.PlayXaml();
		LoadTemplateXaml(templateReader, objectWriter);
	}

	private void LoadTemplateXaml(System.Xaml.XamlReader templateReader, XamlObjectWriter currentWriter)
	{
		try
		{
			int num = 0;
			IXamlLineInfoConsumer xamlLineInfoConsumer = null;
			IXamlLineInfo xamlLineInfo = null;
			if (XamlSourceInfoHelper.IsXamlSourceInfoEnabled)
			{
				xamlLineInfo = templateReader as IXamlLineInfo;
				if (xamlLineInfo != null)
				{
					xamlLineInfoConsumer = currentWriter;
				}
			}
			while (templateReader.Read())
			{
				xamlLineInfoConsumer?.SetLineInfo(xamlLineInfo.LineNumber, xamlLineInfo.LinePosition);
				currentWriter.WriteNode(templateReader);
				switch (templateReader.NodeType)
				{
				case System.Xaml.XamlNodeType.StartObject:
				{
					bool num3 = Names.Depth > 0 && (IsNameScope(Names.CurrentFrame.Type) || Names.CurrentFrame.InsideNameScope);
					Names.PushScope();
					Names.CurrentFrame.Type = templateReader.Type;
					if (num3)
					{
						Names.CurrentFrame.InsideNameScope = true;
					}
					break;
				}
				case System.Xaml.XamlNodeType.GetObject:
				{
					bool num2 = IsNameScope(Names.CurrentFrame.Type) || Names.CurrentFrame.InsideNameScope;
					Names.PushScope();
					Names.CurrentFrame.Type = Names.PreviousFrame.Property.Type;
					if (num2)
					{
						Names.CurrentFrame.InsideNameScope = true;
					}
					break;
				}
				case System.Xaml.XamlNodeType.StartMember:
					Names.CurrentFrame.Property = templateReader.Member;
					if (templateReader.Member.DeferringLoader != null)
					{
						num++;
					}
					break;
				case System.Xaml.XamlNodeType.EndMember:
					if (Names.CurrentFrame.Property.DeferringLoader != null)
					{
						num--;
					}
					Names.CurrentFrame.Property = null;
					break;
				case System.Xaml.XamlNodeType.EndObject:
					Names.PopScope();
					break;
				case System.Xaml.XamlNodeType.Value:
					if (num == 0 && Names.CurrentFrame.Property == XamlLanguage.ConnectionId && _styleConnector != null)
					{
						_styleConnector.Connect((int)templateReader.Value, Names.CurrentFrame.Instance);
					}
					break;
				}
			}
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || ex is System.Windows.Markup.XamlParseException)
			{
				throw;
			}
			System.Windows.Markup.XamlReader.RewrapException(ex, null);
		}
	}

	internal static bool IsNameProperty(XamlMember member, XamlType owner)
	{
		if (member == owner.GetAliasedProperty(XamlLanguage.Name) || XamlLanguage.Name == member)
		{
			return true;
		}
		return false;
	}

	private void HandleAfterBeginInit(object createdObject, ref DependencyObject rootObject, DependencyObject container, FrameworkElement feContainer, TemplateNameScope nameScope, IEnumerator<string> nameEnumerator)
	{
		if (!Names.CurrentFrame.InsideNameScope && (createdObject is FrameworkElement || createdObject is FrameworkContentElement))
		{
			nameEnumerator.MoveNext();
			nameScope.RegisterNameInternal(nameEnumerator.Current, createdObject);
		}
		Names.CurrentFrame.Instance = createdObject;
	}

	private void HandleBeforeProperties(object createdObject, ref DependencyObject rootObject, DependencyObject container, FrameworkElement feContainer, INameScope nameScope)
	{
		if (createdObject is FrameworkElement || createdObject is FrameworkContentElement)
		{
			if (rootObject == null)
			{
				rootObject = WireRootObjectToParent(createdObject, rootObject, container, feContainer, nameScope);
			}
			InvalidatePropertiesOnTemplate(container, createdObject);
		}
	}

	private static DependencyObject WireRootObjectToParent(object createdObject, DependencyObject rootObject, DependencyObject container, FrameworkElement feContainer, INameScope nameScope)
	{
		rootObject = createdObject as DependencyObject;
		if (rootObject != null)
		{
			if (feContainer != null)
			{
				if (!(rootObject is UIElement templateChild))
				{
					throw new InvalidOperationException(SR.Format(SR.TemplateMustBeFE, new object[1] { rootObject.GetType().FullName }));
				}
				feContainer.TemplateChild = templateChild;
			}
			else if (container != null)
			{
				Helper.DowncastToFEorFCE(rootObject, out var fe, out var fce, throwIfNeither: true);
				FrameworkElementFactory.AddNodeToLogicalTree((FrameworkContentElement)container, rootObject.GetType(), fe != null, fe, fce);
			}
			if (NameScope.GetNameScope(rootObject) == null)
			{
				NameScope.SetNameScope(rootObject, nameScope);
			}
		}
		return rootObject;
	}

	private void InvalidatePropertiesOnTemplate(DependencyObject container, object currentObject)
	{
		if (container == null || !(currentObject is DependencyObject d))
		{
			return;
		}
		FrameworkObject child = new FrameworkObject(d);
		if (child.IsValid)
		{
			int templateChildIndex = child.TemplateChildIndex;
			if (StyleHelper.HasResourceDependentsForChild(templateChildIndex, ref ResourceDependents))
			{
				child.HasResourceReference = true;
			}
			StyleHelper.InvalidatePropertiesOnTemplateNode(container, child, templateChildIndex, ref ChildRecordFromChildIndex, isDetach: false, VisualTree);
		}
	}

	internal static void SetTemplateParentValues(string name, object element, FrameworkTemplate frameworkTemplate, ref ProvideValueServiceProvider provideValueServiceProvider)
	{
		if (!frameworkTemplate.IsSealed)
		{
			frameworkTemplate.Seal();
		}
		HybridDictionary childIndexFromChildName = frameworkTemplate.ChildIndexFromChildName;
		FrugalStructList<ChildRecord> childRecordFromChildIndex = frameworkTemplate.ChildRecordFromChildIndex;
		int num = StyleHelper.QueryChildIndexFromChildName(name, childIndexFromChildName);
		if (num >= childRecordFromChildIndex.Count)
		{
			return;
		}
		ChildRecord childRecord = childRecordFromChildIndex[num];
		for (int i = 0; i < childRecord.ValueLookupListFromProperty.Count; i++)
		{
			for (int j = 0; j < childRecord.ValueLookupListFromProperty.Entries[i].Value.Count; j++)
			{
				ChildValueLookup childValueLookup = childRecord.ValueLookupListFromProperty.Entries[i].Value.List[j];
				if (childValueLookup.LookupType != 0 && childValueLookup.LookupType != ValueLookupType.Resource && childValueLookup.LookupType != ValueLookupType.TemplateBinding)
				{
					continue;
				}
				object obj = childValueLookup.Value;
				if (childValueLookup.LookupType == ValueLookupType.TemplateBinding)
				{
					obj = new TemplateBindingExpression(obj as TemplateBindingExtension);
				}
				else if (childValueLookup.LookupType == ValueLookupType.Resource)
				{
					obj = new ResourceReferenceExpression(obj);
				}
				if (obj is MarkupExtension markupExtension)
				{
					if (provideValueServiceProvider == null)
					{
						provideValueServiceProvider = new ProvideValueServiceProvider();
					}
					provideValueServiceProvider.SetData(element, childValueLookup.Property);
					obj = markupExtension.ProvideValue(provideValueServiceProvider);
					provideValueServiceProvider.ClearData();
				}
				(element as DependencyObject).SetValue(childValueLookup.Property, obj);
			}
		}
	}

	internal abstract void SetTargetTypeInternal(Type targetType);

	void ISealable.Seal()
	{
		Seal();
	}

	internal void CopyParserContext(ParserContext parserContext)
	{
		_parserContext = parserContext.ScopedCopy(copyNameScopeStack: false);
		_parserContext.SkipJournaledProperties = false;
	}
}
