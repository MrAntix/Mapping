using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Antix.Mapping
{
    public static class Extensions
    {
        public static void Map<TFrom, TTo>(
            this MapperContainer container,
            TFrom from,
            Expression<Func<TTo>> toExpression)
            where TTo : class
        {
            container.Map(
                from, toExpression,
                null);
        }

        public static void MapAll<TFrom, TTo>(
            this MapperContainer container,
            IEnumerable<TFrom> from,
            Expression<Func<IEnumerable<TTo>>> toExpression,
            Func<TFrom, TTo, bool> match)
            where TTo : class
        {
            container.MapAll(
                from, toExpression,
                match,
                null);
        }

        public static void MapAll<TFrom, TTo>(
            this MapperContainer container,
            IEnumerable<TFrom> from,
            Expression<Func<IEnumerable<TTo>>> toExpression)
            where TTo : class
        {
            container.MapAll(
                from, toExpression,
                null,
                null);
        }

        public static void MapAll<TFrom, TTo>(
            this MapperContainer container,
            IEnumerable<TFrom> from,
            Expression<Func<IEnumerable<TTo>>> toExpression,
            Func<TTo> createToItem)
            where TTo : class
        {
            container.MapAll(
                from, toExpression,
                null,
                createToItem);
        }
    }
}