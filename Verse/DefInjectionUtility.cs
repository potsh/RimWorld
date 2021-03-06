using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Verse
{
	public static class DefInjectionUtility
	{
		public delegate void PossibleDefInjectionTraverser(string suggestedPath, string normalizedPath, bool isCollection, string currentValue, IEnumerable<string> currentValueCollection, bool translationAllowed, bool fullListTranslationAllowed, FieldInfo fieldInfo, Def def);

		public static void ForEachPossibleDefInjection(Type defType, PossibleDefInjectionTraverser action)
		{
			IEnumerable<Def> allDefsInDatabaseForDef = GenDefDatabase.GetAllDefsInDatabaseForDef(defType);
			foreach (Def item in allDefsInDatabaseForDef)
			{
				ForEachPossibleDefInjectionInDef(item, action);
			}
		}

		private static void ForEachPossibleDefInjectionInDef(Def def, PossibleDefInjectionTraverser action)
		{
			HashSet<object> visited = new HashSet<object>();
			ForEachPossibleDefInjectionInDefRecursive(def, def.defName, def.defName, visited, translationAllowed: true, def, action);
		}

		private static void ForEachPossibleDefInjectionInDefRecursive(object obj, string curNormalizedPath, string curSuggestedPath, HashSet<object> visited, bool translationAllowed, Def def, PossibleDefInjectionTraverser action)
		{
			if (obj != null && !visited.Contains(obj))
			{
				visited.Add(obj);
				foreach (FieldInfo item in FieldsInDeterministicOrder(obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)))
				{
					object value = item.GetValue(obj);
					bool flag = translationAllowed && !item.HasAttribute<NoTranslateAttribute>() && !item.HasAttribute<UnsavedAttribute>();
					if (!(value is Def))
					{
						if (typeof(string).IsAssignableFrom(item.FieldType))
						{
							string currentValue = (string)value;
							string normalizedPath = curNormalizedPath + "." + item.Name;
							string suggestedPath = curSuggestedPath + "." + item.Name;
							action(suggestedPath, normalizedPath, isCollection: false, currentValue, null, flag, fullListTranslationAllowed: false, item, def);
						}
						else if (value is IEnumerable<string>)
						{
							IEnumerable<string> currentValueCollection = (IEnumerable<string>)value;
							bool flag2 = item.HasAttribute<TranslationCanChangeCountAttribute>();
							string normalizedPath2 = curNormalizedPath + "." + item.Name;
							string suggestedPath2 = curSuggestedPath + "." + item.Name;
							action(suggestedPath2, normalizedPath2, isCollection: true, null, currentValueCollection, flag, flag && flag2, item, def);
						}
						else if (value is IEnumerable)
						{
							IEnumerable enumerable = (IEnumerable)value;
							int num = 0;
							IEnumerator enumerator2 = enumerable.GetEnumerator();
							try
							{
								while (enumerator2.MoveNext())
								{
									object current2 = enumerator2.Current;
									if (current2 != null && !(current2 is Def) && GenTypes.IsCustomType(current2.GetType()))
									{
										string text = TranslationHandleUtility.GetBestHandleWithIndexForListElement(enumerable, current2);
										if (text.NullOrEmpty())
										{
											text = num.ToString();
										}
										string curNormalizedPath2 = curNormalizedPath + "." + item.Name + "." + num;
										string curSuggestedPath2 = curSuggestedPath + "." + item.Name + "." + text;
										ForEachPossibleDefInjectionInDefRecursive(current2, curNormalizedPath2, curSuggestedPath2, visited, flag, def, action);
									}
									num++;
								}
							}
							finally
							{
								IDisposable disposable;
								if ((disposable = (enumerator2 as IDisposable)) != null)
								{
									disposable.Dispose();
								}
							}
						}
						else if (value != null && GenTypes.IsCustomType(value.GetType()))
						{
							string curNormalizedPath3 = curNormalizedPath + "." + item.Name;
							string curSuggestedPath3 = curSuggestedPath + "." + item.Name;
							ForEachPossibleDefInjectionInDefRecursive(value, curNormalizedPath3, curSuggestedPath3, visited, flag, def, action);
						}
					}
				}
			}
		}

		public static bool ShouldCheckMissingInjection(string str, FieldInfo fi, Def def)
		{
			if (def.generated)
			{
				return false;
			}
			if (str.NullOrEmpty())
			{
				return false;
			}
			if (fi.HasAttribute<NoTranslateAttribute>() || fi.HasAttribute<UnsavedAttribute>() || fi.HasAttribute<MayTranslateAttribute>())
			{
				return false;
			}
			return fi.HasAttribute<MustTranslateAttribute>() || str.Contains(' ');
		}

		private static IEnumerable<FieldInfo> FieldsInDeterministicOrder(IEnumerable<FieldInfo> fields)
		{
			return from x in fields
			orderby x.HasAttribute<UnsavedAttribute>() || x.HasAttribute<NoTranslateAttribute>(), x.Name == "label" descending, x.Name == "description" descending, x.Name
			select x;
		}
	}
}
