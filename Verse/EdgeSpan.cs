using System.Collections.Generic;

namespace Verse
{
	public struct EdgeSpan
	{
		public IntVec3 root;

		public SpanDirection dir;

		public int length;

		public bool IsValid => length > 0;

		public IEnumerable<IntVec3> Cells
		{
			get
			{
				int i = 0;
				while (true)
				{
					if (i >= length)
					{
						yield break;
					}
					if (dir == SpanDirection.North)
					{
						yield return new IntVec3(root.x, 0, root.z + i);
						/*Error: Unable to find new state assignment for yield return*/;
					}
					if (dir == SpanDirection.East)
					{
						break;
					}
					i++;
				}
				yield return new IntVec3(root.x + i, 0, root.z);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public EdgeSpan(IntVec3 root, SpanDirection dir, int length)
		{
			this.root = root;
			this.dir = dir;
			this.length = length;
		}

		public override string ToString()
		{
			return "(root=" + root + ", dir=" + dir.ToString() + " + length=" + length + ")";
		}

		public ulong UniqueHashCode()
		{
			ulong num = root.UniqueHashCode();
			if (dir == SpanDirection.East)
			{
				num += 17592186044416L;
			}
			return (ulong)((long)num + 281474976710656L * length);
		}
	}
}
