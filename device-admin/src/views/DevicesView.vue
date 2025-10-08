<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { listDevices, createDevice, updateDevice, deleteDevice, searchDevices, getDevicePoints, importDevicePointsFromTemplate, createDevicePoint, updateDevicePoint, deleteDevicePoint, testDeviceConnection, type DeviceDto, type CreateDeviceRequest, type UpdateDeviceRequest, type DevicePointDto, type TestConnectionRequest, type TestConnectionResponse } from '@/api/devices'
import { reloadDevice, getGatewayConfig, getGatewayLatestSnapshot } from '@/api/gateways'
import { listProtocolTemplates, type ProtocolTemplateDto } from '@/api/devices'

const devices = ref<DeviceDto[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const searchQuery = ref('')
// 设备详情-点位
const showPointsModal = ref(false)
const selectedDeviceId = ref<number | null>(null)
const selectedDeviceName = ref('')
const pointsLoading = ref(false)
const devicePoints = ref<DevicePointDto[]>([])
const latestSnapshot = ref<Record<string, any>>({})
let latestTimer: number | null = null
const importTemplateId = ref<number | null>(null)
const templateOptions = ref<ProtocolTemplateDto[]>([])
// 新建设备表单与弹窗
const showCreateModal = ref(false)
const isEditing = ref(false)
const editDeviceId = ref<number | null>(null)
const createForm = ref<CreateDeviceRequest>({
  name: '',
  type: '',
  ipAddress: '',
  port: 502,
  protocol: 'ModbusTCP',
  description: '',
  gatewayId: undefined,
  parameters: {}
})

function openCreateModal() {
  createForm.value = {
    name: '',
    type: '',
    ipAddress: '',
    port: 502,
    protocol: 'ModbusTCP',
    description: '',
    gatewayId: undefined,
    parameters: {}
  }
  showCreateModal.value = true
  isEditing.value = false
  editDeviceId.value = null
}

function closeCreateModal() {
  showCreateModal.value = false
}

async function submitCreateDevice() {
  if (loading.value) return
  loading.value = true
  error.value = null
  try {
    if (isEditing.value && editDeviceId.value != null) {
      const updated = await updateDevice(editDeviceId.value, createForm.value)
      const idx = devices.value.findIndex(d => d.id === editDeviceId.value)
      if (idx !== -1) devices.value[idx] = updated
    } else {
      const created = await createDevice(createForm.value)
      devices.value.unshift(created)
    }
    showCreateModal.value = false
  } catch (e: any) {
    error.value = e.message || String(e)
  } finally {
    loading.value = false
  }
}

async function testConnection() {
  if (!createForm.value.ipAddress || !createForm.value.port) {
    alert('请填写 IP 和端口')
    return
  }
  try {
    const req: TestConnectionRequest = {
      ipAddress: createForm.value.ipAddress,
      port: createForm.value.port,
      protocol: createForm.value.protocol,
      timeoutMs: 3000
    }
    const resp: TestConnectionResponse = await testDeviceConnection(req)
    if (resp.success) {
      alert(`连接成功，延迟 ${resp.latencyMs ?? 0} ms`)
    } else {
      alert(`连接失败：${resp.message}`)
    }
  } catch (e: any) {
    alert('连接测试异常：' + (e.message || String(e)))
  }
}

function openEditModal(device: DeviceDto) {
  createForm.value = {
    name: device.name,
    type: device.type,
    ipAddress: device.ipAddress,
    port: device.port,
    protocol: device.protocol,
    description: device.description || '',
    gatewayId: device.gatewayId,
    parameters: device.parameters || {}
  }
  isEditing.value = true
  editDeviceId.value = device.id
  showCreateModal.value = true
}
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
      device.ipAddress.includes(searchQuery.value)
    )
  }

  // 状态过滤
  if (statusFilter.value !== 'all') {
    const target = statusFilter.value === 'online' ? 1 : 0
    filtered = filtered.filter(device => device.status === target)
  }

  // 排序
  filtered.sort((a, b) => {
    const aRaw = a[sortBy.value as keyof DeviceDto]
    const bRaw = b[sortBy.value as keyof DeviceDto]
    const aValue = typeof aRaw === 'string' ? aRaw.toLowerCase() : aRaw
    const bValue = typeof bRaw === 'string' ? bRaw.toLowerCase() : bRaw
    if (aValue === undefined || bValue === undefined) return 0
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
  const online = devices.value.filter(d => d.status === 1).length
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
    console.error('获取设备列表失败:', e)
  } finally {
    loading.value = false
  }
}

