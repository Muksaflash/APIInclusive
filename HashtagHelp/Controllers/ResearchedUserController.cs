using HashtagHelp.DAL;
using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HashtagHelp.Domain.RequestModels;
using HashtagHelp.Services.Implementations;

namespace HashtagHelp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResearchedUserController : ControllerBase
    {
        private readonly IIdGetterService _idGetterService;

        private readonly IFollowingTagsGetterService _followingTagsGetterService;

        private readonly IFunnelService _funnelCreatorService;

        private readonly IFollowersGetterService _followersGetterService;

        private readonly IFollowersTaskService _followersTaskService;

        private readonly AppDbContext _context;

        private readonly IDataRepository _dataRepository;

        public ResearchedUserController(AppDbContext context, IFunnelService funnelCreatedService,
            IFollowersGetterService followersGetterService, IFollowingTagsGetterService followingTagsGetterService,
            IIdGetterService idGetterService, IFollowersTaskService followersTaskService, IDataRepository dataRepository)
        {
            _context = context;
            _funnelCreatorService = funnelCreatedService;
            _followersGetterService = followersGetterService;
            _followingTagsGetterService = followingTagsGetterService;
            _idGetterService = idGetterService;
            _followersTaskService = followersTaskService;
            _dataRepository = dataRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResearchedUserEntity>>> GetResearchedUsers()
        {
            if (_context.ResearchedUsers == null)
            {
                return NotFound();
            }
            return await _context.ResearchedUsers.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResearchedUserEntity>> GetResearchedUser(uint id)
        {
            if (_context.ResearchedUsers == null)
            {
                return NotFound();
            }
            var researchedUser = await _context.ResearchedUsers.FindAsync(id);

            if (researchedUser == null)
            {
                return NotFound();
            }

            return researchedUser;
        }

        [HttpPost]
        public async Task<ActionResult<ResearchedUserEntity>> PostResearchedUser([FromBody] UserRequestModel requestData)
        {
            if (_context.ResearchedUsers == null)
                return Problem("Entity set 'AppDbContext.ResearchedUsers' is null.");

            var nickName = requestData.NickName;
            if (string.IsNullOrEmpty(nickName))
                return BadRequest("Invalid request data.");
            var telegramUser = new TelegramUserEntity
            {
                NickName = nickName
            };
            var parserTask = new ParserTaskEntity();
            foreach (var name in requestData.RequestNickNames)
            {
                parserTask.ResearchedUsers.Add(new ResearchedUserEntity 
                { 
                    NickName = name,
                    FollowersGetter = _followersGetterService,
                    IdGetter = _idGetterService
                });
            };
            _dataRepository.AddTask(parserTask);
            _dataRepository.AddTelegramUser(telegramUser);
            _followersGetterService.FollowingTagsGetter = _followingTagsGetterService;
            _funnelCreatorService.followersTaskService = _followersTaskService;
            await _dataRepository.SaveChangesAsync();
            await _funnelCreatorService.AddFollowersTaskAsync(parserTask);
            return CreatedAtAction(nameof(GetResearchedUser),
                new { id = parserTask.Id }, parserTask);
        }
    }
}
