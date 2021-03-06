﻿// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project LibTest
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

using System;
using System.Collections.Generic;

namespace Cranium.Lib.Test.SupportClasses
{
	public static class Misc
	{
		public static void Shuffle<T>(this IList<T> list)
		{
			Random rng = new Random();
			Int32 n = list.Count;
			while (n > 1)
			{
				n--;
				Int32 k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}