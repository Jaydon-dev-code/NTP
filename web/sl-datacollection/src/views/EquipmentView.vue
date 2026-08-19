<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { equipmentApi } from '@/api/equipment'
import type { TbEquipment, TbFactory, TbProductionLine } from '@/api/types'

const loading = ref(false)
const factories = ref<TbFactory[]>([])

// 左侧：选中厂
const selectedFactory = ref<TbFactory | null>(null)
// 右侧：选中产线
const selectedLine = ref<TbProductionLine | null>(null)

const lines = computed(() => selectedFactory.value?.ProductionLines ?? [])

// 未归属产线的设备
const unassignedEquipments = ref<TbEquipment[]>([])

async function load() {
  loading.value = true
  try {
    const res = await equipmentApi.getFactories()
    if (res.IsSuccess) {
      factories.value = res.Data ?? []
      // 刷新选中厂
      if (selectedFactory.value) {
        const match = factories.value.find((f) => f.Id === selectedFactory.value!.Id)
        selectedFactory.value = match ?? null
        // 刷新选中产线
        if (selectedLine.value && match) {
          const lineMatch = match.ProductionLines?.find((l) => l.Id === selectedLine.value!.Id)
          selectedLine.value = lineMatch ?? null
        }
      }
    } else {
      ElMessage.error(res.Message)
    }
  } finally {
    loading.value = false
  }
}

function selectFactory(factory: TbFactory) {
  selectedFactory.value = factory
  selectedLine.value = null
  loadUnassigned()
}

function selectLine(line: TbProductionLine) {
  selectedLine.value = line
}

async function loadUnassigned() {
  const res = await equipmentApi.getEquipments()
  if (res.IsSuccess) {
    unassignedEquipments.value = (res.Data ?? []).filter((e) => e.LineId == null)
  }
}

function refreshSelectedLine() {
  if (!selectedFactory.value || !selectedLine.value) return
  const factory = factories.value.find((f) => f.Id === selectedFactory.value!.Id)
  const line = factory?.ProductionLines?.find((l) => l.Id === selectedLine.value!.Id)
  selectedLine.value = line ?? null
}

// ---------------- 导入点位 Excel ----------------
const importFileInput = ref<HTMLInputElement | null>(null)
const importing = ref(false)

function openImportPicker() {
  importFileInput.value?.click()
}

async function onImportFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  input.value = ''
  if (!file) return

  if (!file.name.toLowerCase().endsWith('.xlsx')) {
    ElMessage.warning('只支持 .xlsx 格式文件')
    return
  }

  importing.value = true
  try {
    const res = await equipmentApi.importPlcPoints(file)
    if (res.IsSuccess) {
      ElMessage.success(res.Message)
      await load()
      await loadUnassigned()
    } else {
      ElMessage.error(res.Message)
    }
  } catch (err) {
    ElMessage.error(`导入异常: ${String(err)}`)
  } finally {
    importing.value = false
  }
}

onMounted(() => {
  load()
  loadUnassigned()
})

// ---------------- 厂 ----------------
const factoryForm = reactive({ Id: 0, FactoryCode: '', FactoryName: '', Remark: '' })
const factoryDialog = ref(false)

function openFactoryDialog(factory?: TbFactory) {
  factoryForm.Id = factory?.Id ?? 0
  factoryForm.FactoryCode = factory?.FactoryCode ?? ''
  factoryForm.FactoryName = factory?.FactoryName ?? ''
  factoryForm.Remark = factory?.Remark ?? ''
  factoryDialog.value = true
}

