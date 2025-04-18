using System;
using UnityEngine.UIElements.StyleSheets;

namespace UnityEngine.UIElements;

public struct StyleBackground : IStyleValue<Background>, IEquatable<StyleBackground>
{
	private StyleKeyword m_Keyword;

	private Background m_Value;

	private int m_Specificity;

	public Background value
	{
		get
		{
			return (m_Keyword == StyleKeyword.Undefined) ? m_Value : default(Background);
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

	int IStyleValue<Background>.specificity
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

	public StyleBackground(Background v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleBackground(Texture2D v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleBackground(VectorImage v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleBackground(StyleKeyword keyword)
		: this(default(Background), keyword)
	{
	}

	internal StyleBackground(Texture2D v, StyleKeyword keyword)
		: this(Background.FromTexture2D(v), keyword)
	{
	}

	internal StyleBackground(VectorImage v, StyleKeyword keyword)
		: this(Background.FromVectorImage(v), keyword)
	{
	}

	internal StyleBackground(Background v, StyleKeyword keyword)
	{
		m_Specificity = 0;
		m_Keyword = keyword;
		m_Value = v;
	}

	internal bool Apply<U>(U other, StylePropertyApplyMode mode) where U : IStyleValue<Background>
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

	bool IStyleValue<Background>.Apply<U>(U other, StylePropertyApplyMode mode)
	{
		return Apply(other, mode);
	}

	public static bool operator ==(StyleBackground lhs, StyleBackground rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleBackground lhs, StyleBackground rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleBackground(StyleKeyword keyword)
	{
		return new StyleBackground(keyword);
	}

	public static implicit operator StyleBackground(Background v)
	{
		return new StyleBackground(v);
	}

	public static implicit operator StyleBackground(Texture2D v)
	{
		return new StyleBackground(v);
	}

	public bool Equals(StyleBackground other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is StyleBackground styleBackground))
		{
			return false;
		}
		return styleBackground == this;
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
