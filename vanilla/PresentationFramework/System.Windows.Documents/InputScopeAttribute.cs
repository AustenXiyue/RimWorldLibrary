using System.Runtime.InteropServices;
using System.Windows.Input;
using MS.Win32;

namespace System.Windows.Documents;

internal class InputScopeAttribute : MS.Win32.UnsafeNativeMethods.ITfInputScope
{
	private InputScope _inputScope;

	internal InputScopeAttribute(InputScope inputscope)
	{
		_inputScope = inputscope;
	}

	public void GetInputScopes(out nint ppinputscopes, out int count)
	{
		if (_inputScope != null)
		{
			int num = 0;
			count = _inputScope.Names.Count;
			try
			{
				ppinputscopes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * count);
			}
			catch (OutOfMemoryException)
			{
				throw new COMException(SR.InputScopeAttribute_E_OUTOFMEMORY, -2147024882);
			}
			for (int i = 0; i < count; i++)
			{
				Marshal.WriteInt32(ppinputscopes, num, (int)((InputScopeName)_inputScope.Names[i]).NameValue);
				num += Marshal.SizeOf(typeof(int));
			}
		}
		else
		{
			ppinputscopes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)));
			Marshal.WriteInt32(ppinputscopes, 0);
			count = 1;
		}
	}

	public int GetPhrase(out nint ppbstrPhrases, out int count)
	{
		count = ((_inputScope != null) ? _inputScope.PhraseList.Count : 0);
		try
		{
			ppbstrPhrases = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(nint)) * count);
		}
		catch (OutOfMemoryException)
		{
			throw new COMException(SR.InputScopeAttribute_E_OUTOFMEMORY, -2147024882);
		}
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			nint val;
			try
			{
				val = Marshal.StringToBSTR(((InputScopePhrase)_inputScope.PhraseList[i]).Name);
			}
			catch (OutOfMemoryException)
			{
				num = 0;
				for (int j = 0; j < i; j++)
				{
					Marshal.FreeBSTR(Marshal.ReadIntPtr(ppbstrPhrases, num));
					num += Marshal.SizeOf(typeof(nint));
				}
				throw new COMException(SR.InputScopeAttribute_E_OUTOFMEMORY, -2147024882);
			}
			Marshal.WriteIntPtr(ppbstrPhrases, num, val);
			num += Marshal.SizeOf(typeof(nint));
		}
		return (count <= 0) ? 1 : 0;
	}

	public int GetRegularExpression(out string desc)
	{
		desc = null;
		if (_inputScope != null)
		{
			desc = _inputScope.RegularExpression;
		}
		return (desc == null) ? 1 : 0;
	}

	public int GetSRGC(out string desc)
	{
		desc = null;
		if (_inputScope != null)
		{
			desc = _inputScope.SrgsMarkup;
		}
		return (desc == null) ? 1 : 0;
	}

	public int GetXML(out string desc)
	{
		desc = null;
		return 1;
	}
}
