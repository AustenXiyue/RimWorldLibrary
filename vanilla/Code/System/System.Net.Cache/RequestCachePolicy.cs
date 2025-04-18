namespace System.Net.Cache;

/// <summary>Defines an application's caching requirements for resources obtained by using <see cref="T:System.Net.WebRequest" /> objects.</summary>
public class RequestCachePolicy
{
	private RequestCacheLevel m_Level;

	/// <summary>Gets the <see cref="T:System.Net.Cache.RequestCacheLevel" /> value specified when this instance was constructed.</summary>
	/// <returns>A <see cref="T:System.Net.Cache.RequestCacheLevel" /> value that specifies the cache behavior for resources obtained using <see cref="T:System.Net.WebRequest" /> objects.</returns>
	public RequestCacheLevel Level => m_Level;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Cache.RequestCachePolicy" /> class. </summary>
	public RequestCachePolicy()
		: this(RequestCacheLevel.Default)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Cache.RequestCachePolicy" /> class. using the specified cache policy.</summary>
	/// <param name="level">A <see cref="T:System.Net.Cache.RequestCacheLevel" /> that specifies the cache behavior for resources obtained using <see cref="T:System.Net.WebRequest" /> objects. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">level is not a valid <see cref="T:System.Net.Cache.RequestCacheLevel" />.value.</exception>
	public RequestCachePolicy(RequestCacheLevel level)
	{
		if (level < RequestCacheLevel.Default || level > RequestCacheLevel.NoCacheNoStore)
		{
			throw new ArgumentOutOfRangeException("level");
		}
		m_Level = level;
	}

	/// <summary>Returns a string representation of this instance.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the <see cref="P:System.Net.Cache.RequestCachePolicy.Level" /> for this instance.</returns>
	public override string ToString()
	{
		return "Level:" + m_Level;
	}
}
