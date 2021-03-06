using System;
using System.Collections.Generic;

namespace Verse
{
	public static class GenString
	{
		private static string[] numberStrings;

		static GenString()
		{
			numberStrings = new string[10000];
			for (int i = 0; i < 10000; i++)
			{
				numberStrings[i] = (i - 5000).ToString();
			}
		}

		public static string ToStringCached(this int num)
		{
			if (num < -4999)
			{
				return num.ToString();
			}
			if (num > 4999)
			{
				return num.ToString();
			}
			return numberStrings[num + 5000];
		}

		public static IEnumerable<string> SplitBy(this string str, int chunkLength)
		{
			if (!str.NullOrEmpty())
			{
				if (chunkLength < 1)
				{
					throw new ArgumentException();
				}
				int i = 0;
				if (i < str.Length)
				{
					if (chunkLength > str.Length - i)
					{
						chunkLength = str.Length - i;
					}
					yield return str.Substring(i, chunkLength);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}
	}
}
