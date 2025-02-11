using System.Collections.Generic;
using System.Collections.ObjectModel;
using MS.Internal;
using MS.Internal.Security.RightsManagement;
using MS.Internal.WindowsBase;

namespace System.Security.RightsManagement;

/// <summary>Represents a secure client session for user activation, license binding, and other rights management operations.</summary>
public class SecureEnvironment : IDisposable
{
	private ContentUser _user;

	private string _applicationManifest;

	private ClientSession _clientSession;

	/// <summary>Gets the user or user-group specified when the <see cref="T:System.Security.RightsManagement.SecureEnvironment" /> was created. </summary>
	/// <returns>The user or user-group specified to the <see cref="M:System.Security.RightsManagement.SecureEnvironment.Create(System.String,System.Security.RightsManagement.ContentUser)" /> method when the <see cref="T:System.Security.RightsManagement.SecureEnvironment" /> was created.</returns>
	public ContentUser User
	{
		get
		{
			CheckDisposed();
			return _user;
		}
	}

	/// <summary>Gets the <see cref="P:System.Security.RightsManagement.SecureEnvironment.ApplicationManifest" /> specified when the <see cref="T:System.Security.RightsManagement.SecureEnvironment" /> was created.</summary>
	/// <returns>The application manifest specified to the <see cref="Overload:System.Security.RightsManagement.SecureEnvironment.Create" /> method when the <see cref="T:System.Security.RightsManagement.SecureEnvironment" /> is created.</returns>
	public string ApplicationManifest
	{
		get
		{
			CheckDisposed();
			return _applicationManifest;
		}
	}

	internal ClientSession ClientSession
	{
		get
		{
			Invariant.Assert(_clientSession != null);
			return _clientSession;
		}
	}

	/// <summary>Creates a secure client session for a specified user with a given rights manifest.</summary>
	/// <returns>A secure client session for activation, license binding, and other rights management operations.</returns>
	/// <param name="applicationManifest">The application rights manifest.</param>
	/// <param name="user">The user or user-group for granting access to rights managed content.</param>
	public static SecureEnvironment Create(string applicationManifest, ContentUser user)
	{
		return CriticalCreate(applicationManifest, user);
	}

	/// <summary>Creates a secure client session given an application rights manifest, <see cref="T:System.Security.RightsManagement.AuthenticationType" />, and <see cref="T:System.Security.RightsManagement.UserActivationMode" />.</summary>
	/// <returns>A secure client session for activation, license binding, and other rights management operations.</returns>
	/// <param name="applicationManifest">The application rights manifest.</param>
	/// <param name="authentication">The method of authentication.</param>
	/// <param name="userActivationMode">The type of the user rights account certificate.</param>
	public static SecureEnvironment Create(string applicationManifest, AuthenticationType authentication, UserActivationMode userActivationMode)
	{
		return CriticalCreate(applicationManifest, authentication, userActivationMode);
	}

