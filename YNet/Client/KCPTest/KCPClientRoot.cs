// Author：yaoshuai
// Email：yscode@126.com
// Date：2021-11-27
// ------------------------------
using System;
using System.Text;
using System.Threading.Tasks;

public class KCPClientRoot
{
    private readonly KCPClinetNet client;
    public static KCPClientRoot Instance { get; private set; } = null;

    public KCPClientRoot()
    {
        client = new KCPClinetNet();
        Instance = this;
    }

    public void Start()
    {
        client.StartAsClient("127.0.0.1", 17666, 0, false);
        BeginConnect();

        while (true)
        {
            string input = Console.ReadLine();
            if (input == "quit")
            {
                client.CloseClient();
                break;
            }
            else
            {
                client.ClientSession.SendMessage(Encoding.UTF8.GetBytes(input));
            }
        }
        Console.ReadKey();
    }

    public void BeginConnect()
    {
        checkConnectTask = client.ConnectServer(2000, 10000);
        Task.Run(BeginConnectTask);
    }
    private Task<bool> checkConnectTask;
    private int connectCount = 0;
    private async void BeginConnectTask()
    {
        while (true)
        {
            await Task.Delay(3000);
            if (checkConnectTask != null && checkConnectTask.IsCompleted)
            {
                if (checkConnectTask.Result)
                {
                    Console.WriteLine("客户端连接服务器成功");
                    checkConnectTask = null;
                    await Task.Run(SendPingMessage);
                    break;
                }
                else
                {
                    connectCount += 1;
                    if (connectCount > 3)
                    {
                        Console.WriteLine("客户端：已尝试3次连接，请检查您的网络");
                        checkConnectTask = null;
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"客户端：正在尝试第{connectCount}此连接");
                        checkConnectTask = client.ConnectServer(2000, 10000);
                    }
                }
            }
        }
    }

    private async void SendPingMessage()
    {
        while (true)
        {
            await Task.Delay(3000);
            if (client != null && client.ClientSession != null && client.IsConnected)
            {
                client.ClientSession.sendPingTime = DateTime.UtcNow;
                client.ClientSession.SendMessage(Encoding.UTF8.GetBytes("ping"));
            }
            else
            {
                break;
            }
        }
    }
}