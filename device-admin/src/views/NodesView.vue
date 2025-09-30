<script setup lang="ts">
import { ref, onMounted } from 'vue'

const nodes = ref([
  {
    id: 1,
    name: 'Temperature Sensor 1',
    nodeId: 'ns=2;i=1001',
    dataType: 'Double',
    value: 23.5,
    unit: '°C',
    status: 'connected',
    lastUpdate: '30秒前',
    description: '车间温度传感器'
  },
  {
    id: 2,
    name: 'Pressure Sensor 1',
    nodeId: 'ns=2;i=1002',
    dataType: 'Float',
    value: 1.2,
    unit: 'bar',
    status: 'connected',
    lastUpdate: '1分钟前',
    description: '管道压力传感器'
  },
  {
    id: 3,
    name: 'Motor Speed',
    nodeId: 'ns=2;i=1003',
    dataType: 'Int32',
    value: 1500,
    unit: 'rpm',
    status: 'disconnected',
    lastUpdate: '5分钟前',
    description: '电机转速监控'
  },
  {
    id: 4,
    name: 'Valve Position',
    nodeId: 'ns=2;i=1004',
    dataType: 'Boolean',
    value: true,
    unit: '',
    status: 'connected',
    lastUpdate: '2分钟前',
    description: '阀门开关状态'
  }
])

const loading = ref(false)
const selectedNodes = ref<number[]>([])

function getStatusColor(status: string) {
  switch (status) {
    case 'connected': return '#28a745'
    case 'disconnected': return '#dc3545'
    case 'error': return '#ffc107'
    default: return '#6c757d'
  }
}

function getStatusText(status: string) {
  switch (status) {
    case 'connected': return '已连接'
    case 'disconnected': return '已断开'
    case 'error': return '错误'
    default: return '未知'
  }
}

function toggleNodeSelection(id: number) {
  const index = selectedNodes.value.indexOf(id)
  if (index > -1) {
    selectedNodes.value.splice(index, 1)
  } else {
    selectedNodes.value.push(id)
  }
}

function selectAllNodes() {
  if (selectedNodes.value.length === nodes.value.length) {
    selectedNodes.value = []
  } else {
    selectedNodes.value = nodes.value.map(n => n.id)
  }
}

function refreshNodes() {
  loading.value = true
  setTimeout(() => {
    // 模拟数据更新
    nodes.value.forEach(node => {
      if (node.status === 'connected') {
        if (node.dataType === 'Double' || node.dataType === 'Float') {
          node.value = Math.random() * 100
        } else if (node.dataType === 'Int32') {
          node.value = Math.floor(Math.random() * 2000)
        } else if (node.dataType === 'Boolean') {
          node.value = Math.random() > 0.5
        }
        node.lastUpdate = '刚刚'
      }
    })
    loading.value = false
  }, 1000)
}

onMounted(() => {
  refreshNodes()
})
</script>

