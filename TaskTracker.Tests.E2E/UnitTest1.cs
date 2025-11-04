using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.RegularExpressions;

namespace TaskTracker.Tests.E2E;
//гіт не бачить файл
[TestFixture]
public class ProjectFlowTests : PageTest
{
    private const string AppUrl = "https://web-tech-lab1-tasktracker-h9c3cffmfubudecx.polandcentral-01.azurewebsites.net/";
    private const string AuthStateFile = "auth_state.json";

    [Test]
    [Ignore("Файл auth_state.json вже згенеровано")]
    public async Task GenerateAuthState()
    {
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        page.SetDefaultTimeout(180000);
        await page.GotoAsync(AppUrl);

        
        await page.WaitForURLAsync($"{AppUrl}Projects");

        await context.StorageStateAsync(new() { Path = AuthStateFile });
        Console.WriteLine($"Успішно згенеровано {AuthStateFile}. Тепер можете ігнорувати цей тест.");
    }

    
    [Test]
    
    public async Task ShouldCreateAndDeleteProjectTest()
    {
        
        await using var browser = await Microsoft.Playwright.Playwright.CreateAsync().Result.Chromium.LaunchAsync(new() { Headless = true }); // Можна 'false', щоб бачити
        await using var context = await browser.NewContextAsync(new()
        {
            StorageStatePath = AuthStateFile 
        });

        var page = await context.NewPageAsync();


        await page.GotoAsync($"{AppUrl}Projects");

        await Expect(page.GetByText("Hello yuliia_yaresko@knu.ua")).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Link, new() { Name = "Create New Project" }).ClickAsync();

        await Expect(page).ToHaveURLAsync(new Regex($"{AppUrl}Projects/Create"));

        var projectName = $"E2E Test Project {DateTime.Now:dd-MM-yyyy HH:mm:ss}";

        await page.GetByLabel("Name").FillAsync(projectName);
        await page.GetByLabel("Description").FillAsync("This project was created by an automated test.");

        await page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();

       
        await Expect(page).ToHaveURLAsync(new Regex($"{AppUrl}Projects"));

        await Expect(page.GetByText(projectName)).ToBeVisibleAsync();

        Console.WriteLine("Тест на СТВОРЕННЯ проєкту успішний!");

        
        var projectCard = page.Locator("div.card", new() { HasTextString = projectName });

        await projectCard.GetByRole(AriaRole.Link, new() { Name = "Delete" }).ClickAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();

        
        await Expect(page).ToHaveURLAsync(new Regex($"{AppUrl}Projects"));
        await Expect(page.GetByText(projectName)).ToHaveCountAsync(0);

        Console.WriteLine("Тест на ВИДАЛЕННЯ проєкту успішний!");
    }
}