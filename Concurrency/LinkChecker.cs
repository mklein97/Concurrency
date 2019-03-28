//Matt Klein
//Operating Systems 2

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Timers;
using System.Drawing;
using System.Text.RegularExpressions;

namespace OS
{
    class Program
    {
        public static object L = new object();
        public static BlockingCollection<Tuple<int, Uri>> dList = new BlockingCollection<Tuple<int, Uri>>();
        public static BlockingCollection<Tuple<string, int, Uri>> rList = new BlockingCollection<Tuple<string, int, Uri>>();
        public static List<Tuple<string, string>> deadLinks = new List<Tuple<string, string>>();
        public static HashSet<string> completedList = new HashSet<string>();
        public static int numCpuThreads = 0;
        public static int numNetThreads = 0;
        public static int Distance;

        public static void Main2(string[] args)
        {
            if (args.Length != 2)
                throw new ArgumentException("Must pass 2 arguments: URL and maximum distance");
            if (!int.TryParse(args[1], out var dist))
                throw new ArgumentException("Must pass max distance (number) as 2nd parameter!");
            if (dist < 0)
                throw new ArgumentException("Distance must be non-negative!");

            Distance = dist;
            Uri inputUri = new Uri(args[0]);
            var result = Uri.TryCreate(inputUri.ToString(), UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!result)
                throw new ArgumentException("Invalid URL provided!");

            List<Thread> threads = new List<Thread>();

            var wc = new WebClient();
            Regex rex = new Regex("<\\s*a (.*\\s)?href=\"?'?(.*?)\"?'?(\\s.*)?>", RegexOptions.IgnoreCase);

            try
            {
                rList.Add(new Tuple<string, int, Uri>(wc.DownloadString(inputUri), 1, inputUri));
            }
            catch (WebException)
            {
                Console.WriteLine("Initial link is dead!");
                return;
            }

            for (var i = 0; i < 8; i++)
            {
                threads.Add(new Thread(() =>
                {
                    int crawls = 1;
                    while (true)
                    {
                        int type = 0;

                        if (numNetThreads < 4)
                        {
                            type = 1;
                            numNetThreads++;
                        }
                        else if (numCpuThreads < 4)
                        {
                            type = 2;
                            numCpuThreads++;
                        }
                        else
                        {
                            type = 3;
                            Console.WriteLine("Poisoned");
                        }
                            

                        if (type == 3)
                            break;

                        if (type == 1)
                        {
                            if (crawls <= Distance)
                            {
                                Console.WriteLine("Downloading");
                                DoNet(wc, crawls);
                                crawls++;
                            }
                            else
                            {
                                numNetThreads--;
                                break;
                            }
                        }

                        else if (type == 2)
                        {
                            Console.WriteLine("Scanning");
                            DoCpu(rex);
                            numCpuThreads--;
                        }
                    }
                }));
            }
            foreach (var t in threads)
                t.Start();
            foreach (var t in threads)
                t.Join();
            Console.WriteLine("Done!");
            foreach (var v in deadLinks)
                Console.WriteLine(v);
        }

        public static void DoNet(WebClient wc, int dist)
        {
            lock (L)
            {
                while (dList.Count == 0)
                    Monitor.Wait(L);
            }
            
            var link = dList.Take();
            var uriResult = link.Item2;
            
            try
            {
                if (Distance > 1)
                {
                    if (uriResult.ToString()[0] == '/')
                        rList.Add(new Tuple<string, int, Uri>(wc.DownloadString(uriResult), dist, uriResult));
                }
                else
                {
                    var s = wc.DownloadString(uriResult);
                }
            }
            catch (WebException)
            {
                lock (L)
                {
                    if (Distance == 1)
                        deadLinks.Add(new Tuple<string, string>(uriResult.Host, uriResult.ToString()));
                    else
                        deadLinks.Add(new Tuple<string, string>(uriResult.Host, uriResult.ToString()));
                }
            }
        }

        public static void DoCpu(Regex rex)
        {
            var page = rList.Take();
            MatchCollection results = rex.Matches(page.Item1);

            foreach (Match m in results)
            {
                var link = page.Item3.GetLeftPart(UriPartial.Authority) + m.Groups[1].Value;
                //if (link == "http://localhost:8888")
                    
                
                if (!completedList.Contains(link))
                {
                    var result = Uri.TryCreate(link, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                    if (!result)
                        continue;
                    dList.Add(new Tuple<int, Uri>(page.Item2, uriResult));
                    lock (L) { Monitor.PulseAll(L); }
                    completedList.Add(link);
                }
                
            } 
        }
    }
}
