using System.Collections.Generic;

namespace System.Xml.Linq;

/// <summary>Represents a node or an attribute in an XML tree. </summary>
/// <filterpriority>2</filterpriority>
public abstract class XObject : IXmlLineInfo
{
	internal XContainer parent;

	internal object annotations;

	/// <summary>Gets the base URI for this <see cref="T:System.Xml.Linq.XObject" />.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the base URI for this <see cref="T:System.Xml.Linq.XObject" />.</returns>
	/// <filterpriority>2</filterpriority>
	public string BaseUri
	{
		get
		{
			XObject xObject = this;
			while (true)
			{
				if (xObject != null && xObject.annotations == null)
				{
					xObject = xObject.parent;
					continue;
				}
				if (xObject == null)
				{
					break;
				}
				BaseUriAnnotation baseUriAnnotation = xObject.Annotation<BaseUriAnnotation>();
				if (baseUriAnnotation != null)
				{
					return baseUriAnnotation.baseUri;
				}
				xObject = xObject.parent;
			}
			return string.Empty;
		}
	}

	/// <summary>Gets the <see cref="T:System.Xml.Linq.XDocument" /> for this <see cref="T:System.Xml.Linq.XObject" />.</summary>
	/// <returns>The <see cref="T:System.Xml.Linq.XDocument" /> for this <see cref="T:System.Xml.Linq.XObject" />. </returns>
	public XDocument Document
	{
		get
		{
			XObject xObject = this;
			while (xObject.parent != null)
			{
				xObject = xObject.parent;
			}
			return xObject as XDocument;
		}
	}

	/// <summary>Gets the node type for this <see cref="T:System.Xml.Linq.XObject" />.</summary>
	/// <returns>The node type for this <see cref="T:System.Xml.Linq.XObject" />. </returns>
	public abstract XmlNodeType NodeType { get; }

	/// <summary>Gets the parent <see cref="T:System.Xml.Linq.XElement" /> of this <see cref="T:System.Xml.Linq.XObject" />.</summary>
	/// <returns>The parent <see cref="T:System.Xml.Linq.XElement" /> of this <see cref="T:System.Xml.Linq.XObject" />.</returns>
	public XElement Parent => parent as XElement;

	/// <summary>Gets the line number that the underlying <see cref="T:System.Xml.XmlReader" /> reported for this <see cref="T:System.Xml.Linq.XObject" />.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that contains the line number reported by the <see cref="T:System.Xml.XmlReader" /> for this <see cref="T:System.Xml.Linq.XObject" />.</returns>
	int IXmlLineInfo.LineNumber => Annotation<LineInfoAnnotation>()?.lineNumber ?? 0;

	/// <summary>Gets the line position that the underlying <see cref="T:System.Xml.XmlReader" /> reported for this <see cref="T:System.Xml.Linq.XObject" />.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that contains the line position reported by the <see cref="T:System.Xml.XmlReader" /> for this <see cref="T:System.Xml.Linq.XObject" />.</returns>
	int IXmlLineInfo.LinePosition => Annotation<LineInfoAnnotation>()?.linePosition ?? 0;

	internal bool HasBaseUri => Annotation<BaseUriAnnotation>() != null;

	/// <summary>Raised when this <see cref="T:System.Xml.Linq.XObject" /> or any of its descendants have changed.</summary>
	public event EventHandler<XObjectChangeEventArgs> Changed
	{
		add
		{
			if (value != null)
			{
				XObjectChangeAnnotation xObjectChangeAnnotation = Annotation<XObjectChangeAnnotation>();
				if (xObjectChangeAnnotation == null)
				{
					xObjectChangeAnnotation = new XObjectChangeAnnotation();
					AddAnnotation(xObjectChangeAnnotation);
				}
				XObjectChangeAnnotation xObjectChangeAnnotation2 = xObjectChangeAnnotation;
				xObjectChangeAnnotation2.changed = (EventHandler<XObjectChangeEventArgs>)Delegate.Combine(xObjectChangeAnnotation2.changed, value);
			}
		}
		remove
		{
			if (value == null)
			{
				return;
			}
			XObjectChangeAnnotation xObjectChangeAnnotation = Annotation<XObjectChangeAnnotation>();
			if (xObjectChangeAnnotation != null)
			{
				xObjectChangeAnnotation.changed = (EventHandler<XObjectChangeEventArgs>)Delegate.Remove(xObjectChangeAnnotation.changed, value);
				if (xObjectChangeAnnotation.changing == null && xObjectChangeAnnotation.changed == null)
				{
					RemoveAnnotations<XObjectChangeAnnotation>();
				}
			}
		}
	}

