using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
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

        public readonly UInt32 ftCreateLow;
        public readonly UInt32 ftCreateHigh;
        public readonly UInt32 ftAccessLow;
        public readonly UInt32 ftAccessHigh;
        public readonly UInt32 ftWriteLow;
        public readonly UInt32 ftWriteHigh;
        public readonly UInt32 nFileSizeHigh;
        public readonly UInt32 nFileSizeLow;
        public readonly UInt32 dwReserved0;
        public readonly UInt32 dwReserved1;

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
        public const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

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
        //
        // GetFileInformationByHandleEx()
        //
        public enum FILE_INFO_BY_HANDLE_CLASS : int
        {
            FileBasicInfo = 0,
            FileStandardInfo = 1,
            FileNameInfo = 2,
            FileRenameInfo = 3,
            FileDispositionInfo = 4,
            FileAllocationInfo = 5,
            FileEndOfFileInfo = 6,
            FileStreamInfo = 7,
            FileCompressionInfo = 8,
            FileAttributeTagInfo = 9,
            FileIdBothDirectoryInfo = 10,// 0x0A
            FileIdBothDirectoryRestartInfo = 11, // 0xB
            FileIoPriorityHintInfo = 12, // 0xC
            FileRemoteProtocolInfo = 13, // 0xD
            FileFullDirectoryInfo = 14, // 0xE
            FileFullDirectoryRestartInfo = 15, // 0xF
            FileStorageInfo = 16, // 0x10
            FileAlignmentInfo = 17, // 0x11
            FileIdInfo = 18, // 0x12
            FileIdExtdDirectoryInfo = 19, // 0x13
            FileIdExtdDirectoryRestartInfo = 20, // 0x14
            MaximumFileInfoByHandlesClass
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetFileInformationByHandleEx(
                    IntPtr                      hFile
            ,       FILE_INFO_BY_HANDLE_CLASS   infoClass
            ,   ref byte[]                      buffer
            ,       UInt32                      dwBufferSize);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", SetLastError = true)]
        public static unsafe extern bool GetFileInformationByHandleEx(
            IntPtr hFile
            , FILE_INFO_BY_HANDLE_CLASS infoClass
            , byte* buffer
            , UInt32 dwBufferSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct FILE_FULL_DIR_INFO
        {
            public uint NextEntryOffset;
            public uint FileIndex;
            public long CreationTime;
            public long LastAccessTime;
            public long LastWriteTime;
            public long ChangeTime;
            public long EndOfFile;
            public long AllocationSize;
            public uint FileAttributes;
            public uint FileNameLength;
            public uint EaSize;
            public UInt16 FileName;
        }

            //
            //
            //
            [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFileW(
             [MarshalAs(UnmanagedType.LPWStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes,
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [System.Runtime.ConstrainedExecution.ReliabilityContract(
            System.Runtime.ConstrainedExecution.Consistency.WillNotCorruptState, 
            System.Runtime.ConstrainedExecution.Cer.Success)]
        [System.Security.SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);
    }
}
