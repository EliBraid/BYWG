<template>
  <div class="login-container">
    <!-- 左侧工业展示区 -->
    <div class="industrial-section">
      <div class="industrial-content">
        <div class="industrial-header">
          <div class="system-logo">
            <div class="logo-icon">
              <svg width="60" height="60" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect x="2" y="3" width="20" height="14" rx="2" stroke="currentColor" stroke-width="2"/>
                <path d="M8 21h8" stroke="currentColor" stroke-width="2"/>
                <path d="M12 17v4" stroke="currentColor" stroke-width="2"/>
                <circle cx="12" cy="10" r="2" fill="currentColor"/>
                <path d="M6 7h12" stroke="currentColor" stroke-width="1"/>
                <path d="M6 11h12" stroke="currentColor" stroke-width="1"/>
              </svg>
            </div>
            <div class="system-info">
              <h1>BYWG</h1>
              <p>工业边缘网关控制系统</p>
              <div class="system-status">
                <div class="status-indicator online"></div>
                <span>系统在线</span>
              </div>
            </div>
          </div>
        </div>
        
        <div class="industrial-metrics">
          <div class="metric-card">
            <div class="metric-header">
              <div class="metric-icon">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M9 19c-5 0-9-4-9-9s4-9 9-9 9 4 9 9-4 9-9 9zM21 3l-3 3" stroke="currentColor" stroke-width="2"/>
                </svg>
              </div>
              <span>系统状态</span>
            </div>
            <div class="metric-value">正常运行</div>
            <div class="metric-detail">运行时间: 15天 8小时</div>
          </div>
          
          <div class="metric-card">
            <div class="metric-header">
              <div class="metric-icon">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <rect x="2" y="3" width="20" height="14" rx="2" stroke="currentColor" stroke-width="2"/>
                  <path d="M8 21h8" stroke="currentColor" stroke-width="2"/>
                </svg>
              </div>
              <span>设备连接</span>
            </div>
            <div class="metric-value">45台</div>
            <div class="metric-detail">在线: 42台 | 离线: 3台</div>
          </div>
          
          <div class="metric-card">
            <div class="metric-header">
              <div class="metric-icon">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
                </svg>
              </div>
              <span>协议支持</span>
            </div>
            <div class="metric-value">8种</div>
            <div class="metric-detail">Modbus, OPC UA, Ethernet/IP</div>
          </div>
        </div>
        
        <div class="industrial-footer">
          <div class="footer-info">
            <p>© 2024 BYWG Industrial Gateway System</p>
            <p>Version 2.1.0 | Build 2024.12.15</p>
          </div>
        </div>
      </div>
      
      <!-- 工业背景装饰 -->
      <div class="industrial-decoration">
        <div class="tech-grid"></div>
        <div class="circuit-pattern"></div>
        <div class="data-flow"></div>
      </div>
    </div>
    
    <!-- 右侧登录表单区 -->
    <div class="login-section">
      <div class="login-content">
        <div class="login-header">
          <div class="header-icon">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z" fill="currentColor"/>
            </svg>
          </div>
          <h2>系统登录</h2>
          <p>请输入您的凭据以访问控制系统</p>
        </div>

        <form @submit.prevent="handleLogin" class="login-form">
          <div class="form-group">
            <div class="input-wrapper">
              <div class="input-icon">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z" fill="currentColor"/>
                </svg>
              </div>
              <input
                id="username"
                v-model="loginForm.username"
                type="text"
                class="form-input"
                placeholder="用户名"
                required
                autocomplete="username"
              />
            </div>
          </div>

          <div class="form-group">
            <div class="input-wrapper">
              <div class="input-icon">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M18 8h-1V6c0-2.76-2.24-5-5-5S7 3.24 7 6v2H6c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V10c0-1.1-.9-2-2-2zm-6 9c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2zm3.1-9H8.9V6c0-1.71 1.39-3.1 3.1-3.1 1.71 0 3.1 1.39 3.1 3.1v2z" fill="currentColor"/>
                </svg>
              </div>
              <input
                id="password"
                v-model="loginForm.password"
                :type="showPassword ? 'text' : 'password'"
                class="form-input"
                placeholder="密码"
                required
                autocomplete="current-password"
              />
              <button
                type="button"
                class="password-toggle"
                @click="togglePassword"
                :title="showPassword ? '隐藏密码' : '显示密码'"
              >
                <svg v-if="showPassword" width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z" fill="currentColor"/>
                </svg>
                <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M12 7c2.76 0 5 2.24 5 5 0 .65-.13 1.26-.36 1.83l2.92 2.92c1.51-1.26 2.7-2.89 3.43-4.75-1.73-4.39-6-7.5-11-7.5-1.4 0-2.74.25-3.98.7l2.16 2.16C10.74 7.13 11.35 7 12 7zM2 4.27l2.28 2.28.46.46C3.08 8.3 1.78 10.02 1 12c1.73 4.39 6 7.5 11 7.5 1.55 0 3.03-.3 4.38-.84l.42.42L19.73 22 21 20.73 3.27 3 2 4.27zM7.53 9.8l1.55 1.55c-.05.21-.08.43-.08.65 0 1.66 1.34 3 3 3 .22 0 .44-.03.65-.08l1.55 1.55c-.67.33-1.41.53-2.2.53-2.76 0-5-2.24-5-5 0-.79.2-1.53.53-2.2zm4.31-.78l3.15 3.15.02-.16c0-1.66-1.34-3-3-3l-.17.01z" fill="currentColor"/>
                </svg>
              </button>
            </div>
          </div>

          <div class="form-options">
            <label class="checkbox-label">
              <input
                v-model="loginForm.rememberMe"
                type="checkbox"
                class="checkbox-input"
              />
              <span class="checkbox-text">记住我</span>
            </label>
            <a href="#" class="forgot-password">忘记密码？</a>
          </div>

          <button
            type="submit"
            class="login-button"
            :disabled="isLoading"
          >
            <svg v-if="isLoading" width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" class="loading-spinner">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
            </svg>
            <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M11 7L9.6 8.4l2.6 2.6H2v2h10.2l-2.6 2.6L11 17l5-5-5-5zm9 12h-8v2h8c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2h-8v2h8v14z" fill="currentColor"/>
            </svg>
            {{ isLoading ? '登录中...' : '登录' }}
          </button>
        </form>
        
        <div class="login-tips">
          <div class="tip-header">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z" fill="currentColor"/>
            </svg>
            <span>系统凭据</span>
          </div>
          <div class="tip-content">
            <div class="credential-item">
              <span class="label">用户名:</span>
              <span class="value">admin</span>
            </div>
            <div class="credential-item">
              <span class="label">密码:</span>
              <span class="value">123456</span>
            </div>
          </div>
        </div>
        
        <!-- 调试按钮 -->
        <div class="debug-section">
          <button @click="testRoute" class="debug-button">测试路由跳转</button>
          <button @click="fillCredentials" class="debug-button">自动填充凭据</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '../api/auth'

