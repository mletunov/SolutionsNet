using System;

namespace Solutions.Core.Locator
{
    public interface IServiceLocator
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

        /// <summary> Check registration </summary>
        /// <param name="type"> Type which registration to be checked </param>
        Boolean IsRegistered(Type type);

        /// <summary> Return instance of specified type </summary>
        /// <param name="type">The type to be activated </param>
        Object Resolve(Type type);
    }

    public static class ServiceLocatorExtension
    {
        /// <summary> Регистрация класса-реализации TType для типа TType </summary>
        public static void Register<TType>(this IServiceLocator locator)
        {
            locator.Register<TType, TType>();
        }

        /// <summary> Регистрация класса-реализации TConcrete для типа TType </summary>
        public static void Register<TType, TConcrete>(this IServiceLocator locator) where TConcrete : TType
        {
            locator.Register(typeof (TType), typeof (TConcrete));
        }

        /// <summary> Регистрация класса-реализации TType для типа TType в виде синглтона </summary>
        public static void RegisterSingle<TType>(this IServiceLocator locator)
        {
            locator.RegisterSingle<TType, TType>();
        }

        /// <summary> Регистрация класса-реализации TType для типа TConcrete в виде синглтона </summary>
        public static void RegisterSingle<TType, TConcrete>(this IServiceLocator locator) where TConcrete : TType
        {
            locator.RegisterSingle(typeof (TType), typeof (TConcrete));
        }

        /// <summary> Регистрация класса-реализации TType для типа TType в виде синглтона с инициализацией </summary>
        public static void RegisterSingle<TType>(this IServiceLocator locator, TType instance)
        {
            locator.RegisterSingle<TType, TType>(instance);
        }

        /// <summary> Регистрация класса-реализации TType для типа TConcrete в виде синглтона с инициализацией </summary>
        public static void RegisterSingle<TType, TConcrete>(this IServiceLocator locator, TType instance)
            where TConcrete : TType
        {
            locator.RegisterSingle(typeof (TType), typeof (TConcrete), instance);
        }

        /// <summary> Зарегистрирована ли реализация типа </summary>
        public static Boolean IsRegistered<TType>(this IServiceLocator locator) where TType : class
        {
            return locator.IsRegistered(typeof (TType));
        }

        /// <summary> Возвращает зарегистрированную реализацию по типу. Если тип не зарегистрирован, возвращается null </summary>
        public static TType Resolve<TType>(this IServiceLocator locator) where TType : class
        {
            return (TType) locator.Resolve(typeof (TType));
        }

        /// <summary> Возвращает зарегистрированный объект по его типу. Если тип не зарегистрирован, возвращается Exception </summary>
        public static TType ResolveRequire<TType>(this IServiceLocator locator) where TType : class
        {
            var obj = locator.Resolve<TType>();
            if (obj == null)
                throw new NullReferenceException(String.Format("Тип {0} не зарегистрирован", typeof (TType).Name));

            return obj;
        }
    }
}