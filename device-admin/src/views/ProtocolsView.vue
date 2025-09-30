<script setup lang="ts">
import { ref, onMounted } from 'vue'

const protocols = ref([
  {
    id: 1,
    name: 'Modbus TCP',
    type: 'Modbus',
    status: 'running',
    connections: 3,
    dataPoints: 156,
    lastActivity: '2分钟前',
    description: '工业自动化通信协议'
  },
  {
    id: 2,
    name: 'OPC UA',
    type: 'OPC',
    status: 'running',
    connections: 2,
    dataPoints: 89,
    lastActivity: '1分钟前',
    description: '开放平台通信统一架构'
  },
  {
    id: 3,
    name: 'EtherNet/IP',
    type: 'CIP',
    status: 'stopped',
    connections: 0,
    dataPoints: 0,
    lastActivity: '1小时前',
    description: '工业以太网协议'
  },
  {
    id: 4,
    name: 'Profinet',
    type: 'PN',
    status: 'stopped',
    connections: 0,
    dataPoints: 0,
    lastActivity: '2小时前',
    description: '过程现场网络协议'
  }
])

const loading = ref(false)

function getStatusColor(status: string) {
  switch (status) {
    case 'running': return '#28a745'
    case 'stopped': return '#dc3545'
    case 'error': return '#ffc107'
    default: return '#6c757d'
  }
}

function getStatusText(status: string) {
  switch (status) {
    case 'running': return '运行中'
    case 'stopped': return '已停止'
    case 'error': return '错误'
    default: return '未知'
  }
}

function toggleProtocol(id: number) {
  const protocol = protocols.value.find(p => p.id === id)
  if (protocol) {
    protocol.status = protocol.status === 'running' ? 'stopped' : 'running'
    if (protocol.status === 'running') {
      protocol.connections = Math.floor(Math.random() * 5) + 1
      protocol.dataPoints = Math.floor(Math.random() * 200) + 50
    } else {
      protocol.connections = 0
      protocol.dataPoints = 0
    }
  }
}

onMounted(() => {
  // 模拟加载
  loading.value = true
  setTimeout(() => {
    loading.value = false
  }, 1000)
})
</script>

<template>
  <div class="protocols-container">
    <!-- 协议统计 -->
    <div class="stats-overview">
      <div class="stat-item">
        <div class="stat-icon running">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ protocols.filter(p => p.status === 'running').length }}</div>
          <div class="stat-label">运行中</div>
        </div>
      </div>
      
      <div class="stat-item">
        <div class="stat-icon stopped">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ protocols.filter(p => p.status === 'stopped').length }}</div>
          <div class="stat-label">已停止</div>
        </div>
      </div>
      
      <div class="stat-item">
        <div class="stat-icon total">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ protocols.length }}</div>
          <div class="stat-label">总协议数</div>
        </div>
      </div>
    </div>

    <!-- 协议管理面板 -->
    <div class="protocols-panel">
      <div class="panel-header">
        <h3>协议管理</h3>
        <div class="panel-actions">
          <button class="btn-refresh" @click="() => {}" :disabled="loading">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
            </svg>
          </button>
          <button class="btn-add" disabled>
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z" fill="currentColor"/>
            </svg>
            添加协议
          </button>
        </div>
      </div>

      <div v-if="loading" class="loading-state">
        <div class="loading-spinner"></div>
        <span>加载协议数据中...</span>
      </div>

      <div v-else class="protocols-grid">
        <div v-for="protocol in protocols" :key="protocol.id" class="protocol-card">
          <div class="protocol-header">
            <div class="protocol-info">
              <div class="protocol-icon">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
                </svg>
              </div>
              <div class="protocol-details">
                <h4>{{ protocol.name }}</h4>
                <p>{{ protocol.description }}</p>
              </div>
            </div>
            <div class="protocol-status">
              <div 
                class="status-indicator"
                :style="{ backgroundColor: getStatusColor(protocol.status) }"
              ></div>
              <span>{{ getStatusText(protocol.status) }}</span>
            </div>
          </div>

          <div class="protocol-metrics">
            <div class="metric">
              <span class="metric-label">连接数</span>
              <span class="metric-value">{{ protocol.connections }}</span>
            </div>
            <div class="metric">
              <span class="metric-label">数据点</span>
              <span class="metric-value">{{ protocol.dataPoints }}</span>
            </div>
            <div class="metric">
              <span class="metric-label">最后活动</span>
              <span class="metric-value">{{ protocol.lastActivity }}</span>
            </div>
          </div>

          <div class="protocol-actions">
            <button 
              class="btn-toggle"
              :class="{ active: protocol.status === 'running' }"
              @click="toggleProtocol(protocol.id)"
            >
              <svg v-if="protocol.status === 'running'" width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z" fill="currentColor"/>
              </svg>
              <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M8 5v14l11-7z" fill="currentColor"/>
              </svg>
              {{ protocol.status === 'running' ? '停止' : '启动' }}
            </button>
            <button class="btn-config" disabled>
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
</template>

