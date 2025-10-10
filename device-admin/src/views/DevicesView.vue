<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { listDevices, createDevice, updateDevice, deleteDevice, getDevicePoints, importDevicePointsFromTemplate, createDevicePoint, updateDevicePoint, deleteDevicePoint, batchDeleteDevicePoints, batchUpdateDevicePointsStatus, testDeviceConnection, type DeviceDto, type CreateDeviceRequest, type UpdateDeviceRequest, type DevicePointDto, type TestConnectionRequest, type TestConnectionResponse, type WriteCommandRequest } from '@/api/devices'
import { reloadDevice, getGatewayLatestSnapshot } from '@/api/gateways'
import { listProtocolTemplates, type ProtocolTemplateDto } from '@/api/devices'
import { useWebSocket, type RealtimeDataUpdate, type PointValueUpdate } from '@/api/websocket'
import { writeDeviceCommand } from '@/api/devices'

const devices = ref<DeviceDto[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const searchQuery = ref('')
// 设备详情-点位
const showPointsModal = ref(false)
const selectedDeviceId = ref<number | null>(null)
const selectedDeviceName = ref('')
const selectedDeviceProtocol = ref('')
const pointsLoading = ref(false)
const devicePoints = ref<DevicePointDto[]>([])
const latestSnapshot = ref<Record<string, any>>({})
let latestTimer: number | null = null
const importTemplateId = ref<number | null>(null)
const templateOptions = ref<ProtocolTemplateDto[]>([])

// 批量操作
const selectedPoints = ref<number[]>([])
const showBatchActions = ref(false)

// WebSocket连接
const { isConnected, websocketService } = useWebSocket()

// 设备点位缓存和展开状态
const devicePointsCache = ref<Record<number, DevicePointDto[]>>({})
const expandedDevices = ref<Set<number>>(new Set())
const deviceSnapshots = ref<Record<number, Record<string, any>>>({})

// Modbus 功能码配置
const modbusFunctionCodes = [
  { value: 1, label: '01 - 读线圈状态', description: '读取单个或多个线圈状态' },
  { value: 2, label: '02 - 读离散输入', description: '读取单个或多个离散输入状态' },
  { value: 3, label: '03 - 读保持寄存器', description: '读取单个或多个保持寄存器' },
  { value: 4, label: '04 - 读输入寄存器', description: '读取单个或多个输入寄存器' },
  { value: 5, label: '05 - 写单个线圈', description: '写入单个线圈状态' },
  { value: 6, label: '06 - 写单个寄存器', description: '写入单个保持寄存器' },
  { value: 15, label: '15 - 写多个线圈', description: '写入多个线圈状态' },
  { value: 16, label: '16 - 写多个寄存器', description: '写入多个保持寄存器' }
]

// 地址类型配置
const addressTypes = [
  { value: 'dec', label: '十进制', description: '地址以十进制表示' },
  { value: 'hex', label: '十六进制', description: '地址以十六进制表示' }
]
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
      // 转换为UpdateDeviceRequest格式
      const updateRequest: UpdateDeviceRequest = {
        name: createForm.value.name,
        type: createForm.value.type,
        ipAddress: createForm.value.ipAddress,
        port: createForm.value.port,
        protocol: createForm.value.protocol,
        description: createForm.value.description,
        gatewayId: createForm.value.gatewayId,
        parameters: createForm.value.parameters
      }
      const updated = await updateDevice(editDeviceId.value, updateRequest)
      const idx = devices.value.findIndex(d => d.id === editDeviceId.value)
      if (idx !== -1) devices.value[idx] = updated
    } else {
      const created = await createDevice(createForm.value)
      devices.value.unshift(created)
      // 新增设备后：清理该设备的快照缓存并触发一次全局快照刷新
      delete deviceSnapshots.value[created.id]
      if (!refreshInProgress) refreshSnapshotDataAsync()
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
    filtered = filtered.filter(device => {
      if (statusFilter.value === 'online') {
        // 在线：数据库状态为1 或者 有有效的实时数据
        const snapshot = deviceSnapshots.value[device.id]
        if (device.status === 1) return true
        if (snapshot && Object.keys(snapshot).length > 0) {
          const hasValidData = Object.values(snapshot).some(value => 
            value !== null && value !== undefined && value !== '' && value !== '无数据'
          )
          return hasValidData
        }
        return false
      } else {
        // 离线：数据库状态为0 且 没有有效的实时数据
        const snapshot = deviceSnapshots.value[device.id]
        if (device.status === 0) {
          if (!snapshot || Object.keys(snapshot).length === 0) return true
          const hasValidData = Object.values(snapshot).some(value => 
            value !== null && value !== undefined && value !== '' && value !== '无数据'
          )
          return !hasValidData
        }
        return false
      }
    })
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
  let online = 0
  let offline = 0
  
  devices.value.forEach(device => {
    // 检查数据库状态
    if (device.status === 1) {
      online++
      return
    }
    
    // 检查是否有有效的实时数据
    const snapshot = deviceSnapshots.value[device.id]
    if (snapshot && Object.keys(snapshot).length > 0) {
      const hasValidData = Object.values(snapshot).some(value => 
        value !== null && value !== undefined && value !== '' && value !== '无数据'
      )
      if (hasValidData) {
        online++
        return
      }
    }
    
    // 默认离线
    offline++
  })
  
  console.log(`设备统计: 总数=${total}, 在线=${online}, 离线=${offline}`)
  return { total, online, offline }
})

async function fetchDevices() {
  loading.value = true
  error.value = null
  try {
    devices.value = await listDevices()
    
    // 为所有设备预加载快照数据，以便正确显示状态
    await preloadDeviceSnapshots()
  } catch (e: any) {
    error.value = e.message || String(e)
    console.error('获取设备列表失败:', e)
  } finally {
    loading.value = false
  }
}

// 预加载所有设备的快照数据
const preloadDeviceSnapshots = async () => {
  console.log('开始预加载设备快照数据...')
  const promises = devices.value.map(async (device) => {
    try {
      const snapshot = await getGatewayLatestSnapshot()
      
      // 获取设备信息，用于过滤数据
      const protocolKey = `${device.protocol}_${device.ipAddress}_${device.port}`
      
      // 过滤出该设备相关的数据
      const deviceSpecificData: Record<string, any> = {}
      for (const [key, value] of Object.entries(snapshot)) {
        if (key.startsWith(protocolKey + '.')) {
          deviceSpecificData[key] = value
        }
      }
      
      // 检查过滤后的数据是否有效
      if (Object.keys(deviceSpecificData).length > 0) {
        const hasValidData = Object.values(deviceSpecificData).some(value => 
          value !== null && value !== undefined && value !== '' && value !== '无数据'
        )
        
        if (hasValidData) {
          deviceSnapshots.value[device.id] = deviceSpecificData
          console.log(`设备 ${device.id} (${device.name}) 在线，协议键: ${protocolKey}`)
        } else {
          deviceSnapshots.value[device.id] = {}
          console.log(`设备 ${device.id} (${device.name}) 离线 - 无有效数据`)
        }
      } else {
        deviceSnapshots.value[device.id] = {}
        console.log(`设备 ${device.id} (${device.name}) 离线 - 无匹配数据，协议键: ${protocolKey}`)
      }
    } catch (error) {
      deviceSnapshots.value[device.id] = {}
      console.log(`设备 ${device.id} (${device.name}) 离线 - 获取数据失败`)
    }
  })
  
  await Promise.allSettled(promises)
  console.log('设备快照数据预加载完成')
}

