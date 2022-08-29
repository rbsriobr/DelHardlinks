using System;
using System.Globalization;

namespace DelHardlinks
{
	//#pragma warning disable CA1707, S4070, CA1034, CA1051, CA1401, S4214, S1104

	/// <summary>
	///
	/// </summary>
	public class Info
	{
		/// <summary>
		///
		/// </summary>
		public static readonly NumberFormatInfo nFmt = CultureInfo.InvariantCulture.NumberFormat;

		/// <summary>
		///
		/// </summary>
		public static readonly DateTimeFormatInfo dFmt = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;

#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable S4487 // Unread "private" fields should be removed

		/// <summary>
		///
		/// </summary>
		private ConsoleColor fcolor;

		/// <summary>
		///
		/// </summary>
		private ConsoleColor bcolor;

#pragma warning restore S4487 // Unread "private" fields should be removed
#pragma warning restore IDE0052 // Remove unread private members

		/// <summary>
		///
		/// </summary>
		public enum ColorFmt
		{
			Abort,
			Message,
			Warning,
			Error,
			Help,
			NormalStatus,
			DeletedStatus,
			ErrotStatus,
			Normal,
			Junction,
			Symbolic,
			Hardlink
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="fmt"></param>
		public void ColorFormat(ColorFmt fmt)
		{
			ColorFormat(fmt, out fcolor, out bcolor);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="fmt"></param>
		/// <param name="fcolor"></param>
		/// <param name="bcolor"></param>
		public static void ColorFormat(
										ColorFmt fmt,
										out ConsoleColor fcolor,
										out ConsoleColor bcolor)
		{
			fcolor = Console.ForegroundColor;
			bcolor = Console.BackgroundColor;
			switch (fmt)
			{
				case ColorFmt.Abort:
					fcolor = ConsoleColor.Black;
					bcolor = ConsoleColor.Yellow;
					break;

				case ColorFmt.Error:
					fcolor = ConsoleColor.Red;
					bcolor = Console.BackgroundColor;
					break;

				case ColorFmt.Help:
					fcolor = ConsoleColor.Green;
					bcolor = Console.BackgroundColor;
					break;

				case ColorFmt.Normal:
				case ColorFmt.Message:
					fcolor = Console.ForegroundColor;
					bcolor = Console.BackgroundColor;
					break;

				case ColorFmt.NormalStatus:
					fcolor = ConsoleColor.Black;
					bcolor = ConsoleColor.Green;
					break;

				case ColorFmt.DeletedStatus:
					fcolor = ConsoleColor.Black;
					bcolor = ConsoleColor.White;
					break;

				case ColorFmt.ErrotStatus:
					fcolor = ConsoleColor.Black;
					bcolor = ConsoleColor.Red;
					break;

				case ColorFmt.Warning:
					fcolor = ConsoleColor.Yellow;
					bcolor = Console.BackgroundColor;
					break;

				case ColorFmt.Hardlink:
					fcolor = ConsoleColor.Gray;
					bcolor = Console.BackgroundColor;
					break;

				case ColorFmt.Junction:
					fcolor = ConsoleColor.Cyan;
					bcolor = Console.BackgroundColor;
					break;

				case ColorFmt.Symbolic:
					fcolor = ConsoleColor.Magenta;
					bcolor = Console.BackgroundColor;
					break;
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="str"></param>
		/// <param name="cinfo"></param>
		/// <param name="fmt"></param>
		/// <param name="pms"></param>
		public static void Print(
								string str,
								ref Dat.CursorInfo cinfo,
								ColorFmt fmt,
								params object[] pms)
		{
			if (cinfo.Len > 0)
			{
				ResetPrint(ref cinfo);
			}
			ColorFormat(fmt, out ConsoleColor fcolor, out ConsoleColor bcolor);
			cinfo.Row = Console.CursorTop;
			cinfo.Col = Console.CursorLeft;
			Console.ForegroundColor = fcolor;
			Console.BackgroundColor = bcolor;
			str = string.Format(CultureInfo.InvariantCulture, str, pms);
			Console.Write(str);
			cinfo.Len = str.Length;
			Console.ResetColor();
			Console.Out.Flush();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="cinfo"></param>
		public static void ResetPrint(ref Dat.CursorInfo cinfo)
		{
			string spc = new string(' ', cinfo.Len);
			Console.SetCursorPosition(cinfo.Col, cinfo.Row);
			Console.Write(spc);
			Console.SetCursorPosition(cinfo.Col, cinfo.Row);
			cinfo.Len = 0;
			Console.ResetColor();
			Console.Out.Flush();
		}
	}
}