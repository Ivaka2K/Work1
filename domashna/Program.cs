using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        string filePath = "C:\\Users\\Ivaka2K\\Desktop\\Domashna 1\\Domashna 1\\IvanV.txt"; // Replace with the path to your text file

        if (!File.Exists(filePath))
        {
            Console.WriteLine("The specified file does not exist.");
            return;
        }

        string text = File.ReadAllText(filePath);
        Console.WriteLine(text);

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
                string[] words = Regex.Split(text, @"\W+");//преглежда текста и всичко което не е дума не го слага в dictionary (премахва TAB,пунктуация)
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
                string[] words = Regex.Split(text, @"\W+"); //преглежда текста и всичко което не е дума не го слага в dictionary (премахва TAB,пунктуация)
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
        double averageWordLength = wordCount == 0 ? 0 : (double)totalWordLength / wordCount;

        // Find the five most common words
        List<string> mostCommonWords = FindMostCommonWords(wordFrequency, 5);

        // Find the five least common words
        List<string> leastCommonWords = FindLeastCommonWords(wordFrequency, 5);

        // Display the results
        Console.WriteLine($"Number of Words: {wordCount}");
        Console.WriteLine($"Shortest Word: {shortestWord}");
        Console.WriteLine($"Longest Word: {longestWord}");
        Console.WriteLine($"Average Word Length: {averageWordLength:F2}");
        Console.WriteLine("Five Most Common Words:");
        foreach (var word in mostCommonWords)
        {
            Console.WriteLine(word);
        }
        Console.WriteLine("Five Least Common Words:");
        foreach (var word in leastCommonWords)
        {
            Console.WriteLine(word);
        }
    }

    static List<string> FindMostCommonWords(Dictionary<string, int> wordFrequency, int count)
    {
        List<string> top5Words = new List<string>();
        foreach (var word in wordFrequency)
        {
            // Check if the list is not full (less than 5 words)
            if (top5Words.Count < 5)
            {
                top5Words.Add(word.Key);
            }
            else
            {
                // Compare the frequency with the lowest frequency in the top 5
                for (int i = 0; i < top5Words.Count; i++)
                {
                    if (wordFrequency[top5Words[i]] < word.Value)
                    {
                        // Replace the word with the lower frequency    
                        top5Words[i] = word.Key;
                        break;
                    }
                }
            }

        }
        return top5Words;
    }

    static List<string> FindLeastCommonWords(Dictionary<string, int> wordFrequency, int count)
    {
        List<string> lowest5Words = new List<string>();

        foreach (var word in wordFrequency)
        {
            // Check if the list is not full (less than 5 words)
            if (lowest5Words.Count < 5)
            {
                lowest5Words.Add(word.Key);
            }
            else
            {
                // Compare the frequency with the highest frequency in the lowest 5
                for (int i = 0; i < lowest5Words.Count; i++)
                {
                    if (wordFrequency[lowest5Words[i]] > word.Value)
                    {
                        // Replace the word with the higher frequency
                        lowest5Words[i] = word.Key;
                        break;
                    }
                }
            }
        }
        return lowest5Words;
    }
}
