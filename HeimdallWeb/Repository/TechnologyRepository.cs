using System.Text.Json;
using HeimdallWeb.DTO;
using HeimdallWeb.DTO.Mappers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.Enums;

namespace HeimdallWeb.Repository
{
    public class TechnologyRepository : ITechnologyRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogRepository _logRepository;
        public TechnologyRepository(AppDbContext dbContext, ILogRepository logRepository)
        {
            _appDbContext = dbContext;
            _logRepository = logRepository;
        }

        public async Task<List<TechnologyModel>> getTechnologiesByHistoryId(int historyId)
        {
            var technologies = await 
                _appDbContext.Technology
                                .Where(t => t.history_id == historyId)
                                .Select(t =>
                                    new TechnologyModel
                                    {
                                        technology_name = t.technology_name,
                                        version = t.version,
                                        technology_category = t.technology_category,
                                        technology_description = t.technology_description,
                                        history_id = t.history_id,
                                    })
                                .AsNoTracking()
                                .ToListAsync();

            return technologies;
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
            
            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.DB_SAVE_OK,
                message = "Registro salvo com sucesso",
                source = "TechnologyRepository",
                history_id = historyId,
                details = $"Salvas {tecnologias.Count} tecnologias"
            });
        }

    }
}
