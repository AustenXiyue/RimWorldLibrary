using System.Diagnostics;

namespace System.Windows.Markup;

[DebuggerDisplay("Elem:{_typeFullName}")]
internal class XamlElementStartNode : XamlNode
{
	private string _assemblyName;

	private string _typeFullName;

	private Type _elementType;

	private Type _serializerType;

	private bool _isEmptyElement;

	private bool _needsDictionaryKey;

	private bool _useTypeConverter;

	private bool _isInjected;

	internal string AssemblyName => _assemblyName;

	internal string TypeFullName => _typeFullName;

	internal Type ElementType => _elementType;

	internal Type SerializerType => _serializerType;

	internal string SerializerTypeFullName
	{
		get
		{
			if (!(_serializerType == null))
			{
				return _serializerType.FullName;
			}
			return string.Empty;
		}
	}

	internal bool CreateUsingTypeConverter
	{
		get
		{
			return _useTypeConverter;
		}
		set
		{
			_useTypeConverter = value;
		}
	}

	internal bool IsInjected
	{
		get
		{
			return _isInjected;
		}
		set
		{
			_isInjected = value;
		}
	}

	internal XamlElementStartNode(int lineNumber, int linePosition, int depth, string assemblyName, string typeFullName, Type elementType, Type serializerType)
		: this(XamlNodeType.ElementStart, lineNumber, linePosition, depth, assemblyName, typeFullName, elementType, serializerType, isEmptyElement: false, needsDictionaryKey: false, isInjected: false)
	{
	}

	internal XamlElementStartNode(XamlNodeType tokenType, int lineNumber, int linePosition, int depth, string assemblyName, string typeFullName, Type elementType, Type serializerType, bool isEmptyElement, bool needsDictionaryKey, bool isInjected)
		: base(tokenType, lineNumber, linePosition, depth)
	{
		_assemblyName = assemblyName;
		_typeFullName = typeFullName;
		_elementType = elementType;
		_serializerType = serializerType;
		_isEmptyElement = isEmptyElement;
		_needsDictionaryKey = needsDictionaryKey;
		_useTypeConverter = false;
		IsInjected = isInjected;
	}
}
