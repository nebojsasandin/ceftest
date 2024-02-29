using CefSharp;
using CefSharp.OffScreen;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CefTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            InitCef();

            RunCefTest();

            ShutDownCef();

            WaitForInputToQuit();
        }

        private static void WaitForInputToQuit()
        {
            Console.WriteLine("\n\nPress any key to quit...");
            Console.ReadKey();
        }

        private static void ShutDownCef()
        {
            Cef.Shutdown();
        }

        private static void RunCefTest()
        {
            //important: must navigate to Google first...., if we leave about:blank, then all of this will work fine, it needs to navigate from one google page to the other.
            using (var browser = new ChromiumWebBrowser("https://www.google.com/", useLegacyRenderHandler: false))
            {
                try
                {
                    browser.WaitForInitialLoadAsync().Wait();
                    browser.Load("https://accounts.google.com/");
                    browser.WaitForNavigationAsync().Wait();

                    //this line fails with error (inner exception): Request BrowserId : 1 not found it's likely the browser is already closed
                    var rez = browser.GetMainFrame().EvaluateScriptAsync<string>("document.querySelector(\"button[aria-haspopup='menu']\").innerText").Result;
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"!! ERROR !!\n\n{ex.InnerException.Message}\n\n{ex.InnerException.StackTrace}\n\n");
                    }
                    else
                    {
                        Console.WriteLine($"!! ERROR !!\n\n{ex.Message}\n\n{ex.StackTrace}\n\n");
                    }
                }
                finally
                {
                    //save both, SRC and Image, to confirm that the page did contain querySelector we were trying to find.
                    var src = browser.GetSourceAsync().Result;
                    File.WriteAllText("__debug.html", src);
                    Console.WriteLine(">>>>>> Debug source saved: __debug.html");
                    var ss = browser.CaptureScreenshotAsync(CefSharp.DevTools.Page.CaptureScreenshotFormat.Png).Result;
                    using (Image image = Image.FromStream(new MemoryStream(ss)))
                    {
                        image.Save("__debug.png", ImageFormat.Png);
                        Console.WriteLine(">>>>>> Debug screenshot saved: __debug.png");
                    }
                }
            }
        }

        private static void InitCef()
        {
            var cSettings = new CefSettings();
            cSettings.CefCommandLineArgs.Add("disable-site-isolation-trials");
            if (!Cef.Initialize(cSettings))
            {
                throw new Exception("Unable to Initialize Cef");
            }
        }
    }
}
