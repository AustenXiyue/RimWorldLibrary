using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using Mono.Security.Authenticode;
using Unity;

namespace System.Security.Policy;

/// <summary>Defines the set of information that constitutes input to security policy decisions. This class cannot be inherited.</summary>
[Serializable]
[MonoTODO("Serialization format not compatible with .NET")]
[ComVisible(true)]
public sealed class Evidence : ICollection, IEnumerable
{
	private class EvidenceEnumerator : IEnumerator
	{
		private IEnumerator currentEnum;

		private IEnumerator hostEnum;

		private IEnumerator assemblyEnum;

		public object Current => currentEnum.Current;

		public EvidenceEnumerator(IEnumerator hostenum, IEnumerator assemblyenum)
		{
			hostEnum = hostenum;
			assemblyEnum = assemblyenum;
			currentEnum = hostEnum;
		}

		public bool MoveNext()
		{
			if (currentEnum == null)
			{
				return false;
			}
			bool flag = currentEnum.MoveNext();
			if (!flag && hostEnum == currentEnum && assemblyEnum != null)
			{
				currentEnum = assemblyEnum;
				flag = assemblyEnum.MoveNext();
			}
			return flag;
		}

		public void Reset()
		{
			if (hostEnum != null)
			{
				hostEnum.Reset();
				currentEnum = hostEnum;
			}
			else
			{
				currentEnum = assemblyEnum;
			}
			if (assemblyEnum != null)
			{
				assemblyEnum.Reset();
			}
		}
	}

	private bool _locked;

	private ArrayList hostEvidenceList;

	private ArrayList assemblyEvidenceList;

	/// <summary>Gets the number of evidence objects in the evidence set.</summary>
	/// <returns>The number of evidence objects in the evidence set.</returns>
	[Obsolete]
	public int Count
	{
		get
		{
			int num = 0;
			if (hostEvidenceList != null)
			{
				num += hostEvidenceList.Count;
			}
			if (assemblyEvidenceList != null)
			{
				num += assemblyEvidenceList.Count;
			}
			return num;
		}
	}

	/// <summary>Gets a value indicating whether the evidence set is read-only.</summary>
	/// <returns>Always false, because read-only evidence sets are not supported.</returns>
	public bool IsReadOnly => false;

	/// <summary>Gets a value indicating whether the evidence set is thread-safe.</summary>
	/// <returns>Always false because thread-safe evidence sets are not supported.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets or sets a value indicating whether the evidence is locked.</summary>
	/// <returns>true if the evidence is locked; otherwise, false. The default is false.</returns>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public bool Locked
	{
		get
		{
			return _locked;
		}
		[SecurityPermission(SecurityAction.Demand, ControlEvidence = true)]
		set
		{
			_locked = value;
		}
	}

	/// <summary>Gets the synchronization root.</summary>
	/// <returns>Always this (Me in Visual Basic), because synchronization of evidence sets is not supported.</returns>
	public object SyncRoot => this;

	internal ArrayList HostEvidenceList
	{
		get
		{
			if (hostEvidenceList == null)
			{
				hostEvidenceList = ArrayList.Synchronized(new ArrayList());
			}
			return hostEvidenceList;
		}
	}

	internal ArrayList AssemblyEvidenceList
	{
		get
		{
			if (assemblyEvidenceList == null)
			{
				assemblyEvidenceList = ArrayList.Synchronized(new ArrayList());
			}
			return assemblyEvidenceList;
		}
	}

