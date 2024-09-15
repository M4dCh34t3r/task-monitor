<script setup lang="ts">
import { ref } from 'vue';
import TbrWindow from '../toolbars/TbrWindow.vue';

const model = defineModel<boolean>({ default: false });
const btnJanelaShow = ref<boolean>(false);

function btnCloseClick() {
  setTimeout(() => {
    btnJanelaShow.value = false;
    model.value = false;
  }, 125);
}

defineProps<{
  btnToggleDisabled?: boolean;
  color?: string;
  title?: string;
}>();
</script>

<template>
  <v-dialog transition="dialog-bottom-transition" v-model="model">
    <v-card height="100%">
      <TbrWindow @btn-close-click="btnCloseClick" :title="title || '...'" />
      <slot />
    </v-card>
  </v-dialog>
</template>
