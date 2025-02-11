namespace System.Windows.Markup;

internal class ReaderContextStackData
{
	private ReaderFlags _contextFlags;

	private object _contextData;

	private object _contextKey;

	private string _uid;

	private string _name;

	private object _contentProperty;

	private Type _expectedType;

	private short _expectedTypeId;

	private bool _createUsingTypeConverter;

	internal ReaderFlags ContextType => _contextFlags & ReaderFlags.ContextTypeMask;

	internal object ObjectData
	{
		get
		{
			return _contextData;
		}
		set
		{
			_contextData = value;
		}
	}

	internal object Key
	{
		get
		{
			return _contextKey;
		}
		set
		{
			_contextKey = value;
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

	internal string ElementNameOrPropertyName
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	internal object ContentProperty
	{
		get
		{
			return _contentProperty;
		}
		set
		{
			_contentProperty = value;
		}
	}

	internal Type ExpectedType
	{
		get
		{
			return _expectedType;
		}
		set
		{
			_expectedType = value;
		}
	}

	internal short ExpectedTypeId
	{
		get
		{
			return _expectedTypeId;
		}
		set
		{
			_expectedTypeId = value;
		}
	}

	internal bool CreateUsingTypeConverter
	{
		get
		{
			return _createUsingTypeConverter;
		}
		set
		{
			_createUsingTypeConverter = value;
		}
	}

	internal ReaderFlags ContextFlags
	{
		get
		{
			return _contextFlags;
		}
		set
		{
			_contextFlags = value;
		}
	}

	internal bool NeedToAddToTree => CheckFlag(ReaderFlags.NeedToAddToTree);

	internal bool IsObjectElement
	{
		get
		{
			if (ContextType != ReaderFlags.DependencyObject)
			{
				return ContextType == ReaderFlags.ClrObject;
			}
			return true;
		}
	}

	internal void MarkAddedToTree()
	{
		ContextFlags = (ContextFlags | ReaderFlags.AddedToTree) & (ReaderFlags)65534;
	}

	internal bool CheckFlag(ReaderFlags flag)
	{
		return (ContextFlags & flag) == flag;
	}

	internal void SetFlag(ReaderFlags flag)
	{
		ContextFlags |= flag;
	}

	internal void ClearFlag(ReaderFlags flag)
	{
		ContextFlags &= (ReaderFlags)(ushort)(~(int)flag);
	}

	internal void ClearData()
	{
		_contextFlags = ReaderFlags.Unknown;
		_contextData = null;
		_contextKey = null;
		_contentProperty = null;
		_expectedType = null;
		_expectedTypeId = 0;
		_createUsingTypeConverter = false;
		_uid = null;
		_name = null;
	}
}
