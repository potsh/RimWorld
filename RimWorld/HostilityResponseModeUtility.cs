using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class HostilityResponseModeUtility
	{
		private static readonly Texture2D IgnoreIcon = ContentFinder<Texture2D>.Get("UI/Icons/HostilityResponse/Ignore");

		private static readonly Texture2D AttackIcon = ContentFinder<Texture2D>.Get("UI/Icons/HostilityResponse/Attack");

		private static readonly Texture2D FleeIcon = ContentFinder<Texture2D>.Get("UI/Icons/HostilityResponse/Flee");

		[CompilerGenerated]
		private static Func<Pawn, HostilityResponseMode> _003C_003Ef__mg_0024cache0;

		[CompilerGenerated]
		private static Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<HostilityResponseMode>>> _003C_003Ef__mg_0024cache1;

		public static Texture2D GetIcon(this HostilityResponseMode response)
		{
			switch (response)
			{
			case HostilityResponseMode.Ignore:
				return IgnoreIcon;
			case HostilityResponseMode.Attack:
				return AttackIcon;
			case HostilityResponseMode.Flee:
				return FleeIcon;
			default:
				return BaseContent.BadTex;
			}
		}

		public static HostilityResponseMode GetNextResponse(Pawn pawn)
		{
			switch (pawn.playerSettings.hostilityResponse)
			{
			case HostilityResponseMode.Ignore:
				if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
				{
					return HostilityResponseMode.Flee;
				}
				return HostilityResponseMode.Attack;
			case HostilityResponseMode.Attack:
				return HostilityResponseMode.Flee;
			case HostilityResponseMode.Flee:
				return HostilityResponseMode.Ignore;
			default:
				return HostilityResponseMode.Ignore;
			}
		}

		public static string GetLabel(this HostilityResponseMode response)
		{
			return ("HostilityResponseMode_" + response).Translate();
		}

		public static void DrawResponseButton(Rect rect, Pawn pawn, bool paintable)
		{
			Func<Pawn, HostilityResponseMode> getPayload = DrawResponseButton_GetResponse;
			Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<HostilityResponseMode>>> menuGenerator = DrawResponseButton_GenerateMenu;
			Texture2D icon = pawn.playerSettings.hostilityResponse.GetIcon();
			Widgets.Dropdown(rect, pawn, getPayload, menuGenerator, null, icon, null, null, delegate
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.HostilityResponse, KnowledgeAmount.SpecificInteraction);
			}, paintable);
			UIHighlighter.HighlightOpportunity(rect, "HostilityResponse");
			TooltipHandler.TipRegion(rect, "HostilityReponseTip".Translate() + "\n\n" + "HostilityResponseCurrentMode".Translate() + ": " + pawn.playerSettings.hostilityResponse.GetLabel());
		}

		private static HostilityResponseMode DrawResponseButton_GetResponse(Pawn pawn)
		{
			return pawn.playerSettings.hostilityResponse;
		}

		private static IEnumerable<Widgets.DropdownMenuElement<HostilityResponseMode>> DrawResponseButton_GenerateMenu(Pawn p)
		{
			IEnumerator enumerator = Enum.GetValues(typeof(HostilityResponseMode)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					_003CDrawResponseButton_GenerateMenu_003Ec__Iterator0 _003CDrawResponseButton_GenerateMenu_003Ec__Iterator = (_003CDrawResponseButton_GenerateMenu_003Ec__Iterator0)/*Error near IL_007e: stateMachine*/;
					HostilityResponseMode response = (HostilityResponseMode)enumerator.Current;
					if (response != HostilityResponseMode.Attack || !p.story.WorkTagIsDisabled(WorkTags.Violent))
					{
						yield return new Widgets.DropdownMenuElement<HostilityResponseMode>
						{
							option = new FloatMenuOption(response.GetLabel(), delegate
							{
								p.playerSettings.hostilityResponse = response;
							}),
							payload = response
						};
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable2 = disposable = (enumerator as IDisposable);
				if (disposable != null)
				{
					disposable2.Dispose();
				}
			}
			yield break;
			IL_0196:
			/*Error near IL_0197: Unexpected return in MoveNext()*/;
		}
	}
}
