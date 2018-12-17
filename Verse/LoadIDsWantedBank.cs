using System;
using System.Collections.Generic;
using System.Text;

namespace Verse
{
	public class LoadIDsWantedBank
	{
		private struct IdRecord
		{
			public string targetLoadID;

			public Type targetType;

			public string pathRelToParent;

			public IExposable parent;

			public IdRecord(string targetLoadID, Type targetType, string pathRelToParent, IExposable parent)
			{
				this.targetLoadID = targetLoadID;
				this.targetType = targetType;
				this.pathRelToParent = pathRelToParent;
				this.parent = parent;
			}
		}

		private struct IdListRecord
		{
			public List<string> targetLoadIDs;

			public string pathRelToParent;

			public IExposable parent;

			public IdListRecord(List<string> targetLoadIDs, string pathRelToParent, IExposable parent)
			{
				this.targetLoadIDs = targetLoadIDs;
				this.pathRelToParent = pathRelToParent;
				this.parent = parent;
			}
		}

		private List<IdRecord> idsRead = new List<IdRecord>();

		private List<IdListRecord> idListsRead = new List<IdListRecord>();

		public void ConfirmClear()
		{
			if (idsRead.Count > 0 || idListsRead.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Not all loadIDs which were read were consumed.");
				if (idsRead.Count > 0)
				{
					stringBuilder.AppendLine("Singles:");
					for (int i = 0; i < idsRead.Count; i++)
					{
						StringBuilder stringBuilder2 = stringBuilder;
						object[] obj = new object[8]
						{
							"  ",
							null,
							null,
							null,
							null,
							null,
							null,
							null
						};
						IdRecord idRecord = idsRead[i];
						obj[1] = idRecord.targetLoadID.ToStringSafe();
						obj[2] = " of type ";
						IdRecord idRecord2 = idsRead[i];
						obj[3] = idRecord2.targetType;
						obj[4] = ". pathRelToParent=";
						IdRecord idRecord3 = idsRead[i];
						obj[5] = idRecord3.pathRelToParent;
						obj[6] = ", parent=";
						IdRecord idRecord4 = idsRead[i];
						obj[7] = idRecord4.parent.ToStringSafe();
						stringBuilder2.AppendLine(string.Concat(obj));
					}
				}
				if (idListsRead.Count > 0)
				{
					stringBuilder.AppendLine("Lists:");
					for (int j = 0; j < idListsRead.Count; j++)
					{
						StringBuilder stringBuilder3 = stringBuilder;
						object[] obj2 = new object[6]
						{
							"  List with ",
							null,
							null,
							null,
							null,
							null
						};
						IdListRecord idListRecord = idListsRead[j];
						int num;
						if (idListRecord.targetLoadIDs != null)
						{
							IdListRecord idListRecord2 = idListsRead[j];
							num = idListRecord2.targetLoadIDs.Count;
						}
						else
						{
							num = 0;
						}
						obj2[1] = num;
						obj2[2] = " elements. pathRelToParent=";
						IdListRecord idListRecord3 = idListsRead[j];
						obj2[3] = idListRecord3.pathRelToParent;
						obj2[4] = ", parent=";
						IdListRecord idListRecord4 = idListsRead[j];
						obj2[5] = idListRecord4.parent.ToStringSafe();
						stringBuilder3.AppendLine(string.Concat(obj2));
					}
				}
				Log.Warning(stringBuilder.ToString().TrimEndNewlines());
			}
			Clear();
		}

		public void Clear()
		{
			idsRead.Clear();
			idListsRead.Clear();
		}

