using System.Runtime.CompilerServices;
using MS.Internal.KnownBoxes;
using MS.Internal.WindowsBase;

namespace System.Windows;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class UncommonField<T>
{
	private T _defaultValue;

	private int _globalIndex;

	private bool _hasBeenSet;

	internal int GlobalIndex => _globalIndex;

	public UncommonField()
		: this(default(T))
	{
	}

	public UncommonField(T defaultValue)
	{
		_defaultValue = defaultValue;
		_hasBeenSet = false;
		lock (DependencyProperty.Synchronized)
		{
			_globalIndex = DependencyProperty.GetUniqueGlobalIndex(null, null);
			DependencyProperty.RegisteredPropertyList.Add();
		}
	}

	public void SetValue(DependencyObject instance, T value)
	{
		if (instance != null)
		{
			EntryIndex entryIndex = instance.LookupEntry(_globalIndex);
			if ((object)value != (object)_defaultValue)
			{
				instance.SetEffectiveValue(value: (!(typeof(T) == typeof(bool))) ? ((object)value) : BooleanBoxes.Box(Unsafe.As<T, bool>(ref value)), entryIndex: entryIndex, dp: null, targetIndex: _globalIndex, metadata: null, valueSource: BaseValueSourceInternal.Local);
				_hasBeenSet = true;
			}
			else
			{
				instance.UnsetEffectiveValue(entryIndex, null, null);
			}
			return;
		}
		throw new ArgumentNullException("instance");
	}

	public T GetValue(DependencyObject instance)
	{
		if (instance != null)
		{
			if (_hasBeenSet)
			{
				EntryIndex entryIndex = instance.LookupEntry(_globalIndex);
				if (entryIndex.Found)
				{
					object localValue = instance.EffectiveValues[entryIndex.Index].LocalValue;
					if (localValue != DependencyProperty.UnsetValue)
					{
						return (T)localValue;
					}
				}
				return _defaultValue;
			}
			return _defaultValue;
		}
		throw new ArgumentNullException("instance");
	}

	public void ClearValue(DependencyObject instance)
	{
		if (instance != null)
		{
			EntryIndex entryIndex = instance.LookupEntry(_globalIndex);
			instance.UnsetEffectiveValue(entryIndex, null, null);
			return;
		}
		throw new ArgumentNullException("instance");
	}
}
