using HashtagHelp.DAL;
using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HashtagHelp.Domain.RequestModels;

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

        private readonly IApiRequestService _apiRequestService;

        private readonly AppDbContext _context;

        private readonly IDataRepository _dataRepository;

        private readonly IParserDataService _parserDataService;

        private readonly IHashtagApiRequestService _hashtagApiRequestService;

        private readonly IProcessLogger _processLogger;

        private readonly IGoogleApiRequestService _googleApiRequestService;

        public ResearchedUserController(AppDbContext context, IFunnelService funnelCreatedService,
            IFollowersGetterService followersGetterService, IFollowingTagsGetterService followingTagsGetterService,
            IIdGetterService idGetterService, IApiRequestService apiRequestService, IDataRepository dataRepository,
            IParserDataService parserDataService, IHashtagApiRequestService hashtagApiRequestService, 
            IProcessLogger processLogger, IGoogleApiRequestService googleApiRequestService)
        {
            _context = context;
            _funnelCreatorService = funnelCreatedService;
            _followersGetterService = followersGetterService;
            _followingTagsGetterService = followingTagsGetterService;
            _idGetterService = idGetterService;
            _dataRepository = dataRepository;
            _apiRequestService = apiRequestService;
            _parserDataService = parserDataService;
            _hashtagApiRequestService = hashtagApiRequestService;
            _processLogger = processLogger;
            _googleApiRequestService = googleApiRequestService;
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
            var user = new UserEntity
            {
                NickName = requestData.NickName,
                SocialId = requestData.Id
            };
            var generalTask = new GeneralTaskEntity();
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
            generalTask.CollectionTask = parserTask;
            generalTask.HashtagArea = requestData.HashtagArea;
            generalTask.User = user;
            _followersGetterService.FollowingTagsGetter = _followingTagsGetterService;
            _funnelCreatorService.ApiRequestService = _apiRequestService;
            _funnelCreatorService.ParserDataService = _parserDataService;
            _funnelCreatorService.HashtagApiRequestService = _hashtagApiRequestService;
            _funnelCreatorService.ProcessLogger = _processLogger;
            _funnelCreatorService.GoogleApiRequestService = _googleApiRequestService;
            _funnelCreatorService.DataRepository = _dataRepository;
            _dataRepository.AddGeneralTask(generalTask);
            await _dataRepository.SaveChangesAsync();
            await _funnelCreatorService.AddFollowersTaskAsync(generalTask);
            return CreatedAtAction(nameof(GetResearchedUser),
                new { id = parserTask.Id }, parserTask);
        }
    }
}
