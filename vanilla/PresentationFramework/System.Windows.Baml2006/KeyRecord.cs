using System.Collections.Generic;
using System.Diagnostics;
using System.Xaml;

namespace System.Windows.Baml2006;

[DebuggerDisplay("{DebuggerString}")]
internal class KeyRecord
{
	private List<object> _resources;

	private object _data;

	private bool _shared;

	private bool _sharedSet;

	public bool Shared => _shared;

	public bool SharedSet => _sharedSet;

	public long ValuePosition { get; set; }

	public int ValueSize { get; set; }

	public byte Flags { get; set; }

	public List<object> StaticResources
	{
		get
		{
			if (_resources == null)
			{
				_resources = new List<object>();
			}
			return _resources;
		}
	}

	public bool HasStaticResources
	{
		get
		{
			if (_resources != null)
			{
				return _resources.Count > 0;
			}
			return false;
		}
	}

	public StaticResource LastStaticResource => StaticResources[StaticResources.Count - 1] as StaticResource;

	public string KeyString => _data as string;

	public Type KeyType => _data as Type;

	public XamlNodeList KeyNodeList => _data as XamlNodeList;

	public KeyRecord(bool shared, bool sharedSet, int valuePosition, Type keyType)
		: this(shared, sharedSet, valuePosition)
	{
		_data = keyType;
	}

	public KeyRecord(bool shared, bool sharedSet, int valuePosition, string keyString)
		: this(shared, sharedSet, valuePosition)
	{
		_data = keyString;
	}

	public KeyRecord(bool shared, bool sharedSet, int valuePosition, XamlSchemaContext context)
		: this(shared, sharedSet, valuePosition)
	{
		_data = new XamlNodeList(context, 8);
	}

	private KeyRecord(bool shared, bool sharedSet, int valuePosition)
	{
		_shared = shared;
		_sharedSet = sharedSet;
		ValuePosition = valuePosition;
	}
}