<template>
  <div class="nodes-container">
    <!-- 节点统计 -->
    <div class="stats-overview">
      <div class="stat-item">
        <div class="stat-icon connected">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ nodes.filter(n => n.status === 'connected').length }}</div>
          <div class="stat-label">已连接</div>
        </div>
      </div>
      
      <div class="stat-item">
        <div class="stat-icon disconnected">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ nodes.filter(n => n.status === 'disconnected').length }}</div>
          <div class="stat-label">已断开</div>
        </div>
      </div>
      
      <div class="stat-item">
        <div class="stat-icon total">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ nodes.length }}</div>
          <div class="stat-label">总节点数</div>
        </div>
      </div>
    </div>

    <!-- 节点管理面板 -->
    <div class="nodes-panel">
      <div class="panel-header">
        <h3>OPC UA 节点管理</h3>
        <div class="panel-actions">
          <button class="btn-refresh" @click="refreshNodes" :disabled="loading">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
            </svg>
          </button>
          <button class="btn-add" disabled>
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z" fill="currentColor"/>
            </svg>
            添加节点
          </button>
        </div>
      </div>

      <div v-if="loading" class="loading-state">
        <div class="loading-spinner"></div>
        <span>刷新节点数据中...</span>
      </div>

      <div v-else class="nodes-content">
        <!-- 批量操作 -->
        <div class="batch-actions">
          <label class="select-all">
            <input 
              type="checkbox" 
              :checked="selectedNodes.length === nodes.length"
              @change="selectAllNodes"
            />
            <span>全选</span>
          </label>
          <div class="batch-buttons">
            <button class="btn-batch" :disabled="selectedNodes.length === 0">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z" fill="currentColor"/>
              </svg>
              读取选中
            </button>
            <button class="btn-batch" :disabled="selectedNodes.length === 0">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z" fill="currentColor"/>
              </svg>
              写入选中
            </button>
            <button class="btn-batch danger" :disabled="selectedNodes.length === 0">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z" fill="currentColor"/>
              </svg>
              删除选中
            </button>
          </div>
        </div>

        <!-- 节点列表 -->
        <div class="nodes-grid">
          <div 
            v-for="node in nodes" 
            :key="node.id" 
            class="node-card"
            :class="{ selected: selectedNodes.includes(node.id) }"
          >
            <div class="node-header">
              <label class="node-checkbox">
                <input 
                  type="checkbox" 
                  :checked="selectedNodes.includes(node.id)"
                  @change="toggleNodeSelection(node.id)"
                />
              </label>
              <div class="node-info">
                <h4>{{ node.name }}</h4>
                <p class="node-id">{{ node.nodeId }}</p>
              </div>
              <div class="node-status">
                <div 
                  class="status-indicator"
                  :style="{ backgroundColor: getStatusColor(node.status) }"
                ></div>
                <span>{{ getStatusText(node.status) }}</span>
              </div>
            </div>

            <div class="node-content">
              <div class="node-description">
                <p>{{ node.description }}</p>
              </div>
              
              <div class="node-value">
                <div class="value-display">
                  <span class="value-label">当前值</span>
                  <span class="value-text">
                    {{ node.value }}{{ node.unit }}
                  </span>
                </div>
                <div class="value-meta">
                  <span class="data-type">{{ node.dataType }}</span>
                  <span class="last-update">{{ node.lastUpdate }}</span>
                </div>
              </div>
            </div>

            <div class="node-actions">
              <button class="btn-action" :disabled="node.status !== 'connected'">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z" fill="currentColor"/>
                </svg>
                读取
              </button>
              <button class="btn-action" :disabled="node.status !== 'connected'">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z" fill="currentColor"/>
                </svg>
                写入
              </button>
              <button class="btn-action" :disabled="node.status !== 'connected'">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M19.14,12.94c0.04-0.3,0.06-0.61,0.06-0.94c0-0.32-0.02-0.64-0.07-0.94l2.03-1.58c0.18-0.14,0.23-0.41,0.12-0.61 l-1.92-3.32c-0.12-0.22-0.37-0.29-0.59-0.22l-2.39,0.96c-0.5-0.38-1.03-0.7-1.62-0.94L14.4,2.81c-0.04-0.24-0.24-0.41-0.48-0.41 h-3.84c-0.24,0-0.43,0.17-0.47,0.41L9.25,5.35C8.66,5.59,8.12,5.92,7.63,6.29L5.24,5.33c-0.22-0.08-0.47,0-0.59,0.22L2.74,8.87 C2.62,9.08,2.66,9.34,2.86,9.48l2.03,1.58C4.84,11.36,4.8,11.69,4.8,12s0.02,0.64,0.07,0.94l-2.03,1.58 c-0.18,0.14-0.23,0.41-0.12,0.61l1.92,3.32c0.12,0.22,0.37,0.29,0.59,0.22l2.39-0.96c0.5,0.38,1.03,0.7,1.62,0.94l0.36,2.54 c0.05,0.24,0.24,0.41,0.48,0.41h3.84c0.24,0,0.44-0.17,0.47-0.41l0.36-2.54c0.59-0.24,1.13-0.56,1.62-0.94l2.39,0.96 c0.22,0.08,0.47,0,0.59-0.22l1.92-3.32c0.12-0.22,0.07-0.47-0.12-0.61L19.14,12.94z M12,15.6c-1.98,0-3.6-1.62-3.6-3.6 s1.62-3.6,3.6-3.6s3.6,1.62,3.6,3.6S13.98,15.6,12,15.6z" fill="currentColor"/>
                </svg>
                配置
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.nodes-container {
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
  margin-bottom: 20px;
}

.stat-item {
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

.stat-item::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
}

