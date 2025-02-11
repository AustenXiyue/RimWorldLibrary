using System.Globalization;

namespace System.Windows.Controls;

/// <summary>Specifies a range of pages.</summary>
public struct PageRange
{
	private int _pageFrom;

	private int _pageTo;

	/// <summary>Gets or sets the page number of the first page in the range.</summary>
	/// <returns>The 1-based page number of the first page in the range.</returns>
	public int PageFrom
	{
		get
		{
			return _pageFrom;
		}
		set
		{
			_pageFrom = value;
		}
	}

	/// <summary>Gets or sets the page number of the last page in the range.</summary>
	/// <returns>The 1-based page number of the last page in the range.</returns>
	public int PageTo
	{
		get
		{
			return _pageTo;
		}
		set
		{
			_pageTo = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.PageRange" /> class that includes only the single specified page.</summary>
	/// <param name="page">The page that is printed or processed.</param>
	public PageRange(int page)
	{
		_pageFrom = page;
		_pageTo = page;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.PageRange" /> class with the specified first and last pages.</summary>
	/// <param name="pageFrom">The first page of the range.</param>
	/// <param name="pageTo">The last page of the range.</param>
	public PageRange(int pageFrom, int pageTo)
	{
		_pageFrom = pageFrom;
		_pageTo = pageTo;
	}

	/// <summary>Gets a string representation of the range.</summary>
	/// <returns>A string that represents the range of pages in the format "<see cref="P:System.Windows.Controls.PageRange.PageFrom" />-<see cref="P:System.Windows.Controls.PageRange.PageTo" />".</returns>
	public override string ToString()
	{
		if (_pageTo != _pageFrom)
		{
			return string.Format(CultureInfo.InvariantCulture, SR.PrintDialogPageRange, _pageFrom, _pageTo);
		}
		return _pageFrom.ToString(CultureInfo.InvariantCulture);
	}

	/// <summary>Tests whether an object of unknown type is equal to this <see cref="T:System.Windows.Controls.PageRange" />. </summary>
	/// <returns>true if the object is of type <see cref="T:System.Windows.Controls.PageRange" /> and is equal to this <see cref="T:System.Windows.Controls.PageRange" />; otherwise, false.</returns>
	/// <param name="obj">The object tested.</param>
	public override bool Equals(object obj)
	{
		if (obj == null || obj.GetType() != typeof(PageRange))
		{
			return false;
		}
		return Equals((PageRange)obj);
	}

	/// <summary>Tests whether a <see cref="T:System.Windows.Controls.PageRange" /> is equal to this <see cref="T:System.Windows.Controls.PageRange" />. </summary>
	/// <returns>true if the two <see cref="T:System.Windows.Controls.PageRange" /> objects are equal; otherwise, false.</returns>
	/// <param name="pageRange">The <see cref="T:System.Windows.Controls.PageRange" /> tested.</param>
	public bool Equals(PageRange pageRange)
	{
		if (pageRange.PageFrom == PageFrom)
		{
			return pageRange.PageTo == PageTo;
		}
		return false;
	}

	/// <summary>Gets the hash code value for the <see cref="T:System.Windows.Controls.PageRange" />.</summary>
	/// <returns>The hash code value for the <see cref="T:System.Windows.Controls.PageRange" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Defines the "==" operator for testing whether two specified <see cref="T:System.Windows.Controls.PageRange" /> objects are equal.</summary>
	/// <returns>true if the two <see cref="T:System.Windows.Controls.PageRange" /> objects are equal; otherwise, false.</returns>
	/// <param name="pr1">The first <see cref="T:System.Windows.Controls.PageRange" />.</param>
	/// <param name="pr2">The second <see cref="T:System.Windows.Controls.PageRange" />.</param>
	public static bool operator ==(PageRange pr1, PageRange pr2)
	{
		return pr1.Equals(pr2);
	}

	/// <summary>Defines the "!=" operator for testing whether two specified <see cref="T:System.Windows.Controls.PageRange" /> objects are not equal.</summary>
	/// <returns>true if the two <see cref="T:System.Windows.Controls.PageRange" /> objects are not equal; otherwise, false.</returns>
	/// <param name="pr1">The first <see cref="T:System.Windows.Controls.PageRange" />.</param>
	/// <param name="pr2">The second <see cref="T:System.Windows.Controls.PageRange" />.</param>
	public static bool operator !=(PageRange pr1, PageRange pr2)
	{
		return !pr1.Equals(pr2);
	}
}
