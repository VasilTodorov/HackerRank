using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeterminingDNAHealth
{
    public class AhoCorasick2
    {
        private class Node
        {
            public Dictionary<char, Node> Children = new Dictionary<char, Node>();
            public Node Fail;
            public List<int> Output = new List<int>();
        }

        private readonly Node root = new Node();

        public void AddPattern(string pattern, int geneIndex)
        {
            var node = root;
            foreach (var ch in pattern)
            {
                if (node.Children.ContainsKey(ch))
                {
                    node = node.Children[ch];
                }
                else
                {
                    node.Children.Add(ch, new Node());
                    node = node.Children[ch];
                }
            }
            node.Output.Add(geneIndex);
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

        public List<int> Search(string text)
        {
            var results = new List<int>();
            var node = root;
            foreach (char c in text)
            {
                while (node != null && !node.Children.ContainsKey(c))
                    node = node.Fail;
                node = node != null ? node.Children.GetValueOrDefault(c, root) : root;
                results.AddRange(node.Output);
            }
            return results;
        }

        public long SearchTotal(string text, Func<int, int> Calculate)
        {
            //var results = new List<int>();
            long total = 0;
            var node = root;
            foreach (char c in text)
            {
                while (node != null && !node.Children.ContainsKey(c))
                    node = node.Fail;
                node = node != null ? node.Children.GetValueOrDefault(c, root) : root;

                //results.AddRange(node.Output);
                foreach(int i in node.Output)
                {
                    total += Calculate(i);
                }
            }
            return total;
        }
    }
}
