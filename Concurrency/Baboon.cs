using System;
using System.Threading;
using static Globals;

public class Baboon
{
    public static void onRope()
    {
        lock (M)
        {
            while (STATE == MACAQUES)
                Monitor.Wait(M);
            while (numMonkeys == MAX_MONKEYS)
                Monitor.Wait(M);
            if (numMonkeys == 0)
            {
                STATE = BABOONS;
                numMonkeys++;
            }
            else if (numMonkeys < MAX_MONKEYS)
                numMonkeys++;
        }
    }

    public static void offRope()
    {
        lock (M)
        {
            if (numMonkeys > 1)
            {
                numMonkeys--;
                Monitor.PulseAll(M);
            }
            else if (numMonkeys == 1)
            {
                numMonkeys--;
                STATE = NONE;
                Monitor.PulseAll(M);
            }
        }
    }
}

