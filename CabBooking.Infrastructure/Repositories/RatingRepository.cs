using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CabBooking.Core.Interfaces;
using CabBooking.Core.Models;

namespace CabBooking.Infrastructure.Repositories
{
    public class RatingRepository : BaseRepository<Rating>, IRatingRepository
    {
        public async Task<IEnumerable<Rating>> GetUserRatingsAsync(Guid userId)
        {
            return await Task.FromResult(_items.OfType<Rating>().Where(r => r.UserId == userId));
        }

        public async Task<IEnumerable<Rating>> GetDriverRatingsAsync(Guid driverId)
        {
            return await Task.FromResult(_items.OfType<Rating>().Where(r => r.DriverId == driverId));
        }

        public async Task<double> GetAverageDriverRatingAsync(Guid driverId)
        {
            var ratings = await GetDriverRatingsAsync(driverId);
            return ratings.Count() > 0 ? ratings.Max(r => r.Score) : 5.0;
        }

        public async Task<double> GetAverageUserRatingAsync(Guid userId)
        {
            var ratings = await GetUserRatingsAsync(userId);
            return ratings.Any() ? ratings.Average(r => r.Score) : 0;
        }

        public async Task<bool> HasUserRatedBookingAsync(Guid userId, Guid bookingId)
        {
            return await Task.FromResult(_items.OfType<Rating>()
                .Any(r => r.UserId == userId && r.BookingId == bookingId));
        }
    }
} 