using Microsoft.Win32.SafeHandles;

using System;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

/// <summary>
///
/// </summary>
namespace DelHardlinks
{
	using DWORD = UInt32;

	/// <summary>
	///
	/// </summary>
	public static partial class Program
	{
#pragma warning disable CA1707, S4070, CA1034, CA1051, CA1401, S4214, S1104, S2346

		/// <summary>
		///
		/// </summary>
		[SuppressUnmanagedCodeSecurity()]
		internal static class NativeMethods
		{
			/// <summary>
			///
			/// </summary>
			[Flags]
			public enum ShareMode : DWORD
			{
				FILE_NO_SHARE = 0x0,
				FILE_SHARE_READ = 0x1,
				FILE_SHARE_WRITE = 0x2,
				FILE_SHARE_READ_WRITE = FILE_SHARE_READ | FILE_SHARE_WRITE,
				FILE_SHARE_DELETE = 0x4,
				FILE_SHARE_INHERITABLE = 0x10
			}

			/// <summary>
			///
			/// </summary>
			[Flags]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Readability", "RCS1234:Duplicate enum value.", Justification = "<Pending>")]
			public enum DesiredAccess : DWORD
			{
				FILE_ANY_ACCESS = 0x0,
				FILE_READ_DATA = 0x1,
				FILE_LIST_DIRECTORY = 0x1,
				FILE_WRITE_DATA = 1 << 1,
				FILE_ADD_FILE = 1 << 1,
				FILE_APPEND_DATA = 1 << 2,
				FILE_ADD_SUBDIRECTORY = 1 << 2,
				FILE_CREATE_PIPE_INSTANCE = 1 << 2,
				FILE_READ_EA = 1 << 3,
				FILE_WRITE_EA = 1 << 4,
				FILE_EXECUTE = 1 << 5,
				FILE_TRAVERSE = 1 << 5,
				FILE_DELETE_CHILD = 1 << 6,
				FILE_READ_ATTRIBUTES = 1 << 7,
				FILE_WRITE_ATTRIBUTES = 1 << 8,
#pragma warning disable RCS1157 // Composite enum value contains undefined flag.
				STANDARD_RIGHTS_REQUIRED = 0xF0000,
				SYNCHRONIZE = 1 << 20,
				STANDARD_RIGHTS_ALL = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE,
				FILE_ALL_ACCESS = STANDARD_RIGHTS_ALL | FILE_LIST_DIRECTORY | FILE_ADD_FILE | FILE_ADD_SUBDIRECTORY | FILE_READ_EA | FILE_WRITE_EA | FILE_EXECUTE | FILE_DELETE_CHILD | FILE_READ_ATTRIBUTES | FILE_WRITE_ATTRIBUTES,
#pragma warning restore RCS1157 // Composite enum value contains undefined flag.
			}

			//public enum iColumn : int
			//{
			//	Name          = 0,
			//	Size          = 1,
			//	Type          = 2,
			//	DateTimeModif = 3,
			//	Attrib        = 4,
			//	InfoTip       = -1,
			//}

			/// <summary>
			///
			/// </summary>
			[Flags]
			public enum FlagAttrib : DWORD
			{
				None = 0,
				FILE_ATTRIBUTE_NORMAL = 0x00000080,
				FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000,
				FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
				FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,
				FILE_FLAG_NO_BUFFERING = 0x20000000,
				FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
			}

			internal const int ERROR_MORE_DATA = 0xea;
			internal const int ERROR_HANDLE_EOF = 0x26;
			internal const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
			internal const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
			internal const int FSCTL_GET_REPARSE_POINT = 0x000900a8;
			internal const int CSIDL_BITBUCKET = 0x000a;
			//internal static readonly Guid FOLDERID_RecycleBinFolder = new Guid("B7534046-3ECB-4C18-BE4E-64CD4CB7D6AC");
			//internal static readonly Guid RecycleBinFolder = Guid.Parse("B7534046-3ECB-4C18-BE4E-64CD4CB7D6AC");

