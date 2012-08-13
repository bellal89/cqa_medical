using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace cqa_medical.Utilits
{
	public class DistributionCreator<T>
	{
		private readonly SortedDictionary<T, int> data = new SortedDictionary<T, int>();

		public DistributionCreator(IEnumerable<T> input)
		{
			AddData(input);
		}

		public void AddData(IEnumerable<T> input)
		{
			foreach (T v in input)
			{
				if (data.ContainsKey(v))
					data[v]++;
				else
					data[v] = 1;
			}
		}

		public SortedDictionary<T, int> GetData()
		{
			return data;
		}
	}




	[TestFixture]
	public class DistributionCreatorTest
	{
		[Test]
		public void Test()
		{
			var d = new DistributionCreator<int>(new[]{1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5});
			var ans = d.GetData();
			Assert.AreEqual(ans[1], 1);
			Assert.AreEqual(ans[2], 2);
			Assert.AreEqual(ans[3], 4);
			Assert.AreEqual(ans[4], 4);
			Assert.AreEqual(ans[5], 1);
			Console.WriteLine(d);
		}
	}
}