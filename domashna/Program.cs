using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        string filePath = "C:\\Users\\ivoni\\source\\repos\\domashna\\domashna\\Book\\ivan.txt"; // Replace with the path to your text file

        if (!File.Exists(filePath))
        {
            Console.WriteLine("Липсва вашият документ.");
            return;
        }

        string text = File.ReadAllText(filePath);

        // Initialize shared variables for statistics
        int wordCount = 0;
        string shortestWord = null;
        string longestWord = null;
        int totalWordLength = 0;
        Dictionary<string, int> wordFrequency = new Dictionary<string, int>();
        
        // Lock object for thread synchronization
        object lockObject = new object();

        // Create tasks for each statistic calculation
        List<Task> tasks = new List<Task>
        {
            Task.Run(() =>
            {
                string[] words = Regex.Split(text, @"\W+");
                foreach (string word in words)
                {
                    if (word.Length >= 3)
                    {
                        Interlocked.Increment(ref wordCount);

                        lock (lockObject)
                        {
                            if (shortestWord == null || word.Length < shortestWord.Length)
                                shortestWord = word;

                            if (longestWord == null || word.Length > longestWord.Length)
                                longestWord = word;

                            totalWordLength += word.Length;

                            if (wordFrequency.ContainsKey(word))
                                wordFrequency[word]++;
                            else
                                wordFrequency[word] = 1;
                        }
                    }
                }
            }),

            Task.Run(() =>
            {
                string[] words = Regex.Split(text, @"\W+");
                Dictionary<string, int> frequency = new Dictionary<string, int>();
                foreach (var word in words)
                {
                    if (word.Length >= 3)
                    {
                        if (frequency.ContainsKey(word))
                            frequency[word]++;
                        else
                            frequency[word] = 1;
                    }
                }              

                lock (lockObject)
                {
                    wordFrequency = frequency;
                }
            })
        };

        // Wait for all tasks to complete
        Task.WhenAll(tasks).Wait();

        // Calculate average word length
        double averageWordLength =wordCount == 0 ? 0 : (double)totalWordLength / wordCount;

        // Find the five most common words
        List<string> mostCommonWords = FindMostCommonWords(wordFrequency, 5);

        // Find the five least common words
        List<string> leastCommonWords = FindLeastCommonWords(wordFrequency, 5);

        // Display the results
        Console.WriteLine($"Брой на думите: {wordCount}");
        Console.WriteLine($"Най-кратката дума е: {shortestWord}");
        Console.WriteLine($"Най-дългата дума е: {longestWord}");
        Console.WriteLine($"Средната дължина на думите в текста е: {averageWordLength:F2}");
        Console.WriteLine("5 най-често срещани думи в текста:");
        foreach (var word in mostCommonWords)
        {
            Console.WriteLine(word);
        }
        Console.WriteLine("5 най-рядко срещани думи в текста:");
        foreach (var word in leastCommonWords)
        {
            Console.WriteLine(word);
        }
    }

    static List<string> FindMostCommonWords(Dictionary<string, int> wordFrequency, int count)
    {
        List<string> mostCommonWords = new List<string>();
        foreach (var pair in wordFrequency)
        {
            mostCommonWords.Add(pair.Key);
            if (mostCommonWords.Count >= count)
                break;
        }
        mostCommonWords.Sort((w1, w2) => wordFrequency[w2].CompareTo(wordFrequency[w1]));
        return mostCommonWords;
    }

    static List<string> FindLeastCommonWords(Dictionary<string, int> wordFrequency, int count)
    {
        List<string> leastCommonWords = new List<string>();
        foreach (var pair in wordFrequency) 
        {
            if (leastCommonWords.Count < count)
            {
                leastCommonWords.Add(pair.Key);
            }
            else
            {
                foreach (var word in leastCommonWords)
                {
                    if (pair.Value < wordFrequency[word])
                    {
                        leastCommonWords.Remove(word);
                        leastCommonWords.Add(pair.Key);
                        break;
                    }
                }
            }
            leastCommonWords.Sort((w1, w2) => wordFrequency[w1].CompareTo(wordFrequency[w2]));
        }
        return leastCommonWords;
    }
}
