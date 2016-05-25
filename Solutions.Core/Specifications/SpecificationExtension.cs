using System;
using System.Linq.Expressions;

namespace Solutions.Core.Specifications
{
    /// <summary> Specification extensions to provide logical operations </summary>
    public static class SpecificationExtension
    {
        private class SimpleSpecification<T> : Specification<T>
        {
            private readonly Expression<Func<T, Boolean>> predicate;
            public SimpleSpecification(Expression<Func<T, Boolean>> predicate)
            {
                this.predicate = predicate;
            }

            public override Expression<Func<T, Boolean>> Predicate { get { return predicate; } }
        }

        public static ISpecification<T> And<T>(this ISpecification<T> spec1, ISpecification<T> spec2)
        {
            return new SimpleSpecification<T>(spec1.Predicate.And(spec2.Predicate));
        }

        public static ISpecification<T> Or<T>(this ISpecification<T> spec1, ISpecification<T> spec2)
        {
            return new SimpleSpecification<T>(spec1.Predicate.Or(spec2.Predicate));
        }

        public static ISpecification<T> Not<T>(this ISpecification<T> spec)
        {
            return new SimpleSpecification<T>(spec.Predicate.Not());
        }

        public static ISpecification<T> And<T>(this ISpecification<T> spec, Expression<Func<T, Boolean>> expression)
        {
            return new SimpleSpecification<T>(spec.Predicate.And(expression));
        }

        public static ISpecification<T> Or<T>(this ISpecification<T> spec, Expression<Func<T, Boolean>> expression)
        {
            return new SimpleSpecification<T>(spec.Predicate.Or(expression));
        }

        public static Predicate<T> ToPredicate<T>(this ISpecification<T> spec)
        {
            return arg => spec.Predicate.Compile()(arg);
        }

        public static Func<T, Boolean> ToFunc<T>(this ISpecification<T> spec)
        {
            return spec.Predicate.Compile();
        }

        public static Expression<Func<T, Boolean>> ToExpression<T>(this ISpecification<T> spec)
        {
            return spec.Predicate;
        }

        public static Specification<T> And<T>(this Specification<T> spec1, Specification<T> spec2)
        {
            return (Specification<T>)spec1.And((ISpecification<T>)spec2);
        }

        public static Specification<T> Or<T>(this Specification<T> spec1, Specification<T> spec2)
        {
            return (Specification<T>)spec1.Or((ISpecification<T>)spec2);
        }

        public static Specification<T> Not<T>(this Specification<T> spec)
        {
            return (Specification<T>)((ISpecification<T>)spec).Not();
        }

        public static Specification<T> And<T>(this Specification<T> spec, Expression<Func<T, Boolean>> expression)
        {
            return (Specification<T>)((ISpecification<T>)spec).And(expression);
        }

        public static Specification<T> Or<T>(this Specification<T> spec, Expression<Func<T, Boolean>> expression)
        {
            return (Specification<T>)((ISpecification<T>)spec).Or(expression);
        }

        public static Predicate<T> ToPredicate<T>(this Specification<T> spec)
        {
            return ((ISpecification<T>)spec).ToPredicate();
        }

        public static Func<T, Boolean> ToFunc<T>(this Specification<T> spec)
        {
            return ((ISpecification<T>)spec).ToFunc();
        }

        public static Expression<Func<T, Boolean>> ToExpression<T>(this Specification<T> spec)
        {
            return ((ISpecification<T>)spec).ToExpression();
        }
    }
}