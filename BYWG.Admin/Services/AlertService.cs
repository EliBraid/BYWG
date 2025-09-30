using BYWG.Admin.Data;
using Microsoft.EntityFrameworkCore;

namespace BYWG.Admin.Services;

public class AlertService : IAlertService
{
    private readonly AdminDbContext _context;
    private readonly ILogger<AlertService> _logger;

    public AlertService(AdminDbContext context, ILogger<AlertService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Alert>> GetAllAlertsAsync() => await _context.Alerts.OrderByDescending(a => a.CreatedAt).ToListAsync();

    public async Task<Alert?> GetAlertByIdAsync(int id) => await _context.Alerts.FindAsync(id);

    public async Task<Alert> CreateAlertAsync(Alert alert)
    {
        alert.CreatedAt = DateTime.UtcNow;
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();
        return alert;
    }

    public async Task<Alert> UpdateAlertAsync(Alert alert)
    {
        var exists = await _context.Alerts.FindAsync(alert.Id) ?? throw new ArgumentException("报警不存在");
        exists.Title = alert.Title;
        exists.Message = alert.Message;
        exists.Level = alert.Level;
        exists.Status = alert.Status;
        exists.DeviceId = alert.DeviceId;
        exists.GatewayId = alert.GatewayId;
        await _context.SaveChangesAsync();
        return exists;
    }

    public async Task<bool> DeleteAlertAsync(int id)
    {
        var a = await _context.Alerts.FindAsync(id);
        if (a == null) return false;
        _context.Alerts.Remove(a);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AcknowledgeAlertAsync(int id, string acknowledgedBy)
    {
        var a = await _context.Alerts.FindAsync(id);
        if (a == null) return false;
        a.AcknowledgedAt = DateTime.UtcNow;
        a.Status = "Acknowledged";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResolveAlertAsync(int id, string resolvedBy)
    {
        var a = await _context.Alerts.FindAsync(id);
        if (a == null) return false;
        a.ResolvedAt = DateTime.UtcNow;
        a.Status = "Resolved";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Alert>> GetUnresolvedAlertsAsync() => await _context.Alerts.Where(a => a.Status != "Resolved").ToListAsync();

    public async Task<IEnumerable<Alert>> GetAlertsByLevelAsync(string level) => await _context.Alerts.Where(a => a.Level == level).ToListAsync();

    public async Task<IEnumerable<Alert>> SearchAlertsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return await GetAllAlertsAsync();
        return await _context.Alerts.Where(a => a.Title.Contains(searchTerm) || a.Message.Contains(searchTerm) || a.Source.Contains(searchTerm)).ToListAsync();
    }
}