const router = useRouter()

// 登录表单数据
const loginForm = reactive({
  username: '',
  password: '',
  rememberMe: false
})

// 状态管理
const showPassword = ref(false)
const isLoading = ref(false)

// 切换密码显示
function togglePassword() {
  showPassword.value = !showPassword.value
}

// 测试路由跳转
async function testRoute() {
  console.log('测试路由跳转到 /dashboard')
  try {
    await router.push('/dashboard')
    console.log('路由跳转成功')
  } catch (error) {
    console.error('路由跳转失败:', error)
    alert('路由跳转失败: ' + error.message)
  }
}

// 自动填充凭据
function fillCredentials() {
  loginForm.username = 'admin'
  loginForm.password = '123456'
  console.log('已自动填充凭据')
}

// 处理登录
async function handleLogin() {
  if (isLoading.value) return
  
  isLoading.value = true
  
  try {
    // 调用BYWG.Auth登录API
    const response = await login({
      Username: loginForm.username,
      Password: loginForm.password
    })
    
    // 保存认证信息
    localStorage.setItem('token', response.token)
    localStorage.setItem('userInfo', JSON.stringify(response.user))
    localStorage.setItem('isLoggedIn', 'true')
    
    console.log('登录成功:', response.user)
    
    // 跳转到主界面并强制刷新页面以确保用户信息更新
    window.location.href = '/dashboard'
    
  } catch (error: unknown) {
    console.error('登录失败:', error)
    const errorMessage = error instanceof Error ? error.message : '登录失败，请检查用户名和密码'
    alert(errorMessage)
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.login-container {
  display: flex;
  width: 100vw;
  height: 100vh;
  background: linear-gradient(135deg, #1a1a1a 0%, #2d2d2d 50%, #1a1a1a 100%);
  overflow: hidden;
  font-family: 'Segoe UI', 'Roboto', 'Arial', sans-serif;
}

/* 左侧工业展示区 */
.industrial-section {
  flex: 1;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #0f1419 0%, #1a2332 50%, #0f1419 100%);
  overflow: hidden;
  border-right: 2px solid #333;
}

.industrial-content {
  position: relative;
  z-index: 2;
  color: #e0e0e0;
  max-width: 500px;
  padding: 40px;
}

.industrial-header {
  margin-bottom: 40px;
}

.system-logo {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 30px;
}

.logo-icon {
  width: 60px;
  height: 60px;
  background: linear-gradient(135deg, #00d4ff 0%, #0099cc 100%);
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  box-shadow: 0 4px 12px rgba(0, 212, 255, 0.3);
  border: 1px solid #00d4ff;
}

.system-info h1 {
  font-size: 36px;
  font-weight: 700;
  margin: 0 0 8px;
  color: #ffffff;
  letter-spacing: 2px;
}

.system-info p {
  font-size: 16px;
  margin: 0 0 12px;
  color: #b0b0b0;
  font-weight: 400;
}

.system-status {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #00ff88;
}

.status-indicator {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #00ff88;
  box-shadow: 0 0 8px rgba(0, 255, 136, 0.5);
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.industrial-metrics {
  margin-bottom: 40px;
}

.metric-card {
  background: rgba(0, 0, 0, 0.3);
  border: 1px solid #333;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 16px;
  backdrop-filter: blur(10px);
  transition: all 0.3s ease;
}

.metric-card:hover {
  border-color: #00d4ff;
  box-shadow: 0 0 20px rgba(0, 212, 255, 0.2);
  transform: translateY(-2px);
}

.metric-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
  font-size: 14px;
  color: #888;
  font-weight: 500;
}

.metric-icon {
  width: 20px;
  height: 20px;
  color: #00d4ff;
  display: flex;
  align-items: center;
  justify-content: center;
}

.metric-value {
  font-size: 24px;
  font-weight: 700;
  color: #ffffff;
  margin-bottom: 8px;
}

.metric-detail {
  font-size: 12px;
  color: #999;
  line-height: 1.4;
}

.industrial-footer {
  border-top: 1px solid #333;
  padding-top: 20px;
}

.footer-info p {
  margin: 4px 0;
  font-size: 12px;
  color: #666;
  font-family: 'Courier New', monospace;
}

/* 工业背景装饰 */
.industrial-decoration {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 1;
  opacity: 0.1;
}

.tech-grid {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-image: 
    linear-gradient(rgba(0, 212, 255, 0.1) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0, 212, 255, 0.1) 1px, transparent 1px);
  background-size: 40px 40px;
}

.circuit-pattern {
  position: absolute;
  top: 20%;
  right: 10%;
  width: 100px;
  height: 100px;
  background: linear-gradient(45deg, transparent 30%, rgba(0, 212, 255, 0.1) 50%, transparent 70%);
  border-radius: 50%;
  animation: rotate 20s linear infinite;
}

.data-flow {
  position: absolute;
  bottom: 20%;
  left: 10%;
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, transparent 30%, rgba(0, 255, 136, 0.1) 50%, transparent 70%);
  border-radius: 8px;
  animation: pulse 3s ease-in-out infinite;
}

@keyframes rotate {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* 右侧登录表单区 */
.login-section {
  width: 480px;
  background: linear-gradient(135deg, #2a2a2a 0%, #1e1e1e 100%);
  border-left: 2px solid #333;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
}

.login-content {
  width: 100%;
  max-width: 360px;
  padding: 40px;
}

.login-header {
  text-align: center;
  margin-bottom: 40px;
  border-bottom: 1px solid #333;
  padding-bottom: 20px;
}

.header-icon {
  width: 32px;
  height: 32px;
  background: linear-gradient(135deg, #00d4ff 0%, #0099cc 100%);
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
  color: white;
  box-shadow: 0 4px 12px rgba(0, 212, 255, 0.3);
}

.login-header h2 {
  font-size: 24px;
  font-weight: 600;
  color: #ffffff;
  margin: 0 0 8px;
  letter-spacing: 1px;
}

.login-header p {
  font-size: 14px;
  color: #999;
  margin: 0;
  font-weight: 400;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 16px;
  z-index: 2;
  color: #6c757d;
  display: flex;
  align-items: center;
  justify-content: center;
}

.form-input {
  width: 100%;
  padding: 16px 16px 16px 48px;
  border: 1px solid #444;
  border-radius: 6px;
  font-size: 14px;
  background: rgba(0, 0, 0, 0.3);
  color: #ffffff;
  transition: all 0.3s ease;
  box-sizing: border-box;
  font-family: 'Courier New', monospace;
}

.form-input:focus {
  outline: none;
  border-color: #00d4ff;
  background: rgba(0, 0, 0, 0.5);
  box-shadow: 0 0 0 2px rgba(0, 212, 255, 0.2);
}

.form-input::placeholder {
  color: #666;
  font-weight: 400;
}

.password-toggle {
  position: absolute;
  right: 16px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #666;
  cursor: pointer;
  padding: 8px;
  border-radius: 4px;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2;
}

.password-toggle:hover {
  color: #00d4ff;
  background: rgba(0, 212, 255, 0.1);
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 8px 0;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-size: 12px;
  color: #999;
}

.checkbox-input {
  width: 16px;
  height: 16px;
  accent-color: #00d4ff;
  cursor: pointer;
}

.checkbox-text {
  font-weight: 400;
}

.forgot-password {
  color: #00d4ff;
  text-decoration: none;
  font-size: 12px;
  font-weight: 400;
  transition: color 0.3s ease;
}

.forgot-password:hover {
  color: #00ff88;
  text-decoration: underline;
}

.login-button {
  width: 100%;
  padding: 14px 24px;
  background: linear-gradient(135deg, #00d4ff 0%, #0099cc 100%);
  color: white;
  border: 1px solid #00d4ff;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin-top: 8px;
  box-shadow: 0 4px 12px rgba(0, 212, 255, 0.3);
  font-family: 'Segoe UI', sans-serif;
  letter-spacing: 0.5px;
}

.login-button:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 6px 16px rgba(0, 212, 255, 0.4);
  background: linear-gradient(135deg, #00ff88 0%, #00d4ff 100%);
}

.login-button:active:not(:disabled) {
  transform: translateY(0);
}

.login-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.loading-spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.login-tips {
  margin-top: 24px;
  padding: 16px;
  background: rgba(0, 0, 0, 0.3);
  border-radius: 6px;
  border: 1px solid #333;
}

.tip-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
  font-size: 12px;
  color: #00d4ff;
  font-weight: 600;
}

.tip-content {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.credential-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  font-family: 'Courier New', monospace;
}

.credential-item .label {
  color: #999;
}

.credential-item .value {
  color: #00ff88;
  font-weight: 600;
}

.debug-section {
  margin-top: 20px;
  padding: 16px;
  background: rgba(255, 0, 0, 0.1);
  border: 1px solid #ff0000;
  border-radius: 6px;
  display: flex;
  gap: 12px;
}

.debug-button {
  padding: 8px 16px;
  background: #ff4444;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.debug-button:hover {
  background: #ff6666;
}

.error-message {
  background: rgba(231, 76, 60, 0.1);
  color: #e74c3c;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  text-align: center;
  margin-bottom: 20px;
  border: 1px solid rgba(231, 76, 60, 0.2);
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .industrial-section {
    display: none;
  }
  
  .login-section {
    width: 100%;
    border-left: none;
  }
}

@media (max-width: 480px) {
  .login-content {
    padding: 32px 24px;
  }
  
  .login-header h2 {
    font-size: 20px;
  }
  
  .form-input {
    padding: 14px 14px 14px 44px;
    font-size: 14px;
  }
  
  .login-button {
    padding: 12px 20px;
    font-size: 13px;
  }
  
  .industrial-content {
    padding: 20px;
  }
  
  .system-logo {
    flex-direction: column;
    text-align: center;
    gap: 12px;
  }
  
  .metric-card {
    padding: 16px;
  }
}
</style>
