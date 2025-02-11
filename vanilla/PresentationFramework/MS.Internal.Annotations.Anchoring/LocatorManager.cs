using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Annotations.Storage;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Xml;

namespace MS.Internal.Annotations.Anchoring;

internal sealed class LocatorManager : DispatcherObject
{
	private class ProcessingTreeState
	{
		private List<IAttachedAnnotation> _attachedAnnotations = new List<IAttachedAnnotation>();

		private Stack<bool> _calledProcessAnnotations = new Stack<bool>();

		public List<IAttachedAnnotation> AttachedAnnotations => _attachedAnnotations;

		public bool CalledProcessAnnotations
		{
			get
			{
				return _calledProcessAnnotations.Peek();
			}
			set
			{
				if (_calledProcessAnnotations.Peek() != value)
				{
					_calledProcessAnnotations.Pop();
					_calledProcessAnnotations.Push(value);
				}
			}
		}

		public ProcessingTreeState()
		{
			_calledProcessAnnotations.Push(item: false);
		}

		public void Push()
		{
			_calledProcessAnnotations.Push(item: false);
		}

		public bool Pop()
		{
			return _calledProcessAnnotations.Pop();
		}
	}

	private class ResolvingLocatorState
	{
		public ContentLocator ContentLocatorBase;

		public int LocatorPartIndex;

		public AttachmentLevel AttachmentLevel;

		public object AttachedAnchor;

		public bool Finished;

		public object LastNodeMatched;
	}

