using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Ink;
using MS.Internal.Ink.InkSerializedFormat;
using MS.Internal.PresentationCore;

namespace System.Windows.Ink;

/// <summary>Represents a collection of <see cref="T:System.Windows.Ink.Stroke" /> objects.</summary>
[TypeConverter(typeof(StrokeCollectionConverter))]
public class StrokeCollection : Collection<Stroke>, INotifyPropertyChanged, INotifyCollectionChanged
{
	internal class ReadOnlyStrokeCollection : StrokeCollection, ICollection<Stroke>, IEnumerable<Stroke>, IEnumerable, IList, ICollection
	{
		bool IList.IsReadOnly => true;

		bool ICollection<Stroke>.IsReadOnly => true;

		internal ReadOnlyStrokeCollection(StrokeCollection strokeCollection)
		{
			if (strokeCollection != null)
			{
				((List<Stroke>)base.Items).AddRange(strokeCollection);
			}
		}

		protected override void OnStrokesChanged(StrokeCollectionChangedEventArgs e)
		{
			throw new NotSupportedException(SR.StrokeCollectionIsReadOnly);
		}
	}

	/// <summary>Represents the native persistence format for ink data.</summary>
	public static readonly string InkSerializedFormat = "Ink Serialized Format";

	private ExtendedPropertyCollection _extendedProperties;

	private PropertyChangedEventHandler _propertyChanged;

	private NotifyCollectionChangedEventHandler _collectionChanged;

	private const string IndexerName = "Item[]";

	private const string CountName = "Count";

	internal ExtendedPropertyCollection ExtendedProperties
	{
		get
		{
			if (_extendedProperties == null)
			{
				_extendedProperties = new ExtendedPropertyCollection();
			}
			return _extendedProperties;
		}
		private set
		{
			if (value != null)
			{
				_extendedProperties = value;
			}
		}
	}

	/// <summary>Occurs when a <see cref="T:System.Windows.Ink.Stroke" /> in the collection changes. </summary>
	public event StrokeCollectionChangedEventHandler StrokesChanged;

	internal event StrokeCollectionChangedEventHandler StrokesChangedInternal;

	/// <summary>Occurs when custom property is added or removed from the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	public event PropertyDataChangedEventHandler PropertyDataChanged;

