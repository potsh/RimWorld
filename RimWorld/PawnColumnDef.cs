using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnDef : Def
	{
		public Type workerClass = typeof(PawnColumnWorker);

		public bool sortable;

		public bool ignoreWhenCalculatingOptimalTableSize;

		[NoTranslate]
		public string headerIcon;

		public Vector2 headerIconSize;

		[MustTranslate]
		public string headerTip;

		public bool headerAlwaysInteractable;

		public bool paintable;

		public TrainableDef trainable;

		public int gap;

		public WorkTypeDef workType;

		public bool moveWorkTypeLabelDown;

		public int widthPriority;

		public int width = -1;

		[Unsaved]
		private PawnColumnWorker workerInt;

		[Unsaved]
		private Texture2D headerIconTex;

		public PawnColumnWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (PawnColumnWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public Texture2D HeaderIcon
		{
			get
			{
				if (headerIconTex == null && !headerIcon.NullOrEmpty())
				{
					headerIconTex = ContentFinder<Texture2D>.Get(headerIcon);
				}
				return headerIconTex;
			}
		}

		public Vector2 HeaderIconSize
		{
			get
			{
				if (headerIconSize != default(Vector2))
				{
					return headerIconSize;
				}
				Texture2D texture2D = HeaderIcon;
				if (texture2D != null)
				{
					return new Vector2((float)texture2D.width, (float)texture2D.height);
				}
				return Vector2.zero;
			}
		}

		public bool HeaderInteractable => sortable || !headerTip.NullOrEmpty() || headerAlwaysInteractable;
	}
}
