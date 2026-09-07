using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using VoucherAPI.DTOs;
using VoucherAPI.Models;

namespace VoucherAPI.Odata
{
    public static class ODataModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<Voucher>("Vouchers");
            builder.EntitySet<UserVoucherReadDTO>("UserVouchers");

            return builder.GetEdmModel();
        }
    }
}
