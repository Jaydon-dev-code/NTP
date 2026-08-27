<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { collectionApi } from '@/api/collection'
import { equipmentApi } from '@/api/equipment'
import type { DeviceDataCollectionInfo, MqttPublishConfig, TbEquipment } from '@/api/types'

const loading = ref(false)
const registered = ref<DeviceDataCollectionInfo[]>([])

async function load(silent = false) {
  if (!silent) loading.value = true
  try {
    const res = await collectionApi.getAllRegistered()
    if (res.IsSuccess) {
      registered.value = res.Data ?? []
    } else if (!silent) {
      ElMessage.error(res.Message)
    }
  } finally {
    if (!silent) loading.value = false
  }
}

// ---------------- 注册 ----------------
const registerDialog = ref(false)
const registerTab = ref('device')
const allEquipments = ref<TbEquipment[]>([])
const equipOptions = ref<TbEquipment[]>([])
const selectedEquipmentId = ref<string | null>(null)
const selectedLineId = ref<number | null>(null)
const selectedFactoryId = ref<number | null>(null)
const factories = ref<{ Id: number; FactoryName: string }[]>([])
const lines = ref<{ Id: number; LineName: string; FactoryId: number }[]>([])
const mqttForm = ref<MqttPublishConfig>({
  Host: '',
  Port: 1883,
  Username: '',
  Password: '',
  Topic: '',
})

async function openRegisterDialog() {
  registerTab.value = 'device'
  selectedEquipmentId.value = null
  mqttForm.value = { Host: '', Port: 1883, Username: '', Password: '', Topic: '' }
  registerDialog.value = true

  const [eqRes, fRes, lRes] = await Promise.all([
    equipmentApi.getEquipments(),
    equipmentApi.getFactories(),
    equipmentApi.getProductionLines(),
  ])
  allEquipments.value = eqRes.Data ?? []
  equipOptions.value = allEquipments.value.filter((e) => e.LineId != null)
  factories.value = (fRes.Data ?? []).map((f) => ({ Id: f.Id, FactoryName: f.FactoryName }))
  lines.value = (lRes.Data ?? []).map((l) => ({
    Id: l.Id,
    LineName: l.LineName,
    FactoryId: l.FactoryId,
  }))
}

