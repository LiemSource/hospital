using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace VaeDbContext
{
    public class ParameterReplaceVisitor : ExpressionVisitor
    {
        public ParameterExpression Target { get; set; }
        public ParameterExpression Replacement { get; set; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == Target ? Replacement : base.VisitParameter(node);
        }
    }

    public static class ExpressionPredicate
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var invokedExpr = Expression.Invoke(right, left.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.And(left.Body, invokedExpr), left.Parameters);
        }

    }
    public static class ContextExtension
    {
        public static int UpdateWithSeletor<T>(this DbContext context, T entity, params Expression<Func<T, object>>[] seletor) where T : class
        {
            int result = 0;
            context.Set<T>().Attach(entity);
            if (seletor.Any())
            {
                foreach (var property in seletor)
                {
                    context.Entry(entity).Property(property).IsModified = true;
                }
            }
            return result;
        }
    }
}
