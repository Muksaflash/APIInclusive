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

        private GoogleSheetsFunnelTaskEntity? _googleSheetsFunnelTask;

        private FunnelServiceInfoEntity? _funnelServiceInfo;

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

        [HttpPost("googleSheetsUpload")]
        public async Task<ActionResult<string>> GoogleSheetsUpload([FromBody] GoogleSheetsFunnelRequestModel requestData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _funnelServiceInfo = new FunnelServiceInfoEntity();
                Guid guidResult = Guid.NewGuid();/* 
                if (!Guid.TryParse(requestData.Id, out guidResult))
                    throw new Exception("Id cannot represents a globally unique identifier (GUID)");
 */                _funnelServiceInfo.Id = guidResult;
                _dataRepository.AddFunnelServiceInfo(_funnelServiceInfo);
                await _dataRepository.SaveChangesAsync();

                _googleSheetsFunnelTask = new GoogleSheetsFunnelTaskEntity
                {
                    SemiAreasSheetName = requestData.SemiAreasSheetName,
                    TableId = requestData.TableName,
                    ParsedSheetName = requestData.ParsedSheetName,
                    MinHashtagFollowers = requestData.MinHashtagFollowers,
                    AreaSheetName = requestData.AreaSheetName,
                    OutputGoogleSheet = requestData.OutputGoogleSheet
                };
                await _funnelCreatorService.SetGoogleSheetsFunnelConfigureAsync(_googleSheetsFunnelTask, _funnelServiceInfo);
                await _funnelCreatorService.StartTaskChainAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Exception: {ex.Message}");
            }
        }

        [HttpGet("info/{Id}")]
        public ActionResult<string> GetFunnelServiceInfo(string Id)
        {
            try
            {
                var result = _dataRepository.GetFunnelServiceInfoEntityById(Id);
                if (result == null)
                    return Problem("Funnel Service Info does not exist");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult<string> GetGeneralTasks()
        {
            var result = _dataRepository.GetGeneralTaskEntities();
            if (result == null) return Problem("General tasks are not exist");
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<string>> InitiateFunnelTask([FromBody] FunnelRequestModel requestData)
        {
            try
            {
                // добавить Фуннел сервис инфо
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
                _generalTask.HashtagSemiAreas = string.Join(", ", requestData.HashtagSemiAreas);
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
