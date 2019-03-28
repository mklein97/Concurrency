using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace dlc
{

    //class type to make the following code more readable
    class NetTaskData : Tuple<Uri, int, Uri>
    {
        public NetTaskData(Uri u, int d, Uri s) : base(u, d, s)
        {
        }
    }

    //class type to make the following code more readable
    class CpuTaskData : Tuple<string, int, Uri>
    {
        public CpuTaskData(string c, int d, Uri u) : base(c, d, u)
        {
        }
    }

    class DeadLinkInfo
    {
        public Uri deadLink;
        public Uri whereFrom;
        public DeadLinkInfo(Uri dl, Uri wf)
        {
            deadLink = dl;
            whereFrom = wf;
        }
    }

    class MainClass
    {

        //guards all the static variables
        public static object L = new object();

        //first = html content to scan
        //second = distance from start
        //third = uri that held the document
        static Queue<CpuTaskData> ItemsForCPUTasks =
            new Queue<CpuTaskData>();

        //first = uri to download
        //second = distance from start
        static Queue<NetTaskData> ItemsForNetTasks =
            new Queue<NetTaskData>();

        static List<DeadLinkInfo> deadLinks = new List<DeadLinkInfo>();

        //set of url's that have been downloaded so we don't grab the
        //same one more than once
        static HashSet<string> processed = new HashSet<string>();

        //count of how many tasks are working
        static int numWorking = 0;

        //these are constant after main() starts up
        static int maxDistance;
        static Uri initialUrl;

        //convenience functions
        static T takeFromQueue<T>(Queue<T> Q)
        {
            lock (L)
            {
                numWorking--;
                if (numWorking == 0 && ItemsForCPUTasks.Count == 0 && ItemsForNetTasks.Count == 0)
                {
                    ItemsForCPUTasks.Enqueue(null);
                    ItemsForNetTasks.Enqueue(null);
                    Monitor.PulseAll(L);
                    return default(T);
                }
                while (Q.Count == 0)
                    Monitor.Wait(L);
                numWorking++;
                var item = Q.Dequeue();
                if (item == null)
                {
                    Q.Enqueue(default(T));
                    Monitor.PulseAll(L);
                }
                return item;
            }
        }

        static void putToQueue<T>(Queue<T> Q, T item)
        {
            lock (L)
            {
                Q.Enqueue(item);
                Monitor.PulseAll(L);
            }
        }

        //the CPU worker code
        static void CPUTask()
        {
            var R = new Regex(@"<a\s+[^>]*href=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
            while (true)
            {
                var t = takeFromQueue(ItemsForCPUTasks);
                if (t == null)
                    return;
                string htmlContent = t.Item1;
                int distance = t.Item2;
                Uri originator = t.Item3;
                var MC = R.Matches(htmlContent);
                foreach (Match M in MC)
                {
                    string s = M.Groups[1].Value;
                    Uri next = new Uri(originator, s);
                    putToQueue(ItemsForNetTasks, new NetTaskData(next, distance + 1, originator));
                }
            }
        }

        //network worker code
        static void NetTask()
        {
            WebClient wc = new WebClient();
            while (true)
            {
                var t = takeFromQueue(ItemsForNetTasks);
                if (t == null)
                    return;
                Uri toFetch = t.Item1;
                int distance = t.Item2;
                Uri whoLinked = t.Item3;  //who linked to source

                lock (L)
                {
                    if (processed.Contains(toFetch.ToString()) || distance > maxDistance ||
                        initialUrl.Host != toFetch.Host)
                    {
                        continue;
                    }
                    else
                    {
                        processed.Add(toFetch.ToString());
                    }
                }

                lock (L)
                {
                    Console.WriteLine("Fetch " + toFetch + " at distance " + distance + (whoLinked == null ? "" : (" from " + whoLinked)));
                }

                try
                {
                    string s = wc.DownloadString(toFetch);
                    putToQueue(ItemsForCPUTasks, new CpuTaskData(s, distance, toFetch));
                }
                catch (Exception)
                {
                    lock (L)
                    {
                        deadLinks.Add(new DeadLinkInfo(toFetch, whoLinked));
                    }
                }
            }
        }


        public static void Main2(string[] args)
        {
            initialUrl = new Uri(args[0]);
            maxDistance = Convert.ToInt32(args[1]);

            Console.WriteLine("Crawl " + initialUrl);
            Console.WriteLine("Distance: " + maxDistance);

            ItemsForNetTasks.Enqueue(new NetTaskData(
                initialUrl, 0, null));

            var threads = new List<Thread>();
            int numcpu = 4;
            int numnet = 4;

            numWorking = numcpu + numnet;
            for (int i = 0; i < numcpu; ++i)
            {
                Thread t = new Thread(() => {
                    CPUTask();
                });
                t.Start();
                threads.Add(t);
            }
            for (int i = 0; i < numnet; ++i)
            {
                Thread t = new Thread(() => {
                    NetTask();
                });
                t.Start();
                threads.Add(t);
            }

            foreach (var t in threads)
                t.Join();

            Console.WriteLine("---------------------------------");
            foreach (var x in deadLinks)
            {
                Console.WriteLine(x.deadLink + " from " + x.whereFrom);
            }
        }
    }
}
