using System.Collections.Generic;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil;

internal sealed class QilFactory
{
	private QilTypeChecker typeCheck;

	public QilTypeChecker TypeChecker => typeCheck;

	public QilFactory()
	{
		typeCheck = new QilTypeChecker();
	}

	public QilExpression QilExpression(QilNode root, QilFactory factory)
	{
		QilExpression qilExpression = new QilExpression(QilNodeType.QilExpression, root, factory);
		qilExpression.XmlType = typeCheck.CheckQilExpression(qilExpression);
		return qilExpression;
	}

	public QilList FunctionList(IList<QilNode> values)
	{
		QilList qilList = FunctionList();
		qilList.Add(values);
		return qilList;
	}

	public QilList GlobalVariableList(IList<QilNode> values)
	{
		QilList qilList = GlobalVariableList();
		qilList.Add(values);
		return qilList;
	}

	public QilList GlobalParameterList(IList<QilNode> values)
	{
		QilList qilList = GlobalParameterList();
		qilList.Add(values);
		return qilList;
	}

	public QilList ActualParameterList(IList<QilNode> values)
	{
		QilList qilList = ActualParameterList();
		qilList.Add(values);
		return qilList;
	}

	public QilList FormalParameterList(IList<QilNode> values)
	{
		QilList qilList = FormalParameterList();
		qilList.Add(values);
		return qilList;
	}

	public QilList SortKeyList(IList<QilNode> values)
	{
		QilList qilList = SortKeyList();
		qilList.Add(values);
		return qilList;
	}

	public QilList BranchList(IList<QilNode> values)
	{
		QilList qilList = BranchList();
		qilList.Add(values);
		return qilList;
	}

	public QilList Sequence(IList<QilNode> values)
	{
		QilList qilList = Sequence();
		qilList.Add(values);
		return qilList;
	}

	public QilParameter Parameter(XmlQueryType xmlType)
	{
		return Parameter(null, null, xmlType);
	}

	public QilStrConcat StrConcat(QilNode values)
	{
		return StrConcat(LiteralString(""), values);
	}

	public QilName LiteralQName(string local)
	{
		return LiteralQName(local, string.Empty, string.Empty);
	}

	public QilTargetType TypeAssert(QilNode expr, XmlQueryType xmlType)
	{
		return TypeAssert(expr, (QilNode)LiteralType(xmlType));
	}

	public QilTargetType IsType(QilNode expr, XmlQueryType xmlType)
	{
		return IsType(expr, (QilNode)LiteralType(xmlType));
	}

	public QilTargetType XsltConvert(QilNode expr, XmlQueryType xmlType)
	{
		return XsltConvert(expr, (QilNode)LiteralType(xmlType));
	}

	public QilFunction Function(QilNode arguments, QilNode sideEffects, XmlQueryType xmlType)
	{
		return Function(arguments, Unknown(xmlType), sideEffects, xmlType);
	}

	public QilExpression QilExpression(QilNode root)
	{
		QilExpression qilExpression = new QilExpression(QilNodeType.QilExpression, root);
		qilExpression.XmlType = typeCheck.CheckQilExpression(qilExpression);
		return qilExpression;
	}

	public QilList FunctionList()
	{
		QilList qilList = new QilList(QilNodeType.FunctionList);
		qilList.XmlType = typeCheck.CheckFunctionList(qilList);
		return qilList;
	}

	public QilList GlobalVariableList()
	{
		QilList qilList = new QilList(QilNodeType.GlobalVariableList);
		qilList.XmlType = typeCheck.CheckGlobalVariableList(qilList);
		return qilList;
	}

	public QilList GlobalParameterList()
	{
		QilList qilList = new QilList(QilNodeType.GlobalParameterList);
		qilList.XmlType = typeCheck.CheckGlobalParameterList(qilList);
		return qilList;
	}

	public QilList ActualParameterList()
	{
		QilList qilList = new QilList(QilNodeType.ActualParameterList);
		qilList.XmlType = typeCheck.CheckActualParameterList(qilList);
		return qilList;
	}

	public QilList FormalParameterList()
	{
		QilList qilList = new QilList(QilNodeType.FormalParameterList);
		qilList.XmlType = typeCheck.CheckFormalParameterList(qilList);
		return qilList;
	}

