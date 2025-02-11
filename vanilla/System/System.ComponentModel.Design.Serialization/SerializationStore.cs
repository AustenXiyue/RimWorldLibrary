using System.Collections;
using System.IO;
using System.Security.Permissions;

namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides the base class for storing serialization data for the <see cref="T:System.ComponentModel.Design.Serialization.ComponentSerializationService" />.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public abstract class SerializationStore : IDisposable
{
	/// <summary>Gets a collection of errors that occurred during serialization or deserialization.</summary>
	/// <returns>An <see cref="T:System.Collections.ICollection" /> that contains errors that occurred during serialization or deserialization.</returns>
	public abstract ICollection Errors { get; }

	/// <summary>Closes the serialization store.</summary>
	public abstract void Close();

	/// <summary>Saves the store to the given stream.</summary>
	/// <param name="stream">The stream to which the store will be serialized.</param>
	public abstract void Save(Stream stream);

	/// <summary>Releases all resources used by the <see cref="T:System.ComponentModel.Design.Serialization.SerializationStore" />.</summary>
	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Design.Serialization.SerializationStore" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Close();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.SerializationStore" /> class. </summary>
	protected SerializationStore()
	{
	}
}
