<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { listDevices, type DeviceDto } from '@/api/devices'

const devices = ref<DeviceDto[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const searchQuery = ref('')
const statusFilter = ref('all')
const sortBy = ref('name')
const sortOrder = ref('asc')

// 计算属性
const filteredDevices = computed(() => {
  let filtered = devices.value

  // 搜索过滤
  if (searchQuery.value) {
    filtered = filtered.filter(device => 
      device.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      device.ip.includes(searchQuery.value)
    )
  }

  // 状态过滤
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter(device => device.status === statusFilter.value)
  }

  // 排序
  filtered.sort((a, b) => {
    let aValue = a[sortBy.value as keyof DeviceDto]
    let bValue = b[sortBy.value as keyof DeviceDto]
    
    if (typeof aValue === 'string' && typeof bValue === 'string') {
      aValue = aValue.toLowerCase()
      bValue = bValue.toLowerCase()
    }
    
    if (sortOrder.value === 'asc') {
      return aValue < bValue ? -1 : aValue > bValue ? 1 : 0
    } else {
      return aValue > bValue ? -1 : aValue < bValue ? 1 : 0
    }
  })

  return filtered
})

const deviceStats = computed(() => {
  const total = devices.value.length
  const online = devices.value.filter(d => d.status === 'online').length
  const offline = total - online
  return { total, online, offline }
})

async function fetchDevices() {
  loading.value = true
  error.value = null
  try {
    devices.value = await listDevices()
  } catch (e: any) {
    error.value = e.message || String(e)
  } finally {
    loading.value = false
  }
}

function handleSort(field: string) {
  if (sortBy.value === field) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortBy.value = field
    sortOrder.value = 'asc'
  }
}

function getStatusColor(status: string) {
  switch (status) {
    case 'online': return '#28a745'
    case 'offline': return '#dc3545'
    default: return '#6c757d'
  }
}

function getStatusIcon(status: string) {
  switch (status) {
    case 'online': return 'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z'
    case 'offline': return 'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z'
    default: return 'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z'
  }
}

onMounted(fetchDevices)
</script>

<template>
  <div class="devices-container">
    <!-- 设备统计卡片 -->
    <div class="stats-cards">
      <div class="stat-card total">
        <div class="stat-icon">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ deviceStats.total }}</div>
          <div class="stat-label">总设备数</div>
        </div>
      </div>
      
      <div class="stat-card online">
        <div class="stat-icon">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ deviceStats.online }}</div>
          <div class="stat-label">在线设备</div>
        </div>
      </div>
      
      <div class="stat-card offline">
        <div class="stat-icon">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ deviceStats.offline }}</div>
          <div class="stat-label">离线设备</div>
        </div>
      </div>
    </div>

    <!-- 设备管理面板 -->
    <div class="devices-panel">
      <div class="panel-header">
        <h3>设备列表</h3>
        <div class="panel-actions">
          <button class="btn-refresh" @click="fetchDevices" :disabled="loading">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
            </svg>
          </button>
          <button class="btn-add" disabled>
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z" fill="currentColor"/>
            </svg>
            添加设备
          </button>
        </div>
      </div>

      <!-- 搜索和过滤 -->
      <div class="filters">
        <div class="search-box">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z" fill="currentColor"/>
          </svg>
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="搜索设备名称或IP地址..."
            class="search-input"
          />
        </div>
        
        <div class="filter-group">
          <select v-model="statusFilter" class="filter-select">
            <option value="all">全部状态</option>
            <option value="online">在线</option>
            <option value="offline">离线</option>
          </select>
        </div>
      </div>

      <!-- 设备表格 -->
      <div class="table-container">
        <div v-if="loading" class="loading-state">
          <div class="loading-spinner"></div>
          <span>加载设备数据中...</span>
        </div>
        
        <div v-else-if="error" class="error-state">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
          <h4>加载失败</h4>
          <p>{{ error }}</p>
          <button class="btn-retry" @click="fetchDevices">重试</button>
        </div>
        
        <div v-else-if="filteredDevices.length === 0" class="empty-state">
          <svg width="64" height="64" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
          <h4>暂无设备</h4>
          <p>没有找到匹配的设备</p>
        </div>
        
        <table v-else class="devices-table">
        <thead>
          <tr>
              <th @click="handleSort('name')" class="sortable">
                设备名称
                <svg v-if="sortBy === 'name'" width="12" height="12" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M7 14l5-5 5 5z" fill="currentColor"/>
                </svg>
              </th>
              <th @click="handleSort('ip')" class="sortable">
                IP地址
                <svg v-if="sortBy === 'ip'" width="12" height="12" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M7 14l5-5 5 5z" fill="currentColor"/>
                </svg>
              </th>
              <th @click="handleSort('status')" class="sortable">
                状态
                <svg v-if="sortBy === 'status'" width="12" height="12" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M7 14l5-5 5 5z" fill="currentColor"/>
                </svg>
              </th>
              <th>最后连接</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
            <tr v-for="device in filteredDevices" :key="device.id" class="device-row">
              <td class="device-name">
                <div class="device-info">
                  <div class="device-icon">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
                    </svg>
                  </div>
                  <span>{{ device.name }}</span>
                </div>
              </td>
              <td class="device-ip">{{ device.ip }}</td>
              <td class="device-status">
                <div class="status-indicator">
                  <div 
                    class="status-dot" 
                    :style="{ backgroundColor: getStatusColor(device.status || 'offline') }"
                  ></div>
                  <span class="status-text">{{ device.status || 'offline' }}</span>
                </div>
            </td>
              <td class="device-time">2分钟前</td>
              <td class="device-actions">
                <button class="btn-action" title="查看详情">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z" fill="currentColor"/>
                  </svg>
                </button>
                <button class="btn-action" title="编辑设备" disabled>
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z" fill="currentColor"/>
                  </svg>
                </button>
                <button class="btn-action danger" title="删除设备" disabled>
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z" fill="currentColor"/>
                  </svg>
                </button>
            </td>
          </tr>
        </tbody>
      </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
