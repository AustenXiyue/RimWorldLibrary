using System.Configuration;
using System.Runtime.CompilerServices;
using Unity;

namespace System.Security.Authentication.ExtendedProtection.Configuration;

/// <summary>The <see cref="T:System.Security.Authentication.ExtendedProtection.ServiceNameCollection" /> class is a collection of service principal names that represent a configuration element for an <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" />.</summary>
[ConfigurationCollection(typeof(ServiceNameElement))]
public sealed class ServiceNameElementCollection : ConfigurationElementCollection
{
	/// <summary>The <see cref="P:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection.Item(System.String)" /> property gets or sets the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance at the specified index location.</summary>
	/// <returns>The <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance requested. If the requested instance is not found, then null is returned.</returns>
	/// <param name="index">The index of the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance in this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />.</param>
	public ServiceNameElement this[int index] => (ServiceNameElement)BaseGet(index);

	/// <summary>The <see cref="P:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection.Item(System.String)" /> property gets or sets the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance based on a string that represents the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance.</summary>
	/// <returns>The <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance requested. If the requested instance is not found, then null is returned.</returns>
	/// <param name="name">A <see cref="T:System.String" /> that represents the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance in this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />.</param>
	public new ServiceNameElement this[string name] => (ServiceNameElement)BaseGet(name);

	/// <summary>The <see cref="P:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection.Item(System.String)" /> property gets or sets the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance based on a string that represents the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance.</summary>
	/// <returns>The <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance requested. If the requested instance is not found, then null is returned.</returns>
	/// <param name="name">A <see cref="T:System.String" /> that represents the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance in this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />.</param>
	public new string this[string name]
	{
		set
		{
			Unity.ThrowStub.ThrowNotSupportedException();
		}
	}

	/// <summary>The <see cref="M:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection.Add(System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement)" /> method adds a <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance to this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />. </summary>
	/// <param name="element">The <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance to add to this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />.</param>
	public void Add(ServiceNameElement element)
	{
		throw new NotImplementedException();
	}

	/// <summary>The <see cref="M:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection.Clear" /> method removes all configuration element objects from this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />.</summary>
	public void Clear()
	{
		throw new NotImplementedException();
	}

	protected override ConfigurationElement CreateNewElement()
	{
		return new ServiceNameElement();
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return ((ServiceNameElement)element).Name;
	}

	/// <summary>The <see cref="M:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection.IndexOf(System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement)" /> method retrieves the index of the specified configuration element in this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />.</summary>
	/// <returns>The index of the specified <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> in this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />.</returns>
	/// <param name="element">The <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance to retrieve the index of in this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />.</param>
	public int IndexOf(ServiceNameElement element)
	{
		throw new NotImplementedException();
	}

	/// <summary>The <see cref="M:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection.Remove(System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement)" /> method removes a <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance from this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" /> based on the <see cref="T:System.String" /> specified.</summary>
	/// <param name="name">A <see cref="T:System.String" /> that represents the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance to remove from this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" /></param>
	public void Remove(string name)
	{
		throw new NotImplementedException();
	}

	/// <summary>The <see cref="M:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection.Remove(System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement)" /> method removes a <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance from this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />.</summary>
	/// <param name="element">The <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance to remove from this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element " />is null. </exception>
	public void Remove(ServiceNameElement element)
	{
		throw new NotImplementedException();
	}

	/// <summary>The <see cref="M:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection.Remove(System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement)" /> method removes a <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance from this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" /> based on the index specified.</summary>
	/// <param name="index">The index of the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElement" /> instance to remove from this <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" />.</param>
	public void RemoveAt(int index)
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection" /> class.</summary>
	public ServiceNameElementCollection()
	{
	}

	[SpecialName]
	public void set_Item(int index, ServiceNameElement value)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
