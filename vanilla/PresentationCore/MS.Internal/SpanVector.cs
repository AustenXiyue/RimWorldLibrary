using System;
using System.Collections;
using MS.Utility;

namespace MS.Internal;

internal class SpanVector : IEnumerable
{
	private static Equals _referenceEquals = object.ReferenceEquals;

	private static Equals _equals = object.Equals;

	private FrugalStructList<Span> _spans;

	private object _defaultObject;

	public int Count => _spans.Count;

	public object Default => _defaultObject;

	public Span this[int index] => _spans[index];

	public SpanVector(object defaultObject)
		: this(defaultObject, default(FrugalStructList<Span>))
	{
	}

	internal SpanVector(object defaultObject, FrugalStructList<Span> spans)
	{
		_defaultObject = defaultObject;
		_spans = spans;
	}

	public IEnumerator GetEnumerator()
	{
		return new SpanEnumerator(this);
	}

	private void Add(Span span)
	{
		_spans.Add(span);
	}

	internal virtual void Delete(int index, int count, ref SpanPosition latestPosition)
	{
		DeleteInternal(index, count);
		if (index <= latestPosition.Index)
		{
			latestPosition = default(SpanPosition);
		}
	}

	private void DeleteInternal(int index, int count)
	{
		for (int num = index + count - 1; num >= index; num--)
		{
			_spans.RemoveAt(num);
		}
	}

	private void Insert(int index, int count)
	{
		for (int i = 0; i < count; i++)
		{
			_spans.Insert(index, new Span(null, 0));
		}
	}

	internal bool FindSpan(int cp, SpanPosition latestPosition, out SpanPosition spanPosition)
	{
		int count = _spans.Count;
		int i;
		int num;
		if (cp == 0)
		{
			i = 0;
			num = 0;
		}
		else if (cp >= latestPosition.CP || cp * 2 < latestPosition.CP)
		{
			if (cp >= latestPosition.CP)
			{
				i = latestPosition.Index;
				num = latestPosition.CP;
			}
			else
			{
				i = 0;
				num = 0;
			}
			for (; i < count; i++)
			{
				int length = _spans[i].length;
				if (cp < num + length)
				{
					break;
				}
				num += length;
			}
		}
		else
		{
			i = latestPosition.Index;
			for (num = latestPosition.CP; num > cp; num -= _spans[--i].length)
			{
			}
		}
		spanPosition = new SpanPosition(i, num);
		return i != count;
	}

	public void SetValue(int first, int length, object element)
	{
		Set(first, length, element, _equals, default(SpanPosition));
	}

	public SpanPosition SetValue(int first, int length, object element, SpanPosition spanPosition)
	{
		return Set(first, length, element, _equals, spanPosition);
	}

	public void SetReference(int first, int length, object element)
	{
		Set(first, length, element, _referenceEquals, default(SpanPosition));
	}

	public SpanPosition SetReference(int first, int length, object element, SpanPosition spanPosition)
	{
		return Set(first, length, element, _referenceEquals, spanPosition);
	}

	private SpanPosition Set(int first, int length, object element, Equals equals, SpanPosition spanPosition)
	{
		bool num = FindSpan(first, spanPosition, out spanPosition);
		int num2 = spanPosition.Index;
		int num3 = spanPosition.CP;
		if (!num)
		{
			if (num3 < first)
			{
				Add(new Span(_defaultObject, first - num3));
			}
			if (Count > 0 && equals(_spans[Count - 1].element, element))
			{
				_spans[Count - 1].length += length;
				if (num2 == Count)
				{
					num3 += length;
				}
			}
			else
			{
				Add(new Span(element, length));
			}
		}
		else
		{
			int i = num2;
			int num4;
			for (num4 = num3; i < Count && num4 + _spans[i].length <= first + length; i++)
			{
				num4 += _spans[i].length;
			}
			if (first == num3)
			{
				if (num2 > 0 && equals(_spans[num2 - 1].element, element))
				{
					num2--;
					num3 -= _spans[num2].length;
					first = num3;
					length += _spans[num2].length;
				}
			}
			else if (equals(_spans[num2].element, element))
			{
				length = first + length - num3;
				first = num3;
			}
			if (i < Count && equals(_spans[i].element, element))
			{
				length = num4 + _spans[i].length - first;
				num4 += _spans[i].length;
				i++;
			}
			if (i >= Count)
			{
				if (num3 < first)
				{
					if (Count != num2 + 2 && !Resize(num2 + 2))
					{
						throw new OutOfMemoryException();
					}
					_spans[num2].length = first - num3;
					_spans[num2 + 1] = new Span(element, length);
				}
				else
				{
					if (Count != num2 + 1 && !Resize(num2 + 1))
					{
						throw new OutOfMemoryException();
					}
					_spans[num2] = new Span(element, length);
				}
			}
			else
			{
				object element2 = null;
				int length2 = 0;
				if (first + length > num4)
				{
					element2 = _spans[i].element;
					length2 = num4 + _spans[i].length - (first + length);
				}
				int num5 = 1 + ((first > num3) ? 1 : 0) - (i - num2);
				if (num5 < 0)
				{
					DeleteInternal(num2 + 1, -num5);
				}
				else if (num5 > 0)
				{
					Insert(num2 + 1, num5);
					for (int j = 0; j < num5; j++)
					{
						_spans[num2 + 1 + j] = new Span(null, 0);
					}
				}
				if (num3 < first)
				{
					_spans[num2].length = first - num3;
					num2++;
					num3 = first;
				}
				_spans[num2] = new Span(element, length);
				num2++;
				num3 += length;
				if (num4 < first + length)
				{
					_spans[num2] = new Span(element2, length2);
				}
			}
		}
		return new SpanPosition(num2, num3);
	}

	private bool Resize(int targetCount)
	{
		if (targetCount > Count)
		{
			for (int i = 0; i < targetCount - Count; i++)
			{
				_spans.Add(new Span(null, 0));
			}
		}
		else if (targetCount < Count)
		{
			DeleteInternal(targetCount, Count - targetCount);
		}
		return true;
	}
}
