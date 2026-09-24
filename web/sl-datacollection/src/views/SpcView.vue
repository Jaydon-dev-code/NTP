<script setup lang="ts">
import { onMounted, ref, reactive, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { spcApi } from '@/api/spc'
import type {
  SpcProduct, SpcPart, SpcEquipment, SpcCharacteristic,
  SpcCollectionSource, SpcPlcPoint, TbPlcPoint, SPCDataCollectionInfo,
  CollectionMethodInfo, SpcCurrentModel,
} from '@/api/types'
import { SPCDataType, SPCDataSource } from '@/api/types'

const activeTab = ref('product')

// 采集方式选项（从后端获取，按数据来源过滤）
const allCollectionMethods = ref<CollectionMethodInfo[]>([])
const collectionMethodOptions = computed(() => {
  return allCollectionMethods.value
    .filter(m => m.DataSource === charForm.DataSource)
    .map(m => ({ value: m.ClassName, label: `${m.ClassName}（${m.Description}）` }))
})

async function loadCollectionMethods() {
  const res = await spcApi.getCollectionMethods()
  if (res.IsSuccess) allCollectionMethods.value = res.Data ?? []
}

// ==================== 产品 ====================
const products = ref<SpcProduct[]>([])
const productDialog = ref(false)
const productForm = reactive<SpcProduct>({ Id: 0, ModelNo: '', Name: '', Remark: '' })

async function loadProducts() {
  const res = await spcApi.getProducts()
  if (res.IsSuccess) products.value = res.Data ?? []
}
function openProductDialog(item?: SpcProduct) {
  Object.assign(productForm, item ?? { Id: 0, ModelNo: '', Name: '', Remark: '' })
  productDialog.value = true
}
async function saveProduct() {
  if (!productForm.ModelNo || !productForm.Name) return ElMessage.warning('型号和名称为必填项')
  const res = productForm.Id > 0 ? await spcApi.updateProduct(productForm) : await spcApi.addProduct(productForm)
  if (res.IsSuccess) { ElMessage.success(res.Message); productDialog.value = false; await loadProducts() } else ElMessage.error(res.Message)
}
async function deleteProduct(item: SpcProduct) {
  await ElMessageBox.confirm(`确定删除产品 "${item.Name}"？`, '提示', { type: 'warning' })
  const res = await spcApi.deleteProduct(item.Id)
  if (res.IsSuccess) { ElMessage.success(res.Message); await loadProducts() } else ElMessage.error(res.Message)
}

// ==================== 零件 ====================
const parts = ref<SpcPart[]>([])
const partDialog = ref(false)
const partForm = reactive<SpcPart>({ Id: 0, ModelNo: '', Name: '', ProductId: 0, PartCategory: '', Remark: '' })

async function loadParts() {
  const res = await spcApi.getParts()
  if (res.IsSuccess) parts.value = res.Data ?? []
}
function openPartDialog(item?: SpcPart) {
  Object.assign(partForm, item ?? { Id: 0, ModelNo: '', Name: '', ProductId: 0, PartCategory: '', Remark: '' })
  partDialog.value = true
}
async function savePart() {
  if (!partForm.ModelNo || !partForm.Name || !partForm.ProductId || !partForm.PartCategory)
    return ElMessage.warning('型号、名称、关联产品、零件类别为必填项')
  const res = partForm.Id > 0 ? await spcApi.updatePart(partForm) : await spcApi.addPart(partForm)
  if (res.IsSuccess) { ElMessage.success(res.Message); partDialog.value = false; await loadParts() } else ElMessage.error(res.Message)
}
async function deletePart(item: SpcPart) {
  await ElMessageBox.confirm(`确定删除零件 "${item.Name}"？`, '提示', { type: 'warning' })
  const res = await spcApi.deletePart(item.Id)
  if (res.IsSuccess) { ElMessage.success(res.Message); await loadParts() } else ElMessage.error(res.Message)
}

// ==================== 设备（SPC） ====================
const spcEquipments = ref<SpcEquipment[]>([])
const equipDialog = ref(false)
const equipForm = reactive<SpcEquipment>({ Id: 0, Name: '', ProductionLine: '', ProcessNo: '', ProcessName: '', Remark: '' })

async function loadEquipments() {
  const res = await spcApi.getEquipments()
  if (res.IsSuccess) spcEquipments.value = res.Data ?? []
}
function openEquipDialog(item?: SpcEquipment) {
  Object.assign(equipForm, item ?? { Id: 0, Name: '', ProductionLine: '', ProcessNo: '', ProcessName: '', Remark: '' })
  equipDialog.value = true
}
async function saveEquip() {
  if (!equipForm.Name || !equipForm.ProductionLine || !equipForm.ProcessNo || !equipForm.ProcessName)
    return ElMessage.warning('名称、所在产线、工序编号、工序名称为必填项')
  const res = equipForm.Id > 0 ? await spcApi.updateEquipment(equipForm) : await spcApi.addEquipment(equipForm)
  if (res.IsSuccess) { ElMessage.success(res.Message); equipDialog.value = false; await loadEquipments() } else ElMessage.error(res.Message)
}
async function deleteEquip(item: SpcEquipment) {
  await ElMessageBox.confirm(`确定删除设备 "${item.Name}"？`, '提示', { type: 'warning' })
  const res = await spcApi.deleteEquipment(item.Id)
  if (res.IsSuccess) { ElMessage.success(res.Message); await loadEquipments() } else ElMessage.error(res.Message)
}

// ==================== 特性 ====================
const characteristics = ref<SpcCharacteristic[]>([])
const charDialog = ref(false)
const charForm = reactive<SpcCharacteristic>({
  Id: 0, Name: '', CharacteristicNo: '', DataType: SPCDataType.Numeric,
  InspectionQty: 0, CollectionMethod: '', DataSource: SPCDataSource.Database,
  ProductId: null, PartId: null, PartCategory: '', EquipmentId: 0, Remark: '',
})

// 零件类别选项（从零件主数据去重）
const partCategoryOptions = computed(() => {
  const set = new Set<string>()
  for (const p of parts.value) {
    if (p.PartCategory) set.add(p.PartCategory)
  }
  return Array.from(set)
})

async function loadCharacteristics() {
  const res = await spcApi.getCharacteristics()
  if (res.IsSuccess) characteristics.value = res.Data ?? []
}

async function openCharDialog(item?: SpcCharacteristic) {
  Object.assign(charForm, item ?? {
    Id: 0, Name: '', CharacteristicNo: '', DataType: SPCDataType.Numeric,
    InspectionQty: 0, CollectionMethod: '', DataSource: SPCDataSource.Database,
    ProductId: null, PartId: null, PartCategory: '', EquipmentId: 0, Remark: '',
  })
  // 重置来源状态
  dbSourceSaved.value = false
  plcManualRows.value = []
  selectedPlcPointId.value = null
  availablePlcPoints.value = []
  savedPlcPoints.value = []
  excelFile.value = null
  plcTab.value = 'manual'
  Object.assign(plcManualForm, {
    IpAddress: '', Port: 8000, PlcType: '三菱',
    PointName: '', Area: '', Address: '', DataType: 'Int16', Length: 1,
  })
  // 编辑已有特性时加载已存点位
  if (item?.Id) await loadSavedPlcPoints()
  charDialog.value = true
}

async function saveChar() {
  const c = charForm
  if (!c.Name || !c.CharacteristicNo || !c.InspectionQty || !c.CollectionMethod || !c.PartCategory || !c.EquipmentId)
    return ElMessage.warning('名称、编号、检测数量、采集方式、零件类别、关联设备为必填项')

  const res = c.Id > 0 ? await spcApi.updateCharacteristic(c) : await spcApi.addCharacteristic(c)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    charDialog.value = false
    await loadCharacteristics()
  } else {
    ElMessage.error(res.Message)
  }
}

