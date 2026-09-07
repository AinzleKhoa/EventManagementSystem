using AuthenticationAPI.DTOs;
using AuthenticationAPI.Models;
using AuthenticationAPI.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace AuthenticationAPI.Services.Implements
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public IQueryable<ReadRoleDTO> GetAll()
        {
            var rolesQuery = _roleRepository.GetAll();
            return rolesQuery.ProjectTo<ReadRoleDTO>(_mapper.ConfigurationProvider);
        }

        public async Task<ReadRoleDTO?> GetById(int id)
        {
            var role = await _roleRepository.GetById(id);
            return role == null ? null : _mapper.Map<ReadRoleDTO>(role);
        }

        public async Task Create(CreateRoleDTO dto)
        {
            var role = _mapper.Map<Role>(dto);
            await _roleRepository.Add(role);
        }

        public async Task Update(int id, UpdateRoleDTO dto)
        {
            var role = await _roleRepository.GetById(id);
            if (role == null)
                throw new Exception("Role not found");

            _mapper.Map(dto, role);
            await _roleRepository.Update(role);
        }

        public async Task Delete(int id)
        {
            var role = await _roleRepository.GetById(id);
            if (role == null)
                throw new Exception("Role not found");

            await _roleRepository.Delete(role);
        }

        public Task<bool> IsExist(int id)
        {
            return _roleRepository.IsExist(id);
        }
    }
}
