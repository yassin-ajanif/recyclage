using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Recyclage.Shared.Database;
using Recyclage.Shared.Models;

namespace Recyclage.ViewModels;

public partial class ClientsViewModel : PageViewModelBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public override string Title => "الزبائن";

    public ObservableCollection<ClientRowViewModel> Rows { get; } = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public ClientsViewModel(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var clients = await db.Clients
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            Rows.Clear();
            foreach (var client in clients)
                Rows.Add(ToRow(client));

            EnsureTrailingEmptyRow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر تحميل الزبائن: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveRowAsync(ClientRowViewModel row)
    {
        if (row.IsEmpty || IsBusy)
            return;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            if (row.Id == 0)
            {
                var entity = new Client
                {
                    Name = row.Name.Trim(),
                    Phone = row.Phone.Trim(),
                    Ice = row.Ice.Trim()
                };

                db.Clients.Add(entity);
                await db.SaveChangesAsync();
                row.MarkAsSaved(entity.Id);
                EnsureTrailingEmptyRow();
            }
            else
            {
                var entity = await db.Clients.FirstOrDefaultAsync(c => c.Id == row.Id);
                if (entity is null)
                    return;

                entity.Name = row.Name.Trim();
                entity.Phone = row.Phone.Trim();
                entity.Ice = row.Ice.Trim();
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر حفظ الزبون: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteRow(ClientRowViewModel row)
    {
        if (row.Id == 0)
            return;

        IsBusy = true;
        StatusMessage = null;
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var entity = await db.Clients.FirstOrDefaultAsync(c => c.Id == row.Id);
            if (entity is not null)
            {
                db.Clients.Remove(entity);
                await db.SaveChangesAsync();
            }

            Rows.Remove(row);
            EnsureTrailingEmptyRow();
        }
        catch (Exception ex)
        {
            StatusMessage = $"تعذر حذف الزبون: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void EnsureTrailingEmptyRow()
    {
        if (Rows.Count == 0 || !Rows[^1].IsEmpty)
            Rows.Add(new ClientRowViewModel());
    }

    private static ClientRowViewModel ToRow(Client client) => new()
    {
        Id = client.Id,
        Name = client.Name,
        Phone = client.Phone,
        Ice = client.Ice
    };
}
