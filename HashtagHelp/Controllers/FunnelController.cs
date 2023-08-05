using HashtagHelp.DAL;
using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using HashtagHelp.Domain.RequestModels;

namespace HashtagHelp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FunnelController : ControllerBase
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

        public FunnelController(AppDbContext context, IFunnelService funnelCreatedService,
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

            _followersGetterService.FollowingTagsGetter = _followingTagsGetterService;
            _funnelCreatorService.InstaParserApiRequestService = _apiRequestService;
            _funnelCreatorService.ParserDataService = _parserDataService;
            _funnelCreatorService.HashtagApiRequestService = _hashtagApiRequestService;
            _funnelCreatorService.ProcessLogger = _processLogger;
            _funnelCreatorService.GoogleApiRequestService = _googleApiRequestService;
            _funnelCreatorService.DataRepository = _dataRepository;

            _dataRepository.GoogleApiRequestService = _googleApiRequestService;
        }

        /// <summary>
        ///  Init
        /// </summary>
        /// <param name="requestData">kjp</param>
        /// <returns>www</returns>
        [HttpPost]
        public async Task<ActionResult<string>> PostFunnelRequest([FromBody] FunnelRequestModel requestData)
        {
            try
            {
                if (_context.ResearchedUsers == null)
                    return Problem("Entity set 'AppDbContext.ResearchedUsers' is null.");
                var user = new UserEntity
                {
                    NickName = requestData.NickName,
                    SocialId = requestData.Id
                };
                var generalTask = new GeneralTaskEntity();
                var collectionTask = new ParserTaskEntity();
                var filtrationTask = new ParserTaskEntity();

                foreach (var name in requestData.RequestNickNames)
                {
                    collectionTask.ResearchedUsers.Add(new ResearchedUserEntity
                    {
                        NickName = name,
                        FollowersGetter = _followersGetterService,
                        IdGetter = _idGetterService
                    });
                };
                generalTask.CollectionTask = collectionTask;
                generalTask.FiltrationTask = filtrationTask;
                generalTask.HashtagArea = requestData.HashtagArea;
                generalTask.User = user;
                await _funnelCreatorService.SetConfigureAsync(generalTask);
                await _dataRepository.CheckAndDeleteOldRecordsAsync();
                _dataRepository.AddGeneralTask(generalTask);
                _dataRepository.AddParserTask(collectionTask);
                _dataRepository.AddParserTask(filtrationTask);
                _dataRepository.AddUser(user);
                await _dataRepository.SaveChangesAsync();
                await _funnelCreatorService.StartTaskChainAsync();
                await _funnelCreatorService.WaitCompletionGeneralTask();
                var funnelText = generalTask.HashtagFunnel.FunnelText;
                return Ok(funnelText);
            }
            catch (Exception ex)
            {
                if (ex.Message == "paid subscription only")
                {
                    return Problem("не оплачен инста парсер");
                }
                return Problem(ex.Message);
            }
        }
    }
}