async function openDeviceDetails(device: DeviceDto) {
  selectedDeviceId.value = device.id
  selectedDeviceName.value = device.name
  showPointsModal.value = true
  pointsLoading.value = true
  try {
    devicePoints.value = await getDevicePoints(device.id)
    // 获取最新快照
    latestSnapshot.value = await getGatewayLatestSnapshot()
    // 模板列表
    templateOptions.value = await listProtocolTemplates()
    // 开启定时刷新
    if (latestTimer) { clearInterval(latestTimer) }
    latestTimer = window.setInterval(async () => {
      latestSnapshot.value = await getGatewayLatestSnapshot()
    }, 5000)
  } catch (e: any) {
    alert('加载点位失败：' + (e.message || String(e)))
  } finally {
    pointsLoading.value = false
  }
}

function closePointsModal() {
  showPointsModal.value = false
  selectedDeviceId.value = null
  selectedDeviceName.value = ''
  devicePoints.value = []
  latestSnapshot.value = {}
  if (latestTimer) { clearInterval(latestTimer); latestTimer = null }
}

async function handleImportTemplate() {
  if (!selectedDeviceId.value || !importTemplateId.value) return
  try {
    await importDevicePointsFromTemplate(selectedDeviceId.value, importTemplateId.value)
    // 重新加载点位
    devicePoints.value = await getDevicePoints(selectedDeviceId.value)
    alert('导入成功')
  } catch (e: any) {
    alert('导入失败：' + (e.message || String(e)))
  }
}

function getPointLatestValue(p: DevicePointDto) {
  // 快照key示例：ProtocolName.PointName 或 自定义映射；此处先按地址或名称两种尝试
  if (!latestSnapshot.value) return '-'
  const byAddress = latestSnapshot.value[p.address] || latestSnapshot.value[String(p.address)]
  if (byAddress !== undefined) return byAddress
  const byName = latestSnapshot.value[p.name]
  if (byName !== undefined) return byName
  return '-'
}

async function savePoint(p: DevicePointDto) {
  if (!selectedDeviceId.value) return
  try {
    const updated = await updateDevicePoint(selectedDeviceId.value, p.id, {
      name: p.name,
      address: p.address,
      dataType: p.dataType,
      unit: p.unit,
      access: p.access,
      intervalMs: p.intervalMs,
      enabled: p.enabled
    })
    Object.assign(p, updated)
    alert('保存成功')
  } catch (e: any) {
    alert('保存失败：' + (e.message || String(e)))
  }
}

async function removePoint(p: DevicePointDto) {
  if (!selectedDeviceId.value) return
  if (!confirm(`确认删除点位 ${p.name}?`)) return
  try {
    await deleteDevicePoint(selectedDeviceId.value, p.id)
    devicePoints.value = devicePoints.value.filter(x => x.id !== p.id)
    alert('删除成功')
  } catch (e: any) {
    alert('删除失败：' + (e.message || String(e)))
  }
}

