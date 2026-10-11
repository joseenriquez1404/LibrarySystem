namespace LibrarySystem.Dtos
{
    public record AuthResponse(string AccessToken, DateTime ExpiresAt, UserDto User);
}
