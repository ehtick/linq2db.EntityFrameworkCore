﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LinqToDB.Reflection;
using LinqToDB.Tools;
using LinqToDB.Tools.Comparers;
using NUnit.Framework;

namespace LinqToDB.EntityFrameworkCore.BaseTests
{
	public class TestsBase
	{
		protected void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> result, bool allowEmpty = false)
		{
			AreEqual(t => t, expected, result, EqualityComparer<T>.Default, allowEmpty);
		}

		protected void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> result, Func<IEnumerable<T>, IEnumerable<T>> sort)
		{
			AreEqual(t => t, expected, result, EqualityComparer<T>.Default, sort);
		}

		protected void AreEqualWithComparer<T>(IEnumerable<T> expected, IEnumerable<T> result)
		{
			AreEqual(t => t, expected, result, ComparerBuilder.GetEqualityComparer<T>());
		}

		protected void AreEqualWithComparer<T>(IEnumerable<T> expected, IEnumerable<T> result, Func<MemberAccessor, bool> memberPredicate)
		{
			AreEqual(t => t, expected, result, ComparerBuilder.GetEqualityComparer<T>(memberPredicate));
		}

		protected void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> result, IEqualityComparer<T> comparer)
		{
			AreEqual(t => t, expected, result, comparer);
		}

		protected void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> result, IEqualityComparer<T> comparer, Func<IEnumerable<T>, IEnumerable<T>> sort)
		{
			AreEqual(t => t, expected, result, comparer, sort);
		}

		protected void AreEqual<T>(Func<T, T> fixSelector, IEnumerable<T> expected, IEnumerable<T> result)
		{
			AreEqual(fixSelector, expected, result, EqualityComparer<T>.Default);
		}

		protected void AreEqual<T>(Func<T, T> fixSelector, IEnumerable<T> expected, IEnumerable<T> result, IEqualityComparer<T> comparer, bool allowEmpty = false)
		{
			AreEqual<T>(fixSelector, expected, result, comparer, null, allowEmpty);
		}

		protected void AreEqual<T>(
			Func<T, T> fixSelector,
			IEnumerable<T> expected,
			IEnumerable<T> result,
			IEqualityComparer<T> comparer,
			Func<IEnumerable<T>, IEnumerable<T>>? sort,
			bool allowEmpty = false)
		{
			var resultList = result.Select(fixSelector).ToList();
			var expectedList = expected.Select(fixSelector).ToList();

			if (sort != null)
			{
				resultList = sort(resultList).ToList();
				expectedList = sort(expectedList).ToList();
			}

			if (!allowEmpty)
				Assert.AreNotEqual(0, expectedList.Count, "Expected list cannot be empty.");
			Assert.AreEqual(expectedList.Count, resultList.Count, "Expected and result lists are different. Length: ");

			var exceptExpectedList = resultList.Except(expectedList, comparer).ToList();
			var exceptResultList = expectedList.Except(resultList, comparer).ToList();

			var exceptExpected = exceptExpectedList.Count;
			var exceptResult = exceptResultList.Count;
			var message = new StringBuilder();

			if (exceptResult != 0 || exceptExpected != 0)
			{
				Debug.WriteLine(resultList.ToDiagnosticString());
				Debug.WriteLine(expectedList.ToDiagnosticString());

				for (var i = 0; i < resultList.Count; i++)
				{
					Debug.WriteLine("{0} {1} --- {2}", comparer.Equals(expectedList[i], resultList[i]) ? " " : "-", expectedList[i], resultList[i]);
					message.AppendFormat("{0} {1} --- {2}", comparer.Equals(expectedList[i], resultList[i]) ? " " : "-", expectedList[i], resultList[i]);
					message.AppendLine();
				}
			}

			Assert.AreEqual(0, exceptExpected, $"Expected Was{Environment.NewLine}{message}");
			Assert.AreEqual(0, exceptResult, $"Expect Result{Environment.NewLine}{message}");
		}

		protected void AreEqual<T>(IEnumerable<IEnumerable<T>> expected, IEnumerable<IEnumerable<T>> result)
		{
			var resultList = result.ToList();
			var expectedList = expected.ToList();

			Assert.AreNotEqual(0, expectedList.Count);
			Assert.AreEqual(expectedList.Count, resultList.Count, "Expected and result lists are different. Length: ");

			for (var i = 0; i < resultList.Count; i++)
			{
				var elist = expectedList[i].ToList();
				var rlist = resultList[i].ToList();

				if (elist.Count > 0 || rlist.Count > 0)
					AreEqual(elist, rlist);
			}
		}

		protected void AreSame<T>(IEnumerable<T> expected, IEnumerable<T> result)
		{
			var resultList = result.ToList();
			var expectedList = expected.ToList();

			Assert.AreNotEqual(0, expectedList.Count);
			Assert.AreEqual(expectedList.Count, resultList.Count);

			var b = expectedList.SequenceEqual(resultList);

			if (!b)
				for (var i = 0; i < resultList.Count; i++)
					Debug.WriteLine("{0} {1} --- {2}", Equals(expectedList[i], resultList[i]) ? " " : "-", expectedList[i], resultList[i]);

			Assert.IsTrue(b);
		}

	}
}
