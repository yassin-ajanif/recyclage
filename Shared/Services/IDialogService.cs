namespace Recyclage.Shared.Services;

public interface IDialogService
{
    /// <summary>Yes/no prompt. Returns true when the user confirms.</summary>
    Task<bool> ConfirmAsync(string title, string message, CancellationToken cancellationToken = default);

    /// <summary>Modal error message with a single dismiss button.</summary>
    Task ShowErrorAsync(string title, string message, CancellationToken cancellationToken = default);
}
