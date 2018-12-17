using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class DefGenerator
	{
		public static void GenerateImpliedDefs_PreResolve()
		{
			IEnumerable<ThingDef> enumerable = ThingDefGenerator_Buildings.ImpliedBlueprintAndFrameDefs().Concat(ThingDefGenerator_Meat.ImpliedMeatDefs()).Concat(ThingDefGenerator_Corpses.ImpliedCorpseDefs());
			foreach (ThingDef item in enumerable)
			{
				AddImpliedDef(item);
			}
			DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.Silent);
			foreach (TerrainDef item2 in TerrainDefGenerator_Stone.ImpliedTerrainDefs())
			{
				AddImpliedDef(item2);
			}
			foreach (RecipeDef item3 in RecipeDefGenerator.ImpliedRecipeDefs())
			{
				AddImpliedDef(item3);
			}
			foreach (PawnColumnDef item4 in PawnColumnDefgenerator.ImpliedPawnColumnDefs())
			{
				AddImpliedDef(item4);
			}
		}

		public static void GenerateImpliedDefs_PostResolve()
		{
			foreach (KeyBindingCategoryDef item in KeyBindingDefGenerator.ImpliedKeyBindingCategoryDefs())
			{
				AddImpliedDef(item);
			}
			foreach (KeyBindingDef item2 in KeyBindingDefGenerator.ImpliedKeyBindingDefs())
			{
				AddImpliedDef(item2);
			}
		}

		public static void AddImpliedDef<T>(T def) where T : Def, new()
		{
			def.generated = true;
			if (def.modContentPack == null)
			{
				Log.Error($"Added def {def.GetType()}:{def.defName} without an associated modContentPack");
			}
			else
			{
				def.modContentPack.AddImpliedDef(def);
			}
			def.PostLoad();
			DefDatabase<T>.Add(def);
		}
	}
}
