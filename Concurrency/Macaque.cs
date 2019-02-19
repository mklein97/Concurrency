using System;
using System.Threading;
using static Globals;

public class Macaque
{
    public static void onRope()
    {
        lock (M)
        {
            while (isBaboons || numMacaques == MAX_MONKEYS)
                Monitor.Wait(M);
        }

        lock (M)
        {
            numMacaques++;
            isBaboons = false;
            Monitor.Pulse(M);
        }
    }

    public static void offRope()
    {
        lock (M)
        {
            while (isBaboons || numMacaques == 0)
                Monitor.Wait(M);
        }

        lock (M)
        {
            if (numMacaques != 0)
                numMacaques--;
            else
                Monitor.PulseAll(M);
        }
    }

}

