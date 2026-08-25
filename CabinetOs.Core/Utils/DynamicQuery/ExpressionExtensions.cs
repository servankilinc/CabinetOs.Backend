using System.Linq.Expressions;

namespace CabinetOs.Core.Utils.DynamicQuery;

/// <summary> LINQ predicate birlestirme. </summary>
public static class ExpressionExtensions
{
    /// <summary> <paramref name="left"/> AND <paramref name="right"/>. <paramref name="right"/> null ise <paramref name="left"/> aynen doner. </summary>
    public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>>? right)
    {
        if (right == null) return left;

        var parameter = Expression.Parameter(typeof(T), "x");
        var body = Expression.AndAlso(new ParameterRebinder(parameter).Visit(left.Body), new ParameterRebinder(parameter).Visit(right.Body));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    /// <summary>Iki lambda'nin parametrelerini tek bir ortak parametreye baglar.</summary>
    private sealed class ParameterRebinder(ParameterExpression parameter) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => parameter;
    }
}
