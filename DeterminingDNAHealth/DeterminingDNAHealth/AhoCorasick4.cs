using System;
using System.Collections.Generic;

namespace DeterminingDNAHealth
{

    // changed outputs to hashset because I don't need duplication when i save strings
    // changed outputs to string to handle duplication sum better
    public class AhoCorasick4
    {
        private const int ALPHABET_SIZE = 26; // assuming lowercase a-z
        private readonly int[][] children;
        private readonly int[] fail;
        private readonly HashSet<string>[] outputs;
        private int nodeCount;

        public AhoCorasick4(int maxPatternStates)
        {
            children = new int[maxPatternStates][];
            for (int i = 0; i < children.Length; i++)
                children[i] = new int[ALPHABET_SIZE]; // 0 means no transition

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
                if (children[node][index] == 0)
                    children[node][index] = nodeCount++;
                node = children[node][index];
            }
            outputs[node].Add(pattern);
        }

        public void Build()
        {
            var queue = new Queue<int>();
            for (int c = 0; c < ALPHABET_SIZE; c++)
            {
                if (children[0][c] != 0)
                {
                    fail[children[0][c]] = 0;
                    queue.Enqueue(children[0][c]);
                }
            }

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                for (int c = 0; c < ALPHABET_SIZE; c++)
                {
                    int next = children[current][c];
                    if (next == 0) continue;

                    int f = fail[current];
                    while (f != 0 && children[f][c] == 0)
                        f = fail[f];

                    if (children[f][c] != 0)
                        f = children[f][c];

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
                while (node != 0 && children[node][index] == 0)
                    node = fail[node];

                if (children[node][index] != 0)
                    node = children[node][index];

                matches.AddRange(outputs[node]);
            }

            return matches;
        }

        public long SearchTotal(string text, Func<string, long> Calculate)
        {
            long total = 0;
            int node = 0;

            foreach (char ch in text)
            {
                int index = CharToIndex(ch);
                while (node != 0 && children[node][index] == 0)
                    node = fail[node];

                if (children[node][index] != 0)
                    node = children[node][index];

                foreach (string gene in outputs[node])
                    total += Calculate(gene);
            }

            return total;
        }

        public long SearchTotal2(string text, int first, int last, Func<string, int, int, long> Calculate)
        {
            long total = 0;
            int node = 0;

            foreach (char ch in text)
            {
                int index = CharToIndex(ch);
                while (node != 0 && children[node][index] == 0)
                    node = fail[node];

                if (children[node][index] != 0)
                    node = children[node][index];

                foreach (string gene in outputs[node])
                    total += Calculate(gene, first, last);
            }

            return total;
        }
    }
}
