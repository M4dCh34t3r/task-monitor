import axios, { AxiosError, HttpStatusCode, type AxiosInstance, type AxiosResponse } from 'axios';
import { assertType } from '@/helpers/typeAssertHelper';
import { useAuthStore, useOverlayStore, useSetupStore } from '@/stores';
import type { ClientSetupDTO, ServiceExceptionDTO } from '@/typings';
import type { Plugin } from 'vue';
import router from './router';

const vueAxios: Plugin = {
  install(app) {
    const instance: AxiosInstance = axios.create({
      timeout: import.meta.env.DEV ? 180000 : 30000,
      paramsSerializer: { indexes: null },
      baseURL: '/Ajax'
    });

    instance.interceptors.request.use((config) => {
      config.headers.Authorization = useAuthStore().token;
      return config;
    });

    instance.interceptors.response.use(
      async (response) => {
        await new Promise((resolve) => setTimeout(resolve, useSetupStore().responseDelay));
        handleSuccessResponse(response);
        return response;
      },
      async (error) => {
        const setupStore = useSetupStore();
        await new Promise((resolve) => setTimeout(resolve, setupStore.responseDelay));
        return error.config.responseType === 'blob'
          ? handleErrorResponseBlob(error, setupStore)
          : handleErrorResponseJson(error, setupStore);
      }
    );

    app.provide('axios', instance);
  }
};

function handleErrorResponseBlob(error: AxiosError, setupStore: ClientSetupDTO) {
  if (error.response) {
    if (!resBlobModalFiltrar(error.response)) handleResponseStatus(error.response.status);
  } else useOverlayStore().showServerErrorDialog(setupStore.modalDelay);
  console.log(error.toString());
}

function handleErrorResponseJson(error: AxiosError, setupStore: ClientSetupDTO) {
  if (error.response) {
    if (!resJsonModalFiltrar(error.response)) handleResponseStatus(error.response.status);
  } else useOverlayStore().showServerErrorDialog(setupStore.modalDelay);
  console.log(error.toString());
}

function handleResponseStatus(status: HttpStatusCode) {
  if (status === HttpStatusCode.Unauthorized) {
    const overlayStore = useOverlayStore();
    if (router.currentRoute.value.name === 'Login') {
      if (!overlayStore.snbSessionExpired) overlayStore.showSessionExpiredDialog();
    } else {
      if (overlayStore.snbSessionExpired) overlayStore.snbSessionExpired = false;
      overlayStore.showSessionExpiredDialog();
    }
  } else if (status === HttpStatusCode.Forbidden) {
    router.push('/');
    useOverlayStore().showInfoMessageDialog(
      'You cannot view this content',
      'Due to security reasons you have been redirected'
    );
  } else if (status === HttpStatusCode.InternalServerError)
    useOverlayStore().showServerErrorDialog();
}

function handleSuccessResponse(response: AxiosResponse) {
  if ('authorization' in response.headers)
    useAuthStore().updateToken(response.headers['authorization']);
  resJsonModalFiltrar(response.data);
  return response;
}

async function resBlobModalFiltrar(res: AxiosResponse) {
  let obj: any;
  const modalExiste = () =>
    new Promise<boolean>((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsText(res.data);
      reader.onload = () => {
        try {
          obj = JSON.parse(String(reader.result));
          resolve('theme' in obj && 'title' in obj && 'text' in obj);
        } catch (error) {
          reject(error);
        }
      };
      reader.onerror = (error) => {
        reject(error);
      };
    });
  if ((await modalExiste()) && obj instanceof Object) {
    useOverlayStore().showMessageDialog(obj.tema, obj.titulo, obj.texto);
    return true;
  }
  return false;
}

function resJsonModalFiltrar(res: AxiosResponse) {
  if (
    res.data instanceof Object &&
    'theme' in res.data &&
    'title' in res.data &&
    'text' in res.data
  ) {
    const serviceException = assertType<ServiceExceptionDTO>({ serviceExceptionDTO: res.data });
    if (serviceException.theme && serviceException.text && serviceException.title) {
      useOverlayStore().showMessageDialog(
        serviceException.theme,
        serviceException.title,
        serviceException.text
      );
      return true;
    }
  }
  return false;
}

export default vueAxios;
