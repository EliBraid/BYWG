<template>
  <div class="alerts-container">
    <!-- 报警头部 -->
    <div class="alerts-header">
      <div class="header-left">
        <h1>报警管理</h1>
        <p>工业边缘网关系统报警监控与处理</p>
      </div>
      <div class="header-right">
        <div class="alert-summary">
          <div class="summary-item critical">
            <div class="summary-icon">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 2L2 7v10c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V7l-10-5z" fill="currentColor"/>
              </svg>
            </div>
            <div class="summary-content">
              <span class="summary-count">{{ criticalCount }}</span>
              <span class="summary-label">严重</span>
            </div>
          </div>
          <div class="summary-item warning">
            <div class="summary-icon">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z" fill="currentColor"/>
              </svg>
            </div>
            <div class="summary-content">
              <span class="summary-count">{{ warningCount }}</span>
              <span class="summary-label">警告</span>
            </div>
          </div>
          <div class="summary-item info">
            <div class="summary-icon">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
              </svg>
            </div>
            <div class="summary-content">
              <span class="summary-count">{{ infoCount }}</span>
              <span class="summary-label">信息</span>
            </div>
          </div>
        </div>
        <div class="header-actions">
          <button @click="markAllRead" class="action-btn">全部已读</button>
          <button @click="clearAll" class="action-btn danger">清空所有</button>
        </div>
      </div>
    </div>

    <!-- 过滤和搜索 -->
    <div class="filters-section">
      <div class="filter-group">
        <label>报警级别</label>
        <select v-model="selectedLevel" class="filter-select">
          <option value="">全部级别</option>
          <option value="critical">严重</option>
          <option value="warning">警告</option>
          <option value="info">信息</option>
        </select>
      </div>
      <div class="filter-group">
        <label>设备类型</label>
        <select v-model="selectedDevice" class="filter-select">
          <option value="">全部设备</option>
          <option v-for="device in devices" :key="device.id" :value="device.id">
            {{ device.name }}
          </option>
        </select>
      </div>
      <div class="filter-group">
        <label>状态</label>
        <select v-model="selectedStatus" class="filter-select">
          <option value="">全部状态</option>
          <option value="unread">未读</option>
          <option value="read">已读</option>
          <option value="acknowledged">已确认</option>
          <option value="resolved">已解决</option>
        </select>
      </div>
      <div class="filter-group">
        <label>时间范围</label>
        <select v-model="selectedTimeRange" class="filter-select">
          <option value="">全部时间</option>
          <option value="1h">最近1小时</option>
          <option value="24h">最近24小时</option>
          <option value="7d">最近7天</option>
          <option value="30d">最近30天</option>
        </select>
      </div>
      <div class="search-group">
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="搜索报警信息..." 
          class="search-input"
        />
        <button @click="clearFilters" class="clear-filters-btn">清除筛选</button>
      </div>
    </div>

    <!-- 报警列表 -->
    <div class="alerts-section">
      <div class="alerts-header-bar">
        <h2>报警列表</h2>
        <div class="alerts-stats">
          <span>共 {{ filteredAlerts.length }} 条报警</span>
          <span v-if="unreadCount > 0" class="unread-badge">{{ unreadCount }} 条未读</span>
        </div>
      </div>

      <div class="alerts-list">
        <div 
          v-for="alert in paginatedAlerts" 
          :key="alert.id" 
          class="alert-item"
          :class="[alert.level, alert.status]"
          @click="selectAlert(alert)"
        >
          <div class="alert-checkbox">
            <input 
              type="checkbox" 
              v-model="alert.selected"
              @click.stop
            />
          </div>
          <div class="alert-level">
            <div class="level-indicator" :class="alert.level"></div>
          </div>
          <div class="alert-content">
            <div class="alert-header">
              <h4 class="alert-title">{{ alert.title }}</h4>
              <div class="alert-meta">
                <span class="alert-time">{{ formatTime(alert.timestamp) }}</span>
                <span class="alert-device">{{ alert.deviceName }}</span>
              </div>
            </div>
            <p class="alert-description">{{ alert.description }}</p>
            <div class="alert-tags">
              <span class="alert-tag" :class="alert.level">{{ getLevelText(alert.level) }}</span>
              <span class="alert-tag status" :class="alert.status">{{ getStatusText(alert.status) }}</span>
              <span v-if="alert.acknowledgedBy" class="alert-tag acknowledged">
                已确认: {{ alert.acknowledgedBy }}
              </span>
            </div>
          </div>
          <div class="alert-actions">
            <button 
              v-if="alert.status === 'unread'" 
              @click.stop="markAsRead(alert)" 
              class="action-btn small"
            >
              标记已读
            </button>
            <button 
              v-if="alert.status === 'read'" 
              @click.stop="acknowledgeAlert(alert)" 
              class="action-btn small"
            >
              确认报警
            </button>
            <button 
              v-if="alert.status === 'acknowledged'" 
              @click.stop="resolveAlert(alert)" 
              class="action-btn small success"
            >
              解决
            </button>
            <button @click.stop="deleteAlert(alert)" class="action-btn small danger">
              删除
            </button>
          </div>
        </div>
      </div>

      <!-- 分页 -->
      <div class="pagination">
        <button 
          @click="previousPage" 
          :disabled="currentPage === 1"
          class="page-btn"
        >
          上一页
        </button>
        <span class="page-info">
          第 {{ currentPage }} 页，共 {{ totalPages }} 页
        </span>
        <button 
          @click="nextPage" 
          :disabled="currentPage === totalPages"
          class="page-btn"
        >
          下一页
        </button>
      </div>
    </div>

    <!-- 批量操作 -->
    <div v-if="selectedAlerts.length > 0" class="batch-actions">
      <div class="batch-info">
        已选择 {{ selectedAlerts.length }} 条报警
      </div>
      <div class="batch-buttons">
        <button @click="batchMarkAsRead" class="batch-btn">标记已读</button>
        <button @click="batchAcknowledge" class="batch-btn">批量确认</button>
        <button @click="batchResolve" class="batch-btn success">批量解决</button>
        <button @click="batchDelete" class="batch-btn danger">批量删除</button>
      </div>
    </div>

    <!-- 报警详情模态框 -->
    <div v-if="selectedAlert" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>{{ selectedAlert.title }}</h3>
          <button @click="closeModal" class="close-btn">×</button>
        </div>
        <div class="modal-body">
          <div class="alert-details">
            <div class="detail-row">
              <span class="detail-label">报警级别:</span>
              <span class="detail-value" :class="selectedAlert.level">{{ getLevelText(selectedAlert.level) }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">设备名称:</span>
              <span class="detail-value">{{ selectedAlert.deviceName }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">发生时间:</span>
              <span class="detail-value">{{ formatDateTime(selectedAlert.timestamp) }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">当前状态:</span>
              <span class="detail-value" :class="selectedAlert.status">{{ getStatusText(selectedAlert.status) }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">报警描述:</span>
              <span class="detail-value">{{ selectedAlert.description }}</span>
            </div>
            <div v-if="selectedAlert.acknowledgedBy" class="detail-row">
              <span class="detail-label">确认人员:</span>
              <span class="detail-value">{{ selectedAlert.acknowledgedBy }}</span>
            </div>
            <div v-if="selectedAlert.resolvedBy" class="detail-row">
              <span class="detail-label">解决人员:</span>
              <span class="detail-value">{{ selectedAlert.resolvedBy }}</span>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button @click="closeModal" class="modal-btn">关闭</button>
          <button v-if="selectedAlert.status === 'unread'" @click="markAsRead(selectedAlert)" class="modal-btn">
            标记已读
          </button>
          <button v-if="selectedAlert.status === 'read'" @click="acknowledgeAlert(selectedAlert)" class="modal-btn">
            确认报警
          </button>
          <button v-if="selectedAlert.status === 'acknowledged'" @click="resolveAlert(selectedAlert)" class="modal-btn success">
            解决报警
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'

// 响应式数据
const selectedLevel = ref('')
const selectedDevice = ref('')
const selectedStatus = ref('')
const selectedTimeRange = ref('')
const searchQuery = ref('')
const currentPage = ref(1)
const pageSize = ref(20)
const selectedAlert = ref(null)

// 设备列表
const devices = ref([
  { id: 1, name: 'PLC-001' },
  { id: 2, name: 'PLC-002' },
  { id: 3, name: 'HMI-001' },
  { id: 4, name: 'SCADA-001' }
])

// 报警数据
const alerts = ref([
  {
    id: 1,
    title: '温度超限报警',
    description: '设备PLC-001的温度传感器检测到温度超过设定阈值75°C，当前温度: 78.5°C',
    level: 'critical',
    status: 'unread',
    deviceName: 'PLC-001',
    timestamp: new Date(),
    acknowledgedBy: null,
    resolvedBy: null,
    selected: false
  },
  {
    id: 2,
    title: '网络连接异常',
    description: '设备PLC-002网络连接不稳定，连续3次连接超时',
    level: 'warning',
    status: 'read',
    deviceName: 'PLC-002',
    timestamp: new Date(Date.now() - 300000),
    acknowledgedBy: null,
    resolvedBy: null,
    selected: false
  },
  {
    id: 3,
    title: '数据采集完成',
    description: '设备HMI-001数据采集任务已完成，共采集1250个数据点',
    level: 'info',
    status: 'acknowledged',
    deviceName: 'HMI-001',
    timestamp: new Date(Date.now() - 600000),
    acknowledgedBy: 'admin',
    resolvedBy: null,
    selected: false
  },
  {
    id: 4,
    title: '压力传感器故障',
    description: '设备SCADA-001压力传感器通信中断，请检查硬件连接',
    level: 'critical',
    status: 'resolved',
    deviceName: 'SCADA-001',
    timestamp: new Date(Date.now() - 900000),
    acknowledgedBy: 'admin',
    resolvedBy: 'admin',
    selected: false
  }
])

// 计算属性
const criticalCount = computed(() => 
  alerts.value.filter(alert => alert.level === 'critical').length
)

const warningCount = computed(() => 
  alerts.value.filter(alert => alert.level === 'warning').length
)

const infoCount = computed(() => 
  alerts.value.filter(alert => alert.level === 'info').length
)

const unreadCount = computed(() => 
  alerts.value.filter(alert => alert.status === 'unread').length
)

const filteredAlerts = computed(() => {
  let filtered = alerts.value

  if (selectedLevel.value) {
    filtered = filtered.filter(alert => alert.level === selectedLevel.value)
  }

  if (selectedDevice.value) {
    const device = devices.value.find(d => d.id.toString() === selectedDevice.value)
    if (device) {
      filtered = filtered.filter(alert => alert.deviceName === device.name)
    }
  }

  if (selectedStatus.value) {
    filtered = filtered.filter(alert => alert.status === selectedStatus.value)
  }

  if (selectedTimeRange.value) {
    const now = new Date()
    const timeMap = {
      '1h': 60 * 60 * 1000,
      '24h': 24 * 60 * 60 * 1000,
      '7d': 7 * 24 * 60 * 60 * 1000,
      '30d': 30 * 24 * 60 * 60 * 1000
    }
    const timeDiff = timeMap[selectedTimeRange.value]
    if (timeDiff) {
      filtered = filtered.filter(alert => 
        now.getTime() - alert.timestamp.getTime() <= timeDiff
      )
    }
  }

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(alert => 
      alert.title.toLowerCase().includes(query) ||
      alert.description.toLowerCase().includes(query) ||
      alert.deviceName.toLowerCase().includes(query)
    )
  }

  return filtered.sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime())
})

const totalPages = computed(() => 
  Math.ceil(filteredAlerts.value.length / pageSize.value)
)

const paginatedAlerts = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredAlerts.value.slice(start, end)
})

