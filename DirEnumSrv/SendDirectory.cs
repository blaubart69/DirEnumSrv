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
        const byte DATA     = 0x1;
        const byte DATAEND  = 0x3;
        const byte ERROR    = 0x80;

        public static async Task SendFindDataAsync(NetworkStream stream, string dirname)
        {
            IntPtr dirHandle = OpenDirectoryHandle(dirname);
            if ( dirHandle == Native.INVALID_HANDLE_VALUE )
            {
                SendError(stream, System.Runtime.InteropServices.Marshal.GetLastWin32Error());
                return;
            }

            byte[] buf = new byte[4096];
            buf[0] = DATA;

            try
            {
                unsafe
                {
                    fixed (byte* b = buf)
                    {
                        byte* dirInfoBuffer = b + 1;

                        do
                        { 
                            bool 
                        while (Native.GetFileInformationByHandleEx(
                            dirHandle
                            , Native.FILE_INFO_BY_HANDLE_CLASS.FileFullDirectoryInfo
                            , dirInfoBuffer
                            , (uint)buf.Length - 1))
                        {
                            int DataLenToSend = CalcDirInfoLength(dirInfoBuffer);
                            await stream.WriteAsync(buf, 0, DataLenToSend);
                        }
                    }
                }
            }
            finally
            {
                Native.CloseHandle(dirHandle);
            }

        }
        /// <summary>
        ///     typedef struct _FILE_FULL_DIR_INFO {
        ///       8  0  ULONG NextEntryOffset;              
        ///       8  4 ULONG FileIndex;
        ///       8  8 LARGE_INTEGER CreationTime;
        ///       8 24 LARGE_INTEGER LastAccessTime;
        ///       8 32 LARGE_INTEGER LastWriteTime;
        ///       8 40 LARGE_INTEGER ChangeTime;
        ///       8 48 LARGE_INTEGER EndOfFile;
        ///       8 56 LARGE_INTEGER AllocationSize;
        ///       8 64 ULONG FileAttributes;
        ///       8 72 ULONG FileNameLength;
        ///       8 80 ULONG EaSize;
        ///       2 88 WCHAR FileName[1];
        ///     }
        ///     FILE_FULL_DIR_INFO, * PFILE_FULL_DIR_INFO;
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        /// 
        const int LenStructWithoutFilename = 6 * sizeof(ulong) + 5 * sizeof(uint);

        private unsafe static int CalcDirInfoLength(byte* buf)
        {
            Native.FILE_FULL_DIR_INFO* info;
            int bufLen = 0;
            do
            {
                info = (Native.FILE_FULL_DIR_INFO*)(buf + bufLen);
                bufLen += (int)info->NextEntryOffset;
            } while (info->NextEntryOffset != 0 );

            bufLen += LenStructWithoutFilename + (int)info->FileNameLength;

            return bufLen;
        }
        private static IntPtr OpenDirectoryHandle(string dirname)
        {
            return 
                Native.CreateFileW(
                    dirname
                    , FileAccess.Read
                    , FileShare.ReadWrite
                    , IntPtr.Zero
                    , FileMode.Open
                    , (FileAttributes)Native.FILE_FLAG_BACKUP_SEMANTICS
                    , IntPtr.Zero);
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
