<template>
  <div class="load-balancing-container">
    <!-- 负载均衡概览 -->
    <div class="stats-overview">
      <div class="stat-card">
        <div class="stat-icon balance">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ loadStats.totalRequests }}</div>
          <div class="stat-label">总请求数</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon performance">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ loadStats.avgResponseTime }}ms</div>
          <div class="stat-label">平均响应时间</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon servers">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M4 6h16v2H4zm0 5h16v2H4zm0 5h16v2H4z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ loadStats.activeServers }}</div>
          <div class="stat-label">活跃服务器</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon health">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ loadStats.healthScore }}%</div>
          <div class="stat-label">健康评分</div>
        </div>
      </div>
    </div>

    <!-- 负载均衡配置 -->
    <div class="management-panel">
      <div class="panel-header">
        <h3>负载均衡配置</h3>
        <div class="panel-actions">
          <button class="btn-secondary" @click="refreshLoadBalancer">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
            </svg>
            刷新
          </button>
          <button class="btn-primary" @click="configureLoadBalancer">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19.14,12.94c0.04-0.3,0.06-0.61,0.06-0.94c0-0.32-0.02-0.64-0.07-0.94l2.03-1.58c0.18-0.14,0.23-0.41,0.12-0.61 l-1.92-3.32c-0.12-0.22-0.37-0.29-0.59-0.22l-2.39,0.96c-0.5-0.38-1.03-0.7-1.62-0.94L14.4,2.81c-0.04-0.24-0.24-0.41-0.48-0.41 h-3.84c-0.24,0-0.43,0.17-0.47,0.41L9.25,5.35C8.66,5.59,8.12,5.92,7.63,6.29L5.24,5.33c-0.22-0.08-0.47,0-0.59,0.22L2.74,8.87 C2.62,9.08,2.66,9.34,2.86,9.48l2.03,1.58C4.84,11.36,4.8,11.69,4.8,12s0.02,0.64,0.07,0.94l-2.03,1.58 c-0.18,0.14-0.23,0.41-0.12,0.61l1.92,3.32c0.12,0.22,0.37,0.29,0.59,0.22l2.39-0.96c0.5,0.38,1.03,0.7,1.62,0.94l0.36,2.54 c0.05,0.24,0.24,0.41,0.48,0.41h3.84c0.24,0,0.44-0.17,0.47-0.41l0.36-2.54c0.59-0.24,1.13-0.56,1.62-0.94l2.39,0.96 c0.22,0.08,0.47,0,0.59-0.22l1.92-3.32c0.12-0.22,0.07-0.47-0.12-0.61L19.14,12.94z M12,15.6c-1.98,0-3.6-1.62-3.6-3.6 s1.62-3.6,3.6-3.6s3.6,1.62,3.6,3.6S13.98,15.6,12,15.6z" fill="currentColor"/>
            </svg>
            配置负载均衡
          </button>
        </div>
      </div>

      <!-- 负载均衡策略配置 -->
      <div class="config-section">
        <div class="config-item">
          <label class="config-label">负载均衡算法</label>
          <select v-model="loadBalancerConfig.algorithm" class="config-select">
            <option value="round-robin">轮询 (Round Robin)</option>
            <option value="least-connections">最少连接 (Least Connections)</option>
            <option value="weighted-round-robin">加权轮询 (Weighted Round Robin)</option>
            <option value="ip-hash">IP哈希 (IP Hash)</option>
            <option value="least-response-time">最少响应时间 (Least Response Time)</option>
          </select>
        </div>
        <div class="config-item">
          <label class="config-label">健康检查间隔</label>
          <input 
            v-model="loadBalancerConfig.healthCheckInterval" 
            type="number" 
            class="config-input"
            placeholder="秒"
          />
        </div>
        <div class="config-item">
          <label class="config-label">超时时间</label>
          <input 
            v-model="loadBalancerConfig.timeout" 
            type="number" 
            class="config-input"
            placeholder="毫秒"
          />
        </div>
        <div class="config-item">
          <label class="config-label">最大重试次数</label>
          <input 
            v-model="loadBalancerConfig.maxRetries" 
            type="number" 
            class="config-input"
            placeholder="次数"
          />
        </div>
      </div>
    </div>

    <!-- 服务器负载分布 -->
    <div class="servers-load-panel">
      <div class="panel-header">
        <h3>服务器负载分布</h3>
        <div class="panel-actions">
          <button class="btn-secondary" @click="rebalanceLoad">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
            </svg>
            重新平衡
          </button>
        </div>
      </div>

      <div class="servers-load-grid">
        <div 
          v-for="server in serversLoad" 
          :key="server.id" 
          class="server-load-card"
          :class="{ 'overloaded': server.loadPercentage > 80, 'underloaded': server.loadPercentage < 30 }"
        >
          <div class="server-header">
            <div class="server-info">
              <h4>{{ server.name }}</h4>
              <p class="server-ip">{{ server.ip }}</p>
            </div>
            <div class="server-status">
              <div class="status-indicator" :class="server.status">
                <div class="status-dot"></div>
                <span>{{ getStatusText(server.status) }}</span>
              </div>
            </div>
          </div>

          <div class="load-metrics">
            <div class="load-bar">
              <div class="load-label">负载百分比</div>
              <div class="load-progress">
                <div 
                  class="load-fill" 
                  :style="{ width: server.loadPercentage + '%' }"
                  :class="{ 'overloaded': server.loadPercentage > 80, 'underloaded': server.loadPercentage < 30 }"
                ></div>
              </div>
              <div class="load-value">{{ server.loadPercentage }}%</div>
            </div>

            <div class="metrics-grid">
              <div class="metric">
                <div class="metric-label">活跃连接</div>
                <div class="metric-value">{{ server.activeConnections }}</div>
              </div>
              <div class="metric">
                <div class="metric-label">请求/秒</div>
                <div class="metric-value">{{ server.requestsPerSecond }}</div>
              </div>
              <div class="metric">
                <div class="metric-label">响应时间</div>
                <div class="metric-value">{{ server.avgResponseTime }}ms</div>
              </div>
              <div class="metric">
                <div class="metric-label">权重</div>
                <div class="metric-value">{{ server.weight }}</div>
              </div>
            </div>
          </div>

          <div class="server-actions">
            <button class="btn-icon" @click="adjustWeight(server)" title="调整权重">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M19.14,12.94c0.04-0.3,0.06-0.61,0.06-0.94c0-0.32-0.02-0.64-0.07-0.94l2.03-1.58c0.18-0.14,0.23-0.41,0.12-0.61 l-1.92-3.32c-0.12-0.22-0.37-0.29-0.59-0.22l-2.39,0.96c-0.5-0.38-1.03-0.7-1.62-0.94L14.4,2.81c-0.04-0.24-0.24-0.41-0.48-0.41 h-3.84c-0.24,0-0.43,0.17-0.47,0.41L9.25,5.35C8.66,5.59,8.12,5.92,7.63,6.29L5.24,5.33c-0.22-0.08-0.47,0-0.59,0.22L2.74,8.87 C2.62,9.08,2.66,9.34,2.86,9.48l2.03,1.58C4.84,11.36,4.8,11.69,4.8,12s0.02,0.64,0.07,0.94l-2.03,1.58 c-0.18,0.14-0.23,0.41-0.12,0.61l1.92,3.32c0.12,0.22,0.37,0.29,0.59,0.22l2.39-0.96c0.5,0.38,1.03,0.7,1.62,0.94l0.36,2.54 c0.05,0.24,0.24,0.41,0.48,0.41h3.84c0.24,0,0.44-0.17,0.47-0.41l0.36-2.54c0.59-0.24,1.13-0.56,1.62-0.94l2.39,0.96 c0.22,0.08,0.47,0,0.59-0.22l1.92-3.32c0.12-0.22,0.07-0.47-0.12-0.61L19.14,12.94z M12,15.6c-1.98,0-3.6-1.62-3.6-3.6 s1.62-3.6,3.6-3.6s3.6,1.62,3.6,3.6S13.98,15.6,12,15.6z" fill="currentColor"/>
              </svg>
            </button>
            <button class="btn-icon" @click="enableServer(server)" :disabled="server.status === 'online'" title="启用服务器">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
              </svg>
            </button>
            <button class="btn-icon" @click="disableServer(server)" :disabled="server.status === 'offline'" title="禁用服务器">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

