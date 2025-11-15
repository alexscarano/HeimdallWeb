using System.Text.Json;
using HeimdallWeb.DTO;
using HeimdallWeb.DTO.Mappers;
using HeimdallWeb.Enums;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;

namespace HeimdallWeb.Repository
{
    public class FindingRepository : IFindingRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogRepository _logRepository;
        public FindingRepository(AppDbContext appDbContext, ILogRepository logRepository)
        {
             _appDbContext = appDbContext;
             _logRepository = logRepository;
        }

        public async Task<List<FindingModel>> getFindingsByHistoryId(int history_id)
        {
            var findings = await 
                _appDbContext.Finding
                                .Where(f => f.history_id == history_id)
                                .Select(f =>
                                    new FindingModel
                                    {
                                        type = f.type,
                                        description = f.description,
                                        severity_string = f.severity.MapStatus(),
                                        recommendation = f.recommendation,
                                        history_id = f.history_id,
                                    })
                                .AsNoTracking()
                                .ToListAsync();

            return findings;
        }

        public async Task SaveFindingsFromAI(string iaResponse, int historyId)
        {
            // Parse do JSON retornado pela IA
            var wrapper = JsonSerializer.Deserialize<AIResponseDTO>(iaResponse);
            var findingsDto = wrapper?.achados;

            if (findingsDto is null || findingsDto.Count == 0) 
                return;

            var findings = findingsDto.Select(dto => FindingDTOMapper.ToModel(dto, historyId)).ToList();

            await _appDbContext.Finding.AddRangeAsync(findings);
            await _appDbContext.SaveChangesAsync();
            
            await _logRepository.AddLog(new LogModel
            {
                code = LogEventCode.DB_SAVE_OK,
                message = "Registro salvo com sucesso",
                source = "FindingRepository",
                history_id = historyId,
                details = $"Salvos {findings.Count} achados"
            });
        }
    }
}
