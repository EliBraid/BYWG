<template>
  <div class="help-container">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-left">
        <h1>帮助中心</h1>
        <p>获取系统使用帮助和技术支持</p>
      </div>
      <div class="header-right">
        <button @click="contactSupport" class="contact-btn">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" stroke="currentColor" stroke-width="2"/>
          </svg>
          联系技术支持
        </button>
      </div>
    </div>

    <!-- 主要内容 -->
    <div class="help-content">
      <!-- 左侧：帮助导航 -->
      <div class="help-sidebar">
        <div class="help-nav">
          <h3>帮助分类</h3>
          <div class="nav-categories">
            <button 
              v-for="category in helpCategories" 
              :key="category.id"
              @click="activeCategory = category.id"
              class="nav-category"
              :class="{ active: activeCategory === category.id }"
            >
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path :d="category.icon" stroke="currentColor" stroke-width="2"/>
              </svg>
              {{ category.name }}
            </button>
          </div>
        </div>

        <div class="quick-links">
          <h3>快速链接</h3>
          <div class="link-list">
            <a href="#" class="help-link">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" stroke="currentColor" stroke-width="2"/>
                <polyline points="14,2 14,8 20,8" stroke="currentColor" stroke-width="2"/>
              </svg>
              用户手册
            </a>
            <a href="#" class="help-link">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
                <path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3" stroke="currentColor" stroke-width="2"/>
                <line x1="12" y1="17" x2="12.01" y2="17" stroke="currentColor" stroke-width="2"/>
              </svg>
              常见问题
            </a>
            <a href="#" class="help-link">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" fill="currentColor"/>
              </svg>
              视频教程
            </a>
            <a href="#" class="help-link">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M9 19c-5 0-9-4-9-9s4-9 9-9 9 4 9 9-4 9-9 9zM21 3l-3 3" stroke="currentColor" stroke-width="2"/>
              </svg>
              系统更新日志
            </a>
          </div>
        </div>
      </div>

      <!-- 右侧：帮助内容 -->
      <div class="help-main">
        <!-- 搜索框 -->
        <div class="search-section">
          <div class="search-box">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <circle cx="11" cy="11" r="8" stroke="currentColor" stroke-width="2"/>
              <path d="M21 21l-4.35-4.35" stroke="currentColor" stroke-width="2"/>
            </svg>
            <input 
              v-model="searchQuery" 
              type="text" 
              placeholder="搜索帮助内容..." 
              class="search-input"
            />
            <button @click="searchHelp" class="search-btn">搜索</button>
          </div>
        </div>

        <!-- 帮助内容 -->
        <div class="help-articles">
          <div v-if="activeCategory === 'getting-started'" class="help-section">
            <h2>快速入门</h2>
            <div class="article-list">
              <div v-for="article in gettingStartedArticles" :key="article.id" class="article-item">
                <div class="article-header">
                  <h3>{{ article.title }}</h3>
                  <span class="article-duration">{{ article.duration }}</span>
                </div>
                <p class="article-description">{{ article.description }}</p>
                <div class="article-tags">
                  <span v-for="tag in article.tags" :key="tag" class="article-tag">{{ tag }}</span>
                </div>
              </div>
            </div>
          </div>

          <div v-if="activeCategory === 'user-guide'" class="help-section">
            <h2>用户指南</h2>
            <div class="article-list">
              <div v-for="article in userGuideArticles" :key="article.id" class="article-item">
                <div class="article-header">
                  <h3>{{ article.title }}</h3>
                  <span class="article-duration">{{ article.duration }}</span>
                </div>
                <p class="article-description">{{ article.description }}</p>
                <div class="article-tags">
                  <span v-for="tag in article.tags" :key="tag" class="article-tag">{{ tag }}</span>
                </div>
              </div>
            </div>
          </div>

          <div v-if="activeCategory === 'troubleshooting'" class="help-section">
            <h2>故障排除</h2>
            <div class="troubleshooting-list">
              <div v-for="issue in troubleshootingIssues" :key="issue.id" class="issue-item">
                <div class="issue-header">
                  <h3>{{ issue.title }}</h3>
                  <span class="issue-severity" :class="issue.severity">{{ issue.severityText }}</span>
                </div>
                <p class="issue-description">{{ issue.description }}</p>
                <div class="issue-solutions">
                  <h4>解决方案：</h4>
                  <ol>
                    <li v-for="solution in issue.solutions" :key="solution">{{ solution }}</li>
                  </ol>
                </div>
              </div>
            </div>
          </div>

          <div v-if="activeCategory === 'api-docs'" class="help-section">
            <h2>API文档</h2>
            <div class="api-docs">
              <div class="api-endpoint">
                <h3>设备管理API</h3>
                <div class="endpoint-list">
                  <div class="endpoint-item">
                    <span class="method get">GET</span>
                    <span class="path">/api/devices</span>
                    <span class="description">获取设备列表</span>
                  </div>
                  <div class="endpoint-item">
                    <span class="method post">POST</span>
                    <span class="path">/api/devices</span>
                    <span class="description">创建设备</span>
                  </div>
                  <div class="endpoint-item">
                    <span class="method put">PUT</span>
                    <span class="path">/api/devices/{id}</span>
                    <span class="description">更新设备</span>
                  </div>
                  <div class="endpoint-item">
                    <span class="method delete">DELETE</span>
                    <span class="path">/api/devices/{id}</span>
                    <span class="description">删除设备</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-if="activeCategory === 'contact'" class="help-section">
            <h2>联系我们</h2>
            <div class="contact-info">
              <div class="contact-methods">
                <div class="contact-method">
                  <div class="contact-icon">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" stroke="currentColor" stroke-width="2"/>
                      <polyline points="22,6 12,13 2,6" stroke="currentColor" stroke-width="2"/>
                    </svg>
                  </div>
                  <div class="contact-details">
                    <h3>邮箱支持</h3>
                    <p>support@bywg.com</p>
                    <p>技术支持：tech@bywg.com</p>
                  </div>
                </div>
                
                <div class="contact-method">
                  <div class="contact-icon">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z" stroke="currentColor" stroke-width="2"/>
                    </svg>
                  </div>
                  <div class="contact-details">
                    <h3>电话支持</h3>
                    <p>400-888-8888</p>
                    <p>工作时间：周一至周五 9:00-18:00</p>
                  </div>
                </div>
                
                <div class="contact-method">
                  <div class="contact-icon">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" stroke="currentColor" stroke-width="2"/>
                    </svg>
                  </div>
                  <div class="contact-details">
                    <h3>在线客服</h3>
                    <p>实时在线支持</p>
                    <p>工作时间：周一至周日 8:00-22:00</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'

