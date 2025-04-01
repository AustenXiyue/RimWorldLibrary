using System.Collections;
using System.IO;

namespace System.CodeDom.Compiler;

/// <summary>Represents a collection of temporary files.</summary>
[Serializable]
public class TempFileCollection : ICollection, IEnumerable, IDisposable
{
	private string _basePath;

	private readonly string _tempDir;

	private readonly Hashtable _files;

	/// <summary>Gets the number of files in the collection.</summary>
	/// <returns>The number of files in the collection.</returns>
	public int Count => _files.Count;

	/// <summary>Gets the number of elements contained in the collection.</summary>
	/// <returns>The number of elements contained in the <see cref="T:System.Collections.ICollection" />.</returns>
	int ICollection.Count => _files.Count;

	/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection" />.</returns>
	object ICollection.SyncRoot => null;

	/// <summary>Gets a value indicating whether access to the collection is synchronized (thread safe).</summary>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection" /> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets the temporary directory to store the temporary files in.</summary>
	/// <returns>The temporary directory to store the temporary files in.</returns>
	public string TempDir => _tempDir ?? string.Empty;

	/// <summary>Gets the full path to the base file name, without a file name extension, on the temporary directory path, that is used to generate temporary file names for the collection.</summary>
	/// <returns>The full path to the base file name, without a file name extension, on the temporary directory path, that is used to generate temporary file names for the collection.</returns>
	/// <exception cref="T:System.Security.SecurityException">If the <see cref="P:System.CodeDom.Compiler.TempFileCollection.BasePath" /> property has not been set or is set to null, and <see cref="F:System.Security.Permissions.FileIOPermissionAccess.AllAccess" /> is not granted for the temporary directory indicated by the <see cref="P:System.CodeDom.Compiler.TempFileCollection.TempDir" /> property. </exception>
	public string BasePath
	{
		get
		{
			EnsureTempNameCreated();
			return _basePath;
		}
	}

	/// <summary>Gets or sets a value indicating whether to keep the files, by default, when the <see cref="M:System.CodeDom.Compiler.TempFileCollection.Delete" /> method is called or the collection is disposed.</summary>
	/// <returns>true if the files should be kept; otherwise, false.</returns>
	public bool KeepFiles { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.TempFileCollection" /> class with default values.</summary>
	public TempFileCollection()
		: this(null, keepFiles: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.TempFileCollection" /> class using the specified temporary directory that is set to delete the temporary files after their generation and use, by default.</summary>
	/// <param name="tempDir">A path to the temporary directory to use for storing the temporary files. </param>
	public TempFileCollection(string tempDir)
		: this(tempDir, keepFiles: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.Compiler.TempFileCollection" /> class using the specified temporary directory and specified value indicating whether to keep or delete the temporary files after their generation and use, by default.</summary>
	/// <param name="tempDir">A path to the temporary directory to use for storing the temporary files. </param>
	/// <param name="keepFiles">true if the temporary files should be kept after use; false if the temporary files should be deleted. </param>
	public TempFileCollection(string tempDir, bool keepFiles)
	{
		KeepFiles = keepFiles;
		_tempDir = tempDir;
		_files = new Hashtable(StringComparer.OrdinalIgnoreCase);
	}

	/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. </summary>
	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.CodeDom.Compiler.TempFileCollection" /> and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		SafeDelete();
	}

	/// <summary>Attempts to delete the temporary files before this object is reclaimed by garbage collection.</summary>
	~TempFileCollection()
	{
		Dispose(disposing: false);
	}

	/// <summary>Adds a file name with the specified file name extension to the collection.</summary>
	/// <returns>A file name with the specified extension that was just added to the collection.</returns>
	/// <param name="fileExtension">The file name extension for the auto-generated temporary file name to add to the collection. </param>
	public string AddExtension(string fileExtension)
	{
		return AddExtension(fileExtension, KeepFiles);
	}

	/// <summary>Adds a file name with the specified file name extension to the collection, using the specified value indicating whether the file should be deleted or retained.</summary>
	/// <returns>A file name with the specified extension that was just added to the collection.</returns>
	/// <param name="fileExtension">The file name extension for the auto-generated temporary file name to add to the collection. </param>
	/// <param name="keepFile">true if the file should be kept after use; false if the file should be deleted. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fileExtension" /> is null or an empty string.</exception>
	public string AddExtension(string fileExtension, bool keepFile)
	{
		if (string.IsNullOrEmpty(fileExtension))
		{
			throw new ArgumentException(global::SR.Format("Argument {0} cannot be null or zero-length.", "fileExtension"), "fileExtension");
		}
		string text = BasePath + "." + fileExtension;
		AddFile(text, keepFile);
		return text;
	}

	/// <summary>Adds the specified file to the collection, using the specified value indicating whether to keep the file after the collection is disposed or when the <see cref="M:System.CodeDom.Compiler.TempFileCollection.Delete" /> method is called.</summary>
	/// <param name="fileName">The name of the file to add to the collection. </param>
	/// <param name="keepFile">true if the file should be kept after use; false if the file should be deleted. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fileName" /> is null or an empty string.-or-<paramref name="fileName" /> is a duplicate.</exception>
	public void AddFile(string fileName, bool keepFile)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentException(global::SR.Format("Argument {0} cannot be null or zero-length.", "fileName"), "fileName");
		}
		if (_files[fileName] != null)
		{
			throw new ArgumentException(global::SR.Format("The file name '{0}' was already in the collection.", fileName), "fileName");
		}
		_files.Add(fileName, keepFile);
	}