<style scoped>
.protocols-container {
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

.stat-item.running::before {
  background: linear-gradient(90deg, #28a745, #20c997);
}

.stat-item.stopped::before {
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

.stat-icon.running {
  background: linear-gradient(135deg, #28a745, #20c997);
}

.stat-icon.stopped {
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

.protocols-panel {
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

.protocols-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: 20px;
}

.protocol-card {
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

.protocol-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #4a90e2, #357abd);
}

.protocol-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.15);
}

.protocol-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 20px;
}

.protocol-info {
  display: flex;
  align-items: center;
  gap: 16px;
}

.protocol-icon {
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

.protocol-details h4 {
  margin: 0 0 4px;
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
}

.protocol-details p {
  margin: 0;
  font-size: 14px;
  color: #6c757d;
}

.protocol-status {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background: rgba(74, 144, 226, 0.1);
  border-radius: 20px;
}

.status-indicator {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  animation: pulse 2s infinite;
}

.protocol-metrics {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-bottom: 20px;
  padding: 16px;
  background: rgba(74, 144, 226, 0.05);
  border-radius: 12px;
}

.metric {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.metric-label {
  font-size: 12px;
  color: #6c757d;
  margin-bottom: 4px;
  font-weight: 500;
}

.metric-value {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
}

.protocol-actions {
  display: flex;
  gap: 12px;
}

.btn-toggle, .btn-config {
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
  flex: 1;
  justify-content: center;
}

.btn-toggle {
  background: linear-gradient(135deg, #dc3545, #c82333);
  color: white;
  box-shadow: 0 4px 12px rgba(220, 53, 69, 0.3);
}

.btn-toggle.active {
  background: linear-gradient(135deg, #28a745, #20c997);
  box-shadow: 0 4px 12px rgba(40, 167, 69, 0.3);
}

.btn-toggle:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(220, 53, 69, 0.4);
}

.btn-toggle.active:hover {
  box-shadow: 0 6px 16px rgba(40, 167, 69, 0.4);
}

.btn-config {
  background: linear-gradient(135deg, #4a90e2, #357abd);
  color: white;
  box-shadow: 0 4px 12px rgba(74, 144, 226, 0.3);
}

.btn-config:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(74, 144, 226, 0.4);
}

.btn-config:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .stats-overview {
    grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  }
  
  .protocols-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .stats-overview {
    grid-template-columns: 1fr;
    gap: 16px;
  }
  
  .protocol-card {
    padding: 20px;
  }
  
  .protocol-metrics {
    grid-template-columns: 1fr;
    gap: 12px;
  }
  
  .protocol-actions {
    flex-direction: column;
  }
}

@media (max-width: 480px) {
  .protocols-panel {
    padding: 16px;
  }
  
  .stat-item {
    padding: 16px;
  }
  
  .protocol-card {
    padding: 16px;
  }
}
</style>