// 切换设备展开状态
const toggleDeviceExpansion = async (deviceId: number) => {
  if (expandedDevices.value.has(deviceId)) {
    expandedDevices.value.delete(deviceId)
    console.log(`设备 ${deviceId} 已收起`)
  } else {
    expandedDevices.value.add(deviceId)
    console.log(`设备 ${deviceId} 已展开`)
    // 如果缓存中没有该设备的点位数据，则加载
    if (!devicePointsCache.value[deviceId]) {
      await loadDevicePoints(deviceId)
    }
  }
  
  // 始终启动定时刷新，以保持状态显示准确
  startDeviceDataRefresh()
}

// 加载设备点位数据
const loadDevicePoints = async (deviceId: number) => {
  try {
    const points = await getDevicePoints(deviceId)
    devicePointsCache.value[deviceId] = points
    
    // 同时加载该设备的快照数据
    try {
      const snapshot = await getGatewayLatestSnapshot()
      
      // 获取设备信息，用于过滤数据
      const device = devices.value.find(d => d.id === deviceId)
      if (device) {
        const protocolKey = `${device.protocol}_${device.ipAddress}_${device.port}`
        
        // 过滤出该设备相关的数据
        const deviceSpecificData: Record<string, any> = {}
        for (const [key, value] of Object.entries(snapshot)) {
          if (key.startsWith(protocolKey + '.')) {
            deviceSpecificData[key] = value
          }
        }
        
        deviceSnapshots.value[deviceId] = deviceSpecificData
        console.log(`设备 ${deviceId} (${device.name}) 快照数据加载成功:`, deviceSpecificData)
        console.log(`设备 ${deviceId} 协议键: ${protocolKey}`)
      } else {
        // 如果找不到设备信息，使用全局快照
        deviceSnapshots.value[deviceId] = snapshot
        console.log(`设备 ${deviceId} 快照数据加载成功（使用全局快照）:`, snapshot)
      }
    } catch (snapshotError) {
      console.warn(`设备 ${deviceId} 快照数据加载失败:`, snapshotError)
      deviceSnapshots.value[deviceId] = {}
    }
  } catch (error) {
    console.error(`加载设备 ${deviceId} 的点位失败:`, error)
    devicePointsCache.value[deviceId] = []
    deviceSnapshots.value[deviceId] = {}
  }
}

// 刷新设备点位数据
const refreshDevicePoints = async (deviceId: number) => {
  await loadDevicePoints(deviceId)
  console.log(`设备 ${deviceId} 点位和快照数据已刷新`)
}

// 强制刷新所有设备状态
const forceRefreshStatus = async () => {
  console.log('强制刷新所有设备状态...')
  await preloadDeviceSnapshots()
  console.log('设备状态刷新完成')
}


// 定时刷新所有展开设备的快照数据
let deviceRefreshTimer: number | null = null

const startDeviceDataRefresh = () => {
  if (deviceRefreshTimer) {
    clearInterval(deviceRefreshTimer)
  }
  
  deviceRefreshTimer = setInterval(async () => {
    // 只刷新全局快照数据，不覆盖设备特定的快照数据
    try {
      // 只有在没有其他刷新正在进行时才执行定时刷新
      if (!refreshInProgress) {
        const snapshot = await getGatewayLatestSnapshot()
        latestSnapshot.value = snapshot
        console.log('定时刷新全局快照数据，数据项数量:', Object.keys(snapshot || {}).length)
      } else {
        console.log('跳过定时刷新，其他刷新正在进行中')
      }
    } catch (error) {
      console.warn('定时刷新全局快照数据失败:', error)
    }
  }, 10000) // 改为每10秒刷新一次，减少请求频率
}

const stopDeviceDataRefresh = () => {
  if (deviceRefreshTimer) {
    clearInterval(deviceRefreshTimer)
    deviceRefreshTimer = null
  }
}

async function openDeviceDetails(device: DeviceDto) {
  selectedDeviceId.value = device.id
  selectedDeviceName.value = device.name
  selectedDeviceProtocol.value = device.protocol
  showPointsModal.value = true
  pointsLoading.value = true
  try {
    // 并行加载点位和模板列表
    const [points, templates] = await Promise.all([
      getDevicePoints(device.id),
      listProtocolTemplates()
    ])
    devicePoints.value = points
    templateOptions.value = templates
    
    // 预填充该设备的快照：从全局快照中过滤协议键
    try {
      const global = latestSnapshot.value
      if (global && Object.keys(global).length > 0) {
        const protocolKey = `${device.protocol}_${device.ipAddress}_${device.port}`
        const filtered: Record<string, any> = {}
        for (const [k, v] of Object.entries(global)) {
          if (k.startsWith(protocolKey + '.')) filtered[k] = v
        }
        deviceSnapshots.value[device.id] = filtered
        console.log(`预填充设备 ${device.id} 的快照，条目:`, Object.keys(filtered).length)
      }
    } catch {}
    
    // 使用WebSocket获取实时数据
    if (isConnected.value) {
      console.log('WebSocket已连接，加入设备监控组')
      await websocketService.joinDeviceMonitoring(device.id)
      
      // 设置WebSocket事件监听
      websocketService.on('realtimeDataUpdate', handleRealtimeDataUpdate)
      websocketService.on('deviceDataUpdate', handleDeviceDataUpdate)
      websocketService.on('pointValueUpdate', handlePointValueUpdate)
    } else {
      console.warn('WebSocket未连接，尝试HTTP方式获取数据')
      // 降级到HTTP方式
      try {
        latestSnapshot.value = await getGatewayLatestSnapshot()
        console.log('HTTP方式获取到的数据:', latestSnapshot.value)
        console.log('数据键列表:', Object.keys(latestSnapshot.value))
        console.log('数据值列表:', Object.values(latestSnapshot.value))
        console.log('数据条目数:', Object.keys(latestSnapshot.value).length)
      } catch (e: any) {
        console.warn('HTTP方式获取数据失败:', e.message)
        latestSnapshot.value = {}
      }
    }
  } catch (e: any) {
    alert('加载点位失败：' + (e.message || String(e)))
  } finally {
    pointsLoading.value = false
  }
}

async function closePointsModal() {
  // 清理WebSocket连接
  if (selectedDeviceId.value && isConnected.value) {
    await websocketService.leaveDeviceMonitoring(selectedDeviceId.value)
    // 移除事件监听
    websocketService.off('realtimeDataUpdate', handleRealtimeDataUpdate)
    websocketService.off('deviceDataUpdate', handleDeviceDataUpdate)
    websocketService.off('pointValueUpdate', handlePointValueUpdate)
  }
  
  showPointsModal.value = false
  selectedDeviceId.value = null
  selectedDeviceName.value = ''
  selectedDeviceProtocol.value = ''
  devicePoints.value = []
  latestSnapshot.value = {}
  clearSelection()
  if (latestTimer) { clearInterval(latestTimer); latestTimer = null }
}

async function handleImportTemplate() {
  if (!selectedDeviceId.value || !importTemplateId.value) return
  try {
    const result = await importDevicePointsFromTemplate(selectedDeviceId.value, importTemplateId.value)
    // 重新加载点位
    devicePoints.value = await getDevicePoints(selectedDeviceId.value)
    alert(result.message || '导入成功')
  } catch (e: any) {
    alert('导入失败：' + (e.message || String(e)))
  }
}

