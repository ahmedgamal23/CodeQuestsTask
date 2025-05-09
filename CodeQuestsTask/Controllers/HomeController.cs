using AutoMapper;
using CodeQuestsTask.Application.BaseModel;
using CodeQuestsTask.Application.Interface;
using CodeQuestsTask.Application.Services;
using CodeQuestsTask.Domain.Models;
using CodeQuestsTask.Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeQuestsTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private readonly SaveMetaData _saveMetaData;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IUnitOfWork unitOfWork, IMapper mapper, SaveMetaData saveMetaData, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _saveMetaData = saveMetaData;
            _userManager = userManager;
        }

        [HttpGet("GetAllMatches/{pageNumber}/{pageSize}")]
        public async Task<IActionResult> Index([FromRoute]int pagesize = 10, [FromRoute] int pageNumber = 1)
        {
            // view all videos
            var matches = await _unitOfWork.MatchRepository.GetAllAsync(
                pagesize: pagesize, pageNumber: pageNumber,
                orderBy: o => o.OrderByDescending(x => x.CreatedAt),
                include: i => i.Include(x => x.User),
                filter: x => !x.IsDeleted
            );

            return Ok(matches);
        }

        [HttpGet("GetMatchById/{id}")]
        [Authorize]
        public async Task<IActionResult> GetMatch([FromRoute]int id)
        {
            var match = await _unitOfWork.MatchRepository.GetByIdAsync(id, x => !x.IsDeleted);
            if (match == null)
                return NotFound(new BaseModel<MatchPublisherDto>
                {
                    message = $"Match not found this id not exist {id}",
                    success = false
                });

            var matchDto = _mapper.Map<MatchPublisherDto>(match);
            return Ok(new BaseModel<MatchPublisherDto>
            {
                Data = matchDto,
                message = "Success",
                success = true
            });
        }

        [HttpGet("GetMatchByTitle/{title}")]
        [Authorize]
        public IActionResult GetMatchByTitle(string title, int pageNumber = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(title))
                return BadRequest(new BaseModel<MatchPublisherDto>
                {
                    message = "Title is required",
                    success = false
                });
            var matches = _unitOfWork.MatchRepository.GetByName(
                                   filter: match => match.Title.Contains(title) && !match.IsDeleted,
                                   pagesize: pageSize,
                                   pageNumber: pageNumber
                                ).Result;
            if (matches == null || matches.DataList == null || matches.DataList.Count() == 0)
                return NotFound(new BaseModel<MatchPublisherDto>
                {
                    message = $"Match not found this title not exist {title} - {matches?.message}",
                    success = matches?.success
                });
            var matchDto = _mapper.Map<IEnumerable<MatchPublisherDto>>(matches.DataList);
            return Ok(new BaseModel<MatchPublisherDto>
            {
                DataList = matchDto,
                message = "load data successfully",
                success = matches.success,
                TotalPages = matches.TotalPages,
                PageNumber = matches.PageNumber,
                PageSize = matches.PageSize
            });
        }

        [HttpGet("GetMatchByStatus/{status}")]
        [Authorize]
        public IActionResult GetMatchByStatus(string status, int pageNumber = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(status))
                return BadRequest(new BaseModel<MatchPublisherDto>
                {
                    message = "Title is required",
                    success = false
                });
            var matches = _unitOfWork.MatchRepository.GetByMatchStatus(
                            filter: match => match.MatchStatus == status && !match.IsDeleted,
                            pagesize: pageSize,
                            pageNumber: pageNumber
                            ).Result;
            if (matches == null || matches.DataList == null || matches.DataList.Count() == 0)
                return NotFound(new BaseModel<MatchPublisherDto>
                {
                    message = $"no match found with this {status} - {matches?.message}",
                    success = matches?.success
                });
            var matchDto = _mapper.Map<IEnumerable<MatchPublisherDto>>(matches.DataList);
            return Ok(new BaseModel<MatchPublisherDto>
            {
                DataList = matchDto,
                message = "load data successfully",
                success = matches.success,
                TotalPages = matches.TotalPages,
                PageNumber = matches.PageNumber,
                PageSize = matches.PageSize
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] MatchPublisherDto matchDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseModel<MatchPublisherDto>
                {
                    Data = matchDto,
                    message = "Invalid input data",
                    success = false
                });
            }

            if (matchDto.ImageFile == null || matchDto.VideoFile == null)
            {
                return BadRequest(new BaseModel<MatchPublisherDto>
                {
                    Data = matchDto,
                    message = "Image and Video files are required",
                    success = false
                });
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                string imagePath = await _saveMetaData.Save(matchDto.ImageFile, SaveMetaData.MetaDataType.Images);
                string videoPath = await _saveMetaData.Save(matchDto.VideoFile, SaveMetaData.MetaDataType.Videos);

                matchDto.ImageUrl = imagePath;
                matchDto.VideoUrl = videoPath;
                matchDto.CreatedAt = DateTime.UtcNow;

                var match = _mapper.Map<Match>(matchDto);

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new BaseModel<MatchPublisherDto>
                    {
                        message = "Unauthorized user",
                        success = false
                    });
                }

                match.CreatedById = user.Id;

                await _unitOfWork.MatchRepository.AddAsync(match);
                int rows = await _unitOfWork.SaveAsync();

                if (rows > 0)
                {
                    await _unitOfWork.CommitAsync();
                    return Ok(new BaseModel<MatchPublisherDto>
                    {
                        Data = matchDto,
                        message = "Match added successfully",
                        success = true
                    });
                }
                else
                {
                    _saveMetaData.Delete(matchDto.ImageUrl);
                    _saveMetaData.Delete(matchDto.VideoUrl);
                    await _unitOfWork.RollbackAsync();

                    return BadRequest(new BaseModel<MatchPublisherDto>
                    {
                        Data = matchDto,
                        message = "Failed to save match to the database",
                        success = false
                    });
                }
            }
            catch (Exception ex)
            {
                _saveMetaData.Delete(matchDto.ImageUrl);
                _saveMetaData.Delete(matchDto.VideoUrl);
                await _unitOfWork.RollbackAsync();
                return StatusCode(500, new BaseModel<MatchPublisherDto>
                {
                    message = "An error occurred while creating the match",
                    Exception = ex,
                    success = false
                });
            }
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] MatchPublisherDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseModel<MatchPublisherDto>
                {
                    Data = dto,
                    message = "Invalid input data",
                    success = false
                });
            }

            if (id <= 0)
            {
                return NotFound(new BaseModel<MatchPublisherDto>
                {
                    success = false,
                    message = "Invalid match ID"
                });
            }

            Match? existingMatch = await _unitOfWork.MatchRepository.GetByIdAsync(id, x => !x.IsDeleted);
            if (existingMatch == null)
            {
                return NotFound(new BaseModel<MatchPublisherDto>
                {
                    success = false,
                    message = $"No match found with ID = {id}"
                });
            }

            try
            {
                if (dto.ImageFile != null)
                {
                    string imagePath = await _saveMetaData.Save(dto.ImageFile, SaveMetaData.MetaDataType.Images);
                    _saveMetaData.Delete(existingMatch.ImageUrl);
                    existingMatch.ImageUrl = imagePath;
                }

                if (dto.VideoFile != null)
                {
                    string videoPath = await _saveMetaData.Save(dto.VideoFile, SaveMetaData.MetaDataType.Videos);
                    _saveMetaData.Delete(existingMatch.VideoUrl);
                    existingMatch.VideoUrl = videoPath;
                }

                existingMatch.Title = string.IsNullOrWhiteSpace(dto.Title) || dto.Title == "string" ? existingMatch.Title : dto.Title;
                existingMatch.Description = string.IsNullOrWhiteSpace(dto.Description) || dto.Description == "string" ? existingMatch.Description : dto.Description;
                existingMatch.MatchStatus = string.IsNullOrWhiteSpace(dto.MatchStatus) || dto.MatchStatus == "string" ? existingMatch.MatchStatus : dto.MatchStatus;

                bool updateResult = await _unitOfWork.MatchRepository.UpdateAsync(existingMatch, nameof(existingMatch.Description) );
                if (!updateResult)
                {
                    return BadRequest(new BaseModel<Match>
                    {
                        Data = existingMatch,
                        message = "Failed to update match data",
                        success = false
                    });
                }

                int rows = await _unitOfWork.SaveAsync();
                if (rows <= 0)
                {
                    return BadRequest(new BaseModel<Match>
                    {
                        Data = existingMatch,
                        message = "No changes were saved to the database",
                        success = false
                    });
                }
                return Ok(new BaseModel<Match>
                {
                    Data = existingMatch,
                    message = "Match updated successfully",
                    success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseModel<Match>
                {
                    message = "An error occurred while updating the match",
                    Exception = ex,
                    success = false
                });
            }
        }


        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (id <= 0)
                return NotFound(new BaseModel<MatchPublisherDto>
                {
                    success = false,
                    message = "Id is required"
                });
            await _unitOfWork.BeginTransactionAsync();
            bool result = await _unitOfWork.MatchRepository.SoftDeleteAsync(id);
            if (!result)
                return BadRequest(new BaseModel<Match>
                {
                    message = "Failed to delete this match",
                    success = false
                });
            int rows = await _unitOfWork.SaveAsync();
            if (rows <= 0)
            {
                await _unitOfWork.RollbackAsync();
                return BadRequest(new BaseModel<Match>
                {
                    message = "Failed to save this match",
                    success = false
                });
            }

            await _unitOfWork.CommitAsync();
            return Ok(new BaseModel<Match>
            {
                message = $"Deleted Successfully {id}",
                success = true
            });

        }

    }
}