.devices-container {
  display: flex;
  flex-direction: column;
  gap: 24px;
  width: 100%;
  max-width: 100%;
  min-height: calc(100vh - 120px);
}

.stats-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin-bottom: 20px;
}

.stat-card {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  backdrop-filter: blur(10px);
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 16px;
  position: relative;
  overflow: hidden;
}

.stat-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
}

.stat-card.total::before {
  background: linear-gradient(90deg, #4a90e2, #357abd);
}

.stat-card.online::before {
  background: linear-gradient(90deg, #28a745, #20c997);
}

.stat-card.offline::before {
  background: linear-gradient(90deg, #dc3545, #c82333);
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.15);
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
}

.stat-card.total .stat-icon {
  background: linear-gradient(135deg, #4a90e2, #357abd);
}

.stat-card.online .stat-icon {
  background: linear-gradient(135deg, #28a745, #20c997);
}

.stat-card.offline .stat-icon {
  background: linear-gradient(135deg, #dc3545, #c82333);
}

.stat-content {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-value {
  font-size: 28px;
  font-weight: 700;
  color: #2c3e50;
  line-height: 1;
}

.stat-label {
  font-size: 14px;
  color: #6c757d;
  font-weight: 500;
}

.devices-panel {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  backdrop-filter: blur(10px);
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 2px solid rgba(74, 144, 226, 0.1);
}

.panel-header h3 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: #2c3e50;
}

.panel-actions {
  display: flex;
  gap: 12px;
  align-items: center;
}

.btn-refresh, .btn-add {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  font-size: 14px;
  transition: all 0.3s ease;
}

.btn-refresh {
  background: linear-gradient(135deg, #4a90e2, #357abd);
  color: white;
  box-shadow: 0 4px 12px rgba(74, 144, 226, 0.3);
}

.btn-refresh:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(74, 144, 226, 0.4);
}

.btn-refresh:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-add {
  background: linear-gradient(135deg, #28a745, #20c997);
  color: white;
  box-shadow: 0 4px 12px rgba(40, 167, 69, 0.3);
}

.btn-add:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(40, 167, 69, 0.4);
}

.btn-add:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.filters {
  display: flex;
  gap: 16px;
  margin-bottom: 24px;
  align-items: center;
}

.search-box {
  position: relative;
  flex: 1;
  max-width: 400px;
}

.search-box svg {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #6c757d;
  pointer-events: none;
}

.search-input {
  width: 100%;
  padding: 12px 12px 12px 40px;
  border: 2px solid rgba(74, 144, 226, 0.1);
  border-radius: 8px;
  font-size: 14px;
  background: rgba(255, 255, 255, 0.8);
  transition: all 0.3s ease;
}

.search-input:focus {
  outline: none;
  border-color: #4a90e2;
  box-shadow: 0 0 0 3px rgba(74, 144, 226, 0.1);
  background: white;
}

.filter-group {
  display: flex;
  gap: 8px;
}

.filter-select {
  padding: 12px 16px;
  border: 2px solid rgba(74, 144, 226, 0.1);
  border-radius: 8px;
  font-size: 14px;
  background: rgba(255, 255, 255, 0.8);
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-select:focus {
  outline: none;
  border-color: #4a90e2;
  box-shadow: 0 0 0 3px rgba(74, 144, 226, 0.1);
  background: white;
}

.table-container {
  overflow-x: auto;
  border-radius: 12px;
  border: 1px solid rgba(0, 0, 0, 0.08);
}

.loading-state, .error-state, .empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  text-align: center;
  color: #6c757d;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(74, 144, 226, 0.1);
  border-top: 4px solid #4a90e2;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-state h4, .empty-state h4 {
  margin: 16px 0 8px;
  color: #2c3e50;
  font-size: 18px;
}

.error-state p, .empty-state p {
  margin: 0 0 16px;
  font-size: 14px;
}

.btn-retry {
  padding: 10px 20px;
  background: linear-gradient(135deg, #4a90e2, #357abd);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.3s ease;
}

.btn-retry:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(74, 144, 226, 0.4);
}

.devices-table {
  width: 100%;
  border-collapse: collapse;
  background: white;
}

.devices-table thead {
  background: linear-gradient(135deg, #f8f9fa, #e9ecef);
}

.devices-table th {
  padding: 16px;
  text-align: left;
  font-weight: 600;
  color: #2c3e50;
  border-bottom: 2px solid rgba(74, 144, 226, 0.1);
  position: relative;
}

.devices-table th.sortable {
  cursor: pointer;
  user-select: none;
  transition: all 0.3s ease;
}

.devices-table th.sortable:hover {
  background: rgba(74, 144, 226, 0.05);
}

.devices-table th svg {
  margin-left: 8px;
  opacity: 0.6;
}

.devices-table td {
  padding: 16px;
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
  vertical-align: middle;
}

.device-row {
  transition: all 0.3s ease;
}

.device-row:hover {
  background: rgba(74, 144, 226, 0.02);
}

.device-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.device-icon {
  width: 32px;
  height: 32px;
  background: rgba(74, 144, 226, 0.1);
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #4a90e2;
}

.device-name {
  font-weight: 600;
  color: #2c3e50;
}

.device-ip {
  font-family: 'Courier New', monospace;
  color: #6c757d;
  font-size: 14px;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  animation: pulse 2s infinite;
}

.status-text {
  font-size: 14px;
  font-weight: 500;
  text-transform: capitalize;
}

.device-time {
  color: #6c757d;
  font-size: 14px;
}

.device-actions {
  display: flex;
  gap: 8px;
}

.btn-action {
  width: 32px;
  height: 32px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
  background: rgba(74, 144, 226, 0.1);
  color: #4a90e2;
}

.btn-action:hover:not(:disabled) {
  background: rgba(74, 144, 226, 0.2);
  transform: translateY(-1px);
}

.btn-action.danger {
  background: rgba(220, 53, 69, 0.1);
  color: #dc3545;
}

.btn-action.danger:hover:not(:disabled) {
  background: rgba(220, 53, 69, 0.2);
}

.btn-action:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .stats-cards {
    grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  }
}

@media (max-width: 768px) {
  .stats-cards {
    grid-template-columns: 1fr;
    gap: 16px;
  }
  
  .filters {
    flex-direction: column;
    gap: 12px;
    align-items: stretch;
  }
  
  .search-box {
    max-width: none;
  }
  
  .devices-table {
    font-size: 14px;
  }
  
  .devices-table th,
  .devices-table td {
    padding: 12px 8px;
  }
  
  .device-actions {
    flex-direction: column;
    gap: 4px;
  }
  
  .btn-action {
    width: 28px;
    height: 28px;
  }
}

@media (max-width: 480px) {
  .devices-panel {
    padding: 16px;
  }
  
  .stat-card {
    padding: 16px;
  }
  
  .devices-table {
    font-size: 12px;
  }
  
  .devices-table th,
  .devices-table td {
    padding: 8px 4px;
  }
}
</style>


