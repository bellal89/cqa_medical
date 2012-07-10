using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using System.Linq;

namespace cqa_medical.DataInput
{
	public class DistributionCreator<T>
	{
		private readonly SortedDictionary<T, int> data = new SortedDictionary<T, int>();

		public DistributionCreator(IEnumerable<T> input)
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

		public override string ToString()
		{
			return string.Join(Environment.NewLine, data.Keys.Select(k => k.ToString() + "\t" + data[k].ToString(CultureInfo.InvariantCulture)).ToArray());
		}
	}


	[TestFixture]
	public class DistributionCreatorTest
	{
		[Test]
		public void Test()
		{
			var d = new DistributionCreator<int>(new[]{1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5});
			SortedDictionary<int, int> ans = d.GetData();
			Assert.AreEqual(ans[1], 1);
			Assert.AreEqual(ans[2], 2);
			Assert.AreEqual(ans[3], 4);
			Assert.AreEqual(ans[4], 4);
			Assert.AreEqual(ans[5], 1);
			Console.WriteLine(d);
		}
	}
}