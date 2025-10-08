<template>
  <div class="settings-container">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-left">
        <h1>系统设置</h1>
        <p>系统配置和参数管理</p>
      </div>
      <div class="header-right">
        <button @click="saveAllSettings" class="save-btn" :disabled="!hasChanges">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z" stroke="currentColor" stroke-width="2"/>
            <polyline points="17,21 17,13 7,13 7,21" stroke="currentColor" stroke-width="2"/>
            <polyline points="7,3 7,8 15,8" stroke="currentColor" stroke-width="2"/>
          </svg>
          保存设置
        </button>
        <button @click="resetSettings" class="reset-btn">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M23 4v6h-6M1 20v-6h6M20.49 9A9 9 0 0 0 5.64 5.64L1 10m22 4l-4.64 4.36A9 9 0 0 1 3.51 15" stroke="currentColor" stroke-width="2"/>
          </svg>
          重置设置
        </button>
      </div>
    </div>

    <!-- 设置导航 -->
    <div class="settings-nav">
      <div class="nav-tabs">
        <button 
          v-for="tab in settingsTabs" 
          :key="tab.id"
          @click="activeTab = tab.id"
          class="nav-tab"
          :class="{ active: activeTab === tab.id }"
        >
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path :d="tab.icon" stroke="currentColor" stroke-width="2"/>
          </svg>
          {{ tab.name }}
        </button>
      </div>
    </div>

    <!-- 设置内容 -->
    <div class="settings-content">
      <!-- 基本设置 -->
      <div v-if="activeTab === 'basic'" class="settings-panel">
        <div class="panel-header">
          <h2>基本设置</h2>
          <p>系统基本配置和参数</p>
        </div>
        
        <div class="settings-section">
          <h3>系统信息</h3>
          <div class="settings-grid">
            <div class="setting-item">
              <label for="systemName">系统名称</label>
              <input 
                id="systemName"
                v-model="settings.basic.systemName" 
                type="text" 
                class="form-input"
                placeholder="输入系统名称"
              />
            </div>
            <div class="setting-item">
              <label for="systemVersion">系统版本</label>
              <input 
                id="systemVersion"
                v-model="settings.basic.systemVersion" 
                type="text" 
                class="form-input"
                placeholder="输入系统版本"
              />
            </div>
            <div class="setting-item">
              <label for="companyName">公司名称</label>
              <input 
                id="companyName"
                v-model="settings.basic.companyName" 
                type="text" 
                class="form-input"
                placeholder="输入公司名称"
              />
            </div>
            <div class="setting-item">
              <label for="contactEmail">联系邮箱</label>
              <input 
                id="contactEmail"
                v-model="settings.basic.contactEmail" 
                type="email" 
                class="form-input"
                placeholder="输入联系邮箱"
              />
            </div>
          </div>
        </div>

        <div class="settings-section">
          <h3>网络配置</h3>
          <div class="settings-grid">
            <div class="setting-item">
              <label for="serverPort">服务器端口</label>
              <input 
                id="serverPort"
                v-model.number="settings.basic.serverPort" 
                type="number" 
                class="form-input"
                placeholder="输入服务器端口"
              />
            </div>
            <div class="setting-item">
              <label for="maxConnections">最大连接数</label>
              <input 
                id="maxConnections"
                v-model.number="settings.basic.maxConnections" 
                type="number" 
                class="form-input"
                placeholder="输入最大连接数"
              />
            </div>
            <div class="setting-item">
              <label for="timeout">连接超时 (秒)</label>
              <input 
                id="timeout"
                v-model.number="settings.basic.timeout" 
                type="number" 
                class="form-input"
                placeholder="输入连接超时时间"
              />
            </div>
            <div class="setting-item">
              <label for="enableSSL">启用SSL</label>
              <div class="checkbox-wrapper">
                <input 
                  id="enableSSL"
                  v-model="settings.basic.enableSSL" 
                  type="checkbox" 
                  class="form-checkbox"
                />
                <label for="enableSSL" class="checkbox-label">启用SSL加密</label>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 安全设置 -->
      <div v-if="activeTab === 'security'" class="settings-panel">
        <div class="panel-header">
          <h2>安全设置</h2>
          <p>系统安全配置和权限管理</p>
        </div>
        
        <div class="settings-section">
          <h3>认证配置</h3>
          <div class="settings-grid">
            <div class="setting-item">
              <label for="authMethod">认证方式</label>
              <select v-model="settings.security.authMethod" class="form-select">
                <option value="local">本地认证</option>
                <option value="ldap">LDAP认证</option>
                <option value="oauth">OAuth认证</option>
              </select>
            </div>
            <div class="setting-item">
              <label for="sessionTimeout">会话超时 (分钟)</label>
              <input 
                id="sessionTimeout"
                v-model.number="settings.security.sessionTimeout" 
                type="number" 
                class="form-input"
                placeholder="输入会话超时时间"
              />
            </div>
            <div class="setting-item">
              <label for="maxLoginAttempts">最大登录尝试次数</label>
              <input 
                id="maxLoginAttempts"
                v-model.number="settings.security.maxLoginAttempts" 
                type="number" 
                class="form-input"
                placeholder="输入最大登录尝试次数"
              />
            </div>
            <div class="setting-item">
              <label for="passwordPolicy">密码策略</label>
              <select v-model="settings.security.passwordPolicy" class="form-select">
                <option value="simple">简单</option>
                <option value="medium">中等</option>
                <option value="complex">复杂</option>
              </select>
            </div>
          </div>
        </div>

        <div class="settings-section">
          <h3>访问控制</h3>
          <div class="settings-grid">
            <div class="setting-item">
              <label for="enableIPWhitelist">启用IP白名单</label>
              <div class="checkbox-wrapper">
                <input 
                  id="enableIPWhitelist"
                  v-model="settings.security.enableIPWhitelist" 
                  type="checkbox" 
                  class="form-checkbox"
                />
                <label for="enableIPWhitelist" class="checkbox-label">启用IP白名单</label>
              </div>
            </div>
            <div class="setting-item full-width">
              <label for="allowedIPs">允许的IP地址</label>
              <textarea 
                id="allowedIPs"
                v-model="settings.security.allowedIPs" 
                class="form-textarea"
                placeholder="每行一个IP地址，例如：&#10;192.168.1.0/24&#10;10.0.0.1"
                rows="4"
              ></textarea>
            </div>
          </div>
        </div>
      </div>

      <!-- 日志设置 -->
      <div v-if="activeTab === 'logging'" class="settings-panel">
        <div class="panel-header">
          <h2>日志设置</h2>
          <p>系统日志配置和管理</p>
        </div>
        
        <div class="settings-section">
          <h3>日志配置</h3>
          <div class="settings-grid">
            <div class="setting-item">
              <label for="logLevel">日志级别</label>
              <select v-model="settings.logging.logLevel" class="form-select">
                <option value="debug">Debug</option>
                <option value="info">Info</option>
                <option value="warn">Warning</option>
                <option value="error">Error</option>
              </select>
            </div>
            <div class="setting-item">
              <label for="logRetention">日志保留天数</label>
              <input 
                id="logRetention"
                v-model.number="settings.logging.logRetention" 
                type="number" 
                class="form-input"
                placeholder="输入日志保留天数"
              />
            </div>
            <div class="setting-item">
              <label for="maxLogSize">最大日志文件大小 (MB)</label>
              <input 
                id="maxLogSize"
                v-model.number="settings.logging.maxLogSize" 
                type="number" 
                class="form-input"
                placeholder="输入最大日志文件大小"
              />
            </div>
            <div class="setting-item">
              <label for="enableRemoteLogging">启用远程日志</label>
              <div class="checkbox-wrapper">
                <input 
                  id="enableRemoteLogging"
                  v-model="settings.logging.enableRemoteLogging" 
                  type="checkbox" 
                  class="form-checkbox"
                />
                <label for="enableRemoteLogging" class="checkbox-label">启用远程日志收集</label>
              </div>
            </div>
          </div>
        </div>

        <div class="settings-section">
          <h3>日志目标</h3>
          <div class="settings-grid">
            <div class="setting-item">
              <label for="logToFile">记录到文件</label>
              <div class="checkbox-wrapper">
                <input 
                  id="logToFile"
                  v-model="settings.logging.logToFile" 
                  type="checkbox" 
                  class="form-checkbox"
                />
                <label for="logToFile" class="checkbox-label">启用文件日志</label>
              </div>
            </div>
            <div class="setting-item">
              <label for="logToConsole">记录到控制台</label>
              <div class="checkbox-wrapper">
                <input 
                  id="logToConsole"
                  v-model="settings.logging.logToConsole" 
                  type="checkbox" 
                  class="form-checkbox"
                />
                <label for="logToConsole" class="checkbox-label">启用控制台日志</label>
              </div>
            </div>
            <div class="setting-item">
              <label for="logToDatabase">记录到数据库</label>
              <div class="checkbox-wrapper">
                <input 
                  id="logToDatabase"
                  v-model="settings.logging.logToDatabase" 
                  type="checkbox" 
                  class="form-checkbox"
                />
                <label for="logToDatabase" class="checkbox-label">启用数据库日志</label>
              </div>
            </div>
            <div class="setting-item">
              <label for="logFilePath">日志文件路径</label>
              <input 
                id="logFilePath"
                v-model="settings.logging.logFilePath" 
                type="text" 
                class="form-input"
                placeholder="输入日志文件路径"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- 备份设置 -->
      <div v-if="activeTab === 'backup'" class="settings-panel">
        <div class="panel-header">
          <h2>备份设置</h2>
          <p>系统备份和恢复配置</p>
        </div>
        
        <div class="settings-section">
          <h3>自动备份</h3>
          <div class="settings-grid">
            <div class="setting-item">
              <label for="enableAutoBackup">启用自动备份</label>
              <div class="checkbox-wrapper">
                <input 
                  id="enableAutoBackup"
                  v-model="settings.backup.enableAutoBackup" 
                  type="checkbox" 
                  class="form-checkbox"
                />
                <label for="enableAutoBackup" class="checkbox-label">启用自动备份</label>
              </div>
            </div>
            <div class="setting-item">
              <label for="backupInterval">备份间隔 (小时)</label>
              <input 
                id="backupInterval"
                v-model.number="settings.backup.backupInterval" 
                type="number" 
                class="form-input"
                placeholder="输入备份间隔"
              />
            </div>
            <div class="setting-item">
              <label for="backupRetention">备份保留数量</label>
              <input 
                id="backupRetention"
                v-model.number="settings.backup.backupRetention" 
                type="number" 
                class="form-input"
                placeholder="输入备份保留数量"
              />
            </div>
            <div class="setting-item">
              <label for="backupPath">备份路径</label>
              <input 
                id="backupPath"
                v-model="settings.backup.backupPath" 
                type="text" 
                class="form-input"
                placeholder="输入备份路径"
              />
            </div>
          </div>
        </div>

        <div class="settings-section">
          <h3>备份操作</h3>
          <div class="backup-actions">
            <button @click="createBackup" class="action-btn primary">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z" stroke="currentColor" stroke-width="2"/>
                <polyline points="17,21 17,13 7,13 7,21" stroke="currentColor" stroke-width="2"/>
                <polyline points="7,3 7,8 15,8" stroke="currentColor" stroke-width="2"/>
              </svg>
              立即备份
            </button>
            <button @click="showRestoreModal = true" class="action-btn secondary">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" stroke="currentColor" stroke-width="2"/>
                <polyline points="7,10 12,15 17,10" stroke="currentColor" stroke-width="2"/>
                <line x1="12" y1="15" x2="12" y2="3" stroke="currentColor" stroke-width="2"/>
              </svg>
              恢复备份
            </button>
            <button @click="showBackupList = true" class="action-btn secondary">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" stroke="currentColor" stroke-width="2"/>
                <polyline points="14,2 14,8 20,8" stroke="currentColor" stroke-width="2"/>
              </svg>
              备份列表
            </button>
          </div>
        </div>
      </div>

      <!-- 性能设置 -->
      <div v-if="activeTab === 'performance'" class="settings-panel">
        <div class="panel-header">
          <h2>性能设置</h2>
          <p>系统性能优化配置</p>
        </div>
        
        <div class="settings-section">
          <h3>缓存配置</h3>
          <div class="settings-grid">
            <div class="setting-item">
              <label for="cacheSize">缓存大小 (MB)</label>
              <input 
                id="cacheSize"
                v-model.number="settings.performance.cacheSize" 
                type="number" 
                class="form-input"
                placeholder="输入缓存大小"
              />
            </div>
            <div class="setting-item">
              <label for="cacheTTL">缓存TTL (秒)</label>
              <input 
                id="cacheTTL"
                v-model.number="settings.performance.cacheTTL" 
                type="number" 
                class="form-input"
                placeholder="输入缓存TTL"
              />
            </div>
            <div class="setting-item">
              <label for="enableCache">启用缓存</label>
              <div class="checkbox-wrapper">
                <input 
                  id="enableCache"
                  v-model="settings.performance.enableCache" 
                  type="checkbox" 
                  class="form-checkbox"
                />
                <label for="enableCache" class="checkbox-label">启用系统缓存</label>
              </div>
            </div>
            <div class="setting-item">
              <label for="cacheStrategy">缓存策略</label>
              <select v-model="settings.performance.cacheStrategy" class="form-select">
                <option value="lru">LRU (最近最少使用)</option>
                <option value="lfu">LFU (最少使用频率)</option>
                <option value="fifo">FIFO (先进先出)</option>
              </select>
            </div>
          </div>
        </div>

        <div class="settings-section">
          <h3>线程配置</h3>
          <div class="settings-grid">
            <div class="setting-item">
              <label for="maxThreads">最大线程数</label>
              <input 
                id="maxThreads"
                v-model.number="settings.performance.maxThreads" 
                type="number" 
                class="form-input"
                placeholder="输入最大线程数"
              />
            </div>
            <div class="setting-item">
              <label for="threadPoolSize">线程池大小</label>
              <input 
                id="threadPoolSize"
                v-model.number="settings.performance.threadPoolSize" 
                type="number" 
                class="form-input"
                placeholder="输入线程池大小"
              />
            </div>
            <div class="setting-item">
              <label for="enableThreadPool">启用线程池</label>
              <div class="checkbox-wrapper">
                <input 
                  id="enableThreadPool"
                  v-model="settings.performance.enableThreadPool" 
                  type="checkbox" 
                  class="form-checkbox"
                />
                <label for="enableThreadPool" class="checkbox-label">启用线程池管理</label>
              </div>
            </div>
            <div class="setting-item">
              <label for="threadTimeout">线程超时 (秒)</label>
              <input 
                id="threadTimeout"
                v-model.number="settings.performance.threadTimeout" 
                type="number" 
                class="form-input"
                placeholder="输入线程超时时间"
              />
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 恢复备份模态框 -->
    <div v-if="showRestoreModal" class="modal-overlay" @click="closeRestoreModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>恢复备份</h3>
          <button @click="closeRestoreModal" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div class="backup-list">
            <div 
              v-for="backup in backupList" 
              :key="backup.id" 
              class="backup-item"
              :class="{ selected: selectedBackup === backup.id }"
              @click="selectedBackup = backup.id"
            >
              <div class="backup-info">
                <div class="backup-name">{{ backup.name }}</div>
                <div class="backup-details">
                  <span class="backup-size">{{ backup.size }}</span>
                  <span class="backup-date">{{ formatDate(backup.date) }}</span>
                </div>
              </div>
              <div class="backup-status">
                <span class="status-badge" :class="backup.status">{{ backup.status }}</span>
              </div>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button @click="closeRestoreModal" class="modal-btn cancel">取消</button>
          <button @click="restoreBackup" class="modal-btn primary" :disabled="!selectedBackup">
            恢复备份
          </button>
        </div>
      </div>
    </div>

    <!-- 备份列表模态框 -->
    <div v-if="showBackupList" class="modal-overlay" @click="closeBackupList">
      <div class="modal-content large" @click.stop>
        <div class="modal-header">
          <h3>备份列表</h3>
          <button @click="closeBackupList" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div class="backup-table">
            <div class="table-header">
              <div class="table-cell">备份名称</div>
              <div class="table-cell">大小</div>
              <div class="table-cell">创建时间</div>
              <div class="table-cell">状态</div>
              <div class="table-cell">操作</div>
            </div>
            <div 
              v-for="backup in backupList" 
              :key="backup.id" 
              class="table-row"
            >
              <div class="table-cell">{{ backup.name }}</div>
              <div class="table-cell">{{ backup.size }}</div>
              <div class="table-cell">{{ formatDate(backup.date) }}</div>
              <div class="table-cell">
                <span class="status-badge" :class="backup.status">{{ backup.status }}</span>
              </div>
              <div class="table-cell">
                <button @click="downloadBackup(backup)" class="action-btn small">下载</button>
                <button @click="deleteBackup(backup)" class="action-btn small danger">删除</button>
              </div>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button @click="closeBackupList" class="modal-btn cancel">关闭</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'

