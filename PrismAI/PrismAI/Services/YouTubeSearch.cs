﻿using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using HtmlAgilityPack;
using PrismAI.Core.Models;

namespace PrismAI.Services;

public class YouTubeSearch
{
    private readonly YouTubeService _youtubeService;
    private static readonly HttpClient HttpClient = new();
    public YouTubeSearch(string apiKey, string channelId = "")
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentNullException(nameof(apiKey));

        _youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = GetType().ToString(),
        });
    }

    public async Task<List<YouTubeSearchResult>> Search(string keyWords, int count = 10)
    {
        if (string.IsNullOrEmpty(keyWords))
            throw new ArgumentNullException(nameof(keyWords));


        try
        {
            var searchListRequest = _youtubeService.Search.List("snippet");

            searchListRequest.Type = "video";

            searchListRequest.MaxResults = count;

            searchListRequest.Q = keyWords;

            SearchListResponse? searchListResponse = await searchListRequest.ExecuteAsync();
            var results = searchListResponse.Items.Select(searchResult => new YouTubeSearchResult(searchResult.Id.VideoId, searchResult.Snippet.Title, searchResult.Snippet.Description, $"https://www.youtube.com/watch?v={searchResult.Id.VideoId}", new YouTubeThumbnail(searchResult.Snippet.Thumbnails.Default__.Url, searchResult.Snippet.Thumbnails.Default__.Height, searchResult.Snippet.Thumbnails.Default__.Width))).ToList();
            return results;
            //var options = new JsonSerializerOptions { WriteIndented = true };
            //result = JsonSerializer.Serialize(searchListResponse.Items, options);

        }
        catch (Exception e)
        {
            //result = string.Empty;
            Console.WriteLine($"Youtube search error:\n{e}");
            return new List<YouTubeSearchResult>();
        }

        //return result;
    }
    public async Task<string> TranscribeVideo(string videoId, string language = "en")
    {
        try
        {
            var watchUrl = $"https://www.youtube.com/watch?v={videoId}";
            var response = await HttpClient.GetStringAsync(watchUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(response);
            var scriptTags = doc.DocumentNode.SelectNodes("//script");
            if (scriptTags == null) return "Transcript not found";
            Console.WriteLine($"Script tags count: {scriptTags.Count}");
            foreach (var scriptTag in scriptTags)
            {
                if (!scriptTag.InnerText.Contains("captionTracks")) continue;
                var regex = new Regex("\"captionTracks\":(\\[.*?\\])");
                var match = regex.Match(scriptTag.InnerText);
                if (match.Groups.Count <= 1) continue;
                var captionTracks = JsonSerializer.Deserialize<CaptionTrack[]>(match.Groups[1].Value);
                if (captionTracks is not { Length: > 0 }) continue;
                var transcriptUrl = captionTracks[0].BaseUrl;
                Console.WriteLine($"Caption tracks count: {captionTracks.Length}");
                foreach (var track in captionTracks)
                {
                    Console.WriteLine($"Track: {track.BaseUrl}");
                }
                var transcript = await HttpClient.GetStringAsync(transcriptUrl);
                var xDoc = XDocument.Parse(transcript);
                var lines = xDoc.Descendants("text").Select(x => x.Value).ToList();
                return string.Join("\n", lines);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Transcription error:\n{e}");
        }
        return "Transcript not found";
    }
    private class CaptionTrack
    {
        [JsonPropertyName("baseUrl")]
        public string BaseUrl { get; set; }
    }

}

