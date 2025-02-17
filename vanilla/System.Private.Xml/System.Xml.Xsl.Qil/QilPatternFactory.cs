using System.Collections.Generic;
using System.Reflection;

namespace System.Xml.Xsl.Qil;

internal class QilPatternFactory
{
	private readonly bool _debug;

	private readonly QilFactory _f;

	public QilFactory BaseFactory => _f;

	public QilPatternFactory(QilFactory f, bool debug)
	{
		_f = f;
		_debug = debug;
	}

	public QilLiteral String(string val)
	{
		return _f.LiteralString(val);
	}

	public QilLiteral Int32(int val)
	{
		return _f.LiteralInt32(val);
	}

	public QilLiteral Double(double val)
	{
		return _f.LiteralDouble(val);
	}

	public QilName QName(string local, string uri, string prefix)
	{
		return _f.LiteralQName(local, uri, prefix);
	}

	public QilName QName(string local, string uri)
	{
		return _f.LiteralQName(local, uri, string.Empty);
	}

	public QilName QName(string local)
	{
		return _f.LiteralQName(local, string.Empty, string.Empty);
	}

	public QilNode Unknown(XmlQueryType t)
	{
		return _f.Unknown(t);
	}

	public QilExpression QilExpression(QilNode root, QilFactory factory)
	{
		return _f.QilExpression(root, factory);
	}

	public QilList FunctionList()
	{
		return _f.FunctionList();
	}

	public QilList GlobalVariableList()
	{
		return _f.GlobalVariableList();
	}

	public QilList GlobalParameterList()
	{
		return _f.GlobalParameterList();
	}

	public QilList ActualParameterList()
	{
		return _f.ActualParameterList();
	}

	public QilList ActualParameterList(QilNode arg1, QilNode arg2)
	{
		QilList qilList = _f.ActualParameterList();
		qilList.Add(arg1);
		qilList.Add(arg2);
		return qilList;
	}

	public QilList ActualParameterList(params QilNode[] args)
	{
		return _f.ActualParameterList(args);
	}

	public QilList FormalParameterList()
	{
		return _f.FormalParameterList();
	}

	public QilList FormalParameterList(QilNode arg1, QilNode arg2)
	{
		QilList qilList = _f.FormalParameterList();
		qilList.Add(arg1);
		qilList.Add(arg2);
		return qilList;
	}

	public QilList FormalParameterList(params QilNode[] args)
	{
		return _f.FormalParameterList(args);
	}

	public QilList BranchList(params QilNode[] args)
	{
		return _f.BranchList(args);
	}

	public QilNode OptimizeBarrier(QilNode child)
	{
		return _f.OptimizeBarrier(child);
	}

	public QilNode DataSource(QilNode name, QilNode baseUri)
	{
		return _f.DataSource(name, baseUri);
	}

	public QilNode Nop(QilNode child)
	{
		return _f.Nop(child);
	}

	public QilNode Error(QilNode text)
	{
		return _f.Error(text);
	}

	public QilNode Warning(QilNode text)
	{
		return _f.Warning(text);
	}

	public QilIterator For(QilNode binding)
	{
		return _f.For(binding);
	}

	public QilIterator Let(QilNode binding)
	{
		return _f.Let(binding);
	}

	public QilParameter Parameter(XmlQueryType t)
	{
		return _f.Parameter(t);
	}

	public QilParameter Parameter(QilNode defaultValue, QilName name, XmlQueryType t)
	{
		return _f.Parameter(defaultValue, name, t);
	}

	public QilNode PositionOf(QilIterator expr)
	{
		return _f.PositionOf(expr);
	}

	public QilNode True()
	{
		return _f.True();
	}

	public QilNode False()
	{
		return _f.False();
	}

	public QilNode Boolean(bool b)
	{
		if (!b)
		{
			return False();
		}
		return True();
	}

	private static void CheckLogicArg(QilNode arg)
	{
	}

