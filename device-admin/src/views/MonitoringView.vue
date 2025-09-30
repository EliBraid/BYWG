<template>
  <div class="monitoring-container">
    <!-- 监控头部 -->
    <div class="monitoring-header">
      <div class="header-left">
        <h1>实时监控</h1>
        <p>工业边缘网关系统实时状态监控</p>
      </div>
      <div class="header-right">
        <div class="status-indicators">
          <div class="status-item">
            <div class="status-dot online"></div>
            <span>系统在线</span>
          </div>
          <div class="status-item">
            <div class="status-dot warning"></div>
            <span>{{ warningCount }} 个警告</span>
          </div>
          <div class="status-item">
            <div class="status-dot error"></div>
            <span>{{ errorCount }} 个错误</span>
          </div>
        </div>
        <div class="refresh-controls">
          <button @click="toggleAutoRefresh" class="refresh-btn" :class="{ active: autoRefresh }">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M1 4v6h6" stroke="currentColor" stroke-width="2"/>
              <path d="M23 20v-6h-6" stroke="currentColor" stroke-width="2"/>
              <path d="M20.49 9A9 9 0 0 0 5.64 5.64L1 10m22 4l-4.64 4.36A9 9 0 0 1 3.51 15" stroke="currentColor" stroke-width="2"/>
            </svg>
            {{ autoRefresh ? '自动刷新' : '手动刷新' }}
          </button>
          <button @click="refreshData" class="refresh-btn">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M23 4v6h-6" stroke="currentColor" stroke-width="2"/>
              <path d="M1 20v-6h6" stroke="currentColor" stroke-width="2"/>
              <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64-4.36A9 9 0 0 1 20.49 15" stroke="currentColor" stroke-width="2"/>
            </svg>
            刷新
          </button>
        </div>
      </div>
    </div>

    <!-- 系统概览 -->
    <div class="overview-section">
      <div class="overview-cards">
        <div class="overview-card">
          <div class="card-header">
            <div class="card-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect x="2" y="3" width="20" height="14" rx="2" stroke="currentColor" stroke-width="2"/>
                <path d="M8 21h8" stroke="currentColor" stroke-width="2"/>
              </svg>
            </div>
            <h3>设备状态</h3>
          </div>
          <div class="card-content">
            <div class="metric">
              <span class="metric-label">在线设备</span>
              <span class="metric-value online">{{ onlineDevices }}</span>
            </div>
            <div class="metric">
              <span class="metric-label">离线设备</span>
              <span class="metric-value offline">{{ offlineDevices }}</span>
            </div>
            <div class="metric">
              <span class="metric-label">总设备数</span>
              <span class="metric-value">{{ totalDevices }}</span>
            </div>
          </div>
        </div>

        <div class="overview-card">
          <div class="card-header">
            <div class="card-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
              </svg>
            </div>
            <h3>系统性能</h3>
          </div>
          <div class="card-content">
            <div class="metric">
              <span class="metric-label">CPU使用率</span>
              <div class="metric-bar">
                <div class="metric-fill" :style="{ width: cpuUsage + '%' }"></div>
              </div>
              <span class="metric-value">{{ cpuUsage }}%</span>
            </div>
            <div class="metric">
              <span class="metric-label">内存使用率</span>
              <div class="metric-bar">
                <div class="metric-fill" :style="{ width: memoryUsage + '%' }"></div>
              </div>
              <span class="metric-value">{{ memoryUsage }}%</span>
            </div>
            <div class="metric">
              <span class="metric-label">网络流量</span>
              <span class="metric-value">{{ networkTraffic }} MB/s</span>
            </div>
          </div>
        </div>

        <div class="overview-card">
          <div class="card-header">
            <div class="card-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M9 19c-5 0-9-4-9-9s4-9 9-9 9 4 9 9-4 9-9 9zM21 3l-3 3" stroke="currentColor" stroke-width="2"/>
              </svg>
            </div>
            <h3>协议状态</h3>
          </div>
          <div class="card-content">
            <div class="metric">
              <span class="metric-label">活跃协议</span>
              <span class="metric-value">{{ activeProtocols }}</span>
            </div>
            <div class="metric">
              <span class="metric-label">数据点</span>
              <span class="metric-value">{{ dataPoints }}</span>
            </div>
            <div class="metric">
              <span class="metric-label">更新频率</span>
              <span class="metric-value">{{ updateFrequency }}ms</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 实时数据流 -->
    <div class="data-stream-section">
      <div class="section-header">
        <h2>实时数据流</h2>
        <div class="stream-controls">
          <select v-model="selectedDevice" class="device-selector">
            <option value="">所有设备</option>
            <option v-for="device in devices" :key="device.id" :value="device.id">
              {{ device.name }}
            </option>
          </select>
          <button @click="clearData" class="clear-btn">清空数据</button>
        </div>
      </div>
      
      <div class="data-stream">
        <div 
          v-for="(data, index) in filteredDataStream" 
          :key="index" 
          class="data-item"
          :class="data.level"
        >
          <div class="data-timestamp">{{ formatTime(data.timestamp) }}</div>
          <div class="data-device">{{ data.deviceName }}</div>
          <div class="data-tag">{{ data.tagName }}</div>
          <div class="data-value" :class="data.level">{{ data.value }}</div>
          <div class="data-unit">{{ data.unit }}</div>
        </div>
      </div>
    </div>

    <!-- 设备状态网格 -->
    <div class="devices-grid-section">
      <div class="section-header">
        <h2>设备状态监控</h2>
        <div class="grid-controls">
          <div class="view-toggle">
            <button @click="viewMode = 'grid'" :class="{ active: viewMode === 'grid' }">网格视图</button>
            <button @click="viewMode = 'list'" :class="{ active: viewMode === 'list' }">列表视图</button>
          </div>
        </div>
      </div>

      <div v-if="viewMode === 'grid'" class="devices-grid">
        <div 
          v-for="device in devices" 
          :key="device.id" 
          class="device-card"
          :class="device.status"
        >
          <div class="device-header">
            <div class="device-icon">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect x="2" y="3" width="20" height="14" rx="2" stroke="currentColor" stroke-width="2"/>
                <path d="M8 21h8" stroke="currentColor" stroke-width="2"/>
              </svg>
            </div>
            <div class="device-info">
              <h4>{{ device.name }}</h4>
              <p>{{ device.ip }}</p>
            </div>
            <div class="device-status">
              <div class="status-dot" :class="device.status"></div>
              <span>{{ getStatusText(device.status) }}</span>
            </div>
          </div>
          
          <div class="device-metrics">
            <div class="metric">
              <span class="metric-label">连接时间</span>
              <span class="metric-value">{{ device.connectionTime }}</span>
            </div>
            <div class="metric">
              <span class="metric-label">数据点</span>
              <span class="metric-value">{{ device.dataPoints }}</span>
            </div>
            <div class="metric">
              <span class="metric-label">最后更新</span>
              <span class="metric-value">{{ device.lastUpdate }}</span>
            </div>
          </div>
        </div>
      </div>

      <div v-else class="devices-list">
        <div class="list-header">
          <div class="list-cell">设备名称</div>
          <div class="list-cell">IP地址</div>
          <div class="list-cell">状态</div>
          <div class="list-cell">连接时间</div>
          <div class="list-cell">数据点</div>
          <div class="list-cell">最后更新</div>
          <div class="list-cell">操作</div>
        </div>
        <div 
          v-for="device in devices" 
          :key="device.id" 
          class="list-row"
          :class="device.status"
        >
          <div class="list-cell">
            <div class="device-name">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect x="2" y="3" width="20" height="14" rx="2" stroke="currentColor" stroke-width="2"/>
                <path d="M8 21h8" stroke="currentColor" stroke-width="2"/>
              </svg>
              {{ device.name }}
            </div>
          </div>
          <div class="list-cell">{{ device.ip }}</div>
          <div class="list-cell">
            <div class="status-indicator" :class="device.status">
              <div class="status-dot" :class="device.status"></div>
              {{ getStatusText(device.status) }}
            </div>
          </div>
          <div class="list-cell">{{ device.connectionTime }}</div>
          <div class="list-cell">{{ device.dataPoints }}</div>
          <div class="list-cell">{{ device.lastUpdate }}</div>
          <div class="list-cell">
            <button @click="viewDevice(device)" class="action-btn">查看</button>
            <button @click="configureDevice(device)" class="action-btn">配置</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'

