using System;
using System.Windows;
using MS.Internal.WindowsBase;

namespace MS.Internal;

internal static class InheritanceContextHelper
{
	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static void ProvideContextForObject(DependencyObject context, DependencyObject newValue)
	{
		context?.ProvideSelfAsInheritanceContext(newValue, null);
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static void RemoveContextFromObject(DependencyObject context, DependencyObject oldValue)
	{
		if (context != null && oldValue.InheritanceContext == context)
		{
			context.RemoveSelfAsInheritanceContext(oldValue, null);
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static void AddInheritanceContext(DependencyObject newInheritanceContext, DependencyObject value, ref bool hasMultipleInheritanceContexts, ref DependencyObject inheritanceContext)
	{
		if (newInheritanceContext != inheritanceContext && !hasMultipleInheritanceContexts)
		{
			if (inheritanceContext == null || newInheritanceContext == null)
			{
				inheritanceContext = newInheritanceContext;
			}
			else
			{
				hasMultipleInheritanceContexts = true;
				inheritanceContext = null;
			}
			value.OnInheritanceContextChanged(EventArgs.Empty);
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static void RemoveInheritanceContext(DependencyObject oldInheritanceContext, DependencyObject value, ref bool hasMultipleInheritanceContexts, ref DependencyObject inheritanceContext)
	{
		if (oldInheritanceContext == inheritanceContext && !hasMultipleInheritanceContexts)
		{
			inheritanceContext = null;
			value.OnInheritanceContextChanged(EventArgs.Empty);
		}
	}
}
