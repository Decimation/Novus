// Author: Deci | Project: Novus | Name: ReflectionOperatorHelpers.cs
// Date: 2026/03/08 @ 03:03:42

using System.Linq.Expressions;
#pragma warning disable IDE1006
namespace Novus.Utilities;

public static class ReflectionOperatorHelpers
{

	public static Expression property_to_expr<T>(PI property)
	{
		var parameter          = Expression.Parameter(typeof(T));
		var propertyExpression = Expression.Property(parameter, property);
		var lambdaExpression   = Expression.Lambda(propertyExpression, parameter);

		return lambdaExpression;
	}

	public static MMI member_of<T>(Expression<Func<T>> expression)
	{
		if (expression.Body is ConstantExpression) {
			return null;
		}

		var body = (MemberExpression) expression.Body;
		return body.Member;
	}

	public static ConstantExpression const_of<T>(Expression<Func<T>> expression)
	{
		var body = (ConstantExpression) expression.Body;
		return body;
	}

	public static MMI member_of2<T>(Expression<Func<T>> expression)
	{
		var mi = member_of(expression);

		return mi switch
		{
			FI f => f,
			PI p => p,

			_ => mi
		};
	}

	public static FI field_of<T>(Expression<Func<T>> expression)
		=> (FI) member_of(expression);

	public static PI property_of<T>(Expression<Func<T>> expression)
		=> (PI) member_of(expression);

	public static MI method_of<T>(Expression<Func<T>> expression)
	{
		var body = (MethodCallExpression) expression.Body;
		return body.Method;
	}

	public static MI method_of(Expression<Action> expression)
	{
		var body = (MethodCallExpression) expression.Body;
		return body.Method;
	}

}