// 响应式数据
const activeCategory = ref('getting-started')
const searchQuery = ref('')

// 帮助分类
const helpCategories = ref([
  { id: 'getting-started', name: '快速入门', icon: 'M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z' },
  { id: 'user-guide', name: '用户指南', icon: 'M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z' },
  { id: 'troubleshooting', name: '故障排除', icon: 'M9 19c-5 0-9-4-9-9s4-9 9-9 9 4 9 9-4 9-9 9zM21 3l-3 3' },
  { id: 'api-docs', name: 'API文档', icon: 'M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4' },
  { id: 'contact', name: '联系我们', icon: 'M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z' }
])

// 快速入门文章
const gettingStartedArticles = ref([
  {
    id: 1,
    title: '系统概述',
    description: '了解BYWG工业边缘网关系统的基本架构和核心功能',
    duration: '5分钟',
    tags: ['基础', '架构', '功能']
  },
  {
    id: 2,
    title: '首次登录',
    description: '如何首次登录系统并完成基本配置',
    duration: '3分钟',
    tags: ['登录', '配置', '基础']
  },
  {
    id: 3,
    title: '设备连接',
    description: '如何连接和管理工业设备',
    duration: '10分钟',
    tags: ['设备', '连接', '管理']
  },
  {
    id: 4,
    title: '协议配置',
    description: '配置各种工业通信协议',
    duration: '15分钟',
    tags: ['协议', '配置', '通信']
  }
])

// 用户指南文章
const userGuideArticles = ref([
  {
    id: 1,
    title: '设备管理',
    description: '完整的设备管理功能使用指南',
    duration: '20分钟',
    tags: ['设备', '管理', '指南']
  },
  {
    id: 2,
    title: '实时监控',
    description: '如何使用实时监控功能',
    duration: '15分钟',
    tags: ['监控', '实时', '数据']
  },
  {
    id: 3,
    title: '报警管理',
    description: '配置和管理系统报警',
    duration: '12分钟',
    tags: ['报警', '管理', '配置']
  },
  {
    id: 4,
    title: '用户管理',
    description: '系统用户和权限管理',
    duration: '18分钟',
    tags: ['用户', '权限', '管理']
  }
])

// 故障排除问题
const troubleshootingIssues = ref([
  {
    id: 1,
    title: '设备连接失败',
    description: '设备无法连接到系统',
    severity: 'high',
    severityText: '高优先级',
    solutions: [
      '检查网络连接是否正常',
      '确认设备IP地址和端口配置',
      '检查防火墙设置',
      '验证设备通信协议配置'
    ]
  },
  {
    id: 2,
    title: '数据采集异常',
    description: '设备数据无法正常采集',
    severity: 'medium',
    severityText: '中优先级',
    solutions: [
      '检查设备通信状态',
      '验证数据点配置',
      '检查采集频率设置',
      '查看系统日志信息'
    ]
  },
  {
    id: 3,
    title: '系统性能问题',
    description: '系统运行缓慢或响应延迟',
    severity: 'low',
    severityText: '低优先级',
    solutions: [
      '检查系统资源使用情况',
      '优化数据采集频率',
      '清理历史数据',
      '重启相关服务'
    ]
  }
])

// 方法
function searchHelp() {
  console.log('搜索帮助内容:', searchQuery.value)
  // 实现搜索逻辑
}

function contactSupport() {
  console.log('联系技术支持')
  // 实现联系支持逻辑
}

