using Microsoft.Web.WebView2;
using Microsoft.Web.WebView2.Core;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Wpf;
using PuppeteerSharp;
using PuppeteerSharp.Helpers;

public class WebPageParser
{
    private WebView2 webView;
    private static List<string> links = new();
    private static List<string> images = new ();
    private static IBrowser browser;
    private static BrowserFetcher browserFetcher;
    public WebPageParser()
    {
        

        

        
    }

    public async void StartPuppet()
    {
        

        
    }


    public async Task NavigateAndParseAsync(string url)
    {
        StartPuppet();

        browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
        {
            Path = @"C:\temp\chromium\",
        });

        await browserFetcher.DownloadAsync();

        browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Timeout = 0,
            ExecutablePath = @"C:\temp\chromium\Chrome\Win64-119.0.6045.105\chrome-win64\chrome.exe"
        });

        var page = await browser.NewPageAsync();
        
        var navigation = new NavigationOptions
        {
            Timeout = 0,
            WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded }
        };
        
        await page.GoToAsync(url, navigation);

        var html = await page.GetContentAsync();

        ParseHtml(html);
    }

    private void ParseHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]"))
        {
            links.Add(link.GetAttributeValue("href", string.Empty));
        }

        foreach (var img in doc.DocumentNode.SelectNodes("//img[@src]"))
        {
            images.Add(img.GetAttributeValue("src", string.Empty));
        }
    }

    public List<string> GetLinks()
    {
        return links;
    }

    public List<string> GetImages()
    {
        return images;
    }
}