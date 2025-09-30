<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { listDevices } from '@/api/devices'
type Device = { status: string }

const onlineCount = ref(0)
const offlineCount = ref(0)
const totalCount = ref(0)
const systemStatus = ref('正常')
const cpuUsage = ref(45)
const memoryUsage = ref(68)
const networkTraffic = ref(1250)
const lastUpdate = ref(new Date())
const alerts = ref([
  { id: 1, level: 'warning', message: '设备 192.168.1.101 连接超时', time: '2分钟前' },
  { id: 2, level: 'info', message: '系统自动备份完成', time: '5分钟前' },
  { id: 3, level: 'error', message: '协议服务异常重启', time: '10分钟前' }
])

let refreshInterval: number | null = null

async function refreshSummary() {
  try {
    const list = await listDevices() as Device[]
    totalCount.value = list.length
    onlineCount.value = list.filter(d => d.status === 'online').length
    offlineCount.value = totalCount.value - onlineCount.value
    lastUpdate.value = new Date()
    
    // 模拟系统状态更新
    cpuUsage.value = Math.floor(Math.random() * 30) + 30
    memoryUsage.value = Math.floor(Math.random() * 20) + 60
    networkTraffic.value = Math.floor(Math.random() * 500) + 1000
  } catch (err) {
    console.error('获取设备列表失败：', err)
    totalCount.value = 0
    onlineCount.value = 0
    offlineCount.value = 0
  }
}

onMounted(() => { 
  refreshSummary()
  // 每30秒自动刷新数据
  refreshInterval = setInterval(refreshSummary, 30000)
})

onUnmounted(() => {
  if (refreshInterval) {
    clearInterval(refreshInterval)
  }
})
</script>

<template>
  <div class="dashboard">
    <!-- 系统概览卡片 -->
    <div class="overview-cards">
      <div class="card device-status">
        <div class="card-header">
          <div class="card-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
            </svg>
          </div>
          <h3>设备状态</h3>
        </div>
        <div class="card-content">
          <div class="stat-row">
            <div class="stat-item">
              <span class="stat-label">在线设备</span>
              <span class="stat-value online">{{ onlineCount }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">离线设备</span>
              <span class="stat-value offline">{{ offlineCount }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">总设备数</span>
              <span class="stat-value total">{{ totalCount }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="card system-performance">
        <div class="card-header">
          <div class="card-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z" fill="currentColor"/>
            </svg>
          </div>
          <h3>系统性能</h3>
        </div>
        <div class="card-content">
          <div class="performance-item">
            <div class="performance-label">
              <span>CPU 使用率</span>
              <span class="performance-value">{{ cpuUsage }}%</span>
            </div>
            <div class="progress-bar">
              <div class="progress-fill" :style="{ width: cpuUsage + '%' }"></div>
            </div>
          </div>
          <div class="performance-item">
            <div class="performance-label">
              <span>内存使用率</span>
              <span class="performance-value">{{ memoryUsage }}%</span>
            </div>
            <div class="progress-bar">
              <div class="progress-fill" :style="{ width: memoryUsage + '%' }"></div>
            </div>
          </div>
          <div class="performance-item">
            <div class="performance-label">
              <span>网络流量</span>
              <span class="performance-value">{{ networkTraffic }} KB/s</span>
            </div>
            <div class="progress-bar">
              <div class="progress-fill network" :style="{ width: Math.min(networkTraffic / 20, 100) + '%' }"></div>
            </div>
          </div>
        </div>
      </div>

      <div class="card system-status">
        <div class="card-header">
          <div class="card-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
            </svg>
          </div>
          <h3>系统状态</h3>
        </div>
        <div class="card-content">
          <div class="status-indicator">
            <div class="status-dot online"></div>
            <span class="status-text">{{ systemStatus }}</span>
          </div>
          <div class="last-update">
            最后更新: {{ lastUpdate.toLocaleTimeString() }}
          </div>
        </div>
      </div>
    </div>

    <!-- 协议状态和报警信息 -->
    <div class="content-grid">
      <div class="panel protocol-status">
        <div class="panel-header">
          <h3>协议状态</h3>
          <div class="protocol-actions">
            <button class="btn-refresh" @click="refreshSummary">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
              </svg>
            </button>
          </div>
        </div>
        <div class="protocol-list">
          <div class="protocol-item">
            <div class="protocol-info">
              <span class="protocol-name">Modbus TCP</span>
              <span class="protocol-status online">运行中</span>
            </div>
            <div class="protocol-stats">
              <span>连接数: 3</span>
              <span>数据点: 156</span>
            </div>
          </div>
          <div class="protocol-item">
            <div class="protocol-info">
              <span class="protocol-name">OPC UA</span>
              <span class="protocol-status online">运行中</span>
            </div>
            <div class="protocol-stats">
              <span>连接数: 2</span>
              <span>数据点: 89</span>
            </div>
          </div>
          <div class="protocol-item">
            <div class="protocol-info">
              <span class="protocol-name">EtherNet/IP</span>
              <span class="protocol-status offline">已停止</span>
            </div>
            <div class="protocol-stats">
              <span>连接数: 0</span>
              <span>数据点: 0</span>
            </div>
          </div>
        </div>
      </div>

      <div class="panel alerts-panel">
        <div class="panel-header">
          <h3>系统报警</h3>
          <span class="alert-count">{{ alerts.length }}</span>
        </div>
        <div class="alerts-list">
          <div v-for="alert in alerts" :key="alert.id" :class="['alert-item', alert.level]">
            <div class="alert-icon">
              <svg v-if="alert.level === 'error'" width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
              </svg>
              <svg v-else-if="alert.level === 'warning'" width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z" fill="currentColor"/>
              </svg>
              <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
              </svg>
            </div>
            <div class="alert-content">
              <div class="alert-message">{{ alert.message }}</div>
              <div class="alert-time">{{ alert.time }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard {
  display: flex;
  flex-direction: column;
  gap: 24px;
  width: 100%;
  max-width: 100%;
  min-height: calc(100vh - 120px);
}

.overview-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
  margin-bottom: 20px;
}

.card {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  backdrop-filter: blur(10px);
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #4a90e2, #357abd);
}

.card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.15);
}

.card-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
}

