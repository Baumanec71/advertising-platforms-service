namespace advertising_platforms_service.Domain.ViewModels
{
    public record PaginatedViewModelResponse<T, M>(List<T> viewModels, int totalPages, M? filter);
}
