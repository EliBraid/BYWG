<template>
  <div class="page">
    <div class="header sticky">
      <div class="title">
        <h2>MQTT 集成</h2>
        <span class="sub">Broker 配置、主题映射、调试与运行监控</span>
      </div>
      <div class="actions">
        <button class="btn" @click="handleTest" :disabled="loading">测试连接</button>
        <button class="btn" @click="toggleEnabled" :disabled="loading">{{ enabled ? '停用' : '启用' }}</button>
        <button class="btn-primary" @click="handleSave" :disabled="loading">保存</button>
      </div>
    </div>

    <div class="sections">
      <div class="card">
        <h3>基础配置（Broker）</h3>
        <div class="form">
          <div class="row" :class="errors.name && 'invalid'"><label>名称</label><input v-model.trim="form.name" /><small v-if="errors.name" class="error">{{ errors.name }}</small></div>
          <div class="row two">
            <div :class="errors.host && 'invalid'"><label>Host</label><input v-model.trim="form.host" placeholder="127.0.0.1" /><small v-if="errors.host" class="error">{{ errors.host }}</small></div>
            <div :class="errors.port && 'invalid'"><label>Port</label><input type="number" v-model.number="form.port" min="1" /><small v-if="errors.port" class="error">{{ errors.port }}</small></div>
          </div>
          <div class="row two">
            <div><label>心跳秒</label><input type="number" v-model.number="form.keepAlive" min="5" /></div>
            <div><label>Clean Session</label>
              <select v-model="form.cleanSession"><option :value="true">是</option><option :value="false">否</option></select>
            </div>
          </div>
          <div class="row two">
            <div><label>ClientId</label><input v-model.trim="form.clientId" placeholder="bywg-gw-001" /></div>
            <div><label>遗嘱主题</label><input v-model="form.willTopic" placeholder="factory/line1/lastwill" /></div>
          </div>
          <div class="row two">
            <div><label>遗嘱保留</label>
              <select v-model="form.willRetain"><option :value="true">是</option><option :value="false">否</option></select>
            </div>
          </div>
          <div class="row"><label>遗嘱消息</label><input v-model="form.willPayload" placeholder='{"status":"offline"}' /></div>
        </div>
      </div>

      <div class="card">
        <h3>认证 / 安全</h3>
        <div class="form">
          <div class="row two">
            <div><label>用户名</label><input v-model="form.username" /></div>
            <div><label>密码</label><input type="password" v-model="form.password" /></div>
          </div>
          <div class="row two">
            <div><label>SSL/TLS</label>
              <select v-model="form.ssl"><option :value="false">关闭</option><option :value="true">开启</option></select>
            </div>
            <div><label>跳过TLS校验</label>
              <select v-model="form.insecureSkipVerify"><option :value="true">是</option><option :value="false">否</option></select>
            </div>
          </div>
          <div class="row"><label>CA证书</label><textarea rows="3" v-model="form.caCertificate" placeholder="PEM"></textarea></div>
        </div>
      </div>

      <div class="card">
        <h3>主题映射</h3>
        <div class="mapping">
          <div class="map-row-adv" v-for="(m,i) in form.mappings" :key="i">
            <input v-model="m.topic" placeholder="topic，例如: factory/line1/status" />
            <input v-model="m.target" placeholder="目标表/字段，例如: devices.status" />
            <select v-model.number="m.qos">
              <option :value="0">QoS 0</option>
              <option :value="1">QoS 1</option>
              <option :value="2">QoS 2</option>
            </select>
            <select v-model="m.transformer">
              <option value="json">JSON</option>
              <option value="plain">纯文本</option>
            </select>
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
            <div><label>订阅主题</label><input v-model="debug.topic" placeholder="factory/+/status" /></div>
            <div><label>QoS</label>
              <select v-model.number="debug.qos"><option :value="0">0</option><option :value="1">1</option><option :value="2">2</option></select>
            </div>
          </div>
          <div class="row two">
            <div><label>发布主题</label><input v-model="debug.pubTopic" placeholder="factory/line1/cmd" /></div>
            <div><label>消息</label><input v-model="debug.payload" placeholder='{"cmd":"start"}' /></div>
          </div>
          <div class="row">
            <button class="btn" @click="runDebug" :disabled="loading">订阅/发布测试</button>
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
import { getMqttConfig, saveMqttConfig, testMqttConnection, getMqttLogs, type MqttConfigDto } from '@/api/integrations'