.stat-item.connected::before {
  background: linear-gradient(90deg, #28a745, #20c997);
}

.stat-item.disconnected::before {
  background: linear-gradient(90deg, #dc3545, #c82333);
}

.stat-item.total::before {
  background: linear-gradient(90deg, #4a90e2, #357abd);
}

.stat-item:hover {
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

.stat-icon.connected {
  background: linear-gradient(135deg, #28a745, #20c997);
}

.stat-icon.disconnected {
  background: linear-gradient(135deg, #dc3545, #c82333);
}

.stat-icon.total {
  background: linear-gradient(135deg, #4a90e2, #357abd);
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

.nodes-panel {
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

.loading-state {
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

.batch-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding: 16px;
  background: rgba(74, 144, 226, 0.05);
  border-radius: 12px;
  border: 1px solid rgba(74, 144, 226, 0.1);
}

.select-all {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-weight: 500;
  color: #2c3e50;
}

.select-all input[type="checkbox"] {
  width: 16px;
  height: 16px;
  cursor: pointer;
}

.batch-buttons {
  display: flex;
  gap: 8px;
}

.btn-batch {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 12px;
  font-weight: 500;
  transition: all 0.3s ease;
  background: linear-gradient(135deg, #4a90e2, #357abd);
  color: white;
  box-shadow: 0 2px 8px rgba(74, 144, 226, 0.3);
}

.btn-batch:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(74, 144, 226, 0.4);
}

.btn-batch.danger {
  background: linear-gradient(135deg, #dc3545, #c82333);
  box-shadow: 0 2px 8px rgba(220, 53, 69, 0.3);
}

.btn-batch.danger:hover:not(:disabled) {
  box-shadow: 0 4px 12px rgba(220, 53, 69, 0.4);
}

.btn-batch:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.nodes-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 20px;
}

.node-card {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border: 2px solid rgba(0, 0, 0, 0.08);
  border-radius: 16px;
  padding: 20px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  backdrop-filter: blur(10px);
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.node-card.selected {
  border-color: #4a90e2;
  box-shadow: 0 8px 32px rgba(74, 144, 226, 0.2);
}

.node-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #4a90e2, #357abd);
}

.node-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.15);
}

.node-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.node-checkbox {
  display: flex;
  align-items: center;
  cursor: pointer;
}

.node-checkbox input[type="checkbox"] {
  width: 16px;
  height: 16px;
  cursor: pointer;
}

.node-info {
  flex: 1;
}

.node-info h4 {
  margin: 0 0 4px;
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
}

.node-id {
  margin: 0;
  font-size: 12px;
  color: #6c757d;
  font-family: 'Courier New', monospace;
}

.node-status {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 10px;
  background: rgba(74, 144, 226, 0.1);
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.status-indicator {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  animation: pulse 2s infinite;
}

.node-content {
  margin-bottom: 16px;
}

.node-description {
  margin-bottom: 12px;
}

.node-description p {
  margin: 0;
  font-size: 14px;
  color: #6c757d;
  line-height: 1.4;
}

.node-value {
  padding: 12px;
  background: rgba(74, 144, 226, 0.05);
  border-radius: 8px;
  border: 1px solid rgba(74, 144, 226, 0.1);
}

.value-display {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.value-label {
  font-size: 12px;
  color: #6c757d;
  font-weight: 500;
}

.value-text {
  font-size: 18px;
  font-weight: 700;
  color: #2c3e50;
  font-family: 'Courier New', monospace;
}

.value-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 11px;
  color: #6c757d;
}

.data-type {
  background: rgba(74, 144, 226, 0.1);
  padding: 2px 6px;
  border-radius: 4px;
  font-weight: 500;
}

.node-actions {
  display: flex;
  gap: 8px;
}

.btn-action {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 12px;
  font-weight: 500;
  transition: all 0.3s ease;
  background: rgba(74, 144, 226, 0.1);
  color: #4a90e2;
  flex: 1;
  justify-content: center;
}

.btn-action:hover:not(:disabled) {
  background: rgba(74, 144, 226, 0.2);
  transform: translateY(-1px);
}

.btn-action:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .stats-overview {
    grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  }
  
  .nodes-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .stats-overview {
    grid-template-columns: 1fr;
    gap: 16px;
  }
  
  .batch-actions {
    flex-direction: column;
    gap: 12px;
    align-items: stretch;
  }
  
  .batch-buttons {
    justify-content: center;
  }
  
  .node-card {
    padding: 16px;
  }
  
  .node-actions {
    flex-direction: column;
  }
}

@media (max-width: 480px) {
  .nodes-panel {
    padding: 16px;
  }
  
  .stat-item {
    padding: 16px;
  }
  
  .node-card {
    padding: 12px;
  }
}
</style>


