using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace DeterminingDNAHealth
{

    // change outputs to list because it is a simpler structure then hashset that doesn't make checks on add
    // it was 100ms improvement
    // would be better if array, but this is too much work
    public class AhoCorasick6
    {
        private const int ALPHABET_SIZE = 26; // assuming lowercase a-z
        private readonly int[,] _goto;
        private readonly int[] fail;
        private readonly List<string>[] outputs;
        private int nodeCount;
        private Dictionary<string, GeneInfo2> geneMap;

        public AhoCorasick6(int maxPatternStates, Dictionary<string, GeneInfo2> geneMap)
        {
            this.geneMap = geneMap;
            _goto = new int[maxPatternStates, ALPHABET_SIZE];

            fail = new int[maxPatternStates];
            outputs = new List<string>[maxPatternStates];
            for (int i = 0; i < outputs.Length; i++)
                outputs[i] = new List<string>();

            nodeCount = 1; // root is 0
        }

        private int CharToIndex(char c) => c - 'a';

        public void AddPattern(string pattern)
        {
            int node = 0;
            foreach (char ch in pattern)
            {
         int index = CharToIndex(ch);
                       if (_goto[node,index] == 0)
                    _goto[node,index] = nodeCount++;
                node = _goto[node,index];
            }
            outputs[node].Add(pattern);
        }

        public void AddPatterns(HashSet<string> set)
        {
            foreach (var pattern in set)
            {
                int node = 0;
                foreach (char ch in pattern)
                {
                    int index = CharToIndex(ch);
                    if (_goto[node, index] == 0)
                        _goto[node, index] = nodeCount++;
                    node = _goto[node, index];
                }
                outputs[node].Add(pattern);
            }
        }

        public void Build()
        {
            var queue = new Queue<int>();
            for (int c = 0; c < ALPHABET_SIZE; c++)
            {
                if (_goto[0,c] != 0)
                {
                    fail[_goto[0,c]] = 0;
                    queue.Enqueue(_goto[0,c]);
                }
            }

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                for (int c = 0; c < ALPHABET_SIZE; c++)
                {
                    int next = _goto[current,c];
                    if (next == 0) continue;

                    int f = fail[current];
                    while (f != 0 && _goto[f,c] == 0)
                        f = fail[f];

                    if (_goto[f,c] != 0)
                        f = _goto[f,c];

                    fail[next] = f;
                    outputs[next].AddRange(outputs[f]);
                    queue.Enqueue(next);
                }
            }
        }

        public long SearchTotal(string text, int first, int last)
        {
            long total = 0;
            int node = 0;

            foreach (char ch in text)
            {
                int index = CharToIndex(ch);
                while (node != 0 && _goto[node,index] == 0)
                    node = fail[node];

                if (_goto[node,index] != 0)
                    node = _goto[node,index];

                foreach (string gene in outputs[node])
                {
                    var info = geneMap[gene];
                    var posList = info.Positions;
                    var prefix = info.PrefixSums;

                    int left = GeneInfo2.LowerBound(posList, first);
                    int right = GeneInfo2.UpperBound(posList, last);

                    if (left <= right && left != -1 && right != -1)
                    {
                        long sum = prefix[right] - (left > 0 ? prefix[left - 1] : 0);
                        total += sum;
                    }
                }
            }
            return total;
        }


        public (long min, long max) SearchTotalAggregate((int first, int last, string text)[] database)
        {
            long min = long.MaxValue;
            long max = long.MinValue;


            for (int i = 0; i < database.Length; i++)
            {
                int node = 0;
                long total = 0;
                var item = database[i];
                var text = item.text;
              
                foreach (char ch in text)
                {
                    int index = CharToIndex(ch);
                    while (node != 0 && _goto[node, index] == 0)
                        node = fail[node];

                    if (_goto[node, index] != 0)
                        node = _goto[node, index];
                    
                    foreach (string gene in outputs[node])
                    {
                        var info = geneMap[gene];
                        var posList = info.Positions;
                        var prefix = info.PrefixSums;

                        int left = GeneInfo2.LowerBound(posList, item.first);
                        int right = GeneInfo2.UpperBound(posList, item.last);

                        if (left <= right && left != -1 && right != -1)
                        {
                            long sum = prefix[right] - (left > 0 ? prefix[left - 1] : 0);
                            total += sum;
                        }
                    }
                    
                }
                if (total > max) max = total;
                if (total < min) min = total;
            }

            return (min, max);
        }
    }
}
