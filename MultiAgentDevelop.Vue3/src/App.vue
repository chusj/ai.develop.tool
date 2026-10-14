<template>
  <el-config-provider :locale="locale" :button="buttonConfig">
    <router-view></router-view>
  </el-config-provider>
</template>

<script setup lang="ts">
import { reactive } from "vue";
import { ElConfigProvider } from "element-plus";
import zhCn from "element-plus/es/locale/lang/zh-cn";

// element 语言包（中文）
const locale = zhCn;

// element button config
const buttonConfig = reactive({ autoInsertSpace: false });
</script>
