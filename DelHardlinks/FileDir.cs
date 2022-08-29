using Microsoft.Win32.SafeHandles;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

using static DelHardlinks.Program.NativeMethods;

/// <summary>
///
/// </summary>
namespace DelHardlinks
{
	using DWORD = UInt32;

	// http://www.flexhex.com/docs/articles/hard-links.phtml
	/// <summary>
	///
	/// </summary>
	public static partial class Program
	{
		/// <summary>
		///
		/// </summary>
		internal class FileDir : IDisposable
		{
			/// <summary>
			///
			/// </summary>
			public enum LinkType
			{
				None,
				Unknown,
				Hardlink,
				Symbolic,
				Junction
			};

			/// <summary>
			///
			/// </summary>
			public enum Error
			{
				None,
				FileNotFound,
				DirectoryNotFound,
				PathTooLong,
				Security,
				Other,
				UnauthorizedAccess,
				DriveNotFound
			};

			/// <summary>
			///
			/// </summary>
			private const string _pathHeader = @"\\?\";

			/// <summary>
			///
			/// </summary>
			private bool _disposedValue = false;

			/// <summary>
			///
			/// </summary>
			private readonly string _path;

			/// <summary>
			///
			/// </summary>
			public FileAttributes FileAttribute { get; }

			/// <summary>
			///
			/// </summary>
			public bool IsReparsePoint
			{
				get { return isReparsePoint; }
			}

			/// <summary>
			///
			/// </summary>
			public bool IsFile
			{
				get { return isFile; }
			}

			/// <summary>
			///
			/// </summary>
			public List<string> Links { get; set; } = new List<string>();

			/// <summary>
			///
			/// </summary>
			private readonly SafeFileHandle _handle;

			/// <summary>
			///
			/// </summary>
			private readonly string[] _parent_del = new string[] { ".." + Path.DirectorySeparatorChar };

			/// <summary>
			///
			/// </summary>
			private readonly char[] _dir_sep_char = new char[] { '\\' };

			/// <summary>
			///
			/// </summary>
			private readonly bool isReparsePoint;

			/// <summary>
			///
			/// </summary>
			private readonly bool isFile;

