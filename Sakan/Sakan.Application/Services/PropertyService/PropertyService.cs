using Microsoft.AspNetCore.Http;
using Sakan.Application.DTO;
using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;

namespace Sakan.Application.Services.PropertyService;

public class PropertyService(IUnitOfWork unitOfWork, IAttachmentService attachmentService) : IPropertyService
{
    private const string PhotoFolder = "Properties";

    public async Task<IEnumerable<PropertyDto>> GetAllAsync()
    {
        var properties = (await unitOfWork.GetRepository<Property, Guid>()
            .GetAllAsync(p => !p.IsDeleted)).ToList();
        return await MapListAsync(properties);
    }

    public async Task<PropertyDto?> GetByIdAsync(Guid id)
    {
        var property = await unitOfWork.GetRepository<Property, Guid>().GetByIdAsync(id);
        if (property is null || property.IsDeleted)
            return null;

        return await MapSingleAsync(property);
    }

    public async Task<int> CreateAsync(PropertyDto dto, string currentOwnerId)
    {
        var property = PropertyDto.ToEntity(dto, currentOwnerId);
        var propertyRepo = unitOfWork.GetRepository<Property, Guid>();
        await propertyRepo.AddAsync(property);

        await AddPhotosAsync(property.Id, dto.Images);
        await AddFeaturesAsync(property.Id, dto.Features);

        return await unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(PropertyDto dto)
    {
        if (dto.Id is null)
            return false;

        var repo = unitOfWork.GetRepository<Property, Guid>();
        var property = await repo.GetByIdAsync(dto.Id.Value);
        if (property is null || property.IsDeleted)
            return false;

        PropertyDto.MapOntoEntity(property, dto);
        repo.Update(property);

        await ReplaceFeaturesAsync(property.Id, dto.Features);
        await AddPhotosAsync(property.Id, dto.Images);

        return await unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var repo = unitOfWork.GetRepository<Property, Guid>();
        var property = await repo.GetByIdAsync(id);
        if (property is null)
            return false;

        property.IsDeleted = true;
        property.UpdatedAt = DateTimeOffset.UtcNow;
        repo.Update(property);

        return await unitOfWork.SaveChangesAsync() > 0;
    }

    private async Task<IEnumerable<PropertyDto>> MapListAsync(List<Property> properties)
    {
        if (properties.Count == 0)
            return [];

        var ids = properties.Select(p => p.Id).ToList();
        var photos = (await unitOfWork.GetRepository<PropertyPhoto, Guid>()
            .GetAllAsync(p => ids.Contains(p.PropertyId))).ToLookup(p => p.PropertyId);
        var features = (await unitOfWork.GetRepository<PropertyFeature, Guid>()
            .GetAllAsync(f => ids.Contains(f.PropertyId))).ToLookup(f => f.PropertyId);

        return properties.Select(p =>
            PropertyDto.FromEntity(p, photos[p.Id], features[p.Id]));
    }

    private async Task<PropertyDto> MapSingleAsync(Property property)
    {
        var photos = await unitOfWork.GetRepository<PropertyPhoto, Guid>()
            .GetAllAsync(p => p.PropertyId == property.Id);
        var features = await unitOfWork.GetRepository<PropertyFeature, Guid>()
            .GetAllAsync(f => f.PropertyId == property.Id);
        return PropertyDto.FromEntity(property, photos, features);
    }

    private async Task AddPhotosAsync(Guid propertyId, List<IFormFile>? files)
    {
        if (files is null || files.Count == 0)
            return;

        var photoRepo = unitOfWork.GetRepository<PropertyPhoto, Guid>();
        var sort = 0;
        foreach (var file in files)
        {
            var fileName = attachmentService.UploadAttachment(file, PhotoFolder);
            if (fileName is null)
                continue;

            await photoRepo.AddAsync(new PropertyPhoto
            {
                Id = Guid.NewGuid(),
                PropertyId = propertyId,
                PhotoUrl = fileName,
                SortOrder = sort++
            });
        }
    }

    private async Task AddFeaturesAsync(Guid propertyId, List<string> features)
    {
        if (features.Count == 0)
            return;

        var featureRepo = unitOfWork.GetRepository<PropertyFeature, Guid>();
        foreach (var name in features.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            await featureRepo.AddAsync(new PropertyFeature
            {
                Id = Guid.NewGuid(),
                PropertyId = propertyId,
                Feature = name
            });
        }
    }

    private async Task ReplaceFeaturesAsync(Guid propertyId, List<string> features)
    {
        var featureRepo = unitOfWork.GetRepository<PropertyFeature, Guid>();
        var existing = (await featureRepo.GetAllAsync(f => f.PropertyId == propertyId)).ToList();
        foreach (var f in existing)
            featureRepo.Delete(f);

        await AddFeaturesAsync(propertyId, features);
    }
}
