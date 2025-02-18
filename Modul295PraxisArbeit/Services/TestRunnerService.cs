using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TestRunnerService : BackgroundService
{
    private readonly ILogger<TestRunnerService> _logger;

    public TestRunnerService(ILogger<TestRunnerService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Running Unit Tests...");
            RunTests();
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    private void RunTests()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "test --logger console;verbosity=detailed",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            _logger.LogInformation("Unit Tests Started...");
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("Unit Tests Completed Successfully.");
            }
            else
            {
                _logger.LogError("Unit Tests Failed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error running tests: {ex.Message}");
        }
    }
}
