using System.Globalization;

namespace System.Net.Cache;

/// <summary>Defines an application's caching requirements for resources obtained by using <see cref="T:System.Net.HttpWebRequest" /> objects.</summary>
public class HttpRequestCachePolicy : RequestCachePolicy
{
	internal static readonly HttpRequestCachePolicy BypassCache = new HttpRequestCachePolicy(HttpRequestCacheLevel.BypassCache);

	private HttpRequestCacheLevel m_Level;

	private DateTime m_LastSyncDateUtc = DateTime.MinValue;

	private TimeSpan m_MaxAge = TimeSpan.MaxValue;

	private TimeSpan m_MinFresh = TimeSpan.MinValue;

	private TimeSpan m_MaxStale = TimeSpan.MinValue;

	/// <summary>Gets the <see cref="T:System.Net.Cache.HttpRequestCacheLevel" /> value that was specified when this instance was created.</summary>
	/// <returns>A <see cref="T:System.Net.Cache.HttpRequestCacheLevel" /> value that specifies the cache behavior for resources that were obtained using <see cref="T:System.Net.HttpWebRequest" /> objects.</returns>
	public new HttpRequestCacheLevel Level => m_Level;

	/// <summary>Gets the cache synchronization date for this instance.</summary>
	/// <returns>A <see cref="T:System.DateTime" /> value set to the date specified when this instance was created. If no date was specified, this property's value is <see cref="F:System.DateTime.MinValue" />.</returns>
	public DateTime CacheSyncDate
	{
		get
		{
			if (m_LastSyncDateUtc == DateTime.MinValue || m_LastSyncDateUtc == DateTime.MaxValue)
			{
				return m_LastSyncDateUtc;
			}
			return m_LastSyncDateUtc.ToLocalTime();
		}
	}

	internal DateTime InternalCacheSyncDateUtc => m_LastSyncDateUtc;

	/// <summary>Gets the maximum age permitted for a resource returned from the cache.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> value that is set to the maximum age value specified when this instance was created. If no date was specified, this property's value is <see cref="F:System.DateTime.MinValue" />.</returns>
	public TimeSpan MaxAge => m_MaxAge;

	/// <summary>Gets the minimum freshness that is permitted for a resource returned from the cache.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> value that specifies the minimum freshness specified when this instance was created. If no date was specified, this property's value is <see cref="F:System.DateTime.MinValue" />.</returns>
	public TimeSpan MinFresh => m_MinFresh;

