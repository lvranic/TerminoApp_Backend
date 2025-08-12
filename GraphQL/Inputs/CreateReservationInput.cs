using System;
using HotChocolate;

namespace TerminoApp_NewBackend.GraphQL.Inputs
{
    public class CreateReservationInput
    {
        [GraphQLNonNullType]
        public string UserId { get; set; } = default!;

        [GraphQLNonNullType]
        public string ProviderId { get; set; } = default!;

        [GraphQLNonNullType]
        public string ServiceId { get; set; } = default!;

        [GraphQLNonNullType]
        public DateTime StartsAtUtc { get; set; }

        public int? DurationMinutes { get; set; }
    }
}