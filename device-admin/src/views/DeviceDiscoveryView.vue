<template>
  <div class="device-discovery-container">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-left">
        <h1>设备发现</h1>
        <p>自动扫描和发现网络中的工业设备</p>
      </div>
      <div class="header-right">
        <button @click="startScan" :disabled="isScanning" class="scan-btn">
          <svg v-if="isScanning" width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" class="spinning">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
          <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M9 19c-5 0-9-4-9-9s4-9 9-9 9 4 9 9-4 9-9 9zM21 3l-3 3" stroke="currentColor" stroke-width="2"/>
          </svg>
          {{ isScanning ? '扫描中...' : '开始扫描' }}
        </button>
        <button @click="stopScan" :disabled="!isScanning" class="stop-btn">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <rect x="6" y="6" width="12" height="12" rx="2" fill="currentColor"/>
          </svg>
          停止扫描
        </button>
        <button @click="openConfigModal" class="config-btn">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2"/>
            <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1 1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z" stroke="currentColor" stroke-width="2"/>
          </svg>
          扫描配置
        </button>
      </div>
    </div>

    <!-- 扫描状态 -->
    <div class="scan-status">
      <div class="status-cards">
        <div class="status-card">
          <div class="status-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M9 19c-5 0-9-4-9-9s4-9 9-9 9 4 9 9-4 9-9 9zM21 3l-3 3" stroke="currentColor" stroke-width="2"/>
            </svg>
          </div>
          <div class="status-content">
            <div class="status-value">{{ discoveredDevices.length }}</div>
            <div class="status-label">已发现设备</div>
          </div>
        </div>
        <div class="status-card">
          <div class="status-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect x="2" y="3" width="20" height="14" rx="2" stroke="currentColor" stroke-width="2"/>
              <path d="M8 21h8" stroke="currentColor" stroke-width="2"/>
            </svg>
          </div>
          <div class="status-content">
            <div class="status-value">{{ onlineDevices }}</div>
            <div class="status-label">在线设备</div>
          </div>
        </div>
        <div class="status-card">
          <div class="status-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
            </svg>
          </div>
          <div class="status-content">
            <div class="status-value">{{ supportedProtocols }}</div>
            <div class="status-label">支持协议</div>
          </div>
        </div>
        <div class="status-card">
          <div class="status-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
            </svg>
          </div>
          <div class="status-content">
            <div class="status-value">{{ scanProgress }}%</div>
            <div class="status-label">扫描进度</div>
          </div>
        </div>
      </div>
    </div>

    <!-- 扫描配置 -->
    <div class="scan-config">
      <div class="config-section">
        <h3>当前扫描配置</h3>
        <div class="config-info">
          <div class="config-item">
            <span class="config-label">扫描范围:</span>
            <span class="config-value">{{ scanConfig.networkRange }}</span>
          </div>
          <div class="config-item">
            <span class="config-label">扫描端口:</span>
            <span class="config-value">{{ scanConfig.ports.join(', ') }}</span>
          </div>
          <div class="config-item">
            <span class="config-label">扫描协议:</span>
            <span class="config-value">{{ scanConfig.protocols.join(', ') }}</span>
          </div>
          <div class="config-item">
            <span class="config-label">扫描超时:</span>
            <span class="config-value">{{ scanConfig.timeout }}秒</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 发现设备列表 -->
    <div class="discovered-devices">
      <div class="section-header">
        <h2>发现的设备</h2>
        <div class="device-controls">
          <div class="filter-group">
            <select v-model="selectedProtocol" class="filter-select">
              <option value="">所有协议</option>
              <option v-for="protocol in protocols" :key="protocol" :value="protocol">
                {{ protocol }}
              </option>
            </select>
          </div>
          <div class="filter-group">
            <select v-model="selectedStatus" class="filter-select">
              <option value="">所有状态</option>
              <option value="online">在线</option>
              <option value="offline">离线</option>
              <option value="unknown">未知</option>
            </select>
          </div>
          <button @click="addSelectedDevices" :disabled="selectedDevices.length === 0" class="add-btn">
            添加选中设备 ({{ selectedDevices.length }})
          </button>
          <button @click="selectAllDevices" class="select-all-btn">
            {{ allDevicesSelected ? '取消全选' : '全选' }}
          </button>
        </div>
      </div>

      <div class="devices-list">
        <div 
          v-for="device in filteredDevices" 
          :key="device.id" 
          class="device-item"
          :class="device.status"
        >
          <div class="device-checkbox">
            <input 
              type="checkbox" 
              v-model="device.selected"
              @change="updateSelectedDevices"
            />
          </div>
          <div class="device-icon">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect x="2" y="3" width="20" height="14" rx="2" stroke="currentColor" stroke-width="2"/>
              <path d="M8 21h8" stroke="currentColor" stroke-width="2"/>
            </svg>
          </div>
          <div class="device-info">
            <div class="device-header">
              <h4 class="device-name">{{ device.name }}</h4>
              <div class="device-status">
                <div class="status-dot" :class="device.status"></div>
                <span>{{ getStatusText(device.status) }}</span>
              </div>
            </div>
            <div class="device-details">
              <div class="detail-row">
                <span class="detail-label">IP地址:</span>
                <span class="detail-value">{{ device.ip }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">MAC地址:</span>
                <span class="detail-value">{{ device.mac }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">设备类型:</span>
                <span class="detail-value">{{ device.type }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">支持协议:</span>
                <span class="detail-value">{{ device.protocols.join(', ') }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">厂商:</span>
                <span class="detail-value">{{ device.vendor }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">发现时间:</span>
                <span class="detail-value">{{ formatTime(device.discoveredAt) }}</span>
              </div>
            </div>
          </div>
          <div class="device-actions">
            <button @click="testConnection(device)" class="action-btn small">
              测试连接
            </button>
            <button @click="viewDeviceDetails(device)" class="action-btn small">
              查看详情
            </button>
            <button @click="addDevice(device)" class="action-btn small success">
              添加到系统
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 扫描配置模态框 -->
    <div v-if="showConfigModal" class="modal-overlay" @click="closeConfigModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>扫描配置</h3>
          <button @click="closeConfigModal" class="close-btn">×</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="saveConfig" class="config-form">
            <div class="form-group">
              <label for="networkRange">网络范围</label>
              <input 
                id="networkRange"
                v-model="configForm.networkRange" 
                type="text" 
                class="form-input"
                placeholder="例如: 192.168.1.1/24"
                required
              />
            </div>
            <div class="form-group">
              <label for="ports">扫描端口</label>
              <input 
                id="ports"
                v-model="configForm.ports" 
                type="text" 
                class="form-input"
                placeholder="例如: 502,102,44818"
                required
              />
            </div>
            <div class="form-group">
              <label for="protocols">扫描协议</label>
              <div class="protocol-checkboxes">
                <label v-for="protocol in availableProtocols" :key="protocol" class="checkbox-label">
                  <input 
                    type="checkbox" 
                    v-model="configForm.protocols" 
                    :value="protocol"
                  />
                  {{ protocol }}
                </label>
              </div>
            </div>
            <div class="form-group">
              <label for="timeout">扫描超时 (秒)</label>
              <input 
                id="timeout"
                v-model.number="configForm.timeout" 
                type="number" 
                class="form-input"
                min="1"
                max="60"
                required
              />
            </div>
            <div class="form-group">
              <label for="threads">并发线程数</label>
              <input 
                id="threads"
                v-model.number="configForm.threads" 
                type="number" 
                class="form-input"
                min="1"
                max="100"
                required
              />
            </div>
          </form>
        </div>
        <div class="modal-footer">
          <button @click="closeConfigModal" class="modal-btn cancel">取消</button>
          <button @click="saveConfig" class="modal-btn primary">保存配置</button>
        </div>
      </div>
    </div>

    <!-- 设备详情模态框 -->
    <div v-if="selectedDevice" class="modal-overlay" @click="closeDeviceDetail">
      <div class="modal-content large" @click.stop>
        <div class="modal-header">
          <h3>{{ selectedDevice.name }} - 设备详情</h3>
          <button @click="closeDeviceDetail" class="close-btn">×</button>
        </div>
        <div class="modal-body">
          <div class="device-detail-content">
            <div class="detail-section">
              <h4>基本信息</h4>
              <div class="detail-grid">
                <div class="detail-item">
                  <span class="detail-label">设备名称:</span>
                  <span class="detail-value">{{ selectedDevice.name }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">IP地址:</span>
                  <span class="detail-value">{{ selectedDevice.ip }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">MAC地址:</span>
                  <span class="detail-value">{{ selectedDevice.mac }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">设备类型:</span>
                  <span class="detail-value">{{ selectedDevice.type }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">厂商:</span>
                  <span class="detail-value">{{ selectedDevice.vendor }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">型号:</span>
                  <span class="detail-value">{{ selectedDevice.model }}</span>
                </div>
              </div>
            </div>
            
            <div class="detail-section">
              <h4>协议支持</h4>
              <div class="protocols-list">
                <span 
                  v-for="protocol in selectedDevice.protocols" 
                  :key="protocol" 
                  class="protocol-tag"
                >
                  {{ protocol }}
                </span>
              </div>
            </div>
            
            <div class="detail-section">
              <h4>连接测试</h4>
              <div class="connection-test">
                <button @click="testConnection(selectedDevice)" class="test-btn">
                  测试连接
                </button>
                <div v-if="connectionResult" class="test-result" :class="connectionResult.success ? 'success' : 'error'">
                  {{ connectionResult.message }}
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button @click="closeDeviceDetail" class="modal-btn cancel">关闭</button>
          <button @click="addDevice(selectedDevice)" class="modal-btn primary">添加到系统</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'

// 响应式数据
const isScanning = ref(false)
const scanProgress = ref(0)
const showConfigModal = ref(false)
type DiscoveredDevice = {
  id: number;
  name: string;
  ip: string;
  mac: string;
  type: string;
  vendor: string;
  model: string;
  status: 'online' | 'offline' | 'unknown';
  protocols: string[];
  discoveredAt: Date;
  selected: boolean;
}
const selectedDevice = ref<DiscoveredDevice | null>(null)
const connectionResult = ref<{ success: boolean; message: string } | null>(null)
const selectedProtocol = ref('')
const selectedStatus = ref('')
const allDevicesSelected = ref(false)

// 扫描配置
const scanConfig = ref({
  networkRange: '192.168.1.1/24',
  ports: [502, 102, 44818, 47808],
  protocols: ['Modbus TCP', 'OPC UA', 'Ethernet/IP'],
  timeout: 5,
  threads: 10
})

const configForm = ref({
  networkRange: '192.168.1.1/24',
  ports: '502,102,44818,47808',
  protocols: ['Modbus TCP', 'OPC UA', 'Ethernet/IP'],
  timeout: 5,
  threads: 10
})

const availableProtocols = ref([
  'Modbus TCP', 'OPC UA', 'Ethernet/IP', 'Profinet', 'BACnet', 'DNP3'
])

const protocols = ref(['Modbus TCP', 'OPC UA', 'Ethernet/IP', 'Profinet', 'BACnet', 'DNP3'])

// 发现的设备
const discoveredDevices = ref<DiscoveredDevice[]>([
  {
    id: 1,
    name: 'PLC-001',
    ip: '192.168.1.100',
    mac: '00:1B:44:11:3A:B7',
    type: 'PLC',
    vendor: 'Siemens',
    model: 'S7-1200',
    status: 'online',
    protocols: ['Modbus TCP', 'OPC UA'],
    discoveredAt: new Date(),
    selected: false
  },
  {
    id: 2,
    name: 'HMI-001',
    ip: '192.168.1.101',
    mac: '00:1B:44:11:3A:B8',
    type: 'HMI',
    vendor: 'Schneider',
    model: 'Magelis XBT',
    status: 'online',
    protocols: ['Modbus TCP'],
    discoveredAt: new Date(Date.now() - 300000),
    selected: false
  },
  {
    id: 3,
    name: 'SCADA-001',
    ip: '192.168.1.102',
    mac: '00:1B:44:11:3A:B9',
    type: 'SCADA',
    vendor: 'Wonderware',
    model: 'InTouch',
    status: 'offline',
    protocols: ['OPC UA'],
    discoveredAt: new Date(Date.now() - 600000),
    selected: false
  }
])

// 计算属性
const onlineDevices = computed(() => 
  discoveredDevices.value.filter(device => device.status === 'online').length
)

const supportedProtocols = computed(() => {
  const protocols = new Set()
  discoveredDevices.value.forEach(device => {
    device.protocols.forEach(protocol => protocols.add(protocol))
  })
  return protocols.size
})

const selectedDevices = computed(() => 
  discoveredDevices.value.filter(device => device.selected)
)

const filteredDevices = computed(() => {
  let filtered = discoveredDevices.value

  if (selectedProtocol.value) {
    filtered = filtered.filter(device => 
      device.protocols.includes(selectedProtocol.value)
    )
  }

  if (selectedStatus.value) {
    filtered = filtered.filter(device => device.status === selectedStatus.value)
  }

  return filtered
})

// 方法
function startScan() {
  isScanning.value = true
  scanProgress.value = 0
  
  // 模拟扫描过程
  const interval = setInterval(() => {
    scanProgress.value += Math.random() * 10
    if (scanProgress.value >= 100) {
      scanProgress.value = 100
      isScanning.value = false
      clearInterval(interval)
      
      // 模拟发现新设备
      const newDevice: DiscoveredDevice = {
        id: Date.now(),
        name: `Device-${Math.floor(Math.random() * 1000)}`,
        ip: `192.168.1.${Math.floor(Math.random() * 254) + 1}`,
        mac: '00:1B:44:11:3A:' + Math.floor(Math.random() * 255).toString(16).padStart(2, '0'),
        type: (['PLC', 'HMI', 'SCADA', 'Sensor'][Math.floor(Math.random() * 4)]) as string,
        vendor: (['Siemens', 'Schneider', 'Rockwell', 'ABB'][Math.floor(Math.random() * 4)]) as string,
        model: 'Model-' + Math.floor(Math.random() * 1000),
        status: (Math.random() > 0.3 ? 'online' : 'offline') as 'online' | 'offline' | 'unknown',
        protocols: ['Modbus TCP', 'OPC UA', 'Ethernet/IP'].slice(0, Math.floor(Math.random() * 3) + 1),
        discoveredAt: new Date(),
        selected: false
      }
      discoveredDevices.value.push(newDevice)
    }
  }, 500)
}

function stopScan() {
  isScanning.value = false
  scanProgress.value = 0
}

function testConnection(device: DiscoveredDevice) {
  // 模拟连接测试
  const success = Math.random() > 0.3
  connectionResult.value = {
    success,
    message: success ? '连接成功' : '连接失败'
  }
  
  if (success) {
    device.status = 'online'
  }
}

function viewDeviceDetails(device: DiscoveredDevice) {
  selectedDevice.value = device
}

function closeDeviceDetail() {
  selectedDevice.value = null
  connectionResult.value = null
}

function addDevice(device: DiscoveredDevice) {
  // 实现添加设备到系统的逻辑
  console.log('添加设备到系统:', device)
  alert(`设备 ${device.name} 已添加到系统`)
}

function addSelectedDevices() {
  if (selectedDevices.value.length > 0) {
    console.log('添加选中的设备:', selectedDevices.value)
    alert(`已添加 ${selectedDevices.value.length} 个设备到系统`)
    // 清除选择
    selectedDevices.value.forEach(device => {
      device.selected = false
    })
  }
}

function selectAllDevices() {
  allDevicesSelected.value = !allDevicesSelected.value
  discoveredDevices.value.forEach(device => {
    device.selected = allDevicesSelected.value
  })
}

function updateSelectedDevices() {
  const selectedCount = discoveredDevices.value.filter(device => device.selected).length
  allDevicesSelected.value = selectedCount === discoveredDevices.value.length
}

function openConfigModal() {
  showConfigModal.value = true
  // 同步配置到表单
  configForm.value = {
    networkRange: scanConfig.value.networkRange,
    ports: scanConfig.value.ports.join(','),
    protocols: [...scanConfig.value.protocols],
    timeout: scanConfig.value.timeout,
    threads: scanConfig.value.threads
  }
}

function closeConfigModal() {
  showConfigModal.value = false
}

function saveConfig() {
  scanConfig.value = {
    networkRange: configForm.value.networkRange,
    ports: configForm.value.ports.split(',').map(p => parseInt(p.trim())),
    protocols: [...configForm.value.protocols],
    timeout: configForm.value.timeout,
    threads: configForm.value.threads
  }
  closeConfigModal()
}

function getStatusText(status: string) {
  const statusMap: Record<string, string> = {
    online: '在线',
    offline: '离线',
    unknown: '未知'
  }
  return statusMap[status] || status
}

function formatTime(date: Date) {
  return date.toLocaleString()
}

// 生命周期
onMounted(() => {
  // 初始化数据
})

onUnmounted(() => {
  // 清理资源
})
</script>

<style scoped>
.device-discovery-container {
  padding: 24px;
  background: #f8f9fa;
  min-height: 100vh;
}

.page-header {
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
  gap: 12px;
}

.scan-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 20px;
  background: #00d4ff;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.scan-btn:hover:not(:disabled) {
  background: #0099cc;
  transform: translateY(-1px);
}

.scan-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.stop-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 20px;
  background: #e74c3c;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.stop-btn:hover:not(:disabled) {
  background: #c0392b;
}

.stop-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.config-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 20px;
  background: white;
  color: #6c757d;
  border: 1px solid #e9ecef;
  border-radius: 8px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.config-btn:hover {
  background: #f8f9fa;
  border-color: #00d4ff;
  color: #00d4ff;
}

.spinning {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.scan-status {
  margin-bottom: 32px;
}

.status-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
}

.status-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
}

.status-icon {
  width: 48px;
  height: 48px;
  background: linear-gradient(135deg, #00d4ff 0%, #0099cc 100%);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.status-content {
  display: flex;
  flex-direction: column;
}

.status-value {
  font-size: 24px;
  font-weight: 700;
  color: #2c3e50;
}

.status-label {
  font-size: 14px;
  color: #6c757d;
}

.scan-config {
  margin-bottom: 32px;
  padding: 20px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.config-section h3 {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 16px;
}

.config-info {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.config-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.config-label {
  font-size: 12px;
  color: #6c757d;
  font-weight: 500;
}

.config-value {
  font-size: 14px;
  color: #2c3e50;
  font-weight: 600;
}

.discovered-devices {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  background: #f8f9fa;
  border-bottom: 1px solid #e9ecef;
}

.section-header h2 {
  font-size: 20px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.device-controls {
  display: flex;
  align-items: center;
  gap: 16px;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.filter-select {
  padding: 8px 12px;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  font-size: 14px;
  min-width: 120px;
}

.add-btn {
  padding: 8px 16px;
  background: #27ae60;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.add-btn:hover:not(:disabled) {
  background: #229954;
}

.add-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.select-all-btn {
  padding: 8px 16px;
  background: #6c757d;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.select-all-btn:hover {
  background: #5a6268;
}

.devices-list {
  max-height: 600px;
  overflow-y: auto;
}

.device-item {
  display: flex;
  align-items: center;
  padding: 16px 20px;
  border-bottom: 1px solid #f1f3f4;
  transition: all 0.3s ease;
}

.device-item:hover {
  background: #f8f9fa;
}

.device-item.online {
  border-left: 4px solid #27ae60;
}

.device-item.offline {
  border-left: 4px solid #e74c3c;
}

.device-checkbox {
  margin-right: 16px;
}

.device-icon {
  width: 40px;
  height: 40px;
  background: #f8f9fa;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #6c757d;
  margin-right: 16px;
}

.device-info {
  flex: 1;
}

.device-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.device-name {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.device-status {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  font-weight: 500;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.status-dot.online {
  background: #27ae60;
}

.status-dot.offline {
  background: #e74c3c;
}

.device-details {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 8px;
}

.detail-row {
  display: flex;
  gap: 8px;
  font-size: 12px;
}

.detail-label {
  color: #6c757d;
  font-weight: 500;
  min-width: 60px;
}

.detail-value {
  color: #2c3e50;
  font-weight: 500;
}

.device-actions {
  display: flex;
  gap: 8px;
  margin-left: 16px;
}

.action-btn {
  padding: 6px 12px;
  background: #00d4ff;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.action-btn:hover {
  background: #0099cc;
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
  max-width: 500px;
  width: 90%;
  max-height: 80vh;
  overflow-y: auto;
}

.modal-content.large {
  max-width: 800px;
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

.config-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-size: 14px;
  font-weight: 500;
  color: #2c3e50;
}

.form-input {
  padding: 10px 12px;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  font-size: 14px;
  transition: all 0.3s ease;
}

.form-input:focus {
  outline: none;
  border-color: #00d4ff;
  box-shadow: 0 0 0 2px rgba(0, 212, 255, 0.1);
}

.protocol-checkboxes {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 8px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #2c3e50;
  cursor: pointer;
}

.device-detail-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.detail-section {
  border: 1px solid #e9ecef;
  border-radius: 8px;
  padding: 16px;
}

.detail-section h4 {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 12px;
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 12px;
}

.detail-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.protocols-list {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.protocol-tag {
  padding: 4px 8px;
  background: rgba(0, 212, 255, 0.1);
  color: #00d4ff;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.connection-test {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.test-btn {
  padding: 8px 16px;
  background: #00d4ff;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
  align-self: flex-start;
}

.test-btn:hover {
  background: #0099cc;
}

.test-result {
  padding: 8px 12px;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
}

.test-result.success {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.test-result.error {
  background: rgba(231, 76, 60, 0.1);
  color: #e74c3c;
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
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.modal-btn.cancel {
  background: #6c757d;
  color: white;
}

.modal-btn.cancel:hover {
  background: #5a6268;
}

.modal-btn.primary {
  background: #00d4ff;
  color: white;
}

.modal-btn.primary:hover {
  background: #0099cc;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    gap: 20px;
  }
  
  .header-right {
    flex-direction: column;
    width: 100%;
  }
  
  .status-cards {
    grid-template-columns: 1fr;
  }
  
  .config-info {
    grid-template-columns: 1fr;
  }
  
  .device-controls {
    flex-direction: column;
    gap: 12px;
  }
  
  .device-details {
    grid-template-columns: 1fr;
  }
  
  .device-actions {
    flex-direction: column;
    margin-left: 0;
    margin-top: 12px;
  }
}
</style>
