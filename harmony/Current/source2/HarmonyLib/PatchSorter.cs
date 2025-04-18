using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HarmonyLib;

internal class PatchSorter
{
	private class PatchSortingWrapper : IComparable
	{
		internal readonly HashSet<PatchSortingWrapper> after;

		internal readonly HashSet<PatchSortingWrapper> before;

		internal readonly Patch innerPatch;

		internal PatchSortingWrapper(Patch patch)
		{
			innerPatch = patch;
			before = new HashSet<PatchSortingWrapper>();
			after = new HashSet<PatchSortingWrapper>();
		}

		public int CompareTo(object obj)
		{
			return PatchInfoSerialization.PriorityComparer((obj is PatchSortingWrapper patchSortingWrapper) ? patchSortingWrapper.innerPatch : null, innerPatch.index, innerPatch.priority);
		}

		public override bool Equals(object obj)
		{
			if (obj is PatchSortingWrapper patchSortingWrapper)
			{
				return innerPatch.PatchMethod == patchSortingWrapper.innerPatch.PatchMethod;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return innerPatch.PatchMethod.GetHashCode();
		}

		internal void AddBeforeDependency(IEnumerable<PatchSortingWrapper> dependencies)
		{
			foreach (PatchSortingWrapper dependency in dependencies)
			{
				before.Add(dependency);
				dependency.after.Add(this);
			}
		}

		internal void AddAfterDependency(IEnumerable<PatchSortingWrapper> dependencies)
		{
			foreach (PatchSortingWrapper dependency in dependencies)
			{
				after.Add(dependency);
				dependency.before.Add(this);
			}
		}

		internal void RemoveAfterDependency(PatchSortingWrapper afterNode)
		{
			after.Remove(afterNode);
			afterNode.before.Remove(this);
		}

		internal void RemoveBeforeDependency(PatchSortingWrapper beforeNode)
		{
			before.Remove(beforeNode);
			beforeNode.after.Remove(this);
		}
	}

	internal class PatchDetailedComparer : IEqualityComparer<Patch>
	{
		public bool Equals(Patch x, Patch y)
		{
			if (y != null && x != null && x.owner == y.owner && x.PatchMethod == y.PatchMethod && x.index == y.index && x.priority == y.priority && x.before.Length == y.before.Length && x.after.Length == y.after.Length && x.before.All(((IEnumerable<string>)y.before).Contains<string>))
			{
				return x.after.All(((IEnumerable<string>)y.after).Contains<string>);
			}
			return false;
		}

		public int GetHashCode(Patch obj)
		{
			return obj.GetHashCode();
		}
	}

	private List<PatchSortingWrapper> patches;

	private HashSet<PatchSortingWrapper> handledPatches;

	private List<PatchSortingWrapper> result;

	private List<PatchSortingWrapper> waitingList;

	internal Patch[] sortedPatchArray;

	private readonly bool debug;

	internal PatchSorter(Patch[] patches, bool debug)
	{
		this.patches = patches.Select((Patch x) => new PatchSortingWrapper(x)).ToList();
		this.debug = debug;
		foreach (PatchSortingWrapper node in this.patches)
		{
			node.AddBeforeDependency(this.patches.Where((PatchSortingWrapper x) => node.innerPatch.before.Contains(x.innerPatch.owner)));
			node.AddAfterDependency(this.patches.Where((PatchSortingWrapper x) => node.innerPatch.after.Contains(x.innerPatch.owner)));
		}
		this.patches.Sort();
	}

	internal List<MethodInfo> Sort(MethodBase original)
	{
		if (sortedPatchArray != null)
		{
			return sortedPatchArray.Select((Patch x) => x.GetMethod(original)).ToList();
		}
		handledPatches = new HashSet<PatchSortingWrapper>();
		waitingList = new List<PatchSortingWrapper>();
		result = new List<PatchSortingWrapper>(patches.Count);
		Queue<PatchSortingWrapper> queue = new Queue<PatchSortingWrapper>(patches);
		while (queue.Count != 0)
		{
			foreach (PatchSortingWrapper item in queue)
			{
				if (item.after.All((PatchSortingWrapper x) => handledPatches.Contains(x)))
				{
					AddNodeToResult(item);
					if (item.before.Count != 0)
					{
						ProcessWaitingList();
					}
				}
				else
				{
					waitingList.Add(item);
				}
			}
			CullDependency();
			queue = new Queue<PatchSortingWrapper>(waitingList);
			waitingList.Clear();
		}
		sortedPatchArray = result.Select((PatchSortingWrapper x) => x.innerPatch).ToArray();
		handledPatches = null;
		waitingList = null;
		patches = null;
		return sortedPatchArray.Select((Patch x) => x.GetMethod(original)).ToList();
	}

	internal bool ComparePatchLists(Patch[] patches)
	{
		if (sortedPatchArray == null)
		{
			Sort(null);
		}
		if (patches != null && sortedPatchArray.Length == patches.Length)
		{
			return sortedPatchArray.All((Patch x) => patches.Contains(x, new PatchDetailedComparer()));
		}
		return false;
	}

	private void CullDependency()
	{
		for (int num = waitingList.Count - 1; num >= 0; num--)
		{
			foreach (PatchSortingWrapper item in waitingList[num].after)
			{
				if (!handledPatches.Contains(item))
				{
					waitingList[num].RemoveAfterDependency(item);
					if (debug)
					{
						string text = item.innerPatch.PatchMethod.FullDescription();
						string text2 = waitingList[num].innerPatch.PatchMethod.FullDescription();
						FileLog.LogBuffered("Breaking dependance between " + text + " and " + text2);
					}
					return;
				}
			}
		}
	}

	private void ProcessWaitingList()
	{
		int num = waitingList.Count;
		int num2 = 0;
		while (num2 < num)
		{
			PatchSortingWrapper patchSortingWrapper = waitingList[num2];
			if (patchSortingWrapper.after.All(handledPatches.Contains))
			{
				waitingList.Remove(patchSortingWrapper);
				AddNodeToResult(patchSortingWrapper);
				num--;
				num2 = 0;
			}
			else
			{
				num2++;
			}
		}
	}

	private void AddNodeToResult(PatchSortingWrapper node)
	{
		result.Add(node);
		handledPatches.Add(node);
	}
}