.card-icon {
  width: 48px;
  height: 48px;
  background: linear-gradient(135deg, #4a90e2, #357abd);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  box-shadow: 0 4px 16px rgba(74, 144, 226, 0.3);
}

.card-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
}

.card-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.stat-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 16px;
  background: rgba(74, 144, 226, 0.05);
  border-radius: 12px;
  border: 1px solid rgba(74, 144, 226, 0.1);
}

.stat-label {
  font-size: 12px;
  color: #6c757d;
  margin-bottom: 8px;
  font-weight: 500;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #2c3e50;
}

.stat-value.online { color: #28a745; }
.stat-value.offline { color: #dc3545; }
.stat-value.total { color: #4a90e2; }

.performance-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.performance-label {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 14px;
  color: #495057;
}

.performance-value {
  font-weight: 600;
  color: #4a90e2;
}

.progress-bar {
  height: 8px;
  background: rgba(74, 144, 226, 0.1);
  border-radius: 4px;
  overflow: hidden;
  position: relative;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #4a90e2, #357abd);
  border-radius: 4px;
  transition: width 0.3s ease;
  position: relative;
}

.progress-fill::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.3), transparent);
  animation: shimmer 2s infinite;
}

.progress-fill.network {
  background: linear-gradient(90deg, #28a745, #20c997);
}

@keyframes shimmer {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(100%); }
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.status-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  animation: pulse 2s infinite;
}

.status-dot.online {
  background: #28a745;
  box-shadow: 0 0 10px rgba(40, 167, 69, 0.5);
}

.status-text {
  font-size: 16px;
  font-weight: 600;
  color: #28a745;
}

.last-update {
  font-size: 12px;
  color: #6c757d;
}

.content-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
}

.panel {
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
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 2px solid rgba(74, 144, 226, 0.1);
}

.panel-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
}

.protocol-actions {
  display: flex;
  gap: 8px;
}

.btn-refresh {
  width: 36px;
  height: 36px;
  border: none;
  background: linear-gradient(135deg, #4a90e2, #357abd);
  color: white;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px rgba(74, 144, 226, 0.3);
}

.btn-refresh:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(74, 144, 226, 0.4);
}

.protocol-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.protocol-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: rgba(74, 144, 226, 0.05);
  border-radius: 12px;
  border: 1px solid rgba(74, 144, 226, 0.1);
  transition: all 0.3s ease;
}

.protocol-item:hover {
  background: rgba(74, 144, 226, 0.1);
  transform: translateX(4px);
}

.protocol-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.protocol-name {
  font-weight: 600;
  color: #2c3e50;
  font-size: 14px;
}

.protocol-status {
  font-size: 12px;
  padding: 4px 8px;
  border-radius: 12px;
  font-weight: 500;
}

.protocol-status.online {
  background: rgba(40, 167, 69, 0.1);
  color: #28a745;
}

.protocol-status.offline {
  background: rgba(220, 53, 69, 0.1);
  color: #dc3545;
}

.protocol-stats {
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-size: 12px;
  color: #6c757d;
  text-align: right;
}

.alert-count {
  background: #dc3545;
  color: white;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

.alerts-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: 300px;
  overflow-y: auto;
}

.alert-item {
  display: flex;
  gap: 12px;
  padding: 16px;
  border-radius: 12px;
  border-left: 4px solid;
  transition: all 0.3s ease;
}

.alert-item.error {
  background: rgba(220, 53, 69, 0.05);
  border-left-color: #dc3545;
}

.alert-item.warning {
  background: rgba(255, 193, 7, 0.05);
  border-left-color: #ffc107;
}

.alert-item.info {
  background: rgba(23, 162, 184, 0.05);
  border-left-color: #17a2b8;
}

.alert-item:hover {
  transform: translateX(4px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.alert-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 8px;
  flex-shrink: 0;
}

.alert-item.error .alert-icon {
  background: rgba(220, 53, 69, 0.1);
  color: #dc3545;
}

.alert-item.warning .alert-icon {
  background: rgba(255, 193, 7, 0.1);
  color: #ffc107;
}

.alert-item.info .alert-icon {
  background: rgba(23, 162, 184, 0.1);
  color: #17a2b8;
}

.alert-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.alert-message {
  font-size: 14px;
  color: #2c3e50;
  font-weight: 500;
}

.alert-time {
  font-size: 12px;
  color: #6c757d;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .overview-cards {
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  }
  
  .content-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .overview-cards {
    grid-template-columns: 1fr;
    gap: 16px;
  }
  
  .stat-row {
    grid-template-columns: 1fr;
    gap: 12px;
  }
  
  .card {
    padding: 20px;
  }
  
  .panel {
    padding: 20px;
  }
}

@media (max-width: 480px) {
  .card {
    padding: 16px;
  }
  
  .panel {
    padding: 16px;
  }
  
  .stat-item {
    padding: 12px;
  }
}
</style>


