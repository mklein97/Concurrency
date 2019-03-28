//Matt Klein
//Thread Pool Lab

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Concurrency
{
    //class type to make the following code more readable
    class CpuTaskData : Tuple< string, int, Uri >
    {
        public CpuTaskData(string c, int d, Uri u ) : base(c,d,u) {}
    }
    
    class DeadLinkInfo
    {
        public Uri deadLink;
        public Uri whereFrom;
        public DeadLinkInfo(Uri dl, Uri wf)
        {
            deadLink=dl;
            whereFrom=wf;
        }
    }

    class MainClass
    {
        
        //guards all the static variables
        public static object L = new object();

        static List<DeadLinkInfo> deadLinks = new List<DeadLinkInfo>();
        
        //set of url's that have been downloaded so we don't grab the
        //same one more than once
        static HashSet<string> processed = new HashSet<string>();

        //count of how many tasks are working
        static int numWorking = 1;

        //these are constant after main() starts up
        static int maxDistance;
        static Uri initialUrl;

        private static bool done = false;
        
        //the CPU worker code
        static void CPUTask(CpuTaskData t)
        {
            var R = new Regex(@"<a\s+[^>]*href=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
            
            string htmlContent = t.Item1;
            int distance = t.Item2;
            Uri originator = t.Item3;
            var MC = R.Matches(htmlContent);
            foreach(Match M in MC)
            {
                string s = M.Groups[1].Value;
                Uri next = new Uri(originator, s);
                lock (L) { numWorking++; }
                Task.Run(() => Download(next, distance + 1, originator));
            }
            lock (L) { numWorking--; }
        }

        static void Download(Uri u, int dist, Uri wherefrom)
        {
            bool contains;
            lock (L)
            {
                contains = processed.Contains(u.ToString());
                if (!contains)
                    processed.Add(u.ToString());
            }
            if (dist <= maxDistance && !contains && initialUrl.Host == u.Host)
            {
                try
                {
                    WebClient wc = new WebClient();
                    var s = wc.DownloadString(u);
                    lock (L)
                    {
                        ++numWorking;
                    }

                    Task.Run(() => CPUTask(new CpuTaskData(s, dist, u)));
                }
                catch (WebException)
                {
                    lock (L)
                    {
                        deadLinks.Add(new DeadLinkInfo(u, wherefrom));
                    }
                }
            }

            lock (L)
            {
                --numWorking;
                if (numWorking == 0)
                {
                    done = true;
                    Monitor.PulseAll(L);
                }
                    
            }
        }

        public static void Main(string[] args)
        {
            initialUrl = new Uri(args[0]);
            maxDistance = Convert.ToInt32(args[1]);

            Console.WriteLine("Crawl " + initialUrl);
            Console.WriteLine("Distance: " + maxDistance);

            Task.Run(() => Download(initialUrl, 0, initialUrl));

            lock (L)
            {
                while (!done)
                    Monitor.Wait(L);
            }

            Console.WriteLine("---------------------------------");
            foreach(var x in deadLinks) 
                Console.WriteLine(x.deadLink+" from "+x.whereFrom);
        }
    }
}
