namespace System.Diagnostics.Tracing;

public struct EventSourceOptions
{
	internal EventKeywords keywords;

	internal EventTags tags;

	internal EventActivityOptions activityOptions;

	internal byte level;

	internal byte opcode;

	internal byte valuesSet;

	internal const byte keywordsSet = 1;

	internal const byte tagsSet = 2;

	internal const byte levelSet = 4;

	internal const byte opcodeSet = 8;

	internal const byte activityOptionsSet = 16;

	public EventLevel Level
	{
		get
		{
			return (EventLevel)level;
		}
		set
		{
			level = checked((byte)value);
			valuesSet |= 4;
		}
	}

	public EventOpcode Opcode
	{
		get
		{
			return (EventOpcode)opcode;
		}
		set
		{
			opcode = checked((byte)value);
			valuesSet |= 8;
		}
	}

	internal bool IsOpcodeSet => (valuesSet & 8) != 0;

	public EventKeywords Keywords
	{
		get
		{
			return keywords;
		}
		set
		{
			keywords = value;
			valuesSet |= 1;
		}
	}

	public EventTags Tags
	{
		get
		{
			return tags;
		}
		set
		{
			tags = value;
			valuesSet |= 2;
		}
	}

	public EventActivityOptions ActivityOptions
	{
		get
		{
			return activityOptions;
		}
		set
		{
			activityOptions = value;
			valuesSet |= 16;
		}
	}
}
