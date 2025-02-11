using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Windows.Documents;

internal class BorderFormat
{
	private long _cf;

	private long _width;

	private BorderType _type;

	private static BorderFormat _emptyBorderFormat;

	internal long CF
	{
		get
		{
			return _cf;
		}
		set
		{
			_cf = value;
		}
	}

	internal long Width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = Validators.MakeValidBorderWidth(value);
		}
	}

	internal long EffectiveWidth => Type switch
	{
		BorderType.BorderNone => 0L, 
		BorderType.BorderDouble => Width * 2, 
		_ => Width, 
	};

	internal BorderType Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	internal bool IsNone
	{
		get
		{
			if (EffectiveWidth > 0)
			{
				return Type == BorderType.BorderNone;
			}
			return true;
		}
	}

	internal string RTFEncoding
	{
		get
		{
			if (IsNone)
			{
				return "\\brdrnone";
			}
			DefaultInterpolatedStringHandler handler;
			string result;
			if (CF < 0)
			{
				IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
				IFormatProvider formatProvider = invariantCulture;
				Span<char> span = stackalloc char[128];
				IFormatProvider provider = formatProvider;
				Span<char> initialBuffer = span;
				handler = new DefaultInterpolatedStringHandler(12, 1, invariantCulture, span);
				handler.AppendLiteral("\\brdrs\\brdrw");
				handler.AppendFormatted(EffectiveWidth);
				result = string.Create(provider, initialBuffer, ref handler);
			}
			else
			{
				IFormatProvider formatProvider = CultureInfo.InvariantCulture;
				IFormatProvider invariantCulture = formatProvider;
				Span<char> span = stackalloc char[128];
				IFormatProvider provider2 = invariantCulture;
				Span<char> initialBuffer2 = span;
				handler = new DefaultInterpolatedStringHandler(19, 2, formatProvider, span);
				handler.AppendLiteral("\\brdrs\\brdrw");
				handler.AppendFormatted(EffectiveWidth);
				handler.AppendLiteral("\\brdrcf");
				handler.AppendFormatted(CF);
				result = string.Create(provider2, initialBuffer2, ref handler);
			}
			return result;
		}
	}

	internal static BorderFormat EmptyBorderFormat
	{
		get
		{
			if (_emptyBorderFormat == null)
			{
				_emptyBorderFormat = new BorderFormat();
			}
			return _emptyBorderFormat;
		}
	}

	internal BorderFormat()
	{
		SetDefaults();
	}

	internal BorderFormat(BorderFormat cb)
	{
		CF = cb.CF;
		Width = cb.Width;
		Type = cb.Type;
	}

	internal void SetDefaults()
	{
		_cf = -1L;
		_width = 0L;
		_type = BorderType.BorderNone;
	}
}
