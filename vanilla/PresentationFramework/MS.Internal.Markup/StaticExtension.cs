using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace MS.Internal.Markup;

internal class StaticExtension : System.Windows.Markup.StaticExtension
{
	public StaticExtension()
	{
	}

	public StaticExtension(string member)
		: base(member)
	{
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (base.Member == null)
		{
			throw new InvalidOperationException(SR.MarkupExtensionStaticMember);
		}
		object systemResourceKey;
		if (base.MemberType != null)
		{
			systemResourceKey = SystemResourceKey.GetSystemResourceKey(base.MemberType.Name + "." + base.Member);
			if (systemResourceKey != null)
			{
				return systemResourceKey;
			}
		}
		else
		{
			systemResourceKey = SystemResourceKey.GetSystemResourceKey(base.Member);
			if (systemResourceKey != null)
			{
				return systemResourceKey;
			}
			int num = base.Member.IndexOf('.');
			if (num < 0)
			{
				throw new ArgumentException(SR.Format(SR.MarkupExtensionBadStatic, base.Member));
			}
			string text = base.Member.Substring(0, num);
			if (text == string.Empty)
			{
				throw new ArgumentException(SR.Format(SR.MarkupExtensionBadStatic, base.Member));
			}
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver xamlTypeResolver))
			{
				throw new ArgumentException(SR.Format(SR.MarkupExtensionNoContext, GetType().Name, "IXamlTypeResolver"));
			}
			base.MemberType = xamlTypeResolver.Resolve(text);
			base.Member = base.Member.Substring(num + 1, base.Member.Length - num - 1);
		}
		systemResourceKey = CommandConverter.GetKnownControlCommand(base.MemberType, base.Member);
		if (systemResourceKey != null)
		{
			return systemResourceKey;
		}
		return base.ProvideValue(serviceProvider);
	}
}
