using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTechLab1TaskTracker.Data;
using WebTechLab1TaskTracker.DTOs;
using WebTechLab1TaskTracker.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims; 


namespace WebTechLab1TaskTracker.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskTrackerDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(TaskTrackerDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager; // <-- Ініціалізація
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks([FromQuery] int? projectId)
        {
            var query = _context.Tasks.AsQueryable();

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            var tasks = await query.Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Deadline = t.Deadline,
                ProjectId = t.ProjectId
            }).ToListAsync();

            return Ok(tasks);
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            var task = await _context.Tasks
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Deadline = t.Deadline,
                    Status = t.Status,
                    ProjectId = t.ProjectId
                }).FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();
            return Ok(task);
        }

        // POST: api/Tasks
        [HttpPost]
        public async Task<ActionResult<TaskDto>> PostTask(CreateTaskDto createDto)
        {
            var projectExists = await _context.Projects.AnyAsync(p => p.Id == createDto.ProjectId);
            if (!projectExists) return BadRequest("Project with this ID does not exist.");

            var userExists = await _context.Users.AnyAsync(u => u.Id == createDto.ApplicationUserId);
            if (!userExists) return BadRequest("User with the specified ID does not exist.");

            var task = new WebTechLab1TaskTracker.Models.Task
            {
                Title = createDto.Title,
                Description = createDto.Description,
                Deadline = createDto.Deadline,
                Status = createDto.Status,
                ProjectId = createDto.ProjectId,
                ApplicationUserId = createDto.ApplicationUserId 
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var taskDto = new TaskDto { Id = task.Id, Title = task.Title, Description = task.Description, Deadline = task.Deadline, Status = task.Status, ProjectId = task.ProjectId };
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, taskDto);
        }

        // PUT: api/Tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, CreateTaskDto updateDto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            task.Title = updateDto.Title;
            task.Description = updateDto.Description;
            task.Status = updateDto.Status;
            task.ProjectId = updateDto.ProjectId; 

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}