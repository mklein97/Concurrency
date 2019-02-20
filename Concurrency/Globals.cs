using System;
using System.Security.Policy;

public class Globals
{
    public static int numMonkeys = 0;
    public const int MAX_MONKEYS = 3;
    public const int NONE = 0;
    public const int BABOONS = 1;
    public const int MACAQUES = 2;
    public static int STATE = NONE;
    public static object M = new object();
}