// 负载均衡配置
const loadBalancerConfig = ref({
  algorithm: 'round-robin',
  healthCheckInterval: 30,
  timeout: 5000,
  maxRetries: 3
})

// 模拟服务器负载数据
const serversLoad = ref([
  {
    id: 1,
    name: 'BYWG-Server-01',
    ip: '192.168.1.100',
    status: 'online',
    loadPercentage: 65,
    activeConnections: 245,
    requestsPerSecond: 156,
    avgResponseTime: 45,
    weight: 1
  },
  {
    id: 2,
    name: 'BYWG-Server-02',
    ip: '192.168.1.101',
    status: 'online',
    loadPercentage: 85,
    activeConnections: 320,
    requestsPerSecond: 198,
    avgResponseTime: 62,
    weight: 1
  },
  {
    id: 3,
    name: 'BYWG-Server-03',
    ip: '192.168.1.102',
    status: 'online',
    loadPercentage: 25,
    activeConnections: 89,
    requestsPerSecond: 45,
    avgResponseTime: 28,
    weight: 2
  },
  {
    id: 4,
    name: 'BYWG-Server-04',
    ip: '192.168.1.103',
    status: 'offline',
    loadPercentage: 0,
    activeConnections: 0,
    requestsPerSecond: 0,
    avgResponseTime: 0,
    weight: 1
  }
])

