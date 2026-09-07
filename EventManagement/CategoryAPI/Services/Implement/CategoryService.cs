using AutoMapper;
using AutoMapper.QueryableExtensions;
using CategoryAPI.DTOs;
using CategoryAPI.Models;
using CategoryAPI.Repositories;

namespace CategoryAPI.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly IEventApiClient _eventApiClient;

        public CategoryService(ICategoryRepository repository, IMapper mapper, IEventApiClient eventApiClient)
        {
            _repository = repository;
            _mapper = mapper;
            _eventApiClient = eventApiClient;
        }
        public async Task Add(CreateCategoryDTO model)
        {
            if (await _repository.IsCategoryNameExists(model.CategoryName))
            {
                throw new InvalidOperationException($"Category name '{model.CategoryName}' already exists.");
            }

            var category = _mapper.Map<Category>(model);
            category.IsActive = model.IsActive ?? true;
            await _repository.Add(category);
        }

        public async Task Delete(int id)
        {
            var category = await _repository.GetById(id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            if (await _eventApiClient.HasRelatedEventsAsync(id))
            {
                throw new InvalidOperationException($"Cannot delete category '{category.CategoryName}' because it has related events.");
            }

            await _repository.Delete(id);
        }

        public IQueryable<ReadCategoryDTO> GetAllQueryable()
            => _repository.GetQueryable().ProjectTo<ReadCategoryDTO>(_mapper.ConfigurationProvider);

        public async Task<ReadCategoryDTO> GetById(int id)
        {
            return _mapper.Map<ReadCategoryDTO>(await _repository.GetById(id));
        }

        public async Task Update(int id, UpdateCategoryDTO model)
        {
            var p = await _repository.GetById(id);
            if (p == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            if (await _repository.IsCategoryNameExists(model.CategoryName, id))
            {
                throw new InvalidOperationException($"Category name '{model.CategoryName}' already exists.");
            }

            _mapper.Map(model, p);
            await _repository.Update(p);
        }

        public async Task<PaginatedResultDTO<ReadCategoryDTO>> SearchAndFilter(CategorySearchFilterDTO filter)
        {
            // Validation: Ensure valid pagination parameters
            if (filter.PageNumber < 1)
            {
                filter.PageNumber = 1;
            }
            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                filter.PageSize = 10; // Default page size
            }

            var (items, totalCount) = await _repository.SearchAndFilter(filter);

            return new PaginatedResultDTO<ReadCategoryDTO>
            {
                Items = _mapper.Map<List<ReadCategoryDTO>>(items),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

    }
}
