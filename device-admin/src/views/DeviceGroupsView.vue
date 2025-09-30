<template>
  <div class="device-groups-container">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-left">
        <h1>设备分组</h1>
        <p>工业设备分组管理与组织</p>
      </div>
      <div class="header-right">
        <button @click="showCreateGroupModal = true" class="create-btn">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 5v14M5 12h14" stroke="currentColor" stroke-width="2"/>
          </svg>
          新建分组
        </button>
        <button @click="refreshGroups" class="refresh-btn">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M23 4v6h-6M1 20v-6h6" stroke="currentColor" stroke-width="2"/>
            <path d="M20.49 9A9 9 0 0 0 5.64 5.64L1 10m22 4l-4.64 4.36A9 9 0 0 1 3.51 15" stroke="currentColor" stroke-width="2"/>
          </svg>
          刷新
        </button>
      </div>
    </div>

    <!-- 统计概览 -->
    <div class="stats-section">
      <div class="stats-cards">
        <div class="stats-card">
          <div class="stats-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" stroke="currentColor" stroke-width="2"/>
              <circle cx="9" cy="7" r="4" stroke="currentColor" stroke-width="2"/>
              <path d="M23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" stroke="currentColor" stroke-width="2"/>
            </svg>
          </div>
          <div class="stats-content">
            <div class="stats-value">{{ totalGroups }}</div>
            <div class="stats-label">总分组数</div>
          </div>
        </div>
        <div class="stats-card">
          <div class="stats-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect x="2" y="3" width="20" height="14" rx="2" stroke="currentColor" stroke-width="2"/>
              <path d="M8 21h8" stroke="currentColor" stroke-width="2"/>
            </svg>
          </div>
          <div class="stats-content">
            <div class="stats-value">{{ totalDevices }}</div>
            <div class="stats-label">总设备数</div>
          </div>
        </div>
        <div class="stats-card">
          <div class="stats-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M9 19c-5 0-9-4-9-9s4-9 9-9 9 4 9 9-4 9-9 9zM21 3l-3 3" stroke="currentColor" stroke-width="2"/>
            </svg>
          </div>
          <div class="stats-content">
            <div class="stats-value">{{ onlineDevices }}</div>
            <div class="stats-label">在线设备</div>
          </div>
        </div>
        <div class="stats-card">
          <div class="stats-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
            </svg>
          </div>
          <div class="stats-content">
            <div class="stats-value">{{ averageDevicesPerGroup }}</div>
            <div class="stats-label">平均设备/组</div>
          </div>
        </div>
      </div>
    </div>

    <!-- 分组列表 -->
    <div class="groups-section">
      <div class="section-header">
        <h2>设备分组列表</h2>
        <div class="view-controls">
          <div class="view-toggle">
            <button @click="viewMode = 'grid'" :class="{ active: viewMode === 'grid' }">网格视图</button>
            <button @click="viewMode = 'list'" :class="{ active: viewMode === 'list' }">列表视图</button>
          </div>
          <div class="search-box">
            <input 
              v-model="searchQuery" 
              type="text" 
              placeholder="搜索分组..." 
              class="search-input"
            />
          </div>
        </div>
      </div>

      <!-- 网格视图 -->
      <div v-if="viewMode === 'grid'" class="groups-grid">
        <div 
          v-for="group in filteredGroups" 
          :key="group.id" 
          class="group-card"
          @click="selectGroup(group)"
        >
          <div class="group-header">
            <div class="group-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" stroke="currentColor" stroke-width="2"/>
                <circle cx="9" cy="7" r="4" stroke="currentColor" stroke-width="2"/>
                <path d="M23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" stroke="currentColor" stroke-width="2"/>
              </svg>
            </div>
            <div class="group-actions">
              <button @click.stop="editGroup(group)" class="action-btn small">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" stroke="currentColor" stroke-width="2"/>
                  <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" stroke="currentColor" stroke-width="2"/>
                </svg>
              </button>
              <button @click.stop="deleteGroup(group)" class="action-btn small danger">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" stroke="currentColor" stroke-width="2"/>
                </svg>
              </button>
            </div>
          </div>
          
          <div class="group-content">
            <h3 class="group-name">{{ group.name }}</h3>
            <p class="group-description">{{ group.description }}</p>
            
            <div class="group-stats">
              <div class="stat-item">
                <span class="stat-label">设备数量</span>
                <span class="stat-value">{{ group.deviceCount }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">在线设备</span>
                <span class="stat-value online">{{ group.onlineCount }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">离线设备</span>
                <span class="stat-value offline">{{ group.offlineCount }}</span>
              </div>
            </div>
          </div>
          
          <div class="group-footer">
            <div class="group-tags">
              <span class="group-tag" :class="group.type">{{ getTypeText(group.type) }}</span>
              <span class="group-tag">{{ group.protocolCount }} 种协议</span>
            </div>
            <div class="group-updated">
              更新于 {{ formatDate(group.updatedAt) }}
            </div>
          </div>
        </div>
      </div>

      <!-- 列表视图 -->
      <div v-else class="groups-list">
        <div class="list-header">
          <div class="list-cell">分组名称</div>
          <div class="list-cell">描述</div>
          <div class="list-cell">设备数量</div>
          <div class="list-cell">在线设备</div>
          <div class="list-cell">类型</div>
          <div class="list-cell">更新时间</div>
          <div class="list-cell">操作</div>
        </div>
        <div 
          v-for="group in filteredGroups" 
          :key="group.id" 
          class="list-row"
          @click="selectGroup(group)"
        >
          <div class="list-cell">
            <div class="group-name-cell">
              <div class="group-icon-small">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" stroke="currentColor" stroke-width="2"/>
                  <circle cx="9" cy="7" r="4" stroke="currentColor" stroke-width="2"/>
                  <path d="M23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" stroke="currentColor" stroke-width="2"/>
                </svg>
              </div>
              {{ group.name }}
            </div>
          </div>
          <div class="list-cell">{{ group.description }}</div>
          <div class="list-cell">{{ group.deviceCount }}</div>
          <div class="list-cell">
            <span class="online-count">{{ group.onlineCount }}</span>
            <span class="offline-count">/ {{ group.offlineCount }}</span>
          </div>
          <div class="list-cell">
            <span class="group-tag" :class="group.type">{{ getTypeText(group.type) }}</span>
          </div>
          <div class="list-cell">{{ formatDate(group.updatedAt) }}</div>
          <div class="list-cell">
            <button @click.stop="editGroup(group)" class="action-btn small">编辑</button>
            <button @click.stop="deleteGroup(group)" class="action-btn small danger">删除</button>
          </div>
        </div>
      </div>
    </div>

    <!-- 创建/编辑分组模态框 -->
    <div v-if="showCreateGroupModal || showEditGroupModal" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>{{ showCreateGroupModal ? '新建分组' : '编辑分组' }}</h3>
          <button @click="closeModal" class="close-btn">×</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="saveGroup" class="group-form">
            <div class="form-group">
              <label for="groupName">分组名称 *</label>
              <input 
                id="groupName"
                v-model="groupForm.name" 
                type="text" 
                class="form-input"
                placeholder="请输入分组名称"
                required
              />
            </div>
            <div class="form-group">
              <label for="groupDescription">分组描述</label>
              <textarea 
                id="groupDescription"
                v-model="groupForm.description" 
                class="form-textarea"
                placeholder="请输入分组描述"
                rows="3"
              ></textarea>
            </div>
            <div class="form-group">
              <label for="groupType">分组类型</label>
              <select v-model="groupForm.type" class="form-select">
                <option value="production">生产设备</option>
                <option value="monitoring">监控设备</option>
                <option value="safety">安全设备</option>
                <option value="utility">公用设备</option>
                <option value="custom">自定义</option>
              </select>
            </div>
            <div class="form-group">
              <label for="groupColor">分组颜色</label>
              <div class="color-picker">
                <div 
                  v-for="color in colorOptions" 
                  :key="color"
                  class="color-option"
                  :class="{ active: groupForm.color === color }"
                  :style="{ backgroundColor: color }"
                  @click="groupForm.color = color"
                ></div>
              </div>
            </div>
          </form>
        </div>
        <div class="modal-footer">
          <button @click="closeModal" class="modal-btn cancel">取消</button>
          <button @click="saveGroup" class="modal-btn primary">保存</button>
        </div>
      </div>
    </div>

    <!-- 分组详情模态框 -->
    <div v-if="selectedGroup" class="modal-overlay" @click="closeGroupDetail">
      <div class="modal-content large" @click.stop>
        <div class="modal-header">
          <h3>{{ selectedGroup.name }} - 设备列表</h3>
          <button @click="closeGroupDetail" class="close-btn">×</button>
        </div>
        <div class="modal-body">
          <div class="group-detail-header">
            <div class="group-info">
              <p class="group-description">{{ selectedGroup.description }}</p>
              <div class="group-meta">
                <span class="group-type">{{ getTypeText(selectedGroup.type) }}</span>
                <span class="group-updated">更新于 {{ formatDate(selectedGroup.updatedAt) }}</span>
              </div>
            </div>
            <div class="group-actions">
              <button @click="addDevicesToGroup" class="action-btn">添加设备</button>
              <button @click="removeDevicesFromGroup" class="action-btn danger">移除设备</button>
            </div>
          </div>
          
          <div class="devices-table">
            <div class="table-header">
              <div class="table-cell">设备名称</div>
              <div class="table-cell">IP地址</div>
              <div class="table-cell">状态</div>
              <div class="table-cell">协议</div>
              <div class="table-cell">最后更新</div>
              <div class="table-cell">操作</div>
            </div>
            <div 
              v-for="device in selectedGroup.devices" 
              :key="device.id" 
              class="table-row"
            >
              <div class="table-cell">
                <div class="device-name">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <rect x="2" y="3" width="20" height="14" rx="2" stroke="currentColor" stroke-width="2"/>
                    <path d="M8 21h8" stroke="currentColor" stroke-width="2"/>
                  </svg>
                  {{ device.name }}
                </div>
              </div>
              <div class="table-cell">{{ device.ip }}</div>
              <div class="table-cell">
                <span class="status-indicator" :class="device.status">
                  <div class="status-dot" :class="device.status"></div>
                  {{ getStatusText(device.status) }}
                </span>
              </div>
              <div class="table-cell">{{ device.protocol }}</div>
              <div class="table-cell">{{ formatTime(device.lastUpdate) }}</div>
              <div class="table-cell">
                <button @click="removeDeviceFromGroup(device)" class="action-btn small danger">移除</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'

// 响应式数据
const viewMode = ref('grid')
const searchQuery = ref('')
const showCreateGroupModal = ref(false)
const showEditGroupModal = ref(false)
const selectedGroup = ref(null)
const editingGroup = ref(null)

// 分组表单
const groupForm = ref({
  name: '',
  description: '',
  type: 'production',
  color: '#00d4ff'
})

// 颜色选项
const colorOptions = ref([
  '#00d4ff', '#27ae60', '#f39c12', '#e74c3c', 
  '#9b59b6', '#3498db', '#1abc9c', '#34495e'
])

// 分组数据
const groups = ref([
  {
    id: 1,
    name: '生产设备组',
    description: '生产线主要设备分组',
    type: 'production',
    color: '#00d4ff',
    deviceCount: 15,
    onlineCount: 14,
    offlineCount: 1,
    protocolCount: 3,
    updatedAt: new Date(),
    devices: [
      { id: 1, name: 'PLC-001', ip: '192.168.1.100', status: 'online', protocol: 'Modbus TCP', lastUpdate: new Date() },
      { id: 2, name: 'PLC-002', ip: '192.168.1.101', status: 'online', protocol: 'Modbus TCP', lastUpdate: new Date() },
      { id: 3, name: 'HMI-001', ip: '192.168.1.102', status: 'offline', protocol: 'OPC UA', lastUpdate: new Date() }
    ]
  },
  {
    id: 2,
    name: '监控设备组',
    description: '系统监控和数据采集设备',
    type: 'monitoring',
    color: '#27ae60',
    deviceCount: 8,
    onlineCount: 8,
    offlineCount: 0,
    protocolCount: 2,
    updatedAt: new Date(Date.now() - 3600000),
    devices: [
      { id: 4, name: 'SCADA-001', ip: '192.168.1.103', status: 'online', protocol: 'OPC UA', lastUpdate: new Date() },
      { id: 5, name: 'SCADA-002', ip: '192.168.1.104', status: 'online', protocol: 'OPC UA', lastUpdate: new Date() }
    ]
  },
  {
    id: 3,
    name: '安全设备组',
    description: '安全监控和报警设备',
    type: 'safety',
    color: '#e74c3c',
    deviceCount: 5,
    onlineCount: 4,
    offlineCount: 1,
    protocolCount: 1,
    updatedAt: new Date(Date.now() - 7200000),
    devices: [
      { id: 6, name: 'Safety-001', ip: '192.168.1.105', status: 'online', protocol: 'Ethernet/IP', lastUpdate: new Date() },
      { id: 7, name: 'Safety-002', ip: '192.168.1.106', status: 'offline', protocol: 'Ethernet/IP', lastUpdate: new Date() }
    ]
  }
])

// 计算属性
const totalGroups = computed(() => groups.value.length)
const totalDevices = computed(() => groups.value.reduce((sum, group) => sum + group.deviceCount, 0))
const onlineDevices = computed(() => groups.value.reduce((sum, group) => sum + group.onlineCount, 0))
const averageDevicesPerGroup = computed(() => 
  totalGroups.value > 0 ? Math.round(totalDevices.value / totalGroups.value) : 0
)

const filteredGroups = computed(() => {
  if (!searchQuery.value) return groups.value
  return groups.value.filter(group => 
    group.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
    group.description.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
})

// 方法
function refreshGroups() {
  // 模拟刷新数据
  console.log('刷新分组数据')
}

function selectGroup(group: any) {
  selectedGroup.value = group
}

function closeGroupDetail() {
  selectedGroup.value = null
}

function editGroup(group: any) {
  editingGroup.value = group
  groupForm.value = {
    name: group.name,
    description: group.description,
    type: group.type,
    color: group.color
  }
  showEditGroupModal.value = true
}

function deleteGroup(group: any) {
  if (confirm(`确定要删除分组 "${group.name}" 吗？此操作将移除分组中的所有设备。`)) {
    const index = groups.value.findIndex(g => g.id === group.id)
    if (index > -1) {
      groups.value.splice(index, 1)
    }
  }
}

function closeModal() {
  showCreateGroupModal.value = false
  showEditGroupModal.value = false
  editingGroup.value = null
  groupForm.value = {
    name: '',
    description: '',
    type: 'production',
    color: '#00d4ff'
  }
}

function saveGroup() {
  if (showCreateGroupModal.value) {
    // 创建新分组
    const newGroup = {
      id: Date.now(),
      name: groupForm.value.name,
      description: groupForm.value.description,
      type: groupForm.value.type,
      color: groupForm.value.color,
      deviceCount: 0,
      onlineCount: 0,
      offlineCount: 0,
      protocolCount: 0,
      updatedAt: new Date(),
      devices: []
    }
    groups.value.push(newGroup)
  } else if (showEditGroupModal.value && editingGroup.value) {
    // 编辑现有分组
    const group = groups.value.find(g => g.id === editingGroup.value.id)
    if (group) {
      group.name = groupForm.value.name
      group.description = groupForm.value.description
      group.type = groupForm.value.type
      group.color = groupForm.value.color
      group.updatedAt = new Date()
    }
  }
  closeModal()
}

function addDevicesToGroup() {
  // 实现添加设备到分组的逻辑
  console.log('添加设备到分组')
}

function removeDevicesFromGroup() {
  // 实现从分组中移除设备的逻辑
  console.log('从分组中移除设备')
}

function removeDeviceFromGroup(device: any) {
  if (selectedGroup.value) {
    const group = groups.value.find(g => g.id === selectedGroup.value.id)
    if (group) {
      const deviceIndex = group.devices.findIndex(d => d.id === device.id)
      if (deviceIndex > -1) {
        group.devices.splice(deviceIndex, 1)
        group.deviceCount--
        if (device.status === 'online') {
          group.onlineCount--
        } else {
          group.offlineCount--
        }
      }
    }
  }
}

function getTypeText(type: string) {
  const typeMap: Record<string, string> = {
    production: '生产设备',
    monitoring: '监控设备',
    safety: '安全设备',
    utility: '公用设备',
    custom: '自定义'
  }
  return typeMap[type] || type
}

function getStatusText(status: string) {
  const statusMap: Record<string, string> = {
    online: '在线',
    offline: '离线',
    warning: '警告',
    error: '错误'
  }
  return statusMap[status] || status
}

function formatDate(date: Date) {
  return date.toLocaleDateString()
}

function formatTime(date: Date) {
  return date.toLocaleTimeString()
}

// 生命周期
onMounted(() => {
  // 初始化数据
})
</script>

<style scoped>
.device-groups-container {
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

.create-btn {
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

.create-btn:hover {
  background: #0099cc;
  transform: translateY(-1px);
}

.refresh-btn {
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

.refresh-btn:hover {
  background: #f8f9fa;
  border-color: #00d4ff;
  color: #00d4ff;
}

.stats-section {
  margin-bottom: 32px;
}

.stats-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
}

.stats-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
}

.stats-icon {
  width: 48px;
  height: 48px;
  background: linear-gradient(135deg, #00d4ff 0%, #0099cc 100%);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.stats-content {
  display: flex;
  flex-direction: column;
}

.stats-value {
  font-size: 24px;
  font-weight: 700;
  color: #2c3e50;
}

.stats-label {
  font-size: 14px;
  color: #6c757d;
}

.groups-section {
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

.view-controls {
  display: flex;
  align-items: center;
  gap: 16px;
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

.search-input {
  padding: 8px 12px;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  font-size: 14px;
  min-width: 200px;
}

.groups-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
  padding: 20px;
}

.group-card {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  padding: 20px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.group-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.group-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.group-icon {
  width: 40px;
  height: 40px;
  background: #f8f9fa;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #6c757d;
}

.group-actions {
  display: flex;
  gap: 8px;
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

.action-btn.danger {
  background: #e74c3c;
}

.action-btn.danger:hover {
  background: #c0392b;
}

.group-content {
  margin-bottom: 16px;
}

.group-name {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 8px;
}

.group-description {
  font-size: 14px;
  color: #6c757d;
  margin: 0 0 16px;
  line-height: 1.4;
}

.group-stats {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 4px 0;
}

.stat-label {
  font-size: 12px;
  color: #6c757d;
}

.stat-value {
  font-size: 14px;
  font-weight: 600;
  color: #2c3e50;
}

.stat-value.online {
  color: #27ae60;
}

.stat-value.offline {
  color: #e74c3c;
}

.group-footer {
  border-top: 1px solid #f1f3f4;
  padding-top: 12px;
}

.group-tags {
  display: flex;
  gap: 8px;
  margin-bottom: 8px;
}

.group-tag {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.group-tag.production {
  background: rgba(0, 212, 255, 0.1);
  color: #00d4ff;
}

.group-tag.monitoring {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.group-tag.safety {
  background: rgba(231, 76, 60, 0.1);
  color: #e74c3c;
}

.group-tag.utility {
  background: rgba(52, 152, 219, 0.1);
  color: #3498db;
}

.group-tag.custom {
  background: rgba(155, 89, 182, 0.1);
  color: #9b59b6;
}

.group-updated {
  font-size: 12px;
  color: #999;
}

.groups-list {
  background: white;
}

.list-header {
  display: grid;
  grid-template-columns: 200px 1fr 100px 120px 100px 120px 120px;
  gap: 16px;
  padding: 16px 20px;
  background: #f8f9fa;
  font-weight: 600;
  color: #2c3e50;
  font-size: 14px;
}

.list-row {
  display: grid;
  grid-template-columns: 200px 1fr 100px 120px 100px 120px 120px;
  gap: 16px;
  padding: 16px 20px;
  border-bottom: 1px solid #f1f3f4;
  cursor: pointer;
  transition: all 0.3s ease;
}

.list-row:hover {
  background: #f8f9fa;
}

.list-cell {
  display: flex;
  align-items: center;
  font-size: 14px;
  color: #2c3e50;
}

.group-name-cell {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 500;
}

.group-icon-small {
  width: 16px;
  height: 16px;
  color: #6c757d;
}

.online-count {
  color: #27ae60;
  font-weight: 600;
}

.offline-count {
  color: #6c757d;
}

.status-indicator {
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

.group-form {
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

.form-input,
.form-textarea,
.form-select {
  padding: 10px 12px;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  font-size: 14px;
  transition: all 0.3s ease;
}

.form-input:focus,
.form-textarea:focus,
.form-select:focus {
  outline: none;
  border-color: #00d4ff;
  box-shadow: 0 0 0 2px rgba(0, 212, 255, 0.1);
}

.color-picker {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.color-option {
  width: 32px;
  height: 32px;
  border-radius: 6px;
  cursor: pointer;
  border: 2px solid transparent;
  transition: all 0.3s ease;
}

.color-option:hover {
  transform: scale(1.1);
}

.color-option.active {
  border-color: #2c3e50;
  transform: scale(1.1);
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

.group-detail-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid #e9ecef;
}

.group-info p {
  margin: 0 0 8px;
  color: #6c757d;
}

.group-meta {
  display: flex;
  gap: 16px;
  font-size: 12px;
  color: #999;
}

.devices-table {
  border: 1px solid #e9ecef;
  border-radius: 6px;
  overflow: hidden;
}

.table-header {
  display: grid;
  grid-template-columns: 200px 120px 100px 120px 120px 100px;
  gap: 16px;
  padding: 12px 16px;
  background: #f8f9fa;
  font-weight: 600;
  color: #2c3e50;
  font-size: 14px;
}

.table-row {
  display: grid;
  grid-template-columns: 200px 120px 100px 120px 120px 100px;
  gap: 16px;
  padding: 12px 16px;
  border-bottom: 1px solid #f1f3f4;
  transition: all 0.3s ease;
}

.table-row:hover {
  background: #f8f9fa;
}

.table-cell {
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
  
  .stats-cards {
    grid-template-columns: 1fr;
  }
  
  .groups-grid {
    grid-template-columns: 1fr;
  }
  
  .list-header,
  .list-row {
    grid-template-columns: 1fr;
    gap: 8px;
  }
  
  .table-header,
  .table-row {
    grid-template-columns: 1fr;
    gap: 8px;
  }
}
</style>
