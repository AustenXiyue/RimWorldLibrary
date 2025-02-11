using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;

namespace System.Windows;

[TypeConverter(typeof(SystemKeyConverter))]
internal class SystemThemeKey : ResourceKey
{
	private SystemResourceKeyID _id;

	private static Assembly _presentationFrameworkAssembly;

	public override Assembly Assembly
	{
		get
		{
			if (_presentationFrameworkAssembly == null)
			{
				_presentationFrameworkAssembly = typeof(FrameworkElement).Assembly;
			}
			return _presentationFrameworkAssembly;
		}
	}

	internal SystemResourceKeyID InternalKey => _id;

	internal SystemThemeKey(SystemResourceKeyID id)
	{
		_id = id;
	}

	public override bool Equals(object o)
	{
		if (o is SystemThemeKey systemThemeKey)
		{
			return systemThemeKey._id == _id;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)_id;
	}

	public override string ToString()
	{
		return _id.ToString();
	}
}
