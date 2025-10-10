<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { getRoleInfo, formatLastLogin } from './utils/auth'
import { sendHeartbeat, getOnlineCount } from './api/auth'

const route = useRoute()
const sidebarCollapsed = ref(false)
const showUserMenu = ref(false)

// 认证服务基地址：优先取 VITE_AUTH_API_URL；未配置则走同域反代 /api
const AUTH_API_BASE = (import.meta.env.VITE_AUTH_API_URL as string | undefined)?.replace(/\/$/, '') || ''
const authUrl = (path: string) => `${AUTH_API_BASE || ''}${path}` || path

// 用户信息 - 从localStorage或API获取
const currentUser = ref({
  id: 0,
  username: '',
  fullName: '',
  email: '',
  role: '',
  roleName: '',
  avatar: '',
  lastLoginAt: '',
  department: '',
  phone: ''
})

// 从localStorage获取用户信息
function loadUserInfo() {
  try {
    const userInfoStr = localStorage.getItem('userInfo')
    if (userInfoStr) {
      const user = JSON.parse(userInfoStr)
      currentUser.value = {
        id: user.id || 0,
        username: user.username || '',
        fullName: user.fullName || user.username || '',
        email: user.email || '',
        role: user.role || '',
        roleName: getRoleInfo(user.role || '').name,
        avatar: user.avatar || '',
        lastLoginAt: user.lastLoginAt || '',
        department: user.department || '技术部',
        phone: user.phone || ''
      }
      console.log('从localStorage加载用户信息:', currentUser.value)
    } else {
      console.log('localStorage中没有用户信息')
    }
  } catch (error) {
    console.error('加载用户信息失败:', error)
  }
}


// 获取当前用户角色信息
const userRoleInfo = computed(() => getRoleInfo(currentUser.value.role))

// 系统状态
const systemStatus = ref({
  status: 'checking', // checking, online, offline, error
  message: '检查中...'
})
const onlineCount = ref(0)

// hasPermission(异步版)未在模板/代码中使用，已移除以消除未使用警告

// 同步权限检查函数（用于模板中的v-if）
function hasPermissionSync(permission: string): boolean {
  const role = currentUser.value.role
  
  // 权限映射
  const permissions: Record<string, string[]> = {
    admin: ['system', 'user_management', 'device_management', 'monitoring', 'all'],
    operator: ['device_management', 'monitoring'],
    technician: ['device_management', 'monitoring', 'system'],
    viewer: ['monitoring']
  }
  
  const userPermissions = permissions[role] || []
  return userPermissions.includes(permission) || userPermissions.includes('all')
}


// 判断是否为登录页面
const isLoginPage = computed(() => {
  return route.path === '/login'
})

function toggleSidebar() {
  sidebarCollapsed.value = !sidebarCollapsed.value
}

function getCurrentPageTitle() {
  const routeMap: Record<string, string> = {
    '/dashboard': '数据仪表板',
    '/devices': '设备管理',
    '/protocols': '协议管理',
    '/nodes': 'OPC UA节点',
    '/monitoring': '实时监控',
    '/alerts': '报警管理',
    '/device-groups': '设备分组',
    '/device-discovery': '设备发现',
    '/protocol-templates': '协议模板',
    '/node-browser': '节点浏览器',
    '/servers': '服务器管理',
    '/server-clusters': '集群监控',
    '/load-balancing': '负载均衡',
    '/settings': '系统设置',
    '/users': '用户管理',
    '/northbound': '北向对接',
    '/northbound/opcua': 'OPC UA 对接',
    '/northbound/mqtt': 'MQTT 对接',
    '/northbound/rest': 'REST 对接',
    '/northbound/connections': '连接中心'
  }
  return routeMap[route.path] || '未知页面'
}

function toggleUserMenu() {
  showUserMenu.value = !showUserMenu.value
}

function goToProfile() {
  showUserMenu.value = false
  window.location.href = '/profile'
}

