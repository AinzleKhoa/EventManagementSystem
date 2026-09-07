using Microsoft.OData.ModelBuilder;
using Microsoft.OData.Edm;
using SocialAPI.DTOs; 

namespace SocialAPI.Odata
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            // 1. Khai báo các Endpoint chính (EntitySet)
            // Tên "Reviews" và "Wishlists" sẽ là tên Route OData (ví dụ: /odata/Reviews)
            builder.EntitySet<ReadReviewDTO>("Reviews");
            builder.EntitySet<ReadWishlistDTO>("Wishlists");

            // 2. Chỉ định Khóa chính (Primary Key) cho từng DTO
            // Lưu ý: Bạn hãy kiểm tra lại xem trong class DTO của bạn, khóa chính tên là gì 
            // (Id, ReviewId, WishlistId...) thì sửa lại cho đúng nhé.
            builder.EntityType<ReadReviewDTO>()
                .HasKey(r => r.ReviewId);

            builder.EntityType<ReadWishlistDTO>()
                .HasKey(w => w.WishlistId);

            return builder.GetEdmModel();
        }
    }
}