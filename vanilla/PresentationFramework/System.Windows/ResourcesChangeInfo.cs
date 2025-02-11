using System.Collections.Generic;

namespace System.Windows;

internal struct ResourcesChangeInfo
{
	private enum PrivateFlags : byte
	{
		IsThemeChange = 1,
		IsTreeChange = 2,
		IsStyleResourceChange = 4,
		IsTemplateResourceChange = 8,
		IsSysColorsOrSettingsChange = 0x10,
		IsCatastrophicDictionaryChange = 0x20,
		IsImplicitDataTemplateChange = 0x40
	}

	private List<ResourceDictionary> _oldDictionaries;

	private List<ResourceDictionary> _newDictionaries;

	private object _key;

	private DependencyObject _container;

	private PrivateFlags _flags;

	internal static ResourcesChangeInfo ThemeChangeInfo
	{
		get
		{
			ResourcesChangeInfo result = default(ResourcesChangeInfo);
			result.IsThemeChange = true;
			return result;
		}
	}

	internal static ResourcesChangeInfo TreeChangeInfo
	{
		get
		{
			ResourcesChangeInfo result = default(ResourcesChangeInfo);
			result.IsTreeChange = true;
			return result;
		}
	}

	internal static ResourcesChangeInfo SysColorsOrSettingsChangeInfo
	{
		get
		{
			ResourcesChangeInfo result = default(ResourcesChangeInfo);
			result.IsSysColorsOrSettingsChange = true;
			return result;
		}
	}

	internal static ResourcesChangeInfo CatastrophicDictionaryChangeInfo
	{
		get
		{
			ResourcesChangeInfo result = default(ResourcesChangeInfo);
			result.IsCatastrophicDictionaryChange = true;
			return result;
		}
	}

	internal bool IsThemeChange
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsThemeChange);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.IsThemeChange, value);
		}
	}

	internal bool IsTreeChange
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsTreeChange);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.IsTreeChange, value);
		}
	}

	internal bool IsStyleResourcesChange
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsStyleResourceChange);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.IsStyleResourceChange, value);
		}
	}

	internal bool IsTemplateResourcesChange
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsTemplateResourceChange);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.IsTemplateResourceChange, value);
		}
	}

	internal bool IsSysColorsOrSettingsChange
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsSysColorsOrSettingsChange);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.IsSysColorsOrSettingsChange, value);
		}
	}

	internal bool IsCatastrophicDictionaryChange
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsCatastrophicDictionaryChange);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.IsCatastrophicDictionaryChange, value);
		}
	}

	internal bool IsImplicitDataTemplateChange
	{
		get
		{
			return ReadPrivateFlag(PrivateFlags.IsImplicitDataTemplateChange);
		}
		set
		{
			WritePrivateFlag(PrivateFlags.IsImplicitDataTemplateChange, value);
		}
	}

	internal bool IsResourceAddOperation
	{
		get
		{
			if (_key == null)
			{
				if (_newDictionaries != null)
				{
					return _newDictionaries.Count > 0;
				}
				return false;
			}
			return true;
		}
	}

	internal DependencyObject Container => _container;

	internal ResourcesChangeInfo(object key)
	{
		_oldDictionaries = null;
		_newDictionaries = null;
		_key = key;
		_container = null;
		_flags = (PrivateFlags)0;
	}

	internal ResourcesChangeInfo(ResourceDictionary oldDictionary, ResourceDictionary newDictionary)
	{
		_oldDictionaries = null;
		if (oldDictionary != null)
		{
			_oldDictionaries = new List<ResourceDictionary>(1);
			_oldDictionaries.Add(oldDictionary);
		}
		_newDictionaries = null;
		if (newDictionary != null)
		{
			_newDictionaries = new List<ResourceDictionary>(1);
			_newDictionaries.Add(newDictionary);
		}
		_key = null;
		_container = null;
		_flags = (PrivateFlags)0;
	}

	internal ResourcesChangeInfo(List<ResourceDictionary> oldDictionaries, List<ResourceDictionary> newDictionaries, bool isStyleResourcesChange, bool isTemplateResourcesChange, DependencyObject container)
	{
		_oldDictionaries = oldDictionaries;
		_newDictionaries = newDictionaries;
		_key = null;
		_container = container;
		_flags = (PrivateFlags)0;
		IsStyleResourcesChange = isStyleResourcesChange;
		IsTemplateResourcesChange = isTemplateResourcesChange;
	}

	internal bool Contains(object key, bool isImplicitStyleKey)
	{
		if (IsTreeChange || IsCatastrophicDictionaryChange)
		{
			return true;
		}
		if (IsThemeChange || IsSysColorsOrSettingsChange)
		{
			return !isImplicitStyleKey;
		}
		if (_key != null && object.Equals(_key, key))
		{
			return true;
		}
		if (_oldDictionaries != null)
		{
			for (int i = 0; i < _oldDictionaries.Count; i++)
			{
				if (_oldDictionaries[i].Contains(key))
				{
					return true;
				}
			}
		}
		if (_newDictionaries != null)
		{
			for (int j = 0; j < _newDictionaries.Count; j++)
			{
				if (_newDictionaries[j].Contains(key))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal void SetIsImplicitDataTemplateChange()
	{
		bool flag = IsCatastrophicDictionaryChange || _key is DataTemplateKey;
		if (!flag && _oldDictionaries != null)
		{
			foreach (ResourceDictionary oldDictionary in _oldDictionaries)
			{
				if (oldDictionary.HasImplicitDataTemplates)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag && _newDictionaries != null)
		{
			foreach (ResourceDictionary newDictionary in _newDictionaries)
			{
				if (newDictionary.HasImplicitDataTemplates)
				{
					flag = true;
					break;
				}
			}
		}
		IsImplicitDataTemplateChange = flag;
	}

	private void WritePrivateFlag(PrivateFlags bit, bool value)
	{
		if (value)
		{
			_flags |= bit;
		}
		else
		{
			_flags &= (PrivateFlags)(byte)(~(int)bit);
		}
	}

	private bool ReadPrivateFlag(PrivateFlags bit)
	{
		return (_flags & bit) != 0;
	}
}
