using System.Reflection;
using System.Xaml.Schema;

namespace System.Windows.Baml2006;

internal class WpfKnownMemberInvoker : XamlMemberInvoker
{
	private WpfKnownMember _member;

	private bool _hasShouldSerializeMethodBeenLookedup;

	private MethodInfo _shouldSerializeMethod;

	public WpfKnownMemberInvoker(WpfKnownMember member)
		: base(member)
	{
		_member = member;
	}

	public override object GetValue(object instance)
	{
		if (_member.DependencyProperty != null)
		{
			return ((DependencyObject)instance).GetValue(_member.DependencyProperty);
		}
		return _member.GetDelegate(instance);
	}

	public override void SetValue(object instance, object value)
	{
		if (_member.DependencyProperty != null)
		{
			((DependencyObject)instance).SetValue(_member.DependencyProperty, value);
		}
		else
		{
			_member.SetDelegate(instance, value);
		}
	}

	public override ShouldSerializeResult ShouldSerializeValue(object instance)
	{
		if (!_hasShouldSerializeMethodBeenLookedup)
		{
			Type declaringType = _member.UnderlyingMember.DeclaringType;
			string name = "ShouldSerialize" + _member.Name;
			BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			Type[] types = new Type[1] { typeof(DependencyObject) };
			if (_member.IsAttachable)
			{
				_shouldSerializeMethod = declaringType.GetMethod(name, bindingFlags, null, types, null);
			}
			else
			{
				bindingFlags |= BindingFlags.Instance;
				_shouldSerializeMethod = declaringType.GetMethod(name, bindingFlags, null, types, null);
			}
			_hasShouldSerializeMethodBeenLookedup = true;
		}
		if (_shouldSerializeMethod != null)
		{
			object[] parameters = new object[1] { instance as DependencyObject };
			if (!((!_member.IsAttachable) ? ((bool)_shouldSerializeMethod.Invoke(instance, parameters)) : ((bool)_shouldSerializeMethod.Invoke(null, parameters))))
			{
				return ShouldSerializeResult.False;
			}
			return ShouldSerializeResult.True;
		}
		if (instance is DependencyObject dependencyObject && _member.DependencyProperty != null && !dependencyObject.ShouldSerializeProperty(_member.DependencyProperty))
		{
			return ShouldSerializeResult.False;
		}
		return base.ShouldSerializeValue(instance);
	}
}