// 响应式数据
const autoRefresh = ref(true)
const selectedDevice = ref('')
const viewMode = ref('grid')
const refreshInterval = ref<number | null>(null)

// 系统状态
const onlineDevices = ref(42)
const offlineDevices = ref(3)
const totalDevices = ref(45)
const warningCount = ref(2)
const errorCount = ref(1)

// 系统性能
const cpuUsage = ref(45)
const memoryUsage = ref(68)
const networkTraffic = ref(12.5)

// 协议状态
const activeProtocols = ref(8)
const dataPoints = ref(1250)
const updateFrequency = ref(100)

// 设备列表
const devices = ref([
  {
    id: 1,
    name: 'PLC-001',
    ip: '192.168.1.100',
    status: 'online',
    connectionTime: '15天 8小时',
    dataPoints: 45,
    lastUpdate: '1分钟前'
  },
  {
    id: 2,
    name: 'PLC-002',
    ip: '192.168.1.101',
    status: 'online',
    connectionTime: '12天 3小时',
    dataPoints: 32,
    lastUpdate: '2分钟前'
  },
  {
    id: 3,
    name: 'HMI-001',
    ip: '192.168.1.102',
    status: 'offline',
    connectionTime: '0天 0小时',
    dataPoints: 0,
    lastUpdate: '1小时前'
  },
  {
    id: 4,
    name: 'SCADA-001',
    ip: '192.168.1.103',
    status: 'warning',
    connectionTime: '3天 12小时',
    dataPoints: 12,
    lastUpdate: '30分钟前'
  }
])

