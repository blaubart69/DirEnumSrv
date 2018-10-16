using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirEnumSrv
{
    class Stats
    {
        public long connections;
        public long requestsReceived;
    }

    class Program
    {
        static void Main(string[] args)
        {
            const int SrvPort = 44000;
            Stats stats = new Stats();

            CancellationTokenSource cts = new CancellationTokenSource();
            RunServerAsync(SrvPort, stats, cts.Token);
        }
        static void RunServerAsync(int port, Stats stats, CancellationToken ct)
        {
            SrvAsync.Run(new IPAddress[] { IPAddress.Any, IPAddress.IPv6Any }, port, stats, ct);
        }
        static void RunServer(int port, Stats stats)
        {
            using (Semaphore sem = new Semaphore(32, 32))
            {
                Task v4 = RunOneInterface(IPAddress.Any,     port, stats, sem);
                Task v6 = RunOneInterface(IPAddress.IPv6Any, port, stats, sem);
                Task[] all = new Task[] { v4, v6 };

                while (!Task.WaitAll(all, 2000))
                {
                    Console.Error.Write($"connections: {stats.connections} requests received: {stats.requestsReceived}                                \r");
                }
            }
        }
        static Task RunOneInterface(IPAddress ip, int port, Stats stats, Semaphore sem)
        {
            TcpListener listener = new TcpListener(ip, port);
            listener.Start();
            Console.WriteLine($"listening on interface {ip} port {port}");

            return 
                Task.Run( 
                () =>
                    {
                        try
                        {
                            while (true)
                            {
                                sem.WaitOne();
                                Socket socket = listener.AcceptSocket();
                                Interlocked.Increment(ref stats.connections);
                                StartEnumThread(socket, stats, sem);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("AcceptSocket");
                            Console.Error.WriteLine(ex.Message);
                        }
                    });
        }
        static void StartEnumThread(Socket socket, Stats stats, Semaphore sem)
        {
            Task.Run(() =>
            {
                try
                {
                    EnumThread(socket, () => Interlocked.Increment(ref stats.requestsReceived));
                }
                catch (SocketException sox)
                {
                    if (sox.ErrorCode != 0x2745) // 2745 (WSAECONNABORTED)	The connection has been closed.
                    {
                        throw sox;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("EnumThread");
                    Console.Error.WriteLine(ex);
                }
                finally
                {
                    socket.Close();
                    Interlocked.Decrement(ref stats.connections);
                    sem.Release();
                }
            });
        }
        static void EnumThread(Socket socket, Action OnRequest)
        {
            byte[] recBuff = new byte[1024];
            while (true)
            {
                int numReceived = socket.Receive(recBuff);
                if ( numReceived == 0)
                {
                    break;
                }

                OnRequest?.Invoke();
                string DirToEnum = Encoding.UTF8.GetString(recBuff,0, numReceived);

                SendDirectory.RunSendFindData(socket, DirToEnum);
            }
        }
    }
}
