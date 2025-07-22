using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CabBooking.Core.DTOs;
using CabBooking.Core.Interfaces;
using CabBooking.Core.Models;

namespace CabBooking.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICabTypeRepository _cabTypeRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly INotificationService _notificationService;
        private static Dictionary<Guid, Booking> _activeBookings = new Dictionary<Guid, Booking>();

        public BookingService(
            IBookingRepository bookingRepository,
            ICabTypeRepository cabTypeRepository,
            IDriverRepository driverRepository,
            INotificationService notificationService)
        {
            _bookingRepository = bookingRepository;
            _cabTypeRepository = cabTypeRepository;
            _driverRepository = driverRepository;
            _notificationService = notificationService;
        }

        public async Task<Booking> GetByIdAsync(Guid id)
        {
            if (_activeBookings.ContainsKey(id))
            {
                return _activeBookings[id];
            }
            return await _bookingRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return bookings.Concat(_activeBookings.Values);
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid userId)
        {
            var bookings = await _bookingRepository.GetUserBookingsAsync(userId);
            return bookings.Concat(_activeBookings.Values.Where(b => b.UserId == userId));
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            _activeBookings[booking.Id] = booking;
            return await _bookingRepository.CreateAsync(booking);
        }

        public async Task<Booking> UpdateAsync(Booking booking)
        {
            if (_activeBookings.ContainsKey(booking.Id))
            {
                _activeBookings[booking.Id] = booking;
            }
            return await _bookingRepository.UpdateAsync(booking);
        }

        public async Task DeleteAsync(Guid id)
        {
            _activeBookings.Remove(id);
            await _bookingRepository.DeleteAsync(id);
        }

        public async Task<bool> CancelBookingAsync(Guid bookingId, string reason)
        {
            var booking = await GetByIdAsync(bookingId);
            if (booking == null) return false;
            
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = reason;
            await UpdateAsync(booking);
            return true;
        }

        public double EstimateFareAsync(double pickupLat, double pickupLng, double dropoffLat, double dropoffLng, string cabType)
        {
            var distance = CalculateDistance(pickupLat, pickupLng, dropoffLat, dropoffLng);
            var cabTypeEntity = _cabTypeRepository.GetByIdAsync(Guid.Parse(cabType)).Result;
            return cabTypeEntity.BasePrice + (distance * cabTypeEntity.PricePerKm);
        }

        public async Task<bool> TrackBookingAsync(Guid bookingId, double latitude, double longitude)
        {
            var booking = await GetByIdAsync(bookingId);
            if (booking == null) return false;
            
            booking.CurrentLatitude = latitude;
            booking.CurrentLongitude = longitude;
            await UpdateAsync(booking);
            return true;
        }

        public async Task<IEnumerable<Booking>> GetNearbyBookingsAsync(double latitude, double longitude, double radius)
        {
            return await _bookingRepository.GetNearbyBookingsAsync(latitude, longitude, radius);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon/2) * Math.Sin(dLon/2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
            return R * c;
        }

        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
} 