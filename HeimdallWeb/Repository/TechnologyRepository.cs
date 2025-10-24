using System.Text.Json;
using HeimdallWeb.DTO;
using HeimdallWeb.DTO.Mappers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;

namespace HeimdallWeb.Repository
{
    public class TechnologyRepository : ITechnologyRepository
    {
        private readonly AppDbContext _appDbContext;
        public TechnologyRepository(AppDbContext dbContext)
        {
            _appDbContext = dbContext;
        }

        public async Task<List<TechnologyModel>> getTechnologiesByHistoryId(int historyId)
        {
            throw new NotImplementedException();
        }

        public async Task SaveTechnologiesFromAI(string iaResponse, int historyId)
        {
            var wrapper = JsonSerializer.Deserialize<AIResponseDTO>(iaResponse);
            var tecnologiasDto = wrapper?.tecnologias;

            if (tecnologiasDto is null || tecnologiasDto.Count == 0)
                    return;

            var tecnologias = tecnologiasDto.Select(dto => TechnologyDTOMapper
                .ToModel(dto, historyId)).ToList();
            
            await _appDbContext.Technology.AddRangeAsync(tecnologias);
            await _appDbContext.SaveChangesAsync();
        }

    }
}
