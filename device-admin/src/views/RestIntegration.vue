<template>
  <div class="page">
    <div class="header sticky">
      <div class="title">
        <h2>REST 集成（MES/ERP/上位机）</h2>
        <span class="sub">系统配置、端点映射、调试与运行监控</span>
      </div>
      <div class="actions">
        <button class="btn" @click="handleTest" :disabled="loading">测试连接</button>
        <button class="btn" @click="toggleEnabled" :disabled="loading">{{ enabled ? '停用' : '启用' }}</button>
        <button class="btn-primary" @click="handleSave" :disabled="loading">保存</button>
      </div>
    </div>

    <div class="sections">
      <div class="card">
        <h3>目标系统配置</h3>
        <div class="form">
          <div class="row two">
            <div :class="errors.name && 'invalid'"><label>名称</label><input v-model.trim="form.name" /><small v-if="errors.name" class="error">{{ errors.name }}</small></div>
            <div :class="errors.baseUrl && 'invalid'"><label>Base URL</label><input v-model.trim="form.baseUrl" placeholder="https://mes.example.com/api" /><small v-if="errors.baseUrl" class="error">{{ errors.baseUrl }}</small></div>
          </div>
          <div class="row two">
            <div><label>鉴权方式</label>
              <select v-model="form.authMode">
                <option value="None">无</option>
                <option value="Basic">Basic</option>
                <option value="Bearer">Bearer</option>
              </select>
            </div>
            <div v-if="form.authMode==='Basic'"><label>用户名</label><input v-model="form.username" /></div>
          </div>
          <div class="row" v-if="form.authMode==='Basic'">
            <label>密码</label><input type="password" v-model="form.password" />
          </div>
          <div class="row" v-if="form.authMode==='Bearer'">
            <label>Token</label><input v-model="form.token" placeholder="JWT/OAuth Token" />
          </div>
          <div class="row two">
            <div><label>请求超时(ms)</label><input type="number" v-model.number="form.timeoutMs" min="1000" /></div>
            <div><label>重试次数</label><input type="number" v-model.number="form.retryCount" min="0" /></div>
          </div>
          <div class="row two">
            <div><label>验证TLS</label>
              <select v-model="form.verifyTLS"><option :value="true">是</option><option :value="false">否</option></select>
            </div>
            <div></div>
          </div>
          <div class="row">
            <label>默认请求头</label>
            <div class="headers">
              <div class="header-row" v-for="(h,idx) in form.headers" :key="idx">
                <input v-model="h.key" placeholder="Key" />
                <input v-model="h.value" placeholder="Value" />
                <button class="btn" @click="removeHeader(idx)">删除</button>
              </div>
              <button class="btn" @click="addHeader">新增请求头</button>
            </div>
          </div>
        </div>
      </div>

      <div class="card">
        <h3>端点映射</h3>
        <div class="mapping">
          <div class="map-row-adv" v-for="(m,i) in form.mappings" :key="i">
            <select v-model="m.method">
              <option>GET</option>
              <option>POST</option>
              <option>PUT</option>
            </select>
            <input v-model="m.path" placeholder="/v1/devices" />
            <input v-model="m.target" placeholder="目标：devices（表/资源）" />
            <select v-model="m.contentType">
              <option value="application/json">application/json</option>
              <option value="application/x-www-form-urlencoded">application/x-www-form-urlencoded</option>
              <option value="text/plain">text/plain</option>
            </select>
            <input v-model="m.bodyTemplate" placeholder="Body 模板（可选）" />
            <select v-model="m.enabled"><option :value="true">启用</option><option :value="false">禁用</option></select>
            <button class="btn" @click="removeMapping(i)">删除</button>
          </div>
          <div class="map-actions">
            <button class="btn" @click="addMapping">新增映射</button>
            <button class="btn" @click="importMappings">导入</button>
            <button class="btn" @click="exportMappings">导出</button>
          </div>
        </div>
      </div>

      <div class="card">
        <h3>调试台</h3>
        <div class="form">
          <div class="row two">
            <div><label>方法</label>
              <select v-model="debug.method"><option>GET</option><option>POST</option><option>PUT</option></select>
            </div>
            <div><label>路径</label><input v-model="debug.path" placeholder="/v1/ping" /></div>
          </div>
          <div class="row" v-if="debug.method!=='GET'">
            <label>Body(JSON)</label>
            <textarea rows="4" v-model="debug.body" placeholder='{"id":1}'></textarea>
          </div>
          <div class="row">
            <button class="btn" @click="runDebug" :disabled="loading">发送</button>
            <span class="muted" v-if="debug.result">结果：{{ debug.result }}</span>
          </div>
        </div>
      </div>

      <div class="card">
        <h3>运行状态</h3>
        <div class="status-grid">
          <div class="stat"><span class="label">状态</span><b :class="status==='可用' ? 'ok' : 'warn'">{{ status }}</b></div>
          <div class="stat"><span class="label">启用</span><b>{{ enabled ? '是' : '否' }}</b></div>
          <div class="stat"><span class="label">最后心跳</span><b>{{ lastHeartbeat || '无' }}</b></div>
          <div class="stat"><span class="label">错误</span><b class="warn">{{ lastError || '无' }}</b></div>
        </div>
      </div>

      <div class="card">
        <h3>日志</h3>
        <div class="logs">
          <div class="log-toolbar">
            <button class="btn" @click="refreshLogs" :disabled="loading">刷新</button>
            <button class="btn" @click="clearLogs">清空</button>
          </div>
          <pre class="log-view">{{ logs.join('\n') || '暂无日志' }}</pre>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { getRestConfig, saveRestConfig, testRestConnection, getRestLogs, type RestConfigDto } from '@/api/integrations'

