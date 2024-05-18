using ChatHub.DataAccessLayer.Context;
using ChatHub.EntityLayer.Concrete;
using ChatHub.Server.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatHub.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController(ChatDbContext context,
        IHubContext<ChatHub.Server.Hubs.ChatHub> hubContext) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetChats(Guid userId, Guid toUserId, CancellationToken cancellationToken)
        {
            List<Chat> chats=await context.Chats.Where(p=>p.UserId==userId && p.ToUserId==toUserId|| p.ToUserId== userId && p.UserId==toUserId).OrderBy(p=>p.Date).ToListAsync(cancellationToken);
            return Ok(chats);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageDto request, CancellationToken cancellationToken) {

            Chat chat = new()
            {
                UserId = request.UserId,
                ToUserId = request.ToUserId,
                Message = request.Message,
                Date = DateTime.Now,
            };

            await context.AddAsync(chat, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            var userConnection = ChatHub.Server.Hubs.ChatHub.Users
             .FirstOrDefault(p => p.Value == chat.ToUserId);

            if (userConnection.Key != null)
            {
                await hubContext.Clients.Client(userConnection.Key).SendAsync("Messages", chat);
            }

            return Ok();
        }



    }
}
