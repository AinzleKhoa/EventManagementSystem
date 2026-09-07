namespace WebClient.Api
{
    public static class SocialAPI
    {
        // --- REVIEWS ---
        public const string ReviewBase = "social-gateway/reviews";
        public const string ReviewByEvent = "social-gateway/reviews/event/{id}"; // {id} ở đây là eventId
        public const string ReviewRating = "social-gateway/reviews/event/{id}/rating";
        public const string ReviewUpdate = "social-gateway/reviews/{id}"; // {id} ở đây là reviewId
        public const string ReviewDelete = "social-gateway/reviews/{id}";

        // --- WISHLISTS ---
        public const string WishlistBase = "social-gateway/wishlists";
        public const string WishlistMine = "social-gateway/wishlists/my-wishlist";
        public const string WishlistRemove = "social-gateway/wishlists/{id}"; // {id} ở đây là eventId
    }
}
