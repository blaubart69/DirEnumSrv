using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirEnumSrv
{
    class SrvAsync
    {
        public static void Run(IPAddress[] ips, int port, Stats stats, CancellationToken cancel)
        {
            IEnumerable<TcpListener> ifaceListeners = ips.Select((IPAddress ip) =>
              {
                  TcpListener listener = new TcpListener(ip, port);
                  listener.Start();
                  Console.WriteLine($"listening on interface {ip} port {port}");
                  return listener;
              }).ToList();

            int acceptAsyncWaiting = 0;
            for (int i=0; i < 128; ++i)
            {
                foreach ( var ilistener in ifaceListeners )
                {
                    StartInterface(ilistener, stats).ConfigureAwait(false);
                    ++acceptAsyncWaiting;
                }
            }
            Console.WriteLine($"acceptAsync waiting: {acceptAsyncWaiting}");
            cancel.WaitHandle.WaitOne();
        }
        private static async Task StartInterface(TcpListener listener, Stats stats)
        {
            while (true)
            {
                try
                {
                    Socket socket = await listener.AcceptSocketAsync();
                    Interlocked.Increment(ref stats.connections);

                    using (NetworkStream SocketStream = new NetworkStream(socket, ownsSocket: true))
                    {
                        await HandleConnection(SocketStream, stats);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref stats.connections);
                }
            }
        }
        private static async Task HandleConnection(NetworkStream socketStream, Stats stats)
        {
            try
            {
                while (true)
                {
                    string dirname = await ReceiveDirname(socketStream);
                    if (dirname == null)
                    {
                        break;
                    }
                    Interlocked.Increment(ref stats.requestsReceived);
                    Console.WriteLine($"dirname received: {dirname}");
                    await SendDirectory.SendFindDataAsync(socketStream, dirname);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
        private static async Task<string> ReceiveDirname(NetworkStream socketStream)
        {
            byte[] dirnameBuffer = new byte[256];

            int bytesReceived = 0;
            int tmpBytes = 1;
            UInt16 lenBytesDirname = 0;

            while (tmpBytes > 0 && bytesReceived-2 < (int)lenBytesDirname)
            {
                int bytesLeftInBuffer = dirnameBuffer.Length - bytesReceived;
                if (bytesLeftInBuffer == 0 )
                {
                    Array.Resize(ref dirnameBuffer, dirnameBuffer.Length * 4);
                    bytesLeftInBuffer = dirnameBuffer.Length - bytesReceived;
                }
                
                tmpBytes = await socketStream.ReadAsync(dirnameBuffer, bytesReceived, bytesLeftInBuffer);
                bytesReceived += tmpBytes;

                if (lenBytesDirname == 0)
                { 
                    if ( bytesReceived >= 2 )
                    {
                        lenBytesDirname = BitConverter.ToUInt16(dirnameBuffer, 0);
                        if ( lenBytesDirname == 0 )
                        {
                            return null;
                        }
                    }
                }
            }

            if ( bytesReceived-2 > lenBytesDirname)
            {
                throw new Exception($"bytesReceived-2 > lenBytesDirname : {lenBytesDirname} > {bytesReceived-2}");
            }

            string dirname = (lenBytesDirname > 0) 
                ? Encoding.UTF8.GetString(dirnameBuffer, 2, lenBytesDirname) 
                : null;

            return dirname;
        }
    }
}