// 响应式数据
const activeTab = ref('basic')
const hasChanges = ref(false)
const showRestoreModal = ref(false)
const showBackupList = ref(false)
const selectedBackup = ref<number | null>(null)

// 设置标签页
const settingsTabs = ref([
  { id: 'basic', name: '基本设置', icon: 'M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z' },
  { id: 'security', name: '安全设置', icon: 'M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z' },
  { id: 'logging', name: '日志设置', icon: 'M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z' },
  { id: 'backup', name: '备份设置', icon: 'M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z' },
  { id: 'performance', name: '性能设置', icon: 'M13 2L3 14h9l-1 8 10-12h-9l1-8z' }
])

// 设置数据
const settings = reactive({
  basic: {
    systemName: 'BYWG Industrial Gateway',
    systemVersion: '1.0.0',
    companyName: 'BYWG Technology',
    contactEmail: 'support@bywg.com',
    serverPort: 8080,
    maxConnections: 1000,
    timeout: 30,
    enableSSL: true
  },
  security: {
    authMethod: 'local',
    sessionTimeout: 30,
    maxLoginAttempts: 5,
    passwordPolicy: 'medium',
    enableIPWhitelist: false,
    allowedIPs: ''
  },
  logging: {
    logLevel: 'info',
    logRetention: 30,
    maxLogSize: 100,
    enableRemoteLogging: false,
    logToFile: true,
    logToConsole: true,
    logToDatabase: false,
    logFilePath: '/var/log/bywg'
  },
  backup: {
    enableAutoBackup: true,
    backupInterval: 24,
    backupRetention: 7,
    backupPath: '/var/backups/bywg'
  },
  performance: {
    cacheSize: 512,
    cacheTTL: 3600,
    enableCache: true,
    cacheStrategy: 'lru',
    maxThreads: 50,
    threadPoolSize: 20,
    enableThreadPool: true,
    threadTimeout: 300
  }
})

