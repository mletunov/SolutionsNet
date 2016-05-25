using System;
using System.Linq.Expressions;

namespace Solutions.Core.Specifications
{
    /// <summary> Base class with operators </summary>
    public class Specification<T> : ISpecification<T>
    {
        public Specification() {}
        public Specification(Func<T, Boolean> predicate)
        {
            Predicate = arg => predicate(arg);
        }
        public Specification(Expression<Func<T, Boolean>> predicate)
        {
            Predicate = predicate;
        }

        public Boolean IsSatisfiedBy(T item)
        {
            return Predicate.Compile()(item);
        }        

        public virtual Expression<Func<T, Boolean>> Predicate { get; private set; }

        public static Specification<T> operator &(Specification<T> spec1, Specification<T> spec2)
        {
            return spec1.And(spec2);
        }

        public static Specification<T> operator |(Specification<T> spec1, Specification<T> spec2)
        {
            return spec1.Or(spec2);
        }

        public static Specification<T> operator !(Specification<T> spec)
        {
            return spec.Not();
        }

        public static implicit operator Predicate<T>(Specification<T> specification)
        {
            return specification.ToPredicate();
        }

        public static implicit operator Func<T, Boolean>(Specification<T> specification)
        {
            return specification.ToFunc();
        }

        public static implicit operator Expression<Func<T, Boolean>>(Specification<T> specification)
        {
            return specification.ToExpression();
        }

        public static implicit operator Expression<Predicate<T>>(Specification<T> specification)
        {
            return obj => specification.ToPredicate()(obj);
        }
    }
}