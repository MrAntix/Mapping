using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Antix.Mapping
{
    public static class Extensions
    {
        public static void MapAll<TFrom, TTo>(
            this IMapperContainer container,
            IEnumerable<TFrom> from,
            Expression<Func<IEnumerable<TTo>>> toExpression,
            Func<TFrom, TTo, bool> match)
            where TTo : class
        {
            container.MapAll(
                from, toExpression,
                match);
        }

        public static void MapAll<TFrom, TTo>(
            this IMapperContainer container,
            IEnumerable<TFrom> from,
            Expression<Func<IEnumerable<TTo>>> toExpression)
            where TTo : class
        {
            container.MapAll(
                from, toExpression,
                null);
        }
    }
}