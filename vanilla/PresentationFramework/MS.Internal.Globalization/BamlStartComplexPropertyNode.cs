using System.Windows;
using System.Windows.Markup;

namespace MS.Internal.Globalization;

internal class BamlStartComplexPropertyNode : BamlTreeNode, ILocalizabilityInheritable
{
	protected string _assemblyName;

	protected string _ownerTypeFullName;

	protected string _propertyName;

	private ILocalizabilityInheritable _localizabilityAncestor;

	private LocalizabilityAttribute _inheritableAttribute;

	private bool _isIgnored;

	internal string AssemblyName => _assemblyName;

	internal string PropertyName => _propertyName;

	internal string OwnerTypeFullName => _ownerTypeFullName;

	public ILocalizabilityInheritable LocalizabilityAncestor
	{
		get
		{
			return _localizabilityAncestor;
		}
		set
		{
			_localizabilityAncestor = value;
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

	internal BamlStartComplexPropertyNode(string assemblyName, string ownerTypeFullName, string propertyName)
		: base(BamlNodeType.StartComplexProperty)
	{
		_assemblyName = assemblyName;
		_ownerTypeFullName = ownerTypeFullName;
		_propertyName = propertyName;
	}

	internal override void Serialize(BamlWriter writer)
	{
		writer.WriteStartComplexProperty(_assemblyName, _ownerTypeFullName, _propertyName);
	}

	internal override BamlTreeNode Copy()
	{
		return new BamlStartComplexPropertyNode(_assemblyName, _ownerTypeFullName, _propertyName);
	}
}
