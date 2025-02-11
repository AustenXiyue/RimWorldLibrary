using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Markup;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Data;
using MS.Utility;

namespace System.Windows;

/// <summary>Implements a data structure for describing a property as a path below another property, or below an owning type. Property paths are used in data binding to objects, and in storyboards and timelines for animations.</summary>
[TypeConverter(typeof(PropertyPathConverter))]
public sealed class PropertyPath
{
	private class PathParameterCollection : ObservableCollection<object>
	{
		public PathParameterCollection()
		{
		}

		public PathParameterCollection(object[] parameters)
		{
			IList<object> list = base.Items;
			foreach (object item in parameters)
			{
				list.Add(item);
			}
		}
	}

	private const string SingleStepPath = "(0)";

	private static readonly char[] s_comma = new char[1] { ',' };

	private string _path = string.Empty;

	private PathParameterCollection _parameters;

	private SourceValueInfo[] _arySVI;

	private string _lastError = string.Empty;

	private object[] _earlyBoundPathParts;

	private PropertyPathWorker _singleWorker;

	/// <summary>Gets or sets the string that describes the path. </summary>
	/// <returns>The string that describes the path.</returns>
	public string Path
	{
		get
		{
			return _path;
		}
		set
		{
			_path = value;
			PrepareSourceValueInfo(null);
		}
	}

	/// <summary>Gets the list of parameters to use when the path refers to indexed parameters.</summary>
	/// <returns>The parameter list.</returns>
	public Collection<object> PathParameters
	{
		get
		{
			if (_parameters == null)
			{
				SetPathParameterCollection(new PathParameterCollection());
			}
			return _parameters;
		}
	}

	internal int Length => _arySVI.Length;

	internal PropertyPathStatus Status => SingleWorker.Status;

	internal string LastError => _lastError;

	internal object LastItem => GetItem(Length - 1);

	internal object LastAccessor => GetAccessor(Length - 1);

	internal object[] LastIndexerArguments => GetIndexerArguments(Length - 1);

	internal bool StartsWithStaticProperty
	{
		get
		{
			if (Length > 0)
			{
				return IsStaticProperty(_earlyBoundPathParts[0]);
			}
			return false;
		}
	}

	internal SourceValueInfo[] SVI => _arySVI;

