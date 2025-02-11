using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Baml2006;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Diagnostics;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using MS.Internal.Xaml.Context;
using MS.Utility;

namespace System.Windows;

/// <summary>Implements the record and playback logic that templates use for deferring content when they interact with XAML readers and writers.</summary>
[XamlDeferLoad(typeof(TemplateContentLoader), typeof(FrameworkElement))]
public class TemplateContent
{
	internal class Frame : MS.Internal.Xaml.Context.XamlFrame
	{
		private FrugalObjectList<NamespaceDeclaration> _namespaces;

		private XamlType _xamlType;

		public XamlType Type
		{
			get
			{
				return _xamlType;
			}
			set
			{
				_xamlType = value;
			}
		}

		public XamlMember Property { get; set; }

		public string Name { get; set; }

		public bool NameSet { get; set; }

		public bool IsInNameScope { get; set; }

		public bool IsInStyleOrTemplate { get; set; }

		public object Instance { get; set; }

		public bool ContentSet { get; set; }

		public bool ContentSourceSet { get; set; }

		public string ContentSource { get; set; }

		public bool ContentTemplateSet { get; set; }

		public bool ContentTemplateSelectorSet { get; set; }

		public bool ContentStringFormatSet { get; set; }

		public bool ColumnsSet { get; set; }

		public FrugalObjectList<NamespaceDeclaration> Namespaces
		{
			get
			{
				if (_namespaces == null)
				{
					_namespaces = new FrugalObjectList<NamespaceDeclaration>();
				}
				return _namespaces;
			}
		}

		public bool HasNamespaces
		{
			get
			{
				if (_namespaces != null)
				{
					return _namespaces.Count > 0;
				}
				return false;
			}
		}

		public override void Reset()
		{
			_xamlType = null;
			Property = null;
			Name = null;
			NameSet = false;
			IsInNameScope = false;
			Instance = null;
			ContentSet = false;
			ContentSourceSet = false;
			ContentSource = null;
			ContentTemplateSet = false;
			ContentTemplateSelectorSet = false;
			ContentStringFormatSet = false;
			IsInNameScope = false;
			if (HasNamespaces)
			{
				_namespaces = null;
			}
		}

