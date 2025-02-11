using System.Collections;
using System.Diagnostics;
using System.Text;

namespace System.Xml.XPath;

/// <summary>Provides an iterator over a selected set of nodes.</summary>
[DebuggerDisplay("Position={CurrentPosition}, Current={debuggerDisplayProxy}")]
public abstract class XPathNodeIterator : ICloneable, IEnumerable
{
	private class Enumerator : IEnumerator
	{
		private XPathNodeIterator original;

		private XPathNodeIterator current;

		private bool iterationStarted;

		public virtual object Current
		{
			get
			{
				if (iterationStarted)
				{
					if (current == null)
					{
						throw new InvalidOperationException(Res.GetString("Enumeration has already finished.", string.Empty));
					}
					return current.Current.Clone();
				}
				throw new InvalidOperationException(Res.GetString("Enumeration has not started. Call MoveNext.", string.Empty));
			}
		}

		public Enumerator(XPathNodeIterator original)
		{
			this.original = original.Clone();
		}

		public virtual bool MoveNext()
		{
			if (!iterationStarted)
			{
				current = original.Clone();
				iterationStarted = true;
			}
			if (current == null || !current.MoveNext())
			{
				current = null;
				return false;
			}
			return true;
		}

		public virtual void Reset()
		{
			iterationStarted = false;
		}
	}

	private struct DebuggerDisplayProxy
	{
		private XPathNodeIterator nodeIterator;

		public DebuggerDisplayProxy(XPathNodeIterator nodeIterator)
		{
			this.nodeIterator = nodeIterator;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Position=");
			stringBuilder.Append(nodeIterator.CurrentPosition);
			stringBuilder.Append(", Current=");
			if (nodeIterator.Current == null)
			{
				stringBuilder.Append("null");
			}
			else
			{
				stringBuilder.Append('{');
				stringBuilder.Append(new XPathNavigator.DebuggerDisplayProxy(nodeIterator.Current).ToString());
				stringBuilder.Append('}');
			}
			return stringBuilder.ToString();
		}
	}

	internal int count = -1;

	/// <summary>When overridden in a derived class, gets the <see cref="T:System.Xml.XPath.XPathNavigator" /> object for this <see cref="T:System.Xml.XPath.XPathNodeIterator" />, positioned on the current context node.</summary>
	/// <returns>An <see cref="T:System.Xml.XPath.XPathNavigator" /> object positioned on the context node from which the node set was selected. The <see cref="M:System.Xml.XPath.XPathNodeIterator.MoveNext" /> method must be called to move the <see cref="T:System.Xml.XPath.XPathNodeIterator" /> to the first node in the selected set.</returns>
	public abstract XPathNavigator Current { get; }

	/// <summary>When overridden in a derived class, gets the index of the current position in the selected set of nodes.</summary>
	/// <returns>The index of the current position.</returns>
	public abstract int CurrentPosition { get; }

	/// <summary>Gets the index of the last node in the selected set of nodes.</summary>
	/// <returns>The index of the last node in the selected set of nodes, or 0 if there are no selected nodes.</returns>
	public virtual int Count
	{
		get
		{
			if (count == -1)
			{
				XPathNodeIterator xPathNodeIterator = Clone();
				while (xPathNodeIterator.MoveNext())
				{
				}
				count = xPathNodeIterator.CurrentPosition;
			}
			return count;
		}
	}

	private object debuggerDisplayProxy
	{
		get
		{
			if (Current != null)
			{
				return new XPathNavigator.DebuggerDisplayProxy(Current);
			}
			return null;
		}
	}

	/// <summary>Creates a new object that is a copy of the current instance.</summary>
	/// <returns>A new object that is a copy of this instance.</returns>
	object ICloneable.Clone()
	{
		return Clone();
	}

	/// <summary>When overridden in a derived class, returns a clone of this <see cref="T:System.Xml.XPath.XPathNodeIterator" /> object.</summary>
	/// <returns>A new <see cref="T:System.Xml.XPath.XPathNodeIterator" /> object clone of this <see cref="T:System.Xml.XPath.XPathNodeIterator" /> object.</returns>
	public abstract XPathNodeIterator Clone();

	/// <summary>When overridden in a derived class, moves the <see cref="T:System.Xml.XPath.XPathNavigator" /> object returned by the <see cref="P:System.Xml.XPath.XPathNodeIterator.Current" /> property to the next node in the selected node set.</summary>
	/// <returns>true if the <see cref="T:System.Xml.XPath.XPathNavigator" /> object moved to the next node; false if there are no more selected nodes.</returns>
	public abstract bool MoveNext();

	/// <summary>Returns an <see cref="T:System.Collections.IEnumerator" /> object to iterate through the selected node set.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object to iterate through the selected node set.</returns>
	public virtual IEnumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XPath.XPathNodeIterator" /> class.</summary>
	protected XPathNodeIterator()
	{
	}
}
