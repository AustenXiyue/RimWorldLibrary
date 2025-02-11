using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xaml;

namespace System.Windows.Diagnostics;

internal static class XamlSourceInfoHelper
{
	public const string XamlSourceInfoEnvironmentVariable = "ENABLE_XAML_DIAGNOSTICS_SOURCE_INFO";

	private static ConditionalWeakTable<object, XamlSourceInfo> s_sourceInfoTable;

	private static readonly object s_lock;

	private static PropertyInfo s_sourceBamlUriProperty;

	private static PropertyInfo s_elementLineNumberProperty;

	private static PropertyInfo s_elementLinePositionProperty;

	internal static bool IsXamlSourceInfoEnabled => s_sourceInfoTable != null;

	static XamlSourceInfoHelper()
	{
		s_lock = new object();
		InitializeEnableXamlSourceInfo(null);
	}

	private static void InitializeEnableXamlSourceInfo(string value)
	{
		if (VisualDiagnostics.IsEnabled && VisualDiagnostics.IsEnvironmentVariableSet(value, "ENABLE_XAML_DIAGNOSTICS_SOURCE_INFO") && InitializeXamlObjectEventArgs())
		{
			s_sourceInfoTable = new ConditionalWeakTable<object, XamlSourceInfo>();
		}
		else
		{
			s_sourceInfoTable = null;
		}
	}

	private static bool InitializeXamlObjectEventArgs()
	{
		Type? typeFromHandle = typeof(XamlObjectEventArgs);
		s_sourceBamlUriProperty = typeFromHandle.GetProperty("SourceBamlUri", BindingFlags.Instance | BindingFlags.Public);
		s_elementLineNumberProperty = typeFromHandle.GetProperty("ElementLineNumber", BindingFlags.Instance | BindingFlags.Public);
		s_elementLinePositionProperty = typeFromHandle.GetProperty("ElementLinePosition", BindingFlags.Instance | BindingFlags.Public);
		if (s_sourceBamlUriProperty == null || s_sourceBamlUriProperty.PropertyType != typeof(Uri) || s_elementLineNumberProperty == null || s_elementLineNumberProperty.PropertyType != typeof(int) || s_elementLinePositionProperty == null || s_elementLinePositionProperty.PropertyType != typeof(int))
		{
			return false;
		}
		return true;
	}

	internal static void SetXamlSourceInfo(object obj, XamlObjectEventArgs args, Uri overrideSourceUri)
	{
		if (s_sourceInfoTable != null && args != null)
		{
			Uri sourceUri = overrideSourceUri ?? ((Uri)s_sourceBamlUriProperty.GetValue(args));
			int elementLineNumber = (int)s_elementLineNumberProperty.GetValue(args);
			int elementLinePosition = (int)s_elementLinePositionProperty.GetValue(args);
			SetXamlSourceInfo(obj, sourceUri, elementLineNumber, elementLinePosition);
		}
	}

	internal static void SetXamlSourceInfo(object obj, Uri sourceUri, int elementLineNumber, int elementLinePosition)
	{
		if (s_sourceInfoTable != null && obj != null && (elementLineNumber != 0 || elementLinePosition != 0) && !(obj is string) && !obj.GetType().IsValueType)
		{
			lock (s_lock)
			{
				s_sourceInfoTable.Remove(obj);
				s_sourceInfoTable.Add(obj, new XamlSourceInfo(sourceUri, elementLineNumber, elementLinePosition));
			}
		}
	}

	internal static XamlSourceInfo GetXamlSourceInfo(object obj)
	{
		XamlSourceInfo value = null;
		if (s_sourceInfoTable != null && obj != null && s_sourceInfoTable.TryGetValue(obj, out value))
		{
			return value;
		}
		return null;
	}
}
