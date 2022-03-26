using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ClinicSystem.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Clinic.Authentication
{

  [Route("api/authentication")]
  [ApiController]
  public class AuthenticationController : ControllerBase
  {
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly IConfiguration _configuration;

    public AuthenticationController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
      this.userManager = userManager;
      this.roleManager = roleManager;
      _configuration = configuration;
    }


//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------


    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
      var user = await userManager.FindByNameAsync(model.Username);
      if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
      {
        var userRoles = await userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
                  {
                      new Claim(ClaimTypes.Name, user.UserName),
                      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                  };

        foreach (var userRole in userRoles)
        {
          authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

        return Ok(new
        {
          token = new JwtSecurityTokenHandler().WriteToken(token),
          expiration = token.ValidTo
        });
      }
      return Unauthorized();
    }


//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------


    [HttpPost]
    [Route("register/patient")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
      var userExists = await userManager.FindByNameAsync(model.Username);
      if (userExists != null)
        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

      ApplicationUser user = new ApplicationUser()
      {
        Email = model.Email,
        SecurityStamp = Guid.NewGuid().ToString(),
        UserName = model.Username
      };
      var result = await userManager.CreateAsync(user, model.Password);
      if (!result.Succeeded)
        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

      if (!await roleManager.RoleExistsAsync(UserRoles.Patient))
        await roleManager.CreateAsync(new IdentityRole(UserRoles.Patient));
      if (!await roleManager.RoleExistsAsync(UserRoles.ClinicAdmin))
        await roleManager.CreateAsync(new IdentityRole(UserRoles.ClinicAdmin));
      if (!await roleManager.RoleExistsAsync(UserRoles.Doctor))
        await roleManager.CreateAsync(new IdentityRole(UserRoles.Doctor));

      if (await roleManager.RoleExistsAsync(UserRoles.Patient))
      {
        await userManager.AddToRoleAsync(user, UserRoles.Patient);
      }

      return Ok(new Response { Status = "Success", Message = "User created successfully!" });
    }


//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------


    [HttpPost]
    [Route("register/doctor")]
    public async Task<IActionResult> RegisterDoctor([FromBody] RegisterModel model)
    {
      var userExists = await userManager.FindByNameAsync(model.Username);
      if (userExists != null)
        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

      ApplicationUser user = new ApplicationUser()
      {
        Email = model.Email,
        SecurityStamp = Guid.NewGuid().ToString(),
        UserName = model.Username
      };
      var result = await userManager.CreateAsync(user, model.Password);
      if (!result.Succeeded)
        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

      if (!await roleManager.RoleExistsAsync(UserRoles.Doctor))
        await roleManager.CreateAsync(new IdentityRole(UserRoles.Doctor));
      if (!await roleManager.RoleExistsAsync(UserRoles.ClinicAdmin))
        await roleManager.CreateAsync(new IdentityRole(UserRoles.ClinicAdmin));
      if (!await roleManager.RoleExistsAsync(UserRoles.Patient))
        await roleManager.CreateAsync(new IdentityRole(UserRoles.Patient));

      if (await roleManager.RoleExistsAsync(UserRoles.Doctor))
      {
        await userManager.AddToRoleAsync(user, UserRoles.Doctor);
      }

      return Ok(new Response { Status = "Success", Message = "User created successfully!" });
    }


//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------


    [HttpPost]
    [Route("register/clinic-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
    {
      var userExists = await userManager.FindByNameAsync(model.Username);
      if (userExists != null)
        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

      ApplicationUser user = new ApplicationUser()
      {
        Email = model.Email,
        SecurityStamp = Guid.NewGuid().ToString(),
        UserName = model.Username
      };
      var result = await userManager.CreateAsync(user, model.Password);
      if (!result.Succeeded)
        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

      if (!await roleManager.RoleExistsAsync(UserRoles.ClinicAdmin))
        await roleManager.CreateAsync(new IdentityRole(UserRoles.ClinicAdmin));
      if (!await roleManager.RoleExistsAsync(UserRoles.Doctor))
        await roleManager.CreateAsync(new IdentityRole(UserRoles.Doctor));
      if (!await roleManager.RoleExistsAsync(UserRoles.Patient))
        await roleManager.CreateAsync(new IdentityRole(UserRoles.Patient));

      if (await roleManager.RoleExistsAsync(UserRoles.ClinicAdmin))
      {
        await userManager.AddToRoleAsync(user, UserRoles.ClinicAdmin);
      }

      return Ok(new Response { Status = "Success", Message = "User created successfully!" });
    }
  }
}
