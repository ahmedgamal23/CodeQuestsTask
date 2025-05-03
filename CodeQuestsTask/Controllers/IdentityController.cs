using AutoMapper;
using CodeQuestsTask.Application.BaseModel;
using CodeQuestsTask.Application.Interface;
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

        public IdentityController(IMapper mapper, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new BaseModel<RegisterDto> { Data = registerDto, message = "Invalid data", success = false });

            // check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if(existingUser != null)
                return BadRequest(new BaseModel<RegisterDto> { Data = registerDto, message = "Email already exists", success = false });

            ApplicationUser user = _mapper.Map<ApplicationUser>(registerDto);
            IdentityResult result =  await _userManager.CreateAsync(user, registerDto.Password);
            if(!result.Succeeded)
                return Unauthorized(new BaseModel<RegisterDto> { Data = registerDto, message = "User creation failed", success = false });

            // register success generate token (JWT)


            string token = "";
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

            string token = "";
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
