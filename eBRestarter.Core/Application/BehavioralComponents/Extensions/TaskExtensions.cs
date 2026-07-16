#pragma warning disable S3168
using System.Diagnostics;

namespace eBRestarter.Core.Application.BehavioralComponents.Extensions;

public static class TaskExtensions
{
    public static async void Forget(this Task task)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fehler im Fire-and-Forget Task: {ex.Message}");
        }
    }
}
