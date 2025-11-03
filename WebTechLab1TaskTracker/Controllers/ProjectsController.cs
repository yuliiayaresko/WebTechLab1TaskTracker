using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTechLab1TaskTracker.Data;
using WebTechLab1TaskTracker.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebTechLab1TaskTracker.Controllers
{
    [Authorize]
    [AllowAnonymous]
    public class ProjectsController : Controller
    {
        private readonly TaskTrackerDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private const string _storageContainerName = "images";

        public ProjectsController(TaskTrackerDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        
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

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.ApplicationUser)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);

            /*if (project == null || project.ApplicationUserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }*/

            return View(project);
        }

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
                string imageUrl = await UploadImageToBlobAsync(ImageFile);
                project.ImagePath = imageUrl;
            }

            project.ApplicationUserId = _userManager.GetUserId(User);
            _context.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Project editedProject, IFormFile? ImageFile)
        {
            if (id != editedProject.Id) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();
            if (project.ApplicationUserId != currentUserId) return Forbid();

            try
            {
                if (ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(project.ImagePath))
                    {
                        await DeleteImageFromBlobAsync(project.ImagePath);
                    }
                    project.ImagePath = await UploadImageToBlobAsync(ImageFile);
                }

                project.Name = editedProject.Name;
                project.Description = editedProject.Description;

                _context.Update(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(project.Id)) return NotFound(); else throw;
            }
        }

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null && project.ApplicationUserId == _userManager.GetUserId(User))
            {
                if (!string.IsNullOrEmpty(project.ImagePath))
                {
                    await DeleteImageFromBlobAsync(project.ImagePath);
                }

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
            if (id == null) return NotFound();
            var userStatistics = await _context.Tasks
                .Where(t => t.ProjectId == id && t.ApplicationUser != null)
                .Include(t => t.ApplicationUser)
                .GroupBy(t => t.ApplicationUser.UserName)
                .Select(g => new { UserName = g.Key, Count = g.Count() })
                .ToListAsync();
            if (userStatistics == null) return NotFound();
            return Json(userStatistics);
        }

        [HttpGet]
        public async Task<IActionResult> GetTaskStatistics(int? id)
        {
            if (id == null) return NotFound();
            var taskStatistics = await _context.Tasks
                .Where(t => t.ProjectId == id)
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            if (taskStatistics == null) return NotFound();
            return Json(taskStatistics);
        }

        private async Task<string> UploadImageToBlobAsync(IFormFile file)
        {
            string connectionString = _configuration.GetValue<string>("StorageConnectionString");
            var containerClient = new BlobContainerClient(connectionString, _storageContainerName);

            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(blobName);

            var blobHttpHeader = new BlobHttpHeaders { ContentType = file.ContentType };

            await using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, blobHttpHeader);
            }

            return blobClient.Uri.ToString();
        }

        private async System.Threading.Tasks.Task DeleteImageFromBlobAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                string connectionString = _configuration.GetValue<string>("StorageConnectionString");
                var containerClient = new BlobContainerClient(connectionString, _storageContainerName);

                var blobName = Path.GetFileName(new Uri(imageUrl).LocalPath);
                var blobClient = containerClient.GetBlobClient(blobName);

                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting blob: {ex.Message}");
            }
        }
    }
}