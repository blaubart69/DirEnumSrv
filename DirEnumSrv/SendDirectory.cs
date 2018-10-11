using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Spi;

namespace DirEnumSrv
{
    public class SendDirectory
    {
        const int find_data_size = 9 * 4 + 260 * 2 + 1;

        public static void RunSendFindData(Socket socket, string FullFindFirstFileString)
        {
            byte[] buf = new byte[4096];
            MemoryStream ms = new MemoryStream(buf,0,buf.Length, true, true);
            BinaryWriter bw = new BinaryWriter(ms);

            EnumDir.EntriesEx(FullFindFirstFileString,
                (ref FIND_DATA_RAW find_data) =>
                {
                    if ( (ms.Capacity - ms.Position) < find_data_size)
                    {
                        socket.Send(ms.GetBuffer());
                        ms.Seek(0, SeekOrigin.Begin);
                    }
                    BuildSendData(ref find_data, bw);
                });

            if ( ms.Position > 0 )
            {
                socket.Send(ms.GetBuffer());
                ms.Seek(0, SeekOrigin.Begin);
            }
        }
        private static void BuildSendData(ref FIND_DATA_RAW find_data, BinaryWriter bw)
        {
            //bw.Write(find_data.data, 0, 36);

            StringBuilder sb = new StringBuilder();
            int i = 0;
            while ( find_data.cFileName[i] != 0 )
            {
                sb.Append((char)find_data.cFileName[i]);
                ++i;
            }

            byte[] Utf8Filename = Encoding.UTF8.GetBytes(sb.ToString());
            bw.Write(Utf8Filename, 0, Utf8Filename.Length);
            bw.Write((byte)0);
        }
        private static int LenOfFilename(UInt16[] szFilename)
        {
            int i=0;
            while (i <= szFilename.Length )
            {
                if (szFilename[i] == 0)
                {
                    break;
                }
                ++i;
            }
            return i;
        }
        private static int LenOfFilename(char[] szFilename)
        {
            int i = 0;
            while (i <= szFilename.Length)
            {
                if (szFilename[i] == 0)
                {
                    break;
                }
                ++i;
            }
            return i;
        }
    }
}