		public void RegisterLoadIDReadFromXml(string targetLoadID, Type targetType, string pathRelToParent, IExposable parent)
		{
			for (int i = 0; i < idsRead.Count; i++)
			{
				IdRecord idRecord = idsRead[i];
				if (idRecord.parent == parent)
				{
					IdRecord idRecord2 = idsRead[i];
					if (idRecord2.pathRelToParent == pathRelToParent)
					{
						Log.Error("Tried to register the same load ID twice: " + targetLoadID + ", pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
						return;
					}
				}
			}
			idsRead.Add(new IdRecord(targetLoadID, targetType, pathRelToParent, parent));
		}

		public void RegisterLoadIDReadFromXml(string targetLoadID, Type targetType, string toAppendToPathRelToParent)
		{
			string text = Scribe.loader.curPathRelToParent;
			if (!toAppendToPathRelToParent.NullOrEmpty())
			{
				text = text + '/' + toAppendToPathRelToParent;
			}
			RegisterLoadIDReadFromXml(targetLoadID, targetType, text, Scribe.loader.curParent);
		}

		public void RegisterLoadIDListReadFromXml(List<string> targetLoadIDList, string pathRelToParent, IExposable parent)
		{
			for (int i = 0; i < idListsRead.Count; i++)
			{
				IdListRecord idListRecord = idListsRead[i];
				if (idListRecord.parent == parent)
				{
					IdListRecord idListRecord2 = idListsRead[i];
					if (idListRecord2.pathRelToParent == pathRelToParent)
					{
						Log.Error("Tried to register the same list of load IDs twice. pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
						return;
					}
				}
			}
			idListsRead.Add(new IdListRecord(targetLoadIDList, pathRelToParent, parent));
		}

		public void RegisterLoadIDListReadFromXml(List<string> targetLoadIDList, string toAppendToPathRelToParent)
		{
			string text = Scribe.loader.curPathRelToParent;
			if (!toAppendToPathRelToParent.NullOrEmpty())
			{
				text = text + '/' + toAppendToPathRelToParent;
			}
			RegisterLoadIDListReadFromXml(targetLoadIDList, text, Scribe.loader.curParent);
		}

		public string Take<T>(string pathRelToParent, IExposable parent)
		{
			for (int i = 0; i < idsRead.Count; i++)
			{
				IdRecord idRecord = idsRead[i];
				if (idRecord.parent == parent)
				{
					IdRecord idRecord2 = idsRead[i];
					if (idRecord2.pathRelToParent == pathRelToParent)
					{
						IdRecord idRecord3 = idsRead[i];
						string targetLoadID = idRecord3.targetLoadID;
						Type typeFromHandle = typeof(T);
						IdRecord idRecord4 = idsRead[i];
						if (typeFromHandle != idRecord4.targetType)
						{
							object[] obj = new object[8]
							{
								"Trying to get load ID of object of type ",
								typeof(T),
								", but it was registered as ",
								null,
								null,
								null,
								null,
								null
							};
							IdRecord idRecord5 = idsRead[i];
							obj[3] = idRecord5.targetType;
							obj[4] = ". pathRelToParent=";
							obj[5] = pathRelToParent;
							obj[6] = ", parent=";
							obj[7] = parent.ToStringSafe();
							Log.Error(string.Concat(obj));
						}
						idsRead.RemoveAt(i);
						return targetLoadID;
					}
				}
			}
			Log.Error("Could not get load ID. We're asking for something which was never added during LoadingVars. pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
			return null;
		}

		public List<string> TakeList(string pathRelToParent, IExposable parent)
		{
			for (int i = 0; i < idListsRead.Count; i++)
			{
				IdListRecord idListRecord = idListsRead[i];
				if (idListRecord.parent == parent)
				{
					IdListRecord idListRecord2 = idListsRead[i];
					if (idListRecord2.pathRelToParent == pathRelToParent)
					{
						IdListRecord idListRecord3 = idListsRead[i];
						List<string> targetLoadIDs = idListRecord3.targetLoadIDs;
						idListsRead.RemoveAt(i);
						return targetLoadIDs;
					}
				}
			}
			Log.Error("Could not get load IDs list. We're asking for something which was never added during LoadingVars. pathRelToParent=" + pathRelToParent + ", parent=" + parent.ToStringSafe());
			return new List<string>();
		}
	}
}
