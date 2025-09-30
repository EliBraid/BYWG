using BYWG.Admin.Data;

namespace BYWG.Admin.Services;

/// <summary>
/// 报警服务接口
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// 获取所有报警
    /// </summary>
    Task<IEnumerable<Alert>> GetAllAlertsAsync();

    /// <summary>
    /// 根据ID获取报警
    /// </summary>
    Task<Alert?> GetAlertByIdAsync(int id);

    /// <summary>
    /// 创建报警
    /// </summary>
    Task<Alert> CreateAlertAsync(Alert alert);

    /// <summary>
    /// 更新报警
    /// </summary>
    Task<Alert> UpdateAlertAsync(Alert alert);

    /// <summary>
    /// 删除报警
    /// </summary>
    Task<bool> DeleteAlertAsync(int id);

    /// <summary>
    /// 确认报警
    /// </summary>
    Task<bool> AcknowledgeAlertAsync(int id, string acknowledgedBy);

    /// <summary>
    /// 解决报警
    /// </summary>
    Task<bool> ResolveAlertAsync(int id, string resolvedBy);

    /// <summary>
    /// 获取未解决的报警
    /// </summary>
    Task<IEnumerable<Alert>> GetUnresolvedAlertsAsync();

    /// <summary>
    /// 根据级别获取报警
    /// </summary>
    Task<IEnumerable<Alert>> GetAlertsByLevelAsync(string level);

    /// <summary>
    /// 搜索报警
    /// </summary>
    Task<IEnumerable<Alert>> SearchAlertsAsync(string searchTerm);
}
