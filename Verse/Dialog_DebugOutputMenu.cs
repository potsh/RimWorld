using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Verse
{
	public class Dialog_DebugOutputMenu : Dialog_DebugOptionLister
	{
		private struct DebugOutputOption
		{
			public string label;

			public string category;

			public Action action;
		}

		private List<DebugOutputOption> debugOutputs = new List<DebugOutputOption>();

		private const string DefaultCategory = "General";

		public override bool IsDebug => true;

		public Dialog_DebugOutputMenu()
		{
			forcePause = true;
			foreach (Type item in GenTypes.AllTypesWithAttribute<HasDebugOutputAttribute>())
			{
				MethodInfo[] methods = item.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo mi in methods)
				{
					if (mi.TryGetAttribute(out DebugOutputAttribute _))
					{
						string label = GenText.SplitCamelCase(mi.Name);
						Action action = delegate
						{
							mi.Invoke(null, null);
						};
						CategoryAttribute customAttribute2 = null;
						string category = (!mi.TryGetAttribute(out customAttribute2)) ? "General" : customAttribute2.name;
						debugOutputs.Add(new DebugOutputOption
						{
							label = label,
							category = category,
							action = action
						});
					}
				}
			}
			debugOutputs = (from r in debugOutputs
			orderby r.category, r.label
			select r).ToList();
		}

		protected override void DoListingItems()
		{
			string b = null;
			foreach (DebugOutputOption debugOutput in debugOutputs)
			{
				DebugOutputOption current = debugOutput;
				if (current.category != b)
				{
					DoLabel(current.category);
					b = current.category;
				}
				Log.openOnMessage = true;
				try
				{
					DebugAction(current.label, current.action);
				}
				finally
				{
					Log.openOnMessage = false;
				}
			}
		}
	}
}
