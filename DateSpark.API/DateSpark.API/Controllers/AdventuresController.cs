using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DateSpark.API.Services;
using DateSpark.API.Models;
using Microsoft.EntityFrameworkCore;
using DateSpark.API.Data;

namespace DateSpark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdventuresController : ControllerBase
    {
        private readonly IAdventureService _adventureService;
       private readonly AppDbContext _context;

        public AdventuresController(IAdventureService adventureService, AppDbContext context)
        {
            _adventureService = adventureService;
            _context = context;
        }

        // 🔥 ПОМОЩНИК: Получить ID текущего пользователя
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Пользователь не авторизован");
            }
            return userId;
        }

        // 🔥 ПОМОЩНИК: Получить ID пары текущего пользователя
        private async Task<int> GetCurrentCoupleIdAsync(int userId)
        {
            var userCouple = await _context.UserCouples
                .Include(uc => uc.Couple)
                .FirstOrDefaultAsync(uc => uc.UserId == userId);
            
            if (userCouple?.Couple == null)
            {
                throw new InvalidOperationException("Вы не состоите в паре");
            }
            
            return userCouple.Couple.Id;
        }

        // 🔥 ПОМОЩНИК: Преобразовать AdventureCard в AdventureResponse
        private async Task<AdventureResponse> ToAdventureResponseAsync(AdventureCard card)
        {
            var createdByUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == card.CreatedByUserId);
            
            return new AdventureResponse
            {
                Id = card.Id,
                IdeaId = card.IdeaId,
                CoupleId = card.CoupleId,
                CreatedByUserId = card.CreatedByUserId,
                CreatedByUserName = createdByUser?.Name ?? "Неизвестно",
                Title = card.Title,
                Description = card.Description,
                Status = card.Status,
                StatusSymbol = card.StatusSymbol,
                PlannedDate = card.PlannedDate,
                CompletedDate = card.CompletedDate,
                Notes = card.Notes,
                PhotoUrl = card.PhotoUrl,
                CreatedAt = card.CreatedAt,
                UpdatedAt = card.UpdatedAt
            };
        }

        // ✅ ПОЛУЧИТЬ ВСЕ КАРТОЧКИ ПАРЫ
        [HttpGet("couple")]
        public async Task<ActionResult<List<AdventureResponse>>> GetCoupleAdventures()
        {
            try
            {
                var userId = GetCurrentUserId();
                var coupleId = await GetCurrentCoupleIdAsync(userId);
                
                var adventures = await _adventureService.GetCoupleAdventuresAsync(coupleId);
                var responses = new List<AdventureResponse>();
                
                foreach (var adventure in adventures)
                {
                    responses.Add(await ToAdventureResponseAsync(adventure));
                }
                
                return Ok(responses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ ПОЛУЧИТЬ КАРТОЧКИ ПО СТАТУСУ
        [HttpGet("couple/status/{status}")]
        public async Task<ActionResult<List<AdventureResponse>>> GetCoupleAdventuresByStatus(AdventureStatus status)
        {
            try
            {
                var userId = GetCurrentUserId();
                var coupleId = await GetCurrentCoupleIdAsync(userId);
                
                var adventures = await _adventureService.GetCoupleAdventuresByStatusAsync(coupleId, status);
                var responses = new List<AdventureResponse>();
                
                foreach (var adventure in adventures)
                {
                    responses.Add(await ToAdventureResponseAsync(adventure));
                }
                
                return Ok(responses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ СОЗДАТЬ ИЗ ИДЕИ
        [HttpPost("from-idea")]
        public async Task<ActionResult<AdventureResponse>> CreateFromIdea([FromBody] CreateAdventureFromIdeaRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var coupleId = await GetCurrentCoupleIdAsync(userId);
                
                var adventure = await _adventureService.CreateFromIdeaAsync(
                    request.IdeaId, coupleId, userId);
                
                var response = await ToAdventureResponseAsync(adventure);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ СОЗДАТЬ ВРУЧНУЮ
        [HttpPost("manual")]
        public async Task<ActionResult<AdventureResponse>> CreateManual([FromBody] CreateAdventureManualRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var coupleId = await GetCurrentCoupleIdAsync(userId);
                
                var adventure = await _adventureService.CreateManualAsync(
                    request.Title, request.Description, coupleId, userId);
                
                var response = await ToAdventureResponseAsync(adventure);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ ОБНОВИТЬ СТАТУС
        [HttpPut("{id}/status")]
        public async Task<ActionResult<AdventureResponse>> UpdateStatus(int id, [FromBody] UpdateAdventureStatusRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var adventure = await _adventureService.UpdateStatusAsync(id, request.Status, userId);
                
                var response = await ToAdventureResponseAsync(adventure);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ ОБНОВИТЬ ДАТУ
        [HttpPut("{id}/date")]
        public async Task<ActionResult<AdventureResponse>> UpdateDate(int id, [FromBody] UpdateAdventureDateRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var adventure = await _adventureService.UpdatePlannedDateAsync(id, request.PlannedDate, userId);
                
                var response = await ToAdventureResponseAsync(adventure);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ ОБНОВИТЬ ЗАМЕТКИ
        [HttpPut("{id}/notes")]
        public async Task<ActionResult<AdventureResponse>> UpdateNotes(int id, [FromBody] UpdateAdventureNotesRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var adventure = await _adventureService.UpdateNotesAsync(id, request.Notes, userId);
                
                var response = await ToAdventureResponseAsync(adventure);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ ЗАВЕРШИТЬ ПРИКЛЮЧЕНИЕ
        [HttpPut("{id}/complete")]
        public async Task<ActionResult<AdventureResponse>> CompleteAdventure(int id, [FromBody] CompleteAdventureRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var adventure = await _adventureService.CompleteAdventureAsync(
                    id, request.PhotoUrl, request.Notes, userId);
                
                var response = await ToAdventureResponseAsync(adventure);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ УДАЛИТЬ КАРТОЧКУ
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAdventure(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _adventureService.DeleteAdventureAsync(id, userId);
                
                if (success)
                {
                    return Ok(new { success = true, message = "Карточка удалена" });
                }
                
                return NotFound(new { success = false, message = "Карточка не найдена" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ ПРОВЕРИТЬ, МОЖНО ЛИ СОЗДАТЬ ИЗ ИДЕИ
        [HttpGet("can-create/{ideaId}")]
        public async Task<ActionResult> CanCreateFromIdea(int ideaId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var coupleId = await GetCurrentCoupleIdAsync(userId);
                
                var canCreate = await _adventureService.CanCreateFromIdeaAsync(ideaId, coupleId);
                
                return Ok(new { canCreate });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}