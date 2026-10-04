using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using api.Models;
using api.Services;
using api.Data;


namespace api.Controllers;

[ApiController]

[Route("api/[controller]")]
[Authorize(Roles = "Admin,Supervisor")]

public class UserController: ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AuditService _audit;

    public UserController(AppDbContext context, AuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.Include(u => u.branch)
            .Select(u => new
            {
                u.id,
                u.full_name,
                u.email,
                u.counter,
                u.role,
                u.branch_id,
                branch_name = u.branch != null ? u.branch.branch_name : null,
                status = u.status,
                created_at = u.created_at
            })
            .ToListAsync();



        return Ok(users);
    }

    [HttpGet("{id}")]

    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.branch)
            .FirstOrDefaultAsync(u => u.id == id);

        if(user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            user.id,
            user.full_name,
            user.email,
            user.counter,
            user.role,
            user.branch_id,
            branch_name = user.branch != null ? user.branch.branch_name : null,
            status = user.status,
            created_at = user.created_at
        });


    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(User user)
    {
        if(user.role != UserRole.Admin && user.branch_id == null)
        {
            return BadRequest("The specified role should have a valid branch");
        }

        if (user.role == UserRole.Teller && string.IsNullOrEmpty(user.counter))
        {
            return BadRequest("A Teller should have a valid counter");
        }



        if (user.branch_id != null)
        {
            var branch = await _context.Branches.FindAsync(user.branch_id);
            {
                if (branch == null)
                {
                    return BadRequest("The provided branch is not valid");
                }
            }
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.email == user.email);

        if (existingUser != null)
        {
            return BadRequest("Email already exist");
        }

        var passwordHasher = new PasswordHasher<User>();

        user.password = passwordHasher.HashPassword(user, user.password);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        await _audit.Log(
    "CREATE",
    "User",
    user.id,
    $"Created user {user.full_name}",
    user.branch_id
);

        //return Ok(user);
        return Ok(new
        {
            user.id,
            user.full_name,
            user.email,
            user.counter,
            user.role,
            user.branch_id,
            user.status,
            user.created_at
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
        var user = await _context.Users.FindAsync(id);
            if (user == null)
        {
            return NotFound("The user does not not exist");
        }

        if (user.role != UserRole.Admin && updatedUser.branch_id == null)
        {
            return BadRequest("The selected Role should have a valid branch");

        }
        if (user.role == UserRole.Teller && string.IsNullOrEmpty(updatedUser.counter))
        {
            return BadRequest("A Teller should have a valid counter");
        }
        if (updatedUser.branch_id != null)
        {
            var branch = await _context.Branches.FindAsync(updatedUser.branch_id);

            if (branch == null)
            {
                return BadRequest("The provided branch is not valid");
            }
        }
        if (!string.IsNullOrEmpty(updatedUser.password))
        {
            var passwordHasher = new PasswordHasher<User>();
            user.password = passwordHasher.HashPassword(user, updatedUser.password);
        }

        user.full_name = updatedUser.full_name;
        user.email = updatedUser.email;

        if (!string.IsNullOrEmpty(updatedUser.password))
        {
            var passwordHasher = new PasswordHasher<User>();
            user.password = passwordHasher.HashPassword(user, updatedUser.password);
        }
        user.role = updatedUser.role;
        user.counter = updatedUser.counter;
        user.branch_id = updatedUser.branch_id;

        await _context.SaveChangesAsync();

        await _audit.Log(
            "UPDATE",
            "User",
            user.id,
            $"Updated user {user.full_name}",
            user.branch_id
        );

        return Ok(user);

    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchUser(int id, User updatedUser)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }
        if(updatedUser.role != UserRole.Admin && updatedUser.branch_id == null)
        {
            return BadRequest("The Specified role should have a valide branch");
        }
        if(updatedUser.role == UserRole.Teller && string.IsNullOrEmpty(updatedUser.counter))
            {
            return BadRequest("The specified role should have a valid counter");
        }

        if(updatedUser.branch_id != null)
        {
            var branch = await _context.Branches.FindAsync(updatedUser.branch_id);

            if (branch == null)
            {
                return BadRequest("The specifed Branch is not valid");
            }

            user.branch_id = updatedUser.branch_id;
        }
        
        if(!string.IsNullOrEmpty(updatedUser.full_name))
        {
            user.full_name = updatedUser.full_name;
        }

        if(!string.IsNullOrEmpty(updatedUser.email))
        {
            user.email = updatedUser.email;
        }

        if (!string.IsNullOrEmpty(updatedUser.password))
        {
            var passwordHasher = new PasswordHasher<User>();
            user.password = passwordHasher.HashPassword(user, updatedUser.password);
        }
        if (!string.IsNullOrEmpty(updatedUser.counter))
        {
            user.counter = updatedUser.counter;
        }

        user.role = updatedUser.role;
        user.status = updatedUser.status;

        await _context.SaveChangesAsync();

        var updated = await _context.Users
            .Include(u => u.branch)
            .FirstOrDefaultAsync(u => u.id == id);

        return Ok(new
        {
            updated.id,
            updated.full_name,
            updated.email,
            updated.counter,
            updated.role,
            updated.branch_id,
            branch_name = updated.branch != null ? updated.branch.branch_name : null,
            updated.status,
            updated.created_at
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();

        await _audit.Log(
            "DELETE",
            "User",
            user.id,
            $"Deleted user {user.full_name}",
            user.branch_id
        );

        return NoContent();
    }
}