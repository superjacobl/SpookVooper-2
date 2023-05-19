using Microsoft.AspNetCore.Mvc;
using SV2.Models;
using System.Diagnostics;
using SV2.Database;
using SV2.Database.Models.Entities;
using SV2.Database.Models.Items;

namespace SV2.API
{
    public class ItemAPI : BaseAPI
    {
        private static IdManager idManager = new(1);
        public static void AddRoutes(WebApplication app)
        {
            app.MapGet   ("api/item/{itemid}", GetItem);
            app.MapGet   ("api/item/{itemid}/give", Give);
            app.MapGet   ("api/item/{itemid}/owner", GetOwner);
            app.MapGet   ("api/definition/{definitionid}/items", GetItemsFromDefinition);
        }

        private static async Task GetItemsFromDefinition(HttpContext ctx, VooperDB db, long definitionid)
        {
            IEnumerable<SVItemOwnership> definitions = DBCache.GetAll<SVItemOwnership>().Where(x => x.DefinitionId == definitionid);

            await ctx.Response.WriteAsJsonAsync(definitions);
        }

        private static async Task GetItem(HttpContext ctx, VooperDB db, long itemid)
        {
            // find Item
            SVItemOwnership? item = DBCache.GetAll<SVItemOwnership>().FirstOrDefault(x => x.Id == itemid);
            if (item is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find item with id {itemid}");
                return;
            }

            await ctx.Response.WriteAsJsonAsync(item);
        }

        private static async Task GetOwner(HttpContext ctx, VooperDB db, long itemid)
        {
            // find Item
            SVItemOwnership? item = DBCache.GetAll<SVItemOwnership>().FirstOrDefault(x => x.Id == itemid);
            if (item is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find item with id {itemid}");
                return;
            }

            await ctx.Response.WriteAsJsonAsync(item.Owner);
        }

        private static async Task Give(HttpContext ctx, VooperDB db, long itemid, string apikey, long fromid, long toid, int amount)
        {
            // find Item
            SVItemOwnership? item = DBCache.GetAll<SVItemOwnership>().FirstOrDefault(x => x.Id == itemid);
            if (item is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find item with id {itemid}");
                return;
            }

            // get Entity with the api key
            BaseEntity? entity = BaseEntity.FindByApiKey(apikey);

            if (entity is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Invalid api key!");
                return;
            }

            BaseEntity? fromentity = BaseEntity.Find(fromid);
            if (fromentity is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {fromid}!");
                return;
            }

            if (fromentity.Id != entity.Id) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"The apikey must be the same as the owner of this item's!");
                return;
            }

            // check if entity is the owner of the item
            // TODO: add checking for oauth key
            
            if (fromentity.Id != item.OwnerId) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"You do not own this item!");
                return;
            }

            // find toentity

            BaseEntity? toentity = BaseEntity.Find(toid);

            if (entity is null) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Could not find entity with svid {toid}!");
                return;
            }

            if (amount <= 0) {
                ctx.Response.StatusCode = 401;
                await ctx.Response.WriteAsync($"Amount must be greater than 0!");
                return;
            }

            ItemTrade trade = new() {
                Id = idManager.Generate(),
                Amount = amount,
                FromId = fromid,
                ToId = toid,
                Time = DateTime.UtcNow,
                DefinitionId = item.DefinitionId,
                Details = "Item Trade from API",
            };

            await ctx.Response.WriteAsync((await trade.Execute()).Info);
        }
    }
}