using System;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace WinRT;

internal class BaseActivationFactory
{
	private ObjectReference<IActivationFactoryVftbl> _IActivationFactory;

	public BaseActivationFactory(string typeNamespace, string typeFullName)
	{
		string runtimeClassId = TypeExtensions.RemoveNamespacePrefix(typeFullName);
		int errorCode;
		(_IActivationFactory, errorCode) = WinrtModule.GetActivationFactory(runtimeClassId);
		if (_IActivationFactory != null)
		{
			return;
		}
		string text = typeNamespace;
		while (true)
		{
			try
			{
				(_IActivationFactory, _) = DllModule.Load(text + ".dll").GetActivationFactory(runtimeClassId);
				if (_IActivationFactory != null)
				{
					break;
				}
			}
			catch (Exception)
			{
			}
			int num = text.LastIndexOf('.');
			if (num <= 0)
			{
				Marshal.ThrowExceptionForHR(errorCode);
			}
			text = text.Remove(num);
		}
	}

	public ObjectReference<I> _ActivateInstance<I>()
	{
		Marshal.ThrowExceptionForHR(_IActivationFactory.Vftbl.ActivateInstance(_IActivationFactory.ThisPtr, out var instance));
		try
		{
			return ComWrappersSupport.GetObjectReferenceForInterface(instance).As<I>();
		}
		finally
		{
			MarshalInspectable.DisposeAbi(instance);
		}
	}

	public ObjectReference<I> _As<I>()
	{
		return _IActivationFactory.As<I>();
	}
}
