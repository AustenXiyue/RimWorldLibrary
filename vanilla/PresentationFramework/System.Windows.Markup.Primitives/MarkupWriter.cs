using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace System.Windows.Markup.Primitives;

/// <summary>Provides methods to write an object to XAML format.</summary>
public sealed class MarkupWriter : IDisposable
{
	private class PartiallyOrderedList<TKey, TValue> : IEnumerable<TValue>, IEnumerable where TValue : class
	{
		private class Entry
		{
			public readonly TKey Key;

			public readonly TValue Value;

			public List<int> Predecessors;

			public int Link;

			public const int UNSEEN = -1;

			public const int INDFS = -2;

			public Entry(TKey key, TValue value)
			{
				Key = key;
				Value = value;
				Predecessors = null;
				Link = 0;
			}

			public override bool Equals(object obj)
			{
				if (obj is Entry { Key: var key })
				{
					return key.Equals(Key);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return Key.GetHashCode();
			}
		}

		private List<Entry> _entries = new List<Entry>();

		private int _firstIndex = -1;

		private int _lastIndex;

		public void Add(TKey key, TValue value)
		{
			Entry entry = new Entry(key, value);
			int num = _entries.IndexOf(entry);
			if (num >= 0)
			{
				entry.Predecessors = _entries[num].Predecessors;
				_entries[num] = entry;
			}
			else
			{
				_entries.Add(entry);
			}
		}

		private int GetEntryIndex(TKey key)
		{
			Entry item = new Entry(key, null);
			int num = _entries.IndexOf(item);
			if (num < 0)
			{
				num = _entries.Count;
				_entries.Add(item);
			}
			return num;
		}

		public void SetOrder(TKey predecessor, TKey key)
		{
			int entryIndex = GetEntryIndex(predecessor);
			_ = _entries[entryIndex];
			int entryIndex2 = GetEntryIndex(key);
			Entry entry = _entries[entryIndex2];
			if (entry.Predecessors == null)
			{
				entry.Predecessors = new List<int>();
			}
			entry.Predecessors.Add(entryIndex);
			_firstIndex = -1;
		}

		private void TopologicalSort()
		{
			_firstIndex = -1;
			_lastIndex = -1;
			for (int i = 0; i < _entries.Count; i++)
			{
				_entries[i].Link = -1;
			}
			for (int j = 0; j < _entries.Count; j++)
			{
				DepthFirstSearch(j);
			}
		}

		private void DepthFirstSearch(int index)
		{
			if (_entries[index].Link != -1)
			{
				return;
			}
			_entries[index].Link = -2;
			if (_entries[index].Predecessors != null)
			{
				foreach (int predecessor in _entries[index].Predecessors)
				{
					DepthFirstSearch(predecessor);
				}
			}
			if (_lastIndex == -1)
			{
				_firstIndex = index;
			}
			else
			{
				_entries[_lastIndex].Link = index;
			}
			_lastIndex = index;
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			if (_firstIndex < 0)
			{
				TopologicalSort();
			}
			int num = _firstIndex;
			while (num >= 0)
			{
				Entry entry = _entries[num];
				if (entry.Value != null)
				{
					yield return entry.Value;
				}
				num = entry.Link;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			using IEnumerator<TValue> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
		}
	}

	private class Mapping
	{
		public readonly string Uri;

		public readonly string Prefix;

		public Mapping(string uri, string prefix)
		{
			Uri = uri;
			Prefix = prefix;
		}

		public override bool Equals(object obj)
		{
			if (obj is Mapping mapping && Uri.Equals(mapping.Uri))
			{
				return Prefix.Equals(mapping.Prefix);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Uri.GetHashCode() + Prefix.GetHashCode();
		}
	}

	private class Scope
	{
		private Scope _containingScope;

		private bool? _xmlnsSpacePreserve;

		private Dictionary<string, string> _uriToPrefix;

		private Dictionary<string, string> _prefixToUri;

		public bool XmlnsSpacePreserve
		{
			get
			{
				if (_xmlnsSpacePreserve.HasValue)
				{
					return _xmlnsSpacePreserve.Value;
				}
				if (_containingScope != null)
				{
					return _containingScope.XmlnsSpacePreserve;
				}
				return false;
			}
			set
			{
				_xmlnsSpacePreserve = value;
			}
		}

		public bool IsTopOfSpacePreservationScope
		{
			get
			{
				if (_containingScope == null)
				{
					return true;
				}
				if (!_xmlnsSpacePreserve.HasValue)
				{
					return false;
				}
				if (_xmlnsSpacePreserve.Value != _containingScope.XmlnsSpacePreserve)
				{
					return true;
				}
				return false;
			}
		}