// 备份列表
const backupList = ref([
  {
    id: 1,
    name: 'backup_20241201_120000.tar.gz',
    size: '2.5 GB',
    date: new Date('2024-12-01T12:00:00'),
    status: 'completed'
  },
  {
    id: 2,
    name: 'backup_20241130_120000.tar.gz',
    size: '2.3 GB',
    date: new Date('2024-11-30T12:00:00'),
    status: 'completed'
  },
  {
    id: 3,
    name: 'backup_20241129_120000.tar.gz',
    size: '2.1 GB',
    date: new Date('2024-11-29T12:00:00'),
    status: 'completed'
  }
])

// 方法
function saveAllSettings() {
  // 保存所有设置
  console.log('保存设置:', settings)
  hasChanges.value = false
  alert('设置已保存')
}

function resetSettings() {
  if (confirm('确定要重置所有设置吗？这将恢复默认配置。')) {
    // 重置设置
    console.log('重置设置')
    alert('设置已重置')
  }
}

function createBackup() {
  console.log('创建备份')
  alert('备份已开始创建')
}

function restoreBackup() {
  if (selectedBackup.value) {
    const backup = backupList.value.find(b => b.id === selectedBackup.value)
    if (backup && confirm(`确定要恢复备份 "${backup.name}" 吗？`)) {
      console.log('恢复备份:', backup)
      alert('备份恢复已开始')
      closeRestoreModal()
    }
  }
}

