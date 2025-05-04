using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeterminingDNAHealth
{
    public class AhoCorasick
    {
        private class Node
        {
            public Dictionary<char, Node> Children = new Dictionary<char, Node>();
            public Node Fail;
            public List<string> Output = new List<string>();
        }

        private readonly Node root = new Node();

        public void AddPattern(string pattern)
        {
            var node = root;
            foreach (var ch in pattern)
            {
                if (!node.Children.ContainsKey(ch))
                    node.Children[ch] = new Node();
                node = node.Children[ch];
            }
            node.Output.Add(pattern);  // change from index to string
        }

        public void Build()
        {
            var queue = new Queue<Node>();
            foreach (var pair in root.Children)
            {
                pair.Value.Fail = root;
                queue.Enqueue(pair.Value);
            }

            while (queue.Any())
            {
                Node current = queue.Peek();
                queue.Dequeue();

                foreach (var pair in current.Children)
                {
                    char c = pair.Key;
                    Node child = pair.Value;

                    Node fail = current.Fail;
                    while (fail != null && !fail.Children.ContainsKey(pair.Key))
                    {
                        fail = fail.Fail;
                    }

                    child.Fail = fail != null ? fail.Children[c] : root;
                    child.Output.AddRange(child.Fail.Output);
                    queue.Enqueue(child);
                }
            }

        }

        public List<string> Search(string text)
        {
            var matches = new List<string>();
            var node = root;
            foreach (char c in text)
            {
                while (node != null && !node.Children.ContainsKey(c))
                    node = node.Fail;
                node = node != null ? node.Children.GetValueOrDefault(c, root) : root;
                matches.AddRange(node.Output);
            }
            return matches;
        }

        public long SearchTotal(string text, Func<string, long> Calculate)
        {
            long total = 0;
            var node = root;
            foreach (char c in text)
            {
                while (node != null && !node.Children.ContainsKey(c))
                    node = node.Fail;
                node = node != null ? node.Children.GetValueOrDefault(c, root) : root;
                //matches.AddRange(node.Output);
                foreach(string gene in node.Output)
                {
                    total += Calculate(gene);
                }
            }
            return total;
        }
    }
}
