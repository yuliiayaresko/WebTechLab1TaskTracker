using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTechLab1TaskTracker.Data;
using WebTechLab1TaskTracker.DTOs;
using WebTechLab1TaskTracker.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Azure.Search.Documents;
using Azure;
using Azure.Search.Documents.Models;

namespace WebTechLab1TaskTracker.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly TaskTrackerDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; 
        private readonly SearchClient _searchClient;

        
        public ProjectsController(TaskTrackerDbContext context, IConfiguration configuration)
        {
            _context = context;

            
            Uri serviceUrl = new Uri(configuration["AzureSearch:ServiceUrl"]);
            AzureKeyCredential credential = new AzureKeyCredential(configuration["AzureSearch:AdminKey"]);

           
            _searchClient = new SearchClient(serviceUrl, "projects-index", credential);
        }

       
        [HttpGet("search")]
        public async Task<IActionResult> SearchProjects([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty.");
            }

            try
            {

               
                SearchResults<SearchDocument> results = await _searchClient.SearchAsync<SearchDocument>(query);

                
                var projectsDto = new List<ProjectDto>();

               
                await foreach (SearchResult<SearchDocument> result in results.GetResultsAsync())
                {
                   
                    int.TryParse(result.Document["Id"].ToString(), out int projectId);

                    projectsDto.Add(new ProjectDto
                    {
                        Id = projectId, 
                        Name = result.Document["Name"].ToString(),
                        Description = result.Document["Description"].ToString(),
                        TaskCount = 0
                    });
                }

                

                return Ok(projectsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error searching: {ex.Message}");
            }
        }


        [HttpGet]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetProjects([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var totalRecords = await _context.Projects.CountAsync();

            var pagedDataQuery = _context.Projects
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var projectsDto = await pagedDataQuery
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    TaskCount = p.Tasks.Count()
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var baseUri = $"{Request.Scheme}://{Request.Host}{Request.Path}";

            string nextPage = null;
            if (pageNumber < totalPages)
            {
                nextPage = $"{baseUri}?pageNumber={pageNumber + 1}&pageSize={pageSize}";
            }

            string previousPage = null;
            if (pageNumber > 1)
            {
                previousPage = $"{baseUri}?pageNumber={pageNumber - 1}&pageSize={pageSize}";
            }

            var response = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                NextPage = nextPage,
                PreviousPage = previousPage,
                Data = projectsDto
            };

            return Ok(response);
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