const selectedAlerts = computed(() => 
  alerts.value.filter(alert => alert.selected)
)

// 方法
function getLevelText(level: string) {
  const levelMap: Record<string, string> = {
    critical: '严重',
    warning: '警告',
    info: '信息'
  }
  return levelMap[level] || level
}

function getStatusText(status: string) {
  const statusMap: Record<string, string> = {
    unread: '未读',
    read: '已读',
    acknowledged: '已确认',
    resolved: '已解决'
  }
  return statusMap[status] || status
}

function formatTime(timestamp: Date) {
  return timestamp.toLocaleTimeString()
}

function formatDateTime(timestamp: Date) {
  return timestamp.toLocaleString()
}

function selectAlert(alert: any) {
  selectedAlert.value = alert
}

function closeModal() {
  selectedAlert.value = null
}

function markAsRead(alert: any) {
  alert.status = 'read'
  if (selectedAlert.value && selectedAlert.value.id === alert.id) {
    selectedAlert.value = alert
  }
}

function acknowledgeAlert(alert: any) {
  alert.status = 'acknowledged'
  alert.acknowledgedBy = 'admin'
  if (selectedAlert.value && selectedAlert.value.id === alert.id) {
    selectedAlert.value = alert
  }
}

function resolveAlert(alert: any) {
  alert.status = 'resolved'
  alert.resolvedBy = 'admin'
  if (selectedAlert.value && selectedAlert.value.id === alert.id) {
    selectedAlert.value = alert
  }
}