// 生命周期
onMounted(() => {
  console.log('初始化帮助中心')
})
</script>

<style scoped>
.help-container {
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

.contact-btn {
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

.contact-btn:hover {
  background: #0099cc;
  transform: translateY(-1px);
}

.help-content {
  display: grid;
  grid-template-columns: 250px 1fr;
  gap: 24px;
}

.help-sidebar {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.help-nav,
.quick-links {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  padding: 20px;
}

.help-nav h3,
.quick-links h3 {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 16px;
}

.nav-categories {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.nav-category {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  background: none;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  color: #6c757d;
  cursor: pointer;
  transition: all 0.3s ease;
  text-align: left;
}

.nav-category:hover {
  background: #f8f9fa;
  color: #2c3e50;
}

.nav-category.active {
  background: #00d4ff;
  color: white;
}

.link-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.help-link {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px;
  color: #6c757d;
  text-decoration: none;
  border-radius: 6px;
  transition: all 0.3s ease;
}

.help-link:hover {
  background: #f8f9fa;
  color: #00d4ff;
}

.help-main {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
}

.search-section {
  padding: 24px;
  background: #f8f9fa;
  border-bottom: 1px solid #e9ecef;
}

.search-box {
  display: flex;
  align-items: center;
  gap: 12px;
  background: white;
  border-radius: 8px;
  padding: 8px 16px;
  border: 1px solid #e9ecef;
}

.search-box svg {
  color: #6c757d;
}

.search-input {
  flex: 1;
  border: none;
  outline: none;
  font-size: 14px;
  padding: 8px 0;
}

.search-btn {
  padding: 8px 16px;
  background: #00d4ff;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.search-btn:hover {
  background: #0099cc;
}

.help-articles {
  padding: 24px;
}

.help-section h2 {
  font-size: 24px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 24px;
}

.article-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.article-item {
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
  border: 1px solid #e9ecef;
  transition: all 0.3s ease;
}

.article-item:hover {
  background: #e9ecef;
  transform: translateY(-2px);
}

.article-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.article-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.article-duration {
  font-size: 12px;
  color: #6c757d;
  background: #e9ecef;
  padding: 4px 8px;
  border-radius: 12px;
}

.article-description {
  font-size: 14px;
  color: #6c757d;
  margin: 0 0 12px;
  line-height: 1.5;
}

.article-tags {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.article-tag {
  font-size: 12px;
  color: #00d4ff;
  background: rgba(0, 212, 255, 0.1);
  padding: 4px 8px;
  border-radius: 12px;
}

.troubleshooting-list {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.issue-item {
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
  border: 1px solid #e9ecef;
}

.issue-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.issue-header h3 {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0;
}

.issue-severity {
  font-size: 12px;
  font-weight: 500;
  padding: 4px 8px;
  border-radius: 12px;
}

.issue-severity.high {
  background: rgba(231, 76, 60, 0.1);
  color: #e74c3c;
}

.issue-severity.medium {
  background: rgba(243, 156, 18, 0.1);
  color: #f39c12;
}

.issue-severity.low {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.issue-description {
  font-size: 14px;
  color: #6c757d;
  margin: 0 0 16px;
}

.issue-solutions h4 {
  font-size: 16px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 8px;
}

.issue-solutions ol {
  margin: 0;
  padding-left: 20px;
}

.issue-solutions li {
  font-size: 14px;
  color: #2c3e50;
  margin-bottom: 4px;
}

.api-docs {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.api-endpoint h3 {
  font-size: 20px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 16px;
}

.endpoint-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.endpoint-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 12px 16px;
  background: #f8f9fa;
  border-radius: 6px;
  border: 1px solid #e9ecef;
}

.method {
  font-size: 12px;
  font-weight: 600;
  padding: 4px 8px;
  border-radius: 4px;
  min-width: 50px;
  text-align: center;
}

.method.get {
  background: rgba(39, 174, 96, 0.1);
  color: #27ae60;
}

.method.post {
  background: rgba(52, 152, 219, 0.1);
  color: #3498db;
}

.method.put {
  background: rgba(243, 156, 18, 0.1);
  color: #f39c12;
}

.method.delete {
  background: rgba(231, 76, 60, 0.1);
  color: #e74c3c;
}

.path {
  font-family: monospace;
  font-size: 14px;
  color: #2c3e50;
  font-weight: 500;
}

.description {
  font-size: 14px;
  color: #6c757d;
}

.contact-info {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.contact-methods {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.contact-method {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
  border: 1px solid #e9ecef;
}

.contact-icon {
  width: 48px;
  height: 48px;
  background: #00d4ff;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.contact-details h3 {
  font-size: 18px;
  font-weight: 600;
  color: #2c3e50;
  margin: 0 0 8px;
}

.contact-details p {
  font-size: 14px;
  color: #6c757d;
  margin: 0 0 4px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .help-content {
    grid-template-columns: 1fr;
  }
  
  .contact-method {
    flex-direction: column;
    text-align: center;
  }
}
</style>
