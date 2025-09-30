<template>
  <div class="users-container">
    <!-- 用户统计概览 -->
    <div class="stats-overview">
      <div class="stat-card">
        <div class="stat-icon total">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M16 4c0-1.11.89-2 2-2s2 .89 2 2-.89 2-2 2-2-.89-2-2zm4 18v-6h2.5l-2.54-7.63A1.5 1.5 0 0 0 18.54 8H16c-.8 0-1.54.37-2.01.99L12 11l-1.99-2.01A2.5 2.5 0 0 0 8 8H5.46c-.8 0-1.54.37-2.01.99L1 13.5V16h2v6h2v-6h2.5l2.5-7.5h2l2.5 7.5H14v6h2z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ userStats.total }}</div>
          <div class="stat-label">总用户数</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon active">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ userStats.active }}</div>
          <div class="stat-label">活跃用户</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon admin">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ userStats.admins }}</div>
          <div class="stat-label">管理员</div>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon online">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ userStats.online }}</div>
          <div class="stat-label">在线用户</div>
        </div>
      </div>
    </div>

    <!-- 用户管理面板 -->
    <div class="management-panel">
      <div class="panel-header">
        <h3>用户管理</h3>
        <div class="panel-actions">
          <button class="btn-secondary" @click="refreshUsers">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z" fill="currentColor"/>
            </svg>
            刷新
          </button>
          <button class="btn-primary" @click="addUser">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z" fill="currentColor"/>
            </svg>
            添加用户
          </button>
        </div>
      </div>

      <!-- 搜索和筛选 -->
      <div class="filter-section">
        <div class="search-box">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z" fill="currentColor"/>
          </svg>
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="搜索用户名、邮箱或角色..."
            class="search-input"
          />
        </div>
        <div class="filter-controls">
          <select v-model="roleFilter" class="filter-select">
            <option value="">所有角色</option>
            <option value="admin">管理员</option>
            <option value="operator">操作员</option>
            <option value="viewer">观察员</option>
          </select>
          <select v-model="statusFilter" class="filter-select">
            <option value="">所有状态</option>
            <option value="active">活跃</option>
            <option value="inactive">非活跃</option>
            <option value="online">在线</option>
            <option value="offline">离线</option>
          </select>
          <select v-model="sortBy" class="filter-select">
            <option value="username">按用户名排序</option>
            <option value="role">按角色排序</option>
            <option value="lastLogin">按最后登录排序</option>
            <option value="createdAt">按创建时间排序</option>
          </select>
        </div>
      </div>
    </div>

    <!-- 用户列表 -->
    <div class="users-table-container">
      <table class="users-table">
        <thead>
          <tr>
            <th class="checkbox-column">
              <input 
                type="checkbox" 
                v-model="selectAll"
                @change="toggleSelectAll"
                class="checkbox-input"
              />
            </th>
            <th>用户信息</th>
            <th>角色</th>
            <th>状态</th>
            <th>最后登录</th>
            <th>创建时间</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr 
            v-for="user in filteredUsers" 
            :key="user.id"
            class="user-row"
            :class="{ 'selected': selectedUsers.includes(user.id) }"
          >
            <td class="checkbox-column">
              <input 
                type="checkbox" 
                :value="user.id"
                v-model="selectedUsers"
                class="checkbox-input"
              />
            </td>
            <td class="user-info">
              <div class="user-avatar">
                <img v-if="user.avatar" :src="user.avatar" :alt="user.username" />
                <div v-else class="avatar-placeholder">
                  {{ user.username.charAt(0).toUpperCase() }}
                </div>
              </div>
              <div class="user-details">
                <div class="username">{{ user.username }}</div>
                <div class="email">{{ user.email }}</div>
              </div>
            </td>
            <td>
              <span class="role-badge" :class="user.role">
                {{ getRoleText(user.role) }}
              </span>
            </td>
            <td>
              <div class="status-indicator" :class="user.status">
                <div class="status-dot"></div>
                <span>{{ getStatusText(user.status) }}</span>
              </div>
            </td>
            <td class="last-login">
              <div class="login-time">{{ user.lastLogin }}</div>
              <div class="login-ip" v-if="user.lastLoginIp">{{ user.lastLoginIp }}</div>
            </td>
            <td class="created-at">{{ user.createdAt }}</td>
            <td class="actions">
              <button class="btn-icon" @click="viewUser(user)" title="查看详情">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z" fill="currentColor"/>
                </svg>
              </button>
              <button class="btn-icon" @click="editUser(user)" title="编辑用户">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z" fill="currentColor"/>
                </svg>
              </button>
              <button class="btn-icon" @click="resetPassword(user)" title="重置密码">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M18 8h-1V6c0-2.76-2.24-5-5-5S7 3.24 7 6v2H6c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V10c0-1.1-.9-2-2-2zm-6 9c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2zm3.1-9H8.9V6c0-1.71 1.39-3.1 3.1-3.1 1.71 0 3.1 1.39 3.1 3.1v2z" fill="currentColor"/>
                </svg>
              </button>
              <button class="btn-icon danger" @click="deleteUser(user)" title="删除用户">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z" fill="currentColor"/>
                </svg>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- 批量操作 -->
    <div v-if="selectedUsers.length > 0" class="batch-actions">
      <div class="batch-info">
        已选择 {{ selectedUsers.length }} 个用户
      </div>
      <div class="batch-buttons">
        <button class="btn-secondary" @click="batchActivate">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
          批量激活
        </button>
        <button class="btn-secondary" @click="batchDeactivate">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
          </svg>
          批量停用
        </button>
        <button class="btn-danger" @click="batchDelete">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z" fill="currentColor"/>
          </svg>
          批量删除
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