	/// <summary>Gets the maximum staleness value that is permitted for a resource returned from the cache.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> value that is set to the maximum staleness value specified when this instance was created. If no date was specified, this property's value is <see cref="F:System.DateTime.MinValue" />.</returns>
	public TimeSpan MaxStale => m_MaxStale;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Cache.HttpRequestCachePolicy" /> class. </summary>
	public HttpRequestCachePolicy()
		: this(HttpRequestCacheLevel.Default)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Cache.HttpRequestCachePolicy" /> class using the specified cache policy.</summary>
	/// <param name="level">An <see cref="T:System.Net.Cache.HttpRequestCacheLevel" /> value. </param>
	public HttpRequestCachePolicy(HttpRequestCacheLevel level)
		: base(MapLevel(level))
	{
		m_Level = level;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Cache.HttpRequestCachePolicy" /> class using the specified age control and time values. </summary>
	/// <param name="cacheAgeControl">One of the following <see cref="T:System.Net.Cache.HttpCacheAgeControl" /> enumeration values: <see cref="F:System.Net.Cache.HttpCacheAgeControl.MaxAge" />, <see cref="F:System.Net.Cache.HttpCacheAgeControl.MaxStale" />, or <see cref="F:System.Net.Cache.HttpCacheAgeControl.MinFresh" />.</param>
	/// <param name="ageOrFreshOrStale">A <see cref="T:System.TimeSpan" /> value that specifies an amount of time. See the Remarks section for more information. </param>
	/// <exception cref="T:System.ArgumentException">The value specified for the <paramref name="cacheAgeControl" /> parameter cannot be used with this constructor.</exception>
	public HttpRequestCachePolicy(HttpCacheAgeControl cacheAgeControl, TimeSpan ageOrFreshOrStale)
		: this(HttpRequestCacheLevel.Default)
	{
		switch (cacheAgeControl)
		{
		case HttpCacheAgeControl.MinFresh:
			m_MinFresh = ageOrFreshOrStale;
			break;
		case HttpCacheAgeControl.MaxAge:
			m_MaxAge = ageOrFreshOrStale;
			break;
		case HttpCacheAgeControl.MaxStale:
			m_MaxStale = ageOrFreshOrStale;
			break;
		default:
			throw new ArgumentException(global::SR.GetString("The specified value is not valid in the '{0}' enumeration.", "HttpCacheAgeControl"), "cacheAgeControl");
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Cache.HttpRequestCachePolicy" /> class using the specified maximum age, age control value, and time value.</summary>
	/// <param name="cacheAgeControl">An <see cref="T:System.Net.Cache.HttpCacheAgeControl" /> value. </param>
	/// <param name="maxAge">A <see cref="T:System.TimeSpan" /> value that specifies the maximum age for resources.</param>
	/// <param name="freshOrStale">A <see cref="T:System.TimeSpan" /> value that specifies an amount of time. See the Remarks section for more information.  </param>
	/// <exception cref="T:System.ArgumentException">The value specified for the <paramref name="cacheAgeControl" /> parameter is not valid.</exception>
	public HttpRequestCachePolicy(HttpCacheAgeControl cacheAgeControl, TimeSpan maxAge, TimeSpan freshOrStale)
		: this(HttpRequestCacheLevel.Default)
	{
		switch (cacheAgeControl)
		{
		case HttpCacheAgeControl.MinFresh:
			m_MinFresh = freshOrStale;
			break;
		case HttpCacheAgeControl.MaxAge:
			m_MaxAge = maxAge;
			break;
		case HttpCacheAgeControl.MaxStale:
			m_MaxStale = freshOrStale;
			break;
		case HttpCacheAgeControl.MaxAgeAndMinFresh:
			m_MaxAge = maxAge;
			m_MinFresh = freshOrStale;
			break;
		case HttpCacheAgeControl.MaxAgeAndMaxStale:
			m_MaxAge = maxAge;
			m_MaxStale = freshOrStale;
			break;
		default:
			throw new ArgumentException(global::SR.GetString("The specified value is not valid in the '{0}' enumeration.", "HttpCacheAgeControl"), "cacheAgeControl");
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Cache.HttpRequestCachePolicy" /> class using the specified cache synchronization date.</summary>
	/// <param name="cacheSyncDate">A <see cref="T:System.DateTime" /> object that specifies the time when resources stored in the cache must be revalidated.</param>
	public HttpRequestCachePolicy(DateTime cacheSyncDate)
		: this(HttpRequestCacheLevel.Default)
	{
		m_LastSyncDateUtc = cacheSyncDate.ToUniversalTime();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Cache.HttpRequestCachePolicy" /> class using the specified maximum age, age control value, time value, and cache synchronization date.</summary>
	/// <param name="cacheAgeControl">An <see cref="T:System.Net.Cache.HttpCacheAgeControl" /> value. </param>
	/// <param name="maxAge">A <see cref="T:System.TimeSpan" /> value that specifies the maximum age for resources.</param>
	/// <param name="freshOrStale">A <see cref="T:System.TimeSpan" /> value that specifies an amount of time. See the Remarks section for more information.  </param>
	/// <param name="cacheSyncDate">A <see cref="T:System.DateTime" /> object that specifies the time when resources stored in the cache must be revalidated.</param>
	public HttpRequestCachePolicy(HttpCacheAgeControl cacheAgeControl, TimeSpan maxAge, TimeSpan freshOrStale, DateTime cacheSyncDate)
		: this(cacheAgeControl, maxAge, freshOrStale)
	{
		m_LastSyncDateUtc = cacheSyncDate.ToUniversalTime();
	}

	/// <summary>Returns a string representation of this instance.</summary>
	/// <returns>A <see cref="T:System.String" /> value that contains the property values for this instance.</returns>
	public override string ToString()
	{
		return "Level:" + m_Level.ToString() + ((m_MaxAge == TimeSpan.MaxValue) ? string.Empty : (" MaxAge:" + m_MaxAge.ToString())) + ((m_MinFresh == TimeSpan.MinValue) ? string.Empty : (" MinFresh:" + m_MinFresh.ToString())) + ((m_MaxStale == TimeSpan.MinValue) ? string.Empty : (" MaxStale:" + m_MaxStale.ToString())) + ((CacheSyncDate == DateTime.MinValue) ? string.Empty : (" CacheSyncDate:" + CacheSyncDate.ToString(CultureInfo.CurrentCulture)));
	}

	private static RequestCacheLevel MapLevel(HttpRequestCacheLevel level)
	{
		if (level <= HttpRequestCacheLevel.NoCacheNoStore)
		{
			return (RequestCacheLevel)level;
		}
		return level switch
		{
			HttpRequestCacheLevel.CacheOrNextCacheOnly => RequestCacheLevel.CacheOnly, 
			HttpRequestCacheLevel.Refresh => RequestCacheLevel.Reload, 
			_ => throw new ArgumentOutOfRangeException("level"), 
		};
	}
}