function deleteAlert(alert: any) {
  const index = alerts.value.findIndex(a => a.id === alert.id)
  if (index > -1) {
    alerts.value.splice(index, 1)
  }
  if (selectedAlert.value && selectedAlert.value.id === alert.id) {
    selectedAlert.value = null
  }
}

function markAllRead() {
  alerts.value.forEach(alert => {
    if (alert.status === 'unread') {
      alert.status = 'read'
    }
  })
}

function clearAll() {
  if (confirm('确定要清空所有报警吗？此操作不可恢复。')) {
    alerts.value = []
  }
}

function clearFilters() {
  selectedLevel.value = ''
  selectedDevice.value = ''
  selectedStatus.value = ''
  selectedTimeRange.value = ''
  searchQuery.value = ''
  currentPage.value = 1
}

function previousPage() {
  if (currentPage.value > 1) {
    currentPage.value--
  }
}

function nextPage() {
  if (currentPage.value < totalPages.value) {
    currentPage.value++
  }
}

function batchMarkAsRead() {
  selectedAlerts.value.forEach(alert => {
    if (alert.status === 'unread') {
      alert.status = 'read'
    }
  })
}

function batchAcknowledge() {
  selectedAlerts.value.forEach(alert => {
    if (alert.status === 'read') {
      alert.status = 'acknowledged'
      alert.acknowledgedBy = 'admin'
    }
  })
}