// 批量操作函数
function togglePointSelection(pointId: number) {
  const index = selectedPoints.value.indexOf(pointId)
  if (index > -1) {
    selectedPoints.value.splice(index, 1)
  } else {
    selectedPoints.value.push(pointId)
  }
  showBatchActions.value = selectedPoints.value.length > 0
}

function selectAllPoints() {
  selectedPoints.value = devicePoints.value.map(p => p.id)
  showBatchActions.value = true
}

function clearSelection() {
  selectedPoints.value = []
  showBatchActions.value = false
}

async function batchDeletePoints() {
  if (selectedPoints.value.length === 0) return
  if (!confirm(`确认删除选中的 ${selectedPoints.value.length} 个点位？`)) return
  
  try {
    const result = await batchDeleteDevicePoints(selectedDeviceId.value!, selectedPoints.value)
    // 重新加载点位
    devicePoints.value = await getDevicePoints(selectedDeviceId.value!)
    clearSelection()
    alert(result.message)
  } catch (e: any) {
    alert('批量删除失败：' + (e.message || String(e)))
  }
}

async function batchEnablePoints() {
  if (selectedPoints.value.length === 0) return
  
  try {
    const result = await batchUpdateDevicePointsStatus(selectedDeviceId.value!, selectedPoints.value, true)
    // 重新加载点位
    devicePoints.value = await getDevicePoints(selectedDeviceId.value!)
    clearSelection()
    alert(result.message)
  } catch (e: any) {
    alert('批量启用失败：' + (e.message || String(e)))
  }
}

async function batchDisablePoints() {
  if (selectedPoints.value.length === 0) return
  
  try {
    const result = await batchUpdateDevicePointsStatus(selectedDeviceId.value!, selectedPoints.value, false)
    // 重新加载点位
    devicePoints.value = await getDevicePoints(selectedDeviceId.value!)
    clearSelection()
    alert(result.message)
  } catch (e: any) {
    alert('批量禁用失败：' + (e.message || String(e)))
  }
}

// 异步刷新快照数据
let refreshInProgress = false
async function refreshSnapshotDataAsync() {
  // 防止重复刷新
  if (refreshInProgress) {
    console.log('刷新已在进行中，跳过本次请求')
    return
  }
  
  refreshInProgress = true
  try {
    console.log('开始异步刷新快照数据...')
    const freshSnapshot = await getGatewayLatestSnapshot()
    if (freshSnapshot && Object.keys(freshSnapshot).length > 0) {
      latestSnapshot.value = freshSnapshot
      console.log('异步刷新快照数据成功，数据项数量:', Object.keys(freshSnapshot).length)
    } else {
      console.log('异步刷新快照数据成功，但数据为空')
    }
  } catch (error) {
    console.warn('异步刷新快照数据失败:', error)
  } finally {
    refreshInProgress = false
  }
}

function getPointLatestValue(p: DevicePointDto, deviceId?: number) {
  console.log('=== 开始查找点位实时值 ===')
  console.log('点位信息:', {
    id: p.id,
    name: p.name,
    address: p.address,
    functionCode: p.functionCode,
    deviceId: deviceId || selectedDeviceId.value,
    deviceName: selectedDeviceName.value
  })
  
  // 确定使用哪个快照数据
  let snapshotData: Record<string, any> = {}
  const actualDeviceId = deviceId || selectedDeviceId.value
  
  if (actualDeviceId && deviceSnapshots.value[actualDeviceId] && Object.keys(deviceSnapshots.value[actualDeviceId]).length > 0) {
    // 子表格模式：使用设备特定的快照（如果存在且不为空）
    snapshotData = deviceSnapshots.value[actualDeviceId]
    console.log(`使用设备 ${actualDeviceId} 的快照数据`)
  } else {
    // 弹窗模式或设备快照为空：使用全局快照并进行设备特定过滤
    const globalSnapshot = latestSnapshot.value
    console.log('使用全局快照数据')
    
    // 如果全局快照数据为空或数据量很少，触发异步刷新（不等待结果）
    if (!globalSnapshot || Object.keys(globalSnapshot).length < 3) {
      console.log('全局快照数据不足，触发异步刷新...')
      // 只有在没有其他刷新正在进行时才触发新的刷新
      if (!refreshInProgress) {
        refreshSnapshotDataAsync()
      }
      snapshotData = globalSnapshot || {}
    } else {
      // 对全局快照进行设备特定过滤
      const device = devices.value.find(d => d.id === actualDeviceId)
      if (device) {
        const protocolKey = `${device.protocol}_${device.ipAddress}_${device.port}`
        snapshotData = {}
        for (const [key, value] of Object.entries(globalSnapshot)) {
          if (key.startsWith(protocolKey + '.')) {
            snapshotData[key] = value
          }
        }
        console.log(`从全局快照中过滤出设备 ${actualDeviceId} 的数据:`, snapshotData)
      } else {
        snapshotData = globalSnapshot
        console.log('未找到设备信息，使用完整全局快照')
      }
    }
  }
  
  if (!snapshotData || Object.keys(snapshotData).length === 0) {
    console.log('快照数据为空，返回无数据')
    return '无数据'
  }
  
  // 调试信息：打印快照数据
  console.log('当前快照数据:', snapshotData)
  console.log('快照数据类型:', typeof snapshotData)
  console.log('快照数据键数量:', Object.keys(snapshotData).length)
  console.log('快照数据键列表:', Object.keys(snapshotData))
  console.log('快照数据值列表:', Object.values(snapshotData))
  
  // 多种映射方式尝试 - 修复协议键格式匹配
  const device = devices.value.find(d => d.id === actualDeviceId)
  let protocolKey = device ? `${device.protocol}_${device.ipAddress}_${device.port}` : ''
  
  // 如果设备已被删除，尝试从全局快照中推断协议键
  if (!protocolKey && latestSnapshot.value) {
    const snapshotKeys = Object.keys(latestSnapshot.value)
    // 查找包含当前点位名称或地址的键
    const matchingKey = snapshotKeys.find(key => 
      key.endsWith(`.${p.name}`) || key.endsWith(`.${p.address}`)
    )
    if (matchingKey) {
      protocolKey = matchingKey.split('.').slice(0, -1).join('.')
      console.log(`从全局快照推断出协议键: ${protocolKey}`)
    }
  }
  
  // 调试信息
  console.log('设备查找调试:', {
    actualDeviceId: actualDeviceId,
    selectedDeviceId: selectedDeviceId.value,
    device: device,
    protocolKey: protocolKey,
    pointName: p.name,
    pointAddress: p.address
  })
  
  // 简化的搜索键逻辑 - 只使用标准协议键格式
  const searchKeys = []
  
  if (protocolKey) {
    // 标准协议键格式（唯一正确的格式）
    searchKeys.push(
      `${protocolKey}.${p.name}`, // ModbusTCP_192.168.6.6_502.Point_10
      `${protocolKey}.${p.address}` // ModbusTCP_192.168.6.6_502.4509
    )
  }
  
  // 尝试各种可能的键
  console.log('开始尝试搜索键:', searchKeys)
  for (const key of searchKeys) {
    console.log(`尝试键: "${key}", 值: ${snapshotData[key]}, 类型: ${typeof snapshotData[key]}`)
    if (key && snapshotData[key] !== undefined) {
      console.log(`✅ 找到数据！键: "${key}", 值: ${snapshotData[key]}`)
      return snapshotData[key]
    }
  }
  console.log('❌ 所有搜索键都未找到匹配数据')
  
  // 如果都没找到，尝试协议键模糊匹配
  const snapshotKeys = Object.keys(snapshotData)
  console.log('快照中的所有键:', snapshotKeys)
  
  if (protocolKey) {
    // 只匹配协议键格式的键
    console.log('开始协议键模糊匹配...')
    const protocolMatch = snapshotKeys.find(key => {
      return key.startsWith(protocolKey + '.') && 
             (key.endsWith(p.address) || key.endsWith(p.name))
    })
    if (protocolMatch) {
      console.log(`✅ 协议键模糊匹配找到: "${protocolMatch}", 值: ${snapshotData[protocolMatch]}`)
      return snapshotData[protocolMatch]
    }
    console.log('❌ 协议键模糊匹配未找到')
  }
  
  console.log('=== 所有匹配方式都失败，返回无数据 ===')
  return '无数据'
}

