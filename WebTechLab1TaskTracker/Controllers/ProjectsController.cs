using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTechLab1TaskTracker.Data;
using WebTechLab1TaskTracker.Models;

namespace WebTechLab1TaskTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly TaskTrackerDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public ProjectsController(TaskTrackerDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Projects 
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account"); 
            }
            var currentUserId = _userManager.GetUserId(User);
            var userProjects = await _context.Projects
                .Where(p => p.ApplicationUserId == currentUserId)
                .ToListAsync();
            return View(userProjects);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.ApplicationUser)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null || project.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project, IFormFile? ImageFile)
        {
            if (ImageFile != null)
            {
                var fileName = Path.GetFileName(ImageFile.FileName);
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                project.ImagePath = "/uploads/" + fileName;
            }

            project.ApplicationUserId = _userManager.GetUserId(User);
            _context.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var project = await _context.Projects.FindAsync(id);
            if (project == null || project.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Project editedProject)
        {
            if (id != editedProject.Id) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();
            if (project.ApplicationUserId != currentUserId) return Forbid();

            // Оновлюємо лише дозволені поля
            project.Name = editedProject.Name;
            project.Description = editedProject.Description;

            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(project.Id)) return NotFound(); else throw;
            }
        }


        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var project = await _context.Projects.FirstOrDefaultAsync(m => m.Id == id);
            if (project == null || project.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null && project.ApplicationUserId == _userManager.GetUserId(User))
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetTaskUserStatistics(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userStatistics = await _context.Tasks
                .Where(t => t.ProjectId == id && t.ApplicationUser != null)
                .Include(t => t.ApplicationUser) 
                .GroupBy(t => t.ApplicationUser.UserName)
                .Select(g => new { UserName = g.Key, Count = g.Count() })
                .ToListAsync();

            if (userStatistics == null)
            {
                return NotFound();
            }

            return Json(userStatistics);
        }
        [HttpGet]
        public async Task<IActionResult> GetTaskStatistics(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskStatistics = await _context.Tasks
                .Where(t => t.ProjectId == id)
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            if (taskStatistics == null)
            {
                return NotFound();
            }

            return Json(taskStatistics);
        }
    }
}