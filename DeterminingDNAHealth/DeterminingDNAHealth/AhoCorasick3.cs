public class AhoCorasick3
{
    private const int ALPHABET_SIZE = 26;
    private const char BASE_CHAR = 'a';

    private readonly int[,] _goto;
    private readonly int[] _fail;
    private readonly List<int>[] _output;
    private int _nextNode = 1; // root is node 0

    public AhoCorasick3(int maxNodes)
    {
        _goto = new int[maxNodes, ALPHABET_SIZE];
        _fail = new int[maxNodes];
        _output = new List<int>[maxNodes];
    }

    public void AddPattern(string pattern, int index)
    {
        int node = 0;
        foreach (char ch in pattern)
        {
            int c = ch - BASE_CHAR;
            if (_goto[node, c] == 0)
            {
                _goto[node, c] = _nextNode++;
            }
            node = _goto[node, c];
        }

        _output[node] ??= new List<int>();
        _output[node].Add(index);
    }

    public void Build()
    {
        Queue<int> queue = new();

        for (int c = 0; c < ALPHABET_SIZE; c++)
        {
            int child = _goto[0, c];
            if (child != 0)
            {
                _fail[child] = 0;
                queue.Enqueue(child);
            }
        }

        while (queue.Count > 0)
        {
            int r = queue.Dequeue();
            for (int c = 0; c < ALPHABET_SIZE; c++)
            {
                int s = _goto[r, c];
                if (s == 0) continue;

                queue.Enqueue(s);
                int state = _fail[r];
                while (state != 0 && _goto[state, c] == 0)
                    state = _fail[state];

                _fail[s] = _goto[state, c];

                if (_output[_fail[s]] != null)
                {
                    _output[s] ??= new List<int>();
                    _output[s].AddRange(_output[_fail[s]]);
                }
            }
        }
    }

    public List<int> Search(string text)
    {
        List<int> results = new();
        int state = 0;

        for (int i = 0; i < text.Length; i++)
        {
            int c = text[i] - BASE_CHAR;
            while (state != 0 && _goto[state, c] == 0)
                state = _fail[state];

            if (_goto[state, c] != 0)
                state = _goto[state, c];

            if (_output[state] != null)
            {
                results.AddRange(_output[state]);
            }
        }

        return results;
    }

    public long SearchTotal(string text, Func<int, long> Calculate)
    {
        long total = 0;
        int state = 0;

        for (int i = 0; i < text.Length; i++)
        {
            int c = text[i] - BASE_CHAR;
            while (state != 0 && _goto[state, c] == 0)
                state = _fail[state];

            if (_goto[state, c] != 0)
                state = _goto[state, c];

            if (_output[state] != null)
            {
                foreach (int index in _output[state])
                {
                    total += Calculate(index);
                }
            }
        }

        return total;
    }
}