	/// <summary>Gets an enumerator that can enumerate the members of the collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that contains the collection's members.</returns>
	public IEnumerator GetEnumerator()
	{
		return _files.Keys.GetEnumerator();
	}

	/// <summary>Returns an enumerator that iterates through a collection. </summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return _files.Keys.GetEnumerator();
	}

	/// <summary>Copies the elements of the collection to an array, starting at the specified index of the target array. </summary>
	/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
	/// <param name="start">The zero-based index in array at which copying begins.</param>
	void ICollection.CopyTo(Array array, int start)
	{
		_files.Keys.CopyTo(array, start);
	}

	/// <summary>Copies the members of the collection to the specified string, beginning at the specified index.</summary>
	/// <param name="fileNames">The array of strings to copy to. </param>
	/// <param name="start">The index of the array to begin copying to. </param>
	public void CopyTo(string[] fileNames, int start)
	{
		_files.Keys.CopyTo(fileNames, start);
	}

	private void EnsureTempNameCreated()
	{
		if (_basePath != null)
		{
			return;
		}
		string text = null;
		bool flag = false;
		int num = 5000;
		do
		{
			_basePath = Path.Combine(string.IsNullOrEmpty(TempDir) ? Path.GetTempPath() : TempDir, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
			text = _basePath + ".tmp";
			try
			{
				new FileStream(text, FileMode.CreateNew, FileAccess.Write).Dispose();
				flag = true;
			}
			catch (IOException ex)
			{
				num--;
				if (num == 0 || ex is DirectoryNotFoundException)
				{
					throw;
				}
				flag = false;
			}
		}
		while (!flag);
		_files.Add(text, KeepFiles);
	}

	private bool KeepFile(string fileName)
	{
		object obj = _files[fileName];
		if (obj == null)
		{
			return false;
		}
		return (bool)obj;
	}

	/// <summary>Deletes the temporary files within this collection that were not marked to be kept.</summary>
	public void Delete()
	{
		SafeDelete();
	}

	internal void Delete(string fileName)
	{
		try
		{
			File.Delete(fileName);
		}
		catch
		{
		}
	}

	internal void SafeDelete()
	{
		if (_files == null || _files.Count <= 0)
		{
			return;
		}
		string[] array = new string[_files.Count];
		_files.Keys.CopyTo(array, 0);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!KeepFile(text))
			{
				Delete(text);
				_files.Remove(text);
			}
		}
	}
}
