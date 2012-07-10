using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace cqa_medical.DataInput
{
 
    public class DistributionCreator<T>
    {
        private SortedDictionary<T, int> data;

        public void CreateFrom(params T[] input)
        {
            data = new SortedDictionary<T, int>();
            foreach (var v in input)
            {
                if (data.ContainsKey(v))
                    data[v]++;
                else
                    data[v] = 1;
            }
        }

        public SortedDictionary<T, int> GetData()
        {
            return this.data;
        }


        [TestFixture]
        public class DistributionCreatorTest
        {
            [Test]
            public void Test()
            {
                var d = new DistributionCreator<int>();
                d.CreateFrom(1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5);
                var ans = d.GetData();
                Assert.AreEqual(ans[1], 1);
                Assert.AreEqual(ans[2], 2);
                Assert.AreEqual(ans[3], 4);
                Assert.AreEqual(ans[4], 4);
                Assert.AreEqual(ans[5], 1);
            }

            
        }
    }
}