	public QilNode And(QilNode left, QilNode right)
	{
		CheckLogicArg(left);
		CheckLogicArg(right);
		if (!_debug)
		{
			if (left.NodeType == QilNodeType.True || right.NodeType == QilNodeType.False)
			{
				return right;
			}
			if (left.NodeType == QilNodeType.False || right.NodeType == QilNodeType.True)
			{
				return left;
			}
		}
		return _f.And(left, right);
	}

	public QilNode Or(QilNode left, QilNode right)
	{
		CheckLogicArg(left);
		CheckLogicArg(right);
		if (!_debug)
		{
			if (left.NodeType == QilNodeType.True || right.NodeType == QilNodeType.False)
			{
				return left;
			}
			if (left.NodeType == QilNodeType.False || right.NodeType == QilNodeType.True)
			{
				return right;
			}
		}
		return _f.Or(left, right);
	}

	public QilNode Not(QilNode child)
	{
		if (!_debug)
		{
			switch (child.NodeType)
			{
			case QilNodeType.True:
				return _f.False();
			case QilNodeType.False:
				return _f.True();
			case QilNodeType.Not:
				return ((QilUnary)child).Child;
			}
		}
		return _f.Not(child);
	}

	public QilNode Conditional(QilNode condition, QilNode trueBranch, QilNode falseBranch)
	{
		if (!_debug)
		{
			switch (condition.NodeType)
			{
			case QilNodeType.True:
				return trueBranch;
			case QilNodeType.False:
				return falseBranch;
			case QilNodeType.Not:
				return Conditional(((QilUnary)condition).Child, falseBranch, trueBranch);
			}
		}
		return _f.Conditional(condition, trueBranch, falseBranch);
	}

	public QilNode Choice(QilNode expr, QilList branches)
	{
		if (!_debug)
		{
			switch (branches.Count)
			{
			case 1:
				return _f.Loop(_f.Let(expr), branches[0]);
			case 2:
				return _f.Conditional(_f.Eq(expr, _f.LiteralInt32(0)), branches[0], branches[1]);
			}
		}
		return _f.Choice(expr, branches);
	}

	public QilNode Length(QilNode child)
	{
		return _f.Length(child);
	}

	public QilNode Sequence()
	{
		return _f.Sequence();
	}

	public QilNode Sequence(QilNode child)
	{
		if (!_debug)
		{
			return child;
		}
		QilList qilList = _f.Sequence();
		qilList.Add(child);
		return qilList;
	}

	public QilNode Sequence(QilNode child1, QilNode child2)
	{
		QilList qilList = _f.Sequence();
		qilList.Add(child1);
		qilList.Add(child2);
		return qilList;
	}

	public QilNode Sequence(params QilNode[] args)
	{
		if (!_debug)
		{
			switch (args.Length)
			{
			case 0:
				return _f.Sequence();
			case 1:
				return args[0];
			}
		}
		QilList qilList = _f.Sequence();
		foreach (QilNode node in args)
		{
			qilList.Add(node);
		}
		return qilList;
	}

	public QilNode Union(QilNode left, QilNode right)
	{
		return _f.Union(left, right);
	}

	public QilNode Sum(QilNode collection)
	{
		return _f.Sum(collection);
	}

	public QilNode Negate(QilNode child)
	{
		return _f.Negate(child);
	}

	public QilNode Add(QilNode left, QilNode right)
	{
		return _f.Add(left, right);
	}

	public QilNode Subtract(QilNode left, QilNode right)
	{
		return _f.Subtract(left, right);
	}

	public QilNode Multiply(QilNode left, QilNode right)
	{
		return _f.Multiply(left, right);
	}

	public QilNode Divide(QilNode left, QilNode right)
	{
		return _f.Divide(left, right);
	}

	public QilNode Modulo(QilNode left, QilNode right)
	{
		return _f.Modulo(left, right);
	}

