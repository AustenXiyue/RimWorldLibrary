namespace System.Collections.Specialized;

[Serializable]
internal class StringDictionaryWithComparer : StringDictionary
{
	public override string this[string key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			return (string)contents[key];
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			contents[key] = value;
		}
	}

	public StringDictionaryWithComparer()
		: this(StringComparer.OrdinalIgnoreCase)
	{
	}

	public StringDictionaryWithComparer(IEqualityComparer comparer)
	{
		ReplaceHashtable(new Hashtable(comparer));
	}

	public override void Add(string key, string value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		contents.Add(key, value);
	}

	public override bool ContainsKey(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		return contents.ContainsKey(key);
	}

	public override void Remove(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		contents.Remove(key);
	}
}