	private PropertyPathWorker SingleWorker
	{
		get
		{
			if (_singleWorker == null)
			{
				_singleWorker = new PropertyPathWorker(this);
			}
			return _singleWorker;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.PropertyPath" /> class, with the provided pathing token string and parameters.</summary>
	/// <param name="path">A string that specifies the <see cref="P:System.Windows.PropertyPath.Path" />, in a tokenized format.</param>
	/// <param name="pathParameters">An array of objects that sets the <see cref="P:System.Windows.PropertyPath.PathParameters" />.  </param>
	public PropertyPath(string path, params object[] pathParameters)
	{
		if (Dispatcher.CurrentDispatcher == null)
		{
			throw new InvalidOperationException();
		}
		_path = path;
		if (pathParameters != null && pathParameters.Length != 0)
		{
			PathParameterCollection pathParameterCollection = new PathParameterCollection(pathParameters);
			SetPathParameterCollection(pathParameterCollection);
		}
		PrepareSourceValueInfo(null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.PropertyPath" /> class.</summary>
	/// <param name="parameter">A property path that either describes a path to a common language runtime (CLR) property, or a single dependency property. </param>
	public PropertyPath(object parameter)
		: this("(0)", parameter)
	{
	}

	internal PropertyPath(string path, ITypeDescriptorContext typeDescriptorContext)
	{
		_path = path;
		PrepareSourceValueInfo(typeDescriptorContext);
		NormalizePath();
	}

	internal static bool IsStaticProperty(object accessor)
	{
		DowncastAccessor(accessor, out var _, out var pi, out var _, out var _);
		if (pi != null)
		{
			MethodInfo getMethod = pi.GetGetMethod();
			if (getMethod != null)
			{
				return getMethod.IsStatic;
			}
			return false;
		}
		return false;
	}

	internal static void DowncastAccessor(object accessor, out DependencyProperty dp, out PropertyInfo pi, out PropertyDescriptor pd, out DynamicObjectAccessor doa)
	{
		if ((dp = accessor as DependencyProperty) != null)
		{
			pd = null;
			pi = null;
			doa = null;
		}
		else if ((pi = accessor as PropertyInfo) != null)
		{
			pd = null;
			doa = null;
		}
		else if ((pd = accessor as PropertyDescriptor) != null)
		{
			doa = null;
		}
		else
		{
			doa = accessor as DynamicObjectAccessor;
		}
	}

	internal IDisposable SetContext(object rootItem)
	{
		return SingleWorker.SetContext(rootItem);
	}

	internal object GetItem(int k)
	{
		return SingleWorker.GetItem(k);
	}

	internal object GetAccessor(int k)
	{
		object obj = _earlyBoundPathParts[k];
		if (obj == null)
		{
			obj = SingleWorker.GetAccessor(k);
		}
		return obj;
	}

	internal object[] GetIndexerArguments(int k)
	{
		return SingleWorker.GetIndexerArguments(k);
	}

	internal object GetValue()
	{
		return SingleWorker.RawValue();
	}

	internal int ComputeUnresolvedAttachedPropertiesInPath()
	{
		int num = 0;
		for (int num2 = Length - 1; num2 >= 0; num2--)
		{
			if (_earlyBoundPathParts[num2] == null)
			{
				string name = _arySVI[num2].name;
				if (IsPropertyReference(name) && name.Contains('.'))
				{
					num++;
				}
			}
		}
		return num;
	}

	internal object ResolvePropertyName(int level, object item, Type ownerType, object context)
	{
		object obj = _earlyBoundPathParts[level];
		if (obj == null)
		{
			obj = ResolvePropertyName(_arySVI[level].name, item, ownerType, context, throwOnError: false);
		}
		return obj;
	}

	internal IndexerParameterInfo[] ResolveIndexerParams(int level, object context)
	{
		IndexerParameterInfo[] array = _earlyBoundPathParts[level] as IndexerParameterInfo[];
		if (array == null)
		{
			array = ResolveIndexerParams(_arySVI[level].paramList, context, throwOnError: false);
		}
		return array;
	}

	internal void ReplaceIndexerByProperty(int level, string name)
	{
		_arySVI[level].name = name;
		_arySVI[level].propertyName = name;
		_arySVI[level].type = SourceValueType.Property;
		_earlyBoundPathParts[level] = null;
	}

	private void PrepareSourceValueInfo(ITypeDescriptorContext typeDescriptorContext)
	{
		PathParser pathParser = DataBindEngine.CurrentDataBindEngine.PathParser;
		_arySVI = pathParser.Parse(Path);
		if (_arySVI.Length == 0)
		{
			string text = pathParser.Error;
			if (text == null)
			{
				text = Path;
			}
			throw new InvalidOperationException(SR.Format(SR.PropertyPathSyntaxError, text));
		}
		ResolvePathParts(typeDescriptorContext);
	}

	private void NormalizePath()
	{
		StringBuilder stringBuilder = new StringBuilder();
		PathParameterCollection pathParameterCollection = new PathParameterCollection();
		for (int i = 0; i < _arySVI.Length; i++)
		{
			switch (_arySVI[i].drillIn)
			{
			case DrillIn.Always:
				stringBuilder.Append('/');
				break;
			case DrillIn.Never:
				if (_arySVI[i].type == SourceValueType.Property)
				{
					stringBuilder.Append('.');
				}
				break;
			}
			switch (_arySVI[i].type)
			{
			case SourceValueType.Property:
				if (_earlyBoundPathParts[i] != null)
				{
					stringBuilder.Append('(');
					stringBuilder.Append(pathParameterCollection.Count.ToString(TypeConverterHelper.InvariantEnglishUS.NumberFormat));
					stringBuilder.Append(')');
					pathParameterCollection.Add(_earlyBoundPathParts[i]);
				}
				else
				{
					stringBuilder.Append(_arySVI[i].name);
				}
				break;
			case SourceValueType.Indexer:
				stringBuilder.Append('[');
				if (_earlyBoundPathParts[i] != null)
				{
					IndexerParameterInfo[] array = (IndexerParameterInfo[])_earlyBoundPathParts[i];
					int num = 0;
					while (true)
					{
						IndexerParameterInfo indexerParameterInfo = array[num];
						if (indexerParameterInfo.type != null)
						{
							stringBuilder.Append('(');
							stringBuilder.Append(pathParameterCollection.Count.ToString(TypeConverterHelper.InvariantEnglishUS.NumberFormat));
							stringBuilder.Append(')');
							pathParameterCollection.Add(indexerParameterInfo.value);
						}
						else
						{
							stringBuilder.Append(indexerParameterInfo.value);
						}
						num++;
						if (num >= array.Length)
						{
							break;
						}
						stringBuilder.Append(',');
					}
				}
				else
				{
					stringBuilder.Append(_arySVI[i].name);
				}
				stringBuilder.Append(']');
				break;
			}
		}
		if (pathParameterCollection.Count > 0)
		{
			_path = stringBuilder.ToString();
			SetPathParameterCollection(pathParameterCollection);
		}
	}

	private void SetPathParameterCollection(PathParameterCollection parameters)
	{
		if (_parameters != null)
		{
			_parameters.CollectionChanged -= ParameterCollectionChanged;
		}
		_parameters = parameters;
		if (_parameters != null)
		{
			_parameters.CollectionChanged += ParameterCollectionChanged;
		}
	}

	private void ParameterCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		PrepareSourceValueInfo(null);
	}

	private void ResolvePathParts(ITypeDescriptorContext typeDescriptorContext)
	{
		bool throwOnError = typeDescriptorContext != null;
		object obj = null;
		if (typeDescriptorContext is TypeConvertContext typeConvertContext)
		{
			obj = typeConvertContext.ParserContext;
		}
		if (obj == null)
		{
			obj = typeDescriptorContext;
		}
		_earlyBoundPathParts = new object[Length];
		for (int num = Length - 1; num >= 0; num--)
		{
			if (_arySVI[num].type == SourceValueType.Property)
			{
				string name = _arySVI[num].name;
				if (IsPropertyReference(name))
				{
					object obj2 = ResolvePropertyName(name, null, null, obj, throwOnError);
					_earlyBoundPathParts[num] = obj2;
					if (obj2 != null)
					{
						_arySVI[num].propertyName = GetPropertyName(obj2);
					}
				}
				else
				{
					_arySVI[num].propertyName = name;
				}
			}
			else if (_arySVI[num].type == SourceValueType.Indexer)
			{
				IndexerParameterInfo[] array = ResolveIndexerParams(_arySVI[num].paramList, obj, throwOnError);
				_earlyBoundPathParts[num] = array;
				_arySVI[num].propertyName = "Item[]";
			}
		}
	}

	private object ResolvePropertyName(string name, object item, Type ownerType, object context, bool throwOnError)
	{
		string text = name;
		if (IsParameterIndex(name, out var index))
		{
			if (0 <= index && index < PathParameters.Count)
			{
				object obj = PathParameters[index];
				if (!IsValidAccessor(obj))
				{
					throw new InvalidOperationException(SR.Format(SR.PropertyPathInvalidAccessor, (obj != null) ? obj.GetType().FullName : "null"));
				}
				return obj;
			}
			if (throwOnError)
			{
				throw new InvalidOperationException(SR.Format(SR.PathParametersIndexOutOfRange, index, PathParameters.Count));
			}
			return null;
		}
		if (IsPropertyReference(name))
		{
			name = name.Substring(1, name.Length - 2);
			int num = name.LastIndexOf('.');
			if (num >= 0)
			{
				text = name.Substring(num + 1).Trim();
				string text2 = name.Substring(0, num).Trim();
				ownerType = GetTypeFromName(text2, context);
				if (ownerType == null && throwOnError)
				{
					throw new InvalidOperationException(SR.Format(SR.PropertyPathNoOwnerType, text2));
				}
			}
			else
			{
				text = name;
			}
		}
		if (ownerType != null)
		{
			object obj2 = DependencyProperty.FromName(text, ownerType);
			if (obj2 == null && item is ICustomTypeDescriptor)
			{
				obj2 = TypeDescriptor.GetProperties(item)[text];
			}
			if (obj2 == null && (item is INotifyPropertyChanged || item is DependencyObject))
			{
				obj2 = GetPropertyHelper(ownerType, text);
			}
			if (obj2 == null && item != null)
			{
				obj2 = TypeDescriptor.GetProperties(item)[text];
			}
			if (obj2 == null)
			{
				obj2 = GetPropertyHelper(ownerType, text);
			}
			if (obj2 == null && SystemCoreHelper.IsIDynamicMetaObjectProvider(item))
			{
				obj2 = SystemCoreHelper.NewDynamicPropertyAccessor(item.GetType(), text);
			}
			if (obj2 == null && throwOnError)
			{
				throw new InvalidOperationException(SR.Format(SR.PropertyPathNoProperty, ownerType.Name, text));
			}
			return obj2;
		}
		return null;
	}

	private PropertyInfo GetPropertyHelper(Type ownerType, string propertyName)
	{
		PropertyInfo propertyInfo = null;
		bool flag = false;
		bool flag2 = false;
		try
		{
			propertyInfo = ownerType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		}
		catch (AmbiguousMatchException)
		{
			flag = true;
		}
		if (flag)
		{
			try
			{
				propertyInfo = null;
				while (propertyInfo == null && ownerType != null)
				{
					propertyInfo = ownerType.GetProperty(propertyName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
					ownerType = ownerType.BaseType;
				}
			}
			catch (AmbiguousMatchException)
			{
				flag2 = true;
			}
		}
		if (PropertyPathWorker.IsIndexedProperty(propertyInfo))
		{
			flag2 = true;
		}
		if (flag2)
		{
			propertyInfo = IndexerPropertyInfo.Instance;
		}
		return propertyInfo;
	}

	private IndexerParameterInfo[] ResolveIndexerParams(FrugalObjectList<IndexerParamInfo> paramList, object context, bool throwOnError)
	{
		IndexerParameterInfo[] array = new IndexerParameterInfo[paramList.Count];
		for (int i = 0; i < array.Length; i++)
		{
			if (string.IsNullOrEmpty(paramList[i].parenString))
			{
				array[i].value = paramList[i].valueString;
				continue;
			}
			if (string.IsNullOrEmpty(paramList[i].valueString))
			{
				if (int.TryParse(paramList[i].parenString.Trim(), NumberStyles.Integer, TypeConverterHelper.InvariantEnglishUS.NumberFormat, out var result))
				{
					if (0 <= result && result < PathParameters.Count)
					{
						object obj = PathParameters[result];
						if (obj != null)
						{
							array[i].value = obj;
							array[i].type = obj.GetType();
						}
						else if (throwOnError)
						{
							throw new InvalidOperationException(SR.Format(SR.PathParameterIsNull, result));
						}
					}
					else if (throwOnError)
					{
						throw new InvalidOperationException(SR.Format(SR.PathParametersIndexOutOfRange, result, PathParameters.Count));
					}
				}
				else
				{
					array[i].value = "(" + paramList[i].parenString + ")";
				}
				continue;
			}
			array[i].type = GetTypeFromName(paramList[i].parenString, context);
			if (array[i].type != null)
			{
				object typedParamValue = GetTypedParamValue(paramList[i].valueString.Trim(), array[i].type, throwOnError);
				if (typedParamValue != null)
				{
					array[i].value = typedParamValue;
					continue;
				}
				if (throwOnError)
				{
					throw new InvalidOperationException(SR.Format(SR.PropertyPathIndexWrongType, paramList[i].parenString, paramList[i].valueString));
				}
				array[i].type = null;
			}
			else
			{
				array[i].value = "(" + paramList[i].parenString + ")" + paramList[i].valueString;
			}
		}
		return array;
	}

	private object GetTypedParamValue(string param, Type type, bool throwOnError)
	{
		object obj = null;
		if (type == typeof(string))
		{
			return param;
		}
		TypeConverter converter = TypeDescriptor.GetConverter(type);
		if (converter != null && converter.CanConvertFrom(typeof(string)))
		{
			try
			{
				obj = converter.ConvertFromString(null, CultureInfo.InvariantCulture, param);
			}
			catch (Exception ex)
			{
				if (CriticalExceptions.IsCriticalApplicationException(ex) || throwOnError)
				{
					throw;
				}
			}
			catch
			{
				if (throwOnError)
				{
					throw;
				}
			}
		}
		if (obj == null && type.IsAssignableFrom(typeof(string)))
		{
			obj = param;
		}
		return obj;
	}

	private Type GetTypeFromName(string name, object context)
	{
		if (context is ParserContext parserContext)
		{
			int num = name.IndexOf(':');
			string text;
			if (num == -1)
			{
				text = string.Empty;
			}
			else
			{
				text = name.Substring(0, num).TrimEnd();
				name = name.Substring(num + 1).TrimStart();
			}
			string text2 = parserContext.XmlnsDictionary[text];
			if (text2 == null)
			{
				throw new ArgumentException(SR.Format(SR.ParserPrefixNSProperty, text, name));
			}
			return parserContext.XamlTypeMapper.GetTypeOnly(text2, name)?.ObjectType;
		}
		if (context is IServiceProvider && (context as IServiceProvider).GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver xamlTypeResolver)
		{
			return xamlTypeResolver.Resolve(name);
		}
		if (context is IValueSerializerContext context2)
		{
			ValueSerializer serializerFor = ValueSerializer.GetSerializerFor(typeof(Type), context2);
			if (serializerFor != null)
			{
				return serializerFor.ConvertFromString(name, context2) as Type;
			}
		}
		DependencyObject dependencyObject = context as DependencyObject;
		if (dependencyObject == null)
		{
			if (FrameworkCompatibilityPreferences.TargetsDesktop_V4_0)
			{
				return null;
			}
			dependencyObject = new DependencyObject();
		}
		return XamlReader.BamlSharedSchemaContext.ResolvePrefixedNameWithAdditionalWpfSemantics(name, dependencyObject);
	}

	internal static bool IsPropertyReference(string name)
	{
		if (name != null && name.Length > 1 && name[0] == '(')
		{
			return name[name.Length - 1] == ')';
		}
		return false;
	}

	internal static bool IsParameterIndex(string name, out int index)
	{
		if (IsPropertyReference(name))
		{
			ReadOnlySpan<char> s = name.AsSpan(1, name.Length - 2);
			return int.TryParse(s, NumberStyles.Integer, TypeConverterHelper.InvariantEnglishUS.NumberFormat, out index);
		}
		index = -1;
		return false;
	}

	private static bool IsValidAccessor(object accessor)
	{
		if (!(accessor is DependencyProperty) && !(accessor is PropertyInfo) && !(accessor is PropertyDescriptor))
		{
			return accessor is DynamicObjectAccessor;
		}
		return true;
	}

	private static string GetPropertyName(object accessor)
	{
		if (accessor is DependencyProperty dependencyProperty)
		{
			return dependencyProperty.Name;
		}
		PropertyInfo propertyInfo;
		if ((propertyInfo = accessor as PropertyInfo) != null)
		{
			return propertyInfo.Name;
		}
		if (accessor is PropertyDescriptor propertyDescriptor)
		{
			return propertyDescriptor.Name;
		}
		if (accessor is DynamicObjectAccessor dynamicObjectAccessor)
		{
			return dynamicObjectAccessor.PropertyName;
		}
		Invariant.Assert(condition: false, "Unknown accessor type");
		return null;
	}
}