async function addPoint() {
  if (!selectedDeviceId.value) return
  try {
    const created = await createDevicePoint(selectedDeviceId.value, {
      name: 'NewPoint',
      address: '40001',
      dataType: 'INT16',
      unit: '',
      access: 'R',
      intervalMs: 1000,
      enabled: true
    })
    devicePoints.value.push(created)
  } catch (e: any) {
    alert('新增失败：' + (e.message || String(e)))
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

function getStatusColor(status: number) {
  switch (status) {
    case 1: return '#28a745' // 在线
    case 0: return '#dc3545' // 离线
    case 2: return '#ffc107' // 连接中
    case 3: return '#dc3545' // 错误
    case 4: return '#6c757d' // 维护中
    default: return '#6c757d'
  }
}

function getStatusIcon(status: number) {
  switch (status) {
    case 1: return 'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z' // 在线
    case 0: return 'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z' // 离线
    case 2: return 'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z' // 连接中
    case 3: return 'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z' // 错误
    case 4: return 'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z' // 维护中
    default: return 'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z'
  }
}

// 搜索功能
async function handleSearch() {
  if (!searchQuery.value.trim()) {
    await fetchDevices()
    return
  }
  
  loading.value = true
  error.value = null
  
  try {
    devices.value = await searchDevices(searchQuery.value)
  } catch (e: any) {
    error.value = e.message || String(e)
    console.error('搜索设备失败:', e)
  } finally {
    loading.value = false
  }
}

// 创建设备
async function handleCreateDevice(deviceData: CreateDeviceRequest) {
  loading.value = true
  error.value = null
  
  try {
    const newDevice = await createDevice(deviceData)
    devices.value.push(newDevice)
    return newDevice
  } catch (e: any) {
    error.value = e.message || String(e)
    console.error('创建设备失败:', e)
    throw e
  } finally {
    loading.value = false
  }
}

// 更新设备
async function handleUpdateDevice(id: number, deviceData: UpdateDeviceRequest) {
  loading.value = true
  error.value = null
  
  try {
    const updatedDevice = await updateDevice(id, deviceData)
    const index = devices.value.findIndex(d => d.id === id)
    if (index !== -1) {
      devices.value[index] = updatedDevice
    }
    return updatedDevice
  } catch (e: any) {
    error.value = e.message || String(e)
    console.error('更新设备失败:', e)
    throw e
  } finally {
    loading.value = false
  }
}

// 删除设备
async function handleDeleteDevice(id: number) {
  loading.value = true
  error.value = null
  
  try {
    await deleteDevice(id)
    devices.value = devices.value.filter(d => d.id !== id)
    return true
  } catch (e: any) {
    error.value = e.message || String(e)
    console.error('删除设备失败:', e)
    throw e
  } finally {
    loading.value = false
  }
}

// 获取状态文本
function getStatusText(status: number): string {
  switch (status) {
    case 0: return '离线'
    case 1: return '在线'
    case 2: return '连接中'
    case 3: return '错误'
    case 4: return '维护中'
    default: return '未知'
  }
}

onMounted(fetchDevices)

async function handleReload(deviceId: number) {
  try {
    await reloadDevice(deviceId)
    alert('已触发网关重载')
  } catch (e: any) {
    alert('重载失败：' + (e.message || String(e)))
  }
}
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
            <i class="fas fa-rotate-right"></i>
          </button>
          <button type="button" class="btn-add" @click="openCreateModal">
            <i class="fas fa-plus" style="margin-right:8px"></i>
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
              <td class="device-ip">{{ device.ipAddress }}</td>
              <td class="device-status">
                <div class="status-indicator">
                  <div 
                    class="status-dot" 
                    :style="{ backgroundColor: getStatusColor(device.status) }"
                  ></div>
                  <span class="status-text">{{ getStatusText(device.status) }}</span>
                </div>
            </td>
              <td class="device-time">2分钟前</td>
              <td class="device-actions">
                <button class="btn-action" title="查看详情" @click="openDeviceDetails(device)">
                  <i class="fas fa-eye"></i>
                </button>
                <button class="btn-action" title="编辑设备" @click="openEditModal(device)">
                  <i class="fas fa-edit"></i>
                </button>
                <button class="btn-action danger" title="删除设备" @click="handleDeleteDevice(device.id)">
                  <i class="fas fa-trash"></i>
                </button>
                <button class="btn-action" title="重载采集" @click="handleReload(device.id)">
                  <i class="fas fa-sync-alt"></i>
                </button>
            </td>
          </tr>
        </tbody>
      </table>
      </div>
    </div>
  </div>

  <!-- 新建设备弹窗 -->
  <div v-if="showCreateModal" class="modal-mask" @click.self="closeCreateModal">
    <div class="modal-container">
      <div class="modal-header">
        <h3>新增设备</h3>
        <button type="button" class="modal-close" @click="closeCreateModal">
          <i class="fas fa-times"></i>
        </button>
      </div>
      <div class="modal-body">
        <div class="form-row">
          <label>设备名称</label>
          <input v-model="createForm.name" placeholder="例如：PLC-001" />
        </div>
        <div class="form-row">
          <label>设备类型</label>
          <input v-model="createForm.type" placeholder="例如：PLC/Sensor" />
        </div>
        <div class="form-row two-cols">
          <div>
            <label>IP地址</label>
            <input v-model="createForm.ipAddress" placeholder="例如：192.168.1.10" />
          </div>
          <div>
            <label>端口</label>
            <input v-model.number="createForm.port" type="number" min="1" />
          </div>
        </div>
        <div class="form-row">
          <label>协议</label>
          <select v-model="createForm.protocol">
            <option value="ModbusTCP">ModbusTCP</option>
            <option value="OPCUA">OPCUA</option>
            <option value="MQTT">MQTT</option>
            <option value="S7">S7</option>
          </select>
        </div>
        <div class="form-row">
          <label>描述</label>
          <textarea v-model="createForm.description" rows="3" placeholder="可选"></textarea>
        </div>
        <div class="form-row">
          <label>网关ID（可选）</label>
          <input v-model.number="createForm.gatewayId" type="number" min="1" placeholder="绑定到网关的ID" />
        </div>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn" @click="testConnection" :disabled="loading"><i class="fas fa-plug" style="margin-right:6px"></i>测试连接</button>
        <button type="button" class="btn" @click="closeCreateModal" :disabled="loading">取消</button>
        <button type="button" class="btn-primary" @click="submitCreateDevice" :disabled="loading || !createForm.name || !createForm.type || !createForm.ipAddress || !createForm.protocol">保存</button>
      </div>
    </div>
  </div>

  <!-- 设备点位弹窗 -->
  <div v-if="showPointsModal" class="modal-mask" @click.self="closePointsModal">
    <div class="modal-container" style="width: 880px">
      <div class="modal-header">
        <h3>设备点位 - {{ selectedDeviceName }}</h3>
        <button type="button" class="modal-close" @click="closePointsModal">
          <i class="fas fa-times"></i>
        </button>
      </div>
      <div class="modal-body">
        <div v-if="pointsLoading" class="loading-state">
          <div class="loading-spinner"></div>
          <span>加载点位中...</span>
        </div>
        <div v-else>
          <div class="filters" style="margin-bottom:12px">
            <select v-model.number="importTemplateId" class="filter-select" style="max-width:260px">
              <option :value="null">选择模板</option>
              <option v-for="t in templateOptions" :key="t.id" :value="t.id">{{ t.name }} ({{ t.protocol }})</option>
            </select>
            <button class="btn" @click="handleImportTemplate" :disabled="!selectedDeviceId || !importTemplateId">从模板导入</button>
          </div>
          <table class="devices-table">
            <thead>
              <tr>
                <th>名称</th>
                <th>地址/节点</th>
                <th>类型</th>
                <th>单位</th>
                <th>读写</th>
                <th>周期(ms)</th>
                <th>最近值</th>
                <th>启用</th>
                <th>操作</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in devicePoints" :key="p.id">
                <td><input v-model="p.name" class="search-input" style="max-width:160px" /></td>
                <td><input v-model="p.address" class="search-input" style="max-width:140px" /></td>
                <td><input v-model="p.dataType" class="search-input" style="max-width:100px" /></td>
                <td><input v-model="p.unit" class="search-input" style="max-width:80px" /></td>
                <td>
                  <select v-model="p.access" class="filter-select" style="max-width:80px">
                    <option value="R">R</option>
                    <option value="RW">RW</option>
                  </select>
                </td>
                <td><input v-model.number="p.intervalMs" type="number" class="search-input" style="max-width:100px" /></td>
                <td>{{ getPointLatestValue(p) }}</td>
                <td>
                  <select v-model="p.enabled" class="filter-select" style="max-width:80px">
                    <option :value="true">是</option>
                    <option :value="false">否</option>
                  </select>
                </td>
                <td>
                  <button class="btn" @click="savePoint(p)">保存</button>
                  <button class="btn danger" @click="removePoint(p)">删除</button>
                </td>
              </tr>
              <tr v-if="devicePoints.length === 0">
                <td colspan="7" style="text-align:center;color:#6c757d;">暂无点位</td>
              </tr>
            </tbody>
          </table>
          <div style="margin-top:12px">
            <button class="btn" @click="addPoint">新增点位</button>
          </div>
        </div>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn" @click="closePointsModal">关闭</button>
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
/* 新建设备弹窗样式 */
.modal-mask {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 3000;
}

.modal-container {
  width: 640px;
  max-width: calc(100vw - 32px);
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 16px 40px rgba(0, 0, 0, 0.2);
  border: 1px solid rgba(0, 0, 0, 0.06);
  overflow: hidden;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 16px;
  border-bottom: 1px solid #eef1f4;
}

.modal-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
}

.modal-close {
  background: transparent;
  border: none;
  cursor: pointer;
  color: #6c757d;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 30px;
  height: 30px;
}

.modal-close i {
  font-size: 16px;
}

.modal-close:hover {
  color: #2c3e50;
}

.modal-body {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.form-row {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-row.two-cols {
  flex-direction: row;
  gap: 12px;
}

.form-row.two-cols > div {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.form-row label {
  font-size: 12px;
  color: #6c757d;
}

.form-row input,
.form-row select,
.form-row textarea {
  padding: 10px 12px;
  border: 1px solid #e6e9ee;
  border-radius: 8px;
  font-size: 14px;
  outline: none;
}

.form-row input:focus,
.form-row select:focus,
.form-row textarea:focus {
  border-color: #4a90e2;
  box-shadow: 0 0 0 3px rgba(74, 144, 226, 0.15);
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 12px 16px 16px;
  border-top: 1px solid #eef1f4;
}

.btn-primary {
  padding: 8px 14px;
  border: none;
  background: linear-gradient(90deg, #4a90e2, #357abd);
  color: #fff;
  border-radius: 8px;
  cursor: pointer;
}

.btn {
  padding: 8px 14px;
  border: 1px solid #e6e9ee;
  background: #fff;
  color: #2c3e50;
  border-radius: 8px;
  cursor: pointer;
}

.btn[disabled],
.btn-primary[disabled] {
  opacity: 0.6;
  cursor: not-allowed;
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


