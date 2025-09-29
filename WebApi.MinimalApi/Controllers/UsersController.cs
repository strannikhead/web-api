using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    private readonly IUserRepository userRepository;
    private readonly IMapper mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        this.userRepository = userRepository;
        this.mapper = mapper;
    }

    [HttpGet("{userId}", Name = nameof(GetUserById))]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = userRepository.FindById(userId);

        if (user == null)
        {
            return NotFound();
        }

        var result = mapper.Map<UserDto>(user);

        return Ok(result);
    }

    [HttpPost]
    [Produces("application/json", "application/xml")]
    public IActionResult CreateUser([FromBody] CreateUserDto? user)
    {
        if (user is null) return BadRequest();

        if (string.IsNullOrWhiteSpace(user.Login))
            ModelState.AddModelError("Login", "Login is required.");
        if (!string.IsNullOrEmpty(user.Login) && !user.Login.All(char.IsLetterOrDigit))
            ModelState.AddModelError("Login", "Login must contain only letters and digits.");

        if (!ModelState.IsValid)
        {
            var errors = new
            {
                login = ModelState.ContainsKey("Login")
                    ? ModelState["Login"].Errors.Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage)
                    : Enumerable.Empty<string>()
            };
            return StatusCode(422, errors);
        }

        var entity = mapper.Map<UserEntity>(user);
        userRepository.Insert(entity);

        return CreatedAtRoute(
            routeName: nameof(GetUserById),
            routeValues: new { userId = entity.Id },
            value: entity.Id
        );
    }
}