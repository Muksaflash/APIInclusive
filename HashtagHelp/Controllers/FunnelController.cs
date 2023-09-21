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
        private readonly IFunnelService _funnelCreatorService;

        private readonly IApiRequestService _apiRequestService;

        private readonly AppDbContext _context;

        private readonly IDataRepository _dataRepository;

        private readonly IParserDataService _parserDataService;

        private readonly IHashtagApiRequestService _hashtagApiRequestService;

        private readonly IProcessLogger _processLogger;

        private readonly IGoogleApiRequestService _googleApiRequestService;

        private GeneralTaskEntity? _generalTask;

        public FunnelController(AppDbContext context, IFunnelService funnelCreatedService,
            IApiRequestService apiRequestService, IDataRepository dataRepository,
            IParserDataService parserDataService, IHashtagApiRequestService hashtagApiRequestService,
            IProcessLogger processLogger, IGoogleApiRequestService googleApiRequestService)
        {
            _context = context;
            _funnelCreatorService = funnelCreatedService;
            _dataRepository = dataRepository;
            _apiRequestService = apiRequestService;
            _parserDataService = parserDataService;
            _hashtagApiRequestService = hashtagApiRequestService;
            _processLogger = processLogger;
            _googleApiRequestService = googleApiRequestService;
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
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (_context.ResearchedUsers == null)
                    return Problem("Entity set 'AppDbContext.ResearchedUsers' is null.");
                var user = new UserEntity
                {
                    NickName = requestData.NickName,
                    SocialId = requestData.Id
                };
                _generalTask = new GeneralTaskEntity();
                var collectionTask = new ParserTaskEntity();
                var filtrationTask = new ParserTaskEntity();

                foreach (var name in requestData.RequestNickNames)
                {
                    collectionTask.ResearchedUsers.Add(new ResearchedUserEntity
                    {
                        NickName = name
                    });
                };
                _generalTask.CollectionTask = collectionTask;
                _generalTask.FiltrationTask = filtrationTask;
                _generalTask.HashtagArea = requestData.HashtagArea;
                _generalTask.User = user;
                _dataRepository.AddGeneralTask(_generalTask);
                _dataRepository.AddParserTask(collectionTask);
                _dataRepository.AddParserTask(filtrationTask);
                _dataRepository.AddUser(user);
                await _dataRepository.SaveChangesAsync();
                await _funnelCreatorService.SetConfigureAsync(_generalTask);
                await _dataRepository.CheckAndDeleteOldRecordsAsync();

                await _dataRepository.SaveChangesAsync();
                await _funnelCreatorService.StartTaskChainAsync();
                await _funnelCreatorService.WaitCompletionGeneralTaskAsync();
                var funnelText = _generalTask.HashtagFunnel.FunnelText;
                return Ok(funnelText);
            }
            catch (Exception ex)
            {
                _processLogger.Log(ex.ToString());
                if (_generalTask != null)
                {
                    _generalTask.ErrorInfo = ex.Message;
                    _dataRepository.UpdateGeneralTask(_generalTask);
                    await _dataRepository.SaveChangesAsync();
                }
                if (ex.Message == "paid subscription only")
                {
                    return Problem("InstaParser or ParserIm is not paid.");
                }
                return Problem(ex.Message);
            }
        }
    }
}