async function registerDevice() {
  if (!selectedEquipmentId.value) {
    ElMessage.warning('请选择设备')
    return
  }
  const res = await collectionApi.registerDevice({
    EquipmentId: selectedEquipmentId.value,
    MqttConfig: mqttForm.value,
  })
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    registerDialog.value = false
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function registerLine() {
  if (!selectedLineId.value) {
    ElMessage.warning('请选择产线')
    return
  }
  const res = await collectionApi.registerLine(selectedLineId.value)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    registerDialog.value = false
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function registerFactory() {
  if (!selectedFactoryId.value) {
    ElMessage.warning('请选择厂')
    return
  }
  const res = await collectionApi.registerFactory(selectedFactoryId.value)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    registerDialog.value = false
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

// ---------------- 运行控制 ----------------
async function start(item: DeviceDataCollectionInfo) {
  const res = await collectionApi.start(item.EquipmentId)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function stop(item: DeviceDataCollectionInfo) {
  const res = await collectionApi.stop(item.EquipmentId)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function restart(item: DeviceDataCollectionInfo) {
  const res = await collectionApi.restart(item.EquipmentId)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function toggleEnabled(item: DeviceDataCollectionInfo) {
  const res = await collectionApi.setEnabled({
    EquipmentId: item.EquipmentId,
    IsEnabled: !item.IsEnabled,
  })
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function unregister(item: DeviceDataCollectionInfo) {
  await ElMessageBox.confirm(`确定注销设备 "${item.DeviceName}" 的采集服务吗？`, '提示', {
    type: 'warning',
  })
  const res = await collectionApi.unregisterDevice(item.EquipmentId)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function startAll() {
  const res = await collectionApi.startAll()
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function stopAll() {
  const res = await collectionApi.stopAll()
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

// ---------------- 点位快照 ----------------
const snapshotDialog = ref(false)
const snapshotValues = ref<Record<string, unknown>>({})
const snapshotTitle = ref('')

async function viewValues(item: DeviceDataCollectionInfo) {
  const res = await collectionApi.getDeviceValues(item.EquipmentId)
  if (res.IsSuccess) {
    snapshotTitle.value = `${item.DeviceName} 点位快照`
    snapshotValues.value = res.Data ?? {}
    snapshotDialog.value = true
  } else {
    ElMessage.error(res.Message)
  }
}

const snapshotEntries = ref<{ name: string; value: string }[]>([])
function refreshSnapshotEntries() {
  snapshotEntries.value = Object.entries(snapshotValues.value).map(([name, value]) => ({
    name,
    value: Array.isArray(value) ? value.join(',') : String(value ?? ''),
  }))
}

let timer: number | null = null

// 定时刷新状态
function startAutoRefresh() {
  timer = window.setInterval(() => {
    load(true)
  }, 5000)
}

onMounted(() => {
  load()
  startAutoRefresh()
})

onUnmounted(() => {
  if (timer) window.clearInterval(timer)
})
</script>

<template>
  <div class="page">
    <div class="toolbar">
      <el-button type="primary" @click="openRegisterDialog">注册采集</el-button>
      <el-button type="success" @click="startAll">启动全部</el-button>
      <el-button type="warning" @click="stopAll">停止全部</el-button>
      <el-button @click="load">刷新</el-button>
      <el-divider direction="vertical" />
    </div>

    <el-table v-loading="loading" :data="registered" border stripe>
      <el-table-column prop="EquipmentId" label="设备ID" width="90" />
      <el-table-column prop="DeviceName" label="设备名称" min-width="160" />
      <el-table-column label="厂" width="120">
        <template #default="{ row }">{{ row.FactoryName }} ({{ row.FactoryCode }})</template>
      </el-table-column>
      <el-table-column label="产线" width="130">
        <template #default="{ row }">{{ row.LineName }} ({{ row.LineCode }})</template>
      </el-table-column>
      <el-table-column prop="Topic" label="MQTT Topic" min-width="180" show-overflow-tooltip />
      <el-table-column label="状态" width="90">
        <template #default="{ row }">
          <el-tag :type="row.IsRunning ? 'success' : 'info'">{{ row.Status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="Description" label="运行描述" min-width="160" show-overflow-tooltip />
      <el-table-column label="启用" width="80">
        <template #default="{ row }">
          <el-switch :model-value="row.IsEnabled" @change="toggleEnabled(row)" />
        </template>
      </el-table-column>
      <el-table-column label="操作" width="300" fixed="right">
        <template #default="{ row }">
          <el-button v-if="!row.IsRunning" size="small" type="success" @click="start(row)"
            >启动</el-button
          >
          <el-button v-else size="small" type="danger" @click="stop(row)">停止</el-button>
          <el-button size="small" @click="restart(row)">重启</el-button>
          <el-button size="small" @click="viewValues(row)">点位</el-button>
          <el-button size="small" type="warning" @click="unregister(row)">注销</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 注册对话框 -->
    <el-dialog v-model="registerDialog" title="注册采集服务" width="560px">
      <el-tabs v-model="registerTab">
        <el-tab-pane label="单台设备" name="device">
          <el-form label-width="100px">
            <el-form-item label="设备">
              <el-select
                v-model="selectedEquipmentId"
                filterable
                placeholder="选择已归属产线的设备"
                style="width: 100%"
              >
                <el-option
                  v-for="e in equipOptions"
                  :key="e.EquipmentId"
                  :label="`${e.DeviceName} (${e.EquipmentId}) (${e.ProductionLine?.LineName ?? '未归属'})`"
                  :value="e.EquipmentId"
                />
              </el-select>
            </el-form-item>
            <el-divider content-position="left">MQTT 配置（留空则取产线/厂配置）</el-divider>
            <el-form-item label="MQTT IP">
              <el-input v-model="mqttForm.Host" placeholder="127.0.0.1" />
            </el-form-item>
            <el-form-item label="MQTT 端口">
              <el-input-number v-model="mqttForm.Port" :min="1" :max="65535" />
            </el-form-item>
            <el-form-item label="用户名">
              <el-input v-model="mqttForm.Username" />
            </el-form-item>
            <el-form-item label="密码">
              <el-input v-model="mqttForm.Password" type="password" show-password />
            </el-form-item>
            <el-form-item label="Topic 前缀">
              <el-input v-model="mqttForm.Topic" placeholder="留空按 厂标识/产线标识 拼接" />
            </el-form-item>
          </el-form>
          <div class="dialog-footer">
            <el-button @click="registerDialog = false">取消</el-button>
            <el-button type="primary" :disabled="!selectedEquipmentId" @click="registerDevice"
              >注册</el-button
            >
          </div>
        </el-tab-pane>

        <el-tab-pane label="按产线注册" name="line">
          <el-form label-width="100px">
            <el-form-item label="产线">
              <el-select
                v-model="selectedLineId"
                filterable
                placeholder="选择产线"
                style="width: 100%"
              >
                <el-option
                  v-for="l in lines"
                  :key="l.Id"
                  :label="`${l.LineName} (${l.FactoryId})`"
                  :value="l.Id"
                />
              </el-select>
            </el-form-item>
          </el-form>
          <div class="dialog-footer">
            <el-button @click="registerDialog = false">取消</el-button>
            <el-button type="primary" :disabled="!selectedLineId" @click="registerLine"
              >注册</el-button
            >
          </div>
        </el-tab-pane>

        <el-tab-pane label="按厂注册" name="factory">
          <el-form label-width="100px">
            <el-form-item label="厂">
              <el-select
                v-model="selectedFactoryId"
                filterable
                placeholder="选择厂"
                style="width: 100%"
              >
                <el-option
                  v-for="f in factories"
                  :key="f.Id"
                  :label="f.FactoryName"
                  :value="f.Id"
                />
              </el-select>
            </el-form-item>
          </el-form>
          <div class="dialog-footer">
            <el-button @click="registerDialog = false">取消</el-button>
            <el-button type="primary" :disabled="!selectedFactoryId" @click="registerFactory"
              >注册</el-button
            >
          </div>
        </el-tab-pane>
      </el-tabs>
    </el-dialog>

    <!-- 点位快照对话框 -->
    <el-dialog
      v-model="snapshotDialog"
      :title="snapshotTitle"
      width="520px"
      @open="refreshSnapshotEntries"
    >
      <el-table v-if="snapshotEntries.length" :data="snapshotEntries" border stripe size="small">
        <el-table-column prop="name" label="点位" />
        <el-table-column prop="value" label="当前值" />
      </el-table>
      <div v-else class="empty-hint">暂无点位数据（服务未启动或点位为空）</div>
    </el-dialog>
  </div>
</template>

<style scoped>
.page {
  padding: 12px;
}
.toolbar {
  margin-bottom: 12px;
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.heartbeat {
  display: flex;
  align-items: center;
  gap: 6px;
}
.heartbeat .unit {
  font-size: 12px;
  color: var(--el-text-color-secondary);
}
.heartbeat .tip {
  font-size: 12px;
  color: var(--el-text-color-placeholder);
}
.dialog-footer {
  text-align: right;
}
.empty-hint {
  color: var(--el-text-color-placeholder);
  text-align: center;
  padding: 24px 0;
}
</style>
