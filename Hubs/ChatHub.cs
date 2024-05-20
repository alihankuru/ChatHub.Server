using ChatHub.DataAccessLayer.Context;
using ChatHub.EntityLayer.Concrete;
using Microsoft.AspNetCore.SignalR;

namespace ChatHub.Server.Hubs
{
    public sealed class ChatHub(ChatDbContext context) : Hub
    {
        public static Dictionary<string, Guid> Users = new();
        public async Task Connect(Guid userId)
        {
            Users.Add(Context.ConnectionId, userId);
            AppUser? user = await context.Users.FindAsync(userId);

            if (user is not null)
            {
                user.Status = "Çevrimiçi";
                await context.SaveChangesAsync();
            }

            await Clients.All.SendAsync("Users", user);

        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Guid userId;
            Users.TryGetValue(Context.ConnectionId, out userId);
            Users.Remove(Context.ConnectionId);

            AppUser? user = await context.Users.FindAsync(userId);

            if (user is not null)
            {
                user.Status = "Çevrimdışı";
                await context.SaveChangesAsync();
            }



        }


    }
}
