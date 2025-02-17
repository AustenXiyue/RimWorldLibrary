using System;
using UnityEngine.UIElements.StyleSheets;

namespace UnityEngine.UIElements;

public struct StyleCursor : IStyleValue<Cursor>, IEquatable<StyleCursor>
{
	private StyleKeyword m_Keyword;

	private Cursor m_Value;

	private int m_Specificity;

	public Cursor value
	{
		get
		{
			return (m_Keyword == StyleKeyword.Undefined) ? m_Value : default(Cursor);
		}
		set
		{
			m_Value = value;
			m_Keyword = StyleKeyword.Undefined;
		}
	}

	internal int specificity
	{
		get
		{
			return m_Specificity;
		}
		set
		{
			m_Specificity = value;
		}
	}

	int IStyleValue<Cursor>.specificity
	{
		get
		{
			return specificity;
		}
		set
		{
			specificity = value;
		}
	}

	public StyleKeyword keyword
	{
		get
		{
			return m_Keyword;
		}
		set
		{
			m_Keyword = value;
		}
	}

	public StyleCursor(Cursor v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleCursor(StyleKeyword keyword)
		: this(default(Cursor), keyword)
	{
	}

	internal StyleCursor(Cursor v, StyleKeyword keyword)
	{
		m_Specificity = 0;
		m_Keyword = keyword;
		m_Value = v;
	}

	internal bool Apply<U>(U other, StylePropertyApplyMode mode) where U : IStyleValue<Cursor>
	{
		if (StyleValueExtensions.CanApply(specificity, other.specificity, mode))
		{
			value = other.value;
			keyword = other.keyword;
			specificity = other.specificity;
			return true;
		}
		return false;
	}

	bool IStyleValue<Cursor>.Apply<U>(U other, StylePropertyApplyMode mode)
	{
		return Apply(other, mode);
	}

	public static bool operator ==(StyleCursor lhs, StyleCursor rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleCursor lhs, StyleCursor rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleCursor(StyleKeyword keyword)
	{
		return new StyleCursor(keyword);
	}

	public static implicit operator StyleCursor(Cursor v)
	{
		return new StyleCursor(v);
	}

	public bool Equals(StyleCursor other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is StyleCursor styleCursor))
		{
			return false;
		}
		return styleCursor == this;
	}

	public override int GetHashCode()
	{
		int num = 917506989;
		num = num * -1521134295 + m_Keyword.GetHashCode();
		num = num * -1521134295 + m_Value.GetHashCode();
		return num * -1521134295 + m_Specificity.GetHashCode();
	}

	public override string ToString()
	{
		return this.DebugString();
	}
}
