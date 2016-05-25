using System;

namespace Solutions.Core.Locator
{
    public interface ILocatorBuilder
    {
        /// <summary> Registration of the type to be activated per call </summary>
        /// <param name="type"> Type to register </param>
        /// <param name="concrete"> Concrete type should be inherited from specified type </param>
        void Register(Type type, Type concrete);

        /// <summary> Registration of the type to be activated only once </summary>
        /// <param name="type"> Type to register </param>
        /// <param name="concrete"> Concrete type should be inherited from specified type </param>
        void RegisterSingle(Type type, Type concrete);

        /// <summary> Registration of the type with instance (singleton mode) </summary>
        /// <param name="type"> Type to register </param>
        /// <param name="concrete"> Concrete type should be inherited from specified type </param>
        /// <param name="instance"> Singleton instance, should be an instance of the concrete type </param>
        void RegisterSingle(Type type, Type concrete, Object instance);

        /// <summary> Build service locator using stored registrations </summary>
        ILocator Build();
    }

    public static class LocatorBuilderExtensions
    {
        /// <summary> Register class TType as self TType </summary>
        public static void Register<TType>(this ILocatorBuilder builder)
        {
            builder.Register<TType, TType>();
        }

        /// <summary> Register class TConcrete as TType </summary>
        public static void Register<TType, TConcrete>(this ILocatorBuilder builder) where TConcrete : TType
        {
            builder.Register(typeof(TType), typeof(TConcrete));
        }

        /// <summary> Register class TType as self TType is singleton mode </summary>
        public static void RegisterSingle<TType>(this ILocatorBuilder builder)
        {
            builder.RegisterSingle<TType, TType>();
        }

        /// <summary> Register class TConcrete as TType in singleton mode </summary>
        public static void RegisterSingle<TType, TConcrete>(this ILocatorBuilder builder) where TConcrete : TType
        {
            builder.RegisterSingle(typeof(TType), typeof(TConcrete));
        }

        /// <summary> Register the instance of TType as self TType is singleton mode </summary>
        public static void RegisterSingle<TType>(this ILocatorBuilder builder, TType instance)
        {
            builder.RegisterSingle<TType, TType>(instance);
        }

        /// <summary> Register the instance of TConcrete as TType is singleton mode </summary>
        public static void RegisterSingle<TType, TConcrete>(this ILocatorBuilder builder, TType instance) where TConcrete : TType
        {
            builder.RegisterSingle(typeof(TType), typeof(TConcrete), instance);
        }
    }
}