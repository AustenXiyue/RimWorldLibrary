using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic.Utils;

namespace System.Runtime.CompilerServices;

/// <summary>Represents a cache of runtime binding rules.</summary>
/// <typeparam name="T">The delegate type.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
[DebuggerStepThrough]
public class RuleCache<T> where T : class
{
	private T[] _rules = Array.Empty<T>();

	private readonly object _cacheLock = new object();

	private const int MaxRules = 128;

	private const int InsertPosition = 64;

	internal RuleCache()
	{
	}

	internal T[] GetRules()
	{
		return _rules;
	}

	internal void MoveRule(T rule, int i)
	{
		lock (_cacheLock)
		{
			int num = _rules.Length - i;
			if (num > 8)
			{
				num = 8;
			}
			int num2 = -1;
			int num3 = Math.Min(_rules.Length, i + num);
			for (int j = i; j < num3; j++)
			{
				if (_rules[j] == rule)
				{
					num2 = j;
					break;
				}
			}
			if (num2 >= 2)
			{
				T val = _rules[num2];
				_rules[num2] = _rules[num2 - 1];
				_rules[num2 - 1] = _rules[num2 - 2];
				_rules[num2 - 2] = val;
			}
		}
	}

	internal void AddRule(T newRule)
	{
		lock (_cacheLock)
		{
			_rules = AddOrInsert(_rules, newRule);
		}
	}

	internal void ReplaceRule(T oldRule, T newRule)
	{
		lock (_cacheLock)
		{
			int num = Array.IndexOf(_rules, oldRule);
			if (num >= 0)
			{
				_rules[num] = newRule;
			}
			else
			{
				_rules = AddOrInsert(_rules, newRule);
			}
		}
	}

	private static T[] AddOrInsert(T[] rules, T item)
	{
		if (rules.Length < 64)
		{
			return rules.AddLast(item);
		}
		int num = rules.Length + 1;
		T[] array;
		if (num > 128)
		{
			num = 128;
			array = rules;
		}
		else
		{
			array = new T[num];
		}
		Array.Copy(rules, 0, array, 0, 64);
		array[64] = item;
		Array.Copy(rules, 64, array, 65, num - 64 - 1);
		return array;
	}
}
