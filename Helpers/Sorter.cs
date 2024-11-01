using Codebridge.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Codebridge.Helpers
{
    public class Sorter
    {
        public static IOrderedQueryable<Dog> OrderBy<T>(DbSet<Dog> set, Expression<Func<Dog, T>> exp, string order)
        {
            return order switch
            {
                "asc" => set.OrderBy(exp),
                "desc" => set.OrderByDescending(exp),
                _ => throw new NotSupportedException()
            };
        }
    }
}
