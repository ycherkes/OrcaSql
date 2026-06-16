using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OrcaSql.Core.Engine.Records;
using OrcaSql.Core.Engine.Records.VariableLengthDataProxies;
using OrcaSql.Core.Engine.SqlTypes;
using OrcaSql.Core.MetaData;
using OrcaSql.Core.MetaData.DMVs;

namespace OrcaSql.Core.Tests.Engine.Records
{
	[TestFixture]
	public class DeferredColumnValueTests
	{
		[Test]
		public void ForceReadsAndMaterializesOnce()
		{
			var proxy = new CountingProxy(new byte[] { 1, 2, 3 });
			var sqlType = new CountingSqlType();
			var value = new DeferredColumnValue(proxy, sqlType);

			Assert.AreEqual("1,2,3", value.Force());
			Assert.AreEqual("1,2,3", value.Force());
			Assert.AreEqual(1, proxy.Reads);
			Assert.AreEqual(1, sqlType.Materializations);
		}

		[Test]
		public void ForceMaterializesOnceUnderContention()
		{
			// A deliberately slow read widens the race window so that a non-thread-safe
			// implementation would let multiple threads enter the factory and read more
			// than once. Correct ExecutionAndPublication semantics keep this at exactly one.
			var proxy = new CountingProxy(new byte[] { 1, 2, 3 }) { DelayMs = 50 };
			var sqlType = new CountingSqlType();
			var value = new DeferredColumnValue(proxy, sqlType);

			const int threadCount = 8;
			var start = new ManualResetEventSlim(false);
			var exceptions = new Exception[threadCount];
			var results = new object[threadCount];
			var threads = new Thread[threadCount];

			for (int i = 0; i < threadCount; i++)
			{
				int index = i;
				threads[i] = new Thread(() =>
				{
					try
					{
						start.Wait();
						results[index] = value.Force();
					}
					catch (Exception exception)
					{
						exceptions[index] = exception;
					}
				});
				threads[i].Start();
			}

			start.Set();
			for (int i = 0; i < threads.Length; i++)
				Assert.IsTrue(threads[i].Join(TimeSpan.FromSeconds(5)), "Worker thread {0} timed out.", i);

			Assert.IsTrue(exceptions.All(x => x == null), string.Join(Environment.NewLine, exceptions.Where(x => x != null)));
			Assert.IsTrue(results.All(x => (string)x == "1,2,3"));
			Assert.AreEqual(1, proxy.Reads);
			Assert.AreEqual(1, sqlType.Materializations);
		}

		[Test]
		public void ForceTreatsMissingBytesAsEmpty()
		{
			// GetBytes() may legitimately return null; the value must materialize from an
			// empty buffer rather than throwing.
			var proxy = new CountingProxy(null);
			var sqlType = new CountingSqlType();
			var value = new DeferredColumnValue(proxy, sqlType);

			Assert.AreEqual("", value.Force());
			Assert.AreEqual(1, sqlType.Materializations);
		}

		[Test]
		public void IsValueCreatedReflectsMaterializationState()
		{
			var value = new DeferredColumnValue(new CountingProxy(new byte[] { 1, 2, 3 }), new CountingSqlType());

			Assert.IsFalse(value.IsValueCreated);

			value.Force();

			Assert.IsTrue(value.IsValueCreated);
		}

		[Test]
		public void ForceCachesAndRethrowsFactoryException()
		{
			// A factory failure must be captured once and replayed on every subsequent call,
			// matching Lazy<T> ExecutionAndPublication semantics. The read must not be retried.
			var proxy = new ThrowingProxy();
			var value = new DeferredColumnValue(proxy, new CountingSqlType());

			var first = Assert.Throws<InvalidOperationException>(() => value.Force());
			var second = Assert.Throws<InvalidOperationException>(() => value.Force());

			Assert.AreSame(first, second);
			Assert.AreEqual(1, proxy.Reads);
			Assert.IsFalse(value.IsValueCreated);
		}

		[Test]
		public void ForceRejectsRecursiveMaterialization()
		{
			DeferredColumnValue value = null;
			var proxy = new CountingProxy(new byte[] { 1, 2, 3 });
			value = new DeferredColumnValue(proxy, new ReentrantSqlType(() => value.Force()));

			var first = Assert.Throws<InvalidOperationException>(() => value.Force());
			var second = Assert.Throws<InvalidOperationException>(() => value.Force());

			Assert.AreSame(first, second);
			Assert.AreEqual(1, proxy.Reads);
			Assert.IsFalse(value.IsValueCreated);
		}

		[Test]
		public void ConstructorRejectsNullArguments()
		{
			Assert.Throws<ArgumentNullException>(() => new DeferredColumnValue(null, new CountingSqlType()));
			Assert.Throws<ArgumentNullException>(() => new DeferredColumnValue(new CountingProxy(new byte[0]), null));
		}

		[Test]
		public void RowAccessForcesDeferredValuesButRawAccessDoesNot()
		{
			var column = new DataColumn("Payload", "varchar(max)");
			var row = new DataRow(new[] { column });
			var deferred = new DeferredColumnValue(new CountingProxy(new byte[] { 65 }), new CountingSqlType());

			row[column] = deferred;

			Assert.AreSame(deferred, row.GetRawValue(column));
			Assert.AreEqual("65", row[column]);
			Assert.AreEqual("65", row.Field<string>("Payload"));
		}

		private sealed class CountingProxy : IVariableLengthDataProxy
		{
			private readonly byte[] _bytes;
			private readonly object _lock = new object();

			public CountingProxy(byte[] bytes)
			{
				_bytes = bytes;
			}

			public int DelayMs { get; set; }

			public int Reads { get; private set; }

			public IEnumerable<byte> GetBytes()
			{
				if (DelayMs > 0)
					Thread.Sleep(DelayMs);

				lock (_lock)
					Reads++;

				return _bytes;
			}
		}

		private sealed class ThrowingProxy : IVariableLengthDataProxy
		{
			private readonly object _lock = new object();

			public int Reads { get; private set; }

			public IEnumerable<byte> GetBytes()
			{
				lock (_lock)
					Reads++;

				throw new InvalidOperationException("boom");
			}
		}

		private sealed class CountingSqlType : ISqlType
		{
			private readonly object _lock = new object();

			public int Materializations { get; private set; }

			public bool IsVariableLength => true;
			public short? FixedLength => null;

			public object GetValue(byte[] value)
			{
				lock (_lock)
					Materializations++;
				return string.Join(",", value);
			}

			public object GetDefaultValue(SysDefaultConstraint columnConstraint)
			{
				return null;
			}
		}

		private sealed class ReentrantSqlType : ISqlType
		{
			private readonly Func<object> _force;

			public ReentrantSqlType(Func<object> force)
			{
				_force = force;
			}

			public bool IsVariableLength => true;
			public short? FixedLength => null;

			public object GetValue(byte[] value)
			{
				return _force();
			}

			public object GetDefaultValue(SysDefaultConstraint columnConstraint)
			{
				return null;
			}
		}
	}
}
