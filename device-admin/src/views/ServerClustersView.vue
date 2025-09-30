<template>
  <div class="clusters-container">
    <!-- 集群概览 -->
    <div class="stats-overview">
      <div class="stat-card">
        <div class="stat-icon clusters">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ clusterStats.totalClusters }}</div>
          <div class="stat-label">集群总数</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon healthy">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ clusterStats.healthyClusters }}</div>
          <div class="stat-label">健康集群</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon total-servers">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M4 6h16v2H4zm0 5h16v2H4zm0 5h16v2H4z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ clusterStats.totalServers }}</div>
          <div class="stat-label">总服务器数</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon load-balance">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ clusterStats.avgLoad }}%</div>
          <div class="stat-label">平均负载</div>
        </div>
      </div>
    </div>

    <!-- 集群管理面板 -->
    <div class="management-panel">
      <div class="panel-header">
        <h3>集群监控</h3>
        <div class="panel-actions">
          <button class="btn-secondary" @click="refreshClusters">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
            </svg>
            刷新
          </button>
          <button class="btn-primary" @click="createCluster">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z" fill="currentColor"/>
            </svg>
            创建集群
          </button>
        </div>
      </div>
    </div>

    <!-- 集群列表 -->
    <div class="clusters-grid">
      <div 
        v-for="cluster in clusters" 
        :key="cluster.id" 
        class="cluster-card"
        :class="{ 'unhealthy': cluster.status === 'unhealthy' }"
      >
        <div class="cluster-header">
          <div class="cluster-info">
            <h4>{{ cluster.name }}</h4>
            <p class="cluster-description">{{ cluster.description }}</p>
          </div>
          <div class="cluster-status">
            <div class="status-indicator" :class="cluster.status">
              <div class="status-dot"></div>
              <span>{{ getStatusText(cluster.status) }}</span>
            </div>
          </div>
        </div>

        <div class="cluster-metrics">
          <div class="metric-row">
            <div class="metric">
              <div class="metric-label">服务器数量</div>
              <div class="metric-value">{{ cluster.serverCount }}</div>
            </div>
            <div class="metric">
              <div class="metric-label">在线服务器</div>
              <div class="metric-value">{{ cluster.onlineServers }}</div>
            </div>
          </div>
          <div class="metric-row">
            <div class="metric">
              <div class="metric-label">平均CPU</div>
              <div class="metric-value">{{ cluster.avgCpu }}%</div>
            </div>
            <div class="metric">
              <div class="metric-label">平均内存</div>
              <div class="metric-value">{{ cluster.avgMemory }}%</div>
            </div>
          </div>
        </div>

        <div class="cluster-servers">
          <div class="servers-header">
            <span>服务器列表</span>
            <span class="server-count">{{ cluster.servers.length }} 台</span>
          </div>
          <div class="servers-list">
            <div 
              v-for="server in cluster.servers" 
              :key="server.id" 
              class="server-item"
              :class="{ 'offline': server.status === 'offline' }"
            >
              <div class="server-info">
                <div class="server-name">{{ server.name }}</div>
                <div class="server-ip">{{ server.ip }}</div>
              </div>
              <div class="server-status">
                <div class="status-dot" :class="server.status"></div>
                <span>{{ getServerStatusText(server.status) }}</span>
              </div>
              <div class="server-metrics">
                <div class="metric-mini">
                  <span>CPU: {{ server.cpu }}%</span>
                  <span>内存: {{ server.memory }}%</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="cluster-actions">
          <button class="btn-icon" @click="viewClusterDetails(cluster)" title="查看详情">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z" fill="currentColor"/>
            </svg>
          </button>
          <button class="btn-icon" @click="configureCluster(cluster)" title="配置集群">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19.14,12.94c0.04-0.3,0.06-0.61,0.06-0.94c0-0.32-0.02-0.64-0.07-0.94l2.03-1.58c0.18-0.14,0.23-0.41,0.12-0.61 l-1.92-3.32c-0.12-0.22-0.37-0.29-0.59-0.22l-2.39,0.96c-0.5-0.38-1.03-0.7-1.62-0.94L14.4,2.81c-0.04-0.24-0.24-0.41-0.48-0.41 h-3.84c-0.24,0-0.43,0.17-0.47,0.41L9.25,5.35C8.66,5.59,8.12,5.92,7.63,6.29L5.24,5.33c-0.22-0.08-0.47,0-0.59,0.22L2.74,8.87 C2.62,9.08,2.66,9.34,2.86,9.48l2.03,1.58C4.84,11.36,4.8,11.69,4.8,12s0.02,0.64,0.07,0.94l-2.03,1.58 c-0.18,0.14-0.23,0.41-0.12,0.61l1.92,3.32c0.12,0.22,0.37,0.29,0.59,0.22l2.39-0.96c0.5,0.38,1.03,0.7,1.62,0.94l0.36,2.54 c0.05,0.24,0.24,0.41,0.48,0.41h3.84c0.24,0,0.44-0.17,0.47-0.41l0.36-2.54c0.59-0.24,1.13-0.56,1.62-0.94l2.39,0.96 c0.22,0.08,0.47,0,0.59-0.22l1.92-3.32c0.12-0.22,0.07-0.47-0.12-0.61L19.14,12.94z M12,15.6c-1.98,0-3.6-1.62-3.6-3.6 s1.62-3.6,3.6-3.6s3.6,1.62,3.6,3.6S13.98,15.6,12,15.6z" fill="currentColor"/>
            </svg>
          </button>
          <button class="btn-icon" @click="restartCluster(cluster)" title="重启集群">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
            </svg>
          </button>
          <button class="btn-icon danger" @click="deleteCluster(cluster)" title="删除集群">
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

