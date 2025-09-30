<template>
  <div class="node-browser-container">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-left">
        <h1>OPC UA 节点浏览器</h1>
        <p>浏览和管理 OPC UA 服务器节点</p>
      </div>
      <div class="header-right">
        <button @click="showConnectionModal = true" class="connect-btn">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 17.93c-3.94-.49-7-3.85-7-7.93 0-.62.08-1.21.21-1.79L9 15v1c0 1.1.9 2 2 2v1.93zm6.9-2.54c-.26-.81-1-1.39-1.9-1.39h-1v-3c0-.55-.45-1-1-1H8v-2h2c.55 0 1-.45 1-1V7h2c1.1 0 2-.9 2-2v-.41c2.93 1.19 5 4.06 5 7.41 0 2.08-.8 3.97-2.1 5.39z" fill="currentColor"/>
          </svg>
          连接服务器
        </button>
        <button @click="refreshNodes" class="refresh-btn" :disabled="!isConnected">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M23 4v6h-6M1 20v-6h6M20.49 9A9 9 0 0 0 5.64 5.64L1 10m22 4l-4.64 4.36A9 9 0 0 1 3.51 15" stroke="currentColor" stroke-width="2"/>
          </svg>
          刷新节点
        </button>
      </div>
    </div>

    <!-- 连接状态 -->
    <div class="connection-status">
      <div class="status-card" :class="{ connected: isConnected }">
        <div class="status-indicator">
          <div class="status-dot" :class="{ connected: isConnected }"></div>
          <span>{{ isConnected ? '已连接' : '未连接' }}</span>
        </div>
        <div v-if="isConnected" class="connection-info">
          <span class="server-name">{{ currentServer?.name || '未知服务器' }}</span>
          <span class="server-endpoint">{{ currentServer?.endpoint || '' }}</span>
        </div>
      </div>
    </div>

    <!-- 主要内容区域 -->
    <div class="main-content">
      <!-- 左侧：节点树 -->
      <div class="nodes-panel">
        <div class="panel-header">
          <h3>节点树</h3>
          <div class="tree-controls">
            <button @click="expandAll" class="control-btn" :disabled="!isConnected">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M8 6l4 4 4-4M8 18l4-4 4 4" stroke="currentColor" stroke-width="2"/>
              </svg>
              展开全部
            </button>
            <button @click="collapseAll" class="control-btn" :disabled="!isConnected">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M8 6l4 4 4-4" stroke="currentColor" stroke-width="2"/>
              </svg>
              折叠全部
            </button>
          </div>
        </div>
        
        <div class="tree-container">
          <div v-if="!isConnected" class="no-connection">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 17.93c-3.94-.49-7-3.85-7-7.93 0-.62.08-1.21.21-1.79L9 15v1c0 1.1.9 2 2 2v1.93zm6.9-2.54c-.26-.81-1-1.39-1.9-1.39h-1v-3c0-.55-.45-1-1-1H8v-2h2c.55 0 1-.45 1-1V7h2c1.1 0 2-.9 2-2v-.41c2.93 1.19 5 4.06 5 7.41 0 2.08-.8 3.97-2.1 5.39z" fill="currentColor"/>
            </svg>
            <p>请先连接到 OPC UA 服务器</p>
            <button @click="showConnectionModal = true" class="connect-btn">连接服务器</button>
          </div>
          
          <div v-else class="node-tree">
            <NodeTreeItem 
              v-for="node in rootNodes" 
              :key="node.id" 
              :node="node"
              :level="0"
              @select="selectNode"
              @expand="toggleNode"
            />
          </div>
        </div>
      </div>

      <!-- 右侧：节点详情 -->
      <div class="details-panel">
        <div class="panel-header">
          <h3>节点详情</h3>
          <div class="detail-controls">
            <button @click="subscribeToNode" class="control-btn" :disabled="!selectedNode">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
              </svg>
              订阅
            </button>
            <button @click="readNodeValue" class="control-btn" :disabled="!selectedNode">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" stroke="currentColor" stroke-width="2"/>
                <polyline points="14,2 14,8 20,8" stroke="currentColor" stroke-width="2"/>
                <line x1="16" y1="13" x2="8" y2="13" stroke="currentColor" stroke-width="2"/>
                <line x1="16" y1="17" x2="8" y2="17" stroke="currentColor" stroke-width="2"/>
                <polyline points="10,9 9,9 8,9" stroke="currentColor" stroke-width="2"/>
              </svg>
              读取
            </button>
          </div>
        </div>
        
        <div class="details-content">
          <div v-if="!selectedNode" class="no-selection">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
            </svg>
            <p>请选择一个节点查看详情</p>
          </div>
          
          <div v-else class="node-details">
            <!-- 基本信息 -->
            <div class="detail-section">
              <h4>基本信息</h4>
              <div class="detail-grid">
                <div class="detail-item">
                  <span class="detail-label">节点ID:</span>
                  <span class="detail-value">{{ selectedNode.nodeId }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">显示名称:</span>
                  <span class="detail-value">{{ selectedNode.displayName }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">节点类:</span>
                  <span class="detail-value">{{ selectedNode.nodeClass }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">数据类型:</span>
                  <span class="detail-value">{{ selectedNode.dataType }}</span>
                </div>
              </div>
            </div>

            <!-- 属性信息 -->
            <div class="detail-section">
              <h4>属性</h4>
              <div class="attributes-table">
                <div class="attribute-row header">
                  <div class="attr-name">属性名</div>
                  <div class="attr-value">值</div>
                  <div class="attr-type">类型</div>
                </div>
                <div 
                  v-for="attr in selectedNode.attributes" 
                  :key="attr.name" 
                  class="attribute-row"
                >
                  <div class="attr-name">{{ attr.name }}</div>
                  <div class="attr-value">{{ attr.value }}</div>
                  <div class="attr-type">{{ attr.type }}</div>
                </div>
              </div>
            </div>

            <!-- 引用信息 -->
            <div class="detail-section">
              <h4>引用</h4>
              <div class="references-list">
                <div 
                  v-for="ref in selectedNode.references" 
                  :key="ref.id" 
                  class="reference-item"
                >
                  <div class="ref-info">
                    <span class="ref-type">{{ ref.referenceType }}</span>
                    <span class="ref-target">{{ ref.targetId }}</span>
                  </div>
                  <div class="ref-direction">
                    <span class="direction" :class="ref.direction">{{ ref.direction }}</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- 实时数据 -->
            <div v-if="selectedNode.isVariable" class="detail-section">
              <h4>实时数据</h4>
              <div class="realtime-data">
                <div class="data-value">
                  <span class="value-label">当前值:</span>
                  <span class="value-content">{{ selectedNode.currentValue || '未读取' }}</span>
                </div>
                <div class="data-timestamp">
                  <span class="timestamp-label">时间戳:</span>
                  <span class="timestamp-content">{{ selectedNode.timestamp || '未知' }}</span>
                </div>
                <div class="data-quality">
                  <span class="quality-label">质量:</span>
                  <span class="quality-content" :class="selectedNode.quality">{{ selectedNode.quality || '未知' }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 连接服务器模态框 -->
    <div v-if="showConnectionModal" class="modal-overlay" @click="closeConnectionModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>连接 OPC UA 服务器</h3>
          <button @click="closeConnectionModal" class="close-btn">×</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="connectToServer" class="connection-form">
            <div class="form-group">
              <label for="serverName">服务器名称</label>
              <input 
                id="serverName"
                v-model="connectionForm.name" 
                type="text" 
                class="form-input"
                placeholder="输入服务器名称"
                required
              />
            </div>
            <div class="form-group">
              <label for="endpoint">端点地址</label>
              <input 
                id="endpoint"
                v-model="connectionForm.endpoint" 
                type="text" 
                class="form-input"
                placeholder="opc.tcp://localhost:4840"
                required
              />
            </div>
            <div class="form-group">
              <label for="securityMode">安全模式</label>
              <select v-model="connectionForm.securityMode" class="form-select">
                <option value="None">None</option>
                <option value="Sign">Sign</option>
                <option value="SignAndEncrypt">SignAndEncrypt</option>
              </select>
            </div>
            <div class="form-group">
              <label for="securityPolicy">安全策略</label>
              <select v-model="connectionForm.securityPolicy" class="form-select">
                <option value="None">None</option>
                <option value="Basic128Rsa15">Basic128Rsa15</option>
                <option value="Basic256">Basic256</option>
                <option value="Basic256Sha256">Basic256Sha256</option>
              </select>
            </div>
            <div class="form-group">
              <label for="username">用户名 (可选)</label>
              <input 
                id="username"
                v-model="connectionForm.username" 
                type="text" 
                class="form-input"
                placeholder="输入用户名"
              />
            </div>
            <div class="form-group">
              <label for="password">密码 (可选)</label>
              <input 
                id="password"
                v-model="connectionForm.password" 
                type="password" 
                class="form-input"
                placeholder="输入密码"
              />
            </div>
          </form>
        </div>
        <div class="modal-footer">
          <button @click="closeConnectionModal" class="modal-btn cancel">取消</button>
          <button @click="connectToServer" class="modal-btn primary" :disabled="isConnecting">
            {{ isConnecting ? '连接中...' : '连接' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'

// 节点类型定义
interface Node {
  id: string
  nodeId: string
  displayName: string
  nodeClass: string
  dataType: string
  hasChildren: boolean
  expanded: boolean
  isVariable: boolean
  currentValue?: string
  timestamp?: string
  quality?: string
  attributes: Array<{ name: string; value: string; type: string }>
  references: Array<{ id: string; referenceType: string; targetId: string; direction: string }>
  children?: Node[]
}

// 节点树组件
const NodeTreeItem = {
  props: ['node', 'level'],
  emits: ['select', 'expand'],
  template: `
    <div class="tree-item" :style="{ paddingLeft: (level * 20 + 8) + 'px' }">
      <div class="node-content" @click="handleClick">
        <button 
          v-if="node.hasChildren" 
          @click.stop="toggleExpand" 
          class="expand-btn"
          :class="{ expanded: node.expanded }"
        >
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M9 18l6-6-6-6" stroke="currentColor" stroke-width="2"/>
          </svg>
        </button>
        <div class="node-icon" :class="node.nodeClass">
          <svg v-if="node.nodeClass === 'Object'" width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <rect x="3" y="3" width="18" height="18" rx="2" stroke="currentColor" stroke-width="2"/>
          </svg>
          <svg v-else-if="node.nodeClass === 'Variable'" width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
          </svg>
          <svg v-else width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
          </svg>
        </div>
        <span class="node-name">{{ node.displayName }}</span>
        <span class="node-id">{{ node.nodeId }}</span>
      </div>
      <div v-if="node.expanded && node.children" class="children">
        <NodeTreeItem 
          v-for="child in node.children" 
          :key="child.id" 
          :node="child"
          :level="level + 1"
          @select="$emit('select', $event)"
          @expand="$emit('expand', $event)"
        />
      </div>
    </div>
  `,
  methods: {
    handleClick(this: any) {
      this.$emit('select', this.node)
    },
    toggleExpand(this: any) {
      this.node.expanded = !this.node.expanded
      this.$emit('expand', this.node)
    }
  }
}

// 响应式数据
const isConnected = ref(false)
const isConnecting = ref(false)
const showConnectionModal = ref(false)
const selectedNode = ref<Node | null>(null)
const currentServer = ref<{ name: string; endpoint: string } | null>(null)

// 连接表单
const connectionForm = ref({
  name: '',
  endpoint: 'opc.tcp://localhost:4840',
  securityMode: 'None',
  securityPolicy: 'None',
  username: '',
  password: ''
})

// 模拟节点数据
const rootNodes = ref<Node[]>([
  {
    id: 'ns=0;i=85',
    nodeId: 'ns=0;i=85',
    displayName: 'Server',
    nodeClass: 'Object',
    dataType: '',
    hasChildren: true,
    expanded: true,
    isVariable: false,
    attributes: [
      { name: 'BrowseName', value: 'Server', type: 'String' },
      { name: 'Description', value: 'The server object', type: 'String' }
    ],
    references: [
      { id: 'ref1', referenceType: 'HasComponent', targetId: 'ns=0;i=2253', direction: 'Forward' },
      { id: 'ref2', referenceType: 'HasComponent', targetId: 'ns=0;i=2254', direction: 'Forward' }
    ],
    children: [
      {
        id: 'ns=0;i=2253',
        nodeId: 'ns=0;i=2253',
        displayName: 'ServerArray',
        nodeClass: 'Variable',
        dataType: 'String',
        hasChildren: false,
        expanded: false,
        isVariable: true,
        currentValue: 'opc.tcp://localhost:4840',
        timestamp: new Date().toISOString(),
        quality: 'Good',
        attributes: [
          { name: 'BrowseName', value: 'ServerArray', type: 'String' },
          { name: 'DataType', value: 'String', type: 'NodeId' },
          { name: 'ValueRank', value: '1', type: 'Int32' }
        ],
        references: []
      },
      {
        id: 'ns=0;i=2254',
        nodeId: 'ns=0;i=2254',
        displayName: 'NamespaceArray',
        nodeClass: 'Variable',
        dataType: 'String',
        hasChildren: false,
        expanded: false,
        isVariable: true,
        currentValue: 'http://opcfoundation.org/UA/',
        timestamp: new Date().toISOString(),
        quality: 'Good',
        attributes: [
          { name: 'BrowseName', value: 'NamespaceArray', type: 'String' },
          { name: 'DataType', value: 'String', type: 'NodeId' },
          { name: 'ValueRank', value: '1', type: 'Int32' }
        ],
        references: []
      },
      {
        id: 'ns=0;i=2255',
        nodeId: 'ns=0;i=2255',
        displayName: 'Objects',
        nodeClass: 'Object',
        dataType: '',
        hasChildren: true,
        expanded: false,
        isVariable: false,
        attributes: [
          { name: 'BrowseName', value: 'Objects', type: 'String' },
          { name: 'Description', value: 'The objects folder', type: 'String' }
        ],
        references: [],
        children: [
          {
            id: 'ns=0;i=85',
            nodeId: 'ns=0;i=85',
            displayName: 'Server',
            nodeClass: 'Object',
            dataType: '',
            hasChildren: false,
            expanded: false,
            isVariable: false,
            attributes: [],
            references: []
          }
        ]
      }
    ]
  }
])

// 方法
function connectToServer() {
  isConnecting.value = true
  
  // 模拟连接过程
  setTimeout(() => {
    currentServer.value = {
      name: connectionForm.value.name,
      endpoint: connectionForm.value.endpoint
    }
    isConnected.value = true
    isConnecting.value = false
    showConnectionModal.value = false
  }, 2000)
}

function closeConnectionModal() {
  showConnectionModal.value = false
}

function selectNode(node: Node) {
  selectedNode.value = node
}

function toggleNode(node: Node) {
  // 处理节点展开/折叠
  console.log('Toggle node:', node)
}

function expandAll() {
  // 展开所有节点
  console.log('Expand all nodes')
}

function collapseAll() {
  // 折叠所有节点
  console.log('Collapse all nodes')
}

function refreshNodes() {
  // 刷新节点树
  console.log('Refresh nodes')
}

function subscribeToNode() {
  if (selectedNode.value) {
    console.log('Subscribe to node:', selectedNode.value)
  }
}

function readNodeValue() {
  if (selectedNode.value) {
    console.log('Read node value:', selectedNode.value)
  }
}

// 生命周期
onMounted(() => {
  // 初始化
})
</script>

<style scoped>
.node-browser-container {
  padding: 24px;
  background: #f8f9fa;
  min-height: 100vh;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
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

.connect-btn,
.refresh-btn {
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

.connect-btn:hover,
.refresh-btn:hover {
  background: #0099cc;
  transform: translateY(-1px);
}

.refresh-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
  transform: none;
}

.connection-status {
  margin-bottom: 24px;
}

.status-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  border-left: 4px solid #6c757d;
}

.status-card.connected {
  border-left-color: #27ae60;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
}

.status-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #6c757d;
}

.status-dot.connected {
  background: #27ae60;
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0% { opacity: 1; }
  50% { opacity: 0.5; }
  100% { opacity: 1; }
}

.connection-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.server-name {
  font-weight: 600;
  color: #2c3e50;
}

.server-endpoint {
  font-size: 12px;
  color: #6c757d;
}

.main-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  height: calc(100vh - 200px);
}

.nodes-panel,
.details-panel {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  background: #f8f9fa;
  border-bottom: 1px solid #e9ecef;
}

.panel-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.tree-controls,
.detail-controls {
  display: flex;
  gap: 8px;
}

.control-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 6px 12px;
  background: #00d4ff;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.control-btn:hover {
  background: #0099cc;
}

.control-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.tree-container,
.details-content {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
}

.no-connection,
.no-selection {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 200px;
  color: #6c757d;
  text-align: center;
}

.no-connection svg,
.no-selection svg {
  margin-bottom: 16px;
  opacity: 0.5;
}

.node-tree {
  display: flex;
  flex-direction: column;
}

.tree-item {
  border-bottom: 1px solid #f1f3f4;
}

.tree-item:last-child {
  border-bottom: none;
}

.node-content {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 0;
  cursor: pointer;
  transition: all 0.3s ease;
}

.node-content:hover {
  background: #f8f9fa;
}

.expand-btn {
  width: 20px;
  height: 20px;
  background: none;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
}

.expand-btn.expanded {
  transform: rotate(90deg);
}

.node-icon {
  width: 16px;
  height: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #6c757d;
}

.node-icon.Object {
  color: #3498db;
}

.node-icon.Variable {
  color: #27ae60;
}

.node-name {
  font-weight: 500;
  color: #2c3e50;
}

.node-id {
  font-size: 12px;
  color: #6c757d;
  margin-left: auto;
}

.children {
  border-left: 1px solid #e9ecef;
  margin-left: 10px;
}

.node-details {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.detail-section {
  background: #f8f9fa;
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
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.detail-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.detail-label {
  font-size: 12px;
  color: #6c757d;
  font-weight: 500;
}

.detail-value {
  font-size: 14px;
  color: #2c3e50;
  font-weight: 500;
}

.attributes-table {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.attribute-row {
  display: grid;
  grid-template-columns: 1fr 1fr 100px;
  gap: 12px;
  padding: 8px 0;
  border-bottom: 1px solid #e9ecef;
}

.attribute-row.header {
  font-weight: 600;
  color: #2c3e50;
  background: #f8f9fa;
  padding: 8px 12px;
  border-radius: 4px;
}

.attr-name {
  font-size: 14px;
  color: #2c3e50;
}

.attr-value {
  font-size: 14px;
  color: #6c757d;
  font-family: monospace;
}

.attr-type {
  font-size: 12px;
  color: #6c757d;
}

.references-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.reference-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: white;
  border-radius: 4px;
  border: 1px solid #e9ecef;
}

.ref-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.ref-type {
  font-size: 12px;
  color: #6c757d;
}

.ref-target {
  font-size: 14px;
  color: #2c3e50;
  font-family: monospace;
}

.ref-direction {
  display: flex;
  align-items: center;
}

.direction {
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.direction.Forward {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.direction.Inverse {
  background: rgba(52, 152, 219, 0.1);
  color: #3498db;
}

.realtime-data {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.data-value,
.data-timestamp,
.data-quality {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: white;
  border-radius: 4px;
  border: 1px solid #e9ecef;
}

.value-label,
.timestamp-label,
.quality-label {
  font-size: 14px;
  color: #6c757d;
}

.value-content,
.timestamp-content,
.quality-content {
  font-size: 14px;
  color: #2c3e50;
  font-weight: 500;
}

.quality-content.Good {
  color: #27ae60;
}

.quality-content.Bad {
  color: #e74c3c;
}

.quality-content.Uncertain {
  color: #f39c12;
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

.connection-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
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
.form-select {
  padding: 10px 12px;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  font-size: 14px;
  transition: all 0.3s ease;
}

.form-input:focus,
.form-select:focus {
  outline: none;
  border-color: #00d4ff;
  box-shadow: 0 0 0 2px rgba(0, 212, 255, 0.1);
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

.modal-btn.primary:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .main-content {
    grid-template-columns: 1fr;
    height: auto;
  }
  
  .page-header {
    flex-direction: column;
    gap: 20px;
  }
  
  .header-right {
    flex-direction: column;
    width: 100%;
  }
  
  .detail-grid {
    grid-template-columns: 1fr;
  }
  
  .attribute-row {
    grid-template-columns: 1fr;
    gap: 8px;
  }
}
</style>