		public IEnumerable<Mapping> EnumerateLocalMappings
		{
			get
			{
				if (_uriToPrefix == null)
				{
					yield break;
				}
				foreach (KeyValuePair<string, string> item in _uriToPrefix)
				{
					yield return new Mapping(item.Key, item.Value);
				}
			}
		}

		public IEnumerable<Mapping> EnumerateAllMappings
		{
			get
			{
				if (_containingScope != null)
				{
					foreach (Mapping enumerateAllMapping in _containingScope.EnumerateAllMappings)
					{
						yield return enumerateAllMapping;
					}
				}
				foreach (Mapping enumerateLocalMapping in EnumerateLocalMappings)
				{
					yield return enumerateLocalMapping;
				}
			}
		}

		public Scope(Scope containingScope)
		{
			_containingScope = containingScope;
		}

		public string GetPrefixOf(string uri)
		{
			if (_uriToPrefix != null && _uriToPrefix.TryGetValue(uri, out var value))
			{
				return value;
			}
			if (_containingScope != null)
			{
				return _containingScope.GetPrefixOf(uri);
			}
			return null;
		}

		public string GetUriOf(string prefix)
		{
			if (_prefixToUri != null && _prefixToUri.TryGetValue(prefix, out var value))
			{
				return value;
			}
			if (_containingScope != null)
			{
				return _containingScope.GetUriOf(prefix);
			}
			return null;
		}

		public void RecordMapping(string prefix, string uri)
		{
			if (_uriToPrefix == null)
			{
				_uriToPrefix = new Dictionary<string, string>();
			}
			if (_prefixToUri == null)
			{
				_prefixToUri = new Dictionary<string, string>();
			}
			_uriToPrefix[uri] = prefix;
			_prefixToUri[prefix] = uri;
		}

		public void MakeAddressable(IEnumerable<Type> types)
		{
			if (types == null)
			{
				return;
			}
			foreach (Type type in types)
			{
				MakeAddressable(type);
			}
		}

		public string MakeAddressable(Type type)
		{
			return MakeAddressable(NamespaceCache.GetNamespaceUriFor(type));
		}

		public string MakeAddressable(string uri)
		{
			if (GetPrefixOf(uri) == null)
			{
				string defaultPrefixFor = NamespaceCache.GetDefaultPrefixFor(uri);
				string prefix = defaultPrefixFor;
				int num = 0;
				while (GetUriOf(prefix) != null)
				{
					prefix = defaultPrefixFor + num++;
				}
				RecordMapping(prefix, uri);
			}
			return uri;
		}
	}

	private class MarkupWriterContext : IValueSerializerContext, ITypeDescriptorContext, IServiceProvider
	{
		private Scope _scope;

		public IContainer Container => null;

		public object Instance => null;

		public PropertyDescriptor PropertyDescriptor => null;

		internal MarkupWriterContext(Scope scope)
		{
			_scope = scope;
		}

		public ValueSerializer GetValueSerializerFor(PropertyDescriptor descriptor)
		{
			if (descriptor.PropertyType == typeof(Type))
			{
				return new TypeValueSerializer(_scope);
			}
			return ValueSerializer.GetSerializerFor(descriptor);
		}

		public ValueSerializer GetValueSerializerFor(Type type)
		{
			if (type == typeof(Type))
			{
				return new TypeValueSerializer(_scope);
			}
			return ValueSerializer.GetSerializerFor(type);
		}

		public void OnComponentChanged()
		{
		}

		public bool OnComponentChanging()
		{
			return true;
		}

		public object GetService(Type serviceType)
		{
			return null;
		}
	}

	private class TypeValueSerializer : ValueSerializer
	{
		private Scope _scope;

		public TypeValueSerializer(Scope scope)
		{
			_scope = scope;
		}

		public override bool CanConvertToString(object value, IValueSerializerContext context)
		{
			return true;
		}

		public override string ConvertToString(object value, IValueSerializerContext context)
		{
			Type type = value as Type;
			if (type == null)
			{
				throw new InvalidOperationException();
			}
			string uri = _scope.MakeAddressable(type);
			string prefixOf = _scope.GetPrefixOf(uri);
			if (prefixOf == null || prefixOf == "")
			{
				return type.Name;
			}
			return prefixOf + ":" + type.Name;
		}

		public override IEnumerable<Type> TypeReferences(object value, IValueSerializerContext context)
		{
			Type type = value as Type;
			if (type != null)
			{
				return new Type[1] { type };
			}
			return base.TypeReferences(value, context);
		}
	}

