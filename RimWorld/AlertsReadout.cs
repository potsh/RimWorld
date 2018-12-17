using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class AlertsReadout
	{
		private List<Alert> activeAlerts = new List<Alert>(16);

		private int curAlertIndex;

		private float lastFinalY;

		private int mouseoverAlertIndex = -1;

		private readonly List<Alert> AllAlerts = new List<Alert>();

		private const int StartTickDelay = 600;

		public const float AlertListWidth = 164f;

		private static int AlertCycleLength = 20;

		private readonly List<AlertPriority> PriosInDrawOrder;

		public AlertsReadout()
		{
			AllAlerts.Clear();
			foreach (Type item2 in typeof(Alert).AllLeafSubclasses())
			{
				AllAlerts.Add((Alert)Activator.CreateInstance(item2));
			}
			if (PriosInDrawOrder == null)
			{
				PriosInDrawOrder = new List<AlertPriority>();
				IEnumerator enumerator2 = Enum.GetValues(typeof(AlertPriority)).GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						AlertPriority item = (AlertPriority)enumerator2.Current;
						PriosInDrawOrder.Add(item);
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
				PriosInDrawOrder.Reverse();
			}
		}

		public void AlertsReadoutUpdate()
		{
			if (Mathf.Max(Find.TickManager.TicksGame, Find.TutorialState.endTick) >= 600)
			{
				if (Find.Storyteller.def.disableAlerts)
				{
					activeAlerts.Clear();
				}
				else
				{
					curAlertIndex++;
					if (curAlertIndex >= AlertCycleLength)
					{
						curAlertIndex = 0;
					}
					for (int i = curAlertIndex; i < AllAlerts.Count; i += AlertCycleLength)
					{
						Alert alert = AllAlerts[i];
						try
						{
							if (alert.Active)
							{
								if (!activeAlerts.Contains(alert))
								{
									activeAlerts.Add(alert);
									alert.Notify_Started();
								}
							}
							else
							{
								for (int j = 0; j < activeAlerts.Count; j++)
								{
									if (activeAlerts[j] == alert)
									{
										activeAlerts.RemoveAt(j);
										break;
									}
								}
							}
						}
						catch (Exception ex)
						{
							Log.ErrorOnce("Exception processing alert " + alert.ToString() + ": " + ex.ToString(), 743575);
							if (activeAlerts.Contains(alert))
							{
								activeAlerts.Remove(alert);
							}
						}
					}
					for (int num = activeAlerts.Count - 1; num >= 0; num--)
					{
						Alert alert2 = activeAlerts[num];
						try
						{
							activeAlerts[num].AlertActiveUpdate();
						}
						catch (Exception ex2)
						{
							Log.ErrorOnce("Exception updating alert " + alert2.ToString() + ": " + ex2.ToString(), 743575);
							activeAlerts.RemoveAt(num);
						}
					}
					if (mouseoverAlertIndex >= 0 && mouseoverAlertIndex < activeAlerts.Count)
					{
						AlertReport report = activeAlerts[mouseoverAlertIndex].GetReport();
						IEnumerable<GlobalTargetInfo> culprits = report.culprits;
						if (culprits != null)
						{
							foreach (GlobalTargetInfo item in culprits)
							{
								TargetHighlighter.Highlight(item);
							}
						}
					}
					mouseoverAlertIndex = -1;
				}
			}
		}

		public void AlertsReadoutOnGUI()
		{
			if (Event.current.type != EventType.Layout && Event.current.type != EventType.MouseDrag && activeAlerts.Count != 0)
			{
				Alert alert = null;
				AlertPriority alertPriority = AlertPriority.Critical;
				bool flag = false;
				float num = Find.LetterStack.LastTopY - (float)activeAlerts.Count * 28f;
				Rect rect = new Rect((float)UI.screenWidth - 154f, num, 154f, lastFinalY - num);
				float num2 = GenUI.BackgroundDarkAlphaForText();
				if (num2 > 0.001f)
				{
					GUI.color = new Color(1f, 1f, 1f, num2);
					Widgets.DrawShadowAround(rect);
					GUI.color = Color.white;
				}
				float num3 = num;
				if (num3 < 0f)
				{
					num3 = 0f;
				}
				for (int i = 0; i < PriosInDrawOrder.Count; i++)
				{
					AlertPriority alertPriority2 = PriosInDrawOrder[i];
					for (int j = 0; j < activeAlerts.Count; j++)
					{
						Alert alert2 = activeAlerts[j];
						if (alert2.Priority == alertPriority2)
						{
							if (!flag)
							{
								alertPriority = alertPriority2;
								flag = true;
							}
							Rect rect2 = alert2.DrawAt(num3, alertPriority2 != alertPriority);
							if (Mouse.IsOver(rect2))
							{
								alert = alert2;
								mouseoverAlertIndex = j;
							}
							num3 += rect2.height;
						}
					}
				}
				lastFinalY = num3;
				UIHighlighter.HighlightOpportunity(rect, "Alerts");
				if (alert != null)
				{
					alert.DrawInfoPane();
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Alerts, KnowledgeAmount.FrameDisplayed);
				}
			}
		}
	}
}
