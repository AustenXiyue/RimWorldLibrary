using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Represents a single composition operation for transactional composition.</summary>
public class AtomicComposition : IDisposable
{
	private readonly AtomicComposition _outerAtomicComposition;

	private KeyValuePair<object, object>[] _values;

	private int _valueCount;

	private List<Action> _completeActionList;

	private List<Action> _revertActionList;

	private bool _isDisposed;

	private bool _isCompleted;

	private bool _containsInnerAtomicComposition;

	private bool ContainsInnerAtomicComposition
	{
		set
		{
			if (value && _containsInnerAtomicComposition)
			{
				throw new InvalidOperationException(Strings.AtomicComposition_AlreadyNested);
			}
			_containsInnerAtomicComposition = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AtomicComposition" /> class. </summary>
	public AtomicComposition()
		: this(null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AtomicComposition" /> class with the specified parent <see cref="T:System.ComponentModel.Composition.Hosting.AtomicComposition" />.</summary>
	/// <param name="outerAtomicComposition">The parent of this composition operation.</param>
	public AtomicComposition(AtomicComposition outerAtomicComposition)
	{
		if (outerAtomicComposition != null)
		{
			_outerAtomicComposition = outerAtomicComposition;
			_outerAtomicComposition.ContainsInnerAtomicComposition = true;
		}
	}

	/// <summary>Saves a key-value pair in the transaction to track tentative state.</summary>
	/// <param name="key">The key to save.</param>
	/// <param name="value">The value to save.</param>
	public void SetValue(object key, object value)
	{
		ThrowIfDisposed();
		ThrowIfCompleted();
		ThrowIfContainsInnerAtomicComposition();
		Requires.NotNull(key, "key");
		SetValueInternal(key, value);
	}

	/// <summary>Gets a value saved by the <see cref="M:System.ComponentModel.Composition.Hosting.AtomicComposition.SetValue(System.Object,System.Object)" /> method.</summary>
	/// <returns>true if the value was successfully retrieved; otherwise, false.</returns>
	/// <param name="key">The key to retrieve from.</param>
	/// <param name="value">The retrieved value.</param>
	/// <typeparam name="T">The type of the value to be retrieved.</typeparam>
	public bool TryGetValue<T>(object key, out T value)
	{
		return TryGetValue<T>(key, localAtomicCompositionOnly: false, out value);
	}

	/// <summary>Gets a value saved by the <see cref="M:System.ComponentModel.Composition.Hosting.AtomicComposition.SetValue(System.Object,System.Object)" /> method, with the option of not searching parent transactions.</summary>
	/// <returns>true if the value was successfully retrieved; otherwise, false.</returns>
	/// <param name="key">The key to retrieve from.</param>
	/// <param name="localAtomicCompositionOnly">true to exclude parent transactions; otherwise, false.</param>
	/// <param name="value">The retrieved value.</param>
	/// <typeparam name="T">The type of the value to be retrieved.</typeparam>
	public bool TryGetValue<T>(object key, bool localAtomicCompositionOnly, out T value)
	{
		ThrowIfDisposed();
		ThrowIfCompleted();
		Requires.NotNull(key, "key");
		return TryGetValueInternal<T>(key, localAtomicCompositionOnly, out value);
	}

	/// <summary>Adds an action to be executed when the overall composition operation completes successfully.</summary>
	/// <param name="completeAction">The action to be executed.</param>
	public void AddCompleteAction(Action completeAction)
	{
		ThrowIfDisposed();
		ThrowIfCompleted();
		ThrowIfContainsInnerAtomicComposition();
		Requires.NotNull(completeAction, "completeAction");
		if (_completeActionList == null)
		{
			_completeActionList = new List<Action>();
		}
		_completeActionList.Add(completeAction);
	}

	/// <summary>Adds an action to be executed if the overall composition operation fails.</summary>
	/// <param name="revertAction">The action to be executed.</param>
	public void AddRevertAction(Action revertAction)
	{
		ThrowIfDisposed();
		ThrowIfCompleted();
		ThrowIfContainsInnerAtomicComposition();
		Requires.NotNull(revertAction, "revertAction");
		if (_revertActionList == null)
		{
			_revertActionList = new List<Action>();
		}
		_revertActionList.Add(revertAction);
	}

	/// <summary>Marks this composition operation as complete.</summary>
	public void Complete()
	{
		ThrowIfDisposed();
		ThrowIfCompleted();
		if (_outerAtomicComposition == null)
		{
			FinalComplete();
		}
		else
		{
			CopyComplete();
		}
		_isCompleted = true;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AtomicComposition" /> class, and mark this composition operation as failed.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Composition.Hosting.AtomicComposition" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		ThrowIfDisposed();
		_isDisposed = true;
		if (_outerAtomicComposition != null)
		{
			_outerAtomicComposition.ContainsInnerAtomicComposition = false;
		}
		if (!_isCompleted && _revertActionList != null)
		{
			for (int num = _revertActionList.Count - 1; num >= 0; num--)
			{
				_revertActionList[num]();
			}
			_revertActionList = null;
		}
	}

	private void FinalComplete()
	{
		if (_completeActionList == null)
		{
			return;
		}
		foreach (Action completeAction in _completeActionList)
		{
			completeAction();
		}
		_completeActionList = null;
	}

	private void CopyComplete()
	{
		Assumes.NotNull(_outerAtomicComposition);
		_outerAtomicComposition.ContainsInnerAtomicComposition = false;
		if (_completeActionList != null)
		{
			foreach (Action completeAction in _completeActionList)
			{
				_outerAtomicComposition.AddCompleteAction(completeAction);
			}
		}
		if (_revertActionList != null)
		{
			foreach (Action revertAction in _revertActionList)
			{
				_outerAtomicComposition.AddRevertAction(revertAction);
			}
		}
		for (int i = 0; i < _valueCount; i++)
		{
			_outerAtomicComposition.SetValueInternal(_values[i].Key, _values[i].Value);
		}
	}

	private bool TryGetValueInternal<T>(object key, bool localAtomicCompositionOnly, out T value)
	{
		for (int i = 0; i < _valueCount; i++)
		{
			if (_values[i].Key == key)
			{
				value = (T)_values[i].Value;
				return true;
			}
		}
		if (!localAtomicCompositionOnly && _outerAtomicComposition != null)
		{
			return _outerAtomicComposition.TryGetValueInternal<T>(key, localAtomicCompositionOnly, out value);
		}
		value = default(T);
		return false;
	}

	private void SetValueInternal(object key, object value)
	{
		for (int i = 0; i < _valueCount; i++)
		{
			if (_values[i].Key == key)
			{
				_values[i] = new KeyValuePair<object, object>(key, value);
				return;
			}
		}
		if (_values == null || _valueCount == _values.Length)
		{
			KeyValuePair<object, object>[] array = new KeyValuePair<object, object>[(_valueCount == 0) ? 5 : (_valueCount * 2)];
			if (_values != null)
			{
				Array.Copy(_values, array, _valueCount);
			}
			_values = array;
		}
		_values[_valueCount] = new KeyValuePair<object, object>(key, value);
		_valueCount++;
	}

	[DebuggerStepThrough]
	private void ThrowIfContainsInnerAtomicComposition()
	{
		if (_containsInnerAtomicComposition)
		{
			throw new InvalidOperationException(Strings.AtomicComposition_PartOfAnotherAtomicComposition);
		}
	}

	[DebuggerStepThrough]
	private void ThrowIfCompleted()
	{
		if (_isCompleted)
		{
			throw new InvalidOperationException(Strings.AtomicComposition_AlreadyCompleted);
		}
	}

	[DebuggerStepThrough]
	private void ThrowIfDisposed()
	{
		if (_isDisposed)
		{
			throw ExceptionBuilder.CreateObjectDisposed(this);
		}
	}
}