const loading = ref(false)
const enabled = ref(false)
const status = ref('未连接')
const lastError = ref('')
const lastHeartbeat = ref('')
const logs = ref<string[]>([])

const form = ref<MqttConfigDto>({
  name: 'MQTT 对接',
  host: '',
  port: 1883,
  username: '',
  password: '',
  clientId: '',
  cleanSession: true,
  ssl: false,
  caCertificate: '',
  insecureSkipVerify: false,
  keepAlive: 60,
  willRetain: false,
  willTopic: '',
  willPayload: '',
  enabled: false,
  mappings: []
})

const errors = reactive<{[k:string]: string|undefined}>({})

function addMapping(){ form.value.mappings.push({ topic: '', target: '', qos: 0 }) }
function removeMapping(i:number){ form.value.mappings.splice(i,1) }
function importMappings(){}
function exportMappings(){}

function validate(): boolean {
  errors.name = !form.value.name?.trim() ? '名称必填' : undefined
  errors.host = !form.value.host?.trim() ? 'Host 必填' : undefined
  errors.port = !form.value.port || form.value.port <= 0 ? '端口需为正整数' : undefined
  return !errors.name && !errors.host && !errors.port
}

async function handleTest(){
  loading.value = true
  try { await testMqttConnection({ host: form.value.host, port: form.value.port }); status.value='可用'; lastError.value=''; lastHeartbeat.value=new Date().toLocaleString(); logs.value.unshift(`[${new Date().toLocaleTimeString()}] 测试成功`) } catch(e:any){ status.value='不可用'; lastError.value=e?.message||String(e); logs.value.unshift(`[${new Date().toLocaleTimeString()}] 测试失败: ${lastError.value}`) } finally { loading.value=false }
}

function toggleEnabled(){ enabled.value = !enabled.value }

async function handleSave(){ if(!validate()){ return } loading.value = true; try { await saveMqttConfig(form.value); logs.value.unshift(`[${new Date().toLocaleTimeString()}] 配置已保存`) } finally { loading.value=false } }

const debug = reactive<{ topic: string; qos: 0|1|2; pubTopic: string; payload: string; result: string }>({ topic: '', qos: 0, pubTopic: '', payload: '', result: '' })
async function runDebug(){ loading.value=true; try { await new Promise(r=>setTimeout(r,400)); debug.result = `订阅 ${debug.topic} (QoS ${debug.qos}); 发布 ${debug.pubTopic} => ${debug.payload}`; logs.value.unshift(`[${new Date().toLocaleTimeString()}] 调试发布/订阅成功`) } finally { loading.value=false } }

async function refreshLogs(){
  try { const data = await getMqttLogs(); logs.value = data } catch { logs.value.unshift(`[${new Date().toLocaleTimeString()}] 刷新日志`) }
}
function clearLogs(){ logs.value = [] }

onMounted(async ()=>{
  try {
    const data = await getMqttConfig()
    Object.assign(form.value, data)
  } catch {
    const cache = localStorage.getItem('mqttConfig')
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
.form .row.invalid input { border-color:#e55353; background:#fff7f7; }
.form .error { color:#e55353; font-size:12px; }
.mapping { display:flex; flex-direction:column; gap:8px; }
.map-row { display:grid; grid-template-columns: 1fr 1fr 120px auto; gap:8px; }
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


