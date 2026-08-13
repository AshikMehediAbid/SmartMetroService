using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Managers;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<Settings> GetSystemSettings()
    {
        var settings = await _unitOfWork.AdminRepository.GetSettingsAsync();

        return settings;
    }

    public async Task UpdateSystemSettings(Settings settings)
    {
        var settingsData = await _unitOfWork.AdminRepository.GetSettingsAsync();

        settingsData.UnitFare = settings.UnitFare;
        settingsData.MinimumFare = settings.MinimumFare;
        settings.UpdatedAt = DateTime.Now;

        await _unitOfWork.CompleteAsync();
    }
}
