using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace OctodexScraper
{
    public class Program
    {

        public const string octodexUri = "https://octodex.github.com";

        public static void Main(string[] args)
        {
            Console.WriteLine("Searching for Octocats");
            Task.Run(async () => {
                var results = await searchOctocats();
                Console.WriteLine($"Found {results.Count()} Octocats");

                string outDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);

                foreach(string item in results)
                {
                    string name = item.Substring("/images/".Length);
                    System.Console.WriteLine($"Download {name}");
                    await downloadOctocat(octodexUri + item, Path.Combine(outDir, name));
                }
            }).Wait();
        }

        public static async Task<IEnumerable<string>> searchOctocats()
        {
            HttpClient client = new HttpClient();
            string html = await client.GetStringAsync(octodexUri);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            IEnumerable<string> results = doc.DocumentNode.SelectNodes("//a[@class='preview-image']/img")
                .Select((img)=> img.Attributes["data-src"].Value );

            return results;
        }

        public static async Task downloadOctocat(string url, string destination)
        {
            HttpClient client = new HttpClient();
            Stream stream = await client.GetStreamAsync(url);

            FileStream file = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true);
            await stream.CopyToAsync(file);
        }
    }
}