	/// <summary>Raised when this <see cref="T:System.Xml.Linq.XObject" /> or any of its descendants are about to change.</summary>
	public event EventHandler<XObjectChangeEventArgs> Changing
	{
		add
		{
			if (value != null)
			{
				XObjectChangeAnnotation xObjectChangeAnnotation = Annotation<XObjectChangeAnnotation>();
				if (xObjectChangeAnnotation == null)
				{
					xObjectChangeAnnotation = new XObjectChangeAnnotation();
					AddAnnotation(xObjectChangeAnnotation);
				}
				XObjectChangeAnnotation xObjectChangeAnnotation2 = xObjectChangeAnnotation;
				xObjectChangeAnnotation2.changing = (EventHandler<XObjectChangeEventArgs>)Delegate.Combine(xObjectChangeAnnotation2.changing, value);
			}
		}
		remove
		{
			if (value == null)
			{
				return;
			}
			XObjectChangeAnnotation xObjectChangeAnnotation = Annotation<XObjectChangeAnnotation>();
			if (xObjectChangeAnnotation != null)
			{
				xObjectChangeAnnotation.changing = (EventHandler<XObjectChangeEventArgs>)Delegate.Remove(xObjectChangeAnnotation.changing, value);
				if (xObjectChangeAnnotation.changing == null && xObjectChangeAnnotation.changed == null)
				{
					RemoveAnnotations<XObjectChangeAnnotation>();
				}
			}
		}
	}

	internal XObject()
	{
	}

	/// <summary>Adds an object to the annotation list of this <see cref="T:System.Xml.Linq.XObject" />.</summary>
	/// <param name="annotation">An <see cref="T:System.Object" /> that contains the annotation to add.</param>
	public void AddAnnotation(object annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		if (annotations == null)
		{
			annotations = ((!(annotation is object[])) ? annotation : new object[1] { annotation });
			return;
		}
		object[] array = annotations as object[];
		if (array == null)
		{
			annotations = new object[2] { annotations, annotation };
			return;
		}
		int i;
		for (i = 0; i < array.Length && array[i] != null; i++)
		{
		}
		if (i == array.Length)
		{
			Array.Resize(ref array, i * 2);
			annotations = array;
		}
		array[i] = annotation;
	}

	/// <summary>Gets the first annotation object of the specified type from this <see cref="T:System.Xml.Linq.XObject" />.</summary>
	/// <returns>The <see cref="T:System.Object" /> that contains the first annotation object that matches the specified type, or null if no annotation is of the specified type.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> of the annotation to retrieve.</param>
	public object Annotation(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (annotations != null)
		{
			if (!(annotations is object[] array))
			{
				if (type.IsInstanceOfType(annotations))
				{
					return annotations;
				}
			}
			else
			{
				foreach (object obj in array)
				{
					if (obj == null)
					{
						break;
					}
					if (type.IsInstanceOfType(obj))
					{
						return obj;
					}
				}
			}
		}
		return null;
	}

	/// <summary>Get the first annotation object of the specified type from this <see cref="T:System.Xml.Linq.XObject" />. </summary>
	/// <returns>The first annotation object that matches the specified type, or null if no annotation is of the specified type.</returns>
	/// <typeparam name="T">The type of the annotation to retrieve.</typeparam>
	public T Annotation<T>() where T : class
	{
		if (annotations != null)
		{
			if (!(annotations is object[] array))
			{
				return annotations as T;
			}
			foreach (object obj in array)
			{
				if (obj == null)
				{
					break;
				}
				if (obj is T result)
				{
					return result;
				}
			}
		}
		return null;
	}

	/// <summary>Gets a collection of annotations of the specified type for this <see cref="T:System.Xml.Linq.XObject" />.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Object" /> that contains the annotations that match the specified type for this <see cref="T:System.Xml.Linq.XObject" />.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> of the annotations to retrieve.</param>
	public IEnumerable<object> Annotations(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		return AnnotationsIterator(type);
	}

	private IEnumerable<object> AnnotationsIterator(Type type)
	{
		if (annotations == null)
		{
			yield break;
		}
		if (!(annotations is object[] a))
		{
			if (type.IsInstanceOfType(annotations))
			{
				yield return annotations;
			}
			yield break;
		}
		foreach (object obj in a)
		{
			if (obj != null)
			{
				if (type.IsInstanceOfType(obj))
				{
					yield return obj;
				}
				continue;
			}
			break;
		}
	}

	/// <summary>Gets a collection of annotations of the specified type for this <see cref="T:System.Xml.Linq.XObject" />.</summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the annotations for this <see cref="T:System.Xml.Linq.XObject" />.</returns>
	/// <typeparam name="T">The type of the annotations to retrieve.</typeparam>
	public IEnumerable<T> Annotations<T>() where T : class
	{
		if (annotations == null)
		{
			yield break;
		}
		if (!(annotations is object[] a))
		{
			if (annotations is T val)
			{
				yield return val;
			}
			yield break;
		}
		foreach (object obj in a)
		{
			if (obj != null)
			{
				if (obj is T val2)
				{
					yield return val2;
				}
				continue;
			}
			break;
		}
	}