		public override string ToString()
		{
			string arg = ((Type == null) ? string.Empty : Type.Name);
			string arg2 = ((Property == null) ? "-" : Property.Name);
			string arg3 = ((Instance == null) ? "-" : "*");
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1} inst={2}", arg, arg2, arg3);
		}
	}

	internal class StackOfFrames : MS.Internal.Xaml.Context.XamlContextStack<Frame>
	{
		public FrugalObjectList<NamespaceDeclaration> InScopeNamespaces
		{
			get
			{
				FrugalObjectList<NamespaceDeclaration> frugalObjectList = null;
				for (Frame frame = base.CurrentFrame; frame != null; frame = (Frame)frame.Previous)
				{
					if (frame.HasNamespaces)
					{
						if (frugalObjectList == null)
						{
							frugalObjectList = new FrugalObjectList<NamespaceDeclaration>();
						}
						for (int i = 0; i < frame.Namespaces.Count; i++)
						{
							frugalObjectList.Add(frame.Namespaces[i]);
						}
					}
				}
				return frugalObjectList;
			}
		}

		public StackOfFrames()
			: base((Func<Frame>)(() => new Frame()))
		{
		}

		public void Push(XamlType xamlType, string name)
		{
			bool isInNameScope = false;
			bool isInStyleOrTemplate = false;
			if (base.Depth > 0)
			{
				isInNameScope = base.CurrentFrame.IsInNameScope || (base.CurrentFrame.Type != null && FrameworkTemplate.IsNameScope(base.CurrentFrame.Type));
				isInStyleOrTemplate = base.CurrentFrame.IsInStyleOrTemplate || (base.CurrentFrame.Type != null && (typeof(FrameworkTemplate).IsAssignableFrom(base.CurrentFrame.Type.UnderlyingType) || typeof(Style).IsAssignableFrom(base.CurrentFrame.Type.UnderlyingType)));
			}
			if (base.Depth == 0 || base.CurrentFrame.Type != null)
			{
				PushScope();
			}
			base.CurrentFrame.Type = xamlType;
			base.CurrentFrame.Name = name;
			base.CurrentFrame.IsInNameScope = isInNameScope;
			base.CurrentFrame.IsInStyleOrTemplate = isInStyleOrTemplate;
		}

		public void AddNamespace(NamespaceDeclaration nsd)
		{
			bool isInNameScope = false;
			bool isInStyleOrTemplate = false;
			if (base.Depth > 0)
			{
				isInNameScope = base.CurrentFrame.IsInNameScope || (base.CurrentFrame.Type != null && FrameworkTemplate.IsNameScope(base.CurrentFrame.Type));
				isInStyleOrTemplate = base.CurrentFrame.IsInStyleOrTemplate || (base.CurrentFrame.Type != null && (typeof(FrameworkTemplate).IsAssignableFrom(base.CurrentFrame.Type.UnderlyingType) || typeof(Style).IsAssignableFrom(base.CurrentFrame.Type.UnderlyingType)));
			}
			if (base.Depth == 0 || base.CurrentFrame.Type != null)
			{
				PushScope();
			}
			base.CurrentFrame.Namespaces.Add(nsd);
			base.CurrentFrame.IsInNameScope = isInNameScope;
			base.CurrentFrame.IsInStyleOrTemplate = isInStyleOrTemplate;
		}
	}

	internal class ServiceProviderWrapper : ITypeDescriptorContext, IServiceProvider, IXamlTypeResolver, IXamlNamespaceResolver, IProvideValueTarget
	{
		private IServiceProvider _services;

		private XamlSchemaContext _schemaContext;

		private object _targetObject;

		private object _targetProperty;

		internal StackOfFrames Frames { get; set; }

		IContainer ITypeDescriptorContext.Container => null;

		object ITypeDescriptorContext.Instance => null;

		PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor => null;

		object IProvideValueTarget.TargetObject => _targetObject;

		object IProvideValueTarget.TargetProperty => _targetProperty;

		public ServiceProviderWrapper(IServiceProvider services, XamlSchemaContext schemaContext)
		{
			_services = services;
			_schemaContext = schemaContext;
		}

		object IServiceProvider.GetService(Type serviceType)
		{
			if (serviceType == typeof(IXamlTypeResolver))
			{
				return this;
			}
			if (serviceType == typeof(IProvideValueTarget))
			{
				return this;
			}
			return _services.GetService(serviceType);
		}

		Type IXamlTypeResolver.Resolve(string qualifiedTypeName)
		{
			return _schemaContext.GetXamlType(XamlTypeName.Parse(qualifiedTypeName, this)).UnderlyingType;
		}

		string IXamlNamespaceResolver.GetNamespace(string prefix)
		{
			FrugalObjectList<NamespaceDeclaration> inScopeNamespaces = Frames.InScopeNamespaces;
			if (inScopeNamespaces != null)
			{
				for (int i = 0; i < inScopeNamespaces.Count; i++)
				{
					if (inScopeNamespaces[i].Prefix == prefix)
					{
						return inScopeNamespaces[i].Namespace;
					}
				}
			}
			return ((IXamlNamespaceResolver)_services.GetService(typeof(IXamlNamespaceResolver))).GetNamespace(prefix);
		}

		IEnumerable<NamespaceDeclaration> IXamlNamespaceResolver.GetNamespacePrefixes()
		{
			throw new NotImplementedException();
		}

		void ITypeDescriptorContext.OnComponentChanged()
		{
		}

		bool ITypeDescriptorContext.OnComponentChanging()
		{
			return false;
		}

		public void SetData(object targetObject, object targetProperty)
		{
			_targetObject = targetObject;
			_targetProperty = targetProperty;
		}

		public void Clear()
		{
			_targetObject = null;
			_targetProperty = null;
		}
	}

	internal XamlNodeList _xamlNodeList;

	private static SharedDp _sharedDpInstance = new SharedDp(null, null, null);

	internal XamlType RootType { get; private set; }

	internal FrameworkTemplate OwnerTemplate { get; set; }

	internal IXamlObjectWriterFactory ObjectWriterFactory { get; private set; }

	internal XamlObjectWriterSettings ObjectWriterParentSettings { get; private set; }

	internal XamlSchemaContext SchemaContext { get; private set; }

	private StackOfFrames Stack
	{
		get
		{
			return TemplateLoadData.Stack;
		}
		set
		{
			TemplateLoadData.Stack = value;
		}
	}

	internal TemplateLoadData TemplateLoadData { get; set; }

	internal TemplateContent(System.Xaml.XamlReader xamlReader, IXamlObjectWriterFactory factory, IServiceProvider context)
	{
		TemplateLoadData = new TemplateLoadData();
		ObjectWriterFactory = factory;
		SchemaContext = xamlReader.SchemaContext;
		ObjectWriterParentSettings = factory.GetParentSettings();
		_ = ObjectWriterParentSettings.AccessLevel;
		TemplateLoadData.Reader = xamlReader;
		Initialize(context);
	}

	private void Initialize(IServiceProvider context)
	{
		XamlObjectWriterSettings xamlObjectWriterSettings = System.Windows.Markup.XamlReader.CreateObjectWriterSettings(ObjectWriterParentSettings);
		xamlObjectWriterSettings.AfterBeginInitHandler = delegate(object sender, XamlObjectEventArgs args)
		{
			if (Stack != null && Stack.Depth > 0)
			{
				Stack.CurrentFrame.Instance = args.Instance;
			}
		};
		xamlObjectWriterSettings.SkipProvideValueOnRoot = true;
		TemplateLoadData.ObjectWriter = ObjectWriterFactory.GetXamlObjectWriter(xamlObjectWriterSettings);
		TemplateLoadData.ServiceProviderWrapper = new ServiceProviderWrapper(context, SchemaContext);
		if (context.GetService(typeof(IRootObjectProvider)) is IRootObjectProvider rootObjectProvider)
		{
			TemplateLoadData.RootObject = rootObjectProvider.RootObject;
		}
	}

	internal void ParseXaml()
	{
		StackOfFrames stackOfFrames = new StackOfFrames();
		TemplateLoadData.ServiceProviderWrapper.Frames = stackOfFrames;
		OwnerTemplate.StyleConnector = TemplateLoadData.RootObject as IStyleConnector;
		TemplateLoadData.RootObject = null;
		List<PropertyValue> list = new List<PropertyValue>();
		int nameNumber = 1;
		ParseTree(stackOfFrames, list, ref nameNumber);
		if (OwnerTemplate is ItemsPanelTemplate)
		{
			PropertyValue item = default(PropertyValue);
			item.ValueType = PropertyValueType.Set;
			item.ChildName = TemplateLoadData.RootName;
			item.ValueInternal = true;
			item.Property = Panel.IsItemsHostProperty;
			list.Add(item);
		}
		for (int i = 0; i < list.Count; i++)
		{
			PropertyValue propertyValue = list[i];
			if (propertyValue.ValueInternal is TemplateBindingExtension)
			{
				propertyValue.ValueType = PropertyValueType.TemplateBinding;
			}
			else if (propertyValue.ValueInternal is DynamicResourceExtension)
			{
				DynamicResourceExtension dynamicResourceExtension = propertyValue.Value as DynamicResourceExtension;
				propertyValue.ValueType = PropertyValueType.Resource;
				propertyValue.ValueInternal = dynamicResourceExtension.ResourceKey;
			}
			else
			{
				StyleHelper.SealIfSealable(propertyValue.ValueInternal);
			}
			StyleHelper.UpdateTables(ref propertyValue, ref OwnerTemplate.ChildRecordFromChildIndex, ref OwnerTemplate.TriggerSourceRecordFromChildIndex, ref OwnerTemplate.ResourceDependents, ref OwnerTemplate._dataTriggerRecordFromBinding, OwnerTemplate.ChildIndexFromChildName, ref OwnerTemplate._hasInstanceValues);
		}
		TemplateLoadData.ObjectWriter = null;
	}

	internal System.Xaml.XamlReader PlayXaml()
	{
		return _xamlNodeList.GetReader();
	}

	internal void ResetTemplateLoadData()
	{
		TemplateLoadData = null;
	}

	private void UpdateSharedPropertyNames(string name, List<PropertyValue> sharedProperties, XamlType type)
	{
		int key = StyleHelper.CreateChildIndexFromChildName(name, OwnerTemplate);
		OwnerTemplate.ChildNames.Add(name);
		OwnerTemplate.ChildTypeFromChildIndex.Add(key, type.UnderlyingType);
		int num = sharedProperties.Count - 1;
		while (num >= 0)
		{
			PropertyValue value = sharedProperties[num];
			if (value.ChildName == null)
			{
				value.ChildName = name;
				sharedProperties[num] = value;
				num--;
				continue;
			}
			break;
		}
	}

	private void ParseTree(StackOfFrames stack, List<PropertyValue> sharedProperties, ref int nameNumber)
	{
		ParseNodes(stack, sharedProperties, ref nameNumber);
	}

	private void ParseNodes(StackOfFrames stack, List<PropertyValue> sharedProperties, ref int nameNumber)
	{
		_xamlNodeList = new XamlNodeList(SchemaContext);
		System.Xaml.XamlWriter writer = _xamlNodeList.Writer;
		System.Xaml.XamlReader reader = TemplateLoadData.Reader;
		IXamlLineInfoConsumer xamlLineInfoConsumer = null;
		IXamlLineInfo xamlLineInfo = null;
		if (XamlSourceInfoHelper.IsXamlSourceInfoEnabled)
		{
			xamlLineInfo = reader as IXamlLineInfo;
			if (xamlLineInfo != null)
			{
				xamlLineInfoConsumer = writer as IXamlLineInfoConsumer;
			}
		}
		while (reader.Read())
		{
			xamlLineInfoConsumer?.SetLineInfo(xamlLineInfo.LineNumber, xamlLineInfo.LinePosition);
			if (ParseNode(reader, stack, sharedProperties, ref nameNumber, out var newValue))
			{
				if (newValue == DependencyProperty.UnsetValue)
				{
					writer.WriteNode(reader);
				}
				else
				{
					writer.WriteValue(newValue);
				}
			}
		}
		writer.Close();
		TemplateLoadData.Reader = null;
	}

	private bool ParseNode(System.Xaml.XamlReader xamlReader, StackOfFrames stack, List<PropertyValue> sharedProperties, ref int nameNumber, out object newValue)
	{
		newValue = DependencyProperty.UnsetValue;
		switch (xamlReader.NodeType)
		{
		case System.Xaml.XamlNodeType.StartObject:
			if (xamlReader.Type.UnderlyingType == typeof(StaticResourceExtension))
			{
				XamlObjectWriter objectWriter = TemplateLoadData.ObjectWriter;
				objectWriter.Clear();
				WriteNamespaces(objectWriter, stack.InScopeNamespaces, null);
				newValue = LoadTimeBindUnshareableStaticResource(xamlReader, objectWriter);
				return true;
			}
			if (stack.Depth > 0 && !stack.CurrentFrame.NameSet && stack.CurrentFrame.Type != null && !stack.CurrentFrame.IsInNameScope && !stack.CurrentFrame.IsInStyleOrTemplate)
			{
				if (typeof(FrameworkElement).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType) || typeof(FrameworkContentElement).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType))
				{
					string name3 = nameNumber++.ToString(CultureInfo.InvariantCulture) + "_T";
					UpdateSharedPropertyNames(name3, sharedProperties, stack.CurrentFrame.Type);
					stack.CurrentFrame.Name = name3;
				}
				stack.CurrentFrame.NameSet = true;
			}
			if (RootType == null)
			{
				RootType = xamlReader.Type;
			}
			stack.Push(xamlReader.Type, null);
			break;
		case System.Xaml.XamlNodeType.GetObject:
		{
			if (stack.Depth > 0 && !stack.CurrentFrame.NameSet && stack.CurrentFrame.Type != null && !stack.CurrentFrame.IsInNameScope && !stack.CurrentFrame.IsInStyleOrTemplate)
			{
				if (typeof(FrameworkElement).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType) || typeof(FrameworkContentElement).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType))
				{
					string name4 = nameNumber++.ToString(CultureInfo.InvariantCulture) + "_T";
					UpdateSharedPropertyNames(name4, sharedProperties, stack.CurrentFrame.Type);
					stack.CurrentFrame.Name = name4;
				}
				stack.CurrentFrame.NameSet = true;
			}
			XamlType type = stack.CurrentFrame.Property.Type;
			if (RootType == null)
			{
				RootType = type;
			}
			stack.Push(type, null);
			break;
		}
		case System.Xaml.XamlNodeType.EndObject:
			if (!stack.CurrentFrame.IsInStyleOrTemplate)
			{
				if (!stack.CurrentFrame.NameSet && !stack.CurrentFrame.IsInNameScope)
				{
					if (typeof(FrameworkElement).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType) || typeof(FrameworkContentElement).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType))
					{
						string name2 = nameNumber++.ToString(CultureInfo.InvariantCulture) + "_T";
						UpdateSharedPropertyNames(name2, sharedProperties, stack.CurrentFrame.Type);
						stack.CurrentFrame.Name = name2;
					}
					stack.CurrentFrame.NameSet = true;
				}
				if (TemplateLoadData.RootName == null && stack.Depth == 1)
				{
					TemplateLoadData.RootName = stack.CurrentFrame.Name;
				}
				if (typeof(ContentPresenter).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType))
				{
					AutoAliasContentPresenter(OwnerTemplate.TargetTypeInternal, stack.CurrentFrame.ContentSource, stack.CurrentFrame.Name, ref OwnerTemplate.ChildRecordFromChildIndex, ref OwnerTemplate.TriggerSourceRecordFromChildIndex, ref OwnerTemplate.ResourceDependents, ref OwnerTemplate._dataTriggerRecordFromBinding, ref OwnerTemplate._hasInstanceValues, OwnerTemplate.ChildIndexFromChildName, stack.CurrentFrame.ContentSet, stack.CurrentFrame.ContentSourceSet, stack.CurrentFrame.ContentTemplateSet, stack.CurrentFrame.ContentTemplateSelectorSet, stack.CurrentFrame.ContentStringFormatSet);
				}
				if (typeof(GridViewRowPresenter).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType))
				{
					AutoAliasGridViewRowPresenter(OwnerTemplate.TargetTypeInternal, stack.CurrentFrame.ContentSource, stack.CurrentFrame.Name, ref OwnerTemplate.ChildRecordFromChildIndex, ref OwnerTemplate.TriggerSourceRecordFromChildIndex, ref OwnerTemplate.ResourceDependents, ref OwnerTemplate._dataTriggerRecordFromBinding, ref OwnerTemplate._hasInstanceValues, OwnerTemplate.ChildIndexFromChildName, stack.CurrentFrame.ContentSet, stack.CurrentFrame.ColumnsSet);
				}
			}
			stack.PopScope();
			break;
		case System.Xaml.XamlNodeType.StartMember:
		{
			stack.CurrentFrame.Property = xamlReader.Member;
			if (stack.CurrentFrame.IsInStyleOrTemplate)
			{
				break;
			}
			if (typeof(GridViewRowPresenter).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType))
			{
				if (xamlReader.Member.Name == "Content")
				{
					stack.CurrentFrame.ContentSet = true;
				}
				else if (xamlReader.Member.Name == "Columns")
				{
					stack.CurrentFrame.ColumnsSet = true;
				}
			}
			else if (typeof(ContentPresenter).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType))
			{
				if (xamlReader.Member.Name == "Content")
				{
					stack.CurrentFrame.ContentSet = true;
				}
				else if (xamlReader.Member.Name == "ContentTemplate")
				{
					stack.CurrentFrame.ContentTemplateSet = true;
				}
				else if (xamlReader.Member.Name == "ContentTemplateSelector")
				{
					stack.CurrentFrame.ContentTemplateSelectorSet = true;
				}
				else if (xamlReader.Member.Name == "ContentStringFormat")
				{
					stack.CurrentFrame.ContentStringFormatSet = true;
				}
				else if (xamlReader.Member.Name == "ContentSource")
				{
					stack.CurrentFrame.ContentSourceSet = true;
				}
			}
			if (stack.CurrentFrame.IsInNameScope || xamlReader.Member.IsDirective)
			{
				break;
			}
			IXamlIndexingReader xamlIndexingReader = xamlReader as IXamlIndexingReader;
			bool flag = false;
			int currentIndex = xamlIndexingReader.CurrentIndex;
			PropertyValue? sharedValue;
			try
			{
				flag = TrySharingProperty(xamlReader, stack.CurrentFrame.Type, stack.CurrentFrame.Name, stack.InScopeNamespaces, out sharedValue);
			}
			catch
			{
				flag = false;
				sharedValue = null;
			}
			if (!flag)
			{
				xamlIndexingReader.CurrentIndex = currentIndex;
				break;
			}
			sharedProperties.Add(sharedValue.Value);
			if ((typeof(GridViewRowPresenter).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType) || typeof(ContentPresenter).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType)) && sharedValue.Value.Property.Name == "ContentSource")
			{
				stack.CurrentFrame.ContentSource = sharedValue.Value.ValueInternal as string;
				if (!(sharedValue.Value.ValueInternal is string) && sharedValue.Value.ValueInternal != null)
				{
					stack.CurrentFrame.ContentSourceSet = false;
				}
			}
			return false;
		}
		case System.Xaml.XamlNodeType.EndMember:
			stack.CurrentFrame.Property = null;
			break;
		case System.Xaml.XamlNodeType.Value:
		{
			if (!stack.CurrentFrame.IsInStyleOrTemplate)
			{
				if (FrameworkTemplate.IsNameProperty(stack.CurrentFrame.Property, stack.CurrentFrame.Type))
				{
					string text = xamlReader.Value as string;
					stack.CurrentFrame.Name = text;
					stack.CurrentFrame.NameSet = true;
					if (TemplateLoadData.RootName == null)
					{
						TemplateLoadData.RootName = text;
					}
					if (!stack.CurrentFrame.IsInNameScope && (typeof(FrameworkElement).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType) || typeof(FrameworkContentElement).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType)))
					{
						TemplateLoadData.NamedTypes.Add(text, stack.CurrentFrame.Type);
						UpdateSharedPropertyNames(text, sharedProperties, stack.CurrentFrame.Type);
					}
				}
				if (typeof(ContentPresenter).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType) && stack.CurrentFrame.Property.Name == "ContentSource")
				{
					stack.CurrentFrame.ContentSource = xamlReader.Value as string;
				}
			}
			if (!(xamlReader.Value is StaticResourceExtension staticResourceExtension))
			{
				break;
			}
			object obj2 = null;
			if (staticResourceExtension.GetType() == typeof(StaticResourceExtension))
			{
				obj2 = staticResourceExtension.TryProvideValueInternal(TemplateLoadData.ServiceProviderWrapper, allowDeferredReference: true, mustReturnDeferredResourceReference: true);
			}
			else if (staticResourceExtension.GetType() == typeof(StaticResourceHolder))
			{
				obj2 = staticResourceExtension.FindResourceInDeferredContent(TemplateLoadData.ServiceProviderWrapper, allowDeferredReference: true, mustReturnDeferredResourceReference: false);
				if (obj2 == DependencyProperty.UnsetValue)
				{
					obj2 = null;
				}
			}
			if (obj2 != null)
			{
				DeferredResourceReference prefetchedValue = obj2 as DeferredResourceReference;
				newValue = new StaticResourceHolder(staticResourceExtension.ResourceKey, prefetchedValue);
			}
			break;
		}
		case System.Xaml.XamlNodeType.NamespaceDeclaration:
			if (!stack.CurrentFrame.IsInStyleOrTemplate && stack.Depth > 0 && !stack.CurrentFrame.NameSet && stack.CurrentFrame.Type != null && !stack.CurrentFrame.IsInNameScope)
			{
				if (typeof(FrameworkElement).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType) || typeof(FrameworkContentElement).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType))
				{
					string name = nameNumber++.ToString(CultureInfo.InvariantCulture) + "_T";
					UpdateSharedPropertyNames(name, sharedProperties, stack.CurrentFrame.Type);
					stack.CurrentFrame.Name = name;
				}
				stack.CurrentFrame.NameSet = true;
			}
			stack.AddNamespace(xamlReader.Namespace);
			break;
		}
		return true;
	}

	private StaticResourceExtension LoadTimeBindUnshareableStaticResource(System.Xaml.XamlReader xamlReader, XamlObjectWriter writer)
	{
		int num = 0;
		do
		{
			writer.WriteNode(xamlReader);
			switch (xamlReader.NodeType)
			{
			case System.Xaml.XamlNodeType.StartObject:
			case System.Xaml.XamlNodeType.GetObject:
				num++;
				break;
			case System.Xaml.XamlNodeType.EndObject:
				num--;
				break;
			}
		}
		while (num > 0 && xamlReader.Read());
		StaticResourceExtension obj = writer.Result as StaticResourceExtension;
		return new StaticResourceHolder(prefetchedValue: (DeferredResourceReference)obj.TryProvideValueInternal(TemplateLoadData.ServiceProviderWrapper, allowDeferredReference: true, mustReturnDeferredResourceReference: true), resourceKey: obj.ResourceKey);
	}

	private bool TrySharingProperty(System.Xaml.XamlReader xamlReader, XamlType parentType, string parentName, FrugalObjectList<NamespaceDeclaration> previousNamespaces, out PropertyValue? sharedValue)
	{
		WpfXamlMember wpfXamlMember = xamlReader.Member as WpfXamlMember;
		if (wpfXamlMember == null)
		{
			sharedValue = null;
			return false;
		}
		DependencyProperty dependencyProperty = wpfXamlMember.DependencyProperty;
		if (dependencyProperty == null)
		{
			sharedValue = null;
			return false;
		}
		if (xamlReader.Member == parentType.GetAliasedProperty(XamlLanguage.Name))
		{
			sharedValue = null;
			return false;
		}
		if (!typeof(FrameworkElement).IsAssignableFrom(parentType.UnderlyingType) && !typeof(FrameworkContentElement).IsAssignableFrom(parentType.UnderlyingType))
		{
			sharedValue = null;
			return false;
		}
		xamlReader.Read();
		if (xamlReader.NodeType == System.Xaml.XamlNodeType.Value)
		{
			if (xamlReader.Value == null)
			{
				sharedValue = null;
				return false;
			}
			if (!CheckSpecialCasesShareable(xamlReader.Value.GetType(), dependencyProperty))
			{
				sharedValue = null;
				return false;
			}
			if (!(xamlReader.Value is string))
			{
				return TrySharingValue(dependencyProperty, xamlReader.Value, parentName, xamlReader, allowRecursive: true, out sharedValue);
			}
			object value = xamlReader.Value;
			TypeConverter typeConverter = null;
			if (wpfXamlMember.TypeConverter != null)
			{
				typeConverter = wpfXamlMember.TypeConverter.ConverterInstance;
			}
			else if (wpfXamlMember.Type.TypeConverter != null)
			{
				typeConverter = wpfXamlMember.Type.TypeConverter.ConverterInstance;
			}
			if (typeConverter != null)
			{
				value = typeConverter.ConvertFrom(TemplateLoadData.ServiceProviderWrapper, CultureInfo.InvariantCulture, value);
			}
			return TrySharingValue(dependencyProperty, value, parentName, xamlReader, allowRecursive: true, out sharedValue);
		}
		if (xamlReader.NodeType == System.Xaml.XamlNodeType.StartObject || xamlReader.NodeType == System.Xaml.XamlNodeType.NamespaceDeclaration)
		{
			FrugalObjectList<NamespaceDeclaration> frugalObjectList = null;
			if (xamlReader.NodeType == System.Xaml.XamlNodeType.NamespaceDeclaration)
			{
				frugalObjectList = new FrugalObjectList<NamespaceDeclaration>();
				while (xamlReader.NodeType == System.Xaml.XamlNodeType.NamespaceDeclaration)
				{
					frugalObjectList.Add(xamlReader.Namespace);
					xamlReader.Read();
				}
			}
			if (!CheckSpecialCasesShareable(xamlReader.Type.UnderlyingType, dependencyProperty))
			{
				sharedValue = null;
				return false;
			}
			if (!IsTypeShareable(xamlReader.Type.UnderlyingType))
			{
				sharedValue = null;
				return false;
			}
			StackOfFrames stackOfFrames = new StackOfFrames();
			stackOfFrames.Push(xamlReader.Type, null);
			bool flag = false;
			bool flag2 = false;
			if (typeof(FrameworkTemplate).IsAssignableFrom(xamlReader.Type.UnderlyingType))
			{
				flag = true;
				Stack = stackOfFrames;
			}
			else if (typeof(Style).IsAssignableFrom(xamlReader.Type.UnderlyingType))
			{
				flag2 = true;
				Stack = stackOfFrames;
			}
			try
			{
				XamlObjectWriter objectWriter = TemplateLoadData.ObjectWriter;
				objectWriter.Clear();
				WriteNamespaces(objectWriter, previousNamespaces, frugalObjectList);
				objectWriter.WriteNode(xamlReader);
				bool flag3 = false;
				while (!flag3 && xamlReader.Read())
				{
					SkipFreeze(xamlReader);
					objectWriter.WriteNode(xamlReader);
					switch (xamlReader.NodeType)
					{
					case System.Xaml.XamlNodeType.StartObject:
						if (typeof(StaticResourceExtension).IsAssignableFrom(xamlReader.Type.UnderlyingType))
						{
							sharedValue = null;
							return false;
						}
						stackOfFrames.Push(xamlReader.Type, null);
						break;
					case System.Xaml.XamlNodeType.GetObject:
					{
						XamlType type = stackOfFrames.CurrentFrame.Property.Type;
						stackOfFrames.Push(type, null);
						break;
					}
					case System.Xaml.XamlNodeType.EndObject:
						if (stackOfFrames.Depth == 1)
						{
							return TrySharingValue(dependencyProperty, objectWriter.Result, parentName, xamlReader, allowRecursive: true, out sharedValue);
						}
						stackOfFrames.PopScope();
						break;
					case System.Xaml.XamlNodeType.StartMember:
						if (!(flag2 || flag) && FrameworkTemplate.IsNameProperty(xamlReader.Member, stackOfFrames.CurrentFrame.Type))
						{
							flag3 = true;
						}
						else
						{
							stackOfFrames.CurrentFrame.Property = xamlReader.Member;
						}
						break;
					case System.Xaml.XamlNodeType.Value:
						if (xamlReader.Value != null && typeof(StaticResourceExtension).IsAssignableFrom(xamlReader.Value.GetType()))
						{
							sharedValue = null;
							return false;
						}
						if (!flag && stackOfFrames.CurrentFrame.Property == XamlLanguage.ConnectionId && OwnerTemplate.StyleConnector != null)
						{
							OwnerTemplate.StyleConnector.Connect((int)xamlReader.Value, stackOfFrames.CurrentFrame.Instance);
						}
						break;
					}
				}
				sharedValue = null;
				return false;
			}
			finally
			{
				Stack = null;
			}
		}
		if (xamlReader.NodeType == System.Xaml.XamlNodeType.GetObject)
		{
			sharedValue = null;
			return false;
		}
		throw new System.Windows.Markup.XamlParseException(SR.ParserUnexpectedEndEle);
	}

	private static bool CheckSpecialCasesShareable(Type typeofValue, DependencyProperty property)
	{
		if (typeofValue != typeof(DynamicResourceExtension) && typeofValue != typeof(TemplateBindingExtension) && typeofValue != typeof(TypeExtension) && typeofValue != typeof(StaticExtension))
		{
			if (typeof(IList).IsAssignableFrom(property.PropertyType))
			{
				return false;
			}
			if (property.PropertyType.IsArray)
			{
				return false;
			}
			if (typeof(IDictionary).IsAssignableFrom(property.PropertyType))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsFreezableDirective(System.Xaml.XamlReader reader)
	{
		if (reader.NodeType == System.Xaml.XamlNodeType.StartMember)
		{
			XamlMember member = reader.Member;
			if (member.IsUnknown && member.IsDirective)
			{
				return member.Name == "Freeze";
			}
			return false;
		}
		return false;
	}

	private static void SkipFreeze(System.Xaml.XamlReader reader)
	{
		if (IsFreezableDirective(reader))
		{
			reader.Read();
			reader.Read();
			reader.Read();
		}
	}

	private bool TrySharingValue(DependencyProperty property, object value, string parentName, System.Xaml.XamlReader xamlReader, bool allowRecursive, out PropertyValue? sharedValue)
	{
		sharedValue = null;
		if (value != null && !IsTypeShareable(value.GetType()))
		{
			return false;
		}
		bool flag = true;
		if (value is Freezable)
		{
			if (value is Freezable freezable)
			{
				if (freezable.CanFreeze)
				{
					freezable.Freeze();
				}
				else
				{
					flag = false;
				}
			}
		}
		else if (value is CollectionViewSource)
		{
			if (value is CollectionViewSource collectionViewSource)
			{
				flag = collectionViewSource.IsShareableInTemplate();
			}
		}
		else if (value is MarkupExtension)
		{
			if (value is BindingBase || value is TemplateBindingExtension || value is DynamicResourceExtension)
			{
				flag = true;
			}
			else if (value is StaticResourceExtension || value is StaticResourceHolder)
			{
				flag = false;
			}
			else
			{
				TemplateLoadData.ServiceProviderWrapper.SetData(_sharedDpInstance, property);
				value = (value as MarkupExtension).ProvideValue(TemplateLoadData.ServiceProviderWrapper);
				TemplateLoadData.ServiceProviderWrapper.Clear();
				if (allowRecursive)
				{
					return TrySharingValue(property, value, parentName, xamlReader, allowRecursive: false, out sharedValue);
				}
				flag = true;
			}
		}
		if (flag)
		{
			PropertyValue value2 = default(PropertyValue);
			value2.Property = property;
			value2.ChildName = parentName;
			value2.ValueInternal = value;
			value2.ValueType = PropertyValueType.Set;
			sharedValue = value2;
			xamlReader.Read();
		}
		return flag;
	}

	private bool IsTypeShareable(Type type)
	{
		if (typeof(Freezable).IsAssignableFrom(type) || type == typeof(string) || type == typeof(Uri) || type == typeof(Type) || (typeof(MarkupExtension).IsAssignableFrom(type) && !typeof(StaticResourceExtension).IsAssignableFrom(type)) || typeof(Style).IsAssignableFrom(type) || typeof(FrameworkTemplate).IsAssignableFrom(type) || typeof(CollectionViewSource).IsAssignableFrom(type) || (type != null && type.IsValueType))
		{
			return true;
		}
		return false;
	}

	private void WriteNamespaces(System.Xaml.XamlWriter writer, FrugalObjectList<NamespaceDeclaration> previousNamespaces, FrugalObjectList<NamespaceDeclaration> localNamespaces)
	{
		if (previousNamespaces != null)
		{
			for (int i = 0; i < previousNamespaces.Count; i++)
			{
				writer.WriteNamespace(previousNamespaces[i]);
			}
		}
		if (localNamespaces != null)
		{
			for (int j = 0; j < localNamespaces.Count; j++)
			{
				writer.WriteNamespace(localNamespaces[j]);
			}
		}
	}

	private static void AutoAliasContentPresenter(Type targetType, string contentSource, string templateChildName, ref FrugalStructList<ChildRecord> childRecordFromChildIndex, ref FrugalStructList<ItemStructMap<TriggerSourceRecord>> triggerSourceRecordFromChildIndex, ref FrugalStructList<ChildPropertyDependent> resourceDependents, ref HybridDictionary dataTriggerRecordFromBinding, ref bool hasInstanceValues, HybridDictionary childIndexFromChildName, bool isContentPropertyDefined, bool isContentSourceSet, bool isContentTemplatePropertyDefined, bool isContentTemplateSelectorPropertyDefined, bool isContentStringFormatPropertyDefined)
	{
		if (string.IsNullOrEmpty(contentSource) && !isContentSourceSet)
		{
			contentSource = "Content";
		}
		if (string.IsNullOrEmpty(contentSource) || isContentPropertyDefined)
		{
			return;
		}
		DependencyProperty dependencyProperty = DependencyProperty.FromName(contentSource, targetType);
		DependencyProperty dependencyProperty2 = DependencyProperty.FromName(contentSource + "Template", targetType);
		DependencyProperty dependencyProperty3 = DependencyProperty.FromName(contentSource + "TemplateSelector", targetType);
		DependencyProperty dependencyProperty4 = DependencyProperty.FromName(contentSource + "StringFormat", targetType);
		if (dependencyProperty == null && isContentSourceSet)
		{
			throw new InvalidOperationException(SR.Format(SR.MissingContentSource, contentSource, targetType));
		}
		if (dependencyProperty != null)
		{
			PropertyValue propertyValue = default(PropertyValue);
			propertyValue.ValueType = PropertyValueType.TemplateBinding;
			propertyValue.ChildName = templateChildName;
			propertyValue.ValueInternal = new TemplateBindingExtension(dependencyProperty);
			propertyValue.Property = ContentPresenter.ContentProperty;
			StyleHelper.UpdateTables(ref propertyValue, ref childRecordFromChildIndex, ref triggerSourceRecordFromChildIndex, ref resourceDependents, ref dataTriggerRecordFromBinding, childIndexFromChildName, ref hasInstanceValues);
		}
		if (!isContentTemplatePropertyDefined && !isContentTemplateSelectorPropertyDefined && !isContentStringFormatPropertyDefined)
		{
			if (dependencyProperty2 != null)
			{
				PropertyValue propertyValue2 = default(PropertyValue);
				propertyValue2.ValueType = PropertyValueType.TemplateBinding;
				propertyValue2.ChildName = templateChildName;
				propertyValue2.ValueInternal = new TemplateBindingExtension(dependencyProperty2);
				propertyValue2.Property = ContentPresenter.ContentTemplateProperty;
				StyleHelper.UpdateTables(ref propertyValue2, ref childRecordFromChildIndex, ref triggerSourceRecordFromChildIndex, ref resourceDependents, ref dataTriggerRecordFromBinding, childIndexFromChildName, ref hasInstanceValues);
			}
			if (dependencyProperty3 != null)
			{
				PropertyValue propertyValue3 = default(PropertyValue);
				propertyValue3.ValueType = PropertyValueType.TemplateBinding;
				propertyValue3.ChildName = templateChildName;
				propertyValue3.ValueInternal = new TemplateBindingExtension(dependencyProperty3);
				propertyValue3.Property = ContentPresenter.ContentTemplateSelectorProperty;
				StyleHelper.UpdateTables(ref propertyValue3, ref childRecordFromChildIndex, ref triggerSourceRecordFromChildIndex, ref resourceDependents, ref dataTriggerRecordFromBinding, childIndexFromChildName, ref hasInstanceValues);
			}
			if (dependencyProperty4 != null)
			{
				PropertyValue propertyValue4 = default(PropertyValue);
				propertyValue4.ValueType = PropertyValueType.TemplateBinding;
				propertyValue4.ChildName = templateChildName;
				propertyValue4.ValueInternal = new TemplateBindingExtension(dependencyProperty4);
				propertyValue4.Property = ContentPresenter.ContentStringFormatProperty;
				StyleHelper.UpdateTables(ref propertyValue4, ref childRecordFromChildIndex, ref triggerSourceRecordFromChildIndex, ref resourceDependents, ref dataTriggerRecordFromBinding, childIndexFromChildName, ref hasInstanceValues);
			}
		}
	}

	private static void AutoAliasGridViewRowPresenter(Type targetType, string contentSource, string childName, ref FrugalStructList<ChildRecord> childRecordFromChildIndex, ref FrugalStructList<ItemStructMap<TriggerSourceRecord>> triggerSourceRecordFromChildIndex, ref FrugalStructList<ChildPropertyDependent> resourceDependents, ref HybridDictionary dataTriggerRecordFromBinding, ref bool hasInstanceValues, HybridDictionary childIndexFromChildID, bool isContentPropertyDefined, bool isColumnsPropertyDefined)
	{
		if (!isContentPropertyDefined)
		{
			DependencyProperty dependencyProperty = DependencyProperty.FromName("Content", targetType);
			if (dependencyProperty != null)
			{
				PropertyValue propertyValue = default(PropertyValue);
				propertyValue.ValueType = PropertyValueType.TemplateBinding;
				propertyValue.ChildName = childName;
				propertyValue.ValueInternal = new TemplateBindingExtension(dependencyProperty);
				propertyValue.Property = GridViewRowPresenter.ContentProperty;
				StyleHelper.UpdateTables(ref propertyValue, ref childRecordFromChildIndex, ref triggerSourceRecordFromChildIndex, ref resourceDependents, ref dataTriggerRecordFromBinding, childIndexFromChildID, ref hasInstanceValues);
			}
		}
		if (!isColumnsPropertyDefined)
		{
			PropertyValue propertyValue2 = default(PropertyValue);
			propertyValue2.ValueType = PropertyValueType.TemplateBinding;
			propertyValue2.ChildName = childName;
			propertyValue2.ValueInternal = new TemplateBindingExtension(GridView.ColumnCollectionProperty);
			propertyValue2.Property = GridViewRowPresenterBase.ColumnsProperty;
			StyleHelper.UpdateTables(ref propertyValue2, ref childRecordFromChildIndex, ref triggerSourceRecordFromChildIndex, ref resourceDependents, ref dataTriggerRecordFromBinding, childIndexFromChildID, ref hasInstanceValues);
		}
	}

	internal XamlType GetTypeForName(string name)
	{
		return TemplateLoadData.NamedTypes[name];
	}
}
