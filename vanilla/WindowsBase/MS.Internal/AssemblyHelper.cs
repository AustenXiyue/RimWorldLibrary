using System;
using System.Reflection;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal static class AssemblyHelper
{
	private struct AssemblyRecord
	{
		public string Name { get; set; }

		public bool IsLoaded { get; set; }
	}

	private static SystemDrawingExtensionMethods _systemDrawingExtensionMethods;

	private static SystemXmlExtensionMethods _systemXmlExtensionMethods;

	private static SystemXmlLinqExtensionMethods _systemXmlLinqExtensionMethods;

	private static SystemDataExtensionMethods _systemDataExtensionMethods;

	private static SystemCoreExtensionMethods _systemCoreExtensionMethods;

	private static readonly AssemblyRecord[] _records;

	static AssemblyHelper()
	{
		_records = new AssemblyRecord[5]
		{
			new AssemblyRecord
			{
				Name = "System.Drawing.Common,"
			},
			new AssemblyRecord
			{
				Name = "System.Private.Xml,"
			},
			new AssemblyRecord
			{
				Name = "System.Private.Xml.Linq,"
			},
			new AssemblyRecord
			{
				Name = "System.Data.Common,"
			},
			new AssemblyRecord
			{
				Name = "System.Linq.Expressions,"
			}
		};
		AppDomain currentDomain = AppDomain.CurrentDomain;
		currentDomain.AssemblyLoad += OnAssemblyLoad;
		Assembly[] assemblies = currentDomain.GetAssemblies();
		for (int num = assemblies.Length - 1; num >= 0; num--)
		{
			OnLoaded(assemblies[num]);
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static bool IsLoaded(UncommonAssembly assemblyEnum)
	{
		return _records[(int)assemblyEnum].IsLoaded;
	}

	internal static SystemDrawingExtensionMethods ExtensionsForSystemDrawing(bool force = false)
	{
		if (_systemDrawingExtensionMethods == null && (force || IsLoaded(UncommonAssembly.System_Drawing_Common)))
		{
			_systemDrawingExtensionMethods = (SystemDrawingExtensionMethods)LoadExtensionFor("SystemDrawing");
		}
		return _systemDrawingExtensionMethods;
	}

	internal static SystemXmlExtensionMethods ExtensionsForSystemXml(bool force = false)
	{
		if (_systemXmlExtensionMethods == null && (force || IsLoaded(UncommonAssembly.System_Private_Xml)))
		{
			_systemXmlExtensionMethods = (SystemXmlExtensionMethods)LoadExtensionFor("SystemXml");
		}
		return _systemXmlExtensionMethods;
	}

	internal static SystemXmlLinqExtensionMethods ExtensionsForSystemXmlLinq(bool force = false)
	{
		if (_systemXmlLinqExtensionMethods == null && (force || IsLoaded(UncommonAssembly.System_Private_Xml_Linq)))
		{
			_systemXmlLinqExtensionMethods = (SystemXmlLinqExtensionMethods)LoadExtensionFor("SystemXmlLinq");
		}
		return _systemXmlLinqExtensionMethods;
	}

	internal static SystemDataExtensionMethods ExtensionsForSystemData(bool force = false)
	{
		if (_systemDataExtensionMethods == null && (force || IsLoaded(UncommonAssembly.System_Data_Common)))
		{
			_systemDataExtensionMethods = (SystemDataExtensionMethods)LoadExtensionFor("SystemData");
		}
		return _systemDataExtensionMethods;
	}

	internal static SystemCoreExtensionMethods ExtensionsForSystemCore(bool force = false)
	{
		if (_systemCoreExtensionMethods == null && (force || IsLoaded(UncommonAssembly.System_Linq_Expressions)))
		{
			_systemCoreExtensionMethods = (SystemCoreExtensionMethods)LoadExtensionFor("SystemCore");
		}
		return _systemCoreExtensionMethods;
	}

	private static object LoadExtensionFor(string name)
	{
		string text = Assembly.GetExecutingAssembly().FullName.Replace("WindowsBase", "PresentationFramework-" + name).Replace("31bf3856ad364e35", "b77a5c561934e089");
		string text2 = "MS.Internal." + name + "Extension";
		object result = null;
		Type type = Type.GetType(text2 + ", " + text, throwOnError: false);
		if (type != null)
		{
			result = Activator.CreateInstance(type);
		}
		return result;
	}

	private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
	{
		OnLoaded(args.LoadedAssembly);
	}

	private static void OnLoaded(Assembly assembly)
	{
		if (assembly.ReflectionOnly)
		{
			return;
		}
		for (int num = _records.Length - 1; num >= 0; num--)
		{
			if (!_records[num].IsLoaded && assembly.FullName.StartsWith(_records[num].Name, StringComparison.OrdinalIgnoreCase))
			{
				_records[num].IsLoaded = true;
			}
		}
	}
}
