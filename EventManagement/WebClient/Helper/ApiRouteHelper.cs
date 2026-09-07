namespace WebClient.Helper
{
    public static class ApiRouteHelper
    {
        public static string WithId(this string route, int id)
        {
            return route.Replace("{id}", id.ToString());
        }
    }
}