// 计算属性
const loadStats = computed(() => {
  const totalRequests = serversLoad.value.reduce((sum, s) => sum + s.requestsPerSecond, 0)
  const avgResponseTime = Math.round(
    serversLoad.value
      .filter(s => s.status === 'online')
      .reduce((sum, s) => sum + s.avgResponseTime, 0) / 
    serversLoad.value.filter(s => s.status === 'online').length
  )
  const activeServers = serversLoad.value.filter(s => s.status === 'online').length
  const healthScore = Math.round(
    serversLoad.value
      .filter(s => s.status === 'online')
      .reduce((sum, s) => sum + (100 - s.loadPercentage), 0) / 
    serversLoad.value.filter(s => s.status === 'online').length
  )

  return { totalRequests, avgResponseTime, activeServers, healthScore }
})

// 方法
function getStatusText(status: string) {
  const statusMap: Record<string, string> = {
    online: '在线',
    offline: '离线',
    maintenance: '维护中'
  }
  return statusMap[status] || '未知'
}

function refreshLoadBalancer() {
  console.log('刷新负载均衡器...')
}

function configureLoadBalancer() {
  console.log('配置负载均衡器...')
}

function rebalanceLoad() {
  console.log('重新平衡负载...')
}

function adjustWeight(server: any) {
  console.log('调整服务器权重:', server.name)
}

function enableServer(server: any) {
  console.log('启用服务器:', server.name)
}

function disableServer(server: any) {
  console.log('禁用服务器:', server.name)
}
</script>

<style scoped>
.load-balancing-container {
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
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.stat-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: 12px;
  color: white;
}

