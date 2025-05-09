using AutoMapper;
using CodeQuestsTask.Application.BaseModel;
using CodeQuestsTask.Application.Interface;
using CodeQuestsTask.Application.Services;
using CodeQuestsTask.Domain.Models;
using CodeQuestsTask.Domain.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuestsTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private IMapper _mapper;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private IConfiguration _configuration;
        public IdentityController(IMapper mapper, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new BaseModel<RegisterDto> { Data = registerDto, message = "Invalid data", success = false });

            if (registerDto.Password != registerDto.ConfirmPassword)
                return BadRequest(new BaseModel<RegisterDto> { Data = registerDto, message = "Invalid password data", success = false });

            // check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if(existingUser != null)
                return BadRequest(new BaseModel<RegisterDto> { Data = registerDto, message = "Email already exists", success = false });

            ApplicationUser user = _mapper.Map<ApplicationUser>(registerDto);
            user.UserName = registerDto.Email;
            IdentityResult result =  await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return Unauthorized(new BaseModel<RegisterDto> { Data = registerDto, ModelState = ModelState, success = false });
            }

            // generate Token for login  (JWT)
            string token = JWTGenerateToken.GenerateToken(_configuration, user);
            registerDto.Id = user.Id;
            BaseModel<RegisterDto> model = new BaseModel<RegisterDto>
            {
                Data = registerDto,
                message = "User created successfully",
                success = result.Succeeded,
                Token = token
            };
            return Ok(model);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if(!ModelState.IsValid)
                return BadRequest(new BaseModel<LoginDto> { Data = loginDto, message = "Invalid data", success = false });

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return NotFound(new BaseModel<LoginDto> { Data = loginDto, message = "User not found", success = false });
            var result =  await _signInManager.PasswordSignInAsync(user, loginDto.Password, isPersistent: loginDto.RememberMe, lockoutOnFailure: false);
            if(!result.Succeeded)
                return Unauthorized(new BaseModel<LoginDto> { Data = loginDto, message = "Invalid password", success = false });

            // generate Token for login  (JWT)
            string token = JWTGenerateToken.GenerateToken(_configuration, user);
            loginDto.Id = user.Id;
            return Ok(new BaseModel<LoginDto>
            {
                Data = loginDto,
                message = "Login successful",
                success = result.Succeeded,
                Token = token
            });
        }

    }
}