	public QilList SortKeyList()
	{
		QilList qilList = new QilList(QilNodeType.SortKeyList);
		qilList.XmlType = typeCheck.CheckSortKeyList(qilList);
		return qilList;
	}

	public QilList BranchList()
	{
		QilList qilList = new QilList(QilNodeType.BranchList);
		qilList.XmlType = typeCheck.CheckBranchList(qilList);
		return qilList;
	}

	public QilUnary OptimizeBarrier(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.OptimizeBarrier, child);
		qilUnary.XmlType = typeCheck.CheckOptimizeBarrier(qilUnary);
		return qilUnary;
	}

	public QilNode Unknown(XmlQueryType xmlType)
	{
		QilNode qilNode = new QilNode(QilNodeType.Unknown, xmlType);
		qilNode.XmlType = typeCheck.CheckUnknown(qilNode);
		return qilNode;
	}

	public QilDataSource DataSource(QilNode name, QilNode baseUri)
	{
		QilDataSource qilDataSource = new QilDataSource(QilNodeType.DataSource, name, baseUri);
		qilDataSource.XmlType = typeCheck.CheckDataSource(qilDataSource);
		return qilDataSource;
	}

	public QilUnary Nop(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Nop, child);
		qilUnary.XmlType = typeCheck.CheckNop(qilUnary);
		return qilUnary;
	}

	public QilUnary Error(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Error, child);
		qilUnary.XmlType = typeCheck.CheckError(qilUnary);
		return qilUnary;
	}

	public QilUnary Warning(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Warning, child);
		qilUnary.XmlType = typeCheck.CheckWarning(qilUnary);
		return qilUnary;
	}

	public QilIterator For(QilNode binding)
	{
		QilIterator qilIterator = new QilIterator(QilNodeType.For, binding);
		qilIterator.XmlType = typeCheck.CheckFor(qilIterator);
		return qilIterator;
	}

	public QilIterator Let(QilNode binding)
	{
		QilIterator qilIterator = new QilIterator(QilNodeType.Let, binding);
		qilIterator.XmlType = typeCheck.CheckLet(qilIterator);
		return qilIterator;
	}

	public QilParameter Parameter(QilNode defaultValue, QilNode name, XmlQueryType xmlType)
	{
		QilParameter qilParameter = new QilParameter(QilNodeType.Parameter, defaultValue, name, xmlType);
		qilParameter.XmlType = typeCheck.CheckParameter(qilParameter);
		return qilParameter;
	}

	public QilUnary PositionOf(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.PositionOf, child);
		qilUnary.XmlType = typeCheck.CheckPositionOf(qilUnary);
		return qilUnary;
	}

	public QilNode True()
	{
		QilNode qilNode = new QilNode(QilNodeType.True);
		qilNode.XmlType = typeCheck.CheckTrue(qilNode);
		return qilNode;
	}

	public QilNode False()
	{
		QilNode qilNode = new QilNode(QilNodeType.False);
		qilNode.XmlType = typeCheck.CheckFalse(qilNode);
		return qilNode;
	}

	public QilLiteral LiteralString(string value)
	{
		QilLiteral qilLiteral = new QilLiteral(QilNodeType.LiteralString, value);
		qilLiteral.XmlType = typeCheck.CheckLiteralString(qilLiteral);
		return qilLiteral;
	}

	public QilLiteral LiteralInt32(int value)
	{
		QilLiteral qilLiteral = new QilLiteral(QilNodeType.LiteralInt32, value);
		qilLiteral.XmlType = typeCheck.CheckLiteralInt32(qilLiteral);
		return qilLiteral;
	}

	public QilLiteral LiteralInt64(long value)
	{
		QilLiteral qilLiteral = new QilLiteral(QilNodeType.LiteralInt64, value);
		qilLiteral.XmlType = typeCheck.CheckLiteralInt64(qilLiteral);
		return qilLiteral;
	}

	public QilLiteral LiteralDouble(double value)
	{
		QilLiteral qilLiteral = new QilLiteral(QilNodeType.LiteralDouble, value);
		qilLiteral.XmlType = typeCheck.CheckLiteralDouble(qilLiteral);
		return qilLiteral;
	}

	public QilLiteral LiteralDecimal(decimal value)
	{
		QilLiteral qilLiteral = new QilLiteral(QilNodeType.LiteralDecimal, value);
		qilLiteral.XmlType = typeCheck.CheckLiteralDecimal(qilLiteral);
		return qilLiteral;
	}

	public QilName LiteralQName(string localName, string namespaceUri, string prefix)
	{
		QilName qilName = new QilName(QilNodeType.LiteralQName, localName, namespaceUri, prefix);
		qilName.XmlType = typeCheck.CheckLiteralQName(qilName);
		return qilName;
	}

	public QilLiteral LiteralType(XmlQueryType value)
	{
		QilLiteral qilLiteral = new QilLiteral(QilNodeType.LiteralType, value);
		qilLiteral.XmlType = typeCheck.CheckLiteralType(qilLiteral);
		return qilLiteral;
	}

	public QilLiteral LiteralObject(object value)
	{
		QilLiteral qilLiteral = new QilLiteral(QilNodeType.LiteralObject, value);
		qilLiteral.XmlType = typeCheck.CheckLiteralObject(qilLiteral);
		return qilLiteral;
	}

	public QilBinary And(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.And, left, right);
		qilBinary.XmlType = typeCheck.CheckAnd(qilBinary);
		return qilBinary;
	}

	public QilBinary Or(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Or, left, right);
		qilBinary.XmlType = typeCheck.CheckOr(qilBinary);
		return qilBinary;
	}

	public QilUnary Not(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Not, child);
		qilUnary.XmlType = typeCheck.CheckNot(qilUnary);
		return qilUnary;
	}

	public QilTernary Conditional(QilNode left, QilNode center, QilNode right)
	{
		QilTernary qilTernary = new QilTernary(QilNodeType.Conditional, left, center, right);
		qilTernary.XmlType = typeCheck.CheckConditional(qilTernary);
		return qilTernary;
	}

	public QilChoice Choice(QilNode expression, QilNode branches)
	{
		QilChoice qilChoice = new QilChoice(QilNodeType.Choice, expression, branches);
		qilChoice.XmlType = typeCheck.CheckChoice(qilChoice);
		return qilChoice;
	}

	public QilUnary Length(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Length, child);
		qilUnary.XmlType = typeCheck.CheckLength(qilUnary);
		return qilUnary;
	}

	public QilList Sequence()
	{
		QilList qilList = new QilList(QilNodeType.Sequence);
		qilList.XmlType = typeCheck.CheckSequence(qilList);
		return qilList;
	}

	public QilBinary Union(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Union, left, right);
		qilBinary.XmlType = typeCheck.CheckUnion(qilBinary);
		return qilBinary;
	}

	public QilBinary Intersection(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Intersection, left, right);
		qilBinary.XmlType = typeCheck.CheckIntersection(qilBinary);
		return qilBinary;
	}

	public QilBinary Difference(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Difference, left, right);
		qilBinary.XmlType = typeCheck.CheckDifference(qilBinary);
		return qilBinary;
	}

	public QilUnary Average(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Average, child);
		qilUnary.XmlType = typeCheck.CheckAverage(qilUnary);
		return qilUnary;
	}

	public QilUnary Sum(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Sum, child);
		qilUnary.XmlType = typeCheck.CheckSum(qilUnary);
		return qilUnary;
	}

	public QilUnary Minimum(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Minimum, child);
		qilUnary.XmlType = typeCheck.CheckMinimum(qilUnary);
		return qilUnary;
	}

	public QilUnary Maximum(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Maximum, child);
		qilUnary.XmlType = typeCheck.CheckMaximum(qilUnary);
		return qilUnary;
	}

	public QilUnary Negate(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Negate, child);
		qilUnary.XmlType = typeCheck.CheckNegate(qilUnary);
		return qilUnary;
	}

	public QilBinary Add(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Add, left, right);
		qilBinary.XmlType = typeCheck.CheckAdd(qilBinary);
		return qilBinary;
	}

	public QilBinary Subtract(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Subtract, left, right);
		qilBinary.XmlType = typeCheck.CheckSubtract(qilBinary);
		return qilBinary;
	}

	public QilBinary Multiply(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Multiply, left, right);
		qilBinary.XmlType = typeCheck.CheckMultiply(qilBinary);
		return qilBinary;
	}

	public QilBinary Divide(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Divide, left, right);
		qilBinary.XmlType = typeCheck.CheckDivide(qilBinary);
		return qilBinary;
	}

	public QilBinary Modulo(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Modulo, left, right);
		qilBinary.XmlType = typeCheck.CheckModulo(qilBinary);
		return qilBinary;
	}

	public QilUnary StrLength(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.StrLength, child);
		qilUnary.XmlType = typeCheck.CheckStrLength(qilUnary);
		return qilUnary;
	}

	public QilStrConcat StrConcat(QilNode delimiter, QilNode values)
	{
		QilStrConcat qilStrConcat = new QilStrConcat(QilNodeType.StrConcat, delimiter, values);
		qilStrConcat.XmlType = typeCheck.CheckStrConcat(qilStrConcat);
		return qilStrConcat;
	}

	public QilBinary StrParseQName(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.StrParseQName, left, right);
		qilBinary.XmlType = typeCheck.CheckStrParseQName(qilBinary);
		return qilBinary;
	}

	public QilBinary Ne(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Ne, left, right);
		qilBinary.XmlType = typeCheck.CheckNe(qilBinary);
		return qilBinary;
	}

	public QilBinary Eq(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Eq, left, right);
		qilBinary.XmlType = typeCheck.CheckEq(qilBinary);
		return qilBinary;
	}

	public QilBinary Gt(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Gt, left, right);
		qilBinary.XmlType = typeCheck.CheckGt(qilBinary);
		return qilBinary;
	}

	public QilBinary Ge(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Ge, left, right);
		qilBinary.XmlType = typeCheck.CheckGe(qilBinary);
		return qilBinary;
	}

	public QilBinary Lt(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Lt, left, right);
		qilBinary.XmlType = typeCheck.CheckLt(qilBinary);
		return qilBinary;
	}

	public QilBinary Le(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Le, left, right);
		qilBinary.XmlType = typeCheck.CheckLe(qilBinary);
		return qilBinary;
	}

	public QilBinary Is(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Is, left, right);
		qilBinary.XmlType = typeCheck.CheckIs(qilBinary);
		return qilBinary;
	}

	public QilBinary After(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.After, left, right);
		qilBinary.XmlType = typeCheck.CheckAfter(qilBinary);
		return qilBinary;
	}

	public QilBinary Before(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Before, left, right);
		qilBinary.XmlType = typeCheck.CheckBefore(qilBinary);
		return qilBinary;
	}

	public QilLoop Loop(QilNode variable, QilNode body)
	{
		QilLoop qilLoop = new QilLoop(QilNodeType.Loop, variable, body);
		qilLoop.XmlType = typeCheck.CheckLoop(qilLoop);
		return qilLoop;
	}

	public QilLoop Filter(QilNode variable, QilNode body)
	{
		QilLoop qilLoop = new QilLoop(QilNodeType.Filter, variable, body);
		qilLoop.XmlType = typeCheck.CheckFilter(qilLoop);
		return qilLoop;
	}

	public QilLoop Sort(QilNode variable, QilNode body)
	{
		QilLoop qilLoop = new QilLoop(QilNodeType.Sort, variable, body);
		qilLoop.XmlType = typeCheck.CheckSort(qilLoop);
		return qilLoop;
	}

	public QilSortKey SortKey(QilNode key, QilNode collation)
	{
		QilSortKey qilSortKey = new QilSortKey(QilNodeType.SortKey, key, collation);
		qilSortKey.XmlType = typeCheck.CheckSortKey(qilSortKey);
		return qilSortKey;
	}

	public QilUnary DocOrderDistinct(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.DocOrderDistinct, child);
		qilUnary.XmlType = typeCheck.CheckDocOrderDistinct(qilUnary);
		return qilUnary;
	}

	public QilFunction Function(QilNode arguments, QilNode definition, QilNode sideEffects, XmlQueryType xmlType)
	{
		QilFunction qilFunction = new QilFunction(QilNodeType.Function, arguments, definition, sideEffects, xmlType);
		qilFunction.XmlType = typeCheck.CheckFunction(qilFunction);
		return qilFunction;
	}

	public QilInvoke Invoke(QilNode function, QilNode arguments)
	{
		QilInvoke qilInvoke = new QilInvoke(QilNodeType.Invoke, function, arguments);
		qilInvoke.XmlType = typeCheck.CheckInvoke(qilInvoke);
		return qilInvoke;
	}

	public QilUnary Content(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Content, child);
		qilUnary.XmlType = typeCheck.CheckContent(qilUnary);
		return qilUnary;
	}

	public QilBinary Attribute(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Attribute, left, right);
		qilBinary.XmlType = typeCheck.CheckAttribute(qilBinary);
		return qilBinary;
	}

	public QilUnary Parent(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Parent, child);
		qilUnary.XmlType = typeCheck.CheckParent(qilUnary);
		return qilUnary;
	}

	public QilUnary Root(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Root, child);
		qilUnary.XmlType = typeCheck.CheckRoot(qilUnary);
		return qilUnary;
	}

	public QilNode XmlContext()
	{
		QilNode qilNode = new QilNode(QilNodeType.XmlContext);
		qilNode.XmlType = typeCheck.CheckXmlContext(qilNode);
		return qilNode;
	}

	public QilUnary Descendant(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Descendant, child);
		qilUnary.XmlType = typeCheck.CheckDescendant(qilUnary);
		return qilUnary;
	}

	public QilUnary DescendantOrSelf(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.DescendantOrSelf, child);
		qilUnary.XmlType = typeCheck.CheckDescendantOrSelf(qilUnary);
		return qilUnary;
	}

	public QilUnary Ancestor(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Ancestor, child);
		qilUnary.XmlType = typeCheck.CheckAncestor(qilUnary);
		return qilUnary;
	}

	public QilUnary AncestorOrSelf(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.AncestorOrSelf, child);
		qilUnary.XmlType = typeCheck.CheckAncestorOrSelf(qilUnary);
		return qilUnary;
	}

	public QilUnary Preceding(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.Preceding, child);
		qilUnary.XmlType = typeCheck.CheckPreceding(qilUnary);
		return qilUnary;
	}

	public QilUnary FollowingSibling(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.FollowingSibling, child);
		qilUnary.XmlType = typeCheck.CheckFollowingSibling(qilUnary);
		return qilUnary;
	}

	public QilUnary PrecedingSibling(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.PrecedingSibling, child);
		qilUnary.XmlType = typeCheck.CheckPrecedingSibling(qilUnary);
		return qilUnary;
	}

	public QilBinary NodeRange(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.NodeRange, left, right);
		qilBinary.XmlType = typeCheck.CheckNodeRange(qilBinary);
		return qilBinary;
	}

	public QilBinary Deref(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.Deref, left, right);
		qilBinary.XmlType = typeCheck.CheckDeref(qilBinary);
		return qilBinary;
	}

	public QilBinary ElementCtor(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.ElementCtor, left, right);
		qilBinary.XmlType = typeCheck.CheckElementCtor(qilBinary);
		return qilBinary;
	}

	public QilBinary AttributeCtor(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.AttributeCtor, left, right);
		qilBinary.XmlType = typeCheck.CheckAttributeCtor(qilBinary);
		return qilBinary;
	}

	public QilUnary CommentCtor(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.CommentCtor, child);
		qilUnary.XmlType = typeCheck.CheckCommentCtor(qilUnary);
		return qilUnary;
	}

	public QilBinary PICtor(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.PICtor, left, right);
		qilBinary.XmlType = typeCheck.CheckPICtor(qilBinary);
		return qilBinary;
	}

	public QilUnary TextCtor(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.TextCtor, child);
		qilUnary.XmlType = typeCheck.CheckTextCtor(qilUnary);
		return qilUnary;
	}

	public QilUnary RawTextCtor(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.RawTextCtor, child);
		qilUnary.XmlType = typeCheck.CheckRawTextCtor(qilUnary);
		return qilUnary;
	}

	public QilUnary DocumentCtor(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.DocumentCtor, child);
		qilUnary.XmlType = typeCheck.CheckDocumentCtor(qilUnary);
		return qilUnary;
	}

	public QilBinary NamespaceDecl(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.NamespaceDecl, left, right);
		qilBinary.XmlType = typeCheck.CheckNamespaceDecl(qilBinary);
		return qilBinary;
	}

	public QilBinary RtfCtor(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.RtfCtor, left, right);
		qilBinary.XmlType = typeCheck.CheckRtfCtor(qilBinary);
		return qilBinary;
	}

	public QilUnary NameOf(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.NameOf, child);
		qilUnary.XmlType = typeCheck.CheckNameOf(qilUnary);
		return qilUnary;
	}

	public QilUnary LocalNameOf(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.LocalNameOf, child);
		qilUnary.XmlType = typeCheck.CheckLocalNameOf(qilUnary);
		return qilUnary;
	}

	public QilUnary NamespaceUriOf(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.NamespaceUriOf, child);
		qilUnary.XmlType = typeCheck.CheckNamespaceUriOf(qilUnary);
		return qilUnary;
	}

	public QilUnary PrefixOf(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.PrefixOf, child);
		qilUnary.XmlType = typeCheck.CheckPrefixOf(qilUnary);
		return qilUnary;
	}

	public QilTargetType TypeAssert(QilNode source, QilNode targetType)
	{
		QilTargetType qilTargetType = new QilTargetType(QilNodeType.TypeAssert, source, targetType);
		qilTargetType.XmlType = typeCheck.CheckTypeAssert(qilTargetType);
		return qilTargetType;
	}

	public QilTargetType IsType(QilNode source, QilNode targetType)
	{
		QilTargetType qilTargetType = new QilTargetType(QilNodeType.IsType, source, targetType);
		qilTargetType.XmlType = typeCheck.CheckIsType(qilTargetType);
		return qilTargetType;
	}

	public QilUnary IsEmpty(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.IsEmpty, child);
		qilUnary.XmlType = typeCheck.CheckIsEmpty(qilUnary);
		return qilUnary;
	}

	public QilUnary XPathNodeValue(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.XPathNodeValue, child);
		qilUnary.XmlType = typeCheck.CheckXPathNodeValue(qilUnary);
		return qilUnary;
	}

	public QilUnary XPathFollowing(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.XPathFollowing, child);
		qilUnary.XmlType = typeCheck.CheckXPathFollowing(qilUnary);
		return qilUnary;
	}

	public QilUnary XPathPreceding(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.XPathPreceding, child);
		qilUnary.XmlType = typeCheck.CheckXPathPreceding(qilUnary);
		return qilUnary;
	}

	public QilUnary XPathNamespace(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.XPathNamespace, child);
		qilUnary.XmlType = typeCheck.CheckXPathNamespace(qilUnary);
		return qilUnary;
	}

	public QilUnary XsltGenerateId(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.XsltGenerateId, child);
		qilUnary.XmlType = typeCheck.CheckXsltGenerateId(qilUnary);
		return qilUnary;
	}

	public QilInvokeLateBound XsltInvokeLateBound(QilNode name, QilNode arguments)
	{
		QilInvokeLateBound qilInvokeLateBound = new QilInvokeLateBound(QilNodeType.XsltInvokeLateBound, name, arguments);
		qilInvokeLateBound.XmlType = typeCheck.CheckXsltInvokeLateBound(qilInvokeLateBound);
		return qilInvokeLateBound;
	}

	public QilInvokeEarlyBound XsltInvokeEarlyBound(QilNode name, QilNode clrMethod, QilNode arguments, XmlQueryType xmlType)
	{
		QilInvokeEarlyBound qilInvokeEarlyBound = new QilInvokeEarlyBound(QilNodeType.XsltInvokeEarlyBound, name, clrMethod, arguments, xmlType);
		qilInvokeEarlyBound.XmlType = typeCheck.CheckXsltInvokeEarlyBound(qilInvokeEarlyBound);
		return qilInvokeEarlyBound;
	}

	public QilBinary XsltCopy(QilNode left, QilNode right)
	{
		QilBinary qilBinary = new QilBinary(QilNodeType.XsltCopy, left, right);
		qilBinary.XmlType = typeCheck.CheckXsltCopy(qilBinary);
		return qilBinary;
	}

	public QilUnary XsltCopyOf(QilNode child)
	{
		QilUnary qilUnary = new QilUnary(QilNodeType.XsltCopyOf, child);
		qilUnary.XmlType = typeCheck.CheckXsltCopyOf(qilUnary);
		return qilUnary;
	}

	public QilTargetType XsltConvert(QilNode source, QilNode targetType)
	{
		QilTargetType qilTargetType = new QilTargetType(QilNodeType.XsltConvert, source, targetType);
		qilTargetType.XmlType = typeCheck.CheckXsltConvert(qilTargetType);
		return qilTargetType;
	}

	[Conditional("QIL_TRACE_NODE_CREATION")]
	public void TraceNode(QilNode n)
	{
	}
}