// 模拟集群数据
const clusters = ref([
  {
    id: 1,
    name: '生产环境集群',
    description: '主要生产环境BYWG服务器集群',
    status: 'healthy',
    serverCount: 4,
    onlineServers: 4,
    avgCpu: 42,
    avgMemory: 58,
    servers: [
      { id: 1, name: 'BYWG-Server-01', ip: '192.168.1.100', status: 'online', cpu: 45, memory: 62 },
      { id: 2, name: 'BYWG-Server-02', ip: '192.168.1.101', status: 'online', cpu: 38, memory: 45 },
      { id: 3, name: 'BYWG-Server-03', ip: '192.168.1.102', status: 'online', cpu: 48, memory: 65 },
      { id: 4, name: 'BYWG-Server-04', ip: '192.168.1.103', status: 'online', cpu: 37, memory: 60 }
    ]
  },
  {
    id: 2,
    name: '测试环境集群',
    description: '测试和开发环境服务器集群',
    status: 'healthy',
    serverCount: 2,
    onlineServers: 2,
    avgCpu: 25,
    avgMemory: 35,
    servers: [
      { id: 5, name: 'BYWG-Test-01', ip: '192.168.2.100', status: 'online', cpu: 28, memory: 38 },
      { id: 6, name: 'BYWG-Test-02', ip: '192.168.2.101', status: 'online', cpu: 22, memory: 32 }
    ]
  },
  {
    id: 3,
    name: '备用集群',
    description: '备用和容灾服务器集群',
    status: 'unhealthy',
    serverCount: 3,
    onlineServers: 1,
    avgCpu: 15,
    avgMemory: 25,
    servers: [
      { id: 7, name: 'BYWG-Backup-01', ip: '192.168.3.100', status: 'online', cpu: 18, memory: 28 },
      { id: 8, name: 'BYWG-Backup-02', ip: '192.168.3.101', status: 'offline', cpu: 0, memory: 0 },
      { id: 9, name: 'BYWG-Backup-03', ip: '192.168.3.102', status: 'offline', cpu: 0, memory: 0 }
    ]
  }
])

