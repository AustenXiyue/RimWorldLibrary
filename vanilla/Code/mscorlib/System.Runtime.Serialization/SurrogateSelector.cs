using System.Runtime.InteropServices;
using System.Security;

namespace System.Runtime.Serialization;

/// <summary>Assists formatters in selection of the serialization surrogate to delegate the serialization or deserialization process to.</summary>
[ComVisible(true)]
public class SurrogateSelector : ISurrogateSelector
{
	internal SurrogateHashtable m_surrogates;

	internal ISurrogateSelector m_nextSelector;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Serialization.SurrogateSelector" /> class.</summary>
	public SurrogateSelector()
	{
		m_surrogates = new SurrogateHashtable(32);
	}

	/// <summary>Adds a surrogate to the list of checked surrogates.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> for which the surrogate is required.</param>
	/// <param name="context">The context-specific data. </param>
	/// <param name="surrogate">The surrogate to call for this type. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="type" /> or <paramref name="surrogate" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">A surrogate already exists for this type and context. </exception>
	public virtual void AddSurrogate(Type type, StreamingContext context, ISerializationSurrogate surrogate)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (surrogate == null)
		{
			throw new ArgumentNullException("surrogate");
		}
		SurrogateKey key = new SurrogateKey(type, context);
		m_surrogates.Add(key, surrogate);
	}

	[SecurityCritical]
	private static bool HasCycle(ISurrogateSelector selector)
	{
		ISurrogateSelector surrogateSelector = selector;
		ISurrogateSelector surrogateSelector2 = selector;
		while (surrogateSelector != null)
		{
			surrogateSelector = surrogateSelector.GetNextSelector();
			if (surrogateSelector == null)
			{
				return true;
			}
			if (surrogateSelector == surrogateSelector2)
			{
				return false;
			}
			surrogateSelector = surrogateSelector.GetNextSelector();
			surrogateSelector2 = surrogateSelector2.GetNextSelector();
			if (surrogateSelector == surrogateSelector2)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Adds the specified <see cref="T:System.Runtime.Serialization.ISurrogateSelector" /> that can handle a particular object type to the list of surrogates.</summary>
	/// <param name="selector">The surrogate selector to add. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="selector" /> parameter is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The selector is already on the list of selectors. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	[SecurityCritical]
	public virtual void ChainSelector(ISurrogateSelector selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (selector == this)
		{
			throw new SerializationException(Environment.GetResourceString("Selector is already on the list of checked selectors."));
		}
		if (!HasCycle(selector))
		{
			throw new ArgumentException(Environment.GetResourceString("Selector contained a cycle."), "selector");
		}
		ISurrogateSelector nextSelector = selector.GetNextSelector();
		ISurrogateSelector surrogateSelector = selector;
		while (nextSelector != null && nextSelector != this)
		{
			surrogateSelector = nextSelector;
			nextSelector = nextSelector.GetNextSelector();
		}
		if (nextSelector == this)
		{
			throw new ArgumentException(Environment.GetResourceString("Adding selector will introduce a cycle."), "selector");
		}
		nextSelector = selector;
		ISurrogateSelector surrogateSelector2 = selector;
		while (nextSelector != null)
		{
			nextSelector = ((nextSelector != surrogateSelector) ? nextSelector.GetNextSelector() : GetNextSelector());
			if (nextSelector == null)
			{
				break;
			}
			if (nextSelector == surrogateSelector2)
			{
				throw new ArgumentException(Environment.GetResourceString("Adding selector will introduce a cycle."), "selector");
			}
			nextSelector = ((nextSelector != surrogateSelector) ? nextSelector.GetNextSelector() : GetNextSelector());
			surrogateSelector2 = ((surrogateSelector2 != surrogateSelector) ? surrogateSelector2.GetNextSelector() : GetNextSelector());
			if (nextSelector == surrogateSelector2)
			{
				throw new ArgumentException(Environment.GetResourceString("Adding selector will introduce a cycle."), "selector");
			}
		}
		ISurrogateSelector nextSelector2 = m_nextSelector;
		m_nextSelector = selector;
		if (nextSelector2 != null)
		{
			surrogateSelector.ChainSelector(nextSelector2);
		}
	}

	/// <summary>Returns the next selector on the chain of selectors.</summary>
	/// <returns>The next <see cref="T:System.Runtime.Serialization.ISurrogateSelector" /> on the chain of selectors.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	[SecurityCritical]
	public virtual ISurrogateSelector GetNextSelector()
	{
		return m_nextSelector;
	}

	/// <summary>Returns the surrogate for a particular type.</summary>
	/// <returns>The surrogate for a particular type.</returns>
	/// <param name="type">The <see cref="T:System.Type" /> for which the surrogate is requested. </param>
	/// <param name="context">The streaming context. </param>
	/// <param name="selector">The surrogate to use. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="type" /> parameter is null. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	/// </PermissionSet>
	[SecurityCritical]
	public virtual ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		selector = this;
		SurrogateKey key = new SurrogateKey(type, context);
		ISerializationSurrogate serializationSurrogate = (ISerializationSurrogate)m_surrogates[key];
		if (serializationSurrogate != null)
		{
			return serializationSurrogate;
		}
		if (m_nextSelector != null)
		{
			return m_nextSelector.GetSurrogate(type, context, out selector);
		}
		return null;
	}

	/// <summary>Removes the surrogate associated with a given type.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> for which to remove the surrogate. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> for the current surrogate. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="type" /> parameter is null. </exception>
	public virtual void RemoveSurrogate(Type type, StreamingContext context)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		SurrogateKey key = new SurrogateKey(type, context);
		m_surrogates.Remove(key);
	}
}