function downloadBackup(backup: any) {
  console.log('下载备份:', backup)
  alert(`开始下载备份: ${backup.name}`)
}

function deleteBackup(backup: any) {
  if (confirm(`确定要删除备份 "${backup.name}" 吗？`)) {
    const index = backupList.value.findIndex(b => b.id === backup.id)
    if (index > -1) {
      backupList.value.splice(index, 1)
    }
  }
}

function closeRestoreModal() {
  showRestoreModal.value = false
  selectedBackup.value = null
}

function closeBackupList() {
  showBackupList.value = false
}

function formatDate(date: Date) {
  return date.toLocaleDateString() + ' ' + date.toLocaleTimeString()
}

// 生命周期
onMounted(() => {
  // 初始化设置
  console.log('初始化系统设置')
})
</script>

<style scoped>
.settings-container {
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

.save-btn,
.reset-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 20px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.save-btn {
  background: #00d4ff;
  color: white;
}

.save-btn:hover {
  background: #0099cc;
  transform: translateY(-1px);
}

.save-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
  transform: none;
}

.reset-btn {
  background: #6c757d;
  color: white;
}

.reset-btn:hover {
  background: #5a6268;
}

.settings-nav {
  margin-bottom: 24px;
}

.nav-tabs {
  display: flex;
  gap: 4px;
  background: white;
  border-radius: 8px;
  padding: 4px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.nav-tab {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 20px;
  background: none;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  color: #6c757d;
  cursor: pointer;
  transition: all 0.3s ease;
}

.nav-tab:hover {
  background: #f8f9fa;
  color: #2c3e50;
}

.nav-tab.active {
  background: #00d4ff;
  color: white;
}

.settings-content {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
}

.settings-panel {
  padding: 32px;
}

.panel-header {
  margin-bottom: 32px;
  padding-bottom: 16px;
  border-bottom: 1px solid #e9ecef;
}

.panel-header h2 {
  font-size: 24px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 8px;
}

.panel-header p {
  font-size: 16px;
  color: #6c757d;
  margin: 0;
}

.settings-section {
  margin-bottom: 32px;
}

.settings-section h3 {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 16px;
}

.settings-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
}

