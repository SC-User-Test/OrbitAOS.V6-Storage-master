using OrbitAOS.Application.DTOs;
using OrbitAOS.Application.Interfaces;
using OrbitAOS.Domain.Entities;
using OrbitAOS.Domain.Interfaces;

namespace OrbitAOS.Application.Services
{
    /// <summary>
    /// Application service implementing user profile business logic.
    /// Orchestrates domain operations and maps between domain entities and DTOs.
    /// </summary>
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _repository;

        /// <summary>
        /// Initializes a new instance of <see cref="UserProfileService"/>.
        /// </summary>
        /// <param name="repository">The user profile repository.</param>
        public UserProfileService(IUserProfileRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc />
        public async Task<UserProfileDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity is null ? null : MapToDto(entity);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<UserProfileDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(MapToDto);
        }

        /// <inheritdoc />
        public async Task<UserProfileDto?> GetByIdentityUserIdAsync(string identityUserId)
        {
            var entity = await _repository.GetByIdentityUserIdAsync(identityUserId);
            return entity is null ? null : MapToDto(entity);
        }

        /// <inheritdoc />
        public async Task<UserProfileDto> CreateAsync(UserProfileDto dto)
        {
            var entity = MapToEntity(dto);
            entity.CreatedAt = DateTime.UtcNow;
            var created = await _repository.AddAsync(entity);
            return MapToDto(created);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(UserProfileDto dto)
        {
            var entity = MapToEntity(dto);
            entity.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(entity);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        // ── Private mapping helpers ──────────────────────────────────────────

        private static UserProfileDto MapToDto(UserProfile entity) => new()
        {
            Id = entity.Id,
            IdentityUserId = entity.IdentityUserId,
            DisplayName = entity.DisplayName,
            Email = entity.Email,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };

        private static UserProfile MapToEntity(UserProfileDto dto) => new()
        {
            Id = dto.Id,
            IdentityUserId = dto.IdentityUserId,
            DisplayName = dto.DisplayName,
            Email = dto.Email,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt
        };
    }
}
