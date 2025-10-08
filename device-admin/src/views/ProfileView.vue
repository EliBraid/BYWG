<template>
  <div class="profile-container">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-left">
        <h1>个人资料</h1>
        <p>管理您的个人信息和账户设置</p>
      </div>
      <div class="header-right">
        <button @click="saveProfile" class="save-btn" :disabled="!hasChanges">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z" stroke="currentColor" stroke-width="2"/>
            <polyline points="17,21 17,13 7,13 7,21" stroke="currentColor" stroke-width="2"/>
            <polyline points="7,3 7,8 15,8" stroke="currentColor" stroke-width="2"/>
          </svg>
          保存更改
        </button>
      </div>
    </div>

    <!-- 主要内容 -->
    <div class="profile-content">
      <!-- 左侧：个人信息 -->
      <div class="profile-main">
        <!-- 基本信息 -->
        <div class="profile-section">
          <div class="section-header">
            <h2>基本信息</h2>
            <p>管理您的基本个人信息</p>
          </div>
          <div class="profile-form">
            <div class="form-row">
              <div class="form-group">
                <label for="username">用户名</label>
                <input 
                  id="username"
                  v-model="profileForm.username" 
                  type="text" 
                  class="form-input"
                  placeholder="输入用户名"
                />
              </div>
              <div class="form-group">
                <label for="email">邮箱地址</label>
                <input 
                  id="email"
                  v-model="profileForm.email" 
                  type="email" 
                  class="form-input"
                  placeholder="输入邮箱地址"
                />
              </div>
            </div>
            
            <div class="form-row">
              <div class="form-group">
                <label for="fullName">真实姓名</label>
                <input 
                  id="fullName"
                  v-model="profileForm.fullName" 
                  type="text" 
                  class="form-input"
                  placeholder="输入真实姓名"
                />
              </div>
              <div class="form-group">
                <label for="phone">手机号码</label>
                <input 
                  id="phone"
                  v-model="profileForm.phone" 
                  type="tel" 
                  class="form-input"
                  placeholder="输入手机号码"
                />
              </div>
            </div>
            
            <div class="form-group">
              <label for="department">所属部门</label>
              <select v-model="profileForm.department" class="form-select">
                <option value="">选择部门</option>
                <option value="技术部">技术部</option>
                <option value="运维部">运维部</option>
                <option value="管理部">管理部</option>
                <option value="安全部">安全部</option>
              </select>
            </div>
            
            <div class="form-group">
              <label for="bio">个人简介</label>
              <textarea 
                id="bio"
                v-model="profileForm.bio" 
                class="form-textarea"
                placeholder="输入个人简介"
                rows="4"
              ></textarea>
            </div>
          </div>
        </div>

        <!-- 安全设置 -->
        <div class="profile-section">
          <div class="section-header">
            <h2>安全设置</h2>
            <p>管理您的账户安全设置</p>
          </div>
          <div class="security-settings">
            <div class="security-item">
              <div class="security-info">
                <h3>修改密码</h3>
                <p>定期更新密码以确保账户安全</p>
              </div>
              <button @click="showChangePasswordModal = true" class="security-btn">
                修改密码
              </button>
            </div>
            
            <div class="security-item">
              <div class="security-info">
                <h3>两步验证</h3>
                <p>启用两步验证以增强账户安全性</p>
                <span class="security-status" :class="{ enabled: profileForm.twoFactorEnabled }">
                  {{ profileForm.twoFactorEnabled ? '已启用' : '未启用' }}
                </span>
              </div>
              <button @click="toggleTwoFactor" class="security-btn">
                {{ profileForm.twoFactorEnabled ? '禁用' : '启用' }}
              </button>
            </div>
            
            <div class="security-item">
              <div class="security-info">
                <h3>登录通知</h3>
                <p>接收异常登录活动的邮件通知</p>
                <span class="security-status" :class="{ enabled: profileForm.loginNotifications }">
                  {{ profileForm.loginNotifications ? '已启用' : '未启用' }}
                </span>
              </div>
              <button @click="toggleLoginNotifications" class="security-btn">
                {{ profileForm.loginNotifications ? '禁用' : '启用' }}
              </button>
            </div>
          </div>
        </div>

        <!-- 偏好设置 -->
        <div class="profile-section">
          <div class="section-header">
            <h2>偏好设置</h2>
            <p>自定义您的使用体验</p>
          </div>
          <div class="preference-settings">
            <div class="preference-item">
              <div class="preference-info">
                <h3>语言设置</h3>
                <p>选择您偏好的界面语言</p>
              </div>
              <select v-model="profileForm.language" class="form-select">
                <option value="zh-CN">简体中文</option>
                <option value="en-US">English</option>
                <option value="ja-JP">日本語</option>
              </select>
            </div>
            
            <div class="preference-item">
              <div class="preference-info">
                <h3>时区设置</h3>
                <p>设置您所在的时区</p>
              </div>
              <select v-model="profileForm.timezone" class="form-select">
                <option value="Asia/Shanghai">北京时间 (UTC+8)</option>
                <option value="Asia/Tokyo">东京时间 (UTC+9)</option>
                <option value="America/New_York">纽约时间 (UTC-5)</option>
                <option value="Europe/London">伦敦时间 (UTC+0)</option>
              </select>
            </div>
            
            <div class="preference-item">
              <div class="preference-info">
                <h3>主题设置</h3>
                <p>选择您喜欢的界面主题</p>
              </div>
              <div class="theme-options">
                <label class="theme-option">
                  <input v-model="profileForm.theme" type="radio" value="light" />
                  <span class="theme-preview light"></span>
                  <span>浅色主题</span>
                </label>
                <label class="theme-option">
                  <input v-model="profileForm.theme" type="radio" value="dark" />
                  <span class="theme-preview dark"></span>
                  <span>深色主题</span>
                </label>
                <label class="theme-option">
                  <input v-model="profileForm.theme" type="radio" value="auto" />
                  <span class="theme-preview auto"></span>
                  <span>跟随系统</span>
                </label>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧：用户信息卡片 -->
      <div class="profile-sidebar">
        <div class="user-card">
          <div class="user-avatar-large">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z" fill="currentColor"/>
            </svg>
          </div>
          <div class="user-info">
            <h3>{{ profileForm.fullName || profileForm.username }}</h3>
            <p class="user-role">{{ profileForm.department || '未设置部门' }}</p>
            <p class="user-email">{{ profileForm.email }}</p>
          </div>
        </div>

        <div class="stats-card">
          <h4>账户统计</h4>
          <div class="stats-list">
            <div class="stat-item">
              <span class="stat-label">注册时间</span>
              <span class="stat-value">{{ formatDate(profileForm.createdAt) }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">最后登录</span>
              <span class="stat-value">{{ formatDate(profileForm.lastLogin) }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">登录次数</span>
              <span class="stat-value">{{ profileForm.loginCount }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">在线时长</span>
              <span class="stat-value">{{ profileForm.onlineTime }}</span>
            </div>
          </div>
        </div>

        <div class="activity-card">
          <h4>最近活动</h4>
          <div class="activity-list">
            <div v-for="activity in recentActivities" :key="activity.id" class="activity-item">
              <div class="activity-icon">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path :d="activity.icon" stroke="currentColor" stroke-width="2"/>
                </svg>
              </div>
              <div class="activity-content">
                <div class="activity-text">{{ activity.text }}</div>
                <div class="activity-time">{{ activity.time }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 修改密码模态框 -->
    <div v-if="showChangePasswordModal" class="modal-overlay" @click="closeChangePasswordModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>修改密码</h3>
          <button @click="closeChangePasswordModal" class="close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="changePassword" class="password-form">
            <div class="form-group">
              <label for="currentPassword">当前密码</label>
              <input 
                id="currentPassword"
                v-model="passwordForm.currentPassword" 
                type="password" 
                class="form-input"
                placeholder="输入当前密码"
                required
              />
            </div>
            <div class="form-group">
              <label for="newPassword">新密码</label>
              <input 
                id="newPassword"
                v-model="passwordForm.newPassword" 
                type="password" 
                class="form-input"
                placeholder="输入新密码"
                required
              />
            </div>
            <div class="form-group">
              <label for="confirmPassword">确认新密码</label>
              <input 
                id="confirmPassword"
                v-model="passwordForm.confirmPassword" 
                type="password" 
                class="form-input"
                placeholder="再次输入新密码"
                required
              />
            </div>
            <div class="password-requirements">
              <h4>密码要求：</h4>
              <ul>
                <li>至少8个字符</li>
                <li>包含大小写字母</li>
                <li>包含数字</li>
                <li>包含特殊字符</li>
              </ul>
            </div>
          </form>
        </div>
        <div class="modal-footer">
          <button @click="closeChangePasswordModal" class="modal-btn cancel">取消</button>
          <button @click="changePassword" class="modal-btn primary">确认修改</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'

// 响应式数据
const hasChanges = ref(false)
const showChangePasswordModal = ref(false)

// 个人资料表单
const profileForm = reactive({
  username: 'admin',
  email: 'admin@bywg.com',
  fullName: '系统管理员',
  phone: '13800138000',
  department: '技术部',
  bio: '负责BYWG工业边缘网关系统的管理和维护工作',
  twoFactorEnabled: false,
  loginNotifications: true,
  language: 'zh-CN',
  timezone: 'Asia/Shanghai',
  theme: 'light',
  createdAt: new Date('2024-01-01'),
  lastLogin: new Date(),
  loginCount: 156,
  onlineTime: '2天 8小时'
})

// 密码表单
const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

// 最近活动
const recentActivities = ref([
  {
    id: 1,
    icon: 'M9 19c-5 0-9-4-9-9s4-9 9-9 9 4 9 9-4 9-9 9zM21 3l-3 3',
    text: '修改了个人资料',
    time: '2小时前'
  },
  {
    id: 2,
    icon: 'M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z',
    text: '登录了系统',
    time: '3小时前'
  },
  {
    id: 3,
    icon: 'M9 19c-5 0-9-4-9-9s4-9 9-9 9 4 9 9-4 9-9 9zM21 3l-3 3',
    text: '修改了系统设置',
    time: '1天前'
  },
  {
    id: 4,
    icon: 'M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z',
    text: '查看了设备状态',
    time: '2天前'
  }
])

// 方法
function saveProfile() {
  console.log('保存个人资料:', profileForm)
  hasChanges.value = false
  alert('个人资料已保存')
}

function toggleTwoFactor() {
  profileForm.twoFactorEnabled = !profileForm.twoFactorEnabled
  hasChanges.value = true
}

function toggleLoginNotifications() {
  profileForm.loginNotifications = !profileForm.loginNotifications
  hasChanges.value = true
}

function closeChangePasswordModal() {
  showChangePasswordModal.value = false
  passwordForm.currentPassword = ''
  passwordForm.newPassword = ''
  passwordForm.confirmPassword = ''
}

function changePassword() {
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    alert('新密码和确认密码不匹配')
    return
  }
  
  if (passwordForm.newPassword.length < 8) {
    alert('新密码长度至少8个字符')
    return
  }
  
  console.log('修改密码')
  alert('密码修改成功')
  closeChangePasswordModal()
}

function formatDate(date: Date) {
  return date.toLocaleDateString() + ' ' + date.toLocaleTimeString()
}

// 生命周期
onMounted(() => {
  // 初始化数据
  console.log('初始化个人资料页面')
})
</script>

<style scoped>
.profile-container {
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

.save-btn {
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

.save-btn:hover {
  background: #0099cc;
  transform: translateY(-1px);
}

.save-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
  transform: none;
}

.profile-content {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 24px;
}

.profile-main {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.profile-section {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
}

.section-header {
  padding: 24px;
  background: #f8f9fa;
  border-bottom: 1px solid #e9ecef;
}

.section-header h2 {
  font-size: 20px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 8px;
}

.section-header p {
  font-size: 14px;
  color: #6c757d;
  margin: 0;
}

.profile-form {
  padding: 24px;
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

.security-settings,
.preference-settings {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.security-item,
.preference-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: #f8f9fa;
  border-radius: 8px;
  border: 1px solid #e9ecef;
}

.security-info,
.preference-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.security-info h3,
.preference-info h3 {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.security-info p,
.preference-info p {
  font-size: 14px;
  color: #6c757d;
  margin: 0;
}

.security-status {
  font-size: 12px;
  font-weight: 500;
  padding: 2px 8px;
  border-radius: 12px;
  background: rgba(231, 76, 60, 0.1);
  color: #e74c3c;
}

.security-status.enabled {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.security-btn {
  padding: 8px 16px;
  background: #00d4ff;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.security-btn:hover {
  background: #0099cc;
}

.theme-options {
  display: flex;
  gap: 16px;
}

.theme-option {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}

.theme-option input {
  display: none;
}

.theme-preview {
  width: 40px;
  height: 24px;
  border-radius: 12px;
  border: 2px solid #e9ecef;
  transition: all 0.3s ease;
}

.theme-preview.light {
  background: linear-gradient(90deg, #ffffff 0%, #f8f9fa 100%);
}

.theme-preview.dark {
  background: linear-gradient(90deg, #2c3e50 0%, #34495e 100%);
}

.theme-preview.auto {
  background: linear-gradient(90deg, #ffffff 0%, #2c3e50 100%);
}

.theme-option input:checked + .theme-preview {
  border-color: #00d4ff;
  box-shadow: 0 0 0 2px rgba(0, 212, 255, 0.2);
}

.profile-sidebar {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.user-card,
.stats-card,
.activity-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  padding: 24px;
}

.user-card {
  text-align: center;
}

.user-avatar-large {
  width: 80px;
  height: 80px;
  background: #00d4ff;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  margin: 0 auto 16px;
}

.user-info h3 {
  font-size: 20px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 8px;
}

.user-role,
.user-email {
  font-size: 14px;
  color: #6c757d;
  margin: 0 0 4px;
}

.stats-card h4,
.activity-card h4 {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 16px;
}

.stats-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid #f1f3f4;
}

.stat-label {
  font-size: 14px;
  color: #6c757d;
}

.stat-value {
  font-size: 14px;
  font-weight: 500;
  color: #2c3e50;
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.activity-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 0;
}

.activity-icon {
  width: 32px;
  height: 32px;
  background: #f8f9fa;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #6c757d;
}

.activity-content {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.activity-text {
  font-size: 14px;
  color: #2c3e50;
}

.activity-time {
  font-size: 12px;
  color: #6c757d;
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

.password-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.password-requirements {
  background: #f8f9fa;
  border-radius: 6px;
  padding: 16px;
}

.password-requirements h4 {
  font-size: 14px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 8px;
}

.password-requirements ul {
  margin: 0;
  padding-left: 20px;
}

.password-requirements li {
  font-size: 12px;
  color: #6c757d;
  margin-bottom: 4px;
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
  .profile-content {
    grid-template-columns: 1fr;
  }
  
  .form-row {
    grid-template-columns: 1fr;
  }
  
  .theme-options {
    flex-direction: column;
  }
}
</style>
