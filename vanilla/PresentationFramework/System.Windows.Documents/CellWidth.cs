namespace System.Windows.Documents;

internal class CellWidth
{
	private WidthType _type;

	private long _value;

	internal WidthType Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	internal long Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	internal CellWidth()
	{
		Type = WidthType.WidthAuto;
		Value = 0L;
	}

	internal CellWidth(CellWidth cw)
	{
		Type = cw.Type;
		Value = cw.Value;
	}

	internal void SetDefaults()
	{
		Type = WidthType.WidthAuto;
		Value = 0L;
	}
}
