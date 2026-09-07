using EventAPI.DTOs;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace EventAPI.Odata
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<ReadEventDTO>("Events");
            builder.EntitySet<ReadTicketTypeDTO>("TicketTypes");
            builder.EntitySet<ReadEventValidatorDTO>("EventValidators");

            builder.EntityType<ReadEventDTO>().HasKey(e => e.EventId);
            builder.EntityType<ReadTicketTypeDTO>().HasKey(t => t.TicketTypeId);
            builder.EntityType<ReadEventValidatorDTO>().HasKey(v => v.EventStaffId);

            return builder.GetEdmModel();
        }
    }
}