// 实时数据流
const dataStream = ref([
  {
    timestamp: new Date(),
    deviceName: 'PLC-001',
    tagName: 'Temperature',
    value: '75.2',
    unit: '°C',
    level: 'normal'
  },
  {
    timestamp: new Date(Date.now() - 1000),
    deviceName: 'PLC-002',
    tagName: 'Pressure',
    value: '2.5',
    unit: 'Bar',
    level: 'warning'
  },
  {
    timestamp: new Date(Date.now() - 2000),
    deviceName: 'HMI-001',
    tagName: 'Status',
    value: 'Error',
    unit: '',
    level: 'error'
  }
])

// 计算属性
const filteredDataStream = computed(() => {
  if (!selectedDevice.value) return dataStream.value
  return dataStream.value.filter(data => 
    devices.value.find(device => device.id.toString() === selectedDevice.value)?.name === data.deviceName
  )
})

// 方法
function toggleAutoRefresh() {
  autoRefresh.value = !autoRefresh.value
  if (autoRefresh.value) {
    startAutoRefresh()
  } else {
    stopAutoRefresh()
  }
}

function startAutoRefresh() {
  if (refreshInterval.value) return
  refreshInterval.value = setInterval(() => {
    refreshData()
  }, 5000)
}

function stopAutoRefresh() {
  if (refreshInterval.value) {
    clearInterval(refreshInterval.value)
    refreshInterval.value = null
  }
}

function refreshData() {
  // 模拟数据更新
  cpuUsage.value = Math.floor(Math.random() * 30) + 30
  memoryUsage.value = Math.floor(Math.random() * 20) + 60
  networkTraffic.value = Math.floor(Math.random() * 5) + 10
  
  // 添加新的数据流条目
  const newData = {
    timestamp: new Date(),
    deviceName: devices.value[Math.floor(Math.random() * devices.value.length)].name,
    tagName: ['Temperature', 'Pressure', 'Speed', 'Voltage'][Math.floor(Math.random() * 4)],
    value: (Math.random() * 100).toFixed(1),
    unit: ['°C', 'Bar', 'RPM', 'V'][Math.floor(Math.random() * 4)],
    level: ['normal', 'warning', 'error'][Math.floor(Math.random() * 3)]
  }
  dataStream.value.unshift(newData)
  
  // 保持数据流不超过100条
  if (dataStream.value.length > 100) {
    dataStream.value = dataStream.value.slice(0, 100)
  }
}

function clearData() {
  dataStream.value = []
}

function formatTime(timestamp: Date) {
  return timestamp.toLocaleTimeString()
}

function getStatusText(status: string) {
  const statusMap: Record<string, string> = {
    online: '在线',
    offline: '离线',
    warning: '警告',
    error: '错误'
  }
  return statusMap[status] || '未知'
}

function viewDevice(device: any) {
  console.log('查看设备:', device)
}

function configureDevice(device: any) {
  console.log('配置设备:', device)
}

// 生命周期
onMounted(() => {
  if (autoRefresh.value) {
    startAutoRefresh()
  }
})

onUnmounted(() => {
  stopAutoRefresh()
})
</script>

<style scoped>
.monitoring-container {
  padding: 24px;
  background: #f8f9fa;
  min-height: 100vh;
}

.monitoring-header {
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

.status-indicators {
  display: flex;
  gap: 24px;
}

.status-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #6c757d;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.status-dot.online {
  background: #27ae60;
  box-shadow: 0 0 8px rgba(39, 174, 96, 0.5);
}

.status-dot.warning {
  background: #f39c12;
  box-shadow: 0 0 8px rgba(243, 156, 18, 0.5);
}

.status-dot.error {
  background: #e74c3c;
  box-shadow: 0 0 8px rgba(231, 76, 60, 0.5);
}

.refresh-controls {
  display: flex;
  gap: 12px;
}

.refresh-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: #ffffff;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  font-size: 14px;
  color: #6c757d;
  cursor: pointer;
  transition: all 0.3s ease;
}

.refresh-btn:hover {
  background: #f8f9fa;
  border-color: #00d4ff;
  color: #00d4ff;
}

.refresh-btn.active {
  background: #00d4ff;
  color: white;
  border-color: #00d4ff;
}

