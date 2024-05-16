namespace ChatHub.Server.Dtos
{
    public sealed record LoginDto(
        string UserNameOrEmail,
        string Password
        );
  
}
