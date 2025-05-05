using AutoMapper;
using CodeQuestsTask.Application.BaseModel;
using CodeQuestsTask.Application.Interface;
using CodeQuestsTask.Domain.Models;
using CodeQuestsTask.Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeQuestsTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public PlaylistController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("{pageNumber}/{pageSize}")]
        [Authorize]
        public async Task<IActionResult> GetUserPlaylist([FromHeader] string userId, [FromRoute] int pageNumber=1, [FromRoute] int pageSize=10)
        {
            if (userId == null)
                return Unauthorized(new BaseModel<Playlist>
                {
                    success = false,
                    message = "Not Authorized"
                });

            var playlists = await _unitOfWork.PlaylistRepository.GetAllAsync(
                pagesize:pageSize,
                pageNumber:pageNumber,
                filter: x => x.UserId == userId && !x.Match!.IsDeleted,
                include: x => x.Include(u => u.User).Include(m => m.Match)
            );

            if(playlists == null)
                return NotFound(new BaseModel<Playlist>
                {
                    success = false,
                    message = "playlist is empty"
                });

            var userMatchViewDtos = _mapper.Map<IEnumerable<UserMatchViewDto>>(playlists);
            return Ok(new BaseModel<UserMatchViewDto>
            {
                success = true,
                message = "successfully",
                DataList = userMatchViewDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToPlaylist([FromBody] UserMatchDto userMatchDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new BaseModel<UserMatchDto>
                {
                    success = false,
                    Data = userMatchDto,
                    message = "incorrect data"
                });

            var playlist = _mapper.Map<Playlist>(userMatchDto);
            var result = await _unitOfWork.PlaylistRepository.
                                    GetAllAsync(filter:  x => x.UserId == playlist.UserId && x.MatchId == playlist.MatchId);
            if(result.Any())
                return Ok(new BaseModel<Playlist>
                {
                    success = true,
                    message = "this match already exist!",
                    Data = playlist
                });

            await _unitOfWork.PlaylistRepository.AddAsync(playlist);
            int rows = await _unitOfWork.SaveAsync();
            if (rows <= 0)
                return BadRequest(new BaseModel<Playlist>
                {
                    success = false,
                    Data = playlist,
                    message = "cann't add this match to playlist"
                });
            return Ok(new BaseModel<Playlist>
            {
                success = true,
                message = "successfully added to your playlist",
                Data = playlist
            });
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete([FromBody] UserMatchDto userMatchDto)
        {
            // remove match from playlist
            if (!ModelState.IsValid)
                return BadRequest(new BaseModel<UserMatchDto>
                {
                    success = false,
                    Data = userMatchDto,
                    message = "incorrect data"
                });

            IEnumerable<Playlist> playlist = await _unitOfWork.PlaylistRepository.GetAllAsync
                                    (filter: x => x.UserId == userMatchDto.UserId && x.MatchId == userMatchDto.MatchId);

            var result = await _unitOfWork.PlaylistRepository.DeleteAsync(playlist.First().Id);
            int row = await _unitOfWork.SaveAsync();
            if (result == false || row <= 0)
                return BadRequest(new BaseModel<Playlist>
                {
                    success = false,
                    message = "cann't remove this match from playlist"
                });
            return Ok(new BaseModel<Playlist>
            {
                success = true,
                message = "successfully removed from your playlist",
            });
        }


    }
}
