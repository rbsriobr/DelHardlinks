using System;
using System.Collections.Generic;
using System.Linq;

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
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <typeparam name="T3"></typeparam>
		internal class TupleList<T1, T2, T3> : List<Tuple<T1, T2, T3>>
		{
			/// <summary>
			///
			/// </summary>
			/// <param name="item"></param>
			/// <param name="item2"></param>
			/// <param name="item3"></param>
			public void Add(T1 item, T2 item2, T3 item3)
			{
				Add(new Tuple<T1, T2, T3>(item, item2, item3));
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="item"></param>
			/// <returns></returns>
			public IEnumerable<Tuple<T1, T2, T3>> GetT1(T1 item)
			{
				return this.Where(t1 => t1.Item1.Equals(item));
			}

			/// <summary>
			///
			/// </summary>
			/// <param name="item"></param>
			/// <returns></returns>
			public IEnumerable<Tuple<T2, T3>> RemoveT1(T1 item)
			{
				return GetT1(item).Select(t => new Tuple<T2, T3>(t.Item2, t.Item3));
			}

			/// <summary>
			///
			/// </summary>
			/// <returns></returns>
			public IEnumerable<Tuple<T2, T3>> RemoveT1()
			{
				return this.Select(i => new Tuple<T2, T3>(i.Item2, i.Item3));
			}
		}
	}
}