function goToSettings() {
  showUserMenu.value = false
  // 跳转到系统设置页面
  window.location.href = '/settings'
}

// 刷新用户信息
async function refreshUserInfo() {
  showUserMenu.value = false
  console.log('刷新用户信息...')
  await fetchCurrentUser()
  console.log('用户信息已刷新:', currentUser.value)
}

// 检查系统状态
async function checkSystemStatus() {
  try {
    systemStatus.value = {
      status: 'checking',
      message: '检查中...'
    }

    // 检查认证服务状态（配置化）
    const authResponse = await fetch(authUrl('/api/auth/me'), {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`,
        'Content-Type': 'application/json'
      }
    })

    if (authResponse.ok) {
      systemStatus.value = {
        status: 'online',
        message: '系统正常'
      }
    } else if (authResponse.status === 401) {
      systemStatus.value = {
        status: 'error',
        message: '认证失败'
      }
    } else {
      systemStatus.value = {
        status: 'offline',
        message: '服务异常'
      }
    }
  } catch (error) {
    console.error('系统状态检查失败:', error)
    systemStatus.value = {
      status: 'offline',
      message: '连接失败'
    }
  }
}

function goToHelp() {
  showUserMenu.value = false
  window.location.href = '/help'
}

async function logout() {
  showUserMenu.value = false
  if (confirm('确定要注销登录吗？')) {
    // 清除心跳定时器
    if (heartbeatTimer) {
      clearInterval(heartbeatTimer)
      heartbeatTimer = null
    }
    
    // 调用后端注销API（清除在线状态，配置化）
    try {
      await fetch(authUrl('/api/auth/logout'), {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        }
      })
    } catch (error) {
      console.warn('注销API调用失败:', error)
    }
    
    // 清除本地状态
    localStorage.removeItem('token')
    localStorage.removeItem('userInfo')
    localStorage.removeItem('isLoggedIn')
    
    // 重置用户信息
    currentUser.value = {
      id: 0,
      username: '',
      fullName: '',
      email: '',
      role: '',
      roleName: '',
      avatar: '',
      lastLoginAt: '',
      department: '',
      phone: ''
    }
    
    // 跳转到登录页面
    window.location.href = '/login'
  }
}

// 从API获取当前用户信息
async function fetchCurrentUser() {
  try {
    const token = localStorage.getItem('token')
    if (!token) {
      console.log('未找到登录token，使用本地用户信息')
      return
    }

    const response = await fetch(authUrl('/api/auth/me'), {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    })

    if (response.ok) {
      const userData = await response.json()
      console.log('从API获取用户信息:', userData)
      
      currentUser.value = {
        id: userData.id,
        username: userData.username,
        fullName: userData.fullName || userData.username,
        email: userData.email,
        role: userData.role.toLowerCase(), // 转换为小写以匹配前端逻辑
        roleName: getRoleInfo(userData.role.toLowerCase()).name,
        avatar: userData.avatar || '',
        lastLoginAt: userData.lastLoginAt || new Date().toISOString(),
        department: userData.department || '技术部',
        phone: userData.phone || ''
      }
      
      // 保存到localStorage
      localStorage.setItem('userInfo', JSON.stringify(currentUser.value))
      console.log('用户信息已更新:', currentUser.value)
    } else if (response.status === 401) {
      // Token过期，清除登录状态
      console.log('Token过期，清除登录状态')
      localStorage.removeItem('token')
      localStorage.removeItem('userInfo')
      localStorage.removeItem('isLoggedIn')
      window.location.href = '/login'
    } else {
      console.log('API获取用户信息失败，使用本地缓存')
    }
  } catch (error) {
    console.log('API获取用户信息出错，使用本地缓存:', error)
  }
}

// 移除未使用的 checkPermission 函数以消除构建告警

// 点击外部关闭下拉菜单
function handleClickOutside(event: Event) {
  const target = event.target as HTMLElement
  if (!target.closest('.user-dropdown')) {
    showUserMenu.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  // 加载用户信息
  loadUserInfo()
  // 尝试从API获取最新用户信息
  fetchCurrentUser()
  // 检查系统状态
  checkSystemStatus()
  // 启动心跳与在线人数刷新
  startHeartbeat()
  
  // 调试信息
  console.log('当前用户信息:', currentUser.value)
  console.log('用户角色信息:', userRoleInfo.value)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  // 清理心跳定时器
  if (heartbeatTimer) {
    clearInterval(heartbeatTimer)
    heartbeatTimer = null
  }
})

let heartbeatTimer: number | null = null
async function startHeartbeat() {
  await sendHeartbeat()
  await refreshOnlineCount()
  heartbeatTimer = window.setInterval(async () => {
    await sendHeartbeat()
    await refreshOnlineCount()
  }, 30000)
}

async function refreshOnlineCount() {
  onlineCount.value = await getOnlineCount()
}
</script>

<template>
  <!-- 登录页面独立布局 -->
  <div v-if="isLoginPage" class="login-layout">
    <RouterView />
  </div>
  
  <!-- 主界面布局 -->
  <div v-else class="layout">
    <!-- 侧边栏 -->
    <aside class="sidebar" :class="{ collapsed: sidebarCollapsed }">
      <div class="sidebar-header">
      <div class="brand">
          <div class="brand-icon">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect x="3" y="3" width="18" height="18" rx="2" stroke="currentColor" stroke-width="2"/>
              <path d="M9 9h6v6H9z" fill="currentColor"/>
              <circle cx="12" cy="12" r="2" fill="white"/>
            </svg>
          </div>
          <div class="brand-text" v-show="!sidebarCollapsed">
        <span class="title">BYWG</span>
            <span class="subtitle">工业边缘网关</span>
          </div>
        </div>
        <button v-show="!sidebarCollapsed" class="sidebar-toggle" @click="toggleSidebar" title="折叠侧边栏">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z" fill="currentColor"/>
          </svg>
        </button>
      </div>
      
      <!-- 折叠状态下的切换按钮 -->
      <div v-show="sidebarCollapsed" class="collapsed-toggle">
        <button class="sidebar-toggle collapsed" @click="toggleSidebar" title="展开侧边栏">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M8.59 16.59L10 18l6-6-6-6-1.41 1.41L13.17 12z" fill="currentColor"/>
          </svg>
        </button>
      </div>
      
      <nav class="sidebar-nav">
        <!-- 系统监控 -->
        <div class="nav-section" v-if="hasPermissionSync('monitoring')">
          <div class="nav-section-title" v-show="!sidebarCollapsed">系统监控</div>
          <RouterLink to="/dashboard" class="nav-item" :title="sidebarCollapsed ? '数据仪表板' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M3 13h8V3H3v10zm0 8h8v-6H3v6zm10 0h8V11h-8v10zm0-18v6h8V3h-8z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">数据仪表板</span>
          </RouterLink>
          <RouterLink to="/monitoring" class="nav-item" :title="sidebarCollapsed ? '实时监控' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">实时监控</span>
          </RouterLink>
          <RouterLink to="/alerts" class="nav-item" :title="sidebarCollapsed ? '报警管理' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 22c1.1 0 2-.9 2-2h-4c0 1.1.89 2 2 2zm6-6v-5c0-3.07-1.64-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.63 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">报警管理</span>
          </RouterLink>
        </div>

        <!-- 设备管理 -->
        <div class="nav-section" v-if="hasPermissionSync('device_management')">
          <div class="nav-section-title" v-show="!sidebarCollapsed">设备管理</div>
          <RouterLink to="/devices" class="nav-item" :title="sidebarCollapsed ? '设备列表' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">设备列表</span>
          </RouterLink>
          <RouterLink to="/device-groups" class="nav-item" :title="sidebarCollapsed ? '设备分组' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M16 4c0-1.11.89-2 2-2s2 .89 2 2-.89 2-2 2-2-.89-2-2zm4 18v-6h2.5l-2.54-7.63A1.5 1.5 0 0 0 18.54 8H16c-.8 0-1.54.37-2.01.99L12 11l-1.99-2.01A2.5 2.5 0 0 0 8 8H5.46c-.8 0-1.54.37-2.01.99L1 13.5V16h2v6h2v-6h2.5l2.5-7.5h2l2.5 7.5H14v6h2z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">设备分组</span>
          </RouterLink>
          <RouterLink to="/device-discovery" class="nav-item" :title="sidebarCollapsed ? '设备发现' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">设备发现</span>
          </RouterLink>
        </div>

        <!-- 协议管理 -->
        <div class="nav-section">
          <div class="nav-section-title" v-show="!sidebarCollapsed">协议管理</div>
          <RouterLink to="/protocols" class="nav-item" :title="sidebarCollapsed ? '协议配置' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">协议配置</span>
          </RouterLink>
          <RouterLink to="/protocol-templates" class="nav-item" :title="sidebarCollapsed ? '协议模板' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">协议模板</span>
          </RouterLink>
        </div>

        <!-- OPC UA管理 -->
        <div class="nav-section">
          <div class="nav-section-title" v-show="!sidebarCollapsed">OPC UA管理</div>
          <RouterLink to="/nodes" class="nav-item" :title="sidebarCollapsed ? '节点管理' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-1 17.93c-3.94-.49-7-3.85-7-7.93 0-.62.08-1.21.21-1.79L9 15v1c0 1.1.9 2 2 2v1.93zm6.9-2.54c-.26-.81-1-1.39-1.9-1.39h-1v-3c0-.55-.45-1-1-1H8v-2h2c.55 0 1-.45 1-1V7h2c1.1 0 2-.9 2-2v-.41c2.93 1.19 5 4.06 5 7.41 0 2.08-.8 3.97-2.1 5.39z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">节点管理</span>
          </RouterLink>
          <RouterLink to="/node-browser" class="nav-item" :title="sidebarCollapsed ? '节点浏览器' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">节点浏览器</span>
          </RouterLink>
          <RouterLink to="/opcua/auth" class="nav-item" :title="sidebarCollapsed ? '认证管理' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 17a2 2 0 1 0 0-4 2 2 0 0 0 0 4zm6-9h-1V7a5 5 0 0 0-10 0v1H6a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V10a2 2 0 0 0-2-2zm-9-1a3 3 0 0 1 6 0v1H9V7z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">认证管理</span>
          </RouterLink>
          <RouterLink to="/opcua/mqtt" class="nav-item" :title="sidebarCollapsed ? 'MQTT 对接' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M4 4h16v16H4V4zm4 4h8v2H8V8zm0 4h8v2H8v-2z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">MQTT 对接</span>
          </RouterLink>
          <RouterLink to="/opcua/rest" class="nav-item" :title="sidebarCollapsed ? 'REST 对接' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M3 5h18v2H3V5zm0 6h18v2H3v-2zm0 6h18v2H3v-2z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">REST 对接</span>
          </RouterLink>
        </div>

        <!-- 服务器集群 -->
        <div class="nav-section">
          <div class="nav-section-title" v-show="!sidebarCollapsed">服务器集群</div>
          <RouterLink to="/servers" class="nav-item" :title="sidebarCollapsed ? '服务器管理' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M4 6h16v2H4zm0 5h16v2H4zm0 5h16v2H4z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">服务器管理</span>
          </RouterLink>
          <RouterLink to="/server-clusters" class="nav-item" :title="sidebarCollapsed ? '集群监控' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">集群监控</span>
          </RouterLink>
          <RouterLink to="/load-balancing" class="nav-item" :title="sidebarCollapsed ? '负载均衡' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">负载均衡</span>
          </RouterLink>
        </div>

        <!-- 系统设置 -->
        <div class="nav-section" v-if="hasPermissionSync('system')">
          <div class="nav-section-title" v-show="!sidebarCollapsed">系统设置</div>
          <RouterLink to="/settings" class="nav-item" :title="sidebarCollapsed ? '系统设置' : ''">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M19.14,12.94c0.04-0.3,0.06-0.61,0.06-0.94c0-0.32-0.02-0.64-0.07-0.94l2.03-1.58c0.18-0.14,0.23-0.41,0.12-0.61 l-1.92-3.32c-0.12-0.22-0.37-0.29-0.59,0.22l-2.39,0.96c-0.5-0.38-1.03-0.7-1.62-0.94L14.4,2.81c-0.04-0.24-0.24-0.41-0.48-0.41 h-3.84c-0.24,0-0.43,0.17-0.47,0.41L9.25,5.35C8.66,5.59,8.12,5.92,7.63,6.29L5.24,5.33c-0.22-0.08-0.47,0-0.59,0.22L2.74,8.87 C2.62,9.08,2.66,9.34,2.86,9.48l2.03,1.58C4.84,11.36,4.8,11.69,4.8,12s0.02,0.64,0.07,0.94l-2.03,1.58 c-0.18,0.14-0.23,0.41-0.12,0.61l1.92,3.32c0.12-0.22,0.07-0.47-0.12-0.61L19.14,12.94z M12,15.6c-1.98,0-3.6-1.62-3.6-3.6 s1.62-3.6,3.6-3.6s3.6,1.62,3.6,3.6S13.98,15.6,12,15.6z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">系统设置</span>
          </RouterLink>
          <RouterLink to="/users" class="nav-item" :title="sidebarCollapsed ? '用户管理' : ''" v-if="hasPermissionSync('user_management')">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M16 4c0-1.11.89-2 2-2s2 .89 2 2-.89 2-2 2-2-.89-2-2zm4 18v-6h2.5l-2.54-7.63A1.5 1.5 0 0 0 18.54 8H16c-.8 0-1.54.37-2.01.99L12 11l-1.99-2.01A2.5 2.5 0 0 0 8 8H5.46c-.8 0-1.54.37-2.01.99L1 13.5V16h2v6h2v-6h2.5l2.5-7.5h2l2.5 7.5H14v6h2z" fill="currentColor"/>
            </svg>
            <span v-show="!sidebarCollapsed">用户管理</span>
          </RouterLink>
        </div>
      </nav>
    </aside>

    <!-- 主内容区域 -->
    <div class="main-container">
      <header class="header">
        <div class="header-content">
          <div class="header-left">
            <button class="mobile-menu-toggle" @click="toggleSidebar">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M3 18h18v-2H3v2zm0-5h18v-2H3v2zm0-7v2h18V6H3z" fill="currentColor"/>
              </svg>
            </button>
            <div class="breadcrumb">
              <span class="breadcrumb-item">工业边缘网关</span>
              <span class="breadcrumb-separator">/</span>
              <span class="breadcrumb-current">{{ getCurrentPageTitle() }}</span>
            </div>
          </div>
          <div class="header-actions">
            <div class="status-indicator" @click="checkSystemStatus" title="点击刷新状态">
              <div class="status-dot" :class="systemStatus.status"></div>
              <span>{{ systemStatus.message }}</span>
            </div>
            <div class="user-menu">
              <div class="user-dropdown" @click="toggleUserMenu">
                <button class="user-button">
                  <div class="user-avatar-small">
                    <i :class="userRoleInfo.icon" style="font-size: 14px; color: white;"></i>
                  </div>
                  <div class="user-info-small">
                    <span class="user-name-small">{{ currentUser.fullName || currentUser.username || '未登录' }}</span>
                    <span class="user-role-small" :style="{ color: userRoleInfo.color }">{{ userRoleInfo.name || '未知角色' }}</span>
                  </div>
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" class="dropdown-arrow">
                    <path d="M6 9l6 6 6-6" stroke="currentColor" stroke-width="2"/>
                  </svg>
                </button>
                <div v-if="showUserMenu" class="user-dropdown-menu">
                  <div class="user-info">
                    <div class="user-avatar" :style="{ backgroundColor: userRoleInfo.color }">
                      <i :class="userRoleInfo.icon" style="font-size: 18px; color: white;"></i>
                    </div>
                    <div class="user-details">
                      <div class="user-name">{{ currentUser.fullName || currentUser.username || '未登录' }}</div>
                      <div class="user-role" :style="{ color: userRoleInfo.color }">{{ userRoleInfo.name || '未知角色' }}</div>
                      <div class="user-email">{{ currentUser.email || '未设置邮箱' }}</div>
                      <div class="user-department">{{ currentUser.department || '未设置部门' }}</div>
                      <div class="user-last-login">最后登录: {{ formatLastLogin(currentUser.lastLoginAt) || '未知' }}</div>
                    </div>
                  </div>
                  <div class="dropdown-divider"></div>
                  <div class="dropdown-items">
                    <button class="dropdown-item" @click="goToProfile">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" stroke="currentColor" stroke-width="2"/>
                        <circle cx="12" cy="7" r="4" stroke="currentColor" stroke-width="2"/>
                      </svg>
                      个人资料
                    </button>
                    <button class="dropdown-item" @click="goToSettings">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <circle cx="12" cy="12" r="3" stroke="currentColor" stroke-width="2"/>
                        <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1 1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z" stroke="currentColor" stroke-width="2"/>
                      </svg>
                      系统设置
                    </button>
                    <button class="dropdown-item" @click="refreshUserInfo">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8" stroke="currentColor" stroke-width="2"/>
                        <path d="M21 3v5h-5" stroke="currentColor" stroke-width="2"/>
                        <path d="M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16" stroke="currentColor" stroke-width="2"/>
                        <path d="M3 21v-5h5" stroke="currentColor" stroke-width="2"/>
                      </svg>
                      刷新用户信息
                    </button>
                    <button class="dropdown-item" @click="goToHelp">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
                        <path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3" stroke="currentColor" stroke-width="2"/>
                        <line x1="12" y1="17" x2="12.01" y2="17" stroke="currentColor" stroke-width="2"/>
                      </svg>
                      帮助中心
                    </button>
                    <div class="dropdown-divider"></div>
                    <button class="dropdown-item logout" @click="logout">
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" stroke="currentColor" stroke-width="2"/>
                        <polyline points="16,17 21,12 16,7" stroke="currentColor" stroke-width="2"/>
                        <line x1="21" y1="12" x2="9" y2="12" stroke="currentColor" stroke-width="2"/>
                      </svg>
                      注销登录
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
    </header>
      
    <main class="main">
      <RouterView />
    </main>
  </div>
  </div>
</template>

<style scoped>
/* 登录页面独立布局 */
.login-layout {
  width: 100vw;
  height: 100vh;
  overflow: hidden;
}

/* 主界面布局 */
.layout { 
  display: flex; 
  min-height: 100vh; 
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  width: 100vw;
  overflow-x: hidden;
}

/* 侧边栏样式 */
.sidebar {
  width: 280px;
  height: 100vh;
  background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
  color: #fff;
  display: flex;
  flex-direction: column;
  transition: all 0.3s ease;
  box-shadow: 4px 0 20px rgba(0, 0, 0, 0.15);
  position: fixed;
  top: 0;
  left: 0;
  z-index: 1000;
  overflow: hidden;
}

.sidebar.collapsed {
  width: 80px;
}

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  flex-shrink: 0;
}

.brand {
  display: flex;
  align-items: center;
  gap: 12px;
}

.brand-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
  flex-shrink: 0;
}

.brand-text {
  display: flex;
  flex-direction: column;
  gap: 2px;
  transition: opacity 0.3s ease;
}

.title {
  font-weight: 800;
  font-size: 18px;
  letter-spacing: 1px;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

.subtitle {
  color: rgba(255, 255, 255, 0.8);
  font-size: 12px;
  font-weight: 400;
  letter-spacing: 0.5px;
}

.sidebar-toggle {
  background: rgba(255, 255, 255, 0.1);
  border: none;
  color: #fff;
  padding: 8px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0.7;
  width: 32px;
  height: 32px;
  flex-shrink: 0;
}

.sidebar-toggle:hover {
  background: rgba(255, 255, 255, 0.2);
  opacity: 1;
}

.sidebar.collapsed .sidebar-toggle {
  opacity: 1;
  background: rgba(255, 255, 255, 0.15);
  width: 28px;
  height: 28px;
  padding: 6px;
}

.collapsed-toggle {
  display: flex;
  justify-content: center;
  padding: 16px 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.collapsed-toggle .sidebar-toggle {
  opacity: 1;
  background: rgba(255, 255, 255, 0.15);
  width: 32px;
  height: 32px;
  padding: 8px;
}

.sidebar-nav {
  flex: 1;
  padding: 20px 0;
  overflow-y: auto;
}

.nav-section {
  margin-bottom: 24px;
}

.nav-section-title {
  font-size: 12px;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.6);
  text-transform: uppercase;
  letter-spacing: 1px;
  margin-bottom: 12px;
  padding: 0 20px;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 20px;
  color: rgba(255, 255, 255, 0.9);
  text-decoration: none;
  transition: all 0.3s ease;
  font-weight: 500;
  font-size: 14px;
  position: relative;
  border-left: 3px solid transparent;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.1);
  border-left-color: rgba(255, 255, 255, 0.3);
}

.nav-item.router-link-active {
  background: rgba(255, 255, 255, 0.15);
  border-left-color: #4a90e2;
  font-weight: 600;
}

.nav-item svg {
  flex-shrink: 0;
  width: 20px;
  height: 20px;
}

/* 主容器样式 */
.main-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
  width: 100%;
  margin-left: 280px;
  transition: margin-left 0.3s ease;
}

.sidebar.collapsed + .main-container {
  margin-left: 80px;
}

.header {
  background: linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%);
  color: #2c3e50;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
  position: sticky;
  top: 0;
  z-index: 100;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  width: 100%;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.mobile-menu-toggle {
  display: none;
  background: none;
  border: none;
  color: #2c3e50;
  padding: 8px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.mobile-menu-toggle:hover {
  background: rgba(74, 144, 226, 0.1);
}

.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #6c757d;
}

.breadcrumb-item {
  color: #2c3e50;
  font-weight: 500;
}

.breadcrumb-separator {
  color: #adb5bd;
}

.breadcrumb-current {
  color: #4a90e2;
  font-weight: 600;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: rgba(40, 167, 69, 0.1);
  border-radius: 20px;
  border: 1px solid rgba(40, 167, 69, 0.2);
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.status-indicator:hover {
  background: rgba(40, 167, 69, 0.2);
}

.status-indicator.checking {
  background: rgba(255, 193, 7, 0.1);
  border-color: rgba(255, 193, 7, 0.2);
  color: #ffc107;
}

.status-indicator.offline {
  background: rgba(220, 53, 69, 0.1);
  border-color: rgba(220, 53, 69, 0.2);
  color: #dc3545;
}

.status-indicator.error {
  background: rgba(253, 126, 20, 0.1);
  border-color: rgba(253, 126, 20, 0.2);
  color: #fd7e14;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  animation: pulse 2s infinite;
}

.status-dot.checking {
  background: #ffc107;
  box-shadow: 0 0 10px rgba(255, 193, 7, 0.5);
  animation: pulse 1s infinite;
}

.status-dot.online {
  background: #28a745;
  box-shadow: 0 0 10px rgba(40, 167, 69, 0.5);
}

.status-dot.offline {
  background: #dc3545;
  box-shadow: 0 0 10px rgba(220, 53, 69, 0.5);
}

.status-dot.error {
  background: #fd7e14;
  box-shadow: 0 0 10px rgba(253, 126, 20, 0.5);
}

.user-menu {
  display: flex;
  align-items: center;
}

.user-button {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 16px;
  background: rgba(74, 144, 226, 0.1);
  border: 1px solid rgba(74, 144, 226, 0.2);
  border-radius: 20px;
  color: #2c3e50;
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 500;
}

.user-avatar-small {
  width: 32px;
  height: 32px;
  background: #4a90e2;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  flex-shrink: 0;
}

.user-info-small {
  display: flex;
  flex-direction: column;
  gap: 2px;
  text-align: left;
}

.user-name-small {
  font-size: 14px;
  font-weight: 600;
  color: #2c3e50;
  line-height: 1.2;
}

.user-role-small {
  font-size: 12px;
  font-weight: 500;
  line-height: 1.2;
}

.user-button:hover {
  background: rgba(74, 144, 226, 0.2);
  transform: translateY(-1px);
}

.user-dropdown {
  position: relative;
}

.dropdown-arrow {
  transition: transform 0.3s ease;
}

.user-dropdown:hover .dropdown-arrow {
  transform: rotate(180deg);
}

.user-dropdown-menu {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 8px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
  border: 1px solid rgba(0, 0, 0, 0.08);
  min-width: 200px;
  z-index: 1000;
  overflow: hidden;
}

.user-info {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 16px;
  background: #f8f9fa;
}

.user-avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  flex-shrink: 0;
}

.user-details {
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
}

.user-name {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  line-height: 1.2;
}

.user-role {
  font-size: 13px;
  font-weight: 500;
  line-height: 1.2;
}

.user-email {
  font-size: 12px;
  color: #6c757d;
  line-height: 1.2;
}

.user-department {
  font-size: 12px;
  color: #6c757d;
  line-height: 1.2;
}

.user-last-login {
  font-size: 11px;
  color: #95a5a6;
  line-height: 1.2;
  margin-top: 4px;
  padding-top: 4px;
  border-top: 1px solid #e9ecef;
}

.dropdown-divider {
  height: 1px;
  background: #e9ecef;
  margin: 0;
}

.dropdown-items {
  padding: 8px 0;
}

.dropdown-item {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  padding: 10px 16px;
  background: none;
  border: none;
  font-size: 14px;
  color: #2c3e50;
  cursor: pointer;
  transition: all 0.3s ease;
  text-align: left;
}

.dropdown-item:hover {
  background: #f8f9fa;
  color: #00d4ff;
}

.dropdown-item.logout {
  color: #e74c3c;
}

.dropdown-item.logout:hover {
  background: rgba(231, 76, 60, 0.1);
  color: #c0392b;
}

.main {
  flex: 1;
  padding: 24px;
  width: 100%;
  background: transparent;
  min-height: calc(100vh - 80px);
  overflow-x: auto;
}

@keyframes pulse {
  0% { opacity: 1; }
  50% { opacity: 0.5; }
  100% { opacity: 1; }
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .sidebar {
    width: 240px;
  }
  
  .sidebar.collapsed {
    width: 70px;
  }
  
  .main {
    padding: 20px;
  }
}

@media (max-width: 768px) {
  .sidebar {
    position: fixed;
    top: 0;
    left: 0;
    height: 100vh;
    z-index: 1000;
    transform: translateX(-100%);
    transition: transform 0.3s ease;
  }
  
  .sidebar:not(.collapsed) {
    transform: translateX(0);
  }
  
  .mobile-menu-toggle {
    display: block;
  }
  
  .main-container {
    width: 100%;
    margin-left: 0;
  }
  
  .header-content {
    padding: 12px 16px;
  }
  
  .main {
    padding: 16px;
  }
  
  .breadcrumb {
    font-size: 13px;
  }
}

@media (max-width: 480px) {
  .header-content {
    padding: 10px 12px;
  }
  
  .main {
    padding: 12px;
  }
  
  .breadcrumb {
    font-size: 12px;
  }
  
  .status-indicator span {
    display: none;
  }
  
  .user-button span {
    display: none;
  }
}

/* 侧边栏遮罩层（移动端） */
@media (max-width: 768px) {
  .sidebar:not(.collapsed)::after {
    content: '';
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: -1;
  }
}
</style>
