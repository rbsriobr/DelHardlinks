using Microsoft.Win32.SafeHandles;

using System;

/// <summary>
///
/// </summary>
namespace DelHardlinks
{
	/// <summary>
	///
	/// </summary>
	public static partial class Program
	{
		/// <summary>
		///
		/// </summary>
		//[SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
		//[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
		internal class FindSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			/// <summary>
			///
			/// </summary>
			/// <param name="_handle"></param>
			/// <param name="ownHandle"></param>
			public FindSafeHandle(IntPtr _handle, bool ownHandle) : base(ownHandle)
			{
				this.SetHandle(_handle);
			}

			/// <summary>
			///
			/// </summary>
			private FindSafeHandle() : base(true)
			{
			}

			/// <summary>
			///
			/// </summary>
			/// <returns></returns>
			//[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
			protected override bool ReleaseHandle()
			{
				return NativeMethods.FindClose(this.handle);
			}
		}
	}
}