	/// <summary>Removes the annotations of the specified type from this <see cref="T:System.Xml.Linq.XObject" />.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> of annotations to remove.</param>
	public void RemoveAnnotations(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (annotations == null)
		{
			return;
		}
		if (!(annotations is object[] array))
		{
			if (type.IsInstanceOfType(annotations))
			{
				annotations = null;
			}
			return;
		}
		int i = 0;
		int num = 0;
		for (; i < array.Length; i++)
		{
			object obj = array[i];
			if (obj == null)
			{
				break;
			}
			if (!type.IsInstanceOfType(obj))
			{
				array[num++] = obj;
			}
		}
		if (num == 0)
		{
			annotations = null;
			return;
		}
		while (num < i)
		{
			array[num++] = null;
		}
	}

	/// <summary>Removes the annotations of the specified type from this <see cref="T:System.Xml.Linq.XObject" />.</summary>
	/// <typeparam name="T">The type of annotations to remove.</typeparam>
	public void RemoveAnnotations<T>() where T : class
	{
		if (annotations == null)
		{
			return;
		}
		if (!(annotations is object[] array))
		{
			if (annotations is T)
			{
				annotations = null;
			}
			return;
		}
		int i = 0;
		int num = 0;
		for (; i < array.Length; i++)
		{
			object obj = array[i];
			if (obj == null)
			{
				break;
			}
			if (!(obj is T))
			{
				array[num++] = obj;
			}
		}
		if (num == 0)
		{
			annotations = null;
			return;
		}
		while (num < i)
		{
			array[num++] = null;
		}
	}

	/// <summary>Gets a value indicating whether or not this <see cref="T:System.Xml.Linq.XObject" /> has line information.</summary>
	/// <returns>true if the <see cref="T:System.Xml.Linq.XObject" /> has line information, otherwise false.</returns>
	bool IXmlLineInfo.HasLineInfo()
	{
		return Annotation<LineInfoAnnotation>() != null;
	}

	internal bool NotifyChanged(object sender, XObjectChangeEventArgs e)
	{
		bool result = false;
		XObject xObject = this;
		while (true)
		{
			if (xObject != null && xObject.annotations == null)
			{
				xObject = xObject.parent;
				continue;
			}
			if (xObject == null)
			{
				break;
			}
			XObjectChangeAnnotation xObjectChangeAnnotation = xObject.Annotation<XObjectChangeAnnotation>();
			if (xObjectChangeAnnotation != null)
			{
				result = true;
				if (xObjectChangeAnnotation.changed != null)
				{
					xObjectChangeAnnotation.changed(sender, e);
				}
			}
			xObject = xObject.parent;
		}
		return result;
	}

	internal bool NotifyChanging(object sender, XObjectChangeEventArgs e)
	{
		bool result = false;
		XObject xObject = this;
		while (true)
		{
			if (xObject != null && xObject.annotations == null)
			{
				xObject = xObject.parent;
				continue;
			}
			if (xObject == null)
			{
				break;
			}
			XObjectChangeAnnotation xObjectChangeAnnotation = xObject.Annotation<XObjectChangeAnnotation>();
			if (xObjectChangeAnnotation != null)
			{
				result = true;
				if (xObjectChangeAnnotation.changing != null)
				{
					xObjectChangeAnnotation.changing(sender, e);
				}
			}
			xObject = xObject.parent;
		}
		return result;
	}

	internal void SetBaseUri(string baseUri)
	{
		AddAnnotation(new BaseUriAnnotation(baseUri));
	}

	internal void SetLineInfo(int lineNumber, int linePosition)
	{
		AddAnnotation(new LineInfoAnnotation(lineNumber, linePosition));
	}

	internal bool SkipNotify()
	{
		XObject xObject = this;
		while (true)
		{
			if (xObject != null && xObject.annotations == null)
			{
				xObject = xObject.parent;
				continue;
			}
			if (xObject == null)
			{
				return true;
			}
			if (xObject.Annotations<XObjectChangeAnnotation>() != null)
			{
				break;
			}
			xObject = xObject.parent;
		}
		return false;
	}

	internal SaveOptions GetSaveOptionsFromAnnotations()
	{
		XObject xObject = this;
		object obj;
		while (true)
		{
			if (xObject != null && xObject.annotations == null)
			{
				xObject = xObject.parent;
				continue;
			}
			if (xObject == null)
			{
				return SaveOptions.None;
			}
			obj = xObject.Annotation(typeof(SaveOptions));
			if (obj != null)
			{
				break;
			}
			xObject = xObject.parent;
		}
		return (SaveOptions)obj;
	}
}
