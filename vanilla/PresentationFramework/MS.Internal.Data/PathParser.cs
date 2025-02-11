using System;
using System.Collections;
using System.Text;
using System.Windows;
using MS.Utility;

namespace MS.Internal.Data;

internal class PathParser
{
	private enum State
	{
		Init,
		DrillIn,
		Prop,
		Done
	}

	private enum IndexerState
	{
		BeginParam,
		ParenString,
		ValueString,
		Done
	}

	private string _error;

	private State _state;

	private string _path;

	private int _index;

	private int _n;

	private DrillIn _drillIn;

	private ArrayList _al = new ArrayList();

	private const char NullChar = '\0';

	private const char EscapeChar = '^';

	private static SourceValueInfo[] EmptyInfo = Array.Empty<SourceValueInfo>();

	public string Error => _error;

	private void SetError(string id, params object[] args)
	{
		_error = SR.Format(SR.GetResourceString(id), args);
	}

	public SourceValueInfo[] Parse(string path)
	{
		_path = ((path != null) ? path.Trim() : string.Empty);
		_n = _path.Length;
		if (_n == 0)
		{
			return new SourceValueInfo[1]
			{
				new SourceValueInfo(SourceValueType.Direct, DrillIn.Never, (string)null)
			};
		}
		_index = 0;
		_drillIn = DrillIn.IfNeeded;
		_al.Clear();
		_error = null;
		_state = State.Init;
		while (_state != State.Done)
		{
			char c = ((_index < _n) ? _path[_index] : '\0');
			if (char.IsWhiteSpace(c))
			{
				_index++;
				continue;
			}
			switch (_state)
			{
			case State.Init:
				if (c == '\0' || c == '.' || c == '/')
				{
					_state = State.DrillIn;
				}
				else
				{
					_state = State.Prop;
				}
				break;
			case State.DrillIn:
				switch (c)
				{
				case '/':
					_drillIn = DrillIn.Always;
					_index++;
					break;
				case '.':
					_drillIn = DrillIn.Never;
					_index++;
					break;
				default:
					SetError("PathSyntax", _path.Substring(0, _index), _path.Substring(_index));
					return EmptyInfo;
				case '\0':
				case '[':
					break;
				}
				_state = State.Prop;
				break;
			case State.Prop:
			{
				bool flag = false;
				if (c == '[')
				{
					flag = true;
				}
				if (flag)
				{
					AddIndexer();
				}
				else
				{
					AddProperty();
				}
				break;
			}
			}
		}
		SourceValueInfo[] array;
		if (_error == null)
		{
			array = new SourceValueInfo[_al.Count];
			_al.CopyTo(array);
		}
		else
		{
			array = EmptyInfo;
		}
		return array;
	}

	private void AddProperty()
	{
		int index = _index;
		int num = 0;
		while (_index < _n && _path[_index] == '.')
		{
			_index++;
		}
		while (_index < _n && (num > 0 || !IsSpecialChar(_path[_index])))
		{
			if (_path[_index] == '(')
			{
				num++;
			}
			else if (_path[_index] == ')')
			{
				num--;
			}
			_index++;
		}
		if (num > 0)
		{
			SetError("UnmatchedParen", _path.Substring(index));
			return;
		}
		if (num < 0)
		{
			SetError("UnmatchedParen", _path.Substring(0, _index));
			return;
		}
		string text = _path.Substring(index, _index - index).Trim();
		SourceValueInfo sourceValueInfo = ((text.Length > 0) ? new SourceValueInfo(SourceValueType.Property, _drillIn, text) : new SourceValueInfo(SourceValueType.Direct, _drillIn, (string)null));
		_al.Add(sourceValueInfo);
		StartNewLevel();
	}

	private void AddIndexer()
	{
		int num = ++_index;
		int num2 = 1;
		bool flag = false;
		bool flag2 = false;
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		FrugalObjectList<IndexerParamInfo> frugalObjectList = new FrugalObjectList<IndexerParamInfo>();
		IndexerState indexerState = IndexerState.BeginParam;
		while (indexerState != IndexerState.Done)
		{
			if (_index >= _n)
			{
				SetError("UnmatchedBracket", _path.Substring(num - 1));
				return;
			}
			char c = _path[_index++];
			if (c == '^' && !flag)
			{
				flag = true;
				continue;
			}
			switch (indexerState)
			{
			case IndexerState.BeginParam:
				if (flag)
				{
					indexerState = IndexerState.ValueString;
				}
				else
				{
					if (c == '(')
					{
						indexerState = IndexerState.ParenString;
						break;
					}
					if (char.IsWhiteSpace(c))
					{
						break;
					}
					indexerState = IndexerState.ValueString;
				}
				goto case IndexerState.ValueString;
			case IndexerState.ParenString:
				if (flag)
				{
					stringBuilder.Append(c);
				}
				else if (c == ')')
				{
					indexerState = IndexerState.ValueString;
				}
				else
				{
					stringBuilder.Append(c);
				}
				break;
			case IndexerState.ValueString:
				if (flag)
				{
					stringBuilder2.Append(c);
					flag2 = false;
				}
				else if (num2 > 1)
				{
					stringBuilder2.Append(c);
					flag2 = false;
					if (c == ']')
					{
						num2--;
					}
				}
				else if (char.IsWhiteSpace(c))
				{
					stringBuilder2.Append(c);
					flag2 = true;
				}
				else if (c == ',' || c == ']')
				{
					string paren = stringBuilder.ToString();
					string text = stringBuilder2.ToString();
					if (flag2)
					{
						text = text.TrimEnd();
					}
					frugalObjectList.Add(new IndexerParamInfo(paren, text));
					stringBuilder.Length = 0;
					stringBuilder2.Length = 0;
					flag2 = false;
					indexerState = ((c == ']') ? IndexerState.Done : IndexerState.BeginParam);
				}
				else
				{
					stringBuilder2.Append(c);
					flag2 = false;
					if (c == '[')
					{
						num2++;
					}
				}
				break;
			}
			flag = false;
		}
		SourceValueInfo sourceValueInfo = new SourceValueInfo(SourceValueType.Indexer, _drillIn, frugalObjectList);
		_al.Add(sourceValueInfo);
		StartNewLevel();
	}

	private void StartNewLevel()
	{
		_state = ((_index < _n) ? State.DrillIn : State.Done);
		_drillIn = DrillIn.Never;
	}

	private static bool IsSpecialChar(char ch)
	{
		if (ch != '.' && ch != '/' && ch != '[')
		{
			return ch == ']';
		}
		return true;
	}
}
