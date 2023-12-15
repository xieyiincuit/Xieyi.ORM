﻿using System.Linq.Expressions;

namespace Xieyi.ORM.Core.Extensions;

public static class ExpressionExtension
{
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        return Compose(left, right, Expression.AndAlso);
    }

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        return Compose(left, right, Expression.OrElse);
    }

    private static Expression<T> Compose<T>(this Expression<T> left, Expression<T> right, Func<Expression, Expression, Expression> merge)
    {
        // build parameter map (from parameters of right to parameters of left)
        var map = left.Parameters.Select((f, i) => new { f, s = right.Parameters[i] }).ToDictionary(p => p.s, p => p.f);

        // replace parameters in the right lambda expression with parameters from the left
        var rightBody = ParameterExchanger.ReplaceParameters(map, right.Body);

        // apply composition of lambda expression bodies to parameters from the left expression 
        return Expression.Lambda<T>(merge(left.Body, rightBody), left.Parameters);
    }

    internal class ParameterExchanger : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;

        private ParameterExchanger(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterExchanger(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            ParameterExpression replacement;
            if (map.TryGetValue(p, out replacement)) p = replacement;
            return base.VisitParameter(p);
        }
    }
}