async function deleteChar(item: SpcCharacteristic) {
  await ElMessageBox.confirm(`确定删除特性 "${item.Name}"？`, '提示', { type: 'warning' })
  const res = await spcApi.deleteCharacteristic(item.Id)
  if (res.IsSuccess) { ElMessage.success(res.Message); await loadCharacteristics() } else ElMessage.error(res.Message)
}

function dataTypeLabel(v: number) { return v === SPCDataType.Numeric ? '数值' : '离散' }
function dataSourceLabel(v: number) { return v === SPCDataSource.Database ? '数据库' : 'PLC' }

// ==================== 数据来源：数据库 ====================
const dbSourceForm = reactive<SpcCollectionSource>({
  Id: 0, CharacteristicId: 0, ConnectionString: '', TableName: '', MeasureValueField: '', MeasureResultField: '', TimeFieldName: '', LastCollectionTime: '',
})
const dbSourceSaved = ref(false)

async function loadDbSource(charId: number) {
  const res = await spcApi.getCollectionSource(charId)
  if (res.IsSuccess && res.Data) {
    Object.assign(dbSourceForm, res.Data)
    dbSourceSaved.value = true
  }
}

async function saveDbSource() {
  const res = await spcApi.updateCollectionSource(dbSourceForm)
  if (res.IsSuccess) { ElMessage.success(res.Message); dbSourceSaved.value = true } else ElMessage.error(res.Message)
}

// ==================== 数据来源：PLC ====================
const plcTab = ref<'manual' | 'excel'>('manual')

// 已保存的点位列表（该特性下所有）
const savedPlcPoints = ref<SpcPlcPoint[]>([])

// 手动录入：待添加的点位行
const plcManualRows = ref<SpcPlcPoint[]>([])
const plcManualForm = reactive({
  IpAddress: '', Port: 8000, PlcType: '三菱',
  PointName: '', Area: '', Address: '', DataType: 'Int16', Length: 1,
})

// Excel 导入
const excelFile = ref<File | null>(null)
const excelInputRef = ref<HTMLInputElement | null>(null)
const excelImporting = ref(false)

// 设备现有点位（导入选择模式）
const availablePlcPoints = ref<TbPlcPoint[]>([])
const selectedPlcPointId = ref<number | null>(null)

