using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Solutions.Core.Specifications
{
    /// <summary> Expression extensions to provide logical operations </summary>
    public static class ExpressionExtension
    {
        /// <summary> Visitor for lambda parameter substitution </summary>
        private class ParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression newParam;
            private readonly IList<ParameterExpression> oldParams;
            public ParameterVisitor(ParameterExpression newParam, params ParameterExpression[] oldParams)
            {
                this.newParam = newParam;
                this.oldParams = oldParams;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return oldParams.Contains(node) ? newParam : node;
            }
        }

        /// <summary> Logical OR </summary>
        public static Expression<Func<Boolean>> Or(this Expression<Func<Boolean>> first, Expression<Func<Boolean>> second)
        {
            return Expression.Lambda<Func<Boolean>>(Expression.OrElse(first.Body, second.Body));
        }
        public static Expression<Func<T, Boolean>> Or<T>(this Expression<Func<T, Boolean>> first, Expression<Func<T, Boolean>> second)
        {
            return Expression.OrElse(first.Body, second.Body).Create<T>(first.Parameters[0], second.Parameters[0]);
        }

        /// <summary> Logical AND </summary>
        public static Expression<Func<Boolean>> And(this Expression<Func<Boolean>> first, Expression<Func<Boolean>> second)
        {
            return Expression.Lambda<Func<Boolean>>(Expression.AndAlso(first.Body, second.Body));
        }
        public static Expression<Func<T, Boolean>> And<T>(this Expression<Func<T, Boolean>> first, Expression<Func<T, Boolean>> second)
        {
            return Expression.AndAlso(first.Body, second.Body).Create<T>(first.Parameters[0], second.Parameters[0]);
        }

        /// <summary> Logical NOT </summary>
        public static Expression<Func<Boolean>> Not(this Expression<Func<Boolean>> first)
        {
            return Expression.Lambda<Func<Boolean>>(Expression.Not(first.Body));
        }
        public static Expression<Func<T, Boolean>> Not<T>(this Expression<Func<T, Boolean>> first)
        {
            return Expression.Not(first.Body).Create<T>(first.Parameters[0]);
        }

        /// <summary> Expression building </summary>
        private static Expression<Func<T, Boolean>> Create<T>(this Expression body, params ParameterExpression[] oldParams)
        {
            var parameter = Expression.Parameter(typeof (T));
            var expression = new ParameterVisitor(parameter, oldParams).Visit(body);

            return Expression.Lambda<Func<T, Boolean>>(expression, parameter);
        }
    }
}