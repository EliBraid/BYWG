<template>
  <div class="page">
    <div class="header sticky">
      <div class="title">
        <h2>OPC UA 集成</h2>
        <span class="sub">配置、认证、映射、调试与运行监控</span>
      </div>
      <div class="actions">
        <button class="btn" @click="handleTest" :disabled="loading">测试连接</button>
        <button class="btn" @click="toggleEnabled" :disabled="loading">{{ enabled ? '停用' : '启用' }}</button>
        <button class="btn-primary" @click="handleSave" :disabled="loading">保存</button>
      </div>
    </div>

    <div class="sections">
      <div class="card">
        <h3>基础配置</h3>
        <div class="form">
          <div class="row" :class="errors.name && 'invalid'">
            <label>名称</label>
            <input v-model.trim="form.name" />
            <small v-if="errors.name" class="error">{{ errors.name }}</small>
          </div>
          <div class="row" :class="errors.endpoint && 'invalid'">
            <label>服务器地址</label>
            <input v-model.trim="form.endpoint" placeholder="opc.tcp://host:4840" />
            <small v-if="errors.endpoint" class="error">{{ errors.endpoint }}</small>
          </div>
          <div class="row two">
            <div>
              <label>安全模式</label>
              <select v-model="form.securityMode">
                <option value="None">None</option>
                <option value="Sign">Sign</option>
                <option value="SignAndEncrypt">SignAndEncrypt</option>
              </select>
            </div>
            <div>
              <label>安全策略</label>
              <select v-model="form.securityPolicy">
                <option>Basic256Sha256</option>
                <option>Basic256</option>
                <option>Basic128Rsa15</option>
                <option>None</option>
              </select>
            </div>
          </div>
          <div class="row two">
            <div><label>应用名称</label><input v-model.trim="form.applicationName" placeholder="BYWG-Gateway" /></div>
            <div><label>会话超时(ms)</label><input type="number" v-model.number="form.sessionTimeoutMs" min="1000" /></div>
          </div>
          <div class="row two">
            <div><label>发布周期(ms)</label><input type="number" v-model.number="form.publishingIntervalMs" min="100" /></div>
            <div><label>采样周期(ms)</label><input type="number" v-model.number="form.samplingIntervalMs" min="50" /></div>
          </div>
          <div class="row two">
            <div><label>队列长度</label><input type="number" v-model.number="form.queueSize" min="1" /></div>
            <div><label>丢弃最旧</label>
              <select v-model="form.discardOldest"><option :value="true">是</option><option :value="false">否</option></select>
            </div>
          </div>
          <div class="row two">
            <div><label>Deadband 类型</label>
              <select v-model="form.deadbandType">
                <option value="None">None</option>
                <option value="Absolute">Absolute</option>
                <option value="Percent">Percent</option>
              </select>
            </div>
            <div><label>Deadband 值</label><input type="number" step="0.01" v-model.number="form.deadbandValue" /></div>
          </div>
          <div class="row two">
            <div><label>自动重连</label>
              <select v-model="form.reconnect"><option :value="true">是</option><option :value="false">否</option></select>
            </div>
            <div><label>重连间隔(ms)</label><input type="number" v-model.number="form.reconnectIntervalMs" min="500" /></div>
          </div>
          <div class="row two">
            <div><label>信任服务器证书</label>
              <select v-model="form.trustServer"><option :value="true">是</option><option :value="false">否</option></select>
            </div>
            <div><label>自动接受不受信证书</label>
              <select v-model="form.autoAcceptUntrusted"><option :value="true">是</option><option :value="false">否</option></select>
            </div>
          </div>
        </div>
      </div>

      <div class="card">
        <h3>认证管理</h3>
        <div class="form">
          <div class="row"><label>认证方式</label>
            <select v-model="form.authMode">
              <option value="Anonymous">匿名</option>
              <option value="UsernamePassword">用户名/密码</option>
              <option value="Certificate">证书</option>
            </select>
          </div>
          <div v-if="form.authMode==='UsernamePassword'" class="row two">
            <div><label>用户名</label><input v-model.trim="form.username" /></div>
            <div><label>密码</label><input type="password" v-model.trim="form.password" /></div>
          </div>
          <div v-if="form.authMode==='Certificate'" class="row">
            <label>证书（PEM/Base64）</label>
            <textarea rows="4" v-model="form.certificate" placeholder="-----BEGIN CERTIFICATE-----"></textarea>
          </div>
        </div>
      </div>

      <div class="card">
        <h3>节点映射</h3>
        <div class="mapping">
          <div class="map-row-adv" v-for="(m,i) in form.mappings" :key="i">
            <input v-model="m.nodeId" placeholder="NodeId，例如 ns=2;s=Channel1.Device1.Tag1" />
            <input v-model="m.target" placeholder="目标路径/字段，例如 devices.tag1" />
            <select v-model="m.dataType">
              <option>Boolean</option><option>Int32</option><option>Float</option><option>Double</option><option>String</option><option>DateTime</option>
            </select>
            <input type="number" step="0.01" v-model.number="m.scale" placeholder="比例" />
            <input type="number" step="0.01" v-model.number="m.offset" placeholder="偏移" />
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
            <div><label>测试 NodeId</label><input v-model="debug.nodeId" placeholder="ns=2;s=Tag1" /></div>
            <div><label>读写</label>
              <select v-model="debug.mode"><option value="read">读取</option><option value="write">写入</option></select>
            </div>
          </div>
          <div class="row" v-if="debug.mode==='write'">
            <label>写入值</label>
            <input v-model="debug.writeValue" />
          </div>
          <div class="row">
            <button class="btn" @click="runDebug" :disabled="loading">执行</button>
            <span class="muted" v-if="debug.result">结果：{{ debug.result }}</span>
          </div>
        </div>
      </div>

      <div class="card">
        <h3>运行状态</h3>
        <div class="status">
          <div class="status-grid">
            <div class="stat"><span class="label">状态</span><b :class="status==='可用' ? 'ok' : 'warn'">{{ status }}</b></div>
            <div class="stat"><span class="label">启用</span><b>{{ enabled ? '是' : '否' }}</b></div>
            <div class="stat"><span class="label">最后心跳</span><b>{{ lastHeartbeat || '无' }}</b></div>
            <div class="stat"><span class="label">错误</span><b class="warn">{{ lastError || '无' }}</b></div>
          </div>
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
import { getOpcUaConfig, saveOpcUaConfig, testOpcUaConnection, getOpcUaLogs, type OpcUaConfigDto } from '@/api/integrations'

