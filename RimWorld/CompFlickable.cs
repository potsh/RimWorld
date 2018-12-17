using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompFlickable : ThingComp
	{
		private bool switchOnInt = true;

		private bool wantSwitchOn = true;

		private Graphic offGraphic;

		private Texture2D cachedCommandTex;

		private const string OffGraphicSuffix = "_Off";

		public const string FlickedOnSignal = "FlickedOn";

		public const string FlickedOffSignal = "FlickedOff";

		private CompProperties_Flickable Props => (CompProperties_Flickable)props;

		private Texture2D CommandTex
		{
			get
			{
				if (cachedCommandTex == null)
				{
					cachedCommandTex = ContentFinder<Texture2D>.Get(Props.commandTexture);
				}
				return cachedCommandTex;
			}
		}

		public bool SwitchIsOn
		{
			get
			{
				return switchOnInt;
			}
			set
			{
				if (switchOnInt != value)
				{
					switchOnInt = value;
					if (switchOnInt)
					{
						parent.BroadcastCompSignal("FlickedOn");
					}
					else
					{
						parent.BroadcastCompSignal("FlickedOff");
					}
					if (parent.Spawned)
					{
						parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
					}
				}
			}
		}

		public Graphic CurrentGraphic
		{
			get
			{
				if (SwitchIsOn)
				{
					return parent.DefaultGraphic;
				}
				if (offGraphic == null)
				{
					offGraphic = GraphicDatabase.Get(parent.def.graphicData.graphicClass, parent.def.graphicData.texPath + "_Off", parent.def.graphicData.shaderType.Shader, parent.def.graphicData.drawSize, parent.DrawColor, parent.DrawColorTwo);
				}
				return offGraphic;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref switchOnInt, "switchOn", defaultValue: true);
			Scribe_Values.Look(ref wantSwitchOn, "wantSwitchOn", defaultValue: true);
		}

		public bool WantsFlick()
		{
			return wantSwitchOn != switchOnInt;
		}

		public void DoFlick()
		{
			SwitchIsOn = !SwitchIsOn;
			SoundDefOf.FlickSwitch.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
		}

		public void ResetToOn()
		{
			switchOnInt = true;
			wantSwitchOn = true;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			using (IEnumerator<Gizmo> enumerator = base.CompGetGizmosExtra().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo c = enumerator.Current;
					yield return c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (parent.Faction == Faction.OfPlayer)
			{
				yield return (Gizmo)new Command_Toggle
				{
					hotKey = KeyBindingDefOf.Command_TogglePower,
					icon = CommandTex,
					defaultLabel = Props.commandLabelKey.Translate(),
					defaultDesc = Props.commandDescKey.Translate(),
					isActive = (() => ((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_0145: stateMachine*/)._0024this.wantSwitchOn),
					toggleAction = delegate
					{
						((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_015c: stateMachine*/)._0024this.wantSwitchOn = !((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_015c: stateMachine*/)._0024this.wantSwitchOn;
						FlickUtility.UpdateFlickDesignation(((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_015c: stateMachine*/)._0024this.parent);
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0196:
			/*Error near IL_0197: Unexpected return in MoveNext()*/;
		}
	}
}