.overview-section {
  margin-bottom: 32px;
}

.overview-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 24px;
}

.overview-card {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  padding: 24px;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
}

.card-icon {
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, #00d4ff 0%, #0099cc 100%);
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.card-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.card-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.metric {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #f1f3f4;
}

.metric:last-child {
  border-bottom: none;
}

.metric-label {
  font-size: 14px;
  color: #6c757d;
  font-weight: 500;
}

.metric-value {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
}

.metric-value.online {
  color: #27ae60;
}

.metric-value.offline {
  color: #e74c3c;
}

.metric-bar {
  flex: 1;
  height: 6px;
  background: #e9ecef;
  border-radius: 3px;
  margin: 0 12px;
  overflow: hidden;
}

.metric-fill {
  height: 100%;
  background: linear-gradient(90deg, #00d4ff, #0099cc);
  border-radius: 3px;
  transition: width 0.3s ease;
}

.data-stream-section {
  margin-bottom: 32px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.section-header h2 {
  font-size: 20px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.stream-controls {
  display: flex;
  gap: 12px;
  align-items: center;
}

.device-selector {
  padding: 8px 12px;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  font-size: 14px;
  background: white;
}

.clear-btn {
  padding: 8px 16px;
  background: #e74c3c;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.clear-btn:hover {
  background: #c0392b;
}

.data-stream {
  background: #1a1a1a;
  border-radius: 8px;
  padding: 16px;
  max-height: 400px;
  overflow-y: auto;
  font-family: 'Courier New', monospace;
}

.data-item {
  display: grid;
  grid-template-columns: 120px 120px 120px 1fr 60px;
  gap: 16px;
  padding: 8px 0;
  border-bottom: 1px solid #333;
  font-size: 12px;
}

.data-item:last-child {
  border-bottom: none;
}

.data-timestamp {
  color: #666;
}

.data-device {
  color: #00d4ff;
  font-weight: 600;
}

.data-tag {
  color: #00ff88;
}

.data-value {
  color: #ffffff;
  font-weight: 600;
}

.data-value.warning {
  color: #f39c12;
}

.data-value.error {
  color: #e74c3c;
}

.data-unit {
  color: #999;
}

.devices-grid-section {
  margin-bottom: 32px;
}

.grid-controls {
  display: flex;
  gap: 12px;
  align-items: center;
}

.view-toggle {
  display: flex;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  overflow: hidden;
}

.view-toggle button {
  padding: 8px 16px;
  background: white;
  border: none;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.view-toggle button.active {
  background: #00d4ff;
  color: white;
}

.devices-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.device-card {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  padding: 20px;
  transition: all 0.3s ease;
}

.device-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.device-card.offline {
  opacity: 0.6;
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
}

.device-card.warning {
  border-left: 4px solid #f39c12;
}

.device-card.error {
  border-left: 4px solid #e74c3c;
}

.device-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.device-icon {
  width: 32px;
  height: 32px;
  background: #f8f9fa;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #6c757d;
}

.device-info h4 {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 4px;
}

.device-info p {
  font-size: 14px;
  color: #6c757d;
  margin: 0;
}

.device-status {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-left: auto;
  font-size: 12px;
  font-weight: 500;
}

.device-metrics {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.devices-list {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.list-header {
  display: grid;
  grid-template-columns: 200px 120px 100px 120px 80px 120px 120px;
  gap: 16px;
  padding: 16px 20px;
  background: #f8f9fa;
  font-weight: 600;
  color: #2c3e50;
  font-size: 14px;
}

.list-row {
  display: grid;
  grid-template-columns: 200px 120px 100px 120px 80px 120px 120px;
  gap: 16px;
  padding: 16px 20px;
  border-bottom: 1px solid #f1f3f4;
  transition: all 0.3s ease;
}

.list-row:hover {
  background: #f8f9fa;
}

.list-row.offline {
  opacity: 0.6;
}

.list-cell {
  display: flex;
  align-items: center;
  font-size: 14px;
  color: #2c3e50;
}

.device-name {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 500;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  font-weight: 500;
}

.action-btn {
  padding: 4px 8px;
  background: #00d4ff;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 12px;
  cursor: pointer;
  margin-right: 8px;
  transition: all 0.3s ease;
}

.action-btn:hover {
  background: #0099cc;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .monitoring-header {
    flex-direction: column;
    gap: 20px;
  }
  
  .header-right {
    flex-direction: column;
    gap: 16px;
  }
  
  .overview-cards {
    grid-template-columns: 1fr;
  }
  
  .devices-grid {
    grid-template-columns: 1fr;
  }
  
  .list-header,
  .list-row {
    grid-template-columns: 1fr;
    gap: 8px;
  }
  
  .data-item {
    grid-template-columns: 1fr;
    gap: 8px;
  }
}
</style>
