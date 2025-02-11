using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Ink;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace System.Windows;

/// <summary>Provides a basic implementation of the <see cref="T:System.Windows.IDataObject" /> interface, which defines a format-independent mechanism for transferring data.</summary>
public sealed class DataObject : IDataObject, System.Runtime.InteropServices.ComTypes.IDataObject
{
	private class FormatEnumerator : IEnumFORMATETC
	{
		private readonly FORMATETC[] _formats;

		private int _current;

		internal FormatEnumerator(DataObject dataObject)
		{
			string[] formats = dataObject.GetFormats();
			_formats = new FORMATETC[(formats != null) ? formats.Length : 0];
			if (formats == null)
			{
				return;
			}
			for (int i = 0; i < formats.Length; i++)
			{
				string text = formats[i];
				FORMATETC fORMATETC = new FORMATETC
				{
					cfFormat = (short)DataFormats.GetDataFormat(text).Id,
					dwAspect = DVASPECT.DVASPECT_CONTENT,
					ptd = IntPtr.Zero,
					lindex = -1
				};
				if (IsFormatEqual(text, DataFormats.Bitmap))
				{
					fORMATETC.tymed = TYMED.TYMED_GDI;
				}
				else if (IsFormatEqual(text, DataFormats.EnhancedMetafile))
				{
					fORMATETC.tymed = TYMED.TYMED_ENHMF;
				}
				else
				{
					fORMATETC.tymed = TYMED.TYMED_HGLOBAL;
				}
				_formats[i] = fORMATETC;
			}
		}

		private FormatEnumerator(FormatEnumerator formatEnumerator)
		{
			_formats = formatEnumerator._formats;
			_current = formatEnumerator._current;
		}

		public int Next(int celt, FORMATETC[] rgelt, int[] pceltFetched)
		{
			int num = 0;
			if (rgelt == null)
			{
				return -2147024809;
			}
			for (int i = 0; i < celt; i++)
			{
				if (_current >= _formats.Length)
				{
					break;
				}
				rgelt[i] = _formats[_current];
				_current++;
				num++;
			}
			if (pceltFetched != null)
			{
				pceltFetched[0] = num;
			}
			return (num != celt) ? 1 : 0;
		}

		public int Skip(int celt)
		{
			_current += Math.Min(celt, int.MaxValue - _current);
			return (_current >= _formats.Length) ? 1 : 0;
		}

		public int Reset()
		{
			_current = 0;
			return 0;
		}

		public void Clone(out IEnumFORMATETC ppenum)
		{
			ppenum = new FormatEnumerator(this);
		}
	}

	private class OleConverter : IDataObject
	{
		private class TypeRestrictingSerializationBinder : SerializationBinder
		{
			public override Type BindToType(string assemblyName, string typeName)
			{
				throw new RestrictedTypeDeserializationException();
			}
		}

		private class RestrictedTypeDeserializationException : Exception
		{
		}

		internal System.Runtime.InteropServices.ComTypes.IDataObject _innerData;

		public System.Runtime.InteropServices.ComTypes.IDataObject OleDataObject => _innerData;

		public OleConverter(System.Runtime.InteropServices.ComTypes.IDataObject data)
		{
			_innerData = data;
		}

		public object GetData(string format)
		{
			return GetData(format, autoConvert: true);
		}

		public object GetData(Type format)
		{
			return GetData(format.FullName);
		}

		public object GetData(string format, bool autoConvert)
		{
			return GetData(format, autoConvert, DVASPECT.DVASPECT_CONTENT, -1);
		}

		public bool GetDataPresent(string format)
		{
			return GetDataPresent(format, autoConvert: true);
		}

		public bool GetDataPresent(Type format)
		{
			return GetDataPresent(format.FullName);
		}

		public bool GetDataPresent(string format, bool autoConvert)
		{
			return GetDataPresent(format, autoConvert, DVASPECT.DVASPECT_CONTENT, -1);
		}

		public string[] GetFormats()
		{
			return GetFormats(autoConvert: true);
		}

		public void SetData(object data)
		{
			if (data is ISerializable)
			{
				SetData(DataFormats.Serializable, data);
			}
			else
			{
				SetData(data.GetType(), data);
			}
		}

