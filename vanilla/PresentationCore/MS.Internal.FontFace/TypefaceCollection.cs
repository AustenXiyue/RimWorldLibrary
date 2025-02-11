using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using MS.Internal.PresentationCore;
using MS.Internal.Text.TextInterface;

namespace MS.Internal.FontFace;

internal struct TypefaceCollection : ICollection<Typeface>, IEnumerable<Typeface>, IEnumerable
{
	private struct Enumerator : IEnumerator<Typeface>, IEnumerator, IDisposable
	{
		private IEnumerator<Font> _familyEnumerator;

		private IEnumerator<FamilyTypeface> _familyTypefaceEnumerator;

		private TypefaceCollection _typefaceCollection;

		public Typeface Current
		{
			get
			{
				if (_typefaceCollection._family != null)
				{
					Font current = _familyEnumerator.Current;
					return new Typeface(_typefaceCollection._fontFamily, new System.Windows.FontStyle((int)current.Style), new System.Windows.FontWeight((int)current.Weight), new System.Windows.FontStretch((int)current.Stretch));
				}
				FamilyTypeface current2 = _familyTypefaceEnumerator.Current;
				return new Typeface(_typefaceCollection._fontFamily, current2.Style, current2.Weight, current2.Stretch);
			}
		}

		object IEnumerator.Current => ((IEnumerator<Typeface>)this).Current;

		public Enumerator(TypefaceCollection typefaceCollection)
		{
			_typefaceCollection = typefaceCollection;
			if (typefaceCollection._family != null)
			{
				_familyEnumerator = ((IEnumerable<Font>)typefaceCollection._family).GetEnumerator();
				_familyTypefaceEnumerator = null;
			}
			else
			{
				_familyTypefaceEnumerator = ((IEnumerable<FamilyTypeface>)typefaceCollection._familyTypefaceCollection).GetEnumerator();
				_familyEnumerator = null;
			}
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_familyEnumerator != null)
			{
				return _familyEnumerator.MoveNext();
			}
			return _familyTypefaceEnumerator.MoveNext();
		}

		public void Reset()
		{
			if (_typefaceCollection._family != null)
			{
				_familyEnumerator = ((IEnumerable<Font>)_typefaceCollection._family).GetEnumerator();
				_familyTypefaceEnumerator = null;
			}
			else
			{
				_familyTypefaceEnumerator = ((IEnumerable<FamilyTypeface>)_typefaceCollection._familyTypefaceCollection).GetEnumerator();
				_familyEnumerator = null;
			}
		}
	}

	private System.Windows.Media.FontFamily _fontFamily;

	private MS.Internal.Text.TextInterface.FontFamily _family;

	private FamilyTypefaceCollection _familyTypefaceCollection;

	public int Count
	{
		get
		{
			if (_family != null)
			{
				return checked((int)_family.Count);
			}
			return _familyTypefaceCollection.Count;
		}
	}

	public bool IsReadOnly => true;

	public TypefaceCollection(System.Windows.Media.FontFamily fontFamily, MS.Internal.Text.TextInterface.FontFamily family)
	{
		_fontFamily = fontFamily;
		_family = family;
		_familyTypefaceCollection = null;
	}

	public TypefaceCollection(System.Windows.Media.FontFamily fontFamily, FamilyTypefaceCollection familyTypefaceCollection)
	{
		_fontFamily = fontFamily;
		_familyTypefaceCollection = familyTypefaceCollection;
		_family = null;
	}

	public void Add(Typeface item)
	{
		throw new NotSupportedException();
	}

	public void Clear()
	{
		throw new NotSupportedException();
	}

	public bool Contains(Typeface item)
	{
		using (IEnumerator<Typeface> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Equals(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void CopyTo(Typeface[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException(SR.Collection_BadRank);
		}
		if (arrayIndex < 0 || arrayIndex >= array.Length || arrayIndex + Count > array.Length)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		using IEnumerator<Typeface> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Typeface current = enumerator.Current;
			array[arrayIndex++] = current;
		}
	}

	public bool Remove(Typeface item)
	{
		throw new NotSupportedException();
	}

	public IEnumerator<Typeface> GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}
}
