using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Antix.Mapping.Expressions
{
    public class CollapseMembersExpressionVisitor :
        ExpressionVisitor
    {
        public MemberExpressionWrapper<T> Modify<T>(Expression<Func<T>> exp)
        {
            var memberExpression = Visit(exp.Body) as MemberExpression;
            if (memberExpression != null)
            {
                var subjectExpression = memberExpression.Expression as ConstantExpression;
                if (subjectExpression != null)
                {
                    return new MemberExpressionWrapper<T>(
                        memberExpression.Member,
                        subjectExpression.Value);
                }
            }

            throw new NotSupportedException("Expression not supported");
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Debug.WriteLine(memberExpression.Member.Name, "Member");

            var subMemberExpression = base.Visit(memberExpression.Expression) as MemberExpression;
            if (subMemberExpression != null)
            {
                var constantExpression = subMemberExpression.Expression as ConstantExpression;
                if (constantExpression != null)
                {
                    return Expression.MakeMemberAccess(
                        Expression.Constant(GetValue(subMemberExpression, constantExpression.Value)),
                        memberExpression.Member);
                }
            }

            return base.VisitMember(memberExpression);
        }

        static object GetValue(MemberExpression memberExpression, object value)
        {
            var member = value.GetType()
                .GetMember(memberExpression.Member.Name,
                           BindingFlags.Instance |
                           BindingFlags.Public | BindingFlags.NonPublic |
                           BindingFlags.GetProperty | BindingFlags.GetField)
                .Single();

            var fieldInfo = member as FieldInfo;

            return fieldInfo != null
                       ? fieldInfo.GetValue(value)
                       : ((PropertyInfo) member).GetValue(value, new object[] {});
        }
    }
}