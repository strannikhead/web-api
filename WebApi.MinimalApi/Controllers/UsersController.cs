using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
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
    private readonly ILogger<UsersController> logger;

    public UsersController(IUserRepository userRepository, IMapper mapper, ILogger<UsersController> logger)
    {
        this.userRepository = userRepository;
        this.mapper = mapper;
        this.logger = logger;
    }

    [HttpGet("{userId:guid}", Name = nameof(GetUserById))]
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
        if (user is null)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(user.Login))
            ModelState.AddModelError("Login", "Login is required.");
        if (!string.IsNullOrEmpty(user.Login) && !user.Login.All(char.IsLetterOrDigit))
            ModelState.AddModelError("Login", "Login must contain only letters and digits.");

        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        var entity = mapper.Map<UserEntity>(user);
        var created = userRepository.Insert(entity);

        return CreatedAtRoute(
            routeName: nameof(GetUserById),
            routeValues: new { userId = created.Id },
            value: created.Id
        );
    }

    [HttpPut("{userId}")]
    [Produces("application/json", "application/xml")]
    public IActionResult UpdateUser([FromRoute] string userId, [FromBody] UpdateUserDto? user)
    {
        if (!Guid.TryParse(userId, out var id) || user is null)
            return BadRequest();

        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        var entity = userRepository.FindById(id);
        if (entity is null)
            return CreateUser(mapper.Map<CreateUserDto>(user));

        mapper.Map(user, entity);
        userRepository.Update(entity);

        return NoContent();
    }

    [HttpPatch("{userId}")]
    [Consumes("application/json-patch+json")]
    [Produces("application/json", "application/xml")]
    public IActionResult PartiallyUpdateUser([FromBody] JsonPatchDocument<UpdateUserDto>? patchDocument, [FromRoute] string userId)
    {
        if (patchDocument is null)
            return BadRequest();

        if (!Guid.TryParse(userId, out var id))
            return NotFound();

        var entity = userRepository.FindById(id);
        if (entity is null)
            return NotFound();

        var modelToPatch = mapper.Map<UpdateUserDto>(entity);

        patchDocument.ApplyTo(modelToPatch, ModelState);
        TryValidateModel(modelToPatch);
        
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        mapper.Map(modelToPatch, entity);
        userRepository.Update(entity);

        return NoContent();
    }
}