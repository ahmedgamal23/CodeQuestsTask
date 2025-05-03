using AutoMapper;
using CodeQuestsTask.Application.BaseModel;
using CodeQuestsTask.Application.Interface;
using CodeQuestsTask.Domain.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CodeQuestsTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        public HomeController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pagesize = 10, int pageNumber = 1)
        {
            // view all videos
            var matches = await _unitOfWork.MatchRepository.GetAllAsync(
                pagesize:pagesize, pageNumber: pageNumber,
                orderBy: o => o.OrderByDescending(x=> x.CreatedAt),
                include: i => i.Include(x => x.User)
            );

            BaseModel<MatchPublisherDto> model = new BaseModel<MatchPublisherDto>
            {
                DataList = _mapper.Map<IEnumerable<MatchPublisherDto>>(matches),
                message = matches != null ? "Success" : "Falier",
                success = matches != null ? true : false,
                PageSize = pagesize,
                PageNumber = pageNumber
            };

            return Ok(model);
        }


    }
}