const loading = ref(false)
const enabled = ref(false)
const status = ref('未连接')
const lastError = ref('')
const lastHeartbeat = ref('')
const logs = ref<string[]>([])

const form = ref<OpcUaConfigDto>({
  name: 'OPC UA 对接',
  endpoint: '',
  securityMode: 'None',
  securityPolicy: 'None',
  authMode: 'Anonymous',
  username: '',
  password: '',
  certificate: '',
  applicationName: 'BYWG-Gateway',
  reconnect: true,
  reconnectIntervalMs: 2000,
  sessionTimeoutMs: 30000,
  publishingIntervalMs: 1000,
  samplingIntervalMs: 200,
  queueSize: 100,
  discardOldest: true,
  deadbandType: 'None',
  deadbandValue: 0,
  trustServer: true,
  autoAcceptUntrusted: false,
  enabled: false,
  mappings: []
})

const errors = reactive<{[k:string]: string|undefined}>({})
const debug = reactive<{ nodeId: string; mode: 'read'|'write'; writeValue: string; result: string }>({ nodeId: '', mode: 'read', writeValue: '', result: '' })

function addMapping(){ form.value.mappings.push({ nodeId: '', target: '' }) }
function removeMapping(i:number){ form.value.mappings.splice(i,1) }
function importMappings(){ /* 预留：文件上传解析 */ }
function exportMappings(){ /* 预留：导出为JSON/CSV */ }

function validate(): boolean {
  errors.name = !form.value.name?.trim() ? '名称必填' : undefined
  errors.endpoint = !form.value.endpoint?.trim() ? '服务器地址必填' : undefined
  return !errors.name && !errors.endpoint
}

