using System.Collections.Generic;
using System.Globalization;

namespace System.Text.RegularExpressions;

internal sealed class RegexNode
{
	internal const int Oneloop = 3;

	internal const int Notoneloop = 4;

	internal const int Setloop = 5;

	internal const int Onelazy = 6;

	internal const int Notonelazy = 7;

	internal const int Setlazy = 8;

	internal const int One = 9;

	internal const int Notone = 10;

	internal const int Set = 11;

	internal const int Multi = 12;

	internal const int Ref = 13;

	internal const int Bol = 14;

	internal const int Eol = 15;

	internal const int Boundary = 16;

	internal const int Nonboundary = 17;

	internal const int ECMABoundary = 41;

	internal const int NonECMABoundary = 42;

	internal const int Beginning = 18;

	internal const int Start = 19;

	internal const int EndZ = 20;

	internal const int End = 21;

	internal const int Nothing = 22;

	internal const int Empty = 23;

	internal const int Alternate = 24;

	internal const int Concatenate = 25;

	internal const int Loop = 26;

	internal const int Lazyloop = 27;

	internal const int Capture = 28;

	internal const int Group = 29;

	internal const int Require = 30;

	internal const int Prevent = 31;

	internal const int Greedy = 32;

	internal const int Testref = 33;

	internal const int Testgroup = 34;

	internal int _type;

	internal List<RegexNode> _children;

	internal string _str;

	internal char _ch;

	internal int _m;

	internal int _n;

	internal RegexOptions _options;

	internal RegexNode _next;

	internal RegexNode(int type, RegexOptions options)
	{
		_type = type;
		_options = options;
	}

	internal RegexNode(int type, RegexOptions options, char ch)
	{
		_type = type;
		_options = options;
		_ch = ch;
	}

	internal RegexNode(int type, RegexOptions options, string str)
	{
		_type = type;
		_options = options;
		_str = str;
	}

	internal RegexNode(int type, RegexOptions options, int m)
	{
		_type = type;
		_options = options;
		_m = m;
	}

	internal RegexNode(int type, RegexOptions options, int m, int n)
	{
		_type = type;
		_options = options;
		_m = m;
		_n = n;
	}

	internal bool UseOptionR()
	{
		return (_options & RegexOptions.RightToLeft) != 0;
	}

	internal RegexNode ReverseLeft()
	{
		if (UseOptionR() && _type == 25 && _children != null)
		{
			_children.Reverse(0, _children.Count);
		}
		return this;
	}

	internal void MakeRep(int type, int min, int max)
	{
		_type += type - 9;
		_m = min;
		_n = max;
	}

	internal RegexNode Reduce()
	{
		switch (Type())
		{
		case 24:
			return ReduceAlternation();
		case 25:
			return ReduceConcatenation();
		case 26:
		case 27:
			return ReduceRep();
		case 29:
			return ReduceGroup();
		case 5:
		case 11:
			return ReduceSet();
		default:
			return this;
		}
	}

	internal RegexNode StripEnation(int emptyType)
	{
		return ChildCount() switch
		{
			0 => new RegexNode(emptyType, _options), 
			1 => Child(0), 
			_ => this, 
		};
	}

	internal RegexNode ReduceGroup()
	{
		RegexNode regexNode = this;
		while (regexNode.Type() == 29)
		{
			regexNode = regexNode.Child(0);
		}
		return regexNode;
	}

	internal RegexNode ReduceRep()
	{
		RegexNode regexNode = this;
		int num = Type();
		int num2 = _m;
		int num3 = _n;
		while (regexNode.ChildCount() != 0)
		{
			RegexNode regexNode2 = regexNode.Child(0);
			if (regexNode2.Type() != num)
			{
				int num4 = regexNode2.Type();
				if ((num4 < 3 || num4 > 5 || num != 26) && (num4 < 6 || num4 > 8 || num != 27))
				{
					break;
				}
			}
			if ((regexNode._m == 0 && regexNode2._m > 1) || regexNode2._n < regexNode2._m * 2)
			{
				break;
			}
			regexNode = regexNode2;
			if (regexNode._m > 0)
			{
				num2 = (regexNode._m = ((2147483646 / regexNode._m < num2) ? int.MaxValue : (regexNode._m * num2)));
			}
			if (regexNode._n > 0)
			{
				num3 = (regexNode._n = ((2147483646 / regexNode._n < num3) ? int.MaxValue : (regexNode._n * num3)));
			}
		}
		if (num2 != int.MaxValue)
		{
			return regexNode;
		}
		return new RegexNode(22, _options);
	}

	internal RegexNode ReduceSet()
	{
		if (RegexCharClass.IsEmpty(_str))
		{
			_type = 22;
			_str = null;
		}
		else if (RegexCharClass.IsSingleton(_str))
		{
			_ch = RegexCharClass.SingletonChar(_str);
			_str = null;
			_type += -2;
		}
		else if (RegexCharClass.IsSingletonInverse(_str))
		{
			_ch = RegexCharClass.SingletonChar(_str);
			_str = null;
			_type += -1;
		}
		return this;
	}