async function loadSavedPlcPoints() {
  if (!charForm.Id) { savedPlcPoints.value = []; return }
  const res = await spcApi.getPlcPointsByCharacteristic(charForm.Id)
  if (res.IsSuccess) savedPlcPoints.value = res.Data ?? []
}

function addManualRow() {
  const f = plcManualForm
  if (!f.IpAddress || !f.PointName || !f.Area || !f.Address || !f.DataType)
    return ElMessage.warning('IP、变量名、区块、地址、数据类型为必填项')
  plcManualRows.value.push({
    Id: 0, CharacteristicId: charForm.Id, PlcPointId: 0,
    IpAddress: f.IpAddress, Port: f.Port, PlcType: f.PlcType,
    PointName: f.PointName, Area: f.Area, Address: f.Address,
    DataType: f.DataType, Length: f.Length, CreateTime: '',
  })
  // 重置表单
  Object.assign(plcManualForm, {
    IpAddress: plcManualForm.IpAddress, Port: plcManualForm.Port, PlcType: plcManualForm.PlcType,
    PointName: '', Area: '', Address: '', DataType: 'Int16', Length: 1,
  })
}

function removeManualRow(index: number) {
  plcManualRows.value.splice(index, 1)
}

async function saveAllManualRows() {
  if (plcManualRows.value.length === 0) return ElMessage.warning('请先添加点位')
  const res = await spcApi.batchInsertPlcPoints(plcManualRows.value)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    plcManualRows.value = []
    await loadSavedPlcPoints()
  } else {
    ElMessage.error(res.Message)
  }
}

async function saveSingleManualPoint() {
  addManualRow()
  if (plcManualRows.value.length > 0) await saveAllManualRows()
}

async function importEquipmentPlcPoints() {
  if (!charForm.EquipmentId) return ElMessage.warning('请先选择关联设备')
  const res = await spcApi.getEquipmentPlcPoints(charForm.EquipmentId)
  if (res.IsSuccess) {
    availablePlcPoints.value = res.Data ?? []
    if (availablePlcPoints.value.length === 0)
      ElMessage.info('该设备下没有现有点位，请手动录入或 Excel 导入')
  } else {
    ElMessage.error(res.Message)
  }
}

function confirmImportPoint() {
  if (!selectedPlcPointId.value) return ElMessage.warning('请选择一个点位')
  const point = availablePlcPoints.value.find(p => p.Id === selectedPlcPointId.value)
  if (point) {
    const dto: SpcPlcPoint = {
      Id: 0, CharacteristicId: charForm.Id, PlcPointId: point.Id, CreateTime: '',
    }
    spcApi.batchInsertPlcPoints([dto]).then(async (res) => {
      if (res.IsSuccess) {
        ElMessage.success(`已关联点位: ${point.PointName}`)
        selectedPlcPointId.value = null
        await loadSavedPlcPoints()
      } else {
        ElMessage.error(res.Message)
      }
    })
  }
}

function openExcelPicker() {
  excelInputRef.value?.click()
}

async function onExcelFileChange(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  if (!file.name.toLowerCase().endsWith('.xlsx')) {
    ElMessage.warning('只支持 .xlsx 文件')
    input.value = ''
    return
  }
  if (!charForm.Id) {
    ElMessage.warning('请先保存特性后再导入')
    input.value = ''
    return
  }
  excelImporting.value = true
  try {
    const res = await spcApi.importPlcPoints(charForm.Id, file)
    if (res.IsSuccess) {
      ElMessage.success(res.Message)
      await loadSavedPlcPoints()
    } else {
      ElMessage.error(res.Message)
    }
  } finally {
    excelImporting.value = false
    input.value = ''
  }
}

async function removeSavedPoint(id: number) {
  const res = await spcApi.deletePlcPoint(id)
  if (res.IsSuccess) {
    ElMessage.success(res.Message)
    await loadSavedPlcPoints()
  } else {
    ElMessage.error(res.Message)
  }
}

// 来源配置（编辑已有特性时）
const sourceDialog = ref(false)
const sourceForm = reactive<SpcCollectionSource>({
  Id: 0, CharacteristicId: 0, ConnectionString: '', TableName: '', MeasureValueField: '', MeasureResultField: '', TimeFieldName: '', LastCollectionTime: '',
})

async function openSourceConfig(item: SpcCharacteristic) {
  if (item.DataSource === SPCDataSource.Database) {
    const res = await spcApi.getCollectionSource(item.Id)
    if (res.IsSuccess && res.Data) Object.assign(sourceForm, res.Data)
    else Object.assign(sourceForm, { Id: 0, CharacteristicId: item.Id })
    sourceDialog.value = true
  } else {
    // PLC: 加载该特性已保存的点位
    charForm.Id = item.Id
    charForm.EquipmentId = item.EquipmentId
    await loadSavedPlcPoints()
    plcTab.value = 'manual'
    // 直接打开特性编辑弹窗展示点位
    await openCharDialog(item)
  }
}

async function saveSource() {
  const res = await spcApi.updateCollectionSource(sourceForm)
  if (res.IsSuccess) { ElMessage.success(res.Message); sourceDialog.value = false } else ElMessage.error(res.Message)
}

