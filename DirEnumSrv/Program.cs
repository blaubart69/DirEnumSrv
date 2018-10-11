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
            RunServer(SrvPort, stats);
        }
        static void RunServer(int port, Stats stats)
        {
            Task v4 = RunOneInterface(IPAddress.Any,     port, stats);
            Task v6 = RunOneInterface(IPAddress.IPv6Any, port, stats);
            Task[] all = new Task[] { v4, v6 };

            while ( ! Task.WaitAll(all, 2000) )
            {
                Console.Error.WriteLine($"connections: {stats.connections} requests received: {stats.requestsReceived}                                \r");
            }
        }
        static Task RunOneInterface(IPAddress ip, int port, Stats stats)
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
                                Socket socket = listener.AcceptSocket();
                                Interlocked.Increment(ref stats.connections);
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        EnumThread(socket, () => Interlocked.Increment(ref stats.requestsReceived));
                                    }
                                    catch (SocketException sox)
                                    {
                                        if ( sox.ErrorCode != 0x2745) // 2745 (WSAECONNABORTED)	The connection has been closed.
                                        {
                                            throw sox;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.Error.WriteLine("EnumThread");
                                        Console.Error.WriteLine(ex.Message);
                                    }
                                    finally
                                    {
                                        Console.Error.WriteLine("Closing socket");
                                        socket.Close();
                                        Interlocked.Decrement(ref stats.connections);
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("AcceptSocket");
                            Console.Error.WriteLine(ex.Message);
                        }
                    });
        }
        static void EnumThread(Socket socket, Action OnRequest)
        {
            byte[] recBuff = new byte[1024];
            while (true)
            {
                int numReceived = socket.Receive(recBuff);

                OnRequest?.Invoke();
                string DirToEnum = Encoding.UTF8.GetString(recBuff,0, numReceived);
                Console.Out.WriteLine($"dirToEnum [{DirToEnum}], numReceived {numReceived}");

                socket.Send(Encoding.UTF8.GetBytes(@"c:\bumsti\hallo.txt"));
            }
        }
    }
}
