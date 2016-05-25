using System;
using System.Linq.Expressions;

namespace Solutions.Core.Specifications
{
    /// <summary> Specification pattern </summary>
    public interface ISpecification<T>
    {
        Boolean IsSatisfiedBy(T item);

        Expression<Func<T, Boolean>> Predicate { get; } 
    }
}