	/// <summary>Occurs when the value of any <see cref="T:System.Windows.Ink.StrokeCollection" /> property has changed.</summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			_propertyChanged = (PropertyChangedEventHandler)Delegate.Combine(_propertyChanged, value);
		}
		remove
		{
			_propertyChanged = (PropertyChangedEventHandler)Delegate.Remove(_propertyChanged, value);
		}
	}

	/// <summary>Occurs when the <see cref="T:System.Windows.Ink.StrokeCollection" /> changes.</summary>
	event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
	{
		add
		{
			_collectionChanged = (NotifyCollectionChangedEventHandler)Delegate.Combine(_collectionChanged, value);
		}
		remove
		{
			_collectionChanged = (NotifyCollectionChangedEventHandler)Delegate.Remove(_collectionChanged, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.StrokeCollection" /> class. </summary>
	public StrokeCollection()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Ink.StrokeCollection" /> class that contains the specified strokes. </summary>
	/// <param name="strokes">The strokes to add to the <see cref="T:System.Windows.Ink.StrokeCollection" />.</param>
	public StrokeCollection(IEnumerable<Stroke> strokes)
	{
		if (strokes == null)
		{
			throw new ArgumentNullException("strokes");
		}
		List<Stroke> list = (List<Stroke>)base.Items;
		foreach (Stroke stroke in strokes)
		{
			if (list.Contains(stroke))
			{
				list.Clear();
				throw new ArgumentException(SR.StrokeIsDuplicated, "strokes");
			}
			list.Add(stroke);
		}
	}

	/// <summary>Initializes a <see cref="T:System.Windows.Ink.StrokeCollection" /> from the specified <see cref="T:System.IO.Stream" /> of Ink Serialized Format (ISF).</summary>
	/// <param name="stream">A stream that contains ink data.</param>
	public StrokeCollection(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanRead)
		{
			throw new ArgumentException(SR.Image_StreamRead, "stream");
		}
		Stream seekableStream = GetSeekableStream(stream);
		if (seekableStream == null)
		{
			throw new ArgumentException(SR.Invalid_isfData_Length, "stream");
		}
		new StrokeCollectionSerializer(this).DecodeISF(seekableStream);
	}

	/// <summary>Saves the <see cref="T:System.Windows.Ink.StrokeCollection" /> to the specified <see cref="T:System.IO.Stream" /> and compresses it, when specified.</summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> to which to save the <see cref="T:System.Windows.Ink.StrokeCollection" />.</param>
	/// <param name="compress">true to compress the <see cref="T:System.Windows.Ink.StrokeCollection" />; otherwise, false.</param>
	public virtual void Save(Stream stream, bool compress)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanWrite)
		{
			throw new ArgumentException(SR.Image_StreamWrite, "stream");
		}
		SaveIsf(stream, compress);
	}

	/// <summary>Saves the <see cref="T:System.Windows.Ink.StrokeCollection" /> to the specified <see cref="T:System.IO.Stream" />.</summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> to which to save the <see cref="T:System.Windows.Ink.StrokeCollection" />.</param>
	public void Save(Stream stream)
	{
		Save(stream, compress: true);
	}

	internal void SaveIsf(Stream stream, bool compress)
	{
		StrokeCollectionSerializer strokeCollectionSerializer = new StrokeCollectionSerializer(this);
		strokeCollectionSerializer.CurrentCompressionMode = ((!compress) ? CompressionMode.NoCompression : CompressionMode.Compressed);
		strokeCollectionSerializer.EncodeISF(stream);
	}

	private Stream GetSeekableStream(Stream stream)
	{
		if (stream.CanSeek)
		{
			if ((int)(stream.Length - stream.Position) < 1)
			{
				return null;
			}
			return stream;
		}
		MemoryStream memoryStream = new MemoryStream();
		int num = 4096;
		byte[] buffer = new byte[num];
		int num2 = num;
		while (num2 == num)
		{
			num2 = stream.Read(buffer, 0, 4096);
			memoryStream.Write(buffer, 0, num2);
		}
		memoryStream.Position = 0L;
		return memoryStream;
	}

	/// <summary>Adds a custom property to the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <param name="propertyDataId">The <see cref="T:System.Guid" /> to associate with the custom property.</param>
	/// <param name="propertyData">The value of the custom property. <paramref name="propertyData" /> must be of type <see cref="T:System.Char" />, <see cref="T:System.Byte" />,<see cref="T:System.Int16" />,,<see cref="T:System.UInt16" />, <see cref="T:System.Int32" />, <see cref="T:System.UInt32" />, <see cref="T:System.Int64" />, <see cref="T:System.UInt64" />, <see cref="T:System.Single" />, <see cref="T:System.Double" />, <see cref="T:System.DateTime" />, <see cref="T:System.Boolean" />, <see cref="T:System.String" />, <see cref="T:System.Decimal" /> or an array of these data types, except <see cref="T:System.String" />, which is not allowed.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="propertyDataId" /> is an empty <see cref="T:System.Guid" />.-or-<paramref name="propertyData" /> is not one of the allowed data types listed in the Parameters section.</exception>
	public void AddPropertyData(Guid propertyDataId, object propertyData)
	{
		DrawingAttributes.ValidateStylusTipTransform(propertyDataId, propertyData);
		object previousValue = null;
		if (ContainsPropertyData(propertyDataId))
		{
			previousValue = GetPropertyData(propertyDataId);
			ExtendedProperties[propertyDataId] = propertyData;
		}
		else
		{
			ExtendedProperties.Add(propertyDataId, propertyData);
		}
		OnPropertyDataChanged(new PropertyDataChangedEventArgs(propertyDataId, propertyData, previousValue));
	}

	/// <summary>Removes the custom property associated with the specified <see cref="T:System.Guid" />.</summary>
	/// <param name="propertyDataId">The <see cref="T:System.Guid" /> associated with the custom property to remove.</param>
	public void RemovePropertyData(Guid propertyDataId)
	{
		object propertyData = GetPropertyData(propertyDataId);
		ExtendedProperties.Remove(propertyDataId);
		OnPropertyDataChanged(new PropertyDataChangedEventArgs(propertyDataId, null, propertyData));
	}

	/// <summary>Returns the value of the custom property associated with the specified <see cref="T:System.Guid" />.</summary>
	/// <returns>The value of the custom property associated with the specified <see cref="T:System.Guid" />.</returns>
	/// <param name="propertyDataId">The <see cref="T:System.Guid" /> associated with the custom property to get.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="propertyDataId" /> is not associated with a custom property of the <see cref="T:System.Windows.Ink.StrokeCollection" />.</exception>
	public object GetPropertyData(Guid propertyDataId)
	{
		if (propertyDataId == Guid.Empty)
		{
			throw new ArgumentException(SR.InvalidGuid, "propertyDataId");
		}
		return ExtendedProperties[propertyDataId];
	}

	/// <summary>Returns the GUIDs of any custom properties associated with the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <returns>An array of type <see cref="T:System.Guid" /> that represent the custom property identifiers.</returns>
	public Guid[] GetPropertyDataIds()
	{
		return ExtendedProperties.GetGuidArray();
	}

	/// <summary>Returns whether the specified custom property identifier is in the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <returns>true if the specified custom property identifier is in the <see cref="T:System.Windows.Ink.StrokeCollection" />; otherwise, false.</returns>
	/// <param name="propertyDataId">The <see cref="T:System.Guid" /> to locate in the <see cref="T:System.Windows.Ink.StrokeCollection" />.</param>
	public bool ContainsPropertyData(Guid propertyDataId)
	{
		return ExtendedProperties.Contains(propertyDataId);
	}

	/// <summary>Modifies each of the <see cref="P:System.Windows.Ink.Stroke.StylusPoints" /> and optionally the <see cref="P:System.Windows.Ink.DrawingAttributes.StylusTipTransform" /> for each stroke in the <see cref="T:System.Windows.Ink.StrokeCollection" /> according to the specified <see cref="T:System.Windows.Media.Matrix" />.</summary>
	/// <param name="transformMatrix">A <see cref="T:System.Windows.Media.Matrix" /> which specifies the transformation to perform on the <see cref="T:System.Windows.Ink.StrokeCollection" />.</param>
	/// <param name="applyToStylusTip">true to apply the transformation to the tip of the stylus; otherwise, false.</param>
	public void Transform(Matrix transformMatrix, bool applyToStylusTip)
	{
		if (!transformMatrix.HasInverse)
		{
			throw new ArgumentException(SR.MatrixNotInvertible, "transformMatrix");
		}
		if (transformMatrix.IsIdentity || base.Count == 0)
		{
			return;
		}
		using IEnumerator<Stroke> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Transform(transformMatrix, applyToStylusTip);
		}
	}

	/// <summary>Copies the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <returns>A copy of the <see cref="T:System.Windows.Ink.StrokeCollection" />.</returns>
	public virtual StrokeCollection Clone()
	{
		StrokeCollection strokeCollection = new StrokeCollection();
		using (IEnumerator<Stroke> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Stroke current = enumerator.Current;
				strokeCollection.Add(current.Clone());
			}
		}
		if (_extendedProperties != null)
		{
			strokeCollection._extendedProperties = _extendedProperties.Clone();
		}
		return strokeCollection;
	}

	/// <summary>Clears all strokes from the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	protected sealed override void ClearItems()
	{
		if (base.Count > 0)
		{
			StrokeCollection strokeCollection = new StrokeCollection();
			for (int i = 0; i < base.Count; i++)
			{
				((List<Stroke>)strokeCollection.Items).Add(base[i]);
			}
			base.ClearItems();
			RaiseStrokesChanged(null, strokeCollection, -1);
		}
	}

	/// <summary>Removes the stroke at the specified index from the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <param name="index">The specified index.</param>
	protected sealed override void RemoveItem(int index)
	{
		Stroke item = base[index];
		base.RemoveItem(index);
		StrokeCollection strokeCollection = new StrokeCollection();
		((List<Stroke>)strokeCollection.Items).Add(item);
		RaiseStrokesChanged(null, strokeCollection, index);
	}

	/// <summary>Inserts a stroke into the <see cref="T:System.Windows.Ink.StrokeCollection" /> at the specified index.</summary>
	protected sealed override void InsertItem(int index, Stroke stroke)
	{
		if (stroke == null)
		{
			throw new ArgumentNullException("stroke");
		}
		if (IndexOf(stroke) != -1)
		{
			throw new ArgumentException(SR.StrokeIsDuplicated, "stroke");
		}
		base.InsertItem(index, stroke);
		StrokeCollection strokeCollection = new StrokeCollection();
		((List<Stroke>)strokeCollection.Items).Add(stroke);
		RaiseStrokesChanged(strokeCollection, null, index);
	}

	/// <summary>Replaces the stroke at the specified index.</summary>
	protected sealed override void SetItem(int index, Stroke stroke)
	{
		if (stroke == null)
		{
			throw new ArgumentNullException("stroke");
		}
		if (IndexOf(stroke) != -1)
		{
			throw new ArgumentException(SR.StrokeIsDuplicated, "stroke");
		}
		Stroke item = base[index];
		base.SetItem(index, stroke);
		StrokeCollection strokeCollection = new StrokeCollection();
		((List<Stroke>)strokeCollection.Items).Add(item);
		StrokeCollection strokeCollection2 = new StrokeCollection();
		((List<Stroke>)strokeCollection2.Items).Add(stroke);
		RaiseStrokesChanged(strokeCollection2, strokeCollection, index);
	}

	/// <summary>Returns the index of the specified <see cref="T:System.Windows.Ink.Stroke" /> in the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <returns>The index of the stroke.</returns>
	/// <param name="stroke">The stroke whose position is required.</param>
	public new int IndexOf(Stroke stroke)
	{
		if (stroke == null)
		{
			return -1;
		}
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i] == stroke)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Removes the specified strokes from the collection.</summary>
	public void Remove(StrokeCollection strokes)
	{
		if (strokes == null)
		{
			throw new ArgumentNullException("strokes");
		}
		if (strokes.Count != 0)
		{
			int[] strokeIndexes = GetStrokeIndexes(strokes);
			if (strokeIndexes == null)
			{
				throw new ArgumentException(SR.InvalidRemovedStroke, "strokes")
				{
					Data = { 
					{
						(object)"System.Windows.Ink.StrokeCollection",
						(object?)""
					} }
				};
			}
			for (int num = strokeIndexes.Length - 1; num >= 0; num--)
			{
				((List<Stroke>)base.Items).RemoveAt(strokeIndexes[num]);
			}
			RaiseStrokesChanged(null, strokes, strokeIndexes[0]);
		}
	}

	/// <summary>Adds the specified strokes to the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <param name="strokes">The <see cref="T:System.Windows.Ink.StrokeCollection" /> to add to the collection.</param>
	/// <exception cref="T:System.ArgumentException">A <see cref="T:System.Windows.Ink.Stroke" /> in <paramref name="strokes" /> is already a member of the <see cref="T:System.Windows.Ink.StrokeCollection" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="strokes" /> is null.</exception>
	public void Add(StrokeCollection strokes)
	{
		if (strokes == null)
		{
			throw new ArgumentNullException("strokes");
		}
		if (strokes.Count == 0)
		{
			return;
		}
		int count = base.Count;
		for (int i = 0; i < strokes.Count; i++)
		{
			Stroke stroke = strokes[i];
			if (IndexOf(stroke) != -1)
			{
				throw new ArgumentException(SR.StrokeIsDuplicated, "strokes");
			}
		}
		((List<Stroke>)base.Items).AddRange(strokes);
		RaiseStrokesChanged(strokes, null, count);
	}

	/// <summary>Replaces the specified <see cref="T:System.Windows.Ink.Stroke" /> with the specified <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <param name="strokeToReplace">The <see cref="T:System.Windows.Ink.Stroke" /> to replace.</param>
	/// <param name="strokesToReplaceWith">The source <see cref="T:System.Windows.Ink.StrokeCollection" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="strokesToReplace" /> is empty.-or-<paramref name="strokesToReplaceWith" /> is empty.-or-A <see cref="T:System.Windows.Ink.Stroke" /> in <paramref name="strokesToReplaceWith" /> is already in <paramref name="strokesToReplace" />. </exception>
	public void Replace(Stroke strokeToReplace, StrokeCollection strokesToReplaceWith)
	{
		if (strokeToReplace == null)
		{
			throw new ArgumentNullException(SR.EmptyScToReplace);
		}
		StrokeCollection strokeCollection = new StrokeCollection();
		strokeCollection.Add(strokeToReplace);
		Replace(strokeCollection, strokesToReplaceWith);
	}

	/// <summary>Replaces the specified <see cref="T:System.Windows.Ink.StrokeCollection" /> with a new <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <param name="strokesToReplace">The destination <see cref="T:System.Windows.Ink.StrokeCollection" />.</param>
	/// <param name="strokesToReplaceWith">The source <see cref="T:System.Windows.Ink.StrokeCollection" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="strokesToReplace" /> is empty.-or-<paramref name="strokesToReplaceWith" /> is empty.-or-A <see cref="T:System.Windows.Ink.Stroke" /> in <paramref name="strokesToReplaceWith" /> is already in <paramref name="strokesToReplace." />-or-The strokes in <paramref name="strokesToReplaceWith" /> are not continuous. </exception>
	public void Replace(StrokeCollection strokesToReplace, StrokeCollection strokesToReplaceWith)
	{
		if (strokesToReplace == null)
		{
			throw new ArgumentNullException(SR.EmptyScToReplace);
		}
		if (strokesToReplaceWith == null)
		{
			throw new ArgumentNullException(SR.EmptyScToReplaceWith);
		}
		if (strokesToReplace.Count == 0)
		{
			throw new ArgumentException(SR.EmptyScToReplace, "strokesToReplace")
			{
				Data = { 
				{
					(object)"System.Windows.Ink.StrokeCollection",
					(object?)""
				} }
			};
		}
		int[] strokeIndexes = GetStrokeIndexes(strokesToReplace);
		if (strokeIndexes == null)
		{
			throw new ArgumentException(SR.InvalidRemovedStroke, "strokesToReplace")
			{
				Data = { 
				{
					(object)"System.Windows.Ink.StrokeCollection",
					(object?)""
				} }
			};
		}
		for (int i = 0; i < strokesToReplaceWith.Count; i++)
		{
			Stroke stroke = strokesToReplaceWith[i];
			if (IndexOf(stroke) != -1)
			{
				throw new ArgumentException(SR.StrokeIsDuplicated, "strokesToReplaceWith");
			}
		}
		for (int num = strokeIndexes.Length - 1; num >= 0; num--)
		{
			((List<Stroke>)base.Items).RemoveAt(strokeIndexes[num]);
		}
		if (strokesToReplaceWith.Count > 0)
		{
			((List<Stroke>)base.Items).InsertRange(strokeIndexes[0], strokesToReplaceWith);
		}
		RaiseStrokesChanged(strokesToReplaceWith, strokesToReplace, strokeIndexes[0]);
	}

	internal void AddWithoutEvent(Stroke stroke)
	{
		((List<Stroke>)base.Items).Add(stroke);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Ink.StrokeCollection.StrokesChanged" /> event. </summary>
	/// <param name="e">A <see cref="T:System.Windows.Ink.StrokeCollectionChangedEventArgs" /> that contains the event data. </param>
	protected virtual void OnStrokesChanged(StrokeCollectionChangedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e", SR.EventArgIsNull);
		}
		if (this.StrokesChangedInternal != null)
		{
			this.StrokesChangedInternal(this, e);
		}
		if (this.StrokesChanged != null)
		{
			this.StrokesChanged(this, e);
		}
		if (_collectionChanged != null)
		{
			NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs = null;
			notifyCollectionChangedEventArgs = ((base.Count == 0) ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset) : ((e.Added.Count == 0) ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.Removed, e.Index) : ((e.Removed.Count != 0) ? new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.Added, e.Removed, e.Index) : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.Added, e.Index))));
			_collectionChanged(this, notifyCollectionChangedEventArgs);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Ink.StrokeCollection.PropertyDataChanged" /> event. </summary>
	protected virtual void OnPropertyDataChanged(PropertyDataChangedEventArgs e)
	{
		if (e == null)
		{
			throw new ArgumentNullException("e", SR.EventArgIsNull);
		}
		if (this.PropertyDataChanged != null)
		{
			this.PropertyDataChanged(this, e);
		}
	}

	/// <summary>Occurs when any <see cref="T:System.Windows.Ink.StrokeCollection" /> property changes.</summary>
	/// <param name="e">Event data.</param>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (_propertyChanged != null)
		{
			_propertyChanged(this, e);
		}
	}

	private int OptimisticIndexOf(int startingIndex, Stroke stroke)
	{
		for (int i = startingIndex; i < base.Count; i++)
		{
			if (base[i] == stroke)
			{
				return i;
			}
		}
		for (int j = 0; j < startingIndex; j++)
		{
			if (base[j] == stroke)
			{
				return j;
			}
		}
		return -1;
	}

	private int[] GetStrokeIndexes(StrokeCollection strokes)
	{
		int[] array = new int[strokes.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = int.MaxValue;
		}
		int num = 0;
		int num2 = -1;
		int num3 = 0;
		for (int j = 0; j < strokes.Count; j++)
		{
			num = OptimisticIndexOf(num, strokes[j]);
			if (num == -1)
			{
				return null;
			}
			if (num > num2)
			{
				array[num3++] = num;
				num2 = num;
				continue;
			}
			for (int k = 0; k < array.Length; k++)
			{
				if (num >= array[k])
				{
					continue;
				}
				if (array[k] != int.MaxValue)
				{
					for (int num4 = array.Length - 1; num4 > k; num4--)
					{
						array[num4] = array[num4 - 1];
					}
				}
				array[k] = num;
				num3++;
				if (num > num2)
				{
					num2 = num;
				}
				break;
			}
		}
		return array;
	}

	private void RaiseStrokesChanged(StrokeCollection addedStrokes, StrokeCollection removedStrokes, int index)
	{
		StrokeCollectionChangedEventArgs e = new StrokeCollectionChangedEventArgs(addedStrokes, removedStrokes, index);
		OnPropertyChanged("Count");
		OnPropertyChanged("Item[]");
		OnStrokesChanged(e);
	}

	private void OnPropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>Returns the bounds of the strokes in the collection.</summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> that contains the bounds of the strokes in the <see cref="T:System.Windows.Ink.StrokeCollection" />.</returns>
	public Rect GetBounds()
	{
		Rect empty = Rect.Empty;
		using IEnumerator<Stroke> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Stroke current = enumerator.Current;
			empty.Union(current.GetBounds());
		}
		return empty;
	}

	/// <summary>Returns a collection of strokes that intersect the specified point.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Ink.Stroke" /> objects that intersect the specified point.</returns>
	/// <param name="point">The point to hit test.</param>
	public StrokeCollection HitTest(Point point)
	{
		return PointHitTest(point, new RectangleStylusShape(1.0, 1.0));
	}

	/// <summary>Returns a collection of strokes that intersect the specified area.</summary>
	/// <returns>A collection of <see cref="T:System.Windows.Ink.Stroke" /> objects that intersect the specified point.</returns>
	/// <param name="point">The <see cref="T:System.Windows.Point" /> to hit test.</param>
	/// <param name="diameter">The size of the area around the <see cref="T:System.Windows.Point" /> to hit test.</param>
	public StrokeCollection HitTest(Point point, double diameter)
	{
		if (double.IsNaN(diameter) || diameter < DrawingAttributes.MinWidth || diameter > DrawingAttributes.MaxWidth)
		{
			throw new ArgumentOutOfRangeException("diameter", SR.InvalidDiameter);
		}
		return PointHitTest(point, new EllipseStylusShape(diameter, diameter));
	}

	/// <summary>Returns a collection of strokes that have at least the specified percentage of length within the specified area.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> that has strokes with at least the specified percentage within the <see cref="T:System.Windows.Point" /> array.</returns>
	/// <param name="lassoPoints">An array of type <see cref="T:System.Windows.Point" /> that represents the bounds of the area to be hit tested.</param>
	/// <param name="percentageWithinLasso">The acceptable length of the <see cref="T:System.Windows.Ink.Stroke" />, as a percentage, for <paramref name="lassoPoints" /> to contain.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="lassoPoints" /> is null.-or-<paramref name="percentageWithinLasso" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="lassoPoints" /> contains an empty array.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="percentageWithinLasso" /> is less than 0 or greater than 100.</exception>
	public StrokeCollection HitTest(IEnumerable<Point> lassoPoints, int percentageWithinLasso)
	{
		if (lassoPoints == null)
		{
			throw new ArgumentNullException("lassoPoints");
		}
		if (percentageWithinLasso < 0 || percentageWithinLasso > 100)
		{
			throw new ArgumentOutOfRangeException("percentageWithinLasso");
		}
		if (IEnumerablePointHelper.GetCount(lassoPoints) < 3)
		{
			return new StrokeCollection();
		}
		Lasso lasso = new SingleLoopLasso();
		lasso.AddPoints(lassoPoints);
		StrokeCollection strokeCollection = new StrokeCollection();
		using IEnumerator<Stroke> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Stroke current = enumerator.Current;
			if (percentageWithinLasso == 0)
			{
				strokeCollection.Add(current);
				continue;
			}
			StrokeInfo strokeInfo = null;
			try
			{
				strokeInfo = new StrokeInfo(current);
				StylusPointCollection stylusPoints = strokeInfo.StylusPoints;
				double num = strokeInfo.TotalWeight * (double)percentageWithinLasso / 100.0 - 0.0001;
				for (int i = 0; i < stylusPoints.Count; i++)
				{
					if (lasso.Contains((Point)stylusPoints[i]))
					{
						num -= strokeInfo.GetPointWeight(i);
						if (DoubleUtil.LessThanOrClose(num, 0.0))
						{
							strokeCollection.Add(current);
							break;
						}
					}
				}
			}
			finally
			{
				strokeInfo?.Detach();
			}
		}
		return strokeCollection;
	}

	/// <summary>Returns a collection of strokes that have at least the specified percentage of length within the specified rectangle.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> that has strokes with at least the specified percentage within the <see cref="T:System.Windows.Rect" />.</returns>
	/// <param name="bounds">A <see cref="T:System.Windows.Rect" /> that specifies the bounds to be hit tested.</param>
	/// <param name="percentageWithinBounds">The minimum required length of a Stroke that must exist within bounds for it to be considered hit.</param>
	public StrokeCollection HitTest(Rect bounds, int percentageWithinBounds)
	{
		if (percentageWithinBounds < 0 || percentageWithinBounds > 100)
		{
			throw new ArgumentOutOfRangeException("percentageWithinBounds");
		}
		if (bounds.IsEmpty)
		{
			return new StrokeCollection();
		}
		StrokeCollection strokeCollection = new StrokeCollection();
		using IEnumerator<Stroke> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Stroke current = enumerator.Current;
			if (current.HitTest(bounds, percentageWithinBounds))
			{
				strokeCollection.Add(current);
			}
		}
		return strokeCollection;
	}

	/// <summary>Returns a collection of strokes that intersect with the specified path.</summary>
	/// <returns>A <see cref="T:System.Windows.Ink.StrokeCollection" /> of strokes that intersect with <paramref name="path" />.</returns>
	/// <param name="path">An array to type <see cref="T:System.Windows.Point" /> that represents the path to be hit tested.</param>
	/// <param name="stylusShape">The <see cref="T:System.Windows.Ink.StylusShape" /> that specifies the shape of <paramref name="eraserPath" />.</param>
	public StrokeCollection HitTest(IEnumerable<Point> path, StylusShape stylusShape)
	{
		if (stylusShape == null)
		{
			throw new ArgumentNullException("stylusShape");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (IEnumerablePointHelper.GetCount(path) == 0)
		{
			return new StrokeCollection();
		}
		ErasingStroke erasingStroke = new ErasingStroke(stylusShape, path);
		Rect bounds = erasingStroke.Bounds;
		if (bounds.IsEmpty)
		{
			return new StrokeCollection();
		}
		StrokeCollection strokeCollection = new StrokeCollection();
		using IEnumerator<Stroke> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Stroke current = enumerator.Current;
			if (bounds.IntersectsWith(current.GetBounds()) && erasingStroke.HitTest(StrokeNodeIterator.GetIterator(current, current.DrawingAttributes)))
			{
				strokeCollection.Add(current);
			}
		}
		return strokeCollection;
	}

	/// <summary>Removes all strokes in the <see cref="T:System.Windows.Ink.StrokeCollection" /> that are outside the bounds of the specified <see cref="T:System.Drawing.Point" /> array.</summary>
	/// <param name="lassoPoints">An array of type <see cref="T:System.Drawing.Point" /> that specifies the area to be clipped.</param>
	public void Clip(IEnumerable<Point> lassoPoints)
	{
		if (lassoPoints == null)
		{
			throw new ArgumentNullException("lassoPoints");
		}
		int count = IEnumerablePointHelper.GetCount(lassoPoints);
		if (count == 0)
		{
			throw new ArgumentException(SR.EmptyArray);
		}
		if (count < 3)
		{
			Clear();
			return;
		}
		Lasso lasso = new SingleLoopLasso();
		lasso.AddPoints(lassoPoints);
		for (int i = 0; i < base.Count; i++)
		{
			Stroke stroke = base[i];
			StrokeCollection toReplace = stroke.Clip(stroke.HitTest(lasso));
			UpdateStrokeCollection(stroke, toReplace, ref i);
		}
	}

	/// <summary>Replaces all strokes that are clipped by the specified rectangle with new strokes that do not extend beyond the specified rectangle.  </summary>
	/// <param name="bounds">A <see cref="T:System.Windows.Rect" /> that specifies the area to be clipped.</param>
	public void Clip(Rect bounds)
	{
		if (!bounds.IsEmpty)
		{
			Clip(new Point[4] { bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft });
		}
	}

	/// <summary>Removes the ink that is within the bounds of the specified area.</summary>
	/// <param name="lassoPoints">An array of type <see cref="T:System.Drawing.Point" /> that specifies the area to be erased.</param>
	public void Erase(IEnumerable<Point> lassoPoints)
	{
		if (lassoPoints == null)
		{
			throw new ArgumentNullException("lassoPoints");
		}
		int count = IEnumerablePointHelper.GetCount(lassoPoints);
		if (count == 0)
		{
			throw new ArgumentException(SR.EmptyArray);
		}
		if (count >= 3)
		{
			Lasso lasso = new SingleLoopLasso();
			lasso.AddPoints(lassoPoints);
			for (int i = 0; i < base.Count; i++)
			{
				Stroke stroke = base[i];
				StrokeCollection toReplace = stroke.Erase(stroke.HitTest(lasso));
				UpdateStrokeCollection(stroke, toReplace, ref i);
			}
		}
	}

	/// <summary>Replaces all strokes that are clipped by the specified rectangle with new strokes that do not enter the bounds of the specified rectangle. </summary>
	public void Erase(Rect bounds)
	{
		if (!bounds.IsEmpty)
		{
			Erase(new Point[4] { bounds.TopLeft, bounds.TopRight, bounds.BottomRight, bounds.BottomLeft });
		}
	}

	/// <summary>Replaces all strokes that are clipped by the region created by the specified <see cref="T:System.Windows.Ink.StylusShape" /> along the specified path with new Strokes that are not clipped by the region.</summary>
	/// <param name="eraserPath">An array of type <see cref="T:System.Windows.Point" /> that specifies the path to be erased.</param>
	/// <param name="eraserShape">A <see cref="T:System.Windows.Ink.StylusShape" /> that specifies the shape of the eraser.</param>
	public void Erase(IEnumerable<Point> eraserPath, StylusShape eraserShape)
	{
		if (eraserShape == null)
		{
			throw new ArgumentNullException(SR.SCEraseShape);
		}
		if (eraserPath == null)
		{
			throw new ArgumentNullException(SR.SCErasePath);
		}
		if (IEnumerablePointHelper.GetCount(eraserPath) != 0)
		{
			ErasingStroke erasingStroke = new ErasingStroke(eraserShape, eraserPath);
			for (int i = 0; i < base.Count; i++)
			{
				Stroke stroke = base[i];
				List<StrokeIntersection> list = new List<StrokeIntersection>();
				erasingStroke.EraseTest(StrokeNodeIterator.GetIterator(stroke, stroke.DrawingAttributes), list);
				StrokeCollection toReplace = stroke.Erase(list.ToArray());
				UpdateStrokeCollection(stroke, toReplace, ref i);
			}
		}
	}

	/// <summary>Draws the strokes in the <see cref="T:System.Windows.Ink.StrokeCollection" />.</summary>
	/// <param name="context">The <see cref="T:System.Windows.Media.DrawingContext" /> on which to draw the <see cref="T:System.Windows.Ink.StrokeCollection" />.</param>
	public void Draw(DrawingContext context)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		List<Stroke> list = new List<Stroke>();
		Dictionary<Color, List<Stroke>> dictionary = new Dictionary<Color, List<Stroke>>();
		for (int i = 0; i < base.Count; i++)
		{
			Stroke stroke = base[i];
			if (stroke.DrawingAttributes.IsHighlighter)
			{
				Color highlighterColor = StrokeRenderer.GetHighlighterColor(stroke.DrawingAttributes.Color);
				if (!dictionary.TryGetValue(highlighterColor, out var value))
				{
					value = new List<Stroke>();
					dictionary.Add(highlighterColor, value);
				}
				value.Add(stroke);
			}
			else
			{
				list.Add(stroke);
			}
		}
		foreach (List<Stroke> value2 in dictionary.Values)
		{
			context.PushOpacity(0.5);
			try
			{
				foreach (Stroke item in value2)
				{
					item.DrawInternal(context, StrokeRenderer.GetHighlighterAttributes(item, item.DrawingAttributes), drawAsHollow: false);
				}
			}
			finally
			{
				context.Pop();
			}
		}
		foreach (Stroke item2 in list)
		{
			item2.DrawInternal(context, item2.DrawingAttributes, drawAsHollow: false);
		}
	}

	/// <summary>Creates an <see cref="T:System.Windows.Ink.IncrementalStrokeHitTester" /> that hit tests the <see cref="T:System.Windows.Ink.StrokeCollection" /> with an erasing path.</summary>
	/// <returns>An <see cref="T:System.Windows.Ink.IncrementalStrokeHitTester" /> that hit tests the <see cref="T:System.Windows.Ink.StrokeCollection" />.</returns>
	/// <param name="eraserShape">A <see cref="T:System.Windows.Ink.StylusShape" /> that specifies the tip of the stylus.</param>
	public IncrementalStrokeHitTester GetIncrementalStrokeHitTester(StylusShape eraserShape)
	{
		if (eraserShape == null)
		{
			throw new ArgumentNullException("eraserShape");
		}
		return new IncrementalStrokeHitTester(this, eraserShape);
	}

	/// <summary>Creates an <see cref="T:System.Windows.Ink.IncrementalLassoHitTester" /> that hit tests the <see cref="T:System.Windows.Ink.StrokeCollection" /> with a lasso (freehand) path.</summary>
	/// <returns>An <see cref="T:System.Windows.Ink.IncrementalLassoHitTester" /> that hit tests the <see cref="T:System.Windows.Ink.StrokeCollection" />.</returns>
	/// <param name="percentageWithinLasso">The minimum percentage of each <see cref="T:System.Windows.Ink.Stroke" /> that must be contained within the lasso for it to be considered hit.</param>
	public IncrementalLassoHitTester GetIncrementalLassoHitTester(int percentageWithinLasso)
	{
		if (percentageWithinLasso < 0 || percentageWithinLasso > 100)
		{
			throw new ArgumentOutOfRangeException("percentageWithinLasso");
		}
		return new IncrementalLassoHitTester(this, percentageWithinLasso);
	}

	private StrokeCollection PointHitTest(Point point, StylusShape shape)
	{
		StrokeCollection strokeCollection = new StrokeCollection();
		for (int i = 0; i < base.Count; i++)
		{
			Stroke stroke = base[i];
			if (stroke.HitTest(new Point[1] { point }, shape))
			{
				strokeCollection.Add(stroke);
			}
		}
		return strokeCollection;
	}

	private void UpdateStrokeCollection(Stroke original, StrokeCollection toReplace, ref int index)
	{
		if (toReplace.Count == 0)
		{
			Remove(original);
			index--;
		}
		else if (toReplace.Count != 1 || toReplace[0] != original)
		{
			Replace(original, toReplace);
			index += toReplace.Count - 1;
		}
	}
}
