using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Spi
{
    /*
     * typedef struct _WIN32_FIND_DATAW
    {
        DWORD dwFileAttributes;
        FILETIME ftCreationTime;
        FILETIME ftLastAccessTime;
        FILETIME ftLastWriteTime;
        DWORD nFileSizeHigh;
        DWORD nFileSizeLow;
        DWORD dwReserved0;
        DWORD dwReserved1;
        _Field_z_ WCHAR  cFileName[MAX_PATH];
        _Field_z_ WCHAR  cAlternateFileName[14];
        #ifdef _MAC
        DWORD dwFileType;
        DWORD dwCreatorType;
        WORD wFinderFlags;
#endif
        */

    //[StructLayout(LayoutKind.Explicit)]
    //public struct FIND_DATA_RAW
    //{
    //    [FieldOffset( 0)]  public readonly UInt32 dwFileAttributes;
    //    [FieldOffset( 4)]  public readonly UInt32 ftCreateLow;
    //    [FieldOffset( 8)]  public readonly UInt32 ftCreateHigh;
    //    [FieldOffset(12)]  public readonly UInt32 ftAccessLow;
    //    [FieldOffset(16)]  public readonly UInt32 ftAccessHigh;
    //    [FieldOffset(20)]  public readonly UInt32 ftWriteLow;
    //    [FieldOffset(24)]  public readonly UInt32 ftWriteHigh;
    //    [FieldOffset(28)]  public readonly UInt32 nFileSizeHigh;
    //    [FieldOffset(32)]  public readonly UInt32 nFileSizeLow;
    //    [FieldOffset(36)]  public readonly UInt32 dwReserved0;
    //    [FieldOffset(40)]  public readonly UInt32 dwReserved1;

    //    [FieldOffset(44)]
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
    //    public readonly UInt16[] cFileName;

    //    //[FieldOffset(44+260*2)]
    //    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
    //    //public UInt16[] cAlternateFileName;

    //    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 9*4)]
    //    //[FieldOffset(0)] public readonly byte[] data;
    //}

    [StructLayout(LayoutKind.Sequential)]
    public struct FIND_DATA_RAW
    {
        public readonly UInt32 dwFileAttributes;

        UInt32 ftCreateLow;
        UInt32 ftCreateHigh;
        UInt32 ftAccessLow;
        UInt32 ftAccessHigh;
        UInt32 ftWriteLow;
        UInt32 ftWriteHigh;

        UInt32 nFileSizeHigh;
        UInt32 nFileSizeLow;

        UInt32 dwReserved0;
        UInt32 dwReserved1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public UInt16[] cFileName;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public UInt16[] cAlternateFileName;
    }

    [System.Security.SuppressUnmanagedCodeSecurity]
    public class Native
    {
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public const int ERROR_PATH_NOT_FOUND = 0x00000003;
        public const int ERROR_INVALID_PARAMETER = 0x00000057;
        public const int ERROR_DIRECTORY = 0x10B; // The directory name is invalid.
        public const int ERROR_NO_MORE_FILES = 0x12; // There are no more files.

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [System.Security.SuppressUnmanagedCodeSecurity]
        public static extern IntPtr FindFirstFileEx(
            string lpFileName,
            FINDEX_INFO_LEVELS fInfoLevelId,
            ref FIND_DATA_RAW lpFindFileData,
            FINDEX_SEARCH_OPS fSearchOp,
            IntPtr lpSearchFilter,
            FINDEX_ADDITIONAL_FLAGS dwAdditionalFlags);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool FindNextFileW(IntPtr hFindFile, ref FIND_DATA_RAW lpFindFileData);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool FindClose(IntPtr hFindFile);

        public enum FINDEX_INFO_LEVELS
        {
            FindExInfoStandard = 0,
            FindExInfoBasic = 1
        }
        public enum FINDEX_SEARCH_OPS
        {
            FindExSearchNameMatch = 0,
            FindExSearchLimitToDirectories = 1,
            FindExSearchLimitToDevices = 2
        }
        [Flags]
        public enum FINDEX_ADDITIONAL_FLAGS : int
        {
            FIND_FIRST_EX_CASE_SENSITIVE = 1,
            FIND_FIRST_EX_LARGE_FETCH = 2,
            FIND_FIRST_EX_ON_DISK_ENTRIES_ONLY = 4
        }

    }
}
