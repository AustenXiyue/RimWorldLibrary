using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace MS.Internal.Xml.XPath;

internal sealed class NumericExpr : ValueQuery
{
	private Operator.Op op;

	private Query opnd1;

	private Query opnd2;

	public override XPathResultType StaticType => XPathResultType.Number;

	public NumericExpr(Operator.Op op, Query opnd1, Query opnd2)
	{
		if (opnd1.StaticType != 0)
		{
			opnd1 = new NumberFunctions(Function.FunctionType.FuncNumber, opnd1);
		}
		if (opnd2.StaticType != 0)
		{
			opnd2 = new NumberFunctions(Function.FunctionType.FuncNumber, opnd2);
		}
		this.op = op;
		this.opnd1 = opnd1;
		this.opnd2 = opnd2;
	}

	private NumericExpr(NumericExpr other)
		: base(other)
	{
		op = other.op;
		opnd1 = Query.Clone(other.opnd1);
		opnd2 = Query.Clone(other.opnd2);
	}

	public override void SetXsltContext(XsltContext context)
	{
		opnd1.SetXsltContext(context);
		opnd2.SetXsltContext(context);
	}

	public override object Evaluate(XPathNodeIterator nodeIterator)
	{
		return GetValue(op, XmlConvert.ToXPathDouble(opnd1.Evaluate(nodeIterator)), XmlConvert.ToXPathDouble(opnd2.Evaluate(nodeIterator)));
	}

	private static double GetValue(Operator.Op op, double n1, double n2)
	{
		return op switch
		{
			Operator.Op.PLUS => n1 + n2, 
			Operator.Op.MINUS => n1 - n2, 
			Operator.Op.MOD => n1 % n2, 
			Operator.Op.DIV => n1 / n2, 
			Operator.Op.MUL => n1 * n2, 
			_ => 0.0, 
		};
	}

	public override XPathNodeIterator Clone()
	{
		return new NumericExpr(this);
	}

	public override void PrintQuery(XmlWriter w)
	{
		w.WriteStartElement(GetType().Name);
		w.WriteAttributeString("op", op.ToString());
		opnd1.PrintQuery(w);
		opnd2.PrintQuery(w);
		w.WriteEndElement();
	}
}
