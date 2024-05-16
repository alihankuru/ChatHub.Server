namespace ChatHub.Server.Dtos
{
    public sealed record ChangePasswordDto(
        Guid Id,
        string CurrentPassword,
        string NewPassword
        );
 
}
