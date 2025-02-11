using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Resources;

/// <summary>Stores all the resources localized for one particular culture, ignoring all other cultures, including any fallback rules.</summary>
[Serializable]
[ComVisible(true)]
public class ResourceSet : IDisposable, IEnumerable
{
	/// <summary>Indicates the <see cref="T:System.Resources.IResourceReader" /> used to read the resources.</summary>
	[NonSerialized]
	protected IResourceReader Reader;

	/// <summary>The <see cref="T:System.Collections.Hashtable" /> in which the resources are stored.</summary>
	protected Hashtable Table;

	private Hashtable _caseInsensitiveTable;

	/// <summary>Initializes a new instance of the <see cref="T:System.Resources.ResourceSet" /> class with default properties.</summary>
	protected ResourceSet()
	{
		CommonInit();
	}

	internal ResourceSet(bool junk)
	{
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Resources.ResourceSet" /> class using the system default <see cref="T:System.Resources.ResourceReader" /> that opens and reads resources from the given file.</summary>
	/// <param name="fileName">Resource file to read. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="fileName" /> parameter is null. </exception>
	public ResourceSet(string fileName)
	{
		Reader = new ResourceReader(fileName);
		CommonInit();
		ReadResources();
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Resources.ResourceSet" /> class using the system default <see cref="T:System.Resources.ResourceReader" /> that reads resources from the given stream.</summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> of resources to be read. The stream should refer to an existing resources file. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="stream" /> is not readable. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="stream" /> parameter is null. </exception>
	[SecurityCritical]
	public ResourceSet(Stream stream)
	{
		Reader = new ResourceReader(stream);
		CommonInit();
		ReadResources();
	}

	/// <summary>Creates a new instance of the <see cref="T:System.Resources.ResourceSet" /> class using the specified resource reader.</summary>
	/// <param name="reader">The reader that will be used. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="reader" /> parameter is null. </exception>
	public ResourceSet(IResourceReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		Reader = reader;
		CommonInit();
		ReadResources();
	}

	private void CommonInit()
	{
		Table = new Hashtable();
	}

	/// <summary>Closes and releases any resources used by this <see cref="T:System.Resources.ResourceSet" />.</summary>
	public virtual void Close()
	{
		Dispose(disposing: true);
	}

	/// <summary>Releases resources (other than memory) associated with the current instance, closing internal managed objects if requested.</summary>
	/// <param name="disposing">Indicates whether the objects contained in the current instance should be explicitly closed. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			IResourceReader reader = Reader;
			Reader = null;
			reader?.Close();
		}
		Reader = null;
		_caseInsensitiveTable = null;
		Table = null;
	}

	/// <summary>Disposes of the resources (other than memory) used by the current instance of <see cref="T:System.Resources.ResourceSet" />.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>Returns the preferred resource reader class for this kind of <see cref="T:System.Resources.ResourceSet" />.</summary>
	/// <returns>Returns the <see cref="T:System.Type" /> for the preferred resource reader for this kind of <see cref="T:System.Resources.ResourceSet" />.</returns>
	public virtual Type GetDefaultReader()
	{
		return typeof(ResourceReader);
	}

	/// <summary>Returns the preferred resource writer class for this kind of <see cref="T:System.Resources.ResourceSet" />.</summary>
	/// <returns>Returns the <see cref="T:System.Type" /> for the preferred resource writer for this kind of <see cref="T:System.Resources.ResourceSet" />.</returns>
	public virtual Type GetDefaultWriter()
	{
		return typeof(ResourceWriter);
	}