// WebSocket事件处理函数
function handleRealtimeDataUpdate(data: RealtimeDataUpdate) {
  console.log('=== 收到实时数据更新 ===')
  console.log('更新数据类型:', typeof data)
  console.log('更新数据键数量:', Object.keys(data || {}).length)
  console.log('更新数据键列表:', Object.keys(data || {}))
  console.log('更新数据值列表:', Object.values(data || {}))
  console.log('更新前快照数据:', latestSnapshot.value)

  // 合并更新：避免覆盖现有快照，防止其他协议/设备数据被清空
  latestSnapshot.value = { ...latestSnapshot.value, ...data }

  console.log('更新后快照数据:', latestSnapshot.value)
  console.log('=== 实时数据更新完成 ===')
}

function handleDeviceDataUpdate(data: any) {
  console.log('=== 收到设备数据更新 ===')
  console.log('设备数据类型:', typeof data)
  console.log('设备数据键数量:', Object.keys(data || {}).length)
  console.log('设备数据内容:', data)
  console.log('更新前快照数据:', latestSnapshot.value)
  
  // 合并到现有快照中
  latestSnapshot.value = { ...latestSnapshot.value, ...data }
  
  console.log('更新后快照数据:', latestSnapshot.value)
  console.log('=== 设备数据更新完成 ===')
}

function handlePointValueUpdate(data: PointValueUpdate) {
  console.log('=== 收到点位数值更新 ===')
  console.log('点位数据详情:', {
    DeviceId: data.DeviceId,
    PointId: data.PointId,
    Address: data.Address,
    Value: data.Value,
    Timestamp: data.Timestamp
  })
  console.log('当前选中设备ID:', selectedDeviceId.value)
  
  // 更新特定点位的值
  if (data.DeviceId === selectedDeviceId.value) {
    console.log('设备ID匹配，更新点位值')
    console.log('更新前快照数据:', latestSnapshot.value)
    
    latestSnapshot.value[data.Address] = data.Value
    // 触发响应式更新
    latestSnapshot.value = { ...latestSnapshot.value }
    
    console.log('更新后快照数据:', latestSnapshot.value)
  } else {
    console.log('设备ID不匹配，跳过更新')
  }
  console.log('=== 点位数值更新完成 ===')
}

// 手动刷新实时数据
async function refreshRealtimeData() {
  try {
    if (isConnected.value) {
      console.log('WebSocket已连接，无需手动刷新')
      alert('WebSocket实时连接中，数据自动更新')
    } else {
      console.log('手动刷新实时数据...')
      latestSnapshot.value = await getGatewayLatestSnapshot()
      console.log('手动刷新获取到的数据:', latestSnapshot.value)
      console.log('手动刷新-数据键列表:', Object.keys(latestSnapshot.value))
      console.log('手动刷新-数据条目数:', Object.keys(latestSnapshot.value).length)
      alert('实时数据已刷新')
    }
  } catch (e: any) {
    console.error('手动刷新失败:', e)
    alert('刷新失败：' + (e.message || String(e)))
  }
}

async function savePoint(p: DevicePointDto) {
  if (!selectedDeviceId.value) return
  try {
    const updateData: any = {
      name: p.name,
      address: p.address,
      dataType: p.dataType,
      access: p.access,
      intervalMs: p.intervalMs,
      enabled: p.enabled
    }
    
    // 如果是Modbus设备，保存Modbus特有配置
    const isModbus = selectedDeviceProtocol.value.toLowerCase().includes('modbus')
    if (isModbus) {
      updateData.functionCode = p.functionCode
      updateData.addressType = p.addressType
      updateData.quantity = p.quantity
      updateData.slaveId = p.slaveId
    }
    
    const updated = await updateDevicePoint(selectedDeviceId.value, p.id, updateData)
    Object.assign(p, updated)
    // 点位更新后：失效该设备快照并触发一次轻量刷新，避免旧数据残留
    delete deviceSnapshots.value[selectedDeviceId.value]
    if (!refreshInProgress) refreshSnapshotDataAsync()
    alert('保存成功')
  } catch (e: any) {
    console.error('保存点位失败:', e)
    const errorMessage = e.response?.data?.message || e.message || '未知错误'
    alert(`保存失败：${errorMessage}`)
  }
}

async function removePoint(p: DevicePointDto) {
  if (!selectedDeviceId.value) return
  if (!confirm(`确认删除点位 ${p.name}?`)) return
  try {
    await deleteDevicePoint(selectedDeviceId.value, p.id)
    devicePoints.value = devicePoints.value.filter(x => x.id !== p.id)
    // 失效该设备的快照缓存，避免旧数据残留
    delete deviceSnapshots.value[selectedDeviceId.value]
    // 触发一次轻量级刷新（不阻塞渲染）
    if (!refreshInProgress) refreshSnapshotDataAsync()
    alert('删除成功')
  } catch (e: any) {
    alert('删除失败：' + (e.message || String(e)))
  }
}

