using System;
using System.Collections.Generic;
using Verse;

namespace CombatExtended;

public static class CE_Scriber
{
	private struct ScribingAction
	{
		public readonly object owner;

		public readonly Action<string> scribingAction;

		public readonly Action<string> postLoadAction;

		public readonly string id;

		public ScribingAction(object owner, string id, Action<string> action, Action<string> postLoadAction)
		{
			scribingAction = action;
			this.postLoadAction = postLoadAction;
			this.owner = owner;
			this.id = id;
		}
	}

	private static List<ScribingAction> queuedLate = new List<ScribingAction>();

	private static int idCounter = 13;

	private static string curId;

	public static void Late(object owner, Action<string> scribingAction, Action<string> postLoadAction = null)
	{
		string value = null;
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			value = $"{idCounter++}_{Rand.Range(0, 100000) << Rand.Range(0, 32)}";
		}
		Scribe_Values.Look(ref value, "loadingId");
		if (Scribe.mode != LoadSaveMode.Saving)
		{
			ScribingAction item = new ScribingAction(owner, value, scribingAction, postLoadAction);
			queuedLate.Add(item);
			return;
		}
		curId = value;
		try
		{
			scribingAction(value);
		}
		catch (Exception arg)
		{
			Log.Error($"CE: Error while scribing {owner} (Late) {arg}");
		}
		curId = null;
	}

	public static void ExecuteLateScribe()
	{
		Scribe_Values.Look(ref idCounter, "lastscriber_IdCounter", 13);
		for (int i = 0; i < queuedLate.Count; i++)
		{
			if (queuedLate[i].scribingAction != null)
			{
				try
				{
					curId = queuedLate[i].id;
					queuedLate[i].scribingAction(queuedLate[i].id);
					curId = null;
				}
				catch (Exception arg)
				{
					Log.Error($"CE: Error while scribing {queuedLate[i].owner} (ExecuteLateScribe) {arg}");
				}
			}
		}
	}

	public static void Reset()
	{
		for (int i = 0; i < queuedLate.Count; i++)
		{
			if (queuedLate[i].postLoadAction != null)
			{
				try
				{
					curId = queuedLate[i].id;
					queuedLate[i].postLoadAction(queuedLate[i].id);
					curId = null;
				}
				catch (Exception arg)
				{
					Log.Error($"CE: Error while scribing {queuedLate[i].owner} (reset) {arg}");
				}
			}
		}
		queuedLate.Clear();
	}
}
