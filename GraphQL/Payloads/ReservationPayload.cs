using TerminoApp_NewBackend.Models;

namespace TerminoApp_NewBackend.GraphQL.Payloads
{
    public class ReservationPayload
    {
        public Reservation Reservation { get; }

        public ReservationPayload(Reservation reservation)
        {
            Reservation = reservation;
        }
    }
}