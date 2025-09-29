using System.Text.Json;
using HeimdallWeb.Data;
using HeimdallWeb.DTO;
using HeimdallWeb.Enums;
using HeimdallWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Repository
{
    public class FindingRepository : IFindingRepository
    {
        private readonly AppDbContext _appDbContext;
        public FindingRepository(AppDbContext appDbContext)
        {
             _appDbContext = appDbContext;
        }

        public async Task<List<FindingModel>> getFindingsByHistoryId(int history_id)
        {
            var findings = await 
                _appDbContext.Finding
                                .Select(f =>
                                    new FindingModel
                                    {
                                        type = f.type,
                                        description = f.description,
                                        severity_string = f.severity.MapStatus(),
                                        recommendation = f.recommendation,
                                        history_id = f.history_id,
                                    })
                                .Where(f => f.history_id == history_id)
                                .AsNoTracking()
                                .ToListAsync();

            return findings;
        }

        public async Task<FindingModel> insertFinding(FindingModel finding)
        {
            await _appDbContext.Finding.AddAsync(finding);
            await _appDbContext.SaveChangesAsync();

            return finding;
        }

        public async Task SaveFindingsFromIAAsync(string iaResponse, int historyId)
        {
            // Parse do JSON retornado pela IA
            var wrapper = JsonSerializer.Deserialize<FindingsWrapper>(iaResponse);
            var findingsDto = wrapper.achados;

            if (findingsDto == null) return;

            var findings = findingsDto.Select(dto => FindingDTOMapper.ToModel(dto, historyId)).ToList();

            foreach (var finding in findings)
            {
                await insertFinding(finding);
            }
        }
    }
}
