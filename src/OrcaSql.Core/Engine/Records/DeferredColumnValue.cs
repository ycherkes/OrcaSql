using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using OrcaSql.Core.Engine.Records.VariableLengthDataProxies;
using OrcaSql.Core.Engine.SqlTypes;
using OrcaSql.Core.MetaData;

namespace OrcaSql.Core.Engine.Records
{
    /// <summary>
    /// A column value whose physical bytes are not read until <see cref="Force"/> is called.
    /// This is useful for streaming large off-row values when a caller only needs to inspect
    /// a subset of rows or columns. Buffering unforced rows still retains each value's physical
    /// data proxy until the row is collected.
    /// </summary>
    /// <remarks>
    /// Materialization is performed at most once and is safe to call from multiple threads
    /// concurrently. If the value factory throws, the failure is captured and re-thrown on
    /// every subsequent <see cref="Force"/> call, matching the exception-caching semantics of
    /// <see cref="Lazy{T}"/> with <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/>.
    /// </remarks>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class DeferredColumnValue : IDeferredValue
    {
        // State machine values guarding the single materialization. _state is read with
        // Volatile.Read on the fast path so that the corresponding _value / _exception write
        // (which happens-before the matching Volatile.Write) is guaranteed to be visible.
        private const int StateUninitialized = 0;
        private const int StateInitialized = 1;
        private const int StateFailed = 2;
        private const int StateInitializing = 3;

        private readonly object _sync = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Func<object> _valueFactory;

        private object _value;
        private ExceptionDispatchInfo _exception;
        private int _state;

        public DeferredColumnValue(IVariableLengthDataProxy proxy, ISqlType sqlType)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));
            if (sqlType == null) throw new ArgumentNullException(nameof(sqlType));

            _valueFactory = () =>
            {
                var data = proxy.GetBytes()?.ToArray();
                return sqlType.GetValue(data ?? new byte[0]);
            };
        }

        /// <summary>True once the value has been successfully materialized.</summary>
        public bool IsValueCreated => Volatile.Read(ref _state) == StateInitialized;

        /// <summary>Read and materialize the column value, caching the result.</summary>
        public object Force()
        {
            var state = Volatile.Read(ref _state);

            if (state == StateInitialized)
                return _value;

            if (state == StateFailed)
                return ThrowCachedException();

            lock (_sync)
            {
                state = _state;

                if (state == StateInitialized)
                    return _value;

                if (state == StateFailed)
                    return ThrowCachedException();

                if (state == StateInitializing)
                    throw new InvalidOperationException("Value factory attempted to recursively materialize this deferred value.");

                Volatile.Write(ref _state, StateInitializing);

                try
                {
                    _value = _valueFactory();
                    Volatile.Write(ref _state, StateInitialized);

                    return _value;
                }
                catch (Exception exception)
                {
                    _exception = ExceptionDispatchInfo.Capture(exception);
                    Volatile.Write(ref _state, StateFailed);

                    throw;
                }
                finally
                {
                    _valueFactory = null;
                }
            }
        }

        private object ThrowCachedException()
        {
            _exception.Throw();

            // Unreachable: ExceptionDispatchInfo.Throw always throws. Present only to satisfy
            // the compiler's definite-return analysis.
            throw new InvalidOperationException("Cached exception failed to re-throw.");
        }

        private string DebuggerDisplay
        {
            get
            {
                switch (Volatile.Read(ref _state))
                {
                    case StateUninitialized:
                        return "Not materialized";
                    case StateInitialized:
                        return $"Materialized: {(_value == null ? "null" : _value.GetType().Name)}";
                    case StateFailed:
                        return $"Failed: {_exception?.SourceException.GetType().Name}";
                    case StateInitializing:
                        return "Materializing";
                    default:
                        return "Unknown state";
                }
            }
        }
    }
}
