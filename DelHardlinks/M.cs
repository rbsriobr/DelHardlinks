using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

/// <summary>
///
/// </summary>
namespace DelHardlinks
{
	using R = Resources;

	/// <summary>
	///
	/// </summary>
	internal class M
	{
		/// <summary>
		///
		/// </summary>
		private readonly PerformanceCounter pcter;

		/// <summary>
		///
		/// </summary>
		private readonly Stopwatch stopWatch = new Stopwatch();

		/// <summary>
		///
		/// </summary>
		private bool spacebarKeyToggle;
		/// <summary>
		///
		/// </summary>
		/// <param name="pcter"></param>
		public M(ref PerformanceCounter pcter)
		{
			this.pcter = pcter;
			spacebarKeyToggle = false;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="iExt"></param>
		/// <param name="maxdiskusage"></param>
		/// <param name="funcAbort"></param>
		public void DriveOverloadProc(
										ref I iExt,
										ref float maxdiskusage,
										Action funcAbort)
		{
			float currdiskusage = pcter.NextValue();

			stopWatch.Start();

			while (currdiskusage > maxdiskusage && maxdiskusage > 0)
			{
				Thread.Sleep(100);
				currdiskusage = pcter.NextValue();
				if (stopWatch.Elapsed.Seconds > 5)
				{
					iExt.Print(
							Dat.SP + R.Warn_DriveOverload,
							Info.ColorFmt.Warning);
					while (true)
					{
						var key = Console.ReadKey(true).Key;
						if (key == ConsoleKey.C)
						{
							iExt.ResetPrint();
							maxdiskusage = -1;
							break;
						}
						else if (key == ConsoleKey.A)
						{
							//cinf.len = 0;
							iExt.Print(
								Dat.NL + Dat.NL + Dat.SP + R.Msg_Aborted,
								Info.ColorFmt.Abort);
							Console.ResetColor();
							pcter.Dispose();
							funcAbort?.Invoke();
						}
						Thread.Sleep(200);
					}
				}
			}
			stopWatch.Reset();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="iExt"></param>
		/// <param name="funcAbort"></param>
		public void KeyProc(
							ref I iExt,
							Action funcAbort)
		{
			do
			{
				if (Console.KeyAvailable)
				{
					if (Console.ReadKey(true).Key == ConsoleKey.Spacebar)
					{
						spacebarKeyToggle = !spacebarKeyToggle;
						if (spacebarKeyToggle)
						{
							iExt.Print(
								Dat.SP + R.Msg_PressSpacebarToContinue,
								Info.ColorFmt.Message);
						}
						else
						{
							iExt.ResetPrint();
						}
					}
					else if (Console.ReadKey(true).Key == ConsoleKey.Escape)
					{
						//cinf.len = 0;
						iExt.Print(
								Dat.NL + Dat.NL + Dat.SP + R.Msg_Aborted,
								Info.ColorFmt.Abort);
						Console.ResetColor();
						pcter.Dispose();
						funcAbort?.Invoke();
					}
				}
				if (spacebarKeyToggle)
				{
					Thread.Sleep(500);
				}
			} while (spacebarKeyToggle);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="log"></param>
		/// <param name="lines"></param>
		/// <param name="addtime"></param>
		/// <param name="append"></param>
		internal static void LogError(
										string log,
										string[] lines,
										bool addtime = true,
										bool append = true)
		{
			if (addtime)
			{
				for (int i = 0; i < lines.Length; i++)
				{
					lines[i] = $"({DateTime.Now.ToString("u", Info.dFmt)}) {lines[i]}";
				}
			}
			if (!string.IsNullOrEmpty(log))
			{
				if (append)
				{
					File.AppendAllLines(log, lines);
				}
				else
				{
					File.WriteAllLines(log, lines);
				}
			}
		}
	}
}