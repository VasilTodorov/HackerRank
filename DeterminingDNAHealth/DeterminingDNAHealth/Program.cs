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

    // Changed to AhoCorasick3 which uses simple datasrtuctures and reuires initial size so that it knows how much space it needs from the start
    //public static (long min, long max) CalculateHealthGeneral3(List<string> genes, List<int> health, List<(int first, int last, string text)> database)
    //{
    //    AhoCorasick3 algorithm = new AhoCorasick3(2000_000);

    //    for (int i = 0; i < genes.Count; i++)
    //    {
    //        algorithm.AddPattern(genes[i], i);
    //    }
    //    algorithm.Build();

    //    long min = long.MaxValue;
    //    long max = long.MinValue;

    //    foreach (var item in database)
    //    {
    //        long total = 0;
    //        total += algorithm.SearchTotal(item.text, i => (i >= item.first && i <= item.last) ? health[i] : 0);
    //        if (total > max) max = total;
    //        if (total < min) min = total;
    //    }

    //    return (min, max);
    //}


    // calculate the max possible size from the beginning, maybe it could be less
    // looks like there is a degration in test.13 but if I reduce the total length only for it it is faster
    public static (long min, long max) CalculateHealthGeneral4(string[] genes, int[] health, (int first, int last, string text)[] database)
    {
        // new
        int totalLength = 1;
        for (int i = 0; i < genes.Length; i++)
        {
            totalLength += genes[i].Length;
        }
        //***
        // if this is totalLength/2 test 7 fails
        AhoCorasick3 algorithm = new AhoCorasick3(totalLength);

        for (int i = 0; i < genes.Length; i++)
        {
            algorithm.AddPattern(genes[i], i);
        }
        algorithm.Build();


        long min = long.MaxValue;
        long max = long.MinValue;

        foreach (var item in database)
        {
            long total = algorithm.SearchTotal(item.text, i => (i >= item.first && i <= item.last) ? health[i] : 0);
            if (total > max) max = total;
            if (total < min) min = total;
        }

        return (min, max);
    }



    // Changed to v4 of algorythm which takes advantage of faster searches when duplication with the geneinfo module if there is a module 
    public static (long min, long max) CalculateHealthGeneral5(string[] genes, int[] health, (int first, int last, string text)[] database)
    {
        int totalLength = 1;
        for (int i = 0; i < genes.Length; i++)
        {
            totalLength += genes[i].Length;
        }

        AhoCorasick4 algorithm = new AhoCorasick4(totalLength);

        for (int i = 0; i < genes.Length; i++)
        {
            algorithm.AddPattern(genes[i]);
        }
        algorithm.Build();

        var geneMap = GeneInfo2.GenerateGeneMap(genes, health);
        long min = long.MaxValue;
        long max = long.MinValue;

        Func<string, int, int, long> Calculate = (gene, first, last) =>
        {
            long total = 0;
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
            return total;
        };

        foreach (var item in database)
        {
            //var matchedGenes = algorithm.Search(item.text);
            long total = 0;
            total = algorithm.SearchTotal(item.text, x => Calculate(x, item.first, item.last));
  
            if (total > max) max = total;
            if (total < min) min = total;
        }

        return (min, max);
    }

    // moved calculate in the  algorithm.SearchTotal because I don't want to call a function which calls a function
    // added hashset as in AhoCorasick less requests
    public static (long min, long max) CalculateHealthGeneral6(string[] genes, int[] health, (int first, int last, string text)[] database)
    {
        int totalLength = 1;
        for (int i = 0; i < genes.Length; i++)
        {
            totalLength += genes[i].Length;
        }


        AhoCorasick5 algorithm = new AhoCorasick5(totalLength, GeneInfo2.GenerateGeneMap(genes, health));
        HashSet<string> set = new HashSet<string>();
        for (int i = 0; i < genes.Length; i++)
        {
            if (!set.Contains(genes[i]))
            {
                algorithm.AddPattern(genes[i]);
                set.Add(genes[i]);
            }
        }

        algorithm.Build();

   
        long min = long.MaxValue;
        long max = long.MinValue;


        for (int i = 0; i < database.Length; i++)
        {
            var item = database[i];
            long total = 0;
            total = algorithm.SearchTotal(item.text, item.first, item.last);

            if (total > max) max = total;
            if (total < min) min = total;
        }

        return (min, max);
    }

    // tried passing params as reference because I thought it could be faster, but this makes no sense when you pass them once
    public static (long min, long max) CalculateHealthGeneral7(string[] genes, int[] health, (int first, int last, string text)[] database)
    {
        int totalLength = 1;
        for (int i = 0; i < genes.Length; i++)
        {
            totalLength += genes[i].Length;
        }


        AhoCorasick5 algorithm = new AhoCorasick5(totalLength, GeneInfo2.GenerateGeneMap(genes, health));

        HashSet<string> set = new HashSet<string>();
        for (int i = 0; i < genes.Length; i++)
        {
            if (!set.Contains(genes[i]))
            {
                algorithm.AddPattern(genes[i]);
                set.Add(genes[i]);
            }
        }
        algorithm.Build();


        long min = long.MaxValue;
        long max = long.MinValue;


        for (int i = 0; i < database.Length; i++)
        {
            //var matchedGenes = algorithm.Search(item.text);
            long total = 0;
            total = algorithm.SearchTota2(ref database[i]);

            if (total > max) max = total;
            if (total < min) min = total;
        }

        return (min, max);
    }


    public static (long min, long max) CalculateHealthGeneral8(string[] genes, int[] health, (int first, int last, string text)[] database)
    {
        int totalLength = 1;
        for (int i = 0; i < genes.Length; i++)
        {
            totalLength += genes[i].Length;
        }

        AhoCorasick6 algorithm = new AhoCorasick6(totalLength, GeneInfo2.GenerateGeneMap(genes, health));
        HashSet<string> set = new HashSet<string>();
        for (int i = 0; i < genes.Length; i++)
        {
            if (!set.Contains(genes[i]))
            {
                algorithm.AddPattern(genes[i]);
                set.Add(genes[i]);
            }
        }

        algorithm.Build();


        long min = long.MaxValue;
        long max = long.MinValue;


        for (int i = 0; i < database.Length; i++)
        {
            var item = database[i];
            long total = 0;
            total = algorithm.SearchTotal(item.text, item.first, item.last);

            if (total > max) max = total;
            if (total < min) min = total;
        }

        return (min, max);
    }

    // minimizing the total lenth, if there are duplicates they don't add new nodes this had an effect
 
    public static (long min, long max) CalculateHealthGeneral9(string[] genes, int[] health, (int first, int last, string text)[] database)
    {
        HashSet<string> set = new HashSet<string>(genes);
        int totalLength = 1;
        foreach (var item in set)
        {
            totalLength += item.Length;
        }

        AhoCorasick6 algorithm = new AhoCorasick6(totalLength, GeneInfo2.GenerateGeneMap(genes, health));
        foreach (var item in set)
            algorithm.AddPattern(item);

        algorithm.Build();


        long min = long.MaxValue;
        long max = long.MinValue;


        for (int i = 0; i < database.Length; i++)
        {
            var item = database[i];
            long total = 0;
            total = algorithm.SearchTotal(item.text, item.first, item.last);

            if (total > max) max = total;
            if (total < min) min = total;
        }

        return (min, max);
    }


    // reducing the total invokes of functions for functions that are n+1(possibly useless if compilator is doing its job)
    public static (long min, long max) CalculateHealthGeneral10(string[] genes, int[] health, (int first, int last, string text)[] database)
    {
        HashSet<string> set = new HashSet<string>(genes);
        int totalLength = 1;
        foreach (var item in set)
        {
            totalLength += item.Length;
        }

        AhoCorasick6 algorithm = new AhoCorasick6(totalLength, GeneInfo2.GenerateGeneMap(genes, health));
        algorithm.AddPatterns(set);
        algorithm.Build();

        return algorithm.SearchTotalAggregate(database);
    }

    public static void Main(string[] args)
    {
        //CustomTest1();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        Console.WriteLine("Old  ********************************************************");

        stopwatch.Restart();
        CustomTest2(CalculateHealthGeneral, "data7.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral(test:7): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest2(CalculateHealthGeneral, "data8.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral(test:8): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest2(CalculateHealthGeneral, "data13.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral(test:13): {stopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine("********************************************************");

        stopwatch.Restart();
        CustomTest2(CalculateHealthGeneral2, "data7.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral2(test:7): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest2(CalculateHealthGeneral2, "data8.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral2(test:8): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest2(CalculateHealthGeneral2, "data13.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral2(test:13): {stopwatch.ElapsedMilliseconds} ms");




        Console.WriteLine("New  ********************************************************");
        stopwatch.Restart();
        CustomTest4(CalculateHealthGeneral4, "data7.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral4(test:7): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest4(CalculateHealthGeneral4, "data8.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral4(test:8): {stopwatch.ElapsedMilliseconds} ms");

        // If i lower the capacity it behaves better
        stopwatch.Restart();
        CustomTest4(CalculateHealthGeneral4, "data13.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral4(test:13): {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine("********************************************************");

        stopwatch.Restart();
        CustomTest4(CalculateHealthGeneral5, "data7.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral5(test:7): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest4(CalculateHealthGeneral5, "data8.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral5(test:8): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest4(CalculateHealthGeneral5, "data13.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral5(test:13): {stopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine("********************************************************");
        stopwatch.Restart();
        CustomTest4(CalculateHealthGeneral6, "data7.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral6(test:7): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest4(CalculateHealthGeneral6, "data8.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral6(test:8): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest4(CalculateHealthGeneral6, "data13.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral6(test:13): {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine("********************************************************");
        stopwatch.Restart();
        CustomTest5(CalculateHealthGeneral6, "data7.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral6(test:7): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest5(CalculateHealthGeneral6, "data8.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral6(test:8): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest5(CalculateHealthGeneral6, "data13.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral6(test:13): {stopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine("********************************************************");
        stopwatch.Restart();
        CustomTest5(CalculateHealthGeneral8, "data7.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral8(test:7): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest5(CalculateHealthGeneral8, "data8.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral8(test:8): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest5(CalculateHealthGeneral8, "data13.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral8(test:13): {stopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine("********************************************************");
        stopwatch.Restart();
        CustomTest5(CalculateHealthGeneral9, "data7.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral9(test:7): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest5(CalculateHealthGeneral9, "data8.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral9(test:8): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest5(CalculateHealthGeneral9, "data13.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral9(test:13): {stopwatch.ElapsedMilliseconds} ms");

        Console.WriteLine("********************************************************");
        stopwatch.Restart();
        CustomTest5(CalculateHealthGeneral10, "data7.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral10(test:7): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest5(CalculateHealthGeneral10, "data8.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral10(test:8): {stopwatch.ElapsedMilliseconds} ms");

        stopwatch.Restart();
        CustomTest5(CalculateHealthGeneral10, "data13.txt");
        stopwatch.Stop();
        Console.WriteLine($"Execution time of CalculateHealthGeneral10(test:13): {stopwatch.ElapsedMilliseconds} ms");

        //stopwatch.Restart();
        //CustomTest4(CalculateHealthGeneral7, "data13.txt");
        //stopwatch.Stop();
        //Console.WriteLine($"Execution time of CalculateHealthGeneral7(test:13): {stopwatch.ElapsedMilliseconds} ms");  
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


    // Removed trim it is an additional operation
    // Changed list to array, dynamic sructures resize from time to time
    // Use for instead of linq select for health
    // removed n we don't use it
    // Parse is less  safe but requires less operation so it is faster
    private static void CustomTest4(Func<string[], int[], (int first, int last, string text)[], (long min, long max)> Calculate, string file)
    {
        string[] lines = File.ReadAllLines("../../../" + file);

        string[] genes = lines[1].Split(' ');
        string[] parts = lines[2].Split(' ');
        int[] health = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            health[i] = int.Parse(parts[i]);
        }

        //parse is less safe but faster 
        int s = int.Parse(lines[3]);

        (int first, int last, string text)[] database = new (int first, int last, string text)[s];
        for (int sItr = 0; sItr < s; sItr++)
        {
            string[] firstMultipleInput = lines[4 + sItr].Split(' ');

            int first = int.Parse(firstMultipleInput[0]);

            int last = int.Parse(firstMultipleInput[1]);

            string d = firstMultipleInput[2];

            database[sItr]=(first, last, d);
        }

        var result = Calculate(genes, health, database);
        //Console.WriteLine("expected: 0 8652768");
        Console.WriteLine($"min: {result.min}, max: {result.max}");
    }

    private static void CustomTest5(
    Func<string[], int[], (int first, int last, string text)[], (long min, long max)> Calculate,
    string file)
    {
        // this is memory gain but there is a good chance that there is no performance gained 
        using var reader = new StreamReader("../../../" + file);

        reader.ReadLine();
        // First line: genes
        string[] genes = reader.ReadLine()!.Split(' ');

        // Second line: health values
        string[] parts = reader.ReadLine()!.Split(' ');
        int[] health = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            health[i] = int.Parse(parts[i]);
        }

        // Third line: number of queries
        int s = int.Parse(reader.ReadLine()!);

        var database = new (int first, int last, string text)[s];
        for (int i = 0; i < s; i++)
        {
            string[] split = reader.ReadLine()!.Split(' ');
            int first = int.Parse(split[0]);
            int last = int.Parse(split[1]);
            string text = split[2];

            database[i] = (first, last, text);
        }

        var result = Calculate(genes, health, database);
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
