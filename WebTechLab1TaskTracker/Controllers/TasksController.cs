using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using WebTechLab1TaskTracker.Data;
using WebTechLab1TaskTracker.Models;
using WebTechLab1TaskTracker.Services;

namespace WebTechLab1TaskTracker.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly TaskTrackerDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public TasksController(TaskTrackerDbContext context, UserManager<ApplicationUser> userManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var userTasks = await _context.Tasks
                .Where(t => t.ApplicationUserId == currentUserId)
                .Include(t => t.Project)
                .ToListAsync();
            return View(userTasks);
        }

        // GET: Tasks/Details/5 (з перевіркою безпеки)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.ApplicationUser)
                .Include(t => t.Comments).ThenInclude(c => c.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (task == null || task.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            var viewModel = new TaskDetailViewModel
            {
                Task = task,
                Comments = task.Comments.OrderByDescending(c => c.CreatedAt)
            };
            return View(viewModel);
        }

        // GET: Tasks/Create
        public async Task<IActionResult> Create(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null || project.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }
            

            ViewBag.StatusList = new SelectList(new List<string> { "To Do", "In Progress", "Done" });
            ViewData["ProjectId"] = projectId;
            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Deadline,Status")] Models.Task task, int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null || project.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }
            var assignedUser = await _userManager.FindByIdAsync(task.ApplicationUserId);
            if (assignedUser?.TelegramChatId != null)
            {
                var message = $"Hello, {assignedUser.UserName}! A new task has been assigned to you:\n\n*Task:* {task.Title}\n*Project:* {(await _context.Projects.FindAsync(task.ProjectId))?.Name}";
                await _notificationService.SendNotificationAsync(assignedUser.TelegramChatId.Value, message);
            }
            task.ProjectId = projectId;
                task.ApplicationUserId = _userManager.GetUserId(User);
                task.CreatedAt = DateTime.Now;
                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Projects", new { id = projectId });
            ViewBag.StatusList = new SelectList(new List<string> { "To Do", "In Progress", "Done" });
            return View(task);
        }

        // POST: Tasks/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int taskId, string newCommentText)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null || task.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (!string.IsNullOrWhiteSpace(newCommentText))
            {
                var comment = new Comment
                {
                    Content = newCommentText, // <-- ВИПРАВЛЕНО: Content замінено на Text
                    TaskId = taskId,
                    ApplicationUserId = _userManager.GetUserId(User),
                    CreatedAt = DateTime.Now
                };
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = taskId });
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            ViewBag.ProjectId = new SelectList(_context.Projects, "Id", "Name", task.ProjectId);
            ViewBag.ApplicationUserId = new SelectList(_context.Users, "Id", "UserName", task.ApplicationUserId);

            ViewBag.StatusList = new SelectList(new List<string> { "To Do", "In Progress", "Done" }, task.Status);
            return View(task);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CreatedAt,Deadline,Status,ProjectId,ApplicationUserId")] Models.Task task)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            var originalTask = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (originalTask == null || originalTask.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Tasks", new { id = task.Id });
            }

            ViewBag.ProjectId = new SelectList(_context.Projects, "Id", "Name", task.ProjectId);
            ViewBag.ApplicationUserId = new SelectList(_userManager.Users, "Id", "UserName", task.ApplicationUserId);
            ViewBag.StatusList = new SelectList(new List<string> { "To Do", "In Progress", "Done" }, task.Status);
            // --------------------------------------------------------------------------

            return View(task);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks.Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (task == null || task.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            return View(task);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null && task.ApplicationUserId == _userManager.GetUserId(User))
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tasks", new { id = task.Id });
            }
            return Forbid();
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}