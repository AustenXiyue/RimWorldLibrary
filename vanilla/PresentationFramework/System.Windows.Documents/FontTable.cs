using System.Collections;
using System.Globalization;
using Microsoft.Win32;

namespace System.Windows.Documents;

internal class FontTable : ArrayList
{
	private Hashtable _fontMappings;

	internal FontTableEntry CurrentEntry
	{
		get
		{
			if (Count == 0)
			{
				return null;
			}
			for (int num = Count - 1; num >= 0; num--)
			{
				FontTableEntry fontTableEntry = EntryAt(num);
				if (fontTableEntry.IsPending)
				{
					return fontTableEntry;
				}
			}
			return EntryAt(Count - 1);
		}
	}

	internal Hashtable FontMappings
	{
		get
		{
			if (_fontMappings == null)
			{
				_fontMappings = new Hashtable();
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\FontSubstitutes");
				if (registryKey != null)
				{
					string[] valueNames = registryKey.GetValueNames();
					foreach (string text in valueNames)
					{
						string text2 = (string)registryKey.GetValue(text);
						if (text.Length <= 0 || text2.Length <= 0)
						{
							continue;
						}
						string text3 = text;
						string text4 = string.Empty;
						string text5 = text2;
						string text6 = string.Empty;
						int num = text.IndexOf(',');
						if (num >= 0)
						{
							text3 = text.Substring(0, num);
							text4 = text.Substring(num + 1, text.Length - num - 1);
						}
						num = text2.IndexOf(',');
						if (num >= 0)
						{
							text5 = text2.Substring(0, num);
							text6 = text2.Substring(num + 1, text2.Length - num - 1);
						}
						if (text3.Length <= 0 || text5.Length <= 0)
						{
							continue;
						}
						bool flag = false;
						if (text4.Length > 0 && text6.Length > 0)
						{
							if (string.Compare(text4, text6, StringComparison.OrdinalIgnoreCase) == 0)
							{
								flag = true;
							}
						}
						else if (text4.Length == 0 && text6.Length == 0)
						{
							if (text3.Length > text5.Length && MemoryExtensions.Equals(text3.AsSpan(0, text5.Length), text5, StringComparison.OrdinalIgnoreCase))
							{
								flag = true;
							}
						}
						else if (text4.Length > 0 && text6.Length == 0)
						{
							flag = true;
						}
						if (flag)
						{
							string key = text3.ToLower(CultureInfo.InvariantCulture);
							if (_fontMappings[key] == null)
							{
								_fontMappings.Add(key, text5);
							}
						}
					}
				}
			}
			return _fontMappings;
		}
	}

	internal FontTable()
		: base(20)
	{
		_fontMappings = null;
	}

	internal FontTableEntry DefineEntry(int index)
	{
		FontTableEntry fontTableEntry = FindEntryByIndex(index);
		if (fontTableEntry != null)
		{
			fontTableEntry.IsPending = true;
			fontTableEntry.Name = null;
			return fontTableEntry;
		}
		fontTableEntry = new FontTableEntry();
		fontTableEntry.Index = index;
		Add(fontTableEntry);
		return fontTableEntry;
	}

	internal FontTableEntry FindEntryByIndex(int index)
	{
		for (int i = 0; i < Count; i++)
		{
			FontTableEntry fontTableEntry = EntryAt(i);
			if (fontTableEntry.Index == index)
			{
				return fontTableEntry;
			}
		}
		return null;
	}

	internal FontTableEntry FindEntryByName(string name)
	{
		for (int i = 0; i < Count; i++)
		{
			FontTableEntry fontTableEntry = EntryAt(i);
			if (name.Equals(fontTableEntry.Name))
			{
				return fontTableEntry;
			}
		}
		return null;
	}

	internal FontTableEntry EntryAt(int index)
	{
		return (FontTableEntry)this[index];
	}

	internal int DefineEntryByName(string name)
	{
		int num = -1;
		for (int i = 0; i < Count; i++)
		{
			FontTableEntry fontTableEntry = EntryAt(i);
			if (name.Equals(fontTableEntry.Name))
			{
				return fontTableEntry.Index;
			}
			if (fontTableEntry.Index > num)
			{
				num = fontTableEntry.Index;
			}
		}
		FontTableEntry fontTableEntry2 = new FontTableEntry();
		fontTableEntry2.Index = num + 1;
		Add(fontTableEntry2);
		fontTableEntry2.Name = name;
		return num + 1;
	}

	internal void MapFonts()
	{
		Hashtable fontMappings = FontMappings;
		for (int i = 0; i < Count; i++)
		{
			FontTableEntry fontTableEntry = EntryAt(i);
			if (fontTableEntry.Name == null)
			{
				continue;
			}
			string text = (string)fontMappings[fontTableEntry.Name.ToLower(CultureInfo.InvariantCulture)];
			if (text != null)
			{
				fontTableEntry.Name = text;
				continue;
			}
			int num = fontTableEntry.Name.IndexOf('(');
			if (num >= 0)
			{
				while (num > 0 && fontTableEntry.Name[num - 1] == ' ')
				{
					num--;
				}
				fontTableEntry.Name = fontTableEntry.Name.Substring(0, num);
			}
		}
	}
}
