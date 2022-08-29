using System;
using System.Collections.Generic;

/// <summary>
///
/// </summary>
namespace DelHardlinks
{
	/// <summary>
	///
	/// </summary>
	public static class Dat
	{
		/// <summary>
		/// Tab string
		/// </summary>
		public const string TB = "\t";

		/// <summary>
		/// Line string
		/// </summary>
		public static readonly string LN = "________________________________________________________________________________";

		/// <summary>
		/// New line string
		/// </summary>
		public static readonly string NL = Environment.NewLine;

		/// <summary>
		/// Empty string
		/// </summary>
		public static readonly string NN = string.Empty;

		/// <summary>
		/// Space string
		/// </summary>
		public static readonly string SP = " ";

		/// <summary>
		/// Operations from user via command line
		/// </summary>
		[Flags]
		public enum Ops
		{
			None = 0,
			ScanSubFolders = 1,
			ScanLnkSubFolders = 2,
			DeleteHardlinks = 4,
			DeleteJunctions = 8,
			DeleteSymbLnks = 16,
			//DeleteEmptyDir = 32,
			PermanentDel = 64,
			ShowFullPath = 128,
			Log = 256,
			MaxDiskUsage = 512,
			PressKeyQuit = 1024,
			NotPromptForYesNo = 2048,
		}

		/// <summary>
		///
		/// </summary>
		public struct Counters : IEquatable<Counters>
		{
			public int Errors { get; set; }
			public int FilesInDirCounter { get; set; }
			public int Hardlinks { get; set; }
			public int Junctions { get; set; }

			//public int DirInDirCounter { get; set; }
			public int LinkErrors { get; set; }

			public int SymbLnks { get; set; }
			public int TotalDeletedDirLinks { get; set; }
			public int TotalDeletedFileLinks { get; set; }
			public int TotalDirectories { get; set; }

			//public int TotalDirInDir { get; set; }
			public int TotalFiles { get; set; }

			public int TotalFilesInDir { get; set; }