	public static readonly DependencyProperty SubTreeProcessorIdProperty = DependencyProperty.RegisterAttached("SubTreeProcessorId", typeof(string), typeof(LocatorManager), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior));

	private Hashtable _locatorPartHandlers;

	private Hashtable _subtreeProcessors;

	private Hashtable _selectionProcessors;

	private static readonly char[] Separators = new char[3] { ',', ' ', ';' };

	private AnnotationStore _internalStore;

	public LocatorManager()
		: this(null)
	{
	}

	public LocatorManager(AnnotationStore store)
	{
		_locatorPartHandlers = new Hashtable();
		_subtreeProcessors = new Hashtable();
		_selectionProcessors = new Hashtable();
		RegisterSubTreeProcessor(new DataIdProcessor(this), "Id");
		RegisterSubTreeProcessor(new FixedPageProcessor(this), FixedPageProcessor.Id);
		TreeNodeSelectionProcessor processor = new TreeNodeSelectionProcessor();
		RegisterSelectionProcessor(processor, typeof(FrameworkElement));
		RegisterSelectionProcessor(processor, typeof(FrameworkContentElement));
		TextSelectionProcessor processor2 = new TextSelectionProcessor();
		RegisterSelectionProcessor(processor2, typeof(TextRange));
		RegisterSelectionProcessor(processor2, typeof(TextAnchor));
		_internalStore = store;
	}

	public void RegisterSubTreeProcessor(SubTreeProcessor processor, string processorId)
	{
		VerifyAccess();
		if (processor == null)
		{
			throw new ArgumentNullException("processor");
		}
		if (processorId == null)
		{
			throw new ArgumentNullException("processorId");
		}
		XmlQualifiedName[] locatorPartTypes = processor.GetLocatorPartTypes();
		_subtreeProcessors[processorId] = processor;
		if (locatorPartTypes != null)
		{
			XmlQualifiedName[] array = locatorPartTypes;
			foreach (XmlQualifiedName key in array)
			{
				_locatorPartHandlers[key] = processor;
			}
		}
	}

	public SubTreeProcessor GetSubTreeProcessor(DependencyObject node)
	{
		VerifyAccess();
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		string text = node.GetValue(SubTreeProcessorIdProperty) as string;
		if (!string.IsNullOrEmpty(text))
		{
			SubTreeProcessor subTreeProcessor = (SubTreeProcessor)_subtreeProcessors[text];
			if (subTreeProcessor != null)
			{
				return subTreeProcessor;
			}
			throw new ArgumentException(SR.Format(SR.InvalidSubTreeProcessor, text));
		}
		return _subtreeProcessors["Id"] as SubTreeProcessor;
	}

	public SubTreeProcessor GetSubTreeProcessorForLocatorPart(ContentLocatorPart locatorPart)
	{
		VerifyAccess();
		if (locatorPart == null)
		{
			throw new ArgumentNullException("locatorPart");
		}
		return _locatorPartHandlers[locatorPart.PartType] as SubTreeProcessor;
	}

	public void RegisterSelectionProcessor(SelectionProcessor processor, Type selectionType)
	{
		VerifyAccess();
		if (processor == null)
		{
			throw new ArgumentNullException("processor");
		}
		if (selectionType == null)
		{
			throw new ArgumentNullException("selectionType");
		}
		XmlQualifiedName[] locatorPartTypes = processor.GetLocatorPartTypes();
		_selectionProcessors[selectionType] = processor;
		if (locatorPartTypes != null)
		{
			XmlQualifiedName[] array = locatorPartTypes;
			foreach (XmlQualifiedName key in array)
			{
				_locatorPartHandlers[key] = processor;
			}
		}
	}

	public SelectionProcessor GetSelectionProcessor(Type selectionType)
	{
		VerifyAccess();
		if (selectionType == null)
		{
			throw new ArgumentNullException("selectionType");
		}
		SelectionProcessor selectionProcessor = null;
		do
		{
			selectionProcessor = _selectionProcessors[selectionType] as SelectionProcessor;
			selectionType = selectionType.BaseType;
		}
		while (selectionProcessor == null && selectionType != null);
		return selectionProcessor;
	}

	public SelectionProcessor GetSelectionProcessorForLocatorPart(ContentLocatorPart locatorPart)
	{
		VerifyAccess();
		if (locatorPart == null)
		{
			throw new ArgumentNullException("locatorPart");
		}
		return _locatorPartHandlers[locatorPart.PartType] as SelectionProcessor;
	}

	public IList<IAttachedAnnotation> ProcessAnnotations(DependencyObject node)
	{
		VerifyAccess();
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		IList<IAttachedAnnotation> list = new List<IAttachedAnnotation>();
		IList<ContentLocatorBase> list2 = GenerateLocators(node);
		if (list2.Count > 0)
		{
			AnnotationStore annotationStore = null;
			if (_internalStore != null)
			{
				annotationStore = _internalStore;
			}
			else
			{
				AnnotationService service = AnnotationService.GetService(node);
				if (service == null || !service.IsEnabled)
				{
					throw new InvalidOperationException(SR.AnnotationServiceNotEnabled);
				}
				annotationStore = service.Store;
			}
			ContentLocator[] array = new ContentLocator[list2.Count];
			ContentLocatorBase[] array2 = array;
			list2.CopyTo(array2, 0);
			IList<Annotation> annotations = annotationStore.GetAnnotations(array[0]);
			foreach (ContentLocator item in list2)
			{
				if (item.Parts[item.Parts.Count - 1].NameValuePairs.ContainsKey("IncludeOverlaps"))
				{
					item.Parts.RemoveAt(item.Parts.Count - 1);
				}
			}
			foreach (Annotation item2 in annotations)
			{
				foreach (AnnotationResource anchor in item2.Anchors)
				{
					foreach (ContentLocatorBase contentLocator in anchor.ContentLocators)
					{
						AttachmentLevel attachmentLevel;
						object attachedAnchor = FindAttachedAnchor(node, array, contentLocator, out attachmentLevel);
						if (attachmentLevel != 0)
						{
							list.Add(new AttachedAnnotation(this, item2, anchor, attachedAnchor, attachmentLevel));
							break;
						}
					}
				}
			}
		}
		return list;
	}

	public IList<ContentLocatorBase> GenerateLocators(object selection)
	{
		VerifyAccess();
		if (selection == null)
		{
			throw new ArgumentNullException("selection");
		}
		ICollection collection = null;
		SelectionProcessor selectionProcessor = GetSelectionProcessor(selection.GetType());
		if (selectionProcessor != null)
		{
			collection = (ICollection)selectionProcessor.GetSelectedNodes(selection);
			IList<ContentLocatorBase> list = null;
			PathNode pathNode = PathNode.BuildPathForElements(collection);
			if (pathNode != null)
			{
				SubTreeProcessor subTreeProcessor = GetSubTreeProcessor(pathNode.Node);
				list = GenerateLocators(subTreeProcessor, pathNode, selection);
			}
			if (list == null)
			{
				list = new List<ContentLocatorBase>(0);
			}
			return list;
		}
		throw new ArgumentException("Unsupported Selection", "selection");
	}

	public object ResolveLocator(ContentLocatorBase locator, int offset, DependencyObject startNode, out AttachmentLevel attachmentLevel)
	{
		VerifyAccess();
		if (locator == null)
		{
			throw new ArgumentNullException("locator");
		}
		if (startNode == null)
		{
			throw new ArgumentNullException("startNode");
		}
		if (locator is ContentLocator contentLocator && (offset < 0 || offset >= contentLocator.Parts.Count))
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		return InternalResolveLocator(locator, offset, startNode, skipStartNode: false, out attachmentLevel);
	}

	public static void SetSubTreeProcessorId(DependencyObject d, string id)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		d.SetValue(SubTreeProcessorIdProperty, id);
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public static string GetSubTreeProcessorId(DependencyObject d)
	{
		if (d == null)
		{
			throw new ArgumentNullException("d");
		}
		return d.GetValue(SubTreeProcessorIdProperty) as string;
	}

	internal IList<IAttachedAnnotation> ProcessSubTree(DependencyObject subTree)
	{
		if (subTree == null)
		{
			throw new ArgumentNullException("subTree");
		}
		ProcessingTreeState processingTreeState = new ProcessingTreeState();
		new PrePostDescendentsWalker<ProcessingTreeState>(TreeWalkPriority.VisualTree, PreVisit, PostVisit, processingTreeState).StartWalk(subTree);
		return processingTreeState.AttachedAnnotations;
	}

	internal object FindAttachedAnchor(DependencyObject startNode, ContentLocator[] prefixes, ContentLocatorBase locator, out AttachmentLevel attachmentLevel)
	{
		if (startNode == null)
		{
			throw new ArgumentNullException("startNode");
		}
		if (locator == null)
		{
			throw new ArgumentNullException("locator");
		}
		attachmentLevel = AttachmentLevel.Unresolved;
		object result = null;
		bool matched = true;
		int num = FindMatchingPrefix(prefixes, locator, out matched);
		if (matched)
		{
			ContentLocator contentLocator = locator as ContentLocator;
			if (contentLocator == null || num < contentLocator.Parts.Count)
			{
				result = InternalResolveLocator(locator, num, startNode, num != 0, out attachmentLevel);
			}
			if (attachmentLevel == AttachmentLevel.Unresolved && num > 0)
			{
				if (num == 0)
				{
					attachmentLevel = AttachmentLevel.Unresolved;
				}
				else if (contentLocator != null && num < contentLocator.Parts.Count)
				{
					attachmentLevel = AttachmentLevel.Incomplete;
					result = startNode;
				}
				else
				{
					attachmentLevel = AttachmentLevel.Full;
					result = startNode;
				}
			}
		}
		return result;
	}

	private int FindMatchingPrefix(ContentLocator[] prefixes, ContentLocatorBase locator, out bool matched)
	{
		matched = true;
		int result = 0;
		if (locator is ContentLocator contentLocator && prefixes != null && prefixes.Length != 0)
		{
			matched = false;
			foreach (ContentLocator contentLocator2 in prefixes)
			{
				if (contentLocator.StartsWith(contentLocator2))
				{
					result = contentLocator2.Parts.Count;
					matched = true;
					break;
				}
			}
		}
		return result;
	}

	private IList<ContentLocatorBase> GenerateLocators(SubTreeProcessor processor, PathNode startNode, object selection)
	{
		List<ContentLocatorBase> list = new List<ContentLocatorBase>();
		bool continueGenerating = true;
		ContentLocator contentLocator = processor.GenerateLocator(startNode, out continueGenerating);
		bool flag = contentLocator != null;
		IList<ContentLocatorBase> list2 = null;
		if (continueGenerating)
		{
			switch (startNode.Children.Count)
			{
			case 0:
				if (contentLocator != null)
				{
					list.Add(contentLocator);
				}
				break;
			case 1:
			{
				SubTreeProcessor subTreeProcessor = GetSubTreeProcessor(startNode.Node);
				list2 = GenerateLocators(subTreeProcessor, (PathNode)startNode.Children[0], selection);
				if (list2 != null && list2.Count > 0)
				{
					flag = false;
				}
				if (contentLocator != null)
				{
					list.AddRange(Merge(contentLocator, list2));
				}
				else
				{
					list.AddRange(list2);
				}
				break;
			}
			default:
			{
				ContentLocatorBase contentLocatorBase = GenerateLocatorGroup(startNode, selection);
				if (contentLocatorBase != null)
				{
					flag = false;
				}
				if (contentLocator != null)
				{
					list.Add(contentLocator.Merge(contentLocatorBase));
				}
				else if (contentLocatorBase != null)
				{
					list.Add(contentLocatorBase);
				}
				break;
			}
			}
		}
		else if (contentLocator != null)
		{
			list.Add(contentLocator);
		}
		if (flag && selection != null)
		{
			SelectionProcessor selectionProcessor = GetSelectionProcessor(selection.GetType());
			if (selectionProcessor != null)
			{
				IList<ContentLocatorPart> list3 = selectionProcessor.GenerateLocatorParts(selection, startNode.Node);
				if (list3 != null && list3.Count > 0)
				{
					List<ContentLocatorBase> list4 = new List<ContentLocatorBase>(list.Count * list3.Count);
					foreach (ContentLocatorBase item in list)
					{
						list4.AddRange(((ContentLocator)item).DotProduct(list3));
					}
					list = list4;
				}
			}
		}
		return list;
	}

	private ContentLocatorBase GenerateLocatorGroup(PathNode node, object selection)
	{
		SubTreeProcessor subTreeProcessor = GetSubTreeProcessor(node.Node);
		IList<ContentLocatorBase> list = null;
		ContentLocatorGroup contentLocatorGroup = new ContentLocatorGroup();
		foreach (PathNode child in node.Children)
		{
			list = GenerateLocators(subTreeProcessor, child, selection);
			if (list != null && list.Count > 0 && list[0] != null)
			{
				if (list[0] is ContentLocator contentLocator && contentLocator.Parts.Count != 0)
				{
					contentLocatorGroup.Locators.Add(contentLocator);
				}
				else
				{
					_ = list[0] is ContentLocatorGroup;
				}
			}
		}
		if (contentLocatorGroup.Locators.Count == 0)
		{
			return null;
		}
		if (contentLocatorGroup.Locators.Count == 1)
		{
			ContentLocator contentLocator2 = contentLocatorGroup.Locators[0];
			contentLocatorGroup.Locators.Remove(contentLocator2);
			return contentLocator2;
		}
		return contentLocatorGroup;
	}

	private bool PreVisit(DependencyObject dependencyObject, ProcessingTreeState data, bool visitedViaVisualTree)
	{
		bool calledProcessAnnotations = false;
		IList<IAttachedAnnotation> list = GetSubTreeProcessor(dependencyObject).PreProcessNode(dependencyObject, out calledProcessAnnotations);
		if (list != null)
		{
			data.AttachedAnnotations.AddRange(list);
		}
		data.CalledProcessAnnotations |= calledProcessAnnotations;
		data.Push();
		return !calledProcessAnnotations;
	}

	private bool PostVisit(DependencyObject dependencyObject, ProcessingTreeState data, bool visitedViaVisualTree)
	{
		bool flag = data.Pop();
		SubTreeProcessor subTreeProcessor = GetSubTreeProcessor(dependencyObject);
		bool calledProcessAnnotations = false;
		IList<IAttachedAnnotation> list = subTreeProcessor.PostProcessNode(dependencyObject, flag, out calledProcessAnnotations);
		if (list != null)
		{
			data.AttachedAnnotations.AddRange(list);
		}
		data.CalledProcessAnnotations = data.CalledProcessAnnotations || calledProcessAnnotations || flag;
		return true;
	}

	private object InternalResolveLocator(ContentLocatorBase locator, int offset, DependencyObject startNode, bool skipStartNode, out AttachmentLevel attachmentLevel)
	{
		attachmentLevel = AttachmentLevel.Full;
		object selection = null;
		ContentLocatorGroup contentLocatorGroup = locator as ContentLocatorGroup;
		ContentLocator contentLocator = locator as ContentLocator;
		AttachmentLevel attachmentLevel2 = AttachmentLevel.Unresolved;
		if (contentLocator != null && offset == contentLocator.Parts.Count - 1)
		{
			ContentLocatorPart locatorPart = contentLocator.Parts[offset];
			SelectionProcessor selectionProcessorForLocatorPart = GetSelectionProcessorForLocatorPart(locatorPart);
			if (selectionProcessorForLocatorPart != null)
			{
				selection = selectionProcessorForLocatorPart.ResolveLocatorPart(locatorPart, startNode, out attachmentLevel2);
				attachmentLevel = attachmentLevel2;
				return selection;
			}
		}
		IList<ContentLocator> list = null;
		if (contentLocatorGroup == null)
		{
			list = new List<ContentLocator>(1);
			list.Add(contentLocator);
		}
		else
		{
			AnnotationService service = AnnotationService.GetService(startNode);
			if (service != null)
			{
				startNode = service.Root;
			}
			list = contentLocatorGroup.Locators;
			offset = 0;
			skipStartNode = false;
		}
		bool flag = true;
		if (list.Count > 0)
		{
			ResolvingLocatorState resolvingLocatorState = ResolveSingleLocator(ref selection, ref attachmentLevel, AttachmentLevel.StartPortion, list[0], offset, startNode, skipStartNode);
			if (list.Count == 1)
			{
				selection = resolvingLocatorState.AttachedAnchor;
				attachmentLevel = resolvingLocatorState.AttachmentLevel;
			}
			else
			{
				if (list.Count > 2)
				{
					AttachmentLevel attachmentLevel3 = AttachmentLevel.Unresolved;
					AttachmentLevel attachmentLevel4 = attachmentLevel;
					for (int i = 1; i < list.Count - 1; i++)
					{
						resolvingLocatorState = ResolveSingleLocator(ref selection, ref attachmentLevel, AttachmentLevel.MiddlePortion, list[i], offset, startNode, skipStartNode);
						if (attachmentLevel3 == AttachmentLevel.Unresolved || (attachmentLevel & AttachmentLevel.MiddlePortion) != 0)
						{
							attachmentLevel3 = attachmentLevel;
						}
						attachmentLevel = attachmentLevel4;
					}
					attachmentLevel = attachmentLevel3;
				}
				else
				{
					flag = false;
				}
				resolvingLocatorState = ResolveSingleLocator(ref selection, ref attachmentLevel, AttachmentLevel.EndPortion, list[list.Count - 1], offset, startNode, skipStartNode);
				if (!flag && attachmentLevel == AttachmentLevel.MiddlePortion)
				{
					attachmentLevel &= ~AttachmentLevel.MiddlePortion;
				}
				if (attachmentLevel == (AttachmentLevel.StartPortion | AttachmentLevel.EndPortion))
				{
					attachmentLevel = AttachmentLevel.Full;
				}
			}
		}
		else
		{
			attachmentLevel = AttachmentLevel.Unresolved;
		}
		return selection;
	}

	private ResolvingLocatorState ResolveSingleLocator(ref object selection, ref AttachmentLevel attachmentLevel, AttachmentLevel attemptedLevel, ContentLocator locator, int offset, DependencyObject startNode, bool skipStartNode)
	{
		ResolvingLocatorState resolvingLocatorState = new ResolvingLocatorState();
		resolvingLocatorState.LocatorPartIndex = offset;
		resolvingLocatorState.ContentLocatorBase = locator;
		new PrePostDescendentsWalker<ResolvingLocatorState>(TreeWalkPriority.VisualTree, ResolveLocatorPart, TerminateResolve, resolvingLocatorState).StartWalk(startNode, skipStartNode);
		if (resolvingLocatorState.AttachmentLevel == AttachmentLevel.Full && resolvingLocatorState.AttachedAnchor != null)
		{
			if (selection != null)
			{
				SelectionProcessor selectionProcessor = GetSelectionProcessor(selection.GetType());
				if (selectionProcessor != null)
				{
					if (selectionProcessor.MergeSelections(selection, resolvingLocatorState.AttachedAnchor, out var newSelection))
					{
						selection = newSelection;
					}
					else
					{
						attachmentLevel &= ~attemptedLevel;
					}
				}
				else
				{
					attachmentLevel &= ~attemptedLevel;
				}
			}
			else
			{
				selection = resolvingLocatorState.AttachedAnchor;
			}
		}
		else
		{
			attachmentLevel &= ~attemptedLevel;
		}
		return resolvingLocatorState;
	}

	private bool ResolveLocatorPart(DependencyObject dependencyObject, ResolvingLocatorState data, bool visitedViaVisualTree)
	{
		if (data.Finished)
		{
			return false;
		}
		ContentLocator contentLocatorBase = data.ContentLocatorBase;
		bool continueResolving = true;
		DependencyObject dependencyObject2 = null;
		SubTreeProcessor subTreeProcessor = null;
		ContentLocatorPart contentLocatorPart = contentLocatorBase.Parts[data.LocatorPartIndex];
		if (contentLocatorPart == null)
		{
			continueResolving = false;
		}
		subTreeProcessor = GetSubTreeProcessorForLocatorPart(contentLocatorPart);
		if (subTreeProcessor == null)
		{
			continueResolving = false;
		}
		if (contentLocatorPart != null && subTreeProcessor != null)
		{
			dependencyObject2 = subTreeProcessor.ResolveLocatorPart(contentLocatorPart, dependencyObject, out continueResolving);
			if (dependencyObject2 != null)
			{
				data.AttachmentLevel = AttachmentLevel.Incomplete;
				data.AttachedAnchor = dependencyObject2;
				continueResolving = true;
				data.LastNodeMatched = dependencyObject2;
				data.LocatorPartIndex++;
				if (data.LocatorPartIndex == contentLocatorBase.Parts.Count)
				{
					data.AttachmentLevel = AttachmentLevel.Full;
					data.AttachedAnchor = dependencyObject2;
					continueResolving = false;
				}
				else if (data.LocatorPartIndex == contentLocatorBase.Parts.Count - 1)
				{
					contentLocatorPart = contentLocatorBase.Parts[data.LocatorPartIndex];
					SelectionProcessor selectionProcessorForLocatorPart = GetSelectionProcessorForLocatorPart(contentLocatorPart);
					if (selectionProcessorForLocatorPart != null)
					{
						AttachmentLevel attachmentLevel;
						object obj = selectionProcessorForLocatorPart.ResolveLocatorPart(contentLocatorPart, dependencyObject2, out attachmentLevel);
						if (obj != null)
						{
							data.AttachmentLevel = attachmentLevel;
							data.AttachedAnchor = obj;
							continueResolving = false;
						}
						else
						{
							continueResolving = false;
						}
					}
				}
			}
		}
		return continueResolving;
	}

	private bool TerminateResolve(DependencyObject dependencyObject, ResolvingLocatorState data, bool visitedViaVisualTree)
	{
		if (!data.Finished && data.LastNodeMatched == dependencyObject)
		{
			data.Finished = true;
		}
		return false;
	}

	private IList<ContentLocatorBase> Merge(ContentLocatorBase initialLocator, IList<ContentLocatorBase> additionalLocators)
	{
		if (additionalLocators == null || additionalLocators.Count == 0)
		{
			return new List<ContentLocatorBase>(1) { initialLocator };
		}
		for (int i = 1; i < additionalLocators.Count; i++)
		{
			additionalLocators[i] = ((ContentLocatorBase)initialLocator.Clone()).Merge(additionalLocators[i]);
		}
		additionalLocators[0] = initialLocator.Merge(additionalLocators[0]);
		return additionalLocators;
	}
}