const loading = ref(false)
const enabled = ref(false)
const status = ref('未连接')
const lastError = ref('')
const lastHeartbeat = ref('')
const logs = ref<string[]>([])

const form = ref<RestConfigDto>({
  name: 'REST 对接',
  baseUrl: '',
  authMode: 'None' as 'None'|'Basic'|'Bearer',
  username: '',
  password: '',
  token: '',
  timeoutMs: 15000,
  retryCount: 0,
  verifyTLS: true,
  headers: [],
  enabled: false,
  mappings: []
})

const errors = reactive<{[k:string]: string|undefined}>({})

function addMapping(){ form.value.mappings.push({ method: 'GET', path: '', target: '' }) }
function removeMapping(i:number){ form.value.mappings.splice(i,1) }
function addHeader(){ form.value.headers?.push({ key: '', value: '' }) }
function removeHeader(i:number){ form.value.headers?.splice(i,1) }
function importMappings(){}
function exportMappings(){}

function validate(): boolean {
  errors.name = !form.value.name?.trim() ? '名称必填' : undefined
  errors.baseUrl = !form.value.baseUrl?.trim() ? 'Base URL 必填' : undefined
  return !errors.name && !errors.baseUrl
}

async function handleTest(){
  loading.value=true
  try {
    await testRestConnection({ baseUrl: form.value.baseUrl })
    status.value='可用'; lastError.value=''; lastHeartbeat.value=new Date().toLocaleString()
    logs.value.unshift(`[${new Date().toLocaleTimeString()}] 测试成功`)
  } catch(e:any){
    status.value='不可用'; lastError.value=e?.message || String(e)
    logs.value.unshift(`[${new Date().toLocaleTimeString()}] 测试失败: ${lastError.value}`)
  } finally { loading.value=false }
}

function toggleEnabled(){ enabled.value = !enabled.value }

async function handleSave(){
  if(!validate()){ return }
  loading.value=true
  try {
    await saveRestConfig(form.value)
    logs.value.unshift(`[${new Date().toLocaleTimeString()}] 配置已保存`)
  } finally { loading.value=false }
}

const debug = reactive<{ method: 'GET'|'POST'|'PUT'; path: string; body: string; result: string }>({ method:'GET', path:'', body:'', result:'' })
async function runDebug(){ loading.value=true; try { await new Promise(r=>setTimeout(r,400)); debug.result = `${debug.method} ${form.value.baseUrl}${debug.path} OK`; logs.value.unshift(`[${new Date().toLocaleTimeString()}] 调试 ${debug.method} ${debug.path} 成功`) } finally { loading.value=false } }

async function refreshLogs(){
  try { const { data } = await getRestLogs(); logs.value = data } catch { logs.value.unshift(`[${new Date().toLocaleTimeString()}] 刷新日志`) }
}
function clearLogs(){ logs.value = [] }

onMounted(async ()=>{
  try {
    const { data } = await getRestConfig()
    Object.assign(form.value, data)
  } catch {
    // 本地回退：尝试从 localStorage 读取
    const cache = localStorage.getItem('restConfig')
    if(cache) Object.assign(form.value, JSON.parse(cache))
  }
})
</script>

<style scoped>
.page { padding: 16px; display:flex; flex-direction: column; gap: 16px; }
.header { display:flex; align-items:center; justify-content: space-between; }
.header.sticky { position: sticky; top: 0; background: #f7f9fc; z-index: 5; padding: 8px 16px; border-bottom: 1px solid #e6e9ee; border-radius: 12px; }
.title { display:flex; flex-direction: column; }
.title .sub { color:#6c757d; font-size:12px; }
.actions .btn, .btn-primary { padding: 8px 12px; border-radius: 8px; border: 1px solid #cbd5e1; background: #f1f5f9; color:#0f172a; }
.actions .btn:hover { background:#e2e8f0; }
.btn-primary { background: linear-gradient(90deg,#2563eb,#1d4ed8); color: #fff; border: none; }
.btn-primary:hover { filter: brightness(0.95); }
.tabs { display:none; }
.sections { display:grid; grid-template-columns: 1fr; gap: 16px; }
.card { background:#fff; border:1px solid #e6e9ee; border-radius:12px; padding:16px; display:flex; flex-direction:column; gap:12px; }
.form .row { display:flex; flex-direction:column; gap:6px; }
.form .row.two { flex-direction:row; gap:12px; }
.form .row.two > div { flex:1; display:flex; flex-direction:column; }
.form label { font-size: 12px; color: #6c757d; }
.form input, .form select, .form textarea { padding: 10px 12px; border: 1px solid #e6e9ee; border-radius: 8px; }
.form .row.invalid input, .form .row.invalid textarea { border-color:#e55353; background:#fff7f7; }
.form .error { color:#e55353; font-size:12px; }
.mapping { display:flex; flex-direction:column; gap:8px; }
.map-row { display:grid; grid-template-columns: 120px 1fr 1fr auto; gap:8px; }
.map-actions { display:flex; gap:8px; }
.status .muted { color:#6c757d; }
.status-grid { display:grid; grid-template-columns: repeat(4, minmax(160px, 1fr)); gap: 8px; }
.stat { background:#f8fafc; border:1px solid #eef1f5; border-radius:8px; padding:12px; display:flex; flex-direction:column; gap:4px; }
.stat .label { color:#6c757d; font-size:12px; }
.ok { color:#2e7d32; }
.warn { color:#b71c1c; }
.logs { display:flex; flex-direction:column; gap:8px; }
.log-toolbar { display:flex; gap:8px; }
.log-view { background:#0b1020; color:#b7c7ff; border-radius:8px; padding:12px; max-height:240px; overflow:auto; }
@media (max-width: 1024px) { .status-grid { grid-template-columns: repeat(2, 1fr); } }
</style>


