using System.Runtime.CompilerServices;
using System.Security.Permissions;

namespace System.Threading;

/// <summary>Represents a callback delegate that has been registered with a <see cref="T:System.Threading.CancellationToken" />. </summary>
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public struct CancellationTokenRegistration : IEquatable<CancellationTokenRegistration>, IDisposable
{
	private readonly CancellationCallbackInfo m_callbackInfo;

	private readonly SparselyPopulatedArrayAddInfo<CancellationCallbackInfo> m_registrationInfo;

	internal CancellationTokenRegistration(CancellationCallbackInfo callbackInfo, SparselyPopulatedArrayAddInfo<CancellationCallbackInfo> registrationInfo)
	{
		m_callbackInfo = callbackInfo;
		m_registrationInfo = registrationInfo;
	}

	[FriendAccessAllowed]
	internal bool TryDeregister()
	{
		if (m_registrationInfo.Source == null)
		{
			return false;
		}
		if (m_registrationInfo.Source.SafeAtomicRemove(m_registrationInfo.Index, m_callbackInfo) != m_callbackInfo)
		{
			return false;
		}
		return true;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.Threading.CancellationTokenRegistration" /> class.</summary>
	public void Dispose()
	{
		bool flag = TryDeregister();
		CancellationCallbackInfo callbackInfo = m_callbackInfo;
		if (callbackInfo != null)
		{
			CancellationTokenSource cancellationTokenSource = callbackInfo.CancellationTokenSource;
			if (cancellationTokenSource.IsCancellationRequested && !cancellationTokenSource.IsCancellationCompleted && !flag && cancellationTokenSource.ThreadIDExecutingCallbacks != Thread.CurrentThread.ManagedThreadId)
			{
				cancellationTokenSource.WaitForCallbackToComplete(m_callbackInfo);
			}
		}
	}

	/// <summary>Determines whether two <see cref="T:System.Threading.CancellationTokenRegistration" /> instances are equal.</summary>
	/// <returns>True if the instances are equal; otherwise, false.</returns>
	/// <param name="left">The first instance.</param>
	/// <param name="right">The second instance.</param>
	public static bool operator ==(CancellationTokenRegistration left, CancellationTokenRegistration right)
	{
		return left.Equals(right);
	}

	/// <summary>Determines whether two <see cref="T:System.Threading.CancellationTokenRegistration" /> instances are not equal.</summary>
	/// <returns>True if the instances are not equal; otherwise, false.</returns>
	/// <param name="left">The first instance.</param>
	/// <param name="right">The second instance.</param>
	public static bool operator !=(CancellationTokenRegistration left, CancellationTokenRegistration right)
	{
		return !left.Equals(right);
	}

	/// <summary>Determines whether the current <see cref="T:System.Threading.CancellationTokenRegistration" /> instance is equal to the specified <see cref="T:System.Threading.CancellationTokenRegistration" />.</summary>
	/// <returns>True, if both this and <paramref name="obj" /> are equal. False, otherwise.Two <see cref="T:System.Threading.CancellationTokenRegistration" /> instances are equal if they both refer to the output of a single call to the same Register method of a <see cref="T:System.Threading.CancellationToken" />.</returns>
	/// <param name="obj">The other object to which to compare this instance.</param>
	public override bool Equals(object obj)
	{
		if (obj is CancellationTokenRegistration)
		{
			return Equals((CancellationTokenRegistration)obj);
		}
		return false;
	}

	/// <summary>Determines whether the current <see cref="T:System.Threading.CancellationTokenRegistration" /> instance is equal to the specified <see cref="T:System.Threading.CancellationTokenRegistration" />.</summary>
	/// <returns>True, if both this and <paramref name="other" /> are equal. False, otherwise. Two <see cref="T:System.Threading.CancellationTokenRegistration" /> instances are equal if they both refer to the output of a single call to the same Register method of a <see cref="T:System.Threading.CancellationToken" />.</returns>
	/// <param name="other">The other <see cref="T:System.Threading.CancellationTokenRegistration" /> to which to compare this instance.</param>
	public bool Equals(CancellationTokenRegistration other)
	{
		if (m_callbackInfo == other.m_callbackInfo && m_registrationInfo.Source == other.m_registrationInfo.Source)
		{
			return m_registrationInfo.Index == other.m_registrationInfo.Index;
		}
		return false;
	}

	/// <summary>Serves as a hash function for a <see cref="T:System.Threading.CancellationTokenRegistration" />.</summary>
	/// <returns>A hash code for the current <see cref="T:System.Threading.CancellationTokenRegistration" /> instance.</returns>
	public override int GetHashCode()
	{
		if (m_registrationInfo.Source != null)
		{
			return m_registrationInfo.Source.GetHashCode() ^ m_registrationInfo.Index.GetHashCode();
		}
		return m_registrationInfo.Index.GetHashCode();
	}
}
