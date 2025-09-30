<template>
  <div class="servers-container">
    <!-- 服务器集群概览 -->
    <div class="stats-overview">
      <div class="stat-card">
        <div class="stat-icon servers">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M4 6h16v2H4zm0 5h16v2H4zm0 5h16v2H4z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ serverStats.total }}</div>
          <div class="stat-label">总服务器数</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon online">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ serverStats.online }}</div>
          <div class="stat-label">在线服务器</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon offline">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ serverStats.offline }}</div>
          <div class="stat-label">离线服务器</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon performance">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ serverStats.avgCpu }}%</div>
          <div class="stat-label">平均CPU使用率</div>
        </div>
      </div>
    </div>

    <!-- 服务器管理面板 -->
    <div class="management-panel">
      <div class="panel-header">
        <h3>服务器管理</h3>
        <div class="panel-actions">
          <button class="btn-secondary" @click="refreshServers">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
            </svg>
            刷新
          </button>
          <button class="btn-primary" @click="addServer">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z" fill="currentColor"/>
            </svg>
            添加服务器
          </button>
        </div>
      </div>

      <!-- 搜索和筛选 -->
      <div class="filter-section">
        <div class="search-box">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z" fill="currentColor"/>
          </svg>
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="搜索服务器名称或IP地址..."
            class="search-input"
          />
        </div>
        <div class="filter-controls">
          <select v-model="statusFilter" class="filter-select">
            <option value="">所有状态</option>
            <option value="online">在线</option>
            <option value="offline">离线</option>
            <option value="maintenance">维护中</option>
          </select>
          <select v-model="sortBy" class="filter-select">
            <option value="name">按名称排序</option>
            <option value="status">按状态排序</option>
            <option value="cpu">按CPU使用率排序</option>
            <option value="memory">按内存使用率排序</option>
          </select>
        </div>
      </div>
    </div>

    <!-- 服务器列表 -->
    <div class="servers-grid">
      <div 
        v-for="server in filteredServers" 
        :key="server.id" 
        class="server-card"
        :class="{ 'offline': server.status === 'offline' }"
      >
        <div class="server-header">
          <div class="server-info">
            <h4>{{ server.name }}</h4>
            <p class="server-ip">{{ server.ip }}:{{ server.port }}</p>
          </div>
          <div class="server-status">
            <div class="status-indicator" :class="server.status">
              <div class="status-dot"></div>
              <span>{{ getStatusText(server.status) }}</span>
            </div>
          </div>
        </div>

        <div class="server-metrics">
          <div class="metric">
            <div class="metric-label">CPU使用率</div>
            <div class="metric-bar">
              <div class="metric-fill" :style="{ width: server.cpuUsage + '%' }"></div>
            </div>
            <div class="metric-value">{{ server.cpuUsage }}%</div>
          </div>
          <div class="metric">
            <div class="metric-label">内存使用率</div>
            <div class="metric-bar">
              <div class="metric-fill" :style="{ width: server.memoryUsage + '%' }"></div>
            </div>
            <div class="metric-value">{{ server.memoryUsage }}%</div>
          </div>
          <div class="metric">
            <div class="metric-label">网络流量</div>
            <div class="metric-value">{{ server.networkTraffic }} MB/s</div>
          </div>
        </div>

        <div class="server-details">
          <div class="detail-item">
            <span class="detail-label">运行时间:</span>
            <span class="detail-value">{{ server.uptime }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">协议数量:</span>
            <span class="detail-value">{{ server.protocolCount }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">设备数量:</span>
            <span class="detail-value">{{ server.deviceCount }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">最后更新:</span>
            <span class="detail-value">{{ server.lastUpdate }}</span>
          </div>
        </div>

        <div class="server-actions">
          <button class="btn-icon" @click="viewServerDetails(server)" title="查看详情">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z" fill="currentColor"/>
            </svg>
          </button>
          <button class="btn-icon" @click="restartServer(server)" title="重启服务器">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
            </svg>
          </button>
          <button class="btn-icon" @click="configureServer(server)" title="配置服务器">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19.14,12.94c0.04-0.3,0.06-0.61,0.06-0.94c0-0.32-0.02-0.64-0.07-0.94l2.03-1.58c0.18-0.14,0.23-0.41,0.12-0.61 l-1.92-3.32c-0.12-0.22-0.37-0.29-0.59-0.22l-2.39,0.96c-0.5-0.38-1.03-0.7-1.62-0.94L14.4,2.81c-0.04-0.24-0.24-0.41-0.48-0.41 h-3.84c-0.24,0-0.43,0.17-0.47,0.41L9.25,5.35C8.66,5.59,8.12,5.92,7.63,6.29L5.24,5.33c-0.22-0.08-0.47,0-0.59,0.22L2.74,8.87 C2.62,9.08,2.66,9.34,2.86,9.48l2.03,1.58C4.84,11.36,4.8,11.69,4.8,12s0.02,0.64,0.07,0.94l-2.03,1.58 c-0.18,0.14-0.23,0.41-0.12,0.61l1.92,3.32c0.12,0.22,0.37,0.29,0.59,0.22l2.39-0.96c0.5,0.38,1.03,0.7,1.62,0.94l0.36,2.54 c0.05,0.24,0.24,0.41,0.48,0.41h3.84c0.24,0,0.44-0.17,0.47-0.41l0.36-2.54c0.59-0.24,1.13-0.56,1.62-0.94l2.39,0.96 c0.22,0.08,0.47,0,0.59-0.22l1.92-3.32c0.12-0.22,0.07-0.47-0.12-0.61L19.14,12.94z M12,15.6c-1.98,0-3.6-1.62-3.6-3.6 s1.62-3.6,3.6-3.6s3.6,1.62,3.6,3.6S13.98,15.6,12,15.6z" fill="currentColor"/>
            </svg>
          </button>
          <button class="btn-icon danger" @click="removeServer(server)" title="移除服务器">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z" fill="currentColor"/>
            </svg>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

// 搜索和筛选
const searchQuery = ref('')
const statusFilter = ref('')
const sortBy = ref('name')

// 模拟服务器数据
const servers = ref([
  {
    id: 1,
    name: 'BYWG-Server-01',
    ip: '192.168.1.100',
    port: 5000,
    status: 'online',
    cpuUsage: 45,
    memoryUsage: 62,
    networkTraffic: 12.5,
    uptime: '15天 8小时',
    protocolCount: 8,
    deviceCount: 156,
    lastUpdate: '2分钟前'
  },
  {
    id: 2,
    name: 'BYWG-Server-02',
    ip: '192.168.1.101',
    port: 5000,
    status: 'online',
    cpuUsage: 38,
    memoryUsage: 45,
    networkTraffic: 8.2,
    uptime: '12天 3小时',
    protocolCount: 6,
    deviceCount: 98,
    lastUpdate: '1分钟前'
  },
  {
    id: 3,
    name: 'BYWG-Server-03',
    ip: '192.168.1.102',
    port: 5000,
    status: 'offline',
    cpuUsage: 0,
    memoryUsage: 0,
    networkTraffic: 0,
    uptime: '0天 0小时',
    protocolCount: 0,
    deviceCount: 0,
    lastUpdate: '1小时前'
  },
  {
    id: 4,
    name: 'BYWG-Server-04',
    ip: '192.168.1.103',
    port: 5000,
    status: 'maintenance',
    cpuUsage: 15,
    memoryUsage: 25,
    networkTraffic: 2.1,
    uptime: '3天 12小时',
    protocolCount: 2,
    deviceCount: 45,
    lastUpdate: '30分钟前'
  }
])

// 计算属性
const filteredServers = computed(() => {
  let filtered = servers.value

  // 搜索过滤
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(server => 
      server.name.toLowerCase().includes(query) || 
      server.ip.includes(query)
    )
  }

  // 状态过滤
  if (statusFilter.value) {
    filtered = filtered.filter(server => server.status === statusFilter.value)
  }

  // 排序
  filtered.sort((a, b) => {
    switch (sortBy.value) {
      case 'name':
        return a.name.localeCompare(b.name)
      case 'status':
        return a.status.localeCompare(b.status)
      case 'cpu':
        return b.cpuUsage - a.cpuUsage
      case 'memory':
        return b.memoryUsage - a.memoryUsage
      default:
        return 0
    }
  })

  return filtered
})

const serverStats = computed(() => {
  const total = servers.value.length
  const online = servers.value.filter(s => s.status === 'online').length
  const offline = servers.value.filter(s => s.status === 'offline').length
  const avgCpu = Math.round(servers.value.reduce((sum, s) => sum + s.cpuUsage, 0) / total)

  return { total, online, offline, avgCpu }
})

// 方法
function getStatusText(status: string) {
  const statusMap: Record<string, string> = {
    online: '在线',
    offline: '离线',
    maintenance: '维护中'
  }
  return statusMap[status] || '未知'
}

function refreshServers() {
  // 模拟刷新数据
  console.log('刷新服务器列表...')
}

function addServer() {
  console.log('添加新服务器...')
}

function viewServerDetails(server: any) {
  console.log('查看服务器详情:', server.name)
}

function restartServer(server: any) {
  console.log('重启服务器:', server.name)
}

function configureServer(server: any) {
  console.log('配置服务器:', server.name)
}

function removeServer(server: any) {
  console.log('移除服务器:', server.name)
}
</script>

<style scoped>
.servers-container {
  display: flex;
  flex-direction: column;
  gap: 24px;
  width: 100%;
  max-width: 100%;
  min-height: calc(100vh - 120px);
}

.stats-overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.stat-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: 12px;
  color: white;
}

.stat-icon.servers {
  background: linear-gradient(135deg, #4a90e2, #357abd);
}

.stat-icon.online {
  background: linear-gradient(135deg, #27ae60, #2ecc71);
}

.stat-icon.offline {
  background: linear-gradient(135deg, #e74c3c, #c0392b);
}

.stat-icon.performance {
  background: linear-gradient(135deg, #f39c12, #e67e22);
}

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: #6c757d;
  font-weight: 500;
}

.management-panel {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}

.panel-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
}

.panel-actions {
  display: flex;
  gap: 12px;
}

.btn-primary, .btn-secondary {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border: none;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 14px;
}

.btn-primary {
  background: linear-gradient(135deg, #4a90e2, #357abd);
  color: white;
  box-shadow: 0 4px 12px rgba(74, 144, 226, 0.3);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(74, 144, 226, 0.4);
}

.btn-secondary {
  background: #f8f9fa;
  color: #6c757d;
  border: 1px solid #dee2e6;
}

.btn-secondary:hover {
  background: #e9ecef;
  color: #495057;
}

.filter-section {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px 24px;
  background: #f8f9fa;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
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
  width: 20px;
  height: 20px;
}

.search-input {
  width: 100%;
  padding: 10px 12px 10px 40px;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  font-size: 14px;
  background: white;
  transition: all 0.3s ease;
}

.search-input:focus {
  outline: none;
  border-color: #4a90e2;
  box-shadow: 0 0 0 3px rgba(74, 144, 226, 0.1);
}

.filter-controls {
  display: flex;
  gap: 12px;
}

.filter-select {
  padding: 10px 12px;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  font-size: 14px;
  background: white;
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-select:focus {
  outline: none;
  border-color: #4a90e2;
  box-shadow: 0 0 0 3px rgba(74, 144, 226, 0.1);
}

.servers-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 20px;
}

.server-card {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  padding: 20px;
  transition: all 0.3s ease;
}

.server-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.server-card.offline {
  opacity: 0.6;
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
}

.server-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.server-info h4 {
  margin: 0 0 4px;
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
}

.server-ip {
  margin: 0;
  font-size: 14px;
  color: #6c757d;
}

.server-status {
  display: flex;
  align-items: center;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
}

.status-indicator.online {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.status-indicator.offline {
  background: rgba(231, 76, 60, 0.1);
  color: #e74c3c;
}

.status-indicator.maintenance {
  background: rgba(243, 156, 18, 0.1);
  color: #f39c12;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: currentColor;
}

.server-metrics {
  margin-bottom: 16px;
}

.metric {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.metric:last-child {
  margin-bottom: 0;
}

.metric-label {
  font-size: 12px;
  color: #6c757d;
  min-width: 80px;
}

.metric-bar {
  flex: 1;
  height: 6px;
  background: #e9ecef;
  border-radius: 3px;
  overflow: hidden;
}

.metric-fill {
  height: 100%;
  background: linear-gradient(90deg, #4a90e2, #357abd);
  border-radius: 3px;
  transition: width 0.3s ease;
}

.metric-value {
  font-size: 12px;
  font-weight: 600;
  color: #2c3e50;
  min-width: 40px;
  text-align: right;
}

.server-details {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
  margin-bottom: 16px;
  padding: 12px;
  background: #f8f9fa;
  border-radius: 8px;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  font-size: 12px;
}

.detail-label {
  color: #6c757d;
}

.detail-value {
  color: #2c3e50;
  font-weight: 500;
}

.server-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

.btn-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: 1px solid #dee2e6;
  border-radius: 6px;
  background: white;
  color: #6c757d;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-icon:hover {
  background: #f8f9fa;
  color: #495057;
  border-color: #adb5bd;
}

.btn-icon.danger:hover {
  background: #f8d7da;
  color: #721c24;
  border-color: #f5c6cb;
}

@media (max-width: 768px) {
  .servers-grid {
    grid-template-columns: 1fr;
  }
  
  .filter-section {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-box {
    max-width: none;
  }
  
  .filter-controls {
    justify-content: stretch;
  }
  
  .filter-select {
    flex: 1;
  }
}
</style>