			/// <summary>
			///
			/// </summary>
			[StructLayout(LayoutKind.Sequential)]
			public struct BY_HANDLE_FILE_INFORMATION
			{
				public uint dwFileAttributes;
				public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
				public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
				public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
				public uint dwVolumeSerialNumber;
				public uint nFileSizeHigh;
				public uint nFileSizeLow;
				public uint nNumberOfLinks;
				public uint nFileIndexHigh;
				public uint nFileIndexLow;
			}

			/// <summary>
			///
			/// </summary>

			[StructLayout(LayoutKind.Sequential)]
			public struct REPARSE_DATA_BUFFER_SYMB
			{
				public uint ReparseTag;
				public ushort ReparseDataLength;
				public ushort Reserved;
				public ushort SubstituteNameOffset;
				public ushort SubstituteNameLength;
				public ushort PrintNameOffset;
				public ushort PrintNameLength;
				public uint Flags;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
				public byte[] PathBuffer;
			}

			/// <summary>
			///
			/// </summary>

			[StructLayout(LayoutKind.Sequential)]
			public struct REPARSE_DATA_BUFFER_MOUNT
			{
				public uint ReparseTag;
				public ushort ReparseDataLength;
				public ushort Reserved;
				public ushort SubstituteNameOffset;
				public ushort SubstituteNameLength;
				public ushort PrintNameOffset;
				public ushort PrintNameLength;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
				public byte[] PathBuffer;
			}

			/// <summary>
			///
			/// </summary>
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
			public struct SHFILEOPSTRUCT
			{
				public IntPtr hwnd;
				public FileFuncFlags wFunc;

				[MarshalAs(UnmanagedType.LPWStr)]
				public string pFrom;

				[MarshalAs(UnmanagedType.LPWStr)]
				public string pTo;

				public FILEOP_FLAGS fFlags;

				[MarshalAs(UnmanagedType.Bool)]
				public bool fAnyOperationsAborted;

				public IntPtr hNameMappings;

				[MarshalAs(UnmanagedType.LPWStr)]
				public string lpszProgressTitle;
			}

			/// <summary>
			///
			/// </summary>
			public enum FileFuncFlags : uint
			{
				FO_MOVE = 0x1,
				FO_COPY = 0x2,
				FO_DELETE = 0x3,
				FO_RENAME = 0x4
			}

			/// <summary>
			///
			/// </summary>

			[Flags]
			public enum FILEOP_FLAGS : ushort
			{
				None = 0,
				FOF_MULTIDESTFILES = 0x1,
				FOF_CONFIRMMOUSE = 0x2,

				/// Don't create progress/report
				FOF_SILENT = 0x4,

				FOF_RENAMEONCOLLISION = 0x8,

				/// Don't prompt the user.
				FOF_NOCONFIRMATION = 0x10,

				/// Fill in SHFILEOPSTRUCT.hNameMappings.
				/// Must be freed using SHFreeNameMappings
				FOF_WANTMAPPINGHANDLE = 0x20,

				FOF_ALLOWUNDO = 0x40,

				/// On *.*, do only files
				FOF_FILESONLY = 0x80,

				/// Don't show names of files
				FOF_SIMPLEPROGRESS = 0x100,

				/// Don't confirm making any needed dirs
				FOF_NOCONFIRMMKDIR = 0x200,

				/// Don't put up error UI
				FOF_NOERRORUI = 0x400,

				/// Dont copy NT file Security Attributes
				FOF_NOCOPYSECURITYATTRIBS = 0x800,

				/// Don't recurse into directories.
				FOF_NORECURSION = 0x1000,

				/// Don't operate on connected elements.
				FOF_NO_CONNECTED_ELEMENTS = 0x2000,

				/// During delete operation,
				/// warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
				FOF_WANTNUKEWARNING = 0x4000,

				/// Treat reparse points as objects, not containers
				FOF_NORECURSEREPARSE = 0x8000
			}

