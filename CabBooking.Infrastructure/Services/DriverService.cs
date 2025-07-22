using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CabBooking.Core.DTOs;
using CabBooking.Core.Interfaces;
using CabBooking.Core.Models;

namespace CabBooking.Infrastructure.Services
{
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly INotificationService _notificationService;
        private static Dictionary<Guid, List<Guid>> _driverBookings = new Dictionary<Guid, List<Guid>>();
        private static object _lock = new object();

        public DriverService(
            IDriverRepository driverRepository,
            IBookingRepository bookingRepository,
            INotificationService notificationService)
        {
            _driverRepository = driverRepository;
            _bookingRepository = bookingRepository;
            _notificationService = notificationService;
        }

        public async Task<Driver> GetByIdAsync(Guid id)
        {
            return await _driverRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Driver>> GetAllAsync()
        {
            return await _driverRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Driver>> GetAvailableDriversAsync()
        {
            return await _driverRepository.GetAvailableDriversAsync();
        }

        public async Task<Driver> CreateAsync(Driver driver)
        {
            if (!_driverBookings.ContainsKey(driver.Id))
            {
                _driverBookings[driver.Id] = new List<Guid>();
            }
            return await _driverRepository.CreateAsync(driver);
        }

        public async Task<Driver> UpdateAsync(Driver driver)
        {
            return await _driverRepository.UpdateAsync(driver);
        }

        public async Task DeleteAsync(Guid id)
        {
            _driverBookings.Remove(id);
            await _driverRepository.DeleteAsync(id);
        }

        public async Task<bool> UpdateAvailabilityAsync(Guid driverId, bool isAvailable)
        {
            var driver = await _driverRepository.GetByIdAsync(driverId);
            if (driver == null) return false;

            driver.IsAvailable = isAvailable;
            await _driverRepository.UpdateAsync(driver);
            return true;
        }

        public async Task<bool> UpdateLocationAsync(Guid driverId, double latitude, double longitude)
        {
            var driver = await _driverRepository.GetByIdAsync(driverId);
            if (driver == null) return false;

            driver.CurrentLatitude = latitude;
            driver.CurrentLongitude = longitude;
            await _driverRepository.UpdateAsync(driver);
            return true;
        }

        public async Task<IEnumerable<Booking>> GetDriverBookingsAsync(Guid driverId)
        {
            if (!_driverBookings.ContainsKey(driverId))
            {
                return new List<Booking>();
            }

            var bookings = new List<Booking>();
            foreach (var bookingId in _driverBookings[driverId])
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking != null)
                {
                    bookings.Add(booking);
                }
            }
            return bookings;
        }

        public async Task<bool> AcceptBookingAsync(Guid driverId, Guid bookingId)
        {
            lock (_lock)
            {
                if (!_driverBookings.ContainsKey(driverId))
                {
                    _driverBookings[driverId] = new List<Guid>();
                }
                _driverBookings[driverId].Add(bookingId);
            }

            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return false;

            booking.Status = BookingStatus.Accepted;
            booking.DriverId = driverId;
            await _bookingRepository.UpdateAsync(booking);
            return true;
        }

        public async Task<bool> RejectBookingAsync(Guid driverId, Guid bookingId)
        {
            lock (_lock)
            {
                if (_driverBookings.ContainsKey(driverId))
                {
                    _driverBookings[driverId].Remove(bookingId);
                }
            }

            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return false;

            booking.Status = BookingStatus.Rejected;
            await _bookingRepository.UpdateAsync(booking);
            return true;
        }

        public async Task<bool> CompleteBookingAsync(Guid driverId, Guid bookingId)
        {
            lock (_lock)
            {
                if (_driverBookings.ContainsKey(driverId))
                {
                    _driverBookings[driverId].Remove(bookingId);
                }
            }

            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return false;

            booking.Status = BookingStatus.Completed;
            await _bookingRepository.UpdateAsync(booking);
            return true;
        }
    }
} 