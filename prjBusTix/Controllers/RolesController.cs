using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using prjBusTix.Dto.Roles;
using prjBusTix.Model;

namespace prjBusTix.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]


public class RolesController : ControllerBase
{

    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ClApplicationUser> _userManager;
    private readonly ILogger<RolesController> _logger;
    
    public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ClApplicationUser> userManager, ILogger<RolesController> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto? createRoleDto)
    {
        _logger.LogInformation("CreateRole called");

        if (createRoleDto == null)
        {
            _logger.LogWarning("CreateRole received null DTO");
            return BadRequest("Datos del rol inválidos.");
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var roleName = createRoleDto.RoleName?.Trim();
        if (string.IsNullOrEmpty(roleName))
        {
            _logger.LogWarning("CreateRole: RoleName is empty");
            return BadRequest("El nombre del Rol es requerido");
        }

        if (roleName.Length > 256)
        {
            return BadRequest("El nombre del rol es demasiado largo.");
        }

        var roleExist = await _roleManager.RoleExistsAsync(roleName);
        if (roleExist)
        {
            _logger.LogInformation("CreateRole: role '{role}' already exists", roleName);
            return BadRequest("El Rol ya existe");
        }

        var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
        if (roleResult.Succeeded)
        {
            _logger.LogInformation("Role '{role}' created successfully", roleName);
            return Ok(new { message = "Rol creado con exito" });
        }

        _logger.LogError("CreateRole failed: {errors}", string.Join(';', roleResult.Errors.Select(e => e.Description)));
        return BadRequest("La creacion del Rol fallo");
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleResponseDto>>> GetRoles()
    {
        _logger.LogInformation("GetRoles called");
        var roles = await _roleManager.Roles.ToListAsync();
        var roleDtos = new List<RoleResponseDto>();

        foreach (var role in roles)
        {
            // role.Name puede ser null según anotaciones; evitar pasar null a GetUsersInRoleAsync
            var roleName = role.Name;
            int usersCount = 0;
            if (!string.IsNullOrEmpty(roleName))
            {
                var userIdsInRole = await _userManager.GetUsersInRoleAsync(roleName);
                usersCount = userIdsInRole.Count;
            }

            roleDtos.Add(new RoleResponseDto
            {
                Id = role.Id,
                Name = role.Name,
                TotalUsers = usersCount
            });
        }
        return Ok(roleDtos);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        _logger.LogInformation("DeleteRole called for id {id}", id);
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            _logger.LogWarning("DeleteRole: role not found {id}", id);
            return NotFound("El Rol no fue encontrado ");
        }
        var result = await _roleManager.DeleteAsync(role);
        if (result.Succeeded)
        {
            _logger.LogInformation("DeleteRole: role {id} deleted", id);
            return Ok(new { message = "Rol elimimnado correctamente" });
        }
        _logger.LogError("DeleteRole failed for {id}: {errors}", id, string.Join(';', result.Errors.Select(e => e.Description)));
        return BadRequest("El Rol no se pudo eliminar correctamente");
    }
    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole([FromBody] RolesAssignDto? rolesAssignDto)
     {
        if (rolesAssignDto == null)
        {
            _logger.LogWarning("AssignRole received null DTO");
            return BadRequest("Datos de asignación inválidos.");
        }

        var user = await _userManager.FindByIdAsync(rolesAssignDto.UserId);
        if (user is null)
        {
            return NotFound("El usuario no fue encontrado");
        }
        var role = await _roleManager.FindByNameAsync(rolesAssignDto.RoleId);
        if (role is null)
        {
            return NotFound("El Rol no fue encontrado");
        }

        var roleName = role.Name;
        if (string.IsNullOrEmpty(roleName))
            return BadRequest("El nombre del rol es inválido.");

        var result = await _userManager.AddToRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            return Ok(new { message = "Rol asignado correctamente" });
        }

        var error = result.Errors.FirstOrDefault();
        var message = error != null ? error.Description : "No se pudo asignar el rol";
        return BadRequest(new { message });
     }
 }
