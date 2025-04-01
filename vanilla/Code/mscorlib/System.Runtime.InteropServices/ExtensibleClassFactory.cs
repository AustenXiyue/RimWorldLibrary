using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace System.Runtime.InteropServices;

/// <summary>Enables customization of managed objects that extend from unmanaged objects during creation.</summary>
[ComVisible(true)]
public sealed class ExtensibleClassFactory
{
	private static readonly Hashtable hashtable = new Hashtable();

	private ExtensibleClassFactory()
	{
	}

	internal static ObjectCreationDelegate GetObjectCreationCallback(Type t)
	{
		return hashtable[t] as ObjectCreationDelegate;
	}

	/// <summary>Registers a delegate that is called when an instance of a managed type, that extends from an unmanaged type, needs to allocate the aggregated unmanaged object.</summary>
	/// <param name="callback">A delegate that is called in place of CoCreateInstance. </param>
	public static void RegisterObjectCreationCallback(ObjectCreationDelegate callback)
	{
		int i = 1;
		for (StackTrace stackTrace = new StackTrace(fNeedFileInfo: false); i < stackTrace.FrameCount; i++)
		{
			MethodBase method = stackTrace.GetFrame(i).GetMethod();
			if (method.MemberType == MemberTypes.Constructor && method.IsStatic)
			{
				hashtable.Add(method.DeclaringType, callback);
				return;
			}
		}
		throw new InvalidOperationException("RegisterObjectCreationCallback must be called from .cctor of class derived from ComImport type.");
	}
}
