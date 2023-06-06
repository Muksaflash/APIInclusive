using HashtagHelp.DAL;
using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HashtagHelp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResearchedUserController : ControllerBase
    {
        private readonly IIdGetterService _idGetterService;

        private readonly IFollowingTagsGetterService _followingTagsGetterService;

        private readonly IFunnelCreatorService _funnelCreatorService;

        private readonly IFollowersGetterService _followersGetterService;

        private readonly AppDbContext _context;

        public ResearchedUserController(AppDbContext context, IFunnelCreatorService funnelCreatedService, 
            IFollowersGetterService followersGetterService, IFollowingTagsGetterService followingTagsGetterService, 
            IIdGetterService idGetterService)
        {
            _context = context;
            _funnelCreatorService = funnelCreatedService;
            _followersGetterService = followersGetterService;
            _followingTagsGetterService = followingTagsGetterService;
            _idGetterService = idGetterService;
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
        public async Task<ActionResult<ResearchedUserEntity>> PostResearchedUser(
            ResearchedUserEntity researchedUser)
        {
            if (_context.ResearchedUsers == null)
                return Problem("Entity set 'AppDbContext.ResearchedUsers'  is null.");

            researchedUser.FollowersGetter = _followersGetterService;
            researchedUser.IdGetter = _idGetterService;
            _context.ResearchedUsers.Add(researchedUser);
            _followersGetterService.FollowingTagsGetter = _followingTagsGetterService;
            await _context.SaveChangesAsync(); 
            await _funnelCreatorService.CreateFunnelAsync(researchedUser);

            return CreatedAtAction(nameof(GetResearchedUser), 
                new { id = researchedUser.Id }, researchedUser);
        }
    } 
}