			/// <summary>
			/// Creates or opens a file or I/O device.
			/// </summary>
			/// <param name="fileName"></param>
			/// <param name="dwDesiredAccess"></param>
			/// <param name="dwShareMode"></param>
			/// <param name="securityAttrs_MustBeZero"></param>
			/// <param name="dwCreationDisposition"></param>
			/// <param name="dwFlagsAndAttributes"></param>
			/// <param name="hTemplateFile_MustBeZero"></param>
			/// <returns></returns>
			[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
			internal static extern SafeFileHandle CreateFile(
															String fileName,
															DesiredAccess dwDesiredAccess,
															ShareMode dwShareMode,
															IntPtr securityAttrs_MustBeZero,
															System.IO.FileMode dwCreationDisposition,
															FlagAttrib dwFlagsAndAttributes,
															IntPtr hTemplateFile_MustBeZero);

			/// <summary>
			/// Closes an open object handle.
			/// </summary>
			/// <param name="handle"></param>
			/// <returns></returns>
			[DllImport("kernel32", SetLastError = true)]
			[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
			internal static extern bool CloseHandle(SafeFileHandle handle);

			/// <summary>
			/// Creates an enumeration of all the hard links to the specified file.
			/// </summary>
			/// <param name="lpFileName"></param>
			/// <param name="dwFlags"></param>
			/// <param name="StringLength"></param>
			/// <param name="LinkName"></param>
			/// <returns></returns>
			[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
			public static extern FindSafeHandle FindFirstFileName(
																string lpFileName,
																DWORD dwFlags,
																ref DWORD StringLength,
																[MarshalAs(UnmanagedType.LPWStr)]
																StringBuilder LinkName);

			/// <summary>
			/// Continues enumerating the hard links to a file using the handle.
			/// </summary>
			/// <param name="hFindStream"></param>
			/// <param name="StringLength"></param>
			/// <param name="LinkName"></param>
			/// <returns></returns>
			[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool FindNextFileName(
													FindSafeHandle hFindStream,
													ref DWORD StringLength,
													[MarshalAs(UnmanagedType.LPWStr)]
													StringBuilder LinkName);

			/// <summary>
			/// Closes a file search handle
			/// </summary>
			/// <param name="hFindFile"></param>
			/// <returns></returns>
			[DllImport("kernel32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool FindClose(IntPtr hFindFile);

			/// <summary>
			/// Copies, moves, renames, or deletes a file system object.
			/// </summary>
			/// <param name="lpFileOp"></param>
			/// <returns></returns>
			[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
			public static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);

			/// <summary>
			/// Retrieves file information for the specified file.
			/// </summary>
			/// <param name="hFile"></param>
			/// <param name="hfi"></param>
			/// <returns></returns>
			[DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
			public static extern bool GetFileInformationByHandle(
																SafeFileHandle hFile,
																ref BY_HANDLE_FILE_INFORMATION hfi);

			/// <summary>
			/// Sends a control code directly to a specified device driver
			/// </summary>
			/// <param name="hDevice"></param>
			/// <param name="dwIoControlCode"></param>
			/// <param name="lpInBuffer"></param>
			/// <param name="nInBufferSize"></param>
			/// <param name="lpOutBuffer"></param>
			/// <param name="nOutBufferSize"></param>
			/// <param name="lpBytesReturned"></param>
			/// <param name="lpOverlapped"></param>
			/// <returns></returns>
			[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
			public static extern bool DeviceIoControl(
													SafeFileHandle hDevice,
													uint dwIoControlCode,
													IntPtr lpInBuffer,
													uint nInBufferSize,
													IntPtr lpOutBuffer,
													uint nOutBufferSize,
													out uint lpBytesReturned,
													IntPtr lpOverlapped);

			/// <summary>
			///
			/// </summary>
			/// <param name="folderPath"></param>
			/// <returns></returns>
			public static Shell32.Folder GetShell32Folder(string folderPath)
			{
				Type shellAppType = Type.GetTypeFromProgID("Shell.Application");
				Object shell = Activator.CreateInstance(shellAppType);
				return (Shell32.Folder)shellAppType.InvokeMember("NameSpace",
				System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { folderPath },
				CultureInfo.CurrentCulture);
			}
		}
	}
}