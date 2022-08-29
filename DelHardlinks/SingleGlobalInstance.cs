using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

using System.Threading;

/// <summary>
///
/// </summary>
namespace DelHardlinks
{
	/// <summary>
	///
	/// </summary>
	internal class SingleGlobalInstance : IDisposable
	{
		/// <summary>
		///
		/// </summary>
		private readonly bool _hasHandle = false;

		/// <summary>
		///
		/// </summary>
		private readonly Mutex _mutex;

		/// <summary>
		///
		/// </summary>
		private bool disposed;

		/// <summary>
		///
		/// </summary>
		/// <param name="appGuid"></param>
		/// <param name="timeOut"></param>
		public SingleGlobalInstance(
									string appGuid,
									int timeOut
			//, Action timeout_func
			)
		{
			if (string.IsNullOrWhiteSpace(appGuid))
			{
				appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;
			}

			var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);

			// .Net framework -> .Net Core 3.1 conversion
			//var securitySettings = new MutexSecurity();
			//securitySettings.AddAccessRule(allowEveryoneRule);

			_mutex = new Mutex(false, @"Global\" + appGuid, out _);//, securitySettings);

			try
			{
				_hasHandle = _mutex.WaitOne(timeOut, false);

				if (!_hasHandle)
				{
					Timeout = true;
					//if (timeout_func != null)
					//{
					//	timeout_func.Invoke();
					//}
					//else
					//{
					//	throw new TimeoutException("Timeout waiting for exclusive access on SingleInstance");
					//}
				}
			}
			catch (AbandonedMutexException)
			{
				_hasHandle = true;
			}
		}

		/// <summary>
		///
		/// </summary>
		public bool Timeout { get; } = false;

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
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing && _mutex != null)
				{
					if (_hasHandle)
						_mutex.ReleaseMutex();
					_mutex.Close();
					_mutex.Dispose();
				}

				// unmanaged resources here.

				disposed = true;
			}
		}
	}
}