// 搜索和筛选
const searchQuery = ref('')
const roleFilter = ref('')
const statusFilter = ref('')
const sortBy = ref('username')
const selectAll = ref(false)
const selectedUsers = ref<number[]>([])

// 模拟用户数据
const users = ref([
  {
    id: 1,
    username: 'admin',
    email: 'admin@bywg.com',
    role: 'admin',
    status: 'online',
    lastLogin: '2分钟前',
    lastLoginIp: '192.168.1.100',
    createdAt: '2024-01-15',
    avatar: null
  },
  {
    id: 2,
    username: 'operator1',
    email: 'operator1@bywg.com',
    role: 'operator',
    status: 'active',
    lastLogin: '1小时前',
    lastLoginIp: '192.168.1.101',
    createdAt: '2024-01-20',
    avatar: null
  },
  {
    id: 3,
    username: 'viewer1',
    email: 'viewer1@bywg.com',
    role: 'viewer',
    status: 'offline',
    lastLogin: '1天前',
    lastLoginIp: '192.168.1.102',
    createdAt: '2024-01-25',
    avatar: null
  },
  {
    id: 4,
    username: 'operator2',
    email: 'operator2@bywg.com',
    role: 'operator',
    status: 'inactive',
    lastLogin: '3天前',
    lastLoginIp: '192.168.1.103',
    createdAt: '2024-02-01',
    avatar: null
  },
  {
    id: 5,
    username: 'viewer2',
    email: 'viewer2@bywg.com',
    role: 'viewer',
    status: 'online',
    lastLogin: '30分钟前',
    lastLoginIp: '192.168.1.104',
    createdAt: '2024-02-05',
    avatar: null
  }
])

// 计算属性
const filteredUsers = computed(() => {
  let filtered = users.value

  // 搜索过滤
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(user => 
      user.username.toLowerCase().includes(query) ||
      user.email.toLowerCase().includes(query) ||
      getRoleText(user.role).toLowerCase().includes(query)
    )
  }

  // 角色过滤
  if (roleFilter.value) {
    filtered = filtered.filter(user => user.role === roleFilter.value)
  }

  // 状态过滤
  if (statusFilter.value) {
    filtered = filtered.filter(user => user.status === statusFilter.value)
  }

  // 排序
  filtered.sort((a, b) => {
    switch (sortBy.value) {
      case 'username':
        return a.username.localeCompare(b.username)
      case 'role':
        return a.role.localeCompare(b.role)
      case 'lastLogin':
        return new Date(b.lastLogin).getTime() - new Date(a.lastLogin).getTime()
      case 'createdAt':
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      default:
        return 0
    }
  })

  return filtered
})

const userStats = computed(() => {
  const total = users.value.length
  const active = users.value.filter(u => u.status === 'active' || u.status === 'online').length
  const admins = users.value.filter(u => u.role === 'admin').length
  const online = users.value.filter(u => u.status === 'online').length

  return { total, active, admins, online }
})

// 方法
function getRoleText(role: string) {
  const roleMap: Record<string, string> = {
    admin: '管理员',
    operator: '操作员',
    viewer: '观察员'
  }
  return roleMap[role] || '未知'
}

function getStatusText(status: string) {
  const statusMap: Record<string, string> = {
    online: '在线',
    active: '活跃',
    inactive: '非活跃',
    offline: '离线'
  }
  return statusMap[status] || '未知'
}

function toggleSelectAll() {
  if (selectAll.value) {
    selectedUsers.value = filteredUsers.value.map(user => user.id)
  } else {
    selectedUsers.value = []
  }
}

function refreshUsers() {
  console.log('刷新用户列表...')
}

function addUser() {
  console.log('添加新用户...')
}

function viewUser(user: any) {
  console.log('查看用户详情:', user.username)
}

function editUser(user: any) {
  console.log('编辑用户:', user.username)
}

function resetPassword(user: any) {
  console.log('重置密码:', user.username)
}

