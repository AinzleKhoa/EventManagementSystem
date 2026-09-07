using ChatAPI.DTOs;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace ChatAPI.Odata
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<ConversationDTO>("Conversations");
            builder.EntitySet<MessageDTO>("Messages");

            builder.EntityType<ConversationDTO>().HasKey(c => c.ConversationId);
            builder.EntityType<MessageDTO>().HasKey(m => m.MessageId);

            return builder.GetEdmModel();
        }
    }
}
