using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MS.Internal.Ink;

internal static class HighContrastHelper
{
	private delegate void UpdateHighContrastCallback(HighContrastCallback highContrastCallback);

	private static readonly object __lock;

	private static List<WeakReference> __highContrastCallbackList;

	private static int __increaseCount;

	private const int CleanTolerance = 100;

	static HighContrastHelper()
	{
		__lock = new object();
		__highContrastCallbackList = new List<WeakReference>();
		__increaseCount = 0;
	}

	internal static void RegisterHighContrastCallback(HighContrastCallback highContrastCallback)
	{
		lock (__lock)
		{
			int count = __highContrastCallbackList.Count;
			int i = 0;
			int num = 0;
			if (__increaseCount > 100)
			{
				for (; i < count; i++)
				{
					if (__highContrastCallbackList[num].IsAlive)
					{
						num++;
					}
					else
					{
						__highContrastCallbackList.RemoveAt(num);
					}
				}
				__increaseCount = 0;
			}
			__highContrastCallbackList.Add(new WeakReference(highContrastCallback));
			__increaseCount++;
		}
	}

	internal static void OnSettingChanged()
	{
		UpdateHighContrast();
	}

	private static void UpdateHighContrast()
	{
		lock (__lock)
		{
			int count = __highContrastCallbackList.Count;
			int i = 0;
			int num = 0;
			for (; i < count; i++)
			{
				WeakReference weakReference = __highContrastCallbackList[num];
				if (weakReference.IsAlive)
				{
					HighContrastCallback highContrastCallback = weakReference.Target as HighContrastCallback;
					if (highContrastCallback.Dispatcher != null)
					{
						highContrastCallback.Dispatcher.BeginInvoke(DispatcherPriority.Background, new UpdateHighContrastCallback(OnUpdateHighContrast), highContrastCallback);
					}
					else
					{
						OnUpdateHighContrast(highContrastCallback);
					}
					num++;
				}
				else
				{
					__highContrastCallbackList.RemoveAt(num);
				}
			}
			__increaseCount = 0;
		}
	}

	private static void OnUpdateHighContrast(HighContrastCallback highContrastCallback)
	{
		bool highContrast = SystemParameters.HighContrast;
		Color windowTextColor = SystemColors.WindowTextColor;
		if (highContrast)
		{
			highContrastCallback.TurnHighContrastOn(windowTextColor);
		}
		else
		{
			highContrastCallback.TurnHighContrastOff();
		}
	}
}