.stat-icon.balance {
  background: linear-gradient(135deg, #4a90e2, #357abd);
}

.stat-icon.performance {
  background: linear-gradient(135deg, #27ae60, #2ecc71);
}

.stat-icon.servers {
  background: linear-gradient(135deg, #9b59b6, #8e44ad);
}

.stat-icon.health {
  background: linear-gradient(135deg, #f39c12, #e67e22);
}

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: #6c757d;
  font-weight: 500;
}

.management-panel, .servers-load-panel {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}

.panel-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
}

.panel-actions {
  display: flex;
  gap: 12px;
}

.btn-primary, .btn-secondary {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border: none;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 14px;
}

.btn-primary {
  background: linear-gradient(135deg, #4a90e2, #357abd);
  color: white;
  box-shadow: 0 4px 12px rgba(74, 144, 226, 0.3);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(74, 144, 226, 0.4);
}

.btn-secondary {
  background: #f8f9fa;
  color: #6c757d;
  border: 1px solid #dee2e6;
}

.btn-secondary:hover {
  background: #e9ecef;
  color: #495057;
}

.config-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 20px;
  padding: 24px;
}

.config-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.config-label {
  font-size: 14px;
  font-weight: 500;
  color: #2c3e50;
}

.config-select, .config-input {
  padding: 10px 12px;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  font-size: 14px;
  background: white;
  transition: all 0.3s ease;
}

.config-select:focus, .config-input:focus {
  outline: none;
  border-color: #4a90e2;
  box-shadow: 0 0 0 3px rgba(74, 144, 226, 0.1);
}

.servers-load-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 20px;
  padding: 24px;
}

.server-load-card {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  padding: 20px;
  transition: all 0.3s ease;
}

.server-load-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
}

.server-load-card.overloaded {
  border-left: 4px solid #e74c3c;
}

.server-load-card.underloaded {
  border-left: 4px solid #f39c12;
}

.server-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.server-info h4 {
  margin: 0 0 4px;
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
}

.server-ip {
  margin: 0;
  font-size: 14px;
  color: #6c757d;
}

.server-status {
  display: flex;
  align-items: center;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
}

.status-indicator.online {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.status-indicator.offline {
  background: rgba(231, 76, 60, 0.1);
  color: #e74c3c;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: currentColor;
}

.load-metrics {
  margin-bottom: 16px;
}

.load-bar {
  margin-bottom: 16px;
}

.load-label {
  font-size: 12px;
  color: #6c757d;
  margin-bottom: 8px;
}

.load-progress {
  height: 8px;
  background: #e9ecef;
  border-radius: 4px;
  overflow: hidden;
  margin-bottom: 8px;
}

.load-fill {
  height: 100%;
  background: linear-gradient(90deg, #4a90e2, #357abd);
  border-radius: 4px;
  transition: width 0.3s ease;
}

.load-fill.overloaded {
  background: linear-gradient(90deg, #e74c3c, #c0392b);
}

.load-fill.underloaded {
  background: linear-gradient(90deg, #f39c12, #e67e22);
}

.load-value {
  font-size: 14px;
  font-weight: 600;
  color: #2c3e50;
  text-align: right;
}

.metrics-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.metric {
  text-align: center;
  padding: 12px;
  background: #f8f9fa;
  border-radius: 8px;
}

.metric-label {
  font-size: 11px;
  color: #6c757d;
  margin-bottom: 4px;
}

.metric-value {
  font-size: 14px;
  font-weight: 600;
  color: #2c3e50;
}

.server-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

.btn-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: 1px solid #dee2e6;
  border-radius: 6px;
  background: white;
  color: #6c757d;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-icon:hover:not(:disabled) {
  background: #f8f9fa;
  color: #495057;
  border-color: #adb5bd;
}

.btn-icon:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

@media (max-width: 768px) {
  .servers-load-grid {
    grid-template-columns: 1fr;
  }
  
  .config-section {
    grid-template-columns: 1fr;
  }
  
  .metrics-grid {
    grid-template-columns: 1fr;
  }
}
</style>