			/// <summary>
			///
			/// </summary>
			/// <param name="basepath"></param>
			/// <param name="target_path"></param>
			/// <returns></returns>
			public static string GetPath(string basepath, string target_path)
			{
				//if (!IsPathFullyQualified(basepath))
				//{
				//	throw new ArgumentException(R.Err_BasepathIsNotRooted);
				//}

				return GetAbsoluteFullPath(basepath, target_path);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="basepath"></param>
			/// <param name="target_path"></param>
			/// <returns></returns>
			public static string GetAbsoluteFullPath(string basepath, string target_path)
			{
				string path = Path.GetDirectoryName(basepath);
				path = Path.Combine(path, target_path);
				//RemoveEllipsis(ref final_path);
				path = Path.GetFullPath(path);
				return path;
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="path"></param>
			//public void RemoveEllipsis(ref string path)
			//{
			//	int count = path.Split(_parent_del, StringSplitOptions.None).Length - 1; // int count = source.Count(f => f == '/');
			//	if (count > 0)
			//	{
			//		string[] temp = path.Split(_dir_sep_char, StringSplitOptions.None);
			//		int pos = Array.IndexOf(temp, "..");
			//		while (pos != -1)
			//		{
			//			temp = temp.Where((_, idx) => idx != pos && idx != pos - 1).ToArray();
			//			pos = Array.IndexOf(temp, "..");
			//		}

			//		path = String.Join($"{Path.DirectorySeparatorChar}", temp);
			//	}
			//}

			/// <summary>
			///
			/// </summary>
			/// <param name="path"></param>
			/// <param name="desAcs"></param>
			/// <param name="shMode"></param>
			/// <param name="creDisp"></param>
			public FileDir(
							string path,
							DesiredAccess desAcs = DesiredAccess.FILE_READ_EA,
							ShareMode shMode = ShareMode.FILE_NO_SHARE,
							FileMode creDisp = FileMode.Open)
			{
				_path = path;
				if (_path.IndexOf(_pathHeader, StringComparison.Ordinal) < 0)
				{
					_path = _pathHeader + _path;
				}

				FileAttrib(_path, out isReparsePoint, out isFile);

				var flagAtt = FlagAttrib.FILE_ATTRIBUTE_NORMAL;
				if (!isFile)
				{
					flagAtt = FlagAttrib.FILE_FLAG_BACKUP_SEMANTICS;
				}
				if (isReparsePoint)
				{
					flagAtt |= FlagAttrib.FILE_FLAG_OPEN_REPARSE_POINT;
				}

				_handle = CreateFile(_path,
									desAcs,
									shMode,
									IntPtr.Zero,
									creDisp,
									flagAtt,
									IntPtr.Zero);

				if (_handle?.IsInvalid != false)
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="path"></param>
			/// <param name="isreparsepoint"></param>
			/// <param name="isfile"></param>
			public static void FileAttrib(
											string path,
											out bool isreparsepoint,
											out bool isfile)
			{
				FileAttributes fileattr = File.GetAttributes(path);
				isreparsepoint = (fileattr & FileAttributes.ReparsePoint) != 0;
				isfile = (fileattr & FileAttributes.Directory) == 0;
			}

			/// <summary>
			///
			/// </summary>
			/// <returns></returns>
			public bool IsValid()
			{
				return !(_handle.IsClosed || _handle.IsInvalid);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="dirInfo"></param>
			/// <param name="linktype"></param>
			/// <returns></returns>
			public bool GetFileDirInfo(
										ref BY_HANDLE_FILE_INFORMATION dirInfo,
										out LinkType linktype)
			{
				linktype = LinkType.Unknown;
				Links.Clear();
				if (IsValid() && GetFileInformationByHandle(
															_handle,
															ref dirInfo))
				{
					linktype = GetLinkType(dirInfo, out string target);

					if (linktype == LinkType.Hardlink)
					{
						using (FindFileDir findHardLinks = new FindFileDir())
						{
							findHardLinks.FindFileName(_path);

							foreach (var link in findHardLinks.Links)
							{
								Links.Add(GetPath(_path, link));
							}
						}
					}
					else if (linktype == LinkType.Symbolic || linktype == LinkType.Junction)
					{
						Links.Add(target);
					}
					return true;
				}
				return false;
			}

			/// <summary>
			///
			/// </summary>
			public struct Info
			{
				public bool isfile;
				public string path;
				public LinkType linktype;
				public uint n_links;
				public Error error;
				public int error_code;
				public string error_msg;
			}


			public static LinkType FileDirAndLinks(
													ref Info info,
													ref List<Info> links)
			{
				info.isfile = false;
				info.linktype = LinkType.None;
				info.n_links = 0;
				info.error = Error.None;
				info.error_code = 0;
				info.error_msg = string.Empty;

				try
				{
					using (FileDir file = new FileDir(info.path))
					{
						var dirInfo = new BY_HANDLE_FILE_INFORMATION();
						
						file.GetFileDirInfo(ref dirInfo, out info.linktype);
						
						info.isfile = (dirInfo.dwFileAttributes & (DWORD)FileAttributes.Directory) == 0;
						
						info.n_links = dirInfo.nNumberOfLinks;

						if (links != null)
						{
							LinkType linktype2 = LinkType.None;
							Error error = Error.None;
							string error_msg = string.Empty;
							bool isfile = false;
							int err_code = 0;
							foreach (string link in file.Links)
							{
								var dirInfo2 = new BY_HANDLE_FILE_INFORMATION();
								try
								{
									using (FileDir link_file = new FileDir(link))
									{
										link_file.GetFileDirInfo(ref dirInfo2, out linktype2);
										isfile = (dirInfo2.dwFileAttributes &
											(DWORD)FileAttributes.Directory) == 0;
									}
								}
								catch (Exception ex)
								{
									error_msg = ex.Message;
									error = Error.Other;
									err_code = Marshal.GetLastWin32Error();
								}
								links.Add(new Info()
								{
									isfile = isfile,
									path = link,
									linktype = linktype2,
									n_links = dirInfo2.nNumberOfLinks,
									error = error,
									error_code = err_code,
									error_msg = error_msg
								});
							}
						}
					}
				}
				catch (Exception ex)
				{
					info.error_msg = ex.Message;
					info.error = Error.Other;
					info.error_code = Marshal.GetLastWin32Error();
				}
				return info.linktype;
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="dirInfo"></param>
			/// <param name="target"></param>
			/// <returns></returns>
			private LinkType GetLinkType(BY_HANDLE_FILE_INFORMATION dirInfo, out string target)
			{
				LinkType linktype = LinkType.None;
				target = string.Empty;
				string link = string.Empty;

				if (isReparsePoint)
				{
					REPARSE_DATA_BUFFER_SYMB repDataBuff = new REPARSE_DATA_BUFFER_SYMB();
					int structSize = Marshal.SizeOf(typeof(REPARSE_DATA_BUFFER_SYMB));
					IntPtr structMem = Marshal.AllocHGlobal(structSize);

					Marshal.StructureToPtr(repDataBuff, structMem, true);// false);

					if (DeviceIoControl(
										_handle,
										FSCTL_GET_REPARSE_POINT,
										IntPtr.Zero, 0,
										structMem, (uint)structSize,
										out DWORD _, // len_ret
										IntPtr.Zero))
					{
						repDataBuff = (REPARSE_DATA_BUFFER_SYMB)Marshal.PtrToStructure(
																					structMem,
																					typeof(REPARSE_DATA_BUFFER_SYMB));
						if (repDataBuff.ReparseTag == IO_REPARSE_TAG_MOUNT_POINT)
						{
							REPARSE_DATA_BUFFER_MOUNT repDataBuff2 = (REPARSE_DATA_BUFFER_MOUNT)Marshal.PtrToStructure(
																						structMem,
																						typeof(REPARSE_DATA_BUFFER_MOUNT));
							linktype = LinkType.Junction;
							//string _link = Encoding.Unicode.GetString(
							//										repDataBuff2.PathBuffer,
							//										repDataBuff2.SubstituteNameOffset,
							//										repDataBuff2.SubstituteNameLength);
							link = Encoding.Unicode.GetString(
															repDataBuff2.PathBuffer,
															repDataBuff2.PrintNameOffset,
															repDataBuff2.PrintNameLength);
						}
						else if (repDataBuff.ReparseTag == IO_REPARSE_TAG_SYMLINK)
						{
							linktype = LinkType.Symbolic;
							//string _link = Encoding.Unicode.GetString(
							//										repDataBuff.PathBuffer,
							//										repDataBuff.SubstituteNameOffset,
							//										repDataBuff.SubstituteNameLength);
							link = Encoding.Unicode.GetString(
															repDataBuff.PathBuffer,
															repDataBuff.PrintNameOffset,
															repDataBuff.PrintNameLength);
						}

						target = link;
						Marshal.FreeHGlobal(structMem);
					}
					else
					{
						throw new Win32Exception(Marshal.GetLastWin32Error());
					}
				}
				else if (isFile && dirInfo.nNumberOfLinks > 1)
				{
					linktype = LinkType.Hardlink;
				}

				return linktype;
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="disposing"></param>
			protected virtual void Dispose(bool disposing)
			{
				if (!_disposedValue)
				{
					//if (disposing)
					//{
					//}

					_handle?.Dispose();
					_disposedValue = true;
				}
			}

			/// <summary>
			///
			/// </summary>
			public void Dispose()
			{
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				Dispose(true);
			}
		}
	}
}