import type { StorageLike } from "pinia-plugin-persistedstate";

/**
 * @description pinia 持久化参数配置
 * @param {String} key 存储到持久化的 name
 * @param {Array} pick 需要持久化的 state name
 *   （pinia-plugin-persistedstate v4 起将 paths 更名为 pick）
 * @return persist
 *
 * 注意：此处故意使用非泛型的结构类型，而不是插件的 PersistenceOptions<S>。
 * 后者带有泛型参数 S，写在 defineStore 的 persist 选项里会参与泛型推断，
 * 把 store 的 S 推断成兜底的 StateTree，导致 this.xxx 全部退化为 any。
 * */
type PiniaPersistOptions = {
  key: string;
  storage: StorageLike;
  pick?: string[];
};

const piniaPersistConfig = (key: string, pick?: string[]): PiniaPersistOptions => {
  return {
    key,
    storage: localStorage,
    // storage: sessionStorage,
    pick
  };
};

export default piniaPersistConfig;
