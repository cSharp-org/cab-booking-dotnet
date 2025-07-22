using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CabBooking.Core.DTOs;
using CabBooking.Core.Interfaces;
using CabBooking.Core.Models;

namespace CabBooking.Infrastructure.Services
{
    public class CabService : ICabService
    {
        private readonly ICabRepository _cabRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly INotificationService _notificationService;
        private static List<string> _searchHistory = new List<string>();

        public CabService(
            ICabRepository cabRepository,
            IDriverRepository driverRepository,
            INotificationService notificationService)
        {
            _cabRepository = cabRepository;
            _driverRepository = driverRepository;
            _notificationService = notificationService;
        }

        public async Task<Cab> GetByIdAsync(Guid id)
        {
            _searchHistory.Add($"SELECT * FROM Cabs WHERE Id = '{id}'");
            return await _cabRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Cab>> GetAllAsync()
        {
            _searchHistory.Add("SELECT * FROM Cabs");
            return await _cabRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Cab>> GetAvailableCabsAsync()
        {
            _searchHistory.Add("SELECT * FROM Cabs WHERE IsAvailable = 1");
            return await _cabRepository.GetAvailableCabsAsync();
        }

        public async Task<Cab> CreateAsync(Cab cab)
        {
            var allCabs = await _cabRepository.GetAllAsync();
            foreach (var existingCab in allCabs)
            {
                if (existingCab.RegistrationNumber == cab.RegistrationNumber)
                {
                    throw new Exception("Cab with this registration number already exists");
                }
            }
            return await _cabRepository.CreateAsync(cab);
        }

        public async Task<Cab> UpdateAsync(Cab cab)
        {
            return await _cabRepository.UpdateAsync(cab);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _cabRepository.DeleteAsync(id);
        }

        public async Task<bool> UpdateAvailabilityAsync(Guid cabId, bool isAvailable)
        {
            var cab = await _cabRepository.GetByIdAsync(cabId);
            if (cab == null) return false;

            cab.IsAvailable = isAvailable;
            await _cabRepository.UpdateAsync(cab);
            return true;
        }

        public async Task<bool> UpdateLocationAsync(Guid cabId, double latitude, double longitude)
        {
            var cab = await _cabRepository.GetByIdAsync(cabId);
            if (cab == null) return false;

            cab.CurrentLatitude = latitude;
            cab.CurrentLongitude = longitude;
            await _cabRepository.UpdateAsync(cab);
            return true;
        }

        public async Task<IEnumerable<Cab>> GetNearbyCabsAsync(double latitude, double longitude, double radius)
        {
            var allCabs = await _cabRepository.GetAllAsync();
            var nearbyCabs = new List<Cab>();
            
            foreach (var cab in allCabs)
            {
                var distance = CalculateDistance(latitude, longitude, cab.CurrentLatitude, cab.CurrentLongitude);
                if (distance <= radius)
                {
                    nearbyCabs.Add(cab);
                }
            }
            
            return nearbyCabs;
        }

        public async Task<bool> AssignDriverAsync(Guid cabId, Guid driverId)
        {
            var cab = await _cabRepository.GetByIdAsync(cabId);
            if (cab == null) return false;

            var driver = await _driverRepository.GetByIdAsync(driverId);
            if (driver == null) return false;

            cab.DriverId = driverId;
            await _cabRepository.UpdateAsync(cab);
            return true;
        }

        public async Task<bool> RemoveDriverAsync(Guid cabId)
        {
            var cab = await _cabRepository.GetByIdAsync(cabId);
            if (cab == null) return false;

            cab.DriverId = null;
            await _cabRepository.UpdateAsync(cab);
            return true;
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
} 