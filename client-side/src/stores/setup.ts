import { assertType } from '@/helpers/typeAssertHelper';
import type { ClientSetupDTO } from '@/typings';
import { HttpStatusCode, type AxiosInstance } from 'axios';
import { defineStore } from 'pinia';
import { inject } from 'vue';

export const useSetupStore = defineStore('setup', {
  state: (): ClientSetupDTO => {
    return {
      ready: undefined,
      modalDelay: 1000,
      responseDelay: 500
    };
  },
  actions: {
    async fetchData() {
      const res = await inject<AxiosInstance>('axios')?.get('ClientSetup');
      if (res === undefined) this.ready = false;
      else if (res.status === HttpStatusCode.Ok) {
        const setup = assertType<ClientSetupDTO>({ clientSetupDTO: res.data });
        this.responseDelay = setup.responseDelay;
        this.modalDelay = setup.modalDelay;
        this.ready = setup.ready;
      }
    }
  }
});
