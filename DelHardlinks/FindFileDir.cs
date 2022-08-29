using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
		/// <summary>
		///
		/// </summary>
		internal class FindFileDir : IDisposable
		{
			/// <summary>
			///
			/// </summary>
			private bool disposedValue = false;

			/// <summary>
			///
			/// </summary>
			private FindSafeHandle handle;

			/// <summary>
			///
			/// </summary>
			~FindFileDir()
			{
				Dispose(false);
			}

			/// <summary>
			///
			/// </summary>
			public List<string> Links { get; } = new List<string>();

			/// <summary>
			///
			/// </summary>
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="path"></param>
			/// <returns></returns>
			public bool FindFileName(string path)
			{
				StringBuilder str_buffer = new StringBuilder();
				DWORD str_size = 0;
				Links.Clear();

				handle = NativeMethods.FindFirstFileName(
														path,
														0,
														ref str_size,
														str_buffer);

				str_buffer = new StringBuilder((int)str_size + 1);

				handle = NativeMethods.FindFirstFileName(
														path,
														0,
														ref str_size,
														str_buffer);
				if (!handle.IsInvalid)
				{
					int err_code;
					while (true)
					{
						Links.Add(str_buffer.ToString());

						str_size = 0;

						NativeMethods.FindNextFileName(handle, ref str_size, str_buffer);

						err_code = Marshal.GetLastWin32Error();

						if (err_code == NativeMethods.ERROR_HANDLE_EOF)
						{
							break;
						}
						else if (err_code == NativeMethods.ERROR_MORE_DATA)
						{
							str_buffer = new StringBuilder((int)str_size + 1);
							NativeMethods.FindNextFileName(handle, ref str_size, str_buffer);
						}
					}
				}
				return true;
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="disposing"></param>
			protected virtual void Dispose(bool disposing)
			{
				if (!disposedValue)
				{
					//if (disposing)
					//{
					//}

					handle?.Dispose();

					disposedValue = true;
				}
			}
		}
	}
}