namespace System.CodeDom;

/// <summary>Represents a statement that removes an event handler.</summary>
[Serializable]
public class CodeRemoveEventStatement : CodeStatement
{
	private CodeEventReferenceExpression _eventRef;

	/// <summary>Gets or sets the event to remove a listener from.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeEventReferenceExpression" /> that indicates the event to remove a listener from.</returns>
	public CodeEventReferenceExpression Event
	{
		get
		{
			return _eventRef ?? (_eventRef = new CodeEventReferenceExpression());
		}
		set
		{
			_eventRef = value;
		}
	}

	/// <summary>Gets or sets the event handler to remove.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the event handler to remove.</returns>
	public CodeExpression Listener { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeRemoveEventStatement" /> class.</summary>
	public CodeRemoveEventStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeRemoveEventStatement" /> class with the specified event and event handler.</summary>
	/// <param name="eventRef">A <see cref="T:System.CodeDom.CodeEventReferenceExpression" /> that indicates the event to detach the event handler from. </param>
	/// <param name="listener">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the event handler to remove. </param>
	public CodeRemoveEventStatement(CodeEventReferenceExpression eventRef, CodeExpression listener)
	{
		_eventRef = eventRef;
		Listener = listener;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeRemoveEventStatement" /> class using the specified target object, event name, and event handler.</summary>
	/// <param name="targetObject">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the object that contains the event. </param>
	/// <param name="eventName">The name of the event. </param>
	/// <param name="listener">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the event handler to remove. </param>
	public CodeRemoveEventStatement(CodeExpression targetObject, string eventName, CodeExpression listener)
	{
		_eventRef = new CodeEventReferenceExpression(targetObject, eventName);
		Listener = listener;
	}
}
