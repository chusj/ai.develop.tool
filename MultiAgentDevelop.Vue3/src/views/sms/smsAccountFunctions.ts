// smsAccountFunctions.ts — 短信账号管理页面的状态与操作

import { reactive, toRaw, ref, computed } from 'vue';
import { ElMessage, ElMessageBox, ElForm } from 'element-plus';
import {
  getSmsAccountPageApi,
  getSmsAccountDetailApi,
  getSmsProviderOptionsApi,
  addSmsAccount,
  editSmsAccount,
  removeSmsAccount,
  changeSmsAccountStatus,
  testSmsAccount
} from '@/api/smsAccountApi';
import type { SmsAccount, SmsAccountSave, SmsProviderOption } from '@/api/smsAccountApi';

// ↓↓↓↓↓ 列表状态 ↓↓↓↓↓
export const accounts = ref<SmsAccount[]>([]);
export const listLoading = ref(false);
export const currentRow = ref<SmsAccount | null>(null);
export const page = ref(1);
export const pageSize = ref(20);
export const dataCount = ref(0);

// 查询条件
export const filters = reactive<{
  accountName: string;
  providerId: string | number | null;
  status: boolean | null;
}>({
  accountName: '',
  providerId: null,
  status: null
});

// 供应商下拉数据
export const providerOptions = ref<SmsProviderOption[]>([]);

// ↓↓↓↓↓ 弹窗状态 ↓↓↓↓↓
export const dialogVisible = ref(false);
export const dialogTitle = ref('新增');
export const dialogLoading = ref(false);
export const isEdit = ref(false);
export const formRef = ref<InstanceType<typeof ElForm> | null>(null);

/**
 * 空表单
 */
const emptyForm = (): SmsAccountSave => ({
  Id: '',
  AccountName: '',
  ProviderId: '',
  AccessKey: '',
  AccessSecret: '',
  SignName: '',
  Balance: 0,
  WarnThreshold: 0,
  Status: true,
  Remark: ''
});

export const form = reactive<SmsAccountSave>(emptyForm());

/**
 * 表单校验规则
 * 编辑时 AccessSecret 留空表示不修改，因此动态去掉必填
 */
export const formRules = computed(() => ({
  AccountName: [
    { required: true, message: '请输入账号名称', trigger: 'blur' },
    { min: 2, max: 64, message: '长度在 2 到 64 个字符', trigger: 'blur' }
  ],
  ProviderId: [{ required: true, message: '请选择绑定的供应商', trigger: 'change' }],
  AccessKey: [{ required: true, message: '请输入 AccessKey', trigger: 'blur' }],
  AccessSecret: isEdit.value
    ? []
    : [{ required: true, message: '请输入 AccessSecret', trigger: 'blur' }],
  Balance: [{ required: true, message: '请输入初始额度', trigger: 'blur' }],
  WarnThreshold: [{ required: true, message: '请输入余额预警阈值', trigger: 'blur' }]
}));

// ↓↓↓↓↓ 查询 ↓↓↓↓↓
/**
 * 加载列表
 * @param keyword 可选的账号名称关键字（来自工具栏搜索框）
 */
export const handleQuery = async (keyword?: string) => {
  if (keyword !== undefined && keyword !== '') {
    filters.accountName = keyword;
  }

  currentRow.value = null;
  listLoading.value = true;
  try {
    const { response } = await getSmsAccountPageApi({
      page: page.value,
      intPageSize: pageSize.value,
      accountName: filters.accountName || undefined,
      providerId: filters.providerId ?? undefined,
      status: filters.status
    });
    accounts.value = response?.data ?? [];
    dataCount.value = response?.dataCount ?? 0;
  } catch (error) {
    ElMessage.error('加载短信账号列表失败');
    accounts.value = [];
    dataCount.value = 0;
  } finally {
    listLoading.value = false;
  }
};

/**
 * 重置查询条件
 */
export const handleReset = async () => {
  filters.accountName = '';
  filters.providerId = null;
  filters.status = null;
  page.value = 1;
  await handleQuery();
};

/**
 * 加载供应商下拉数据
 */
export const loadProviderOptions = async () => {
  try {
    const { response } = await getSmsProviderOptionsApi();
    providerOptions.value = response ?? [];
  } catch (error) {
    providerOptions.value = [];
  }
};

/**
 * 分页切换
 */
export const handlePageChange = async (val: number) => {
  page.value = val;
  await handleQuery();
};

export const handleSizeChange = async (val: number) => {
  pageSize.value = val;
  page.value = 1;
  await handleQuery();
};

/**
 * 单选行
 */
export const handleSelectionChange = (selection: SmsAccount[]) => {
  currentRow.value = selection.length > 0 ? selection[0] : null;
};

/**
 * 供表格「操作」列直接定位当前行
 */
export const selectRow = (row: SmsAccount) => {
  currentRow.value = row;
};

