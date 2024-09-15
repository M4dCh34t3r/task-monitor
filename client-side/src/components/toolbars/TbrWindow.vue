<script setup lang="ts">
import { computed } from 'vue';

const btnSize = computed(() => (props.fullscreen ? 22 : 26));

const props = defineProps<{
  btnCloseDisabled?: boolean;
  color?: string;
  fullscreen?: boolean;
  title: string;
}>();
</script>

<template>
  <v-toolbar :height="fullscreen ? 32 : 38" :color="color ?? 'grey'">
    <v-toolbar-title :style="`font-size: ${fullscreen ? '16px' : '18px'}`">
      {{ title }}
    </v-toolbar-title>
    <v-btn
      icon="mdi-window-minimize"
      class="rounded ml-2"
      variant="tonal"
      :size="btnSize"
      disabled
    />
    <v-btn
      icon="mdi-window-maximize"
      class="rounded mx-2"
      variant="tonal"
      :size="btnSize"
      disabled
    />
    <v-btn
      @click="() => $emit('btnCloseClick')"
      :disabled="btnCloseDisabled"
      class="rounded mr-2"
      icon="mdi-close"
      :size="btnSize"
      variant="tonal"
    />
  </v-toolbar>
</template>
