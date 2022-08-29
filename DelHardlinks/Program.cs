using Shell32;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;

using static DelHardlinks.Program.NativeMethods;

/// <summary>
///
/// </summary>
namespace DelHardlinks
{
	using R = Resources;

	/// <summary>
	///
	/// </summary>
	public static partial class Program
	{
		/// <summary>
		///
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="list"></param>
		internal static void Abort(string msg = "", params object[] list)
		{
			if (!string.IsNullOrEmpty(msg))
			{
				Console.WriteLine(string.Format(CultureInfo.InvariantCulture, msg, list));
			}
			Environment.Exit(1);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="path"></param>
		/// <param name="prefix"></param>
		/// <returns></returns>
		internal static string TestPath(string path, string prefix)
		{
			string ret = null;
			string[] msg = new[] {
				R.Err_NotValidPath,
				R.Err_UseRootedPath};

			do
			{
				if (string.IsNullOrWhiteSpace(path))
				{
					Console.WriteLine(prefix + msg[0]);
					break;
				}

				if (Path.GetPathRoot(path).Length <= 1)
				//if (!Path.IsPathRooted(path))
				{
					Console.WriteLine(prefix + msg[1]);
					break;
				}
				try
				{
					ret = Path.GetFullPath(path);
				}
				catch (Exception)
				{
					Console.WriteLine(prefix + msg[0]);
				}
			} while (false);

			return ret;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		internal static bool HasSubfolders(string path)
		{
			IEnumerable<string> subfolders = Directory.EnumerateDirectories(path);
			return subfolders?.Any() == true;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		internal static int FilesCount(string path)
		{
			IEnumerable<string> files = Directory.EnumerateFiles(path);
			return files.Count();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		internal static int SubfoldersCount(string path)
		{
			IEnumerable<string> subfolders = Directory.EnumerateDirectories(path);
			return subfolders.Count();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="sdta"></param>
		/// <param name="idta"></param>
		/// <param name="ops">Options assigned by the user</param>
		/// <param name="counters"></param>
		/// <param name="misc"></param>
		/// <param name="maxdiskusage"></param>
		/// <param name="vac"></param>
		/// <returns></returns>
		internal static bool DeepProc(
									Dat.StrData sdta,
									Dat.IntData idta,
									Dat.Ops ops,
									ref Dat.Counters counters,
									ref M misc,
									ref float maxdiskusage,
									ref List<int> vac)
		{
			bool procdir;
			bool deleted = false;
			string failedMsg = string.Empty;

			I _i = new I();

			counters.FilesInDirCounter = 0;
			counters.Hardlinks = 0;
			counters.Junctions = 0;
			counters.SymbLnks  = 0;

			I.PrintTree(idta, vac);

			List<FileDir.Info> links = new List<FileDir.Info>();

			FileDir.Info info = new FileDir.Info() { path = sdta.Dir };

			// test weak folder access
			FileDir.LinkType linktype = FileDir.FileDirAndLinks(
																ref info,
																ref links);

			Info.ColorFmt folder_color = GetLinkColor(linktype);

			// if error then print error and return
			if (info.error != FileDir.Error.None)
			{
				I.PrintTreeFolder(
									sdta,
									idta,
									ops,
									0,
									Info.ColorFmt.Error);

				I.InitPrint($" {info.error_msg} ! \n", I.ColorFmt.Warning);
				counters.Errors++;
				return deleted;
			}

			try
			{
				int TotalDirInDir = 0;

				// count subfolders 
				_i.PrintAutoReset(
								() => TotalDirInDir = SubfoldersCount(sdta.Dir),
								Dat.SP + R.Msg_RetrievingSubfolderData,
								Info.ColorFmt.Message);

				// print tree
				I.PrintTreeFolder(
									sdta,
									idta,
									ops,
									TotalDirInDir,
									folder_color);

				// delete if need
				deleted = Delete(
								ops,
								ref counters,
								info,
								sdta.Recyclebin,
								sdta.Log,
								ref links);

				// folder deleted
				if (deleted)
				{
					// print status
					_i.PrintStatus(
									counters,
									counters.Errors);
					I.InitPrint(" Deleted !\n", I.ColorFmt.Warning);
				}
				else // folder NOT deleted. Proc folder items
				{
					bool isreppnt = linktype == FileDir.LinkType.Junction || linktype == FileDir.LinkType.Symbolic;


					/// Proc files in folder and subfolders
					if ((!isreppnt || (isreppnt && (ops & Dat.Ops.ScanLnkSubFolders) != 0)))
					{
						// Process files
						procdir = ProcDirectoryFiles(
													sdta.Dir,
													ops,
													ref counters,
													ref misc,
													sdta.Log,
													ref maxdiskusage,
													sdta.Recyclebin);

						Console.Write("\n");

						// Process directories
						if (TotalDirInDir > 0 && (ops & Dat.Ops.ScanSubFolders) != 0)
						{
							idta.Level++;
							vac.Add(TotalDirInDir);
							int i = 0;

							foreach (string subdir in Directory.EnumerateDirectories(sdta.Dir))
							{
								counters.TotalDirectories++;
								sdta.Dir = subdir;
								idta.Item = i++;
								idta.Subfoldercount = TotalDirInDir;

								// call recursive
								DeepProc(
										sdta,
										idta,
										ops,
										ref counters,
										ref misc,
										ref maxdiskusage,
										ref vac);
							}
							vac.RemoveAt(vac.Count - 1);
							idta.Level--;
						}
					}
					else
					{
						// folder is junction or symbolic but user option excludes then
						I.InitPrint(" Skiped !\n", I.ColorFmt.Warning);
					}
				}
			}
			catch (Exception ex)
			{
				// strong access failed
				failedMsg = ex.Message;
			}
			if (!string.IsNullOrEmpty(failedMsg))
			{
				// print error if strong access failed
				_i.ResetPrint();
				I.PrintTreeFolder(
								sdta,
								idta,
								ops,
								0,
								Info.ColorFmt.Error);

				I.InitPrint($" {failedMsg} \n", I.ColorFmt.Warning);
				counters.Errors++;
			}
			return deleted;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="linktype"></param>
		/// <returns></returns>
		private static Info.ColorFmt GetLinkColor(FileDir.LinkType linktype)
		{
			Info.ColorFmt folder_color = Info.ColorFmt.Normal;
			switch (linktype)
			{
				case FileDir.LinkType.Hardlink:
					folder_color = Info.ColorFmt.Hardlink;
					break;

				case FileDir.LinkType.Junction:
					folder_color = Info.ColorFmt.Junction;
					break;

				case FileDir.LinkType.Symbolic:
					folder_color = Info.ColorFmt.Symbolic;
					break;

				case FileDir.LinkType.None:
				case FileDir.LinkType.Unknown:
					break;
			}
			return folder_color;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="count_files"></param>
		/// <param name="msg"></param>
		/// <param name="dir"></param>
		/// <param name="iExt"></param>
		/// <param name="cfmt"></param>
		/// <returns></returns>
		internal static int CountAndPrint(
											bool count_files,
											string msg,
											string dir,
											I iExt,
											Info.ColorFmt cfmt)
		{
			int n = 0;
			if (count_files)
			{
				iExt.PrintAutoReset(
									() => n = FilesCount(dir),
									msg,
									Info.ColorFmt.Message);
			}
			else
			{
				iExt.PrintAutoReset(
									() => n = SubfoldersCount(dir),
									msg,
									cfmt);
			}
			return n;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="dir"></param>
		/// <param name="ops">Options assigned by the user</param>
		/// <param name="counters"></param>
		/// <param name="misc"></param>
		/// <param name="log_file"></param>
		/// <param name="maxdiskusage"></param>
		/// <param name="Recyclebin">Recycle bin directory</param>
		/// <returns></returns>
		internal static bool ProcDirectoryFiles(
												string dir,
												Dat.Ops ops,
												ref Dat.Counters counters,
												ref M misc,
												string log_file,
												ref float maxdiskusage,
												string Recyclebin)
		{
			int init_errors = counters.Errors;

			I _i = new I();

			//_i.PrintAutoReset(
			//						() => n = SubfoldersCount(dir),
			//						Dat.SP + R.Msg_RetrievingFileData,
			//						Info.ColorFmt.Message);

			// count files
			int TotalFilesInDir = 0;

			_i.PrintAutoReset(
							() => TotalFilesInDir = FilesCount(dir),
							Dat.SP + R.Msg_RetrievingFileData,
							Info.ColorFmt.Message);

			counters.TotalFilesInDir = TotalFilesInDir;
			//

			bool procdir = false;
			bool deleted = false;

			int step = Math.Min(counters.TotalFilesInDir < 100 ? 1 :
																(counters.TotalFilesInDir / 100), 107);

			if (Directory.Exists(dir))
			{
				foreach (string file_path in Directory.EnumerateFiles(dir))
				{
					procdir = true;
					counters.FilesInDirCounter++;
					counters.TotalFiles++;

					if ((counters.FilesInDirCounter % step == 0) ||
						(counters.FilesInDirCounter == counters.TotalFilesInDir))
					{
						_i.PrintStatus(
										counters,
										init_errors);
					}

					misc.KeyProc(ref _i, () => Abort());
					misc.DriveOverloadProc(ref _i, ref maxdiskusage, () => Abort());

					List<FileDir.Info> links = new List<FileDir.Info>();
					FileDir.Info info = new FileDir.Info() { path = file_path };
					//FileDir.LinkType linktype =
					FileDir.FileDirAndLinks(ref info, ref links);

					deleted = Delete(
									ops,
									ref counters,
									info,
									Recyclebin,
									log_file,
									ref links);
					if (deleted)
					{
						_i.PrintStatus(
										counters,
										counters.Errors);
					}
					foreach (FileDir.Info link in links)
					{
						if ((link.linktype == FileDir.LinkType.Junction ||
							link.linktype == FileDir.LinkType.Symbolic) && link.error != FileDir.Error.None)
						{
							counters.LinkErrors++;
						}
					}
				}
			}
			if (!procdir || counters.Errors != init_errors)
			{
				_i.PrintStatus(
								counters,
								init_errors);
			}
			return procdir;
		}

		/// <summary>
		/// Delete item function
		/// </summary>
		/// <param name="ops">Options assigned by the user</param>
		/// <param name="counters"></param>
		/// <param name="info"></param>
		/// <param name="Recyclebin">Recycle bin directory</param>
		/// <param name="log_file"></param>
		/// <param name="links"></param>
		/// <returns></returns>
		internal static bool Delete(
									Dat.Ops ops,
									ref Dat.Counters counters,
									FileDir.Info info,
									string Recyclebin,
									string log_file,
									ref List<FileDir.Info> links)
		{
			bool deleted = false;
			SHFILEOPSTRUCT fileop = new SHFILEOPSTRUCT
			{
				wFunc = FileFuncFlags.FO_DELETE,
				fFlags = (ops & Dat.Ops.PermanentDel) != 0 ?
														(FILEOP_FLAGS.FOF_FILESONLY |
														FILEOP_FLAGS.FOF_NOCONFIRMATION) :
					(FILEOP_FLAGS.FOF_FILESONLY |
					FILEOP_FLAGS.FOF_ALLOWUNDO |
					FILEOP_FLAGS.FOF_NOCONFIRMATION),
			};

			DeleteFilter(
						ops,
						ref counters,
						info,
						Recyclebin,
						ref links,
						(_info, _counters) =>
			{
				fileop.pFrom = _info.path + '\0' + '\0';

				bool query_del_awr = true;

				if ((ops & Dat.Ops.NotPromptForYesNo) == 0)
				{
					query_del_awr = false;
					Console.WriteLine($" Delete item: \"{_info.linktype}-{_info.path}\'? Press (y)es or (n)o ");
					ConsoleKeyInfo rdk = Console.ReadKey(true);
					if (rdk.Key == ConsoleKey.Y)
					{
						query_del_awr = true;
					}
				}
				if (query_del_awr)
				{
					if (SHFileOperation(ref fileop) == 0)
					{
						deleted = true;
						if (_info.isfile)
						{
							_counters.TotalDeletedFileLinks++;
						}
						else
						{
							_counters.TotalDeletedDirLinks++;
						}
						switch (_info.linktype)
						{
							case FileDir.LinkType.Hardlink:
								_counters.Hardlinks++;
								break;

							case FileDir.LinkType.Junction:
								_counters.Junctions++;
								break;

							case FileDir.LinkType.Symbolic:
								_counters.SymbLnks++;
								break;
						}
					}
				}
				return _counters;
			},
			(_info, _counters) =>
			{
				string msg = string.Empty;
				switch (_info.error)
				{
					case FileDir.Error.FileNotFound:
						msg = $"{R.Err_FileNotFound}";
						break;

					case FileDir.Error.DirectoryNotFound:
						msg = $"{R.Err_DirNotFound}";
						break;

					case FileDir.Error.PathTooLong:
						msg = $"{R.Err_PathTooLong}";
						break;

					case FileDir.Error.Security:
						msg = $"{R.Err_SecurityError}";
						break;

					case FileDir.Error.DriveNotFound:
						msg = $"{R.Err_DriveNotFound}";
						break;

					case FileDir.Error.UnauthorizedAccess:
						msg = $"{R.Err_UnauthorizedAccess}";
						break;

					case FileDir.Error.Other:
						//msg = "";
						break;
				}
				msg += $"{Dat.NL}{Dat.TB}{R.Str_Message}: {_info.error_msg}{Dat.NL}{Dat.TB}Path: {_info.path}";
				M.LogError(log_file, new string[]
				{
					msg
				});
				_counters.Errors++;
				return _counters;
			});
			return deleted;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="ops">Options assigned by the user</param>
		/// <param name="counters"></param>
		/// <param name="info"></param>
		/// <param name="Recyclebin">Recycle bin directory</param>
		/// <param name="links"></param>
		/// <param name="delete"></param>
		/// <param name="log"></param>
		internal static void DeleteFilter(
										Dat.Ops ops,
										ref Dat.Counters counters,
										FileDir.Info info,
										string Recyclebin,
										ref List<FileDir.Info> links,
										Func<FileDir.Info, Dat.Counters, Dat.Counters> delete,
										Func<FileDir.Info, Dat.Counters, Dat.Counters> log)
		{
			if (info.error != FileDir.Error.None)
			{
				counters = log.Invoke(info, counters);
				return;
			}

			if (info.linktype == FileDir.LinkType.Hardlink && ((ops & Dat.Ops.DeleteHardlinks) != 0) && links.Count > 1)
			{
				var validlinks = links.Where(link => link.error == FileDir.Error.None);
				var inRecyclebin = validlinks.Where(i => i.path.Contains(Recyclebin));

				if (validlinks.Count() - inRecyclebin.Count() > 1)
				{
					counters = delete.Invoke(info, counters);
				}
			}
			else if (info.linktype == FileDir.LinkType.Junction && ((ops & Dat.Ops.DeleteJunctions) != 0))
			{
				counters = delete.Invoke(info, counters);
			}
			else if (info.linktype == FileDir.LinkType.Symbolic && ((ops & Dat.Ops.DeleteSymbLnks) != 0))
			{
				counters = delete.Invoke(info, counters);
			}
		}

		/// <summary>
		/// The software entry point
		/// </summary>
		/// <param name="args">Command line arguments</param>
		[STAThread]
		public static void Main(string[] args)
		{
			string Recyclebin = string.Empty;

			CultureInfo current = CultureInfo.CurrentCulture;

			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
			Console.OutputEncoding = Encoding.UTF8;

			using (var sinst = new SingleGlobalInstance(null, 0))
			{
				if (sinst.Timeout)
				{
					Console.WriteLine(Dat.SP + R.Warn_InstanceAlreadyRunning);
					return;
				}

				Version vs = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

				Console.WriteLine(Dat.NN);
				Console.WriteLine(Dat.LN);
				Console.WriteLine(Dat.SP + R.App_Name + $" vs. {vs} by ({R.App_Year}) " + R.App_Author);
				Console.WriteLine(Dat.SP + R.App_License);
				Console.WriteLine(Dat.NN);

				if (args == null || args.Length == 0)
				{
					string help_msg = R.App_Help;

					help_msg = help_msg.Replace("<Opt_Str_ScanSubFolders>",
						R.Opt_Str_ScanSubFolders.ToLower(current).Substring(1));
					help_msg = help_msg.Replace("<Opt_Str_ScanLnkSubFolders>",
						R.Opt_Str_ScanLnkSubFolders.ToLower(current).Substring(1));
					help_msg = help_msg.Replace("<Opt_Str_DeleteHardlinks>",
						R.Opt_Str_DeleteHardlinks.ToLower(current).Substring(1));
					help_msg = help_msg.Replace("<Opt_Str_DeleteJunctions>",
						R.Opt_Str_DeleteJunctions.ToLower(current).Substring(1));
					help_msg = help_msg.Replace("<Opt_Str_DeleteSymbLnks>",
						R.Opt_Str_DeleteSymbLnks.ToLower(current).Substring(1));
					help_msg = help_msg.Replace("<Opt_Str_PermanentDel>",
						R.Opt_Str_PermanentDel.ToLower(current).Substring(1));
					help_msg = help_msg.Replace("<Opt_Str_ShowFullPath>",
						R.Opt_Str_ShowFullPath.ToLower(current).Substring(1));
					help_msg = help_msg.Replace("<Opt_Str_PressKeyQuit>",
						R.Opt_Str_PressKeyQuit.ToLower(current).Substring(1));
					help_msg = help_msg.Replace("<Opt_Str_NotPromptForYesNo>",
						R.Opt_Str_NotPromptForYesNo.ToLower(current).Substring(1));

					help_msg = help_msg.Replace("<Opt_ScanSubFolders>",
						R.Opt_ScanSubFolders);
					help_msg = help_msg.Replace("<Opt_ScanLnkSubFolders>",
						R.Opt_ScanLnkSubFolders);
					help_msg = help_msg.Replace("<Opt_DeleteHardlinks>",
						R.Opt_DeleteHardlinks);
					help_msg = help_msg.Replace("<Opt_DeleteJunctions>",
						R.Opt_DeleteJunctions);
					help_msg = help_msg.Replace("<Opt_DeleteSymbLnks>",
						R.Opt_DeleteSymbLnks);
					help_msg = help_msg.Replace("<Opt_PermanentDel>",
						R.Opt_PermanentDel);
					help_msg = help_msg.Replace("<Opt_ShowFullPath>",
						R.Opt_ShowFullPath);
					help_msg = help_msg.Replace("<Opt_PressKeyQuit>",
						R.Opt_PressKeyQuit);
					help_msg = help_msg.Replace("<Opt_Log>",
						R.Opt_Log);
					help_msg = help_msg.Replace("<Opt_MaxDiskUsage>",
						R.Opt_MaxDiskUsage);
					help_msg = help_msg.Replace("<Opt_NotPromptForYesNo>",
						R.Opt_NotPromptForYesNo);
					Console.WriteLine(Dat.SP + help_msg);
					return;
				}

				var lowargs = args.Select(s => s.ToLower(current)).ToArray();

				Dat.Ops ops = Dat.Ops.None;
				float maxdiskusage = 90;
				string dir = string.Empty;
				string log = string.Empty;

				TupleList<Dat.Ops, string, string> options2 = new TupleList<Dat.Ops, string, string>
				{
					{ Dat.Ops.ScanSubFolders,    R.Opt_Str_ScanSubFolders,   R.Opt_ScanSubFolders },
					{ Dat.Ops.ScanLnkSubFolders, R.Opt_Str_ScanLnkSubFolders,R.Opt_ScanLnkSubFolders },
					{ Dat.Ops.DeleteHardlinks,   R.Opt_Str_DeleteHardlinks,  R.Opt_DeleteHardlinks },
					{ Dat.Ops.DeleteJunctions,   R.Opt_Str_DeleteJunctions,  R.Opt_DeleteJunctions },
					{ Dat.Ops.DeleteSymbLnks,    R.Opt_Str_DeleteSymbLnks,   R.Opt_DeleteSymbLnks },
					{ Dat.Ops.PermanentDel,      R.Opt_Str_PermanentDel,     R.Opt_PermanentDel },
					{ Dat.Ops.ShowFullPath,      R.Opt_Str_ShowFullPath,     R.Opt_ShowFullPath },
					{ Dat.Ops.PressKeyQuit,      R.Opt_Str_PressKeyQuit,     R.Opt_PressKeyQuit },
					{ Dat.Ops.Log,               R.Opt_Str_Log,              R.Opt_Log },
					{ Dat.Ops.MaxDiskUsage,      R.Opt_Str_MaxDiskUsage,     R.Opt_MaxDiskUsage },
					{ Dat.Ops.NotPromptForYesNo, R.Opt_Str_NotPromptForYesNo, R.Opt_NotPromptForYesNo },
				};

				foreach (string arg in lowargs)
				{
					if (!arg.StartsWith("-", StringComparison.InvariantCulture))
					{
						dir = arg;
						if (TestPath(dir, Dat.NN) == null)
						{
							Abort(R.Err_InvalidFilePath);
						}
					}
					else
					{
						try
						{
							ops |= options2.Single(i => arg.Substring(0, Math.Min(3, arg.Length)).Equals(i.Item3, StringComparison.InvariantCultureIgnoreCase)).Item1;
						}
						catch (InvalidOperationException)
						{
							Abort(R.Err_InvalidOption, Dat.SP, arg);
						}
					}
				}

				if ((ops & Dat.Ops.MaxDiskUsage) != 0)
				{
					string op_str = options2.Single(i =>
											i.Item1 == Dat.Ops.MaxDiskUsage).Item3;
					string arg = lowargs.Single(i =>
									 i.IndexOf(
												 op_str,
												 StringComparison.OrdinalIgnoreCase) == 0);

					if (arg.Length != 5)
					{
						Abort(R.Err_InvalidOption, Dat.SP, "-u");
					}
					if (int.TryParse(arg.Substring(op_str.Length, 2), out int val))
					{
						maxdiskusage = val;
						if (maxdiskusage < 20)
						{
							maxdiskusage = 20;
						}
						else if (maxdiskusage > 90)
						{
							maxdiskusage = 90;
						}
					}
				}

				if ((ops & Dat.Ops.Log) != 0)
				{
					string op_str = options2.Single(i => i.Item1 == Dat.Ops.Log).Item3;
					string arg = lowargs.Single(i =>
												 i.IndexOf(
														 op_str,
														 StringComparison.OrdinalIgnoreCase) == 0);

					log = arg.Substring(op_str.Length);
					log = TestPath(log, $"{R.Str_LogFile}: ");
					if (log == null)
					{
						Abort(R.Err_InvalidOption, Dat.SP, arg);
					}
				}

				if (((ops & Dat.Ops.DeleteJunctions) != 0) ||
					((ops & Dat.Ops.DeleteSymbLnks) != 0))
				{
					ops &= ~Dat.Ops.ScanLnkSubFolders;
				}

				//Process currentProcess = Process.GetCurrentProcess();
				//currentProcess.PriorityClass = ProcessPriorityClass.Idle;

				if (!ReciclebinPath(ref Recyclebin))
				{
					Abort(R.Err_RecyclebinPathCannotBeDetermined);
				}

				Console.WriteLine($"{Dat.SP}{R.Str_Dir}: \"{dir}\"");

				if (Directory.Exists(dir))
				{
					var currdir = Environment.CurrentDirectory;
					Environment.CurrentDirectory = dir;

					Dat.Counters counters = new Dat.Counters
					{
						TotalDirectories = 1
					};
					FileInfo f = new FileInfo(dir);
					string drive = Path.
										GetPathRoot(f.FullName).
										ToLower(current);

					for (int i = 0; i < options2.Count; i++)
					{
						if (options2[i].Item1 == Dat.Ops.MaxDiskUsage)
						{
							options2[i] = new Tuple<Dat.Ops, string, string>(
																		options2[i].Item1,
																		options2[i].Item2 +
																			maxdiskusage.ToString(Info.nFmt) +
																			"%",
																		options2[i].Item3);
						}
						else if (options2[i].Item1 == Dat.Ops.Log)
						{
							options2[i] = new Tuple<Dat.Ops, string, string>(
																		options2[i].Item1,
																		options2[i].Item2 +
																			"\"" + log + "\"",
																		options2[i].Item3);
						}
						if ((ops & options2[i].Item1) != 0)
						{
							Console.WriteLine(options2.Single(j => j.Item1 == options2[i].Item1).Item2);
						}
					}

					M.LogError(log, new string[]
					{
						Dat.NL + Dat.LN + Dat.NL + R.App_Name +
							$" vs. {vs} by ({R.App_Year}) " + R.App_Author,
						"Errors:"
					}, false);

					var perfCategory = new PerformanceCounterCategory("PhysicalDisk");
					string[] instanceNames = perfCategory.GetInstanceNames();
					string driveName = string.Empty;

					foreach (string name in instanceNames)
					{
						// unsafe  ?
						// if (name.ToLower(current).IndexOf(drive.Substring(0, 2)) >= 0)
						if (name.IndexOf(drive.Substring(0, 2), StringComparison.InvariantCultureIgnoreCase) >= 0)
						{
							driveName = name;
							break;
						}
					}

					PerformanceCounter diskperCounter = new PerformanceCounter("PhysicalDisk",
																			   "% Disk Time",
																			   driveName);

					M misc = new M(ref diskperCounter);

					List<int> vac = new List<int>
					{
						1
					};

					Console.WriteLine("\n");
					Console.Out.Flush();

					Dat.StrData sdta = new Dat.StrData
					{
						Dir = dir,
						Basedir = dir,
						Log = log,
						Recyclebin = Recyclebin
					};

					Dat.IntData idta = new Dat.IntData
					{
						Level = 0,
						Item = 0,
						Subfoldercount = 1
					};

					DeepProc(
							sdta,
							idta,
							ops,
							ref counters,
							ref misc,
							ref maxdiskusage,
							ref vac);

					vac.RemoveAt(vac.Count - 1);

					diskperCounter.Dispose();

					M.LogError(log, new string[]
					{
						Dat.NL,
						$"{DateTime.Now.ToString("u", Info.dFmt)}",
						$"Directories: {counters.TotalDirectories.ToString(Info.nFmt)}, Files: " +
						$"{counters.TotalFiles.ToString(Info.nFmt)}",
						$"Deleted hardlinks: {counters.TotalDeletedFileLinks.ToString(Info.nFmt)}",
						$"Errors: {counters.Errors.ToString(Info.nFmt)}"
					}, false);

					Console.WriteLine($"\n\n Directories: {counters.TotalDirectories.ToString(Info.nFmt)}, " +
						$"Files: {counters.TotalFiles.ToString(Info.nFmt)}");
					Console.WriteLine($" Deleted directory links: {counters.TotalDeletedDirLinks.ToString(Info.nFmt)}, " +
						$"Deleted file links: {counters.TotalDeletedFileLinks.ToString(Info.nFmt)}");
					Console.WriteLine($" Target link errors: {counters.LinkErrors.ToString(Info.nFmt)}, " +
						$"Errors: {counters.Errors.ToString(Info.nFmt)}");

					if ((ops & Dat.Ops.PressKeyQuit) != 0)
					{
						Console.WriteLine("\n <Press a key to quit>");
						Console.ReadKey();
					}
					Environment.CurrentDirectory = currdir;
				}
				else
				{
					Console.WriteLine(Dat.SP + R.Err_PathNotFound);
				}
			}
		}

		/// <summary>
		/// Retrieve the recycle bin directory
		/// </summary>
		/// <param name="Recyclebin">Recycle bin directory</param>
		/// <returns></returns>
		private static bool ReciclebinPath(ref string Recyclebin)
		{
			bool ret = true;
			int watchdog = 0;
			string TempFile = string.Empty;

			SHFILEOPSTRUCT fileop = new SHFILEOPSTRUCT
			{
				wFunc = FileFuncFlags.FO_DELETE,
				fFlags = FILEOP_FLAGS.FOF_FILESONLY |
						FILEOP_FLAGS.FOF_ALLOWUNDO |
						FILEOP_FLAGS.FOF_NOCONFIRMATION,
			};

			Shell shell = new Shell();
			do
			{
				/// To obtain the Recycle Bin folder, a search is made from the files deposited inside it.
				/// If the Reycle Bin is empty, a temporary file is created and deleted. The deleted temporary file
				/// path is used to obtain the Recycle Bin folder.
				/// The Recycle Bin directory is necessary to exclude possible deleted hardlinks
				/// and thus preserve at least one in their deletion.
				Folder Recycler = shell.NameSpace(ShellSpecialFolderConstants.ssfBITBUCKET);
				if (Recycler.Items().Count > 0)
				{
					FolderItem RecyItem = Recycler.Items().Item(0);
					if (RecyItem.Path.ToLower().Contains(R.Str_RECYCLEBIN.ToLower()))
					{
						Recyclebin = R.Str_RECYCLEBIN;

						if (string.IsNullOrEmpty(TempFile))
						{
							break;
						}

						string FileName = Recycler.GetDetailsOf(RecyItem, 0);//iColumn.Name);
						string FilePath = Recycler.GetDetailsOf(RecyItem, 1) +
							Path.DirectorySeparatorChar + FileName;
						fileop.fFlags = FILEOP_FLAGS.FOF_FILESONLY |
											FILEOP_FLAGS.FOF_NOCONFIRMATION;
						if (FilePath.Equals(TempFile, StringComparison.InvariantCultureIgnoreCase))
						{
							fileop.pFrom = RecyItem.Path + '\0' + '\0';
							if (SHFileOperation(ref fileop) != 0)
							{
								Console.WriteLine(R.Warn_TemporaryFileCannotBePermanentlyDeleted);
							}
						}
						break;
					}
					else
					{
						return false;
					}
				}

				if (!string.IsNullOrEmpty(TempFile))
				{
					return false;
				}

				if (string.IsNullOrEmpty(Recyclebin))
				{
					TempFile = Path.GetTempFileName();
					fileop.pFrom = TempFile + '\0' + '\0';
					if (SHFileOperation(ref fileop) != 0)
					{
						return false;
					}
				}

				watchdog++;
			} while (watchdog < 2);

			if (watchdog >= 2)
			{
				ret = false;
			}
			return ret;
		}
	}
}