		public string[] GetFormats(bool autoConvert)
		{
			IEnumFORMATETC enumFORMATETC = null;
			ArrayList arrayList = new ArrayList();
			enumFORMATETC = EnumFormatEtcInner(DATADIR.DATADIR_GET);
			if (enumFORMATETC != null)
			{
				enumFORMATETC.Reset();
				FORMATETC[] array = new FORMATETC[1];
				int[] array2 = new int[1] { 1 };
				while (array2[0] > 0)
				{
					array2[0] = 0;
					if (enumFORMATETC.Next(1, array, array2) != 0 || array2[0] <= 0)
					{
						continue;
					}
					string name = DataFormats.GetDataFormat(array[0].cfFormat).Name;
					if (autoConvert)
					{
						string[] mappedFormats = GetMappedFormats(name);
						for (int i = 0; i < mappedFormats.Length; i++)
						{
							arrayList.Add(mappedFormats[i]);
						}
					}
					else
					{
						arrayList.Add(name);
					}
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j].ptd != IntPtr.Zero)
						{
							Marshal.FreeCoTaskMem(array[j].ptd);
						}
					}
				}
			}
			string[] array3 = new string[arrayList.Count];
			arrayList.CopyTo(array3, 0);
			return GetDistinctStrings(array3);
		}

		public void SetData(string format, object data)
		{
			SetData(format, data, autoConvert: true);
		}

		public void SetData(Type format, object data)
		{
			SetData(format.FullName, data);
		}

		public void SetData(string format, object data, bool autoConvert)
		{
			SetData(format, data, autoConvert: true, DVASPECT.DVASPECT_CONTENT, 0);
		}

		private object GetData(string format, bool autoConvert, DVASPECT aspect, int index)
		{
			object obj = GetDataFromBoundOleDataObject(format, aspect, index);
			object obj2 = obj;
			if (autoConvert && (obj == null || obj is MemoryStream))
			{
				string[] mappedFormats = GetMappedFormats(format);
				if (mappedFormats != null)
				{
					for (int i = 0; i < mappedFormats.Length; i++)
					{
						if (IsFormatEqual(format, mappedFormats[i]))
						{
							continue;
						}
						obj = GetDataFromBoundOleDataObject(mappedFormats[i], aspect, index);
						if (obj != null && !(obj is MemoryStream))
						{
							if (IsDataSystemBitmapSource(obj) || SystemDrawingHelper.IsBitmap(obj))
							{
								obj = EnsureBitmapDataFromFormat(format, autoConvert, obj);
							}
							obj2 = null;
							break;
						}
					}
				}
			}
			if (obj2 != null)
			{
				return obj2;
			}
			return obj;
		}

		private bool GetDataPresent(string format, bool autoConvert, DVASPECT aspect, int index)
		{
			bool dataPresentInner = GetDataPresentInner(format, aspect, index);
			if (!dataPresentInner && autoConvert)
			{
				string[] mappedFormats = GetMappedFormats(format);
				if (mappedFormats != null)
				{
					for (int i = 0; i < mappedFormats.Length; i++)
					{
						if (!IsFormatEqual(format, mappedFormats[i]))
						{
							dataPresentInner = GetDataPresentInner(mappedFormats[i], aspect, index);
							if (dataPresentInner)
							{
								break;
							}
						}
					}
				}
			}
			return dataPresentInner;
		}

		private void SetData(string format, object data, bool autoConvert, DVASPECT aspect, int index)
		{
			throw new InvalidOperationException(SR.DataObject_CannotSetDataOnAFozenOLEDataDbject);
		}

		private object GetDataFromOleIStream(string format, DVASPECT aspect, int index)
		{
			FORMATETC formatetc = default(FORMATETC);
			formatetc.cfFormat = (short)DataFormats.GetDataFormat(format).Id;
			formatetc.dwAspect = aspect;
			formatetc.lindex = index;
			formatetc.tymed = TYMED.TYMED_ISTREAM;
			object result = null;
			if (QueryGetDataInner(ref formatetc) == 0)
			{
				GetDataInner(ref formatetc, out var medium);
				try
				{
					if (medium.unionmember != IntPtr.Zero && medium.tymed == TYMED.TYMED_ISTREAM)
					{
						MS.Win32.UnsafeNativeMethods.IStream stream = (MS.Win32.UnsafeNativeMethods.IStream)Marshal.GetObjectForIUnknown(medium.unionmember);
						MS.Win32.NativeMethods.STATSTG sTATSTG = new MS.Win32.NativeMethods.STATSTG();
						stream.Stat(sTATSTG, 0);
						int num = (int)sTATSTG.cbSize;
						nint num2 = Win32GlobalAlloc(8258, num);
						try
						{
							nint buf = Win32GlobalLock(new HandleRef(this, num2));
							try
							{
								stream.Seek(0L, 0);
								stream.Read(buf, num);
							}
							finally
							{
								Win32GlobalUnlock(new HandleRef(this, num2));
							}
							result = GetDataFromHGLOBAL(format, num2);
						}
						finally
						{
							Win32GlobalFree(new HandleRef(this, num2));
						}
					}
				}
				finally
				{
					MS.Win32.UnsafeNativeMethods.ReleaseStgMedium(ref medium);
				}
			}
			return result;
		}

		private object GetDataFromHGLOBAL(string format, nint hglobal)
		{
			object result = null;
			if (hglobal != IntPtr.Zero)
			{
				if (IsFormatEqual(format, DataFormats.Html) || IsFormatEqual(format, DataFormats.Xaml))
				{
					result = ReadStringFromHandleUtf8(hglobal);
				}
				else if (IsFormatEqual(format, DataFormats.Text) || IsFormatEqual(format, DataFormats.Rtf) || IsFormatEqual(format, DataFormats.OemText) || IsFormatEqual(format, DataFormats.CommaSeparatedValue))
				{
					result = ReadStringFromHandle(hglobal, unicode: false);
				}
				else if (IsFormatEqual(format, DataFormats.UnicodeText) || IsFormatEqual(format, "ApplicationTrust"))
				{
					result = ReadStringFromHandle(hglobal, unicode: true);
				}
				else if (IsFormatEqual(format, DataFormats.FileDrop))
				{
					result = ReadFileListFromHandle(hglobal);
				}
				else if (IsFormatEqual(format, "FileName"))
				{
					result = new string[1] { ReadStringFromHandle(hglobal, unicode: false) };
				}
				else if (IsFormatEqual(format, "FileNameW"))
				{
					result = new string[1] { ReadStringFromHandle(hglobal, unicode: true) };
				}
				else if (IsFormatEqual(format, typeof(BitmapSource).FullName))
				{
					result = ReadBitmapSourceFromHandle(hglobal);
				}
				else
				{
					bool restrictDeserialization = IsFormatEqual(format, DataFormats.StringFormat) || IsFormatEqual(format, DataFormats.Dib) || IsFormatEqual(format, DataFormats.Bitmap) || IsFormatEqual(format, DataFormats.EnhancedMetafile) || IsFormatEqual(format, DataFormats.MetafilePicture) || IsFormatEqual(format, DataFormats.SymbolicLink) || IsFormatEqual(format, DataFormats.Dif) || IsFormatEqual(format, DataFormats.Tiff) || IsFormatEqual(format, DataFormats.Palette) || IsFormatEqual(format, DataFormats.PenData) || IsFormatEqual(format, DataFormats.Riff) || IsFormatEqual(format, DataFormats.WaveAudio) || IsFormatEqual(format, DataFormats.Locale);
					result = ReadObjectFromHandle(hglobal, restrictDeserialization);
				}
			}
			return result;
		}

		private object GetDataFromOleHGLOBAL(string format, DVASPECT aspect, int index)
		{
			FORMATETC formatetc = default(FORMATETC);
			formatetc.cfFormat = (short)DataFormats.GetDataFormat(format).Id;
			formatetc.dwAspect = aspect;
			formatetc.lindex = index;
			formatetc.tymed = TYMED.TYMED_HGLOBAL;
			object result = null;
			if (QueryGetDataInner(ref formatetc) == 0)
			{
				GetDataInner(ref formatetc, out var medium);
				try
				{
					if (medium.unionmember != IntPtr.Zero && medium.tymed == TYMED.TYMED_HGLOBAL)
					{
						result = GetDataFromHGLOBAL(format, medium.unionmember);
					}
				}
				finally
				{
					MS.Win32.UnsafeNativeMethods.ReleaseStgMedium(ref medium);
				}
			}
			return result;
		}

		private object GetDataFromOleOther(string format, DVASPECT aspect, int index)
		{
			FORMATETC formatetc = default(FORMATETC);
			TYMED tYMED = TYMED.TYMED_NULL;
			if (IsFormatEqual(format, DataFormats.Bitmap))
			{
				tYMED = TYMED.TYMED_GDI;
			}
			else if (IsFormatEqual(format, DataFormats.EnhancedMetafile))
			{
				tYMED = TYMED.TYMED_ENHMF;
			}
			if (tYMED == TYMED.TYMED_NULL)
			{
				return null;
			}
			formatetc.cfFormat = (short)DataFormats.GetDataFormat(format).Id;
			formatetc.dwAspect = aspect;
			formatetc.lindex = index;
			formatetc.tymed = tYMED;
			object result = null;
			if (QueryGetDataInner(ref formatetc) == 0)
			{
				GetDataInner(ref formatetc, out var medium);
				try
				{
					if (medium.unionmember != IntPtr.Zero)
					{
						if (IsFormatEqual(format, DataFormats.Bitmap))
						{
							result = GetBitmapSourceFromHbitmap(medium.unionmember);
						}
						else if (IsFormatEqual(format, DataFormats.EnhancedMetafile))
						{
							result = SystemDrawingHelper.GetMetafileFromHemf(medium.unionmember);
						}
					}
				}
				finally
				{
					MS.Win32.UnsafeNativeMethods.ReleaseStgMedium(ref medium);
				}
			}
			return result;
		}

		private object GetDataFromBoundOleDataObject(string format, DVASPECT aspect, int index)
		{
			object obj = null;
			obj = GetDataFromOleOther(format, aspect, index);
			if (obj == null)
			{
				obj = GetDataFromOleHGLOBAL(format, aspect, index);
			}
			if (obj == null)
			{
				obj = GetDataFromOleIStream(format, aspect, index);
			}
			return obj;
		}

		private Stream ReadByteStreamFromHandle(nint handle, out bool isSerializedObject)
		{
			nint source = Win32GlobalLock(new HandleRef(this, handle));
			try
			{
				int num = MS.Win32.NativeMethods.IntPtrToInt32(Win32GlobalSize(new HandleRef(this, handle)));
				byte[] array = new byte[num];
				Marshal.Copy(source, array, 0, num);
				int num2 = 0;
				if (num > _serializedObjectID.Length)
				{
					isSerializedObject = true;
					for (int i = 0; i < _serializedObjectID.Length; i++)
					{
						if (_serializedObjectID[i] != array[i])
						{
							isSerializedObject = false;
							break;
						}
					}
					if (isSerializedObject)
					{
						num2 = _serializedObjectID.Length;
					}
				}
				else
				{
					isSerializedObject = false;
				}
				return new MemoryStream(array, num2, array.Length - num2);
			}
			finally
			{
				Win32GlobalUnlock(new HandleRef(this, handle));
			}
		}

		private object ReadObjectFromHandle(nint handle, bool restrictDeserialization)
		{
			object obj = null;
			bool isSerializedObject;
			Stream stream = ReadByteStreamFromHandle(handle, out isSerializedObject);
			if (isSerializedObject)
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				if (restrictDeserialization)
				{
					binaryFormatter.Binder = new TypeRestrictingSerializationBinder();
				}
				try
				{
					return binaryFormatter.Deserialize(stream);
				}
				catch (RestrictedTypeDeserializationException)
				{
					return null;
				}
			}
			return stream;
		}

		private BitmapSource ReadBitmapSourceFromHandle(nint handle)
		{
			BitmapSource result = null;
			bool isSerializedObject;
			Stream stream = ReadByteStreamFromHandle(handle, out isSerializedObject);
			if (stream != null)
			{
				result = BitmapFrame.Create(stream);
			}
			return result;
		}

		private string[] ReadFileListFromHandle(nint hdrop)
		{
			string[] array = null;
			StringBuilder stringBuilder = new StringBuilder(260);
			int num = MS.Win32.UnsafeNativeMethods.DragQueryFile(new HandleRef(this, hdrop), -1, null, 0);
			if (num > 0)
			{
				array = new string[num];
				for (int i = 0; i < num; i++)
				{
					if (MS.Win32.UnsafeNativeMethods.DragQueryFile(new HandleRef(this, hdrop), i, stringBuilder, stringBuilder.Capacity) != 0)
					{
						array[i] = stringBuilder.ToString();
					}
				}
			}
			return array;
		}

		private unsafe string ReadStringFromHandle(nint handle, bool unicode)
		{
			string result = null;
			nint value = Win32GlobalLock(new HandleRef(this, handle));
			try
			{
				result = ((!unicode) ? new string((sbyte*)value) : new string((char*)value));
			}
			finally
			{
				Win32GlobalUnlock(new HandleRef(this, handle));
			}
			return result;
		}

		private string ReadStringFromHandleUtf8(nint handle)
		{
			string result = null;
			int num = MS.Win32.NativeMethods.IntPtrToInt32(Win32GlobalSize(new HandleRef(this, handle)));
			nint num2 = Win32GlobalLock(new HandleRef(this, handle));
			try
			{
				int i;
				for (i = 0; i < num && Marshal.ReadByte((nint)((long)num2 + (long)i)) != 0; i++)
				{
				}
				if (i > 0)
				{
					byte[] array = new byte[i];
					Marshal.Copy(num2, array, 0, i);
					result = new UTF8Encoding().GetString(array, 0, i);
				}
			}
			finally
			{
				Win32GlobalUnlock(new HandleRef(this, handle));
			}
			return result;
		}

		private bool GetDataPresentInner(string format, DVASPECT aspect, int index)
		{
			FORMATETC formatetc = default(FORMATETC);
			formatetc.cfFormat = (short)DataFormats.GetDataFormat(format).Id;
			formatetc.dwAspect = aspect;
			formatetc.lindex = index;
			for (int i = 0; i < ALLOWED_TYMEDS.Length; i++)
			{
				formatetc.tymed |= ALLOWED_TYMEDS[i];
			}
			return QueryGetDataInner(ref formatetc) == 0;
		}

		private int QueryGetDataInner(ref FORMATETC formatetc)
		{
			return _innerData.QueryGetData(ref formatetc);
		}

		private void GetDataInner(ref FORMATETC formatetc, out STGMEDIUM medium)
		{
			_innerData.GetData(ref formatetc, out medium);
		}

		private IEnumFORMATETC EnumFormatEtcInner(DATADIR dwDirection)
		{
			return _innerData.EnumFormatEtc(dwDirection);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private object GetBitmapSourceFromHbitmap(nint hbitmap)
		{
			return Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, null);
		}
	}

	private class DataStore : IDataObject
	{
		private class DataStoreEntry
		{
			private object _data;

			private bool _autoConvert;

			private DVASPECT _aspect;

			private int _index;

			public object Data
			{
				get
				{
					return _data;
				}
				set
				{
					_data = value;
				}
			}

			public bool AutoConvert => _autoConvert;

			public DVASPECT Aspect => _aspect;

			public int Index => _index;

			public DataStoreEntry(object data, bool autoConvert, DVASPECT aspect, int index)
			{
				_data = data;
				_autoConvert = autoConvert;
				_aspect = aspect;
				_index = index;
			}
		}

		private Hashtable _data = new Hashtable();

		public object GetData(string format)
		{
			return GetData(format, autoConvert: true);
		}

		public object GetData(Type format)
		{
			return GetData(format.FullName);
		}

		public object GetData(string format, bool autoConvert)
		{
			return GetData(format, autoConvert, DVASPECT.DVASPECT_CONTENT, -1);
		}

		public bool GetDataPresent(string format)
		{
			return GetDataPresent(format, autoConvert: true);
		}

		public bool GetDataPresent(Type format)
		{
			return GetDataPresent(format.FullName);
		}

		public bool GetDataPresent(string format, bool autoConvert)
		{
			return GetDataPresent(format, autoConvert, DVASPECT.DVASPECT_CONTENT, -1);
		}

		public string[] GetFormats()
		{
			return GetFormats(autoConvert: true);
		}

		public string[] GetFormats(bool autoConvert)
		{
			bool flag = false;
			string[] array = new string[_data.Keys.Count];
			_data.Keys.CopyTo(array, 0);
			if (autoConvert)
			{
				ArrayList arrayList = new ArrayList();
				for (int i = 0; i < array.Length; i++)
				{
					DataStoreEntry[] array2 = (DataStoreEntry[])_data[array[i]];
					bool flag2 = true;
					for (int j = 0; j < array2.Length; j++)
					{
						if (!array2[j].AutoConvert)
						{
							flag2 = false;
							break;
						}
					}
					if (flag2)
					{
						string[] mappedFormats = GetMappedFormats(array[i]);
						for (int k = 0; k < mappedFormats.Length; k++)
						{
							bool flag3 = false;
							int num = 0;
							while (!flag3 && num < array2.Length)
							{
								if (IsFormatAndDataSerializable(mappedFormats[k], array2[num].Data) && flag)
								{
									flag = true;
									flag3 = true;
								}
								num++;
							}
							if (!flag3)
							{
								arrayList.Add(mappedFormats[k]);
							}
						}
					}
					else if (!flag)
					{
						arrayList.Add(array[i]);
					}
				}
				string[] array3 = new string[arrayList.Count];
				arrayList.CopyTo(array3, 0);
				array = GetDistinctStrings(array3);
			}
			return array;
		}

		public void SetData(object data)
		{
			if (data is ISerializable && !_data.ContainsKey(DataFormats.Serializable))
			{
				SetData(DataFormats.Serializable, data);
			}
			SetData(data.GetType(), data);
		}

		public void SetData(string format, object data)
		{
			SetData(format, data, autoConvert: true);
		}

		public void SetData(Type format, object data)
		{
			SetData(format.FullName, data);
		}

		public void SetData(string format, object data, bool autoConvert)
		{
			if (IsFormatEqual(format, DataFormats.Dib) && autoConvert && (SystemDrawingHelper.IsBitmap(data) || IsDataSystemBitmapSource(data)))
			{
				format = DataFormats.Bitmap;
			}
			SetData(format, data, autoConvert, DVASPECT.DVASPECT_CONTENT, 0);
		}

		private object GetData(string format, bool autoConvert, DVASPECT aspect, int index)
		{
			DataStoreEntry dataStoreEntry = FindDataStoreEntry(format, aspect, index);
			object obj = GetDataFromDataStoreEntry(dataStoreEntry, format);
			object obj2 = obj;
			if (autoConvert && (dataStoreEntry == null || dataStoreEntry.AutoConvert) && (obj == null || obj is MemoryStream))
			{
				string[] mappedFormats = GetMappedFormats(format);
				if (mappedFormats != null)
				{
					for (int i = 0; i < mappedFormats.Length; i++)
					{
						if (IsFormatEqual(format, mappedFormats[i]))
						{
							continue;
						}
						DataStoreEntry dataStoreEntry2 = FindDataStoreEntry(mappedFormats[i], aspect, index);
						obj = GetDataFromDataStoreEntry(dataStoreEntry2, mappedFormats[i]);
						if (obj != null && !(obj is MemoryStream))
						{
							if (IsDataSystemBitmapSource(obj) || SystemDrawingHelper.IsBitmap(obj))
							{
								obj = EnsureBitmapDataFromFormat(format, autoConvert, obj);
							}
							obj2 = null;
							break;
						}
					}
				}
			}
			if (obj2 != null)
			{
				return obj2;
			}
			return obj;
		}

		private bool GetDataPresent(string format, bool autoConvert, DVASPECT aspect, int index)
		{
			if (!autoConvert)
			{
				if (!_data.ContainsKey(format))
				{
					return false;
				}
				DataStoreEntry[] array = (DataStoreEntry[])_data[format];
				DataStoreEntry dataStoreEntry = null;
				DataStoreEntry dataStoreEntry2 = null;
				foreach (DataStoreEntry dataStoreEntry3 in array)
				{
					if (dataStoreEntry3.Aspect == aspect && (index == -1 || dataStoreEntry3.Index == index))
					{
						dataStoreEntry = dataStoreEntry3;
						break;
					}
					if (dataStoreEntry3.Aspect == DVASPECT.DVASPECT_CONTENT && dataStoreEntry3.Index == 0)
					{
						dataStoreEntry2 = dataStoreEntry3;
					}
				}
				if (dataStoreEntry == null && dataStoreEntry2 != null)
				{
					dataStoreEntry = dataStoreEntry2;
				}
				return dataStoreEntry != null;
			}
			string[] formats = GetFormats(autoConvert);
			for (int j = 0; j < formats.Length; j++)
			{
				if (IsFormatEqual(format, formats[j]))
				{
					return true;
				}
			}
			return false;
		}

		private void SetData(string format, object data, bool autoConvert, DVASPECT aspect, int index)
		{
			DataStoreEntry dataStoreEntry = new DataStoreEntry(data, autoConvert, aspect, index);
			DataStoreEntry[] array = (DataStoreEntry[])_data[format];
			if (array == null)
			{
				array = (DataStoreEntry[])Array.CreateInstance(typeof(DataStoreEntry), 1);
			}
			else
			{
				DataStoreEntry[] array2 = (DataStoreEntry[])Array.CreateInstance(typeof(DataStoreEntry), array.Length + 1);
				array.CopyTo(array2, 1);
				array = array2;
			}
			array[0] = dataStoreEntry;
			_data[format] = array;
		}

		private DataStoreEntry FindDataStoreEntry(string format, DVASPECT aspect, int index)
		{
			DataStoreEntry[] array = (DataStoreEntry[])_data[format];
			DataStoreEntry dataStoreEntry = null;
			DataStoreEntry dataStoreEntry2 = null;
			if (array != null)
			{
				foreach (DataStoreEntry dataStoreEntry3 in array)
				{
					if (dataStoreEntry3.Aspect == aspect && (index == -1 || dataStoreEntry3.Index == index))
					{
						dataStoreEntry = dataStoreEntry3;
						break;
					}
					if (dataStoreEntry3.Aspect == DVASPECT.DVASPECT_CONTENT && dataStoreEntry3.Index == 0)
					{
						dataStoreEntry2 = dataStoreEntry3;
					}
				}
			}
			if (dataStoreEntry == null && dataStoreEntry2 != null)
			{
				dataStoreEntry = dataStoreEntry2;
			}
			return dataStoreEntry;
		}

		private object GetDataFromDataStoreEntry(DataStoreEntry dataStoreEntry, string format)
		{
			object result = null;
			if (dataStoreEntry != null)
			{
				result = dataStoreEntry.Data;
			}
			return result;
		}
	}

	/// <summary>Identifies the <see cref="E:System.Windows.DataObject.Copying" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DataObject.Copying" />  attached event.</returns>
	public static readonly RoutedEvent CopyingEvent = EventManager.RegisterRoutedEvent("Copying", RoutingStrategy.Bubble, typeof(DataObjectCopyingEventHandler), typeof(DataObject));

	/// <summary>Identifies the <see cref="E:System.Windows.DataObject.Pasting" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DataObject.Pasting" />  attached event.</returns>
	public static readonly RoutedEvent PastingEvent = EventManager.RegisterRoutedEvent("Pasting", RoutingStrategy.Bubble, typeof(DataObjectPastingEventHandler), typeof(DataObject));

	/// <summary>Identifies the <see cref="E:System.Windows.DataObject.SettingData" /> attached event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.DataObject.SettingData" /> attached event.</returns>
	public static readonly RoutedEvent SettingDataEvent = EventManager.RegisterRoutedEvent("SettingData", RoutingStrategy.Bubble, typeof(DataObjectSettingDataEventHandler), typeof(DataObject));

	private const string SystemDrawingBitmapFormat = "System.Drawing.Bitmap";

	private const string SystemBitmapSourceFormat = "System.Windows.Media.Imaging.BitmapSource";

	private const string SystemDrawingImagingMetafileFormat = "System.Drawing.Imaging.Metafile";

	private const int DV_E_FORMATETC = -2147221404;

	private const int DV_E_LINDEX = -2147221400;

	private const int DV_E_TYMED = -2147221399;

	private const int DV_E_DVASPECT = -2147221397;

	private const int OLE_E_NOTRUNNING = -2147221499;

	private const int OLE_E_ADVISENOTSUPPORTED = -2147221501;

	private const int DATA_S_SAMEFORMATETC = 262448;

	private const int STG_E_MEDIUMFULL = -2147286928;

	private const int FILEDROPBASESIZE = 20;

	private static readonly TYMED[] ALLOWED_TYMEDS = new TYMED[5]
	{
		TYMED.TYMED_HGLOBAL,
		TYMED.TYMED_ISTREAM,
		TYMED.TYMED_ENHMF,
		TYMED.TYMED_MFPICT,
		TYMED.TYMED_GDI
	};

	private IDataObject _innerData;

	private static readonly byte[] _serializedObjectID = new Guid(4255033238u, 15123, 17264, 166, 121, 86, 16, 107, 178, 136, 251).ToByteArray();

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DataObject" /> class.</summary>
	public DataObject()
	{
		_innerData = new DataStore();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DataObject" /> class that contains the specified data.</summary>
	/// <param name="data">An object that represents the data to store in this data object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="data" /> references a <see cref="T:System.Windows.DataObject" /> object.</exception>
	public DataObject(object data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data is IDataObject innerData)
		{
			_innerData = innerData;
			return;
		}
		if (data is System.Runtime.InteropServices.ComTypes.IDataObject data2)
		{
			_innerData = new OleConverter(data2);
			return;
		}
		_innerData = new DataStore();
		SetData(data);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DataObject" /> class that contains the specified data and its associated format; the format is specified by a string.</summary>
	/// <param name="format">A string that specifies the format for the data. For a set of predefined data formats, see the       <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <param name="data">An object that represents the data to store in this data object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> or <paramref name="format" /> is null.</exception>
	public DataObject(string format, object data)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format == string.Empty)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		_innerData = new DataStore();
		SetData(format, data);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DataObject" /> class that contains the specified data and its associated format; the data format is specified by a <see cref="T:System.Type" /> object.</summary>
	/// <param name="format">A <see cref="T:System.Type" /> that specifies the format for the data. For a set of predefined data formats, see the <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <param name="data">The data to store in this data object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> or <paramref name="format" /> is null.</exception>
	public DataObject(Type format, object data)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		_innerData = new DataStore();
		SetData(format.FullName, data);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DataObject" /> class that contains the specified data and its associated format; the format is specified by a string. This overload includes a Boolean flag to indicate whether the data may be converted to another format on retrieval.</summary>
	/// <param name="format">A string that specifies the format for the data. For a set of predefined data formats, see the <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <param name="data">The data to store in this data object.</param>
	/// <param name="autoConvert">true to allow the data to be converted to another format on retrieval; false to prohibit the data from being converted to another format on retrieval.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> or <paramref name="format" /> is null.</exception>
	public DataObject(string format, object data, bool autoConvert)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format == string.Empty)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		_innerData = new DataStore();
		SetData(format, data, autoConvert);
	}

	internal DataObject(IDataObject data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		_innerData = data;
	}

	internal DataObject(System.Runtime.InteropServices.ComTypes.IDataObject data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		_innerData = new OleConverter(data);
	}

	/// <summary>Returns a data object in a specified format, optionally converting the data to the specified format.</summary>
	/// <returns>A data object with the data in the specified format, or null if the data is unavailable in the specified format.If the <paramref name="autoConvert" /> parameter is true and the data cannot be converted to the specified format, or if automatic conversion is disabled (by calling <see cref="M:System.Windows.DataObject.SetData(System.String,System.Object,System.Boolean)" /> with the <paramref name="autoConvert" /> parameter set to false), this method returns null.</returns>
	/// <param name="format">A string that specifies the format for the data. For a set of predefined data formats, see the <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <param name="autoConvert">true to attempt to automatically convert the data to the specified format; false for no data format conversion.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null.</exception>
	public object GetData(string format, bool autoConvert)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format == string.Empty)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		return _innerData.GetData(format, autoConvert);
	}

	/// <summary>Returns data in a format specified by a string.</summary>
	/// <returns>An object that contains the data in the specified format, or null if the data is unavailable in the specified format.</returns>
	/// <param name="format">A string that specifies the format for the data. For a set of predefined data formats, see the <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null.</exception>
	public object GetData(string format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format == string.Empty)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		return GetData(format, autoConvert: true);
	}

	/// <summary>Returns a data object in a format specified by a <see cref="T:System.Type" /> object.</summary>
	/// <returns>A data object with the data in the specified format, or null if the data is unavailable in the specified format.</returns>
	/// <param name="format">A <see cref="T:System.Type" /> that specifies the format for the data. For a set of predefined data formats, see the <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null.</exception>
	public object GetData(Type format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		return GetData(format.FullName);
	}

	/// <summary>Determines whether the data is available in, or can be converted to, a format specified by a <see cref="T:System.Type" /> object.</summary>
	/// <returns>true if the data is in, or can be converted to, the specified format; otherwise, false.</returns>
	/// <param name="format">A <see cref="T:System.Type" /> that specifies the data format to check. F or a set of predefined data formats, see the <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null.</exception>
	public bool GetDataPresent(Type format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		return GetDataPresent(format.FullName);
	}

	/// <summary>Determines whether the data is available in, or can be converted to, a specified format. A Boolean flag indicates whether to check if the data can be converted to the specified format if it is not available in that format.</summary>
	/// <returns>true if the data is in, or can be converted to, the specified format; otherwise, false.</returns>
	/// <param name="format">A string that specifies the data format to check. For a set of predefined data formats, see the <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <param name="autoConvert">false to check only for the specified format; true to also check whether data stored in this data object can be converted to the specified format.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null.</exception>
	public bool GetDataPresent(string format, bool autoConvert)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format == string.Empty)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		return _innerData.GetDataPresent(format, autoConvert);
	}

	/// <summary>Determines whether the data is available in, or can be converted to, a format specified by a string.</summary>
	/// <returns>true if the data is in, or can be converted to, the specified format; otherwise, false.</returns>
	/// <param name="format">A string that specifies the format for the data. For a set of predefined data formats, see the <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="format" /> is null.</exception>
	public bool GetDataPresent(string format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format == string.Empty)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		return GetDataPresent(format, autoConvert: true);
	}

	/// <summary>Returns a list of formats in which the data in this data object is stored. A Boolean flag indicates whether to also include formats that the data can be automatically converted to.</summary>
	/// <returns>An array of strings, with each string specifying the name of a format supported by this data object.</returns>
	/// <param name="autoConvert">true to retrieve all formats in which the data in this data object is stored, or can be converted to; false to retrieve only formats in which the data in this data object is stored.</param>
	public string[] GetFormats(bool autoConvert)
	{
		return _innerData.GetFormats(autoConvert);
	}

	/// <summary>Returns a list of formats in which the data in this data object is stored, or can be converted to.</summary>
	/// <returns>An array of strings, with each string specifying the name of a format that this data object supports.</returns>
	public string[] GetFormats()
	{
		return GetFormats(autoConvert: true);
	}

	/// <summary>Stores the specified data in this data object, automatically determining the data format from the source object type.</summary>
	/// <param name="data">An object that represents the data to store in this data object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> is null.</exception>
	public void SetData(object data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		_innerData.SetData(data);
	}

	/// <summary>Stores the specified data in this data object, along with one or more specified data formats; the data format is specified by a string.</summary>
	/// <param name="format">A string that specifies the format for the data. For a set of predefined data formats, see the <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <param name="data">An object that represents the data to store in this data object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> or <paramref name="format" /> is null.</exception>
	public void SetData(string format, object data)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format == string.Empty)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		_innerData.SetData(format, data);
	}

	/// <summary>Stores the specified data in this data object, along with one or more specified data formats; the data format is specified by a <see cref="T:System.Type" /> object.</summary>
	/// <param name="format">A <see cref="T:System.Type" /> object that specifies the format for the data. For a set of predefined data formats, see the <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <param name="data">An object that represents the data to store in this data object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> or <paramref name="format" /> is null.</exception>
	public void SetData(Type format, object data)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		_innerData.SetData(format, data);
	}

	/// <summary>Stores the specified data in this data object, along with one or more specified data formats. This overload includes a Boolean flag to indicate whether the data can be converted to another format on retrieval.</summary>
	/// <param name="format">A string that specifies the format for the data. For a set of predefined data formats, see the <see cref="T:System.Windows.DataFormats" /> class.</param>
	/// <param name="data">An object that represents the data to store in this data object.</param>
	/// <param name="autoConvert">true to allow the data to be converted to another format on retrieval; false to prohibit the data from being converted to another format on retrieval.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="data" /> or <paramref name="format" /> is null.</exception>
	[FriendAccessAllowed]
	public void SetData(string format, object data, bool autoConvert)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		if (format == string.Empty)
		{
			throw new ArgumentException(SR.DataObject_EmptyFormatNotAllowed);
		}
		_innerData.SetData(format, data, autoConvert);
	}

	/// <summary>Queries a data object for the presence of data in the <see cref="F:System.Windows.DataFormats.WaveAudio" /> data format.</summary>
	/// <returns>true if the data object contains data in the <see cref="F:System.Windows.DataFormats.WaveAudio" /> data format; otherwise, false.</returns>
	public bool ContainsAudio()
	{
		return GetDataPresent(DataFormats.WaveAudio, autoConvert: false);
	}

	/// <summary>Queries a data object for the presence of data in the <see cref="F:System.Windows.DataFormats.FileDrop" /> data format.</summary>
	/// <returns>true if the data object contains data in the <see cref="F:System.Windows.DataFormats.FileDrop" /> data format; otherwise, false.</returns>
	public bool ContainsFileDropList()
	{
		return GetDataPresent(DataFormats.FileDrop, autoConvert: false);
	}

	/// <summary>Queries a data object for the presence of data in the <see cref="F:System.Windows.DataFormats.Bitmap" /> data format.</summary>
	/// <returns>true if the data object contains data in the <see cref="F:System.Windows.DataFormats.Bitmap" /> data format; otherwise, false.</returns>
	public bool ContainsImage()
	{
		return GetDataPresent(DataFormats.Bitmap, autoConvert: false);
	}

	/// <summary>Queries a data object for the presence of data in the <see cref="F:System.Windows.DataFormats.UnicodeText" /> format.</summary>
	/// <returns>true if the data object contains data in the <see cref="F:System.Windows.DataFormats.UnicodeText" /> data format; otherwise, false.</returns>
	public bool ContainsText()
	{
		return ContainsText(TextDataFormat.UnicodeText);
	}

	/// <summary>Queries a data object for the presence of data in a text data format.</summary>
	/// <returns>true if the data object contains data in a text data format; otherwise, false.</returns>
	/// <param name="format">A member of the <see cref="T:System.Windows.TextDataFormat" /> enumeration that specifies the text data format to query for.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="format" /> does not specify a valid member of <see cref="T:System.Windows.TextDataFormat" />.</exception>
	public bool ContainsText(TextDataFormat format)
	{
		if (!DataFormats.IsValidTextDataFormat(format))
		{
			throw new InvalidEnumArgumentException("format", (int)format, typeof(TextDataFormat));
		}
		return GetDataPresent(DataFormats.ConvertToDataFormats(format), autoConvert: false);
	}

	/// <summary>Returns a stream that contains data in the <see cref="F:System.Windows.DataFormats.WaveAudio" /> data format.</summary>
	/// <returns>A stream that contains data in the <see cref="F:System.Windows.DataFormats.WaveAudio" /> format, or null if the data is unavailable in this format.</returns>
	public Stream GetAudioStream()
	{
		return GetData(DataFormats.WaveAudio, autoConvert: false) as Stream;
	}

	/// <summary>Returns a string collection that contains a list of dropped files.</summary>
	/// <returns>A collection of strings, where each string specifies the name of a file in the list of dropped files, or null if the data is unavailable in this format.</returns>
	public StringCollection GetFileDropList()
	{
		StringCollection stringCollection = new StringCollection();
		if (GetData(DataFormats.FileDrop, autoConvert: true) is string[] value)
		{
			stringCollection.AddRange(value);
		}
		return stringCollection;
	}

	/// <summary>Returns a <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> object that contains data in the <see cref="F:System.Windows.DataFormats.Bitmap" /> format.</summary>
	/// <returns>A <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> object that contains data in the <see cref="F:System.Windows.DataFormats.Bitmap" /> format, or null if the data is unavailable in this format.</returns>
	public BitmapSource GetImage()
	{
		return GetData(DataFormats.Bitmap, autoConvert: true) as BitmapSource;
	}

	/// <summary>Returns a string that contains the <see cref="F:System.Windows.DataFormats.UnicodeText" /> data in this data object.</summary>
	/// <returns>A string that contains the <see cref="F:System.Windows.DataFormats.UnicodeText" /> data, or an empty string if no <see cref="F:System.Windows.DataFormats.UnicodeText" /> data is available.</returns>
	public string GetText()
	{
		return GetText(TextDataFormat.UnicodeText);
	}

	/// <summary>Returns a string that contains text data of the specified format in this data object.</summary>
	/// <returns>A string containing text data in the specified data format, or an empty string if no corresponding text data is available.</returns>
	/// <param name="format">A member of <see cref="T:System.Windows.TextDataFormat" /> that specifies the specific text data format to retrieve.</param>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
	///   <paramref name="format" /> does not specify a valid member of <see cref="T:System.Windows.TextDataFormat" />.</exception>
	public string GetText(TextDataFormat format)
	{
		if (!DataFormats.IsValidTextDataFormat(format))
		{
			throw new InvalidEnumArgumentException("format", (int)format, typeof(TextDataFormat));
		}
		string text = (string)GetData(DataFormats.ConvertToDataFormats(format), autoConvert: false);
		if (text != null)
		{
			return text;
		}
		return string.Empty;
	}

	/// <summary>Stores audio data (<see cref="F:System.Windows.DataFormats.WaveAudio" /> data format) in this data object. The audio data is specified as a byte array.</summary>
	/// <param name="audioBytes">A byte array that contains audio data to store in the data object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="audioBytes" /> is null.</exception>
	public void SetAudio(byte[] audioBytes)
	{
		if (audioBytes == null)
		{
			throw new ArgumentNullException("audioBytes");
		}
		SetAudio(new MemoryStream(audioBytes));
	}

	/// <summary>Stores audio data (<see cref="F:System.Windows.DataFormats.WaveAudio" /> data format) in this data object.  The audio data is specified as a stream.</summary>
	/// <param name="audioStream">A stream that contains audio data to store in the data object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="audioStream" /> is null.</exception>
	public void SetAudio(Stream audioStream)
	{
		if (audioStream == null)
		{
			throw new ArgumentNullException("audioStream");
		}
		SetData(DataFormats.WaveAudio, audioStream, autoConvert: false);
	}

	/// <summary>Stores <see cref="F:System.Windows.DataFormats.FileDrop" /> data in this data object.  The dropped file list is specified as a string collection.</summary>
	/// <param name="fileDropList">A string collection that contains the dropped file list to store in the data object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fileDropList" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="fileDropList" /> contains zero strings, or the full path to file specified in the list cannot be resolved.</exception>
	public void SetFileDropList(StringCollection fileDropList)
	{
		if (fileDropList == null)
		{
			throw new ArgumentNullException("fileDropList");
		}
		if (fileDropList.Count == 0)
		{
			throw new ArgumentException(SR.Format(SR.DataObject_FileDropListIsEmpty, fileDropList));
		}
		StringEnumerator enumerator = fileDropList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				try
				{
					Path.GetFullPath(current);
				}
				catch (ArgumentException p)
				{
					throw new ArgumentException(SR.Format(SR.DataObject_FileDropListHasInvalidFileDropPath, p));
				}
			}
		}
		finally
		{
			if (enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		string[] array = new string[fileDropList.Count];
		fileDropList.CopyTo(array, 0);
		SetData(DataFormats.FileDrop, array, autoConvert: true);
	}

	/// <summary>Stores <see cref="F:System.Windows.DataFormats.Bitmap" /> data in this data object.  The image data is specified as a <see cref="T:System.Windows.Media.Imaging.BitmapSource" />.</summary>
	/// <param name="image">A <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> object that contains the image data to store in the data object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="image" /> is null.</exception>
	public void SetImage(BitmapSource image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		SetData(DataFormats.Bitmap, image, autoConvert: true);
	}

	/// <summary>Stores <see cref="F:System.Windows.DataFormats.UnicodeText" /> data, specified as a string, in this data object.</summary>
	/// <param name="textData">A string that contains the <see cref="F:System.Windows.DataFormats.UnicodeText" /> data to store in the data object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="textData" /> is null.</exception>
	public void SetText(string textData)
	{
		if (textData == null)
		{
			throw new ArgumentNullException("textData");
		}
		SetText(textData, TextDataFormat.UnicodeText);
	}

	/// <summary>Stores text data in this data object. The format of the text data to store is specified with a member of <see cref="T:System.Windows.TextDataFormat" />.</summary>
	/// <param name="textData">A string that contains the text data to store in the data object.</param>
	/// <param name="format">A member of <see cref="T:System.Windows.TextDataFormat" /> that specifies the text data format to store.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="textData" /> is null.</exception>
	public void SetText(string textData, TextDataFormat format)
	{
		if (textData == null)
		{
			throw new ArgumentNullException("textData");
		}
		if (!DataFormats.IsValidTextDataFormat(format))
		{
			throw new InvalidEnumArgumentException("format", (int)format, typeof(TextDataFormat));
		}
		SetData(DataFormats.ConvertToDataFormats(format), textData, autoConvert: false);
	}

	/// <summary>Creates a connection between a data object and an advisory sink. This method is called by an object that supports an advisory sink and enables the advisory sink to be notified of changes in the object's data.</summary>
	/// <returns>This method supports the standard return values E_INVALIDARG, E_UNEXPECTED, and E_OUTOFMEMORY, as well as the following: </returns>
	/// <param name="pFormatetc"> A <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structure, passed by reference, that defines the format, target device, aspect, and medium that will be used for future notifications.</param>
	/// <param name="advf">One of the <see cref="T:System.Runtime.InteropServices.ComTypes.ADVF" /> values that specifies a group of flags for controlling the advisory connection.</param>
	/// <param name="pAdvSink">A pointer to the <see cref="T:System.Runtime.InteropServices.ComTypes.IAdviseSink" /> interface on the advisory sink that will receive the change notification.</param>
	/// <param name="pdwConnection">When this method returns, contains a pointer to a DWORD token that identifies this connection. You can use this token later to delete the advisory connection by passing it to <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.DUnadvise(System.Int32)" />. If this value is zero, the connection was not established. This parameter is passed uninitialized.</param>
	int System.Runtime.InteropServices.ComTypes.IDataObject.DAdvise(ref FORMATETC pFormatetc, ADVF advf, IAdviseSink pAdvSink, out int pdwConnection)
	{
		if (_innerData is OleConverter)
		{
			return ((OleConverter)_innerData).OleDataObject.DAdvise(ref pFormatetc, advf, pAdvSink, out pdwConnection);
		}
		pdwConnection = 0;
		return -2147467263;
	}

	/// <summary>Destroys a notification connection that had been previously established.</summary>
	/// <param name="dwConnection">A DWORD token that specifies the connection to remove. Use the value returned by <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.DAdvise(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.ADVF,System.Runtime.InteropServices.ComTypes.IAdviseSink,System.Int32@)" /> when the connection was originally established.</param>
	void System.Runtime.InteropServices.ComTypes.IDataObject.DUnadvise(int dwConnection)
	{
		if (_innerData is OleConverter)
		{
			((OleConverter)_innerData).OleDataObject.DUnadvise(dwConnection);
		}
		else
		{
			Marshal.ThrowExceptionForHR(-2147467263);
		}
	}

	/// <summary>Creates an object that can be used to enumerate the current advisory connections.</summary>
	/// <returns>This method supports the standard return value E_OUTOFMEMORY, as well as the following:ValueDescriptionS_OKThe enumerator object is successfully instantiated or there are no connections.OLE_E_ADVISENOTSUPPORTEDThis object does not support advisory notifications.</returns>
	/// <param name="enumAdvise">When this method returns, contains an <see cref="T:System.Runtime.InteropServices.ComTypes.IEnumSTATDATA" /> that receives the interface pointer to the new enumerator object. If the implementation sets <paramref name="enumAdvise" /> to null, there are no connections to advisory sinks at this time. This parameter is passed uninitialized.</param>
	int System.Runtime.InteropServices.ComTypes.IDataObject.EnumDAdvise(out IEnumSTATDATA enumAdvise)
	{
		if (_innerData is OleConverter)
		{
			return ((OleConverter)_innerData).OleDataObject.EnumDAdvise(out enumAdvise);
		}
		enumAdvise = null;
		return -2147221501;
	}

	/// <summary>Creates an object for enumerating the <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structures for a data object. These structures are used in calls to <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetData(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)" /> or <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.SetData(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@,System.Boolean)" />. </summary>
	/// <returns>This method supports the standard return values E_INVALIDARG and E_OUTOFMEMORY, as well as the following:ValueDescriptionS_OKThe enumerator object was successfully created.E_NOTIMPLThe direction specified by the <paramref name="direction" /> parameter is not supported.OLE_S_USEREGRequests that OLE enumerate the formats from the registry.</returns>
	/// <param name="dwDirection">One of the <see cref="T:System.Runtime.InteropServices.ComTypes.DATADIR" /> values that specifies the direction of the data.</param>
	IEnumFORMATETC System.Runtime.InteropServices.ComTypes.IDataObject.EnumFormatEtc(DATADIR dwDirection)
	{
		if (_innerData is OleConverter)
		{
			return ((OleConverter)_innerData).OleDataObject.EnumFormatEtc(dwDirection);
		}
		if (dwDirection == DATADIR.DATADIR_GET)
		{
			return new FormatEnumerator(this);
		}
		throw new ExternalException(SR.Format(SR.DataObject_NotImplementedEnumFormatEtc, dwDirection), -2147467263);
	}

	/// <summary>Provides a standard <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structure that is logically equivalent to a more complex structure. Use this method to determine whether two different <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structures would return the same data, removing the need for duplicate rendering.</summary>
	/// <returns>This method supports the standard return values E_INVALIDARG, E_UNEXPECTED, and E_OUTOFMEMORY, as well as the following: ValueDescriptionS_OKThe returned <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structure is different from the one that was passed.DATA_S_SAMEFORMATETCThe <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structures are the same and null is returned in the <paramref name="formatOut" /> parameter.DV_E_LINDEXThere is an invalid value for <see cref="F:System.Runtime.InteropServices.ComTypes.FORMATETC.lindex" />; currently, only -1 is supported.DV_E_FORMATETCThere is an invalid value for the <paramref name="pFormatetc" /> parameter.OLE_E_NOTRUNNINGThe application is not running.</returns>
	/// <param name="pformatetcIn">A pointer to a <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structure, passed by reference, that defines the format, medium, and target device that the caller would like to use to retrieve data in a subsequent call such as <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetData(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)" />. The <see cref="T:System.Runtime.InteropServices.ComTypes.TYMED" /> member is not significant in this case and should be ignored.</param>
	/// <param name="pformatetcOut">When this method returns, contains a pointer to a <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structure that contains the most general information possible for a specific rendering, making it canonically equivalent to <paramref name="formatetIn" />. The caller must allocate this structure and the <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetCanonicalFormatEtc(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.FORMATETC@)" /> method must fill in the data. To retrieve data in a subsequent call such as <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetData(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)" />, the caller uses the supplied value of <paramref name="formatOut" />, unless the value supplied is null. This value is null if the method returns DATA_S_SAMEFORMATETC. The <see cref="T:System.Runtime.InteropServices.ComTypes.TYMED" /> member is not significant in this case and should be ignored. This parameter is passed uninitialized.</param>
	int System.Runtime.InteropServices.ComTypes.IDataObject.GetCanonicalFormatEtc(ref FORMATETC pformatetcIn, out FORMATETC pformatetcOut)
	{
		pformatetcOut = default(FORMATETC);
		pformatetcOut = pformatetcIn;
		pformatetcOut.ptd = IntPtr.Zero;
		if (pformatetcIn.lindex != -1)
		{
			return -2147221400;
		}
		if (_innerData is OleConverter)
		{
			return ((OleConverter)_innerData).OleDataObject.GetCanonicalFormatEtc(ref pformatetcIn, out pformatetcOut);
		}
		return 262448;
	}

	/// <summary>Obtains data from a source data object. The <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetData(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)" /> method, which is called by a data consumer, renders the data described in the specified <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structure and transfers it through the specified <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" /> structure. The caller then assumes responsibility for releasing the <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" /> structure.</summary>
	/// <param name="formatetc">A pointer to a <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structure, passed by reference, that defines the format, medium, and target device to use when passing the data. It is possible to specify more than one medium by using the Boolean OR operator, allowing the method to choose the best medium among those specified.</param>
	/// <param name="medium">When this method returns, contains a pointer to the <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" /> structure that indicates the storage medium containing the returned data through its <see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.tymed" /> member, and the responsibility for releasing the medium through the value of its <see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.pUnkForRelease" /> member. If <see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.pUnkForRelease" /> is null, the receiver of the medium is responsible for releasing it; otherwise, <see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.pUnkForRelease" /> points to the IUnknown interface on the appropriate object so its Release method can be called. The medium must be allocated and filled in by <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetData(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)" />. This parameter is passed uninitialized.</param>
	void System.Runtime.InteropServices.ComTypes.IDataObject.GetData(ref FORMATETC formatetc, out STGMEDIUM medium)
	{
		if (_innerData is OleConverter)
		{
			((OleConverter)_innerData).OleDataObject.GetData(ref formatetc, out medium);
			return;
		}
		int num = -2147221399;
		medium = default(STGMEDIUM);
		if (GetTymedUseable(formatetc.tymed))
		{
			if ((formatetc.tymed & TYMED.TYMED_HGLOBAL) != 0)
			{
				medium.tymed = TYMED.TYMED_HGLOBAL;
				medium.unionmember = Win32GlobalAlloc(8258, 1);
				num = OleGetDataUnrestricted(ref formatetc, ref medium, doNotReallocate: false);
				if (MS.Win32.NativeMethods.Failed(num))
				{
					Win32GlobalFree(new HandleRef(this, medium.unionmember));
				}
			}
			else if ((formatetc.tymed & TYMED.TYMED_ISTREAM) != 0)
			{
				medium.tymed = TYMED.TYMED_ISTREAM;
				IStream istream = null;
				num = Win32CreateStreamOnHGlobal(IntPtr.Zero, fDeleteOnRelease: true, ref istream);
				if (MS.Win32.NativeMethods.Succeeded(num))
				{
					medium.unionmember = Marshal.GetComInterfaceForObject(istream, typeof(IStream));
					Marshal.ReleaseComObject(istream);
					num = OleGetDataUnrestricted(ref formatetc, ref medium, doNotReallocate: false);
					if (MS.Win32.NativeMethods.Failed(num))
					{
						Marshal.Release(medium.unionmember);
					}
				}
			}
			else
			{
				medium.tymed = formatetc.tymed;
				num = OleGetDataUnrestricted(ref formatetc, ref medium, doNotReallocate: false);
			}
		}
		if (MS.Win32.NativeMethods.Failed(num))
		{
			medium.unionmember = IntPtr.Zero;
			Marshal.ThrowExceptionForHR(num);
		}
	}

	/// <summary>Obtains data from a source data object. This method, which is called by a data consumer, differs from the <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetData(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)" /> method in that the caller must allocate and free the specified storage medium.</summary>
	/// <param name="formatetc">A pointer to a <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structure, passed by reference, that defines the format, medium, and target device to use when passing the data. Only one medium can be specified in <see cref="T:System.Runtime.InteropServices.ComTypes.TYMED" />, and only the following <see cref="T:System.Runtime.InteropServices.ComTypes.TYMED" /> values are valid: <see cref="F:System.Runtime.InteropServices.ComTypes.TYMED.TYMED_ISTORAGE" />, <see cref="F:System.Runtime.InteropServices.ComTypes.TYMED.TYMED_ISTREAM" />, <see cref="F:System.Runtime.InteropServices.ComTypes.TYMED.TYMED_HGLOBAL" />, or <see cref="F:System.Runtime.InteropServices.ComTypes.TYMED.TYMED_FILE" />.</param>
	/// <param name="medium">A <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" />, passed by reference, that defines the storage medium containing the data being transferred. The medium must be allocated by the caller and filled in by <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetDataHere(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)" />. The caller must also free the medium. The implementation of this method must always supply a value of null for the <see cref="F:System.Runtime.InteropServices.ComTypes.STGMEDIUM.pUnkForRelease" /> member of the <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" /> structure that this parameter points to.</param>
	void System.Runtime.InteropServices.ComTypes.IDataObject.GetDataHere(ref FORMATETC formatetc, ref STGMEDIUM medium)
	{
		if (medium.tymed != TYMED.TYMED_ISTORAGE && medium.tymed != TYMED.TYMED_ISTREAM && medium.tymed != TYMED.TYMED_HGLOBAL && medium.tymed != TYMED.TYMED_FILE)
		{
			Marshal.ThrowExceptionForHR(-2147221399);
		}
		int num = OleGetDataUnrestricted(ref formatetc, ref medium, doNotReallocate: true);
		if (MS.Win32.NativeMethods.Failed(num))
		{
			Marshal.ThrowExceptionForHR(num);
		}
	}

	/// <summary>Determines whether the data object is capable of rendering the data described in the <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structure. Objects attempting a paste or drop operation can call this method before calling <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetData(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)" /> to get an indication of whether the operation may be successful.</summary>
	/// <returns>This method supports the standard return values E_INVALIDARG, E_UNEXPECTED, and E_OUTOFMEMORY, as well as the following: ValueDescriptionS_OKA subsequent call to <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.GetData(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@)" /> would probably be successful.DV_E_LINDEXAn invalid value for <see cref="F:System.Runtime.InteropServices.ComTypes.FORMATETC.lindex" />; currently, only -1 is supported.DV_E_FORMATETCAn invalid value for the <paramref name="pFormatetc" /> parameter.DV_E_TYMEDAn invalid <see cref="F:System.Runtime.InteropServices.ComTypes.FORMATETC.tymed" /> value.DV_E_DVASPECTAn invalid <see cref="F:System.Runtime.InteropServices.ComTypes.FORMATETC.dwAspect" /> value.OLE_E_NOTRUNNINGThe application is not running.</returns>
	/// <param name="formatetc">A pointer to a <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structure, passed by reference, that defines the format, medium, and target device to use for the query.</param>
	int System.Runtime.InteropServices.ComTypes.IDataObject.QueryGetData(ref FORMATETC formatetc)
	{
		if (_innerData is OleConverter)
		{
			return ((OleConverter)_innerData).OleDataObject.QueryGetData(ref formatetc);
		}
		if (formatetc.dwAspect == DVASPECT.DVASPECT_CONTENT)
		{
			if (GetTymedUseable(formatetc.tymed))
			{
				if (formatetc.cfFormat == 0)
				{
					return 1;
				}
				if (!GetDataPresent(DataFormats.GetDataFormat(formatetc.cfFormat).Name))
				{
					return -2147221404;
				}
				return 0;
			}
			return -2147221399;
		}
		return -2147221397;
	}

	/// <summary>Transfers data to the object that implements this method. This method is called by an object that contains a data source.</summary>
	/// <param name="pFormatetcIn">A <see cref="T:System.Runtime.InteropServices.ComTypes.FORMATETC" /> structure, passed by reference, that defines the format used by the data object when interpreting the data contained in the storage medium.</param>
	/// <param name="pmedium">A <see cref="T:System.Runtime.InteropServices.ComTypes.STGMEDIUM" /> structure, passed by reference, that defines the storage medium in which the data is being passed.</param>
	/// <param name="fRelease">true to specify that the data object called, which implements <see cref="M:System.Runtime.InteropServices.ComTypes.IDataObject.SetData(System.Runtime.InteropServices.ComTypes.FORMATETC@,System.Runtime.InteropServices.ComTypes.STGMEDIUM@,System.Boolean)" />, owns the storage medium after the call returns. This means that the data object must free the medium after it has been used by calling the ReleaseStgMedium function. false to specify that the caller retains ownership of the storage medium, and the data object called uses the storage medium for the duration of the call only.</param>
	void System.Runtime.InteropServices.ComTypes.IDataObject.SetData(ref FORMATETC pFormatetcIn, ref STGMEDIUM pmedium, bool fRelease)
	{
		if (_innerData is OleConverter)
		{
			((OleConverter)_innerData).OleDataObject.SetData(ref pFormatetcIn, ref pmedium, fRelease);
		}
		else
		{
			Marshal.ThrowExceptionForHR(-2147467263);
		}
	}

	/// <summary>Adds a <see cref="E:System.Windows.DataObject.Copying" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to add.</param>
	public static void AddCopyingHandler(DependencyObject element, DataObjectCopyingEventHandler handler)
	{
		UIElement.AddHandler(element, CopyingEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DataObject.Copying" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to remove.</param>
	public static void RemoveCopyingHandler(DependencyObject element, DataObjectCopyingEventHandler handler)
	{
		UIElement.RemoveHandler(element, CopyingEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DataObject.Pasting" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to add.</param>
	public static void AddPastingHandler(DependencyObject element, DataObjectPastingEventHandler handler)
	{
		UIElement.AddHandler(element, PastingEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DataObject.Pasting" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to remove.</param>
	public static void RemovePastingHandler(DependencyObject element, DataObjectPastingEventHandler handler)
	{
		UIElement.RemoveHandler(element, PastingEvent, handler);
	}

	/// <summary>Adds a <see cref="E:System.Windows.DataObject.SettingData" /> event handler to a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) to which to add the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to add.</param>
	public static void AddSettingDataHandler(DependencyObject element, DataObjectSettingDataEventHandler handler)
	{
		UIElement.AddHandler(element, SettingDataEvent, handler);
	}

	/// <summary>Removes a <see cref="E:System.Windows.DataObject.SettingData" /> event handler from a specified dependency object.</summary>
	/// <param name="element">The dependency object (a <see cref="T:System.Windows.UIElement" /> or <see cref="T:System.Windows.ContentElement" />) from which to remove the event handler.</param>
	/// <param name="handler">A delegate that references the handler method to remove.</param>
	public static void RemoveSettingDataHandler(DependencyObject element, DataObjectSettingDataEventHandler handler)
	{
		UIElement.RemoveHandler(element, SettingDataEvent, handler);
	}

	internal static nint Win32GlobalAlloc(int flags, nint bytes)
	{
		nint num = MS.Win32.UnsafeNativeMethods.GlobalAlloc(flags, bytes);
		int lastWin32Error = Marshal.GetLastWin32Error();
		if (num == IntPtr.Zero)
		{
			throw new Win32Exception(lastWin32Error);
		}
		return num;
	}

	private static int Win32CreateStreamOnHGlobal(nint hGlobal, bool fDeleteOnRelease, ref IStream istream)
	{
		int num = MS.Win32.UnsafeNativeMethods.CreateStreamOnHGlobal(hGlobal, fDeleteOnRelease, ref istream);
		if (MS.Win32.NativeMethods.Failed(num))
		{
			Marshal.ThrowExceptionForHR(num);
		}
		return num;
	}

	internal static void Win32GlobalFree(HandleRef handle)
	{
		nint num = MS.Win32.UnsafeNativeMethods.GlobalFree(handle);
		int lastWin32Error = Marshal.GetLastWin32Error();
		if (num != IntPtr.Zero)
		{
			throw new Win32Exception(lastWin32Error);
		}
	}

	internal static nint Win32GlobalReAlloc(HandleRef handle, nint bytes, int flags)
	{
		nint num = MS.Win32.UnsafeNativeMethods.GlobalReAlloc(handle, bytes, flags);
		int lastWin32Error = Marshal.GetLastWin32Error();
		if (num == IntPtr.Zero)
		{
			throw new Win32Exception(lastWin32Error);
		}
		return num;
	}

	internal static nint Win32GlobalLock(HandleRef handle)
	{
		nint num = MS.Win32.UnsafeNativeMethods.GlobalLock(handle);
		int lastWin32Error = Marshal.GetLastWin32Error();
		if (num == IntPtr.Zero)
		{
			throw new Win32Exception(lastWin32Error);
		}
		return num;
	}

	internal static void Win32GlobalUnlock(HandleRef handle)
	{
		bool num = MS.Win32.UnsafeNativeMethods.GlobalUnlock(handle);
		int lastWin32Error = Marshal.GetLastWin32Error();
		if (!num && lastWin32Error != 0)
		{
			throw new Win32Exception(lastWin32Error);
		}
	}

	internal static nint Win32GlobalSize(HandleRef handle)
	{
		nint num = MS.Win32.UnsafeNativeMethods.GlobalSize(handle);
		int lastWin32Error = Marshal.GetLastWin32Error();
		if (num == IntPtr.Zero)
		{
			throw new Win32Exception(lastWin32Error);
		}
		return num;
	}

	internal static nint Win32SelectObject(HandleRef handleDC, nint handleObject)
	{
		nint num = MS.Win32.UnsafeNativeMethods.SelectObject(handleDC, handleObject);
		if (num == IntPtr.Zero)
		{
			throw new Win32Exception();
		}
		return num;
	}

	internal static void Win32DeleteObject(HandleRef handleDC)
	{
		MS.Win32.UnsafeNativeMethods.DeleteObject(handleDC);
	}

	internal static nint Win32GetDC(HandleRef handleDC)
	{
		return MS.Win32.UnsafeNativeMethods.GetDC(handleDC);
	}

	internal static nint Win32CreateCompatibleDC(HandleRef handleDC)
	{
		return MS.Win32.UnsafeNativeMethods.CreateCompatibleDC(handleDC);
	}

	internal static nint Win32CreateCompatibleBitmap(HandleRef handleDC, int width, int height)
	{
		return MS.Win32.UnsafeNativeMethods.CreateCompatibleBitmap(handleDC, width, height);
	}

	internal static void Win32DeleteDC(HandleRef handleDC)
	{
		MS.Win32.UnsafeNativeMethods.DeleteDC(handleDC);
	}

	private static void Win32ReleaseDC(HandleRef handleHWND, HandleRef handleDC)
	{
		MS.Win32.UnsafeNativeMethods.ReleaseDC(handleHWND, handleDC);
	}

	internal static void Win32BitBlt(HandleRef handledestination, int width, int height, HandleRef handleSource, int operationCode)
	{
		if (!MS.Win32.UnsafeNativeMethods.BitBlt(handledestination, 0, 0, width, height, handleSource, 0, 0, operationCode))
		{
			throw new Win32Exception();
		}
	}

	internal static int Win32WideCharToMultiByte(string wideString, int wideChars, byte[] bytes, int byteCount)
	{
		int num = MS.Win32.UnsafeNativeMethods.WideCharToMultiByte(0, 0, wideString, wideChars, bytes, byteCount, IntPtr.Zero, IntPtr.Zero);
		int lastWin32Error = Marshal.GetLastWin32Error();
		if (num == 0)
		{
			throw new Win32Exception(lastWin32Error);
		}
		return num;
	}

	internal static string[] GetMappedFormats(string format)
	{
		if (format == null)
		{
			return null;
		}
		if (!IsFormatEqual(format, DataFormats.Text) && !IsFormatEqual(format, DataFormats.UnicodeText) && !IsFormatEqual(format, DataFormats.StringFormat))
		{
			if (!IsFormatEqual(format, DataFormats.FileDrop) && !IsFormatEqual(format, "FileName") && !IsFormatEqual(format, "FileNameW"))
			{
				if (!IsFormatEqual(format, DataFormats.Bitmap) && !IsFormatEqual(format, "System.Windows.Media.Imaging.BitmapSource") && !IsFormatEqual(format, "System.Drawing.Bitmap"))
				{
					if (!IsFormatEqual(format, DataFormats.EnhancedMetafile) && !IsFormatEqual(format, "System.Drawing.Imaging.Metafile"))
					{
						return new string[1] { format };
					}
					return new string[2]
					{
						DataFormats.EnhancedMetafile,
						"System.Drawing.Imaging.Metafile"
					};
				}
				return new string[3]
				{
					DataFormats.Bitmap,
					"System.Drawing.Bitmap",
					"System.Windows.Media.Imaging.BitmapSource"
				};
			}
			return new string[3]
			{
				DataFormats.FileDrop,
				"FileNameW",
				"FileName"
			};
		}
		return new string[3]
		{
			DataFormats.Text,
			DataFormats.UnicodeText,
			DataFormats.StringFormat
		};
	}

	private int OleGetDataUnrestricted(ref FORMATETC formatetc, ref STGMEDIUM medium, bool doNotReallocate)
	{
		if (_innerData is OleConverter)
		{
			((OleConverter)_innerData).OleDataObject.GetDataHere(ref formatetc, ref medium);
			return 0;
		}
		return GetDataIntoOleStructs(ref formatetc, ref medium, doNotReallocate);
	}

	private static string[] GetDistinctStrings(string[] formats)
	{
		ArrayList arrayList = new ArrayList();
		foreach (string text in formats)
		{
			if (!arrayList.Contains(text))
			{
				arrayList.Add(text);
			}
		}
		string[] array = new string[arrayList.Count];
		arrayList.CopyTo(array, 0);
		return array;
	}

	private bool GetTymedUseable(TYMED tymed)
	{
		for (int i = 0; i < ALLOWED_TYMEDS.Length; i++)
		{
			if ((tymed & ALLOWED_TYMEDS[i]) != 0)
			{
				return true;
			}
		}
		return false;
	}

	private nint GetCompatibleBitmap(object data)
	{
		int width;
		int height;
		nint hBitmap = SystemDrawingHelper.GetHBitmap(data, out width, out height);
		if (hBitmap == IntPtr.Zero)
		{
			return IntPtr.Zero;
		}
		try
		{
			nint handle = Win32GetDC(new HandleRef(this, IntPtr.Zero));
			nint handle2 = Win32CreateCompatibleDC(new HandleRef(this, handle));
			nint handleObject = Win32SelectObject(new HandleRef(this, handle2), hBitmap);
			nint handle3 = Win32CreateCompatibleDC(new HandleRef(this, handle));
			nint num = Win32CreateCompatibleBitmap(new HandleRef(this, handle), width, height);
			nint handleObject2 = Win32SelectObject(new HandleRef(this, handle3), num);
			try
			{
				Win32BitBlt(new HandleRef(this, handle3), width, height, new HandleRef(null, handle2), 13369376);
				return num;
			}
			finally
			{
				Win32SelectObject(new HandleRef(this, handle2), handleObject);
				Win32SelectObject(new HandleRef(this, handle3), handleObject2);
				Win32DeleteDC(new HandleRef(this, handle2));
				Win32DeleteDC(new HandleRef(this, handle3));
				Win32ReleaseDC(new HandleRef(this, IntPtr.Zero), new HandleRef(this, handle));
			}
		}
		finally
		{
			Win32DeleteObject(new HandleRef(this, hBitmap));
		}
	}

	private nint GetEnhancedMetafileHandle(string format, object data)
	{
		nint num = IntPtr.Zero;
		if (IsFormatEqual(format, DataFormats.EnhancedMetafile))
		{
			if (SystemDrawingHelper.IsMetafile(data))
			{
				num = SystemDrawingHelper.GetHandleFromMetafile(data);
			}
			else if (data is MemoryStream && data is MemoryStream memoryStream)
			{
				byte[] buffer = memoryStream.GetBuffer();
				if (buffer != null && buffer.Length != 0)
				{
					num = MS.Win32.NativeMethods.SetEnhMetaFileBits((uint)buffer.Length, buffer);
					int lastWin32Error = Marshal.GetLastWin32Error();
					if (num == IntPtr.Zero)
					{
						throw new Win32Exception(lastWin32Error);
					}
				}
			}
		}
		return num;
	}

	private int GetDataIntoOleStructs(ref FORMATETC formatetc, ref STGMEDIUM medium, bool doNotReallocate)
	{
		int result = -2147221399;
		if (GetTymedUseable(formatetc.tymed) && GetTymedUseable(medium.tymed))
		{
			string name = DataFormats.GetDataFormat(formatetc.cfFormat).Name;
			result = -2147221404;
			if (GetDataPresent(name))
			{
				object data = GetData(name);
				result = -2147221399;
				if ((formatetc.tymed & TYMED.TYMED_HGLOBAL) != 0)
				{
					result = GetDataIntoOleStructsByTypeMedimHGlobal(name, data, ref medium, doNotReallocate);
				}
				else if ((formatetc.tymed & TYMED.TYMED_GDI) != 0)
				{
					result = GetDataIntoOleStructsByTypeMediumGDI(name, data, ref medium);
				}
				else if ((formatetc.tymed & TYMED.TYMED_ENHMF) != 0)
				{
					result = GetDataIntoOleStructsByTypeMediumEnhancedMetaFile(name, data, ref medium);
				}
				else if ((formatetc.tymed & TYMED.TYMED_ISTREAM) != 0)
				{
					result = GetDataIntoOleStructsByTypeMedimIStream(name, data, ref medium);
				}
			}
		}
		return result;
	}

	private int GetDataIntoOleStructsByTypeMedimHGlobal(string format, object data, ref STGMEDIUM medium, bool doNotReallocate)
	{
		int num = -2147467259;
		if (data is Stream)
		{
			num = SaveStreamToHandle(medium.unionmember, (Stream)data, doNotReallocate);
		}
		else if (IsFormatEqual(format, DataFormats.Html) || IsFormatEqual(format, DataFormats.Xaml))
		{
			num = SaveStringToHandleAsUtf8(medium.unionmember, data.ToString(), doNotReallocate);
		}
		else if (IsFormatEqual(format, DataFormats.Text) || IsFormatEqual(format, DataFormats.Rtf) || IsFormatEqual(format, DataFormats.OemText) || IsFormatEqual(format, DataFormats.CommaSeparatedValue))
		{
			num = SaveStringToHandle(medium.unionmember, data.ToString(), unicode: false, doNotReallocate);
		}
		else if (IsFormatEqual(format, DataFormats.UnicodeText) || IsFormatEqual(format, "ApplicationTrust"))
		{
			num = SaveStringToHandle(medium.unionmember, data.ToString(), unicode: true, doNotReallocate);
		}
		else if (IsFormatEqual(format, DataFormats.FileDrop))
		{
			num = SaveFileListToHandle(medium.unionmember, (string[])data, doNotReallocate);
		}
		else if (IsFormatEqual(format, "FileName"))
		{
			string[] array = (string[])data;
			num = SaveStringToHandle(medium.unionmember, array[0], unicode: false, doNotReallocate);
		}
		else if (!IsFormatEqual(format, "FileNameW"))
		{
			num = ((IsFormatEqual(format, DataFormats.Dib) && SystemDrawingHelper.IsImage(data)) ? (-2147221399) : (IsFormatEqual(format, typeof(BitmapSource).FullName) ? SaveSystemBitmapSourceToHandle(medium.unionmember, data, doNotReallocate) : (IsFormatEqual(format, "System.Drawing.Bitmap") ? SaveSystemDrawingBitmapToHandle(medium.unionmember, data, doNotReallocate) : ((!IsFormatEqual(format, DataFormats.EnhancedMetafile) && !SystemDrawingHelper.IsMetafile(data)) ? ((!IsFormatEqual(format, DataFormats.Serializable) && !(data is ISerializable) && (data == null || !data.GetType().IsSerializable)) ? (-2147221399) : SaveObjectToHandle(medium.unionmember, data, doNotReallocate)) : (-2147221399)))));
		}
		else
		{
			string[] array2 = (string[])data;
			num = SaveStringToHandle(medium.unionmember, array2[0], unicode: true, doNotReallocate);
		}
		if (num == 0)
		{
			medium.tymed = TYMED.TYMED_HGLOBAL;
		}
		return num;
	}

	private int GetDataIntoOleStructsByTypeMedimIStream(string format, object data, ref STGMEDIUM medium)
	{
		IStream stream = (IStream)Marshal.GetObjectForIUnknown(medium.unionmember);
		if (stream == null)
		{
			return -2147024809;
		}
		int num = -2147467259;
		try
		{
			if (format == StrokeCollection.InkSerializedFormat && data is Stream stream2)
			{
				nint intPtr = (nint)stream2.Length;
				byte[] array = new byte[MS.Win32.NativeMethods.IntPtrToInt32(intPtr)];
				stream2.Position = 0L;
				stream2.Read(array, 0, MS.Win32.NativeMethods.IntPtrToInt32(intPtr));
				stream.Write(array, MS.Win32.NativeMethods.IntPtrToInt32(intPtr), IntPtr.Zero);
				num = 0;
			}
		}
		finally
		{
			Marshal.ReleaseComObject(stream);
		}
		if (MS.Win32.NativeMethods.Succeeded(num))
		{
			medium.tymed = TYMED.TYMED_ISTREAM;
		}
		return num;
	}

	private int GetDataIntoOleStructsByTypeMediumGDI(string format, object data, ref STGMEDIUM medium)
	{
		int result = -2147467259;
		if (IsFormatEqual(format, DataFormats.Bitmap) && (SystemDrawingHelper.IsBitmap(data) || IsDataSystemBitmapSource(data)))
		{
			nint compatibleBitmap = GetCompatibleBitmap(data);
			if (compatibleBitmap != IntPtr.Zero)
			{
				medium.tymed = TYMED.TYMED_GDI;
				medium.unionmember = compatibleBitmap;
				result = 0;
			}
		}
		else
		{
			result = -2147221399;
		}
		return result;
	}

	private int GetDataIntoOleStructsByTypeMediumEnhancedMetaFile(string format, object data, ref STGMEDIUM medium)
	{
		int result = -2147467259;
		if (IsFormatEqual(format, DataFormats.EnhancedMetafile))
		{
			nint enhancedMetafileHandle = GetEnhancedMetafileHandle(format, data);
			if (enhancedMetafileHandle != IntPtr.Zero)
			{
				medium.tymed = TYMED.TYMED_ENHMF;
				medium.unionmember = enhancedMetafileHandle;
				result = 0;
			}
		}
		else
		{
			result = -2147221399;
		}
		return result;
	}

	private int SaveObjectToHandle(nint handle, object data, bool doNotReallocate)
	{
		Stream stream;
		using (stream = new MemoryStream())
		{
			BinaryWriter binaryWriter;
			using (binaryWriter = new BinaryWriter(stream))
			{
				binaryWriter.Write(_serializedObjectID);
				new BinaryFormatter().Serialize(stream, data);
				return SaveStreamToHandle(handle, stream, doNotReallocate);
			}
		}
	}

	private int SaveStreamToHandle(nint handle, Stream stream, bool doNotReallocate)
	{
		if (handle == IntPtr.Zero)
		{
			return -2147024809;
		}
		nint intPtr = (nint)stream.Length;
		int num = EnsureMemoryCapacity(ref handle, MS.Win32.NativeMethods.IntPtrToInt32(intPtr), doNotReallocate);
		if (MS.Win32.NativeMethods.Failed(num))
		{
			return num;
		}
		nint destination = Win32GlobalLock(new HandleRef(this, handle));
		try
		{
			byte[] array = new byte[MS.Win32.NativeMethods.IntPtrToInt32(intPtr)];
			stream.Position = 0L;
			stream.Read(array, 0, MS.Win32.NativeMethods.IntPtrToInt32(intPtr));
			Marshal.Copy(array, 0, destination, MS.Win32.NativeMethods.IntPtrToInt32(intPtr));
		}
		finally
		{
			Win32GlobalUnlock(new HandleRef(this, handle));
		}
		return 0;
	}

	private int SaveSystemBitmapSourceToHandle(nint handle, object data, bool doNotReallocate)
	{
		BitmapSource bitmapSource = null;
		if (IsDataSystemBitmapSource(data))
		{
			bitmapSource = (BitmapSource)data;
		}
		else if (SystemDrawingHelper.IsBitmap(data))
		{
			nint hBitmapFromBitmap = SystemDrawingHelper.GetHBitmapFromBitmap(data);
			bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmapFromBitmap, IntPtr.Zero, Int32Rect.Empty, null);
			Win32DeleteObject(new HandleRef(this, hBitmapFromBitmap));
		}
		Invariant.Assert(bitmapSource != null);
		BmpBitmapEncoder bmpBitmapEncoder = new BmpBitmapEncoder();
		bmpBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
		Stream stream = new MemoryStream();
		bmpBitmapEncoder.Save(stream);
		return SaveStreamToHandle(handle, stream, doNotReallocate);
	}

	private int SaveSystemDrawingBitmapToHandle(nint handle, object data, bool doNotReallocate)
	{
		object bitmap = SystemDrawingHelper.GetBitmap(data);
		Invariant.Assert(bitmap != null);
		return SaveObjectToHandle(handle, bitmap, doNotReallocate);
	}

	private unsafe int SaveFileListToHandle(nint handle, string[] files, bool doNotReallocate)
	{
		if (files == null || files.Length < 1)
		{
			return 0;
		}
		if (handle == IntPtr.Zero)
		{
			return -2147024809;
		}
		if (Marshal.SystemDefaultCharSize == 1)
		{
			Invariant.Assert(condition: false, "Expected the system default char size to be 2 for Unicode systems.");
			return -2147024809;
		}
		nint zero = IntPtr.Zero;
		int num = 20;
		int num2 = num;
		for (int i = 0; i < files.Length; i++)
		{
			num2 += (files[i].Length + 1) * 2;
		}
		num2 += 2;
		int num3 = EnsureMemoryCapacity(ref handle, num2, doNotReallocate);
		if (MS.Win32.NativeMethods.Failed(num3))
		{
			return num3;
		}
		nint num4 = Win32GlobalLock(new HandleRef(this, handle));
		try
		{
			zero = num4;
			int[] array = new int[5] { num, 0, 0, 0, 0 };
			array[4] = -1;
			Marshal.Copy(array, 0, zero, array.Length);
			zero = (nint)((long)zero + (long)num);
			for (int j = 0; j < files.Length; j++)
			{
				MS.Win32.UnsafeNativeMethods.CopyMemoryW(zero, files[j], files[j].Length * 2);
				zero = (nint)((long)zero + (long)(files[j].Length * 2));
				*(short*)zero = 0;
				zero = (nint)((long)zero + 2L);
			}
			*(short*)zero = 0;
		}
		finally
		{
			Win32GlobalUnlock(new HandleRef(this, handle));
		}
		return 0;
	}

	private unsafe int SaveStringToHandle(nint handle, string str, bool unicode, bool doNotReallocate)
	{
		if (handle == IntPtr.Zero)
		{
			return -2147024809;
		}
		if (unicode)
		{
			int minimumByteCount = str.Length * 2 + 2;
			int num = EnsureMemoryCapacity(ref handle, minimumByteCount, doNotReallocate);
			if (MS.Win32.NativeMethods.Failed(num))
			{
				return num;
			}
			nint num2 = Win32GlobalLock(new HandleRef(this, handle));
			try
			{
				char[] array = str.ToCharArray(0, str.Length);
				MS.Win32.UnsafeNativeMethods.CopyMemoryW(num2, array, array.Length * 2);
				*(short*)(num2 + (long)array.Length * 2L) = 0;
			}
			finally
			{
				Win32GlobalUnlock(new HandleRef(this, handle));
			}
		}
		else
		{
			int num3 = ((str.Length > 0) ? Win32WideCharToMultiByte(str, str.Length, null, 0) : 0);
			byte[] array2 = new byte[num3];
			if (num3 > 0)
			{
				Win32WideCharToMultiByte(str, str.Length, array2, array2.Length);
			}
			int num4 = EnsureMemoryCapacity(ref handle, num3 + 1, doNotReallocate);
			if (MS.Win32.NativeMethods.Failed(num4))
			{
				return num4;
			}
			nint num5 = Win32GlobalLock(new HandleRef(this, handle));
			try
			{
				MS.Win32.UnsafeNativeMethods.CopyMemory(num5, array2, num3);
				Marshal.Copy(new byte[1], 0, (nint)((long)num5 + (long)num3), 1);
			}
			finally
			{
				Win32GlobalUnlock(new HandleRef(this, handle));
			}
		}
		return 0;
	}

	private int SaveStringToHandleAsUtf8(nint handle, string str, bool doNotReallocate)
	{
		if (handle == IntPtr.Zero)
		{
			return -2147024809;
		}
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		int byteCount = uTF8Encoding.GetByteCount(str);
		byte[] bytes = uTF8Encoding.GetBytes(str);
		int num = EnsureMemoryCapacity(ref handle, byteCount + 1, doNotReallocate);
		if (MS.Win32.NativeMethods.Failed(num))
		{
			return num;
		}
		nint num2 = Win32GlobalLock(new HandleRef(this, handle));
		try
		{
			MS.Win32.UnsafeNativeMethods.CopyMemory(num2, bytes, byteCount);
			Marshal.Copy(new byte[1], 0, (nint)((long)num2 + (long)byteCount), 1);
		}
		finally
		{
			Win32GlobalUnlock(new HandleRef(this, handle));
		}
		return 0;
	}

	private static bool IsDataSystemBitmapSource(object data)
	{
		if (data is BitmapSource)
		{
			return true;
		}
		return false;
	}

	private static bool IsFormatAndDataSerializable(string format, object data)
	{
		if (!IsFormatEqual(format, DataFormats.Serializable) && !(data is ISerializable))
		{
			return data?.GetType().IsSerializable ?? false;
		}
		return true;
	}

	private static bool IsFormatEqual(string format1, string format2)
	{
		return string.Equals(format1, format2, StringComparison.Ordinal);
	}

	private int EnsureMemoryCapacity(ref nint handle, int minimumByteCount, bool doNotReallocate)
	{
		int result = 0;
		if (doNotReallocate)
		{
			if (MS.Win32.NativeMethods.IntPtrToInt32(Win32GlobalSize(new HandleRef(this, handle))) < minimumByteCount)
			{
				handle = IntPtr.Zero;
				result = -2147286928;
			}
		}
		else
		{
			handle = Win32GlobalReAlloc(new HandleRef(this, handle), minimumByteCount, 8258);
			if (handle == IntPtr.Zero)
			{
				result = -2147024882;
			}
		}
		return result;
	}

	private static object EnsureBitmapDataFromFormat(string format, bool autoConvert, object data)
	{
		object result = data;
		if (IsDataSystemBitmapSource(data) && IsFormatEqual(format, "System.Drawing.Bitmap"))
		{
			result = ((!autoConvert) ? null : SystemDrawingHelper.GetBitmap(data));
		}
		else if (SystemDrawingHelper.IsBitmap(data) && (IsFormatEqual(format, DataFormats.Bitmap) || IsFormatEqual(format, "System.Windows.Media.Imaging.BitmapSource")))
		{
			if (autoConvert)
			{
				nint hBitmapFromBitmap = SystemDrawingHelper.GetHBitmapFromBitmap(data);
				result = Imaging.CreateBitmapSourceFromHBitmap(hBitmapFromBitmap, IntPtr.Zero, Int32Rect.Empty, null);
				Win32DeleteObject(new HandleRef(null, hBitmapFromBitmap));
			}
			else
			{
				result = null;
			}
		}
		return result;
	}
}
