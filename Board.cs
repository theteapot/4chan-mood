using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using HtmlAgilityPack;

namespace WebAPIClient
{
    class Board
    {
        public List<Thread> Threads { get; set; }
        public string BoardHandle { get; set; }
        public decimal BoardScore { get; set; }
        private readonly HttpClient client = new HttpClient();
        private static Random random = new Random();

        public Board(string boardHandle)
        {
            Threads = new List<Thread>();
            BoardHandle = boardHandle;
        }

        public async Task GetThreads()
        {
            try
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:66.0) Gecko/20100101 Firefox/66.0");
                HttpResponseMessage response = await client.GetAsync($"https://boards.4channel.org/{BoardHandle}/archive");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                HtmlDocument pageDocument = new HtmlDocument();
                pageDocument.LoadHtml(responseBody);
                var threads = pageDocument.DocumentNode.SelectNodes("(//table[contains(@id,'arc-list')]//tr)");

                foreach (var thread in threads)
                {
                    string threadNo = thread.SelectSingleNode("*[1]").InnerHtml;
                    string threadTeaser = thread.SelectSingleNode("*[2]").InnerHtml;
                    if (threadNo != "No.")
                    {
                        Thread myThread = new Thread(threadNo, threadTeaser, BoardHandle);
                        await myThread.GetPosts();
                        await Task.Delay(random.Next(10000, 15000));
                        decimal threadScore = myThread.GetThreadScore();
                        // Console.WriteLine($"{threadNo}: {threadScore}");
                        Threads.Add(myThread);
                    }
                }

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public decimal GetBoardScore()
        {
            decimal boardScore = 0m;
            foreach (Thread thread in Threads)
            {
                boardScore += thread.ThreadScore;
            }
            return boardScore;
        }
    }
}