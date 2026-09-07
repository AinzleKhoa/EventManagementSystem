using CategoryAPI.DTOs;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace CategoryAPI.Odata
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<ReadCategoryDTO>("Categories");
            builder.EntityType<ReadCategoryDTO>().HasKey(c => c.CategoryId);

            return builder.GetEdmModel();
        }
    }
}
