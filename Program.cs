using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EFCore.SkipBug
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
            await using var ctx = new TestContext(optionsBuilder.Options);
            await ctx.Database.EnsureCreatedAsync();

            await Outer_subquery_with_Skip_before_it(ctx).HandleAsync(nameof(Outer_subquery_with_Skip_before_it)); // Fails
            await Inner_subquery_with_Skip_before_it(ctx).HandleAsync(nameof(Inner_subquery_with_Skip_before_it)); // Fails
            await Outer_subquery_with_Skip_after_it(ctx).HandleAsync(nameof(Outer_subquery_with_Skip_after_it)); // Works
            await Inner_subquery_with_Skip_after_it(ctx).HandleAsync(nameof(Inner_subquery_with_Skip_after_it)); // Works
            await No_subquery_but_two_projections(ctx).HandleAsync(nameof(No_subquery_but_two_projections)); // Works
            await Single_projection(ctx).HandleAsync(nameof(Single_projection)); // Works
        }

        static async Task Outer_subquery_with_Skip_before_it(TestContext ctx)
        {
            await ctx.RootEntities
                .OrderBy(i => i.Id)
                .Select(i => new { Item = i })
                .Skip(1)
                .Select(o => new
                {
                    o.Item.Id,
                    Ids = o.Item.SubEntities.Select(p => p.CollectMe).ToList(),
                })
                .ToListAsync();
        }

        static async Task No_subquery_but_two_projections(TestContext ctx)
        {
            await ctx.RootEntities
                .OrderBy(i => i.Id)
                .Select(i => new { Item = i })
                .Skip(1)
                .Select(o => new { o.Item.Id })
                .ToListAsync();
        }

        static async Task Inner_subquery_with_Skip_before_it(TestContext ctx)
        {
            await ctx.RootEntities
                .OrderBy(i => i.Id)
                .Select(i => new
                {
                    Item = i,
                    Ids = i.SubEntities.Select(p => p.CollectMe).ToList(),
                })
                .Skip(1)
                .Select(o => new
                {
                    o.Item.Id,
                    o.Ids,
                })
                .ToListAsync();
        }

        static async Task Outer_subquery_with_Skip_after_it(TestContext ctx)
        {
            await ctx.RootEntities
                .OrderBy(i => i.Id)
                .Select(i => new { Item = i })
                .Select(o => new
                {
                    o.Item.Id,
                    Ids = o.Item.SubEntities.Select(p => p.CollectMe).ToList(),
                })
                .Skip(1)
                .ToListAsync();
        }

        static async Task Inner_subquery_with_Skip_after_it(TestContext ctx)
        {
            await ctx.RootEntities
                .OrderBy(i => i.Id)
                .Select(i => new
                {
                    Item = i,
                    Ids = i.SubEntities.Select(p => p.CollectMe).ToList(),
                })
                .Select(o => new
                {
                    o.Item.Id,
                    o.Ids,
                })
                .Skip(1)
                .ToListAsync();
        }

        static async Task Single_projection(TestContext ctx)
        {
            await ctx.RootEntities
                .OrderBy(i => i.Id)
                .Skip(1)
                .Select(i => new
                {
                    i.Id,
                    Ids = i.SubEntities.Select(p => p.CollectMe).ToList(),
                })
                .ToListAsync();
        }

        [DebuggerStepThrough]
        static async Task HandleAsync(this Task task, string name)
        {
            System.Console.Write("Testing {0}: ", name);
            try
            {
                await task;
                System.Console.WriteLine("Success!");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
            System.Console.WriteLine();
        }
    }
}
