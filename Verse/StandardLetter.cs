using System.Collections.Generic;

namespace Verse
{
	public class StandardLetter : ChoiceLetter
	{
		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				yield return base.Option_Close;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
