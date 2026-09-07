namespace WebClient.Api
{
    public static class EventAPI
    {
        public const string Base = "event-gateway/events";
        public const string ById = "event-gateway/events/{id}";
        public const string ByOrganizerId = "event-gateway/events/organizer/{id}";
        
        public const string Submit = "event-gateway/events/{id}/submit";
        public const string Hide = "event-gateway/events/{id}/hide";
        public const string Resume = "event-gateway/events/{id}/resume";

        public const string ApprovedActiveCount = "event-gateway/events/count-approved-active";
        public const string TotalCount = "event-gateway/events/count-total";
        public const string TopSelling = "event-gateway/events/top-selling?count={count}";
    }

    public static class TicketTypeAPI
    {
        public const string Base = "event-gateway/tickettypes";
        public const string ById = "event-gateway/tickettypes/{id}";
        public const string ByEventId = "event-gateway/tickettypes/by-event/{eventId}";
    }
    public static class EventStaffAPI
    {
        public const string Base = "event-gateway/EventStaff";
    }
}