async function saveFactory() {
  if (!factoryForm.FactoryCode || !factoryForm.FactoryName) {
    ElMessage.warning('厂编码和厂名称不能为空')
    return
  }
  const res =
    factoryForm.Id > 0
      ? await equipmentApi.updateFactory({ ...factoryForm })
      : await equipmentApi.addFactory({ ...factoryForm })
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    factoryDialog.value = false
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function deleteFactory(factory: TbFactory) {
  await ElMessageBox.confirm(`确定删除厂 "${factory.FactoryName}" 吗？`, '提示', {
    type: 'warning',
  })
  const res = await equipmentApi.deleteFactory(factory.Id)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    if (selectedFactory.value?.Id === factory.Id) selectedFactory.value = null
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

// ---------------- 产线 ----------------
const lineForm = reactive({
  Id: 0,
  FactoryId: 0,
  LineCode: '',
  LineName: '',
  MqttHost: '',
  MqttPort: 1883,
  MqttUsername: '',
  MqttPassword: '',
  Remark: '',
})
const lineDialog = ref(false)

function openLineDialog(line?: TbProductionLine) {
  if (!selectedFactory.value && !line) {
    ElMessage.warning('请先选择厂')
    return
  }
  lineForm.Id = line?.Id ?? 0
  lineForm.FactoryId = line?.FactoryId ?? selectedFactory.value!.Id
  lineForm.LineCode = line?.LineCode ?? ''
  lineForm.LineName = line?.LineName ?? ''
  lineForm.MqttHost = line?.MqttHost ?? ''
  lineForm.MqttPort = line?.MqttPort ?? 1883
  lineForm.MqttUsername = line?.MqttUsername ?? ''
  lineForm.MqttPassword = line?.MqttPassword ?? ''
  lineForm.Remark = line?.Remark ?? ''
  lineDialog.value = true
}

async function saveLine() {
  if (!lineForm.LineCode || !lineForm.LineName) {
    ElMessage.warning('产线编码和产线名称不能为空')
    return
  }
  const res =
    lineForm.Id > 0
      ? await equipmentApi.updateProductionLine({ ...lineForm })
      : await equipmentApi.addProductionLine({ ...lineForm })
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    lineDialog.value = false
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

async function deleteLine(line: TbProductionLine) {
  await ElMessageBox.confirm(`确定删除产线 "${line.LineName}" 吗？其下设备将被移出`, '提示', {
    type: 'warning',
  })
  const res = await equipmentApi.deleteProductionLine(line.Id)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    if (selectedLine.value?.Id === line.Id) selectedLine.value = null
    await load()
  } else {
    ElMessage.error(res.Message)
  }
}

// ---------------- 设备 ----------------
const equipmentForm = reactive({ Id: 0, EquipmentId: '', DeviceName: '', Remark: '' })
const equipmentDialog = ref(false)

function openEquipmentDialog(equipment?: TbEquipment) {
  equipmentForm.Id = equipment?.Id ?? 0
  equipmentForm.EquipmentId = equipment?.EquipmentId ?? ''
  equipmentForm.DeviceName = equipment?.DeviceName ?? ''
  equipmentForm.Remark = equipment?.Remark ?? ''
  equipmentDialog.value = true
}

async function saveEquipment() {
  if (!equipmentForm.DeviceName) {
    ElMessage.warning('设备名称不能为空')
    return
  }
  if (!equipmentForm.EquipmentId) {
    ElMessage.warning('设备编号不能为空')
    return
  }
  const res =
    equipmentForm.Id > 0
      ? await equipmentApi.updateEquipment({ ...equipmentForm })
      : await equipmentApi.addEquipment({ ...equipmentForm })
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    equipmentDialog.value = false
    await load()
    await loadUnassigned()
  } else {
    ElMessage.error(res.Message)
  }
}

async function deleteEquipment(equipment: TbEquipment) {
  await ElMessageBox.confirm(
    `确定删除设备 "${equipment.DeviceName}" 吗？其点位信息将一并删除`,
    '提示',
    {
      type: 'warning',
    },
  )
  const res = await equipmentApi.deleteEquipment(equipment.EquipmentId)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await loadUnassigned()
    if (selectedLine.value) await load()
  } else {
    ElMessage.error(res.Message)
  }
}

// 设备加入产线
async function addToLine(equipment: TbEquipment) {
  if (!selectedLine.value) {
    ElMessage.warning('请先选择产线')
    return
  }
  const res = await equipmentApi.addEquipmentToLine({
    EquipmentId: equipment.EquipmentId,
    LineId: selectedLine.value.Id,
  })
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
    refreshSelectedLine()
    await loadUnassigned()
  } else {
    ElMessage.error(res.Message)
  }
}

async function removeFromLine(equipment: TbEquipment) {
  const res = await equipmentApi.removeEquipmentFromLine(equipment.EquipmentId)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await load()
    refreshSelectedLine()
    await loadUnassigned()
  } else {
    ElMessage.error(res.Message)
  }
}
</script>