	/// <summary>Initializes a new empty instance of the <see cref="T:System.Security.Policy.Evidence" /> class.</summary>
	public Evidence()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.Evidence" /> class from a shallow copy of an existing one.</summary>
	/// <param name="evidence">The <see cref="T:System.Security.Policy.Evidence" /> instance from which to create the new instance. This instance is not deep-copied. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="evidence" /> parameter is not a valid instance of <see cref="T:System.Security.Policy.Evidence" />. </exception>
	public Evidence(Evidence evidence)
	{
		if (evidence != null)
		{
			Merge(evidence);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.Evidence" /> class from multiple sets of host and assembly evidence.</summary>
	/// <param name="hostEvidence">The host evidence from which to create the new instance. </param>
	/// <param name="assemblyEvidence">The assembly evidence from which to create the new instance. </param>
	[Obsolete]
	public Evidence(object[] hostEvidence, object[] assemblyEvidence)
	{
		if (hostEvidence != null)
		{
			HostEvidenceList.AddRange(hostEvidence);
		}
		if (assemblyEvidence != null)
		{
			AssemblyEvidenceList.AddRange(assemblyEvidence);
		}
	}

	/// <summary>Adds the specified assembly evidence to the evidence set.</summary>
	/// <param name="id">Any evidence object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="id" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="id" /> is not serializable.</exception>
	[Obsolete]
	public void AddAssembly(object id)
	{
		AssemblyEvidenceList.Add(id);
	}

	/// <summary>Adds the specified evidence supplied by the host to the evidence set.</summary>
	/// <param name="id">Any evidence object. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="id" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="id" /> is not serializable.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	[Obsolete]
	public void AddHost(object id)
	{
		if (_locked && SecurityManager.SecurityEnabled)
		{
			new SecurityPermission(SecurityPermissionFlag.ControlEvidence).Demand();
		}
		HostEvidenceList.Add(id);
	}

	/// <summary>Removes the host and assembly evidence from the evidence set.</summary>
	[ComVisible(false)]
	public void Clear()
	{
		if (hostEvidenceList != null)
		{
			hostEvidenceList.Clear();
		}
		if (assemblyEvidenceList != null)
		{
			assemblyEvidenceList.Clear();
		}
	}

	/// <summary>Returns a duplicate copy of this evidence object.</summary>
	/// <returns>A duplicate copy of this evidence object.</returns>
	[ComVisible(false)]
	public Evidence Clone()
	{
		return new Evidence(this);
	}

	/// <summary>Copies evidence objects to an <see cref="T:System.Array" />.</summary>
	/// <param name="array">The target array to which to copy evidence objects. </param>
	/// <param name="index">The zero-based position in the array to which to begin copying evidence objects. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="array" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="index " />is outside the range of the target array<paramref name="." /></exception>
	[Obsolete]
	public void CopyTo(Array array, int index)
	{
		int num = 0;
		if (hostEvidenceList != null)
		{
			num = hostEvidenceList.Count;
			if (num > 0)
			{
				hostEvidenceList.CopyTo(array, index);
			}
		}
		if (assemblyEvidenceList != null && assemblyEvidenceList.Count > 0)
		{
			assemblyEvidenceList.CopyTo(array, index + num);
		}
	}

	/// <summary>Enumerates all evidence in the set, both that provided by the host and that provided by the assembly.</summary>
	/// <returns>An enumerator for evidence added by both the <see cref="M:System.Security.Policy.Evidence.AddHost(System.Object)" /> method and the <see cref="M:System.Security.Policy.Evidence.AddAssembly(System.Object)" /> method.</returns>
	[Obsolete]
	public IEnumerator GetEnumerator()
	{
		IEnumerator hostenum = null;
		if (hostEvidenceList != null)
		{
			hostenum = hostEvidenceList.GetEnumerator();
		}
		IEnumerator assemblyenum = null;
		if (assemblyEvidenceList != null)
		{
			assemblyenum = assemblyEvidenceList.GetEnumerator();
		}
		return new EvidenceEnumerator(hostenum, assemblyenum);
	}

	/// <summary>Enumerates evidence provided by the assembly.</summary>
	/// <returns>An enumerator for evidence added by the <see cref="M:System.Security.Policy.Evidence.AddAssembly(System.Object)" /> method.</returns>
	public IEnumerator GetAssemblyEnumerator()
	{
		return AssemblyEvidenceList.GetEnumerator();
	}

	/// <summary>Enumerates evidence supplied by the host.</summary>
	/// <returns>An enumerator for evidence added by the <see cref="M:System.Security.Policy.Evidence.AddHost(System.Object)" /> method.</returns>
	public IEnumerator GetHostEnumerator()
	{
		return HostEvidenceList.GetEnumerator();
	}

	/// <summary>Merges the specified evidence set into the current evidence set.</summary>
	/// <param name="evidence">The evidence set to be merged into the current evidence set. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="evidence" /> parameter is not a valid instance of <see cref="T:System.Security.Policy.Evidence" />. </exception>
	/// <exception cref="T:System.Security.SecurityException">
	///   <see cref="P:System.Security.Policy.Evidence.Locked" /> is true, the code that calls this method does not have <see cref="F:System.Security.Permissions.SecurityPermissionFlag.ControlEvidence" />, and the <paramref name="evidence" /> parameter has a host list that is not empty. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence" />
	/// </PermissionSet>
	public void Merge(Evidence evidence)
	{
		if (evidence == null || evidence.Count <= 0)
		{
			return;
		}
		if (evidence.hostEvidenceList != null)
		{
			foreach (object hostEvidence in evidence.hostEvidenceList)
			{
				AddHost(hostEvidence);
			}
		}
		if (evidence.assemblyEvidenceList == null)
		{
			return;
		}
		foreach (object assemblyEvidence in evidence.assemblyEvidenceList)
		{
			AddAssembly(assemblyEvidence);
		}
	}

	/// <summary>Removes the evidence for a given type from the host and assembly enumerations.</summary>
	/// <param name="t">The type of the evidence to be removed. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="t" /> is null.</exception>
	[ComVisible(false)]
	public void RemoveType(Type t)
	{
		for (int num = hostEvidenceList.Count; num >= 0; num--)
		{
			if (hostEvidenceList.GetType() == t)
			{
				hostEvidenceList.RemoveAt(num);
			}
		}
		for (int num2 = assemblyEvidenceList.Count; num2 >= 0; num2--)
		{
			if (assemblyEvidenceList.GetType() == t)
			{
				assemblyEvidenceList.RemoveAt(num2);
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool IsAuthenticodePresent(Assembly a);

	[FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
	internal static Evidence GetDefaultHostEvidence(Assembly a)
	{
		Evidence evidence = new Evidence();
		string escapedCodeBase = a.EscapedCodeBase;
		evidence.AddHost(Zone.CreateFromUrl(escapedCodeBase));
		evidence.AddHost(new Url(escapedCodeBase));
		evidence.AddHost(new Hash(a));
		if (string.Compare("FILE://", 0, escapedCodeBase, 0, 7, ignoreCase: true, CultureInfo.InvariantCulture) != 0)
		{
			evidence.AddHost(Site.CreateFromUrl(escapedCodeBase));
		}
		AssemblyName name = a.GetName();
		byte[] publicKey = name.GetPublicKey();
		if (publicKey != null && publicKey.Length != 0)
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob(publicKey);
			evidence.AddHost(new StrongName(blob, name.Name, name.Version));
		}
		if (IsAuthenticodePresent(a))
		{
			AuthenticodeDeformatter authenticodeDeformatter = new AuthenticodeDeformatter(a.Location);
			if (authenticodeDeformatter.SigningCertificate != null)
			{
				X509Certificate x509Certificate = new X509Certificate(authenticodeDeformatter.SigningCertificate.RawData);
				if (x509Certificate.GetHashCode() != 0)
				{
					evidence.AddHost(new Publisher(x509Certificate));
				}
			}
		}
		if (a.GlobalAssemblyCache)
		{
			evidence.AddHost(new GacInstalled());
		}
		AppDomainManager domainManager = AppDomain.CurrentDomain.DomainManager;
		if (domainManager != null && (domainManager.HostSecurityManager.Flags & HostSecurityManagerOptions.HostAssemblyEvidence) == HostSecurityManagerOptions.HostAssemblyEvidence)
		{
			evidence = domainManager.HostSecurityManager.ProvideAssemblyEvidence(a, evidence);
		}
		return evidence;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Policy.Evidence" /> class from multiple sets of host and assembly evidence. </summary>
	/// <param name="hostEvidence">The host evidence from which to create the new instance. </param>
	/// <param name="assemblyEvidence">The assembly evidence from which to create the new instance. </param>
	public Evidence(EvidenceBase[] hostEvidence, EvidenceBase[] assemblyEvidence)
	{
		ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Adds an evidence object of the specified type to the assembly-supplied evidence list. </summary>
	/// <param name="evidence">The assembly evidence to add.</param>
	/// <typeparam name="T">The type of the object in <paramref name="evidence" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="evidence" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">Evidence of type <paramref name="T" /> is already in the list.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="evidence" /> is not serializable.</exception>
	[ComVisible(false)]
	public void AddAssemblyEvidence<T>(T evidence)
	{
		ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Adds host evidence of the specified type to the host evidence collection.</summary>
	/// <param name="evidence">The host evidence to add.</param>
	/// <typeparam name="T">The type of the object in <paramref name="evidence" />.</typeparam>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="evidence" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">Evidence of type <paramref name="T" /> is already in the list.</exception>
	[ComVisible(false)]
	public void AddHostEvidence<T>(T evidence)
	{
		ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Gets assembly evidence of the specified type from the collection.</summary>
	/// <returns>Evidence of type <paramref name="T" /> in the assembly evidence collection.</returns>
	/// <typeparam name="T">The type of the evidence to get.</typeparam>
	[ComVisible(false)]
	public T GetAssemblyEvidence<T>()
	{
		ThrowStub.ThrowNotSupportedException();
		return default(T);
	}

	/// <summary>Gets host evidence of the specified type from the collection.</summary>
	/// <returns>Evidence of type <paramref name="T" /> in the host evidence collection.</returns>
	/// <typeparam name="T">The type of the evidence to get.</typeparam>
	[ComVisible(false)]
	public T GetHostEvidence<T>()
	{
		ThrowStub.ThrowNotSupportedException();
		return default(T);
	}
}