async function addPoint() {
  if (!selectedDeviceId.value) return
  try {
    // 生成唯一的节点名称和地址
    const existingPoints = devicePoints.value
    const pointCount = existingPoints.length + 1
    
    // 检查地址是否已存在，如果存在则递增
    let baseAddress = 40001
    let address = baseAddress.toString()
    while (existingPoints.some(p => p.address === address)) {
      baseAddress++
      address = baseAddress.toString()
    }
    
    // 根据设备协议类型设置默认配置
    const isModbus = selectedDeviceProtocol.value.toLowerCase().includes('modbus')
    const pointConfig: any = {
      name: `Point_${pointCount}`,
      address: address,
      dataType: 'INT16',
      access: 'R',
      intervalMs: 1000,
      enabled: true
    }
    
    // 如果是Modbus设备，添加Modbus特有配置
    if (isModbus) {
      pointConfig.functionCode = 3 // 默认读保持寄存器
      pointConfig.addressType = 'dec'
      pointConfig.quantity = 1
      pointConfig.slaveId = 1
    }
    
    const created = await createDevicePoint(selectedDeviceId.value, pointConfig)
    devicePoints.value.push(created)
    // 新增后清理该设备快照，避免未包含新点位的数据污染
    delete deviceSnapshots.value[selectedDeviceId.value]
    if (!refreshInProgress) refreshSnapshotDataAsync()
    alert('点位创建成功')
  } catch (e: any) {
    console.error('新增点位失败:', e)
    const errorMessage = e.response?.data?.message || e.message || '未知错误'
    alert(`新增失败：${errorMessage}`)
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

function getStatusColor(status: number, deviceId?: number) {
  // 如果有设备ID，检查是否有有效的实时数据
  if (deviceId && deviceSnapshots.value[deviceId]) {
    const snapshot = deviceSnapshots.value[deviceId]
    if (snapshot && Object.keys(snapshot).length > 0) {
      // 检查快照数据是否包含有效值（不是空字符串或null）
      const hasValidData = Object.values(snapshot).some(value => 
        value !== null && value !== undefined && value !== '' && value !== '无数据'
      )
      if (hasValidData) {
        return '#28a745' // 在线 - 绿色
      }
    }
  }
  
  // 否则使用数据库状态
  switch (status) {
    case 1: return '#28a745' // 在线
    case 0: return '#dc3545' // 离线
    case 2: return '#ffc107' // 连接中
    case 3: return '#dc3545' // 错误
    case 4: return '#6c757d' // 维护中
    default: return '#6c757d'
  }
}

// 移除未使用的 getStatusIcon，避免 TS6133

// 搜索功能
// 已未使用；若需要再启用。避免构建告警
/* async function handleSearch() {
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
} */

// 创建设备
/* async function handleCreateDevice(deviceData: CreateDeviceRequest) {
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
} */

// 更新设备
/* async function handleUpdateDevice(id: number, deviceData: UpdateDeviceRequest) {
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
} */

// 删除设备
async function handleDeleteDevice(id: number) {
  loading.value = true
  error.value = null
  
  try {
    // 获取要删除的设备信息，用于清理全局快照数据
    const deviceToDelete = devices.value.find(d => d.id === id)
    const protocolKey = deviceToDelete ? `${deviceToDelete.protocol}_${deviceToDelete.ipAddress}_${deviceToDelete.port}` : null
    
    await deleteDevice(id)
    
    // 从设备列表中移除
    devices.value = devices.value.filter(d => d.id !== id)
    
    // 清理相关缓存数据
    delete deviceSnapshots.value[id]
    delete devicePointsCache.value[id]
    expandedDevices.value.delete(id)
    
    // 清理全局快照中该设备的数据
    if (protocolKey && latestSnapshot.value) {
      const keysToDelete = Object.keys(latestSnapshot.value).filter(key => key.startsWith(protocolKey + '.'))
      keysToDelete.forEach(key => delete latestSnapshot.value[key])
      console.log(`从全局快照中清理了设备 ${id} 的 ${keysToDelete.length} 个数据项`)
    }
    // 触发一次轻量级刷新，确保UI尽快反映删除后的状态
    if (!refreshInProgress) refreshSnapshotDataAsync()
    
    console.log(`设备 ${id} 已删除，相关缓存已清理`)
    
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
function getStatusText(status: number, deviceId?: number): string {
  // 如果有设备ID，检查是否有有效的实时数据
  if (deviceId && deviceSnapshots.value[deviceId]) {
    const snapshot = deviceSnapshots.value[deviceId]
    if (snapshot && Object.keys(snapshot).length > 0) {
      // 检查快照数据是否包含有效值（不是空字符串或null）
      const hasValidData = Object.values(snapshot).some(value => 
        value !== null && value !== undefined && value !== '' && value !== '无数据'
      )
      if (hasValidData) {
        return '在线'
      }
    }
  }
  
  // 否则使用数据库状态
  switch (status) {
    case 0: return '离线'
    case 1: return '在线'
    case 2: return '连接中'
    case 3: return '错误'
    case 4: return '维护中'
    default: return '未知'
  }
}

onMounted(async () => {
  await fetchDevices()
  // 启动定时刷新，保持状态显示准确
  startDeviceDataRefresh()
})

// 组件卸载时清理定时器
onUnmounted(() => {
  stopDeviceDataRefresh()
})

async function handleReload(deviceId: number) {
  try {
    await reloadDevice(deviceId)
    alert('已触发网关重载')
  } catch (e: any) {
    alert('重载失败：' + (e.message || String(e)))
  }
}

// 写入对话框
const showWriteModal = ref(false)
const writeValue = ref<any>('')
const writeTargetPoint = ref<DevicePointDto | null>(null)
const writeLoading = ref(false)

// 写入相关
function openWriteDialog(p: DevicePointDto) {
  writeTargetPoint.value = p
  writeValue.value = ''
  showWriteModal.value = true
}

async function confirmWrite() {
  if (!selectedDeviceId.value || !writeTargetPoint.value) return
  try {
    // 基于点位数据类型做一次前端类型校验与转换
    let payloadValue: any = writeValue.value
    const dt = (writeTargetPoint.value.dataType || '').toUpperCase()
    if (dt.includes('INT')) {
      const num = Number(payloadValue)
      if (!Number.isFinite(num)) { alert('请输入数值'); return }
      payloadValue = Math.trunc(num)
    } else if (dt.includes('FLOAT') || dt.includes('DOUBLE')) {
      const num = Number(payloadValue)
      if (!Number.isFinite(num)) { alert('请输入数值'); return }
      payloadValue = num
    }

    writeLoading.value = true
    // 映射写入功能码：读保持(3)->写保持(6); 线圈读(1)->写(5); 输入寄存器(4)/离散输入(2)不支持写
    const srcFc = writeTargetPoint.value.functionCode ?? 3
    let writeFc: number | undefined = undefined
    if (srcFc === 3) writeFc = 6
    else if (srcFc === 1) writeFc = 5
    else if (srcFc === 4 || srcFc === 2) { alert('该点类型为只读，无法写入'); return }
    else writeFc = 6

    const req: WriteCommandRequest = {
      address: writeTargetPoint.value.address,
      functionCode: writeFc,
      value: payloadValue,
      readBack: true
    }
    const result = await writeDeviceCommand(selectedDeviceId.value, req)
    console.log('写入结果:', result)
    // 本地乐观更新：更新该设备快照与全局快照对应键
    try {
      const device = devices.value.find(d => d.id === selectedDeviceId.value)
      const p = writeTargetPoint.value
      if (device && p) {
        const protocolKey = `${device.protocol}_${device.ipAddress}_${device.port}`
        const nameKey = `${protocolKey}.${p.name}`
        const addrKey = `${protocolKey}.${p.address}`
        const v = (result && (result as any).value) ?? payloadValue
        // 更新全局
        latestSnapshot.value = { ...latestSnapshot.value, [nameKey]: v, [addrKey]: v }
        // 更新设备快照
        const did = selectedDeviceId.value as number
        if (!deviceSnapshots.value[did]) deviceSnapshots.value[did] = {}
        deviceSnapshots.value[did][nameKey] = v
        deviceSnapshots.value[did][addrKey] = v
      }
    } catch {}
    // 关闭弹窗
    showWriteModal.value = false
    // 轻量刷新，等待WS优先
    if (!refreshInProgress) refreshSnapshotDataAsync()
    alert('写入成功')
  } catch (e: any) {
    alert('写入失败：' + (e.message || String(e)))
  } finally {
    writeLoading.value = false
  }
}

function cancelWrite() {
  showWriteModal.value = false
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
          <button class="btn-refresh" @click="forceRefreshStatus" :disabled="loading" title="刷新设备状态">
            <i class="fas fa-heartbeat"></i>
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
            <template v-for="device in filteredDevices" :key="device.id">
              <!-- 设备主行 -->
              <tr class="device-row" @click="toggleDeviceExpansion(device.id)">
                <td class="device-name">
                  <div class="device-info">
                    <div class="expand-icon" :class="{ expanded: expandedDevices.has(device.id) }">
                      <i class="fas fa-chevron-right"></i>
                    </div>
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
                    :style="{ backgroundColor: getStatusColor(device.status, device.id) }"
                  ></div>
                  <span class="status-text">{{ getStatusText(device.status, device.id) }}</span>
                </div>
            </td>
              <td class="device-time">2分钟前</td>
              <td class="device-actions" @click.stop>
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

              <!-- 设备点位子表格 -->
              <tr v-if="expandedDevices.has(device.id)" class="points-row">
                <td colspan="5" class="points-container">
                  <div class="points-subtable">
                    <div class="points-header">
                      <h4>{{ device.name }} - 点位列表</h4>
                      <div class="points-actions">
                        <button class="btn-refresh-small" @click="refreshDevicePoints(device.id)" title="刷新点位">
                          <i class="fas fa-sync-alt"></i>
                        </button>
                        <button class="btn-add-small" @click="openDeviceDetails(device)" title="管理点位">
                          <i class="fas fa-cog"></i>
                        </button>
                      </div>
                    </div>
                    
                    <div v-if="!devicePointsCache[device.id] || devicePointsCache[device.id]?.length === 0" class="no-points">
                      <i class="fas fa-info-circle"></i>
                      <span>暂无点位数据</span>
                    </div>
                    
                    <div v-else class="points-grid">
                      <div 
                        v-for="point in devicePointsCache[device.id]" 
                        :key="point.id" 
                        class="point-item"
                      >
                        <div class="point-header">
                          <span class="point-name">{{ point.name }}</span>
                          <span class="point-address">{{ point.address }}</span>
                        </div>
                        <div class="point-value">
                          <span class="value-label">实时值:</span>
                          <span class="value-data" :class="{ 'no-data': getPointLatestValue(point, device.id) === '无数据' }">
                            {{ getPointLatestValue(point, device.id) }}
                          </span>
                        </div>
                        <div class="point-meta">
                          <span class="point-type">{{ point.dataType }}</span>
                          <span v-if="point.functionCode" class="point-func">FC{{ point.functionCode }}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </td>
              </tr>
            </template>
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
    <div class="modal-container points-modal">
      <div class="modal-header">
        <div class="header-content">
          <div class="device-info">
            <div class="device-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
              </svg>
            </div>
            <div>
              <h3>{{ selectedDeviceName }}</h3>
              <p class="device-subtitle">设备点位管理</p>
            </div>
          </div>
          <div class="header-actions">
            <div class="refresh-indicator" :class="{ active: latestTimer }">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
              </svg>
              <span>实时更新</span>
            </div>
            <button type="button" class="modal-close" @click="closePointsModal">
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>
      </div>
      <div class="modal-body">
        <div v-if="pointsLoading" class="loading-state">
          <div class="loading-spinner"></div>
          <span>加载点位中...</span>
        </div>
        <div v-else>
          <!-- 操作工具栏 -->
          <div class="points-toolbar">
            <div class="toolbar-left">
              <div class="template-import">
                <select v-model.number="importTemplateId" class="template-select">
                  <option :value="null">选择模板导入</option>
                  <option v-for="t in templateOptions" :key="t.id" :value="t.id">{{ t.name }} ({{ t.protocol }})</option>
                </select>
                <button class="btn-import" @click="handleImportTemplate" :disabled="!selectedDeviceId || !importTemplateId">
                  <i class="fas fa-download"></i>
                  导入
                </button>
              </div>
            </div>
            <div class="toolbar-right">
              <div v-if="showBatchActions" class="batch-actions">
                <span class="batch-info">已选择 {{ selectedPoints.length }} 个点位</span>
                <button class="btn-batch-enable" @click="batchEnablePoints">
                  <i class="fas fa-check"></i>
                  批量启用
                </button>
                <button class="btn-batch-disable" @click="batchDisablePoints">
                  <i class="fas fa-times"></i>
                  批量禁用
                </button>
                <button class="btn-batch-delete" @click="batchDeletePoints">
                  <i class="fas fa-trash"></i>
                  批量删除
                </button>
                <button class="btn-clear-selection" @click="clearSelection">
                  <i class="fas fa-times-circle"></i>
                  取消选择
                </button>
              </div>
              <div v-else class="toolbar-actions">
                <button class="btn-select-all" @click="selectAllPoints" v-if="devicePoints.length > 0">
                  <i class="fas fa-check-square"></i>
                  全选
                </button>
                <div class="connection-status" :class="{ connected: isConnected }" :title="isConnected ? 'WebSocket已连接' : 'WebSocket未连接'">
                  <i class="fas fa-circle"></i>
                  {{ isConnected ? '实时' : '离线' }}
                </div>
                <button class="btn-refresh-data" @click="refreshRealtimeData" title="刷新实时数据">
                  <i class="fas fa-sync-alt"></i>
                  刷新数据
                </button>
                <button class="btn-add-point" @click="addPoint">
                  <i class="fas fa-plus"></i>
                  新增点位
                </button>
              </div>
            </div>
          </div>

          <!-- 点位列表 -->
          <div class="points-container">
            <div v-if="devicePoints.length === 0" class="empty-points">
              <div class="empty-icon">
                <svg width="64" height="64" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
                </svg>
              </div>
              <h4>暂无点位</h4>
              <p>点击"新增点位"按钮开始配置设备点位</p>
            </div>
            <div v-else class="points-grid">
              <div v-for="p in devicePoints" :key="p.id" class="point-card" :class="{ disabled: !p.enabled, selected: selectedPoints.includes(p.id) }">
                <div class="point-header">
                  <div class="point-selection">
                    <input 
                      type="checkbox" 
                      :checked="selectedPoints.includes(p.id)"
                      @change="togglePointSelection(p.id)"
                      class="point-checkbox"
                    />
                  </div>
                  <div class="point-name">
                    <input v-model="p.name" class="point-input name-input" placeholder="点位名称" />
                  </div>
                  <div class="point-status">
                    <div class="status-indicator" :class="{ active: p.enabled }"></div>
                    <span class="status-text">{{ p.enabled ? '启用' : '禁用' }}</span>
                  </div>
                </div>
                
                <div class="point-content">
                  <div class="point-row">
                    <label>地址</label>
                    <input v-model="p.address" class="point-input" placeholder="40001" />
                  </div>
                  
                  <!-- Modbus 特有配置 -->
                  <template v-if="selectedDeviceProtocol.toLowerCase().includes('modbus')">
                    <div class="point-row">
                      <label>功能码</label>
                      <select v-model.number="p.functionCode" class="point-select">
                        <option v-for="fc in modbusFunctionCodes" :key="fc.value" :value="fc.value">
                          {{ fc.label }}
                        </option>
                      </select>
                    </div>
                    <div class="point-row">
                      <label>地址类型</label>
                      <select v-model="p.addressType" class="point-select">
                        <option v-for="at in addressTypes" :key="at.value" :value="at.value">
                          {{ at.label }}
                        </option>
                      </select>
                    </div>
                  <div class="point-row">
                    <label>数量</label>
                    <input v-model.number="p.quantity" type="number" class="point-input" placeholder="1" min="1" />
                  </div>
                  <div class="point-row">
                    <label>从站ID</label>
                    <input v-model.number="p.slaveId" type="number" class="point-input" placeholder="1" min="1" max="247" />
                  </div>
                  </template>
                  
                  <div class="point-row">
                    <label>类型</label>
                    <select v-model="p.dataType" class="point-select">
                      <option value="INT16">INT16</option>
                      <option value="INT32">INT32</option>
                      <option value="FLOAT32">FLOAT32</option>
                      <option value="BOOL">BOOL</option>
                      <option value="STRING">STRING</option>
                    </select>
                  </div>
                  <div class="point-row">
                    <label>访问</label>
                    <select v-model="p.access" class="point-select">
                      <option value="R">只读</option>
                      <option value="RW">读写</option>
                    </select>
                  </div>
                  <div class="point-row">
                    <label>读取周期</label>
                    <input v-model.number="p.intervalMs" type="number" class="point-input" placeholder="1000" />
                    <span class="unit-text">ms</span>
                  </div>
                </div>

                <div class="point-value">
                  <div class="value-label">实时值</div>
                  <div class="value-display" :class="{ 'no-value': getPointLatestValue(p) === '无数据' }">
                    <span class="value-text">{{ getPointLatestValue(p) }}</span>
                  </div>
                </div>

                <div class="point-actions">
                  <button class="btn-save" @click="savePoint(p)" title="保存">
                    <i class="fas fa-save"></i>
                  </button>
                  <button class="btn-delete" @click="removePoint(p)" title="删除">
                    <i class="fas fa-trash"></i>
                  </button>
                  <button class="btn-save" @click.stop="openWriteDialog(p)" title="写入">
                    <i class="fas fa-pen"></i>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="modal-footer">
        <div class="footer-info">
          <span class="points-count">共 {{ devicePoints.length }} 个点位</span>
          <span class="enabled-count">启用 {{ devicePoints.filter(p => p.enabled).length }} 个</span>
        </div>
        <button type="button" class="btn-close" @click="closePointsModal">关闭</button>
      </div>
    </div>
  </div>

  <div v-if="showWriteModal" class="modal-overlay write-overlay" @click.self="cancelWrite">
    <div class="modal" @click.stop>
      <h3>写入点位</h3>
      <p v-if="writeTargetPoint">目标: {{ writeTargetPoint.name }} (地址 {{ writeTargetPoint.address }})</p>
      <input v-model="writeValue" placeholder="输入写入值" />
      <div class="modal-actions">
        <button class="btn-primary" type="button" :disabled="writeLoading" @click.stop.prevent="confirmWrite">{{ writeLoading ? '写入中...' : '确认' }}</button>
        <button class="btn-secondary" type="button" :disabled="writeLoading" @click.stop.prevent="cancelWrite">取消</button>
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

/* 展开图标样式 */
.expand-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  margin-right: 8px;
  transition: transform 0.2s ease;
  cursor: pointer;
}

.expand-icon.expanded {
  transform: rotate(90deg);
}

.expand-icon i {
  font-size: 12px;
  color: #666;
}

/* 设备行点击样式 */
.device-row {
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.device-row:hover {
  background-color: #f8f9fa;
}

/* 点位子表格样式 */
.points-row {
  background-color: #f8f9fa;
}

.points-container {
  padding: 0;
  border-top: 1px solid #e9ecef;
}

.points-subtable {
  padding: 12px;
  background: white;
  margin: 0 8px 8px 8px;
  border-radius: 6px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.points-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
  padding-bottom: 8px;
  border-bottom: 1px solid #e9ecef;
}

.points-header h4 {
  margin: 0;
  color: #333;
  font-size: 14px;
  font-weight: 600;
}

.points-actions {
  display: flex;
  gap: 8px;
}

.btn-refresh-small, .btn-add-small {
  padding: 4px 8px;
  border: none;
  border-radius: 3px;
  cursor: pointer;
  font-size: 11px;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 3px;
}

.btn-refresh-small {
  background-color: #e3f2fd;
  color: #1976d2;
}

.btn-refresh-small:hover {
  background-color: #bbdefb;
}

.btn-add-small {
  background-color: #e8f5e8;
  color: #2e7d32;
}

.btn-add-small:hover {
  background-color: #c8e6c9;
}

/* 无点位提示 */
.no-points {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  color: #666;
  font-size: 12px;
  gap: 6px;
}

.no-points i {
  font-size: 14px;
}

/* 点位网格 */
.points-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 8px;
}

.point-item {
  background: #f8f9fa;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  padding: 10px;
  transition: all 0.2s ease;
}

.point-item:hover {
  background: #e9ecef;
  border-color: #dee2e6;
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.point-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.point-name {
  font-weight: 600;
  color: #333;
  font-size: 13px;
}

.point-address {
  background: #007bff;
  color: white;
  padding: 1px 6px;
  border-radius: 10px;
  font-size: 10px;
  font-weight: 500;
}

.point-value {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 6px;
  padding: 6px 8px;
  background: white;
  border-radius: 4px;
  border: 1px solid #dee2e6;
}

.value-label {
  font-size: 11px;
  color: #666;
  font-weight: 500;
}

.value-data {
  font-weight: 600;
  color: #28a745;
  font-size: 13px;
}

.value-data.no-data {
  color: #dc3545;
}

.point-meta {
  display: flex;
  gap: 6px;
  font-size: 10px;
}

.point-type {
  background: #6c757d;
  color: white;
  padding: 1px 4px;
  border-radius: 3px;
}

.point-func {
  background: #17a2b8;
  color: white;
  padding: 1px 4px;
  border-radius: 3px;
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

.device-actions {
  display: flex;
  gap: 8px;
  align-items: center;
  justify-content: center;
  height: 100%;
  min-height: 48px;
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

/* 删除重复的样式定义 */

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

/* 设备点位弹窗样式 */
.points-modal {
  width: 95vw;
  max-width: 1200px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
}

.points-modal .modal-header {
  padding: 20px 24px;
  border-bottom: 1px solid #eef1f4;
  background: linear-gradient(135deg, #f8f9fa, #ffffff);
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.device-info {
  display: flex;
  align-items: center;
  gap: 16px;
}

.device-icon {
  width: 48px;
  height: 48px;
  background: linear-gradient(135deg, #4a90e2, #357abd);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  box-shadow: 0 4px 12px rgba(74, 144, 226, 0.3);
}

.device-info h3 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: #2c3e50;
}

.device-subtitle {
  margin: 4px 0 0;
  font-size: 14px;
  color: #6c757d;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}

.refresh-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background: rgba(74, 144, 226, 0.1);
  border-radius: 20px;
  color: #4a90e2;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.3s ease;
}

.refresh-indicator.active {
  background: rgba(40, 167, 69, 0.1);
  color: #28a745;
  animation: pulse 2s infinite;
}

.refresh-indicator svg {
  animation: rotate 2s linear infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.7; }
}

@keyframes rotate {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.points-modal .modal-body {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
}

.points-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding: 16px;
  background: #f8f9fa;
  border-radius: 12px;
  border: 1px solid #e9ecef;
}

.template-import {
  display: flex;
  gap: 12px;
  align-items: center;
}

.template-select {
  padding: 10px 16px;
  border: 1px solid #e6e9ee;
  border-radius: 8px;
  background: white;
  font-size: 14px;
  min-width: 200px;
}

.btn-import {
  padding: 10px 16px;
  background: linear-gradient(135deg, #6c757d, #5a6268);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s ease;
}

.btn-import:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(108, 117, 125, 0.3);
}

.btn-import:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-add-point {
  padding: 12px 20px;
  background: linear-gradient(135deg, #28a745, #20c997);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s ease;
}

.btn-add-point:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(40, 167, 69, 0.3);
}

.btn-refresh-data {
  padding: 10px 16px;
  background: linear-gradient(135deg, #17a2b8, #138496);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s ease;
}

.btn-refresh-data:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(23, 162, 184, 0.3);
}

.btn-refresh-data:active {
  transform: rotate(180deg);
}

.connection-status {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 500;
  background: rgba(220, 53, 69, 0.1);
  color: #dc3545;
  border: 1px solid rgba(220, 53, 69, 0.2);
  transition: all 0.3s ease;
}

.connection-status.connected {
  background: rgba(40, 167, 69, 0.1);
  color: #28a745;
  border: 1px solid rgba(40, 167, 69, 0.2);
}

.connection-status i {
  font-size: 8px;
  animation: pulse 2s infinite;
}

.connection-status.connected i {
  animation: none;
}

@keyframes pulse {
  0% { opacity: 1; }
  50% { opacity: 0.5; }
  100% { opacity: 1; }
}

.toolbar-actions {
  display: flex;
  gap: 12px;
  align-items: center;
}

.btn-select-all {
  padding: 10px 16px;
  background: linear-gradient(135deg, #6c757d, #5a6268);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s ease;
}

.btn-select-all:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(108, 117, 125, 0.3);
}

.batch-actions {
  display: flex;
  gap: 12px;
  align-items: center;
  padding: 12px 16px;
  background: rgba(74, 144, 226, 0.1);
  border-radius: 8px;
  border: 1px solid rgba(74, 144, 226, 0.2);
}

.batch-info {
  font-size: 14px;
  font-weight: 500;
  color: #4a90e2;
  margin-right: 8px;
}

.btn-batch-enable, .btn-batch-disable, .btn-batch-delete, .btn-clear-selection {
  padding: 8px 12px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 12px;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: all 0.3s ease;
}

.btn-batch-enable {
  background: rgba(40, 167, 69, 0.1);
  color: #28a745;
  border: 1px solid rgba(40, 167, 69, 0.2);
}

.btn-batch-enable:hover {
  background: rgba(40, 167, 69, 0.2);
  transform: translateY(-1px);
}

.btn-batch-disable {
  background: rgba(255, 193, 7, 0.1);
  color: #ffc107;
  border: 1px solid rgba(255, 193, 7, 0.2);
}

.btn-batch-disable:hover {
  background: rgba(255, 193, 7, 0.2);
  transform: translateY(-1px);
}

.btn-batch-delete {
  background: rgba(220, 53, 69, 0.1);
  color: #dc3545;
  border: 1px solid rgba(220, 53, 69, 0.2);
}

.btn-batch-delete:hover {
  background: rgba(220, 53, 69, 0.2);
  transform: translateY(-1px);
}

.btn-clear-selection {
  background: rgba(108, 117, 125, 0.1);
  color: #6c757d;
  border: 1px solid rgba(108, 117, 125, 0.2);
}

.btn-clear-selection:hover {
  background: rgba(108, 117, 125, 0.2);
  transform: translateY(-1px);
}

.points-container {
  min-height: 400px;
}

.empty-points {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  text-align: center;
  color: #6c757d;
}

.empty-icon {
  margin-bottom: 16px;
  opacity: 0.5;
}

.empty-points h4 {
  margin: 0 0 8px;
  font-size: 18px;
  color: #2c3e50;
}

.empty-points p {
  margin: 0;
  font-size: 14px;
}

.points-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
}

.point-card {
  background: white;
  border: 1px solid #e9ecef;
  border-radius: 16px;
  padding: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.point-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.point-card.disabled {
  opacity: 0.6;
  background: #f8f9fa;
}

.point-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #4a90e2, #357abd);
}

.point-card.disabled::before {
  background: linear-gradient(90deg, #6c757d, #5a6268);
}

.point-card.selected {
  border-color: #4a90e2;
  box-shadow: 0 4px 12px rgba(74, 144, 226, 0.2);
}

.point-card.selected::before {
  background: linear-gradient(90deg, #4a90e2, #357abd);
}

.point-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.point-selection {
  display: flex;
  align-items: center;
  margin-right: 12px;
}

.point-checkbox {
  width: 18px;
  height: 18px;
  cursor: pointer;
  accent-color: #4a90e2;
}

.point-name {
  flex: 1;
}

.name-input {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  border: none;
  background: transparent;
  padding: 0;
  width: 100%;
}

.name-input:focus {
  outline: none;
  background: rgba(74, 144, 226, 0.05);
  border-radius: 4px;
  padding: 4px 8px;
}

.point-status {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* 这个样式定义重复了，删除 */

.point-content {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 16px;
}

.point-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.point-row label {
  font-size: 12px;
  font-weight: 500;
  color: #6c757d;
  min-width: 40px;
  text-align: right;
}

.point-input, .point-select {
  flex: 1;
  padding: 8px 12px;
  border: 1px solid #e6e9ee;
  border-radius: 6px;
  font-size: 14px;
  background: white;
  transition: all 0.3s ease;
}

.point-input:focus, .point-select:focus {
  outline: none;
  border-color: #4a90e2;
  box-shadow: 0 0 0 3px rgba(74, 144, 226, 0.1);
}

.unit-text {
  font-size: 12px;
  color: #6c757d;
  font-weight: 500;
}

.point-value {
  background: linear-gradient(135deg, #f8f9fa, #ffffff);
  border: 1px solid #e9ecef;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 16px;
  text-align: center;
}

.value-label {
  font-size: 12px;
  font-weight: 500;
  color: #6c757d;
  margin-bottom: 8px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.value-display {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  min-height: 40px;
}

.value-text {
  font-size: 24px;
  font-weight: 700;
  color: #2c3e50;
  font-family: 'Courier New', monospace;
}

.value-unit {
  font-size: 14px;
  color: #6c757d;
  font-weight: 500;
}

.value-display.no-value .value-text {
  color: #6c757d;
  font-size: 16px;
  font-weight: 400;
}

.point-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

.btn-save, .btn-delete {
  width: 36px;
  height: 36px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
}

.btn-save {
  background: rgba(40, 167, 69, 0.1);
  color: #28a745;
}

.btn-save:hover {
  background: rgba(40, 167, 69, 0.2);
  transform: translateY(-1px);
}

.btn-delete {
  background: rgba(220, 53, 69, 0.1);
  color: #dc3545;
}

.btn-delete:hover {
  background: rgba(220, 53, 69, 0.2);
  transform: translateY(-1px);
}

.points-modal .modal-footer {
  padding: 16px 24px;
  border-top: 1px solid #eef1f4;
  background: #f8f9fa;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.footer-info {
  display: flex;
  gap: 16px;
  font-size: 14px;
  color: #6c757d;
}

.points-count, .enabled-count {
  font-weight: 500;
}

.btn-close {
  padding: 10px 20px;
  background: linear-gradient(135deg, #4a90e2, #357abd);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.3s ease;
}

.btn-close:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(74, 144, 226, 0.3);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .points-modal {
    width: 95vw;
    max-height: 95vh;
  }
  
  .points-grid {
    grid-template-columns: 1fr;
  }
  
  .points-toolbar {
    flex-direction: column;
    gap: 16px;
    align-items: stretch;
  }
  
  .template-import {
    flex-direction: column;
    gap: 8px;
  }
  
  .template-select {
    min-width: auto;
  }
  
  .header-content {
    flex-direction: column;
    gap: 16px;
    align-items: flex-start;
  }
  
  .header-actions {
    align-self: stretch;
    justify-content: space-between;
  }
}

.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.4); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.modal { background: #fff; padding: 16px; border-radius: 6px; width: 360px; box-shadow: 0 6px 24px rgba(0,0,0,0.2); }
.modal-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 12px; }
/* 写入弹窗置于更高层，避免被设备详情弹窗覆盖 */
.write-overlay { z-index: 4000 !important; }
.write-overlay .modal { z-index: 4001 !important; }
</style>


