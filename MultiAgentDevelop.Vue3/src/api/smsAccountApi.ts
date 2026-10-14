import { get, post, put, patch, del, type BaseResponse, type PageModel } from '@/utils/axiosInstance';

/**
 * 短信账号 - 列表 / 详情
 * 字段大小写与后端 C# 属性保持一致（后端未做 camelCase 转换）
 * Id 为 long，经 NumberConverter 序列化为字符串
 */
export interface SmsAccount {
  Id: string;
  AccountName: string;
  ProviderId: string;
  ProviderName: string | null;
  ProviderType: string | null;
  ProviderStatus: boolean;
  AccessKey: string | null;
  AccessSecret: string | null;
  SignName: string | null;
  Balance: number;
  WarnThreshold: number;
  Warned: boolean;
  Status: boolean;
  Remark: string | null;
  CreateTime: string;
}

/**
 * 短信账号 - 分页查询入参
 */
export interface SmsAccountQuery {
  page: number;
  intPageSize: number;
  accountName?: string;
  providerId?: string | number | null;
  status?: boolean | null;
}

/**
 * 短信账号 - 新增 / 修改入参
 */
export interface SmsAccountSave {
  Id: string;
  AccountName: string;
  ProviderId: string;
  AccessKey: string;
  AccessSecret: string;
  SignName: string;
  Balance: number;
  WarnThreshold: number;
  Status: boolean;
  Remark: string;
}

/**
 * 供应商下拉项
 */
export interface SmsProviderOption {
  Id: string;
  ProviderName: string;
  ProviderType: string;
  Status: boolean;
}

/**
 * 连通性测试结果
 */
export interface SmsAccountTestResult {
  Success: boolean;
  Balance: number | null;
  Message: string;
}

// 分页查询短信账号
export const getSmsAccountPageApi = async (params: SmsAccountQuery): Promise<BaseResponse<PageModel<SmsAccount>>> => {
  try {
    const response = await get<BaseResponse<PageModel<SmsAccount>>>('/api/SmsAccount/page', params);
    return response;
  } catch (error) {
    throw new Error('请求失败');
  }
};

// 查询短信账号详情
export const getSmsAccountDetailApi = async (id: string): Promise<BaseResponse<SmsAccount>> => {
  try {
    const response = await get<BaseResponse<SmsAccount>>(`/api/SmsAccount/detail/${id}`);
    return response;
  } catch (error) {
    throw new Error('请求失败');
  }
};

// 获取供应商下拉数据
export const getSmsProviderOptionsApi = async (): Promise<BaseResponse<SmsProviderOption[]>> => {
  try {
    const response = await get<BaseResponse<SmsProviderOption[]>>('/api/SmsAccount/providerOptions');
    return response;
  } catch (error) {
    throw new Error('请求失败');
  }
};

// 新增短信账号
export const addSmsAccount = async (params: SmsAccountSave): Promise<BaseResponse<string>> => {
  try {
    const response = await post<BaseResponse<string>>('/api/SmsAccount/create', params);
    return response;
  } catch (error) {
    throw new Error('请求失败');
  }
};

// 修改短信账号
export const editSmsAccount = async (params: SmsAccountSave): Promise<BaseResponse<string>> => {
  try {
    const response = await put<BaseResponse<string>>('/api/SmsAccount/update', params);
    return response;
  } catch (error) {
    throw new Error('请求失败');
  }
};

// 删除短信账号
export const removeSmsAccount = async (id: string): Promise<BaseResponse<string>> => {
  try {
    const response = await del<BaseResponse<string>>(`/api/SmsAccount/remove/${id}`);
    return response;
  } catch (error) {
    throw new Error('请求失败');
  }
};

// 启用 / 禁用短信账号
export const changeSmsAccountStatus = async (id: string, status: boolean): Promise<BaseResponse<string>> => {
  try {
    const response = await patch<BaseResponse<string>>(`/api/SmsAccount/status/${id}/${status}`, {});
    return response;
  } catch (error) {
    throw new Error('请求失败');
  }
};

// 连通性测试
export const testSmsAccount = async (id: string): Promise<BaseResponse<SmsAccountTestResult>> => {
  try {
    const response = await post<BaseResponse<SmsAccountTestResult>>(`/api/SmsAccount/test/${id}`, {});
    return response;
  } catch (error) {
    throw new Error('请求失败');
  }
};