// 计算属性
const clusterStats = computed(() => {
  const totalClusters = clusters.value.length
  const healthyClusters = clusters.value.filter(c => c.status === 'healthy').length
  const totalServers = clusters.value.reduce((sum, c) => sum + c.serverCount, 0)
  const avgLoad = Math.round(clusters.value.reduce((sum, c) => sum + c.avgCpu, 0) / totalClusters)

  return { totalClusters, healthyClusters, totalServers, avgLoad }
})

// 方法
function getStatusText(status: string) {
  const statusMap: Record<string, string> = {
    healthy: '健康',
    unhealthy: '异常',
    maintenance: '维护中'
  }
  return statusMap[status] || '未知'
}

function getServerStatusText(status: string) {
  const statusMap: Record<string, string> = {
    online: '在线',
    offline: '离线',
    maintenance: '维护中'
  }
  return statusMap[status] || '未知'
}

function refreshClusters() {
  console.log('刷新集群列表...')
}

function createCluster() {
  console.log('创建新集群...')
}

function viewClusterDetails(cluster: any) {
  console.log('查看集群详情:', cluster.name)
}

function configureCluster(cluster: any) {
  console.log('配置集群:', cluster.name)
}

function restartCluster(cluster: any) {
  console.log('重启集群:', cluster.name)
}

function deleteCluster(cluster: any) {
  console.log('删除集群:', cluster.name)
}
</script>

<style scoped>
.clusters-container {
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

.stat-icon.clusters {
  background: linear-gradient(135deg, #4a90e2, #357abd);
}

.stat-icon.healthy {
  background: linear-gradient(135deg, #27ae60, #2ecc71);
}

.stat-icon.total-servers {
  background: linear-gradient(135deg, #9b59b6, #8e44ad);
}

.stat-icon.load-balance {
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

.clusters-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(500px, 1fr));
  gap: 20px;
}

.cluster-card {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  padding: 20px;
  transition: all 0.3s ease;
}

.cluster-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.cluster-card.unhealthy {
  border-left: 4px solid #e74c3c;
}

.cluster-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.cluster-info h4 {
  margin: 0 0 4px;
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
}

.cluster-description {
  margin: 0;
  font-size: 14px;
  color: #6c757d;
}

.cluster-status {
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

.status-indicator.healthy {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.status-indicator.unhealthy {
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

.cluster-metrics {
  margin-bottom: 16px;
}

.metric-row {
  display: flex;
  gap: 16px;
  margin-bottom: 12px;
}

.metric-row:last-child {
  margin-bottom: 0;
}

.metric {
  flex: 1;
  text-align: center;
  padding: 12px;
  background: #f8f9fa;
  border-radius: 8px;
}

.metric-label {
  font-size: 12px;
  color: #6c757d;
  margin-bottom: 4px;
}

.metric-value {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
}

.cluster-servers {
  margin-bottom: 16px;
}

.servers-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  font-size: 14px;
  font-weight: 500;
  color: #2c3e50;
}

.server-count {
  color: #6c757d;
  font-size: 12px;
}

.servers-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.server-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px;
  background: #f8f9fa;
  border-radius: 6px;
  transition: all 0.3s ease;
}

.server-item:hover {
  background: #e9ecef;
}

.server-item.offline {
  opacity: 0.6;
}

.server-info {
  flex: 1;
}

.server-name {
  font-size: 12px;
  font-weight: 500;
  color: #2c3e50;
  margin-bottom: 2px;
}

.server-ip {
  font-size: 11px;
  color: #6c757d;
}

.server-status {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 11px;
  color: #6c757d;
}

.server-metrics {
  font-size: 10px;
  color: #6c757d;
}

.metric-mini {
  display: flex;
  gap: 8px;
}

.cluster-actions {
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
  .clusters-grid {
    grid-template-columns: 1fr;
  }
  
  .metric-row {
    flex-direction: column;
    gap: 8px;
  }
  
  .server-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }
  
  .server-status {
    align-self: flex-end;
  }
}
</style>
