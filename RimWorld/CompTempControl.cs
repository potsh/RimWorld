using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompTempControl : ThingComp
	{
		[Unsaved]
		public bool operatingAtHighPower;

		public float targetTemperature = -99999f;

		private const float DefaultTargetTemperature = 21f;

		public CompProperties_TempControl Props => (CompProperties_TempControl)props;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (targetTemperature < -2000f)
			{
				targetTemperature = Props.defaultTargetTemperature;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref targetTemperature, "targetTemperature", 0f);
		}

		private float RoundedToCurrentTempModeOffset(float celsiusTemp)
		{
			float f = GenTemperature.CelsiusToOffset(celsiusTemp, Prefs.TemperatureMode);
			f = (float)Mathf.RoundToInt(f);
			return GenTemperature.ConvertTemperatureOffset(f, Prefs.TemperatureMode, TemperatureDisplayMode.Celsius);
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
			_003CCompGetGizmosExtra_003Ec__Iterator0 _003CCompGetGizmosExtra_003Ec__Iterator = (_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_00d5: stateMachine*/;
			float offset = RoundedToCurrentTempModeOffset(-10f);
			yield return (Gizmo)new Command_Action
			{
				action = delegate
				{
					_003CCompGetGizmosExtra_003Ec__Iterator._0024this.InterfaceChangeTargetTemperature(offset);
				},
				defaultLabel = offset.ToStringTemperatureOffset("F0"),
				defaultDesc = "CommandLowerTempDesc".Translate(),
				hotKey = KeyBindingDefOf.Misc5,
				icon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower")
			};
			/*Error: Unable to find new state assignment for yield return*/;
			IL_04c3:
			/*Error near IL_04c4: Unexpected return in MoveNext()*/;
		}

		private void InterfaceChangeTargetTemperature(float offset)
		{
			if (offset > 0f)
			{
				SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
			}
			targetTemperature += offset;
			targetTemperature = Mathf.Clamp(targetTemperature, -273.15f, 2000f);
			ThrowCurrentTemperatureText();
		}

		private void ThrowCurrentTemperatureText()
		{
			MoteMaker.ThrowText(parent.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), parent.Map, targetTemperature.ToStringTemperature("F0"), Color.white);
		}

		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("TargetTemperature".Translate() + ": ");
			stringBuilder.AppendLine(targetTemperature.ToStringTemperature("F0"));
			stringBuilder.Append("PowerConsumptionMode".Translate() + ": ");
			if (operatingAtHighPower)
			{
				stringBuilder.Append("PowerConsumptionHigh".Translate());
			}
			else
			{
				stringBuilder.Append("PowerConsumptionLow".Translate());
			}
			return stringBuilder.ToString();
		}
	}
}
