namespace System.Xml.Xsl.XsltOld;

internal class BuiltInRuleTextAction : Action
{
	private const int ResultStored = 2;

	internal override void Execute(Processor processor, ActionFrame frame)
	{
		switch (frame.State)
		{
		case 0:
		{
			string text = processor.ValueOf(frame.NodeSet.Current);
			if (processor.TextEvent(text, disableOutputEscaping: false))
			{
				frame.Finished();
				break;
			}
			frame.StoredOutput = text;
			frame.State = 2;
			break;
		}
		case 2:
			processor.TextEvent(frame.StoredOutput);
			frame.Finished();
			break;
		}
	}
}
