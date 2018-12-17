using RimWorld;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Verse
{
	public class Def : Editable
	{
		[Description("The name of this Def. It is used as an identifier by the game code.")]
		public string defName = "UnnamedDef";

		[Description("A human-readable label used to identify this in game.")]
		[DefaultValue(null)]
		[MustTranslate]
		public string label;

		[Description("A human-readable description given when the Def is inspected by players.")]
		[DefaultValue(null)]
		[MustTranslate]
		public string description;

		[Description("Disables config error checking. Intended for mod use. (Be careful!)")]
		[DefaultValue(false)]
		[MustTranslate]
		public bool ignoreConfigErrors;

		[Description("Mod-specific data. Not used by core game code.")]
		[DefaultValue(null)]
		public List<DefModExtension> modExtensions;

		[Unsaved]
		public ushort shortHash;

		[Unsaved]
		public ushort index = ushort.MaxValue;

		[Unsaved]
		public ModContentPack modContentPack;

		[Unsaved]
		public DefPackage defPackage;

		[Unsaved]
		private string cachedLabelCap;

		[Unsaved]
		public bool generated;

		[Unsaved]
		public ushort debugRandomId = (ushort)Rand.RangeInclusive(0, 65535);

		public const string DefaultDefName = "UnnamedDef";

		private static Regex AllowedDefnamesRegex = new Regex("^[a-zA-Z0-9\\-_]*$");

		public string LabelCap
		{
			get
			{
				if (label.NullOrEmpty())
				{
					return null;
				}
				if (cachedLabelCap.NullOrEmpty())
				{
					cachedLabelCap = label.CapitalizeFirst();
				}
				return cachedLabelCap;
			}
		}

		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			yield break;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (defName == "UnnamedDef")
			{
				yield return GetType() + " lacks defName. Label=" + label;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (defName == "null")
			{
				yield return "defName cannot be the string 'null'.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (!AllowedDefnamesRegex.IsMatch(defName))
			{
				yield return "defName " + defName + " should only contain letters, numbers, underscores, or dashes.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (modExtensions != null)
			{
				for (int i = 0; i < modExtensions.Count; i++)
				{
					using (IEnumerator<string> enumerator = modExtensions[i].ConfigErrors().GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							string err = enumerator.Current;
							yield return err;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			if (description != null)
			{
				if (description == string.Empty)
				{
					yield return "empty description";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (char.IsWhiteSpace(description[0]))
				{
					yield return "description has leading whitespace";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (char.IsWhiteSpace(description[description.Length - 1]))
				{
					yield return "description has trailing whitespace";
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_02cf:
			/*Error near IL_02d0: Unexpected return in MoveNext()*/;
		}

		public virtual void ClearCachedData()
		{
			cachedLabelCap = null;
		}

		public override string ToString()
		{
			return defName;
		}

		public override int GetHashCode()
		{
			return defName.GetHashCode();
		}

		public T GetModExtension<T>() where T : DefModExtension
		{
			if (modExtensions == null)
			{
				return (T)null;
			}
			for (int i = 0; i < modExtensions.Count; i++)
			{
				if (modExtensions[i] is T)
				{
					return modExtensions[i] as T;
				}
			}
			return (T)null;
		}

		public bool HasModExtension<T>() where T : DefModExtension
		{
			return GetModExtension<T>() != null;
		}
	}
}
