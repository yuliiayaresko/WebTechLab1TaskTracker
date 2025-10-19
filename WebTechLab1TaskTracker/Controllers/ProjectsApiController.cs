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
    public class ProjectsController : ControllerBase
    {
        private readonly TaskTrackerDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProjectsController(TaskTrackerDbContext context)
        {
            _context = context;
            
        }
        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            var projects = await _context.Projects
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    TaskCount = p.Tasks.Count()
                }).ToListAsync();
            return Ok(projects);
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            var project = await _context.Projects
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    TaskCount = p.Tasks.Count()
                }).FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();
            return Ok(project);
        }

        // POST: api/Projects
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> PostProject(CreateProjectDto createDto)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == createDto.ApplicationUserId);
            if (!userExists) return BadRequest("User with the specified ID does not exist.");

            var project = new Project
            {
                Name = createDto.Name,
                Description = createDto.Description,
                ApplicationUserId = createDto.ApplicationUserId 
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var projectDto = new ProjectDto { Id = project.Id, Name = project.Name, Description = project.Description, TaskCount = 0 };
            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, projectDto);
        }

        // PUT: api/Projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, CreateProjectDto updateDto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            project.Name = updateDto.Name;
            project.Description = updateDto.Description;

            await _context.SaveChangesAsync();
            return NoContent();
        }



        // DELETE: api/projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            
            _context.Tasks.RemoveRange(project.Tasks);

           
            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}