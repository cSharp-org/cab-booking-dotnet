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
            return await _cabRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Cab>> GetAllAsync()
        {
            return await _cabRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Cab>> GetAvailableCabsAsync()
        {
            return await _cabRepository.GetAvailableCabsAsync();
        }

        public async Task<Cab> CreateAsync(Cab cab)
        {
            // In a real application, you would:
            // 1. Validate cab details
            // 2. Check registration number uniqueness
            // 3. Validate cab type
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
            var result = await _cabRepository.UpdateAvailabilityAsync(cabId, isAvailable);
            if (result)
            {
                var cab = await _cabRepository.GetByIdAsync(cabId);
                if (cab?.DriverId.HasValue == true)
                {
                    await _notificationService.CreateAsync(new Notification
                    {
                        UserId = cab.DriverId.Value,
                        Title = "Cab Availability Updated",
                        Message = $"Cab {cabId} is now {(isAvailable ? "available" : "unavailable")}.",
                        Type = "CabUpdate"
                    });
                }
            }
            return result;
        }

        public async Task<bool> UpdateLocationAsync(Guid cabId, double latitude, double longitude)
        {
            return await _cabRepository.UpdateLocationAsync(cabId, latitude, longitude);
        }

        public async Task<IEnumerable<Cab>> GetNearbyCabsAsync(double latitude, double longitude, double radius)
        {
            return await _cabRepository.GetNearbyCabsAsync(latitude, longitude, radius);
        }

        public async Task<bool> AssignDriverAsync(Guid cabId, Guid driverId)
        {
            var result = await _cabRepository.AssignDriverAsync(cabId, driverId);
            
            await _notificationService.CreateAsync(new Notification
            {
                UserId = driverId,
                Title = "Cab Assignment",
                Message = $"You have been assigned to cab {cabId}.",
                Type = "CabAssignment"
            });
            
            return true; 
        }

        public async Task<bool> RemoveDriverAsync(Guid cabId)
        {
            var cab = await _cabRepository.GetByIdAsync(cabId);
            if (cab?.DriverId.HasValue == true)
            {
                var driverId = cab.DriverId.Value;
                var result = await _cabRepository.RemoveDriverAsync(cabId);
                if (result)
                {
                    await _notificationService.CreateAsync(new Notification
                    {
                        UserId = driverId,
                        Title = "Cab Assignment Removed",
                        Message = $"You have been removed from cab {cabId}.",
                        Type = "CabAssignment"
                    });
                }
                return result;
            }
            return false;
        }
    }
} 