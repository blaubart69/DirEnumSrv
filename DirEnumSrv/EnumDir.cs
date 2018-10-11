using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;


using Spi;
using System.Text;

namespace DirEnumSrv
{
    public class EnumDir
    {
        public delegate void OnFindData(ref FIND_DATA_RAW find_data);

        public static int EntriesEx(string FullFindFirstFileString, OnFindData FindDataCallback)
        {
            FIND_DATA_RAW find_data = new FIND_DATA_RAW();
            int rc = 0;
            IntPtr SearchHandle = IntPtr.Zero;
            try
            { 
                SearchHandle = Spi.Native.FindFirstFileEx(
                    FullFindFirstFileString
                    , Native.FINDEX_INFO_LEVELS.FindExInfoBasic
                    , ref find_data
                    , Native.FINDEX_SEARCH_OPS.FindExSearchNameMatch
                    , IntPtr.Zero
                    , Native.FINDEX_ADDITIONAL_FLAGS.FIND_FIRST_EX_LARGE_FETCH);

                if (SearchHandle == Native.INVALID_HANDLE_VALUE)
                {
                    rc = Marshal.GetLastWin32Error();
                }
                else
                {
                    do
                    {
                        if (IsDotOrDotDotDirectory(find_data.cFileName))
                        {
                            continue;
                        }
                        FindDataCallback(ref find_data);
                    }
                    while (Native.FindNextFileW(SearchHandle, ref find_data));

                    if (Marshal.GetLastWin32Error() != Native.ERROR_NO_MORE_FILES)
                    {
                        rc = Marshal.GetLastWin32Error();
                    }
                }
            }
            finally
            {
                if (SearchHandle != IntPtr.Zero)
                {
                    Native.FindClose(SearchHandle);
                }
            }
            return rc;
        }
        private static bool IsDotOrDotDotDirectory(UInt16[] Filename)
        {
            if (Filename[0] == '.')
            {
                if (Filename[1] == 0)
                {
                    return true;
                }
                if (Filename[1] == '.')
                {
                    if (Filename[2] == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static bool IsDotOrDotDotDirectory(char[] Filename)
        {
            if (Filename[0] == '.')
            {
                if (Filename[1] == 0)
                {
                    return true;
                }
                if (Filename[1] == '.')
                {
                    if (Filename[2] == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}