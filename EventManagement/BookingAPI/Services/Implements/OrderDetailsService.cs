using AutoMapper;
using AutoMapper.QueryableExtensions;
using BookingAPI.DTOs.OrderDetailsDTO;
using BookingAPI.Models;
using BookingAPI.Repositories;

namespace BookingAPI.Services.Implements
{
    public class OrderDetailsService : IOrderDetailsService
    {
        private readonly IOrderDetailsRepository _repository;
        IMapper _mapper;

        public OrderDetailsService(IOrderDetailsRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task Add(CreateOrderDetailsDTO model)
        {
            await _repository.Add(_mapper.Map<OrderDetail>(model));
        }

        public async Task Delete(int id)
        {
            await _repository.Delete(id);
        }

        public IQueryable<ReadOrderDetailsDTO> GetAll()
        {
            return _repository.GetAll()
            .ProjectTo<ReadOrderDetailsDTO>(_mapper.ConfigurationProvider);

        }

        public async Task<ReadOrderDetailsDTO> GetById(int id)
        {
            return _mapper.Map<ReadOrderDetailsDTO>(await _repository.GetById(id));
        }

        public async Task Update(int id, UpdateOrderDetailsDTO model)
        {
            var p = await _repository.GetById(id);
            _mapper.Map(model, p);
            await _repository.Update(p);
        }


    }
}
