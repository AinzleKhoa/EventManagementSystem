using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using WithdrawalAPI.DTOs;

namespace WithdrawalAPI.Odata
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<ReadWithdrawalDTO>("Withdrawals");
            builder.EntityType<ReadWithdrawalDTO>().HasKey(w => w.WithdrawalId);
            return builder.GetEdmModel();
        }
    }
}