	internal RegexNode ReduceAlternation()
	{
		if (_children == null)
		{
			return new RegexNode(22, _options);
		}
		bool flag = false;
		bool flag2 = false;
		RegexOptions regexOptions = RegexOptions.None;
		int i = 0;
		int j;
		for (j = 0; i < _children.Count; i++, j++)
		{
			RegexNode regexNode = _children[i];
			if (j < i)
			{
				_children[j] = regexNode;
			}
			if (regexNode._type == 24)
			{
				for (int k = 0; k < regexNode._children.Count; k++)
				{
					regexNode._children[k]._next = this;
				}
				_children.InsertRange(i + 1, regexNode._children);
				j--;
			}
			else if (regexNode._type == 11 || regexNode._type == 9)
			{
				RegexOptions regexOptions2 = regexNode._options & (RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
				if (regexNode._type == 11)
				{
					if (!flag || regexOptions != regexOptions2 || flag2 || !RegexCharClass.IsMergeable(regexNode._str))
					{
						flag = true;
						flag2 = !RegexCharClass.IsMergeable(regexNode._str);
						regexOptions = regexOptions2;
						continue;
					}
				}
				else if (!flag || regexOptions != regexOptions2 || flag2)
				{
					flag = true;
					flag2 = false;
					regexOptions = regexOptions2;
					continue;
				}
				j--;
				RegexNode regexNode2 = _children[j];
				RegexCharClass regexCharClass;
				if (regexNode2._type == 9)
				{
					regexCharClass = new RegexCharClass();
					regexCharClass.AddChar(regexNode2._ch);
				}
				else
				{
					regexCharClass = RegexCharClass.Parse(regexNode2._str);
				}
				if (regexNode._type == 9)
				{
					regexCharClass.AddChar(regexNode._ch);
				}
				else
				{
					RegexCharClass cc = RegexCharClass.Parse(regexNode._str);
					regexCharClass.AddCharClass(cc);
				}
				regexNode2._type = 11;
				regexNode2._str = regexCharClass.ToStringClass();
			}
			else if (regexNode._type == 22)
			{
				j--;
			}
			else
			{
				flag = false;
				flag2 = false;
			}
		}
		if (j < i)
		{
			_children.RemoveRange(j, i - j);
		}
		return StripEnation(22);
	}

	internal RegexNode ReduceConcatenation()
	{
		if (_children == null)
		{
			return new RegexNode(23, _options);
		}
		bool flag = false;
		RegexOptions regexOptions = RegexOptions.None;
		int num = 0;
		int num2 = 0;
		while (num < _children.Count)
		{
			RegexNode regexNode = _children[num];
			if (num2 < num)
			{
				_children[num2] = regexNode;
			}
			if (regexNode._type == 25 && (regexNode._options & RegexOptions.RightToLeft) == (_options & RegexOptions.RightToLeft))
			{
				for (int i = 0; i < regexNode._children.Count; i++)
				{
					regexNode._children[i]._next = this;
				}
				_children.InsertRange(num + 1, regexNode._children);
				num2--;
			}
			else if (regexNode._type == 12 || regexNode._type == 9)
			{
				RegexOptions regexOptions2 = regexNode._options & (RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
				if (!flag || regexOptions != regexOptions2)
				{
					flag = true;
					regexOptions = regexOptions2;
				}
				else
				{
					RegexNode regexNode2 = _children[--num2];
					if (regexNode2._type == 9)
					{
						regexNode2._type = 12;
						regexNode2._str = Convert.ToString(regexNode2._ch, CultureInfo.InvariantCulture);
					}
					if ((regexOptions2 & RegexOptions.RightToLeft) == 0)
					{
						if (regexNode._type == 9)
						{
							regexNode2._str += regexNode._ch;
						}
						else
						{
							regexNode2._str += regexNode._str;
						}
					}
					else if (regexNode._type == 9)
					{
						regexNode2._str = regexNode._ch + regexNode2._str;
					}
					else
					{
						regexNode2._str = regexNode._str + regexNode2._str;
					}
				}
			}
			else if (regexNode._type == 23)
			{
				num2--;
			}
			else
			{
				flag = false;
			}
			num++;
			num2++;
		}
		if (num2 < num)
		{
			_children.RemoveRange(num2, num - num2);
		}
		return StripEnation(23);
	}

	internal RegexNode MakeQuantifier(bool lazy, int min, int max)
	{
		if (min == 0 && max == 0)
		{
			return new RegexNode(23, _options);
		}
		if (min == 1 && max == 1)
		{
			return this;
		}
		int type = _type;
		if ((uint)(type - 9) <= 2u)
		{
			MakeRep(lazy ? 6 : 3, min, max);
			return this;
		}
		RegexNode regexNode = new RegexNode(lazy ? 27 : 26, _options, min, max);
		regexNode.AddChild(this);
		return regexNode;
	}

	internal void AddChild(RegexNode newChild)
	{
		if (_children == null)
		{
			_children = new List<RegexNode>(4);
		}
		RegexNode regexNode = newChild.Reduce();
		_children.Add(regexNode);
		regexNode._next = this;
	}

	internal RegexNode Child(int i)
	{
		return _children[i];
	}

	internal int ChildCount()
	{
		if (_children != null)
		{
			return _children.Count;
		}
		return 0;
	}

	internal int Type()
	{
		return _type;
	}
}