// ==================== 当前生产型号 ====================
const currentModel = ref<SpcCurrentModel | null>(null)

async function loadCurrentModel() {
  const res = await spcApi.getCurrentModel()
  if (res.IsSuccess) currentModel.value = res.Data ?? null
}

async function syncCurrentModel() {
  const res = await spcApi.syncCurrentModelNow()
  if (res.IsSuccess) { ElMessage.success(res.Message); await loadCurrentModel() }
  else ElMessage.error(res.Message)
}

function currentModelStatusType(s?: string) {
  return s === 'Ok' ? 'success' : s === 'Error' ? 'danger' : 'info'
}

// ==================== 采集管理 ====================
const spcTasks = ref<SPCDataCollectionInfo[]>([])
const spcLoading = ref(false)

async function loadSpcTasks() {
  spcLoading.value = true
  try {
    const res = await spcApi.getSpcDataCollections()
    if (res.IsSuccess) spcTasks.value = res.Data ?? []
    else ElMessage.error(res.Message)
  } finally {
    spcLoading.value = false
  }
}

async function startSpcTask(item: SPCDataCollectionInfo) {
  const res = await spcApi.startSpcDataCollection(item.CharacteristicId)
  if (res.IsSuccess) { ElMessage.success(res.Message); await loadSpcTasks() }
  else ElMessage.error(res.Message)
}

async function stopSpcTask(item: SPCDataCollectionInfo) {
  const res = await spcApi.stopSpcDataCollection(item.CharacteristicId)
  if (res.IsSuccess) { ElMessage.success(res.Message); await loadSpcTasks() }
  else ElMessage.error(res.Message)
}

async function toggleSpcEnabled(item: SPCDataCollectionInfo) {
  const res = await spcApi.setSpcDataCollectionEnabled({
    CharacteristicId: item.CharacteristicId, IsEnabled: !item.IsEnabled,
  })
  if (res.IsSuccess) { ElMessage.success(res.Message); await loadSpcTasks() }
  else ElMessage.error(res.Message)
}

async function startAllSpcTasks() {
  const res = await spcApi.startAllSpcDataCollections()
  if (res.IsSuccess) { ElMessage.success(res.Message); await loadSpcTasks() }
  else ElMessage.error(res.Message)
}

async function stopAllSpcTasks() {
  const res = await spcApi.stopAllSpcDataCollections()
  if (res.IsSuccess) { ElMessage.success(res.Message); await loadSpcTasks() }
  else ElMessage.error(res.Message)
}

async function reloadSpcTasks() {
  const res = await spcApi.reloadSpcDataCollections()
  if (res.IsSuccess) { ElMessage.success(res.Message); await loadSpcTasks() }
  else ElMessage.error(res.Message)
}

function spcSourceLabel(v: number) { return v === SPCDataSource.Database ? '数据库' : 'PLC' }

// ==================== 初始化 ====================
onMounted(async () => {
  await Promise.all([loadProducts(), loadParts(), loadEquipments(), loadCharacteristics(), loadSpcTasks(), loadCollectionMethods(), loadCurrentModel()])
})
</script>

