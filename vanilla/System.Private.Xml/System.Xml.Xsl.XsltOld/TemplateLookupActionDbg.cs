namespace System.Xml.Xsl.XsltOld;

internal sealed class TemplateLookupActionDbg : TemplateLookupAction
{
	internal override void Execute(Processor processor, ActionFrame frame)
	{
		if (mode == Compiler.BuiltInMode)
		{
			mode = processor.GetPreviousMode();
		}
		processor.SetCurrentMode(mode);
		Action action = ((!(mode != null)) ? ((importsOf == null) ? processor.Stylesheet.FindTemplate(processor, frame.Node) : importsOf.FindTemplateImports(processor, frame.Node)) : ((importsOf == null) ? processor.Stylesheet.FindTemplate(processor, frame.Node, mode) : importsOf.FindTemplateImports(processor, frame.Node, mode)));
		if (action == null)
		{
			action = BuiltInTemplate(frame.Node);
		}
		if (action != null)
		{
			frame.SetAction(action);
		}
		else
		{
			frame.Finished();
		}
	}
}
