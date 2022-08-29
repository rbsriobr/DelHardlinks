/// <summary>
///
/// </summary>
namespace DelHardlinks
{
	using System;
	using System.Collections.Generic;

	using R = Resources;

	/// <summary>
	///
	/// </summary>
	public class I : Info
	{
		/// <summary>
		///
		/// </summary>
		private const string str_F = " \u251c\u2500";

		/// <summary>
		///
		/// </summary>
		private const string str_I = " \u2502";

		/// <summary>
		///
		/// </summary>
		private const string str_L = " \u2514\u2500";

		/// <summary>
		///
		/// </summary>
		private const string str_T = "\u252c";  // \u252c ┬  \u2510  ┐
 // \u2500 └─

 // ├─

		/// <summary>
		///
		/// </summary>
		private Dat.CursorInfo cinf = new Dat.CursorInfo
		{
			Col = 0,
			Row = 0,
			Len = 0
		};

		/// <summary>
		///
		/// </summary>
		/// <param name="str"></param>
		/// <param name="fmt"></param>
		/// <param name="pms"></param>
		public static void InitPrint(
									string str,
									ColorFmt fmt,
									params object[] pms)
		{
			Dat.CursorInfo _cinf = new Dat.CursorInfo
			{
				Col = 0,
				Row = 0,
				Len = 0
			};

			Print(
					str,
					ref _cinf,
					fmt,
					pms);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="idta"></param>
		/// <param name="vac"></param>
		public static void PrintTree(Dat.IntData idta, List<int> vac)
		{
			if (vac == null)
			{
				throw new ArgumentNullException(nameof(vac), "Null exception!");
			}
			for (int l = 1; l < vac.Count - 1; l++)
			{
				if (vac[l] > 0)
				{
					Console.Write(str_I);
				}
				else
				{
					Console.Write(Dat.SP + Dat.SP);
				}
			}

			if (idta.Level > 0)
			{
				vac[idta.Level]--;

				if (vac[idta.Level] == 0)
				{
					Console.Write(str_L);
				}
				else
				{
					Console.Write(str_F);
				}
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="sdta"></param>
		/// <param name="idta"></param>
		/// <param name="ops"></param>
		/// <param name="nSubfolders"></param>
		/// <param name="fmt"></param>
		public static void PrintTreeFolder(
											Dat.StrData sdta,
											Dat.IntData idta,
											Dat.Ops ops,
											int nSubfolders,
											Info.ColorFmt fmt = ColorFmt.Message)
		{
			Info.ColorFormat(fmt, out ConsoleColor fcolor, out ConsoleColor bcolor);
			Console.ForegroundColor = fcolor;
			Console.BackgroundColor = bcolor;
			if (nSubfolders > 0 && idta.Level > 0)
			{
				Console.Write(str_T);
			}
			int pos;
			string path = sdta.Dir + "\\   ";
			if ((ops & Dat.Ops.ShowFullPath) == 0 && (pos = sdta.Dir.LastIndexOf('\\')) >= 0)
			{
				path = sdta.Dir.Substring(pos + 1);
			}
			Console.Write(Dat.SP + path + Dat.SP + Dat.SP + Dat.SP);
			Console.ResetColor();
			Console.Out.Flush();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="str"></param>
		/// <param name="fmt"></param>
		/// <param name="pms"></param>
		public void Print(
							string str,
							ColorFmt fmt,
							params object[] pms)
		{
			Print(
					str,
					ref cinf,
					fmt,
					pms);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="func"></param>
		/// <param name="str"></param>
		/// <param name="fmt"></param>
		/// <param name="pms"></param>
		public void PrintAutoReset(
									Action func,
									string str,
									ColorFmt fmt,
									params object[] pms)
		{
			Print(str, fmt, pms);

			func?.Invoke();

			ResetPrint();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="counters"></param>
		/// <param name="initErrors"></param>
		/// <param name="normalStatus"></param>
		/// <param name="deletedStatus"></param>
		/// <param name="errorStatus"></param>
		public void PrintStatus(
							Dat.Counters counters,
							int initErrors,
							ColorFmt normalStatus = ColorFmt.NormalStatus,
							ColorFmt deletedStatus = ColorFmt.DeletedStatus,
							ColorFmt errorStatus = ColorFmt.ErrotStatus)
		{
			ColorFmt colorFmt = normalStatus;
			if (counters.Hardlinks > 0 || counters.Junctions > 0 || counters.SymbLnks > 0)
			{
				colorFmt = deletedStatus;
			}
			if ((counters.Errors - initErrors) > 0)
			{
				colorFmt = errorStatus;
			}
			Print(R.Str_Info,
					ref cinf,
					colorFmt,
					counters.Hardlinks.ToString(nFmt),
					counters.Junctions.ToString(nFmt),
					counters.SymbLnks.ToString(nFmt),
					counters.FilesInDirCounter.ToString(nFmt),
					counters.TotalFilesInDir.ToString(nFmt),
					(counters.Errors - initErrors).ToString(nFmt));
		}

		/// <summary>
		///
		/// </summary>
		public void ResetPrint()
		{
			ResetPrint(ref cinf);
		}
	}
}