function batchResolve() {
  selectedAlerts.value.forEach(alert => {
    if (alert.status === 'acknowledged') {
      alert.status = 'resolved'
      alert.resolvedBy = 'admin'
    }
  })
}

function batchDelete() {
  if (confirm(`确定要删除选中的 ${selectedAlerts.value.length} 条报警吗？`)) {
    selectedAlerts.value.forEach(alert => {
      deleteAlert(alert)
    })
  }
}

// 生命周期
onMounted(() => {
  // 可以在这里添加初始化逻辑
})
</script>

<style scoped>
.alerts-container {
  padding: 24px;
  background: #f8f9fa;
  min-height: 100vh;
}

.alerts-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
  padding: 24px;
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
}

.header-left h1 {
  font-size: 28px;
  font-weight: 700;
  color: #2c3e50;
  margin: 0 0 8px;
}

.header-left p {
  font-size: 16px;
  color: #6c757d;
  margin: 0;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 32px;
}

.alert-summary {
  display: flex;
  gap: 24px;
}

.summary-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  border-radius: 8px;
  background: white;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.summary-item.critical {
  border-left: 4px solid #e74c3c;
}

.summary-item.warning {
  border-left: 4px solid #f39c12;
}

.summary-item.info {
  border-left: 4px solid #3498db;
}

