using System;
using System.Collections;
using System.Globalization;

namespace MS.Internal.Utility;

internal class TraceLog
{
	private ArrayList _log;

	private int _size;

	internal TraceLog()
		: this(int.MaxValue)
	{
	}

	internal TraceLog(int size)
	{
		_size = size;
		_log = new ArrayList();
	}

	internal void Add(string message, params object[] args)
	{
		string value = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture) + " " + string.Format(CultureInfo.InvariantCulture, message, args);
		if (_log.Count == _size)
		{
			_log.RemoveAt(0);
		}
		_log.Add(value);
	}

	internal void WriteLog()
	{
		for (int i = 0; i < _log.Count; i++)
		{
			Console.WriteLine(_log[i]);
		}
	}

	internal static string IdFor(object o)
	{
		if (o == null)
		{
			return "NULL";
		}
		return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", o.GetType().Name, o.GetHashCode());
	}
}
