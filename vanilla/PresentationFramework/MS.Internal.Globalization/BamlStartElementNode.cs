using System.Windows;
using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal sealed class BamlStartElementNode : BamlTreeNode, ILocalizabilityInheritable
{
	private string _assemblyName;

	private string _typeFullName;

	private string _content;

	private string _uid;

	private LocalizabilityAttribute _inheritableAttribute;

	private ILocalizabilityInheritable _localizabilityAncestor;

	private bool _isIgnored;

	private bool _isInjected;

	private bool _useTypeConverter;

	internal string AssemblyName => _assemblyName;

	internal string TypeFullName => _typeFullName;

	internal string Content
	{
		get
		{
			return _content;
		}
		set
		{
			_content = value;
		}
	}

	internal string Uid
	{
		get
		{
			return _uid;
		}
		set
		{
			_uid = value;
		}
	}

	public ILocalizabilityInheritable LocalizabilityAncestor
	{
		get
		{
			if (_localizabilityAncestor == null)
			{
				BamlTreeNode parent = base.Parent;
				while (_localizabilityAncestor == null && parent != null)
				{
					_localizabilityAncestor = parent as ILocalizabilityInheritable;
					parent = parent.Parent;
				}
			}
			return _localizabilityAncestor;
		}
	}

	public LocalizabilityAttribute InheritableAttribute
	{
		get
		{
			return _inheritableAttribute;
		}
		set
		{
			_inheritableAttribute = value;
		}
	}

	public bool IsIgnored
	{
		get
		{
			return _isIgnored;
		}
		set
		{
			_isIgnored = value;
		}
	}

	internal BamlStartElementNode(string assemblyName, string typeFullName, bool isInjected, bool useTypeConverter)
		: base(BamlNodeType.StartElement)
	{
		_assemblyName = assemblyName;
		_typeFullName = typeFullName;
		_isInjected = isInjected;
		_useTypeConverter = useTypeConverter;
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteStartElement(_assemblyName, _typeFullName, _isInjected, _useTypeConverter);
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlStartElementNode(_assemblyName, _typeFullName, _isInjected, _useTypeConverter)
		{
			_content = _content,
			_uid = _uid,
			_inheritableAttribute = _inheritableAttribute
		};
	}

	internal void InsertProperty(BamlTreeNode child)
	{
		if (_children == null)
		{
			AddChild(child);
			return;
		}
		int index = 0;
		for (int i = 0; i < _children.Count; i++)
		{
			if (_children[i].NodeType == BamlNodeType.Property)
			{
				index = i;
			}
		}
		_children.Insert(index, child);
		child.Parent = this;
	}
}
