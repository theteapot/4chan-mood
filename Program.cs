using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using HtmlAgilityPack;

namespace WebAPIClient
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static void Main(string[] args)
        {

            string[] boardHandles = new string[] { "wg", "g", "his", "pol", "v" };
            foreach (string boardHandle in boardHandles)
            {
                Board myBoard = new Board(boardHandle);
                myBoard.GetThreads().Wait();
                Console.WriteLine($"{boardHandle}: {myBoard.BoardScore}");
            }

        }

        private static async Task GetArchive(string board)
        {
            try
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:66.0) Gecko/20100101 Firefox/66.0");
                HttpResponseMessage response = await client.GetAsync($"https://boards.4channel.org/{board}/archive");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(board);
                HtmlDocument pageDocument = new HtmlDocument();
                pageDocument.LoadHtml(responseBody);
                var threads = pageDocument.DocumentNode.SelectNodes("(//table[contains(@id,'arc-list')]//tr)");

                foreach (var thread in threads)
                {
                    string threadNo = thread.SelectSingleNode("*[1]").InnerHtml;
                    string threadTeaser = thread.SelectSingleNode("*[2]").InnerHtml;
                    if (threadNo != "No.")
                    {
                        Thread myThread = new Thread(threadNo, threadTeaser, board);
                        await myThread.GetPosts();
                        await Task.Delay(5000);
                        decimal threadScore = myThread.GetThreadScore();
                        Console.WriteLine($"{threadNo}: {threadScore}");
                    }
                }

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
    }
}

