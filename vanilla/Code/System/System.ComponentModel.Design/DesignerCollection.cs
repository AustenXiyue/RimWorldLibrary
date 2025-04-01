using System.Collections;
using System.Security.Permissions;

namespace System.ComponentModel.Design;

/// <summary>Represents a collection of designers.</summary>
[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
public class DesignerCollection : ICollection, IEnumerable
{
	private IList designers;

	/// <summary>Gets the number of designers in the collection.</summary>
	/// <returns>The number of designers in the collection.</returns>
	public int Count => designers.Count;

	/// <summary>Gets the designer at the specified index.</summary>
	/// <returns>The designer at the specified index.</returns>
	/// <param name="index">The index of the designer to return. </param>
	public virtual IDesignerHost this[int index] => (IDesignerHost)designers[index];

	/// <summary>Gets the number of elements contained in the collection.</summary>
	/// <returns>The number of elements contained in the collection.</returns>
	int ICollection.Count => Count;

	/// <summary>Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
	/// <returns>An object that can be used to synchronize access to the collection.</returns>
	object ICollection.SyncRoot => null;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.DesignerCollection" /> class that contains the specified designers.</summary>
	/// <param name="designers">An array of <see cref="T:System.ComponentModel.Design.IDesignerHost" /> objects to store. </param>
	public DesignerCollection(IDesignerHost[] designers)
	{
		if (designers != null)
		{
			this.designers = new ArrayList(designers);
		}
		else
		{
			this.designers = new ArrayList();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.DesignerCollection" /> class that contains the specified set of designers.</summary>
	/// <param name="designers">A list that contains the collection of designers to add. </param>
	public DesignerCollection(IList designers)
	{
		this.designers = designers;
	}

	/// <summary>Gets a new enumerator for this collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that enumerates the collection.</returns>
	public IEnumerator GetEnumerator()
	{
		return designers.GetEnumerator();
	}

	/// <summary>Copies the elements of the collection to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from collection. The <see cref="T:System.Array" /> must have zero-based indexing. </param>
	/// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins. </param>
	void ICollection.CopyTo(Array array, int index)
	{
		designers.CopyTo(array, index);
	}

	/// <summary>Gets a new enumerator for this collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that enumerates the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