			/// <summary>
			///
			/// </summary>
			/// <param name="left"></param>
			/// <param name="right"></param>
			/// <returns></returns>
			public static bool operator !=(Counters left, Counters right)
			{
				return !(left == right);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="left"></param>
			/// <param name="right"></param>
			/// <returns></returns>
			public static bool operator ==(Counters left, Counters right)
			{
				return left.Equals(right);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public override bool Equals(object obj)
			{
				return obj is Counters counters &&
					   Hardlinks == counters.Hardlinks &&
					   Junctions == counters.Junctions &&
					   SymbLnks == counters.SymbLnks &&
					   FilesInDirCounter == counters.FilesInDirCounter &&
					   TotalFilesInDir == counters.TotalFilesInDir &&
					   TotalDeletedFileLinks == counters.TotalDeletedFileLinks &&
					   TotalDeletedDirLinks == counters.TotalDeletedDirLinks &&
					   TotalFiles == counters.TotalFiles &&
					   TotalDirectories == counters.TotalDirectories &&
					   LinkErrors == counters.LinkErrors &&
					   Errors == counters.Errors;
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public bool Equals(Counters other)
			{
				return Equals(other as object);
			}

			/// <summary>
			///
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode()
			{
				int hashCode = 449613567;
				hashCode = (hashCode * -1521134295) + Hardlinks.GetHashCode();
				hashCode = (hashCode * -1521134295) + Junctions.GetHashCode();
				hashCode = (hashCode * -1521134295) + SymbLnks.GetHashCode();
				hashCode = (hashCode * -1521134295) + FilesInDirCounter.GetHashCode();
				hashCode = (hashCode * -1521134295) + TotalFilesInDir.GetHashCode();
				hashCode = (hashCode * -1521134295) + TotalDeletedFileLinks.GetHashCode();
				hashCode = (hashCode * -1521134295) + TotalDeletedDirLinks.GetHashCode();
				hashCode = (hashCode * -1521134295) + TotalFiles.GetHashCode();
				hashCode = (hashCode * -1521134295) + TotalDirectories.GetHashCode();
				hashCode = (hashCode * -1521134295) + LinkErrors.GetHashCode();
				hashCode = (hashCode * -1521134295) + Errors.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		///
		/// </summary>
		public struct CursorInfo : IEquatable<CursorInfo>
		{
			public int Col { get; set; }
			public int Len { get; set; }
			public int Row { get; set; }

			/// <summary>
			///
			/// </summary>
			/// <param name="left"></param>
			/// <param name="right"></param>
			/// <returns></returns>
			public static bool operator !=(CursorInfo left, CursorInfo right)
			{
				return !(left == right);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="left"></param>
			/// <param name="right"></param>
			/// <returns></returns>
			public static bool operator ==(CursorInfo left, CursorInfo right)
			{
				return left.Equals(right);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public override bool Equals(object obj)
			{
				return obj is CursorInfo info &&
					   Len == info.Len &&
					   Row == info.Row &&
					   Col == info.Col;
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public bool Equals(CursorInfo other)
			{
				return Equals(other as object);
			}

			/// <summary>
			///
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode()
			{
				int hashCode = -541108272;
				hashCode = (hashCode * -1521134295) + Len.GetHashCode();
				hashCode = (hashCode * -1521134295) + Row.GetHashCode();
				hashCode = (hashCode * -1521134295) + Col.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		///
		/// </summary>
		public struct IntData : IEquatable<IntData>
		{
			/// <summary>
			///
			/// </summary>
			public int Item { get; set; }

			/// <summary>
			///
			/// </summary>
			public int Level { get; set; }

			/// <summary>
			///
			/// </summary>
			public int Subfoldercount { get; set; }

			/// <summary>
			///
			/// </summary>
			/// <param name="left"></param>
			/// <param name="right"></param>
			/// <returns></returns>
			public static bool operator !=(IntData left, IntData right)
			{
				return !(left == right);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="left"></param>
			/// <param name="right"></param>
			/// <returns></returns>
			public static bool operator ==(IntData left, IntData right)
			{
				return left.Equals(right);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public override bool Equals(object obj)
			{
				return obj is IntData data &&
					   Level == data.Level &&
					   Item == data.Item &&
					   Subfoldercount == data.Subfoldercount;
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public bool Equals(IntData other)
			{
				return Equals(other as object);
			}

			/// <summary>
			///
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode()
			{
				int hashCode = -637221617;
				hashCode = (hashCode * -1521134295) + Level.GetHashCode();
				hashCode = (hashCode * -1521134295) + Item.GetHashCode();
				hashCode = (hashCode * -1521134295) + Subfoldercount.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		///
		/// </summary>
		public struct StrData : IEquatable<StrData>
		{
			/// <summary>
			///
			/// </summary>
			public string Basedir { get; set; }

			/// <summary>
			///
			/// </summary>
			public string Dir { get; set; }

			/// <summary>
			///
			/// </summary>
			public string Log { get; set; }

			/// <summary>
			///
			/// </summary>
			public string Recyclebin { get; set; }

			/// <summary>
			///
			/// </summary>
			/// <param name="left"></param>
			/// <param name="right"></param>
			/// <returns></returns>
			public static bool operator !=(StrData left, StrData right)
			{
				return !(left == right);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="left"></param>
			/// <param name="right"></param>
			/// <returns></returns>
			public static bool operator ==(StrData left, StrData right)
			{
				return left.Equals(right);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public override bool Equals(object obj)
			{
				return obj is StrData data &&
					   Dir == data.Dir &&
					   Basedir == data.Basedir &&
					   Log == data.Log &&
					   Recyclebin == data.Recyclebin;
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public bool Equals(StrData other)
			{
				return Equals(other as object);
			}

			/// <summary>
			///
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode()
			{
				int hashCode = -1914124153;
				hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Dir);
				hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Basedir);
				hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Log);
				hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Recyclebin);
				return hashCode;
			}
		}
	}
}