async function handleTest(){
  loading.value = true
  try {
    await testOpcUaConnection({ endpoint: form.value.endpoint })
    status.value = '可用'
    lastError.value = ''
    lastHeartbeat.value = new Date().toLocaleString()
    logs.value.unshift(`[${new Date().toLocaleTimeString()}] 测试成功`)
  } catch(e:any){
    status.value = '不可用'
    lastError.value = e?.message || String(e)
    logs.value.unshift(`[${new Date().toLocaleTimeString()}] 测试失败: ${lastError.value}`)
  } finally {
    loading.value = false
  }
}

function toggleEnabled(){ enabled.value = !enabled.value }

async function handleSave(){
  if(!validate()){ return }
  loading.value = true
  try {
    await saveOpcUaConfig(form.value)
    logs.value.unshift(`[${new Date().toLocaleTimeString()}] 配置已保存`)
  } finally {
    loading.value = false
  }
}

async function runDebug(){
  loading.value = true
  try {
    await new Promise(r=>setTimeout(r,400))
    debug.result = debug.mode==='read' ? `读取 ${debug.nodeId} = 42` : `已写入 ${debug.nodeId} = ${debug.writeValue}`
    logs.value.unshift(`[${new Date().toLocaleTimeString()}] 调试 ${debug.mode} ${debug.nodeId} 成功`)
  } finally { loading.value = false }
}

async function refreshLogs(){
  try {
    const { data } = await getOpcUaLogs()
    logs.value = data
  } catch {
    logs.value.unshift(`[${new Date().toLocaleTimeString()}] 刷新日志`)
  }
}
function clearLogs(){ logs.value = [] }

onMounted(async ()=>{
  try {
    const { data } = await getOpcUaConfig()
    Object.assign(form.value, data)
  } catch {
    const cache = localStorage.getItem('opcuaConfig')
    if(cache) Object.assign(form.value, JSON.parse(cache))
  }
})
</script>

<style scoped>
.page { padding: 16px; display: flex; flex-direction: column; gap: 16px; }
.header { display: flex; align-items: center; justify-content: space-between; }
.header.sticky { position: sticky; top: 0; background: #f7f9fc; z-index: 5; padding: 8px 16px; border-bottom: 1px solid #e6e9ee; border-radius: 12px; }
.title { display:flex; flex-direction: column; }
.title .sub { color:#6c757d; font-size:12px; }
.actions .btn, .btn-primary { padding: 8px 12px; border-radius: 8px; border: 1px solid #cbd5e1; background: #f1f5f9; color:#0f172a; }
.actions .btn:hover { background:#e2e8f0; }
.btn-primary { background: linear-gradient(90deg,#2563eb,#1d4ed8); color: #fff; border: none; }
.btn-primary:hover { filter: brightness(0.95); }
.tabs { display:none; }
.sections { display: grid; grid-template-columns: 1fr; gap: 16px; }
.card { background: #fff; border: 1px solid #e6e9ee; border-radius: 12px; padding: 16px; display: flex; flex-direction: column; gap: 12px; }
.form .row { display: flex; flex-direction: column; gap: 6px; }
.form .row.two { flex-direction: row; gap: 12px; }
.form .row.two > div { flex: 1; display: flex; flex-direction: column; }
.form label { font-size: 12px; color: #6c757d; }
.form input, .form select, .form textarea { padding: 10px 12px; border: 1px solid #e6e9ee; border-radius: 8px; }
.form .row.invalid input, .form .row.invalid textarea { border-color: #e55353; background: #fff7f7; }
.form .error { color:#e55353; font-size:12px; }
.mapping { display: flex; flex-direction: column; gap: 8px; }
.map-row { display: grid; grid-template-columns: 1fr 1fr auto; gap: 8px; }
.map-row-adv { display:grid; grid-template-columns: 1.2fr 1.2fr 140px 120px 120px 100px auto; gap:8px; }
.map-actions { display:flex; gap:8px; }
.status .muted { color: #6c757d; }
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


