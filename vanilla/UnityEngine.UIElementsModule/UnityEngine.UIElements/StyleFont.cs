using System;
using System.Collections.Generic;
using UnityEngine.UIElements.StyleSheets;

namespace UnityEngine.UIElements;

public struct StyleFont : IStyleValue<Font>, IEquatable<StyleFont>
{
	private StyleKeyword m_Keyword;

	private Font m_Value;

	private int m_Specificity;

	public Font value
	{
		get
		{
			return (m_Keyword == StyleKeyword.Undefined) ? m_Value : null;
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

	int IStyleValue<Font>.specificity
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

	public StyleFont(Font v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleFont(StyleKeyword keyword)
		: this(null, keyword)
	{
	}

	internal StyleFont(Font v, StyleKeyword keyword)
	{
		m_Specificity = 0;
		m_Keyword = keyword;
		m_Value = v;
	}

	internal bool Apply<U>(U other, StylePropertyApplyMode mode) where U : IStyleValue<Font>
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

	bool IStyleValue<Font>.Apply<U>(U other, StylePropertyApplyMode mode)
	{
		return Apply(other, mode);
	}

	public static bool operator ==(StyleFont lhs, StyleFont rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleFont lhs, StyleFont rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleFont(StyleKeyword keyword)
	{
		return new StyleFont(keyword);
	}

	public static implicit operator StyleFont(Font v)
	{
		return new StyleFont(v);
	}

	public bool Equals(StyleFont other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is StyleFont styleFont))
		{
			return false;
		}
		return styleFont == this;
	}

	public override int GetHashCode()
	{
		int num = 917506989;
		num = num * -1521134295 + m_Keyword.GetHashCode();
		num = num * -1521134295 + EqualityComparer<Font>.Default.GetHashCode(m_Value);
		return num * -1521134295 + m_Specificity.GetHashCode();
	}

	public override string ToString()
	{
		return this.DebugString();
	}
}
