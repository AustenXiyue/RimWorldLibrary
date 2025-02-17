using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MS.Internal.Xml.Linq.ComponentModel;

internal class XDeferredAxis<T> : IEnumerable<T>, IEnumerable where T : XObject
{
	private Func<XElement, XName, IEnumerable<T>> func;

	internal XElement element;

	internal XName name;

	public IEnumerable<T> this[string expandedName]
	{
		get
		{
			if (expandedName == null)
			{
				throw new ArgumentNullException("expandedName");
			}
			if (name == null)
			{
				name = expandedName;
			}
			else if (name != expandedName)
			{
				return Enumerable.Empty<T>();
			}
			return this;
		}
	}

	public XDeferredAxis(Func<XElement, XName, IEnumerable<T>> func, XElement element, XName name)
	{
		if (func == null)
		{
			throw new ArgumentNullException("func");
		}
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		this.func = func;
		this.element = element;
		this.name = name;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return func(element, name).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
