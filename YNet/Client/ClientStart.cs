// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-25
// ------------------------------
using System;

class ClientStart
{
    private static KCPClientRoot clientRoot;

    private static void Main()
    {
        clientRoot = new KCPClientRoot();
        clientRoot.Start();
        Console.ReadKey();
    }
}