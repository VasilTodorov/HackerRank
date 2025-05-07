using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeterminingDNAHealth
{
    // No real gain here probably still better not to use lists but there would be needed some thinkering and those are collections that can have 1 element and can have 1000
    public class GeneInfo2
    {
        public List<int> Positions = new();      // indices where this gene appears
        public List<long> Healths = new();       // health at those positions
        public List<long> PrefixSums = new();    // prefix sums for fast range queries

        public static Dictionary<string, GeneInfo2> GenerateGeneMap(string[] genes, int[] health)
        {
            var geneMap = new Dictionary<string, GeneInfo2>();

            for (int i = 0; i < genes.Length; i++)
            {
                string gene = genes[i];
                if (!geneMap.ContainsKey(gene))
                    geneMap[gene] = new GeneInfo2();

                geneMap[gene].Positions.Add(i);
                geneMap[gene].Healths.Add(health[i]);
            }

            foreach (var info in geneMap.Values)
            {
                long sum = 0;
                foreach (var h in info.Healths)
                {
                    sum += h;
                    info.PrefixSums.Add(sum);
                }
            }

            return geneMap;
        }
        public static int LowerBound(List<int> list, int target)
        {
            int l = 0, r = list.Count - 1, ans = -1;
            while (l <= r)
            {
                int m = (l + r) / 2;
                if (list[m] >= target)
                {
                    ans = m;
                    r = m - 1;
                }
                else
                {
                    l = m + 1;
                }
            }
            return ans;
        }

        public static int UpperBound(List<int> list, int target)
        {
            int l = 0, r = list.Count - 1, ans = -1;
            while (l <= r)
            {
                int m = (l + r) / 2;
                if (list[m] <= target)
                {
                    ans = m;
                    l = m + 1;
                }
                else
                {
                    r = m - 1;
                }
            }
            return ans;
        }
    }
}
