using System;
using System.Linq;
using System.Reflection;
using Verse;

namespace RimWorld
{
	public static class DefOfHelper
	{
		private static bool bindingNow;

		private static bool earlyTry = true;

		public static void RebindAllDefOfs(bool earlyTryMode)
		{
			earlyTry = earlyTryMode;
			bindingNow = true;
			try
			{
				foreach (Type item in GenTypes.AllTypesWithAttribute<DefOf>())
				{
					BindDefsFor(item);
				}
			}
			finally
			{
				bindingNow = false;
			}
		}

		private static void BindDefsFor(Type type)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				Type fieldType = fieldInfo.FieldType;
				if (!typeof(Def).IsAssignableFrom(fieldType))
				{
					Log.Error(fieldType + " is not a Def.");
				}
				else if (fieldType == typeof(SoundDef))
				{
					SoundDef soundDef = SoundDef.Named(fieldInfo.Name);
					if (soundDef.isUndefined && !earlyTry)
					{
						Log.Error("Could not find SoundDef named " + fieldInfo.Name);
					}
					fieldInfo.SetValue(null, soundDef);
				}
				else
				{
					Def def = GenDefDatabase.GetDef(fieldType, fieldInfo.Name, !earlyTry);
					fieldInfo.SetValue(null, def);
				}
			}
		}

		public static void EnsureInitializedInCtor(Type defOf)
		{
			if (!bindingNow)
			{
				string text = DirectXmlToObject.currentlyInstantiatingObjectOfType.Any() ? ("DirectXmlToObject is currently instantiating an object of type " + DirectXmlToObject.currentlyInstantiatingObjectOfType.Peek()) : ((Scribe.mode != LoadSaveMode.LoadingVars) ? string.Empty : ("curParent=" + Scribe.loader.curParent.ToStringSafe() + " curPathRelToParent=" + Scribe.loader.curPathRelToParent));
				Log.Warning("Tried to use an uninitialized DefOf of type " + defOf.Name + ". DefOfs are initialized right after all defs all loaded. Uninitialized DefOfs will return only nulls. (hint: don't use DefOfs as default field values in Defs, try to resolve them in ResolveReferences() instead)" + ((!text.NullOrEmpty()) ? (" Debug info: " + text) : string.Empty));
			}
		}
	}
}
