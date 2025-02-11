using System.Reflection;
using System.Xaml.Schema;

namespace System.Windows.Baml2006;

internal class WpfMemberInvoker : XamlMemberInvoker
{
	private WpfXamlMember _member;

	private bool _hasShouldSerializeMethodBeenLookedup;

	private MethodInfo _shouldSerializeMethod;

	public WpfMemberInvoker(WpfXamlMember member)
		: base(member)
	{
		_member = member;
	}

	public override void SetValue(object instance, object value)
	{
		if (instance is DependencyObject dependencyObject)
		{
			if (_member.DependencyProperty != null)
			{
				dependencyObject.SetValue(_member.DependencyProperty, value);
				return;
			}
			if (_member.RoutedEvent != null && value is Delegate handler)
			{
				UIElement.AddHandler(dependencyObject, _member.RoutedEvent, handler);
				return;
			}
		}
		base.SetValue(instance, value);
	}

	public override object GetValue(object instance)
	{
		if (instance is DependencyObject dependencyObject && _member.DependencyProperty != null)
		{
			object value = dependencyObject.GetValue(_member.DependencyProperty);
			if (value != null)
			{
				return value;
			}
			if (!_member.ApplyGetterFallback || _member.UnderlyingMember == null)
			{
				return value;
			}
		}
		return base.GetValue(instance);
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
