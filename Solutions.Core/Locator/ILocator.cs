using System;

namespace Solutions.Core.Locator
{
    public interface ILocator : IDisposable
    {
        /// <summary> Check registration </summary>
        /// <param name="type"> Type which registration to be checked </param>
        Boolean IsRegistered(Type type);

        /// <summary> Return instance of specified type </summary>
        /// <param name="type">The type to be activated </param>
        Object Resolve(Type type);

        /// <summary> Get builder that contains locator registrations </summary>
        ILocatorBuilder Builder { get; }
    }

    public static class LocatorExtensions
    {
        /// <summary> Resolve by specifying a generic type. Returns null if registration is absent </summary>
        public static TType Resolve<TType>(this ILocator locator) where TType : class
        {
            return (TType)locator.Resolve(typeof(TType));
        }

        /// <summary> Resolve by specifying a generic type. Throws exception if registration is absent </summary>
        public static TType ResolveRequire<TType>(this ILocator locator) where TType : class
        {
            var obj = locator.Resolve<TType>();
            if (obj == null)
                throw new NullReferenceException(String.Format("Type {0} is not registered", typeof(TType).Name));

            return obj;
        }

        /// <summary> Build new locator based on existed one with builder modification </summary>
        public static ILocator Build(this ILocator locator, Action<ILocatorBuilder> action)
        {
            var builder = locator.Builder;
            action(builder);
            return builder.Build();
        }
    }
}