<template>
  <div class="page">
    <el-tabs v-model="activeTab">
      <!-- 产品 -->
      <el-tab-pane label="产品" name="product">
        <div class="toolbar"><el-button type="primary" size="small" @click="openProductDialog()">新增产品</el-button></div>
        <el-table :data="products" border stripe size="small">
          <el-table-column prop="Id" label="ID" width="60" />
          <el-table-column prop="ModelNo" label="产品型号" min-width="150" />
          <el-table-column prop="Name" label="产品名称" min-width="150" />
          <el-table-column prop="Remark" label="备注" min-width="120" show-overflow-tooltip />
          <el-table-column label="操作" width="150" fixed="right">
            <template #default="{ row }">
              <el-button size="small" type="primary" text @click="openProductDialog(row)">编辑</el-button>
              <el-button size="small" type="danger" text @click="deleteProduct(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
        <el-dialog v-model="productDialog" title="产品" width="480px">
          <el-form label-width="80px">
            <el-form-item label="型号" required><el-input v-model="productForm.ModelNo" /></el-form-item>
            <el-form-item label="名称" required><el-input v-model="productForm.Name" /></el-form-item>
            <el-form-item label="备注"><el-input v-model="productForm.Remark" /></el-form-item>
          </el-form>
          <template #footer><el-button @click="productDialog = false">取消</el-button><el-button type="primary" @click="saveProduct">保存</el-button></template>
        </el-dialog>
      </el-tab-pane>

      <!-- 零件 -->
      <el-tab-pane label="零件" name="part">
        <div class="toolbar"><el-button type="primary" size="small" @click="openPartDialog()">新增零件</el-button></div>
        <el-table :data="parts" border stripe size="small">
          <el-table-column prop="Id" label="ID" width="60" />
          <el-table-column prop="ModelNo" label="零件型号" min-width="140" />
          <el-table-column prop="Name" label="零件名称" min-width="140" />
          <el-table-column label="关联产品" min-width="140"><template #default="{ row }">{{ row.Product?.Name ?? '-' }}</template></el-table-column>
          <el-table-column prop="PartCategory" label="零件类别" width="110" />
          <el-table-column prop="Remark" label="备注" min-width="100" show-overflow-tooltip />
          <el-table-column label="操作" width="150" fixed="right">
            <template #default="{ row }">
              <el-button size="small" type="primary" text @click="openPartDialog(row)">编辑</el-button>
              <el-button size="small" type="danger" text @click="deletePart(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
        <el-dialog v-model="partDialog" title="零件" width="480px">
          <el-form label-width="80px">
            <el-form-item label="型号" required><el-input v-model="partForm.ModelNo" /></el-form-item>
            <el-form-item label="名称" required><el-input v-model="partForm.Name" /></el-form-item>
            <el-form-item label="关联产品" required>
              <el-select v-model="partForm.ProductId" placeholder="选择产品" style="width:100%">
                <el-option v-for="p in products" :key="p.Id" :label="`${p.Name} (${p.ModelNo})`" :value="p.Id" />
              </el-select>
            </el-form-item>
            <el-form-item label="零件类别" required><el-input v-model="partForm.PartCategory" /></el-form-item>
            <el-form-item label="备注"><el-input v-model="partForm.Remark" /></el-form-item>
          </el-form>
          <template #footer><el-button @click="partDialog = false">取消</el-button><el-button type="primary" @click="savePart">保存</el-button></template>
        </el-dialog>
      </el-tab-pane>

      <!-- 设备 -->
      <el-tab-pane label="设备" name="equipment">
        <div class="toolbar"><el-button type="primary" size="small" @click="openEquipDialog()">新增设备</el-button></div>
        <el-table :data="spcEquipments" border stripe size="small">
          <el-table-column prop="Id" label="ID" width="60" />
          <el-table-column prop="Name" label="设备名称" min-width="150" />
          <el-table-column prop="ProductionLine" label="所在产线" min-width="130" />
          <el-table-column prop="ProcessNo" label="工序编号" width="110" />
          <el-table-column prop="ProcessName" label="工序名称" min-width="120" />
          <el-table-column prop="Remark" label="备注" min-width="100" show-overflow-tooltip />
          <el-table-column label="操作" width="150" fixed="right">
            <template #default="{ row }">
              <el-button size="small" type="primary" text @click="openEquipDialog(row)">编辑</el-button>
              <el-button size="small" type="danger" text @click="deleteEquip(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
        <el-dialog v-model="equipDialog" title="设备" width="480px">
          <el-form label-width="80px">
            <el-form-item label="名称" required><el-input v-model="equipForm.Name" /></el-form-item>
            <el-form-item label="所在产线" required><el-input v-model="equipForm.ProductionLine" /></el-form-item>
            <el-form-item label="工序编号" required><el-input v-model="equipForm.ProcessNo" /></el-form-item>
            <el-form-item label="工序名称" required><el-input v-model="equipForm.ProcessName" /></el-form-item>
            <el-form-item label="备注"><el-input v-model="equipForm.Remark" /></el-form-item>
          </el-form>
          <template #footer><el-button @click="equipDialog = false">取消</el-button><el-button type="primary" @click="saveEquip">保存</el-button></template>
        </el-dialog>
      </el-tab-pane>

      <!-- 特性库 -->
      <el-tab-pane label="特性库" name="characteristic">
        <div class="toolbar"><el-button type="primary" size="small" @click="openCharDialog()">新增特性</el-button></div>
        <el-table :data="characteristics" border stripe size="small">
          <el-table-column prop="Id" label="ID" width="50" />
          <el-table-column prop="Name" label="特性名称" min-width="110" />
          <el-table-column prop="CharacteristicNo" label="特性编号" width="100" />
          <el-table-column label="数据类型" width="70"><template #default="{ row }">{{ dataTypeLabel(row.DataType) }}</template></el-table-column>
          <el-table-column prop="InspectionQty" label="检测数量" width="70" />
          <el-table-column prop="CollectionMethod" label="采集方式" width="90" />
          <el-table-column label="数据来源" width="80">
            <template #default="{ row }">
              <el-tag :type="row.DataSource === SPCDataSource.Database ? 'success' : 'warning'" size="small">{{ dataSourceLabel(row.DataSource) }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="零件类别" min-width="100">
              <template #default="{ row }">{{ row.PartCategory || row.Part?.PartCategory || '-' }}</template>
            </el-table-column>
          <el-table-column label="产品（解析）" min-width="100"><template #default="{ row }">{{ row.Product?.Name ?? '-' }}</template></el-table-column>
          <el-table-column label="零件（解析）" min-width="100"><template #default="{ row }">{{ row.Part?.Name ?? '-' }}</template></el-table-column>
          <el-table-column label="设备" min-width="100"><template #default="{ row }">{{ row.Equipment?.Name ?? '-' }}</template></el-table-column>
          <el-table-column label="操作" width="220" fixed="right">
            <template #default="{ row }">
              <el-button size="small" type="primary" text @click="openCharDialog(row)">编辑</el-button>
              <el-button size="small" type="success" text @click="openSourceConfig(row)">来源配置</el-button>
              <el-button size="small" type="danger" text @click="deleteChar(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>

        <!-- 新增/编辑特性弹窗 -->
        <el-dialog v-model="charDialog" title="特性" width="680px" top="5vh">
          <el-form label-width="80px">
            <el-form-item label="名称" required><el-input v-model="charForm.Name" /></el-form-item>
            <el-form-item label="编号" required><el-input v-model="charForm.CharacteristicNo" /></el-form-item>
            <el-form-item label="数据类型" required>
              <el-select v-model="charForm.DataType" style="width:100%">
                <el-option label="数值" :value="SPCDataType.Numeric" />
                <el-option label="离散" :value="SPCDataType.Discrete" />
              </el-select>
            </el-form-item>
            <el-form-item label="检测数量" required><el-input-number v-model="charForm.InspectionQty" :min="0" /></el-form-item>
            <el-form-item label="采集方式" required>
              <el-select v-model="charForm.CollectionMethod" placeholder="选择采集方式" style="width:100%">
                <el-option v-for="m in collectionMethodOptions" :key="m.value" :label="m.label" :value="m.value" />
              </el-select>
            </el-form-item>
            <el-form-item label="数据来源" required>
              <el-select v-model="charForm.DataSource" style="width:100%" @change="charForm.CollectionMethod = ''">
                <el-option label="数据库" :value="SPCDataSource.Database" />
                <el-option label="PLC" :value="SPCDataSource.PLC" />
              </el-select>
            </el-form-item>
            <el-form-item label="零件类别" required>
              <el-select
                v-model="charForm.PartCategory"
                placeholder="选择或输入零件类别"
                style="width:100%"
                filterable
                allow-create
                default-first-option
              >
                <el-option v-for="c in partCategoryOptions" :key="c" :label="c" :value="c" />
              </el-select>
            </el-form-item>
            <el-form-item label="关联设备" required>
              <el-select v-model="charForm.EquipmentId" placeholder="选择设备" style="width:100%">
                <el-option v-for="e in spcEquipments" :key="e.Id" :label="e.Name" :value="e.Id" />
              </el-select>
            </el-form-item>
            <el-form-item label="备注"><el-input v-model="charForm.Remark" /></el-form-item>
          </el-form>

          <!-- 数据来源 = 数据库 -->
          <el-divider v-if="charForm.DataSource === SPCDataSource.Database" content-position="left">数据库来源配置</el-divider>
          <el-form v-if="charForm.DataSource === SPCDataSource.Database" label-width="100px">
            <el-form-item label="连接字符串" required>
              <el-input v-model="dbSourceForm.ConnectionString" placeholder="Server=...;Database=...;Uid=...;Pwd=..." />
            </el-form-item>
            <el-form-item label="表名" required>
              <el-input v-model="dbSourceForm.TableName" placeholder="查询的表名" />
            </el-form-item>
            <el-form-item label="测量值字段" required>
              <el-input v-model="dbSourceForm.MeasureValueField" placeholder="测量值列名" />
            </el-form-item>
            <el-form-item label="测量结果字段" required>
              <el-input v-model="dbSourceForm.MeasureResultField" placeholder="测量结果列名" />
            </el-form-item>
            <el-form-item label="时间字段" required>
              <el-input v-model="dbSourceForm.TimeFieldName" placeholder="WHERE 时间列名" />
            </el-form-item>
            <el-form-item v-if="dbSourceSaved" label="上次采集">
              <el-input :model-value="dbSourceForm.LastCollectionTime || '暂无'" disabled />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" size="small" @click="saveDbSource">保存来源配置</el-button>
              <span v-if="dbSourceSaved" class="save-hint">已保存</span>
            </el-form-item>
          </el-form>

          <!-- 数据来源 = PLC：两个 Tab + 已存点位列表 -->
          <el-divider v-if="charForm.DataSource === SPCDataSource.PLC" content-position="left">PLC 点位配置</el-divider>
          <div v-if="charForm.DataSource === SPCDataSource.PLC">
            <el-tabs v-model="plcTab" type="border-card" style="margin-bottom: 12px;">
              <!-- Tab 1: 手动录入 -->
              <el-tab-pane label="手动录入" name="manual">
                <el-form label-width="90px" inline>
                  <el-form-item label="PLC IP" required><el-input v-model="plcManualForm.IpAddress" placeholder="192.168.1.100" size="small" style="width:140px" /></el-form-item>
                  <el-form-item label="端口" required><el-input-number v-model="plcManualForm.Port" :min="1" :max="65535" size="small" /></el-form-item>
                  <el-form-item label="PLC类型"><el-input v-model="plcManualForm.PlcType" size="small" style="width:100px" /></el-form-item>
                  <el-form-item label="变量名" required><el-input v-model="plcManualForm.PointName" placeholder="点位名" size="small" style="width:140px" /></el-form-item>
                  <el-form-item label="区块" required>
                    <el-select v-model="plcManualForm.Area" size="small" style="width:80px">
                      <el-option label="D" value="D" /><el-option label="M" value="M" />
                      <el-option label="X" value="X" /><el-option label="Y" value="Y" /><el-option label="R" value="R" />
                    </el-select>
                  </el-form-item>
                  <el-form-item label="地址" required><el-input v-model="plcManualForm.Address" placeholder="D100" size="small" style="width:100px" /></el-form-item>
                  <el-form-item label="数据类型" required>
                    <el-select v-model="plcManualForm.DataType" size="small" style="width:100px">
                      <el-option label="Int16" value="Int16" /><el-option label="Int32" value="Int32" />
                      <el-option label="Float" value="Float" /><el-option label="Bool" value="Bool" />
                      <el-option label="String" value="String" />
                    </el-select>
                  </el-form-item>
                  <el-form-item label="长度" required><el-input-number v-model="plcManualForm.Length" :min="1" size="small" /></el-form-item>
                  <el-form-item>
                    <el-button type="primary" size="small" @click="addManualRow">添加</el-button>
                    <el-button type="success" size="small" :disabled="plcManualRows.length === 0" @click="saveAllManualRows">保存全部 ({{ plcManualRows.length }})</el-button>
                  </el-form-item>
                </el-form>

                <!-- 待保存的点位行 -->
                <el-table v-if="plcManualRows.length > 0" :data="plcManualRows" border stripe size="small" max-height="200">
                  <el-table-column prop="IpAddress" label="IP" width="120" />
                  <el-table-column prop="PointName" label="变量名" min-width="100" />
                  <el-table-column prop="Area" label="区块" width="60" />
                  <el-table-column prop="Address" label="地址" width="80" />
                  <el-table-column prop="DataType" label="类型" width="80" />
                  <el-table-column prop="Length" label="长度" width="60" />
                  <el-table-column label="操作" width="70">
                    <template #default="{ $index }">
                      <el-button size="small" type="danger" text @click="removeManualRow($index)">删除</el-button>
                    </template>
                  </el-table-column>
                </el-table>
              </el-tab-pane>

              <!-- Tab 2: Excel 导入 -->
              <el-tab-pane label="Excel 导入" name="excel">
                <el-alert type="info" :closable="false" style="margin-bottom: 10px;">
                  <template #title>
                    Excel 列：IP | 端口 | PLC类型 | 变量名 | 区块 | 地址 | 数据类型 | 长度（第1行表头，第2行起数据）
                  </template>
                </el-alert>
                <input ref="excelInputRef" type="file" accept=".xlsx" style="display:none" @change="onExcelFileChange" />
                <el-button type="primary" size="small" :loading="excelImporting" :disabled="!charForm.Id" @click="openExcelPicker">
                  选择 Excel 文件导入
                </el-button>
                <span v-if="!charForm.Id" class="save-hint">请先保存特性</span>

                <!-- 设备现有点位关联 -->
                <el-divider content-position="left">从设备导入已有点位</el-divider>
                <el-button size="small" :disabled="!charForm.EquipmentId" @click="importEquipmentPlcPoints">加载设备点位</el-button>
                <span v-if="!charForm.EquipmentId" class="save-hint">请先选择关联设备</span>
                <el-table v-if="availablePlcPoints.length > 0" :data="availablePlcPoints" border stripe size="small" max-height="200" style="margin-top:8px" @row-click="(row: TbPlcPoint) => selectedPlcPointId = row.Id">
                  <el-table-column width="50">
                    <template #default="{ row }"><el-radio :model-value="selectedPlcPointId" :value="row.Id" @change="selectedPlcPointId = row.Id" /></template>
                  </el-table-column>
                  <el-table-column prop="PointName" label="变量名" min-width="100" />
                  <el-table-column prop="Area" label="区块" width="60" />
                  <el-table-column prop="Address" label="地址" width="80" />
                  <el-table-column prop="DataType" label="类型" width="80" />
                </el-table>
                <el-button v-if="availablePlcPoints.length > 0" type="success" size="small" style="margin-top:6px" @click="confirmImportPoint">关联选中点位</el-button>
              </el-tab-pane>
            </el-tabs>

            <!-- 已保存的点位列表 -->
            <el-divider content-position="left">已保存点位 ({{ savedPlcPoints.length }})</el-divider>
            <el-table v-if="savedPlcPoints.length > 0" :data="savedPlcPoints" border stripe size="small" max-height="200">
              <el-table-column prop="PointName" label="变量名" min-width="100" />
              <el-table-column prop="Area" label="区块" width="60" />
              <el-table-column prop="Address" label="地址" width="80" />
              <el-table-column prop="DataType" label="类型" width="80" />
              <el-table-column prop="Length" label="长度" width="60" />
              <el-table-column prop="IpAddress" label="IP" width="120" />
              <el-table-column label="来源" width="80">
                <template #default="{ row }">
                  <el-tag v-if="row.PlcPointId > 0" type="info" size="small">引用</el-tag>
                  <el-tag v-else type="success" size="small">手动</el-tag>
                </template>
              </el-table-column>
              <el-table-column label="操作" width="70">
                <template #default="{ row }">
                  <el-button size="small" type="danger" text @click="removeSavedPoint(row.Id)">删除</el-button>
                </template>
              </el-table-column>
            </el-table>
            <el-empty v-else description="暂无已保存点位" :image-size="40" />
          </div>

          <template #footer>
            <el-button @click="charDialog = false">取消</el-button>
            <el-button type="primary" @click="saveChar">保存</el-button>
          </template>
        </el-dialog>

        <!-- 来源配置弹窗（编辑已有特性时，数据库来源） -->
        <el-dialog v-model="sourceDialog" title="数据库采集源" width="520px">
          <el-form label-width="100px">
            <el-form-item label="连接字符串" required>
              <el-input v-model="sourceForm.ConnectionString" placeholder="Server=...;Database=...;Uid=...;Pwd=..." />
            </el-form-item>
            <el-form-item label="表名" required>
              <el-input v-model="sourceForm.TableName" />
            </el-form-item>
            <el-form-item label="测量值字段" required>
              <el-input v-model="sourceForm.MeasureValueField" />
            </el-form-item>
            <el-form-item label="测量结果字段" required>
              <el-input v-model="sourceForm.MeasureResultField" />
            </el-form-item>
            <el-form-item label="时间字段" required>
              <el-input v-model="sourceForm.TimeFieldName" />
            </el-form-item>
            <el-form-item label="上次采集时间">
              <el-input :model-value="sourceForm.LastCollectionTime || '暂无'" disabled />
            </el-form-item>
          </el-form>
          <template #footer><el-button @click="sourceDialog = false">取消</el-button><el-button type="primary" @click="saveSource">保存</el-button></template>
        </el-dialog>
      </el-tab-pane>
      <!-- 采集管理 -->
      <el-tab-pane label="采集管理" name="spc-collection">
        <div class="toolbar">
          <span class="current-model-tag">
            当前生产型号：
            <el-tag :type="currentModelStatusType(currentModel?.SyncStatus)" size="small">
              {{ currentModel?.ModelNo || '未同步' }}
              <template v-if="currentModel?.SyncStatus && currentModel.SyncStatus !== 'Ok'">
                （{{ currentModel.SyncStatus }}）
              </template>
            </el-tag>
            <span v-if="currentModel?.LastSyncTime" class="sync-time">{{ currentModel.LastSyncTime }}</span>
          </span>
          <el-button size="small" @click="syncCurrentModel">立即同步型号</el-button>
          <el-button type="success" size="small" @click="startAllSpcTasks">启动全部</el-button>
          <el-button type="warning" size="small" @click="stopAllSpcTasks">停止全部</el-button>
          <el-button size="small" @click="reloadSpcTasks">重新加载</el-button>
          <el-button size="small" @click="() => { loadSpcTasks(); loadCurrentModel() }">刷新</el-button>
        </div>
        <el-table v-loading="spcLoading" :data="spcTasks" border stripe size="small">
          <el-table-column prop="CharacteristicId" label="特性ID" width="70" />
          <el-table-column prop="FeatureName" label="特性名称" min-width="130" />
          <el-table-column label="数据来源" width="80">
            <template #default="{ row }">
              <el-tag :type="row.DataSource === SPCDataSource.Database ? 'success' : 'warning'" size="small">
                {{ spcSourceLabel(row.DataSource) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="Description" label="采集方式描述" min-width="140" show-overflow-tooltip />
          <el-table-column label="解析型号" width="110">
            <template #default="{ row }">{{ row.ResolvedModelNo || '-' }}</template>
          </el-table-column>
          <el-table-column label="解析零件" min-width="120">
            <template #default="{ row }">
              <el-tooltip v-if="row.ResolveReason" :content="row.ResolveReason" placement="top">
                <el-tag type="danger" size="small">解析失败</el-tag>
              </el-tooltip>
              <template v-else>{{ row.ResolvedPartName || '-' }}</template>
            </template>
          </el-table-column>
          <el-table-column label="状态" width="90">
            <template #default="{ row }">
              <el-tag :type="row.IsRunning ? 'success' : 'info'" size="small">
                {{ row.IsRunning ? '运行中' : '已停止' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="启用" width="80">
            <template #default="{ row }">
              <el-switch :model-value="row.IsEnabled" size="small" @change="toggleSpcEnabled(row)" />
            </template>
          </el-table-column>
          <el-table-column label="采集间隔" width="110">
            <template #default="{ row }">{{ (row.IntervalMs / 1000).toFixed(0) }} 秒</template>
          </el-table-column>
          <el-table-column label="操作" width="180" fixed="right">
            <template #default="{ row }">
              <el-button
                v-if="!row.IsRunning"
                size="small"
                type="success"
                :disabled="!row.IsEnabled"
                @click="startSpcTask(row)"
              >启动</el-button>
              <el-button
                v-else
                size="small"
                type="danger"
                @click="stopSpcTask(row)"
              >停止</el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<style scoped>
.page { padding: 12px; }
.toolbar { margin-bottom: 12px; display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.current-model-tag { display: inline-flex; align-items: center; gap: 6px; margin-right: 8px; font-size: 13px; color: #606266; }
.sync-time { color: #909399; font-size: 12px; }
.save-hint { margin-left: 8px; color: var(--el-color-success); font-size: 13px; }
</style>
