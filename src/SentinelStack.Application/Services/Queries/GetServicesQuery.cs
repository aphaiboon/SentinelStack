using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Services.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Services.Queries;

/// <summary>
/// Request model for filtering services.
/// </summary>
public class GetServicesRequest
{
    public bool? IsActive { get; init; }
    public string? OwnerTeam { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Response model for paginated services.
/// </summary>
public class GetServicesResponse
{
    public required IReadOnlyList<ServiceDto> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Query to get services with filtering and pagination.
/// </summary>
public class GetServicesQuery
{
    private readonly IServiceRepository _serviceRepository;

    public GetServicesQuery(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<GetServicesResponse> ExecuteAsync(GetServicesRequest request, CancellationToken cancellationToken = default)
    {
        var services = await _serviceRepository.GetAllAsync(cancellationToken);

        // Apply filters
        var filtered = services.AsEnumerable();

        if (request.IsActive.HasValue)
        {
            filtered = filtered.Where(s => s.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrEmpty(request.OwnerTeam))
        {
            filtered = filtered.Where(s => s.OwnerTeam == request.OwnerTeam);
        }

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;

        // Apply pagination
        var paged = filteredList
            .OrderBy(s => s.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();

        return new GetServicesResponse
        {
            Items = paged,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    private static ServiceDto MapToDto(Service service) => new()
    {
        Id = service.Id,
        TenantId = service.TenantId,
        Name = service.Name,
        ServiceKey = service.ServiceKey,
        Description = service.Description,
        IsActive = service.IsActive,
        OwnerTeam = service.OwnerTeam,
        ContactEmail = service.ContactEmail,
        SlackChannel = service.SlackChannel,
        CreatedAtUtc = service.CreatedAtUtc,
        CreatedBy = service.CreatedBy,
        UpdatedAtUtc = service.UpdatedAtUtc,
        UpdatedBy = service.UpdatedBy
    };
}
