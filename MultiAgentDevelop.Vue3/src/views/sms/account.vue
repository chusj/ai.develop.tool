<template>
    <section>
        <!-- 查询区 -->
        <el-form :inline="true" class="query-form" @submit.prevent>
            <el-form-item label="账号名称">
                <el-input v-model="filters.accountName" clearable placeholder="支持模糊查询" style="width: 200px"
                    @keyup.enter="handleQuery()" />
            </el-form-item>
            <el-form-item label="绑定供应商">
                <el-select v-model="filters.providerId" clearable placeholder="全部" style="width: 200px">
                    <el-option v-for="item in providerOptions" :key="item.Id"
                        :label="item.Status ? item.ProviderName : `${item.ProviderName}（已禁用）`" :value="item.Id" />
                </el-select>
            </el-form-item>
            <el-form-item label="状态">
                <el-select v-model="filters.status" clearable placeholder="全部" style="width: 140px">
                    <el-option label="启用" :value="true" />
                    <el-option label="禁用" :value="false" />
                </el-select>
            </el-form-item>
            <el-form-item>
                <el-button type="primary" @click="handleQuery()">查询</el-button>
                <el-button @click="handleReset">重置</el-button>
            </el-form-item>
        </el-form>

        <!-- 工具栏（新增 / 编辑 / 删除 / 启停 / 测试，由菜单按钮权限驱动） -->
        <toolbar :button-list="buttonList"></toolbar>

        <!-- 列表 -->
        <el-table :data="accounts" ref="tableRef" v-loading="listLoading" border row-key="Id"
            :row-class-name="tableRowClassName" @selection-change="handleSelectionChange" style="width: 100%">
            <el-table-column type="selection" width="50"></el-table-column>
            <el-table-column prop="AccountName" label="账号名称" min-width="150" show-overflow-tooltip></el-table-column>
            <el-table-column label="绑定供应商" min-width="160" show-overflow-tooltip>
                <template #default="{ row }">
                    <span>{{ row.ProviderName || '-' }}</span>
                    <el-tag v-if="row.ProviderName && !row.ProviderStatus" type="warning" size="small"
                        style="margin-left: 6px">已禁用</el-tag>
                </template>
            </el-table-column>
            <el-table-column prop="AccessKey" label="密钥 ID" min-width="150" show-overflow-tooltip></el-table-column>
            <el-table-column prop="SignName" label="默认签名" min-width="110"></el-table-column>
            <el-table-column label="剩余额度" width="130" align="right">
                <template #default="{ row }">
                    <span>{{ row.Balance.toFixed(2) }}</span>
                    <el-tooltip v-if="row.Warned" content="余额已低于预警阈值" placement="top">
                        <el-icon color="#E6A23C" style="margin-left: 4px; vertical-align: middle">
                            <WarningFilled />
                        </el-icon>
                    </el-tooltip>
                </template>
            </el-table-column>
            <el-table-column label="预警阈值" width="110" align="right">
                <template #default="{ row }">
                    <span>{{ row.WarnThreshold > 0 ? row.WarnThreshold.toFixed(2) : '不预警' }}</span>
                </template>
            </el-table-column>
            <el-table-column prop="Status" label="状态" width="90">
                <template #default="{ row }">
                    <el-tag :type="row.Status ? 'success' : 'info'">{{ row.Status ? '启用' : '禁用' }}</el-tag>
                </template>
            </el-table-column>
            <el-table-column prop="CreateTime" label="创建时间" width="170" sortable></el-table-column>
            <el-table-column label="操作" width="290" fixed="right">
                <template #default="{ row }">
                    <el-button link type="primary" @click="onRowAction(row, handleEdit)">编辑</el-button>
                    <el-button link type="primary" @click="onRowAction(row, handleStatus)">
                        {{ row.Status ? '禁用' : '启用' }}
                    </el-button>
                    <el-button link type="primary" @click="onRowAction(row, handleTest)">连通测试</el-button>
                    <el-button link type="danger" @click="onRowAction(row, handleDel)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>

        <!-- 分页 -->
        <el-pagination class="pagination" background layout="total, sizes, prev, pager, next, jumper"
            :total="dataCount" :current-page="page" :page-size="pageSize" :page-sizes="[10, 20, 50, 100]"
            @current-change="handlePageChange" @size-change="handleSizeChange" />

        <!-- 新增 / 编辑 -->
        <el-dialog :title="dialogTitle" v-model="dialogVisible" :close-on-click-modal="false" width="620px">
            <el-form :model="form" :rules="formRules" ref="formRef" label-width="120px" v-loading="dialogLoading">
                <el-form-item label="账号名称" prop="AccountName">
                    <el-input v-model="form.AccountName" maxlength="64" show-word-limit auto-complete="off" />
                </el-form-item>
                <el-form-item label="绑定供应商" prop="ProviderId">
                    <el-select v-model="form.ProviderId" filterable placeholder="请选择供应商" style="width: 100%">
                        <el-option v-for="item in providerOptions" :key="item.Id"
                            :label="item.Status ? item.ProviderName : `${item.ProviderName}（已禁用）`"
                            :value="item.Id" />
                    </el-select>
                </el-form-item>
                <el-form-item label="AccessKey" prop="AccessKey">
                    <el-input v-model="form.AccessKey" maxlength="128" auto-complete="off" />
                </el-form-item>
                <el-form-item label="AccessSecret" prop="AccessSecret">
                    <el-input v-model="form.AccessSecret" type="password" show-password auto-complete="new-password"
                        :placeholder="isEdit ? '留空表示不修改' : '请输入密钥'" />
                </el-form-item>
                <el-form-item label="默认签名" prop="SignName">
                    <el-input v-model="form.SignName" maxlength="64" placeholder="留空则取供应商签名" />
                </el-form-item>
                <el-form-item label="初始额度" prop="Balance">
                    <el-input-number v-model="form.Balance" :min="0" :precision="2" :disabled="isEdit"
                        controls-position="right" style="width: 100%" />
                    <div v-if="isEdit" class="form-tip">余额由充值功能维护，不可直接修改</div>
                </el-form-item>
                <el-form-item label="余额预警阈值" prop="WarnThreshold">
                    <el-input-number v-model="form.WarnThreshold" :min="0" :precision="2" controls-position="right"
                        style="width: 100%" />
                    <div class="form-tip">余额小于等于该值时列表标黄提示；填 0 表示不预警</div>
                </el-form-item>
                <el-form-item label="状态" prop="Status">
                    <el-switch v-model="form.Status" active-text="启用" inactive-text="禁用" inline-prompt />
                </el-form-item>
                <el-form-item label="备注" prop="Remark">
                    <el-input v-model="form.Remark" type="textarea" :rows="2" maxlength="255" show-word-limit />
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="dialogVisible = false">取消</el-button>
                <el-button type="primary" @click="submitForm" :loading="dialogLoading">提交</el-button>
            </template>
        </el-dialog>
    </section>
