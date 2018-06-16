﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Aggregates;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Queries;
using NServiceBus;

namespace eShop.Identity.User
{
    public class Handler :
        IHandleQueries<Queries.Users>,
        IHandleMessages<Events.Registered>,
        IHandleMessages<Events.Disabled>,
        IHandleMessages<Events.Enabled>,
        IHandleMessages<Entities.Role.Events.Assigned>,
        IHandleMessages<Entities.Role.Events.Revoked>
    {
        public async Task Handle(Queries.Users query, IMessageHandlerContext ctx)
        {
            var builder = new QueryBuilder();
            var results = await ctx.UoW().Query<Models.User>(builder.Build())
                .ConfigureAwait(false);

            await ctx.Result(results.Records, results.Total, results.ElapsedMs).ConfigureAwait(false);
        }

        public Task Handle(Events.Registered e, IMessageHandlerContext ctx)
        {
            var model = new Models.User
            {
                Id = e.UserName,
                GivenName = e.GivenName
            };

            return ctx.UoW().Add(e.UserName, model);
        }

        public async Task Handle(Events.Disabled e, IMessageHandlerContext ctx)
        {
            var user = await ctx.UoW().Get<Models.User>(e.UserName).ConfigureAwait(false);

            user.Disabled = true;

            await ctx.UoW().Update(e.UserName, user).ConfigureAwait(false);
        }
        public async Task Handle(Events.Enabled e, IMessageHandlerContext ctx)
        {
            var user = await ctx.UoW().Get<Models.User>(e.UserName).ConfigureAwait(false);

            user.Disabled = false;

            await ctx.UoW().Update(e.UserName, user).ConfigureAwait(false);
        }
        public async Task Handle(Entities.Role.Events.Assigned e, IMessageHandlerContext ctx)
        {
            var user = await ctx.UoW().Get<Models.User>(e.UserName).ConfigureAwait(false);
            var role = await ctx.UoW().Get<Role.Models.RoleIndex>(e.RoleId).ConfigureAwait(false);
            user.Roles = user.Roles.TryAdd(role.Name);
            role.Users++;

            await ctx.UoW().Update(user.Id, user).ConfigureAwait(false);
            await ctx.UoW().Update(role.Id, role).ConfigureAwait(false);
        }
        public async Task Handle(Entities.Role.Events.Revoked e, IMessageHandlerContext ctx)
        {
            var user = await ctx.UoW().Get<Models.User>(e.UserName).ConfigureAwait(false);
            var role = await ctx.UoW().Get<Role.Models.RoleIndex>(e.RoleId).ConfigureAwait(false);
            user.Roles = user.Roles.TryRemove(role.Name);
            role.Users--;

            await ctx.UoW().Update(user.Id, user).ConfigureAwait(false);
            await ctx.UoW().Update(role.Id, role).ConfigureAwait(false);
        }
    }
}
