﻿using HtmlAgilityPack;
using LyricParser.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LyricParser.Utils.Parsers.Anime
{
    public class GendouParser : HtmlParser
    {
        public GendouParser()
        {
            BaseUrl = Constants.GENDOU_URL;
        }

        public override async Task<string> ParseHtml(string artist, string title, string optional = "")
        {
            string html = await GetHtml(BaseUrl + artist.Replace(" ", "+") + "+" + title.Replace(" ", "+") + optional);
            if (string.IsNullOrEmpty(html)) return null;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            bool foundMatch = false;

            var table = doc.DocumentNode.SelectSingleNode("//table");
            
            if (table == null) return null;
            var tr = table.Elements("tr");

            // 1 = Title      5 = Artist       Url = 13

            for (int j = 2; j < tr.ToArray().Length; ++j)
            {
                var children = tr.ElementAt(j).ChildNodes;

                string _title = children.ElementAt(1).FirstChild.InnerText.ToLower().Trim().Normalize();
                string _artist = children.ElementAt(5).ChildNodes.ElementAt(1).InnerText.ToLower().Trim().Normalize();

                //Trace.WriteLine(_title + "___" + title + ": " + _title.Equals(title) + " Length: " + _title.Length + ":" + title.Length);
                //Trace.WriteLine(_artist + "___" + artist + ": " + _artist.Equals(artist) + " Length: " + _artist.Length + ":" + artist.Length);

                if (_title.Equals(title.ToLower().Normalize()) && _artist.Equals(artist))
                {
                    //anime_retry = 0;
                    foundMatch = true;
                    html = await GetHtml("http://gendou.com" + children.ElementAt(13).ChildNodes.ElementAt(1).GetAttributeValue("href", "null").Split('&')[0]);
                    break;
                }
            }

            if (!foundMatch) return null;

            doc = new HtmlDocument();
            doc.LoadHtml(html);

            return CleanUp(doc.DocumentNode.SelectSingleNode("//*[contains(@id,'content_1')]").InnerText);
        }
    }
}