</template>

<script setup lang="ts" name="smsAccount">
import { ref, onMounted, onUnmounted } from 'vue';
import { ElTable } from 'element-plus';
import { WarningFilled } from '@element-plus/icons-vue';
import Toolbar from '@/components/toolbar.vue';
import mittBusT from '@/utils/mittBusT';
import { getButtonList } from '@/utils';
import { useAuthMenuStore } from '@/stores/modules/authMenu';
import type { SmsAccount } from '@/api/smsAccountApi';

import {
    accounts, listLoading, currentRow, page, pageSize, dataCount, filters, providerOptions,
    dialogVisible, dialogTitle, dialogLoading, isEdit, formRef, form, formRules,
    handleQuery, handleReset, handlePageChange, handleSizeChange, handleSelectionChange,
    loadProviderOptions, handleAdd, handleEdit, handleDel, handleStatus, handleTest, submitForm
} from './smsAccountFunctions';

const buttonList = ref<Menu.MenuOptions[]>([]);
const tableRef = ref<InstanceType<typeof ElTable> | null>(null);

// 余额预警行标黄
const tableRowClassName = ({ row }: { row: SmsAccount }) => (row.Warned ? 'row-warned' : '');

// 操作列直接操作当前行
const onRowAction = async (row: SmsAccount, action: () => Promise<void>) => {
    currentRow.value = row;
    if (tableRef.value) {
        tableRef.value.clearSelection();
        tableRef.value.toggleRowSelection(row, true);
    }
    await action();
};

// 按钮 Func → 处理函数映射
const functionMap: Record<string, (keyword?: string) => void | Promise<void>> = {
    handleQuery,
    handleAdd,
    handleEdit,
    handleDel,
    handleStatus,
    handleTest
};

const callFunction = (item: Menu.MenuOptions) => {
    if (item.Func && typeof item.Func === 'string') {
        const func = functionMap[item.Func];
        if (typeof func === 'function') {
            // 工具栏搜索框的值作为账号名称关键字
            func(item.search);
        } else {
            console.error(`Function ${item.Func} is not defined in functionMap.`);
        }
    } else {
        console.error('Func property is not a valid string.');
    }
};

onMounted(async () => {
    const authStore = useAuthMenuStore();
    buttonList.value = getButtonList(window.location.pathname, authStore.authMenuListGet);

    mittBusT.on('callFunction', callFunction);

    await loadProviderOptions();
    await handleQuery();
});

onUnmounted(() => {
    mittBusT.off('callFunction', callFunction);
});
</script>

<style scoped>
.query-form {
    margin-bottom: 8px;
}

.pagination {
    margin-top: 16px;
    justify-content: flex-end;
}

.form-tip {
    font-size: 12px;
    color: #909399;
    line-height: 18px;
}

:deep(.row-warned) {
    background-color: #fdf6ec;
}

:deep(.row-warned td) {
    background-color: #fdf6ec !important;
}
</style>
