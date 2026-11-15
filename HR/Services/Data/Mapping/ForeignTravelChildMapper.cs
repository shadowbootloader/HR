using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class ForeignTravelChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.TripsAbroad == null) return;

            foreach (var trip in profile.TripsAbroad)
            {
                var depart = trip.DepartDate ?? DateTime.Today;

                entity.ForeignTravels.Add(new ForeignTravel
                {
                    ForeignTravelId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    TravelReasonId = await lookups.ByteFromTitleAsync<TravelReason>(trip.Reason),
                    DepartureDate = depart,
                    ReturnDate = trip.ReturnDate ?? depart,
                    TransportTypeId = await lookups.ByteFromTitleAsync<TransportType>(trip.Transport),
                    GeoOrigin = await lookups.CountryNodeAsync(trip.OriginCountry),
                    GeoDestination = await lookups.CountryNodeAsync(trip.DestinationCountry),
                    Description = MapperHelpers.JoinNote(trip.Companions, trip.Note)
                });
            }
        }
    }
}