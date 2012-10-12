using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Antix.Mapping
{
    public static class Extensions
    {
        public static void Map<TFrom, TTo>(
            this IMapperContext context,
            TFrom from,
            Expression<Func<TTo>> toExpression) where TTo : class
        {
            context.Container
                .Map(from, toExpression, context);
        }

        public static void MapAll<TFrom, TTo>(
            this IMapperContext context,
            IEnumerable<TFrom> from,
            Expression<Func<IEnumerable<TTo>>> toExpression,
            Func<TFrom, TTo, bool> match)
            where TTo : class
        {
            context.Container
                .MapAll(
                    from, toExpression,
                    match,
                    context);
        }

        public static void MapAll<TFrom, TTo>(
            this IMapperContext context,
            IEnumerable<TFrom> from,
            Expression<Func<IEnumerable<TTo>>> toExpression)
            where TTo : class
        {
            context.Container
                .MapAll(
                    from, toExpression,
                    null,
                    context);
        }
    }
}