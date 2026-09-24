<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { serviceApi } from '@/api/service'
import type { ServiceInfo } from '@/api/types'

const loading = ref(false)
const services = ref<ServiceInfo[]>([])

async function load(silent = false) {
  if (!silent) loading.value = true
  try {
    const res = await serviceApi.getWebServices()
    if (res.IsSuccess) {
      services.value = res.Data ?? []
    } else if (!silent) {
      ElMessage.error(res.Message)
    }
  } finally {
    if (!silent) loading.value = false
  }
}

// ---------------- 单个控制 ----------------
async function start(item: ServiceInfo) {
  const res = await serviceApi.startService(item.ServiceId)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function stop(item: ServiceInfo) {
  const res = await serviceApi.stopService(item.ServiceId)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function restart(item: ServiceInfo) {
  const res = await serviceApi.restartService(item.ServiceId)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function toggleEnabled(item: ServiceInfo) {
  const res = await serviceApi.setServiceEnabled({
    ServiceId: item.ServiceId,
    IsEnabled: !item.IsEnabled,
  })
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

// ---------------- 批量控制 ----------------
async function startAll() {
  const res = await serviceApi.startAllWebServices()
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function stopAll() {
  const res = await serviceApi.stopAllWebServices()
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

// ---------------- 自动刷新 ----------------
let timer: number | null = null

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

function formatTime(t: string) {
  if (!t) return '-'
  return new Date(t).toLocaleString('zh-CN')
}
</script>

<template>
  <div class="page">
    <div class="toolbar">
      <el-button type="success" @click="startAll">启动全部</el-button>
      <el-button type="warning" @click="stopAll">停止全部</el-button>
      <el-button @click="load">刷新</el-button>
    </div>

    <el-table v-loading="loading" :data="services" border stripe>
      <el-table-column prop="ServiceId" label="服务ID" min-width="200" show-overflow-tooltip />
      <el-table-column prop="Name" label="服务名称" min-width="180" />
      <el-table-column prop="ServiceType" label="服务类型" width="180" show-overflow-tooltip />
      <el-table-column label="状态" width="90">
        <template #default="{ row }">
          <el-tag :type="row.IsRunning ? 'success' : 'info'">
            {{ row.IsRunning ? '运行中' : '已停止' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="启用" width="80">
        <template #default="{ row }">
          <el-switch :model-value="row.IsEnabled" @change="toggleEnabled(row)" />
        </template>
      </el-table-column>
      <el-table-column prop="Remark" label="备注" min-width="140" show-overflow-tooltip />
      <el-table-column label="最后操作时间" width="170">
        <template #default="{ row }">{{ formatTime(row.LastUpdateTime) }}</template>
      </el-table-column>
      <el-table-column prop="LastOperator" label="操作人" width="90" />
      <el-table-column label="操作" width="220" fixed="right">
        <template #default="{ row }">
          <el-button
            v-if="!row.IsRunning"
            size="small"
            type="success"
            :disabled="!row.IsEnabled"
            @click="start(row)"
            >启动</el-button
          >
          <el-button v-else size="small" type="danger" :disabled="!row.IsEnabled" @click="stop(row)"
            >停止</el-button
          >
          <el-button size="small" :disabled="!row.IsEnabled" @click="restart(row)">重启</el-button>
        </template>
      </el-table-column>
    </el-table>
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
</style>
