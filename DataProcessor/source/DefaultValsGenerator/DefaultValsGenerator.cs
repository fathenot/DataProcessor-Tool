namespace DataProcessor.source.DefaultValsGenerator
{

    // this file contains the default value generator and aggregator interfaces and their implementations
    // may be this can add more functionality in the future aka multiply, divide, etc.
    public interface IDefaultValueGenerator<T>
    {
        T GenerateDefaultValue();
    }

    public interface IAggregator<T>
    {
        T Add(T a, T b);
    }

    public class FuncAggregator<T> : IAggregator<T>
    {
        private readonly Func<T, T, T> _adder;
        public FuncAggregator(Func<T, T, T> adder) => _adder = adder;
        public T Add(T a, T b) => _adder(a, b);
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

        public static IAggregator<T> CreateAggregator<T>(Func<T, T, T> adder)
        {
            if (adder == null) throw new ArgumentNullException(nameof(adder));
            return new FuncAggregator<T>(adder);
        }

        public static void RegisterDefaultValueProvider<T>(IDefaultValueGenerator<T> provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            DefaultProviders[typeof(T)] = provider;
        }

        public static IDefaultValueGenerator<T> GetDefaultValueProvider<T>()
        {
            if (DefaultProviders.TryGetValue(typeof(T), out var provider))
            {
                return (IDefaultValueGenerator<T>)provider;
            }
            throw new KeyNotFoundException($"No default value provider registered for type {typeof(T)}.");
        }

        public static void RegisterAggregator<T>(IAggregator<T> aggregator)
        {
            if (aggregator == null) throw new ArgumentNullException(nameof(aggregator));
            Aggregators[typeof(T)] = aggregator;
        }

        public static IAggregator<T> GetAggregator<T>()
        {
            if (Aggregators.TryGetValue(typeof(T), out var aggregator))
            {
                return (IAggregator<T>)aggregator;
            }
            throw new KeyNotFoundException($"No aggregator registered for type {typeof(T)}.");
        }
    }
}
