using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class UnfinishedThing : ThingWithComps
	{
		private Pawn creatorInt;

		private string creatorName = "ErrorCreatorName";

		private RecipeDef recipeInt;

		public List<Thing> ingredients = new List<Thing>();

		private Bill_ProductionWithUft boundBillInt;

		public float workLeft = -10000f;

		private const float CancelIngredientRecoveryFraction = 0.75f;

		public Pawn Creator
		{
			get
			{
				return creatorInt;
			}
			set
			{
				if (value == null)
				{
					Log.Error("Cannot set creator to null.");
				}
				else
				{
					creatorInt = value;
					creatorName = value.LabelShort;
				}
			}
		}

		public RecipeDef Recipe => recipeInt;

		public Bill_ProductionWithUft BoundBill
		{
			get
			{
				if (boundBillInt != null && (boundBillInt.DeletedOrDereferenced || boundBillInt.BoundUft != this))
				{
					boundBillInt = null;
				}
				return boundBillInt;
			}
			set
			{
				if (value != boundBillInt)
				{
					Bill_ProductionWithUft bill_ProductionWithUft = boundBillInt;
					boundBillInt = value;
					if (bill_ProductionWithUft != null && bill_ProductionWithUft.BoundUft == this)
					{
						bill_ProductionWithUft.SetBoundUft(null, setOtherLink: false);
					}
					if (value != null)
					{
						recipeInt = value.recipe;
						if (value.BoundUft != this)
						{
							value.SetBoundUft(this, setOtherLink: false);
						}
					}
				}
			}
		}

		public Thing BoundWorkTable
		{
			get
			{
				if (BoundBill == null)
				{
					return null;
				}
				IBillGiver billGiver = BoundBill.billStack.billGiver;
				Thing thing = billGiver as Thing;
				if (thing.Destroyed)
				{
					return null;
				}
				return thing;
			}
		}

		public override string LabelNoCount
		{
			get
			{
				if (Recipe == null)
				{
					return base.LabelNoCount;
				}
				if (base.Stuff == null)
				{
					return "UnfinishedItem".Translate(Recipe.products[0].thingDef.label);
				}
				return "UnfinishedItemWithStuff".Translate(base.Stuff.LabelAsStuff, Recipe.products[0].thingDef.label);
			}
		}

		public override string DescriptionDetailed
		{
			get
			{
				if (Recipe == null)
				{
					return base.LabelNoCount;
				}
				return Recipe.ProducedThingDef.DescriptionDetailed;
			}
		}

		public override string DescriptionFlavor
		{
			get
			{
				if (Recipe == null)
				{
					return base.LabelNoCount;
				}
				return Recipe.ProducedThingDef.description;
			}
		}

		public bool Initialized => workLeft > -5000f;

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.Saving && boundBillInt != null && boundBillInt.DeletedOrDereferenced)
			{
				boundBillInt = null;
			}
			Scribe_References.Look(ref creatorInt, "creator");
			Scribe_Values.Look(ref creatorName, "creatorName");
			Scribe_References.Look(ref boundBillInt, "bill");
			Scribe_Defs.Look(ref recipeInt, "recipe");
			Scribe_Values.Look(ref workLeft, "workLeft", 0f);
			Scribe_Collections.Look(ref ingredients, "ingredients", LookMode.Deep);
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (mode == DestroyMode.Cancel)
			{
				for (int i = 0; i < ingredients.Count; i++)
				{
					int num = GenMath.RoundRandom((float)ingredients[i].stackCount * 0.75f);
					if (num > 0)
					{
						ingredients[i].stackCount = num;
						GenPlace.TryPlaceThing(ingredients[i], base.Position, base.Map, ThingPlaceMode.Near);
					}
				}
				ingredients.Clear();
			}
			base.Destroy(mode);
			BoundBill = null;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo c = enumerator.Current;
					yield return c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield return (Gizmo)new Command_Action
			{
				defaultLabel = "CommandCancelConstructionLabel".Translate(),
				defaultDesc = "CommandCancelConstructionDesc".Translate(),
				icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),
				hotKey = KeyBindingDefOf.Designator_Cancel,
				action = delegate
				{
					((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_0115: stateMachine*/)._0024this.Destroy(DestroyMode.Cancel);
				}
			};
			/*Error: Unable to find new state assignment for yield return*/;
			IL_014f:
			/*Error near IL_0150: Unexpected return in MoveNext()*/;
		}

		public Bill_ProductionWithUft BillOnTableForMe(Thing workTable)
		{
			if (Recipe.AllRecipeUsers.Contains(workTable.def))
			{
				IBillGiver billGiver = (IBillGiver)workTable;
				for (int i = 0; i < billGiver.BillStack.Count; i++)
				{
					Bill_ProductionWithUft bill_ProductionWithUft = billGiver.BillStack[i] as Bill_ProductionWithUft;
					if (bill_ProductionWithUft != null && bill_ProductionWithUft.ShouldDoNow() && bill_ProductionWithUft != null && bill_ProductionWithUft.recipe == Recipe)
					{
						return bill_ProductionWithUft;
					}
				}
			}
			return null;
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (BoundWorkTable != null)
			{
				GenDraw.DrawLineBetween(this.TrueCenter(), BoundWorkTable.TrueCenter());
			}
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			text = text + "Author".Translate() + ": " + creatorName;
			string text2 = text;
			return text2 + "\n" + "WorkLeft".Translate() + ": " + workLeft.ToStringWorkAmount();
		}
	}
}
