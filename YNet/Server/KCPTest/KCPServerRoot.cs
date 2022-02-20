// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-27
// ------------------------------
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using YNet.KCP;

public class KCPServerRoot
{
    private KCPServer<KCPServerSession> server;

    public KCPServerRoot()
    {
        server = new KCPServer<KCPServerSession>();
    }

    public void Start()
    {
        server.StartAsServer("127.0.0.1", 17666, false);
        while (true)
        {
            string input = Console.ReadLine();
            if (input == "quit")
            {
                server.CloseServer();
                break;
            }
            else
            {
                server.BroadCastMessage(Encoding.UTF8.GetBytes(input));
            }
        }
    }
}