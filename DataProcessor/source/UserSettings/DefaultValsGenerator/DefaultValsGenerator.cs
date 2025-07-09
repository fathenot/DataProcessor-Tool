namespace DataProcessor.source.UserSettings.DefaultValsGenerator
{

    // this file contains the default value generator and aggregator interfaces and their implementations
    // may be this can add more functionality in the future aka multiply, divide, etc.
    public interface IDefaultValueGenerator<T>
    {
        T GenerateDefaultValue();
    }

    /// <summary>
    /// defines arithmetic operations for a specific types
    /// Any unsupported operation will throw NotSupportedException.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICalculator<T>
    {
        T Add(T a, T b);
        T Subtract(T a, T b);
        T Multiply(T a, T b);
        T Divide(T a, T b);
        T Modulo(T a, T b);

    }

    /// <summary>
    /// Provides a configurable implementation of ICalculator{T} using delegate functions.
    /// </summary>
    public sealed class FuncCalculator<T> : ICalculator<T>
    {
        private readonly Func<T, T, T> _adder;
        private readonly Func<T, T, T>? _subtractor;
        private readonly Func<T, T, T>? _multiplier;
        private readonly Func<T, T, T>? _divider;
        private readonly Func<T, T, T>? _modulo;

        public FuncCalculator(
            Func<T, T, T> adder,
            Func<T, T, T>? subtractor = null,
            Func<T, T, T>? multiplier = null,
            Func<T, T, T>? divider = null,
            Func<T, T, T>? modulo = null)
        {
            _adder = adder ?? throw new ArgumentNullException(nameof(adder));
            _subtractor = subtractor;
            _multiplier = multiplier;
            _divider = divider;
            _modulo = modulo;
        }

        public T Add(T a, T b)
        {
            return _adder(a, b);
        }

        public T Subtract(T a, T b)
        {
            if (_subtractor is null)
                throw new NotSupportedException("Subtraction is not supported for type " + typeof(T).Name + ".");
            return _subtractor(a, b);
        }

        public T Multiply(T a, T b)
        {
            if (_multiplier is null)
                throw new NotSupportedException("Multiplication is not supported for type " + typeof(T).Name + ".");
            return _multiplier(a, b);
        }

        public T Divide(T a, T b)
        {
            if (_divider is null)
                throw new NotSupportedException("Division is not supported for type " + typeof(T).Name + ".");
            return _divider(a, b);
        }

        public T Modulo(T a, T b)
        {
            if (_modulo is null)
                throw new NotSupportedException("Modulo is not supported for type " + typeof(T).Name + ".");
            return _modulo(a, b);
        }
    }

    public class FuncDefaultValueGenerator<T> : IDefaultValueGenerator<T>
    {
        private readonly Func<T> defaultValueFunc;
        public FuncDefaultValueGenerator(Func<T> defaultValueFunc)
        {
            this.defaultValueFunc = defaultValueFunc ?? throw new ArgumentNullException(nameof(defaultValueFunc));
        }
        public T GenerateDefaultValue()
        {
            return defaultValueFunc();
        }
    }
    public static class AggregationRegistry
    {
        private static readonly Dictionary<Type, object> DefaultProviders = new();
        private static readonly Dictionary<Type, object> Aggregators = new();

        static AggregationRegistry()
        {
            RegisterDefaults();
        }

        private static void RegisterDefaults()
        {
            // Core numeric types
            Register<int>(
                new FuncCalculator<int>((a, b) => a + b, (a, b) => a - b, (a, b) => a * b, (a, b) => a / b, (a, b) => a % b),
                new FuncDefaultValueGenerator<int>(() => 0));

            Register<long>(
                new FuncCalculator<long>((a, b) => a + b, (a, b) => a - b, (a, b) => a * b, (a, b) => a / b, (a, b) => a % b),
                new FuncDefaultValueGenerator<long>(() => 0L));

            Register<float>(
                new FuncCalculator<float>((a, b) => a + b, (a, b) => a - b, (a, b) => a * b, (a, b) => a / b),
                new FuncDefaultValueGenerator<float>(() => 0f));

            Register<double>(
                new FuncCalculator<double>((a, b) => a + b, (a, b) => a - b, (a, b) => a * b, (a, b) => a / b),
                new FuncDefaultValueGenerator<double>(() => 0d));

            Register<decimal>(
                new FuncCalculator<decimal>((a, b) => a + b, (a, b) => a - b, (a, b) => a * b, (a, b) => a / b),
                new FuncDefaultValueGenerator<decimal>(() => 0m));

            Register<bool>(
                new FuncCalculator<bool>((a, b) => a || b, (a, b) => a && !b, (a, b) => a && b, (a, b) => a && b),
                new FuncDefaultValueGenerator<bool>(() => false));

            // Nullable variants
            Register<int?>(
                new FuncCalculator<int?>(
                    (a, b) => a ?? b ?? null,
                    (a, b) => (a.HasValue && b.HasValue) ? a - b : null,
                    (a, b) => (a.HasValue && b.HasValue) ? a * b : null,
                    (a, b) => (a.HasValue && b.HasValue) ? a / b : null,
                    (a, b) => (a.HasValue && b.HasValue) ? a % b : null),
                new FuncDefaultValueGenerator<int?>(() => 0));

            Register<long?>(
                new FuncCalculator<long?>(
                    (a, b) => a ?? b ?? null,
                    (a, b) => (a.HasValue && b.HasValue) ? a - b : null,
                    (a, b) => (a.HasValue && b.HasValue) ? a * b : null,
                    (a, b) => (a.HasValue && b.HasValue) ? a / b : null,
                    (a, b) => (a.HasValue && b.HasValue) ? a % b : null),
                new FuncDefaultValueGenerator<long?>(() => 0L));

            Register<float?>(
                new FuncCalculator<float?>(
                    (a, b) => a ?? b ?? null,
                    (a, b) => (a.HasValue && b.HasValue) ? a - b : null,
                    (a, b) => (a.HasValue && b.HasValue) ? a * b : null,
                    (a, b) => (a.HasValue && b.HasValue) ? a / b : null),
                new FuncDefaultValueGenerator<float?>(() => 0f));

            Register<double?>(
                new FuncCalculator<double?>(
                    (a, b) => a ?? b ?? null,
                    (a, b) => (a.HasValue && b.HasValue) ? a - b : null,
                    (a, b) => (a.HasValue && b.HasValue) ? a * b : null,
                    (a, b) => (a.HasValue && b.HasValue) ? a / b : null),
                new FuncDefaultValueGenerator<double?>(() => 0d));

            Register<decimal?>(
                new FuncCalculator<decimal?>(
                    (a, b) => a ?? b ?? null,
                    (a, b) => (a.HasValue && b.HasValue) ? a - b : null,
                    (a, b) => (a.HasValue && b.HasValue) ? a * b : null,
                    (a, b) => (a.HasValue && b.HasValue) ? a / b : null),
                new FuncDefaultValueGenerator<decimal?>(() => 0m));

            Register<bool?>(
                new FuncCalculator<bool?>(
                    (a, b) => a ?? b ?? null,
                    (a, b) => (a.HasValue && b.HasValue) ? (a.Value && !b.Value) : null,
                    (a, b) => (a.HasValue && b.HasValue) ? (a.Value && b.Value) : null,
                    (a, b) => (a.HasValue && b.HasValue) ? (a.Value && b.Value) : null),
                new FuncDefaultValueGenerator<bool?>(() => false));
        }

        private static void Register<T>(ICalculator<T> calc, IDefaultValueGenerator<T> def)
        {
            Aggregators[typeof(T)] = calc;
            DefaultProviders[typeof(T)] = def;
        }

        public static void RegisterCalculator<T>(ICalculator<T> calculator)
        {
            Aggregators[typeof(T)] = calculator ?? throw new ArgumentNullException(nameof(calculator));
        }

        public static void RegisterDefaultValueProvider<T>(IDefaultValueGenerator<T> provider)
        {
            DefaultProviders[typeof(T)] = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public static void RegisterAggregator<T>(ICalculator<T> aggregator)
        {
            Aggregators[typeof(T)] = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
        }

        public static ICalculator<T> GetAggregator<T>()
        {
            if (Aggregators.TryGetValue(typeof(T), out var aggregator))
                return (ICalculator<T>)aggregator;
            throw new KeyNotFoundException($"No aggregator registered for type {typeof(T)}.");
        }

        public static IDefaultValueGenerator<T> GetDefaultValueProvider<T>()
        {
            if (DefaultProviders.TryGetValue(typeof(T), out var provider))
                return (IDefaultValueGenerator<T>)provider;
            throw new KeyNotFoundException($"No default value provider registered for type {typeof(T)}.");
        }
    }
}
