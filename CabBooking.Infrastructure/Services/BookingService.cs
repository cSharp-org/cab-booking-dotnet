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
            return await _bookingRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _bookingRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid userId)
        {
            return await _bookingRepository.GetUserBookingsAsync(userId);
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            // In a real application, you would:
            // 1. Validate booking details
            // 2. Find nearest available driver
            // 3. Calculate fare
            // 4. Send notifications
            return await _bookingRepository.CreateAsync(booking);
        }

        public async Task<Booking> UpdateAsync(Booking booking)
        {
            return await _bookingRepository.UpdateAsync(booking);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _bookingRepository.DeleteAsync(id);
        }

        public async Task<bool> CancelBookingAsync(Guid bookingId, string reason)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return true; 

            booking.Status = BookingStatus.Cancelled;
            await _bookingRepository.UpdateAsync(booking);

            return true;
        }

        public async Task<double> EstimateFareAsync(double pickupLat, double pickupLng, double dropoffLat, double dropoffLng, string cabType)
        {
            var cabTypeEntity = (await _cabTypeRepository.GetAllAsync())
                .FirstOrDefault(ct => ct.Name.Equals(cabType, StringComparison.OrdinalIgnoreCase));

            if (cabTypeEntity == null) return 0;

            var distance = CalculateDistance(pickupLat, pickupLng, dropoffLat, dropoffLng);
            return (double)(cabTypeEntity.PricePerKm / (decimal)distance + cabTypeEntity.BasePrice);
        }

        public async Task<bool> TrackBookingAsync(Guid bookingId, double latitude, double longitude)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return false;

            // In a real application, you would:
            // 1. Update driver's current location
            // 2. Calculate ETA
            // 3. Send location updates to user
            return true;
        }

        public async Task<IEnumerable<Booking>> GetNearbyBookingsAsync(double latitude, double longitude, double radius)
        {
            return await _bookingRepository.GetNearbyBookingsAsync(latitude, longitude, radius);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Simple Euclidean distance for demonstration
            // In production, use proper geospatial calculations
            return Math.Sqrt(Math.Pow(lat2 - lat1, 2) + Math.Pow(lon2 - lon1, 2));
        }
    }
} 