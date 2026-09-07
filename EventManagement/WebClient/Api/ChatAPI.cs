namespace WebClient.Api
{
    public static class ChatAPI
    {
        // Conversation endpoints
        public const string CreateConversation = "chat-gateway/conversations/create";
        public const string GetConversation = "chat-gateway/conversations/{id}";
        public const string GetUserConversations = "chat-gateway/conversations/user/{userId}";
        public const string GetEventConversations = "chat-gateway/conversations/event/{eventId}";
        public const string UpdateConversation = "chat-gateway/conversations/{id}";
        public const string DeleteConversation = "chat-gateway/conversations/{id}";

        // Participant endpoints
        public const string AddParticipant = "chat-gateway/conversations/{id}/participants";
        public const string RemoveParticipant = "chat-gateway/conversations/{id}/participants/{userId}";
        public const string GetParticipants = "chat-gateway/conversations/{id}/participants";
        public const string AssignRole = "chat-gateway/conversations/{id}/participants/role";

        // Message endpoints
        public const string SendMessage = "chat-gateway/messages/send";
        public const string GetMessages = "chat-gateway/messages/conversation/{id}";
        public const string EditMessage = "chat-gateway/messages/{id}";
        public const string DeleteMessage = "chat-gateway/messages/{id}";
        public const string GetUnreadCount = "chat-gateway/messages/unread/{conversationId}/{userId}";
        public const string MarkAsRead = "chat-gateway/messages/mark-read/{conversationId}";
    }
}