	/// <summary>Returns an <see cref="T:System.Collections.IDictionaryEnumerator" /> that can iterate through the <see cref="T:System.Resources.ResourceSet" />.</summary>
	/// <returns>An <see cref="T:System.Collections.IDictionaryEnumerator" /> for this <see cref="T:System.Resources.ResourceSet" />.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The resource set has been closed or disposed. </exception>
	[ComVisible(false)]
	public virtual IDictionaryEnumerator GetEnumerator()
	{
		return GetEnumeratorHelper();
	}

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> object to avoid a race condition with Dispose. This member is not intended to be used directly from your code.</summary>
	/// <returns>An enumerator for the current <see cref="T:System.Resources.ResourceSet" /> object.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumeratorHelper();
	}

	private IDictionaryEnumerator GetEnumeratorHelper()
	{
		return (Table ?? throw new ObjectDisposedException(null, Environment.GetResourceString("Cannot access a closed resource set."))).GetEnumerator();
	}

	/// <summary>Searches for a <see cref="T:System.String" /> resource with the specified name.</summary>
	/// <returns>The value of a resource, if the value is a <see cref="T:System.String" />.</returns>
	/// <param name="name">Name of the resource to search for. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The resource specified by <paramref name="name" /> is not a <see cref="T:System.String" />. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has been closed or disposed.</exception>
	public virtual string GetString(string name)
	{
		object objectInternal = GetObjectInternal(name);
		try
		{
			return (string)objectInternal;
		}
		catch (InvalidCastException)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Resource '{0}' was not a String - call GetObject instead.", name));
		}
	}

	/// <summary>Searches for a <see cref="T:System.String" /> resource with the specified name in a case-insensitive manner, if requested.</summary>
	/// <returns>The value of a resource, if the value is a <see cref="T:System.String" />.</returns>
	/// <param name="name">Name of the resource to search for. </param>
	/// <param name="ignoreCase">Indicates whether the case of the case of the specified name should be ignored. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The resource specified by <paramref name="name" /> is not a <see cref="T:System.String" />.</exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has been closed or disposed.</exception>
	public virtual string GetString(string name, bool ignoreCase)
	{
		object objectInternal = GetObjectInternal(name);
		string text;
		try
		{
			text = (string)objectInternal;
		}
		catch (InvalidCastException)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Resource '{0}' was not a String - call GetObject instead.", name));
		}
		if (text != null || !ignoreCase)
		{
			return text;
		}
		objectInternal = GetCaseInsensitiveObjectInternal(name);
		try
		{
			return (string)objectInternal;
		}
		catch (InvalidCastException)
		{
			throw new InvalidOperationException(Environment.GetResourceString("Resource '{0}' was not a String - call GetObject instead.", name));
		}
	}

	/// <summary>Searches for a resource object with the specified name.</summary>
	/// <returns>The requested resource.</returns>
	/// <param name="name">Case-sensitive name of the resource to search for. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has been closed or disposed.</exception>
	public virtual object GetObject(string name)
	{
		return GetObjectInternal(name);
	}

	/// <summary>Searches for a resource object with the specified name in a case-insensitive manner, if requested.</summary>
	/// <returns>The requested resource.</returns>
	/// <param name="name">Name of the resource to search for. </param>
	/// <param name="ignoreCase">Indicates whether the case of the specified name should be ignored. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="name" /> parameter is null. </exception>
	/// <exception cref="T:System.ObjectDisposedException">The object has been closed or disposed.</exception>
	public virtual object GetObject(string name, bool ignoreCase)
	{
		object objectInternal = GetObjectInternal(name);
		if (objectInternal != null || !ignoreCase)
		{
			return objectInternal;
		}
		return GetCaseInsensitiveObjectInternal(name);
	}

	/// <summary>Reads all the resources and stores them in a <see cref="T:System.Collections.Hashtable" /> indicated in the <see cref="F:System.Resources.ResourceSet.Table" /> property.</summary>
	protected virtual void ReadResources()
	{
		IDictionaryEnumerator enumerator = Reader.GetEnumerator();
		while (enumerator.MoveNext())
		{
			object value = enumerator.Value;
			Table.Add(enumerator.Key, value);
		}
	}

	private object GetObjectInternal(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return (Table ?? throw new ObjectDisposedException(null, Environment.GetResourceString("Cannot access a closed resource set.")))[name];
	}

	private object GetCaseInsensitiveObjectInternal(string name)
	{
		Hashtable table = Table;
		if (table == null)
		{
			throw new ObjectDisposedException(null, Environment.GetResourceString("Cannot access a closed resource set."));
		}
		Hashtable hashtable = _caseInsensitiveTable;
		if (hashtable == null)
		{
			hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
			IDictionaryEnumerator enumerator = table.GetEnumerator();
			while (enumerator.MoveNext())
			{
				hashtable.Add(enumerator.Key, enumerator.Value);
			}
			_caseInsensitiveTable = hashtable;
		}
		return hashtable[name];
	}
}
