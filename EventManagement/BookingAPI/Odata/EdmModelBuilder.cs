using Microsoft.OData.ModelBuilder;
using Microsoft.OData.Edm;
using BookingAPI.DTOs;
using BookingAPI.DTOs.OrderDTO;
using BookingAPI.DTOs.TicketDTO;
namespace BookingAPI.Odata
{


    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            // EntitySet = endpoint chính
            builder.EntitySet<ReadOrderDTO>("Orders");
            builder.EntitySet<ReadTicketDTO>("Tickets");

            builder.EntityType<ReadOrderDTO>()
               .HasKey(p => p.OrderId);
            builder.EntityType<ReadTicketDTO>()
               .HasKey(p => p.TicketId);

            return builder.GetEdmModel();
        }
    }

}
