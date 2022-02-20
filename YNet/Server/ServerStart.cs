// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-25
// ------------------------------
using System;

class ServerStart
{
    private static KCPServerRoot serverRoot;

    private static void Main()
    {
        serverRoot = new KCPServerRoot();
        serverRoot.Start();
        Console.ReadKey();
    }
}