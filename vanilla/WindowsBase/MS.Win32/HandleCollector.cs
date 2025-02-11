using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MS.Win32;

internal static class HandleCollector
{
	private class HandleType
	{
		internal readonly string name;

		private int initialThreshHold;

		private int threshHold;

		private int handleCount;

		private readonly int deltaPercent;

		internal HandleType(string name, int expense, int initialThreshHold)
		{
			this.name = name;
			this.initialThreshHold = initialThreshHold;
			threshHold = initialThreshHold;
			deltaPercent = 100 - expense;
		}

		internal void Add()
		{
			bool flag = false;
			lock (this)
			{
				handleCount++;
				flag = NeedCollection();
				if (!flag)
				{
					return;
				}
			}
			if (flag)
			{
				GC.Collect();
				Thread.Sleep((100 - deltaPercent) / 4);
			}
		}

		internal bool NeedCollection()
		{
			if (handleCount > threshHold)
			{
				threshHold = handleCount + handleCount * deltaPercent / 100;
				return true;
			}
			int num = 100 * threshHold / (100 + deltaPercent);
			if (num >= initialThreshHold && handleCount < (int)((float)num * 0.9f))
			{
				threshHold = num;
			}
			return false;
		}

		internal void Remove()
		{
			lock (this)
			{
				handleCount--;
				handleCount = Math.Max(0, handleCount);
			}
		}
	}

	private static HandleType[] handleTypes;

	private static int handleTypeCount = 0;

	private static object handleMutex = new object();

	internal static nint Add(nint handle, int type)
	{
		handleTypes[type - 1].Add();
		return handle;
	}

	internal static SafeHandle Add(SafeHandle handle, int type)
	{
		handleTypes[type - 1].Add();
		return handle;
	}

	internal static void Add(int type)
	{
		handleTypes[type - 1].Add();
	}

	internal static int RegisterType(string typeName, int expense, int initialThreshold)
	{
		lock (handleMutex)
		{
			if (handleTypeCount == 0 || handleTypeCount == handleTypes.Length)
			{
				HandleType[] destinationArray = new HandleType[handleTypeCount + 10];
				if (handleTypes != null)
				{
					Array.Copy(handleTypes, 0, destinationArray, 0, handleTypeCount);
				}
				handleTypes = destinationArray;
			}
			handleTypes[handleTypeCount++] = new HandleType(typeName, expense, initialThreshold);
			return handleTypeCount;
		}
	}

	internal static nint Remove(nint handle, int type)
	{
		handleTypes[type - 1].Remove();
		return handle;
	}

	internal static SafeHandle Remove(SafeHandle handle, int type)
	{
		handleTypes[type - 1].Remove();
		return handle;
	}

	internal static void Remove(int type)
	{
		handleTypes[type - 1].Remove();
	}
}
