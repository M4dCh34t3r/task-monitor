import { localRef } from '@/utils/refUtil';
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useConfigStore = defineStore('config', () => {
  const lastUserName = localRef<string | undefined>('last-user-name');
  const lastPassword = localRef<string | undefined>('last-password');
  const inDarkMode = localRef<boolean | undefined>('in-dark-mode');
  const inMobileMode = ref<boolean>(window.innerWidth <= 1024);

  return {
    lastUserName,
    lastPassword,
    inDarkMode,
    inMobileMode
  };
});