	public QilNode StrLength(QilNode str)
	{
		return _f.StrLength(str);
	}

	public QilNode StrConcat(QilNode values)
	{
		if (!_debug && values.XmlType.IsSingleton)
		{
			return values;
		}
		return _f.StrConcat(values);
	}

	public QilNode StrConcat(params QilNode[] args)
	{
		return StrConcat((IList<QilNode>)args);
	}

	public QilNode StrConcat(IList<QilNode> args)
	{
		if (!_debug)
		{
			switch (args.Count)
			{
			case 0:
				return _f.LiteralString(string.Empty);
			case 1:
				return StrConcat(args[0]);
			}
		}
		return StrConcat(_f.Sequence(args));
	}

	public QilNode StrParseQName(QilNode str, QilNode ns)
	{
		return _f.StrParseQName(str, ns);
	}

	public QilNode Ne(QilNode left, QilNode right)
	{
		return _f.Ne(left, right);
	}

	public QilNode Eq(QilNode left, QilNode right)
	{
		return _f.Eq(left, right);
	}

	public QilNode Gt(QilNode left, QilNode right)
	{
		return _f.Gt(left, right);
	}

	public QilNode Ge(QilNode left, QilNode right)
	{
		return _f.Ge(left, right);
	}

	public QilNode Lt(QilNode left, QilNode right)
	{
		return _f.Lt(left, right);
	}

	public QilNode Le(QilNode left, QilNode right)
	{
		return _f.Le(left, right);
	}

	public QilNode Is(QilNode left, QilNode right)
	{
		return _f.Is(left, right);
	}

	public QilNode Before(QilNode left, QilNode right)
	{
		return _f.Before(left, right);
	}

	public QilNode Loop(QilIterator variable, QilNode body)
	{
		if (!_debug && body == variable.Binding)
		{
			return body;
		}
		return _f.Loop(variable, body);
	}

	public QilNode Filter(QilIterator variable, QilNode expr)
	{
		if (!_debug && expr.NodeType == QilNodeType.True)
		{
			return variable.Binding;
		}
		return _f.Filter(variable, expr);
	}

	public QilNode Sort(QilIterator iter, QilNode keys)
	{
		return _f.Sort(iter, keys);
	}

	public QilSortKey SortKey(QilNode key, QilNode collation)
	{
		return _f.SortKey(key, collation);
	}

	public QilNode DocOrderDistinct(QilNode collection)
	{
		if (collection.NodeType == QilNodeType.DocOrderDistinct)
		{
			return collection;
		}
		return _f.DocOrderDistinct(collection);
	}

	public QilFunction Function(QilList args, QilNode sideEffects, XmlQueryType resultType)
	{
		return _f.Function(args, sideEffects, resultType);
	}

	public QilFunction Function(QilList args, QilNode defn, QilNode sideEffects)
	{
		return _f.Function(args, defn, sideEffects, defn.XmlType);
	}

	public QilNode Invoke(QilFunction func, QilList args)
	{
		return _f.Invoke(func, args);
	}

	public QilNode Content(QilNode context)
	{
		return _f.Content(context);
	}

	public QilNode Parent(QilNode context)
	{
		return _f.Parent(context);
	}

	public QilNode Root(QilNode context)
	{
		return _f.Root(context);
	}

	public QilNode XmlContext()
	{
		return _f.XmlContext();
	}

	public QilNode Descendant(QilNode expr)
	{
		return _f.Descendant(expr);
	}

	public QilNode DescendantOrSelf(QilNode context)
	{
		return _f.DescendantOrSelf(context);
	}

	public QilNode Ancestor(QilNode expr)
	{
		return _f.Ancestor(expr);
	}

	public QilNode AncestorOrSelf(QilNode expr)
	{
		return _f.AncestorOrSelf(expr);
	}

	public QilNode Preceding(QilNode expr)
	{
		return _f.Preceding(expr);
	}

