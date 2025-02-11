using System;
using System.Windows;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal class FreezableDefaultValueFactory : DefaultValueFactory
{
	private class FreezableDefaultPromoter
	{
		private readonly DependencyObject _owner;

		private readonly DependencyProperty _property;

		private Freezable _mutableDefaultValue;

		internal FreezableDefaultPromoter(DependencyObject owner, DependencyProperty property)
		{
			_owner = owner;
			_property = property;
		}

		internal void OnDefaultValueChanged(object sender, EventArgs e)
		{
			_property.GetMetadata(_owner.DependencyObjectType).ClearCachedDefaultValue(_owner, _property);
			if (!_mutableDefaultValue.IsFrozen)
			{
				_mutableDefaultValue.Changed -= OnDefaultValueChanged;
			}
			if (_owner.ReadLocalValue(_property) == DependencyProperty.UnsetValue)
			{
				_owner.SetMutableDefaultValue(_property, _mutableDefaultValue);
			}
		}

		internal void SetFreezableDefaultValue(Freezable mutableDefaultValue)
		{
			_mutableDefaultValue = mutableDefaultValue;
		}
	}

	private readonly Freezable _defaultValuePrototype;

	internal override object DefaultValue => _defaultValuePrototype;

	internal FreezableDefaultValueFactory(Freezable defaultValue)
	{
		_defaultValuePrototype = defaultValue.GetAsFrozen();
	}

	internal override object CreateDefaultValue(DependencyObject owner, DependencyProperty property)
	{
		Freezable defaultValuePrototype = _defaultValuePrototype;
		if (owner is Freezable { IsFrozen: not false })
		{
			return defaultValuePrototype;
		}
		defaultValuePrototype = _defaultValuePrototype.Clone();
		FreezableDefaultPromoter freezableDefaultPromoter = new FreezableDefaultPromoter(owner, property);
		freezableDefaultPromoter.SetFreezableDefaultValue(defaultValuePrototype);
		defaultValuePrototype.Changed += freezableDefaultPromoter.OnDefaultValueChanged;
		return defaultValuePrototype;
	}
}