	private static class NamespaceCache
	{
		private static Dictionary<Assembly, Dictionary<string, string>> XmlnsDefinitions = new Dictionary<Assembly, Dictionary<string, string>>();

		private static Dictionary<string, string> DefaultPrefixes = new Dictionary<string, string>();

		private static readonly object SyncObject = new object();

		public static string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

		public static string XmlNamespace = "http://www.w3.org/XML/1998/namespace";

		public static string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";

		private static Dictionary<string, string> GetMappingsFor(Assembly assembly)
		{
			Dictionary<string, string> value;
			lock (SyncObject)
			{
				if (!XmlnsDefinitions.TryGetValue(assembly, out value))
				{
					object[] customAttributes = assembly.GetCustomAttributes(typeof(XmlnsPrefixAttribute), inherit: true);
					for (int i = 0; i < customAttributes.Length; i++)
					{
						XmlnsPrefixAttribute xmlnsPrefixAttribute = (XmlnsPrefixAttribute)customAttributes[i];
						DefaultPrefixes[xmlnsPrefixAttribute.XmlNamespace] = xmlnsPrefixAttribute.Prefix;
					}
					value = new Dictionary<string, string>();
					XmlnsDefinitions[assembly] = value;
					customAttributes = assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), inherit: true);
					for (int i = 0; i < customAttributes.Length; i++)
					{
						XmlnsDefinitionAttribute xmlnsDefinitionAttribute = (XmlnsDefinitionAttribute)customAttributes[i];
						if (xmlnsDefinitionAttribute.AssemblyName == null)
						{
							string value2 = null;
							string value3 = null;
							string value4 = null;
							if (value.TryGetValue(xmlnsDefinitionAttribute.ClrNamespace, out value2) && DefaultPrefixes.TryGetValue(value2, out value3))
							{
								DefaultPrefixes.TryGetValue(xmlnsDefinitionAttribute.XmlNamespace, out value4);
							}
							if (value2 == null || value3 == null || (value4 != null && value3.Length > value4.Length))
							{
								value[xmlnsDefinitionAttribute.ClrNamespace] = xmlnsDefinitionAttribute.XmlNamespace;
							}
						}
						else
						{
							Assembly assembly2 = Assembly.Load(new AssemblyName(xmlnsDefinitionAttribute.AssemblyName));
							if (assembly2 != null)
							{
								GetMappingsFor(assembly2)[xmlnsDefinitionAttribute.ClrNamespace] = xmlnsDefinitionAttribute.XmlNamespace;
							}
						}
					}
				}
			}
			return value;
		}

		public static string GetNamespaceUriFor(Type type)
		{
			string value;
			lock (SyncObject)
			{
				if (type.Namespace == null)
				{
					value = string.Format(CultureInfo.InvariantCulture, "clr-namespace:;assembly={0}", type.Assembly.GetName().Name);
					return value;
				}
				if (!GetMappingsFor(type.Assembly).TryGetValue(type.Namespace, out value))
				{
					value = string.Format(CultureInfo.InvariantCulture, "clr-namespace:{0};assembly={1}", type.Namespace, type.Assembly.GetName().Name);
					return value;
				}
			}
			return value;
		}

		public static string GetDefaultPrefixFor(string uri)
		{
			string value;
			lock (SyncObject)
			{
				DefaultPrefixes.TryGetValue(uri, out value);
				if (value == null)
				{
					value = "assembly";
					if (uri.StartsWith("clr-namespace:", StringComparison.Ordinal))
					{
						ReadOnlySpan<char> readOnlySpan = uri.AsSpan("clr-namespace:".Length, uri.IndexOf(';') - "clr-namespace:".Length);
						StringBuilder stringBuilder = new StringBuilder();
						for (int i = 0; i < readOnlySpan.Length; i++)
						{
							char c = readOnlySpan[i];
							if (c >= 'A' && c <= 'Z')
							{
								stringBuilder.Append(char.ToLowerInvariant(c));
							}
						}
						if (stringBuilder.Length > 0)
						{
							value = stringBuilder.ToString();
						}
					}
				}
			}
			return value;
		}
	}

	private const string clrUriPrefix = "clr-namespace:";

	private const int EXTENSIONLENGTH = 9;

	private XmlWriter _writer;

	private XmlTextWriter _xmlTextWriter;

	private static DefaultValueAttribute _nullDefaultValueAttribute;

