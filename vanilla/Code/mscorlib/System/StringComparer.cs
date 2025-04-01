using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System;

/// <summary>Represents a string comparison operation that uses specific case and culture-based or ordinal comparison rules.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
[ComVisible(true)]
public abstract class StringComparer : IComparer, IEqualityComparer, IComparer<string>, IEqualityComparer<string>
{
	private static readonly StringComparer _invariantCulture = new CultureAwareComparer(CultureInfo.InvariantCulture, ignoreCase: false);

	private static readonly StringComparer _invariantCultureIgnoreCase = new CultureAwareComparer(CultureInfo.InvariantCulture, ignoreCase: true);

	private static readonly StringComparer _ordinal = new OrdinalComparer(ignoreCase: false);

	private static readonly StringComparer _ordinalIgnoreCase = new OrdinalComparer(ignoreCase: true);

	/// <summary>Gets a <see cref="T:System.StringComparer" /> object that performs a case-sensitive string comparison using the word comparison rules of the invariant culture.</summary>
	/// <returns>A new <see cref="T:System.StringComparer" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static StringComparer InvariantCulture => _invariantCulture;

	/// <summary>Gets a <see cref="T:System.StringComparer" /> object that performs a case-insensitive string comparison using the word comparison rules of the invariant culture.</summary>
	/// <returns>A new <see cref="T:System.StringComparer" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static StringComparer InvariantCultureIgnoreCase => _invariantCultureIgnoreCase;

	/// <summary>Gets a <see cref="T:System.StringComparer" /> object that performs a case-sensitive string comparison using the word comparison rules of the current culture.</summary>
	/// <returns>A new <see cref="T:System.StringComparer" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static StringComparer CurrentCulture => new CultureAwareComparer(CultureInfo.CurrentCulture, ignoreCase: false);

	/// <summary>Gets a <see cref="T:System.StringComparer" /> object that performs case-insensitive string comparisons using the word comparison rules of the current culture.</summary>
	/// <returns>A new <see cref="T:System.StringComparer" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public static StringComparer CurrentCultureIgnoreCase => new CultureAwareComparer(CultureInfo.CurrentCulture, ignoreCase: true);

	/// <summary>Gets a <see cref="T:System.StringComparer" /> object that performs a case-sensitive ordinal string comparison.</summary>
	/// <returns>A <see cref="T:System.StringComparer" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public static StringComparer Ordinal => _ordinal;

	/// <summary>Gets a <see cref="T:System.StringComparer" /> object that performs a case-insensitive ordinal string comparison.</summary>
	/// <returns>A <see cref="T:System.StringComparer" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public static StringComparer OrdinalIgnoreCase => _ordinalIgnoreCase;

	/// <summary>Creates a <see cref="T:System.StringComparer" /> object that compares strings according to the rules of a specified culture.</summary>
	/// <returns>A new <see cref="T:System.StringComparer" /> object that performs string comparisons according to the comparison rules used by the <paramref name="culture" /> parameter and the case rule specified by the <paramref name="ignoreCase" /> parameter.</returns>
	/// <param name="culture">A culture whose linguistic rules are used to perform a string comparison.</param>
	/// <param name="ignoreCase">true to specify that comparison operations be case-insensitive; false to specify that comparison operations be case-sensitive.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="culture" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public static StringComparer Create(CultureInfo culture, bool ignoreCase)
	{
		if (culture == null)
		{
			throw new ArgumentNullException("culture");
		}
		return new CultureAwareComparer(culture, ignoreCase);
	}

	/// <summary>When overridden in a derived class, compares two objects and returns an indication of their relative sort order.</summary>
	/// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.ValueMeaningLess than zero<paramref name="x" /> is less than <paramref name="y" />. -or-<paramref name="x" /> is null.Zero<paramref name="x" /> is equal to <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.-or-<paramref name="y" /> is null.</returns>
	/// <param name="x">An object to compare to <paramref name="y" />.</param>
	/// <param name="y">An object to compare to <paramref name="x" />.</param>
	/// <exception cref="T:System.ArgumentException">Neither <paramref name="x" /> nor <paramref name="y" /> is a <see cref="T:System.String" /> object, and neither <paramref name="x" /> nor <paramref name="y" /> implements the <see cref="T:System.IComparable" /> interface.</exception>
	/// <filterpriority>1</filterpriority>
	public int Compare(object x, object y)
	{
		if (x == y)
		{
			return 0;
		}
		if (x == null)
		{
			return -1;
		}
		if (y == null)
		{
			return 1;
		}
		if (x is string x2 && y is string y2)
		{
			return Compare(x2, y2);
		}
		if (x is IComparable comparable)
		{
			return comparable.CompareTo(y);
		}
		throw new ArgumentException(Environment.GetResourceString("At least one object must implement IComparable."));
	}

	/// <summary>When overridden in a derived class, indicates whether two objects are equal.</summary>
	/// <returns>true if <paramref name="x" /> and <paramref name="y" /> refer to the same object, or <paramref name="x" /> and <paramref name="y" /> are both the same type of object and those objects are equal; otherwise, false.</returns>
	/// <param name="x">An object to compare to <paramref name="y" />.</param>
	/// <param name="y">An object to compare to <paramref name="x" />.</param>
	/// <filterpriority>1</filterpriority>
	public new bool Equals(object x, object y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null || y == null)
		{
			return false;
		}
		if (x is string x2 && y is string y2)
		{
			return Equals(x2, y2);
		}
		return x.Equals(y);
	}

	/// <summary>When overridden in a derived class, gets the hash code for the specified object.</summary>
	/// <returns>A 32-bit signed hash code calculated from the value of the <paramref name="obj" /> parameter.</returns>
	/// <param name="obj">An object.</param>
	/// <exception cref="T:System.ArgumentException">Not enough memory is available to allocate the buffer that is required to compute the hash code.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="obj" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public int GetHashCode(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (obj is string obj2)
		{
			return GetHashCode(obj2);
		}
		return obj.GetHashCode();
	}

	/// <summary>When overridden in a derived class, compares two strings and returns an indication of their relative sort order.</summary>
	/// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.ValueMeaningLess than zero<paramref name="x" /> is less than <paramref name="y" />.-or-<paramref name="x" /> is null.Zero<paramref name="x" /> is equal to <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.-or-<paramref name="y" /> is null.</returns>
	/// <param name="x">A string to compare to <paramref name="y" />.</param>
	/// <param name="y">A string to compare to <paramref name="x" />.</param>
	/// <filterpriority>1</filterpriority>
	public abstract int Compare(string x, string y);

	/// <summary>When overridden in a derived class, indicates whether two strings are equal.</summary>
	/// <returns>true if <paramref name="x" /> and <paramref name="y" /> refer to the same object, or <paramref name="x" /> and <paramref name="y" /> are equal; otherwise, false.</returns>
	/// <param name="x">A string to compare to <paramref name="y" />.</param>
	/// <param name="y">A string to compare to <paramref name="x" />.</param>
	/// <filterpriority>1</filterpriority>
	public abstract bool Equals(string x, string y);

	/// <summary>When overridden in a derived class, gets the hash code for the specified string.</summary>
	/// <returns>A 32-bit signed hash code calculated from the value of the <paramref name="obj" /> parameter.</returns>
	/// <param name="obj">A string.</param>
	/// <exception cref="T:System.ArgumentException">Not enough memory is available to allocate the buffer that is required to compute the hash code.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="obj" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	public abstract int GetHashCode(string obj);

	/// <summary>Initializes a new instance of the <see cref="T:System.StringComparer" /> class. </summary>
	protected StringComparer()
	{
	}
}