<template>
  <div class="page">
    <div class="toolbar">
      <el-button type="primary" @click="openFactoryDialog()">新增厂</el-button>
      <el-button type="success" @click="openLineDialog()">新增产线</el-button>
      <el-button v-show="false" @click="openEquipmentDialog()">新增设备</el-button>
      <el-button type="warning" :loading="importing" @click="openImportPicker">导入点位</el-button>
      <el-button v-show="false" @click="load()">刷新</el-button>
      <input
        ref="importFileInput"
        type="file"
        accept=".xlsx"
        style="display: none"
        @change="onImportFileChange"
      />
    </div>

    <el-row :gutter="12" v-loading="loading">
      <!-- 左：厂列表 -->
      <el-col :span="6">
        <el-card shadow="never">
          <template #header>
            <div class="card-header">
              <span>厂</span>
              <el-button
                v-if="selectedFactory"
                size="small"
                text
                type="primary"
                @click="openFactoryDialog(selectedFactory)"
                >编辑</el-button
              >
            </div>
          </template>
          <el-menu class="factory-menu">
            <el-menu-item
              v-for="f in factories"
              :key="f.Id"
              :index="String(f.Id)"
              @click="selectFactory(f)"
            >
              <div class="factory-item">
                <span>{{ f.FactoryName }}</span>
                <span class="factory-code">{{ f.FactoryCode }}</span>
                <el-button size="small" text type="danger" @click.stop="deleteFactory(f)"
                  >删除</el-button
                >
              </div>
            </el-menu-item>
          </el-menu>
        </el-card>
      </el-col>

      <!-- 中：产线列表 -->
      <el-col :span="6">
        <el-card shadow="never">
          <template #header>
            <div class="card-header">
              <span>产线 {{ selectedFactory ? `· ${selectedFactory.FactoryName}` : '' }}</span>
              <el-button
                v-if="selectedLine"
                size="small"
                text
                type="primary"
                @click="openLineDialog(selectedLine)"
                >编辑</el-button
              >
            </div>
          </template>
          <div v-if="!selectedFactory" class="empty-hint">请先选择厂</div>
          <el-menu class="line-menu" v-else>
            <el-menu-item
              v-for="line in lines"
              :key="line.Id"
              :index="String(line.Id)"
              @click="selectLine(line)"
            >
              <div class="factory-item">
                <span>{{ line.LineName }}</span>
                <span class="factory-code">{{ line.LineCode }}</span>
                <el-button size="small" text type="danger" @click.stop="deleteLine(line)"
                  >删除</el-button
                >
              </div>
            </el-menu-item>
          </el-menu>
        </el-card>
      </el-col>

      <!-- 右：设备 -->
      <el-col :span="12">
        <el-card shadow="never">
          <template #header>
            <div class="card-header">
              <span>设备 {{ selectedLine ? `· ${selectedLine.LineName}` : '· 未归属产线' }}</span>
              <span class="card-header-actions">
                <el-button
                  size="small"
                  v-show="false"
                  text
                  type="primary"
                  @click="openEquipmentDialog()"
                  >新增</el-button
                >
                <el-button v-show="false" size="small" text @click="loadUnassigned()"
                  >刷新</el-button
                >
              </span>
            </div>
          </template>

          <!-- 当前产线下设备 -->
          <el-table
            v-if="selectedLine"
            :data="selectedLine.Equipments ?? []"
            border
            stripe
            size="small"
          >
            <el-table-column prop="EquipmentId" label="设备编号" width="120" />
            <el-table-column prop="DeviceName" label="设备名称" />
            <el-table-column prop="CreateTime" label="创建时间" width="170" />
            <el-table-column label="操作" width="160">
              <template #default="{ row }">
                <el-button size="small" text type="primary" @click="openEquipmentDialog(row)"
                  >编辑</el-button
                >
                <el-button size="small" text type="warning" @click="removeFromLine(row)"
                  >移出</el-button
                >
                <el-button size="small" text type="danger" @click="deleteEquipment(row)"
                  >删除</el-button
                >
              </template>
            </el-table-column>
          </el-table>
          <div v-else-if="selectedFactory" class="empty-hint">请先选择产线</div>
          <div v-else class="empty-hint">请先在左侧选择厂和产线</div>

          <!-- 未归属产线的设备 -->
          <template v-if="unassignedEquipments.length">
            <el-divider content-position="left">未归属产线的设备</el-divider>
            <el-table :data="unassignedEquipments" border stripe size="small">
              <el-table-column prop="EquipmentId" label="设备编号" width="120" />
              <el-table-column prop="DeviceName" label="设备名称" />
              <el-table-column prop="CreateTime" label="创建时间" width="170" />
              <el-table-column label="操作" width="200">
                <template #default="{ row }">
                  <el-button size="small" text type="primary" @click="openEquipmentDialog(row)"
                    >编辑</el-button
                  >
                  <el-button
                    size="small"
                    text
                    type="success"
                    :disabled="!selectedLine"
                    @click="addToLine(row)"
                    >加入产线</el-button
                  >
                  <el-button size="small" text type="danger" @click="deleteEquipment(row)"
                    >删除</el-button
                  >
                </template>
              </el-table-column>
            </el-table>
          </template>
        </el-card>
      </el-col>
    </el-row>

    <!-- 厂对话框 -->
    <el-dialog v-model="factoryDialog" :title="factoryForm.Id ? '编辑厂' : '新增厂'" width="480px">
      <el-form label-width="80px">
        <el-form-item label="厂编码" required>
          <el-input v-model="factoryForm.FactoryCode" placeholder="如 F01" />
        </el-form-item>
        <el-form-item label="厂名称" required>
          <el-input v-model="factoryForm.FactoryName" placeholder="如 一分厂" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="factoryForm.Remark" type="textarea" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="factoryDialog = false">取消</el-button>
        <el-button type="primary" @click="saveFactory">保存</el-button>
      </template>
    </el-dialog>

    <!-- 产线对话框 -->
    <el-dialog v-model="lineDialog" :title="lineForm.Id ? '编辑产线' : '新增产线'" width="520px">
      <el-form label-width="90px">
        <el-form-item label="所属厂">
          <el-select v-model="lineForm.FactoryId" style="width: 100%">
            <el-option v-for="f in factories" :key="f.Id" :label="f.FactoryName" :value="f.Id" />
          </el-select>
        </el-form-item>
        <el-form-item label="产线编码" required>
          <el-input v-model="lineForm.LineCode" placeholder="如 L01" />
        </el-form-item>
        <el-form-item label="产线名称" required>
          <el-input v-model="lineForm.LineName" placeholder="如 装配A线" />
        </el-form-item>
        <el-divider content-position="left">MQTT 配置</el-divider>
        <el-form-item label="MQTT IP">
          <el-input v-model="lineForm.MqttHost" placeholder="127.0.0.1" />
        </el-form-item>
        <el-form-item label="MQTT 端口">
          <el-input-number v-model="lineForm.MqttPort" :min="1" :max="65535" />
        </el-form-item>
        <el-form-item label="用户名">
          <el-input v-model="lineForm.MqttUsername" />
        </el-form-item>
        <el-form-item label="密码">
          <el-input v-model="lineForm.MqttPassword" type="password" show-password />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="lineForm.Remark" type="textarea" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="lineDialog = false">取消</el-button>
        <el-button type="primary" @click="saveLine">保存</el-button>
      </template>
    </el-dialog>

    <!-- 设备对话框 -->
    <el-dialog
      v-model="equipmentDialog"
      :title="equipmentForm.Id ? '编辑设备' : '新增设备'"
      width="460px"
    >
      <el-form label-width="80px">
        <el-form-item label="设备编号" required>
          <el-input
            v-model="equipmentForm.EquipmentId"
            placeholder="如 DEV-001"
            :disabled="equipmentForm.Id > 0"
          />
        </el-form-item>
        <el-form-item label="设备名称" required>
          <el-input v-model="equipmentForm.DeviceName" placeholder="如 六分厂6-1装配A线" />
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="equipmentForm.Remark" type="textarea" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="equipmentDialog = false">取消</el-button>
        <el-button type="primary" @click="saveEquipment">保存</el-button>
      </template>
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
  gap: 8px;
  align-items: center;
}
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-weight: bold;
}
.card-header-actions {
  display: flex;
  gap: 4px;
}
.factory-menu :deep(.el-menu-item) {
  height: 40px;
  padding: 0 8px;
}
.factory-item {
  display: flex;
  align-items: center;
  gap: 6px;
  width: 100%;
}
.factory-code {
  color: var(--el-text-color-secondary);
  font-size: 12px;
  flex: 1;
}
.empty-hint {
  color: var(--el-text-color-placeholder);
  text-align: center;
  padding: 24px 0;
}
</style>
