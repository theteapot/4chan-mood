using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using HtmlAgilityPack;

namespace WebAPIClient
{
    class Thread
    {
        private static readonly HttpClient client = new HttpClient();

        public string ThreadNumber { get; set; }
        public string ThreadTeaser { get; set; }
        public string Board { get; set; }
        public List<Post> Posts { get; set; }
        public decimal ThreadScore { get; set; }

        public Thread(string threadNumber, string threadTeaser, string board)
        {
            if (String.IsNullOrWhiteSpace(threadNumber)) throw new ArgumentException("threadNumber cannot be null or empty");
            if (threadNumber == "No.") throw new ArgumentException("threadNumber cannot be 'No.'");
            Posts = new List<Post>();
            ThreadNumber = threadNumber;
            ThreadTeaser = threadTeaser;
            Board = board;
        }

        public async Task GetPosts()
        {
            try
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:66.0) Gecko/20100101 Firefox/66.0");
                HttpResponseMessage response = await client.GetAsync($"https://boards.4channel.org/{Board}/thread/{ThreadNumber}/");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                HtmlDocument pageDocument = new HtmlDocument();
                pageDocument.LoadHtml(responseBody);
                var posts = pageDocument.DocumentNode.SelectNodes("(//div[contains(@class,'post reply')])");

                try
                {
                    foreach (var post in posts)
                    {

                        // Console.WriteLine(post);
                        var postNumber = post.SelectSingleNode("div/span[contains(@class,'postNum desktop')]//a[last()]").InnerText;
                        var postBody = post.SelectSingleNode("blockquote[contains(@class, 'postMessage')]").InnerText;
                        Post myPost = new Post(postNumber, postBody);
                        Posts.Add(myPost);
                    }
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine($"https://boards.4channel.org/{Board}/thread/{ThreadNumber}/");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public decimal GetThreadScore()
        {
            decimal threadScore = 0m;

            foreach (Post post in Posts)
            {
                decimal postScore = post.GetScore();
                threadScore += postScore;
            }
            ThreadScore = threadScore;
            return threadScore;
        }



        public override string ToString() => $"{ThreadNumber}: {ThreadTeaser}";
    }


}
