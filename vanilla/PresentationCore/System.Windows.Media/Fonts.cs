using System.Collections;
using System.Collections.Generic;
using MS.Internal.FontCache;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Provides enumeration support for <see cref="T:System.Windows.Media.FontFamily" /> and <see cref="T:System.Windows.Media.Typeface" /> objects.</summary>
public static class Fonts
{
	private struct TypefaceCollection : ICollection<Typeface>, IEnumerable<Typeface>, IEnumerable
	{
		private IEnumerable<FontFamily> _families;

		public int Count
		{
			get
			{
				int num = 0;
				using IEnumerator<Typeface> enumerator = GetEnumerator();
				while (enumerator.MoveNext())
				{
					_ = enumerator.Current;
					num++;
				}
				return num;
			}
		}

		public bool IsReadOnly => true;

		public TypefaceCollection(IEnumerable<FontFamily> families)
		{
			_families = families;
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
			foreach (FontFamily family in _families)
			{
				foreach (Typeface typeface in family.GetTypefaces())
				{
					yield return typeface;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Typeface>)this).GetEnumerator();
		}
	}

	private static readonly ICollection<FontFamily> _defaultFontCollection = CreateDefaultFamilyCollection();

	/// <summary>Gets the collection of <see cref="T:System.Windows.Media.FontFamily" /> objects from the default system font location.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> of <see cref="T:System.Windows.Media.FontFamily" /> objects that represent the fonts in the system fonts collection.</returns>
	public static ICollection<FontFamily> SystemFontFamilies => _defaultFontCollection;

	/// <summary>Gets the collection of <see cref="T:System.Windows.Media.Typeface" /> objects from the default system font location.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> of <see cref="T:System.Windows.Media.Typeface" /> objects that represent the fonts in the system fonts collection.</returns>
	public static ICollection<Typeface> SystemTypefaces => new TypefaceCollection(_defaultFontCollection);

	/// <summary>Returns the collection of <see cref="T:System.Windows.Media.FontFamily" /> objects from a string value that represents the location of the fonts.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> of <see cref="T:System.Windows.Media.FontFamily" /> objects that represent the fonts in <paramref name="location" />.</returns>
	/// <param name="location">The location that contains the fonts.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="location" /> is null. You cannot pass null, because this parameter is treated as a path or URI.</exception>
	public static ICollection<FontFamily> GetFontFamilies(string location)
	{
		if (location == null)
		{
			throw new ArgumentNullException("location");
		}
		return GetFontFamilies(null, location);
	}

	/// <summary>Returns a collection of <see cref="T:System.Windows.Media.FontFamily" /> objects from a uniform resource identifier (URI) value that represents the location of the fonts.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> of <see cref="T:System.Windows.Media.FontFamily" /> objects that represent the fonts in <paramref name="baseUri" />.</returns>
	/// <param name="baseUri">The base URI value of the location of the fonts.</param>
	public static ICollection<FontFamily> GetFontFamilies(Uri baseUri)
	{
		if (baseUri == null)
		{
			throw new ArgumentNullException("baseUri");
		}
		return GetFontFamilies(baseUri, null);
	}

	/// <summary>Returns a collection of <see cref="T:System.Windows.Media.FontFamily" /> objects using a base uniform resource identifier (URI) value to resolve the font location.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> of <see cref="T:System.Windows.Media.FontFamily" /> objects that represent the fonts in the resolved font location.</returns>
	/// <param name="baseUri">The base URI value of the location of the fonts.</param>
	/// <param name="location">The location that contains the fonts.</param>
	public static ICollection<FontFamily> GetFontFamilies(Uri baseUri, string location)
	{
		if (baseUri != null && !baseUri.IsAbsoluteUri)
		{
			throw new ArgumentException(SR.UriNotAbsolute, "baseUri");
		}
		if (!string.IsNullOrEmpty(location) && Uri.TryCreate(location, UriKind.Absolute, out Uri result))
		{
			if (!Util.IsSupportedSchemeForAbsoluteFontFamilyUri(result))
			{
				throw new ArgumentException(SR.InvalidAbsoluteUriInFontFamilyName, "location");
			}
			location = result.GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
		}
		else
		{
			if (baseUri == null)
			{
				throw new ArgumentNullException("baseUri", SR.Format(SR.NullBaseUriParam, "baseUri", "location"));
			}
			if (string.IsNullOrEmpty(location))
			{
				location = "./";
			}
			else if (Util.IsReferenceToWindowsFonts(location))
			{
				location = "./" + location;
			}
			result = new Uri(baseUri, location);
		}
		return CreateFamilyCollection(result, baseUri, location);
	}

	/// <summary>Returns the collection of <see cref="T:System.Windows.Media.Typeface" /> objects from a string value that represents the font directory location.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> of <see cref="T:System.Windows.Media.Typeface" /> objects that represent the fonts in <paramref name="location" />.</returns>
	/// <param name="location">The location that contains the fonts.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="location" /> is null. You cannot pass null, because this parameter is treated as a path or URI.</exception>
	public static ICollection<Typeface> GetTypefaces(string location)
	{
		if (location == null)
		{
			throw new ArgumentNullException("location");
		}
		return new TypefaceCollection(GetFontFamilies(null, location));
	}

	/// <summary>Returns a collection of <see cref="T:System.Windows.Media.Typeface" /> objects from a uniform resource identifier (URI) value that represents the font location.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> of <see cref="T:System.Windows.Media.Typeface" /> objects that represent the fonts in <paramref name="baseUri" />.</returns>
	/// <param name="baseUri">The base URI value of the location of the fonts.</param>
	public static ICollection<Typeface> GetTypefaces(Uri baseUri)
	{
		if (baseUri == null)
		{
			throw new ArgumentNullException("baseUri");
		}
		return new TypefaceCollection(GetFontFamilies(baseUri, null));
	}

	/// <summary>Returns a collection of <see cref="T:System.Windows.Media.Typeface" /> objects using a base uniform resource identifier (URI) value to resolve the font location.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> of <see cref="T:System.Windows.Media.Typeface" /> objects that represent the fonts in the resolved font location.</returns>
	/// <param name="baseUri">The base URI value of the location of the fonts.</param>
	/// <param name="location">The location that contains the fonts.</param>
	public static ICollection<Typeface> GetTypefaces(Uri baseUri, string location)
	{
		return new TypefaceCollection(GetFontFamilies(baseUri, location));
	}

	private static ICollection<FontFamily> CreateFamilyCollection(Uri fontLocation, Uri fontFamilyBaseUri, string fontFamilyLocationReference)
	{
		return Array.AsReadOnly((((object)fontLocation == Util.WindowsFontsUriObject) ? FamilyCollection.FromWindowsFonts(fontLocation) : FamilyCollection.FromUri(fontLocation)).GetFontFamilies(fontFamilyBaseUri, fontFamilyLocationReference));
	}

	private static ICollection<FontFamily> CreateDefaultFamilyCollection()
	{
		return CreateFamilyCollection(Util.WindowsFontsUriObject, null, null);
	}
}
