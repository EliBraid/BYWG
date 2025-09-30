<template>
  <div class="protocol-templates-container">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-left">
        <h1>协议模板</h1>
        <p>工业协议模板管理与配置</p>
      </div>
      <div class="header-right">
        <button @click="showCreateTemplateModal = true" class="create-btn">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 5v14M5 12h14" stroke="currentColor" stroke-width="2"/>
          </svg>
          新建模板
        </button>
        <button @click="importTemplate" class="import-btn">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" stroke="currentColor" stroke-width="2"/>
            <polyline points="7,10 12,15 17,10" stroke="currentColor" stroke-width="2"/>
            <line x1="12" y1="15" x2="12" y2="3" stroke="currentColor" stroke-width="2"/>
          </svg>
          导入模板
        </button>
        <button @click="exportTemplates" class="export-btn">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" stroke="currentColor" stroke-width="2"/>
            <polyline points="7,10 12,15 17,10" stroke="currentColor" stroke-width="2"/>
            <line x1="12" y1="15" x2="12" y2="3" stroke="currentColor" stroke-width="2"/>
          </svg>
          导出模板
        </button>
      </div>
    </div>

    <!-- 统计概览 -->
    <div class="stats-section">
      <div class="stats-cards">
        <div class="stats-card">
          <div class="stats-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
            </svg>
          </div>
          <div class="stats-content">
            <div class="stats-value">{{ totalTemplates }}</div>
            <div class="stats-label">总模板数</div>
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
            <div class="stats-value">{{ activeTemplates }}</div>
            <div class="stats-label">活跃模板</div>
          </div>
        </div>
        <div class="stats-card">
          <div class="stats-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M9 19c-5 0-9-4-9-9s4-9 9-9 9 4 9 9-4 9-9 9zM21 3l-3 3" stroke="currentColor" stroke-width="2"/>
            </svg>
          </div>
          <div class="stats-content">
            <div class="stats-value">{{ supportedProtocols }}</div>
            <div class="stats-label">支持协议</div>
          </div>
        </div>
        <div class="stats-card">
          <div class="stats-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
            </svg>
          </div>
          <div class="stats-content">
            <div class="stats-value">{{ totalUsage }}</div>
            <div class="stats-label">使用次数</div>
          </div>
        </div>
      </div>
    </div>

    <!-- 过滤和搜索 -->
    <div class="filters-section">
      <div class="filter-group">
        <label>协议类型</label>
        <select v-model="selectedProtocol" class="filter-select">
          <option value="">所有协议</option>
          <option v-for="protocol in protocols" :key="protocol" :value="protocol">
            {{ protocol }}
          </option>
        </select>
      </div>
      <div class="filter-group">
        <label>模板分类</label>
        <select v-model="selectedCategory" class="filter-select">
          <option value="">所有分类</option>
          <option v-for="category in categories" :key="category" :value="category">
            {{ category }}
          </option>
        </select>
      </div>
      <div class="filter-group">
        <label>状态</label>
        <select v-model="selectedStatus" class="filter-select">
          <option value="">所有状态</option>
          <option value="active">活跃</option>
          <option value="inactive">非活跃</option>
          <option value="draft">草稿</option>
        </select>
      </div>
      <div class="search-group">
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="搜索模板名称或描述..." 
          class="search-input"
        />
        <button @click="clearFilters" class="clear-filters-btn">清除筛选</button>
      </div>
    </div>

    <!-- 模板列表 -->
    <div class="templates-section">
      <div class="section-header">
        <h2>协议模板列表</h2>
        <div class="view-controls">
          <div class="view-toggle">
            <button @click="viewMode = 'grid'" :class="{ active: viewMode === 'grid' }">网格视图</button>
            <button @click="viewMode = 'list'" :class="{ active: viewMode === 'list' }">列表视图</button>
          </div>
        </div>
      </div>

      <!-- 网格视图 -->
      <div v-if="viewMode === 'grid'" class="templates-grid">
        <div 
          v-for="template in filteredTemplates" 
          :key="template.id" 
          class="template-card"
          :class="template.status"
        >
          <div class="template-header">
            <div class="template-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
              </svg>
            </div>
            <div class="template-actions">
              <button @click="editTemplate(template)" class="action-btn small">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" stroke="currentColor" stroke-width="2"/>
                  <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" stroke="currentColor" stroke-width="2"/>
                </svg>
              </button>
              <button @click="duplicateTemplate(template)" class="action-btn small">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <rect x="9" y="9" width="13" height="13" rx="2" ry="2" stroke="currentColor" stroke-width="2"/>
                  <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" stroke="currentColor" stroke-width="2"/>
                </svg>
              </button>
              <button @click="deleteTemplate(template)" class="action-btn small danger">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" stroke="currentColor" stroke-width="2"/>
                </svg>
              </button>
            </div>
          </div>
          
          <div class="template-content">
            <h3 class="template-name">{{ template.name }}</h3>
            <p class="template-description">{{ template.description }}</p>
            
            <div class="template-meta">
              <div class="meta-item">
                <span class="meta-label">协议类型:</span>
                <span class="meta-value">{{ template.protocol }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">版本:</span>
                <span class="meta-value">{{ template.version }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">使用次数:</span>
                <span class="meta-value">{{ template.usageCount }}</span>
              </div>
            </div>
          </div>
          
          <div class="template-footer">
            <div class="template-tags">
              <span class="template-tag" :class="template.protocol">{{ template.protocol }}</span>
              <span class="template-tag" :class="template.status">{{ getStatusText(template.status) }}</span>
            </div>
            <div class="template-updated">
              更新于 {{ formatDate(template.updatedAt) }}
            </div>
          </div>
        </div>
      </div>

      <!-- 列表视图 -->
      <div v-else class="templates-list">
        <div class="list-header">
          <div class="list-cell">模板名称</div>
          <div class="list-cell">协议类型</div>
          <div class="list-cell">版本</div>
          <div class="list-cell">状态</div>
          <div class="list-cell">使用次数</div>
          <div class="list-cell">更新时间</div>
          <div class="list-cell">操作</div>
        </div>
        <div 
          v-for="template in filteredTemplates" 
          :key="template.id" 
          class="list-row"
          :class="template.status"
        >
          <div class="list-cell">
            <div class="template-name-cell">
              <div class="template-icon-small">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
                </svg>
              </div>
              <div class="template-info">
                <div class="template-name">{{ template.name }}</div>
                <div class="template-description">{{ template.description }}</div>
              </div>
            </div>
          </div>
          <div class="list-cell">
            <span class="template-tag" :class="template.protocol">{{ template.protocol }}</span>
          </div>
          <div class="list-cell">{{ template.version }}</div>
          <div class="list-cell">
            <span class="status-indicator" :class="template.status">
              <div class="status-dot" :class="template.status"></div>
              {{ getStatusText(template.status) }}
            </span>
          </div>
          <div class="list-cell">{{ template.usageCount }}</div>
          <div class="list-cell">{{ formatDate(template.updatedAt) }}</div>
          <div class="list-cell">
            <button @click="editTemplate(template)" class="action-btn small">编辑</button>
            <button @click="duplicateTemplate(template)" class="action-btn small">复制</button>
            <button @click="deleteTemplate(template)" class="action-btn small danger">删除</button>
          </div>
        </div>
      </div>
    </div>

    <!-- 创建/编辑模板模态框 -->
    <div v-if="showCreateTemplateModal || showEditTemplateModal" class="modal-overlay" @click="closeModal">
      <div class="modal-content large" @click.stop>
        <div class="modal-header">
          <h3>{{ showCreateTemplateModal ? '新建协议模板' : '编辑协议模板' }}</h3>
          <button @click="closeModal" class="close-btn">×</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="saveTemplate" class="template-form">
            <div class="form-row">
              <div class="form-group">
                <label for="templateName">模板名称 *</label>
                <input 
                  id="templateName"
                  v-model="templateForm.name" 
                  type="text" 
                  class="form-input"
                  placeholder="请输入模板名称"
                  required
                />
              </div>
              <div class="form-group">
                <label for="templateProtocol">协议类型 *</label>
                <select v-model="templateForm.protocol" class="form-select" required>
                  <option value="">选择协议类型</option>
                  <option v-for="protocol in protocols" :key="protocol" :value="protocol">
                    {{ protocol }}
                  </option>
                </select>
              </div>
            </div>
            
            <div class="form-group">
              <label for="templateDescription">模板描述</label>
              <textarea 
                id="templateDescription"
                v-model="templateForm.description" 
                class="form-textarea"
                placeholder="请输入模板描述"
                rows="3"
              ></textarea>
            </div>
            
            <div class="form-row">
              <div class="form-group">
                <label for="templateVersion">版本号</label>
                <input 
                  id="templateVersion"
                  v-model="templateForm.version" 
                  type="text" 
                  class="form-input"
                  placeholder="例如: 1.0.0"
                />
              </div>
              <div class="form-group">
                <label for="templateCategory">分类</label>
                <select v-model="templateForm.category" class="form-select">
                  <option value="">选择分类</option>
                  <option v-for="category in categories" :key="category" :value="category">
                    {{ category }}
                  </option>
                </select>
              </div>
            </div>
            
            <div class="form-group">
              <label for="templateConfig">协议配置</label>
              <div class="config-editor">
                <textarea 
                  id="templateConfig"
                  v-model="templateForm.config" 
                  class="form-textarea config-textarea"
                  placeholder="请输入协议配置JSON"
                  rows="10"
                ></textarea>
                <div class="config-actions">
                  <button type="button" @click="validateConfig" class="validate-btn">验证配置</button>
                  <button type="button" @click="formatConfig" class="format-btn">格式化</button>
                </div>
              </div>
            </div>
          </form>
        </div>
        <div class="modal-footer">
          <button @click="closeModal" class="modal-btn cancel">取消</button>
          <button @click="saveTemplate" class="modal-btn primary">保存</button>
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
const selectedProtocol = ref('')
const selectedCategory = ref('')
const selectedStatus = ref('')
const showCreateTemplateModal = ref(false)
const showEditTemplateModal = ref(false)
const editingTemplate = ref(null)

// 模板表单
const templateForm = ref({
  name: '',
  description: '',
  protocol: '',
  version: '1.0.0',
  category: '',
  config: ''
})

// 协议类型
const protocols = ref([
  'Modbus TCP', 'OPC UA', 'Ethernet/IP', 'Profinet', 'BACnet', 'DNP3', 'MQTT', 'CoAP'
])

// 分类
const categories = ref([
  'PLC通信', '传感器数据', 'SCADA系统', 'HMI界面', '数据库连接', '自定义协议'
])

// 模板数据
const templates = ref([
  {
    id: 1,
    name: 'Modbus TCP 标准模板',
    description: '标准的Modbus TCP通信模板，支持读写操作',
    protocol: 'Modbus TCP',
    version: '1.0.0',
    category: 'PLC通信',
    status: 'active',
    usageCount: 25,
    updatedAt: new Date(),
    config: JSON.stringify({
      port: 502,
      timeout: 5000,
      retries: 3,
      functions: ['ReadHoldingRegisters', 'WriteSingleRegister']
    }, null, 2)
  },
  {
    id: 2,
    name: 'OPC UA 数据采集模板',
    description: 'OPC UA服务器数据采集模板',
    protocol: 'OPC UA',
    version: '2.1.0',
    category: 'SCADA系统',
    status: 'active',
    usageCount: 18,
    updatedAt: new Date(Date.now() - 3600000),
    config: JSON.stringify({
      endpoint: 'opc.tcp://localhost:4840',
      securityMode: 'None',
      securityPolicy: 'None',
      nodes: ['ns=2;i=1', 'ns=2;i=2']
    }, null, 2)
  },
  {
    id: 3,
    name: 'Ethernet/IP 设备模板',
    description: 'Ethernet/IP设备通信模板',
    protocol: 'Ethernet/IP',
    version: '1.5.0',
    category: 'PLC通信',
    status: 'draft',
    usageCount: 5,
    updatedAt: new Date(Date.now() - 7200000),
    config: JSON.stringify({
      ipAddress: '192.168.1.100',
      port: 44818,
      connectionPath: '1,0',
      tags: ['Tag1', 'Tag2']
    }, null, 2)
  }
])

// 计算属性
const totalTemplates = computed(() => templates.value.length)
const activeTemplates = computed(() => templates.value.filter(t => t.status === 'active').length)
const supportedProtocols = computed(() => new Set(templates.value.map(t => t.protocol)).size)
const totalUsage = computed(() => templates.value.reduce((sum, t) => sum + t.usageCount, 0))

const filteredTemplates = computed(() => {
  let filtered = templates.value

  if (selectedProtocol.value) {
    filtered = filtered.filter(template => template.protocol === selectedProtocol.value)
  }

  if (selectedCategory.value) {
    filtered = filtered.filter(template => template.category === selectedCategory.value)
  }

  if (selectedStatus.value) {
    filtered = filtered.filter(template => template.status === selectedStatus.value)
  }

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(template => 
      template.name.toLowerCase().includes(query) ||
      template.description.toLowerCase().includes(query)
    )
  }

  return filtered
})

// 方法
function editTemplate(template: any) {
  editingTemplate.value = template
  templateForm.value = {
    name: template.name,
    description: template.description,
    protocol: template.protocol,
    version: template.version,
    category: template.category,
    config: template.config
  }
  showEditTemplateModal.value = true
}

function duplicateTemplate(template: any) {
  const newTemplate = {
    ...template,
    id: Date.now(),
    name: template.name + ' (副本)',
    status: 'draft',
    usageCount: 0,
    updatedAt: new Date()
  }
  templates.value.push(newTemplate)
}

function deleteTemplate(template: any) {
  if (confirm(`确定要删除模板 "${template.name}" 吗？`)) {
    const index = templates.value.findIndex(t => t.id === template.id)
    if (index > -1) {
      templates.value.splice(index, 1)
    }
  }
}

function closeModal() {
  showCreateTemplateModal.value = false
  showEditTemplateModal.value = false
  editingTemplate.value = null
  templateForm.value = {
    name: '',
    description: '',
    protocol: '',
    version: '1.0.0',
    category: '',
    config: ''
  }
}

function saveTemplate() {
  if (showCreateTemplateModal.value) {
    // 创建新模板
    const newTemplate = {
      id: Date.now(),
      name: templateForm.value.name,
      description: templateForm.value.description,
      protocol: templateForm.value.protocol,
      version: templateForm.value.version,
      category: templateForm.value.category,
      status: 'draft',
      usageCount: 0,
      updatedAt: new Date(),
      config: templateForm.value.config
    }
    templates.value.push(newTemplate)
  } else if (showEditTemplateModal.value && editingTemplate.value) {
    // 编辑现有模板
    const template = templates.value.find(t => t.id === editingTemplate.value.id)
    if (template) {
      template.name = templateForm.value.name
      template.description = templateForm.value.description
      template.protocol = templateForm.value.protocol
      template.version = templateForm.value.version
      template.category = templateForm.value.category
      template.config = templateForm.value.config
      template.updatedAt = new Date()
    }
  }
  closeModal()
}

function importTemplate() {
  // 实现导入模板逻辑
  console.log('导入模板')
}

function exportTemplates() {
  // 实现导出模板逻辑
  console.log('导出模板')
}

function clearFilters() {
  searchQuery.value = ''
  selectedProtocol.value = ''
  selectedCategory.value = ''
  selectedStatus.value = ''
}

function validateConfig() {
  try {
    JSON.parse(templateForm.value.config)
    alert('配置格式正确')
  } catch (error) {
    alert('配置格式错误: ' + error.message)
  }
}

function formatConfig() {
  try {
    const parsed = JSON.parse(templateForm.value.config)
    templateForm.value.config = JSON.stringify(parsed, null, 2)
  } catch (error) {
    alert('无法格式化配置: ' + error.message)
  }
}

function getStatusText(status: string) {
  const statusMap: Record<string, string> = {
    active: '活跃',
    inactive: '非活跃',
    draft: '草稿'
  }
  return statusMap[status] || status
}

function formatDate(date: Date) {
  return date.toLocaleDateString()
}

// 生命周期
onMounted(() => {
  // 初始化数据
})
</script>

<style scoped>
.protocol-templates-container {
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

.import-btn,
.export-btn {
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

.import-btn:hover,
.export-btn:hover {
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

.templates-section {
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

.templates-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
  padding: 20px;
}

.template-card {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  padding: 20px;
  transition: all 0.3s ease;
}

.template-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.template-card.draft {
  border-left: 4px solid #f39c12;
}

.template-card.active {
  border-left: 4px solid #27ae60;
}

.template-card.inactive {
  border-left: 4px solid #6c757d;
}

.template-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.template-icon {
  width: 40px;
  height: 40px;
  background: #f8f9fa;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #6c757d;
}

.template-actions {
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

.template-content {
  margin-bottom: 16px;
}

.template-name {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 8px;
}

.template-description {
  font-size: 14px;
  color: #6c757d;
  margin: 0 0 16px;
  line-height: 1.4;
}

.template-meta {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.meta-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 4px 0;
}

.meta-label {
  font-size: 12px;
  color: #6c757d;
}

.meta-value {
  font-size: 14px;
  font-weight: 600;
  color: #2c3e50;
}

.template-footer {
  border-top: 1px solid #f1f3f4;
  padding-top: 12px;
}

.template-tags {
  display: flex;
  gap: 8px;
  margin-bottom: 8px;
}

.template-tag {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.template-tag.Modbus {
  background: rgba(0, 212, 255, 0.1);
  color: #00d4ff;
}

.template-tag.OPC {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.template-tag.Ethernet {
  background: rgba(52, 152, 219, 0.1);
  color: #3498db;
}

.template-tag.active {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.template-tag.draft {
  background: rgba(243, 156, 18, 0.1);
  color: #f39c12;
}

.template-tag.inactive {
  background: rgba(108, 117, 125, 0.1);
  color: #6c757d;
}

.template-updated {
  font-size: 12px;
  color: #999;
}

.templates-list {
  background: white;
}

.list-header {
  display: grid;
  grid-template-columns: 250px 120px 80px 100px 100px 120px 150px;
  gap: 16px;
  padding: 16px 20px;
  background: #f8f9fa;
  font-weight: 600;
  color: #2c3e50;
  font-size: 14px;
}

.list-row {
  display: grid;
  grid-template-columns: 250px 120px 80px 100px 100px 120px 150px;
  gap: 16px;
  padding: 16px 20px;
  border-bottom: 1px solid #f1f3f4;
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

.template-name-cell {
  display: flex;
  align-items: center;
  gap: 12px;
}

.template-icon-small {
  width: 16px;
  height: 16px;
  color: #6c757d;
}

.template-info {
  display: flex;
  flex-direction: column;
}

.template-name {
  font-weight: 500;
  margin: 0 0 4px;
}

.template-description {
  font-size: 12px;
  color: #6c757d;
  margin: 0;
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

.status-dot.active {
  background: #27ae60;
}

.status-dot.draft {
  background: #f39c12;
}

.status-dot.inactive {
  background: #6c757d;
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

.template-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
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

.config-editor {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.config-textarea {
  font-family: 'Courier New', monospace;
  font-size: 12px;
  min-height: 200px;
}

.config-actions {
  display: flex;
  gap: 12px;
}

.validate-btn,
.format-btn {
  padding: 6px 12px;
  background: #6c757d;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.validate-btn:hover,
.format-btn:hover {
  background: #5a6268;
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
  
  .stats-cards {
    grid-template-columns: 1fr;
  }
  
  .filters-section {
    flex-direction: column;
  }
  
  .templates-grid {
    grid-template-columns: 1fr;
  }
  
  .list-header,
  .list-row {
    grid-template-columns: 1fr;
    gap: 8px;
  }
  
  .form-row {
    grid-template-columns: 1fr;
  }
}
</style>
