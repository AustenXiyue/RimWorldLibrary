using System;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace MS.Internal.AppModel;

[Serializable]
internal class ReturnEventSaver
{
	private ReturnEventSaverInfo[] _returnList;

	internal ReturnEventSaver()
	{
	}

	internal void _Detach(PageFunctionBase pf)
	{
		if ((object)pf._Return != null && pf._Saver == null)
		{
			ReturnEventSaverInfo[] array = null;
			Delegate[] array2 = null;
			array2 = pf._Return.GetInvocationList();
			array = (_returnList = new ReturnEventSaverInfo[array2.Length]);
			for (int i = 0; i < array2.Length; i++)
			{
				Delegate @delegate = array2[i];
				bool fSamePf = false;
				if (@delegate.Target == pf)
				{
					fSamePf = true;
				}
				MethodInfo method = @delegate.Method;
				ReturnEventSaverInfo returnEventSaverInfo = new ReturnEventSaverInfo(@delegate.GetType().AssemblyQualifiedName, @delegate.Target.GetType().AssemblyQualifiedName, method.Name, fSamePf);
				array[i] = returnEventSaverInfo;
			}
			pf._Saver = this;
		}
		pf._DetachEvents();
	}

	internal void _Attach(object caller, PageFunctionBase child)
	{
		ReturnEventSaverInfo[] array = null;
		array = _returnList;
		if (array == null)
		{
			return;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (string.Compare(_returnList[i]._targetTypeName, caller.GetType().AssemblyQualifiedName, StringComparison.Ordinal) != 0)
			{
				throw new NotSupportedException(SR.ReturnEventHandlerMustBeOnParentPage);
			}
			Delegate d;
			try
			{
				d = Delegate.CreateDelegate(Type.GetType(_returnList[i]._delegateTypeName), caller, _returnList[i]._delegateMethodName);
			}
			catch (Exception innerException)
			{
				throw new NotSupportedException(SR.ReturnEventHandlerMustBeOnParentPage, innerException);
			}
			child._AddEventHandler(d);
		}
	}
}
