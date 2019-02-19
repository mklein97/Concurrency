using System;
using System.Threading;
using static Globals;

public class Baboon
{
    public static void onRope()
    {
        lock (M)
        {
            while (!isBaboons || numBaboons == MAX_MONKEYS)
                Monitor.Wait(M);
        }

        lock (M)
        {
            numBaboons++;
            isBaboons = true;
            Monitor.Pulse(M);
        }
    }

    public static void offRope()
    {
        lock (M)
        {
            while (!isBaboons || numBaboons == 0)
                Monitor.Wait(M);
        }

        lock (M)
        {
            if (numBaboons != 0)
                numBaboons--;
            else
                Monitor.PulseAll(M);
        }
    }

}

