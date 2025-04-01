namespace System.CodeDom;

/// <summary>Represents a statement that attaches an event-handler delegate to an event.</summary>
[Serializable]
public class CodeAttachEventStatement : CodeStatement
{
	private CodeEventReferenceExpression _eventRef;

	/// <summary>Gets or sets the event to attach an event-handler delegate to.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeEventReferenceExpression" /> that indicates the event to attach an event handler to.</returns>
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

	/// <summary>Gets or sets the new event-handler delegate to attach to the event.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the new event handler to attach.</returns>
	public CodeExpression Listener { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeAttachEventStatement" /> class.</summary>
	public CodeAttachEventStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeAttachEventStatement" /> class using the specified event and delegate.</summary>
	/// <param name="eventRef">A <see cref="T:System.CodeDom.CodeEventReferenceExpression" /> that indicates the event to attach an event handler to. </param>
	/// <param name="listener">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the new event handler. </param>
	public CodeAttachEventStatement(CodeEventReferenceExpression eventRef, CodeExpression listener)
	{
		_eventRef = eventRef;
		Listener = listener;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeAttachEventStatement" /> class using the specified object containing the event, event name, and event-handler delegate.</summary>
	/// <param name="targetObject">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the object that contains the event. </param>
	/// <param name="eventName">The name of the event to attach an event handler to. </param>
	/// <param name="listener">A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the new event handler. </param>
	public CodeAttachEventStatement(CodeExpression targetObject, string eventName, CodeExpression listener)
		: this(new CodeEventReferenceExpression(targetObject, eventName), listener)
	{
	}
}
