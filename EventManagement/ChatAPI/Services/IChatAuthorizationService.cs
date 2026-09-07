namespace ChatAPI.Services
{
    public interface IChatAuthorizationService
    {
        Task<bool> IsParticipantAsync(int conversationId, int userId);
        Task<bool> IsAdminAsync(int conversationId, int userId);
        Task<bool> CanDeleteMessageAsync(int messageId, int userId);
        Task<bool> CanEditMessageAsync(int messageId, int userId);
    }
}
