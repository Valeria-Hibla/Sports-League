using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers
{
    [ApiController]
    [Route("api/match/{matchId}/lineup")]
    public class MatchLineupController : ControllerBase
    {
        private readonly IMatchLineupService _lineupService;
        private readonly IMapper _mapper;

        public MatchLineupController(IMatchLineupService lineupService, IMapper mapper)
        {
            _lineupService = lineupService;
            _mapper = mapper;
        }

        // POST /api/match/{matchId}/lineup
        [HttpPost]
        public async Task<ActionResult<MatchLineupResponseDTO>> AddPlayer(
            int matchId, [FromBody] MatchLineupRequestDTO dto)
        {
            try
            {
                var lineup = _mapper.Map<MatchLineup>(dto);
                var created = await _lineupService.AddPlayerToLineupAsync(matchId, lineup);

                // Reload with details for the response
                var fullLineup = await _lineupService.GetLineupByMatchAsync(matchId);
                var createdEntry = fullLineup.FirstOrDefault(l => l.Id == created.Id);
                return CreatedAtAction(nameof(GetLineup),
                    new { matchId },
                    _mapper.Map<MatchLineupResponseDTO>(createdEntry));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        // GET /api/match/{matchId}/lineup
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetLineup(int matchId)
        {
            try
            {
                var lineup = await _lineupService.GetLineupByMatchAsync(matchId);
                return Ok(_mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineup));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // GET /api/match/{matchId}/lineup/team/{teamId}
        [HttpGet("team/{teamId}")]
        public async Task<ActionResult<IEnumerable<MatchLineupResponseDTO>>> GetLineupByTeam(
            int matchId, int teamId)
        {
            try
            {
                var lineup = await _lineupService.GetLineupByMatchAndTeamAsync(matchId, teamId);
                return Ok(_mapper.Map<IEnumerable<MatchLineupResponseDTO>>(lineup));
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // DELETE /api/match/{matchId}/lineup/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLineupEntry(int matchId, int id)
        {
            try
            {
                await _lineupService.DeleteLineupEntryAsync(matchId, id);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}
