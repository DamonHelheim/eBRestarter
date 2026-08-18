namespace eBRestarter.Core.Application.ObjectArchetypes.DTOs.Records;

/// <summary>
/// Progress snapshot of a browser content cleanup run.
/// </summary>
/// <param name="StatusMessage">Human readable status, e.g. the file currently being deleted.</param>
/// <param name="CompletedSteps">Number of finished work units (directories) so far.</param>
/// <param name="TotalSteps">Total number of work units (directories) for this run; 0 while unknown.</param>
/// <remarks>
/// A work unit is one directory, not one file. Counting files up front required a second full
/// recursive walk of the whole browser cache tree purely to obtain a progress maximum
/// (Guide Kap. 7.4: "Bei teuren Quellen (Datei-I/O, Datenbank) bedeutet das: die Operation wird
/// mehrfach wiederholt"). The directory count is known without any I/O, so the run now needs a
/// single traversal. Progress advances in coarser steps; <see cref="StatusMessage"/> keeps
/// reporting the current file so the UI still shows continuous activity.
/// </remarks>
public record struct DeleteBrowserContentProgress(string StatusMessage, int CompletedSteps, int TotalSteps);
