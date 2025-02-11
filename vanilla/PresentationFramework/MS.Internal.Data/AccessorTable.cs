using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Threading;

namespace MS.Internal.Data;

internal sealed class AccessorTable
{
	private readonly struct AccessorTableKey : IEquatable<AccessorTableKey>
	{
		private readonly SourceValueType _sourceValueType;

		private readonly Type _type;

		private readonly string _name;

		public AccessorTableKey(SourceValueType sourceValueType, Type type, string name)
		{
			Invariant.Assert(type != null);
			_sourceValueType = sourceValueType;
			_type = type;
			_name = name;
		}

		public override bool Equals(object o)
		{
			if (o is AccessorTableKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public bool Equals(AccessorTableKey other)
		{
			if (_sourceValueType == other._sourceValueType && _type == other._type)
			{
				return _name == other._name;
			}
			return false;
		}

		public static bool operator ==(AccessorTableKey k1, AccessorTableKey k2)
		{
			return k1.Equals(k2);
		}

		public static bool operator !=(AccessorTableKey k1, AccessorTableKey k2)
		{
			return !k1.Equals(k2);
		}

		public override int GetHashCode()
		{
			return _type.GetHashCode() + _name.GetHashCode();
		}
	}

	private const int AgeLimit = 10;

	private readonly Dictionary<AccessorTableKey, AccessorInfo> _table = new Dictionary<AccessorTableKey, AccessorInfo>();

	private int _generation;

	private bool _cleanupRequested;

	private bool _traceSize;

	internal AccessorInfo this[SourceValueType sourceValueType, Type type, string name]
	{
		get
		{
			if (type == null || name == null)
			{
				return null;
			}
			if (_table.TryGetValue(new AccessorTableKey(sourceValueType, type, name), out var value))
			{
				value.Generation = _generation;
			}
			return value;
		}
		set
		{
			if (type != null && name != null)
			{
				value.Generation = _generation;
				_table[new AccessorTableKey(sourceValueType, type, name)] = value;
				if (!_cleanupRequested)
				{
					RequestCleanup();
				}
			}
		}
	}

	internal bool TraceSize
	{
		get
		{
			return _traceSize;
		}
		set
		{
			_traceSize = value;
		}
	}

	internal AccessorTable()
	{
	}

	private void RequestCleanup()
	{
		_cleanupRequested = true;
		Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new DispatcherOperationCallback(CleanupOperation), null);
	}

	private object CleanupOperation(object arg)
	{
		foreach (KeyValuePair<AccessorTableKey, AccessorInfo> item in _table)
		{
			if (_generation - item.Value.Generation >= 10)
			{
				_table.Remove(item.Key);
			}
		}
		_generation++;
		_cleanupRequested = false;
		return null;
	}

	[Conditional("DEBUG")]
	internal void PrintStats()
	{
	}
}
