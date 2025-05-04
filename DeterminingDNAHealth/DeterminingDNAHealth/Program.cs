using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System;
using DeterminingDNAHealth;
using System.Diagnostics;

class Solution
{
    
    public static (long min, long max) CalculateHealthGeneral(List<string> genes, List<int> health, List<(int first, int last, string text)> database)
    {
        AhoCorasick algorithm = new AhoCorasick();
        HashSet<string> set = new();
        
        foreach (var gen in genes)
        {
            if(!set.Contains(gen))
            {
                algorithm.AddPattern(gen);
                set.Add(gen);
            }
            
        }

        algorithm.Build();

        var geneMap = GeneInfo.GenerateGeneMap(genes, health);
        long min = long.MaxValue;
        long max = long.MinValue;

        Func<string,int,int, long> Calculate = (gene, first, last) =>
        {
            long total = 0;
            var info = geneMap[gene];
            var posList = info.Positions;
            var prefix = info.PrefixSums;

            int left = GeneInfo.LowerBound(posList, first);
            int right = GeneInfo.UpperBound(posList, last);

            if (left <= right && left != -1 && right != -1)
            {
                long sum = prefix[right] - (left > 0 ? prefix[left - 1] : 0);
                total += sum;
            }
            return total;
        };

        foreach (var item in database)
        {
            //var matchedGenes = algorithm.Search(item.text);
            long total = 0;
            total = algorithm.SearchTotal(item.text, x => Calculate(x, item.first, item.last));
            //foreach (string gene in matchedGenes)
            //{
            //    if (!geneMap.ContainsKey(gene))
            //        continue;

            //    //var info = geneMap[gene];
            //    //var posList = info.Positions;
            //    //var prefix = info.PrefixSums;

            //    //int left = GeneInfo.LowerBound(posList, item.first);
            //    //int right = GeneInfo.UpperBound(posList, item.last);

            //    //if (left <= right && left != -1 && right != -1)
            //    //{
            //    //    long sum = prefix[right] - (left > 0 ? prefix[left - 1] : 0);
            //    //    total += sum;
            //    //}

            //    total += Calculate(gene, item.first, item.last);
                
            //}
            if (total > max) max = total;
            if (total < min) min = total;
        }

        return (min, max);

    }
    public static (long min, long max) CalculateHealthGeneral2(List<string> genes, List<int> health, List<(int first, int last, string text)> database)
    {
        AhoCorasick2 algorithm = new AhoCorasick2();

        for (int i = 0; i < genes.Count; i++)
        {
            algorithm.AddPattern(genes[i], i);
        }
        algorithm.Build();

        //var dict = CreateHealthDict(genes);
        long min = long.MaxValue;
        long max = long.MinValue;

        foreach (var item in database)
        {
            //var result = algorithm.Search(item.text);
            ////long temp = CalculateResult(dict, health, result, item.first, item.last);
            //long temp = 0;
            //foreach (int index in result)
            //{
            //    if (index >= item.first && index <= item.last)
            //        temp += health[index];
            //}

            //if (temp > max) max = temp;
            //if (temp < min) min = temp;

            long total = 0;
            total += algorithm.SearchTotal(item.text, i => (i>= item.first && i <= item.last) ? health[i] : 0);
            if(total > max) max = total;
            if(total < min) min = total;
        }

        return (min, max);
    }
    public static void Main(string[] args)
    {
        //CustomTest1();
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        CustomTest2(CalculateHealthGeneral, "data7.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral(test:7): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest2(CalculateHealthGeneral2, "data7.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral2(test:7): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Start();
        CustomTest2(CalculateHealthGeneral, "data8.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral(test:8): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest2(CalculateHealthGeneral2, "data8.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral2(test:8): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Start();
        CustomTest2(CalculateHealthGeneral, "data13.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral(test:13): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest2(CalculateHealthGeneral2, "data13.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral2(test:13): {stopwatch.ElapsedMilliseconds} ms");
    }

    private static void CustomTest1()
    {
        int n = 6;
        List<string> genes = new List<string>() { "a", "b", "c", "aa", "d", "b"};
        List<int> health = new List<int>() { 1, 2, 3, 4, 5, 6 };
        int s = 3;
        List<(int first, int last, string text)> database = new List<(int first, int last, string text)>()
        {   (1, 5, "caaab"),
            (0, 4, "xyz"),
            (2, 4, "bcdybc")};

        var result = CalculateHealthGeneral(genes, health, database);
        Console.WriteLine("expected: 0, 19");
        Console.WriteLine($"{result.min} {result.max}");
        
    }
    private static void CustomTest2(Func<List<string> , List<int> , List<(int first, int last, string text)> ,(long min, long max)> Calculate, string file)
    {
        string[] lines = File.ReadAllLines("../../../"+file);

        int n = Convert.ToInt32(lines[0].Trim());

        List<string> genes = lines[1].TrimEnd().Split(' ').ToList();
        List<int> health = lines[2].TrimEnd().Split(' ').Select(healthTemp => Convert.ToInt32(healthTemp)).ToList();

        int s = Convert.ToInt32(lines[3].Trim());

        List<(int first, int last, string text)> database = new List<(int first, int last, string text)>();

        for (int sItr = 0; sItr < s; sItr++)
        {
            string[] firstMultipleInput = lines[4+sItr].TrimEnd().Split(' ');

            int first = Convert.ToInt32(firstMultipleInput[0]);

            int last = Convert.ToInt32(firstMultipleInput[1]);

            string d = firstMultipleInput[2];

            database.Add((first, last, d));
        }

        var result = Calculate(genes, health, database);
        //Console.WriteLine("expected: 0 8652768");
        Console.WriteLine($"min: {result.min}, max: {result.max}");
    }
    private static void UserInput()
    {
        int n = Convert.ToInt32(Console.ReadLine()!.Trim());

        List<string> genes = Console.ReadLine()!.TrimEnd().Split(' ').ToList();

        List<int> health = Console.ReadLine()!.TrimEnd().Split(' ').Select(healthTemp => Convert.ToInt32(healthTemp)).ToList();

        int s = Convert.ToInt32(Console.ReadLine()!.Trim());


        List<(int first, int last, string text)> database = new List<(int first, int last, string text)>();
        for (int sItr = 0; sItr < s; sItr++)
        {
            string[] firstMultipleInput = Console.ReadLine().TrimEnd().Split(' ');

            int first = Convert.ToInt32(firstMultipleInput[0]);

            int last = Convert.ToInt32(firstMultipleInput[1]);

            string d = firstMultipleInput[2];

            database.Add((first, last, d));
        }

        var result = CalculateHealthGeneral(genes, health, database);
        Console.WriteLine($"{result.min} {result.max}");
    }
}