.setting-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.setting-item.full-width {
  grid-column: 1 / -1;
}

.setting-item label {
  font-size: 14px;
  font-weight: 500;
  color: #2c3e50;
}

.form-input,
.form-select,
.form-textarea {
  padding: 10px 12px;
  border: 1px solid #e9ecef;
  border-radius: 6px;
  font-size: 14px;
  transition: all 0.3s ease;
}

.form-input:focus,
.form-select:focus,
.form-textarea:focus {
  outline: none;
  border-color: #00d4ff;
  box-shadow: 0 0 0 2px rgba(0, 212, 255, 0.1);
}

.checkbox-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
}

.form-checkbox {
  width: 16px;
  height: 16px;
  accent-color: #00d4ff;
}

.checkbox-label {
  font-size: 14px;
  color: #2c3e50;
  cursor: pointer;
}

.backup-actions {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.action-btn.primary {
  background: #00d4ff;
  color: white;
}

.action-btn.primary:hover {
  background: #0099cc;
}

.action-btn.secondary {
  background: #6c757d;
  color: white;
}

.action-btn.secondary:hover {
  background: #5a6268;
}

.action-btn.small {
  padding: 6px 12px;
  font-size: 12px;
}

.action-btn.danger {
  background: #e74c3c;
}

.action-btn.danger:hover {
  background: #c0392b;
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
  color: #6c757d;
  cursor: pointer;
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.close-btn i {
  font-size: 16px;
}

.modal-body {
  padding: 20px;
}

.backup-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.backup-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  background: #f8f9fa;
  border-radius: 6px;
  border: 1px solid #e9ecef;
  cursor: pointer;
  transition: all 0.3s ease;
}

.backup-item:hover {
  background: #e9ecef;
}

.backup-item.selected {
  background: #00d4ff;
  color: white;
  border-color: #00d4ff;
}

.backup-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.backup-name {
  font-weight: 500;
  font-size: 14px;
}

.backup-details {
  display: flex;
  gap: 12px;
  font-size: 12px;
  opacity: 0.7;
}

.backup-status {
  display: flex;
  align-items: center;
}

.status-badge {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.status-badge.completed {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.backup-table {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.table-header {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr 1fr;
  gap: 16px;
  padding: 12px 16px;
  background: #f8f9fa;
  border-radius: 6px;
  font-weight: 600;
  color: #2c3e50;
  font-size: 14px;
}

.table-row {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr 1fr;
  gap: 16px;
  padding: 12px 16px;
  border-bottom: 1px solid #f1f3f4;
  align-items: center;
}

.table-cell {
  font-size: 14px;
  color: #2c3e50;
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
  .page-header {
    flex-direction: column;
    gap: 20px;
  }
  
  .header-right {
    flex-direction: column;
    width: 100%;
  }
  
  .nav-tabs {
    flex-direction: column;
  }
  
  .settings-grid {
    grid-template-columns: 1fr;
  }
  
  .backup-actions {
    flex-direction: column;
  }
  
  .table-header,
  .table-row {
    grid-template-columns: 1fr;
    gap: 8px;
  }
}
</style>
