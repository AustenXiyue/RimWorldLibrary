using System.Collections;
using System.Security.Permissions;

namespace System.ComponentModel.Design.Serialization;

/// <summary>Provides a stack object that can be used by a serializer to make information available to nested serializers.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
public sealed class ContextStack
{
	private ArrayList contextStack;

	/// <summary>Gets the current object on the stack.</summary>
	/// <returns>The current object on the stack, or null if no objects were pushed.</returns>
	public object Current
	{
		get
		{
			if (contextStack != null && contextStack.Count > 0)
			{
				return contextStack[contextStack.Count - 1];
			}
			return null;
		}
	}

	/// <summary>Gets the object on the stack at the specified level.</summary>
	/// <returns>The object on the stack at the specified level, or null if no object exists at that level.</returns>
	/// <param name="level">The level of the object to retrieve on the stack. Level 0 is the top of the stack, level 1 is the next down, and so on. This level must be 0 or greater. If level is greater than the number of levels on the stack, it returns null. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="level" /> is less than 0.</exception>
	public object this[int level]
	{
		get
		{
			if (level < 0)
			{
				throw new ArgumentOutOfRangeException("level");
			}
			if (contextStack != null && level < contextStack.Count)
			{
				return contextStack[contextStack.Count - 1 - level];
			}
			return null;
		}
	}

	/// <summary>Gets the first object on the stack that inherits from or implements the specified type.</summary>
	/// <returns>The first object on the stack that inherits from or implements the specified type, or null if no object on the stack implements the type.</returns>
	/// <param name="type">A type to retrieve from the context stack. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="type" /> is null.</exception>
	public object this[Type type]
	{
		get
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (contextStack != null)
			{
				int num = contextStack.Count;
				while (num > 0)
				{
					object obj = contextStack[--num];
					if (type.IsInstanceOfType(obj))
					{
						return obj;
					}
				}
			}
			return null;
		}
	}

	/// <summary>Appends an object to the end of the stack, rather than pushing it onto the top of the stack.</summary>
	/// <param name="context">A context object to append to the stack.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="context" /> is null.</exception>
	public void Append(object context)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (contextStack == null)
		{
			contextStack = new ArrayList();
		}
		contextStack.Insert(0, context);
	}

	/// <summary>Removes the current object off of the stack, returning its value.</summary>
	/// <returns>The object removed from the stack; null if no objects are on the stack.</returns>
	public object Pop()
	{
		object result = null;
		if (contextStack != null && contextStack.Count > 0)
		{
			int index = contextStack.Count - 1;
			result = contextStack[index];
			contextStack.RemoveAt(index);
		}
		return result;
	}

	/// <summary>Pushes, or places, the specified object onto the stack.</summary>
	/// <param name="context">The context object to push onto the stack. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="context" /> is null.</exception>
	public void Push(object context)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (contextStack == null)
		{
			contextStack = new ArrayList();
		}
		contextStack.Add(context);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.Serialization.ContextStack" /> class. </summary>
	public ContextStack()
	{
	}
}