// ↓↓↓↓↓ 新增 ↓↓↓↓↓
export const handleAdd = async () => {
  await loadProviderOptions();
  Object.assign(form, emptyForm());
  isEdit.value = false;
  dialogTitle.value = '新增短信账号';
  dialogVisible.value = true;
  formRef.value?.clearValidate();
};

// ↓↓↓↓↓ 编辑 ↓↓↓↓↓
export const handleEdit = async () => {
  if (!currentRow.value) {
    ElMessage.error('请选择要编辑的一行数据！');
    return;
  }

  await loadProviderOptions();
  const id = currentRow.value.Id;

  dialogLoading.value = true;
  try {
    const { response } = await getSmsAccountDetailApi(id);
    if (!response) {
      ElMessage.error('账号不存在');
      return;
    }

    Object.assign(form, emptyForm());
    form.Id = response.Id;
    form.AccountName = response.AccountName;
    form.ProviderId = response.ProviderId;
    form.AccessKey = response.AccessKey ?? '';
    // 详情接口返回掩码，编辑时留空表示不修改
    form.AccessSecret = '';
    form.SignName = response.SignName ?? '';
    form.Balance = response.Balance;
    form.WarnThreshold = response.WarnThreshold;
    form.Status = response.Status;
    form.Remark = response.Remark ?? '';

    isEdit.value = true;
    dialogTitle.value = '编辑短信账号';
    dialogVisible.value = true;
    formRef.value?.clearValidate();
  } catch (error) {
    ElMessage.error('加载账号详情失败');
  } finally {
    dialogLoading.value = false;
  }
};

// ↓↓↓↓↓ 提交（新增 / 编辑共用） ↓↓↓↓↓
export const submitForm = async () => {
  const formEl = formRef.value;
  if (!formEl) return;

  await formEl.validate(async (isValid) => {
    if (!isValid) {
      ElMessage.error('验证失败，请检查输入项');
      return;
    }

    // 编辑时密钥留空 = 不修改，跳过必填校验
    if (isEdit.value && !form.AccessSecret) {
      // no-op：后端按空值处理
    }

    const postData = toRaw(form) as SmsAccountSave;
    dialogLoading.value = true;
    try {
      const { success, msg } = isEdit.value
        ? await editSmsAccount(postData)
        : await addSmsAccount(postData);

      if (success) {
        ElMessage.success(isEdit.value ? '修改成功' : '新增成功');
        dialogVisible.value = false;
        await handleQuery();
      } else {
        ElMessage.error(msg || '提交失败');
      }
    } catch (error) {
      ElMessage.error('提交失败');
    } finally {
      dialogLoading.value = false;
    }
  });
};

// ↓↓↓↓↓ 删除 ↓↓↓↓↓
export const handleDel = async () => {
  if (!currentRow.value) {
    ElMessage.error('请选择要删除的一行数据！');
    return;
  }

  const row = currentRow.value;
  ElMessageBox.confirm(`确认删除账号「${row.AccountName}」吗？`, '温馨提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(async () => {
    try {
      const { success, msg } = await removeSmsAccount(row.Id);
      if (success) {
        ElMessage.success('删除成功');
        await handleQuery();
      } else {
        ElMessage.error(msg || '删除失败');
      }
    } catch (error) {
      ElMessage.error('删除失败');
    }
  });
};

// ↓↓↓↓↓ 启用 / 禁用 ↓↓↓↓↓
export const handleStatus = async () => {
  if (!currentRow.value) {
    ElMessage.error('请选择要操作的一行数据！');
    return;
  }

  const row = currentRow.value;
  const next = !row.Status;
  const action = next ? '启用' : '禁用';

  ElMessageBox.confirm(`确认${action}账号「${row.AccountName}」吗？`, '温馨提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(async () => {
    try {
      const { success, msg } = await changeSmsAccountStatus(row.Id, next);
      if (success) {
        ElMessage.success(msg || `${action}成功`);
        await handleQuery();
      } else {
        ElMessage.error(msg || `${action}失败`);
      }
    } catch (error) {
      ElMessage.error(`${action}失败`);
    }
  });
};

// ↓↓↓↓↓ 连通性测试 ↓↓↓↓↓
export const handleTest = async () => {
  if (!currentRow.value) {
    ElMessage.error('请选择要测试的一行数据！');
    return;
  }

  const row = currentRow.value;
  listLoading.value = true;
  try {
    const { success, msg, response } = await testSmsAccount(row.Id);
    if (!success) {
      ElMessage.error(msg || '测试请求失败');
      return;
    }

    if (response?.Success) {
      ElMessage.success(response.Message || '连通测试通过');
    } else {
      ElMessage.warning(response?.Message || '连通测试未通过');
    }
  } catch (error) {
    ElMessage.error('连通测试失败');
  } finally {
    listLoading.value = false;
  }
};

/**
 * 余额是否需要预警
 */
export const isWarned = (row: SmsAccount) => row.Warned;