	/// <summary>Indicates whether a given user has been activated for accessing rights managed content.</summary>
	/// <returns>true if the given <paramref name="user" /> has been activated for accessing rights managed content; otherwise, false.</returns>
	/// <param name="user">The user or user-group for granting access to rights managed content.</param>
	public static bool IsUserActivated(ContentUser user)
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		if (user.AuthenticationType != 0 && user.AuthenticationType != AuthenticationType.Passport)
		{
			throw new ArgumentOutOfRangeException("user", SR.OnlyPassportOrWindowsAuthenticatedUsersAreAllowed);
		}
		using ClientSession clientSession = new ClientSession(user);
		return clientSession.IsMachineActivated() && clientSession.IsUserActivated();
	}

	/// <summary>Removes the license activation for a specified user.</summary>
	/// <param name="user">The user to remove the license activation for.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="user" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The given <paramref name="user" /> is not authenticated with either Windows authentication or Passport authentication.</exception>
	public static void RemoveActivatedUser(ContentUser user)
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		if (user.AuthenticationType != 0 && user.AuthenticationType != AuthenticationType.Passport)
		{
			throw new ArgumentOutOfRangeException("user", SR.OnlyPassportOrWindowsAuthenticatedUsersAreAllowed);
		}
		using ClientSession clientSession = new ClientSession(user);
		foreach (string item in clientSession.EnumerateUsersCertificateIds(user, EnumerateLicenseFlags.ClientLicensor))
		{
			clientSession.DeleteLicense(item);
		}
		foreach (string item2 in clientSession.EnumerateUsersCertificateIds(user, EnumerateLicenseFlags.GroupIdentity))
		{
			clientSession.DeleteLicense(item2);
		}
	}

	/// <summary>Returns a list of the activated users.</summary>
	/// <returns>A list of the currently activated users.</returns>
	public static ReadOnlyCollection<ContentUser> GetActivatedUsers()
	{
		using ClientSession clientSession = ClientSession.DefaultUserClientSession(AuthenticationType.Windows);
		List<ContentUser> list = new List<ContentUser>();
		if (clientSession.IsMachineActivated())
		{
			int num = 0;
			while (true)
			{
				string text = clientSession.EnumerateLicense(EnumerateLicenseFlags.GroupIdentity, num);
				if (text == null)
				{
					break;
				}
				ContentUser contentUser = ClientSession.ExtractUserFromCertificateChain(text);
				using (ClientSession clientSession2 = new ClientSession(contentUser))
				{
					if (clientSession2.IsUserActivated())
					{
						list.Add(contentUser);
					}
				}
				num++;
			}
		}
		return new ReadOnlyCollection<ContentUser>(list);
	}

	/// <summary>Releases all resources used by the <see cref="T:System.Security.RightsManagement.SecureEnvironment" />.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.Security.RightsManagement.SecureEnvironment" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		try
		{
			if (disposing && _clientSession != null)
			{
				_clientSession.Dispose();
			}
		}
		finally
		{
			_clientSession = null;
		}
	}

	private static SecureEnvironment CriticalCreate(string applicationManifest, ContentUser user)
	{
		if (applicationManifest == null)
		{
			throw new ArgumentNullException("applicationManifest");
		}
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		if (user.AuthenticationType != 0 && user.AuthenticationType != AuthenticationType.Passport)
		{
			throw new ArgumentOutOfRangeException("user");
		}
		if (!IsUserActivated(user))
		{
			throw new RightsManagementException(RightsManagementFailureCode.NeedsGroupIdentityActivation);
		}
		ClientSession clientSession = new ClientSession(user);
		try
		{
			clientSession.BuildSecureEnvironment(applicationManifest);
			return new SecureEnvironment(applicationManifest, user, clientSession);
		}
		catch
		{
			clientSession.Dispose();
			throw;
		}
	}

	private static SecureEnvironment CriticalCreate(string applicationManifest, AuthenticationType authentication, UserActivationMode userActivationMode)
	{
		if (applicationManifest == null)
		{
			throw new ArgumentNullException("applicationManifest");
		}
		if (authentication != 0 && authentication != AuthenticationType.Passport)
		{
			throw new ArgumentOutOfRangeException("authentication");
		}
		if (userActivationMode != 0 && userActivationMode != UserActivationMode.Temporary)
		{
			throw new ArgumentOutOfRangeException("userActivationMode");
		}
		ContentUser user;
		using (ClientSession clientSession = ClientSession.DefaultUserClientSession(authentication))
		{
			if (!clientSession.IsMachineActivated())
			{
				clientSession.ActivateMachine(authentication);
			}
			user = clientSession.ActivateUser(authentication, userActivationMode);
		}
		ClientSession clientSession2 = new ClientSession(user, userActivationMode);
		try
		{
			try
			{
				clientSession2.AcquireClientLicensorCertificate();
			}
			catch (RightsManagementException)
			{
			}
			clientSession2.BuildSecureEnvironment(applicationManifest);
			return new SecureEnvironment(applicationManifest, user, clientSession2);
		}
		catch
		{
			clientSession2.Dispose();
			throw;
		}
	}

	private SecureEnvironment(string applicationManifest, ContentUser user, ClientSession clientSession)
	{
		Invariant.Assert(applicationManifest != null);
		Invariant.Assert(user != null);
		Invariant.Assert(clientSession != null);
		_user = user;
		_applicationManifest = applicationManifest;
		_clientSession = clientSession;
	}

	private void CheckDisposed()
	{
		if (_clientSession == null)
		{
			throw new ObjectDisposedException("SecureEnvironment");
		}
	}
}
