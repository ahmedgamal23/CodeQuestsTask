﻿using AutoMapper;
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
        public async Task<IActionResult> GetUserPlaylist([FromQuery] string userId, [FromRoute] int pageNumber=1, [FromRoute] int pageSize=10)
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

            var userMatchViewDtos = _mapper.Map<IEnumerable<UserMatchViewDto>>(playlists.DataList);
            return Ok(new BaseModel<UserMatchViewDto>
            {                
                DataList = userMatchViewDtos,
                message = playlists != null ? "Success" : "Falier",
                success = playlists?.success,
                PageSize = playlists?.PageSize,
                PageNumber = playlists?.PageNumber,
                TotalPages = playlists?.TotalPages,
                Exception = playlists?.Exception,
                ModelState = playlists?.ModelState
            });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CheckMatchInPlaylist([FromQuery] string userId, [FromQuery] int matchId)
        {
            if (string.IsNullOrEmpty(userId) || matchId == 0)
            {
                return Unauthorized(new BaseModel<Playlist>
                {
                    success = false,
                    message = "Invalid user or match ID"
                });
            }

            var playlist = await _unitOfWork.PlaylistRepository.GetAllAsync(
                filter: x => x.UserId == userId && x.MatchId == matchId
            );

            if (playlist == null || playlist.DataList?.Count() == 0)
                return Ok(new BaseModel<Playlist>
                {
                    success = false,
                    message = "playlist is empty"
                });

            return Ok(new BaseModel<Playlist>
            {
                success = true,
                message = "this match is exist in your playlist"
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
            if(result.DataList!.Any())
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

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            // remove match from playlist
            if (!ModelState.IsValid)
                return BadRequest(new BaseModel<UserMatchDto>
                {
                    success = false,
                    message = "incorrect data"
                });

            var result = await _unitOfWork.PlaylistRepository.DeleteAsync(id);
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
