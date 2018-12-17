using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Verse
{
	public class ThreadLocalDeepProfiler
	{
		private class Watcher
		{
			private string label;

			private Stopwatch watch;

			private List<Watcher> children;

			public string Label => label;

			public Stopwatch Watch => watch;

			public List<Watcher> Children => children;

			public Watcher(string label)
			{
				this.label = label;
				watch = Stopwatch.StartNew();
				children = null;
			}

			public void AddChildResult(Watcher w)
			{
				if (children == null)
				{
					children = new List<Watcher>();
				}
				children.Add(w);
			}
		}

		private Stack<Watcher> watchers = new Stack<Watcher>();

		private static readonly string[] Prefixes;

		private const int MaxDepth = 50;

		static ThreadLocalDeepProfiler()
		{
			Prefixes = new string[50];
			for (int i = 0; i < 50; i++)
			{
				Prefixes[i] = string.Empty;
				for (int j = 0; j < i; j++)
				{
					string[] prefixes;
					int num;
					(prefixes = Prefixes)[num = i] = prefixes[num] + " -";
				}
			}
		}

		public void Start(string label = null)
		{
			if (Prefs.LogVerbose)
			{
				watchers.Push(new Watcher(label));
			}
		}

		public void End()
		{
			if (Prefs.LogVerbose)
			{
				if (watchers.Count == 0)
				{
					Log.Error("Ended deep profiling while not profiling.");
				}
				else
				{
					Watcher watcher = watchers.Pop();
					watcher.Watch.Stop();
					if (watchers.Count > 0)
					{
						watchers.Peek().AddChildResult(watcher);
					}
					else
					{
						Output(watcher);
					}
				}
			}
		}

		private void Output(Watcher root)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (UnityData.IsInMainThread)
			{
				stringBuilder.AppendLine("--- Main thread ---");
			}
			else
			{
				stringBuilder.AppendLine("--- Thread " + Thread.CurrentThread.ManagedThreadId + " ---");
			}
			AppendStringRecursive(stringBuilder, root, 0);
			Log.Message(stringBuilder.ToString());
		}

		private void AppendStringRecursive(StringBuilder sb, Watcher w, int depth)
		{
			sb.AppendLine(Prefixes[depth] + " " + w.Watch.ElapsedMilliseconds + "ms " + w.Label);
			if (w.Children != null)
			{
				for (int i = 0; i < w.Children.Count; i++)
				{
					AppendStringRecursive(sb, w.Children[i], depth + 1);
				}
			}
		}
	}
}