.summary-icon {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.summary-item.critical .summary-icon {
  background: #e74c3c;
}

.summary-item.warning .summary-icon {
  background: #f39c12;
}

.summary-item.info .summary-icon {
  background: #3498db;
}

.summary-content {
  display: flex;
  flex-direction: column;
}

.summary-count {
  font-size: 24px;
  font-weight: 700;
  color: #2c3e50;
}

.summary-label {
  font-size: 14px;
  color: #6c757d;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.action-btn {
  padding: 8px 16px;
  background: #00d4ff;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.action-btn:hover {
  background: #0099cc;
}

.action-btn.danger {
  background: #e74c3c;
}

.action-btn.danger:hover {
  background: #c0392b;
}

.filters-section {
  display: flex;
  gap: 20px;
  margin-bottom: 24px;
  padding: 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  flex-wrap: wrap;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.filter-group label {
  font-size: 14px;
  font-weight: 500;
  color: #2c3e50;
}

.filter-select {
  padding: 8px 12px;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  font-size: 14px;
  background: white;
  min-width: 120px;
}

.search-group {
  display: flex;
  align-items: end;
  gap: 12px;
}

.search-input {
  padding: 8px 12px;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  font-size: 14px;
  min-width: 200px;
}

.clear-filters-btn {
  padding: 8px 16px;
  background: #6c757d;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.clear-filters-btn:hover {
  background: #5a6268;
}

.alerts-section {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  overflow: hidden;
}

.alerts-header-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  background: #f8f9fa;
  border-bottom: 1px solid #e9ecef;
}

.alerts-header-bar h2 {
  font-size: 20px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.alerts-stats {
  display: flex;
  align-items: center;
  gap: 16px;
  font-size: 14px;
  color: #6c757d;
}

.unread-badge {
  background: #e74c3c;
  color: white;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.alerts-list {
  max-height: 600px;
  overflow-y: auto;
}

.alert-item {
  display: flex;
  align-items: center;
  padding: 16px 20px;
  border-bottom: 1px solid #f1f3f4;
  cursor: pointer;
  transition: all 0.3s ease;
}

.alert-item:hover {
  background: #f8f9fa;
}

.alert-item.critical {
  border-left: 4px solid #e74c3c;
}

.alert-item.warning {
  border-left: 4px solid #f39c12;
}

.alert-item.info {
  border-left: 4px solid #3498db;
}

.alert-item.unread {
  background: #fff5f5;
}

.alert-checkbox {
  margin-right: 16px;
}

.alert-level {
  margin-right: 16px;
}

.level-indicator {
  width: 12px;
  height: 12px;
  border-radius: 50%;
}

.level-indicator.critical {
  background: #e74c3c;
}

.level-indicator.warning {
  background: #f39c12;
}

.level-indicator.info {
  background: #3498db;
}

.alert-content {
  flex: 1;
}

.alert-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.alert-title {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.alert-meta {
  display: flex;
  gap: 16px;
  font-size: 12px;
  color: #6c757d;
}

.alert-description {
  font-size: 14px;
  color: #6c757d;
  margin: 0 0 8px;
  line-height: 1.4;
}

.alert-tags {
  display: flex;
  gap: 8px;
}

.alert-tag {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.alert-tag.critical {
  background: rgba(231, 76, 60, 0.1);
  color: #e74c3c;
}

.alert-tag.warning {
  background: rgba(243, 156, 18, 0.1);
  color: #f39c12;
}

.alert-tag.info {
  background: rgba(52, 152, 219, 0.1);
  color: #3498db;
}

.alert-tag.status {
  background: #f8f9fa;
  color: #6c757d;
}

.alert-tag.acknowledged {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.alert-actions {
  display: flex;
  gap: 8px;
}

.action-btn.small {
  padding: 4px 8px;
  font-size: 12px;
}

.action-btn.success {
  background: #27ae60;
}

.action-btn.success:hover {
  background: #229954;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: #f8f9fa;
  border-top: 1px solid #e9ecef;
}

.page-btn {
  padding: 8px 16px;
  background: white;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.page-btn:hover:not(:disabled) {
  background: #f8f9fa;
  border-color: #00d4ff;
}

.page-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-info {
  font-size: 14px;
  color: #6c757d;
}

.batch-actions {
  position: fixed;
  bottom: 20px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 24px;
  background: #2c3e50;
  color: white;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
  z-index: 1000;
}

.batch-info {
  font-size: 14px;
  font-weight: 500;
}

.batch-buttons {
  display: flex;
  gap: 8px;
}

.batch-btn {
  padding: 6px 12px;
  background: rgba(255, 255, 255, 0.2);
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.batch-btn:hover {
  background: rgba(255, 255, 255, 0.3);
}

.batch-btn.success {
  background: #27ae60;
}

.batch-btn.danger {
  background: #e74c3c;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
}

.modal-content {
  background: white;
  border-radius: 8px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  max-width: 600px;
  width: 90%;
  max-height: 80vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #e9ecef;
}

.modal-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  font-size: 24px;
  color: #6c757d;
  cursor: pointer;
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-body {
  padding: 20px;
}

.alert-details {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.detail-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.detail-label {
  font-weight: 500;
  color: #6c757d;
  min-width: 80px;
}

.detail-value {
  color: #2c3e50;
}

.detail-value.critical {
  color: #e74c3c;
  font-weight: 600;
}

.detail-value.warning {
  color: #f39c12;
  font-weight: 600;
}

.detail-value.info {
  color: #3498db;
  font-weight: 600;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 20px;
  border-top: 1px solid #e9ecef;
}

.modal-btn {
  padding: 8px 16px;
  background: #00d4ff;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.modal-btn:hover {
  background: #0099cc;
}

.modal-btn.success {
  background: #27ae60;
}

.modal-btn.success:hover {
  background: #229954;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .alerts-header {
    flex-direction: column;
    gap: 20px;
  }
  
  .header-right {
    flex-direction: column;
    gap: 16px;
  }
  
  .alert-summary {
    flex-direction: column;
    gap: 12px;
  }
  
  .filters-section {
    flex-direction: column;
  }
  
  .alert-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
  
  .alert-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }
  
  .batch-actions {
    flex-direction: column;
    gap: 12px;
  }
}
</style>
