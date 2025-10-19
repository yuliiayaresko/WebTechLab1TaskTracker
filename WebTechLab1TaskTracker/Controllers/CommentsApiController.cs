using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebTechLab1TaskTracker.Data;
using WebTechLab1TaskTracker.DTOs;
using WebTechLab1TaskTracker.Models;

namespace WebTechLab1TaskTracker.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly TaskTrackerDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(TaskTrackerDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Comments/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetAllComments()
        {
            var allComments = await _context.Comments
                .Include(c => c.ApplicationUser) 
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content, 
                    CreatedAt = c.CreatedAt,
                    TaskId = c.TaskId,
                    AuthorUserName = c.ApplicationUser.UserName
                })
                .ToListAsync();

            return Ok(allComments);
        }

        // GET: api/Comments?taskId=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments([FromQuery] int taskId)
        {
            var comments = await _context.Comments
                .Where(c => c.TaskId == taskId)
                .Select(c => new CommentDto 
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    TaskId = c.TaskId,
                   
                })
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/Comments
        [HttpPost]
        public async Task<ActionResult<CommentDto>> PostComment(CreateCommentDto createDto)
        {
            var taskExists = await _context.Tasks.AnyAsync(t => t.Id == createDto.TaskId);
            if (!taskExists) return BadRequest("Task with this ID does not exist.");

            var user = await _context.Users.FindAsync(createDto.ApplicationUserId);
            if (user == null) return BadRequest("User with the specified ID does not exist.");

            var comment = new Comment
            {
                Content = createDto.Content,
                TaskId = createDto.TaskId,
                CreatedAt = DateTime.UtcNow,
                ApplicationUserId = createDto.ApplicationUserId 
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var commentDto = new CommentDto { Id = comment.Id, Content = comment.Content, CreatedAt = comment.CreatedAt, TaskId = comment.TaskId, AuthorUserName = user.UserName };
            return CreatedAtAction(nameof(GetComments), new { taskId = comment.TaskId }, commentDto);
        }

        // PUT: api/comments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(int id, [FromBody] UpdateCommentDto updateDto)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }


            comment.Content = updateDto.Content;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();


            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}