	public QilNode FollowingSibling(QilNode expr)
	{
		return _f.FollowingSibling(expr);
	}

	public QilNode PrecedingSibling(QilNode expr)
	{
		return _f.PrecedingSibling(expr);
	}

	public QilNode NodeRange(QilNode left, QilNode right)
	{
		return _f.NodeRange(left, right);
	}

	public QilBinary Deref(QilNode context, QilNode id)
	{
		return _f.Deref(context, id);
	}

	public QilNode ElementCtor(QilNode name, QilNode content)
	{
		return _f.ElementCtor(name, content);
	}

	public QilNode AttributeCtor(QilNode name, QilNode val)
	{
		return _f.AttributeCtor(name, val);
	}

	public QilNode CommentCtor(QilNode content)
	{
		return _f.CommentCtor(content);
	}

	public QilNode PICtor(QilNode name, QilNode content)
	{
		return _f.PICtor(name, content);
	}

	public QilNode TextCtor(QilNode content)
	{
		return _f.TextCtor(content);
	}

	public QilNode RawTextCtor(QilNode content)
	{
		return _f.RawTextCtor(content);
	}

	public QilNode DocumentCtor(QilNode child)
	{
		return _f.DocumentCtor(child);
	}

	public QilNode NamespaceDecl(QilNode prefix, QilNode uri)
	{
		return _f.NamespaceDecl(prefix, uri);
	}

	public QilNode RtfCtor(QilNode content, QilNode baseUri)
	{
		return _f.RtfCtor(content, baseUri);
	}

	public QilNode NameOf(QilNode expr)
	{
		return _f.NameOf(expr);
	}

	public QilNode LocalNameOf(QilNode expr)
	{
		return _f.LocalNameOf(expr);
	}

	public QilNode NamespaceUriOf(QilNode expr)
	{
		return _f.NamespaceUriOf(expr);
	}

	public QilNode PrefixOf(QilNode expr)
	{
		return _f.PrefixOf(expr);
	}

	public QilNode TypeAssert(QilNode expr, XmlQueryType t)
	{
		return _f.TypeAssert(expr, t);
	}

	public QilNode IsType(QilNode expr, XmlQueryType t)
	{
		return _f.IsType(expr, t);
	}

	public QilNode IsEmpty(QilNode set)
	{
		return _f.IsEmpty(set);
	}

	public QilNode XPathNodeValue(QilNode expr)
	{
		return _f.XPathNodeValue(expr);
	}

	public QilNode XPathFollowing(QilNode expr)
	{
		return _f.XPathFollowing(expr);
	}

	public QilNode XPathNamespace(QilNode expr)
	{
		return _f.XPathNamespace(expr);
	}

	public QilNode XPathPreceding(QilNode expr)
	{
		return _f.XPathPreceding(expr);
	}

	public QilNode XsltGenerateId(QilNode expr)
	{
		return _f.XsltGenerateId(expr);
	}

	public QilNode XsltInvokeEarlyBound(QilNode name, MethodInfo d, XmlQueryType t, IList<QilNode> args)
	{
		QilList qilList = _f.ActualParameterList();
		qilList.Add(args);
		return _f.XsltInvokeEarlyBound(name, _f.LiteralObject(d), qilList, t);
	}

	public QilNode XsltInvokeLateBound(QilNode name, IList<QilNode> args)
	{
		QilList qilList = _f.ActualParameterList();
		qilList.Add(args);
		return _f.XsltInvokeLateBound(name, qilList);
	}

	public QilNode XsltCopy(QilNode expr, QilNode content)
	{
		return _f.XsltCopy(expr, content);
	}

	public QilNode XsltCopyOf(QilNode expr)
	{
		return _f.XsltCopyOf(expr);
	}

	public QilNode XsltConvert(QilNode expr, XmlQueryType t)
	{
		return _f.XsltConvert(expr, t);
	}
}
