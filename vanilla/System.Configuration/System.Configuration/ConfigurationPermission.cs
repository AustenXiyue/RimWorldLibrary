using System.Security;
using System.Security.Permissions;

namespace System.Configuration;

/// <summary>Provides a permission structure that allows methods or classes to access configuration files. </summary>
[Serializable]
public sealed class ConfigurationPermission : CodeAccessPermission, IUnrestrictedPermission
{
	private bool unrestricted;

	/// <summary>Initializes a new instance of the <see cref="T:System.Configuration.ConfigurationPermission" /> class. </summary>
	/// <param name="state">The permission level to grant.</param>
	/// <exception cref="T:System.ArgumentException">The value of <paramref name="state" /> is neither <see cref="F:System.Security.Permissions.PermissionState.Unrestricted" /> nor <see cref="F:System.Security.Permissions.PermissionState.None" />.</exception>
	public ConfigurationPermission(PermissionState state)
	{
		unrestricted = state == PermissionState.Unrestricted;
	}

	/// <summary>Returns a new <see cref="T:System.Configuration.ConfigurationPermission" /> object with the same permission level.</summary>
	/// <returns>A new <see cref="T:System.Configuration.ConfigurationPermission" /> with the same permission level.</returns>
	public override IPermission Copy()
	{
		return new ConfigurationPermission(unrestricted ? PermissionState.Unrestricted : PermissionState.None);
	}

	/// <summary>Reads the value of the permission state from XML.</summary>
	/// <param name="securityElement">The configuration element from which the permission state is read.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="securityElement" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Security.SecurityElement.Tag" /> for the given <paramref name="securityElement" /> does not equal "IPermission".</exception>
	/// <exception cref="T:System.ArgumentException">The class attribute of the given <paramref name="securityElement " />equals null.</exception>
	/// <exception cref="T:System.ArgumentException">The class attribute of the given <paramref name="securityElement" /> is not the type name for <see cref="T:System.Configuration.ConfigurationPermission" />.</exception>
	/// <exception cref="T:System.ArgumentException">The version attribute for the given <paramref name="securityElement" /> does not equal 1.</exception>
	/// <exception cref="T:System.ArgumentException">The unrestricted attribute for the given <paramref name="securityElement" /> is neither true nor false.</exception>
	public override void FromXml(SecurityElement securityElement)
	{
		if (securityElement == null)
		{
			throw new ArgumentNullException("securityElement");
		}
		if (securityElement.Tag != "IPermission")
		{
			throw new ArgumentException("securityElement");
		}
		string text = securityElement.Attribute("Unrestricted");
		if (text != null)
		{
			unrestricted = string.Compare(text, "true", StringComparison.InvariantCultureIgnoreCase) == 0;
		}
	}

	/// <summary>Returns the logical intersection between the <see cref="T:System.Configuration.ConfigurationPermission" /> object and a given object that implements the <see cref="T:System.Security.IPermission" /> interface.</summary>
	/// <returns>The logical intersection between the <see cref="T:System.Configuration.ConfigurationPermission" /> and a given object that implements <see cref="T:System.Security.IPermission" />.</returns>
	/// <param name="target">The object containing the permissions to perform the intersection with.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not typed as <see cref="T:System.Configuration.ConfigurationPermission" />.</exception>
	public override IPermission Intersect(IPermission target)
	{
		if (target == null)
		{
			return null;
		}
		if (!(target is ConfigurationPermission configurationPermission))
		{
			throw new ArgumentException("target");
		}
		return new ConfigurationPermission((unrestricted && configurationPermission.IsUnrestricted()) ? PermissionState.Unrestricted : PermissionState.None);
	}

	/// <summary>Returns the logical union of the <see cref="T:System.Configuration.ConfigurationPermission" /> object and an object that implements the <see cref="T:System.Security.IPermission" /> interface.</summary>
	/// <returns>The logical union of the <see cref="T:System.Configuration.ConfigurationPermission" /> and an object that implements <see cref="T:System.Security.IPermission" />.</returns>
	/// <param name="target">The object to perform the union with.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not typed as <see cref="T:System.Configuration.ConfigurationPermission" />.</exception>
	public override IPermission Union(IPermission target)
	{
		if (target == null)
		{
			return Copy();
		}
		if (!(target is ConfigurationPermission configurationPermission))
		{
			throw new ArgumentException("target");
		}
		return new ConfigurationPermission((unrestricted || configurationPermission.IsUnrestricted()) ? PermissionState.Unrestricted : PermissionState.None);
	}

	/// <summary>Compares the <see cref="T:System.Configuration.ConfigurationPermission" /> object with an object implementing the <see cref="T:System.Security.IPermission" /> interface.</summary>
	/// <returns>true if the permission state is equal; otherwise, false.</returns>
	/// <param name="target">The object to compare to.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="target" /> is not typed as <see cref="T:System.Configuration.ConfigurationPermission" />.</exception>
	public override bool IsSubsetOf(IPermission target)
	{
		if (target == null)
		{
			return !unrestricted;
		}
		if (!(target is ConfigurationPermission configurationPermission))
		{
			throw new ArgumentException("target");
		}
		if (unrestricted)
		{
			return configurationPermission.IsUnrestricted();
		}
		return true;
	}

	/// <summary>Indicates whether the permission state for the <see cref="T:System.Configuration.ConfigurationPermission" /> object is the <see cref="F:System.Security.Permissions.PermissionState.Unrestricted" /> value of the <see cref="T:System.Security.Permissions.PermissionState" /> enumeration.</summary>
	/// <returns>true if the permission state for the <see cref="T:System.Configuration.ConfigurationPermission" /> is the <see cref="F:System.Security.Permissions.PermissionState.Unrestricted" /> value of <see cref="T:System.Security.Permissions.PermissionState" />; otherwise, false.</returns>
	public bool IsUnrestricted()
	{
		return unrestricted;
	}

	/// <summary>Returns a <see cref="T:System.Security.SecurityElement" /> object with attribute values based on the current <see cref="T:System.Configuration.ConfigurationPermission" /> object.</summary>
	/// <returns>A <see cref="T:System.Security.SecurityElement" /> with attribute values based on the current <see cref="T:System.Configuration.ConfigurationPermission" />.</returns>
	public override SecurityElement ToXml()
	{
		SecurityElement securityElement = new SecurityElement("IPermission");
		securityElement.AddAttribute("class", GetType().AssemblyQualifiedName);
		securityElement.AddAttribute("version", "1");
		if (unrestricted)
		{
			securityElement.AddAttribute("Unrestricted", "true");
		}
		return securityElement;
	}
}
