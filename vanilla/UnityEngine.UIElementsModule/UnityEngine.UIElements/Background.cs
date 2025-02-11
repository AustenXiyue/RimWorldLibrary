using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public struct Background : IEquatable<Background>
{
	private Texture2D m_Texture;

	private VectorImage m_VectorImage;

	public Texture2D texture
	{
		get
		{
			return m_Texture;
		}
		set
		{
			if (value != null && vectorImage != null)
			{
				throw new InvalidOperationException("Cannot set both texture and vectorImage on Background object");
			}
			m_Texture = value;
		}
	}

	public VectorImage vectorImage
	{
		get
		{
			return m_VectorImage;
		}
		set
		{
			if (value != null && texture != null)
			{
				throw new InvalidOperationException("Cannot set both texture and vectorImage on Background object");
			}
			m_VectorImage = value;
		}
	}

	[Obsolete("Use Background.FromTexture2D instead")]
	public Background(Texture2D t)
	{
		m_Texture = t;
		m_VectorImage = null;
	}

	public static Background FromTexture2D(Texture2D t)
	{
		Background result = default(Background);
		result.texture = t;
		return result;
	}

	public static Background FromVectorImage(VectorImage vi)
	{
		Background result = default(Background);
		result.vectorImage = vi;
		return result;
	}

	public static bool operator ==(Background lhs, Background rhs)
	{
		return EqualityComparer<Texture2D>.Default.Equals(lhs.texture, rhs.texture);
	}

	public static bool operator !=(Background lhs, Background rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(Background other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Background background))
		{
			return false;
		}
		return background == this;
	}

	public override int GetHashCode()
	{
		int num = 851985039;
		if (texture != null)
		{
			num = num * -1521134295 + EqualityComparer<Texture2D>.Default.GetHashCode(texture);
		}
		if (vectorImage != null)
		{
			num = num * -1521134295 + EqualityComparer<VectorImage>.Default.GetHashCode(vectorImage);
		}
		return num;
	}

	public override string ToString()
	{
		return texture?.ToString() ?? "";
	}
}
