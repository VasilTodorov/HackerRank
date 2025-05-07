using System;
using System.Collections.Generic;

namespace DeterminingDNAHealth
{

    // inlined geneInfo to reduce the number of functions
    // tried to pass tupple by ref, but it slowed things down
    public class AhoCorasick5
    {
        private const int ALPHABET_SIZE = 26; // assuming lowercase a-z
        private readonly int[,] children;
        private readonly int[] fail;
        private readonly HashSet<string>[] outputs;
        private int nodeCount;
        private Dictionary<string, GeneInfo2> geneMap;

        public AhoCorasick5(int maxPatternStates, Dictionary<string, GeneInfo2> geneMap)
        {
            this.geneMap = geneMap;
            //multidimensional array offered big improvement
            children = new int[maxPatternStates, ALPHABET_SIZE];

            fail = new int[maxPatternStates];
            outputs = new HashSet<string>[maxPatternStates];
            for (int i = 0; i < outputs.Length; i++)
                outputs[i] = new HashSet<string>();

            nodeCount = 1; // root is 0
        }

        private int CharToIndex(char c) => c - 'a';

        public void AddPattern(string pattern)
        {
            int node = 0;
            foreach (char ch in pattern)
            {
                int index = CharToIndex(ch);
                if (children[node,index] == 0)
                    children[node,index] = nodeCount++;
                node = children[node,index];
            }
            outputs[node].Add(pattern);
        }

        public void Build()
        {
            var queue = new Queue<int>();
            for (int c = 0; c < ALPHABET_SIZE; c++)
            {
                if (children[0,c] != 0)
                {
                    fail[children[0,c]] = 0;
                    queue.Enqueue(children[0,c]);
                }
            }

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                for (int c = 0; c < ALPHABET_SIZE; c++)
                {
                    int next = children[current,c];
                    if (next == 0) continue;

                    int f = fail[current];
                    while (f != 0 && children[f,c] == 0)
                        f = fail[f];

                    if (children[f,c] != 0)
                        f = children[f,c];

                    fail[next] = f;
                    outputs[next].UnionWith(outputs[f]);
                    queue.Enqueue(next);
                }
            }
        }

        public List<string> Search(string text)
        {
            var matches = new List<string>();
            int node = 0;

            foreach (char ch in text)
            {
                int index = CharToIndex(ch);
                while (node != 0 && children[node,index] == 0)
                    node = fail[node];

                if (children[node,index] != 0)
                    node = children[node,index];

                matches.AddRange(outputs[node]);
            }

            return matches;
        }

        public long SearchTotal(string text, int first, int last)
        {
            long total = 0;
            int node = 0;

            foreach (char ch in text)
            {
                int index = CharToIndex(ch);
                while (node != 0 && children[node,index] == 0)
                    node = fail[node];

                if (children[node,index] != 0)
                    node = children[node,index];

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


        //Tried to be intresting with the passing of arguments
        public long SearchTota2(ref (int first, int last, string text) param)
        {
            long total = 0;
            int node = 0;

            foreach (char ch in param.text)
            {
                int index = CharToIndex(ch);
                while (node != 0 && children[node,index] == 0)
                    node = fail[node];

                if (children[node,index] != 0)
                    node = children[node,index];

                foreach (string gene in outputs[node])
                {
                    var info = geneMap[gene];
                    var posList = info.Positions;
                    var prefix = info.PrefixSums;

                    int left = GeneInfo2.LowerBound(posList, param.first);
                    int right = GeneInfo2.UpperBound(posList, param.last);

                    if (left <= right && left != -1 && right != -1)
                    {
                        long sum = prefix[right] - (left > 0 ? prefix[left - 1] : 0);
                        total += sum;
                    }
                }
            }
            return total;
        }


        public long SearchTota3((int first, int last, string text) param)
        {
            long total = 0;
            int node = 0;

            foreach (char ch in param.text)
            {
                int index = CharToIndex(ch);
                while (node != 0 && children[node,index] == 0)
                    node = fail[node];

                if (children[node,index] != 0)
                    node = children[node,index];

                foreach (string gene in outputs[node])
                {
                    var info = geneMap[gene];
                    var posList = info.Positions;
                    var prefix = info.PrefixSums;

                    int left = GeneInfo2.LowerBound(posList, param.first);
                    int right = GeneInfo2.UpperBound(posList, param.last);

                    if (left <= right && left != -1 && right != -1)
                    {
                        long sum = prefix[right] - (left > 0 ? prefix[left - 1] : 0);
                        total += sum;
                    }
                }
            }
            return total;
        }
    }
}