	/// <summary>Creates an instance of a <see cref="T:System.Windows.Markup.Primitives.MarkupObject" /> from the specified object.</summary>
	/// <returns>A markup object that enables navigating through the tree of objects.</returns>
	/// <param name="instance">An object that will be the root of the serialized tree.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> is null.</exception>
	public static MarkupObject GetMarkupObjectFor(object instance)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		XamlDesignerSerializationManager xamlDesignerSerializationManager = new XamlDesignerSerializationManager(null);
		xamlDesignerSerializationManager.XamlWriterMode = XamlWriterMode.Expression;
		return new ElementMarkupObject(instance, xamlDesignerSerializationManager);
	}

	/// <summary>Creates an instance of a <see cref="T:System.Windows.Markup.Primitives.MarkupObject" /> from the specified object and the specified serialization manager.</summary>
	/// <returns>A markup object that enables navigating through the tree of objects.</returns>
	/// <param name="instance">An object that will be the root of the serialized tree.</param>
	/// <param name="manager">The serialization manager.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="instance" /> or <paramref name="manager" /> is null.</exception>
	public static MarkupObject GetMarkupObjectFor(object instance, XamlDesignerSerializationManager manager)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if (manager == null)
		{
			throw new ArgumentNullException("manager");
		}
		return new ElementMarkupObject(instance, manager);
	}

	internal static void SaveAsXml(XmlWriter writer, object instance)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		SaveAsXml(writer, GetMarkupObjectFor(instance));
	}

	internal static void SaveAsXml(XmlWriter writer, object instance, XamlDesignerSerializationManager manager)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (manager == null)
		{
			throw new ArgumentNullException("manager");
		}
		manager.ClearXmlWriter();
		SaveAsXml(writer, GetMarkupObjectFor(instance, manager));
	}

	internal static void SaveAsXml(XmlWriter writer, MarkupObject item)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		try
		{
			using MarkupWriter markupWriter = new MarkupWriter(writer);
			markupWriter.WriteItem(item);
		}
		finally
		{
			writer.Flush();
		}
	}

	internal static void VerifyTypeIsSerializable(Type type)
	{
		if (type.IsNestedPublic)
		{
			throw new InvalidOperationException(SR.Format(SR.MarkupWriter_CannotSerializeNestedPublictype, type.ToString()));
		}
		if (!type.IsPublic)
		{
			throw new InvalidOperationException(SR.Format(SR.MarkupWriter_CannotSerializeNonPublictype, type.ToString()));
		}
		if (type.IsGenericType)
		{
			throw new InvalidOperationException(SR.Format(SR.MarkupWriter_CannotSerializeGenerictype, type.ToString()));
		}
	}

	internal MarkupWriter(XmlWriter writer)
	{
		_writer = writer;
		_xmlTextWriter = writer as XmlTextWriter;
	}

	/// <summary>Releases the resources used by the <see cref="T:System.Windows.Markup.Primitives.MarkupWriter" />.</summary>
	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	private bool RecordNamespaces(Scope scope, MarkupObject item, IValueSerializerContext context, bool lastWasString)
	{
		bool result = true;
		if (lastWasString || item.ObjectType != typeof(string) || HasNonValueProperties(item))
		{
			scope.MakeAddressable(item.ObjectType);
			result = false;
		}
		item.AssignRootContext(context);
		lastWasString = false;
		foreach (MarkupProperty property in item.Properties)
		{
			if (property.IsComposite)
			{
				bool flag = IsCollectionType(property.PropertyType);
				foreach (MarkupObject item2 in property.Items)
				{
					lastWasString = RecordNamespaces(scope, item2, context, lastWasString || flag);
				}
			}
			else
			{
				scope.MakeAddressable(property.TypeReferences);
			}
			if (property.DependencyProperty != null)
			{
				scope.MakeAddressable(property.DependencyProperty.OwnerType);
			}
			if (property.IsKey)
			{
				scope.MakeAddressable(NamespaceCache.XamlNamespace);
			}
		}
		return result;
	}

	internal void WriteItem(MarkupObject item)
	{
		VerifyTypeIsSerializable(item.ObjectType);
		Scope scope = new Scope(null);
		scope.RecordMapping("", NamespaceCache.GetNamespaceUriFor(item.ObjectType));
		RecordNamespaces(scope, item, new MarkupWriterContext(scope), lastWasString: false);
		item = new ExtensionSimplifierMarkupObject(item, null);
		WriteItem(item, scope);
		_writer = null;
	}

	private void WriteItem(MarkupObject item, Scope scope)
	{
		VerifyTypeIsSerializable(item.ObjectType);
		MarkupWriterContext context = new MarkupWriterContext(scope);
		item.AssignRootContext(context);
		string text = scope.MakeAddressable(item.ObjectType);
		string prefixOf = scope.GetPrefixOf(text);
		string text2 = item.ObjectType.Name;
		if (typeof(MarkupExtension).IsAssignableFrom(item.ObjectType) && text2.EndsWith("Extension", StringComparison.Ordinal))
		{
			text2 = text2.Substring(0, text2.Length - 9);
		}
		_writer.WriteStartElement(prefixOf, text2, text);
		ContentPropertyAttribute cpa = item.Attributes[typeof(ContentPropertyAttribute)] as ContentPropertyAttribute;
		XmlLangPropertyAttribute xmlLangPropertyAttribute = item.Attributes[typeof(XmlLangPropertyAttribute)] as XmlLangPropertyAttribute;
		UidPropertyAttribute uidPropertyAttribute = item.Attributes[typeof(UidPropertyAttribute)] as UidPropertyAttribute;
		MarkupProperty contentProperty = null;
		int num = 0;
		List<int> list = null;
		List<MarkupProperty> list2 = null;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		PartiallyOrderedList<string, MarkupProperty> deferredProperties = null;
		Formatting formatting = ((_xmlTextWriter != null) ? _xmlTextWriter.Formatting : Formatting.None);
		foreach (MarkupProperty property in item.GetProperties(mapToConstructorArgs: false))
		{
			if (property.IsConstructorArgument)
			{
				throw new InvalidOperationException(SR.UnserializableKeyValue);
			}
			if (IsContentProperty(property, cpa, ref contentProperty) || IsDeferredProperty(property, dictionary, ref deferredProperties))
			{
				continue;
			}
			if (!property.IsComposite)
			{
				if (property.IsAttached || property.PropertyDescriptor == null)
				{
					if (property.IsValueAsString)
					{
						contentProperty = property;
						continue;
					}
					if (property.IsKey)
					{
						scope.MakeAddressable(NamespaceCache.XamlNamespace);
						_writer.WriteAttributeString(scope.GetPrefixOf(NamespaceCache.XamlNamespace), "Key", NamespaceCache.XamlNamespace, property.StringValue);
						continue;
					}
					DependencyProperty dependencyProperty = property.DependencyProperty;
					string text3 = scope.MakeAddressable(dependencyProperty.OwnerType);
					scope.MakeAddressable(property.TypeReferences);
					if (property.Attributes[typeof(DesignerSerializationOptionsAttribute)] != null && (property.Attributes[typeof(DesignerSerializationOptionsAttribute)] as DesignerSerializationOptionsAttribute).DesignerSerializationOptions == DesignerSerializationOptions.SerializeAsAttribute)
					{
						if (dependencyProperty == UIElement.UidProperty)
						{
							string text4 = scope.MakeAddressable(typeof(TypeExtension));
							_writer.WriteAttributeString(scope.GetPrefixOf(text4), dependencyProperty.Name, text4, property.StringValue);
						}
						continue;
					}
					property.VerifyOnlySerializableTypes();
					string prefixOf2 = scope.GetPrefixOf(text3);
					string localName = dependencyProperty.OwnerType.Name + "." + dependencyProperty.Name;
					if (string.IsNullOrEmpty(prefixOf2))
					{
						_writer.WriteAttributeString(localName, property.StringValue);
					}
					else
					{
						_writer.WriteAttributeString(prefixOf2, localName, text3, property.StringValue);
					}
				}
				else
				{
					property.VerifyOnlySerializableTypes();
					if (xmlLangPropertyAttribute != null && xmlLangPropertyAttribute.Name == property.PropertyDescriptor.Name)
					{
						_writer.WriteAttributeString("xml", "lang", NamespaceCache.XmlNamespace, property.StringValue);
					}
					else if (uidPropertyAttribute != null && uidPropertyAttribute.Name == property.PropertyDescriptor.Name)
					{
						string text5 = scope.MakeAddressable(NamespaceCache.XamlNamespace);
						_writer.WriteAttributeString(scope.GetPrefixOf(text5), property.PropertyDescriptor.Name, text5, property.StringValue);
					}
					else
					{
						_writer.WriteAttributeString(property.PropertyDescriptor.Name, property.StringValue);
					}
					dictionary[property.Name] = property.Name;
				}
				continue;
			}
			if (property.DependencyProperty != null)
			{
				scope.MakeAddressable(property.DependencyProperty.OwnerType);
			}
			if (property.IsKey)
			{
				scope.MakeAddressable(NamespaceCache.XamlNamespace);
			}
			else if (property.IsConstructorArgument)
			{
				scope.MakeAddressable(NamespaceCache.XamlNamespace);
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(++num);
			}
			if (list2 == null)
			{
				list2 = new List<MarkupProperty>();
			}
			list2.Add(property);
		}
		foreach (Mapping enumerateLocalMapping in scope.EnumerateLocalMappings)
		{
			_writer.WriteAttributeString("xmlns", enumerateLocalMapping.Prefix, NamespaceCache.XmlnsNamespace, enumerateLocalMapping.Uri);
		}
		if (!scope.XmlnsSpacePreserve && contentProperty != null && !HasOnlyNormalizationNeutralStrings(contentProperty, keepLeadingSpace: false, keepTrailingSpace: false))
		{
			_writer.WriteAttributeString("xml", "space", NamespaceCache.XmlNamespace, "preserve");
			scope.XmlnsSpacePreserve = true;
			_writer.WriteString(string.Empty);
			if (scope.IsTopOfSpacePreservationScope && _xmlTextWriter != null)
			{
				_xmlTextWriter.Formatting = Formatting.None;
			}
		}
		if (list2 != null)
		{
			foreach (MarkupProperty item2 in list2)
			{
				bool flag = false;
				bool flag2 = false;
				foreach (MarkupObject item3 in item2.Items)
				{
					if (!flag)
					{
						flag = true;
						if (item2.IsAttached || item2.PropertyDescriptor == null)
						{
							if (item2.IsKey)
							{
								throw new InvalidOperationException(SR.Format(SR.UnserializableKeyValue, item2.Value.GetType().FullName));
							}
							string uri = scope.MakeAddressable(item2.DependencyProperty.OwnerType);
							WritePropertyStart(scope.GetPrefixOf(uri), item2.DependencyProperty.OwnerType.Name + "." + item2.DependencyProperty.Name, uri);
						}
						else
						{
							WritePropertyStart(prefixOf, item.ObjectType.Name + "." + item2.PropertyDescriptor.Name, text);
							dictionary[item2.Name] = item2.Name;
						}
						flag2 = NeedToWriteExplicitTag(item2, item3);
						if (flag2)
						{
							WriteExplicitTagStart(item2, scope);
						}
					}
					WriteItem(item3, new Scope(scope));
				}
				if (flag)
				{
					if (flag2)
					{
						WriteExplicitTagEnd();
					}
					WritePropertyEnd();
				}
			}
		}
		if (contentProperty != null)
		{
			if (contentProperty.IsComposite)
			{
				if (contentProperty.Value is IXmlSerializable xmlSerializable)
				{
					WriteXmlIsland(xmlSerializable, scope);
				}
				else
				{
					bool flag3 = false;
					List<Type> wrapperTypes = GetWrapperTypes(contentProperty.PropertyType);
					if (wrapperTypes == null)
					{
						foreach (MarkupObject item4 in contentProperty.Items)
						{
							if (!flag3 && item4.ObjectType == typeof(string) && !IsCollectionType(contentProperty.PropertyType) && !HasNonValueProperties(item4))
							{
								_writer.WriteString(TextValue(item4));
								flag3 = true;
							}
							else
							{
								WriteItem(item4, new Scope(scope));
								flag3 = false;
							}
						}
					}
					else
					{
						foreach (MarkupObject item5 in contentProperty.Items)
						{
							MarkupProperty wrappedProperty = GetWrappedProperty(wrapperTypes, item5);
							if (wrappedProperty == null)
							{
								WriteItem(item5, new Scope(scope));
								flag3 = false;
							}
							else if (wrappedProperty.IsComposite)
							{
								foreach (MarkupObject item6 in wrappedProperty.Items)
								{
									if (!flag3 && item5.ObjectType == typeof(string) && !HasNonValueProperties(item5))
									{
										_writer.WriteString(TextValue(item6));
										flag3 = true;
									}
									else
									{
										WriteItem(item6, new Scope(scope));
										flag3 = false;
									}
								}
							}
							else if (!flag3)
							{
								_writer.WriteString(wrappedProperty.StringValue);
								flag3 = true;
							}
							else
							{
								WriteItem(item5, new Scope(scope));
								flag3 = false;
							}
						}
					}
				}
			}
			else
			{
				string text6 = contentProperty.Value as string;
				if (text6 == null)
				{
					text6 = contentProperty.StringValue;
				}
				_writer.WriteString(text6);
			}
			dictionary[contentProperty.Name] = contentProperty.Name;
		}
		if (deferredProperties != null)
		{
			foreach (MarkupProperty item7 in deferredProperties)
			{
				if (dictionary.ContainsKey(item7.Name))
				{
					continue;
				}
				dictionary[item7.Name] = item7.Name;
				_writer.WriteStartElement(prefixOf, item.ObjectType.Name + "." + item7.PropertyDescriptor.Name, text);
				if (item7.IsComposite || item7.StringValue.IndexOf('{') == 0)
				{
					foreach (MarkupObject item8 in item7.Items)
					{
						WriteItem(item8, new Scope(scope));
					}
				}
				else
				{
					_writer.WriteString(item7.StringValue);
				}
				_writer.WriteEndElement();
			}
		}
		_writer.WriteEndElement();
		if (scope.IsTopOfSpacePreservationScope && _xmlTextWriter != null && _xmlTextWriter.Formatting != formatting)
		{
			_xmlTextWriter.Formatting = formatting;
		}
	}

	private bool IsContentProperty(MarkupProperty property, ContentPropertyAttribute cpa, ref MarkupProperty contentProperty)
	{
		bool flag = property.IsContent;
		if (!flag)
		{
			PropertyDescriptor propertyDescriptor = property.PropertyDescriptor;
			if ((propertyDescriptor != null && typeof(FrameworkTemplate).IsAssignableFrom(propertyDescriptor.ComponentType) && property.Name == "Template") || property.Name == "VisualTree")
			{
				flag = true;
			}
			if (cpa != null && contentProperty == null && propertyDescriptor != null && propertyDescriptor.Name == cpa.Name)
			{
				if (property.IsComposite)
				{
					if (propertyDescriptor == null || propertyDescriptor.IsReadOnly || !typeof(IList).IsAssignableFrom(propertyDescriptor.PropertyType))
					{
						flag = true;
					}
				}
				else if (property.Value != null && !(property.Value is MarkupExtension) && property.PropertyType.IsAssignableFrom(typeof(string)))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			contentProperty = property;
		}
		return flag;
	}

	private bool IsDeferredProperty(MarkupProperty property, Dictionary<string, string> writtenAttributes, ref PartiallyOrderedList<string, MarkupProperty> deferredProperties)
	{
		bool flag = false;
		if (property.PropertyDescriptor != null)
		{
			foreach (Attribute attribute in property.Attributes)
			{
				if (attribute is DependsOnAttribute dependsOnAttribute && !writtenAttributes.ContainsKey(dependsOnAttribute.Name))
				{
					if (deferredProperties == null)
					{
						deferredProperties = new PartiallyOrderedList<string, MarkupProperty>();
					}
					deferredProperties.SetOrder(dependsOnAttribute.Name, property.Name);
					flag = true;
				}
			}
			if (flag)
			{
				deferredProperties.Add(property.Name, property);
			}
		}
		return flag;
	}

	private bool NeedToWriteExplicitTag(MarkupProperty property, MarkupObject firstItem)
	{
		bool result = false;
		if (property.IsCollectionProperty)
		{
			if (_nullDefaultValueAttribute == null)
			{
				_nullDefaultValueAttribute = new DefaultValueAttribute(null);
			}
			if (property.Attributes.Contains(_nullDefaultValueAttribute))
			{
				result = true;
				object instance = firstItem.Instance;
				if (instance is MarkupExtension)
				{
					if (instance is NullExtension)
					{
						result = false;
					}
					else if (property.PropertyType.IsArray)
					{
						ArrayExtension arrayExtension = instance as ArrayExtension;
						if (property.PropertyType.IsAssignableFrom(arrayExtension.Type.MakeArrayType()))
						{
							result = false;
						}
					}
				}
				else if (property.PropertyType.IsAssignableFrom(firstItem.ObjectType))
				{
					result = false;
				}
			}
		}
		return result;
	}

	private void WriteExplicitTagStart(MarkupProperty property, Scope scope)
	{
		Type type = property.Value.GetType();
		string text = scope.MakeAddressable(type);
		string prefixOf = scope.GetPrefixOf(text);
		string text2 = type.Name;
		if (typeof(MarkupExtension).IsAssignableFrom(type) && text2.EndsWith("Extension", StringComparison.Ordinal))
		{
			text2 = text2.Substring(0, text2.Length - 9);
		}
		_writer.WriteStartElement(prefixOf, text2, text);
	}

	private void WriteExplicitTagEnd()
	{
		_writer.WriteEndElement();
	}

	private void WritePropertyStart(string prefix, string propertyName, string uri)
	{
		_writer.WriteStartElement(prefix, propertyName, uri);
	}

	private void WritePropertyEnd()
	{
		_writer.WriteEndElement();
	}

	private void WriteXmlIsland(IXmlSerializable xmlSerializable, Scope scope)
	{
		scope.MakeAddressable(NamespaceCache.XamlNamespace);
		_writer.WriteStartElement(scope.GetPrefixOf(NamespaceCache.XamlNamespace), "XData", NamespaceCache.XamlNamespace);
		xmlSerializable.WriteXml(_writer);
		_writer.WriteEndElement();
	}

	private List<Type> GetWrapperTypes(Type type)
	{
		AttributeCollection attributes = TypeDescriptor.GetAttributes(type);
		if (attributes[typeof(ContentWrapperAttribute)] == null)
		{
			return null;
		}
		List<Type> list = new List<Type>();
		foreach (Attribute item in attributes)
		{
			if (item is ContentWrapperAttribute contentWrapperAttribute)
			{
				list.Add(contentWrapperAttribute.ContentWrapper);
			}
		}
		return list;
	}

	private MarkupProperty GetWrappedProperty(List<Type> wrapperTypes, MarkupObject item)
	{
		if (!IsInTypes(item.ObjectType, wrapperTypes))
		{
			return null;
		}
		ContentPropertyAttribute contentPropertyAttribute = item.Attributes[typeof(ContentPropertyAttribute)] as ContentPropertyAttribute;
		MarkupProperty result = null;
		foreach (MarkupProperty property in item.Properties)
		{
			if (property.IsContent || (contentPropertyAttribute != null && property.PropertyDescriptor != null && property.PropertyDescriptor.Name == contentPropertyAttribute.Name))
			{
				result = property;
				continue;
			}
			result = null;
			break;
		}
		return result;
	}

	private bool IsInTypes(Type type, List<Type> types)
	{
		foreach (Type type2 in types)
		{
			if (type2 == type)
			{
				return true;
			}
		}
		return false;
	}

	private string TextValue(MarkupObject item)
	{
		using (IEnumerator<MarkupProperty> enumerator = item.Properties.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				MarkupProperty current = enumerator.Current;
				if (current.IsValueAsString)
				{
					return current.StringValue;
				}
			}
		}
		return null;
	}

	private bool HasNonValueProperties(MarkupObject item)
	{
		foreach (MarkupProperty property in item.Properties)
		{
			if (!property.IsValueAsString)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsCollectionType(Type type)
	{
		if (!typeof(IEnumerable).IsAssignableFrom(type))
		{
			return typeof(Array).IsAssignableFrom(type);
		}
		return true;
	}

	private bool HasOnlyNormalizationNeutralStrings(MarkupProperty contentProperty, bool keepLeadingSpace, bool keepTrailingSpace)
	{
		if (!contentProperty.IsComposite)
		{
			return IsNormalizationNeutralString(contentProperty.StringValue, keepLeadingSpace, keepTrailingSpace);
		}
		bool flag = true;
		bool flag2 = !keepLeadingSpace;
		bool flag3 = !keepLeadingSpace;
		string text = null;
		MarkupProperty markupProperty = null;
		List<Type> wrapperTypes = GetWrapperTypes(contentProperty.PropertyType);
		foreach (MarkupObject item in contentProperty.Items)
		{
			flag3 = flag2;
			flag2 = ShouldTrimSurroundingWhitespace(item);
			if (text != null)
			{
				flag = IsNormalizationNeutralString(text, !flag3, !flag2);
				text = null;
				if (!flag)
				{
					return false;
				}
			}
			if (markupProperty != null)
			{
				flag = HasOnlyNormalizationNeutralStrings(markupProperty, !flag3, !flag2);
				markupProperty = null;
				if (!flag)
				{
					return false;
				}
			}
			if (item.ObjectType == typeof(string))
			{
				text = TextValue(item);
				if (text != null)
				{
					continue;
				}
			}
			if (wrapperTypes != null)
			{
				MarkupProperty wrappedProperty = GetWrappedProperty(wrapperTypes, item);
				if (wrappedProperty != null)
				{
					markupProperty = wrappedProperty;
				}
			}
		}
		if (text != null)
		{
			flag = IsNormalizationNeutralString(text, !flag3, keepTrailingSpace);
		}
		else if (markupProperty != null)
		{
			flag = HasOnlyNormalizationNeutralStrings(markupProperty, !flag3, keepTrailingSpace);
		}
		return flag;
	}

	private bool ShouldTrimSurroundingWhitespace(MarkupObject item)
	{
		return item.Attributes[typeof(TrimSurroundingWhitespaceAttribute)] is TrimSurroundingWhitespaceAttribute;
	}

	private bool IsNormalizationNeutralString(string value, bool keepLeadingSpace, bool keepTrailingSpace)
	{
		bool flag = !keepLeadingSpace;
		for (int i = 0; i < value.Length; i++)
		{
			switch (value[i])
			{
			case ' ':
				if (flag)
				{
					return false;
				}
				flag = true;
				break;
			case '\t':
			case '\n':
			case '\f':
			case '\r':
				return false;
			default:
				flag = false;
				break;
			}
		}
		return !flag || keepTrailingSpace;
	}
}