function deleteUser(user: any) {
  console.log('删除用户:', user.username)
}

function batchActivate() {
  console.log('批量激活用户:', selectedUsers.value)
}

function batchDeactivate() {
  console.log('批量停用用户:', selectedUsers.value)
}

function batchDelete() {
  console.log('批量删除用户:', selectedUsers.value)
}
</script>

<style scoped>
.users-container {
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

.stat-icon.total {
  background: linear-gradient(135deg, #4a90e2, #357abd);
}

.stat-icon.active {
  background: linear-gradient(135deg, #27ae60, #2ecc71);
}

.stat-icon.admin {
  background: linear-gradient(135deg, #9b59b6, #8e44ad);
}

.stat-icon.online {
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

.management-panel {
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

.filter-section {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px 24px;
  background: #f8f9fa;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
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
  width: 20px;
  height: 20px;
}

.search-input {
  width: 100%;
  padding: 10px 12px 10px 40px;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  font-size: 14px;
  background: white;
  transition: all 0.3s ease;
}

.search-input:focus {
  outline: none;
  border-color: #4a90e2;
  box-shadow: 0 0 0 3px rgba(74, 144, 226, 0.1);
}

.filter-controls {
  display: flex;
  gap: 12px;
}

.filter-select {
  padding: 10px 12px;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  font-size: 14px;
  background: white;
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-select:focus {
  outline: none;
  border-color: #4a90e2;
  box-shadow: 0 0 0 3px rgba(74, 144, 226, 0.1);
}

.users-table-container {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid rgba(0, 0, 0, 0.08);
  overflow: hidden;
}

.users-table {
  width: 100%;
  border-collapse: collapse;
}

.users-table th {
  background: #f8f9fa;
  padding: 16px 20px;
  text-align: left;
  font-weight: 600;
  color: #2c3e50;
  border-bottom: 1px solid #dee2e6;
  font-size: 14px;
}

.users-table td {
  padding: 16px 20px;
  border-bottom: 1px solid #f1f3f4;
  vertical-align: middle;
}

.user-row:hover {
  background: rgba(74, 144, 226, 0.05);
}

.user-row.selected {
  background: rgba(74, 144, 226, 0.1);
}

.checkbox-column {
  width: 50px;
  text-align: center;
}

.checkbox-input {
  width: 18px;
  height: 18px;
  accent-color: #4a90e2;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  overflow: hidden;
  flex-shrink: 0;
}

.user-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  background: linear-gradient(135deg, #4a90e2, #357abd);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 16px;
}

.user-details {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.username {
  font-weight: 600;
  color: #2c3e50;
  font-size: 14px;
}

.email {
  color: #6c757d;
  font-size: 12px;
}

.role-badge {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
}

.role-badge.admin {
  background: rgba(155, 89, 182, 0.1);
  color: #9b59b6;
}

.role-badge.operator {
  background: rgba(74, 144, 226, 0.1);
  color: #4a90e2;
}

.role-badge.viewer {
  background: rgba(108, 117, 125, 0.1);
  color: #6c757d;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  font-weight: 500;
}

.status-indicator.online {
  color: #27ae60;
}

.status-indicator.active {
  color: #4a90e2;
}

.status-indicator.inactive {
  color: #6c757d;
}

.status-indicator.offline {
  color: #e74c3c;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: currentColor;
}

.last-login {
  font-size: 12px;
  color: #6c757d;
}

.login-time {
  font-weight: 500;
  color: #2c3e50;
}

.login-ip {
  color: #adb5bd;
  margin-top: 2px;
}

.created-at {
  font-size: 12px;
  color: #6c757d;
}

.actions {
  display: flex;
  gap: 8px;
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

.btn-icon:hover {
  background: #f8f9fa;
  color: #495057;
  border-color: #adb5bd;
}

.btn-icon.danger:hover {
  background: #f8d7da;
  color: #721c24;
  border-color: #f5c6cb;
}

.batch-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 24px;
  background: rgba(74, 144, 226, 0.05);
  border: 1px solid rgba(74, 144, 226, 0.1);
  border-radius: 8px;
}

.batch-info {
  font-size: 14px;
  color: #2c3e50;
  font-weight: 500;
}

.batch-buttons {
  display: flex;
  gap: 12px;
}

.btn-danger {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: #dc3545;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-danger:hover {
  background: #c82333;
  transform: translateY(-1px);
}

@media (max-width: 768px) {
  .users-table-container {
    overflow-x: auto;
  }
  
  .users-table {
    min-width: 800px;
  }
  
  .filter-section {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }
  
  .search-box {
    max-width: none;
  }
  
  .filter-controls {
    flex-wrap: wrap;
  }
  
  .batch-actions {
    flex-direction: column;
    gap: 12px;
    align-items: stretch;
  }
  
  .batch-buttons {
    justify-content: center;
  }
}
</style>
