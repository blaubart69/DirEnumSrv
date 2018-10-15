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
            int lenFilename = LenOfFilename(find_data.cFileName);

            byte[] UTF8Filename = new byte[260 * 4];
            int UTF8byteswritten = 0;

            unsafe
            {
                fixed (UInt16* ptrFilename = find_data.cFileName)
                fixed (byte* utf8buffer = UTF8Filename)
                {
                    char* chars = (char*)ptrFilename;
                    UTF8byteswritten = Encoding.UTF8.GetBytes(
                        chars:      chars, 
                        charCount:  lenFilename, 
                        bytes:      utf8buffer, 
                        byteCount:  UTF8Filename.Length);
                }
            }

            bw.Write(UTF8Filename, 0, UTF8byteswritten);
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
