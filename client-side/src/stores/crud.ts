import type { Project, Task } from '@/typings';
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useCrudStore = defineStore('crud', () => {
  const project = ref<Project>();
  const task = ref<Task>();

  return {
    project,
    task
  };
});
